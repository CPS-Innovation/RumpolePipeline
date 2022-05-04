using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace coordinator.Domain.Tracker
{
    public class TrackerDocument
    {
        [JsonProperty("documentId")]
        public string DocumentId { get; set; }

        [JsonProperty("pdfBlobName")]
        public string PdfBlobName { get; set; }

        [JsonConverter(typeof(StringEnumConverter))]
        [JsonProperty("status")]
        public DocumentStatus Status { get; set; }
    }
}