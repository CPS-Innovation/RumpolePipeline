using System;
using System.IO;
using Aspose.Words;
using pdf_generator.Domain.Exceptions;

namespace pdf_generator.Services.PdfService
{
    public class WordsPdfService : IPdfService
    {
        public WordsPdfService()
        {
            try
            {
                var license = new License();
                license.SetLicense("Aspose.Total.NET.lic");
            }
            catch (Exception exception)
            {
                throw new AsposeLicenseException(exception.Message);
            }
        }

        public void ReadToPdfStream(Stream inputStream, Stream pdfStream)
        {
            var doc = new Document(inputStream);
            doc.Save(pdfStream, SaveFormat.Pdf);
            pdfStream.Seek(0, SeekOrigin.Begin);
        }
    }
}
