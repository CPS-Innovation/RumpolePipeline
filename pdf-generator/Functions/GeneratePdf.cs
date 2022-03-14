using System;
using System.IO;
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
        public async Task<HttpResponseMessage> Run([HttpTrigger(AuthorizationLevel.Function, "post", Route = "generate")] HttpRequestMessage req, ILogger log)
        {
            try
            {
                var content = await req.Content.ReadAsStringAsync();
                //TODO test if json exception can be thrown and if it can then add to exception handler
                var request = _jsonConvertWrapper.DeserializeObject<GeneratePdfRequest>(content);

                var results = _validatorWrapper.Validate(request);
                if (results.Any())
                {
                    throw new BadRequestException(string.Join(Environment.NewLine, results), nameof(request));
                }

                var documentSasUrl = await _documentExtractionService.GetDocumentSasLinkAsync(request.CaseId, request.DocumentId);

                var documentStream = await _blobStorageService.DownloadDocumentAsync(documentSasUrl);

                //var pdfStream = _pdfOrchestratorService.ReadToPdfStream(documentStream, request.FileName);

                var blobName = $"{request.CaseId}/pdfs/{request.DocumentId}.txt";
                await _blobStorageService.UploadAsync(documentStream, blobName);

                var response =  new GeneratePdfResponse
                {
                    BlobName = blobName
                };

                return OkResponse(Serialize(response));
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