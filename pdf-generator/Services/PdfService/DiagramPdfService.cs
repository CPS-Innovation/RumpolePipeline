using System;
using System.IO;
using Aspose.Diagram;

namespace pdf_generator.Services.PdfService
{
    public class DiagramPdfService : IPdfService
    {
        public DiagramPdfService()
        {
            try
            {
                //TODO do we only need 1 license for all pdf services, and can it go in the orchestrator instead?
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
            var doc = new Diagram(inputStream);
            doc.Save(pdfStream, SaveFileFormat.Pdf);
            pdfStream.Seek(0, SeekOrigin.Begin);
        }
    }
}
