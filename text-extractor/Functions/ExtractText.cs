using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Security.Claims;
using System.Threading.Tasks;
using common.Domain.Exceptions;
using common.Handlers;
using common.Wrappers;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using text_extractor.Domain.Requests;
using text_extractor.Handlers;
using text_extractor.Services.OcrService;
using text_extractor.Services.SearchIndexService;

namespace text_extractor.Functions.ProcessDocument
{
    public class ExtractText
    {
        private readonly IAuthorizationHandler _authorizationHandler;
        private readonly IJsonConvertWrapper _jsonConvertWrapper;
        private readonly IValidatorWrapper<ExtractTextRequest> _validatorWrapper;
        private readonly IOcrService _ocrService;
        private readonly ISearchIndexService _searchIndexService;
        private readonly IExceptionHandler _exceptionHandler;

        public ExtractText(
             IAuthorizationHandler authorizationHandler, IJsonConvertWrapper jsonConvertWrapper,
             IValidatorWrapper<ExtractTextRequest> validatorWrapper, IOcrService ocrService,
             ISearchIndexService searchIndexService, IExceptionHandler exceptionHandler)
        {
            _authorizationHandler = authorizationHandler;
            _jsonConvertWrapper = jsonConvertWrapper;
            _validatorWrapper = validatorWrapper;
            _ocrService = ocrService;
            _searchIndexService = searchIndexService;
            _exceptionHandler = exceptionHandler;
        }

        [FunctionName("ExtractText")]
        public async Task<HttpResponseMessage> Run([HttpTrigger(AuthorizationLevel.Function, "post", Route = "extract")] HttpRequestMessage request, ClaimsPrincipal claimsPrincipal)
        {
            try
            {
                if (!_authorizationHandler.IsAuthorized(request.Headers, claimsPrincipal, out var errorMessage))
                {
                    throw new UnauthorizedException(errorMessage);
                }

                var content = await request.Content.ReadAsStringAsync();
                if (string.IsNullOrWhiteSpace(content))
                {
                    throw new BadRequestException("Request body cannot be null.", nameof(request));
                }

                var extractTextRequest = _jsonConvertWrapper.DeserializeObject<ExtractTextRequest>(content);

                var results = _validatorWrapper.Validate(extractTextRequest);
                if (results.Any())
                {
                    throw new BadRequestException(string.Join(Environment.NewLine, results), nameof(request));
                }

                var ocrResults = await _ocrService.GetOcrResults(extractTextRequest.BlobName);
                await _searchIndexService.StoreResults(ocrResults, extractTextRequest.CaseId, extractTextRequest.DocumentId);

                return new HttpResponseMessage(HttpStatusCode.OK);
            }
            catch (Exception exception)
            {
                return _exceptionHandler.HandleException(exception);
            }
        }
    }
}