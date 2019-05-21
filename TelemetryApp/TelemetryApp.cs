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
using System.Text.RegularExpressions;
using System.Threading;
using Microsoft.Win32.SafeHandles;
using Newtonsoft.Json.Linq;

namespace MyPubgTelemetry
{
    public class TelemetryApp
    {
        public static TelemetryApp App { get; set; }
        static TelemetryApp()
        {
            App = new TelemetryApp();
        }

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

        private TelemetryApp()
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

        public void OpenFolderInFileExplorer(string path)
        {
            Process.Start("explorer.exe", path);
        }

        public void SelectFileInFileExplorer(string dir)
        {
            Process.Start("explorer.exe", "/select," + dir);
        }

        public void OpenUrlInWebBrowser(string url)
        {
            ProcessStartInfo psi = new ProcessStartInfo(url) {UseShellExecute = true};
            Process.Start(psi);
        }

        public List<JToken> ApiGetPlayersByNames(string players)
        {
            String url = "players?filter[playerNames]=" + players;
            string result = HttpClient.GetStringAsync(url).Result;
            JObject playersObject = JObject.Parse(result);
            return playersObject["data"].ToList();
        }

        public string ApiGetMatch(string matchId)
        {
            return HttpClient.GetStringAsync("matches/" + matchId).Result;
        }

        public static string TelemetryFilenameToMatchId(string fname)
        {
            fname = Path.GetFileName(fname);
            if (string.IsNullOrEmpty(fname)) return null;

            while (fname != Path.GetFileNameWithoutExtension(fname))
            {
                fname = Path.GetFileNameWithoutExtension(fname);
            }

            const string pfx = "mt-";
            if (fname.StartsWith(pfx))
            {
                fname = fname.Substring(pfx.Length);
            }

            return fname;
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
        public Mutex Mutex { get; } = new Mutex();

        public StreamReader NewTelemetryReader()
        {
            FileStream fs = new FileStream(FileInfo.FullName, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
            Stream stream = new BufferedStream(fs, 1024 * 10);
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

        public static string GetTimeZoneAbbreviation(this DateTime time)
        {
            string tz;
            if (time.Kind == DateTimeKind.Local)
            {
                tz = time.IsDaylightSavingTime() ? TimeZoneInfo.Local.DaylightName : TimeZoneInfo.Local.StandardName;
            }
            else
            {
                tz = "UTC";
            }
            string tza = Regex.Replace(tz, @"[a-z\s]", "");
            return tza;
        }

        public static bool SetFileCreateTime(this SafeFileHandle hFile, long creationTime)
        {
            GCHandle pin = GCHandle.Alloc(creationTime, GCHandleType.Pinned);
            try
            {
                return SetFileTime(hFile, pin.AddrOfPinnedObject(), IntPtr.Zero, IntPtr.Zero);
            }
            finally
            {
                pin.Free();
            }
        }

        [DllImport("kernel32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool SetFileTime(SafeFileHandle hFile, IntPtr lpCreationTime, IntPtr lpLastAccessTime, IntPtr lpLastWriteTime);
    }

}
