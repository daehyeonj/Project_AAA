using System;

public sealed class PrototypeRpgInventoryMemberSurfaceData
{
    public string MemberId = string.Empty;
    public string DisplayName = "Adventurer";
    public string RoleLabel = "Adventurer";
    public string RoleIdentityText = "Flexible role";
    public string GearPreferenceText = "Wants balanced gear";
    public int Level = 1;
    public int CurrentExperience;
    public int NextLevelExperience = 18;
    public string BaseStatsText = "None";
    public string LevelBonusText = "None";
    public string GearBonusText = "None";
    public string ResolvedStatsText = "None";
    public string SummaryText = "None";
    public bool IsSelected;
}

public sealed class PrototypeRpgInventorySlotSurfaceData
{
    public string SlotKey = string.Empty;
    public string SlotLabel = "Gear";
    public string HotkeyLabel = string.Empty;
    public string EquippedItemInstanceId = string.Empty;
    public string EquippedItemName = "Empty";
    public string EquippedItemSummaryText = "Empty";
    public string StatBonusText = "No bonus";
    public bool HasEquippedItem;
    public bool IsSelected;
}

public sealed class PrototypeRpgInventoryItemSurfaceData
{
    public string ItemInstanceId = string.Empty;
    public string DisplayName = "Gear";
    public string SlotKey = string.Empty;
    public string SlotLabel = "Gear";
    public string TierLabel = "Starter";
    public string StatBonusText = "No bonus";
    public string SourceSummaryText = "None";
    public string EquippedBySummaryText = "Stored";
    public string ScoreSummaryText = "Score 0";
    public string SummaryText = "None";
    public bool IsCompatible;
    public bool IsEquipped;
    public bool IsSelected;
}

public sealed class PrototypeRpgInventoryComparisonSurfaceData
{
    public string CurrentText = "Current: Empty";
    public string CandidateText = "Candidate: None";
    public string DeltaText = "No stat change";
    public string ActionHintText = "Enter Equip / U Unequip / Esc Close";
    public string ReasonText = "Select an item.";
}

public sealed class PrototypeRpgInventorySurfaceData
{
    public bool IsOpen;
    public bool IsReadOnly;
    public bool CanEquipSelectedItem;
    public bool CanUnequipSelectedSlot;
    public string ContextLabel = "Inventory";
    public string PanelTitleText = "Character Equipment";
    public string PartyId = string.Empty;
    public string PartyLabel = "None";
    public string SelectedMemberId = string.Empty;
    public string SelectedSlotKey = string.Empty;
    public string SelectedItemInstanceId = string.Empty;
    public string InputHintText = "[I] Toggle | [Q/E] Member | [1-7] Slot | [Up/Down] Item | [Enter] Equip | [U] Unequip | [Esc] Close";
    public string FooterSummaryText = "None";
    public string InventorySummaryText = "Owned 0 | Equipped 0 | Stored 0";
    public string LatestRewardSummaryText = "None";
    public string LatestAutoEquipSummaryText = "None";
    public string LatestContinuitySummaryText = "None";
    public string LatestManualActionSummaryText = "None";
    public string FeedbackText = "None";
    public string SelectedMemberHeaderText = "None";
    public string SelectedMemberProgressText = "None";
    public string SelectedMemberRoleText = "None";
    public string SelectedMemberGearPreferenceText = "None";
    public string SelectedItemHeaderText = "None";
    public string SelectedItemMetaText = "None";
    public string SelectedItemDetailText = "None";
    public string SelectedItemFitText = "None";
    public string SelectedItemOwnerText = "None";
    public PrototypeRpgInventoryMemberSurfaceData[] Members = Array.Empty<PrototypeRpgInventoryMemberSurfaceData>();
    public PrototypeRpgInventorySlotSurfaceData[] Slots = Array.Empty<PrototypeRpgInventorySlotSurfaceData>();
    public PrototypeRpgInventoryItemSurfaceData[] InventoryItems = Array.Empty<PrototypeRpgInventoryItemSurfaceData>();
    public PrototypeRpgInventoryComparisonSurfaceData Comparison = new PrototypeRpgInventoryComparisonSurfaceData();
}
