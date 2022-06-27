using System;
using System.Net.Http;
using System.Threading.Tasks;
using AutoFixture;
using coordinator.Clients;
using coordinator.Domain;
using coordinator.Factories;
using coordinator.Functions.ActivityFunctions;
using FluentAssertions;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Moq;
using Xunit;

namespace coordinator.tests.Functions.ActivityFunctions
{
    public class CreateGeneratePdfHttpRequestTests
    {
        private Fixture _fixture;
        private CreateGeneratePdfHttpRequestActivityPayload _payload;
        private DurableHttpRequest _durableRequest;

        private Mock<IGeneratePdfHttpRequestFactory> _mockGeneratePdfHttpFactory;
        private Mock<IDurableActivityContext> _mockDurableActivityContext;

        private CreateGeneratePdfHttpRequest CreateGeneratePdfHttpRequest;

        public CreateGeneratePdfHttpRequestTests()
        {
            _fixture = new Fixture();
            _payload = _fixture.Create<CreateGeneratePdfHttpRequestActivityPayload>();
            _durableRequest = new DurableHttpRequest(HttpMethod.Post, new Uri("https://www.test.co.uk"));

            _mockGeneratePdfHttpFactory = new Mock<IGeneratePdfHttpRequestFactory>();
            _mockDurableActivityContext = new Mock<IDurableActivityContext>();

            _mockDurableActivityContext.Setup(context => context.GetInput<CreateGeneratePdfHttpRequestActivityPayload>())
                .Returns(_payload);

            _mockGeneratePdfHttpFactory.Setup(client => client.Create(_payload.CaseId, _payload.DocumentId, _payload.FileName))
                .ReturnsAsync(_durableRequest);

            CreateGeneratePdfHttpRequest = new CreateGeneratePdfHttpRequest(_mockGeneratePdfHttpFactory.Object);
        }

        [Fact]
        public async Task Run_ThrowsWhenPayloadIsNull()
        {
            _mockDurableActivityContext.Setup(context => context.GetInput<CreateGeneratePdfHttpRequestActivityPayload>())
                .Returns(default(CreateGeneratePdfHttpRequestActivityPayload));

            await Assert.ThrowsAsync<ArgumentException>(() => CreateGeneratePdfHttpRequest.Run(_mockDurableActivityContext.Object));
        }

        [Fact]
        public async Task Run_ReturnsDurableRequest()
        {
            var durableRequest = await CreateGeneratePdfHttpRequest.Run(_mockDurableActivityContext.Object);

            durableRequest.Should().Be(_durableRequest);
        }
    }
}
