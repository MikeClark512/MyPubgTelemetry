using System;
using System.Data;
using System.IO;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace MyPubgTelemetry.MatchAnalyzer
{
    class Program
    {
        static void Main(string[] args)
        {
            TelemetryApp app = TelemetryApp.App;
            string jsonStr = File.ReadAllText(@"C:\Users\mclark\Desktop\player.json");
            JToken json = JToken.Parse(jsonStr);
            var data = json["data"];
            var dt = data.ToObject<DataTable>();
            
            //DataTable dt = JsonConvert.DeserializeObject<DataTable>(json);
            Console.WriteLine(dt);
        }
    }
}
