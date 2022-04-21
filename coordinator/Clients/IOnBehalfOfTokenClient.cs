using System.Threading.Tasks;

namespace coordinator.Clients
{
    public interface IOnBehalfOfTokenClient
    {
        Task<string> GetAccessTokenAsync(string accessToken);
    }
}
