public sealed partial class StaticPlaceholderWorldView
{
    public AppFlowObservedSnapshot BuildAppFlowSnapshot()
    {
        RefreshDungeonRunContracts();
        AppFlowObservedSnapshot snapshot = new AppFlowObservedSnapshot();
        snapshot.WorldSelection = BuildWorldSelectionContext();
        snapshot.ActiveExpeditionPlan = BuildExpeditionPlanContext();
        snapshot.CurrentDungeonRun = BuildDungeonRunContext();
        snapshot.PendingBattle = BuildBattleContext();
        snapshot.LatestResult = BuildResultContext();
        snapshot.HasPendingWorldReturn = _pendingDungeonExit;
        snapshot.ObservedStage = DetermineObservedAppFlowStage();
        return snapshot;
    }

    private AppFlowStage DetermineObservedAppFlowStage()
    {
        if (IsDungeonResultPanelVisible)
        {
            return AppFlowStage.ResultPipeline;
        }

        if (IsBattleViewActive)
        {
            return AppFlowStage.BattleScene;
        }

        if (IsDungeonRouteChoiceVisible)
        {
            return AppFlowStage.ExpeditionPrep;
        }

        if (IsDungeonRunActive)
        {
            return AppFlowStage.DungeonRun;
        }

        return IsCityHubSelection() ? AppFlowStage.CityHub : AppFlowStage.WorldSim;
    }

    private AppFlowWorldSelection BuildWorldSelectionContext()
    {
        AppFlowWorldSelection context = new AppFlowWorldSelection();
        string cityId = GetSelectedAppFlowCityId();
        string dungeonId = GetSelectedAppFlowDungeonId();

        context.WorldDayCount = WorldDayCount;
        context.SelectedCityId = cityId;
        context.SelectedCityLabel = GetEntityLabel(cityId);
        context.SelectedDungeonId = dungeonId;
        context.SelectedDungeonLabel = GetEntityLabel(dungeonId);
        context.IdlePartyId = _runtimeEconomyState != null && HasText(cityId)
            ? _runtimeEconomyState.GetIdlePartyIdInCity(cityId)
            : string.Empty;
        return context;
    }

    private AppFlowExpeditionPlan BuildExpeditionPlanContext()
    {
        ExpeditionPrepReadModel prepReadModel = GetCurrentExpeditionPrepReadModel();
        ExpeditionPlan activePlan = GetCurrentExpeditionPlanForAppFlow();
        AppFlowExpeditionPlan context = new AppFlowExpeditionPlan();
        context.CityId = _currentHomeCityId ?? string.Empty;
        context.CityLabel = HasText(_currentHomeCityId) ? GetHomeCityDisplayName() : "None";
        context.DungeonId = _currentDungeonId ?? string.Empty;
        context.DungeonLabel = HasText(_currentDungeonName) ? _currentDungeonName : "None";
        context.PartyId = _activeDungeonParty != null ? _activeDungeonParty.PartyId : string.Empty;
        context.SelectedRouteId = HasText(_selectedRouteChoiceId) ? _selectedRouteChoiceId : _selectedRouteId;
        context.SelectedRouteLabel = HasText(_selectedRouteLabel) ? _selectedRouteLabel : "None";
        context.RecommendedRouteId = _recommendedRouteId ?? string.Empty;
        context.RecommendedRouteLabel = HasText(_recommendedRouteLabel) ? _recommendedRouteLabel : "None";
        context.DispatchPolicyText = GetCurrentDispatchPolicyText();
        context.ObjectiveText = prepReadModel != null && HasText(prepReadModel.ObjectiveText) ? prepReadModel.ObjectiveText : "None";
        context.WhyNowText = prepReadModel != null && HasText(prepReadModel.WhyNowText) ? prepReadModel.WhyNowText : "None";
        context.ExpectedUsefulnessText = prepReadModel != null && HasText(prepReadModel.ExpectedUsefulnessText) ? prepReadModel.ExpectedUsefulnessText : "None";
        context.RiskRewardPreviewText = prepReadModel != null && HasText(prepReadModel.RiskRewardPreviewText) ? prepReadModel.RiskRewardPreviewText : "None";
        context.PlanSummaryText = activePlan != null && HasText(activePlan.SummaryText) ? activePlan.SummaryText : "None";
        context.HasConfirmedPlan = activePlan != null && activePlan.IsConfirmed;
        context.PrepReadModel = prepReadModel ?? new ExpeditionPrepReadModel();
        context.LaunchReadiness = prepReadModel != null ? prepReadModel.LaunchReadiness : new LaunchReadiness();
        context.CurrentPlan = activePlan ?? new ExpeditionPlan();
        context.ConfirmedPlan = activePlan != null && activePlan.IsConfirmed ? activePlan : new ExpeditionPlan();
        return context;
    }

    private AppFlowDungeonRunContext BuildDungeonRunContext()
    {
        PrototypeBattleResultSnapshot lastBattleSnapshot = CopyBattleResultSnapshot(_currentBattleResultSnapshot);
        ExpeditionPlan launchPlan = GetCurrentExpeditionPlanForAppFlow();
        ExpeditionRunState runState = GetCurrentExpeditionRunState();
        DungeonRunReadModel screenModel = GetCurrentDungeonRunReadModel();
        AppFlowDungeonRunContext context = new AppFlowDungeonRunContext();
        context.CityId = _currentHomeCityId ?? string.Empty;
        context.CityLabel = HasText(_currentHomeCityId) ? GetHomeCityDisplayName() : "None";
        context.DungeonId = _currentDungeonId ?? string.Empty;
        context.DungeonLabel = HasText(_currentDungeonName) ? _currentDungeonName : "None";
        context.PartyId = _activeDungeonParty != null ? _activeDungeonParty.PartyId : string.Empty;
        context.RouteId = HasText(_selectedRouteId) ? _selectedRouteId : _selectedRouteChoiceId;
        context.RouteLabel = HasText(_selectedRouteLabel) ? _selectedRouteLabel : "None";
        context.TurnCount = _runTurnCount;
        context.LastBattleOutcomeKey = runState != null &&
                                       runState.LastBattleReturn != null &&
                                       HasText(runState.LastBattleReturn.OutcomeKey) &&
                                       runState.LastBattleReturn.OutcomeKey != PrototypeBattleOutcomeKeys.None
            ? runState.LastBattleReturn.OutcomeKey
            : lastBattleSnapshot != null &&
              HasText(lastBattleSnapshot.ResultStateKey) &&
              lastBattleSnapshot.ResultStateKey != PrototypeBattleOutcomeKeys.None
                ? lastBattleSnapshot.ResultStateKey
            : PrototypeBattleOutcomeKeys.None;
        context.LastEncounterName = runState != null &&
                                    runState.LastBattleReturn != null &&
                                    HasText(runState.LastBattleReturn.EncounterName)
            ? runState.LastBattleReturn.EncounterName
            : lastBattleSnapshot != null &&
              ((HasText(lastBattleSnapshot.ResultStateKey) && lastBattleSnapshot.ResultStateKey != PrototypeBattleOutcomeKeys.None) ||
               (HasText(lastBattleSnapshot.OutcomeKey) && lastBattleSnapshot.OutcomeKey != PrototypeBattleOutcomeKeys.None)) &&
              HasText(lastBattleSnapshot.EncounterName)
                ? lastBattleSnapshot.EncounterName
            : "None";
        context.LaunchPlan = launchPlan ?? new ExpeditionPlan();
        context.RunState = runState ?? new ExpeditionRunState();
        context.ScreenModel = screenModel ?? new DungeonRunReadModel();
        return context;
    }

    private AppFlowBattleContext BuildBattleContext()
    {
        ExpeditionRunState runState = GetCurrentExpeditionRunState();
        AppFlowBattleContext context = new AppFlowBattleContext();
        context.ReturnPayload = runState != null ? runState.LastBattleReturn : new BattleReturnPayload();
        if (!IsBattleViewActive)
        {
            return context;
        }

        PrototypeBattleResultSnapshot resultSnapshot = BuildBattleResultSnapshot();
        context.EncounterId = resultSnapshot != null ? resultSnapshot.EncounterId : string.Empty;
        context.EncounterName = resultSnapshot != null && HasText(resultSnapshot.EncounterName)
            ? resultSnapshot.EncounterName
            : GetCurrentEncounterNameText();
        context.EncounterRoomType = GetEncounterRoomTypeText();
        context.BattleStateLabel = BattleStateText;
        context.ResultSnapshot = resultSnapshot ?? new PrototypeBattleResultSnapshot();
        context.HandoffPayload = runState != null ? runState.PendingBattle : new BattleHandoffPayload();
        return context;
    }

    private AppFlowResultContext BuildResultContext()
    {
        string cityId = ResolveResultContextCityId();
        ExpeditionOutcome expeditionOutcome = BuildResultContextExpeditionOutcome(cityId);
        OutcomeReadback outcomeReadback = BuildResultContextOutcomeReadback(cityId);
        ExpeditionRunState runState = GetCurrentExpeditionRunState();
        AppFlowResultContext context = new AppFlowResultContext();
        context.ExpeditionOutcome = expeditionOutcome;
        context.OutcomeReadback = outcomeReadback;
        context.RunResultSnapshot = LatestRpgRunResultSnapshot;
        context.ResolvedRunState = runState ?? new ExpeditionRunState();
        context.IsAppliedToWorld = HasText(cityId) && HasMeaningfulResult(expeditionOutcome, outcomeReadback);
        context.AppliedWorldStateMarker = BuildResultAppliedMarker(expeditionOutcome, outcomeReadback);
        return context;
    }

    private string ResolveResultContextCityId()
    {
        if (HasText(_currentHomeCityId))
        {
            return _currentHomeCityId;
        }

        string selectedCityId = GetSelectedAppFlowCityId();
        if (HasText(selectedCityId))
        {
            return selectedCityId;
        }

        if (_runtimeEconomyState == null)
        {
            return string.Empty;
        }

        OutcomeReadback latestOutcomeReadback = _runtimeEconomyState.GetLatestOutcomeReadback();
        if (HasText(latestOutcomeReadback.SourceCityId))
        {
            return latestOutcomeReadback.SourceCityId;
        }

        ExpeditionOutcome latestExpeditionOutcome = _runtimeEconomyState.GetLatestExpeditionOutcome();
        return HasText(latestExpeditionOutcome.SourceCityId)
            ? latestExpeditionOutcome.SourceCityId
            : string.Empty;
    }

    private ExpeditionOutcome BuildResultContextExpeditionOutcome(string cityId)
    {
        if (_runtimeEconomyState == null)
        {
            return new ExpeditionOutcome();
        }

        ExpeditionOutcome cityOutcome = HasText(cityId)
            ? _runtimeEconomyState.GetLatestExpeditionOutcomeForCity(cityId)
            : new ExpeditionOutcome();
        return HasMeaningfulOutcome(cityOutcome)
            ? CopyExpeditionOutcome(cityOutcome)
            : CopyExpeditionOutcome(_runtimeEconomyState.GetLatestExpeditionOutcome());
    }

    private OutcomeReadback BuildResultContextOutcomeReadback(string cityId)
    {
        if (_runtimeEconomyState == null)
        {
            return new OutcomeReadback();
        }

        OutcomeReadback cityReadback = HasText(cityId)
            ? _runtimeEconomyState.GetLatestOutcomeReadbackForCity(cityId)
            : new OutcomeReadback();
        return HasMeaningfulReadback(cityReadback)
            ? CopyOutcomeReadbackForAppFlow(cityReadback)
            : CopyOutcomeReadbackForAppFlow(_runtimeEconomyState.GetLatestOutcomeReadback());
    }

    private string BuildResultAppliedMarker(ExpeditionOutcome expeditionOutcome, OutcomeReadback outcomeReadback)
    {
        if (HasText(outcomeReadback != null ? outcomeReadback.CityStatusChangeSummaryText : string.Empty))
        {
            return outcomeReadback.CityStatusChangeSummaryText;
        }

        if (HasText(outcomeReadback != null ? outcomeReadback.ExpeditionLogEntryText : string.Empty))
        {
            return outcomeReadback.ExpeditionLogEntryText;
        }

        if (HasText(expeditionOutcome != null ? expeditionOutcome.ResultSummaryText : string.Empty))
        {
            return expeditionOutcome.ResultSummaryText;
        }

        return "None";
    }

    private bool IsCityHubSelection()
    {
        return _selectedMarker != null &&
               _selectedMarker.EntityData != null &&
               _selectedMarker.EntityData.Kind == WorldEntityKind.City;
    }

    private string GetSelectedAppFlowCityId()
    {
        if (_selectedMarker == null || _selectedMarker.EntityData == null)
        {
            return string.Empty;
        }

        if (_selectedMarker.EntityData.Kind == WorldEntityKind.City)
        {
            return _selectedMarker.EntityData.Id ?? string.Empty;
        }

        if (_selectedMarker.EntityData.Kind == WorldEntityKind.Dungeon)
        {
            return _selectedMarker.EntityData.LinkedCityId ?? string.Empty;
        }

        return string.Empty;
    }

    private string GetSelectedAppFlowDungeonId()
    {
        return HasText(GetSelectedExpeditionDungeonId()) ? GetSelectedExpeditionDungeonId() : string.Empty;
    }

    private string GetEntityLabel(string entityId)
    {
        WorldEntityData entity = FindEntity(entityId);
        return entity != null && HasText(entity.DisplayName)
            ? entity.DisplayName
            : HasText(entityId)
                ? entityId
                : "None";
    }

    private static ExpeditionOutcome CopyExpeditionOutcome(ExpeditionOutcome source)
    {
        ExpeditionOutcome safeSource = source ?? new ExpeditionOutcome();
        ExpeditionOutcome copy = new ExpeditionOutcome();
        copy.SourceCityId = safeSource.SourceCityId;
        copy.SourceCityLabel = safeSource.SourceCityLabel;
        copy.TargetDungeonId = safeSource.TargetDungeonId;
        copy.TargetDungeonLabel = safeSource.TargetDungeonLabel;
        copy.RewardResourceId = safeSource.RewardResourceId;
        copy.ResultStateKey = safeSource.ResultStateKey;
        copy.Success = safeSource.Success;
        copy.ReturnedLootAmount = safeSource.ReturnedLootAmount;
        copy.TotalTurnsTaken = safeSource.TotalTurnsTaken;
        copy.ClearedEncounterCount = safeSource.ClearedEncounterCount;
        copy.OpenedChestCount = safeSource.OpenedChestCount;
        copy.SurvivingMemberCount = safeSource.SurvivingMemberCount;
        copy.KnockedOutMemberCount = safeSource.KnockedOutMemberCount;
        copy.EliteDefeated = safeSource.EliteDefeated;
        copy.ResultSummaryText = safeSource.ResultSummaryText;
        copy.LootSummaryText = safeSource.LootSummaryText;
        copy.SurvivingMembersSummaryText = safeSource.SurvivingMembersSummaryText;
        copy.ClearedEncountersSummaryText = safeSource.ClearedEncountersSummaryText;
        copy.EventChoiceSummaryText = safeSource.EventChoiceSummaryText;
        copy.LootBreakdownSummaryText = safeSource.LootBreakdownSummaryText;
        copy.RouteSummaryText = safeSource.RouteSummaryText;
        copy.DungeonSummaryText = safeSource.DungeonSummaryText;
        copy.MissionObjectiveText = safeSource.MissionObjectiveText;
        copy.MissionRelevanceText = safeSource.MissionRelevanceText;
        copy.RiskRewardContextText = safeSource.RiskRewardContextText;
        copy.RunPathSummaryText = safeSource.RunPathSummaryText;
        copy.OutcomeMeaningId = safeSource.OutcomeMeaningId;
        copy.OutcomeRewardMeaningText = safeSource.OutcomeRewardMeaningText;
        copy.CityImpactMeaningText = safeSource.CityImpactMeaningText;
        copy.RecommendationShiftText = safeSource.RecommendationShiftText;
        copy.PartyConditionText = safeSource.PartyConditionText;
        copy.PartyHpSummaryText = safeSource.PartyHpSummaryText;
        copy.EliteSummaryText = safeSource.EliteSummaryText;
        return copy;
    }

    private static OutcomeReadback CopyOutcomeReadbackForAppFlow(OutcomeReadback source)
    {
        OutcomeReadback safeSource = source ?? new OutcomeReadback();
        OutcomeReadback copy = new OutcomeReadback();
        copy.SourceCityId = safeSource.SourceCityId;
        copy.SourceCityLabel = safeSource.SourceCityLabel;
        copy.TargetDungeonId = safeSource.TargetDungeonId;
        copy.TargetDungeonLabel = safeSource.TargetDungeonLabel;
        copy.CityId = safeSource.CityId;
        copy.CityLabel = safeSource.CityLabel;
        copy.ResultStateKey = safeSource.ResultStateKey;
        copy.Success = safeSource.Success;
        copy.SummaryText = safeSource.SummaryText;
        copy.AcknowledgementText = safeSource.AcknowledgementText;
        copy.LatestReturnAftermathText = safeSource.LatestReturnAftermathText;
        copy.PostRunSummaryText = safeSource.PostRunSummaryText;
        copy.NextSuggestedActionText = safeSource.NextSuggestedActionText;
        copy.FollowUpHintText = safeSource.FollowUpHintText;
        copy.LastExpeditionResultText = safeSource.LastExpeditionResultText;
        copy.WorldWritebackSummaryText = safeSource.WorldWritebackSummaryText;
        copy.SelectedWorldWritebackText = safeSource.SelectedWorldWritebackText;
        copy.DungeonStatusText = safeSource.DungeonStatusText;
        copy.DungeonAvailabilityText = safeSource.DungeonAvailabilityText;
        copy.DungeonLastOutcomeText = safeSource.DungeonLastOutcomeText;
        copy.LootSummaryText = safeSource.LootSummaryText;
        copy.SurvivingMembersSummaryText = safeSource.SurvivingMembersSummaryText;
        copy.ClearedEncountersSummaryText = safeSource.ClearedEncountersSummaryText;
        copy.EventChoiceSummaryText = safeSource.EventChoiceSummaryText;
        copy.LootBreakdownSummaryText = safeSource.LootBreakdownSummaryText;
        copy.RouteSummaryText = safeSource.RouteSummaryText;
        copy.DungeonSummaryText = safeSource.DungeonSummaryText;
        copy.StockBeforeText = safeSource.StockBeforeText;
        copy.StockAfterText = safeSource.StockAfterText;
        copy.StockDeltaText = safeSource.StockDeltaText;
        copy.NeedPressureBeforeText = safeSource.NeedPressureBeforeText;
        copy.NeedPressureAfterText = safeSource.NeedPressureAfterText;
        copy.DispatchReadinessBeforeText = safeSource.DispatchReadinessBeforeText;
        copy.DispatchReadinessAfterText = safeSource.DispatchReadinessAfterText;
        copy.RecoveryEtaText = safeSource.RecoveryEtaText;
        copy.MissionObjectiveText = safeSource.MissionObjectiveText;
        copy.MissionRelevanceText = safeSource.MissionRelevanceText;
        copy.RiskRewardContextText = safeSource.RiskRewardContextText;
        copy.RunPathSummaryText = safeSource.RunPathSummaryText;
        copy.OutcomeMeaningId = safeSource.OutcomeMeaningId;
        copy.OutcomeRewardMeaningText = safeSource.OutcomeRewardMeaningText;
        copy.CityImpactMeaningText = safeSource.CityImpactMeaningText;
        copy.RecommendationShiftText = safeSource.RecommendationShiftText;
        copy.GearRewardSummaryText = safeSource.GearRewardSummaryText;
        copy.EquipSwapSummaryText = safeSource.EquipSwapSummaryText;
        copy.GearContinuitySummaryText = safeSource.GearContinuitySummaryText;
        copy.RecentExpeditionLog1Text = safeSource.RecentExpeditionLog1Text;
        copy.RecentExpeditionLog2Text = safeSource.RecentExpeditionLog2Text;
        copy.RecentExpeditionLog3Text = safeSource.RecentExpeditionLog3Text;
        copy.RecentWorldWritebackLog1Text = safeSource.RecentWorldWritebackLog1Text;
        copy.RecentWorldWritebackLog2Text = safeSource.RecentWorldWritebackLog2Text;
        copy.RecentWorldWritebackLog3Text = safeSource.RecentWorldWritebackLog3Text;
        copy.CityStatusChangeSummaryText = safeSource.CityStatusChangeSummaryText;
        copy.ExpeditionLogEntryText = safeSource.ExpeditionLogEntryText;
        copy.PartyConditionText = safeSource.PartyConditionText;
        copy.PartyHpSummaryText = safeSource.PartyHpSummaryText;
        copy.EliteSummaryText = safeSource.EliteSummaryText;
        return copy;
    }

    private static bool HasMeaningfulResult(ExpeditionOutcome expeditionOutcome, OutcomeReadback outcomeReadback)
    {
        return HasText(expeditionOutcome != null ? expeditionOutcome.SourceCityId : string.Empty) ||
               HasText(outcomeReadback != null ? outcomeReadback.SourceCityId : string.Empty) ||
               HasText(outcomeReadback != null ? outcomeReadback.CityId : string.Empty) ||
               HasText(outcomeReadback != null ? outcomeReadback.SummaryText : string.Empty) ||
               HasText(outcomeReadback != null ? outcomeReadback.PostRunSummaryText : string.Empty);
    }

    private static bool HasMeaningfulOutcome(ExpeditionOutcome expeditionOutcome)
    {
        return expeditionOutcome != null &&
               (HasText(expeditionOutcome.SourceCityId) ||
                HasText(expeditionOutcome.TargetDungeonId) ||
                HasText(expeditionOutcome.ResultSummaryText));
    }

    private static bool HasMeaningfulReadback(OutcomeReadback outcomeReadback)
    {
        return outcomeReadback != null &&
               (HasText(outcomeReadback.SourceCityId) ||
                HasText(outcomeReadback.CityId) ||
                HasText(outcomeReadback.SummaryText) ||
                HasText(outcomeReadback.PostRunSummaryText) ||
                HasText(outcomeReadback.CityStatusChangeSummaryText));
    }

    private static bool HasText(string value)
    {
        return !string.IsNullOrEmpty(value) && value != "None";
    }
}
