using System;
using System.Net.Http;
using System.Threading.Tasks;
using common.Wrappers;
using coordinator.Domain;
using coordinator.Domain.Requests;
using coordinator.Domain.Responses;
using coordinator.Domain.Tracker;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Extensions.Options;

namespace coordinator.Functions.SubOrchestrators
{
    public class CaseDocumentOrchestrator
    {
        private readonly EndpointOptions _endpoints;
        private readonly IJsonConvertWrapper _jsonConvertWrapper;

        public CaseDocumentOrchestrator(IOptions<EndpointOptions> endpointOptions, IJsonConvertWrapper jsonConvertWrapper)
        {
            _endpoints = endpointOptions.Value;
            _jsonConvertWrapper = jsonConvertWrapper;
        }

        [FunctionName("CaseDocumentOrchestrator")]
        public async Task RunDocumentOrchestrator([OrchestrationTrigger] IDurableOrchestrationContext context)
        {
            var caseDocumentDetails = context.GetInput<CmsCaseDocumentDetails>();

            var tracker = GetTracker(context, caseDocumentDetails.CaseId);

            //var response = await CallHttpAsync<GeneratePdfResponse>(
            //    context,
            //    HttpMethod.Post,
            //    _endpoints.GeneratePdf,
            //    new GeneratePdfRequest
            //    {
            //        CaseId = caseDocumentDetails.CaseId,
            //        DocumentId = caseDocumentDetails.DocumentId
            //    });

            var response = new GeneratePdfResponse { BlobName = $"{caseDocumentDetails.CaseId}/pdfs/{caseDocumentDetails.DocumentId}.pdf" };

            tracker.RegisterPdfBlobName(new RegisterPdfBlobNameArg { DocumentId = caseDocumentDetails.DocumentId, BlobName = response.BlobName });
        }

        private async Task<T> CallHttpAsync<T>(IDurableOrchestrationContext context, HttpMethod httpMethod, string url, object content)
        {
            var response = await context.CallHttpAsync(httpMethod, new Uri(url), _jsonConvertWrapper.SerializeObject(content));
            return _jsonConvertWrapper.DeserializeObject<T>(response.Content);
        }

        private ITracker GetTracker(IDurableOrchestrationContext context, int caseId)
        {
            var entityId = new EntityId(nameof(Tracker), caseId.ToString());
            return context.CreateEntityProxy<ITracker>(entityId);
        }
    }
}
