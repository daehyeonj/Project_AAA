using System.Collections.Generic;
using UnityEngine;

public sealed partial class PrototypePresentationShell
{
    private const float DungeonShellMargin = 22f;
    private const float DungeonShellGap = 14f;
    private const float DungeonHeaderHeight = 74f;
    private const float DungeonFooterHeight = 38f;

    private void DrawDungeonRunShell()
    {
        PrototypeDungeonRunShellSurfaceData shellSurface = _bootEntry.GetDungeonRunShellSurfaceData();
        Rect screenRect = new Rect(0f, 0f, Screen.width, Screen.height);
        _blockingRects = new[] { screenRect };

        if (shellSurface.IsResultPanelVisible)
        {
            DrawDungeonShellBackdrop(screenRect);
            DrawDungeonResultShell(screenRect, shellSurface);
            return;
        }

        if (shellSurface.IsBattleViewActive)
        {
            _blockingRects = EmptyBlockingRects;
            return;
        }

        DrawDungeonShellBackdrop(screenRect);

        if (shellSurface.IsRouteChoiceVisible)
        {
            DrawLegacyDungeonRouteFallbackShell(screenRect, shellSurface);
            return;
        }

        if (shellSurface.IsEventChoiceVisible)
        {
            DrawDungeonDecisionShell(screenRect, shellSurface, false);
            return;
        }

        if (shellSurface.IsPreEliteChoiceVisible)
        {
            DrawDungeonDecisionShell(screenRect, shellSurface, true);
            return;
        }

        DrawDungeonExploreShell(screenRect, shellSurface);
    }

    private void DrawDungeonShellBackdrop(Rect screenRect)
    {
        DrawRect(screenRect, new Color(0.05f, 0.06f, 0.09f, 0.96f));
        DrawRect(new Rect(0f, 0f, screenRect.width, screenRect.height * 0.16f), new Color(0.10f, 0.14f, 0.19f, 0.22f));
        DrawRect(new Rect(0f, screenRect.height * 0.80f, screenRect.width, screenRect.height * 0.20f), new Color(0.03f, 0.04f, 0.07f, 0.26f));
        DrawRotatedRect(new Rect(screenRect.width * 0.12f, screenRect.height * 0.20f, screenRect.width * 0.34f, 14f), -8f, new Color(0.30f, 0.48f, 0.66f, 0.08f));
        DrawRotatedRect(new Rect(screenRect.width * 0.48f, screenRect.height * 0.72f, screenRect.width * 0.28f, 12f), 10f, new Color(0.90f, 0.72f, 0.34f, 0.06f));
    }

    private void DrawLegacyDungeonRouteFallbackShell(Rect screenRect, PrototypeDungeonRunShellSurfaceData shellSurface)
    {
        ExpeditionStartContext startContext = shellSurface != null ? shellSurface.ExpeditionStartContext : new ExpeditionStartContext();
        PrototypeDungeonPanelContext panelContext = shellSurface != null ? shellSurface.PanelContext : new PrototypeDungeonPanelContext();
        Rect layoutRect = new Rect(DungeonShellMargin, DungeonShellMargin, screenRect.width - (DungeonShellMargin * 2f), screenRect.height - (DungeonShellMargin * 2f));
        Rect headerRect = new Rect(layoutRect.x, layoutRect.y, layoutRect.width, DungeonHeaderHeight);
        Rect contentRect = new Rect(layoutRect.x, headerRect.yMax + DungeonShellGap, layoutRect.width, layoutRect.height - DungeonHeaderHeight - DungeonShellGap);

        DrawDungeonHeaderBar(
            headerRect,
            HasMeaningfulValue(shellSurface.RouteChoiceTitleText) ? SafeShellText(shellSurface.RouteChoiceTitleText) : "Legacy Dispatch Fallback",
            BuildDungeonLines(
                SafeShellText(shellSurface.RouteChoiceDescriptionText),
                "Normal Launch Owner: ExpeditionPrep confirm seam",
                startContext.BriefingSummaryText),
            BuildDungeonHeaderChips(
                BuildDungeonChip("City", startContext.StartCityLabel),
                BuildDungeonChip("Dungeon", startContext.DungeonLabel),
                BuildDungeonChip("Risk", startContext.RouteRiskLabel),
                BuildDungeonChip("Supply", startContext.SupplyPressureSummaryText)));

        float leftWidth = Mathf.Clamp(contentRect.width * 0.30f, 320f, 420f);
        float centerWidth = Mathf.Clamp(contentRect.width * 0.34f, 360f, 460f);
        float rightWidth = contentRect.width - leftWidth - centerWidth - (DungeonShellGap * 2f);
        Rect leftRect = new Rect(contentRect.x, contentRect.y, leftWidth, contentRect.height);
        Rect centerRect = new Rect(leftRect.xMax + DungeonShellGap, contentRect.y, centerWidth, contentRect.height);
        Rect rightRect = new Rect(centerRect.xMax + DungeonShellGap, contentRect.y, rightWidth, contentRect.height);

        DrawDungeonInfoCard(
            leftRect,
            "Fallback Handoff Snapshot",
            BuildDungeonLines(
                "Normal Launch Owner: ExpeditionPrep confirm seam",
                "City: " + SafeShellText(startContext.StartCityLabel),
                "Dungeon Identity: " + SafeShellText(startContext.DungeonLabel),
                "Party Posture: " + SafeShellText(startContext.PartySummaryText),
                "Recommended Line: " + SafeShellText(shellSurface.RecommendedRouteText),
                "Recommendation: " + SafeShellText(shellSurface.RecommendationReasonText),
                "Route Fit: " + SafeShellText(startContext.RouteFitSummaryText),
                "Launch Lock: " + SafeShellText(startContext.LaunchLockSummaryText),
                "Projected Result: " + SafeShellText(startContext.ProjectedOutcomeSummaryText),
                "Supply Pressure: " + SafeShellText(startContext.SupplyPressureSummaryText),
                "Time Pressure: " + SafeShellText(startContext.TimePressureSummaryText),
                "Threat Pressure: " + SafeShellText(startContext.ThreatPressureSummaryText),
                "Discovery Outlook: " + SafeShellText(startContext.DiscoverySummaryText),
                "Extraction Outlook: " + SafeShellText(startContext.ExtractionPressureSummaryText),
                "Policy: " + SafeShellText(shellSurface.DispatchPolicyText)));

        DrawLegacyDungeonRouteFallbackOptions(centerRect, panelContext);

        DrawDungeonInfoCard(
            rightRect,
            "Fallback Route Contrast",
            BuildDungeonLines(
                "Hovered Line: " + SafeShellText(panelContext.ChoiceSummaryText),
                "Path Identity: " + SafeShellText(startContext.RoutePreviewSummaryText),
                "Payout Shape: " + SafeShellText(startContext.RewardPreviewSummaryText),
                "Shrine Trade: " + SafeShellText(startContext.EventPreviewSummaryText),
                "Supply Outlook: " + SafeShellText(startContext.SupplySummaryText),
                "World Pressure: " + SafeShellText(startContext.WorldModifierSummaryText),
                "Panel Stage: " + SafeShellText(panelContext.Template.StageLabel),
                "Legacy fallback only | " + SafeShellText(shellSurface.RouteChoicePromptText),
                "Controls: [1] [2] [Q] [Enter] [Esc] | Fallback only"));
    }

    private void DrawLegacyDungeonRouteFallbackOptions(Rect rect, PrototypeDungeonPanelContext panelContext)
    {
        DrawPanel(rect, new Color(0.12f, 0.18f, 0.24f, 0.98f), new Color(0.08f, 0.11f, 0.16f, 0.94f));
        Rect titleRect = new Rect(rect.x + 16f, rect.y + 14f, rect.width - 32f, 24f);
        GUI.Label(titleRect, "Choose Fallback Opening Line", _panelTitleStyle);

        PrototypeDungeonPanelOption[] options = panelContext != null && panelContext.Options != null
            ? panelContext.Options
            : System.Array.Empty<PrototypeDungeonPanelOption>();
        string hoveredOptionId = string.Empty;
        float cardHeight = Mathf.Clamp((rect.height - 124f - DungeonShellGap) * 0.5f, 116f, 164f);
        float cardY = titleRect.yMax + 14f;

        for (int i = 0; i < options.Length && i < 2; i++)
        {
            PrototypeDungeonPanelOption option = options[i];
            Rect cardRect = new Rect(rect.x + 16f, cardY + ((cardHeight + DungeonShellGap) * i), rect.width - 32f, cardHeight);
            bool hovered;
            string footer = option.IsBlocked
                ? "Locked"
                : (i == 0 ? "[1] Select fallback" : "[2] Select fallback");
            bool clicked = DrawDungeonOptionCard(
                cardRect,
                SafeShellText(option.OptionLabel),
                BuildPlannerOptionSubtitle(option),
                BuildDungeonLines(
                    "Path Read: " + SafeShellText(option.OutcomeHintText),
                    "Trade-off: " + SafeShellText(option.RiskHintText),
                    "Payout Line: " + SafeShellText(option.RewardHintText)),
                footer,
                i == 0 ? new Color(0.34f, 0.58f, 0.78f, 1f) : new Color(0.80f, 0.62f, 0.34f, 1f),
                ResolveOptionTag(option),
                option.IsBlocked,
                out hovered);

            if (hovered)
            {
                hoveredOptionId = option.OptionId;
            }

            if (clicked)
            {
                _bootEntry.TryTriggerLegacyDungeonRouteChoice(option.OptionId);
            }
        }

        _bootEntry.SetLegacyDungeonRouteChoiceHover(hoveredOptionId);

        float actionY = rect.yMax - 54f;
        float actionWidth = (rect.width - 48f - DungeonShellGap) * 0.5f;
        Rect policyRect = new Rect(rect.x + 16f, actionY, actionWidth, 38f);
        Rect dispatchRect = new Rect(policyRect.xMax + DungeonShellGap, actionY, actionWidth, 38f);
        if (DrawActionButton(policyRect, "[Q] Policy", new Color(0.20f, 0.28f, 0.36f, 1f), true, _badgeStyle))
        {
            _bootEntry.TryCycleCurrentDispatchPolicy();
        }

        if (DrawActionButton(dispatchRect, "[Enter] Continue Fallback", new Color(0.22f, 0.46f, 0.34f, 1f), _bootEntry.CanConfirmLegacyDungeonRouteChoice(), _badgeStyle))
        {
            _bootEntry.TryConfirmLegacyDungeonRouteChoice();
        }
    }

    private void DrawDungeonExploreShell(Rect screenRect, PrototypeDungeonRunShellSurfaceData shellSurface)
    {
        PrototypeDungeonPanelContext panelContext = shellSurface != null ? shellSurface.PanelContext : new PrototypeDungeonPanelContext();
        Rect layoutRect = new Rect(DungeonShellMargin, DungeonShellMargin, screenRect.width - (DungeonShellMargin * 2f), screenRect.height - (DungeonShellMargin * 2f));
        Rect headerRect = new Rect(layoutRect.x, layoutRect.y, layoutRect.width, DungeonHeaderHeight);
        Rect contentRect = new Rect(layoutRect.x, headerRect.yMax + DungeonShellGap, layoutRect.width, layoutRect.height - DungeonHeaderHeight - DungeonShellGap);

        DrawDungeonHeaderBar(
            headerRect,
            SafeShellText(panelContext.Template.StageLabel),
            BuildDungeonLines(
                SafeShellText(panelContext.PanelSummaryText),
                SafeShellText(shellSurface.CurrentSelectionPromptText)),
            BuildDungeonHeaderChips(
                BuildDungeonChip("Dungeon", panelContext.DungeonLabel),
                BuildDungeonChip("Route", panelContext.RouteLabel),
                BuildDungeonChip("Room", panelContext.ProgressSummaryText),
                BuildDungeonChip("Condition", shellSurface.PartyConditionText),
                BuildDungeonChip("Loot", shellSurface.LootBreakdownText)));

        float trackWidth = Mathf.Clamp(contentRect.width * 0.60f, 700f, 980f);
        Rect trackRect = new Rect(contentRect.x, contentRect.y, trackWidth, contentRect.height);
        Rect sidebarRect = new Rect(trackRect.xMax + DungeonShellGap, contentRect.y, contentRect.width - trackWidth - DungeonShellGap, contentRect.height);
        DrawDungeonExploreTrack(trackRect, panelContext);
        DrawDungeonInfoCard(
            sidebarRect,
            "Expedition Readback",
            BuildDungeonLines(
                "Current Room: " + SafeShellText(panelContext.RoomDisplayLabel),
                "Room Type: " + SafeShellText(panelContext.RoomTypeLabel),
                "Next Goal: " + SafeShellText(panelContext.NextMajorGoalText),
                "Party HP: " + SafeShellText(shellSurface.TotalPartyHpText),
                "Party Condition: " + SafeShellText(shellSurface.PartyConditionText),
                "Sustain Pressure: " + SafeShellText(shellSurface.SustainPressureText),
                "Elite Status: " + SafeShellText(shellSurface.EliteStatusText),
                "Supply Pressure: " + SafeShellText(panelContext.SupplyPressureSummaryText),
                "Time Pressure: " + SafeShellText(panelContext.TimePressureSummaryText),
                "Threat Pressure: " + SafeShellText(panelContext.ThreatPressureSummaryText),
                "Discovery: " + SafeShellText(panelContext.DiscoverySummaryText),
                "Extraction: " + SafeShellText(panelContext.ExtractionPressureSummaryText),
                "Choice Outcome: " + SafeShellText(panelContext.LatestChoiceOutcomeSummaryText),
                "Event Resolution: " + SafeShellText(panelContext.EventResolutionSummaryText),
                "Extraction Result: " + SafeShellText(panelContext.ExtractionSummaryText),
                "Delta: " + SafeShellText(panelContext.LaneDeltaSummaryText)));
    }

    private void DrawDungeonExploreTrack(Rect rect, PrototypeDungeonPanelContext panelContext)
    {
        DrawPanel(rect, new Color(0.12f, 0.18f, 0.24f, 0.98f), new Color(0.07f, 0.10f, 0.15f, 0.94f));
        Rect innerRect = Inset(rect, 16f);
        Rect headerBandRect = new Rect(innerRect.x, innerRect.y, innerRect.width, 60f);
        DrawPanel(headerBandRect, new Color(0.16f, 0.22f, 0.30f, 0.96f), new Color(0.10f, 0.14f, 0.20f, 0.92f));
        GUI.Label(new Rect(headerBandRect.x + 18f, headerBandRect.y + 10f, headerBandRect.width - 36f, 24f), SafeShellText(panelContext.VisibleAffordanceSummaryText), _panelTitleStyle);
        GUI.Label(new Rect(headerBandRect.x + 18f, headerBandRect.y + 34f, headerBandRect.width - 36f, 18f), SafeShellText(panelContext.RiskPreviewSummaryText), _captionStyle);

        PrototypeDungeonPanelOption[] options = panelContext != null && panelContext.Options != null
            ? panelContext.Options
            : System.Array.Empty<PrototypeDungeonPanelOption>();
        string hoveredOptionId = string.Empty;
        float laneAreaTop = headerBandRect.yMax + 18f;
        float laneHeight = Mathf.Clamp((innerRect.height - 166f - (DungeonShellGap * 2f)) / 3f, 108f, 142f);

        for (int i = 0; i < options.Length && i < 3; i++)
        {
            PrototypeDungeonPanelOption option = options[i];
            Rect cardRect = new Rect(innerRect.x + 14f, laneAreaTop + ((laneHeight + DungeonShellGap) * i), innerRect.width - 28f, laneHeight);
            bool hovered;
            bool clicked = DrawDungeonOptionCard(
                cardRect,
                SafeShellText(option.OptionLabel),
                BuildDungeonChip("Lane", option.LaneLabel),
                BuildDungeonLines(
                    SafeShellText(option.OutcomeHintText),
                    "Risk: " + SafeShellText(option.RiskHintText),
                    "Reward: " + SafeShellText(option.RewardHintText)),
                option.IsBlocked ? "Blocked" : ("[" + (i + 1) + "] Commit lane"),
                i == 0 ? new Color(0.38f, 0.70f, 0.86f, 1f) : i == 1 ? new Color(0.82f, 0.76f, 0.40f, 1f) : new Color(0.88f, 0.54f, 0.36f, 1f),
                ResolveOptionTag(option),
                option.IsBlocked,
                out hovered);
            if (hovered)
            {
                hoveredOptionId = option.OptionId;
            }

            if (clicked)
            {
                _bootEntry.TryTriggerDungeonPanelLaneOption(option.OptionId);
            }
        }

        _bootEntry.SetDungeonPanelLaneHover(hoveredOptionId);

        Rect footerRect = new Rect(innerRect.x, rect.yMax - 78f, innerRect.width, 62f);
        DrawPanel(footerRect, new Color(0.12f, 0.16f, 0.22f, 0.96f), new Color(0.08f, 0.11f, 0.16f, 0.92f));
        GUI.Label(new Rect(footerRect.x + 16f, footerRect.y + 10f, footerRect.width - 32f, 18f), "Current Lane: " + SafeShellText(panelContext.CurrentLaneSelectionText), _bodyStyle);
        GUI.Label(new Rect(footerRect.x + 16f, footerRect.y + 30f, footerRect.width - 32f, 18f), "Delta: " + SafeShellText(panelContext.LaneDeltaSummaryText), _captionStyle);
    }
    private void DrawDungeonDecisionShell(Rect screenRect, PrototypeDungeonRunShellSurfaceData shellSurface, bool isPreElite)
    {
        PrototypeDungeonPanelContext panelContext = shellSurface != null ? shellSurface.PanelContext : new PrototypeDungeonPanelContext();
        string title = isPreElite ? shellSurface.PreEliteTitleText : shellSurface.EventTitleText;
        string description = isPreElite ? shellSurface.PreEliteDescriptionText : shellSurface.EventDescriptionText;
        string prompt = isPreElite ? shellSurface.PreElitePromptText : shellSurface.EventPromptText;
        PrototypeDungeonPanelOption[] options = panelContext != null && panelContext.Options != null
            ? panelContext.Options
            : System.Array.Empty<PrototypeDungeonPanelOption>();
        Rect layoutRect = CenterRect(Mathf.Min(Screen.width - 80f, 1260f), Mathf.Min(Screen.height - 80f, 720f));
        Rect headerRect = new Rect(layoutRect.x, layoutRect.y, layoutRect.width, DungeonHeaderHeight);
        Rect bodyRect = new Rect(layoutRect.x, headerRect.yMax + DungeonShellGap, layoutRect.width, layoutRect.height - DungeonHeaderHeight - DungeonShellGap);
        DrawDungeonHeaderBar(
            headerRect,
            SafeShellText(title),
            BuildDungeonLines(SafeShellText(description), SafeShellText(prompt)),
            BuildDungeonHeaderChips(
                BuildDungeonChip("Dungeon", shellSurface.CurrentDungeonLabel),
                BuildDungeonChip("Route", shellSurface.CurrentRouteLabel),
                BuildDungeonChip("Party HP", shellSurface.TotalPartyHpText),
                BuildDungeonChip("Condition", shellSurface.PartyConditionText),
                BuildDungeonChip("Elite", shellSurface.EliteEncounterNameText)));

        float leftWidth = Mathf.Clamp(bodyRect.width * 0.60f, 620f, 780f);
        Rect optionsRect = new Rect(bodyRect.x, bodyRect.y, leftWidth, bodyRect.height);
        Rect summaryRect = new Rect(optionsRect.xMax + DungeonShellGap, bodyRect.y, bodyRect.width - leftWidth - DungeonShellGap, bodyRect.height);
        DrawPanel(optionsRect, new Color(0.12f, 0.18f, 0.24f, 0.98f), new Color(0.08f, 0.11f, 0.16f, 0.94f));

        string hoveredOptionId = string.Empty;
        float optionHeight = Mathf.Clamp((optionsRect.height - 82f - DungeonShellGap) * 0.5f, 186f, 244f);
        float optionY = optionsRect.y + 18f;
        for (int i = 0; i < options.Length && i < 2; i++)
        {
            PrototypeDungeonPanelOption option = options[i];
            Rect optionRect = new Rect(optionsRect.x + 18f, optionY + ((optionHeight + DungeonShellGap) * i), optionsRect.width - 36f, optionHeight);
            bool hovered;
            bool clicked = DrawDungeonOptionCard(
                optionRect,
                SafeShellText(option.OptionLabel),
                BuildDecisionOptionSubtitle(isPreElite, option),
                BuildDungeonLines(
                    "Commit: " + SafeShellText(option.OutcomeHintText),
                    "Cost: " + SafeShellText(option.RiskHintText),
                    "Payoff: " + SafeShellText(option.RewardHintText)),
                i == 0 ? "[1] Take This" : "[2] Take This",
                isPreElite ? new Color(0.66f, 0.48f, 0.28f, 1f) : new Color(0.34f, 0.62f, 0.54f, 1f),
                ResolveOptionTag(option),
                option.IsBlocked,
                out hovered);
            if (hovered)
            {
                hoveredOptionId = option.OptionId;
            }

            if (clicked)
            {
                if (isPreElite)
                {
                    _bootEntry.TryTriggerDungeonRunPreEliteDecision(option.OptionId);
                }
                else
                {
                    _bootEntry.TryTriggerDungeonRunEventDecision(option.OptionId);
                }
            }
        }

        if (isPreElite)
        {
            _bootEntry.SetDungeonRunPreEliteDecisionHover(hoveredOptionId);
        }
        else
        {
            _bootEntry.SetDungeonRunEventDecisionHover(hoveredOptionId);
        }

        DrawDungeonInfoCard(
            summaryRect,
            isPreElite ? "Elite Stakes" : "Shrine Stakes",
            BuildDungeonLines(
                "Decision Prompt: " + SafeShellText(prompt),
                "Room Identity: " + SafeShellText(panelContext.RoomDisplayLabel),
                "Party HP: " + SafeShellText(shellSurface.TotalPartyHpText),
                "Party Condition: " + SafeShellText(shellSurface.PartyConditionText),
                (isPreElite ? "Elite Threat: " : "Shrine Outcome: ") + SafeShellText(isPreElite ? shellSurface.EliteEncounterNameText : shellSurface.EventChoiceText),
                (isPreElite ? "Preparation: " : "Preparation Ahead: ") + SafeShellText(shellSurface.PreEliteChoiceText),
                "Current Stakes: " + SafeShellText(shellSurface.EliteRewardHintText),
                "Latest Shift: " + SafeShellText(panelContext.LatestChoiceOutcomeSummaryText),
                "Resolution Chain: " + SafeShellText(panelContext.EventResolutionSummaryText),
                "Loot Breakdown: " + SafeShellText(shellSurface.LootBreakdownText),
                "Controls: [1] [2] or click card"));
    }

    private void DrawDungeonBattleShell(Rect screenRect)
    {
        if (_gameplayBattleHudPresenter == null)
        {
            _gameplayBattleHudPresenter = new GameplayBattleHudPresenter(this);
        }

        _gameplayBattleHudPresenter.Draw(screenRect);
    }

    private void DrawGameplayBattleHud(Rect screenRect)
    {
        PrototypeBattleUiSurfaceData surface = _bootEntry.GetBattleUiSurfaceData();
        Rect layoutRect = new Rect(DungeonShellMargin, DungeonShellMargin, screenRect.width - (DungeonShellMargin * 2f), screenRect.height - (DungeonShellMargin * 2f));
        float sectionGap = Mathf.Clamp(DungeonShellGap * 0.75f, 6f, 10f);
        float topStripHeight = Mathf.Clamp(layoutRect.height * 0.085f, 84f, 96f);
        float bottomHeight = Mathf.Clamp(layoutRect.height * 0.26f, 220f, 274f);
        Rect topStripRect = new Rect(layoutRect.x, layoutRect.y, layoutRect.width, topStripHeight);
        Rect bottomRect = new Rect(layoutRect.x, layoutRect.yMax - bottomHeight, layoutRect.width, bottomHeight);
        Rect stageRect = new Rect(
            layoutRect.x,
            topStripRect.yMax + sectionGap,
            layoutRect.width,
            bottomRect.y - topStripRect.yMax - (sectionGap * 2f));

        DrawDungeonBattleTopStrip(topStripRect, surface);

        string hoveredTargetId;
        DrawDungeonBattleStage(stageRect, surface, out hoveredTargetId);
        _bootEntry.SetBattleTargetHover(surface.TargetSelection.IsActive ? hoveredTargetId : string.Empty);

        string hoveredActionKey;
        DrawDungeonBattleBottomHud(bottomRect, surface, out hoveredActionKey);
        _bootEntry.SetBattleActionHover(hoveredActionKey);

        DrawDungeonBattleFocusOverlay(stageRect, bottomRect, surface);
    }
    private void DrawDungeonBattleCenter(Rect rect, PrototypeBattleUiSurfaceData surface)
    {
        Rect topRect = new Rect(rect.x, rect.y, rect.width, rect.height * 0.30f);
        Rect midRect = new Rect(rect.x, topRect.yMax + DungeonShellGap, rect.width, rect.height * 0.28f);
        Rect bottomRect = new Rect(rect.x, midRect.yMax + DungeonShellGap, rect.width, rect.yMax - midRect.yMax - DungeonShellGap);

        DrawDungeonInfoCard(
            topRect,
            "Battle Context",
            BuildDungeonLines(
                "Current Actor: " + SafeShellText(surface.CurrentActor.DisplayName),
                "Action: " + SafeShellText(surface.ActionContext.SelectedActionLabel),
                "Range: " + SafeShellText(surface.ActionContext.RangeText),
                "Reachability: " + SafeShellText(surface.ActionContext.ReachabilitySummaryText),
                "Threat: " + SafeShellText(surface.ActionContext.ThreatSummaryText),
                "Request: " + SafeShellText(surface.BattleContext.EncounterRequestSummaryText),
                "Runtime: " + SafeShellText(surface.BattleResult.RuntimeSummaryText),
                "Cancel: " + SafeShellText(surface.MessageSurface.CancelHint)));

        string targetBody = surface.TargetSelection.IsActive
            ? BuildDungeonLines(
                SafeShellText(surface.TargetSelection.Title),
                "Queued Action: " + SafeShellText(surface.TargetSelection.QueuedActionLabel),
                "Target: " + SafeShellText(surface.TargetSelection.TargetLabel),
                "Target Rule: " + SafeShellText(surface.TargetSelection.TargetRuleText),
                "Reachability: " + SafeShellText(surface.TargetSelection.ReachabilitySummaryText),
                "Threat: " + SafeShellText(surface.TargetSelection.ThreatSummaryText),
                "Hint: " + SafeShellText(surface.TargetSelection.CancelHint))
            : BuildDungeonLines(
                "Feedback: " + SafeShellText(surface.MessageSurface.Feedback),
                SafeShellText(surface.BattleResult.ResultSummaryText));
        DrawDungeonInfoCard(midRect, surface.TargetSelection.IsActive ? "Target Window" : "Battle Feed", targetBody);

        DrawPanel(bottomRect, new Color(0.12f, 0.18f, 0.24f, 0.98f), new Color(0.08f, 0.11f, 0.16f, 0.94f));
        GUI.Label(new Rect(bottomRect.x + 14f, bottomRect.y + 10f, bottomRect.width - 28f, 22f), "Command Deck", _panelTitleStyle);
        float logsHeight = 62f;
        GUI.Label(new Rect(bottomRect.x + 14f, bottomRect.y + 34f, bottomRect.width - 28f, logsHeight), BuildRecentBattleLogs(surface.MessageSurface.RecentLogs), _captionStyle);

        string hoveredActionKey = string.Empty;
        PrototypeBattleUiCommandDetailData[] details = surface.CommandSurface != null && surface.CommandSurface.Details != null
            ? surface.CommandSurface.Details
            : System.Array.Empty<PrototypeBattleUiCommandDetailData>();
        float buttonTop = bottomRect.y + 34f + logsHeight + 8f;
        float buttonWidth = (bottomRect.width - 42f - DungeonShellGap) * 0.5f;
        float buttonHeight = 58f;
        for (int i = 0; i < details.Length; i++)
        {
            PrototypeBattleUiCommandDetailData detail = details[i];
            int row = i / 2;
            int column = i % 2;
            Rect buttonRect = new Rect(bottomRect.x + 14f + ((buttonWidth + DungeonShellGap) * column), buttonTop + ((buttonHeight + 10f) * row), buttonWidth, buttonHeight);
            bool hovered;
            bool clicked = DrawDungeonOptionCard(
                buttonRect,
                SafeShellText(detail.Label),
                SafeShellText(detail.TargetText),
                BuildDungeonLines(
                    SafeShellText(detail.Description),
                    SafeShellText(detail.EffectText),
                    SafeShellText(detail.CostText)),
                detail.IsAvailable ? "Click or use hotkey" : "Unavailable",
                new Color(0.30f, 0.46f, 0.66f, 1f),
                detail.IsSelected ? "Queued" : string.Empty,
                !detail.IsAvailable,
                out hovered);
            if (hovered)
            {
                hoveredActionKey = detail.Key;
            }

            if (clicked)
            {
                _bootEntry.TryTriggerBattleAction(detail.Key);
            }
        }

        _bootEntry.SetBattleActionHover(hoveredActionKey);
    }

    private void DrawDungeonBattleEnemies(Rect rect, PrototypeBattleUiSurfaceData surface)
    {
        DrawPanel(rect, new Color(0.12f, 0.18f, 0.24f, 0.98f), new Color(0.08f, 0.11f, 0.16f, 0.94f));
        GUI.Label(new Rect(rect.x + 14f, rect.y + 10f, rect.width - 28f, 22f), "Threat Lanes", _panelTitleStyle);

        PrototypeBattleUiEnemyData[] enemies = surface.EnemyRoster != null ? surface.EnemyRoster : System.Array.Empty<PrototypeBattleUiEnemyData>();
        string hoveredTargetId = string.Empty;
        float cardHeight = Mathf.Clamp((rect.height - 108f - (DungeonShellGap * 2f)) / 3f, 94f, 126f);
        float cardY = rect.y + 40f;
        for (int i = 0; i < enemies.Length; i++)
        {
            PrototypeBattleUiEnemyData enemy = enemies[i];
            Rect cardRect = new Rect(rect.x + 14f, cardY + ((cardHeight + 10f) * i), rect.width - 28f, cardHeight);
            bool canTarget = surface.TargetSelection.IsActive && enemy.IsReachableByCurrentAction && !enemy.IsDefeated;
            bool hovered;
            bool clicked = DrawDungeonOptionCard(
                cardRect,
                SafeShellText(enemy.DisplayName),
                BuildDungeonChip("Role", enemy.RoleLabel),
                BuildDungeonLines(
                    "HP: " + enemy.CurrentHp + " / " + enemy.MaxHp,
                    "Intent: " + SafeShellText(enemy.IntentLabel),
                    "Lane: " + SafeShellText(enemy.LaneLabel),
                    SafeShellText(enemy.TraitText)),
                canTarget ? "Click to target" : SafeShellText(enemy.StateLabel),
                enemy.IsElite ? new Color(0.82f, 0.48f, 0.34f, 1f) : new Color(0.56f, 0.36f, 0.34f, 1f),
                enemy.IsSelected ? "Selected" : enemy.IsHovered ? "Hover" : enemy.IsElite ? "Elite" : string.Empty,
                !canTarget,
                out hovered);
            if (hovered)
            {
                hoveredTargetId = enemy.MonsterId;
            }

            if (clicked)
            {
                _bootEntry.TryTriggerBattleTarget(enemy.MonsterId);
            }
        }

        _bootEntry.SetBattleTargetHover(surface.TargetSelection.IsActive ? hoveredTargetId : string.Empty);

        Rect footerRect = new Rect(rect.x + 14f, rect.yMax - 52f, rect.width - 28f, 38f);
        DrawPanel(footerRect, new Color(0.14f, 0.18f, 0.24f, 0.96f), new Color(0.08f, 0.11f, 0.16f, 0.92f));
        GUI.Label(new Rect(footerRect.x + 10f, footerRect.y + 10f, footerRect.width - 20f, 18f), "Reward Hint: " + SafeShellText(surface.EliteRewardHintText), _captionStyle);
    }

    private void DrawDungeonBattleTopStrip(Rect rect, PrototypeBattleUiSurfaceData surface)
    {
        DrawRect(rect, new Color(0.05f, 0.07f, 0.11f, 0.88f));
        DrawRect(new Rect(rect.x, rect.yMax - 2f, rect.width, 2f), new Color(0.20f, 0.26f, 0.34f, 0.34f));
        float rightWidth = Mathf.Clamp(rect.width * 0.18f, 260f, 340f);
        float leftWidth = Mathf.Clamp(rect.width * 0.26f, 320f, 430f);
        Rect leftRect = new Rect(rect.x + 12f, rect.y + 8f, leftWidth, rect.height - 16f);
        Rect rightRect = new Rect(rect.xMax - rightWidth - 12f, rect.y + 8f, rightWidth, rect.height - 16f);
        Rect timelineRect = new Rect(leftRect.xMax + 14f, rect.y + 12f, rightRect.x - leftRect.xMax - 28f, rect.height - 24f);

        GUIStyle encounterStyle = new GUIStyle(_bodyStyle);
        encounterStyle.fontSize = 30;
        encounterStyle.fontStyle = FontStyle.Bold;
        encounterStyle.wordWrap = false;
        encounterStyle.clipping = TextClipping.Clip;
        GUI.Label(new Rect(leftRect.x, leftRect.y - 1f, leftRect.width, 32f), CompactShellText(SafeShellText(surface.EncounterName), 24), encounterStyle);

        List<string> metaChips = new List<string>();
        if (HasMeaningfulValue(surface.CurrentDungeonName))
        {
            metaChips.Add(CompactShellText(surface.CurrentDungeonName, 18));
        }

        if (HasMeaningfulValue(surface.CurrentRouteLabel))
        {
            metaChips.Add(CompactShellText(surface.CurrentRouteLabel, 18));
        }

        if (HasMeaningfulValue(surface.EncounterRoomType))
        {
            metaChips.Add(CompactShellText(surface.EncounterRoomType, 16));
        }

        if (HasMeaningfulValue(surface.EliteStatusText) && surface.EliteStatusText != "Pending")
        {
            metaChips.Add("Elite " + CompactShellText(surface.EliteStatusText, 12));
        }

        float metaX = leftRect.x;
        float metaY = leftRect.y + 40f;
        for (int i = 0; i < metaChips.Count && i < 4; i++)
        {
            string chipText = metaChips[i];
            float chipWidth = Mathf.Clamp(_badgeStyle.CalcSize(new GUIContent(chipText)).x + 34f, 96f, 196f);
            DrawPill(new Rect(metaX, metaY, chipWidth, 24f), chipText, new Color(0.18f, 0.24f, 0.32f, 0.96f), Color.white);
            metaX += chipWidth + 6f;
        }

        PrototypeBattleUiTimelineSlotData[] slots = surface != null && surface.Timeline != null && surface.Timeline.Slots != null
            ? surface.Timeline.Slots
            : System.Array.Empty<PrototypeBattleUiTimelineSlotData>();
        float chipX = timelineRect.x;
        float chipY = timelineRect.y + Mathf.Max(8f, (timelineRect.height - 30f) * 0.5f);
        for (int i = 0; i < slots.Length && i < 5; i++)
        {
            PrototypeBattleUiTimelineSlotData slot = slots[i] ?? new PrototypeBattleUiTimelineSlotData();
            string chipText = CompactShellText(
                CompactShellText(slot.Label, 10) + (HasMeaningfulValue(slot.LaneLabel) ? " " + BuildDungeonBattleLaneTag(slot.LaneLabel) : string.Empty),
                14);
            float chipWidth = Mathf.Clamp(_badgeStyle.CalcSize(new GUIContent(chipText)).x + 32f, 104f, 156f);
            if (chipX + chipWidth > timelineRect.xMax)
            {
                break;
            }

            Color fill = slot.IsCurrent
                ? new Color(0.76f, 0.58f, 0.24f, 0.96f)
                : slot.IsEnemy
                    ? new Color(0.48f, 0.25f, 0.22f, 0.96f)
                    : new Color(0.18f, 0.28f, 0.38f, 0.96f);
            DrawPill(new Rect(chipX, chipY, chipWidth, 30f), chipText, fill, Color.white);
            chipX += chipWidth + 6f;
            if (chipX > timelineRect.xMax - 72f)
            {
                break;
            }
        }

        string actorText = ChooseFirstMeaningfulDungeonText(
            surface != null && surface.CurrentActorSurface != null ? surface.CurrentActorSurface.DisplayName : string.Empty,
            surface != null && surface.CurrentActor != null ? surface.CurrentActor.DisplayName : string.Empty,
            "Current actor");
        string phaseText = ChooseFirstMeaningfulDungeonText(
            surface != null && surface.Timeline != null ? surface.Timeline.PhaseLabel : string.Empty,
            "Battle");
        string actionText = ChooseFirstMeaningfulDungeonText(
            surface != null && surface.CommandSurface != null ? surface.CommandSurface.SelectedActionLabel : string.Empty,
            surface != null && surface.ActionContext != null ? surface.ActionContext.SelectedActionLabel : string.Empty,
            "Await command");
        GUIStyle rightPrimaryStyle = new GUIStyle(_bodyStyle);
        rightPrimaryStyle.fontSize = 22;
        rightPrimaryStyle.fontStyle = FontStyle.Bold;
        rightPrimaryStyle.wordWrap = false;
        rightPrimaryStyle.clipping = TextClipping.Clip;
        GUIStyle rightMetaStyle = new GUIStyle(_captionStyle);
        rightMetaStyle.fontSize = 18;
        rightMetaStyle.wordWrap = false;
        rightMetaStyle.clipping = TextClipping.Clip;
        DrawPill(new Rect(rightRect.x, rightRect.y + 2f, Mathf.Min(rightRect.width, 180f), 24f), "TURN", new Color(0.22f, 0.30f, 0.24f, 0.96f), Color.white);
        GUI.Label(new Rect(rightRect.x, rightRect.y + 30f, rightRect.width, 24f), CompactShellText(actorText, 18), rightPrimaryStyle);
        GUI.Label(new Rect(rightRect.x, rightRect.y + 54f, rightRect.width, 18f), CompactShellText(phaseText + " | " + actionText, 28), rightMetaStyle);
    }

    private void DrawDungeonBattleStage(Rect rect, PrototypeBattleUiSurfaceData surface, out string hoveredTargetId)
    {
        hoveredTargetId = string.Empty;
        DrawRect(rect, new Color(0.03f, 0.04f, 0.07f, 0.54f));
        Rect innerRect = new Rect(rect.x + 18f, rect.y + 12f, rect.width - 36f, rect.height - 24f);
        Rect allyAuraRect = new Rect(innerRect.x + (innerRect.width * 0.10f), innerRect.y + (innerRect.height * 0.18f), innerRect.width * 0.22f, innerRect.height * 0.50f);
        Rect enemyAuraRect = new Rect(innerRect.xMax - (innerRect.width * 0.32f), innerRect.y + (innerRect.height * 0.18f), innerRect.width * 0.22f, innerRect.height * 0.50f);
        Rect clashRect = new Rect(innerRect.center.x - (innerRect.width * 0.12f), innerRect.y + (innerRect.height * 0.12f), innerRect.width * 0.24f, innerRect.height * 0.42f);
        Rect floorRect = new Rect(innerRect.x + 90f, innerRect.yMax - 86f, innerRect.width - 180f, 24f);
        DrawRect(allyAuraRect, new Color(0.22f, 0.34f, 0.42f, 0.10f));
        DrawRect(enemyAuraRect, new Color(0.42f, 0.24f, 0.20f, 0.10f));
        DrawRect(clashRect, new Color(0.40f, 0.34f, 0.16f, 0.06f));
        DrawRect(floorRect, new Color(0.18f, 0.18f, 0.20f, 0.20f));
        DrawRotatedRect(new Rect(innerRect.center.x - (innerRect.width * 0.20f), innerRect.yMax - 118f, innerRect.width * 0.40f, 10f), -2.2f, new Color(0.72f, 0.62f, 0.34f, 0.08f));

        float formationWidth = innerRect.width * 0.42f;
        Rect partyRect = new Rect(innerRect.x + 20f, innerRect.y + 8f, formationWidth, innerRect.height - 36f);
        Rect enemyRect = new Rect(innerRect.xMax - formationWidth - 20f, innerRect.y + 8f, formationWidth, innerRect.height - 36f);

        DrawDungeonBattleRowGuides(partyRect, true);
        DrawDungeonBattleRowGuides(enemyRect, false);
        DrawDungeonBattlePartyFormation(partyRect, surface != null ? surface.PartyMembers : null);
        DrawDungeonBattleEnemyFormation(enemyRect, surface, out hoveredTargetId);
    }

    private void DrawDungeonBattlePartyFormation(Rect rect, PrototypeBattleUiPartyMemberData[] members)
    {
        PrototypeBattleUiPartyMemberData[] safeMembers = members ?? System.Array.Empty<PrototypeBattleUiPartyMemberData>();
        int[] laneCounts = new int[3];
        List<PrototypeBattleUiPartyMemberData> sortedMembers = new List<PrototypeBattleUiPartyMemberData>(safeMembers);
        sortedMembers.Sort((left, right) =>
        {
            int laneCompare = ResolveDungeonBattleLaneIndex(left != null ? left.LaneKey : string.Empty, left != null ? left.LaneLabel : string.Empty)
                .CompareTo(ResolveDungeonBattleLaneIndex(right != null ? right.LaneKey : string.Empty, right != null ? right.LaneLabel : string.Empty));
            if (laneCompare != 0)
            {
                return laneCompare;
            }

            return (left != null ? left.SlotIndex : 0).CompareTo(right != null ? right.SlotIndex : 0);
        });

        for (int i = 0; i < sortedMembers.Count; i++)
        {
            PrototypeBattleUiPartyMemberData member = sortedMembers[i];
            if (member == null)
            {
                continue;
            }

            int laneIndex = ResolveDungeonBattleLaneIndex(member.LaneKey, member.LaneLabel);
            int duplicateIndex = laneCounts[laneIndex]++;
            Color accent = ResolveDungeonBattlePartyDangerColor(member);
            bool hovered;
            bool frontRow = laneIndex == 0;
            float cardWidth = frontRow
                ? Mathf.Clamp(rect.width * 0.34f, 224f, 278f)
                : Mathf.Clamp(rect.width * 0.30f, 200f, 246f);
            float cardHeight = frontRow
                ? Mathf.Clamp(rect.height * 0.36f, 180f, 232f)
                : Mathf.Clamp(rect.height * 0.32f, 164f, 214f);
            float anchorX = rect.x + (rect.width * ResolveDungeonBattleRowDepthAnchor(laneIndex, true)) + ResolveDungeonBattleFormationXOffset(duplicateIndex, true);
            float anchorY = rect.y + (rect.height * ResolveDungeonBattleRowVerticalAnchor(laneIndex, true)) + ResolveDungeonBattleFormationYOffset(duplicateIndex);
            DrawDungeonBattlePartyToken(
                new Rect(anchorX - (cardWidth * 0.5f), anchorY - (cardHeight * 0.5f), cardWidth, cardHeight),
                member,
                accent,
                out hovered);
        }
    }

    private void DrawDungeonBattleEnemyFormation(Rect rect, PrototypeBattleUiSurfaceData surface, out string hoveredTargetId)
    {
        hoveredTargetId = string.Empty;
        PrototypeBattleUiEnemyData[] enemies = surface != null && surface.EnemyRoster != null ? surface.EnemyRoster : System.Array.Empty<PrototypeBattleUiEnemyData>();
        int[] laneCounts = new int[3];
        List<PrototypeBattleUiEnemyData> sortedEnemies = new List<PrototypeBattleUiEnemyData>(enemies);
        sortedEnemies.Sort((left, right) =>
        {
            int laneCompare = ResolveDungeonBattleLaneIndex(left != null ? left.LaneKey : string.Empty, left != null ? left.LaneLabel : string.Empty)
                .CompareTo(ResolveDungeonBattleLaneIndex(right != null ? right.LaneKey : string.Empty, right != null ? right.LaneLabel : string.Empty));
            if (laneCompare != 0)
            {
                return laneCompare;
            }

            return string.Compare(left != null ? left.MonsterId : string.Empty, right != null ? right.MonsterId : string.Empty, System.StringComparison.Ordinal);
        });

        for (int i = 0; i < sortedEnemies.Count; i++)
        {
            PrototypeBattleUiEnemyData enemy = sortedEnemies[i];
            if (enemy == null)
            {
                continue;
            }

            int laneIndex = ResolveDungeonBattleLaneIndex(enemy.LaneKey, enemy.LaneLabel);
            int duplicateIndex = laneCounts[laneIndex]++;
            bool canTarget = surface != null && surface.TargetSelection != null && surface.TargetSelection.IsActive && enemy.IsReachableByCurrentAction && !enemy.IsDefeated;
            bool hovered;
            bool frontRow = laneIndex == 0;
            float cardWidth = frontRow
                ? Mathf.Clamp(rect.width * 0.34f, 224f, 278f)
                : Mathf.Clamp(rect.width * 0.30f, 200f, 246f);
            float cardHeight = frontRow
                ? Mathf.Clamp(rect.height * 0.36f, 180f, 232f)
                : Mathf.Clamp(rect.height * 0.32f, 164f, 214f);
            float anchorX = rect.x + (rect.width * ResolveDungeonBattleRowDepthAnchor(laneIndex, false)) + ResolveDungeonBattleFormationXOffset(duplicateIndex, false);
            float anchorY = rect.y + (rect.height * ResolveDungeonBattleRowVerticalAnchor(laneIndex, false)) + ResolveDungeonBattleFormationYOffset(duplicateIndex);
            bool clicked = DrawDungeonBattleEnemyToken(
                new Rect(anchorX - (cardWidth * 0.5f), anchorY - (cardHeight * 0.5f), cardWidth, cardHeight),
                enemy,
                canTarget,
                out hovered);
            if (hovered && canTarget)
            {
                hoveredTargetId = enemy.MonsterId;
            }

            if (clicked)
            {
                _bootEntry.TryTriggerBattleTarget(enemy.MonsterId);
            }
        }
    }

    private void DrawDungeonBattleBottomHud(Rect rect, PrototypeBattleUiSurfaceData surface, out string hoveredActionKey)
    {
        hoveredActionKey = string.Empty;
        DrawRect(rect, new Color(0.03f, 0.04f, 0.07f, 0.86f));
        DrawRect(new Rect(rect.x, rect.y, rect.width, 2f), new Color(0.20f, 0.26f, 0.34f, 0.34f));
        float leftWidth = Mathf.Clamp(rect.width * 0.20f, 300f, 360f);
        float rightWidth = Mathf.Clamp(rect.width * 0.25f, 340f, 430f);
        Rect leftRect = new Rect(rect.x + 12f, rect.y + 12f, leftWidth, rect.height - 24f);
        Rect rightRect = new Rect(rect.xMax - rightWidth - 12f, rect.y + 12f, rightWidth, rect.height - 24f);
        Rect centerRect = new Rect(leftRect.xMax + DungeonShellGap, rect.y + 12f, rect.width - leftWidth - rightWidth - (DungeonShellGap * 2f) - 24f, rect.height - 24f);

        DrawDungeonBattleCurrentActorPanel(leftRect, surface != null ? surface.CurrentActorSurface : null);
        DrawDungeonBattleCommandPanel(centerRect, surface, out hoveredActionKey);
        DrawDungeonBattlePartyStrip(rightRect, surface != null ? surface.PartyStatusSurfaces : null);
    }

    private void DrawDungeonBattleCurrentActorPanel(Rect rect, CurrentActorSurfaceData actorSurface)
    {
        CurrentActorSurfaceData actor = actorSurface ?? new CurrentActorSurfaceData();
        DrawPanel(rect, new Color(0.10f, 0.14f, 0.20f, 0.90f), new Color(0.06f, 0.08f, 0.12f, 0.84f));
        DrawPill(new Rect(rect.x + 14f, rect.y + 12f, 76f, 24f), "TURN", new Color(0.22f, 0.30f, 0.24f, 0.96f), Color.white);

        Rect portraitRect = new Rect(rect.x + 14f, rect.y + 44f, 82f, 82f);
        DrawPanel(portraitRect, new Color(0.24f, 0.34f, 0.44f, 0.94f), new Color(0.10f, 0.14f, 0.20f, 0.90f));
        GUIStyle glyphStyle = new GUIStyle(_heroSubtitleStyle);
        glyphStyle.fontSize = 38;
        glyphStyle.alignment = TextAnchor.MiddleCenter;
        GUI.Label(new Rect(portraitRect.x, portraitRect.y + 10f, portraitRect.width, 54f), CompactShellText(actor.PortraitGlyph, 2), glyphStyle);

        Rect textRect = new Rect(portraitRect.xMax + 16f, rect.y + 44f, rect.width - 126f, 90f);
        GUIStyle actorNameStyle = new GUIStyle(_bodyStyle);
        actorNameStyle.fontSize = 30;
        actorNameStyle.fontStyle = FontStyle.Bold;
        actorNameStyle.wordWrap = false;
        actorNameStyle.clipping = TextClipping.Clip;
        GUIStyle actorMetaStyle = new GUIStyle(_captionStyle);
        actorMetaStyle.fontSize = 18;
        actorMetaStyle.wordWrap = false;
        actorMetaStyle.clipping = TextClipping.Clip;
        GUI.Label(new Rect(textRect.x, textRect.y, textRect.width, 34f), CompactShellText(SafeShellText(actor.DisplayName), 16), actorNameStyle);
        GUI.Label(new Rect(textRect.x, textRect.y + 34f, textRect.width, 20f), CompactShellText(BuildDungeonBattleJoinedTagLine(SafeShellText(actor.RoleLabel), BuildDungeonBattleLaneTag(actor.LaneLabel)), 20), actorMetaStyle);
        GUI.Label(new Rect(textRect.x, textRect.y + 58f, textRect.width, 20f), CompactShellText(SafeShellText(actor.StatusText), 18), actorMetaStyle);

        string resourceLine = HasMeaningfulValue(actor.ResourceText)
            ? CompactShellText(actor.ResourceLabel + " " + SafeShellText(actor.ResourceText), 24)
            : CompactShellText(ChooseFirstMeaningfulDungeonText(actor.SkillLabel, actor.ResourceLabel), 24);
        GUI.Label(new Rect(rect.x + 14f, rect.yMax - 56f, rect.width - 28f, 20f), resourceLine, actorMetaStyle);
        Rect hpRect = new Rect(rect.x + 14f, rect.yMax - 30f, rect.width - 28f, 18f);
        DrawDungeonBattleMeterBar(hpRect, actor.MaxHp > 0 ? (float)actor.CurrentHp / actor.MaxHp : 0f, new Color(0.42f, 0.68f, 0.38f, 1f), "HP " + actor.CurrentHp + "/" + actor.MaxHp);
    }

    private void DrawDungeonBattleCommandPanel(Rect rect, PrototypeBattleUiSurfaceData surface, out string hoveredActionKey)
    {
        hoveredActionKey = string.Empty;
        PrototypeBattleUiCommandSurfaceData commandSurface = surface != null ? surface.CommandSurface : new PrototypeBattleUiCommandSurfaceData();
        PrototypeBattleUiCommandButtonData[] buttons = commandSurface != null && commandSurface.PrimaryButtons != null
            ? commandSurface.PrimaryButtons
            : System.Array.Empty<PrototypeBattleUiCommandButtonData>();
        PrototypeBattleUiCommandDetailData[] contextualDetails = commandSurface != null && commandSurface.ContextualDetails != null
            ? commandSurface.ContextualDetails
            : System.Array.Empty<PrototypeBattleUiCommandDetailData>();

        GUIStyle sectionTitleStyle = new GUIStyle(_panelTitleStyle);
        sectionTitleStyle.fontSize = 22;
        sectionTitleStyle.wordWrap = false;
        sectionTitleStyle.clipping = TextClipping.Clip;
        GUI.Label(new Rect(rect.x, rect.y + 4f, rect.width, 26f), "COMMANDS", sectionTitleStyle);
        float buttonWidth = (rect.width - (DungeonShellGap * 3f)) * 0.25f;
        float buttonY = rect.y + 38f;
        for (int i = 0; i < buttons.Length && i < 4; i++)
        {
            PrototypeBattleUiCommandButtonData button = buttons[i] ?? new PrototypeBattleUiCommandButtonData();
            bool hovered;
            bool clicked = DrawDungeonBattleCommandButton(
                new Rect(rect.x + ((buttonWidth + DungeonShellGap) * i), buttonY, buttonWidth, 96f),
                button,
                out hovered);
            if (hovered && CanHoverDungeonBattleAction(button.Key, button.IsAvailable))
            {
                hoveredActionKey = button.Key;
            }

            if (clicked && CanTriggerDungeonBattleAction(button.Key, button.IsAvailable))
            {
                _bootEntry.TryTriggerBattleAction(button.Key);
            }
        }

        float utilityY = rect.yMax - 36f;
        float utilityX = rect.x;
        for (int i = 0; i < contextualDetails.Length && i < 3; i++)
        {
            PrototypeBattleUiCommandDetailData detail = contextualDetails[i] ?? new PrototypeBattleUiCommandDetailData();
            if (detail.Key != "move")
            {
                continue;
            }

            float width = Mathf.Clamp(_badgeStyle.CalcSize(new GUIContent(detail.Label)).x + 56f, 108f, 156f);
            bool enabled = detail.IsAvailable && CanTriggerDungeonBattleAction(detail.Key, detail.IsAvailable);
            if (DrawActionButton(new Rect(utilityX, utilityY, width, 30f), detail.Label, new Color(0.20f, 0.30f, 0.40f, 1f), enabled, _badgeStyle))
            {
                _bootEntry.TryTriggerBattleAction(detail.Key);
            }

            if (new Rect(utilityX, utilityY, width, 30f).Contains(Event.current.mousePosition) && enabled)
            {
                hoveredActionKey = detail.Key;
            }

            utilityX += width + 8f;
        }

        if (surface != null && surface.TargetSelection != null && surface.TargetSelection.IsActive)
        {
            GUIStyle targetPromptStyle = new GUIStyle(_captionStyle);
            targetPromptStyle.fontSize = 18;
            targetPromptStyle.wordWrap = false;
            targetPromptStyle.clipping = TextClipping.Clip;
            GUI.Label(
                new Rect(rect.x + 160f, rect.yMax - 28f, rect.width - 160f, 18f),
                CompactShellText("Select target | " + SafeShellText(surface.TargetSelection.CancelHint), 48),
                targetPromptStyle);
        }
    }

    private void DrawDungeonBattlePartyStrip(Rect rect, PartyStatusSurfaceData[] partySurfaces)
    {
        PartyStatusSurfaceData[] safeSurfaces = partySurfaces ?? System.Array.Empty<PartyStatusSurfaceData>();
        int visibleCount = Mathf.Min(4, safeSurfaces.Length);
        if (visibleCount <= 0)
        {
            return;
        }

        GUIStyle sectionTitleStyle = new GUIStyle(_panelTitleStyle);
        sectionTitleStyle.fontSize = 22;
        sectionTitleStyle.wordWrap = false;
        sectionTitleStyle.clipping = TextClipping.Clip;
        GUI.Label(new Rect(rect.x, rect.y + 4f, rect.width, 26f), "PARTY", sectionTitleStyle);
        float cellGap = 8f;
        float cardWidth = (rect.width - cellGap) * 0.5f;
        float cardHeight = (rect.height - 40f - cellGap) * 0.5f;
        for (int i = 0; i < safeSurfaces.Length && i < 4; i++)
        {
            PartyStatusSurfaceData member = safeSurfaces[i] ?? new PartyStatusSurfaceData();
            Color accent = ResolveDungeonBattleDangerColor(member.DangerStateKey);
            bool hovered;
            int row = i / 2;
            int column = i % 2;
            DrawDungeonBattlePartyStatusRow(
                new Rect(rect.x + ((cardWidth + cellGap) * column), rect.y + 34f + ((cardHeight + cellGap) * row), cardWidth, cardHeight),
                member,
                accent,
                out hovered);
        }
    }

    private bool DrawDungeonBattleCommandButton(Rect rect, PrototypeBattleUiCommandButtonData button, out bool hovered)
    {
        PrototypeBattleUiCommandButtonData safeButton = button ?? new PrototypeBattleUiCommandButtonData();
        hovered = rect.Contains(Event.current.mousePosition);
        bool clickable = safeButton.IsAvailable && CanTriggerDungeonBattleAction(safeButton.Key, safeButton.IsAvailable);
        GUIStyle titleStyle = new GUIStyle(_bodyStyle);
        titleStyle.fontSize = 26;
        titleStyle.fontStyle = FontStyle.Bold;
        titleStyle.wordWrap = false;
        titleStyle.clipping = TextClipping.Clip;
        GUIStyle metaStyle = new GUIStyle(_captionStyle);
        metaStyle.fontSize = 18;
        metaStyle.wordWrap = false;
        metaStyle.clipping = TextClipping.Clip;
        metaStyle.alignment = TextAnchor.UpperLeft;
        GUIStyle hotkeyStyle = new GUIStyle(metaStyle);
        hotkeyStyle.alignment = TextAnchor.UpperRight;
        Color accent = safeButton.IsSelected
            ? new Color(0.72f, 0.56f, 0.24f, 1f)
            : safeButton.IsAvailable
                ? new Color(0.24f, 0.38f, 0.50f, 1f)
                : new Color(0.20f, 0.20f, 0.24f, 1f);
        DrawPanel(rect, hovered && safeButton.IsAvailable ? accent : new Color(0.12f, 0.16f, 0.22f, 0.98f), new Color(0.08f, 0.10f, 0.14f, 0.94f));
        GUI.Label(new Rect(rect.x + 16f, rect.y + 18f, rect.width - 32f, 30f), SafeShellText(safeButton.Label), titleStyle);
        GUI.Label(new Rect(rect.x + 16f, rect.y + 54f, rect.width - 92f, 20f), CompactShellText(SafeShellText(safeButton.FooterText), 20), metaStyle);
        GUI.Label(new Rect(rect.x + 16f, rect.yMax - 28f, rect.width - 32f, 18f), SafeShellText(safeButton.HotkeyText), hotkeyStyle);
        return clickable && GUI.Button(rect, GUIContent.none, GUIStyle.none);
    }

    private void DrawDungeonBattlePartyStatusRow(Rect rect, PartyStatusSurfaceData member, Color accentColor, out bool hovered)
    {
        PartyStatusSurfaceData safeMember = member ?? new PartyStatusSurfaceData();
        hovered = rect.Contains(Event.current.mousePosition);
        Color frameColor = safeMember.IsActive ? accentColor : new Color(0.10f, 0.14f, 0.20f, 0.74f);
        DrawPanel(rect, frameColor, new Color(0.06f, 0.08f, 0.12f, 0.74f));
        DrawRect(new Rect(rect.x + 6f, rect.y + 6f, 3f, rect.height - 12f), accentColor);
        Rect portraitRect = new Rect(rect.x + 12f, rect.y + 10f, 38f, 38f);
        DrawPanel(portraitRect, new Color(0.16f, 0.22f, 0.30f, 0.94f), new Color(0.10f, 0.14f, 0.20f, 0.90f));
        GUIStyle nameStyle = new GUIStyle(_bodyStyle);
        nameStyle.fontSize = 22;
        nameStyle.fontStyle = FontStyle.Bold;
        nameStyle.wordWrap = false;
        nameStyle.clipping = TextClipping.Clip;
        GUIStyle compactCaptionStyle = new GUIStyle(_captionStyle);
        compactCaptionStyle.fontSize = 18;
        compactCaptionStyle.wordWrap = false;
        compactCaptionStyle.clipping = TextClipping.Clip;
        GUIStyle portraitStyle = new GUIStyle(_captionStyle);
        portraitStyle.alignment = TextAnchor.MiddleCenter;
        portraitStyle.wordWrap = false;
        portraitStyle.fontSize = 18;
        portraitStyle.clipping = TextClipping.Clip;
        GUI.Label(new Rect(portraitRect.x, portraitRect.y + 6f, portraitRect.width, portraitRect.height - 12f), CompactShellText(SafeShellText(safeMember.DisplayName), 1).ToUpperInvariant(), portraitStyle);
        float contentX = portraitRect.xMax + 8f;
        float contentWidth = rect.xMax - contentX - 8f;
        GUI.Label(new Rect(contentX, rect.y + 10f, contentWidth, 22f), CompactShellText(safeMember.DisplayName, 10), nameStyle);
        GUI.Label(new Rect(contentX, rect.y + 34f, contentWidth, 18f), CompactShellText(BuildDungeonBattleJoinedTagLine(BuildDungeonBattleLaneTag(safeMember.LaneLabel), BuildDungeonBattlePartyStatusFooter(safeMember)), 16), compactCaptionStyle);
        DrawDungeonBattleMeterBar(
            new Rect(contentX, rect.yMax - 18f, contentWidth, 12f),
            safeMember.MaxHp > 0 ? (float)safeMember.CurrentHp / safeMember.MaxHp : 0f,
            accentColor,
            string.Empty);
    }

    private void DrawDungeonBattleFocusOverlay(Rect stageRect, Rect bottomRect, PrototypeBattleUiSurfaceData surface)
    {
        if (!ShouldShowDungeonBattleFocusOverlay(surface))
        {
            return;
        }

        bool targetFocus = surface != null && surface.TargetSelection != null && surface.TargetSelection.IsActive;
        float overlayWidth = Mathf.Clamp(stageRect.width * 0.24f, 300f, 380f);
        float overlayHeight = targetFocus ? 190f : 154f;
        Rect overlayRect = targetFocus
            ? new Rect(stageRect.xMax - overlayWidth - 20f, stageRect.center.y - (overlayHeight * 0.5f), overlayWidth, overlayHeight)
            : new Rect(bottomRect.center.x - (overlayWidth * 0.5f), stageRect.yMax - overlayHeight - 18f, overlayWidth, overlayHeight);
        DrawPanel(overlayRect, new Color(0.12f, 0.15f, 0.20f, 0.96f), new Color(0.05f, 0.07f, 0.10f, 0.92f));

        if (targetFocus)
        {
            PrototypeBattleUiTargetSelectionData target = surface.TargetSelection ?? new PrototypeBattleUiTargetSelectionData();
            GUIStyle overlayTitleStyle = new GUIStyle(_panelTitleStyle);
            overlayTitleStyle.fontSize = 22;
            overlayTitleStyle.wordWrap = false;
            overlayTitleStyle.clipping = TextClipping.Clip;
            GUIStyle overlayBodyStyle = new GUIStyle(_bodyStyle);
            overlayBodyStyle.fontSize = 18;
            overlayBodyStyle.wordWrap = false;
            overlayBodyStyle.clipping = TextClipping.Clip;
            GUIStyle overlayCaptionStyle = new GUIStyle(_captionStyle);
            overlayCaptionStyle.fontSize = 18;
            overlayCaptionStyle.wordWrap = false;
            overlayCaptionStyle.clipping = TextClipping.Clip;
            GUI.Label(new Rect(overlayRect.x + 16f, overlayRect.y + 12f, overlayRect.width - 32f, 28f), CompactShellText(SafeShellText(target.TargetLabel), 18), overlayTitleStyle);
            GUI.Label(new Rect(overlayRect.x + 16f, overlayRect.y + 42f, overlayRect.width - 32f, 22f), CompactShellText(ChooseFirstMeaningfulDungeonText(target.TargetIntentLabel, target.TargetStateText, "Target pending"), 32), overlayCaptionStyle);
            DrawDungeonBattleMeterBar(
                new Rect(overlayRect.x + 16f, overlayRect.y + 72f, overlayRect.width - 32f, 18f),
                target.TargetMaxHp > 0 ? (float)target.TargetCurrentHp / target.TargetMaxHp : 0f,
                new Color(0.72f, 0.34f, 0.26f, 1f),
                "HP " + target.TargetCurrentHp + "/" + target.TargetMaxHp);
            GUI.Label(new Rect(overlayRect.x + 16f, overlayRect.y + 98f, overlayRect.width - 32f, 20f), CompactShellText("Rule " + ChooseFirstMeaningfulDungeonText(target.TargetRuleText, target.ReachabilitySummaryText, "Select target"), 34), overlayBodyStyle);
            GUI.Label(new Rect(overlayRect.x + 16f, overlayRect.y + 124f, overlayRect.width - 32f, 18f), CompactShellText(ChooseFirstMeaningfulDungeonText(target.ThreatSummaryText, target.SkillHintText, target.TargetTraitText), 34), overlayCaptionStyle);
            GUI.Label(new Rect(overlayRect.x + 16f, overlayRect.yMax - 24f, overlayRect.width - 32f, 18f), CompactShellText(SafeShellText(target.CancelHint), 24), overlayCaptionStyle);
            return;
        }

        PrototypeBattleUiCommandDetailData detail = ResolveDungeonBattleFocusDetail(surface != null ? surface.CommandSurface : null, surface != null && surface.CommandSurface != null ? surface.CommandSurface.ContextualDetails : null);
        if (detail == null)
        {
            return;
        }

        GUIStyle commandOverlayTitleStyle = new GUIStyle(_panelTitleStyle);
        commandOverlayTitleStyle.fontSize = 22;
        commandOverlayTitleStyle.wordWrap = false;
        commandOverlayTitleStyle.clipping = TextClipping.Clip;
        GUIStyle commandOverlayBodyStyle = new GUIStyle(_bodyStyle);
        commandOverlayBodyStyle.fontSize = 18;
        commandOverlayBodyStyle.wordWrap = false;
        commandOverlayBodyStyle.clipping = TextClipping.Clip;
        GUIStyle commandOverlayCaptionStyle = new GUIStyle(_captionStyle);
        commandOverlayCaptionStyle.fontSize = 18;
        commandOverlayCaptionStyle.wordWrap = false;
        commandOverlayCaptionStyle.clipping = TextClipping.Clip;
        GUI.Label(new Rect(overlayRect.x + 16f, overlayRect.y + 12f, overlayRect.width - 32f, 28f), CompactShellText(SafeShellText(detail.Label), 16), commandOverlayTitleStyle);
        GUI.Label(new Rect(overlayRect.x + 16f, overlayRect.y + 42f, overlayRect.width - 32f, 20f), CompactShellText(ChooseFirstMeaningfulDungeonText(detail.Description, detail.EffectText, "Ready"), 34), commandOverlayCaptionStyle);
        GUI.Label(new Rect(overlayRect.x + 16f, overlayRect.y + 76f, overlayRect.width - 32f, 20f), CompactShellText("Target " + ChooseFirstMeaningfulDungeonText(detail.TargetText, "Self"), 34), commandOverlayBodyStyle);
        GUI.Label(new Rect(overlayRect.x + 16f, overlayRect.y + 102f, overlayRect.width - 32f, 20f), CompactShellText(ChooseFirstMeaningfulDungeonText(FormatDungeonBattleFocusLine("Effect", detail.EffectText), FormatDungeonBattleFocusLine("Cost", detail.CostText), string.Empty), 34), commandOverlayBodyStyle);
    }

    private bool ShouldShowDungeonBattleFocusOverlay(PrototypeBattleUiSurfaceData surface)
    {
        if (surface == null)
        {
            return false;
        }

        if (surface.TargetSelection != null && surface.TargetSelection.IsActive)
        {
            return true;
        }

        return ShouldShowDungeonBattleCommandFocus(surface.CommandSurface);
    }

    private bool ShouldShowDungeonBattleCommandFocus(PrototypeBattleUiCommandSurfaceData commandSurface)
    {
        string selectedKey = commandSurface != null ? commandSurface.SelectedActionKey : string.Empty;
        return selectedKey == "skill" || selectedKey == "move" || selectedKey == "retreat";
    }

    private bool DrawDungeonBattleCard(Rect rect, string title, string subtitle, string body, string footer, Color accentColor, bool clickable, bool disabled, out bool hovered)
    {
        hovered = rect.Contains(Event.current.mousePosition);
        bool compact = rect.height < 82f;
        Color borderColor = disabled
            ? new Color(0.18f, 0.18f, 0.22f, 0.92f)
            : hovered && clickable
                ? accentColor
                : new Color(0.12f, 0.16f, 0.22f, 0.96f);
        Color innerColor = disabled ? new Color(0.06f, 0.07f, 0.10f, 0.92f) : new Color(0.08f, 0.10f, 0.15f, 0.94f);
        DrawPanel(rect, borderColor, innerColor);
        DrawRect(new Rect(rect.x + 8f, rect.y + 10f, 5f, rect.height - 20f), new Color(accentColor.r, accentColor.g, accentColor.b, disabled ? 0.36f : 0.92f));
        float contentX = rect.x + 20f;
        float contentWidth = rect.width - 28f;
        GUI.Label(new Rect(contentX, rect.y + 8f, contentWidth, 20f), CompactShellText(title, compact ? 18 : 26), _panelTitleStyle);
        GUI.Label(new Rect(contentX, rect.y + 26f, contentWidth, 16f), CompactShellText(subtitle, compact ? 18 : 28), _captionStyle);
        if (compact)
        {
            GUI.Label(new Rect(contentX, rect.y + 42f, contentWidth, 14f), BuildDisplayBlock(body, 1, 28), _captionStyle);
            GUI.Label(new Rect(contentX, rect.yMax - 16f, contentWidth, 12f), CompactShellText(footer, 26), _captionStyle);
        }
        else
        {
            GUI.Label(new Rect(contentX, rect.y + 50f, contentWidth, rect.height - 72f), BuildDisplayBlock(body, 4, 46), _bodyStyle);
            GUI.Label(new Rect(contentX, rect.yMax - 20f, contentWidth, 16f), CompactShellText(footer, 40), _captionStyle);
        }

        return clickable && !disabled && GUI.Button(rect, GUIContent.none, GUIStyle.none);
    }

    private string BuildDungeonBattleFocusTitle(PrototypeBattleUiSurfaceData surface)
    {
        if (surface != null && surface.TargetSelection != null && surface.TargetSelection.IsActive)
        {
            return "Target Focus";
        }

        string actionLabel = ChooseFirstMeaningfulDungeonText(
            surface != null && surface.CommandSurface != null ? surface.CommandSurface.SelectedActionLabel : string.Empty,
            surface != null && surface.ActionContext != null ? surface.ActionContext.SelectedActionLabel : string.Empty);
        return HasMeaningfulValue(actionLabel) ? SafeShellText(actionLabel) : "Command Focus";
    }

    private string BuildDungeonBattleFocusSummary(PrototypeBattleUiSurfaceData surface)
    {
        if (surface != null && surface.TargetSelection != null && surface.TargetSelection.IsActive)
        {
            return CompactShellText(
                ChooseFirstMeaningfulDungeonText(
                    surface.TargetSelection.TargetLabel + " | " + surface.TargetSelection.TargetIntentLabel,
                    surface.TargetSelection.Title,
                    surface.TargetSelection.ReachabilitySummaryText),
                72);
        }

        return CompactShellText(
            ChooseFirstMeaningfulDungeonText(
                surface != null && surface.CommandSurface != null ? surface.CommandSurface.ContextualPanelSummaryText : string.Empty,
                surface != null && surface.MessageSurface != null ? surface.MessageSurface.Prompt : string.Empty,
                "Select an action."),
            88);
    }

    private string BuildDungeonBattleFocusBody(PrototypeBattleUiSurfaceData surface, PrototypeBattleUiCommandDetailData[] contextualDetails)
    {
        if (surface != null && surface.TargetSelection != null && surface.TargetSelection.IsActive)
        {
            PrototypeBattleUiTargetSelectionData target = surface.TargetSelection;
            return BuildDungeonLines(
                CompactShellText("Target: " + SafeShellText(target.TargetLabel), 84),
                CompactShellText("Intent: " + ChooseFirstMeaningfulDungeonText(target.TargetIntentLabel, target.TargetStateText, "Unknown"), 84),
                CompactShellText("Rule: " + ChooseFirstMeaningfulDungeonText(target.TargetRuleText, target.ReachabilitySummaryText, target.ThreatSummaryText), 84));
        }

        PrototypeBattleUiCommandDetailData detail = ResolveDungeonBattleFocusDetail(surface != null ? surface.CommandSurface : null, contextualDetails);
        if (detail == null)
        {
            return "Select a command.";
        }

        return BuildDungeonLines(
            CompactShellText(ChooseFirstMeaningfulDungeonText(detail.Description, detail.EffectText, "Ready."), 84),
            CompactShellText("Target: " + ChooseFirstMeaningfulDungeonText(detail.TargetText, "Self"), 84),
            CompactShellText(ChooseFirstMeaningfulDungeonText(
                FormatDungeonBattleFocusLine("Effect", detail.EffectText),
                FormatDungeonBattleFocusLine("Cost", detail.CostText),
                string.Empty), 84));
    }

    private PrototypeBattleUiCommandDetailData ResolveDungeonBattleFocusDetail(PrototypeBattleUiCommandSurfaceData commandSurface, PrototypeBattleUiCommandDetailData[] contextualDetails)
    {
        PrototypeBattleUiCommandDetailData[] details = contextualDetails ?? System.Array.Empty<PrototypeBattleUiCommandDetailData>();
        string selectedKey = commandSurface != null ? commandSurface.SelectedActionKey : string.Empty;
        for (int i = 0; i < details.Length; i++)
        {
            PrototypeBattleUiCommandDetailData detail = details[i];
            if (detail != null && (detail.IsSelected || detail.Key == selectedKey))
            {
                return detail;
            }
        }

        return details.Length > 0 ? details[0] : null;
    }

    private string FormatDungeonBattleFocusLine(string label, string value)
    {
        return HasMeaningfulValue(value) ? label + ": " + value : string.Empty;
    }

    private void DrawDungeonBattlePartyToken(Rect rect, PrototypeBattleUiPartyMemberData member, Color accentColor, out bool hovered)
    {
        PrototypeBattleUiPartyMemberData safeMember = member ?? new PrototypeBattleUiPartyMemberData();
        hovered = rect.Contains(Event.current.mousePosition);
        bool emphasized = hovered || safeMember.IsActive || safeMember.IsTargeted || safeMember.IsKnockedOut;
        Rect bodyRect = new Rect(rect.x + 18f, rect.y + 12f, rect.width - 56f, rect.height - 78f);
        Rect shadowRect = new Rect(bodyRect.x + 10f, bodyRect.yMax - 8f, bodyRect.width + 16f, 12f);
        if (emphasized)
        {
            DrawRect(new Rect(bodyRect.x - 10f, bodyRect.y - 10f, bodyRect.width + 20f, bodyRect.height + 20f), new Color(accentColor.r, accentColor.g, accentColor.b, 0.14f));
        }
        DrawRect(shadowRect, new Color(0f, 0f, 0f, 0.28f));
        DrawPanel(bodyRect, new Color(accentColor.r * 0.72f, accentColor.g * 0.72f, accentColor.b * 0.72f, 0.94f), new Color(0.14f, 0.20f, 0.26f, 0.92f));
        DrawRect(new Rect(bodyRect.xMax - 9f, bodyRect.y + 12f, 9f, bodyRect.height - 24f), new Color(accentColor.r, accentColor.g, accentColor.b, 0.78f));
        GUIStyle glyphStyle = new GUIStyle(_heroSubtitleStyle);
        glyphStyle.fontSize = Mathf.Clamp(Mathf.RoundToInt(bodyRect.height * 0.46f), 34, 56);
        glyphStyle.alignment = TextAnchor.MiddleCenter;
        GUI.Label(new Rect(bodyRect.x, bodyRect.y + 8f, bodyRect.width, bodyRect.height - 16f), BuildDungeonBattleGlyph(safeMember.DisplayName), glyphStyle);
        Rect plateRect = new Rect(rect.x + 10f, rect.yMax - 58f, rect.width - 20f, 50f);
        DrawPanel(plateRect, new Color(0.14f, 0.18f, 0.24f, 0.94f), new Color(0.06f, 0.08f, 0.12f, 0.94f));

        GUIStyle titleStyle = new GUIStyle(_bodyStyle);
        titleStyle.fontSize = 22;
        titleStyle.fontStyle = FontStyle.Bold;
        titleStyle.wordWrap = false;
        titleStyle.clipping = TextClipping.Clip;
        GUIStyle metaStyle = new GUIStyle(_captionStyle);
        metaStyle.fontSize = 18;
        metaStyle.wordWrap = false;
        metaStyle.clipping = TextClipping.Clip;
        float contentX = plateRect.x + 12f;
        float contentWidth = plateRect.width - 24f;
        GUI.Label(new Rect(contentX, plateRect.y + 6f, contentWidth - 52f, 24f), CompactShellText(safeMember.DisplayName, 12), titleStyle);
        string laneTag = BuildDungeonBattleLaneTag(safeMember.LaneLabel);
        string stateTag = BuildDungeonBattlePartyStateTag(safeMember);
        if (HasMeaningfulValue(laneTag))
        {
            float laneWidth = Mathf.Clamp(_badgeStyle.CalcSize(new GUIContent(laneTag)).x + 18f, 42f, 62f);
            DrawPill(new Rect(rect.x + 10f, rect.y + 10f, laneWidth, 22f), laneTag, new Color(0.18f, 0.26f, 0.34f, 0.96f), Color.white);
        }

        if (HasMeaningfulValue(stateTag))
        {
            float stateWidth = Mathf.Clamp(_badgeStyle.CalcSize(new GUIContent(stateTag)).x + 18f, 44f, 72f);
            DrawPill(new Rect(rect.xMax - stateWidth - 10f, rect.y + 10f, stateWidth, 22f), stateTag, new Color(accentColor.r, accentColor.g, accentColor.b, 0.96f), Color.white);
        }

        DrawDungeonBattleMeterBar(
            new Rect(contentX, plateRect.yMax - 18f, contentWidth, 14f),
            safeMember.MaxHp > 0 ? (float)safeMember.CurrentHp / safeMember.MaxHp : 0f,
            accentColor,
            string.Empty);
    }

    private bool DrawDungeonBattleEnemyToken(Rect rect, PrototypeBattleUiEnemyData enemy, bool canTarget, out bool hovered)
    {
        PrototypeBattleUiEnemyData safeEnemy = enemy ?? new PrototypeBattleUiEnemyData();
        hovered = rect.Contains(Event.current.mousePosition);
        Color accentColor = safeEnemy.IsActing
            ? new Color(0.78f, 0.58f, 0.24f, 1f)
            : safeEnemy.IsElite
                ? new Color(0.80f, 0.44f, 0.28f, 1f)
                : new Color(0.54f, 0.30f, 0.28f, 1f);
        Rect bodyRect = new Rect(rect.x + 20f, rect.y + 12f, rect.width - 56f, rect.height - 78f);
        Rect shadowRect = new Rect(bodyRect.x - 8f, bodyRect.yMax - 8f, bodyRect.width + 18f, 12f);
        if (safeEnemy.IsSelected || safeEnemy.IsActing || (hovered && canTarget))
        {
            DrawRect(new Rect(bodyRect.x - 10f, bodyRect.y - 10f, bodyRect.width + 20f, bodyRect.height + 20f), new Color(accentColor.r, accentColor.g, accentColor.b, 0.14f));
        }
        DrawRect(shadowRect, new Color(0f, 0f, 0f, 0.28f));
        DrawPanel(bodyRect, new Color(accentColor.r * 0.72f, accentColor.g * 0.60f, accentColor.b * 0.60f, 0.94f), new Color(0.18f, 0.12f, 0.12f, 0.92f));
        DrawRect(new Rect(bodyRect.x, bodyRect.y + 12f, 9f, bodyRect.height - 24f), new Color(accentColor.r, accentColor.g, accentColor.b, 0.78f));
        GUIStyle glyphStyle = new GUIStyle(_heroSubtitleStyle);
        glyphStyle.fontSize = Mathf.Clamp(Mathf.RoundToInt(bodyRect.height * 0.46f), 34, 56);
        glyphStyle.alignment = TextAnchor.MiddleCenter;
        GUI.Label(new Rect(bodyRect.x, bodyRect.y + 8f, bodyRect.width, bodyRect.height - 16f), BuildDungeonBattleGlyph(safeEnemy.DisplayName), glyphStyle);
        Rect plateRect = new Rect(rect.x + 10f, rect.yMax - 58f, rect.width - 20f, 50f);
        DrawPanel(plateRect, new Color(0.18f, 0.14f, 0.16f, 0.94f), new Color(0.08f, 0.06f, 0.10f, 0.94f));
        if (safeEnemy.IsSelected || (hovered && canTarget))
        {
            DrawDungeonBattleReticle(bodyRect, accentColor);
        }

        GUIStyle titleStyle = new GUIStyle(_bodyStyle);
        titleStyle.fontSize = 22;
        titleStyle.fontStyle = FontStyle.Bold;
        titleStyle.wordWrap = false;
        titleStyle.clipping = TextClipping.Clip;
        GUIStyle metaStyle = new GUIStyle(_captionStyle);
        metaStyle.fontSize = 18;
        metaStyle.wordWrap = false;
        metaStyle.clipping = TextClipping.Clip;
        float contentX = plateRect.x + 12f;
        float contentWidth = plateRect.width - 24f;
        GUI.Label(new Rect(contentX, plateRect.y + 6f, contentWidth - 58f, 24f), CompactShellText(safeEnemy.DisplayName, 12), titleStyle);
        string intentTag = BuildDungeonBattleIntentTag(safeEnemy.IntentLabel);
        float intentWidth = Mathf.Clamp(_badgeStyle.CalcSize(new GUIContent(intentTag)).x + 18f, 44f, 74f);
        DrawPill(new Rect(rect.xMax - intentWidth - 10f, rect.y + 10f, intentWidth, 22f), intentTag, new Color(accentColor.r, accentColor.g, accentColor.b, 0.96f), Color.white);
        string laneTag = BuildDungeonBattleLaneTag(safeEnemy.LaneLabel);
        if (HasMeaningfulValue(laneTag))
        {
            float laneWidth = Mathf.Clamp(_badgeStyle.CalcSize(new GUIContent(laneTag)).x + 18f, 42f, 62f);
            DrawPill(new Rect(rect.x + 10f, rect.y + 10f, laneWidth, 22f), laneTag, new Color(0.18f, 0.24f, 0.30f, 0.96f), Color.white);
        }

        DrawDungeonBattleMeterBar(
            new Rect(contentX, plateRect.yMax - 18f, contentWidth, 14f),
            safeEnemy.MaxHp > 0 ? (float)safeEnemy.CurrentHp / safeEnemy.MaxHp : 0f,
            accentColor,
            string.Empty);
        return canTarget && !safeEnemy.IsDefeated && GUI.Button(rect, GUIContent.none, GUIStyle.none);
    }

    private void DrawDungeonBattleMeterBar(Rect rect, float fillRatio, Color fillColor, string label)
    {
        float ratio = Mathf.Clamp01(fillRatio);
        DrawRect(rect, new Color(0.04f, 0.06f, 0.09f, 0.96f));
        float innerWidth = Mathf.Max(0f, (rect.width - 2f) * ratio);
        if (innerWidth > 0f)
        {
            DrawRect(new Rect(rect.x + 1f, rect.y + 1f, innerWidth, Mathf.Max(0f, rect.height - 2f)), fillColor);
        }

        if (!HasMeaningfulValue(label))
        {
            return;
        }

        GUIStyle meterLabelStyle = new GUIStyle(_captionStyle);
        meterLabelStyle.alignment = TextAnchor.MiddleCenter;
        meterLabelStyle.fontSize = Mathf.Clamp(Mathf.RoundToInt(rect.height * 1.1f), 18, 20);
        meterLabelStyle.wordWrap = false;
        meterLabelStyle.clipping = TextClipping.Clip;
        GUI.Label(new Rect(rect.x + 4f, rect.y - 1f, rect.width - 8f, rect.height + 2f), label, meterLabelStyle);
    }

    private void DrawDungeonBattleReticle(Rect rect, Color color)
    {
        float size = 18f;
        float thickness = 3f;
        DrawRect(new Rect(rect.x - 2f, rect.y - 2f, size, thickness), color);
        DrawRect(new Rect(rect.x - 2f, rect.y - 2f, thickness, size), color);
        DrawRect(new Rect(rect.xMax - size + 2f, rect.y - 2f, size, thickness), color);
        DrawRect(new Rect(rect.xMax - thickness + 2f, rect.y - 2f, thickness, size), color);
        DrawRect(new Rect(rect.x - 2f, rect.yMax - thickness + 2f, size, thickness), color);
        DrawRect(new Rect(rect.x - 2f, rect.yMax - size + 2f, thickness, size), color);
        DrawRect(new Rect(rect.xMax - size + 2f, rect.yMax - thickness + 2f, size, thickness), color);
        DrawRect(new Rect(rect.xMax - thickness + 2f, rect.yMax - size + 2f, thickness, size), color);
    }

    private string BuildDungeonBattleGlyph(string displayName)
    {
        string safeName = SafeShellText(displayName);
        return HasMeaningfulValue(safeName) ? safeName.Substring(0, 1).ToUpperInvariant() : "?";
    }

    private string BuildDungeonBattlePartyFooter(PrototypeBattleUiPartyMemberData member)
    {
        if (member == null)
        {
            return "Stand by";
        }

        if (member.IsKnockedOut || member.CurrentHp <= 0)
        {
            return "Down";
        }

        if (member.IsActive)
        {
            return "Current actor";
        }

        if (member.IsTargeted)
        {
            return "Under pressure";
        }

        return "Stand by";
    }

    private string BuildDungeonBattlePartyStatusFooter(PartyStatusSurfaceData member)
    {
        if (member == null)
        {
            return "Stable";
        }

        if (member.IsKnockedOut || member.CurrentHp <= 0)
        {
            return "Down";
        }

        if (member.IsActive)
        {
            return "Acting";
        }

        if (member.IsTargeted)
        {
            return "Threat";
        }

        return ChooseFirstMeaningfulDungeonText(member.StatusText, "Stable");
    }

    private string BuildDungeonBattleEnemyFooter(PrototypeBattleUiEnemyData enemy, bool canTarget)
    {
        if (enemy == null)
        {
            return "Pending";
        }

        if (enemy.IsDefeated)
        {
            return "Defeated";
        }

        if (canTarget)
        {
            return enemy.IsSelected ? "Locked target" : "Click to target";
        }

        if (enemy.IsActing)
        {
            return "Acting now";
        }

        if (enemy.IsElite)
        {
            return "Elite pressure";
        }

        return ChooseFirstMeaningfulDungeonText(enemy.StateLabel, enemy.ThreatLaneText, "Pending");
    }

    private string BuildDungeonBattleLaneTag(string laneLabel)
    {
        if (!HasMeaningfulValue(laneLabel))
        {
            return string.Empty;
        }

        string normalized = SafeShellText(laneLabel).ToLowerInvariant();
        if (normalized.Contains("front") || normalized.Contains("top"))
        {
            return "FR";
        }

        if (normalized.Contains("middle") || normalized.Contains("mid"))
        {
            return "MID";
        }

        if (normalized.Contains("back") || normalized.Contains("bottom") || normalized.Contains("bot"))
        {
            return "BACK";
        }

        if (normalized.Contains("entire") || normalized.Contains("any"))
        {
            return "ALL";
        }

        return CompactShellText(SafeShellText(laneLabel), 4).ToUpperInvariant();
    }

    private string BuildDungeonBattleIntentTag(string intentLabel)
    {
        if (!HasMeaningfulValue(intentLabel))
        {
            return "?";
        }

        string intent = SafeShellText(intentLabel).ToLowerInvariant();
        if (intent.Contains("heal") || intent.Contains("support"))
        {
            return "SUP";
        }

        if (intent.Contains("buff") || intent.Contains("guard"))
        {
            return "BUF";
        }

        if (intent.Contains("debuff") || intent.Contains("curse"))
        {
            return "HEX";
        }

        if (intent.Contains("pressure") || intent.Contains("strike") || intent.Contains("attack"))
        {
            return "ATK";
        }

        if (intent.Contains("random") || intent.Contains("unstable"))
        {
            return "RND";
        }

        return CompactShellText(SafeShellText(intentLabel), 4).ToUpperInvariant();
    }

    private string BuildDungeonBattlePartyStateTag(PrototypeBattleUiPartyMemberData member)
    {
        if (member == null)
        {
            return string.Empty;
        }

        if (member.IsKnockedOut || member.CurrentHp <= 0)
        {
            return "DOWN";
        }

        if (member.IsActive)
        {
            return "ACT";
        }

        if (member.IsTargeted)
        {
            return "HOT";
        }

        return string.Empty;
    }

    private string BuildDungeonBattleEnemyMetaTag(PrototypeBattleUiEnemyData enemy, bool canTarget)
    {
        if (enemy == null)
        {
            return string.Empty;
        }

        if (enemy.IsDefeated)
        {
            return "Down";
        }

        if (enemy.IsElite)
        {
            return "Elite";
        }

        if (enemy.IsActing)
        {
            return "Acting";
        }

        return canTarget ? "Targetable" : string.Empty;
    }

    private string BuildDungeonBattleJoinedTagLine(string primary, string secondary)
    {
        bool hasPrimary = HasMeaningfulValue(primary);
        bool hasSecondary = HasMeaningfulValue(secondary);
        if (hasPrimary && hasSecondary)
        {
            return primary + " | " + secondary;
        }

        if (hasPrimary)
        {
            return primary;
        }

        if (hasSecondary)
        {
            return secondary;
        }

        return string.Empty;
    }

    private bool CanTriggerDungeonBattleAction(string actionKey, bool isAvailable)
    {
        return isAvailable &&
               (actionKey == "attack" ||
                actionKey == "skill" ||
                actionKey == "move" ||
                actionKey == "move_front" ||
                actionKey == "move_middle" ||
                actionKey == "move_back" ||
                actionKey == "end_turn" ||
                actionKey == "retreat");
    }

    private bool CanHoverDungeonBattleAction(string actionKey, bool isAvailable)
    {
        return isAvailable &&
               (actionKey == "attack" ||
                actionKey == "skill" ||
                actionKey == "move" ||
                actionKey == "move_front" ||
                actionKey == "move_middle" ||
                actionKey == "move_back" ||
                actionKey == "end_turn" ||
                actionKey == "retreat");
    }

    private bool IsDungeonBattleFrontLane(string laneKey, string laneLabel)
    {
        string lane = (laneKey + "|" + laneLabel).ToLowerInvariant();
        return lane.Contains("front") || lane.Contains("vanguard") || lane.Contains("1");
    }

    private bool IsDungeonBattleFrontPartyMember(PrototypeBattleUiPartyMemberData member)
    {
        if (member == null)
        {
            return false;
        }

        return ResolveDungeonBattleFrontline(
            member.PositionRuleText,
            member.LaneKey,
            member.LaneLabel);
    }

    private bool IsDungeonBattleFrontEnemy(PrototypeBattleUiEnemyData enemy)
    {
        if (enemy == null)
        {
            return false;
        }

        return ResolveDungeonBattleFrontline(
            enemy.PositionRuleText,
            enemy.LaneKey,
            enemy.LaneLabel);
    }

    private int ResolveDungeonBattleLaneIndex(string laneKey, string laneLabel)
    {
        string lane = (laneKey + "|" + laneLabel).ToLowerInvariant();
        if (lane.Contains("front") || lane.Contains("top"))
        {
            return 0;
        }

        if (lane.Contains("back") || lane.Contains("bottom") || lane.Contains("bot"))
        {
            return 2;
        }

        return 1;
    }

    private void DrawDungeonBattleRowGuides(Rect rect, bool allySide)
    {
        float segmentWidth = Mathf.Clamp(rect.width * 0.24f, 78f, 136f);
        float lineThickness = 5f;
        float baseY = rect.yMax - 26f;
        float backCenterX = rect.x + (rect.width * ResolveDungeonBattleRowDepthAnchor(2, allySide));
        float middleCenterX = rect.x + (rect.width * ResolveDungeonBattleRowDepthAnchor(1, allySide));
        float frontCenterX = rect.x + (rect.width * ResolveDungeonBattleRowDepthAnchor(0, allySide));
        float rowMinX = Mathf.Min(backCenterX, Mathf.Min(middleCenterX, frontCenterX)) - (segmentWidth * 0.5f);
        float rowMaxX = Mathf.Max(backCenterX, Mathf.Max(middleCenterX, frontCenterX)) + (segmentWidth * 0.5f);

        DrawRect(new Rect(rowMinX, baseY - 12f, rowMaxX - rowMinX, 4f), new Color(0.20f, 0.76f, 0.34f, 0.92f));
        DrawRect(new Rect(backCenterX - (segmentWidth * 0.5f), baseY + 8f, segmentWidth, lineThickness), new Color(0.62f, 0.34f, 0.86f, 0.96f));
        DrawRect(new Rect(middleCenterX - (segmentWidth * 0.5f), baseY + 8f, segmentWidth, lineThickness), new Color(0.24f, 0.70f, 0.98f, 0.96f));
        DrawRect(new Rect(frontCenterX - (segmentWidth * 0.5f), baseY + 8f, segmentWidth, lineThickness), new Color(0.96f, 0.28f, 0.28f, 0.96f));
    }

    private float ResolveDungeonBattleRowDepthAnchor(int laneIndex, bool allySide)
    {
        if (allySide)
        {
            switch (laneIndex)
            {
                case 0:
                    return 0.82f;
                case 2:
                    return 0.28f;
                default:
                    return 0.55f;
            }
        }

        switch (laneIndex)
        {
            case 0:
                return 0.18f;
            case 2:
                return 0.72f;
            default:
                return 0.45f;
        }
    }

    private float ResolveDungeonBattleRowVerticalAnchor(int laneIndex, bool allySide)
    {
        if (allySide)
        {
            switch (laneIndex)
            {
                case 0:
                    return 0.54f;
                case 2:
                    return 0.68f;
                default:
                    return 0.61f;
            }
        }

        switch (laneIndex)
        {
            case 0:
                return 0.46f;
            case 2:
                return 0.32f;
            default:
                return 0.39f;
        }
    }

    private float ResolveDungeonBattleLaneAnchor(int laneIndex)
    {
        switch (laneIndex)
        {
            case 0:
                return 0.28f;
            case 2:
                return 0.76f;
            default:
                return 0.52f;
        }
    }

    private float ResolveDungeonBattleFormationYOffset(int duplicateIndex)
    {
        switch (duplicateIndex)
        {
            case 1:
                return 36f;
            case 2:
                return -34f;
            case 3:
                return 64f;
            default:
                return 0f;
        }
    }

    private float ResolveDungeonBattleFormationXOffset(int duplicateIndex, bool allySide)
    {
        float direction = allySide ? -1f : 1f;
        switch (duplicateIndex)
        {
            case 1:
                return 24f * direction;
            case 2:
                return -18f * direction;
            case 3:
                return 42f * direction;
            default:
                return 0f;
        }
    }

    private bool ResolveDungeonBattleFrontline(string positionRuleText, string laneKey, string laneLabel)
    {
        string combined = (positionRuleText + "|" + laneKey + "|" + laneLabel).ToLowerInvariant();
        if (combined.Contains("back"))
        {
            return false;
        }

        if (combined.Contains("front") || combined.Contains("vanguard"))
        {
            return true;
        }

        if (combined.Contains("top") || combined.Contains("mid"))
        {
            return true;
        }

        if (combined.Contains("bottom") || combined.Contains("rear"))
        {
            return false;
        }

        return IsDungeonBattleFrontLane(laneKey, laneLabel);
    }

    private Color ResolveDungeonBattlePartyDangerColor(PrototypeBattleUiPartyMemberData member)
    {
        if (member == null || member.IsKnockedOut || member.CurrentHp <= 0)
        {
            return new Color(0.58f, 0.26f, 0.22f, 1f);
        }

        float hpRatio = member.MaxHp > 0 ? (float)member.CurrentHp / member.MaxHp : 0f;
        if (hpRatio <= 0.35f)
        {
            return new Color(0.72f, 0.30f, 0.24f, 1f);
        }

        if (member.IsTargeted || hpRatio <= 0.60f)
        {
            return new Color(0.72f, 0.52f, 0.22f, 1f);
        }

        return member.IsActive ? new Color(0.42f, 0.62f, 0.30f, 1f) : new Color(0.30f, 0.44f, 0.58f, 1f);
    }

    private Color ResolveDungeonBattleDangerColor(string dangerStateKey)
    {
        if (dangerStateKey == "critical")
        {
            return new Color(0.72f, 0.30f, 0.24f, 1f);
        }

        if (dangerStateKey == "warning")
        {
            return new Color(0.72f, 0.52f, 0.22f, 1f);
        }

        if (dangerStateKey == "focus")
        {
            return new Color(0.42f, 0.62f, 0.30f, 1f);
        }

        return new Color(0.30f, 0.44f, 0.58f, 1f);
    }
    private void DrawDungeonResultShell(Rect screenRect, PrototypeDungeonRunShellSurfaceData shellSurface)
    {
        PrototypeDungeonRunResultContext resultContext = shellSurface != null ? shellSurface.ResultContext : new PrototypeDungeonRunResultContext();
        CityWriteback returnAftermath = resultContext.WorldReturnAftermathPreview ?? new CityWriteback();
        OutcomeReadback outcomeReadback = resultContext.WorldOutcomeReadbackPreview ?? new OutcomeReadback();
        Rect layoutRect = CenterRect(Mathf.Min(Screen.width - 60f, 1360f), Mathf.Min(Screen.height - 60f, 760f));
        Rect headerRect = new Rect(layoutRect.x, layoutRect.y, layoutRect.width, DungeonHeaderHeight);
        Rect bodyRect = new Rect(layoutRect.x, headerRect.yMax + DungeonShellGap, layoutRect.width, layoutRect.height - DungeonHeaderHeight - DungeonShellGap);
        DrawDungeonHeaderBar(
            headerRect,
            BuildDungeonResultTitle(resultContext),
            BuildDungeonResultSubtitle(resultContext, outcomeReadback),
            BuildDungeonHeaderChips(
                BuildDungeonChip("Dungeon", resultContext.DungeonLabel),
                BuildDungeonChip("Route", resultContext.RouteLabel),
                BuildDungeonChip("Turns", resultContext.TurnsTakenText),
                BuildDungeonChip("Party", resultContext.PartyConditionText),
                BuildDungeonChip("Loot", resultContext.LootGainedText)));

        float columnWidth = (bodyRect.width - (DungeonShellGap * 2f)) / 3f;
        Rect leftRect = new Rect(bodyRect.x, bodyRect.y, columnWidth, bodyRect.height);
        Rect centerRect = new Rect(leftRect.xMax + DungeonShellGap, bodyRect.y, columnWidth, bodyRect.height);
        Rect rightRect = new Rect(centerRect.xMax + DungeonShellGap, bodyRect.y, columnWidth, bodyRect.height);

        DrawDungeonInfoCard(
            leftRect,
            "Outcome Summary",
            BuildDungeonResultOutcomeBody(resultContext, returnAftermath));

        DrawDungeonInfoCard(
            centerRect,
            "Rewards & Build Change",
            BuildDungeonResultRewardBody(resultContext));

        DrawDungeonInfoCard(
            rightRect,
            "World Handoff",
            BuildDungeonResultWorldBody(resultContext, returnAftermath, outcomeReadback));

        Rect footerRect = new Rect(layoutRect.x, layoutRect.yMax - DungeonFooterHeight, layoutRect.width, DungeonFooterHeight);
        DrawPanel(footerRect, new Color(0.12f, 0.18f, 0.24f, 0.96f), new Color(0.08f, 0.11f, 0.16f, 0.92f));
        string footerText = BuildDungeonResultFooterText(resultContext, outcomeReadback);
        GUI.Label(new Rect(footerRect.x + 16f, footerRect.y + 10f, footerRect.width - 32f, 18f), footerText, _captionStyle);
    }

    private string BuildPlannerOptionSubtitle(PrototypeDungeonPanelOption option)
    {
        if (option == null)
        {
            return "Route Read";
        }

        List<string> tags = new List<string>();
        tags.Add(option.OptionId == "safe" ? "Controlled opening" : option.OptionId == "risky" ? "Pressure-first opening" : "Route read");
        if (option.IsRecommended)
        {
            tags.Add("Recommended");
        }

        if (option.IsSelected)
        {
            tags.Add("Selected");
        }

        return tags.Count > 0 ? string.Join(" | ", tags.ToArray()) : "Route Read";
    }

    private string BuildDecisionOptionSubtitle(bool isPreElite, PrototypeDungeonPanelOption option)
    {
        if (option != null)
        {
            if (option.OptionId == "recover")
            {
                return isPreElite ? "Steadier elite entry" : "Take the blessing";
            }

            if (option.OptionId == "bonus" || option.OptionId == "loot")
            {
                return isPreElite ? "Bet on the kill" : "Cash out the shrine";
            }
        }

        return isPreElite ? "Final preparation" : "Shrine choice";
    }

    private string BuildDungeonResultTitle(PrototypeDungeonRunResultContext resultContext)
    {
        string outcomeKey = resultContext != null ? resultContext.RunOutcomeKey : string.Empty;
        if (outcomeKey == PrototypeBattleOutcomeKeys.RunClear)
        {
            return "Expedition Secured";
        }

        if (outcomeKey == PrototypeBattleOutcomeKeys.RunRetreat)
        {
            return "Expedition Returned Early";
        }

        if (outcomeKey == PrototypeBattleOutcomeKeys.RunDefeat)
        {
            return "Expedition Broken";
        }

        string stateText = resultContext != null ? SafeShellText(resultContext.ResultStateText) : "None";
        return HasMeaningfulValue(stateText) && stateText != "None"
            ? "Run Settlement | " + stateText
            : "Run Settlement";
    }

    private string BuildDungeonResultSubtitle(PrototypeDungeonRunResultContext resultContext, OutcomeReadback outcomeReadback)
    {
        return BuildDungeonLines(
            BuildDungeonResultHeaderLeadLine(resultContext),
            BuildDungeonResultHeaderAftermathLine(resultContext, outcomeReadback));
    }

    private string BuildDungeonResultHeaderLeadLine(PrototypeDungeonRunResultContext resultContext)
    {
        List<string> parts = new List<string>();
        if (resultContext != null && HasMeaningfulValue(resultContext.DungeonLabel))
        {
            parts.Add(SafeShellText(resultContext.DungeonLabel));
        }

        if (resultContext != null && HasMeaningfulValue(resultContext.RouteLabel))
        {
            parts.Add("via " + SafeShellText(resultContext.RouteLabel));
        }

        if (resultContext != null && HasMeaningfulValue(resultContext.TurnsTakenText) && resultContext.TurnsTakenText != "0")
        {
            parts.Add(SafeShellText(resultContext.TurnsTakenText) + " turns");
        }

        if (resultContext != null && HasMeaningfulValue(resultContext.LootGainedText))
        {
            parts.Add("Loot " + SafeShellText(resultContext.LootGainedText));
        }

        return parts.Count > 0
            ? string.Join(" | ", parts.ToArray())
            : SafeShellText(resultContext != null ? resultContext.ResultSummaryText : "None");
    }

    private string BuildDungeonResultHeaderAftermathLine(PrototypeDungeonRunResultContext resultContext, OutcomeReadback outcomeReadback)
    {
        return ChooseFirstMeaningfulDungeonText(
            BuildDungeonLabeledLine("Latest Return", outcomeReadback != null ? outcomeReadback.LatestReturnAftermathText : "None"),
            BuildDungeonLabeledLine("World Impact", resultContext != null ? resultContext.WorldWritebackResultSummaryText : "None"),
            SafeShellText(resultContext != null ? resultContext.ResultSummaryText : "None"));
    }

    private string BuildDungeonResultOutcomeBody(PrototypeDungeonRunResultContext resultContext, CityWriteback returnAftermath)
    {
        return BuildDungeonSections(
            BuildDungeonSection(
                "Run Outcome",
                SafeShellText(resultContext != null ? resultContext.ResultSummaryText : "None"),
                BuildDungeonLabeledLine("Route Run", resultContext != null ? resultContext.SelectedRouteSummaryText : "None"),
                BuildDungeonLabeledLine("Room Path", resultContext != null ? resultContext.RoomPathSummaryText : "None"),
                BuildDungeonLabeledLine("Encounters Closed", resultContext != null ? resultContext.ClearedEncountersText : "None"),
                BuildDungeonLabeledLine("Extraction", resultContext != null ? resultContext.ExtractionSummaryText : "None"),
                BuildDungeonLabeledLine("Time Cost", resultContext != null ? resultContext.TimeCostSummaryText : "None")),
            BuildDungeonSection(
                "Party Outcome",
                BuildDungeonLabeledLine("Survivors", resultContext != null ? resultContext.SurvivingMembersSummaryText : "None"),
                BuildDungeonLabeledLine("Party Condition", resultContext != null ? resultContext.PartyConditionText : "None"),
                BuildDungeonLabeledLine("Party End HP", resultContext != null ? resultContext.PartyHpSummaryText : "None"),
                BuildDungeonLabeledLine("Party Aftermath", returnAftermath != null ? returnAftermath.PartyOutcomeSummaryText : "None"),
                BuildDungeonLabeledLine("Key Encounter", resultContext != null ? resultContext.KeyEncounterSummaryText : "None")));
    }

    private string BuildDungeonResultRewardBody(PrototypeDungeonRunResultContext resultContext)
    {
        return BuildDungeonSections(
            BuildDungeonSection(
                "Reward Outcome",
                BuildDungeonLabeledLine("Reward Total", resultContext != null ? resultContext.LootGainedText : "None"),
                BuildDungeonRewardMixLine(resultContext),
                BuildDungeonEliteRewardLine(resultContext),
                BuildDungeonEliteBonusLine(resultContext),
                BuildDungeonLabeledLine("Resource Delta", resultContext != null ? resultContext.ResourceDeltaSummaryText : "None")),
            BuildDungeonSection(
                "Carryover / Build Change",
                BuildDungeonDecisionPathLine(resultContext),
                BuildDungeonLabeledLine("Choice Result", resultContext != null ? resultContext.ChoiceOutcomeSummaryText : "None"),
                BuildDungeonLabeledLine("Gear Reward", resultContext != null ? resultContext.GearRewardCandidateSummaryText : "None"),
                BuildDungeonLabeledLine("Equip Swap", resultContext != null ? resultContext.EquipSwapChoiceSummaryText : "None"),
                BuildDungeonLabeledLine("Gear Continuity", resultContext != null ? resultContext.GearCarryContinuitySummaryText : "None"),
                BuildDungeonLabeledLine("Progression", BuildDungeonProgressionSummary(resultContext))));
    }

    private string BuildDungeonResultWorldBody(PrototypeDungeonRunResultContext resultContext, CityWriteback returnAftermath, OutcomeReadback outcomeReadback)
    {
        return BuildDungeonSections(
            BuildDungeonSection(
                "World Impact",
                BuildDungeonLabeledLine("Latest Return", outcomeReadback != null ? outcomeReadback.LatestReturnAftermathText : "None"),
                BuildDungeonLabeledLine("Outcome Readback", resultContext != null ? resultContext.WorldWritebackResultSummaryText : "None"),
                BuildDungeonLabeledLine("Loot Returned", resultContext != null ? resultContext.WorldWritebackLootSummaryText : "None"),
                BuildDungeonLabeledLine("Stock Reaction", returnAftermath != null ? returnAftermath.StockReactionSummaryText : "None"),
                BuildDungeonStateTransitionLine("Need Pressure", resultContext != null ? resultContext.NeedPressureBeforeText : "None", resultContext != null ? resultContext.NeedPressureAfterText : "None"),
                BuildDungeonStateTransitionLine("Dispatch Readiness", resultContext != null ? resultContext.DispatchReadinessBeforeText : "None", resultContext != null ? resultContext.DispatchReadinessAfterText : "None"),
                BuildDungeonLabeledLine("Recovery ETA", outcomeReadback != null ? outcomeReadback.RecoveryEtaText : "None"),
                BuildDungeonReturnChainLine(resultContext),
                BuildDungeonDungeonStateLine(outcomeReadback)),
            BuildDungeonSection(
                "Next Follow-Up",
                BuildDungeonLabeledLine("Corrective Follow-Up", outcomeReadback != null ? outcomeReadback.NextSuggestedActionText : "None"),
                BuildDungeonLabeledLine("Return Board Hint", outcomeReadback != null ? outcomeReadback.FollowUpHintText : "None")));
    }

    private string BuildDungeonResultFooterText(PrototypeDungeonRunResultContext resultContext, OutcomeReadback outcomeReadback)
    {
        string promptText = HasMeaningfulValue(outcomeReadback != null ? outcomeReadback.NextSuggestedActionText : "None")
            ? "Press [Enter] to return to World and line up the next follow-up."
            : "Press [Enter] to return to World and review the aftermath.";
        string nextText = BuildDungeonLabeledLine("Next", outcomeReadback != null ? outcomeReadback.NextSuggestedActionText : "None");
        return HasMeaningfulValue(nextText)
            ? promptText + "  |  " + nextText
            : promptText;
    }

    private string BuildDungeonSection(string heading, params string[] lines)
    {
        string body = BuildDungeonLines(lines);
        return HasMeaningfulValue(body)
            ? SafeShellText(heading) + "\n" + body
            : string.Empty;
    }

    private string BuildDungeonSections(params string[] sections)
    {
        if (sections == null || sections.Length == 0)
        {
            return "None";
        }

        List<string> filtered = new List<string>();
        for (int i = 0; i < sections.Length; i++)
        {
            if (HasMeaningfulValue(sections[i]))
            {
                filtered.Add(sections[i]);
            }
        }

        return filtered.Count > 0 ? string.Join("\n\n", filtered.ToArray()) : "None";
    }

    private string BuildDungeonLabeledLine(string label, string value)
    {
        return HasMeaningfulValue(value)
            ? label + ": " + SafeShellText(value)
            : string.Empty;
    }

    private string BuildDungeonRewardMixLine(PrototypeDungeonRunResultContext resultContext)
    {
        List<string> parts = new List<string>();
        if (resultContext != null && HasMeaningfulValue(resultContext.BattleLootText))
        {
            parts.Add("Battle " + SafeShellText(resultContext.BattleLootText));
        }

        if (resultContext != null && HasMeaningfulValue(resultContext.ChestLootText))
        {
            parts.Add("Chest " + SafeShellText(resultContext.ChestLootText));
        }

        if (resultContext != null && HasMeaningfulValue(resultContext.EventLootText))
        {
            parts.Add("Event " + SafeShellText(resultContext.EventLootText));
        }

        return parts.Count > 0
            ? "Reward Mix: " + string.Join(" | ", parts.ToArray())
            : string.Empty;
    }

    private string BuildDungeonEliteRewardLine(PrototypeDungeonRunResultContext resultContext)
    {
        if (resultContext == null)
        {
            return string.Empty;
        }

        bool hasIdentity = HasMeaningfulValue(resultContext.EliteRewardIdentityText);
        bool hasAmount = HasMeaningfulValue(resultContext.EliteRewardAmountText);
        if (hasIdentity && hasAmount)
        {
            return "Elite Reward: " + SafeShellText(resultContext.EliteRewardIdentityText) + " (" + SafeShellText(resultContext.EliteRewardAmountText) + ")";
        }

        if (hasIdentity)
        {
            return "Elite Reward: " + SafeShellText(resultContext.EliteRewardIdentityText);
        }

        if (hasAmount)
        {
            return "Elite Reward: " + SafeShellText(resultContext.EliteRewardAmountText);
        }

        return string.Empty;
    }

    private string BuildDungeonEliteBonusLine(PrototypeDungeonRunResultContext resultContext)
    {
        if (resultContext == null)
        {
            return string.Empty;
        }

        string amountText = resultContext.EliteBonusRewardAmountText;
        string earnedText = resultContext.EliteBonusRewardEarnedText;
        if (!HasMeaningfulValue(amountText) && !HasMeaningfulValue(earnedText))
        {
            return string.Empty;
        }

        if (HasMeaningfulValue(amountText) && SafeShellText(earnedText) == "Yes")
        {
            return "Elite Bonus: Secured " + SafeShellText(amountText);
        }

        if (HasMeaningfulValue(amountText) && SafeShellText(earnedText) == "No")
        {
            return "Elite Bonus: Missed " + SafeShellText(amountText);
        }

        if (HasMeaningfulValue(amountText))
        {
            return "Elite Bonus: " + SafeShellText(amountText);
        }

        return "Elite Bonus: " + SafeShellText(earnedText);
    }

    private string BuildDungeonDecisionPathLine(PrototypeDungeonRunResultContext resultContext)
    {
        if (resultContext == null)
        {
            return string.Empty;
        }

        List<string> parts = new List<string>();
        if (HasOptionalDungeonCopyValue(resultContext.EventChoiceText))
        {
            parts.Add("Event " + SafeShellText(resultContext.EventChoiceText));
        }

        if (HasOptionalDungeonCopyValue(resultContext.PreEliteChoiceText))
        {
            parts.Add("Prep " + SafeShellText(resultContext.PreEliteChoiceText));
        }

        return parts.Count > 0
            ? "Decision Path: " + string.Join(" | ", parts.ToArray())
            : string.Empty;
    }

    private string BuildDungeonProgressionSummary(PrototypeDungeonRunResultContext resultContext)
    {
        PrototypeRpgPartyMemberOutcomeSnapshot[] members =
            resultContext != null &&
            resultContext.CanonicalRunResult != null &&
            resultContext.CanonicalRunResult.PartyOutcome != null &&
            resultContext.CanonicalRunResult.PartyOutcome.Members != null
                ? resultContext.CanonicalRunResult.PartyOutcome.Members
                : System.Array.Empty<PrototypeRpgPartyMemberOutcomeSnapshot>();
        if (members.Length <= 0)
        {
            return string.Empty;
        }

        List<string> parts = new List<string>();
        for (int i = 0; i < members.Length && parts.Count < 2; i++)
        {
            PrototypeRpgPartyMemberOutcomeSnapshot member = members[i];
            if (member == null)
            {
                continue;
            }

            string detail = ChooseFirstOptionalDungeonText(
                member.AppliedProgressionSummaryText,
                member.NextRunPreviewSummaryText,
                member.CurrentRunSummaryText,
                member.GearContributionSummaryText);
            if (!HasOptionalDungeonCopyValue(detail))
            {
                continue;
            }

            string displayName = HasMeaningfulValue(member.DisplayName) ? SafeShellText(member.DisplayName) : "Party";
            parts.Add(displayName + ": " + CompactShellText(detail, 56));
        }

        return parts.Count > 0 ? string.Join(" | ", parts.ToArray()) : string.Empty;
    }

    private string BuildDungeonStateTransitionLine(string label, string beforeText, string afterText)
    {
        bool hasBefore = HasMeaningfulValue(beforeText);
        bool hasAfter = HasMeaningfulValue(afterText);
        if (hasBefore && hasAfter)
        {
            string safeBefore = SafeShellText(beforeText);
            string safeAfter = SafeShellText(afterText);
            return safeBefore == safeAfter
                ? label + ": " + safeAfter
                : label + ": " + safeBefore + " -> " + safeAfter;
        }

        if (hasAfter)
        {
            return label + ": " + SafeShellText(afterText);
        }

        if (hasBefore)
        {
            return label + ": " + SafeShellText(beforeText);
        }

        return string.Empty;
    }

    private string BuildDungeonReturnChainLine(PrototypeDungeonRunResultContext resultContext)
    {
        if (resultContext == null)
        {
            return string.Empty;
        }

        List<string> parts = new List<string>();
        if (HasMeaningfulValue(resultContext.WorldWritebackRouteSummaryText))
        {
            parts.Add(SafeShellText(resultContext.WorldWritebackRouteSummaryText));
        }

        if (HasMeaningfulValue(resultContext.WorldWritebackDungeonSummaryText))
        {
            parts.Add(SafeShellText(resultContext.WorldWritebackDungeonSummaryText));
        }

        return parts.Count > 0
            ? "Return Chain: " + string.Join(" | ", parts.ToArray())
            : string.Empty;
    }

    private string BuildDungeonDungeonStateLine(OutcomeReadback outcomeReadback)
    {
        if (outcomeReadback == null)
        {
            return string.Empty;
        }

        List<string> parts = new List<string>();
        if (HasMeaningfulValue(outcomeReadback.DungeonStatusText))
        {
            parts.Add(SafeShellText(outcomeReadback.DungeonStatusText));
        }

        if (HasMeaningfulValue(outcomeReadback.DungeonAvailabilityText))
        {
            parts.Add(SafeShellText(outcomeReadback.DungeonAvailabilityText));
        }

        return parts.Count > 0
            ? "Dungeon State: " + string.Join(" | ", parts.ToArray())
            : string.Empty;
    }

    private string ChooseFirstMeaningfulDungeonText(params string[] values)
    {
        if (values == null)
        {
            return string.Empty;
        }

        for (int i = 0; i < values.Length; i++)
        {
            if (HasMeaningfulValue(values[i]))
            {
                return values[i];
            }
        }

        return string.Empty;
    }

    private string ChooseFirstOptionalDungeonText(params string[] values)
    {
        if (values == null)
        {
            return string.Empty;
        }

        for (int i = 0; i < values.Length; i++)
        {
            if (HasOptionalDungeonCopyValue(values[i]))
            {
                return SafeShellText(values[i]);
            }
        }

        return string.Empty;
    }

    private bool HasOptionalDungeonCopyValue(string value)
    {
        if (!HasMeaningfulValue(value))
        {
            return false;
        }

        string text = SafeShellText(value).Trim();
        return text != "Pending" &&
               text != "No gear" &&
               text != "No bonus" &&
               text != "No applied progression." &&
               text != "No current-run summary." &&
               text != "No next-run preview.";
    }

    private void DrawDungeonHeaderBar(Rect rect, string title, string subtitle, string[] chips)
    {
        DrawPanel(rect, new Color(0.12f, 0.18f, 0.24f, 0.98f), new Color(0.08f, 0.11f, 0.16f, 0.94f));
        GUI.Label(new Rect(rect.x + 18f, rect.y + 10f, rect.width * 0.42f, 28f), SafeShellText(title), _heroSubtitleStyle);
        GUI.Label(
            new Rect(rect.x + 18f, rect.y + 38f, rect.width * 0.50f, rect.height - 42f),
            BuildDisplayBlock(subtitle, 2, 112),
            _captionStyle);

        float rightX = rect.xMax - 16f;
        if (chips != null)
        {
            for (int i = chips.Length - 1; i >= 0; i--)
            {
                if (!HasMeaningfulValue(chips[i]))
                {
                    continue;
                }

                Vector2 chipSize = _badgeStyle.CalcSize(new GUIContent(chips[i]));
                float chipWidth = Mathf.Clamp(chipSize.x + 20f, 96f, 188f);
                Rect chipRect = new Rect(rightX - chipWidth, rect.y + 16f, chipWidth, 28f);
                DrawPill(chipRect, chips[i], new Color(0.18f, 0.28f, 0.38f, 0.96f), new Color(0.94f, 0.96f, 0.98f, 1f));
                rightX = chipRect.x - 8f;
            }
        }
    }

    private void DrawDungeonInfoCard(Rect rect, string title, string body)
    {
        DrawPanel(rect, new Color(0.12f, 0.18f, 0.24f, 0.98f), new Color(0.08f, 0.11f, 0.16f, 0.94f));
        GUI.Label(new Rect(rect.x + 16f, rect.y + 14f, rect.width - 32f, 22f), SafeShellText(title), _panelTitleStyle);
        Rect bodyRect = new Rect(rect.x + 16f, rect.y + 42f, rect.width - 32f, rect.height - 58f);
        string scrollKey = "dungeon_info:" + SafeShellText(title) + ":" + Mathf.RoundToInt(rect.x) + ":" + Mathf.RoundToInt(rect.y);
        DrawScrollableTextBlock(bodyRect, scrollKey, body, _bodyStyle);
    }

    private bool DrawDungeonOptionCard(Rect rect, string title, string subtitle, string body, string footer, Color accentColor, string tagText, bool blocked, out bool hovered)
    {
        hovered = rect.Contains(Event.current.mousePosition);
        bool highlighted = hovered && !blocked;
        bool compact = rect.height < 136f;
        Color borderColor = blocked
            ? new Color(0.18f, 0.20f, 0.24f, 0.96f)
            : highlighted
                ? accentColor
                : new Color(0.12f, 0.18f, 0.24f, 0.98f);
        Color innerColor = blocked
            ? new Color(0.08f, 0.09f, 0.12f, 0.92f)
            : new Color(0.08f, 0.11f, 0.16f, 0.94f);
        DrawPanel(rect, borderColor, innerColor);

        if (HasMeaningfulValue(tagText))
        {
            Vector2 tagSize = _badgeStyle.CalcSize(new GUIContent(tagText));
            Rect tagRect = new Rect(rect.xMax - tagSize.x - 28f, rect.y + 10f, tagSize.x + 18f, 22f);
            DrawPill(tagRect, tagText, new Color(accentColor.r, accentColor.g, accentColor.b, 0.90f), Color.white);
        }

        Rect accentRect = new Rect(rect.x + 12f, rect.y + 12f, 6f, rect.height - 24f);
        DrawRect(accentRect, blocked ? new Color(0.28f, 0.30f, 0.34f, 0.60f) : accentColor);

        float contentX = rect.x + 28f;
        float contentWidth = rect.width - 48f;
        GUI.Label(new Rect(contentX, rect.y + 12f, contentWidth, 24f), CompactShellText(title, compact ? 34 : 52), _panelTitleStyle);
        GUI.Label(new Rect(contentX, rect.y + 38f, contentWidth, 18f), CompactShellText(subtitle, compact ? 28 : 48), _captionStyle);

        Rect footerRect = new Rect(contentX, rect.yMax - 28f, contentWidth, 18f);
        float bodyHeight = Mathf.Max(20f, footerRect.y - 6f - (rect.y + 60f));
        Rect bodyRect = new Rect(contentX, rect.y + 60f, contentWidth, bodyHeight);
        int maxLines = compact
            ? Mathf.Clamp(Mathf.FloorToInt((bodyRect.height - 4f) / 18f), 3, 4)
            : Mathf.Clamp(Mathf.FloorToInt((bodyRect.height - 4f) / 18f), 4, 7);
        int maxChars = compact ? 64 : 88;
        GUI.Label(bodyRect, BuildDisplayBlock(body, maxLines, maxChars), _bodyStyle);
        GUI.Label(footerRect, CompactShellText(footer, compact ? 52 : 92), _captionStyle);

        return !blocked && GUI.Button(rect, GUIContent.none, GUIStyle.none);
    }
    private string[] BuildDungeonHeaderChips(params string[] chips)
    {
        return chips ?? System.Array.Empty<string>();
    }

    private string BuildDungeonChip(string label, string value)
    {
        if (!HasMeaningfulValue(value))
        {
            return string.Empty;
        }

        return label + ": " + CompactShellText(value, 24);
    }

    private string BuildDungeonLines(params string[] lines)
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
                filtered.Add(CompactShellText(lines[i], 132));
            }
        }

        return filtered.Count > 0 ? string.Join("\n", filtered.ToArray()) : "None";
    }

    private string BuildDisplayBlock(string value, int maxLines, int maxCharsPerLine)
    {
        if (!HasMeaningfulValue(value))
        {
            return "None";
        }

        string[] rawLines = SafeShellText(value).Split('\n');
        List<string> compactedLines = new List<string>();
        for (int i = 0; i < rawLines.Length; i++)
        {
            if (HasMeaningfulValue(rawLines[i]))
            {
                compactedLines.Add(CompactShellText(rawLines[i], maxCharsPerLine));
            }
        }

        if (compactedLines.Count == 0)
        {
            return "None";
        }

        if (compactedLines.Count > maxLines)
        {
            List<string> limitedLines = new List<string>();
            for (int i = 0; i < maxLines; i++)
            {
                limitedLines.Add(compactedLines[i]);
            }

            limitedLines[maxLines - 1] = CompactShellText(limitedLines[maxLines - 1], Mathf.Max(12, maxCharsPerLine - 4)) + " ...";
            return string.Join("\n", limitedLines.ToArray());
        }

        return string.Join("\n", compactedLines.ToArray());
    }

    private string CompactShellText(string value, int maxLength)
    {
        string text = SafeShellText(value).Replace("\r", string.Empty).Replace("\n", " ").Trim();
        if (text.Length <= maxLength)
        {
            return text;
        }

        if (maxLength <= 3)
        {
            return text.Substring(0, Mathf.Max(1, maxLength));
        }

        return text.Substring(0, maxLength - 3).TrimEnd() + "...";
    }

    private string SafeShellText(string value)
    {
        return HasMeaningfulValue(value) ? value : "None";
    }

    private string ResolveOptionTag(PrototypeDungeonPanelOption option)
    {
        if (option == null)
        {
            return string.Empty;
        }

        if (option.IsBlocked)
        {
            return "Blocked";
        }

        if (option.IsSelected)
        {
            return "Selected";
        }

        if (option.IsRecommended)
        {
            return "Recommended";
        }

        return string.Empty;
    }

    private string BuildBattlePartyBody(PrototypeBattleUiSurfaceData surface)
    {
        List<string> lines = new List<string>();
        lines.Add("Total Party HP: " + SafeShellText(surface.TotalPartyHp));
        lines.Add("Condition: " + SafeShellText(surface.PartyCondition));
        PrototypeBattleUiPartyMemberData[] partyMembers = surface.PartyMembers != null ? surface.PartyMembers : System.Array.Empty<PrototypeBattleUiPartyMemberData>();
        for (int i = 0; i < partyMembers.Length; i++)
        {
            PrototypeBattleUiPartyMemberData member = partyMembers[i];
            if (member == null)
            {
                continue;
            }

            lines.Add(CompactShellText(member.DisplayName, 16) + " | HP " + member.CurrentHp + "/" + member.MaxHp + " | " + CompactShellText(member.StatusText, 18) + " | " + CompactShellText(member.LaneLabel, 16));
        }

        return BuildDungeonLines(lines.ToArray());
    }

    private string BuildRecentBattleLogs(string[] logs)
    {
        if (logs == null || logs.Length == 0)
        {
            return "No recent events.";
        }

        List<string> lines = new List<string>();
        for (int i = 0; i < logs.Length; i++)
        {
            if (HasMeaningfulValue(logs[i]))
            {
                lines.Add(CompactShellText(logs[i], 104));
            }
        }

        if (lines.Count == 0)
        {
            return "No recent events.";
        }

        int keepCount = Mathf.Min(3, lines.Count);
        List<string> recentLines = new List<string>();
        for (int i = 0; i < keepCount; i++)
        {
            recentLines.Add(lines[i]);
        }

        return string.Join("\n", recentLines.ToArray());
    }
}




