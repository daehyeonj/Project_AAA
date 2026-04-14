using System;

public sealed class PrototypeRpgEquipmentDefinition
{
    public string LoadoutId { get; }
    public string DisplayName { get; }
    public string RoleTag { get; }
    public string SlotLabel { get; }
    public string TierLabel { get; }
    public int TierRank { get; }
    public int MaxHpBonus { get; }
    public int AttackBonus { get; }
    public int DefenseBonus { get; }
    public int SpeedBonus { get; }

    public PrototypeRpgEquipmentDefinition(
        string loadoutId,
        string displayName,
        string roleTag,
        string slotLabel,
        string tierLabel,
        int tierRank,
        int maxHpBonus,
        int attackBonus,
        int defenseBonus,
        int speedBonus)
    {
        LoadoutId = string.IsNullOrWhiteSpace(loadoutId) ? string.Empty : loadoutId.Trim();
        DisplayName = string.IsNullOrWhiteSpace(displayName) ? "Gear" : displayName.Trim();
        RoleTag = string.IsNullOrWhiteSpace(roleTag) ? string.Empty : roleTag.Trim().ToLowerInvariant();
        SlotLabel = string.IsNullOrWhiteSpace(slotLabel) ? "Loadout" : slotLabel.Trim();
        TierLabel = string.IsNullOrWhiteSpace(tierLabel) ? "Base" : tierLabel.Trim();
        TierRank = tierRank > 0 ? tierRank : 1;
        MaxHpBonus = maxHpBonus;
        AttackBonus = attackBonus;
        DefenseBonus = defenseBonus;
        SpeedBonus = speedBonus;
    }

    public int GetScore()
    {
        return (MaxHpBonus * 2) + (AttackBonus * 4) + (DefenseBonus * 3) + (SpeedBonus * 3) + TierRank;
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
    private static readonly PrototypeRpgEquipmentDefinition[] Definitions =
    {
        new PrototypeRpgEquipmentDefinition("equip_warrior_placeholder", "Recruit Harness", "warrior", "Armor", "Base", 1, 0, 0, 0, 0),
        new PrototypeRpgEquipmentDefinition("equip_rogue_placeholder", "Scout Wraps", "rogue", "Blades", "Base", 1, 0, 0, 0, 0),
        new PrototypeRpgEquipmentDefinition("equip_mage_placeholder", "Apprentice Focus", "mage", "Catalyst", "Base", 1, 0, 0, 0, 0),
        new PrototypeRpgEquipmentDefinition("equip_cleric_placeholder", "Pilgrim Vestments", "cleric", "Relic", "Base", 1, 0, 0, 0, 0),

        new PrototypeRpgEquipmentDefinition("equip_warrior_fieldkit", "Bulwark Harness", "warrior", "Armor", "Field", 2, 4, 0, 1, 0),
        new PrototypeRpgEquipmentDefinition("equip_rogue_fieldkit", "Shadow Fang Set", "rogue", "Blades", "Field", 2, 0, 1, 0, 1),
        new PrototypeRpgEquipmentDefinition("equip_mage_fieldkit", "Stormglass Focus", "mage", "Catalyst", "Field", 2, 0, 1, 0, 1),
        new PrototypeRpgEquipmentDefinition("equip_cleric_fieldkit", "Sanctum Vestments", "cleric", "Relic", "Field", 2, 3, 0, 1, 0),

        new PrototypeRpgEquipmentDefinition("equip_warrior_elite", "Vanguard Bulwark", "warrior", "Armor", "Elite", 3, 6, 1, 2, 0),
        new PrototypeRpgEquipmentDefinition("equip_rogue_elite", "Nightblade Array", "rogue", "Blades", "Elite", 3, 0, 2, 0, 2),
        new PrototypeRpgEquipmentDefinition("equip_mage_elite", "Emberwake Focus", "mage", "Catalyst", "Elite", 3, 0, 3, 0, 1),
        new PrototypeRpgEquipmentDefinition("equip_cleric_elite", "Sunwell Relic", "cleric", "Relic", "Elite", 3, 4, 1, 2, 0)
    };

    public static PrototypeRpgEquipmentDefinition ResolveDefinition(string loadoutId, string roleTag)
    {
        string normalizedLoadoutId = Normalize(loadoutId);
        string normalizedRoleTag = Normalize(roleTag);

        for (int i = 0; i < Definitions.Length; i++)
        {
            PrototypeRpgEquipmentDefinition definition = Definitions[i];
            if (definition != null && Normalize(definition.LoadoutId) == normalizedLoadoutId)
            {
                return definition;
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

    public static PrototypeRpgEquipmentDefinition GetRewardDefinition(string currentLoadoutId, string roleTag, bool eliteDefeated, string routeId, int openedChestCount)
    {
        string normalizedRoleTag = Normalize(roleTag);
        PrototypeRpgEquipmentDefinition currentDefinition = ResolveDefinition(currentLoadoutId, normalizedRoleTag);
        bool highPressureReward = eliteDefeated || Normalize(routeId) == "risky" || openedChestCount > 0;
        bool alreadyFieldTier = currentDefinition != null && currentDefinition.TierRank >= 2;
        string targetId;

        if (highPressureReward && alreadyFieldTier)
        {
            targetId = normalizedRoleTag switch
            {
                "warrior" => "equip_warrior_elite",
                "rogue" => "equip_rogue_elite",
                "mage" => "equip_mage_elite",
                "cleric" => "equip_cleric_elite",
                _ => string.Empty
            };
        }
        else if (highPressureReward && currentDefinition != null && currentDefinition.TierRank < 2)
        {
            targetId = normalizedRoleTag switch
            {
                "warrior" => "equip_warrior_elite",
                "rogue" => "equip_rogue_elite",
                "mage" => "equip_mage_elite",
                "cleric" => "equip_cleric_elite",
                _ => string.Empty
            };
        }
        else
        {
            targetId = normalizedRoleTag switch
            {
                "warrior" => "equip_warrior_fieldkit",
                "rogue" => "equip_rogue_fieldkit",
                "mage" => "equip_mage_fieldkit",
                "cleric" => "equip_cleric_fieldkit",
                _ => string.Empty
            };
        }

        return ResolveDefinition(targetId, normalizedRoleTag);
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
        if (definition == null)
        {
            return "No bonus";
        }

        string summary = BuildSignedSegment("HP", definition.MaxHpBonus);
        summary = AppendSegment(summary, BuildSignedSegment("ATK", definition.AttackBonus));
        summary = AppendSegment(summary, BuildSignedSegment("DEF", definition.DefenseBonus));
        summary = AppendSegment(summary, BuildSignedSegment("SPD", definition.SpeedBonus));
        return string.IsNullOrEmpty(summary) ? "No bonus" : summary;
    }

    public static string BuildComparisonSummary(PrototypeRpgEquipmentDefinition currentDefinition, PrototypeRpgEquipmentDefinition rewardDefinition)
    {
        if (rewardDefinition == null)
        {
            return "No comparison";
        }

        int hpDelta = rewardDefinition.MaxHpBonus - (currentDefinition != null ? currentDefinition.MaxHpBonus : 0);
        int attackDelta = rewardDefinition.AttackBonus - (currentDefinition != null ? currentDefinition.AttackBonus : 0);
        int defenseDelta = rewardDefinition.DefenseBonus - (currentDefinition != null ? currentDefinition.DefenseBonus : 0);
        int speedDelta = rewardDefinition.SpeedBonus - (currentDefinition != null ? currentDefinition.SpeedBonus : 0);
        string summary = BuildSignedSegment("HP", hpDelta);
        summary = AppendSegment(summary, BuildSignedSegment("ATK", attackDelta));
        summary = AppendSegment(summary, BuildSignedSegment("DEF", defenseDelta));
        summary = AppendSegment(summary, BuildSignedSegment("SPD", speedDelta));
        return string.IsNullOrEmpty(summary) ? "No change" : summary;
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
