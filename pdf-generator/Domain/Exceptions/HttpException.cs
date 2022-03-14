using System;
using System.Net;
using System.Net.Http;

namespace pdf_generator.Domain.Exceptions
{
    public class HttpException : Exception
    {
        public HttpStatusCode StatusCode { get; private set; }

        public HttpException(HttpStatusCode statusCode, HttpRequestException httpRequestException)
            : base($"The HTTP request failed with status code {statusCode}", httpRequestException)
        {
            StatusCode = statusCode;
        }
    }
}
