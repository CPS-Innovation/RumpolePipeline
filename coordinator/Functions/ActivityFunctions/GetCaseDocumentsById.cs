using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using coordinator.Clients;
using coordinator.Domain;
using coordinator.Domain.CoreDataApi;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;

namespace coordinator.Functions.ActivityFunctions
{
    public class GetCaseDocumentsById
    {
        private readonly ICoreDataApiClient _coreDataApiClient;

        public GetCaseDocumentsById(ICoreDataApiClient coreDataApiClient)
        {
           _coreDataApiClient = coreDataApiClient;
        }

        [FunctionName("GetCaseDocumentsById")]
        public async Task<List<Document>> Run([ActivityTrigger] IDurableActivityContext context)
        {
            var payload = context.GetInput<GetCaseDocumentsByIdActivityPayload>();
            if (payload == null)
            {
                throw new ArgumentException("Payload cannot be null.");
            }

            return await _coreDataApiClient.GetCaseDocumentsByIdAsync(payload.CaseId, payload.AccessToken);
        }
    }
}
