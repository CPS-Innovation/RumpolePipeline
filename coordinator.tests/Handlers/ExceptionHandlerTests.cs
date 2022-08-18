using System;
using System.Net;
using common.Domain.Exceptions;
using coordinator.Handlers;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace coordinator.tests.Handlers
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
        public void HandleException_ReturnsInternalServerErrorWhenUnhandledErrorOccurs()
        {
            var httpResponseMessage = ExceptionHandler.HandleException(new ApplicationException());

            httpResponseMessage.StatusCode.Should().Be(HttpStatusCode.InternalServerError);
        }
    }
}
