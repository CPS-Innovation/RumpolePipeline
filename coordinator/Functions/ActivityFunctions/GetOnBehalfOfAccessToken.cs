using System;
using System.Threading.Tasks;
using coordinator.Domain.Adapters;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Extensions.Configuration;

namespace coordinator.Functions.ActivityFunctions
{
    public class GetOnBehalfOfAccessToken
    {
        private readonly IIdentityClientAdapter _identityClientAdapter;
        private readonly IConfiguration _configuration;

        public GetOnBehalfOfAccessToken(IIdentityClientAdapter identityClientAdapter, IConfiguration configuration)
        {
            _identityClientAdapter = identityClientAdapter;
            _configuration = configuration;
        }

        [FunctionName("GetOnBehalfOfAccessToken")]
        public async Task<string> Run([ActivityTrigger] IDurableActivityContext context)
        {
            var accessToken = context.GetInput<string>();
            if (string.IsNullOrWhiteSpace(accessToken))
            {
                throw new ArgumentException("Access token cannot be null.");
            }
            
            var onBehalfOfScopes = _configuration["CoreDataApiScope"];

            return await _identityClientAdapter.GetAccessTokenOnBehalfOfAsync(accessToken, onBehalfOfScopes);
        }
    }
}
