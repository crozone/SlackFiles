using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SlackFiles {
    public class AppSettings {
        public string AuthToken { get; set; }
        public string DownloadPath { get; set; } = "./SlackDownload/{channel}/{filename}";
        public bool Download { get; set; } = false;
        public bool Delete { get; set; } = false;
        public int MaxConcurrentRequests { get; set; } = 64;
        public bool ForceOverwrite { get; set; } = false;
        public int DownloadAttempts { get; set; } = 5;
    }
}
