using System;
using System.Threading.Tasks;
using AutoFixture;
using Common.Adapters;
using Common.Domain.Requests;
using coordinator.Functions.ActivityFunctions;
using FluentAssertions;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace coordinator.tests.Functions.ActivityFunctions
{
    public class GetOnBehalfOfAccessTokenTests
    {
        private readonly string _onBehalfOfAccessToken;
        private readonly Guid _correlationId;
        private readonly string _accessToken;

        private readonly Mock<IDurableActivityContext> _mockDurableActivityContext;

        private readonly GetOnBehalfOfAccessToken _getOnBehalfOfAccessToken;

        public GetOnBehalfOfAccessTokenTests()
        {
            var fixture = new Fixture();
            
            _accessToken = fixture.Create<string>();
            _onBehalfOfAccessToken = fixture.Create<string>();
            _correlationId = fixture.Create<Guid>();

            var identityClientAdapterMock = new Mock<IIdentityClientAdapter>();
            _mockDurableActivityContext = new Mock<IDurableActivityContext>();
            var mockConfiguration = new Mock<IConfiguration>();

            _mockDurableActivityContext.Setup(context => context.GetInput<GetOnBehalfOfTokenRequest>())
                .Returns(new GetOnBehalfOfTokenRequest { AccessToken = _accessToken, CorrelationId = _correlationId });

            identityClientAdapterMock.Setup(client => client.GetAccessTokenOnBehalfOfAsync(_accessToken, It.IsAny<string>(), It.IsAny<Guid>()))
                .ReturnsAsync(_onBehalfOfAccessToken);

            var mockLogger = new Mock<ILogger<GetOnBehalfOfAccessToken>>();
            _getOnBehalfOfAccessToken = new GetOnBehalfOfAccessToken(identityClientAdapterMock.Object, mockConfiguration.Object, mockLogger.Object);
        }

        [Fact]
        public async Task Run_ThrowsWhenAccessToken_InPayload_IsNull()
        {
            _mockDurableActivityContext.Setup(context => context.GetInput<GetOnBehalfOfTokenRequest>())
                .Returns(new GetOnBehalfOfTokenRequest { AccessToken = default, CorrelationId = _correlationId });

            await Assert.ThrowsAsync<ArgumentException>(() => _getOnBehalfOfAccessToken.Run(_mockDurableActivityContext.Object));
        }
        
        [Fact]
        public async Task Run_ThrowsWhenCorrelationId_InPayload_IsNull()
        {
            _mockDurableActivityContext.Setup(context => context.GetInput<GetOnBehalfOfTokenRequest>())
                .Returns(new GetOnBehalfOfTokenRequest { AccessToken = _accessToken, CorrelationId = default });

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
