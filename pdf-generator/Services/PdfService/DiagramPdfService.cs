using System;
using System.IO;
using Aspose.Diagram;
using pdf_generator.Domain.Exceptions;

namespace pdf_generator.Services.PdfService
{
    public class DiagramPdfService : IPdfService
    {
        private readonly IAsposeItemFactory _asposeItemFactory;

        public DiagramPdfService(IAsposeItemFactory asposeItemFactory)
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

            _asposeItemFactory = asposeItemFactory;
        }

        public void ReadToPdfStream(Stream inputStream, Stream pdfStream)
        {
            var doc = _asposeItemFactory.CreateDiagram(inputStream);
            doc.Save(pdfStream, SaveFileFormat.Pdf);
            pdfStream.Seek(0, SeekOrigin.Begin);
        }
    }
}
