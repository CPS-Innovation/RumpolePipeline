using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using Domain;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Services.BlobStorageService;
using Services.CmsService;
using Services.PdfService;

namespace Functions
{
    public class DocToPdf
    {
        private readonly CmsService _cmsService;

        private readonly PdfService _pdfService;

        private readonly BlobStorageService _blobStorageService;

        public DocToPdf(CmsService cmsService, PdfService pdfService, BlobStorageService blobStorageService)
        {
            _cmsService = cmsService;
            _pdfService = pdfService;
            _blobStorageService = blobStorageService;
        }

        [FunctionName("doc-to-pdf")]
        public async Task<BlobNameAndSasLinkUrl> Run([HttpTrigger(AuthorizationLevel.Function, "get", "post", Route = null)] HttpRequestMessage req, ILogger log)
        {
            var content = await req.Content.ReadAsStringAsync();
            var arg = JsonConvert.DeserializeObject<DocToPdfArg>(content);

            var url = arg.DocumentUrl;
            var caseId = arg.CaseId;
            var documentId = arg.DocumentId;

            var cmsDocument = await _cmsService.GetDocument(url);

            using var pdfStream = new MemoryStream();
            await _pdfService.ReadToPdfStream(cmsDocument.Stream, pdfStream, cmsDocument.ContentType.ToString());

            var blobName = $"{caseId}/pdfs/{documentId}.pdf";
            var sasLink = await _blobStorageService.UploadAsync(pdfStream, blobName, "application/pdf");

            return new BlobNameAndSasLinkUrl
            {
                BlobName = blobName,
                SasLinkUrl = sasLink
            };
        }
    }
}