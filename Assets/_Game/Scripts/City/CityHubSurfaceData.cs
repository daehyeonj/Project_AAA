public sealed class CityHubSurfaceData
{
    public string OwnerStageKey = "city_hub";
    public string OwnerStageLabel = "CityHub";
    public bool HasSelectedCity;
    public WorldSimCitySourceData EntrySnapshot = new WorldSimCitySourceData();
    public WorldSimCitySourceData WorldSimInput = new WorldSimCitySourceData();
    public CitySimSurfaceData CitySim = new CitySimSurfaceData();
    public CityInteractionSurfaceData CityInteraction = new CityInteractionSurfaceData();
    public CityPartyRosterSurfaceData PartyRoster = new CityPartyRosterSurfaceData();
    public ExpeditionPrepHandoff ExpeditionPrep = new ExpeditionPrepHandoff();
    public OutcomeReadback ResultPipelineReadback = new OutcomeReadback();
    public CityWriteback ResultPipelineCityWriteback = new CityWriteback();
}

public sealed class WorldSimCitySourceData
{
    public string OwnerStageKey = "world_sim";
    public string OwnerStageLabel = "WorldSim";
    public string TargetStageKey = "city_hub";
    public string TargetStageLabel = "CityHub";
    public bool HasSelectedCity;
    public string SelectedCityId = string.Empty;
    public string SelectedCityLabel = "None";
    public string CityManaShardStockText = "None";
    public string LinkedDungeonId = string.Empty;
    public string LinkedDungeonLabel = "None";
    public string RecommendedRouteId = string.Empty;
    public string RecommendedRouteLabel = "None";
    public string RecommendedRouteSummaryText = "None";
    public string NeedPressureText = "None";
    public string DispatchReadinessText = "None";
    public string RecoveryProgressText = "None";
    public string RecoveryEtaText = "None";
    public string DispatchPolicyText = "None";
    public string RecentOutcomeText = "None";
    public bool CanRecruitParty;
    public bool CanOpenPartyRoster;
    public bool HasStagedParty;
    public bool CanOpenExpeditionPrep;
    public string StagedPartyId = string.Empty;
    public string StagedPartyLabel = "None";
    public string BlockedReasonText = "None";
    public string RecommendedNextActionText = "None";
    public string PartyRosterSummaryText = "None";
    public string ExpeditionContextSummaryText = "None";
    public string AvailabilitySummaryText = "None";
    public string SelectionSummaryText = "None";
}

public sealed class CitySimSurfaceData
{
    public string OwnerStageKey = "city_sim";
    public string OwnerStageLabel = "CitySim";
    public string CityId = string.Empty;
    public string CityLabel = "None";
    public string NeedPressureText = "None";
    public string DispatchReadinessText = "None";
    public string RecoveryProgressText = "None";
    public string RecoveryEtaText = "None";
    public string DispatchPolicyText = "None";
    public string RecommendedRouteSummaryText = "None";
    public string RecommendationReasonText = "None";
    public string CityVacancyText = "None";
    public string LastDispatchImpactText = "None";
    public string LastDispatchStockDeltaText = "None";
    public string LastNeedPressureChangeText = "None";
    public string LastDispatchReadinessChangeText = "None";
    public string LastExpeditionResultText = "None";
    public string WorldWritebackText = "None";
}

public sealed class CityInteractionSurfaceData
{
    public string OwnerStageKey = "city_interaction";
    public string OwnerStageLabel = "CityInteraction";
    public bool CanRecruitParty;
    public bool CanOpenPartyRoster;
    public bool CanOpenExpeditionPrep;
    public string BlockedReasonText = "None";
    public string RecruitmentActionText = "None";
    public string PartyRosterActionText = "None";
    public string InteractionSummaryText = "None";
}

public sealed class CityPartyRosterSurfaceData
{
    public string OwnerStageKey = "city_party_roster";
    public string OwnerStageLabel = "PartyRoster";
    public string CityId = string.Empty;
    public string CityLabel = "None";
    public bool HasReadyParty;
    public string ReadyPartyId = string.Empty;
    public string ReadyPartyLabel = "None";
    public int TotalPartyCount;
    public int IdlePartyCount;
    public int ActiveExpeditionCount;
    public int ReadyPartyPower;
    public int ReadyPartyCarryCapacity;
    public string ReadyPartyRoleSummaryText = "None";
    public string ReadyPartyLoadoutSummaryText = "None";
    public string ActivePartyStatusText = "None";
    public string AvailabilitySummaryText = "None";
}

public sealed class ExpeditionPrepHandoff
{
    public string SourceStageKey = "city_hub";
    public string SourceStageLabel = "CityHub";
    public string TargetStageKey = "expedition_prep";
    public string TargetStageLabel = "ExpeditionPrep";
    public bool IsReady;
    public string CityId = string.Empty;
    public string CityLabel = "None";
    public string DungeonId = string.Empty;
    public string DungeonLabel = "None";
    public string PartyId = string.Empty;
    public string PartyLabel = "None";
    public string PartyLoadoutSummaryText = "None";
    public string SelectedRouteId = string.Empty;
    public string SelectedRouteLabel = "None";
    public string RecommendedRouteId = string.Empty;
    public string RecommendedRouteLabel = "None";
    public string RouteRiskText = "None";
    public string DispatchPolicyText = "None";
    public string NeedPressureText = "None";
    public string DispatchReadinessText = "None";
    public string RecoveryProgressText = "None";
    public string RecoveryEtaText = "None";
    public string RecommendationReasonText = "None";
    public string ExpectedNeedImpactText = "None";
    public LaunchReadiness LaunchReadiness = new LaunchReadiness();
    public ExpeditionStartContext StartContext = new ExpeditionStartContext();
    public string BlockedReasonText = "None";
    public string WarningSummaryText = "None";
    public string RecommendedNextActionText = "None";
    public string RouteFitSummaryText = "None";
    public string LaunchGateSummaryText = "None";
    public string ProjectedOutcomeSummaryText = "None";
    public string RewardPreviewText = "None";
    public string EventPreviewText = "None";
    public string SummaryText = "None";
}
