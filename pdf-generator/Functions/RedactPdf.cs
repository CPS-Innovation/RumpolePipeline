using System;
using System.Net;
using System.Net.Http;
using System.Net.Mime;
using System.Text;
using System.Threading.Tasks;
using common.Domain.Exceptions;
using Common.Domain.Extensions;
using common.Handlers;
using common.Wrappers;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using pdf_generator.Domain.Requests;
using pdf_generator.Domain.Validators;
using pdf_generator.Handlers;
using pdf_generator.Services.DocumentRedactionService;

namespace pdf_generator.Functions
{
    public class RedactPdf
    {
        private readonly IAuthorizationValidator _authorizationValidator;
        private readonly IExceptionHandler _exceptionHandler;
        private readonly IJsonConvertWrapper _jsonConvertWrapper;
        private readonly IDocumentRedactionService _documentRedactionService;

        public RedactPdf(IAuthorizationValidator authorizationValidator, IExceptionHandler exceptionHandler, IJsonConvertWrapper jsonConvertWrapper, IDocumentRedactionService documentRedactionService)
        {
            _authorizationValidator = authorizationValidator ?? throw new ArgumentNullException(nameof(authorizationValidator));
            _exceptionHandler = exceptionHandler ?? throw new ArgumentNullException(nameof(exceptionHandler));
            _jsonConvertWrapper = jsonConvertWrapper ?? throw new ArgumentNullException(nameof(jsonConvertWrapper));
            _documentRedactionService = documentRedactionService ?? throw new ArgumentNullException(nameof(documentRedactionService));
        }

        [FunctionName("redact-pdf")]
        public async Task<HttpResponseMessage> Run([HttpTrigger(AuthorizationLevel.Anonymous, "put", Route = "redactPdf")]
            HttpRequestMessage request)
        {
            try
            {
                var authValidation = await _authorizationValidator.ValidateTokenAsync(request.Headers.Authorization, Environment.GetEnvironmentVariable("RedactPdfValidAudience"));
                if (!authValidation.Item1)
                    throw new UnauthorizedException("Token validation failed");

                if (request.Content == null)
                    throw new BadRequestException("Request body has no content", nameof(request));

                var content = await request.Content.ReadAsStringAsync();
                if (string.IsNullOrWhiteSpace(content))
                {
                    throw new BadRequestException("Request body cannot be null.", nameof(request));
                }

                var redactions = await request.GetJsonBody<RedactPdfRequest, RedactPdfRequestValidator>();
                if (!redactions.IsValid)
                    throw new BadRequestException(redactions.FlattenErrors(), nameof(request));

                //TODO exchange access token via on behalf of?
                //var accessToken = values.First().Replace("Bearer ", "");
                var redactResponse = await _documentRedactionService.RedactPdfAsync(redactions.Value, "onBehalfOfAccessToken");
                return new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = new StringContent(_jsonConvertWrapper.SerializeObject(redactResponse), Encoding.UTF8, MediaTypeNames.Application.Json)
                };
            }
            catch (Exception ex)
            {
                return _exceptionHandler.HandleException(ex);
            }
        }
    }
}
