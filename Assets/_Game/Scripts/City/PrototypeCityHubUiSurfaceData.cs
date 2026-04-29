using System;

public sealed class PrototypeCityHubUiSurfaceData
{
    public string SurfaceName = "CityHubUI";
    public string OwnerStageKey = "city_hub";
    public string OwnerStageLabel = "CityHub";
    public string TargetStageKey = "city_interaction";
    public string TargetStageLabel = "CityInteraction";
    public bool HasSelectedCity;
    public PrototypeCityHubHeaderSurfaceData Header = new PrototypeCityHubHeaderSurfaceData();
    public PrototypeCityHubAlertSurfaceData AlertRibbon = new PrototypeCityHubAlertSurfaceData();
    public PrototypeCityHubSelectionSurfaceData Selection = new PrototypeCityHubSelectionSurfaceData();
    public PrototypeCityHubActionSurfaceData Actions = new PrototypeCityHubActionSurfaceData();
    public PrototypeCityHubRecruitmentBoardSurfaceData RecruitmentBoard = new PrototypeCityHubRecruitmentBoardSurfaceData();
    public PrototypeCityHubPartyRosterBoardSurfaceData PartyRosterBoard = new PrototypeCityHubPartyRosterBoardSurfaceData();
    public PrototypeCityHubOutcomeSurfaceData Outcome = new PrototypeCityHubOutcomeSurfaceData();
    public PrototypeCityHubOverviewSurfaceData Overview = new PrototypeCityHubOverviewSurfaceData();
    public PrototypeCityHubLogSurfaceData Logs = new PrototypeCityHubLogSurfaceData();
}

public sealed class PrototypeCityHubHeaderSurfaceData
{
    public string SelectionLabel = "None";
    public string LastTransitionText = "(missing)";
    public int WorldDayCount;
    public bool AutoTickEnabled;
    public int ActiveExpeditions;
    public int IdleParties;
}

public sealed class PrototypeCityHubAlertSurfaceData
{
    public string Title = "World Board";
    public string SummaryText = "None";
    public string FooterText = "None";
}

public sealed class PrototypeCityHubSelectionSurfaceData
{
    public bool HasSelection;
    public bool IsCitySelection;
    public string DisplayName = "None";
    public string TypeLabel = "None";
    public string CityManaShardStockText = "None";
    public string LinkedDungeonText = "None";
    public string LinkedCityText = "None";
    public string NeedPressureText = "None";
    public string DispatchReadinessText = "None";
    public string RecoveryProgressText = "None";
    public string DispatchPolicyText = "None";
    public string PressureBoardSummaryText = "None";
    public string WhyCityMattersText = "None";
    public string RecentResultEvidenceText = "None";
    public string PressureChangeText = "None";
    public string PartyReadinessSummaryText = "None";
    public string RecommendedRouteText = "None";
    public string RecommendationReasonText = "None";
    public string DispatchPartySummaryText = "None";
    public string LaunchLockSummaryText = "None";
    public string ReturnEtaText = "None";
    public string ProjectedOutcomeSummaryText = "None";
    public string RewardPreviewText = "None";
    public string EventPreviewText = "None";
    public string DungeonDangerText = "None";
    public string DungeonStatusText = "None";
    public string DungeonAvailabilityText = "None";
}

public sealed class PrototypeCityHubActionSurfaceData
{
    public bool IsExpeditionPrepBoardOpen;
    public bool CanRecruitParty;
    public bool CanOpenPartyRoster;
    public bool CanOpenExpeditionPrep;
    public bool CanEnterDungeon;
    public string DispatchBriefingSummaryText = "None";
    public string DispatchPartySummaryText = "None";
    public string RouteFitSummaryText = "None";
    public string LaunchLockSummaryText = "None";
    public string ProjectedOutcomeSummaryText = "None";
    public string ActiveExpeditionLaneText = "None";
    public string CityVacancyText = "None";
    public string ReturnEtaText = "None";
    public string BlockedReasonText = "None";
    public string InteractionSummaryText = "None";
    public string EconomyControlsText = "None";
    public string ExpeditionControlsText = "None";
}

public sealed class PrototypeCityHubRecruitmentBoardSurfaceData
{
    public bool IsAvailable;
    public string Title = "Recruitment";
    public string SummaryText = "None";
    public string ActionText = "None";
    public string BlockedReasonText = "None";
}

public sealed class PrototypeCityHubPartyRosterBoardSurfaceData
{
    public bool IsAvailable;
    public bool HasReadyParty;
    public string Title = "Party Roster";
    public string ReadyPartyLabel = "None";
    public string ReadyPartyRoleSummaryText = "None";
    public string ReadyPartyLoadoutSummaryText = "None";
    public string AvailabilitySummaryText = "None";
    public int TotalPartyCount;
    public int IdlePartyCount;
    public int ActiveExpeditionCount;
}

public sealed class PrototypeCityHubOutcomeSurfaceData
{
    public string LatestReturnAftermathText = "None";
    public string OutcomeReadbackText = "None";
    public string CorrectiveFollowUpText = "None";
    public string LootReturnedText = "None";
    public string PartyOutcomeText = "None";
    public string WorldWritebackText = "None";
    public string DungeonStatusText = "None";
    public string DungeonAvailabilityText = "None";
    public string StockReactionText = "None";
    public string DispatchReadinessText = "None";
    public string RecoveryEtaText = "None";
    public string FollowUpHintText = "None";
}

public sealed class PrototypeCityHubOverviewSurfaceData
{
    public int WorldDayCount;
    public int TradeStepCount;
    public int TotalParties;
    public int IdleParties;
    public int ActiveExpeditions;
    public string PriorityCityText = "None";
    public string PrioritySummaryText = "None";
    public string PriorityNextActionText = "None";
    public string ActiveExpeditionLaneText = "None";
    public string ReturnEtaText = "None";
    public string WorldWritebackText = "None";
    public string CitiesWithShortagesText = "None";
    public string LastTransitionText = "(missing)";
}

public sealed class PrototypeCityHubLogSurfaceData
{
    public string[] RecentWorldWritebackLogs = Array.Empty<string>();
    public string[] RecentExpeditionLogs = Array.Empty<string>();
    public string[] RecentDayLogs = Array.Empty<string>();
    public string DepartureEchoText = "None";
    public string ReturnWindowText = "None";
}
