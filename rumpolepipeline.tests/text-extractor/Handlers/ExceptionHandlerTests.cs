using System.Net;
using Azure;
using common.Domain.Exceptions;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using text_extractor.Domain.Exceptions;
using text_extractor.Handlers;
using Xunit;

namespace rumpolepipeline.tests.text_extractor.Handlers
{
    public class ExceptionHandlerTests
    {
        private readonly IExceptionHandler _exceptionHandler;

        public ExceptionHandlerTests()
        {
            var mockLogger = new Mock<ILogger<IExceptionHandler>>();

            _exceptionHandler = new ExceptionHandler(mockLogger.Object);
        }

        [Fact]
        public void HandleException_ReturnsUnauthorizedWhenUnauthorizedExceptionOccurs()
        {
            var httpResponseMessage = _exceptionHandler.HandleException(new UnauthorizedException("Test unauthorized exception"));

            httpResponseMessage.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        }

        [Fact]
        public void HandleException_ReturnsBadRequestWhenBadRequestExceptionOccurs()
        {
            var httpResponseMessage = _exceptionHandler.HandleException(new BadRequestException("Test bad request exception", "id"));

            httpResponseMessage.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        [Fact]
        public void HandleException_ReturnsInternalServerErrorWhenRequestFailedExceptionWithBadRequestOccurs()
        {
            var httpResponseMessage = _exceptionHandler.HandleException(
                new RequestFailedException((int)HttpStatusCode.BadRequest, "Test request failed exception"));

            httpResponseMessage.StatusCode.Should().Be(HttpStatusCode.InternalServerError);
        }

        [Fact]
        public void HandleException_ReturnsInternalServerErrorWhenRequestFailedExceptionWithNotFoundOccurs()
        {
            var httpResponseMessage = _exceptionHandler.HandleException(
                new RequestFailedException((int)HttpStatusCode.NotFound, "Test request failed exception"));

            httpResponseMessage.StatusCode.Should().Be(HttpStatusCode.InternalServerError);
        }

        [Fact]
        public void HandleException_ReturnsExpectedStatusCodeWhenRequestFailedExceptionOccurs()
        {
            var expectedStatusCode = HttpStatusCode.ExpectationFailed;
            var httpResponseMessage = _exceptionHandler.HandleException(
                new RequestFailedException((int)expectedStatusCode, "Test request failed exception"));

            httpResponseMessage.StatusCode.Should().Be(expectedStatusCode);
        }

        [Fact]
        public void HandleException_ReturnsInternalServerErrorWhenOcrServiceExceptionOccurs()
        {
            var httpResponseMessage = _exceptionHandler.HandleException(new OcrServiceException("Test message"));

            httpResponseMessage.StatusCode.Should().Be(HttpStatusCode.InternalServerError);
        }

        [Fact]
        public void HandleException_ReturnsInternalServerErrorWhenUnhandledErrorOccurs()
        {
            var httpResponseMessage = _exceptionHandler.HandleException(new ApplicationException());

            httpResponseMessage.StatusCode.Should().Be(HttpStatusCode.InternalServerError);
        }
    }
}
