using System.ComponentModel.DataAnnotations;

namespace Common.Domain.Requests;

public class EvaluateDocumentRequest
{
    public EvaluateDocumentRequest(long caseId, string documentId, long versionId, string proposedBlobName)
    {
        CaseId = caseId;
        DocumentId = documentId;
        VersionId = versionId;
        ProposedBlobName = proposedBlobName;
    }
    
    [Required]
    public long CaseId { get; set; }

    [Required]
    public string DocumentId { get; set; }
        
    [Required] 
    public long VersionId { get; set; }
    
    [Required]
    public string ProposedBlobName { get; set; }
}
