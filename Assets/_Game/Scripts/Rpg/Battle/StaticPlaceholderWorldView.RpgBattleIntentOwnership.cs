using System.Collections.Generic;
using UnityEngine;

public sealed partial class StaticPlaceholderWorldView
{
    private PrototypeEnemyIntentSnapshot BuildRpgOwnedEnemyIntentSnapshot(DungeonMonsterRuntimeData monster, int targetIndex, bool useSpecial)
    {
        PrototypeEnemyIntentSnapshot snapshot = new PrototypeEnemyIntentSnapshot();
        if (monster == null)
        {
            return snapshot;
        }

        PrototypeBattleLaneRuleResolution laneResolution = BuildEnemyActionLaneResolution(monster, targetIndex, useSpecial);
        string targetPatternKey = useSpecial && IsRpgOwnedPartyWideEliteSpecial(monster, useSpecial)
            ? "all_living_allies"
            : monster.TargetPattern == MonsterTargetPattern.LowestHpLiving
                ? "lowest_hp_living"
                : monster.TargetPattern == MonsterTargetPattern.RandomLiving
                    ? "random_living"
                    : "frontmost_living";
        DungeonPartyMemberRuntimeData targetMember = GetPartyMemberAtIndex(targetIndex);
        snapshot.IntentKey = useSpecial ? "enemy_special_attack" : "enemy_attack";
        snapshot.TargetPatternKey = targetPatternKey;
        snapshot.PreviewText = BuildRpgOwnedEnemyIntentText(monster, targetIndex, useSpecial);
        snapshot.PredictedValue = Mathf.Max(1, GetRpgOwnedEnemyActionPower(monster, useSpecial));
        snapshot.TargetId = targetMember != null ? targetMember.MemberId : string.Empty;
        snapshot.TargetName = targetMember != null ? targetMember.DisplayName : string.Empty;
        snapshot.SourceEnemyId = monster.MonsterId;
        snapshot.SourceEnemyName = monster.DisplayName;
        snapshot.ActionKey = useSpecial ? "skill" : "attack";
        snapshot.ActionLabel = GetRpgOwnedEnemyActionLabel(monster, useSpecial);
        snapshot.EffectKey = useSpecial ? "skill_damage" : "damage";
        snapshot.SourceRoleLabel = GetMonsterRoleText(monster);
        snapshot.RangeKey = laneResolution.ResolvedRangeKey;
        snapshot.LaneRuleKey = laneResolution.ResolvedLaneRuleKey;
        snapshot.ThreatLaneKey = laneResolution.ThreatLaneKey;
        snapshot.ThreatLaneLabel = laneResolution.ThreatLaneLabel;
        snapshot.RangeText = laneResolution.RangeText;
        snapshot.PredictedRangeText = laneResolution.RangeText;
        snapshot.PredictedReachabilityText = laneResolution.PredictedReachabilityText;
        snapshot.TargetRuleText = laneResolution.TargetRuleText;
        return snapshot;
    }

    private PrototypeEnemyIntentSnapshot BuildRpgOwnedCurrentEnemyIntentSnapshotView()
    {
        return CopyRpgOwnedEnemyIntentSnapshot(_currentEnemyIntentSnapshot);
    }

    private void ResetRpgOwnedCombatRulePipelineState()
    {
        _battleEventRecords.Clear();
        _currentBattleResultSnapshot = new PrototypeBattleResultSnapshot();
        _currentEnemyIntentSnapshot = new PrototypeEnemyIntentSnapshot();
        _battleEventStepIndex = 0;
        _totalDamageDealt = 0;
        _totalDamageTaken = 0;
        _totalHealingDone = 0;
    }

    private void UpdateRpgOwnedBattleResultSnapshot(string outcomeKey)
    {
        _currentBattleResultSnapshot = BuildRpgOwnedBattleResultSnapshot(outcomeKey);
    }

    private PrototypeBattleEventRecord[] BuildRpgOwnedRecentBattleEventRecords(int maxCount = 8)
    {
        if (_battleEventRecords.Count == 0)
        {
            return System.Array.Empty<PrototypeBattleEventRecord>();
        }

        int count = Mathf.Clamp(maxCount, 1, _battleEventRecords.Count);
        PrototypeBattleEventRecord[] records = new PrototypeBattleEventRecord[count];
        int startIndex = _battleEventRecords.Count - count;
        for (int i = 0; i < count; i++)
        {
            records[i] = CopyRpgOwnedBattleEventRecord(_battleEventRecords[startIndex + i]);
        }

        return records;
    }

    private PrototypeBattleEventRecord GetRpgOwnedLatestBattleEventRecord()
    {
        if (_battleEventRecords.Count <= 0)
        {
            return new PrototypeBattleEventRecord();
        }

        return CopyRpgOwnedBattleEventRecord(_battleEventRecords[_battleEventRecords.Count - 1]);
    }

    private void RecordRpgOwnedBattleEvent(
        string eventKey,
        string actorId,
        string targetId,
        int amount,
        string summary,
        string actionKey = "",
        string skillId = "",
        string phaseKey = "",
        string actorName = "",
        string targetName = "",
        string shortText = "",
        string eventType = "")
    {
        if (string.IsNullOrEmpty(eventKey))
        {
            return;
        }

        int sequence = _battleEventStepIndex;
        string resolvedPhaseKey = string.IsNullOrEmpty(phaseKey) ? GetBattlePhaseKey() : phaseKey;
        string resolvedActionKey = string.IsNullOrEmpty(actionKey) ? InferBattleActionKeyForEvent(actorId) : actionKey;
        string resolvedSkillId = string.IsNullOrEmpty(skillId) ? InferBattleSkillIdForEvent(actorId, resolvedActionKey) : skillId;
        string resolvedActorName = string.IsNullOrEmpty(actorName) ? GetCombatEntityDisplayName(actorId) : actorName;
        string resolvedTargetName = string.IsNullOrEmpty(targetName) ? GetCombatEntityDisplayName(targetId) : targetName;
        string resolvedSummary = string.IsNullOrEmpty(summary) ? eventKey : summary;
        string resolvedShortText = string.IsNullOrEmpty(shortText) ? resolvedSummary : shortText;

        PrototypeBattleEventRecord record = new PrototypeBattleEventRecord();
        record.EventId = eventKey + "_" + sequence;
        record.Sequence = sequence;
        record.EventKey = eventKey;
        record.EventType = string.IsNullOrEmpty(eventType) ? eventKey : eventType;
        record.PhaseKey = resolvedPhaseKey;
        record.ActorId = string.IsNullOrEmpty(actorId) ? string.Empty : actorId;
        record.ActorName = string.IsNullOrEmpty(resolvedActorName) ? string.Empty : resolvedActorName;
        record.TargetId = string.IsNullOrEmpty(targetId) ? string.Empty : targetId;
        record.TargetName = string.IsNullOrEmpty(resolvedTargetName) ? string.Empty : resolvedTargetName;
        record.ActionKey = string.IsNullOrEmpty(resolvedActionKey) ? string.Empty : resolvedActionKey;
        record.SkillId = string.IsNullOrEmpty(resolvedSkillId) ? string.Empty : resolvedSkillId;
        record.Amount = amount;
        record.Value = amount;
        record.StepIndex = sequence;
        record.TurnIndex = Mathf.Max(0, _battleTurnIndex);
        record.ShortText = resolvedShortText;
        record.Summary = resolvedSummary;
        _battleEventStepIndex += 1;
        _battleEventRecords.Add(record);
        if (_battleEventRecords.Count > 32)
        {
            _battleEventRecords.RemoveAt(0);
        }
    }

    private void RecordRpgOwnedEnemyTurnStartEvent(DungeonMonsterRuntimeData monster)
    {
        if (monster == null)
        {
            return;
        }

        RecordRpgOwnedBattleEvent(
            PrototypeBattleEventKeys.TurnStart,
            monster.MonsterId,
            string.Empty,
            Mathf.Max(0, _enemyTurnMonsterCursor),
            monster.DisplayName + " turn started.",
            phaseKey: "enemy_turn",
            actorName: monster.DisplayName,
            shortText: "Turn started");
    }

    private int GetRpgOwnedMonsterTargetPartyMemberIndex(DungeonMonsterRuntimeData monster)
    {
        return GetRpgOwnedMonsterTargetPartyMemberIndex(monster, false);
    }

    private int GetRpgOwnedMonsterTargetPartyMemberIndex(DungeonMonsterRuntimeData monster, bool useSpecial)
    {
        if (monster == null)
        {
            return GetFirstLivingPartyMemberIndex();
        }

        List<int> candidates = GetRpgOwnedReachableEnemyTargetCandidates(monster, useSpecial);
        if (candidates.Count <= 0)
        {
            return -1;
        }

        if (monster.TargetPattern == MonsterTargetPattern.LowestHpLiving)
        {
            int bestIndex = candidates[0];
            int bestHp = GetPartyMemberAtIndex(bestIndex) != null ? GetPartyMemberAtIndex(bestIndex).CurrentHp : int.MaxValue;
            for (int i = 1; i < candidates.Count; i++)
            {
                DungeonPartyMemberRuntimeData candidate = GetPartyMemberAtIndex(candidates[i]);
                int hp = candidate != null ? candidate.CurrentHp : int.MaxValue;
                if (hp < bestHp)
                {
                    bestHp = hp;
                    bestIndex = candidates[i];
                }
            }

            return bestIndex;
        }

        if (monster.TargetPattern == MonsterTargetPattern.RandomLiving)
        {
            int startIndex = Mathf.Abs(monster.TurnsActed) % candidates.Count;
            return candidates[startIndex];
        }

        return candidates[0];
    }

    private List<int> GetRpgOwnedReachableEnemyTargetCandidates(DungeonMonsterRuntimeData monster, bool useSpecial)
    {
        List<int> candidates = new List<int>();
        if (_activeDungeonParty == null || _activeDungeonParty.Members == null)
        {
            return candidates;
        }

        for (int i = 0; i < _activeDungeonParty.Members.Length; i++)
        {
            DungeonPartyMemberRuntimeData member = _activeDungeonParty.Members[i];
            if (member == null || member.IsDefeated || member.CurrentHp <= 0)
            {
                continue;
            }

            PrototypeBattleLaneRuleResolution resolution = BuildEnemyActionLaneResolution(monster, i, useSpecial);
            if (resolution.ReachabilityStateKey == "reachable" || resolution.ReachabilityStateKey == "lane_agnostic")
            {
                candidates.Add(i);
            }
        }

        return candidates;
    }

    private string BuildRpgOwnedEnemyIntentText(DungeonMonsterRuntimeData monster, int targetIndex, bool useSpecial)
    {
        if (monster == null)
        {
            return "Enemy intent.";
        }

        PrototypeBattleLaneRuleResolution laneResolution = BuildEnemyActionLaneResolution(monster, targetIndex, useSpecial);
        string targetName = GetPartyMemberDisplayName(targetIndex);
        string actionLabel = GetRpgOwnedEnemyActionLabel(monster, useSpecial);
        string roleLabel = GetMonsterRoleText(monster);
        if (monster.IsElite)
        {
            if (IsRpgOwnedPartyWideEliteSpecial(monster, useSpecial))
            {
                return monster.DisplayName + " (" + roleLabel + ") intends " + actionLabel + " on all living allies. " + laneResolution.RangeText + ".";
            }

            return useSpecial
                ? monster.DisplayName + " (" + roleLabel + ") intends " + actionLabel + " on " + targetName + ". " + laneResolution.ReachabilitySummaryText + " Focused execution incoming."
                : monster.DisplayName + " (" + roleLabel + ") intends " + actionLabel + " on " + targetName + ". " + laneResolution.ReachabilitySummaryText;
        }

        if (monster.EncounterRole == MonsterEncounterRole.Striker)
        {
            return monster.DisplayName + " (" + roleLabel + ") intends " + actionLabel + " on " + targetName + ". " + laneResolution.ReachabilitySummaryText;
        }

        return monster.TargetPattern == MonsterTargetPattern.LowestHpLiving
            ? monster.DisplayName + " (" + roleLabel + ") intends " + actionLabel + " on lowest HP target: " + targetName + ". " + laneResolution.ReachabilitySummaryText
            : monster.TargetPattern == MonsterTargetPattern.RandomLiving
                ? monster.DisplayName + " (" + roleLabel + ") intends " + actionLabel + " on random target: " + targetName + ". " + laneResolution.ReachabilitySummaryText
                : monster.DisplayName + " (" + roleLabel + ") intends " + actionLabel + " on " + targetName + ". " + laneResolution.ReachabilitySummaryText;
    }

    private bool TryQueueRpgOwnedEnemyIntent(int startDisplayIndex)
    {
        DungeonEncounterRuntimeData encounter = GetActiveEncounter();
        if (encounter == null || encounter.MonsterIds == null)
        {
            return false;
        }

        int firstIndex = Mathf.Max(0, startDisplayIndex);
        if (firstIndex >= encounter.MonsterIds.Length)
        {
            return false;
        }

        for (int displayIndex = firstIndex; displayIndex < encounter.MonsterIds.Length; displayIndex++)
        {
            DungeonMonsterRuntimeData monster = GetBattleMonsterAtDisplayIndex(displayIndex);
            if (monster == null || monster.IsDefeated || monster.CurrentHp <= 0)
            {
                continue;
            }

            bool useSpecial = ShouldRpgOwnedUseEliteSpecialAttack(monster);
            int targetIndex = GetRpgOwnedMonsterTargetPartyMemberIndex(monster, useSpecial);
            if (targetIndex < 0)
            {
                continue;
            }

            float telegraphDuration = useSpecial ? EnemyIntentTelegraphSeconds + 0.12f : EnemyIntentTelegraphSeconds;
            _enemyTurnMonsterCursor = displayIndex;
            _pendingEnemyTargetIndex = targetIndex;
            _pendingEnemyAttackPower = GetRpgOwnedEnemyActionPower(monster, useSpecial);
            _pendingEnemyActionLabel = GetRpgOwnedEnemyActionLabel(monster, useSpecial);
            _pendingEnemyUsedSpecialAttack = useSpecial;
            SetActiveBattleMonster(monster);
            _enemyIntentTelegraphActive = true;
            _enemyIntentResolveAtTime = Time.unscaledTime + telegraphDuration;

            for (int monsterIndex = 0; monsterIndex < _activeMonsters.Count; monsterIndex++)
            {
                if (_activeMonsters[monsterIndex] != null)
                {
                    _activeMonsters[monsterIndex].RuntimeState?.ClearIntent();
                }
            }

            RecordRpgOwnedEnemyTurnStartEvent(monster);
            _currentEnemyIntentSnapshot = BuildRpgOwnedEnemyIntentSnapshot(monster, targetIndex, useSpecial);
            _enemyIntentText = string.IsNullOrEmpty(_currentEnemyIntentSnapshot.PreviewText)
                ? BuildRpgOwnedEnemyIntentText(monster, targetIndex, useSpecial)
                : _currentEnemyIntentSnapshot.PreviewText;
            monster.RuntimeState?.SetIntent(
                _currentEnemyIntentSnapshot.IntentKey,
                _currentEnemyIntentSnapshot.TargetPatternKey,
                _enemyIntentText,
                _currentEnemyIntentSnapshot.PredictedValue,
                _currentEnemyIntentSnapshot.TargetId,
                _currentEnemyIntentSnapshot.RangeKey,
                _currentEnemyIntentSnapshot.LaneRuleKey,
                _currentEnemyIntentSnapshot.ThreatLaneKey,
                _currentEnemyIntentSnapshot.ThreatLaneLabel,
                _currentEnemyIntentSnapshot.RangeText,
                _currentEnemyIntentSnapshot.PredictedReachabilityText,
                _currentEnemyIntentSnapshot.TargetRuleText);
            RecordRpgOwnedBattleEvent(
                PrototypeBattleEventKeys.EnemyIntentShown,
                monster.MonsterId,
                _currentEnemyIntentSnapshot.TargetId,
                _currentEnemyIntentSnapshot.PredictedValue,
                _enemyIntentText,
                actionKey: _currentEnemyIntentSnapshot.ActionKey,
                phaseKey: "enemy_turn",
                actorName: monster.DisplayName,
                targetName: _currentEnemyIntentSnapshot.TargetName,
                shortText: _pendingEnemyActionLabel);
            AppendBattleLog(_enemyIntentText);
            SetBattleFeedbackText(_enemyIntentText);
            LockBattleInput(telegraphDuration);
            RefreshSelectionPrompt();
            RefreshDungeonPresentation();
            return true;
        }

        return false;
    }

    private void ExecuteRpgOwnedQueuedEnemyIntent()
    {
        if (_dungeonRunState != DungeonRunState.Battle || _battleState != BattleState.EnemyTurn || !_enemyIntentTelegraphActive || _activeBattleMonster == null)
        {
            return;
        }

        _enemyIntentTelegraphActive = false;
        ClearBattleInputLock();
        int targetIndex = _pendingEnemyTargetIndex;
        bool usePartyWideEliteSpecial = IsRpgOwnedPartyWideEliteSpecial(_activeBattleMonster, _pendingEnemyUsedSpecialAttack);
        if (!usePartyWideEliteSpecial && (_activeDungeonParty == null || _activeDungeonParty.Members == null || targetIndex < 0 || targetIndex >= _activeDungeonParty.Members.Length))
        {
            if (!TryQueueRpgOwnedEnemyIntent(_enemyTurnMonsterCursor + 1))
            {
                ResumeRpgOwnedPlayerTurnFromStartIndex(0);
            }

            return;
        }

        DungeonPartyMemberRuntimeData targetMember = (!usePartyWideEliteSpecial && _activeDungeonParty != null && _activeDungeonParty.Members != null && targetIndex >= 0 && targetIndex < _activeDungeonParty.Members.Length)
            ? _activeDungeonParty.Members[targetIndex]
            : null;
        if (!usePartyWideEliteSpecial && (targetMember == null || targetMember.IsDefeated || targetMember.CurrentHp <= 0))
        {
            if (!TryQueueRpgOwnedEnemyIntent(_enemyTurnMonsterCursor + 1))
            {
                ResumeRpgOwnedPlayerTurnFromStartIndex(0);
            }

            return;
        }

        int damage = Mathf.Max(1, _pendingEnemyAttackPower > 0 ? _pendingEnemyAttackPower : _activeBattleMonster.Attack);
        string actionLabel = string.IsNullOrEmpty(_pendingEnemyActionLabel) ? "Attack" : _pendingEnemyActionLabel;
        if (usePartyWideEliteSpecial)
        {
            int sweepDamage = Mathf.Max(1, damage - 4);
            int hitCount = 0;
            int totalDamage = 0;
            var livingAllyIndices = GetLivingAllies();
            for (int i = 0; i < livingAllyIndices.Count; i++)
            {
                int memberIndex = livingAllyIndices[i];
                int applied = ApplyRpgOwnedBattleDamageToPartyMember(_activeBattleMonster, memberIndex, sweepDamage, new Color(1f, 0.34f, 0.42f, 1f));
                if (applied > 0)
                {
                    totalDamage += applied;
                    hitCount += 1;
                }
            }

            AppendBattleLog(_activeBattleMonster.DisplayName + " used " + actionLabel + " on all living allies for " + totalDamage + " total damage.");
            SetBattleFeedbackText(_activeBattleMonster.DisplayName + " unleashed " + actionLabel + " on " + hitCount + " allies.");
            RecordRpgOwnedBattleEvent(_pendingEnemyUsedSpecialAttack ? PrototypeBattleEventKeys.SkillResolved : PrototypeBattleEventKeys.AttackResolved, _activeBattleMonster.MonsterId, string.Empty, totalDamage, _activeBattleMonster.DisplayName + " resolved " + actionLabel + " on all allies.", actionKey: _pendingEnemyUsedSpecialAttack ? "skill" : "attack", phaseKey: "enemy_turn", actorName: _activeBattleMonster.DisplayName, targetName: "All living allies", shortText: actionLabel);
            _activeBattleMonster.TurnsActed += 1;
        }
        else
        {
            PrototypeBattleLaneRuleResolution laneResolution = BuildEnemyActionLaneResolution(_activeBattleMonster, targetIndex, _pendingEnemyUsedSpecialAttack);
            DungeonPartyMemberRuntimeData intendedTargetMember = targetMember;
            int resolvedTargetIndex = ResolveGuardInterceptPartyTargetIndex(_activeBattleMonster, targetIndex, _pendingEnemyUsedSpecialAttack, laneResolution);
            if (resolvedTargetIndex != targetIndex)
            {
                DungeonPartyMemberRuntimeData interceptMember = GetPartyMemberAtIndex(resolvedTargetIndex);
                if (interceptMember != null && !interceptMember.IsDefeated && interceptMember.CurrentHp > 0 && intendedTargetMember != null)
                {
                    AppendBattleLog(interceptMember.DisplayName + " intercepted the hit for " + intendedTargetMember.DisplayName + ".");
                    RecordRpgOwnedBattleEvent(PrototypeBattleEventKeys.GuardInterceptTriggered, _activeBattleMonster.MonsterId, interceptMember.MemberId, 0, interceptMember.DisplayName + " intercepted the hit for " + intendedTargetMember.DisplayName + ".", actionKey: _pendingEnemyUsedSpecialAttack ? "skill" : "attack", phaseKey: "enemy_turn", actorName: _activeBattleMonster.DisplayName, targetName: interceptMember.DisplayName, shortText: "Guard intercept");
                    targetIndex = resolvedTargetIndex;
                    _pendingEnemyTargetIndex = resolvedTargetIndex;
                    targetMember = interceptMember;
                }
            }

            int appliedDamage = ApplyRpgOwnedBattleDamageToPartyMember(_activeBattleMonster, targetIndex, damage, new Color(1f, 0.40f, 0.35f, 1f));
            string resolvedTargetName = targetMember != null ? targetMember.DisplayName : "target";
            AppendBattleLog(_activeBattleMonster.DisplayName + " used " + actionLabel + " on " + resolvedTargetName + " for " + appliedDamage + " damage.");
            SetBattleFeedbackText(_activeBattleMonster.DisplayName + " used " + actionLabel + " on " + resolvedTargetName + ".");
            RecordRpgOwnedBattleEvent(_pendingEnemyUsedSpecialAttack ? PrototypeBattleEventKeys.SkillResolved : PrototypeBattleEventKeys.AttackResolved, _activeBattleMonster.MonsterId, targetMember != null ? targetMember.MemberId : string.Empty, appliedDamage, _activeBattleMonster.DisplayName + " resolved " + actionLabel + " on " + resolvedTargetName + ".", actionKey: _pendingEnemyUsedSpecialAttack ? "skill" : "attack", phaseKey: "enemy_turn", actorName: _activeBattleMonster.DisplayName, targetName: resolvedTargetName, shortText: actionLabel);
            _activeBattleMonster.TurnsActed += 1;
        }

        if (GetFirstLivingPartyMemberIndex() < 0)
        {
            FinishDungeonRun(RunResultState.Defeat, BattleState.Defeat, false, 0, ActiveDungeonPartyText + " was defeated in " + _currentDungeonName + ".");
            return;
        }

        if (TryQueueRpgOwnedEnemyIntent(_enemyTurnMonsterCursor + 1))
        {
            return;
        }

        ResumeRpgOwnedPlayerTurnFromStartIndex(0);
    }

    private void RecordRpgOwnedCurrentPartyTurnStartEvent()
    {
        DungeonPartyMemberRuntimeData member = GetCurrentActorMember();
        if (member == null)
        {
            return;
        }

        RecordRpgOwnedBattleEvent(
            PrototypeBattleEventKeys.TurnStart,
            member.MemberId,
            string.Empty,
            Mathf.Max(0, _currentActorIndex),
            member.DisplayName + " turn started.",
            phaseKey: "party_turn",
            actorName: member.DisplayName,
            shortText: "Turn started");
    }

    private void ClearRpgOwnedEnemyIntentState()
    {
        _enemyIntentText = "None";
        _enemyTurnMonsterCursor = -1;
        _pendingEnemyTargetIndex = -1;
        _pendingEnemyAttackPower = 0;
        _pendingEnemyActionLabel = string.Empty;
        _pendingEnemyUsedSpecialAttack = false;
        _enemyIntentResolveAtTime = 0f;
        _enemyIntentTelegraphActive = false;
        _currentEnemyIntentSnapshot = new PrototypeEnemyIntentSnapshot();

        for (int i = 0; i < _activeMonsters.Count; i++)
        {
            if (_activeMonsters[i] != null)
            {
                _activeMonsters[i].RuntimeState?.ClearIntent();
            }
        }
    }

    private void ResetRpgOwnedRoundActions()
    {
        for (int i = 0; i < _partyActedThisRound.Length; i++)
        {
            _partyActedThisRound[i] = false;
        }
    }

    private bool TrySelectRpgOwnedNextPartyActor(int startIndex)
    {
        if (_activeDungeonParty == null || _activeDungeonParty.Members == null)
        {
            _currentActorIndex = -1;
            return false;
        }

        int firstIndex = Mathf.Max(0, startIndex);
        for (int i = firstIndex; i < _activeDungeonParty.Members.Length; i++)
        {
            DungeonPartyMemberRuntimeData member = _activeDungeonParty.Members[i];
            if (member == null || member.IsDefeated || member.CurrentHp <= 0 || _partyActedThisRound[i])
            {
                continue;
            }

            _currentActorIndex = i;
            _selectedPartyMemberIndex = i;
            return true;
        }

        _currentActorIndex = -1;
        _selectedPartyMemberIndex = -1;
        return false;
    }

    private void AdvanceRpgOwnedBattleAfterPartyAction()
    {
        AddRunMemberContributionValue(_runMemberActionCount, _currentActorIndex, 1);
        _partyActedThisRound[_currentActorIndex] = true;
        _queuedBattleAction = BattleActionType.None;
        _hoverBattleAction = BattleActionType.None;
        ClearBattleHoverState();
        ClearBattleInputLock();

        if (GetLivingBattleMonsterCount() <= 0)
        {
            ResolveCurrentEncounterVictory();
            return;
        }

        int nextIndex = Mathf.Max(0, _currentActorIndex + 1);
        if (TrySelectRpgOwnedNextPartyActor(nextIndex))
        {
            _battleState = BattleState.PartyActionSelect;
            SetActiveBattleMonster(GetFirstLivingBattleMonster());
            RecordRpgOwnedCurrentPartyTurnStartEvent();
            RefreshSelectionPrompt();
            RefreshDungeonPresentation();
            return;
        }

        BeginRpgOwnedEnemyTurn();
    }

    private void ResumeRpgOwnedPlayerTurnFromStartIndex(int startIndex)
    {
        ResetRpgOwnedRoundActions();
        _battleState = BattleState.PartyActionSelect;
        TrySelectRpgOwnedNextPartyActor(Mathf.Max(0, startIndex));
        SetActiveBattleMonster(GetFirstLivingBattleMonster());
        ClearRpgOwnedEnemyIntentState();
        RecordRpgOwnedCurrentPartyTurnStartEvent();
        RefreshSelectionPrompt();
        RefreshDungeonPresentation();
    }

    private void BeginRpgOwnedEnemyTurn()
    {
        _battleState = BattleState.EnemyTurn;
        _currentActorIndex = -1;
        _selectedPartyMemberIndex = -1;
        _hoverBattleAction = BattleActionType.None;
        _queuedBattleAction = BattleActionType.None;
        ClearBattleHoverState();
        if (!TryQueueRpgOwnedEnemyIntent(0))
        {
            ResetRpgOwnedRoundActions();
            _battleState = BattleState.PartyActionSelect;
            TrySelectRpgOwnedNextPartyActor(0);
            SetActiveBattleMonster(GetFirstLivingBattleMonster());
            ClearRpgOwnedEnemyIntentState();
            RefreshSelectionPrompt();
            RefreshDungeonPresentation();
        }
    }

    private bool ShouldRpgOwnedUseEliteSpecialAttack(DungeonMonsterRuntimeData monster)
    {
        if (monster == null)
        {
            return false;
        }

        if (monster.IsElite)
        {
            return monster.TurnsActed > 0 && monster.TurnsActed % 2 == 1;
        }

        return monster.EncounterRole == MonsterEncounterRole.Striker && monster.TurnsActed % 2 == 0;
    }

    private bool IsRpgOwnedPartyWideEliteSpecial(DungeonMonsterRuntimeData monster, bool useSpecial)
    {
        return useSpecial && monster != null && monster.IsElite && monster.MonsterType == "Slime";
    }

    private int GetRpgOwnedEnemyActionPower(DungeonMonsterRuntimeData monster, bool useSpecial)
    {
        if (monster == null)
        {
            return 1;
        }

        return useSpecial ? Mathf.Max(1, monster.SpecialAttack) : Mathf.Max(1, monster.Attack);
    }

    private string GetRpgOwnedEnemyActionLabel(DungeonMonsterRuntimeData monster, bool useSpecial)
    {
        if (monster == null)
        {
            return "Attack";
        }

        return useSpecial ? monster.SpecialActionName : "Attack";
    }
}
