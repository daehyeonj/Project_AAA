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

    [Header("Shared Panels")]
    [Tooltip("Default battle panel/card background.")]
    public GraphicSlot PanelBackground = new GraphicSlot();

    [Tooltip("Optional header or summary card background.")]
    public GraphicSlot PanelHeader = new GraphicSlot();

    [Tooltip("Optional accent strip/line graphic.")]
    public GraphicSlot PanelAccent = new GraphicSlot();

    [Tooltip("Wide battle top-strip background. Leave empty to keep fallback rendering instead of stretching general panel art.")]
    public GraphicSlot TopStripBackground = new GraphicSlot();

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

    [Tooltip("Optional badge background for reward/drop callouts in preview or result popovers.")]
    public GraphicSlot DropPopupBadge = new GraphicSlot();

    [Tooltip("Reserved badge/background slot for burst-window callouts.")]
    public GraphicSlot BurstWindowBadge = new GraphicSlot();

    [Header("Text")]
    [Tooltip("Default title text color when a light popup background is assigned.")]
    public Color PopupTitleTextColor = new Color(0.18f, 0.12f, 0.08f, 1f);

    [Tooltip("Default body text color when a light popup background is assigned.")]
    public Color PopupBodyTextColor = new Color(0.20f, 0.14f, 0.10f, 1f);

    [Tooltip("Default hint or badge text color when a light popup background is assigned.")]
    public Color PopupHintTextColor = new Color(0.24f, 0.17f, 0.11f, 1f);

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

    public GraphicSlot GetTopStripSlot()
    {
        return TopStripBackground != null && TopStripBackground.HasAssignedGraphic
            ? TopStripBackground
            : null;
    }

    public Color GetPopupTitleTextColor(Color fallbackColor)
    {
        return PopupBackground != null && PopupBackground.HasAssignedGraphic
            ? PopupTitleTextColor
            : fallbackColor;
    }

    public Color GetPopupBodyTextColor(Color fallbackColor)
    {
        return PopupBackground != null && PopupBackground.HasAssignedGraphic
            ? PopupBodyTextColor
            : fallbackColor;
    }

    public Color GetPopupHintTextColor(Color fallbackColor)
    {
        return PopupBackground != null && PopupBackground.HasAssignedGraphic
            ? PopupHintTextColor
            : fallbackColor;
    }
}

public enum BattleUiSkinButtonState
{
    Normal,
    Hover,
    Selected,
    Disabled
}
