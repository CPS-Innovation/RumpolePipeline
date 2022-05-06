using System;
namespace coordinator.Domain.Exceptions
{
    [Serializable]
    public class OnBehalfOfTokenClientException : Exception
    {
        public OnBehalfOfTokenClientException(string message) : base(message) { }
    }
}
