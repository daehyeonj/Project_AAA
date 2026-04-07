using System;

public sealed class PrototypeRpgRunResultSnapshot
{
    public string ResultStateKey = string.Empty;
    public string DungeonId = string.Empty;
    public string DungeonLabel = string.Empty;
    public string RouteId = string.Empty;
    public string RouteLabel = string.Empty;
    public int TotalTurnsTaken;
    public string ResultSummary = string.Empty;
    public string SurvivingMembersSummary = string.Empty;
    public string PostRunHeadlineText = string.Empty;
    public string PostRunSubheadlineText = string.Empty;
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
    public PrototypeRpgPartyOutcomeSnapshot PartyOutcome = new PrototypeRpgPartyOutcomeSnapshot();
    public PrototypeRpgLootOutcomeSnapshot LootOutcome = new PrototypeRpgLootOutcomeSnapshot();
    public PrototypeRpgEliteOutcomeSnapshot EliteOutcome = new PrototypeRpgEliteOutcomeSnapshot();
    public PrototypeRpgEncounterOutcomeSnapshot EncounterOutcome = new PrototypeRpgEncounterOutcomeSnapshot();
}

public sealed class PrototypeRpgPartyOutcomeSnapshot
{
    public string PartyConditionText = string.Empty;
    public string PartyHpSummaryText = string.Empty;
    public string PartyMembersAtEndSummary = string.Empty;
    public string ContributionSummaryText = string.Empty;
    public string StatusEffectSummaryText = string.Empty;
    public int SurvivingMemberCount;
    public int KnockedOutMemberCount;
    public PrototypeRpgPartyMemberOutcomeSnapshot[] Members = Array.Empty<PrototypeRpgPartyMemberOutcomeSnapshot>();
}

public sealed class PrototypeRpgPartyMemberOutcomeSnapshot
{
    public string MemberId = string.Empty;
    public string DisplayName = string.Empty;
    public string RoleTag = string.Empty;
    public string RoleLabel = string.Empty;
    public string DefaultSkillId = string.Empty;
    public int CurrentHp;
    public int MaxHp = 1;
    public bool Survived;
    public bool KnockedOut;
    public int Level = 1;
    public int CurrentExperience;
    public int ExperienceToNextLevel = 1;
    public int GainedExperienceThisRun;
    public bool LeveledUpThisRun;
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
}

public sealed class PrototypeRpgLootOutcomeSnapshot
{
    public int TotalLootGained;
    public int BattleLootGained;
    public int ChestLootGained;
    public int EventLootGained;
    public int EliteRewardAmount;
    public int EliteBonusRewardAmount;
    public int PendingBonusRewardLostAmount;
    public string PendingRewardDeltaSummaryText = string.Empty;
    public string RewardCarryoverSummaryText = string.Empty;
    public string RewardGrantSummaryText = string.Empty;
    public string FinalLootSummary = string.Empty;
}

public sealed class PrototypeRpgEliteOutcomeSnapshot
{
    public bool IsEliteDefeated;
    public string EliteName = string.Empty;
    public string EliteTypeLabel = string.Empty;
    public string EliteRewardLabel = string.Empty;
    public int EliteRewardAmount;
    public bool EliteBonusRewardEarned;
    public int EliteBonusRewardAmount;
}

public sealed class PrototypeRpgEncounterOutcomeSnapshot
{
    public int ClearedEncounterCount;
    public string ClearedEncounterSummary = string.Empty;
    public int OpenedChestCount;
    public string RoomPathSummary = string.Empty;
    public string PhaseReachedSummary = string.Empty;
    public string WaveSummaryText = string.Empty;
    public string BossPatternSummaryText = string.Empty;
    public string SelectedEventChoice = string.Empty;
    public string SelectedPreEliteChoice = string.Empty;
    public int PreEliteHealAmount;
}
