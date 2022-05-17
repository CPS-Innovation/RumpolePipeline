using System.Net.Http.Headers;
using System.Security.Claims;

namespace pdf_generator.Handlers
{
    public interface IAuthorizationHandler
    {
        bool IsAuthorized(HttpRequestHeaders headers, ClaimsPrincipal claimsPrincipal, out string errorMessage);
    }
}

