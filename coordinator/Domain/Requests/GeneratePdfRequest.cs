namespace coordinator.Domain.Requests
{
    public class GeneratePdfRequest
    {
        public int CaseId { get; set; }

        public string DocumentId { get; set; }
    }
}