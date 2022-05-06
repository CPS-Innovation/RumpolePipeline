using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using coordinator.Domain;
using coordinator.Domain.DocumentExtraction;
using coordinator.Domain.Exceptions;
using coordinator.Domain.Tracker;
using coordinator.Functions.ActivityFunctions;
using coordinator.Functions.SubOrchestrators;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Extensions.Logging;

namespace coordinator.Functions
{
    public class CoordinatorOrchestrator
    {
        private readonly ILogger<CoordinatorOrchestrator> _log;

        public CoordinatorOrchestrator(ILogger<CoordinatorOrchestrator> log)
        {
           _log = log;
        }

        [FunctionName("CoordinatorOrchestrator")]
        public async Task<List<TrackerDocument>> Run(
        [OrchestrationTrigger] IDurableOrchestrationContext context)
        {
            var payload = context.GetInput<CoordinatorOrchestrationPayload>();
            if (payload == null)
            {
                throw new ArgumentException("Orchestration payload cannot be null.", nameof(CoordinatorOrchestrationPayload));
            }

            var tracker = GetTracker(context, payload.CaseId);
            try
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
                    new GetCaseDocumentsActivityPayload { CaseId = payload.CaseId, AccessToken = "accessToken" });

                if (documents.Count() == 0)
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

                await Task.WhenAll(caseDocumentTasks.Select(t => BufferCall(t)));

                if (await tracker.AllDocumentsFailed())
                {
                    throw new CoordinatorOrchestrationException("All documents failed to process during orchestration.");
                }

                await tracker.RegisterCompleted();

                return await tracker.GetDocuments();
            }
            catch (Exception exception)
            {
                await tracker.RegisterFailed();
                _log.LogError(exception, $"Error when running {nameof(CoordinatorOrchestrator)} orchestration.");
                throw;
            }
        }

        private async Task BufferCall(Task task)
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