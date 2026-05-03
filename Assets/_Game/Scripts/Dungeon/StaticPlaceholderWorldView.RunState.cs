using System;
using System.Collections.Generic;
using UnityEngine;

public sealed partial class StaticPlaceholderWorldView
{
    private ExpeditionRunState _activeExpeditionRunState = new ExpeditionRunState();
    private DungeonRunReadModel _activeDungeonRunReadModel = new DungeonRunReadModel();

    private void RefreshDungeonRunContracts()
    {
        EnsureBattleContracts();
        _activeExpeditionRunState = BuildExpeditionRunStateInternal();
        _activeDungeonRunReadModel = DungeonRunCoordinator.BuildReadModel(_activeExpeditionRunState);
    }

    private ExpeditionRunState GetCurrentExpeditionRunState()
    {
        if (!HasMeaningfulRunState(_activeExpeditionRunState))
        {
            RefreshDungeonRunContracts();
        }

        return _activeExpeditionRunState ?? new ExpeditionRunState();
    }

    private DungeonRunReadModel GetCurrentDungeonRunReadModel()
    {
        if (!HasMeaningfulRunReadModel(_activeDungeonRunReadModel))
        {
            RefreshDungeonRunContracts();
        }

        return _activeDungeonRunReadModel ?? new DungeonRunReadModel();
    }

    private void ResetDungeonRunStateContracts()
    {
        _activeExpeditionRunState = new ExpeditionRunState();
        _activeDungeonRunReadModel = new DungeonRunReadModel();
    }

    private ExpeditionRunState BuildExpeditionRunStateInternal()
    {
        if (!IsDungeonRunActive && !HasText(_currentDungeonId) && !HasText(_currentHomeCityId))
        {
            return new ExpeditionRunState();
        }

        ExpeditionPlan launchPlan = GetCurrentExpeditionPlanForAppFlow();
        ExpeditionRunStateSeed seed = new ExpeditionRunStateSeed();
        seed.RunId = BuildActiveRunId(launchPlan);
        seed.WorldDayCount = WorldDayCount;
        seed.LaunchPlan = launchPlan ?? new ExpeditionPlan();
        seed.Phase = ResolveRunPhase();
        seed.Status = ResolveRunStatus();
        seed.OriginCityId = _currentHomeCityId ?? string.Empty;
        seed.OriginCityLabel = HasText(_currentHomeCityId) ? GetHomeCityDisplayName() : "None";
        seed.TargetDungeonId = _currentDungeonId ?? string.Empty;
        seed.TargetDungeonLabel = HasText(_currentDungeonName) ? _currentDungeonName : "None";
        seed.RouteId = HasText(_selectedRouteId) ? _selectedRouteId : _selectedRouteChoiceId;
        seed.RouteLabel = HasText(_selectedRouteLabel) ? _selectedRouteLabel : "None";
        seed.RouteRiskText = HasText(_selectedRouteRiskLabel) ? _selectedRouteRiskLabel : "None";
        seed.RouteContextText = BuildSelectedRouteContextText();
        seed.ObjectiveText = HasText(launchPlan != null ? launchPlan.ObjectiveText : string.Empty) ? launchPlan.ObjectiveText : "None";
        seed.WhyNowText = HasText(launchPlan != null ? launchPlan.WhyNowText : string.Empty) ? launchPlan.WhyNowText : "None";
        seed.ExpectedUsefulnessText = HasText(launchPlan != null ? launchPlan.ExpectedUsefulnessText : string.Empty) ? launchPlan.ExpectedUsefulnessText : "None";
        ExpeditionPrepReadModel prepReadModel = GetCurrentExpeditionPrepReadModel();
        seed.RiskRewardPreviewText = HasText(launchPlan != null ? launchPlan.RiskRewardPreviewText : string.Empty)
            ? launchPlan.RiskRewardPreviewText
            : HasText(prepReadModel != null ? prepReadModel.RiskRewardPreviewText : string.Empty)
                ? prepReadModel.RiskRewardPreviewText
            : launchPlan != null &&
              launchPlan.SelectedRoute != null &&
              HasText(launchPlan.SelectedRoute.RewardPreviewText)
                ? launchPlan.SelectedRoute.RewardPreviewText
            : BuildSelectedRouteSummary();
        seed.PlanSummaryText = HasText(launchPlan != null ? launchPlan.SummaryText : string.Empty) ? launchPlan.SummaryText : "None";
        seed.PartyId = _activeDungeonParty != null ? _activeDungeonParty.PartyId : string.Empty;
        seed.PartySummaryText = ActiveDungeonPartyText;
        seed.PartyHpSummaryText = BuildTotalPartyHpSummary();
        seed.PartyConditionText = GetPartyConditionText();
        seed.SustainPressureText = GetSustainPressureText();
        seed.CurrentRoomId = _currentRoomStepId ?? string.Empty;
        seed.CurrentRoomLabel = GetCurrentRoomText();
        seed.CurrentRoomTypeLabel = GetCurrentRoomTypeText();
        seed.RoomProgressText = GetRoomProgressText();
        seed.NextGoalText = GetNextMajorGoalText();
        seed.CurrentPromptText = CurrentSelectionPromptText;
        seed.ResultStateKey = BuildRunResultStateKey();
        seed.ResultSummaryText = BuildRunResultSummaryText();
        seed.RoomPathSummaryText = BuildCurrentRoomPathSummary();
        seed.CurrentTurnCount = Mathf.Max(_runTurnCount, _battleTurnIndex);
        seed.ClearedEncounterCount = Mathf.Max(0, _clearedEncounterCount);
        seed.OpenedChestCount = Mathf.Max(0, _chestOpenedCount);
        seed.CarriedLootAmount = Mathf.Max(0, _carriedLootAmount);
        seed.ExitUnlocked = _exitUnlocked;
        seed.EliteDefeated = _eliteDefeated;
        seed.EventResolved = _eventResolved;
        seed.PreEliteDecisionResolved = _preEliteDecisionResolved;
        seed.RunSucceeded = _runResultState == RunResultState.Clear;
        seed.RunFailed = _runResultState == RunResultState.Defeat;
        seed.RunRetreated = _runResultState == RunResultState.Retreat;
        seed.IsResolved = _dungeonRunState == DungeonRunState.ResultPanel || _pendingDungeonExit;
        seed.ActiveEncounter = BuildActiveEncounterContext();
        seed.LastEventChoice = BuildEventChoiceResult();
        seed.LastPreEliteChoice = BuildPreEliteChoiceResult();
        seed.PendingBattle = BuildBattleHandoffPayload();
        seed.LastBattleReturn = BuildBattleReturnPayload();
        seed.ActiveBattleState = GetBattleRuntimeState();
        seed.ActiveBattleViewModel = GetBattleViewModel();
        seed.LatestBattleSnapshot = CopyBattleResultSnapshot(_currentBattleResultSnapshot);
        seed.ResultSnapshot = CopyRpgRunResultSnapshot(_latestRpgRunResultSnapshot);
        seed.RecentEntries = BuildRecentRunLogEntries();
        return DungeonRunCoordinator.CreateState(seed);
    }

    private string BuildActiveRunId(ExpeditionPlan launchPlan)
    {
        if (launchPlan != null && HasText(launchPlan.PlanId))
        {
            return launchPlan.PlanId + "_run";
        }

        string cityId = HasText(_currentHomeCityId) ? _currentHomeCityId : "city";
        string dungeonId = HasText(_currentDungeonId) ? _currentDungeonId : "dungeon";
        string routeId = HasText(_selectedRouteId) ? _selectedRouteId : HasText(_selectedRouteChoiceId) ? _selectedRouteChoiceId : "route";
        return cityId + "_" + dungeonId + "_" + routeId + "_day" + WorldDayCount;
    }

    private RunPhase ResolveRunPhase()
    {
        if (_dungeonRunState == DungeonRunState.ResultPanel)
        {
            if (_runResultState == RunResultState.Clear)
            {
                return RunPhase.Completed;
            }

            if (_runResultState == RunResultState.Defeat)
            {
                return RunPhase.Failed;
            }

            if (_runResultState == RunResultState.Retreat)
            {
                return RunPhase.Retreated;
            }

            return RunPhase.Result;
        }

        if (_dungeonRunState == DungeonRunState.RouteChoice)
        {
            return RunPhase.Start;
        }

        if (_dungeonRunState == DungeonRunState.EventChoice)
        {
            return RunPhase.EventChoice;
        }

        if (_dungeonRunState == DungeonRunState.PreEliteChoice)
        {
            return RunPhase.PreEliteChoice;
        }

        if (_dungeonRunState == DungeonRunState.Battle)
        {
            return _battleState == BattleState.None ? RunPhase.PendingBattle : RunPhase.InBattle;
        }

        return _dungeonRunState == DungeonRunState.Explore
            ? RunPhase.Exploring
            : RunPhase.None;
    }

    private DungeonRunStatus ResolveRunStatus()
    {
        if (_dungeonRunState == DungeonRunState.ResultPanel)
        {
            if (_runResultState == RunResultState.Clear)
            {
                return DungeonRunStatus.Completed;
            }

            if (_runResultState == RunResultState.Defeat)
            {
                return DungeonRunStatus.Failed;
            }

            return DungeonRunStatus.Resolved;
        }

        if (_dungeonRunState == DungeonRunState.Battle)
        {
            return DungeonRunStatus.InBattle;
        }

        if (_dungeonRunState == DungeonRunState.RouteChoice ||
            _dungeonRunState == DungeonRunState.EventChoice ||
            _dungeonRunState == DungeonRunState.PreEliteChoice)
        {
            return DungeonRunStatus.AwaitingChoice;
        }

        return IsDungeonRunActive ? DungeonRunStatus.Active : DungeonRunStatus.Inactive;
    }

    private string BuildSelectedRouteContextText()
    {
        if (!HasText(_selectedRouteId) && !HasText(_selectedRouteChoiceId))
        {
            return "None";
        }

        string routeId = HasText(_selectedRouteId) ? _selectedRouteId : _selectedRouteChoiceId;
        string preview = HasText(_currentDungeonId)
            ? BuildRoomPathPreviewText(_currentDungeonId, routeId)
            : "None";
        string routeSummary = BuildSelectedRouteSummary();
        string routeFeel = HasText(_currentDungeonId)
            ? BuildRouteInternalFeelText(_currentDungeonId, routeId)
            : string.Empty;
        if (HasText(routeSummary) && HasText(preview))
        {
            return BuildScenarioPipeText(routeSummary, preview, BuildLabeledScenarioClause("Dungeon feel", routeFeel));
        }

        if (HasText(routeSummary))
        {
            return BuildScenarioPipeText(routeSummary, BuildLabeledScenarioClause("Dungeon feel", routeFeel));
        }

        return BuildScenarioPipeText(preview, BuildLabeledScenarioClause("Dungeon feel", routeFeel));
    }

    private string BuildRunResultStateKey()
    {
        if (HasText(_latestRpgRunResultSnapshot != null ? _latestRpgRunResultSnapshot.ResultStateKey : string.Empty))
        {
            return _latestRpgRunResultSnapshot.ResultStateKey;
        }

        if (_runResultState == RunResultState.Clear)
        {
            return PrototypeBattleOutcomeKeys.RunClear;
        }

        if (_runResultState == RunResultState.Defeat)
        {
            return PrototypeBattleOutcomeKeys.RunDefeat;
        }

        if (_runResultState == RunResultState.Retreat)
        {
            return PrototypeBattleOutcomeKeys.RunRetreat;
        }

        return PrototypeBattleOutcomeKeys.None;
    }

    private string BuildRunResultSummaryText()
    {
        if (HasText(_latestRpgRunResultSnapshot != null ? _latestRpgRunResultSnapshot.ResultSummary : string.Empty))
        {
            return _latestRpgRunResultSnapshot.ResultSummary;
        }

        if (_dungeonRunState == DungeonRunState.ResultPanel && HasText(_battleFeedbackText))
        {
            return _battleFeedbackText;
        }

        if (HasRecordedBattleReturn(_currentBattleResultSnapshot))
        {
            PrototypeBattleRequest request = HasMeaningfulBattleRequest(_activeBattleRequest)
                ? _activeBattleRequest
                : null;
            string encounterName = ResolveBattleReturnEncounterName(_currentBattleResultSnapshot, request);
            return BuildBattleReturnSummaryText(_currentBattleResultSnapshot, encounterName);
        }

        return "None";
    }

    private EncounterContext BuildActiveEncounterContext()
    {
        DungeonRoomTemplateData room = GetCurrentPlannedRoomStep();
        DungeonEncounterRuntimeData encounter = GetActiveEncounter();
        EncounterContext context = new EncounterContext();
        context.RoomId = room != null ? room.RoomId : _currentRoomStepId ?? string.Empty;
        context.RoomLabel = room != null ? room.DisplayName : GetCurrentRoomText();
        context.RoomTypeLabel = room != null ? room.RoomTypeLabel : GetCurrentRoomTypeText();
        context.RoomIndex = room != null ? Mathf.Max(1, GetCurrentPlannedRoomIndex() + 1) : Mathf.Max(1, _currentRoomIndex);
        context.TotalRooms = Mathf.Max(0, _plannedRooms.Count);
        context.EncounterId = encounter != null
            ? encounter.EncounterId
            : room != null && HasText(room.EncounterId)
                ? room.EncounterId
                : string.Empty;
        context.EncounterName = encounter != null
            ? encounter.DisplayName
            : HasText(GetCurrentEncounterNameText())
                ? GetCurrentEncounterNameText()
                : room != null
                    ? room.DisplayName
                    : string.Empty;
        context.IsEliteEncounter = encounter != null
            ? encounter.IsEliteEncounter
            : room != null && room.RoomType == DungeonRoomType.Elite;
        context.IsResolved = encounter != null
            ? encounter.IsCleared
            : room != null && IsRoomStepResolved(room);
        context.PromptText = BuildEncounterPromptText(room, encounter);
        context.PreviewText = BuildEncounterPreviewText(room, encounter);
        return context;
    }

    private string BuildEncounterPromptText(DungeonRoomTemplateData room, DungeonEncounterRuntimeData encounter)
    {
        if (_dungeonRunState == DungeonRunState.EventChoice)
        {
            return EventPromptText;
        }

        if (_dungeonRunState == DungeonRunState.PreEliteChoice)
        {
            return PreElitePromptText;
        }

        if (_dungeonRunState == DungeonRunState.Battle)
        {
            return CurrentSelectionPromptText;
        }

        if (encounter != null && !encounter.IsCleared)
        {
            return "Resolve " + encounter.DisplayName + ".";
        }

        if (room != null)
        {
            return "Advance through " + room.DisplayName + ".";
        }

        return CurrentSelectionPromptText;
    }

    private string BuildEncounterPreviewText(DungeonRoomTemplateData room, DungeonEncounterRuntimeData encounter)
    {
        if (encounter != null)
        {
            return encounter.DisplayName + " | " + (encounter.IsEliteEncounter ? "Elite Encounter" : "Encounter");
        }

        if (room != null)
        {
            return room.DisplayName + " | " + room.RoomTypeLabel;
        }

        return "None";
    }

    private EventChoiceResult BuildEventChoiceResult()
    {
        EventChoiceResult result = new EventChoiceResult();
        if (!_eventResolved &&
            _dungeonRunState != DungeonRunState.EventChoice &&
            !HasText(_selectedEventChoiceId) &&
            !IsMeaningfulRoomInteractionSummary(_roomInteractionSummaryText))
        {
            return result;
        }

        bool hasShrineChoice = HasText(_selectedEventChoiceId);
        result.EventId = hasShrineChoice ? "shrine_choice" : "room_interaction";
        result.EventLabel = hasShrineChoice ? GetCurrentEventTitleText() : GetCurrentRoomText();
        result.ChoiceId = _selectedEventChoiceId ?? string.Empty;
        result.Resolved = _eventResolved || IsMeaningfulRoomInteractionSummary(_roomInteractionSummaryText);
        if (_selectedEventChoiceId == "recover")
        {
            result.ChoiceLabel = GetCurrentEventOptionAText();
            result.OutcomeTag = "recover";
            result.RecoverAmount = Mathf.Max(0, _totalEventHealAmount);
        }
        else if (_selectedEventChoiceId == "loot")
        {
            result.ChoiceLabel = GetCurrentEventOptionBText();
            result.OutcomeTag = "bonus_loot";
            result.RewardAmount = Mathf.Max(0, _eventLootAmount);
        }
        else
        {
            result.ChoiceId = IsMeaningfulRoomInteractionSummary(_roomInteractionSummaryText) ? "room_interaction" : result.ChoiceId;
            result.ChoiceLabel = IsMeaningfulRoomInteractionSummary(_roomInteractionSummaryText) ? _roomInteractionSummaryText : "Pending";
            result.OutcomeTag = IsMeaningfulRoomInteractionSummary(_roomInteractionSummaryText) ? "room_interaction" : string.Empty;
            result.RewardAmount = Mathf.Max(0, _chestLootAmount);
        }

        result.ResultSummaryText = result.Resolved
            ? GetSelectedEventChoiceDisplayText()
            : EventPromptText;
        return result;
    }

    private EventChoiceResult BuildPreEliteChoiceResult()
    {
        EventChoiceResult result = new EventChoiceResult();
        if (!_preEliteDecisionResolved && _dungeonRunState != DungeonRunState.PreEliteChoice && !HasText(_selectedPreEliteChoiceId))
        {
            return result;
        }

        result.EventId = "pre_elite_choice";
        result.EventLabel = GetCurrentPreEliteTitleText();
        result.ChoiceId = _selectedPreEliteChoiceId ?? string.Empty;
        result.Resolved = _preEliteDecisionResolved;
        if (_selectedPreEliteChoiceId == "recover")
        {
            result.ChoiceLabel = GetCurrentPreEliteOptionAText();
            result.OutcomeTag = "recover";
            result.RecoverAmount = Mathf.Max(0, _preEliteHealAmount);
        }
        else if (_selectedPreEliteChoiceId == "bonus")
        {
            result.ChoiceLabel = GetCurrentPreEliteOptionBText();
            result.OutcomeTag = "bonus_reward";
            result.RewardAmount = _eliteBonusRewardGranted
                ? Mathf.Max(0, _eliteBonusRewardGrantedAmount)
                : Mathf.Max(0, _eliteBonusRewardPending);
        }
        else
        {
            result.ChoiceLabel = "Pending";
        }

        result.ResultSummaryText = result.Resolved
            ? GetSelectedPreEliteChoiceDisplayText()
            : PreElitePromptText;
        return result;
    }

    private BattleHandoffPayload BuildBattleHandoffPayload()
    {
        BattleHandoffPayload payload = new BattleHandoffPayload();
        if (_dungeonRunState != DungeonRunState.Battle)
        {
            return payload;
        }

        DungeonEncounterRuntimeData encounter = GetActiveEncounter();
        DungeonRoomTemplateData room = encounter != null ? GetRoomStepByEncounterId(encounter.EncounterId) : GetCurrentPlannedRoomStep();
        ExpeditionPlan launchPlan = GetCurrentExpeditionPlanForAppFlow();
        PrototypeBattleRequest request = GetBattleRequest();
        payload.EncounterId = encounter != null ? encounter.EncounterId : string.Empty;
        payload.EncounterName = encounter != null ? encounter.DisplayName : "None";
        payload.DungeonId = _currentDungeonId ?? string.Empty;
        payload.DungeonLabel = HasText(_currentDungeonName) ? _currentDungeonName : "None";
        payload.RouteId = HasText(_selectedRouteId) ? _selectedRouteId : _selectedRouteChoiceId;
        payload.RouteLabel = HasText(_selectedRouteLabel) ? _selectedRouteLabel : "None";
        payload.RoomId = room != null ? room.RoomId : _currentRoomStepId ?? string.Empty;
        payload.RoomLabel = room != null ? room.DisplayName : GetCurrentRoomText();
        payload.PartySummaryText = ActiveDungeonPartyText;
        payload.ObjectiveText = HasText(request != null ? request.ObjectiveText : string.Empty)
            ? request.ObjectiveText
            : HasText(launchPlan != null ? launchPlan.ObjectiveText : string.Empty)
                ? launchPlan.ObjectiveText
                : "None";
        payload.RiskContextText = HasText(request != null ? request.RiskContextText : string.Empty)
            ? request.RiskContextText
            : BuildSelectedRouteContextText();
        payload.IsEliteEncounter = encounter != null && encounter.IsEliteEncounter;
        payload.TurnIndex = Mathf.Max(_runTurnCount, _battleTurnIndex);
        payload.Request = request ?? new PrototypeBattleRequest();
        return payload;
    }

    private BattleReturnPayload BuildBattleReturnPayload()
    {
        BattleReturnPayload payload = new BattleReturnPayload();
        if (!HasRecordedBattleReturn(_currentBattleResultSnapshot))
        {
            return payload;
        }

        ExpeditionPlan launchPlan = GetCurrentExpeditionPlanForAppFlow();
        PrototypeBattleResultSnapshot snapshot = CopyBattleResultSnapshot(_currentBattleResultSnapshot);
        PrototypeBattleRequest request = HasMeaningfulBattleRequest(_activeBattleRequest)
            ? _activeBattleRequest
            : null;
        string encounterId = ResolveBattleReturnEncounterId(snapshot, request);
        string encounterName = ResolveBattleReturnEncounterName(snapshot, request);
        DungeonRoomTemplateData encounterRoom = GetRoomStepByEncounterId(encounterId);
        payload.OutcomeKey = HasText(snapshot.ResultStateKey) ? snapshot.ResultStateKey : PrototypeBattleOutcomeKeys.None;
        payload.EncounterId = encounterId;
        payload.EncounterName = encounterName;
        payload.EncounterRoomTypeText = ResolveBattleReturnEncounterRoomTypeText(encounterRoom, request);
        payload.ObjectiveText = ResolveBattleReturnObjectiveText(launchPlan, request);
        payload.WhyNowText = HasText(launchPlan != null ? launchPlan.WhyNowText : string.Empty)
            ? launchPlan.WhyNowText
            : "None";
        payload.ExpectedUsefulnessText = HasText(launchPlan != null ? launchPlan.ExpectedUsefulnessText : string.Empty)
            ? launchPlan.ExpectedUsefulnessText
            : "None";
        payload.RoomProgressText = BuildEncounterRoomProgressText(encounterRoom);
        payload.NextGoalText = GetNextMajorGoalText();
        payload.ResultSummaryText = BuildBattleReturnSummaryText(snapshot, encounterName);
        payload.PartyConditionText = HasText(_resultPartyConditionText)
            ? _resultPartyConditionText
            : GetPartyConditionText();
        payload.PartyHpSummaryText = HasText(_resultPartyHpSummaryText)
            ? _resultPartyHpSummaryText
            : BuildTotalPartyHpSummary();
        payload.LootSummaryText = HasText(snapshot.FinalLootSummary) ? snapshot.FinalLootSummary : BuildLootBreakdownSummary();
        payload.EncounterCleared = snapshot.ResultStateKey == PrototypeBattleOutcomeKeys.EncounterVictory ||
                                   snapshot.ResultStateKey == PrototypeBattleOutcomeKeys.RunClear;
        payload.EliteDefeated = snapshot.EliteDefeated || _eliteDefeated;
        payload.TurnsTaken = Mathf.Max(0, snapshot.TurnsTaken);
        payload.SurvivingMemberCount = Mathf.Max(0, snapshot.SurvivingMemberCount);
        payload.KnockedOutMemberCount = Mathf.Max(0, snapshot.KnockedOutMemberCount);
        payload.ResultSnapshot = snapshot;
        payload.Resolution = GetLatestBattleResolution();
        return payload;
    }

    private string ResolveBattleReturnEncounterId(PrototypeBattleResultSnapshot snapshot, PrototypeBattleRequest request)
    {
        if (HasText(request != null ? request.EncounterId : string.Empty))
        {
            return request.EncounterId;
        }

        if (HasText(snapshot != null ? snapshot.EncounterId : string.Empty))
        {
            return snapshot.EncounterId;
        }

        return string.Empty;
    }

    private string ResolveBattleReturnEncounterName(PrototypeBattleResultSnapshot snapshot, PrototypeBattleRequest request)
    {
        if (HasText(request != null ? request.EncounterName : string.Empty))
        {
            return request.EncounterName;
        }

        if (HasText(snapshot != null ? snapshot.EncounterName : string.Empty))
        {
            return snapshot.EncounterName;
        }

        return "None";
    }

    private string ResolveBattleReturnEncounterRoomTypeText(DungeonRoomTemplateData encounterRoom, PrototypeBattleRequest request)
    {
        if (HasText(request != null ? request.RoomTypeLabel : string.Empty))
        {
            return request.RoomTypeLabel;
        }

        if (encounterRoom != null && HasText(encounterRoom.RoomTypeLabel))
        {
            return encounterRoom.RoomTypeLabel;
        }

        return GetEncounterRoomTypeText();
    }

    private string ResolveBattleReturnObjectiveText(ExpeditionPlan launchPlan, PrototypeBattleRequest request)
    {
        if (HasText(request != null ? request.ObjectiveText : string.Empty))
        {
            return request.ObjectiveText;
        }

        if (HasText(launchPlan != null ? launchPlan.ObjectiveText : string.Empty))
        {
            return launchPlan.ObjectiveText;
        }

        return "None";
    }

    private string BuildEncounterRoomProgressText(DungeonRoomTemplateData room)
    {
        if (room == null)
        {
            return GetRoomProgressText();
        }

        int totalRooms = _plannedRooms.Count;
        if (totalRooms <= 0)
        {
            return "0 / 0";
        }

        for (int i = 0; i < _plannedRooms.Count; i++)
        {
            DungeonRoomTemplateData plannedRoom = _plannedRooms[i];
            if (plannedRoom != null && plannedRoom.RoomId == room.RoomId)
            {
                return (i + 1) + " / " + totalRooms;
            }
        }

        return GetRoomProgressText();
    }

    private string BuildBattleReturnSummaryText(PrototypeBattleResultSnapshot snapshot, string encounterNameOverride)
    {
        if (snapshot == null)
        {
            return "None";
        }

        string encounterName = HasText(encounterNameOverride)
            ? encounterNameOverride
            : HasText(snapshot.EncounterName)
                ? snapshot.EncounterName
                : "None";

        if (snapshot.ResultStateKey == PrototypeBattleOutcomeKeys.EncounterVictory)
        {
            return encounterName + " cleared. Continue the expedition.";
        }

        if (snapshot.ResultStateKey == PrototypeBattleOutcomeKeys.RunClear && HasText(_latestRpgRunResultSnapshot != null ? _latestRpgRunResultSnapshot.ResultSummary : string.Empty))
        {
            return _latestRpgRunResultSnapshot.ResultSummary;
        }

        if (snapshot.ResultStateKey == PrototypeBattleOutcomeKeys.RunDefeat && HasText(_latestRpgRunResultSnapshot != null ? _latestRpgRunResultSnapshot.ResultSummary : string.Empty))
        {
            return _latestRpgRunResultSnapshot.ResultSummary;
        }

        if (snapshot.ResultStateKey == PrototypeBattleOutcomeKeys.RunRetreat && HasText(_latestRpgRunResultSnapshot != null ? _latestRpgRunResultSnapshot.ResultSummary : string.Empty))
        {
            return _latestRpgRunResultSnapshot.ResultSummary;
        }

        if (HasText(encounterName))
        {
            return encounterName + " resolved.";
        }

        return "None";
    }

    private RunLogEntry[] BuildRecentRunLogEntries()
    {
        if (_recentBattleLogs.Count == 0)
        {
            return Array.Empty<RunLogEntry>();
        }

        RunLogEntry[] entries = new RunLogEntry[_recentBattleLogs.Count];
        for (int i = 0; i < _recentBattleLogs.Count; i++)
        {
            entries[i] = new RunLogEntry
            {
                Sequence = i + 1,
                EntryType = "run_log",
                PhaseKey = ResolveRunPhase().ToString(),
                RelatedRoomId = _currentRoomStepId ?? string.Empty,
                RelatedEncounterId = _activeEncounterId ?? string.Empty,
                SummaryText = _recentBattleLogs[i],
                DetailText = _recentBattleLogs[i]
            };
        }

        return entries;
    }

    private bool HasRecordedBattleReturn(PrototypeBattleResultSnapshot snapshot)
    {
        return snapshot != null &&
               ((HasText(snapshot.ResultStateKey) && snapshot.ResultStateKey != PrototypeBattleOutcomeKeys.None) ||
                (HasText(snapshot.OutcomeKey) && snapshot.OutcomeKey != PrototypeBattleOutcomeKeys.None));
    }

    private static bool HasMeaningfulRunState(ExpeditionRunState state)
    {
        return state != null &&
               (HasText(state.TargetDungeonId) ||
                HasText(state.OriginCityId) ||
                state.Status != DungeonRunStatus.Inactive ||
                state.Phase != RunPhase.None ||
                state.IsResolved);
    }

    private static bool HasMeaningfulRunReadModel(DungeonRunReadModel model)
    {
        return model != null &&
               (HasText(model.ObjectiveText) ||
                HasText(model.CurrentLocationText) ||
                HasText(model.StatusText) ||
                HasText(model.RecentOutcomeText));
    }
}
