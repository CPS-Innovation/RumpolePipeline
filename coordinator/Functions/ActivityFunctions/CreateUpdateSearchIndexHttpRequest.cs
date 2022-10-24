using System;
using System.Threading.Tasks;
using Common.Domain.Extensions;
using Common.Logging;
using coordinator.Domain;
using coordinator.Factories;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Extensions.Logging;

namespace coordinator.Functions.ActivityFunctions
{
    public class CreateUpdateSearchIndexHttpRequest
    {
        private readonly IUpdateSearchIndexHttpRequestFactory _updateSearchIndexHttpRequestFactory;
        private readonly ILogger<CreateUpdateSearchIndexHttpRequest> _log;

        public CreateUpdateSearchIndexHttpRequest(IUpdateSearchIndexHttpRequestFactory updateSearchIndexHttpRequestFactory, ILogger<CreateUpdateSearchIndexHttpRequest> logger)
        {
           _updateSearchIndexHttpRequestFactory = updateSearchIndexHttpRequestFactory;
           _log = logger;
        }

        [FunctionName("CreateUpdateSearchIndexHttpRequest")]
        public async Task<DurableHttpRequest> Run([ActivityTrigger] IDurableActivityContext context)
        {
            const string loggingName = $"{nameof(CreateUpdateSearchIndexHttpRequest)} - {nameof(Run)}";
            var payload = context.GetInput<CreateUpdateSearchIndexHttpRequestActivityPayload>();
            
            if (payload == null)
                throw new ArgumentException("Payload cannot be null.");
            if (payload.CaseId == 0)
                throw new ArgumentException("CaseId cannot be zero");
            if (string.IsNullOrWhiteSpace(payload.DocumentId))
                throw new ArgumentException("DocumentId is empty");
            if (payload.CorrelationId == Guid.Empty)
                throw new ArgumentException("CorrelationId must be valid GUID");
            
            _log.LogMethodEntry(payload.CorrelationId, loggingName, payload.ToJson());
            var result = await _updateSearchIndexHttpRequestFactory.Create(payload.CaseId, payload.DocumentId, payload.CorrelationId);
            
            _log.LogMethodExit(payload.CorrelationId, loggingName, string.Empty);
            return result;
        }
    }
}
