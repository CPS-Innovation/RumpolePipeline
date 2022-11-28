using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Common.Constants;
using Common.Logging;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace coordinator.Domain.Tracker
{

    [JsonObject(MemberSerialization.OptIn)]
    public class Tracker : ITracker
    {
        [JsonProperty("transactionId")]
        public string TransactionId { get; set; }

        [JsonProperty("documents")]
        public List<TrackerDocument> Documents { get; set; }

        [JsonConverter(typeof(StringEnumConverter))]
        [JsonProperty("status")]
        public TrackerStatus Status { get; set; }

        [JsonProperty("logs")]
        public List<Log> Logs { get; set; }

        public Task Initialise(string transactionId)
        {
            TransactionId = transactionId;
            Documents = new List<TrackerDocument>();
            Status = TrackerStatus.Running;
            Logs = new List<Log>();

            Log(LogType.Initialised);

            return Task.CompletedTask;
        }

        public Task RegisterDocumentIds(IEnumerable<Tuple<string, long>> documentIds)
        {
            Documents = documentIds
                .Select(item => new TrackerDocument { DocumentId = item.Item1, VersionId = item.Item2})
                .ToList();

            Log(LogType.RegisteredDocumentIds);

            return Task.CompletedTask;
        }
        
        public Task RegisterDocumentEvaluated(string documentId)
        {
            var document = Documents.Find(document => document.DocumentId.Equals(documentId, StringComparison.OrdinalIgnoreCase));
            document!.Status = DocumentStatus.DocumentEvaluated;

            Log(LogType.DocumentEvaluated, documentId);

            return Task.CompletedTask;
        }

        public Task RegisterUnexpectedDocumentEvaluationFailure(string documentId)
        {
            var document = Documents.Find(document => document.DocumentId.Equals(documentId, StringComparison.OrdinalIgnoreCase));
            document!.Status = DocumentStatus.UnexpectedFailure;

            Log(LogType.UnexpectedDocumentEvaluationFailure, documentId);

            return Task.CompletedTask;
        }
        
        public Task RegisterUnexpectedExistingDocumentsEvaluationFailure()
        {
            Status = TrackerStatus.UnableToEvaluateExistingDocuments;
            foreach (var doc in Documents)
            {
                doc.Status = DocumentStatus.UnexpectedFailure;
            }
            
            Log(LogType.UnexpectedExistingDocumentsEvaluationFailure);

            return Task.CompletedTask;
        }
        
        public Task RegisterUnableToEvaluateDocument(string documentId)
        {
            var document = Documents.Find(document => document.DocumentId.Equals(documentId, StringComparison.OrdinalIgnoreCase));
            document!.Status = DocumentStatus.UnableToEvaluateDocument;

            Log(LogType.UnableToEvaluateDocument, documentId);

            return Task.CompletedTask;
        }


        public Task RegisterPdfBlobName(RegisterPdfBlobNameArg arg)
        {
            var document = Documents.Find(document => document.DocumentId.Equals(arg.DocumentId, StringComparison.OrdinalIgnoreCase));
            document!.PdfBlobName = arg.BlobName;
            document.Status = DocumentStatus.PdfUploadedToBlob;

            Log(LogType.RegisteredPdfBlobName, arg.DocumentId);

            return Task.CompletedTask;
        }
        
        public Task RegisterBlobAlreadyProcessed(RegisterPdfBlobNameArg arg)
        {
            var document = Documents.Find(document => document.DocumentId.Equals(arg.DocumentId, StringComparison.OrdinalIgnoreCase));
            document!.PdfBlobName = arg.BlobName;
            document.Status = DocumentStatus.DocumentAlreadyProcessed;

            Log(LogType.DocumentAlreadyProcessed, arg.DocumentId);

            return Task.CompletedTask;
        }

        public Task RegisterDocumentNotFoundInDDEI(string documentId)
        {
            var document = Documents.Find(document => document.DocumentId.Equals(documentId, StringComparison.OrdinalIgnoreCase));
            document!.Status = DocumentStatus.NotFoundInDDEI;

            Log(LogType.DocumentNotFoundInDDEI, documentId);

            return Task.CompletedTask;
        }

        public Task RegisterUnableToConvertDocumentToPdf(string documentId)
        {
            var document = Documents.Find(document => document.DocumentId.Equals(documentId, StringComparison.OrdinalIgnoreCase));
            document!.Status = DocumentStatus.UnableToConvertToPdf;

            Log(LogType.UnableToConvertDocumentToPdf, documentId);

            return Task.CompletedTask;
        }

        public Task RegisterUnexpectedPdfDocumentFailure(string documentId)
        {
            var document = Documents.Find(document => document.DocumentId.Equals(documentId, StringComparison.OrdinalIgnoreCase));
            document!.Status = DocumentStatus.UnexpectedFailure;

            Log(LogType.UnexpectedDocumentFailure, documentId);

            return Task.CompletedTask;
        }

        public Task RegisterNoDocumentsFoundInDDEI()
        {
            Status = TrackerStatus.NoDocumentsFoundInDDEI;
            Log(LogType.NoDocumentsFoundInDDEI);

            return Task.CompletedTask;
        }

        public Task RegisterIndexed(string documentId)
        {
            var document = Documents.Find(document => document.DocumentId.Equals(documentId, StringComparison.OrdinalIgnoreCase));
            document!.Status = DocumentStatus.Indexed;

            Log(LogType.Indexed, documentId);

            return Task.CompletedTask;
        }

        public Task RegisterOcrAndIndexFailure(string documentId)
        {
            var document = Documents.Find(document => document.DocumentId.Equals(documentId, StringComparison.OrdinalIgnoreCase));
            document!.Status = DocumentStatus.OcrAndIndexFailure;

            Log(LogType.OcrAndIndexFailure, documentId);

            return Task.CompletedTask;
        }

        public Task RegisterDocumentRemovedFromSearchIndex(string documentId)
        {
            var document = Documents.Find(document => document.DocumentId.Equals(documentId, StringComparison.OrdinalIgnoreCase));
            document!.Status = DocumentStatus.DocumentRemovedFromSearchIndex;

            Log(LogType.DocumentRemovedFromSearchIndex, documentId);

            return Task.CompletedTask;
        }

        public Task RegisterUnexpectedSearchIndexRemovalFailure(string documentId)
        {
            var document = Documents.Find(document => document.DocumentId.Equals(documentId, StringComparison.OrdinalIgnoreCase));
            document!.Status = DocumentStatus.UnexpectedSearchIndexRemovalFailure;

            Log(LogType.IndexRemovalFailure, documentId);

            return Task.CompletedTask;
        }

        public Task RegisterUnableToUpdateSearchIndex(string documentId)
        {
            var document = Documents.Find(document => document.DocumentId.Equals(documentId, StringComparison.OrdinalIgnoreCase));
            document!.Status = DocumentStatus.SearchIndexUpdateFailure;

            Log(LogType.IndexRemovalFailure, documentId);

            return Task.CompletedTask;
        }

        public Task RegisterCompleted()
        {
            Status = TrackerStatus.Completed;
            Log(LogType.Completed);

            return Task.CompletedTask;
        }

        public Task RegisterFailed()
        {
            Status = TrackerStatus.Failed;
            Log(LogType.Failed);

            return Task.CompletedTask;
        }

        public Task<List<TrackerDocument>> GetDocuments()
        {
            return Task.FromResult(Documents);
        }

        public Task<bool> AllDocumentsFailed()
        {
            return Task.FromResult(
                Documents.All(d => d.Status is DocumentStatus.NotFoundInDDEI 
                    or DocumentStatus.UnableToConvertToPdf or DocumentStatus.UnexpectedFailure));
        }

        public Task<bool> IsAlreadyProcessed()
        {
            return Task.FromResult(Status is TrackerStatus.Completed or TrackerStatus.NoDocumentsFoundInDDEI);
        }

        private void Log(LogType status, string documentId = null)
        {
            Logs.Add(new Log
            {
                LogType = status.ToString(),
                TimeStamp = DateTime.UtcNow.ToString("yyyy-MM-ddTHH\\:mm\\:ss.fffzzz"),
                DocumentId = documentId
            });
        }

        [FunctionName("Tracker")]
        public static Task Run([EntityTrigger] IDurableEntityContext context)
        {
            return context.DispatchAsync<Tracker>();
        }

        [FunctionName("TrackerStatus")]
        public async Task<IActionResult> HttpStart(
            [HttpTrigger(AuthorizationLevel.Function, "get", Route = "cases/{caseUrn}/{caseId}/tracker")] HttpRequestMessage req,
            string caseUrn,
            string caseId,
            [DurableClient] IDurableEntityClient client,
            ILogger log)
        {
            const string loggingName = $"TrackerStatus - {nameof(HttpStart)}";
            const string correlationErrorMessage = "Invalid correlationId. A valid GUID is required.";
            
            req.Headers.TryGetValues(HttpHeaderKeys.CorrelationId, out var correlationIdValues);
            if (correlationIdValues == null)
            {
                log.LogMethodFlow(Guid.Empty, loggingName, correlationErrorMessage);
                return new BadRequestObjectResult(correlationErrorMessage);
            }

            var correlationId = correlationIdValues.FirstOrDefault();
            if (!Guid.TryParse(correlationId, out var currentCorrelationId))
                if (currentCorrelationId == Guid.Empty)
                {
                    log.LogMethodFlow(Guid.Empty, loggingName, correlationErrorMessage);
                    return new BadRequestObjectResult(correlationErrorMessage);
                }

            log.LogMethodEntry(currentCorrelationId, loggingName, caseId);

            var entityKey = string.Concat(caseUrn, "-", caseId);
            var entityId = new EntityId(nameof(Tracker), entityKey);
            var stateResponse = await client.ReadEntityStateAsync<Tracker>(entityId);
            if (!stateResponse.EntityExists)
            {
                var baseMessage = $"No pipeline tracker found with id '{entityKey}'";
                log.LogMethodFlow(currentCorrelationId, loggingName, baseMessage);
                return new NotFoundObjectResult(baseMessage);
            }

            log.LogMethodExit(currentCorrelationId, loggingName, string.Empty);
            return new OkObjectResult(stateResponse.EntityState);
        }
    }
}