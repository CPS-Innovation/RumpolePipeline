using System;
using System.Threading.Tasks;
using Common.Domain.DocumentExtraction;

namespace coordinator.TempService
{
    public interface IBlobStorageService
    {
        Task<Case> GetDocumentsAsync(string caseId, Guid correlationId);
    }
}
