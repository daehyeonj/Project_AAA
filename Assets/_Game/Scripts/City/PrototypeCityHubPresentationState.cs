public enum PrototypeCityHubTopDropdownMode
{
    None,
    Snapshot,
    Selection,
    Operations
}

public enum PrototypeCityHubBottomSheetMode
{
    None,
    Actions,
    Selection,
    Overview,
    Logs
}

public enum PrototypeCityHubModalState
{
    None,
    ExpeditionPrepBoard,
    PostRunRevealSpotlight
}

public sealed class PrototypeCityHubPresentationState
{
    public PrototypeCityHubTopDropdownMode ActiveTopDropdown = PrototypeCityHubTopDropdownMode.None;
    public PrototypeCityHubBottomSheetMode ActiveBottomSheet = PrototypeCityHubBottomSheetMode.None;
    public PrototypeCityHubModalState ActiveModal = PrototypeCityHubModalState.None;
    public bool IsBoardOverlayEnabled;
    public string LastStateChangeReasonText = "None";
    public string LastRefreshHintText = "None";

    public bool HasBlockingModal => ActiveModal != PrototypeCityHubModalState.None;
}
