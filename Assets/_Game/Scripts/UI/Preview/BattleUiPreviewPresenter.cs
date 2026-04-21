using UnityEngine;

public sealed class BattleUiPreviewPresenter
{
    private const float ScreenMargin = 18f;

    private BattleUiSkinDefinition _skin;
    private BattleUiLayoutProfile _layout;
    private BattleUiPreviewData _previewData;
    private GUIStyle _titleStyle;
    private GUIStyle _sectionStyle;
    private GUIStyle _bodyStyle;
    private GUIStyle _captionStyle;
    private GUIStyle _buttonStyle;

    public void Configure(BattleUiSkinDefinition skin, BattleUiLayoutProfile layout, BattleUiPreviewData previewData)
    {
        _skin = skin;
        _layout = layout;
        _previewData = previewData;
    }

    public void Draw()
    {
        EnsureStyles();
        BattleUiPresenterModel model = BattleUiPreviewAdapter.BuildPreviewModel(_previewData);
        BattleUiLayoutProfile layout = _layout != null ? _layout : ScriptableObject.CreateInstance<BattleUiLayoutProfile>();

        Rect screenRect = new Rect(0f, 0f, Screen.width, Screen.height);
        DrawRect(screenRect, new Color(0.08f, 0.09f, 0.12f, 1f));

        Rect topRect = new Rect(ScreenMargin, ScreenMargin, Screen.width - (ScreenMargin * 2f), layout.TopStripHeight);
        DrawTopStrip(topRect, layout, model);

        Rect commandRect = new Rect(ScreenMargin + 8f, topRect.yMax + 22f, layout.CommandPanelWidth, layout.CommandPanelHeight);
        DrawCommandPanel(commandRect, layout, model);

        float lowerY = Screen.height - 190f;
        Rect actorRect = new Rect(ScreenMargin + 8f, lowerY, layout.CurrentUnitPanelWidth, 164f);
        Rect targetRect = new Rect(Screen.width - layout.TargetStatusPanelWidth - ScreenMargin - 8f, lowerY, layout.TargetStatusPanelWidth, 164f);
        DrawCurrentActorPanel(actorRect, layout, model);
        DrawTargetPanel(targetRect, layout, model);

        Rect popoverRect = new Rect(
            (Screen.width * 0.5f) - (layout.PopoverWidth * 0.5f),
            topRect.yMax + 28f,
            layout.PopoverWidth,
            layout.PopoverHeight);
        DrawPopover(popoverRect, model);

        Rect spoilsRect = new Rect(popoverRect.x, popoverRect.yMax + 10f, popoverRect.width, 28f);
        DrawSpoilsLine(spoilsRect, model);
    }

    private void DrawTopStrip(Rect rect, BattleUiLayoutProfile layout, BattleUiPresenterModel model)
    {
        DrawBattleSlot(rect, new Color(0.06f, 0.10f, 0.14f, 0.98f), _skin != null ? _skin.GetTopStripSlot() : null);
        float innerPadding = layout.PanelPadding;
        Rect dungeonRect = new Rect(rect.x + innerPadding, rect.y + innerPadding, 400f, rect.height - (innerPadding * 2f));
        Rect phaseRect = new Rect(dungeonRect.xMax + layout.CardGap, dungeonRect.y, 118f, dungeonRect.height);
        Rect timelineRect = new Rect(phaseRect.xMax + layout.CardGap, dungeonRect.y, rect.xMax - phaseRect.xMax - layout.CardGap - innerPadding, dungeonRect.height);

        DrawBattleSlot(dungeonRect, new Color(0.10f, 0.15f, 0.21f, 0.98f), _skin != null ? _skin.PanelHeader : null);
        GUI.Label(new Rect(dungeonRect.x + 14f, dungeonRect.y + 10f, dungeonRect.width - 28f, 26f), model.DungeonName, _titleStyle);
        GUI.Label(new Rect(dungeonRect.x + 14f, dungeonRect.y + 40f, dungeonRect.width - 28f, 18f), model.RouteLabel, _bodyStyle);
        GUI.Label(new Rect(dungeonRect.x + 14f, dungeonRect.y + 62f, dungeonRect.width - 28f, 18f), model.ChoiceProgressText, _captionStyle);

        DrawBattleSlot(phaseRect, new Color(0.13f, 0.21f, 0.30f, 0.98f), _skin != null ? _skin.PanelHeader : null);
        GUI.Label(new Rect(phaseRect.x + 10f, phaseRect.y + 10f, phaseRect.width - 20f, 22f), "Turn", _sectionStyle);
        GUI.Label(new Rect(phaseRect.x + 10f, phaseRect.y + 34f, phaseRect.width - 20f, 22f), model.PhaseLabel, _bodyStyle);
        GUI.Label(new Rect(phaseRect.x + 10f, phaseRect.y + 56f, phaseRect.width - 20f, 18f), model.TurnHintText, _captionStyle);

        DrawBattleSlot(timelineRect, new Color(0.07f, 0.11f, 0.16f, 0.99f), _skin != null ? _skin.PanelHeader : null);
        BattleUiPresenterModel.TimelineSlot[] slots = model.Timeline ?? System.Array.Empty<BattleUiPresenterModel.TimelineSlot>();
        float chipWidth = layout.TimelineChipWidth;
        float chipGap = 8f;
        float chipX = timelineRect.x + 10f;
        float chipY = timelineRect.y + 8f;
        float chipHeight = layout.TimelineChipHeight;
        for (int i = 0; i < slots.Length; i++)
        {
            Rect chipRect = new Rect(chipX, chipY, chipWidth, chipHeight);
            DrawTimelineChip(chipRect, slots[i]);
            chipX += chipWidth + chipGap;
            if (chipX + chipWidth > timelineRect.xMax - 10f)
            {
                break;
            }
        }
    }

    private void DrawCommandPanel(Rect rect, BattleUiLayoutProfile layout, BattleUiPresenterModel model)
    {
        DrawBattleSlot(rect, new Color(0.08f, 0.11f, 0.16f, 0.96f), _skin != null ? _skin.PanelBackground : null);
        Rect shellRect = Inset(rect, 8f);
        DrawBattleSlot(shellRect, new Color(0.06f, 0.09f, 0.14f, 0.94f), _skin != null ? _skin.PanelHeader : null);
        GUI.Label(new Rect(shellRect.x + 12f, shellRect.y + 10f, shellRect.width - 24f, 22f), "Command Selection", _sectionStyle);
        GUI.Label(new Rect(shellRect.x + 12f, shellRect.y + 34f, shellRect.width - 24f, 18f), model.CurrentActorName, _bodyStyle);
        GUI.Label(new Rect(shellRect.x + 12f, shellRect.y + 54f, shellRect.width - 24f, 18f), model.CommandHintText, _captionStyle);

        string[] actions = model.ActionLabels ?? System.Array.Empty<string>();
        float buttonY = shellRect.y + 78f;
        for (int i = 0; i < actions.Length; i++)
        {
            Rect buttonRect = new Rect(shellRect.x + 12f, buttonY, shellRect.width - 24f, layout.ButtonHeight);
            DrawCommandButton(buttonRect, actions[i], i == model.SelectedActionIndex);
            buttonY += layout.ButtonHeight + 8f;
        }
    }

    private void DrawCurrentActorPanel(Rect rect, BattleUiLayoutProfile layout, BattleUiPresenterModel model)
    {
        DrawBattleSlot(rect, new Color(0.08f, 0.11f, 0.16f, 0.98f), _skin != null ? _skin.CurrentUnitCard : null);
        GUI.Label(new Rect(rect.x + 14f, rect.y + 10f, rect.width - 28f, 22f), "Current Unit", _sectionStyle);
        GUI.Label(new Rect(rect.x + 14f, rect.y + 34f, rect.width - 28f, 22f), model.CurrentActorName, _titleStyle);
        GUI.Label(new Rect(rect.x + 14f, rect.y + 58f, rect.width - 28f, 18f), model.CurrentActorRole + " | Lv " + model.CurrentActorLevel, _bodyStyle);
        GUI.Label(new Rect(rect.x + 14f, rect.y + 78f, rect.width - 28f, 18f), "ATK " + model.Attack + "  DEF " + model.Defense + "  SPD " + model.Speed, _captionStyle);
        GUI.Label(new Rect(rect.x + 14f, rect.y + 98f, rect.width - 28f, 18f), model.GearSummary, _captionStyle);
        Rect hpRect = new Rect(rect.x + 14f, rect.yMax - 34f, rect.width - 28f, layout.HpBarHeight);
        DrawHpBar(hpRect, model.CurrentHp, model.CurrentMaxHp);
        GUI.Label(new Rect(rect.x + 14f, rect.yMax - 58f, rect.width - 28f, 18f), model.ActorFooterText, _captionStyle);
    }

    private void DrawTargetPanel(Rect rect, BattleUiLayoutProfile layout, BattleUiPresenterModel model)
    {
        DrawBattleSlot(rect, new Color(0.10f, 0.12f, 0.17f, 0.98f), _skin != null ? _skin.TargetStatusCard : null);
        GUI.Label(new Rect(rect.x + 14f, rect.y + 10f, rect.width - 28f, 22f), "Target Status", _sectionStyle);
        GUI.Label(new Rect(rect.x + 14f, rect.y + 34f, rect.width - 28f, 22f), model.TargetName, _titleStyle);
        GUI.Label(new Rect(rect.x + 14f, rect.y + 58f, rect.width - 28f, 18f), model.TargetRole, _bodyStyle);
        GUI.Label(new Rect(rect.x + 14f, rect.y + 80f, rect.width - 28f, 18f), model.ExpectedDamageText, _bodyStyle);
        GUI.Label(new Rect(rect.x + 14f, rect.y + 100f, rect.width - 28f, 18f), model.EnemyIntentText, _captionStyle);
        GUI.Label(new Rect(rect.x + 14f, rect.y + 120f, rect.width - 28f, 18f), model.TargetSummaryText, _captionStyle);
        Rect hpRect = new Rect(rect.x + 14f, rect.yMax - 34f, rect.width - 28f, layout.HpBarHeight);
        DrawHpBar(hpRect, model.TargetHp, model.TargetMaxHp);
    }

    private void DrawPopover(Rect rect, BattleUiPresenterModel model)
    {
        DrawBattleSlot(rect, new Color(0.12f, 0.18f, 0.24f, 0.96f), _skin != null ? _skin.PopupBackground : null);
        Color titleColor = _skin != null ? _skin.GetPopupTitleTextColor(_sectionStyle.normal.textColor) : _sectionStyle.normal.textColor;
        Color bodyColor = _skin != null ? _skin.GetPopupBodyTextColor(_bodyStyle.normal.textColor) : _bodyStyle.normal.textColor;
        Color hintColor = _skin != null ? _skin.GetPopupHintTextColor(_captionStyle.normal.textColor) : _captionStyle.normal.textColor;
        DrawLabel(new Rect(rect.x + 16f, rect.y + 12f, rect.width - 32f, 24f), model.ResultPopoverTitle, _sectionStyle, titleColor);
        DrawLabel(new Rect(rect.x + 16f, rect.y + 40f, rect.width - 32f, rect.height - 56f), model.ResultPopoverBody, _bodyStyle, bodyColor);
        if (_skin != null && _skin.DropPopupBadge != null && _skin.DropPopupBadge.HasAssignedGraphic)
        {
            Rect badgeRect = new Rect(rect.x + 16f, rect.yMax - 36f, 108f, 22f);
            if (!BattleUiSkinRenderer.TryDrawGraphic(badgeRect, _skin.DropPopupBadge))
            {
                DrawRect(badgeRect, new Color(0.62f, 0.46f, 0.19f, 0.98f));
            }

            DrawLabel(badgeRect, "Drop Preview", _captionStyle, hintColor);
        }
    }

    private void DrawSpoilsLine(Rect rect, BattleUiPresenterModel model)
    {
        DrawRect(rect, new Color(0.12f, 0.16f, 0.12f, 0.92f));
        GUI.Label(new Rect(rect.x + 10f, rect.y + 4f, rect.width - 20f, rect.height - 8f), model.RunSpoilsLine, _captionStyle);
    }

    private void DrawTimelineChip(Rect rect, BattleUiPresenterModel.TimelineSlot slot)
    {
        BattleUiSkinDefinition.GraphicSlot skinSlot = _skin != null ? _skin.GetTimelineSlot(slot != null && slot.IsCurrent, slot != null && slot.IsEnemy) : null;
        Color fallback = slot != null && slot.IsEnemy
            ? new Color(0.29f, 0.15f, 0.15f, 0.98f)
            : slot != null && slot.IsCurrent
                ? new Color(0.13f, 0.21f, 0.30f, 0.98f)
                : new Color(0.10f, 0.15f, 0.22f, 0.98f);
        DrawBattleSlot(rect, fallback, skinSlot);
        GUI.Label(new Rect(rect.x + 10f, rect.y + 8f, rect.width - 20f, 18f), slot != null ? slot.StatusLabel : "Queued", _captionStyle);
        GUI.Label(new Rect(rect.x + 10f, rect.y + 26f, rect.width - 20f, 20f), slot != null ? slot.Label : "None", _sectionStyle);
        GUI.Label(new Rect(rect.x + 10f, rect.y + 46f, rect.width - 20f, 16f), slot != null ? slot.SecondaryLabel : "None", _captionStyle);
    }

    private void DrawCommandButton(Rect rect, string label, bool selected)
    {
        BattleUiSkinDefinition.GraphicSlot buttonSlot = _skin != null
            ? _skin.GetCommandButtonSlot(selected ? BattleUiSkinButtonState.Selected : BattleUiSkinButtonState.Normal)
            : null;
        DrawBattleSlot(rect, selected ? new Color(0.24f, 0.40f, 0.58f, 0.98f) : new Color(0.11f, 0.15f, 0.22f, 0.98f), buttonSlot);
        GUI.Label(rect, label, _buttonStyle);
    }

    private void DrawHpBar(Rect rect, int current, int max)
    {
        float ratio = max > 0 ? Mathf.Clamp01((float)current / max) : 0f;
        if (_skin == null || !BattleUiSkinRenderer.TryDrawGraphic(rect, _skin.HpBarBackground))
        {
            DrawRect(rect, new Color(0.06f, 0.08f, 0.12f, 1f));
        }

        Rect fillRect = new Rect(rect.x + 2f, rect.y + 2f, (rect.width - 4f) * ratio, rect.height - 4f);
        if (_skin == null || !BattleUiSkinRenderer.TryDrawGraphic(fillRect, _skin.HpBarFill))
        {
            DrawRect(fillRect, new Color(0.30f, 0.74f, 0.42f, 1f));
        }

        GUI.Label(rect, "HP " + current + " / " + max, _captionStyle);
    }

    private void DrawBattleSlot(Rect rect, Color fallbackColor, BattleUiSkinDefinition.GraphicSlot slot)
    {
        if (_skin == null || slot == null || !BattleUiSkinRenderer.TryDrawGraphic(rect, slot))
        {
            DrawRect(rect, fallbackColor);
        }
    }

    private static void DrawLabel(Rect rect, string text, GUIStyle style, Color color)
    {
        Color previous = GUI.contentColor;
        GUI.contentColor = color;
        GUI.Label(rect, text, style);
        GUI.contentColor = previous;
    }

    private static Rect Inset(Rect rect, float padding)
    {
        return new Rect(rect.x + padding, rect.y + padding, rect.width - (padding * 2f), rect.height - (padding * 2f));
    }

    private static void DrawRect(Rect rect, Color color)
    {
        Color previous = GUI.color;
        GUI.color = color;
        GUI.DrawTexture(rect, Texture2D.whiteTexture);
        GUI.color = previous;
    }

    private void EnsureStyles()
    {
        if (_titleStyle != null)
        {
            return;
        }

        _titleStyle = CreateStyle(20, FontStyle.Bold, TextAnchor.UpperLeft, false);
        _sectionStyle = CreateStyle(14, FontStyle.Bold, TextAnchor.UpperLeft, false);
        _bodyStyle = CreateStyle(12, FontStyle.Normal, TextAnchor.UpperLeft, true);
        _captionStyle = CreateStyle(11, FontStyle.Normal, TextAnchor.UpperLeft, true);
        _buttonStyle = CreateStyle(13, FontStyle.Bold, TextAnchor.MiddleCenter, true);
    }

    private static GUIStyle CreateStyle(int fontSize, FontStyle fontStyle, TextAnchor alignment, bool wordWrap)
    {
        return new GUIStyle(GUI.skin.label)
        {
            fontSize = fontSize,
            fontStyle = fontStyle,
            alignment = alignment,
            wordWrap = wordWrap,
            normal = { textColor = Color.white }
        };
    }
}
