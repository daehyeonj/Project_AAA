using System.Collections.Generic;
using UnityEngine;

[DisallowMultipleComponent]
public sealed class PrototypeDebugHUD : MonoBehaviour
{
    private const float Margin = 16f;
    private const float PanelWidthMin = 320f;
    private const float PanelWidthMax = 420f;
    private const float PanelPadding = 12f;
    private const float HeaderHeight = 28f;
    private const float FilterHeight = 34f;
    private const float PanelGap = 10f;
    private const float ChipGap = 6f;
    private const float ChipHeight = 24f;
    private const float LanguageButtonWidth = 44f;
    private const float SearchHeight = 24f;
    private const float SearchLabelWidth = 48f;
    private const float SearchClearButtonWidth = 52f;
    private const float DungeonBattleBarMinHeight = 300f;
    private const float DungeonBattleBarMaxHeight = 360f;
    private const float OverlaySectionGap = 10f;
    private const float SectionInnerPadding = 10f;
    private const float ActionButtonHeight = 36f;
    private const float ActionButtonGap = 8f;
    private const string SearchFieldControlName = "PrototypeDebugHUD.SearchField";

    private enum HudFilter
    {
        All,
        Simulation,
        Economy,
        Selected,
        Logs
    }

    private sealed class HudPanel
    {
        public string Key { get; private set; }
        public string Title { get; private set; }
        public string Body { get; private set; }
        public HudFilter Group { get; private set; }

        public HudPanel(string key, string title, string body, HudFilter group)
        {
            Key = string.IsNullOrEmpty(key) ? "panel" : key;
            Title = string.IsNullOrEmpty(title) ? "Panel" : title;
            Body = string.IsNullOrEmpty(body) ? "None" : body;
            Group = group;
        }
    }

    private BootEntry _bootEntry;
    private GUIStyle _bodyStyle;
    private GUIStyle _panelStyle;
    private GUIStyle _titleStyle;
    private GUIStyle _chipStyle;
    private GUIStyle _chipActiveStyle;
    private GUIStyle _foldoutStyle;
    private GUIStyle _searchFieldStyle;
    private GUIStyle _actionButtonStyle;
    private Vector2 _scrollPosition;
    private HudFilter _activeFilter = HudFilter.All;
    private string _searchText = string.Empty;
    private Dictionary<string, bool> _expandedByKey;
    private bool _isSearchFieldFocused;

    public bool IsSearchFieldFocused => _isSearchFieldFocused;

    private void Awake()
    {
        CacheBootEntry();
        EnsureExpandedState();
    }

    private void OnGUI()
    {
        CacheBootEntry();
        if (_bootEntry == null)
        {
            _isSearchFieldFocused = false;
            return;
        }

        EnsureExpandedState();
        EnsureStyles();

        List<HudPanel> panels = BuildPanels();
        bool dungeonHudMode = _bootEntry.IsDungeonRunHudMode;
        bool overlayMode = _bootEntry.IsDungeonBattleViewActive || _bootEntry.IsDungeonRouteChoiceVisible || _bootEntry.IsDungeonEventChoiceVisible || _bootEntry.IsDungeonPreEliteChoiceVisible || _bootEntry.IsDungeonResultPanelVisible;

        float panelWidth = overlayMode
            ? Mathf.Clamp(Screen.width * 0.18f, 276f, 308f)
            : Mathf.Clamp(Screen.width * 0.34f, PanelWidthMin, PanelWidthMax);
        panelWidth = Mathf.Min(panelWidth, Screen.width - (Margin * 2f));

        float panelHeight = overlayMode
            ? Mathf.Clamp(Screen.height * 0.18f, 148f, 188f)
            : dungeonHudMode
                ? Mathf.Clamp(Screen.height * 0.34f, 248f, 380f)
                : Mathf.Max(220f, Screen.height - (Margin * 2f));

        Rect dockRect = new Rect(Screen.width - panelWidth - Margin, Margin, panelWidth, panelHeight);
        DrawDockPanel(dockRect, panels);

        if (overlayMode)
        {
            DrawDungeonBattleBottomBar(dockRect);
        }
    }

    private void DrawDockPanel(Rect rect, List<HudPanel> panels)
    {
        Color previousColor = GUI.color;
        GUI.color = new Color(0.05f, 0.08f, 0.06f, 0.88f);
        GUI.Box(rect, GUIContent.none, _panelStyle);

        Rect titleRect = new Rect(rect.x, rect.y, rect.width, HeaderHeight);
        GUI.color = new Color(0.15f, 0.28f, 0.2f, 0.96f);
        GUI.DrawTexture(titleRect, Texture2D.whiteTexture);
        GUI.color = Color.white;
        DrawTitleBar(titleRect);

        Rect filterRect = new Rect(rect.x + PanelPadding, rect.y + HeaderHeight + 6f, rect.width - (PanelPadding * 2f), FilterHeight);
        DrawFilterChips(filterRect);

        Rect searchRect = new Rect(rect.x + PanelPadding, filterRect.yMax + 4f, rect.width - (PanelPadding * 2f), SearchHeight);
        DrawSearchBar(searchRect);

        float bodyTop = searchRect.yMax + 6f;
        Rect scrollRect = new Rect(rect.x + 6f, bodyTop, rect.width - 12f, rect.height - (bodyTop - rect.y) - 6f);
        float contentWidth = scrollRect.width - 18f;
        List<HudPanel> visiblePanels = FilterPanels(panels);
        float contentHeight = CalculateContentHeight(visiblePanels, contentWidth);

        _scrollPosition = GUI.BeginScrollView(scrollRect, _scrollPosition, new Rect(0f, 0f, contentWidth, contentHeight));
        DrawPanels(new Rect(0f, 0f, contentWidth, contentHeight), visiblePanels);
        GUI.EndScrollView();

        GUI.color = previousColor;
    }
    private void DrawDungeonBattleBottomBar(Rect dockRect)
    {
        float barHeight = Mathf.Clamp(Screen.height * 0.36f, DungeonBattleBarMinHeight, DungeonBattleBarMaxHeight);
        float availableWidth = Screen.width - (Margin * 3f) - dockRect.width;
        if (availableWidth < 600f)
        {
            availableWidth = Screen.width - (Margin * 2f);
        }

        Rect rect = new Rect(Margin, Screen.height - barHeight - Margin, availableWidth, barHeight);
        Color previousColor = GUI.color;
        GUI.color = new Color(0.04f, 0.07f, 0.10f, 0.96f);
        GUI.Box(rect, GUIContent.none, _panelStyle);

        Rect titleRect = new Rect(rect.x, rect.y, rect.width, HeaderHeight);
        GUI.color = new Color(0.12f, 0.22f, 0.30f, 0.98f);
        GUI.DrawTexture(titleRect, Texture2D.whiteTexture);
        GUI.color = Color.white;

        string overlayTitle = _bootEntry.IsDungeonResultPanelVisible
            ? T("PanelDungeonResult")
            : _bootEntry.IsDungeonRouteChoiceVisible
                ? T("PanelDungeonRouteChoice")
                : _bootEntry.IsDungeonEventChoiceVisible
                    ? V(_bootEntry.EventTitleLabel)
                    : _bootEntry.IsDungeonPreEliteChoiceVisible
                        ? T("PanelDungeonPreElite")
                        : T("PanelDungeonBattle");
        GUI.Label(new Rect(titleRect.x + PanelPadding, titleRect.y + 4f, titleRect.width - (PanelPadding * 2f), titleRect.height), overlayTitle, _titleStyle);

        Rect contentRect = new Rect(rect.x + PanelPadding, rect.y + HeaderHeight + 8f, rect.width - (PanelPadding * 2f), rect.height - HeaderHeight - 16f);
        if (_bootEntry.IsDungeonRouteChoiceVisible)
        {
            float leftWidth = (contentRect.width - (OverlaySectionGap * 2f)) * 0.30f;
            float centerWidth = (contentRect.width - (OverlaySectionGap * 2f)) * 0.26f;
            float rightWidth = contentRect.width - leftWidth - centerWidth - (OverlaySectionGap * 2f);
            Rect leftRect = new Rect(contentRect.x, contentRect.y, leftWidth, contentRect.height);
            Rect centerRect = new Rect(leftRect.xMax + OverlaySectionGap, contentRect.y, centerWidth, contentRect.height);
            Rect rightRect = new Rect(centerRect.xMax + OverlaySectionGap, contentRect.y, rightWidth, contentRect.height);

            DrawOverlaySection(leftRect, BuildPanelBody(
                Line("RunState", V(_bootEntry.DungeonRunStateLabel)),
                Line("SelectedCity", V(_bootEntry.CurrentCityRunLabel)),
                Line("CurrentDungeon", V(_bootEntry.CurrentDungeonRunLabel)),
                Line("DungeonDanger", V(_bootEntry.CurrentDungeonDangerLabel)),
                Line("CityManaShardStock", V(_bootEntry.CurrentCityManaShardStockRunLabel)),
                Line("NeedPressure", V(_bootEntry.CurrentNeedPressureRunLabel)),
                Line("DispatchReadiness", V(_bootEntry.CurrentDispatchReadinessRunLabel)),
                Line("RecoveryProgress", V(_bootEntry.CurrentDispatchRecoveryProgressRunLabel)),
                Line("ConsecutiveDispatches", V(_bootEntry.CurrentDispatchConsecutiveRunLabel)),
                Line("DispatchPolicy", V(_bootEntry.CurrentDispatchPolicyRunLabel)),
                Line("RecommendedRoute", V(_bootEntry.RecommendedRouteRunLabel)),
                Line("RecommendationReason", V(_bootEntry.RecommendationReasonRunLabel)),
                Line("RecoveryAdvice", V(_bootEntry.RecoveryAdviceRunLabel)),
                Line("ExpectedNeedImpact", V(_bootEntry.ExpectedNeedImpactRunLabel)),
                Line("ActiveParty", V(_bootEntry.ActiveDungeonPartyLabel)),
                Line("DungeonRouteControls", _bootEntry.DungeonRunRouteControlsLabel),
                V(_bootEntry.RouteChoiceDescriptionLabel),
                V(_bootEntry.RouteChoicePromptLabel)), new Color(0.08f, 0.12f, 0.16f, 0.94f));
            DrawRouteChoiceSection(centerRect);
            DrawOverlaySection(rightRect, BuildPanelBody(
                Line("RoutePreview1", V(_bootEntry.SelectedRoutePreview1Label)),
                Line("RoutePreview2", V(_bootEntry.SelectedRoutePreview2Label)),
                Line("RewardPreview", V(_bootEntry.SelectedRewardPreviewLabel)),
                Line("EventPreview", V(_bootEntry.SelectedEventPreviewLabel))), new Color(0.09f, 0.11f, 0.14f, 0.94f));
        }
        else if (_bootEntry.IsDungeonEventChoiceVisible)
        {
            float leftWidth = (contentRect.width - (OverlaySectionGap * 2f)) * 0.34f;
            float centerWidth = (contentRect.width - (OverlaySectionGap * 2f)) * 0.32f;
            float rightWidth = contentRect.width - leftWidth - centerWidth - (OverlaySectionGap * 2f);
            Rect leftRect = new Rect(contentRect.x, contentRect.y, leftWidth, contentRect.height);
            Rect centerRect = new Rect(leftRect.xMax + OverlaySectionGap, contentRect.y, centerWidth, contentRect.height);
            Rect rightRect = new Rect(centerRect.xMax + OverlaySectionGap, contentRect.y, rightWidth, contentRect.height);

            DrawOverlaySection(leftRect, BuildPanelBody(
                V(_bootEntry.EventTitleLabel),
                V(_bootEntry.EventDescriptionLabel),
                V(_bootEntry.EventPromptLabel)), new Color(0.08f, 0.12f, 0.16f, 0.94f));
            DrawEventChoiceSection(centerRect);
            DrawOverlaySection(rightRect, BuildPanelBody(
                Line("RunState", V(_bootEntry.DungeonRunStateLabel)),
                Line("CurrentDungeon", V(_bootEntry.CurrentDungeonRunLabel)),
                Line("CurrentRoom", V(_bootEntry.CurrentRoomLabel)),
                Line("CurrentRoomType", V(_bootEntry.CurrentRoomTypeLabel)),
                Line("RoomProgress", V(_bootEntry.RoomProgressLabel)),
                Line("TotalPartyHp", V(_bootEntry.TotalPartyHpLabel)),
                Line("PartyCondition", V(_bootEntry.PartyConditionLabel)),
                Line("EventStatus", V(_bootEntry.EventStatusLabel)),
                Line("EventChoice", V(_bootEntry.EventChoiceLabel)),
                Line("PreEliteChoice", V(_bootEntry.PreEliteChoiceRunLabel)),
                Line("LootBreakdown", V(_bootEntry.LootBreakdownLabel)),
                Line("LastBattleLog1", V(_bootEntry.LastBattleLog1Label)),
                Line("LastBattleLog2", V(_bootEntry.LastBattleLog2Label)),
                Line("LastBattleLog3", V(_bootEntry.LastBattleLog3Label))), new Color(0.09f, 0.11f, 0.14f, 0.94f));
        }
        else if (_bootEntry.IsDungeonPreEliteChoiceVisible)
        {
            float leftWidth = (contentRect.width - (OverlaySectionGap * 2f)) * 0.34f;
            float centerWidth = (contentRect.width - (OverlaySectionGap * 2f)) * 0.30f;
            float rightWidth = contentRect.width - leftWidth - centerWidth - (OverlaySectionGap * 2f);
            Rect leftRect = new Rect(contentRect.x, contentRect.y, leftWidth, contentRect.height);
            Rect centerRect = new Rect(leftRect.xMax + OverlaySectionGap, contentRect.y, centerWidth, contentRect.height);
            Rect rightRect = new Rect(centerRect.xMax + OverlaySectionGap, contentRect.y, rightWidth, contentRect.height);

            DrawOverlaySection(leftRect, BuildPanelBody(
                V(_bootEntry.PreEliteTitleLabel),
                V(_bootEntry.PreEliteDescriptionLabel),
                V(_bootEntry.PreElitePromptLabel)), new Color(0.11f, 0.10f, 0.17f, 0.94f));
            DrawPreEliteChoiceSection(centerRect);
            DrawOverlaySection(rightRect, BuildPanelBody(
                Line("CurrentDungeon", V(_bootEntry.CurrentDungeonRunLabel)),
                Line("CurrentRoute", V(_bootEntry.CurrentRouteLabel)),
                Line("CurrentRoom", V(_bootEntry.CurrentRoomLabel)),
                Line("RoomProgress", V(_bootEntry.RoomProgressLabel)),
                Line("TotalPartyHp", V(_bootEntry.TotalPartyHpLabel)),
                Line("PartyCondition", V(_bootEntry.PartyConditionLabel)),
                Line("EliteEncounter", V(_bootEntry.EliteEncounterNameLabel)),
                Line("PreEliteChoice", V(_bootEntry.PreEliteChoiceRunLabel)),
                Line("LastBattleLog1", V(_bootEntry.LastBattleLog1Label)),
                Line("LastBattleLog2", V(_bootEntry.LastBattleLog2Label)),
                Line("LastBattleLog3", V(_bootEntry.LastBattleLog3Label))), new Color(0.10f, 0.10f, 0.15f, 0.94f));
        }
        else if (_bootEntry.IsDungeonResultPanelVisible)
        {
            float leftWidth = (contentRect.width - OverlaySectionGap) * 0.58f;
            Rect leftRect = new Rect(contentRect.x, contentRect.y, leftWidth, contentRect.height);
            Rect rightRect = new Rect(leftRect.xMax + OverlaySectionGap, contentRect.y, contentRect.width - leftWidth - OverlaySectionGap, contentRect.height);

            DrawOverlaySection(leftRect, BuildPanelBody(
                Line("Result", V(_bootEntry.ResultPanelStateLabel)),
                Line("CityDispatchedFrom", V(_bootEntry.ResultPanelCityDispatchedFromLabel)),
                Line("DungeonChosen", V(_bootEntry.ResultPanelDungeonChosenLabel)),
                Line("DungeonDanger", V(_bootEntry.ResultPanelDungeonDangerLabel)),
                Line("RouteChosen", V(_bootEntry.ResultPanelRouteChosenLabel)),
                Line("RecommendedRoute", V(_bootEntry.ResultPanelRecommendedRouteLabel)),
                Line("FollowedRecommendation", V(_bootEntry.ResultPanelFollowedRecommendationLabel)),
                Line("ManaShardsReturned", V(_bootEntry.ResultPanelManaShardsReturnedLabel)),
                Line("StockBefore", V(_bootEntry.ResultPanelStockBeforeLabel)),
                Line("StockAfter", V(_bootEntry.ResultPanelStockAfterLabel)),
                Line("StockDelta", V(_bootEntry.ResultPanelStockDeltaLabel)),
                Line("NeedPressureBefore", V(_bootEntry.ResultPanelNeedPressureBeforeLabel)),
                Line("NeedPressureAfter", V(_bootEntry.ResultPanelNeedPressureAfterLabel)),
                Line("DispatchReadinessBefore", V(_bootEntry.ResultPanelDispatchReadinessBeforeLabel)),
                Line("DispatchReadinessAfter", V(_bootEntry.ResultPanelDispatchReadinessAfterLabel)),
                Line("RecoveryEta", V(_bootEntry.ResultPanelRecoveryEtaLabel)),
                Line("RouteRisk", V(_bootEntry.ResultPanelRouteRiskLabel)),
                Line("TurnsTaken", V(_bootEntry.ResultPanelTurnsTakenLabel)),
                Line("LootGained", V(_bootEntry.ResultPanelLootGainedLabel)),
                Line("BattleLoot", V(_bootEntry.ResultPanelBattleLootLabel)),
                Line("ChestLoot", V(_bootEntry.ResultPanelChestLootLabel)),
                Line("EventLoot", V(_bootEntry.ResultPanelEventLootLabel)),
                Line("EventChoice", V(_bootEntry.ResultPanelEventChoiceLabel)),
                Line("PreEliteChoice", V(_bootEntry.ResultPanelPreEliteChoiceLabel)),
                Line("PreEliteHealAmount", V(_bootEntry.ResultPanelPreEliteHealAmountLabel)),
                Line("EliteBonusRewardEarned", V(_bootEntry.ResultPanelEliteBonusRewardEarnedLabel)),
                Line("EliteBonusRewardAmount", V(_bootEntry.ResultPanelEliteBonusRewardAmountLabel)),
                Line("EliteDefeated", V(_bootEntry.ResultPanelEliteDefeatedLabel)),
                Line("EliteName", V(_bootEntry.ResultPanelEliteNameLabel)),
                Line("EliteRewardIdentity", V(_bootEntry.ResultPanelEliteRewardIdentityLabel)),
                Line("EliteRewardAmount", V(_bootEntry.ResultPanelEliteRewardAmountLabel)),
                Line("RoomPathSummary", V(_bootEntry.ResultPanelRoomPathSummaryLabel)),
                Line("PartyHpSummary", V(_bootEntry.ResultPanelPartyHpSummaryLabel)),
                Line("PartyConditionAtEnd", V(_bootEntry.ResultPanelPartyConditionLabel)),
                Line("SurvivingMembers", V(_bootEntry.ResultPanelSurvivingMembersLabel)),
                Line("ClearedEncounters", V(_bootEntry.ResultPanelClearedEncountersLabel)),
                Line("OpenedChests", V(_bootEntry.ResultPanelOpenedChestsLabel)),
                Line("ReturnToWorldPrompt", V(_bootEntry.ResultPanelReturnPromptLabel))), new Color(0.08f, 0.12f, 0.16f, 0.94f));
            DrawOverlaySection(rightRect, BuildPanelBody(
                Line("CurrentDungeon", V(_bootEntry.CurrentDungeonRunLabel)),
                Line("CurrentRoute", V(_bootEntry.CurrentRouteLabel)),
                Line("RoomPathSummary", V(_bootEntry.ResultPanelRoomPathSummaryLabel)),
                Line("PartyHpSummary", V(_bootEntry.ResultPanelPartyHpSummaryLabel)),
                Line("PartyConditionAtEnd", V(_bootEntry.ResultPanelPartyConditionLabel)),
                Line("ActiveParty", V(_bootEntry.ActiveDungeonPartyLabel)),
                Line("LootBreakdown", V(_bootEntry.LootBreakdownLabel)),
                Line("LastBattleLog1", V(_bootEntry.LastBattleLog1Label)),
                Line("LastBattleLog2", V(_bootEntry.LastBattleLog2Label)),
                Line("LastBattleLog3", V(_bootEntry.LastBattleLog3Label))), new Color(0.09f, 0.11f, 0.14f, 0.94f));
        }
        else
        {
            float leftWidth = (contentRect.width - (OverlaySectionGap * 2f)) * 0.36f;
            float centerWidth = (contentRect.width - (OverlaySectionGap * 2f)) * 0.29f;
            float rightWidth = contentRect.width - leftWidth - centerWidth - (OverlaySectionGap * 2f);
            Rect leftRect = new Rect(contentRect.x, contentRect.y, leftWidth, contentRect.height);
            Rect centerRect = new Rect(leftRect.xMax + OverlaySectionGap, contentRect.y, centerWidth, contentRect.height);
            Rect rightRect = new Rect(centerRect.xMax + OverlaySectionGap, contentRect.y, rightWidth, contentRect.height);

            DrawBattleActionSection(leftRect);
            DrawOverlaySection(centerRect, BuildPanelBody(
                Line("ActiveParty", V(_bootEntry.ActiveDungeonPartyLabel)),
                Line("CarriedLoot", V(_bootEntry.CarriedLootRunLabel)),
                Line("DungeonDanger", V(_bootEntry.CurrentDungeonDangerLabel)),
                Line("CurrentRoute", V(_bootEntry.CurrentRouteLabel)),
                Line("RouteRisk", V(_bootEntry.RouteRiskLabel)),
                Line("CurrentRoomType", V(_bootEntry.CurrentRoomTypeLabel)),
                Line("RoomProgress", V(_bootEntry.RoomProgressLabel)),
                Line("NextMajorGoal", V(_bootEntry.NextMajorGoalLabel)),
                Line("TotalPartyHp", V(_bootEntry.TotalPartyHpLabel)),
                Line("PartyCondition", V(_bootEntry.PartyConditionLabel)),
                Line("SustainPressure", V(_bootEntry.SustainPressureLabel)),
                Line("LootBreakdown", V(_bootEntry.LootBreakdownLabel)),
                Line("RunTurnCount", _bootEntry.DungeonRunTurnCount),
                Line("EncounterProgress", V(_bootEntry.EncounterProgressLabel)),
                Line("EliteStatus", V(_bootEntry.EliteStatusLabel)),
                Line("EliteRewardStatus", V(_bootEntry.EliteRewardStatusLabel)),
                Line("EventStatus", V(_bootEntry.EventStatusLabel)),
                Line("EventChoice", V(_bootEntry.EventChoiceLabel)),
                Line("PreEliteChoice", V(_bootEntry.PreEliteChoiceRunLabel)),
                Line("Party1Hp", V(_bootEntry.Party1HpLabel)),
                Line("Party2Hp", V(_bootEntry.Party2HpLabel)),
                Line("Party3Hp", V(_bootEntry.Party3HpLabel)),
                Line("Party4Hp", V(_bootEntry.Party4HpLabel))), new Color(0.09f, 0.13f, 0.17f, 0.94f));
            DrawOverlaySection(rightRect, BuildPanelBody(
                "Battle View: " + V(_bootEntry.BattleViewStateLabel),
                "Encounter: " + V(_bootEntry.EncounterNameLabel),
                Line("EncounterRoomType", V(_bootEntry.EncounterRoomTypeLabel)),
                Line("EliteEncounter", V(_bootEntry.EliteEncounterNameLabel)),
                Line("EliteType", V(_bootEntry.EliteTypeLabel)),
                Line("EliteHp", V(_bootEntry.EliteHpLabel)),
                Line("EliteRewardHint", V(_bootEntry.EliteRewardHintLabel)),
                Line("EliteDefeated", V(_bootEntry.EliteDefeatedLabel)),
                "Phase: " + V(_bootEntry.BattlePhaseLabel),
                Line("EliteIntent", V(_bootEntry.EnemyIntentLabel)),
                Line("Monster1", V(_bootEntry.BattleMonster1Label)),
                Line("Monster2", V(_bootEntry.BattleMonster2Label)),
                Line("LastBattleLog1", V(_bootEntry.LastBattleLog1Label)),
                Line("LastBattleLog2", V(_bootEntry.LastBattleLog2Label)),
                Line("LastBattleLog3", V(_bootEntry.LastBattleLog3Label))), new Color(0.09f, 0.11f, 0.14f, 0.94f));
        }

        GUI.color = previousColor;
    }
    private void DrawBattleActionSection(Rect rect)
    {
        DrawOverlaySectionBackground(rect, new Color(0.08f, 0.12f, 0.16f, 0.94f));

        Rect innerRect = new Rect(rect.x + SectionInnerPadding, rect.y + SectionInnerPadding, rect.width - (SectionInnerPadding * 2f), rect.height - (SectionInnerPadding * 2f));
        float buttonAreaHeight = (ActionButtonHeight * 3f) + (ActionButtonGap * 2f);
        float summaryHeight = Mathf.Max(110f, innerRect.height - buttonAreaHeight - 12f);
        Rect summaryRect = new Rect(innerRect.x, innerRect.y, innerRect.width, summaryHeight);
        Rect buttonsRect = new Rect(innerRect.x, summaryRect.yMax + 12f, innerRect.width, buttonAreaHeight);

        List<string> summaryLines = new List<string>();
        summaryLines.Add(Line("RunState", V(_bootEntry.DungeonRunStateLabel)));
        summaryLines.Add(Line("CurrentDungeon", V(_bootEntry.CurrentDungeonRunLabel)));
        summaryLines.Add(Line("DungeonDanger", V(_bootEntry.CurrentDungeonDangerLabel)));
        summaryLines.Add(Line("CurrentRoom", V(_bootEntry.CurrentRoomLabel)));
        summaryLines.Add(Line("CurrentRoomType", V(_bootEntry.CurrentRoomTypeLabel)));
        summaryLines.Add(Line("RoomProgress", V(_bootEntry.RoomProgressLabel)));
        summaryLines.Add(Line("NextMajorGoal", V(_bootEntry.NextMajorGoalLabel)));
        summaryLines.Add(Line("TotalPartyHp", V(_bootEntry.TotalPartyHpLabel)));
        summaryLines.Add(Line("PartyCondition", V(_bootEntry.PartyConditionLabel)));
        summaryLines.Add(Line("SustainPressure", V(_bootEntry.SustainPressureLabel)));
        summaryLines.Add(Line("CurrentActor", V(_bootEntry.CurrentBattleActorLabel)));
        summaryLines.Add("Phase: " + V(_bootEntry.BattlePhaseLabel));
        summaryLines.Add("Prompt: " + V(_bootEntry.CurrentSelectionPromptLabel));
        AddOptionalSummaryLine(summaryLines, "Intent", V(_bootEntry.EnemyIntentLabel));
        AddOptionalSummaryLine(summaryLines, "EliteRewardHint", V(_bootEntry.EliteRewardHintLabel));
        AddOptionalSummaryLine(summaryLines, "Feedback", V(_bootEntry.BattleFeedbackLabel));
        AddOptionalSummaryLine(summaryLines, "Cancel", V(_bootEntry.BattleCancelHintLabel));
        GUI.Label(summaryRect, string.Join("\n", summaryLines.ToArray()), _bodyStyle);

        Event current = Event.current;
        Vector2 mousePosition = current != null ? current.mousePosition : Vector2.zero;
        string hoveredActionKey = string.Empty;

        DrawBattleActionButton(new Rect(buttonsRect.x, buttonsRect.y, buttonsRect.width, ActionButtonHeight), "attack", "[1] Attack", mousePosition, ref hoveredActionKey);
        DrawBattleActionButton(new Rect(buttonsRect.x, buttonsRect.y + ActionButtonHeight + ActionButtonGap, buttonsRect.width, ActionButtonHeight), "skill", "[2] Skill", mousePosition, ref hoveredActionKey);
        DrawBattleActionButton(new Rect(buttonsRect.x, buttonsRect.y + ((ActionButtonHeight + ActionButtonGap) * 2f), buttonsRect.width, ActionButtonHeight), "retreat", "[3] Retreat", mousePosition, ref hoveredActionKey);

        if (string.IsNullOrEmpty(hoveredActionKey))
        {
            _bootEntry.SetBattleActionHover(string.Empty);
        }
    }

    private void DrawBattleActionButton(Rect rect, string actionKey, string label, Vector2 mousePosition, ref string hoveredActionKey)
    {
        bool available = _bootEntry.IsBattleActionAvailable(actionKey);
        bool selected = _bootEntry.IsBattleActionSelected(actionKey);
        bool hovered = available && rect.Contains(mousePosition);
        if (hovered)
        {
            hoveredActionKey = actionKey;
            _bootEntry.SetBattleActionHover(actionKey);
        }

        if (DrawInteractiveButton(rect, label, available, hovered, selected))
        {
            _bootEntry.TryTriggerBattleAction(actionKey);
        }
    }

    private void DrawRouteChoiceSection(Rect rect)
    {
        DrawOverlaySectionBackground(rect, new Color(0.08f, 0.12f, 0.16f, 0.94f));

        Rect innerRect = new Rect(rect.x + SectionInnerPadding, rect.y + SectionInnerPadding, rect.width - (SectionInnerPadding * 2f), rect.height - (SectionInnerPadding * 2f));
        Rect promptRect = new Rect(innerRect.x, innerRect.y, innerRect.width, 72f);
        Rect optionARect = new Rect(innerRect.x, promptRect.yMax + 12f, innerRect.width, 52f);
        Rect optionBRect = new Rect(innerRect.x, optionARect.yMax + 12f, innerRect.width, 52f);
        float actionRowY = optionBRect.yMax + 14f;
        float policyWidth = Mathf.Max(96f, innerRect.width * 0.42f);
        policyWidth = Mathf.Min(policyWidth, innerRect.width - 112f);
        Rect policyRect = new Rect(innerRect.x, actionRowY, policyWidth, 44f);
        Rect confirmRect = new Rect(policyRect.xMax + ActionButtonGap, actionRowY, innerRect.width - policyWidth - ActionButtonGap, 44f);

        GUI.Label(promptRect, BuildPanelBody(
            V(_bootEntry.RouteChoiceTitleLabel),
            V(_bootEntry.RouteChoicePromptLabel)), _bodyStyle);

        Event current = Event.current;
        Vector2 mousePosition = current != null ? current.mousePosition : Vector2.zero;
        string hoveredChoiceKey = string.Empty;

        DrawRouteChoiceButton(optionARect, "safe", "[1] " + V(_bootEntry.RouteOption1Label), mousePosition, ref hoveredChoiceKey);
        DrawRouteChoiceButton(optionBRect, "risky", "[2] " + V(_bootEntry.RouteOption2Label), mousePosition, ref hoveredChoiceKey);

        if (string.IsNullOrEmpty(hoveredChoiceKey))
        {
            _bootEntry.SetRouteChoiceHover(string.Empty);
        }

        bool policyHovered = policyRect.Contains(mousePosition);
        string policyLabel = "[Q] " + T("CyclePolicy") + " " + V(_bootEntry.CurrentDispatchPolicyRunLabel);
        if (DrawInteractiveButton(policyRect, policyLabel, true, policyHovered, false))
        {
            _bootEntry.TryCycleCurrentDispatchPolicy();
        }

        bool canConfirm = _bootEntry.CanConfirmRouteChoice();
        bool confirmHovered = canConfirm && confirmRect.Contains(mousePosition);
        if (DrawInteractiveButton(confirmRect, "[Enter] Dispatch", canConfirm, confirmHovered, false))
        {
            _bootEntry.TryConfirmRouteChoice();
        }
    }

    private void DrawRouteChoiceButton(Rect rect, string optionKey, string label, Vector2 mousePosition, ref string hoveredChoiceKey)
    {
        bool available = _bootEntry.IsRouteChoiceAvailable(optionKey);
        bool selected = _bootEntry.IsRouteChoiceSelected(optionKey);
        bool hovered = available && rect.Contains(mousePosition);
        if (hovered)
        {
            hoveredChoiceKey = optionKey;
            _bootEntry.SetRouteChoiceHover(optionKey);
        }

        if (DrawInteractiveButton(rect, label, available, hovered, selected))
        {
            _bootEntry.TryTriggerRouteChoice(optionKey);
        }
    }

    private void DrawEventChoiceSection(Rect rect)
    {
        DrawOverlaySectionBackground(rect, new Color(0.08f, 0.12f, 0.16f, 0.94f));

        Rect innerRect = new Rect(rect.x + SectionInnerPadding, rect.y + SectionInnerPadding, rect.width - (SectionInnerPadding * 2f), rect.height - (SectionInnerPadding * 2f));
        Rect promptRect = new Rect(innerRect.x, innerRect.y, innerRect.width, 76f);
        Rect optionARect = new Rect(innerRect.x, promptRect.yMax + 12f, innerRect.width, 48f);
        Rect optionBRect = new Rect(innerRect.x, optionARect.yMax + 10f, innerRect.width, 48f);

        GUI.Label(promptRect, BuildPanelBody(
            V(_bootEntry.EventPromptLabel),
            Line("EventStatus", V(_bootEntry.EventStatusLabel)),
            Line("EventChoice", V(_bootEntry.EventChoiceLabel))), _bodyStyle);

        Event current = Event.current;
        Vector2 mousePosition = current != null ? current.mousePosition : Vector2.zero;
        string hoveredChoiceKey = string.Empty;

        DrawEventChoiceButton(optionARect, "recover", "[1] " + V(_bootEntry.EventOptionALabel), mousePosition, ref hoveredChoiceKey);
        DrawEventChoiceButton(optionBRect, "loot", "[2] " + V(_bootEntry.EventOptionBLabel), mousePosition, ref hoveredChoiceKey);

        if (string.IsNullOrEmpty(hoveredChoiceKey))
        {
            _bootEntry.SetEventChoiceHover(string.Empty);
        }
    }

    private void DrawEventChoiceButton(Rect rect, string optionKey, string label, Vector2 mousePosition, ref string hoveredChoiceKey)
    {
        bool available = _bootEntry.IsEventChoiceAvailable(optionKey);
        bool selected = _bootEntry.IsEventChoiceSelected(optionKey);
        bool hovered = available && rect.Contains(mousePosition);
        if (hovered)
        {
            hoveredChoiceKey = optionKey;
            _bootEntry.SetEventChoiceHover(optionKey);
        }

        if (DrawInteractiveButton(rect, label, available, hovered, selected))
        {
            _bootEntry.TryTriggerEventChoice(optionKey);
        }
    }

    private void DrawPreEliteChoiceSection(Rect rect)
    {
        DrawOverlaySectionBackground(rect, new Color(0.11f, 0.10f, 0.17f, 0.94f));

        Rect innerRect = new Rect(rect.x + SectionInnerPadding, rect.y + SectionInnerPadding, rect.width - (SectionInnerPadding * 2f), rect.height - (SectionInnerPadding * 2f));
        Rect promptRect = new Rect(innerRect.x, innerRect.y, innerRect.width, 90f);
        Rect optionARect = new Rect(innerRect.x, promptRect.yMax + 12f, innerRect.width, 54f);
        Rect optionBRect = new Rect(innerRect.x, optionARect.yMax + 12f, innerRect.width, 54f);

        GUI.Label(promptRect, BuildPanelBody(
            V(_bootEntry.PreElitePromptLabel),
            Line("PreEliteChoice", V(_bootEntry.PreEliteChoiceRunLabel)),
            Line("TotalPartyHp", V(_bootEntry.TotalPartyHpLabel)),
            Line("PartyCondition", V(_bootEntry.PartyConditionLabel)),
            Line("EliteEncounter", V(_bootEntry.EliteEncounterNameLabel))), _bodyStyle);

        Event current = Event.current;
        Vector2 mousePosition = current != null ? current.mousePosition : Vector2.zero;
        string hoveredChoiceKey = string.Empty;

        DrawPreEliteChoiceButton(optionARect, "recover", "[1] " + V(_bootEntry.PreEliteOptionALabel), mousePosition, ref hoveredChoiceKey);
        DrawPreEliteChoiceButton(optionBRect, "bonus", "[2] " + V(_bootEntry.PreEliteOptionBLabel), mousePosition, ref hoveredChoiceKey);

        if (string.IsNullOrEmpty(hoveredChoiceKey))
        {
            _bootEntry.SetPreEliteChoiceHover(string.Empty);
        }
    }

    private void DrawPreEliteChoiceButton(Rect rect, string optionKey, string label, Vector2 mousePosition, ref string hoveredChoiceKey)
    {
        bool available = _bootEntry.IsPreEliteChoiceAvailable(optionKey);
        bool selected = _bootEntry.IsPreEliteChoiceSelected(optionKey);
        bool hovered = available && rect.Contains(mousePosition);
        if (hovered)
        {
            hoveredChoiceKey = optionKey;
            _bootEntry.SetPreEliteChoiceHover(optionKey);
        }

        if (DrawInteractiveButton(rect, label, available, hovered, selected))
        {
            _bootEntry.TryTriggerPreEliteChoice(optionKey);
        }
    }

    private bool DrawInteractiveButton(Rect rect, string label, bool available, bool hovered, bool selected)
    {
        Color previousColor = GUI.color;
        GUI.color = !available
            ? new Color(0.10f, 0.10f, 0.12f, 0.72f)
            : selected
                ? new Color(0.22f, 0.37f, 0.52f, 0.96f)
                : hovered
                    ? new Color(0.18f, 0.28f, 0.40f, 0.96f)
                    : new Color(0.10f, 0.14f, 0.20f, 0.94f);
        GUI.Box(rect, GUIContent.none, _panelStyle);
        GUI.color = available ? Color.white : new Color(0.6f, 0.64f, 0.68f, 1f);
        GUI.Label(rect, label, _actionButtonStyle);
        GUI.color = previousColor;

        Event current = Event.current;
        if (available && current != null && current.type == EventType.MouseDown && current.button == 0 && rect.Contains(current.mousePosition))
        {
            current.Use();
            return true;
        }

        return false;
    }

    private void DrawOverlaySection(Rect rect, string body, Color backgroundColor)
    {
        DrawOverlaySectionBackground(rect, backgroundColor);
        Rect contentRect = new Rect(rect.x + SectionInnerPadding, rect.y + SectionInnerPadding, rect.width - (SectionInnerPadding * 2f), rect.height - (SectionInnerPadding * 2f));
        GUI.Label(contentRect, body, _bodyStyle);
    }

    private void DrawOverlaySectionBackground(Rect rect, Color backgroundColor)
    {
        Color previousColor = GUI.color;
        GUI.color = backgroundColor;
        GUI.Box(rect, GUIContent.none, _panelStyle);
        GUI.color = previousColor;
    }

    private void DrawTitleBar(Rect rect)
    {
        GUI.Label(new Rect(rect.x + PanelPadding, rect.y + 4f, rect.width - 160f, rect.height), "Prototype HUD", _titleStyle);

        float buttonY = rect.y + 2f;
        float krX = rect.xMax - PanelPadding - LanguageButtonWidth;
        float enX = krX - ChipGap - LanguageButtonWidth;
        DrawLanguageButton(new Rect(enX, buttonY, LanguageButtonWidth, rect.height - 4f), "EN", PrototypeLanguage.English);
        DrawLanguageButton(new Rect(krX, buttonY, LanguageButtonWidth, rect.height - 4f), "KR", PrototypeLanguage.Korean);
    }

    private void DrawLanguageButton(Rect rect, string label, PrototypeLanguage language)
    {
        bool active = _bootEntry.CurrentLanguage == language;
        GUIStyle style = active ? _chipActiveStyle : _chipStyle;
        Color previousColor = GUI.color;
        GUI.color = active ? new Color(0.26f, 0.38f, 0.28f, 1f) : new Color(0.14f, 0.18f, 0.15f, 0.96f);
        if (GUI.Button(rect, label, style))
        {
            _bootEntry.SetLanguage(language);
        }
        GUI.color = previousColor;
    }

    private void DrawFilterChips(Rect rect)
    {
        HudFilter[] filters = new HudFilter[] { HudFilter.All, HudFilter.Simulation, HudFilter.Economy, HudFilter.Selected, HudFilter.Logs };
        float chipWidth = (rect.width - (ChipGap * (filters.Length - 1))) / filters.Length;
        for (int i = 0; i < filters.Length; i++)
        {
            HudFilter filter = filters[i];
            Rect chipRect = new Rect(rect.x + ((chipWidth + ChipGap) * i), rect.y, chipWidth, ChipHeight);
            bool active = _activeFilter == filter;
            GUIStyle style = active ? _chipActiveStyle : _chipStyle;
            Color previousColor = GUI.color;
            GUI.color = active ? new Color(0.18f, 0.20f, 0.18f, 1f) : new Color(0.09f, 0.10f, 0.11f, 0.94f);
            if (GUI.Button(chipRect, GetFilterLabel(filter), style))
            {
                _activeFilter = filter;
            }
            GUI.color = previousColor;
        }
    }

    private void DrawSearchBar(Rect rect)
    {
        GUI.Label(new Rect(rect.x, rect.y + 3f, SearchLabelWidth, rect.height), T("Search"), _bodyStyle);
        Rect fieldRect = new Rect(rect.x + SearchLabelWidth, rect.y, rect.width - SearchLabelWidth - SearchClearButtonWidth - ChipGap, rect.height);
        Rect clearRect = new Rect(fieldRect.xMax + ChipGap, rect.y, SearchClearButtonWidth, rect.height);
        GUI.SetNextControlName(SearchFieldControlName);
        _searchText = GUI.TextField(fieldRect, _searchText ?? string.Empty, _searchFieldStyle);
        if (GUI.Button(clearRect, T("Clear"), _chipStyle))
        {
            _searchText = string.Empty;
            GUI.FocusControl(string.Empty);
            _isSearchFieldFocused = false;
            return;
        }

        _isSearchFieldFocused = GUI.GetNameOfFocusedControl() == SearchFieldControlName;
    }

    private List<HudPanel> FilterPanels(List<HudPanel> panels)
    {
        List<HudPanel> visiblePanels = new List<HudPanel>();
        if (panels == null)
        {
            return visiblePanels;
        }

        for (int i = 0; i < panels.Count; i++)
        {
            HudPanel panel = panels[i];
            if (_activeFilter != HudFilter.All && panel.Group != _activeFilter)
            {
                continue;
            }

            if (!MatchesQuery(panel, _searchText))
            {
                continue;
            }

            visiblePanels.Add(panel);
        }

        return visiblePanels;
    }

    private static bool MatchesQuery(HudPanel panel, string query)
    {
        if (panel == null || string.IsNullOrEmpty(query))
        {
            return true;
        }

        string normalizedQuery = query.Trim().ToLowerInvariant();
        if (normalizedQuery.Length == 0)
        {
            return true;
        }

        string title = string.IsNullOrEmpty(panel.Title) ? string.Empty : panel.Title.ToLowerInvariant();
        string body = string.IsNullOrEmpty(panel.Body) ? string.Empty : panel.Body.ToLowerInvariant();
        return title.Contains(normalizedQuery) || body.Contains(normalizedQuery);
    }

    private float CalculateContentHeight(List<HudPanel> panels, float width)
    {
        if (panels == null || panels.Count == 0)
        {
            return 24f;
        }

        float height = 0f;
        float bodyWidth = Mathf.Max(32f, width - 20f);
        for (int i = 0; i < panels.Count; i++)
        {
            bool expanded = IsExpanded(panels[i].Key);
            float bodyHeight = expanded ? Mathf.Max(28f, _bodyStyle.CalcHeight(new GUIContent(panels[i].Body), bodyWidth)) : 0f;
            height += 28f + (expanded ? bodyHeight + 16f : 0f) + PanelGap;
        }

        return height;
    }

    private void DrawPanels(Rect rect, List<HudPanel> panels)
    {
        float y = rect.y;
        for (int i = 0; i < panels.Count; i++)
        {
            HudPanel panel = panels[i];
            bool expanded = IsExpanded(panel.Key);
            float bodyHeight = expanded ? Mathf.Max(28f, _bodyStyle.CalcHeight(new GUIContent(panel.Body), rect.width - 20f)) : 0f;
            float panelHeight = 28f + (expanded ? bodyHeight + 16f : 0f);
            Rect panelRect = new Rect(rect.x, y, rect.width, panelHeight);

            Color previousColor = GUI.color;
            GUI.color = new Color(0.06f, 0.08f, 0.09f, 0.92f);
            GUI.Box(panelRect, GUIContent.none, _panelStyle);
            GUI.color = previousColor;

            Rect headerRect = new Rect(panelRect.x + 4f, panelRect.y + 2f, panelRect.width - 8f, 24f);
            string foldoutLabel = (expanded ? "[-] " : "[+] ") + panel.Title;
            if (GUI.Button(headerRect, foldoutLabel, _foldoutStyle))
            {
                _expandedByKey[panel.Key] = !expanded;
                expanded = !expanded;
            }

            if (expanded)
            {
                Rect bodyRect = new Rect(panelRect.x + 10f, headerRect.yMax + 6f, panelRect.width - 20f, panelHeight - headerRect.height - 10f);
                GUI.Label(bodyRect, panel.Body, _bodyStyle);
            }

            y += panelHeight + PanelGap;
        }
    }
    private List<HudPanel> BuildPanels()
    {
        if (_bootEntry != null && _bootEntry.IsDungeonRunHudMode)
        {
            if (_bootEntry.IsDungeonBattleViewActive || _bootEntry.IsDungeonRouteChoiceVisible || _bootEntry.IsDungeonEventChoiceVisible || _bootEntry.IsDungeonResultPanelVisible)
            {
                return new List<HudPanel>();
            }

            return new List<HudPanel>
            {
                new HudPanel("dungeon_run", T("PanelDungeonRun"), BuildPanelBody(
                    Line("Step", _bootEntry.DebugStepLabel),
                    Line("WorldSimControls", _bootEntry.DungeonRunWorldControlsLabel),
                    Line("DungeonExploreControls", _bootEntry.DungeonRunExploreControlsLabel),
                    Line("DungeonBattleControls", _bootEntry.DungeonRunBattleControlsLabel),
                    Line("DungeonTargetControls", _bootEntry.DungeonRunTargetControlsLabel),
                    Line("DungeonRouteControls", _bootEntry.DungeonRunRouteControlsLabel),
                    Line("RunState", V(_bootEntry.DungeonRunStateLabel)),
                    Line("CurrentCity", V(_bootEntry.CurrentCityRunLabel)),
                    Line("CurrentDungeon", V(_bootEntry.CurrentDungeonRunLabel)),
                    Line("DungeonDanger", V(_bootEntry.CurrentDungeonDangerLabel)),
                    Line("CityManaShardStock", V(_bootEntry.CurrentCityManaShardStockRunLabel)),
                    Line("NeedPressure", V(_bootEntry.CurrentNeedPressureRunLabel)),
                    Line("DispatchPolicy", V(_bootEntry.CurrentDispatchPolicyRunLabel)),
                        Line("RecommendedRoute", V(_bootEntry.RecommendedRouteRunLabel)),
                    Line("RecommendationReason", V(_bootEntry.RecommendationReasonRunLabel)),
                    Line("ExpectedNeedImpact", V(_bootEntry.ExpectedNeedImpactRunLabel)),
                    Line("ActiveParty", V(_bootEntry.ActiveDungeonPartyLabel)),
                    Line("CurrentRoom", V(_bootEntry.CurrentRoomLabel)),
                    Line("CurrentRoomType", V(_bootEntry.CurrentRoomTypeLabel)),
                    Line("RoomProgress", V(_bootEntry.RoomProgressLabel)),
                    Line("NextMajorGoal", V(_bootEntry.NextMajorGoalLabel)),
                    Line("TotalPartyHp", V(_bootEntry.TotalPartyHpLabel)),
                    Line("PartyCondition", V(_bootEntry.PartyConditionLabel)),
                    Line("SustainPressure", V(_bootEntry.SustainPressureLabel)),
                    Line("CurrentRoute", V(_bootEntry.CurrentRouteLabel)),
                    Line("RouteRisk", V(_bootEntry.RouteRiskLabel)),
                    Line("CarriedLoot", V(_bootEntry.CarriedLootRunLabel)),
                    Line("RunTurnCount", _bootEntry.DungeonRunTurnCount),
                    Line("EncounterProgress", V(_bootEntry.EncounterProgressLabel)),
                    Line("EliteStatus", V(_bootEntry.EliteStatusLabel)),
                    Line("ExitUnlocked", V(_bootEntry.ExitUnlockedLabel)),
                    Line("ChestOpened", V(_bootEntry.ChestOpenedLabel)),
                    Line("EventStatus", V(_bootEntry.EventStatusLabel)),
                    Line("EventChoice", V(_bootEntry.EventChoiceLabel)),
                Line("PreEliteChoice", V(_bootEntry.PreEliteChoiceRunLabel)),
                    Line("LootBreakdown", V(_bootEntry.LootBreakdownLabel)),
                    Line("LastBattleLog1", V(_bootEntry.LastBattleLog1Label)),
                    Line("LastBattleLog2", V(_bootEntry.LastBattleLog2Label)),
                    Line("LastBattleLog3", V(_bootEntry.LastBattleLog3Label))), HudFilter.Simulation)
            };
        }

        return new List<HudPanel>
        {
            new HudPanel("sim_status", T("PanelSimulationStatus"), BuildPanelBody(
                Line("Prototype", _bootEntry.PrototypeNameLabel),
                Line("Step", _bootEntry.DebugStepLabel),
                Line("Language", _bootEntry.CurrentLanguageLabel),
                Line("CurrentState", V(_bootEntry.CurrentStateLabel)),
                Line("LastTransition", V(_bootEntry.LastTransitionLabel)),
                Line("Visible", BuildVisibleSummary())), HudFilter.Simulation),
            new HudPanel("world_snapshot", T("PanelWorldSnapshot"), BuildPanelBody(
                Line("WorldEntities", _bootEntry.WorldEntityCount),
                Line("WorldRoutes", _bootEntry.WorldRouteCount),
                Line("ResourceCount", _bootEntry.ResourceCount),
                Line("ResourceIds", V(_bootEntry.ResourceIdsLabel)),
                Line("Route1Id", V(_bootEntry.FirstRouteIdLabel)),
                Line("Route1Tags", V(_bootEntry.FirstRouteTagsLabel)),
                Line("Route1Link", V(_bootEntry.FirstRouteLinkLabel)),
                Line("Route1Capacity", V(_bootEntry.FirstRouteCapacityLabel)),
                Line("Route1Usage", V(_bootEntry.FirstRouteUsageLabel)),
                Line("Route1Utilization", V(_bootEntry.FirstRouteUtilizationLabel)),
                Line("CameraPosition", GetCameraPositionText()),
                Line("Zoom", GetZoomText()),
                Line("Controls", _bootEntry.ControlsLabel)), HudFilter.Simulation),
            new HudPanel("trade_flow", T("PanelTradeFlow"), BuildPanelBody(
                Line("TradeOpportunities", _bootEntry.TradeOpportunityCount),
                Line("ActiveTradeOpportunities", _bootEntry.ActiveTradeOpportunityCount),
                Line("UnmetCityNeeds", _bootEntry.UnmetCityNeedsCount),
                Line("TradeLink1", V(_bootEntry.TradeLink1Label)),
                Line("TradeLink2", V(_bootEntry.TradeLink2Label)),
                Line("DungeonOutputHint", V(_bootEntry.DungeonOutputHintLabel))), HudFilter.Economy),
            new HudPanel("economy_control", T("PanelEconomyControl"), BuildPanelBody(
                Line("EconomyControls", _bootEntry.EconomyControlsLabel),
                Line("WorldDay", _bootEntry.WorldDayCount),
                Line("AutoTickEnabled", BoolText(_bootEntry.AutoTickEnabled)),
                Line("AutoTickPaused", BoolText(_bootEntry.AutoTickPaused)),
                Line("TickInterval", _bootEntry.TickIntervalSeconds.ToString("0.00")),
                Line("AutoTickCount", _bootEntry.AutoTickCount),
                Line("TradeStepCount", _bootEntry.TradeStepCount)), HudFilter.Economy),
            new HudPanel("expedition_loop", T("PanelExpeditionLoop"), BuildPanelBody(
                Line("ExpeditionControls", _bootEntry.ExpeditionControlsLabel),
                Line("TotalParties", _bootEntry.TotalParties),
                Line("IdleParties", _bootEntry.IdleParties),
                Line("ActiveExpeditions", _bootEntry.ActiveExpeditions),
                Line("AvailableContracts", _bootEntry.AvailableContracts),
                Line("ExpeditionSuccesses", _bootEntry.ExpeditionSuccessCount),
                Line("ExpeditionFailures", _bootEntry.ExpeditionFailureCount),
                Line("ExpeditionLootReturned", _bootEntry.ExpeditionLootReturnedTotal),
                Line("RecentExpeditionLog1", V(_bootEntry.RecentExpeditionLog1Label)),
                Line("RecentExpeditionLog2", V(_bootEntry.RecentExpeditionLog2Label)),
                Line("RecentExpeditionLog3", V(_bootEntry.RecentExpeditionLog3Label))), HudFilter.Economy),
            new HudPanel("dungeon_run", T("PanelDungeonRun"), BuildPanelBody(
                Line("Step", _bootEntry.DebugStepLabel),
                Line("WorldSimControls", _bootEntry.DungeonRunWorldControlsLabel),
                Line("DungeonExploreControls", _bootEntry.DungeonRunExploreControlsLabel),
                Line("DungeonBattleControls", _bootEntry.DungeonRunBattleControlsLabel),
                Line("DungeonTargetControls", _bootEntry.DungeonRunTargetControlsLabel),
                Line("DungeonRouteControls", _bootEntry.DungeonRunRouteControlsLabel),
                Line("RunState", V(_bootEntry.DungeonRunStateLabel)),
                Line("CurrentCity", V(_bootEntry.CurrentCityRunLabel)),
                Line("CurrentDungeon", V(_bootEntry.CurrentDungeonRunLabel)),
                Line("DungeonDanger", V(_bootEntry.CurrentDungeonDangerLabel)),
                Line("CityManaShardStock", V(_bootEntry.CurrentCityManaShardStockRunLabel)),
                Line("NeedPressure", V(_bootEntry.CurrentNeedPressureRunLabel)),
                Line("DispatchReadiness", V(_bootEntry.CurrentDispatchReadinessRunLabel)),
                Line("RecoveryProgress", V(_bootEntry.CurrentDispatchRecoveryProgressRunLabel)),
                Line("ConsecutiveDispatches", V(_bootEntry.CurrentDispatchConsecutiveRunLabel)),
                Line("DispatchPolicy", V(_bootEntry.CurrentDispatchPolicyRunLabel)),
                Line("RecommendedRoute", V(_bootEntry.RecommendedRouteRunLabel)),
                Line("RecommendationReason", V(_bootEntry.RecommendationReasonRunLabel)),
                Line("RecoveryAdvice", V(_bootEntry.RecoveryAdviceRunLabel)),
                Line("ExpectedNeedImpact", V(_bootEntry.ExpectedNeedImpactRunLabel)),
                Line("ActiveParty", V(_bootEntry.ActiveDungeonPartyLabel)),
                Line("CurrentRoom", V(_bootEntry.CurrentRoomLabel)),
                Line("CurrentRoomType", V(_bootEntry.CurrentRoomTypeLabel)),
                Line("RoomProgress", V(_bootEntry.RoomProgressLabel)),
                Line("NextMajorGoal", V(_bootEntry.NextMajorGoalLabel)),
                    Line("TotalPartyHp", V(_bootEntry.TotalPartyHpLabel)),
                    Line("PartyCondition", V(_bootEntry.PartyConditionLabel)),
                    Line("SustainPressure", V(_bootEntry.SustainPressureLabel)),
                Line("CurrentRoute", V(_bootEntry.CurrentRouteLabel)),
                Line("RouteRisk", V(_bootEntry.RouteRiskLabel)),
                Line("CarriedLoot", V(_bootEntry.CarriedLootRunLabel)),
                Line("RunTurnCount", _bootEntry.DungeonRunTurnCount),
                Line("EncounterProgress", V(_bootEntry.EncounterProgressLabel)),
                Line("ExitUnlocked", V(_bootEntry.ExitUnlockedLabel)),
                Line("ChestOpened", V(_bootEntry.ChestOpenedLabel)),
                Line("EventStatus", V(_bootEntry.EventStatusLabel)),
                Line("EventChoice", V(_bootEntry.EventChoiceLabel)),
                Line("PreEliteChoice", V(_bootEntry.PreEliteChoiceRunLabel)),
                Line("LootBreakdown", V(_bootEntry.LootBreakdownLabel)),
                Line("LastBattleLog1", V(_bootEntry.LastBattleLog1Label)),
                Line("LastBattleLog2", V(_bootEntry.LastBattleLog2Label)),
                Line("LastBattleLog3", V(_bootEntry.LastBattleLog3Label))), HudFilter.Simulation),
            new HudPanel("dungeon_battle", T("PanelDungeonBattle"), BuildPanelBody(
                "Battle View: " + V(_bootEntry.BattleViewStateLabel),
                "Encounter: " + V(_bootEntry.EncounterNameLabel),
                Line("EncounterRoomType", V(_bootEntry.EncounterRoomTypeLabel)),
                Line("BattleState", V(_bootEntry.BattleStateLabel)),
                Line("CurrentActor", V(_bootEntry.CurrentBattleActorLabel)),
                "Phase: " + V(_bootEntry.BattlePhaseLabel),
                Line("EliteEncounter", V(_bootEntry.EliteEncounterNameLabel)),
                Line("EliteType", V(_bootEntry.EliteTypeLabel)),
                Line("EliteHp", V(_bootEntry.EliteHpLabel)),
                Line("EliteRewardHint", V(_bootEntry.EliteRewardHintLabel)),
                Line("EliteDefeated", V(_bootEntry.EliteDefeatedLabel)),
                Line("EliteIntent", V(_bootEntry.EnemyIntentLabel)),
                Line("Monster1", V(_bootEntry.BattleMonster1Label)),
                Line("Monster2", V(_bootEntry.BattleMonster2Label)),
                Line("Party1Hp", V(_bootEntry.Party1HpLabel)),
                Line("Party2Hp", V(_bootEntry.Party2HpLabel)),
                Line("Party3Hp", V(_bootEntry.Party3HpLabel)),
                Line("Party4Hp", V(_bootEntry.Party4HpLabel)),
                "Feedback: " + V(_bootEntry.BattleFeedbackLabel),
                Line("CurrentSelectionPrompt", V(_bootEntry.CurrentSelectionPromptLabel))), HudFilter.Logs),
            new HudPanel("dungeon_result", T("PanelDungeonResult"), BuildPanelBody(
                Line("Result", V(_bootEntry.ResultPanelStateLabel)),
                Line("CityDispatchedFrom", V(_bootEntry.ResultPanelCityDispatchedFromLabel)),
                Line("DungeonChosen", V(_bootEntry.ResultPanelDungeonChosenLabel)),
                Line("DungeonDanger", V(_bootEntry.ResultPanelDungeonDangerLabel)),
                Line("RecommendedRoute", V(_bootEntry.ResultPanelRecommendedRouteLabel)),
                Line("FollowedRecommendation", V(_bootEntry.ResultPanelFollowedRecommendationLabel)),
                Line("ManaShardsReturned", V(_bootEntry.ResultPanelManaShardsReturnedLabel)),
                Line("StockBefore", V(_bootEntry.ResultPanelStockBeforeLabel)),
                Line("StockAfter", V(_bootEntry.ResultPanelStockAfterLabel)),
                Line("StockDelta", V(_bootEntry.ResultPanelStockDeltaLabel)),
                Line("NeedPressureBefore", V(_bootEntry.ResultPanelNeedPressureBeforeLabel)),
                Line("NeedPressureAfter", V(_bootEntry.ResultPanelNeedPressureAfterLabel)),
                Line("TurnsTaken", V(_bootEntry.ResultPanelTurnsTakenLabel)),
                Line("LootGained", V(_bootEntry.ResultPanelLootGainedLabel)),
                Line("RouteChosen", V(_bootEntry.ResultPanelRouteChosenLabel)),
                Line("RouteRisk", V(_bootEntry.ResultPanelRouteRiskLabel)),
                Line("BattleLoot", V(_bootEntry.ResultPanelBattleLootLabel)),
                Line("ChestLoot", V(_bootEntry.ResultPanelChestLootLabel)),
                Line("EventLoot", V(_bootEntry.ResultPanelEventLootLabel)),
                Line("EventChoice", V(_bootEntry.ResultPanelEventChoiceLabel)),
                Line("PreEliteChoice", V(_bootEntry.ResultPanelPreEliteChoiceLabel)),
                Line("PreEliteHealAmount", V(_bootEntry.ResultPanelPreEliteHealAmountLabel)),
                Line("EliteBonusRewardEarned", V(_bootEntry.ResultPanelEliteBonusRewardEarnedLabel)),
                Line("EliteBonusRewardAmount", V(_bootEntry.ResultPanelEliteBonusRewardAmountLabel)),
                Line("RoomPathSummary", V(_bootEntry.ResultPanelRoomPathSummaryLabel)),
                Line("PartyHpSummary", V(_bootEntry.ResultPanelPartyHpSummaryLabel)),
                Line("PartyConditionAtEnd", V(_bootEntry.ResultPanelPartyConditionLabel)),
                Line("EliteDefeated", V(_bootEntry.ResultPanelEliteDefeatedLabel)),
                Line("EliteName", V(_bootEntry.ResultPanelEliteNameLabel)),
                Line("EliteRewardIdentity", V(_bootEntry.ResultPanelEliteRewardIdentityLabel)),
                Line("EliteRewardAmount", V(_bootEntry.ResultPanelEliteRewardAmountLabel)),
                Line("SurvivingMembers", V(_bootEntry.ResultPanelSurvivingMembersLabel)),
                Line("ClearedEncounters", V(_bootEntry.ResultPanelClearedEncountersLabel)),
                Line("OpenedChests", V(_bootEntry.ResultPanelOpenedChestsLabel)),
                Line("ReturnToWorldPrompt", V(_bootEntry.ResultPanelReturnPromptLabel))), HudFilter.Logs),
            new HudPanel("economy_pressure", T("PanelEconomyPressure"), BuildPanelBody(
                Line("ProducedTotal", _bootEntry.ProducedTotal),
                Line("ClaimedDungeonOutputs", _bootEntry.ClaimedDungeonOutputsTotal),
                Line("TradedTotal", _bootEntry.TradedTotal),
                Line("ProcessedTotal", _bootEntry.ProcessedTotal),
                Line("ConsumedTotal", _bootEntry.ConsumedTotal),
                Line("CriticalFulfilledTotal", _bootEntry.CriticalFulfilledTotal),
                Line("CriticalUnmetTotal", _bootEntry.CriticalUnmetTotal),
                Line("NormalFulfilledTotal", _bootEntry.NormalFulfilledTotal),
                Line("NormalUnmetTotal", _bootEntry.NormalUnmetTotal),
                Line("FulfilledTotal", _bootEntry.FulfilledTotal),
                Line("UnmetTotal", _bootEntry.UnmetTotal),
                Line("ShortagesTotal", _bootEntry.ShortagesTotal),
                Line("ProcessingBlockedTotal", _bootEntry.ProcessingBlockedTotal),
                Line("ProcessingReservedTotal", _bootEntry.ProcessingReservedTotal),
                Line("UnclaimedDungeonOutputs", _bootEntry.UnclaimedDungeonOutputsTotal),
                Line("ActiveTradeOpportunities", _bootEntry.ActiveTradeOpportunityCount),
                Line("RouteCapacityUsed", V(_bootEntry.RouteCapacityUsedLabel)),
                Line("SaturatedRoutes", V(_bootEntry.SaturatedRoutesLabel)),
                Line("CitiesWithShortages", V(_bootEntry.CitiesWithShortagesLabel)),
                Line("CitiesWithSurplus", V(_bootEntry.CitiesWithSurplusLabel)),
                Line("CitiesWithProcessing", V(_bootEntry.CitiesWithProcessingLabel)),
                Line("CitiesWithCriticalUnmet", V(_bootEntry.CitiesWithCriticalUnmetLabel))), HudFilter.Economy),
            new HudPanel("selected_entity", T("PanelSelectedEntity"), BuildPanelBody(
                Line("Selected", V(_bootEntry.SelectedDisplayName)),
                Line("SelectedType", V(_bootEntry.SelectedTypeLabel)),
                Line("SelectedPosition", V(_bootEntry.SelectedPositionLabel)),
                Line("SelectedId", V(_bootEntry.SelectedIdLabel)),
                Line("SelectedTags", V(_bootEntry.SelectedTagsLabel)),
                Line("SelectedStat", V(_bootEntry.SelectedStatLabel)),
                Line("SelectedResources", V(_bootEntry.SelectedResourcesLabel)),
                Line("SelectedResourceRoles", V(_bootEntry.SelectedResourceRolesLabel))), HudFilter.Selected),
            new HudPanel("selected_economy", T("PanelSelectedEconomy"), BuildPanelBody(
                Line("SelectedSupply", V(_bootEntry.SelectedSupplyLabel)),
                Line("SelectedNeeds", V(_bootEntry.SelectedNeedsLabel)),
                Line("SelectedHighPriorityNeeds", V(_bootEntry.SelectedHighPriorityNeedsLabel)),
                Line("SelectedNormalPriorityNeeds", V(_bootEntry.SelectedNormalPriorityNeedsLabel)),
                Line("SelectedOutput", V(_bootEntry.SelectedOutputLabel)),
                Line("SelectedProcessing", V(_bootEntry.SelectedProcessingLabel)),
                Line("LinkedCity", V(_bootEntry.SelectedLinkedCityLabel)),
                Line("PartiesInCity", V(_bootEntry.SelectedPartiesInCityLabel)),
                Line("IdleParties", V(_bootEntry.SelectedIdlePartiesLabel)),
                Line("ActiveExpeditionsFromCity", V(_bootEntry.SelectedActiveExpeditionsFromCityLabel)),
                Line("AvailableContract", V(_bootEntry.SelectedAvailableContractLabel)),
                Line("LinkedDungeon", V(_bootEntry.SelectedLinkedDungeonLabel)),
                Line("DungeonDanger", V(_bootEntry.SelectedDungeonDangerLabel)),
                Line("CityManaShardStock", V(_bootEntry.SelectedCityManaShardStockLabel)),
                Line("NeedPressure", V(_bootEntry.SelectedNeedPressureLabel)),
                Line("DispatchReadiness", V(_bootEntry.SelectedDispatchReadinessLabel)),
                Line("RecoveryProgress", V(_bootEntry.SelectedDispatchRecoveryProgressLabel)),
                Line("DispatchPolicy", V(_bootEntry.SelectedDispatchPolicyLabel)),
                Line("RecommendedRoute", V(_bootEntry.SelectedRecommendedRouteSummaryLabel)),
                Line("RecommendedRouteForLinkedCity", V(_bootEntry.SelectedRecommendedRouteForLinkedCityLabel)),
                Line("RecommendationReason", V(_bootEntry.SelectedRecommendationReasonLabel)),
                Line("LastDispatchImpact", V(_bootEntry.SelectedLastDispatchImpactLabel)),
                Line("LastDispatchStockDelta", V(_bootEntry.SelectedLastDispatchStockDeltaLabel)),
                Line("LastNeedPressureChange", V(_bootEntry.SelectedLastNeedPressureChangeLabel)),
                Line("LastDispatchReadinessChange", V(_bootEntry.SelectedLastDispatchReadinessChangeLabel)),
                Line("RecoveryEta", V(_bootEntry.SelectedRecoveryEtaLabel)),
                Line("RecommendedPower", V(_bootEntry.SelectedRecommendedPowerLabel)),
                Line("ExpeditionDurationDays", V(_bootEntry.SelectedExpeditionDurationDaysLabel)),
                Line("MonsterCountPreview", V(_bootEntry.SelectedMonsterCountPreviewLabel)),
                Line("RewardPreview", V(_bootEntry.SelectedRewardPreviewLabel)),
                Line("EventPreview", V(_bootEntry.SelectedEventPreviewLabel)),
                Line("RoutePreview1", V(_bootEntry.SelectedRoutePreview1Label)),
                Line("RoutePreview2", V(_bootEntry.SelectedRoutePreview2Label)),
                Line("ActiveExpeditions", V(_bootEntry.SelectedActiveExpeditionsLabel)),
                Line("LastExpeditionResult", V(_bootEntry.SelectedLastExpeditionResultLabel)),
                Line("LastRunLootReturned", V(_bootEntry.SelectedExpeditionLootReturnedLabel)),
                Line("LastRunSurvivingMembers", V(_bootEntry.SelectedLastRunSurvivingMembersLabel)),
                Line("LastRunEventChoice", V(_bootEntry.SelectedLastRunEventChoiceLabel)),
                Line("LastRunDungeon", V(_bootEntry.SelectedLastRunDungeonLabel)),
                Line("LastRunRoute", V(_bootEntry.SelectedLastRunRouteLabel)),
                Line("LastRunLootBreakdown", V(_bootEntry.SelectedLastRunLootBreakdownLabel)),
                Line("LastRunClearedEncounters", V(_bootEntry.SelectedLastRunClearedEncountersLabel)),
                Line("SelectedStocks", V(_bootEntry.SelectedStocksLabel)),
                Line("SelectedSurplus", V(_bootEntry.SelectedSurplusLabel)),
                Line("SelectedDeficit", V(_bootEntry.SelectedDeficitLabel)),
                Line("SelectedIdentity", V(_bootEntry.SelectedIdentityLabel)),
                Line("ReserveStockRule", V(_bootEntry.SelectedReserveStockRuleLabel)),
                Line("ProcessingPreference", V(_bootEntry.SelectedProcessingPreferenceLabel))), HudFilter.Selected),
            new HudPanel("selected_day_metrics", T("PanelSelectedDayMetrics"), BuildPanelBody(
                Line("LastDayProduced", V(_bootEntry.SelectedLastDayProducedLabel)),
                Line("LastDayDungeonImports", V(_bootEntry.SelectedLastDayDungeonImportsLabel)),
                Line("LastDayImported", V(_bootEntry.SelectedLastDayImportedLabel)),
                Line("LastDayExported", V(_bootEntry.SelectedLastDayExportedLabel)),
                Line("LastDayProcessedIn", V(_bootEntry.SelectedLastDayProcessedInLabel)),
                Line("LastDayProcessedOut", V(_bootEntry.SelectedLastDayProcessedOutLabel)),
                Line("LastDayProcessedTotal", V(_bootEntry.SelectedLastDayProcessedTotalLabel)),
                Line("LastDayConsumed", V(_bootEntry.SelectedLastDayConsumedLabel)),
                Line("LastDayCriticalFulfilled", V(_bootEntry.SelectedLastDayCriticalFulfilledLabel)),
                Line("LastDayCriticalUnmet", V(_bootEntry.SelectedLastDayCriticalUnmetLabel)),
                Line("LastDayNormalFulfilled", V(_bootEntry.SelectedLastDayNormalFulfilledLabel)),
                Line("LastDayNormalUnmet", V(_bootEntry.SelectedLastDayNormalUnmetLabel)),
                Line("LastDayFulfilled", V(_bootEntry.SelectedLastDayFulfilledLabel)),
                Line("LastDayUnmet", V(_bootEntry.SelectedLastDayUnmetLabel)),
                Line("LastDayShortages", V(_bootEntry.SelectedLastDayShortagesLabel)),
                Line("TotalFulfilled", V(_bootEntry.SelectedTotalFulfilledLabel)),
                Line("TotalUnmet", V(_bootEntry.SelectedTotalUnmetLabel)),
                Line("TotalCriticalUnmet", V(_bootEntry.SelectedTotalCriticalUnmetLabel)),
                Line("TotalNormalUnmet", V(_bootEntry.SelectedTotalNormalUnmetLabel)),
                Line("TotalShortages", V(_bootEntry.SelectedTotalShortagesLabel)),
                Line("LastDayProcessingBlocked", V(_bootEntry.SelectedLastDayProcessingBlockedLabel)),
                Line("LastDayProcessingReserved", V(_bootEntry.SelectedLastDayProcessingReservedLabel)),
                Line("LastDayClaimedOut", V(_bootEntry.SelectedLastDayClaimedOutLabel))), HudFilter.Selected),
            new HudPanel("selected_trade_signals", T("PanelSelectedTradeSignals"), BuildPanelBody(
                Line("SelectedIncomingTrade", V(_bootEntry.SelectedIncomingTradeLabel)),
                Line("SelectedOutgoingTrade", V(_bootEntry.SelectedOutgoingTradeLabel)),
                Line("SelectedUnmetNeeds", V(_bootEntry.SelectedUnmetNeedsLabel))), HudFilter.Selected),
            new HudPanel("recent_logs", T("PanelRecentDayLogs"), BuildPanelBody(
                Line("RecentDayLog1", V(_bootEntry.RecentDayLog1Label)),
                Line("RecentDayLog2", V(_bootEntry.RecentDayLog2Label)),
                Line("RecentDayLog3", V(_bootEntry.RecentDayLog3Label))), HudFilter.Logs)
        };
    }

    private string T(string key)
    {
        return _bootEntry != null ? _bootEntry.GetText(key) : key;
    }

    private string V(string value)
    {
        return _bootEntry != null ? _bootEntry.TranslateValue(value) : value;
    }

    private string Line(string labelKey, string value)
    {
        return T(labelKey) + ": " + value;
    }

    private string Line(string labelKey, int value)
    {
        return Line(labelKey, value.ToString());
    }

    private string BuildVisibleSummary()
    {
        return T("CitiesLabel") + " " + _bootEntry.VisibleCityCount + " / " +
               T("DungeonsLabel") + " " + _bootEntry.VisibleDungeonCount + " / " +
               T("RoadsLabel") + " " + _bootEntry.VisibleRoadCount;
    }

    private string BoolText(bool value)
    {
        return T(value ? "BoolOn" : "BoolOff");
    }

    private string GetFilterLabel(HudFilter filter)
    {
        return filter == HudFilter.All
            ? T("FilterAll")
            : filter == HudFilter.Simulation
                ? T("FilterSimulation")
                : filter == HudFilter.Economy
                    ? T("FilterEconomy")
                    : filter == HudFilter.Selected
                        ? T("FilterSelected")
                        : T("FilterLogs");
    }

    private static string BuildPanelBody(params string[] lines)
    {
        if (lines == null || lines.Length == 0)
        {
            return "None";
        }

        List<string> filtered = new List<string>();
        for (int i = 0; i < lines.Length; i++)
        {
            if (!string.IsNullOrEmpty(lines[i]))
            {
                filtered.Add(lines[i]);
            }
        }

        return filtered.Count == 0 ? "None" : string.Join("\n", filtered.ToArray());
    }

    private void AddOptionalSummaryLine(List<string> lines, string prefix, string value)
    {
        if (lines == null || !HasMeaningfulText(value))
        {
            return;
        }

        lines.Add(prefix + ": " + value);
    }

    private static bool HasMeaningfulText(string value)
    {
        return !string.IsNullOrEmpty(value) && value != "None" && value != "(missing)";
    }
    private void EnsureExpandedState()
    {
        if (_expandedByKey != null)
        {
            return;
        }

        _expandedByKey = new Dictionary<string, bool>
        {
            { "sim_status", true },
            { "world_snapshot", false },
            { "trade_flow", false },
            { "economy_control", false },
            { "expedition_loop", true },
            { "economy_pressure", true },
            { "selected_entity", true },
            { "selected_economy", true },
            { "selected_day_metrics", false },
            { "selected_trade_signals", false },
            { "dungeon_run", true },
            { "dungeon_battle", true },
            { "dungeon_result", true },
            { "recent_logs", false }
        };
    }

    private bool IsExpanded(string key)
    {
        if (_expandedByKey == null || string.IsNullOrEmpty(key))
        {
            return false;
        }

        bool expanded;
        if (_expandedByKey.TryGetValue(key, out expanded))
        {
            return expanded;
        }

        _expandedByKey[key] = false;
        return false;
    }

    private void EnsureStyles()
    {
        if (_bodyStyle != null)
        {
            return;
        }

        _bodyStyle = new GUIStyle(GUI.skin != null ? GUI.skin.label : GUIStyle.none);
        _bodyStyle.fontSize = 13;
        _bodyStyle.wordWrap = true;
        _bodyStyle.alignment = TextAnchor.UpperLeft;
        _bodyStyle.normal.textColor = new Color(0.92f, 0.95f, 0.92f, 1f);

        _titleStyle = new GUIStyle(_bodyStyle);
        _titleStyle.fontSize = 15;
        _titleStyle.fontStyle = FontStyle.Bold;
        _titleStyle.normal.textColor = Color.white;

        _chipStyle = new GUIStyle(GUI.skin != null ? GUI.skin.button : GUIStyle.none);
        _chipStyle.fontSize = 12;
        _chipStyle.alignment = TextAnchor.MiddleCenter;
        _chipStyle.normal.textColor = new Color(0.88f, 0.92f, 0.88f, 1f);
        _chipStyle.hover.textColor = Color.white;
        _chipStyle.padding = new RectOffset(8, 8, 3, 3);

        _chipActiveStyle = new GUIStyle(_chipStyle);
        _chipActiveStyle.fontStyle = FontStyle.Bold;
        _chipActiveStyle.normal.textColor = Color.white;

        _foldoutStyle = new GUIStyle(GUI.skin != null ? GUI.skin.button : GUIStyle.none);
        _foldoutStyle.fontSize = 13;
        _foldoutStyle.fontStyle = FontStyle.Bold;
        _foldoutStyle.alignment = TextAnchor.MiddleLeft;
        _foldoutStyle.padding = new RectOffset(10, 8, 3, 3);
        _foldoutStyle.normal.textColor = Color.white;

        _searchFieldStyle = new GUIStyle(GUI.skin != null ? GUI.skin.textField : GUIStyle.none);
        _searchFieldStyle.fontSize = 12;
        _searchFieldStyle.alignment = TextAnchor.MiddleLeft;

        _actionButtonStyle = new GUIStyle(_bodyStyle);
        _actionButtonStyle.fontSize = 14;
        _actionButtonStyle.fontStyle = FontStyle.Bold;
        _actionButtonStyle.alignment = TextAnchor.MiddleCenter;
        _actionButtonStyle.wordWrap = false;

        _panelStyle = new GUIStyle(GUI.skin != null ? GUI.skin.box : GUIStyle.none);
        _panelStyle.border = new RectOffset(1, 1, 1, 1);
        _panelStyle.padding = new RectOffset(0, 0, 0, 0);
        _panelStyle.margin = new RectOffset(0, 0, 0, 0);
    }

    private void CacheBootEntry()
    {
        if (_bootEntry == null)
        {
            _bootEntry = GetComponent<BootEntry>();
        }
    }

    private string GetCameraPositionText()
    {
        Camera mainCamera = Camera.main;
        if (mainCamera == null)
        {
            return V("(missing)");
        }

        Vector3 position = mainCamera.transform.position;
        return position.x.ToString("0.00") + ", " + position.y.ToString("0.00");
    }

    private string GetZoomText()
    {
        Camera mainCamera = Camera.main;
        if (mainCamera == null || !mainCamera.orthographic)
        {
            return V("(missing)");
        }

        return mainCamera.orthographicSize.ToString("0.00");
    }
}




