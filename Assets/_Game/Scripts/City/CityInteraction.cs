using UnityEngine;

public sealed class CityInteraction
{
    private readonly StaticPlaceholderWorldView _worldView;
    private readonly GameFlowCoordinator _gameFlowCoordinator;
    private readonly System.Func<Camera> _resolveMainCamera;
    private readonly ExpeditionPrepGateway _expeditionPrepGateway;

    public CityInteraction(
        StaticPlaceholderWorldView worldView,
        GameFlowCoordinator gameFlowCoordinator,
        System.Func<Camera> resolveMainCamera)
    {
        _worldView = worldView;
        _gameFlowCoordinator = gameFlowCoordinator;
        _resolveMainCamera = resolveMainCamera;
        _expeditionPrepGateway = new ExpeditionPrepGateway(worldView);
    }

    private bool IsWorldSimActive => _gameFlowCoordinator != null && _gameFlowCoordinator.CurrentState == GameStateId.WorldSim;

    public CityHubSurfaceData GetCityHubSurfaceData()
    {
        return _worldView != null ? _worldView.BuildSelectedCityHubSurfaceData() : new CityHubSurfaceData();
    }

    public WorldSimCitySourceData GetEntrySnapshot()
    {
        return _worldView != null ? _worldView.BuildSelectedCityHubEntrySnapshot() : new WorldSimCitySourceData();
    }

    public CityInteractionSurfaceData GetInteractionSurfaceData()
    {
        CityHubSurfaceData cityHub = GetCityHubSurfaceData();
        return cityHub != null && cityHub.CityInteraction != null
            ? cityHub.CityInteraction
            : new CityInteractionSurfaceData();
    }

    public CityPartyRosterSurfaceData GetPartyRosterSurfaceData()
    {
        CityHubSurfaceData cityHub = GetCityHubSurfaceData();
        return cityHub != null && cityHub.PartyRoster != null
            ? cityHub.PartyRoster
            : new CityPartyRosterSurfaceData();
    }

    public ExpeditionPrepSurfaceData GetExpeditionPrepSurfaceData()
    {
        return _expeditionPrepGateway != null
            ? _expeditionPrepGateway.GetSurfaceData()
            : new ExpeditionPrepSurfaceData();
    }

    public ExpeditionLaunchRequest GetExpeditionLaunchRequest()
    {
        return _expeditionPrepGateway != null
            ? _expeditionPrepGateway.GetLaunchRequest()
            : new ExpeditionLaunchRequest();
    }

    public ExpeditionPrepHandoff GetExpeditionPrepHandoff()
    {
        CityHubSurfaceData cityHub = GetCityHubSurfaceData();
        return cityHub != null && cityHub.ExpeditionPrep != null
            ? cityHub.ExpeditionPrep
            : new ExpeditionPrepHandoff();
    }

    public OutcomeReadback GetOutcomeReadbackSurface()
    {
        CityHubSurfaceData cityHub = GetCityHubSurfaceData();
        return cityHub != null && cityHub.ResultPipelineReadback != null
            ? cityHub.ResultPipelineReadback
            : new OutcomeReadback();
    }

    public CityWriteback GetCityWritebackSurface()
    {
        CityHubSurfaceData cityHub = GetCityHubSurfaceData();
        return cityHub != null && cityHub.ResultPipelineCityWriteback != null
            ? cityHub.ResultPipelineCityWriteback
            : new CityWriteback();
    }

    public PrototypeCityHubUiSurfaceData BuildUiSurfaceData()
    {
        WorldObservationSurfaceData observation = _worldView != null
            ? _worldView.BuildWorldObservationSurfaceData()
            : new WorldObservationSurfaceData();
        return BuildUiSurfaceData(observation);
    }

    public PrototypeCityHubUiSurfaceData BuildUiSurfaceData(WorldObservationSurfaceData observation)
    {
        WorldObservationSurfaceData safeObservation = observation ?? new WorldObservationSurfaceData();
        CityHubSurfaceData cityHub = safeObservation.CityHub != null
            ? safeObservation.CityHub
            : GetCityHubSurfaceData();
        WorldSimCitySourceData entry = cityHub != null && cityHub.EntrySnapshot != null
            ? cityHub.EntrySnapshot
            : new WorldSimCitySourceData();
        CitySimSurfaceData citySim = cityHub != null && cityHub.CitySim != null
            ? cityHub.CitySim
            : new CitySimSurfaceData();
        CityInteractionSurfaceData interaction = cityHub != null && cityHub.CityInteraction != null
            ? cityHub.CityInteraction
            : new CityInteractionSurfaceData();
        CityPartyRosterSurfaceData roster = cityHub != null && cityHub.PartyRoster != null
            ? cityHub.PartyRoster
            : new CityPartyRosterSurfaceData();
        ExpeditionPrepSurfaceData prep = safeObservation.ExpeditionPrep != null
            ? safeObservation.ExpeditionPrep
            : new ExpeditionPrepSurfaceData();
        OutcomeReadback readback = cityHub != null && cityHub.ResultPipelineReadback != null
            ? cityHub.ResultPipelineReadback
            : new OutcomeReadback();
        CityWriteback writeback = cityHub != null && cityHub.ResultPipelineCityWriteback != null
            ? cityHub.ResultPipelineCityWriteback
            : new CityWriteback();
        WorldObservationPriorityBoardData priorityBoard = safeObservation.PriorityBoard != null
            ? safeObservation.PriorityBoard
            : new WorldObservationPriorityBoardData();
        ExpeditionPostRunRevealState postRunReveal = safeObservation.RecentOutcome != null &&
            safeObservation.RecentOutcome.PostRunReveal != null
            ? safeObservation.RecentOutcome.PostRunReveal
            : new ExpeditionPostRunRevealState();

        PrototypeCityHubUiSurfaceData data = new PrototypeCityHubUiSurfaceData();
        data.HasSelectedCity = entry.HasSelectedCity;
        data.Header.SelectionLabel = HasMeaningfulText(entry.SelectedCityLabel)
            ? entry.SelectedCityLabel
            : HasMeaningfulText(safeObservation.SelectedEntity.SelectedEntityLabel)
                ? safeObservation.SelectedEntity.SelectedEntityLabel
                : "None";
        data.Header.LastTransitionText = _gameFlowCoordinator != null ? _gameFlowCoordinator.LastTransition : "(missing)";
        data.Header.WorldDayCount = safeObservation.WorldDayCount;
        data.Header.AutoTickEnabled = safeObservation.AutoTickEnabled;
        data.Header.ActiveExpeditions = safeObservation.ActiveExpeditions;
        data.Header.IdleParties = safeObservation.IdleParties;

        data.AlertRibbon.Title = ResolveAlertTitle(postRunReveal, readback, priorityBoard);
        data.AlertRibbon.SummaryText = ResolveAlertSummary(postRunReveal, readback, priorityBoard);
        data.AlertRibbon.FooterText = ResolveAlertFooter(postRunReveal, priorityBoard);

        data.Selection.HasSelection = HasMeaningfulText(safeObservation.SelectedEntity.SelectedEntityLabel);
        data.Selection.IsCitySelection = entry.HasSelectedCity;
        data.Selection.DisplayName = data.Selection.IsCitySelection
            ? (HasMeaningfulText(entry.SelectedCityLabel) ? entry.SelectedCityLabel : "None")
            : (HasMeaningfulText(safeObservation.SelectedEntity.SelectedEntityLabel) ? safeObservation.SelectedEntity.SelectedEntityLabel : "None");
        data.Selection.TypeLabel = data.Selection.IsCitySelection
            ? "City"
            : (HasMeaningfulText(safeObservation.SelectedEntity.SelectedKindLabel) ? safeObservation.SelectedEntity.SelectedKindLabel : "None");
        data.Selection.CityManaShardStockText = HasMeaningfulText(entry.CityManaShardStockText)
            ? entry.CityManaShardStockText
            : !string.IsNullOrEmpty(writeback.CityId)
                ? writeback.StockAfter.ToString()
                : "None";
        data.Selection.LinkedDungeonText = HasMeaningfulText(entry.LinkedDungeonLabel)
            ? entry.LinkedDungeonLabel
            : safeObservation.SelectedEntity.SelectedLinkedDungeonText;
        data.Selection.LinkedCityText = HasMeaningfulText(safeObservation.SelectedEntity.SelectedLinkedCityText)
            ? safeObservation.SelectedEntity.SelectedLinkedCityText
            : "None";
        data.Selection.NeedPressureText = HasMeaningfulText(citySim.NeedPressureText) ? citySim.NeedPressureText : entry.NeedPressureText;
        data.Selection.DispatchReadinessText = HasMeaningfulText(citySim.DispatchReadinessText) ? citySim.DispatchReadinessText : entry.DispatchReadinessText;
        data.Selection.RecoveryProgressText = HasMeaningfulText(citySim.RecoveryProgressText) ? citySim.RecoveryProgressText : entry.RecoveryProgressText;
        data.Selection.DispatchPolicyText = HasMeaningfulText(citySim.DispatchPolicyText) ? citySim.DispatchPolicyText : entry.DispatchPolicyText;
        data.Selection.PressureBoardSummaryText = HasMeaningfulText(entry.PressureBoardSummaryText)
            ? entry.PressureBoardSummaryText
            : safeObservation.CurrentWorldObservationSummaryText;
        data.Selection.WhyCityMattersText = HasMeaningfulText(entry.WhyCityMattersText)
            ? entry.WhyCityMattersText
            : citySim.RecommendationReasonText;
        data.Selection.RecentResultEvidenceText = HasMeaningfulText(entry.RecentResultEvidenceText)
            ? entry.RecentResultEvidenceText
            : entry.RecentOutcomeText;
        data.Selection.PressureChangeText = HasMeaningfulText(entry.PressureChangeText)
            ? entry.PressureChangeText
            : "No pressure change recorded.";
        data.Selection.PartyReadinessSummaryText = HasMeaningfulText(entry.PartyReadinessSummaryText)
            ? entry.PartyReadinessSummaryText
            : entry.PartyRosterSummaryText;
        data.Selection.RecommendedRouteText = HasMeaningfulText(citySim.RecommendedRouteSummaryText) ? citySim.RecommendedRouteSummaryText : entry.RecommendedRouteSummaryText;
        data.Selection.RecommendationReasonText = prep != null && HasMeaningfulText(prep.RecommendationReasonText)
            ? prep.RecommendationReasonText
            : citySim.RecommendationReasonText;
        data.Selection.DispatchPartySummaryText = prep != null && HasMeaningfulText(prep.StagedPartySummaryText)
            ? prep.StagedPartySummaryText
            : entry.PartyRosterSummaryText;
        data.Selection.LaunchLockSummaryText = prep != null && HasMeaningfulText(prep.LaunchGateSummaryText)
            ? prep.LaunchGateSummaryText
            : interaction.BlockedReasonText;
        data.Selection.ReturnEtaText = prep != null && HasMeaningfulText(prep.ReturnEtaText)
            ? prep.ReturnEtaText
            : readback.RecoveryEtaText;
        data.Selection.ProjectedOutcomeSummaryText = prep != null && HasMeaningfulText(prep.ProjectedOutcomeSummaryText)
            ? prep.ProjectedOutcomeSummaryText
            : "None";
        data.Selection.RewardPreviewText = prep != null && HasMeaningfulText(prep.RewardPreviewText)
            ? prep.RewardPreviewText
            : "None";
        data.Selection.EventPreviewText = prep != null && HasMeaningfulText(prep.EventPreviewText)
            ? prep.EventPreviewText
            : "None";
        data.Selection.DungeonDangerText = HasMeaningfulText(safeObservation.SelectedEntity.SelectedDungeonDangerText)
            ? safeObservation.SelectedEntity.SelectedDungeonDangerText
            : "None";
        data.Selection.DungeonStatusText = HasMeaningfulText(safeObservation.SelectedEntity.SelectedDungeonStatusText)
            ? safeObservation.SelectedEntity.SelectedDungeonStatusText
            : "None";
        data.Selection.DungeonAvailabilityText = HasMeaningfulText(safeObservation.SelectedEntity.SelectedDungeonAvailabilityText)
            ? safeObservation.SelectedEntity.SelectedDungeonAvailabilityText
            : "None";

        data.Actions.IsExpeditionPrepBoardOpen = prep != null && prep.IsBoardOpen;
        data.Actions.CanRecruitParty = interaction.CanRecruitParty;
        data.Actions.CanOpenPartyRoster = interaction.CanOpenPartyRoster;
        data.Actions.CanOpenExpeditionPrep = interaction.CanOpenExpeditionPrep;
        data.Actions.CanEnterDungeon = data.Actions.IsExpeditionPrepBoardOpen
            ? prep != null && prep.CanConfirmLaunch
            : interaction.CanOpenExpeditionPrep;
        data.Actions.DispatchBriefingSummaryText = prep != null && HasMeaningfulText(prep.DispatchBriefingSummaryText)
            ? prep.DispatchBriefingSummaryText
            : "None";
        data.Actions.DispatchPartySummaryText = data.Selection.DispatchPartySummaryText;
        data.Actions.RouteFitSummaryText = prep != null && HasMeaningfulText(prep.RouteFitSummaryText)
            ? prep.RouteFitSummaryText
            : "None";
        data.Actions.LaunchLockSummaryText = data.Selection.LaunchLockSummaryText;
        data.Actions.ProjectedOutcomeSummaryText = data.Selection.ProjectedOutcomeSummaryText;
        data.Actions.ActiveExpeditionLaneText = safeObservation.ActiveExpedition != null &&
            HasMeaningfulText(safeObservation.ActiveExpedition.SelectedActiveExpeditionLaneText)
            ? safeObservation.ActiveExpedition.SelectedActiveExpeditionLaneText
            : "None";
        data.Actions.CityVacancyText = HasMeaningfulText(citySim.CityVacancyText)
            ? citySim.CityVacancyText
            : prep != null && HasMeaningfulText(prep.CityVacancyText)
                ? prep.CityVacancyText
                : "None";
        data.Actions.ReturnEtaText = data.Selection.ReturnEtaText;
        data.Actions.BlockedReasonText = HasMeaningfulText(interaction.BlockedReasonText)
            ? interaction.BlockedReasonText
            : prep != null
                ? prep.BlockedReasonText
                : "None";
        data.Actions.InteractionSummaryText = HasMeaningfulText(interaction.InteractionSummaryText)
            ? interaction.InteractionSummaryText
            : entry.SelectionSummaryText;

        data.RecruitmentBoard.IsAvailable = interaction.CanRecruitParty;
        data.RecruitmentBoard.SummaryText = HasMeaningfulText(interaction.InteractionSummaryText)
            ? interaction.InteractionSummaryText
            : entry.AvailabilitySummaryText;
        data.RecruitmentBoard.ActionText = HasMeaningfulText(interaction.RecruitmentActionText)
            ? interaction.RecruitmentActionText
            : "Recruitment is unavailable.";
        data.RecruitmentBoard.BlockedReasonText = interaction.CanRecruitParty ? "None" : data.Actions.BlockedReasonText;

        data.PartyRosterBoard.IsAvailable = interaction.CanOpenPartyRoster;
        data.PartyRosterBoard.HasReadyParty = roster.HasReadyParty;
        data.PartyRosterBoard.ReadyPartyLabel = HasMeaningfulText(roster.ReadyPartyLabel) ? roster.ReadyPartyLabel : "None";
        data.PartyRosterBoard.ReadyPartyRoleSummaryText = HasMeaningfulText(roster.ReadyPartyRoleSummaryText) ? roster.ReadyPartyRoleSummaryText : "None";
        data.PartyRosterBoard.ReadyPartyLoadoutSummaryText = HasMeaningfulText(roster.ReadyPartyLoadoutSummaryText) ? roster.ReadyPartyLoadoutSummaryText : "None";
        data.PartyRosterBoard.AvailabilitySummaryText = HasMeaningfulText(roster.AvailabilitySummaryText)
            ? roster.AvailabilitySummaryText
            : interaction.PartyRosterActionText;
        data.PartyRosterBoard.TotalPartyCount = roster.TotalPartyCount;
        data.PartyRosterBoard.IdlePartyCount = roster.IdlePartyCount;
        data.PartyRosterBoard.ActiveExpeditionCount = roster.ActiveExpeditionCount;

        data.Outcome.LatestReturnAftermathText = HasMeaningfulText(readback.LatestReturnAftermathText) ? readback.LatestReturnAftermathText : "None";
        data.Outcome.OutcomeReadbackText = HasMeaningfulText(readback.PostRunSummaryText) ? readback.PostRunSummaryText : entry.RecentOutcomeText;
        data.Outcome.CorrectiveFollowUpText = HasMeaningfulText(readback.NextSuggestedActionText)
            ? readback.NextSuggestedActionText
            : entry.RecommendedNextActionText;
        data.Outcome.LootReturnedText = HasMeaningfulText(writeback.LootSummaryText) ? writeback.LootSummaryText : "None";
        data.Outcome.PartyOutcomeText = HasMeaningfulText(readback.SurvivingMembersSummaryText) ? readback.SurvivingMembersSummaryText : writeback.PartyOutcomeSummaryText;
        data.Outcome.WorldWritebackText = HasMeaningfulText(readback.SelectedWorldWritebackText)
            ? readback.SelectedWorldWritebackText
            : citySim.WorldWritebackText;
        data.Outcome.DungeonStatusText = HasMeaningfulText(readback.DungeonStatusText) ? readback.DungeonStatusText : data.Selection.DungeonStatusText;
        data.Outcome.DungeonAvailabilityText = HasMeaningfulText(readback.DungeonAvailabilityText) ? readback.DungeonAvailabilityText : data.Selection.DungeonAvailabilityText;
        data.Outcome.StockReactionText = HasMeaningfulText(writeback.StockReactionSummaryText) ? writeback.StockReactionSummaryText : "None";
        data.Outcome.DispatchReadinessText = HasMeaningfulText(readback.DispatchReadinessAfterText) ? readback.DispatchReadinessAfterText : data.Selection.DispatchReadinessText;
        data.Outcome.RecoveryEtaText = HasMeaningfulText(readback.RecoveryEtaText) ? readback.RecoveryEtaText : data.Selection.ReturnEtaText;
        data.Outcome.FollowUpHintText = HasMeaningfulText(readback.FollowUpHintText) ? readback.FollowUpHintText : "None";

        data.Overview.WorldDayCount = safeObservation.WorldDayCount;
        data.Overview.TradeStepCount = safeObservation.TradeStepCount;
        data.Overview.TotalParties = safeObservation.TotalParties;
        data.Overview.IdleParties = safeObservation.IdleParties;
        data.Overview.ActiveExpeditions = safeObservation.ActiveExpeditions;
        data.Overview.PriorityCityText = priorityBoard.HasPriorityCity && HasMeaningfulText(priorityBoard.CityLabel)
            ? priorityBoard.CityLabel
            : "None";
        data.Overview.PrioritySummaryText = HasMeaningfulText(priorityBoard.SummaryText)
            ? priorityBoard.SummaryText
            : "None";
        data.Overview.PriorityNextActionText = HasMeaningfulText(priorityBoard.NextActionText)
            ? priorityBoard.NextActionText
            : "None";
        data.Overview.ActiveExpeditionLaneText = safeObservation.ActiveExpedition != null &&
            HasMeaningfulText(safeObservation.ActiveExpedition.WorldActiveExpeditionLaneText)
            ? safeObservation.ActiveExpedition.WorldActiveExpeditionLaneText
            : "None";
        data.Overview.ReturnEtaText = safeObservation.ActiveExpedition != null &&
            HasMeaningfulText(safeObservation.ActiveExpedition.WorldReturnEtaText)
            ? safeObservation.ActiveExpedition.WorldReturnEtaText
            : data.Actions.ReturnEtaText;
        data.Overview.WorldWritebackText = HasMeaningfulText(safeObservation.RecentOutcome.WorldWritebackSummaryText)
            ? safeObservation.RecentOutcome.WorldWritebackSummaryText
            : data.Outcome.WorldWritebackText;
        data.Overview.CitiesWithShortagesText = HasMeaningfulText(safeObservation.CitiesWithShortagesText)
            ? safeObservation.CitiesWithShortagesText
            : "None";
        data.Overview.LastTransitionText = data.Header.LastTransitionText;

        data.Logs.RecentWorldWritebackLogs = new[]
        {
            safeObservation.Logs != null && HasMeaningfulText(safeObservation.Logs.RecentWorldWritebackLog1Text) ? safeObservation.Logs.RecentWorldWritebackLog1Text : "None",
            safeObservation.Logs != null && HasMeaningfulText(safeObservation.Logs.RecentWorldWritebackLog2Text) ? safeObservation.Logs.RecentWorldWritebackLog2Text : "None",
            safeObservation.Logs != null && HasMeaningfulText(safeObservation.Logs.RecentWorldWritebackLog3Text) ? safeObservation.Logs.RecentWorldWritebackLog3Text : "None"
        };
        data.Logs.RecentExpeditionLogs = new[]
        {
            safeObservation.Logs != null && HasMeaningfulText(safeObservation.Logs.RecentExpeditionLog1Text) ? safeObservation.Logs.RecentExpeditionLog1Text : "None",
            safeObservation.Logs != null && HasMeaningfulText(safeObservation.Logs.RecentExpeditionLog2Text) ? safeObservation.Logs.RecentExpeditionLog2Text : "None",
            safeObservation.Logs != null && HasMeaningfulText(safeObservation.Logs.RecentExpeditionLog3Text) ? safeObservation.Logs.RecentExpeditionLog3Text : "None"
        };
        data.Logs.RecentDayLogs = new[]
        {
            safeObservation.Logs != null && HasMeaningfulText(safeObservation.Logs.RecentDayLog1Text) ? safeObservation.Logs.RecentDayLog1Text : "None",
            safeObservation.Logs != null && HasMeaningfulText(safeObservation.Logs.RecentDayLog2Text) ? safeObservation.Logs.RecentDayLog2Text : "None",
            safeObservation.Logs != null && HasMeaningfulText(safeObservation.Logs.RecentDayLog3Text) ? safeObservation.Logs.RecentDayLog3Text : "None"
        };
        data.Logs.DepartureEchoText = safeObservation.ActiveExpedition != null &&
            HasMeaningfulText(safeObservation.ActiveExpedition.WorldDepartureEchoText)
            ? safeObservation.ActiveExpedition.WorldDepartureEchoText
            : "None";
        data.Logs.ReturnWindowText = safeObservation.ActiveExpedition != null &&
            HasMeaningfulText(safeObservation.ActiveExpedition.SelectedReturnWindowText)
            ? safeObservation.ActiveExpedition.SelectedReturnWindowText
            : "None";

        return data;
    }

    public CityInteractionPresentationSurfaceData BuildPresentationSurfaceData()
    {
        return BuildPresentationSurfaceData(BuildUiSurfaceData());
    }

    public CityInteractionPresentationSurfaceData BuildPresentationSurfaceData(WorldObservationSurfaceData observation)
    {
        return BuildPresentationSurfaceData(BuildUiSurfaceData(observation));
    }

    public CityInteractionPresentationSurfaceData BuildPresentationSurfaceData(PrototypeCityHubUiSurfaceData uiSurface)
    {
        PrototypeCityHubUiSurfaceData safeUiSurface = uiSurface ?? new PrototypeCityHubUiSurfaceData();
        CityInteractionPresentationSurfaceData data = new CityInteractionPresentationSurfaceData();
        data.HasSelectedCity = safeUiSurface.HasSelectedCity;
        data.IsExpeditionPrepBoardOpen = safeUiSurface.Actions.IsExpeditionPrepBoardOpen;
        data.CanRecruitParty = safeUiSurface.Actions.CanRecruitParty;
        data.CanOpenPartyRoster = safeUiSurface.Actions.CanOpenPartyRoster;
        data.CanOpenExpeditionPrep = safeUiSurface.Actions.CanOpenExpeditionPrep;
        data.CanEnterDungeon = safeUiSurface.Actions.CanEnterDungeon;
        data.SelectionLabel = safeUiSurface.Selection.DisplayName;
        data.SelectionTypeLabel = safeUiSurface.Selection.TypeLabel;
        data.CityManaShardStockText = safeUiSurface.Selection.CityManaShardStockText;
        data.LinkedDungeonText = safeUiSurface.Selection.LinkedDungeonText;
        data.NeedPressureText = safeUiSurface.Selection.NeedPressureText;
        data.DispatchReadinessText = safeUiSurface.Selection.DispatchReadinessText;
        data.RecoveryProgressText = safeUiSurface.Selection.RecoveryProgressText;
        data.DispatchPolicyText = safeUiSurface.Selection.DispatchPolicyText;
        data.RecommendedRouteText = safeUiSurface.Selection.RecommendedRouteText;
        data.RecommendationReasonText = safeUiSurface.Selection.RecommendationReasonText;
        data.DispatchPartySummaryText = safeUiSurface.Actions.DispatchPartySummaryText;
        data.DispatchBriefingSummaryText = safeUiSurface.Actions.DispatchBriefingSummaryText;
        data.RouteFitSummaryText = safeUiSurface.Actions.RouteFitSummaryText;
        data.LaunchLockSummaryText = safeUiSurface.Actions.LaunchLockSummaryText;
        data.ProjectedOutcomeSummaryText = safeUiSurface.Actions.ProjectedOutcomeSummaryText;
        data.ActiveExpeditionLaneText = safeUiSurface.Actions.ActiveExpeditionLaneText;
        data.CityVacancyText = safeUiSurface.Actions.CityVacancyText;
        data.ReturnEtaText = safeUiSurface.Actions.ReturnEtaText;
        data.RewardPreviewText = safeUiSurface.Selection.RewardPreviewText;
        data.EventPreviewText = safeUiSurface.Selection.EventPreviewText;
        data.PressureChangeText = safeUiSurface.Selection.PressureChangeText;
        data.LatestReturnAftermathText = safeUiSurface.Outcome.LatestReturnAftermathText;
        data.OutcomeReadbackText = safeUiSurface.Outcome.OutcomeReadbackText;
        data.CorrectiveFollowUpText = safeUiSurface.Outcome.CorrectiveFollowUpText;
        data.WorldWritebackText = safeUiSurface.Outcome.WorldWritebackText;
        data.BlockedReasonText = safeUiSurface.Actions.BlockedReasonText;
        data.InteractionSummaryText = safeUiSurface.Actions.InteractionSummaryText;
        data.RecruitmentActionText = safeUiSurface.RecruitmentBoard.ActionText;
        data.PartyRosterSummaryText = safeUiSurface.PartyRosterBoard.AvailabilitySummaryText;
        return data;
    }

    public PrototypeCityHubActionResult TryExecuteCityHubAction(PrototypeCityHubActionRequest request)
    {
        PrototypeCityHubActionRequest safeRequest = request ?? new PrototypeCityHubActionRequest();
        string actionKey = string.IsNullOrEmpty(safeRequest.ActionKey) ? string.Empty : safeRequest.ActionKey;
        switch (actionKey)
        {
            case PrototypeCityHubActionKeys.RecruitSelectedCityParty:
                return ExecuteRecruitSelectedCityParty(actionKey);
            case PrototypeCityHubActionKeys.EnterSelectedWorldDungeon:
                return ExecuteEnterSelectedWorldDungeon(actionKey);
            case PrototypeCityHubActionKeys.OpenSelectedWorldExpeditionPrepBoard:
                return ExecuteOpenSelectedWorldExpeditionPrepBoard(actionKey);
            case PrototypeCityHubActionKeys.CancelSelectedWorldExpeditionPrepBoard:
                return ExecuteCancelSelectedWorldExpeditionPrepBoard(actionKey);
            case PrototypeCityHubActionKeys.SelectExpeditionPrepRoute:
                return ExecuteSelectExpeditionPrepRoute(actionKey, safeRequest.OptionKey);
            case PrototypeCityHubActionKeys.CycleExpeditionPrepDispatchPolicy:
                return ExecuteCycleExpeditionPrepDispatchPolicy(actionKey);
            case PrototypeCityHubActionKeys.ConfirmSelectedExpeditionLaunch:
                return ExecuteConfirmSelectedExpeditionLaunch(actionKey);
            default:
                return CreateResult(
                    actionKey,
                    wasHandled: false,
                    succeeded: false,
                    shouldRefreshPresentation: false,
                    errorSummaryText: "Unknown CityHub action key.");
        }
    }

    public void RecruitSelectedCityParty()
    {
        TryExecuteCityHubAction(new PrototypeCityHubActionRequest
        {
            ActionKey = PrototypeCityHubActionKeys.RecruitSelectedCityParty,
            SourceSurfaceName = "BootEntry"
        });
    }

    public bool TryEnterSelectedWorldDungeon()
    {
        PrototypeCityHubActionResult result = TryExecuteCityHubAction(new PrototypeCityHubActionRequest
        {
            ActionKey = PrototypeCityHubActionKeys.EnterSelectedWorldDungeon,
            SourceSurfaceName = "BootEntry"
        });
        return result.WasHandled && result.Succeeded;
    }

    public bool TryOpenSelectedWorldExpeditionPrepBoard()
    {
        PrototypeCityHubActionResult result = TryExecuteCityHubAction(new PrototypeCityHubActionRequest
        {
            ActionKey = PrototypeCityHubActionKeys.OpenSelectedWorldExpeditionPrepBoard,
            SourceSurfaceName = "BootEntry"
        });
        return result.WasHandled && result.Succeeded;
    }

    public void CancelSelectedWorldExpeditionPrepBoard()
    {
        TryExecuteCityHubAction(new PrototypeCityHubActionRequest
        {
            ActionKey = PrototypeCityHubActionKeys.CancelSelectedWorldExpeditionPrepBoard,
            SourceSurfaceName = "BootEntry"
        });
    }

    public bool TrySelectExpeditionPrepRoute(string optionKey)
    {
        PrototypeCityHubActionResult result = TryExecuteCityHubAction(new PrototypeCityHubActionRequest
        {
            ActionKey = PrototypeCityHubActionKeys.SelectExpeditionPrepRoute,
            OptionKey = optionKey ?? string.Empty,
            SourceSurfaceName = "BootEntry"
        });
        return result.WasHandled && result.Succeeded;
    }

    public bool TryCycleExpeditionPrepDispatchPolicy()
    {
        PrototypeCityHubActionResult result = TryExecuteCityHubAction(new PrototypeCityHubActionRequest
        {
            ActionKey = PrototypeCityHubActionKeys.CycleExpeditionPrepDispatchPolicy,
            SourceSurfaceName = "BootEntry"
        });
        return result.WasHandled && result.Succeeded;
    }

    public bool TryConfirmSelectedExpeditionLaunch()
    {
        PrototypeCityHubActionResult result = TryExecuteCityHubAction(new PrototypeCityHubActionRequest
        {
            ActionKey = PrototypeCityHubActionKeys.ConfirmSelectedExpeditionLaunch,
            SourceSurfaceName = "BootEntry"
        });
        return result.WasHandled && result.Succeeded;
    }

    private static bool HasMeaningfulText(string value)
    {
        return !string.IsNullOrEmpty(value) && value != "None";
    }

    private static string ResolveAlertTitle(ExpeditionPostRunRevealState reveal, OutcomeReadback readback, WorldObservationPriorityBoardData priorityBoard)
    {
        if (reveal != null && reveal.HasPendingReveal)
        {
            return HasMeaningfulText(reveal.HeadlineText) ? reveal.HeadlineText : "Return Spotlight";
        }

        if (priorityBoard != null && priorityBoard.HasPriorityCity && HasMeaningfulText(priorityBoard.CityLabel))
        {
            return "Priority City: " + priorityBoard.CityLabel;
        }

        if (readback != null && HasMeaningfulText(readback.LatestReturnAftermathText))
        {
            return "World Return Focus";
        }

        if (readback != null && HasMeaningfulText(readback.PostRunSummaryText))
        {
            return "World Outcome Focus";
        }

        return "World Board";
    }

    private static string ResolveAlertSummary(ExpeditionPostRunRevealState reveal, OutcomeReadback readback, WorldObservationPriorityBoardData priorityBoard)
    {
        if (reveal != null && reveal.HasPendingReveal)
        {
            OutcomeReadback pendingReadback = reveal.OutcomeReadback ?? readback;
            ExpeditionResult result = reveal.LatestExpeditionResult ?? new ExpeditionResult();
            if (pendingReadback != null && HasMeaningfulText(pendingReadback.LatestReturnAftermathText))
            {
                return "Latest Return: " + pendingReadback.LatestReturnAftermathText;
            }

            if (pendingReadback != null && HasMeaningfulText(pendingReadback.NextSuggestedActionText))
            {
                return "Corrective Follow-Up: " + pendingReadback.NextSuggestedActionText;
            }

            if (HasMeaningfulText(result.ResultSummaryText))
            {
                return "Outcome Readback: " + result.ResultSummaryText;
            }
        }

        if (priorityBoard != null && HasMeaningfulText(priorityBoard.SummaryText))
        {
            return priorityBoard.SummaryText;
        }

        if (readback != null && HasMeaningfulText(readback.LatestReturnAftermathText))
        {
            return "Latest Return: " + readback.LatestReturnAftermathText;
        }

        if (readback != null && HasMeaningfulText(readback.NextSuggestedActionText))
        {
            return "Corrective Follow-Up: " + readback.NextSuggestedActionText;
        }

        if (readback != null && HasMeaningfulText(readback.PostRunSummaryText))
        {
            return "Outcome Readback: " + readback.PostRunSummaryText;
        }

        return "None";
    }

    private static string ResolveAlertFooter(ExpeditionPostRunRevealState reveal, WorldObservationPriorityBoardData priorityBoard)
    {
        if (reveal != null && reveal.HasPendingReveal)
        {
            return "Press X to review the return, Esc to dismiss the spotlight.";
        }

        if (priorityBoard != null && priorityBoard.HasPriorityCity)
        {
            string nextAction = HasMeaningfulText(priorityBoard.NextActionText)
                ? "Next: " + priorityBoard.NextActionText
                : string.Empty;
            string readiness = HasMeaningfulText(priorityBoard.PartyReadinessText)
                ? "Party: " + priorityBoard.PartyReadinessText
                : string.Empty;
            if (HasMeaningfulText(nextAction) && HasMeaningfulText(readiness))
            {
                return nextAction + " | " + readiness;
            }

            if (HasMeaningfulText(nextAction))
            {
                return nextAction;
            }

            if (HasMeaningfulText(readiness))
            {
                return readiness;
            }
        }

        return "Selected = bright link | Return = pinned pulse | Relief = calm green | Rebound = warning amber";
    }

    private PrototypeCityHubActionResult ExecuteRecruitSelectedCityParty(string actionKey)
    {
        if (!IsWorldSimActive || _worldView == null)
        {
            return CreateResult(
                actionKey,
                wasHandled: true,
                succeeded: false,
                shouldRefreshPresentation: false,
                errorSummaryText: "CityHub recruitment is only available in WorldSim.");
        }

        CityInteractionSurfaceData interaction = GetInteractionSurfaceData();
        if (!interaction.CanRecruitParty)
        {
            return CreateResult(
                actionKey,
                wasHandled: true,
                succeeded: false,
                shouldRefreshPresentation: false,
                errorSummaryText: ResolveBlockedActionText(
                    interaction.RecruitmentActionText,
                    interaction.BlockedReasonText,
                    "Recruitment is currently unavailable."));
        }

        _worldView.RecruitSelectedCityParty();
        return CreateResult(
            actionKey,
            wasHandled: true,
            succeeded: true,
            shouldRefreshPresentation: true,
            refreshReasonText: "Recruitment updated the selected city party state.");
    }

    private PrototypeCityHubActionResult ExecuteEnterSelectedWorldDungeon(string actionKey)
    {
        ExpeditionPrepSurfaceData prep = GetExpeditionPrepSurfaceData();
        return prep != null && prep.IsBoardOpen
            ? ExecuteConfirmSelectedExpeditionLaunch(actionKey)
            : ExecuteOpenSelectedWorldExpeditionPrepBoard(actionKey);
    }

    private PrototypeCityHubActionResult ExecuteOpenSelectedWorldExpeditionPrepBoard(string actionKey)
    {
        if (!IsWorldSimActive || _worldView == null)
        {
            return CreateResult(
                actionKey,
                wasHandled: true,
                succeeded: false,
                shouldRefreshPresentation: false,
                errorSummaryText: "Expedition prep is only available in WorldSim.");
        }

        if (_expeditionPrepGateway != null && _expeditionPrepGateway.TryOpenBoard())
        {
            return CreateResult(
                actionKey,
                wasHandled: true,
                succeeded: true,
                shouldRefreshPresentation: true,
                refreshReasonText: "Expedition prep board opened.");
        }

        ExpeditionPrepSurfaceData prep = GetExpeditionPrepSurfaceData();
        CityInteractionSurfaceData interaction = GetInteractionSurfaceData();
        return CreateResult(
            actionKey,
            wasHandled: true,
            succeeded: false,
            shouldRefreshPresentation: false,
            errorSummaryText: ResolveBlockedActionText(
                prep != null ? prep.BlockedReasonText : null,
                interaction.BlockedReasonText,
                "Expedition prep could not be opened."));
    }

    private PrototypeCityHubActionResult ExecuteCancelSelectedWorldExpeditionPrepBoard(string actionKey)
    {
        if (!IsWorldSimActive || _worldView == null)
        {
            return CreateResult(
                actionKey,
                wasHandled: true,
                succeeded: false,
                shouldRefreshPresentation: false,
                errorSummaryText: "Expedition prep cancel is only available in WorldSim.");
        }

        ExpeditionPrepSurfaceData prep = GetExpeditionPrepSurfaceData();
        if (prep == null || !prep.IsBoardOpen)
        {
            return CreateResult(
                actionKey,
                wasHandled: true,
                succeeded: false,
                shouldRefreshPresentation: false,
                errorSummaryText: "No expedition prep board is open.");
        }

        if (_expeditionPrepGateway != null)
        {
            _expeditionPrepGateway.CancelPreparation();
        }
        return CreateResult(
            actionKey,
            wasHandled: true,
            succeeded: true,
            shouldRefreshPresentation: true,
            refreshReasonText: "Expedition prep board closed.");
    }

    private PrototypeCityHubActionResult ExecuteSelectExpeditionPrepRoute(string actionKey, string optionKey)
    {
        if (!IsWorldSimActive || _worldView == null)
        {
            return CreateResult(
                actionKey,
                wasHandled: true,
                succeeded: false,
                shouldRefreshPresentation: false,
                errorSummaryText: "Expedition route selection is only available in WorldSim.");
        }

        if (string.IsNullOrEmpty(optionKey))
        {
            return CreateResult(
                actionKey,
                wasHandled: true,
                succeeded: false,
                shouldRefreshPresentation: false,
                errorSummaryText: "Expedition route selection requires an option key.");
        }

        if (_expeditionPrepGateway != null && _expeditionPrepGateway.TrySelectRoute(optionKey))
        {
            return CreateResult(
                actionKey,
                wasHandled: true,
                succeeded: true,
                shouldRefreshPresentation: true,
                refreshReasonText: "Expedition prep route selection updated.");
        }

        ExpeditionPrepSurfaceData prep = GetExpeditionPrepSurfaceData();
        return CreateResult(
            actionKey,
            wasHandled: true,
            succeeded: false,
            shouldRefreshPresentation: false,
            errorSummaryText: ResolveBlockedActionText(
                prep != null ? prep.FeedbackText : null,
                prep != null ? prep.BlockedReasonText : null,
                "Selected expedition route is unavailable."));
    }

    private PrototypeCityHubActionResult ExecuteCycleExpeditionPrepDispatchPolicy(string actionKey)
    {
        if (!IsWorldSimActive || _worldView == null)
        {
            return CreateResult(
                actionKey,
                wasHandled: true,
                succeeded: false,
                shouldRefreshPresentation: false,
                errorSummaryText: "Dispatch policy cycling is only available in WorldSim.");
        }

        if (_expeditionPrepGateway != null && _expeditionPrepGateway.TryCycleDispatchPolicy())
        {
            return CreateResult(
                actionKey,
                wasHandled: true,
                succeeded: true,
                shouldRefreshPresentation: true,
                refreshReasonText: "Dispatch policy updated.");
        }

        ExpeditionPrepSurfaceData prep = GetExpeditionPrepSurfaceData();
        return CreateResult(
            actionKey,
            wasHandled: true,
            succeeded: false,
            shouldRefreshPresentation: false,
            errorSummaryText: ResolveBlockedActionText(
                prep != null ? prep.FeedbackText : null,
                prep != null ? prep.BlockedReasonText : null,
                "Dispatch policy could not be cycled."));
    }

    private PrototypeCityHubActionResult ExecuteConfirmSelectedExpeditionLaunch(string actionKey)
    {
        if (!IsWorldSimActive || _worldView == null)
        {
            return CreateResult(
                actionKey,
                wasHandled: true,
                succeeded: false,
                shouldRefreshPresentation: false,
                errorSummaryText: "Expedition launch is only available in WorldSim.");
        }

        Camera mainCamera = _resolveMainCamera != null ? _resolveMainCamera() : Camera.main;
        if (_expeditionPrepGateway != null && _expeditionPrepGateway.TryConfirmLaunch(mainCamera))
        {
            return CreateResult(
                actionKey,
                wasHandled: true,
                succeeded: true,
                shouldRefreshPresentation: true,
                transitionToDungeonRunRequested: true,
                refreshReasonText: "Expedition launch confirmed.");
        }

        ExpeditionPrepSurfaceData prep = GetExpeditionPrepSurfaceData();
        ExpeditionLaunchRequest launchRequest = GetExpeditionLaunchRequest();
        return CreateResult(
            actionKey,
            wasHandled: true,
            succeeded: false,
            shouldRefreshPresentation: false,
            errorSummaryText: ResolveBlockedActionText(
                prep != null ? prep.FeedbackText : null,
                launchRequest.LaunchGateSummaryText,
                "Expedition launch is blocked."));
    }

    private static PrototypeCityHubActionResult CreateResult(
        string actionKey,
        bool wasHandled,
        bool succeeded,
        bool shouldRefreshPresentation,
        bool transitionToDungeonRunRequested = false,
        string errorSummaryText = "None",
        string refreshReasonText = "None")
    {
        return new PrototypeCityHubActionResult
        {
            ActionKey = string.IsNullOrEmpty(actionKey) ? string.Empty : actionKey,
            WasHandled = wasHandled,
            Succeeded = succeeded,
            TransitionToDungeonRunRequested = transitionToDungeonRunRequested,
            ShouldRefreshPresentation = shouldRefreshPresentation,
            ErrorSummaryText = HasMeaningfulText(errorSummaryText) ? errorSummaryText : "None",
            RefreshReasonText = HasMeaningfulText(refreshReasonText) ? refreshReasonText : "None"
        };
    }

    private static string ResolveBlockedActionText(string primary, string secondary, string fallback)
    {
        if (HasMeaningfulText(primary))
        {
            return primary;
        }

        if (HasMeaningfulText(secondary))
        {
            return secondary;
        }

        return string.IsNullOrEmpty(fallback) ? "None" : fallback;
    }
}

public sealed class CityInteractionPresentationSurfaceData
{
    public bool HasSelectedCity;
    public bool IsExpeditionPrepBoardOpen;
    public bool CanRecruitParty;
    public bool CanOpenPartyRoster;
    public bool CanOpenExpeditionPrep;
    public bool CanEnterDungeon;
    public string SelectionLabel = "None";
    public string SelectionTypeLabel = "None";
    public string CityManaShardStockText = "None";
    public string LinkedDungeonText = "None";
    public string NeedPressureText = "None";
    public string DispatchReadinessText = "None";
    public string RecoveryProgressText = "None";
    public string DispatchPolicyText = "None";
    public string RecommendedRouteText = "None";
    public string RecommendationReasonText = "None";
    public string DispatchPartySummaryText = "None";
    public string DispatchBriefingSummaryText = "None";
    public string RouteFitSummaryText = "None";
    public string LaunchLockSummaryText = "None";
    public string ProjectedOutcomeSummaryText = "None";
    public string ActiveExpeditionLaneText = "None";
    public string CityVacancyText = "None";
    public string ReturnEtaText = "None";
    public string RewardPreviewText = "None";
    public string EventPreviewText = "None";
    public string PressureChangeText = "None";
    public string LatestReturnAftermathText = "None";
    public string OutcomeReadbackText = "None";
    public string CorrectiveFollowUpText = "None";
    public string WorldWritebackText = "None";
    public string BlockedReasonText = "None";
    public string InteractionSummaryText = "None";
    public string RecruitmentActionText = "None";
    public string PartyRosterSummaryText = "None";
}
