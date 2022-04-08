using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using coordinator.Domain;
using coordinator.Domain.CoreDataApi;
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
            try
            {
                var payload = context.GetInput<CoordinatorOrchestrationPayload>();
                if (payload == null)
                {
                    throw new ArgumentException("Orchestration payload cannot be null.", nameof(CoordinatorOrchestrationPayload));
                }

                var tracker = GetTracker(context, payload.CaseId);

                if (!payload.ForceRefresh && await tracker.IsAlreadyProcessed())
                {
                    return await tracker.GetDocuments();
                }

                await tracker.Initialise(context.InstanceId);

                var accessToken = await context.CallActivityAsync<string>(nameof(GetOnBehalfOfAccessToken), payload.AccessToken);
                var documents = await context.CallActivityAsync<List<Document>>(
                    nameof(GetCaseDocumentsById),
                    new GetCaseDocumentsByIdActivityPayload { CaseId = payload.CaseId, AccessToken = accessToken });

                if (documents.Count() == 0)
                {
                    await tracker.RegisterCompleted();
                    return new List<TrackerDocument>();
                }

                var documentIds = documents.Select(item => item.Id);
                await tracker.RegisterDocumentIds(documentIds);

                var caseDocumentTasks = documentIds.Select(id =>
                    context.CallSubOrchestratorAsync(
                            nameof(CaseDocumentOrchestrator),
                            new CaseDocumentOrchestrationPayload { CaseId = payload.CaseId, DocumentId = id }));

                await Task.WhenAll(caseDocumentTasks.Select(t => BufferCall(t)));

                await tracker.RegisterCompleted();

                return await tracker.GetDocuments();
            }
            catch (Exception exception)
            {
                _log.LogError(exception, $"Error when running {nameof(CoordinatorOrchestrator)} orchestration.");
                throw;
            }
        }

        //TODO test
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