using Common.Constants;

namespace Common.Domain.Responses
{
    public class EvaluateDocumentResponse
    {
        public DocumentEvaluationResult EvaluationResult { get; set; }

        public string CaseId { get; set; }

        public string DocumentId { get; set; }

        public bool UpdateSearchIndex { get; set; }
    }
}
