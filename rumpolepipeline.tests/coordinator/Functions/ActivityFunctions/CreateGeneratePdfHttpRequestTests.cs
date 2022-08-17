using AutoFixture;
using coordinator.Domain;
using coordinator.Factories;
using coordinator.Functions.ActivityFunctions;
using FluentAssertions;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Moq;
using Xunit;

namespace rumpolepipeline.tests.coordinator.Functions.ActivityFunctions
{
    public class CreateGeneratePdfHttpRequestTests
    {
        private readonly DurableHttpRequest _durableRequest;

        private readonly Mock<IDurableActivityContext> _mockDurableActivityContext;

        private readonly CreateGeneratePdfHttpRequest _createGeneratePdfHttpRequest;

        public CreateGeneratePdfHttpRequestTests()
        {
            var fixture = new Fixture();
            var payload = fixture.Create<CreateGeneratePdfHttpRequestActivityPayload>();
            _durableRequest = new DurableHttpRequest(HttpMethod.Post, new Uri("https://www.test.co.uk"));

            var mockGeneratePdfHttpFactory = new Mock<IGeneratePdfHttpRequestFactory>();
            _mockDurableActivityContext = new Mock<IDurableActivityContext>();

            _mockDurableActivityContext.Setup(context => context.GetInput<CreateGeneratePdfHttpRequestActivityPayload>())
                .Returns(payload);

            mockGeneratePdfHttpFactory.Setup(client => client.Create(payload.CaseId, payload.DocumentId, payload.FileName))
                .ReturnsAsync(_durableRequest);

            _createGeneratePdfHttpRequest = new CreateGeneratePdfHttpRequest(mockGeneratePdfHttpFactory.Object);
        }

        [Fact]
        public async Task Run_ThrowsWhenPayloadIsNull()
        {
            _mockDurableActivityContext.Setup(context => context.GetInput<CreateGeneratePdfHttpRequestActivityPayload>())
                .Returns(default(CreateGeneratePdfHttpRequestActivityPayload)!);

            await Assert.ThrowsAsync<ArgumentException>(() => _createGeneratePdfHttpRequest.Run(_mockDurableActivityContext.Object));
        }

        [Fact]
        public async Task Run_ReturnsDurableRequest()
        {
            var durableRequest = await _createGeneratePdfHttpRequest.Run(_mockDurableActivityContext.Object);

            durableRequest.Should().Be(_durableRequest);
        }
    }
}
