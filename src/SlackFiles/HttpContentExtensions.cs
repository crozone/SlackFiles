using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace SlackFiles {
    public static class HttpContentExtensions {
        public static Task ReadAsFileAsync(this HttpContent content, string filename, bool overwrite) {
            string fullPath = Path.GetFullPath(filename);
            string dirName = Path.GetDirectoryName(filename);
            Directory.CreateDirectory(dirName);

            if (!overwrite && File.Exists(filename)) {
                throw new InvalidOperationException($"File {fullPath} already exists.");
            }

            FileStream fileStream = null;
            try {
                fileStream = new FileStream(fullPath, FileMode.Create, FileAccess.Write, FileShare.None);
                return content.CopyToAsync(fileStream).ContinueWith(
                     (copyTask) => {
                         fileStream.Dispose();
                     });
            }
            catch {
                fileStream?.Dispose();
                throw;
            }
        }
    }
}
