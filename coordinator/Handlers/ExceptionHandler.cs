using System;
using System.Net;
using System.Net.Http;
using System.Net.Mime;
using System.Text;
using coordinator.Domain.Exceptions;
using Microsoft.Extensions.Logging;

namespace coordinator.Handlers
{
    public class ExceptionHandler : IExceptionHandler
    {
        private readonly ILogger<IExceptionHandler> _log;

        public ExceptionHandler(ILogger<IExceptionHandler> log)
        {
            _log = log;
        }

        public HttpResponseMessage HandleException(Exception exception)
        {
            var baseErrorMessage = "An unhandled exception occurred";
            var statusCode = HttpStatusCode.InternalServerError;

            if (exception is UnauthorizedException)
            {
                baseErrorMessage = "Unauthorized";
                statusCode = HttpStatusCode.Unauthorized;
            }
            else if (exception is BadRequestException)
            {
                baseErrorMessage = "Invalid request";
                statusCode = HttpStatusCode.BadRequest;
            }

            return ErrorResponse(baseErrorMessage, exception, statusCode);
        }

        private HttpResponseMessage ErrorResponse(string baseErrorMessage, Exception exception, HttpStatusCode httpStatusCode)
        {
            _log.LogError(exception, baseErrorMessage);

            var errorMessage = $"{baseErrorMessage}. Base exception message: {exception.GetBaseException().Message}";
            return new HttpResponseMessage(httpStatusCode)
            {
                Content = new StringContent(errorMessage, Encoding.UTF8, MediaTypeNames.Application.Json)
            };
        }

        private HttpResponseMessage OkResponse()
        {
            return new HttpResponseMessage(HttpStatusCode.OK);
        }
    }
}
