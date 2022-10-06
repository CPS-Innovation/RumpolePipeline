using System;
using System.Net.Http;
using System.Threading.Tasks;
using AutoFixture;
using coordinator.Domain;
using coordinator.Factories;
using coordinator.Functions.ActivityFunctions;
using FluentAssertions;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace coordinator.tests.Functions.ActivityFunctions
{
    public class CreateTextExtractorHttpRequestTests
    {
        private readonly DurableHttpRequest _durableRequest;

        private readonly Mock<IDurableActivityContext> _mockDurableActivityContext;

        private readonly CreateTextExtractorHttpRequest _createTextExtractorHttpRequest;

        public CreateTextExtractorHttpRequestTests()
        {
            var fixture = new Fixture();
            var payload = fixture.Create<CreateTextExtractorHttpRequestActivityPayload>();
            _durableRequest = new DurableHttpRequest(HttpMethod.Post, new Uri("https://www.test.co.uk"));

            var mockTextExtractorHttpFactory = new Mock<ITextExtractorHttpRequestFactory>();
            _mockDurableActivityContext = new Mock<IDurableActivityContext>();

            _mockDurableActivityContext.Setup(context => context.GetInput<CreateTextExtractorHttpRequestActivityPayload>())
                .Returns(payload);

            mockTextExtractorHttpFactory.Setup(client => client.Create(payload.CaseId, payload.DocumentId, payload.BlobName, payload.CorrelationId))
                .ReturnsAsync(_durableRequest);

            var mockLogger = new Mock<ILogger<CreateTextExtractorHttpRequest>>();
            _createTextExtractorHttpRequest = new CreateTextExtractorHttpRequest(mockTextExtractorHttpFactory.Object, mockLogger.Object);
        }

        [Fact]
        public async Task Run_ThrowsWhenPayloadIsNull()
        {
            _mockDurableActivityContext.Setup(context => context.GetInput<CreateTextExtractorHttpRequestActivityPayload>())
                .Returns(default(CreateTextExtractorHttpRequestActivityPayload));

            await Assert.ThrowsAsync<ArgumentException>(() => _createTextExtractorHttpRequest.Run(_mockDurableActivityContext.Object));
        }

        [Fact]
        public async Task Run_ReturnsDurableRequest()
        {
            var durableRequest = await _createTextExtractorHttpRequest.Run(_mockDurableActivityContext.Object);

            durableRequest.Should().Be(_durableRequest);
        }
    }
}
