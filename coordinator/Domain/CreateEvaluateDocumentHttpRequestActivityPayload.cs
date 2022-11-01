using System;

namespace coordinator.Domain;

public class CreateEvaluateDocumentHttpRequestActivityPayload : BasePipelinePayload
{
    public CreateEvaluateDocumentHttpRequestActivityPayload(int caseId, string documentId, string lastUpdatedDate, Guid correlationId)
        : base(caseId, correlationId)
    {
        CaseId = caseId;
        DocumentId = documentId;
        LastUpdatedDate = lastUpdatedDate;
    }
    
    public string DocumentId { get; set; }

    public string LastUpdatedDate { get; set; }
}
