using System;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Common.Constants;
using Common.Domain.Extensions;
using Common.Domain.Responses;
using Common.Logging;
using Common.Wrappers;
using coordinator.Domain;
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
            var tracker = GetTracker(context, payload.CaseUrn, payload.CaseId, payload.CorrelationId, log);

            log.LogMethodFlow(payload.CorrelationId, loggingName, $"Calling the PDF Generator for DocumentId: '{payload.DocumentId}', FileName: '{payload.FileName}'");
            var pdfGeneratorResponse = await CallPdfGeneratorAsync(context, payload, tracker, log);

            if (!pdfGeneratorResponse.AlreadyProcessed)
            {
                if (pdfGeneratorResponse.UpdateSearchIndex)
                {
                    log.LogMethodFlow(payload.CorrelationId, loggingName, $"Updating the search index for DocumentId: '{payload.DocumentId}'");
                    await CallUpdateSearchIndexAsync(context, payload, tracker, log);
                }
                
                log.LogMethodFlow(payload.CorrelationId, loggingName, $"Calling the Text Extractor for DocumentId: '{payload.DocumentId}', FileName: '{payload.FileName}'");
                await CallTextExtractorAsync(context, payload, pdfGeneratorResponse.BlobName, tracker, log);    
            }

            log.LogMethodExit(payload.CorrelationId, loggingName, string.Empty);
        }
        
        private async Task<GeneratePdfResponse> CallPdfGeneratorAsync(IDurableOrchestrationContext context, CaseDocumentOrchestrationPayload payload, ITracker tracker, ILogger log)
        {
            GeneratePdfResponse response = null;
            
            try
            {
                log.LogMethodEntry(payload.CorrelationId, nameof(CallPdfGeneratorAsync), payload.ToJson());
                
                response = await CallPdfGeneratorHttpAsync(context, payload, tracker, log);

                if (response.AlreadyProcessed)
                {
                    await tracker.RegisterBlobAlreadyProcessed(new RegisterPdfBlobNameArg { DocumentId = payload.DocumentId, BlobName = response.BlobName });
                }

                else
                {
                    await tracker.RegisterPdfBlobName(new RegisterPdfBlobNameArg { DocumentId = payload.DocumentId, BlobName = response.BlobName });
                }

                return response;
            }
            catch (Exception exception)
            {
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
                new CreateGeneratePdfHttpRequestActivityPayload(payload.CaseUrn, payload.CaseId, payload.DocumentCategory, payload.DocumentId, payload.FileName, payload.VersionId, payload.UpstreamToken, payload.CorrelationId));
            var response = await context.CallHttpAsync(request);

            switch (response.StatusCode)
            {
                case HttpStatusCode.OK:
                    return _jsonConvertWrapper.DeserializeObject<GeneratePdfResponse>(response.Content);
                case HttpStatusCode.NotFound:
                    await tracker.RegisterDocumentNotFoundInDDEI(payload.DocumentId);
                    break;
                case HttpStatusCode.NotImplemented:
                    await tracker.RegisterUnableToConvertDocumentToPdf(payload.DocumentId);
                    break;
            }
            
            throw new HttpRequestException($"Failed to generate pdf for document id '{payload.DocumentId}'. Status code: {response.StatusCode}.");
        }
        
        private async Task CallUpdateSearchIndexAsync(IDurableOrchestrationContext context, CaseDocumentOrchestrationPayload payload, ITracker tracker, ILogger log)
        {
            try
            {
                log.LogMethodEntry(payload.CorrelationId, nameof(CallUpdateSearchIndexAsync), payload.ToJson());
                
                await CallUpdateSearchIndexHttpAsync(context, payload, tracker, log);
                await tracker.RegisterDocumentRemovedFromSearchIndex(payload.DocumentId);
            }
            catch (Exception exception)
            {
                log.LogMethodFlow(payload.CorrelationId, nameof(CallUpdateSearchIndexAsync), $"Register search index removal failure in the tracker for DocumentId {payload.DocumentId}");
                
                log.LogMethodError(payload.CorrelationId, nameof(CaseDocumentOrchestrator),
                    $"Error when running {nameof(CaseDocumentOrchestrator)} orchestration: {exception.Message}",
                    exception);

                //do not throw, log only
                //the search index could have been updated earlier in the Pipeline flow by the global documents evaluation process
                //the ocr and search index update will handle the restoration of search index data for the latest version of the document
            }
            finally
            {
                log.LogMethodExit(payload.CorrelationId, nameof(CallUpdateSearchIndexAsync), string.Empty);
            }
        }
        
        private async Task CallUpdateSearchIndexHttpAsync(IDurableOrchestrationContext context, CaseDocumentOrchestrationPayload payload, ITracker tracker, ILogger log)
        {
            log.LogMethodEntry(payload.CorrelationId, nameof(CallUpdateSearchIndexHttpAsync), payload.ToJson());
            
            var request = await context.CallActivityAsync<DurableHttpRequest>(
                nameof(CreateUpdateSearchIndexHttpRequest),
                new CreateUpdateSearchIndexHttpRequestActivityPayload(payload.CaseUrn, payload.CaseId, payload.DocumentId, payload.CorrelationId));
            var response = await context.CallHttpAsync(request);

            if (response.StatusCode != HttpStatusCode.OK)
            {
                switch (response.StatusCode)
                {
                    case HttpStatusCode.NotFound:
                        log.LogMethodFlow(payload.CorrelationId, nameof(CallUpdateSearchIndexHttpAsync), $"Registering Document Not Found in DDEI in the tracker for, caseId: {payload.CaseId}, documentId: {payload.DocumentId}");
                        await tracker.RegisterDocumentNotFoundInDDEI(payload.DocumentId);
                        break;
                    case HttpStatusCode.NotImplemented:
                        log.LogMethodFlow(payload.CorrelationId, nameof(CallUpdateSearchIndexHttpAsync), $"Registering Search Index Removal failure in the tracker for caseId: {payload.CaseId}, documentId: {payload.DocumentId}");
                        await tracker.RegisterUnableToUpdateSearchIndex(payload.DocumentId);
                        break;
                }

                request.Headers.TryGetValue(HttpHeaderKeys.Authorization, out var tokenUsed);
                throw new HttpRequestException($"Failed to update search index for caseId: '{payload.CaseId}' and document id '{payload.DocumentId}'. Status code: {response.StatusCode}. Token Used: [{tokenUsed}]. CorrelationId: {payload.CorrelationId}");
            }

            log.LogMethodFlow(payload.CorrelationId, nameof(CallUpdateSearchIndexHttpAsync), $"Removed caseId: {payload.CaseId}, documentId: {payload.DocumentId} from the search index");
            log.LogMethodExit(payload.CorrelationId, nameof(CallUpdateSearchIndexHttpAsync), string.Empty);
        }

        private async Task CallTextExtractorAsync(IDurableOrchestrationContext context, CaseDocumentOrchestrationPayload payload, string blobName, ITracker tracker, ILogger log)
        {
            log.LogMethodEntry(payload.CorrelationId, nameof(CallTextExtractorAsync), payload.ToJson());

            try
            {
                await CallTextExtractorHttpAsync(context, payload, blobName, log);
                await tracker.RegisterIndexed(payload.DocumentId);
            }
            catch (Exception exception)
            {
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
                new CreateTextExtractorHttpRequestActivityPayload(payload.CaseUrn, payload.CaseId, payload.DocumentId, payload.VersionId, blobName, payload.CorrelationId));
            var response = await context.CallHttpAsync(request);

            if (response.StatusCode != HttpStatusCode.OK)
            {
                request.Headers.TryGetValue(HttpHeaderKeys.Authorization, out var tokenUsed);
                throw new HttpRequestException($"Failed to ocr/index document with id '{payload.DocumentId}'. Status code: {response.StatusCode}. Token Used: [{tokenUsed}]. CorrelationId: {payload.CorrelationId}");
            }
            
            log.LogMethodExit(payload.CorrelationId, nameof(CallTextExtractorHttpAsync), string.Empty);
        }
        
        private ITracker GetTracker(IDurableOrchestrationContext context, string caseUrn, long caseId, Guid correlationId, ILogger log)
        {
            log.LogMethodEntry(correlationId, nameof(GetTracker), $"CaseId: {caseId.ToString()}");
            
            var entityId = new EntityId(nameof(Tracker), string.Concat(caseUrn, "-", caseId.ToString()));
            
            log.LogMethodExit(correlationId, nameof(GetTracker), string.Empty);
            return context.CreateEntityProxy<ITracker>(entityId);
        }
    }
}
