using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace SlackFiles.ApiEntities {
    public class FilesListSlackResponse : SlackResponse {
        [JsonProperty("files")]
        public List<SlackFileEntry> Files { get; set; }

        [JsonProperty("paging")]
        public PagingInfo PagingInfo { get; set; }
    }

    public class SlackFileEntry {
        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("title")]
        public string Title { get; set; }

        [JsonProperty("created")]
        public long CreatedTimeUnix { get; set; }

        public DateTimeOffset CreatedTime
        {
            get
            {
                return DateTimeOffset.FromUnixTimeSeconds(CreatedTimeUnix);
            }
        }

        [JsonProperty("mimetype")]
        public string MimeType { get; set; }

        [JsonProperty("filetype")]
        public string FileType { get; set; }

        [JsonProperty("pretty_type")]
        public string PrettyType { get; set; }

        [JsonProperty("user")]
        public string UserId { get; set; }

        [JsonProperty("size")]
        public long Size { get; set; }

        [JsonProperty("is_public")]
        public bool IsPublic { get; set; }

        [JsonProperty("url_private")]
        public Uri PrivateUrl { get; set; }

        [JsonProperty("url_private_download")]
        public Uri PrivateDownloadUrl { get; set; }

        [JsonProperty("channels")]
        public List<string> Channels { get; set; }

        [JsonProperty("groups")]
        public List<string> Groups { get; set; }
    }

    public class PagingInfo {
        [JsonProperty("count")]
        public int FileCount { get; set; }

        [JsonProperty("total")]
        public int TotalFiles { get; set; }

        [JsonProperty("page")]
        public int CurrentPage { get; set; }

        [JsonProperty("pages")]
        public int TotalPages { get; set; }

    }
}
