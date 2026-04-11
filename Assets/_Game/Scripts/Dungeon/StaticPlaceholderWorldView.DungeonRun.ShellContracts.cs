using System;
using UnityEngine;

public sealed class PrototypeDungeonPanelTemplate
{
    public string PanelId = string.Empty;
    public string StageKindKey = "none";
    public string StageLabel = "None";
    public string RoomTypeLabel = "None";
    public string BiomeStyleText = "None";
    public string AffordanceSummaryText = "None";
    public string ChoiceSummaryText = "None";
}

public sealed class PrototypeDungeonPanelOption
{
    public string OptionId = string.Empty;
    public string OptionLabel = "None";
    public string OptionKindKey = string.Empty;
    public string TransitionStageKindKey = string.Empty;
    public string OutcomeHintText = "None";
    public string RiskHintText = "None";
    public string RewardHintText = "None";
    public bool IsSelected;
    public bool IsRecommended;
    public bool IsBlocked;
    public int LaneIndex = -1;
    public string LaneLabel = "None";
}

public sealed class PrototypeDungeonEncounterRequest
{
    public string PanelId = string.Empty;
    public string RoomId = string.Empty;
    public string RoomLabel = "None";
    public string EncounterId = string.Empty;
    public string EncounterLabel = "None";
    public string EncounterTypeKey = string.Empty;
    public string EncounterTemplateKey = string.Empty;
    public string KeyEncounterTag = "None";
    public string DungeonId = string.Empty;
    public string DungeonLabel = "None";
    public string RouteId = string.Empty;
    public string RouteLabel = "None";
    public string RouteRiskLabel = "None";
    public string SupplyPressureSummaryText = "None";
    public string ThreatPressureSummaryText = "None";
    public string DiscoverySummaryText = "None";
    public string ExtractionPressureSummaryText = "None";
    public string ModifierSummaryText = "None";
    public string SummaryText = "None";
}

public sealed class PrototypeDungeonPanelContext
{
    public PrototypeDungeonPanelTemplate Template = new PrototypeDungeonPanelTemplate();
    public string PanelId = string.Empty;
    public string DungeonId = string.Empty;
    public string DungeonLabel = "None";
    public string RouteId = string.Empty;
    public string RouteLabel = "None";
    public string RoomStepId = string.Empty;
    public string RoomDisplayLabel = "None";
    public string RoomTypeLabel = "None";
    public int CurrentRoomIndex;
    public int TotalPlannedRooms;
    public string ProgressSummaryText = "None";
    public string NextMajorGoalText = "None";
    public string VisibleAffordanceSummaryText = "None";
    public string ChoiceSummaryText = "None";
    public string RiskPreviewSummaryText = "None";
    public string RewardPreviewSummaryText = "None";
    public string EventPreviewSummaryText = "None";
    public string EncounterRequestSummaryText = "None";
    public string SupplyPressureSummaryText = "None";
    public string TimePressureSummaryText = "None";
    public string ThreatPressureSummaryText = "None";
    public string DiscoverySummaryText = "None";
    public string ExtractionPressureSummaryText = "None";
    public string CurrentLaneSelectionText = "None";
    public string LaneDeltaSummaryText = "None";
    public PrototypeDungeonPanelOption[] Options = Array.Empty<PrototypeDungeonPanelOption>();
    public string LatestChoiceOutcomeSummaryText = "None";
    public string EventResolutionSummaryText = "None";
    public string ExtractionSummaryText = "None";
    public bool IsDiscovered;
    public bool IsResolved;
    public bool IsBlocked;
    public string DiscoveryStateText = "Hidden";
    public string ResolutionStateText = "Pending";
    public string BlockedStateText = "Open";
    public string PanelSummaryText = "None";
}

public sealed class PrototypeDungeonChoiceOutcome
{
    public string PanelId = string.Empty;
    public string StageKindKey = string.Empty;
    public string SelectedOptionId = string.Empty;
    public string SelectedOptionLabel = "None";
    public string OutcomeKindKey = string.Empty;
    public string TransitionStageKindKey = string.Empty;
    public bool IsConfirmed;
    public string SummaryText = "None";
    public string DeltaSummaryText = "None";
}

public sealed class PrototypeDungeonEventResolution
{
    public bool ShrineResolved;
    public string ShrineChoiceId = string.Empty;
    public string ShrineChoiceLabel = "None";
    public int ShrineHealAmount;
    public int ShrineLootAmount;
    public string ShrineSummaryText = "None";
    public bool PreparationResolved;
    public string PreparationChoiceId = string.Empty;
    public string PreparationChoiceLabel = "Pending";
    public int PreparationHealAmount;
    public int PreparationBonusAmount;
    public bool PreparationBonusPending;
    public string PreparationSummaryText = "Pending";
    public string LoopSummaryText = "Shrine pending | Preparation pending";
}

public sealed class PrototypeDungeonExtractionResult
{
    public string ExtractionKey = string.Empty;
    public string ExtractionLabel = "None";
    public int ReturnedLootAmount;
    public bool ExitUnlocked;
    public bool EliteDefeated;
    public bool Success;
    public string ResultSummaryText = "None";
    public string SummaryText = "Pending";
    public string PressureSummaryText = "None";
}

public sealed class PrototypeDungeonRunResultContext
{
    public PrototypeRpgRunResultSnapshot CanonicalRunResult = new PrototypeRpgRunResultSnapshot();
    public CityWriteback WorldReturnAftermathPreview = new CityWriteback();
    public OutcomeReadback WorldOutcomeReadbackPreview = new OutcomeReadback();
    public string DungeonId = string.Empty;
    public string DungeonLabel = "None";
    public string RouteId = string.Empty;
    public string RouteLabel = "None";
    public string RunOutcomeKey = string.Empty;
    public string ResultStateText = "None";
    public string CityDispatchedFromText = "None";
    public string DungeonDangerText = "None";
    public string RecommendedRouteText = "None";
    public string FollowedRecommendationText = "None";
    public string ManaShardsReturnedText = "None";
    public string StockBeforeText = "None";
    public string StockAfterText = "None";
    public string StockDeltaText = "None";
    public string NeedPressureBeforeText = "None";
    public string NeedPressureAfterText = "None";
    public string DispatchReadinessBeforeText = "None";
    public string DispatchReadinessAfterText = "None";
    public string RecoveryEtaText = "None";
    public string RouteRiskText = "None";
    public int TimeCostTurns;
    public string TimeCostSummaryText = "None";
    public string TurnsTakenText = "0";
    public string ResultSummaryText = "None";
    public string SurvivingMembersSummaryText = "None";
    public string ResourceDeltaSummaryText = "None";
    public string SupplyPressureSummaryText = "None";
    public string InjurySummaryText = "None";
    public string CasualtySummaryText = "None";
    public string DiscoveredFlagsSummaryText = "None";
    public string ThreatProgressSummaryText = "None";
    public string ClearProgressSummaryText = "None";
    public string EventLogSummaryText = "None";
    public string EncounterRequestSummaryText = "None";
    public string BattleAbsorbSummaryText = "None";
    public string ChoiceOutcomeSummaryText = "None";
    public string EventResolutionSummaryText = "None";
    public string ExtractionSummaryText = "None";
    public string ExtractionPressureSummaryText = "None";
    public int BattlesFoughtCount;
    public string KeyEncounterSummaryText = "None";
    public string SelectedRouteSummaryText = "None";
    public string FollowedRecommendationSummaryText = "None";
    public string BattleContextSummaryText = "None";
    public string BattleRuntimeSummaryText = "None";
    public string BattleRuleSummaryText = "None";
    public string BattleResultCoreSummaryText = "None";
    public string NotableBattleEventsSummaryText = "None";
    public string GearRewardCandidateSummaryText = "None";
    public string EquipSwapChoiceSummaryText = "None";
    public string GearCarryContinuitySummaryText = "None";
    public string LootGainedText = "None";
    public string BattleLootText = "None";
    public string ChestLootText = "None";
    public string EventLootText = "None";
    public string EventChoiceText = "None";
    public string PreEliteChoiceText = "Pending";
    public string PreEliteHealAmountText = "None";
    public string EliteBonusRewardEarnedText = "None";
    public string EliteBonusRewardAmountText = "None";
    public string RoomPathSummaryText = "None";
    public string PartyHpSummaryText = "None";
    public string PartyConditionText = "None";
    public string ClearedEncountersText = "0 / 0";
    public string OpenedChestsText = "0 / 0";
    public string EliteDefeatedText = "None";
    public string EliteNameText = "None";
    public string EliteRewardIdentityText = "None";
    public string EliteRewardAmountText = "None";
    public string ReturnPromptText = "None";
    public string WorldWritebackResultSummaryText = "None";
    public string WorldWritebackSurvivingMembersSummaryText = "None";
    public string WorldWritebackEncounterSummaryText = "None";
    public string WorldWritebackChoiceSummaryText = "None";
    public string WorldWritebackLootSummaryText = "None";
    public string WorldWritebackRouteSummaryText = "None";
    public string WorldWritebackDungeonSummaryText = "None";
}

public sealed class PrototypeDungeonRunShellSurfaceData
{
    public string ModeKey = "none";
    public string ModeLabel = "None";
    public bool IsBattleViewActive;
    public bool IsRouteChoiceVisible;
    public bool IsEventChoiceVisible;
    public bool IsPreEliteChoiceVisible;
    public bool IsResultPanelVisible;
    public string RunStateLabel = "None";
    public string CurrentCityLabel = "None";
    public string CurrentDungeonLabel = "None";
    public string DungeonDangerLabel = "None";
    public string CurrentRouteLabel = "None";
    public string RouteRiskLabel = "None";
    public string ActivePartyLabel = "None";
    public string CurrentRoomLabel = "None";
    public string CurrentRoomTypeLabel = "None";
    public string RoomProgressText = "None";
    public string NextMajorGoalText = "None";
    public int TurnCount;
    public string TotalPartyHpText = "None";
    public string PartyConditionText = "None";
    public string SustainPressureText = "None";
    public string CarriedLootText = "None";
    public string EncounterProgressText = "None";
    public string ExitUnlockedText = "None";
    public string EliteStatusText = "None";
    public string EliteEncounterNameText = "None";
    public string EliteRewardHintText = "None";
    public string EventStatusText = "None";
    public string EventChoiceText = "None";
    public string PreEliteChoiceText = "Pending";
    public string LootBreakdownText = "None";
    public string CurrentSelectionPromptText = "None";
    public string DispatchPolicyText = "None";
    public string RecommendedRouteText = "None";
    public string RecommendationReasonText = "None";
    public string RecoveryAdviceText = "None";
    public string ExpectedNeedImpactText = "None";
    public string RouteChoiceTitleText = "None";
    public string RouteChoiceDescriptionText = "None";
    public string RouteChoicePromptText = "None";
    public string EventTitleText = "None";
    public string EventDescriptionText = "None";
    public string EventPromptText = "None";
    public string PreEliteTitleText = "None";
    public string PreEliteDescriptionText = "None";
    public string PreElitePromptText = "None";
    public string RecentBattleLog1Text = "None";
    public string RecentBattleLog2Text = "None";
    public string RecentBattleLog3Text = "None";
    public ExpeditionStartContext ExpeditionStartContext = new ExpeditionStartContext();
    public PrototypeDungeonPanelContext PanelContext = new PrototypeDungeonPanelContext();
    public PrototypeDungeonRunResultContext ResultContext = new PrototypeDungeonRunResultContext();
}

public sealed partial class StaticPlaceholderWorldView
{
    private ExpeditionStartContext _latestExpeditionStartContext = new ExpeditionStartContext();
    private PrototypeDungeonRunResultContext _latestDungeonRunResultContext = new PrototypeDungeonRunResultContext();
    private PrototypeDungeonEncounterRequest _latestDungeonEncounterRequest = new PrototypeDungeonEncounterRequest();
    private PrototypeDungeonChoiceOutcome _latestDungeonChoiceOutcome = new PrototypeDungeonChoiceOutcome();
    private PrototypeDungeonEventResolution _latestDungeonEventResolution = new PrototypeDungeonEventResolution();
    private PrototypeDungeonExtractionResult _latestDungeonExtractionResult = new PrototypeDungeonExtractionResult();

    public ExpeditionStartContext LatestExpeditionStartContext => CopyExpeditionStartContext(_latestExpeditionStartContext);
    public PrototypeDungeonPanelContext CurrentDungeonPanelContext => BuildCurrentDungeonPanelContext();
    public PrototypeBattleContextData CurrentBattleContextSnapshot => BuildCurrentBattleContextView();
    public PrototypeDungeonEncounterRequest LatestDungeonEncounterRequest => CopyDungeonEncounterRequest(_latestDungeonEncounterRequest);
    public PrototypeDungeonChoiceOutcome LatestDungeonChoiceOutcome => CopyDungeonChoiceOutcome(_latestDungeonChoiceOutcome);
    public PrototypeDungeonEventResolution LatestDungeonEventResolution => CopyDungeonEventResolution(_latestDungeonEventResolution);
    public PrototypeDungeonExtractionResult LatestDungeonExtractionResult => CopyDungeonExtractionResult(_latestDungeonExtractionResult);
    public PrototypeDungeonRunShellSurfaceData CurrentDungeonRunShellSurfaceData => BuildDungeonRunShellSurfaceData();
    public PrototypeDungeonRunResultContext LatestDungeonRunResultContext
    {
        get
        {
            PrototypeDungeonRunResultContext context = HasStructuredRunResultContext()
                ? _latestDungeonRunResultContext
                : BuildDungeonRunResultContext(_latestRpgRunResultSnapshot);
            return CopyDungeonRunResultContext(context);
        }
    }

    private bool HasStructuredRunResultContext()
    {
        return _latestDungeonRunResultContext != null &&
               (!string.IsNullOrEmpty(_latestDungeonRunResultContext.RunOutcomeKey) ||
                !string.IsNullOrEmpty(_latestDungeonRunResultContext.DungeonLabel));
    }

    private PrototypeDungeonRunShellSurfaceData BuildDungeonRunShellSurfaceData()
    {
        PrototypeDungeonRunShellSurfaceData surface = new PrototypeDungeonRunShellSurfaceData();
        surface.ExpeditionStartContext = LatestExpeditionStartContext;
        surface.PanelContext = CurrentDungeonPanelContext;
        surface.ResultContext = LatestDungeonRunResultContext;
        surface.IsBattleViewActive = IsBattleViewActive;
        surface.IsRouteChoiceVisible = IsDungeonRouteChoiceVisible;
        surface.IsEventChoiceVisible = IsDungeonEventChoiceVisible;
        surface.IsPreEliteChoiceVisible = IsDungeonPreEliteChoiceVisible;
        surface.IsResultPanelVisible = IsDungeonResultPanelVisible;
        surface.ModeKey = ResolveDungeonShellSurfaceModeKey(surface);
        surface.ModeLabel = ResolveDungeonShellSurfaceModeLabel(surface);
        surface.RunStateLabel = string.IsNullOrEmpty(DungeonRunStateText) ? "None" : DungeonRunStateText;
        surface.CurrentCityLabel = string.IsNullOrEmpty(CurrentCityText) ? "None" : CurrentCityText;
        surface.CurrentDungeonLabel = string.IsNullOrEmpty(CurrentDungeonRunText) ? "None" : CurrentDungeonRunText;
        surface.DungeonDangerLabel = string.IsNullOrEmpty(CurrentDungeonDangerText) ? "None" : CurrentDungeonDangerText;
        surface.CurrentRouteLabel = string.IsNullOrEmpty(CurrentRouteText) ? "None" : CurrentRouteText;
        surface.RouteRiskLabel = string.IsNullOrEmpty(CurrentRouteRiskText) ? "None" : CurrentRouteRiskText;
        surface.ActivePartyLabel = string.IsNullOrEmpty(ActiveDungeonPartyText) ? "None" : ActiveDungeonPartyText;
        surface.CurrentRoomLabel = string.IsNullOrEmpty(CurrentRoomText) ? "None" : CurrentRoomText;
        surface.CurrentRoomTypeLabel = string.IsNullOrEmpty(CurrentRoomTypeText) ? "None" : CurrentRoomTypeText;
        surface.RoomProgressText = string.IsNullOrEmpty(RoomProgressText) ? "None" : RoomProgressText;
        surface.NextMajorGoalText = string.IsNullOrEmpty(NextMajorGoalText) ? "None" : NextMajorGoalText;
        surface.TurnCount = Mathf.Max(0, DungeonRunTurnCount);
        surface.TotalPartyHpText = string.IsNullOrEmpty(TotalPartyHpText) ? "None" : TotalPartyHpText;
        surface.PartyConditionText = string.IsNullOrEmpty(PartyConditionText) ? "None" : PartyConditionText;
        surface.SustainPressureText = string.IsNullOrEmpty(SustainPressureText) ? "None" : SustainPressureText;
        surface.CarriedLootText = string.IsNullOrEmpty(CarriedLootText) ? "None" : CarriedLootText;
        surface.EncounterProgressText = string.IsNullOrEmpty(EncounterProgressText) ? "None" : EncounterProgressText;
        surface.ExitUnlockedText = string.IsNullOrEmpty(ExitUnlockedText) ? "None" : ExitUnlockedText;
        surface.EliteStatusText = string.IsNullOrEmpty(EliteStatusText) ? "None" : EliteStatusText;
        surface.EliteEncounterNameText = string.IsNullOrEmpty(EliteEncounterNameText) ? "None" : EliteEncounterNameText;
        surface.EliteRewardHintText = string.IsNullOrEmpty(EliteRewardHintText) ? "None" : EliteRewardHintText;
        surface.EventStatusText = string.IsNullOrEmpty(EventStatusText) ? "None" : EventStatusText;
        surface.EventChoiceText = string.IsNullOrEmpty(EventChoiceText) ? "None" : EventChoiceText;
        surface.PreEliteChoiceText = string.IsNullOrEmpty(PreEliteChoiceText) ? "Pending" : PreEliteChoiceText;
        surface.LootBreakdownText = string.IsNullOrEmpty(LootBreakdownText) ? "None" : LootBreakdownText;
        surface.CurrentSelectionPromptText = string.IsNullOrEmpty(CurrentSelectionPromptText) ? "None" : CurrentSelectionPromptText;
        surface.DispatchPolicyText = string.IsNullOrEmpty(DispatchPolicyText) ? "None" : DispatchPolicyText;
        surface.RecommendedRouteText = string.IsNullOrEmpty(RecommendedRouteText) ? "None" : RecommendedRouteText;
        surface.RecommendationReasonText = string.IsNullOrEmpty(RecommendationReasonText) ? "None" : RecommendationReasonText;
        surface.RecoveryAdviceText = string.IsNullOrEmpty(RecoveryAdviceText) ? "None" : RecoveryAdviceText;
        surface.ExpectedNeedImpactText = string.IsNullOrEmpty(ExpectedNeedImpactText) ? "None" : ExpectedNeedImpactText;
        surface.RouteChoiceTitleText = string.IsNullOrEmpty(RouteChoiceTitleText) ? "None" : RouteChoiceTitleText;
        surface.RouteChoiceDescriptionText = string.IsNullOrEmpty(RouteChoiceDescriptionText) ? "None" : RouteChoiceDescriptionText;
        surface.RouteChoicePromptText = string.IsNullOrEmpty(RouteChoicePromptText) ? "None" : RouteChoicePromptText;
        surface.EventTitleText = string.IsNullOrEmpty(EventTitleText) ? "None" : EventTitleText;
        surface.EventDescriptionText = string.IsNullOrEmpty(EventDescriptionText) ? "None" : EventDescriptionText;
        surface.EventPromptText = string.IsNullOrEmpty(EventPromptText) ? "None" : EventPromptText;
        surface.PreEliteTitleText = string.IsNullOrEmpty(PreEliteTitleText) ? "None" : PreEliteTitleText;
        surface.PreEliteDescriptionText = string.IsNullOrEmpty(PreEliteDescriptionText) ? "None" : PreEliteDescriptionText;
        surface.PreElitePromptText = string.IsNullOrEmpty(PreElitePromptText) ? "None" : PreElitePromptText;
        surface.RecentBattleLog1Text = string.IsNullOrEmpty(RecentBattleLog1Text) ? "None" : RecentBattleLog1Text;
        surface.RecentBattleLog2Text = string.IsNullOrEmpty(RecentBattleLog2Text) ? "None" : RecentBattleLog2Text;
        surface.RecentBattleLog3Text = string.IsNullOrEmpty(RecentBattleLog3Text) ? "None" : RecentBattleLog3Text;
        return surface;
    }

    private string ResolveDungeonShellSurfaceModeKey(PrototypeDungeonRunShellSurfaceData surface)
    {
        if (surface == null)
        {
            return "none";
        }

        if (surface.IsResultPanelVisible)
        {
            return "result_panel";
        }

        if (surface.IsBattleViewActive)
        {
            return "battle";
        }

        if (surface.IsRouteChoiceVisible)
        {
            return "route_choice";
        }

        if (surface.IsEventChoiceVisible)
        {
            return "event_choice";
        }

        if (surface.IsPreEliteChoiceVisible)
        {
            return "pre_elite_choice";
        }

        return "explore";
    }

    private string ResolveDungeonShellSurfaceModeLabel(PrototypeDungeonRunShellSurfaceData surface)
    {
        if (surface == null)
        {
            return "None";
        }

        switch (surface.ModeKey)
        {
            case "result_panel":
                return surface.ResultContext != null && !string.IsNullOrEmpty(surface.ResultContext.ResultStateText)
                    ? surface.ResultContext.ResultStateText
                    : "Run Result";
            case "route_choice":
                return surface.PanelContext != null ? surface.PanelContext.Template.StageLabel : "Route Choice";
            case "event_choice":
                return string.IsNullOrEmpty(surface.EventTitleText) ? "Event Choice" : surface.EventTitleText;
            case "pre_elite_choice":
                return string.IsNullOrEmpty(surface.PreEliteTitleText) ? "Elite Preparation" : surface.PreEliteTitleText;
            case "battle":
                return surface.PanelContext != null ? surface.PanelContext.Template.StageLabel : "Battle";
            default:
                return surface.PanelContext != null ? surface.PanelContext.Template.StageLabel : "Explore";
        }
    }

    private void CaptureLatestExpeditionStartContext()
    {
        _latestExpeditionStartContext = BuildCurrentExpeditionStartContext();
    }

    private void CaptureLatestRunResultContext(PrototypeRpgRunResultSnapshot runResult)
    {
        _latestDungeonRunResultContext = BuildDungeonRunResultContext(runResult);
    }

    private void ResetDungeonRunShellContractState(bool preserveExpeditionStartContext = false)
    {
        if (!preserveExpeditionStartContext)
        {
            _latestExpeditionStartContext = new ExpeditionStartContext();
        }

        _latestDungeonRunResultContext = new PrototypeDungeonRunResultContext();
        _latestDungeonEncounterRequest = new PrototypeDungeonEncounterRequest();
        _latestDungeonChoiceOutcome = new PrototypeDungeonChoiceOutcome();
        _latestDungeonEventResolution = new PrototypeDungeonEventResolution();
        _latestDungeonExtractionResult = new PrototypeDungeonExtractionResult();
    }

    private ExpeditionStartContext BuildCurrentExpeditionStartContext()
    {
        return BuildProjectedExpeditionStartContext();
    }

    private PrototypeDungeonEncounterRequest BuildCurrentEncounterRequest(DungeonRoomTemplateData room = null, DungeonEncounterRuntimeData encounter = null)
    {
        DungeonRoomTemplateData safeRoom = room ?? GetCurrentPlannedRoomStep();
        DungeonEncounterRuntimeData safeEncounter = encounter ?? GetActiveEncounter();
        if (safeEncounter == null && safeRoom != null && !string.IsNullOrEmpty(safeRoom.EncounterId))
        {
            safeEncounter = safeRoom.RoomType == DungeonRoomType.Elite ? GetEliteEncounter() : GetEncounterById(safeRoom.EncounterId);
        }

        PrototypeDungeonEncounterRequest request = new PrototypeDungeonEncounterRequest();
        request.PanelId = ResolveCurrentPanelStageKindKey(safeRoom);
        request.RoomId = safeRoom != null ? safeRoom.RoomId : string.Empty;
        request.RoomLabel = safeRoom != null ? safeRoom.DisplayName : "None";
        request.EncounterId = safeEncounter != null ? safeEncounter.EncounterId : string.Empty;
        request.EncounterLabel = safeEncounter != null && !string.IsNullOrEmpty(safeEncounter.DisplayName)
            ? safeEncounter.DisplayName
            : safeRoom != null
                ? safeRoom.DisplayName
                : "None";
        request.EncounterTypeKey = safeEncounter != null && safeEncounter.IsEliteEncounter
            ? "elite"
            : safeRoom != null && safeRoom.RoomType == DungeonRoomType.Elite
                ? "elite"
                : "skirmish";
        request.EncounterTemplateKey = !string.IsNullOrEmpty(request.EncounterId) ? request.EncounterId : request.RoomId;
        request.KeyEncounterTag = request.EncounterTypeKey == "elite"
            ? "Final Elite"
            : safeRoom != null && !string.IsNullOrEmpty(safeRoom.RoomTypeLabel)
                ? safeRoom.RoomTypeLabel
                : "Encounter";
        request.DungeonId = string.IsNullOrEmpty(_currentDungeonId) ? string.Empty : _currentDungeonId;
        request.DungeonLabel = string.IsNullOrEmpty(_currentDungeonName) ? "None" : _currentDungeonName;
        request.RouteId = string.IsNullOrEmpty(_selectedRouteId) ? string.Empty : _selectedRouteId;
        request.RouteLabel = string.IsNullOrEmpty(_selectedRouteLabel) ? "None" : _selectedRouteLabel;
        request.RouteRiskLabel = string.IsNullOrEmpty(_selectedRouteRiskLabel) ? "None" : _selectedRouteRiskLabel;
        request.SupplyPressureSummaryText = BuildCurrentSupplyPressureSummaryText();
        request.ThreatPressureSummaryText = BuildCurrentThreatPressureSummaryText();
        request.DiscoverySummaryText = BuildCurrentDiscoverySummaryText();
        request.ExtractionPressureSummaryText = BuildCurrentExtractionPressureSummaryText();
        request.ModifierSummaryText = GetCurrentDungeonDangerText() + " | " + BuildSelectedRouteSummary() + " | Lane " + CurrentLaneSelectionText;
        request.SummaryText = request.EncounterLabel + " | " + request.KeyEncounterTag + " | Lane " + CurrentLaneSelectionText + " | " + CurrentPanelDeltaSummaryText + " | " + request.ThreatPressureSummaryText + " | " + request.ExtractionPressureSummaryText;
        return request;
    }

    private string BuildCurrentEncounterRequestSummaryText()
    {
        if (_latestDungeonEncounterRequest != null && !string.IsNullOrEmpty(_latestDungeonEncounterRequest.SummaryText) && _latestDungeonEncounterRequest.SummaryText != "None")
        {
            return _latestDungeonEncounterRequest.SummaryText;
        }

        PrototypeDungeonEncounterRequest request = BuildCurrentEncounterRequest();
        return request != null && !string.IsNullOrEmpty(request.SummaryText) ? request.SummaryText : "None";
    }

    private string BuildCurrentBattleAbsorbSummaryText(string outcomeKey)
    {
        string absorbLabel = outcomeKey == PrototypeBattleOutcomeKeys.RunClear || outcomeKey == PrototypeBattleOutcomeKeys.EncounterVictory
            ? "Victory absorbed"
            : outcomeKey == PrototypeBattleOutcomeKeys.RunDefeat
                ? "Defeat absorbed"
                : outcomeKey == PrototypeBattleOutcomeKeys.RunRetreat
                    ? "Retreat absorbed"
                    : "Battle absorb pending";
        return absorbLabel + " | " + BuildCurrentEncounterRequestSummaryText() + " | Lane " + CurrentLaneSelectionText + " | " + CurrentPanelDeltaSummaryText + " | Battles " + Mathf.Max(0, _battlesFoughtCount) + " | " + BuildCurrentThreatPressureSummaryText() + " | " + BuildCurrentExtractionPressureSummaryText();
    }

    private string ResolveExpeditionContextRouteId()
    {
        if (!string.IsNullOrEmpty(_selectedRouteChoiceId))
        {
            return NormalizeRouteChoiceId(_selectedRouteChoiceId);
        }

        if (!string.IsNullOrEmpty(_selectedRouteId))
        {
            return NormalizeRouteChoiceId(_selectedRouteId);
        }

        if (!string.IsNullOrEmpty(_recommendedRouteId))
        {
            return NormalizeRouteChoiceId(_recommendedRouteId);
        }

        return string.Empty;
    }

    private string BuildExpeditionSupplySummaryText(PrototypeWorldDispatchBriefingSnapshot briefing)
    {
        if (string.IsNullOrEmpty(_currentHomeCityId) && (_activeDungeonParty == null || _activeDungeonParty.Members == null))
        {
            return "None";
        }

        int manaShardStock = string.IsNullOrEmpty(_currentHomeCityId) ? 0 : GetCityManaShardStock(_currentHomeCityId);
        string partySummary = briefing != null && !string.IsNullOrEmpty(briefing.PartyRoleSummary)
            ? briefing.PartyRoleSummary
            : ActiveDungeonPartyText;
        string hpSummary = BuildTotalPartyHpSummary();
        return "Stock " + BuildLootAmountText(manaShardStock) + " | Party " + partySummary + " | HP " + hpSummary;
    }

    private string BuildExpeditionWorldModifierSummaryText(PrototypeWorldDispatchBriefingSnapshot briefing)
    {
        if (string.IsNullOrEmpty(_currentHomeCityId))
        {
            return "None";
        }

        string readinessText = briefing != null && !string.IsNullOrEmpty(briefing.ReadinessLabel)
            ? briefing.ReadinessLabel
            : GetDispatchReadinessText(_currentHomeCityId);
        return "Need " + GetCurrentNeedPressureText() + " | Dispatch " + GetCurrentDispatchPolicyText() + " | Readiness " + readinessText + ".";
    }

    private string BuildProjectedExpeditionSupplyPressureSummaryText(PrototypeWorldSnapshot snapshot, PrototypeWorldDispatchBriefingSnapshot briefing)
    {
        string supplySummary = BuildExpeditionSupplySummaryText(briefing);
        string needPressure = snapshot != null && snapshot.CitySummary != null && !string.IsNullOrEmpty(snapshot.CitySummary.NeedPressureText)
            ? snapshot.CitySummary.NeedPressureText
            : GetCurrentNeedPressureText();
        return "Launch reserves | " + supplySummary + " | Need " + needPressure;
    }

    private string BuildProjectedExpeditionTimePressureSummaryText(PrototypeWorldSnapshot snapshot, PrototypeWorldDispatchBriefingSnapshot briefing)
    {
        string routePreview = snapshot != null && !string.IsNullOrEmpty(snapshot.SelectedDungeonId)
            ? BuildRoutePreviewSummaryText(snapshot.SelectedDungeonId, snapshot.SelectedRouteId)
            : "Route pending";
        string riskText = snapshot != null && !string.IsNullOrEmpty(snapshot.SelectedRouteId)
            ? BuildRouteRiskSummaryForRoute(snapshot.SelectedDungeonId, snapshot.SelectedRouteId)
            : "Risk pending";
        return "Planned pace | " + riskText + " | " + routePreview;
    }

    private string BuildProjectedExpeditionThreatPressureSummaryText(PrototypeWorldSnapshot snapshot, PrototypeWorldDispatchBriefingSnapshot briefing)
    {
        string dungeonDanger = snapshot != null && snapshot.DungeonSummary != null && !string.IsNullOrEmpty(snapshot.DungeonSummary.DangerText)
            ? snapshot.DungeonSummary.DangerText
            : "None";
        string routeRisk = snapshot != null && !string.IsNullOrEmpty(snapshot.SelectedRouteId)
            ? BuildRouteRiskSummaryForRoute(snapshot.SelectedDungeonId, snapshot.SelectedRouteId)
            : "Risk pending";
        return "Threat forecast | " + dungeonDanger + " | " + routeRisk;
    }

    private string BuildProjectedExpeditionDiscoverySummaryText(PrototypeWorldSnapshot snapshot, PrototypeWorldDispatchBriefingSnapshot briefing)
    {
        string routePreview = snapshot != null && !string.IsNullOrEmpty(snapshot.SelectedDungeonId)
            ? BuildRoomPathPreviewText(snapshot.SelectedDungeonId, snapshot.SelectedRouteId)
            : "Unknown path";
        return "Known route | " + routePreview + " | Hidden outcomes remain.";
    }

    private string BuildProjectedExpeditionExtractionPressureSummaryText(PrototypeWorldSnapshot snapshot, PrototypeWorldDispatchBriefingSnapshot briefing)
    {
        string rewardPreview = snapshot != null && !string.IsNullOrEmpty(snapshot.SelectedDungeonId)
            ? BuildRouteRewardPreviewText(snapshot.SelectedDungeonId)
            : "Reward preview pending";
        return "Extraction after elite only | " + rewardPreview;
    }

    private int GetResolvedPlannedRoomCount()
    {
        if (_plannedRooms == null || _plannedRooms.Count <= 0)
        {
            return 0;
        }

        int resolvedCount = 0;
        for (int i = 0; i < _plannedRooms.Count; i++)
        {
            DungeonRoomTemplateData plannedRoom = _plannedRooms[i];
            if (plannedRoom == null)
            {
                continue;
            }

            bool resolved = false;
            switch (plannedRoom.RoomType)
            {
                case DungeonRoomType.Start:
                    resolved = _dungeonRunState != DungeonRunState.None && _currentRoomIndex > 1;
                    break;
                case DungeonRoomType.Skirmish:
                    DungeonEncounterRuntimeData encounter = GetEncounterById(plannedRoom.EncounterId);
                    resolved = encounter != null && encounter.IsCleared;
                    break;
                case DungeonRoomType.Cache:
                    resolved = _activeChest != null && _activeChest.IsOpened;
                    break;
                case DungeonRoomType.Shrine:
                    resolved = _eventResolved;
                    break;
                case DungeonRoomType.Preparation:
                    resolved = _preEliteDecisionResolved;
                    break;
                case DungeonRoomType.Elite:
                    resolved = _eliteDefeated;
                    break;
            }

            if (resolved)
            {
                resolvedCount += 1;
            }
        }

        return resolvedCount;
    }

    private bool HasPlannedRoomType(DungeonRoomType roomType)
    {
        if (_plannedRooms == null)
        {
            return false;
        }

        for (int i = 0; i < _plannedRooms.Count; i++)
        {
            if (_plannedRooms[i] != null && _plannedRooms[i].RoomType == roomType)
            {
                return true;
            }
        }

        return false;
    }

    private bool HasResolvedRoomType(DungeonRoomType roomType)
    {
        switch (roomType)
        {
            case DungeonRoomType.Shrine:
                return _eventResolved;
            case DungeonRoomType.Cache:
                return _activeChest != null && _activeChest.IsOpened;
            case DungeonRoomType.Preparation:
                return _preEliteDecisionResolved;
            case DungeonRoomType.Elite:
                return _eliteDefeated;
            case DungeonRoomType.Skirmish:
                return _clearedEncounterCount > 0;
            case DungeonRoomType.Start:
                return _dungeonRunState != DungeonRunState.None && _currentRoomIndex > 1;
            default:
                return false;
        }
    }

    private bool HasReachedRoomType(DungeonRoomType roomType)
    {
        if (_plannedRooms == null || _plannedRooms.Count <= 0)
        {
            return false;
        }

        for (int i = 0; i < _plannedRooms.Count; i++)
        {
            DungeonRoomTemplateData plannedRoom = _plannedRooms[i];
            if (plannedRoom != null && plannedRoom.RoomType == roomType)
            {
                return _currentRoomIndex >= (i + 1);
            }
        }

        return false;
    }

    private string BuildRoomDiscoveryStateText(DungeonRoomType roomType, string label)
    {
        string safeLabel = string.IsNullOrEmpty(label) ? roomType.ToString() : label;
        if (!HasPlannedRoomType(roomType))
        {
            return safeLabel + " n/a";
        }

        if (HasResolvedRoomType(roomType))
        {
            return safeLabel + " resolved";
        }

        if (HasReachedRoomType(roomType))
        {
            return safeLabel + " discovered";
        }

        return safeLabel + " hidden";
    }

    private string BuildCurrentSupplyPressureSummaryText()
    {
        return "Reserves " + GetPartyConditionText() + " | HP " + BuildTotalPartyHpSummary() + " | Sustain " + GetSustainPressureText();
    }

    private string BuildCurrentTimePressureSummaryText()
    {
        int totalRooms = _plannedRooms != null ? _plannedRooms.Count : 0;
        string goal = _eliteDefeated
            ? "Extract"
            : _eliteEncounterActive
                ? "Survive elite"
                : _preEliteDecisionResolved
                    ? "Reach elite"
                    : HasPlannedRoomType(DungeonRoomType.Preparation)
                        ? "Resolve preparation"
                        : GetNextMajorGoalText();
        return "Turns " + Mathf.Max(0, _runTurnCount) + " | Rooms " + GetResolvedPlannedRoomCount() + "/" + totalRooms + " | Goal " + goal;
    }

    private string BuildCurrentThreatPressureSummaryText()
    {
        int threatScore = _panelThreatTicks + (_clearedEncounterCount > 0 ? 1 : 0) + (_preEliteDecisionResolved ? 1 : 0) + (_eliteEncounterActive ? 2 : 0);
        string pressure = threatScore >= 4
            ? "Threat peak"
            : threatScore >= 3
                ? "Threat high"
                : threatScore >= 2
                    ? "Threat building"
                    : threatScore >= 1
                        ? "Threat low+"
                        : "Threat low";
        return pressure + " | Rail ticks " + _panelThreatTicks + " | " + BuildSelectedRouteSummary() + " | Encounters " + _clearedEncounterCount + "/" + TotalEncounterCount;
    }

    private string BuildCurrentDiscoverySummaryText()
    {
        return "Flags " + _panelDiscoveryFlags + " | " + BuildRoomDiscoveryStateText(DungeonRoomType.Shrine, "Shrine") +
            " | " + BuildRoomDiscoveryStateText(DungeonRoomType.Cache, "Cache") +
            " | " + BuildRoomDiscoveryStateText(DungeonRoomType.Preparation, "Prep") +
            " | " + BuildRoomDiscoveryStateText(DungeonRoomType.Elite, "Elite");
    }

    private string BuildCurrentExtractionPressureSummaryText()
    {
        string gateState = _exitUnlocked ? "Extraction open" : _eliteDefeated ? "Exit unlocking" : "Extraction gated";
        string carryText = _carriedLootAmount > 0 ? BuildLootAmountText(_carriedLootAmount) : "None";
        return gateState + " | Pull " + _panelExtractionTicks + " | Carry " + carryText + " | Party " + GetPartyConditionText();
    }

    private PrototypeDungeonPanelContext BuildCurrentDungeonPanelContext()
    {
        PrototypeDungeonPanelContext context = new PrototypeDungeonPanelContext();
        DungeonRoomTemplateData room = GetCurrentPlannedRoomStep();
        string stageKindKey = ResolveCurrentPanelStageKindKey(room);
        PrototypeDungeonPanelTemplate template = BuildDungeonPanelTemplate(stageKindKey, room);

        context.Template = template;
        context.PanelId = template.PanelId;
        context.DungeonId = string.IsNullOrEmpty(_currentDungeonId) ? string.Empty : _currentDungeonId;
        context.DungeonLabel = string.IsNullOrEmpty(_currentDungeonName) ? "None" : _currentDungeonName;
        context.RouteId = string.IsNullOrEmpty(_selectedRouteId) ? string.Empty : _selectedRouteId;
        context.RouteLabel = string.IsNullOrEmpty(_selectedRouteLabel) ? "None" : _selectedRouteLabel;
        context.RoomStepId = room != null ? room.RoomId : string.Empty;
        context.RoomDisplayLabel = room != null ? room.DisplayName : template.StageLabel;
        context.RoomTypeLabel = room != null ? room.RoomTypeLabel : template.RoomTypeLabel;
        context.CurrentRoomIndex = Mathf.Max(0, _currentRoomIndex);
        context.TotalPlannedRooms = _plannedRooms != null ? _plannedRooms.Count : 0;
        context.ProgressSummaryText = GetRoomProgressText();
        context.NextMajorGoalText = GetNextMajorGoalText();
        context.VisibleAffordanceSummaryText = template.AffordanceSummaryText;
        context.ChoiceSummaryText = !string.IsNullOrEmpty(CurrentSelectionPromptText) && CurrentSelectionPromptText != "None"
            ? CurrentSelectionPromptText
            : template.ChoiceSummaryText;
        context.RiskPreviewSummaryText = BuildPanelRiskPreviewText(stageKindKey, room);
        context.RewardPreviewSummaryText = BuildPanelRewardPreviewText(stageKindKey, room);
        context.EventPreviewSummaryText = BuildPanelEventPreviewText(stageKindKey, room);
        context.EncounterRequestSummaryText = BuildCurrentEncounterRequestSummaryText();
        context.SupplyPressureSummaryText = BuildCurrentSupplyPressureSummaryText();
        context.TimePressureSummaryText = BuildCurrentTimePressureSummaryText();
        context.ThreatPressureSummaryText = BuildCurrentThreatPressureSummaryText();
        context.DiscoverySummaryText = BuildCurrentDiscoverySummaryText();
        context.ExtractionPressureSummaryText = BuildCurrentExtractionPressureSummaryText();
        context.CurrentLaneSelectionText = CurrentLaneSelectionText;
        context.LaneDeltaSummaryText = CurrentPanelDeltaSummaryText;
        context.Options = BuildCurrentDungeonPanelOptions(stageKindKey, room);
        context.LatestChoiceOutcomeSummaryText = BuildLatestChoiceOutcomeSummaryText();
        context.EventResolutionSummaryText = BuildCurrentDungeonEventResolutionSummaryText();
        context.ExtractionSummaryText = BuildCurrentExtractionSummaryText();
        context.IsDiscovered = room != null || _dungeonRunState == DungeonRunState.RouteChoice || _dungeonRunState == DungeonRunState.ResultPanel || stageKindKey == "extraction_panel";
        context.IsResolved = EvaluateCurrentPanelResolved(stageKindKey, room);
        context.IsBlocked = EvaluateCurrentPanelBlocked(stageKindKey, room);
        context.DiscoveryStateText = context.IsDiscovered ? "Discovered" : "Hidden";
        context.ResolutionStateText = context.IsResolved ? "Resolved" : "Pending";
        context.BlockedStateText = context.IsBlocked ? "Blocked" : "Open";
        context.PanelSummaryText = template.StageLabel + " | " + context.ProgressSummaryText + " | Lane " + context.CurrentLaneSelectionText + " | " + context.ThreatPressureSummaryText + " | Options " + context.Options.Length;
        return context;
    }

    private string ResolveCurrentPanelStageKindKey(DungeonRoomTemplateData room)
    {
        if (_dungeonRunState == DungeonRunState.RouteChoice)
        {
            return "route_choice";
        }

        if (_dungeonRunState == DungeonRunState.ResultPanel)
        {
            return "result_panel";
        }

        if (_dungeonRunState == DungeonRunState.EventChoice)
        {
            return "shrine_choice";
        }

        if (_dungeonRunState == DungeonRunState.PreEliteChoice)
        {
            return "pre_elite_choice";
        }

        if (_dungeonRunState == DungeonRunState.Battle)
        {
            return _eliteEncounterActive ? "elite_battle" : "skirmish_battle";
        }

        if (room == null)
        {
            if (_dungeonRunState == DungeonRunState.Explore && _exitUnlocked && _eliteDefeated)
            {
                return "extraction_panel";
            }

            return _dungeonRunState == DungeonRunState.Explore ? "explore" : "none";
        }

        switch (room.RoomType)
        {
            case DungeonRoomType.Start:
                return "start_room";
            case DungeonRoomType.Skirmish:
                return "skirmish_room";
            case DungeonRoomType.Cache:
                return "cache_room";
            case DungeonRoomType.Shrine:
                return "shrine_room";
            case DungeonRoomType.Preparation:
                return "preparation_room";
            case DungeonRoomType.Elite:
                return "elite_room";
            default:
                return "explore";
        }
    }

    private PrototypeDungeonPanelTemplate BuildDungeonPanelTemplate(string stageKindKey, DungeonRoomTemplateData room)
    {
        PrototypeDungeonPanelTemplate template = new PrototypeDungeonPanelTemplate();
        template.StageKindKey = string.IsNullOrEmpty(stageKindKey) ? "none" : stageKindKey;
        template.PanelId = template.StageKindKey + ":" + (room != null ? room.RoomId : "panel");
        template.StageLabel = ResolvePanelStageLabel(stageKindKey, room);
        template.RoomTypeLabel = room != null ? room.RoomTypeLabel : template.StageLabel;
        template.BiomeStyleText = string.IsNullOrEmpty(_currentDungeonId) ? "None" : BuildDungeonDangerSummaryText(_currentDungeonId);
        template.AffordanceSummaryText = BuildPanelAffordanceSummaryText(stageKindKey, room);
        template.ChoiceSummaryText = BuildPanelChoiceSummaryText(stageKindKey, room);
        return template;
    }

    private string ResolvePanelStageLabel(string stageKindKey, DungeonRoomTemplateData room)
    {
        if (room != null)
        {
            return room.DisplayName;
        }

        switch (stageKindKey)
        {
            case "route_choice":
                return "Route Choice";
            case "result_panel":
                return "Run Result";
            case "shrine_choice":
                return GetCurrentEventTitleText();
            case "pre_elite_choice":
                return GetCurrentPreEliteTitleText();
            case "elite_battle":
                return GetEliteEncounterNameText();
            case "skirmish_battle":
                return GetCurrentEncounterNameText();
            case "extraction_panel":
                return "Extraction Panel";
            default:
                return "Dungeon Panel";
        }
    }

    private string BuildPanelAffordanceSummaryText(string stageKindKey, DungeonRoomTemplateData room)
    {
        switch (stageKindKey)
        {
            case "route_choice":
                return "Read both opening lines, then lock the route the city can actually carry.";
            case "result_panel":
                return "Review the expedition outcome and return to the world.";
            case "shrine_choice":
                return "Decide whether the shrine steadies the party or gets cashed out for shards.";
            case "pre_elite_choice":
                return "Choose whether the elite is entered steadier or with a bigger bounty at stake.";
            case "shrine_room":
                return "Choose one of three shrine approaches to open the room choice.";
            case "cache_room":
                return "Choose how to secure the cache through the side-panel shell.";
            case "preparation_room":
                return "Choose a preparation rail before the final elite.";
            case "elite_battle":
            case "elite_room":
                return "Choose an approach rail and defeat the final elite to unlock extraction.";
            case "skirmish_battle":
            case "skirmish_room":
                return "Choose upper, middle, or lower rail to trigger the encounter.";
            case "start_room":
                return "Choose an opening rail and enter the expedition shell.";
            case "extraction_panel":
                return "Choose an extraction rail and seal the run.";
            default:
                return room != null ? "Choose a rail and continue the expedition shell." : "Dungeon shell inactive.";
        }
    }

    private string BuildPanelChoiceSummaryText(string stageKindKey, DungeonRoomTemplateData room)
    {
        if (!string.IsNullOrEmpty(CurrentSelectionPromptText) && CurrentSelectionPromptText != "None")
        {
            return CurrentSelectionPromptText;
        }

        switch (stageKindKey)
        {
            case "route_choice":
                return "Compare the steadier line against the greedier line, then commit the dispatch.";
            case "shrine_choice":
                return "Pick whether this shrine resets the party or gets stripped for payout.";
            case "pre_elite_choice":
                return "Make the last prep call: cleaner elite entry now or a larger bounty only if the elite falls.";
            case "result_panel":
                return "Review the settlement, then return to the world board.";
            case "extraction_panel":
                return "Choose the extraction line that settles the run cleanly.";
            case "start_room":
            case "skirmish_room":
            case "cache_room":
            case "shrine_room":
            case "preparation_room":
            case "elite_room":
                return "Pick [1] Upper, [2] Middle, or [3] Lower rail to advance this room.";
            default:
                return room != null ? room.RoomTypeLabel + " pending." : "None";
        }
    }

    private string BuildPanelRiskPreviewText(string stageKindKey, DungeonRoomTemplateData room)
    {
        if (stageKindKey == "route_choice")
        {
            string routeId = ResolveExpeditionContextRouteId();
            return !string.IsNullOrEmpty(routeId) ? BuildRoutePreviewSummaryText(_currentDungeonId, routeId) : "None";
        }

        if (stageKindKey == "skirmish_battle" || stageKindKey == "elite_battle")
        {
            return BuildCurrentBattleContextView().EnemyGroupSeedSummary;
        }

        if (room != null && room.RoomType == DungeonRoomType.Elite)
        {
            return GetEliteEncounterNameText() + " | " + GetEliteStatusText();
        }

        if (stageKindKey == "extraction_panel")
        {
            return BuildCurrentExtractionPressureSummaryText();
        }

        return GetCurrentDungeonDangerText();
    }

    private string BuildPanelRewardPreviewText(string stageKindKey, DungeonRoomTemplateData room)
    {
        if (stageKindKey == "route_choice")
        {
            return string.IsNullOrEmpty(_currentDungeonId) ? "None" : BuildRouteRewardPreviewText(_currentDungeonId);
        }

        if (room != null && room.RoomType == DungeonRoomType.Cache)
        {
            int chestReward = _activeChest != null ? _activeChest.RewardAmount : GetSelectedRouteTemplate().ChestRewardAmount;
            return "Cache Reward " + BuildLootAmountText(chestReward);
        }

        if (stageKindKey == "pre_elite_choice" || (room != null && room.RoomType == DungeonRoomType.Preparation))
        {
            string eliteLabel = GetEliteEncounterNameText();
            if (string.IsNullOrEmpty(eliteLabel) || eliteLabel == "None")
            {
                eliteLabel = "the elite";
            }

            return "Steady line: Recover " + BuildRawHpAmountText(GetCurrentPreEliteRecoverAmount()) +
                " each before " + eliteLabel +
                " | Greed line: Arm " + BuildLootAmountText(GetCurrentEliteBonusRewardAmount()) + " if " + eliteLabel + " falls";
        }

        if (stageKindKey == "elite_battle" || (room != null && room.RoomType == DungeonRoomType.Elite))
        {
            return _eliteRewardLabel + " " + BuildLootAmountText(_eliteRewardAmount) +
                (_eliteBonusRewardPending > 0 ? " | Bonus Pending " + BuildLootAmountText(_eliteBonusRewardPending) : string.Empty);
        }

        if (stageKindKey == "extraction_panel")
        {
            return "Secure Carry " + BuildLootAmountText(_carriedLootAmount) + " | " + BuildLootBreakdownSummary();
        }

        return BuildLootBreakdownSummary();
    }

    private string BuildPanelEventPreviewText(string stageKindKey, DungeonRoomTemplateData room)
    {
        if (stageKindKey == "route_choice")
        {
            return string.IsNullOrEmpty(_currentDungeonId) ? "None" : BuildRouteEventPreviewText(_currentDungeonId);
        }

        if (stageKindKey == "shrine_choice" || (room != null && room.RoomType == DungeonRoomType.Shrine))
        {
            return GetCurrentEventTitleText() + " | Recover " + BuildRawHpAmountText(GetCurrentShrineRecoverAmount()) +
                " each or take " + BuildLootAmountText(GetCurrentShrineBonusLootAmount()) + " | " + GetEventDescriptionText();
        }

        if (stageKindKey == "pre_elite_choice" || (room != null && room.RoomType == DungeonRoomType.Preparation))
        {
            return GetCurrentPreEliteTitleText() + " | Recover " + BuildRawHpAmountText(GetCurrentPreEliteRecoverAmount()) +
                " each or arm " + BuildLootAmountText(GetCurrentEliteBonusRewardAmount()) + " on elite victory";
        }

        if (stageKindKey == "extraction_panel")
        {
            return BuildCurrentExtractionSummaryText();
        }

        return GetSelectedEventChoiceDisplayText();
    }

    private string BuildRouteOptionOutcomeHintText(string dungeonId, string routeId)
    {
        DungeonRouteTemplate template = GetRouteTemplateById(dungeonId, routeId);
        return template == null ? "None" : template.Description;
    }

    private string BuildRouteOptionRiskHintText(string dungeonId, string routeId)
    {
        DungeonRouteTemplate template = GetRouteTemplateById(dungeonId, routeId);
        return template == null ? "None" : template.RiskLabel + " risk | " + template.EventFocus;
    }

    private string BuildRouteOptionRewardHintText(string dungeonId, string routeId)
    {
        DungeonRouteTemplate template = GetRouteTemplateById(dungeonId, routeId);
        if (template == null)
        {
            return "None";
        }

        int totalReward = template.BattleLootAmount + template.ChestRewardAmount + template.BonusLootAmount;
        return template.RewardPreview + " | Total " + BuildLootAmountText(totalReward);
    }

    private string BuildShrineChoiceOutcomeHintText(string choiceId)
    {
        if (choiceId == "recover")
        {
            if (_currentDungeonId == "dungeon-beta")
            {
                return _selectedRouteId == RiskyRouteId
                    ? "Take the thin war-banner patch-up and reduce the chief's opening spike."
                    : "Use the watchfire to reset before the guarded captain approach.";
            }

            return _selectedRouteId == RiskyRouteId
                ? "Let the unstable shrine steady the party before the mixed route snaps again."
                : "Take the calm rest line and walk back into the slime route steadier.";
        }

        if (choiceId == "loot")
        {
            if (_currentDungeonId == "dungeon-beta")
            {
                return _selectedRouteId == RiskyRouteId
                    ? "Rip down the banner cache and leave the shrine with stolen raider bounty."
                    : "Strip the watchfire cache and trade some captain control for more shards.";
            }

            return _selectedRouteId == RiskyRouteId
                ? "Drain the unstable reservoir and keep the mixed route dangerous but richer."
                : "Break the rest shrine cache and leave the mire rooms harsher but richer.";
        }

        return "None";
    }

    private string BuildShrineChoiceRiskHintText(string choiceId)
    {
        if (choiceId == "recover")
        {
            if (_currentDungeonId == "dungeon-beta")
            {
                return _selectedRouteId == RiskyRouteId
                    ? "Smaller raid haul | chief entry stays more stable."
                    : "Smaller cache pull | captain entry stays ordered.";
            }

            return _selectedRouteId == RiskyRouteId
                ? "Shard reservoir skipped | mixed-pressure rooms stay manageable."
                : "Lower payout | cleanest way to keep the slime route under control.";
        }

        if (choiceId == "loot")
        {
            if (_currentDungeonId == "dungeon-beta")
            {
                return _selectedRouteId == RiskyRouteId
                    ? "No real reset | raider burst stays live into the chief."
                    : "Captain entry gets rougher | better payout now.";
            }

            return _selectedRouteId == RiskyRouteId
                ? "Mixed route stays bruising | more shards immediately."
                : "Rest line weakened | next slime rooms stay harsher.";
        }

        return "None";
    }

    private string BuildShrineChoiceRewardHintText(string choiceId)
    {
        return choiceId == "recover"
            ? "Recover " + BuildRawHpAmountText(GetCurrentShrineRecoverAmount()) + " to each living ally"
            : choiceId == "loot"
                ? "Take " + BuildLootAmountText(GetCurrentShrineBonusLootAmount()) + " and keep current wounds"
                : "None";
    }

    private string BuildCurrentEliteReferenceText()
    {
        string eliteLabel = GetEliteEncounterNameText();
        return string.IsNullOrEmpty(eliteLabel) || eliteLabel == "None" ? "the elite" : eliteLabel;
    }

    private string BuildPreEliteChoiceOutcomeHintText(string choiceId)
    {
        string eliteLabel = BuildCurrentEliteReferenceText();
        if (choiceId == "recover")
        {
            if (_currentDungeonId == "dungeon-beta")
            {
                return _selectedRouteId == RiskyRouteId
                    ? "Use the war table patch-up and give " + eliteLabel + " less room to snowball."
                    : "Take the guard muster and enter " + eliteLabel + " with steadier reserves.";
            }

            return _selectedRouteId == RiskyRouteId
                ? "Stabilize at the core threshold before " + eliteLabel + " spikes."
                : "Reset in the antechamber and meet " + eliteLabel + " on cleaner footing.";
        }

        if (choiceId == "bonus")
        {
            if (_currentDungeonId == "dungeon-beta")
            {
                return _selectedRouteId == RiskyRouteId
                    ? "Ignore the patch-up and bet the entire chief fight on the larger bounty."
                    : "Skip the muster, keep the party nicked, and chase captain spoils.";
            }

            return _selectedRouteId == RiskyRouteId
                ? "Carry the damage into the core and only get paid if the elite breaks first."
                : "Refuse the calm reset and squeeze a richer monarch finish out of the run.";
        }

        return "None";
    }

    private string BuildPreEliteChoiceRiskHintText(string choiceId)
    {
        string eliteLabel = BuildCurrentEliteReferenceText();
        if (choiceId == "recover")
        {
            if (_currentDungeonId == "dungeon-beta")
            {
                return _selectedRouteId == RiskyRouteId
                    ? "Bonus line lost | cleanest chief entry available."
                    : "Bonus line lost | captain opener stays controlled.";
            }

            return _selectedRouteId == RiskyRouteId
                ? "Bonus line lost | dulls the core spike before it blooms."
                : "Bonus line lost | safest monarch approach in the run.";
        }

        if (choiceId == "bonus")
        {
            if (_currentDungeonId == "dungeon-beta")
            {
                return _selectedRouteId == RiskyRouteId
                    ? "No pre-elite heal | the chief decides whether this greed line pays."
                    : "No pre-elite heal | the captain opener gets rougher.";
            }

            return _selectedRouteId == RiskyRouteId
                ? "No pre-elite heal | the core spike decides whether the wager pays."
                : "No pre-elite heal | " + eliteLabel + " opens less forgiving.";
        }

        return "None";
    }

    private string BuildPreEliteChoiceRewardHintText(string choiceId)
    {
        string eliteLabel = BuildCurrentEliteReferenceText();
        return choiceId == "recover"
            ? "Recover " + BuildRawHpAmountText(GetCurrentPreEliteRecoverAmount()) + " to each living ally before " + eliteLabel
            : choiceId == "bonus"
                ? "Arm bonus " + BuildLootAmountText(GetCurrentEliteBonusRewardAmount()) + " if " + eliteLabel + " falls"
                : "None";
    }

    private PrototypeDungeonPanelOption[] BuildCurrentDungeonPanelOptions(string stageKindKey, DungeonRoomTemplateData room)
    {
        string panelId = room != null && !string.IsNullOrEmpty(room.RoomId) ? room.RoomId : stageKindKey;
        switch (stageKindKey)
        {
            case "route_choice":
                return new[]
                {
                    BuildDungeonPanelOption(SafeRouteId, BuildRouteButtonLabel(_currentDungeonId, SafeRouteId), "route", "start_room", BuildRouteOptionOutcomeHintText(_currentDungeonId, SafeRouteId), BuildRouteOptionRiskHintText(_currentDungeonId, SafeRouteId), BuildRouteOptionRewardHintText(_currentDungeonId, SafeRouteId), _selectedRouteChoiceId == SafeRouteId, _recommendedRouteId == SafeRouteId, false),
                    BuildDungeonPanelOption(RiskyRouteId, BuildRouteButtonLabel(_currentDungeonId, RiskyRouteId), "route", "start_room", BuildRouteOptionOutcomeHintText(_currentDungeonId, RiskyRouteId), BuildRouteOptionRiskHintText(_currentDungeonId, RiskyRouteId), BuildRouteOptionRewardHintText(_currentDungeonId, RiskyRouteId), _selectedRouteChoiceId == RiskyRouteId, _recommendedRouteId == RiskyRouteId, false)
                };
            case "start_room":
                return new[]
                {
                    BuildDungeonPanelLaneOption(panelId + ":upper", "Scout Upper Gate", "lane_progress", "explore", "Push through the upper rail and reveal more of the route shell.", "Discovery +1 | Lowest shell strain.", GetRoomProgressText(), 0, false),
                    BuildDungeonPanelLaneOption(panelId + ":middle", "Hold Center Rail", "lane_progress", "explore", "Take the center rail and steady the expedition start.", "Extraction +1 | Balanced entry.", GetRoomProgressText(), 1, false),
                    BuildDungeonPanelLaneOption(panelId + ":lower", "Break Lower Breach", "lane_progress", "explore", "Force the lower rail and enter under sharper pressure.", "Threat +1 | Aggressive opening.", GetRoomProgressText(), 2, false)
                };
            case "skirmish_room":
                DungeonEncounterRuntimeData encounter = room != null ? GetEncounterById(room.EncounterId) : null;
                bool encounterBlocked = encounter == null || encounter.IsCleared;
                string encounterLabel = encounter != null && !string.IsNullOrEmpty(encounter.DisplayName) ? encounter.DisplayName : "Encounter";
                string battleReward = BuildLootAmountText(GetSelectedRouteTemplate().BattleLootAmount);
                return new[]
                {
                    BuildDungeonPanelLaneOption(panelId + ":upper", "Scout Upper Breach", "encounter_request", "skirmish_battle", "Approach " + encounterLabel + " from the upper rail and trigger battle.", "Discovery +1 | Safer read on the fight.", battleReward, 0, encounterBlocked),
                    BuildDungeonPanelLaneOption(panelId + ":middle", "Take Center Rail", "encounter_request", "skirmish_battle", "Enter " + encounterLabel + " from the center rail.", "Balanced pressure.", battleReward, 1, encounterBlocked),
                    BuildDungeonPanelLaneOption(panelId + ":lower", "Crash Lower Breach", "encounter_request", "skirmish_battle", "Force the lower rail and start the fight hot.", "Threat +1 | HP chip on entry.", battleReward, 2, encounterBlocked)
                };
            case "cache_room":
                int cacheReward = _activeChest != null ? _activeChest.RewardAmount : GetSelectedRouteTemplate().ChestRewardAmount;
                bool cacheBlocked = _activeChest == null || _activeChest.IsOpened;
                return new[]
                {
                    BuildDungeonPanelLaneOption(panelId + ":upper", "Check Side Lockers", "cache_claim", "explore", "Secure the cache through the upper rail.", "Discovery +1 | Safer claim.", BuildLootAmountText(cacheReward), 0, cacheBlocked),
                    BuildDungeonPanelLaneOption(panelId + ":middle", "Secure Main Cache", "cache_claim", "explore", "Open the main cache and keep moving.", "Balanced claim.", BuildLootAmountText(cacheReward), 1, cacheBlocked),
                    BuildDungeonPanelLaneOption(panelId + ":lower", "Pry Hidden Crate", "cache_claim", "explore", "Force the lower crate for a sharper payout.", "Threat +1 | HP chip possible.", BuildLootAmountText(cacheReward + 1), 2, cacheBlocked)
                };
            case "shrine_room":
                return new[]
                {
                    BuildDungeonPanelLaneOption(panelId + ":upper", "Read Upper Sigils", "event_gate", "shrine_choice", "Approach the shrine from the upper rail.", "Discovery +1 | Opens shrine choice.", BuildPanelEventPreviewText("shrine_choice", room), 0, _eventResolved),
                    BuildDungeonPanelLaneOption(panelId + ":middle", "Kneel at Center Shrine", "event_gate", "shrine_choice", "Take the center line into the shrine.", "Balanced approach.", BuildPanelEventPreviewText("shrine_choice", room), 1, _eventResolved),
                    BuildDungeonPanelLaneOption(panelId + ":lower", "Touch Lower Reservoir", "event_gate", "shrine_choice", "Risk the lower rail to open the shrine.", "Threat +1 | No immediate recovery.", BuildPanelEventPreviewText("shrine_choice", room), 2, _eventResolved)
                };
            case "shrine_choice":
                return new[]
                {
                    BuildDungeonPanelOption("recover", GetCurrentEventOptionAText(), "event_choice", "explore", BuildShrineChoiceOutcomeHintText("recover"), BuildShrineChoiceRiskHintText("recover"), BuildShrineChoiceRewardHintText("recover"), IsEventChoiceSelected("recover"), false, !IsEventChoiceAvailable("recover")),
                    BuildDungeonPanelOption("loot", GetCurrentEventOptionBText(), "event_choice", "explore", BuildShrineChoiceOutcomeHintText("loot"), BuildShrineChoiceRiskHintText("loot"), BuildShrineChoiceRewardHintText("loot"), IsEventChoiceSelected("loot"), false, !IsEventChoiceAvailable("loot"))
                };
            case "preparation_room":
                return new[]
                {
                    BuildDungeonPanelLaneOption(panelId + ":upper", "Scout Elite Flank", "pre_elite_gate", "pre_elite_choice", "Circle through the upper rail and open preparation.", "Discovery +1 | Safer read before the elite.", BuildPanelRewardPreviewText("pre_elite_choice", room), 0, _preEliteDecisionResolved),
                    BuildDungeonPanelLaneOption(panelId + ":middle", "Hold Preparation Line", "pre_elite_gate", "pre_elite_choice", "Take the center line into the final preparation.", "Balanced preparation.", BuildPanelRewardPreviewText("pre_elite_choice", room), 1, _preEliteDecisionResolved),
                    BuildDungeonPanelLaneOption(panelId + ":lower", "Press Core Passage", "pre_elite_gate", "pre_elite_choice", "Force the lower rail before the elite.", "Threat +1 | HP chip possible.", BuildPanelRewardPreviewText("pre_elite_choice", room), 2, _preEliteDecisionResolved)
                };
            case "pre_elite_choice":
                return new[]
                {
                    BuildDungeonPanelOption("recover", GetCurrentPreEliteOptionAText(), "pre_elite_choice", "explore", BuildPreEliteChoiceOutcomeHintText("recover"), BuildPreEliteChoiceRiskHintText("recover"), BuildPreEliteChoiceRewardHintText("recover"), IsPreEliteChoiceSelected("recover"), false, !IsPreEliteChoiceAvailable("recover")),
                    BuildDungeonPanelOption("bonus", GetCurrentPreEliteOptionBText(), "pre_elite_choice", "explore", BuildPreEliteChoiceOutcomeHintText("bonus"), BuildPreEliteChoiceRiskHintText("bonus"), BuildPreEliteChoiceRewardHintText("bonus"), IsPreEliteChoiceSelected("bonus"), false, !IsPreEliteChoiceAvailable("bonus"))
                };
            case "elite_room":
                bool eliteBlocked = !_preEliteDecisionResolved || _eliteDefeated;
                string eliteRewardText = BuildPanelRewardPreviewText("elite_battle", room);
                return new[]
                {
                    BuildDungeonPanelLaneOption(panelId + ":upper", "Probe Upper Flank", "elite_request", "elite_battle", "Approach the elite from the upper rail.", "Discovery +1 | Lower entry strain.", eliteRewardText, 0, eliteBlocked),
                    BuildDungeonPanelLaneOption(panelId + ":middle", "Take Center Challenge", "elite_request", "elite_battle", "Take the center rail into the elite chamber.", "Balanced elite entry.", eliteRewardText, 1, eliteBlocked),
                    BuildDungeonPanelLaneOption(panelId + ":lower", "Crash Lower Breach", "elite_request", "elite_battle", "Force the lower rail and trigger the elite under pressure.", "Threat +1 | HP chip on entry.", eliteRewardText, 2, eliteBlocked)
                };
            case "extraction_panel":
                bool extractionBlocked = !_exitUnlocked || !_eliteDefeated;
                string extractionRewardText = "Secure Carry " + BuildLootAmountText(_carriedLootAmount);
                return new[]
                {
                    BuildDungeonPanelLaneOption("extract:upper", "Secure Upper Exit", "extraction_route", "result_panel", "Lock the upper extraction line and leave the ruin.", "Discovery +1 | Controlled extraction.", extractionRewardText, 0, extractionBlocked),
                    BuildDungeonPanelLaneOption("extract:middle", "Take Center Exit", "extraction_route", "result_panel", "Take the central extraction rail out.", "Balanced extraction.", extractionRewardText, 1, extractionBlocked),
                    BuildDungeonPanelLaneOption("extract:lower", "Sweep Lower Exit", "extraction_route", "result_panel", "Force the lower extraction line and seal the run fast.", "Threat +1 | Highest pressure line.", extractionRewardText, 2, extractionBlocked)
                };
            case "result_panel":
                return new[]
                {
                    BuildDungeonPanelOption("return_world", "Return to World", "result_exit", "world_return", "Absorb the run result and leave the dungeon shell.", BuildCurrentExtractionSummaryText(), BuildLootBreakdownSummary(), false, false, false)
                };
            default:
                return Array.Empty<PrototypeDungeonPanelOption>();
        }
    }
    private PrototypeDungeonPanelOption BuildDungeonPanelLaneOption(string optionId, string optionLabel, string optionKindKey, string transitionStageKindKey, string outcomeHintText, string riskHintText, string rewardHintText, int laneIndex, bool isBlocked)
    {
        string laneLabel = laneIndex >= 0 && laneIndex < ExploreLaneLabels.Length
            ? ExploreLaneLabels[laneIndex]
            : "Lane";
        return BuildDungeonPanelOption(optionId, optionLabel, optionKindKey, transitionStageKindKey, outcomeHintText, riskHintText, rewardHintText, _selectedPanelLaneOptionId == optionId, false, isBlocked, laneIndex, laneLabel);
    }

    private PrototypeDungeonPanelOption BuildDungeonPanelOption(string optionId, string optionLabel, string optionKindKey, string transitionStageKindKey, string outcomeHintText, string riskHintText, string rewardHintText, bool isSelected, bool isRecommended, bool isBlocked, int laneIndex = -1, string laneLabel = "None")
    {
        return new PrototypeDungeonPanelOption
        {
            OptionId = string.IsNullOrEmpty(optionId) ? string.Empty : optionId,
            OptionLabel = string.IsNullOrEmpty(optionLabel) ? "None" : optionLabel,
            OptionKindKey = string.IsNullOrEmpty(optionKindKey) ? string.Empty : optionKindKey,
            TransitionStageKindKey = string.IsNullOrEmpty(transitionStageKindKey) ? string.Empty : transitionStageKindKey,
            OutcomeHintText = string.IsNullOrEmpty(outcomeHintText) ? "None" : outcomeHintText,
            RiskHintText = string.IsNullOrEmpty(riskHintText) ? "None" : riskHintText,
            RewardHintText = string.IsNullOrEmpty(rewardHintText) ? "None" : rewardHintText,
            IsSelected = isSelected,
            IsRecommended = isRecommended,
            IsBlocked = isBlocked,
            LaneIndex = laneIndex,
            LaneLabel = string.IsNullOrEmpty(laneLabel) ? "None" : laneLabel
        };
    }

    private string BuildLatestChoiceOutcomeSummaryText()
    {
        PrototypeDungeonChoiceOutcome outcome = _latestDungeonChoiceOutcome;
        if (outcome == null)
        {
            return "None";
        }

        string summary = !string.IsNullOrEmpty(outcome.SummaryText) ? outcome.SummaryText : "None";
        string delta = !string.IsNullOrEmpty(outcome.DeltaSummaryText) && outcome.DeltaSummaryText != "None"
            ? outcome.DeltaSummaryText
            : BuildChoiceOutcomeDeltaSummaryText(outcome);
        if (string.IsNullOrEmpty(delta) || delta == "None")
        {
            return summary;
        }

        return summary == "None" ? delta : summary + " | " + delta;
    }

    private string BuildCurrentDungeonEventResolutionSummaryText()
    {
        if (_latestDungeonEventResolution != null && !string.IsNullOrEmpty(_latestDungeonEventResolution.LoopSummaryText))
        {
            return _latestDungeonEventResolution.LoopSummaryText;
        }

        string shrineText = _eventResolved ? GetSelectedEventChoiceDisplayText() : "Shrine pending";
        string preparationText = _preEliteDecisionResolved ? GetSelectedPreEliteChoiceDisplayText() : "Preparation pending";
        return shrineText + " | " + preparationText;
    }

    private string BuildCurrentExtractionSummaryText()
    {
        string summary = _latestDungeonExtractionResult != null && !string.IsNullOrEmpty(_latestDungeonExtractionResult.SummaryText)
            ? _latestDungeonExtractionResult.SummaryText
            : _runResultState == RunResultState.Clear
                ? "Extraction secured."
                : _runResultState == RunResultState.Defeat
                    ? "Extraction failed."
                    : _runResultState == RunResultState.Retreat
                        ? "Extraction abandoned."
                        : _exitUnlocked
                            ? "Extraction available."
                            : "Extraction pending.";
        string pressure = _latestDungeonExtractionResult != null && !string.IsNullOrEmpty(_latestDungeonExtractionResult.PressureSummaryText)
            ? _latestDungeonExtractionResult.PressureSummaryText
            : BuildCurrentExtractionPressureSummaryText();
        if (string.IsNullOrEmpty(pressure) || pressure == "None")
        {
            return summary;
        }

        return summary + " | " + pressure;
    }

    private string BuildChoiceOutcomeDeltaSummaryText(PrototypeDungeonChoiceOutcome outcome)
    {
        if (outcome == null)
        {
            return "None";
        }

        switch (outcome.OutcomeKindKey)
        {
            case "dispatch_route":
            {
                DungeonRouteTemplate template = GetRouteTemplateById(_currentDungeonId, outcome.SelectedOptionId);
                return template == null
                    ? "Route slope set | Threat " + _selectedRouteRiskLabel
                    : template.RouteLabel + " locked | Shrine read: " + template.EventFocus + " | Elite line: " + template.RewardPreview;
            }
            case "event_resolution":
                return outcome.SelectedOptionId == "recover"
                    ? GetCurrentEventTitleText() + " steadied the route | Recover " + BuildRawHpAmountText(_totalEventHealAmount) + " each | Loot skipped"
                    : GetCurrentEventTitleText() + " stripped for payout | Loot +" + BuildLootAmountText(_eventLootAmount) + " | Wounds carried forward";
            case "preparation_resolution":
                return outcome.SelectedOptionId == "recover"
                    ? GetCurrentPreEliteTitleText() + " secured a steadier entry | Recover " + BuildRawHpAmountText(_preEliteHealAmount) + " each | Bonus line dropped"
                    : GetCurrentPreEliteTitleText() + " armed the bounty line | Bonus " + BuildLootAmountText(_eliteBonusRewardPending) + " pending | No pre-elite reset";
            case "cache_claim":
                return "Loot +" + BuildLootAmountText(_chestLootAmount) + " | Threat flat | Route continues";
            case "encounter_request":
                return "Battle engaged | Time pressure rising | Threat active";
            case "elite_request":
                return "Final threat engaged | Extraction locked to elite result";
            case "extraction_resolution":
                return BuildCurrentExtractionPressureSummaryText();
            default:
                return "None";
        }
    }

    private string BuildResolvedExtractionPressureSummaryText(string extractionKey, int returnedLootAmount, bool exitUnlocked, bool eliteDefeated, bool success)
    {
        string gateText = exitUnlocked ? "Exit open" : eliteDefeated ? "Exit unlocking" : "Exit sealed";
        string carryText = success ? BuildLootAmountText(returnedLootAmount) : "None";
        string pressureText = success
            ? "Pressure released"
            : extractionKey == PrototypeBattleOutcomeKeys.RunRetreat
                ? "Partial extraction"
                : extractionKey == PrototypeBattleOutcomeKeys.RunDefeat
                    ? "Pressure collapsed"
                    : "Pressure unresolved";
        return pressureText + " | " + gateText + " | Returned " + carryText;
    }

    private string BuildRunResultTimeCostSummary(PrototypeRpgRunResultSnapshot snapshot)
    {
        int totalRooms = _plannedRooms != null ? _plannedRooms.Count : 0;
        int battlesFought = Mathf.Max(Mathf.Max(0, _battlesFoughtCount), _clearedEncounterCount + (_eliteDefeated ? 1 : 0));
        return "Turns " + Mathf.Max(0, snapshot.TotalTurnsTaken) + " | Rooms " + GetResolvedPlannedRoomCount() + "/" + totalRooms + " | Battles " + battlesFought;
    }

    private string BuildRunResultSupplyPressureSummary(PrototypeRpgPartyOutcomeSnapshot partyOutcome)
    {
        string hpSummary = string.IsNullOrEmpty(partyOutcome.PartyHpSummaryText) ? "None" : partyOutcome.PartyHpSummaryText;
        string condition = string.IsNullOrEmpty(partyOutcome.PartyConditionText) ? "None" : partyOutcome.PartyConditionText;
        return "End reserves | HP " + hpSummary + " | Condition " + condition + " | KO " + Mathf.Max(0, partyOutcome.KnockedOutMemberCount);
    }

    private string BuildRunResultExtractionPressureSummary(PrototypeRpgRunResultSnapshot snapshot, PrototypeRpgLootOutcomeSnapshot lootOutcome)
    {
        string extractionState = snapshot.ResultStateKey == PrototypeBattleOutcomeKeys.RunClear
            ? "Extraction secured"
            : snapshot.ResultStateKey == PrototypeBattleOutcomeKeys.RunRetreat
                ? "Extraction partial"
                : snapshot.ResultStateKey == PrototypeBattleOutcomeKeys.RunDefeat
                    ? "Extraction failed"
                    : "Extraction unresolved";
        return extractionState + " | Returned " + BuildLootAmountText(lootOutcome.TotalLootGained) + " | " + (_exitUnlocked ? "Exit open" : "Exit closed");
    }

    private string BuildPanelResolutionChainSummaryText(string choiceOutcomeSummaryText, string eventResolutionSummaryText, string extractionSummaryText)
    {
        string summary = string.Empty;

        if (!string.IsNullOrEmpty(choiceOutcomeSummaryText) && choiceOutcomeSummaryText != "None")
        {
            summary = choiceOutcomeSummaryText;
        }

        if (!string.IsNullOrEmpty(eventResolutionSummaryText) && eventResolutionSummaryText != "None")
        {
            summary = string.IsNullOrEmpty(summary) ? eventResolutionSummaryText : summary + " | " + eventResolutionSummaryText;
        }

        if (!string.IsNullOrEmpty(extractionSummaryText) && extractionSummaryText != "None")
        {
            summary = string.IsNullOrEmpty(summary) ? extractionSummaryText : summary + " | " + extractionSummaryText;
        }

        return string.IsNullOrEmpty(summary) ? "None" : summary;
    }
    private bool EvaluateCurrentPanelResolved(string stageKindKey, DungeonRoomTemplateData room)
    {
        if (stageKindKey == "result_panel")
        {
            return _runResultState != RunResultState.None;
        }

        if (stageKindKey == "start_room" || (room != null && room.RoomType == DungeonRoomType.Start))
        {
            return _startPanelResolved;
        }

        if (stageKindKey == "extraction_panel")
        {
            return _runResultState == RunResultState.Clear;
        }

        if (stageKindKey == "shrine_choice" || (room != null && room.RoomType == DungeonRoomType.Shrine))
        {
            return _eventResolved;
        }

        if (stageKindKey == "pre_elite_choice" || (room != null && room.RoomType == DungeonRoomType.Preparation))
        {
            return _preEliteDecisionResolved;
        }

        if (stageKindKey == "elite_battle" || (room != null && room.RoomType == DungeonRoomType.Elite))
        {
            return _eliteDefeated;
        }

        if (stageKindKey == "skirmish_battle" || (room != null && room.RoomType == DungeonRoomType.Skirmish))
        {
            DungeonEncounterRuntimeData encounter = room != null ? GetEncounterById(room.EncounterId) : GetActiveEncounter();
            return encounter != null && encounter.IsCleared;
        }

        if (room != null && room.RoomType == DungeonRoomType.Cache)
        {
            return _activeChest != null && _activeChest.IsOpened;
        }

        return false;
    }

    private bool EvaluateCurrentPanelBlocked(string stageKindKey, DungeonRoomTemplateData room)
    {
        if (stageKindKey == "route_choice")
        {
            return string.IsNullOrEmpty(ResolveExpeditionContextRouteId());
        }

        if (stageKindKey == "elite_room" || stageKindKey == "elite_battle" || (room != null && room.RoomType == DungeonRoomType.Elite))
        {
            return !_preEliteDecisionResolved;
        }

        if (stageKindKey == "extraction_panel")
        {
            return !_exitUnlocked || !_eliteDefeated;
        }

        return false;
    }

    // Compatibility shell only. BattleScene publishes BattleResult; downstream consumers own post-run derivations.
    private PrototypeDungeonRunResultContext BuildDungeonRunResultContext(PrototypeRpgRunResultSnapshot runResult)
    {
        PrototypeDungeonRunResultContext context = new PrototypeDungeonRunResultContext();
        PrototypeRpgRunResultSnapshot snapshot = CopyRpgRunResultSnapshot(runResult);
        PrototypeRpgPartyOutcomeSnapshot partyOutcome = snapshot.PartyOutcome ?? new PrototypeRpgPartyOutcomeSnapshot();
        PrototypeRpgLootOutcomeSnapshot lootOutcome = snapshot.LootOutcome ?? new PrototypeRpgLootOutcomeSnapshot();
        PrototypeRpgEliteOutcomeSnapshot eliteOutcome = snapshot.EliteOutcome ?? new PrototypeRpgEliteOutcomeSnapshot();
        PrototypeRpgEncounterOutcomeSnapshot encounterOutcome = snapshot.EncounterOutcome ?? new PrototypeRpgEncounterOutcomeSnapshot();
        BattleResult battleResult = BuildBattleResult();

        context.CanonicalRunResult = snapshot;
        context.WorldReturnAftermathPreview = BuildDungeonRunResultCityWritebackPreviewSurface();
        context.WorldOutcomeReadbackPreview = BuildDungeonRunResultOutcomeReadbackPreviewSurface();
        context.DungeonId = string.IsNullOrEmpty(snapshot.DungeonId) ? string.Empty : snapshot.DungeonId;
        context.DungeonLabel = string.IsNullOrEmpty(snapshot.DungeonLabel) ? "None" : snapshot.DungeonLabel;
        context.RouteId = string.IsNullOrEmpty(snapshot.RouteId) ? string.Empty : snapshot.RouteId;
        context.RouteLabel = string.IsNullOrEmpty(snapshot.RouteLabel) ? "None" : snapshot.RouteLabel;
        context.RunOutcomeKey = string.IsNullOrEmpty(snapshot.ResultStateKey) ? string.Empty : snapshot.ResultStateKey;
        context.ResultStateText = string.IsNullOrEmpty(ResultPanelStateText) ? "None" : ResultPanelStateText;
        context.CityDispatchedFromText = string.IsNullOrEmpty(ResultPanelCityDispatchedFromText) ? "None" : ResultPanelCityDispatchedFromText;
        context.DungeonDangerText = string.IsNullOrEmpty(ResultPanelDungeonDangerText) ? "None" : ResultPanelDungeonDangerText;
        context.RecommendedRouteText = string.IsNullOrEmpty(ResultPanelRecommendedRouteText) ? "None" : ResultPanelRecommendedRouteText;
        context.FollowedRecommendationText = string.IsNullOrEmpty(ResultPanelFollowedRecommendationText) ? "None" : ResultPanelFollowedRecommendationText;
        context.ManaShardsReturnedText = string.IsNullOrEmpty(ResultPanelManaShardsReturnedText) ? "None" : ResultPanelManaShardsReturnedText;
        context.StockBeforeText = string.IsNullOrEmpty(ResultPanelStockBeforeText) ? "None" : ResultPanelStockBeforeText;
        context.StockAfterText = string.IsNullOrEmpty(ResultPanelStockAfterText) ? "None" : ResultPanelStockAfterText;
        context.StockDeltaText = string.IsNullOrEmpty(ResultPanelStockDeltaText) ? "None" : ResultPanelStockDeltaText;
        context.NeedPressureBeforeText = string.IsNullOrEmpty(ResultPanelNeedPressureBeforeText) ? "None" : ResultPanelNeedPressureBeforeText;
        context.NeedPressureAfterText = string.IsNullOrEmpty(ResultPanelNeedPressureAfterText) ? "None" : ResultPanelNeedPressureAfterText;
        context.DispatchReadinessBeforeText = string.IsNullOrEmpty(ResultPanelDispatchReadinessBeforeText) ? "None" : ResultPanelDispatchReadinessBeforeText;
        context.DispatchReadinessAfterText = string.IsNullOrEmpty(ResultPanelDispatchReadinessAfterText) ? "None" : ResultPanelDispatchReadinessAfterText;
        context.RecoveryEtaText = string.IsNullOrEmpty(ResultPanelRecoveryEtaText) ? "None" : ResultPanelRecoveryEtaText;
        context.RouteRiskText = string.IsNullOrEmpty(ResultPanelRouteRiskText) ? "None" : ResultPanelRouteRiskText;
        context.TimeCostTurns = Mathf.Max(0, snapshot.TotalTurnsTaken);
        context.TimeCostSummaryText = BuildRunResultTimeCostSummary(snapshot);
        context.TurnsTakenText = string.IsNullOrEmpty(ResultPanelTurnsTakenText) ? "0" : ResultPanelTurnsTakenText;
        context.ResultSummaryText = string.IsNullOrEmpty(snapshot.ResultSummary) ? "None" : snapshot.ResultSummary;
        context.SurvivingMembersSummaryText = string.IsNullOrEmpty(snapshot.SurvivingMembersSummary) ? "None" : snapshot.SurvivingMembersSummary;
        context.ResourceDeltaSummaryText = string.IsNullOrEmpty(lootOutcome.FinalLootSummary) ? "None" : lootOutcome.FinalLootSummary;
        context.SupplyPressureSummaryText = BuildRunResultSupplyPressureSummary(partyOutcome);
        context.InjurySummaryText = string.IsNullOrEmpty(partyOutcome.PartyConditionText) ? "None" : partyOutcome.PartyConditionText;
        context.CasualtySummaryText = string.IsNullOrEmpty(partyOutcome.PartyMembersAtEndSummary) ? "None" : partyOutcome.PartyMembersAtEndSummary;
        context.DiscoveredFlagsSummaryText = BuildRunResultDiscoveredFlagsSummary(encounterOutcome, eliteOutcome);
        context.ThreatProgressSummaryText = BuildRunResultThreatProgressSummary(partyOutcome, eliteOutcome);
        context.ClearProgressSummaryText = BuildRunResultClearProgressSummary(encounterOutcome, eliteOutcome);
        context.ChoiceOutcomeSummaryText = BuildLatestChoiceOutcomeSummaryText();
        context.EventResolutionSummaryText = BuildCurrentDungeonEventResolutionSummaryText();
        context.ExtractionSummaryText = BuildCurrentExtractionSummaryText();
        context.ExtractionPressureSummaryText = BuildRunResultExtractionPressureSummary(snapshot, lootOutcome);
        context.EventLogSummaryText = BuildPanelResolutionChainSummaryText(context.ChoiceOutcomeSummaryText, context.EventResolutionSummaryText, context.ExtractionSummaryText);
        context.EncounterRequestSummaryText = string.IsNullOrEmpty(snapshot.EncounterRequestSummaryText) ? BuildCurrentEncounterRequestSummaryText() : snapshot.EncounterRequestSummaryText;
        context.BattleAbsorbSummaryText = battleResult != null && !string.IsNullOrEmpty(battleResult.AbsorbSummaryText)
            ? battleResult.AbsorbSummaryText
            : "None";
        context.BattlesFoughtCount = Mathf.Max(Mathf.Max(0, _battlesFoughtCount), encounterOutcome.ClearedEncounterCount + (eliteOutcome.IsEliteDefeated ? 1 : 0));
        context.KeyEncounterSummaryText = BuildRunResultKeyEncounterSummary(encounterOutcome, eliteOutcome);
        context.SelectedRouteSummaryText = BuildSelectedRouteSummary();
        context.FollowedRecommendationSummaryText = _followedRecommendation ? "Yes" : "No";
        context.BattleContextSummaryText = battleResult != null && !string.IsNullOrEmpty(battleResult.BattleContextSummaryText)
            ? battleResult.BattleContextSummaryText
            : "None";
        context.BattleRuntimeSummaryText = battleResult != null && !string.IsNullOrEmpty(battleResult.RuntimeSummaryText)
            ? battleResult.RuntimeSummaryText
            : "None";
        context.BattleRuleSummaryText = battleResult != null && !string.IsNullOrEmpty(battleResult.LaneRuleSummaryText)
            ? battleResult.LaneRuleSummaryText
            : "None";
        context.BattleResultCoreSummaryText = battleResult != null && !string.IsNullOrEmpty(battleResult.ResultSummaryText)
            ? battleResult.ResultSummaryText
            : "None";
        context.NotableBattleEventsSummaryText = battleResult != null && !string.IsNullOrEmpty(battleResult.NotableEventsSummaryText)
            ? battleResult.NotableEventsSummaryText
            : "None";
        context.GearRewardCandidateSummaryText = string.IsNullOrEmpty(snapshot.GearRewardCandidateSummaryText) ? "None" : snapshot.GearRewardCandidateSummaryText;
        context.EquipSwapChoiceSummaryText = string.IsNullOrEmpty(snapshot.EquipSwapChoiceSummaryText) ? "None" : snapshot.EquipSwapChoiceSummaryText;
        context.GearCarryContinuitySummaryText = string.IsNullOrEmpty(snapshot.GearCarryContinuitySummaryText) ? "None" : snapshot.GearCarryContinuitySummaryText;
        context.LootGainedText = string.IsNullOrEmpty(ResultPanelLootGainedText) ? "None" : ResultPanelLootGainedText;
        context.BattleLootText = string.IsNullOrEmpty(ResultPanelBattleLootText) ? "None" : ResultPanelBattleLootText;
        context.ChestLootText = string.IsNullOrEmpty(ResultPanelChestLootText) ? "None" : ResultPanelChestLootText;
        context.EventLootText = string.IsNullOrEmpty(ResultPanelEventLootText) ? "None" : ResultPanelEventLootText;
        context.EventChoiceText = string.IsNullOrEmpty(ResultPanelEventChoiceText) ? "None" : ResultPanelEventChoiceText;
        context.PreEliteChoiceText = string.IsNullOrEmpty(ResultPanelPreEliteChoiceText) ? "Pending" : ResultPanelPreEliteChoiceText;
        context.PreEliteHealAmountText = string.IsNullOrEmpty(ResultPanelPreEliteHealAmountText) ? "None" : ResultPanelPreEliteHealAmountText;
        context.EliteBonusRewardEarnedText = string.IsNullOrEmpty(ResultPanelEliteBonusRewardEarnedText) ? "None" : ResultPanelEliteBonusRewardEarnedText;
        context.EliteBonusRewardAmountText = string.IsNullOrEmpty(ResultPanelEliteBonusRewardAmountText) ? "None" : ResultPanelEliteBonusRewardAmountText;
        context.RoomPathSummaryText = string.IsNullOrEmpty(ResultPanelRoomPathSummaryText) ? "None" : ResultPanelRoomPathSummaryText;
        context.PartyHpSummaryText = string.IsNullOrEmpty(ResultPanelPartyHpSummaryText) ? "None" : ResultPanelPartyHpSummaryText;
        context.PartyConditionText = string.IsNullOrEmpty(ResultPanelPartyConditionText) ? "None" : ResultPanelPartyConditionText;
        context.ClearedEncountersText = string.IsNullOrEmpty(ResultPanelClearedEncountersText) ? "0 / 0" : ResultPanelClearedEncountersText;
        context.OpenedChestsText = string.IsNullOrEmpty(ResultPanelOpenedChestsText) ? "0 / 0" : ResultPanelOpenedChestsText;
        context.EliteDefeatedText = string.IsNullOrEmpty(ResultPanelEliteDefeatedText) ? "None" : ResultPanelEliteDefeatedText;
        context.EliteNameText = string.IsNullOrEmpty(ResultPanelEliteNameText) ? "None" : ResultPanelEliteNameText;
        context.EliteRewardIdentityText = string.IsNullOrEmpty(ResultPanelEliteRewardIdentityText) ? "None" : ResultPanelEliteRewardIdentityText;
        context.EliteRewardAmountText = string.IsNullOrEmpty(ResultPanelEliteRewardAmountText) ? "None" : ResultPanelEliteRewardAmountText;
        context.ReturnPromptText = string.IsNullOrEmpty(ResultPanelReturnPromptText) ? "None" : ResultPanelReturnPromptText;
        context.WorldWritebackResultSummaryText = IsMeaningfulSnapshotText(context.WorldOutcomeReadbackPreview.PostRunSummaryText)
            ? context.WorldOutcomeReadbackPreview.PostRunSummaryText
            : context.ResultSummaryText;
        context.WorldWritebackSurvivingMembersSummaryText = IsMeaningfulSnapshotText(context.WorldOutcomeReadbackPreview.SurvivingMembersSummaryText)
            ? context.WorldOutcomeReadbackPreview.SurvivingMembersSummaryText
            : context.SurvivingMembersSummaryText;
        context.WorldWritebackEncounterSummaryText = IsMeaningfulSnapshotText(context.WorldOutcomeReadbackPreview.ClearedEncountersSummaryText)
            ? context.WorldOutcomeReadbackPreview.ClearedEncountersSummaryText
            : string.IsNullOrEmpty(encounterOutcome.ClearedEncounterSummary) ? "None" : encounterOutcome.ClearedEncounterSummary;
        context.WorldWritebackChoiceSummaryText = IsMeaningfulSnapshotText(context.WorldOutcomeReadbackPreview.EventChoiceSummaryText)
            ? context.WorldOutcomeReadbackPreview.EventChoiceSummaryText
            : context.EventLogSummaryText;
        context.WorldWritebackLootSummaryText = IsMeaningfulSnapshotText(context.WorldReturnAftermathPreview.LootSummaryText)
            ? context.WorldReturnAftermathPreview.LootSummaryText
            : context.ResourceDeltaSummaryText;
        context.WorldWritebackRouteSummaryText = IsMeaningfulSnapshotText(context.WorldOutcomeReadbackPreview.RouteSummaryText)
            ? context.WorldOutcomeReadbackPreview.RouteSummaryText
            : context.SelectedRouteSummaryText;
        context.WorldWritebackDungeonSummaryText = IsMeaningfulSnapshotText(context.WorldOutcomeReadbackPreview.DungeonSummaryText)
            ? context.WorldOutcomeReadbackPreview.DungeonSummaryText
            : BuildCurrentDungeonWritebackSummaryText();
        return context;
    }
    private string BuildRunResultDiscoveredFlagsSummary(PrototypeRpgEncounterOutcomeSnapshot encounterOutcome, PrototypeRpgEliteOutcomeSnapshot eliteOutcome)
    {
        return BuildCurrentDiscoverySummaryText() + " | Exit " + (_exitUnlocked ? "Unlocked" : "Locked");
    }

    private string BuildRunResultThreatProgressSummary(PrototypeRpgPartyOutcomeSnapshot partyOutcome, PrototypeRpgEliteOutcomeSnapshot eliteOutcome)
    {
        string partyCondition = string.IsNullOrEmpty(partyOutcome.PartyConditionText) ? "None" : partyOutcome.PartyConditionText;
        return BuildCurrentThreatPressureSummaryText() + " | Party " + partyCondition;
    }

    private string BuildRunResultClearProgressSummary(PrototypeRpgEncounterOutcomeSnapshot encounterOutcome, PrototypeRpgEliteOutcomeSnapshot eliteOutcome)
    {
        int clearedEncounters = Mathf.Max(0, encounterOutcome.ClearedEncounterCount);
        int openedChests = Mathf.Max(0, encounterOutcome.OpenedChestCount);
        string eliteState = eliteOutcome.IsEliteDefeated ? "Yes" : "No";
        return "Encounters " + clearedEncounters + "/" + TotalEncounterCount +
            " | Chests " + openedChests + "/" + TotalChestCount +
            " | Elite " + eliteState;
    }

    private string BuildRunResultKeyEncounterSummary(PrototypeRpgEncounterOutcomeSnapshot encounterOutcome, PrototypeRpgEliteOutcomeSnapshot eliteOutcome)
    {
        string eliteLabel = !string.IsNullOrEmpty(eliteOutcome.EliteName) ? eliteOutcome.EliteName : "Elite";
        string encounterSummary = string.IsNullOrEmpty(encounterOutcome.ClearedEncounterSummary)
            ? "No encounter summary."
            : encounterOutcome.ClearedEncounterSummary;
        int battlesFought = Mathf.Max(Mathf.Max(0, _battlesFoughtCount), encounterOutcome.ClearedEncounterCount + (eliteOutcome.IsEliteDefeated ? 1 : 0));
        return "Battles " + battlesFought + " | " + eliteLabel + " | " + encounterSummary;
    }

    private void CaptureLatestDungeonEncounterRequest(PrototypeDungeonEncounterRequest request)
    {
        _latestDungeonEncounterRequest = CopyDungeonEncounterRequest(request);
    }

    private void CaptureLatestDungeonChoiceOutcome(PrototypeDungeonChoiceOutcome outcome)
    {
        _latestDungeonChoiceOutcome = CopyDungeonChoiceOutcome(outcome);
        if (_latestDungeonChoiceOutcome != null && (string.IsNullOrEmpty(_latestDungeonChoiceOutcome.DeltaSummaryText) || _latestDungeonChoiceOutcome.DeltaSummaryText == "None"))
        {
            _latestDungeonChoiceOutcome.DeltaSummaryText = BuildChoiceOutcomeDeltaSummaryText(_latestDungeonChoiceOutcome);
        }
    }

    private void CaptureLatestDungeonEventResolution(PrototypeDungeonEventResolution resolution)
    {
        _latestDungeonEventResolution = CopyDungeonEventResolution(resolution);
    }

    private void CaptureLatestDungeonExtractionResult(PrototypeDungeonExtractionResult result)
    {
        _latestDungeonExtractionResult = CopyDungeonExtractionResult(result);
    }

    private void CaptureShrineResolutionState(string choiceId, string choiceLabel, int healAmount, int lootAmount)
    {
        PrototypeDungeonEventResolution resolution = CopyDungeonEventResolution(_latestDungeonEventResolution);
        resolution.ShrineResolved = !string.IsNullOrEmpty(choiceId);
        resolution.ShrineChoiceId = string.IsNullOrEmpty(choiceId) ? string.Empty : choiceId;
        resolution.ShrineChoiceLabel = string.IsNullOrEmpty(choiceLabel) ? "None" : choiceLabel;
        resolution.ShrineHealAmount = Mathf.Max(0, healAmount);
        resolution.ShrineLootAmount = Mathf.Max(0, lootAmount);
        resolution.ShrineSummaryText = choiceId == "recover"
            ? GetCurrentEventTitleText() + ": blessing taken (" + BuildRawHpAmountText(healAmount) + " each)."
            : choiceId == "loot"
                ? GetCurrentEventTitleText() + ": cache stripped (" + BuildLootAmountText(lootAmount) + ")."
                : "Shrine pending";
        resolution.LoopSummaryText = BuildDungeonResolutionLoopSummaryText(resolution);
        CaptureLatestDungeonEventResolution(resolution);
    }

    private void CapturePreparationResolutionState(string choiceId, string choiceLabel, int healAmount, int bonusAmount)
    {
        PrototypeDungeonEventResolution resolution = CopyDungeonEventResolution(_latestDungeonEventResolution);
        resolution.PreparationResolved = !string.IsNullOrEmpty(choiceId);
        resolution.PreparationChoiceId = string.IsNullOrEmpty(choiceId) ? string.Empty : choiceId;
        resolution.PreparationChoiceLabel = string.IsNullOrEmpty(choiceLabel) ? "Pending" : choiceLabel;
        resolution.PreparationHealAmount = Mathf.Max(0, healAmount);
        resolution.PreparationBonusAmount = Mathf.Max(0, bonusAmount);
        resolution.PreparationBonusPending = choiceId == "bonus" && bonusAmount > 0;
        resolution.PreparationSummaryText = choiceId == "recover"
            ? GetCurrentPreEliteTitleText() + ": steady entry taken (" + BuildRawHpAmountText(healAmount) + " each)."
            : choiceId == "bonus"
                ? GetCurrentPreEliteTitleText() + ": bounty line armed (" + BuildLootAmountText(bonusAmount) + " pending)."
                : "Preparation pending";
        resolution.LoopSummaryText = BuildDungeonResolutionLoopSummaryText(resolution);
        CaptureLatestDungeonEventResolution(resolution);
    }

    private string BuildDungeonResolutionLoopSummaryText(PrototypeDungeonEventResolution resolution)
    {
        string shrineText = resolution != null && resolution.ShrineResolved ? (string.IsNullOrEmpty(resolution.ShrineSummaryText) ? "Shrine resolved" : resolution.ShrineSummaryText) : "Shrine pending";
        string preparationText = resolution != null && resolution.PreparationResolved ? (string.IsNullOrEmpty(resolution.PreparationSummaryText) ? "Preparation resolved" : resolution.PreparationSummaryText) : "Preparation pending";
        return shrineText + " | " + preparationText;
    }

    private void CaptureExtractionResultState(string extractionKey, int returnedLootAmount, bool exitUnlocked, bool eliteDefeated, bool success, string resultSummary)
    {
        string extractionLabel = extractionKey == PrototypeBattleOutcomeKeys.RunClear ? "Extraction secured" : extractionKey == PrototypeBattleOutcomeKeys.RunRetreat ? "Extraction abandoned" : extractionKey == PrototypeBattleOutcomeKeys.RunDefeat ? "Extraction failed" : "Extraction pending";
        CaptureLatestDungeonExtractionResult(new PrototypeDungeonExtractionResult
        {
            ExtractionKey = string.IsNullOrEmpty(extractionKey) ? string.Empty : extractionKey,
            ExtractionLabel = extractionLabel,
            ReturnedLootAmount = Mathf.Max(0, returnedLootAmount),
            ExitUnlocked = exitUnlocked,
            EliteDefeated = eliteDefeated,
            Success = success,
            ResultSummaryText = string.IsNullOrEmpty(resultSummary) ? "None" : resultSummary,
            SummaryText = extractionLabel + " | Returned " + BuildLootAmountText(returnedLootAmount) + " | Exit " + (exitUnlocked ? "Unlocked" : "Locked"),
            PressureSummaryText = BuildResolvedExtractionPressureSummaryText(extractionKey, returnedLootAmount, exitUnlocked, eliteDefeated, success)
        });
    }


    private ExpeditionStartContext CopyExpeditionStartContext(ExpeditionStartContext source)
    {
        ExpeditionStartContext copy = new ExpeditionStartContext();
        if (source == null)
        {
            return copy;
        }

        copy.StartCityId = string.IsNullOrEmpty(source.StartCityId) ? string.Empty : source.StartCityId;
        copy.StartCityLabel = string.IsNullOrEmpty(source.StartCityLabel) ? "None" : source.StartCityLabel;
        copy.DungeonId = string.IsNullOrEmpty(source.DungeonId) ? string.Empty : source.DungeonId;
        copy.DungeonLabel = string.IsNullOrEmpty(source.DungeonLabel) ? "None" : source.DungeonLabel;
        copy.PartyId = string.IsNullOrEmpty(source.PartyId) ? string.Empty : source.PartyId;
        copy.PartyLabel = string.IsNullOrEmpty(source.PartyLabel) ? "None" : source.PartyLabel;
        copy.PartyLoadoutSummaryText = string.IsNullOrEmpty(source.PartyLoadoutSummaryText) ? "None" : source.PartyLoadoutSummaryText;
        copy.StagedPartySummaryText = string.IsNullOrEmpty(source.StagedPartySummaryText) ? "None" : source.StagedPartySummaryText;
        copy.FollowedRecommendation = source.FollowedRecommendation;
        copy.LaunchManifestSummaryText = string.IsNullOrEmpty(source.LaunchManifestSummaryText) ? "None" : source.LaunchManifestSummaryText;
        copy.PartyManifest = CopyExpeditionPartyManifest(source.PartyManifest);
        copy.SelectedRouteId = string.IsNullOrEmpty(source.SelectedRouteId) ? string.Empty : source.SelectedRouteId;
        copy.SelectedRouteLabel = string.IsNullOrEmpty(source.SelectedRouteLabel) ? "None" : source.SelectedRouteLabel;
        copy.RecommendedRouteId = string.IsNullOrEmpty(source.RecommendedRouteId) ? string.Empty : source.RecommendedRouteId;
        copy.RecommendedRouteLabel = string.IsNullOrEmpty(source.RecommendedRouteLabel) ? "None" : source.RecommendedRouteLabel;
        copy.NeedPressureText = string.IsNullOrEmpty(source.NeedPressureText) ? "None" : source.NeedPressureText;
        copy.DispatchReadinessText = string.IsNullOrEmpty(source.DispatchReadinessText) ? "None" : source.DispatchReadinessText;
        copy.RecoveryProgressText = string.IsNullOrEmpty(source.RecoveryProgressText) ? "None" : source.RecoveryProgressText;
        copy.RecoveryEtaText = string.IsNullOrEmpty(source.RecoveryEtaText) ? "None" : source.RecoveryEtaText;
        copy.DispatchPolicyText = string.IsNullOrEmpty(source.DispatchPolicyText) ? "None" : source.DispatchPolicyText;
        copy.RecommendationReasonText = string.IsNullOrEmpty(source.RecommendationReasonText) ? "None" : source.RecommendationReasonText;
        copy.ExpectedNeedImpactText = string.IsNullOrEmpty(source.ExpectedNeedImpactText) ? "None" : source.ExpectedNeedImpactText;
        copy.RouteRiskLabel = string.IsNullOrEmpty(source.RouteRiskLabel) ? "None" : source.RouteRiskLabel;
        copy.DungeonDangerLabel = string.IsNullOrEmpty(source.DungeonDangerLabel) ? "None" : source.DungeonDangerLabel;
        copy.SupplySummaryText = string.IsNullOrEmpty(source.SupplySummaryText) ? "None" : source.SupplySummaryText;
        copy.SupplyPressureSummaryText = string.IsNullOrEmpty(source.SupplyPressureSummaryText) ? "None" : source.SupplyPressureSummaryText;
        copy.TimePressureSummaryText = string.IsNullOrEmpty(source.TimePressureSummaryText) ? "None" : source.TimePressureSummaryText;
        copy.ThreatPressureSummaryText = string.IsNullOrEmpty(source.ThreatPressureSummaryText) ? "None" : source.ThreatPressureSummaryText;
        copy.DiscoverySummaryText = string.IsNullOrEmpty(source.DiscoverySummaryText) ? "None" : source.DiscoverySummaryText;
        copy.ExtractionPressureSummaryText = string.IsNullOrEmpty(source.ExtractionPressureSummaryText) ? "None" : source.ExtractionPressureSummaryText;
        copy.WorldModifierSummaryText = string.IsNullOrEmpty(source.WorldModifierSummaryText) ? "None" : source.WorldModifierSummaryText;
        copy.RoutePreviewSummaryText = string.IsNullOrEmpty(source.RoutePreviewSummaryText) ? "None" : source.RoutePreviewSummaryText;
        copy.RewardPreviewSummaryText = string.IsNullOrEmpty(source.RewardPreviewSummaryText) ? "None" : source.RewardPreviewSummaryText;
        copy.EventPreviewSummaryText = string.IsNullOrEmpty(source.EventPreviewSummaryText) ? "None" : source.EventPreviewSummaryText;
        copy.PartySummaryText = string.IsNullOrEmpty(source.PartySummaryText) ? "None" : source.PartySummaryText;
        copy.BriefingSummaryText = string.IsNullOrEmpty(source.BriefingSummaryText) ? "None" : source.BriefingSummaryText;
        copy.RouteFitSummaryText = string.IsNullOrEmpty(source.RouteFitSummaryText) ? "None" : source.RouteFitSummaryText;
        copy.LaunchLockSummaryText = string.IsNullOrEmpty(source.LaunchLockSummaryText) ? "None" : source.LaunchLockSummaryText;
        copy.ProjectedOutcomeSummaryText = string.IsNullOrEmpty(source.ProjectedOutcomeSummaryText) ? "None" : source.ProjectedOutcomeSummaryText;
        copy.ContextSummaryText = string.IsNullOrEmpty(source.ContextSummaryText) ? "None" : source.ContextSummaryText;
        return copy;
    }

    private ExpeditionPartyManifest CopyExpeditionPartyManifest(ExpeditionPartyManifest source)
    {
        ExpeditionPartyManifest copy = new ExpeditionPartyManifest();
        if (source == null)
        {
            return copy;
        }

        copy.PartyId = string.IsNullOrEmpty(source.PartyId) ? string.Empty : source.PartyId;
        copy.PartyLabel = string.IsNullOrEmpty(source.PartyLabel) ? "None" : source.PartyLabel;
        copy.MemberCount = source.MemberCount;
        copy.LoadoutSummaryText = string.IsNullOrEmpty(source.LoadoutSummaryText) ? "None" : source.LoadoutSummaryText;
        copy.MemberSummaryText = string.IsNullOrEmpty(source.MemberSummaryText) ? "None" : source.MemberSummaryText;
        copy.AppliedProgressionSummaryText = string.IsNullOrEmpty(source.AppliedProgressionSummaryText) ? "None" : source.AppliedProgressionSummaryText;
        copy.CurrentRunSummaryText = string.IsNullOrEmpty(source.CurrentRunSummaryText) ? "None" : source.CurrentRunSummaryText;
        copy.NextRunPreviewSummaryText = string.IsNullOrEmpty(source.NextRunPreviewSummaryText) ? "None" : source.NextRunPreviewSummaryText;
        copy.ManifestSummaryText = string.IsNullOrEmpty(source.ManifestSummaryText) ? "None" : source.ManifestSummaryText;
        copy.Members = CopyExpeditionPartyMemberManifestArray(source.Members);
        return copy;
    }

    private ExpeditionPartyMemberManifest[] CopyExpeditionPartyMemberManifestArray(ExpeditionPartyMemberManifest[] source)
    {
        if (source == null || source.Length <= 0)
        {
            return new ExpeditionPartyMemberManifest[0];
        }

        ExpeditionPartyMemberManifest[] copy = new ExpeditionPartyMemberManifest[source.Length];
        for (int i = 0; i < source.Length; i++)
        {
            ExpeditionPartyMemberManifest member = source[i] ?? new ExpeditionPartyMemberManifest();
            copy[i] = new ExpeditionPartyMemberManifest
            {
                MemberId = string.IsNullOrEmpty(member.MemberId) ? string.Empty : member.MemberId,
                DisplayName = string.IsNullOrEmpty(member.DisplayName) ? "Adventurer" : member.DisplayName,
                RoleTag = string.IsNullOrEmpty(member.RoleTag) ? "adventurer" : member.RoleTag,
                RoleLabel = string.IsNullOrEmpty(member.RoleLabel) ? "Adventurer" : member.RoleLabel,
                PartySlotIndex = member.PartySlotIndex,
                GrowthTrackId = string.IsNullOrEmpty(member.GrowthTrackId) ? string.Empty : member.GrowthTrackId,
                JobId = string.IsNullOrEmpty(member.JobId) ? string.Empty : member.JobId,
                EquipmentLoadoutId = string.IsNullOrEmpty(member.EquipmentLoadoutId) ? string.Empty : member.EquipmentLoadoutId,
                SkillLoadoutId = string.IsNullOrEmpty(member.SkillLoadoutId) ? string.Empty : member.SkillLoadoutId,
                DefaultSkillId = string.IsNullOrEmpty(member.DefaultSkillId) ? string.Empty : member.DefaultSkillId,
                ResolvedSkillName = string.IsNullOrEmpty(member.ResolvedSkillName) ? "Skill" : member.ResolvedSkillName,
                ResolvedSkillShortText = string.IsNullOrEmpty(member.ResolvedSkillShortText) ? "None" : member.ResolvedSkillShortText,
                EquipmentSummaryText = string.IsNullOrEmpty(member.EquipmentSummaryText) ? "No gear" : member.EquipmentSummaryText,
                GearContributionSummaryText = string.IsNullOrEmpty(member.GearContributionSummaryText) ? "No bonus" : member.GearContributionSummaryText,
                AppliedProgressionSummaryText = string.IsNullOrEmpty(member.AppliedProgressionSummaryText) ? "None" : member.AppliedProgressionSummaryText,
                CurrentRunSummaryText = string.IsNullOrEmpty(member.CurrentRunSummaryText) ? "None" : member.CurrentRunSummaryText,
                NextRunPreviewSummaryText = string.IsNullOrEmpty(member.NextRunPreviewSummaryText) ? "None" : member.NextRunPreviewSummaryText,
                SummaryText = string.IsNullOrEmpty(member.SummaryText) ? "None" : member.SummaryText
            };
        }

        return copy;
    }

    private PrototypeDungeonPanelTemplate CopyDungeonPanelTemplate(PrototypeDungeonPanelTemplate source)
    {
        PrototypeDungeonPanelTemplate copy = new PrototypeDungeonPanelTemplate();
        if (source == null)
        {
            return copy;
        }

        copy.PanelId = string.IsNullOrEmpty(source.PanelId) ? string.Empty : source.PanelId;
        copy.StageKindKey = string.IsNullOrEmpty(source.StageKindKey) ? "none" : source.StageKindKey;
        copy.StageLabel = string.IsNullOrEmpty(source.StageLabel) ? "None" : source.StageLabel;
        copy.RoomTypeLabel = string.IsNullOrEmpty(source.RoomTypeLabel) ? "None" : source.RoomTypeLabel;
        copy.BiomeStyleText = string.IsNullOrEmpty(source.BiomeStyleText) ? "None" : source.BiomeStyleText;
        copy.AffordanceSummaryText = string.IsNullOrEmpty(source.AffordanceSummaryText) ? "None" : source.AffordanceSummaryText;
        copy.ChoiceSummaryText = string.IsNullOrEmpty(source.ChoiceSummaryText) ? "None" : source.ChoiceSummaryText;
        return copy;
    }

    private PrototypeDungeonPanelOption CopyDungeonPanelOption(PrototypeDungeonPanelOption source)
    {
        PrototypeDungeonPanelOption copy = new PrototypeDungeonPanelOption();
        if (source == null)
        {
            return copy;
        }

        copy.OptionId = string.IsNullOrEmpty(source.OptionId) ? string.Empty : source.OptionId;
        copy.OptionLabel = string.IsNullOrEmpty(source.OptionLabel) ? "None" : source.OptionLabel;
        copy.OptionKindKey = string.IsNullOrEmpty(source.OptionKindKey) ? string.Empty : source.OptionKindKey;
        copy.TransitionStageKindKey = string.IsNullOrEmpty(source.TransitionStageKindKey) ? string.Empty : source.TransitionStageKindKey;
        copy.OutcomeHintText = string.IsNullOrEmpty(source.OutcomeHintText) ? "None" : source.OutcomeHintText;
        copy.RiskHintText = string.IsNullOrEmpty(source.RiskHintText) ? "None" : source.RiskHintText;
        copy.RewardHintText = string.IsNullOrEmpty(source.RewardHintText) ? "None" : source.RewardHintText;
        copy.IsSelected = source.IsSelected;
        copy.IsRecommended = source.IsRecommended;
        copy.IsBlocked = source.IsBlocked;
        copy.LaneIndex = source.LaneIndex;
        copy.LaneLabel = string.IsNullOrEmpty(source.LaneLabel) ? "None" : source.LaneLabel;
        return copy;
    }

    private PrototypeDungeonPanelOption[] CopyDungeonPanelOptions(PrototypeDungeonPanelOption[] source)
    {
        if (source == null || source.Length == 0)
        {
            return Array.Empty<PrototypeDungeonPanelOption>();
        }

        PrototypeDungeonPanelOption[] copy = new PrototypeDungeonPanelOption[source.Length];
        for (int i = 0; i < source.Length; i++)
        {
            copy[i] = CopyDungeonPanelOption(source[i]);
        }

        return copy;
    }

    private PrototypeDungeonEncounterRequest CopyDungeonEncounterRequest(PrototypeDungeonEncounterRequest source)
    {
        PrototypeDungeonEncounterRequest copy = new PrototypeDungeonEncounterRequest();
        if (source == null)
        {
            return copy;
        }

        copy.PanelId = string.IsNullOrEmpty(source.PanelId) ? string.Empty : source.PanelId;
        copy.RoomId = string.IsNullOrEmpty(source.RoomId) ? string.Empty : source.RoomId;
        copy.RoomLabel = string.IsNullOrEmpty(source.RoomLabel) ? "None" : source.RoomLabel;
        copy.EncounterId = string.IsNullOrEmpty(source.EncounterId) ? string.Empty : source.EncounterId;
        copy.EncounterLabel = string.IsNullOrEmpty(source.EncounterLabel) ? "None" : source.EncounterLabel;
        copy.EncounterTypeKey = string.IsNullOrEmpty(source.EncounterTypeKey) ? string.Empty : source.EncounterTypeKey;
        copy.EncounterTemplateKey = string.IsNullOrEmpty(source.EncounterTemplateKey) ? string.Empty : source.EncounterTemplateKey;
        copy.KeyEncounterTag = string.IsNullOrEmpty(source.KeyEncounterTag) ? "None" : source.KeyEncounterTag;
        copy.DungeonId = string.IsNullOrEmpty(source.DungeonId) ? string.Empty : source.DungeonId;
        copy.DungeonLabel = string.IsNullOrEmpty(source.DungeonLabel) ? "None" : source.DungeonLabel;
        copy.RouteId = string.IsNullOrEmpty(source.RouteId) ? string.Empty : source.RouteId;
        copy.RouteLabel = string.IsNullOrEmpty(source.RouteLabel) ? "None" : source.RouteLabel;
        copy.RouteRiskLabel = string.IsNullOrEmpty(source.RouteRiskLabel) ? "None" : source.RouteRiskLabel;
        copy.SupplyPressureSummaryText = string.IsNullOrEmpty(source.SupplyPressureSummaryText) ? "None" : source.SupplyPressureSummaryText;
        copy.ThreatPressureSummaryText = string.IsNullOrEmpty(source.ThreatPressureSummaryText) ? "None" : source.ThreatPressureSummaryText;
        copy.DiscoverySummaryText = string.IsNullOrEmpty(source.DiscoverySummaryText) ? "None" : source.DiscoverySummaryText;
        copy.ExtractionPressureSummaryText = string.IsNullOrEmpty(source.ExtractionPressureSummaryText) ? "None" : source.ExtractionPressureSummaryText;
        copy.ModifierSummaryText = string.IsNullOrEmpty(source.ModifierSummaryText) ? "None" : source.ModifierSummaryText;
        copy.SummaryText = string.IsNullOrEmpty(source.SummaryText) ? "None" : source.SummaryText;
        return copy;
    }

    private PrototypeDungeonChoiceOutcome CopyDungeonChoiceOutcome(PrototypeDungeonChoiceOutcome source)
    {
        PrototypeDungeonChoiceOutcome copy = new PrototypeDungeonChoiceOutcome();
        if (source == null)
        {
            return copy;
        }

        copy.PanelId = string.IsNullOrEmpty(source.PanelId) ? string.Empty : source.PanelId;
        copy.StageKindKey = string.IsNullOrEmpty(source.StageKindKey) ? string.Empty : source.StageKindKey;
        copy.SelectedOptionId = string.IsNullOrEmpty(source.SelectedOptionId) ? string.Empty : source.SelectedOptionId;
        copy.SelectedOptionLabel = string.IsNullOrEmpty(source.SelectedOptionLabel) ? "None" : source.SelectedOptionLabel;
        copy.OutcomeKindKey = string.IsNullOrEmpty(source.OutcomeKindKey) ? string.Empty : source.OutcomeKindKey;
        copy.TransitionStageKindKey = string.IsNullOrEmpty(source.TransitionStageKindKey) ? string.Empty : source.TransitionStageKindKey;
        copy.IsConfirmed = source.IsConfirmed;
        copy.SummaryText = string.IsNullOrEmpty(source.SummaryText) ? "None" : source.SummaryText;
        copy.DeltaSummaryText = string.IsNullOrEmpty(source.DeltaSummaryText) ? "None" : source.DeltaSummaryText;
        return copy;
    }

    private PrototypeDungeonEventResolution CopyDungeonEventResolution(PrototypeDungeonEventResolution source)
    {
        PrototypeDungeonEventResolution copy = new PrototypeDungeonEventResolution();
        if (source == null)
        {
            return copy;
        }

        copy.ShrineResolved = source.ShrineResolved;
        copy.ShrineChoiceId = string.IsNullOrEmpty(source.ShrineChoiceId) ? string.Empty : source.ShrineChoiceId;
        copy.ShrineChoiceLabel = string.IsNullOrEmpty(source.ShrineChoiceLabel) ? "None" : source.ShrineChoiceLabel;
        copy.ShrineHealAmount = Mathf.Max(0, source.ShrineHealAmount);
        copy.ShrineLootAmount = Mathf.Max(0, source.ShrineLootAmount);
        copy.ShrineSummaryText = string.IsNullOrEmpty(source.ShrineSummaryText) ? "None" : source.ShrineSummaryText;
        copy.PreparationResolved = source.PreparationResolved;
        copy.PreparationChoiceId = string.IsNullOrEmpty(source.PreparationChoiceId) ? string.Empty : source.PreparationChoiceId;
        copy.PreparationChoiceLabel = string.IsNullOrEmpty(source.PreparationChoiceLabel) ? "Pending" : source.PreparationChoiceLabel;
        copy.PreparationHealAmount = Mathf.Max(0, source.PreparationHealAmount);
        copy.PreparationBonusAmount = Mathf.Max(0, source.PreparationBonusAmount);
        copy.PreparationBonusPending = source.PreparationBonusPending;
        copy.PreparationSummaryText = string.IsNullOrEmpty(source.PreparationSummaryText) ? "Pending" : source.PreparationSummaryText;
        copy.LoopSummaryText = string.IsNullOrEmpty(source.LoopSummaryText) ? "None" : source.LoopSummaryText;
        return copy;
    }

    private PrototypeDungeonExtractionResult CopyDungeonExtractionResult(PrototypeDungeonExtractionResult source)
    {
        PrototypeDungeonExtractionResult copy = new PrototypeDungeonExtractionResult();
        if (source == null)
        {
            return copy;
        }

        copy.ExtractionKey = string.IsNullOrEmpty(source.ExtractionKey) ? string.Empty : source.ExtractionKey;
        copy.ExtractionLabel = string.IsNullOrEmpty(source.ExtractionLabel) ? "None" : source.ExtractionLabel;
        copy.ReturnedLootAmount = Mathf.Max(0, source.ReturnedLootAmount);
        copy.ExitUnlocked = source.ExitUnlocked;
        copy.EliteDefeated = source.EliteDefeated;
        copy.Success = source.Success;
        copy.ResultSummaryText = string.IsNullOrEmpty(source.ResultSummaryText) ? "None" : source.ResultSummaryText;
        copy.SummaryText = string.IsNullOrEmpty(source.SummaryText) ? "Pending" : source.SummaryText;
        copy.PressureSummaryText = string.IsNullOrEmpty(source.PressureSummaryText) ? "None" : source.PressureSummaryText;
        return copy;
    }

    private PrototypeDungeonPanelContext CopyDungeonPanelContext(PrototypeDungeonPanelContext source)
    {
        PrototypeDungeonPanelContext copy = new PrototypeDungeonPanelContext();
        if (source == null)
        {
            return copy;
        }

        copy.Template = CopyDungeonPanelTemplate(source.Template);
        copy.PanelId = string.IsNullOrEmpty(source.PanelId) ? string.Empty : source.PanelId;
        copy.DungeonId = string.IsNullOrEmpty(source.DungeonId) ? string.Empty : source.DungeonId;
        copy.DungeonLabel = string.IsNullOrEmpty(source.DungeonLabel) ? "None" : source.DungeonLabel;
        copy.RouteId = string.IsNullOrEmpty(source.RouteId) ? string.Empty : source.RouteId;
        copy.RouteLabel = string.IsNullOrEmpty(source.RouteLabel) ? "None" : source.RouteLabel;
        copy.RoomStepId = string.IsNullOrEmpty(source.RoomStepId) ? string.Empty : source.RoomStepId;
        copy.RoomDisplayLabel = string.IsNullOrEmpty(source.RoomDisplayLabel) ? "None" : source.RoomDisplayLabel;
        copy.RoomTypeLabel = string.IsNullOrEmpty(source.RoomTypeLabel) ? "None" : source.RoomTypeLabel;
        copy.CurrentRoomIndex = Mathf.Max(0, source.CurrentRoomIndex);
        copy.TotalPlannedRooms = Mathf.Max(0, source.TotalPlannedRooms);
        copy.ProgressSummaryText = string.IsNullOrEmpty(source.ProgressSummaryText) ? "None" : source.ProgressSummaryText;
        copy.NextMajorGoalText = string.IsNullOrEmpty(source.NextMajorGoalText) ? "None" : source.NextMajorGoalText;
        copy.VisibleAffordanceSummaryText = string.IsNullOrEmpty(source.VisibleAffordanceSummaryText) ? "None" : source.VisibleAffordanceSummaryText;
        copy.ChoiceSummaryText = string.IsNullOrEmpty(source.ChoiceSummaryText) ? "None" : source.ChoiceSummaryText;
        copy.RiskPreviewSummaryText = string.IsNullOrEmpty(source.RiskPreviewSummaryText) ? "None" : source.RiskPreviewSummaryText;
        copy.RewardPreviewSummaryText = string.IsNullOrEmpty(source.RewardPreviewSummaryText) ? "None" : source.RewardPreviewSummaryText;
        copy.EventPreviewSummaryText = string.IsNullOrEmpty(source.EventPreviewSummaryText) ? "None" : source.EventPreviewSummaryText;
        copy.EncounterRequestSummaryText = string.IsNullOrEmpty(source.EncounterRequestSummaryText) ? "None" : source.EncounterRequestSummaryText;
        copy.SupplyPressureSummaryText = string.IsNullOrEmpty(source.SupplyPressureSummaryText) ? "None" : source.SupplyPressureSummaryText;
        copy.TimePressureSummaryText = string.IsNullOrEmpty(source.TimePressureSummaryText) ? "None" : source.TimePressureSummaryText;
        copy.ThreatPressureSummaryText = string.IsNullOrEmpty(source.ThreatPressureSummaryText) ? "None" : source.ThreatPressureSummaryText;
        copy.DiscoverySummaryText = string.IsNullOrEmpty(source.DiscoverySummaryText) ? "None" : source.DiscoverySummaryText;
        copy.ExtractionPressureSummaryText = string.IsNullOrEmpty(source.ExtractionPressureSummaryText) ? "None" : source.ExtractionPressureSummaryText;
        copy.CurrentLaneSelectionText = string.IsNullOrEmpty(source.CurrentLaneSelectionText) ? "None" : source.CurrentLaneSelectionText;
        copy.LaneDeltaSummaryText = string.IsNullOrEmpty(source.LaneDeltaSummaryText) ? "None" : source.LaneDeltaSummaryText;
        copy.Options = CopyDungeonPanelOptions(source.Options);
        copy.LatestChoiceOutcomeSummaryText = string.IsNullOrEmpty(source.LatestChoiceOutcomeSummaryText) ? "None" : source.LatestChoiceOutcomeSummaryText;
        copy.EventResolutionSummaryText = string.IsNullOrEmpty(source.EventResolutionSummaryText) ? "None" : source.EventResolutionSummaryText;
        copy.ExtractionSummaryText = string.IsNullOrEmpty(source.ExtractionSummaryText) ? "None" : source.ExtractionSummaryText;
        copy.IsDiscovered = source.IsDiscovered;
        copy.IsResolved = source.IsResolved;
        copy.IsBlocked = source.IsBlocked;
        copy.DiscoveryStateText = string.IsNullOrEmpty(source.DiscoveryStateText) ? "Hidden" : source.DiscoveryStateText;
        copy.ResolutionStateText = string.IsNullOrEmpty(source.ResolutionStateText) ? "Pending" : source.ResolutionStateText;
        copy.BlockedStateText = string.IsNullOrEmpty(source.BlockedStateText) ? "Open" : source.BlockedStateText;
        copy.PanelSummaryText = string.IsNullOrEmpty(source.PanelSummaryText) ? "None" : source.PanelSummaryText;
        return copy;
    }

    private PrototypeDungeonRunResultContext CopyDungeonRunResultContext(PrototypeDungeonRunResultContext source)
    {
        PrototypeDungeonRunResultContext copy = new PrototypeDungeonRunResultContext();
        if (source == null)
        {
            return copy;
        }

        copy.CanonicalRunResult = CopyRpgRunResultSnapshot(source.CanonicalRunResult);
        copy.WorldReturnAftermathPreview = CopyCityWriteback(source.WorldReturnAftermathPreview);
        copy.WorldOutcomeReadbackPreview = CopyOutcomeReadback(source.WorldOutcomeReadbackPreview);
        copy.DungeonId = string.IsNullOrEmpty(source.DungeonId) ? string.Empty : source.DungeonId;
        copy.DungeonLabel = string.IsNullOrEmpty(source.DungeonLabel) ? "None" : source.DungeonLabel;
        copy.RouteId = string.IsNullOrEmpty(source.RouteId) ? string.Empty : source.RouteId;
        copy.RouteLabel = string.IsNullOrEmpty(source.RouteLabel) ? "None" : source.RouteLabel;
        copy.RunOutcomeKey = string.IsNullOrEmpty(source.RunOutcomeKey) ? string.Empty : source.RunOutcomeKey;
        copy.ResultStateText = string.IsNullOrEmpty(source.ResultStateText) ? "None" : source.ResultStateText;
        copy.CityDispatchedFromText = string.IsNullOrEmpty(source.CityDispatchedFromText) ? "None" : source.CityDispatchedFromText;
        copy.DungeonDangerText = string.IsNullOrEmpty(source.DungeonDangerText) ? "None" : source.DungeonDangerText;
        copy.RecommendedRouteText = string.IsNullOrEmpty(source.RecommendedRouteText) ? "None" : source.RecommendedRouteText;
        copy.FollowedRecommendationText = string.IsNullOrEmpty(source.FollowedRecommendationText) ? "None" : source.FollowedRecommendationText;
        copy.ManaShardsReturnedText = string.IsNullOrEmpty(source.ManaShardsReturnedText) ? "None" : source.ManaShardsReturnedText;
        copy.StockBeforeText = string.IsNullOrEmpty(source.StockBeforeText) ? "None" : source.StockBeforeText;
        copy.StockAfterText = string.IsNullOrEmpty(source.StockAfterText) ? "None" : source.StockAfterText;
        copy.StockDeltaText = string.IsNullOrEmpty(source.StockDeltaText) ? "None" : source.StockDeltaText;
        copy.NeedPressureBeforeText = string.IsNullOrEmpty(source.NeedPressureBeforeText) ? "None" : source.NeedPressureBeforeText;
        copy.NeedPressureAfterText = string.IsNullOrEmpty(source.NeedPressureAfterText) ? "None" : source.NeedPressureAfterText;
        copy.DispatchReadinessBeforeText = string.IsNullOrEmpty(source.DispatchReadinessBeforeText) ? "None" : source.DispatchReadinessBeforeText;
        copy.DispatchReadinessAfterText = string.IsNullOrEmpty(source.DispatchReadinessAfterText) ? "None" : source.DispatchReadinessAfterText;
        copy.RecoveryEtaText = string.IsNullOrEmpty(source.RecoveryEtaText) ? "None" : source.RecoveryEtaText;
        copy.RouteRiskText = string.IsNullOrEmpty(source.RouteRiskText) ? "None" : source.RouteRiskText;
        copy.TimeCostTurns = Mathf.Max(0, source.TimeCostTurns);
        copy.TimeCostSummaryText = string.IsNullOrEmpty(source.TimeCostSummaryText) ? "None" : source.TimeCostSummaryText;
        copy.TurnsTakenText = string.IsNullOrEmpty(source.TurnsTakenText) ? "0" : source.TurnsTakenText;
        copy.ResultSummaryText = string.IsNullOrEmpty(source.ResultSummaryText) ? "None" : source.ResultSummaryText;
        copy.SurvivingMembersSummaryText = string.IsNullOrEmpty(source.SurvivingMembersSummaryText) ? "None" : source.SurvivingMembersSummaryText;
        copy.ResourceDeltaSummaryText = string.IsNullOrEmpty(source.ResourceDeltaSummaryText) ? "None" : source.ResourceDeltaSummaryText;
        copy.SupplyPressureSummaryText = string.IsNullOrEmpty(source.SupplyPressureSummaryText) ? "None" : source.SupplyPressureSummaryText;
        copy.InjurySummaryText = string.IsNullOrEmpty(source.InjurySummaryText) ? "None" : source.InjurySummaryText;
        copy.CasualtySummaryText = string.IsNullOrEmpty(source.CasualtySummaryText) ? "None" : source.CasualtySummaryText;
        copy.DiscoveredFlagsSummaryText = string.IsNullOrEmpty(source.DiscoveredFlagsSummaryText) ? "None" : source.DiscoveredFlagsSummaryText;
        copy.ThreatProgressSummaryText = string.IsNullOrEmpty(source.ThreatProgressSummaryText) ? "None" : source.ThreatProgressSummaryText;
        copy.ClearProgressSummaryText = string.IsNullOrEmpty(source.ClearProgressSummaryText) ? "None" : source.ClearProgressSummaryText;
        copy.EventLogSummaryText = string.IsNullOrEmpty(source.EventLogSummaryText) ? "None" : source.EventLogSummaryText;
        copy.EncounterRequestSummaryText = string.IsNullOrEmpty(source.EncounterRequestSummaryText) ? "None" : source.EncounterRequestSummaryText;
        copy.BattleAbsorbSummaryText = string.IsNullOrEmpty(source.BattleAbsorbSummaryText) ? "None" : source.BattleAbsorbSummaryText;
        copy.ChoiceOutcomeSummaryText = string.IsNullOrEmpty(source.ChoiceOutcomeSummaryText) ? "None" : source.ChoiceOutcomeSummaryText;
        copy.EventResolutionSummaryText = string.IsNullOrEmpty(source.EventResolutionSummaryText) ? "None" : source.EventResolutionSummaryText;
        copy.ExtractionSummaryText = string.IsNullOrEmpty(source.ExtractionSummaryText) ? "None" : source.ExtractionSummaryText;
        copy.ExtractionPressureSummaryText = string.IsNullOrEmpty(source.ExtractionPressureSummaryText) ? "None" : source.ExtractionPressureSummaryText;
        copy.BattlesFoughtCount = Mathf.Max(0, source.BattlesFoughtCount);
        copy.KeyEncounterSummaryText = string.IsNullOrEmpty(source.KeyEncounterSummaryText) ? "None" : source.KeyEncounterSummaryText;
        copy.SelectedRouteSummaryText = string.IsNullOrEmpty(source.SelectedRouteSummaryText) ? "None" : source.SelectedRouteSummaryText;
        copy.FollowedRecommendationSummaryText = string.IsNullOrEmpty(source.FollowedRecommendationSummaryText) ? "None" : source.FollowedRecommendationSummaryText;
        copy.BattleContextSummaryText = string.IsNullOrEmpty(source.BattleContextSummaryText) ? "None" : source.BattleContextSummaryText;
        copy.BattleRuntimeSummaryText = string.IsNullOrEmpty(source.BattleRuntimeSummaryText) ? "None" : source.BattleRuntimeSummaryText;
        copy.BattleRuleSummaryText = string.IsNullOrEmpty(source.BattleRuleSummaryText) ? "None" : source.BattleRuleSummaryText;
        copy.BattleResultCoreSummaryText = string.IsNullOrEmpty(source.BattleResultCoreSummaryText) ? "None" : source.BattleResultCoreSummaryText;
        copy.NotableBattleEventsSummaryText = string.IsNullOrEmpty(source.NotableBattleEventsSummaryText) ? "None" : source.NotableBattleEventsSummaryText;
        copy.GearRewardCandidateSummaryText = string.IsNullOrEmpty(source.GearRewardCandidateSummaryText) ? "None" : source.GearRewardCandidateSummaryText;
        copy.EquipSwapChoiceSummaryText = string.IsNullOrEmpty(source.EquipSwapChoiceSummaryText) ? "None" : source.EquipSwapChoiceSummaryText;
        copy.GearCarryContinuitySummaryText = string.IsNullOrEmpty(source.GearCarryContinuitySummaryText) ? "None" : source.GearCarryContinuitySummaryText;
        copy.LootGainedText = string.IsNullOrEmpty(source.LootGainedText) ? "None" : source.LootGainedText;
        copy.BattleLootText = string.IsNullOrEmpty(source.BattleLootText) ? "None" : source.BattleLootText;
        copy.ChestLootText = string.IsNullOrEmpty(source.ChestLootText) ? "None" : source.ChestLootText;
        copy.EventLootText = string.IsNullOrEmpty(source.EventLootText) ? "None" : source.EventLootText;
        copy.EventChoiceText = string.IsNullOrEmpty(source.EventChoiceText) ? "None" : source.EventChoiceText;
        copy.PreEliteChoiceText = string.IsNullOrEmpty(source.PreEliteChoiceText) ? "Pending" : source.PreEliteChoiceText;
        copy.PreEliteHealAmountText = string.IsNullOrEmpty(source.PreEliteHealAmountText) ? "None" : source.PreEliteHealAmountText;
        copy.EliteBonusRewardEarnedText = string.IsNullOrEmpty(source.EliteBonusRewardEarnedText) ? "None" : source.EliteBonusRewardEarnedText;
        copy.EliteBonusRewardAmountText = string.IsNullOrEmpty(source.EliteBonusRewardAmountText) ? "None" : source.EliteBonusRewardAmountText;
        copy.RoomPathSummaryText = string.IsNullOrEmpty(source.RoomPathSummaryText) ? "None" : source.RoomPathSummaryText;
        copy.PartyHpSummaryText = string.IsNullOrEmpty(source.PartyHpSummaryText) ? "None" : source.PartyHpSummaryText;
        copy.PartyConditionText = string.IsNullOrEmpty(source.PartyConditionText) ? "None" : source.PartyConditionText;
        copy.ClearedEncountersText = string.IsNullOrEmpty(source.ClearedEncountersText) ? "0 / 0" : source.ClearedEncountersText;
        copy.OpenedChestsText = string.IsNullOrEmpty(source.OpenedChestsText) ? "0 / 0" : source.OpenedChestsText;
        copy.EliteDefeatedText = string.IsNullOrEmpty(source.EliteDefeatedText) ? "None" : source.EliteDefeatedText;
        copy.EliteNameText = string.IsNullOrEmpty(source.EliteNameText) ? "None" : source.EliteNameText;
        copy.EliteRewardIdentityText = string.IsNullOrEmpty(source.EliteRewardIdentityText) ? "None" : source.EliteRewardIdentityText;
        copy.EliteRewardAmountText = string.IsNullOrEmpty(source.EliteRewardAmountText) ? "None" : source.EliteRewardAmountText;
        copy.ReturnPromptText = string.IsNullOrEmpty(source.ReturnPromptText) ? "None" : source.ReturnPromptText;
        copy.WorldWritebackResultSummaryText = string.IsNullOrEmpty(source.WorldWritebackResultSummaryText) ? "None" : source.WorldWritebackResultSummaryText;
        copy.WorldWritebackSurvivingMembersSummaryText = string.IsNullOrEmpty(source.WorldWritebackSurvivingMembersSummaryText) ? "None" : source.WorldWritebackSurvivingMembersSummaryText;
        copy.WorldWritebackEncounterSummaryText = string.IsNullOrEmpty(source.WorldWritebackEncounterSummaryText) ? "None" : source.WorldWritebackEncounterSummaryText;
        copy.WorldWritebackChoiceSummaryText = string.IsNullOrEmpty(source.WorldWritebackChoiceSummaryText) ? "None" : source.WorldWritebackChoiceSummaryText;
        copy.WorldWritebackLootSummaryText = string.IsNullOrEmpty(source.WorldWritebackLootSummaryText) ? "None" : source.WorldWritebackLootSummaryText;
        copy.WorldWritebackRouteSummaryText = string.IsNullOrEmpty(source.WorldWritebackRouteSummaryText) ? "None" : source.WorldWritebackRouteSummaryText;
        copy.WorldWritebackDungeonSummaryText = string.IsNullOrEmpty(source.WorldWritebackDungeonSummaryText) ? "None" : source.WorldWritebackDungeonSummaryText;
        return copy;
    }

    private CityWriteback CopyCityWriteback(CityWriteback source)
    {
        CityWriteback copy = new CityWriteback();
        if (source == null)
        {
            return copy;
        }

        copy.CityId = string.IsNullOrEmpty(source.CityId) ? string.Empty : source.CityId;
        copy.CityLabel = string.IsNullOrEmpty(source.CityLabel) ? "None" : source.CityLabel;
        copy.ResultStateKey = string.IsNullOrEmpty(source.ResultStateKey) ? string.Empty : source.ResultStateKey;
        copy.RewardResourceId = string.IsNullOrEmpty(source.RewardResourceId) ? string.Empty : source.RewardResourceId;
        copy.LootReturned = Mathf.Max(0, source.LootReturned);
        copy.LootSummaryText = string.IsNullOrEmpty(source.LootSummaryText) ? "None" : source.LootSummaryText;
        copy.PartyOutcomeSummaryText = string.IsNullOrEmpty(source.PartyOutcomeSummaryText) ? "None" : source.PartyOutcomeSummaryText;
        copy.StockReactionSummaryText = string.IsNullOrEmpty(source.StockReactionSummaryText) ? "None" : source.StockReactionSummaryText;
        copy.StockBefore = source.StockBefore;
        copy.StockAfter = source.StockAfter;
        copy.StockDelta = source.StockDelta;
        copy.NeedPressureBeforeText = string.IsNullOrEmpty(source.NeedPressureBeforeText) ? "None" : source.NeedPressureBeforeText;
        copy.NeedPressureAfterText = string.IsNullOrEmpty(source.NeedPressureAfterText) ? "None" : source.NeedPressureAfterText;
        copy.DispatchReadinessBeforeText = string.IsNullOrEmpty(source.DispatchReadinessBeforeText) ? "None" : source.DispatchReadinessBeforeText;
        copy.DispatchReadinessAfterText = string.IsNullOrEmpty(source.DispatchReadinessAfterText) ? "None" : source.DispatchReadinessAfterText;
        copy.RecoveryEtaText = string.IsNullOrEmpty(source.RecoveryEtaText) ? "None" : source.RecoveryEtaText;
        copy.FollowUpHintText = string.IsNullOrEmpty(source.FollowUpHintText) ? "None" : source.FollowUpHintText;
        copy.SummaryText = string.IsNullOrEmpty(source.SummaryText) ? "None" : source.SummaryText;
        return copy;
    }

    private OutcomeReadback CopyOutcomeReadback(OutcomeReadback source)
    {
        OutcomeReadback copy = new OutcomeReadback();
        if (source == null)
        {
            return copy;
        }

        copy.CityId = string.IsNullOrEmpty(source.CityId) ? string.Empty : source.CityId;
        copy.CityLabel = string.IsNullOrEmpty(source.CityLabel) ? "None" : source.CityLabel;
        copy.ResultStateKey = string.IsNullOrEmpty(source.ResultStateKey) ? string.Empty : source.ResultStateKey;
        copy.AcknowledgementText = string.IsNullOrEmpty(source.AcknowledgementText) ? "None" : source.AcknowledgementText;
        copy.LatestReturnAftermathText = string.IsNullOrEmpty(source.LatestReturnAftermathText) ? "None" : source.LatestReturnAftermathText;
        copy.PostRunSummaryText = string.IsNullOrEmpty(source.PostRunSummaryText) ? "None" : source.PostRunSummaryText;
        copy.NextSuggestedActionText = string.IsNullOrEmpty(source.NextSuggestedActionText) ? "None" : source.NextSuggestedActionText;
        copy.FollowUpHintText = string.IsNullOrEmpty(source.FollowUpHintText) ? "None" : source.FollowUpHintText;
        copy.LastExpeditionResultText = string.IsNullOrEmpty(source.LastExpeditionResultText) ? "None" : source.LastExpeditionResultText;
        copy.WorldWritebackSummaryText = string.IsNullOrEmpty(source.WorldWritebackSummaryText) ? "None" : source.WorldWritebackSummaryText;
        copy.SelectedWorldWritebackText = string.IsNullOrEmpty(source.SelectedWorldWritebackText) ? "None" : source.SelectedWorldWritebackText;
        copy.DungeonStatusText = string.IsNullOrEmpty(source.DungeonStatusText) ? "None" : source.DungeonStatusText;
        copy.DungeonAvailabilityText = string.IsNullOrEmpty(source.DungeonAvailabilityText) ? "None" : source.DungeonAvailabilityText;
        copy.DungeonLastOutcomeText = string.IsNullOrEmpty(source.DungeonLastOutcomeText) ? "None" : source.DungeonLastOutcomeText;
        copy.SurvivingMembersSummaryText = string.IsNullOrEmpty(source.SurvivingMembersSummaryText) ? "None" : source.SurvivingMembersSummaryText;
        copy.ClearedEncountersSummaryText = string.IsNullOrEmpty(source.ClearedEncountersSummaryText) ? "None" : source.ClearedEncountersSummaryText;
        copy.EventChoiceSummaryText = string.IsNullOrEmpty(source.EventChoiceSummaryText) ? "None" : source.EventChoiceSummaryText;
        copy.LootBreakdownSummaryText = string.IsNullOrEmpty(source.LootBreakdownSummaryText) ? "None" : source.LootBreakdownSummaryText;
        copy.DungeonSummaryText = string.IsNullOrEmpty(source.DungeonSummaryText) ? "None" : source.DungeonSummaryText;
        copy.RouteSummaryText = string.IsNullOrEmpty(source.RouteSummaryText) ? "None" : source.RouteSummaryText;
        copy.StockBeforeText = string.IsNullOrEmpty(source.StockBeforeText) ? "None" : source.StockBeforeText;
        copy.StockAfterText = string.IsNullOrEmpty(source.StockAfterText) ? "None" : source.StockAfterText;
        copy.StockDeltaText = string.IsNullOrEmpty(source.StockDeltaText) ? "None" : source.StockDeltaText;
        copy.NeedPressureBeforeText = string.IsNullOrEmpty(source.NeedPressureBeforeText) ? "None" : source.NeedPressureBeforeText;
        copy.NeedPressureAfterText = string.IsNullOrEmpty(source.NeedPressureAfterText) ? "None" : source.NeedPressureAfterText;
        copy.DispatchReadinessBeforeText = string.IsNullOrEmpty(source.DispatchReadinessBeforeText) ? "None" : source.DispatchReadinessBeforeText;
        copy.DispatchReadinessAfterText = string.IsNullOrEmpty(source.DispatchReadinessAfterText) ? "None" : source.DispatchReadinessAfterText;
        copy.RecoveryEtaText = string.IsNullOrEmpty(source.RecoveryEtaText) ? "None" : source.RecoveryEtaText;
        copy.GearRewardSummaryText = string.IsNullOrEmpty(source.GearRewardSummaryText) ? "None" : source.GearRewardSummaryText;
        copy.EquipSwapSummaryText = string.IsNullOrEmpty(source.EquipSwapSummaryText) ? "None" : source.EquipSwapSummaryText;
        copy.GearContinuitySummaryText = string.IsNullOrEmpty(source.GearContinuitySummaryText) ? "None" : source.GearContinuitySummaryText;
        copy.RecentExpeditionLog1Text = string.IsNullOrEmpty(source.RecentExpeditionLog1Text) ? "None" : source.RecentExpeditionLog1Text;
        copy.RecentExpeditionLog2Text = string.IsNullOrEmpty(source.RecentExpeditionLog2Text) ? "None" : source.RecentExpeditionLog2Text;
        copy.RecentExpeditionLog3Text = string.IsNullOrEmpty(source.RecentExpeditionLog3Text) ? "None" : source.RecentExpeditionLog3Text;
        copy.RecentWorldWritebackLog1Text = string.IsNullOrEmpty(source.RecentWorldWritebackLog1Text) ? "None" : source.RecentWorldWritebackLog1Text;
        copy.RecentWorldWritebackLog2Text = string.IsNullOrEmpty(source.RecentWorldWritebackLog2Text) ? "None" : source.RecentWorldWritebackLog2Text;
        copy.RecentWorldWritebackLog3Text = string.IsNullOrEmpty(source.RecentWorldWritebackLog3Text) ? "None" : source.RecentWorldWritebackLog3Text;
        return copy;
    }
}













