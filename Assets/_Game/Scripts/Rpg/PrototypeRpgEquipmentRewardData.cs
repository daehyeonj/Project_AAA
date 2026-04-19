using System;
using System.Collections.Generic;

public static class PrototypeRpgEquipmentSlotKeys
{
    public const string Head = "head";
    public const string LeftArm = "left_arm";
    public const string RightArm = "right_arm";
    public const string Torso = "torso";
    public const string Belt = "belt";
    public const string Pants = "pants";
    public const string Shoes = "shoes";

    private static readonly string[] OrderedKeysInternal =
    {
        Head,
        LeftArm,
        RightArm,
        Torso,
        Belt,
        Pants,
        Shoes
    };

    public static string[] OrderedKeys => OrderedKeysInternal;

    public static string ToDisplayLabel(string slotKey)
    {
        switch (Normalize(slotKey))
        {
            case Head:
                return "Head";
            case LeftArm:
                return "Left Arm";
            case RightArm:
                return "Right Arm";
            case Torso:
                return "Torso";
            case Belt:
                return "Belt";
            case Pants:
                return "Pants";
            case Shoes:
                return "Shoes";
            default:
                return "Gear";
        }
    }

    public static string ToShortLabel(string slotKey)
    {
        switch (Normalize(slotKey))
        {
            case Head:
                return "H";
            case LeftArm:
                return "LA";
            case RightArm:
                return "RA";
            case Torso:
                return "T";
            case Belt:
                return "B";
            case Pants:
                return "P";
            case Shoes:
                return "S";
            default:
                return "G";
        }
    }

    private static string Normalize(string value)
    {
        return string.IsNullOrWhiteSpace(value) ? string.Empty : value.Trim().ToLowerInvariant();
    }
}

public sealed class PrototypeRpgEquipmentDefinition
{
    public string LoadoutId { get; }
    public string ItemId => LoadoutId;
    public string DisplayName { get; }
    public string RoleTag { get; }
    public string SlotKey { get; }
    public string SlotLabel { get; }
    public string TierKey { get; }
    public string TierLabel { get; }
    public int TierRank { get; }
    public int MaxHpBonus { get; }
    public int AttackBonus { get; }
    public int DefenseBonus { get; }
    public int SpeedBonus { get; }
    public int SkillPowerBonus { get; }

    public PrototypeRpgEquipmentDefinition(
        string loadoutId,
        string displayName,
        string roleTag,
        string slotKey,
        string slotLabel,
        string tierKey,
        string tierLabel,
        int tierRank,
        int maxHpBonus,
        int attackBonus,
        int defenseBonus,
        int speedBonus,
        int skillPowerBonus)
    {
        LoadoutId = string.IsNullOrWhiteSpace(loadoutId) ? string.Empty : loadoutId.Trim();
        DisplayName = string.IsNullOrWhiteSpace(displayName) ? "Gear" : displayName.Trim();
        RoleTag = string.IsNullOrWhiteSpace(roleTag) ? string.Empty : roleTag.Trim().ToLowerInvariant();
        SlotKey = string.IsNullOrWhiteSpace(slotKey) ? string.Empty : slotKey.Trim().ToLowerInvariant();
        SlotLabel = string.IsNullOrWhiteSpace(slotLabel) ? PrototypeRpgEquipmentSlotKeys.ToDisplayLabel(SlotKey) : slotLabel.Trim();
        TierKey = string.IsNullOrWhiteSpace(tierKey) ? "starter" : tierKey.Trim().ToLowerInvariant();
        TierLabel = string.IsNullOrWhiteSpace(tierLabel) ? "Starter" : tierLabel.Trim();
        TierRank = tierRank > 0 ? tierRank : 1;
        MaxHpBonus = maxHpBonus;
        AttackBonus = attackBonus;
        DefenseBonus = defenseBonus;
        SpeedBonus = speedBonus;
        SkillPowerBonus = skillPowerBonus;
    }

    public int GetScore()
    {
        return (MaxHpBonus * 2) +
               (AttackBonus * 4) +
               (DefenseBonus * 3) +
               (SpeedBonus * 3) +
               (SkillPowerBonus * 4);
    }
}

public sealed class PrototypeRpgEquipmentRewardCandidate
{
    public string RewardId = string.Empty;
    public string MemberId = string.Empty;
    public string MemberDisplayName = string.Empty;
    public string RoleTag = string.Empty;
    public string TargetSlotLabel = string.Empty;
    public string TierLabel = string.Empty;
    public string CurrentEquipmentLoadoutId = string.Empty;
    public string RewardEquipmentLoadoutId = string.Empty;
    public string CurrentEquipmentSummaryText = string.Empty;
    public string RewardEquipmentSummaryText = string.Empty;
    public string RewardStatSummaryText = string.Empty;
    public string ComparisonSummaryText = string.Empty;
    public bool IsRecommended;
}

public sealed class PrototypeRpgEquipmentRewardChoice
{
    public string MemberId = string.Empty;
    public string ChoiceKey = string.Empty;
    public string ChoiceLabel = string.Empty;
    public string CurrentEquipmentLoadoutId = string.Empty;
    public string TargetEquipmentLoadoutId = string.Empty;
    public string ChoiceSummaryText = string.Empty;
    public bool HasChanges;
}

public static class PrototypeRpgEquipmentCatalog
{
    private static readonly PrototypeRpgEquipmentDefinition[] Definitions = BuildDefinitions();

    public static PrototypeRpgEquipmentDefinition ResolveDefinition(string loadoutId, string roleTag)
    {
        string normalizedLoadoutId = Normalize(loadoutId);
        string normalizedRoleTag = Normalize(roleTag);

        if (!string.IsNullOrEmpty(normalizedLoadoutId))
        {
            for (int i = 0; i < Definitions.Length; i++)
            {
                PrototypeRpgEquipmentDefinition definition = Definitions[i];
                if (definition != null && Normalize(definition.LoadoutId) == normalizedLoadoutId)
                {
                    return definition;
                }
            }
        }

        return GetFallbackDefinitionForRoleTag(normalizedRoleTag);
    }

    public static PrototypeRpgEquipmentDefinition GetFallbackDefinitionForRoleTag(string roleTag)
    {
        string normalizedRoleTag = Normalize(roleTag);
        string fallbackId = normalizedRoleTag switch
        {
            "warrior" => "equip_warrior_placeholder",
            "rogue" => "equip_rogue_placeholder",
            "mage" => "equip_mage_placeholder",
            "cleric" => "equip_cleric_placeholder",
            _ => string.Empty
        };

        for (int i = 0; i < Definitions.Length; i++)
        {
            PrototypeRpgEquipmentDefinition definition = Definitions[i];
            if (definition != null && Normalize(definition.LoadoutId) == fallbackId)
            {
                return definition;
            }
        }

        return null;
    }

    public static PrototypeRpgEquipmentDefinition GetStarterDefinition(string roleTag, string slotKey)
    {
        return ResolveDefinition(BuildSlotItemId(roleTag, slotKey, 1), roleTag);
    }

    public static PrototypeRpgEquipmentDefinition GetSlotUpgradeDefinition(
        string roleTag,
        string slotKey,
        bool eliteDefeated,
        string routeId,
        int openedChestCount,
        int currentTierRank)
    {
        int targetTierRank = ResolveRewardTierRank(eliteDefeated, routeId, openedChestCount);
        if (targetTierRank <= currentTierRank)
        {
            targetTierRank = currentTierRank + 1;
        }

        if (targetTierRank > 3)
        {
            return null;
        }

        return ResolveDefinition(BuildSlotItemId(roleTag, slotKey, targetTierRank), roleTag);
    }

    public static string[] GetRewardSlotPriority(string roleTag)
    {
        switch (Normalize(roleTag))
        {
            case "warrior":
                return new[]
                {
                    PrototypeRpgEquipmentSlotKeys.RightArm,
                    PrototypeRpgEquipmentSlotKeys.Torso,
                    PrototypeRpgEquipmentSlotKeys.Head,
                    PrototypeRpgEquipmentSlotKeys.LeftArm,
                    PrototypeRpgEquipmentSlotKeys.Belt,
                    PrototypeRpgEquipmentSlotKeys.Pants,
                    PrototypeRpgEquipmentSlotKeys.Shoes
                };

            case "rogue":
                return new[]
                {
                    PrototypeRpgEquipmentSlotKeys.RightArm,
                    PrototypeRpgEquipmentSlotKeys.LeftArm,
                    PrototypeRpgEquipmentSlotKeys.Shoes,
                    PrototypeRpgEquipmentSlotKeys.Head,
                    PrototypeRpgEquipmentSlotKeys.Belt,
                    PrototypeRpgEquipmentSlotKeys.Torso,
                    PrototypeRpgEquipmentSlotKeys.Pants
                };

            case "mage":
                return new[]
                {
                    PrototypeRpgEquipmentSlotKeys.RightArm,
                    PrototypeRpgEquipmentSlotKeys.Head,
                    PrototypeRpgEquipmentSlotKeys.Belt,
                    PrototypeRpgEquipmentSlotKeys.LeftArm,
                    PrototypeRpgEquipmentSlotKeys.Torso,
                    PrototypeRpgEquipmentSlotKeys.Shoes,
                    PrototypeRpgEquipmentSlotKeys.Pants
                };

            case "cleric":
                return new[]
                {
                    PrototypeRpgEquipmentSlotKeys.LeftArm,
                    PrototypeRpgEquipmentSlotKeys.Torso,
                    PrototypeRpgEquipmentSlotKeys.Head,
                    PrototypeRpgEquipmentSlotKeys.Belt,
                    PrototypeRpgEquipmentSlotKeys.RightArm,
                    PrototypeRpgEquipmentSlotKeys.Pants,
                    PrototypeRpgEquipmentSlotKeys.Shoes
                };

            default:
                return PrototypeRpgEquipmentSlotKeys.OrderedKeys;
        }
    }

    public static bool IsLegalForRole(PrototypeRpgEquipmentDefinition definition, string roleTag)
    {
        return definition != null &&
               (string.IsNullOrEmpty(definition.RoleTag) || Normalize(definition.RoleTag) == Normalize(roleTag));
    }

    public static string BuildEquipmentSummaryText(PrototypeRpgEquipmentDefinition definition)
    {
        if (definition == null)
        {
            return "No gear";
        }

        return definition.DisplayName + " (" + definition.TierLabel + " " + definition.SlotLabel + ")";
    }

    public static string BuildStatContributionSummary(PrototypeRpgEquipmentDefinition definition)
    {
        return definition == null
            ? "No bonus"
            : BuildStatContributionSummary(
                definition.MaxHpBonus,
                definition.AttackBonus,
                definition.DefenseBonus,
                definition.SpeedBonus,
                definition.SkillPowerBonus);
    }

    public static string BuildStatContributionSummary(
        int maxHpBonus,
        int attackBonus,
        int defenseBonus,
        int speedBonus,
        int skillPowerBonus)
    {
        string summary = BuildSignedSegment("HP", maxHpBonus);
        summary = AppendSegment(summary, BuildSignedSegment("ATK", attackBonus));
        summary = AppendSegment(summary, BuildSignedSegment("DEF", defenseBonus));
        summary = AppendSegment(summary, BuildSignedSegment("SPD", speedBonus));
        summary = AppendSegment(summary, BuildSignedSegment("POW", skillPowerBonus));
        return string.IsNullOrEmpty(summary) ? "No bonus" : summary;
    }

    public static bool HasMeaningfulStatContributionSummary(string statContributionSummary)
    {
        return !string.IsNullOrWhiteSpace(statContributionSummary) &&
               !string.Equals(statContributionSummary.Trim(), "No bonus", StringComparison.OrdinalIgnoreCase);
    }

    public static string ExtractDisplayName(string equipmentSummaryText)
    {
        if (string.IsNullOrWhiteSpace(equipmentSummaryText))
        {
            return "No gear";
        }

        string trimmed = equipmentSummaryText.Trim();
        if (string.Equals(trimmed, "No gear", StringComparison.OrdinalIgnoreCase))
        {
            return "No gear";
        }

        int markerIndex = trimmed.IndexOf(" (", StringComparison.Ordinal);
        return markerIndex > 0 ? trimmed.Substring(0, markerIndex).Trim() : trimmed;
    }

    public static string BuildCompactReadbackText(string equipmentSummaryText, string gearContributionSummaryText)
    {
        string displayName = ExtractDisplayName(equipmentSummaryText);
        if (string.Equals(displayName, "No gear", StringComparison.OrdinalIgnoreCase))
        {
            return displayName;
        }

        return HasMeaningfulStatContributionSummary(gearContributionSummaryText)
            ? displayName + " (" + gearContributionSummaryText.Trim() + ")"
            : displayName;
    }

    public static string BuildCompactReadbackText(PrototypeRpgEquipmentDefinition definition)
    {
        return definition == null
            ? "No gear"
            : BuildCompactReadbackText(BuildEquipmentSummaryText(definition), BuildStatContributionSummary(definition));
    }

    public static int ResolveRewardTierRank(bool eliteDefeated, string routeId, int openedChestCount)
    {
        if (eliteDefeated || Normalize(routeId) == "risky" || openedChestCount > 0)
        {
            return 3;
        }

        return 2;
    }

    private static PrototypeRpgEquipmentDefinition[] BuildDefinitions()
    {
        List<PrototypeRpgEquipmentDefinition> definitions = new List<PrototypeRpgEquipmentDefinition>();
        AddLegacyLoadoutDefinitions(definitions);
        AddRoleSlotDefinitions(definitions, "warrior");
        AddRoleSlotDefinitions(definitions, "rogue");
        AddRoleSlotDefinitions(definitions, "mage");
        AddRoleSlotDefinitions(definitions, "cleric");
        return definitions.ToArray();
    }

    private static void AddLegacyLoadoutDefinitions(List<PrototypeRpgEquipmentDefinition> definitions)
    {
        definitions.Add(new PrototypeRpgEquipmentDefinition("equip_warrior_placeholder", "Recruit Harness", "warrior", PrototypeRpgEquipmentSlotKeys.Torso, "Armor", "legacy", "Base", 1, 0, 0, 0, 0, 0));
        definitions.Add(new PrototypeRpgEquipmentDefinition("equip_rogue_placeholder", "Scout Wraps", "rogue", PrototypeRpgEquipmentSlotKeys.Torso, "Blades", "legacy", "Base", 1, 0, 0, 0, 0, 0));
        definitions.Add(new PrototypeRpgEquipmentDefinition("equip_mage_placeholder", "Apprentice Focus", "mage", PrototypeRpgEquipmentSlotKeys.RightArm, "Catalyst", "legacy", "Base", 1, 0, 0, 0, 0, 0));
        definitions.Add(new PrototypeRpgEquipmentDefinition("equip_cleric_placeholder", "Pilgrim Vestments", "cleric", PrototypeRpgEquipmentSlotKeys.Torso, "Relic", "legacy", "Base", 1, 0, 0, 0, 0, 0));

        definitions.Add(new PrototypeRpgEquipmentDefinition("equip_warrior_fieldkit", "Bulwark Harness", "warrior", PrototypeRpgEquipmentSlotKeys.Torso, "Armor", "legacy", "Field", 2, 4, 0, 1, 0, 0));
        definitions.Add(new PrototypeRpgEquipmentDefinition("equip_rogue_fieldkit", "Shadow Fang Set", "rogue", PrototypeRpgEquipmentSlotKeys.RightArm, "Blades", "legacy", "Field", 2, 0, 1, 0, 1, 0));
        definitions.Add(new PrototypeRpgEquipmentDefinition("equip_mage_fieldkit", "Stormglass Focus", "mage", PrototypeRpgEquipmentSlotKeys.RightArm, "Catalyst", "legacy", "Field", 2, 0, 1, 0, 1, 1));
        definitions.Add(new PrototypeRpgEquipmentDefinition("equip_cleric_fieldkit", "Sanctum Vestments", "cleric", PrototypeRpgEquipmentSlotKeys.Torso, "Relic", "legacy", "Field", 2, 3, 0, 1, 0, 1));

        definitions.Add(new PrototypeRpgEquipmentDefinition("equip_warrior_elite", "Vanguard Bulwark", "warrior", PrototypeRpgEquipmentSlotKeys.Torso, "Armor", "legacy", "Elite", 3, 6, 1, 2, 0, 0));
        definitions.Add(new PrototypeRpgEquipmentDefinition("equip_rogue_elite", "Nightblade Array", "rogue", PrototypeRpgEquipmentSlotKeys.RightArm, "Blades", "legacy", "Elite", 3, 0, 2, 0, 2, 0));
        definitions.Add(new PrototypeRpgEquipmentDefinition("equip_mage_elite", "Emberwake Focus", "mage", PrototypeRpgEquipmentSlotKeys.RightArm, "Catalyst", "legacy", "Elite", 3, 0, 3, 0, 1, 2));
        definitions.Add(new PrototypeRpgEquipmentDefinition("equip_cleric_elite", "Sunwell Relic", "cleric", PrototypeRpgEquipmentSlotKeys.LeftArm, "Relic", "legacy", "Elite", 3, 4, 1, 2, 0, 2));
    }

    private static void AddRoleSlotDefinitions(List<PrototypeRpgEquipmentDefinition> definitions, string roleTag)
    {
        string[] orderedKeys = PrototypeRpgEquipmentSlotKeys.OrderedKeys;
        for (int i = 0; i < orderedKeys.Length; i++)
        {
            string slotKey = orderedKeys[i];
            definitions.Add(CreateRoleSlotDefinition(roleTag, slotKey, 1));
            definitions.Add(CreateRoleSlotDefinition(roleTag, slotKey, 2));
            definitions.Add(CreateRoleSlotDefinition(roleTag, slotKey, 3));
        }
    }

    private static PrototypeRpgEquipmentDefinition CreateRoleSlotDefinition(string roleTag, string slotKey, int tierRank)
    {
        ResolveRoleSlotBonuses(roleTag, slotKey, tierRank, out int maxHpBonus, out int attackBonus, out int defenseBonus, out int speedBonus, out int skillPowerBonus);
        return new PrototypeRpgEquipmentDefinition(
            BuildSlotItemId(roleTag, slotKey, tierRank),
            ResolveRoleSlotDisplayName(roleTag, slotKey, tierRank),
            Normalize(roleTag),
            Normalize(slotKey),
            PrototypeRpgEquipmentSlotKeys.ToDisplayLabel(slotKey),
            ResolveTierKey(tierRank),
            ResolveTierLabel(tierRank),
            tierRank,
            maxHpBonus,
            attackBonus,
            defenseBonus,
            speedBonus,
            skillPowerBonus);
    }

    private static string BuildSlotItemId(string roleTag, string slotKey, int tierRank)
    {
        return "gear_" + Normalize(roleTag) + "_" + Normalize(slotKey) + "_" + ResolveTierKey(tierRank);
    }

    private static string ResolveRoleSlotDisplayName(string roleTag, string slotKey, int tierRank)
    {
        return ResolveRoleSlotPrefix(roleTag, tierRank) + " " + ResolveRoleSlotNoun(roleTag, slotKey);
    }

    private static string ResolveRoleSlotPrefix(string roleTag, int tierRank)
    {
        switch (Normalize(roleTag))
        {
            case "warrior":
                return tierRank >= 3 ? "Vanguard" : tierRank == 2 ? "Bulwark" : "Recruit";
            case "rogue":
                return tierRank >= 3 ? "Night" : tierRank == 2 ? "Shadow" : "Scout";
            case "mage":
                return tierRank >= 3 ? "Emberwake" : tierRank == 2 ? "Stormglass" : "Apprentice";
            case "cleric":
                return tierRank >= 3 ? "Sunwell" : tierRank == 2 ? "Sanctum" : "Pilgrim";
            default:
                return tierRank >= 3 ? "Veteran" : tierRank == 2 ? "Field" : "Starter";
        }
    }

    private static string ResolveRoleSlotNoun(string roleTag, string slotKey)
    {
        string normalizedRoleTag = Normalize(roleTag);
        switch (Normalize(slotKey))
        {
            case PrototypeRpgEquipmentSlotKeys.Head:
                return normalizedRoleTag == "mage" ? "Circlet" : normalizedRoleTag == "cleric" ? "Hood" : normalizedRoleTag == "rogue" ? "Hood" : "Visor";
            case PrototypeRpgEquipmentSlotKeys.LeftArm:
                return normalizedRoleTag == "warrior" ? "Shield" : normalizedRoleTag == "rogue" ? "Dagger" : normalizedRoleTag == "mage" ? "Charm" : "Relic";
            case PrototypeRpgEquipmentSlotKeys.RightArm:
                return normalizedRoleTag == "warrior" ? "Blade" : normalizedRoleTag == "rogue" ? "Blade" : normalizedRoleTag == "mage" ? "Focus" : "Mace";
            case PrototypeRpgEquipmentSlotKeys.Torso:
                return normalizedRoleTag == "rogue" ? "Coat" : normalizedRoleTag == "mage" ? "Robe" : normalizedRoleTag == "cleric" ? "Vestments" : "Harness";
            case PrototypeRpgEquipmentSlotKeys.Belt:
                return normalizedRoleTag == "mage" ? "Sash" : normalizedRoleTag == "cleric" ? "Cord" : normalizedRoleTag == "rogue" ? "Harness" : "Warbelt";
            case PrototypeRpgEquipmentSlotKeys.Pants:
                return normalizedRoleTag == "cleric" ? "Legwraps" : "Trousers";
            case PrototypeRpgEquipmentSlotKeys.Shoes:
                return normalizedRoleTag == "mage" ? "Sandals" : normalizedRoleTag == "rogue" ? "Soles" : "Boots";
            default:
                return "Gear";
        }
    }

    private static void ResolveRoleSlotBonuses(
        string roleTag,
        string slotKey,
        int tierRank,
        out int maxHpBonus,
        out int attackBonus,
        out int defenseBonus,
        out int speedBonus,
        out int skillPowerBonus)
    {
        int multiplier = tierRank <= 1 ? 0 : tierRank == 2 ? 1 : 2;
        maxHpBonus = 0;
        attackBonus = 0;
        defenseBonus = 0;
        speedBonus = 0;
        skillPowerBonus = 0;

        switch (Normalize(roleTag))
        {
            case "warrior":
                switch (Normalize(slotKey))
                {
                    case PrototypeRpgEquipmentSlotKeys.Head:
                        maxHpBonus = 2 * multiplier;
                        defenseBonus = 1 * multiplier;
                        break;
                    case PrototypeRpgEquipmentSlotKeys.LeftArm:
                        defenseBonus = 2 * multiplier;
                        break;
                    case PrototypeRpgEquipmentSlotKeys.RightArm:
                        attackBonus = 2 * multiplier;
                        break;
                    case PrototypeRpgEquipmentSlotKeys.Torso:
                        maxHpBonus = 4 * multiplier;
                        defenseBonus = 1 * multiplier;
                        break;
                    case PrototypeRpgEquipmentSlotKeys.Belt:
                        maxHpBonus = 2 * multiplier;
                        attackBonus = 1 * multiplier;
                        break;
                    case PrototypeRpgEquipmentSlotKeys.Pants:
                        maxHpBonus = 2 * multiplier;
                        defenseBonus = 1 * multiplier;
                        break;
                    case PrototypeRpgEquipmentSlotKeys.Shoes:
                        defenseBonus = 1 * multiplier;
                        speedBonus = 1 * multiplier;
                        break;
                }
                break;

            case "rogue":
                switch (Normalize(slotKey))
                {
                    case PrototypeRpgEquipmentSlotKeys.Head:
                        attackBonus = 1 * multiplier;
                        speedBonus = 1 * multiplier;
                        break;
                    case PrototypeRpgEquipmentSlotKeys.LeftArm:
                        attackBonus = 1 * multiplier;
                        speedBonus = 1 * multiplier;
                        break;
                    case PrototypeRpgEquipmentSlotKeys.RightArm:
                        attackBonus = 2 * multiplier;
                        speedBonus = 1 * multiplier;
                        break;
                    case PrototypeRpgEquipmentSlotKeys.Torso:
                        maxHpBonus = 2 * multiplier;
                        speedBonus = 1 * multiplier;
                        break;
                    case PrototypeRpgEquipmentSlotKeys.Belt:
                        attackBonus = 1 * multiplier;
                        speedBonus = 1 * multiplier;
                        break;
                    case PrototypeRpgEquipmentSlotKeys.Pants:
                        attackBonus = 1 * multiplier;
                        speedBonus = 1 * multiplier;
                        break;
                    case PrototypeRpgEquipmentSlotKeys.Shoes:
                        speedBonus = 2 * multiplier;
                        break;
                }
                break;

            case "mage":
                switch (Normalize(slotKey))
                {
                    case PrototypeRpgEquipmentSlotKeys.Head:
                        attackBonus = 1 * multiplier;
                        skillPowerBonus = 1 * multiplier;
                        break;
                    case PrototypeRpgEquipmentSlotKeys.LeftArm:
                        defenseBonus = 1 * multiplier;
                        skillPowerBonus = 1 * multiplier;
                        break;
                    case PrototypeRpgEquipmentSlotKeys.RightArm:
                        attackBonus = 2 * multiplier;
                        skillPowerBonus = 1 * multiplier;
                        break;
                    case PrototypeRpgEquipmentSlotKeys.Torso:
                        maxHpBonus = 2 * multiplier;
                        attackBonus = 1 * multiplier;
                        break;
                    case PrototypeRpgEquipmentSlotKeys.Belt:
                        speedBonus = 1 * multiplier;
                        skillPowerBonus = 1 * multiplier;
                        break;
                    case PrototypeRpgEquipmentSlotKeys.Pants:
                        maxHpBonus = 1 * multiplier;
                        defenseBonus = 1 * multiplier;
                        break;
                    case PrototypeRpgEquipmentSlotKeys.Shoes:
                        speedBonus = 1 * multiplier;
                        skillPowerBonus = 1 * multiplier;
                        break;
                }
                break;

            case "cleric":
                switch (Normalize(slotKey))
                {
                    case PrototypeRpgEquipmentSlotKeys.Head:
                        maxHpBonus = 2 * multiplier;
                        defenseBonus = 1 * multiplier;
                        break;
                    case PrototypeRpgEquipmentSlotKeys.LeftArm:
                        maxHpBonus = 1 * multiplier;
                        defenseBonus = 1 * multiplier;
                        skillPowerBonus = 1 * multiplier;
                        break;
                    case PrototypeRpgEquipmentSlotKeys.RightArm:
                        attackBonus = 1 * multiplier;
                        skillPowerBonus = 1 * multiplier;
                        break;
                    case PrototypeRpgEquipmentSlotKeys.Torso:
                        maxHpBonus = 3 * multiplier;
                        defenseBonus = 1 * multiplier;
                        break;
                    case PrototypeRpgEquipmentSlotKeys.Belt:
                        maxHpBonus = 2 * multiplier;
                        skillPowerBonus = 1 * multiplier;
                        break;
                    case PrototypeRpgEquipmentSlotKeys.Pants:
                        maxHpBonus = 2 * multiplier;
                        defenseBonus = 1 * multiplier;
                        break;
                    case PrototypeRpgEquipmentSlotKeys.Shoes:
                        defenseBonus = 1 * multiplier;
                        speedBonus = 1 * multiplier;
                        break;
                }
                break;
        }
    }

    private static string ResolveTierKey(int tierRank)
    {
        return tierRank >= 3 ? "elite" : tierRank == 2 ? "field" : "starter";
    }

    private static string ResolveTierLabel(int tierRank)
    {
        return tierRank >= 3 ? "Elite" : tierRank == 2 ? "Field" : "Starter";
    }

    private static string AppendSegment(string existing, string segment)
    {
        if (string.IsNullOrEmpty(segment))
        {
            return existing;
        }

        return string.IsNullOrEmpty(existing) ? segment : existing + " " + segment;
    }

    private static string BuildSignedSegment(string label, int value)
    {
        if (value == 0)
        {
            return string.Empty;
        }

        return label + " " + (value > 0 ? "+" : string.Empty) + value;
    }

    private static string Normalize(string value)
    {
        return string.IsNullOrWhiteSpace(value) ? string.Empty : value.Trim().ToLowerInvariant();
    }
}
