public sealed partial class PrototypeBattleRuntimeState
{
    public string BattleId = string.Empty;
    public string EncounterId = string.Empty;
    public string LaneRuleSetKey = string.Empty;
    public string PositionRuleKey = string.Empty;
    public string PhaseKey = string.Empty;
    public int TurnIndex;
    public string CurrentActorLaneKey = string.Empty;
    public string CurrentActorLaneLabel = string.Empty;
    public string QueuedActionKey = string.Empty;
    public string QueuedActionLabel = string.Empty;
    public string SelectedTargetId = string.Empty;
    public string SelectedTargetLaneKey = string.Empty;
    public string SelectedTargetLaneLabel = string.Empty;
    public string ReachabilityStateKey = string.Empty;
    public string ReachabilitySummaryText = string.Empty;
    public string ThreatLaneKey = string.Empty;
    public string ThreatLaneLabel = string.Empty;
    public string PendingIntentKey = string.Empty;
    public string PendingIntentPreviewText = string.Empty;
    public string PendingIntentTargetId = string.Empty;
    public string TimelineSummaryText = string.Empty;
    public int RecentEventCount;
    public string RecentEventSummaryText = string.Empty;
    public bool IsEnemyTurnActive;
}
