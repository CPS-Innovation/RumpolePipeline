using System;
using pdf_generator.Domain;
using pdf_generator.Domain.Exceptions;

namespace pdf_generator
{
    public static class Extensions
    {
        public static FileType ToFileType(this string fileType)
        {   
            if(int.TryParse(fileType, out var _) || !Enum.TryParse(typeof(FileType), fileType, true, out var type))
            {
                throw new UnsupportedFileTypeException(fileType);
            }

            return (FileType)type;
        }
    }
}
