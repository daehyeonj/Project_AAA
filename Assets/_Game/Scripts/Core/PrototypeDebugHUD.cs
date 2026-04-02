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
        float barHeight = battleHudShellVisible
            ? Mathf.Min(Screen.height - (Margin * 2f), Mathf.Clamp(Screen.height * 0.46f, 390f, 470f))
            : Mathf.Clamp(Screen.height * 0.36f, DungeonBattleBarMinHeight, DungeonBattleBarMaxHeight);
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
            ? "Victory Summary"
            : _bootEntry.IsDungeonRouteChoiceVisible
                ? T("PanelDungeonRouteChoice")
                : _bootEntry.IsDungeonEventChoiceVisible
                    ? V(_bootEntry.EventTitleLabel)
                    : _bootEntry.IsDungeonPreEliteChoiceVisible
                        ? T("PanelDungeonPreElite")
                        : "Encounter";
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
            DrawScrollableOverlaySection("route_planner_preview", rightRect, BuildPanelBody(
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

            DrawNamedScrollableOverlaySection("victory_summary_panel", leftRect, "VictorySummaryPanel", BuildPanelBody(
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
            DrawNamedScrollableOverlaySection("victory_support_panel", rightRect, "BattleResultFeed", BuildPanelBody(
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
            DrawBattleHudShell(contentRect);
        }

        GUI.color = previousColor;
    }

    private void DrawBattleHudShell(Rect rect)
    {
        float layoutGap = OverlaySectionGap;
        float timelineHeight = 46f;
        float commandBarHeight = 50f;
        float messageHeight = 40f;
        float mainHeight = rect.height - timelineHeight - commandBarHeight - messageHeight - (layoutGap * 2f) - 6f;
        if (mainHeight < 150f)
        {
            timelineHeight = 42f;
            commandBarHeight = 46f;
            messageHeight = 36f;
            mainHeight = rect.height - timelineHeight - commandBarHeight - messageHeight - (layoutGap * 2f) - 6f;
        }

        Rect timelineRect = new Rect(rect.x, rect.y, rect.width, timelineHeight);
        Rect mainRect = new Rect(rect.x, timelineRect.yMax + layoutGap, rect.width, mainHeight);
        Rect commandBarRect = new Rect(rect.x, mainRect.yMax + layoutGap, rect.width, commandBarHeight);
        Rect messageRect = new Rect(rect.x, commandBarRect.yMax + 6f, rect.width, rect.yMax - commandBarRect.yMax - 6f);

        float leftWidth = (mainRect.width - (OverlaySectionGap * 2f)) * 0.31f;
        float centerWidth = (mainRect.width - (OverlaySectionGap * 2f)) * 0.37f;
        float rightWidth = mainRect.width - leftWidth - centerWidth - (OverlaySectionGap * 2f);
        Rect leftRect = new Rect(mainRect.x, mainRect.y, leftWidth, mainRect.height);
        Rect centerRect = new Rect(leftRect.xMax + OverlaySectionGap, mainRect.y, centerWidth, mainRect.height);
        Rect rightRect = new Rect(centerRect.xMax + OverlaySectionGap, mainRect.y, rightWidth, mainRect.height);

        DrawTurnOrderTimeline(timelineRect);
        DrawPartyStatusPanel(leftRect);
        DrawCommandFlyoutPanel(centerRect);
        DrawEnemyInfoPanel(rightRect);
        DrawBottomCommandBar(commandBarRect);
        DrawBattleMessageBox(messageRect);

        if (IsTargetSelectionActive())
        {
            DrawTargetSelectionOverlay(rect, mainRect);
        }
    }

    private void DrawTurnOrderTimeline(Rect rect)
    {
        DrawOverlaySectionBackground(rect, new Color(0.09f, 0.13f, 0.18f, 0.98f));
        Rect innerRect = new Rect(rect.x + SectionInnerPadding, rect.y + 8f, rect.width - (SectionInnerPadding * 2f), rect.height - 16f);
        float tokenGap = 8f;
        float currentWidth = Mathf.Clamp(innerRect.width * 0.28f, 150f, 220f);
        float phaseWidth = Mathf.Clamp(innerRect.width * 0.22f, 120f, 180f);
        float nextWidth = innerRect.width - currentWidth - phaseWidth - (tokenGap * 2f);
        Rect currentRect = new Rect(innerRect.x, innerRect.y, currentWidth, innerRect.height);
        Rect phaseRect = new Rect(currentRect.xMax + tokenGap, innerRect.y, phaseWidth, innerRect.height);
        Rect nextRect = new Rect(phaseRect.xMax + tokenGap, innerRect.y, nextWidth, innerRect.height);

        DrawTimelineToken(currentRect, "Current", GetBattleActorShortLabel(), new Color(0.19f, 0.31f, 0.46f, 0.96f));
        DrawTimelineToken(phaseRect, "Phase", GetBattlePhaseShortLabel(), new Color(0.15f, 0.23f, 0.34f, 0.96f));
        DrawTimelineToken(nextRect, "Next", GetNextBattleTimelineText(), new Color(0.12f, 0.18f, 0.27f, 0.96f));
    }

    private void DrawTimelineToken(Rect rect, string title, string body, Color backgroundColor)
    {
        DrawOverlaySectionBackground(rect, backgroundColor);
        Rect titleRect = new Rect(rect.x + 8f, rect.y + 4f, rect.width - 16f, 16f);
        Rect bodyRect = new Rect(rect.x + 8f, titleRect.yMax + 1f, rect.width - 16f, rect.height - 24f);
        GUI.Label(titleRect, title, _sectionTitleStyle);
        GUI.Label(bodyRect, body, _bodyStyle);
    }

    private void DrawPartyStatusPanel(Rect rect)
    {
        DrawOverlaySectionBackground(rect, new Color(0.09f, 0.13f, 0.17f, 0.97f));
        Rect titleRect = new Rect(rect.x + SectionInnerPadding, rect.y + 6f, rect.width - (SectionInnerPadding * 2f), 18f);
        GUI.Label(titleRect, "Party", _sectionTitleStyle);

        Rect contentRect = new Rect(rect.x + SectionInnerPadding, titleRect.yMax + 6f, rect.width - (SectionInnerPadding * 2f), rect.height - (titleRect.yMax - rect.y) - SectionInnerPadding);
        string[] memberLabels = new string[]
        {
            V(_bootEntry.Party1HpLabel),
            V(_bootEntry.Party2HpLabel),
            V(_bootEntry.Party3HpLabel),
            V(_bootEntry.Party4HpLabel)
        };

        float cardGap = 8f;
        float cardWidth = (contentRect.width - cardGap) * 0.5f;
        float cardHeight = (contentRect.height - cardGap) * 0.5f;
        for (int index = 0; index < memberLabels.Length; index++)
        {
            int row = index / 2;
            int column = index % 2;
            Rect cardRect = new Rect(
                contentRect.x + ((cardWidth + cardGap) * column),
                contentRect.y + ((cardHeight + cardGap) * row),
                cardWidth,
                cardHeight);
            DrawPartyMemberStatusCard(cardRect, memberLabels[index], index + 1);
        }
    }

    private void DrawPartyMemberStatusCard(Rect rect, string memberLabel, int slotIndex)
    {
        string memberName = GetBattleLabelToken(memberLabel, 0);
        if (!HasMeaningfulText(memberName))
        {
            memberName = "Member " + slotIndex;
        }

        string role = GetBattleLabelToken(memberLabel, 1);
        string hpValue = GetBattleLabelValue(memberLabel, "HP");
        string skillName = GetBattleLabelValue(memberLabel, "Skill");
        bool active = HasMeaningfulText(memberName) && ContainsIgnoreCase(V(_bootEntry.CurrentBattleActorLabel), memberName);
        bool targeted = ContainsIgnoreCase(memberLabel, "targeted");
        bool knockedOut = ContainsIgnoreCase(memberLabel, "ko");

        Color backgroundColor = knockedOut
            ? new Color(0.22f, 0.10f, 0.10f, 0.98f)
            : active
                ? new Color(0.14f, 0.24f, 0.34f, 0.98f)
                : targeted
                    ? new Color(0.19f, 0.16f, 0.12f, 0.98f)
                    : new Color(0.11f, 0.14f, 0.19f, 0.98f);
        DrawOverlaySectionBackground(rect, backgroundColor);

        Rect innerRect = new Rect(rect.x + 8f, rect.y + 8f, rect.width - 16f, rect.height - 16f);
        Rect nameRect = new Rect(innerRect.x, innerRect.y, innerRect.width, 18f);
        Rect roleRect = new Rect(innerRect.x, nameRect.yMax + 2f, innerRect.width, 16f);
        Rect hpRect = new Rect(innerRect.x, roleRect.yMax + 8f, innerRect.width, 18f);
        Rect skillRect = new Rect(innerRect.x, hpRect.yMax + 6f, innerRect.width, 16f);
        Rect statusRect = new Rect(innerRect.x, skillRect.yMax + 2f, innerRect.width, 16f);

        GUI.Label(nameRect, memberName, _sectionTitleStyle);
        GUI.Label(roleRect, HasMeaningfulText(role) ? role : "Adventurer", _bodyStyle);

        if (TryParseCurrentMaxPair(hpValue, out float hpCurrent, out float hpMax))
        {
            DrawStatusBar(hpRect, hpCurrent, hpMax, new Color(0.26f, 0.68f, 0.36f, 0.98f), "HP " + hpValue);
        }
        else
        {
            GUI.Label(hpRect, HasMeaningfulText(hpValue) ? "HP " + hpValue : "HP ?", _bodyStyle);
        }

        string compactSkill = HasMeaningfulText(skillName) ? GetCompactHudText(skillName, 24, false) : "Basic Action";
        GUI.Label(skillRect, compactSkill, _bodyStyle);
        GUI.Label(statusRect, BuildPartyCardStatusText(memberLabel, active, targeted, knockedOut), _bodyStyle);
    }

    private void DrawEnemyInfoPanel(Rect rect)
    {
        Color panelColor = IsTargetSelectionActive()
            ? new Color(0.17f, 0.14f, 0.10f, 0.97f)
            : new Color(0.10f, 0.11f, 0.15f, 0.97f);
        DrawOverlaySectionBackground(rect, panelColor);

        Rect titleRect = new Rect(rect.x + SectionInnerPadding, rect.y + 6f, rect.width - (SectionInnerPadding * 2f), 18f);
        GUI.Label(titleRect, IsTargetSelectionActive() ? "Target" : "Enemy", _sectionTitleStyle);

        Rect contentRect = new Rect(rect.x + SectionInnerPadding, titleRect.yMax + 6f, rect.width - (SectionInnerPadding * 2f), rect.height - (titleRect.yMax - rect.y) - SectionInnerPadding);
        float selectedHeight = Mathf.Clamp(contentRect.height * 0.55f, 96f, 126f);
        Rect selectedRect = new Rect(contentRect.x, contentRect.y, contentRect.width, selectedHeight);
        Rect rosterRect = new Rect(contentRect.x, selectedRect.yMax + 8f, contentRect.width, contentRect.height - selectedHeight - 8f);

        DrawSelectedEnemyCard(selectedRect);
        DrawEnemyRosterStrip(rosterRect);
    }

    private void DrawSelectedEnemyCard(Rect rect)
    {
        DrawOverlaySectionBackground(rect, new Color(0.12f, 0.14f, 0.19f, 0.98f));
        Rect innerRect = new Rect(rect.x + 8f, rect.y + 8f, rect.width - 16f, rect.height - 16f);
        Rect nameRect = new Rect(innerRect.x, innerRect.y, innerRect.width, 18f);
        Rect subtitleRect = new Rect(innerRect.x, nameRect.yMax + 2f, innerRect.width, 16f);
        Rect hpRect = new Rect(innerRect.x, subtitleRect.yMax + 8f, innerRect.width, 18f);
        Rect intentRect = new Rect(innerRect.x, hpRect.yMax + 6f, innerRect.width, 18f);
        Rect traitRect = new Rect(innerRect.x, intentRect.yMax + 2f, innerRect.width, 16f);

        string enemyName = GetCompactHudText(V(_bootEntry.BattleMonsterNameLabel), 28, false);
        if (!HasMeaningfulText(enemyName) || enemyName == "None")
        {
            enemyName = "No target selected";
        }

        string subtitle = GetCompactHudText(GetPreferredText(V(_bootEntry.EliteTypeLabel), V(_bootEntry.EncounterNameLabel)), 30, false);
        string enemyHp = GetCompactHudText(V(_bootEntry.BattleMonsterHpLabel), 20, false);
        string intent = GetCompactHudText(V(_bootEntry.EnemyIntentLabel), 34, false);
        string trait = GetCompactHudText(GetPreferredText(V(_bootEntry.EliteRewardHintLabel), V(_bootEntry.BattleMonster1Label)), 40, false);

        GUI.Label(nameRect, enemyName, _sectionTitleStyle);
        GUI.Label(subtitleRect, HasMeaningfulText(subtitle) && subtitle != "None" ? subtitle : "Frontline target", _bodyStyle);

        if (TryParseCurrentMaxPair(enemyHp, out float hpCurrent, out float hpMax))
        {
            DrawStatusBar(hpRect, hpCurrent, hpMax, new Color(0.72f, 0.28f, 0.26f, 0.98f), "HP " + enemyHp);
        }
        else
        {
            GUI.Label(hpRect, HasMeaningfulText(enemyHp) ? "HP " + enemyHp : "HP ?", _bodyStyle);
        }

        GUI.Label(intentRect, HasMeaningfulText(intent) && intent != "None" ? "Intent: " + intent : "Intent: Unknown", _bodyStyle);
        GUI.Label(traitRect, HasMeaningfulText(trait) && trait != "None" ? trait : "Traits pending", _bodyStyle);
    }

    private void DrawEnemyRosterStrip(Rect rect)
    {
        DrawOverlaySectionBackground(rect, new Color(0.11f, 0.13f, 0.18f, 0.98f));
        Rect titleRect = new Rect(rect.x + 8f, rect.y + 6f, rect.width - 16f, 18f);
        GUI.Label(titleRect, "Roster", _sectionTitleStyle);
        Rect bodyRect = new Rect(rect.x + 8f, titleRect.yMax + 4f, rect.width - 16f, rect.height - (titleRect.yMax - rect.y) - 10f);
        DrawScrollableTextRegion("battle_enemy_roster_strip", bodyRect, BuildEnemyRosterBody());
    }

    private void DrawCommandFlyoutPanel(Rect rect)
    {
        _battleHudHoverDetailKey = string.Empty;
        BattleHudFlyoutMode mode = GetDisplayedBattleFlyoutMode();
        DrawOverlaySectionBackground(rect, new Color(0.08f, 0.12f, 0.16f, 0.97f));

        Rect headerRect = new Rect(rect.x + SectionInnerPadding, rect.y + 6f, rect.width - (SectionInnerPadding * 2f), 18f);
        Rect actorRect = new Rect(headerRect.x, headerRect.y, headerRect.width * 0.62f, headerRect.height);
        Rect modeRect = new Rect(actorRect.xMax, headerRect.y, headerRect.width - actorRect.width, headerRect.height);
        GUI.Label(actorRect, "Commands", _sectionTitleStyle);
        GUI.Label(modeRect, GetBattleActorShortLabel(), _bodyStyle);

        Rect bodyRect = new Rect(rect.x + SectionInnerPadding, headerRect.yMax + 6f, rect.width - (SectionInnerPadding * 2f), rect.height - (headerRect.yMax - rect.y) - SectionInnerPadding);
        float menuWidth = Mathf.Clamp(bodyRect.width * 0.40f, 140f, 200f);
        Rect menuRect = new Rect(bodyRect.x, bodyRect.y, menuWidth, bodyRect.height);
        Rect detailRect = new Rect(menuRect.xMax + OverlaySectionGap, bodyRect.y, bodyRect.width - menuWidth - OverlaySectionGap, bodyRect.height);

        DrawCommandMenuPanel(menuRect, mode);
        DrawCommandDetailPanel(detailRect, mode);
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
        DrawOverlaySectionBackground(rect, new Color(0.10f, 0.13f, 0.18f, 0.98f));
        Rect titleRect = new Rect(rect.x + 8f, rect.y + 6f, rect.width - 16f, 18f);
        GUI.Label(titleRect, "Details", _sectionTitleStyle);
        Rect contentRect = new Rect(rect.x + 8f, titleRect.yMax + 4f, rect.width - 16f, rect.height - (titleRect.yMax - rect.y) - 10f);
        DrawScrollableTextRegion("battle_command_detail_panel", contentRect, BuildCommandDetailBody(mode));
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
        float menuButtonHeight = 38f;
        float menuButtonGap = 8f;
        float totalHeight = (menuButtonHeight * 3f) + (menuButtonGap * 2f);
        float startY = rect.y + Mathf.Max(0f, (rect.height - totalHeight) * 0.5f);
        Rect attackRect = new Rect(rect.x, startY, rect.width, menuButtonHeight);
        Rect skillRect = new Rect(rect.x, attackRect.yMax + menuButtonGap, rect.width, menuButtonHeight);
        Rect itemRect = new Rect(rect.x, skillRect.yMax + menuButtonGap, rect.width, menuButtonHeight);

        Event current = Event.current;
        Vector2 mousePosition = current != null ? current.mousePosition : Vector2.zero;
        string hoveredActionKey = string.Empty;

        if (DrawBattleActionMenuButton(attackRect, "attack", "[1] Attack", _bootEntry.IsBattleActionAvailable("attack"), _bootEntry.IsBattleActionSelected("attack"), mousePosition, ref hoveredActionKey))
        {
            _bootEntry.TryTriggerBattleAction("attack");
            _battleFlyoutMode = BattleHudFlyoutMode.ActorCommandMenu;
        }

        if (DrawBattleActionMenuButton(skillRect, "skill", "[2] Skill", _bootEntry.IsBattleActionAvailable("skill"), _bootEntry.IsBattleActionSelected("skill") || _battleFlyoutMode == BattleHudFlyoutMode.SkillListPanel, mousePosition, ref hoveredActionKey))
        {
            _battleFlyoutMode = BattleHudFlyoutMode.SkillListPanel;
        }

        bool itemHovered = itemRect.Contains(mousePosition);
        if (itemHovered)
        {
            _battleHudHoverDetailKey = "item";
        }

        if (DrawBottomCommandButton(itemRect, "Item", _bootEntry.IsDungeonBattleViewActive, itemHovered, _battleFlyoutMode == BattleHudFlyoutMode.ItemListPanel))
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
        float menuButtonHeight = 38f;
        float menuButtonGap = 8f;
        float totalHeight = (menuButtonHeight * 2f) + menuButtonGap;
        float startY = rect.y + Mathf.Max(0f, (rect.height - totalHeight) * 0.5f);
        Rect useRect = new Rect(rect.x, startY, rect.width, menuButtonHeight);
        Rect backRect = new Rect(rect.x, useRect.yMax + menuButtonGap, rect.width, menuButtonHeight);

        Event current = Event.current;
        Vector2 mousePosition = current != null ? current.mousePosition : Vector2.zero;
        string hoveredActionKey = string.Empty;

        if (DrawBattleActionMenuButton(useRect, "skill", "Use Skill", _bootEntry.IsBattleActionAvailable("skill"), _bootEntry.IsBattleActionSelected("skill"), mousePosition, ref hoveredActionKey))
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
        float menuButtonHeight = 38f;
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
        float menuButtonHeight = 38f;
        float menuButtonGap = 8f;
        float totalHeight = (menuButtonHeight * 2f) + menuButtonGap;
        float startY = rect.y + Mathf.Max(0f, (rect.height - totalHeight) * 0.5f);
        Rect retreatRect = new Rect(rect.x, startY, rect.width, menuButtonHeight);
        Rect backRect = new Rect(rect.x, retreatRect.yMax + menuButtonGap, rect.width, menuButtonHeight);

        Event current = Event.current;
        Vector2 mousePosition = current != null ? current.mousePosition : Vector2.zero;
        string hoveredActionKey = string.Empty;

        if (DrawBattleActionMenuButton(retreatRect, "retreat", "[3] Retreat", _bootEntry.IsBattleActionAvailable("retreat"), false, mousePosition, ref hoveredActionKey))
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
        float menuButtonHeight = 38f;
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

        if (DrawBottomCommandButton(cancelRect, "Cancel", true, cancelHovered, false))
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
        DrawOverlaySectionBackground(rect, new Color(0.08f, 0.11f, 0.15f, 0.98f));
        Rect innerRect = new Rect(rect.x + SectionInnerPadding, rect.y + 5f, rect.width - (SectionInnerPadding * 2f), rect.height - 10f);
        float buttonWidth = Mathf.Clamp(innerRect.width * 0.16f, 112f, 152f);
        float infoWidth = innerRect.width - (buttonWidth * 2f) - (OverlaySectionGap * 2f);
        Rect actorRect = new Rect(innerRect.x, innerRect.y, buttonWidth, innerRect.height);
        Rect partyRect = new Rect(actorRect.xMax + OverlaySectionGap, innerRect.y, buttonWidth, innerRect.height);
        Rect infoRect = new Rect(partyRect.xMax + OverlaySectionGap, innerRect.y, infoWidth, innerRect.height);

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

        DrawOverlaySectionBackground(infoRect, new Color(0.10f, 0.14f, 0.19f, 0.98f));
        GUI.Label(new Rect(infoRect.x + 8f, infoRect.y + 3f, infoRect.width - 16f, infoRect.height - 6f), BuildBottomCommandBarBody(), _bodyStyle);
    }

    private void DrawBattleMessageBox(Rect rect)
    {
        DrawOverlaySectionBackground(rect, new Color(0.08f, 0.10f, 0.13f, 0.98f));
        Rect promptRect = new Rect(rect.x + 10f, rect.y + 6f, rect.width * 0.72f, rect.height - 12f);
        Rect hintRect = new Rect(promptRect.xMax + 8f, rect.y + 6f, rect.width - (promptRect.width + 18f), rect.height - 12f);
        GUI.Label(promptRect, GetBattlePromptMessage(), _bodyStyle);
        GUI.Label(hintRect, GetBattleCancelHintText(), _bodyStyle);
    }

    private void DrawTargetSelectionOverlay(Rect hudRect, Rect mainRect)
    {
        float overlayWidth = Mathf.Min(420f, mainRect.width * 0.62f);
        float overlayHeight = 64f;
        float overlayX = mainRect.x + ((mainRect.width - overlayWidth) * 0.5f);
        float overlayY = Mathf.Max(Margin + 64f, hudRect.y - overlayHeight - 10f);
        Rect overlayRect = new Rect(overlayX, overlayY, overlayWidth, overlayHeight);
        DrawOverlaySectionBackground(overlayRect, new Color(0.24f, 0.18f, 0.09f, 0.98f));

        Rect titleRect = new Rect(overlayRect.x + 10f, overlayRect.y + 6f, overlayRect.width - 20f, 18f);
        Rect bodyRect = new Rect(overlayRect.x + 10f, titleRect.yMax + 2f, overlayRect.width - 20f, overlayRect.height - (titleRect.yMax - overlayRect.y) - 10f);
        string targetName = GetCompactHudText(V(_bootEntry.BattleMonsterNameLabel), 28, false);
        string queuedAction = GetSelectedBattleActionLabel();
        string body = queuedAction + " -> " + (HasMeaningfulText(targetName) && targetName != "None" ? targetName : "Choose a target") + "    " + GetBattleCancelHintText();
        GUI.Label(titleRect, "Select target", _sectionTitleStyle);
        GUI.Label(bodyRect, body, _bodyStyle);
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
        AddOptionalSummaryLine(lines, "Encounter", GetCompactHudText(V(_bootEntry.EncounterNameLabel), 34, false));
        AddOptionalSummaryLine(lines, "Enemy 1", GetCompactHudText(V(_bootEntry.BattleMonster1Label), 56, false));
        AddOptionalSummaryLine(lines, "Enemy 2", GetCompactHudText(V(_bootEntry.BattleMonster2Label), 56, false));
        AddOptionalSummaryLine(lines, "Elite", GetCompactHudText(V(_bootEntry.EliteTypeLabel), 40, false));
        if (lines.Count == 0)
        {
            lines.Add("Enemy roster pending.");
        }

        return BuildPanelBody(lines.ToArray());
    }

    private string BuildCommandDetailBody(BattleHudFlyoutMode mode)
    {
        string detailKey = GetCommandDetailKey(mode);
        string actorName = GetBattleActorShortLabel();
        string actorRole = GetCurrentActorRoleLabel();
        string currentSkillName = GetCurrentActorSkillName();
        List<string> lines = new List<string>();

        if (detailKey == "skill")
        {
            lines.Add(HasMeaningfulText(currentSkillName) && currentSkillName != "None" ? currentSkillName : "Skill");
            lines.Add("Target: Single enemy");
            lines.Add("Cost: Placeholder cost");
            lines.Add("Effect: Uses the active actor's default skill identity.");
        }
        else if (detailKey == "item")
        {
            lines.Add("Item");
            lines.Add("Target: Self / ally");
            lines.Add("Cost: Not available yet");
            lines.Add("Effect: Reserved for the later inventory batch.");
        }
        else if (detailKey == "retreat")
        {
            lines.Add("Retreat");
            lines.Add("Target: Party");
            lines.Add("Cost: Ends the current run early");
            lines.Add("Effect: Return to WorldSim with current resolution flow.");
        }
        else if (detailKey == "retreat_confirm")
        {
            lines.Add("Confirm retreat");
            lines.Add("Target: Party");
            lines.Add("Cost: Commit to leaving the encounter");
            lines.Add("Effect: Uses the current retreat resolution.");
        }
        else if (detailKey == "cancel")
        {
            lines.Add("Cancel");
            lines.Add("Target: Current command layer");
            lines.Add("Cost: None");
            lines.Add("Effect: Return to party commands without confirming.");
        }
        else if (detailKey == "back")
        {
            lines.Add("Back");
            lines.Add("Target: Previous menu");
            lines.Add("Cost: None");
            lines.Add("Effect: Return to the previous command view.");
        }
        else
        {
            lines.Add("Attack");
            lines.Add("Target: Single enemy");
            lines.Add("Cost: None");
            lines.Add("Effect: Reliable basic strike.");
        }

        lines.Add(string.Empty);
        lines.Add("Actor: " + actorName + (HasMeaningfulText(actorRole) && actorRole != "None" ? " (" + actorRole + ")" : string.Empty));
        lines.Add("Prompt: " + GetBattlePromptMessage());
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
        return GetBattleActorShortLabel() + " | " + GetBattlePhaseShortLabel() + "\nMode: " + modeLabel + "   Confirm: Click / [1][2][3]   Cancel: " + GetBattleCancelHintText();
    }

    private string GetBattlePromptMessage()
    {
        string prompt = GetCompactHudText(V(_bootEntry.CurrentSelectionPromptLabel), 132, false);
        if (HasMeaningfulText(prompt) && prompt != "None")
        {
            return prompt;
        }

        string feedback = GetCompactHudText(V(_bootEntry.BattleFeedbackLabel), 132, false);
        if (HasMeaningfulText(feedback) && feedback != "None")
        {
            return feedback;
        }

        return "Select an action.";
    }

    private string GetBattleCancelHintText()
    {
        string cancelHint = GetCompactHudText(V(_bootEntry.BattleCancelHintLabel), 56, false);
        return HasMeaningfulText(cancelHint) && cancelHint != "None"
            ? cancelHint
            : "Esc: Cancel";
    }

    private string GetBattleActorShortLabel()
    {
        string actorName = GetBattleLabelToken(V(_bootEntry.CurrentBattleActorLabel), 0);
        if (HasMeaningfulText(actorName))
        {
            return actorName;
        }

        return GetCompactHudText(V(_bootEntry.CurrentBattleActorLabel), 34, true);
    }

    private string GetCurrentActorRoleLabel()
    {
        return GetBattleLabelToken(V(_bootEntry.CurrentBattleActorLabel), 1);
    }

    private string GetCurrentActorSkillName()
    {
        return GetBattleLabelValue(V(_bootEntry.CurrentBattleActorLabel), "Skill");
    }

    private string GetBattlePhaseShortLabel()
    {
        return GetCompactHudText(V(_bootEntry.BattlePhaseLabel), 24, false);
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
        GUI.color = new Color(0.06f, 0.08f, 0.11f, 0.98f);
        GUI.Box(rect, GUIContent.none, _panelStyle);

        float fillRatio = Mathf.Clamp01(current / max);
        if (fillRatio > 0f)
        {
            Rect fillRect = new Rect(rect.x + 1f, rect.y + 1f, (rect.width - 2f) * fillRatio, rect.height - 2f);
            GUI.color = fillColor;
            GUI.DrawTexture(fillRect, Texture2D.whiteTexture);
        }

        GUI.color = Color.white;
        GUI.Label(new Rect(rect.x + 6f, rect.y + 1f, rect.width - 12f, rect.height - 2f), label, _bodyStyle);
        GUI.color = previousColor;
    }

    private string GetNextBattleTimelineText()
    {
        string phase = V(_bootEntry.BattlePhaseLabel);
        if (IsTargetSelectionActive())
        {
            return "Choose target -> Enemy turn";
        }

        if (ContainsIgnoreCase(phase, "enemy"))
        {
            return "Enemy action resolving";
        }

        if (ContainsIgnoreCase(phase, "victory"))
        {
            return "Return to summary";
        }

        if (ContainsIgnoreCase(phase, "defeat") || ContainsIgnoreCase(phase, "retreat"))
        {
            return "Resolve battle result";
        }

        return "Open commands -> choose action";
    }

    private string GetSelectedBattleActionLabel()
    {
        string actionKey = GetSelectedBattleActionKey();
        return actionKey == "attack"
            ? "Attack"
            : actionKey == "skill"
                ? "Skill"
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
        return _bootEntry != null && (
            _bootEntry.IsBattleActionSelected("attack") ||
            _bootEntry.IsBattleActionSelected("skill") ||
            _bootEntry.IsBattleActionSelected("retreat") ||
            ContainsIgnoreCase(V(_bootEntry.BattlePhaseLabel), "target") ||
            HasMeaningfulText(V(_bootEntry.BattleCancelHintLabel)));
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

        _sectionTitleStyle = new GUIStyle(_bodyStyle);
        _sectionTitleStyle.fontSize = 12;
        _sectionTitleStyle.fontStyle = FontStyle.Bold;
        _sectionTitleStyle.normal.textColor = Color.white;

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
