namespace coordinator.Domain.Requests
{
    public class TextExtractorRequest
    {
        public int CaseId { get; set; }

        public string DocumentId { get; set; }

        public string BlobName { get; set; }
    }
}