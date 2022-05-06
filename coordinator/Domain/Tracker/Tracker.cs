using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
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

        public Task RegisterDocumentIds(IEnumerable<string> documentIds)
        {
            Documents = documentIds
                .Select(documentId => new TrackerDocument { DocumentId = documentId })
                .ToList();

            Log(LogType.RegisteredDocumentIds);

            return Task.CompletedTask;
        }

        public Task RegisterPdfBlobName(RegisterPdfBlobNameArg arg)
        {
            var document = Documents.Find(document => document.DocumentId.Equals(arg.DocumentId, StringComparison.OrdinalIgnoreCase));
            document.PdfBlobName = arg.BlobName;
            document.Status = DocumentStatus.PdfUploadedToBlob;

            Log(LogType.RegisteredPdfBlobName, arg.DocumentId);

            return Task.CompletedTask;
        }

        public Task RegisterDocumentNotFoundInCDE(string documentId)
        {
            var document = Documents.Find(document => document.DocumentId.Equals(documentId, StringComparison.OrdinalIgnoreCase));
            document.Status = DocumentStatus.NotFoundInCDE;

            Log(LogType.DocumentNotFoundInCDE, documentId);

            return Task.CompletedTask;
        }

        public Task RegisterUnableToConvertDocumentToPdf(string documentId)
        {
            var document = Documents.Find(document => document.DocumentId.Equals(documentId, StringComparison.OrdinalIgnoreCase));
            document.Status = DocumentStatus.UnableToConvertToPdf;

            Log(LogType.UnableToConvertDocumentToPdf, documentId);

            return Task.CompletedTask;
        }

        public Task RegisterUnexpectedDocumentFailure(string documentId)
        {
            var document = Documents.Find(document => document.DocumentId.Equals(documentId, StringComparison.OrdinalIgnoreCase));
            document.Status = DocumentStatus.UnexpectedFailure;

            Log(LogType.UnexpectedDocumentFailure, documentId);

            return Task.CompletedTask;
        }

        public Task RegisterNoDocumentsFoundInCDE()
        {
            Status = TrackerStatus.NoDocumentsFoundInCDE;
            Log(LogType.NoDocumentsFoundInCDE);

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
                Documents.All(d => d.Status == DocumentStatus.NotFoundInCDE ||
                                   d.Status == DocumentStatus.UnableToConvertToPdf ||
                                   d.Status == DocumentStatus.UnexpectedFailure));
        }

        public Task<bool> IsAlreadyProcessed()
        {
            return Task.FromResult(Status == TrackerStatus.Completed || Status == TrackerStatus.NoDocumentsFoundInCDE);
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
        public Task Run([EntityTrigger] IDurableEntityContext context)
        { 
            return context.DispatchAsync<Tracker>();
        }

        [FunctionName("TrackerStatus")]
        public async Task<IActionResult> HttpStart(
            [HttpTrigger(AuthorizationLevel.Function, "get", Route = "cases/{caseId}/tracker")] HttpRequestMessage req,
            string caseId,
            [DurableClient] IDurableEntityClient client,
            ILogger log)
        {
            var entityId = new EntityId(nameof(Tracker), caseId);
            var stateResponse = await client.ReadEntityStateAsync<Tracker>(entityId);
            if (!stateResponse.EntityExists)
            {
                var baseMessage = $"No pipeline tracker found with id '{caseId}'";
                log.LogError(baseMessage);
                return new NotFoundObjectResult(baseMessage);
            }

            return new OkObjectResult(stateResponse.EntityState);
        }
    }
}