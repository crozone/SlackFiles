using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SlackFiles {
    public class AppSettings {
        public string ApiKey { get; set; }
        public string DownloadPath { get; set; }
        public bool Download { get; set; }
        public bool Delete { get; set; }
        public int MaxConcurrentRequests { get; set; }
        public bool ForceOverwrite { get; set; }
        public int DownloadAttempts { get; set; }
    }
}
