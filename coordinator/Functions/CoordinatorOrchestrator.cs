using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using common.Wrappers;
using coordinator.Clients;
using coordinator.Domain;
using coordinator.Domain.Tracker;
using coordinator.Functions.SubOrchestrators;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace coordinator.Functions
{
    public class CoordinatorOrchestrator
    {
        private readonly IOnBehalfOfTokenClient _onBehalfOfTokenClient;
        private readonly ICoreDataApiClient _coreDataApiClient;

        public CoordinatorOrchestrator(
            IOnBehalfOfTokenClient onBehalfOfTokenClient,
            ICoreDataApiClient coreDataApiClient)
        {
            _onBehalfOfTokenClient = onBehalfOfTokenClient;
            _coreDataApiClient = coreDataApiClient;
        }

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

                //TODO if exceptions are thrown below are they caught by the exception handler in CoordinatorStart - test
                var accessToken = await _onBehalfOfTokenClient.GetAccessToken(payload.AccessToken);
                var caseDetails = await _coreDataApiClient.GetCaseDetailsById(payload.CaseId, accessToken);

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

                await Task.WhenAll(caseDocumentTasks);

                await tracker.RegisterCompleted();

                return await tracker.GetDocuments();
            }
            catch (Exception exception)
            {
                log.LogError(exception, $"Error when running {nameof(CoordinatorOrchestrator)} orchestration");
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