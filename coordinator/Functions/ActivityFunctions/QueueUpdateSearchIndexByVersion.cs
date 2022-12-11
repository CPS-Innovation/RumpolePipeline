using System;
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
    public class QueueUpdateSearchIndexByVersion
    {
        private readonly ILogger<QueueUpdateSearchIndexByVersion> _log;
        private readonly IJsonConvertWrapper _jsonConvertWrapper;
        private readonly IConfiguration _configuration;
        private readonly IStorageQueueService _storageQueueService;

        public QueueUpdateSearchIndexByVersion(ILogger<QueueUpdateSearchIndexByVersion> logger, IJsonConvertWrapper jsonConvertWrapper, 
            IConfiguration configuration, IStorageQueueService storageQueueService)
        {
           _log = logger;
           _jsonConvertWrapper = jsonConvertWrapper;
           _configuration = configuration;
           _storageQueueService = storageQueueService;
        }

        [FunctionName("QueueUpdateSearchIndexByVersion")]
        public async Task Run([ActivityTrigger] IDurableActivityContext context)
        {
            const string loggingName = $"{nameof(QueueUpdateSearchIndexByVersion)} - {nameof(Run)}";
            var payload = context.GetInput<QueueUpdateSearchIndexByVersionPayload>();
            
            if (payload == null)
                throw new ArgumentException("Payload cannot be null.");
            if (string.IsNullOrWhiteSpace(payload.CaseUrn))
                throw new ArgumentException("CaseUrn cannot be empty");
            if (payload.CaseId == 0)
                throw new ArgumentException("CaseId cannot be zero");
            if (string.IsNullOrWhiteSpace(payload.DocumentId))
                throw new ArgumentException("A valid documentId must be included in the request");
            if (payload.CorrelationId == Guid.Empty)
                throw new ArgumentException("CorrelationId must be valid GUID");
            
            _log.LogMethodEntry(payload.CorrelationId, loggingName, payload.ToJson());
            
            await _storageQueueService.AddNewMessage(_jsonConvertWrapper.SerializeObject(new UpdateSearchIndexByVersionRequest(payload.CaseId, 
                     payload.DocumentId, payload.VersionId, payload.CorrelationId)), _configuration[ConfigKeys.SharedKeys.UpdateSearchIndexByVersionQueueName]);
            
            _log.LogMethodExit(payload.CorrelationId, loggingName, string.Empty);
        }
    }
}
