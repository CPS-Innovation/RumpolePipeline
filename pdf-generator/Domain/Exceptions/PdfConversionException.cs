using System;
namespace pdf_generator.Domain.Exceptions
{
	public class PdfConversionException : Exception
	{
		public PdfConversionException(string documentId, string message) :
			base($"Failed to convert document with id '{documentId}' to pdf. Exception: {message}.")
		{
		}
	}
}

