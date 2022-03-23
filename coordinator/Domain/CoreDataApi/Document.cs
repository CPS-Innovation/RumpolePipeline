using Newtonsoft.Json;

namespace coordinator.Domain.CoreDataApi
{
    public class Document
    {
        [JsonProperty("id")]
        //TODO is this int?
        public int Id { get; set; }

        //TODO can I rename this class to document type?
        [JsonProperty("type")]
        public Type Type { get; set; }
    }
}
