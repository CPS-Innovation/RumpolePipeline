using System;
using System.IO;
using Aspose.Imaging;
using Aspose.Imaging.FileFormats.Pdf;
using Aspose.Imaging.ImageOptions;

namespace pdf_generator.Services.PdfService
{
    public class ImagingPdfService : IPdfService
    {
        public ImagingPdfService()
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
            using var image = Image.Load(inputStream);
            image.Save(pdfStream, new PdfOptions { PdfDocumentInfo = new PdfDocumentInfo() });
            pdfStream.Seek(0, System.IO.SeekOrigin.Begin);
        }
    }
}
