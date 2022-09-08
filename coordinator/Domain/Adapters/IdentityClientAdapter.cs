using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Identity.Client;

namespace coordinator.Domain.Adapters
{
    public class IdentityClientAdapter : IIdentityClientAdapter
    {
        private readonly IConfidentialClientApplication _confidentialClientApplication;

        public IdentityClientAdapter(IConfidentialClientApplication confidentialClientApplication)
        {
            _confidentialClientApplication = confidentialClientApplication;
        }

        public async Task<string> GetAccessTokenAsync(IEnumerable<string> scopes)
        {
            var tokenBuilder = _confidentialClientApplication.AcquireTokenForClient(scopes);
            var token = await tokenBuilder.ExecuteAsync();
            return token.AccessToken;
        }
    }
}
