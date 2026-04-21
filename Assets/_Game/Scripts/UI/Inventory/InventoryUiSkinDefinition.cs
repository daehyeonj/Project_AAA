using System;
using UnityEngine;

[CreateAssetMenu(menuName = "Project AAA/UI/Inventory UI Skin", fileName = "InventoryUiSkin")]
public sealed class InventoryUiSkinDefinition : ScriptableObject
{
    [Serializable]
    public sealed class GraphicSlot
    {
        [Tooltip("Optional sprite reference. Preferred when the source texture is imported as Sprite (2D and UI).")]
        public Sprite Sprite;

        [Tooltip("Preview-only texture fallback. Final skin authoring should prefer Sprite references.")]
        public Texture2D Texture;

        [Tooltip("Tint applied to the assigned sprite or texture.")]
        public Color Tint = Color.white;

        [Tooltip("When enabled, the renderer keeps the assigned graphic's aspect ratio inside the target rect.")]
        public bool PreserveAspect;

        public bool HasAssignedGraphic
        {
            get { return Sprite != null || Texture != null; }
        }
    }

    [Header("Panels")]
    [Tooltip("Inventory preview/root background.")]
    public GraphicSlot InventoryBackground = new GraphicSlot();

    [Tooltip("Inventory header panel background.")]
    public GraphicSlot HeaderPanel = new GraphicSlot();

    [Tooltip("Inventory footer panel background.")]
    public GraphicSlot FooterPanel = new GraphicSlot();

    [Tooltip("Selected item detail panel background.")]
    public GraphicSlot ItemDetailPanel = new GraphicSlot();

    [Header("Rows")]
    [Tooltip("Party member row background.")]
    public GraphicSlot MemberRow = new GraphicSlot();

    [Tooltip("Selected party member row background.")]
    public GraphicSlot MemberRowSelected = new GraphicSlot();

    [Tooltip("Inventory item row background.")]
    public GraphicSlot ItemRow = new GraphicSlot();

    [Tooltip("Selected inventory item row background.")]
    public GraphicSlot ItemRowSelected = new GraphicSlot();

    [Header("Equipment")]
    [Tooltip("Equipment slot background.")]
    public GraphicSlot EquipmentSlot = new GraphicSlot();

    [Tooltip("Empty equipment slot background.")]
    public GraphicSlot EquipmentSlotEmpty = new GraphicSlot();

    [Tooltip("Equipped equipment slot background.")]
    public GraphicSlot EquipmentSlotEquipped = new GraphicSlot();

    [Tooltip("Selected equipment slot background.")]
    public GraphicSlot EquipmentSlotSelected = new GraphicSlot();

    [Header("Badges")]
    [Tooltip("Run spoils badge background shown while extraction is still pending.")]
    public GraphicSlot RunSpoilsBadge = new GraphicSlot();

    [Tooltip("Pending extraction badge background.")]
    public GraphicSlot PendingExtractionBadge = new GraphicSlot();

    public GraphicSlot GetMemberRowSlot(bool selected)
    {
        if (selected && MemberRowSelected != null && MemberRowSelected.HasAssignedGraphic)
        {
            return MemberRowSelected;
        }

        return MemberRow;
    }

    public GraphicSlot GetItemRowSlot(bool selected)
    {
        if (selected && ItemRowSelected != null && ItemRowSelected.HasAssignedGraphic)
        {
            return ItemRowSelected;
        }

        return ItemRow;
    }

    public GraphicSlot GetEquipmentSlot(bool selected)
    {
        return GetEquipmentSlot(selected, true);
    }

    public GraphicSlot GetEquipmentSlot(bool selected, bool hasEquippedItem)
    {
        if (selected && EquipmentSlotSelected != null && EquipmentSlotSelected.HasAssignedGraphic)
        {
            return EquipmentSlotSelected;
        }

        if (hasEquippedItem && EquipmentSlotEquipped != null && EquipmentSlotEquipped.HasAssignedGraphic)
        {
            return EquipmentSlotEquipped;
        }

        if (!hasEquippedItem && EquipmentSlotEmpty != null && EquipmentSlotEmpty.HasAssignedGraphic)
        {
            return EquipmentSlotEmpty;
        }

        return EquipmentSlot;
    }
}
