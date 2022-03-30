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

        [JsonProperty("logs")]
        public List<Log> Logs { get; set; }

        [JsonProperty("status")]
        [JsonConverter(typeof(StringEnumConverter))]
        public TrackerStatus Status { get; set; }

        public Task Initialise(string transactionId)
        {
            TransactionId = transactionId;
            Documents = new List<TrackerDocument>();
            Logs = new List<Log>();
            Status = TrackerStatus.Initialised;

            Log(Status);

            return Task.CompletedTask;
        }

        public Task RegisterDocumentIds(IEnumerable<int> documentIds)
        {
            Documents = documentIds
                .Select(documentId => new TrackerDocument { DocumentId = documentId })
                .ToList();

            Status = TrackerStatus.RegisteredDocumentIds;
            Log(Status);

            return Task.CompletedTask;
        }

        public Task RegisterPdfBlobName(RegisterPdfBlobNameArg arg)
        {
            var document = Documents.Find(document => document.DocumentId == arg.DocumentId);
            document.PdfBlobName = arg.BlobName;

            //TODO always set status??
            Status = TrackerStatus.RegisteredPdfBlobName;
            Log(Status, arg.DocumentId);

            return Task.CompletedTask;
        }

        public Task RegisterCompleted()
        {
            Status = TrackerStatus.Completed;
            Log(Status);

            return Task.CompletedTask;
        }

        public Task RegisterError()
        {
            Status = TrackerStatus.Errored;
            Log(Status);

            return Task.CompletedTask;
        }

        public Task<List<TrackerDocument>> GetDocuments()
        {
            return Task.FromResult(Documents);
        }

        public Task<bool> IsAlreadyProcessed()
        {
            //TODO is it already processed if errored?
            return Task.FromResult(Status == TrackerStatus.Completed);
        }

        private void Log(TrackerStatus status, int? documentId = null)
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
        public async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Function, "get", Route = "cases/{caseId}/tracker")] HttpRequestMessage req,
            string caseId,
            [DurableClient] IDurableEntityClient client,
            ILogger<Tracker> log)
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