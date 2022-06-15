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

namespace text_extractor.Services.SearchIndexService
{
    public class SearchIndexService : ISearchIndexService
    {
        private readonly SearchIndexOptions _searchIndexOptions;
        private readonly SearchIndexClient _indexClient;

        public SearchIndexService(IOptions<SearchIndexOptions> indexOptions)
        {
            _searchIndexOptions = indexOptions.Value;

            _indexClient = new SearchIndexClient(
                new Uri(_searchIndexOptions.EndpointUrl),
                new AzureKeyCredential(_searchIndexOptions.AuthorizationKey),
                new SearchClientOptions { Serializer = new NewtonsoftJsonObjectSerializer() });

        }

        public async Task StoreResults(AnalyzeResults analyzeresults, int caseId, string documentId)
        {
            var lines = new List<SearchLine>();

            foreach (var readResult in analyzeresults.ReadResults)
            {
                //TODO search line factory with tests
                lines.AddRange(readResult.Lines.Select((line, index) => new SearchLine
                {
                    Id = $"{caseId}-{documentId}-{readResult.Page}-{index}",
                    CaseId = caseId,
                    DocumentId = documentId,
                    PageIndex = readResult.Page,
                    LineIndex = index,
                    Language = line.Language,
                    BoundingBox = line.BoundingBox,
                    Appearance = line.Appearance,
                    Text = line.Text,
                    Words = line.Words
                }));
            }

            var searchClient = _indexClient.GetSearchClient(_searchIndexOptions.IndexName);

            //TODO search indexing factory
            using var indexer = new SearchIndexingBufferedSender<SearchLine>(searchClient, new SearchIndexingBufferedSenderOptions<SearchLine>
            {
                KeyFieldAccessor = searchLine => searchLine.Id
            });

            indexer.ActionFailed += async (arg) =>
            {
                //TODO what to do here? just log?
                var exception = arg.Exception == null ? "No exception" : arg.Exception.Message;
                var result = arg.Result == null ? "No result" : "Result";
                await Console.Out.WriteLineAsync($"Failed {exception}, {result}");
            };

            await indexer.UploadDocumentsAsync(lines);
            await indexer.FlushAsync();
        }
    }
}
