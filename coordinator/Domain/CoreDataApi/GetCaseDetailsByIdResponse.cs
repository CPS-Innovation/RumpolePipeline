using Newtonsoft.Json;

namespace coordinator.Domain.CoreDataApi
{
    public class GetCaseDetailsByIdResponse
    {
        [JsonProperty("caseDetails")]
        public CaseDetails CaseDetails { get; set; }
    }
}
