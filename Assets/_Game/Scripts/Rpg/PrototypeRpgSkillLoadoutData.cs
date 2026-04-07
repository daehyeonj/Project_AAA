using System;

public static class PrototypeRpgSkillSegmentKeys
{
    public const string Frontline = "frontline";
    public const string Precision = "precision";
    public const string Arcane = "arcane";
    public const string Support = "support";
    public const string Utility = "utility";
}

public sealed class PrototypeRpgSkillEffectEntry
{
    public string EffectKey { get; }
    public string EffectType { get; }
    public string TargetKind { get; }
    public int PowerValue { get; }
    public string ApplyModeKey { get; }
    public string UseFlowKey { get; }
    public string ContributionHintKey { get; }
    public string SummaryText { get; }

    public PrototypeRpgSkillEffectEntry(string effectKey, string effectType, string targetKind, int powerValue, string summaryText, string applyModeKey = "direct_damage", string useFlowKey = "standard", string contributionHintKey = "offense")
    {
        EffectKey = NormalizeKey(effectKey);
        EffectType = NormalizeKey(effectType);
        TargetKind = NormalizeKey(targetKind);
        PowerValue = powerValue > 0 ? powerValue : 1;
        ApplyModeKey = NormalizeKey(applyModeKey);
        UseFlowKey = NormalizeKey(useFlowKey);
        ContributionHintKey = NormalizeKey(contributionHintKey);
        SummaryText = string.IsNullOrWhiteSpace(summaryText) ? string.Empty : summaryText.Trim();
    }

    private static string NormalizeKey(string value)
    {
        return string.IsNullOrWhiteSpace(value) ? string.Empty : value.Trim().ToLowerInvariant();
    }
}

public sealed class PrototypeRpgSkillUsageProfile
{
    public string ProfileId { get; }
    public string RoleTag { get; }
    public string SegmentKey { get; }
    public string TempoKey { get; }
    public string SummaryText { get; }

    public PrototypeRpgSkillUsageProfile(string profileId, string roleTag, string segmentKey, string tempoKey, string summaryText)
    {
        ProfileId = NormalizeKey(profileId);
        RoleTag = NormalizeKey(roleTag);
        SegmentKey = NormalizeKey(segmentKey);
        TempoKey = NormalizeKey(tempoKey);
        SummaryText = string.IsNullOrWhiteSpace(summaryText) ? string.Empty : summaryText.Trim();
    }

    private static string NormalizeKey(string value)
    {
        return string.IsNullOrWhiteSpace(value) ? string.Empty : value.Trim().ToLowerInvariant();
    }
}

public sealed class PrototypeRpgRuntimeSkillSlotData
{
    public string SlotKey { get; }
    public string SkillId { get; }
    public string SkillLabel { get; }
    public string SegmentKey { get; }
    public string EffectBucketKey { get; }
    public string TargetKind { get; }
    public string EffectType { get; }
    public string ApplyModeKey { get; }
    public string UseFlowKey { get; }
    public string ContributionHintKey { get; }
    public int PowerValue { get; }
    public int CooldownTurns { get; }
    public int MaxEncounterCharges { get; }
    public int ResourceCost { get; }
    public int ActionPointCost { get; }
    public int RecommendedLevel { get; }
    public string SummaryText { get; }

    public PrototypeRpgRuntimeSkillSlotData(string slotKey, string skillId, string skillLabel, string segmentKey, string effectBucketKey, string targetKind, string effectType, string applyModeKey, string useFlowKey, string contributionHintKey, int powerValue, int cooldownTurns, int maxEncounterCharges, int resourceCost, int actionPointCost, int recommendedLevel, string summaryText)
    {
        SlotKey = NormalizeKey(slotKey);
        SkillId = NormalizeKey(skillId);
        SkillLabel = string.IsNullOrWhiteSpace(skillLabel) ? "Skill" : skillLabel.Trim();
        SegmentKey = NormalizeKey(segmentKey);
        EffectBucketKey = NormalizeKey(effectBucketKey);
        TargetKind = NormalizeKey(targetKind);
        EffectType = NormalizeKey(effectType);
        ApplyModeKey = NormalizeKey(applyModeKey);
        UseFlowKey = NormalizeKey(useFlowKey);
        ContributionHintKey = NormalizeKey(contributionHintKey);
        PowerValue = powerValue > 0 ? powerValue : 1;
        CooldownTurns = cooldownTurns >= 0 ? cooldownTurns : 0;
        MaxEncounterCharges = maxEncounterCharges >= 0 ? maxEncounterCharges : 0;
        ResourceCost = resourceCost >= 0 ? resourceCost : 0;
        ActionPointCost = actionPointCost > 0 ? actionPointCost : 1;
        RecommendedLevel = recommendedLevel > 0 ? recommendedLevel : 1;
        SummaryText = string.IsNullOrWhiteSpace(summaryText) ? string.Empty : summaryText.Trim();
    }

    private static string NormalizeKey(string value)
    {
        return string.IsNullOrWhiteSpace(value) ? string.Empty : value.Trim().ToLowerInvariant();
    }
}

public sealed class PrototypeRpgActiveSkillSlotState
{
    public string SlotKey = string.Empty;
    public string SkillId = string.Empty;
    public string SkillLabel = string.Empty;
    public string SegmentKey = string.Empty;
    public string EffectBucketKey = string.Empty;
    public string TargetKind = string.Empty;
    public string EffectType = string.Empty;
    public string ApplyModeKey = string.Empty;
    public string UseFlowKey = string.Empty;
    public string ContributionHintKey = string.Empty;
    public int PowerValue;
    public int CooldownTurns;
    public int CooldownRemaining;
    public int MaxEncounterCharges;
    public int EncounterChargesRemaining;
    public int ResourceCost;
    public int ActionPointCost = 1;
    public int RecommendedLevel = 1;
    public bool IsAvailable;
    public string AvailabilityStateKey = string.Empty;
    public string AvailabilitySummaryText = string.Empty;
    public string SummaryText = string.Empty;
}

public sealed class PrototypeRpgResolvedSkillEffect
{
    public string SlotKey = string.Empty;
    public string SkillId = string.Empty;
    public string SkillLabel = string.Empty;
    public string SegmentKey = string.Empty;
    public string EffectBucketKey = string.Empty;
    public string TargetKind = string.Empty;
    public string EffectType = string.Empty;
    public string ApplyModeKey = string.Empty;
    public string UseFlowKey = string.Empty;
    public string ContributionHintKey = string.Empty;
    public int PowerValue;
    public string SummaryText = string.Empty;
}

public sealed class PrototypeRpgResolvedSkillLoadout
{
    public string MemberId = string.Empty;
    public string RoleTag = string.Empty;
    public string LoadoutId = string.Empty;
    public string LoadoutLabel = string.Empty;
    public string SegmentKey = string.Empty;
    public string PrimarySkillId = string.Empty;
    public string PrimarySkillLabel = string.Empty;
    public string SummaryText = string.Empty;
    public string ActiveSkillSlotSummaryText = string.Empty;
    public string CooldownSummaryText = string.Empty;
    public string ResourceSummaryText = string.Empty;
    public string ActionEconomySummaryText = string.Empty;
    public PrototypeRpgRuntimeSkillSlotData[] SkillSlots = Array.Empty<PrototypeRpgRuntimeSkillSlotData>();
}

public sealed class PrototypeRpgSkillUseContext
{
    public string MemberId = string.Empty;
    public string LoadoutId = string.Empty;
    public string SelectedActionKey = string.Empty;
    public int ResolvedSlotIndex = -1;
    public string ResolvedSlotKey = string.Empty;
    public string ResolvedSkillId = string.Empty;
    public string ResolvedSkillLabel = string.Empty;
    public string SkillSegmentKey = string.Empty;
    public string TargetKind = string.Empty;
    public string EffectBucketKey = string.Empty;
    public string EffectType = string.Empty;
    public string ApplyModeKey = string.Empty;
    public string UseFlowKey = string.Empty;
    public string ContributionHintKey = string.Empty;
    public int PowerValue;
    public int CooldownTurns;
    public int CooldownRemaining;
    public int MaxEncounterCharges;
    public int EncounterChargesRemaining;
    public int ResourceCost;
    public int CurrentResource;
    public int MaxResource = 1;
    public int ActionPointCost = 1;
    public bool IsAvailable;
    public string AvailabilityStateKey = string.Empty;
    public string AvailabilitySummaryText = string.Empty;
    public string SummaryText = string.Empty;
    public string EffectSummaryText = string.Empty;
    public string ActiveSkillSlotSummaryText = string.Empty;
    public string CooldownSummaryText = string.Empty;
    public string ResourceSummaryText = string.Empty;
    public string ActionEconomySummaryText = string.Empty;
    public string NextRunPreviewText = string.Empty;
}

public sealed class PrototypeRpgSkillLoadoutDefinition
{
    public string LoadoutId { get; }
    public string DisplayName { get; }
    public string RoleTag { get; }
    public string SegmentKey { get; }
    public string PrimarySkillId { get; }
    public string SecondarySkillId { get; }
    public string FinisherSkillId { get; }
    public string SummaryText { get; }
    public string[] SkillIds { get; }
    public PrototypeRpgSkillUsageProfile UsageProfile { get; }
    public PrototypeRpgSkillEffectEntry[] EffectEntries { get; }

    public PrototypeRpgSkillLoadoutDefinition(string loadoutId, string displayName, string roleTag, string segmentKey, string primarySkillId, string secondarySkillId, string finisherSkillId, string summaryText, string[] skillIds, PrototypeRpgSkillUsageProfile usageProfile, PrototypeRpgSkillEffectEntry[] effectEntries)
    {
        LoadoutId = NormalizeKey(loadoutId);
        DisplayName = string.IsNullOrWhiteSpace(displayName) ? "Loadout" : displayName.Trim();
        RoleTag = NormalizeKey(roleTag);
        SegmentKey = NormalizeKey(segmentKey);
        PrimarySkillId = NormalizeKey(primarySkillId);
        SecondarySkillId = NormalizeKey(secondarySkillId);
        FinisherSkillId = NormalizeKey(finisherSkillId);
        SummaryText = string.IsNullOrWhiteSpace(summaryText) ? string.Empty : summaryText.Trim();
        SkillIds = NormalizeSkills(skillIds);
        UsageProfile = usageProfile ?? new PrototypeRpgSkillUsageProfile(string.Empty, roleTag, segmentKey, "steady", string.Empty);
        EffectEntries = effectEntries ?? Array.Empty<PrototypeRpgSkillEffectEntry>();
    }

    private static string[] NormalizeSkills(string[] skillIds)
    {
        if (skillIds == null || skillIds.Length <= 0)
        {
            return Array.Empty<string>();
        }

        string[] normalized = new string[skillIds.Length];
        int count = 0;
        for (int i = 0; i < skillIds.Length; i++)
        {
            string skillId = NormalizeKey(skillIds[i]);
            if (string.IsNullOrEmpty(skillId))
            {
                continue;
            }

            normalized[count++] = skillId;
        }

        if (count <= 0)
        {
            return Array.Empty<string>();
        }

        if (count == normalized.Length)
        {
            return normalized;
        }

        string[] trimmed = new string[count];
        Array.Copy(normalized, trimmed, count);
        return trimmed;
    }

    private static string NormalizeKey(string value)
    {
        return string.IsNullOrWhiteSpace(value) ? string.Empty : value.Trim().ToLowerInvariant();
    }
}

public static class PrototypeRpgSkillLoadoutCatalog
{
    private static readonly PrototypeRpgSkillLoadoutDefinition[] SharedDefinitions =
    {
        new PrototypeRpgSkillLoadoutDefinition("skillloadout_warrior_placeholder", "Vanguard Burst", "warrior", PrototypeRpgSkillSegmentKeys.Frontline, "skill_power_strike", "skill_guarding_stance", "skill_power_strike", "Burst opener with a guard fallback.", new[] { "skill_power_strike", "skill_guarding_stance" }, new PrototypeRpgSkillUsageProfile("usage_warrior_vanguard", "warrior", PrototypeRpgSkillSegmentKeys.Frontline, "steady", "Frontline pressure with a guard reset."), new[] { new PrototypeRpgSkillEffectEntry("warrior_burst", "damage", "single_enemy", 10, "Heavy frontline strike.", "direct_damage", "burst", "offense"), new PrototypeRpgSkillEffectEntry("warrior_guard", "guard", "self", 4, "Guarded recovery stance.", "self_recover_or_guarded_strike", "recovery", "survival") }),
        new PrototypeRpgSkillLoadoutDefinition("skillloadout_rogue_placeholder", "Precision Chain", "rogue", PrototypeRpgSkillSegmentKeys.Precision, "skill_weak_point", "skill_exploit_mark", "skill_weak_point", "Finisher-first chain with a setup mark.", new[] { "skill_weak_point", "skill_exploit_mark" }, new PrototypeRpgSkillUsageProfile("usage_rogue_precision", "rogue", PrototypeRpgSkillSegmentKeys.Precision, "sharp", "Precision setup into a finisher."), new[] { new PrototypeRpgSkillEffectEntry("rogue_finish", "finisher_damage", "single_enemy", 7, "Precision finisher.", "finisher_damage", "execution", "offense"), new PrototypeRpgSkillEffectEntry("rogue_mark", "setup", "single_enemy", 4, "Weak-point mark.", "direct_damage", "setup", "pressure") }),
        new PrototypeRpgSkillLoadoutDefinition("skillloadout_mage_placeholder", "Arcane Volley", "mage", PrototypeRpgSkillSegmentKeys.Arcane, "skill_arcane_burst", "skill_focus_surge", "skill_arcane_burst", "AOE release backed by a charge window.", new[] { "skill_arcane_burst", "skill_focus_surge" }, new PrototypeRpgSkillUsageProfile("usage_mage_volley", "mage", PrototypeRpgSkillSegmentKeys.Arcane, "cycle", "Charge, then release arcane pressure."), new[] { new PrototypeRpgSkillEffectEntry("mage_burst", "damage", "all_enemies", 6, "Arcane volley across all enemies.", "splash_damage_or_all_enemies", "burst", "aoe"), new PrototypeRpgSkillEffectEntry("mage_charge", "amplify", "self", 4, "Focus surge.", "self_recover_or_guarded_strike", "setup", "tempo") }),
        new PrototypeRpgSkillLoadoutDefinition("skillloadout_cleric_placeholder", "Sanctuary Chorus", "cleric", PrototypeRpgSkillSegmentKeys.Support, "skill_radiant_hymn", "skill_restoration_ward", "skill_radiant_hymn", "Party sustain with a protection ward.", new[] { "skill_radiant_hymn", "skill_restoration_ward" }, new PrototypeRpgSkillUsageProfile("usage_cleric_sanctuary", "cleric", PrototypeRpgSkillSegmentKeys.Support, "steady", "Group sustain with a safety ward."), new[] { new PrototypeRpgSkillEffectEntry("cleric_heal", "heal", "all_allies", 5, "Group healing pulse.", "party_heal", "sustain", "support"), new PrototypeRpgSkillEffectEntry("cleric_ward", "protect_heal", "all_allies", 4, "Protection ward.", "party_heal", "guard", "support") })
    };

    public static PrototypeRpgSkillLoadoutDefinition GetDefinition(string loadoutId)
    {
        string normalizedLoadoutId = NormalizeKey(loadoutId);
        if (string.IsNullOrEmpty(normalizedLoadoutId))
        {
            return null;
        }

        for (int i = 0; i < SharedDefinitions.Length; i++)
        {
            PrototypeRpgSkillLoadoutDefinition definition = SharedDefinitions[i];
            if (definition != null && definition.LoadoutId == normalizedLoadoutId)
            {
                return definition;
            }
        }

        return null;
    }

    public static PrototypeRpgSkillLoadoutDefinition GetFallbackDefinitionForRoleTag(string roleTag)
    {
        switch (NormalizeKey(roleTag))
        {
            case "warrior": return GetDefinition("skillloadout_warrior_placeholder");
            case "rogue": return GetDefinition("skillloadout_rogue_placeholder");
            case "mage": return GetDefinition("skillloadout_mage_placeholder");
            case "cleric": return GetDefinition("skillloadout_cleric_placeholder");
            default: return null;
        }
    }

    public static PrototypeRpgSkillLoadoutDefinition ResolveDefinition(string loadoutId, string roleTag, string defaultSkillId)
    {
        PrototypeRpgSkillLoadoutDefinition direct = GetDefinition(loadoutId);
        if (direct != null)
        {
            return direct;
        }

        PrototypeRpgSkillLoadoutDefinition fallback = GetFallbackDefinitionForRoleTag(roleTag);
        if (fallback != null)
        {
            return fallback;
        }

        PrototypeRpgSkillDefinition fallbackSkill = PrototypeRpgSkillCatalog.ResolveDefinition(defaultSkillId, roleTag);
        return new PrototypeRpgSkillLoadoutDefinition(string.IsNullOrEmpty(loadoutId) ? "skillloadout_placeholder" : loadoutId, string.IsNullOrEmpty(roleTag) ? "Fallback Loadout" : roleTag + " Loadout", roleTag, "core", fallbackSkill != null ? fallbackSkill.SkillId : string.Empty, string.Empty, fallbackSkill != null ? fallbackSkill.SkillId : string.Empty, fallbackSkill != null ? fallbackSkill.ShortText : "Fallback loadout.", fallbackSkill != null ? new[] { fallbackSkill.SkillId } : Array.Empty<string>(), new PrototypeRpgSkillUsageProfile("usage_fallback", roleTag, "core", "steady", fallbackSkill != null ? fallbackSkill.ShortText : "Fallback loadout."), fallbackSkill != null ? new[] { new PrototypeRpgSkillEffectEntry(fallbackSkill.SkillId, fallbackSkill.EffectType, fallbackSkill.TargetKind, fallbackSkill.PowerValue, fallbackSkill.ShortText, fallbackSkill.TargetKind == "all_allies" ? "party_heal" : fallbackSkill.TargetKind == "all_enemies" ? "splash_damage_or_all_enemies" : fallbackSkill.EffectType == "finisher_damage" ? "finisher_damage" : fallbackSkill.TargetKind == "self" ? "self_recover_or_guarded_strike" : "direct_damage") } : Array.Empty<PrototypeRpgSkillEffectEntry>());
    }

    private static string NormalizeKey(string value)
    {
        return string.IsNullOrWhiteSpace(value) ? string.Empty : value.Trim().ToLowerInvariant();
    }
}
