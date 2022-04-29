using System;
namespace pdf_generator.Domain.Exceptions
{
	public class FileTypeNotSupportedException : Exception
	{
		public FileTypeNotSupportedException(string value) :
			base($"File type '{value}' not supported.")
		{
		}
	}
}

