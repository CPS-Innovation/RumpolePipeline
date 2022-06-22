using System.Net.Http.Headers;
using System.Security.Claims;

namespace common.Handlers
{
    public interface IAuthorizationHandler
    {
        bool IsAuthorized(HttpRequestHeaders headers, ClaimsPrincipal claimsPrincipal, out string errorMessage);
    }
}

