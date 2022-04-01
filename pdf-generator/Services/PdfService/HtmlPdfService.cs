using System;
using System.IO;
using Aspose.Html;
using Aspose.Html.Converters;
using Aspose.Html.Saving;
using Aspose.Pdf;
using License = Aspose.Pdf.License;

namespace pdf_generator.Services.PdfService
{
    public class HtmlPdfService : IPdfService
    {
        public HtmlPdfService()
        {
            try
            {
                var license = new License();
                license.SetLicense("Aspose.Total.NET.lic");
            }
            catch (Exception exception)
            {
                //throw new Exception($"Failed to set Aspose License: {exception.Message}");
            }
        }

        public void ReadToPdfStream(Stream inputStream, Stream pdfStream)
        {
            using var doc = new Document(inputStream, new HtmlLoadOptions());
            doc.Save(pdfStream);
            //using var document = new HTMLDocument(inputStream, ".");
            //Converter.ConvertHTML(document, new PdfSaveOptions(), pdfStream);
            pdfStream.Seek(0, SeekOrigin.Begin);
        }
    }
}
