// Exact battle-owned contract. Expedition/writeback layers consume this via adapters.
public sealed class BattleResult
{
    public string OutcomeKey = string.Empty;
    public string ResultStateKey = string.Empty;
    public string BattleId = string.Empty;
    public string EncounterId = string.Empty;
    public string EncounterName = "None";
    public string EliteEncounterName = "None";
    public string EliteRewardLabel = "None";
    public string DungeonId = string.Empty;
    public string DungeonLabel = "None";
    public string RouteId = string.Empty;
    public string RouteLabel = "None";
    public int TurnCount;
    public int SurvivingMemberCount;
    public int KnockedOutMemberCount;
    public int DefeatedEnemyCount;
    public bool EliteDefeated;
    public int TotalDamageDealt;
    public int TotalDamageTaken;
    public int TotalHealingDone;
    public string ResultSummaryText = "None";
    public string RuntimeSummaryText = "None";
    public string PartyAftermathSummaryText = "None";
    public string ResourceDeltaSummaryText = "None";
    public string StatusSummaryText = "None";
    public string ConsumableUseSummaryText = "None";
    public string LootRewardSummaryText = "None";
    public string NotableEventsSummaryText = "None";
    public string PartyConditionAfterBattleText = "None";
    public string BattleContextSummaryText = "None";
    public string LaneRuleSummaryText = "None";
    public string AbsorbSummaryText = "None";
}
