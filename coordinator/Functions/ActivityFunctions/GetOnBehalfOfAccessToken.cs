using System;
using System.Threading.Tasks;
using Common.Adapters;
using coordinator.Domain.Requests;
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
            var request = context.GetInput<GetOnBehalfOfTokenRequest>();
            if (string.IsNullOrWhiteSpace(request.AccessToken))
                throw new ArgumentException("Access token cannot be null.");

            if (request.CorrelationId == Guid.Empty)
                throw new ArgumentException("CorrelationId must not be null");
            
            var onBehalfOfScopes = _configuration["CoreDataApiScope"];

            return await _identityClientAdapter.GetAccessTokenOnBehalfOfAsync(request.AccessToken, onBehalfOfScopes, request.CorrelationId);
        }
    }
}
