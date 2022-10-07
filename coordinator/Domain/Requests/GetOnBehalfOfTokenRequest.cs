using System;

namespace coordinator.Domain.Requests;

public class GetOnBehalfOfTokenRequest
{
    public string AccessToken { get; set; }

    public Guid CorrelationId { get; set; }
}