using System;

namespace coordinator.Domain;

public class CreateEvaluateDocumentHttpRequestActivityPayload : BasePipelinePayload
{
    public CreateEvaluateDocumentHttpRequestActivityPayload(string caseUrn, long caseId, string documentId, long versionId, Guid correlationId)
        : base(caseUrn, caseId, correlationId)
    {
        CaseId = caseId;
        DocumentId = documentId;
        VersionId = versionId;
    }
    
    public string DocumentId { get; set; }

    public long VersionId { get; set; }
}
