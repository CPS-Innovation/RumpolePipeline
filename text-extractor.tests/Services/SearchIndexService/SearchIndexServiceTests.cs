using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using AutoFixture;
using Azure.Search.Documents;
using Azure.Search.Documents.Indexes;
using Microsoft.Azure.CognitiveServices.Vision.ComputerVision.Models;
using Microsoft.Extensions.Options;
using Moq;
using text_extractor.Domain;
using text_extractor.Factories;
using text_extractor.Services.SearchIndexService;
using Xunit;

namespace text_extractor.tests.Services.SearchIndexService
{
	public class SearchIndexServiceTests
	{
		private Fixture _fixture;
		private Domain.SearchClientOptions _searchClientOptions;
		private AnalyzeResults _analyzeResults;
		private int _caseId;
		private string _documentId;
		private SearchLine _searchLine;

		private Mock<IOptions<Domain.SearchClientOptions>> _mockSearchClientOptions;
		private Mock<ISearchClientFactory> _mockSearchClientFactory;
		private Mock<ISearchLineFactory> _mockSearchLineFactory;
		private Mock<ISearchIndexingBufferedSenderFactory> _mockSearchIndexingBufferedSenderFactory;
		private Mock<SearchClient> _mockSearchClient;
		private Mock<SearchIndexingBufferedSender<SearchLine>> _mockSearchIndexingBufferedSender;

		private ISearchIndexService SearchIndexService;

		public SearchIndexServiceTests()
		{
			_fixture = new Fixture();
            _searchClientOptions = _fixture.Create<Domain.SearchClientOptions>();
			_analyzeResults = new AnalyzeResults();//TODO
			_caseId = _fixture.Create<int>();
			_documentId = _fixture.Create<string>();
			_searchLine = _fixture.Create<SearchLine>();

            _mockSearchClientOptions = new Mock<IOptions<Domain.SearchClientOptions>>();
			_mockSearchClientFactory = new Mock<ISearchClientFactory>();
			_mockSearchLineFactory = new Mock<ISearchLineFactory>();
			_mockSearchIndexingBufferedSenderFactory = new Mock<ISearchIndexingBufferedSenderFactory>();
			_mockSearchClient = new Mock<SearchClient>();
			_mockSearchIndexingBufferedSender = new Mock<SearchIndexingBufferedSender<SearchLine>>();

			_mockSearchClientOptions.Setup(options => options.Value).Returns(_searchClientOptions);
			_mockSearchClientFactory.Setup(factory => factory.Create()).Returns(_mockSearchClient.Object);
			_mockSearchLineFactory.Setup(factory => factory.Create(_caseId, _documentId, It.IsAny<ReadResult>(), It.IsAny<Line>(), It.IsAny<int>())) //TODO read result, line and index
				.Returns(_searchLine);
			_mockSearchIndexingBufferedSenderFactory.Setup(factory => factory.Create(_mockSearchClient.Object))
				.Returns(_mockSearchIndexingBufferedSender.Object);

			SearchIndexService = new text_extractor.Services.SearchIndexService.SearchIndexService(
				_mockSearchClientFactory.Object, _mockSearchLineFactory.Object, _mockSearchIndexingBufferedSenderFactory.Object);
		}

        [Fact]
        public async Task StoreResultsAsync_UploadsDocuments()
        {
			await SearchIndexService.StoreResultsAsync(_analyzeResults, _caseId, _documentId);

			_mockSearchIndexingBufferedSender
				.Verify(sender => sender.UploadDocumentsAsync(It.IsAny<IEnumerable<SearchLine>>(), It.IsAny<CancellationToken>()));
			//TODO searchline
        }

		[Fact]
		public async Task StoreResultsAsync_Flushes()
		{
			await SearchIndexService.StoreResultsAsync(_analyzeResults, _caseId, _documentId);

			_mockSearchIndexingBufferedSender.Verify(sender => sender.FlushAsync(It.IsAny<CancellationToken>()));
		}
	}
}

