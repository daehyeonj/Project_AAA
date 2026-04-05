using System;

public sealed class PrototypeRpgEncounterPreview
{
    public string DungeonId = string.Empty;
    public string EncounterId = string.Empty;
    public string DisplayName = string.Empty;
    public string EncounterTypeLabel = string.Empty;
    public string RoomTypeLabel = string.Empty;
    public int EnemyCount;
    public bool HasElitePresence;
    public string EnemyTypeSummary = string.Empty;
    public string ThreatLabel = string.Empty;
    public string RecommendedPowerHint = string.Empty;
    public string IntentPreviewText = string.Empty;
    public string SummaryText = string.Empty;
}

public sealed class PrototypeRpgRouteEncounterPreview
{
    public string DungeonId = string.Empty;
    public string RouteId = string.Empty;
    public string RouteLabel = string.Empty;
    public string RouteDescriptionText = string.Empty;
    public string RouteRiskLabel = string.Empty;
    public string RoomPathSummaryText = string.Empty;
    public int EncounterCount;
    public int EliteEncounterCount;
    public int TotalEnemyCount;
    public string EnemyTypeSummary = string.Empty;
    public string PatternSummaryText = string.Empty;
    public string ThreatLabel = string.Empty;
    public string RecommendedPowerText = string.Empty;
    public string EventFocusText = string.Empty;
    public string RewardFocusText = string.Empty;
    public string EncounterSummaryText = string.Empty;
    public string SummaryText = string.Empty;
}

public sealed class PrototypeRpgRouteRewardPreview
{
    public string DungeonId = string.Empty;
    public string RouteId = string.Empty;
    public string RouteLabel = string.Empty;
    public string RewardLabel = string.Empty;
    public string RewardResourceId = string.Empty;
    public int BattleRewardAmountHint;
    public int ChestRewardAmountHint;
    public int EventRewardAmountHint;
    public int EliteRewardAmountHint;
    public int PendingBonusRewardHint;
    public int TotalRewardAmountHint;
    public string RewardBreakdownText = string.Empty;
    public string SummaryText = string.Empty;
}

public sealed class PrototypeRpgRouteEventPreview
{
    public string DungeonId = string.Empty;
    public string RouteId = string.Empty;
    public string RouteLabel = string.Empty;
    public int RecoverAmountHint;
    public int BonusRewardAmountHint;
    public string BonusRewardResourceId = string.Empty;
    public string EventFocusText = string.Empty;
    public string SummaryText = string.Empty;
}

public sealed class PrototypeRpgThreatProfile
{
    public string DungeonId = string.Empty;
    public string RouteId = string.Empty;
    public string EncounterId = string.Empty;
    public string DangerLabel = string.Empty;
    public string ThreatTierLabel = string.Empty;
    public int RecommendedPowerValue;
    public string RecommendedPowerBand = string.Empty;
    public string EnemyPatternSummary = string.Empty;
    public string SustainRiskHint = string.Empty;
    public string SummaryText = string.Empty;
}

public sealed class PrototypeRpgRecommendedPowerPreview
{
    public string DungeonId = string.Empty;
    public string DangerLabel = string.Empty;
    public string ThreatTierLabel = string.Empty;
    public int RecommendedPowerValue;
    public string RecommendedPowerBand = string.Empty;
    public string RecommendedPowerText = string.Empty;
    public string EnemyPatternSummary = string.Empty;
    public string SustainRiskHint = string.Empty;
    public string SummaryText = string.Empty;
}

public sealed class PrototypeRpgDungeonSelectionPreviewData
{
    public string DungeonId = string.Empty;
    public string DungeonLabel = string.Empty;
    public string DangerLabel = string.Empty;
    public bool HasAppliedPartyProgress;
    public string AppliedPartySummaryText = string.Empty;
    public string AppliedLastRunIdentity = string.Empty;
    public string AppliedLastResultStateKey = string.Empty;
    public string SelectedLastRunSummaryText = string.Empty;
    public PrototypeRpgEncounterPreview EncounterPreview = new PrototypeRpgEncounterPreview();
    public PrototypeRpgRouteEncounterPreview SafeRoutePreview = new PrototypeRpgRouteEncounterPreview();
    public PrototypeRpgRouteEncounterPreview RiskyRoutePreview = new PrototypeRpgRouteEncounterPreview();
    public PrototypeRpgRouteRewardPreview SafeRewardPreview = new PrototypeRpgRouteRewardPreview();
    public PrototypeRpgRouteRewardPreview RiskyRewardPreview = new PrototypeRpgRouteRewardPreview();
    public PrototypeRpgRouteEventPreview SafeEventPreview = new PrototypeRpgRouteEventPreview();
    public PrototypeRpgRouteEventPreview RiskyEventPreview = new PrototypeRpgRouteEventPreview();
    public PrototypeRpgRecommendedPowerPreview RecommendedPowerPreview = new PrototypeRpgRecommendedPowerPreview();
}