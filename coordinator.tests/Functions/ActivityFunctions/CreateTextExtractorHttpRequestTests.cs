using System;
using System.Net.Http;
using System.Threading.Tasks;
using AutoFixture;
using coordinator.Domain;
using coordinator.Factories;
using coordinator.Functions.ActivityFunctions;
using FluentAssertions;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Moq;
using Xunit;

namespace coordinator.tests.Functions.ActivityFunctions
{
    public class CreateTextExtractorHttpRequestTests
    {
        private Fixture _fixture;
        private CreateTextExtractorHttpRequestActivityPayload _payload;
        private DurableHttpRequest _durableRequest;

        private Mock<ITextExtractorHttpRequestFactory> _mockTextExtractorHttpFactory;
        private Mock<IDurableActivityContext> _mockDurableActivityContext;

        private CreateTextExtractorHttpRequest CreateTextExtractorHttpRequest;

        public CreateTextExtractorHttpRequestTests()
        {
            _fixture = new Fixture();
            _payload = _fixture.Create<CreateTextExtractorHttpRequestActivityPayload>();
            _durableRequest = new DurableHttpRequest(HttpMethod.Post, new Uri("https://www.test.co.uk"));

            _mockTextExtractorHttpFactory = new Mock<ITextExtractorHttpRequestFactory>();
            _mockDurableActivityContext = new Mock<IDurableActivityContext>();

            _mockDurableActivityContext.Setup(context => context.GetInput<CreateTextExtractorHttpRequestActivityPayload>())
                .Returns(_payload);

            _mockTextExtractorHttpFactory.Setup(client => client.Create(_payload.CaseId, _payload.DocumentId, _payload.BlobName))
                .ReturnsAsync(_durableRequest);

            CreateTextExtractorHttpRequest = new CreateTextExtractorHttpRequest(_mockTextExtractorHttpFactory.Object);
        }

        [Fact]
        public async Task Run_ThrowsWhenPayloadIsNull()
        {
            _mockDurableActivityContext.Setup(context => context.GetInput<CreateTextExtractorHttpRequestActivityPayload>())
                .Returns(default(CreateTextExtractorHttpRequestActivityPayload));

            await Assert.ThrowsAsync<ArgumentException>(() => CreateTextExtractorHttpRequest.Run(_mockDurableActivityContext.Object));
        }

        [Fact]
        public async Task Run_ReturnsDurableRequest()
        {
            var durableRequest = await CreateTextExtractorHttpRequest.Run(_mockDurableActivityContext.Object);

            durableRequest.Should().Be(_durableRequest);
        }
    }
}
