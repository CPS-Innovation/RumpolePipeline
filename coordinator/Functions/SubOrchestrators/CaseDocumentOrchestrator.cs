using System;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Common.Domain.Extensions;
using Common.Logging;
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
        
        public CaseDocumentOrchestrator(IJsonConvertWrapper jsonConvertWrapper, ILogger<CaseDocumentOrchestrator> log)
        {
            _jsonConvertWrapper = jsonConvertWrapper;
            _log = log;
        }

        [FunctionName("CaseDocumentOrchestrator")]
        public async Task Run([OrchestrationTrigger] IDurableOrchestrationContext context)
        {
            const string loggingName = $"{nameof(CaseDocumentOrchestrator)} - {nameof(Run)}";
            var payload = context.GetInput<CaseDocumentOrchestrationPayload>();
            if (payload == null)
                throw new ArgumentException("Orchestration payload cannot be null.", nameof(context));

            var log = context.CreateReplaySafeLogger(_log);

            log.LogMethodEntry(payload.CorrelationId, loggingName, payload.ToJson());
            
            log.LogMethodFlow(payload.CorrelationId, loggingName, $"Get the pipeline tracker for DocumentId: '{payload.DocumentId}'");
            var tracker = GetTracker(context, payload.CaseId, payload.CorrelationId, log);

            log.LogMethodFlow(payload.CorrelationId, loggingName, "Calling the PDF Generator for DocumentId: '{payload.DocumentId}'");
            var pdfGeneratorResponse = await CallPdfGeneratorAsync(context, payload, tracker, log);
            
            log.LogMethodFlow(payload.CorrelationId, loggingName, "Calling the Text Extractor for DocumentId: '{payload.DocumentId}'");
            await CallTextExtractorAsync(context, payload, pdfGeneratorResponse.BlobName, tracker, log);
            
            log.LogMethodExit(payload.CorrelationId, loggingName, string.Empty);
        }

        private async Task<GeneratePdfResponse> CallPdfGeneratorAsync(IDurableOrchestrationContext context, CaseDocumentOrchestrationPayload payload, ITracker tracker, ILogger log)
        {
            GeneratePdfResponse response = null;
            
            try
            {
                log.LogMethodEntry(payload.CorrelationId, nameof(CallPdfGeneratorAsync), payload.ToJson());
                
                response = await CallPdfGeneratorHttpAsync(context, payload, tracker, log);

                log.LogMethodFlow(payload.CorrelationId, nameof(CallPdfGeneratorAsync), $"Register PDF Blob Name in tracker - {response.BlobName} for DocumentId - {payload.DocumentId}");
                await tracker.RegisterPdfBlobName(new RegisterPdfBlobNameArg
                    {DocumentId = payload.DocumentId, BlobName = response.BlobName});
                
                return response;
            }
            catch (Exception exception)
            {
                log.LogMethodFlow(payload.CorrelationId, nameof(CallPdfGeneratorAsync), $"Register document processing failure in the tracker for DocumentId {payload.DocumentId}");
                await tracker.RegisterUnexpectedPdfDocumentFailure(payload.DocumentId);

                log.LogMethodError(payload.CorrelationId, nameof(CaseDocumentOrchestrator),
                    $"Error when running {nameof(CaseDocumentOrchestrator)} orchestration: {exception.Message}",
                    exception);

                throw;
            }
            finally
            {
                log.LogMethodExit(payload.CorrelationId, nameof(CallPdfGeneratorAsync), response.ToJson());
            }
        }

        private async Task<GeneratePdfResponse> CallPdfGeneratorHttpAsync(IDurableOrchestrationContext context, CaseDocumentOrchestrationPayload payload, ITracker tracker, ILogger log)
        {
            log.LogMethodEntry(payload.CorrelationId, nameof(CallPdfGeneratorHttpAsync), payload.ToJson());
            
            var request = await context.CallActivityAsync<DurableHttpRequest>(
                nameof(CreateGeneratePdfHttpRequest),
                new CreateGeneratePdfHttpRequestActivityPayload { CaseId = payload.CaseId, DocumentId = payload.DocumentId, FileName = payload.FileName, CorrelationId = payload.CorrelationId });
            var response = await context.CallHttpAsync(request);

            if (response.StatusCode != HttpStatusCode.OK)
            {
                switch (response.StatusCode)
                {
                    case HttpStatusCode.NotFound:
                        log.LogMethodFlow(payload.CorrelationId, nameof(CallPdfGeneratorHttpAsync), $"Registering Document Not Found in CDE in the tracker for documentId: {payload.DocumentId}");
                        await tracker.RegisterDocumentNotFoundInCde(payload.DocumentId);
                        break;
                    case HttpStatusCode.NotImplemented:
                        log.LogMethodFlow(payload.CorrelationId, nameof(CallPdfGeneratorHttpAsync), $"Registering Document Conversion failure in the tracker for documentId: {payload.DocumentId}");
                        await tracker.RegisterUnableToConvertDocumentToPdf(payload.DocumentId);
                        break;
                }

                request.Headers.TryGetValue("Authorization", out var tokenUsed);
                throw new HttpRequestException($"Failed to generate pdf for document id '{payload.DocumentId}'. Status code: {response.StatusCode}. Token Used: [{tokenUsed}]. CorrelationId: {payload.CorrelationId}");
            }

            var result = _jsonConvertWrapper.DeserializeObject<GeneratePdfResponse>(response.Content);
            
            log.LogMethodExit(payload.CorrelationId, nameof(CallPdfGeneratorHttpAsync), result.ToJson());
            return result;
        }

        private async Task CallTextExtractorAsync(IDurableOrchestrationContext context, CaseDocumentOrchestrationPayload payload, string blobName, ITracker tracker, ILogger log)
        {
            log.LogMethodEntry(payload.CorrelationId, nameof(CallTextExtractorAsync), payload.ToJson());

            try
            {
                log.LogMethodFlow(payload.CorrelationId, nameof(CallPdfGeneratorHttpAsync), $"Calling the text extractor for: {payload.DocumentId}");
                await CallTextExtractorHttpAsync(context, payload, blobName, log);

                log.LogMethodFlow(payload.CorrelationId, nameof(CallPdfGeneratorHttpAsync), $"Registering documentId: {payload.DocumentId} as successfully indexed in the tracker");
                await tracker.RegisterIndexed(payload.DocumentId);
            }
            catch (Exception exception)
            {
                log.LogMethodFlow(payload.CorrelationId, nameof(CallPdfGeneratorHttpAsync), $"Registering the failure of documentId: {payload.DocumentId} to be indexed in the tracker");
                await tracker.RegisterOcrAndIndexFailure(payload.DocumentId);

                log.LogMethodError(payload.CorrelationId, nameof(CallTextExtractorAsync), $"Error when running {nameof(CaseDocumentOrchestrator)} orchestration: {exception.Message}", exception);
                throw;
            }
            finally
            {
                log.LogMethodExit(payload.CorrelationId, nameof(CallTextExtractorAsync), string.Empty);
            }
        }

        private async Task CallTextExtractorHttpAsync(IDurableOrchestrationContext context, CaseDocumentOrchestrationPayload payload, string blobName, ILogger log)
        {
            log.LogMethodEntry(payload.CorrelationId, nameof(CallTextExtractorHttpAsync), payload.ToJson());
            
            var request = await context.CallActivityAsync<DurableHttpRequest>(
                nameof(CreateTextExtractorHttpRequest),
                new CreateTextExtractorHttpRequestActivityPayload { CaseId = payload.CaseId, DocumentId = payload.DocumentId, BlobName = blobName, CorrelationId = payload.CorrelationId });
            var response = await context.CallHttpAsync(request);

            if (response.StatusCode != HttpStatusCode.OK)
            {
                request.Headers.TryGetValue("Authorization", out var tokenUsed);
                throw new HttpRequestException($"Failed to ocr/index document with id '{payload.DocumentId}'. Status code: {response.StatusCode}. Token Used: [{tokenUsed}]. CorrelationId: {payload.CorrelationId}");
            }
            
            log.LogMethodExit(payload.CorrelationId, nameof(CallTextExtractorHttpAsync), string.Empty);
        }

        private ITracker GetTracker(IDurableOrchestrationContext context, int caseId, Guid correlationId, ILogger log)
        {
            log.LogMethodEntry(correlationId, nameof(GetTracker), $"CaseId: {caseId.ToString()}");
            
            var entityId = new EntityId(nameof(Tracker), caseId.ToString());
            
            log.LogMethodExit(correlationId, nameof(GetTracker), string.Empty);
            return context.CreateEntityProxy<ITracker>(entityId);
        }
    }
}
