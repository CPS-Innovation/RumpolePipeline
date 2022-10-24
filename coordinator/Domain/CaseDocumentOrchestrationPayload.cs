namespace coordinator.Domain
{
    public class CaseDocumentOrchestrationPayload : BasePipelinePayload
    {
        public string DocumentId { get; set; }

        public string MaterialId { get; set; }

        public string LastUpdatedDate { get; set; }

        public string FileName { get; set; }
    }
}
