using System;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using common.Wrappers;
using coordinator.Domain;
using coordinator.Domain.Responses;
using coordinator.Domain.Tracker;
using coordinator.Functions.ActivityFunctions;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Extensions.Logging;

namespace coordinator.Functions.SubOrchestrators
{
    public class CaseDocumentOrchestrator
    {
        private readonly IJsonConvertWrapper _jsonConvertWrapper;
        private readonly ILogger<CaseDocumentOrchestrator> _log;

        public CaseDocumentOrchestrator(
            IJsonConvertWrapper jsonConvertWrapper,
            ILogger<CaseDocumentOrchestrator> log)
        {
            _jsonConvertWrapper = jsonConvertWrapper;
            _log = log;
        }

        [FunctionName("CaseDocumentOrchestrator")]
        public async Task Run([OrchestrationTrigger] IDurableOrchestrationContext context)
        {
            var payload = context.GetInput<CaseDocumentOrchestrationPayload>();
            if (payload == null)
            {
                throw new ArgumentException("Orchestration payload cannot be null.", nameof(context));
            }

            var tracker = GetTracker(context, payload.CaseId);
            var pdfGeneratorResponse = await CallPdfGeneratorAsync(context, payload, tracker);
            await CallTextExtractorAsync(context, payload, pdfGeneratorResponse.BlobName, tracker);
        }

        private async Task<GeneratePdfResponse> CallPdfGeneratorAsync(IDurableOrchestrationContext context, CaseDocumentOrchestrationPayload payload, ITracker tracker)
        {
            try
            {
                var response = await CallPdfGeneratorHttpAsync(context, payload, tracker);

                await tracker.RegisterPdfBlobName(new RegisterPdfBlobNameArg { DocumentId = payload.DocumentId, BlobName = response.BlobName });

                return response;
            }
            catch (Exception exception)
            {
                await tracker.RegisterUnexpectedPdfDocumentFailure(payload.DocumentId);
                _log.LogError(exception, $"Error when running {nameof(CaseDocumentOrchestrator)} orchestration.");
                throw;
            }
        }

        private async Task<GeneratePdfResponse> CallPdfGeneratorHttpAsync(IDurableOrchestrationContext context, CaseDocumentOrchestrationPayload payload, ITracker tracker)
        {
            var request = await context.CallActivityAsync<DurableHttpRequest>(
                nameof(CreateGeneratePdfHttpRequest),
                new CreateGeneratePdfHttpRequestActivityPayload { CaseId = payload.CaseId, DocumentId = payload.DocumentId, FileName = payload.FileName });
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

                request.Headers.TryGetValue("Authorization", out var tokenUsed);
                throw new HttpRequestException($"Failed to generate pdf for document id '{payload.DocumentId}'. Status code: {response.StatusCode}. Token Used: [{tokenUsed}].");
            }

            return _jsonConvertWrapper.DeserializeObject<GeneratePdfResponse>(response.Content);
        }

        private async Task CallTextExtractorAsync(IDurableOrchestrationContext context, CaseDocumentOrchestrationPayload payload, string blobName, ITracker tracker)
        {
            try
            {
                await CallTextExtractorHttpAsync(context, payload, blobName, tracker);

                await tracker.RegisterIndexed(payload.DocumentId);
            }
            catch (Exception exception)
            {
                await tracker.RegisterOcrAndIndexFailure(payload.DocumentId);
                
                _log.LogError(exception, $"Error when running {nameof(CaseDocumentOrchestrator)} orchestration.");
                throw;
            }
        }

        private async Task CallTextExtractorHttpAsync(IDurableOrchestrationContext context, CaseDocumentOrchestrationPayload payload, string blobName, ITracker tracker)
        {
            var request = await context.CallActivityAsync<DurableHttpRequest>(
                nameof(CreateTextExtractorHttpRequest),
                new CreateTextExtractorHttpRequestActivityPayload { CaseId = payload.CaseId, DocumentId = payload.DocumentId, BlobName = blobName });
            var response = await context.CallHttpAsync(request);

            if (response.StatusCode != HttpStatusCode.OK)
            {
                request.Headers.TryGetValue("Authorization", out var tokenUsed);
                throw new HttpRequestException($"Failed to ocr/index document with id '{payload.DocumentId}'. Status code: {response.StatusCode}. Token Used: [{tokenUsed}].");
            }
        }

        private ITracker GetTracker(IDurableOrchestrationContext context, int caseId)
        {
            var entityId = new EntityId(nameof(Tracker), caseId.ToString());
            return context.CreateEntityProxy<ITracker>(entityId);
        }
    }
}
