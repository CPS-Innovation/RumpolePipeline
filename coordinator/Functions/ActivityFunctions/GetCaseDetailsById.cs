using System;
using System.Threading.Tasks;
using coordinator.Clients;
using coordinator.Domain;
using coordinator.Domain.CoreDataApi;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;

namespace coordinator.Functions.ActivityFunctions
{
    public class GetCaseDetailsById
    {
        private readonly ICoreDataApiClient _coreDataApiClient;

        public GetCaseDetailsById(ICoreDataApiClient coreDataApiClient)
        {
           _coreDataApiClient = coreDataApiClient;
        }

        [FunctionName("GetCaseDetailsById")]
        public async Task<CaseDetails> Run([ActivityTrigger] IDurableActivityContext context)
        {
            var payload = context.GetInput<GetCaseDetailsByIdActivityPayload>();
            if (payload == null)
            {
                throw new ArgumentException("Payload cannot be null.");
            }

            return await _coreDataApiClient.GetCaseDetailsByIdAsync(payload.CaseId, payload.AccessToken);
        }
    }
}
