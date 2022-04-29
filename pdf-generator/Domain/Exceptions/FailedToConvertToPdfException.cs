using System;
namespace pdf_generator.Domain.Exceptions
{
	public class FailedToConvertToPdfException : Exception
	{
		public FailedToConvertToPdfException(string documentId, string message) :
			base($"Failed to convert document with id '{documentId}' to pdf. Exception: {message}.")
		{
		}
	}
}

