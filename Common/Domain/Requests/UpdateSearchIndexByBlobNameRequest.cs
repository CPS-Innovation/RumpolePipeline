using System;

namespace Common.Domain.Requests;

public class UpdateSearchIndexByBlobNameRequest
{
    public UpdateSearchIndexByBlobNameRequest(long caseId, string blobName, Guid correlationId)
    {
        CaseId = caseId;
        BlobName = blobName;
        CorrelationId = correlationId;
    }
    
    public long CaseId { get; set; }

    public string BlobName { get; set; }
    
    public Guid CorrelationId { get; set; }
}
