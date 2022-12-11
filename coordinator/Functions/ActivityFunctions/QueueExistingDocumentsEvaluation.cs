using System;
using System.Linq;
using System.Threading.Tasks;
using Common.Constants;
using Common.Domain.Extensions;
using Common.Domain.Requests;
using Common.Logging;
using Common.Services.StorageQueueService.Contracts;
using Common.Wrappers;
using coordinator.Domain;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace coordinator.Functions.ActivityFunctions
{
    public class QueueExistingDocumentsEvaluation
    {
        private readonly ILogger<QueueExistingDocumentsEvaluation> _log;
        private readonly IJsonConvertWrapper _jsonConvertWrapper;
        private readonly IConfiguration _configuration;
        private readonly IStorageQueueService _storageQueueService;
        
        public QueueExistingDocumentsEvaluation(ILogger<QueueExistingDocumentsEvaluation> logger, IJsonConvertWrapper jsonConvertWrapper, 
            IConfiguration configuration, IStorageQueueService storageQueueService)
        {
           _log = logger;
           _jsonConvertWrapper = jsonConvertWrapper;
           _configuration = configuration;
           _storageQueueService = storageQueueService;
        }

        [FunctionName("QueueExistingDocumentsEvaluation")]
        public async Task Run([ActivityTrigger] IDurableActivityContext context)
        {
            const string loggingName = $"{nameof(QueueExistingDocumentsEvaluation)} - {nameof(Run)}";
            var payload = context.GetInput<QueueExistingDocumentsEvaluationPayload>();
            
            if (payload == null)
                throw new ArgumentException("Payload cannot be null.");
            if (string.IsNullOrWhiteSpace(payload.CaseUrn))
                throw new ArgumentException("CaseUrn cannot be empty");
            if (payload.CaseId == 0)
                throw new ArgumentException("CaseId cannot be zero");
            if (payload.CorrelationId == Guid.Empty)
                throw new ArgumentException("CorrelationId must be valid GUID");
            if (payload.CaseDocuments == null)
                throw new ArgumentNullException(nameof(payload), "CaseDocuments collection cannot be null");
            if (payload.CaseDocuments != null && !payload.CaseDocuments.Any())
                throw new ArgumentException("CaseDocuments collection cannot be zero-length", nameof(context));
            
            _log.LogMethodEntry(payload.CorrelationId, loggingName, payload.ToJson());
            
            await _storageQueueService.AddNewMessage(_jsonConvertWrapper.SerializeObject(new EvaluateExistingDocumentsRequest(payload.CaseId, payload.CaseDocuments, 
                payload.CorrelationId)), _configuration[ConfigKeys.SharedKeys.EvaluateExistingDocumentsQueueName]);
            
            _log.LogMethodExit(payload.CorrelationId, loggingName, string.Empty);
        }
    }
}
