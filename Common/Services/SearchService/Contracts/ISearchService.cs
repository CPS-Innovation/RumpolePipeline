using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Common.Domain.DocumentEvaluation;

namespace Common.Services.SearchService.Contracts
{
    public interface ISearchService
    {
        Task<List<DocumentInformation>> ListDocumentsForCaseAsync(string caseId, Guid correlationId);

        Task<DocumentInformation> FindDocumentForCaseAsync(string caseId, string documentId, Guid correlationId);
    }
}
