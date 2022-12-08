using System;

namespace Common.Domain.Requests;

public class UpdateSearchIndexByVersionRequest
{
    public UpdateSearchIndexByVersionRequest(long caseId, string documentId, long versionId, Guid correlationId)
    {
        CaseId = caseId;
        DocumentId = documentId;
        VersionId = versionId;
        CorrelationId = correlationId;
    }
    
    public long CaseId { get; set; }

    public string DocumentId { get; set; }
    
    public long VersionId { get; set; }
    
    public Guid CorrelationId { get; set; }
}
