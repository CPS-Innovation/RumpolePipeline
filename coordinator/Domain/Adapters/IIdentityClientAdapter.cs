using System.Threading.Tasks;

namespace coordinator.Domain.Adapters
{
    public interface IIdentityClientAdapter
    {
        Task<string> GetAccessTokenAsync(string tenantId, string clientId, string clientSecret, string scopes);

        Task<string> GetAccessTokenOnBehalfOfAsync(string currentAccessToken, string tenantId, string clientId,
            string clientSecret, string scopes);
    }
}
