using System;
using Azure;
using Azure.Core.Serialization;
using Azure.Search.Documents;
using Azure.Search.Documents.Indexes;
using Microsoft.Extensions.Options;
using text_extractor.Services.SearchIndexService;

namespace text_extractor.Factories
{
	public class SearchIndexClientFactory : ISearchIndexClientFactory
	{
        private readonly SearchIndexOptions _options;

        public SearchIndexClientFactory(IOptions<SearchIndexOptions> options)
        {
            _options = options.Value;
        }

		public SearchIndexClient Create()
        {
            return new SearchIndexClient(
                new Uri(_options.EndpointUrl),
                new AzureKeyCredential(_options.AuthorizationKey),
                new SearchClientOptions { Serializer = new NewtonsoftJsonObjectSerializer() });
        }
	}
}

