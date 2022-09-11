using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
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
            var payload = context.GetInput<CoordinatorOrchestrationPayload>();
            if (payload == null)
            {
                throw new ArgumentException("Orchestration payload cannot be null.", nameof(context));
            }

            var tracker = GetTracker(context, payload.CaseId);
            try
            {
                var timeout = TimeSpan.FromSeconds(double.Parse(_configuration["CoordinatorOrchestratorTimeoutSecs"]));
                var deadline = context.CurrentUtcDateTime.Add(timeout);

                using var cts = new CancellationTokenSource();
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
                await tracker.RegisterFailed();
                _log.LogError(exception, "Error when running {CoordinatorOrchestratorName} orchestration with id \'{ContextInstanceId}\'.", nameof(CoordinatorOrchestrator), context.InstanceId);
                throw;
            }
        }

        private static async Task<List<TrackerDocument>> RunOrchestrator(IDurableOrchestrationContext context, ITracker tracker, CoordinatorOrchestrationPayload payload)
        {
            if (!payload.ForceRefresh && await tracker.IsAlreadyProcessed())
            {
                return await tracker.GetDocuments();
            }

            await tracker.Initialise(context.InstanceId);

            //TODO do we need this token exchange for cde?
            //var accessToken = await context.CallActivityAsync<string>(nameof(GetOnBehalfOfAccessToken), payload.AccessToken);

            var documents = await context.CallActivityAsync<CaseDocument[]>(
                nameof(GetCaseDocuments),
                new GetCaseDocumentsActivityPayload { CaseId = payload.CaseId, AccessToken = payload.AccessToken });

            if (documents.Length == 0)
            {
                await tracker.RegisterNoDocumentsFoundInCDE();
                return new List<TrackerDocument>();
            }

            await tracker.RegisterDocumentIds(documents.Select(item => item.DocumentId));

            var caseDocumentTasks = new List<Task>();
            for (var documentIndex = 0; documentIndex < documents.Length; documentIndex++)
            {
                caseDocumentTasks.Add(context.CallSubOrchestratorAsync(
                    nameof(CaseDocumentOrchestrator),
                    new CaseDocumentOrchestrationPayload
                    {
                        CaseId = payload.CaseId,
                        DocumentId = documents[documentIndex].DocumentId,
                        FileName = documents[documentIndex].FileName
                    }));
            }

            await Task.WhenAll(caseDocumentTasks.Select(BufferCall));

            if (await tracker.AllDocumentsFailed())
            {
                throw new CoordinatorOrchestrationException("All documents failed to process during orchestration.");
            }

            await tracker.RegisterCompleted();

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
                return;
            }
        }

        private ITracker GetTracker(IDurableOrchestrationContext context, int caseId)
        {
            var entityId = new EntityId(nameof(Tracker), caseId.ToString());
            return context.CreateEntityProxy<ITracker>(entityId);
        }
    }
}