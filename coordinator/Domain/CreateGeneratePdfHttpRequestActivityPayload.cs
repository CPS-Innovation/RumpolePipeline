namespace coordinator.Domain
{
    public class CreateTextExtractorHttpRequestActivityPayload
    {
        public int CaseId { get; set; }

        public string DocumentId { get; set; }

        public string BlobName { get; set; }
    }
}
