using System;

namespace coordinator.Domain
{
    public class CoordinatorOrchestrationPayload : BasePipelinePayload
    {
        public CoordinatorOrchestrationPayload(int caseId, bool forceRefresh, string accessToken, Guid correlationId)
            : base(caseId, correlationId)
        {
            ForceRefresh = forceRefresh;
            AccessToken = accessToken;
        }
        
        public bool ForceRefresh { get; set; }

        public string AccessToken { get; set; }
    }
}