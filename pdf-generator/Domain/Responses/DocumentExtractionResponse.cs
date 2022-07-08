using System.Diagnostics.CodeAnalysis;
using Newtonsoft.Json;

namespace pdf_generator.Domain.Responses
{
    [ExcludeFromCodeCoverage]
    public class DocumentExtractionResponse
    {
        [JsonProperty("caseId")]
        public string CaseId { get; set; }

        [JsonProperty("documentSasDetails")]
        public DocumentSasDetails DocumentSasDetails { get; set; }
    }
}
