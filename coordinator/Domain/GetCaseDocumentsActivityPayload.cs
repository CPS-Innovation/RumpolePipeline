using System;

namespace coordinator.Domain
{
    public class GetCaseDocumentsActivityPayload : BasePipelinePayload
    {
        public GetCaseDocumentsActivityPayload(int caseId, string accessToken, Guid correlationId) : 
            base(caseId, correlationId)
        {
            AccessToken = accessToken;
        }
        
        public string AccessToken { get; set; }
    }
}
