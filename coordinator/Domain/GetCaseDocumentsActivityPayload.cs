using System;

namespace coordinator.Domain
{
    public class GetCaseDocumentsActivityPayload : BasePipelinePayload
    {
        public GetCaseDocumentsActivityPayload(string caseUrn, long caseId, string accessToken, Guid correlationId) : 
            base(caseUrn, caseId, correlationId)
        {
            AccessToken = accessToken;
        }

        public string AccessToken { get; set; }
    }
}
