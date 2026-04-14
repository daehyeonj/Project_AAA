using System;

public enum PrototypeBattlePhase
{
    None,
    PendingStart,
    PlayerCommand,
    PlayerTargetSelection,
    EnemyIntent,
    EnemyAction,
    Victory,
    Defeat,
    Retreat,
    Result
}

public enum PrototypeBattleResultType
{
    None,
    EncounterVictory,
    RunClear,
    RunDefeat,
    RunRetreat
}

public enum PrototypeBattleCommandType
{
    None,
    Attack,
    Skill,
    Retreat
}

public sealed class PrototypeBattleCombatantState
{
    public string CombatantId = string.Empty;
    public string TeamKey = string.Empty;
    public int SlotIndex = -1;
    public string DisplayName = "None";
    public string RoleLabel = "None";
    public string LaneLabel = "None";
    public string SkillId = string.Empty;
    public string SkillLabel = string.Empty;
    public string IntentLabel = string.Empty;
    public string StatusText = string.Empty;
    public int CurrentHp;
    public int MaxHp = 1;
    public int Attack;
    public int Defense;
    public int Speed;
    public bool IsElite;
    public bool IsActing;
    public bool IsSelected;
    public bool IsTargeted;
    public bool IsHovered;
    public bool IsDefeated;
}

public sealed class PrototypeBattleRequest
{
    public string EncounterId = string.Empty;
    public string EncounterName = "None";
    public string DungeonId = string.Empty;
    public string DungeonLabel = "None";
    public string RouteId = string.Empty;
    public string RouteLabel = "None";
    public string RoomId = string.Empty;
    public string RoomLabel = "None";
    public string RoomTypeLabel = "None";
    public string PartySummaryText = "None";
    public string ObjectiveText = "None";
    public string RiskContextText = "None";
    public string RewardPreviewText = "None";
    public string EncounterProfileId = string.Empty;
    public string EncounterProfileSourceText = "fallback:hardcoded";
    public string EncounterContextText = "None";
    public string BattleSetupId = string.Empty;
    public string BattleSetupSourceText = "fallback:hardcoded";
    public string EnemyGroupText = "None";
    public string BattleSetupSummaryText = "None";
    public string EliteTypeText = "None";
    public string EliteRewardHintText = "None";
    public bool IsEliteEncounter;
    public bool RetreatAllowed = true;
    public int EnterTurnIndex;
    public PrototypeBattleCombatantState[] PartyMembers = Array.Empty<PrototypeBattleCombatantState>();
    public PrototypeBattleCombatantState[] EnemyMembers = Array.Empty<PrototypeBattleCombatantState>();
}

public sealed class PrototypeBattleCommand
{
    public PrototypeBattleCommandType CommandType = PrototypeBattleCommandType.None;
    public string CommandKey = string.Empty;
    public string ActorId = string.Empty;
    public string ActorName = string.Empty;
    public int ActorIndex = -1;
    public string SkillId = string.Empty;
    public string SkillLabel = string.Empty;
    public string TargetKind = string.Empty;
    public string EffectType = string.Empty;
    public int PowerValue;
    public bool RequiresTarget;
    public string TargetId = string.Empty;
    public string TargetName = string.Empty;
    public int TargetIndex = -1;
    public bool IsTargetLocked;
    public string SummaryText = string.Empty;
}

public sealed class PrototypeBattleRuntimeState
{
    public bool IsBattleActive;
    public string BattleStateKey = PrototypeBattleOutcomeKeys.None;
    public PrototypeBattlePhase Phase = PrototypeBattlePhase.None;
    public string PhaseLabel = "None";
    public int BattleTurnIndex;
    public int CurrentActorIndex = -1;
    public int SelectedPartyMemberIndex = -1;
    public string CurrentActorId = string.Empty;
    public string FocusedEnemyId = string.Empty;
    public string HoveredEnemyId = string.Empty;
    public bool IsTargetSelectionActive;
    public bool IsInputLocked;
    public bool CanAttack;
    public bool CanSkill;
    public bool CanRetreat;
    public string PromptText = "None";
    public string FeedbackText = "None";
    public string PartySummaryText = "None";
    public string PartyHpSummaryText = "None";
    public string PartyConditionText = "None";
    public PrototypeBattleCombatantState CurrentActor = new PrototypeBattleCombatantState();
    public PrototypeBattleCombatantState[] PartyMembers = Array.Empty<PrototypeBattleCombatantState>();
    public PrototypeBattleCombatantState[] EnemyMembers = Array.Empty<PrototypeBattleCombatantState>();
    public PrototypeBattleCommand SelectedCommand = new PrototypeBattleCommand();
    public PrototypeEnemyIntentSnapshot PendingEnemyIntent = new PrototypeEnemyIntentSnapshot();
    public PrototypeBattleEventRecord[] RecentEvents = Array.Empty<PrototypeBattleEventRecord>();
    public string[] RecentLogLines = Array.Empty<string>();
    public PrototypeBattleResultSnapshot CurrentResultSnapshot = new PrototypeBattleResultSnapshot();
}

public sealed class PrototypeBattleResolution
{
    public PrototypeBattleResultType ResultType = PrototypeBattleResultType.None;
    public string OutcomeKey = PrototypeBattleOutcomeKeys.None;
    public string EncounterId = string.Empty;
    public string EncounterName = "None";
    public string DungeonId = string.Empty;
    public string DungeonLabel = "None";
    public string RouteLabel = "None";
    public string SummaryText = "None";
    public string PartyConditionText = "None";
    public string PartyHpSummaryText = "None";
    public string LootSummaryText = "None";
    public bool EncounterCleared;
    public bool RunEnded;
    public bool EliteDefeated;
    public int TurnsTaken;
    public int SurvivingMemberCount;
    public int KnockedOutMemberCount;
    public int DefeatedEnemyCount;
    public int ReturnedLootAmount;
    public int TotalDamageDealt;
    public int TotalDamageTaken;
    public int TotalHealingDone;
    public string[] ResultTags = Array.Empty<string>();
    public PrototypeBattleCombatantState[] PartyMembersAtEnd = Array.Empty<PrototypeBattleCombatantState>();
    public PrototypeBattleCombatantState[] EnemyMembersAtEnd = Array.Empty<PrototypeBattleCombatantState>();
    public PrototypeBattleEventRecord[] RecentEvents = Array.Empty<PrototypeBattleEventRecord>();
    public PrototypeBattleResultSnapshot ResultSnapshot = new PrototypeBattleResultSnapshot();
}

public sealed class PrototypeBattleViewModel
{
    public string EncounterTitle = "None";
    public string PhaseText = "None";
    public string TurnText = "None";
    public string CurrentActorText = "None";
    public string ResultText = "None";
    public string[] RecentLogLines = Array.Empty<string>();
    public PrototypeBattleRequest Request = new PrototypeBattleRequest();
    public PrototypeBattleRuntimeState State = new PrototypeBattleRuntimeState();
    public PrototypeBattleResolution Resolution = new PrototypeBattleResolution();
    public PrototypeBattleUiSurfaceData HudSurface = new PrototypeBattleUiSurfaceData();
}
