using System;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Reflection;

namespace MyPubgTelemetry
{
    public class TelemetryApp
    {
        public string DefaultApiKeyFile { get; }
        public string AppName { get; }
        public string AppDir { get; }
        public string TelemetryDir { get; }
        public HttpClient HttpClient { get; }
        private string _apiKey;

        public string ApiKey
        {
            get => _apiKey;
            set
            {
                _apiKey = value;
                HttpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _apiKey);
            }
        }

        public TelemetryApp()
        {
            HttpClientHandler httpClientHandler = new HttpClientHandler();
            HttpClient = new HttpClient(httpClientHandler);
            //_httpClient.DefaultRequestHeaders.Add("Cache-Control", "no-store, max-age=0");
            HttpClient.DefaultRequestHeaders.Add("Accept", "application/vnd.api+json");
            HttpClient.BaseAddress = new Uri("https://api.pubg.com/shards/steam/");
            AppName = Assembly.GetExecutingAssembly().GetTypes().Select(x => x.Namespace).First().Split('.').First();
            AppDir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), AppName);
            TelemetryDir = Path.Combine(AppDir, "telemetry_files");
            DefaultApiKeyFile = Path.Combine(AppDir, "pubg-apikey.txt");
            Directory.CreateDirectory(AppDir);
            Directory.CreateDirectory(TelemetryDir);
            if (File.Exists(DefaultApiKeyFile))
            {
                ApiKey = File.ReadAllText(DefaultApiKeyFile)?.Trim();
            }
        }
    }

    public class TelemetryEvent
    {
        public DateTime _D;
        public string _T;
        public float damage;
        public TelemetryPlayer victim;
        public TelemetryPlayer character;
    }

    public class TelemetryPlayer
    {
        public string name;
        public float health;
    }
}
