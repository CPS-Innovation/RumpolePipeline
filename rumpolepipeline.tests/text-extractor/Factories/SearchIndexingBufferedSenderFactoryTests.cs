using Azure.Search.Documents;
using FluentAssertions;
using Moq;
using text_extractor.Domain;
using text_extractor.Factories;
using Xunit;

namespace rumpolepipeline.tests.text_extractor.Factories
{
	public class SearchIndexingBufferedSenderFactoryTests
	{
		[Fact]
		public void Create_ReturnsSearchIndexBufferedSender()
        {
			var searchClient = new Mock<SearchClient>();
			var factory = new SearchIndexingBufferedSenderFactory();

			var sender = factory.Create(searchClient.Object);

			sender.Should().BeOfType<SearchIndexingBufferedSender<SearchLine>>();
        }
	}
}

