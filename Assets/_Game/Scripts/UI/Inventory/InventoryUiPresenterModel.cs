using System;

[Serializable]
public sealed class InventoryUiPresenterModel
{
    [Serializable]
    public sealed class Member
    {
        public string Name = "None";
        public string Role = "None";
        public int Level;
        public bool Selected;
    }

    [Serializable]
    public sealed class EquipmentSlot
    {
        public string SlotLabel = "Slot";
        public string ItemLabel = "No gear";
        public bool HasItem;
        public bool Selected;
    }

    [Serializable]
    public sealed class InventoryItem
    {
        public string ItemLabel = "Item";
        public string DetailText = "None";
        public bool Selected;
    }

    public string HeaderTitle = "Inventory";
    public string PartyLabel = "None";
    public string SummaryText = "None";
    public string RunSpoilsBadgeText = "None";
    public string PendingExtractionBadgeText = "None";
    public string SelectedItemDetailText = "None";
    public Member[] Members = Array.Empty<Member>();
    public EquipmentSlot[] Slots = Array.Empty<EquipmentSlot>();
    public InventoryItem[] Items = Array.Empty<InventoryItem>();
}
