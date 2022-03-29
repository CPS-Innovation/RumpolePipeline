namespace coordinator.Domain
{
    public class GetCaseDetailsByIdActivityPayload
    {
        public int CaseId { get; set; }

        public string AccessToken { get; set; }
    }
}
