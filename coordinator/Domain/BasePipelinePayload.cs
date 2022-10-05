using System;

namespace coordinator.Domain;

public class BasePipelinePayload
{
    public Guid CorrelationId { get; set; }

    public int CaseId { get; set; }
}