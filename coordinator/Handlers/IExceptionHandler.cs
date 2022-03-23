using System;
using System.Net.Http;

namespace coordinator.Handlers
{
    public interface IExceptionHandler
    {
        HttpResponseMessage HandleException(Exception exception);
    }
}
