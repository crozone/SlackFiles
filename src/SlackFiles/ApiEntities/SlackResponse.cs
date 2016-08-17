using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace SlackFiles.ApiEntities {
    public abstract class SlackResponse {
        [JsonProperty("ok")]
        public bool Success { get; set; }

        [JsonProperty("error")]
        public string Error { get; set; }
    }
}
