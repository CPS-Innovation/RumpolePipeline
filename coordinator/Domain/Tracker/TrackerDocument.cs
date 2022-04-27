using Newtonsoft.Json;

namespace coordinator.Domain.Tracker
{
    public class TrackerDocument
    {
        [JsonProperty("documentId")]
        public string DocumentId { get; set; }

        [JsonProperty("pdfBlobName")]
        public string PdfBlobName { get; set; }
    }
}