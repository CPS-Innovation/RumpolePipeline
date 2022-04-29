using System;
using System.IO;
using Aspose.Email;
using Aspose.Words;
using pdf_generator.Domain.Exceptions;
using License = Aspose.Email.License;

namespace pdf_generator.Services.PdfService
{
    public class EmailPdfService : IPdfService
    {
        public EmailPdfService()
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
            var mailMsg = MailMessage.Load(inputStream);
            using var memoryStream = new MemoryStream();
            memoryStream.Seek(0, SeekOrigin.Begin);
            mailMsg.Save(memoryStream, SaveOptions.DefaultMhtml);

            //// load the MTHML from memoryStream into a document
            var document = new Document(memoryStream, new Aspose.Words.Loading.LoadOptions { LoadFormat = LoadFormat.Mhtml });
            document.Save(pdfStream, SaveFormat.Pdf);
            pdfStream.Seek(0, SeekOrigin.Begin);
        }
    }
}
