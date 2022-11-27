using Common.Domain.DocumentEvaluation;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Common.Logging;
using Azure.Search.Documents;

namespace pdf_generator.Services.SearchService
{
    public class SearchService : ISearchService
    {
        private readonly ILogger<SearchService> _logger;
        private readonly ISearchServiceProcessor _searchServiceProcessor;
        
        public SearchService(ILogger<SearchService> logger, ISearchServiceProcessor searchServiceProcessor)
        {
            _logger = logger;
            _searchServiceProcessor = searchServiceProcessor;
        }

        public async Task<List<DocumentInformation>> ListDocumentsForCaseAsync(string caseId, Guid correlationId)
        {
            _logger.LogMethodEntry(correlationId, nameof(ListDocumentsForCaseAsync), caseId);

            if (string.IsNullOrWhiteSpace(caseId))
                throw new ArgumentNullException(nameof(caseId));
            
            var searchOptions = new SearchOptions
            {
                Filter = $"caseId eq {caseId}"
            };
            searchOptions.OrderBy.Add("id");

            var documentsFound = await _searchServiceProcessor.SearchForDocumentsAsync(searchOptions, correlationId);

            _logger.LogMethodExit(correlationId, nameof(ListDocumentsForCaseAsync), string.Empty);
            return documentsFound;
        }

        public async Task<DocumentInformation> FindDocumentForCaseAsync(string caseId, string documentId, Guid correlationId)
        {
            _logger.LogMethodEntry(correlationId, nameof(FindDocumentForCaseAsync), caseId);

            if (string.IsNullOrWhiteSpace(caseId))
                throw new ArgumentNullException(nameof(caseId));

            if (string.IsNullOrWhiteSpace(documentId))
                throw new ArgumentNullException(documentId);

            var searchOptions = new SearchOptions
            {
                Filter = $"caseId eq {caseId} and documentId eq '{documentId}'"
            };
            searchOptions.OrderBy.Add("id");

            var documentsFound = await _searchServiceProcessor.SearchForDocumentsAsync(searchOptions, correlationId);

            _logger.LogMethodExit(correlationId, nameof(FindDocumentForCaseAsync), string.Empty);
            return documentsFound.FirstOrDefault();
        }
    }
}
