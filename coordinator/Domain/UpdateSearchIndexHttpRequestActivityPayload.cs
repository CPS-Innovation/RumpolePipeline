using System;

namespace coordinator.Domain;

public class UpdateSearchIndexHttpRequestActivityPayload : BasePipelinePayload
{
    public UpdateSearchIndexHttpRequestActivityPayload(string caseUrn, long caseId, string documentId, Guid correlationId)
        : base(caseUrn, caseId, correlationId)
    {
        DocumentId = documentId;
    }
    
    public string DocumentId { get; set; }
}
