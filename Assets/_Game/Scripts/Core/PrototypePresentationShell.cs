using System.Collections.Generic;
using UnityEngine;

[DisallowMultipleComponent]
public sealed partial class PrototypePresentationShell : MonoBehaviour
{
    private const float ScreenMargin = 18f;
    private const float WorldTopBarHeight = 96f;
    private const float WorldBottomBarHeight = 58f;
    private const float WorldDropdownHeight = 212f;
    private const float WorldBottomSheetHeight = 166f;
    private static readonly Rect[] EmptyBlockingRects = System.Array.Empty<Rect>();

    private BootstrapSceneStateBridge _bootEntry;
    private GUIStyle _heroTitleStyle;
    private GUIStyle _heroSubtitleStyle;
    private GUIStyle _panelTitleStyle;
    private GUIStyle _bodyStyle;
    private GUIStyle _captionStyle;
    private GUIStyle _metricLabelStyle;
    private GUIStyle _metricValueStyle;
    private GUIStyle _buttonStyle;
    private GUIStyle _badgeStyle;
    private readonly PrototypeCityHubPresentationState _cityHubPresentationState = new PrototypeCityHubPresentationState();
    private Rect[] _blockingRects = EmptyBlockingRects;
    private readonly System.Collections.Generic.Dictionary<string, Vector2> _shellScrollByKey = new System.Collections.Generic.Dictionary<string, Vector2>();

    public bool IsPointerOverBlockingUi(Vector2 screenPosition)
    {
        if (_bootEntry == null)
        {
            CacheBootEntry();
        }

        if (_bootEntry == null || (!_bootEntry.IsMainMenuActive && !_bootEntry.IsWorldSimActive && !_bootEntry.IsDungeonRunHudMode))
        {
            return false;
        }

        Vector2 guiPosition = new Vector2(screenPosition.x, Screen.height - screenPosition.y);
        for (int i = 0; i < _blockingRects.Length; i++)
        {
            if (_blockingRects[i].Contains(guiPosition))
            {
                return true;
            }
        }

        return false;
    }

    private void OnGUI()
    {
        CacheBootEntry();
        if (_bootEntry == null)
        {
            _blockingRects = EmptyBlockingRects;
            return;
        }

        EnsureStyles();
        GUI.depth = 100;

        if (_bootEntry.IsMainMenuActive)
        {
            ResetCityHubPresentationState("MainMenu active.");
            DrawMainMenuShell();
            return;
        }

        if (_bootEntry.IsWorldSimActive)
        {
            DrawWorldSimShell();
            return;
        }

        if (_bootEntry.IsDungeonRunHudMode)
        {
            ResetCityHubPresentationState("DungeonRun active.");
            DrawDungeonRunShell();
            return;
        }

        ResetCityHubPresentationState("No active shell mode.");
        _blockingRects = EmptyBlockingRects;
    }

    private void DrawMainMenuShell()
    {
        Rect screenRect = new Rect(0f, 0f, Screen.width, Screen.height);
        _blockingRects = new[] { screenRect };

        DrawRect(screenRect, new Color(0.05f, 0.08f, 0.11f, 0.94f));
        DrawRect(new Rect(0f, 0f, Screen.width, Screen.height * 0.18f), new Color(0.11f, 0.17f, 0.22f, 0.28f));
        DrawRect(new Rect(0f, Screen.height * 0.78f, Screen.width, Screen.height * 0.22f), new Color(0.02f, 0.03f, 0.05f, 0.40f));
        DrawRotatedRect(new Rect(Screen.width * 0.08f, Screen.height * 0.12f, Screen.width * 0.34f, 18f), -12f, new Color(0.20f, 0.34f, 0.42f, 0.18f));
        DrawRotatedRect(new Rect(Screen.width * 0.54f, Screen.height * 0.22f, Screen.width * 0.28f, 12f), 18f, new Color(0.88f, 0.72f, 0.34f, 0.10f));
        DrawRotatedRect(new Rect(Screen.width * 0.60f, Screen.height * 0.72f, Screen.width * 0.22f, 12f), -22f, new Color(0.24f, 0.58f, 0.66f, 0.10f));

        float contentWidth = Mathf.Min(1200f, Screen.width - 96f);
        float contentHeight = Mathf.Min(760f, Screen.height - 96f);
        Rect contentRect = CenterRect(contentWidth, contentHeight);
        Rect heroRect = new Rect(contentRect.x, contentRect.y + 54f, contentRect.width * 0.58f - 14f, 430f);
        Rect previewRect = new Rect(heroRect.xMax + 28f, heroRect.y, contentRect.width - heroRect.width - 28f, 430f);
        Rect pillarsRect = new Rect(contentRect.x, heroRect.yMax + 24f, contentRect.width, 132f);
        Rect languageRect = new Rect(contentRect.xMax - 150f, contentRect.y, 150f, 34f);
        Rect badgeRect = new Rect(contentRect.x, contentRect.y, 220f, 34f);

        DrawPill(badgeRect, T("FrontMenuPrototypeBadge"), new Color(0.14f, 0.23f, 0.28f, 0.96f), new Color(0.93f, 0.95f, 0.92f, 1f));
        DrawLanguageButtons(languageRect, 68f, 8f, true);
        DrawMainMenuHero(heroRect);
        DrawMainMenuPreview(previewRect);
        DrawMainMenuPillars(pillarsRect);
    }

    private void DrawMainMenuHero(Rect rect)
    {
        DrawPanel(rect, new Color(0.08f, 0.11f, 0.15f, 0.98f), new Color(0.12f, 0.17f, 0.23f, 0.96f));

        Rect titleRect = new Rect(rect.x + 24f, rect.y + 30f, rect.width - 48f, 56f);
        Rect subtitleRect = new Rect(titleRect.x, titleRect.yMax + 8f, titleRect.width, 50f);
        Rect conceptRect = new Rect(titleRect.x, subtitleRect.yMax + 12f, titleRect.width - 16f, 78f);
        Rect enterRect = new Rect(titleRect.x, rect.yMax - 102f, 300f, 54f);
        Rect hintRect = new Rect(titleRect.x, enterRect.yMax + 8f, rect.width - 48f, 24f);

        GUI.Label(titleRect, V(_bootEntry.PrototypeNameLabel), _heroTitleStyle);
        GUI.Label(subtitleRect, T("FrontMenuSubtitle"), _heroSubtitleStyle);
        GUI.Label(conceptRect, T("FrontMenuConcept"), _bodyStyle);

        if (DrawActionButton(enterRect, T("FrontActionEnterWorld") + "  [Enter]", new Color(0.20f, 0.48f, 0.58f, 1f), true))
        {
            _bootEntry.EnterWorldSimFromMenu();
        }

        GUI.Label(hintRect, T("FrontMenuHint"), _captionStyle);
    }

    private void DrawMainMenuPreview(Rect rect)
    {
        DrawPanel(rect, new Color(0.07f, 0.10f, 0.14f, 0.98f), new Color(0.11f, 0.15f, 0.18f, 0.94f));

        Rect titleRect = new Rect(rect.x + 20f, rect.y + 20f, rect.width - 40f, 24f);
        Rect boardRect = new Rect(rect.x + 20f, titleRect.yMax + 16f, rect.width - 40f, rect.height - 94f);
        Rect footerRect = new Rect(rect.x + 20f, boardRect.yMax + 10f, rect.width - 40f, 20f);

        GUI.Label(titleRect, T("FrontMenuPillarNetwork"), _panelTitleStyle);
        DrawPanel(boardRect, new Color(0.06f, 0.08f, 0.11f, 0.98f), new Color(0.12f, 0.16f, 0.17f, 0.76f));
        DrawRect(new Rect(boardRect.x + 18f, boardRect.y + 22f, boardRect.width - 36f, 2f), new Color(0.88f, 0.72f, 0.34f, 0.18f));
        DrawRect(new Rect(boardRect.x + 18f, boardRect.yMax - 24f, boardRect.width - 36f, 2f), new Color(0.20f, 0.54f, 0.62f, 0.16f));

        Vector2 cityA = new Vector2(boardRect.x + boardRect.width * 0.26f, boardRect.y + boardRect.height * 0.34f);
        Vector2 cityB = new Vector2(boardRect.x + boardRect.width * 0.70f, boardRect.y + boardRect.height * 0.58f);
        Vector2 dungeonA = new Vector2(boardRect.x + boardRect.width * 0.40f, boardRect.y + boardRect.height * 0.72f);
        Vector2 dungeonB = new Vector2(boardRect.x + boardRect.width * 0.78f, boardRect.y + boardRect.height * 0.26f);
        DrawConnection(cityA, cityB, new Color(0.64f, 0.55f, 0.37f, 0.95f), 7f);
        DrawConnection(cityA, dungeonA, new Color(0.22f, 0.52f, 0.60f, 0.45f), 4f);
        DrawConnection(cityB, dungeonB, new Color(0.80f, 0.46f, 0.34f, 0.36f), 4f);
        DrawMiniNode(new Rect(cityA.x - 28f, cityA.y - 28f, 56f, 56f), false, new Color(0.35f, 0.74f, 0.90f, 1f));
        DrawMiniNode(new Rect(cityB.x - 28f, cityB.y - 28f, 56f, 56f), false, new Color(0.86f, 0.72f, 0.34f, 1f));
        DrawMiniNode(new Rect(dungeonA.x - 22f, dungeonA.y - 22f, 44f, 44f), true, new Color(0.84f, 0.42f, 0.52f, 1f));
        DrawMiniNode(new Rect(dungeonB.x - 22f, dungeonB.y - 22f, 44f, 44f), true, new Color(0.46f, 0.76f, 0.46f, 1f));
        GUI.Label(footerRect, T("FrontMenuSubtitle"), _captionStyle);
    }

    private void DrawMainMenuPillars(Rect rect)
    {
        float gap = 14f;
        float width = (rect.width - (gap * 2f)) / 3f;
        DrawFeatureCard(new Rect(rect.x, rect.y, width, rect.height), T("FrontMenuPillarEconomy"));
        DrawFeatureCard(new Rect(rect.x + width + gap, rect.y, width, rect.height), T("FrontMenuPillarDispatch"));
        DrawFeatureCard(new Rect(rect.x + ((width + gap) * 2f), rect.y, width, rect.height), T("FrontMenuPillarNetwork"));
    }

    private void DrawFeatureCard(Rect rect, string body)
    {
        DrawPanel(rect, new Color(0.08f, 0.11f, 0.14f, 0.96f), new Color(0.10f, 0.13f, 0.16f, 0.92f));
        GUI.Label(new Rect(rect.x + 16f, rect.y + 16f, rect.width - 32f, rect.height - 32f), body, _bodyStyle);
    }

    private void DrawWorldSimShell()
    {
        float reservedRight = Mathf.Clamp(Screen.width * 0.19f, 320f, 360f);
        Rect layoutRect = new Rect(ScreenMargin, ScreenMargin, Screen.width - reservedRight - (ScreenMargin * 2f), Screen.height - (ScreenMargin * 2f));
        Rect gnbRect = new Rect(layoutRect.x, layoutRect.y, layoutRect.width, WorldTopBarHeight);
        Rect bnbRect = new Rect(layoutRect.x, layoutRect.yMax - WorldBottomBarHeight, layoutRect.width, WorldBottomBarHeight);
        Rect boardRect = new Rect(layoutRect.x, gnbRect.yMax + 14f, layoutRect.width, bnbRect.y - gnbRect.yMax - 28f);
        ExpeditionPrepSurfaceData expeditionPrep = _bootEntry.GetSelectedExpeditionPrepSurfaceData();
        ExpeditionPostRunRevealState postRunReveal = GetPostRunRevealState();
        SyncCityHubPresentationState(expeditionPrep, postRunReveal);

        List<Rect> blockingRects = new List<Rect>();
        blockingRects.Add(gnbRect);
        blockingRects.Add(bnbRect);

        DrawRect(new Rect(0f, 0f, layoutRect.xMax + 24f, 120f), new Color(0.10f, 0.16f, 0.19f, 0.14f));
        DrawRect(new Rect(0f, Screen.height - 140f, layoutRect.xMax + 24f, 140f), new Color(0.03f, 0.04f, 0.07f, 0.18f));
        DrawRotatedRect(new Rect(boardRect.x - 18f, boardRect.y + 28f, boardRect.width + 36f, 14f), -4f, new Color(0.18f, 0.26f, 0.30f, 0.08f));
        DrawRotatedRect(new Rect(boardRect.x + 20f, boardRect.yMax - 42f, boardRect.width - 40f, 10f), 5f, new Color(0.92f, 0.75f, 0.32f, 0.05f));

        if (_cityHubPresentationState.ActiveModal == PrototypeCityHubModalState.ExpeditionPrepBoard)
        {
            DrawExpeditionPrepBoard(boardRect, expeditionPrep);
            blockingRects.Add(boardRect);
        }
        else if (_cityHubPresentationState.ActiveModal == PrototypeCityHubModalState.PostRunRevealSpotlight)
        {
            DrawExpeditionPostRunRevealSpotlight(boardRect, postRunReveal);
            blockingRects.Add(boardRect);
        }
        else if (_cityHubPresentationState.IsBoardOverlayEnabled)
        {
            PrototypeCityHubUiSurfaceData cityHubUi = GetCityHubUiSurfaceData();
            DrawWorldBoardFrame(
                boardRect,
                BuildWorldBoardOverlayTitle(cityHubUi),
                BuildWorldBoardOverlaySubtitle(cityHubUi),
                BuildWorldBoardOverlayFooter(cityHubUi));
        }
        Rect dropdownRect = DrawWorldTopBar(gnbRect);
        if (dropdownRect.width > 0f && dropdownRect.height > 0f)
        {
            blockingRects.Add(dropdownRect);
        }

        Rect bottomSheetRect = DrawWorldBottomSheet(layoutRect, bnbRect);
        if (bottomSheetRect.width > 0f && bottomSheetRect.height > 0f)
        {
            blockingRects.Add(bottomSheetRect);
        }

        DrawWorldBottomBar(bnbRect);
        _blockingRects = blockingRects.ToArray();
    }

    private Rect DrawWorldTopBar(Rect rect)
    {
        DrawPanel(rect, new Color(0.07f, 0.10f, 0.14f, 0.98f), new Color(0.10f, 0.14f, 0.18f, 0.94f));

        PrototypeCityHubUiSurfaceData cityHubUi = GetCityHubUiSurfaceData();
        string selectionName = HasMeaningfulValue(cityHubUi.Header.SelectionLabel) ? V(cityHubUi.Header.SelectionLabel) : T("FrontWorldHeadline");
        GUI.Label(new Rect(rect.x + 16f, rect.y + 10f, 300f, 24f), selectionName, _panelTitleStyle);
        GUI.Label(new Rect(rect.x + 16f, rect.y + 34f, 320f, 18f), V(cityHubUi.Header.LastTransitionText), _captionStyle);

        float metricsX = rect.x + 338f;
        float metricWidth = 132f;
        float metricGap = 8f;
        DrawMetricPill(new Rect(metricsX, rect.y + 10f, metricWidth, 42f), T("WorldDay"), cityHubUi.Header.WorldDayCount.ToString(), new Color(0.15f, 0.23f, 0.28f, 0.94f));
        DrawMetricPill(new Rect(metricsX + metricWidth + metricGap, rect.y + 10f, metricWidth, 42f), T("FrontActionAutoTick"), cityHubUi.Header.AutoTickEnabled ? T("BoolOn") : T("BoolOff"), new Color(0.13f, 0.20f, 0.18f, 0.94f));
        DrawMetricPill(new Rect(metricsX + ((metricWidth + metricGap) * 2f), rect.y + 10f, metricWidth, 42f), T("ActiveExpeditions"), cityHubUi.Header.ActiveExpeditions.ToString(), new Color(0.18f, 0.16f, 0.12f, 0.94f));
        DrawMetricPill(new Rect(metricsX + ((metricWidth + metricGap) * 3f), rect.y + 10f, metricWidth, 42f), T("IdleParties"), cityHubUi.Header.IdleParties.ToString(), new Color(0.14f, 0.17f, 0.22f, 0.94f));

        float utilityRight = rect.xMax - 16f;
        Rect langRect = new Rect(utilityRight - 118f, rect.y + 12f, 118f, 30f);
        DrawLanguageButtons(langRect, 55f, 8f, false);
        bool canToggleCityPanels = !_cityHubPresentationState.HasBlockingModal;

        Rect escBadgeRect = new Rect(langRect.x - 48f, rect.y + 14f, 40f, 24f);
        DrawPill(escBadgeRect, "Esc", new Color(0.18f, 0.20f, 0.24f, 0.96f), new Color(0.90f, 0.92f, 0.96f, 1f));

        float overlayWidth = GetCompactTabWidth(_cityHubPresentationState.IsBoardOverlayEnabled ? T("FrontActionOverlayOn") : T("FrontActionOverlayOff"), 132f, 178f);
        Rect overlayRect = new Rect(escBadgeRect.x - overlayWidth - 8f, rect.y + 8f, overlayWidth, 36f);
        if (DrawActionButton(overlayRect, _cityHubPresentationState.IsBoardOverlayEnabled ? T("FrontActionOverlayOn") : T("FrontActionOverlayOff"), _cityHubPresentationState.IsBoardOverlayEnabled ? new Color(0.20f, 0.36f, 0.44f, 1f) : new Color(0.12f, 0.16f, 0.20f, 1f), canToggleCityPanels, _badgeStyle))
        {
            ToggleBoardOverlay();
        }

        float menuWidth = GetCompactTabWidth(T("FrontActionMainMenu"), 128f, 176f);
        Rect menuRect = new Rect(overlayRect.x - menuWidth - 8f, rect.y + 8f, menuWidth, 36f);
        if (DrawActionButton(menuRect, T("FrontActionMainMenu"), new Color(0.22f, 0.18f, 0.14f, 1f), true, _badgeStyle))
        {
            ResetCityHubPresentationState("MainMenu return requested.");
            _bootEntry.ReturnToMainMenu();
        }

        string snapshotLabel = T("PanelWorldSnapshot");
        string selectionLabel = T("FrontWorldSelection");
        string operationsLabel = T("FrontWorldOverview");
        float tabsY = rect.y + 58f;
        float tabGap = 8f;
        float tabHeight = 28f;
        float snapshotWidth = GetCompactTabWidth(snapshotLabel, 140f, 204f);
        float selectionWidth = GetCompactTabWidth(selectionLabel, 146f, 218f);
        float operationsWidth = GetCompactTabWidth(operationsLabel, 198f, 280f);
        Rect snapshotRect = new Rect(rect.x + 16f, tabsY, snapshotWidth, tabHeight);
        Rect selectionRect = new Rect(snapshotRect.xMax + tabGap, tabsY, selectionWidth, tabHeight);
        Rect operationsRect = new Rect(selectionRect.xMax + tabGap, tabsY, operationsWidth, tabHeight);

        if (DrawNavToggle(snapshotRect, snapshotLabel, _cityHubPresentationState.ActiveTopDropdown == PrototypeCityHubTopDropdownMode.Snapshot, canToggleCityPanels))
        {
            ToggleTopDropdown(PrototypeCityHubTopDropdownMode.Snapshot);
        }

        if (DrawNavToggle(selectionRect, selectionLabel, _cityHubPresentationState.ActiveTopDropdown == PrototypeCityHubTopDropdownMode.Selection, canToggleCityPanels))
        {
            ToggleTopDropdown(PrototypeCityHubTopDropdownMode.Selection);
        }

        if (DrawNavToggle(operationsRect, operationsLabel, _cityHubPresentationState.ActiveTopDropdown == PrototypeCityHubTopDropdownMode.Operations, canToggleCityPanels))
        {
            ToggleTopDropdown(PrototypeCityHubTopDropdownMode.Operations);
        }

        if (_cityHubPresentationState.ActiveTopDropdown == PrototypeCityHubTopDropdownMode.None)
        {
            return Rect.zero;
        }

        Rect anchorRect = _cityHubPresentationState.ActiveTopDropdown == PrototypeCityHubTopDropdownMode.Selection
            ? selectionRect
            : _cityHubPresentationState.ActiveTopDropdown == PrototypeCityHubTopDropdownMode.Operations
                ? operationsRect
                : snapshotRect;
        float dropdownWidth = Mathf.Clamp(rect.width * 0.36f, 400f, 520f);
        float dropdownX = Mathf.Clamp(anchorRect.x, rect.x, rect.xMax - dropdownWidth);
        Rect dropdownRect = new Rect(dropdownX, rect.yMax + 8f, dropdownWidth, WorldDropdownHeight);
        DrawPanel(dropdownRect, new Color(0.08f, 0.11f, 0.14f, 0.98f), new Color(0.10f, 0.14f, 0.18f, 0.96f));
        Rect titleRect = new Rect(dropdownRect.x + 16f, dropdownRect.y + 14f, dropdownRect.width - 32f, 26f);
        Rect bodyRect = new Rect(dropdownRect.x + 16f, dropdownRect.y + 46f, dropdownRect.width - 32f, dropdownRect.height - 60f);

        string dropdownTitle = _cityHubPresentationState.ActiveTopDropdown == PrototypeCityHubTopDropdownMode.Selection
            ? selectionLabel
            : _cityHubPresentationState.ActiveTopDropdown == PrototypeCityHubTopDropdownMode.Operations
                ? operationsLabel
                : snapshotLabel;
        GUI.Label(titleRect, dropdownTitle, _panelTitleStyle);
        GUI.Label(bodyRect, GetTopDropdownBody(_cityHubPresentationState.ActiveTopDropdown), _bodyStyle);
        return dropdownRect;
    }

    private Rect DrawWorldBottomSheet(Rect layoutRect, Rect bnbRect)
    {
        if (_cityHubPresentationState.ActiveBottomSheet == PrototypeCityHubBottomSheetMode.None)
        {
            return Rect.zero;
        }

        Rect sheetRect = new Rect(layoutRect.x, bnbRect.y - WorldBottomSheetHeight - 10f, layoutRect.width, WorldBottomSheetHeight);
        DrawPanel(sheetRect, new Color(0.07f, 0.10f, 0.14f, 0.98f), new Color(0.10f, 0.14f, 0.18f, 0.94f));
        GUI.Label(new Rect(sheetRect.x + 16f, sheetRect.y + 12f, sheetRect.width - 32f, 20f), GetBottomSheetTitle(_cityHubPresentationState.ActiveBottomSheet), _panelTitleStyle);

        Rect contentRect = new Rect(sheetRect.x + 16f, sheetRect.y + 40f, sheetRect.width - 32f, sheetRect.height - 52f);
        if (_cityHubPresentationState.ActiveBottomSheet == PrototypeCityHubBottomSheetMode.Actions)
        {
            DrawWorldActionSheet(contentRect);
        }
        else
        {
            GUI.Label(contentRect, GetBottomSheetBody(_cityHubPresentationState.ActiveBottomSheet), _bodyStyle);
        }

        return sheetRect;
    }

    private void DrawWorldBottomBar(Rect rect)
    {
        DrawPanel(rect, new Color(0.07f, 0.10f, 0.14f, 0.98f), new Color(0.10f, 0.14f, 0.18f, 0.94f));

        float tabGap = 10f;
        float tabWidth = (rect.width - 32f - (tabGap * 3f)) / 4f;
        float tabY = rect.y + 12f;
        Rect actionsRect = new Rect(rect.x + 16f, tabY, tabWidth, 32f);
        Rect selectionRect = new Rect(actionsRect.xMax + tabGap, tabY, tabWidth, 32f);
        Rect overviewRect = new Rect(selectionRect.xMax + tabGap, tabY, tabWidth, 32f);
        Rect logsRect = new Rect(overviewRect.xMax + tabGap, tabY, tabWidth, 32f);
        bool canToggleCityPanels = !_cityHubPresentationState.HasBlockingModal;

        if (DrawNavToggle(actionsRect, T("FrontWorldActions"), _cityHubPresentationState.ActiveBottomSheet == PrototypeCityHubBottomSheetMode.Actions, canToggleCityPanels))
        {
            ToggleBottomSheet(PrototypeCityHubBottomSheetMode.Actions);
        }

        if (DrawNavToggle(selectionRect, T("FrontWorldSelection"), _cityHubPresentationState.ActiveBottomSheet == PrototypeCityHubBottomSheetMode.Selection, canToggleCityPanels))
        {
            ToggleBottomSheet(PrototypeCityHubBottomSheetMode.Selection);
        }

        if (DrawNavToggle(overviewRect, T("FrontWorldOverview"), _cityHubPresentationState.ActiveBottomSheet == PrototypeCityHubBottomSheetMode.Overview, canToggleCityPanels))
        {
            ToggleBottomSheet(PrototypeCityHubBottomSheetMode.Overview);
        }

        if (DrawNavToggle(logsRect, T("FilterLogs"), _cityHubPresentationState.ActiveBottomSheet == PrototypeCityHubBottomSheetMode.Logs, canToggleCityPanels))
        {
            ToggleBottomSheet(PrototypeCityHubBottomSheetMode.Logs);
        }
    }

    private void DrawWorldBoardFrame(Rect rect, string title = null, string subtitle = null, string footer = null)
    {
        DrawRect(new Rect(rect.x + 4f, rect.y + 6f, rect.width, rect.height), new Color(0f, 0f, 0f, 0.18f));
        DrawRect(rect, new Color(0.05f, 0.07f, 0.10f, 0.24f));
        DrawRect(Inset(rect, 2f), new Color(0.12f, 0.16f, 0.20f, 0.10f));
        Rect innerRect = Inset(rect, 8f);
        DrawRect(innerRect, new Color(0.03f, 0.05f, 0.06f, 0.05f));
        DrawRect(new Rect(innerRect.x, innerRect.y, innerRect.width, 48f), new Color(0.04f, 0.06f, 0.08f, 0.34f));
        DrawRect(new Rect(innerRect.x, innerRect.yMax - 22f, innerRect.width, 22f), new Color(0.04f, 0.06f, 0.08f, 0.20f));
        DrawRect(new Rect(innerRect.x, innerRect.y, innerRect.width, 2f), new Color(0.88f, 0.74f, 0.36f, 0.30f));
        DrawRect(new Rect(innerRect.x, innerRect.yMax - 2f, innerRect.width, 2f), new Color(0.18f, 0.52f, 0.60f, 0.22f));

        Rect titleRect = new Rect(innerRect.x + 14f, innerRect.y + 10f, innerRect.width - 28f, 24f);
        Rect subtitleRect = new Rect(titleRect.x, titleRect.yMax + 4f, titleRect.width, 30f);
        Rect footerRect = new Rect(innerRect.x + 14f, innerRect.yMax - 18f, innerRect.width - 28f, 16f);
        GUI.Label(titleRect, string.IsNullOrEmpty(title) ? T("FrontWorldBoardOverlay") : title, _panelTitleStyle);
        GUI.Label(subtitleRect, string.IsNullOrEmpty(subtitle) ? T("FrontWorldOverlayReason") : subtitle, _captionStyle);
        GUI.Label(footerRect, string.IsNullOrEmpty(footer) ? BuildWorldLegendText() : footer, _captionStyle);

        float pinSize = 10f;
        DrawRect(new Rect(innerRect.x + 8f, innerRect.y + 8f, pinSize, pinSize), new Color(0.84f, 0.72f, 0.34f, 0.72f));
        DrawRect(new Rect(innerRect.xMax - 18f, innerRect.y + 8f, pinSize, pinSize), new Color(0.36f, 0.74f, 0.86f, 0.72f));
        DrawRect(new Rect(innerRect.x + 8f, innerRect.yMax - 18f, pinSize, pinSize), new Color(0.80f, 0.42f, 0.46f, 0.72f));
        DrawRect(new Rect(innerRect.xMax - 18f, innerRect.yMax - 18f, pinSize, pinSize), new Color(0.48f, 0.78f, 0.46f, 0.72f));
    }

    private void DrawWorldActionSheet(Rect rect)
    {
        PrototypeCityHubUiSurfaceData cityHubUi = GetCityHubUiSurfaceData();
        PrototypeCityHubActionSurfaceData actionSurface = cityHubUi.Actions;
        PrototypeCityHubRecruitmentBoardSurfaceData recruitmentBoard = cityHubUi.RecruitmentBoard;
        PrototypeCityHubPartyRosterBoardSurfaceData partyRosterBoard = cityHubUi.PartyRosterBoard;
        ExpeditionPrepSurfaceData expeditionPrep = _bootEntry.GetSelectedExpeditionPrepSurfaceData();
        ExpeditionPostRunRevealState postRunReveal = GetPostRunRevealState();
        bool hasCitySelection = cityHubUi.HasSelectedCity;
        bool boardOpen = actionSurface.IsExpeditionPrepBoardOpen;
        bool canEnterDungeon = actionSurface.CanEnterDungeon;
        bool autoTickEnabled = _bootEntry.AutoTickEnabled;
        bool hasBriefing = HasMeaningfulValue(actionSurface.DispatchBriefingSummaryText);
        string expeditionActionLabel = boardOpen
            ? "[X] Launch Expedition"
            : postRunReveal != null && postRunReveal.HasPendingReveal && postRunReveal.CanOpenExpeditionPrep
                ? "[X] Review Return"
                : "[X] Expedition Prep";
        float gap = 10f;
        float width = (rect.width - (gap * 5f)) / 6f;
        float buttonHeight = 44f;
        Rect runDayRect = new Rect(rect.x, rect.y + 4f, width, buttonHeight);
        Rect recruitRect = new Rect(runDayRect.xMax + gap, rect.y + 4f, width, buttonHeight);
        Rect enterRect = new Rect(recruitRect.xMax + gap, rect.y + 4f, width, buttonHeight);
        Rect autoRect = new Rect(enterRect.xMax + gap, rect.y + 4f, width, buttonHeight);
        Rect pauseRect = new Rect(autoRect.xMax + gap, rect.y + 4f, width, buttonHeight);
        Rect resetRect = new Rect(pauseRect.xMax + gap, rect.y + 4f, width, buttonHeight);
        Rect hintRect = new Rect(rect.x, runDayRect.yMax + 12f, rect.width, rect.height - buttonHeight - 16f);

        if (DrawActionButton(runDayRect, "[T] " + T("FrontActionRunDay"), new Color(0.24f, 0.46f, 0.58f, 1f), true))
        {
            _bootEntry.RunWorldDayStep();
        }

        if (DrawActionButton(recruitRect, "[G] " + T("FrontActionRecruit"), new Color(0.24f, 0.36f, 0.26f, 1f), hasCitySelection && actionSurface.CanRecruitParty))
        {
            TryExecuteCityHubAction(PrototypeCityHubActionKeys.RecruitSelectedCityParty, "WorldActionSheet");
        }

        if (DrawActionButton(enterRect, expeditionActionLabel, new Color(0.44f, 0.30f, 0.18f, 1f), canEnterDungeon))
        {
            TryExecuteCityHubAction(PrototypeCityHubActionKeys.EnterSelectedWorldDungeon, "WorldActionSheet");
        }

        if (DrawActionButton(autoRect, "[Y] " + T("FrontActionAutoTick"), autoTickEnabled ? new Color(0.20f, 0.44f, 0.42f, 1f) : new Color(0.18f, 0.24f, 0.34f, 1f), true))
        {
            _bootEntry.ToggleWorldAutoTick();
        }

        if (DrawActionButton(pauseRect, "[P] " + (autoTickEnabled && _bootEntry.AutoTickPaused ? T("FrontActionResume") : T("FrontActionPause")), new Color(0.28f, 0.22f, 0.16f, 1f), autoTickEnabled))
        {
            _bootEntry.ToggleWorldAutoTickPause();
        }

        if (DrawActionButton(resetRect, "[R] " + T("FrontActionResetWorld"), new Color(0.18f, 0.19f, 0.23f, 1f), true))
        {
            _bootEntry.ResetWorldSimulation();
        }

        string hintText = boardOpen
            ? "Dispatch Policy: " + V(expeditionPrep.DispatchPolicyText) + "\n" +
              "Launch Gate: " + V(expeditionPrep.LaunchGateSummaryText) + "\n" +
              "Next Action: " + V(expeditionPrep.RecommendedNextActionText) + "\n" +
              "Board Feedback: " + V(expeditionPrep.FeedbackText) + "\n\n" +
              "[Q] Cycle Policy  [1] Route 1  [2] Route 2  [Enter] Launch  [Esc] Cancel"
            : postRunReveal != null && postRunReveal.HasPendingReveal
            ? BuildExpeditionPostRunRevealHintText(postRunReveal)
            : hasBriefing
            ? Line(T("DispatchBriefing"), V(actionSurface.DispatchBriefingSummaryText)) + "\n" +
              Line("Recruitment", V(recruitmentBoard.ActionText)) + "\n" +
              Line("Party Roster", V(partyRosterBoard.AvailabilitySummaryText)) + "\n" +
              Line(T("DispatchParty"), V(actionSurface.DispatchPartySummaryText)) + "\n" +
              Line(T("RouteFit"), V(actionSurface.RouteFitSummaryText)) + "\n" +
              Line(T("LaunchLock"), V(actionSurface.LaunchLockSummaryText)) + "\n" +
              Line(T("ProjectedOutcome"), V(actionSurface.ProjectedOutcomeSummaryText)) + "\n" +
              Line(T("ActiveExpeditionLane"), V(actionSurface.ActiveExpeditionLaneText)) + "\n" +
              Line(T("CityVacancy"), V(actionSurface.CityVacancyText)) + "\n" +
              Line(T("ReturnEta"), V(actionSurface.ReturnEtaText)) + "\n\n" +
              V(_bootEntry.ExpeditionControlsLabel)
            : V(_bootEntry.EconomyControlsLabel) + "\n" + V(_bootEntry.ExpeditionControlsLabel);
        GUI.Label(hintRect, hintText, _captionStyle);
    }

    private void DrawExpeditionPostRunRevealSpotlight(Rect rect, ExpeditionPostRunRevealState reveal)
    {
        string subtitle = V(reveal.CityLabel) + " -> " + V(reveal.DungeonLabel);
        string footer = reveal.CanOpenExpeditionPrep
            ? "[X] Review Return  [Esc] Dismiss"
            : "[Esc] Dismiss | Focus pinned to return target";
        DrawWorldBoardFrame(rect, V(reveal.HeadlineText), subtitle, footer);

        Rect innerRect = Inset(rect, 16f);
        Rect contentRect = new Rect(innerRect.x + 18f, innerRect.y + 58f, innerRect.width - 36f, innerRect.height - 96f);
        Rect bodyRect = new Rect(contentRect.x, contentRect.y, contentRect.width, contentRect.height - 52f);
        Rect actionsRect = new Rect(contentRect.x, bodyRect.yMax + 12f, contentRect.width, 40f);

        DrawPanel(bodyRect, new Color(0.28f, 0.22f, 0.14f, 0.96f), new Color(0.09f, 0.10f, 0.14f, 0.94f));
        GUI.Label(new Rect(bodyRect.x + 14f, bodyRect.y + 12f, bodyRect.width - 28f, 22f), "One-Shot Return Reveal", _panelTitleStyle);
        DrawScrollableTextBlock(
            new Rect(bodyRect.x + 14f, bodyRect.y + 40f, bodyRect.width - 28f, bodyRect.height - 54f),
            "post_run_reveal:spotlight",
            BuildExpeditionPostRunRevealBodyText(reveal),
            _bodyStyle);

        float gap = 10f;
        float buttonWidth = (actionsRect.width - gap) * 0.5f;
        Rect reviewRect = new Rect(actionsRect.x, actionsRect.y, buttonWidth, actionsRect.height);
        Rect dismissRect = new Rect(reviewRect.xMax + gap, actionsRect.y, buttonWidth, actionsRect.height);

        if (DrawActionButton(reviewRect, "[X] Review Return", new Color(0.42f, 0.30f, 0.18f, 1f), reveal.CanOpenExpeditionPrep))
        {
            TryExecuteCityHubAction(PrototypeCityHubActionKeys.EnterSelectedWorldDungeon, "PostRunRevealSpotlight");
        }

        if (DrawActionButton(dismissRect, "[Esc] Dismiss", new Color(0.20f, 0.20f, 0.24f, 1f), true))
        {
            _bootEntry.AcknowledgePendingExpeditionPostRunReveal();
        }
    }

    private void DrawExpeditionPrepBoard(Rect rect, ExpeditionPrepSurfaceData data)
    {
        string subtitle = V(data.CityLabel) + " -> " + V(data.DungeonLabel) + " | Party: " + V(data.PartyLabel);
        string footer = "[Q] Policy  [1] Route 1  [2] Route 2  [Enter] Launch  [Esc] Cancel";
        DrawWorldBoardFrame(rect, data.BoardTitleText, subtitle, footer);

        Rect innerRect = Inset(rect, 16f);
        Rect contentRect = new Rect(innerRect.x + 14f, innerRect.y + 56f, innerRect.width - 28f, innerRect.height - 86f);
        float columnGap = 16f;
        float leftWidth = Mathf.Clamp(contentRect.width * 0.48f, 320f, 520f);
        Rect leftRect = new Rect(contentRect.x, contentRect.y, leftWidth, contentRect.height);
        Rect rightRect = new Rect(leftRect.xMax + columnGap, contentRect.y, contentRect.width - leftWidth - columnGap, contentRect.height);

        Rect summaryRect = new Rect(leftRect.x, leftRect.y, leftRect.width, 116f);
        Rect route1Rect = new Rect(leftRect.x, summaryRect.yMax + 12f, leftRect.width, 126f);
        Rect route2Rect = new Rect(leftRect.x, route1Rect.yMax + 10f, leftRect.width, 126f);
        Rect loadoutRect = new Rect(leftRect.x, route2Rect.yMax + 12f, leftRect.width, Mathf.Max(92f, leftRect.yMax - route2Rect.yMax - 12f));

        DrawPanel(summaryRect, new Color(0.18f, 0.28f, 0.34f, 0.96f), new Color(0.10f, 0.13f, 0.18f, 0.94f));
        GUI.Label(new Rect(summaryRect.x + 12f, summaryRect.y + 10f, summaryRect.width - 24f, 24f), "Launch Readiness", _panelTitleStyle);
        DrawScrollableTextBlock(
            new Rect(summaryRect.x + 12f, summaryRect.y + 40f, summaryRect.width - 24f, summaryRect.height - 50f),
            "expedition_prep:summary",
            "Need Pressure: " + V(data.NeedPressureText) + "\n" +
            "Readiness: " + V(data.DispatchReadinessText) + "\n" +
            "Policy: " + V(data.DispatchPolicyText) + "\n" +
            "Recovery: " + V(data.RecoveryProgressText) + " | ETA " + V(data.RecoveryEtaText),
            _bodyStyle);

        if (data.RouteOptions.Length > 0)
        {
            DrawExpeditionPrepRouteOptionCard(route1Rect, data.RouteOptions[0], "[1]", true);
        }

        if (data.RouteOptions.Length > 1)
        {
            DrawExpeditionPrepRouteOptionCard(route2Rect, data.RouteOptions[1], "[2]", true);
        }

        DrawPanel(loadoutRect, new Color(0.16f, 0.26f, 0.22f, 0.96f), new Color(0.09f, 0.12f, 0.15f, 0.94f));
        GUI.Label(new Rect(loadoutRect.x + 12f, loadoutRect.y + 10f, loadoutRect.width - 24f, 24f), "Staged Party", _panelTitleStyle);
        DrawScrollableTextBlock(
            new Rect(loadoutRect.x + 12f, loadoutRect.y + 40f, loadoutRect.width - 24f, loadoutRect.height - 50f),
            "expedition_prep:loadout",
            "Staged Summary: " + V(data.StagedPartySummaryText) + "\n" +
            "Loadout: " + V(data.PartyLoadoutSummaryText) + "\n" +
            "Launch Manifest: " + BuildExpeditionPrepLaunchManifestText(data.StartContext) + "\n" +
            "Members:\n" + BuildExpeditionPrepMemberManifestText(data.StartContext),
            _bodyStyle);

        float infoHeight = 140f;
        Rect readinessRect = new Rect(rightRect.x, rightRect.y, rightRect.width, infoHeight);
        Rect recommendationRect = new Rect(rightRect.x, readinessRect.yMax + 12f, rightRect.width, 116f);
        Rect previewRect = new Rect(rightRect.x, recommendationRect.yMax + 12f, rightRect.width, 134f);
        Rect feedbackRect = new Rect(rightRect.x, previewRect.yMax + 12f, rightRect.width, Mathf.Max(92f, rightRect.yMax - previewRect.yMax - 60f));
        Rect actionsRect = new Rect(rightRect.x, rightRect.yMax - 40f, rightRect.width, 40f);

        DrawPanel(readinessRect, new Color(0.30f, 0.24f, 0.16f, 0.96f), new Color(0.10f, 0.11f, 0.14f, 0.94f));
        GUI.Label(new Rect(readinessRect.x + 12f, readinessRect.y + 10f, readinessRect.width - 24f, 24f), "Launch Gate", _panelTitleStyle);
        DrawScrollableTextBlock(
            new Rect(readinessRect.x + 12f, readinessRect.y + 40f, readinessRect.width - 24f, readinessRect.height - 50f),
            "expedition_prep:gate",
            "Selected Route: " + V(data.SelectedRouteLabel) + "\n" +
            "Recommended Route: " + V(data.RecommendedRouteLabel) + "\n" +
            "Gate: " + V(data.LaunchGateSummaryText) + "\n" +
            "Blocked Reason: " + V(data.BlockedReasonText),
            _bodyStyle);

        DrawPanel(recommendationRect, new Color(0.18f, 0.24f, 0.34f, 0.96f), new Color(0.08f, 0.11f, 0.15f, 0.94f));
        GUI.Label(new Rect(recommendationRect.x + 12f, recommendationRect.y + 10f, recommendationRect.width - 24f, 24f), "Recommendation", _panelTitleStyle);
        DrawScrollableTextBlock(
            new Rect(recommendationRect.x + 12f, recommendationRect.y + 40f, recommendationRect.width - 24f, recommendationRect.height - 50f),
            "expedition_prep:recommendation",
            "Reason: " + V(data.RecommendationReasonText) + "\n" +
            "Expected Need Impact: " + V(data.ExpectedNeedImpactText),
            _bodyStyle);

        DrawPanel(previewRect, new Color(0.22f, 0.30f, 0.18f, 0.96f), new Color(0.08f, 0.11f, 0.14f, 0.94f));
        GUI.Label(new Rect(previewRect.x + 12f, previewRect.y + 10f, previewRect.width - 24f, 24f), "Projected Preview", _panelTitleStyle);
        DrawScrollableTextBlock(
            new Rect(previewRect.x + 12f, previewRect.y + 40f, previewRect.width - 24f, previewRect.height - 50f),
            "expedition_prep:preview",
            "Route Fit: " + V(data.RouteFitSummaryText) + "\n" +
            "Route Preview: " + V(data.RoutePreviewSummaryText) + "\n" +
            "Reward Preview: " + V(data.RewardPreviewText) + "\n" +
            "Event Preview: " + V(data.EventPreviewText),
            _bodyStyle);

        DrawPanel(feedbackRect, new Color(0.24f, 0.18f, 0.26f, 0.96f), new Color(0.09f, 0.10f, 0.14f, 0.94f));
        GUI.Label(new Rect(feedbackRect.x + 12f, feedbackRect.y + 10f, feedbackRect.width - 24f, 24f), "Return Consume", _panelTitleStyle);
        DrawScrollableTextBlock(
            new Rect(feedbackRect.x + 12f, feedbackRect.y + 40f, feedbackRect.width - 24f, feedbackRect.height - 50f),
            "expedition_prep:return_consume",
            BuildExpeditionPrepReturnConsumeText(data),
            _bodyStyle);

        float actionGap = 10f;
        float actionWidth = (actionsRect.width - (actionGap * 2f)) / 3f;
        Rect policyRect = new Rect(actionsRect.x, actionsRect.y, actionWidth, actionsRect.height);
        Rect launchRect = new Rect(policyRect.xMax + actionGap, actionsRect.y, actionWidth, actionsRect.height);
        Rect cancelRect = new Rect(launchRect.xMax + actionGap, actionsRect.y, actionWidth, actionsRect.height);

        if (DrawActionButton(policyRect, "[Q] Cycle Policy", new Color(0.24f, 0.34f, 0.42f, 1f), data.CanCycleDispatchPolicy))
        {
            TryExecuteCityHubAction(PrototypeCityHubActionKeys.CycleExpeditionPrepDispatchPolicy, "ExpeditionPrepBoard");
        }

        if (DrawActionButton(launchRect, "[Enter] Launch", new Color(0.42f, 0.28f, 0.18f, 1f), data.CanConfirmLaunch))
        {
            TryExecuteCityHubAction(PrototypeCityHubActionKeys.ConfirmSelectedExpeditionLaunch, "ExpeditionPrepBoard");
        }

        if (DrawActionButton(cancelRect, "[Esc] Cancel", new Color(0.22f, 0.18f, 0.18f, 1f), true))
        {
            TryExecuteCityHubAction(PrototypeCityHubActionKeys.CancelSelectedWorldExpeditionPrepBoard, "ExpeditionPrepBoard");
        }
    }

    private void DrawExpeditionPrepRouteOptionCard(Rect rect, ExpeditionPrepRouteOptionData option, string hotkeyLabel, bool isEnabled)
    {
        Color borderColor = option.IsSelected
            ? new Color(0.86f, 0.72f, 0.34f, 0.96f)
            : option.IsRecommended
                ? new Color(0.32f, 0.64f, 0.78f, 0.96f)
                : new Color(0.18f, 0.22f, 0.28f, 0.96f);
        DrawPanel(rect, borderColor, new Color(0.09f, 0.12f, 0.16f, 0.94f));
        GUI.Label(new Rect(rect.x + 12f, rect.y + 10f, rect.width - 24f, 24f), hotkeyLabel + " " + V(option.OptionLabel), _panelTitleStyle);
        Rect buttonRect = new Rect(rect.x + 12f, rect.yMax - 34f, rect.width - 24f, 24f);
        float bodyHeight = Mathf.Max(20f, buttonRect.y - 8f - (rect.y + 40f));
        Rect bodyRect = new Rect(rect.x + 12f, rect.y + 40f, rect.width - 24f, bodyHeight);
        int maxLines = Mathf.Clamp(Mathf.FloorToInt((bodyRect.height - 4f) / 18f), 2, 4);
        GUI.Label(
            bodyRect,
            BuildDisplayBlock(
                V(option.RouteRiskText) + "\n" +
                V(option.RoutePreviewText) + "\n" +
                "Reward Mix: " + V(option.RewardPreviewText),
                maxLines,
                rect.width >= 420f ? 78 : 58),
            _bodyStyle);
        string buttonLabel = option.IsSelected
            ? "Selected"
            : option.IsRecommended
                ? "Follow Recommendation"
                : "Select Route";
        if (DrawActionButton(buttonRect, buttonLabel, borderColor, isEnabled))
        {
            TryExecuteCityHubAction(PrototypeCityHubActionKeys.SelectExpeditionPrepRoute, "ExpeditionPrepRouteCard", option.OptionKey);
        }
    }

    private string BuildExpeditionPrepLaunchManifestText(ExpeditionStartContext startContext)
    {
        if (startContext == null || !HasMeaningfulValue(startContext.LaunchManifestSummaryText))
        {
            return "None";
        }

        return V(startContext.LaunchManifestSummaryText);
    }

    private string BuildExpeditionPrepMemberManifestText(ExpeditionStartContext startContext)
    {
        ExpeditionPartyManifest manifest = startContext != null ? startContext.PartyManifest : null;
        if (manifest == null)
        {
            return "None";
        }

        if (HasMeaningfulValue(manifest.MemberSummaryText))
        {
            return V(manifest.MemberSummaryText);
        }

        if (manifest.Members == null || manifest.Members.Length <= 0)
        {
            return "None";
        }

        string summary = string.Empty;
        for (int i = 0; i < manifest.Members.Length; i++)
        {
            ExpeditionPartyMemberManifest member = manifest.Members[i];
            if (member == null)
            {
                continue;
            }

            string roleLabel = HasMeaningfulValue(member.RoleLabel) ? member.RoleLabel : "Adventurer";
            string skillName = HasMeaningfulValue(member.ResolvedSkillName) ? member.ResolvedSkillName : "Skill";
            string line = (HasMeaningfulValue(member.DisplayName) ? member.DisplayName : roleLabel) +
                " [" + roleLabel + "] | Skill " + skillName;
            summary = string.IsNullOrEmpty(summary) ? line : summary + "\n" + line;
        }

        return string.IsNullOrEmpty(summary) ? "None" : summary;
    }

    private string BuildExpeditionPrepReturnConsumeText(ExpeditionPrepSurfaceData data)
    {
        ExpeditionResult result = data != null ? data.LatestExpeditionResult : null;
        string boardFeedbackText = data != null ? data.FeedbackText : "None";
        string nextActionText = data != null ? data.RecommendedNextActionText : "None";
        if (result == null ||
            (!HasMeaningfulValue(result.ResultSummaryText) &&
             !HasMeaningfulValue(result.LatestReturnAftermathSummaryText) &&
             !HasMeaningfulValue(result.NextPrepFollowUpSummaryText)))
        {
            return "Last Result: None\n" +
                "Aftermath: " + V(boardFeedbackText) + "\n" +
                "Follow-Up: " + V(HasMeaningfulValue(nextActionText) ? nextActionText : boardFeedbackText);
        }

        string routeText = HasMeaningfulValue(result.SelectedRouteSummaryText)
            ? result.SelectedRouteSummaryText
            : result.RouteLabel;
        string followedText = HasMeaningfulValue(result.FollowedRecommendationSummaryText)
            ? result.FollowedRecommendationSummaryText
            : result.FollowedRecommendation
                ? "Yes"
                : "No";
        string aftermathText = HasMeaningfulValue(result.LatestReturnAftermathSummaryText)
            ? result.LatestReturnAftermathSummaryText
            : boardFeedbackText;
        string followUpText = HasMeaningfulValue(result.NextPrepFollowUpSummaryText)
            ? result.NextPrepFollowUpSummaryText
            : HasMeaningfulValue(result.NextSuggestedActionText)
                ? result.NextSuggestedActionText
                : nextActionText;
        return "Last Result: " + V(result.ResultSummaryText) + "\n" +
            "Route / Followed: " + V(routeText) + " | " + V(followedText) + "\n" +
            "Loot / Survivors: " + V(result.ReturnedLootSummaryText) + " | " + V(result.SelectedPartySummaryText) + "\n" +
            "Elite / World: " + V(result.EliteOutcomeSummaryText) + " | " + V(result.WorldWritebackSummaryText) + "\n" +
            "Aftermath: " + V(aftermathText) + "\n" +
            "Follow-Up: " + V(followUpText) + "\n" +
            "Board Feedback: " + V(boardFeedbackText);
    }

    private string BuildExpeditionPostRunRevealHintText(ExpeditionPostRunRevealState reveal)
    {
        OutcomeReadback readback = reveal != null ? reveal.OutcomeReadback : new OutcomeReadback();
        ExpeditionResult result = reveal != null ? reveal.LatestExpeditionResult : new ExpeditionResult();
        string nextPrepHintText = HasMeaningfulValue(result != null ? result.NextPrepFollowUpSummaryText : "None")
            ? result.NextPrepFollowUpSummaryText
            : readback != null && HasMeaningfulValue(readback.FollowUpHintText)
                ? readback.FollowUpHintText
                : readback != null
                    ? readback.NextSuggestedActionText
                    : "None";
        return "Return Spotlight: " + V(reveal != null ? reveal.HeadlineText : "None") + "\n" +
            "Aftermath: " + V(readback != null ? readback.LatestReturnAftermathText : "None") + "\n" +
            "Corrective Follow-Up: " + V(readback != null ? readback.NextSuggestedActionText : "None") + "\n" +
            "Next Prep Hint: " + V(nextPrepHintText) + "\n\n" +
            (reveal != null && reveal.CanOpenExpeditionPrep ? "[X] Review Return  [Esc] Dismiss" : "[Esc] Dismiss");
    }

    private string BuildExpeditionPostRunRevealBodyText(ExpeditionPostRunRevealState reveal)
    {
        ExpeditionResult result = reveal != null ? reveal.LatestExpeditionResult : new ExpeditionResult();
        OutcomeReadback readback = reveal != null ? reveal.OutcomeReadback : new OutcomeReadback();
        CityWriteback cityWriteback = reveal != null ? reveal.CityWriteback : new CityWriteback();
        WorldWriteback worldWriteback = reveal != null ? reveal.WorldWriteback : new WorldWriteback();
        string routeText = HasMeaningfulValue(result.SelectedRouteSummaryText) ? result.SelectedRouteSummaryText : worldWriteback.RouteSummaryText;
        string lootText = HasMeaningfulValue(result.ReturnedLootSummaryText) ? result.ReturnedLootSummaryText : cityWriteback.LootSummaryText;
        string survivorsText = HasMeaningfulValue(result.SelectedPartySummaryText) ? result.SelectedPartySummaryText : readback.SurvivingMembersSummaryText;
        string followUpText = HasMeaningfulValue(result.NextPrepFollowUpSummaryText)
            ? result.NextPrepFollowUpSummaryText
            : readback.FollowUpHintText;
        return "Result: " + V(result.ResultSummaryText) + "\n" +
            "City / Dungeon: " + V(reveal != null ? reveal.CityLabel : "None") + " -> " + V(reveal != null ? reveal.DungeonLabel : "None") + "\n" +
            "Route: " + V(routeText) + "\n" +
            "Loot / Survivors: " + V(lootText) + " | " + V(survivorsText) + "\n" +
            "Aftermath: " + V(readback.LatestReturnAftermathText) + "\n" +
            "Changed Best Move: " + V(readback.NextSuggestedActionText) + "\n" +
            "Next Prep Hint: " + V(followUpText) + "\n" +
            "World Writeback: " + V(worldWriteback.WritebackSummaryText);
    }

    private string GetTopDropdownBody(PrototypeCityHubTopDropdownMode mode)
    {
        if (mode == PrototypeCityHubTopDropdownMode.Selection)
        {
            return BuildWorldSelectionDetailBody();
        }

        if (mode == PrototypeCityHubTopDropdownMode.Operations)
        {
            return BuildWorldOperationsBody();
        }

        return BuildWorldSnapshotBody();
    }

    private string GetBottomSheetTitle(PrototypeCityHubBottomSheetMode mode)
    {
        return mode == PrototypeCityHubBottomSheetMode.Selection
            ? T("FrontWorldSelection")
            : mode == PrototypeCityHubBottomSheetMode.Overview
                ? T("FrontWorldOverview")
                : mode == PrototypeCityHubBottomSheetMode.Logs
                    ? T("FilterLogs")
                    : T("FrontWorldActions");
    }

    private string GetBottomSheetBody(PrototypeCityHubBottomSheetMode mode)
    {
        return mode == PrototypeCityHubBottomSheetMode.Selection
            ? BuildWorldSelectionBriefBody()
            : mode == PrototypeCityHubBottomSheetMode.Overview
                ? BuildWorldOverviewBriefBody()
                : BuildWorldLogBody();
    }

    private string BuildWorldSnapshotBody()
    {
        return Line(T("Visible"), _bootEntry.VisibleCityCount + " / " + _bootEntry.VisibleDungeonCount + " / " + _bootEntry.VisibleRoadCount) + "\n" +
               Line(T("WorldDay"), _bootEntry.WorldDayCount.ToString()) + "\n" +
               Line(T("TradeStepCount"), _bootEntry.TradeStepCount.ToString()) + "\n" +
               Line(T("AutoTickEnabled"), _bootEntry.AutoTickEnabled ? T("BoolOn") : T("BoolOff")) + "\n" +
               Line(T("RouteCapacityUsed"), V(_bootEntry.RouteCapacityUsedLabel)) + "\n" +
               Line(T("CitiesWithShortages"), V(_bootEntry.CitiesWithShortagesLabel)) + "\n\n" +
               V(_bootEntry.ControlsLabel);
    }

    private string BuildWorldOperationsBody()
    {
        PrototypeCityHubUiSurfaceData cityHubUi = GetCityHubUiSurfaceData();
        PrototypeCityHubActionSurfaceData actionSurface = cityHubUi.Actions;
        return Line(T("EconomyControls"), V(_bootEntry.EconomyControlsLabel)) + "\n" +
               Line(T("ExpeditionControls"), V(_bootEntry.ExpeditionControlsLabel)) + "\n" +
               Line(T("DispatchBriefing"), V(actionSurface.DispatchBriefingSummaryText)) + "\n" +
               Line(T("RouteFit"), V(actionSurface.RouteFitSummaryText)) + "\n" +
               Line(T("LaunchLock"), V(actionSurface.LaunchLockSummaryText)) + "\n" +
               Line(T("ProjectedOutcome"), V(actionSurface.ProjectedOutcomeSummaryText)) + "\n" +
               Line(T("ActiveExpeditions"), cityHubUi.Overview.ActiveExpeditions.ToString()) + "\n" +
               Line(T("IdleParties"), cityHubUi.Overview.IdleParties.ToString()) + "\n" +
               Line(T("UnmetTotal"), _bootEntry.UnmetTotal.ToString()) + "\n\n" +
               Line(T("LastTransition"), V(cityHubUi.Overview.LastTransitionText));
    }

    private string BuildWorldSelectionBriefBody()
    {
        PrototypeCityHubUiSurfaceData cityHubUi = GetCityHubUiSurfaceData();
        PrototypeCityHubSelectionSurfaceData selection = cityHubUi.Selection;
        if (!HasMeaningfulValue(selection.DisplayName))
        {
            return T("FrontWorldNoSelection");
        }

        if (selection.IsCitySelection)
        {
            return BuildWorldLines(
                V(selection.DisplayName),
                "Pressure Board: " + V(selection.PressureBoardSummaryText),
                "Why City Matters: " + V(selection.WhyCityMattersText),
                "Route Answer: " + V(selection.RecommendedRouteText),
                "Party Readiness: " + V(selection.PartyReadinessSummaryText),
                "Latest Evidence: " + V(selection.RecentResultEvidenceText),
                "Next Action: " + V(cityHubUi.Outcome.CorrectiveFollowUpText),
                BuildWorldReturnHandoffBriefBody(cityHubUi.Outcome));
        }

        return BuildWorldLines(
            V(selection.DisplayName),
            Line(T("DungeonDanger"), V(selection.DungeonDangerText)),
            Line(T("LinkedCity"), V(selection.LinkedCityText)),
            Line(T("LaunchLock"), V(selection.LaunchLockSummaryText)),
            Line(T("DungeonStatus"), V(selection.DungeonStatusText)),
            Line(T("DungeonAvailability"), V(selection.DungeonAvailabilityText)),
            Line(T("ReturnEta"), V(selection.ReturnEtaText)),
            Line(T("RewardPreview"), V(selection.RewardPreviewText)),
            BuildWorldReturnHandoffBriefBody(cityHubUi.Outcome));
    }

    private string BuildWorldSelectionDetailBody()
    {
        PrototypeCityHubUiSurfaceData cityHubUi = GetCityHubUiSurfaceData();
        PrototypeCityHubSelectionSurfaceData selection = cityHubUi.Selection;
        if (!HasMeaningfulValue(selection.DisplayName))
        {
            return T("FrontWorldNoSelection");
        }

        if (selection.IsCitySelection)
        {
            return BuildWorldLines(
                V(selection.DisplayName),
                "Pressure Board: " + V(selection.PressureBoardSummaryText),
                "Why City Matters: " + V(selection.WhyCityMattersText),
                Line(T("SelectedType"), V(selection.TypeLabel)),
                Line(T("LinkedDungeon"), V(selection.LinkedDungeonText)),
                Line(T("NeedPressure"), V(selection.NeedPressureText)),
                Line(T("DispatchReadiness"), V(selection.DispatchReadinessText)),
                Line(T("RecoveryProgress"), V(selection.RecoveryProgressText)),
                "Party Readiness: " + V(selection.PartyReadinessSummaryText),
                Line(T("DispatchPolicy"), V(selection.DispatchPolicyText)),
                "Route Answer: " + V(selection.RecommendedRouteText),
                "Recommendation Reason: " + V(selection.RecommendationReasonText),
                Line(T("RouteFit"), V(cityHubUi.Actions.RouteFitSummaryText)),
                Line(T("LaunchLock"), V(selection.LaunchLockSummaryText)),
                "Latest Evidence: " + V(selection.RecentResultEvidenceText),
                Line(T("ProjectedOutcome"), V(selection.ProjectedOutcomeSummaryText)),
                BuildWorldReturnHandoffDetailBody(cityHubUi.Outcome));
        }

        return BuildWorldLines(
            V(selection.DisplayName),
            Line(T("SelectedType"), V(selection.TypeLabel)),
            Line(T("DungeonDanger"), V(selection.DungeonDangerText)),
            Line(T("LinkedCity"), V(selection.LinkedCityText)),
            Line(T("LaunchLock"), V(selection.LaunchLockSummaryText)),
            Line(T("DungeonStatus"), V(selection.DungeonStatusText)),
            Line(T("DungeonAvailability"), V(selection.DungeonAvailabilityText)),
            Line(T("RewardPreview"), V(selection.RewardPreviewText)),
            Line(T("EventPreview"), V(selection.EventPreviewText)),
            BuildWorldReturnHandoffDetailBody(cityHubUi.Outcome));
    }

    private string BuildWorldOverviewBriefBody()
    {
        PrototypeCityHubOverviewSurfaceData overview = GetCityHubUiSurfaceData().Overview;
        return BuildWorldLines(
            "Priority City: " + V(overview.PriorityCityText),
            "Pressure Board: " + V(overview.PrioritySummaryText),
            "Next Action: " + V(overview.PriorityNextActionText),
            Line(T("WorldDay"), overview.WorldDayCount.ToString()),
            Line(T("TradeStepCount"), overview.TradeStepCount.ToString()),
            Line(T("TotalParties"), overview.TotalParties.ToString()),
            Line(T("IdleParties"), overview.IdleParties.ToString()),
            Line(T("ActiveExpeditions"), overview.ActiveExpeditions.ToString()),
            Line(T("ActiveExpeditionLane"), V(overview.ActiveExpeditionLaneText)),
            Line(T("ReturnEta"), V(overview.ReturnEtaText)),
            Line(T("WorldWriteback"), V(overview.WorldWritebackText)),
            BuildWorldReturnHandoffOverviewBody(GetCityHubUiSurfaceData().Outcome),
            Line(T("CitiesWithShortages"), V(overview.CitiesWithShortagesText)));
    }

    private string BuildWorldLogBody()
    {
        PrototypeCityHubLogSurfaceData logs = GetCityHubUiSurfaceData().Logs;
        return Line(T("RecentWorldWritebackLog1"), V(_bootEntry != null ? _bootEntry.RecentWorldWritebackLog1Label : "None")) + "\n" +
               Line(T("RecentWorldWritebackLog2"), V(_bootEntry != null ? _bootEntry.RecentWorldWritebackLog2Label : "None")) + "\n" +
               Line(T("RecentWorldWritebackLog3"), V(_bootEntry != null ? _bootEntry.RecentWorldWritebackLog3Label : "None")) + "\n" +
               Line(T("RecentExpeditionLog1"), V(_bootEntry != null ? _bootEntry.RecentExpeditionLog1Label : "None")) + "\n" +
               Line(T("RecentExpeditionLog2"), V(_bootEntry != null ? _bootEntry.RecentExpeditionLog2Label : "None")) + "\n" +
               Line(T("RecentExpeditionLog3"), V(_bootEntry != null ? _bootEntry.RecentExpeditionLog3Label : "None")) + "\n" +
               Line(T("DepartureEcho"), V(logs.DepartureEchoText)) + "\n" +
               Line(T("ReturnWindow"), V(logs.ReturnWindowText)) + "\n\n" +
               Line(T("RecentDayLog1"), V(_bootEntry != null ? _bootEntry.RecentDayLog1Label : "None")) + "\n" +
               Line(T("RecentDayLog2"), V(_bootEntry != null ? _bootEntry.RecentDayLog2Label : "None")) + "\n" +
               Line(T("RecentDayLog3"), V(_bootEntry != null ? _bootEntry.RecentDayLog3Label : "None"));
    }

    private string BuildWorldReturnHandoffBriefBody(PrototypeCityHubOutcomeSurfaceData outcome)
    {
        return BuildWorldLines(
            "World Writeback: " + V(outcome != null ? outcome.WorldWritebackText : "None"),
            "Dungeon Status: " + V(outcome != null ? outcome.DungeonStatusText : "None"),
            "Corrective Follow-Up: " + V(outcome != null ? outcome.CorrectiveFollowUpText : "None"));
    }

    private string BuildWorldReturnHandoffDetailBody(PrototypeCityHubOutcomeSurfaceData outcome)
    {
        return BuildWorldLines(
            "Latest Return Aftermath: " + V(outcome != null ? outcome.LatestReturnAftermathText : "None"),
            "Outcome Readback: " + V(outcome != null ? outcome.OutcomeReadbackText : "None"),
            "Loot Returned: " + V(outcome != null ? outcome.LootReturnedText : "None"),
            "Party Outcome: " + V(outcome != null ? outcome.PartyOutcomeText : "None"),
            "World Writeback: " + V(outcome != null ? outcome.WorldWritebackText : "None"),
            "Dungeon Status: " + V(outcome != null ? outcome.DungeonStatusText : "None"),
            "Dungeon Availability: " + V(outcome != null ? outcome.DungeonAvailabilityText : "None"),
            "Stock Reaction: " + V(outcome != null ? outcome.StockReactionText : "None"),
            "Dispatch Readiness: " + V(outcome != null ? outcome.DispatchReadinessText : "None"),
            "Recovery ETA: " + V(outcome != null ? outcome.RecoveryEtaText : "None"),
            "Corrective Follow-Up: " + V(outcome != null ? outcome.CorrectiveFollowUpText : "None"),
            "Follow-Up Hint: " + V(outcome != null ? outcome.FollowUpHintText : "None"));
    }

    private string BuildWorldReturnHandoffOverviewBody(PrototypeCityHubOutcomeSurfaceData outcome)
    {
        return BuildWorldLines(
            "Latest Return Aftermath: " + V(outcome != null ? outcome.LatestReturnAftermathText : "None"),
            "World Writeback: " + V(outcome != null ? outcome.WorldWritebackText : "None"),
            "Corrective Follow-Up: " + V(outcome != null ? outcome.CorrectiveFollowUpText : "None"));
    }

    private string BuildWorldBoardOverlayTitle(PrototypeCityHubUiSurfaceData cityHubUi)
    {
        return cityHubUi != null && HasMeaningfulValue(cityHubUi.AlertRibbon.Title)
            ? cityHubUi.AlertRibbon.Title
            : T("FrontWorldBoardOverlay");
    }

    private string BuildWorldBoardOverlaySubtitle(PrototypeCityHubUiSurfaceData cityHubUi)
    {
        return cityHubUi != null && HasMeaningfulValue(cityHubUi.AlertRibbon.SummaryText)
            ? V(cityHubUi.AlertRibbon.SummaryText)
            : T("FrontWorldOverlayReason");
    }

    private string BuildWorldBoardOverlayFooter(PrototypeCityHubUiSurfaceData cityHubUi)
    {
        return cityHubUi != null && HasMeaningfulValue(cityHubUi.AlertRibbon.FooterText)
            ? cityHubUi.AlertRibbon.FooterText
            : "Selected = bright link | Return = pinned pulse | Relief = calm green | Rebound = warning amber";
    }

    private string BuildWorldLines(params string[] lines)
    {
        if (lines == null || lines.Length == 0)
        {
            return "None";
        }

        List<string> filtered = new List<string>();
        for (int i = 0; i < lines.Length; i++)
        {
            if (HasMeaningfulValue(lines[i]))
            {
                filtered.Add(lines[i]);
            }
        }

        return filtered.Count > 0 ? string.Join("\n", filtered.ToArray()) : "None";
    }

    private string BuildWorldLegendText()
    {
        return T("FrontMenuPillarNetwork") + "  |  " + T("FrontMenuPillarDispatch") + "  |  " + T("FrontMenuPillarEconomy");
    }

    private bool DrawNavToggle(Rect rect, string label, bool active, bool isEnabled)
    {
        Color fillColor = active
            ? new Color(0.20f, 0.42f, 0.52f, 1f)
            : new Color(0.12f, 0.16f, 0.20f, 0.96f);
        return DrawActionButton(rect, label, fillColor, isEnabled, _badgeStyle);
    }

    private void ToggleTopDropdown(PrototypeCityHubTopDropdownMode nextMode)
    {
        if (_cityHubPresentationState.HasBlockingModal)
        {
            return;
        }

        _cityHubPresentationState.ActiveTopDropdown = _cityHubPresentationState.ActiveTopDropdown == nextMode
            ? PrototypeCityHubTopDropdownMode.None
            : nextMode;
        RecordCityHubStateChange("Top dropdown toggled.");
    }

    private void ToggleBottomSheet(PrototypeCityHubBottomSheetMode nextMode)
    {
        if (_cityHubPresentationState.HasBlockingModal)
        {
            return;
        }

        _cityHubPresentationState.ActiveBottomSheet = _cityHubPresentationState.ActiveBottomSheet == nextMode
            ? PrototypeCityHubBottomSheetMode.None
            : nextMode;
        RecordCityHubStateChange("Bottom sheet toggled.");
    }

    private void ToggleBoardOverlay()
    {
        if (_cityHubPresentationState.HasBlockingModal)
        {
            return;
        }

        _cityHubPresentationState.IsBoardOverlayEnabled = !_cityHubPresentationState.IsBoardOverlayEnabled;
        RecordCityHubStateChange("Board overlay toggled.");
    }

    private void CloseCityHubPanels(string reason)
    {
        _cityHubPresentationState.ActiveTopDropdown = PrototypeCityHubTopDropdownMode.None;
        _cityHubPresentationState.ActiveBottomSheet = PrototypeCityHubBottomSheetMode.None;
        _cityHubPresentationState.IsBoardOverlayEnabled = false;
        RecordCityHubStateChange(reason);
    }

    private void ResetCityHubPresentationState(string reason)
    {
        CloseCityHubPanels(reason);
        _cityHubPresentationState.ActiveModal = PrototypeCityHubModalState.None;
    }

    private void SyncCityHubPresentationState(ExpeditionPrepSurfaceData expeditionPrep, ExpeditionPostRunRevealState postRunReveal)
    {
        PrototypeCityHubModalState nextModal = ResolveCityHubModalState(expeditionPrep, postRunReveal);
        if (_cityHubPresentationState.ActiveModal == nextModal)
        {
            return;
        }

        _cityHubPresentationState.ActiveModal = nextModal;
        if (nextModal != PrototypeCityHubModalState.None)
        {
            CloseCityHubPanels("City hub modal took precedence.");
        }
        else
        {
            RecordCityHubStateChange("City hub modal cleared.");
        }
    }

    private PrototypeCityHubModalState ResolveCityHubModalState(ExpeditionPrepSurfaceData expeditionPrep, ExpeditionPostRunRevealState postRunReveal)
    {
        if (expeditionPrep != null && expeditionPrep.IsBoardOpen)
        {
            return PrototypeCityHubModalState.ExpeditionPrepBoard;
        }

        if (postRunReveal != null && postRunReveal.HasPendingReveal)
        {
            return PrototypeCityHubModalState.PostRunRevealSpotlight;
        }

        return PrototypeCityHubModalState.None;
    }

    private void RecordCityHubStateChange(string reason)
    {
        if (HasMeaningfulValue(reason))
        {
            _cityHubPresentationState.LastStateChangeReasonText = reason;
        }
    }

    private float GetCompactTabWidth(string label, float minWidth, float maxWidth)
    {
        Vector2 size = _badgeStyle.CalcSize(new GUIContent(label));
        return Mathf.Clamp(size.x + 42f, minWidth, maxWidth);
    }
    private void DrawMetricPill(Rect rect, string label, string value, Color fillColor)
    {
        DrawPanel(rect, fillColor, new Color(0.05f, 0.07f, 0.10f, 0.24f));
        GUI.Label(new Rect(rect.x + 10f, rect.y + 4f, rect.width - 20f, 18f), label, _metricLabelStyle);
        GUI.Label(new Rect(rect.x + 10f, rect.y + 18f, rect.width - 20f, rect.height - 20f), value, _metricValueStyle);
    }

    private void DrawLanguageButtons(Rect rect, float buttonWidth, float gap, bool large)
    {
        GUIStyle buttonStyle = large ? _buttonStyle : _badgeStyle;
        Rect englishRect = new Rect(rect.x, rect.y, buttonWidth, rect.height);
        Rect koreanRect = new Rect(englishRect.xMax + gap, rect.y, buttonWidth, rect.height);
        DrawLanguageButton(englishRect, "EN", PrototypeLanguage.English, buttonStyle);
        DrawLanguageButton(koreanRect, "KR", PrototypeLanguage.Korean, buttonStyle);
    }

    private void DrawLanguageButton(Rect rect, string label, PrototypeLanguage language, GUIStyle style)
    {
        bool active = _bootEntry.CurrentLanguage == language;
        Color fill = active ? new Color(0.24f, 0.44f, 0.36f, 1f) : new Color(0.12f, 0.16f, 0.18f, 0.96f);
        if (DrawActionButton(rect, label, fill, true, style))
        {
            _bootEntry.SetLanguage(language);
        }
    }

    private bool DrawActionButton(Rect rect, string label, Color fillColor, bool isEnabled)
    {
        return DrawActionButton(rect, label, fillColor, isEnabled, _buttonStyle);
    }

    private bool DrawActionButton(Rect rect, string label, Color fillColor, bool isEnabled, GUIStyle style)
    {
        bool previousEnabled = GUI.enabled;
        Color previousBackgroundColor = GUI.backgroundColor;
        Color previousContentColor = GUI.contentColor;
        GUI.enabled = isEnabled;
        GUI.backgroundColor = isEnabled ? fillColor : new Color(0.16f, 0.18f, 0.20f, 0.92f);
        GUI.contentColor = isEnabled ? Color.white : new Color(0.64f, 0.68f, 0.72f, 1f);
        bool clicked = GUI.Button(rect, label, style);
        GUI.enabled = previousEnabled;
        GUI.backgroundColor = previousBackgroundColor;
        GUI.contentColor = previousContentColor;
        return isEnabled && clicked;
    }

    private void DrawMiniNode(Rect rect, bool isDungeon, Color accentColor)
    {
        DrawRect(new Rect(rect.x + 4f, rect.y + 4f, rect.width, rect.height), new Color(0f, 0f, 0f, 0.32f));
        if (isDungeon)
        {
            DrawRotatedRect(rect, 45f, new Color(0.08f, 0.10f, 0.14f, 0.96f));
            DrawRotatedRect(Inset(rect, 8f), 45f, accentColor);
        }
        else
        {
            DrawPanel(rect, new Color(0.08f, 0.10f, 0.12f, 0.96f), new Color(0.10f, 0.13f, 0.16f, 0.96f));
            DrawRect(Inset(rect, 10f), accentColor);
        }
    }

    private void DrawConnection(Vector2 start, Vector2 end, Color color, float thickness)
    {
        float length = Vector2.Distance(start, end);
        if (length <= 0.01f)
        {
            return;
        }

        Vector2 delta = end - start;
        float angle = Mathf.Atan2(delta.y, delta.x) * Mathf.Rad2Deg;
        Rect rect = new Rect(start.x, start.y - (thickness * 0.5f), length, thickness);
        DrawRotatedRect(rect, angle, color);
    }

    private void DrawPill(Rect rect, string text, Color fillColor, Color textColor)
    {
        DrawPanel(rect, fillColor, new Color(fillColor.r * 0.6f, fillColor.g * 0.6f, fillColor.b * 0.6f, 0.18f));
        GUIStyle pillTextStyle = new GUIStyle(_badgeStyle);
        pillTextStyle.fontSize = Mathf.Clamp(Mathf.RoundToInt(rect.height * 0.62f), 16, 20);
        pillTextStyle.normal.background = null;
        pillTextStyle.hover.background = null;
        pillTextStyle.active.background = null;
        pillTextStyle.focused.background = null;
        pillTextStyle.alignment = TextAnchor.MiddleLeft;
        pillTextStyle.wordWrap = false;
        pillTextStyle.clipping = TextClipping.Clip;
        pillTextStyle.normal.textColor = textColor;
        pillTextStyle.hover.textColor = textColor;
        pillTextStyle.active.textColor = textColor;
        pillTextStyle.focused.textColor = textColor;
        GUI.Label(new Rect(rect.x + 12f, rect.y + 4f, rect.width - 24f, rect.height - 8f), text, pillTextStyle);
    }

    private void DrawPanel(Rect rect, Color borderColor, Color innerColor)
    {
        DrawRect(rect, new Color(0f, 0f, 0f, 0.36f));
        DrawRect(Inset(rect, 1f), borderColor);
        DrawRect(Inset(rect, 3f), innerColor);
        DrawRect(new Rect(rect.x + 3f, rect.y + 3f, rect.width - 6f, 2f), new Color(0.90f, 0.92f, 0.88f, 0.06f));
    }

    private Rect CenterRect(float width, float height)
    {
        return new Rect((Screen.width - width) * 0.5f, (Screen.height - height) * 0.5f, width, height);
    }

    private Rect Inset(Rect rect, float inset)
    {
        return new Rect(rect.x + inset, rect.y + inset, Mathf.Max(0f, rect.width - (inset * 2f)), Mathf.Max(0f, rect.height - (inset * 2f)));
    }

    private void DrawRotatedRect(Rect rect, float angle, Color color)
    {
        Matrix4x4 previousMatrix = GUI.matrix;
        Vector2 pivot = rect.center;
        GUIUtility.RotateAroundPivot(angle, pivot);
        DrawRect(rect, color);
        GUI.matrix = previousMatrix;
    }

    private void DrawRect(Rect rect, Color color)
    {
        Color previousColor = GUI.color;
        GUI.color = color;
        GUI.DrawTexture(rect, Texture2D.whiteTexture);
        GUI.color = previousColor;
    }

    private string T(string key)
    {
        return _bootEntry != null ? _bootEntry.GetText(key) : key;
    }

    private bool TryExecuteCityHubAction(string actionKey, string sourceSurfaceName, string optionKey = "")
    {
        if (_bootEntry == null)
        {
            return false;
        }

        PrototypeCityHubActionResult result = _bootEntry.TryExecuteCityHubAction(new PrototypeCityHubActionRequest
        {
            ActionKey = actionKey ?? string.Empty,
            OptionKey = optionKey ?? string.Empty,
            SourceSurfaceName = string.IsNullOrEmpty(sourceSurfaceName) ? "CityHubUI" : sourceSurfaceName
        });
        if (result != null && HasMeaningfulValue(result.RefreshReasonText))
        {
            _cityHubPresentationState.LastRefreshHintText = result.RefreshReasonText;
        }

        if (result != null && result.Succeeded && result.TransitionToDungeonRunRequested)
        {
            ResetCityHubPresentationState("DungeonRun transition requested by CityHub action.");
        }

        return result != null && result.WasHandled && result.Succeeded;
    }

    private PrototypeCityHubUiSurfaceData GetCityHubUiSurfaceData()
    {
        return _bootEntry != null
            ? _bootEntry.GetCityHubUiSurfaceData()
            : new PrototypeCityHubUiSurfaceData();
    }

    private WorldObservationSurfaceData GetWorldObservationSurfaceData()
    {
        return _bootEntry != null ? _bootEntry.GetWorldObservationSurfaceData() : new WorldObservationSurfaceData();
    }

    private ExpeditionPostRunRevealState GetPostRunRevealState()
    {
        WorldObservationSurfaceData observation = GetWorldObservationSurfaceData();
        return observation != null &&
               observation.RecentOutcome != null &&
               observation.RecentOutcome.PostRunReveal != null
            ? observation.RecentOutcome.PostRunReveal
            : new ExpeditionPostRunRevealState();
    }

    private string V(string value)
    {
        return _bootEntry != null ? _bootEntry.TranslateValue(value) : value;
    }

    private string Line(string label, string value)
    {
        return label + ": " + value;
    }

    private bool HasMeaningfulValue(string value)
    {
        return !string.IsNullOrEmpty(value) && value != "None" && value != "(missing)";
    }

    private Vector2 GetShellScrollPosition(string scrollKey)
    {
        if (string.IsNullOrEmpty(scrollKey))
        {
            return Vector2.zero;
        }

        return _shellScrollByKey.TryGetValue(scrollKey, out Vector2 scrollPosition)
            ? scrollPosition
            : Vector2.zero;
    }

    private void DrawScrollableTextBlock(Rect rect, string scrollKey, string text, GUIStyle style)
    {
        string safeText = SafeShellText(text).Replace("\r", string.Empty);
        float measuredWidth = Mathf.Max(24f, rect.width - 8f);
        float contentHeight = Mathf.Max(rect.height, style.CalcHeight(new GUIContent(safeText), measuredWidth) + 6f);
        bool showVerticalScrollbar = contentHeight > rect.height + 0.5f;
        float contentWidth = Mathf.Max(24f, rect.width - (showVerticalScrollbar ? 18f : 4f));
        Vector2 scrollPosition = GetShellScrollPosition(scrollKey);
        Vector2 nextScrollPosition = GUI.BeginScrollView(
            rect,
            scrollPosition,
            new Rect(0f, 0f, contentWidth, contentHeight),
            false,
            showVerticalScrollbar);
        GUI.Label(new Rect(0f, 0f, contentWidth - 4f, contentHeight), safeText, style);
        GUI.EndScrollView();

        if (showVerticalScrollbar || scrollPosition != Vector2.zero || nextScrollPosition != Vector2.zero)
        {
            _shellScrollByKey[scrollKey] = nextScrollPosition;
        }
        else
        {
            _shellScrollByKey.Remove(scrollKey);
        }
    }

    private void CacheBootEntry()
    {
        if (_bootEntry == null)
        {
            _bootEntry = GetComponent<BootstrapSceneStateBridge>();
        }
    }

    private void EnsureStyles()
    {
        if (_bodyStyle != null)
        {
            return;
        }

        _heroTitleStyle = new GUIStyle(GUI.skin.label);
        _heroTitleStyle.fontSize = 34;
        _heroTitleStyle.fontStyle = FontStyle.Bold;
        _heroTitleStyle.wordWrap = false;
        _heroTitleStyle.normal.textColor = new Color(0.95f, 0.97f, 0.95f, 1f);

        _heroSubtitleStyle = new GUIStyle(GUI.skin.label);
        _heroSubtitleStyle.fontSize = 16;
        _heroSubtitleStyle.fontStyle = FontStyle.Bold;
        _heroSubtitleStyle.wordWrap = true;
        _heroSubtitleStyle.normal.textColor = new Color(0.80f, 0.88f, 0.92f, 1f);

        _panelTitleStyle = new GUIStyle(GUI.skin.label);
        _panelTitleStyle.fontSize = 18;
        _panelTitleStyle.fontStyle = FontStyle.Bold;
        _panelTitleStyle.wordWrap = false;
        _panelTitleStyle.normal.textColor = new Color(0.96f, 0.97f, 0.95f, 1f);

        _bodyStyle = new GUIStyle(GUI.skin.label);
        _bodyStyle.fontSize = 14;
        _bodyStyle.wordWrap = true;
        _bodyStyle.normal.textColor = new Color(0.88f, 0.92f, 0.91f, 1f);

        _captionStyle = new GUIStyle(GUI.skin.label);
        _captionStyle.fontSize = 12;
        _captionStyle.wordWrap = true;
        _captionStyle.normal.textColor = new Color(0.68f, 0.76f, 0.78f, 1f);

        _metricLabelStyle = new GUIStyle(GUI.skin.label);
        _metricLabelStyle.fontSize = 8;
        _metricLabelStyle.fontStyle = FontStyle.Bold;
        _metricLabelStyle.wordWrap = true;
        _metricLabelStyle.normal.textColor = new Color(0.68f, 0.80f, 0.84f, 1f);

        _metricValueStyle = new GUIStyle(GUI.skin.label);
        _metricValueStyle.fontSize = 14;
        _metricValueStyle.fontStyle = FontStyle.Bold;
        _metricValueStyle.wordWrap = false;
        _metricValueStyle.normal.textColor = Color.white;

        _buttonStyle = new GUIStyle(GUI.skin.button);
        _buttonStyle.fontSize = 14;
        _buttonStyle.fontStyle = FontStyle.Bold;
        _buttonStyle.alignment = TextAnchor.MiddleCenter;
        _buttonStyle.wordWrap = false;
        _buttonStyle.padding = new RectOffset(10, 10, 6, 6);
        _buttonStyle.normal.background = Texture2D.whiteTexture;
        _buttonStyle.hover.background = Texture2D.whiteTexture;
        _buttonStyle.active.background = Texture2D.whiteTexture;
        _buttonStyle.normal.textColor = Color.white;
        _buttonStyle.hover.textColor = Color.white;
        _buttonStyle.active.textColor = Color.white;

        _badgeStyle = new GUIStyle(_buttonStyle);
        _badgeStyle.fontSize = 11;
        _badgeStyle.padding = new RectOffset(6, 6, 4, 4);
    }
}
















