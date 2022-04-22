using System.Net.Http;
using System.Net.Http.Headers;

namespace pdf_generator.Factories
{
    public class DocumentExtractionHttpRequestFactory : IDocumentExtractionHttpRequestFactory
    {
        public HttpRequestMessage Create(string requestUri, string accessToken)
        {
            var request = new HttpRequestMessage(HttpMethod.Get, requestUri);
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
            return request;
        }
    }
}
