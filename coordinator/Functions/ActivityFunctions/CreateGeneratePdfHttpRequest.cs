using System;
using System.Threading.Tasks;
using coordinator.Domain;
using coordinator.Factories;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;

namespace coordinator.Functions.ActivityFunctions
{
    public class CreateGeneratePdfHttpRequest
    {
        private readonly IGeneratePdfHttpRequestFactory _generatePdfHttpRequestFactory;

        public CreateGeneratePdfHttpRequest(IGeneratePdfHttpRequestFactory generatePdfHttpRequestFactory)
        {
           _generatePdfHttpRequestFactory = generatePdfHttpRequestFactory;
        }

        [FunctionName("CreateGeneratePdfHttpRequest")]
        public async Task<DurableHttpRequest> Run([ActivityTrigger] IDurableActivityContext context)
        {
            var payload = context.GetInput<CreateGeneratePdfHttpRequestActivityPayload>();
            if (payload == null)
            {
                throw new ArgumentException("Payload cannot be null.");
            }

            return await _generatePdfHttpRequestFactory.Create(payload.CaseId, payload.DocumentId, payload.FileName);
        }
    }
}
