namespace Common.Domain.Responses
{
    public class GeneratePdfResponse
    {
        public string BlobName { get; set; }

        public bool AlreadyProcessed { get; set; } = false;

        public bool UpdateSearchIndex { get; set; } = false;
    }
}