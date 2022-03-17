using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Mime;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using pdf_generator.Domain.Exceptions;
using pdf_generator.Domain.Requests;
using pdf_generator.Domain.Responses;
using pdf_generator.Handlers;
using pdf_generator.Services.BlobStorageService;
using pdf_generator.Services.DocumentExtractionService;
using pdf_generator.Services.PdfService;
using pdf_generator.Wrappers;

namespace pdf_generator.Functions
{
    public class GeneratePdf
    {
        private readonly IJsonConvertWrapper _jsonConvertWrapper;
        private readonly IValidatorWrapper<GeneratePdfRequest> _validatorWrapper;
        private readonly IDocumentExtractionService _documentExtractionService;
        private readonly IBlobStorageService _blobStorageService;
        private readonly IPdfOrchestratorService _pdfOrchestratorService;
        private readonly IExceptionHandler _exceptionHandler;

        public GeneratePdf(
            IJsonConvertWrapper jsonConvertWrapper, IValidatorWrapper<GeneratePdfRequest> validatorWrapper,
            IDocumentExtractionService documentExtractionService, IBlobStorageService blobStorageService,
            IPdfOrchestratorService pdfOrchestratorService, IExceptionHandler exceptionHandler)
        {
            _jsonConvertWrapper = jsonConvertWrapper;
            _validatorWrapper = validatorWrapper;
            _documentExtractionService = documentExtractionService;
            _blobStorageService = blobStorageService;
            _pdfOrchestratorService = pdfOrchestratorService;
            _exceptionHandler = exceptionHandler;
        }

        [FunctionName("generate-pdf")]
        public async Task<HttpResponseMessage> Run([HttpTrigger(AuthorizationLevel.Function, "post", Route = "generate")] HttpRequestMessage request)
        {
            try
            {
                var content = await request.Content.ReadAsStringAsync();
                if (string.IsNullOrWhiteSpace(content))
                {
                    throw new BadRequestException("Request body cannot be null", nameof(request));
                }

                var pdfRequest = _jsonConvertWrapper.DeserializeObject<GeneratePdfRequest>(content);

                var results = _validatorWrapper.Validate(pdfRequest);
                if (results.Any())
                {
                    throw new BadRequestException(string.Join(Environment.NewLine, results), nameof(request));
                }

                var documentSasUrl = await _documentExtractionService.GetDocumentSasLinkAsync(pdfRequest.DocumentId);

                var documentStream = await _blobStorageService.DownloadDocumentAsync(documentSasUrl);

                var blobName = $"{pdfRequest.CaseId}/pdfs/{pdfRequest.DocumentId}.pdf";

                //TODO extract file type from filename
                if (pdfRequest.FileName == "pdf")
                {
                    await _blobStorageService.UploadAsync(documentStream, blobName);
                }
                else
                {
                    var pdfStream = _pdfOrchestratorService.ReadToPdfStream(documentStream, pdfRequest.FileName);
                    await _blobStorageService.UploadAsync(pdfStream, blobName);
                }

                return OkResponse(Serialize(new GeneratePdfResponse { BlobName = blobName }));
            }
            catch(Exception exception)
            {
                return _exceptionHandler.HandleException(exception);
            }
        }

        private string Serialize(object objectToSerialize)
        {
            return _jsonConvertWrapper.SerializeObject(objectToSerialize, Formatting.None, new JsonSerializerSettings { ReferenceLoopHandling = ReferenceLoopHandling.Ignore });
        }

        private HttpResponseMessage OkResponse(string content)
        {
            return new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(content, Encoding.UTF8, MediaTypeNames.Application.Json)
            };
        }
    }
}