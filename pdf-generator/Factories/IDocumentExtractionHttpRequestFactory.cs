using System;
using System.Net.Http;

namespace pdf_generator.Factories
{
    public interface IDocumentExtractionHttpRequestFactory
    {
        HttpRequestMessage Create(string requestUri, string accessToken, Guid correlationId);
    }
}
