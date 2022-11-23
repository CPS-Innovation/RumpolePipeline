using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Azure.Search.Documents;
using Common.Constants;
using Common.Domain.DocumentEvaluation;
using Common.Domain.Extensions;
using Common.Factories.Contracts;
using Common.Logging;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using pdf_generator.Domain.SearchResults;

namespace pdf_generator.Services.SearchService
{
    public class SearchServiceProcessor : ISearchServiceProcessor
    {
        private readonly ILogger<SearchServiceProcessor> _logger;
        private readonly SearchClient _searchClient;
        private readonly IConfiguration _configuration;

        public SearchServiceProcessor(ILogger<SearchServiceProcessor> logger, IConfiguration configuration, ISearchClientFactory searchClientFactory)
        {
            _logger = logger;
            _configuration = configuration;
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
                if (searchResult.Document != null && searchLines.Find(sl => sl.Id == searchResult.Document.Id) == null)
                    searchLines.Add(searchResult.Document);
            }

            if (searchLines.Count == 0)
            {
                _logger.LogMethodFlow(correlationId, nameof(SearchForDocumentsAsync), "No documents found in the index");
                return documentsFound;
            }

            var containerName = _configuration[ConfigKeys.SharedKeys.BlobServiceContainerName];
            _logger.LogMethodFlow(correlationId, nameof(SearchForDocumentsAsync), $"{searchLines.Count} documents found in the index");

            foreach (var line in searchLines)
            {
                var blobName = $"{line.CaseId}/pdfs/{line.DocumentId}.pdf";
                var blobNameEncoded = blobName.UrlEncodeString();
                if (documentsFound.FindIndex(x => x.BlobName == blobNameEncoded) != -1) continue;
                
                var newWrapper = new DocumentInformation
                {
                    DocumentMetadata = new Dictionary<string, string>(),
                    BlobName = blobNameEncoded,
                    BlobContainerName = containerName
                };

                newWrapper.DocumentMetadata.Add(DocumentTags.CaseId, line.CaseId.ToString());
                newWrapper.DocumentMetadata.Add(DocumentTags.DocumentId, line.DocumentId);
                newWrapper.DocumentMetadata.Add(DocumentTags.VersionId, line.VersionId.ToString());

                documentsFound.Add(newWrapper);
            }

            _logger.LogMethodExit(correlationId, nameof(SearchForDocumentsAsync), documentsFound.ToJson());
            return documentsFound;
        }
    }
}
