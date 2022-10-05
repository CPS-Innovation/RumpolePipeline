using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using Common.Domain.Exceptions;
using Microsoft.Identity.Client;

namespace Common.Adapters
{
    public class IdentityClientAdapter : IIdentityClientAdapter
    {
        private readonly IConfidentialClientApplication _confidentialClientApplication;
        
        public IdentityClientAdapter(IConfidentialClientApplication confidentialClientApplication)
        {
            _confidentialClientApplication = confidentialClientApplication ??
                                             throw new ArgumentNullException(nameof(confidentialClientApplication));
        }

        public async Task<string> GetAccessTokenOnBehalfOfAsync(string currentAccessToken, string scopes, Guid correlationId)
        {
            try
            {
                var userAssertion = new UserAssertion(currentAccessToken, Constants.Authentication.AzureAuthenticationAssertionType);
                var requestedScopes = new Collection<string> { scopes };
                var result = await _confidentialClientApplication.AcquireTokenOnBehalfOf(requestedScopes, userAssertion).WithCorrelationId(correlationId).ExecuteAsync();
                return result.AccessToken;
            }
            catch (MsalException exception)
            {
                throw new OnBehalfOfTokenClientException($"Failed to acquire onBehalfOf token. Exception: {exception.Message}");
            }
        }

        public async Task<string> GetClientAccessTokenAsync(string scopes, Guid correlationId)
        {
            try
            {
                var requestedScopes = new Collection<string> { scopes };
                var result = await _confidentialClientApplication.AcquireTokenForClient(requestedScopes).WithCorrelationId(correlationId).ExecuteAsync();
                return result.AccessToken;
            }
            catch (MsalUiRequiredException ex)
            {
                throw new ClientTokenException($"Failed to acquire a client token. Insufficient permissions. Exception: {ex.Message}");
            }
            catch (MsalServiceException ex) when (ex.Message.Contains("AADSTS70011"))
            {
                throw new ClientTokenException($"Failed to acquire a client token. Invalid scope. Exception: {ex.Message}");
            }
            catch (Exception ex)
            {
                throw new ClientTokenException($"Failed to acquire a client token. Exception: {ex.Message}");
            }
        }
    }
}
