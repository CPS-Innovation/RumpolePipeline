namespace coordinator.Domain
{
    public class CoordinatorOrchestrationPayload
    {
        public int CaseId { get; set; }

        public bool ForceRefresh { get; set; }
    }
}