using System;
using System.IO;
using Aspose.Pdf;
using pdf_generator.Domain.Exceptions;
using License = Aspose.Pdf.License;

namespace pdf_generator.Services.PdfService
{
    public class HtmlPdfService : IPdfService
    {
        private readonly IAsposeItemFactory _asposeItemFactory;

        public HtmlPdfService(IAsposeItemFactory asposeItemFactory)
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
            using var doc = _asposeItemFactory.CreateHtmlDocument(inputStream);
            doc.Save(pdfStream);
            pdfStream.Seek(0, SeekOrigin.Begin);
        }
    }
}
