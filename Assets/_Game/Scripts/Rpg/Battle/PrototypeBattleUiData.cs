using System;

public sealed class PrototypeBattleUiSurfaceData
{
    public string SceneSurfaceName = "BattleScene";
    public string HudSurfaceName = "BattleHUD";
    public bool IsBattleActive;
    public bool IsTargetSelectionActive;
    public string BattleStateKey = "none";
    public string CurrentDungeonName = "None";
    public string CurrentRouteLabel = "None";
    public string EncounterName = "None";
    public string EncounterRoomType = "None";
    public string PartyCondition = "None";
    public string TotalPartyHp = "None";
    public string EliteStatusText = "None";
    public string EliteEncounterName = "None";
    public string EliteTypeText = "None";
    public string EliteRewardHintText = "None";
    public PrototypeBattleUiActorData CurrentActor = new PrototypeBattleUiActorData();
    public PrototypeBattleUiActionContextData ActionContext = new PrototypeBattleUiActionContextData();
    public PrototypeBattleUiTargetContextData TargetContext = new PrototypeBattleUiTargetContextData();
    public PrototypeBattleUiTimelineData Timeline = new PrototypeBattleUiTimelineData();
    public PrototypeBattleUiPartyMemberData[] PartyMembers = Array.Empty<PrototypeBattleUiPartyMemberData>();
    public PrototypeBattleUiEnemyData SelectedEnemy = new PrototypeBattleUiEnemyData();
    public PrototypeBattleUiEnemyData[] EnemyRoster = Array.Empty<PrototypeBattleUiEnemyData>();
    public PrototypeBattleUiCommandSurfaceData CommandSurface = new PrototypeBattleUiCommandSurfaceData();
    public PrototypeBattleUiMessageSurfaceData MessageSurface = new PrototypeBattleUiMessageSurfaceData();
    public PrototypeBattleUiTargetSelectionData TargetSelection = new PrototypeBattleUiTargetSelectionData();
    public PrototypeEnemyIntentSnapshot EnemyIntent = new PrototypeEnemyIntentSnapshot();
    public PrototypeBattleEventRecord[] RecentEvents = Array.Empty<PrototypeBattleEventRecord>();
    public PrototypeBattleResultSnapshot ResultSnapshot = new PrototypeBattleResultSnapshot();
}

public sealed class PrototypeBattleUiActorData
{
    public string ActorId = string.Empty;
    public string DisplayName = "None";
    public string RoleLabel = "None";
    public string SkillLabel = "None";
    public string SkillShortText = string.Empty;
    public string LevelSummaryText = string.Empty;
    public string LoadoutSummaryText = string.Empty;
    public string ConsumableSlotSummaryText = string.Empty;
    public string ActiveSkillSlotSummaryText = string.Empty;
    public string SkillCooldownSummaryText = string.Empty;
    public string SkillResourceSummaryText = string.Empty;
    public string ActionEconomySummaryText = string.Empty;
    public int CurrentHp;
    public int MaxHp = 1;
    public bool IsEnemy;
    public string StatusText = "Idle";
}

public sealed class PrototypeBattleUiActionContextData
{
    public string ActorId = string.Empty;
    public int ActorIndex = -1;
    public string SelectedActionKey = string.Empty;
    public string SelectedActionLabel = "Action";
    public string SelectedTargetId = string.Empty;
    public int ResolvedSlotIndex = -1;
    public string ResolvedSlotKey = string.Empty;
    public string ResolvedSkillId = string.Empty;
    public string ResolvedSkillLabel = string.Empty;
    public string ResolvedLoadoutId = string.Empty;
    public string ResolvedLoadoutSummaryText = string.Empty;
    public string ResolvedSkillSegmentKey = string.Empty;
    public string ResolvedTargetKind = string.Empty;
    public string ResolvedEffectBucketKey = string.Empty;
    public string ResolvedEffectType = string.Empty;
    public int ResolvedPowerValue;
    public int ResolvedCooldownTurns;
    public int ResolvedCooldownRemaining;
    public int ResolvedResourceCost;
    public int ResolvedCurrentResource;
    public int ResolvedMaxResource = 1;
    public int ResolvedEncounterChargesRemaining;
    public int ResolvedMaxEncounterCharges;
    public string ResolvedAvailabilityStateKey = string.Empty;
    public string ResolvedAvailabilitySummaryText = string.Empty;
    public string ActiveSkillSlotSummaryText = string.Empty;
    public string SkillCooldownSummaryText = string.Empty;
    public string SkillResourceSummaryText = string.Empty;
    public string ActionEconomySummaryText = string.Empty;
    public string ConsumableSlotSummaryText = string.Empty;
    public bool IsSkillAction;
    public bool RequiresTarget;
}

public sealed class PrototypeBattleUiTargetContextData
{
    public bool HasTarget;
    public string TargetMonsterId = string.Empty;
    public int TargetDisplayIndex = -1;
    public string TargetLabel = "Choose a target";
    public string TargetRoleLabel = string.Empty;
    public string TargetIntentLabel = string.Empty;
    public string TargetStateText = "No target";
    public int TargetCurrentHp;
    public int TargetMaxHp = 1;
    public bool IsHovered;
    public bool IsLocked;
    public bool IsDefeated;
}

public sealed class PrototypeBattleUiTimelineData
{
    public string PhaseLabel = "None";
    public string NextStepLabel = "Select an action.";
    public PrototypeBattleUiTimelineSlotData[] Slots = Array.Empty<PrototypeBattleUiTimelineSlotData>();
}

public sealed class PrototypeBattleUiTimelineSlotData
{
    public string Label = "None";
    public string SecondaryLabel = string.Empty;
    public string StatusLabel = string.Empty;
    public bool IsCurrent;
    public bool IsEnemy;
    public bool IsPending;
}

public sealed class PrototypeBattleUiPartyMemberData
{
    public string MemberId = string.Empty;
    public int SlotIndex;
    public string DisplayName = "None";
    public string RoleLabel = "Adventurer";
    public string SkillLabel = "Basic Action";
    public string SkillShortText = string.Empty;
    public string LevelSummaryText = string.Empty;
    public string LoadoutSummaryText = string.Empty;
    public string ConsumableSlotSummaryText = string.Empty;
    public string ActiveSkillSlotSummaryText = string.Empty;
    public string SkillResourceSummaryText = string.Empty;
    public int CurrentHp;
    public int MaxHp = 1;
    public int Attack;
    public int Defense;
    public int Speed;
    public bool IsActive;
    public bool IsTargeted;
    public bool IsKnockedOut;
    public string StatusText = "Ready";
}

public sealed class PrototypeBattleUiEnemyData
{
    public string MonsterId = string.Empty;
    public string DisplayName = "None";
    public string TypeLabel = "Monster";
    public string RoleLabel = "Bulwark";
    public string IntentLabel = "Unknown";
    public string TraitText = string.Empty;
    public int CurrentHp;
    public int MaxHp = 1;
    public int Attack;
    public bool IsElite;
    public bool IsSelected;
    public bool IsHovered;
    public bool IsActing;
    public bool IsDefeated;
    public string StateLabel = "Alive";
}

public sealed class PrototypeBattleUiCommandSurfaceData
{
    public string SelectedActionKey = string.Empty;
    public string SelectedActionLabel = string.Empty;
    public string SupplementalSummaryText = string.Empty;
    public PrototypeBattleUiCommandDetailData[] Details = Array.Empty<PrototypeBattleUiCommandDetailData>();
}

public sealed class PrototypeBattleUiCommandDetailData
{
    public string Key = string.Empty;
    public string Label = "None";
    public string Description = string.Empty;
    public string TargetText = string.Empty;
    public string CostText = string.Empty;
    public string EffectText = string.Empty;
    public bool IsAvailable = true;
    public bool IsSelected;
}

public sealed class PrototypeBattleUiMessageSurfaceData
{
    public string Prompt = "Select an action.";
    public string CancelHint = "Esc: Cancel";
    public string Feedback = string.Empty;
    public string[] RecentLogs = Array.Empty<string>();
}

public sealed class PrototypeBattleUiTargetSelectionData
{
    public bool IsActive;
    public bool HasFocusedTarget;
    public string Title = "Select target";
    public string QueuedActionLabel = "Action";
    public string TargetLabel = "Choose a target";
    public string TargetRoleLabel = string.Empty;
    public string TargetIntentLabel = string.Empty;
    public string TargetTraitText = string.Empty;
    public string TargetStateText = "No target";
    public int TargetCurrentHp;
    public int TargetMaxHp = 1;
    public string SkillHintText = string.Empty;
    public string CancelHint = "Esc: Cancel";
}
