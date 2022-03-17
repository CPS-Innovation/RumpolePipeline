using System.Collections.Generic;
using Newtonsoft.Json;

namespace coordinator.Tracker
{
    public class TrackerPageArg
    {
        public int DocumentId { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public List<TrackerPageDimensions> PageDimensions { get; set; }
    }
}