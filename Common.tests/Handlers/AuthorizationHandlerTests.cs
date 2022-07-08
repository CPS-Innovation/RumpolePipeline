using System.Net.Http.Headers;
using System.Security.Claims;
using AutoFixture;
using common.Handlers;
using FluentAssertions;
using Moq;
using Xunit;

namespace common.tests.Handlers
{
    public class AuthorizationHandlerTests
    {
        private Fixture _fixture;
        private HttpRequestMessage _httpRequestMessage;
        private HttpRequestHeaders _httpRequestHeaders;
        private string _claim;
        private string _errorMessage;

        private Mock<ClaimsPrincipal> _mockClaimsPrincipal;

        private IAuthorizationHandler AuthorizationHandler;

        public AuthorizationHandlerTests()
        {
            _fixture = new Fixture();
            _httpRequestMessage = new HttpRequestMessage();
            _httpRequestHeaders = _httpRequestMessage.Headers;
            _claim = _fixture.Create<string>();
            _errorMessage = _fixture.Create<string>();

            _mockClaimsPrincipal = new Mock<ClaimsPrincipal>();

            _httpRequestHeaders.Add("Authorization", $"Bearer {_fixture.Create<string>()}");
            _mockClaimsPrincipal.Setup(principal => principal.Claims).Returns(new List<Claim> { new Claim("testType", _claim) });

            AuthorizationHandler = new AuthorizationHandler(_claim);
        }

        [Fact]
        public void IsAuthorized_ReturnsFalseWhenAuthorizationHeaderIsMissing()
        {
            _httpRequestHeaders.Clear();

            var isAuthorized = AuthorizationHandler.IsAuthorized(_httpRequestHeaders, _mockClaimsPrincipal.Object, out _errorMessage);

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
            var isAuthorized = AuthorizationHandler.IsAuthorized(_httpRequestHeaders, _mockClaimsPrincipal.Object, out _errorMessage);

            isAuthorized.Should().BeTrue();
        }
    }
}

