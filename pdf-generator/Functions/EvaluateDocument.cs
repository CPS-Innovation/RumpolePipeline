using System;
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
using Common.Handlers;
using Common.Logging;
using Common.Wrappers;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using pdf_generator.Handlers;
using pdf_generator.Services.DocumentEvaluationService;

namespace pdf_generator.Functions;

public class EvaluateDocument
{
    private readonly IAuthorizationValidator _authorizationValidator;
    private readonly IJsonConvertWrapper _jsonConvertWrapper;
    private readonly ILogger<EvaluateDocument> _logger;
    private readonly IValidatorWrapper<EvaluateDocumentRequest> _validatorWrapper;
    private readonly IDocumentEvaluationService _documentEvaluationService;
    private readonly IExceptionHandler _exceptionHandler;

    public EvaluateDocument(IAuthorizationValidator authorizationValidator, IJsonConvertWrapper jsonConvertWrapper, ILogger<EvaluateDocument> logger, 
        IValidatorWrapper<EvaluateDocumentRequest> validatorWrapper, IDocumentEvaluationService documentEvaluationService, IExceptionHandler exceptionHandler)
    {
        _authorizationValidator = authorizationValidator;
        _jsonConvertWrapper = jsonConvertWrapper;
        _validatorWrapper = validatorWrapper;
        _logger = logger;
        _documentEvaluationService = documentEvaluationService;
        _exceptionHandler = exceptionHandler;
    }

    [FunctionName("EvaluateDocument")]
    public async Task<HttpResponseMessage> Run([HttpTrigger(AuthorizationLevel.Function, "post", Route = "evaluateDocument")] HttpRequestMessage request)
    {
        Guid currentCorrelationId = default;
        const string loggingName = "EvaluateDocument - Run";
        
        try
        {
            request.Headers.TryGetValues("Correlation-Id", out var correlationIdValues);
            if (correlationIdValues == null)
                throw new BadRequestException("Invalid correlationId. A valid GUID is required.", nameof(request));

            var correlationId = correlationIdValues.First();
            if (!Guid.TryParse(correlationId, out currentCorrelationId) || currentCorrelationId == Guid.Empty)
                throw new BadRequestException("Invalid correlationId. A valid GUID is required.", correlationId);

            _logger.LogMethodEntry(currentCorrelationId, loggingName, string.Empty);

            var authValidation =
                await _authorizationValidator.ValidateTokenAsync(request.Headers.Authorization, currentCorrelationId, PipelineScopes.EvaluateDocument, 
                    PipelineRoles.EvaluateDocument);
            if (!authValidation.Item1)
                throw new UnauthorizedException("Token validation failed");

            if (request.Content == null)
                throw new BadRequestException("Request body has no content", nameof(request));

            var content = await request.Content.ReadAsStringAsync();
            if (string.IsNullOrWhiteSpace(content))
            {
                throw new BadRequestException("Request body cannot be null.", nameof(request));
            }

            var evaluateDocumentRequest = _jsonConvertWrapper.DeserializeObject<EvaluateDocumentRequest>(content);

            var results = _validatorWrapper.Validate(evaluateDocumentRequest);
            if (results.Any())
                throw new BadRequestException(string.Join(Environment.NewLine, results), nameof(request));
            
            _logger.LogMethodFlow(currentCorrelationId, loggingName, $"Beginning document evaluation process for documentId {evaluateDocumentRequest.DocumentId}, materialId {evaluateDocumentRequest.MaterialId}, lastUpdatedDate {evaluateDocumentRequest.LastUpdatedDate}");

            var evaluationResult = _documentEvaluationService.EvaluateDocumentAsync(evaluateDocumentRequest, currentCorrelationId);
            
            _logger.LogMethodFlow(currentCorrelationId, loggingName, $"Document evaluation process completed for documentId {evaluateDocumentRequest.DocumentId}");

            return new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(evaluationResult.ToJson(), Encoding.UTF8, MediaTypeNames.Application.Json)
            };
        }
        catch (Exception exception)
        {
            return _exceptionHandler.HandleException(exception, currentCorrelationId, loggingName, _logger);
        }
        finally
        {
            _logger.LogMethodExit(currentCorrelationId, loggingName, string.Empty);
        }
    }
}
