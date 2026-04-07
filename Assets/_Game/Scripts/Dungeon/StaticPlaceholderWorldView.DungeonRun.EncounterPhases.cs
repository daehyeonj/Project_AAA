using System;
using System.Collections.Generic;
using UnityEngine;

public sealed partial class StaticPlaceholderWorldView
{
    private PrototypeRpgEncounterPhaseRuntimeState _currentEncounterPhaseRuntimeState = new PrototypeRpgEncounterPhaseRuntimeState();
    private PrototypeRpgEncounterWaveRuntimeState _currentEncounterWaveRuntimeState = new PrototypeRpgEncounterWaveRuntimeState();
    private PrototypeRpgEncounterPatternRuntimeState _currentEncounterPatternRuntimeState = new PrototypeRpgEncounterPatternRuntimeState();
    private string _lastResolvedEncounterPhaseSummaryText = string.Empty;
    private string _lastResolvedEncounterWaveSummaryText = string.Empty;
    private string _lastResolvedBossPatternSummaryText = string.Empty;

    public string CurrentEncounterPhaseSummaryText => _runResultState != RunResultState.None
        ? ResolveRpgEncounterOrchestrationSurfaceText(_lastResolvedEncounterPhaseSummaryText, BuildCurrentEncounterPhaseSummaryText())
        : ResolveRpgEncounterOrchestrationSurfaceText(BuildCurrentEncounterPhaseSummaryText(), _lastResolvedEncounterPhaseSummaryText);

    public string CurrentEncounterWaveSummaryText => _runResultState != RunResultState.None
        ? ResolveRpgEncounterOrchestrationSurfaceText(_lastResolvedEncounterWaveSummaryText, BuildCurrentEncounterWaveSummaryText())
        : ResolveRpgEncounterOrchestrationSurfaceText(BuildCurrentEncounterWaveSummaryText(), _lastResolvedEncounterWaveSummaryText);

    public string CurrentBossPatternSummaryText => _runResultState != RunResultState.None
        ? ResolveRpgEncounterOrchestrationSurfaceText(_lastResolvedBossPatternSummaryText, BuildCurrentBossPatternSummaryText())
        : ResolveRpgEncounterOrchestrationSurfaceText(BuildCurrentBossPatternSummaryText(), _lastResolvedBossPatternSummaryText);

    private string ResolveRpgEncounterOrchestrationSurfaceText(string liveText, string capturedText)
    {
        string resolved = !string.IsNullOrEmpty(liveText) ? liveText : capturedText;
        return string.IsNullOrEmpty(resolved) ? "None" : resolved;
    }

    private void ResetRpgEncounterPhaseRuntime(bool clearReadback)
    {
        _currentEncounterPhaseRuntimeState = new PrototypeRpgEncounterPhaseRuntimeState();
        _currentEncounterWaveRuntimeState = new PrototypeRpgEncounterWaveRuntimeState();
        _currentEncounterPatternRuntimeState = new PrototypeRpgEncounterPatternRuntimeState();

        if (clearReadback)
        {
            _lastResolvedEncounterPhaseSummaryText = string.Empty;
            _lastResolvedEncounterWaveSummaryText = string.Empty;
            _lastResolvedBossPatternSummaryText = string.Empty;
        }
    }

    private void CaptureRpgEncounterPhaseReadback()
    {
        _lastResolvedEncounterPhaseSummaryText = BuildCurrentEncounterPhaseSummaryText();
        _lastResolvedEncounterWaveSummaryText = BuildCurrentEncounterWaveSummaryText();
        _lastResolvedBossPatternSummaryText = BuildCurrentBossPatternSummaryText();
    }

    private void InitializeRpgEncounterPhaseRuntime(DungeonEncounterRuntimeData encounter)
    {
        if (encounter == null || string.IsNullOrEmpty(encounter.EncounterId))
        {
            ResetRpgEncounterPhaseRuntime(false);
            return;
        }

        if (_currentEncounterPhaseRuntimeState != null &&
            _currentEncounterPhaseRuntimeState.EncounterId == encounter.EncounterId &&
            _currentEncounterWaveRuntimeState != null &&
            _currentEncounterWaveRuntimeState.EncounterId == encounter.EncounterId)
        {
            RefreshRpgEncounterPhaseRuntime(encounter);
            return;
        }

        string[] plannedMonsterIds = encounter.MonsterIds ?? Array.Empty<string>();
        string[] startingMonsterIds = encounter.IsEliteEncounter || plannedMonsterIds.Length <= 1
            ? plannedMonsterIds
            : new[] { plannedMonsterIds[0] };
        string[] pendingMonsterIds = encounter.IsEliteEncounter || plannedMonsterIds.Length <= 1
            ? Array.Empty<string>()
            : BuildEncounterPendingMonsterIds(plannedMonsterIds, 1);

        _currentEncounterWaveRuntimeState = new PrototypeRpgEncounterWaveRuntimeState();
        _currentEncounterWaveRuntimeState.EncounterId = encounter.EncounterId;
        _currentEncounterWaveRuntimeState.WaveId = "wave_1";
        _currentEncounterWaveRuntimeState.WaveIndex = 1;
        _currentEncounterWaveRuntimeState.TotalWaves = encounter.IsEliteEncounter ? 1 : Mathf.Max(1, plannedMonsterIds.Length);
        _currentEncounterWaveRuntimeState.TriggerReasonKey = "encounter_start";
        _currentEncounterWaveRuntimeState.ActiveMonsterIds = CopyStringArray(startingMonsterIds);
        _currentEncounterWaveRuntimeState.PendingMonsterIds = CopyStringArray(pendingMonsterIds);
        _currentEncounterWaveRuntimeState.IsFinalWave = _currentEncounterWaveRuntimeState.PendingMonsterIds.Length <= 0;
        _currentEncounterWaveRuntimeState.TotalEncounterRewardAmount = CalculateEncounterRewardTotal(plannedMonsterIds);

        _currentEncounterPhaseRuntimeState = new PrototypeRpgEncounterPhaseRuntimeState();
        _currentEncounterPhaseRuntimeState.EncounterId = encounter.EncounterId;
        _currentEncounterPhaseRuntimeState.PhaseIndex = 1;
        _currentEncounterPhaseRuntimeState.TotalPhases = encounter.IsEliteEncounter ? 3 : _currentEncounterWaveRuntimeState.TotalWaves > 1 ? 2 : 1;
        _currentEncounterPhaseRuntimeState.WaveIndex = 1;
        _currentEncounterPhaseRuntimeState.TriggerReasonKey = "encounter_start";

        encounter.MonsterIds = CopyStringArray(_currentEncounterWaveRuntimeState.ActiveMonsterIds);
        ResetEncounterMonstersForBattle(encounter.MonsterIds);
        ApplyEncounterPatternState(encounter, "encounter_start");
        RefreshRpgEncounterPhaseRuntime(encounter);
    }

    private void RefreshRpgEncounterPhaseRuntime(DungeonEncounterRuntimeData encounter)
    {
        if (encounter == null || string.IsNullOrEmpty(encounter.EncounterId))
        {
            return;
        }

        if (_currentEncounterPhaseRuntimeState == null || _currentEncounterPhaseRuntimeState.EncounterId != encounter.EncounterId)
        {
            return;
        }

        int remainingEnemyCount = GetLivingBattleMonsterCount();
        _currentEncounterPhaseRuntimeState.RemainingEnemyCount = remainingEnemyCount;
        _currentEncounterWaveRuntimeState.RemainingEnemyCount = remainingEnemyCount;
        _currentEncounterWaveRuntimeState.IsFinalWave = _currentEncounterWaveRuntimeState.PendingMonsterIds == null || _currentEncounterWaveRuntimeState.PendingMonsterIds.Length <= 0;
        _currentEncounterWaveRuntimeState.SummaryText = BuildEncounterWaveSummaryText(_currentEncounterWaveRuntimeState);
        _currentEncounterPatternRuntimeState.SummaryText = BuildEncounterPatternSummaryText(encounter, _currentEncounterPatternRuntimeState);
        _currentEncounterPhaseRuntimeState.SummaryText = BuildEncounterPhaseSummaryText(encounter, _currentEncounterPhaseRuntimeState, _currentEncounterPatternRuntimeState);
    }

    private void AdvanceRpgEncounterPhaseTurn(DungeonEncounterRuntimeData encounter)
    {
        if (encounter == null)
        {
            return;
        }

        InitializeRpgEncounterPhaseRuntime(encounter);
        if (_currentEncounterPhaseRuntimeState == null || _currentEncounterPhaseRuntimeState.EncounterId != encounter.EncounterId)
        {
            return;
        }

        _currentEncounterPhaseRuntimeState.TotalTurnsElapsed += 1;
        _currentEncounterPhaseRuntimeState.TurnInPhase += 1;
        RefreshRpgEncounterPhaseRuntime(encounter);

        if (encounter.IsEliteEncounter)
        {
            ResolveEncounterPhaseTransition(encounter, "turn_threshold");
            return;
        }

        if (_currentEncounterWaveRuntimeState.PendingMonsterIds != null &&
            _currentEncounterWaveRuntimeState.PendingMonsterIds.Length > 0 &&
            _currentEncounterPhaseRuntimeState.TotalTurnsElapsed >= 2 &&
            !_currentEncounterWaveRuntimeState.ReinforcementSpawned)
        {
            TryAdvanceEncounterWave(encounter, "turn_threshold");
        }
    }

    private bool ResolveEncounterPhaseTransition(DungeonEncounterRuntimeData encounter, string triggerReasonKey)
    {
        if (encounter == null || !encounter.IsEliteEncounter)
        {
            return false;
        }

        InitializeRpgEncounterPhaseRuntime(encounter);
        DungeonMonsterRuntimeData eliteMonster = GetEliteMonster();
        if (eliteMonster == null || eliteMonster.IsDefeated || eliteMonster.CurrentHp <= 0)
        {
            return false;
        }

        int hpThresholdPercent = Mathf.RoundToInt((eliteMonster.CurrentHp / (float)Mathf.Max(1, eliteMonster.MaxHp)) * 100f);
        int desiredPhaseIndex = 1;
        if (hpThresholdPercent <= 35 || _currentEncounterPhaseRuntimeState.TotalTurnsElapsed >= 4)
        {
            desiredPhaseIndex = 3;
        }
        else if (hpThresholdPercent <= 70 || _currentEncounterPhaseRuntimeState.TotalTurnsElapsed >= 2)
        {
            desiredPhaseIndex = 2;
        }

        if (desiredPhaseIndex <= _currentEncounterPhaseRuntimeState.PhaseIndex)
        {
            return false;
        }

        _currentEncounterPhaseRuntimeState.PhaseIndex = desiredPhaseIndex;
        _currentEncounterPhaseRuntimeState.TurnInPhase = 0;
        _currentEncounterPhaseRuntimeState.TriggerReasonKey = string.IsNullOrEmpty(triggerReasonKey) ? "phase_shift" : triggerReasonKey;
        ApplyEncounterPatternState(encounter, _currentEncounterPhaseRuntimeState.TriggerReasonKey);
        RefreshRpgEncounterPhaseRuntime(encounter);

        string transitionText = encounter.DisplayName + " shifts to " + _currentEncounterPatternRuntimeState.PatternLabel + ".";
        AppendBattleLog(transitionText);
        SetBattleFeedbackText(transitionText);
        RecordBattleEvent(
            PrototypeBattleEventKeys.EncounterPhaseTransition,
            eliteMonster.MonsterId,
            encounter.EncounterId,
            desiredPhaseIndex,
            transitionText,
            actionKey: "phase_shift",
            phaseKey: "enemy_turn",
            actorName: eliteMonster.DisplayName,
            targetName: encounter.DisplayName,
            shortText: _currentEncounterPatternRuntimeState.PatternLabel);
        RefreshSelectionPrompt();
        RefreshDungeonPresentation();
        return true;
    }

    private bool TryAdvanceEncounterWave(DungeonEncounterRuntimeData encounter, string triggerReasonKey)
    {
        if (encounter == null)
        {
            return false;
        }

        InitializeRpgEncounterPhaseRuntime(encounter);
        if (_currentEncounterWaveRuntimeState == null ||
            _currentEncounterWaveRuntimeState.EncounterId != encounter.EncounterId ||
            _currentEncounterWaveRuntimeState.PendingMonsterIds == null ||
            _currentEncounterWaveRuntimeState.PendingMonsterIds.Length <= 0)
        {
            return false;
        }

        List<string> activeMonsterIds = new List<string>();
        if (encounter.MonsterIds != null)
        {
            for (int i = 0; i < encounter.MonsterIds.Length; i++)
            {
                DungeonMonsterRuntimeData livingMonster = GetMonsterById(encounter.MonsterIds[i]);
                if (livingMonster != null && !livingMonster.IsDefeated && livingMonster.CurrentHp > 0)
                {
                    activeMonsterIds.Add(livingMonster.MonsterId);
                }
            }
        }

        string reinforcementMonsterId = _currentEncounterWaveRuntimeState.PendingMonsterIds[0];
        DungeonMonsterRuntimeData reinforcementMonster = GetMonsterById(reinforcementMonsterId);
        if (reinforcementMonster != null)
        {
            reinforcementMonster.ResetForEncounter();
            activeMonsterIds.Add(reinforcementMonster.MonsterId);
        }

        encounter.MonsterIds = activeMonsterIds.ToArray();
        _currentEncounterWaveRuntimeState.ActiveMonsterIds = CopyStringArray(encounter.MonsterIds);
        _currentEncounterWaveRuntimeState.PendingMonsterIds = BuildEncounterPendingMonsterIds(_currentEncounterWaveRuntimeState.PendingMonsterIds, 1);
        _currentEncounterWaveRuntimeState.WaveIndex = Mathf.Min(_currentEncounterWaveRuntimeState.TotalWaves, Mathf.Max(_currentEncounterWaveRuntimeState.WaveIndex + 1, 2));
        _currentEncounterWaveRuntimeState.WaveId = "wave_" + _currentEncounterWaveRuntimeState.WaveIndex;
        _currentEncounterWaveRuntimeState.TriggerReasonKey = string.IsNullOrEmpty(triggerReasonKey) ? "wave_shift" : triggerReasonKey;
        _currentEncounterWaveRuntimeState.ReinforcementSpawned = true;
        _currentEncounterWaveRuntimeState.IsTransitionPending = false;

        _currentEncounterPhaseRuntimeState.PhaseIndex = Mathf.Min(_currentEncounterPhaseRuntimeState.TotalPhases, Mathf.Max(_currentEncounterPhaseRuntimeState.PhaseIndex + 1, 2));
        _currentEncounterPhaseRuntimeState.WaveIndex = _currentEncounterWaveRuntimeState.WaveIndex;
        _currentEncounterPhaseRuntimeState.TurnInPhase = 0;
        _currentEncounterPhaseRuntimeState.TriggerReasonKey = _currentEncounterWaveRuntimeState.TriggerReasonKey;

        ApplyEncounterPatternState(encounter, _currentEncounterPhaseRuntimeState.TriggerReasonKey);
        RefreshRpgEncounterPhaseRuntime(encounter);

        string reinforcementText = encounter.DisplayName + " opens " + _currentEncounterWaveRuntimeState.WaveId.Replace('_', ' ') + ": " + (reinforcementMonster != null ? reinforcementMonster.DisplayName : "reinforcements") + " joins the fight.";
        AppendBattleLog(reinforcementText);
        SetBattleFeedbackText(reinforcementText);
        RecordBattleEvent(
            PrototypeBattleEventKeys.EncounterWaveAdvanced,
            reinforcementMonster != null ? reinforcementMonster.MonsterId : string.Empty,
            encounter.EncounterId,
            _currentEncounterWaveRuntimeState.WaveIndex,
            reinforcementText,
            actionKey: "reinforce",
            phaseKey: "enemy_turn",
            actorName: reinforcementMonster != null ? reinforcementMonster.DisplayName : encounter.DisplayName,
            targetName: encounter.DisplayName,
            shortText: "Wave " + _currentEncounterWaveRuntimeState.WaveIndex);
        SetActiveBattleMonster(GetFirstLivingBattleMonster());
        RefreshSelectionPrompt();
        RefreshDungeonPresentation();
        return true;
    }

    private void ApplyEncounterPatternState(DungeonEncounterRuntimeData encounter, string triggerReasonKey)
    {
        if (encounter == null)
        {
            return;
        }

        _currentEncounterPatternRuntimeState = new PrototypeRpgEncounterPatternRuntimeState();
        _currentEncounterPatternRuntimeState.EncounterId = encounter.EncounterId;
        if (encounter.IsEliteEncounter)
        {
            if (_currentEncounterPhaseRuntimeState.PhaseIndex >= 3)
            {
                _currentEncounterPhaseRuntimeState.PhaseId = "boss_phase_pressure";
                _currentEncounterPatternRuntimeState.PatternKey = "pressure_wave";
                _currentEncounterPatternRuntimeState.PatternLabel = "Phase 3: Pressure Wave";
                _currentEncounterPatternRuntimeState.TargetPolicyKey = "random";
                _currentEncounterPatternRuntimeState.BiasSummaryText = "AOE pressure and status spread";
            }
            else if (_currentEncounterPhaseRuntimeState.PhaseIndex == 2)
            {
                _currentEncounterPhaseRuntimeState.PhaseId = "boss_phase_mark_burst";
                _currentEncounterPatternRuntimeState.PatternKey = "mark_burst";
                _currentEncounterPatternRuntimeState.PatternLabel = "Phase 2: Mark and Burst";
                _currentEncounterPatternRuntimeState.TargetPolicyKey = "lowest_hp";
                _currentEncounterPatternRuntimeState.BiasSummaryText = "Debuff into focused execution";
            }
            else
            {
                _currentEncounterPhaseRuntimeState.PhaseId = "boss_phase_opening";
                _currentEncounterPatternRuntimeState.PatternKey = "focused_strike";
                _currentEncounterPatternRuntimeState.PatternLabel = "Phase 1: Focused Strike";
                _currentEncounterPatternRuntimeState.TargetPolicyKey = "first_living";
                _currentEncounterPatternRuntimeState.BiasSummaryText = "Single-target pressure opener";
            }
        }
        else
        {
            if (_currentEncounterWaveRuntimeState.PendingMonsterIds != null &&
                _currentEncounterWaveRuntimeState.PendingMonsterIds.Length > 0 &&
                !_currentEncounterWaveRuntimeState.ReinforcementSpawned)
            {
                _currentEncounterPhaseRuntimeState.PhaseId = "skirmish_phase_opening";
                _currentEncounterPatternRuntimeState.PatternKey = "opening_screen";
                _currentEncounterPatternRuntimeState.PatternLabel = "Opening Screen";
                _currentEncounterPatternRuntimeState.TargetPolicyKey = "default";
                _currentEncounterPatternRuntimeState.BiasSummaryText = "Frontline probe before reinforcements";
            }
            else
            {
                _currentEncounterPhaseRuntimeState.PhaseId = "skirmish_phase_reinforcement";
                _currentEncounterPatternRuntimeState.PatternKey = "reinforcement_push";
                _currentEncounterPatternRuntimeState.PatternLabel = "Reinforcement Push";
                _currentEncounterPatternRuntimeState.TargetPolicyKey = "random";
                _currentEncounterPatternRuntimeState.BiasSummaryText = "Wave chain pressure and flank focus";
            }
        }

        _currentEncounterPhaseRuntimeState.PatternKey = _currentEncounterPatternRuntimeState.PatternKey;
        _currentEncounterPhaseRuntimeState.PatternLabel = _currentEncounterPatternRuntimeState.PatternLabel;
        _currentEncounterPhaseRuntimeState.IsFinalPhase = _currentEncounterPhaseRuntimeState.PhaseIndex >= _currentEncounterPhaseRuntimeState.TotalPhases;
        _currentEncounterPhaseRuntimeState.TriggerReasonKey = string.IsNullOrEmpty(triggerReasonKey) ? "encounter_start" : triggerReasonKey;
        _currentEncounterPatternRuntimeState.SummaryText = BuildEncounterPatternSummaryText(encounter, _currentEncounterPatternRuntimeState);
    }

    private string BuildCurrentEncounterPhaseSummaryText()
    {
        return _currentEncounterPhaseRuntimeState != null &&
               !string.IsNullOrEmpty(_currentEncounterPhaseRuntimeState.EncounterId) &&
               !string.IsNullOrEmpty(_currentEncounterPhaseRuntimeState.SummaryText)
            ? _currentEncounterPhaseRuntimeState.SummaryText
            : string.Empty;
    }

    private string BuildCurrentEncounterWaveSummaryText()
    {
        return _currentEncounterWaveRuntimeState != null &&
               !string.IsNullOrEmpty(_currentEncounterWaveRuntimeState.EncounterId) &&
               !string.IsNullOrEmpty(_currentEncounterWaveRuntimeState.SummaryText)
            ? _currentEncounterWaveRuntimeState.SummaryText
            : string.Empty;
    }

    private string BuildCurrentBossPatternSummaryText()
    {
        return _currentEncounterPatternRuntimeState != null &&
               !string.IsNullOrEmpty(_currentEncounterPatternRuntimeState.EncounterId) &&
               !string.IsNullOrEmpty(_currentEncounterPatternRuntimeState.SummaryText)
            ? _currentEncounterPatternRuntimeState.SummaryText
            : string.Empty;
    }

    private string BuildEncounterPhaseSummaryText(DungeonEncounterRuntimeData encounter, PrototypeRpgEncounterPhaseRuntimeState phaseState, PrototypeRpgEncounterPatternRuntimeState patternState)
    {
        if (phaseState == null || encounter == null)
        {
            return string.Empty;
        }

        string phaseLabel = (encounter.IsEliteEncounter ? "Boss Phase " : "Phase ") + phaseState.PhaseIndex + "/" + Mathf.Max(1, phaseState.TotalPhases);
        string turnLabel = phaseState.TurnInPhase <= 0 ? "Setup ready" : "Turn " + Mathf.Max(0, phaseState.TurnInPhase);
        string patternLabel = patternState != null && !string.IsNullOrEmpty(patternState.PatternLabel) ? patternState.PatternLabel : phaseState.PatternLabel;
        string shiftHint = BuildEncounterNextShiftSummaryText(encounter, phaseState);
        return BuildRpgCompactSummaryText(phaseLabel, turnLabel, patternLabel, shiftHint);
    }

    private string BuildEncounterWaveSummaryText(PrototypeRpgEncounterWaveRuntimeState waveState)
    {
        if (waveState == null || string.IsNullOrEmpty(waveState.EncounterId))
        {
            return string.Empty;
        }

        string waveLabel = "Wave " + waveState.WaveIndex + "/" + Mathf.Max(1, waveState.TotalWaves);
        string activeLabel = waveState.RemainingEnemyCount == 1
            ? "1 enemy active"
            : waveState.RemainingEnemyCount + " enemies active";
        string pendingLabel = BuildEncounterWaveTransitionSummaryText(waveState);
        return BuildRpgCompactSummaryText(waveLabel, activeLabel, pendingLabel);
    }

    private string BuildEncounterPatternSummaryText(DungeonEncounterRuntimeData encounter, PrototypeRpgEncounterPatternRuntimeState patternState)
    {
        if (patternState == null || encounter == null)
        {
            return string.Empty;
        }

        string labelPrefix = encounter.IsEliteEncounter ? "Boss Pattern" : "Encounter Pattern";
        string targetPolicyText = ResolveEncounterPatternTargetPolicyLabel(patternState.TargetPolicyKey, encounter.IsEliteEncounter);
        return BuildRpgCompactSummaryText(labelPrefix + ": " + patternState.PatternLabel, targetPolicyText, patternState.BiasSummaryText);
    }

    private string BuildEncounterNextShiftSummaryText(DungeonEncounterRuntimeData encounter, PrototypeRpgEncounterPhaseRuntimeState phaseState)
    {
        if (encounter == null || phaseState == null)
        {
            return string.Empty;
        }

        if (encounter.IsEliteEncounter)
        {
            if (phaseState.PhaseIndex >= Mathf.Max(1, phaseState.TotalPhases))
            {
                return "Final boss phase armed";
            }

            return phaseState.PhaseIndex <= 1
                ? "Next shift at 70% HP or turn 2"
                : "Next shift at 35% HP or turn 4";
        }

        if (_currentEncounterWaveRuntimeState != null &&
            _currentEncounterWaveRuntimeState.PendingMonsterIds != null &&
            _currentEncounterWaveRuntimeState.PendingMonsterIds.Length > 0)
        {
            return _currentEncounterWaveRuntimeState.ReinforcementSpawned
                ? "Reserve wave waits for a clear"
                : "Reinforce on turn 2 or after a clear";
        }

        return "Encounter chain stabilized";
    }

    private string BuildEncounterWaveTransitionSummaryText(PrototypeRpgEncounterWaveRuntimeState waveState)
    {
        if (waveState == null)
        {
            return string.Empty;
        }

        if (waveState.PendingMonsterIds != null && waveState.PendingMonsterIds.Length > 0)
        {
            string nextMonsterName = ResolveEncounterWaveMonsterDisplayName(waveState.PendingMonsterIds);
            string triggerText = waveState.ReinforcementSpawned
                ? "after this wave clears"
                : "on turn 2 or after a clear";
            return string.IsNullOrEmpty(nextMonsterName)
                ? "Next reinforce " + triggerText
                : "Next: " + nextMonsterName + " " + triggerText;
        }

        return waveState.IsFinalWave ? "Final wave" : "Wave stable";
    }

    private string ResolveEncounterWaveMonsterDisplayName(string[] monsterIds)
    {
        if (monsterIds == null || monsterIds.Length <= 0)
        {
            return string.Empty;
        }

        for (int i = 0; i < monsterIds.Length; i++)
        {
            DungeonMonsterRuntimeData monster = GetMonsterById(monsterIds[i]);
            if (monster != null && !string.IsNullOrEmpty(monster.DisplayName))
            {
                return monster.DisplayName;
            }
        }

        return string.Empty;
    }

    private string ResolveEncounterPatternTargetPolicyLabel(string targetPolicyKey, bool isEliteEncounter)
    {
        switch (targetPolicyKey)
        {
            case "lowest_hp":
                return isEliteEncounter ? "Executes the weakest ally" : "Focuses the weakest ally";
            case "random":
                return isEliteEncounter ? "Pressure spreads across the party" : "Pressure shifts across the line";
            case "first_living":
                return "Leads with frontline focus";
            default:
                return "Flexible target pressure";
        }
    }

    private string BuildBattleUiEncounterPatternHintText(DungeonMonsterRuntimeData monster)
    {
        if (monster == null || !IsMonsterUsingCurrentEncounterPattern(monster))
        {
            return string.Empty;
        }

        return BuildCurrentBossPatternSummaryText();
    }

    private string BuildEncounterPatternIntentSuffix(DungeonMonsterRuntimeData monster)
    {
        string patternHint = BuildBattleUiEncounterPatternHintText(monster);
        return string.IsNullOrEmpty(patternHint) ? string.Empty : patternHint;
    }

    private int ScoreEncounterPatternBias(DungeonMonsterRuntimeData monster, PrototypeRpgEnemySkillRuntimeState skill)
    {
        if (monster == null || skill == null || !IsMonsterUsingCurrentEncounterPattern(monster))
        {
            return 0;
        }

        switch (_currentEncounterPatternRuntimeState.PatternKey)
        {
            case "focused_strike":
                return skill.ActionBiasKey == "finisher" ? 20 : skill.ActionBiasKey == "basic_attack" ? 8 : skill.ActionBiasKey == "aoe" ? -8 : 0;
            case "mark_burst":
                return skill.ActionBiasKey == "debuff" ? 18 : skill.ActionBiasKey == "finisher" ? 12 : skill.ActionBiasKey == "self_buff" ? -6 : 0;
            case "pressure_wave":
                return skill.ActionBiasKey == "aoe" ? 22 : skill.ActionBiasKey == "debuff" ? 8 : skill.ActionBiasKey == "basic_attack" ? -4 : 0;
            case "reinforcement_push":
                return skill.ActionBiasKey == "aoe" ? 10 : skill.ActionBiasKey == "debuff" ? 10 : skill.ActionBiasKey == "self_buff" ? -4 : 0;
            case "opening_screen":
                return skill.ActionBiasKey == "self_buff" ? 8 : skill.ActionBiasKey == "basic_attack" ? 6 : 0;
            default:
                return 0;
        }
    }

    private int ResolveEncounterPatternTargetIndex(DungeonMonsterRuntimeData monster)
    {
        if (monster == null || !IsMonsterUsingCurrentEncounterPattern(monster))
        {
            return -1;
        }

        switch (_currentEncounterPatternRuntimeState.TargetPolicyKey)
        {
            case "lowest_hp":
                return GetLowestHpLivingPartyMemberIndex();
            case "random":
                return GetRandomLivingPartyMemberIndex();
            case "first_living":
                return GetFirstLivingPartyMemberIndex();
            default:
                return -1;
        }
    }

    private bool IsMonsterUsingCurrentEncounterPattern(DungeonMonsterRuntimeData monster)
    {
        return monster != null &&
               _currentEncounterPatternRuntimeState != null &&
               !string.IsNullOrEmpty(_currentEncounterPatternRuntimeState.EncounterId) &&
               _currentEncounterPatternRuntimeState.EncounterId == monster.EncounterId;
    }

    private void ResetEncounterMonstersForBattle(string[] monsterIds)
    {
        if (monsterIds == null)
        {
            return;
        }

        for (int i = 0; i < monsterIds.Length; i++)
        {
            DungeonMonsterRuntimeData monster = GetMonsterById(monsterIds[i]);
            if (monster != null)
            {
                monster.ResetForEncounter();
            }
        }
    }

    private string[] BuildEncounterPendingMonsterIds(string[] monsterIds, int skipCount)
    {
        if (monsterIds == null || monsterIds.Length <= skipCount)
        {
            return Array.Empty<string>();
        }

        int length = monsterIds.Length - skipCount;
        string[] pendingMonsterIds = new string[length];
        for (int i = 0; i < length; i++)
        {
            pendingMonsterIds[i] = monsterIds[i + skipCount] ?? string.Empty;
        }

        return pendingMonsterIds;
    }

    private string[] CopyStringArray(string[] source)
    {
        if (source == null || source.Length <= 0)
        {
            return Array.Empty<string>();
        }

        string[] copy = new string[source.Length];
        for (int i = 0; i < source.Length; i++)
        {
            copy[i] = source[i] ?? string.Empty;
        }

        return copy;
    }

    private int CalculateEncounterRewardTotal(string[] monsterIds)
    {
        if (monsterIds == null || monsterIds.Length <= 0)
        {
            return 0;
        }

        int total = 0;
        for (int i = 0; i < monsterIds.Length; i++)
        {
            DungeonMonsterRuntimeData monster = GetMonsterById(monsterIds[i]);
            if (monster != null)
            {
                total += monster.RewardAmount;
            }
        }

        return total;
    }
}

