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
        MemberId = string.IsNullOrWhiteSpace(memberId) ? string.Empty : memberId.Trim();
        DisplayName = string.IsNullOrWhiteSpace(displayName) ? "Adventurer" : displayName.Trim();
        RoleTag = string.IsNullOrWhiteSpace(roleTag) ? "adventurer" : roleTag.Trim().ToLowerInvariant();
        RoleLabel = string.IsNullOrWhiteSpace(roleLabel) ? "Adventurer" : roleLabel.Trim();
        PartySlotIndex = partySlotIndex >= 0 ? partySlotIndex : 0;
        BaseStats = baseStats ?? new PrototypeRpgStatBlock(1, 1, 0, 0);
        DefaultSkillId = string.IsNullOrWhiteSpace(defaultSkillId) ? string.Empty : defaultSkillId.Trim().ToLowerInvariant();
        DefaultSkillName = string.IsNullOrWhiteSpace(defaultSkillName) ? "Skill" : defaultSkillName.Trim();
        DefaultSkillShortText = string.IsNullOrWhiteSpace(defaultSkillShortText) ? string.Empty : defaultSkillShortText.Trim();
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
                CreateMember(safePartyId, 0, "alden", "Alden", "warrior", "Warrior", 28, 5, 2, 3, "skill_power_strike", "Power Strike", "Heavy single-target strike.", "growth_frontline", "job_warrior_novice", "equip_warrior_placeholder", "skillloadout_warrior_placeholder"),
                CreateMember(safePartyId, 1, "mira", "Mira", "rogue", "Rogue", 19, 4, 1, 5, "skill_weak_point", "Weak Point", "Finisher that hits harder on weak targets.", "growth_precision", "job_rogue_novice", "equip_rogue_placeholder", "skillloadout_rogue_placeholder"),
                CreateMember(safePartyId, 2, "rune", "Rune", "mage", "Mage", 16, 3, 0, 4, "skill_arcane_burst", "Arcane Burst", "Arcane blast that hits all enemies.", "growth_arcane", "job_mage_novice", "equip_mage_placeholder", "skillloadout_mage_placeholder"),
                CreateMember(safePartyId, 3, "lia", "Lia", "cleric", "Cleric", 22, 3, 1, 2, "skill_radiant_hymn", "Radiant Hymn", "Party heal that restores all allies.", "growth_support", "job_cleric_novice", "equip_cleric_placeholder", "skillloadout_cleric_placeholder")
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
        string defaultSkillName,
        string defaultSkillShortText,
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
            defaultSkillName,
            defaultSkillShortText,
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
