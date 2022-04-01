using System;
namespace coordinator.Domain.Exceptions
{
    public class CoreDataApiClientException : Exception
    {
        public CoreDataApiClientException(string message): base(message)
        {
        }
    }
}
