using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Common.Constants;
using Common.Domain.DocumentExtraction;
using Common.Domain.Extensions;
using Common.Domain.Responses;
using Common.Logging;
using Common.Wrappers;
using coordinator.Domain;
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
        private readonly IJsonConvertWrapper _jsonConvertWrapper;

        public CoordinatorOrchestrator(IConfiguration configuration, ILogger<CoordinatorOrchestrator> log, IJsonConvertWrapper jsonConvertWrapper)
        {
            _configuration = configuration;
            _log = log;
            _jsonConvertWrapper = jsonConvertWrapper;
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
            var tracker = GetTracker(context, payload.CaseUrn, payload.CaseId, payload.CorrelationId, log);
            
            try
            {
                var timeout = TimeSpan.FromSeconds(double.Parse(_configuration[ConfigKeys.CoordinatorKeys.CoordinatorOrchestratorTimeoutSecs]));
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

            var documents = await RetrieveDocuments(context, tracker, loggingName, log, payload);
            if (documents.Length == 0)
                return new List<TrackerDocument>();

            await RegisterDocuments(tracker, loggingName, log, payload, documents);

            await EvaluateDocuments(context, tracker, loggingName, log, payload, documents);
            
            log.LogMethodFlow(payload.CorrelationId, loggingName, $"Now process each document for case {payload.CaseId}");
            var caseDocumentTasks = documents.Select(t => context.CallSubOrchestratorAsync(nameof(CaseDocumentOrchestrator), 
                    new CaseDocumentOrchestrationPayload(payload.CaseUrn, payload.CaseId, t.CmsDocType.Name, t.DocumentId, t.VersionId, t.FileName, payload.UpstreamToken, payload.CorrelationId)))
                .ToList();

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

        private ITracker GetTracker(IDurableOrchestrationContext context, string caseUrn, long caseId, Guid correlationId, ILogger safeLoggerInstance)
        {
            safeLoggerInstance.LogMethodEntry(correlationId, nameof(GetTracker), $"CaseUrn: {caseUrn}, CaseId: {caseId.ToString()}");

            var entityKey = string.Concat(caseUrn, "-", caseId.ToString());
            var entityId = new EntityId(nameof(Tracker), entityKey);
            var result = context.CreateEntityProxy<ITracker>(entityId);
            
            safeLoggerInstance.LogMethodExit(correlationId, nameof(GetTracker), "n/a");
            return result;
        }

        private static async Task<CaseDocument[]> RetrieveDocuments(IDurableOrchestrationContext context, ITracker tracker, string nameToLog, ILogger safeLogger, CoordinatorOrchestrationPayload payload)
        {
            safeLogger.LogMethodFlow(payload.CorrelationId, nameToLog, $"Getting list of documents for case {payload.CaseId}");
            
            //do we need this token exchange for cde, maybe not, perhaps some auth config for DDEI?
            //log.LogMethodFlow(currentCorrelationId, loggingName, "Get CDE access token");
            //var accessToken = await context.CallActivityAsync<string>(nameof(GetOnBehalfOfAccessToken), payload.AccessToken);
            
            safeLogger.LogMethodFlow(payload.CorrelationId, nameToLog, $"Getting list of documents for case {payload.CaseId}");
            var documents = await context.CallActivityAsync<CaseDocument[]>(
                nameof(GetCaseDocuments),
                new GetCaseDocumentsActivityPayload(payload.CaseUrn, payload.CaseId, payload.UpstreamToken, payload.CorrelationId));

            if (documents.Length != 0) return documents;
            
            safeLogger.LogMethodFlow(payload.CorrelationId, nameToLog, $"No documents found, register this in the tracker for case {payload.CaseId}");
            await tracker.RegisterNoDocumentsFoundInCDE();
            return documents;
        }

        private static async Task RegisterDocuments(ITracker tracker, string nameToLog, ILogger safeLogger, BasePipelinePayload payload, IEnumerable<CaseDocument> documents)
        {
            safeLogger.LogMethodFlow(payload.CorrelationId, nameToLog, $"Documents found, register document Ids in tracker for case {payload.CaseId}");
            await tracker.RegisterDocumentIds(documents.Select(item => item.DocumentId));
        }

        private async Task EvaluateDocuments(IDurableOrchestrationContext context, ITracker tracker, string nameToLog, ILogger safeLogger, CoordinatorOrchestrationPayload payload, IEnumerable<CaseDocument> documents)
        {
            safeLogger.LogMethodFlow(payload.CorrelationId, nameToLog,
                $"Beginning evaluation of existing polaris documents against list of incoming documents for case {payload.CaseId}");
            var evaluateExistingDocumentsRequest = await context.CallActivityAsync<DurableHttpRequest>(nameof(CreateEvaluateExistingDocumentsHttpRequest),
                new CreateEvaluateExistingDocumentsHttpRequestActivityPayload(payload.CaseUrn, payload.CaseId, documents.ToList(), payload.CorrelationId));
            var response = await context.CallHttpAsync(evaluateExistingDocumentsRequest);

            if (response.StatusCode != HttpStatusCode.OK)
            {
                safeLogger.LogMethodFlow(payload.CorrelationId, nameof(RunOrchestrator), "Could not evaluate existing documents");
                await tracker.RegisterUnexpectedExistingDocumentsEvaluationFailure();
                return;
            }
            
            var evaluateExistingDocumentsResult = _jsonConvertWrapper.DeserializeObject<List<EvaluateDocumentResponse>>(response.Content);

            var documentsToRemove = evaluateExistingDocumentsResult.Select(x => x.UpdateSearchIndex).Count();

            safeLogger.LogMethodFlow(payload.CorrelationId, nameToLog,
                $"Evaluation of existing polaris documents completed, {documentsToRemove} documents to remove from the search index for {payload.CaseId}");
            if (documentsToRemove > 0)
            {
                var existingDocumentTasks = (from result in evaluateExistingDocumentsResult
                    where result.UpdateSearchIndex
                    select context.CallActivityAsync(nameof(CreateUpdateSearchIndexHttpRequest),
                        new CreateUpdateSearchIndexHttpRequestActivityPayload(payload.CaseUrn, payload.CaseId, result.DocumentId, payload.CorrelationId))).ToList();

                await Task.WhenAll(existingDocumentTasks.Select(BufferCall));
            }
        }
    }
}