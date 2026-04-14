using System;

public enum RunPhase
{
    None,
    Start,
    Exploring,
    EventChoice,
    PreEliteChoice,
    PendingBattle,
    InBattle,
    PostEncounter,
    Result,
    Completed,
    Failed,
    Retreated
}

public enum DungeonRunStatus
{
    Inactive,
    Active,
    AwaitingChoice,
    InBattle,
    Resolved,
    Failed,
    Completed
}

public sealed class RunLogEntry
{
    public int Sequence;
    public string EntryType = string.Empty;
    public string PhaseKey = string.Empty;
    public string RelatedRoomId = string.Empty;
    public string RelatedEncounterId = string.Empty;
    public string SummaryText = string.Empty;
    public string DetailText = string.Empty;
}

public sealed class EncounterContext
{
    public string EncounterId = string.Empty;
    public string EncounterName = string.Empty;
    public string RoomId = string.Empty;
    public string RoomLabel = string.Empty;
    public string RoomTypeLabel = string.Empty;
    public int RoomIndex;
    public int TotalRooms;
    public bool IsEliteEncounter;
    public bool IsResolved;
    public string PromptText = string.Empty;
    public string PreviewText = string.Empty;
}

public sealed class EventChoiceResult
{
    public string EventId = string.Empty;
    public string EventLabel = string.Empty;
    public string ChoiceId = string.Empty;
    public string ChoiceLabel = string.Empty;
    public string OutcomeTag = string.Empty;
    public string ResultSummaryText = string.Empty;
    public int RewardAmount;
    public int RecoverAmount;
    public bool Resolved;
}

public sealed class BattleHandoffPayload
{
    public string EncounterId = string.Empty;
    public string EncounterName = string.Empty;
    public string DungeonId = string.Empty;
    public string DungeonLabel = string.Empty;
    public string RouteId = string.Empty;
    public string RouteLabel = string.Empty;
    public string RoomId = string.Empty;
    public string RoomLabel = string.Empty;
    public string PartySummaryText = string.Empty;
    public string ObjectiveText = string.Empty;
    public string RiskContextText = string.Empty;
    public bool IsEliteEncounter;
    public int TurnIndex;
    public PrototypeBattleRequest Request = new PrototypeBattleRequest();
}

public sealed class BattleReturnPayload
{
    public string OutcomeKey = PrototypeBattleOutcomeKeys.None;
    public string EncounterId = string.Empty;
    public string EncounterName = string.Empty;
    public string EncounterRoomTypeText = string.Empty;
    public string ObjectiveText = string.Empty;
    public string WhyNowText = string.Empty;
    public string ExpectedUsefulnessText = string.Empty;
    public string RoomProgressText = string.Empty;
    public string NextGoalText = string.Empty;
    public string ResultSummaryText = string.Empty;
    public string PartyConditionText = string.Empty;
    public string PartyHpSummaryText = string.Empty;
    public string LootSummaryText = string.Empty;
    public bool EncounterCleared;
    public bool EliteDefeated;
    public int TurnsTaken;
    public int SurvivingMemberCount;
    public int KnockedOutMemberCount;
    public PrototypeBattleResultSnapshot ResultSnapshot = new PrototypeBattleResultSnapshot();
    public PrototypeBattleResolution Resolution = new PrototypeBattleResolution();
}

public sealed class ExpeditionRunState
{
    public string RunId = string.Empty;
    public int WorldDayCount;
    public ExpeditionPlan LaunchPlan = new ExpeditionPlan();
    public RunPhase Phase = RunPhase.None;
    public DungeonRunStatus Status = DungeonRunStatus.Inactive;
    public string OriginCityId = string.Empty;
    public string OriginCityLabel = "None";
    public string TargetDungeonId = string.Empty;
    public string TargetDungeonLabel = "None";
    public string RouteId = string.Empty;
    public string RouteLabel = "None";
    public string RouteRiskText = "None";
    public string RouteContextText = "None";
    public string ObjectiveText = "None";
    public string WhyNowText = "None";
    public string ExpectedUsefulnessText = "None";
    public string RiskRewardPreviewText = "None";
    public string PlanSummaryText = "None";
    public string PartyId = string.Empty;
    public string PartySummaryText = "None";
    public string PartyHpSummaryText = "None";
    public string PartyConditionText = "None";
    public string SustainPressureText = "None";
    public string CurrentRoomId = string.Empty;
    public string CurrentRoomLabel = "None";
    public string CurrentRoomTypeLabel = "None";
    public string RoomProgressText = "None";
    public string NextGoalText = "None";
    public string CurrentPromptText = "None";
    public string ResultStateKey = PrototypeBattleOutcomeKeys.None;
    public string ResultSummaryText = "None";
    public string RoomPathSummaryText = "None";
    public int CurrentTurnCount;
    public int ClearedEncounterCount;
    public int OpenedChestCount;
    public int CarriedLootAmount;
    public bool ExitUnlocked;
    public bool EliteDefeated;
    public bool EventResolved;
    public bool PreEliteDecisionResolved;
    public bool RunSucceeded;
    public bool RunFailed;
    public bool RunRetreated;
    public bool IsResolved;
    public EncounterContext ActiveEncounter = new EncounterContext();
    public EventChoiceResult LastEventChoice = new EventChoiceResult();
    public EventChoiceResult LastPreEliteChoice = new EventChoiceResult();
    public BattleHandoffPayload PendingBattle = new BattleHandoffPayload();
    public BattleReturnPayload LastBattleReturn = new BattleReturnPayload();
    public PrototypeBattleRuntimeState ActiveBattleState = new PrototypeBattleRuntimeState();
    public PrototypeBattleViewModel ActiveBattleViewModel = new PrototypeBattleViewModel();
    public PrototypeBattleResultSnapshot LatestBattleSnapshot = new PrototypeBattleResultSnapshot();
    public PrototypeRpgRunResultSnapshot ResultSnapshot = new PrototypeRpgRunResultSnapshot();
    public RunLogEntry[] RecentEntries = Array.Empty<RunLogEntry>();
}

public sealed class DungeonRunReadModel
{
    public string ObjectiveText = "None";
    public string PhaseText = "None";
    public string StatusText = "None";
    public string CurrentLocationText = "None";
    public string CurrentChoiceText = "None";
    public string RouteContextText = "None";
    public string PartyStatusText = "None";
    public string RiskRewardText = "None";
    public string NextGoalText = "None";
    public string RecentOutcomeText = "None";
    public string BattleHandoffText = "None";
    public string BattleReturnText = "None";
    public string ResultSummaryText = "None";
    public string[] RecentLogLines = Array.Empty<string>();
}

public sealed class ExpeditionRunStateSeed
{
    public string RunId = string.Empty;
    public int WorldDayCount;
    public ExpeditionPlan LaunchPlan = new ExpeditionPlan();
    public RunPhase Phase = RunPhase.None;
    public DungeonRunStatus Status = DungeonRunStatus.Inactive;
    public string OriginCityId = string.Empty;
    public string OriginCityLabel = "None";
    public string TargetDungeonId = string.Empty;
    public string TargetDungeonLabel = "None";
    public string RouteId = string.Empty;
    public string RouteLabel = "None";
    public string RouteRiskText = "None";
    public string RouteContextText = "None";
    public string ObjectiveText = "None";
    public string WhyNowText = "None";
    public string ExpectedUsefulnessText = "None";
    public string RiskRewardPreviewText = "None";
    public string PlanSummaryText = "None";
    public string PartyId = string.Empty;
    public string PartySummaryText = "None";
    public string PartyHpSummaryText = "None";
    public string PartyConditionText = "None";
    public string SustainPressureText = "None";
    public string CurrentRoomId = string.Empty;
    public string CurrentRoomLabel = "None";
    public string CurrentRoomTypeLabel = "None";
    public string RoomProgressText = "None";
    public string NextGoalText = "None";
    public string CurrentPromptText = "None";
    public string ResultStateKey = PrototypeBattleOutcomeKeys.None;
    public string ResultSummaryText = "None";
    public string RoomPathSummaryText = "None";
    public int CurrentTurnCount;
    public int ClearedEncounterCount;
    public int OpenedChestCount;
    public int CarriedLootAmount;
    public bool ExitUnlocked;
    public bool EliteDefeated;
    public bool EventResolved;
    public bool PreEliteDecisionResolved;
    public bool RunSucceeded;
    public bool RunFailed;
    public bool RunRetreated;
    public bool IsResolved;
    public EncounterContext ActiveEncounter = new EncounterContext();
    public EventChoiceResult LastEventChoice = new EventChoiceResult();
    public EventChoiceResult LastPreEliteChoice = new EventChoiceResult();
    public BattleHandoffPayload PendingBattle = new BattleHandoffPayload();
    public BattleReturnPayload LastBattleReturn = new BattleReturnPayload();
    public PrototypeBattleRuntimeState ActiveBattleState = new PrototypeBattleRuntimeState();
    public PrototypeBattleViewModel ActiveBattleViewModel = new PrototypeBattleViewModel();
    public PrototypeBattleResultSnapshot LatestBattleSnapshot = new PrototypeBattleResultSnapshot();
    public PrototypeRpgRunResultSnapshot ResultSnapshot = new PrototypeRpgRunResultSnapshot();
    public RunLogEntry[] RecentEntries = Array.Empty<RunLogEntry>();
}
