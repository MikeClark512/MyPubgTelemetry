using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace MyPubgTelemetry
{
    class Program
    {
        // Change to your username
        public const string USERNAME = "wckd";

        private string _apiKey;
        private string _appDir;
        private string _telemetryFilesDir;
        private readonly HttpClient _httpClient = new HttpClient(new HttpClientHandler()
        {
        });

        void MMain(string[] args)
        {
            // Read API Key and initializes cache
            InitAppData();

            _httpClient.DefaultRequestHeaders.Add("Authorization", "Bearer " + _apiKey);
            _httpClient.DefaultRequestHeaders.Add("Accept", "application/vnd.api+json");
            //_httpClient.DefaultRequestHeaders.Add("Cache-Control", "no-store, max-age=0");
            _httpClient.BaseAddress = new Uri("https://api.pubg.com/shards/steam/");
            string playerJson = ApiGetPlayer(USERNAME);
            JObject playerObj = JObject.Parse(playerJson);
            List<JToken> matches = playerObj.SelectToken("data[0].relationships.matches.data").ToList();//["data"][0]["relationships"]["matches"]["data"].ToList();
            int nApiMatches = 0;
            int cApiMatches = matches.Count;
            int nDownloaded = 0;
            int nCacheHits = 0;
            Console.WriteLine("\nChecking for new telemetry data ...");
            foreach (JToken match in matches)
            {
                string matchId = match["id"].ToString();
                DownloadTelemetryForMatchId(matchId, ++nApiMatches, cApiMatches, ref nDownloaded, ref nCacheHits);
            }
            Console.WriteLine("\n... telemetry data update complete.");
            string[] teleFiles = Directory.GetFiles(_telemetryFilesDir, "*.json");
            int nTeleFiles = teleFiles.Length;
            Console.WriteLine($"\nSummary: MatchesOnline: {cApiMatches}, MatchesDownloaded: {nDownloaded}, AlreadyDownloaded: {nCacheHits}, TotalStored: {nTeleFiles}\n");

            if (System.Diagnostics.Debugger.IsAttached)
                Console.ReadKey();
        }

        private void DownloadTelemetryForMatchId(string matchId, int counter, int count, ref int nDownloaded, ref int nCacheHits)
        {
            string mtOutputFileName = "mt-" + matchId + ".json";
            string mtOutputFilePath = Path.Combine(_telemetryFilesDir, mtOutputFileName);
            if (File.Exists(mtOutputFilePath))
            {

                ConsoleRewrite($"[{counter}/{count}] Telemetry {matchId} already downloaded. Skip.");
                nCacheHits++;
                return;
            }
            string matchJson = ApiGetMatch(matchId);
            JObject oMatch = JObject.Parse(matchJson);
            string telemetryId = oMatch.SelectToken("data.relationships.assets.data[0].id").Value<string>();
            var oIncluded = oMatch["included"];
            var oAsset = oIncluded.First(x => x["id"].Value<string>() == telemetryId);
            var url = oAsset.SelectToken("attributes.URL").Value<string>();
            string outputFileName = @"telem-" + telemetryId + ".json";
            string outputFilePath = Path.Combine(_telemetryFilesDir, outputFileName);
            if (File.Exists(outputFilePath))
            {
                ConsoleRewrite($"[{counter}/{count}] Telemetry {matchId} already downloaded; skip.");
                // Migrate old naming style to new naming style
                File.Move(outputFilePath, mtOutputFilePath);
                nCacheHits++;
            }
            else
            {
                using (var stream = new GZipStream(_httpClient.GetStreamAsync(url).Result, CompressionMode.Decompress))
                {
                    Uri uri = new Uri(url);
                    ConsoleRewrite($"[{counter}/{count}] DL {uri.AbsolutePath}");
                    string pJson = PrettyPrintJsonStream(stream);
                    File.WriteAllText(mtOutputFilePath, pJson, Encoding.UTF8);

                    string outputPath = Path.GetFullPath(mtOutputFilePath);
                    nDownloaded++;
                }
            }
        }

        private void ConsoleRewrite(string msg)
        {
            int remainder = Console.BufferWidth - msg.Length - 1;
            if (remainder > 0) msg += new string(' ', remainder);
            Console.CursorLeft = 0;
            Console.Write(msg);
        }

        private void InitAppData()
        {
            string appname = System.Reflection.Assembly.GetExecutingAssembly().GetName().Name;
            _appDir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), appname);
            Directory.CreateDirectory(_appDir);
            string apiKeyFile = Path.Combine(_appDir, "pubg-apikey.txt");
            if (!File.Exists(apiKeyFile))
            {
                Console.WriteLine("API key file not found: " + apiKeyFile);
                CollectApiKey(apiKeyFile);
            }

            var content = File.ReadAllText(apiKeyFile);
            if (string.IsNullOrEmpty(content))
            {
                Console.WriteLine("API key file is empty: " + apiKeyFile);
                CollectApiKey(apiKeyFile);
            }

            content = File.ReadAllText(apiKeyFile);
            content = content.Trim();
            if (string.IsNullOrEmpty(content))
            {
                Console.WriteLine("API key file is still empty. Giving up.");
                Environment.Exit(1);
            }

            Console.WriteLine("API Key Filename: " + apiKeyFile);
            Console.WriteLine("API Key Contents: " + content);

            _apiKey = content;

            _telemetryFilesDir = Path.Combine(_appDir, "telemetry_files");
            Console.WriteLine("Local Storage: " + _telemetryFilesDir);
            Directory.CreateDirectory(_telemetryFilesDir);
            if (!Directory.Exists(_telemetryFilesDir))
            {
                Console.WriteLine("Exiting because couldn't create storage dir: " + _telemetryFilesDir);
                Environment.Exit(1);
            }
        }

        private static void CollectApiKey(string apiKeyFile)
        {
            Console.WriteLine("Paste your API key into this window and then press enter.");
            string line = Console.ReadLine()?.Trim();
            File.WriteAllText(apiKeyFile, line);
        }

        private string ApiGetPlayer(string player)
        {
            String url = "players?filter[playerNames]=" + player;
            //Console.WriteLine(new Uri(_httpClient.BaseAddress, url));
            string result = _httpClient.GetStringAsync(url).Result;
            return result;
        }

        private string ApiGetMatch(string matchId)
        {
            return _httpClient.GetStringAsync("matches/" + matchId).Result;
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

