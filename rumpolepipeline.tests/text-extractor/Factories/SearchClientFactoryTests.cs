using AutoFixture;
using Azure.Search.Documents;
using FluentAssertions;
using Microsoft.Extensions.Options;
using Moq;
using text_extractor.Factories;
using Xunit;

namespace rumpolepipeline.tests.text_extractor.Factories
{
	public class SearchClientFactoryTests
	{
        private readonly global::text_extractor.Domain.SearchClientOptions _searchClientOptions;

        private readonly ISearchClientFactory _searchClientFactory;

		public SearchClientFactoryTests()
		{
            var fixture = new Fixture();
			_searchClientOptions = fixture.Build<global::text_extractor.Domain.SearchClientOptions>()
									.With(o => o.EndpointUrl, "https://www.google.co.uk")
									.Create();

            var mockSearchClientOptions = new Mock<IOptions<global::text_extractor.Domain.SearchClientOptions>>();

			mockSearchClientOptions.Setup(options => options.Value).Returns(_searchClientOptions);

			_searchClientFactory = new SearchClientFactory(mockSearchClientOptions.Object);
		}

		[Fact]
		public void Create_ReturnsSearchClient()
		{
			var searchClient = _searchClientFactory.Create();

			searchClient.Should().BeOfType<SearchClient>();
		}

		[Fact]
		public void Create_SetsExpectedEndpoint()
		{
			var searchClient = _searchClientFactory.Create();

			searchClient.Endpoint.Should().Be(_searchClientOptions.EndpointUrl);
		}

		[Fact]
		public void Create_SetsExpectedIndexName()
		{
			var searchClient = _searchClientFactory.Create();

			searchClient.IndexName.Should().Be(_searchClientOptions.IndexName);
		}
	}
}

