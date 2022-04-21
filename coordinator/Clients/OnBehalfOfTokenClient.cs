using coordinator.Domain.Exceptions;
using Microsoft.Extensions.Configuration;
using Microsoft.Identity.Client;
using System.Collections.ObjectModel;
using System.Threading.Tasks;

namespace coordinator.Clients
{
    public class OnBehalfOfTokenClient : IOnBehalfOfTokenClient
    {
        private readonly IConfidentialClientApplication _application;
        private readonly IConfiguration _configuration;

        private const string assertionType = "urn:ietf:params:oauth:grant-type:jwt-bearer";

        public OnBehalfOfTokenClient(IConfidentialClientApplication application, 
                                     IConfiguration configuration)
        {
            _application = application;
            _configuration = configuration;
        }

        public async Task<string> GetAccessTokenAsync(string accessToken)
        {
            AuthenticationResult result;
            
            try
            {
                var userAssertion = new UserAssertion(accessToken, assertionType);
                var scopes = new Collection<string> { _configuration["CoreDataApiScope"] };
                result = await _application.AcquireTokenOnBehalfOf(scopes, userAssertion).ExecuteAsync();
            }
            catch (MsalException exception)
            {
                throw new OnBehalfOfTokenClientException($"Failed to acquire onBehalfOf token. Exception: {exception.Message}");
            }

            return result.AccessToken;
        }
    }
}
