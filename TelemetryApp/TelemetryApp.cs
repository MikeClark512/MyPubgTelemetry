using Iesi.Collections.Generic;
using Microsoft.Win32.SafeHandles;
using MyPubgTelemetry.ApiMatchModel;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
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
using System.Runtime.CompilerServices;
using System.Runtime.ConstrainedExecution;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using System.Threading;
using System.Windows.Forms;

namespace MyPubgTelemetry
{
    public class TelemetryApp
    {
        public static TelemetryApp App { get; set; }
        static TelemetryApp()
        {
            App = new TelemetryApp();
        }

        public string DefaultApiKeyFile { get; set; }
        public string AppName { get; set; }
        public string DataDir { get; set; }
        public string TelemetryDir { get; set; }
        public string MatchDir { get; set; }
        public HttpClient HttpClient { get; set; }
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
        public Regex RegexCsv { get; } = new Regex(@"\s*,\s*", RegexOptions.IgnoreCase);

        private TelemetryApp()
        {
            ReInit();
        }

        public void ReInit()
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

        public static void DebugThreadWriteLine(string msg, [CallerMemberName] string caller = "")
        {
            Debug.WriteLine($"T:{Thread.CurrentThread.Name}{Thread.CurrentThread.ManagedThreadId} C:{caller} {msg}");
        }

        public string NormalizePlayerCsv(string csv)
        {
            List<string> strings = RegexCsv.Split(csv).ToList();
            strings.RemoveAll(string.IsNullOrEmpty);
            return string.Join(",", strings);
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
            ProcessStartInfo psi = new ProcessStartInfo(url) { UseShellExecute = true };
            Process.Start(psi);
        }

        public List<JToken> ApiGetPlayersByNames(string players)
        {
            String url = "players?filter[playerNames]=" + players;
            string result = HttpClient.GetStringAsync(url).Result;
            JObject playersObject = JObject.Parse(result);
            return playersObject["data"].ToList();
        }

        public static void FileWriteAllTextAtomic(string path, string contents)
        {
            FileInfo temp = new FileInfo(Path.GetTempFileName());
            try
            {
                File.WriteAllText(temp.FullName, contents);
                File.Move(temp.FullName, path);
            }
            finally
            {
                try
                {
                    if (temp.Exists)
                    {
                        temp.Delete();
                    }
                }
                catch (Exception)
                {
                    // Just trying to be maximally tidy. It is highly unusual to be unable to delete a temp file that we just created for ourselves.
                    // (And we have not shared a reference to the temp file path with any other function or program.)
                    // If this exception occurs, something very bad is happening that we're probably powerless to prevent or help the user with.
                }
            }
        }

        public static bool EnvBooly(string str)
        {
            return ToBooly(Environment.GetEnvironmentVariable(str));
        }


        public static bool ToBooly(string str)
        {
            TryParseBooly(str, out bool result);
            return result;
        }

        public static bool TryParseBooly(string str, out bool result)
        {
            if (bool.TryParse(str, out bool bResult))
            {
                result = bResult;
                return true;
            }
            str = str?.Trim();
            string[] truthy = { "1", "on", "yes" };
            if (truthy.Contains(str, StringComparer.OrdinalIgnoreCase))
            {
                result = true;
                return true;
            }
            string[] falsy = { "0", "off", "no" };
            if (falsy.Contains(str, StringComparer.OrdinalIgnoreCase))
            {
                result = false;
                return true;
            }
            if (long.TryParse(str, out long lResult))
            {
                result = lResult > 0;
                return true;
            }
            result = false;
            return false;
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

    public class TelemetryFile : MatchModelStats
    {
        public FileInfo FileInfo { set; get; }

        private string _title = "";
        public string Title
        {
            set => _title = value;
            get
            {
                if (string.IsNullOrEmpty(_title) && Squad != null)
                {
                    _title = string.Join(", ", Squad);
                }
                return _title;
            }
        }

        public DateTime? MatchDate { get; set; }
        public ISet<string> Squad { get; set; }
        public bool TelemetryMetaDataLoaded { get; set; }
        public int Index { get; set; }
        public PreparedData PreparedData { get; set; }
        public Mutex Mutex { get; } = new Mutex();
        public NormalizedMatch NormalizedMatch { get; set; }
        public NormalizedRoster NormalizedRoster { get; set; }

        public TelemetryFile() { }

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

        public StreamReader NewMatchMetaDataReader()
        {
            return NewMatchMetaDataReader(FileMode.Open, FileAccess.Read, FileShare.ReadWrite, out FileStream fs);
        }

        public StreamReader NewMatchMetaDataReader(FileMode fileMode, FileAccess fileAccess, FileShare fileShare,
            out FileStream fileStream)
        {
            fileStream = new FileStream(FileInfo.FullName, fileMode, fileAccess, fileShare);
            BufferedStream bufferedStream = new BufferedStream(fileStream, 1024 * 10);
            Stream stream = bufferedStream;
            if (FileInfo.Name.EndsWith(".gz", StringComparison.CurrentCultureIgnoreCase))
            {
                stream = new GZipStream(bufferedStream, CompressionMode.Decompress);
            }
            return new StreamReader(stream);
        }

        public string GetMatchId()
        {
            if (FileInfo == null) return null;
            return TelemetryApp.TelemetryFilenameToMatchId(FileInfo.Name);
        }
    }

    public class PreparedData
    {
        public Dictionary<string, List<TelemetryEvent>> PlayerToEvents { get; } =
            new Dictionary<string, List<TelemetryEvent>>();
        public IDictionary<DateTime, Dictionary<string, List<TelemetryEvent>>> TimeToPlayerToEvents { get; } =
            new SortedDictionary<DateTime, Dictionary<string, List<TelemetryEvent>>>();
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

    public class NormalizedMatch
    {
        public string Id { get; set; }
        [JsonIgnore]
        public MatchModel Model { get; set; }
        [JsonIgnore]
        public string JsonStr { get; set; }
        public List<NormalizedRoster> Rosters { get; } = new List<NormalizedRoster>();
        public bool MetadataAlreadyDownloaded { get; set; }
        public bool TelemetryAlreadyDownloaded { get; set; }
    }

    public class NormalizedRoster
    {
        public List<MatchModelIncluded> Players { get; } = new List<MatchModelIncluded>();
        [JsonIgnore]
        public MatchModelIncluded Roster { get; set; }
    }

    public abstract class VolatileBindableBase : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        public readonly Control MainControl;
        protected VolatileBindableBase(Control mainControl)
        {
            MainControl = mainControl;
        }
        [ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success)]
        protected bool SetProperty<T>(ref T storage, T value, [CallerMemberName] string propertyName = null)
        {
            lock (this)
            {
                if (Equals(storage, value)) return false;
                storage = value;
                if (MainControl.InvokeRequired)
                {
                    MainControl.Invoke((MethodInvoker)delegate
                    {
                        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
                    });
                }
                else
                {
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
                }
                return true;
            }
        }
    }

    public static class TelemetryAppExtensions
    {
        public static HashSet<TSource> ToHashSet<TSource>(this IEnumerable<TSource> source)
        {
            return source.ToHashSet(null);
        }

        public static HashSet<TSource> ToHashSet<TSource>(this IEnumerable<TSource> source, IEqualityComparer<TSource> comparer)
        {
            if (source == null) throw new ArgumentNullException(nameof(source));
            return new HashSet<TSource>(source, comparer);
        }

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

        public static TValue GetValueOrDefault<TKey, TValue>(this IDictionary<TKey, TValue> dict, TKey key, Func<TValue> defValSelector)
        {
            return dict.TryGetValue(key, out TValue value) ? value : defValSelector();
        }

        public static LinkedHashSet<TSource> ToLinkedHashSet<TSource>(this IEnumerable<TSource> source)
        {
            return new LinkedHashSet<TSource>(source);
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

        //public static IEnumerable<TSource> WhereNot<TSource>(this IEnumerable<TSource> source, Func<TSource, bool> predicate)
        //{
        //    return source.Where(x => !predicate(x));
        //}

        [DllImport("kernel32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool SetFileTime(SafeFileHandle hFile, IntPtr lpCreationTime, IntPtr lpLastAccessTime, IntPtr lpLastWriteTime);
    }

}
