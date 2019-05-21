using System;
using System.Collections.Generic;

namespace MyPubgTelemetry.MatchMetadataModel
{
    public partial class MatchMetadata
    {
        public MatchData Data { get; set; }
        public List<MatchIncluded> Included { get; set; }
        public MatchDataLinks Links { get; set; }
        public Meta Meta { get; set; }
    }

    public partial class MatchData
    {
        public string Type { get; set; }
        public string Id { get; set; }
        public MatchDataAttributes Attributes { get; set; }
        public MatchDataRelationships Relationships { get; set; }
        public MatchDataLinks Links { get; set; }
    }

    public partial class MatchDataAttributes
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
        public DateTimeOffset CreatedAt { get; set; }
    }

    public partial class MatchDataLinks
    {
        public Uri Self { get; set; }
        public string Schema { get; set; }
    }

    public partial class MatchDataRelationships
    {
        public MatchAssets MatchAssets { get; set; }
        public MatchAssets Rosters { get; set; }
    }

    public partial class MatchAssets
    {
        public List<MatchDatum> Data { get; set; }
    }

    public partial class MatchDatum
    {
        public string Type { get; set; }
        public string Id { get; set; }
    }

    public partial class MatchIncluded
    {
        public string Type { get; set; }
        public string Id { get; set; }
        public IncludedAttributes Attributes { get; set; }
        public IncludedRelationships Relationships { get; set; }
    }

    public partial class IncludedAttributes
    {
        public Stats Stats { get; set; }
        public string Actor { get; set; }
        public string ShardId { get; set; }
        public bool? Won { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public DateTimeOffset? CreatedAt { get; set; }
        public Uri Url { get; set; }
    }

    public partial class Stats
    {
        public long? DbnOs { get; set; }
        public long? Assists { get; set; }
        public long? Boosts { get; set; }
        public double? DamageDealt { get; set; }
        public string DeathType { get; set; }
        public long? HeadshotKills { get; set; }
        public long? Heals { get; set; }
        public long? KillPlace { get; set; }
        public long? KillPoints { get; set; }
        public long? KillPointsDelta { get; set; }
        public long? KillStreaks { get; set; }
        public long? Kills { get; set; }
        public long? LastKillPoints { get; set; }
        public long? LastWinPoints { get; set; }
        public double? LongestKill { get; set; }
        public long? MostDamage { get; set; }
        public string Name { get; set; }
        public string PlayerId { get; set; }
        public long? RankPoints { get; set; }
        public long? Revives { get; set; }
        public double? RideDistance { get; set; }
        public long? RoadKills { get; set; }
        public double? SwimDistance { get; set; }
        public long? TeamKills { get; set; }
        public double? TimeSurvived { get; set; }
        public long? VehicleDestroys { get; set; }
        public double? WalkDistance { get; set; }
        public long? WeaponsAcquired { get; set; }
        public long? WinPlace { get; set; }
        public long? WinPoints { get; set; }
        public long? WinPointsDelta { get; set; }
        public long? Rank { get; set; }
        public long? TeamId { get; set; }
    }

    public partial class IncludedRelationships
    {
        public MatchAssets Team { get; set; }
        public MatchAssets Participants { get; set; }
    }

    public partial class Meta
    {
    }
}
