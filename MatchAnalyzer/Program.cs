using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Driver;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MongoDB.Bson.IO;

namespace MyPubgTelemetry.MatchAnalyzer
{
    internal static class Program
    {
        public static string FileReadAllTextGz(string path)
        {
            using (var fs = new FileStream(path, FileMode.Open, FileAccess.Read))
            {
                GZipStream gz = new GZipStream(fs, CompressionMode.Decompress);
                StreamReader sr = new StreamReader(gz);
                return sr.ReadToEnd();
            }
        }

        public static StreamReader NewGzReader(string path)
        {
            var fs = new FileStream(path, FileMode.Open, FileAccess.Read);
            var gz = new GZipStream(fs, CompressionMode.Decompress);
            return new StreamReader(gz);
        }

        public static string Fmt(this Stopwatch sw)
        {
            TimeSpan e = sw.Elapsed;
            return $"{e.TotalMinutes:00}m{e.Seconds}s{e.Milliseconds}ms";
        }

        static void Main(string[] args)
        {
            ThreadPool.GetMaxThreads(out int workerThreads, out int completionPortThreads);
            Console.WriteLine($"workerThreads={workerThreads} completionPortThreads={completionPortThreads}");

            MongoClient client = new MongoClient("mongodb://localhost");
            IMongoDatabase db = client.GetDatabase("telemetry");
            var parallelOptions = new ParallelOptions { MaxDegreeOfParallelism = Math.Max(1, Environment.ProcessorCount) };

            TelemetryApp app = TelemetryApp.App;

            {
                Stopwatch swd = Stopwatch.StartNew();
                List<string> colNames = db.ListCollectionNames().ToList();
                int dropCount = 0;
                Parallel.ForEach(colNames, parallelOptions, (name, state, i) =>
                {
                    db.DropCollection(name);
                    int count = Interlocked.Increment(ref dropCount);
                    Console.WriteLine($"Finished dropping col {count}/{colNames.Count} in {swd.Fmt()}");
                });
                Console.WriteLine($"Dropped {colNames.Count} collections in {swd.Fmt()}");
            }

            string[] files = Directory.GetFiles(app.TelemetryDir, "*.json.gz");

            Stopwatch sw = Stopwatch.StartNew();
            int counter = 0;
            var collection = db.GetCollection<BsonDocument>("telemetry");
            var indexBuilder = new IndexKeysDefinitionBuilder<BsonDocument>();
            var caseInsens = new CreateIndexOptions
            {
                Collation = new Collation("en", true, CollationCaseFirst.Upper, CollationStrength.Secondary, true)
            };
            var createIndexModels = new[]
            {
                new CreateIndexModel<BsonDocument>(indexBuilder.Ascending("_D")),
                new CreateIndexModel<BsonDocument>(indexBuilder.Hashed("_T")),
                new CreateIndexModel<BsonDocument>(indexBuilder.Hashed("_M")),
                new CreateIndexModel<BsonDocument>(indexBuilder.Ascending("character.name"), caseInsens),
                new CreateIndexModel<BsonDocument>(indexBuilder.Ascending("attacker.name"), caseInsens),
                new CreateIndexModel<BsonDocument>(indexBuilder.Ascending("victim.name"), caseInsens),
                new CreateIndexModel<BsonDocument>(indexBuilder.Ascending("killer.name"), caseInsens),
            };
            collection.Indexes.CreateMany(createIndexModels);
            BlockingCollection<Task> tasks = new BlockingCollection<Task>();
            Parallel.ForEach(files, parallelOptions, (file, state, i) =>
            {
                string matchId = TelemetryApp.TelemetryFilenameToMatchId(file);
                var mid = new BsonElement("_M", matchId);
                using (var s = NewGzReader(file))
                {
                    var ary = BsonSerializer.Deserialize<BsonArray>(s);
                    JsonReader jsr = new JsonReader(s);
                    IEnumerable<BsonDocument> docs = ary.Select(ele =>
                    {
                        BsonDocument doc = ele.Clone().AsBsonDocument;
                        doc["_D"] = DateTime.Parse(doc["_D"].AsString, null, DateTimeStyles.RoundtripKind);
                        return doc.Clone().AsBsonDocument.Add(mid);
                    });
                    int lcounter = Interlocked.Increment(ref counter);
                    collection.InsertMany(docs);
                    //Task task = collection.InsertManyAsync(docs);
                    //tasks.Add(task);
                    //task.GetAwaiter().OnCompleted(() =>
                    //{
                        Console.WriteLine($"Finished inserting doc {lcounter}/{files.Length} -- {matchId} -- in {sw.Fmt()}");
                    //});
                    //Console.WriteLine($"Finished parsing doc {lcounter}/{files.Length} -- {matchId} -- in {sw.Fmt()}");
                }
            });
            Task.WaitAll(tasks.ToArray());
            Console.WriteLine($"Done in {sw.Fmt()}");
        }

        //static void Main(string[] args)
        //{
        //    MongoClient dbClient = new MongoClient("mongodb://localhost");
        //    IMongoDatabase db = dbClient.GetDatabase("hi");
        //    IMongoCollection<BsonDocument> col = db.GetCollection<BsonDocument>("players");
        //    FilterDefinition<BsonDocument> query = Builders<BsonDocument>.Filter.Empty;
        //    // FilterDefinition<BsonDocument> query = Builders<BsonDocument>.Filter.Gt("data.attributes.duration", 1962);

        //    PipelineDefinition<BsonDocument, BsonDocument> pipeline = new[]
        //    {
        //        new BsonDocument("$unwind", "$data"),
        //        new BsonDocument("$project", 
        //                new BsonDocument("x", "$data")
        //            ),
        //        new BsonDocument("$unwind", "$x.relationships.matches.data"),
        //    };
        //    //col.Aggregate().Unwind("$data").Project(new BsonDocument("x", $"data"));
        //    //var ba = new PipelineStagePipelineDefinition<BsonDocument,BsonDocument>();
        //    //{
        //    //    new BsonDocument("$unwind", "$data"),
        //    //    new BsonDocument("$project",
        //    //        new BsonDocument("x", "$data")),
        //    //    new BsonDocument("$unwind", "$x.relationships.matches.data")
        //    //};
        //    IAggregateFluent<BsonDocument> agg = col.Aggregate()
        //        .Unwind("data")
        //        .Project(new BsonDocument {{"x", "$data"}, {"_id", 0}})
        //        .Unwind("x.relationships.matches.data");
        //    foreach (BsonDocument result in agg.ToEnumerable())
        //    {
        //        Dictionary<string, object> flat = JsonHelper.DeserializeAndFlatten(result.ToString());
        //        Console.WriteLine("========================================");
        //        foreach (KeyValuePair<string, object> kvp in flat)
        //        {
        //            string key = kvp.Key;
        //            string value = kvp.Value != null ? kvp.Value.ToString() : "null";
        //            Console.WriteLine(key + "=" + value);
        //        }
        //        Console.WriteLine("========================================");
        //    }

        //    /*
        //    TelemetryApp app = TelemetryApp.App;
        //    string jsonStr = File.ReadAllText(@"C:\Users\mclark\Desktop\current\player.json");
        //    JToken json = JToken.Parse(jsonStr);
        //    List<string> matchIds = json.SelectTokens("data[0].relationships.matches.data[*].id").Select(x => x.Value<string>()).ToList();
        //    foreach (string matchId in matchIds)
        //    {
        //        Console.WriteLine(matchId);
        //        string path = $@"C:\Users\mclark\AppData\Roaming\MyPubgTelemetry\match_files\mm-{matchId}.json";
        //        string mmJsonStr = File.ReadAllText(path);
        //        var doc = BsonSerializer.Deserialize<BsonDocument>(mmJsonStr);
        //        col.InsertOne(doc);
        //    }
        //    */
        //}
        /*
        static void Main(string[] args)
        {
            TelemetryApp app = TelemetryApp.App;
            string jsonStr = File.ReadAllText(@"C:\Users\mclark\Desktop\current\player.json");
            JToken json = JToken.Parse(jsonStr);
            List<string> matchIds = json.SelectTokens("data[0].relationships.matches.data[*].id").Select(x => x.Value<string>()).ToList();
            foreach (string matchId in matchIds)
            {
                Console.WriteLine(matchId);
                string path = $@"C:\Users\mclark\AppData\Roaming\MyPubgTelemetry\match_files\mm-{matchId}.json";

                JToken mmJson = JToken.Parse(jsonStr);
                //mmJson.SelectTokens("")

                string mmJsonStr = File.ReadAllText(path);
                Dictionary<string, object> flat = JsonHelper.DeserializeAndFlatten(mmJsonStr);
                foreach (KeyValuePair<string, object> kvp in flat)
                {
                    string key = kvp.Key;
                    string value = kvp.Value != null ? kvp.Value.ToString() : "null";
                    Console.WriteLine(key + "=" + value);
                }

                //JToken mmJson = JToken.Parse(jsonStr);
                //foreach (var d in mmJson)
                //    if (d is JProperty jp)
                //        Console.WriteLine(jp.Name);
                break;
            }
        }
        */

        public class JsonHelper
        {
            public static Dictionary<string, object> DeserializeAndFlatten(string json)
            {
                Dictionary<string, object> dict = new Dictionary<string, object>();
                JToken token = JToken.Parse(json);
                FillDictionaryFromJToken(dict, token, "");
                return dict;
            }

            private static void FillDictionaryFromJToken(Dictionary<string, object> dict, JToken token, string prefix)
            {
                switch (token.Type)
                {
                    case JTokenType.Object:
                        foreach (JProperty prop in token.Children<JProperty>())
                        {
                            FillDictionaryFromJToken(dict, prop.Value, JoinObject(prefix, prop.Name));
                        }

                        break;
                    case JTokenType.Array:
                        int index = 0;
                        foreach (JToken value in token.Children())
                        {
                            FillDictionaryFromJToken(dict, value, JoinArray(prefix, index.ToString()));
                            index++;
                        }

                        break;
                    default:
                        dict.Add(prefix, ((JValue)token).Value);
                        break;
                }
            }

            private static string JoinObject(string prefix, string name)
            {
                return string.IsNullOrEmpty(prefix) ? name : prefix + "." + name;
            }

            private static string JoinArray(string prefix, string name)
            {
                return string.IsNullOrEmpty(prefix) ? name : prefix + "[" + name + "]";
            }
        }
    }
}