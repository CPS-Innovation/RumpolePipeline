using System;
using System.IO;
using System.Threading.Tasks;
using Aspose.Words;
using Aspose.Words.Saving;

namespace Services.PdfService
{
    public class PdfService
    {

        public PdfService()
        {
            try
            {
                var license = new Aspose.Words.License();
                license.SetLicense("Aspose.Total.NET.lic");
            }
            catch (Exception e)
            {
                Console.WriteLine("\nThere was an error setting the word license: " + e.Message);
            }
        }

        public Task ReadToPdfStream(Stream inputStream, Stream outputStream, string contentType)
        {
            var doc = new Document(inputStream);

            doc.Save(outputStream, new PdfSaveOptions());
            outputStream.Seek(0, SeekOrigin.Begin); // check this is actually required
            return Task.CompletedTask;
        }

    }
}
