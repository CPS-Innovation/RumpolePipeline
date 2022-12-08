using Common.Constants;

namespace Common.Domain.Responses;

public class EvaluateExistingDocumentResponse
{
    public EvaluateExistingDocumentResponse(long caseId, string blobName, bool updateSearchIndex, DocumentEvaluationResult evaluationResult)
    {
        CaseId = caseId;
        BlobName = blobName;
        UpdateSearchIndex = updateSearchIndex;
        EvaluationResult = evaluationResult;
    }
    
    public DocumentEvaluationResult EvaluationResult { get; set; }

    public long CaseId { get; set; }

    public string BlobName { get; set; }
        
    public bool UpdateSearchIndex { get; set; }
}
