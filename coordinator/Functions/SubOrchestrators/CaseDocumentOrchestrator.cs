using System;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using common.Wrappers;
using coordinator.Domain;
using coordinator.Domain.Requests;
using coordinator.Domain.Responses;
using coordinator.Domain.Tracker;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace coordinator.Functions.SubOrchestrators
{
    public class CaseDocumentOrchestrator
    {
        private readonly FunctionEndpointOptions _functionEndpoints;
        private readonly IJsonConvertWrapper _jsonConvertWrapper;
        private readonly ILogger<CaseDocumentOrchestrator> _log;

        public CaseDocumentOrchestrator(
            IOptions<FunctionEndpointOptions> functionEndpointOptions,
            IJsonConvertWrapper jsonConvertWrapper,
            ILogger<CaseDocumentOrchestrator> log)
        {
            _functionEndpoints = functionEndpointOptions.Value;
            _jsonConvertWrapper = jsonConvertWrapper;
            _log = log;
        }

        [FunctionName("CaseDocumentOrchestrator")]
        public async Task Run([OrchestrationTrigger] IDurableOrchestrationContext context)
        {
            try
            {
                var payload = context.GetInput<CaseDocumentOrchestrationPayload>();
                if (payload == null)
                {
                    throw new ArgumentException("Orchestration payload cannot be null.", nameof(CaseDocumentOrchestrationPayload));
                }

                var tracker = GetTracker(context, payload.CaseId);

                var response = await CallHttpAsync(context, payload.CaseId, payload.DocumentId);

                await tracker.RegisterPdfBlobName(new RegisterPdfBlobNameArg { DocumentId = payload.DocumentId, BlobName = response.BlobName });
            }
            catch (Exception exception)
            {
                _log.LogError(exception, $"Error when running {nameof(CaseDocumentOrchestrator)} orchestration.");
                throw;
            }
        }

        private async Task<GeneratePdfResponse> CallHttpAsync(IDurableOrchestrationContext context, int caseId, int documentId)
        {
            var request = _jsonConvertWrapper.SerializeObject(new GeneratePdfRequest { CaseId = caseId, DocumentId = documentId });
            //TODO add access token to this?
            var response = await context.CallHttpAsync(HttpMethod.Post, new Uri(_functionEndpoints.GeneratePdf), request);

            if(response.StatusCode != HttpStatusCode.OK)
            {
                throw new HttpRequestException($"Failed to generate pdf for document id '{documentId}'.");
            }

            return _jsonConvertWrapper.DeserializeObject<GeneratePdfResponse>(response.Content);
        }

        private ITracker GetTracker(IDurableOrchestrationContext context, int caseId)
        {
            var entityId = new EntityId(nameof(Tracker), caseId.ToString());
            return context.CreateEntityProxy<ITracker>(entityId);
        }
    }
}
