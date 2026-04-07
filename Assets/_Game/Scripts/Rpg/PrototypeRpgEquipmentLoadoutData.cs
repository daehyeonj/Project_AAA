using System;

public sealed class PrototypeRpgEquipmentDefinition
{
    public string EquipmentId { get; }
    public string LoadoutId { get; }
    public string DisplayName { get; }
    public string SlotKey { get; }
    public int MaxHpDelta { get; }
    public int AttackDelta { get; }
    public int DefenseDelta { get; }
    public int SpeedDelta { get; }
    public string PassiveHintText { get; }
    public string BattleLabelHint { get; }
    public string SummaryText { get; }

    public PrototypeRpgEquipmentDefinition(string equipmentId, string loadoutId, string displayName, string slotKey, int maxHpDelta, int attackDelta, int defenseDelta, int speedDelta, string passiveHintText, string battleLabelHint, string summaryText)
    {
        EquipmentId = NormalizeKey(equipmentId);
        LoadoutId = NormalizeKey(loadoutId);
        DisplayName = string.IsNullOrWhiteSpace(displayName) ? "Equipment" : displayName.Trim();
        SlotKey = NormalizeKey(slotKey);
        MaxHpDelta = maxHpDelta;
        AttackDelta = attackDelta;
        DefenseDelta = defenseDelta;
        SpeedDelta = speedDelta;
        PassiveHintText = string.IsNullOrWhiteSpace(passiveHintText) ? string.Empty : passiveHintText.Trim();
        BattleLabelHint = string.IsNullOrWhiteSpace(battleLabelHint) ? string.Empty : battleLabelHint.Trim();
        SummaryText = string.IsNullOrWhiteSpace(summaryText)
            ? BuildContributionSummary(maxHpDelta, attackDelta, defenseDelta, speedDelta)
            : summaryText.Trim();
    }

    private static string NormalizeKey(string value)
    {
        return string.IsNullOrWhiteSpace(value) ? string.Empty : value.Trim().ToLowerInvariant();
    }

    private static string BuildContributionSummary(int maxHpDelta, int attackDelta, int defenseDelta, int speedDelta)
    {
        return "HP+" + Math.Max(0, maxHpDelta) + " ATK+" + Math.Max(0, attackDelta) + " DEF+" + Math.Max(0, defenseDelta) + " SPD+" + Math.Max(0, speedDelta);
    }
}

public sealed class PrototypeRpgEquipmentLoadoutDefinition
{
    public string LoadoutId { get; }
    public string DisplayName { get; }
    public string RoleTag { get; }
    public string TierKey { get; }
    public PrototypeRpgEquipmentDefinition[] Items { get; }
    public int TotalMaxHpDelta { get; }
    public int TotalAttackDelta { get; }
    public int TotalDefenseDelta { get; }
    public int TotalSpeedDelta { get; }
    public string PassiveHintText { get; }
    public string BattleLabelHint { get; }
    public string SummaryText { get; }

    public PrototypeRpgEquipmentLoadoutDefinition(string loadoutId, string displayName, string roleTag, string tierKey, PrototypeRpgEquipmentDefinition[] items, string passiveHintText, string battleLabelHint, string summaryText)
    {
        LoadoutId = NormalizeKey(loadoutId);
        DisplayName = string.IsNullOrWhiteSpace(displayName) ? "Equipment Loadout" : displayName.Trim();
        RoleTag = NormalizeKey(roleTag);
        TierKey = string.IsNullOrWhiteSpace(tierKey) ? "core" : tierKey.Trim().ToLowerInvariant();
        Items = items ?? Array.Empty<PrototypeRpgEquipmentDefinition>();

        int maxHpDelta = 0;
        int attackDelta = 0;
        int defenseDelta = 0;
        int speedDelta = 0;
        for (int i = 0; i < Items.Length; i++)
        {
            PrototypeRpgEquipmentDefinition item = Items[i];
            if (item == null)
            {
                continue;
            }

            maxHpDelta += item.MaxHpDelta;
            attackDelta += item.AttackDelta;
            defenseDelta += item.DefenseDelta;
            speedDelta += item.SpeedDelta;
        }

        TotalMaxHpDelta = maxHpDelta;
        TotalAttackDelta = attackDelta;
        TotalDefenseDelta = defenseDelta;
        TotalSpeedDelta = speedDelta;
        PassiveHintText = string.IsNullOrWhiteSpace(passiveHintText) ? string.Empty : passiveHintText.Trim();
        BattleLabelHint = string.IsNullOrWhiteSpace(battleLabelHint) ? string.Empty : battleLabelHint.Trim();
        SummaryText = string.IsNullOrWhiteSpace(summaryText)
            ? DisplayName + " | " + BuildContributionSummary(TotalMaxHpDelta, TotalAttackDelta, TotalDefenseDelta, TotalSpeedDelta)
            : summaryText.Trim();
    }

    private static string NormalizeKey(string value)
    {
        return string.IsNullOrWhiteSpace(value) ? string.Empty : value.Trim().ToLowerInvariant();
    }

    private static string BuildContributionSummary(int maxHpDelta, int attackDelta, int defenseDelta, int speedDelta)
    {
        return "HP+" + Math.Max(0, maxHpDelta) + " ATK+" + Math.Max(0, attackDelta) + " DEF+" + Math.Max(0, defenseDelta) + " SPD+" + Math.Max(0, speedDelta);
    }
}

public static class PrototypeRpgEquipmentCatalog
{
    public static PrototypeRpgEquipmentLoadoutDefinition ResolveDefinition(string loadoutId, string roleTag)
    {
        PrototypeRpgEquipmentLoadoutDefinition direct = GetDefinition(loadoutId);
        return direct ?? GetFallbackDefinitionForRoleTag(roleTag);
    }

    public static PrototypeRpgEquipmentLoadoutDefinition GetFallbackDefinitionForRoleTag(string roleTag)
    {
        switch (NormalizeKey(roleTag))
        {
            case "warrior": return GetDefinition("equip_warrior_placeholder");
            case "rogue": return GetDefinition("equip_rogue_placeholder");
            case "mage": return GetDefinition("equip_mage_placeholder");
            case "cleric": return GetDefinition("equip_cleric_placeholder");
            default: return null;
        }
    }

    public static PrototypeRpgEquipmentLoadoutDefinition GetDefinition(string loadoutId)
    {
        switch (NormalizeKey(loadoutId))
        {
            case "equip_warrior_placeholder": return CreateSingleItemLoadout("equip_warrior_placeholder", "Iron Vanguard Kit", "warrior", "core", "armor", 2, 0, 1, 0, "Guarded strikes blunt the first counterhit.", "Vanguard", "Baseline frontline plating for steady pressure.");
            case "equip_warrior_brutal": return CreateSingleItemLoadout("equip_warrior_brutal", "Brutal Edge Harness", "warrior", "tier_1", "weapon", 0, 2, 0, 0, "Power Strike leans harder into burst windows.", "Breaker", "Aggressive weapon loadout that lifts direct strike damage.");
            case "equip_warrior_shielded": return CreateSingleItemLoadout("equip_warrior_shielded", "Shieldwall Plate", "warrior", "tier_1", "shield", 3, 0, 2, 0, "Shield stance softens focus fire on the frontline.", "Bulwark", "Guard-heavy loadout that improves HP and DEF.");
            case "equip_warrior_vanguard": return CreateSingleItemLoadout("equip_warrior_vanguard", "Vanguard Standard", "warrior", "tier_2", "armor", 2, 1, 1, 0, "Balanced pressure keeps the front line active longer.", "Vanguard", "Balanced frontline kit with mixed offense and guard.");
            case "equip_warrior_bastion": return CreateSingleItemLoadout("equip_warrior_bastion", "Bastion Bulwark", "warrior", "tier_3", "armor", 4, 0, 3, 0, "Bastion plating rewards staying in the thick of the fight.", "Bastion", "Heavy bastion gear for strong HP and DEF spikes.");
            case "equip_rogue_placeholder": return CreateSingleItemLoadout("equip_rogue_placeholder", "Shadow Knife Rig", "rogue", "core", "weapon", 0, 1, 0, 1, "Quick knives favor early picks and tempo.", "Skirmisher", "Baseline rogue kit with ATK and SPD lift.");
            case "equip_rogue_finisher": return CreateSingleItemLoadout("equip_rogue_finisher", "Execution Knives", "rogue", "tier_1", "weapon", 0, 2, 0, 1, "Weak Point hits harder when the finisher window opens.", "Finisher", "Finisher loadout that boosts ATK and tempo.");
            case "equip_rogue_evasion": return CreateSingleItemLoadout("equip_rogue_evasion", "Evasion Cloak Rig", "rogue", "tier_1", "armor", 0, 0, 1, 2, "Slip gear gives the rogue more room to avoid retaliation.", "Evasive", "Defensive rogue kit with SPD-first bias.");
            case "equip_rogue_saboteur": return CreateSingleItemLoadout("equip_rogue_saboteur", "Saboteur Satchel", "rogue", "tier_2", "utility", 0, 1, 1, 1, "Saboteur tools keep pressure after setup turns.", "Saboteur", "Balanced rogue kit for setup and sustained pressure.");
            case "equip_rogue_phantom": return CreateSingleItemLoadout("equip_rogue_phantom", "Phantom Weave", "rogue", "tier_3", "armor", 0, 2, 1, 2, "Phantom weave keeps the rogue ahead in turn tempo.", "Phantom", "High-end rogue kit that spikes tempo and finish pressure.");
            case "equip_mage_placeholder": return CreateSingleItemLoadout("equip_mage_placeholder", "Focus Rod Kit", "mage", "core", "focus", 0, 1, 0, 1, "Focus rods stabilize Arcane Burst releases.", "Channeler", "Baseline mage focus with ATK and SPD lift.");
            case "equip_mage_focus": return CreateSingleItemLoadout("equip_mage_focus", "Focused Catalyst", "mage", "tier_1", "focus", 0, 2, 0, 0, "Focused catalysts amplify arcane burst power.", "Focus", "Attack-forward mage focus for stronger spell output.");
            case "equip_mage_warded": return CreateSingleItemLoadout("equip_mage_warded", "Wardwoven Mantle", "mage", "tier_1", "armor", 1, 1, 1, 0, "Wards soften incoming pressure while casting.", "Warded", "Safer arcane kit that adds HP and DEF support.");
            case "equip_mage_tempest": return CreateSingleItemLoadout("equip_mage_tempest", "Tempest Sigil Set", "mage", "tier_2", "focus", 0, 2, 0, 1, "Tempest sigils accelerate multi-target burst tempo.", "Tempest", "Arcane tempo kit with ATK and SPD emphasis.");
            case "equip_mage_astral": return CreateSingleItemLoadout("equip_mage_astral", "Astral Conduit", "mage", "tier_3", "focus", 1, 3, 0, 1, "Astral conduits support high-impact spell turns.", "Astral", "High-impact mage gear that pushes burst and sustain together.");
            case "equip_cleric_placeholder": return CreateSingleItemLoadout("equip_cleric_placeholder", "Pilgrim Relic Set", "cleric", "core", "relic", 1, 1, 1, 0, "Travel relics steady hymn output and recovery windows.", "Sanctuary", "Baseline cleric relic with balanced sustain stats.");
            case "equip_cleric_relic": return CreateSingleItemLoadout("equip_cleric_relic", "Radiant Relic", "cleric", "tier_1", "relic", 1, 2, 1, 0, "Relic focus deepens whole-party healing output.", "Relic", "Support gear that boosts healing-facing power.");
            case "equip_cleric_guarded": return CreateSingleItemLoadout("equip_cleric_guarded", "Guarded Vestments", "cleric", "tier_1", "armor", 2, 0, 2, 0, "Guarded vestments keep the back line steady.", "Warden", "Durable cleric gear for safer recovery turns.");
            case "equip_cleric_beacon": return CreateSingleItemLoadout("equip_cleric_beacon", "Beacon Charm", "cleric", "tier_2", "relic", 1, 2, 1, 1, "Beacon charms speed up rescue turns and support timing.", "Beacon", "Tempo support gear with light offense and speed.");
            case "equip_cleric_saint": return CreateSingleItemLoadout("equip_cleric_saint", "Saint Regalia", "cleric", "tier_3", "armor", 2, 2, 2, 0, "Saint regalia keeps sustain windows wide open.", "Saint", "Late-session cleric gear with broad stat reinforcement.");
            default: return null;
        }
    }

    public static PrototypeRpgEquipmentDefinition GetPrimaryItemDefinition(string loadoutId, string roleTag)
    {
        PrototypeRpgEquipmentLoadoutDefinition definition = ResolveDefinition(loadoutId, roleTag);
        PrototypeRpgEquipmentDefinition[] items = definition != null ? definition.Items : Array.Empty<PrototypeRpgEquipmentDefinition>();
        return items.Length > 0 ? items[0] : null;
    }

    private static PrototypeRpgEquipmentLoadoutDefinition CreateSingleItemLoadout(string loadoutId, string displayName, string roleTag, string tierKey, string slotKey, int maxHpDelta, int attackDelta, int defenseDelta, int speedDelta, string passiveHintText, string battleLabelHint, string summaryText)
    {
        PrototypeRpgEquipmentDefinition item = new PrototypeRpgEquipmentDefinition(loadoutId + "_core", loadoutId, displayName, slotKey, maxHpDelta, attackDelta, defenseDelta, speedDelta, passiveHintText, battleLabelHint, summaryText);
        return new PrototypeRpgEquipmentLoadoutDefinition(loadoutId, displayName, roleTag, tierKey, new[] { item }, passiveHintText, battleLabelHint, summaryText + " | HP+" + Math.Max(0, maxHpDelta) + " ATK+" + Math.Max(0, attackDelta) + " DEF+" + Math.Max(0, defenseDelta) + " SPD+" + Math.Max(0, speedDelta));
    }

    private static string NormalizeKey(string value)
    {
        return string.IsNullOrWhiteSpace(value) ? string.Empty : value.Trim().ToLowerInvariant();
    }
}