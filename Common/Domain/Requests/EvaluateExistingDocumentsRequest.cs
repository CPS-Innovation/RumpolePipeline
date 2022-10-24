using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Common.Domain.DocumentExtraction;

namespace Common.Domain.Requests;

public class EvaluateExistingDocumentsRequest
{
    [Required]
    public string CaseId { get; set; }
    
    [Required]
    public List<CaseDocument> CaseDocuments { get; set; }
}
