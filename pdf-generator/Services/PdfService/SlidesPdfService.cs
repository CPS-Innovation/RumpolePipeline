using System;
using System.IO;
using Aspose.Slides;

namespace pdf_generator.Services.PdfService
{
    public class SlidesPdfService : IPdfService
    {
        public SlidesPdfService()
        {
            try
            {
                //TODO move this into orchestrator - will that work?
                var license = new License();
                license.SetLicense("Aspose.Total.NET.lic");
            }
            catch (Exception e)
            {
                throw new Exception($"Failed to set Aspose License: {e.Message}");
            }
        }

        public void ReadToPdfStream(Stream inputStream, Stream pdfStream)
        {
            //var doc = new Presentation(inputStream);
            //doc.Save(pdfStream, new PdfSaveOptions());
            //pdfStream.Seek(0, SeekOrigin.Begin); // check this is actually required
        }
    }
}
