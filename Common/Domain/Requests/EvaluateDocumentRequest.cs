using System.ComponentModel.DataAnnotations;

namespace Common.Domain.Requests;

public class EvaluateDocumentRequest
{
    public EvaluateDocumentRequest(int caseId, string documentId, string lastUpdatedDate)
    {
        CaseId = caseId;
        DocumentId = documentId;
        LastUpdatedDate = lastUpdatedDate;
    }
    
    [Required]
    public int CaseId { get; set; }

    [Required]
    public string DocumentId { get; set; }
        
    [Required] 
    public string LastUpdatedDate { get; set; }
}
