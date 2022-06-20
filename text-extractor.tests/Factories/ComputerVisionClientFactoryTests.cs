using AutoFixture;
using FluentAssertions;
using Microsoft.Azure.CognitiveServices.Vision.ComputerVision;
using Microsoft.Extensions.Options;
using Moq;
using text_extractor.Domain;
using text_extractor.Factories;
using Xunit;

namespace text_extractor.tests.Factories
{
	public class ComputerVisionClientFactoryTests
	{
		private Fixture _fixture;
		private ComputerVisionClientOptions _computerVisionClientOptions;

		private Mock<IOptions<ComputerVisionClientOptions>> _mockComputerVisionClientOptions;

		private IComputerVisionClientFactory ComputerVisionClientFactory;

		public ComputerVisionClientFactoryTests()
		{
			_fixture = new Fixture();
			_computerVisionClientOptions = _fixture.Create<ComputerVisionClientOptions>();

			_mockComputerVisionClientOptions = new Mock<IOptions<ComputerVisionClientOptions>>();

			_mockComputerVisionClientOptions.Setup(options => options.Value).Returns(_computerVisionClientOptions);

			ComputerVisionClientFactory = new ComputerVisionClientFactory(_mockComputerVisionClientOptions.Object);
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

			client.Endpoint.Should().Be(_computerVisionClientOptions.ServiceUrl);
		}
	}
}

