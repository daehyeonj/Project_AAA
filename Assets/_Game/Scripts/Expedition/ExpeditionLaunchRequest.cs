public sealed class ExpeditionLaunchRequest
{
    public string SourceStageKey = "expedition_prep";
    public string SourceStageLabel = "ExpeditionPrep";
    public string TargetStageKey = "dungeon_run";
    public string TargetStageLabel = "DungeonRun";
    public bool IsPreparationBoardOpen;
    public bool HasStagedParty;
    public bool CanConfirmLaunch;
    public string CityId = string.Empty;
    public string CityLabel = "None";
    public string DungeonId = string.Empty;
    public string DungeonLabel = "None";
    public string StagedPartyId = string.Empty;
    public string StagedPartyLabel = "None";
    public string SelectedRouteId = string.Empty;
    public string SelectedRouteLabel = "None";
    public string RecommendedRouteId = string.Empty;
    public string RecommendedRouteLabel = "None";
    public string DispatchPolicyText = "None";
    public string NeedPressureText = "None";
    public string DispatchReadinessText = "None";
    public string ExpectedNeedImpactText = "None";
    public string LaunchGateSummaryText = "None";
    public string BlockedReasonText = "None";
    public ExpeditionStartContext StartContext = new ExpeditionStartContext();
}
