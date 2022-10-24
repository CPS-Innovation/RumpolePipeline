namespace coordinator.Domain;

public class CreateEvaluateDocumentHttpRequestActivityPayload : BasePipelinePayload
{
    public string DocumentId { get; set; }

    public string MaterialId { get; set; }

    public string LastUpdatedDate { get; set; }
}
