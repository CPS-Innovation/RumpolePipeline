using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Azure.Search.Documents;
using Common.Domain.DocumentEvaluation;
using Common.Domain.Extensions;
using Common.Factories.Contracts;
using Common.Logging;
using Microsoft.Extensions.Logging;
using pdf_generator.Domain.SearchResults;

namespace pdf_generator.Services.SearchService
{
    public class SearchServiceProcessor : ISearchServiceProcessor
    {
        private readonly ILogger<SearchServiceProcessor> _logger;
        private readonly SearchClient _searchClient;
        
        public SearchServiceProcessor(ILogger<SearchServiceProcessor> logger, ISearchClientFactory searchClientFactory)
        {
            _logger = logger;
            _searchClient = searchClientFactory.Create();
        }

        public async Task<List<DocumentInformation>> SearchForDocumentsAsync(SearchOptions searchOptions, Guid correlationId)
        {
            _logger.LogMethodEntry(correlationId, nameof(SearchForDocumentsAsync), searchOptions.ToJson());

            var documentsFound = new List<DocumentInformation>();
            var searchResults = await _searchClient.SearchAsync<SearchLine>("*", searchOptions);

            var searchLines = new List<SearchLine>();
            await foreach (var searchResult in searchResults.Value.GetResultsAsync())
            {
                if (searchResult.Document != null && searchLines.Find(sl => sl.Id == searchResult.Document.Id && sl.VersionId == searchResult.Document.VersionId) == null)
                    searchLines.Add(searchResult.Document);
            }

            if (searchLines.Count == 0)
            {
                _logger.LogMethodFlow(correlationId, nameof(SearchForDocumentsAsync), "No documents found in the index");
                return documentsFound;
            }

            _logger.LogMethodFlow(correlationId, nameof(SearchForDocumentsAsync), $"{searchLines.Count} documents found in the index");

            documentsFound.AddRange(searchLines.Select(line => new DocumentInformation {CaseId = line.CaseId, DocumentId = line.DocumentId, VersionId = line.VersionId, FileName = line.FileName}));

            _logger.LogMethodExit(correlationId, nameof(SearchForDocumentsAsync), string.Empty);
            return documentsFound;
        }
    }
}
