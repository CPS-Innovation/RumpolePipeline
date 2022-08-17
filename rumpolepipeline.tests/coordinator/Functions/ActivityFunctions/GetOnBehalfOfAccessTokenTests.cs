using AutoFixture;
using coordinator.Clients;
using coordinator.Functions.ActivityFunctions;
using FluentAssertions;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Moq;
using Xunit;

namespace rumpolepipeline.tests.coordinator.Functions.ActivityFunctions
{
    public class GetOnBehalfOfAccessTokenTests
    {
        private readonly string _onBehalfOfAccessToken;

        private readonly Mock<IDurableActivityContext> _mockDurableActivityContext;

        private readonly GetOnBehalfOfAccessToken _getOnBehalfOfAccessToken;

        public GetOnBehalfOfAccessTokenTests()
        {
            var fixture = new Fixture();
            var accessToken = fixture.Create<string>();
            _onBehalfOfAccessToken = fixture.Create<string>();

            var mockOnBehalfOfAccessTokenClient = new Mock<IOnBehalfOfTokenClient>();
            _mockDurableActivityContext = new Mock<IDurableActivityContext>();

            _mockDurableActivityContext.Setup(context => context.GetInput<string>())
                .Returns(accessToken);

            mockOnBehalfOfAccessTokenClient.Setup(client => client.GetAccessTokenAsync(accessToken))
                .ReturnsAsync(_onBehalfOfAccessToken);

            _getOnBehalfOfAccessToken = new GetOnBehalfOfAccessToken(mockOnBehalfOfAccessTokenClient.Object);
        }

        [Fact]
        public async Task Run_ThrowsWhenAccessTokenPayloadIsNull()
        {
            _mockDurableActivityContext.Setup(context => context.GetInput<string>())
                .Returns(default(string)!);

            await Assert.ThrowsAsync<ArgumentException>(() => _getOnBehalfOfAccessToken.Run(_mockDurableActivityContext.Object));
        }

        [Fact]
        public async Task Run_ReturnsAccessToken()
        {
            var caseDetails = await _getOnBehalfOfAccessToken.Run(_mockDurableActivityContext.Object);

            caseDetails.Should().Be(_onBehalfOfAccessToken);
        }
    }
}
