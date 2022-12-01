using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Azure.Search.Documents;
using Common.Domain.DocumentEvaluation;

namespace Common.Services.SearchService.Contracts
{
    public interface ISearchServiceProcessor
    {
        Task<List<DocumentInformation>> SearchForDocumentsAsync(SearchOptions searchOptions, Guid correlationId);
    }
}
