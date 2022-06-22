using System;
using System.Net.Http;

namespace text_extractor.Handlers
{
    public interface IExceptionHandler
    {
        HttpResponseMessage HandleException(Exception exception);
    }
}
