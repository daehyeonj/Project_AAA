using UnityEngine;

public sealed partial class StaticPlaceholderWorldView
{
    private bool _isExpeditionPrepBoardOpen;
    private string _expeditionPrepFeedbackText = "None";

    public bool IsExpeditionPrepBoardOpen => _isExpeditionPrepBoardOpen;
    public bool CanOpenSelectedCityExpeditionPrepBoard => CanOpenSelectedCityDungeonAction;

    private bool IsExpeditionPrepRouteSelectionActive()
    {
        return _isExpeditionPrepBoardOpen || _dungeonRunState == DungeonRunState.RouteChoice;
    }

    private void SetExpeditionPrepFeedbackText(string text)
    {
        _expeditionPrepFeedbackText = IsMeaningfulSnapshotText(text) ? text : "None";
    }

    private void CloseExpeditionPrepBoardShell()
    {
        _isExpeditionPrepBoardOpen = false;
        _expeditionPrepFeedbackText = "None";
    }

    public bool TryOpenSelectedCityExpeditionPrepBoard()
    {
        FocusPendingExpeditionPostRunRevealTarget();
        WorldSimCitySourceData entrySnapshot = BuildSelectedCityHubEntrySnapshot();
        ExpeditionPrepHandoff handoff = BuildSelectedExpeditionPrepHandoff();
        if (_runtimeEconomyState == null ||
            _selectedMarker == null ||
            _selectedMarker.EntityData.Kind != WorldEntityKind.City ||
            IsDungeonRunActive ||
            !CanOpenSelectedCityExpeditionPrepBoard ||
            entrySnapshot == null ||
            !entrySnapshot.HasSelectedCity ||
            !entrySnapshot.CanOpenExpeditionPrep ||
            handoff == null ||
            !handoff.IsReady)
        {
            return false;
        }

        string cityId = !string.IsNullOrEmpty(handoff.CityId)
            ? handoff.CityId
            : entrySnapshot.SelectedCityId;
        string dungeonId = !string.IsNullOrEmpty(handoff.DungeonId)
            ? handoff.DungeonId
            : entrySnapshot.LinkedDungeonId;
        string partyId = !string.IsNullOrEmpty(handoff.PartyId)
            ? handoff.PartyId
            : entrySnapshot.StagedPartyId;
        WorldEntityData dungeon = FindEntity(dungeonId);
        if (string.IsNullOrEmpty(cityId) || dungeon == null || string.IsNullOrEmpty(partyId))
        {
            return false;
        }

        ResetDungeonRunPresentationState();
        _activeDungeonParty = GetOrCreateDungeonParty(cityId, partyId);
        _currentHomeCityId = cityId;
        _currentDungeonId = dungeonId;
        _currentDungeonName = IsMeaningfulSnapshotText(handoff.DungeonLabel)
            ? handoff.DungeonLabel
            : string.IsNullOrEmpty(dungeon.DisplayName)
                ? dungeonId
                : dungeon.DisplayName;
        _isExpeditionPrepBoardOpen = true;
        _pendingDungeonExit = false;
        _recentBattleLogs.Clear();
        RefreshDispatchRecommendation();
        string recommendedRouteId = !string.IsNullOrEmpty(handoff.RecommendedRouteId)
            ? handoff.RecommendedRouteId
            : _recommendedRouteId;
        if (!string.IsNullOrEmpty(recommendedRouteId))
        {
            TryTriggerRouteChoice(recommendedRouteId);
        }

        CacheCurrentExpeditionPrepReadModel();
        SetExpeditionPrepFeedbackText(
            IsMeaningfulSnapshotText(handoff.RecommendedNextActionText)
                ? handoff.RecommendedNextActionText
                : "Review the dispatch plan before launching the expedition.");
        RefreshSelectionPrompt();
        RefreshDungeonPresentation();
        AcknowledgePendingExpeditionPostRunReveal();
        return true;
    }

    public void CancelExpeditionPrepBoard()
    {
        if (!_isExpeditionPrepBoardOpen)
        {
            return;
        }

        CloseExpeditionPrepBoardShell();
        _recentBattleLogs.Clear();
        ResetDungeonRunPresentationState();
        if (_dungeonRoot != null)
        {
            _dungeonRoot.SetActive(false);
        }
    }

    public bool TrySelectExpeditionPrepRoute(string optionKey)
    {
        return _isExpeditionPrepBoardOpen && TryTriggerRouteChoice(optionKey);
    }

    public bool TryCycleExpeditionPrepDispatchPolicy()
    {
        return _isExpeditionPrepBoardOpen && TryCycleCurrentDispatchPolicy();
    }

    public bool TryConfirmSelectedExpeditionLaunch(Camera worldCamera)
    {
        return _isExpeditionPrepBoardOpen && TryConfirmRouteChoice();
    }

    public ExpeditionPrepSurfaceData BuildSelectedExpeditionPrepSurfaceData()
    {
        WorldObservationSurfaceData observation = BuildWorldObservationSurfaceData(includeDetailedPartyReadbacks: true);
        return CopyExpeditionPrepSurfaceData(observation != null ? observation.ExpeditionPrep : null);
    }

    public ExpeditionLaunchRequest BuildSelectedExpeditionLaunchRequest()
    {
        return BuildExpeditionLaunchRequestFromSurface(BuildSelectedExpeditionPrepSurfaceData());
    }

    private ExpeditionPrepSurfaceData BuildCanonicalExpeditionPrepSurfaceData(
        PrototypeWorldSnapshot snapshot,
        PrototypeWorldDispatchBriefingSnapshot briefing,
        ExpeditionStartContext startContext,
        bool includeDetailedPartyReadbacks = false)
    {
        ExpeditionPrepSurfaceData data = new ExpeditionPrepSurfaceData();
        CityPartyRosterSurfaceData roster = BuildSelectedCityPartyRosterSurfaceData();
        PrototypeWorldActiveExpeditionSnapshot activeSnapshot = BuildSelectedActiveExpeditionSnapshot();
        LaunchReadiness readiness = BuildLaunchReadiness(snapshot, briefing);
        ExpeditionStartContext context = CopyExpeditionStartContext(startContext);
        bool hasSelectedRoute = context != null && !string.IsNullOrEmpty(context.SelectedRouteId);

        data.IsBoardOpen = _isExpeditionPrepBoardOpen;
        data.CanOpenBoard = briefing != null && briefing.CanOpenPlanner;
        data.CanCycleDispatchPolicy = _isExpeditionPrepBoardOpen && !string.IsNullOrEmpty(_currentHomeCityId);
        data.CanLaunch = readiness != null && readiness.GateResult != null && readiness.GateResult.CanLaunch;
        data.BoardTitleText = "Expedition Prep";
        data.BoardSummaryText = context != null && IsMeaningfulSnapshotText(context.LaunchManifestSummaryText)
            ? context.LaunchManifestSummaryText
            : context != null && IsMeaningfulSnapshotText(context.ContextSummaryText)
                ? context.ContextSummaryText
            : "No expedition handoff is ready.";
        data.CityId = snapshot != null ? snapshot.SelectedCityId ?? string.Empty : string.Empty;
        data.CityLabel = snapshot != null && IsMeaningfulSnapshotText(snapshot.SelectedCityLabel) ? snapshot.SelectedCityLabel : "None";
        data.DungeonId = snapshot != null ? snapshot.SelectedDungeonId ?? string.Empty : string.Empty;
        data.DungeonLabel = snapshot != null && IsMeaningfulSnapshotText(snapshot.SelectedDungeonLabel) ? snapshot.SelectedDungeonLabel : "None";
        data.PartyId = snapshot != null ? snapshot.SelectedPartyId ?? string.Empty : string.Empty;
        data.PartyLabel = snapshot != null &&
            snapshot.PartySummary != null &&
            IsMeaningfulSnapshotText(snapshot.PartySummary.PartyLabel)
            ? snapshot.PartySummary.PartyLabel
            : "None";
        data.HasStagedParty = roster != null && roster.HasReadyParty;
        data.StagedPartyId = roster != null && !string.IsNullOrEmpty(roster.ReadyPartyId)
            ? roster.ReadyPartyId
            : data.PartyId;
        data.StagedPartyLabel = roster != null && IsMeaningfulSnapshotText(roster.ReadyPartyLabel)
            ? roster.ReadyPartyLabel
            : data.PartyLabel;
        data.StagedPartySummaryText = context != null && IsMeaningfulSnapshotText(context.StagedPartySummaryText)
            ? context.StagedPartySummaryText
            : briefing != null && IsMeaningfulSnapshotText(briefing.PartySummaryText)
                ? briefing.PartySummaryText
            : roster != null && IsMeaningfulSnapshotText(roster.AvailabilitySummaryText)
                ? roster.AvailabilitySummaryText
                : "None";
        data.TotalPartyCount = roster != null ? roster.TotalPartyCount : 0;
        data.IdlePartyCount = roster != null ? roster.IdlePartyCount : 0;
        data.ActiveExpeditionCount = roster != null ? roster.ActiveExpeditionCount : 0;
        data.PartyLoadoutSummaryText = context != null &&
            context.PartyManifest != null &&
            IsMeaningfulSnapshotText(context.PartyManifest.LoadoutSummaryText)
            ? context.PartyManifest.LoadoutSummaryText
            : context != null && IsMeaningfulSnapshotText(context.PartyLoadoutSummaryText)
                ? context.PartyLoadoutSummaryText
            : roster != null && IsMeaningfulSnapshotText(roster.ReadyPartyLoadoutSummaryText)
                ? roster.ReadyPartyLoadoutSummaryText
            : "None";
        data.DispatchPolicyText = context != null && IsMeaningfulSnapshotText(context.DispatchPolicyText)
            ? context.DispatchPolicyText
            : readiness.DispatchPolicyText;
        data.NeedPressureText = context != null && IsMeaningfulSnapshotText(context.NeedPressureText)
            ? context.NeedPressureText
            : readiness.NeedPressureText;
        data.DispatchReadinessText = context != null && IsMeaningfulSnapshotText(context.DispatchReadinessText)
            ? context.DispatchReadinessText
            : readiness.ReadinessText;
        data.RecoveryProgressText = context != null && IsMeaningfulSnapshotText(context.RecoveryProgressText)
            ? context.RecoveryProgressText
            : readiness.RecoveryProgressText;
        data.RecoveryEtaText = context != null && IsMeaningfulSnapshotText(context.RecoveryEtaText)
            ? context.RecoveryEtaText
            : readiness.RecoveryEtaText;
        data.LaunchReadiness = CopyLaunchReadiness(readiness);
        data.StartContext = context;
        data.SelectedRouteId = context != null && !string.IsNullOrEmpty(context.SelectedRouteId)
            ? context.SelectedRouteId
            : snapshot != null
                ? snapshot.SelectedRouteId ?? string.Empty
                : string.Empty;
        data.SelectedRouteLabel = context != null && IsMeaningfulSnapshotText(context.SelectedRouteLabel)
            ? context.SelectedRouteLabel
            : snapshot != null && IsMeaningfulSnapshotText(snapshot.SelectedRouteLabel)
                ? snapshot.SelectedRouteLabel
                : "None";
        data.RecommendedRouteId = context != null && !string.IsNullOrEmpty(context.RecommendedRouteId)
            ? context.RecommendedRouteId
            : snapshot != null
                ? snapshot.RecommendedRouteId ?? string.Empty
                : string.Empty;
        data.RecommendedRouteLabel = context != null && IsMeaningfulSnapshotText(context.RecommendedRouteLabel)
            ? context.RecommendedRouteLabel
            : snapshot != null && IsMeaningfulSnapshotText(snapshot.RecommendedRouteLabel)
                ? snapshot.RecommendedRouteLabel
                : "None";
        data.RecommendedRouteSummaryText = !string.IsNullOrEmpty(data.CityId) && !string.IsNullOrEmpty(data.DungeonId)
            ? BuildRecommendedRouteSummaryText(data.CityId, data.DungeonId)
            : data.RecommendedRouteLabel;
        data.RecommendationReasonText = context != null && IsMeaningfulSnapshotText(context.RecommendationReasonText)
            ? context.RecommendationReasonText
            : readiness.RecommendationReasonText;
        data.ExpectedNeedImpactText = context != null && IsMeaningfulSnapshotText(context.ExpectedNeedImpactText)
            ? context.ExpectedNeedImpactText
            : readiness.ExpectedNeedImpactText;
        data.DispatchBriefingSummaryText = briefing != null && IsMeaningfulSnapshotText(briefing.BriefingSummaryText)
            ? briefing.BriefingSummaryText
            : data.BoardSummaryText;
        data.RouteFitSummaryText = briefing != null && IsMeaningfulSnapshotText(briefing.RouteFitSummaryText)
            ? briefing.RouteFitSummaryText
            : "None";
        data.ProjectedOutcomeSummaryText = briefing != null && IsMeaningfulSnapshotText(briefing.ProjectedOutcomeSummaryText)
            ? briefing.ProjectedOutcomeSummaryText
            : context != null && IsMeaningfulSnapshotText(context.ProjectedOutcomeSummaryText)
                ? context.ProjectedOutcomeSummaryText
                : "None";
        data.ConsequencePreviewText = IsMeaningfulSnapshotText(data.ProjectedOutcomeSummaryText)
            ? data.ProjectedOutcomeSummaryText
            : data.ExpectedNeedImpactText;
        data.RoutePreviewSummaryText = context != null && IsMeaningfulSnapshotText(context.RoutePreviewSummaryText)
            ? context.RoutePreviewSummaryText
            : !string.IsNullOrEmpty(data.DungeonId) && !string.IsNullOrEmpty(data.SelectedRouteId)
                ? BuildRoutePreviewSummaryText(data.DungeonId, data.SelectedRouteId)
                : "None";
        data.RewardPreviewText = context != null && IsMeaningfulSnapshotText(context.RewardPreviewSummaryText)
            ? context.RewardPreviewSummaryText
            : "None";
        data.EventPreviewText = context != null && IsMeaningfulSnapshotText(context.EventPreviewSummaryText)
            ? context.EventPreviewSummaryText
            : "None";
        data.BlockedReasonText = readiness.GateResult != null &&
            readiness.GateResult.BlockedReason != null &&
            IsMeaningfulSnapshotText(readiness.GateResult.BlockedReason.SummaryText)
            ? readiness.GateResult.BlockedReason.SummaryText
            : "None";
        data.LaunchGateSummaryText = readiness.GateResult != null && IsMeaningfulSnapshotText(readiness.GateResult.SummaryText)
            ? readiness.GateResult.SummaryText
            : "None";
        data.ConfirmationStateText = data.CanLaunch && hasSelectedRoute
            ? "Launch confirmation is ready."
            : data.CanLaunch
                ? "Select a route before confirming the launch."
                : "Launch confirmation is blocked by the current world state.";
        data.CommitReasonText = IsMeaningfulSnapshotText(data.LaunchGateSummaryText)
            ? data.LaunchGateSummaryText
            : data.BlockedReasonText;
        data.RecommendedNextActionText = IsMeaningfulSnapshotText(readiness.RecommendedActionText)
            ? readiness.RecommendedActionText
            : "None";
        data.AlternateMoveText = data.RecommendedNextActionText;
        data.CityVacancyText = activeSnapshot != null &&
            activeSnapshot.TravelSummary != null &&
            IsMeaningfulSnapshotText(activeSnapshot.TravelSummary.CityVacancyLabel)
            ? activeSnapshot.TravelSummary.CityVacancyLabel
            : "None";
        data.ReturnEtaText = activeSnapshot != null &&
            activeSnapshot.TravelSummary != null &&
            IsMeaningfulSnapshotText(activeSnapshot.TravelSummary.ReturnEtaText)
            ? activeSnapshot.TravelSummary.ReturnEtaText
            : "None";
        data.ReturnWindowText = activeSnapshot != null &&
            activeSnapshot.ReturnWindow != null &&
            IsMeaningfulSnapshotText(activeSnapshot.ReturnWindow.ReturnWindowLabel)
            ? activeSnapshot.ReturnWindow.ReturnWindowLabel
            : "None";
        data.RecoveryAfterReturnText = activeSnapshot != null &&
            activeSnapshot.ReturnWindow != null &&
            IsMeaningfulSnapshotText(activeSnapshot.ReturnWindow.RecoveryAfterReturnLabel)
            ? activeSnapshot.ReturnWindow.RecoveryAfterReturnLabel
            : "None";
        data.LatestExpeditionResult = BuildExpeditionPrepLatestResult(data.CityId, data.DungeonId, data.CityLabel);
        data.AlternateMoveText = IsMeaningfulSnapshotText(data.LatestExpeditionResult.NextPrepFollowUpSummaryText)
            ? data.LatestExpeditionResult.NextPrepFollowUpSummaryText
            : IsMeaningfulSnapshotText(data.LatestExpeditionResult.NextSuggestedActionText)
                ? data.LatestExpeditionResult.NextSuggestedActionText
                : data.RecommendedNextActionText;
        data.FeedbackText = _isExpeditionPrepBoardOpen && IsMeaningfulSnapshotText(_expeditionPrepFeedbackText)
            ? _expeditionPrepFeedbackText
            : IsMeaningfulSnapshotText(data.LatestExpeditionResult.LatestReturnAftermathSummaryText)
                ? data.LatestExpeditionResult.LatestReturnAftermathSummaryText
                : data.RecommendedNextActionText;
        data.CanConfirmLaunch = _isExpeditionPrepBoardOpen && hasSelectedRoute && data.CanLaunch;
        data.RouteOptions = BuildExpeditionPrepRouteOptions(data.DungeonId, data.SelectedRouteId, data.RecommendedRouteId);
        return data;
    }

    private ExpeditionLaunchRequest BuildExpeditionLaunchRequestFromSurface(ExpeditionPrepSurfaceData source)
    {
        ExpeditionPrepSurfaceData data = source ?? new ExpeditionPrepSurfaceData();
        ExpeditionLaunchRequest request = new ExpeditionLaunchRequest();
        request.IsPreparationBoardOpen = data.IsBoardOpen;
        request.HasStagedParty = data.HasStagedParty;
        request.CanConfirmLaunch = data.CanConfirmLaunch;
        request.CityId = data.CityId ?? string.Empty;
        request.CityLabel = IsMeaningfulSnapshotText(data.CityLabel) ? data.CityLabel : "None";
        request.DungeonId = data.DungeonId ?? string.Empty;
        request.DungeonLabel = IsMeaningfulSnapshotText(data.DungeonLabel) ? data.DungeonLabel : "None";
        request.StagedPartyId = !string.IsNullOrEmpty(data.StagedPartyId) ? data.StagedPartyId : data.PartyId;
        request.StagedPartyLabel = IsMeaningfulSnapshotText(data.StagedPartyLabel) ? data.StagedPartyLabel : data.PartyLabel;
        request.SelectedRouteId = data.SelectedRouteId ?? string.Empty;
        request.SelectedRouteLabel = IsMeaningfulSnapshotText(data.SelectedRouteLabel) ? data.SelectedRouteLabel : "None";
        request.RecommendedRouteId = data.RecommendedRouteId ?? string.Empty;
        request.RecommendedRouteLabel = IsMeaningfulSnapshotText(data.RecommendedRouteLabel) ? data.RecommendedRouteLabel : "None";
        request.DispatchPolicyText = IsMeaningfulSnapshotText(data.DispatchPolicyText) ? data.DispatchPolicyText : "None";
        request.NeedPressureText = IsMeaningfulSnapshotText(data.NeedPressureText) ? data.NeedPressureText : "None";
        request.DispatchReadinessText = IsMeaningfulSnapshotText(data.DispatchReadinessText) ? data.DispatchReadinessText : "None";
        request.ExpectedNeedImpactText = IsMeaningfulSnapshotText(data.ExpectedNeedImpactText) ? data.ExpectedNeedImpactText : "None";
        request.BlockedReasonText = IsMeaningfulSnapshotText(data.BlockedReasonText) ? data.BlockedReasonText : "None";
        request.LaunchGateSummaryText = IsMeaningfulSnapshotText(data.LaunchGateSummaryText)
            ? data.LaunchGateSummaryText
            : request.BlockedReasonText;
        request.StartContext = CopyExpeditionStartContext(data.StartContext);
        return request;
    }

    private ExpeditionResult BuildExpeditionPrepLatestResult(string cityId, string dungeonId, string cityLabel)
    {
        ExpeditionResult latestExpeditionResult = _runtimeEconomyState != null && !string.IsNullOrEmpty(cityId)
            ? _runtimeEconomyState.GetLatestExpeditionResultForCity(cityId)
            : _runtimeEconomyState != null
                ? _runtimeEconomyState.GetLatestExpeditionResult()
                : null;
        if (latestExpeditionResult == null)
        {
            return new ExpeditionResult();
        }

        string resolvedCityId = !string.IsNullOrEmpty(cityId)
            ? cityId
            : latestExpeditionResult.SourceCityId;
        string resolvedDungeonId = !string.IsNullOrEmpty(dungeonId)
            ? dungeonId
            : latestExpeditionResult.DungeonId;
        string resolvedCityLabel = IsMeaningfulSnapshotText(cityLabel)
            ? cityLabel
            : ResolveDispatchEntityDisplayName(resolvedCityId);
        string nextSuggestedActionText = ResolveOutcomeReadbackNextSuggestedActionText(null, resolvedCityId, resolvedDungeonId);
        string latestReturnAftermathText = ResolveOutcomeReadbackLatestReturnAftermathText(null, resolvedCityId, resolvedDungeonId);
        CityWriteback cityWriteback = BuildCityWritebackSurfaceForCity(resolvedCityId, nextSuggestedActionText);
        OutcomeReadback outcomeReadback = BuildOutcomeReadbackSurfaceForContext(
            resolvedCityId,
            resolvedDungeonId,
            resolvedCityLabel,
            string.IsNullOrEmpty(resolvedCityId)
                ? "No city selected for ExpeditionPrep return consume."
                : resolvedCityLabel + " return consume is ready for ExpeditionPrep.",
            nextSuggestedActionText,
            latestReturnAftermathText,
            cityWriteback);
        WorldWriteback worldWriteback = _runtimeEconomyState != null && !string.IsNullOrEmpty(resolvedCityId)
            ? _runtimeEconomyState.GetLatestWorldWritebackForCity(resolvedCityId)
            : _runtimeEconomyState != null
                ? _runtimeEconomyState.GetLatestWorldWriteback()
                : new WorldWriteback();
        return ResultPipeline.BuildReturnConsumedExpeditionResult(
            latestExpeditionResult,
            cityWriteback,
            worldWriteback,
            outcomeReadback);
    }

    private ExpeditionPrepHandoff BuildExpeditionPrepHandoffFromSurface(ExpeditionPrepSurfaceData source)
    {
        ExpeditionPrepSurfaceData data = source ?? new ExpeditionPrepSurfaceData();
        ExpeditionPrepHandoff handoff = new ExpeditionPrepHandoff();
        handoff.IsReady = data.CanOpenBoard;
        handoff.CityId = data.CityId ?? string.Empty;
        handoff.CityLabel = IsMeaningfulSnapshotText(data.CityLabel) ? data.CityLabel : "None";
        handoff.DungeonId = data.DungeonId ?? string.Empty;
        handoff.DungeonLabel = IsMeaningfulSnapshotText(data.DungeonLabel) ? data.DungeonLabel : "None";
        handoff.PartyId = !string.IsNullOrEmpty(data.StagedPartyId) ? data.StagedPartyId : data.PartyId ?? string.Empty;
        handoff.PartyLabel = IsMeaningfulSnapshotText(data.StagedPartyLabel) ? data.StagedPartyLabel : data.PartyLabel;
        handoff.PartyLoadoutSummaryText = IsMeaningfulSnapshotText(data.PartyLoadoutSummaryText) ? data.PartyLoadoutSummaryText : "None";
        handoff.SelectedRouteId = data.SelectedRouteId ?? string.Empty;
        handoff.SelectedRouteLabel = IsMeaningfulSnapshotText(data.SelectedRouteLabel) ? data.SelectedRouteLabel : "None";
        handoff.RecommendedRouteId = data.RecommendedRouteId ?? string.Empty;
        handoff.RecommendedRouteLabel = IsMeaningfulSnapshotText(data.RecommendedRouteLabel) ? data.RecommendedRouteLabel : "None";
        handoff.RouteRiskText = data.StartContext != null && IsMeaningfulSnapshotText(data.StartContext.RouteRiskLabel)
            ? data.StartContext.RouteRiskLabel
            : "None";
        handoff.DispatchPolicyText = IsMeaningfulSnapshotText(data.DispatchPolicyText) ? data.DispatchPolicyText : "None";
        handoff.NeedPressureText = IsMeaningfulSnapshotText(data.NeedPressureText) ? data.NeedPressureText : "None";
        handoff.DispatchReadinessText = IsMeaningfulSnapshotText(data.DispatchReadinessText) ? data.DispatchReadinessText : "None";
        handoff.RecoveryProgressText = IsMeaningfulSnapshotText(data.RecoveryProgressText) ? data.RecoveryProgressText : "None";
        handoff.RecoveryEtaText = IsMeaningfulSnapshotText(data.RecoveryEtaText) ? data.RecoveryEtaText : "None";
        handoff.RecommendationReasonText = IsMeaningfulSnapshotText(data.RecommendationReasonText) ? data.RecommendationReasonText : "None";
        handoff.ExpectedNeedImpactText = IsMeaningfulSnapshotText(data.ExpectedNeedImpactText) ? data.ExpectedNeedImpactText : "None";
        handoff.LaunchReadiness = CopyLaunchReadiness(data.LaunchReadiness);
        handoff.StartContext = CopyExpeditionStartContext(data.StartContext);
        handoff.BlockedReasonText = IsMeaningfulSnapshotText(data.BlockedReasonText) ? data.BlockedReasonText : "None";
        handoff.WarningSummaryText = data.CanLaunch ? "None" : handoff.BlockedReasonText;
        handoff.RecommendedNextActionText = IsMeaningfulSnapshotText(data.RecommendedNextActionText) ? data.RecommendedNextActionText : "None";
        handoff.RouteFitSummaryText = IsMeaningfulSnapshotText(data.RouteFitSummaryText) ? data.RouteFitSummaryText : "None";
        handoff.LaunchGateSummaryText = IsMeaningfulSnapshotText(data.LaunchGateSummaryText) ? data.LaunchGateSummaryText : "None";
        handoff.ProjectedOutcomeSummaryText = IsMeaningfulSnapshotText(data.ProjectedOutcomeSummaryText) ? data.ProjectedOutcomeSummaryText : "None";
        handoff.RewardPreviewText = IsMeaningfulSnapshotText(data.RewardPreviewText) ? data.RewardPreviewText : "None";
        handoff.EventPreviewText = IsMeaningfulSnapshotText(data.EventPreviewText) ? data.EventPreviewText : "None";
        handoff.SummaryText = IsMeaningfulSnapshotText(data.BoardSummaryText) ? data.BoardSummaryText : "None";
        return handoff;
    }

    private ExpeditionPrepSurfaceData CopyExpeditionPrepSurfaceData(ExpeditionPrepSurfaceData source)
    {
        ExpeditionPrepSurfaceData data = new ExpeditionPrepSurfaceData();
        if (source == null)
        {
            return data;
        }

        data.IsBoardOpen = source.IsBoardOpen;
        data.CanOpenBoard = source.CanOpenBoard;
        data.CanCycleDispatchPolicy = source.CanCycleDispatchPolicy;
        data.HasStagedParty = source.HasStagedParty;
        data.CanLaunch = source.CanLaunch;
        data.CanConfirmLaunch = source.CanConfirmLaunch;
        data.BoardTitleText = source.BoardTitleText ?? "Expedition Prep";
        data.BoardSummaryText = source.BoardSummaryText ?? "None";
        data.FeedbackText = source.FeedbackText ?? "None";
        data.CityId = source.CityId ?? string.Empty;
        data.CityLabel = source.CityLabel ?? "None";
        data.DungeonId = source.DungeonId ?? string.Empty;
        data.DungeonLabel = source.DungeonLabel ?? "None";
        data.PartyId = source.PartyId ?? string.Empty;
        data.PartyLabel = source.PartyLabel ?? "None";
        data.StagedPartyId = source.StagedPartyId ?? string.Empty;
        data.StagedPartyLabel = source.StagedPartyLabel ?? "None";
        data.StagedPartySummaryText = source.StagedPartySummaryText ?? "None";
        data.TotalPartyCount = source.TotalPartyCount;
        data.IdlePartyCount = source.IdlePartyCount;
        data.ActiveExpeditionCount = source.ActiveExpeditionCount;
        data.PartyLoadoutSummaryText = source.PartyLoadoutSummaryText ?? "None";
        data.DispatchPolicyText = source.DispatchPolicyText ?? "None";
        data.NeedPressureText = source.NeedPressureText ?? "None";
        data.DispatchReadinessText = source.DispatchReadinessText ?? "None";
        data.RecoveryProgressText = source.RecoveryProgressText ?? "None";
        data.RecoveryEtaText = source.RecoveryEtaText ?? "None";
        data.SelectedRouteId = source.SelectedRouteId ?? string.Empty;
        data.SelectedRouteLabel = source.SelectedRouteLabel ?? "None";
        data.RecommendedRouteId = source.RecommendedRouteId ?? string.Empty;
        data.RecommendedRouteLabel = source.RecommendedRouteLabel ?? "None";
        data.RecommendedRouteSummaryText = source.RecommendedRouteSummaryText ?? "None";
        data.RecommendationReasonText = source.RecommendationReasonText ?? "None";
        data.ExpectedNeedImpactText = source.ExpectedNeedImpactText ?? "None";
        data.DispatchBriefingSummaryText = source.DispatchBriefingSummaryText ?? "None";
        data.RouteFitSummaryText = source.RouteFitSummaryText ?? "None";
        data.ProjectedOutcomeSummaryText = source.ProjectedOutcomeSummaryText ?? "None";
        data.ConsequencePreviewText = source.ConsequencePreviewText ?? "None";
        data.RoutePreviewSummaryText = source.RoutePreviewSummaryText ?? "None";
        data.RewardPreviewText = source.RewardPreviewText ?? "None";
        data.EventPreviewText = source.EventPreviewText ?? "None";
        data.BlockedReasonText = source.BlockedReasonText ?? "None";
        data.LaunchGateSummaryText = source.LaunchGateSummaryText ?? "None";
        data.ConfirmationStateText = source.ConfirmationStateText ?? "None";
        data.CommitReasonText = source.CommitReasonText ?? "None";
        data.AlternateMoveText = source.AlternateMoveText ?? "None";
        data.RecommendedNextActionText = source.RecommendedNextActionText ?? "None";
        data.CityVacancyText = source.CityVacancyText ?? "None";
        data.ReturnEtaText = source.ReturnEtaText ?? "None";
        data.ReturnWindowText = source.ReturnWindowText ?? "None";
        data.RecoveryAfterReturnText = source.RecoveryAfterReturnText ?? "None";
        data.LatestExpeditionResult = ResultPipeline.BuildReturnConsumedExpeditionResult(
            source.LatestExpeditionResult,
            null,
            null,
            null);
        data.LaunchReadiness = CopyLaunchReadiness(source.LaunchReadiness);
        data.StartContext = CopyExpeditionStartContext(source.StartContext);
        data.RouteOptions = CopyExpeditionPrepRouteOptions(source.RouteOptions);
        return data;
    }

    private LaunchReadiness CopyLaunchReadiness(LaunchReadiness source)
    {
        LaunchReadiness data = new LaunchReadiness();
        if (source == null)
        {
            return data;
        }

        data.IsReady = source.IsReady;
        data.ReadinessKey = source.ReadinessKey ?? "none";
        data.ReadinessText = source.ReadinessText ?? "None";
        data.DispatchPolicyText = source.DispatchPolicyText ?? "None";
        data.NeedPressureText = source.NeedPressureText ?? "None";
        data.RecoveryProgressText = source.RecoveryProgressText ?? "None";
        data.RecoveryEtaText = source.RecoveryEtaText ?? "None";
        data.RecommendationReasonText = source.RecommendationReasonText ?? "None";
        data.ExpectedNeedImpactText = source.ExpectedNeedImpactText ?? "None";
        data.SummaryText = source.SummaryText ?? "None";
        data.RecommendedActionText = source.RecommendedActionText ?? "None";
        data.GateResult = CopyGateResult(source.GateResult);
        return data;
    }

    private GateResult CopyGateResult(GateResult source)
    {
        GateResult data = new GateResult();
        if (source == null)
        {
            return data;
        }

        data.CanLaunch = source.CanLaunch;
        data.IsBlocked = source.IsBlocked;
        data.SummaryText = source.SummaryText ?? "None";
        data.RecommendedActionText = source.RecommendedActionText ?? "None";
        data.BlockedReason = CopyBlockedReason(source.BlockedReason);
        return data;
    }

    private BlockedReason CopyBlockedReason(BlockedReason source)
    {
        BlockedReason data = new BlockedReason();
        if (source == null)
        {
            return data;
        }

        data.ReasonCode = source.ReasonCode ?? string.Empty;
        data.IsBlocking = source.IsBlocking;
        data.SummaryText = source.SummaryText ?? "None";
        return data;
    }

    private ExpeditionPrepRouteOptionData[] CopyExpeditionPrepRouteOptions(ExpeditionPrepRouteOptionData[] source)
    {
        if (source == null || source.Length <= 0)
        {
            return new ExpeditionPrepRouteOptionData[0];
        }

        ExpeditionPrepRouteOptionData[] data = new ExpeditionPrepRouteOptionData[source.Length];
        for (int i = 0; i < source.Length; i++)
        {
            ExpeditionPrepRouteOptionData option = source[i] ?? new ExpeditionPrepRouteOptionData();
            data[i] = new ExpeditionPrepRouteOptionData
            {
                OptionKey = option.OptionKey ?? string.Empty,
                OptionLabel = option.OptionLabel ?? "None",
                RouteRiskText = option.RouteRiskText ?? "None",
                RoutePreviewText = option.RoutePreviewText ?? "None",
                RewardPreviewText = option.RewardPreviewText ?? "None",
                EventPreviewText = option.EventPreviewText ?? "None",
                IsSelected = option.IsSelected,
                IsRecommended = option.IsRecommended
            };
        }

        return data;
    }

    private ExpeditionPrepRouteOptionData[] BuildExpeditionPrepRouteOptions(string dungeonId, string selectedRouteId, string recommendedRouteId)
    {
        if (string.IsNullOrEmpty(dungeonId))
        {
            return new ExpeditionPrepRouteOptionData[0];
        }

        string normalizedSelectedRouteId = NormalizeRouteChoiceId(selectedRouteId);
        string normalizedRecommendedRouteId = NormalizeRouteChoiceId(recommendedRouteId);
        return new[]
        {
            BuildExpeditionPrepRouteOptionData(dungeonId, SafeRouteId, normalizedSelectedRouteId, normalizedRecommendedRouteId),
            BuildExpeditionPrepRouteOptionData(dungeonId, RiskyRouteId, normalizedSelectedRouteId, normalizedRecommendedRouteId)
        };
    }

    private ExpeditionPrepRouteOptionData BuildExpeditionPrepRouteOptionData(string dungeonId, string routeId, string selectedRouteId, string recommendedRouteId)
    {
        ExpeditionPrepRouteOptionData option = new ExpeditionPrepRouteOptionData();
        option.OptionKey = routeId;
        option.OptionLabel = BuildRouteButtonLabel(dungeonId, routeId);
        option.RouteRiskText = BuildRouteRiskSummaryForRoute(dungeonId, routeId);
        option.RoutePreviewText = BuildRoutePreviewSummaryText(dungeonId, routeId);
        option.RewardPreviewText = BuildRouteRewardPreviewEntryText(dungeonId, routeId);
        option.EventPreviewText = BuildRouteCombatPlanEntryText(dungeonId, routeId);
        option.IsSelected = selectedRouteId == routeId;
        option.IsRecommended = recommendedRouteId == routeId;
        return option;
    }

    private string BuildDispatchReadinessKey(string cityId)
    {
        if (string.IsNullOrEmpty(cityId))
        {
            return "none";
        }

        switch (GetDispatchReadinessState(cityId))
        {
            case DispatchReadinessState.Ready:
                return "ready";
            case DispatchReadinessState.Recovering:
                return "recovering";
            case DispatchReadinessState.Strained:
                return "strained";
            default:
                return "none";
        }
    }

    private BlockedReason BuildBlockedReason(PrototypeWorldDispatchBriefingSnapshot briefing)
    {
        BlockedReason reason = new BlockedReason();
        if (briefing == null || briefing.HardBlockerKeys == null || briefing.HardBlockerKeys.Count <= 0)
        {
            return reason;
        }

        reason.ReasonCode = briefing.HardBlockerKeys[0] ?? string.Empty;
        reason.IsBlocking = !string.IsNullOrEmpty(reason.ReasonCode);
        reason.SummaryText = reason.IsBlocking
            ? BuildDispatchBlockerReasonText(reason.ReasonCode)
            : "None";
        return reason;
    }

    private GateResult BuildGateResult(PrototypeWorldSnapshot snapshot, PrototypeWorldDispatchBriefingSnapshot briefing)
    {
        GateResult gate = new GateResult();
        BlockedReason blockedReason = BuildBlockedReason(briefing);
        gate.BlockedReason = blockedReason;
        gate.CanLaunch = briefing != null && briefing.CommitAllowed;
        gate.IsBlocked = !gate.CanLaunch;
        gate.SummaryText = briefing != null && IsMeaningfulSnapshotText(briefing.LaunchLockSummaryText)
            ? briefing.LaunchLockSummaryText
            : blockedReason.SummaryText;
        gate.RecommendedActionText = snapshot != null &&
            snapshot.ModifierSummary != null &&
            IsMeaningfulSnapshotText(snapshot.ModifierSummary.RecommendationHintText)
            ? snapshot.ModifierSummary.RecommendationHintText
            : gate.IsBlocked
                ? "Resolve the current launch blocker before committing the expedition."
                : "Current launch context matches the latest world readback.";
        return gate;
    }

    private LaunchReadiness BuildLaunchReadiness(PrototypeWorldSnapshot snapshot, PrototypeWorldDispatchBriefingSnapshot briefing)
    {
        LaunchReadiness readiness = new LaunchReadiness();
        string cityId = snapshot != null ? snapshot.SelectedCityId : string.Empty;
        string dungeonId = snapshot != null ? snapshot.SelectedDungeonId : string.Empty;
        string routeId = snapshot != null && IsMeaningfulSnapshotText(snapshot.SelectedRouteId)
            ? snapshot.SelectedRouteId
            : snapshot != null
                ? snapshot.RecommendedRouteId
                : string.Empty;
        readiness.ReadinessKey = BuildDispatchReadinessKey(cityId);
        readiness.ReadinessText = snapshot != null &&
            snapshot.CitySummary != null &&
            IsMeaningfulSnapshotText(snapshot.CitySummary.ReadinessText)
            ? snapshot.CitySummary.ReadinessText
            : "None";
        readiness.DispatchPolicyText = snapshot != null &&
            snapshot.CitySummary != null &&
            IsMeaningfulSnapshotText(snapshot.CitySummary.DispatchPolicyText)
            ? snapshot.CitySummary.DispatchPolicyText
            : "None";
        readiness.NeedPressureText = snapshot != null &&
            snapshot.CitySummary != null &&
            IsMeaningfulSnapshotText(snapshot.CitySummary.NeedPressureText)
            ? snapshot.CitySummary.NeedPressureText
            : "None";
        readiness.RecoveryProgressText = snapshot != null &&
            snapshot.CitySummary != null &&
            IsMeaningfulSnapshotText(snapshot.CitySummary.RecoveryText)
            ? snapshot.CitySummary.RecoveryText
            : "None";
        readiness.RecoveryEtaText = !string.IsNullOrEmpty(cityId)
            ? BuildRecoveryEtaText(GetRecoveryDaysToReady(cityId))
            : "None";
        readiness.RecommendationReasonText = !string.IsNullOrEmpty(cityId) && !string.IsNullOrEmpty(dungeonId)
            ? BuildRecommendationReasonText(cityId, dungeonId)
            : "None";
        readiness.ExpectedNeedImpactText = !string.IsNullOrEmpty(cityId) && !string.IsNullOrEmpty(dungeonId) && !string.IsNullOrEmpty(routeId)
            ? BuildExpectedNeedImpactText(cityId, dungeonId, routeId)
            : "None";
        readiness.GateResult = BuildGateResult(snapshot, briefing);
        readiness.IsReady = readiness.GateResult.CanLaunch;
        readiness.RecommendedActionText = readiness.GateResult.RecommendedActionText;
        readiness.SummaryText = readiness.ReadinessText == "None"
            ? readiness.GateResult.SummaryText
            : readiness.ReadinessText + " | " + readiness.GateResult.SummaryText;
        return readiness;
    }

    private LaunchReadiness BuildProjectedExpeditionLaunchReadiness()
    {
        string cityId = ResolveDispatchBriefingCityId();
        string dungeonId = ResolveDispatchBriefingDungeonId(cityId);
        string routeId = ResolveWorldSnapshotRouteId(cityId, dungeonId);
        bool requireSelectedRoute = IsExpeditionPrepRouteSelectionActive();
        PrototypeWorldDispatchBriefingSnapshot briefing = BuildDispatchBriefingSnapshot(cityId, dungeonId, routeId, requireSelectedRoute);
        PrototypeWorldSnapshot snapshot = BuildWorldSnapshot(cityId, dungeonId, routeId, briefing);
        return BuildLaunchReadiness(snapshot, briefing);
    }

    private LaunchReadiness BuildPlannerLaunchReadiness()
    {
        string routeId = NormalizeRouteChoiceId(_selectedRouteChoiceId);
        PrototypeWorldDispatchBriefingSnapshot briefing = BuildDispatchBriefingSnapshot(_currentHomeCityId, _currentDungeonId, routeId, true);
        PrototypeWorldSnapshot snapshot = BuildWorldSnapshot(_currentHomeCityId, _currentDungeonId, routeId, briefing);
        return BuildLaunchReadiness(snapshot, briefing);
    }

    private string BuildPartyLoadoutSummaryText(string partyId)
    {
        if (_runtimeEconomyState != null)
        {
            string cachedLoadoutSummary = _runtimeEconomyState.GetPartyLoadoutSummary(partyId);
            if (IsMeaningfulSnapshotText(cachedLoadoutSummary))
            {
                return cachedLoadoutSummary;
            }
        }

        return BuildPartyLoadoutSummaryText(BuildExpeditionPartyRuntimeResolveSurface(partyId));
    }

    private PrototypeRpgPartyRuntimeResolveSurface BuildExpeditionPartyRuntimeResolveSurface(string partyId)
    {
        return BuildRuntimePartyResolveSurface(partyId);
    }

    private ExpeditionPartyManifest BuildExpeditionPartyManifest(
        string partyId,
        string fallbackPartyLabel,
        bool includeDetailedPartyReadbacks = false)
    {
        ExpeditionPartyManifest manifest = new ExpeditionPartyManifest();
        manifest.PartyId = string.IsNullOrEmpty(partyId) ? string.Empty : partyId;
        manifest.PartyLabel = IsMeaningfulSnapshotText(fallbackPartyLabel) ? fallbackPartyLabel : "None";
        bool includeDetailed = includeDetailedPartyReadbacks || _isExpeditionPrepBoardOpen;

        if (_runtimeEconomyState != null)
        {
            string displayName = _runtimeEconomyState.GetPartyDisplayName(partyId);
            if (IsMeaningfulSnapshotText(displayName))
            {
                manifest.PartyLabel = displayName;
            }

            manifest.MemberCount = _runtimeEconomyState.GetPartyMemberCount(partyId);
            manifest.RoleSummaryText = _runtimeEconomyState.GetPartyRoleSummary(partyId);
            manifest.LoadoutSummaryText = _runtimeEconomyState.GetPartyLoadoutSummary(partyId);
        }

        if (!includeDetailed)
        {
            manifest.ManifestSummaryText = BuildExpeditionPartyManifestSummaryText(manifest);
            return manifest;
        }

        PrototypeRpgPartyRuntimeResolveSurface partySurface = BuildExpeditionPartyRuntimeResolveSurface(partyId);
        if (partySurface == null)
        {
            manifest.ManifestSummaryText = BuildExpeditionPartyManifestSummaryText(manifest);
            return manifest;
        }

        manifest.PartyLabel = IsMeaningfulSnapshotText(partySurface.DisplayName) ? partySurface.DisplayName : manifest.PartyLabel;
        manifest.MemberCount = partySurface.Members != null ? partySurface.Members.Length : 0;
        manifest.RoleSummaryText = PrototypeRpgRoleIdentity.BuildPartyRoleSummary(
            partySurface.Members,
            member => member != null ? member.DisplayName : string.Empty,
            member => member != null ? member.RoleTag : string.Empty);
        manifest.LoadoutSummaryText = BuildPartyLoadoutSummaryText(partySurface);
        manifest.MemberSummaryText = BuildExpeditionPartyMemberSummaryText(partySurface.Members);
        manifest.AppliedProgressionSummaryText = IsMeaningfulSnapshotText(partySurface.AppliedProgressionSummaryText)
            ? partySurface.AppliedProgressionSummaryText
            : "None";
        manifest.CurrentRunSummaryText = IsMeaningfulSnapshotText(partySurface.CurrentRunSummaryText)
            ? partySurface.CurrentRunSummaryText
            : "None";
        manifest.NextRunPreviewSummaryText = IsMeaningfulSnapshotText(partySurface.NextRunPreviewSummaryText)
            ? partySurface.NextRunPreviewSummaryText
            : "None";
        manifest.Members = BuildExpeditionPartyMemberManifestArray(partySurface.Members);
        manifest.ManifestSummaryText = BuildExpeditionPartyManifestSummaryText(manifest);
        return manifest;
    }

    private ExpeditionPartyMemberManifest[] BuildExpeditionPartyMemberManifestArray(PrototypeRpgMemberRuntimeResolveSurface[] members)
    {
        if (members == null || members.Length <= 0)
        {
            return new ExpeditionPartyMemberManifest[0];
        }

        ExpeditionPartyMemberManifest[] manifest = new ExpeditionPartyMemberManifest[members.Length];
        for (int i = 0; i < members.Length; i++)
        {
            manifest[i] = BuildExpeditionPartyMemberManifest(members[i]);
        }

        return manifest;
    }

    private ExpeditionPartyMemberManifest BuildExpeditionPartyMemberManifest(PrototypeRpgMemberRuntimeResolveSurface member)
    {
        ExpeditionPartyMemberManifest manifest = new ExpeditionPartyMemberManifest();
        if (member == null)
        {
            return manifest;
        }

        manifest.MemberId = string.IsNullOrEmpty(member.MemberId) ? string.Empty : member.MemberId;
        manifest.DisplayName = string.IsNullOrEmpty(member.DisplayName) ? "Adventurer" : member.DisplayName;
        manifest.RoleTag = string.IsNullOrEmpty(member.RoleTag) ? "adventurer" : member.RoleTag;
        manifest.RoleLabel = string.IsNullOrEmpty(member.RoleLabel) ? "Adventurer" : member.RoleLabel;
        manifest.RoleIdentityText = PrototypeRpgRoleIdentity.BuildRoleIdentityText(manifest.RoleTag);
        manifest.GearPreferenceText = PrototypeRpgRoleIdentity.BuildGearPreferenceText(manifest.RoleTag);
        manifest.BattleHintText = PrototypeRpgRoleIdentity.BuildBattleHintText(manifest.RoleTag);
        manifest.PartySlotIndex = member.PartySlotIndex;
        manifest.GrowthTrackId = string.IsNullOrEmpty(member.GrowthTrackId) ? string.Empty : member.GrowthTrackId;
        manifest.JobId = string.IsNullOrEmpty(member.JobId) ? string.Empty : member.JobId;
        manifest.EquipmentLoadoutId = string.IsNullOrEmpty(member.EquipmentLoadoutId) ? string.Empty : member.EquipmentLoadoutId;
        manifest.SkillLoadoutId = string.IsNullOrEmpty(member.SkillLoadoutId) ? string.Empty : member.SkillLoadoutId;
        manifest.DefaultSkillId = string.IsNullOrEmpty(member.DefaultSkillId) ? string.Empty : member.DefaultSkillId;
        manifest.ResolvedSkillName = string.IsNullOrEmpty(member.ResolvedSkillName) ? "Skill" : member.ResolvedSkillName;
        manifest.ResolvedSkillShortText = string.IsNullOrEmpty(member.ResolvedSkillShortText) ? "None" : member.ResolvedSkillShortText;
        manifest.EquipmentSummaryText = string.IsNullOrEmpty(member.EquipmentSummaryText) ? "No gear" : member.EquipmentSummaryText;
        manifest.GearContributionSummaryText = string.IsNullOrEmpty(member.GearContributionSummaryText) ? "No bonus" : member.GearContributionSummaryText;
        manifest.AppliedProgressionSummaryText = string.IsNullOrEmpty(member.AppliedProgressionSummaryText) ? "None" : member.AppliedProgressionSummaryText;
        manifest.CurrentRunSummaryText = string.IsNullOrEmpty(member.CurrentRunSummaryText) ? "None" : member.CurrentRunSummaryText;
        manifest.NextRunPreviewSummaryText = string.IsNullOrEmpty(member.NextRunPreviewSummaryText) ? "None" : member.NextRunPreviewSummaryText;
        manifest.Level = member.Level > 0 ? member.Level : 1;
        manifest.CurrentExperience = Mathf.Max(0, member.CurrentExperience);
        manifest.NextLevelExperience = member.NextLevelExperience > 0
            ? member.NextLevelExperience
            : PrototypeRpgMemberProgressionRules.GetNextLevelExperience(manifest.Level);
        manifest.MaxHp = Mathf.Max(1, member.MaxHp);
        manifest.Attack = Mathf.Max(1, member.Attack);
        manifest.Defense = Mathf.Max(0, member.Defense);
        manifest.Speed = Mathf.Max(0, member.Speed);
        manifest.SkillPower = Mathf.Max(1, member.SkillPower);
        manifest.SummaryText = BuildExpeditionPartyMemberSummaryLine(manifest);
        return manifest;
    }

    private string BuildExpeditionPartyMemberSummaryText(PrototypeRpgMemberRuntimeResolveSurface[] members)
    {
        if (members == null || members.Length <= 0)
        {
            return "None";
        }

        string summary = string.Empty;
        for (int i = 0; i < members.Length; i++)
        {
            PrototypeRpgMemberRuntimeResolveSurface member = members[i];
            if (member == null)
            {
                continue;
            }

            string part = BuildExpeditionPartyMemberSummaryLine(BuildExpeditionPartyMemberManifest(member));
            summary = string.IsNullOrEmpty(summary) ? part : summary + "\n" + part;
        }

        return string.IsNullOrEmpty(summary) ? "None" : summary;
    }

    private string BuildExpeditionPartyManifestSummaryText(ExpeditionPartyManifest manifest)
    {
        if (manifest == null)
        {
            return "None";
        }

        string partyLabel = IsMeaningfulSnapshotText(manifest.PartyLabel) ? manifest.PartyLabel : "Party";
        string roleSummaryText = IsMeaningfulSnapshotText(manifest.RoleSummaryText) ? manifest.RoleSummaryText : "None";
        string partyIdentityText = ExtractRuntimeSummaryClauseText(manifest.AppliedProgressionSummaryText, "Party");
        string nextEdgeText = ExtractRuntimeSummaryClauseText(manifest.NextRunPreviewSummaryText, "Next Edge");
        string battlePlanText = ExtractRuntimeSummaryClauseText(manifest.CurrentRunSummaryText, "Battle Plan");
        string previewText = ChooseRuntimePartySummaryText(
            roleSummaryText,
            ChooseRuntimePartySummaryText(
                partyIdentityText,
                ChooseRuntimePartySummaryText(nextEdgeText, battlePlanText)));
        if (!IsMeaningfulSnapshotText(previewText))
        {
            previewText = manifest.MemberSummaryText;
        }

        return partyLabel + " | " + Mathf.Max(0, manifest.MemberCount) + " members ready | " + previewText;
    }

    private string BuildExpeditionLaunchManifestSummaryText(ExpeditionStartContext context)
    {
        if (context == null)
        {
            return "None";
        }

        string selectedRoute = IsMeaningfulSnapshotText(context.SelectedRouteLabel) ? context.SelectedRouteLabel : "No route selected";
        string recommendedRoute = IsMeaningfulSnapshotText(context.RecommendedRouteLabel) ? context.RecommendedRouteLabel : "None";
        string routeDecision = context.FollowedRecommendation
            ? "Following recommendation"
            : IsMeaningfulSnapshotText(context.SelectedRouteId)
                ? "Route override locked"
                : "Route pending";
        ExpeditionPartyManifest manifest = context.PartyManifest ?? new ExpeditionPartyManifest();
        string partySummary = IsMeaningfulSnapshotText(manifest.ManifestSummaryText)
            ? manifest.ManifestSummaryText
            : IsMeaningfulSnapshotText(context.StagedPartySummaryText)
                ? context.StagedPartySummaryText
                : "No staged party";
        return "Selected " + selectedRoute +
            " | Recommended " + recommendedRoute +
            " | " + routeDecision +
            " | " + partySummary +
            " | Fit " + (IsMeaningfulSnapshotText(context.RouteFitSummaryText) ? context.RouteFitSummaryText : "None");
    }

    private string BuildExpeditionPartyMemberSummaryLine(ExpeditionPartyMemberManifest manifest)
    {
        if (manifest == null)
        {
            return "None";
        }

        string displayName = IsMeaningfulSnapshotText(manifest.DisplayName) ? manifest.DisplayName : "Adventurer";
        return displayName + " | " +
            PrototypeRpgRoleIdentity.BuildRoleIdentityLabel(manifest.RoleLabel, manifest.RoleTag) + " | " +
            manifest.GearPreferenceText + " | " +
            "Skill " + (IsMeaningfulSnapshotText(manifest.ResolvedSkillName) ? manifest.ResolvedSkillName : "Skill");
    }

    private string BuildExpeditionPartyMemberLevelText(ExpeditionPartyMemberManifest manifest)
    {
        return PrototypeRpgMemberProgressionRules.BuildLevelProgressText(
            manifest != null ? manifest.Level : 1,
            manifest != null ? manifest.CurrentExperience : 0,
            manifest != null ? manifest.NextLevelExperience : PrototypeRpgMemberProgressionRules.GetNextLevelExperience(1));
    }

    private string BuildExpeditionPartyMemberStatText(ExpeditionPartyMemberManifest manifest)
    {
        if (manifest == null)
        {
            return "HP 1 ATK 1 DEF 0 SPD 0";
        }

        return "HP " + Mathf.Max(1, manifest.MaxHp) +
            " ATK " + Mathf.Max(1, manifest.Attack) +
            " DEF " + Mathf.Max(0, manifest.Defense) +
            " SPD " + Mathf.Max(0, manifest.Speed);
    }

    private string BuildExpeditionPartyMemberGearReadback(ExpeditionPartyMemberManifest manifest)
    {
        string gearText = PrototypeRpgEquipmentCatalog.BuildCompactReadbackText(
            manifest != null ? manifest.EquipmentSummaryText : "No gear",
            manifest != null ? manifest.GearContributionSummaryText : "No bonus");
        return "Gear: " + gearText;
    }

    private string BuildPartyLoadoutSummaryText(PrototypeRpgPartyRuntimeResolveSurface partySurface)
    {
        if (partySurface == null || partySurface.Members == null || partySurface.Members.Length <= 0)
        {
            return "None";
        }

        string summary = string.Empty;
        for (int i = 0; i < partySurface.Members.Length; i++)
        {
            PrototypeRpgMemberRuntimeResolveSurface member = partySurface.Members[i];
            if (member == null)
            {
                continue;
            }

            string label = string.IsNullOrEmpty(member.DisplayName) ? member.RoleLabel : member.DisplayName;
            string gear = _runtimeEconomyState != null
                ? _runtimeEconomyState.GetPartyMemberEquipmentSlotSummary(partySurface.PartyId, member.MemberId)
                : string.Empty;
            if (!IsMeaningfulSnapshotText(gear))
            {
                gear = string.IsNullOrEmpty(member.EquipmentSummaryText) ? "No gear" : member.EquipmentSummaryText;
            }

            string part = label + ": " + gear;
            summary = string.IsNullOrEmpty(summary) ? part : summary + " | " + part;
        }

        string inventorySummary = _runtimeEconomyState != null
            ? _runtimeEconomyState.GetPartyInventorySummary(partySurface.PartyId)
            : string.Empty;
        if (IsMeaningfulSnapshotText(inventorySummary))
        {
            summary = string.IsNullOrEmpty(summary)
                ? "Inventory " + inventorySummary
                : summary + " | Inventory " + inventorySummary;
        }

        return string.IsNullOrEmpty(summary) ? "None" : summary;
    }

}
