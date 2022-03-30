using System;
using System.Threading.Tasks;
using AutoFixture;
using coordinator.Clients;
using coordinator.Functions.ActivityFunctions;
using FluentAssertions;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Moq;
using Xunit;

namespace coordinator.tests.Functions.ActivityFunctions
{
    public class GetOnBehalfOfAccessTokenTests
    {
        private Fixture _fixture;
        private string _accessToken;
        private string _onBehalfOfAccessToken;

        private Mock<IOnBehalfOfTokenClient> _mockOnBehalfOfAccessTokenClient;
        private Mock<IDurableActivityContext> _mockDurableActivityContext;

        private GetOnBehalfOfAccessToken GetOnBehalfOfAccessToken;

        public GetOnBehalfOfAccessTokenTests()
        {
            _fixture = new Fixture();
            _accessToken = _fixture.Create<string>();
            _onBehalfOfAccessToken = _fixture.Create<string>();

            _mockOnBehalfOfAccessTokenClient = new Mock<IOnBehalfOfTokenClient>();
            _mockDurableActivityContext = new Mock<IDurableActivityContext>();

            _mockDurableActivityContext.Setup(context => context.GetInput<string>())
                .Returns(_accessToken);

            _mockOnBehalfOfAccessTokenClient.Setup(client => client.GetAccessTokenAsync(_accessToken))
                .ReturnsAsync(_onBehalfOfAccessToken);

            GetOnBehalfOfAccessToken = new GetOnBehalfOfAccessToken(_mockOnBehalfOfAccessTokenClient.Object);
        }

        [Fact]
        public async Task Run_ThrowsWhenAccessTokenPayloadIsNull()
        {
            _mockDurableActivityContext.Setup(context => context.GetInput<string>())
                .Returns(default(string));

            await Assert.ThrowsAsync<ArgumentException>(() => GetOnBehalfOfAccessToken.Run(_mockDurableActivityContext.Object));
        }

        [Fact]
        public async Task Run_ReturnsAccessToken()
        {
            var caseDetails = await GetOnBehalfOfAccessToken.Run(_mockDurableActivityContext.Object);

            caseDetails.Should().Be(_onBehalfOfAccessToken);
        }
    }
}
