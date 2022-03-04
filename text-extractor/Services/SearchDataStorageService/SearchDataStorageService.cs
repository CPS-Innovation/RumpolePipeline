using System.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;
using Domain;
using Microsoft.Azure.CognitiveServices.Vision.ComputerVision.Models;
using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Options;
using System;
using Azure.Search.Documents.Indexes;
using Azure;
using Azure.Search.Documents;
using Azure.Core.Serialization;

namespace Services.SearchDataStorageService
{
    public class SearchDataStorageService
    {
        private readonly SearchDataStorageOptions _storageOptions;
        private readonly SearchDataIndexOptions _indexOptions;
        private readonly CosmosClient _cosmosClient;
        private readonly SearchIndexClient _indexClient;

        public SearchDataStorageService(IOptions<SearchDataStorageOptions> storageOptions, IOptions<SearchDataIndexOptions> indexOptions)
        {
            _storageOptions = storageOptions.Value;
            _indexOptions = indexOptions.Value;
            _cosmosClient = new CosmosClient(_storageOptions.EndpointUrl, _storageOptions.AuthorizationKey, new CosmosClientOptions()
            {
                SerializerOptions = new CosmosSerializationOptions()
                {
                    PropertyNamingPolicy = CosmosPropertyNamingPolicy.CamelCase
                },
                AllowBulkExecution = true,
                MaxRetryAttemptsOnRateLimitedRequests = 20
            });

            _indexClient = new SearchIndexClient(
                new Uri(_indexOptions.EndpointUrl),
                new AzureKeyCredential(_indexOptions.AuthorizationKey),
                new SearchClientOptions { Serializer = new NewtonsoftJsonObjectSerializer() });

        }

        public async Task StoreResults(AnalyzeResults analyzeresults, int caseId, int documentId, string transactionId)
        {
            if (_storageOptions.Enabled)
            {
                await StoreResultsCosmosDb(analyzeresults, caseId, documentId, transactionId);
            }
            if (_indexOptions.Enabled)
            {
                await StoreResultsIndex(analyzeresults, caseId, documentId, transactionId);
            }
        }

        public async Task StoreResultsIndex(AnalyzeResults analyzeresults, int caseId, int documentId, string transactionId)
        {
            var lines = new List<SearchLine>();

            foreach (var readResult in analyzeresults.ReadResults)
            {
                lines.AddRange(readResult.Lines.Select((line, index) => new SearchLine
                {
                    Id = $"{caseId}-{documentId}-{readResult.Page}-{index}",
                    CaseId = caseId,
                    DocumentId = documentId.ToString(),
                    PageIndex = readResult.Page,
                    LineIndex = index,
                    Language = line.Language,
                    BoundingBox = line.BoundingBox,
                    Appearance = line.Appearance,
                    Text = line.Text,
                    Words = line.Words,
                    TransactionId = transactionId
                }));
            }

            var searchClient = _indexClient.GetSearchClient(_indexOptions.IndexName);

            using var indexer = new SearchIndexingBufferedSender<SearchLine>(searchClient, new SearchIndexingBufferedSenderOptions<SearchLine>
            {
                KeyFieldAccessor = searchLine => searchLine.Id
            });

            await indexer.UploadDocumentsAsync(lines);
            await indexer.FlushAsync();
        }

        private async Task StoreResultsCosmosDb(AnalyzeResults analyzeresults, int caseId, int documentId, string transactionId)
        {
            var container = _cosmosClient.GetContainer(_storageOptions.DatabaseName, _storageOptions.ContainerName);
            var tasks = new List<Task>();

            tasks.Add(Upsert(container, new SearchDocument
            {
                CaseId = caseId,
                Id = $"{caseId}-{documentId}",
                ModelVersion = analyzeresults.ModelVersion,
                Version = analyzeresults.Version,
                ReadResults = analyzeresults.ReadResults,
                TransactionId = transactionId
            }, caseId));

            await Task.WhenAll(tasks);
        }

        private Task Upsert<T>(Container container, T item, int caseId)
        {
            return container.UpsertItemAsync(item, new PartitionKey(caseId)).ContinueWith(itemResponse =>
        {
            if (!itemResponse.IsCompletedSuccessfully)
            {
                AggregateException innerExceptions = itemResponse.Exception.Flatten();
                if (innerExceptions.InnerExceptions.FirstOrDefault(innerEx => innerEx is CosmosException) is CosmosException cosmosException)
                {
                    Console.WriteLine($"CosmosDb - Received {(int)cosmosException.StatusCode}: {cosmosException.StatusCode} ({cosmosException.Message}).");
                }
                else
                {
                    Console.WriteLine($"CosmosDb - Exception {innerExceptions.InnerExceptions.FirstOrDefault()}.");
                }
            }
        });
        }
    }
}
