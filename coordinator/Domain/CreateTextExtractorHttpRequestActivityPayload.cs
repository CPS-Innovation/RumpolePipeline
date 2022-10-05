namespace coordinator.Domain
{
    public class CreateGeneratePdfHttpRequestActivityPayload : BasePipelinePayload
    {
        public string DocumentId { get; set; }

        public string FileName { get; set; }
    }
}
