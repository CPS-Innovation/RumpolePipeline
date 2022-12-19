using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Common.Domain.DocumentEvaluation;

namespace Common.Domain.Requests;

public class ProcessEvaluateDocumentsRequest
{
    public ProcessEvaluateDocumentsRequest(long caseId, List<DocumentToRemove> documentsToRemove, List<DocumentToUpdate> documentsToUpdate)
    {
        CaseId = caseId;
        DocumentsToRemove = documentsToRemove;
        DocumentsToUpdate = documentsToUpdate;
    }
    
    [Required]
    public long CaseId { get; set; }
    
    [Required]
    public List<DocumentToRemove> DocumentsToRemove { get; set; }
    
    [Required]
    public List<DocumentToUpdate> DocumentsToUpdate { get; set; }
}
