using Newtonsoft.Json;

namespace coordinator.Domain.Tracker
{
    public class TrackerDocument
    {
        [JsonProperty("documentId")]
        public int DocumentId { get; set; }

        [JsonProperty("pdfBlobName")]
        public string PdfBlobName { get; set; }
    }
}