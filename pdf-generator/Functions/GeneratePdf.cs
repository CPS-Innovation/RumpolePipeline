﻿using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Mime;
using System.Text;
using System.Threading.Tasks;
using Common.Constants;
using Common.Domain.Exceptions;
using Common.Domain.Extensions;
using Common.Domain.Requests;
using Common.Domain.Responses;
using Common.Handlers;
using Common.Logging;
using Common.Services.Contracts;
using Common.Wrappers;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using pdf_generator.Domain;
using pdf_generator.Handlers;
using pdf_generator.Services.BlobStorageService;
using pdf_generator.Services.PdfService;

namespace pdf_generator.Functions
{
    public class GeneratePdf
    {
        private readonly IAuthorizationValidator _authorizationValidator;
        private readonly IJsonConvertWrapper _jsonConvertWrapper;
        private readonly IValidatorWrapper<GeneratePdfRequest> _validatorWrapper;
        private readonly IDocumentExtractionService _documentExtractionService;
        private readonly IBlobStorageService _blobStorageService;
        private readonly IPdfOrchestratorService _pdfOrchestratorService;
        private readonly IExceptionHandler _exceptionHandler;
        private readonly ILogger<GeneratePdf> _log;

        public GeneratePdf(
             IAuthorizationValidator authorizationValidator, IJsonConvertWrapper jsonConvertWrapper,
             IValidatorWrapper<GeneratePdfRequest> validatorWrapper, IDocumentExtractionService documentExtractionService,
             IBlobStorageService blobStorageService, IPdfOrchestratorService pdfOrchestratorService, IExceptionHandler exceptionHandler, ILogger<GeneratePdf> logger)
        {
            _authorizationValidator = authorizationValidator;
            _jsonConvertWrapper = jsonConvertWrapper;
            _validatorWrapper = validatorWrapper;
            _documentExtractionService = documentExtractionService;
            _blobStorageService = blobStorageService;
            _pdfOrchestratorService = pdfOrchestratorService;
            _exceptionHandler = exceptionHandler;
            _log = logger;
        }

        [FunctionName("generate-pdf")]
        public async Task<HttpResponseMessage> Run([HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "generate")] HttpRequestMessage request)
        {
            Guid currentCorrelationId = default;
            string upstreamToken = default;
            const string loggingName = "GeneratePdf - Run";
            GeneratePdfResponse generatePdfResponse = null;

            try
            {
                request.Headers.TryGetValues(HttpHeaderKeys.CorrelationId, out var correlationIdValues);
                if (correlationIdValues == null)
                    throw new BadRequestException("Invalid correlationId. A valid GUID is required.", nameof(request));

                var correlationId = correlationIdValues.First();
                if (!Guid.TryParse(correlationId, out currentCorrelationId) || currentCorrelationId == Guid.Empty)
                        throw new BadRequestException("Invalid correlationId. A valid GUID is required.", correlationId);
                
                request.Headers.TryGetValues(HttpHeaderKeys.UpstreamTokenName, out var upstreamTokenValues);
                if (upstreamTokenValues == null)
                    throw new BadRequestException("Invalid upstream token. A valid DDEI token must be received for this request.", nameof(request));
                upstreamToken = upstreamTokenValues.First();
                if (string.IsNullOrWhiteSpace(upstreamToken))
                    throw new BadRequestException("Invalid upstream token. A valid DDEI token must be received for this request.", nameof(request));

                _log.LogMethodEntry(currentCorrelationId, loggingName, string.Empty);

                var authValidation = await _authorizationValidator.ValidateTokenAsync(request.Headers.Authorization, currentCorrelationId, 
                    PipelineScopes.GeneratePdf, PipelineRoles.GeneratePdf);
                if (!authValidation.Item1)
                    throw new UnauthorizedException("Token validation failed");

                if (request.Content == null)
                    throw new BadRequestException("Request body has no content", nameof(request));

                var content = await request.Content.ReadAsStringAsync();
                if (string.IsNullOrWhiteSpace(content))
                    throw new BadRequestException("Request body cannot be null.", nameof(request));
                
                var pdfRequest = _jsonConvertWrapper.DeserializeObject<GeneratePdfRequest>(content);

                var results = _validatorWrapper.Validate(pdfRequest);
                if (results.Any())
                {
                    throw new BadRequestException(string.Join(Environment.NewLine, results), nameof(request));
                }

                //Will need to prepare a custom oAuth request to send to Cde
                _log.LogMethodFlow(currentCorrelationId, loggingName, $"Retrieving Document from Cde for documentId: '{pdfRequest.DocumentId}'");
                
                var documentStream = await _documentExtractionService.GetDocumentAsync(pdfRequest.CaseUrn, pdfRequest.CaseId.ToString(), pdfRequest.DocumentCategory, pdfRequest.DocumentId, upstreamToken, currentCorrelationId);

                var blobName = $"{pdfRequest.CaseId}/pdfs/{pdfRequest.DocumentId}.pdf";
                var fileType = pdfRequest.FileName.Split('.').Last().ToFileType();
                if (fileType == FileType.PDF)
                {
                    _log.LogMethodFlow(currentCorrelationId, loggingName, 
                        $"Retrieved document is already a PDF and so no conversion necessary; uploading and storing original file: '{pdfRequest.FileName}' to blob storage as file: '{blobName}'");
                    
                    await _blobStorageService.UploadDocumentAsync(documentStream, blobName, pdfRequest.CaseId.ToString(), pdfRequest.DocumentId, 
                        pdfRequest.VersionId.ToString(), currentCorrelationId);
                    
                    _log.LogMethodFlow(currentCorrelationId, loggingName, $"{blobName} uploaded successfully");
                }
                else
                {
                    _log.LogMethodFlow(currentCorrelationId, loggingName, 
                        $"Retrieved document, '{pdfRequest.FileName}', is not a PDF and so, beginning conversion to PDF...");
                    var pdfStream = _pdfOrchestratorService.ReadToPdfStream(documentStream, fileType, pdfRequest.DocumentId, currentCorrelationId);
                    
                    _log.LogMethodFlow(currentCorrelationId, loggingName, $"Document converted to PDF successfully, beginning upload of '{blobName}'...");
                    await _blobStorageService.UploadDocumentAsync(pdfStream, blobName, pdfRequest.CaseId.ToString(), pdfRequest.DocumentId, 
                        pdfRequest.VersionId.ToString(), currentCorrelationId);
                    
                    _log.LogMethodFlow(currentCorrelationId, loggingName, $"'{blobName}' uploaded successfully");
                }

                generatePdfResponse = new GeneratePdfResponse {BlobName = blobName};
                
                return OkResponse(Serialize(generatePdfResponse));
            }
            catch (Exception exception)
            {
                return _exceptionHandler.HandleException(exception, currentCorrelationId, nameof(GeneratePdf), _log);
            }
            finally
            {
                _log.LogMethodExit(currentCorrelationId, loggingName, generatePdfResponse.ToJson());
            }
        }

        private string Serialize(object objectToSerialize)
        {
            return _jsonConvertWrapper.SerializeObject(objectToSerialize);
        }

        private static HttpResponseMessage OkResponse(string content)
        {
            return new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(content, Encoding.UTF8, MediaTypeNames.Application.Json)
            };
        }
    }
}