using System;

namespace coordinator.Domain;

public abstract class BasePipelinePayload
{
    protected BasePipelinePayload(int caseId, Guid correlationId)
    {
        CaseId = caseId;
        CorrelationId = correlationId;
    }
    
    public int CaseId { get; set; }
    
    public Guid CorrelationId { get; set; }

}