namespace coordinator.Domain
{
    public class GetCaseDocumentsActivityPayload
    {
        public int CaseId { get; set; }

        public string AccessToken { get; set; }
    }
}
