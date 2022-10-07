using System;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using Common.Logging;
using Microsoft.Extensions.Logging;
using pdf_generator.Domain.Exceptions;
using pdf_generator.Factories;

namespace pdf_generator.Services.DocumentExtractionService
{
    public class DocumentExtractionService : IDocumentExtractionService
    {
        private readonly HttpClient _httpClient;
        private readonly IDocumentExtractionHttpRequestFactory _documentExtractionHttpRequestFactory;
        private readonly ILogger<DocumentExtractionService> _logger;

        public DocumentExtractionService(
            HttpClient httpClient,
            IDocumentExtractionHttpRequestFactory documentExtractionHttpRequestFactory, ILogger<DocumentExtractionService> logger)
        {
            _httpClient = httpClient;
            _documentExtractionHttpRequestFactory = documentExtractionHttpRequestFactory;
            _logger = logger;
        }

        public async Task<Stream> GetDocumentAsync(string documentId, string fileName, string accessToken,
            Guid correlationId)
        {
            _logger.LogMethodEntry(correlationId, nameof(GetDocumentAsync), $"DocumentId: {documentId}, FileName: {fileName}");
            //TODO ive assumed here that CDE will return 404 not found when document cant be found. Test this when hooked up properly
            var content = await GetHttpContentAsync($"doc-fetch/{documentId}/{fileName}", accessToken, correlationId);
            var result = await content.ReadAsStreamAsync();
            _logger.LogMethodExit(correlationId, nameof(GetDocumentAsync), string.Empty);
            return result;
        }

        private async Task<HttpContent> GetHttpContentAsync(string requestUri, string accessToken, Guid correlationId)
        {
            _logger.LogMethodEntry(correlationId, nameof(GetHttpContentAsync), $"RequestUri: {requestUri}");
            
            var request = _documentExtractionHttpRequestFactory.Create(requestUri, accessToken, correlationId);
            var response = await _httpClient.SendAsync(request);

            try
            {
                response.EnsureSuccessStatusCode();
            }
            catch (HttpRequestException exception)
            {
                throw new HttpException(response.StatusCode, exception);
            }

            var result = response.Content;
            _logger.LogMethodExit(correlationId, nameof(GetHttpContentAsync), string.Empty);
            return result;
        }
    }
}
