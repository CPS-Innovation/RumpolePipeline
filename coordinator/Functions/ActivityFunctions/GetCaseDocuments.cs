using System;
using System.Threading.Tasks;
using Common.Domain.DocumentExtraction;
using Common.Domain.Extensions;
using Common.Logging;
using coordinator.Clients;
using coordinator.Domain;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Extensions.Logging;

namespace coordinator.Functions.ActivityFunctions
{
    public class GetCaseDocuments
    {
        private readonly IDocumentExtractionClient _documentExtractionClient;
        private readonly ILogger<GetCaseDocuments> _log;

        public GetCaseDocuments(IDocumentExtractionClient documentExtractionClient, ILogger<GetCaseDocuments> logger)
        {
           _documentExtractionClient = documentExtractionClient;
           _log = logger;
        }

        [FunctionName("GetCaseDocuments")]
        public async Task<CaseDocument[]> Run([ActivityTrigger] IDurableActivityContext context)
        {
            const string loggingName = $"{nameof(GetCaseDocuments)} - {nameof(Run)}";
            var payload = context.GetInput<GetCaseDocumentsActivityPayload>();
            
            if (payload == null)
                throw new ArgumentException("Payload cannot be null.");
            if (payload.CaseId == 0)
                throw new ArgumentException("CaseId cannot be zero");
            if (string.IsNullOrWhiteSpace(payload.AccessToken))
                throw new ArgumentException("Access Token cannot be null");
            if (payload.CorrelationId == Guid.Empty)
                throw new ArgumentException("CorrelationId must be valid GUID");
            
            _log.LogMethodEntry(payload.CorrelationId, loggingName, payload.ToJson());
            var caseDetails = await _documentExtractionClient.GetCaseDocumentsAsync(payload.CaseId.ToString(), payload.AccessToken, payload.CorrelationId);
            
            _log.LogMethodExit(payload.CorrelationId, loggingName, string.Empty);
            return caseDetails.CaseDocuments;
        }
    }
}
