using System.Net;
using common.Domain.Exceptions;
using coordinator.Handlers;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace rumpolepipeline.tests.coordinator.Handlers
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
        public void HandleException_ReturnsInternalServerErrorWhenUnhandledErrorOccurs()
        {
            var httpResponseMessage = _exceptionHandler.HandleException(new ApplicationException());

            httpResponseMessage.StatusCode.Should().Be(HttpStatusCode.InternalServerError);
        }
    }
}
