using System;

namespace coordinator.Domain.Exceptions
{
    public class BadRequestException : ArgumentException
    {
        public BadRequestException(string message, string paramName) : base(message, paramName)
        {
        }
    }
}