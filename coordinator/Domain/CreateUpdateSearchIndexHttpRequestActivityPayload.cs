namespace coordinator.Domain;

public class CreateUpdateSearchIndexHttpRequestActivityPayload : BasePipelinePayload
{
    public string DocumentId { get; set; }
}
