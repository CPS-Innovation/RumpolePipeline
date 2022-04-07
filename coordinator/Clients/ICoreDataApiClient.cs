using coordinator.Domain.CoreDataApi;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace coordinator.Clients
{
    public interface ICoreDataApiClient
    {
        Task<List<Document>> GetCaseDocumentsByIdAsync(int caseId, string accessToken);
    }
}
