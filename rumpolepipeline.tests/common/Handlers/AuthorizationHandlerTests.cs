using System.Net.Http.Headers;
using System.Security.Claims;
using AutoFixture;
using common.Handlers;
using FluentAssertions;
using Moq;
using Xunit;

namespace rumpolepipeline.tests.common.Handlers
{
    public class AuthorizationHandlerTests
    {
        private readonly HttpRequestHeaders _httpRequestHeaders;

        private readonly Mock<ClaimsPrincipal> _mockClaimsPrincipal;

        private readonly IAuthorizationHandler _authorizationHandler;

        public AuthorizationHandlerTests()
        {
            var fixture = new Fixture();
            var httpRequestMessage = new HttpRequestMessage();
            _httpRequestHeaders = httpRequestMessage.Headers;
            var claim = fixture.Create<string>();
            fixture.Create<string>();

            _mockClaimsPrincipal = new Mock<ClaimsPrincipal>();

            _httpRequestHeaders.Add("Authorization", $"Bearer {fixture.Create<string>()}");
            _mockClaimsPrincipal.Setup(principal => principal.Claims).Returns(new List<Claim> { new Claim("testType", claim) });

            _authorizationHandler = new AuthorizationHandler(claim);
        }

        [Fact]
        public void IsAuthorized_ReturnsFalseWhenAuthorizationHeaderIsMissing()
        {
            _httpRequestHeaders.Clear();

            var isAuthorized = _authorizationHandler.IsAuthorized(_httpRequestHeaders, _mockClaimsPrincipal.Object, out _);

            isAuthorized.Should().BeFalse();
        }

        //TODO add back in once posh test gallery is back online
        //[Fact]
        //public void IsAuthorized_ReturnsFalseWhenClaimIsNotFound()
        //{
        //    _mockClaimsPrincipal.Setup(principal => principal.Claims).Returns(new List<Claim>());

        //    var isAuthorized = AuthorizationHandler.IsAuthorized(_httpRequestHeaders, _mockClaimsPrincipal.Object, out _errorMessage);

        //    isAuthorized.Should().BeFalse();
        //}

        [Fact]
        public void IsAuthorized_ReturnsTrue()
        {
            var isAuthorized = _authorizationHandler.IsAuthorized(_httpRequestHeaders, _mockClaimsPrincipal.Object, out _);

            isAuthorized.Should().BeTrue();
        }
    }
}

