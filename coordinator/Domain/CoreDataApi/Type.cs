using Newtonsoft.Json;

namespace coordinator.Domain.CoreDataApi
{
    public class Type
    {
        [JsonProperty("code")]
        public string Code { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }
    }
}
