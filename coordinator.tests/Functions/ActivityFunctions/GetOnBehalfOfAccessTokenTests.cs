using System;
using System.Threading.Tasks;
using AutoFixture;
using coordinator.Domain.Adapters;
using coordinator.Functions.ActivityFunctions;
using FluentAssertions;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Extensions.Configuration;
using Moq;
using Xunit;

namespace coordinator.tests.Functions.ActivityFunctions
{
    public class GetOnBehalfOfAccessTokenTests
    {
        private Fixture _fixture;
        private string _accessToken;
        private string _onBehalfOfAccessToken;

        private Mock<IIdentityClientAdapter> _identityClientAdapterMock;
        private Mock<IDurableActivityContext> _mockDurableActivityContext;
        private Mock<IConfiguration> _mockConfiguration;

        private GetOnBehalfOfAccessToken GetOnBehalfOfAccessToken;

        public GetOnBehalfOfAccessTokenTests()
        {
            _fixture = new Fixture();
            _accessToken = _fixture.Create<string>();
            _onBehalfOfAccessToken = _fixture.Create<string>();

            _identityClientAdapterMock = new Mock<IIdentityClientAdapter>();
            _mockDurableActivityContext = new Mock<IDurableActivityContext>();
            _mockConfiguration = new Mock<IConfiguration>();

            _mockDurableActivityContext.Setup(context => context.GetInput<string>())
                .Returns(_accessToken);

            _identityClientAdapterMock.Setup(client => client.GetAccessTokenOnBehalfOfAsync(_accessToken, It.IsAny<string>()))
                .ReturnsAsync(_onBehalfOfAccessToken);

            GetOnBehalfOfAccessToken = new GetOnBehalfOfAccessToken(_identityClientAdapterMock.Object, _mockConfiguration.Object);
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
