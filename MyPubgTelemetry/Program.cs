using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace MyPubgTelemetry
{
    class Program
    {
        // Your username.
        const string USERNAME = "wckd";
        const string APPNAME = "MyPubgTelemetry";
        private string apiKey = null;
        readonly HttpClient httpClient = new HttpClient();

        void MMain(string[] args)
        {
            // Reads API Key and initializes cache
            InitAppData();

            httpClient.DefaultRequestHeaders.Add("Authorization", "Bearer " + apiKey);
            httpClient.DefaultRequestHeaders.Add("Accept", "application/vnd.api+json");
            httpClient.BaseAddress = new Uri("https://api.pubg.com/shards/steam/");

            string playerJson = ApiGetPlayer(USERNAME);
            JObject playerObj = JObject.Parse(playerJson);
            List<JToken> matches = playerObj.SelectToken("data[0].relationships.matches.data").ToList();//["data"][0]["relationships"]["matches"]["data"].ToList();
            //foreach (JToken item in matches)
            //{
            //    Console.WriteLine(item["id"]);
            //}
            string match0 = matches[0]["id"].ToString();
            Console.WriteLine(match0);
            Console.WriteLine("=================");

            string matchJson = ApiGetMatch(match0);
            JObject matchObj = JObject.Parse(matchJson);
            string telemetryId = matchObj.SelectToken("data.relationships.assets.data[0].id").Value<string>();
            Console.WriteLine("******telemetryId******");
            Console.WriteLine(telemetryId);
            Console.WriteLine("******telemetryId******");

            var oIncluded = matchObj["included"];
            //Console.WriteLine(oIncluded);
            var oAsset = oIncluded.First(x => x["id"].Value<string>() == telemetryId);
            Console.WriteLine("oAsset " + oAsset);
            var url = oAsset.SelectToken("attributes.URL").Value<string>();
            Console.WriteLine("Downloading telemetry from: " + url);

            using (var stream = httpClient.GetStreamAsync(url).Result)
            {
                string outputFile = @"telem-" + telemetryId + ".json.gz";
                using (var ostream = new FileStream(outputFile, FileMode.OpenOrCreate))
                {
                    stream.CopyTo(ostream);
                }
                string outputPath = Path.GetFullPath(outputFile);
                Console.WriteLine("Saved telemetry file to:\n" + outputPath);
            }
        }

        private void InitAppData()
        {
            string appDir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), APPNAME);
            Directory.CreateDirectory(appDir);
            string apiKeyFile = Path.Combine(appDir, "pubg-apikey.txt");
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
            if (string.IsNullOrEmpty(content))
            {
                Console.WriteLine("API key file is still empty. Giving up.");
                Environment.Exit(1);
            }

            Console.WriteLine("API Key Filename: " + apiKeyFile);
            Console.WriteLine("API Key Contents: " + content);

            apiKey = content;
        }

        private static void CollectApiKey(string apiKeyFile)
        {
            Console.WriteLine("Paste your API key into this window and then press enter.");
            string line = Console.ReadLine();
            File.WriteAllText(apiKeyFile, line);
        }

        private string ApiGetPlayer(string player)
        {
            string result = httpClient.GetStringAsync("players?filter[playerNames]=" + player).Result;
            Console.WriteLine(">>>>>>>>>>>>>>> ApiGetPlayer(" + player + ") >>>>>>>>>>>>>>>>>>>>");
            Console.WriteLine(PrettyPrintJson(result));
            Console.WriteLine("<<<<<<<<<<<<<<< ApiGetPlayer(" + player + ") <<<<<<<<<<<<<<<<<<<<");
            return result;
        }

        private string ApiGetMatch(string matchId)
        {
            string result = httpClient.GetStringAsync("matches/" + matchId).Result;
            Console.WriteLine(">>>>>>>>>>>>>>> ApiGetMatch(" + matchId + ") >>>>>>>>>>>>>>>>>>>>");
            Console.WriteLine(PrettyPrintJson(result));
            Console.WriteLine("<<<<<<<<<<<<<<< ApiGetMatch(" + matchId + ") <<<<<<<<<<<<<<<<<<<<");
            return result;
        }

        public static string PrettyPrintJson(string json)
        {
            JObject jsonObject = JObject.Parse(json);
            return jsonObject.ToString(Formatting.Indented);
        }

        public static void PrintJson(string json)
        {
            Console.WriteLine(PrettyPrintJson(json));
        }

        static void Main(string[] args)
        {
            Program program = new Program();
            program.MMain(args);
        }
    }
}

