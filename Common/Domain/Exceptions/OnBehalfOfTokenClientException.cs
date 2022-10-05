using System;

namespace Common.Domain.Exceptions
{
    [Serializable]
    public class OnBehalfOfTokenClientException : Exception
    {
        public OnBehalfOfTokenClientException(string message) : base(message) { }
    }
}
