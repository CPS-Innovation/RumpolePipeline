using System.ComponentModel.DataAnnotations;

namespace Common.Domain.Requests;

public class UpdateSearchIndexRequest
{
    [Required]
    public string CaseId { get; set; }

    [Required]
    public string DocumentId { get; set; }
}
