using System.Collections.Generic;
using Common.Domain.DocumentExtraction;

namespace coordinator.Domain;

public class CreateEvaluateExistingDocumentsHttpRequestActivityPayload : BasePipelinePayload
{
    public List<CaseDocument> CaseDocuments { get; set; }
}
