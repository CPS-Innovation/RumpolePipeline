using System;
using System.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Azure.CognitiveServices.Vision.ComputerVision.Models;
using text_extractor.Domain;
using text_extractor.Factories;
using Azure.Search.Documents;
using Azure;
using Common.Factories.Contracts;
using Common.Logging;
using Microsoft.Extensions.Logging;

namespace text_extractor.Services.SearchIndexService
{
    public class SearchIndexService : ISearchIndexService
    {
        private readonly SearchClient _searchClient;
        private readonly ISearchLineFactory _searchLineFactory;
        private readonly ISearchIndexingBufferedSenderFactory _searchIndexingBufferedSenderFactory;
        private readonly ILogger<SearchIndexService> _logger;

        public SearchIndexService(
            ISearchClientFactory searchClientFactory,
            ISearchLineFactory searchLineFactory,
            ISearchIndexingBufferedSenderFactory searchIndexingBufferedSenderFactory,
            ILogger<SearchIndexService> logger)
        {
            _searchClient = searchClientFactory.Create();
            _searchLineFactory = searchLineFactory;
            _searchIndexingBufferedSenderFactory = searchIndexingBufferedSenderFactory;
            _logger = logger;
        }

        public async Task StoreResultsAsync(AnalyzeResults analyzeResults, long caseId, string documentId, long versionId, Guid correlationId)
        {
            _logger.LogMethodEntry(correlationId, nameof(StoreResultsAsync), $"CaseId: {caseId}, DocumentId: {documentId}");
            
            _logger.LogMethodFlow(correlationId, nameof(StoreResultsAsync), "Building search line results");
            var lines = new List<SearchLine>();
            foreach (var readResult in analyzeResults.ReadResults)
            {
                lines.AddRange(readResult.Lines.Select((line, index) =>
                                    _searchLineFactory.Create(caseId, documentId, versionId, readResult, line, index)));
            }

            _logger.LogMethodFlow(correlationId, nameof(StoreResultsAsync), "Beginning search index update");
            await using var indexer = _searchIndexingBufferedSenderFactory.Create(_searchClient);

            var indexTaskCompletionSource = new TaskCompletionSource<bool>();

            var failureCount = 0;
            indexer.ActionFailed += _ =>
            {
                failureCount++;
                if (!indexTaskCompletionSource.Task.IsCompleted)
                {
                    indexTaskCompletionSource.SetResult(false);
                }

                return Task.CompletedTask;
            };

            var successCount = 0;
            indexer.ActionCompleted += _ =>
            {
                successCount++;
                if (successCount == lines.Count)
                {
                    indexTaskCompletionSource.SetResult(true);
                }
                
                return Task.CompletedTask;
            };

            await indexer.UploadDocumentsAsync(lines);
            await indexer.FlushAsync();
            _logger.LogMethodFlow(correlationId, nameof(StoreResultsAsync), $"Updating the search index completed - number of lines: {lines.Count}, successes: {successCount}, failures: {failureCount}");

            if (!await indexTaskCompletionSource.Task)
            {
                throw new RequestFailedException("At least one indexing action failed.");
            }
        }

        public async Task RemoveResultsForDocumentAsync(long caseId, string documentId, Guid correlationId)
        {
            _logger.LogMethodEntry(correlationId, nameof(RemoveResultsForDocumentAsync), $"CaseId: {caseId}, DocumentId: {documentId}");

            if (caseId == 0)
                throw new ArgumentException("Invalid argument", nameof(caseId));

            if (string.IsNullOrWhiteSpace(documentId))
                throw new ArgumentException("Invalid document identifier", nameof(documentId));
            
            var searchOptions = new SearchOptions
            {
                Filter = $"caseId eq {caseId} and documentId eq '{documentId}'"
            };
            var results = await _searchClient.SearchAsync<SearchLine>("*", searchOptions);
            var searchLines = new List<SearchLine>();
            await foreach (var searchResult in results.Value.GetResultsAsync())
            {
                searchLines.Add(searchResult.Document);
            }

            if (searchLines.Count == 0)
            {
                _logger.LogMethodFlow(correlationId, nameof(RemoveResultsForDocumentAsync), "No results found - all documents for this case have been previously removed");
            }
            else
            {
                await using var indexer = _searchIndexingBufferedSenderFactory.Create(_searchClient);
                var indexTaskCompletionSource = new TaskCompletionSource<bool>();

                var failureCount = 0;
                indexer.ActionFailed += _ =>
                {
                    failureCount++;
                    if (!indexTaskCompletionSource.Task.IsCompleted)
                    {
                        indexTaskCompletionSource.SetResult(false);
                    }

                    return Task.CompletedTask;
                };

                var successCount = 0;
                indexer.ActionCompleted += _ =>
                {
                    successCount++;
                    if (successCount == searchLines.Count)
                    {
                        indexTaskCompletionSource.SetResult(true);
                    }
                
                    return Task.CompletedTask;
                };

                await indexer.DeleteDocumentsAsync(searchLines);
                await indexer.FlushAsync();
                _logger.LogMethodFlow(correlationId, nameof(StoreResultsAsync),
                    $"Updating the search index completed following a deletion request for caseId '{caseId}' and documentId '{documentId}' - number of lines: {searchLines.Count}, successes: {successCount}, failures: {failureCount}");

                if (!await indexTaskCompletionSource.Task)
                {
                    throw new RequestFailedException("At least one indexing action failed.");
                }
            }
            
            _logger.LogMethodFlow(correlationId, nameof(RemoveResultsForDocumentAsync), "Beginning search index update");
        }
    }
}
