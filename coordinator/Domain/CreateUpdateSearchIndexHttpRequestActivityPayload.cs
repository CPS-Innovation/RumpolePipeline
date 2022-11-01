using System;

namespace coordinator.Domain;

public class CreateUpdateSearchIndexHttpRequestActivityPayload : BasePipelinePayload
{
    public CreateUpdateSearchIndexHttpRequestActivityPayload(int caseId, string documentId, Guid correlationId)
        : base(caseId, correlationId)
    {
        DocumentId = documentId;
    }
    
    public string DocumentId { get; set; }
}
