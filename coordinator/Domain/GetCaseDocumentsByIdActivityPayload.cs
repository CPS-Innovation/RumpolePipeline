namespace coordinator.Domain
{
    public class GetCaseDocumentsByIdActivityPayload
    {
        public int CaseId { get; set; }

        public string AccessToken { get; set; }
    }
}
