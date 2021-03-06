﻿using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using MyPubgTelemetry.ApiMatchModel;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace MyPubgTelemetry
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
                var parallelOptions = new ParallelOptions() { MaxDegreeOfParallelism = 2 };
                int parallelCounter = 0;
                Parallel.ForEach(matchIds, parallelOptions, (matchId, state, i) =>
                {
                    // ReSharper disable once AccessToModifiedClosure -- Interlocked.Increment
                    NormalizedMatch nm = DownloadOnlyMatchMetadataForMatchId(matchId, ref parallelCounter, matchIds.Count);
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
                parallelCounter = 0;
                DownloadProgressEvent?.Invoke(this, new DownloadProgressEventArgs
                {
                    Value = 0,
                    Max = nmsToDownload.Count,
                    Msg = $"Starting download of {nmsToDownload.Count} match telemetry file(s)."
                });
                Parallel.ForEach(nmsToDownload, parallelOptions, (normalizedMatch, state, i) =>
                {
                    DownloadOnlyTelemetryForMatch(normalizedMatch, ref parallelCounter, nmsToDownload.Count);
                });
                return nms.ToList();
            });
        }

        public NormalizedMatch DownloadOnlyMatchMetadataForMatchId(string matchId, ref int counter, int matchesCount)
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
                    Value = Interlocked.Increment(ref counter),
                    Max = matchesCount,
                    Msg = $"[{counter}/{matchesCount}] Downloading metadata for match: {matchId}"
                });
                matchJsonStr = TelemetryApp.App.ApiGetMatch(matchId);
                matchJsonStr = PrettyPrintJson(matchJsonStr);
                TelemetryApp.FileWriteAllTextAtomic(mmOutputFilePath, matchJsonStr);
                metadataAlreadyDownloaded = false;
            }
            else
            {
                DownloadProgressEvent?.Invoke(this, new DownloadProgressEventArgs
                {
                    Value = Interlocked.Increment(ref counter),
                    Max = matchesCount,
                    Msg = $"[{counter}/{matchesCount}] Loading cached metadata for match: {matchId}"
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

        public void DownloadOnlyTelemetryForMatch(NormalizedMatch normalizedMatch, ref int counter, int count)
        {
            string matchId = normalizedMatch.Id;
            string mtOutputFileName = "mt-" + matchId + ".json.gz";
            string mtOutputFilePath = Path.Combine(TelemetryApp.App.TelemetryDir, mtOutputFileName);
            if (File.Exists(mtOutputFilePath))
            {
                DownloadProgressEvent?.Invoke(this, new DownloadProgressEventArgs
                {
                    Rewrite = true,
                    Value = Interlocked.Increment(ref counter),
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
                string pJson = PrettyPrintTelemetryJson(stream, out DateTime matchDateTime);
                DownloadProgressEvent?.Invoke(this, new DownloadProgressEventArgs
                {
                    Rewrite = true,
                    Value = Interlocked.Increment(ref counter),
                    Max = count,
                    Msg = $"[{counter}/{count}] Downloaded {uri.AbsolutePath}"
                });
                WriteStringToGzFile(mtOutputFilePath, pJson, matchDateTime);
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