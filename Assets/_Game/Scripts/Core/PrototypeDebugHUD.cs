using System;
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
    private const KeyCode DebugHudToggleKey = KeyCode.F1;

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
        public HudFilter Group { get; private set; }
        private readonly Func<string> _bodyBuilder;
        private string _body;
        private bool _hasBuiltBody;

        public HudPanel(string key, string title, string body, HudFilter group)
            : this(key, title, () => body, group)
        {
        }

        public HudPanel(string key, string title, Func<string> bodyBuilder, HudFilter group)
        {
            Key = string.IsNullOrEmpty(key) ? "panel" : key;
            Title = string.IsNullOrEmpty(title) ? "Panel" : title;
            _bodyBuilder = bodyBuilder;
            Group = group;
        }

        public string Body
        {
            get
            {
                if (_hasBuiltBody)
                {
                    return _body;
                }

                string builtBody = _bodyBuilder != null ? _bodyBuilder.Invoke() : null;
                _body = string.IsNullOrEmpty(builtBody) ? "None" : builtBody;
                _hasBuiltBody = true;
                return _body;
            }
        }
    }

    private struct HudStyleVariantKey : IEquatable<HudStyleVariantKey>
    {
        public readonly int BaseStyleId;
        public readonly int FontSize;
        public readonly bool WordWrap;

        public HudStyleVariantKey(int baseStyleId, int fontSize, bool wordWrap)
        {
            BaseStyleId = baseStyleId;
            FontSize = fontSize;
            WordWrap = wordWrap;
        }

        public bool Equals(HudStyleVariantKey other)
        {
            return BaseStyleId == other.BaseStyleId &&
                   FontSize == other.FontSize &&
                   WordWrap == other.WordWrap;
        }

        public override bool Equals(object obj)
        {
            return obj is HudStyleVariantKey other && Equals(other);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                int hash = BaseStyleId;
                hash = (hash * 397) ^ FontSize;
                hash = (hash * 397) ^ (WordWrap ? 1 : 0);
                return hash;
            }
        }
    }

    private struct HudMeasureKey : IEquatable<HudMeasureKey>
    {
        public readonly int BaseStyleId;
        public readonly int FontSize;
        public readonly int WidthBucket;
        public readonly string Text;

        public HudMeasureKey(int baseStyleId, int fontSize, int widthBucket, string text)
        {
            BaseStyleId = baseStyleId;
            FontSize = fontSize;
            WidthBucket = widthBucket;
            Text = text ?? string.Empty;
        }

        public bool Equals(HudMeasureKey other)
        {
            return BaseStyleId == other.BaseStyleId &&
                   FontSize == other.FontSize &&
                   WidthBucket == other.WidthBucket &&
                   string.Equals(Text, other.Text, StringComparison.Ordinal);
        }

        public override bool Equals(object obj)
        {
            return obj is HudMeasureKey other && Equals(other);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                int hash = BaseStyleId;
                hash = (hash * 397) ^ FontSize;
                hash = (hash * 397) ^ WidthBucket;
                hash = (hash * 397) ^ (Text != null ? Text.GetHashCode() : 0);
                return hash;
            }
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
    [SerializeField] private bool _startDebugHudVisible;
    private bool _debugHudVisible;
    private BattleHudFlyoutMode _battleFlyoutMode = BattleHudFlyoutMode.None;
    private string _pendingConfirmActionKey = string.Empty;
    private string _selectedBattleCommandKey = string.Empty;
    private string _selectedBattleCommandOwnerKey = string.Empty;
    private string _battleHudHoverDetailKey = string.Empty;
    private bool _isSearchFieldFocused;
    private PrototypeBattleUiSurfaceData _battleUiSurface = new PrototypeBattleUiSurfaceData();
    private readonly Dictionary<HudStyleVariantKey, GUIStyle> _hudStyleVariants = new Dictionary<HudStyleVariantKey, GUIStyle>();
    private readonly Dictionary<HudMeasureKey, float> _hudMeasureHeights = new Dictionary<HudMeasureKey, float>();
    private readonly GUIContent _sharedHudContent = new GUIContent();
    private readonly PrototypeBattleUiCommandDetailData[] _battleMoveOptionBuffer = new PrototypeBattleUiCommandDetailData[3];
    private int _battleMoveOptionCount;
    private int _battleUiSurfaceFrame = -1;
    private EventType _battleUiSurfaceEventType = EventType.Ignore;
    private int _dockPanelsFrame = -1;
    private List<HudPanel> _dockPanels;
    private string _lastBattleActionHoverKey = string.Empty;
    private bool _battleHudFastBackground;
    public bool IsSearchFieldFocused => _isSearchFieldFocused;
    public bool ShouldBlockDungeonInput => IsBattleInputModalOpen();

    private void Awake()
    {
        CacheBootEntry();
        _debugHudVisible = _startDebugHudVisible;
        EnsureExpandedState();
    }

    private void OnGUI()
    {
        CacheBootEntry();
        if (_bootEntry == null)
        {
            _isSearchFieldFocused = false;
            ResetDockPanelCache();
            ResetBattleUiSurfaceCache();
            return;
        }

        Event current = Event.current;
        EventType currentEventType = current != null ? current.type : EventType.Repaint;
        HandleDebugHudToggle(current);

        bool battleHudMode = _bootEntry.IsDungeonBattleViewActive && !_bootEntry.IsDungeonResultPanelVisible;
        bool playerFacingDebugHudMode = battleHudMode ||
                                        _bootEntry.IsLegacyDungeonRouteChoiceVisible ||
                                        _bootEntry.IsDungeonRunEventDecisionVisible ||
                                        _bootEntry.IsDungeonRunPreEliteDecisionVisible ||
                                        _bootEntry.IsDungeonResultPanelVisible;
        if (!_debugHudVisible && !playerFacingDebugHudMode)
        {
            _isSearchFieldFocused = false;
            ResetDockPanelCache();
            ResetBattleHudState();
            ResetBattleUiSurfaceCache();
            return;
        }

        EnsureExpandedState();
        if (battleHudMode)
        {
            _isSearchFieldFocused = false;
            EnsureStyles();
            if (!IsBattleHudUsefulEvent(currentEventType))
            {
                return;
            }

            RefreshBattleUiSurface(currentEventType);
            SyncBattleHudState();
            DrawBattleHudShell(new Rect(0f, 0f, Screen.width, Screen.height));
            return;
        }

        if (_bootEntry.IsDungeonRunHudMode)
        {
            _isSearchFieldFocused = false;
            ResetDockPanelCache();
            ResetBattleHudState();
            ResetBattleUiSurfaceCache();
            return;
        }

        EnsureStyles();
        bool overlayMode = battleHudMode || _bootEntry.IsLegacyDungeonRouteChoiceVisible || _bootEntry.IsDungeonRunEventDecisionVisible || _bootEntry.IsDungeonRunPreEliteDecisionVisible || _bootEntry.IsDungeonResultPanelVisible;
        bool frontendDockMode = _bootEntry.IsMainMenuActive || _bootEntry.IsWorldSimActive;
        List<HudPanel> panels = overlayMode ? null : GetDockPanels();

        float panelWidth = overlayMode
            ? Mathf.Clamp(Screen.width * 0.18f, 276f, 308f)
            : frontendDockMode
                ? Mathf.Clamp(Screen.width * 0.18f, 300f, 360f)
                : Mathf.Clamp(Screen.width * 0.34f, PanelWidthMin, PanelWidthMax);
        panelWidth = Mathf.Min(panelWidth, Screen.width - (Margin * 2f));

        float panelHeight = overlayMode
            ? Mathf.Clamp(Screen.height * 0.18f, 148f, 188f)
            : frontendDockMode
                ? Mathf.Clamp(Screen.height * 0.44f, 300f, 420f)
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
            DrawBattleHudShell(new Rect(0f, 0f, Screen.width, Screen.height));
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
            : _bootEntry.IsLegacyDungeonRouteChoiceVisible
                ? "Legacy Dispatch Fallback"
                : _bootEntry.IsDungeonRunEventDecisionVisible
                    ? "Dungeon Event Decision"
                    : "Dungeon Elite Decision";
        GUI.Label(new Rect(titleRect.x + PanelPadding, titleRect.y + 4f, titleRect.width - (PanelPadding * 2f), titleRect.height), overlayTitle, _titleStyle);
        Rect contentRectLegacy = new Rect(rectLegacy.x + PanelPadding, rectLegacy.y + HeaderHeight + 8f, rectLegacy.width - (PanelPadding * 2f), rectLegacy.height - HeaderHeight - 16f);
        if (_bootEntry.IsLegacyDungeonRouteChoiceVisible)
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
                Line("NormalLaunchOwner", "ExpeditionPrep confirm seam"),
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
                Line("FallbackScope", "Legacy fallback only"),
                V(_bootEntry.LegacyDungeonRouteChoiceDescriptionLabel),
                V(_bootEntry.LegacyDungeonRouteChoicePromptLabel)), new Color(0.08f, 0.12f, 0.16f, 0.94f));
            DrawRouteChoiceSection(centerRect);
            DrawScrollableOverlaySection("route_planner_detail", rightRect, BuildPanelBody(
                Line("RouteOption1", V(_bootEntry.LegacyDungeonRouteOption1Label)),
                Line("RouteOption2", V(_bootEntry.LegacyDungeonRouteOption2Label)),
                Line("RecommendedRoute", V(_bootEntry.RecommendedRouteRunLabel)),
                Line("CurrentRoute", V(_bootEntry.CurrentRouteLabel)),
                Line("RouteRisk", V(_bootEntry.RouteRiskLabel)),
                Line("SelectedPreview1", V(_bootEntry.SelectedRoutePreview1Label)),
                Line("SelectedPreview2", V(_bootEntry.SelectedRoutePreview2Label)),
                Line("SelectedRecommendedRoute", V(_bootEntry.SelectedRecommendedRouteSummaryLabel)),
                Line("SelectedRecommendedRouteForCity", V(_bootEntry.SelectedRecommendedRouteForLinkedCityLabel)),
                V(_bootEntry.LegacyDungeonRouteChoiceDescriptionLabel)), new Color(0.08f, 0.12f, 0.16f, 0.94f));
        }
        else if (_bootEntry.IsDungeonRunEventDecisionVisible)
        {
            float leftWidth = (contentRectLegacy.width - OverlaySectionGap) * 0.46f;
            Rect leftRect = new Rect(contentRectLegacy.x, contentRectLegacy.y, leftWidth, contentRectLegacy.height);
            Rect rightRect = new Rect(leftRect.xMax + OverlaySectionGap, contentRectLegacy.y, contentRectLegacy.width - leftWidth - OverlaySectionGap, contentRectLegacy.height);
            DrawNamedScrollableOverlaySection("event_choice_info", leftRect, "Dungeon Event Decision", BuildPanelBody(
                Line("DecisionScope", "DungeonRun internal event decision"),
                Line("EventPrompt", V(_bootEntry.DungeonRunEventDecisionPromptLabel)),
                Line("EventStatus", V(_bootEntry.EventStatusLabel)),
                Line("EventChoice", V(_bootEntry.DungeonRunEventDecisionLabel)),
                Line("OptionA", V(_bootEntry.DungeonRunEventDecisionOptionALabel)),
                Line("OptionB", V(_bootEntry.DungeonRunEventDecisionOptionBLabel))), new Color(0.08f, 0.12f, 0.16f, 0.94f));
            DrawEventChoiceSection(rightRect);
        }
        else if (_bootEntry.IsDungeonRunPreEliteDecisionVisible)
        {
            float leftWidth = (contentRectLegacy.width - OverlaySectionGap) * 0.46f;
            Rect leftRect = new Rect(contentRectLegacy.x, contentRectLegacy.y, leftWidth, contentRectLegacy.height);
            Rect rightRect = new Rect(leftRect.xMax + OverlaySectionGap, contentRectLegacy.y, contentRectLegacy.width - leftWidth - OverlaySectionGap, contentRectLegacy.height);
            DrawNamedScrollableOverlaySection("pre_elite_info", leftRect, "Dungeon Elite Decision", BuildPanelBody(
                Line("DecisionScope", "DungeonRun internal elite decision"),
                Line("EliteEncounter", V(_bootEntry.EliteEncounterNameLabel)),
                Line("EliteType", V(_bootEntry.EliteTypeLabel)),
                Line("EliteHp", V(_bootEntry.EliteHpLabel)),
                Line("EliteDefeated", V(_bootEntry.EliteDefeatedLabel)),
                Line("EliteRewardStatus", V(_bootEntry.EliteRewardStatusLabel)),
                Line("EliteRewardHint", V(_bootEntry.EliteRewardHintLabel)),
                Line("PreElitePrompt", V(_bootEntry.DungeonRunPreEliteDecisionPromptLabel)),
                Line("PreEliteChoice", V(_bootEntry.DungeonRunPreEliteDecisionLabel)),
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
        bool previousFastBackground = _battleHudFastBackground;
        _battleHudFastBackground = true;
        bool battleModalOpen = IsBattleInputModalOpen();
        if (battleModalOpen)
        {
            DismissBattleCommandOverlayState();
        }

        float screenLeft = Margin;
        float screenWidth = Screen.width - (Margin * 2f);
        float topHeight = Mathf.Clamp(Screen.height * 0.085f, 80f, 92f);
        Rect topRect = new Rect(screenLeft, Margin, screenWidth, topHeight);
        float statusPanelHeight = Mathf.Clamp(Screen.height * 0.16f, 150f, 186f);
        float statusY = Screen.height - Margin - statusPanelHeight;
        float currentUnitWidth = Mathf.Clamp(screenWidth * 0.30f, 340f, 540f);
        float rightWidth = Mathf.Clamp(screenWidth * 0.24f, 300f, 420f);
        Rect currentUnitRect = new Rect(screenLeft + 8f, statusY, currentUnitWidth, statusPanelHeight);
        Rect rightRect = new Rect(screenLeft + screenWidth - rightWidth - 8f, statusY, rightWidth, statusPanelHeight);
        Rect statusRowRect = new Rect(screenLeft, statusY, screenWidth, statusPanelHeight);

        float commandWidth = Mathf.Clamp(screenWidth * 0.15f, 228f, 286f);
        float commandTop = topRect.yMax + 24f;
        float availableCommandHeight = currentUnitRect.y - commandTop - 18f;
        float maxPanelHeight = Mathf.Max(132f, availableCommandHeight);
        float commandHeight = Mathf.Min(GetPreferredBattleCommandSelectionHeight(), maxPanelHeight);
        float flyoutWidth = 0f;
        if (!battleModalOpen && ShouldShowBattleCommandFlyout())
        {
            float maxFlyoutWidth = Mathf.Max(280f, rightRect.x - (screenLeft + 8f + commandWidth) - 28f);
            flyoutWidth = Mathf.Min(Mathf.Clamp(screenWidth * 0.28f, 360f, 520f), maxFlyoutWidth);
            float sharedPanelHeight = Mathf.Max(GetPreferredBattleCommandSelectionHeight(), GetPreferredBattleCommandFlyoutHeight(flyoutWidth));
            commandHeight = Mathf.Min(sharedPanelHeight, maxPanelHeight);
        }

        Rect commandRect = new Rect(screenLeft + 8f, commandTop, commandWidth, commandHeight);

        DrawBattleTopStrip(topRect);
        if (!battleModalOpen)
        {
            DrawCurrentUnitPanel(currentUnitRect);
            DrawCommandSelectionPanel(commandRect);
        }

        DrawTargetStatusPanel(rightRect);
        if (!battleModalOpen && ShouldShowBattleCommandFlyout())
        {
            Rect flyoutRect = new Rect(commandRect.xMax + 16f, commandRect.y, flyoutWidth, commandHeight);
            DrawCommandFlyoutPanel(flyoutRect);
        }

        if (IsTargetSelectionActive())
        {
            DrawTargetSelectionOverlay(topRect, statusRowRect);
        }

        _battleHudFastBackground = previousFastBackground;
    }

    private void DrawCurrentUnitPanel(Rect rect)
    {
        DrawOverlaySectionBackground(rect, new Color(0.08f, 0.11f, 0.16f, 0.98f));
        Rect titleRect = new Rect(rect.x + SectionInnerPadding, rect.y + 8f, rect.width - (SectionInnerPadding * 2f), 18f);
        Rect subtitleRect = new Rect(titleRect.x, titleRect.yMax + 3f, titleRect.width, 16f);
        Rect contentRect = new Rect(rect.x + SectionInnerPadding, subtitleRect.yMax + 8f, rect.width - (SectionInnerPadding * 2f), rect.height - (subtitleRect.yMax - rect.y) - 12f);

        DrawFittedLabel(titleRect, "Current Unit", _sectionTitleStyle, 11, 10, false);
        DrawFittedLabel(subtitleRect, GetCompactHudText(GetNextBattleTimelineText(), 60, false), _bodyStyle, 10, 9, false);

        PrototypeBattleUiPartyMemberData memberData = GetCurrentBattlePartyMember();
        CurrentActorSurfaceData actorSurface = GetBattleUiSurfaceData().CurrentActorSurface;
        bool hasActorSurface = actorSurface != null && HasMeaningfulText(actorSurface.DisplayName);
        if (memberData == null && !hasActorSurface)
        {
            DrawFittedLabel(contentRect, "Awaiting playable turn.", _bodyStyle, 10, 9, false);
            return;
        }

        DrawCurrentBattleActorCard(contentRect, memberData, actorSurface, ResolveBattlePartyMemberSlot(memberData));
    }

    private void DrawTargetStatusPanel(Rect rect)
    {
        DrawOverlaySectionBackground(rect, new Color(0.08f, 0.12f, 0.10f, 0.98f));
        Rect titleRect = new Rect(rect.x + SectionInnerPadding, rect.y + 8f, rect.width - (SectionInnerPadding * 2f), 18f);
        Rect subtitleRect = new Rect(titleRect.x, titleRect.yMax + 3f, titleRect.width, 16f);
        Rect contentRect = new Rect(rect.x + SectionInnerPadding, subtitleRect.yMax + 8f, rect.width - (SectionInnerPadding * 2f), rect.height - (subtitleRect.yMax - rect.y) - 12f);

        DrawFittedLabel(titleRect, "Target Status", _sectionTitleStyle, 11, 10, false);
        DrawFittedLabel(subtitleRect, GetCompactHudText(GetBattleUiSurfaceData().EncounterName, 46, false), _bodyStyle, 10, 9, false);

        PrototypeBattleUiEnemyData enemyData = GetBattleUiSurfaceData().SelectedEnemy;
        if (enemyData == null || !HasMeaningfulText(enemyData.DisplayName))
        {
            string fallback = IsTargetSelectionActive()
                ? "Hover or select an enemy to inspect it."
                : "No focused enemy.";
            DrawFittedLabel(contentRect, fallback, _bodyStyle, 10, 9, true);
            return;
        }

        DrawSelectedEnemyCard(contentRect);
    }

    private void DrawCurrentTurnHeaderCard(Rect rect)
    {
        DrawOverlaySectionBackground(rect, new Color(0.11f, 0.17f, 0.24f, 0.90f));
        Rect titleRect = new Rect(rect.x + 10f, rect.y + 6f, rect.width - 20f, 12f);
        Rect nameRect = new Rect(rect.x + 10f, titleRect.yMax + 5f, rect.width - 20f, 16f);
        Rect roleRect = new Rect(rect.x + 10f, nameRect.yMax + 3f, rect.width - 20f, rect.height - (nameRect.yMax - rect.y) - 8f);
        DrawFittedLabel(titleRect, "Current", _sectionTitleStyle, 9, 8, false);
        DrawFittedLabel(nameRect, GetBattleActorShortLabel(), _sectionTitleStyle, 11, 9, false);
        DrawFittedLabel(roleRect, GetCompactHudText(GetCurrentActorRoleLabel(), 18, false), _bodyStyle, 9, 8, false);
    }

    private void DrawBattleTopStrip(Rect rect)
    {
        DrawOverlaySectionBackground(rect, new Color(0.05f, 0.09f, 0.14f, 0.84f));
        float summaryWidth = Mathf.Clamp(rect.width * 0.24f, 280f, 420f);
        float currentWidth = Mathf.Clamp(rect.width * 0.10f, 136f, 176f);
        float phaseWidth = Mathf.Clamp(rect.width * 0.07f, 96f, 116f);
        Rect summaryRect = new Rect(rect.x, rect.y, summaryWidth, rect.height);
        Rect currentRect = new Rect(summaryRect.xMax + 8f, rect.y, currentWidth, rect.height);
        Rect phaseRect = new Rect(currentRect.xMax + 8f, rect.y, phaseWidth, rect.height);
        Rect timelineRect = new Rect(phaseRect.xMax + 8f, rect.y, rect.width - summaryWidth - currentWidth - phaseWidth - 24f, rect.height);

        DrawBattleDungeonSummaryCard(summaryRect);
        DrawCurrentTurnHeaderCard(currentRect);
        DrawTimelinePhaseToken(phaseRect, GetBattleUiSurfaceData().Timeline);
        DrawTurnOrderTimeline(timelineRect);
    }

    private void DrawBattleDungeonSummaryCard(Rect rect)
    {
        DrawOverlaySectionBackground(rect, new Color(0.09f, 0.14f, 0.20f, 0.90f));
        Rect titleRect = new Rect(rect.x + 14f, rect.y + 8f, rect.width - 28f, 18f);
        Rect typeRect = new Rect(rect.x + 14f, titleRect.yMax + 4f, rect.width - 28f, 15f);
        Rect progressRect = new Rect(rect.x + 14f, typeRect.yMax + 4f, rect.width - 28f, rect.height - (typeRect.yMax - rect.y) - 10f);
        DrawFittedLabel(titleRect, GetBattleDungeonNameLabel(), _titleStyle, 14, 10, false);
        DrawFittedLabel(typeRect, GetBattleDungeonTypeLabel(), _bodyStyle, 10, 8, false);
        DrawFittedLabel(progressRect, GetBattleChoiceProgressLabel(), _bodyStyle, 10, 8, false);
    }

    private void DrawTurnOrderTimeline(Rect rect)
    {
        DrawOverlaySectionBackground(rect, new Color(0.07f, 0.11f, 0.16f, 0.99f));
        Rect innerRect = new Rect(rect.x + SectionInnerPadding, rect.y + 7f, rect.width - (SectionInnerPadding * 2f), rect.height - 14f);
        DrawTimelineQueue(innerRect, GetBattleUiSurfaceData().Timeline);
    }

    private void DrawTimelinePhaseToken(Rect rect, PrototypeBattleUiTimelineData timeline)
    {
        DrawOverlaySectionBackground(rect, new Color(0.13f, 0.21f, 0.30f, 0.98f));
        Rect titleRect = new Rect(rect.x + 7f, rect.y + 5f, rect.width - 14f, 12f);
        Rect phaseRect = new Rect(rect.x + 7f, titleRect.yMax + 3f, rect.width - 14f, 15f);
        Rect footerRect = new Rect(rect.x + 7f, phaseRect.yMax + 3f, rect.width - 14f, rect.height - 26f);
        string phaseLabel = timeline != null && HasMeaningfulText(timeline.PhaseLabel)
            ? GetCompactHudText(timeline.PhaseLabel, 24, false)
            : "Battle";
        string nextStep = timeline != null && HasMeaningfulText(timeline.NextStepLabel)
            ? GetCompactHudText(timeline.NextStepLabel, 32, false)
            : "Awaiting turn";
        DrawFittedLabel(titleRect, "Turn", _sectionTitleStyle, 9, 8, false);
        DrawFittedLabel(phaseRect, phaseLabel, _sectionTitleStyle, 10, 8, false);
        DrawFittedLabel(footerRect, nextStep, _bodyStyle, 8, 7, true);
    }

    private void DrawTimelineQueue(Rect rect, PrototypeBattleUiTimelineData timeline)
    {
        PrototypeBattleUiTimelineSlotData[] slots = timeline != null ? timeline.Slots : null;
        if (slots == null || slots.Length == 0)
        {
            DrawFittedLabel(rect, "Turn queue pending.", _bodyStyle, 10, 9, false);
            return;
        }

        int startIndex = 0;
        while (startIndex < slots.Length && slots[startIndex] != null && slots[startIndex].IsCurrent)
        {
            startIndex++;
        }

        if (startIndex >= slots.Length)
        {
            DrawFittedLabel(rect, "No queued unit.", _bodyStyle, 10, 9, false);
            return;
        }

        int queuedCount = slots.Length - startIndex;
        int visibleCount = rect.width < 860f ? Mathf.Min(4, queuedCount) : Mathf.Min(5, queuedCount);
        float tokenGap = rect.width < 980f ? 6f : 8f;
        float tokenWidth = (rect.width - (tokenGap * Mathf.Max(0, visibleCount - 1))) / Mathf.Max(1, visibleCount);
        tokenWidth = Mathf.Max(90f, tokenWidth);
        float totalWidth = (tokenWidth * visibleCount) + (tokenGap * Mathf.Max(0, visibleCount - 1));
        float startX = rect.x + Mathf.Max(0f, (rect.width - totalWidth) * 0.5f);
        for (int index = 0; index < visibleCount; index++)
        {
            Rect tokenRect = new Rect(startX + ((tokenWidth + tokenGap) * index), rect.y, tokenWidth, rect.height);
            DrawTimelineSlotToken(tokenRect, slots[startIndex + index], index);
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
        DrawSolidHudRect(new Rect(rect.x, rect.y, rect.width, 3f), accentColor);

        string statusLabel = slotData != null && HasMeaningfulText(slotData.StatusLabel)
            ? GetCompactHudText(slotData.StatusLabel.ToUpperInvariant(), 12, false)
            : isCurrent
                ? "CURRENT"
                : isPending
                    ? "READY"
                    : queueIndex == 0
                        ? "NEXT"
                        : "QUEUE";
        string displayName = slotData != null && HasMeaningfulText(slotData.Label)
            ? GetCompactHudText(slotData.Label, 18, false)
            : "Unknown";
        Rect statusRect = new Rect(rect.x + 8f, rect.y + 6f, rect.width - 16f, 12f);
        Rect nameRect = new Rect(rect.x + 8f, statusRect.yMax + 6f, rect.width - 16f, rect.height - 24f);
        DrawFittedLabel(statusRect, statusLabel, _bodyStyle, 8, 7, false);
        DrawFittedLabel(nameRect, displayName, _sectionTitleStyle, 10, 8, true);
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
        DrawSolidHudRect(new Rect(rect.x, rect.y, 5f, rect.height), accentColor);

        Rect avatarRect = new Rect(rect.x + 10f, rect.y + 10f, 28f, 28f);
        DrawOverlaySectionBackground(avatarRect, new Color(accentColor.r, accentColor.g, accentColor.b, 0.28f));
        string avatarText = !string.IsNullOrEmpty(memberName) ? memberName.Substring(0, 1).ToUpperInvariant() : slotIndex.ToString();
        DrawFittedLabel(new Rect(avatarRect.x + 3f, avatarRect.y + 4f, avatarRect.width - 6f, avatarRect.height - 8f), avatarText, _sectionTitleStyle, 11, 9, false);

        Rect statusRect = new Rect(rect.xMax - 96f, rect.y + 10f, 86f, 20f);
        Rect nameRect = new Rect(avatarRect.xMax + 10f, rect.y + 10f, statusRect.x - avatarRect.xMax - 18f, 18f);
        Rect roleRect = new Rect(nameRect.x, nameRect.yMax + 3f, nameRect.width, 16f);
        Rect hpRect = new Rect(rect.x + 10f, rect.yMax - 24f, rect.width - 20f, 16f);
        Rect contributionRect = new Rect(nameRect.x, roleRect.yMax + 2f, rect.width - (nameRect.x - rect.x) - 10f, Mathf.Max(12f, hpRect.y - roleRect.yMax - 4f));
        string contributionText = BuildBattlePartyMemberReadbackText(memberData, slotIndex - 1);

        DrawOverlaySectionBackground(statusRect, new Color(0.11f, 0.14f, 0.19f, 0.96f));
        DrawFittedLabel(nameRect, GetCompactHudText(memberName, 20, false), _sectionTitleStyle, 12, 10, false);
        DrawFittedLabel(roleRect, GetCompactHudText(role, 18, false), _bodyStyle, 10, 9, false);
        DrawFittedLabel(contributionRect, GetCompactHudText(contributionText, 28, false), _bodyStyle, 9, 7, false);
        DrawFittedLabel(new Rect(statusRect.x + 6f, statusRect.y + 3f, statusRect.width - 12f, statusRect.height - 6f), GetCompactHudText(statusText, 16, false), _bodyStyle, 10, 8, false);

        float hpCurrent = memberData != null ? memberData.CurrentHp : 0f;
        float hpMax = memberData != null ? Mathf.Max(1f, memberData.MaxHp) : 1f;
        DrawStatusBar(hpRect, hpCurrent, hpMax, new Color(0.26f, 0.68f, 0.36f, 0.98f), "HP " + Mathf.RoundToInt(hpCurrent) + " / " + Mathf.RoundToInt(hpMax));
    }

    private string BuildBattlePartyMemberReadbackText(PrototypeBattleUiPartyMemberData memberData, int memberIndex)
    {
        if (memberData != null && HasMeaningfulText(memberData.SummaryText))
        {
            return memberData.SummaryText;
        }

        return _bootEntry != null ? _bootEntry.GetPartyMemberContributionLabel(memberIndex) : "D 0  H 0  A 0  K 0";
    }

    private void DrawCurrentBattleActorCard(Rect rect, PrototypeBattleUiPartyMemberData memberData, CurrentActorSurfaceData actorSurface, int slotIndex)
    {
        string memberName = actorSurface != null && HasMeaningfulText(actorSurface.DisplayName)
            ? actorSurface.DisplayName
            : memberData != null && HasMeaningfulText(memberData.DisplayName)
                ? memberData.DisplayName
                : "Member " + slotIndex;
        string role = actorSurface != null && HasMeaningfulText(actorSurface.RoleLabel)
            ? actorSurface.RoleLabel
            : memberData != null && HasMeaningfulText(memberData.RoleLabel)
                ? memberData.RoleLabel
                : "Adventurer";
        string roleHint = actorSurface != null && HasMeaningfulText(actorSurface.RoleHintText)
            ? actorSurface.RoleHintText
            : string.Empty;
        bool active = memberData != null && memberData.IsActive;
        bool targeted = memberData != null && memberData.IsTargeted;
        bool knockedOut = memberData != null && memberData.IsKnockedOut;
        string statusText = actorSurface != null && HasMeaningfulText(actorSurface.StatusText)
            ? actorSurface.StatusText
            : memberData != null && HasMeaningfulText(memberData.StatusText)
                ? memberData.StatusText
                : BuildPartyCardStatusText(memberName, active, targeted, knockedOut);
        string summaryText = actorSurface != null && HasMeaningfulText(actorSurface.ResolvedStatsText)
            ? actorSurface.ResolvedStatsText
            : memberData != null && HasMeaningfulText(memberData.SummaryText)
                ? memberData.SummaryText
                : BuildBattlePartyMemberReadbackText(memberData, slotIndex - 1);
        string sourceText = actorSurface != null && HasMeaningfulText(actorSurface.StatSourceText)
            ? actorSurface.StatSourceText
            : actorSurface != null && HasMeaningfulText(actorSurface.ResourceText)
                ? actorSurface.ResourceText
                : BuildBattlePartyMemberReadbackText(memberData, slotIndex - 1);

        Color accentColor = GetBattleRoleAccentColor(role, false);
        Color backgroundColor = knockedOut
            ? new Color(0.18f, 0.08f, 0.08f, 0.98f)
            : active
                ? new Color(0.12f, 0.20f, 0.29f, 0.98f)
                : targeted
                    ? new Color(0.17f, 0.14f, 0.10f, 0.98f)
                    : new Color(0.09f, 0.12f, 0.18f, 0.98f);
        DrawOverlaySectionBackground(rect, backgroundColor);
        DrawSolidHudRect(new Rect(rect.x, rect.y, 5f, rect.height), accentColor);

        Rect avatarRect = new Rect(rect.x + 10f, rect.y + 10f, 28f, 28f);
        DrawOverlaySectionBackground(avatarRect, new Color(accentColor.r, accentColor.g, accentColor.b, 0.28f));
        string avatarText = !string.IsNullOrEmpty(memberName) ? memberName.Substring(0, 1).ToUpperInvariant() : slotIndex.ToString();
        DrawFittedLabel(new Rect(avatarRect.x + 3f, avatarRect.y + 4f, avatarRect.width - 6f, avatarRect.height - 8f), avatarText, _sectionTitleStyle, 11, 9, false);

        Rect statusRect = new Rect(rect.xMax - 96f, rect.y + 10f, 86f, 20f);
        Rect nameRect = new Rect(avatarRect.xMax + 10f, rect.y + 10f, statusRect.x - avatarRect.xMax - 18f, 18f);
        Rect roleRect = new Rect(nameRect.x, nameRect.yMax + 3f, nameRect.width, 16f);
        Rect hpRect = new Rect(rect.x + 10f, rect.yMax - 24f, rect.width - 20f, 16f);
        Rect summaryRect = new Rect(nameRect.x, roleRect.yMax + 3f, rect.width - (nameRect.x - rect.x) - 10f, 16f);
        Rect sourceRect = new Rect(nameRect.x, summaryRect.yMax + 2f, rect.width - (nameRect.x - rect.x) - 10f, Mathf.Max(12f, hpRect.y - summaryRect.yMax - 4f));

        DrawOverlaySectionBackground(statusRect, new Color(0.11f, 0.14f, 0.19f, 0.96f));
        DrawFittedLabel(nameRect, GetCompactHudText(memberName, 20, false), _sectionTitleStyle, 12, 10, false);
        string roleLine = HasMeaningfulText(roleHint) ? role + " | " + roleHint : role;
        DrawFittedLabel(roleRect, GetCompactHudText(roleLine, 38, false), _bodyStyle, 10, 9, false);
        DrawFittedLabel(summaryRect, GetCompactHudText(summaryText, 44, false), _bodyStyle, 9, 8, false);
        DrawFittedLabel(sourceRect, GetCompactHudText(sourceText, 58, false), _bodyStyle, 8, 7, true);
        DrawFittedLabel(new Rect(statusRect.x + 6f, statusRect.y + 3f, statusRect.width - 12f, statusRect.height - 6f), GetCompactHudText(statusText, 16, false), _bodyStyle, 10, 8, false);

        float hpCurrent = memberData != null ? memberData.CurrentHp : actorSurface != null ? actorSurface.CurrentHp : 0f;
        float hpMax = memberData != null ? Mathf.Max(1f, memberData.MaxHp) : actorSurface != null ? Mathf.Max(1f, actorSurface.MaxHp) : 1f;
        DrawStatusBar(hpRect, hpCurrent, hpMax, new Color(0.26f, 0.68f, 0.36f, 0.98f), "HP " + Mathf.RoundToInt(hpCurrent) + " / " + Mathf.RoundToInt(hpMax));
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
        PrototypeBattleUiTargetSelectionData targetSelection = GetBattleUiSurfaceData().TargetSelection;
        string enemyName = enemyData != null && HasMeaningfulText(enemyData.DisplayName) ? enemyData.DisplayName : "No target selected";
        string typeLabel = enemyData != null && HasMeaningfulText(enemyData.TypeLabel) ? enemyData.TypeLabel : "Monster";
        string roleLabel = enemyData != null && HasMeaningfulText(enemyData.RoleLabel) ? enemyData.RoleLabel : "Frontline";
        string stateLabel = enemyData != null && HasMeaningfulText(enemyData.StateLabel) ? enemyData.StateLabel : "Unknown";
        string intentLabel = enemyData != null && HasMeaningfulText(enemyData.IntentLabel) ? enemyData.IntentLabel : "Unknown";
        string traitLabel = enemyData != null && HasMeaningfulText(enemyData.TraitText) ? enemyData.TraitText : "Traits pending";
        bool showTargetPreview = targetSelection != null &&
            targetSelection.IsActive &&
            HasMeaningfulText(targetSelection.ExpectedEffectText);
        Color accentColor = GetBattleRoleAccentColor(roleLabel, true);

        DrawOverlaySectionBackground(rect, new Color(0.10f, 0.12f, 0.17f, 0.98f));
        DrawSolidHudRect(new Rect(rect.x, rect.y, rect.width, 4f), accentColor);

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
        DrawFittedLabel(
            intentRect,
            showTargetPreview
                ? GetCompactHudText(targetSelection.ExpectedEffectText, 58, false)
                : "Intent: " + GetCompactHudText(intentLabel, 44, false),
            _bodyStyle,
            10,
            9,
            false);
        DrawFittedLabel(
            traitRect,
            showTargetPreview
                ? GetCompactHudText(GetPreferredText(targetSelection.PostEffectText, targetSelection.FormulaText), 58, false)
                : GetCompactHudText(traitLabel, 58, false),
            _bodyStyle,
            10,
            8,
            false);
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
        DrawSolidHudRect(new Rect(rect.x, rect.y, 4f, rect.height), accentColor);

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
    }

    private void DrawCommandFlyoutPanel(Rect rect)
    {
        string focusKey = GetBattleCommandFocusKey();
        if (!IsPrimaryBattleCommandKey(focusKey))
        {
            return;
        }

        DrawOverlaySectionBackground(rect, new Color(0.07f, 0.10f, 0.15f, 0.98f));
        PrototypeBattleUiCommandDetailData[] moveOptions = focusKey == "move"
            ? GetBattleUiMoveOptionDetails()
            : Array.Empty<PrototypeBattleUiCommandDetailData>();
        int moveOptionCount = focusKey == "move" ? GetBattleUiMoveOptionCount() : 0;
        PrototypeBattleUiCommandDetailData detail = focusKey == "move"
            ? ResolveBattleFlyoutMoveDetail(GetBattleUiCommandDetailByKey(focusKey), moveOptions)
            : GetBattleUiCommandDetailByKey(focusKey);
        Rect headerRect = new Rect(rect.x + SectionInnerPadding, rect.y + 8f, rect.width - (SectionInnerPadding * 2f), 18f);
        Rect subtitleRect = new Rect(headerRect.x, headerRect.yMax + 3f, headerRect.width, 16f);
        Rect contentRect = new Rect(rect.x + SectionInnerPadding, subtitleRect.yMax + 8f, rect.width - (SectionInnerPadding * 2f), rect.height - (subtitleRect.yMax - rect.y) - 12f);

        DrawFittedLabel(headerRect, HasMeaningfulText(detail.Label) ? detail.Label : "Command Detail", _sectionTitleStyle, 12, 10, false);
        DrawFittedLabel(subtitleRect, GetCompactHudText(GetBattleActorShortLabel() + " | " + GetCurrentActorRoleLabel(), 54, false), _bodyStyle, 10, 9, false);

        float buttonHeight = 44f;
        bool available = detail != null && detail.IsAvailable;
        bool showMoveOptions = focusKey == "move" && moveOptionCount > 0;
        bool showPrimaryAction = focusKey != "item" && !showMoveOptions;
        float contentY = contentRect.y;
        if (showMoveOptions)
        {
            Event current = Event.current;
            Vector2 mousePosition = current != null ? current.mousePosition : Vector2.zero;
            for (int index = 0; index < moveOptionCount; index++)
            {
                PrototypeBattleUiCommandDetailData option = moveOptions[index];
                if (option == null)
                {
                    continue;
                }

                Rect optionRect = new Rect(contentRect.x, contentY, contentRect.width, buttonHeight);
                bool hovered = optionRect.Contains(mousePosition);
                if (hovered)
                {
                    _battleHudHoverDetailKey = option.Key;
                    SetBattleActionHoverIfChanged(option.Key);
                }

                if (DrawBottomCommandButton(optionRect, option.Label, option.IsAvailable, hovered, false))
                {
                    _bootEntry.TryTriggerBattleAction(option.Key);
                }

                contentY = optionRect.yMax + 8f;
            }
        }

        if (showPrimaryAction)
        {
            Rect actionRect = new Rect(contentRect.x, contentY, contentRect.width, buttonHeight);
            bool hovered = actionRect.Contains(Event.current != null ? Event.current.mousePosition : Vector2.zero);
            if (DrawBottomCommandButton(actionRect, GetBattleCommandTriggerLabel(focusKey, detail), available, hovered, false))
            {
                TryExecuteBattleCommand(focusKey);
            }

            contentY = actionRect.yMax + 8f;
        }

        string descriptionText = detail != null && HasMeaningfulText(detail.Description)
            ? detail.Description
            : "Choose a command to inspect its target, cost, and effect.";
        float gap = 8f;
        float descriptionHeight = GetBattleFlyoutDescriptionHeight(descriptionText, contentRect.width);
        bool hasPreviewText = detail != null && HasMeaningfulText(detail.PreviewText);
        bool hasFormulaText = detail != null && HasMeaningfulText(detail.FormulaText);
        bool hasGrowthText = detail != null && HasMeaningfulText(detail.GrowthText);
        float previewHeight = hasPreviewText ? GetDynamicInfoCardHeight(detail.PreviewText, contentRect.width) : 0f;
        float metaGap = 6f;
        float metaWidth = (contentRect.width - metaGap) * 0.5f;
        float targetHeight = GetDynamicInfoCardHeight(detail != null ? detail.TargetText : string.Empty, metaWidth);
        float costHeight = GetDynamicInfoCardHeight(detail != null ? detail.CostText : string.Empty, metaWidth);
        float metaRowHeight = Mathf.Max(targetHeight, costHeight);
        float effectHeight = GetDynamicInfoCardHeight(detail != null ? detail.EffectText : string.Empty, contentRect.width);
        float formulaHeight = hasFormulaText ? GetDynamicInfoCardHeight(detail.FormulaText, contentRect.width) : 0f;
        float growthHeight = hasGrowthText ? GetDynamicInfoCardHeight(detail.GrowthText, contentRect.width) : 0f;
        float noteHeight = !available ? 24f : 0f;

        Rect descriptionRect = new Rect(contentRect.x, contentY, contentRect.width, descriptionHeight);
        DrawOverlaySectionBackground(descriptionRect, new Color(0.11f, 0.15f, 0.21f, 0.88f));
        DrawHudLabel(new Rect(descriptionRect.x + 8f, descriptionRect.y + 6f, descriptionRect.width - 16f, descriptionRect.height - 12f), descriptionText, CreateHudMeasureStyle(_bodyStyle, 10, true));

        float cardY = descriptionRect.yMax + gap;
        if (hasPreviewText)
        {
            Rect previewRect = new Rect(contentRect.x, cardY, contentRect.width, previewHeight);
            DrawDynamicInfoCard(previewRect, "Preview", detail.PreviewText, new Color(0.13f, 0.17f, 0.23f, 0.92f));
            cardY = previewRect.yMax + gap;
        }

        Rect targetRect = new Rect(contentRect.x, cardY, metaWidth, metaRowHeight);
        Rect costRect = new Rect(targetRect.xMax + metaGap, targetRect.y, contentRect.width - metaWidth - metaGap, metaRowHeight);
        Rect effectRect = new Rect(contentRect.x, targetRect.yMax + gap, contentRect.width, effectHeight);
        DrawDynamicInfoCard(targetRect, "Target", detail != null ? detail.TargetText : string.Empty, new Color(0.11f, 0.15f, 0.21f, 0.88f));
        DrawDynamicInfoCard(costRect, "Cost", detail != null ? detail.CostText : string.Empty, new Color(0.11f, 0.15f, 0.21f, 0.88f));
        DrawDynamicInfoCard(effectRect, "Effect", detail != null ? detail.EffectText : string.Empty, new Color(0.11f, 0.15f, 0.21f, 0.88f));
        float nextY = effectRect.yMax;
        if (hasFormulaText)
        {
            Rect formulaRect = new Rect(contentRect.x, nextY + gap, contentRect.width, formulaHeight);
            DrawDynamicInfoCard(formulaRect, "Formula", detail.FormulaText, new Color(0.11f, 0.15f, 0.21f, 0.88f));
            nextY = formulaRect.yMax;
        }

        if (hasGrowthText)
        {
            Rect growthRect = new Rect(contentRect.x, nextY + gap, contentRect.width, growthHeight);
            DrawDynamicInfoCard(growthRect, "Growth", detail.GrowthText, new Color(0.11f, 0.15f, 0.21f, 0.88f));
            nextY = growthRect.yMax;
        }

        if (!available)
        {
            Rect noteRect = new Rect(contentRect.x, nextY + gap, contentRect.width, noteHeight);
            DrawOverlaySectionBackground(noteRect, new Color(0.22f, 0.18f, 0.10f, 0.88f));
            DrawHudLabel(new Rect(noteRect.x + 8f, noteRect.y + 4f, noteRect.width - 16f, noteRect.height - 8f), "Unavailable in this batch.", _bodyStyle);
        }
    }

    private void DrawCommandSelectionPanel(Rect rect)
    {
        _battleHudHoverDetailKey = string.Empty;
        DrawOverlaySectionBackground(rect, new Color(0.08f, 0.11f, 0.16f, 0.96f));
        Rect shellRect = new Rect(rect.x + 10f, rect.y + 10f, rect.width - 20f, rect.height - 20f);
        DrawOverlaySectionBackground(shellRect, new Color(0.06f, 0.09f, 0.14f, 0.94f));
        Rect titleRect = new Rect(shellRect.x + 10f, shellRect.y + 8f, shellRect.width - 20f, 18f);
        Rect subtitleRect = new Rect(titleRect.x, titleRect.yMax + 3f, titleRect.width, 16f);
        Rect hintRect = new Rect(titleRect.x, subtitleRect.yMax + 4f, titleRect.width, 15f);
        Rect contentRect = new Rect(shellRect.x + 10f, hintRect.yMax + 10f, shellRect.width - 20f, shellRect.height - (hintRect.yMax - shellRect.y) - 22f);
        DrawFittedLabel(titleRect, "Command Selection", _sectionTitleStyle, 11, 10, false);
        DrawFittedLabel(subtitleRect, GetCompactHudText(GetBattleActorShortLabel(), 24, false), _bodyStyle, 10, 9, false);
        DrawFittedLabel(hintRect, ShouldShowBattleCommandFlyout() ? "Detail flyout active" : "Select action to open detail", _bodyStyle, 9, 8, false);

        string[] commandKeys = { "attack", "skill", "item", "move", "end_turn" };
        string[] labels =
        {
            "Attack [1]",
            "Skill [2]",
            "Item [3]",
            "Move [4]",
            "End Turn [5]"
        };

        Event current = Event.current;
        Vector2 mousePosition = current != null ? current.mousePosition : Vector2.zero;
        float buttonGap = 10f;
        float maxButtonHeight = 54f;
        float buttonHeight = Mathf.Min(maxButtonHeight, (contentRect.height - (buttonGap * (commandKeys.Length - 1))) / commandKeys.Length);
        string hoveredActionKey = string.Empty;
        for (int index = 0; index < commandKeys.Length; index++)
        {
            string commandKey = commandKeys[index];
            Rect buttonRect = new Rect(contentRect.x, contentRect.y + ((buttonHeight + buttonGap) * index), contentRect.width, buttonHeight);
            bool available = IsBattleCommandAvailable(commandKey);
            bool hovered = buttonRect.Contains(mousePosition);
            bool selected = GetBattleCommandFocusKey() == commandKey;
            if (hovered)
            {
                if (commandKey == "attack" || commandKey == "skill" || commandKey == "move" || commandKey == "end_turn")
                {
                    hoveredActionKey = commandKey;
                    SetBattleActionHoverIfChanged(commandKey);
                }
            }

            if (DrawBottomCommandButton(buttonRect, labels[index], available || commandKey == "item", hovered, selected))
            {
                SelectBattleCommand(commandKey);
            }
        }

        if (string.IsNullOrEmpty(hoveredActionKey))
        {
            SetBattleActionHoverIfChanged(string.Empty);
        }
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

    private void DrawDynamicInfoCard(Rect rect, string title, string value, Color backgroundColor)
    {
        DrawOverlaySectionBackground(rect, backgroundColor);
        Rect titleRect = new Rect(rect.x + 8f, rect.y + 5f, rect.width - 16f, 12f);
        Rect valueRect = new Rect(rect.x + 8f, titleRect.yMax + 4f, rect.width - 16f, rect.height - 21f);
        DrawFittedLabel(titleRect, title, _sectionTitleStyle, 9, 8, false);
        DrawHudLabel(valueRect, HasMeaningfulText(value) ? value : "None", CreateHudMeasureStyle(_bodyStyle, 9, true));
    }

    private float GetPreferredBattleCommandSelectionHeight()
    {
        const float buttonGap = 10f;
        const float buttonHeight = 48f;
        const int buttonCount = 5;
        return 10f + 8f + 18f + 3f + 16f + 4f + 15f + 10f + (buttonHeight * buttonCount) + (buttonGap * (buttonCount - 1)) + 22f + 10f;
    }

    private float GetPreferredBattleCommandFlyoutHeight(float panelWidth)
    {
        string focusKey = GetBattleCommandFocusKey();
        PrototypeBattleUiCommandDetailData[] moveOptions = focusKey == "move"
            ? GetBattleUiMoveOptionDetails()
            : Array.Empty<PrototypeBattleUiCommandDetailData>();
        int moveOptionCount = focusKey == "move" ? GetBattleUiMoveOptionCount() : 0;
        PrototypeBattleUiCommandDetailData detail = focusKey == "move"
            ? ResolveBattleFlyoutMoveDetail(GetBattleUiCommandDetailByKey(focusKey), moveOptions)
            : GetBattleUiCommandDetailByKey(focusKey);
        bool available = detail != null && detail.IsAvailable;
        bool showMoveOptions = focusKey == "move" && moveOptionCount > 0;
        bool showPrimaryAction = focusKey != "item" && !showMoveOptions;
        float contentWidth = Mathf.Max(160f, panelWidth - (SectionInnerPadding * 2f));
        string descriptionText = detail != null && HasMeaningfulText(detail.Description)
            ? detail.Description
            : "Choose a command to inspect its target, cost, and effect.";
        float baseHeight = 8f + 18f + 3f + 16f + 8f + 12f;
        float actionHeight = showMoveOptions
            ? (moveOptionCount * 52f) + (Mathf.Max(0, moveOptionCount - 1) * 8f)
            : showPrimaryAction
                ? 52f
                : 0f;
        float gap = 8f;
        float descriptionHeight = GetBattleFlyoutDescriptionHeight(descriptionText, contentWidth);
        bool hasPreviewText = detail != null && HasMeaningfulText(detail.PreviewText);
        bool hasFormulaText = detail != null && HasMeaningfulText(detail.FormulaText);
        bool hasGrowthText = detail != null && HasMeaningfulText(detail.GrowthText);
        float previewHeight = hasPreviewText ? GetDynamicInfoCardHeight(detail.PreviewText, contentWidth) : 0f;
        float metaGap = 6f;
        float metaWidth = (contentWidth - metaGap) * 0.5f;
        float targetHeight = GetDynamicInfoCardHeight(detail != null ? detail.TargetText : string.Empty, metaWidth);
        float costHeight = GetDynamicInfoCardHeight(detail != null ? detail.CostText : string.Empty, metaWidth);
        float metaRowHeight = Mathf.Max(targetHeight, costHeight);
        float effectHeight = GetDynamicInfoCardHeight(detail != null ? detail.EffectText : string.Empty, contentWidth);
        float formulaHeight = hasFormulaText ? GetDynamicInfoCardHeight(detail.FormulaText, contentWidth) : 0f;
        float growthHeight = hasGrowthText ? GetDynamicInfoCardHeight(detail.GrowthText, contentWidth) : 0f;
        float totalHeight = baseHeight + actionHeight + descriptionHeight;
        if (hasPreviewText)
        {
            totalHeight += gap + previewHeight;
        }

        totalHeight += gap + metaRowHeight;
        totalHeight += gap + effectHeight;
        if (hasFormulaText)
        {
            totalHeight += gap + formulaHeight;
        }

        if (hasGrowthText)
        {
            totalHeight += gap + growthHeight;
        }

        if (!available)
        {
            totalHeight += gap + 24f;
        }

        return totalHeight;
    }

    private float GetBattleFlyoutDescriptionHeight(string text, float width)
    {
        float measuredHeight = MeasureWrappedHudTextHeight(text, _bodyStyle, 10, Mathf.Max(64f, width - 16f));
        return Mathf.Max(56f, measuredHeight + 12f);
    }

    private float GetDynamicInfoCardHeight(string value, float width)
    {
        float measuredHeight = MeasureWrappedHudTextHeight(HasMeaningfulText(value) ? value : "None", _bodyStyle, 9, Mathf.Max(48f, width - 16f));
        return Mathf.Max(38f, measuredHeight + 29f);
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
            _bootEntry.TryTriggerBattleAction("skill");
            _battleFlyoutMode = BattleHudFlyoutMode.ActorCommandMenu;
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
            SetBattleActionHoverIfChanged(string.Empty);
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
            SetBattleActionHoverIfChanged(string.Empty);
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

        SetBattleActionHoverIfChanged(string.Empty);
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
            SetBattleActionHoverIfChanged(string.Empty);
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
            SetBattleActionHoverIfChanged(string.Empty);
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
        DrawSolidHudRect(dividerRect, new Color(0.56f, 0.68f, 0.82f, 0.16f));
        DrawFittedLabel(feedbackRect, "Last: " + feedback, _bodyStyle, 10, 9, false);
    }

    private void DrawTargetSelectionOverlay(Rect hudRect, Rect mainRect)
    {
        PrototypeBattleUiTargetSelectionData targetSelection = GetBattleUiSurfaceData().TargetSelection;
        float overlayWidth = Mathf.Min(520f, Screen.width * 0.44f);
        List<string> detailLines = new List<string>();
        string actionText = targetSelection != null && HasMeaningfulText(targetSelection.QueuedActionLabel)
            ? targetSelection.QueuedActionLabel
            : "Action";
        string targetText = targetSelection != null && HasMeaningfulText(targetSelection.TargetLabel)
            ? targetSelection.TargetLabel
            : "Choose a target";
        string cancelText = targetSelection != null && HasMeaningfulText(targetSelection.CancelHint)
            ? GetCompactHudText(targetSelection.CancelHint, 36, false)
            : GetBattleCancelHintText();
        detailLines.Add(GetCompactHudText(actionText + "  |  " + targetText + "  |  " + cancelText, 104, false));
        if (targetSelection != null && targetSelection.HasFocusedTarget)
        {
            string detail = "HP " + targetSelection.TargetCurrentHp + " / " + targetSelection.TargetMaxHp;
            if (HasMeaningfulText(targetSelection.TargetIntentLabel))
            {
                detail += "  |  " + GetCompactHudText(targetSelection.TargetIntentLabel, 38, false);
            }

            detailLines.Add(detail);
        }

        if (targetSelection != null && HasMeaningfulText(targetSelection.ThreatSummaryText))
        {
            detailLines.Add(GetCompactHudText(targetSelection.ThreatSummaryText, 88, false));
        }

        if (targetSelection != null && HasMeaningfulText(targetSelection.ExpectedEffectText))
        {
            detailLines.Add(GetCompactHudText(targetSelection.ExpectedEffectText, 88, false));
        }

        if (targetSelection != null && HasMeaningfulText(targetSelection.PostEffectText))
        {
            detailLines.Add(GetCompactHudText(targetSelection.PostEffectText, 88, false));
        }

        if (targetSelection != null && HasMeaningfulText(targetSelection.FormulaText))
        {
            detailLines.Add(GetCompactHudText(targetSelection.FormulaText, 88, false));
        }
        else if (targetSelection != null && HasMeaningfulText(targetSelection.GrowthText))
        {
            detailLines.Add(GetCompactHudText(targetSelection.GrowthText, 88, false));
        }

        if (targetSelection != null && HasMeaningfulText(targetSelection.SkillHintText))
        {
            detailLines.Add(GetCompactHudText(targetSelection.SkillHintText, 88, false));
        }

        float overlayHeight = 32f + (detailLines.Count * 18f) + 8f;
        float overlayX = (Screen.width - overlayWidth) * 0.5f;
        float overlayY = Mathf.Max(hudRect.yMax + 14f, mainRect.y - overlayHeight - 12f);
        Rect overlayRect = new Rect(overlayX, overlayY, overlayWidth, overlayHeight);
        DrawOverlaySectionBackground(overlayRect, new Color(0.18f, 0.13f, 0.08f, 0.94f));

        Rect titleRect = new Rect(overlayRect.x + 12f, overlayRect.y + 6f, overlayRect.width - 24f, 14f);
        string title = targetSelection != null && HasMeaningfulText(targetSelection.Title)
            ? targetSelection.Title
            : "Select target";
        DrawFittedLabel(titleRect, title, _sectionTitleStyle, 10, 9, false);
        for (int index = 0; index < detailLines.Count; index++)
        {
            Rect lineRect = new Rect(overlayRect.x + 12f, titleRect.yMax + 4f + (index * 18f), overlayRect.width - 24f, 16f);
            DrawFittedLabel(lineRect, detailLines[index], _bodyStyle, 10, 8, false);
        }
    }

    private bool DrawBattleActionMenuButton(Rect rect, string actionKey, string label, bool available, bool selected, Vector2 mousePosition, ref string hoveredActionKey)
    {
        bool hovered = available && rect.Contains(mousePosition);
        if (hovered)
        {
            hoveredActionKey = actionKey;
            _battleHudHoverDetailKey = actionKey;
            SetBattleActionHoverIfChanged(actionKey);
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

        if (detail != null && HasMeaningfulText(detail.PreviewText))
        {
            lines.Add("Preview: " + detail.PreviewText);
        }

        if (detail != null && HasMeaningfulText(detail.FormulaText))
        {
            lines.Add("Formula: " + detail.FormulaText);
        }

        if (detail != null && HasMeaningfulText(detail.GrowthText))
        {
            lines.Add("Growth: " + detail.GrowthText);
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

        if (_bootEntry.IsBattleActionHovered("move"))
        {
            return "move";
        }

        if (_bootEntry.IsBattleActionHovered("end_turn"))
        {
            return "end_turn";
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
        string stateText = knockedOut
            ? "KO"
            : active
                ? "Acting"
                : "Ready";

        if (targeted)
        {
            stateText += " | Targeted";
        }

        if (ContainsIgnoreCase(memberLabel, "guard"))
        {
            stateText += " | Guard";
        }

        return stateText;
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
        string summary = string.Empty;
        if (surface != null && HasMeaningfulText(surface.CurrentDungeonName) && surface.CurrentDungeonName != "None")
        {
            summary = surface.CurrentDungeonName;
        }

        if (surface != null && HasMeaningfulText(surface.CurrentRouteLabel) && surface.CurrentRouteLabel != "None")
        {
            summary = AppendBattleSummaryToken(summary, surface.CurrentRouteLabel);
        }

        if (surface != null && HasMeaningfulText(surface.TotalPartyHp) && surface.TotalPartyHp != "None")
        {
            summary = AppendBattleSummaryToken(summary, "Party " + surface.TotalPartyHp);
        }

        if (surface != null && HasMeaningfulText(surface.EliteStatusText) && surface.EliteStatusText != "None")
        {
            summary = AppendBattleSummaryToken(summary, surface.EliteStatusText);
        }

        return HasMeaningfulText(summary)
            ? GetCompactHudText(summary, 132, false)
            : "Battle in progress";
    }

    private string AppendBattleSummaryToken(string summary, string nextToken)
    {
        if (!HasMeaningfulText(nextToken))
        {
            return summary;
        }

        return HasMeaningfulText(summary)
            ? summary + "   " + nextToken
            : nextToken;
    }

    private string GetBattleDungeonNameLabel()
    {
        PrototypeBattleUiSurfaceData surface = GetBattleUiSurfaceData();
        string dungeonName = surface != null && HasMeaningfulText(surface.CurrentDungeonName) && surface.CurrentDungeonName != "None"
            ? surface.CurrentDungeonName
            : "Dungeon";
        return GetCompactHudText(dungeonName, 40, false);
    }

    private string GetBattleDungeonTypeLabel()
    {
        PrototypeBattleUiSurfaceData surface = GetBattleUiSurfaceData();
        string dungeonType = surface != null && HasMeaningfulText(surface.CurrentRouteLabel) && surface.CurrentRouteLabel != "None"
            ? surface.CurrentRouteLabel
            : surface != null && HasMeaningfulText(surface.EncounterRoomType) && surface.EncounterRoomType != "None"
                ? surface.EncounterRoomType
                : "Type pending";
        return GetCompactHudText(dungeonType, 40, false);
    }

    private string GetBattleChoiceProgressLabel()
    {
        PrototypeBattleUiSurfaceData surface = GetBattleUiSurfaceData();
        string roomProgress = surface != null && HasMeaningfulText(surface.RoomProgressText) && surface.RoomProgressText != "None"
            ? surface.RoomProgressText
            : "0 / 0";
        return GetCompactHudText("Choice " + roomProgress, 24, false);
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
        if (!ShouldDrawCurrentEventVisuals())
        {
            return;
        }

        string safeText = string.IsNullOrEmpty(text) ? string.Empty : text;
        int resolvedFontSize = Mathf.Max(minFontSize, maxFontSize);
        if (wordWrap && minFontSize < maxFontSize)
        {
            float measuredHeight = MeasureWrappedHudTextHeight(safeText, baseStyle, resolvedFontSize, rect.width);
            if (measuredHeight > rect.height)
            {
                resolvedFontSize = minFontSize;
            }
        }
        else if (!wordWrap && minFontSize < maxFontSize)
        {
            GUIStyle maxStyle = GetHudStyleVariant(baseStyle, resolvedFontSize, false);
            if (maxStyle.lineHeight > rect.height + 0.5f)
            {
                resolvedFontSize = minFontSize;
            }
        }

        DrawHudLabel(rect, safeText, GetHudStyleVariant(baseStyle, resolvedFontSize, wordWrap));
    }

    private GUIStyle CreateHudMeasureStyle(GUIStyle baseStyle, int fontSize, bool wordWrap)
    {
        return GetHudStyleVariant(baseStyle, fontSize, wordWrap);
    }

    private float MeasureWrappedHudTextHeight(string text, GUIStyle baseStyle, int fontSize, float width)
    {
        string safeText = string.IsNullOrEmpty(text) ? string.Empty : text;
        int widthBucket = Mathf.RoundToInt(Mathf.Max(1f, width) * 10f);
        HudMeasureKey cacheKey = new HudMeasureKey(GetHudBaseStyleId(baseStyle), fontSize, widthBucket, safeText);
        if (_hudMeasureHeights.TryGetValue(cacheKey, out float cachedHeight))
        {
            return cachedHeight;
        }

        GUIStyle style = CreateHudMeasureStyle(baseStyle, fontSize, true);
        float measuredHeight = style.CalcHeight(GetReusableHudContent(safeText), Mathf.Max(1f, width));
        if (_hudMeasureHeights.Count >= 512)
        {
            _hudMeasureHeights.Clear();
        }

        _hudMeasureHeights[cacheKey] = measuredHeight;
        return measuredHeight;
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

    private PrototypeBattleUiPartyMemberData GetCurrentBattlePartyMember()
    {
        PrototypeBattleUiSurfaceData surface = GetBattleUiSurfaceData();
        PrototypeBattleUiPartyMemberData[] members = surface.PartyMembers;
        if (members == null || members.Length == 0)
        {
            return null;
        }

        for (int index = 0; index < members.Length; index++)
        {
            PrototypeBattleUiPartyMemberData member = members[index];
            if (member != null && member.IsActive)
            {
                return member;
            }
        }

        string actorName = surface.CurrentActor != null ? surface.CurrentActor.DisplayName : string.Empty;
        if (HasMeaningfulText(actorName))
        {
            for (int index = 0; index < members.Length; index++)
            {
                PrototypeBattleUiPartyMemberData member = members[index];
                if (member != null &&
                    HasMeaningfulText(member.DisplayName) &&
                    string.Equals(member.DisplayName, actorName, System.StringComparison.OrdinalIgnoreCase))
                {
                    return member;
                }
            }
        }

        for (int index = 0; index < members.Length; index++)
        {
            if (members[index] != null)
            {
                return members[index];
            }
        }

        return null;
    }

    private int ResolveBattlePartyMemberSlot(PrototypeBattleUiPartyMemberData member)
    {
        if (member == null)
        {
            return 1;
        }

        if (member.SlotIndex > 0)
        {
            return member.SlotIndex;
        }

        PrototypeBattleUiPartyMemberData[] members = GetBattleUiSurfaceData().PartyMembers;
        if (members != null)
        {
            for (int index = 0; index < members.Length; index++)
            {
                if (ReferenceEquals(members[index], member))
                {
                    return index + 1;
                }
            }
        }

        return 1;
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
        RefreshBattleUiSurface(EventType.Repaint);
    }

    private void RefreshBattleUiSurface(EventType currentEventType)
    {
        if (_bootEntry == null)
        {
            ResetBattleUiSurfaceCache();
            return;
        }

        int currentFrame = Time.frameCount;
        if (_battleUiSurfaceFrame == currentFrame && _battleUiSurfaceEventType == currentEventType)
        {
            return;
        }

        PrototypeBattleUiSurfaceData surface = _bootEntry.GetBattleUiSurfaceData();
        _battleUiSurface = surface ?? new PrototypeBattleUiSurfaceData();
        _battleUiSurfaceFrame = currentFrame;
        _battleUiSurfaceEventType = currentEventType;
        RebuildBattleMoveOptionCache();
    }

    private PrototypeBattleUiSurfaceData GetBattleUiSurfaceData()
    {
        if (_battleUiSurface == null)
        {
            _battleUiSurface = new PrototypeBattleUiSurfaceData();
        }

        return _battleUiSurface;
    }

    private PrototypeBattleUiCommandDetailData GetBattleUiCommandDetailByKey(string detailKey)
    {
        PrototypeBattleUiCommandSurfaceData commandSurface = GetBattleUiSurfaceData().CommandSurface;
        PrototypeBattleUiCommandDetailData fallbackDetail = new PrototypeBattleUiCommandDetailData();
        fallbackDetail.Label = "Command";
        fallbackDetail.Description = "Details pending.";
        if (commandSurface == null || commandSurface.Details == null || commandSurface.Details.Length == 0)
        {
            return fallbackDetail;
        }

        if (HasMeaningfulText(detailKey))
        {
            for (int i = 0; i < commandSurface.Details.Length; i++)
            {
                PrototypeBattleUiCommandDetailData detail = commandSurface.Details[i];
                if (detail != null && detail.Key == detailKey)
                {
                    return detail;
                }
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

    private PrototypeBattleUiCommandDetailData GetBattleUiCommandDetail(BattleHudFlyoutMode mode)
    {
        return GetBattleUiCommandDetailByKey(GetCommandDetailKey(mode));
    }

    private PrototypeBattleUiCommandDetailData[] GetBattleUiMoveOptionDetails()
    {
        return _battleMoveOptionBuffer;
    }

    private int GetBattleUiMoveOptionCount()
    {
        return _battleMoveOptionCount;
    }

    private PrototypeBattleUiCommandDetailData ResolveBattleFlyoutMoveDetail(PrototypeBattleUiCommandDetailData fallbackDetail, PrototypeBattleUiCommandDetailData[] moveOptions)
    {
        if (HasMeaningfulText(_battleHudHoverDetailKey) && IsBattleMoveOptionKey(_battleHudHoverDetailKey))
        {
            PrototypeBattleUiCommandDetailData hoveredDetail = GetBattleUiCommandDetailByKey(_battleHudHoverDetailKey);
            if (hoveredDetail != null)
            {
                return hoveredDetail;
            }
        }

        return moveOptions != null && _battleMoveOptionCount > 0
            ? moveOptions[0] ?? fallbackDetail
            : fallbackDetail;
    }

    private bool IsBattleMoveOptionKey(string commandKey)
    {
        return commandKey == "move_front" ||
               commandKey == "move_middle" ||
               commandKey == "move_back";
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
        if (!ShouldDrawCurrentEventVisuals())
        {
            return;
        }

        DrawSolidHudRect(rect, new Color(0.06f, 0.08f, 0.11f, 0.92f));

        float fillRatio = Mathf.Clamp01(current / max);
        if (fillRatio > 0f)
        {
            Rect fillRect = new Rect(rect.x + 1f, rect.y + 1f, (rect.width - 2f) * fillRatio, rect.height - 2f);
            DrawSolidHudRect(fillRect, fillColor);
        }

        int maxLength = rect.width < 120f ? 14 : 22;
        DrawHudLabel(new Rect(rect.x + 6f, rect.y, rect.width - 12f, rect.height), GetCompactHudText(label, maxLength, false), _sectionTitleStyle);
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
        if (actionKey == "attack")
        {
            return "Attack";
        }

        if (actionKey == "skill")
        {
            return GetCurrentActorSkillName();
        }

        if (actionKey == "move")
        {
            return "Move";
        }

        if (actionKey == "end_turn")
        {
            return "End Turn";
        }

        if (actionKey == "retreat")
        {
            return "Retreat";
        }

        return "Action";
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

        if (_bootEntry.IsBattleActionSelected("move"))
        {
            return "move";
        }

        if (_bootEntry.IsBattleActionSelected("end_turn"))
        {
            return "end_turn";
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

        if (IsBattleInputModalOpen())
        {
            DismissBattleCommandOverlayState();
            return;
        }

        string ownerKey = GetBattleCommandOwnerKey();
        if (!HasMeaningfulText(ownerKey))
        {
            _selectedBattleCommandKey = string.Empty;
            _selectedBattleCommandOwnerKey = string.Empty;
        }
        else if (!HasMeaningfulText(_selectedBattleCommandOwnerKey))
        {
            _selectedBattleCommandOwnerKey = ownerKey;
        }
        else if (_selectedBattleCommandOwnerKey != ownerKey)
        {
            _selectedBattleCommandKey = string.Empty;
            _selectedBattleCommandOwnerKey = ownerKey;
        }

        string selectedActionKey = GetSelectedBattleActionKey();
        if (HasMeaningfulText(selectedActionKey) && IsPrimaryBattleCommandKey(selectedActionKey))
        {
            _selectedBattleCommandKey = selectedActionKey;
            _selectedBattleCommandOwnerKey = ownerKey;
        }
        else if (!IsPrimaryBattleCommandKey(_selectedBattleCommandKey))
        {
            _selectedBattleCommandKey = string.Empty;
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
        _selectedBattleCommandKey = string.Empty;
        _selectedBattleCommandOwnerKey = string.Empty;
        _battleHudHoverDetailKey = string.Empty;
        SetBattleActionHoverIfChanged(string.Empty);
    }

    private void DismissBattleCommandOverlayState()
    {
        _battleFlyoutMode = BattleHudFlyoutMode.None;
        _pendingConfirmActionKey = string.Empty;
        _battleHudHoverDetailKey = string.Empty;
        SetBattleActionHoverIfChanged(string.Empty);
    }

    private bool IsBattleInputModalOpen()
    {
        return _bootEntry != null && _bootEntry.IsInventorySurfaceOpen;
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

    private string GetBattleCommandFocusKey()
    {
        if (HasMeaningfulText(_battleHudHoverDetailKey) && IsPrimaryBattleCommandKey(_battleHudHoverDetailKey))
        {
            return _battleHudHoverDetailKey;
        }

        PrototypeBattleUiCommandSurfaceData commandSurface = GetBattleUiSurfaceData().CommandSurface;
        if (commandSurface != null &&
            HasMeaningfulText(commandSurface.FocusedActionKey) &&
            IsPrimaryBattleCommandKey(commandSurface.FocusedActionKey))
        {
            return commandSurface.FocusedActionKey;
        }

        string selectedActionKey = GetSelectedBattleActionKey();
        if (HasMeaningfulText(selectedActionKey) && IsPrimaryBattleCommandKey(selectedActionKey))
        {
            return selectedActionKey;
        }

        return IsPrimaryBattleCommandKey(_selectedBattleCommandKey)
            ? _selectedBattleCommandKey
            : string.Empty;
    }

    private bool ShouldShowBattleCommandFlyout()
    {
        return IsPrimaryBattleCommandKey(GetBattleCommandFocusKey());
    }

    private string GetBattleCommandOwnerKey()
    {
        string actorLabel = GetBattleActorShortLabel();
        string roleLabel = GetCurrentActorRoleLabel();
        if (!HasMeaningfulText(actorLabel))
        {
            return string.Empty;
        }

        return actorLabel + "|" + roleLabel;
    }

    private bool IsPrimaryBattleCommandKey(string commandKey)
    {
        return commandKey == "attack" ||
               commandKey == "skill" ||
               commandKey == "item" ||
               commandKey == "move" ||
               commandKey == "end_turn";
    }

    private void SelectBattleCommand(string commandKey)
    {
        if (!IsPrimaryBattleCommandKey(commandKey))
        {
            return;
        }

        if (_selectedBattleCommandKey == commandKey && !HasMeaningfulText(GetSelectedBattleActionKey()))
        {
            _selectedBattleCommandKey = string.Empty;
            return;
        }

        _selectedBattleCommandKey = commandKey;
        _selectedBattleCommandOwnerKey = GetBattleCommandOwnerKey();
    }

    private bool IsBattleCommandAvailable(string commandKey)
    {
        if (_bootEntry == null)
        {
            return false;
        }

        if (commandKey == "item")
        {
            return false;
        }

        return _bootEntry.IsBattleActionAvailable(commandKey);
    }

    private string GetBattleCommandTriggerLabel(string commandKey, PrototypeBattleUiCommandDetailData detail)
    {
        if (commandKey == "attack")
        {
            return IsTargetSelectionActive() && GetSelectedBattleActionKey() == "attack"
                ? "Targeting Attack"
                : "Choose Attack Target";
        }

        if (commandKey == "skill")
        {
            string skillLabel = detail != null && HasMeaningfulText(detail.Label) ? detail.Label : "Skill";
            return IsTargetSelectionActive() && GetSelectedBattleActionKey() == "skill"
                ? "Targeting " + skillLabel
                : "Use " + skillLabel;
        }

        if (commandKey == "move")
        {
            return "Choose Row";
        }

        if (commandKey == "end_turn")
        {
            return "Pass Turn";
        }

        return "Unavailable";
    }

    private bool TryExecuteBattleCommand(string commandKey)
    {
        if (_bootEntry == null || !IsBattleCommandAvailable(commandKey))
        {
            return false;
        }

        SelectBattleCommand(commandKey);
        return _bootEntry.TryTriggerBattleAction(commandKey);
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
            V(_bootEntry.LegacyDungeonRouteChoiceTitleLabel),
            "Legacy fallback only. ExpeditionPrep owns the normal launch seam.",
            V(_bootEntry.LegacyDungeonRouteChoicePromptLabel)), _bodyStyle);

        Event current = Event.current;
        Vector2 mousePosition = current != null ? current.mousePosition : Vector2.zero;
        string hoveredChoiceKey = string.Empty;

        DrawRouteChoiceButton(optionARect, "safe", "[1] " + V(_bootEntry.LegacyDungeonRouteOption1Label), mousePosition, ref hoveredChoiceKey);
        DrawRouteChoiceButton(optionBRect, "risky", "[2] " + V(_bootEntry.LegacyDungeonRouteOption2Label), mousePosition, ref hoveredChoiceKey);

        if (string.IsNullOrEmpty(hoveredChoiceKey))
        {
            _bootEntry.SetLegacyDungeonRouteChoiceHover(string.Empty);
        }

        bool policyHovered = policyRect.Contains(mousePosition);
        string policyLabel = "[Q] " + T("CyclePolicy") + " " + V(_bootEntry.CurrentDispatchPolicyRunLabel);
        if (DrawInteractiveButton(policyRect, policyLabel, true, policyHovered, false))
        {
            _bootEntry.TryCycleCurrentDispatchPolicy();
        }

        bool canConfirm = _bootEntry.CanConfirmLegacyDungeonRouteChoice();
        bool confirmHovered = canConfirm && confirmRect.Contains(mousePosition);
        if (DrawInteractiveButton(confirmRect, "[Enter] Continue Fallback", canConfirm, confirmHovered, false))
        {
            _bootEntry.TryConfirmLegacyDungeonRouteChoice();
        }
    }

    private void DrawRouteChoiceButton(Rect rect, string optionKey, string label, Vector2 mousePosition, ref string hoveredChoiceKey)
    {
        bool available = _bootEntry.IsLegacyDungeonRouteChoiceAvailable(optionKey);
        bool selected = _bootEntry.IsLegacyDungeonRouteChoiceSelected(optionKey);
        bool hovered = available && rect.Contains(mousePosition);
        if (hovered)
        {
            hoveredChoiceKey = optionKey;
            _bootEntry.SetLegacyDungeonRouteChoiceHover(optionKey);
        }

        if (DrawInteractiveButton(rect, label, available, hovered, selected))
        {
            _bootEntry.TryTriggerLegacyDungeonRouteChoice(optionKey);
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
            "DungeonRun internal event decision.",
            V(_bootEntry.DungeonRunEventDecisionPromptLabel),
            Line("EventStatus", V(_bootEntry.EventStatusLabel)),
            Line("EventChoice", V(_bootEntry.DungeonRunEventDecisionLabel))), _bodyStyle);

        Event current = Event.current;
        Vector2 mousePosition = current != null ? current.mousePosition : Vector2.zero;
        string hoveredChoiceKey = string.Empty;

        DrawEventChoiceButton(optionARect, "recover", "[1] " + V(_bootEntry.DungeonRunEventDecisionOptionALabel), mousePosition, ref hoveredChoiceKey);
        DrawEventChoiceButton(optionBRect, "loot", "[2] " + V(_bootEntry.DungeonRunEventDecisionOptionBLabel), mousePosition, ref hoveredChoiceKey);

        if (string.IsNullOrEmpty(hoveredChoiceKey))
        {
            _bootEntry.SetDungeonRunEventDecisionHover(string.Empty);
        }
    }

    private void DrawEventChoiceButton(Rect rect, string optionKey, string label, Vector2 mousePosition, ref string hoveredChoiceKey)
    {
        bool available = _bootEntry.IsDungeonRunEventDecisionAvailable(optionKey);
        bool selected = _bootEntry.IsDungeonRunEventDecisionSelected(optionKey);
        bool hovered = available && rect.Contains(mousePosition);
        if (hovered)
        {
            hoveredChoiceKey = optionKey;
            _bootEntry.SetDungeonRunEventDecisionHover(optionKey);
        }

        if (DrawInteractiveButton(rect, label, available, hovered, selected))
        {
            _bootEntry.TryTriggerDungeonRunEventDecision(optionKey);
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
            "DungeonRun internal elite decision.",
            V(_bootEntry.DungeonRunPreEliteDecisionPromptLabel),
            Line("PreEliteChoice", V(_bootEntry.DungeonRunPreEliteDecisionLabel)),
            Line("TotalPartyHp", V(_bootEntry.TotalPartyHpLabel)),
            Line("PartyCondition", V(_bootEntry.PartyConditionLabel)),
            Line("EliteEncounter", V(_bootEntry.EliteEncounterNameLabel))), _bodyStyle);

        Event current = Event.current;
        Vector2 mousePosition = current != null ? current.mousePosition : Vector2.zero;
        string hoveredChoiceKey = string.Empty;

        DrawPreEliteChoiceButton(optionARect, "recover", "[1] " + V(_bootEntry.DungeonRunPreEliteDecisionOptionALabel), mousePosition, ref hoveredChoiceKey);
        DrawPreEliteChoiceButton(optionBRect, "bonus", "[2] " + V(_bootEntry.DungeonRunPreEliteDecisionOptionBLabel), mousePosition, ref hoveredChoiceKey);

        if (string.IsNullOrEmpty(hoveredChoiceKey))
        {
            _bootEntry.SetDungeonRunPreEliteDecisionHover(string.Empty);
        }
    }

    private void DrawPreEliteChoiceButton(Rect rect, string optionKey, string label, Vector2 mousePosition, ref string hoveredChoiceKey)
    {
        bool available = _bootEntry.IsDungeonRunPreEliteDecisionAvailable(optionKey);
        bool selected = _bootEntry.IsDungeonRunPreEliteDecisionSelected(optionKey);
        bool hovered = available && rect.Contains(mousePosition);
        if (hovered)
        {
            hoveredChoiceKey = optionKey;
            _bootEntry.SetDungeonRunPreEliteDecisionHover(optionKey);
        }

        if (DrawInteractiveButton(rect, label, available, hovered, selected))
        {
            _bootEntry.TryTriggerDungeonRunPreEliteDecision(optionKey);
        }
    }

    private bool DrawInteractiveButton(Rect rect, string label, bool available, bool hovered, bool selected)
    {
        if (ShouldDrawCurrentEventVisuals())
        {
            Color buttonColor = !available
                ? new Color(0.10f, 0.10f, 0.12f, 0.72f)
                : selected
                    ? new Color(0.22f, 0.37f, 0.52f, 0.96f)
                    : hovered
                        ? new Color(0.18f, 0.28f, 0.40f, 0.96f)
                        : new Color(0.10f, 0.14f, 0.20f, 0.94f);
            DrawSolidHudRect(rect, buttonColor);
            if (!_battleHudFastBackground)
            {
                DrawSolidHudRect(new Rect(rect.x, rect.y, rect.width, 1f), new Color(0.64f, 0.78f, 0.92f, 0.16f));
            }

            Color previousColor = GUI.color;
            GUI.color = available ? Color.white : new Color(0.6f, 0.64f, 0.68f, 1f);
            DrawHudLabel(rect, label, _actionButtonStyle);
            GUI.color = previousColor;
        }

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
        float contentHeight = Mathf.Max(viewportRect.height, MeasureWrappedHudTextHeight(safeBody, _bodyStyle, _bodyStyle.fontSize, contentWidth));
        if (contentHeight > viewportRect.height)
        {
            showVerticalScrollbar = true;
            contentWidth = Mathf.Max(32f, viewportRect.width - 18f);
            contentHeight = Mathf.Max(viewportRect.height, MeasureWrappedHudTextHeight(safeBody, _bodyStyle, _bodyStyle.fontSize, contentWidth));
        }

        Vector2 scrollPosition = GetOverlayScrollPosition(scrollKey);
        Vector2 nextScrollPosition = GUI.BeginScrollView(
            viewportRect,
            scrollPosition,
            new Rect(0f, 0f, contentWidth, contentHeight),
            false,
            showVerticalScrollbar);
        DrawHudLabel(new Rect(0f, 0f, contentWidth, contentHeight), safeBody, _bodyStyle);
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
        if (!ShouldDrawCurrentEventVisuals())
        {
            return;
        }

        if (_battleHudFastBackground)
        {
            DrawSolidHudRect(rect, backgroundColor);
            return;
        }

        float innerWidth = Mathf.Max(0f, rect.width - 2f);
        float innerHeight = Mathf.Max(0f, rect.height - 2f);
        Rect innerRect = new Rect(rect.x + 1f, rect.y + 1f, innerWidth, innerHeight);
        DrawSolidHudRect(rect, new Color(0.01f, 0.02f, 0.03f, Mathf.Clamp01(backgroundColor.a)));
        if (innerWidth > 0f && innerHeight > 0f)
        {
            DrawSolidHudRect(innerRect, backgroundColor);
            DrawSolidHudRect(new Rect(innerRect.x, innerRect.y, innerRect.width, 2f), new Color(0.56f, 0.68f, 0.82f, 0.10f));
            DrawSolidHudRect(new Rect(innerRect.x, innerRect.yMax - 1f, innerRect.width, 1f), new Color(0f, 0f, 0f, 0.34f));
        }
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

        Color previousBackgroundColor = GUI.backgroundColor;
        Color previousContentColor = GUI.contentColor;
        GUI.backgroundColor = new Color(0.08f, 0.11f, 0.14f, 0.98f);
        GUI.contentColor = Color.white;
        GUI.SetNextControlName(SearchFieldControlName);
        _searchText = GUI.TextField(fieldRect, _searchText ?? string.Empty, _searchFieldStyle);
        GUI.backgroundColor = previousBackgroundColor;
        GUI.contentColor = previousContentColor;

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
            float bodyHeight = expanded ? Mathf.Max(28f, MeasureWrappedHudTextHeight(panels[i].Body, _bodyStyle, _bodyStyle.fontSize, bodyWidth)) : 0f;
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
            float bodyHeight = expanded ? Mathf.Max(28f, MeasureWrappedHudTextHeight(panel.Body, _bodyStyle, _bodyStyle.fontSize, rect.width - 20f)) : 0f;
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

    private List<HudPanel> GetDockPanels()
    {
        if (_dockPanels != null && _dockPanelsFrame == Time.frameCount)
        {
            return _dockPanels;
        }

        _dockPanels = BuildPanels();
        _dockPanelsFrame = Time.frameCount;
        return _dockPanels;
    }

    private List<HudPanel> BuildPanels()
    {
        PrototypeDungeonRunShellSurfaceData dungeonShellSurface = new PrototypeDungeonRunShellSurfaceData();
        ExpeditionStartContext expeditionStartContext = new ExpeditionStartContext();
        PrototypeDungeonPanelContext dungeonPanelContext = new PrototypeDungeonPanelContext();
        PrototypeDungeonRunResultContext dungeonRunResultContext = new PrototypeDungeonRunResultContext();

        if (_bootEntry != null && _bootEntry.IsDungeonRunHudMode)
        {
            if (_expandedByKey != null)
            {
                _expandedByKey["dungeon_run"] = true;
            }

            dungeonShellSurface = _bootEntry.GetDungeonRunShellSurfaceData();
            expeditionStartContext = dungeonShellSurface.ExpeditionStartContext ?? new ExpeditionStartContext();
            dungeonPanelContext = dungeonShellSurface.PanelContext ?? new PrototypeDungeonPanelContext();
            dungeonRunResultContext = dungeonShellSurface.ResultContext ?? new PrototypeDungeonRunResultContext();
            if (_bootEntry.IsDungeonBattleViewActive || _bootEntry.IsLegacyDungeonRouteChoiceVisible || _bootEntry.IsDungeonRunEventDecisionVisible || _bootEntry.IsDungeonRunPreEliteDecisionVisible || _bootEntry.IsDungeonResultPanelVisible)
            {
                return new List<HudPanel>();
            }

            return new List<HudPanel>
            {
                BuildDungeonRunSummaryPanel(dungeonShellSurface, expeditionStartContext, dungeonPanelContext)
            };
        }

        return new List<HudPanel>
        {
            new HudPanel("sim_status", T("PanelSimulationStatus"), () => BuildPanelBody(
                Line("Prototype", _bootEntry.PrototypeNameLabel),
                Line("Step", _bootEntry.DebugStepLabel),
                Line("Language", _bootEntry.CurrentLanguageLabel),
                Line("CurrentState", V(_bootEntry.CurrentStateLabel)),
                Line("LastTransition", V(_bootEntry.LastTransitionLabel)),
                Line("Visible", BuildVisibleSummary())), HudFilter.Simulation),
            new HudPanel("world_snapshot", T("PanelWorldSnapshot"), () => BuildPanelBody(
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
            new HudPanel("trade_flow", T("PanelTradeFlow"), () => BuildPanelBody(
                Line("TradeOpportunities", _bootEntry.TradeOpportunityCount),
                Line("ActiveTradeOpportunities", _bootEntry.ActiveTradeOpportunityCount),
                Line("UnmetCityNeeds", _bootEntry.UnmetCityNeedsCount),
                Line("TradeLink1", V(_bootEntry.TradeLink1Label)),
                Line("TradeLink2", V(_bootEntry.TradeLink2Label)),
                Line("DungeonOutputHint", V(_bootEntry.DungeonOutputHintLabel))), HudFilter.Economy),
            new HudPanel("economy_control", T("PanelEconomyControl"), () => BuildPanelBody(
                Line("EconomyControls", _bootEntry.EconomyControlsLabel),
                Line("WorldDay", _bootEntry.WorldDayCount),
                Line("AutoTickEnabled", BoolText(_bootEntry.AutoTickEnabled)),
                Line("AutoTickPaused", BoolText(_bootEntry.AutoTickPaused)),
                Line("TickInterval", _bootEntry.TickIntervalSeconds.ToString("0.00")),
                Line("AutoTickCount", _bootEntry.AutoTickCount),
                Line("TradeStepCount", _bootEntry.TradeStepCount)), HudFilter.Economy),
            new HudPanel("expedition_loop", T("PanelExpeditionLoop"), () => BuildPanelBody(
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
            BuildDungeonRunSummaryPanel(dungeonShellSurface, expeditionStartContext, dungeonPanelContext),
            new HudPanel("dungeon_battle", T("PanelDungeonBattle"), () => BuildPanelBody(
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
            BuildDungeonResultSummaryPanel(dungeonRunResultContext),
            new HudPanel("economy_pressure", T("PanelEconomyPressure"), () => BuildPanelBody(
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
            new HudPanel("selected_entity", T("PanelSelectedEntity"), () => BuildPanelBody(
                Line("Selected", V(_bootEntry.SelectedDisplayName)),
                Line("SelectedType", V(_bootEntry.SelectedTypeLabel)),
                Line("SelectedPosition", V(_bootEntry.SelectedPositionLabel)),
                Line("SelectedId", V(_bootEntry.SelectedIdLabel)),
                Line("SelectedTags", V(_bootEntry.SelectedTagsLabel)),
                Line("SelectedStat", V(_bootEntry.SelectedStatLabel)),
                Line("SelectedResources", V(_bootEntry.SelectedResourcesLabel)),
                Line("SelectedResourceRoles", V(_bootEntry.SelectedResourceRolesLabel))), HudFilter.Selected),
            new HudPanel("selected_economy", T("PanelSelectedEconomy"), () => BuildPanelBody(
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
            new HudPanel("selected_day_metrics", T("PanelSelectedDayMetrics"), () => BuildPanelBody(
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
            new HudPanel("selected_trade_signals", T("PanelSelectedTradeSignals"), () => BuildPanelBody(
                Line("SelectedIncomingTrade", V(_bootEntry.SelectedIncomingTradeLabel)),
                Line("SelectedOutgoingTrade", V(_bootEntry.SelectedOutgoingTradeLabel)),
                Line("SelectedUnmetNeeds", V(_bootEntry.SelectedUnmetNeedsLabel))), HudFilter.Selected),
            new HudPanel("recent_logs", T("PanelRecentDayLogs"), () => BuildPanelBody(
                Line("RecentDayLog1", V(_bootEntry.RecentDayLog1Label)),
                Line("RecentDayLog2", V(_bootEntry.RecentDayLog2Label)),
                Line("RecentDayLog3", V(_bootEntry.RecentDayLog3Label))), HudFilter.Logs)
        };
    }

    private HudPanel BuildDungeonRunSummaryPanel(PrototypeDungeonRunShellSurfaceData shellSurface, ExpeditionStartContext expeditionStartContext, PrototypeDungeonPanelContext dungeonPanelContext)
    {
        PrototypeDungeonRunShellSurfaceData safeShellSurface = shellSurface ?? new PrototypeDungeonRunShellSurfaceData();
        ExpeditionStartContext safeExpeditionStartContext = expeditionStartContext ?? new ExpeditionStartContext();
        PrototypeDungeonPanelContext safeDungeonPanelContext = dungeonPanelContext ?? new PrototypeDungeonPanelContext();
        return new HudPanel("dungeon_run", T("PanelDungeonRun"), () => BuildPanelBody(
            Line("Step", _bootEntry.DebugStepLabel),
            Line("WorldSimControls", _bootEntry.DungeonRunWorldControlsLabel),
            Line("DungeonExploreControls", _bootEntry.DungeonRunExploreControlsLabel),
            Line("DungeonBattleControls", _bootEntry.DungeonRunBattleControlsLabel),
            Line("DungeonTargetControls", _bootEntry.DungeonRunTargetControlsLabel),
            Line("DungeonRouteControls", _bootEntry.DungeonRunRouteControlsLabel),
            Line("RunState", V(safeShellSurface.RunStateLabel)),
            Line("CurrentCity", V(safeShellSurface.CurrentCityLabel)),
            Line("CurrentDungeon", V(safeShellSurface.CurrentDungeonLabel)),
            Line("DungeonDanger", V(safeShellSurface.DungeonDangerLabel)),
            Line("DispatchPolicy", V(safeShellSurface.DispatchPolicyText)),
            Line("RecommendedRoute", V(safeShellSurface.RecommendedRouteText)),
            Line("RecommendationReason", V(safeShellSurface.RecommendationReasonText)),
            Line("RecoveryAdvice", V(safeShellSurface.RecoveryAdviceText)),
            Line("ExpectedNeedImpact", V(safeShellSurface.ExpectedNeedImpactText)),
            "Start Supply: " + V(safeExpeditionStartContext.SupplyPressureSummaryText),
            "Supply Pressure: " + V(safeDungeonPanelContext.SupplyPressureSummaryText),
            "Time Pressure: " + V(safeDungeonPanelContext.TimePressureSummaryText),
            "Threat Pressure: " + V(safeDungeonPanelContext.ThreatPressureSummaryText),
            "Discovery: " + V(safeDungeonPanelContext.DiscoverySummaryText),
            "Extraction Pressure: " + V(safeDungeonPanelContext.ExtractionPressureSummaryText),
            "Choice Outcome: " + V(safeDungeonPanelContext.LatestChoiceOutcomeSummaryText),
            "Event Resolution: " + V(safeDungeonPanelContext.EventResolutionSummaryText),
            "Extraction Summary: " + V(safeDungeonPanelContext.ExtractionSummaryText),
            "Encounter Request: " + V(safeDungeonPanelContext.EncounterRequestSummaryText),
            Line("ActiveParty", V(safeShellSurface.ActivePartyLabel)),
            Line("CurrentRoom", V(safeShellSurface.CurrentRoomLabel)),
            Line("CurrentRoomType", V(safeShellSurface.CurrentRoomTypeLabel)),
            Line("RoomProgress", V(safeShellSurface.RoomProgressText)),
            Line("NextMajorGoal", V(safeShellSurface.NextMajorGoalText)),
            Line("TotalPartyHp", V(safeShellSurface.TotalPartyHpText)),
            Line("PartyCondition", V(safeShellSurface.PartyConditionText)),
            Line("SustainPressure", V(safeShellSurface.SustainPressureText)),
            Line("CurrentRoute", V(safeShellSurface.CurrentRouteLabel)),
            Line("RouteRisk", V(safeShellSurface.RouteRiskLabel)),
            Line("CarriedLoot", V(safeShellSurface.CarriedLootText)),
            Line("RunTurnCount", safeShellSurface.TurnCount),
            Line("EncounterProgress", V(safeShellSurface.EncounterProgressText)),
            Line("EliteStatus", V(safeShellSurface.EliteStatusText)),
            Line("ExitUnlocked", V(safeShellSurface.ExitUnlockedText)),
            Line("EventStatus", V(safeShellSurface.EventStatusText)),
            Line("EventChoice", V(safeShellSurface.EventChoiceText)),
            Line("PreEliteChoice", V(safeShellSurface.PreEliteChoiceText)),
            Line("LootBreakdown", V(safeShellSurface.LootBreakdownText)),
            Line("LastBattleLog1", V(safeShellSurface.RecentBattleLog1Text)),
            Line("LastBattleLog2", V(safeShellSurface.RecentBattleLog2Text)),
            Line("LastBattleLog3", V(safeShellSurface.RecentBattleLog3Text))), HudFilter.Simulation);
    }

    private HudPanel BuildDungeonResultSummaryPanel(PrototypeDungeonRunResultContext resultContext)
    {
        PrototypeDungeonRunResultContext safeResultContext = resultContext ?? new PrototypeDungeonRunResultContext();
        return new HudPanel("dungeon_result", T("PanelDungeonResult"), () => BuildPanelBody(
            Line("Result", V(safeResultContext.ResultStateText)),
            Line("CityDispatchedFrom", V(safeResultContext.CityDispatchedFromText)),
            Line("DungeonChosen", V(safeResultContext.DungeonLabel)),
            Line("DungeonDanger", V(safeResultContext.DungeonDangerText)),
            Line("RecommendedRoute", V(safeResultContext.RecommendedRouteText)),
            Line("FollowedRecommendation", V(safeResultContext.FollowedRecommendationText)),
            Line("ManaShardsReturned", V(safeResultContext.ManaShardsReturnedText)),
            Line("StockBefore", V(safeResultContext.StockBeforeText)),
            Line("StockAfter", V(safeResultContext.StockAfterText)),
            Line("StockDelta", V(safeResultContext.StockDeltaText)),
            Line("NeedPressureBefore", V(safeResultContext.NeedPressureBeforeText)),
            Line("NeedPressureAfter", V(safeResultContext.NeedPressureAfterText)),
            Line("TurnsTaken", V(safeResultContext.TurnsTakenText)),
            "Time Cost Summary: " + V(safeResultContext.TimeCostSummaryText),
            "Resource Delta Summary: " + V(safeResultContext.ResourceDeltaSummaryText),
            "Supply Pressure Summary: " + V(safeResultContext.SupplyPressureSummaryText),
            "Discovered Flags: " + V(safeResultContext.DiscoveredFlagsSummaryText),
            "Threat Progress: " + V(safeResultContext.ThreatProgressSummaryText),
            "Extraction Pressure: " + V(safeResultContext.ExtractionPressureSummaryText),
            "Battles Fought: " + safeResultContext.BattlesFoughtCount,
            "Key Encounters: " + V(safeResultContext.KeyEncounterSummaryText),
            "Choice Outcome Summary: " + V(safeResultContext.ChoiceOutcomeSummaryText),
            "Event Resolution Summary: " + V(safeResultContext.EventResolutionSummaryText),
            "Extraction Summary: " + V(safeResultContext.ExtractionSummaryText),
            "Encounter Request Summary: " + V(safeResultContext.EncounterRequestSummaryText),
            "Battle Absorb Summary: " + V(safeResultContext.BattleAbsorbSummaryText),
            Line("LootGained", V(safeResultContext.LootGainedText)),
            Line("RouteChosen", V(safeResultContext.RouteLabel)),
            Line("RouteRisk", V(safeResultContext.RouteRiskText)),
            Line("BattleLoot", V(safeResultContext.BattleLootText)),
            Line("ChestLoot", V(safeResultContext.ChestLootText)),
            Line("EventLoot", V(safeResultContext.EventLootText)),
            Line("EventChoice", V(safeResultContext.EventChoiceText)),
            Line("PreEliteChoice", V(safeResultContext.PreEliteChoiceText)),
            Line("PreEliteHealAmount", V(safeResultContext.PreEliteHealAmountText)),
            Line("EliteBonusRewardEarned", V(safeResultContext.EliteBonusRewardEarnedText)),
            Line("EliteBonusRewardAmount", V(safeResultContext.EliteBonusRewardAmountText)),
            Line("RoomPathSummary", V(safeResultContext.RoomPathSummaryText)),
            Line("PartyHpSummary", V(safeResultContext.PartyHpSummaryText)),
            Line("PartyConditionAtEnd", V(safeResultContext.PartyConditionText)),
            Line("EliteDefeated", V(safeResultContext.EliteDefeatedText)),
            Line("EliteName", V(safeResultContext.EliteNameText)),
            Line("EliteRewardIdentity", V(safeResultContext.EliteRewardIdentityText)),
            Line("EliteRewardAmount", V(safeResultContext.EliteRewardAmountText)),
            Line("SurvivingMembers", V(safeResultContext.SurvivingMembersSummaryText)),
            Line("ClearedEncounters", V(safeResultContext.ClearedEncountersText)),
            Line("OpenedChests", V(safeResultContext.OpenedChestsText)),
            "Latest Return Aftermath: " + V(safeResultContext.WorldOutcomeReadbackPreview.LatestReturnAftermathText),
            "Outcome Readback: " + V(safeResultContext.WorldOutcomeReadbackPreview.PostRunSummaryText),
            "Corrective Follow-Up: " + V(safeResultContext.WorldOutcomeReadbackPreview.NextSuggestedActionText),
            Line("ReturnToWorldPrompt", V(safeResultContext.ReturnPromptText))), HudFilter.Logs);
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
            { "expedition_loop", false },
            { "economy_pressure", false },
            { "selected_entity", false },
            { "selected_economy", false },
            { "selected_day_metrics", false },
            { "selected_trade_signals", false },
            { "dungeon_run", false },
            { "dungeon_battle", false },
            { "dungeon_result", false },
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
        _chipStyle.fontSize = 11;
        _chipStyle.alignment = TextAnchor.MiddleCenter;
        _chipStyle.normal.textColor = new Color(0.88f, 0.92f, 0.88f, 1f);
        _chipStyle.hover.textColor = Color.white;
        _chipStyle.active.textColor = Color.white;
        _chipStyle.normal.background = Texture2D.whiteTexture;
        _chipStyle.hover.background = Texture2D.whiteTexture;
        _chipStyle.active.background = Texture2D.whiteTexture;
        _chipStyle.border = new RectOffset(1, 1, 1, 1);
        _chipStyle.padding = new RectOffset(6, 6, 3, 3);

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
        _searchFieldStyle.normal.textColor = new Color(0.92f, 0.95f, 0.96f, 1f);
        _searchFieldStyle.focused.textColor = new Color(0.96f, 0.97f, 0.98f, 1f);
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

    private GUIContent GetReusableHudContent(string text)
    {
        _sharedHudContent.text = text ?? string.Empty;
        _sharedHudContent.tooltip = string.Empty;
        _sharedHudContent.image = null;
        return _sharedHudContent;
    }

    private int GetHudBaseStyleId(GUIStyle baseStyle)
    {
        if (ReferenceEquals(baseStyle, _bodyStyle))
        {
            return 1;
        }

        if (ReferenceEquals(baseStyle, _titleStyle))
        {
            return 2;
        }

        if (ReferenceEquals(baseStyle, _sectionTitleStyle))
        {
            return 3;
        }

        if (ReferenceEquals(baseStyle, _actionButtonStyle))
        {
            return 4;
        }

        return 0;
    }

    private GUIStyle GetHudStyleVariant(GUIStyle baseStyle, int fontSize, bool wordWrap)
    {
        GUIStyle sourceStyle = baseStyle ?? GUIStyle.none;
        HudStyleVariantKey cacheKey = new HudStyleVariantKey(GetHudBaseStyleId(sourceStyle), fontSize, wordWrap);
        if (_hudStyleVariants.TryGetValue(cacheKey, out GUIStyle cachedStyle))
        {
            return cachedStyle;
        }

        GUIStyle style = new GUIStyle(sourceStyle);
        style.fontSize = fontSize;
        style.wordWrap = wordWrap;
        style.clipping = TextClipping.Clip;
        style.padding = new RectOffset(0, 0, 0, 0);
        style.margin = new RectOffset(0, 0, 0, 0);
        _hudStyleVariants[cacheKey] = style;
        return style;
    }

    private void DrawHudLabel(Rect rect, string text, GUIStyle style)
    {
        if (!ShouldDrawCurrentEventVisuals())
        {
            return;
        }

        GUI.Label(rect, GetReusableHudContent(text), style);
    }

    private void DrawSolidHudRect(Rect rect, Color color)
    {
        if (!ShouldDrawCurrentEventVisuals() || rect.width <= 0f || rect.height <= 0f)
        {
            return;
        }

        Color previousColor = GUI.color;
        GUI.color = color;
        GUI.DrawTexture(rect, Texture2D.whiteTexture);
        GUI.color = previousColor;
    }

    private static bool ShouldDrawCurrentEventVisuals()
    {
        Event current = Event.current;
        return current == null || current.type == EventType.Repaint;
    }

    private static bool IsBattleHudUsefulEvent(EventType eventType)
    {
        return eventType == EventType.Repaint ||
               eventType == EventType.MouseDown ||
               eventType == EventType.MouseUp ||
               eventType == EventType.MouseMove ||
               eventType == EventType.MouseDrag ||
               eventType == EventType.ScrollWheel ||
               eventType == EventType.KeyDown ||
               eventType == EventType.KeyUp;
    }

    private void HandleDebugHudToggle(Event current)
    {
        if (current == null || current.type != EventType.KeyDown || current.keyCode != DebugHudToggleKey)
        {
            return;
        }

        _debugHudVisible = !_debugHudVisible;
        ResetDockPanelCache();
        _isSearchFieldFocused = false;
        GUI.FocusControl(string.Empty);
        current.Use();
    }

    private void ResetDockPanelCache()
    {
        _dockPanelsFrame = -1;
        _dockPanels = null;
    }

    private void SetBattleActionHoverIfChanged(string actionKey)
    {
        if (_bootEntry == null)
        {
            _lastBattleActionHoverKey = string.Empty;
            return;
        }

        string nextKey = HasMeaningfulText(actionKey) ? actionKey : string.Empty;
        if (string.Equals(_lastBattleActionHoverKey, nextKey, StringComparison.Ordinal))
        {
            return;
        }

        _bootEntry.SetBattleActionHover(nextKey);
        _lastBattleActionHoverKey = nextKey;
    }

    private void ResetBattleUiSurfaceCache()
    {
        ResetDockPanelCache();
        _battleUiSurface = new PrototypeBattleUiSurfaceData();
        _battleMoveOptionCount = 0;
        Array.Clear(_battleMoveOptionBuffer, 0, _battleMoveOptionBuffer.Length);
        _battleUiSurfaceFrame = -1;
        _battleUiSurfaceEventType = EventType.Ignore;
    }

    private void RebuildBattleMoveOptionCache()
    {
        _battleMoveOptionCount = 0;
        Array.Clear(_battleMoveOptionBuffer, 0, _battleMoveOptionBuffer.Length);

        PrototypeBattleUiCommandSurfaceData commandSurface = GetBattleUiSurfaceData().CommandSurface;
        if (commandSurface == null || commandSurface.Details == null || commandSurface.Details.Length == 0)
        {
            return;
        }

        for (int index = 0; index < commandSurface.Details.Length && _battleMoveOptionCount < _battleMoveOptionBuffer.Length; index++)
        {
            PrototypeBattleUiCommandDetailData detail = commandSurface.Details[index];
            if (detail == null || !IsBattleMoveOptionKey(detail.Key))
            {
                continue;
            }

            _battleMoveOptionBuffer[_battleMoveOptionCount++] = detail;
        }
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






