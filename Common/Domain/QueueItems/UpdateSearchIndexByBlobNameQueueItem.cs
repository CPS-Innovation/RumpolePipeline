using System;
using System.ComponentModel.DataAnnotations;

namespace Common.Domain.QueueItems
{
    public class UpdateSearchIndexByBlobNameQueueItem
    {
        public UpdateSearchIndexByBlobNameQueueItem(long caseId, string blobName, Guid correlationId)
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
}
