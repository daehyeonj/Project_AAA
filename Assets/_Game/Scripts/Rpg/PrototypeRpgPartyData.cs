using System;

public sealed class PrototypeRpgStatBlock
{
    public int MaxHp { get; }
    public int Attack { get; }
    public int Defense { get; }
    public int Speed { get; }

    public PrototypeRpgStatBlock(int maxHp, int attack, int defense, int speed)
    {
        MaxHp = maxHp > 0 ? maxHp : 1;
        Attack = attack > 0 ? attack : 1;
        Defense = defense >= 0 ? defense : 0;
        Speed = speed >= 0 ? speed : 0;
    }
}

public sealed class PrototypeRpgSkillDefinition
{
    public string SkillId { get; }
    public string DisplayName { get; }
    public string ShortText { get; }
    public string TargetKind { get; }
    public string EffectType { get; }
    public int PowerValue { get; }
    public string PowerHint { get; }
    public string EffectHint { get; }
    public string RoleHint { get; }
    public string SegmentKey { get; }
    public string UsageProfileKey { get; }
    public string TierKey { get; }
    public string[] TagKeys { get; }

    public PrototypeRpgSkillDefinition(
        string skillId,
        string displayName,
        string shortText,
        string targetKind,
        string effectType,
        int powerValue,
        string powerHint,
        string effectHint,
        string roleHint,
        string segmentKey = "core",
        string usageProfileKey = "baseline",
        string tierKey = "core",
        string[] tagKeys = null)
    {
        SkillId = string.IsNullOrWhiteSpace(skillId) ? string.Empty : skillId.Trim().ToLowerInvariant();
        DisplayName = string.IsNullOrWhiteSpace(displayName) ? "Skill" : displayName.Trim();
        ShortText = string.IsNullOrWhiteSpace(shortText) ? string.Empty : shortText.Trim();
        TargetKind = string.IsNullOrWhiteSpace(targetKind) ? "single_enemy" : targetKind.Trim().ToLowerInvariant();
        EffectType = string.IsNullOrWhiteSpace(effectType) ? "damage" : effectType.Trim().ToLowerInvariant();
        PowerValue = powerValue > 0 ? powerValue : 1;
        PowerHint = string.IsNullOrWhiteSpace(powerHint) ? string.Empty : powerHint.Trim();
        EffectHint = string.IsNullOrWhiteSpace(effectHint) ? string.Empty : effectHint.Trim();
        RoleHint = string.IsNullOrWhiteSpace(roleHint) ? string.Empty : roleHint.Trim();
        SegmentKey = string.IsNullOrWhiteSpace(segmentKey) ? "core" : segmentKey.Trim().ToLowerInvariant();
        UsageProfileKey = string.IsNullOrWhiteSpace(usageProfileKey) ? "baseline" : usageProfileKey.Trim().ToLowerInvariant();
        TierKey = string.IsNullOrWhiteSpace(tierKey) ? "core" : tierKey.Trim().ToLowerInvariant();
        TagKeys = NormalizeTags(tagKeys);
    }

    private static string[] NormalizeTags(string[] tagKeys)
    {
        if (tagKeys == null || tagKeys.Length <= 0)
        {
            return Array.Empty<string>();
        }

        string[] normalized = new string[tagKeys.Length];
        int count = 0;
        for (int i = 0; i < tagKeys.Length; i++)
        {
            string tag = string.IsNullOrWhiteSpace(tagKeys[i]) ? string.Empty : tagKeys[i].Trim().ToLowerInvariant();
            if (string.IsNullOrEmpty(tag))
            {
                continue;
            }

            normalized[count++] = tag;
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
}

public static class PrototypeRpgSkillCatalog
{
    private static readonly PrototypeRpgSkillDefinition[] SharedDefinitions =
    {
        new PrototypeRpgSkillDefinition(
            "skill_power_strike",
            "Power Strike",
            "Heavy single-target strike.",
            "single_enemy",
            "damage",
            10,
            "high",
            "front-loaded physical burst",
            "Warrior",
            segmentKey: "frontline",
            usageProfileKey: "vanguard_burst",
            tierKey: "core",
            tagKeys: new[] { "burst", "single_target", "frontline" }),
        new PrototypeRpgSkillDefinition(
            "skill_weak_point",
            "Weak Point",
            "Finisher that hits harder on weak targets.",
            "single_enemy",
            "finisher_damage",
            7,
            "medium_high",
            "precision finisher",
            "Rogue",
            segmentKey: "precision",
            usageProfileKey: "precision_chain",
            tierKey: "core",
            tagKeys: new[] { "finisher", "single_target", "precision" }),
        new PrototypeRpgSkillDefinition(
            "skill_arcane_burst",
            "Arcane Burst",
            "Arcane blast that hits all enemies.",
            "all_enemies",
            "damage",
            6,
            "medium",
            "multi-target arcane burst",
            "Mage",
            segmentKey: "arcane",
            usageProfileKey: "arcane_volley",
            tierKey: "core",
            tagKeys: new[] { "aoe", "arcane", "pressure" }),
        new PrototypeRpgSkillDefinition(
            "skill_radiant_hymn",
            "Radiant Hymn",
            "Party heal that restores all allies.",
            "all_allies",
            "heal",
            5,
            "support",
            "party healing pulse",
            "Cleric",
            segmentKey: "support",
            usageProfileKey: "sanctuary_chorus",
            tierKey: "core",
            tagKeys: new[] { "heal", "support", "party" }),
        new PrototypeRpgSkillDefinition(
            "skill_guarding_stance",
            "Guarding Stance",
            "Placeholder guard follow-up that reinforces the frontline.",
            "self",
            "guard",
            4,
            "guard",
            "stability and guard posture",
            "Warrior",
            segmentKey: "frontline",
            usageProfileKey: "vanguard_guard",
            tierKey: "skeleton",
            tagKeys: new[] { "guard", "frontline", "placeholder" }),
        new PrototypeRpgSkillDefinition(
            "skill_exploit_mark",
            "Exploit Mark",
            "Placeholder setup skill that sharpens precision follow-up.",
            "single_enemy",
            "setup",
            4,
            "setup",
            "marking and setup pressure",
            "Rogue",
            segmentKey: "precision",
            usageProfileKey: "precision_setup",
            tierKey: "skeleton",
            tagKeys: new[] { "setup", "precision", "placeholder" }),
        new PrototypeRpgSkillDefinition(
            "skill_focus_surge",
            "Focus Surge",
            "Placeholder channel that amplifies the next arcane release.",
            "self",
            "amplify",
            4,
            "amplify",
            "charge and spell shaping",
            "Mage",
            segmentKey: "arcane",
            usageProfileKey: "arcane_charge",
            tierKey: "skeleton",
            tagKeys: new[] { "charge", "arcane", "placeholder" }),
        new PrototypeRpgSkillDefinition(
            "skill_restoration_ward",
            "Restoration Ward",
            "Placeholder safeguard that stabilizes wounded allies.",
            "all_allies",
            "protect_heal",
            4,
            "support",
            "stability and recovery buffer",
            "Cleric",
            segmentKey: "support",
            usageProfileKey: "sanctuary_guard",
            tierKey: "skeleton",
            tagKeys: new[] { "support", "guard", "placeholder" })
    };

    public static PrototypeRpgSkillDefinition GetDefinition(string skillId)
    {
        string normalizedSkillId = NormalizeKey(skillId);
        if (string.IsNullOrEmpty(normalizedSkillId))
        {
            return null;
        }

        for (int i = 0; i < SharedDefinitions.Length; i++)
        {
            PrototypeRpgSkillDefinition definition = SharedDefinitions[i];
            if (definition != null && definition.SkillId == normalizedSkillId)
            {
                return definition;
            }
        }

        return null;
    }

    public static PrototypeRpgSkillDefinition GetFallbackDefinitionForRoleTag(string roleTag)
    {
        switch (NormalizeKey(roleTag))
        {
            case "warrior":
                return GetDefinition("skill_power_strike");
            case "rogue":
                return GetDefinition("skill_weak_point");
            case "mage":
                return GetDefinition("skill_arcane_burst");
            case "cleric":
                return GetDefinition("skill_radiant_hymn");
            default:
                return null;
        }
    }

    public static PrototypeRpgSkillDefinition ResolveDefinition(string skillId, string roleTag)
    {
        PrototypeRpgSkillDefinition directMatch = GetDefinition(skillId);
        return directMatch ?? GetFallbackDefinitionForRoleTag(roleTag);
    }

    private static string NormalizeKey(string value)
    {
        return string.IsNullOrWhiteSpace(value) ? string.Empty : value.Trim().ToLowerInvariant();
    }
}

public sealed class PrototypeRpgPartyMemberDefinition
{
    public string MemberId { get; }
    public string DisplayName { get; }
    public string RoleTag { get; }
    public string RoleLabel { get; }
    public int PartySlotIndex { get; }
    public PrototypeRpgStatBlock BaseStats { get; }
    public string DefaultSkillId { get; }
    public string DefaultSkillName { get; }
    public string DefaultSkillShortText { get; }
    public string GrowthTrackId { get; }
    public string JobId { get; }
    public string EquipmentLoadoutId { get; }
    public string SkillLoadoutId { get; }

    public PrototypeRpgPartyMemberDefinition(
        string memberId,
        string displayName,
        string roleTag,
        string roleLabel,
        int partySlotIndex,
        PrototypeRpgStatBlock baseStats,
        string defaultSkillId,
        string defaultSkillName,
        string defaultSkillShortText,
        string growthTrackId,
        string jobId,
        string equipmentLoadoutId,
        string skillLoadoutId)
    {
        string normalizedRoleTag = string.IsNullOrWhiteSpace(roleTag) ? "adventurer" : roleTag.Trim().ToLowerInvariant();
        PrototypeRpgSkillDefinition sharedSkill = PrototypeRpgSkillCatalog.ResolveDefinition(defaultSkillId, normalizedRoleTag);

        MemberId = string.IsNullOrWhiteSpace(memberId) ? string.Empty : memberId.Trim();
        DisplayName = string.IsNullOrWhiteSpace(displayName) ? "Adventurer" : displayName.Trim();
        RoleTag = normalizedRoleTag;
        RoleLabel = string.IsNullOrWhiteSpace(roleLabel) ? "Adventurer" : roleLabel.Trim();
        PartySlotIndex = partySlotIndex >= 0 ? partySlotIndex : 0;
        BaseStats = baseStats ?? new PrototypeRpgStatBlock(1, 1, 0, 0);
        DefaultSkillId = sharedSkill != null
            ? sharedSkill.SkillId
            : (string.IsNullOrWhiteSpace(defaultSkillId) ? string.Empty : defaultSkillId.Trim().ToLowerInvariant());
        DefaultSkillName = !string.IsNullOrWhiteSpace(defaultSkillName)
            ? defaultSkillName.Trim()
            : (sharedSkill != null ? sharedSkill.DisplayName : "Skill");
        DefaultSkillShortText = !string.IsNullOrWhiteSpace(defaultSkillShortText)
            ? defaultSkillShortText.Trim()
            : (sharedSkill != null ? sharedSkill.ShortText : string.Empty);
        GrowthTrackId = string.IsNullOrWhiteSpace(growthTrackId) ? string.Empty : growthTrackId.Trim();
        JobId = string.IsNullOrWhiteSpace(jobId) ? string.Empty : jobId.Trim();
        EquipmentLoadoutId = string.IsNullOrWhiteSpace(equipmentLoadoutId) ? string.Empty : equipmentLoadoutId.Trim();
        SkillLoadoutId = string.IsNullOrWhiteSpace(skillLoadoutId) ? string.Empty : skillLoadoutId.Trim();
    }
}

public sealed class PrototypeRpgPartyDefinition
{
    public string PartyId { get; }
    public string DisplayName { get; }
    public PrototypeRpgPartyMemberDefinition[] Members { get; }

    public PrototypeRpgPartyDefinition(string partyId, string displayName, PrototypeRpgPartyMemberDefinition[] members)
    {
        PartyId = string.IsNullOrWhiteSpace(partyId) ? string.Empty : partyId.Trim();
        DisplayName = string.IsNullOrWhiteSpace(displayName) ? "Test Party" : displayName.Trim();
        Members = members ?? Array.Empty<PrototypeRpgPartyMemberDefinition>();
    }
}

public static class PrototypeRpgPartyCatalog
{
    public static PrototypeRpgPartyDefinition CreateDefaultPlaceholderParty(string partyId)
    {
        return CreatePlaceholderPartyByArchetype(partyId, "balanced_company");
    }

    public static PrototypeRpgPartyDefinition CreatePlaceholderPartyByArchetype(string partyId, string archetypeKey, string cityLabel = "")
    {
        string safePartyId = string.IsNullOrWhiteSpace(partyId) ? "Test Party" : partyId.Trim();
        string safeCityLabel = string.IsNullOrWhiteSpace(cityLabel) ? string.Empty : cityLabel.Trim();
        string normalizedArchetypeKey = NormalizeArchetypeKey(archetypeKey);

        switch (normalizedArchetypeKey)
        {
            case "vanguard_company":
                return CreateVanguardCompany(safePartyId, safeCityLabel);
            case "skirmish_company":
                return CreateSkirmishCompany(safePartyId, safeCityLabel);
            case "sustain_company":
                return CreateSustainCompany(safePartyId, safeCityLabel);
            default:
                return CreateBalancedCompany(safePartyId, safeCityLabel);
        }
    }

    public static string GetPlaceholderPartyArchetypeDisplayName(string archetypeKey)
    {
        switch (NormalizeArchetypeKey(archetypeKey))
        {
            case "vanguard_company":
                return "Vanguard Company";
            case "skirmish_company":
                return "Skirmish Company";
            case "sustain_company":
                return "Sustain Company";
            default:
                return "Balanced Company";
        }
    }

    private static PrototypeRpgPartyDefinition CreateBalancedCompany(string partyId, string cityLabel)
    {
        return new PrototypeRpgPartyDefinition(
            partyId,
            BuildPartyDisplayName(cityLabel, "Balanced Company"),
            new[]
            {
                CreateMember(partyId, 0, "alden", "Alden", "warrior", "Warrior", 28, 5, 2, 3, "skill_power_strike", "growth_frontline", "job_warrior_novice", "equip_warrior_placeholder", "skillloadout_warrior_placeholder"),
                CreateMember(partyId, 1, "mira", "Mira", "rogue", "Rogue", 19, 4, 1, 5, "skill_weak_point", "growth_precision", "job_rogue_novice", "equip_rogue_placeholder", "skillloadout_rogue_placeholder"),
                CreateMember(partyId, 2, "rune", "Rune", "mage", "Mage", 16, 3, 0, 4, "skill_arcane_burst", "growth_arcane", "job_mage_novice", "equip_mage_placeholder", "skillloadout_mage_placeholder"),
                CreateMember(partyId, 3, "lia", "Lia", "cleric", "Cleric", 22, 3, 1, 2, "skill_radiant_hymn", "growth_support", "job_cleric_novice", "equip_cleric_placeholder", "skillloadout_cleric_placeholder")
            });
    }

    private static PrototypeRpgPartyDefinition CreateVanguardCompany(string partyId, string cityLabel)
    {
        return new PrototypeRpgPartyDefinition(
            partyId,
            BuildPartyDisplayName(cityLabel, "Vanguard Company"),
            new[]
            {
                CreateMember(partyId, 0, "bram", "Bram", "warrior", "Warrior", 32, 6, 3, 2, "skill_power_strike", "growth_frontline", "job_warrior_novice", "equip_warrior_placeholder", "skillloadout_warrior_placeholder"),
                CreateMember(partyId, 1, "sera", "Sera", "warrior", "Vanguard", 27, 5, 2, 3, "skill_guarding_stance", "growth_frontline", "job_warrior_novice", "equip_warrior_placeholder", "skillloadout_warrior_placeholder"),
                CreateMember(partyId, 2, "nyx", "Nyx", "mage", "Mage", 17, 4, 0, 4, "skill_arcane_burst", "growth_arcane", "job_mage_novice", "equip_mage_placeholder", "skillloadout_mage_placeholder"),
                CreateMember(partyId, 3, "edda", "Edda", "cleric", "Cleric", 24, 3, 1, 2, "skill_restoration_ward", "growth_support", "job_cleric_novice", "equip_cleric_placeholder", "skillloadout_cleric_placeholder")
            });
    }

    private static PrototypeRpgPartyDefinition CreateSkirmishCompany(string partyId, string cityLabel)
    {
        return new PrototypeRpgPartyDefinition(
            partyId,
            BuildPartyDisplayName(cityLabel, "Skirmish Company"),
            new[]
            {
                CreateMember(partyId, 0, "vale", "Vale", "warrior", "Warrior", 24, 4, 1, 4, "skill_power_strike", "growth_frontline", "job_warrior_novice", "equip_warrior_placeholder", "skillloadout_warrior_placeholder"),
                CreateMember(partyId, 1, "kite", "Kite", "rogue", "Rogue", 20, 5, 1, 6, "skill_exploit_mark", "growth_precision", "job_rogue_novice", "equip_rogue_placeholder", "skillloadout_rogue_placeholder"),
                CreateMember(partyId, 2, "tess", "Tess", "Rogue", "Rogue", 18, 4, 1, 6, "skill_weak_point", "growth_precision", "job_rogue_novice", "equip_rogue_placeholder", "skillloadout_rogue_placeholder"),
                CreateMember(partyId, 3, "iris", "Iris", "mage", "Mage", 16, 4, 0, 5, "skill_arcane_burst", "growth_arcane", "job_mage_novice", "equip_mage_placeholder", "skillloadout_mage_placeholder")
            });
    }

    private static PrototypeRpgPartyDefinition CreateSustainCompany(string partyId, string cityLabel)
    {
        return new PrototypeRpgPartyDefinition(
            partyId,
            BuildPartyDisplayName(cityLabel, "Sustain Company"),
            new[]
            {
                CreateMember(partyId, 0, "garran", "Garran", "warrior", "Warrior", 30, 4, 3, 2, "skill_guarding_stance", "growth_frontline", "job_warrior_novice", "equip_warrior_placeholder", "skillloadout_warrior_placeholder"),
                CreateMember(partyId, 1, "liora", "Liora", "cleric", "Cleric", 24, 3, 1, 2, "skill_radiant_hymn", "growth_support", "job_cleric_novice", "equip_cleric_placeholder", "skillloadout_cleric_placeholder"),
                CreateMember(partyId, 2, "marin", "Marin", "cleric", "Cleric", 22, 3, 1, 3, "skill_restoration_ward", "growth_support", "job_cleric_novice", "equip_cleric_placeholder", "skillloadout_cleric_placeholder"),
                CreateMember(partyId, 3, "rune", "Rune", "mage", "Mage", 16, 3, 0, 4, "skill_focus_surge", "growth_arcane", "job_mage_novice", "equip_mage_placeholder", "skillloadout_mage_placeholder")
            });
    }

    private static string NormalizeArchetypeKey(string archetypeKey)
    {
        return string.IsNullOrWhiteSpace(archetypeKey) ? "balanced_company" : archetypeKey.Trim().ToLowerInvariant();
    }

    private static string BuildPartyDisplayName(string cityLabel, string companyLabel)
    {
        return string.IsNullOrWhiteSpace(cityLabel)
            ? companyLabel
            : cityLabel.Trim() + " " + companyLabel;
    }

    private static PrototypeRpgPartyMemberDefinition CreateMember(
        string partyId,
        int partySlotIndex,
        string memberKey,
        string displayName,
        string roleTag,
        string roleLabel,
        int maxHp,
        int attack,
        int defense,
        int speed,
        string defaultSkillId,
        string growthTrackId,
        string jobId,
        string equipmentLoadoutId,
        string skillLoadoutId)
    {
        return new PrototypeRpgPartyMemberDefinition(
            BuildMemberId(partyId, partySlotIndex, memberKey),
            displayName,
            roleTag,
            roleLabel,
            partySlotIndex,
            new PrototypeRpgStatBlock(maxHp, attack, defense, speed),
            defaultSkillId,
            string.Empty,
            string.Empty,
            growthTrackId,
            jobId,
            equipmentLoadoutId,
            skillLoadoutId);
    }

    private static string BuildMemberId(string partyId, int partySlotIndex, string memberKey)
    {
        string safePartyId = string.IsNullOrWhiteSpace(partyId)
            ? "party"
            : partyId.Trim().ToLowerInvariant().Replace(" ", "-");
        string safeMemberKey = string.IsNullOrWhiteSpace(memberKey)
            ? "member"
            : memberKey.Trim().ToLowerInvariant();
        return safePartyId + "-slot-" + (partySlotIndex + 1) + "-" + safeMemberKey;
    }
}
