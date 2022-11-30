using System;

namespace coordinator.Domain
{
    public class CaseDocumentOrchestrationPayload : BasePipelinePayload
    {
        public CaseDocumentOrchestrationPayload(string caseUrn, long caseId, string documentType, string documentCategory, string documentId, long versionId, string fileName, string upstreamToken, Guid correlationId)
            : base(caseUrn, caseId, correlationId)
        {
            DocumentType = documentType;
            DocumentCategory = documentCategory;
            DocumentId = documentId;
            VersionId = versionId;
            FileName = fileName;
            UpstreamToken = upstreamToken;
        }

        public string DocumentType { get; set; }

        public string DocumentCategory { get; set; }
        
        public string DocumentId { get; set; }

        public long VersionId { get; set; }

        public string FileName { get; set; }

        public string UpstreamToken { get; set; }
    }
}
