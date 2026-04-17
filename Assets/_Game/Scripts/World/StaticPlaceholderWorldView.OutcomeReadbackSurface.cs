using System.Collections.Generic;

public sealed partial class StaticPlaceholderWorldView
{
    public CityHubSurfaceData BuildSelectedCityHubSurfaceData()
    {
        return GetCityHubSurfaceFromObservation(BuildWorldObservationSurfaceData());
    }

    public WorldSimCitySourceData BuildSelectedCityHubEntrySnapshot()
    {
        CityHubSurfaceData cityHub = GetCityHubSurfaceFromObservation(BuildWorldObservationSurfaceData());
        return cityHub.EntrySnapshot ?? new WorldSimCitySourceData();
    }

    public ExpeditionPrepHandoff BuildSelectedExpeditionPrepHandoff()
    {
        CityHubSurfaceData cityHub = GetCityHubSurfaceFromObservation(BuildWorldObservationSurfaceData());
        return cityHub.ExpeditionPrep ?? new ExpeditionPrepHandoff();
    }

    public OutcomeReadback BuildSelectedOutcomeReadbackSurface()
    {
        CityHubSurfaceData cityHub = GetCityHubSurfaceFromObservation(BuildWorldObservationSurfaceData());
        return cityHub.ResultPipelineReadback ?? new OutcomeReadback();
    }

    public CityWriteback BuildSelectedCityWritebackSurface()
    {
        CityHubSurfaceData cityHub = GetCityHubSurfaceFromObservation(BuildWorldObservationSurfaceData());
        return cityHub.ResultPipelineCityWriteback ?? new CityWriteback();
    }

    private void PopulateWorldObservationCityHub(CityHubSurfaceData cityHub, WorldObservationSurfaceData observation, WorldBoardReadModel board)
    {
        if (cityHub == null)
        {
            return;
        }

        WorldObservationSurfaceData safeObservation = observation ?? new WorldObservationSurfaceData();
        WorldSimCitySourceData entrySnapshot = BuildSelectedCityWorldSimInputData(safeObservation, board);
        CityPartyRosterSurfaceData partyRoster = BuildSelectedCityPartyRosterSurfaceData();
        cityHub.EntrySnapshot = entrySnapshot;
        cityHub.WorldSimInput = entrySnapshot;
        cityHub.HasSelectedCity = entrySnapshot.HasSelectedCity;
        cityHub.PartyRoster = partyRoster;
        cityHub.CitySim = BuildSelectedCitySimSurfaceData(safeObservation);
        cityHub.CityInteraction = BuildSelectedCityInteractionSurfaceData(safeObservation, partyRoster);
        cityHub.ExpeditionPrep = BuildSelectedExpeditionPrepHandoff(safeObservation);
        cityHub.ResultPipelineReadback = BuildSelectedOutcomeReadbackSurface(safeObservation);
        cityHub.ResultPipelineCityWriteback = BuildSelectedCityWritebackSurface(safeObservation);
    }

    private CityHubSurfaceData GetCityHubSurfaceFromObservation(WorldObservationSurfaceData observation)
    {
        return observation != null && observation.CityHub != null
            ? observation.CityHub
            : new CityHubSurfaceData();
    }

    private ExpeditionPrepHandoff BuildSelectedExpeditionPrepHandoff(WorldObservationSurfaceData observation)
    {
        return BuildExpeditionPrepHandoffFromSurface(observation != null ? observation.ExpeditionPrep : null);
    }

    private OutcomeReadback BuildSelectedOutcomeReadbackSurface(WorldObservationSurfaceData observation)
    {
        string cityId = ResolveDispatchBriefingCityId();
        string dungeonId = ResolveDispatchBriefingDungeonId(cityId);
        string cityLabel = string.IsNullOrEmpty(cityId) ? "None" : ResolveDispatchEntityDisplayName(cityId);
        string nextSuggestedActionText = ResolveOutcomeReadbackNextSuggestedActionText(observation, cityId, dungeonId);
        string latestReturnAftermathText = ResolveOutcomeReadbackLatestReturnAftermathText(observation, cityId, dungeonId);
        CityWriteback cityWriteback = BuildCityWritebackSurfaceForCity(cityId, nextSuggestedActionText);
        return BuildOutcomeReadbackSurfaceForContext(
            cityId,
            dungeonId,
            cityLabel,
            string.IsNullOrEmpty(cityId)
                ? "No city selected for CityHub readback."
                : cityLabel + " outcome readback is ready.",
            nextSuggestedActionText,
            latestReturnAftermathText,
            cityWriteback);
    }

    private CityWriteback BuildSelectedCityWritebackSurface(WorldObservationSurfaceData observation)
    {
        string cityId = ResolveDispatchBriefingCityId();
        string dungeonId = ResolveDispatchBriefingDungeonId(cityId);
        string nextSuggestedActionText = ResolveOutcomeReadbackNextSuggestedActionText(observation, cityId, dungeonId);
        return BuildCityWritebackSurfaceForCity(cityId, nextSuggestedActionText);
    }

    private CityWriteback BuildCityWritebackSurfaceForCity(string cityId, string nextSuggestedActionText)
    {
        if (string.IsNullOrEmpty(cityId))
        {
            return new CityWriteback();
        }

        WorldWriteback selectedWorldWriteback = _runtimeEconomyState != null
            ? _runtimeEconomyState.GetLatestWorldWritebackForCity(cityId)
            : new WorldWriteback();
        bool hasCanonicalWritebackForCity = selectedWorldWriteback != null &&
            IsMeaningfulSnapshotText(selectedWorldWriteback.RunResultStateKey);
        bool hasPreviewDeltaForCity = hasCanonicalWritebackForCity &&
            _runtimeEconomyState != null &&
            _runtimeEconomyState.GetLatestWorldWritebackCityId() == cityId;
        int stockAfter = hasPreviewDeltaForCity ? _resultStockAfter : GetCityManaShardStock(cityId);
        int stockBefore = hasPreviewDeltaForCity ? _resultStockBefore : stockAfter;
        return ResultPipeline.BuildCityWriteback(
            cityId,
            ResolveDispatchEntityDisplayName(cityId),
            selectedWorldWriteback != null ? selectedWorldWriteback.RunResultStateKey : string.Empty,
            selectedWorldWriteback != null ? selectedWorldWriteback.RewardResourceId : string.Empty,
            selectedWorldWriteback != null ? selectedWorldWriteback.LootReturned : 0,
            selectedWorldWriteback != null ? selectedWorldWriteback.LootSummaryText : "None",
            selectedWorldWriteback != null && selectedWorldWriteback.CityWriteback != null
                ? selectedWorldWriteback.CityWriteback.PartyOutcomeSummaryText
                : "None",
            selectedWorldWriteback != null && selectedWorldWriteback.CityWriteback != null
                ? selectedWorldWriteback.CityWriteback.StockReactionSummaryText
                : "None",
            stockBefore,
            stockAfter,
            hasPreviewDeltaForCity ? _resultStockDelta : 0,
            hasPreviewDeltaForCity && IsMeaningfulSnapshotText(_resultNeedPressureBeforeText)
                ? _resultNeedPressureBeforeText
                : BuildNeedPressureText(cityId),
            hasPreviewDeltaForCity && IsMeaningfulSnapshotText(_resultNeedPressureAfterText)
                ? _resultNeedPressureAfterText
                : BuildNeedPressureText(cityId),
            hasPreviewDeltaForCity && IsMeaningfulSnapshotText(_resultDispatchReadinessBeforeText)
                ? _resultDispatchReadinessBeforeText
                : GetDispatchReadinessText(cityId),
            hasPreviewDeltaForCity && IsMeaningfulSnapshotText(_resultDispatchReadinessAfterText)
                ? _resultDispatchReadinessAfterText
                : GetDispatchReadinessText(cityId),
            BuildRecoveryEtaText(hasPreviewDeltaForCity ? _resultRecoveryEtaDays : GetRecoveryDaysToReady(cityId)),
            string.IsNullOrEmpty(nextSuggestedActionText) ? "None" : nextSuggestedActionText,
            selectedWorldWriteback != null && selectedWorldWriteback.CityWriteback != null
                ? selectedWorldWriteback.CityWriteback.SummaryText
                : "None");
    }

    private OutcomeReadback BuildOutcomeReadbackSurfaceForContext(
        string cityId,
        string dungeonId,
        string cityLabel,
        string acknowledgementText,
        string nextSuggestedActionText,
        string latestReturnAftermathText,
        CityWriteback cityWriteback)
    {
        ExpeditionResult expeditionResult = _runtimeEconomyState != null && !string.IsNullOrEmpty(cityId)
            ? _runtimeEconomyState.GetLatestExpeditionResultForCity(cityId)
            : null;
        WorldWriteback selectedWorldWriteback = _runtimeEconomyState != null && !string.IsNullOrEmpty(cityId)
            ? _runtimeEconomyState.GetLatestWorldWritebackForCity(cityId)
            : new WorldWriteback();
        WorldWriteback latestWorldWriteback = _runtimeEconomyState != null
            ? _runtimeEconomyState.GetLatestWorldWriteback()
            : new WorldWriteback();
        bool hasPreviewDeltaForCity = _runtimeEconomyState != null &&
            !string.IsNullOrEmpty(cityId) &&
            _runtimeEconomyState.GetLatestWorldWritebackCityId() == cityId &&
            selectedWorldWriteback != null &&
            IsMeaningfulSnapshotText(selectedWorldWriteback.RunResultStateKey);
        string resultStateKey = selectedWorldWriteback != null && IsMeaningfulSnapshotText(selectedWorldWriteback.RunResultStateKey)
            ? selectedWorldWriteback.RunResultStateKey
            : expeditionResult != null
                ? expeditionResult.ResultStateKey
                : string.Empty;
        return ResultPipeline.BuildOutcomeReadback(
            expeditionResult,
            cityWriteback,
            selectedWorldWriteback,
            latestWorldWriteback != null && IsMeaningfulSnapshotText(latestWorldWriteback.WritebackSummaryText)
                ? latestWorldWriteback.WritebackSummaryText
                : "None",
            latestReturnAftermathText,
            cityId ?? string.Empty,
            cityLabel,
            resultStateKey,
            acknowledgementText,
            nextSuggestedActionText,
            nextSuggestedActionText,
            _runtimeEconomyState != null && !string.IsNullOrEmpty(cityId)
                ? _runtimeEconomyState.GetExpeditionStatusTextForCity(cityId)
                : "None",
            _runtimeEconomyState != null && !string.IsNullOrEmpty(cityId)
                ? _runtimeEconomyState.GetLastRunSurvivingMembersSummaryForCity(cityId)
                : "None",
            _runtimeEconomyState != null && !string.IsNullOrEmpty(cityId)
                ? _runtimeEconomyState.GetLastRunClearedEncountersSummaryForCity(cityId)
                : "None",
            _runtimeEconomyState != null && !string.IsNullOrEmpty(cityId)
                ? _runtimeEconomyState.GetLastRunEventChoiceSummaryForCity(cityId)
                : "None",
            _runtimeEconomyState != null && !string.IsNullOrEmpty(cityId)
                ? _runtimeEconomyState.GetLastRunLootBreakdownSummaryForCity(cityId)
                : "None",
            _runtimeEconomyState != null && !string.IsNullOrEmpty(cityId)
                ? _runtimeEconomyState.GetLastRunDungeonSummaryForCity(cityId)
                : "None",
            _runtimeEconomyState != null && !string.IsNullOrEmpty(cityId)
                ? _runtimeEconomyState.GetLastRunRouteSummaryForCity(cityId)
                : "None",
            hasPreviewDeltaForCity ? BuildLootAmountText(cityWriteback.StockBefore) : "None",
            hasPreviewDeltaForCity ? BuildLootAmountText(cityWriteback.StockAfter) : "None",
            hasPreviewDeltaForCity ? BuildSignedLootAmountText(cityWriteback.StockDelta) : "None",
            _runtimeEconomyState != null && !string.IsNullOrEmpty(cityId)
                ? _runtimeEconomyState.GetLastRunGearRewardSummaryForCity(cityId)
                : "None",
            _runtimeEconomyState != null && !string.IsNullOrEmpty(cityId)
                ? _runtimeEconomyState.GetLastRunEquipSwapSummaryForCity(cityId)
                : "None",
            _runtimeEconomyState != null && !string.IsNullOrEmpty(cityId)
                ? _runtimeEconomyState.GetLastRunGearContinuitySummaryForCity(cityId)
                : "None",
            _runtimeEconomyState != null ? _runtimeEconomyState.GetRecentExpeditionLogText(0) : "None",
            _runtimeEconomyState != null ? _runtimeEconomyState.GetRecentExpeditionLogText(1) : "None",
            _runtimeEconomyState != null ? _runtimeEconomyState.GetRecentExpeditionLogText(2) : "None",
            RecentWorldWritebackLog1Text,
            RecentWorldWritebackLog2Text,
            RecentWorldWritebackLog3Text);
    }

    private string ResolveOutcomeReadbackNextSuggestedActionText(WorldObservationSurfaceData observation, string cityId, string dungeonId)
    {
        if (observation != null &&
            observation.Launch != null &&
            IsMeaningfulSnapshotText(observation.Launch.RecommendedNextActionSummaryText))
        {
            return observation.Launch.RecommendedNextActionSummaryText;
        }

        if (!string.IsNullOrEmpty(cityId) && !string.IsNullOrEmpty(dungeonId))
        {
            string routeId = ResolveWorldSnapshotRouteId(cityId, dungeonId);
            PrototypeWorldDispatchBriefingSnapshot briefing = BuildDispatchBriefingSnapshot(cityId, dungeonId, routeId, false);
            PrototypeWorldSnapshot snapshot = BuildWorldSnapshot(cityId, dungeonId, routeId, briefing);
            LaunchReadiness launchReadiness = BuildLaunchReadiness(snapshot, briefing);
            if (launchReadiness != null && IsMeaningfulSnapshotText(launchReadiness.RecommendedActionText))
            {
                return launchReadiness.RecommendedActionText;
            }

            if (launchReadiness != null && launchReadiness.GateResult != null && IsMeaningfulSnapshotText(launchReadiness.GateResult.SummaryText))
            {
                return launchReadiness.GateResult.SummaryText;
            }
        }

        return string.IsNullOrEmpty(cityId)
            ? "No city selected for CityHub readback."
            : "Review the latest return and set the next dispatch from the world board.";
    }

    private string ResolveOutcomeReadbackLatestReturnAftermathText(WorldObservationSurfaceData observation, string cityId, string dungeonId)
    {
        if (observation != null &&
            observation.RecentOutcome != null &&
            IsMeaningfulSnapshotText(observation.RecentOutcome.RecentAftermathSummaryText))
        {
            return observation.RecentOutcome.RecentAftermathSummaryText;
        }

        return BuildWorldSnapshotAftermathEchoText(cityId, dungeonId, null);
    }

    private WorldSimCitySourceData BuildSelectedCityWorldSimInputData(WorldObservationSurfaceData observation, WorldBoardReadModel board)
    {
        WorldSimCitySourceData data = new WorldSimCitySourceData();
        WorldObservationSurfaceData safeObservation = observation ?? new WorldObservationSurfaceData();
        ExpeditionPrepSurfaceData prep = safeObservation.ExpeditionPrep ?? new ExpeditionPrepSurfaceData();
        string cityId = ResolveDispatchBriefingCityId();
        if (string.IsNullOrEmpty(cityId))
        {
            return data;
        }

        string dungeonId = ResolveDispatchBriefingDungeonId(cityId);
        CityStatusReadModel city = FindBoardCityStatusReadModel(board, cityId);
        CityDecisionReadModel decision = city != null ? city.Decision : new CityDecisionReadModel();
        data.HasSelectedCity = true;
        data.SelectedCityId = cityId;
        data.SelectedCityLabel = ResolveDispatchEntityDisplayName(cityId);
        data.CityManaShardStockText = safeObservation.SelectedEntity != null &&
            IsMeaningfulSnapshotText(safeObservation.SelectedEntity.SelectedCityManaShardStockText)
            ? safeObservation.SelectedEntity.SelectedCityManaShardStockText
            : "None";
        data.LinkedDungeonId = dungeonId ?? string.Empty;
        data.LinkedDungeonLabel = ResolveDispatchEntityDisplayName(dungeonId);
        data.RecommendedRouteId = prep.RecommendedRouteId ?? string.Empty;
        data.RecommendedRouteLabel = IsMeaningfulSnapshotText(prep.RecommendedRouteLabel)
            ? prep.RecommendedRouteLabel
            : "None";
        data.RecommendedRouteSummaryText = IsMeaningfulSnapshotText(prep.RecommendedRouteSummaryText)
            ? prep.RecommendedRouteSummaryText
            : "None";
        data.NeedPressureText = IsMeaningfulSnapshotText(prep.NeedPressureText)
            ? prep.NeedPressureText
            : "None";
        data.DispatchReadinessText = IsMeaningfulSnapshotText(prep.DispatchReadinessText)
            ? prep.DispatchReadinessText
            : "None";
        data.RecoveryProgressText = IsMeaningfulSnapshotText(prep.RecoveryProgressText)
            ? prep.RecoveryProgressText
            : "None";
        data.RecoveryEtaText = IsMeaningfulSnapshotText(prep.RecoveryEtaText)
            ? prep.RecoveryEtaText
            : "None";
        data.DispatchPolicyText = IsMeaningfulSnapshotText(prep.DispatchPolicyText)
            ? prep.DispatchPolicyText
            : "None";
        data.RecentOutcomeText = safeObservation.RecentOutcome != null &&
            IsMeaningfulSnapshotText(safeObservation.RecentOutcome.SelectedLastExpeditionResultText)
            ? safeObservation.RecentOutcome.SelectedLastExpeditionResultText
            : safeObservation.RecentOutcome != null &&
                IsMeaningfulSnapshotText(safeObservation.RecentOutcome.SelectedLastDispatchImpactText)
                ? safeObservation.RecentOutcome.SelectedLastDispatchImpactText
                : "None";
        data.PressureBoardSummaryText = BuildSelectedCityPressureBoardSummaryText(city);
        data.WhyCityMattersText = IsMeaningfulSnapshotText(decision != null ? decision.WhyCityMattersText : string.Empty)
            ? decision.WhyCityMattersText
            : data.RecentOutcomeText;
        data.RecentResultEvidenceText = BuildCityRecentResultEvidenceText(city);
        data.PartyReadinessSummaryText = BuildCityPartyReadinessSummaryText(city);
        data.CanRecruitParty = prep.TotalPartyCount == 0;
        data.CanOpenPartyRoster = true;
        data.HasStagedParty = prep.HasStagedParty;
        data.CanOpenExpeditionPrep = prep.CanOpenBoard;
        data.StagedPartyId = prep.StagedPartyId ?? string.Empty;
        data.StagedPartyLabel = IsMeaningfulSnapshotText(prep.StagedPartyLabel)
            ? prep.StagedPartyLabel
            : "None";
        data.BlockedReasonText = IsMeaningfulSnapshotText(prep.BlockedReasonText)
            ? prep.BlockedReasonText
            : "None";
        data.RecommendedNextActionText = IsMeaningfulSnapshotText(prep.RecommendedNextActionText)
            ? prep.RecommendedNextActionText
            : "None";
        data.PartyRosterSummaryText = IsMeaningfulSnapshotText(prep.StagedPartySummaryText)
            ? prep.StagedPartySummaryText
            : "None";
        data.ExpeditionContextSummaryText = IsMeaningfulSnapshotText(prep.BoardSummaryText)
            ? prep.BoardSummaryText
            : "None";
        data.AvailabilitySummaryText = safeObservation.SelectedEntity != null &&
            IsMeaningfulSnapshotText(safeObservation.SelectedEntity.SelectedAvailableContractText)
            ? safeObservation.SelectedEntity.SelectedAvailableContractText
            : _runtimeEconomyState != null
                ? _runtimeEconomyState.GetAvailableContractTextForCity(cityId)
                : "None";
        data.SelectionSummaryText = IsMeaningfulSnapshotText(safeObservation.CurrentWorldObservationSummaryText)
            ? safeObservation.CurrentWorldObservationSummaryText
            : data.SelectedCityLabel + " -> " + data.LinkedDungeonLabel;
        return data;
    }

    private CityStatusReadModel FindBoardCityStatusReadModel(WorldBoardReadModel board, string cityId)
    {
        CityStatusReadModel[] cities = board != null ? board.Cities : null;
        if (cities == null || cities.Length <= 0 || string.IsNullOrEmpty(cityId))
        {
            return null;
        }

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

    private string BuildSelectedCityPressureBoardSummaryText(CityStatusReadModel city)
    {
        if (city == null)
        {
            return "None";
        }

        CityDecisionReadModel decision = city.Decision ?? new CityDecisionReadModel();
        CityBottleneckSignal topBottleneck = GetFirstBoardItem(decision.Bottlenecks);
        CityActionRecommendation topAction = GetFirstBoardItem(decision.RecommendedActions);
        List<string> parts = new List<string>();

        if (IsMeaningfulSnapshotText(topBottleneck != null ? topBottleneck.SummaryText : string.Empty))
        {
            parts.Add(topBottleneck.SummaryText);
        }
        else
        {
            parts.Add(BuildCityUrgencyFallbackText(city));
        }

        if (IsMeaningfulSnapshotText(topAction != null ? topAction.SummaryText : string.Empty))
        {
            parts.Add("Answer " + topAction.SummaryText);
        }
        else if (IsMeaningfulSnapshotText(city.RecommendedRouteSummaryText))
        {
            parts.Add("Answer " + city.RecommendedRouteSummaryText);
        }

        return parts.Count > 0 ? string.Join(" | ", parts.ToArray()) : "None";
    }

    private CitySimSurfaceData BuildSelectedCitySimSurfaceData(WorldObservationSurfaceData observation)
    {
        CitySimSurfaceData data = new CitySimSurfaceData();
        WorldObservationSurfaceData safeObservation = observation ?? new WorldObservationSurfaceData();
        ExpeditionPrepSurfaceData prep = safeObservation.ExpeditionPrep ?? new ExpeditionPrepSurfaceData();
        string cityId = ResolveDispatchBriefingCityId();
        if (string.IsNullOrEmpty(cityId))
        {
            return data;
        }

        data.CityId = cityId;
        data.CityLabel = ResolveDispatchEntityDisplayName(cityId);
        data.NeedPressureText = IsMeaningfulSnapshotText(prep.NeedPressureText)
            ? prep.NeedPressureText
            : "None";
        data.DispatchReadinessText = IsMeaningfulSnapshotText(prep.DispatchReadinessText)
            ? prep.DispatchReadinessText
            : "None";
        data.RecoveryProgressText = IsMeaningfulSnapshotText(prep.RecoveryProgressText)
            ? prep.RecoveryProgressText
            : "None";
        data.RecoveryEtaText = IsMeaningfulSnapshotText(prep.RecoveryEtaText)
            ? prep.RecoveryEtaText
            : "None";
        data.DispatchPolicyText = IsMeaningfulSnapshotText(prep.DispatchPolicyText)
            ? prep.DispatchPolicyText
            : "None";
        data.RecommendedRouteSummaryText = IsMeaningfulSnapshotText(prep.RecommendedRouteSummaryText)
            ? prep.RecommendedRouteSummaryText
            : "None";
        data.RecommendationReasonText = IsMeaningfulSnapshotText(prep.RecommendationReasonText)
            ? prep.RecommendationReasonText
            : "None";
        data.CityVacancyText = IsMeaningfulSnapshotText(prep.CityVacancyText)
            ? prep.CityVacancyText
            : "None";
        data.LastDispatchImpactText = GetLastDispatchImpactText(cityId);
        data.LastDispatchStockDeltaText = GetLastDispatchStockDeltaText(cityId);
        data.LastNeedPressureChangeText = GetLastNeedPressureChangeText(cityId);
        data.LastDispatchReadinessChangeText = GetLastDispatchReadinessChangeText(cityId);
        data.LastExpeditionResultText = safeObservation.RecentOutcome != null &&
            IsMeaningfulSnapshotText(safeObservation.RecentOutcome.SelectedLastExpeditionResultText)
            ? safeObservation.RecentOutcome.SelectedLastExpeditionResultText
            : "None";
        data.WorldWritebackText = safeObservation.SelectedEntity != null &&
            IsMeaningfulSnapshotText(safeObservation.SelectedEntity.SelectedWorldWritebackText)
            ? safeObservation.SelectedEntity.SelectedWorldWritebackText
            : "None";
        return data;
    }
}
