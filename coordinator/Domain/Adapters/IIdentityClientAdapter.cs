using System.Collections.Generic;
using System.Threading.Tasks;

namespace coordinator.Domain.Adapters
{
    public interface IIdentityClientAdapter
    {
        Task<string> GetAccessTokenAsync(IEnumerable<string> scopes);
    }
}
