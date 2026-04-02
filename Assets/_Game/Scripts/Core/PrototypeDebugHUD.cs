using System.Collections.Generic;
using UnityEngine;

[DisallowMultipleComponent]
public sealed class PrototypeDebugHUD : MonoBehaviour
{
    private const float Margin = 16f;
    private const float PanelWidthMin = 360f;
    private const float PanelWidthMax = 520f;
    private const float PanelPadding = 12f;
    private const float HeaderHeight = 32f;
    private const float FilterHeight = 38f;
    private const float PanelGap = 10f;
    private const float ChipGap = 6f;
    private const float ChipHeight = 28f;
    private const float LanguageButtonWidth = 54f;
    private const float SearchHeight = 28f;
    private const float SearchLabelWidth = 56f;
    private const float SearchClearButtonWidth = 64f;
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

    private enum BattleHudFlyoutMode
    {
        None,
        ActorCommandMenu,
        PartyCommandMenu,
        SkillListPanel,
        ItemListPanel,
        ConfirmDialog
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
    private GUIStyle _sectionTitleStyle;
    private GUIStyle _chipStyle;
    private GUIStyle _chipActiveStyle;
    private GUIStyle _foldoutStyle;
    private GUIStyle _searchFieldStyle;
    private GUIStyle _actionButtonStyle;
    private Vector2 _scrollPosition;
    private readonly Dictionary<string, Vector2> _overlayScrollByKey = new Dictionary<string, Vector2>();
    private HudFilter _activeFilter = HudFilter.All;
    private string _searchText = string.Empty;
    private Dictionary<string, bool> _expandedByKey;
    private BattleHudFlyoutMode _battleFlyoutMode = BattleHudFlyoutMode.None;
    private string _pendingConfirmActionKey = string.Empty;
    private string _battleHudHoverDetailKey = string.Empty;
    private bool _isSearchFieldFocused;
    private PrototypeBattleUiSurfaceData _battleUiSurface = new PrototypeBattleUiSurfaceData();
    public bool IsSearchFieldFocused => _isSearchFieldFocused;
    public bool ShouldBlockDungeonInput => IsBattleInputModalOpen();

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
        SyncBattleHudState();
        RefreshBattleUiSurface();

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
        if (!overlayMode)
        {
            DrawDockPanel(dockRect, panels);
        }

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
        bool battleHudShellVisible = _bootEntry != null && _bootEntry.IsDungeonBattleViewActive && !_bootEntry.IsDungeonResultPanelVisible;
        if (battleHudShellVisible)
        {
            float barHeight = Mathf.Clamp(Screen.height * 0.18f, 126f, 154f);
            Rect rect = new Rect(0f, Screen.height - barHeight, Screen.width, barHeight);
            Color previousColor = GUI.color;
            GUI.color = new Color(0.02f, 0.04f, 0.06f, 0.94f);
            GUI.DrawTexture(rect, Texture2D.whiteTexture);
            GUI.color = new Color(0.46f, 0.62f, 0.80f, 0.28f);
            GUI.DrawTexture(new Rect(rect.x, rect.y, rect.width, 2f), Texture2D.whiteTexture);
            GUI.color = Color.white;
            Rect contentRect = new Rect(rect.x + PanelPadding, rect.y + 10f, rect.width - (PanelPadding * 2f), rect.height - 20f);
            DrawBattleHudShell(contentRect);
            GUI.color = previousColor;
            return;
        }

        float overlayBarHeight = Mathf.Clamp(Screen.height * 0.36f, DungeonBattleBarMinHeight, DungeonBattleBarMaxHeight);
        float availableWidth = Screen.width;
        float rectX = 0f;
        float rectY = Screen.height - overlayBarHeight;
        Rect rectLegacy = new Rect(rectX, rectY, availableWidth, overlayBarHeight);
        Color previousColorLegacy = GUI.color;
        GUI.color = new Color(0.04f, 0.07f, 0.10f, 0.96f);
        GUI.Box(rectLegacy, GUIContent.none, _panelStyle);

        Rect titleRect = new Rect(rectLegacy.x, rectLegacy.y, rectLegacy.width, HeaderHeight);
        GUI.color = new Color(0.12f, 0.22f, 0.30f, 0.98f);
        GUI.DrawTexture(titleRect, Texture2D.whiteTexture);
        GUI.color = Color.white;
        string overlayTitle = _bootEntry.IsDungeonResultPanelVisible
            ? "Victory Summary"
            : _bootEntry.IsDungeonRouteChoiceVisible
                ? T("PanelDungeonRouteChoice")
                : _bootEntry.IsDungeonEventChoiceVisible
                    ? V(_bootEntry.EventTitleLabel)
                    : T("PanelDungeonPreElite");
        GUI.Label(new Rect(titleRect.x + PanelPadding, titleRect.y + 4f, titleRect.width - (PanelPadding * 2f), titleRect.height), overlayTitle, _titleStyle);
        Rect contentRectLegacy = new Rect(rectLegacy.x + PanelPadding, rectLegacy.y + HeaderHeight + 8f, rectLegacy.width - (PanelPadding * 2f), rectLegacy.height - HeaderHeight - 16f);
        if (_bootEntry.IsDungeonRouteChoiceVisible)
        {
            float leftWidth = (contentRectLegacy.width - (OverlaySectionGap * 2f)) * 0.30f;
            float centerWidth = (contentRectLegacy.width - (OverlaySectionGap * 2f)) * 0.26f;
            float rightWidth = contentRectLegacy.width - leftWidth - centerWidth - (OverlaySectionGap * 2f);
            Rect leftRect = new Rect(contentRectLegacy.x, contentRectLegacy.y, leftWidth, contentRectLegacy.height);
            Rect centerRect = new Rect(leftRect.xMax + OverlaySectionGap, contentRectLegacy.y, centerWidth, contentRectLegacy.height);
            Rect rightRect = new Rect(centerRect.xMax + OverlaySectionGap, contentRectLegacy.y, rightWidth, contentRectLegacy.height);

            DrawScrollableOverlaySection("route_planner_info", leftRect, BuildPanelBody(
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
            DrawScrollableOverlaySection("route_planner_detail", rightRect, BuildPanelBody(
                Line("RouteOption1", V(_bootEntry.RouteOption1Label)),
                Line("RouteOption2", V(_bootEntry.RouteOption2Label)),
                Line("RecommendedRoute", V(_bootEntry.RecommendedRouteRunLabel)),
                Line("CurrentRoute", V(_bootEntry.CurrentRouteLabel)),
                Line("RouteRisk", V(_bootEntry.RouteRiskLabel)),
                Line("SelectedPreview1", V(_bootEntry.SelectedRoutePreview1Label)),
                Line("SelectedPreview2", V(_bootEntry.SelectedRoutePreview2Label)),
                Line("SelectedRecommendedRoute", V(_bootEntry.SelectedRecommendedRouteSummaryLabel)),
                Line("SelectedRecommendedRouteForCity", V(_bootEntry.SelectedRecommendedRouteForLinkedCityLabel)),
                V(_bootEntry.RouteChoiceDescriptionLabel)), new Color(0.08f, 0.12f, 0.16f, 0.94f));
        }
        else if (_bootEntry.IsDungeonEventChoiceVisible)
        {
            float leftWidth = (contentRectLegacy.width - OverlaySectionGap) * 0.46f;
            Rect leftRect = new Rect(contentRectLegacy.x, contentRectLegacy.y, leftWidth, contentRectLegacy.height);
            Rect rightRect = new Rect(leftRect.xMax + OverlaySectionGap, contentRectLegacy.y, contentRectLegacy.width - leftWidth - OverlaySectionGap, contentRectLegacy.height);
            DrawNamedScrollableOverlaySection("event_choice_info", leftRect, "Event", BuildPanelBody(
                Line("EventPrompt", V(_bootEntry.EventPromptLabel)),
                Line("EventStatus", V(_bootEntry.EventStatusLabel)),
                Line("EventChoice", V(_bootEntry.EventChoiceLabel)),
                Line("OptionA", V(_bootEntry.EventOptionALabel)),
                Line("OptionB", V(_bootEntry.EventOptionBLabel))), new Color(0.08f, 0.12f, 0.16f, 0.94f));
            DrawEventChoiceSection(rightRect);
        }
        else if (_bootEntry.IsDungeonPreEliteChoiceVisible)
        {
            float leftWidth = (contentRectLegacy.width - OverlaySectionGap) * 0.46f;
            Rect leftRect = new Rect(contentRectLegacy.x, contentRectLegacy.y, leftWidth, contentRectLegacy.height);
            Rect rightRect = new Rect(leftRect.xMax + OverlaySectionGap, contentRectLegacy.y, contentRectLegacy.width - leftWidth - OverlaySectionGap, contentRectLegacy.height);
            DrawNamedScrollableOverlaySection("pre_elite_info", leftRect, "Elite Decision", BuildPanelBody(
                Line("EliteEncounter", V(_bootEntry.EliteEncounterNameLabel)),
                Line("EliteType", V(_bootEntry.EliteTypeLabel)),
                Line("EliteHp", V(_bootEntry.EliteHpLabel)),
                Line("EliteDefeated", V(_bootEntry.EliteDefeatedLabel)),
                Line("EliteRewardStatus", V(_bootEntry.EliteRewardStatusLabel)),
                Line("EliteRewardHint", V(_bootEntry.EliteRewardHintLabel)),
                Line("PreElitePrompt", V(_bootEntry.PreElitePromptLabel)),
                Line("PreEliteChoice", V(_bootEntry.PreEliteChoiceRunLabel)),
                Line("PartyCondition", V(_bootEntry.PartyConditionLabel)),
                Line("TotalPartyHp", V(_bootEntry.TotalPartyHpLabel))), new Color(0.11f, 0.10f, 0.17f, 0.94f));
            DrawPreEliteChoiceSection(rightRect);
        }
        else if (_bootEntry.IsDungeonResultPanelVisible)
        {
            float leftWidth = (contentRectLegacy.width - (OverlaySectionGap * 2f)) * 0.30f;
            float centerWidth = (contentRectLegacy.width - (OverlaySectionGap * 2f)) * 0.34f;
            float rightWidth = contentRectLegacy.width - leftWidth - centerWidth - (OverlaySectionGap * 2f);
            Rect leftRect = new Rect(contentRectLegacy.x, contentRectLegacy.y, leftWidth, contentRectLegacy.height);
            Rect centerRect = new Rect(leftRect.xMax + OverlaySectionGap, contentRectLegacy.y, centerWidth, contentRectLegacy.height);
            Rect rightRect = new Rect(centerRect.xMax + OverlaySectionGap, contentRectLegacy.y, rightWidth, contentRectLegacy.height);

            DrawNamedScrollableOverlaySection("victory_party_panel", leftRect, "Party Outcome", BuildPanelBody(
                Line("PartyHpSummary", V(_bootEntry.ResultPanelPartyHpSummaryLabel)),
                Line("PartyConditionAtEnd", V(_bootEntry.ResultPanelPartyConditionLabel)),
                Line("SurvivingMembers", V(_bootEntry.ResultPanelSurvivingMembersLabel)),
                Line("CurrentRoute", V(_bootEntry.CurrentRouteLabel)),
                Line("CurrentDungeon", V(_bootEntry.CurrentDungeonRunLabel))), new Color(0.08f, 0.12f, 0.16f, 0.94f));
            DrawNamedScrollableOverlaySection("victory_reward_panel", centerRect, "Victory Summary", BuildPanelBody(
                Line("EliteRewardIdentity", V(_bootEntry.ResultPanelEliteRewardIdentityLabel)),
                Line("EliteRewardAmount", V(_bootEntry.ResultPanelEliteRewardAmountLabel)),
                Line("EliteBonusRewardAmount", V(_bootEntry.ResultPanelEliteBonusRewardAmountLabel)),
                Line("RoomPathSummary", V(_bootEntry.ResultPanelRoomPathSummaryLabel)),
                Line("PartyHpSummary", V(_bootEntry.ResultPanelPartyHpSummaryLabel)),
                Line("PartyConditionAtEnd", V(_bootEntry.ResultPanelPartyConditionLabel)),
                Line("SurvivingMembers", V(_bootEntry.ResultPanelSurvivingMembersLabel)),
                Line("ClearedEncounters", V(_bootEntry.ResultPanelClearedEncountersLabel)),
                Line("OpenedChests", V(_bootEntry.ResultPanelOpenedChestsLabel)),
                Line("ReturnToWorldPrompt", V(_bootEntry.ResultPanelReturnPromptLabel))), new Color(0.08f, 0.12f, 0.16f, 0.94f));
            DrawNamedScrollableOverlaySection("victory_support_panel", rightRect, "Battle Result Feed", BuildPanelBody(
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

        GUI.color = previousColorLegacy;
    }

    private void DrawBattleHudShell(Rect rect)
    {
        float screenLeft = Margin;
        float screenWidth = Screen.width - (Margin * 2f);
        float topHeight = Mathf.Clamp(Screen.height * 0.105f, 96f, 112f);
        Rect topRect = new Rect(screenLeft, Margin, screenWidth, topHeight);

        float commandZoneHeight = Mathf.Clamp(Screen.height * 0.30f, 250f, 320f);
        Rect commandZoneRect = new Rect(screenLeft, Screen.height - Margin - commandZoneHeight, screenWidth, commandZoneHeight);
        float messageHeight = Mathf.Clamp(commandZoneHeight * 0.17f, 42f, 52f);
        float commandBarHeight = Mathf.Clamp(commandZoneHeight * 0.24f, 64f, 78f);
        Rect commandBarRect = new Rect(screenLeft, commandZoneRect.yMax - commandBarHeight, screenWidth, commandBarHeight);
        Rect messageRect = new Rect(screenLeft, commandBarRect.y - 8f - messageHeight, screenWidth, messageHeight);

        float flyoutWidth = Mathf.Clamp(screenWidth * 0.46f, 680f, 860f);
        float flyoutHeight = Mathf.Clamp(commandZoneRect.height - commandBarHeight - messageHeight - 34f, 176f, 228f);
        Rect flyoutRect = new Rect(screenLeft + ((screenWidth - flyoutWidth) * 0.5f), commandZoneRect.y + 10f, flyoutWidth, flyoutHeight);

        float sideWidth = Mathf.Clamp(screenWidth * 0.22f, 276f, 340f);
        float sideY = topRect.yMax + Mathf.Clamp(Screen.height * 0.038f, 36f, 52f);
        float sideHeight = Mathf.Clamp(commandZoneRect.y - sideY - 24f, 300f, 430f);
        Rect leftRect = new Rect(screenLeft + 8f, sideY, sideWidth, sideHeight);
        Rect rightRect = new Rect(screenLeft + screenWidth - sideWidth - 8f, sideY, sideWidth, sideHeight);

        DrawBattleTopStrip(topRect);
        DrawPartyStatusPanel(leftRect);
        DrawEnemyInfoPanel(rightRect);

        DrawOverlaySectionBackground(commandZoneRect, new Color(0.04f, 0.07f, 0.10f, 0.82f));
        Rect commandBackdropRect = new Rect(commandZoneRect.x + 12f, commandZoneRect.y + 8f, commandZoneRect.width - 24f, flyoutRect.height + 14f);
        DrawOverlaySectionBackground(commandBackdropRect, new Color(0.06f, 0.09f, 0.13f, 0.72f));

        DrawCommandFlyoutPanel(flyoutRect);
        DrawBattleMessageBox(messageRect);
        DrawBottomCommandBar(commandBarRect);

        if (IsTargetSelectionActive())
        {
            DrawTargetSelectionOverlay(topRect, commandZoneRect);
        }
    }

    private void DrawBattleTopStrip(Rect rect)
    {
        DrawOverlaySectionBackground(rect, new Color(0.05f, 0.09f, 0.14f, 0.84f));
        float summaryWidth = Mathf.Clamp(rect.width * 0.31f, 370f, 460f);
        float actorWidth = Mathf.Clamp(rect.width * 0.16f, 176f, 214f);
        Rect summaryRect = new Rect(rect.x, rect.y, summaryWidth, rect.height);
        Rect actorRect = new Rect(summaryRect.xMax + 8f, rect.y, actorWidth, rect.height);
        Rect timelineRect = new Rect(actorRect.xMax + 8f, rect.y, rect.width - summaryWidth - actorWidth - 16f, rect.height);

        DrawOverlaySectionBackground(summaryRect, new Color(0.09f, 0.14f, 0.20f, 0.90f));
        PrototypeBattleUiSurfaceData surface = GetBattleUiSurfaceData();
        string dungeonLine = HasMeaningfulText(surface.CurrentDungeonName) || HasMeaningfulText(surface.CurrentRouteLabel)
            ? GetCompactHudText(surface.CurrentDungeonName + " | " + surface.CurrentRouteLabel, 64, false)
            : "Dungeon pending";
        string stateLine = HasMeaningfulText(surface.TotalPartyHp) || HasMeaningfulText(surface.PartyCondition)
            ? GetCompactHudText(surface.TotalPartyHp + " | " + surface.PartyCondition, 56, false)
            : "Party state pending";
        Rect titleRect = new Rect(summaryRect.x + 14f, summaryRect.y + 10f, summaryRect.width - 28f, 22f);
        Rect subRect = new Rect(summaryRect.x + 14f, titleRect.yMax + 5f, summaryRect.width - 28f, 18f);
        Rect stateRect = new Rect(summaryRect.x + 14f, subRect.yMax + 4f, summaryRect.width - 28f, 18f);
        DrawFittedLabel(titleRect, GetBattleHudHeaderTitle(), _titleStyle, 15, 11, false);
        DrawFittedLabel(subRect, dungeonLine, _bodyStyle, 11, 9, false);
        DrawFittedLabel(stateRect, stateLine, _bodyStyle, 11, 9, false);

        DrawOverlaySectionBackground(actorRect, new Color(0.11f, 0.17f, 0.24f, 0.90f));
        Rect actorTitleRect = new Rect(actorRect.x + 12f, actorRect.y + 10f, actorRect.width - 24f, 16f);
        Rect actorNameRect = new Rect(actorRect.x + 12f, actorTitleRect.yMax + 5f, actorRect.width - 24f, 20f);
        Rect actorRoleRect = new Rect(actorRect.x + 12f, actorNameRect.yMax + 4f, actorRect.width - 24f, 18f);
        DrawFittedLabel(actorTitleRect, "Current", _sectionTitleStyle, 11, 9, false);
        DrawFittedLabel(actorNameRect, GetBattleActorShortLabel(), _sectionTitleStyle, 12, 10, false);
        DrawFittedLabel(actorRoleRect, GetCompactHudText(GetCurrentActorRoleLabel(), 22, false), _bodyStyle, 11, 9, false);

        DrawTurnOrderTimeline(timelineRect);
    }

    private void DrawTurnOrderTimeline(Rect rect)
    {
        DrawOverlaySectionBackground(rect, new Color(0.07f, 0.11f, 0.16f, 0.99f));
        Rect innerRect = new Rect(rect.x + SectionInnerPadding, rect.y + 7f, rect.width - (SectionInnerPadding * 2f), rect.height - 14f);
        PrototypeBattleUiTimelineData timeline = GetBattleUiSurfaceData().Timeline;
        float phaseWidth = Mathf.Clamp(innerRect.width * 0.15f, 126f, 156f);
        Rect phaseRect = new Rect(innerRect.x, innerRect.y, phaseWidth, innerRect.height);
        Rect queueRect = new Rect(phaseRect.xMax + 10f, innerRect.y, innerRect.width - phaseWidth - 10f, innerRect.height);

        DrawTimelinePhaseToken(phaseRect, timeline);
        DrawTimelineQueue(queueRect, timeline);
    }

    private void DrawTimelinePhaseToken(Rect rect, PrototypeBattleUiTimelineData timeline)
    {
        DrawOverlaySectionBackground(rect, new Color(0.13f, 0.21f, 0.30f, 0.98f));
        Rect titleRect = new Rect(rect.x + 8f, rect.y + 6f, rect.width - 16f, 14f);
        Rect phaseRect = new Rect(rect.x + 8f, titleRect.yMax + 4f, rect.width - 16f, 18f);
        Rect footerRect = new Rect(rect.x + 8f, phaseRect.yMax + 4f, rect.width - 16f, rect.height - 32f);
        string phaseLabel = timeline != null && HasMeaningfulText(timeline.PhaseLabel)
            ? GetCompactHudText(timeline.PhaseLabel, 24, false)
            : "Battle";
        string nextStep = timeline != null && HasMeaningfulText(timeline.NextStepLabel)
            ? GetCompactHudText(timeline.NextStepLabel, 32, false)
            : "Awaiting turn";
        DrawFittedLabel(titleRect, "Turn", _sectionTitleStyle, 10, 9, false);
        DrawFittedLabel(phaseRect, phaseLabel, _sectionTitleStyle, 12, 10, false);
        DrawFittedLabel(footerRect, nextStep, _bodyStyle, 10, 8, true);
    }

    private void DrawTimelineQueue(Rect rect, PrototypeBattleUiTimelineData timeline)
    {
        PrototypeBattleUiTimelineSlotData[] slots = timeline != null ? timeline.Slots : null;
        if (slots == null || slots.Length == 0)
        {
            DrawFittedLabel(rect, "Turn queue pending.", _bodyStyle, 10, 9, false);
            return;
        }

        int visibleCount = rect.width < 860f ? Mathf.Min(4, slots.Length) : Mathf.Min(5, slots.Length);
        float tokenGap = rect.width < 980f ? 8f : 10f;
        float tokenWidth = (rect.width - (tokenGap * Mathf.Max(0, visibleCount - 1))) / Mathf.Max(1, visibleCount);
        tokenWidth = Mathf.Max(102f, tokenWidth);
        float totalWidth = (tokenWidth * visibleCount) + (tokenGap * Mathf.Max(0, visibleCount - 1));
        float startX = rect.x + Mathf.Max(0f, (rect.width - totalWidth) * 0.5f);
        for (int index = 0; index < visibleCount; index++)
        {
            Rect tokenRect = new Rect(startX + ((tokenWidth + tokenGap) * index), rect.y, tokenWidth, rect.height);
            DrawTimelineSlotToken(tokenRect, slots[index], index);
        }
    }

    private void DrawTimelineSlotToken(Rect rect, PrototypeBattleUiTimelineSlotData slotData, int queueIndex)
    {
        bool isCurrent = slotData != null && slotData.IsCurrent;
        bool isPending = slotData != null && slotData.IsPending;
        bool isEnemy = slotData != null && slotData.IsEnemy;
        string roleLabel = slotData != null ? slotData.SecondaryLabel : string.Empty;
        Color accentColor = GetBattleRoleAccentColor(roleLabel, isEnemy);
        Color backgroundColor = isCurrent
            ? new Color(0.15f, 0.24f, 0.34f, 0.98f)
            : isPending
                ? new Color(0.23f, 0.18f, 0.10f, 0.98f)
                : isEnemy
                    ? new Color(0.22f, 0.12f, 0.12f, 0.98f)
                    : new Color(0.10f, 0.15f, 0.22f, 0.98f);
        DrawOverlaySectionBackground(rect, backgroundColor);
        Color previousColor = GUI.color;
        GUI.color = accentColor;
        GUI.DrawTexture(new Rect(rect.x, rect.y, rect.width, 3f), Texture2D.whiteTexture);
        GUI.color = Color.white;

        string statusLabel = slotData != null && HasMeaningfulText(slotData.StatusLabel)
            ? GetCompactHudText(slotData.StatusLabel.ToUpperInvariant(), 12, false)
            : isCurrent
                ? "CURRENT"
                : isPending
                    ? "READY"
                    : queueIndex == 1
                        ? "NEXT"
                        : "QUEUE";
        string displayName = slotData != null && HasMeaningfulText(slotData.Label)
            ? GetCompactHudText(slotData.Label, 18, false)
            : "Unknown";
        string secondaryLabel = slotData != null && HasMeaningfulText(slotData.SecondaryLabel)
            ? GetCompactHudText(slotData.SecondaryLabel, 18, false)
            : isEnemy
                ? "Enemy"
                : "Ally";

        Rect statusRect = new Rect(rect.x + 10f, rect.y + 7f, rect.width - 20f, 14f);
        Rect nameRect = new Rect(rect.x + 10f, statusRect.yMax + 4f, rect.width - 20f, 18f);
        Rect secondaryRect = new Rect(rect.x + 10f, nameRect.yMax + 4f, rect.width - 20f, rect.height - 39f);
        DrawFittedLabel(statusRect, statusLabel, _bodyStyle, 9, 8, false);
        DrawFittedLabel(nameRect, displayName, _sectionTitleStyle, 11, 9, false);
        DrawFittedLabel(secondaryRect, secondaryLabel, _bodyStyle, 10, 8, false);
        GUI.color = previousColor;
    }

    private void DrawPartyStatusPanel(Rect rect)
    {
        DrawOverlaySectionBackground(rect, new Color(0.08f, 0.11f, 0.16f, 0.98f));
        Rect titleRect = new Rect(rect.x + SectionInnerPadding, rect.y + 8f, rect.width - (SectionInnerPadding * 2f), 20f);
        DrawFittedLabel(titleRect, "Party Status", _sectionTitleStyle, 11, 10, false);

        Rect contentRect = new Rect(rect.x + SectionInnerPadding, titleRect.yMax + 8f, rect.width - (SectionInnerPadding * 2f), rect.height - (titleRect.yMax - rect.y) - 12f);
        PrototypeBattleUiPartyMemberData[] members = GetBattleUiSurfaceData().PartyMembers;
        if (members == null || members.Length == 0)
        {
            DrawFittedLabel(contentRect, "Party data pending.", _bodyStyle, 10, 9, false);
            return;
        }

        int visibleCount = Mathf.Min(4, members.Length);
        float cardGap = 10f;
        float cardHeight = (contentRect.height - (cardGap * Mathf.Max(0, visibleCount - 1))) / Mathf.Max(1, visibleCount);
        for (int index = 0; index < visibleCount; index++)
        {
            Rect cardRect = new Rect(contentRect.x, contentRect.y + ((cardHeight + cardGap) * index), contentRect.width, cardHeight);
            DrawPartyMemberStatusCard(cardRect, members[index], index + 1);
        }
    }

    private void DrawPartyMemberStatusCard(Rect rect, PrototypeBattleUiPartyMemberData memberData, int slotIndex)
    {
        string memberName = memberData != null && HasMeaningfulText(memberData.DisplayName)
            ? memberData.DisplayName
            : "Member " + slotIndex;
        string role = memberData != null && HasMeaningfulText(memberData.RoleLabel)
            ? memberData.RoleLabel
            : "Adventurer";
        bool active = memberData != null && memberData.IsActive;
        bool targeted = memberData != null && memberData.IsTargeted;
        bool knockedOut = memberData == null || memberData.IsKnockedOut;
        string statusText = memberData != null && HasMeaningfulText(memberData.StatusText)
            ? memberData.StatusText
            : BuildPartyCardStatusText(memberName, active, targeted, knockedOut);

        Color accentColor = GetBattleRoleAccentColor(role, false);
        Color backgroundColor = knockedOut
            ? new Color(0.18f, 0.08f, 0.08f, 0.98f)
            : active
                ? new Color(0.12f, 0.20f, 0.29f, 0.98f)
                : targeted
                    ? new Color(0.17f, 0.14f, 0.10f, 0.98f)
                    : new Color(0.09f, 0.12f, 0.18f, 0.98f);
        DrawOverlaySectionBackground(rect, backgroundColor);

        Color previousColor = GUI.color;
        GUI.color = accentColor;
        GUI.DrawTexture(new Rect(rect.x, rect.y, 5f, rect.height), Texture2D.whiteTexture);
        GUI.color = Color.white;

        Rect avatarRect = new Rect(rect.x + 10f, rect.y + 10f, 28f, 28f);
        DrawOverlaySectionBackground(avatarRect, new Color(accentColor.r, accentColor.g, accentColor.b, 0.28f));
        string avatarText = !string.IsNullOrEmpty(memberName) ? memberName.Substring(0, 1).ToUpperInvariant() : slotIndex.ToString();
        DrawFittedLabel(new Rect(avatarRect.x + 3f, avatarRect.y + 4f, avatarRect.width - 6f, avatarRect.height - 8f), avatarText, _sectionTitleStyle, 11, 9, false);

        Rect statusRect = new Rect(rect.xMax - 96f, rect.y + 10f, 86f, 20f);
        Rect nameRect = new Rect(avatarRect.xMax + 10f, rect.y + 10f, statusRect.x - avatarRect.xMax - 18f, 18f);
        Rect roleRect = new Rect(nameRect.x, nameRect.yMax + 3f, nameRect.width, 16f);
        Rect hpRect = new Rect(rect.x + 10f, rect.yMax - 24f, rect.width - 20f, 16f);

        DrawOverlaySectionBackground(statusRect, new Color(0.11f, 0.14f, 0.19f, 0.96f));
        DrawFittedLabel(nameRect, GetCompactHudText(memberName, 20, false), _sectionTitleStyle, 12, 10, false);
        DrawFittedLabel(roleRect, GetCompactHudText(role, 18, false), _bodyStyle, 10, 9, false);
        DrawFittedLabel(new Rect(statusRect.x + 6f, statusRect.y + 3f, statusRect.width - 12f, statusRect.height - 6f), GetCompactHudText(statusText, 16, false), _bodyStyle, 10, 8, false);

        float hpCurrent = memberData != null ? memberData.CurrentHp : 0f;
        float hpMax = memberData != null ? Mathf.Max(1f, memberData.MaxHp) : 1f;
        DrawStatusBar(hpRect, hpCurrent, hpMax, new Color(0.26f, 0.68f, 0.36f, 0.98f), "HP " + Mathf.RoundToInt(hpCurrent) + " / " + Mathf.RoundToInt(hpMax));
        GUI.color = previousColor;
    }

    private void DrawEnemyInfoPanel(Rect rect)
    {
        Color panelColor = IsTargetSelectionActive()
            ? new Color(0.15f, 0.12f, 0.09f, 0.98f)
            : new Color(0.09f, 0.10f, 0.15f, 0.98f);
        DrawOverlaySectionBackground(rect, panelColor);

        Rect titleRect = new Rect(rect.x + SectionInnerPadding, rect.y + 8f, rect.width * 0.44f, 20f);
        Rect summaryRect = new Rect(titleRect.xMax + 8f, rect.y + 8f, rect.width - (titleRect.width + (SectionInnerPadding * 2f) + 8f), 20f);
        DrawFittedLabel(titleRect, IsTargetSelectionActive() ? "Target Focus" : "Enemy Focus", _sectionTitleStyle, 11, 10, false);
        DrawFittedLabel(summaryRect, GetCompactHudText(GetBattleUiSurfaceData().EncounterName, 38, false), _bodyStyle, 10, 9, false);

        Rect contentRect = new Rect(rect.x + SectionInnerPadding, titleRect.yMax + 8f, rect.width - (SectionInnerPadding * 2f), rect.height - (titleRect.yMax - rect.y) - 12f);
        float selectedHeight = Mathf.Clamp(contentRect.height * 0.62f, 132f, 176f);
        Rect selectedRect = new Rect(contentRect.x, contentRect.y, contentRect.width, selectedHeight);
        Rect rosterRect = new Rect(contentRect.x, selectedRect.yMax + 10f, contentRect.width, contentRect.height - selectedHeight - 10f);

        DrawSelectedEnemyCard(selectedRect);
        DrawEnemyRosterStrip(rosterRect);
    }

    private void DrawSelectedEnemyCard(Rect rect)
    {
        PrototypeBattleUiEnemyData enemyData = GetBattleUiSurfaceData().SelectedEnemy;
        string enemyName = enemyData != null && HasMeaningfulText(enemyData.DisplayName) ? enemyData.DisplayName : "No target selected";
        string typeLabel = enemyData != null && HasMeaningfulText(enemyData.TypeLabel) ? enemyData.TypeLabel : "Monster";
        string roleLabel = enemyData != null && HasMeaningfulText(enemyData.RoleLabel) ? enemyData.RoleLabel : "Frontline";
        string stateLabel = enemyData != null && HasMeaningfulText(enemyData.StateLabel) ? enemyData.StateLabel : "Unknown";
        string intentLabel = enemyData != null && HasMeaningfulText(enemyData.IntentLabel) ? enemyData.IntentLabel : "Unknown";
        string traitLabel = enemyData != null && HasMeaningfulText(enemyData.TraitText) ? enemyData.TraitText : "Traits pending";
        Color accentColor = GetBattleRoleAccentColor(roleLabel, true);

        DrawOverlaySectionBackground(rect, new Color(0.10f, 0.12f, 0.17f, 0.98f));
        Color previousColor = GUI.color;
        GUI.color = accentColor;
        GUI.DrawTexture(new Rect(rect.x, rect.y, rect.width, 4f), Texture2D.whiteTexture);
        GUI.color = Color.white;

        Rect nameRect = new Rect(rect.x + 10f, rect.y + 10f, rect.width - 106f, 20f);
        Rect eliteRect = new Rect(rect.xMax - 88f, rect.y + 10f, 78f, 22f);
        DrawFittedLabel(nameRect, GetCompactHudText(enemyName, 30, false), _sectionTitleStyle, 12, 10, false);
        if (enemyData != null && enemyData.IsElite)
        {
            DrawOverlaySectionBackground(eliteRect, new Color(0.34f, 0.22f, 0.10f, 0.96f));
            DrawFittedLabel(new Rect(eliteRect.x + 6f, eliteRect.y + 3f, eliteRect.width - 12f, eliteRect.height - 6f), "Elite", _bodyStyle, 10, 8, false);
        }

        float chipGap = 6f;
        float chipWidth = (rect.width - 20f - (chipGap * 2f)) / 3f;
        Rect typeRect = new Rect(rect.x + 10f, nameRect.yMax + 8f, chipWidth, 38f);
        Rect roleRect = new Rect(typeRect.xMax + chipGap, typeRect.y, chipWidth, 38f);
        Rect stateRect = new Rect(roleRect.xMax + chipGap, typeRect.y, chipWidth, 38f);
        Rect hpRect = new Rect(rect.x + 10f, typeRect.yMax + 10f, rect.width - 20f, 18f);
        Rect intentRect = new Rect(rect.x + 10f, hpRect.yMax + 8f, rect.width - 20f, 16f);
        Rect traitRect = new Rect(rect.x + 10f, intentRect.yMax + 4f, rect.width - 20f, 16f);

        DrawCompactInfoCard(typeRect, "Type", typeLabel, new Color(0.11f, 0.14f, 0.19f, 0.98f));
        DrawCompactInfoCard(roleRect, "Role", roleLabel, new Color(0.11f, 0.14f, 0.19f, 0.98f));
        DrawCompactInfoCard(stateRect, "State", stateLabel, new Color(0.11f, 0.14f, 0.19f, 0.98f));
        DrawStatusBar(hpRect, enemyData != null ? enemyData.CurrentHp : 0f, enemyData != null ? Mathf.Max(1f, enemyData.MaxHp) : 1f, new Color(0.72f, 0.28f, 0.26f, 0.98f), "HP " + (enemyData != null ? enemyData.CurrentHp + " / " + enemyData.MaxHp : "?"));
        DrawFittedLabel(intentRect, "Intent: " + GetCompactHudText(intentLabel, 44, false), _bodyStyle, 10, 9, false);
        DrawFittedLabel(traitRect, GetCompactHudText(traitLabel, 58, false), _bodyStyle, 10, 8, false);
        GUI.color = previousColor;
    }

    private void DrawEnemyRosterStrip(Rect rect)
    {
        DrawOverlaySectionBackground(rect, new Color(0.10f, 0.12f, 0.17f, 0.98f));
        Rect titleRect = new Rect(rect.x + 8f, rect.y + 6f, rect.width * 0.40f, 18f);
        Rect summaryRect = new Rect(titleRect.xMax + 8f, rect.y + 6f, rect.width - titleRect.width - 24f, 18f);
        PrototypeBattleUiEnemyData[] roster = GetBattleUiSurfaceData().EnemyRoster;
        DrawFittedLabel(titleRect, "Enemy Line", _sectionTitleStyle, 11, 10, false);
        DrawFittedLabel(summaryRect, roster != null ? roster.Length + " active" : "0 active", _bodyStyle, 10, 9, false);
        Rect viewportRect = new Rect(rect.x + 8f, titleRect.yMax + 6f, rect.width - 16f, rect.height - (titleRect.yMax - rect.y) - 12f);
        if (roster == null || roster.Length == 0)
        {
            DrawFittedLabel(viewportRect, "Enemy roster pending.", _bodyStyle, 10, 9, false);
            return;
        }

        float rowHeight = 42f;
        float rowGap = 6f;
        float contentHeight = (rowHeight * roster.Length) + (rowGap * Mathf.Max(0, roster.Length - 1));
        bool showVerticalScrollbar = contentHeight > viewportRect.height;
        float contentWidth = Mathf.Max(32f, viewportRect.width - (showVerticalScrollbar ? 18f : 4f));
        Vector2 scrollPosition = GetOverlayScrollPosition("battle_enemy_roster_strip");
        Vector2 nextScrollPosition = GUI.BeginScrollView(
            viewportRect,
            scrollPosition,
            new Rect(0f, 0f, contentWidth, contentHeight),
            false,
            showVerticalScrollbar);
        for (int index = 0; index < roster.Length; index++)
        {
            Rect rowRect = new Rect(0f, (rowHeight + rowGap) * index, contentWidth, rowHeight);
            DrawEnemyRosterRow(rowRect, roster[index], index + 1);
        }
        GUI.EndScrollView();
        _overlayScrollByKey["battle_enemy_roster_strip"] = nextScrollPosition;
    }

    private void DrawEnemyRosterRow(Rect rect, PrototypeBattleUiEnemyData enemyData, int displayIndex)
    {
        bool defeated = enemyData != null && enemyData.IsDefeated;
        bool selected = enemyData != null && enemyData.IsSelected;
        bool hovered = enemyData != null && enemyData.IsHovered;
        string roleLabel = enemyData != null ? enemyData.RoleLabel : string.Empty;
        Color accentColor = GetBattleRoleAccentColor(roleLabel, true);
        Color rowColor = defeated
            ? new Color(0.15f, 0.08f, 0.08f, 0.98f)
            : selected
                ? new Color(0.20f, 0.15f, 0.09f, 0.98f)
                : hovered
                    ? new Color(0.17f, 0.18f, 0.11f, 0.98f)
                    : new Color(0.09f, 0.11f, 0.15f, 0.98f);
        DrawOverlaySectionBackground(rect, rowColor);

        Color previousColor = GUI.color;
        GUI.color = accentColor;
        GUI.DrawTexture(new Rect(rect.x, rect.y, 4f, rect.height), Texture2D.whiteTexture);
        GUI.color = Color.white;

        Rect indexRect = new Rect(rect.x + 8f, rect.y + 8f, 20f, 20f);
        Rect nameRect = new Rect(indexRect.xMax + 8f, rect.y + 6f, rect.width * 0.48f, 16f);
        Rect stateRect = new Rect(rect.xMax - 94f, rect.y + 6f, 84f, 16f);
        Rect hpRect = new Rect(indexRect.xMax + 8f, nameRect.yMax + 6f, rect.width - 44f, 14f);

        string displayName = enemyData != null && HasMeaningfulText(enemyData.DisplayName) ? enemyData.DisplayName : "Enemy " + displayIndex;
        string stateText = enemyData != null && HasMeaningfulText(enemyData.StateLabel) ? enemyData.StateLabel : "Alive";
        if (enemyData != null && enemyData.IsElite)
        {
            stateText = "Elite / " + stateText;
        }

        DrawOverlaySectionBackground(indexRect, new Color(0.12f, 0.15f, 0.20f, 0.96f));
        DrawFittedLabel(new Rect(indexRect.x + 4f, indexRect.y + 2f, indexRect.width - 8f, indexRect.height - 4f), displayIndex.ToString(), _bodyStyle, 10, 8, false);
        DrawFittedLabel(nameRect, GetCompactHudText(displayName, 22, false), _bodyStyle, 10, 9, false);
        DrawFittedLabel(stateRect, GetCompactHudText(stateText, 18, false), _bodyStyle, 10, 8, false);
        DrawStatusBar(hpRect, enemyData != null ? enemyData.CurrentHp : 0f, enemyData != null ? Mathf.Max(1f, enemyData.MaxHp) : 1f, new Color(0.72f, 0.28f, 0.26f, 0.98f), "HP " + (enemyData != null ? enemyData.CurrentHp + " / " + enemyData.MaxHp : "?"));
        GUI.color = previousColor;
    }

    private void DrawCommandFlyoutPanel(Rect rect)
    {
        _battleHudHoverDetailKey = string.Empty;
        BattleHudFlyoutMode mode = GetDisplayedBattleFlyoutMode();
        DrawOverlaySectionBackground(rect, new Color(0.07f, 0.10f, 0.15f, 0.98f));

        Rect headerRect = new Rect(rect.x + SectionInnerPadding, rect.y + 6f, rect.width - (SectionInnerPadding * 2f), 18f);
        Rect actorRect = new Rect(headerRect.x, headerRect.y, headerRect.width * 0.50f, headerRect.height);
        Rect modeRect = new Rect(actorRect.xMax + 8f, headerRect.y, headerRect.xMax - actorRect.xMax - 8f, headerRect.height);
        GUI.Label(actorRect, "Command Selection", _sectionTitleStyle);
        GUI.Label(modeRect, GetBattleActorShortLabel() + " | " + GetBattleModeLabel(), _bodyStyle);

        Rect bodyRect = new Rect(rect.x + SectionInnerPadding, headerRect.yMax + 6f, rect.width - (SectionInnerPadding * 2f), rect.height - (headerRect.yMax - rect.y) - SectionInnerPadding);
        float menuWidth = Mathf.Clamp(bodyRect.width * 0.38f, 150f, 220f);
        Rect menuRect = new Rect(bodyRect.x, bodyRect.y, menuWidth, bodyRect.height);
        Rect detailRect = new Rect(menuRect.xMax + OverlaySectionGap, bodyRect.y, bodyRect.width - menuWidth - OverlaySectionGap, bodyRect.height);

        DrawCommandMenuPanel(menuRect, mode);
        DrawCommandDetailPanel(detailRect, mode);

        Color previousColor = GUI.color;
        GUI.color = new Color(0.26f, 0.35f, 0.46f, 0.28f);
        GUI.DrawTexture(new Rect(menuRect.xMax + (OverlaySectionGap * 0.5f), bodyRect.y + 6f, 1f, bodyRect.height - 12f), Texture2D.whiteTexture);
        GUI.color = previousColor;
    }

    private void DrawCommandMenuPanel(Rect rect, BattleHudFlyoutMode mode)
    {
        DrawOverlaySectionBackground(rect, new Color(0.11f, 0.14f, 0.18f, 0.98f));
        Rect titleRect = new Rect(rect.x + 8f, rect.y + 6f, rect.width - 16f, 18f);
        GUI.Label(titleRect, GetCommandMenuTitle(mode), _sectionTitleStyle);
        Rect contentRect = new Rect(rect.x + 8f, titleRect.yMax + 6f, rect.width - 16f, rect.height - (titleRect.yMax - rect.y) - 12f);

        switch (mode)
        {
            case BattleHudFlyoutMode.SkillListPanel:
                DrawSkillListPanel(contentRect);
                break;
            case BattleHudFlyoutMode.ItemListPanel:
                DrawItemListPanel(contentRect);
                break;
            case BattleHudFlyoutMode.PartyCommandMenu:
                DrawPartyCommandMenu(contentRect);
                break;
            case BattleHudFlyoutMode.ConfirmDialog:
                DrawConfirmDialog(contentRect);
                break;
            default:
                DrawActorCommandMenu(contentRect);
                break;
        }
    }

    private void DrawCommandDetailPanel(Rect rect, BattleHudFlyoutMode mode)
    {
        DrawOverlaySectionBackground(rect, new Color(0.09f, 0.12f, 0.17f, 0.90f));
        PrototypeBattleUiCommandDetailData detail = GetBattleUiCommandDetail(mode);
        string detailTitle = detail != null && HasMeaningfulText(detail.Label) ? detail.Label : "Command";
        Rect titleRect = new Rect(rect.x + 8f, rect.y + 6f, rect.width - 16f, 14f);
        GUI.Label(titleRect, detailTitle, _sectionTitleStyle);

        Rect viewportRect = new Rect(rect.x + 8f, titleRect.yMax + 6f, rect.width - 16f, rect.height - (titleRect.yMax - rect.y) - 12f);
        float descriptionHeight = 48f;
        float metaHeight = 34f;
        float gap = 6f;
        float noteHeight = detail != null && !detail.IsAvailable ? 24f : 0f;
        float contentHeight = descriptionHeight + gap + metaHeight + gap + metaHeight + (noteHeight > 0f ? gap + noteHeight : 0f);
        bool showVerticalScrollbar = contentHeight > viewportRect.height;
        float contentWidth = Mathf.Max(32f, viewportRect.width - (showVerticalScrollbar ? 18f : 4f));
        Vector2 scrollPosition = GetOverlayScrollPosition("battle_command_detail");
        Vector2 nextScrollPosition = GUI.BeginScrollView(viewportRect, scrollPosition, new Rect(0f, 0f, contentWidth, contentHeight), false, showVerticalScrollbar);

        Rect descriptionRect = new Rect(0f, 0f, contentWidth, descriptionHeight);
        DrawOverlaySectionBackground(descriptionRect, new Color(0.11f, 0.15f, 0.21f, 0.88f));
        GUI.Label(new Rect(descriptionRect.x + 8f, descriptionRect.y + 6f, descriptionRect.width - 16f, descriptionRect.height - 12f), detail != null && HasMeaningfulText(detail.Description) ? detail.Description : "Choose a command to inspect its target, cost, and effect.", _bodyStyle);

        float metaGap = 6f;
        float metaWidth = (contentWidth - metaGap) * 0.5f;
        Rect targetRect = new Rect(0f, descriptionRect.yMax + gap, metaWidth, metaHeight);
        Rect costRect = new Rect(targetRect.xMax + metaGap, targetRect.y, contentWidth - metaWidth - metaGap, metaHeight);
        Rect effectRect = new Rect(0f, targetRect.yMax + gap, contentWidth, metaHeight);
        DrawCompactInfoCard(targetRect, "Target", detail != null ? detail.TargetText : string.Empty, new Color(0.11f, 0.15f, 0.21f, 0.88f));
        DrawCompactInfoCard(costRect, "Cost", detail != null ? detail.CostText : string.Empty, new Color(0.11f, 0.15f, 0.21f, 0.88f));
        DrawCompactInfoCard(effectRect, "Effect", detail != null ? detail.EffectText : string.Empty, new Color(0.11f, 0.15f, 0.21f, 0.88f));
        if (detail != null && !detail.IsAvailable)
        {
            Rect noteRect = new Rect(0f, effectRect.yMax + gap, contentWidth, noteHeight);
            DrawOverlaySectionBackground(noteRect, new Color(0.22f, 0.18f, 0.10f, 0.88f));
            GUI.Label(new Rect(noteRect.x + 8f, noteRect.y + 4f, noteRect.width - 16f, noteRect.height - 8f), "Unavailable in this batch.", _bodyStyle);
        }

        GUI.EndScrollView();
        _overlayScrollByKey["battle_command_detail"] = nextScrollPosition;
    }
    private void DrawCompactInfoCard(Rect rect, string title, string value, Color backgroundColor)
    {
        DrawOverlaySectionBackground(rect, backgroundColor);
        Rect titleRect = new Rect(rect.x + 8f, rect.y + 5f, rect.width - 16f, 12f);
        Rect valueRect = new Rect(rect.x + 8f, titleRect.yMax + 4f, rect.width - 16f, rect.height - 20f);
        int maxLength = rect.width < 132f ? 16 : 24;
        DrawFittedLabel(titleRect, title, _sectionTitleStyle, 9, 8, false);
        DrawFittedLabel(valueRect, HasMeaningfulText(value) ? GetCompactHudText(value, maxLength, false) : "None", _bodyStyle, 10, 8, true);
    }

    private string GetCommandMenuTitle(BattleHudFlyoutMode mode)
    {
        return mode == BattleHudFlyoutMode.SkillListPanel
            ? "Skill"
            : mode == BattleHudFlyoutMode.ItemListPanel
                ? "Item"
                : mode == BattleHudFlyoutMode.PartyCommandMenu
                    ? "Party"
                    : mode == BattleHudFlyoutMode.ConfirmDialog
                        ? "Confirm"
                        : "Action";
    }

    private void DrawActorCommandMenu(Rect rect)
    {
        float menuButtonHeight = 42f;
        float menuButtonGap = 8f;
        float totalHeight = (menuButtonHeight * 3f) + (menuButtonGap * 2f);
        float startY = rect.y + Mathf.Max(0f, (rect.height - totalHeight) * 0.5f);
        Rect attackRect = new Rect(rect.x, startY, rect.width, menuButtonHeight);
        Rect skillRect = new Rect(rect.x, attackRect.yMax + menuButtonGap, rect.width, menuButtonHeight);
        Rect itemRect = new Rect(rect.x, skillRect.yMax + menuButtonGap, rect.width, menuButtonHeight);

        Event current = Event.current;
        Vector2 mousePosition = current != null ? current.mousePosition : Vector2.zero;
        string hoveredActionKey = string.Empty;

        if (DrawBattleActionMenuButton(attackRect, "attack", "Attack  [1]", _bootEntry.IsBattleActionAvailable("attack"), _bootEntry.IsBattleActionSelected("attack"), mousePosition, ref hoveredActionKey))
        {
            _bootEntry.TryTriggerBattleAction("attack");
            _battleFlyoutMode = BattleHudFlyoutMode.ActorCommandMenu;
        }

        if (DrawBattleActionMenuButton(skillRect, "skill", "Skill  [2]", _bootEntry.IsBattleActionAvailable("skill"), _bootEntry.IsBattleActionSelected("skill") || _battleFlyoutMode == BattleHudFlyoutMode.SkillListPanel, mousePosition, ref hoveredActionKey))
        {
            _battleFlyoutMode = BattleHudFlyoutMode.SkillListPanel;
        }

        bool itemHovered = itemRect.Contains(mousePosition);
        if (itemHovered)
        {
            _battleHudHoverDetailKey = "item";
        }

        if (DrawBottomCommandButton(itemRect, "Items", _bootEntry.IsDungeonBattleViewActive, itemHovered, _battleFlyoutMode == BattleHudFlyoutMode.ItemListPanel))
        {
            _battleFlyoutMode = BattleHudFlyoutMode.ItemListPanel;
        }

        if (string.IsNullOrEmpty(hoveredActionKey))
        {
            _bootEntry.SetBattleActionHover(string.Empty);
        }
    }

    private void DrawSkillListPanel(Rect rect)
    {
        float menuButtonHeight = 42f;
        float menuButtonGap = 8f;
        float totalHeight = (menuButtonHeight * 2f) + menuButtonGap;
        float startY = rect.y + Mathf.Max(0f, (rect.height - totalHeight) * 0.5f);
        string skillLabel = GetCompactHudText("Use " + GetCurrentActorSkillName(), 20, false) + "  [2]";
        Rect useRect = new Rect(rect.x, startY, rect.width, menuButtonHeight);
        Rect backRect = new Rect(rect.x, useRect.yMax + menuButtonGap, rect.width, menuButtonHeight);

        Event current = Event.current;
        Vector2 mousePosition = current != null ? current.mousePosition : Vector2.zero;
        string hoveredActionKey = string.Empty;

        if (DrawBattleActionMenuButton(useRect, "skill", skillLabel, _bootEntry.IsBattleActionAvailable("skill"), _bootEntry.IsBattleActionSelected("skill"), mousePosition, ref hoveredActionKey))
        {
            _bootEntry.TryTriggerBattleAction("skill");
            _battleFlyoutMode = BattleHudFlyoutMode.ActorCommandMenu;
        }

        bool backHovered = backRect.Contains(mousePosition);
        if (backHovered)
        {
            _battleHudHoverDetailKey = "back";
        }

        if (DrawBottomCommandButton(backRect, "Back", true, backHovered, false))
        {
            _battleFlyoutMode = BattleHudFlyoutMode.ActorCommandMenu;
        }

        if (string.IsNullOrEmpty(hoveredActionKey))
        {
            _bootEntry.SetBattleActionHover(string.Empty);
        }
    }

    private void DrawItemListPanel(Rect rect)
    {
        float menuButtonHeight = 42f;
        float menuButtonGap = 8f;
        float totalHeight = (menuButtonHeight * 2f) + menuButtonGap;
        float startY = rect.y + Mathf.Max(0f, (rect.height - totalHeight) * 0.5f);
        Rect emptyRect = new Rect(rect.x, startY, rect.width, menuButtonHeight);
        Rect backRect = new Rect(rect.x, emptyRect.yMax + menuButtonGap, rect.width, menuButtonHeight);

        Event current = Event.current;
        Vector2 mousePosition = current != null ? current.mousePosition : Vector2.zero;
        bool emptyHovered = emptyRect.Contains(mousePosition);
        bool backHovered = backRect.Contains(mousePosition);
        if (emptyHovered)
        {
            _battleHudHoverDetailKey = "item";
        }
        else if (backHovered)
        {
            _battleHudHoverDetailKey = "back";
        }

        DrawInteractiveButton(emptyRect, "Inventory Empty", false, false, false);
        if (DrawBottomCommandButton(backRect, "Back", true, backHovered, false))
        {
            _battleFlyoutMode = BattleHudFlyoutMode.ActorCommandMenu;
        }

        _bootEntry.SetBattleActionHover(string.Empty);
    }

    private void DrawPartyCommandMenu(Rect rect)
    {
        float menuButtonHeight = 42f;
        float menuButtonGap = 8f;
        float totalHeight = (menuButtonHeight * 2f) + menuButtonGap;
        float startY = rect.y + Mathf.Max(0f, (rect.height - totalHeight) * 0.5f);
        Rect retreatRect = new Rect(rect.x, startY, rect.width, menuButtonHeight);
        Rect backRect = new Rect(rect.x, retreatRect.yMax + menuButtonGap, rect.width, menuButtonHeight);

        Event current = Event.current;
        Vector2 mousePosition = current != null ? current.mousePosition : Vector2.zero;
        string hoveredActionKey = string.Empty;

        if (DrawBattleActionMenuButton(retreatRect, "retreat", "Retreat  [3]", _bootEntry.IsBattleActionAvailable("retreat"), false, mousePosition, ref hoveredActionKey))
        {
            _battleFlyoutMode = BattleHudFlyoutMode.ConfirmDialog;
            _pendingConfirmActionKey = "retreat";
        }

        bool backHovered = backRect.Contains(mousePosition);
        if (backHovered)
        {
            _battleHudHoverDetailKey = "back";
        }

        if (DrawBottomCommandButton(backRect, "Back", true, backHovered, false))
        {
            _battleFlyoutMode = BattleHudFlyoutMode.ActorCommandMenu;
            _pendingConfirmActionKey = string.Empty;
        }

        if (string.IsNullOrEmpty(hoveredActionKey))
        {
            _bootEntry.SetBattleActionHover(string.Empty);
        }
    }

    private void DrawConfirmDialog(Rect rect)
    {
        float menuButtonHeight = 42f;
        float menuButtonGap = 8f;
        float totalHeight = (menuButtonHeight * 2f) + menuButtonGap;
        float startY = rect.y + Mathf.Max(0f, (rect.height - totalHeight) * 0.5f);
        Rect confirmRect = new Rect(rect.x, startY, rect.width, menuButtonHeight);
        Rect cancelRect = new Rect(rect.x, confirmRect.yMax + menuButtonGap, rect.width, menuButtonHeight);
        string actionKey = string.IsNullOrEmpty(_pendingConfirmActionKey) ? "retreat" : _pendingConfirmActionKey;
        string actionLabel = actionKey == "retreat" ? "Retreat" : actionKey;

        Event current = Event.current;
        Vector2 mousePosition = current != null ? current.mousePosition : Vector2.zero;
        string hoveredActionKey = string.Empty;
        bool confirmHovered = confirmRect.Contains(mousePosition);
        bool cancelHovered = cancelRect.Contains(mousePosition);
        if (confirmHovered)
        {
            _battleHudHoverDetailKey = "retreat_confirm";
        }
        else if (cancelHovered)
        {
            _battleHudHoverDetailKey = "cancel";
        }

        if (DrawBattleActionMenuButton(confirmRect, actionKey, "Confirm " + actionLabel, _bootEntry.IsBattleActionAvailable(actionKey), false, mousePosition, ref hoveredActionKey))
        {
            _bootEntry.TryTriggerBattleAction(actionKey);
            ResetBattleHudState();
        }

        if (DrawBottomCommandButton(cancelRect, "Keep Fighting", true, cancelHovered, false))
        {
            _battleFlyoutMode = BattleHudFlyoutMode.PartyCommandMenu;
            _pendingConfirmActionKey = string.Empty;
        }

        if (string.IsNullOrEmpty(hoveredActionKey))
        {
            _bootEntry.SetBattleActionHover(string.Empty);
        }
    }

    private void DrawBottomCommandBar(Rect rect)
    {
        DrawOverlaySectionBackground(rect, new Color(0.07f, 0.10f, 0.14f, 0.92f));
        Rect innerRect = new Rect(rect.x + SectionInnerPadding, rect.y + 6f, rect.width - (SectionInnerPadding * 2f), rect.height - 12f);
        float tabWidth = Mathf.Clamp(innerRect.width * 0.10f, 96f, 122f);
        Rect actorRect = new Rect(innerRect.x, innerRect.y, tabWidth, innerRect.height);
        Rect partyRect = new Rect(actorRect.xMax + 10f, innerRect.y, tabWidth, innerRect.height);
        Rect infoRect = new Rect(partyRect.xMax + 12f, innerRect.y, innerRect.width - (tabWidth * 2f) - 22f, innerRect.height);

        Event current = Event.current;
        Vector2 mousePosition = current != null ? current.mousePosition : Vector2.zero;
        BattleHudFlyoutMode mode = GetDisplayedBattleFlyoutMode();
        bool actorSelected = mode == BattleHudFlyoutMode.ActorCommandMenu || mode == BattleHudFlyoutMode.SkillListPanel || mode == BattleHudFlyoutMode.ItemListPanel;
        bool partySelected = mode == BattleHudFlyoutMode.PartyCommandMenu || mode == BattleHudFlyoutMode.ConfirmDialog;

        if (DrawBottomCommandButton(actorRect, "Actor", true, actorRect.Contains(mousePosition), actorSelected))
        {
            _battleFlyoutMode = BattleHudFlyoutMode.ActorCommandMenu;
            _pendingConfirmActionKey = string.Empty;
        }

        if (DrawBottomCommandButton(partyRect, "Party", true, partyRect.Contains(mousePosition), partySelected))
        {
            _battleFlyoutMode = BattleHudFlyoutMode.PartyCommandMenu;
            _pendingConfirmActionKey = string.Empty;
        }

        DrawOverlaySectionBackground(infoRect, new Color(0.09f, 0.13f, 0.18f, 0.90f));
        float pillGap = 8f;
        float currentWidth = Mathf.Clamp(infoRect.width * 0.16f, 124f, 180f);
        float phaseWidth = Mathf.Clamp(infoRect.width * 0.13f, 108f, 140f);
        float modeWidth = Mathf.Clamp(infoRect.width * 0.12f, 104f, 132f);
        Rect currentRect = new Rect(infoRect.x + 8f, infoRect.y + 4f, currentWidth, infoRect.height - 8f);
        Rect phaseRect = new Rect(currentRect.xMax + pillGap, currentRect.y, phaseWidth, currentRect.height);
        Rect modeRect = new Rect(phaseRect.xMax + pillGap, currentRect.y, modeWidth, currentRect.height);
        Rect hintRect = new Rect(modeRect.xMax + pillGap, currentRect.y, infoRect.xMax - modeRect.xMax - pillGap - 8f, currentRect.height);
        string inputText = IsTargetSelectionActive()
            ? "Click target or " + GetBattleCancelHintText()
            : "[1][2][3] or click / " + GetBattleCancelHintText();
        DrawBattleInfoPill(currentRect, "Current", GetBattleActorShortLabel(), new Color(0.11f, 0.15f, 0.21f, 0.88f));
        DrawBattleInfoPill(phaseRect, "Phase", GetBattlePhaseShortLabel(), new Color(0.11f, 0.15f, 0.21f, 0.88f));
        DrawBattleInfoPill(modeRect, "Mode", GetBattleModeLabel(), new Color(0.11f, 0.15f, 0.21f, 0.88f));
        DrawBattleInfoPill(hintRect, "Input", inputText, new Color(0.11f, 0.15f, 0.21f, 0.88f));
    }

    private void DrawBattleMessageBox(Rect rect)
    {
        DrawOverlaySectionBackground(rect, new Color(0.05f, 0.08f, 0.11f, 0.84f));
        PrototypeBattleUiMessageSurfaceData messageSurface = GetBattleUiSurfaceData().MessageSurface;
        Rect promptRect = new Rect(rect.x + 12f, rect.y + 6f, rect.width * 0.72f, rect.height - 12f);
        Rect dividerRect = new Rect(promptRect.xMax + 6f, rect.y + 7f, 1f, rect.height - 14f);
        Rect feedbackRect = new Rect(dividerRect.xMax + 8f, rect.y + 6f, rect.xMax - dividerRect.xMax - 20f, rect.height - 12f);

        string prompt = messageSurface != null && HasMeaningfulText(messageSurface.Prompt)
            ? GetCompactHudText(messageSurface.Prompt, 132, false)
            : "Select an action.";
        string feedback = messageSurface != null && HasMeaningfulText(messageSurface.Feedback)
            ? GetCompactHudText(messageSurface.Feedback, 64, false)
            : "No new feedback.";

        DrawFittedLabel(promptRect, prompt, _bodyStyle, 10, 9, false);
        Color previousColor = GUI.color;
        GUI.color = new Color(0.56f, 0.68f, 0.82f, 0.16f);
        GUI.DrawTexture(dividerRect, Texture2D.whiteTexture);
        GUI.color = Color.white;
        DrawFittedLabel(feedbackRect, "Last: " + feedback, _bodyStyle, 10, 9, false);
        GUI.color = previousColor;
    }

    private void DrawTargetSelectionOverlay(Rect hudRect, Rect mainRect)
    {
        PrototypeBattleUiTargetSelectionData targetSelection = GetBattleUiSurfaceData().TargetSelection;
        float overlayWidth = Mathf.Min(520f, Screen.width * 0.44f);
        float overlayHeight = targetSelection != null && targetSelection.HasFocusedTarget ? 64f : 52f;
        float overlayX = (Screen.width - overlayWidth) * 0.5f;
        float overlayY = Mathf.Max(hudRect.yMax + 14f, mainRect.y - overlayHeight - 12f);
        Rect overlayRect = new Rect(overlayX, overlayY, overlayWidth, overlayHeight);
        DrawOverlaySectionBackground(overlayRect, new Color(0.18f, 0.13f, 0.08f, 0.94f));

        Rect titleRect = new Rect(overlayRect.x + 12f, overlayRect.y + 6f, overlayRect.width - 24f, 14f);
        Rect line1Rect = new Rect(overlayRect.x + 12f, titleRect.yMax + 4f, overlayRect.width - 24f, 16f);
        Rect line2Rect = new Rect(overlayRect.x + 12f, line1Rect.yMax + 3f, overlayRect.width - 24f, 16f);
        string title = targetSelection != null && HasMeaningfulText(targetSelection.Title)
            ? targetSelection.Title
            : "Select target";
        string actionText = targetSelection != null && HasMeaningfulText(targetSelection.QueuedActionLabel)
            ? targetSelection.QueuedActionLabel
            : "Action";
        string targetText = targetSelection != null && HasMeaningfulText(targetSelection.TargetLabel)
            ? targetSelection.TargetLabel
            : "Choose a target";
        string cancelText = targetSelection != null && HasMeaningfulText(targetSelection.CancelHint)
            ? GetCompactHudText(targetSelection.CancelHint, 36, false)
            : GetBattleCancelHintText();
        DrawFittedLabel(titleRect, title, _sectionTitleStyle, 10, 9, false);
        DrawFittedLabel(line1Rect, GetCompactHudText(actionText + "  |  " + targetText + "  |  " + cancelText, 104, false), _bodyStyle, 10, 9, false);
        if (targetSelection != null && targetSelection.HasFocusedTarget)
        {
            string detail = "HP " + targetSelection.TargetCurrentHp + " / " + targetSelection.TargetMaxHp;
            if (HasMeaningfulText(targetSelection.TargetIntentLabel))
            {
                detail += "  |  " + GetCompactHudText(targetSelection.TargetIntentLabel, 38, false);
            }
            DrawFittedLabel(line2Rect, detail, _bodyStyle, 10, 9, false);
        }
    }

    private bool DrawBattleActionMenuButton(Rect rect, string actionKey, string label, bool available, bool selected, Vector2 mousePosition, ref string hoveredActionKey)
    {
        bool hovered = available && rect.Contains(mousePosition);
        if (hovered)
        {
            hoveredActionKey = actionKey;
            _battleHudHoverDetailKey = actionKey;
            _bootEntry.SetBattleActionHover(actionKey);
        }

        return DrawInteractiveButton(rect, label, available, hovered, selected);
    }

    private bool DrawBottomCommandButton(Rect rect, string label, bool available, bool hovered, bool selected)
    {
        return DrawInteractiveButton(rect, label, available, hovered, selected);
    }

    private string BuildEnemyRosterBody()
    {
        List<string> lines = new List<string>();
        PrototypeBattleUiEnemyData[] roster = GetBattleUiSurfaceData().EnemyRoster;
        if (roster != null)
        {
            for (int i = 0; i < roster.Length; i++)
            {
                PrototypeBattleUiEnemyData enemyData = roster[i];
                if (enemyData == null || !HasMeaningfulText(enemyData.DisplayName))
                {
                    continue;
                }

                string line = enemyData.DisplayName + " | HP " + enemyData.CurrentHp + " / " + enemyData.MaxHp + " | " + enemyData.StateLabel;
                if (enemyData.IsElite)
                {
                    line += " | Elite";
                }

                lines.Add(GetCompactHudText(line, 60, false));
            }
        }

        if (lines.Count == 0)
        {
            lines.Add("Enemy roster pending.");
        }

        return BuildPanelBody(lines.ToArray());
    }

    private string BuildCommandDetailBody(BattleHudFlyoutMode mode)
    {
        PrototypeBattleUiCommandDetailData detail = GetBattleUiCommandDetail(mode);
        List<string> lines = new List<string>();
        string title = detail != null && HasMeaningfulText(detail.Label) ? detail.Label : "Command";
        lines.Add(title);

        if (detail != null && HasMeaningfulText(detail.Description))
        {
            lines.Add(detail.Description);
        }

        if (detail != null && HasMeaningfulText(detail.TargetText))
        {
            lines.Add("Target: " + detail.TargetText);
        }

        if (detail != null && HasMeaningfulText(detail.CostText))
        {
            lines.Add("Cost: " + detail.CostText);
        }

        if (detail != null && HasMeaningfulText(detail.EffectText))
        {
            lines.Add("Effect: " + detail.EffectText);
        }

        if (detail != null && !detail.IsAvailable)
        {
            lines.Add("Availability: Not available in this batch.");
        }

        return BuildPanelBody(lines.ToArray());
    }


    private string GetCommandDetailKey(BattleHudFlyoutMode mode)
    {
        if (HasMeaningfulText(_battleHudHoverDetailKey))
        {
            return _battleHudHoverDetailKey;
        }

        if (_bootEntry.IsBattleActionHovered("attack"))
        {
            return "attack";
        }

        if (_bootEntry.IsBattleActionHovered("skill"))
        {
            return "skill";
        }

        if (_bootEntry.IsBattleActionHovered("retreat"))
        {
            return "retreat";
        }

        if (mode == BattleHudFlyoutMode.SkillListPanel)
        {
            return "skill";
        }

        if (mode == BattleHudFlyoutMode.ItemListPanel)
        {
            return "item";
        }

        if (mode == BattleHudFlyoutMode.PartyCommandMenu)
        {
            return "retreat";
        }

        if (mode == BattleHudFlyoutMode.ConfirmDialog)
        {
            return "retreat_confirm";
        }

        string selectedActionKey = GetSelectedBattleActionKey();
        return HasMeaningfulText(selectedActionKey) ? selectedActionKey : "attack";
    }

    private string BuildPartyCardStatusText(string memberLabel, bool active, bool targeted, bool knockedOut)
    {
        List<string> states = new List<string>();
        if (knockedOut)
        {
            states.Add("KO");
        }
        else if (active)
        {
            states.Add("Acting");
        }
        else
        {
            states.Add("Ready");
        }

        if (targeted)
        {
            states.Add("Targeted");
        }

        if (ContainsIgnoreCase(memberLabel, "guard"))
        {
            states.Add("Guard");
        }

        return string.Join(" | ", states.ToArray());
    }

    private string BuildBottomCommandBarBody()
    {
        string modeLabel = GetDisplayedBattleFlyoutMode() == BattleHudFlyoutMode.PartyCommandMenu || GetDisplayedBattleFlyoutMode() == BattleHudFlyoutMode.ConfirmDialog
            ? "Party"
            : "Actor";
        return "Actor: " + GetBattleActorShortLabel() + " | Phase: " + GetBattlePhaseShortLabel() + "\nMode: " + modeLabel + "   Confirm: Click / [1][2][3]   Cancel: " + GetBattleCancelHintText();
    }

    private string GetBattleHudHeaderTitle()
    {
        PrototypeBattleUiSurfaceData surface = GetBattleUiSurfaceData();
        string encounter = surface != null && HasMeaningfulText(surface.EncounterName) && surface.EncounterName != "None"
            ? surface.EncounterName
            : "Encounter";
        string roomType = surface != null && HasMeaningfulText(surface.EncounterRoomType) && surface.EncounterRoomType != "None"
            ? surface.EncounterRoomType
            : string.Empty;
        return HasMeaningfulText(roomType)
            ? GetCompactHudText(encounter + " / " + roomType, 72, false)
            : GetCompactHudText(encounter, 72, false);
    }

    private string GetBattleHudHeaderSummary()
    {
        PrototypeBattleUiSurfaceData surface = GetBattleUiSurfaceData();
        List<string> tokens = new List<string>();
        if (surface != null && HasMeaningfulText(surface.CurrentDungeonName) && surface.CurrentDungeonName != "None")
        {
            tokens.Add(surface.CurrentDungeonName);
        }

        if (surface != null && HasMeaningfulText(surface.CurrentRouteLabel) && surface.CurrentRouteLabel != "None")
        {
            tokens.Add(surface.CurrentRouteLabel);
        }

        if (surface != null && HasMeaningfulText(surface.TotalPartyHp) && surface.TotalPartyHp != "None")
        {
            tokens.Add("Party " + surface.TotalPartyHp);
        }

        if (surface != null && HasMeaningfulText(surface.EliteStatusText) && surface.EliteStatusText != "None")
        {
            tokens.Add(surface.EliteStatusText);
        }

        return tokens.Count > 0
            ? GetCompactHudText(string.Join("   ", tokens.ToArray()), 132, false)
            : "Battle in progress";
    }

    private Color GetBattleRoleAccentColor(string roleLabel, bool enemy)
    {
        if (ContainsIgnoreCase(roleLabel, "warrior") || ContainsIgnoreCase(roleLabel, "bulwark") || ContainsIgnoreCase(roleLabel, "front"))
        {
            return new Color(0.72f, 0.52f, 0.24f, 1f);
        }

        if (ContainsIgnoreCase(roleLabel, "rogue") || ContainsIgnoreCase(roleLabel, "striker"))
        {
            return new Color(0.28f, 0.67f, 0.62f, 1f);
        }

        if (ContainsIgnoreCase(roleLabel, "mage") || ContainsIgnoreCase(roleLabel, "caster") || ContainsIgnoreCase(roleLabel, "arcane"))
        {
            return new Color(0.38f, 0.62f, 0.84f, 1f);
        }

        if (ContainsIgnoreCase(roleLabel, "cleric") || ContainsIgnoreCase(roleLabel, "support") || ContainsIgnoreCase(roleLabel, "healer"))
        {
            return new Color(0.56f, 0.76f, 0.48f, 1f);
        }

        return enemy
            ? new Color(0.72f, 0.34f, 0.28f, 1f)
            : new Color(0.42f, 0.56f, 0.72f, 1f);
    }

    private string GetBattleModeLabel()
    {
        BattleHudFlyoutMode mode = GetDisplayedBattleFlyoutMode();
        return mode == BattleHudFlyoutMode.SkillListPanel
            ? "Skill"
            : mode == BattleHudFlyoutMode.ItemListPanel
                ? "Item"
                : mode == BattleHudFlyoutMode.PartyCommandMenu
                    ? "Party"
                    : mode == BattleHudFlyoutMode.ConfirmDialog
                        ? "Confirm"
                        : "Actor";
    }

    private void DrawFittedLabel(Rect rect, string text, GUIStyle baseStyle, int maxFontSize, int minFontSize, bool wordWrap)
    {
        string safeText = string.IsNullOrEmpty(text) ? string.Empty : text;
        GUIStyle fittedStyle = new GUIStyle(baseStyle);
        fittedStyle.fontSize = Mathf.Max(minFontSize, maxFontSize);
        fittedStyle.wordWrap = wordWrap;
        fittedStyle.clipping = TextClipping.Clip;
        fittedStyle.padding = new RectOffset(0, 0, 0, 0);
        fittedStyle.margin = new RectOffset(0, 0, 0, 0);

        GUIContent content = new GUIContent(safeText);
        float measuredHeight = wordWrap ? fittedStyle.CalcHeight(content, rect.width) : fittedStyle.CalcSize(content).y;
        while (fittedStyle.fontSize > minFontSize && measuredHeight > rect.height)
        {
            fittedStyle.fontSize--;
            measuredHeight = wordWrap ? fittedStyle.CalcHeight(content, rect.width) : fittedStyle.CalcSize(content).y;
        }

        GUI.Label(rect, safeText, fittedStyle);
    }

    private void DrawBattleInfoPill(Rect rect, string title, string value, Color backgroundColor)
    {
        DrawOverlaySectionBackground(rect, backgroundColor);
        Rect titleRect = new Rect(rect.x + 8f, rect.y + 4f, rect.width - 16f, 12f);
        Rect valueRect = new Rect(rect.x + 8f, titleRect.yMax + 4f, rect.width - 16f, rect.height - 20f);
        int maxLength = rect.width < 132f ? 14 : rect.width < 220f ? 24 : 52;
        DrawFittedLabel(titleRect, title, _sectionTitleStyle, 9, 8, false);
        DrawFittedLabel(valueRect, HasMeaningfulText(value) ? GetCompactHudText(value, maxLength, false) : "None", _bodyStyle, 10, 8, true);
    }

    private string GetBattlePromptMessage()
    {
        PrototypeBattleUiMessageSurfaceData messageSurface = GetBattleUiSurfaceData().MessageSurface;
        if (messageSurface != null && HasMeaningfulText(messageSurface.Prompt))
        {
            return GetCompactHudText(messageSurface.Prompt, 132, false);
        }

        return "Select an action.";
    }

    private string GetBattleCancelHintText()
    {
        PrototypeBattleUiMessageSurfaceData messageSurface = GetBattleUiSurfaceData().MessageSurface;
        return messageSurface != null && HasMeaningfulText(messageSurface.CancelHint)
            ? GetCompactHudText(messageSurface.CancelHint, 56, false)
            : "Esc: Cancel";
    }

    private string GetBattleActorShortLabel()
    {
        PrototypeBattleUiActorData actor = GetBattleUiSurfaceData().CurrentActor;
        if (actor != null && HasMeaningfulText(actor.DisplayName))
        {
            return GetCompactHudText(actor.DisplayName, 34, false);
        }

        return "None";
    }

    private string GetCurrentActorRoleLabel()
    {
        PrototypeBattleUiActorData actor = GetBattleUiSurfaceData().CurrentActor;
        return actor != null && HasMeaningfulText(actor.RoleLabel) ? actor.RoleLabel : "None";
    }

    private string GetCurrentActorSkillName()
    {
        PrototypeBattleUiActorData actor = GetBattleUiSurfaceData().CurrentActor;
        return actor != null && HasMeaningfulText(actor.SkillLabel) ? actor.SkillLabel : "None";
    }

    private string GetBattlePhaseShortLabel()
    {
        PrototypeBattleUiTimelineData timeline = GetBattleUiSurfaceData().Timeline;
        return timeline != null && HasMeaningfulText(timeline.PhaseLabel)
            ? GetCompactHudText(timeline.PhaseLabel, 24, false)
            : "Battle";
    }

    private void RefreshBattleUiSurface()
    {
        if (_bootEntry == null)
        {
            _battleUiSurface = new PrototypeBattleUiSurfaceData();
            return;
        }

        PrototypeBattleUiSurfaceData surface = _bootEntry.GetBattleUiSurfaceData();
        _battleUiSurface = surface ?? new PrototypeBattleUiSurfaceData();
    }

    private PrototypeBattleUiSurfaceData GetBattleUiSurfaceData()
    {
        if (_battleUiSurface == null)
        {
            _battleUiSurface = new PrototypeBattleUiSurfaceData();
        }

        return _battleUiSurface;
    }

    private PrototypeBattleUiCommandDetailData GetBattleUiCommandDetail(BattleHudFlyoutMode mode)
    {
        PrototypeBattleUiCommandSurfaceData commandSurface = GetBattleUiSurfaceData().CommandSurface;
        PrototypeBattleUiCommandDetailData fallbackDetail = new PrototypeBattleUiCommandDetailData();
        fallbackDetail.Label = "Command";
        fallbackDetail.Description = "Details pending.";
        if (commandSurface == null || commandSurface.Details == null || commandSurface.Details.Length == 0)
        {
            return fallbackDetail;
        }

        string detailKey = GetCommandDetailKey(mode);
        for (int i = 0; i < commandSurface.Details.Length; i++)
        {
            PrototypeBattleUiCommandDetailData detail = commandSurface.Details[i];
            if (detail != null && detail.Key == detailKey)
            {
                return detail;
            }
        }

        for (int i = 0; i < commandSurface.Details.Length; i++)
        {
            PrototypeBattleUiCommandDetailData detail = commandSurface.Details[i];
            if (detail != null && detail.IsSelected)
            {
                return detail;
            }
        }

        return commandSurface.Details[0] ?? fallbackDetail;
    }

    private string GetCompactHudText(string value, int maxLength, bool preferPipePrefix)
    {
        if (!HasMeaningfulText(value))
        {
            return "None";
        }

        string compact = value.Replace('\r', ' ').Replace('\n', ' ').Trim();
        if (preferPipePrefix)
        {
            int pipeIndex = compact.IndexOf('|');
            if (pipeIndex > 0)
            {
                compact = compact.Substring(0, pipeIndex).Trim();
            }
        }

        while (compact.Contains("  "))
        {
            compact = compact.Replace("  ", " ");
        }

        if (maxLength > 3 && compact.Length > maxLength)
        {
            compact = compact.Substring(0, maxLength - 3) + "...";
        }

        return compact;
    }

    private string GetBattleLabelToken(string value, int tokenIndex)
    {
        if (!HasMeaningfulText(value))
        {
            return string.Empty;
        }

        string[] tokens = value.Split('|');
        if (tokenIndex < 0 || tokenIndex >= tokens.Length)
        {
            return string.Empty;
        }

        return tokens[tokenIndex].Trim();
    }

    private string GetBattleLabelValue(string value, string prefix)
    {
        if (!HasMeaningfulText(value) || !HasMeaningfulText(prefix))
        {
            return string.Empty;
        }

        string[] tokens = value.Split('|');
        for (int index = 0; index < tokens.Length; index++)
        {
            string token = tokens[index].Trim();
            if (token.StartsWith(prefix + " ", System.StringComparison.OrdinalIgnoreCase))
            {
                return token.Substring(prefix.Length).Trim();
            }
        }

        return string.Empty;
    }

    private string GetPreferredText(string primary, string fallback)
    {
        return HasMeaningfulText(primary) && primary != "None"
            ? primary
            : fallback;
    }

    private bool TryParseCurrentMaxPair(string value, out float current, out float max)
    {
        current = 0f;
        max = 0f;
        if (!HasMeaningfulText(value))
        {
            return false;
        }

        string numericText = ExtractNumericPairText(value);
        if (string.IsNullOrEmpty(numericText))
        {
            return false;
        }

        string[] segments = numericText.Split('/');
        if (segments.Length != 2)
        {
            return false;
        }

        return float.TryParse(segments[0], out current) &&
               float.TryParse(segments[1], out max) &&
               max > 0f;
    }

    private string ExtractNumericPairText(string value)
    {
        char[] buffer = new char[value.Length];
        int count = 0;
        for (int index = 0; index < value.Length; index++)
        {
            char current = value[index];
            if (char.IsDigit(current) || current == '/')
            {
                buffer[count++] = current;
            }
        }

        return count > 0 ? new string(buffer, 0, count) : string.Empty;
    }

    private void DrawStatusBar(Rect rect, float current, float max, Color fillColor, string label)
    {
        Color previousColor = GUI.color;
        GUI.color = new Color(0.06f, 0.08f, 0.11f, 0.92f);
        GUI.Box(rect, GUIContent.none, _panelStyle);

        float fillRatio = Mathf.Clamp01(current / max);
        if (fillRatio > 0f)
        {
            Rect fillRect = new Rect(rect.x + 1f, rect.y + 1f, (rect.width - 2f) * fillRatio, rect.height - 2f);
            GUI.color = fillColor;
            GUI.DrawTexture(fillRect, Texture2D.whiteTexture);
        }

        GUI.color = Color.white;
        int maxLength = rect.width < 120f ? 14 : 22;
        GUI.Label(new Rect(rect.x + 6f, rect.y, rect.width - 12f, rect.height), GetCompactHudText(label, maxLength, false), _sectionTitleStyle);
        GUI.color = previousColor;
    }
    private string GetNextBattleTimelineText()
    {
        PrototypeBattleUiTimelineData timeline = GetBattleUiSurfaceData().Timeline;
        return timeline != null && HasMeaningfulText(timeline.NextStepLabel)
            ? GetCompactHudText(timeline.NextStepLabel, 44, false)
            : "Choose command -> choose target";
    }

    private string GetSelectedBattleActionLabel()
    {
        PrototypeBattleUiCommandSurfaceData commandSurface = GetBattleUiSurfaceData().CommandSurface;
        if (commandSurface != null && HasMeaningfulText(commandSurface.SelectedActionLabel))
        {
            return commandSurface.SelectedActionLabel;
        }

        string actionKey = GetSelectedBattleActionKey();
        return actionKey == "attack"
            ? "Attack"
            : actionKey == "skill"
                ? GetCurrentActorSkillName()
                : actionKey == "retreat"
                    ? "Retreat"
                    : "Action";
    }

    private string GetSelectedBattleActionKey()
    {
        if (_bootEntry.IsBattleActionSelected("attack"))
        {
            return "attack";
        }

        if (_bootEntry.IsBattleActionSelected("skill"))
        {
            return "skill";
        }

        if (_bootEntry.IsBattleActionSelected("retreat"))
        {
            return "retreat";
        }

        return string.Empty;
    }

    private BattleHudFlyoutMode GetDisplayedBattleFlyoutMode()
    {
        if (_battleFlyoutMode == BattleHudFlyoutMode.ConfirmDialog && string.IsNullOrEmpty(_pendingConfirmActionKey))
        {
            return BattleHudFlyoutMode.PartyCommandMenu;
        }

        return _battleFlyoutMode == BattleHudFlyoutMode.None
            ? BattleHudFlyoutMode.ActorCommandMenu
            : _battleFlyoutMode;
    }

    private void SyncBattleHudState()
    {
        if (_bootEntry == null || !_bootEntry.IsDungeonBattleViewActive)
        {
            ResetBattleHudState();
            return;
        }

        if (_battleFlyoutMode != BattleHudFlyoutMode.ConfirmDialog)
        {
            _pendingConfirmActionKey = string.Empty;
        }
    }

    private void ResetBattleHudState()
    {
        _battleFlyoutMode = BattleHudFlyoutMode.None;
        _pendingConfirmActionKey = string.Empty;
        _battleHudHoverDetailKey = string.Empty;
    }

    private bool IsBattleInputModalOpen()
    {
        if (_bootEntry == null || !_bootEntry.IsDungeonBattleViewActive)
        {
            return false;
        }

        return _battleFlyoutMode == BattleHudFlyoutMode.SkillListPanel ||
               _battleFlyoutMode == BattleHudFlyoutMode.ItemListPanel ||
               _battleFlyoutMode == BattleHudFlyoutMode.ConfirmDialog;
    }

    private bool IsTargetSelectionActive()
    {
        PrototypeBattleUiTargetSelectionData targetSelection = GetBattleUiSurfaceData().TargetSelection;
        return targetSelection != null && targetSelection.IsActive;
    }

    private static bool ContainsIgnoreCase(string value, string token)
    {
        return !string.IsNullOrEmpty(value) &&
               !string.IsNullOrEmpty(token) &&
               value.IndexOf(token, System.StringComparison.OrdinalIgnoreCase) >= 0;
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

    private Vector2 GetOverlayScrollPosition(string scrollKey)
    {
        if (string.IsNullOrEmpty(scrollKey))
        {
            return Vector2.zero;
        }

        return _overlayScrollByKey.TryGetValue(scrollKey, out Vector2 scrollPosition)
            ? scrollPosition
            : Vector2.zero;
    }

    private void DrawScrollableTextRegion(string scrollKey, Rect viewportRect, string body)
    {
        string safeBody = string.IsNullOrEmpty(body) ? "None" : body;
        bool showVerticalScrollbar = false;
        float contentWidth = Mathf.Max(32f, viewportRect.width - 4f);
        float contentHeight = Mathf.Max(viewportRect.height, _bodyStyle.CalcHeight(new GUIContent(safeBody), contentWidth));
        if (contentHeight > viewportRect.height)
        {
            showVerticalScrollbar = true;
            contentWidth = Mathf.Max(32f, viewportRect.width - 18f);
            contentHeight = Mathf.Max(viewportRect.height, _bodyStyle.CalcHeight(new GUIContent(safeBody), contentWidth));
        }

        Vector2 scrollPosition = GetOverlayScrollPosition(scrollKey);
        Vector2 nextScrollPosition = GUI.BeginScrollView(
            viewportRect,
            scrollPosition,
            new Rect(0f, 0f, contentWidth, contentHeight),
            false,
            showVerticalScrollbar);
        GUI.Label(new Rect(0f, 0f, contentWidth, contentHeight), safeBody, _bodyStyle);
        GUI.EndScrollView();

        if (!string.IsNullOrEmpty(scrollKey))
        {
            _overlayScrollByKey[scrollKey] = nextScrollPosition;
        }
    }

    private void DrawNamedScrollableOverlaySection(string scrollKey, Rect rect, string title, string body, Color backgroundColor)
    {
        DrawOverlaySectionBackground(rect, backgroundColor);
        Rect titleRect = new Rect(rect.x + SectionInnerPadding, rect.y + 6f, rect.width - (SectionInnerPadding * 2f), 18f);
        GUI.Label(titleRect, title, _sectionTitleStyle);
        float contentHeight = Mathf.Max(24f, rect.height - (titleRect.yMax - rect.y) - SectionInnerPadding);
        Rect contentRect = new Rect(rect.x + SectionInnerPadding, titleRect.yMax + 4f, rect.width - (SectionInnerPadding * 2f), contentHeight);
        DrawScrollableTextRegion(scrollKey, contentRect, body);
    }

    private void DrawScrollableOverlaySection(string scrollKey, Rect rect, string body, Color backgroundColor)
    {
        DrawOverlaySectionBackground(rect, backgroundColor);
        Rect contentRect = new Rect(rect.x + SectionInnerPadding, rect.y + SectionInnerPadding, rect.width - (SectionInnerPadding * 2f), rect.height - (SectionInnerPadding * 2f));
        DrawScrollableTextRegion(scrollKey, contentRect, body);
    }

    private void DrawOverlaySection(Rect rect, string body, Color backgroundColor)
    {
        DrawOverlaySectionBackground(rect, backgroundColor);
        Rect contentRect = new Rect(rect.x + SectionInnerPadding, rect.y + SectionInnerPadding, rect.width - (SectionInnerPadding * 2f), rect.height - (SectionInnerPadding * 2f));
        GUI.Label(contentRect, body, _bodyStyle);
    }

    private void DrawOverlaySectionBackground(Rect rect, Color backgroundColor)
    {
        float innerWidth = Mathf.Max(0f, rect.width - 2f);
        float innerHeight = Mathf.Max(0f, rect.height - 2f);
        Rect innerRect = new Rect(rect.x + 1f, rect.y + 1f, innerWidth, innerHeight);
        Color previousColor = GUI.color;
        GUI.color = new Color(0.01f, 0.02f, 0.03f, Mathf.Clamp01(backgroundColor.a));
        GUI.DrawTexture(rect, Texture2D.whiteTexture);
        if (innerWidth > 0f && innerHeight > 0f)
        {
            GUI.color = backgroundColor;
            GUI.DrawTexture(innerRect, Texture2D.whiteTexture);
            GUI.color = new Color(0.56f, 0.68f, 0.82f, 0.10f);
            GUI.DrawTexture(new Rect(innerRect.x, innerRect.y, innerRect.width, 2f), Texture2D.whiteTexture);
            GUI.color = new Color(0f, 0f, 0f, 0.34f);
            GUI.DrawTexture(new Rect(innerRect.x, innerRect.yMax - 1f, innerRect.width, 1f), Texture2D.whiteTexture);
        }
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
        Color previousBackgroundColor = GUI.backgroundColor;
        Color previousContentColor = GUI.contentColor;
        GUI.backgroundColor = active ? new Color(0.26f, 0.38f, 0.28f, 1f) : new Color(0.14f, 0.18f, 0.15f, 0.96f);
        GUI.contentColor = Color.white;
        if (GUI.Button(rect, label, style))
        {
            _bootEntry.SetLanguage(language);
        }
        GUI.backgroundColor = previousBackgroundColor;
        GUI.contentColor = previousContentColor;
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
            Color previousBackgroundColor = GUI.backgroundColor;
            Color previousContentColor = GUI.contentColor;
            GUI.backgroundColor = active ? new Color(0.18f, 0.20f, 0.18f, 1f) : new Color(0.09f, 0.10f, 0.11f, 0.94f);
            GUI.contentColor = Color.white;
            if (GUI.Button(chipRect, GetFilterLabel(filter), style))
            {
                _activeFilter = filter;
            }
            GUI.backgroundColor = previousBackgroundColor;
            GUI.contentColor = previousContentColor;
        }
    }

    private void DrawSearchBar(Rect rect)
    {
        GUI.Label(new Rect(rect.x, rect.y + 4f, SearchLabelWidth, rect.height), T("Search"), _bodyStyle);
        Rect fieldRect = new Rect(rect.x + SearchLabelWidth, rect.y, rect.width - SearchLabelWidth - SearchClearButtonWidth - ChipGap, rect.height);
        Rect clearRect = new Rect(fieldRect.xMax + ChipGap, rect.y, SearchClearButtonWidth, rect.height);
        GUI.SetNextControlName(SearchFieldControlName);
        _searchText = GUI.TextField(fieldRect, _searchText ?? string.Empty, _searchFieldStyle);
        Color previousBackgroundColor = GUI.backgroundColor;
        Color previousContentColor = GUI.contentColor;
        GUI.backgroundColor = new Color(0.12f, 0.16f, 0.18f, 0.96f);
        GUI.contentColor = Color.white;
        if (GUI.Button(clearRect, T("Clear"), _chipStyle))
        {
            _searchText = string.Empty;
            GUI.FocusControl(string.Empty);
            _isSearchFieldFocused = false;
            GUI.backgroundColor = previousBackgroundColor;
            GUI.contentColor = previousContentColor;
            return;
        }
        GUI.backgroundColor = previousBackgroundColor;
        GUI.contentColor = previousContentColor;

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
            Color previousBackgroundColor = GUI.backgroundColor;
            Color previousContentColor = GUI.contentColor;
            GUI.backgroundColor = new Color(0.12f, 0.17f, 0.18f, 0.96f);
            GUI.contentColor = Color.white;
            if (GUI.Button(headerRect, foldoutLabel, _foldoutStyle))
            {
                _expandedByKey[panel.Key] = !expanded;
                expanded = !expanded;
            }
            GUI.backgroundColor = previousBackgroundColor;
            GUI.contentColor = previousContentColor;

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
        _bodyStyle.fontSize = 12;
        _bodyStyle.wordWrap = true;
        _bodyStyle.alignment = TextAnchor.UpperLeft;
        _bodyStyle.normal.textColor = new Color(0.92f, 0.95f, 0.92f, 1f);
        _bodyStyle.clipping = TextClipping.Clip;
        _bodyStyle.padding = new RectOffset(0, 0, 0, 0);
        _bodyStyle.margin = new RectOffset(0, 0, 0, 0);

        _titleStyle = new GUIStyle(_bodyStyle);
        _titleStyle.fontSize = 16;
        _titleStyle.fontStyle = FontStyle.Bold;
        _titleStyle.wordWrap = false;
        _titleStyle.alignment = TextAnchor.UpperLeft;
        _titleStyle.clipping = TextClipping.Clip;
        _titleStyle.normal.textColor = Color.white;
        _titleStyle.padding = new RectOffset(0, 0, 0, 0);
        _titleStyle.margin = new RectOffset(0, 0, 0, 0);

        _sectionTitleStyle = new GUIStyle(_bodyStyle);
        _sectionTitleStyle.fontSize = 13;
        _sectionTitleStyle.fontStyle = FontStyle.Bold;
        _sectionTitleStyle.wordWrap = false;
        _sectionTitleStyle.alignment = TextAnchor.UpperLeft;
        _sectionTitleStyle.clipping = TextClipping.Clip;
        _sectionTitleStyle.normal.textColor = Color.white;
        _sectionTitleStyle.padding = new RectOffset(0, 0, 0, 0);
        _sectionTitleStyle.margin = new RectOffset(0, 0, 0, 0);

        _chipStyle = new GUIStyle(GUI.skin != null ? GUI.skin.button : GUIStyle.none);
        _chipStyle.fontSize = 13;
        _chipStyle.alignment = TextAnchor.MiddleCenter;
        _chipStyle.normal.textColor = new Color(0.88f, 0.92f, 0.88f, 1f);
        _chipStyle.hover.textColor = Color.white;
        _chipStyle.active.textColor = Color.white;
        _chipStyle.normal.background = Texture2D.whiteTexture;
        _chipStyle.hover.background = Texture2D.whiteTexture;
        _chipStyle.active.background = Texture2D.whiteTexture;
        _chipStyle.border = new RectOffset(1, 1, 1, 1);
        _chipStyle.padding = new RectOffset(8, 8, 3, 3);

        _chipActiveStyle = new GUIStyle(_chipStyle);
        _chipActiveStyle.fontStyle = FontStyle.Bold;

        _foldoutStyle = new GUIStyle(GUI.skin != null ? GUI.skin.button : GUIStyle.none);
        _foldoutStyle.fontSize = 13;
        _foldoutStyle.fontStyle = FontStyle.Bold;
        _foldoutStyle.alignment = TextAnchor.MiddleLeft;
        _foldoutStyle.padding = new RectOffset(12, 8, 1, 1);
        _foldoutStyle.normal.textColor = Color.white;
        _foldoutStyle.hover.textColor = Color.white;
        _foldoutStyle.active.textColor = Color.white;
        _foldoutStyle.normal.background = Texture2D.whiteTexture;
        _foldoutStyle.hover.background = Texture2D.whiteTexture;
        _foldoutStyle.active.background = Texture2D.whiteTexture;

        _searchFieldStyle = new GUIStyle(GUI.skin != null ? GUI.skin.textField : GUIStyle.none);
        _searchFieldStyle.fontSize = 13;
        _searchFieldStyle.alignment = TextAnchor.MiddleLeft;
        _searchFieldStyle.normal.textColor = Color.white;
        _searchFieldStyle.focused.textColor = Color.white;
        _searchFieldStyle.normal.background = Texture2D.whiteTexture;
        _searchFieldStyle.focused.background = Texture2D.whiteTexture;
        _searchFieldStyle.padding = new RectOffset(8, 8, 5, 1);
        _searchFieldStyle.border = new RectOffset(1, 1, 1, 1);

        _panelStyle = new GUIStyle(GUI.skin != null ? GUI.skin.box : GUIStyle.none);
        _panelStyle.normal.background = Texture2D.whiteTexture;
        _panelStyle.border = new RectOffset(1, 1, 1, 1);
        _panelStyle.padding = new RectOffset(0, 0, 0, 0);
        _panelStyle.margin = new RectOffset(0, 0, 0, 0);

        _actionButtonStyle = new GUIStyle(_bodyStyle);
        _actionButtonStyle.fontSize = 13;
        _actionButtonStyle.fontStyle = FontStyle.Bold;
        _actionButtonStyle.alignment = TextAnchor.MiddleCenter;
        _actionButtonStyle.wordWrap = false;
        _actionButtonStyle.clipping = TextClipping.Clip;
        _actionButtonStyle.padding = new RectOffset(8, 8, 1, 1);
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
