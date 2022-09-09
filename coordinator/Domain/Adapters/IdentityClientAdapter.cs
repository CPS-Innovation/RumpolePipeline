using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using coordinator.Domain.Exceptions;
using Microsoft.Identity.Client;

namespace coordinator.Domain.Adapters
{
    public class IdentityClientAdapter : IIdentityClientAdapter
    {
        private readonly IConfidentialClientApplication _confidentialClientApplication;
        
        public IdentityClientAdapter(IConfidentialClientApplication confidentialClientApplication)
        {
            _confidentialClientApplication = confidentialClientApplication ??
                                             throw new ArgumentNullException(nameof(confidentialClientApplication));
        }

        public async Task<string> GetAccessTokenOnBehalfOfAsync(string currentAccessToken, string scopes)
        {
            try
            {
                var userAssertion = new UserAssertion(currentAccessToken, Common.Constants.Authentication.AzureAuthenticationAssertionType);
                var requestedScopes = new Collection<string> { scopes };
                var result = await _confidentialClientApplication.AcquireTokenOnBehalfOf(requestedScopes, userAssertion).ExecuteAsync();
                return result.AccessToken;
            }
            catch (MsalException exception)
            {
                throw new OnBehalfOfTokenClientException($"Failed to acquire onBehalfOf token. Exception: {exception.Message}");
            }
        }
    }
}
