using Newtonsoft.Json;

namespace pdf_generator.Domain.Responses
{
    public class DocumentExtractionResponse
    {
        [JsonProperty("caseId")]
        public string CaseId { get; set; }

        [JsonProperty("documentSasDetails")]
        public DocumentSasDetails DocumentSasDetails { get; set; }
    }
}
