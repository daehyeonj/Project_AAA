using System;
using UnityEngine;

[CreateAssetMenu(menuName = "Project AAA/UI/Battle UI Skin", fileName = "BattleUiSkin")]
public sealed class BattleUiSkinDefinition : ScriptableObject
{
    [Serializable]
    public sealed class GraphicSlot
    {
        [Tooltip("Optional sprite reference. Preferred when the source texture is imported as Sprite (2D and UI).")]
        public Sprite Sprite;

        [Tooltip("Optional texture reference for packs that are still imported as Default Texture.")]
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

    [Header("Shared Panels")]
    [Tooltip("Default battle panel/card background.")]
    public GraphicSlot PanelBackground = new GraphicSlot();

    [Tooltip("Optional header or summary card background.")]
    public GraphicSlot PanelHeader = new GraphicSlot();

    [Tooltip("Optional accent strip/line graphic.")]
    public GraphicSlot PanelAccent = new GraphicSlot();

    [Header("Command Buttons")]
    [Tooltip("Normal command button background.")]
    public GraphicSlot CommandButtonNormal = new GraphicSlot();

    [Tooltip("Hovered command button background.")]
    public GraphicSlot CommandButtonHover = new GraphicSlot();

    [Tooltip("Selected command button background.")]
    public GraphicSlot CommandButtonSelected = new GraphicSlot();

    [Tooltip("Disabled command button background.")]
    public GraphicSlot CommandButtonDisabled = new GraphicSlot();

    [Header("Battle Cards")]
    [Tooltip("Current Unit panel/card background.")]
    public GraphicSlot CurrentUnitCard = new GraphicSlot();

    [Tooltip("Target Status panel/card background.")]
    public GraphicSlot TargetStatusCard = new GraphicSlot();

    [Header("Timeline")]
    [Tooltip("Default queued timeline chip background.")]
    public GraphicSlot TimelineChip = new GraphicSlot();

    [Tooltip("Current-turn timeline chip background.")]
    public GraphicSlot TimelineChipCurrent = new GraphicSlot();

    [Tooltip("Enemy timeline chip background.")]
    public GraphicSlot TimelineChipEnemy = new GraphicSlot();

    [Header("Meters")]
    [Tooltip("HP bar background.")]
    public GraphicSlot HpBarBackground = new GraphicSlot();

    [Tooltip("HP bar fill overlay.")]
    public GraphicSlot HpBarFill = new GraphicSlot();

    [Header("Overlays")]
    [Tooltip("Battle result popover background.")]
    public GraphicSlot PopupBackground = new GraphicSlot();

    [Tooltip("Reserved for the battle-adjacent inventory overlay if the project later chooses to share the same skin.")]
    public GraphicSlot InventoryOverlayBackground = new GraphicSlot();

    [Tooltip("Reserved for inventory/equipment slot framing if this skin is expanded later.")]
    public GraphicSlot SlotFrame = new GraphicSlot();

    public GraphicSlot GetCommandButtonSlot(BattleUiSkinButtonState state)
    {
        switch (state)
        {
            case BattleUiSkinButtonState.Disabled:
                return CommandButtonDisabled;
            case BattleUiSkinButtonState.Selected:
                return CommandButtonSelected;
            case BattleUiSkinButtonState.Hover:
                return CommandButtonHover;
            default:
                return CommandButtonNormal;
        }
    }

    public GraphicSlot GetTimelineSlot(bool isCurrent, bool isEnemy)
    {
        if (isCurrent && TimelineChipCurrent != null && TimelineChipCurrent.HasAssignedGraphic)
        {
            return TimelineChipCurrent;
        }

        if (isEnemy && TimelineChipEnemy != null && TimelineChipEnemy.HasAssignedGraphic)
        {
            return TimelineChipEnemy;
        }

        return TimelineChip;
    }
}

public enum BattleUiSkinButtonState
{
    Normal,
    Hover,
    Selected,
    Disabled
}
