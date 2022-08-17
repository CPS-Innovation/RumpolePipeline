using AutoFixture;
using FluentAssertions;
using Microsoft.Azure.CognitiveServices.Vision.ComputerVision;
using Microsoft.Extensions.Options;
using Moq;
using text_extractor.Domain;
using text_extractor.Factories;
using Xunit;

namespace rumpolepipeline.tests.text_extractor.Factories
{
	public class ComputerVisionClientFactoryTests
	{
        private readonly ComputerVisionClientOptions _computerVisionClientOptions;

        private readonly IComputerVisionClientFactory _computerVisionClientFactory;

		public ComputerVisionClientFactoryTests()
		{
            var fixture = new Fixture();
			_computerVisionClientOptions = fixture.Create<ComputerVisionClientOptions>();

			var mockComputerVisionClientOptions = new Mock<IOptions<ComputerVisionClientOptions>>();

			mockComputerVisionClientOptions.Setup(options => options.Value).Returns(_computerVisionClientOptions);

			_computerVisionClientFactory = new ComputerVisionClientFactory(mockComputerVisionClientOptions.Object);
		}

        [Fact]
		public void Create_ReturnsComputerVisionClient()
        {
			var client = _computerVisionClientFactory.Create();

			client.Should().BeOfType<ComputerVisionClient>();
        }

		[Fact]
		public void Create_SetsExpectedEndpointUrl()
		{
			var client = _computerVisionClientFactory.Create();

			client.Endpoint.Should().Be(_computerVisionClientOptions.ServiceUrl);
		}
	}
}

