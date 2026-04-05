using System;

public sealed class PrototypeRpgBattleRuntimeState
{
    public bool IsBattleActive;
    public bool IsTargetSelectionActive;
    public string BattleStateKey = "none";
    public string PhaseKey = "inactive";
    public int TurnIndex;
    public string CurrentDungeonName = "None";
    public string CurrentRouteLabel = "None";
    public string CurrentEncounterId = string.Empty;
    public PrototypeRpgEncounterRuntimeState EncounterRuntime = new PrototypeRpgEncounterRuntimeState();
    public string CurrentActorId = string.Empty;
    public int CurrentActorIndex = -1;
    public string CurrentActorLabel = "None";
    public bool CurrentActorIsEnemy;
    public string ActingEnemyId = string.Empty;
    public string QueuedActionKey = string.Empty;
    public string QueuedActionLabel = "Action";
    public string SelectedTargetId = string.Empty;
    public string HoveredTargetId = string.Empty;
    public string PendingEnemyTargetId = string.Empty;
    public string FeedbackText = string.Empty;
    public string SelectionPromptText = string.Empty;
    public string CancelHintText = string.Empty;
    public string PartyConditionText = "None";
    public string TotalPartyHpText = "None";
    public string EliteStatusText = "None";
    public string EliteEncounterName = "None";
    public string EliteTypeText = "None";
    public string EliteRewardHintText = "None";
    public string EnemyIntentText = "None";
    public string[] RecentLogs = Array.Empty<string>();
    public PrototypeEnemyIntentSnapshot EnemyIntent = new PrototypeEnemyIntentSnapshot();
    public PrototypeBattleEventRecord[] RecentEvents = Array.Empty<PrototypeBattleEventRecord>();
    public PrototypeBattleResultSnapshot ResultSnapshot = new PrototypeBattleResultSnapshot();
    public PrototypeRpgCombatContributionSnapshot PartyContribution = new PrototypeRpgCombatContributionSnapshot();
    public PrototypeCombatResolutionRecord CurrentActionResolution = new PrototypeCombatResolutionRecord();
    public PrototypeCombatResolutionRecord PendingEnemyResolution = new PrototypeCombatResolutionRecord();
}
