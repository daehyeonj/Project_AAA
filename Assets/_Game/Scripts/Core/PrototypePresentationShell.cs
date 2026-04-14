using System.Collections.Generic;
using UnityEngine;

[DisallowMultipleComponent]
public sealed class PrototypePresentationShell : MonoBehaviour
{
    private const float ScreenMargin = 18f;
    private const float WorldTopBarHeight = 96f;
    private const float WorldBottomBarHeight = 58f;
    private const float WorldDropdownHeight = 212f;
    private const float WorldBottomSheetHeight = 166f;
    private static readonly Rect[] EmptyBlockingRects = System.Array.Empty<Rect>();

    private enum WorldTopDropdownMode
    {
        None,
        Snapshot,
        Selection,
        Operations
    }

    private enum WorldBottomSheetMode
    {
        None,
        Actions,
        Selection,
        Overview,
        Logs
    }

    private BootEntry _bootEntry;
    private GUIStyle _heroTitleStyle;
    private GUIStyle _heroSubtitleStyle;
    private GUIStyle _panelTitleStyle;
    private GUIStyle _bodyStyle;
    private GUIStyle _captionStyle;
    private GUIStyle _metricLabelStyle;
    private GUIStyle _metricValueStyle;
    private GUIStyle _buttonStyle;
    private GUIStyle _badgeStyle;
    private WorldTopDropdownMode _activeTopDropdown = WorldTopDropdownMode.None;
    private WorldBottomSheetMode _activeBottomSheet = WorldBottomSheetMode.Actions;
    private bool _showBoardOverlay;
    private Rect[] _blockingRects = EmptyBlockingRects;

    public bool IsPointerOverBlockingUi(Vector2 screenPosition)
    {
        if (_bootEntry == null)
        {
            CacheBootEntry();
        }

        if (_bootEntry == null || (!_bootEntry.IsMainMenuActive && !_bootEntry.IsWorldSimActive))
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
            DrawMainMenuShell();
            return;
        }

        if (_bootEntry.IsWorldSimActive)
        {
            DrawWorldSimShell();
            return;
        }

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

        List<Rect> blockingRects = new List<Rect>();
        blockingRects.Add(gnbRect);
        blockingRects.Add(bnbRect);

        DrawRect(new Rect(0f, 0f, layoutRect.xMax + 24f, 120f), new Color(0.10f, 0.16f, 0.19f, 0.14f));
        DrawRect(new Rect(0f, Screen.height - 140f, layoutRect.xMax + 24f, 140f), new Color(0.03f, 0.04f, 0.07f, 0.18f));
        DrawRotatedRect(new Rect(boardRect.x - 18f, boardRect.y + 28f, boardRect.width + 36f, 14f), -4f, new Color(0.18f, 0.26f, 0.30f, 0.08f));
        DrawRotatedRect(new Rect(boardRect.x + 20f, boardRect.yMax - 42f, boardRect.width - 40f, 10f), 5f, new Color(0.92f, 0.75f, 0.32f, 0.05f));

        if (_showBoardOverlay)
        {
            DrawWorldBoardFrame(boardRect);
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
        WorldBoardReadModel board = GetWorldBoard();
        DrawPanel(rect, new Color(0.07f, 0.10f, 0.14f, 0.98f), new Color(0.10f, 0.14f, 0.18f, 0.94f));

        string selectionName = board.Selection != null && board.Selection.HasSelection && HasMeaningfulValue(board.Selection.DisplayName)
            ? V(board.Selection.DisplayName)
            : T("FrontWorldHeadline");
        GUI.Label(new Rect(rect.x + 16f, rect.y + 10f, 300f, 24f), selectionName, _panelTitleStyle);
        GUI.Label(new Rect(rect.x + 16f, rect.y + 34f, 320f, 18f), V(_bootEntry.LastTransitionLabel), _captionStyle);

        float metricsX = rect.x + 338f;
        float metricWidth = 132f;
        float metricGap = 8f;
        DrawMetricPill(new Rect(metricsX, rect.y + 10f, metricWidth, 42f), T("WorldDay"), board.WorldDayCount.ToString(), new Color(0.15f, 0.23f, 0.28f, 0.94f));
        DrawMetricPill(new Rect(metricsX + metricWidth + metricGap, rect.y + 10f, metricWidth, 42f), T("FrontActionAutoTick"), board.AutoTickEnabled ? T("BoolOn") : T("BoolOff"), new Color(0.13f, 0.20f, 0.18f, 0.94f));
        DrawMetricPill(new Rect(metricsX + ((metricWidth + metricGap) * 2f), rect.y + 10f, metricWidth, 42f), T("ActiveExpeditions"), board.ActiveExpeditions.ToString(), new Color(0.18f, 0.16f, 0.12f, 0.94f));
        DrawMetricPill(new Rect(metricsX + ((metricWidth + metricGap) * 3f), rect.y + 10f, metricWidth, 42f), T("IdleParties"), board.IdleParties.ToString(), new Color(0.14f, 0.17f, 0.22f, 0.94f));

        float utilityRight = rect.xMax - 16f;
        Rect langRect = new Rect(utilityRight - 118f, rect.y + 12f, 118f, 30f);
        DrawLanguageButtons(langRect, 55f, 8f, false);

        Rect escBadgeRect = new Rect(langRect.x - 48f, rect.y + 14f, 40f, 24f);
        DrawPill(escBadgeRect, "Esc", new Color(0.18f, 0.20f, 0.24f, 0.96f), new Color(0.90f, 0.92f, 0.96f, 1f));

        float overlayWidth = GetCompactTabWidth(_showBoardOverlay ? T("FrontActionOverlayOn") : T("FrontActionOverlayOff"), 132f, 178f);
        Rect overlayRect = new Rect(escBadgeRect.x - overlayWidth - 8f, rect.y + 8f, overlayWidth, 36f);
        if (DrawActionButton(overlayRect, _showBoardOverlay ? T("FrontActionOverlayOn") : T("FrontActionOverlayOff"), _showBoardOverlay ? new Color(0.20f, 0.36f, 0.44f, 1f) : new Color(0.12f, 0.16f, 0.20f, 1f), true, _badgeStyle))
        {
            _showBoardOverlay = !_showBoardOverlay;
        }

        float menuWidth = GetCompactTabWidth(T("FrontActionMainMenu"), 128f, 176f);
        Rect menuRect = new Rect(overlayRect.x - menuWidth - 8f, rect.y + 8f, menuWidth, 36f);
        if (DrawActionButton(menuRect, T("FrontActionMainMenu"), new Color(0.22f, 0.18f, 0.14f, 1f), true, _badgeStyle))
        {
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

        if (DrawNavToggle(snapshotRect, snapshotLabel, _activeTopDropdown == WorldTopDropdownMode.Snapshot))
        {
            ToggleTopDropdown(WorldTopDropdownMode.Snapshot);
        }

        if (DrawNavToggle(selectionRect, selectionLabel, _activeTopDropdown == WorldTopDropdownMode.Selection))
        {
            ToggleTopDropdown(WorldTopDropdownMode.Selection);
        }

        if (DrawNavToggle(operationsRect, operationsLabel, _activeTopDropdown == WorldTopDropdownMode.Operations))
        {
            ToggleTopDropdown(WorldTopDropdownMode.Operations);
        }

        if (_activeTopDropdown == WorldTopDropdownMode.None)
        {
            return Rect.zero;
        }

        Rect anchorRect = _activeTopDropdown == WorldTopDropdownMode.Selection
            ? selectionRect
            : _activeTopDropdown == WorldTopDropdownMode.Operations
                ? operationsRect
                : snapshotRect;
        float dropdownWidth = Mathf.Clamp(rect.width * 0.36f, 400f, 520f);
        float dropdownX = Mathf.Clamp(anchorRect.x, rect.x, rect.xMax - dropdownWidth);
        Rect dropdownRect = new Rect(dropdownX, rect.yMax + 8f, dropdownWidth, WorldDropdownHeight);
        DrawPanel(dropdownRect, new Color(0.08f, 0.11f, 0.14f, 0.98f), new Color(0.10f, 0.14f, 0.18f, 0.96f));
        Rect titleRect = new Rect(dropdownRect.x + 16f, dropdownRect.y + 14f, dropdownRect.width - 32f, 26f);
        Rect bodyRect = new Rect(dropdownRect.x + 16f, dropdownRect.y + 46f, dropdownRect.width - 32f, dropdownRect.height - 60f);

        string dropdownTitle = _activeTopDropdown == WorldTopDropdownMode.Selection
            ? selectionLabel
            : _activeTopDropdown == WorldTopDropdownMode.Operations
                ? operationsLabel
                : snapshotLabel;
        GUI.Label(titleRect, dropdownTitle, _panelTitleStyle);
        GUI.Label(bodyRect, GetTopDropdownBody(_activeTopDropdown), _bodyStyle);
        return dropdownRect;
    }

    private Rect DrawWorldBottomSheet(Rect layoutRect, Rect bnbRect)
    {
        if (_activeBottomSheet == WorldBottomSheetMode.None)
        {
            return Rect.zero;
        }

        Rect sheetRect = new Rect(layoutRect.x, bnbRect.y - WorldBottomSheetHeight - 10f, layoutRect.width, WorldBottomSheetHeight);
        DrawPanel(sheetRect, new Color(0.07f, 0.10f, 0.14f, 0.98f), new Color(0.10f, 0.14f, 0.18f, 0.94f));
        GUI.Label(new Rect(sheetRect.x + 16f, sheetRect.y + 12f, sheetRect.width - 32f, 20f), GetBottomSheetTitle(_activeBottomSheet), _panelTitleStyle);

        Rect contentRect = new Rect(sheetRect.x + 16f, sheetRect.y + 40f, sheetRect.width - 32f, sheetRect.height - 52f);
        if (_activeBottomSheet == WorldBottomSheetMode.Actions)
        {
            DrawWorldActionSheet(contentRect);
        }
        else
        {
            GUI.Label(contentRect, GetBottomSheetBody(_activeBottomSheet), _bodyStyle);
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

        if (DrawNavToggle(actionsRect, T("FrontWorldActions"), _activeBottomSheet == WorldBottomSheetMode.Actions))
        {
            ToggleBottomSheet(WorldBottomSheetMode.Actions);
        }

        if (DrawNavToggle(selectionRect, T("FrontWorldSelection"), _activeBottomSheet == WorldBottomSheetMode.Selection))
        {
            ToggleBottomSheet(WorldBottomSheetMode.Selection);
        }

        if (DrawNavToggle(overviewRect, T("FrontWorldOverview"), _activeBottomSheet == WorldBottomSheetMode.Overview))
        {
            ToggleBottomSheet(WorldBottomSheetMode.Overview);
        }

        if (DrawNavToggle(logsRect, T("FilterLogs"), _activeBottomSheet == WorldBottomSheetMode.Logs))
        {
            ToggleBottomSheet(WorldBottomSheetMode.Logs);
        }
    }

    private void DrawWorldBoardFrame(Rect rect)
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

        Rect titleRect = new Rect(innerRect.x + 14f, innerRect.y + 12f, innerRect.width - 28f, 20f);
        Rect subtitleRect = new Rect(titleRect.x, titleRect.yMax + 4f, titleRect.width, 32f);
        Rect footerRect = new Rect(innerRect.x + 14f, innerRect.yMax - 18f, innerRect.width - 28f, 16f);
        GUI.Label(titleRect, T("FrontWorldBoardOverlay"), _panelTitleStyle);
        GUI.Label(subtitleRect, T("FrontWorldOverlayReason"), _captionStyle);
        GUI.Label(footerRect, BuildWorldLegendText(), _captionStyle);

        float pinSize = 10f;
        DrawRect(new Rect(innerRect.x + 8f, innerRect.y + 8f, pinSize, pinSize), new Color(0.84f, 0.72f, 0.34f, 0.72f));
        DrawRect(new Rect(innerRect.xMax - 18f, innerRect.y + 8f, pinSize, pinSize), new Color(0.36f, 0.74f, 0.86f, 0.72f));
        DrawRect(new Rect(innerRect.x + 8f, innerRect.yMax - 18f, pinSize, pinSize), new Color(0.80f, 0.42f, 0.46f, 0.72f));
        DrawRect(new Rect(innerRect.xMax - 18f, innerRect.yMax - 18f, pinSize, pinSize), new Color(0.48f, 0.78f, 0.46f, 0.72f));
    }

    private void DrawWorldActionSheet(Rect rect)
    {
        WorldBoardReadModel board = GetWorldBoard();
        CityStatusReadModel selectedCity = GetSelectedCity(board);
        bool hasCitySelection = selectedCity != null;
        bool canEnterDungeon = selectedCity != null && HasMeaningfulValue(selectedCity.LinkedDungeonId);
        bool autoTickEnabled = _bootEntry.AutoTickEnabled;
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

        if (DrawActionButton(recruitRect, "[G] " + T("FrontActionRecruit"), new Color(0.24f, 0.36f, 0.26f, 1f), hasCitySelection))
        {
            _bootEntry.RecruitWorldSimParty();
        }

        if (DrawActionButton(enterRect, "[X] " + T("FrontActionEnterDungeon"), new Color(0.44f, 0.30f, 0.18f, 1f), canEnterDungeon))
        {
            _bootEntry.TryEnterSelectedWorldDungeon();
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

        if (selectedCity != null)
        {
            DrawCityDecisionActionHint(hintRect, selectedCity);
            return;
        }

        GUI.Label(hintRect, V(_bootEntry.EconomyControlsLabel) + "\n" + V(_bootEntry.ExpeditionControlsLabel), _captionStyle);
    }

    private string GetTopDropdownBody(WorldTopDropdownMode mode)
    {
        if (mode == WorldTopDropdownMode.Selection)
        {
            return BuildWorldSelectionDetailBody();
        }

        if (mode == WorldTopDropdownMode.Operations)
        {
            return BuildWorldOperationsBody();
        }

        return BuildWorldSnapshotBody();
    }

    private string GetBottomSheetTitle(WorldBottomSheetMode mode)
    {
        return mode == WorldBottomSheetMode.Selection
            ? T("FrontWorldSelection")
            : mode == WorldBottomSheetMode.Overview
                ? T("FrontWorldOverview")
                : mode == WorldBottomSheetMode.Logs
                    ? T("FilterLogs")
                    : T("FrontWorldActions");
    }

    private string GetBottomSheetBody(WorldBottomSheetMode mode)
    {
        return mode == WorldBottomSheetMode.Selection
            ? BuildWorldSelectionBriefBody()
            : mode == WorldBottomSheetMode.Overview
                ? BuildWorldOverviewBriefBody()
                : BuildWorldLogBody();
    }

    private string BuildWorldSnapshotBody()
    {
        WorldBoardReadModel board = GetWorldBoard();
        return Line(T("Visible"), board.VisibleCityCount + " / " + board.VisibleDungeonCount + " / " + board.VisibleRoadCount) + "\n" +
               Line(T("WorldDay"), board.WorldDayCount.ToString()) + "\n" +
               Line(T("TradeStepCount"), board.TradeStepCount.ToString()) + "\n" +
               Line(T("AutoTickEnabled"), board.AutoTickEnabled ? T("BoolOn") : T("BoolOff")) + "\n" +
               Line(T("RouteCapacityUsed"), BuildRoadCapacitySummary(board)) + "\n" +
               Line(T("CitiesWithShortages"), BuildShortageCitySummary(board, 2)) + "\n\n" +
               V(_bootEntry.ControlsLabel);
    }

    private string BuildWorldOperationsBody()
    {
        WorldBoardReadModel board = GetWorldBoard();
        return Line(T("EconomyControls"), V(_bootEntry.EconomyControlsLabel)) + "\n" +
               Line(T("ExpeditionControls"), V(_bootEntry.ExpeditionControlsLabel)) + "\n" +
               Line(T("ActiveExpeditions"), board.ActiveExpeditions.ToString()) + "\n" +
               Line(T("IdleParties"), board.IdleParties.ToString()) + "\n" +
               Line(T("UnmetTotal"), board.LastDayUnmetTotal.ToString()) + "\n" +
               Line(T("LastExpeditionResult"), BuildLatestResultSummary(board.LatestResult)) + "\n\n" +
               Line(T("LastTransition"), V(_bootEntry.LastTransitionLabel));
    }

    private string BuildWorldSelectionBriefBody()
    {
        WorldBoardReadModel board = GetWorldBoard();
        CityStatusReadModel selectedCity = GetSelectedCity(board);
        if (selectedCity != null)
        {
            return BuildCityDecisionBriefBody(selectedCity);
        }

        DungeonStatusReadModel selectedDungeon = GetSelectedDungeon(board);
        if (selectedDungeon == null)
        {
            return T("FrontWorldNoSelection");
        }

        return BuildDungeonSelectionBriefBody(board, selectedDungeon);
    }

    private string BuildWorldSelectionDetailBody()
    {
        WorldBoardReadModel board = GetWorldBoard();
        CityStatusReadModel selectedCity = GetSelectedCity(board);
        if (selectedCity != null)
        {
            return BuildCityDecisionDetailBody(board, selectedCity);
        }

        DungeonStatusReadModel selectedDungeon = GetSelectedDungeon(board);
        if (selectedDungeon == null)
        {
            return T("FrontWorldNoSelection");
        }

        return BuildDungeonSelectionDetailBody(board, selectedDungeon);
    }

    private string BuildWorldOverviewBriefBody()
    {
        WorldBoardReadModel board = GetWorldBoard();
        return Line(T("WorldDay"), board.WorldDayCount.ToString()) + "\n" +
               Line(T("TradeStepCount"), board.TradeStepCount.ToString()) + "\n" +
               Line(T("TotalParties"), board.TotalParties.ToString()) + "\n" +
               Line(T("IdleParties"), board.IdleParties.ToString()) + "\n" +
               Line(T("ActiveExpeditions"), board.ActiveExpeditions.ToString()) + "\n" +
               Line(T("UnmetTotal"), board.LastDayUnmetTotal.ToString()) + "\n" +
               Line(T("CitiesWithShortages"), BuildShortageCitySummary(board, 3));
    }

    private string BuildWorldLogBody()
    {
        WorldBoardReadModel board = GetWorldBoard();
        return Line(T("RecentExpeditionLog1"), GetLogLine(board.RecentExpeditionLogs, 0)) + "\n" +
               Line(T("RecentExpeditionLog2"), GetLogLine(board.RecentExpeditionLogs, 1)) + "\n" +
               Line(T("RecentExpeditionLog3"), GetLogLine(board.RecentExpeditionLogs, 2)) + "\n\n" +
               Line(T("RecentDayLog1"), GetLogLine(board.RecentDayLogs, 0)) + "\n" +
               Line(T("RecentDayLog2"), GetLogLine(board.RecentDayLogs, 1)) + "\n" +
               Line(T("RecentDayLog3"), GetLogLine(board.RecentDayLogs, 2));
    }

    private string BuildWorldLegendText()
    {
        return T("FrontMenuPillarNetwork") + "  |  " + T("FrontMenuPillarDispatch") + "  |  " + T("FrontMenuPillarEconomy");
    }

    private string BuildCityDecisionBriefBody(CityStatusReadModel city)
    {
        CityDecisionReadModel decision = GetCityDecision(city);
        return city.DisplayName + "\n" +
               Line(T("NeedPressure"), decision.NeedPressureStateId) + "\n" +
               Line(T("DispatchReadiness"), decision.DispatchReadinessStateId) + "\n" +
               Line(T("CityHubWhyNow"), decision.WhyCityMattersText) + "\n" +
               Line(T("CityHubBottleneck"), BuildCityBottleneckSummary(decision.Bottlenecks, 1)) + "\n" +
               Line(T("CityHubOpportunity"), BuildCityOpportunitySummary(decision.Opportunities, 1)) + "\n" +
               Line(T("CityHubRecentImpact"), BuildRecentImpactSummary(decision.RecentImpacts, 1)) + "\n" +
               Line(T("CityHubRecommendedAction"), BuildCityRecommendationSummary(decision.RecommendedActions, 1));
    }

    private string BuildCityDecisionDetailBody(WorldBoardReadModel board, CityStatusReadModel city)
    {
        CityDecisionReadModel decision = GetCityDecision(city);
        return city.DisplayName + "\n" +
               Line(T("SelectedType"), board.Selection.Kind.ToString()) + "\n" +
               Line(T("NeedPressure"), decision.NeedPressureStateId) + "\n" +
               Line(T("DispatchReadiness"), decision.DispatchReadinessStateId) + "\n" +
               Line(T("DispatchPolicy"), decision.DispatchPolicyStateId) + "\n" +
               Line(T("LinkedDungeon"), city.LinkedDungeonDisplayName) + "\n" +
               Line(T("RecommendedRoute"), HasMeaningfulValue(city.RecommendedRouteSummaryText) ? city.RecommendedRouteSummaryText : "None") + "\n" +
               Line(T("CityHubWhyNow"), decision.WhyCityMattersText) + "\n" +
               Line(T("CityHubBottleneck"), BuildCityBottleneckSummary(decision.Bottlenecks, 3)) + "\n" +
               Line(T("CityHubOpportunity"), BuildCityOpportunitySummary(decision.Opportunities, 2)) + "\n" +
               Line(T("CityHubRecentImpact"), BuildRecentImpactSummary(decision.RecentImpacts, 2)) + "\n" +
               Line(T("CityHubRecommendedAction"), BuildCityRecommendationSummary(decision.RecommendedActions, 3)) + "\n" +
               Line(T("SelectedStocks"), BuildResourceSummary(city.KeyStocks, 4)) + "\n" +
               Line(T("AvailableContract"), city.AvailableContractSlots + "/" + city.MaxActiveExpeditionSlots);
    }

    private string BuildDungeonSelectionBriefBody(WorldBoardReadModel board, DungeonStatusReadModel dungeon)
    {
        return dungeon.DisplayName + "\n" +
               Line(T("DungeonDanger"), dungeon.DangerLevel.ToString()) + "\n" +
               Line(T("LinkedCity"), dungeon.LinkedCityDisplayName) + "\n" +
               Line(T("RewardPreview"), BuildStringArraySummary(dungeon.OutputResourceIds, 2)) + "\n" +
               Line(T("CityHubWhyNow"), BuildDungeonWhyItMatters(board, dungeon)) + "\n" +
               Line(T("LastExpeditionResult"), BuildLatestResultSummary(dungeon.LatestResult));
    }

    private string BuildDungeonSelectionDetailBody(WorldBoardReadModel board, DungeonStatusReadModel dungeon)
    {
        return dungeon.DisplayName + "\n" +
               Line(T("SelectedType"), board.Selection.Kind.ToString()) + "\n" +
               Line(T("DungeonDanger"), dungeon.DangerLevel.ToString()) + "\n" +
               Line(T("LinkedCity"), dungeon.LinkedCityDisplayName) + "\n" +
               Line(T("RewardPreview"), BuildStringArraySummary(dungeon.OutputResourceIds, 3)) + "\n" +
               Line(T("RecommendedPower"), dungeon.RecommendedPower.ToString()) + "\n" +
               Line(T("ExpeditionDurationDays"), dungeon.ExpeditionDurationDays.ToString()) + "\n" +
               Line(T("CityHubWhyNow"), BuildDungeonWhyItMatters(board, dungeon)) + "\n" +
               Line(T("CityHubOpportunity"), BuildDungeonOpportunitySummary(board, dungeon)) + "\n" +
               Line(T("LastExpeditionResult"), BuildLatestResultSummary(dungeon.LatestResult));
    }

    private WorldBoardReadModel GetWorldBoard()
    {
        return _bootEntry != null ? _bootEntry.GetWorldBoardReadModel() : WorldBoardReadModel.Empty;
    }

    private CityStatusReadModel GetSelectedCity(WorldBoardReadModel board)
    {
        if (board == null || board.Selection == null || !board.Selection.HasSelection || board.Selection.Kind != WorldEntityKind.City)
        {
            return null;
        }

        string cityId = board.Selection.EntityId;
        return FindCityById(board, cityId);
    }

    private CityStatusReadModel FindCityById(WorldBoardReadModel board, string cityId)
    {
        if (!HasMeaningfulValue(cityId))
        {
            return null;
        }

        CityStatusReadModel[] cities = board.Cities ?? System.Array.Empty<CityStatusReadModel>();
        for (int i = 0; i < cities.Length; i++)
        {
            CityStatusReadModel city = cities[i];
            if (city != null && city.CityId == cityId)
            {
                return city;
            }
        }

        return null;
    }

    private DungeonStatusReadModel GetSelectedDungeon(WorldBoardReadModel board)
    {
        if (board == null || board.Selection == null || !board.Selection.HasSelection)
        {
            return null;
        }

        string dungeonId = board.Selection.Kind == WorldEntityKind.Dungeon
            ? board.Selection.EntityId
            : board.Selection.LinkedDungeonId;
        DungeonStatusReadModel[] dungeons = board.Dungeons ?? System.Array.Empty<DungeonStatusReadModel>();
        for (int i = 0; i < dungeons.Length; i++)
        {
            DungeonStatusReadModel dungeon = dungeons[i];
            if (dungeon != null && dungeon.DungeonId == dungeonId)
            {
                return dungeon;
            }
        }

        return null;
    }

    private string BuildRoadCapacitySummary(WorldBoardReadModel board)
    {
        return board != null ? board.LastDayRouteCapacityUsedTotal + "/" + board.TotalRouteCapacityPerDay : "None";
    }

    private string BuildShortageCitySummary(WorldBoardReadModel board, int maxCount)
    {
        if (board == null || board.Cities == null || board.Cities.Length == 0)
        {
            return "None";
        }

        System.Text.StringBuilder builder = new System.Text.StringBuilder();
        int written = 0;
        for (int i = 0; i < board.Cities.Length && written < maxCount; i++)
        {
            CityStatusReadModel city = board.Cities[i];
            if (city == null || city.LastDayShortages < 1)
            {
                continue;
            }

            if (written > 0)
            {
                builder.Append(" | ");
            }

            builder.Append(city.DisplayName);
            builder.Append(" (");
            builder.Append(city.LastDayShortages);
            builder.Append(")");
            written += 1;
        }

        return written > 0 ? builder.ToString() : "None";
    }

    private string BuildStringArraySummary(string[] values, int maxCount)
    {
        if (values == null || values.Length == 0)
        {
            return "None";
        }

        System.Text.StringBuilder builder = new System.Text.StringBuilder();
        int written = 0;
        for (int i = 0; i < values.Length && written < maxCount; i++)
        {
            string value = values[i];
            if (!HasMeaningfulValue(value))
            {
                continue;
            }

            if (written > 0)
            {
                builder.Append(", ");
            }

            builder.Append(value);
            written += 1;
        }

        return written > 0 ? builder.ToString() : "None";
    }

    private string BuildResourceSummary(ResourceAmountReadModel[] resources, int maxCount)
    {
        if (resources == null || resources.Length == 0)
        {
            return "None";
        }

        System.Text.StringBuilder builder = new System.Text.StringBuilder();
        int written = 0;
        for (int i = 0; i < resources.Length && written < maxCount; i++)
        {
            ResourceAmountReadModel resource = resources[i];
            if (resource == null || !HasMeaningfulValue(resource.ResourceId))
            {
                continue;
            }

            if (written > 0)
            {
                builder.Append(", ");
            }

            builder.Append(resource.ResourceId);
            builder.Append(" x");
            builder.Append(resource.Amount);
            written += 1;
        }

        return written > 0 ? builder.ToString() : "None";
    }

    private string BuildLatestResultSummary(ExpeditionResultReadModel result)
    {
        if (result == null || !result.HasResult)
        {
            return "None";
        }

        if (HasMeaningfulValue(result.WorldReturnSummaryText))
        {
            return result.WorldReturnSummaryText;
        }

        if (HasMeaningfulValue(result.SummaryText))
        {
            return result.SummaryText;
        }

        if (HasMeaningfulValue(result.ResultStateKey))
        {
            return result.ResultStateKey;
        }

        return result.SourceCityDisplayName + " -> " + result.TargetDungeonDisplayName;
    }

    private string BuildSignalSummary(WorldDecisionSignalReadModel[] signals, int maxCount)
    {
        if (signals == null || signals.Length == 0)
        {
            return "None";
        }

        System.Text.StringBuilder builder = new System.Text.StringBuilder();
        int written = 0;
        for (int i = 0; i < signals.Length && written < maxCount; i++)
        {
            WorldDecisionSignalReadModel signal = signals[i];
            if (signal == null)
            {
                continue;
            }

            if (written > 0)
            {
                builder.Append(" | ");
            }

            builder.Append(DescribeSignal(signal));
            written += 1;
        }

        return written > 0 ? builder.ToString() : "None";
    }

    private string BuildCityBottleneckSummary(CityBottleneckSignal[] signals, int maxCount)
    {
        if (signals == null || signals.Length == 0)
        {
            return "None";
        }

        System.Text.StringBuilder builder = new System.Text.StringBuilder();
        int written = 0;
        for (int i = 0; i < signals.Length && written < maxCount; i++)
        {
            CityBottleneckSignal signal = signals[i];
            if (signal == null || !HasMeaningfulValue(signal.SummaryText))
            {
                continue;
            }

            if (written > 0)
            {
                builder.Append(" | ");
            }

            builder.Append(signal.SummaryText);
            written += 1;
        }

        return written > 0 ? builder.ToString() : "None";
    }

    private string BuildCityOpportunitySummary(CityOpportunitySignal[] signals, int maxCount)
    {
        if (signals == null || signals.Length == 0)
        {
            return "None";
        }

        System.Text.StringBuilder builder = new System.Text.StringBuilder();
        int written = 0;
        for (int i = 0; i < signals.Length && written < maxCount; i++)
        {
            CityOpportunitySignal signal = signals[i];
            if (signal == null || !HasMeaningfulValue(signal.SummaryText))
            {
                continue;
            }

            if (written > 0)
            {
                builder.Append(" | ");
            }

            builder.Append(signal.SummaryText);
            written += 1;
        }

        return written > 0 ? builder.ToString() : "None";
    }

    private string BuildRecentImpactSummary(RecentImpactSummary[] impacts, int maxCount)
    {
        if (impacts == null || impacts.Length == 0)
        {
            return "None";
        }

        System.Text.StringBuilder builder = new System.Text.StringBuilder();
        int written = 0;
        for (int i = 0; i < impacts.Length && written < maxCount; i++)
        {
            RecentImpactSummary impact = impacts[i];
            if (impact == null || !HasMeaningfulValue(impact.SummaryText))
            {
                continue;
            }

            if (written > 0)
            {
                builder.Append(" | ");
            }

            builder.Append(impact.SummaryText);
            written += 1;
        }

        return written > 0 ? builder.ToString() : "None";
    }

    private string BuildCityRecommendationSummary(CityActionRecommendation[] recommendations, int maxCount)
    {
        if (recommendations == null || recommendations.Length == 0)
        {
            return "None";
        }

        System.Text.StringBuilder builder = new System.Text.StringBuilder();
        int written = 0;
        for (int i = 0; i < recommendations.Length && written < maxCount; i++)
        {
            CityActionRecommendation recommendation = recommendations[i];
            if (recommendation == null || !HasMeaningfulValue(recommendation.SummaryText))
            {
                continue;
            }

            if (written > 0)
            {
                builder.Append(" | ");
            }

            builder.Append(recommendation.SummaryText);
            written += 1;
        }

        return written > 0 ? builder.ToString() : "None";
    }

    private string BuildDungeonWhyItMatters(WorldBoardReadModel board, DungeonStatusReadModel dungeon)
    {
        CityStatusReadModel linkedCity = dungeon != null ? FindCityById(board, dungeon.LinkedCityId) : null;
        CityDecisionReadModel decision = GetCityDecision(linkedCity);
        if (decision.Opportunities != null)
        {
            for (int i = 0; i < decision.Opportunities.Length; i++)
            {
                CityOpportunitySignal opportunity = decision.Opportunities[i];
                if (opportunity != null && opportunity.DungeonId == dungeon.DungeonId && HasMeaningfulValue(opportunity.WhyItMattersText))
                {
                    return opportunity.WhyItMattersText;
                }
            }
        }

        return decision.WhyCityMattersText;
    }

    private string BuildDungeonOpportunitySummary(WorldBoardReadModel board, DungeonStatusReadModel dungeon)
    {
        CityStatusReadModel linkedCity = dungeon != null ? FindCityById(board, dungeon.LinkedCityId) : null;
        CityDecisionReadModel decision = GetCityDecision(linkedCity);
        if (decision.Opportunities != null)
        {
            for (int i = 0; i < decision.Opportunities.Length; i++)
            {
                CityOpportunitySignal opportunity = decision.Opportunities[i];
                if (opportunity != null && opportunity.DungeonId == dungeon.DungeonId && HasMeaningfulValue(opportunity.SummaryText))
                {
                    return opportunity.SummaryText;
                }
            }
        }

        return "None";
    }

    private CityDecisionReadModel GetCityDecision(CityStatusReadModel city)
    {
        return city != null && city.Decision != null ? city.Decision : new CityDecisionReadModel();
    }

    private void DrawCityDecisionActionHint(Rect rect, CityStatusReadModel city)
    {
        CityDecisionReadModel decision = GetCityDecision(city);
        string summaryText =
            ShortenText(decision.WhyCityMattersText, 120) + "\n" +
            T("CityHubBottleneck") + ": " + ShortenText(BuildCityBottleneckSummary(decision.Bottlenecks, 1), 70) +
            "  |  " + T("CityHubOpportunity") + ": " + ShortenText(BuildCityOpportunitySummary(decision.Opportunities, 1), 70) + "\n" +
            T("CityHubRecentImpact") + ": " + ShortenText(BuildRecentImpactSummary(decision.RecentImpacts, 1), 64) +
            "  |  " + T("CityHubRecommendedAction") + ": " + ShortenText(BuildCityRecommendationSummary(decision.RecommendedActions, 1), 64);
        GUI.Label(rect, summaryText, _captionStyle);
    }

    private string ShortenText(string value, int maxLength)
    {
        if (!HasMeaningfulValue(value) || maxLength < 4)
        {
            return "None";
        }

        return value.Length <= maxLength ? value : value.Substring(0, maxLength - 3) + "...";
    }

    private string DescribeSignal(WorldDecisionSignalReadModel signal)
    {
        if (signal == null)
        {
            return "None";
        }

        if (signal.Kind == WorldDecisionSignalKind.CityShortage)
        {
            string resourceText = HasMeaningfulValue(signal.ResourceId) ? signal.ResourceId : T("LastDayProcessingBlocked");
            return signal.EntityDisplayName + " " + resourceText + " x" + signal.Magnitude;
        }

        if (signal.Kind == WorldDecisionSignalKind.CitySurplus)
        {
            return signal.EntityDisplayName + " " + signal.ResourceId + " x" + signal.Magnitude;
        }

        if (signal.Kind == WorldDecisionSignalKind.RoadCapacity)
        {
            return signal.EntityDisplayName + " " + signal.Magnitude + "%";
        }

        if (signal.Kind == WorldDecisionSignalKind.ActiveExpedition)
        {
            return signal.EntityDisplayName + " -> " + signal.RelatedEntityDisplayName + " (" + signal.Magnitude + "d)";
        }

        if (signal.Kind == WorldDecisionSignalKind.RecentResult)
        {
            return signal.EntityDisplayName + " " + signal.ResultStateKey;
        }

        return signal.EntityDisplayName + " " + signal.ResourceId;
    }

    private string GetLogLine(string[] logs, int index)
    {
        return logs != null && index >= 0 && index < logs.Length && HasMeaningfulValue(logs[index]) ? logs[index] : "None";
    }

    private bool DrawNavToggle(Rect rect, string label, bool active)
    {
        Color fillColor = active
            ? new Color(0.20f, 0.42f, 0.52f, 1f)
            : new Color(0.12f, 0.16f, 0.20f, 0.96f);
        return DrawActionButton(rect, label, fillColor, true, _badgeStyle);
    }

    private void ToggleTopDropdown(WorldTopDropdownMode nextMode)
    {
        _activeTopDropdown = _activeTopDropdown == nextMode ? WorldTopDropdownMode.None : nextMode;
    }

    private void ToggleBottomSheet(WorldBottomSheetMode nextMode)
    {
        _activeBottomSheet = _activeBottomSheet == nextMode ? WorldBottomSheetMode.None : nextMode;
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
        Color previousColor = GUI.contentColor;
        GUI.contentColor = textColor;
        GUI.Label(new Rect(rect.x + 12f, rect.y + 6f, rect.width - 24f, rect.height - 12f), text, _badgeStyle);
        GUI.contentColor = previousColor;
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

    private void CacheBootEntry()
    {
        if (_bootEntry == null)
        {
            _bootEntry = GetComponent<BootEntry>();
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
        _bodyStyle.fontSize = 13;
        _bodyStyle.wordWrap = true;
        _bodyStyle.normal.textColor = new Color(0.88f, 0.92f, 0.91f, 1f);

        _captionStyle = new GUIStyle(GUI.skin.label);
        _captionStyle.fontSize = 11;
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
        _buttonStyle.fontSize = 13;
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
        _badgeStyle.fontSize = 10;
        _badgeStyle.padding = new RectOffset(6, 6, 4, 4);
    }
}










