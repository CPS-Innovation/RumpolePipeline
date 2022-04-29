using System;
using System.IO;
using Aspose.Diagram;
using pdf_generator.Domain.Exceptions;

namespace pdf_generator.Services.PdfService
{
    public class DiagramPdfService : IPdfService
    {
        public DiagramPdfService()
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
            var doc = new Diagram(inputStream);
            doc.Save(pdfStream, SaveFileFormat.Pdf);
            pdfStream.Seek(0, SeekOrigin.Begin);
        }
    }
}
