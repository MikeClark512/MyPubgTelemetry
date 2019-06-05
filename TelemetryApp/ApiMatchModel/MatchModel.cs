using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;

namespace MyPubgTelemetry.ApiMatchModel
{
    public class MatchModel
    {
        public MatchModelData Data { get; set; }
        public List<MatchModelIncluded> Included { get; set; }
        public MatchModelLinks Links { get; set; }
        public MatchModelMeta Meta { get; set; }
    }

    public class MatchModelData
    {
        public string Type { get; set; }
        public string Id { get; set; }
        public MatchModelAttributes Attributes { get; set; }
        public MatchModelRelationships Relationships { get; set; }
        public MatchModelLinks Links { get; set; }
    }

    public class MatchModelAttributes
    {
        public string ShardId { get; set; }
        public object Tags { get; set; }
        public string MapName { get; set; }
        public string GameMode { get; set; }
        public long Duration { get; set; }
        public object Stats { get; set; }
        public string TitleId { get; set; }
        public bool IsCustomMatch { get; set; }
        public string SeasonState { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    public class MatchModelLinks
    {
        public Uri Self { get; set; }
        public string Schema { get; set; }
    }

    public class MatchModelRelationships
    {
        public MatchModelAssets MatchAssets { get; set; }
        public MatchModelAssets Rosters { get; set; }
    }

    public class MatchModelAssets
    {
        public List<MatchModelDatum> Data { get; set; }
    }

    public class MatchModelDatum
    {
        public string Type { get; set; }
        public string Id { get; set; }
    }

    public class MatchModelIncluded
    {
        public string Type { get; set; }
        public string Id { get; set; }
        public MatchModelIncludedAttributes Attributes { get; set; }
        public MatchModelIncludedRelationships Relationships { get; set; }
    }

    public class MatchModelIncludedAttributes
    {
        public MatchModelStats Stats { get; set; }
        public string Actor { get; set; }
        public string ShardId { get; set; }
        public bool? Won { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public DateTime? CreatedAt { get; set; }
        public string Url { get; set; }
    }

    public class MatchModelStats
    {

        // ReSharper disable once InconsistentNaming
        public long Rank { get; set; }
        public long Kills { get; set; }
        public double DamageDealt { get; set; }
        public long Revives { get; set; }
        public long DBNOs { get; set; }
        public long RoadKills { get; set; }
        public long VehicleDestroys { get; set; }
        public long HeadshotKills { get; set; }
        public long XFragKills { get; set; }
        public long KillStreaks { get; set; }
        public long Assists { get; set; }
        public long Heals { get; set; }
        public long Boosts { get; set; }
        public double TimeSurvived { get; set; }
        public double RideDistance { get; set; }
        public double WalkDistance { get; set; }
        public double SwimDistance { get; set; }
        public double LongestKill { get; set; }
        public long TeamKills { get; set; }
        public long WeaponsAcquired { get; set; }
        public long KillPlace { get; set; }
        public long KillPoints { get; set; }
        public string DeathType { get; set; }
        public long KillPointsDelta { get; set; }
        public long LastKillPoints { get; set; }
        public long LastWinPoints { get; set; }
        public long MostDamage { get; set; }
        public string Name { get; set; }
        public string PlayerId { get; set; }
        public long RankPoints { get; set; }
        public long WinPlace { get; set; }
        public long WinPoints { get; set; }
        public long WinPointsDelta { get; set; }
        public long TeamId { get; set; }


        //[JsonExtensionData]
        //public Dictionary<string, JToken> OtherStats { get; set; } = new Dictionary<string, JToken>();
    }

    public class MatchModelIncludedRelationships
    {
        public MatchModelAssets Team { get; set; }
        public MatchModelAssets Participants { get; set; }
    }

    public class MatchModelMeta
    {
    }
}
