using System;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using common.Wrappers;
using coordinator.Domain;
using coordinator.Domain.Requests;
using coordinator.Domain.Responses;
using coordinator.Domain.Tracker;
using coordinator.Factories;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace coordinator.Functions.SubOrchestrators
{
    public class CaseDocumentOrchestrator
    {
        private readonly IGeneratePdfHttpRequestFactory _generatePdfHttpRequestFactory;
        private readonly FunctionEndpointOptions _functionEndpoints;
        private readonly IJsonConvertWrapper _jsonConvertWrapper;
        private readonly ILogger<CaseDocumentOrchestrator> _log;

        public CaseDocumentOrchestrator(
            IGeneratePdfHttpRequestFactory generatePdfHttpRequestFactory,
            IOptions<FunctionEndpointOptions> functionEndpointOptions,
            IJsonConvertWrapper jsonConvertWrapper,
            ILogger<CaseDocumentOrchestrator> log)
        {
            _generatePdfHttpRequestFactory = generatePdfHttpRequestFactory;
            _functionEndpoints = functionEndpointOptions.Value;
            _jsonConvertWrapper = jsonConvertWrapper;
            _log = log;
        }

        [FunctionName("CaseDocumentOrchestrator")]
        public async Task Run([OrchestrationTrigger] IDurableOrchestrationContext context)
        {
            var payload = context.GetInput<CaseDocumentOrchestrationPayload>();
            if (payload == null)
            {
                throw new ArgumentException("Orchestration payload cannot be null.", nameof(CaseDocumentOrchestrationPayload));
            }

            var tracker = GetTracker(context, payload.CaseId);
            try
            {
                var response = await CallHttpAsync(context, payload, tracker);

                await tracker.RegisterPdfBlobName(new RegisterPdfBlobNameArg { DocumentId = payload.DocumentId, BlobName = response.BlobName });
            }
            catch (Exception exception)
            {
                await tracker.RegisterUnexpectedDocumentFailure(payload.DocumentId);
                _log.LogError(exception, $"Error when running {nameof(CaseDocumentOrchestrator)} orchestration.");
                throw;
            }
        }

        private async Task<GeneratePdfResponse> CallHttpAsync(IDurableOrchestrationContext context, CaseDocumentOrchestrationPayload payload, ITracker tracker)
        {
            var request = await _generatePdfHttpRequestFactory.Create(payload.CaseId, payload.DocumentId, payload.FileName, new Uri(_functionEndpoints.GeneratePdf));
            var response = await context.CallHttpAsync(request);

            if (response.StatusCode != HttpStatusCode.OK)
            {
                switch (response.StatusCode)
                {
                    case HttpStatusCode.NotFound:
                        await tracker.RegisterDocumentNotFoundInCDE(payload.DocumentId);
                        break;
                    case HttpStatusCode.NotImplemented:
                        await tracker.RegisterUnableToConvertDocumentToPdf(payload.DocumentId);
                        break;
                }
                
                throw new HttpRequestException($"Failed to generate pdf for document id '{payload.DocumentId}'. Status code: {response.StatusCode}.");
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
