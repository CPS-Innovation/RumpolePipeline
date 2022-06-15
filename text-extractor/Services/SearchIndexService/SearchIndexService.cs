using System.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Azure.CognitiveServices.Vision.ComputerVision.Models;
using Microsoft.Extensions.Options;
using System;
using Azure.Search.Documents.Indexes;
using Azure;
using Azure.Search.Documents;
using Azure.Core.Serialization;
using text_extractor.Domain;
using text_extractor.Factories;

namespace text_extractor.Services.SearchIndexService
{
    public class SearchIndexService : ISearchIndexService
    {
        private readonly SearchIndexOptions _searchIndexOptions;
        private readonly SearchIndexClient _searchIndexClient;
        private readonly ISearchLineFactory _searchLineFactory;
        private readonly ISearchIndexingBufferedSenderFactory _searchIndexingBufferedSenderFactory;

        public SearchIndexService(
            ISearchIndexClientFactory searchIndexClientFactory,
            ISearchLineFactory searchLineFactory,
            ISearchIndexingBufferedSenderFactory searchIndexingBufferedSenderFactory)
        {
            _searchIndexClient = searchIndexClientFactory.Create();
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

            var searchClient = _searchIndexClient.GetSearchClient(_searchIndexOptions.IndexName);
            using var indexer = _searchIndexingBufferedSenderFactory.Create(searchClient);

            var failCount = 0;
            indexer.ActionFailed += async (arg) =>
            {
                failCount++;
                //TODO what to do here? just log? fail completely if all fail? Speak to Stef
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
