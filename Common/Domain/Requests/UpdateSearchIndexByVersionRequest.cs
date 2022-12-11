using System;
using System.ComponentModel.DataAnnotations;

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
    
    [Required]
    public long CaseId { get; set; }

    [Required]
    public string DocumentId { get; set; }
    
    [Required]
    public long VersionId { get; set; }
    
    [Required]
    public Guid CorrelationId { get; set; }
}
