using System;

public enum WorldDecisionSignalKind
{
    CityShortage,
    CitySurplus,
    RoadCapacity,
    ActiveExpedition,
    RecentResult,
    DungeonOutput
}

public sealed class WorldSelectionReadModel
{
    public bool HasSelection;
    public string EntityId = string.Empty;
    public string DisplayName = "None";
    public WorldEntityKind Kind = WorldEntityKind.City;
    public string LinkedCityId = string.Empty;
    public string LinkedDungeonId = string.Empty;
}

public sealed class ResourceAmountReadModel
{
    public string ResourceId = string.Empty;
    public int Amount;
    public int StockAmount;
    public bool IsHighPriority;
}

public sealed class TradeOpportunityReadModel
{
    public string SupplierEntityId = string.Empty;
    public string SupplierDisplayName = "None";
    public string ConsumerEntityId = string.Empty;
    public string ConsumerDisplayName = "None";
    public string ResourceId = string.Empty;
    public string RouteId = string.Empty;
}

public sealed class WorldUnmetNeedReadModel
{
    public string CityId = string.Empty;
    public string CityDisplayName = "None";
    public string ResourceId = string.Empty;
}

public sealed class DungeonOutputReadModel
{
    public string DungeonId = string.Empty;
    public string DungeonDisplayName = "None";
    public string LinkedCityId = string.Empty;
    public string LinkedCityDisplayName = "None";
    public string ResourceId = string.Empty;
}

public sealed class WorldDecisionSignalReadModel
{
    public WorldDecisionSignalKind Kind = WorldDecisionSignalKind.CityShortage;
    public string EntityId = string.Empty;
    public string EntityDisplayName = "None";
    public string RelatedEntityId = string.Empty;
    public string RelatedEntityDisplayName = "None";
    public string ResourceId = string.Empty;
    public string ResultStateKey = string.Empty;
    public int Magnitude;
    public int Priority;
    public bool IsBlocking;
}

public sealed class ExpeditionResultReadModel
{
    public bool HasResult;
    public string SourceCityId = string.Empty;
    public string SourceCityDisplayName = "None";
    public string TargetDungeonId = string.Empty;
    public string TargetDungeonDisplayName = "None";
    public string RewardResourceId = string.Empty;
    public string ResultStateKey = string.Empty;
    public bool Success;
    public int ReturnedLootAmount;
    public int TotalTurnsTaken;
    public int ClearedEncounterCount;
    public int OpenedChestCount;
    public int SurvivingMemberCount;
    public int KnockedOutMemberCount;
    public bool EliteDefeated;
    public string SummaryText = "None";
    public string LootSummaryText = "None";
    public string SurvivingMembersSummaryText = "None";
    public string ClearedEncountersSummaryText = "None";
    public string EventChoiceSummaryText = "None";
    public string LootBreakdownSummaryText = "None";
    public string RouteSummaryText = "None";
    public string DungeonSummaryText = "None";
    public string MissionObjectiveText = "None";
    public string MissionRelevanceText = "None";
    public string RiskRewardContextText = "None";
    public string RunPathSummaryText = "None";
    public string OutcomeMeaningId = string.Empty;
    public string OutcomeRewardMeaningText = "None";
    public string CityImpactMeaningText = "None";
    public string RecommendationShiftText = "None";
    public string CityStatusChangeSummaryText = "None";
    public string WorldReturnSummaryText = "None";
    public string ExpeditionLogEntryText = "None";
    public string PartyConditionText = "None";
    public string PartyHpSummaryText = "None";
    public string EliteSummaryText = "None";
}

public sealed class ExpeditionStatusReadModel
{
    public string PartyId = string.Empty;
    public string HomeCityId = string.Empty;
    public string HomeCityDisplayName = "None";
    public string TargetDungeonId = string.Empty;
    public string TargetDungeonDisplayName = "None";
    public int DaysRemaining;
    public int Power;
    public int CarryCapacity;
    public string StatusText = "None";
}

public sealed class CityStatusReadModel
{
    public string CityId = string.Empty;
    public string DisplayName = "None";
    public int PopulationValue;
    public string[] RelatedResourceIds = Array.Empty<string>();
    public string[] SupplyResourceIds = Array.Empty<string>();
    public string[] NeedResourceIds = Array.Empty<string>();
    public string[] HighPriorityNeedResourceIds = Array.Empty<string>();
    public string[] OutputResourceIds = Array.Empty<string>();
    public ResourceAmountReadModel[] KeyStocks = Array.Empty<ResourceAmountReadModel>();
    public ResourceAmountReadModel[] TopShortages = Array.Empty<ResourceAmountReadModel>();
    public ResourceAmountReadModel[] TopSurpluses = Array.Empty<ResourceAmountReadModel>();
    public int LastDayProduced;
    public int LastDayImported;
    public int LastDayExported;
    public int LastDayProcessedTotal;
    public int LastDayConsumed;
    public int LastDayShortages;
    public int LastDayUnmet;
    public int LastDayCriticalUnmet;
    public int LastDayProcessingBlocked;
    public int TotalShortages;
    public int TotalUnmet;
    public int TotalCriticalUnmet;
    public int PartyCount;
    public int IdlePartyCount;
    public int ActiveExpeditionCount;
    public int ExpeditionLootReturnedTotal;
    public string LinkedDungeonId = string.Empty;
    public string LinkedDungeonDisplayName = "None";
    public int AvailableContractSlots;
    public int MaxActiveExpeditionSlots;
    public string NeedPressureStateId = "None";
    public string DispatchReadinessStateId = "None";
    public string DispatchPolicyStateId = "None";
    public int DispatchRecoveryDaysRemaining;
    public int ConsecutiveDispatchCount;
    public string RecommendedRouteId = string.Empty;
    public string RecommendedRouteSummaryText = "None";
    public string RecommendationReasonText = "None";
    public string LastDispatchImpactText = "None";
    public string LastNeedPressureChangeText = "None";
    public string LastDispatchReadinessChangeText = "None";
    public ExpeditionStatusReadModel ActiveExpedition = null;
    public ExpeditionResultReadModel LatestResult = new ExpeditionResultReadModel();
    public WorldDecisionSignalReadModel[] ActionSignals = Array.Empty<WorldDecisionSignalReadModel>();
    public CityDecisionReadModel Decision = new CityDecisionReadModel();
}

public sealed class DungeonStatusReadModel
{
    public string DungeonId = string.Empty;
    public string DisplayName = "None";
    public string LinkedCityId = string.Empty;
    public string LinkedCityDisplayName = "None";
    public int DangerLevel;
    public string[] OutputResourceIds = Array.Empty<string>();
    public int RecommendedPower;
    public int ExpeditionDurationDays;
    public int ActiveExpeditionCount;
    public int AvailableContractSlots;
    public int MaxActiveExpeditionSlots;
    public ExpeditionStatusReadModel ActiveExpedition = null;
    public ExpeditionResultReadModel LatestResult = new ExpeditionResultReadModel();
}

public sealed class RoadStatusReadModel
{
    public string RoadId = string.Empty;
    public string[] Tags = Array.Empty<string>();
    public string FromEntityId = string.Empty;
    public string FromEntityDisplayName = "None";
    public string ToEntityId = string.Empty;
    public string ToEntityDisplayName = "None";
    public int CapacityPerDay;
    public int LastDayUsage;
    public int AvailableCapacity;
    public int UtilizationPercent;
    public bool IsSaturated;
    public int TradeOpportunityCount;
}

public sealed class WorldBoardReadModel
{
    public static readonly WorldBoardReadModel Empty = new WorldBoardReadModel();

    public int WorldEntityCount;
    public int WorldRouteCount;
    public int VisibleCityCount;
    public int VisibleDungeonCount;
    public int VisibleRoadCount;
    public int TradeOpportunityCount;
    public int UnmetCityNeedCount;
    public int UnclaimedDungeonOutputCount;
    public bool AutoTickEnabled;
    public bool AutoTickPaused;
    public int AutoTickCount;
    public int WorldDayCount;
    public int TradeStepCount;
    public int LastDayProducedTotal;
    public int LastDayClaimedDungeonOutputsTotal;
    public int LastDayTradedTotal;
    public int LastDayProcessedTotal;
    public int LastDayConsumedTotal;
    public int LastDayCriticalFulfilledTotal;
    public int LastDayCriticalUnmetTotal;
    public int LastDayNormalFulfilledTotal;
    public int LastDayNormalUnmetTotal;
    public int LastDayFulfilledTotal;
    public int LastDayUnmetTotal;
    public int LastDayShortagesTotal;
    public int LastDayProcessingBlockedTotal;
    public int LastDayProcessingReservedTotal;
    public int LastDayRouteCapacityUsedTotal;
    public int TotalRouteCapacityPerDay;
    public int TotalParties;
    public int IdleParties;
    public int ActiveExpeditions;
    public int AvailableContracts;
    public int ExpeditionSuccessCount;
    public int ExpeditionFailureCount;
    public int ExpeditionLootReturnedTotal;
    public int CurrentUnclaimedDungeonOutputsTotal;
    public WorldSelectionReadModel Selection = new WorldSelectionReadModel();
    public ExpeditionResultReadModel LatestResult = new ExpeditionResultReadModel();
    public CityStatusReadModel[] Cities = Array.Empty<CityStatusReadModel>();
    public DungeonStatusReadModel[] Dungeons = Array.Empty<DungeonStatusReadModel>();
    public RoadStatusReadModel[] Roads = Array.Empty<RoadStatusReadModel>();
    public ExpeditionStatusReadModel[] ActiveExpeditionEntries = Array.Empty<ExpeditionStatusReadModel>();
    public TradeOpportunityReadModel[] TradeOpportunities = Array.Empty<TradeOpportunityReadModel>();
    public WorldUnmetNeedReadModel[] UnmetCityNeeds = Array.Empty<WorldUnmetNeedReadModel>();
    public DungeonOutputReadModel[] UnclaimedDungeonOutputs = Array.Empty<DungeonOutputReadModel>();
    public WorldDecisionSignalReadModel[] Signals = Array.Empty<WorldDecisionSignalReadModel>();
    public string[] RecentDayLogs = Array.Empty<string>();
    public string[] RecentExpeditionLogs = Array.Empty<string>();
}
