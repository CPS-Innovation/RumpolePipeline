using System;

namespace coordinator.Domain
{
    public class CaseDocumentOrchestrationPayload : BasePipelinePayload
    {
        public CaseDocumentOrchestrationPayload(string caseUrn, long caseId, string documentId, long versionId, string fileName, Guid correlationId)
            : base(caseUrn, caseId, correlationId)
        {
            DocumentId = documentId;
            VersionId = versionId;
            FileName = fileName;
        }
        
        public string DocumentId { get; set; }

        public long VersionId { get; set; }

        public string FileName { get; set; }
    }
}
