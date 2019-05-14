using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Reflection;
using System.Runtime.InteropServices;

namespace MyPubgTelemetry
{
    public class TelemetryApp
    {
        public string DefaultApiKeyFile { get; }
        public string AppName { get; }
        public string DataDir { get; }
        public string TelemetryDir { get; }
        public string MatchDir { get; }
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
            DataDir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), AppName);
            MatchDir = Path.Combine(DataDir, "match_files");
            TelemetryDir = Path.Combine(DataDir, "telemetry_files");
            DefaultApiKeyFile = Path.Combine(DataDir, "pubg-apikey.txt");
            Directory.CreateDirectory(DataDir);
            Directory.CreateDirectory(TelemetryDir);
            Directory.CreateDirectory(MatchDir);
            if (File.Exists(DefaultApiKeyFile))
            {
                ApiKey = File.ReadAllText(DefaultApiKeyFile)?.Trim();
            }
        }

        public static void OpenFolderInFileExplorer(string path)
        {
            Process.Start("explorer.exe", path);
        }

        public static void SelectFileInFileExplorer(string dir)
        {
            Process.Start("explorer.exe", "/select," + dir);
        }

        public static void OpenUrlInWebBrowser(string url)
        {
            ProcessStartInfo psi = new ProcessStartInfo(url) {UseShellExecute = true};
            Process.Start(psi);
        }
    }

    public class TelemetryFile
    {
        public FileInfo FileInfo { set; get; }
        public string Title { set; get; }
        public DateTime? MatchDate { get; set; }
        public ISet<string> Squad { get; set; }
        public bool TelemetryMetaDataLoaded { get; set; }
        public int Index { get; set; }
        public int SquadKills { get; set; }
        public PreparedData PreparedData { get; set; }

        public StreamReader NewTelemetryReader()
        {
            Stream stream = new BufferedStream(FileInfo.OpenRead(), 1024 * 10);
            if (FileInfo.Name.EndsWith(".gz", StringComparison.CurrentCultureIgnoreCase))
            {
                stream = new GZipStream(stream, CompressionMode.Decompress);
            }
            var streamReader = new StreamReader(stream);
            return streamReader;
        }

        public Stream NewMatchMetaDataReader()
        {
            BufferedStream bufferedStream = new BufferedStream(FileInfo.OpenRead(), 1024 * 10);
            if (FileInfo.Name.EndsWith(".gz", StringComparison.CurrentCultureIgnoreCase))
            {
                return new GZipStream(bufferedStream, CompressionMode.Decompress);
            }
            return bufferedStream;
        }
    }

    public class PreparedData
    {
        public Dictionary<string, List<TelemetryEvent>> PlayerToEvents { get; } =
            new Dictionary<string, List<TelemetryEvent>>();
        public Dictionary<DateTime, Dictionary<string, List<TelemetryEvent>>> TimeToPlayerToEvents { get; } =
            new Dictionary<DateTime, Dictionary<string, List<TelemetryEvent>>>();
        public List<TelemetryEvent> NormalizedEvents { get; } = new List<TelemetryEvent>();
        public HashSet<string> Squad { get; } = new HashSet<string>();
        public TelemetryFile File { get; set; }
    }

    public class TelemetryEvent
    {
        public DateTime _D;
        public string _T;
        public float damage;
        public TelemetryPlayer victim;
        public TelemetryPlayer character;
        public TelemetryPlayer attacker;
        public TelemetryPlayer killer;
        public bool skip;
    }

    public class TelemetryPlayer
    {
        public string name;
        public float health;
        public int teamId = -1;
        public string accountId;
    }

    public static class TelemetryAppExtensions
    {
        public static TValue GetValueOrDefault<TKey, TValue>(this IDictionary<TKey, TValue> dict, TKey key)
        {
            return dict.GetValueOrDefault(key, default(TValue));
        }

        public static TValue GetValueOrDefault<TKey, TValue>(this IDictionary<TKey, TValue> dict, TKey key, TValue defVal)
        {
            return dict.GetValueOrDefault(key, () => defVal);
        }

        public static TValue GetOrAdd<TKey, TValue>(this IDictionary<TKey, TValue> dict, TKey key, Func<TValue> defValSelector)
        {
            if (!dict.TryGetValue(key, out TValue value))
                dict.Add(key, value = defValSelector());
            return value;
        }

        public static V GetValueOrDefault<K, V>(this IDictionary<K, V> dict, K key, Func<V> defValSelector)
        {
            return dict.TryGetValue(key, out V value) ? value : defValSelector();
        }
    }

}
