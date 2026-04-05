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
    public string ConditionKey { get; }
    public string PatternKey { get; }

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
        string conditionKey,
        string patternKey)
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
        ConditionKey = string.IsNullOrWhiteSpace(conditionKey) ? string.Empty : conditionKey.Trim().ToLowerInvariant();
        PatternKey = string.IsNullOrWhiteSpace(patternKey) ? string.Empty : patternKey.Trim().ToLowerInvariant();
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
            string.Empty,
            "heavy_single"),
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
            "target_weakened",
            "precision_finisher"),
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
            string.Empty,
            "arcane_volley"),
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
            "injured_allies",
            "party_recovery")
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
        Members = members ?? System.Array.Empty<PrototypeRpgPartyMemberDefinition>();
    }
}

public static class PrototypeRpgPartyCatalog
{
    public static PrototypeRpgPartyDefinition CreateDefaultPlaceholderParty(string partyId)
    {
        string safePartyId = string.IsNullOrWhiteSpace(partyId) ? "Test Party" : partyId.Trim();
        return new PrototypeRpgPartyDefinition(
            safePartyId,
            safePartyId,
            new[]
            {
                CreateMember(safePartyId, 0, "alden", "Alden", "warrior", "Warrior", 28, 5, 2, 3, "skill_power_strike", "growth_frontline", "job_warrior_novice", "equip_warrior_placeholder", "skillloadout_warrior_placeholder"),
                CreateMember(safePartyId, 1, "mira", "Mira", "rogue", "Rogue", 19, 4, 1, 5, "skill_weak_point", "growth_precision", "job_rogue_novice", "equip_rogue_placeholder", "skillloadout_rogue_placeholder"),
                CreateMember(safePartyId, 2, "rune", "Rune", "mage", "Mage", 16, 3, 0, 4, "skill_arcane_burst", "growth_arcane", "job_mage_novice", "equip_mage_placeholder", "skillloadout_mage_placeholder"),
                CreateMember(safePartyId, 3, "lia", "Lia", "cleric", "Cleric", 22, 3, 1, 2, "skill_radiant_hymn", "growth_support", "job_cleric_novice", "equip_cleric_placeholder", "skillloadout_cleric_placeholder")
            });
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
