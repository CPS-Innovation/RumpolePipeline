using System;

namespace coordinator.Domain
{
    public class CaseDocumentOrchestrationPayload : BasePipelinePayload
    {
        public CaseDocumentOrchestrationPayload(int caseId, string documentId, string lastUpdatedDate, string fileName, Guid correlationId)
            : base(caseId, correlationId)
        {
            DocumentId = documentId;
            LastUpdatedDate = lastUpdatedDate;
            FileName = fileName;
        }
        
        public string DocumentId { get; set; }

        public string LastUpdatedDate { get; set; }

        public string FileName { get; set; }
    }
}
