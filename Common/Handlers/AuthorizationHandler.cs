using System.Linq;
using System.Net.Http.Headers;
using System.Security.Claims;

namespace common.Handlers
{
    public class AuthorizationHandler : IAuthorizationHandler
    {
        private readonly string _claim;

        public AuthorizationHandler(string claim)
        {
            _claim = claim;
        }

        public bool IsAuthorized(HttpRequestHeaders headers, ClaimsPrincipal claimsPrincipal, out string errorMessage)
        {
            if(!headers.TryGetValues("Authorization", out var values) ||
                    string.IsNullOrWhiteSpace(values.FirstOrDefault()))
            {
                errorMessage = "No authorization token supplied.";
                return false;
            }

            //TODO add back in once posh test gallery is back online
            //if (claimsPrincipal.Claims.SingleOrDefault(c => c.Value.Equals(_claim, StringComparison.OrdinalIgnoreCase)) == null)
            //{
            //    errorMessage = $"Claim '{_claim}' not found";
            //    return false;
            //}

            errorMessage = null;
            return true;
        }
    }
}

