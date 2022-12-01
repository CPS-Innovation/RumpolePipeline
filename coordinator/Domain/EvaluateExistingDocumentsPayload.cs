using System;
using System.Collections.Generic;
using Common.Domain.DocumentExtraction;

namespace coordinator.Domain;

public class EvaluateExistingDocumentsPayload : BasePipelinePayload
{
    public EvaluateExistingDocumentsPayload(string caseUrn, long caseId, IEnumerable<CaseDocument> caseDocuments, Guid correlationId)
        : base(caseUrn, caseId, correlationId)
    {
        CaseDocuments = caseDocuments;
    }
        
    public IEnumerable<CaseDocument> CaseDocuments { get; set; }
}