using System;

namespace coordinator.Domain
{
    public class CoordinatorOrchestrationPayload : BasePipelinePayload
    {
        public CoordinatorOrchestrationPayload(string caseUrn, long caseId, bool forceRefresh, string accessToken, Guid correlationId)
            : base(caseUrn, caseId, correlationId)
        {
            ForceRefresh = forceRefresh;
            AccessToken = accessToken;
        }
        
        public bool ForceRefresh { get; set; }

        public string AccessToken { get; set; }
    }
}