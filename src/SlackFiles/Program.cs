using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;

namespace SlackFiles {
    public class Program {
        public static void Main(string[] args) {
            try {
                // run an async task on the threadpool
                Task.Run(async () => await MainAsync(args)).Wait();
            }
            catch (Exception e) {
                // catch all for any errors
                Console.WriteLine(e);
            }
            finally {
                // wait for enter before exiting
                Console.WriteLine("Press enter to exit...");
                Console.ReadLine();
            }
        }

        public static async Task MainAsync(string[] args) {
            // load configuration from both json config file, and command line args
            var config = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: true)
                //.AddEnvironmentVariables()
                .AddCommandLine(args)
                .Build();

            AppSettings settings = new AppSettings();
            config.Bind(settings);

            MainApp app = new MainApp(settings);
            await app.Run();
        }
    }
}
