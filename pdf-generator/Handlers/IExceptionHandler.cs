using System;
using System.Net.Http;

namespace pdf_generator.Handlers
{
    public interface IExceptionHandler
    {
        HttpResponseMessage HandleException(Exception exception);
    }
}
