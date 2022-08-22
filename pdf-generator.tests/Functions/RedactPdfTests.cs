using AutoFixture;
using common.Handlers;
using common.Wrappers;
using Moq;
using System.Security.Claims;
using pdf_generator.Functions;
using System.Net.Http;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using common.Domain.Exceptions;
using FluentAssertions;
using Newtonsoft.Json;
using pdf_generator.Domain.Requests;
using pdf_generator.Domain.Responses;
using pdf_generator.Handlers;
using pdf_generator.Services.DocumentRedactionService;
using Xunit;
using System;

namespace pdf_generator.tests.Functions
{
    public class RedactPdfTests
    {
        private readonly Fixture _fixture = new();
        private readonly string _serializedRedactPdfResponse;
        private HttpRequestMessage _httpRequestMessage;
        private readonly Mock<IAuthorizationHandler> _mockAuthorizationHandler;
        private readonly Mock<ClaimsPrincipal> _mockClaimsPrincipal;
        private readonly Mock<IJsonConvertWrapper> _mockJsonConvertWrapper;
        private readonly Mock<IExceptionHandler> _mockExceptionHandler;

        private readonly RedactPdf _redactPdf;

        public RedactPdfTests()
        {
            var errorMessage = _fixture.Create<string>();
            var request = _fixture.Create<RedactPdfRequest>();

            var serializedRedactPdfRequest = JsonConvert.SerializeObject(request);
            _httpRequestMessage = new HttpRequestMessage()
            {
                Content = new StringContent(serializedRedactPdfRequest, Encoding.UTF8, "application/json")
            };
            _serializedRedactPdfResponse = _fixture.Create<string>();

            _mockAuthorizationHandler = new Mock<IAuthorizationHandler>();
            _mockClaimsPrincipal = new Mock<ClaimsPrincipal>();
            _mockJsonConvertWrapper = new Mock<IJsonConvertWrapper>();
            _mockExceptionHandler = new Mock<IExceptionHandler>();
            var mockDocumentRedactionService = new Mock<IDocumentRedactionService>();

            _mockAuthorizationHandler.Setup(handler => handler.IsAuthorized(_httpRequestMessage.Headers, _mockClaimsPrincipal.Object, out errorMessage))
                .Returns(true);
            _mockJsonConvertWrapper.Setup(wrapper => wrapper.SerializeObject(It.IsAny<RedactPdfResponse>()))
                .Returns(_serializedRedactPdfResponse);

            mockDocumentRedactionService.Setup(x => x.RedactPdfAsync(It.IsAny<RedactPdfRequest>(), It.IsAny<string>())).ReturnsAsync(_fixture.Create<RedactPdfResponse>());

            _redactPdf = new RedactPdf(_mockAuthorizationHandler.Object, _mockExceptionHandler.Object,
                _mockJsonConvertWrapper.Object, mockDocumentRedactionService.Object);
        }

        [Fact]
        public async Task Run_ReturnsUnauthorizedWhenUnauthorized()
        {
            var errorMessage = _fixture.Create<string>();
            
            _mockAuthorizationHandler.Setup(handler => handler.IsAuthorized(_httpRequestMessage.Headers, _mockClaimsPrincipal.Object, out errorMessage))
                .Returns(false);
            _mockExceptionHandler.Setup(handler => handler.HandleException(It.IsAny<UnauthorizedException>()))
                .Returns(new HttpResponseMessage(HttpStatusCode.Unauthorized));
            _httpRequestMessage.Content = new StringContent(" ");

            var response = await _redactPdf.Run(_httpRequestMessage, _mockClaimsPrincipal.Object);

            response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        }

        [Fact]
        public async Task Run_ReturnsBadRequestWhenContentIsInvalid()
        {
            var errorHttpResponseMessage = new HttpResponseMessage(HttpStatusCode.BadRequest);
            _mockExceptionHandler.Setup(handler => handler.HandleException(It.IsAny<BadRequestException>()))
                .Returns(errorHttpResponseMessage);
            _httpRequestMessage.Content = new StringContent(" ");

            var response = await _redactPdf.Run(_httpRequestMessage, _mockClaimsPrincipal.Object);

            response.Should().Be(errorHttpResponseMessage);
        }

        [Fact]
        public async Task Run_ReturnsResponseWhenExceptionOccurs()
        {
            var errorHttpResponseMessage = new HttpResponseMessage(HttpStatusCode.InternalServerError);
            var exception = new Exception();
            _mockJsonConvertWrapper.Setup(wrapper => wrapper.SerializeObject(It.IsAny<RedactPdfResponse>()))
                .Throws(exception);
            _mockExceptionHandler.Setup(handler => handler.HandleException(exception))
                .Returns(errorHttpResponseMessage);

            var response = await _redactPdf.Run(_httpRequestMessage, _mockClaimsPrincipal.Object);

            response.StatusCode.Should().Be(HttpStatusCode.InternalServerError);
        }

        [Fact]
        public async Task Run_ReturnsOk()
        {
            var response = await _redactPdf.Run(_httpRequestMessage, _mockClaimsPrincipal.Object);

            response.StatusCode.Should().Be(HttpStatusCode.OK);
        }

        [Fact]
        public async Task Run_ReturnsExpectedContent()
        {
            var response = await _redactPdf.Run(_httpRequestMessage, _mockClaimsPrincipal.Object);

            var content = await response.Content.ReadAsStringAsync();
            content.Should().Be(_serializedRedactPdfResponse);
        }

        [Fact]
        public async Task Run_ReturnsBadRequest_WhenValidationFailed()
        {
            var errorHttpResponseMessage = new HttpResponseMessage(HttpStatusCode.BadRequest);
            _mockExceptionHandler.Setup(handler => handler.HandleException(It.IsAny<BadRequestException>()))
                .Returns(errorHttpResponseMessage);

            var request = _fixture.Create<RedactPdfRequest>();
            request.CaseId = string.Empty;

            var serializedRedactPdfRequest = JsonConvert.SerializeObject(request);
            _httpRequestMessage = new HttpRequestMessage()
            {
                Content = new StringContent(serializedRedactPdfRequest, Encoding.UTF8, "application/json")
            };

            var response = await _redactPdf.Run(_httpRequestMessage, _mockClaimsPrincipal.Object);

            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }
    }
}
