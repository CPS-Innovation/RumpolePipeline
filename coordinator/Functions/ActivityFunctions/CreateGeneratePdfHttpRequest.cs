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
                throw new ArgumentException("Payload cannot be null.");
            if (payload.CaseId == 0)
                throw new ArgumentException("CaseId cannot be zero");
            if (string.IsNullOrWhiteSpace(payload.DocumentId))
                throw new ArgumentException("DocumentId is empty");
            if (string.IsNullOrWhiteSpace(payload.FileName))
                throw new ArgumentException("The supplied filename is empty");
            if (payload.CorrelationId == Guid.Empty)
                throw new ArgumentException("CorrelationId must be valid GUID");
            
            return await _generatePdfHttpRequestFactory.Create(payload.CaseId, payload.DocumentId, payload.FileName, payload.CorrelationId);
        }
    }
}
