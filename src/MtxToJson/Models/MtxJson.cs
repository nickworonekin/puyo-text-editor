using Newtonsoft.Json;
using System.Collections.Generic;

namespace MtxToJson.Models
{
    public class MtxJson
    {
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Ignore)]
        public bool Has64BitOffsets { get; set; }

        public List<List<string>> Entries { get; set; }
    }
}
