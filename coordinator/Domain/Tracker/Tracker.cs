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
            Status = TrackerStatus.Initialise;

            Log(Status);

            return Task.CompletedTask;
        }

        public Task RegisterDocumentIds(IEnumerable<int> documentIds)
        {
            Documents = documentIds
                .Select(documentId => new TrackerDocument { DocumentId = documentId })
                .ToList();

            Status = TrackerStatus.RegisterDocumentIds;
            Log(Status);

            return Task.CompletedTask;
        }

        public Task RegisterPdfBlobName(RegisterPdfBlobNameArg arg)
        {
            var document = Documents.Find(document => document.DocumentId == arg.DocumentId);
            document.PdfBlobName = arg.BlobName;

            Status = TrackerStatus.RegisterPdfBlobName;
            Log(Status, arg.DocumentId);

            return Task.CompletedTask;
        }

        public Task RegisterCompleted()
        {
            Status = TrackerStatus.Complete;
            Log(Status);

            return Task.CompletedTask;
        }

        public Task RegisterError()
        {
            Status = TrackerStatus.Error;
            Log(Status);

            return Task.CompletedTask;
        }

        public Task<List<TrackerDocument>> GetDocuments()
        {
            return Task.FromResult(Documents);
        }

        public Task<bool> IsAlreadyProcessed()
        {
            return Task.FromResult(Status == TrackerStatus.Complete);
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
        public static Task Run([EntityTrigger] IDurableEntityContext context)
        { 
            return context.DispatchAsync<Tracker>();
        }

        [FunctionName("TrackerStatus")]
        public static async Task<IActionResult> HttpStart(
            [HttpTrigger(AuthorizationLevel.Function, "get", Route = "cases/{caseId}/tracker")] HttpRequestMessage req,
            string caseId,
            [DurableClient] IDurableEntityClient client,
            ILogger log)
        {
            var entityId = new EntityId(nameof(Tracker), caseId);
            var stateResponse = await client.ReadEntityStateAsync<Tracker>(entityId);
            if (!stateResponse.EntityExists)
            {
                //TODO log
                return new NotFoundObjectResult($"No pipeline tracker found with id '{caseId}'");
            }

            return new OkObjectResult(stateResponse.EntityState);
        }
    }
}