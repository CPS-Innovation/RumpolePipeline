namespace coordinator.Domain
{
    public class CoordinatorOrchestrationPayload : BasePipelinePayload
    {
        public bool ForceRefresh { get; set; }

        public string AccessToken { get; set; }
    }
}