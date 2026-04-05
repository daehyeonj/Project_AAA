using System;

public sealed class PrototypeRpgEncounterRuntimeState
{
    public string DefinitionId { get; }
    public string EncounterId { get; }
    public int RoomIndex { get; }
    public string DisplayName { get; }
    public string EncounterTypeLabel { get; }
    public string RoomTypeLabel { get; }
    public string EliteStyleLabel { get; }
    public string RouteRiskLabel { get; }
    public string DangerHintLabel { get; }
    public string RewardPreviewHint { get; }
    public string RewardLabel { get; }
    public int RewardAmountHint { get; }
    public string[] EnemyIds { get; private set; }
    public bool IsEliteEncounter { get; }
    public bool IsCleared { get; private set; }
    public string SelectedEnemyId { get; private set; }
    public string FocusedEnemyId { get; private set; }
    public string ActingEnemyId { get; private set; }
    public string IntentSummaryText { get; private set; }
    public int LivingEnemyCount { get; private set; }
    public int ClearedEnemyCount { get; private set; }

    public PrototypeRpgEncounterRuntimeState()
        : this(string.Empty, string.Empty, 0, "Encounter", "Skirmish", "Skirmish Room", string.Empty, string.Empty, string.Empty, string.Empty, string.Empty, 0, Array.Empty<string>(), false)
    {
    }

    public PrototypeRpgEncounterRuntimeState(
        string definitionId,
        string encounterId,
        int roomIndex,
        string displayName,
        string encounterTypeLabel,
        string roomTypeLabel,
        string eliteStyleLabel,
        string routeRiskLabel,
        string dangerHintLabel,
        string rewardPreviewHint,
        string rewardLabel,
        int rewardAmountHint,
        string[] enemyIds,
        bool isEliteEncounter)
    {
        DefinitionId = string.IsNullOrWhiteSpace(definitionId) ? string.Empty : definitionId.Trim();
        EncounterId = string.IsNullOrWhiteSpace(encounterId) ? string.Empty : encounterId.Trim();
        RoomIndex = roomIndex;
        DisplayName = string.IsNullOrWhiteSpace(displayName) ? "Encounter" : displayName.Trim();
        EncounterTypeLabel = string.IsNullOrWhiteSpace(encounterTypeLabel) ? "Skirmish" : encounterTypeLabel.Trim();
        RoomTypeLabel = string.IsNullOrWhiteSpace(roomTypeLabel) ? "Skirmish Room" : roomTypeLabel.Trim();
        EliteStyleLabel = string.IsNullOrWhiteSpace(eliteStyleLabel) ? string.Empty : eliteStyleLabel.Trim();
        RouteRiskLabel = string.IsNullOrWhiteSpace(routeRiskLabel) ? string.Empty : routeRiskLabel.Trim();
        DangerHintLabel = string.IsNullOrWhiteSpace(dangerHintLabel) ? string.Empty : dangerHintLabel.Trim();
        RewardPreviewHint = string.IsNullOrWhiteSpace(rewardPreviewHint) ? string.Empty : rewardPreviewHint.Trim();
        RewardLabel = string.IsNullOrWhiteSpace(rewardLabel) ? string.Empty : rewardLabel.Trim();
        RewardAmountHint = rewardAmountHint > 0 ? rewardAmountHint : 0;
        EnemyIds = enemyIds != null && enemyIds.Length > 0 ? (string[])enemyIds.Clone() : Array.Empty<string>();
        IsEliteEncounter = isEliteEncounter;
        ResetForRun();
    }

    public void ResetForRun()
    {
        IsCleared = false;
        SelectedEnemyId = string.Empty;
        FocusedEnemyId = string.Empty;
        ActingEnemyId = string.Empty;
        IntentSummaryText = string.Empty;
        LivingEnemyCount = EnemyIds != null ? EnemyIds.Length : 0;
        ClearedEnemyCount = 0;
    }

    public void SetCleared(bool isCleared)
    {
        IsCleared = isCleared;
    }

    public void UpdateCombatViewState(string selectedEnemyId, string focusedEnemyId, string actingEnemyId, string intentSummaryText, int livingEnemyCount, int clearedEnemyCount)
    {
        SelectedEnemyId = string.IsNullOrWhiteSpace(selectedEnemyId) ? string.Empty : selectedEnemyId.Trim();
        FocusedEnemyId = string.IsNullOrWhiteSpace(focusedEnemyId) ? string.Empty : focusedEnemyId.Trim();
        ActingEnemyId = string.IsNullOrWhiteSpace(actingEnemyId) ? string.Empty : actingEnemyId.Trim();
        IntentSummaryText = string.IsNullOrWhiteSpace(intentSummaryText) ? string.Empty : intentSummaryText.Trim();
        LivingEnemyCount = livingEnemyCount >= 0 ? livingEnemyCount : 0;
        ClearedEnemyCount = clearedEnemyCount >= 0 ? clearedEnemyCount : 0;
    }
}
