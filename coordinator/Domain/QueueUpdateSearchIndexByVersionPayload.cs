using System;

namespace coordinator.Domain;

public class QueueUpdateSearchIndexByVersionPayload : BasePipelinePayload
{
    public QueueUpdateSearchIndexByVersionPayload(string caseUrn, long caseId, string documentId, long versionId, Guid correlationId)
        : base(caseUrn, caseId, correlationId)
    {
        DocumentId = documentId;
        VersionId = versionId;
    }
    
    public string DocumentId { get; set; }

    public long VersionId { get; set; }
}
