using System;
using System.Threading.Tasks;
using coordinator.Clients;
using coordinator.Domain;
using coordinator.Domain.DocumentExtraction;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;

namespace coordinator.Functions.ActivityFunctions
{
    public class GetCaseDocuments
    {
        private readonly IDocumentExtractionClient _documentExtractionClient;

        public GetCaseDocuments(IDocumentExtractionClient documentExtractionClient)
        {
           _documentExtractionClient = documentExtractionClient;
        }

        [FunctionName("GetCaseDocuments")]
        public async Task<CaseDocument[]> Run([ActivityTrigger] IDurableActivityContext context)
        {
            var payload = context.GetInput<GetCaseDocumentsActivityPayload>();
            if (payload == null)
            {
                throw new ArgumentException("Payload cannot be null.");
            }

            var caseDetails = await _documentExtractionClient.GetCaseDocumentsAsync(payload.CaseId.ToString(), payload.AccessToken);
            return caseDetails.CaseDocuments;
        }
    }
}
