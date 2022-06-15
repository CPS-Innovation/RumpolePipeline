using System;
using System.Threading;
using System.Threading.Tasks;
using AutoFixture;
using FluentAssertions;
using Microsoft.Azure.CognitiveServices.Vision.ComputerVision;
using Microsoft.Azure.CognitiveServices.Vision.ComputerVision.Models;
using Moq;
using text_extractor.Domain.Exceptions;
using text_extractor.Factories;
using text_extractor.Services.OcrService;
using text_extractor.Services.SasGeneratorService;
using Xunit;

namespace text_extractor.tests.Services.OcrService
{
	public class OcrServiceTests
	{
		private Fixture _fixture;
		private string _blobName;
		private string _sasLink;
		private ReadHeaders _readHeaders;
		private string _operationId;
		private ReadOperationResult _readOperationResult;
		private AnalyzeResults _analyzeResults;

		private Mock<IComputerVisionClientFactory> _mockComputerVisionClientFactory;
		private Mock<ComputerVisionClient> _mockComputerVisionClient;
		private Mock<ISasGeneratorService> _mockSasGeneratorService;

		private IOcrService OcrService;

		public OcrServiceTests()
		{
			_fixture = new Fixture();
			_blobName = _fixture.Create<string>();
			_sasLink = _fixture.Create<string>();
			_operationId = "169B3BDD-9FB2-440C-8ECB-14F94A4BFC32";
			_readHeaders = new ReadHeaders
			{
				OperationLocation = $"Test:{_operationId}"
			};
			_analyzeResults = new AnalyzeResults();
			_readOperationResult = new ReadOperationResult
			{
				AnalyzeResult = _analyzeResults,
				Status = OperationStatusCodes.Succeeded
			};

			_mockComputerVisionClientFactory = new Mock<IComputerVisionClientFactory>();
			_mockComputerVisionClient = new Mock<ComputerVisionClient>();
			_mockSasGeneratorService = new Mock<ISasGeneratorService>();

			_mockComputerVisionClientFactory.Setup(factory => factory.Create()).Returns(_mockComputerVisionClient.Object);
			_mockSasGeneratorService.Setup(service => service.GenerateSasUrlAsync(_blobName)).ReturnsAsync(_sasLink);
			_mockComputerVisionClient.Setup(client => client.ReadAsync(_sasLink, null, null, "latest", It.IsAny<CancellationToken>()))
				.ReturnsAsync(_readHeaders);
			_mockComputerVisionClient.Setup(client => client.GetReadResultAsync(It.Is<Guid>(g => g.Equals(Guid.Parse(_operationId))), It.IsAny<CancellationToken>()))
				.ReturnsAsync(_readOperationResult);

			OcrService = new text_extractor.Services.OcrService.OcrService(_mockComputerVisionClientFactory.Object, _mockSasGeneratorService.Object);
		}

        [Fact]
		public async Task GetOcrResultsAsync_ReturnsExpectedResults()
        {
			var results = OcrService.GetOcrResultsAsync(_blobName);

			results.Should().Be(_analyzeResults);
        }

		[Fact]
		public async Task GetOcrResultsAsync_DelaysWhenResultsStatusIsRunning()
		{

		}

		[Fact]
		public async Task GetOcrResultsAsync_DelaysWhenResultsStatusIsNotStarted()
		{

		}

		[Fact]
		public async Task GetOcrResultsAsync_ThrowsOcrServiceExceptionWhenExceptionOccurs()
		{
			_mockComputerVisionClient.Setup(client => client.ReadAsync(_sasLink, null, null, "latest", It.IsAny<CancellationToken>()))
				.ThrowsAsync(new Exception());

			await Assert.ThrowsAsync<OcrServiceException>(() => OcrService.GetOcrResultsAsync(_blobName));
		}
	}
}

