using Newtonsoft.Json;

namespace coordinator.Domain.CoreDataApi
{
    public class GetCaseDetailsByIdResponse
    {
        [JsonProperty("case")]
        public CaseDetails CaseDetails { get; set; }
    }
}
