using System.Net.Http;
using System.Threading.Tasks;
using pdf_generator.Domain.Exceptions;
using pdf_generator.Factories;
using pdf_generator.Wrappers;

namespace pdf_generator.Services.DocumentExtractionService
{
    public class DocumentExtractionService : IDocumentExtractionService
    {
        private readonly HttpClient _httpClient;
        private readonly IDocumentExtractionHttpRequestFactory _documentExtractionHttpRequestFactory;
        private readonly IJsonConvertWrapper _jsonConvertWrapper;

        public DocumentExtractionService(
            HttpClient httpClient,
            IDocumentExtractionHttpRequestFactory documentExtractionHttpRequestFactory,
            IJsonConvertWrapper jsonConvertWrapper)
        {
            _httpClient = httpClient;
            _documentExtractionHttpRequestFactory = documentExtractionHttpRequestFactory;
            _jsonConvertWrapper = jsonConvertWrapper;
        }

        public async Task<string> GetDocumentSasLinkAsync(int caseId, int documentId)
        {
            //var content = await GetHttpContentAsync($"cases/{caseId}/documents/{documentId}");
            //var stringContent = await content.ReadAsStringAsync();

            //var response = _jsonConvertWrapper.DeserializeObject<DocumentExtractionResponse>(stringContent);

            //return response.DocumentSasDetails.DocumentSasUrl;
            return "https://sadevcmsdocumentservices.blob.core.windows.net/cms-documents/004fb83c-8206-4992-84e0-19d868e76624/title-543096053-331205364-unusedMaterials-otherMaterials-a.txt?sp=r&st=2022-03-14T13:37:24Z&se=2022-03-14T21:37:24Z&spr=https&sv=2020-08-04&sr=b&sig=WZ9dg00cZ%2BRWSQMH3QS8kUtK%2FTL5KnNQdpdSUOS%2Fq9U%3D";
        }

        private async Task<HttpContent> GetHttpContentAsync(string requestUri)
        {
            var request = _documentExtractionHttpRequestFactory.Create(requestUri);
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
