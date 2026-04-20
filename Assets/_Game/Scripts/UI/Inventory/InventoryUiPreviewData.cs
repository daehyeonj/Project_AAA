using System;
using UnityEngine;

[CreateAssetMenu(menuName = "Project AAA/UI/Inventory UI Preview Data", fileName = "InventoryUiPreview")]
public sealed class InventoryUiPreviewData : ScriptableObject
{
    [Serializable]
    public sealed class Member
    {
        public string Name = "Alden";
        public string Role = "Anchor";
        public int Level = 4;
        public bool Selected;
    }

    [Serializable]
    public sealed class EquipmentSlot
    {
        public string SlotLabel = "Head";
        public string ItemLabel = "No gear";
        public bool HasItem;
        public bool Selected;
    }

    [Serializable]
    public sealed class InventoryItem
    {
        public string ItemLabel = "Iron Bulwark Helm";
        public string DetailText = "HP +3 | DEF +2 | Safe frontline upgrade.";
        public bool Selected;
    }

    public string HeaderTitle = "Character Equipment";
    public string PartyLabel = "Bulwark Crew";
    public string SummaryText = "4 members | 6 items";
    public string RunSpoilsBadgeText = "Run Spoils +3 pending extraction";
    public string PendingExtractionBadgeText = "Pending Extraction";
    [TextArea(3, 8)]
    public string SelectedItemDetailText = "Iron Bulwark Helm\nHP +3 | DEF +2\nHeld for Alden after extraction.";

    public Member[] Members =
    {
        new Member { Name = "Alden", Role = "Anchor", Level = 4, Selected = true },
        new Member { Name = "Mira", Role = "Rogue", Level = 4 },
        new Member { Name = "Rune", Role = "Mage", Level = 4 },
        new Member { Name = "Lia", Role = "Cleric", Level = 4 }
    };

    public EquipmentSlot[] Slots =
    {
        new EquipmentSlot { SlotLabel = "Head", ItemLabel = "Iron Bulwark Helm", HasItem = true, Selected = true },
        new EquipmentSlot { SlotLabel = "Left Arm", ItemLabel = "Guard Buckler", HasItem = true },
        new EquipmentSlot { SlotLabel = "Right Arm", ItemLabel = "Guild Blade", HasItem = true },
        new EquipmentSlot { SlotLabel = "Torso", ItemLabel = "Traveler Coat", HasItem = true },
        new EquipmentSlot { SlotLabel = "Belt", ItemLabel = "No gear" },
        new EquipmentSlot { SlotLabel = "Pants", ItemLabel = "Field Greaves", HasItem = true },
        new EquipmentSlot { SlotLabel = "Shoes", ItemLabel = "No gear" }
    };

    public InventoryItem[] Items =
    {
        new InventoryItem { ItemLabel = "Iron Bulwark Helm", DetailText = "HP +3 | DEF +2 | Safe frontline upgrade.", Selected = true },
        new InventoryItem { ItemLabel = "Traveler Gloves", DetailText = "SPD +1 | low-risk utility fit." },
        new InventoryItem { ItemLabel = "Mana Thread Sash", DetailText = "Skill +2 | backline-friendly item." },
        new InventoryItem { ItemLabel = "Field Tonic", DetailText = "Preview-only consumable row." }
    };
}
