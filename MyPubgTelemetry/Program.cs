using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace MyPubgTelemetry
{
    class Program
    {
        // Your username.
        const string USERNAME = "wckd";
        const string APPNAME = "MyPubgTelemetry";

        readonly HttpClient _httpClient = new HttpClient(new HttpClientHandler()
        {
        });
        private string _apiKey = null;
        private string _appDir;

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
            foreach (JToken match in matches)
            {
                string matchId = match["id"].ToString();
                //Console.WriteLine(match);
                Console.WriteLine(matchId);
                DownloadTelemetryForMatchId(matchId);
            }
        }

        private void DownloadTelemetryForMatchId(string matchId)
        {
            string matchJson = ApiGetMatch(matchId);
            JObject oMatch = JObject.Parse(matchJson);
            string telemetryId = oMatch.SelectToken("data.relationships.assets.data[0].id").Value<string>();
            //Console.WriteLine(matchId);
            //Console.WriteLine("=================");
            //Console.WriteLine("******telemetryId******");
            //Console.WriteLine(telemetryId);
            //Console.WriteLine("******telemetryId******");
            var oIncluded = oMatch["included"];
            //Console.WriteLine(oIncluded);
            var oAsset = oIncluded.First(x => x["id"].Value<string>() == telemetryId);
            //Console.WriteLine("oAsset " + oAsset);
            var url = oAsset.SelectToken("attributes.URL").Value<string>();
            Console.WriteLine("Downloading telemetry from: " + url);
            string teleDir = Path.Combine(_appDir, "telemetry_files");
            Console.WriteLine(teleDir);
            Directory.CreateDirectory(teleDir);
            using (var stream = _httpClient.GetStreamAsync(url).Result)
            {
                
                string outputFileName = @"telem-" + telemetryId + ".json.gz";
                string outputFilePath = Path.Combine(teleDir, outputFileName);
                using (FileStream ostream = new FileStream(outputFilePath, FileMode.OpenOrCreate))
                {
                    Task task = stream.CopyToAsync(ostream);
                    while (!task.IsCompleted)
                    {
                        Console.Write("\r" + ostream.Position);
                        Thread.Sleep(10);
                    }
                    Console.WriteLine();
                }

                string outputPath = Path.GetFullPath(outputFilePath);
                Console.WriteLine("Saved telemetry file to:\n" + outputPath);
            }
        }

        private void InitAppData()
        {
            _appDir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), APPNAME);
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
        }

        private static void CollectApiKey(string apiKeyFile)
        {
            Console.WriteLine("Paste your API key into this window and then press enter.");
            string line = Console.ReadLine();
            File.WriteAllText(apiKeyFile, line);
        }

        private string ApiGetPlayer(string player)
        {
            String url = "players?filter[playerNames]=" + player;
            Console.WriteLine(new Uri(_httpClient.BaseAddress, url));
            string result = _httpClient.GetStringAsync(url).Result;
            //Console.WriteLine(">>>>>>>>>>>>>>> ApiGetPlayer(" + player + ") >>>>>>>>>>>>>>>>>>>>");
            //Console.WriteLine(PrettyPrintJson(result));
            //Console.WriteLine("<<<<<<<<<<<<<<< ApiGetPlayer(" + player + ") <<<<<<<<<<<<<<<<<<<<");
            return result;
        }

        private string ApiGetMatch(string matchId)
        {
            string result = _httpClient.GetStringAsync("matches/" + matchId).Result;
            //Console.WriteLine(">>>>>>>>>>>>>>> ApiGetMatch(" + matchId + ") >>>>>>>>>>>>>>>>>>>>");
            //Console.WriteLine(PrettyPrintJson(result));
            //Console.WriteLine("<<<<<<<<<<<<<<< ApiGetMatch(" + matchId + ") <<<<<<<<<<<<<<<<<<<<");
            return result;
        }

        public static string PrettyPrintJson(string json)
        {
            JObject jsonObject = JObject.Parse(json);
            return jsonObject.ToString(Formatting.Indented);
        }

        static void Main(string[] args)
        {
            Program program = new Program();
            program.MMain(args);
        }
    }
}

