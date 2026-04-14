public sealed class WorldObservationSurfaceData
{
    public string CurrentWorldObservationSummaryText = "None";
    public string CurrentExpeditionStartContextSummaryText = "None";
    public bool AutoTickEnabled;
    public int WorldDayCount;
    public int TradeStepCount;
    public int TotalParties;
    public int IdleParties;
    public int ActiveExpeditions;
    public string CitiesWithShortagesText = "None";
    public ExpeditionStartContext CurrentExpeditionStartContext = new ExpeditionStartContext();
    public WorldObservationCurrentContextData CurrentContext = new WorldObservationCurrentContextData();
    public WorldObservationSelectedEntityData SelectedEntity = new WorldObservationSelectedEntityData();
    public ExpeditionPrepSurfaceData ExpeditionPrep = new ExpeditionPrepSurfaceData();
    public WorldObservationLaunchData Launch = new WorldObservationLaunchData();
    public WorldObservationActiveExpeditionData ActiveExpedition = new WorldObservationActiveExpeditionData();
    public WorldObservationRecentOutcomeData RecentOutcome = new WorldObservationRecentOutcomeData();
    public CityHubSurfaceData CityHub = new CityHubSurfaceData();
    public WorldObservationLogData Logs = new WorldObservationLogData();
}

public sealed class WorldObservationCurrentContextData
{
    public bool HasContext;
    public string CityId = string.Empty;
    public string CityLabel = "None";
    public string DungeonId = string.Empty;
    public string DungeonLabel = "None";
    public string RouteId = string.Empty;
    public string RouteLabel = "None";
    public string RecommendedRouteId = string.Empty;
    public string RecommendedRouteLabel = "None";
    public string PartyId = string.Empty;
    public string PartyLabel = "None";
}

public sealed class WorldObservationSelectedEntityData
{
    public bool HasSelection;
    public string SelectedKindKey = "none";
    public string SelectedKindLabel = "None";
    public string SelectedEntityId = string.Empty;
    public string SelectedEntityLabel = "None";
    public string SelectedPositionText = "None";
    public string SelectedResourcesText = "None";
    public string SelectedResourceRolesText = "None";
    public string SelectedSupplyText = "None";
    public string SelectedNeedsText = "None";
    public string SelectedHighPriorityNeedsText = "None";
    public string SelectedNormalPriorityNeedsText = "None";
    public string SelectedOutputText = "None";
    public string SelectedProcessingText = "None";
    public string SelectedStocksText = "None";
    public string SelectedLinkedCityText = "None";
    public string SelectedPartiesInCityText = "None";
    public string SelectedIdlePartiesText = "None";
    public string SelectedActiveExpeditionsFromCityText = "None";
    public string SelectedAvailableContractText = "None";
    public string SelectedLinkedDungeonText = "None";
    public string SelectedDungeonDangerText = "None";
    public string SelectedCityManaShardStockText = "None";
    public string SelectedCityId = string.Empty;
    public string SelectedCityLabel = "None";
    public string SelectedDungeonId = string.Empty;
    public string SelectedDungeonLabel = "None";
    public string SelectedRouteId = string.Empty;
    public string SelectedRouteLabel = "None";
    public string SelectedPartyId = string.Empty;
    public string SelectedPartyLabel = "None";
    public string SelectedWorldWritebackText = "None";
    public string SelectedDungeonStatusText = "None";
    public string SelectedDungeonAvailabilityText = "None";
}

public sealed class WorldObservationLaunchData
{
    public bool CanLaunch;
    public bool HasSelectedRoute;
    public ExpeditionLaunchRequest LaunchRequest = new ExpeditionLaunchRequest();
    public string DispatchPolicyText = "None";
    public string NeedPressureText = "None";
    public string DispatchReadinessText = "None";
    public string RecoveryProgressText = "None";
    public string RecoveryEtaText = "None";
    public LaunchReadiness LaunchReadiness = new LaunchReadiness();
    public string BlockedReasonSummaryText = "None";
    public string CurrentBlockedLaunchReasonSummaryText = "None";
    public string RecommendedNextActionSummaryText = "None";
    public string DispatchPartySummaryText = "None";
    public string DispatchBriefingSummaryText = "None";
    public string RouteFitSummaryText = "None";
    public string LaunchGateSummaryText = "None";
    public string ProjectedOutcomeSummaryText = "None";
    public string RecommendedRouteId = string.Empty;
    public string RecommendedRouteLabel = "None";
    public string RecommendedRouteSummaryText = "None";
    public string RecommendedRouteForLinkedCityText = "None";
    public string RecommendationReasonText = "None";
    public int RecommendedPower;
    public int ExpeditionDurationDays;
    public string RewardPreviewText = "None";
    public string EventPreviewText = "None";
    public string ExpectedNeedImpactText = "None";
}

public sealed class WorldObservationActiveExpeditionData
{
    public bool HasSelectedActiveExpedition;
    public string SelectedCityId = string.Empty;
    public string SelectedCityLabel = "None";
    public string SelectedDungeonId = string.Empty;
    public string SelectedDungeonLabel = "None";
    public string SelectedPartyId = string.Empty;
    public string SelectedPartyLabel = "None";
    public string SelectedRouteId = string.Empty;
    public string SelectedRouteLabel = "None";
    public string SelectedTravelStateKey = "idle";
    public int SelectedDaysRemaining;
    public int SelectedProjectedReturnDay = -1;
    public string SelectedActiveExpeditionLaneText = "None";
    public string SelectedDepartureEchoText = "None";
    public string SelectedReturnEtaText = "None";
    public string SelectedCityVacancyText = "None";
    public string SelectedDungeonInboundExpeditionText = "None";
    public string SelectedRouteOccupancyText = "None";
    public string SelectedReturnWindowText = "None";
    public string SelectedRecoveryAfterReturnText = "None";
    public bool HasWorldActiveExpedition;
    public string WorldCityId = string.Empty;
    public string WorldCityLabel = "None";
    public string WorldDungeonId = string.Empty;
    public string WorldDungeonLabel = "None";
    public string WorldPartyId = string.Empty;
    public string WorldPartyLabel = "None";
    public string WorldRouteId = string.Empty;
    public string WorldRouteLabel = "None";
    public string WorldTravelStateKey = "idle";
    public int WorldDaysRemaining;
    public int WorldProjectedReturnDay = -1;
    public string WorldActiveExpeditionLaneText = "None";
    public string WorldDepartureEchoText = "None";
    public string WorldReturnEtaText = "None";
}

public sealed class WorldObservationRecentOutcomeData
{
    public bool HasSelectedWorldWriteback;
    public bool HasLatestWorldWriteback;
    public WorldWriteback SelectedWorldWriteback = new WorldWriteback();
    public WorldWriteback LatestWorldWriteback = new WorldWriteback();
    public ExpeditionPostRunRevealState PostRunReveal = new ExpeditionPostRunRevealState();
    public OutcomeReadback ResultPipelineReadback = new OutcomeReadback();
    public string WorldWritebackSummaryText = "None";
    public string RecentAftermathSummaryText = "None";
    public string SelectedLastDispatchImpactText = "None";
    public string SelectedLastDispatchStockDeltaText = "None";
    public string SelectedLastNeedPressureChangeText = "None";
    public string SelectedLastDispatchReadinessChangeText = "None";
    public string SelectedActiveExpeditionsText = "None";
    public string SelectedLastExpeditionResultText = "None";
    public string SelectedExpeditionLootReturnedText = "None";
    public string SelectedLastRunSurvivingMembersText = "None";
    public string SelectedLastRunClearedEncountersText = "None";
    public string SelectedLastRunEventChoiceText = "None";
    public string SelectedLastRunLootBreakdownText = "None";
    public string SelectedLastRunDungeonText = "None";
    public string SelectedLastRunRouteText = "None";
}

public sealed class WorldObservationLogData
{
    public string RecentWorldWritebackLog1Text = "None";
    public string RecentWorldWritebackLog2Text = "None";
    public string RecentWorldWritebackLog3Text = "None";
    public string RecentLaunchContextLog1Text = "None";
    public string RecentLaunchContextLog2Text = "None";
    public string RecentLaunchContextLog3Text = "None";
    public string RecentExpeditionLog1Text = "None";
    public string RecentExpeditionLog2Text = "None";
    public string RecentExpeditionLog3Text = "None";
    public string RecentDayLog1Text = "None";
    public string RecentDayLog2Text = "None";
    public string RecentDayLog3Text = "None";
}
