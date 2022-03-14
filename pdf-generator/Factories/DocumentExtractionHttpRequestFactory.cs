using System.Net.Http;

namespace pdf_generator.Factories
{
    public class DocumentExtractionHttpRequestFactory : IDocumentExtractionHttpRequestFactory
    {
        public HttpRequestMessage Create(string requestUri)
        {
            return new HttpRequestMessage(HttpMethod.Get, requestUri);
        }
    }
}
