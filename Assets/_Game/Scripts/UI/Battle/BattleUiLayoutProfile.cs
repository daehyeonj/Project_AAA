using UnityEngine;

[CreateAssetMenu(menuName = "Project AAA/UI/Battle UI Layout Profile", fileName = "BattleUiLayout")]
public sealed class BattleUiLayoutProfile : ScriptableObject
{
    [Header("Frame")]
    public float TopStripHeight = 96f;
    public float PanelPadding = 16f;
    public float CardGap = 14f;

    [Header("Primary Panels")]
    public float CommandPanelWidth = 284f;
    public float CurrentUnitPanelWidth = 520f;
    public float TargetStatusPanelWidth = 404f;
    public float CommandPanelHeight = 382f;
    public float PopoverWidth = 420f;
    public float PopoverHeight = 236f;

    [Header("Controls")]
    public float ButtonHeight = 48f;
    public float TimelineChipWidth = 164f;
    public float TimelineChipHeight = 72f;
    public float HpBarHeight = 18f;

    [Header("Typography")]
    public float TitleFontScale = 1f;
    public float BodyFontScale = 1f;
    public float CaptionFontScale = 1f;
}
