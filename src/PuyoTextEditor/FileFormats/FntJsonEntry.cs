using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace PuyoTextEditor.FileFormats
{
    public class FntJsonEntry
    {
        public int Width { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string Path { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string Data { get; set; }
    }
}
