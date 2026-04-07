using System;

public sealed class PrototypeRpgEncounterWaveRuntimeState
{
    public string EncounterId = string.Empty;
    public string WaveId = string.Empty;
    public int WaveIndex;
    public int TotalWaves = 1;
    public int RemainingEnemyCount;
    public string TriggerReasonKey = string.Empty;
    public bool IsTransitionPending;
    public bool IsFinalWave;
    public bool ReinforcementSpawned;
    public int TotalEncounterRewardAmount;
    public string[] ActiveMonsterIds = Array.Empty<string>();
    public string[] PendingMonsterIds = Array.Empty<string>();
    public string SummaryText = string.Empty;
}

public sealed class PrototypeRpgEncounterPhaseRuntimeState
{
    public string EncounterId = string.Empty;
    public string PhaseId = string.Empty;
    public int PhaseIndex = 1;
    public int TotalPhases = 1;
    public int WaveIndex = 1;
    public int TurnInPhase;
    public int TotalTurnsElapsed;
    public int RemainingEnemyCount;
    public string TriggerReasonKey = string.Empty;
    public bool IsTransitionPending;
    public bool IsFinalPhase = true;
    public string PatternKey = string.Empty;
    public string PatternLabel = string.Empty;
    public string SummaryText = string.Empty;
}

public sealed class PrototypeRpgEncounterPatternRuntimeState
{
    public string EncounterId = string.Empty;
    public string PatternKey = string.Empty;
    public string PatternLabel = string.Empty;
    public string TargetPolicyKey = string.Empty;
    public string BiasSummaryText = string.Empty;
    public string SummaryText = string.Empty;
}
