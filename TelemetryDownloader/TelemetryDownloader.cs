using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MyPubgTelemetry.MatchMetadataModel;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace MyPubgTelemetry.Downloader
{
    public class TelemetryDownloader
    {
        public class DownloadProgressEventArgs : EventArgs
        {
            public int Value { get; set; }
            public int Max { get; set; }
            public string Message { get; set; }
            public bool Rewrite { get; set; }
        }

        public event EventHandler<DownloadProgressEventArgs> DownloadProgressEvent;

        public void DownloadTelemetryForPlayers(string playerNames)
        {
            TelemetryApp app = TelemetryApp.App;
            List<JToken> players = app.ApiGetPlayersByNames(playerNames);
            for (int i = 0; i < players.Count; i++)
            {
                JToken player = players[i];
                string name = player.SelectToken("attributes.name").ToString();
                List<JToken> matches = player.SelectToken("relationships.matches.data").ToList();
                int nApiMatches = 0;
                int cApiMatches = matches.Count;
                int nDownloaded = 0;
                int nCacheHits = 0;
                DownloadProgressEvent?.Invoke(this, new DownloadProgressEventArgs
                {
                    Value = i,
                    Max = players.Count,
                    Message = $"\nChecking for new telemetry data for {name} ..."
                });
                foreach (JToken match in matches)
                {
                    string matchId = match["id"].ToString();
                    DownloadTelemetryForMatchId(matchId, ++nApiMatches, cApiMatches, ref nDownloaded, ref nCacheHits);
                }

                DownloadProgressEvent?.Invoke(this, new DownloadProgressEventArgs
                {
                    Value = i,
                    Max = players.Count,
                    Message = "\n... telemetry data update complete."
                });
                string[] teleFiles = Directory.GetFiles(app.TelemetryDir, "*.json");
                int nTeleFiles = teleFiles.Length;
                DownloadProgressEvent?.Invoke(this, new DownloadProgressEventArgs
                {
                    Value = i,
                    Max = players.Count,
                    Message =
                        $"Summary: MatchesOnline: {cApiMatches}, NewMatchesDownloaded: {nDownloaded}, AlreadyDownloaded: {nCacheHits}, TotalStored: {nTeleFiles}\n"
                });
            }
        }

        public List<NormalizedMatch> DownloadTelemetryForPlayers2(string playerNames)
        {
            var playerNamesLocal = TelemetryApp.App.NormalizePlayerCsv(playerNames);
            if (string.IsNullOrEmpty(playerNamesLocal))
            {
                throw new ArgumentException("Failed to parse playerNames: " + playerNames);
            }
            playerNames = playerNamesLocal;
            List<JToken> players = TelemetryApp.App.ApiGetPlayersByNames(playerNames);
            List<NormalizedMatch> nms = new List<NormalizedMatch>();
            foreach (JToken player in players)
            {
                string name = player.SelectToken("attributes.name").ToString();
                List<JToken> matches = player.SelectToken("relationships.matches.data").ToList();
                foreach (JToken match in matches)
                {
                    string matchId = match["id"].ToString();
                    NormalizedMatch nm = DownloadMatchMetadataForMatchId(matchId);
                    nms.Add(nm);
                }
            }
            return nms;
        }

        public class NormalizedMatch
        {
            public string Id { get; set; }
            [JsonIgnore]
            public MatchMetadata Model { get; set; }
            [JsonIgnore]
            public string JsonStr { get; set; }
            public List<NormalizedRoster> Rosters { get; } = new List<NormalizedRoster>();
            public bool MetadataAlreadyDownloaded { get; set; }
            public bool TelemetryAlreadyDownloaded { get; set; }
        }

        public class NormalizedRoster
        {
            public List<MatchIncluded> Players { get; } = new List<MatchIncluded>();
            [JsonIgnore]
            public MatchIncluded Roster { get; set;  }
        }

        public NormalizedMatch DownloadMatchMetadataForMatchId(string matchId)
        {
            string mtOutputFileName = "mt-" + matchId + ".json.gz";
            string mmOutputFileName = "mm-" + matchId + ".json";
            string mtOutputFilePath = Path.Combine(TelemetryApp.App.TelemetryDir, mtOutputFileName);
            string mmOutputFilePath = Path.Combine(TelemetryApp.App.MatchDir, mmOutputFileName);

            NormalizedMatch nm = new NormalizedMatch { Id = matchId };
            if (!File.Exists(mmOutputFilePath))
            {
                nm.JsonStr = TelemetryApp.App.ApiGetMatch(matchId);
                var model = JsonConvert.DeserializeObject<MatchMetadata>(nm.JsonStr);
                nm.Model = model;
                nm.JsonStr = PrettyPrintJson(nm.JsonStr);

                List<string> rosterIds = model.Data.Relationships.Rosters.Data.Select(x => x.Id).ToList();
                foreach (string rosterId in rosterIds)
                {

                    MatchIncluded includedRoster = model.Included.First(x => x.Id == rosterId);
                    List<string> participantIds = includedRoster.Relationships.Participants.Data.Select(x => x.Id).ToList();

                    NormalizedRoster roster = new NormalizedRoster {Roster = includedRoster};
                    nm.Rosters.Add(roster);

                    foreach (string participantId in participantIds)
                    {
                        MatchIncluded parti = model.Included.First(x => x.Id == participantId);
                        roster.Players.Add(parti);
                    }
                }
                File.WriteAllText(mmOutputFilePath, nm.JsonStr);
            }
            else
            {
                nm.MetadataAlreadyDownloaded = true;
            }
            if (File.Exists(mtOutputFilePath))
            {
                nm.TelemetryAlreadyDownloaded = true;
            }
            return nm;
        }

        public void DownloadTelemetryForMatchId(string matchId, int counter, int count,
            ref int nDownloaded, ref int nCacheHits)
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
                DownloadProgressEvent?.Invoke(this, new DownloadProgressEventArgs
                {
                    Rewrite = true,
                    Value = counter,
                    Max = count,
                    Message = $"[{counter}/{count}] Downloading match metadata."
                });
            }
            else
            {
                matchJson = File.ReadAllText(mmOutputFilePath);
            }

            if (File.Exists(mtOutputFilePath))
            {
                DownloadProgressEvent?.Invoke(this, new DownloadProgressEventArgs
                {
                    Rewrite = true,
                    Value = counter,
                    Max = count,
                    Message = $"[{counter}/{count}] Telemetry {matchId} already downloaded. Skip."
                });
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
                DownloadProgressEvent?.Invoke(this, new DownloadProgressEventArgs
                {
                    Rewrite = true,
                    Value = counter,
                    Max = count,
                    Message = $"[{counter}/{count}] Downloading {uri.AbsolutePath}"
                });
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
    }
}