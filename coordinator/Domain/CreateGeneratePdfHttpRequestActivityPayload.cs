namespace coordinator.Domain
{
    public class CreateTextExtractorHttpRequestActivityPayload : BasePipelinePayload
    {
        public string DocumentId { get; set; }

        public string BlobName { get; set; }
    }
}
