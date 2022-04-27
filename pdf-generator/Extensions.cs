using System;
using pdf_generator.Domain;

namespace pdf_generator
{
    public static class Extensions
    {
        public static FileType ToFileType(this string fileName)
        {
            //TODO test
            var fileType = fileName.Split('.')[1];
            
            Enum.TryParse(typeof(FileType), fileType, true, out var type);
            return (FileType)type;
        }
    }
}
