using System;

namespace Common.Domain.Exceptions
{
    public class ClientTokenException : Exception
    {
        public ClientTokenException(string message) : base(message) { }
    }
}