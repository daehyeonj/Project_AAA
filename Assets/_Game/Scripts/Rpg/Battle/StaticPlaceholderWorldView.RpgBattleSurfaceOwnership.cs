using System;
using System.Collections.Generic;
using UnityEngine;

public sealed partial class StaticPlaceholderWorldView
{
    private PrototypeBattleUiSurfaceData _cachedRpgOwnedBattleUiSurface = new PrototypeBattleUiSurfaceData();
    private int _cachedRpgOwnedBattleUiSurfaceStamp = int.MinValue;

    private PrototypeBattleUiSurfaceData BuildRpgOwnedBattleUiSurfaceData()
    {
        int surfaceStamp = ComputeRpgOwnedBattleUiSurfaceStamp();
        if (_cachedRpgOwnedBattleUiSurface != null && _cachedRpgOwnedBattleUiSurfaceStamp == surfaceStamp)
        {
            return _cachedRpgOwnedBattleUiSurface;
        }

        EnsureBattleContracts();
        PrototypeBattleUiSurfaceData surface = new PrototypeBattleUiSurfaceData();
        PrototypeBattleRequest request = PrototypeBattleStateFactory.CreateRequest(_activeBattleRequest);
        surface.IsBattleActive = _dungeonRunState == DungeonRunState.Battle;
        surface.IsTargetSelectionActive = _battleState == BattleState.PartyTargetSelect;
        surface.BattleStateKey = GetBattleStateKey();
        surface.CurrentDungeonName = string.IsNullOrEmpty(_currentDungeonName) ? "None" : _currentDungeonName;
        surface.CurrentRouteLabel = string.IsNullOrEmpty(_selectedRouteLabel) ? "None" : _selectedRouteLabel;
        surface.EncounterName = GetCurrentEncounterNameText();
        surface.EncounterRoomType = GetEncounterRoomTypeText();
        surface.MissionObjectiveText = string.IsNullOrEmpty(request != null ? request.ObjectiveText : string.Empty) ? "None" : request.ObjectiveText;
        surface.MissionRewardPreviewText = string.IsNullOrEmpty(request != null ? request.RewardPreviewText : string.Empty) ? "None" : request.RewardPreviewText;
        surface.MissionRiskContextText = string.IsNullOrEmpty(request != null ? request.RiskContextText : string.Empty) ? "None" : request.RiskContextText;
        surface.MissionIntentSummaryText = BuildBattleIntentSummaryText(request);
        surface.RoomProgressText = RoomProgressText;
        surface.PartyCondition = GetPartyConditionText();
        surface.TotalPartyHp = BuildTotalPartyHpSummary();
        surface.EliteStatusText = GetEliteStatusText();
        surface.EliteEncounterName = GetEliteEncounterNameText();
        surface.EliteTypeText = string.IsNullOrEmpty(_eliteType) ? "None" : _eliteType;
        surface.EliteRewardHintText = GetEliteRewardHintText();
        surface.BattleContext = BuildRpgOwnedBattleContextView();
        surface.BattleResult = BuildRpgOwnedCurrentBattleCoreResultView();
        surface.CurrentActor = BuildRpgOwnedBattleUiCurrentActorData();
        surface.Timeline = BuildRpgOwnedBattleUiTimelineData();
        surface.PartyMembers = BuildRpgOwnedBattleUiPartyMembers();
        surface.PartyStatusSurfaces = BuildRpgOwnedPartyStatusSurfaces(surface.PartyMembers);
        surface.SelectedEnemy = BuildRpgOwnedSelectedBattleUiEnemyData();
        surface.EnemyRoster = BuildRpgOwnedBattleUiEnemyRoster();
        surface.EnemyIntentSurfaces = BuildRpgOwnedEnemyIntentSurfaces(surface.EnemyRoster);
        if ((surface.SelectedEnemy == null || string.IsNullOrEmpty(surface.SelectedEnemy.MonsterId)) && surface.EnemyRoster.Length > 0)
        {
            surface.SelectedEnemy = surface.EnemyRoster[0];
        }

        surface.ActionContext = BuildRpgOwnedBattleUiActionContextData(surface.CurrentActor, surface.SelectedEnemy);
        surface.CurrentActorSurface = BuildRpgOwnedCurrentActorSurface(surface.CurrentActor, surface.ActionContext);
        surface.TargetContext = BuildRpgOwnedBattleUiTargetContextData(surface.SelectedEnemy);
        surface.CommandSurface = BuildRpgOwnedBattleUiCommandSurfaceData(surface.CurrentActor, surface.ActionContext);
        surface.MessageSurface = BuildRpgOwnedBattleUiMessageSurfaceData();
        surface.TargetSelection = BuildRpgOwnedBattleUiTargetSelectionData(surface.CurrentActor, surface.ActionContext, surface.TargetContext);
        surface.EnemyIntent = BuildRpgOwnedCurrentEnemyIntentSnapshotView();
        surface.RuntimeState = CreateRpgOwnedBattleRuntimeStateView(surface.CurrentActor, surface.ActionContext, surface.TargetContext, surface.Timeline, surface.EnemyIntent);
        surface.RecentEvents = BuildRpgOwnedRecentBattleEventRecords();
        surface.ResultSnapshot = BuildRpgOwnedCurrentBattleResultSnapshotView();
        _cachedRpgOwnedBattleUiSurface = surface;
        _cachedRpgOwnedBattleUiSurfaceStamp = surfaceStamp;
        return surface;
    }

    private int ComputeRpgOwnedBattleUiSurfaceStamp()
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
            hash = CombineBattleStamp(hash, _currentDungeonName);
            hash = CombineBattleStamp(hash, _selectedRouteLabel);
            hash = CombineBattleStamp(hash, _currentRoomStepId);
            hash = CombineBattleStamp(hash, _activeEncounterId);
            hash = CombineBattleStamp(hash, _activeBattleMonsterId);
            hash = CombineBattleStamp(hash, _hoverBattleMonsterId);
            hash = CombineBattleStamp(hash, _battleFeedbackText);
            hash = CombineBattleStamp(hash, _enemyIntentText);
            hash = CombineBattleStamp(hash, _currentSelectionPrompt);
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
                hash = CombineBattleStamp(hash, member.DisplayName);
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

    private PrototypeBattleContextData BuildRpgOwnedBattleContextView()
    {
        PrototypeBattleContextData context = new PrototypeBattleContextData();
        DungeonEncounterRuntimeData encounter = GetActiveEncounter();
        DungeonRoomTemplateData room = GetCurrentPlannedRoomStep();
        PrototypeDungeonEncounterRequest encounterRequest = BuildCurrentEncounterRequest(room, encounter);
        string encounterId = encounter != null ? encounter.EncounterId : room != null ? room.EncounterId : string.Empty;
        string encounterTypeKey = encounter != null && encounter.IsEliteEncounter
            ? "elite"
            : room != null && room.RoomType == DungeonRoomType.Elite
                ? "elite"
                : "skirmish";

        context.BattleId = BuildRpgOwnedBattleId(encounterId);
        context.EncounterId = encounterId;
        context.EncounterTypeKey = encounterTypeKey;
        context.DungeonId = string.IsNullOrEmpty(_currentDungeonId) ? string.Empty : _currentDungeonId;
        context.DungeonLabel = string.IsNullOrEmpty(_currentDungeonName) ? "Dungeon" : _currentDungeonName;
        context.RouteId = string.IsNullOrEmpty(_selectedRouteId) ? string.Empty : _selectedRouteId;
        context.RouteLabel = string.IsNullOrEmpty(_selectedRouteLabel) ? "Route" : _selectedRouteLabel;
        context.LaneRuleSetKey = BattleLaneRuleSetKey;
        context.PositionRuleKey = BattlePositionRuleKey;
        context.ConsumableRuleKey = BattleConsumableRuleKey;
        context.RetreatRuleKey = BattleRetreatRuleKey;
        context.PartySeedSummary = BuildRpgOwnedBattlePartySeedSummary();
        context.EnemyGroupSeedSummary = BuildRpgOwnedBattleEnemySeedSummary();
        context.DungeonModifierSummary = GetCurrentDungeonDangerText() + " | " + BuildCurrentThreatPressureSummaryText();
        context.WorldModifierSummary = BuildRpgOwnedBattleWorldModifierSummary();
        context.EncounterRequestSummaryText = encounterRequest.SummaryText;
        context.ContextSummaryText = context.DungeonLabel + " | " + context.RouteLabel + " | " + BuildCurrentTimePressureSummaryText();
        return context;
    }

    private PrototypeBattleRuntimeState CreateRpgOwnedBattleRuntimeStateView(
        PrototypeBattleUiActorData currentActor = null,
        PrototypeBattleUiActionContextData actionContext = null,
        PrototypeBattleUiTargetContextData targetContext = null,
        PrototypeBattleUiTimelineData timeline = null,
        PrototypeEnemyIntentSnapshot enemyIntent = null)
    {
        PrototypeBattleContextData context = BuildRpgOwnedBattleContextView();
        PrototypeBattleRuntimeState runtime = new PrototypeBattleRuntimeState();
        PrototypeBattleUiActorData safeActor = currentActor ?? BuildRpgOwnedBattleUiCurrentActorData();
        PrototypeBattleUiEnemyData selectedEnemy = null;
        if (actionContext == null || targetContext == null)
        {
            selectedEnemy = BuildRpgOwnedSelectedBattleUiEnemyData();
        }

        PrototypeBattleUiActionContextData safeActionContext = actionContext ?? BuildRpgOwnedBattleUiActionContextData(safeActor, selectedEnemy);
        PrototypeBattleUiTargetContextData safeTargetContext = targetContext ?? BuildRpgOwnedBattleUiTargetContextData(selectedEnemy);
        PrototypeBattleUiTimelineData safeTimeline = timeline ?? BuildRpgOwnedBattleUiTimelineData();
        PrototypeEnemyIntentSnapshot safeEnemyIntent = enemyIntent ?? BuildRpgOwnedCurrentEnemyIntentSnapshotView();

        runtime.BattleId = context.BattleId;
        runtime.EncounterId = context.EncounterId;
        runtime.LaneRuleSetKey = context.LaneRuleSetKey;
        runtime.PositionRuleKey = context.PositionRuleKey;
        runtime.BattleStateKey = GetBattleStateKey();
        runtime.PhaseKey = GetBattlePhaseKey();
        runtime.PhaseLabel = !string.IsNullOrEmpty(safeTimeline.PhaseLabel) ? safeTimeline.PhaseLabel : GetBattlePhaseText();
        runtime.TurnIndex = Mathf.Max(_runTurnCount, _battleTurnIndex);
        runtime.CurrentActorId = safeActor != null ? safeActor.ActorId : string.Empty;
        runtime.CurrentActorLaneKey = safeActor != null ? safeActor.LaneKey : string.Empty;
        runtime.CurrentActorLaneLabel = safeActor != null ? safeActor.LaneLabel : string.Empty;
        runtime.QueuedActionKey = safeActionContext != null && !string.IsNullOrEmpty(safeActionContext.SelectedActionKey) ? safeActionContext.SelectedActionKey : GetBattleUiSelectedActionKey();
        runtime.QueuedActionLabel = safeActionContext != null ? safeActionContext.SelectedActionLabel : string.Empty;
        runtime.SelectedTargetId = safeTargetContext != null && safeTargetContext.HasTarget ? safeTargetContext.TargetMonsterId : safeActionContext != null ? safeActionContext.SelectedTargetId : string.Empty;
        runtime.SelectedTargetLaneKey = safeTargetContext != null && safeTargetContext.HasTarget ? safeTargetContext.TargetLaneKey : safeEnemyIntent != null ? safeEnemyIntent.ThreatLaneKey : string.Empty;
        runtime.SelectedTargetLaneLabel = safeTargetContext != null && safeTargetContext.HasTarget ? safeTargetContext.TargetLaneLabel : safeEnemyIntent != null ? safeEnemyIntent.ThreatLaneLabel : string.Empty;
        runtime.ReachabilityStateKey = safeTargetContext != null && !string.IsNullOrEmpty(safeTargetContext.ReachabilityStateKey)
            ? safeTargetContext.ReachabilityStateKey
            : safeActionContext != null && safeActionContext.RequiresTarget
                ? (string.IsNullOrEmpty(runtime.SelectedTargetId) ? "pending_target" : "resolved")
                : "lane_agnostic";
        runtime.ReachabilitySummaryText = safeTargetContext != null && !string.IsNullOrEmpty(safeTargetContext.ReachabilitySummaryText)
            ? safeTargetContext.ReachabilitySummaryText
            : safeActionContext != null ? safeActionContext.ReachabilitySummaryText : string.Empty;
        runtime.ThreatLaneKey = safeEnemyIntent != null && !string.IsNullOrEmpty(safeEnemyIntent.ThreatLaneKey) ? safeEnemyIntent.ThreatLaneKey : runtime.SelectedTargetLaneKey;
        runtime.ThreatLaneLabel = safeEnemyIntent != null && !string.IsNullOrEmpty(safeEnemyIntent.ThreatLaneLabel) ? safeEnemyIntent.ThreatLaneLabel : runtime.SelectedTargetLaneLabel;
        runtime.PendingIntentKey = safeEnemyIntent != null ? safeEnemyIntent.IntentKey : string.Empty;
        runtime.PendingIntentPreviewText = safeEnemyIntent != null ? safeEnemyIntent.PreviewText : string.Empty;
        runtime.PendingIntentTargetId = safeEnemyIntent != null ? safeEnemyIntent.TargetId : string.Empty;
        runtime.TimelineSummaryText = BuildRpgOwnedBattleTimelineRuntimeSummaryText(safeTimeline);
        runtime.RecentEventCount = _battleEventRecords.Count;
        runtime.RecentEventSummaryText = BuildRpgOwnedRecentBattleEventSummaryText(3);
        runtime.IsTargetSelectionActive = _battleState == BattleState.PartyTargetSelect;
        runtime.IsEnemyTurnActive = _battleState == BattleState.EnemyTurn;
        return runtime;
    }

    private string BuildRpgOwnedBattleRuntimeSummaryText(PrototypeBattleRuntimeState runtimeState)
    {
        PrototypeBattleRuntimeState safeRuntime = runtimeState ?? new PrototypeBattleRuntimeState();
        string actorName = !string.IsNullOrEmpty(safeRuntime.CurrentActorId) ? GetCombatEntityDisplayName(safeRuntime.CurrentActorId) : string.Empty;
        string actorText = !string.IsNullOrEmpty(safeRuntime.CurrentActorId)
            ? (string.IsNullOrEmpty(actorName) ? safeRuntime.CurrentActorId : actorName)
            : "No actor";
        string actionText = !string.IsNullOrEmpty(safeRuntime.QueuedActionLabel) ? safeRuntime.QueuedActionLabel : "No action";
        string targetText = !string.IsNullOrEmpty(safeRuntime.SelectedTargetId)
            ? GetCombatEntityDisplayName(safeRuntime.SelectedTargetId)
            : "No target";
        string reachabilityText = !string.IsNullOrEmpty(safeRuntime.ReachabilitySummaryText) ? safeRuntime.ReachabilitySummaryText : "Reachability pending.";
        return safeRuntime.PhaseLabel + " | Turn " + safeRuntime.TurnIndex + " | " + actorText + " | " + actionText + " | " + targetText + " | " + reachabilityText;
    }

    private string BuildRpgOwnedBattleTimelineRuntimeSummaryText(PrototypeBattleUiTimelineData timeline)
    {
        PrototypeBattleUiTimelineData safeTimeline = timeline ?? new PrototypeBattleUiTimelineData();
        PrototypeBattleUiTimelineSlotData[] slots = safeTimeline.Slots ?? System.Array.Empty<PrototypeBattleUiTimelineSlotData>();
        if (slots.Length <= 0)
        {
            return string.IsNullOrEmpty(safeTimeline.NextStepLabel) ? "Timeline pending." : safeTimeline.NextStepLabel;
        }

        int takeCount = Mathf.Min(3, slots.Length);
        List<string> parts = new List<string>(takeCount);
        for (int i = 0; i < takeCount; i++)
        {
            PrototypeBattleUiTimelineSlotData slot = slots[i];
            if (slot == null || string.IsNullOrEmpty(slot.Label))
            {
                continue;
            }

            parts.Add(slot.Label);
        }

        return parts.Count > 0 ? string.Join(" -> ", parts.ToArray()) : "Timeline pending.";
    }

    private string BuildRpgOwnedBattlePartySeedSummary()
    {
        int totalMembers = _activeDungeonParty != null && _activeDungeonParty.Members != null ? _activeDungeonParty.Members.Length : 0;
        return ActiveDungeonPartyText + " | " + GetLivingPartyMemberCount() + "/" + totalMembers + " ready";
    }

    private string BuildRpgOwnedBattleEnemySeedSummary()
    {
        DungeonEncounterRuntimeData encounter = GetActiveEncounter();
        int totalEnemies = encounter != null && encounter.MonsterIds != null ? encounter.MonsterIds.Length : _activeMonsters.Count;
        return GetCurrentEncounterNameText() + " | " + GetDefeatedEnemyCount() + "/" + totalEnemies + " defeated";
    }

    private string BuildRpgOwnedBattleWorldModifierSummary()
    {
        return BuildCurrentSupplyPressureSummaryText() + " | " + BuildCurrentExtractionPressureSummaryText();
    }

    private string BuildRpgOwnedBattleId(string encounterId)
    {
        string dungeonId = string.IsNullOrEmpty(_currentDungeonId) ? "dungeon" : _currentDungeonId;
        string safeEncounterId = string.IsNullOrEmpty(encounterId) ? "encounter" : encounterId;
        return dungeonId + ":" + safeEncounterId + ":turn_" + Mathf.Max(_runTurnCount, _battleTurnIndex);
    }

    private PrototypeBattleUiActorData BuildRpgOwnedBattleUiCurrentActorData()
    {
        PrototypeBattleUiActorData actor = new PrototypeBattleUiActorData();
        if (_dungeonRunState == DungeonRunState.Battle && _battleState == BattleState.EnemyTurn && _activeBattleMonster != null)
        {
            actor.ActorId = _activeBattleMonster.MonsterId;
            actor.DisplayName = _activeBattleMonster.DisplayName;
            actor.RoleLabel = GetMonsterRoleText(_activeBattleMonster);
            actor.LaneKey = GetMonsterLaneKey(_activeBattleMonster);
            actor.LaneLabel = GetBattleLaneLabel(actor.LaneKey);
            actor.PositionRuleText = BuildEnemyPositionRuleText(_activeBattleMonster);
            actor.SkillLabel = string.IsNullOrEmpty(_activeBattleMonster.SpecialActionName) ? "Attack" : _activeBattleMonster.SpecialActionName;
            actor.SkillShortText = BuildRpgOwnedBattleUiEnemyIntentLabel(_activeBattleMonster);
            actor.CurrentHp = Mathf.Max(0, _activeBattleMonster.CurrentHp);
            actor.MaxHp = Mathf.Max(1, _activeBattleMonster.MaxHp);
            actor.IsEnemy = true;
            actor.StatusText = "Enemy turn";
            return actor;
        }

        DungeonPartyMemberRuntimeData member = GetCurrentActorMember();
        if (member == null)
        {
            actor.StatusText = "Awaiting turn";
            return actor;
        }

        actor.ActorId = string.IsNullOrEmpty(member.MemberId) ? string.Empty : member.MemberId;
        actor.DisplayName = string.IsNullOrEmpty(member.DisplayName) ? "None" : member.DisplayName;
        actor.RoleLabel = string.IsNullOrEmpty(member.RoleLabel) ? "Adventurer" : member.RoleLabel;
        actor.LaneKey = GetPartyMemberLaneKey(member);
        actor.LaneLabel = GetBattleLaneLabel(actor.LaneKey);
        actor.PositionRuleText = BuildPartyPositionRuleText(member);
        actor.SkillLabel = string.IsNullOrEmpty(member.SkillName) ? "Skill" : member.SkillName;
        actor.SkillShortText = string.IsNullOrEmpty(member.SkillShortText) ? string.Empty : member.SkillShortText;
        actor.CurrentHp = Mathf.Max(0, member.CurrentHp);
        actor.MaxHp = Mathf.Max(1, member.MaxHp);
        actor.Level = member.RuntimeState != null ? Mathf.Max(1, member.RuntimeState.Level) : 1;
        actor.Attack = Mathf.Max(1, member.Attack);
        actor.Defense = Mathf.Max(0, member.Defense);
        actor.Speed = Mathf.Max(0, member.Speed);
        actor.GearLabel = BuildRpgOwnedBattleGearLabel(member);
        actor.SummaryText = BuildRpgOwnedBattleMemberSummaryText(member);
        actor.IsEnemy = false;
        string statusText = _battleState == BattleState.PartyTargetSelect
            ? "Selecting target"
            : _battleState == BattleState.PartyActionSelect
                ? "Awaiting command"
                : "Ready";
        actor.StatusText = statusText;
        return actor;
    }

    private PrototypeBattleUiTimelineData BuildRpgOwnedBattleUiTimelineData()
    {
        PrototypeBattleUiTimelineData timeline = new PrototypeBattleUiTimelineData();
        timeline.PhaseLabel = GetBattlePhaseText();
        timeline.NextStepLabel = GetBattleUiNextStepText();

        List<PrototypeBattleUiTimelineSlotData> slots = new List<PrototypeBattleUiTimelineSlotData>();
        if (_battleState == BattleState.EnemyTurn && _activeBattleMonster != null)
        {
            string enemyLaneKey = GetMonsterLaneKey(_activeBattleMonster);
            string enemyLaneLabel = GetBattleLaneLabel(enemyLaneKey);
            string threatLabel = _currentEnemyIntentSnapshot != null && !string.IsNullOrEmpty(_currentEnemyIntentSnapshot.ThreatLaneLabel)
                ? _currentEnemyIntentSnapshot.ThreatLaneLabel
                : enemyLaneLabel;
            slots.Add(CreateRpgOwnedBattleUiTimelineSlot(_activeBattleMonster.DisplayName, GetMonsterRoleText(_activeBattleMonster), true, true, false, "Current", enemyLaneKey, enemyLaneLabel, threatLabel));
            DungeonPartyMemberRuntimeData targetMember = GetPartyMemberAtIndex(_pendingEnemyTargetIndex);
            if (targetMember != null && !targetMember.IsDefeated && targetMember.CurrentHp > 0)
            {
                string targetLaneKey = GetPartyMemberLaneKey(targetMember);
                string targetLaneLabel = GetBattleLaneLabel(targetLaneKey);
                slots.Add(CreateRpgOwnedBattleUiTimelineSlot(targetMember.DisplayName, targetMember.RoleLabel, false, false, true, "Targeted", targetLaneKey, targetLaneLabel));
            }

            AppendRpgOwnedBattleUiNextPartySlots(slots, _pendingEnemyTargetIndex, 3);
        }
        else
        {
            DungeonPartyMemberRuntimeData currentMember = GetCurrentActorMember();
            if (currentMember != null)
            {
                string memberLaneKey = GetPartyMemberLaneKey(currentMember);
                string memberLaneLabel = GetBattleLaneLabel(memberLaneKey);
                slots.Add(CreateRpgOwnedBattleUiTimelineSlot(currentMember.DisplayName, currentMember.RoleLabel, true, false, false, "Current", memberLaneKey, memberLaneLabel));
            }

            AppendRpgOwnedBattleUiNextPartySlots(slots, _currentActorIndex, 3);
            DungeonMonsterRuntimeData previewMonster = GetBattleUiPrimaryPreviewMonster();
            if (previewMonster != null)
            {
                string previewLaneKey = GetMonsterLaneKey(previewMonster);
                string previewLaneLabel = GetBattleLaneLabel(previewLaneKey);
                string threatLabel = BuildRpgOwnedBattleUiEnemyThreatLaneText(previewMonster);
                slots.Add(CreateRpgOwnedBattleUiTimelineSlot(
                    previewMonster.DisplayName,
                    GetMonsterRoleText(previewMonster),
                    false,
                    true,
                    _battleState == BattleState.PartyTargetSelect,
                    _battleState == BattleState.PartyTargetSelect ? "Preview" : "Queued",
                    previewLaneKey,
                    previewLaneLabel,
                    threatLabel));
            }
        }

        if (slots.Count == 0)
        {
            slots.Add(CreateRpgOwnedBattleUiTimelineSlot("Awaiting encounter", "Battle", true, false, false, "Current"));
        }

        timeline.Slots = slots.ToArray();
        return timeline;
    }

    private PrototypeBattleUiTimelineSlotData CreateRpgOwnedBattleUiTimelineSlot(
        string label,
        string secondaryLabel,
        bool isCurrent,
        bool isEnemy,
        bool isPending,
        string statusLabel,
        string laneKey = "",
        string laneLabel = "",
        string threatLabel = "")
    {
        PrototypeBattleUiTimelineSlotData slot = new PrototypeBattleUiTimelineSlotData();
        slot.Label = string.IsNullOrEmpty(label) ? "None" : label;
        slot.SecondaryLabel = string.IsNullOrEmpty(secondaryLabel) ? string.Empty : secondaryLabel;
        slot.StatusLabel = string.IsNullOrEmpty(statusLabel)
            ? isCurrent
                ? "Current"
                : isPending
                    ? "Pending"
                    : "Queued"
            : statusLabel;
        slot.LaneKey = string.IsNullOrEmpty(laneKey) ? string.Empty : laneKey;
        slot.LaneLabel = string.IsNullOrEmpty(laneLabel) ? string.Empty : laneLabel;
        slot.ThreatLabel = string.IsNullOrEmpty(threatLabel) ? string.Empty : threatLabel;
        slot.IsCurrent = isCurrent;
        slot.IsEnemy = isEnemy;
        slot.IsPending = isPending;
        return slot;
    }

    private void AppendRpgOwnedBattleUiNextPartySlots(List<PrototypeBattleUiTimelineSlotData> slots, int anchorIndex, int maxCount)
    {
        if (_activeDungeonParty == null || _activeDungeonParty.Members == null || maxCount <= 0)
        {
            return;
        }

        int startIndex = anchorIndex >= 0 ? anchorIndex + 1 : 0;
        int addedCount = 0;
        for (int offset = 0; offset < _activeDungeonParty.Members.Length && addedCount < maxCount; offset++)
        {
            int index = (startIndex + offset) % _activeDungeonParty.Members.Length;
            if (index == anchorIndex)
            {
                continue;
            }

            DungeonPartyMemberRuntimeData member = GetPartyMemberAtIndex(index);
            if (member == null || member.IsDefeated || member.CurrentHp <= 0)
            {
                continue;
            }

            string laneKey = GetPartyMemberLaneKey(member);
            string laneLabel = GetBattleLaneLabel(laneKey);
            slots.Add(CreateRpgOwnedBattleUiTimelineSlot(member.DisplayName, member.RoleLabel, false, false, false, "Queued", laneKey, laneLabel));
            addedCount += 1;
        }
    }

    private PrototypeBattleUiEnemyData BuildRpgOwnedSelectedBattleUiEnemyData()
    {
        return BuildRpgOwnedBattleUiEnemyData(GetBattleUiFocusedMonster());
    }

    private string BuildRpgOwnedBattleUiEnemyTraitText(DungeonMonsterRuntimeData monster)
    {
        if (monster == null)
        {
            return "Traits pending";
        }

        string baseTraitText;
        if (monster.IsElite)
        {
            string eliteTypeText = string.IsNullOrEmpty(_eliteType) ? "Elite pattern" : _eliteType;
            baseTraitText = string.IsNullOrEmpty(monster.SpecialActionName)
                ? eliteTypeText
                : eliteTypeText + " | " + monster.SpecialActionName;
        }
        else
        {
            baseTraitText = monster.EncounterRole == MonsterEncounterRole.Striker
                ? "Fast pressure"
                : monster.EncounterRole == MonsterEncounterRole.Skirmisher
                    ? "Flexible flank"
                    : "Front guard";
        }

        string burstWindowText = BuildRpgOwnedBurstWindowTraitText(monster);
        return string.IsNullOrEmpty(burstWindowText)
            ? baseTraitText
            : baseTraitText + " | " + burstWindowText;
    }

    private PrototypeBattleUiMessageSurfaceData BuildRpgOwnedBattleUiMessageSurfaceData()
    {
        PrototypeBattleUiMessageSurfaceData message = new PrototypeBattleUiMessageSurfaceData();
        message.Prompt = string.IsNullOrEmpty(_currentSelectionPrompt)
            ? string.IsNullOrEmpty(_battleFeedbackText) ? "Select an action." : _battleFeedbackText
            : _currentSelectionPrompt;
        message.CancelHint = GetBattleCancelHintText();
        message.Feedback = string.IsNullOrEmpty(_battleFeedbackText) ? string.Empty : _battleFeedbackText;

        List<string> logs = new List<string>();
        for (int i = 0; i < RecentBattleLogLimit; i++)
        {
            string logLine = GetRecentBattleLogText(i);
            if (!string.IsNullOrEmpty(logLine) && logLine != "None")
            {
                logs.Add(logLine);
            }
        }

        message.RecentLogs = logs.Count > 0 ? logs.ToArray() : System.Array.Empty<string>();
        return message;
    }

    private PrototypeBattleUiPartyMemberData[] BuildRpgOwnedBattleUiPartyMembers()
    {
        if (_activeDungeonParty == null || _activeDungeonParty.Members == null || _activeDungeonParty.Members.Length == 0)
        {
            return System.Array.Empty<PrototypeBattleUiPartyMemberData>();
        }

        PrototypeBattleUiPartyMemberData[] members = new PrototypeBattleUiPartyMemberData[_activeDungeonParty.Members.Length];
        for (int i = 0; i < _activeDungeonParty.Members.Length; i++)
        {
            members[i] = BuildRpgOwnedBattleUiPartyMemberData(_activeDungeonParty.Members[i], i);
        }

        return members;
    }

    private PrototypeBattleUiPartyMemberData BuildRpgOwnedBattleUiPartyMemberData(DungeonPartyMemberRuntimeData member, int memberIndex)
    {
        PrototypeBattleUiPartyMemberData data = new PrototypeBattleUiPartyMemberData();
        data.SlotIndex = memberIndex;
        if (member == null)
        {
            data.DisplayName = "Empty";
            data.StatusText = "Unavailable";
            return data;
        }

        bool isTargeted = _dungeonRunState == DungeonRunState.Battle &&
                          _battleState == BattleState.EnemyTurn &&
                          _pendingEnemyTargetIndex == memberIndex &&
                          !member.IsDefeated &&
                          member.CurrentHp > 0;
        bool isActive = _dungeonRunState == DungeonRunState.Battle &&
                        _currentActorIndex == memberIndex &&
                        !member.IsDefeated &&
                        member.CurrentHp > 0;

        data.MemberId = string.IsNullOrEmpty(member.MemberId) ? string.Empty : member.MemberId;
        data.DisplayName = string.IsNullOrEmpty(member.DisplayName) ? "Member " + (memberIndex + 1) : member.DisplayName;
        data.RoleLabel = string.IsNullOrEmpty(member.RoleLabel) ? "Adventurer" : member.RoleLabel;
        data.LaneKey = GetPartyMemberLaneKey(member);
        data.LaneLabel = GetBattleLaneLabel(data.LaneKey);
        data.PositionRuleText = BuildPartyPositionRuleText(member);
        data.SkillLabel = string.IsNullOrEmpty(member.SkillName) ? "Basic Action" : member.SkillName;
        data.SkillShortText = string.IsNullOrEmpty(member.SkillShortText) ? string.Empty : member.SkillShortText;
        data.CurrentHp = Mathf.Max(0, member.CurrentHp);
        data.MaxHp = Mathf.Max(1, member.MaxHp);
        data.Level = member.RuntimeState != null ? Mathf.Max(1, member.RuntimeState.Level) : 1;
        data.Attack = member.Attack;
        data.Defense = member.Defense;
        data.Speed = member.Speed;
        data.GearLabel = BuildRpgOwnedBattleGearLabel(member);
        data.SummaryText = BuildRpgOwnedBattleMemberSummaryText(member);
        data.IsActive = isActive;
        data.IsTargeted = isTargeted;
        data.IsKnockedOut = member.IsDefeated || member.CurrentHp <= 0;
        data.IsReachableByCurrentAction = true;
        if (!data.IsKnockedOut && _dungeonRunState == DungeonRunState.Battle)
        {
            if (_battleState == BattleState.EnemyTurn && _activeBattleMonster != null)
            {
                PrototypeBattleLaneRuleResolution enemyResolution = BuildEnemyActionLaneResolution(_activeBattleMonster, memberIndex, _pendingEnemyUsedSpecialAttack);
                data.IsReachableByCurrentAction = enemyResolution.ReachabilityStateKey == "reachable" || enemyResolution.ReachabilityStateKey == "lane_agnostic";
            }
            else if ((_battleState == BattleState.PartyActionSelect || _battleState == BattleState.PartyTargetSelect) &&
                     (_queuedBattleAction == BattleActionType.Attack || _queuedBattleAction == BattleActionType.Skill))
            {
                data.IsReachableByCurrentAction = true;
            }
        }

        data.StatusText = data.IsKnockedOut
            ? "KO"
            : data.IsActive
                ? "Acting"
                : data.IsTargeted
                    ? "Targeted"
                    : "Ready";

        return data;
    }

    private string BuildRpgOwnedBattleGearLabel(DungeonPartyMemberRuntimeData member)
    {
        if (member == null)
        {
            return string.Empty;
        }

        string gearText = PrototypeRpgEquipmentCatalog.BuildCompactReadbackText(
            member.EquipmentSummaryText,
            member.RuntimeState != null ? member.RuntimeState.GearContributionSummaryText : string.Empty);
        return string.Equals(gearText, "No gear", StringComparison.OrdinalIgnoreCase) ? string.Empty : gearText;
    }

    private string BuildRpgOwnedBattleMemberSummaryText(DungeonPartyMemberRuntimeData member)
    {
        if (member == null)
        {
            return string.Empty;
        }

        int level = member.RuntimeState != null ? Mathf.Max(1, member.RuntimeState.Level) : 1;
        string summary = "Lv " + level +
            " | ATK " + Mathf.Max(1, member.Attack) +
            " DEF " + Mathf.Max(0, member.Defense) +
            " SPD " + Mathf.Max(0, member.Speed);
        string gearLabel = BuildRpgOwnedBattleGearLabel(member);
        return string.IsNullOrEmpty(gearLabel) ? summary : summary + " | " + gearLabel;
    }

    private string BuildRpgOwnedBattleGrowthTag(DungeonPartyMemberRuntimeData member)
    {
        if (member == null || string.IsNullOrEmpty(member.EquipmentSummaryText))
        {
            return string.Empty;
        }

        string gearText = member.EquipmentSummaryText;
        if (gearText.Contains("(Elite"))
        {
            return "Elite kit";
        }

        if (gearText.Contains("(Field"))
        {
            return "Field kit";
        }

        if (gearText.Contains("(Base"))
        {
            return "Recruit kit";
        }

        return gearText;
    }

    private PrototypeBattleUiEnemyData[] BuildRpgOwnedBattleUiEnemyRoster()
    {
        List<PrototypeBattleUiEnemyData> roster = new List<PrototypeBattleUiEnemyData>();
        DungeonEncounterRuntimeData encounter = GetActiveEncounter();
        if (encounter != null && encounter.MonsterIds != null)
        {
            for (int i = 0; i < encounter.MonsterIds.Length; i++)
            {
                DungeonMonsterRuntimeData monster = GetMonsterById(encounter.MonsterIds[i]);
                if (monster != null)
                {
                    roster.Add(BuildRpgOwnedBattleUiEnemyData(monster));
                }
            }
        }

        if (roster.Count == 0)
        {
            for (int i = 0; i < _activeMonsters.Count; i++)
            {
                DungeonMonsterRuntimeData monster = _activeMonsters[i];
                if (monster != null)
                {
                    roster.Add(BuildRpgOwnedBattleUiEnemyData(monster));
                }
            }
        }

        return roster.Count > 0 ? roster.ToArray() : System.Array.Empty<PrototypeBattleUiEnemyData>();
    }

    private PrototypeBattleUiEnemyData BuildRpgOwnedBattleUiEnemyData(DungeonMonsterRuntimeData monster)
    {
        PrototypeBattleUiEnemyData data = new PrototypeBattleUiEnemyData();
        if (monster == null)
        {
            data.DisplayName = "No target selected";
            data.StateLabel = "None";
            data.IntentLabel = "Unknown";
            data.TraitText = "Traits pending";
            return data;
        }

        bool isSelected = !monster.IsDefeated && _battleState == BattleState.PartyTargetSelect && _activeBattleMonsterId == monster.MonsterId;
        bool isHovered = !monster.IsDefeated && _battleState == BattleState.PartyTargetSelect && _hoverBattleMonsterId == monster.MonsterId;
        bool isActing = !monster.IsDefeated && _battleState == BattleState.EnemyTurn && _activeBattleMonsterId == monster.MonsterId;
        data.MonsterId = string.IsNullOrEmpty(monster.MonsterId) ? string.Empty : monster.MonsterId;
        data.DisplayName = string.IsNullOrEmpty(monster.DisplayName) ? "Monster" : monster.DisplayName;
        data.TypeLabel = string.IsNullOrEmpty(monster.MonsterType) ? "Monster" : monster.MonsterType;
        data.RoleLabel = GetMonsterRoleText(monster);
        data.LaneKey = GetMonsterLaneKey(monster);
        data.LaneLabel = GetBattleLaneLabel(data.LaneKey);
        data.ThreatLaneLabel = monster.RuntimeState != null && !string.IsNullOrEmpty(monster.RuntimeState.IntentThreatLaneLabel)
            ? monster.RuntimeState.IntentThreatLaneLabel
            : data.LaneLabel;
        data.ThreatLaneText = BuildRpgOwnedBattleUiEnemyThreatLaneText(monster);
        data.PositionRuleText = BuildEnemyPositionRuleText(monster);
        if (_dungeonRunState == DungeonRunState.Battle &&
            (_battleState == BattleState.PartyActionSelect || _battleState == BattleState.PartyTargetSelect) &&
            (_queuedBattleAction == BattleActionType.Attack || _queuedBattleAction == BattleActionType.Skill))
        {
            DungeonPartyMemberRuntimeData member = GetCurrentActorMember();
            PrototypeRpgSkillDefinition skillDefinition = _queuedBattleAction == BattleActionType.Skill ? ResolveMemberSkillDefinition(member) : null;
            PrototypeBattleLaneRuleResolution laneResolution = BuildPartyActionLaneResolution(member, monster, _queuedBattleAction, skillDefinition);
            data.IsReachableByCurrentAction = laneResolution.ReachabilityStateKey == "reachable" || laneResolution.ReachabilityStateKey == "lane_agnostic";
        }

        data.IntentLabel = BuildRpgOwnedBattleUiEnemyIntentLabel(monster);
        data.TraitText = BuildRpgOwnedBattleUiEnemyTraitText(monster);
        data.CurrentHp = Mathf.Max(0, monster.CurrentHp);
        data.MaxHp = Mathf.Max(1, monster.MaxHp);
        data.Attack = monster.Attack;
        data.IsElite = monster.IsElite;
        data.IsSelected = isSelected;
        data.IsHovered = isHovered;
        data.IsActing = isActing;
        data.IsDefeated = monster.IsDefeated;
        string burstWindowStateText = BuildRpgOwnedBurstWindowStateText(monster);
        data.StateLabel = monster.IsDefeated
            ? "Defeated"
            : !string.IsNullOrEmpty(burstWindowStateText)
                ? burstWindowStateText
                : isActing
                ? "Acting"
                : isHovered
                    ? "Hover"
                    : isSelected
                        ? "Targeted"
                        : "Alive";
        if (!monster.IsDefeated &&
            (_battleState == BattleState.PartyActionSelect || _battleState == BattleState.PartyTargetSelect) &&
            (_queuedBattleAction == BattleActionType.Attack || _queuedBattleAction == BattleActionType.Skill))
        {
            data.StateLabel += data.IsReachableByCurrentAction ? " / Reachable" : " / Blocked";
        }

        return data;
    }

    private CurrentActorSurfaceData BuildRpgOwnedCurrentActorSurface(PrototypeBattleUiActorData actor, PrototypeBattleUiActionContextData actionContext)
    {
        CurrentActorSurfaceData surface = new CurrentActorSurfaceData();
        PrototypeBattleUiActorData safeActor = actor ?? new PrototypeBattleUiActorData();
        string displayName = string.IsNullOrEmpty(safeActor.DisplayName) ? "None" : safeActor.DisplayName;
        surface.ActorId = string.IsNullOrEmpty(safeActor.ActorId) ? string.Empty : safeActor.ActorId;
        surface.PortraitGlyph = string.IsNullOrEmpty(displayName) ? "?" : displayName.Substring(0, 1).ToUpperInvariant();
        surface.DisplayName = displayName;
        surface.RoleLabel = string.IsNullOrEmpty(safeActor.RoleLabel) ? "None" : safeActor.RoleLabel;
        surface.LaneKey = string.IsNullOrEmpty(safeActor.LaneKey) ? string.Empty : safeActor.LaneKey;
        surface.LaneLabel = string.IsNullOrEmpty(safeActor.LaneLabel) ? string.Empty : safeActor.LaneLabel;
        surface.PositionRuleText = string.IsNullOrEmpty(safeActor.PositionRuleText) ? string.Empty : safeActor.PositionRuleText;
        surface.ResourceLabel = safeActor.IsEnemy ? "Intent" : "Resource";
        surface.ResourceText = safeActor.IsEnemy
            ? (!string.IsNullOrEmpty(safeActor.SkillShortText) ? safeActor.SkillShortText : safeActor.SkillLabel)
            : "No MP rule";
        if (!safeActor.IsEnemy && actionContext != null && actionContext.IsSkillAction)
        {
            surface.ResourceText = "No MP | Skill-driven";
        }

        surface.SkillLabel = string.IsNullOrEmpty(safeActor.SkillLabel) ? "None" : safeActor.SkillLabel;
        surface.SummaryText = string.IsNullOrEmpty(safeActor.SummaryText) ? string.Empty : safeActor.SummaryText;
        surface.StatusText = string.IsNullOrEmpty(safeActor.StatusText) ? "Idle" : safeActor.StatusText;
        surface.CurrentHp = Mathf.Max(0, safeActor.CurrentHp);
        surface.MaxHp = Mathf.Max(1, safeActor.MaxHp);
        surface.IsEnemy = safeActor.IsEnemy;
        return surface;
    }

    private PartyStatusSurfaceData[] BuildRpgOwnedPartyStatusSurfaces(PrototypeBattleUiPartyMemberData[] members)
    {
        PrototypeBattleUiPartyMemberData[] safeMembers = members ?? System.Array.Empty<PrototypeBattleUiPartyMemberData>();
        if (safeMembers.Length <= 0)
        {
            return System.Array.Empty<PartyStatusSurfaceData>();
        }

        PartyStatusSurfaceData[] surfaces = new PartyStatusSurfaceData[safeMembers.Length];
        for (int i = 0; i < safeMembers.Length; i++)
        {
            surfaces[i] = BuildRpgOwnedPartyStatusSurface(safeMembers[i]);
        }

        return surfaces;
    }

    private PartyStatusSurfaceData BuildRpgOwnedPartyStatusSurface(PrototypeBattleUiPartyMemberData member)
    {
        PrototypeBattleUiPartyMemberData safeMember = member ?? new PrototypeBattleUiPartyMemberData();
        PartyStatusSurfaceData surface = new PartyStatusSurfaceData();
        surface.MemberId = string.IsNullOrEmpty(safeMember.MemberId) ? string.Empty : safeMember.MemberId;
        surface.DisplayName = string.IsNullOrEmpty(safeMember.DisplayName) ? "None" : safeMember.DisplayName;
        surface.RoleLabel = string.IsNullOrEmpty(safeMember.RoleLabel) ? "None" : safeMember.RoleLabel;
        surface.LaneLabel = string.IsNullOrEmpty(safeMember.LaneLabel) ? string.Empty : safeMember.LaneLabel;
        surface.PositionRuleText = string.IsNullOrEmpty(safeMember.PositionRuleText) ? string.Empty : safeMember.PositionRuleText;
        surface.SummaryText = string.IsNullOrEmpty(safeMember.SummaryText) ? string.Empty : safeMember.SummaryText;
        surface.StatusText = string.IsNullOrEmpty(safeMember.StatusText) ? "Ready" : safeMember.StatusText;
        surface.CurrentHp = Mathf.Max(0, safeMember.CurrentHp);
        surface.MaxHp = Mathf.Max(1, safeMember.MaxHp);
        surface.IsActive = safeMember.IsActive;
        surface.IsTargeted = safeMember.IsTargeted;
        surface.IsKnockedOut = safeMember.IsKnockedOut;
        surface.DangerStateKey = ResolveRpgOwnedPartyDangerStateKey(safeMember);
        return surface;
    }

    private string ResolveRpgOwnedPartyDangerStateKey(PrototypeBattleUiPartyMemberData member)
    {
        if (member == null || member.IsKnockedOut || member.CurrentHp <= 0)
        {
            return "critical";
        }

        float hpRatio = member.MaxHp > 0 ? (float)member.CurrentHp / member.MaxHp : 0f;
        if (hpRatio <= 0.35f)
        {
            return "critical";
        }

        if (member.IsTargeted || hpRatio <= 0.60f)
        {
            return "warning";
        }

        return member.IsActive ? "focus" : "stable";
    }

    private EnemyIntentSurfaceData[] BuildRpgOwnedEnemyIntentSurfaces(PrototypeBattleUiEnemyData[] enemies)
    {
        PrototypeBattleUiEnemyData[] safeEnemies = enemies ?? System.Array.Empty<PrototypeBattleUiEnemyData>();
        if (safeEnemies.Length <= 0)
        {
            return System.Array.Empty<EnemyIntentSurfaceData>();
        }

        EnemyIntentSurfaceData[] surfaces = new EnemyIntentSurfaceData[safeEnemies.Length];
        for (int i = 0; i < safeEnemies.Length; i++)
        {
            PrototypeBattleUiEnemyData enemy = safeEnemies[i] ?? new PrototypeBattleUiEnemyData();
            surfaces[i] = new EnemyIntentSurfaceData
            {
                MonsterId = string.IsNullOrEmpty(enemy.MonsterId) ? string.Empty : enemy.MonsterId,
                DisplayName = string.IsNullOrEmpty(enemy.DisplayName) ? "None" : enemy.DisplayName,
                IntentLabel = string.IsNullOrEmpty(enemy.IntentLabel) ? "Unknown" : enemy.IntentLabel,
                ThreatLaneLabel = string.IsNullOrEmpty(enemy.ThreatLaneLabel) ? string.Empty : enemy.ThreatLaneLabel,
                LaneLabel = string.IsNullOrEmpty(enemy.LaneLabel) ? string.Empty : enemy.LaneLabel,
                StateLabel = string.IsNullOrEmpty(enemy.StateLabel) ? "Alive" : enemy.StateLabel,
                CurrentHp = Mathf.Max(0, enemy.CurrentHp),
                MaxHp = Mathf.Max(1, enemy.MaxHp),
                IsElite = enemy.IsElite,
                IsActing = enemy.IsActing,
                IsSelected = enemy.IsSelected
            };
        }

        return surfaces;
    }

    private string BuildRpgOwnedBattleUiEnemyIntentLabel(DungeonMonsterRuntimeData monster)
    {
        if (monster == null)
        {
            return "Unknown";
        }

        if (!string.IsNullOrEmpty(monster.IntentLabel))
        {
            return monster.IntentLabel;
        }

        if (!string.IsNullOrEmpty(_enemyIntentText) && _activeBattleMonsterId == monster.MonsterId)
        {
            return _enemyIntentText;
        }

        if (monster.IsElite && !string.IsNullOrEmpty(monster.SpecialActionName))
        {
            return monster.SpecialActionName;
        }

        return monster.TargetPattern == MonsterTargetPattern.LowestHpLiving
            ? "Pressure lowest HP"
            : monster.TargetPattern == MonsterTargetPattern.RandomLiving
                ? "Unstable focus"
                : "Frontline pressure";
    }

    private string BuildRpgOwnedBattleUiEnemyThreatLaneText(DungeonMonsterRuntimeData monster)
    {
        if (monster == null || monster.RuntimeState == null)
        {
            return string.Empty;
        }

        List<string> parts = new List<string>();
        if (!string.IsNullOrEmpty(monster.RuntimeState.IntentThreatLaneLabel))
        {
            parts.Add(monster.RuntimeState.IntentThreatLaneLabel);
        }

        if (!string.IsNullOrEmpty(monster.RuntimeState.IntentRangeText))
        {
            parts.Add(monster.RuntimeState.IntentRangeText);
        }

        if (!string.IsNullOrEmpty(monster.RuntimeState.IntentTargetRuleText))
        {
            parts.Add(monster.RuntimeState.IntentTargetRuleText);
        }

        if (!string.IsNullOrEmpty(monster.RuntimeState.IntentPredictedReachabilityText))
        {
            parts.Add(monster.RuntimeState.IntentPredictedReachabilityText);
        }

        return parts.Count > 0 ? string.Join(" | ", parts.ToArray()) : string.Empty;
    }

    private PrototypeBattleUiCommandSurfaceData BuildRpgOwnedBattleUiCommandSurfaceData(PrototypeBattleUiActorData actor, PrototypeBattleUiActionContextData actionContext)
    {
        PrototypeBattleUiCommandSurfaceData commandSurface = new PrototypeBattleUiCommandSurfaceData();
        string selectedActionKey = actionContext != null && !string.IsNullOrEmpty(actionContext.SelectedActionKey)
            ? actionContext.SelectedActionKey
            : GetBattleUiSelectedActionKey();
        commandSurface.SelectedActionKey = selectedActionKey;
        commandSurface.SelectedActionLabel = actionContext != null && !string.IsNullOrEmpty(actionContext.SelectedActionLabel)
            ? actionContext.SelectedActionLabel
            : GetBattleUiActionDisplayLabel(selectedActionKey, actor);

        DungeonPartyMemberRuntimeData currentMember = actor != null && !actor.IsEnemy ? GetCurrentActorMember() : null;
        PrototypeRpgSkillDefinition currentSkillDefinition = currentMember != null ? ResolveMemberSkillDefinition(currentMember) : null;
        DungeonMonsterRuntimeData previewMonster = GetBattleUiPrimaryPreviewMonster();
        int basicAttackPower = currentMember != null ? Mathf.Max(1, currentMember.Attack) : 1;
        string skillLabel = actionContext != null && !string.IsNullOrEmpty(actionContext.ResolvedSkillLabel)
            ? actionContext.ResolvedSkillLabel
            : actor != null && !string.IsNullOrEmpty(actor.SkillLabel) && actor.SkillLabel != "None"
                ? actor.SkillLabel
                : "Skill";
        string skillDescription = actor != null && !string.IsNullOrEmpty(actor.SkillShortText)
            ? actor.SkillShortText
            : "Uses the active actor's shared skill definition.";
        if (actionContext != null && !string.IsNullOrEmpty(actionContext.ResolvedTargetKind))
        {
            skillDescription = GetBattleUiTargetTypeLabel(actionContext.ResolvedTargetKind) + " | " + skillDescription;
        }

        if (actionContext != null && !string.IsNullOrEmpty(actionContext.ThreatSummaryText))
        {
            skillDescription += " | " + actionContext.ThreatSummaryText;
        }

        string skillTargetText = actionContext != null && !string.IsNullOrEmpty(actionContext.ResolvedTargetKind)
            ? GetBattleUiTargetTypeLabel(actionContext.ResolvedTargetKind)
            : "Single enemy";
        string skillEffectText = BuildRpgOwnedBattleSkillEffectText(currentMember, currentSkillDefinition, actionContext, previewMonster);

        string attackTargetText = "Single enemy";
        string attackEffectText = BuildRpgOwnedBattleAttackEffectText(currentMember, previewMonster, basicAttackPower);

        string moveDescription = "Choose one adjacent row and spend the turn.";
        string moveTargetText = "Self";
        string moveEffectText = "Select a destination row from the flyout.";
        if (currentMember != null)
        {
            string currentLaneLabel = GetBattleLaneLabel(GetPartyMemberLaneKey(currentMember));
            string[] availableLaneKeys = GetRpgOwnedAvailableBattleLaneKeys(currentMember);
            string[] availableLaneLabels = new string[availableLaneKeys.Length];
            for (int i = 0; i < availableLaneKeys.Length; i++)
            {
                availableLaneLabels[i] = GetBattleLaneLabel(availableLaneKeys[i]);
            }

            moveTargetText = availableLaneLabels.Length > 0
                ? currentLaneLabel + " -> " + string.Join(" / ", availableLaneLabels)
                : currentLaneLabel + " -> None";
            moveEffectText = availableLaneLabels.Length > 0
                ? "Available shifts: " + string.Join(" / ", availableLaneLabels) + "."
                : "No adjacent row is available.";
        }

        string endTurnDescription = "Pass without attacking, using a skill, or moving.";
        string endTurnTargetText = "Self";
        string endTurnEffectText = "Advance to the next unit in the queue.";

        List<PrototypeBattleUiCommandDetailData> details = new List<PrototypeBattleUiCommandDetailData>();
        details.Add(BuildBattleUiCommandDetailData("attack", "Attack", "Basic attack with projected damage preview.", attackTargetText, "None", attackEffectText, IsBattleActionAvailable(BattleActionType.Attack), selectedActionKey == "attack"));
        details.Add(BuildBattleUiCommandDetailData("skill", skillLabel, skillDescription, skillTargetText, "No cost", skillEffectText, IsBattleActionAvailable(BattleActionType.Skill), selectedActionKey == "skill"));
        details.Add(BuildBattleUiCommandDetailData("item", "Item", "Shows usable consumables once the inventory batch is wired.", "Self / ally", "Not implemented", "No consumables are wired in this batch.", false, false));
        details.Add(BuildBattleUiCommandDetailData("defend", "Defend", "Reserved for a later guard/reaction batch.", "Self", "Not available yet", "No defend rule is wired in this batch.", false, false));
        details.Add(BuildBattleUiCommandDetailData("move", "Move", moveDescription, moveTargetText, "Ends turn", moveEffectText, IsBattleActionAvailable(BattleActionType.Move), selectedActionKey == "move"));
        AppendRpgOwnedBattleMoveOptionDetails(details, currentMember, selectedActionKey);
        details.Add(BuildBattleUiCommandDetailData("end_turn", "End Turn", endTurnDescription, endTurnTargetText, "Ends turn", endTurnEffectText, IsBattleActionAvailable(BattleActionType.EndTurn), selectedActionKey == "end_turn"));
        details.Add(BuildBattleUiCommandDetailData("retreat", "Retreat", "Leave the run using the current resolution flow.", "Party", "Ends the current run", "Return to WorldSim with the current writeback path.", IsBattleActionAvailable(BattleActionType.Retreat), selectedActionKey == "retreat"));
        details.Add(BuildBattleUiCommandDetailData("retreat_confirm", "Confirm retreat", "Commit to the retreat action.", "Party", "Confirm exit", "Uses the existing retreat resolution.", IsBattleActionAvailable(BattleActionType.Retreat), false));
        details.Add(BuildBattleUiCommandDetailData("back", "Back", "Return to the previous command layer.", "Current menu", "None", "Keeps the battle flow intact.", true, false));
        details.Add(BuildBattleUiCommandDetailData("cancel", "Cancel", "Leave confirmation without committing.", "Current dialog", "None", "Returns to party commands.", true, false));
        commandSurface.Details = details.ToArray();
        commandSurface.PrimaryButtons = BuildRpgOwnedBattlePrimaryButtons(selectedActionKey);
        commandSurface.ContextualPanelTitle = BuildRpgOwnedBattleContextPanelTitle(selectedActionKey);
        commandSurface.ContextualDetails = BuildRpgOwnedBattleContextualDetails(commandSurface.Details, selectedActionKey);
        commandSurface.ContextualPanelSummaryText = BuildRpgOwnedBattleContextPanelSummary(commandSurface.ContextualDetails);
        return commandSurface;
    }

    private PrototypeBattleUiCommandButtonData[] BuildRpgOwnedBattlePrimaryButtons(string selectedActionKey)
    {
        return new[]
        {
            BuildBattleUiCommandButtonData("attack", "Attack", "[1]", "Front strike", IsBattleActionAvailable(BattleActionType.Attack), selectedActionKey == "attack"),
            BuildBattleUiCommandButtonData("skill", "Skill", "[2]", "Signature", IsBattleActionAvailable(BattleActionType.Skill), selectedActionKey == "skill"),
            BuildBattleUiCommandButtonData("item", "Item", "[3]", "Pending", false, false),
            BuildBattleUiCommandButtonData("move", "Move", "[4]", "Row shift", IsBattleActionAvailable(BattleActionType.Move), selectedActionKey == "move"),
            BuildBattleUiCommandButtonData("end_turn", "End Turn", "[5]", "Pass", IsBattleActionAvailable(BattleActionType.EndTurn), selectedActionKey == "end_turn")
        };
    }

    private PrototypeBattleUiCommandButtonData BuildBattleUiCommandButtonData(
        string key,
        string label,
        string hotkeyText,
        string footerText,
        bool isAvailable,
        bool isSelected)
    {
        PrototypeBattleUiCommandButtonData button = new PrototypeBattleUiCommandButtonData();
        button.Key = key;
        button.Label = label;
        button.HotkeyText = hotkeyText;
        button.FooterText = footerText;
        button.IsAvailable = isAvailable;
        button.IsSelected = isSelected;
        button.OpensContextPanel = true;
        return button;
    }

    private string BuildRpgOwnedBattleContextPanelTitle(string selectedActionKey)
    {
        if (selectedActionKey == "skill")
        {
            return "Skill Context";
        }

        if (selectedActionKey == "retreat")
        {
            return "Retreat Context";
        }

        if (selectedActionKey == "move")
        {
            return "Movement Options";
        }

        if (selectedActionKey == "end_turn")
        {
            return "Turn Flow";
        }

        return "Command Context";
    }

    private PrototypeBattleUiCommandDetailData[] BuildRpgOwnedBattleContextualDetails(PrototypeBattleUiCommandDetailData[] details, string selectedActionKey)
    {
        PrototypeBattleUiCommandDetailData[] safeDetails = details ?? System.Array.Empty<PrototypeBattleUiCommandDetailData>();
        List<PrototypeBattleUiCommandDetailData> contextual = new List<PrototypeBattleUiCommandDetailData>();
        string focusKey = string.IsNullOrEmpty(selectedActionKey) ? "attack" : selectedActionKey;
        if (focusKey == "move")
        {
            AppendRpgOwnedBattleMoveContextualDetails(contextual, safeDetails);
            return contextual.Count > 0 ? contextual.ToArray() : safeDetails;
        }

        AppendBattleCommandDetail(contextual, safeDetails, focusKey);
        if (focusKey != "retreat")
        {
            AppendBattleCommandDetail(contextual, safeDetails, "retreat");
        }

        if (_battleState == BattleState.PartyTargetSelect)
        {
            AppendBattleCommandDetail(contextual, safeDetails, "cancel");
        }

        return contextual.Count > 0 ? contextual.ToArray() : safeDetails;
    }

    private void AppendBattleCommandDetail(List<PrototypeBattleUiCommandDetailData> target, PrototypeBattleUiCommandDetailData[] source, string key)
    {
        if (target == null || source == null || string.IsNullOrEmpty(key))
        {
            return;
        }

        for (int i = 0; i < source.Length; i++)
        {
            PrototypeBattleUiCommandDetailData detail = source[i];
            if (detail != null && detail.Key == key)
            {
                target.Add(detail);
                return;
            }
        }
    }

    private string BuildRpgOwnedBattleContextPanelSummary(PrototypeBattleUiCommandDetailData[] contextualDetails)
    {
        PrototypeBattleUiCommandDetailData[] safeDetails = contextualDetails ?? System.Array.Empty<PrototypeBattleUiCommandDetailData>();
        if (safeDetails.Length <= 0 || safeDetails[0] == null)
        {
            return "Select a command.";
        }

        PrototypeBattleUiCommandDetailData detail = safeDetails[0];
        List<string> parts = new List<string>();
        if (!string.IsNullOrEmpty(detail.Description))
        {
            parts.Add(detail.Description);
        }

        if (!string.IsNullOrEmpty(detail.TargetText))
        {
            parts.Add(detail.TargetText);
        }

        if (!string.IsNullOrEmpty(detail.EffectText))
        {
            parts.Add(detail.EffectText);
        }

        return parts.Count > 0 ? string.Join(" | ", parts.ToArray()) : "Select a command.";
    }

    private void AppendRpgOwnedBattleMoveOptionDetails(List<PrototypeBattleUiCommandDetailData> details, DungeonPartyMemberRuntimeData member, string selectedActionKey)
    {
        if (details == null || member == null)
        {
            return;
        }

        string currentLaneLabel = GetBattleLaneLabel(GetPartyMemberLaneKey(member));
        string[] availableLaneKeys = GetRpgOwnedAvailableBattleLaneKeys(member);
        for (int i = 0; i < availableLaneKeys.Length; i++)
        {
            string laneKey = availableLaneKeys[i];
            string laneLabel = GetBattleLaneLabel(laneKey);
            bool isRanged = IsRangedPartyMember(member);
            details.Add(BuildBattleUiCommandDetailData(
                GetRpgOwnedBattleMoveActionKey(laneKey),
                "To " + laneLabel,
                "Shift from " + currentLaneLabel + " into " + laneLabel + ".",
                currentLaneLabel + " -> " + laneLabel,
                "Ends turn",
                BuildRowReachText(laneKey, isRanged),
                CanRpgOwnedCurrentActorShiftToBattleLane(member, laneKey),
                selectedActionKey == "move" && i == 0));
        }
    }

    private void AppendRpgOwnedBattleMoveContextualDetails(List<PrototypeBattleUiCommandDetailData> contextual, PrototypeBattleUiCommandDetailData[] details)
    {
        if (contextual == null || details == null)
        {
            return;
        }

        for (int i = 0; i < details.Length; i++)
        {
            PrototypeBattleUiCommandDetailData detail = details[i];
            if (detail != null && IsRpgOwnedBattleMoveActionKey(detail.Key))
            {
                contextual.Add(detail);
            }
        }

        if (contextual.Count == 0)
        {
            AppendBattleCommandDetail(contextual, details, "move");
        }
    }

    private PrototypeBattleUiTargetSelectionData BuildRpgOwnedBattleUiTargetSelectionData(PrototypeBattleUiActorData actor, PrototypeBattleUiActionContextData actionContext, PrototypeBattleUiTargetContextData targetContext)
    {
        PrototypeBattleUiTargetSelectionData targetSelection = new PrototypeBattleUiTargetSelectionData();
        targetSelection.IsActive = _battleState == BattleState.PartyTargetSelect;
        targetSelection.Title = actionContext != null && actionContext.IsSkillAction ? "Skill Target" : "Target Selection";
        targetSelection.QueuedActionLabel = actionContext != null && !string.IsNullOrEmpty(actionContext.SelectedActionLabel)
            ? actionContext.SelectedActionLabel
            : GetBattleUiActionDisplayLabel(GetBattleUiSelectedActionKey(), actor);
        targetSelection.HasFocusedTarget = targetContext != null && targetContext.HasTarget && targetContext.IsHovered;
        targetSelection.TargetLabel = targetContext != null && targetContext.HasTarget ? targetContext.TargetLabel : "Choose a target";
        targetSelection.TargetRoleLabel = targetContext != null && targetContext.HasTarget ? targetContext.TargetRoleLabel : string.Empty;
        targetSelection.TargetIntentLabel = targetContext != null && targetContext.HasTarget ? targetContext.TargetIntentLabel : string.Empty;
        targetSelection.TargetTraitText = targetContext != null && targetContext.HasTarget
            ? BuildRpgOwnedBattleUiEnemyTraitText(GetMonsterById(targetContext.TargetMonsterId))
            : string.Empty;
        targetSelection.TargetStateText = targetContext != null && targetContext.HasTarget
            ? targetContext.TargetStateText
            : "Hover an enemy or click a target.";
        targetSelection.TargetCurrentHp = targetContext != null && targetContext.HasTarget ? targetContext.TargetCurrentHp : 0;
        targetSelection.TargetMaxHp = targetContext != null && targetContext.HasTarget ? Mathf.Max(1, targetContext.TargetMaxHp) : 1;
        List<string> selectionHints = new List<string>();
        if (actionContext != null && actionContext.IsSkillAction && actor != null && !string.IsNullOrEmpty(actor.SkillShortText))
        {
            selectionHints.Add(actor.SkillShortText);
        }

        if (targetContext != null && !string.IsNullOrEmpty(targetContext.TargetRuleText))
        {
            selectionHints.Add(targetContext.TargetRuleText);
        }

        if (targetContext != null && !string.IsNullOrEmpty(targetContext.ReachabilitySummaryText))
        {
            selectionHints.Add(targetContext.ReachabilitySummaryText);
        }

        targetSelection.SkillHintText = selectionHints.Count > 0 ? string.Join(" | ", selectionHints.ToArray()) : string.Empty;
        targetSelection.ReachabilitySummaryText = targetContext != null && !string.IsNullOrEmpty(targetContext.ReachabilitySummaryText)
            ? targetContext.ReachabilitySummaryText
            : actionContext != null
                ? actionContext.ReachabilitySummaryText
                : string.Empty;
        targetSelection.TargetRuleText = targetContext != null && !string.IsNullOrEmpty(targetContext.TargetRuleText)
            ? targetContext.TargetRuleText
            : actionContext != null && !string.IsNullOrEmpty(actionContext.ResolvedLaneRuleKey)
                ? BuildTargetRuleText(actionContext.ResolvedLaneRuleKey)
                : string.Empty;
        targetSelection.ThreatSummaryText = actionContext != null ? actionContext.ThreatSummaryText : string.Empty;
        targetSelection.CancelHint = GetBattleCancelHintText();
        return targetSelection;
    }

    private PrototypeBattleUiActionContextData BuildRpgOwnedBattleUiActionContextData(PrototypeBattleUiActorData actor, PrototypeBattleUiEnemyData selectedEnemy)
    {
        PrototypeBattleUiActionContextData actionContext = new PrototypeBattleUiActionContextData();
        if (actor == null)
        {
            return actionContext;
        }

        actionContext.ActorId = string.IsNullOrEmpty(actor.ActorId) ? string.Empty : actor.ActorId;
        actionContext.ActorIndex = actor.IsEnemy ? _enemyTurnMonsterCursor : _currentActorIndex;
        string selectedActionKey = GetBattleUiSelectedActionKey();
        actionContext.SelectedActionKey = selectedActionKey;
        actionContext.SelectedActionLabel = GetBattleUiActionDisplayLabel(selectedActionKey, actor);
        actionContext.SelectedTargetId = selectedEnemy != null && !string.IsNullOrEmpty(selectedEnemy.MonsterId)
            ? selectedEnemy.MonsterId
            : string.Empty;

        if (actor.IsEnemy)
        {
            PrototypeBattleLaneRuleResolution enemyResolution = BuildEnemyActionLaneResolution(_activeBattleMonster, _pendingEnemyTargetIndex, _pendingEnemyUsedSpecialAttack);
            actionContext.ResolvedRangeKey = enemyResolution.ResolvedRangeKey;
            actionContext.ResolvedLaneRuleKey = enemyResolution.ResolvedLaneRuleKey;
            actionContext.RangeText = enemyResolution.RangeText;
            actionContext.LaneImpactText = enemyResolution.LaneImpactText;
            actionContext.ReachabilitySummaryText = enemyResolution.ReachabilitySummaryText;
            actionContext.ThreatSummaryText = enemyResolution.ThreatSummaryText;
            actionContext.RequiresTarget = !string.IsNullOrEmpty(actionContext.SelectedTargetId);
            return actionContext;
        }

        DungeonPartyMemberRuntimeData member = GetCurrentActorMember();
        if (member == null)
        {
            return actionContext;
        }

        if (selectedActionKey == "skill")
        {
            PrototypeRpgSkillDefinition skillDefinition = ResolveMemberSkillDefinition(member);
            DungeonMonsterRuntimeData selectedMonster = selectedEnemy != null && !string.IsNullOrEmpty(selectedEnemy.MonsterId)
                ? GetMonsterById(selectedEnemy.MonsterId)
                : GetBattleUiPrimaryPreviewMonster();
            PrototypeBattleLaneRuleResolution laneResolution = BuildPartyActionLaneResolution(member, selectedMonster, BattleActionType.Skill, skillDefinition);
            actionContext.IsSkillAction = true;
            actionContext.ResolvedSkillId = skillDefinition != null ? skillDefinition.SkillId : string.Empty;
            actionContext.ResolvedSkillLabel = GetResolvedSkillDisplayName(member, skillDefinition);
            actionContext.ResolvedTargetKind = GetResolvedSkillTargetKind(member, skillDefinition);
            actionContext.ResolvedEffectType = GetResolvedSkillEffectType(member, skillDefinition);
            actionContext.ResolvedPowerValue = GetResolvedSkillPower(member, skillDefinition);
            actionContext.ResolvedRangeKey = laneResolution.ResolvedRangeKey;
            actionContext.ResolvedLaneRuleKey = laneResolution.ResolvedLaneRuleKey;
            actionContext.RangeText = laneResolution.RangeText;
            actionContext.LaneImpactText = laneResolution.LaneImpactText;
            actionContext.ReachabilitySummaryText = laneResolution.ReachabilitySummaryText;
            actionContext.ThreatSummaryText = BuildRpgOwnedBattleThreatSummary(
                laneResolution.ThreatSummaryText,
                "skill",
                member,
                skillDefinition,
                selectedMonster);
            actionContext.RequiresTarget = actionContext.ResolvedTargetKind == "single_enemy";
            return actionContext;
        }

        if (selectedActionKey == "attack")
        {
            DungeonMonsterRuntimeData selectedMonster = selectedEnemy != null && !string.IsNullOrEmpty(selectedEnemy.MonsterId)
                ? GetMonsterById(selectedEnemy.MonsterId)
                : GetBattleUiPrimaryPreviewMonster();
            PrototypeBattleLaneRuleResolution laneResolution = BuildPartyActionLaneResolution(member, selectedMonster, BattleActionType.Attack, null);
            actionContext.ResolvedTargetKind = "single_enemy";
            actionContext.ResolvedEffectType = "damage";
            actionContext.ResolvedPowerValue = Mathf.Max(1, member.Attack);
            actionContext.ResolvedRangeKey = laneResolution.ResolvedRangeKey;
            actionContext.ResolvedLaneRuleKey = laneResolution.ResolvedLaneRuleKey;
            actionContext.RangeText = laneResolution.RangeText;
            actionContext.LaneImpactText = laneResolution.LaneImpactText;
            actionContext.ReachabilitySummaryText = laneResolution.ReachabilitySummaryText;
            actionContext.ThreatSummaryText = BuildRpgOwnedBattleThreatSummary(
                laneResolution.ThreatSummaryText,
                "attack",
                member,
                null,
                selectedMonster);
            actionContext.RequiresTarget = true;
            return actionContext;
        }

        if (selectedActionKey == "move")
        {
            string currentLaneKey = GetPartyMemberLaneKey(member);
            string currentLaneLabel = GetBattleLaneLabel(currentLaneKey);
            string[] availableLaneKeys = GetRpgOwnedAvailableBattleLaneKeys(member);
            string[] availableLaneLabels = new string[availableLaneKeys.Length];
            for (int i = 0; i < availableLaneKeys.Length; i++)
            {
                availableLaneLabels[i] = GetBattleLaneLabel(availableLaneKeys[i]);
            }

            actionContext.ResolvedTargetKind = "self";
            actionContext.ResolvedEffectType = "move";
            actionContext.ResolvedPowerValue = 0;
            actionContext.ResolvedRangeKey = PrototypeBattleRangeKeys.LaneAgnostic;
            actionContext.ResolvedLaneRuleKey = PrototypeBattleLaneRuleKeys.PartyWide;
            actionContext.RangeText = "Self reposition";
            actionContext.LaneImpactText = availableLaneLabels.Length > 0
                ? "Shift from " + currentLaneLabel + " to " + string.Join(" / ", availableLaneLabels) + "."
                : "No adjacent row is available.";
            actionContext.ReachabilitySummaryText = CanCurrentActorShiftBattleLane(member)
                ? "Choose one adjacent row."
                : "No row shift available.";
            actionContext.ThreatSummaryText = "Reposition to change row reach and threat.";
            actionContext.RequiresTarget = false;
            return actionContext;
        }

        if (selectedActionKey == "end_turn")
        {
            actionContext.ResolvedTargetKind = "self";
            actionContext.ResolvedEffectType = "end_turn";
            actionContext.ResolvedPowerValue = 0;
            actionContext.ResolvedRangeKey = PrototypeBattleRangeKeys.LaneAgnostic;
            actionContext.ResolvedLaneRuleKey = PrototypeBattleLaneRuleKeys.PartyWide;
            actionContext.RangeText = "Self";
            actionContext.LaneImpactText = "No movement.";
            actionContext.ReachabilitySummaryText = "Always available while the unit can act.";
            actionContext.ThreatSummaryText = "Pass and advance to the next turn.";
            actionContext.RequiresTarget = false;
            return actionContext;
        }

        if (selectedActionKey == "retreat")
        {
            PrototypeBattleLaneRuleResolution laneResolution = BuildPartyActionLaneResolution(member, null, BattleActionType.Retreat, null);
            actionContext.ResolvedTargetKind = "party";
            actionContext.ResolvedEffectType = "retreat";
            actionContext.ResolvedPowerValue = 0;
            actionContext.ResolvedRangeKey = laneResolution.ResolvedRangeKey;
            actionContext.ResolvedLaneRuleKey = laneResolution.ResolvedLaneRuleKey;
            actionContext.RangeText = laneResolution.RangeText;
            actionContext.LaneImpactText = laneResolution.LaneImpactText;
            actionContext.ReachabilitySummaryText = laneResolution.ReachabilitySummaryText;
            actionContext.ThreatSummaryText = laneResolution.ThreatSummaryText;
            actionContext.RequiresTarget = false;
        }

        return actionContext;
    }

    private PrototypeBattleUiTargetContextData BuildRpgOwnedBattleUiTargetContextData(PrototypeBattleUiEnemyData selectedEnemy)
    {
        PrototypeBattleUiTargetContextData targetContext = new PrototypeBattleUiTargetContextData();
        if (selectedEnemy == null || string.IsNullOrEmpty(selectedEnemy.MonsterId))
        {
            return targetContext;
        }

        targetContext.HasTarget = true;
        targetContext.TargetMonsterId = selectedEnemy.MonsterId;
        targetContext.TargetDisplayIndex = GetBattleMonsterDisplayIndex(selectedEnemy.MonsterId);
        targetContext.TargetLabel = string.IsNullOrEmpty(selectedEnemy.DisplayName) ? "Monster" : selectedEnemy.DisplayName;
        targetContext.TargetRoleLabel = string.IsNullOrEmpty(selectedEnemy.RoleLabel) ? "Monster" : selectedEnemy.RoleLabel;
        targetContext.TargetLaneKey = selectedEnemy.LaneKey;
        targetContext.TargetLaneLabel = selectedEnemy.LaneLabel;
        targetContext.TargetIntentLabel = string.IsNullOrEmpty(selectedEnemy.IntentLabel) ? "Unknown" : selectedEnemy.IntentLabel;
        targetContext.TargetStateText = string.IsNullOrEmpty(selectedEnemy.StateLabel) ? "Alive" : selectedEnemy.StateLabel;
        targetContext.TargetCurrentHp = Mathf.Max(0, selectedEnemy.CurrentHp);
        targetContext.TargetMaxHp = Mathf.Max(1, selectedEnemy.MaxHp);
        targetContext.IsHovered = selectedEnemy.IsHovered;
        targetContext.IsLocked = !string.IsNullOrEmpty(_activeBattleMonsterId) && _activeBattleMonsterId == selectedEnemy.MonsterId;
        targetContext.IsDefeated = selectedEnemy.IsDefeated;
        if (_dungeonRunState == DungeonRunState.Battle &&
            (_battleState == BattleState.PartyActionSelect || _battleState == BattleState.PartyTargetSelect) &&
            (_queuedBattleAction == BattleActionType.Attack || _queuedBattleAction == BattleActionType.Skill))
        {
            targetContext.ReachabilityStateKey = selectedEnemy.IsReachableByCurrentAction ? "reachable" : "blocked";
        }

        DungeonPartyMemberRuntimeData member = GetCurrentActorMember();
        if (member != null && (_queuedBattleAction == BattleActionType.Attack || _queuedBattleAction == BattleActionType.Skill))
        {
            DungeonMonsterRuntimeData selectedMonster = GetMonsterById(selectedEnemy.MonsterId);
            PrototypeRpgSkillDefinition skillDefinition = _queuedBattleAction == BattleActionType.Skill ? ResolveMemberSkillDefinition(member) : null;
            PrototypeBattleLaneRuleResolution laneResolution = BuildPartyActionLaneResolution(member, selectedMonster, _queuedBattleAction, skillDefinition);
            targetContext.ReachabilityStateKey = laneResolution.ReachabilityStateKey;
            string burstWindowText = BuildRpgOwnedBurstWindowTraitText(selectedMonster);
            targetContext.TargetRuleText = string.IsNullOrEmpty(burstWindowText)
                ? laneResolution.TargetRuleText
                : burstWindowText + " | " + laneResolution.TargetRuleText;
            targetContext.ReachabilitySummaryText = laneResolution.ReachabilitySummaryText;
        }

        return targetContext;
    }

    private string BuildRpgOwnedRecentBattleEventSummaryText(int count)
    {
        PrototypeBattleEventRecord[] records = BuildRpgOwnedRecentBattleEventRecords(count);
        if (records == null || records.Length == 0)
        {
            return "No recent battle events.";
        }

        List<string> parts = new List<string>(records.Length);
        for (int i = 0; i < records.Length; i++)
        {
            PrototypeBattleEventRecord record = records[i];
            if (record == null)
            {
                continue;
            }

            string text = !string.IsNullOrEmpty(record.ShortText) ? record.ShortText : record.Summary;
            if (!string.IsNullOrEmpty(text))
            {
                parts.Add(text);
            }
        }

        return parts.Count > 0 ? string.Join(" | ", parts.ToArray()) : "No recent battle events.";
    }
}
