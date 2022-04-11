using System;
namespace coordinator.Domain.Exceptions
{
    public class OnBehalfOfTokenClientException : Exception
    {
        public OnBehalfOfTokenClientException(string message) : base(message) { }
    }
}
