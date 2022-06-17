using AutoFixture;
using FluentAssertions;
using Microsoft.Azure.CognitiveServices.Vision.ComputerVision;
using Microsoft.Extensions.Options;
using Moq;
using text_extractor.Factories;
using text_extractor.Services.OcrService;
using Xunit;

namespace text_extractor.tests.Factories
{
	public class ComputerVisionClientFactoryTests
	{
		private Fixture _fixture;
		private OcrOptions _ocrOptions;

		private Mock<IOptions<OcrOptions>> _mockOcrOptions;

		private IComputerVisionClientFactory ComputerVisionClientFactory;

		public ComputerVisionClientFactoryTests()
		{
			_fixture = new Fixture();
			_ocrOptions = _fixture.Create<OcrOptions>();

			_mockOcrOptions = new Mock<IOptions<OcrOptions>>();

			_mockOcrOptions.Setup(options => options.Value).Returns(_ocrOptions);

			ComputerVisionClientFactory = new ComputerVisionClientFactory(_mockOcrOptions.Object);
		}

        [Fact]
		public void Create_ReturnsComputerVisionClient()
        {
			var client = ComputerVisionClientFactory.Create();

			client.Should().BeOfType<ComputerVisionClient>();
        }

		[Fact]
		public void Create_SetsExpectedEndpointUrl()
		{
			var client = ComputerVisionClientFactory.Create();

			client.Endpoint.Should().Be(_ocrOptions.ServiceUrl);
		}
	}
}

