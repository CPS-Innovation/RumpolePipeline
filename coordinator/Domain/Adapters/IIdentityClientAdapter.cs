using System.Threading.Tasks;

namespace coordinator.Domain.Adapters
{
    public interface IIdentityClientAdapter
    {
        Task<string> GetAccessTokenOnBehalfOfAsync(string currentAccessToken, string scopes);
    }
}
