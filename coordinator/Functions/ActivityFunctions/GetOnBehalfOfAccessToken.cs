using System;
using System.Threading.Tasks;
using coordinator.Clients;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;

namespace coordinator.Functions.ActivityFunctions
{
    public class GetOnBehalfOfAccessToken
    {
        private readonly IOnBehalfOfTokenClient _onBehalfOfTokenClient;

        public GetOnBehalfOfAccessToken(IOnBehalfOfTokenClient onBehalfOfTokenClient)
        {
            _onBehalfOfTokenClient = onBehalfOfTokenClient;
        }

        [FunctionName("GetOnBehalfOfAccessToken")]
        public async Task<string> Run([ActivityTrigger] IDurableActivityContext context)
        {
            var accessToken = context.GetInput<string>();
            if (string.IsNullOrWhiteSpace(accessToken))
            {
                throw new ArgumentException("Access token cannot be null.");
            }

            return await _onBehalfOfTokenClient.GetAccessToken(accessToken);
        }
    }
}
