using System.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Azure.CognitiveServices.Vision.ComputerVision.Models;
using System;
using text_extractor.Domain;
using text_extractor.Factories;
using Azure.Search.Documents;

namespace text_extractor.Services.SearchIndexService
{
    public class SearchIndexService : ISearchIndexService
    {
        private readonly SearchIndexOptions _searchIndexOptions;
        private readonly SearchClient _searchClient;
        private readonly ISearchLineFactory _searchLineFactory;
        private readonly ISearchIndexingBufferedSenderFactory _searchIndexingBufferedSenderFactory;

        public SearchIndexService(
            ISearchClientFactory searchClientFactory,
            ISearchLineFactory searchLineFactory,
            ISearchIndexingBufferedSenderFactory searchIndexingBufferedSenderFactory)
        {
            _searchClient = searchClientFactory.Create();
            _searchLineFactory = searchLineFactory;
            _searchIndexingBufferedSenderFactory = searchIndexingBufferedSenderFactory;
        }

        public async Task StoreResultsAsync(AnalyzeResults analyzeResults, int caseId, string documentId)
        {
            var lines = new List<SearchLine>();
            foreach (var readResult in analyzeResults.ReadResults)
            {
                lines.AddRange(readResult.Lines.Select((line, index) =>
                                    _searchLineFactory.Create(caseId, documentId, readResult, line, index)));
            }

            using var indexer = _searchIndexingBufferedSenderFactory.Create(_searchClient);

            var failCount = 0;
            indexer.ActionFailed += async (arg) =>
            {
                failCount++;
                //TODO what to do here? just log? fail completely if all fail?
                var exception = arg.Exception == null ? "No exception" : arg.Exception.Message;
                var result = arg.Result == null ? "No result" : "Result";
                await Console.Out.WriteLineAsync($"Failed {exception}, {result}");
                //if (failCount == lines.Count)
                //{
                //    throw new RequestFailedException("All search index actions failed.");
                //}
            };

            await indexer.UploadDocumentsAsync(lines);
            await indexer.FlushAsync();
        }
    }
}
