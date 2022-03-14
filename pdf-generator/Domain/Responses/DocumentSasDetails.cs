using Newtonsoft.Json;

namespace pdf_generator.Domain.Responses
{
    public class DocumentSasDetails
    {
        [JsonProperty("id")]
        public int Id { get; set; }

        [JsonProperty("documentSasUrl")]
        public string DocumentSasUrl { get; set; }
    }
}
