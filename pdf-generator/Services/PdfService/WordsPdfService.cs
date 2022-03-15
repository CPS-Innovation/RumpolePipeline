using System;
using System.IO;
using Aspose.Words;
using Aspose.Words.Saving;

namespace pdf_generator.Services.PdfService
{
    public class WordsPdfService : IPdfService
    {
        public WordsPdfService()
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
            var doc = new Document(inputStream);
            doc.Save(pdfStream, SaveFormat.Pdf); //TODO need to save in specific format? (like Slides)
            pdfStream.Seek(0, SeekOrigin.Begin); // check this is actually required, and if needed for others
        }
    }
}
