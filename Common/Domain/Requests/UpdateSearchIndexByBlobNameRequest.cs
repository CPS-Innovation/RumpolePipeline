using System;
using System.ComponentModel.DataAnnotations;

namespace Common.Domain.Requests;

public class UpdateSearchIndexByBlobNameRequest
{
    public UpdateSearchIndexByBlobNameRequest(long caseId, string blobName, Guid correlationId)
    {
        CaseId = caseId;
        BlobName = blobName;
        CorrelationId = correlationId;
    }
    
    [Required]
    public long CaseId { get; set; }

    [Required]
    public string BlobName { get; set; }
    
    [Required]
    public Guid CorrelationId { get; set; }
}
