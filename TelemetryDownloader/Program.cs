using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net.Http;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Web;

namespace MyPubgTelemetry.Downloader
{
    class Program
    {
        // Comma separated list of usernames. Case sensitive.
        // Minimum one username, maximum six.
        public const string USERNAMES = "wckd,Celaven,Giles333,Solunth";

        void MMain(string[] args)
        {
            // Increase Console.ReadLine character limit
            Console.SetIn(new StreamReader(Console.OpenStandardInput(1024), Console.InputEncoding, false, 1024));
            TelemetryApp app = TelemetryApp.App;

            if (string.IsNullOrEmpty(app.ApiKey))
            {
                app.ApiKey = GetApiKeyInteractive(app);
            }
            Console.WriteLine("API Key Filename: " + app.DefaultApiKeyFile);
            Console.WriteLine("API Key Contents: " + app.ApiKey);

            TelemetryDownloader downloader = new TelemetryDownloader();
            downloader.DownloadProgressEvent += (sender, eventArgs) =>
            {
                if (eventArgs.Rewrite)
                    ConsoleRewrite(eventArgs.Message);
                else
                    Console.WriteLine(eventArgs.Message);
            };
            downloader.DownloadTelemetryForPlayers(USERNAMES);

            if (Debugger.IsAttached)
            {
                Console.ReadKey();
            }
        }

        private void ConsoleRewrite(string msg)
        {
            int remainder = Console.BufferWidth - msg.Length - 1;
            if (remainder > 0) msg += new string(' ', remainder);
            Console.CursorLeft = 0;
            Console.Write(msg);
        }

        private static string GetApiKeyInteractive(TelemetryApp app)
        {
            Console.WriteLine("Paste your API key into this window and then press enter.");
            string line = Console.ReadLine()?.Trim();
            File.WriteAllText(app.DefaultApiKeyFile, line);
            return line;
        }

        static void Main(string[] args)
        {
            Program program = new Program();
            program.MMain(args);
        }
    }
}

