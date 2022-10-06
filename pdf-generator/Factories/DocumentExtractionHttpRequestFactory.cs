using System;
using System.Net.Http;
using System.Net.Http.Headers;
using Common.Logging;
using Microsoft.Extensions.Logging;

namespace pdf_generator.Factories
{
    public class DocumentExtractionHttpRequestFactory : IDocumentExtractionHttpRequestFactory
    {
        private readonly ILogger<DocumentExtractionHttpRequestFactory> _logger;

        public DocumentExtractionHttpRequestFactory(ILogger<DocumentExtractionHttpRequestFactory> logger)
        {
            _logger = logger;
        }

        public HttpRequestMessage Create(string requestUri, string accessToken, Guid correlationId)
        {
            _logger.LogMethodEntry(correlationId, nameof(Create), $"RequestUri: {requestUri}");
            
            var request = new HttpRequestMessage(HttpMethod.Get, requestUri);
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
            request.Headers.Add("X-Correlation-ID", correlationId.ToString());
            
            _logger.LogMethodExit(correlationId, nameof(Create), string.Empty);
            return request;
        }
    }
}
