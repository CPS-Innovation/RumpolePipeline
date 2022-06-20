using System;
using Azure;
using Azure.Core.Serialization;
using Azure.Search.Documents;
using Microsoft.Extensions.Options;

namespace text_extractor.Factories
{
	public class SearchClientFactory : ISearchClientFactory
	{
        private readonly Domain.SearchClientOptions _options;

        public SearchClientFactory(IOptions<Domain.SearchClientOptions> options)
        {
            _options = options.Value;
        }

		public SearchClient Create()
        {
            return new SearchClient(
                new Uri(_options.EndpointUrl),
                _options.IndexName,
                new AzureKeyCredential(_options.AuthorizationKey),
                new SearchClientOptions { Serializer = new NewtonsoftJsonObjectSerializer() });
        }
	}
}

