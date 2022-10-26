using System;
using Azure.Search.Documents;
using Common.Domain.DocumentEvaluation;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace pdf_generator.Services.SearchService
{
    public interface ISearchServiceProcessor
    {
        Task<List<DocumentInformation>> SearchForDocumentsAsync(SearchOptions searchOptions, Guid correlationId);
    }
}
