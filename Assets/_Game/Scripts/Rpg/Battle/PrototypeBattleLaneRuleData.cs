public static class PrototypeBattleLaneKeys
{
    public const string None = "none";
    public const string Top = "top";
    public const string Mid = "mid";
    public const string Bottom = "bottom";
    public const string Any = "any";
}

public static class PrototypeBattleRangeKeys
{
    public const string None = "none";
    public const string MeleeSameLane = "melee_same_lane";
    public const string MeleeSameOrAdjacent = "melee_same_or_adjacent";
    public const string SnipeAnyLane = "snipe_any_lane";
    public const string PierceLine = "pierce_line";
    public const string AllEnemyLanes = "all_enemy_lanes";
    public const string AllAllyLanes = "all_ally_lanes";
    public const string PartyWide = "party_wide";
    public const string LaneAgnostic = "lane_agnostic";
}

public static class PrototypeBattleLaneRuleKeys
{
    public const string None = "none";
    public const string SameLaneOnly = "same_lane_only";
    public const string SameOrAdjacentEnemyLane = "same_or_adjacent_enemy_lane";
    public const string AnyEnemyLane = "any_enemy_lane";
    public const string BacklineSnipe = "backline_snipe";
    public const string GuardIntercept = "guard_intercept";
    public const string PierceLine = "pierce_line";
    public const string AllEnemyLanes = "all_enemy_lanes";
    public const string AllAllyLanes = "all_ally_lanes";
    public const string PartyWide = "party_wide";
    public const string LaneAgnostic = "lane_agnostic";
}

public sealed class PrototypeBattleLaneRuleResolution
{
    public string ActorLaneKey = PrototypeBattleLaneKeys.None;
    public string ActorLaneLabel = string.Empty;
    public string TargetLaneKey = PrototypeBattleLaneKeys.None;
    public string TargetLaneLabel = string.Empty;
    public string ThreatLaneKey = PrototypeBattleLaneKeys.None;
    public string ResolvedRangeKey = PrototypeBattleRangeKeys.None;
    public string ResolvedLaneRuleKey = PrototypeBattleLaneRuleKeys.None;
    public string LaneImpactKey = string.Empty;
    public string ReachabilityStateKey = "unknown";
    public string RangeText = string.Empty;
    public string LaneImpactText = string.Empty;
    public string ReachabilitySummaryText = string.Empty;
    public string PredictedReachabilityText = string.Empty;
    public string PositionRuleText = string.Empty;
    public string TargetRuleText = string.Empty;
    public string ThreatLaneLabel = string.Empty;
    public string ThreatSummaryText = string.Empty;
}
