using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using FormatWith;
using Newtonsoft.Json;
using SlackFiles.ApiEntities;

namespace SlackFiles {
    public class MainApp {
        private AppSettings settings;
        public MainApp(AppSettings settings) {
            this.settings = settings;
        }

        public async Task Run() {
            HttpClient httpClient = new HttpClient();
            // set auth header
            httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", settings.AuthToken);

            Console.WriteLine("Getting channel list...");
            var channels = await GetChannelList(httpClient);
            Console.WriteLine($"{channels.Count} channels found.");

            Console.WriteLine("Getting file list...");
            var files = await GetFileList(httpClient);
            Console.WriteLine($"{files.Count} files found.");

            if (settings.Download) {
                Console.WriteLine("Downloading files...");
                List<DownloadFileResult> downloadResults = await DownloadFiles(files, channels, httpClient);
                Console.WriteLine($"{downloadResults.Count(r => r.Success)} of {files.Count} files downloaded.");
                Console.WriteLine("Errors:");
                foreach (var errorResult in downloadResults.Where(r => !r.Success)) {
                    Console.WriteLine($"{errorResult.File.Name}: {errorResult.Message}");
                }
            }
            else {
                Console.WriteLine("Skipping download.");
            }

            if (settings.Delete) {
                // CONFIRM DELETE
                Console.WriteLine($"WARNING: You are about to delete {files.Count} files. Confirm [y/N]");
                if (Console.ReadKey().Key == ConsoleKey.Y) {
                    Console.WriteLine("Deleting files...");
                    List<DeleteFileResult> deleteResults = await DeleteFiles(files, httpClient);
                    Console.WriteLine($"{deleteResults.Count(r => r.Success)} of {files.Count} files deleted.");
                    Console.WriteLine("Errors:");
                    foreach (var errorResult in deleteResults.Where(r => !r.Success)) {
                        Console.WriteLine($"{errorResult.File.Name}: {errorResult.Message}");
                    }
                }
                else {
                    Console.WriteLine("Decided against delete.");
                }
            }
            else {
                Console.WriteLine("Skipping delete.");
            }
        }

        private async Task<DeleteFileResult> DeleteFile(SlackFileEntry file, HttpClient existingClient = null) {
            if (existingClient == null) existingClient = new HttpClient();

            // this is the url of the file delete api
            string slackChannelsApiUrl = "https://slack.com/api/files.delete";

            // set up the content that will be posted as form parameters
            HttpContent content = new FormUrlEncodedContent(new Dictionary<string, string>() {
                ["token"] = settings.AuthToken,
                ["file"] = file.Id
            });

            string lastError = "Success";

            for (int attempt = 1; attempt <= settings.DownloadAttempts; attempt++) {
                try {
                    Console.WriteLine($"DELETE (attempt {attempt}): {file.Name}");

                    // Post the request to the api, and get the response
                    HttpResponseMessage response = await existingClient.PostAsync(slackChannelsApiUrl, content);
                    lastError = response.StatusCode.ToString();

                    if (response.StatusCode == System.Net.HttpStatusCode.OK) {
                        // read the result from the response out as a string
                        string jsonResultString = await response.Content.ReadAsStringAsync();

                        // deserialize the string (which is a JSON object) into a POCO object
                        DeleteFileSlackResponse result = JsonConvert.DeserializeObject<DeleteFileSlackResponse>(jsonResultString);

                        if (!result.Success) {
                            Console.WriteLine($"ERROR (attempt {attempt}): {file.Name} - {result.Error}");
                        }

                        // return the result
                        return new DeleteFileResult() {
                            Success = result.Success,
                            Message = result.Error ?? "Success",
                            File = file
                        };
                    }
                    Console.WriteLine($"{response.StatusCode} (attempt {attempt}): {file.Name}");
                }
                catch (Exception ex) {
                    lastError = ex.ToString();
                    Console.WriteLine(ex);
                }
            }

            // return the result
            return new DeleteFileResult() {
                Success = false,
                Message = lastError,
                File = file
            };
        }

        private async Task<List<DeleteFileResult>> DeleteFiles(List<SlackFileEntry> files, HttpClient existingClient = null) {
            if (existingClient == null) existingClient = new HttpClient();

                List<Task<DeleteFileResult>> tasks = new List<Task<DeleteFileResult>>();

                SemaphoreSlim requestLimiter = new SemaphoreSlim(settings.MaxConcurrentRequests, settings.MaxConcurrentRequests);

                foreach (var file in files) {
                    tasks.Add(Task.Run<DeleteFileResult>(async () => {
                        await requestLimiter.WaitAsync();
                        DeleteFileResult result = await DeleteFile(file, existingClient);
                        requestLimiter.Release();
                        return result;
                    }));
                }

                // wait for all delete tasks to complete
                await Task.WhenAll(tasks);

                return tasks.Select(t => t.Result).ToList();
            }

        private async Task<List<SlackChannelEntry>> GetChannelList(HttpClient existingClient = null) {
            if (existingClient == null) existingClient = new HttpClient();
            // this is the url of the channel list api
            string slackChannelsApiUrl = "https://slack.com/api/channels.list";

            // set up the content that will be posted as form parameters
            HttpContent content = new FormUrlEncodedContent(new Dictionary<string, string>() {
                ["token"] = settings.AuthToken
            });

            // Post the request to the api, and get the response
            HttpResponseMessage response = await existingClient.PostAsync(slackChannelsApiUrl, content);

            // read the result from the response out as a string
            string jsonResultString = await response.Content.ReadAsStringAsync();

            // deserialize the string (which is a JSON object) into a POCO object
            ChannelListSlackResponse result = JsonConvert.DeserializeObject<ChannelListSlackResponse>(jsonResultString);

            if (!result.Success) {
                Console.WriteLine($"Error getting channel list: {result.Error}");
            }

            // return the files
            return result?.Channels ?? new List<SlackChannelEntry>();
        }

        private async Task<List<SlackFileEntry>> GetFileList(HttpClient existingClient = null) {
            if (existingClient == null) existingClient = new HttpClient();
            // this is the url of the file list api
            string slackFilesApiURL = "https://slack.com/api/files.list";

            int currentPage = 1;

            List<SlackFileEntry> allFiles = new List<SlackFileEntry>();

            while (true) {
                Console.WriteLine($"Getting files list page {currentPage}");
                // set up the content that will be posted as form parameters
                HttpContent content = new FormUrlEncodedContent(new Dictionary<string, string>() {
                    ["token"] = settings.AuthToken,
                    ["page"] = currentPage.ToString()//,
                    //["count"] = "999" // this appears to be the largest amount of files we can grab per page
                });

                // Post the request to the api, and get the response
                HttpResponseMessage response = await existingClient.PostAsync(slackFilesApiURL, content);

                // read the result from the response out as a string
                string jsonResultString = await response.Content.ReadAsStringAsync();

                // deserialize the string (which is a JSON object) into a POCO object
                FilesListSlackResponse result = JsonConvert.DeserializeObject<FilesListSlackResponse>(jsonResultString);

                if (!result.Success) {
                    Console.WriteLine($"Error getting file list: {result.Error}");
                    break;
                }

                if (result.Files != null) {
                    allFiles.AddRange(result.Files);
                }

                currentPage = result.PagingInfo.CurrentPage + 1;
                if (currentPage > result.PagingInfo.TotalPages) {
                    break;
                }
            }

            // return the files
            return allFiles;
        }

        private async Task<List<DownloadFileResult>> DownloadFiles(IEnumerable<SlackFileEntry> files,
            List<SlackChannelEntry> channels, HttpClient existingClient = null) {
            if (existingClient == null) existingClient = new HttpClient();

            List<Task<DownloadFileResult>> tasks = new List<Task<DownloadFileResult>>();

            SemaphoreSlim downloadLimiter = new SemaphoreSlim(settings.MaxConcurrentRequests, settings.MaxConcurrentRequests);

            foreach (var file in files) {
                tasks.Add(Task.Run<DownloadFileResult>(async () => {
                    await downloadLimiter.WaitAsync();
                    DownloadFileResult result = await DownloadFile(file, channels, existingClient);
                    downloadLimiter.Release();
                    return result;
                }));
            }

            // wait for all download tasks to complete
            await Task.WhenAll(tasks);

            return tasks.Select(t => t.Result).ToList();
        }

        private async Task<DownloadFileResult> DownloadFile(SlackFileEntry file, List<SlackChannelEntry> channels, HttpClient existingClient = null) {
            if (existingClient == null) existingClient = new HttpClient();

            // Get the slack file uri
            Uri downloadUri = file.PrivateDownloadUrl ?? file.PrivateUrl;

            if (downloadUri == null) {
                Console.WriteLine($"File {file.Name} has no valid download path available");
            }

            // convert the channel id into the channel name
            string channel = null;
            if (file.Channels != null && file.Channels.Count > 0) channel = file.Channels[0];

            string channelName = null;
            if (!string.IsNullOrWhiteSpace(channel)) {
                channelName = channels.FirstOrDefault(c => c.Id == channel).Name;
            }

            if (string.IsNullOrWhiteSpace(channelName)) {
                if (channel != null) {
                    channelName = channel;
                }
                else {
                    channelName = string.Empty;
                }
            }

            // get a friendly file name that doesn't contain illegal path characters
            string filename = string.Join("_", file.Name.Split(Path.GetInvalidFileNameChars()));

            // get a friendly channel name that doesn't contain illegal path characters
            channelName = string.Join("_", channelName.Split(Path.GetInvalidFileNameChars()));

            // get the output file path by subbing in values into a format string
            string savePath = settings.DownloadPath.FormatWith(new { channel = channelName, filename = filename });

            string lastError = "Success";

            for (int attempt = 1; attempt <= settings.DownloadAttempts; attempt++) {
                try {
                    Console.WriteLine($"GET (attempt {attempt}): {downloadUri}");

                    HttpResponseMessage response = await existingClient.GetAsync(downloadUri, HttpCompletionOption.ResponseHeadersRead);
                    lastError = response.StatusCode.ToString();
                    if (response.StatusCode == System.Net.HttpStatusCode.OK) {
                        try {
                            await response.Content.ReadAsFileAsync(savePath, settings.ForceOverwrite);
                            Console.WriteLine($"{response.StatusCode} (attempt {attempt}): {downloadUri}");
                        }
                        catch (InvalidOperationException) {
                            Console.WriteLine($"CACHE: {file.Name}");
                        }

                        return new DownloadFileResult() { Success = true, Message = lastError, File = file };
                    }
                    Console.WriteLine($"{response.StatusCode} (attempt {attempt}): {downloadUri}");
                }
                catch (Exception ex) {
                    lastError = ex.ToString();
                    Console.WriteLine(ex);
                }
            }

            return new DownloadFileResult() { Success = false, Message = lastError, File = file };
        }
    }
}
