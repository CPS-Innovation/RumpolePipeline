using System;
using System.IO;
using Aspose.Slides;
using Aspose.Slides.Export;
using pdf_generator.Domain.Exceptions;

namespace pdf_generator.Services.PdfService
{
    public class SlidesPdfService : IPdfService
    {
        public SlidesPdfService()
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
            using var presentation = new Presentation(inputStream);
            presentation.Save(pdfStream, SaveFormat.Pdf);
            pdfStream.Seek(0, SeekOrigin.Begin);
        }
    }
}
