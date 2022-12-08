using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Common.Domain.DocumentExtraction;

namespace Common.Domain.Requests;

public class EvaluateExistingDocumentsRequest
{
    public EvaluateExistingDocumentsRequest(long caseId, IEnumerable<CaseDocument> caseDocuments, Guid correlationId)
    {
        CaseId = caseId;
        CaseDocuments = caseDocuments;
        CorrelationId = correlationId;
    }
    
    [Required]
    public long CaseId { get; set; }
    
    [Required]
    public IEnumerable<CaseDocument> CaseDocuments { get; set; }
    
    [Required]
    public Guid CorrelationId { get; set; }
}
