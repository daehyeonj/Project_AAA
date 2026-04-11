public static class PrototypeCityHubActionKeys
{
    public const string RecruitSelectedCityParty = "recruit_selected_city_party";
    public const string EnterSelectedWorldDungeon = "enter_selected_world_dungeon";
    public const string OpenSelectedWorldExpeditionPrepBoard = "open_selected_world_expedition_prep_board";
    public const string CancelSelectedWorldExpeditionPrepBoard = "cancel_selected_world_expedition_prep_board";
    public const string SelectExpeditionPrepRoute = "select_expedition_prep_route";
    public const string CycleExpeditionPrepDispatchPolicy = "cycle_expedition_prep_dispatch_policy";
    public const string ConfirmSelectedExpeditionLaunch = "confirm_selected_expedition_launch";
}

public sealed class PrototypeCityHubActionRequest
{
    public string SourceStageKey = "city_hub";
    public string SourceStageLabel = "CityHub";
    public string TargetStageKey = "city_interaction";
    public string TargetStageLabel = "CityInteraction";
    public string ActionKey = string.Empty;
    public int OptionIndex = -1;
    public string OptionKey = string.Empty;
    public string SourceSurfaceName = "CityHubUI";
}

public sealed class PrototypeCityHubActionResult
{
    public string OwnerStageKey = "city_interaction";
    public string OwnerStageLabel = "CityInteraction";
    public string ActionKey = string.Empty;
    public bool WasHandled;
    public bool Succeeded;
    public bool TransitionToDungeonRunRequested;
    public bool ShouldRefreshPresentation;
    public string ErrorSummaryText = "None";
    public string RefreshReasonText = "None";
}
