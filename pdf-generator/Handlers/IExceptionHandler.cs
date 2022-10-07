using System;
using System.Net.Http;
using Microsoft.Extensions.Logging;

namespace pdf_generator.Handlers
{
    public interface IExceptionHandler
    {
        HttpResponseMessage HandleException(Exception exception, Guid correlationId, string source, ILogger logger);
    }
}
