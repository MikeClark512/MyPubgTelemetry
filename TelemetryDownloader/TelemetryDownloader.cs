using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MyPubgTelemetry.ApiMatchModel;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace MyPubgTelemetry.Downloader
{
    public class TelemetryDownloader
    {
        public event EventHandler<DownloadProgressEventArgs> DownloadProgressEvent;

        public async Task<List<NormalizedMatch>> DownloadForPlayersAsync(string playerNames)
        {
            return await Task.Run(() =>
            {
                var playerNamesLocal = TelemetryApp.App.NormalizePlayerCsv(playerNames);
                if (string.IsNullOrEmpty(playerNamesLocal))
                {
                    throw new ArgumentException("Failed to parse playerNames: " + playerNames);
                }
                playerNames = playerNamesLocal;
                List<JToken> players = TelemetryApp.App.ApiGetPlayersByNames(playerNames);
                HashSet<string> matchIds = new HashSet<string>();
                foreach (JToken player in players)
                {
                    string name = player.SelectToken("attributes.name").ToString();
                    List<JToken> matches = player.SelectToken("relationships.matches.data").ToList();
                    foreach (JToken match in matches)
                    {
                        string matchId = match["id"].ToString();
                        matchIds.Add(matchId);
                    }
                }

                BlockingCollection<NormalizedMatch> nms = new BlockingCollection<NormalizedMatch>();
                var parallelOptions = new ParallelOptions() { MaxDegreeOfParallelism = 4 };
                Parallel.ForEach(matchIds, parallelOptions, (matchId, state, i) =>
                {
                    NormalizedMatch nm = DownloadOnlyMatchMetadataForMatchId(matchId, i, matchIds.Count);
                    //NormalizedRoster normalizedRoster = nm.Rosters.FirstOrDefault(r => r.Players.Select(p => p.Attributes.Name).Contains(name));
                    nms.Add(nm);
                });

                DownloadProgressEvent?.Invoke(this, new DownloadProgressEventArgs
                {
                    Value = matchIds.Count,
                    Max = matchIds.Count,
                    Msg = $"Finished loading all match metadata."
                });

                List<NormalizedMatch> nmsToDownload = nms.Where(nm => nm.TelemetryAlreadyDownloaded == false).ToList();
                Parallel.ForEach(nmsToDownload, parallelOptions, (normalizedMatch, state, i) =>
                {
                    DownloadOnlyTelemetryForMatch(normalizedMatch, i, nmsToDownload.Count);
                });
                DownloadProgressEvent?.Invoke(this, new DownloadProgressEventArgs
                {
                    Complete = true,
                    Value = players.Count,
                    Max = players.Count,
                    Msg = $"Summary: Online: {matchIds.Count}, NewDownloaded: {nmsToDownload.Count}"
                });
                return nms.ToList();
            });
        }

        public NormalizedMatch DownloadOnlyMatchMetadataForMatchId(string matchId, long i, int matchesCount)
        {
            string mmOutputFileName = "mm-" + matchId + ".json";
            string mmOutputFilePath = Path.Combine(TelemetryApp.App.MatchDir, mmOutputFileName);

            string mtOutputFileName = "mt-" + matchId + ".json.gz";
            string mtOutputFilePath = Path.Combine(TelemetryApp.App.TelemetryDir, mtOutputFileName);

            string matchJsonStr;
            bool metadataAlreadyDownloaded;
            if (!File.Exists(mmOutputFilePath))
            {
                DownloadProgressEvent?.Invoke(this, new DownloadProgressEventArgs
                {
                    Value = i,
                    Max = matchesCount,
                    Msg = $"Downloading metadata for match {i + 1}/{matchesCount}"
                });
                matchJsonStr = TelemetryApp.App.ApiGetMatch(matchId);
                matchJsonStr = PrettyPrintJson(matchJsonStr);
                File.WriteAllText(mmOutputFilePath, matchJsonStr);
                metadataAlreadyDownloaded = false;
            }
            else
            {
                DownloadProgressEvent?.Invoke(this, new DownloadProgressEventArgs
                {
                    Value = i,
                    Max = matchesCount,
                    Msg = $"Loading cached metadata for match {i + 1}/{matchesCount}"
                });
                matchJsonStr = File.ReadAllText(mmOutputFilePath);
                metadataAlreadyDownloaded = true;
            }
            NormalizedMatch normedMatch = NormalizeMatch(matchId, matchJsonStr);
            normedMatch.JsonStr = matchJsonStr;
            normedMatch.MetadataAlreadyDownloaded = metadataAlreadyDownloaded;
            normedMatch.TelemetryAlreadyDownloaded = File.Exists(mtOutputFilePath);
            return normedMatch;
        }

        public static NormalizedMatch NormalizeMatch(string matchId, string matchJsonStr)
        {
            NormalizedMatch normedMatch = new NormalizedMatch { Id = matchId };
            MatchModel model = JsonConvert.DeserializeObject<MatchModel>(matchJsonStr);
            List<string> rosterIds = model.Data.Relationships.Rosters.Data.Select(x => x.Id).ToList();
            foreach (string rosterId in rosterIds)
            {
                MatchModelIncluded modelIncludedRoster = model.Included.First(x => x.Id == rosterId);
                List<string> participantIds = modelIncludedRoster.Relationships.Participants.Data.Select(x => x.Id).ToList();
                NormalizedRoster roster = new NormalizedRoster { Roster = modelIncludedRoster };
                normedMatch.Rosters.Add(roster);
                foreach (string participantId in participantIds)
                {
                    MatchModelIncluded participant = model.Included.First(x => x.Id == participantId);
                    roster.Players.Add(participant);
                }
            }
            normedMatch.Model = model;
            normedMatch.JsonStr = matchJsonStr;
            return normedMatch;
        }

        public void DownloadOnlyTelemetryForMatch(NormalizedMatch normalizedMatch, long counter, int count)
        {
            string matchId = normalizedMatch.Id;
            string mtOutputFileName = "mt-" + matchId + ".json.gz";
            string mtOutputFilePath = Path.Combine(TelemetryApp.App.TelemetryDir, mtOutputFileName);
            if (File.Exists(mtOutputFilePath))
            {
                DownloadProgressEvent?.Invoke(this, new DownloadProgressEventArgs
                {
                    Rewrite = true,
                    Value = counter,
                    Max = count,
                    Msg = $"[{counter}/{count}] Telemetry {matchId} already downloaded. Skip."
                });
                return;
            }
            MatchModelIncluded telemetryAsset = normalizedMatch.Model.Included
                .FirstOrDefault(x => x.Type == "asset" && x.Attributes.Name == "telemetry");
            string url = telemetryAsset?.Attributes?.Url;
            if (url == null)
            {
                return;
            }
            Debug.WriteLine("Telemetry URL: " + url);
            using (Stream result = TelemetryApp.App.HttpClient.GetStreamAsync(url).Result)
            using (var stream = new GZipStream(result, CompressionMode.Decompress))
            {
                Uri uri = new Uri(url);
                DownloadProgressEvent?.Invoke(this, new DownloadProgressEventArgs
                {
                    Rewrite = true,
                    Value = counter,
                    Max = count,
                    Msg = $"[{counter}/{count}] Downloading {uri.AbsolutePath}"
                });
                string pJson = PrettyPrintTelemetryJson(stream, out DateTime matchDateTime);
                WriteStringToGzFile(mtOutputFilePath, pJson, matchDateTime);
            }
        }

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
                    Msg = $"\nChecking for new telemetry data for {name} ..."
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
                    Msg = "\n... telemetry data update complete."
                });
                string[] teleFiles = Directory.GetFiles(app.TelemetryDir, "*.json");
                int nTeleFiles = teleFiles.Length;
                DownloadProgressEvent?.Invoke(this, new DownloadProgressEventArgs
                {
                    Value = i,
                    Max = players.Count,
                    Msg =
                        $"Summary: MatchesOnline: {cApiMatches}, NewMatchesDownloaded: {nDownloaded}, AlreadyDownloaded: {nCacheHits}, TotalStored: {nTeleFiles}\n"
                });
            }
        }

        public void DownloadTelemetryForMatchId(string matchId, int counter, int count, ref int nDownloaded, ref int nCacheHits)
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
                    Msg = $"[{counter}/{count}] Downloading match metadata."
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
                    Msg = $"[{counter}/{count}] Telemetry {matchId} already downloaded. Skip."
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
                    Msg = $"[{counter}/{count}] Downloading {uri.AbsolutePath}"
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

    public class DownloadProgressEventArgs : EventArgs
    {
        public long Value { get; set; }
        public long Max { get; set; }
        public string Msg { get; set; }
        public bool Rewrite { get; set; }
        public bool Complete { set; get; }
    }
}