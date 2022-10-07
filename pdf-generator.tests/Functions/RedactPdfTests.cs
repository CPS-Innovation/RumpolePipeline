using AutoFixture;
using common.Wrappers;
using Moq;
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
using System.Net.Http.Headers;
using common.Handlers;
using Microsoft.Extensions.Logging;

namespace pdf_generator.tests.Functions
{
    public class RedactPdfTests
    {
        private readonly Fixture _fixture = new();
        private readonly string _serializedRedactPdfResponse;
        private HttpRequestMessage _httpRequestMessage;
        private readonly Mock<IAuthorizationValidator> _mockAuthorizationValidator;
        private readonly Mock<IJsonConvertWrapper> _mockJsonConvertWrapper;
        private readonly Mock<IExceptionHandler> _mockExceptionHandler;
        private readonly Mock<ILogger<RedactPdf>> _loggerMock;
        private readonly Guid _correlationId;

        private readonly RedactPdf _redactPdf;

        public RedactPdfTests()
        {
            var request = _fixture.Create<RedactPdfRequest>();

            var serializedRedactPdfRequest = JsonConvert.SerializeObject(request);
            _httpRequestMessage = new HttpRequestMessage
            {
                Content = new StringContent(serializedRedactPdfRequest, Encoding.UTF8, "application/json")
            };
            _httpRequestMessage.Headers.Add("Correlation-Id", _correlationId.ToString());
            _serializedRedactPdfResponse = _fixture.Create<string>();

            _mockAuthorizationValidator = new Mock<IAuthorizationValidator>();
            _mockJsonConvertWrapper = new Mock<IJsonConvertWrapper>();
            _mockExceptionHandler = new Mock<IExceptionHandler>();
            var mockDocumentRedactionService = new Mock<IDocumentRedactionService>();

            _mockAuthorizationValidator.Setup(handler => handler.ValidateTokenAsync(It.IsAny<AuthenticationHeaderValue>(), It.IsAny<Guid>(), It.IsAny<string>()))
                .ReturnsAsync(new Tuple<bool, string>(true, _fixture.Create<string>()));
            _mockJsonConvertWrapper.Setup(wrapper => wrapper.SerializeObject(It.IsAny<RedactPdfResponse>()))
                .Returns(_serializedRedactPdfResponse);

            mockDocumentRedactionService.Setup(x => x.RedactPdfAsync(It.IsAny<RedactPdfRequest>(), It.IsAny<string>(), It.IsAny<Guid>())).ReturnsAsync(_fixture.Create<RedactPdfResponse>());

            _loggerMock = new Mock<ILogger<RedactPdf>>();
            _correlationId = _fixture.Create<Guid>();

            _redactPdf = new RedactPdf(_mockAuthorizationValidator.Object, _mockExceptionHandler.Object,
                _mockJsonConvertWrapper.Object, mockDocumentRedactionService.Object, _loggerMock.Object);
        }

        [Fact]
        public async Task Run_ReturnsUnauthorizedWhenUnauthorized()
        {
            _mockAuthorizationValidator.Setup(handler => handler.ValidateTokenAsync(It.IsAny<AuthenticationHeaderValue>(), It.IsAny<Guid>(), It.IsAny<string>()))
                .ReturnsAsync(new Tuple<bool, string>(false, string.Empty));
            _mockExceptionHandler.Setup(handler => handler.HandleException(It.IsAny<UnauthorizedException>(), It.IsAny<Guid>(), It.IsAny<string>(), _loggerMock.Object))
                .Returns(new HttpResponseMessage(HttpStatusCode.Unauthorized));
            _httpRequestMessage.Content = new StringContent(" ");

            var response = await _redactPdf.Run(_httpRequestMessage);

            response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        }

        [Fact]
        public async Task Run_ReturnsBadRequestWhenContentIsInvalid()
        {
            var errorHttpResponseMessage = new HttpResponseMessage(HttpStatusCode.BadRequest);
            _mockExceptionHandler.Setup(handler => handler.HandleException(It.IsAny<BadRequestException>(), It.IsAny<Guid>(), It.IsAny<string>(), _loggerMock.Object))
                .Returns(errorHttpResponseMessage);
            _httpRequestMessage.Content = new StringContent(" ");

            var response = await _redactPdf.Run(_httpRequestMessage);

            response.Should().Be(errorHttpResponseMessage);
        }

        [Fact]
        public async Task Run_ReturnsResponseWhenExceptionOccurs()
        {
            var errorHttpResponseMessage = new HttpResponseMessage(HttpStatusCode.InternalServerError);
            var exception = new Exception();
            _mockJsonConvertWrapper.Setup(wrapper => wrapper.SerializeObject(It.IsAny<RedactPdfResponse>()))
                .Throws(exception);
            _mockExceptionHandler.Setup(handler => handler.HandleException(exception, It.IsAny<Guid>(), It.IsAny<string>(), _loggerMock.Object))
                .Returns(errorHttpResponseMessage);

            var response = await _redactPdf.Run(_httpRequestMessage);

            response.StatusCode.Should().Be(HttpStatusCode.InternalServerError);
        }

        [Fact]
        public async Task Run_ReturnsOk()
        {
            var response = await _redactPdf.Run(_httpRequestMessage);

            response.StatusCode.Should().Be(HttpStatusCode.OK);
        }

        [Fact]
        public async Task Run_ReturnsExpectedContent()
        {
            var response = await _redactPdf.Run(_httpRequestMessage);

            var content = await response.Content.ReadAsStringAsync();
            content.Should().Be(_serializedRedactPdfResponse);
        }

        [Fact]
        public async Task Run_ReturnsBadRequest_WhenValidationFailed()
        {
            var errorHttpResponseMessage = new HttpResponseMessage(HttpStatusCode.BadRequest);
            _mockExceptionHandler.Setup(handler => handler.HandleException(It.IsAny<BadRequestException>(), It.IsAny<Guid>(), It.IsAny<string>(), _loggerMock.Object))
                .Returns(errorHttpResponseMessage);

            var request = _fixture.Create<RedactPdfRequest>();
            request.CaseId = string.Empty;

            var serializedRedactPdfRequest = JsonConvert.SerializeObject(request);
            _httpRequestMessage = new HttpRequestMessage()
            {
                Content = new StringContent(serializedRedactPdfRequest, Encoding.UTF8, "application/json")
            };

            var response = await _redactPdf.Run(_httpRequestMessage);

            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }
    }
}
