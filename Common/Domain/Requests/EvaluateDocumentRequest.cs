using System.ComponentModel.DataAnnotations;

namespace Common.Domain.Requests;

public class EvaluateDocumentRequest
{
    [Required]
    public int CaseId { get; set; }

    [Required]
    public string DocumentId { get; set; }
        
    [Required] 
    public string MaterialId { get; set; }
        
    [Required] 
    public string LastUpdatedDate { get; set; }
}
