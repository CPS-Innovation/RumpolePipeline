using System.ComponentModel.DataAnnotations;

namespace Common.Domain.Requests;

public class EvaluateDocumentRequest
{
    public EvaluateDocumentRequest(long caseId, string documentId, long versionId)
    {
        CaseId = caseId;
        DocumentId = documentId;
        VersionId = versionId;
    }
    
    [Required]
    public long CaseId { get; set; }

    [Required]
    public string DocumentId { get; set; }
        
    [Required] 
    public long VersionId { get; set; }
}
