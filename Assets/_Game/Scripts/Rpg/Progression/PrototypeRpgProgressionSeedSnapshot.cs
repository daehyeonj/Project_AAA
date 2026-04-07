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
    public string ContributionSummaryText = string.Empty;
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
    public string ContributionSummaryText = string.Empty;
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
    public string ContributionSummaryText = string.Empty;
    public string PreviewSummaryText = string.Empty;
    public string PendingRewardDeltaSummaryText = string.Empty;
    public string RewardCarryoverSummaryText = string.Empty;
    public string ExperienceGainSummaryText = string.Empty;
    public string LevelUpPreviewSummaryText = string.Empty;
    public string LevelUpApplyReadySummaryText = string.Empty;
    public string ActualLevelApplySummaryText = string.Empty;
    public string DerivedStatHydrateSummaryText = string.Empty;
    public string NextRunStatProjectionSummaryText = string.Empty;
    public string JobSpecializationSummaryText = string.Empty;
    public string PassiveSkillSlotSummaryText = string.Empty;
    public string RuntimeSynergySummaryText = string.Empty;
    public string NextRunSpecializationPreviewText = string.Empty;
    public string ActiveSkillSlotSummaryText = string.Empty;
    public string SkillCooldownSummaryText = string.Empty;
    public string SkillResourceSummaryText = string.Empty;
    public string ActionEconomySummaryText = string.Empty;
    public string StatusEffectSummaryText = string.Empty;
    public string NotableStatusUsageSummaryText = string.Empty;
    public string EnemyIntentSummaryText = string.Empty;
    public string EnemySkillUsageSummaryText = string.Empty;
    public string EnemyActionEconomySummaryText = string.Empty;
    public string EncounterPhaseSummaryText = string.Empty;
    public string EncounterWaveSummaryText = string.Empty;
    public string BossPatternSummaryText = string.Empty;
    public string NextRunActiveSkillPreviewText = string.Empty;
    public string SkillLoadoutReadbackSummaryText = string.Empty;
    public string EquipmentLoadoutReadbackSummaryText = string.Empty;
    public string GearContributionSummaryText = string.Empty;
    public string ConsumableCarrySummaryText = string.Empty;
    public string ConsumableCarryoverPolicySummaryText = string.Empty;
    public string GearRewardCandidateSummaryText = string.Empty;
    public string EquipSwapChoiceSummaryText = string.Empty;
    public string GearCarryContinuitySummaryText = string.Empty;
    public string GearRuleSummaryText = string.Empty;
    public string GearUnlockSummaryText = string.Empty;
    public string GearComparisonSummaryText = string.Empty;
    public string GearAffixLiteSummaryText = string.Empty;
    public string UnlockedGearPoolSummaryText = string.Empty;
    public string LevelBandGearLinkageSummaryText = string.Empty;
    public string ApplyTraceSummaryText = string.Empty;
    public string NextOfferBasisSummaryText = string.Empty;
    public string CurrentAppliedIdentitySummaryText = string.Empty;
    public string OfferRefreshSummaryText = string.Empty;
    public string ApplyReadySummaryText = string.Empty;
    public string OfferContinuitySummaryText = string.Empty;
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
    public string ContributionSummaryText = string.Empty;
    public string PreviewSummaryText = string.Empty;
    public string LevelSummaryText = string.Empty;
    public string LevelUpPreviewText = string.Empty;
    public string JobSpecializationSummaryText = string.Empty;
    public string PassiveSkillSlotSummaryText = string.Empty;
    public string RuntimeSynergySummaryText = string.Empty;
    public string ActiveSkillSlotSummaryText = string.Empty;
    public string SkillCooldownSummaryText = string.Empty;
    public string SkillResourceSummaryText = string.Empty;
    public string ActionEconomySummaryText = string.Empty;
    public string StatusEffectSummaryText = string.Empty;
    public string DerivedStatSummaryText = string.Empty;
    public string EquipmentSummaryText = string.Empty;
    public string GearContributionSummaryText = string.Empty;
    public string SkillLoadoutSummaryText = string.Empty;
    public string ConsumableSlotSummaryText = string.Empty;
    public string CurrentAppliedIdentitySummaryText = string.Empty;
    public string NextOfferSummaryText = string.Empty;
    public string ApplyReadySummaryText = string.Empty;
    public string OfferReasonText = string.Empty;
    public string OfferContinuitySummaryText = string.Empty;
    public string[] SuggestedGrowthHintTags = Array.Empty<string>();
    public string[] SuggestedRewardHintTags = Array.Empty<string>();
    public string NotableOutcomeKey = string.Empty;
}
