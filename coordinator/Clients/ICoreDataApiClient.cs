using coordinator.Domain.CoreDataApi;
using System.Threading.Tasks;

namespace coordinator.Clients
{
    public interface ICoreDataApiClient
    {
        Task<CaseDetails> GetCaseDetailsById(int caseId, string accessToken);
    }
}
