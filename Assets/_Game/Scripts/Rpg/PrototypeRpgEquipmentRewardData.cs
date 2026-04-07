using System;

public sealed class PrototypeRpgGearAffixLiteDefinition
{
    public string AffixId = string.Empty;
    public string DisplayLabel = string.Empty;
    public string RoleTag = string.Empty;
    public string SlotKey = string.Empty;
    public string TierKey = string.Empty;
    public string RarityKey = string.Empty;
    public int BonusMaxHp;
    public int BonusAttack;
    public int BonusDefense;
    public int BonusSpeed;
    public string SkillPowerHintText = string.Empty;
    public string HealPowerHintText = string.Empty;
    public string SummaryText = string.Empty;
}

public sealed class PrototypeRpgGearAffixLiteContribution
{
    public string AffixId = string.Empty;
    public string DisplayLabel = string.Empty;
    public int BonusMaxHp;
    public int BonusAttack;
    public int BonusDefense;
    public int BonusSpeed;
    public string HintText = string.Empty;
    public string SummaryText = string.Empty;
}

public static class PrototypeRpgGearAffixLiteCatalog
{
    public static PrototypeRpgGearAffixLiteDefinition GetDefinition(string affixId)
    {
        switch (NormalizeKey(affixId))
        {
            case "flat_max_hp_small":
                return CreateDefinition("flat_max_hp_small", "Stalwart", string.Empty, string.Empty, "tier_1", "uncommon", 2, 0, 0, 0, string.Empty, string.Empty, "Affix HP+2");
            case "flat_attack_small":
                return CreateDefinition("flat_attack_small", "Sharpened", string.Empty, string.Empty, "tier_1", "uncommon", 0, 1, 0, 0, string.Empty, string.Empty, "Affix ATK+1");
            case "flat_defense_small":
                return CreateDefinition("flat_defense_small", "Guarded", string.Empty, string.Empty, "tier_1", "uncommon", 0, 0, 1, 0, string.Empty, string.Empty, "Affix DEF+1");
            case "flat_speed_small":
                return CreateDefinition("flat_speed_small", "Quickstep", string.Empty, string.Empty, "tier_1", "uncommon", 0, 0, 0, 1, string.Empty, string.Empty, "Affix SPD+1");
            case "skill_power_hint_small":
                return CreateDefinition("skill_power_hint_small", "Focused", string.Empty, string.Empty, "tier_2", "rare", 0, 1, 0, 0, "Skill edge", string.Empty, "Affix ATK+1 | Skill edge");
            case "heal_power_hint_small":
                return CreateDefinition("heal_power_hint_small", "Restoring", string.Empty, string.Empty, "tier_2", "rare", 1, 0, 1, 0, string.Empty, "Heal edge", "Affix HP+1 DEF+1 | Heal edge");
            default:
                return null;
        }
    }

    private static PrototypeRpgGearAffixLiteDefinition CreateDefinition(string affixId, string displayLabel, string roleTag, string slotKey, string tierKey, string rarityKey, int bonusMaxHp, int bonusAttack, int bonusDefense, int bonusSpeed, string skillPowerHintText, string healPowerHintText, string summaryText)
    {
        PrototypeRpgGearAffixLiteDefinition definition = new PrototypeRpgGearAffixLiteDefinition();
        definition.AffixId = NormalizeKey(affixId);
        definition.DisplayLabel = string.IsNullOrWhiteSpace(displayLabel) ? "Affix" : displayLabel.Trim();
        definition.RoleTag = NormalizeKey(roleTag);
        definition.SlotKey = NormalizeKey(slotKey);
        definition.TierKey = NormalizeKey(tierKey);
        definition.RarityKey = NormalizeKey(rarityKey);
        definition.BonusMaxHp = bonusMaxHp;
        definition.BonusAttack = bonusAttack;
        definition.BonusDefense = bonusDefense;
        definition.BonusSpeed = bonusSpeed;
        definition.SkillPowerHintText = string.IsNullOrWhiteSpace(skillPowerHintText) ? string.Empty : skillPowerHintText.Trim();
        definition.HealPowerHintText = string.IsNullOrWhiteSpace(healPowerHintText) ? string.Empty : healPowerHintText.Trim();
        definition.SummaryText = string.IsNullOrWhiteSpace(summaryText) ? string.Empty : summaryText.Trim();
        return definition;
    }

    private static string NormalizeKey(string value)
    {
        return string.IsNullOrWhiteSpace(value) ? string.Empty : value.Trim().ToLowerInvariant();
    }
}

public sealed class PrototypeRpgGearDefinition
{
    public string GearDefinitionId = string.Empty;
    public string EquipmentLoadoutId = string.Empty;
    public string SlotKey = string.Empty;
    public string TierKey = string.Empty;
    public string RarityKey = string.Empty;
    public string UnlockStateKey = string.Empty;
    public string GroupKey = string.Empty;
    public string DisplayLabel = string.Empty;
    public string EffectPreviewText = string.Empty;
    public string ComparisonHintText = string.Empty;
}

public sealed class PrototypeRpgGearSlotRule
{
    public string SlotKey = string.Empty;
    public string SlotLabel = string.Empty;
    public string CurrentEquipmentLoadoutId = string.Empty;
    public string CurrentEquipmentDisplayLabel = string.Empty;
    public string CandidateEquipmentLoadoutId = string.Empty;
    public string CandidateEquipmentDisplayLabel = string.Empty;
    public string AllowedTierKey = string.Empty;
    public string AllowedRarityKey = string.Empty;
    public string ConflictSummaryText = string.Empty;
    public string ReplacementSummaryText = string.Empty;
    public string SummaryText = string.Empty;
}

public sealed class PrototypeRpgGearUnlockState
{
    public string PartyId = string.Empty;
    public string LastRunIdentity = string.Empty;
    public string HighestUnlockedTierKey = string.Empty;
    public string HighestUnlockedRarityKey = string.Empty;
    public string HighestUnlockedLevelBandKey = string.Empty;
    public string UnlockSummaryText = string.Empty;
    public string UnlockedPoolSummaryText = string.Empty;
    public string[] UnlockedTierKeys = Array.Empty<string>();
    public string[] UnlockedGearDefinitionIds = Array.Empty<string>();
    public string[] RecentlySeenGearGroupKeys = Array.Empty<string>();
}

public sealed class PrototypeRpgGearComparisonSummary
{
    public string MemberId = string.Empty;
    public string MemberDisplayName = string.Empty;
    public string CurrentEquipmentLoadoutId = string.Empty;
    public string CurrentEquipmentDisplayLabel = string.Empty;
    public string CandidateEquipmentLoadoutId = string.Empty;
    public string CandidateEquipmentDisplayLabel = string.Empty;
    public string SlotKey = string.Empty;
    public string TierKey = string.Empty;
    public string RarityKey = string.Empty;
    public string StatDeltaPreviewText = string.Empty;
    public string SkillHintDeltaText = string.Empty;
    public string SwapReasonText = string.Empty;
    public string AffixLiteSummaryText = string.Empty;
    public string ComparisonHintText = string.Empty;
    public string SummaryText = string.Empty;
}

public sealed class PrototypeRpgEquipmentRewardDefinition
{
    public string RewardId = string.Empty;
    public string EquipmentLoadoutId = string.Empty;
    public string GearDefinitionId = string.Empty;
    public string SlotKey = string.Empty;
    public string TierKey = string.Empty;
    public string RarityKey = string.Empty;
    public string UnlockStateKey = string.Empty;
    public string GearGroupKey = string.Empty;
    public string DisplayLabel = string.Empty;
    public string EffectPreviewText = string.Empty;
    public string CandidateSourceKey = string.Empty;
}

public sealed class PrototypeRpgEquipmentRewardCandidate
{
    public string RewardId = string.Empty;
    public string MemberId = string.Empty;
    public string MemberDisplayName = string.Empty;
    public string EquipmentLoadoutId = string.Empty;
    public string CurrentEquipmentLoadoutId = string.Empty;
    public string CurrentEquipmentDisplayLabel = string.Empty;
    public string DisplayLabel = string.Empty;
    public string SlotKey = string.Empty;
    public string TierKey = string.Empty;
    public string RarityKey = string.Empty;
    public string UnlockStateKey = string.Empty;
    public string GearGroupKey = string.Empty;
    public string CandidateSourceKey = string.Empty;
    public string EffectPreviewText = string.Empty;
    public string DerivedStatDeltaSummaryText = string.Empty;
    public string SkillHintText = string.Empty;
    public string RoleHintText = string.Empty;
    public string AffixLiteSummaryText = string.Empty;
    public string LevelBandSummaryText = string.Empty;
    public string UnlockedPoolSummaryText = string.Empty;
    public string ComparisonSummaryText = string.Empty;
    public string RuleSummaryText = string.Empty;
    public string UnlockSummaryText = string.Empty;
    public string SummaryText = string.Empty;
    public bool IsRecommended;
}

public sealed class PrototypeRpgEquipSwapChoice
{
    public string RewardId = string.Empty;
    public string MemberId = string.Empty;
    public string MemberDisplayName = string.Empty;
    public string CurrentEquipmentLoadoutId = string.Empty;
    public string CurrentEquipmentDisplayLabel = string.Empty;
    public string TargetEquipmentLoadoutId = string.Empty;
    public string TargetEquipmentDisplayLabel = string.Empty;
    public string SlotKey = string.Empty;
    public string ReplacementSummaryText = string.Empty;
    public string ComparisonSummaryText = string.Empty;
    public string AffixLiteSummaryText = string.Empty;
    public string EffectPreviewText = string.Empty;
    public string ContinuitySummaryText = string.Empty;
    public string SummaryText = string.Empty;
    public bool ApplyReady;
    public bool HasChanges;
}

public sealed class PrototypeRpgRuntimeGearInventoryState
{
    public string PartyId = string.Empty;
    public string DisplayName = string.Empty;
    public string LastRunIdentity = string.Empty;
    public string LastAppliedRewardId = string.Empty;
    public string RewardCandidateSummaryText = string.Empty;
    public string EquipSwapSummaryText = string.Empty;
    public string GearCarryContinuitySummaryText = string.Empty;
    public string GearRuleSummaryText = string.Empty;
    public string GearUnlockSummaryText = string.Empty;
    public string GearComparisonSummaryText = string.Empty;
    public string GearAffixLiteSummaryText = string.Empty;
    public string UnlockedGearPoolSummaryText = string.Empty;
    public string LevelBandGearLinkageSummaryText = string.Empty;
    public string ActiveEquippedSummaryText = string.Empty;
    public string HudSummaryText = string.Empty;
    public PrototypeRpgGearDefinition[] GearDefinitions = Array.Empty<PrototypeRpgGearDefinition>();
    public PrototypeRpgGearSlotRule[] SlotRules = Array.Empty<PrototypeRpgGearSlotRule>();
    public PrototypeRpgGearUnlockState UnlockState = new PrototypeRpgGearUnlockState();
    public PrototypeRpgGearComparisonSummary[] ComparisonSummaries = Array.Empty<PrototypeRpgGearComparisonSummary>();
    public PrototypeRpgGearAffixLiteDefinition[] AffixDefinitions = Array.Empty<PrototypeRpgGearAffixLiteDefinition>();
    public PrototypeRpgGearAffixLiteContribution[] AffixContributions = Array.Empty<PrototypeRpgGearAffixLiteContribution>();
    public PrototypeRpgEquipmentRewardDefinition[] RewardDefinitions = Array.Empty<PrototypeRpgEquipmentRewardDefinition>();
    public PrototypeRpgEquipmentRewardCandidate[] RewardCandidates = Array.Empty<PrototypeRpgEquipmentRewardCandidate>();
    public PrototypeRpgEquipSwapChoice[] EquipSwapChoices = Array.Empty<PrototypeRpgEquipSwapChoice>();
}
