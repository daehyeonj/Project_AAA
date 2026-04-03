using System;

public sealed class PrototypeRpgProgressionSeedSnapshot
{
    public string ResultStateKey = string.Empty;
    public string DungeonId = string.Empty;
    public string DungeonLabel = string.Empty;
    public string DungeonDangerLabel = string.Empty;
    public string RouteId = string.Empty;
    public string RouteLabel = string.Empty;
    public string RouteRiskLabel = string.Empty;
    public int TotalTurnsTaken;
    public int ClearedEncounterCount;
    public bool EliteDefeated;
    public string EliteName = string.Empty;
    public string EliteTypeLabel = string.Empty;
    public string PartyConditionText = string.Empty;
    public int SurvivingMemberCount;
    public int KnockedOutMemberCount;
    public string EliteClearBonusHint = string.Empty;
    public string RouteRiskHint = string.Empty;
    public string DungeonDangerHint = string.Empty;
    public PrototypeRpgLootSeed Loot = new PrototypeRpgLootSeed();
    public PrototypeRpgMemberProgressionSeed[] Members = Array.Empty<PrototypeRpgMemberProgressionSeed>();
    public string[] RewardTags = Array.Empty<string>();
    public string[] GrowthTags = Array.Empty<string>();
}

public sealed class PrototypeRpgMemberProgressionSeed
{
    public string MemberId = string.Empty;
    public string DisplayName = string.Empty;
    public string RoleTag = string.Empty;
    public string RoleLabel = string.Empty;
    public string DefaultSkillId = string.Empty;
    public bool Survived;
    public bool KnockedOut;
    public int CurrentHp;
    public int MaxHp = 1;
    public PrototypeRpgCombatContributionSeed Combat = new PrototypeRpgCombatContributionSeed();
}

public sealed class PrototypeRpgCombatContributionSeed
{
    public int DamageDealt;
    public int DamageTaken;
    public int HealingDone;
    public int ActionCount;
}

public sealed class PrototypeRpgLootSeed
{
    public int TotalLootGained;
    public int BattleLootGained;
    public int ChestLootGained;
    public int EventLootGained;
    public int EliteRewardAmount;
    public int EliteBonusRewardAmount;
    public string LootBreakdownSummary = string.Empty;
}
public sealed class PrototypeRpgCombatContributionSnapshot
{
    public string ResultStateKey = string.Empty;
    public string DungeonId = string.Empty;
    public string DungeonLabel = string.Empty;
    public string RouteId = string.Empty;
    public string RouteLabel = string.Empty;
    public int TotalTurnsTaken;
    public int SurvivingMemberCount;
    public int KnockedOutMemberCount;
    public int TotalDamageDealt;
    public int TotalDamageTaken;
    public int TotalHealingDone;
    public PrototypeRpgMemberContributionSnapshot[] Members = Array.Empty<PrototypeRpgMemberContributionSnapshot>();
    public PrototypeBattleEventRecord[] RecentEvents = Array.Empty<PrototypeBattleEventRecord>();
}

public sealed class PrototypeRpgMemberContributionSnapshot
{
    public string MemberId = string.Empty;
    public string DisplayName = string.Empty;
    public string RoleTag = string.Empty;
    public string RoleLabel = string.Empty;
    public string DefaultSkillId = string.Empty;
    public int DamageDealt;
    public int DamageTaken;
    public int HealingDone;
    public int ActionCount;
    public int KillCount;
    public bool KnockedOut;
    public bool Survived;
    public bool EliteVictor;
}

public sealed class PrototypeRpgProgressionPreviewSnapshot
{
    public string ResultStateKey = string.Empty;
    public string DungeonId = string.Empty;
    public string DungeonLabel = string.Empty;
    public string DungeonDangerLabel = string.Empty;
    public string RouteId = string.Empty;
    public string RouteLabel = string.Empty;
    public string RouteRiskLabel = string.Empty;
    public bool EliteDefeated;
    public int TotalLootGained;
    public string[] RewardHintTags = Array.Empty<string>();
    public string[] GrowthHintTags = Array.Empty<string>();
    public PrototypeRpgMemberProgressPreview[] Members = Array.Empty<PrototypeRpgMemberProgressPreview>();
}

public sealed class PrototypeRpgMemberProgressPreview
{
    public string MemberId = string.Empty;
    public string DisplayName = string.Empty;
    public string RoleTag = string.Empty;
    public string RoleLabel = string.Empty;
    public bool Survived;
    public PrototypeRpgMemberContributionSnapshot Contribution = new PrototypeRpgMemberContributionSnapshot();
    public string[] SuggestedGrowthHintTags = Array.Empty<string>();
    public string[] SuggestedRewardHintTags = Array.Empty<string>();
    public string NotableOutcomeKey = string.Empty;
}
