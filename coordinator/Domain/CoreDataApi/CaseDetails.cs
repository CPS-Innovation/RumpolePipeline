using Newtonsoft.Json;
using System.Collections.Generic;

namespace coordinator.Domain.CoreDataApi
{
    public class CaseDetails
    {
        [JsonProperty("id")]
        public int Id { get; set; }

        [JsonProperty("documents")]
        public List<Document> Documents { get; set; } = new List<Document>();
    }
}
