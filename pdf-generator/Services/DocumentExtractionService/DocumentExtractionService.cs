using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using pdf_generator.Domain.Exceptions;
using pdf_generator.Factories;

namespace pdf_generator.Services.DocumentExtractionService
{
    public class DocumentExtractionService : IDocumentExtractionService
    {
        private readonly HttpClient _httpClient;
        private readonly IDocumentExtractionHttpRequestFactory _documentExtractionHttpRequestFactory;

        public DocumentExtractionService(
            HttpClient httpClient,
            IDocumentExtractionHttpRequestFactory documentExtractionHttpRequestFactory)
        {
            _httpClient = httpClient;
            _documentExtractionHttpRequestFactory = documentExtractionHttpRequestFactory;
        }

        public async Task<Stream> GetDocumentAsync(string documentId, string fileName, string accessToken)
        {
            var content = await GetHttpContentAsync($"doc-fetch/{documentId}/{fileName}", accessToken);
            return await content.ReadAsStreamAsync();
        }

        private async Task<HttpContent> GetHttpContentAsync(string requestUri, string accessToken)
        {
            var request = _documentExtractionHttpRequestFactory.Create(requestUri, accessToken);
            var response = await _httpClient.SendAsync(request);

            try
            {
                response.EnsureSuccessStatusCode();
            }
            catch (HttpRequestException exception)
            {
                throw new HttpException(response.StatusCode, exception);
            }

            return response.Content;
        }
    }
}
