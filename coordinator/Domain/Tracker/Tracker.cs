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

        // a `Task` so we can wait for it and be assured we are initialsed and avoid null references //TODO do we really need this to be a Task?
        public void Initialise(string transactionId)
        {
            TransactionId = transactionId;
            Documents = new List<TrackerDocument>();
            Logs = new List<Log>();
            Status = TrackerStatus.Initialise;

            Log(Status);
        }

        // a `Task` so we can wait for it and be assured we are registered and avoid null references //TODO do we really need this to be a Task?
        public void RegisterDocumentIds(List<int> documentIds)
        {
            Documents = documentIds
                .Select(documentId => new TrackerDocument { DocumentId = documentId })
                .ToList();

            Status = TrackerStatus.RegisterDocumentIds;
            Log(Status);
        }

        public void RegisterPdfBlobName(RegisterPdfBlobNameArg arg)
        {
            var doc = Documents.Find(document => document.DocumentId == arg.DocumentId);
            doc.PdfBlobName = arg.BlobName;

            Status = TrackerStatus.RegisterPdfBlobName;
            Log(Status, arg.DocumentId);
        }

        public void RegisterCompleted()
        {
            Status = TrackerStatus.Complete;
            Log(Status);
        }

        public Task<ITracker> Get()
        {
            return Task.FromResult((ITracker)this);
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
                return new NotFoundObjectResult($"No pipeline tracker with id '{caseId}'");
            }

            var response = await stateResponse.EntityState.Get(); //TODO can we just do entity state here? and not Get
            return new OkObjectResult(response);
        }
    }
}