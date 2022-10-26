using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Aspose.Email.Clients.Exchange.WebService.Schema_2016;
using Aspose.Imaging.MemoryManagement;
using Azure.Search.Documents;
using Common.Constants;
using Common.Domain.DocumentEvaluation;
using Common.Domain.Extensions;
using Common.Logging;
using DnsClient.Internal;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using pdf_generator.Domain.SearchResults;
using pdf_generator.Factories;

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

            var containerName = _configuration["BlobServiceContainerName"];
            _logger.LogMethodFlow(correlationId, nameof(SearchForDocumentsAsync), $"{searchLines.Count} documents found in the index");

            foreach (var line in searchLines)
            {
                var blobName = $"{line.CaseId}/pdfs/{line.DocumentId}.pdf";
                var newWrapper = new DocumentInformation
                {
                    DocumentMetadata = new Dictionary<string, string>(),
                    BlobName = blobName.EscapeUriDataStringRfc3986(),
                    BlobContainerName = containerName
                };

                newWrapper.DocumentMetadata.Add(DocumentTags.CaseId, line.CaseId.ToString());
                newWrapper.DocumentMetadata.Add(DocumentTags.DocumentId, line.DocumentId);
                newWrapper.DocumentMetadata.Add(DocumentTags.LastUpdatedDate, line.LastUpdatedDate);
                newWrapper.DocumentMetadata.Add(DocumentTags.MaterialId, line.MaterialId);

                documentsFound.Add(newWrapper);
            }

            _logger.LogMethodExit(correlationId, nameof(SearchForDocumentsAsync), documentsFound.ToJson());
            return documentsFound;
        }
    }
}
