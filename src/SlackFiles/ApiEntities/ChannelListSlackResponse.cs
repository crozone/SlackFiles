using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace SlackFiles.ApiEntities {
    public class ChannelListSlackResponse : SlackResponse {
        [JsonProperty("channels")]
        public List<SlackChannelEntry> Channels { get; set; }
    }

    public class SlackChannelEntry {
        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("is_channel")]
        public bool IsChannel { get; set; }

        [JsonProperty("created")]
        public long CreatedTimeUnix { get; set; }

        public DateTimeOffset CreatedTime
        {
            get
            {
                return DateTimeOffset.FromUnixTimeSeconds(CreatedTimeUnix);
            }
        }

        [JsonProperty("creator")]
        public string CreatorId { get; set; }

        [JsonProperty("is_archived")]
        public bool IsArchived { get; set; }

        [JsonProperty("is_general")]
        public bool IsGeneral { get; set; }

        [JsonProperty("is_member")]
        public bool IsMember { get; set; }

        [JsonProperty("members")]
        public List<string> Members { get; set; }
    }
}
