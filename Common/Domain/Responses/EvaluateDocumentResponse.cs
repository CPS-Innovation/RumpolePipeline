using Common.Constants;

namespace Common.Domain.Responses
{
    public class EvaluateDocumentResponse
    {
        public EvaluateDocumentResponse(long caseId, string documentId, long versionId, bool updateSearchIndex, DocumentEvaluationResult evaluationResult)
        {
            CaseId = caseId;
            DocumentId = documentId;
            VersionId = versionId;
            UpdateSearchIndex = updateSearchIndex;
            EvaluationResult = evaluationResult;
        }
        
        public DocumentEvaluationResult EvaluationResult { get; set; }

        public long CaseId { get; set; }

        public string DocumentId { get; set; }

        public long VersionId { get; set; }

        public bool UpdateSearchIndex { get; set; }
    }
}
