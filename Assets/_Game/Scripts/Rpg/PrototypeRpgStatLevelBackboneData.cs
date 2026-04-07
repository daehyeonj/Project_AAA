using System;

public sealed class PrototypeRpgExperienceProgress
{
    public int Level = 1;
    public int CurrentExperience;
    public int ExperienceToNextLevel = 1;
    public int LifetimeExperience;
    public int GainedExperienceThisRun;
    public int LastGrantedExperience;
    public int PreviousLevel = 1;
    public int AppliedLevelThisSession = 1;
    public bool LeveledUpThisRun;
    public string LastAppliedRunIdentity = string.Empty;
    public string SummaryText = string.Empty;
}

public sealed class PrototypeRpgStatModifierBundle
{
    public int BonusMaxHp;
    public int BonusAttack;
    public int BonusDefense;
    public int BonusSpeed;
    public int BonusSkillPower;
    public int EquipmentMaxHpDelta;
    public int EquipmentAttackDelta;
    public int EquipmentDefenseDelta;
    public int EquipmentSpeedDelta;
    public string EquipmentLoadoutId = string.Empty;
    public string EquipmentDisplayName = string.Empty;
    public string EquipmentSlotKey = string.Empty;
    public string JobSpecializationKey = string.Empty;
    public string JobSpecializationSummaryText = string.Empty;
    public string PassiveSkillId = string.Empty;
    public string PassiveSkillSlotSummaryText = string.Empty;
    public string RuntimeSynergySummaryText = string.Empty;
    public string EquipmentSummaryText = string.Empty;
    public string GearContributionSummaryText = string.Empty;
    public string GearAffixLiteSummaryText = string.Empty;
    public string LevelBandSummaryText = string.Empty;
    public string PassiveHintText = string.Empty;
    public string BattleLabelHint = string.Empty;
    public string SummaryText = string.Empty;
}

public sealed class PrototypeRpgDerivedStatSummary
{
    public int Level = 1;
    public int MaxHp = 1;
    public int Attack = 1;
    public int Defense;
    public int Speed;
    public int PowerScore;
    public int SkillPowerBonus;
    public string JobSpecializationKey = string.Empty;
    public string JobSpecializationSummaryText = string.Empty;
    public string PassiveSkillId = string.Empty;
    public string PassiveSkillSlotSummaryText = string.Empty;
    public string RuntimeSynergySummaryText = string.Empty;
    public string EquipmentSummaryText = string.Empty;
    public string GearContributionSummaryText = string.Empty;
    public string GearAffixLiteSummaryText = string.Empty;
    public string LevelBandSummaryText = string.Empty;
    public string PassiveHintText = string.Empty;
    public string BattleLabelHint = string.Empty;
    public string SummaryText = string.Empty;
}

public sealed class PrototypeRpgLevelUpPreview
{
    public string MemberId = string.Empty;
    public string DisplayName = string.Empty;
    public int PreviousLevel = 1;
    public int NewLevel = 1;
    public int PendingLevelUps;
    public int GainedExperienceThisRun;
    public bool LeveledUpThisRun;
    public int ProjectedMaxHpDelta;
    public int ProjectedAttackDelta;
    public int ProjectedDefenseDelta;
    public int ProjectedSpeedDelta;
    public string DerivedStatSummaryText = string.Empty;
    public string NextRunStatProjectionSummaryText = string.Empty;
    public string ApplyReadySummaryText = string.Empty;
    public string ActualApplySummaryText = string.Empty;
    public string SummaryText = string.Empty;
}

public sealed class PrototypeRpgPartyMemberLevelRuntimeState
{
    public string MemberId = string.Empty;
    public string DisplayName = string.Empty;
    public string GrowthTrackId = string.Empty;
    public string JobId = string.Empty;
    public string EquipmentLoadoutId = string.Empty;
    public string SkillLoadoutId = string.Empty;
    public int LastGrantedExperience;
    public int AppliedLevelThisSession = 1;
    public string LastAppliedRunIdentity = string.Empty;
    public string ActualLevelApplySummaryText = string.Empty;
    public string DerivedStatHydrateSummaryText = string.Empty;
    public string JobSpecializationKey = string.Empty;
    public string JobSpecializationSummaryText = string.Empty;
    public string PassiveSkillId = string.Empty;
    public string PassiveSkillSlotSummaryText = string.Empty;
    public string RuntimeSynergySummaryText = string.Empty;
    public string NextRunSpecializationPreviewText = string.Empty;
    public string PendingUnlockHintText = string.Empty;
    public int RuntimeSkillPowerBonus;
    public string EquipmentSummaryText = string.Empty;
    public string GearContributionSummaryText = string.Empty;
    public string GearAffixLiteSummaryText = string.Empty;
    public string LevelBandSummaryText = string.Empty;
    public string PassiveHintText = string.Empty;
    public string BattleLabelHint = string.Empty;
    public PrototypeRpgExperienceProgress Experience = new PrototypeRpgExperienceProgress();
    public PrototypeRpgStatModifierBundle Modifiers = new PrototypeRpgStatModifierBundle();
    public PrototypeRpgDerivedStatSummary DerivedStats = new PrototypeRpgDerivedStatSummary();
    public PrototypeRpgLevelUpPreview LevelUpPreview = new PrototypeRpgLevelUpPreview();
    public string LevelSummaryText = string.Empty;
}

public sealed class PrototypeRpgPartyLevelRuntimeState
{
    public string PartyId = string.Empty;
    public string DisplayName = string.Empty;
    public string LastRunIdentity = string.Empty;
    public string AppliedLastRunIdentity = string.Empty;
    public int LastGrantedExperience;
    public int PendingLevelUpCount;
    public int AppliedMemberCount;
    public string SummaryText = string.Empty;
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
    public string EquipmentLoadoutReadbackSummaryText = string.Empty;
    public string GearContributionSummaryText = string.Empty;
    public string GearAffixLiteSummaryText = string.Empty;
    public string LevelBandGearLinkageSummaryText = string.Empty;
    public PrototypeRpgPartyMemberLevelRuntimeState[] Members = Array.Empty<PrototypeRpgPartyMemberLevelRuntimeState>();
}
