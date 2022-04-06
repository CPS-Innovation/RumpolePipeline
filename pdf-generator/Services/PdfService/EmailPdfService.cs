using System;
using System.IO;
using Aspose.Email;
using Aspose.Words;
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
                //throw new Exception($"Failed to set Aspose License: {exception.Message}");
            }
        }

        public void ReadToPdfStream(Stream inputStream, Stream pdfStream)
        {
            var mailMsg = MailMessage.Load(inputStream);
            mailMsg.Save(pdfStream, SaveOptions.DefaultMhtml);

            // load the MTHML from pdfStream into a document
            var document = new Document(pdfStream, new Aspose.Words.Loading.LoadOptions { LoadFormat = LoadFormat.Mhtml });
            document.Save(pdfStream, SaveFormat.Pdf);
            pdfStream.Seek(0, SeekOrigin.Begin);
        }
    }
}
