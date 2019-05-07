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
        // Change to your username
        public const string USERNAME = "wckd,Celaven,Giles333,Solunth";

        void MMain(string[] args)
        {
            Console.SetIn(new StreamReader(Console.OpenStandardInput(1024), Console.InputEncoding, false, 1024)); // Increase Console.ReadLine character limit
            TelemetryDownloader td = new TelemetryDownloader();
            TelemetryApp app = td.App;

            if (string.IsNullOrEmpty(app.ApiKey))
            {
                app.ApiKey = GetApiKeyInteractive(app);
            }
            Console.WriteLine("API Key Filename: " + app.DefaultApiKeyFile);
            Console.WriteLine("API Key Contents: " + app.ApiKey);

            List<JToken> players = td.ApiGetPlayersByNames(USERNAME);
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
                    DownloadTelemetryForMatchId(app, matchId, ++nApiMatches, cApiMatches, ref nDownloaded, ref nCacheHits);
                }
                Console.WriteLine("\n... telemetry data update complete.");
                string[] teleFiles = Directory.GetFiles(app.TelemetryDir, "*.json");
                int nTeleFiles = teleFiles.Length;
                Console.WriteLine(
                    $"\nSummary: MatchesOnline: {cApiMatches}, MatchesDownloaded: {nDownloaded}, AlreadyDownloaded: {nCacheHits}, TotalStored: {nTeleFiles}\n");
                //List<JToken> matches = playerObj.SelectToken("data[0].relationships.matches.data").ToList();//["data"][0]["relationships"]["matches"]["data"].ToList();
            }
            //List<JToken> matches = playerObj.SelectToken("data[0].relationships.matches.data").ToList();//["data"][0]["relationships"]["matches"]["data"].ToList();
            if (System.Diagnostics.Debugger.IsAttached)
                Console.ReadKey();
        }


        public void DownloadTelemetryForMatchId(TelemetryApp app, string matchId, int counter, int count,
            ref int nDownloaded, ref int nCacheHits)
        {
            string mtOutputFileName = "mt-" + matchId + ".json";
            string mtOutputFilePath = Path.Combine(app.TelemetryDir, mtOutputFileName);
            if (File.Exists(mtOutputFilePath))
            {

                ConsoleRewrite($"[{counter}/{count}] Telemetry {matchId} already downloaded. Skip.");
                nCacheHits++;
                return;
            }
            string matchJson = ApiGetMatch(app, matchId);
            JObject oMatch = JObject.Parse(matchJson);
            string telemetryId = oMatch.SelectToken("data.relationships.assets.data[0].id").Value<string>();
            JToken oIncluded = oMatch["included"];
            JToken oAsset = oIncluded.First(x => x["id"].Value<string>() == telemetryId);
            string url = oAsset.SelectToken("attributes.URL").Value<string>();
            using (GZipStream stream = new GZipStream(app.HttpClient.GetStreamAsync(url).Result, CompressionMode.Decompress))
            {
                Uri uri = new Uri(url);
                ConsoleRewrite($"[{counter}/{count}] Downloading {uri.AbsolutePath}");
                string pJson = PrettyPrintJsonStream(stream);
                File.WriteAllText(mtOutputFilePath, pJson, Encoding.UTF8);
                nDownloaded++;
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

        private string ApiGetMatch(TelemetryApp app, string matchId)
        {
            return app.HttpClient.GetStringAsync("matches/" + matchId).Result;
        }

        public static string PrettyPrintJsonStream(Stream stream)
        {
            JToken jsonObject = JToken.Load(new JsonTextReader(new StreamReader(stream, Encoding.UTF8)));
            return jsonObject.ToString(Formatting.Indented);
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

