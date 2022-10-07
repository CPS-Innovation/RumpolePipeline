using System;
using System.Net.Http;
using Microsoft.Extensions.Logging;

namespace text_extractor.Handlers
{
    public interface IExceptionHandler
    {
        HttpResponseMessage HandleException(Exception exception, Guid correlationId, string source, ILogger logger);
    }
}
