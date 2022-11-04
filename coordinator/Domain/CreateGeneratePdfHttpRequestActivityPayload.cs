using System;

namespace coordinator.Domain
{
    public class CreateGeneratePdfHttpRequestActivityPayload : BasePipelinePayload
    {
        public CreateGeneratePdfHttpRequestActivityPayload(int caseId, string documentId, string fileName, string lastUpdatedDate, Guid correlationId)
            : base(caseId, correlationId)
        {
            DocumentId = documentId;
            FileName = fileName;
            LastUpdatedDate = lastUpdatedDate;
        }
        
        public string DocumentId { get; set; }

        public string FileName { get; set; }

        public string LastUpdatedDate { get; set; }
    }
}
