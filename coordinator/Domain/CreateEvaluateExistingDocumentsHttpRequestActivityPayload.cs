using System;
using System.Collections.Generic;
using Common.Domain.DocumentExtraction;

namespace coordinator.Domain;

public class CreateEvaluateExistingDocumentsHttpRequestActivityPayload : BasePipelinePayload
{
    public CreateEvaluateExistingDocumentsHttpRequestActivityPayload(string caseUrn, long caseId, List<CaseDocument> caseDocuments, Guid correlationId)
        : base(caseUrn, caseId, correlationId)
    {
        CaseDocuments = caseDocuments;
    }
    
    public List<CaseDocument> CaseDocuments { get; set; }
}
