using System;
using System.Threading.Tasks;
using Common.Domain.Extensions;
using Common.Logging;
using Common.Services.DocumentExtractionService.Contracts;
using coordinator.Domain;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Extensions.Logging;

namespace coordinator.Functions.ActivityFunctions
{
    public class EvaluateExistingDocuments
    {
        private readonly IDdeiDocumentExtractionService _documentExtractionService;
        private readonly ILogger<EvaluateExistingDocuments> _log;

        public EvaluateExistingDocuments(IDdeiDocumentExtractionService documentExtractionService, ILogger<EvaluateExistingDocuments> logger)
        {
           _documentExtractionService = documentExtractionService;
           _log = logger;
        }

        [FunctionName("EvaluateExistingDocuments")]
        public async Task Run([ActivityTrigger] IDurableActivityContext context)
        {
            const string loggingName = $"{nameof(GetCaseDocuments)} - {nameof(Run)}";
            var payload = context.GetInput<EvaluateExistingDocumentsPayload>();
            
            if (payload == null)
                throw new ArgumentException("Payload cannot be null.");
            if (string.IsNullOrWhiteSpace(payload.CaseUrn))
                throw new ArgumentException("CaseUrn cannot be empty");
            if (payload.CaseId == 0)
                throw new ArgumentException("CaseId cannot be zero");
            if (payload.CorrelationId == Guid.Empty)
                throw new ArgumentException("CorrelationId must be valid GUID");
            
            _log.LogMethodEntry(payload.CorrelationId, loggingName, payload.ToJson());
            
            
            
            _log.LogMethodExit(payload.CorrelationId, loggingName, string.Empty);
        }
    }
}
