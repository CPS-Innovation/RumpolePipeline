using System;
using System.Net;
using System.Net.Http;
using Azure;
using common.Domain.Exceptions;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using pdf_generator.Domain.Exceptions;
using pdf_generator.Handlers;
using Xunit;

namespace pdf_generator.tests.Handlers
{
    public class ExceptionHandlerTests
    {
        private IExceptionHandler ExceptionHandler;
        private Mock<ILogger<IExceptionHandler>> _mockLogger;

        public ExceptionHandlerTests()
        {
            _mockLogger = new Mock<ILogger<IExceptionHandler>>();

            ExceptionHandler = new ExceptionHandler(_mockLogger.Object);
        }

        [Fact]
        public void HandleException_ReturnsUnauthorizedWhenUnauthorizedExceptionOccurs()
        {
            var httpResponseMessage = ExceptionHandler.HandleException(new UnauthorizedException("Test unauthorized exception"));

            httpResponseMessage.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        }

        [Fact]
        public void HandleException_ReturnsBadRequestWhenBadRequestExceptionOccurs()
        {
            var httpResponseMessage = ExceptionHandler.HandleException(new BadRequestException("Test bad request exception", "id"));

            httpResponseMessage.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        [Fact]
        public void HandleException_ReturnsInternalServerErrorWhenHttpExceptionWithBadRequestOccurs()
        {
            var httpResponseMessage = ExceptionHandler.HandleException(
                new HttpException(HttpStatusCode.BadRequest, new HttpRequestException()));

            httpResponseMessage.StatusCode.Should().Be(HttpStatusCode.InternalServerError);
        }

        [Fact]
        public void HandleException_ReturnsInternalServerErrorWhenHttpExceptionWithNotFoundOccurs()
        {
            var httpResponseMessage = ExceptionHandler.HandleException(
                new HttpException(HttpStatusCode.NotFound, new HttpRequestException()));

            httpResponseMessage.StatusCode.Should().Be(HttpStatusCode.InternalServerError);
        }

        [Fact]
        public void HandleException_ReturnsExpectedStatusCodeWhenHttpExceptionOccurs()
        {
            var expectedStatusCode = HttpStatusCode.ExpectationFailed;
            var httpResponseMessage = ExceptionHandler.HandleException(
                new HttpException(expectedStatusCode, new HttpRequestException()));

            httpResponseMessage.StatusCode.Should().Be(expectedStatusCode);
        }

        [Fact]
        public void HandleException_ReturnsInternalServerErrorWhenRequestFailedExceptionWithBadRequestOccurs()
        {
            var httpResponseMessage = ExceptionHandler.HandleException(
                new RequestFailedException((int)HttpStatusCode.BadRequest, "Test request failed exception"));

            httpResponseMessage.StatusCode.Should().Be(HttpStatusCode.InternalServerError);
        }

        [Fact]
        public void HandleException_ReturnsInternalServerErrorWhenRequestFailedExceptionWithNotFoundOccurs()
        {
            var httpResponseMessage = ExceptionHandler.HandleException(
                new RequestFailedException((int)HttpStatusCode.NotFound, "Test request failed exception"));

            httpResponseMessage.StatusCode.Should().Be(HttpStatusCode.InternalServerError);
        }

        [Fact]
        public void HandleException_ReturnsExpectedStatusCodeWhenRequestFailedExceptionOccurs()
        {
            var expectedStatusCode = HttpStatusCode.ExpectationFailed;
            var httpResponseMessage = ExceptionHandler.HandleException(
                new RequestFailedException((int)expectedStatusCode, "Test request failed exception"));

            httpResponseMessage.StatusCode.Should().Be(expectedStatusCode);
        }

        [Fact]
        public void HandleException_ReturnsInternalServerErrorWhenUnhandledErrorOccurs()
        {
            var httpResponseMessage = ExceptionHandler.HandleException(new ApplicationException());

            httpResponseMessage.StatusCode.Should().Be(HttpStatusCode.InternalServerError);
        }
    }
}
