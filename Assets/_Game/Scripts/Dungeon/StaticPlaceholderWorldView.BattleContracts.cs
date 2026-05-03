using System;
using System.Collections.Generic;
using UnityEngine;

public sealed partial class StaticPlaceholderWorldView
{
    private PrototypeBattleRequest _activeBattleRequest = new PrototypeBattleRequest();
    private PrototypeBattleRuntimeState _activeBattleRuntimeState = new PrototypeBattleRuntimeState();
    private PrototypeBattleResolution _latestBattleResolution = new PrototypeBattleResolution();
    private PrototypeBattleViewModel _activeBattleViewModel = new PrototypeBattleViewModel();
    private PrototypeBattleCommand _pendingBattleCommand = new PrototypeBattleCommand();
    private int _battleContractStateStamp = int.MinValue;

    public PrototypeBattleRequest GetBattleRequest()
    {
        EnsureBattleContracts();
        return PrototypeBattleStateFactory.CreateRequest(_activeBattleRequest);
    }

    public PrototypeBattleRuntimeState GetBattleRuntimeState()
    {
        EnsureBattleContracts();
        return PrototypeBattleStateFactory.CreateState(_activeBattleRuntimeState);
    }

    public PrototypeBattleResolution GetLatestBattleResolution()
    {
        EnsureBattleContracts();
        return PrototypeBattleStateFactory.CreateResolution(_latestBattleResolution);
    }

    public PrototypeBattleViewModel GetBattleViewModel()
    {
        EnsureBattleContracts();
        return CloneBattleViewModel(_activeBattleViewModel);
    }

    private void EnsureBattleContracts()
    {
        if (!HasMeaningfulBattleRequest(_activeBattleRequest) && _dungeonRunState == DungeonRunState.Battle)
        {
            CaptureBattleRequestContract();
        }

        if (!HasMeaningfulBattleResolution(_latestBattleResolution) && HasRecordedBattleReturn(_currentBattleResultSnapshot))
        {
            CaptureBattleResolutionContract(
                !string.IsNullOrEmpty(_currentBattleResultSnapshot.ResultStateKey)
                    ? _currentBattleResultSnapshot.ResultStateKey
                    : _currentBattleResultSnapshot.OutcomeKey,
                _carriedLootAmount,
                string.Empty);
            return;
        }

        int stateStamp = ComputeBattleContractStateStamp();
        if (_battleContractStateStamp == stateStamp &&
            _activeBattleViewModel != null &&
            HasMeaningfulBattleRequest(_activeBattleRequest))
        {
            return;
        }

        RefreshBattleContractView();
    }

    private void CaptureBattleRequestContract()
    {
        _activeBattleRequest = PrototypeBattleStateFactory.CreateRequest(BuildBattleRequestSnapshot());
        _latestBattleResolution = new PrototypeBattleResolution();
        _pendingBattleCommand = new PrototypeBattleCommand();
        RefreshBattleContractView();
    }

    private void CaptureBattleResolutionContract(string outcomeKey, int returnedLootAmount, string summaryOverride)
    {
        if (!HasMeaningfulBattleRequest(_activeBattleRequest))
        {
            _activeBattleRequest = PrototypeBattleStateFactory.CreateRequest(BuildBattleRequestSnapshot());
        }

        PrototypeBattleRuntimeState state = BuildBattleRuntimeStateSnapshot(_activeBattleRequest);
        _latestBattleResolution = PrototypeBattleStateFactory.CreateResolution(
            BuildBattleResolutionSnapshot(_activeBattleRequest, state, outcomeKey, returnedLootAmount, summaryOverride));
        _currentBattleResultSnapshot = CopyBattleResultSnapshot(_latestBattleResolution.ResultSnapshot);
        RefreshBattleContractView();
    }

    private void ResetBattleContractState()
    {
        _activeBattleRequest = new PrototypeBattleRequest();
        _activeBattleRuntimeState = new PrototypeBattleRuntimeState();
        _latestBattleResolution = new PrototypeBattleResolution();
        _activeBattleViewModel = new PrototypeBattleViewModel();
        _pendingBattleCommand = new PrototypeBattleCommand();
        _battleContractStateStamp = int.MinValue;
    }

    private void RefreshBattleContractView()
    {
        _activeBattleRuntimeState = PrototypeBattleStateFactory.CreateState(BuildBattleRuntimeStateSnapshot(_activeBattleRequest));
        PrototypeBattleUiSurfaceData surface = BuildBattleUiSurfaceDataFromContracts(_activeBattleRequest, _activeBattleRuntimeState, _latestBattleResolution);
        _activeBattleViewModel = PrototypeBattleCoordinator.BuildViewModel(_activeBattleRequest, _activeBattleRuntimeState, _latestBattleResolution, surface);
        _battleContractStateStamp = ComputeBattleContractStateStamp();
    }

    private PrototypeBattleViewModel CloneBattleViewModel(PrototypeBattleViewModel source)
    {
        PrototypeBattleViewModel copy = new PrototypeBattleViewModel();
        if (source == null)
        {
            return copy;
        }

        copy.EncounterTitle = string.IsNullOrEmpty(source.EncounterTitle) ? "None" : source.EncounterTitle;
        copy.PhaseText = string.IsNullOrEmpty(source.PhaseText) ? "None" : source.PhaseText;
        copy.TurnText = string.IsNullOrEmpty(source.TurnText) ? "Turn 0" : source.TurnText;
        copy.CurrentActorText = string.IsNullOrEmpty(source.CurrentActorText) ? "None" : source.CurrentActorText;
        copy.ResultText = string.IsNullOrEmpty(source.ResultText) ? "None" : source.ResultText;
        copy.RecentLogLines = source.RecentLogLines ?? Array.Empty<string>();
        copy.Request = PrototypeBattleStateFactory.CreateRequest(source.Request);
        copy.State = PrototypeBattleStateFactory.CreateState(source.State);
        copy.Resolution = PrototypeBattleStateFactory.CreateResolution(source.Resolution);
        copy.HudSurface = source.HudSurface ?? new PrototypeBattleUiSurfaceData();
        return copy;
    }

    private int ComputeBattleContractStateStamp()
    {
        unchecked
        {
            int hash = 17;
            hash = CombineBattleStamp(hash, _dungeonRunState.GetHashCode());
            hash = CombineBattleStamp(hash, _battleState.GetHashCode());
            hash = CombineBattleStamp(hash, _runTurnCount);
            hash = CombineBattleStamp(hash, _battleTurnIndex);
            hash = CombineBattleStamp(hash, _currentActorIndex);
            hash = CombineBattleStamp(hash, _pendingEnemyTargetIndex);
            hash = CombineBattleStamp(hash, _queuedBattleAction.GetHashCode());
            hash = CombineBattleStamp(hash, _runResultState.GetHashCode());
            hash = CombineBattleStamp(hash, _currentDungeonId);
            hash = CombineBattleStamp(hash, _currentDungeonName);
            hash = CombineBattleStamp(hash, _selectedRouteId);
            hash = CombineBattleStamp(hash, _selectedRouteLabel);
            hash = CombineBattleStamp(hash, _currentRoomStepId);
            hash = CombineBattleStamp(hash, _activeEncounterId);
            hash = CombineBattleStamp(hash, _activeBattleMonsterId);
            hash = CombineBattleStamp(hash, _hoverBattleMonsterId);
            hash = CombineBattleStamp(hash, _battleFeedbackText);
            hash = CombineBattleStamp(hash, _enemyIntentText);
            hash = CombineBattleStamp(hash, _currentSelectionPrompt);
            hash = CombineBattleStamp(hash, _partyActedThisRound.Length > 0 && _currentActorIndex >= 0 && _currentActorIndex < _partyActedThisRound.Length && _partyActedThisRound[_currentActorIndex]);
            hash = CombineBattleStamp(hash, _pendingEnemyUsedSpecialAttack);
            hash = CombineBattleStamp(hash, _eliteEncounterActive);
            hash = CombineBattleStamp(hash, _eliteDefeated);
            hash = CombineBattleStamp(hash, _exitUnlocked);
            hash = CombineBattleStamp(hash, _eventResolved);
            hash = CombineBattleStamp(hash, _preEliteDecisionResolved);
            hash = CombineBattleStamp(hash, _carriedLootAmount);
            hash = CombineBattleStamp(hash, _battleEventRecords.Count);

            DungeonPartyMemberRuntimeData[] partyMembers = _activeDungeonParty != null ? _activeDungeonParty.Members : null;
            int partyCount = partyMembers != null ? partyMembers.Length : 0;
            hash = CombineBattleStamp(hash, partyCount);
            for (int i = 0; i < partyCount; i++)
            {
                DungeonPartyMemberRuntimeData member = partyMembers[i];
                hash = CombineBattleStamp(hash, member != null);
                if (member == null)
                {
                    continue;
                }

                hash = CombineBattleStamp(hash, member.MemberId);
                hash = CombineBattleStamp(hash, member.RoleTag);
                hash = CombineBattleStamp(hash, member.SkillName);
                hash = CombineBattleStamp(hash, GetPartyMemberLaneKey(member));
                hash = CombineBattleStamp(hash, member.CurrentHp);
                hash = CombineBattleStamp(hash, member.MaxHp);
                hash = CombineBattleStamp(hash, member.IsDefeated);
                hash = CombineBattleStamp(hash, i >= 0 && i < _partyActedThisRound.Length && _partyActedThisRound[i]);
            }

            hash = CombineBattleStamp(hash, _activeMonsters.Count);
            for (int i = 0; i < _activeMonsters.Count; i++)
            {
                DungeonMonsterRuntimeData monster = _activeMonsters[i];
                hash = CombineBattleStamp(hash, monster != null);
                if (monster == null)
                {
                    continue;
                }

                hash = CombineBattleStamp(hash, monster.MonsterId);
                hash = CombineBattleStamp(hash, monster.DisplayName);
                hash = CombineBattleStamp(hash, monster.MonsterType);
                hash = CombineBattleStamp(hash, monster.EncounterRole.GetHashCode());
                hash = CombineBattleStamp(hash, GetMonsterLaneKey(monster));
                hash = CombineBattleStamp(hash, monster.CurrentHp);
                hash = CombineBattleStamp(hash, monster.MaxHp);
                hash = CombineBattleStamp(hash, monster.IsDefeated);
                hash = CombineBattleStamp(hash, monster.IsElite);
            }

            return hash;
        }
    }

    private static int CombineBattleStamp(int hash, int value)
    {
        unchecked
        {
            return (hash * 31) + value;
        }
    }

    private static int CombineBattleStamp(int hash, bool value)
    {
        unchecked
        {
            return (hash * 31) + (value ? 1 : 0);
        }
    }

    private static int CombineBattleStamp(int hash, string value)
    {
        unchecked
        {
            return (hash * 31) + (string.IsNullOrEmpty(value) ? 0 : value.GetHashCode());
        }
    }

    private PrototypeBattleRequest BuildBattleRequestSnapshot()
    {
        PrototypeBattleRequest request = new PrototypeBattleRequest();
        DungeonEncounterRuntimeData encounter = GetActiveEncounter();
        DungeonRoomTemplateData room = encounter != null ? GetRoomStepByEncounterId(encounter.EncounterId) : GetCurrentPlannedRoomStep();

        request.EncounterId = encounter != null ? encounter.EncounterId : string.Empty;
        request.EncounterName = encounter != null && HasText(encounter.DisplayName)
            ? encounter.DisplayName
            : HasText(GetCurrentEncounterNameText())
                ? GetCurrentEncounterNameText()
                : "None";
        request.DungeonId = _currentDungeonId ?? string.Empty;
        request.DungeonLabel = HasText(_currentDungeonName) ? _currentDungeonName : "None";
        request.RouteId = HasText(_selectedRouteId) ? _selectedRouteId : _selectedRouteChoiceId;
        request.RouteLabel = HasText(_selectedRouteLabel) ? _selectedRouteLabel : "None";
        request.RoomId = room != null ? room.RoomId : _currentRoomStepId ?? string.Empty;
        request.RoomLabel = room != null ? room.DisplayName : GetCurrentRoomText();
        request.RoomTypeLabel = room != null ? room.RoomTypeLabel : GetCurrentRoomTypeText();
        request.PartySummaryText = ActiveDungeonPartyText;
        request.ObjectiveText = GetBattleObjectiveText();
        request.RiskContextText = BuildSelectedRouteContextText();
        request.RewardPreviewText = encounter != null
            ? BuildLootAmountText(CalculateActiveEncounterLoot())
            : BuildLootBreakdownSummary();
        request.IsEliteEncounter = encounter != null && encounter.IsEliteEncounter;
        request.RetreatAllowed = true;
        request.EncounterProfileSourceText = "fallback:hardcoded";
        request.BattleSetupSourceText = "fallback:hardcoded";
        request.EliteTypeText = request.IsEliteEncounter ? DefaultText(_eliteType, "Elite") : "None";
        request.EliteRewardHintText = request.IsEliteEncounter ? DefaultText(_eliteRewardLabel, DefaultText(request.RewardPreviewText, "None")) : "None";

        if (TryResolveEncounterBattleAuthoring(
                _currentHomeCityId,
                request.DungeonId,
                request.RouteId,
                request.EncounterId,
                out GoldenPathRoomDefinition roomDefinition,
                out GoldenPathEncounterProfileDefinition encounterProfile,
                out GoldenPathBattleSetupDefinition battleSetup))
        {
            request.EncounterProfileId = roomDefinition != null ? roomDefinition.EncounterProfileId ?? string.Empty : string.Empty;
            request.EncounterProfileSourceText = BuildEncounterProfileSourceLabel(request.EncounterProfileId);
            request.EncounterContextText = HasText(encounterProfile != null ? encounterProfile.BattleContextText : string.Empty)
                ? encounterProfile.BattleContextText
                : HasText(encounterProfile != null ? encounterProfile.MissionRelevanceText : string.Empty)
                    ? encounterProfile.MissionRelevanceText
                    : "None";
            request.BattleSetupId = roomDefinition != null ? roomDefinition.BattleSetupId ?? string.Empty : string.Empty;
            request.BattleSetupSourceText = BuildBattleSetupSourceLabel(request.BattleSetupId);
            request.EnemyGroupText = HasText(battleSetup != null ? battleSetup.EnemyGroupLabel : string.Empty)
                ? battleSetup.EnemyGroupLabel
                : "None";
            request.BattleSetupSummaryText = HasText(battleSetup != null ? battleSetup.SetupSummaryText : string.Empty)
                ? battleSetup.SetupSummaryText
                : HasText(encounterProfile != null ? encounterProfile.MissionRelevanceText : string.Empty)
                    ? encounterProfile.MissionRelevanceText
                    : "None";

            if (battleSetup != null)
            {
                request.RetreatAllowed = battleSetup.RetreatAllowed;
                if (request.IsEliteEncounter)
                {
                    request.EliteTypeText = HasText(battleSetup.EliteTypeText) ? battleSetup.EliteTypeText : request.EliteTypeText;
                    request.EliteRewardHintText = HasText(battleSetup.RewardHintText) ? battleSetup.RewardHintText : request.EliteRewardHintText;
                }
            }
        }

        request.EncounterContextText = AppendRoomInteractionConsequenceText(
            request.EncounterContextText,
            BuildRoomInteractionBattleContextText(request.EncounterId));
        request.EncounterContextText = AppendRoomInteractionConsequenceText(
            request.EncounterContextText,
            BuildEncounterVarietyRoutePressureContextText(request.EncounterId));

        request.EnterTurnIndex = Mathf.Max(_runTurnCount, _battleTurnIndex);
        request.PartyMembers = BuildPartyCombatantSnapshots(includeLiveHighlights: false);
        request.EnemyMembers = BuildEnemyCombatantSnapshots(includeLiveHighlights: false);
        return request;
    }

    private PrototypeBattleRuntimeState BuildBattleRuntimeStateSnapshot(PrototypeBattleRequest request)
    {
        PrototypeBattleRuntimeState state = new PrototypeBattleRuntimeState();
        state.IsBattleActive = _dungeonRunState == DungeonRunState.Battle;
        state.BattleStateKey = state.IsBattleActive ? GetBattleStateKey() : PrototypeBattleOutcomeKeys.None;
        state.Phase = ResolveBattlePhaseContract();
        state.PhaseLabel = DescribeBattlePhase(state.Phase);
        state.BattleTurnIndex = Mathf.Max(_runTurnCount, _battleTurnIndex);
        state.CurrentActorIndex = _currentActorIndex;
        state.SelectedPartyMemberIndex = _selectedPartyMemberIndex;
        state.IsTargetSelectionActive = _battleState == BattleState.PartyTargetSelect;
        state.IsInputLocked = IsBattleInputLocked();
        state.CanAttack = IsBattleActionAvailable(BattleActionType.Attack);
        state.CanSkill = IsBattleActionAvailable(BattleActionType.Skill);
        state.CanRetreat = IsBattleActionAvailable(BattleActionType.Retreat);
        state.PromptText = HasText(CurrentSelectionPromptText) ? CurrentSelectionPromptText : "Select an action.";
        state.FeedbackText = HasText(_battleFeedbackText) ? _battleFeedbackText : string.Empty;
        state.PartySummaryText = HasText(request != null ? request.PartySummaryText : string.Empty) ? request.PartySummaryText : ActiveDungeonPartyText;
        state.PartyHpSummaryText = BuildTotalPartyHpSummary();
        state.PartyConditionText = GetPartyConditionText();
        state.PartyMembers = BuildPartyCombatantSnapshots(includeLiveHighlights: true);
        state.EnemyMembers = BuildEnemyCombatantSnapshots(includeLiveHighlights: true);
        state.CurrentActor = BuildCurrentActorCombatantSnapshot(state.PartyMembers, state.EnemyMembers);
        state.CurrentActorId = state.CurrentActor != null ? state.CurrentActor.CombatantId : string.Empty;
        state.FocusedEnemyId = GetFocusedEnemyId();
        state.HoveredEnemyId = _hoverBattleMonsterId ?? string.Empty;
        state.SelectedCommand = BuildSelectedCommandSnapshot();
        state.PendingEnemyIntent = BuildCurrentEnemyIntentSnapshotView();
        state.RecentEvents = BuildRecentBattleEventRecords();
        state.RecentLogLines = BuildBattleLogLines();
        state.CurrentResultSnapshot = BuildCurrentBattleResultSnapshotView();
        return state;
    }

    private PrototypeBattleResolution BuildBattleResolutionSnapshot(
        PrototypeBattleRequest request,
        PrototypeBattleRuntimeState state,
        string outcomeKey,
        int returnedLootAmount,
        string summaryOverride)
    {
        PrototypeBattleResolution resolution = new PrototypeBattleResolution();
        string resolvedOutcomeKey = HasText(outcomeKey)
            ? outcomeKey
            : HasText(_currentBattleResultSnapshot != null ? _currentBattleResultSnapshot.ResultStateKey : string.Empty)
                ? _currentBattleResultSnapshot.ResultStateKey
                : HasText(_currentBattleResultSnapshot != null ? _currentBattleResultSnapshot.OutcomeKey : string.Empty)
                    ? _currentBattleResultSnapshot.OutcomeKey
                    : PrototypeBattleOutcomeKeys.None;
        PrototypeBattleResultSnapshot resultSnapshot = HasText(resolvedOutcomeKey) && resolvedOutcomeKey != PrototypeBattleOutcomeKeys.None
            ? BuildBattleResultSnapshot(resolvedOutcomeKey)
            : BuildCurrentBattleResultSnapshotView();
        int safeReturnedLoot = returnedLootAmount >= 0
            ? Mathf.Max(0, returnedLootAmount)
            : _runResultState == RunResultState.Clear
                ? Mathf.Max(0, _carriedLootAmount)
                : 0;

        resolution.ResultType = ResolveBattleResultType(resolvedOutcomeKey);
        resolution.OutcomeKey = resolvedOutcomeKey;
        resolution.EncounterId = HasText(request != null ? request.EncounterId : string.Empty)
            ? request.EncounterId
            : resultSnapshot.EncounterId;
        resolution.EncounterName = HasText(request != null ? request.EncounterName : string.Empty)
            ? request.EncounterName
            : DefaultText(resultSnapshot.EncounterName, "None");
        resolution.DungeonId = request != null ? request.DungeonId : _currentDungeonId ?? string.Empty;
        resolution.DungeonLabel = HasText(request != null ? request.DungeonLabel : string.Empty)
            ? request.DungeonLabel
            : HasText(_currentDungeonName)
                ? _currentDungeonName
                : "None";
        resolution.RouteLabel = HasText(request != null ? request.RouteLabel : string.Empty)
            ? request.RouteLabel
            : HasText(_selectedRouteLabel)
                ? _selectedRouteLabel
                : "None";
        resolution.SummaryText = HasText(summaryOverride)
            ? summaryOverride
            : BuildBattleResolutionSummary(resolvedOutcomeKey, resolution.EncounterName);
        resolution.PartyConditionText = HasText(_resultPartyConditionText)
            ? _resultPartyConditionText
            : HasText(state != null ? state.PartyConditionText : string.Empty)
                ? state.PartyConditionText
                : GetPartyConditionText();
        resolution.PartyHpSummaryText = HasText(_resultPartyHpSummaryText)
            ? _resultPartyHpSummaryText
            : HasText(state != null ? state.PartyHpSummaryText : string.Empty)
                ? state.PartyHpSummaryText
                : BuildTotalPartyHpSummary();
        resolution.LootSummaryText = HasText(resultSnapshot.FinalLootSummary)
            ? resultSnapshot.FinalLootSummary
            : safeReturnedLoot > 0
                ? BuildLootAmountText(safeReturnedLoot)
                : "None";
        resolution.EncounterCleared = resolvedOutcomeKey == PrototypeBattleOutcomeKeys.EncounterVictory ||
                                      resolvedOutcomeKey == PrototypeBattleOutcomeKeys.RunClear;
        resolution.RunEnded = resolvedOutcomeKey == PrototypeBattleOutcomeKeys.RunClear ||
                              resolvedOutcomeKey == PrototypeBattleOutcomeKeys.RunDefeat ||
                              resolvedOutcomeKey == PrototypeBattleOutcomeKeys.RunRetreat;
        resolution.EliteDefeated = _eliteDefeated || resultSnapshot.EliteDefeated;
        resolution.TurnsTaken = Mathf.Max(0, resultSnapshot.TurnsTaken);
        resolution.SurvivingMemberCount = Mathf.Max(0, resultSnapshot.SurvivingMemberCount);
        resolution.KnockedOutMemberCount = Mathf.Max(0, resultSnapshot.KnockedOutMemberCount);
        resolution.DefeatedEnemyCount = Mathf.Max(0, resultSnapshot.DefeatedEnemyCount);
        resolution.ReturnedLootAmount = safeReturnedLoot;
        resolution.TotalDamageDealt = Mathf.Max(0, resultSnapshot.TotalDamageDealt);
        resolution.TotalDamageTaken = Mathf.Max(0, resultSnapshot.TotalDamageTaken);
        resolution.TotalHealingDone = Mathf.Max(0, resultSnapshot.TotalHealingDone);
        resolution.ResultTags = BuildBattleResultTags(resolvedOutcomeKey, request);
        resolution.PartyMembersAtEnd = BuildPartyCombatantSnapshots(includeLiveHighlights: true);
        resolution.EnemyMembersAtEnd = BuildEnemyCombatantSnapshots(includeLiveHighlights: true);
        resolution.RecentEvents = BuildRecentBattleEventRecords();
        resolution.ResultSnapshot = resultSnapshot;
        return resolution;
    }

    private PrototypeBattleUiSurfaceData BuildBattleUiSurfaceDataFromContracts(
        PrototypeBattleRequest request,
        PrototypeBattleRuntimeState state,
        PrototypeBattleResolution resolution)
    {
        PrototypeBattleUiSurfaceData surface = new PrototypeBattleUiSurfaceData();
        surface.IsBattleActive = state != null && state.IsBattleActive;
        surface.IsTargetSelectionActive = state != null && state.IsTargetSelectionActive;
        surface.BattleStateKey = DefaultText(state != null ? state.BattleStateKey : string.Empty, PrototypeBattleOutcomeKeys.None);
        surface.CurrentDungeonName = DefaultText(request != null ? request.DungeonLabel : string.Empty, "None");
        surface.CurrentRouteLabel = DefaultText(request != null ? request.RouteLabel : string.Empty, "None");
        surface.EncounterName = DefaultText(request != null ? request.EncounterName : string.Empty, "None");
        surface.EncounterRoomType = DefaultText(request != null ? request.RoomTypeLabel : string.Empty, "None");
        surface.MissionObjectiveText = DefaultText(request != null ? request.ObjectiveText : string.Empty, "None");
        surface.MissionRewardPreviewText = DefaultText(request != null ? request.RewardPreviewText : string.Empty, "None");
        surface.MissionRiskContextText = DefaultText(request != null ? request.RiskContextText : string.Empty, "None");
        surface.MissionIntentSummaryText = BuildBattleIntentSummaryText(request);
        surface.PartyCondition = DefaultText(state != null ? state.PartyConditionText : string.Empty, "None");
        surface.TotalPartyHp = DefaultText(state != null ? state.PartyHpSummaryText : string.Empty, "None");
        surface.EliteStatusText = request != null && request.IsEliteEncounter
            ? (resolution != null && resolution.EliteDefeated ? "Defeated" : "Active")
            : "None";
        surface.EliteEncounterName = request != null && request.IsEliteEncounter
            ? DefaultText(request.EncounterName, "None")
            : "None";
        surface.EliteTypeText = request != null && request.IsEliteEncounter
            ? DefaultText(request.EliteTypeText, DefaultText(_eliteType, "Elite"))
            : "None";
        surface.EliteRewardHintText = request != null && request.IsEliteEncounter
            ? DefaultText(request.EliteRewardHintText, DefaultText(request.RewardPreviewText, "None"))
            : "None";
        surface.CurrentActor = BuildBattleUiActorFromCombatant(state != null ? state.CurrentActor : null, state != null ? state.Phase : PrototypeBattlePhase.None);
        surface.Timeline = BuildBattleUiTimelineFromContracts(state);
        surface.PartyMembers = BuildBattleUiPartyMembersFromContracts(state != null ? state.PartyMembers : Array.Empty<PrototypeBattleCombatantState>());
        surface.EnemyRoster = BuildBattleUiEnemyRosterFromContracts(state != null ? state.EnemyMembers : Array.Empty<PrototypeBattleCombatantState>());
        surface.SelectedEnemy = SelectBattleUiEnemy(surface.EnemyRoster, state != null ? state.FocusedEnemyId : string.Empty);
        surface.ActionContext = BuildBattleUiActionContextFromContracts(state, surface.CurrentActor, surface.SelectedEnemy);
        surface.TargetContext = BuildBattleUiTargetContextFromContracts(surface.SelectedEnemy);
        surface.CommandSurface = BuildBattleUiCommandSurfaceFromContracts(state, surface.CurrentActor, surface.ActionContext);
        surface.MessageSurface = BuildBattleUiMessageSurfaceFromContracts(state);
        surface.TargetSelection = BuildBattleUiTargetSelectionFromContracts(state, surface.CurrentActor, surface.ActionContext, surface.TargetContext, surface.SelectedEnemy);
        surface.EnemyIntent = state != null && state.PendingEnemyIntent != null
            ? state.PendingEnemyIntent
            : new PrototypeEnemyIntentSnapshot();
        surface.RecentEvents = state != null && state.RecentEvents != null
            ? state.RecentEvents
            : Array.Empty<PrototypeBattleEventRecord>();
        surface.ResultSnapshot = resolution != null && resolution.ResultSnapshot != null
            ? resolution.ResultSnapshot
            : state != null && state.CurrentResultSnapshot != null
                ? state.CurrentResultSnapshot
                : new PrototypeBattleResultSnapshot();
        return surface;
    }

    private string BuildBattleIntentSummaryText(PrototypeBattleRequest request)
    {
        List<string> parts = new List<string>();
        if (HasText(request != null ? request.ObjectiveText : string.Empty))
        {
            parts.Add(request.ObjectiveText);
        }

        if (HasText(request != null ? request.EncounterContextText : string.Empty))
        {
            parts.Add("Context: " + request.EncounterContextText);
        }

        if (HasText(request != null ? request.EnemyGroupText : string.Empty))
        {
            parts.Add("Enemy: " + request.EnemyGroupText);
        }

        if (parts.Count == 0 && HasText(request != null ? request.RewardPreviewText : string.Empty))
        {
            parts.Add("Reward: " + request.RewardPreviewText);
        }

        if (parts.Count <= 1 && HasText(request != null ? request.RiskContextText : string.Empty))
        {
            parts.Add("Risk: " + request.RiskContextText);
        }

        return parts.Count > 0 ? string.Join(" | ", parts.ToArray()) : "None";
    }

    private PrototypeBattleCombatantState[] BuildPartyCombatantSnapshots(bool includeLiveHighlights)
    {
        if (_activeDungeonParty == null || _activeDungeonParty.Members == null || _activeDungeonParty.Members.Length == 0)
        {
            return Array.Empty<PrototypeBattleCombatantState>();
        }

        PrototypeBattleCombatantState[] members = new PrototypeBattleCombatantState[_activeDungeonParty.Members.Length];
        for (int i = 0; i < _activeDungeonParty.Members.Length; i++)
        {
            members[i] = BuildPartyCombatantSnapshot(_activeDungeonParty.Members[i], i, includeLiveHighlights);
        }

        return members;
    }

    private PrototypeBattleCombatantState[] BuildEnemyCombatantSnapshots(bool includeLiveHighlights)
    {
        List<PrototypeBattleCombatantState> enemies = new List<PrototypeBattleCombatantState>();
        DungeonEncounterRuntimeData encounter = GetActiveEncounter();
        if (encounter != null && encounter.MonsterIds != null && encounter.MonsterIds.Length > 0)
        {
            for (int i = 0; i < encounter.MonsterIds.Length; i++)
            {
                DungeonMonsterRuntimeData monster = GetMonsterById(encounter.MonsterIds[i]);
                if (monster != null)
                {
                    enemies.Add(BuildEnemyCombatantSnapshot(monster, i, includeLiveHighlights));
                }
            }

            return enemies.ToArray();
        }

        for (int i = 0; i < _activeMonsters.Count; i++)
        {
            DungeonMonsterRuntimeData monster = _activeMonsters[i];
            if (monster != null)
            {
                enemies.Add(BuildEnemyCombatantSnapshot(monster, enemies.Count, includeLiveHighlights));
            }
        }

        return enemies.Count > 0 ? enemies.ToArray() : Array.Empty<PrototypeBattleCombatantState>();
    }

    private PrototypeBattleCombatantState BuildPartyCombatantSnapshot(DungeonPartyMemberRuntimeData member, int memberIndex, bool includeLiveHighlights)
    {
        PrototypeBattleCombatantState snapshot = new PrototypeBattleCombatantState();
        snapshot.TeamKey = "party";
        snapshot.SlotIndex = memberIndex;
        snapshot.LaneLabel = BuildPartyLaneLabel(memberIndex);
        if (member == null)
        {
            snapshot.DisplayName = "Empty";
            snapshot.StatusText = "Unavailable";
            snapshot.IsDefeated = true;
            return snapshot;
        }

        bool isTargeted = includeLiveHighlights &&
                          _dungeonRunState == DungeonRunState.Battle &&
                          _battleState == BattleState.EnemyTurn &&
                          _pendingEnemyTargetIndex == memberIndex &&
                          !member.IsDefeated &&
                          member.CurrentHp > 0;
        bool isActing = includeLiveHighlights &&
                        _dungeonRunState == DungeonRunState.Battle &&
                        _currentActorIndex == memberIndex &&
                        !member.IsDefeated &&
                        member.CurrentHp > 0;

        snapshot.CombatantId = member.MemberId ?? string.Empty;
        snapshot.DisplayName = HasText(member.DisplayName) ? member.DisplayName : "Adventurer";
        snapshot.RoleLabel = HasText(member.RoleLabel) ? member.RoleLabel : "Adventurer";
        snapshot.SkillId = member.DefaultSkillId ?? string.Empty;
        snapshot.SkillLabel = HasText(member.SkillName) ? member.SkillName : "Skill";
        snapshot.CurrentHp = Mathf.Max(0, member.CurrentHp);
        snapshot.MaxHp = Mathf.Max(1, member.MaxHp);
        snapshot.Attack = member.Attack;
        snapshot.Defense = member.Defense;
        snapshot.Speed = member.Speed;
        snapshot.IsActing = isActing;
        snapshot.IsTargeted = isTargeted;
        snapshot.IsDefeated = member.IsDefeated || member.CurrentHp <= 0;
        snapshot.StatusText = snapshot.IsDefeated
            ? "KO"
            : isActing
                ? "Acting"
                : isTargeted
                    ? "Targeted"
                    : "Ready";
        return snapshot;
    }

    private PrototypeBattleCombatantState BuildEnemyCombatantSnapshot(DungeonMonsterRuntimeData monster, int displayIndex, bool includeLiveHighlights)
    {
        PrototypeBattleCombatantState snapshot = new PrototypeBattleCombatantState();
        snapshot.TeamKey = "enemy";
        snapshot.SlotIndex = displayIndex;
        snapshot.LaneLabel = "Enemy " + (displayIndex + 1);
        if (monster == null)
        {
            snapshot.DisplayName = "Monster";
            snapshot.StatusText = "Unknown";
            return snapshot;
        }

        bool isSelected = includeLiveHighlights &&
                          _battleState == BattleState.PartyTargetSelect &&
                          _activeBattleMonsterId == monster.MonsterId &&
                          !monster.IsDefeated;
        bool isHovered = includeLiveHighlights &&
                         _battleState == BattleState.PartyTargetSelect &&
                         _hoverBattleMonsterId == monster.MonsterId &&
                         !monster.IsDefeated;
        bool isActing = includeLiveHighlights &&
                        _battleState == BattleState.EnemyTurn &&
                        _activeBattleMonsterId == monster.MonsterId &&
                        !monster.IsDefeated;

        snapshot.CombatantId = monster.MonsterId ?? string.Empty;
        snapshot.DisplayName = HasText(monster.DisplayName) ? monster.DisplayName : "Monster";
        snapshot.RoleLabel = GetMonsterRoleText(monster);
        snapshot.IntentLabel = BuildBattleUiEnemyIntentLabel(monster);
        snapshot.CurrentHp = Mathf.Max(0, monster.CurrentHp);
        snapshot.MaxHp = Mathf.Max(1, monster.MaxHp);
        snapshot.Attack = monster.Attack;
        snapshot.IsElite = monster.IsElite;
        snapshot.IsActing = isActing;
        snapshot.IsSelected = isSelected;
        snapshot.IsHovered = isHovered;
        snapshot.IsDefeated = monster.IsDefeated || monster.CurrentHp <= 0;
        snapshot.StatusText = snapshot.IsDefeated
            ? "Defeated"
            : isActing
                ? "Acting"
                : isSelected
                    ? "Targeted"
                    : isHovered
                        ? "Hover"
                        : "Ready";
        return snapshot;
    }

    private PrototypeBattleCombatantState BuildCurrentActorCombatantSnapshot(
        PrototypeBattleCombatantState[] partyMembers,
        PrototypeBattleCombatantState[] enemyMembers)
    {
        if (_dungeonRunState == DungeonRunState.Battle &&
            _battleState == BattleState.EnemyTurn &&
            !string.IsNullOrEmpty(_activeBattleMonsterId))
        {
            for (int i = 0; i < enemyMembers.Length; i++)
            {
                PrototypeBattleCombatantState enemy = enemyMembers[i];
                if (enemy != null && enemy.CombatantId == _activeBattleMonsterId)
                {
                    return enemy;
                }
            }
        }

        if (_currentActorIndex >= 0 && _currentActorIndex < partyMembers.Length)
        {
            PrototypeBattleCombatantState actor = partyMembers[_currentActorIndex];
            if (actor != null)
            {
                return actor;
            }
        }

        return new PrototypeBattleCombatantState();
    }

    private PrototypeBattleCommand BuildSelectedCommandSnapshot()
    {
        if (_pendingBattleCommand != null && _pendingBattleCommand.CommandType != PrototypeBattleCommandType.None)
        {
            return _pendingBattleCommand;
        }

        DungeonPartyMemberRuntimeData member = GetCurrentActorMember();
        if (member == null || _queuedBattleAction == BattleActionType.None)
        {
            return new PrototypeBattleCommand();
        }

        return CreateBattleCommandSnapshot(_queuedBattleAction, member);
    }

    private PrototypeBattleCommand CreateBattleCommandSnapshot(BattleActionType action, DungeonPartyMemberRuntimeData member)
    {
        PrototypeRpgSkillDefinition skillDefinition = action == BattleActionType.Skill ? ResolveMemberSkillDefinition(member) : null;
        PrototypeBattleCommandType commandType = action == BattleActionType.Attack
            ? PrototypeBattleCommandType.Attack
            : action == BattleActionType.Skill
                ? PrototypeBattleCommandType.Skill
                : action == BattleActionType.Retreat
                    ? PrototypeBattleCommandType.Retreat
                    : PrototypeBattleCommandType.None;
        string skillId = action == BattleActionType.Skill && skillDefinition != null
            ? skillDefinition.SkillId
            : string.Empty;
        string skillLabel = action == BattleActionType.Skill
            ? GetResolvedSkillDisplayName(member, skillDefinition)
            : action == BattleActionType.Attack
                ? "Attack"
                : "Retreat";
        string targetKind = action == BattleActionType.Attack
            ? "single_enemy"
            : action == BattleActionType.Skill
                ? GetResolvedSkillTargetKind(member, skillDefinition)
                : "party";
        string effectType = action == BattleActionType.Attack
            ? "damage"
            : action == BattleActionType.Skill
                ? GetResolvedSkillEffectType(member, skillDefinition)
                : "retreat";
        int powerValue = action == BattleActionType.Attack
            ? Mathf.Max(1, member.Attack)
            : action == BattleActionType.Skill
                ? GetResolvedSkillPower(member, skillDefinition)
                : 0;
        bool requiresTarget = action == BattleActionType.Attack || (action == BattleActionType.Skill && DoesSkillRequireTarget(member));
        string summaryText = member.DisplayName + " -> " + GetBattleActionDisplayName(action, member);
        return PrototypeBattleCommandResolver.CreateCommand(
            commandType,
            member.MemberId,
            member.DisplayName,
            _currentActorIndex,
            skillId,
            skillLabel,
            targetKind,
            effectType,
            powerValue,
            requiresTarget,
            summaryText);
    }

    private PrototypeBattlePhase ResolveBattlePhaseContract()
    {
        if (_dungeonRunState != DungeonRunState.Battle)
        {
            if (HasMeaningfulBattleResolution(_latestBattleResolution))
            {
                return PrototypeBattlePhase.Result;
            }

            return PrototypeBattlePhase.None;
        }

        switch (_battleState)
        {
            case BattleState.PartyActionSelect:
                return PrototypeBattlePhase.PlayerCommand;
            case BattleState.PartyTargetSelect:
                return PrototypeBattlePhase.PlayerTargetSelection;
            case BattleState.EnemyTurn:
                return _enemyIntentTelegraphActive ? PrototypeBattlePhase.EnemyIntent : PrototypeBattlePhase.EnemyAction;
            case BattleState.Victory:
                return PrototypeBattlePhase.Victory;
            case BattleState.Defeat:
                return PrototypeBattlePhase.Defeat;
            case BattleState.Retreat:
                return PrototypeBattlePhase.Retreat;
            default:
                return PrototypeBattlePhase.PendingStart;
        }
    }

    private PrototypeBattleResultType ResolveBattleResultType(string outcomeKey)
    {
        switch (outcomeKey)
        {
            case PrototypeBattleOutcomeKeys.EncounterVictory:
                return PrototypeBattleResultType.EncounterVictory;
            case PrototypeBattleOutcomeKeys.RunClear:
                return PrototypeBattleResultType.RunClear;
            case PrototypeBattleOutcomeKeys.RunDefeat:
                return PrototypeBattleResultType.RunDefeat;
            case PrototypeBattleOutcomeKeys.RunRetreat:
                return PrototypeBattleResultType.RunRetreat;
            default:
                return PrototypeBattleResultType.None;
        }
    }

    private string DescribeBattlePhase(PrototypeBattlePhase phase)
    {
        switch (phase)
        {
            case PrototypeBattlePhase.PendingStart:
                return "Encounter Start";
            case PrototypeBattlePhase.PlayerCommand:
                return "Player Command";
            case PrototypeBattlePhase.PlayerTargetSelection:
                return "Target Selection";
            case PrototypeBattlePhase.EnemyIntent:
                return "Enemy Intent";
            case PrototypeBattlePhase.EnemyAction:
                return "Enemy Action";
            case PrototypeBattlePhase.Victory:
                return "Victory";
            case PrototypeBattlePhase.Defeat:
                return "Defeat";
            case PrototypeBattlePhase.Retreat:
                return "Retreat";
            case PrototypeBattlePhase.Result:
                return "Result";
            default:
                return "None";
        }
    }

    private string[] BuildBattleResultTags(string outcomeKey, PrototypeBattleRequest request)
    {
        List<string> tags = new List<string>();
        if (HasText(outcomeKey))
        {
            tags.Add(outcomeKey);
        }

        if (request != null && request.IsEliteEncounter)
        {
            tags.Add("elite");
        }

        if (request != null && HasText(request.RouteId))
        {
            tags.Add(request.RouteId);
        }

        return tags.Count > 0 ? tags.ToArray() : Array.Empty<string>();
    }

    private string[] BuildBattleLogLines()
    {
        if (_recentBattleLogs.Count == 0)
        {
            return Array.Empty<string>();
        }

        string[] lines = new string[_recentBattleLogs.Count];
        for (int i = 0; i < _recentBattleLogs.Count; i++)
        {
            lines[i] = _recentBattleLogs[i];
        }

        return lines;
    }

    private string GetBattleObjectiveText()
    {
        ExpeditionPlan launchPlan = GetCurrentExpeditionPlanForAppFlow();
        return HasText(launchPlan != null ? launchPlan.ObjectiveText : string.Empty)
            ? launchPlan.ObjectiveText
            : "Resolve " + DefaultText(GetCurrentEncounterNameText(), "the encounter") + ".";
    }

    private string BuildBattleResolutionSummary(string outcomeKey, string encounterName)
    {
        string safeEncounterName = HasText(encounterName) ? encounterName : "Encounter";
        switch (outcomeKey)
        {
            case PrototypeBattleOutcomeKeys.EncounterVictory:
                return safeEncounterName + " cleared. Continue the expedition.";
            case PrototypeBattleOutcomeKeys.RunClear:
                return ActiveDungeonPartyText + " cleared " + _currentDungeonName + " and returned with " + BuildLootAmountText(_carriedLootAmount) + ".";
            case PrototypeBattleOutcomeKeys.RunDefeat:
                return ActiveDungeonPartyText + " was defeated in " + _currentDungeonName + ".";
            case PrototypeBattleOutcomeKeys.RunRetreat:
                return ActiveDungeonPartyText + " retreated from " + _currentDungeonName + ".";
            default:
                return safeEncounterName + " resolved.";
        }
    }

    private string GetFocusedEnemyId()
    {
        if (HasText(_hoverBattleMonsterId))
        {
            return _hoverBattleMonsterId;
        }

        if (HasText(_activeBattleMonsterId))
        {
            return _activeBattleMonsterId;
        }

        return _activeBattleMonster != null ? _activeBattleMonster.MonsterId : string.Empty;
    }

    private string BuildPartyLaneLabel(int memberIndex)
    {
        return "Slot " + (memberIndex + 1);
    }

    private PrototypeBattleUiActorData BuildBattleUiActorFromCombatant(PrototypeBattleCombatantState combatant, PrototypeBattlePhase phase)
    {
        PrototypeBattleUiActorData actor = new PrototypeBattleUiActorData();
        if (combatant == null || !HasText(combatant.DisplayName))
        {
            actor.StatusText = "Awaiting turn";
            return actor;
        }

        actor.ActorId = combatant.CombatantId ?? string.Empty;
        actor.DisplayName = DefaultText(combatant.DisplayName, "None");
        actor.RoleLabel = DefaultText(combatant.RoleLabel, "None");
        actor.SkillLabel = DefaultText(combatant.SkillLabel, combatant.TeamKey == "enemy" ? "Attack" : "Skill");
        actor.CurrentHp = Mathf.Max(0, combatant.CurrentHp);
        actor.MaxHp = Mathf.Max(1, combatant.MaxHp);
        actor.IsEnemy = combatant.TeamKey == "enemy";
        actor.StatusText = combatant.IsDefeated
            ? "Defeated"
            : phase == PrototypeBattlePhase.PlayerTargetSelection
                ? "Selecting target"
                : DefaultText(combatant.StatusText, "Ready");
        return actor;
    }

    private PrototypeBattleUiTimelineData BuildBattleUiTimelineFromContracts(PrototypeBattleRuntimeState state)
    {
        PrototypeBattleUiTimelineData timeline = new PrototypeBattleUiTimelineData();
        timeline.PhaseLabel = DefaultText(state != null ? state.PhaseLabel : string.Empty, "None");
        timeline.NextStepLabel = DefaultText(state != null ? state.PromptText : string.Empty, "Select an action.");

        List<PrototypeBattleUiTimelineSlotData> slots = new List<PrototypeBattleUiTimelineSlotData>();
        if (state != null && state.CurrentActor != null && HasText(state.CurrentActor.DisplayName))
        {
            slots.Add(CreateBattleTimelineSlot(state.CurrentActor.DisplayName, state.CurrentActor.RoleLabel, true, state.CurrentActor.TeamKey == "enemy", false, "Current"));
        }

        if (state != null && state.Phase == PrototypeBattlePhase.EnemyIntent && state.PendingEnemyIntent != null && HasText(state.PendingEnemyIntent.TargetName))
        {
            slots.Add(CreateBattleTimelineSlot(state.PendingEnemyIntent.TargetName, "Target", false, false, true, "Targeted"));
        }
        else if (state != null)
        {
            AppendQueuedPartyTimelineSlots(slots, state.PartyMembers, state.CurrentActorIndex, 3);
            PrototypeBattleCombatantState previewEnemy = GetFocusedEnemy(state.EnemyMembers, state.FocusedEnemyId);
            if (previewEnemy != null && HasText(previewEnemy.DisplayName))
            {
                slots.Add(CreateBattleTimelineSlot(previewEnemy.DisplayName, previewEnemy.RoleLabel, false, true, state.IsTargetSelectionActive, state.IsTargetSelectionActive ? "Preview" : "Queued"));
            }
        }

        if (slots.Count == 0)
        {
            slots.Add(CreateBattleTimelineSlot("Awaiting encounter", "Battle", true, false, false, "Current"));
        }

        timeline.Slots = slots.ToArray();
        return timeline;
    }

    private PrototypeBattleUiTimelineSlotData CreateBattleTimelineSlot(string label, string secondaryLabel, bool isCurrent, bool isEnemy, bool isPending, string statusLabel)
    {
        PrototypeBattleUiTimelineSlotData slot = new PrototypeBattleUiTimelineSlotData();
        slot.Label = DefaultText(label, "None");
        slot.SecondaryLabel = DefaultText(secondaryLabel, string.Empty);
        slot.StatusLabel = DefaultText(statusLabel, "Queued");
        slot.IsCurrent = isCurrent;
        slot.IsEnemy = isEnemy;
        slot.IsPending = isPending;
        return slot;
    }

    private void AppendQueuedPartyTimelineSlots(List<PrototypeBattleUiTimelineSlotData> slots, PrototypeBattleCombatantState[] partyMembers, int anchorIndex, int maxCount)
    {
        if (partyMembers == null || partyMembers.Length == 0 || maxCount <= 0)
        {
            return;
        }

        int startIndex = anchorIndex >= 0 ? anchorIndex + 1 : 0;
        int addedCount = 0;
        for (int offset = 0; offset < partyMembers.Length && addedCount < maxCount; offset++)
        {
            int index = (startIndex + offset) % partyMembers.Length;
            if (index == anchorIndex)
            {
                continue;
            }

            PrototypeBattleCombatantState member = partyMembers[index];
            if (member == null || member.IsDefeated || !HasText(member.DisplayName))
            {
                continue;
            }

            slots.Add(CreateBattleTimelineSlot(member.DisplayName, member.RoleLabel, false, false, false, "Queued"));
            addedCount += 1;
        }
    }

    private PrototypeBattleUiPartyMemberData[] BuildBattleUiPartyMembersFromContracts(PrototypeBattleCombatantState[] partyMembers)
    {
        if (partyMembers == null || partyMembers.Length == 0)
        {
            return Array.Empty<PrototypeBattleUiPartyMemberData>();
        }

        PrototypeBattleUiPartyMemberData[] members = new PrototypeBattleUiPartyMemberData[partyMembers.Length];
        for (int i = 0; i < partyMembers.Length; i++)
        {
            PrototypeBattleCombatantState combatant = partyMembers[i] ?? new PrototypeBattleCombatantState();
            PrototypeBattleUiPartyMemberData data = new PrototypeBattleUiPartyMemberData();
            data.MemberId = combatant.CombatantId ?? string.Empty;
            data.SlotIndex = combatant.SlotIndex;
            data.DisplayName = DefaultText(combatant.DisplayName, "Empty");
            data.RoleLabel = DefaultText(combatant.RoleLabel, "Adventurer");
            data.SkillLabel = DefaultText(combatant.SkillLabel, "Skill");
            data.CurrentHp = Mathf.Max(0, combatant.CurrentHp);
            data.MaxHp = Mathf.Max(1, combatant.MaxHp);
            data.Attack = combatant.Attack;
            data.Defense = combatant.Defense;
            data.Speed = combatant.Speed;
            data.IsActive = combatant.IsActing;
            data.IsTargeted = combatant.IsTargeted;
            data.IsKnockedOut = combatant.IsDefeated;
            data.StatusText = DefaultText(combatant.StatusText, "Ready");
            members[i] = data;
        }

        return members;
    }

    private PrototypeBattleUiEnemyData[] BuildBattleUiEnemyRosterFromContracts(PrototypeBattleCombatantState[] enemyMembers)
    {
        if (enemyMembers == null || enemyMembers.Length == 0)
        {
            return Array.Empty<PrototypeBattleUiEnemyData>();
        }

        PrototypeBattleUiEnemyData[] enemies = new PrototypeBattleUiEnemyData[enemyMembers.Length];
        for (int i = 0; i < enemyMembers.Length; i++)
        {
            PrototypeBattleCombatantState combatant = enemyMembers[i] ?? new PrototypeBattleCombatantState();
            PrototypeBattleUiEnemyData data = new PrototypeBattleUiEnemyData();
            data.MonsterId = combatant.CombatantId ?? string.Empty;
            data.DisplayName = DefaultText(combatant.DisplayName, "Monster");
            data.TypeLabel = DefaultText(combatant.TeamKey == "enemy" ? "Enemy" : combatant.TeamKey, "Monster");
            data.RoleLabel = DefaultText(combatant.RoleLabel, "Monster");
            data.IntentLabel = DefaultText(combatant.IntentLabel, "Unknown");
            data.TraitText = combatant.IsElite ? "Elite threat" : DefaultText(combatant.RoleLabel, string.Empty);
            data.CurrentHp = Mathf.Max(0, combatant.CurrentHp);
            data.MaxHp = Mathf.Max(1, combatant.MaxHp);
            data.Attack = combatant.Attack;
            data.IsElite = combatant.IsElite;
            data.IsSelected = combatant.IsSelected;
            data.IsHovered = combatant.IsHovered;
            data.IsActing = combatant.IsActing;
            data.IsDefeated = combatant.IsDefeated;
            data.StateLabel = DefaultText(combatant.StatusText, combatant.IsDefeated ? "Defeated" : "Ready");
            enemies[i] = data;
        }

        return enemies;
    }

    private PrototypeBattleUiEnemyData SelectBattleUiEnemy(PrototypeBattleUiEnemyData[] roster, string focusedEnemyId)
    {
        if (roster == null || roster.Length == 0)
        {
            return new PrototypeBattleUiEnemyData();
        }

        if (HasText(focusedEnemyId))
        {
            for (int i = 0; i < roster.Length; i++)
            {
                PrototypeBattleUiEnemyData enemy = roster[i];
                if (enemy != null && enemy.MonsterId == focusedEnemyId)
                {
                    return enemy;
                }
            }
        }

        for (int i = 0; i < roster.Length; i++)
        {
            PrototypeBattleUiEnemyData enemy = roster[i];
            if (enemy != null && !enemy.IsDefeated)
            {
                return enemy;
            }
        }

        return roster[0] ?? new PrototypeBattleUiEnemyData();
    }

    private PrototypeBattleUiActionContextData BuildBattleUiActionContextFromContracts(
        PrototypeBattleRuntimeState state,
        PrototypeBattleUiActorData actor,
        PrototypeBattleUiEnemyData selectedEnemy)
    {
        PrototypeBattleUiActionContextData actionContext = new PrototypeBattleUiActionContextData();
        PrototypeBattleCommand command = state != null ? state.SelectedCommand : null;
        actionContext.ActorId = actor != null ? actor.ActorId : string.Empty;
        actionContext.ActorIndex = state != null ? state.CurrentActorIndex : -1;
        actionContext.SelectedActionKey = command != null ? command.CommandKey : string.Empty;
        actionContext.SelectedActionLabel = ResolveBattleUiActionLabel(command, actor);
        actionContext.SelectedTargetId = selectedEnemy != null ? selectedEnemy.MonsterId : string.Empty;
        actionContext.ResolvedSkillId = command != null ? command.SkillId : string.Empty;
        actionContext.ResolvedSkillLabel = command != null ? command.SkillLabel : string.Empty;
        actionContext.ResolvedTargetKind = command != null ? command.TargetKind : string.Empty;
        actionContext.ResolvedEffectType = command != null ? command.EffectType : string.Empty;
        actionContext.ResolvedPowerValue = command != null ? command.PowerValue : 0;
        actionContext.IsSkillAction = command != null && command.CommandType == PrototypeBattleCommandType.Skill;
        actionContext.RequiresTarget = command != null && command.RequiresTarget;
        return actionContext;
    }

    private PrototypeBattleUiTargetContextData BuildBattleUiTargetContextFromContracts(PrototypeBattleUiEnemyData selectedEnemy)
    {
        PrototypeBattleUiTargetContextData targetContext = new PrototypeBattleUiTargetContextData();
        if (selectedEnemy == null || !HasText(selectedEnemy.MonsterId))
        {
            return targetContext;
        }

        targetContext.HasTarget = true;
        targetContext.TargetMonsterId = selectedEnemy.MonsterId;
        targetContext.TargetDisplayIndex = GetBattleMonsterDisplayIndex(selectedEnemy.MonsterId);
        targetContext.TargetLabel = DefaultText(selectedEnemy.DisplayName, "Monster");
        targetContext.TargetRoleLabel = DefaultText(selectedEnemy.RoleLabel, "Monster");
        targetContext.TargetIntentLabel = DefaultText(selectedEnemy.IntentLabel, "Unknown");
        targetContext.TargetStateText = DefaultText(selectedEnemy.StateLabel, "Ready");
        targetContext.TargetCurrentHp = Mathf.Max(0, selectedEnemy.CurrentHp);
        targetContext.TargetMaxHp = Mathf.Max(1, selectedEnemy.MaxHp);
        targetContext.IsHovered = selectedEnemy.IsHovered;
        targetContext.IsLocked = selectedEnemy.IsSelected;
        targetContext.IsDefeated = selectedEnemy.IsDefeated;
        return targetContext;
    }

    private PrototypeBattleUiCommandSurfaceData BuildBattleUiCommandSurfaceFromContracts(
        PrototypeBattleRuntimeState state,
        PrototypeBattleUiActorData actor,
        PrototypeBattleUiActionContextData actionContext)
    {
        PrototypeBattleUiCommandSurfaceData commandSurface = new PrototypeBattleUiCommandSurfaceData();
        commandSurface.SelectedActionKey = actionContext != null ? actionContext.SelectedActionKey : string.Empty;
        commandSurface.SelectedActionLabel = actionContext != null ? DefaultText(actionContext.SelectedActionLabel, "Action") : "Action";

        DungeonPartyMemberRuntimeData currentMember = actor != null && !actor.IsEnemy ? GetCurrentActorMember() : null;
        int attackPower = currentMember != null ? Mathf.Max(1, currentMember.Attack) : 1;
        string skillLabel = actor != null && HasText(actor.SkillLabel) ? actor.SkillLabel : "Skill";
        string skillEffect = actionContext != null && HasText(actionContext.ResolvedEffectType)
            ? GetBattleUiEffectText(actionContext.ResolvedEffectType, actionContext.ResolvedPowerValue)
            : "Uses the active actor's shared skill definition.";

        List<PrototypeBattleUiCommandDetailData> details = new List<PrototypeBattleUiCommandDetailData>();
        details.Add(CreateBattleUiCommandDetail("attack", "Attack", "Reliable basic strike.", "Single enemy", "None", "Deal " + attackPower + " damage.", state != null && state.CanAttack, commandSurface.SelectedActionKey == "attack"));
        details.Add(CreateBattleUiCommandDetail("skill", skillLabel, "Use the active actor skill.", DefaultText(actionContext != null ? GetBattleUiTargetTypeLabel(actionContext.ResolvedTargetKind) : string.Empty, "Single enemy"), "No cost", skillEffect, state != null && state.CanSkill, commandSurface.SelectedActionKey == "skill"));
        details.Add(CreateBattleUiCommandDetail("item", "Item", "Reserved for inventory integration.", "Self / ally", "Unavailable", "No item payload is wired yet.", false, false));
        details.Add(CreateBattleUiCommandDetail("retreat", "Retreat", "Exit the run using the current resolution flow.", "Party", "Run ends", "Return to WorldSim with the current writeback path.", state != null && state.CanRetreat, commandSurface.SelectedActionKey == "retreat"));
        commandSurface.Details = details.ToArray();
        return commandSurface;
    }

    private PrototypeBattleUiCommandDetailData CreateBattleUiCommandDetail(string key, string label, string description, string targetText, string costText, string effectText, bool isAvailable, bool isSelected)
    {
        PrototypeBattleUiCommandDetailData data = new PrototypeBattleUiCommandDetailData();
        data.Key = key ?? string.Empty;
        data.Label = DefaultText(label, "None");
        data.Description = DefaultText(description, string.Empty);
        data.TargetText = DefaultText(targetText, string.Empty);
        data.CostText = DefaultText(costText, string.Empty);
        data.EffectText = DefaultText(effectText, string.Empty);
        data.IsAvailable = isAvailable;
        data.IsSelected = isSelected;
        return data;
    }

    private PrototypeBattleUiMessageSurfaceData BuildBattleUiMessageSurfaceFromContracts(PrototypeBattleRuntimeState state)
    {
        PrototypeBattleUiMessageSurfaceData message = new PrototypeBattleUiMessageSurfaceData();
        message.Prompt = DefaultText(state != null ? state.PromptText : string.Empty, "Select an action.");
        message.CancelHint = state != null && state.IsTargetSelectionActive ? "Esc: Cancel target" : "Esc: Back";
        message.Feedback = DefaultText(state != null ? state.FeedbackText : string.Empty, string.Empty);
        message.RecentLogs = state != null && state.RecentLogLines != null
            ? state.RecentLogLines
            : Array.Empty<string>();
        return message;
    }

    private PrototypeBattleUiTargetSelectionData BuildBattleUiTargetSelectionFromContracts(
        PrototypeBattleRuntimeState state,
        PrototypeBattleUiActorData actor,
        PrototypeBattleUiActionContextData actionContext,
        PrototypeBattleUiTargetContextData targetContext,
        PrototypeBattleUiEnemyData selectedEnemy)
    {
        PrototypeBattleUiTargetSelectionData targetSelection = new PrototypeBattleUiTargetSelectionData();
        targetSelection.IsActive = state != null && state.IsTargetSelectionActive;
        targetSelection.HasFocusedTarget = targetContext != null && targetContext.HasTarget;
        targetSelection.Title = targetSelection.IsActive ? "Select target" : "Target";
        targetSelection.QueuedActionLabel = ResolveBattleUiActionLabel(state != null ? state.SelectedCommand : null, actor);
        targetSelection.TargetLabel = targetContext != null ? DefaultText(targetContext.TargetLabel, "Choose a target") : "Choose a target";
        targetSelection.TargetRoleLabel = targetContext != null ? DefaultText(targetContext.TargetRoleLabel, string.Empty) : string.Empty;
        targetSelection.TargetIntentLabel = targetContext != null ? DefaultText(targetContext.TargetIntentLabel, string.Empty) : string.Empty;
        targetSelection.TargetTraitText = selectedEnemy != null ? DefaultText(selectedEnemy.TraitText, string.Empty) : string.Empty;
        targetSelection.TargetStateText = targetContext != null ? DefaultText(targetContext.TargetStateText, "No target") : "No target";
        targetSelection.TargetCurrentHp = targetContext != null ? Mathf.Max(0, targetContext.TargetCurrentHp) : 0;
        targetSelection.TargetMaxHp = targetContext != null ? Mathf.Max(1, targetContext.TargetMaxHp) : 1;
        targetSelection.SkillHintText = actionContext != null && HasText(actionContext.ResolvedEffectType)
            ? GetBattleUiEffectText(actionContext.ResolvedEffectType, actionContext.ResolvedPowerValue)
            : string.Empty;
        targetSelection.CancelHint = "Esc: Cancel";
        return targetSelection;
    }

    private string ResolveBattleUiActionLabel(PrototypeBattleCommand command, PrototypeBattleUiActorData actor)
    {
        if (command == null)
        {
            return "Action";
        }

        switch (command.CommandType)
        {
            case PrototypeBattleCommandType.Attack:
                return "Attack";
            case PrototypeBattleCommandType.Skill:
                return HasText(command.SkillLabel)
                    ? command.SkillLabel
                    : actor != null && HasText(actor.SkillLabel)
                        ? actor.SkillLabel
                        : "Skill";
            case PrototypeBattleCommandType.Retreat:
                return "Retreat";
            default:
                return "Action";
        }
    }

    private PrototypeBattleCombatantState GetFocusedEnemy(PrototypeBattleCombatantState[] enemies, string focusedEnemyId)
    {
        if (enemies == null || enemies.Length == 0)
        {
            return null;
        }

        if (HasText(focusedEnemyId))
        {
            for (int i = 0; i < enemies.Length; i++)
            {
                PrototypeBattleCombatantState enemy = enemies[i];
                if (enemy != null && enemy.CombatantId == focusedEnemyId)
                {
                    return enemy;
                }
            }
        }

        for (int i = 0; i < enemies.Length; i++)
        {
            if (enemies[i] != null && !enemies[i].IsDefeated)
            {
                return enemies[i];
            }
        }

        return enemies[0];
    }

    private bool HasMeaningfulBattleRequest(PrototypeBattleRequest request)
    {
        return request != null &&
               (HasText(request.EncounterId) ||
                HasText(request.EncounterName) ||
                request.PartyMembers.Length > 0 ||
                request.EnemyMembers.Length > 0);
    }

    private bool HasMeaningfulBattleResolution(PrototypeBattleResolution resolution)
    {
        return resolution != null &&
               ((HasText(resolution.OutcomeKey) && resolution.OutcomeKey != PrototypeBattleOutcomeKeys.None) ||
                HasText(resolution.EncounterName));
    }

    private string DefaultText(string value, string fallback)
    {
        return HasText(value) ? value : fallback;
    }
}
