public enum AppFlowStage
{
    Boot,
    MainMenu,
    WorldSim,
    CityHub,
    ExpeditionPrep,
    DungeonRun,
    BattleScene,
    ResultPipeline
}

public sealed class AppFlowWorldSelection
{
    public int WorldDayCount;
    public string SelectedCityId = string.Empty;
    public string SelectedCityLabel = "None";
    public string SelectedDungeonId = string.Empty;
    public string SelectedDungeonLabel = "None";
    public string IdlePartyId = string.Empty;
}

public sealed class AppFlowExpeditionPlan
{
    public string CityId = string.Empty;
    public string CityLabel = "None";
    public string DungeonId = string.Empty;
    public string DungeonLabel = "None";
    public string PartyId = string.Empty;
    public string SelectedRouteId = string.Empty;
    public string SelectedRouteLabel = "None";
    public string RecommendedRouteId = string.Empty;
    public string RecommendedRouteLabel = "None";
    public string DispatchPolicyText = "None";
    public string ObjectiveText = "None";
    public string WhyNowText = "None";
    public string ExpectedUsefulnessText = "None";
    public string RiskRewardPreviewText = "None";
    public string PlanSummaryText = "None";
    public bool HasConfirmedPlan;
    public LaunchReadiness LaunchReadiness = new LaunchReadiness();
    public ExpeditionPrepReadModel PrepReadModel = new ExpeditionPrepReadModel();
    public ExpeditionPlan CurrentPlan = new ExpeditionPlan();
    public ExpeditionPlan ConfirmedPlan = new ExpeditionPlan();
}

public sealed class AppFlowDungeonRunContext
{
    public string CityId = string.Empty;
    public string CityLabel = "None";
    public string DungeonId = string.Empty;
    public string DungeonLabel = "None";
    public string PartyId = string.Empty;
    public string RouteId = string.Empty;
    public string RouteLabel = "None";
    public int TurnCount;
    public string LastBattleOutcomeKey = PrototypeBattleOutcomeKeys.None;
    public string LastEncounterName = "None";
    public ExpeditionPlan LaunchPlan = new ExpeditionPlan();
    public ExpeditionRunState RunState = new ExpeditionRunState();
    public DungeonRunReadModel ScreenModel = new DungeonRunReadModel();
}

public sealed class AppFlowBattleContext
{
    public string EncounterId = string.Empty;
    public string EncounterName = "None";
    public string EncounterRoomType = "None";
    public string BattleStateLabel = "None";
    public PrototypeBattleResultSnapshot ResultSnapshot = new PrototypeBattleResultSnapshot();
    public BattleHandoffPayload HandoffPayload = new BattleHandoffPayload();
    public BattleReturnPayload ReturnPayload = new BattleReturnPayload();
}

public sealed class AppFlowResultContext
{
    public ExpeditionOutcome ExpeditionOutcome = new ExpeditionOutcome();
    public OutcomeReadback OutcomeReadback = new OutcomeReadback();
    public PrototypeRpgRunResultSnapshot RunResultSnapshot = new PrototypeRpgRunResultSnapshot();
    public ExpeditionRunState ResolvedRunState = new ExpeditionRunState();
    public bool IsAppliedToWorld;
    public string AppliedWorldStateMarker = "None";
}

public sealed class AppFlowObservedSnapshot
{
    public AppFlowStage ObservedStage = AppFlowStage.WorldSim;
    public bool HasPendingWorldReturn;
    public AppFlowWorldSelection WorldSelection = new AppFlowWorldSelection();
    public AppFlowExpeditionPlan ActiveExpeditionPlan = new AppFlowExpeditionPlan();
    public AppFlowDungeonRunContext CurrentDungeonRun = new AppFlowDungeonRunContext();
    public AppFlowBattleContext PendingBattle = new AppFlowBattleContext();
    public AppFlowResultContext LatestResult = new AppFlowResultContext();
}

public sealed class AppFlowContext
{
    public AppFlowWorldSelection WorldSelection = new AppFlowWorldSelection();
    public AppFlowExpeditionPlan ActiveExpeditionPlan = new AppFlowExpeditionPlan();
    public AppFlowDungeonRunContext CurrentDungeonRun = new AppFlowDungeonRunContext();
    public AppFlowBattleContext PendingBattle = new AppFlowBattleContext();
    public AppFlowResultContext LatestResult = new AppFlowResultContext();
}
