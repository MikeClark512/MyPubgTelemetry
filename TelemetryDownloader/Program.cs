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
        // Change to your username(s)
        public const string USERNAME = "wckd,Celaven,Giles333,Solunth";

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

            List<JToken> players = app.ApiGetPlayersByNames(USERNAME);
            foreach (JToken player in players)
            {
                string name = player.SelectToken("attributes.name").ToString();
                List<JToken> matches = player.SelectToken("relationships.matches.data").ToList();
                int nApiMatches = 0;
                int cApiMatches = matches.Count;
                int nDownloaded = 0;
                int nCacheHits = 0;
                Console.WriteLine($"\nChecking for new telemetry data for {name} ...");
                foreach (JToken match in matches)
                {
                    string matchId = match["id"].ToString();
                    DownloadTelemetryForMatchId(matchId, ++nApiMatches, cApiMatches, ref nDownloaded, ref nCacheHits,
                        (_) => { });
                }
                Console.WriteLine("\n... telemetry data update complete.");
                string[] teleFiles = Directory.GetFiles(app.TelemetryDir, "*.json");
                int nTeleFiles = teleFiles.Length;
                Console.WriteLine(
                    $"\nSummary: MatchesOnline: {cApiMatches}, MatchesDownloaded: {nDownloaded}, AlreadyDownloaded: {nCacheHits}, TotalStored: {nTeleFiles}\n");
            }
            if (Debugger.IsAttached)
            {
                Console.ReadKey();
            }
        }

        public void DownloadTelemetryForMatchId(string matchId, int counter, int count,
            ref int nDownloaded, ref int nCacheHits, Action<string> progress)
        {
            string mtOutputFileName = "mt-" + matchId + ".json.gz";
            string mmOutputFileName = "mm-" + matchId + ".json";
            string mtOutputFilePath = Path.Combine(TelemetryApp.App.TelemetryDir, mtOutputFileName);
            string mmOutputFilePath = Path.Combine(TelemetryApp.App.MatchDir, mmOutputFileName);

            string matchJson = null;
            if (!File.Exists(mmOutputFilePath))
            {
                matchJson = PrettyPrintJson(TelemetryApp.App.ApiGetMatch(matchId));
                File.WriteAllText(mmOutputFilePath, matchJson);
                ConsoleRewrite($"[{counter}/{count}] Downloading match metadata.");
            }
            else
            {
                matchJson = File.ReadAllText(mmOutputFilePath);
            }
            if (File.Exists(mtOutputFilePath))
            {

                ConsoleRewrite($"[{counter}/{count}] Telemetry {matchId} already downloaded. Skip.");
                nCacheHits++;
                return;
            }
            JObject oMatch = JObject.Parse(matchJson);
            string telemetryId = oMatch.SelectToken("data.relationships.assets.data[0].id").Value<string>();
            JToken oIncluded = oMatch["included"];
            JToken oAsset = oIncluded.First(x => x["id"].Value<string>() == telemetryId);
            string url = oAsset.SelectToken("attributes.URL").Value<string>();
            using (Stream result = TelemetryApp.App.HttpClient.GetStreamAsync(url).Result)
            using (var stream = new GZipStream(result, CompressionMode.Decompress))
            {
                Uri uri = new Uri(url);
                ConsoleRewrite($"[{counter}/{count}] Downloading {uri.AbsolutePath}");
                string pJson = PrettyPrintTelemetryJson(stream, out DateTime matchDateTime);
                WriteStringToGzFile(mtOutputFilePath, pJson, matchDateTime);
                nDownloaded++;
            }
        }

        private static void WriteStringToGzFile(string outputFilePath, string pJson, DateTime matchDateTime)
        {
            using (var fs = new FileStream(outputFilePath, FileMode.OpenOrCreate))
            using (var gz = new GZipStream(fs, CompressionLevel.Optimal))
            using (var sw = new StreamWriter(gz, Encoding.UTF8))
            {
                sw.Write(pJson);
            }
            File.SetCreationTime(outputFilePath, matchDateTime);
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

        public static string PrettyPrintTelemetryJson(Stream stream, out DateTime dt)
        {
            JArray json = JArray.Load(new JsonTextReader(new StreamReader(stream, Encoding.UTF8)));
            dt = json[0]["_D"].Value<DateTime>();
            return json.ToString(Formatting.Indented);
        }

        public static string PrettyPrintJson(string json)
        {
            JToken jsonObject = JToken.Parse(json);
            return jsonObject.ToString(Formatting.Indented);
        }

        static void Main(string[] args)
        {
            Program program = new Program();
            program.MMain(args);
        }
    }
}

