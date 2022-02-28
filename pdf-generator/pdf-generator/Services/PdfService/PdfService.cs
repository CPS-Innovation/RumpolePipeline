using System.Net;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using System;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Aspose.Words;
using Aspose.Words.Saving;

namespace Services.PdfService
{
    public class PdfService
    {

        public PdfService()
        {
            // try
            // {
            var license = new Aspose.Words.License();
            license.SetLicense("Aspose.Total.NET.lic");
            // }
            // catch (Exception e)
            // {
            //     Console.WriteLine("\nThere was an error setting the word license: " + e.Message);
            // }
        }

        public Task<MemoryStream> GetPdfStream(Stream inputStream, string contentType)
        {
            var doc = new Document(inputStream);
            var ms = new MemoryStream();
            doc.Save(ms, new PdfSaveOptions());
            ms.Seek(0, SeekOrigin.Begin); // check this is actually required
            return Task.FromResult(ms);
        }

    }
}
