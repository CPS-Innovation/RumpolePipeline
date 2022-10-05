using System;
using System.Threading.Tasks;
using coordinator.Domain;
using coordinator.Factories;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;

namespace coordinator.Functions.ActivityFunctions
{
    public class CreateTextExtractorHttpRequest
    {
        private readonly ITextExtractorHttpRequestFactory _textExtractorHttpRequestFactory;

        public CreateTextExtractorHttpRequest(ITextExtractorHttpRequestFactory textExtractorHttpRequestFactory)
        {
           _textExtractorHttpRequestFactory = textExtractorHttpRequestFactory;
        }

        [FunctionName("CreateTextExtractorHttpRequest")]
        public async Task<DurableHttpRequest> Run([ActivityTrigger] IDurableActivityContext context)
        {
            var payload = context.GetInput<CreateTextExtractorHttpRequestActivityPayload>();
            
            if (payload == null)
                throw new ArgumentException("Payload cannot be null.");
            if (payload.CaseId == 0)
                throw new ArgumentException("CaseId cannot be zero");
            if (string.IsNullOrWhiteSpace(payload.DocumentId))
                throw new ArgumentException("DocumentId is empty");
            if (string.IsNullOrWhiteSpace(payload.BlobName))
                throw new ArgumentException("The supplied blob name is empty");
            if (payload.CorrelationId == Guid.Empty)
                throw new ArgumentException("CorrelationId must be valid GUID");
            
            return await _textExtractorHttpRequestFactory.Create(payload.CaseId, payload.DocumentId, payload.BlobName, payload.CorrelationId);
        }
    }
}
