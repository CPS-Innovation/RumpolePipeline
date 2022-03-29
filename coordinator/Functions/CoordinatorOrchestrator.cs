using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using coordinator.Clients;
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
        [FunctionName("CoordinatorOrchestrator")]
        public async Task<List<TrackerDocument>> RunCaseOrchestrator(
        [OrchestrationTrigger] IDurableOrchestrationContext context, ILogger log)
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
                var caseDetails = await context.CallActivityAsync<CaseDetails>(
                    nameof(GetCaseDetailsById),
                    new GetCaseDetailsByIdActivityPayload { CaseId = payload.CaseId, AccessToken = payload.AccessToken });

                var caseDocumentTasks = new List<Task<string>>();
                var documentIds = caseDetails.Documents.Select(item =>
                {
                    caseDocumentTasks.Add(
                        context.CallSubOrchestratorAsync<string>(
                            nameof(CaseDocumentOrchestrator),
                            new CaseDocumentOrchestrationPayload { CaseId = payload.CaseId, DocumentId = item.Id }));
                    return item.Id;
                });

                await tracker.RegisterDocumentIds(documentIds);

                //TODO what happens when one task fails?
                await Task.WhenAll(caseDocumentTasks);

                await tracker.RegisterCompleted();

                return await tracker.GetDocuments();
            }
            catch (Exception exception)
            {
                //await tracker.RegisterError();
                log.LogError(exception, $"Error when running {nameof(CoordinatorOrchestrator)} orchestration.");
                throw;
            }
        }

        private ITracker GetTracker(IDurableOrchestrationContext context, int caseId)
        {
            var entityId = new EntityId(nameof(Tracker), caseId.ToString());
            return context.CreateEntityProxy<ITracker>(entityId);
        }
    }
}