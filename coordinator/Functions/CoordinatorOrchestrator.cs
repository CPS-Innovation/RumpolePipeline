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
                var caseDetails = await context.CallActivityAsync<CaseDetails>(
                    nameof(GetCaseDetailsById),
                    new GetCaseDetailsByIdActivityPayload { CaseId = payload.CaseId, AccessToken = accessToken });

                var documentIds = caseDetails.Documents.Select(item => item.Id);
                await tracker.RegisterDocumentIds(documentIds);

                var caseDocumentTasks = documentIds.Select(id =>
                    context.CallSubOrchestratorAsync(
                            nameof(CaseDocumentOrchestrator),
                            new CaseDocumentOrchestrationPayload { CaseId = payload.CaseId, DocumentId = id }));

                //TODO what happens when one task fails?
                await Task.WhenAll(caseDocumentTasks);

                await tracker.RegisterCompleted();

                return await tracker.GetDocuments();
            }
            catch (Exception exception)
            {
                //await tracker.RegisterError();
                _log.LogError(exception, $"Error when running {nameof(CoordinatorOrchestrator)} orchestration.");
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