using System.ComponentModel.DataAnnotations;

namespace Common.Domain.Requests;

public class UpdateSearchIndexRequest
{
    public UpdateSearchIndexRequest(string caseId, string documentId)
    {
        CaseId = caseId;
        DocumentId = documentId;
    }
    
    [Required]
    public string CaseId { get; set; }

    [Required]
    public string DocumentId { get; set; }
}
