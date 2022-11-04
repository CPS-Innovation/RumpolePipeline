using Common.Domain.DocumentEvaluation;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace pdf_generator.Services.SearchService
{
    public interface ISearchService
    {
        Task<List<DocumentInformation>> ListDocumentsForCaseAsync(string caseId, Guid correlationId);

        Task<DocumentInformation> FindDocumentForCaseAsync(string caseId, string documentId, Guid correlationId);
    }
}
