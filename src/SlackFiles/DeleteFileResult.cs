using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using SlackFiles.ApiEntities;

namespace SlackFiles {
    public class DeleteFileResult {
        public bool Success { get; set; }
        public string Message { get; set; }
        public SlackFileEntry File { get; set; }
    }
}
