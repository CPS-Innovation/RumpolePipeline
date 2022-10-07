using System;
using System.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Azure.CognitiveServices.Vision.ComputerVision.Models;
using text_extractor.Domain;
using text_extractor.Factories;
using Azure.Search.Documents;
using Azure;
using Azure.Search.Documents.Indexes.Models;
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

        public async Task StoreResultsAsync(AnalyzeResults analyzeResults, int caseId, string documentId, Guid correlationId)
        {
            _logger.LogMethodEntry(correlationId, nameof(StoreResultsAsync), $"CaseId: {caseId}, DocumentId: {documentId}");
            
            _logger.LogMethodFlow(correlationId, nameof(StoreResultsAsync), "Building search line results");
            var lines = new List<SearchLine>();
            foreach (var readResult in analyzeResults.ReadResults)
            {
                lines.AddRange(readResult.Lines.Select((line, index) =>
                                    _searchLineFactory.Create(caseId, documentId, readResult, line, index)));
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
    }
}
