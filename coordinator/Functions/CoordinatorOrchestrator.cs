using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using common.Wrappers;
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
        private readonly EndpointOptions _endpoints;
        private readonly IJsonConvertWrapper _jsonConvertWrapper;

        public CoordinatorOrchestrator(IOptions<EndpointOptions> endpointOptions, IJsonConvertWrapper jsonConvertWrapper)
        {
            _endpoints = endpointOptions.Value;
            _jsonConvertWrapper = jsonConvertWrapper;
        }

        [FunctionName("CoordinatorOrchestrator")]
        public async Task<List<TrackerDocument>> RunCaseOrchestrator(
        [OrchestrationTrigger] IDurableOrchestrationContext context, ILogger log)
        {
            var payload = context.GetInput<CoordinatorOrchestrationPayload>();

            if (payload == null)
            {
                throw new ArgumentException("Orchestration payload cannot be null.", nameof(CoordinatorOrchestrationPayload));
            }

            var tracker = GetTracker(context, payload.CaseId);

            //!arg.ForceRefresh &&
            if (await tracker.IsAlreadyProcessed())
            {
                return await tracker.GetDocuments();
            }

            tracker.Initialise(context.InstanceId);

            //TODO what are we meant to be calling here - core data api?
            //var cmsCaseDocumentDetails = await CallHttpAsync<List<CmsCaseDocumentDetails>>(context, HttpMethod.Get, _endpoints.CmsDocumentDetails);
            var cmsCaseDocumentDetails = new List<CmsCaseDocumentDetails> { new CmsCaseDocumentDetails { CaseId = 1, DocumentId = 1 }, new CmsCaseDocumentDetails { CaseId = 1, DocumentId = 2 }, new CmsCaseDocumentDetails { CaseId = 1, DocumentId = 3 } };
            tracker.RegisterDocumentIds(cmsCaseDocumentDetails.Select(item => item.DocumentId).ToList());

            var caseDocumentTasks = new List<Task<string>>();
            foreach (var caseDocumentDetails in cmsCaseDocumentDetails)
            {
                caseDocumentDetails.CaseId = payload.CaseId; //TODO do we need to set this?
                caseDocumentTasks.Add(context.CallSubOrchestratorAsync<string>(nameof(CaseDocumentOrchestrator), caseDocumentDetails));
            }

            await Task.WhenAll(caseDocumentTasks);

            tracker.RegisterCompleted();

            return await tracker.GetDocuments();
        }

        private async Task<T> CallHttpAsync<T>(IDurableOrchestrationContext context, HttpMethod httpMethod, string url)
        {
            var response = await context.CallHttpAsync(httpMethod, new Uri(url));
            return _jsonConvertWrapper.DeserializeObject<T>(response.Content);
        }

        private ITracker GetTracker(IDurableOrchestrationContext context, int caseId)
        {
            var entityId = new EntityId(nameof(Tracker), caseId.ToString());
            return context.CreateEntityProxy<ITracker>(entityId);
        }
    }
}