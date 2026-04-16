using System.Collections.Generic;
using UnityEngine;

public sealed partial class StaticPlaceholderWorldView
{
    private BattleResult BuildRpgOwnedCurrentBattleResultContract()
    {
        string outcomeKey = _currentBattleResultSnapshot != null && !string.IsNullOrEmpty(_currentBattleResultSnapshot.OutcomeKey)
            ? _currentBattleResultSnapshot.OutcomeKey
            : PrototypeBattleOutcomeKeys.None;
        return BuildRpgOwnedBattleResultContract(outcomeKey);
    }

    private BattleResult BuildRpgOwnedBattleResultContract(string outcomeKey)
    {
        PrototypeBattleContextData context = BuildRpgOwnedBattleContextView();
        PrototypeBattleResultData coreResult = BuildRpgOwnedBattleCoreResultData(outcomeKey);
        PrototypeBattleResultSnapshot snapshot = BuildRpgOwnedBattleResultSnapshot(outcomeKey);
        BattleResult result = new BattleResult();
        result.OutcomeKey = snapshot.OutcomeKey;
        result.ResultStateKey = snapshot.ResultStateKey;
        result.BattleId = context.BattleId;
        result.EncounterId = !string.IsNullOrEmpty(snapshot.EncounterId) ? snapshot.EncounterId : context.EncounterId;
        result.EncounterName = !string.IsNullOrEmpty(snapshot.EncounterName) ? snapshot.EncounterName : GetCurrentEncounterNameText();
        result.EliteEncounterName = !string.IsNullOrEmpty(snapshot.EliteEncounterName) ? snapshot.EliteEncounterName : "None";
        result.EliteRewardLabel = !string.IsNullOrEmpty(snapshot.EliteRewardLabel) ? snapshot.EliteRewardLabel : "None";
        result.DungeonId = context.DungeonId;
        result.DungeonLabel = context.DungeonLabel;
        result.RouteId = context.RouteId;
        result.RouteLabel = context.RouteLabel;
        result.TurnCount = Mathf.Max(snapshot.TurnsTaken, coreResult.TurnCount);
        result.SurvivingMemberCount = Mathf.Max(0, snapshot.SurvivingMemberCount);
        result.KnockedOutMemberCount = Mathf.Max(0, snapshot.KnockedOutMemberCount);
        result.DefeatedEnemyCount = Mathf.Max(0, snapshot.DefeatedEnemyCount);
        result.EliteDefeated = snapshot.EliteDefeated;
        result.TotalDamageDealt = Mathf.Max(0, snapshot.TotalDamageDealt);
        result.TotalDamageTaken = Mathf.Max(0, snapshot.TotalDamageTaken);
        result.TotalHealingDone = Mathf.Max(0, snapshot.TotalHealingDone);
        result.ResultSummaryText = coreResult.ResultSummaryText;
        result.RuntimeSummaryText = coreResult.RuntimeSummaryText;
        result.PartyAftermathSummaryText = coreResult.PartyAftermathSummary;
        result.ResourceDeltaSummaryText = coreResult.ResourceDeltaSummary;
        result.StatusSummaryText = coreResult.StatusInjurySummary;
        result.ConsumableUseSummaryText = coreResult.ConsumableUseSummary;
        result.LootRewardSummaryText = coreResult.LootRewardSummary;
        result.NotableEventsSummaryText = coreResult.NotableEventsSummary;
        result.PartyConditionAfterBattleText = coreResult.PartyConditionAfterBattle;
        result.BattleContextSummaryText = !string.IsNullOrEmpty(snapshot.BattleContextSummary) ? snapshot.BattleContextSummary : context.ContextSummaryText;
        result.LaneRuleSummaryText = !string.IsNullOrEmpty(snapshot.LaneRuleSummary) ? snapshot.LaneRuleSummary : BuildRpgOwnedBattleRuleSummaryText();
        result.AbsorbSummaryText = coreResult.AbsorbSummaryText;
        return result;
    }

    private PrototypeBattleResultData BuildRpgOwnedCurrentBattleCoreResultView()
    {
        string outcomeKey = _currentBattleResultSnapshot != null && !string.IsNullOrEmpty(_currentBattleResultSnapshot.OutcomeKey)
            ? _currentBattleResultSnapshot.OutcomeKey
            : PrototypeBattleOutcomeKeys.None;
        return BuildRpgOwnedBattleCoreResultData(outcomeKey);
    }

    private PrototypeBattleResultData BuildRpgOwnedBattleCoreResultData(string outcomeKey)
    {
        PrototypeBattleContextData context = BuildRpgOwnedBattleContextView();
        PrototypeBattleRuntimeState runtimeState = CreateRpgOwnedBattleRuntimeStateView();
        PrototypeBattleResultData result = new PrototypeBattleResultData();
        result.OutcomeKey = string.IsNullOrEmpty(outcomeKey) ? PrototypeBattleOutcomeKeys.None : outcomeKey;
        result.BattleId = context.BattleId;
        result.EncounterId = context.EncounterId;
        result.ResultSummaryText = BuildRpgOwnedBattleOutcomeSummaryText(result.OutcomeKey);
        result.RuntimeSummaryText = BuildRpgOwnedBattleRuntimeSummaryText(runtimeState);
        result.TurnCount = Mathf.Max(_runTurnCount, _battleTurnIndex);
        result.PartyAftermathSummary = BuildPartyMembersAtEndSummary();
        result.ResourceDeltaSummary = "Damage " + _totalDamageDealt + " dealt | " + _totalDamageTaken + " taken | Healing " + _totalHealingDone + ".";
        result.StatusInjurySummary = string.IsNullOrEmpty(GetPartyConditionText()) ? "No injury summary." : GetPartyConditionText();
        result.ConsumableUseSummary = "Consumable runtime is not wired in this checkout.";
        result.LootRewardSummary = BuildRpgOwnedBattleLootRewardSummaryText();
        result.NotableEventsSummary = BuildRpgOwnedNotableBattleEventsSummary(3);
        result.PartyConditionAfterBattle = GetPartyConditionText();
        result.AbsorbSummaryText = BuildRpgOwnedBattleAbsorbSummaryText(
            result.OutcomeKey,
            GetCurrentEncounterNameText(),
            result.TurnCount,
            GetLivingPartyMemberCount(),
            GetKnockedOutMemberCount(),
            GetDefeatedEnemyCount());
        return result;
    }

    private string BuildRpgOwnedBattleOutcomeSummaryText(string outcomeKey)
    {
        if (outcomeKey == PrototypeBattleOutcomeKeys.RunClear || outcomeKey == PrototypeBattleOutcomeKeys.EncounterVictory)
        {
            return "Victory | " + BuildRpgOwnedBattleRuleSummaryText();
        }

        if (outcomeKey == PrototypeBattleOutcomeKeys.RunDefeat)
        {
            return "Defeat | " + BuildRpgOwnedBattleRuleSummaryText();
        }

        if (outcomeKey == PrototypeBattleOutcomeKeys.RunRetreat)
        {
            return "Retreat | " + BuildRpgOwnedBattleRuleSummaryText();
        }

        return "Battle active | " + BuildRpgOwnedBattleRuleSummaryText();
    }

    private string BuildRpgOwnedBattleRuleSummaryText()
    {
        return "Standard JRPG battle: turn-based command selection, direct target resolution, role skills, and retreat remain available.";
    }

    private string BuildRpgOwnedNotableBattleEventsSummary(int count)
    {
        if (_battleEventRecords == null || _battleEventRecords.Count <= 0)
        {
            return "No notable events.";
        }

        int desiredCount = Mathf.Max(1, count);
        List<string> notable = new List<string>(desiredCount);
        for (int i = _battleEventRecords.Count - 1; i >= 0 && notable.Count < desiredCount; i--)
        {
            PrototypeBattleEventRecord record = _battleEventRecords[i];
            if (!IsLaneNotableBattleEvent(record))
            {
                continue;
            }

            string text = !string.IsNullOrEmpty(record.ShortText) ? record.ShortText : record.Summary;
            if (!string.IsNullOrEmpty(text))
            {
                notable.Insert(0, text);
            }
        }

        for (int i = _battleEventRecords.Count - 1; i >= 0 && notable.Count < desiredCount; i--)
        {
            PrototypeBattleEventRecord record = _battleEventRecords[i];
            if (record == null)
            {
                continue;
            }

            string text = !string.IsNullOrEmpty(record.ShortText) ? record.ShortText : record.Summary;
            if (!string.IsNullOrEmpty(text) && !notable.Contains(text))
            {
                notable.Insert(0, text);
            }
        }

        return notable.Count > 0 ? string.Join(" | ", notable.ToArray()) : "No notable events.";
    }

    private PrototypeBattleResultSnapshot BuildRpgOwnedCurrentBattleResultSnapshotView()
    {
        if (_currentBattleResultSnapshot != null &&
            ((!string.IsNullOrEmpty(_currentBattleResultSnapshot.ResultStateKey) && _currentBattleResultSnapshot.ResultStateKey != PrototypeBattleOutcomeKeys.None) ||
             !string.IsNullOrEmpty(_currentBattleResultSnapshot.EncounterName)))
        {
            return CopyRpgOwnedBattleResultSnapshot(_currentBattleResultSnapshot);
        }

        string outcomeKey = _currentBattleResultSnapshot != null && !string.IsNullOrEmpty(_currentBattleResultSnapshot.OutcomeKey)
            ? _currentBattleResultSnapshot.OutcomeKey
            : PrototypeBattleOutcomeKeys.None;
        return BuildRpgOwnedBattleResultSnapshot(outcomeKey);
    }

    private PrototypeBattleResultSnapshot BuildRpgOwnedBattleResultSnapshot(string outcomeKey)
    {
        PrototypeBattleResultSnapshot snapshot = new PrototypeBattleResultSnapshot();
        DungeonEncounterRuntimeData encounter = GetActiveEncounter();
        DungeonRoomTemplateData room = GetCurrentPlannedRoomStep();
        string encounterId = encounter != null ? encounter.EncounterId : room != null ? room.EncounterId : string.Empty;
        string encounterName = encounter != null ? encounter.DisplayName : room != null ? room.DisplayName : string.Empty;

        snapshot.OutcomeKey = string.IsNullOrEmpty(outcomeKey) ? PrototypeBattleOutcomeKeys.None : outcomeKey;
        snapshot.ResultStateKey = snapshot.OutcomeKey;
        snapshot.EncounterId = string.IsNullOrEmpty(encounterId) ? string.Empty : encounterId;
        snapshot.EncounterName = string.IsNullOrEmpty(encounterName) ? (!string.IsNullOrEmpty(_eliteName) ? _eliteName : string.Empty) : encounterName;
        snapshot.RouteLabel = string.IsNullOrEmpty(_selectedRouteLabel) ? "None" : _selectedRouteLabel;
        snapshot.CurrentDungeonName = string.IsNullOrEmpty(_currentDungeonName) ? "None" : _currentDungeonName;
        snapshot.PartyMembersAtEndSummary = BuildPartyMembersAtEndSummary();
        snapshot.FinalLootSummary = BuildRpgOwnedBattleLootRewardSummaryText();
        snapshot.EliteEncounterName = string.IsNullOrEmpty(_eliteName) ? "None" : _eliteName;
        snapshot.EliteRewardLabel = string.IsNullOrEmpty(_eliteRewardLabel) ? "None" : _eliteRewardLabel;
        snapshot.PreEliteChoiceSummary = GetSelectedPreEliteChoiceDisplayText();
        snapshot.TurnsTaken = Mathf.Max(_runTurnCount, _battleTurnIndex);
        snapshot.SurvivingMemberCount = GetLivingPartyMemberCount();
        snapshot.KnockedOutMemberCount = GetKnockedOutMemberCount();
        snapshot.DefeatedEnemyCount = GetDefeatedEnemyCount();
        snapshot.EliteDefeated = _eliteDefeated;
        snapshot.TotalDamageDealt = _totalDamageDealt;
        snapshot.TotalDamageTaken = _totalDamageTaken;
        snapshot.TotalHealingDone = _totalHealingDone;
        PrototypeBattleContextData battleContext = BuildRpgOwnedBattleContextView();
        PrototypeBattleResultData coreResult = BuildRpgOwnedBattleCoreResultData(snapshot.OutcomeKey);
        snapshot.BattleContextSummary = battleContext.ContextSummaryText;
        snapshot.BattleRuntimeSummary = coreResult.RuntimeSummaryText;
        snapshot.LaneRuleSummary = BuildRpgOwnedBattleRuleSummaryText();
        snapshot.ResourceDeltaSummary = coreResult.ResourceDeltaSummary;
        snapshot.StatusSummary = coreResult.StatusInjurySummary;
        snapshot.ConsumableUseSummary = coreResult.ConsumableUseSummary;
        snapshot.NotableEventsSummary = coreResult.NotableEventsSummary;
        return snapshot;
    }

    private string BuildRpgOwnedBattleLootRewardSummaryText()
    {
        int battleLoot = Mathf.Max(0, _battleLootAmount);
        if (_eliteRewardGranted && !string.IsNullOrEmpty(_eliteRewardLabel))
        {
            string eliteRewardText = _eliteRewardLabel + " (" + BuildLootAmountText(_eliteRewardAmount) + ")";
            if (_eliteBonusRewardGranted && _eliteBonusRewardGrantedAmount > 0)
            {
                eliteRewardText += " + bonus " + BuildLootAmountText(_eliteBonusRewardGrantedAmount);
            }

            return "Battle " + battleLoot + " | Elite " + eliteRewardText;
        }

        return "Battle " + battleLoot;
    }

    private string BuildRpgOwnedBattleAbsorbSummaryText(
        string outcomeKey,
        string encounterName,
        int turnCount,
        int survivingMemberCount,
        int knockedOutMemberCount,
        int defeatedEnemyCount)
    {
        string absorbLabel = outcomeKey == PrototypeBattleOutcomeKeys.RunClear || outcomeKey == PrototypeBattleOutcomeKeys.EncounterVictory
            ? "Victory absorbed"
            : outcomeKey == PrototypeBattleOutcomeKeys.RunDefeat
                ? "Defeat absorbed"
                : outcomeKey == PrototypeBattleOutcomeKeys.RunRetreat
                    ? "Retreat absorbed"
                    : "Battle absorb pending";
        string safeEncounterName = string.IsNullOrEmpty(encounterName) ? "Encounter" : encounterName;
        return absorbLabel +
               " | " + safeEncounterName +
               " | Turn " + Mathf.Max(0, turnCount) +
               " | Party " + Mathf.Max(0, survivingMemberCount) + " up / " + Mathf.Max(0, knockedOutMemberCount) + " KO" +
               " | Enemies defeated " + Mathf.Max(0, defeatedEnemyCount);
    }
}
