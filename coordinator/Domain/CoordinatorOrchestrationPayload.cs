namespace coordinator.Domain
{
    public class CoordinatorOrchestrationPayload
    {
        public int CaseId { get; set; }

        //TODO do we need?
        //public string TrackerUrl { get; set; }

        //TODO do we need to force?
        public bool ForceRefresh { get; set; }
    }
}