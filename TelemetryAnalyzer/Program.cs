using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace MyPubgTelemetry.Analyzer
{
    class Program
    {
        private string _appDir;
        const string APPNAME = "MyPubgTelemetry";

        static void Main(string[] args)
        {
            Program program = new Program();
            program.MMain(args);
        }

        public static StreamReader GetUtf8Reader(string path)
        {
            Stream stream = new FileStream(path, FileMode.Open);
            if (path.EndsWith(".gz", StringComparison.InvariantCultureIgnoreCase))
            {
                stream = new GZipStream(stream, CompressionMode.Decompress);
            }
            return new StreamReader(stream, Encoding.UTF8);
        }

        private void MMain(string[] args)
        {
            _appDir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), APPNAME);
            Directory.CreateDirectory(_appDir);
            string teleDir = Path.Combine(_appDir, "telemetry_files");
            HashSet<string> squad = new HashSet<string>();
            squad.Add("Celaven");
            squad.Add("wckd");
            squad.Add("Giles333");
            squad.Add("Solunth");
            int matchCount = 0;
            // map enemy playername to a set of match IDs that they were encountered in
            var enemyEncountersByMatch = new Dictionary<string, HashSet<string>>();
            string[] paths = Directory.GetFileSystemEntries(teleDir, "*.json");
            for(int p = 0; p < paths.Length; p++)
            {
                string path = paths[p];
                //if (++matchCount > 3) break;
                Console.WriteLine("path=" + path);
                using (var jsonReader = new JsonTextReader(GetUtf8Reader(path)))
                {
                    dynamic events = JArray.Load(jsonReader);
                    Console.WriteLine(events.Count);
                    for (int i = 0; i < events.Count; i++)
                    {
                        dynamic @event = events[i];
                        string type = @event["_T"];
                        // only process the types of events we're interested in (damage, DBNOs, kills)
                        if (type != "LogPlayerTakeDamage" && type != "LogPlayerMakeGroggy" && type != "LogPlayerKill")
                            continue;
                        dynamic victim = @event.victim;
                        string victimName = victim.name;
                        // attacker is stored under "killer" in the case of LogPlayerKill, for some reason.
                        dynamic attacker = @event.attacker != null ? @event.attacker : @event.killer;
                        string attackerName;
                        string damageType = @event["damageTypeCategory"];
                        string damageReason = @event["damageReason"];

                        // skip events about taking damage from bleeding out, blue zone, and red zone
                        if (damageType == "Damage_Groggy" || damageType == "Damage_BlueZone" ||
                            damageType == "Damage_Explosion_RedZone")
                            continue;

                        if (attacker == null || attacker.Type == JTokenType.Null)
                        {
                            attackerName = "<" + damageType + ">";
                            // skip events that don't have information about who the attacker is
                            continue;
                        }
                        else
                        {
                            attackerName = attacker.name;
                        }

                        // skip events that don't involve a squad member
                        if (!squad.Contains(victimName) && !squad.Contains(attackerName))
                            continue;

                        // skip self damage: e.g.: your own grenade, bleeding out, crashing your own vehicle
                        if (victimName == attackerName)
                            continue;

                        // skip friendly fire; could also be done more dynamically by teamId
                        if (squad.Contains(attackerName))
                            continue;

                        if (!enemyEncountersByMatch.ContainsKey(attackerName))
                            enemyEncountersByMatch[attackerName] = new HashSet<string>();
                        enemyEncountersByMatch[attackerName].Add(path);

                        float aHP = attacker.health;
                        float vHP = victim.health;

                        Console.WriteLine($"Attacker:{attackerName}({aHP}) Victim:{victimName}({vHP}) EventType:{type} DamageType:{damageType}");
                        //\n" + @event);
                    } // foreach event
                    int count = 0;
                    foreach (var entry in enemyEncountersByMatch.OrderBy(x => -x.Value.Count))
                    {
                        Console.WriteLine(entry.Key + " encountered in " + entry.Value.Count + " matches");
                        if (++count > 10) break;
                    }
                    Console.WriteLine($"============= End Match Report ({p}/{paths.Length}) ================");
                } // foreach match
            }
        }
    }
}
