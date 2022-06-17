using AutoFixture;
using Azure.Search.Documents;
using FluentAssertions;
using Microsoft.Extensions.Options;
using Moq;
using text_extractor.Factories;
using text_extractor.Services.SearchIndexService;
using Xunit;

namespace text_extractor.tests.Factories
{
	public class SearchClientFactoryTests
	{
		private Fixture _fixture;
		private SearchIndexOptions _searchIndexOptions;

		private Mock<IOptions<SearchIndexOptions>> _mockSearchIndexOptions;

		private ISearchClientFactory SearchClientFactory;

		public SearchClientFactoryTests()
		{
			_fixture = new Fixture();
			_searchIndexOptions = _fixture.Build<SearchIndexOptions>()
									.With(o => o.EndpointUrl, "https://www.google.co.uk")
									.Create();

			_mockSearchIndexOptions = new Mock<IOptions<SearchIndexOptions>>();

			_mockSearchIndexOptions.Setup(options => options.Value).Returns(_searchIndexOptions);

			SearchClientFactory = new SearchClientFactory(_mockSearchIndexOptions.Object);
		}

		[Fact]
		public void Create_ReturnsSearchClient()
		{
			var searchClient = SearchClientFactory.Create();

			searchClient.Should().BeOfType<SearchClient>();
		}

		[Fact]
		public void Create_SetsExpectedEndpoint()
		{
			var searchClient = SearchClientFactory.Create();

			searchClient.Endpoint.Should().Be(_searchIndexOptions.EndpointUrl);
		}

		[Fact]
		public void Create_SetsExpectedIndexName()
		{
			var searchClient = SearchClientFactory.Create();

			searchClient.IndexName.Should().Be(_searchIndexOptions.IndexName);
		}
	}
}

