using System.Collections.ObjectModel;
using System.Threading.Tasks;
using coordinator.Domain.Exceptions;
using Microsoft.Identity.Client;

namespace coordinator.Domain.Adapters
{
    public class IdentityClientAdapter : IIdentityClientAdapter
    {
        public IdentityClientAdapter()
        {
        }

        public async Task<string> GetAccessTokenAsync(string tenantId, string clientId, string clientSecret, string scopes)
        {
            try
            {
                var confidentialClientApplication = BuildConfidentialClient(tenantId, clientId, clientSecret);
                var requestedScopes = new Collection<string> { scopes };
                var tokenBuilder = confidentialClientApplication.AcquireTokenForClient(requestedScopes);
                var token = await tokenBuilder.ExecuteAsync();
                return token.AccessToken;
            }
            catch (MsalException exception)
            {
                throw new OnBehalfOfTokenClientException($"Failed to acquire service-to-service access token. Exception: {exception.Message}");
            }
        }

        public async Task<string> GetAccessTokenOnBehalfOfAsync(string currentAccessToken, string tenantId, string clientId, string clientSecret, string scopes)
        {
            try
            {
                var confidentialClientApplication = BuildConfidentialClient(tenantId, clientId, clientSecret);
                var userAssertion = new UserAssertion(currentAccessToken, Common.Constants.Authentication.AzureAuthenticationAssertionType);
                var requestedScopes = new Collection<string> { scopes };
                var result = await confidentialClientApplication.AcquireTokenOnBehalfOf(requestedScopes, userAssertion).ExecuteAsync();
                return result.AccessToken;
            }
            catch (MsalException exception)
            {
                throw new OnBehalfOfTokenClientException($"Failed to acquire onBehalfOf token. Exception: {exception.Message}");
            }
        }

        private static IConfidentialClientApplication BuildConfidentialClient(string tenantId, string clientId, string clientSecret)
        {
            const string instance = Common.Constants.Authentication.AzureAuthenticationInstanceUrl;
            var appOptions = new ConfidentialClientApplicationOptions
            {
                Instance = instance,
                TenantId = tenantId,
                ClientId = clientId,
                ClientSecret = clientSecret
            };

            var authority = $"{instance}{tenantId}/";

            return ConfidentialClientApplicationBuilder.CreateWithApplicationOptions(appOptions).WithAuthority(authority).Build();
        }
    }
}
