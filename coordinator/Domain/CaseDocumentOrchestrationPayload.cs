namespace coordinator.Domain
{
    public class CaseDocumentOrchestrationPayload
    {
        public int CaseId { get; set; }

        public string DocumentId { get; set; }

        public string FileName { get; set; }

        public string AccessToken { get; set; }
    }
}
