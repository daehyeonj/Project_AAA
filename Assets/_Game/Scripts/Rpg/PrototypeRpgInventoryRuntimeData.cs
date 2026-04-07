using System;

public sealed class PrototypeRpgConsumableDefinition
{
    public string ItemId { get; }
    public string DisplayName { get; }
    public string SummaryText { get; }
    public bool IsConsumable { get; }
    public int StackLimit { get; }
    public bool CanUseInBattle { get; }
    public string BattleEffectType { get; }
    public string TargetKind { get; }
    public int PowerValue { get; }
    public int RecommendedLevel { get; }

    public PrototypeRpgConsumableDefinition(string itemId, string displayName, string summaryText, bool isConsumable, int stackLimit, bool canUseInBattle, string battleEffectType, string targetKind, int powerValue, int recommendedLevel)
    {
        ItemId = NormalizeKey(itemId);
        DisplayName = string.IsNullOrWhiteSpace(displayName) ? "Item" : displayName.Trim();
        SummaryText = string.IsNullOrWhiteSpace(summaryText) ? string.Empty : summaryText.Trim();
        IsConsumable = isConsumable;
        StackLimit = stackLimit > 0 ? stackLimit : 1;
        CanUseInBattle = canUseInBattle;
        BattleEffectType = NormalizeKey(battleEffectType);
        TargetKind = NormalizeKey(targetKind);
        PowerValue = powerValue > 0 ? powerValue : 1;
        RecommendedLevel = recommendedLevel > 0 ? recommendedLevel : 1;
    }

    private static string NormalizeKey(string value)
    {
        return string.IsNullOrWhiteSpace(value) ? string.Empty : value.Trim().ToLowerInvariant();
    }
}

public static class PrototypeRpgConsumableCatalog
{
    private static readonly PrototypeRpgConsumableDefinition[] SharedDefinitions =
    {
        new PrototypeRpgConsumableDefinition("consumable_field_tonic", "Field Tonic", "Placeholder tonic that stabilizes a rough fight.", true, 9, true, "heal", "all_allies", 3, 1),
        new PrototypeRpgConsumableDefinition("consumable_smoke_ward", "Smoke Ward", "Placeholder escape utility for risky runs.", true, 9, false, "retreat_support", "party", 0, 1),
        new PrototypeRpgConsumableDefinition("consumable_arcane_tonic", "Arcane Tonic", "Placeholder self-recovery tonic for arcane pressure.", true, 9, true, "heal", "self", 2, 1),
        new PrototypeRpgConsumableDefinition("consumable_burst_flask", "Burst Flask", "Placeholder offensive kit that splashes all enemies.", true, 9, true, "damage", "all_enemies", 3, 2),
        new PrototypeRpgConsumableDefinition("consumable_safeguard_kit", "Safeguard Kit", "Placeholder rescue kit that reinforces the whole party.", true, 9, true, "protect_heal", "all_allies", 4, 2)
    };

    public static PrototypeRpgConsumableDefinition GetDefinition(string itemId)
    {
        string normalizedItemId = string.IsNullOrWhiteSpace(itemId) ? string.Empty : itemId.Trim().ToLowerInvariant();
        if (string.IsNullOrEmpty(normalizedItemId))
        {
            return null;
        }

        for (int i = 0; i < SharedDefinitions.Length; i++)
        {
            PrototypeRpgConsumableDefinition definition = SharedDefinitions[i];
            if (definition != null && definition.ItemId == normalizedItemId)
            {
                return definition;
            }
        }

        return null;
    }
}

public sealed class PrototypeRpgInventoryEntry
{
    public string ItemId = string.Empty;
    public string DisplayName = string.Empty;
    public int Quantity;
    public string SourceKey = string.Empty;
    public string SummaryText = string.Empty;
}

public sealed class PrototypeRpgEquippedLoadoutEntry
{
    public string MemberId = string.Empty;
    public string MemberDisplayName = string.Empty;
    public string LoadoutId = string.Empty;
    public string DisplayName = string.Empty;
    public string SlotKey = string.Empty;
    public int MaxHpDelta;
    public int AttackDelta;
    public int DefenseDelta;
    public int SpeedDelta;
    public string PassiveHintText = string.Empty;
    public string BattleLabelHint = string.Empty;
    public string SummaryText = string.Empty;
    public string GearContributionSummaryText = string.Empty;
}

public sealed class PrototypeRpgRewardCarryEntry
{
    public string ResourceId = string.Empty;
    public string ResourceLabel = string.Empty;
    public int Amount;
    public string SourceKey = string.Empty;
    public string RunIdentity = string.Empty;
    public string SummaryText = string.Empty;
}

public sealed class PrototypeRpgBattleConsumableSlot
{
    public string SlotKey = string.Empty;
    public string ItemId = string.Empty;
    public string DisplayName = string.Empty;
    public int Quantity;
    public bool IsAvailable;
    public string EffectType = string.Empty;
    public string TargetKind = string.Empty;
    public int PowerValue;
    public int RecommendedLevel = 1;
    public string SummaryText = string.Empty;
}

public sealed class PrototypeRpgInventoryRuntimeState
{
    public string PartyId = string.Empty;
    public string DisplayName = string.Empty;
    public string LastRunIdentity = string.Empty;
    public int TotalConsumableCount;
    public int TotalEquippedLoadoutCount;
    public int TotalCarryRewardAmount;
    public int ConsumablesUsedThisRun;
    public int DepletedConsumableCount;
    public string EquipmentSummaryText = string.Empty;
    public string InventorySummaryText = string.Empty;
    public string BattleConsumableSlotSummaryText = string.Empty;
    public string LastConsumableUseSummaryText = string.Empty;
    public string ConsumedThisRunSummaryText = string.Empty;
    public string CarryRewardSummaryText = string.Empty;
    public string CarryoverPolicySummaryText = string.Empty;
    public string ReplenishSummaryText = string.Empty;
    public string DepletedSummaryText = string.Empty;
    public string NextRunCarrySummaryText = string.Empty;
    public string LastResolvedConsumableId = string.Empty;
    public PrototypeRpgEquippedLoadoutEntry[] EquippedLoadouts = Array.Empty<PrototypeRpgEquippedLoadoutEntry>();
    public PrototypeRpgInventoryEntry[] Consumables = Array.Empty<PrototypeRpgInventoryEntry>();
    public PrototypeRpgRewardCarryEntry[] CarryEntries = Array.Empty<PrototypeRpgRewardCarryEntry>();
    public PrototypeRpgBattleConsumableSlot[] BattleConsumableSlots = Array.Empty<PrototypeRpgBattleConsumableSlot>();
}