using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace MyPubgTelemetry.Downloader
{
    class TelemetryDownloader
    {
        public TelemetryApp App { get; set; }

        public TelemetryDownloader()
        {
            App = new TelemetryApp();
        }

        public List<JToken> ApiGetPlayersByNames(string players)
        {
            String url = "players?filter[playerNames]=" + players;
            string result = App.HttpClient.GetStringAsync(url).Result;
            JObject playersObject = JObject.Parse(result);
            return playersObject["data"].ToList();
        }
    }
}
