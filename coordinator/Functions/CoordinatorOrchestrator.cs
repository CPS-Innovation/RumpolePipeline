using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Common.Domain.Extensions;
using Common.Logging;
using coordinator.Domain;
using coordinator.Domain.DocumentExtraction;
using coordinator.Domain.Exceptions;
using coordinator.Domain.Tracker;
using coordinator.Functions.ActivityFunctions;
using coordinator.Functions.SubOrchestrators;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace coordinator.Functions
{
    public class CoordinatorOrchestrator
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<CoordinatorOrchestrator> _log;

        public CoordinatorOrchestrator(IConfiguration configuration, ILogger<CoordinatorOrchestrator> log)
        {
            _configuration = configuration;
            _log = log;
        }

        [FunctionName("CoordinatorOrchestrator")]
        public async Task<List<TrackerDocument>> Run(
        [OrchestrationTrigger] IDurableOrchestrationContext context)
        {
            const string loggingName = $"{nameof(CoordinatorOrchestrator)} - {nameof(Run)}";
            var payload = context.GetInput<CoordinatorOrchestrationPayload>();
            if (payload == null)
                throw new ArgumentException("Orchestration payload cannot be null.", nameof(context));

            var log = context.CreateReplaySafeLogger(_log);
            var currentCaseId = payload.CaseId;
            
            log.LogMethodEntry(payload.CorrelationId, loggingName, payload.ToJson());
            
            log.LogMethodFlow(payload.CorrelationId, loggingName, $"Retrieve tracker for case {currentCaseId}");
            var tracker = GetTracker(context, payload.CaseId, payload.CorrelationId, log);
            
            try
            {
                var timeout = TimeSpan.FromSeconds(double.Parse(_configuration["CoordinatorOrchestratorTimeoutSecs"]));
                var deadline = context.CurrentUtcDateTime.Add(timeout);

                using var cts = new CancellationTokenSource();
                log.LogMethodFlow(payload.CorrelationId, loggingName, $"Run main orchestration for case {currentCaseId}");
                var orchestratorTask = RunOrchestrator(context, tracker, payload);
                var timeoutTask = context.CreateTimer(deadline, cts.Token);

                var result = await Task.WhenAny(orchestratorTask, timeoutTask);
                if (result == orchestratorTask)
                {
                    // success case
                    cts.Cancel();
                    return await orchestratorTask;
                }

                // timeout case
                throw new TimeoutException($"Orchestration with id '{context.InstanceId}' timed out.");
            }
            catch (Exception exception)
            {
                log.LogMethodFlow(payload.CorrelationId, loggingName, "Registering Failure in the tracker");
                await tracker.RegisterFailed();
                log.LogMethodError(payload.CorrelationId, loggingName, $"Error when running {nameof(CoordinatorOrchestrator)} orchestration with id '{context.InstanceId}'", exception);
                throw;
            }
            finally
            {
                log.LogMethodExit(payload.CorrelationId, loggingName, string.Empty);
            }
        }

        private async Task<List<TrackerDocument>> RunOrchestrator(IDurableOrchestrationContext context, ITracker tracker, CoordinatorOrchestrationPayload payload)
        {
            const string loggingName = nameof(RunOrchestrator);
            var log = context.CreateReplaySafeLogger(_log);
            
            log.LogMethodEntry(payload.CorrelationId, loggingName, payload.ToJson());
            
            if (!payload.ForceRefresh && await tracker.IsAlreadyProcessed())
            {
                log.LogMethodFlow(payload.CorrelationId, loggingName, $"Tracker has already finished processing and a 'force refresh' has not been issued - returning documents - {context.InstanceId}");
                return await tracker.GetDocuments();
            }

            log.LogMethodFlow(payload.CorrelationId, loggingName, $"Initialising tracker for {context.InstanceId}");
            await tracker.Initialise(context.InstanceId);

            //TODO do we need this token exchange for cde?
            //log.LogMethodFlow(currentCorrelationId, loggingName, "Get CDE access token");
            //var accessToken = await context.CallActivityAsync<string>(nameof(GetOnBehalfOfAccessToken), payload.AccessToken);

            log.LogMethodFlow(payload.CorrelationId, loggingName, $"Getting list of documents for case {payload.CaseId}");
            var documents = await context.CallActivityAsync<CaseDocument[]>(
                nameof(GetCaseDocuments),
                new GetCaseDocumentsActivityPayload { CaseId = payload.CaseId, AccessToken = payload.AccessToken, CorrelationId = payload.CorrelationId });

            if (documents.Length == 0)
            {
                log.LogMethodFlow(payload.CorrelationId, loggingName, $"No documents found, register this in the tracker for case {payload.CaseId}");
                await tracker.RegisterNoDocumentsFoundInCde();
                return new List<TrackerDocument>();
            }

            log.LogMethodFlow(payload.CorrelationId, loggingName, $"Documents found, register document Ids in tracker for case {payload.CaseId}");
            await tracker.RegisterDocumentIds(documents.Select(item => item.DocumentId));

            var caseDocumentTasks = new List<Task>();
            log.LogMethodFlow(payload.CorrelationId, loggingName, $"Now process each document for case {payload.CaseId}");
            for (var documentIndex = 0; documentIndex < documents.Length; documentIndex++)
            {
                caseDocumentTasks.Add(context.CallSubOrchestratorAsync(
                    nameof(CaseDocumentOrchestrator),
                    new CaseDocumentOrchestrationPayload
                    {
                        CaseId = payload.CaseId,
                        DocumentId = documents[documentIndex].DocumentId,
                        FileName = documents[documentIndex].FileName,
                        CorrelationId = payload.CorrelationId
                    }));
            }

            await Task.WhenAll(caseDocumentTasks.Select(BufferCall));

            if (await tracker.AllDocumentsFailed())
                throw new CoordinatorOrchestrationException("All documents failed to process during orchestration.");
            
            log.LogMethodFlow(payload.CorrelationId, loggingName, $"All documents processed successfully for case {payload.CaseId}");
            await tracker.RegisterCompleted();

            log.LogMethodExit(payload.CorrelationId, loggingName, "Returning documents");
            return await tracker.GetDocuments();
        }

        private static async Task BufferCall(Task task)
        {
            try
            {
                await task;
            }
            catch (Exception)
            {
                // ReSharper disable once RedundantJumpStatement
                return;
            }
        }

        private ITracker GetTracker(IDurableOrchestrationContext context, int caseId, Guid correlationId, ILogger safeLoggerInstance)
        {
            safeLoggerInstance.LogMethodEntry(correlationId, nameof(GetTracker), $"CaseId: {caseId.ToString()}");
            
            var entityId = new EntityId(nameof(Tracker), caseId.ToString());
            var result = context.CreateEntityProxy<ITracker>(entityId);
            
            safeLoggerInstance.LogMethodExit(correlationId, nameof(GetTracker), "n/a");
            return result;
        }
    }
}