using System;
using System.Net;
using coordinator.Domain.Exceptions;
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
        public void HandleException_LogsExpectedErrorWhenUnauthorizedExceptionOccurs()
        {
            var unauthorizedException = new UnauthorizedException("Test unauthorized exception");
            ExceptionHandler.HandleException(unauthorizedException);

            _mockLogger.Verify(x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.IsAny<It.IsAnyType>(),
                It.Is<UnauthorizedException>(e => e == unauthorizedException),
                (Func<It.IsAnyType, Exception, string>)It.IsAny<object>()));
        }

        [Fact]
        public void HandleException_ReturnsBadRequestWhenBadRequestExceptionOccurs()
        {
            var httpResponseMessage = ExceptionHandler.HandleException(new BadRequestException("Test bad request exception", "id"));

            httpResponseMessage.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        [Fact]
        public void HandleException_LogsExpectedErrorWhenBadRequestExceptionOccurs()
        {
            var badRequestException = new BadRequestException("Test bad request exception", "id");
            ExceptionHandler.HandleException(badRequestException);

            _mockLogger.Verify(x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.IsAny<It.IsAnyType>(),
                It.Is<ArgumentException>(e => e == badRequestException),
                (Func<It.IsAnyType, Exception, string>)It.IsAny<object>()));
        }

        [Fact]
        public void HandleException_ReturnsInternalServerErrorWhenUnhandledErrorOccurs()
        {
            var httpResponseMessage = ExceptionHandler.HandleException(new ApplicationException());

            httpResponseMessage.StatusCode.Should().Be(HttpStatusCode.InternalServerError);
        }

        [Fact]
        public void HandleException_LogsExpectedErrorWhenUnhandledErrorOccurs()
        {
            var exception = new ApplicationException("Test exception");
            ExceptionHandler.HandleException(exception);

            _mockLogger.Verify(x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.IsAny<It.IsAnyType>(),
                It.Is<ApplicationException>(ae => ae == exception),
                (Func<It.IsAnyType, Exception, string>)It.IsAny<object>()));
        }
    }
}
