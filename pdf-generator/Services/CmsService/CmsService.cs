using System.Net.Http;
using System.Threading.Tasks;
using Domain;

namespace Services.CmsService
{
    public class CmsService
    {
        private readonly HttpClient _httpClient;

        public CmsService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<CmsDocument> GetDocument(string url)
        {
            var response = await _httpClient.GetAsync(url);
            var contentType = response.Content.Headers.ContentType;

            return new CmsDocument
            {
                ContentType = contentType,
                // todo: what is best practice for returning a stream wrt disposing it
                Stream = await response.Content.ReadAsStreamAsync()
            };

        }
    }
}