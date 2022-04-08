using Newtonsoft.Json;

namespace coordinator.Domain.CoreDataApi
{
    public class Document
    {
        [JsonProperty("id")]
        public int Id { get; set; }

        [JsonProperty("type")]
        public Type Type { get; set; }
    }
}
