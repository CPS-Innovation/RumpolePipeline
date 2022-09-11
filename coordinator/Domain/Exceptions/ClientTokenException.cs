using System;

namespace coordinator.Domain.Exceptions;

public class ClientTokenException : Exception
{
    public ClientTokenException(string message) : base(message) { }
}