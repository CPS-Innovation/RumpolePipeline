using System;

namespace coordinator.Domain
{
    public class CreateTextExtractorHttpRequestActivityPayload : BasePipelinePayload
    {
        public CreateTextExtractorHttpRequestActivityPayload(int caseId, string documentId, string lastUpdatedDate, string blobName, Guid correlationId)
            : base(caseId, correlationId)
        {
            DocumentId = documentId;
            LastUpdatedDate = lastUpdatedDate;
            BlobName = blobName;
        }
        
        public string DocumentId { get; set; }

        public string LastUpdatedDate { get; set; }

        public string BlobName { get; set; }
    }
}
