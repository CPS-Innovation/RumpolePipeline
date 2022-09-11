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
            {
                throw new ArgumentException("Payload cannot be null.");
            }

            return await _textExtractorHttpRequestFactory.Create(payload.CaseId, payload.DocumentId, payload.BlobName);
        }
    }
}
