using System;

public static class PrototypeBattleEventKeys
{
    public const string TurnStart = "turn_start";
    public const string ActionSelected = "action_selected";
    public const string TargetSelected = "target_selected";
    public const string TargetRejected = "target_rejected";
    public const string GuardInterceptTriggered = "guard_intercept_triggered";
    public const string MoveResolved = "move_resolved";
    public const string DamageApplied = "damage_applied";
    public const string HealApplied = "heal_applied";
    public const string KnockOut = "knock_out";
    public const string EnemyDefeated = "enemy_defeated";
    public const string EnemyIntentShown = "enemy_intent_shown";
    public const string BurstWindowOpened = "burst_window_opened";
    public const string BurstWindowExtended = "burst_window_extended";
    public const string BurstWindowConsumed = "burst_window_consumed";
    public const string RangeRuleResolved = "range_rule_resolved";
    public const string LaneRuleResolved = "lane_rule_resolved";
    public const string AttackResolved = "attack_resolved";
    public const string SkillResolved = "skill_resolved";
    public const string BattleVictory = "battle_victory";
    public const string BattleDefeat = "battle_defeat";
    public const string RetreatConfirmed = "retreat_confirmed";
    public const string BattleEnd = "battle_end";
}

public static class PrototypeBattleOutcomeKeys
{
    public const string None = "none";
    public const string EncounterVictory = "encounter_victory";
    public const string RunClear = "run_clear";
    public const string RunDefeat = "run_defeat";
    public const string RunRetreat = "run_retreat";
}

public sealed class PrototypeBattleEventRecord
{
    public string EventId = string.Empty;
    public int Sequence;
    public string EventKey = string.Empty;
    public string EventType = string.Empty;
    public string PhaseKey = string.Empty;
    public string ActorId = string.Empty;
    public string ActorName = string.Empty;
    public string TargetId = string.Empty;
    public string TargetName = string.Empty;
    public string ActionKey = string.Empty;
    public string SkillId = string.Empty;
    public int Amount;
    public int Value;
    public int StepIndex;
    public int TurnIndex;
    public string ShortText = string.Empty;
    public string Summary = string.Empty;
}

public sealed class PrototypeEnemyIntentSnapshot
{
    public string IntentKey = string.Empty;
    public string TargetPatternKey = string.Empty;
    public string PreviewText = string.Empty;
    public int PredictedValue;
    public string TargetId = string.Empty;
    public string TargetName = string.Empty;
    public string SourceEnemyId = string.Empty;
    public string SourceEnemyName = string.Empty;
    public string ActionKey = string.Empty;
    public string ActionLabel = string.Empty;
    public string EffectKey = string.Empty;
    public string SourceRoleLabel = string.Empty;
    public string RangeKey = string.Empty;
    public string LaneRuleKey = string.Empty;
    public string ThreatLaneKey = string.Empty;
    public string ThreatLaneLabel = string.Empty;
    public string RangeText = string.Empty;
    public string PredictedRangeText = string.Empty;
    public string PredictedReachabilityText = string.Empty;
    public string TargetRuleText = string.Empty;
}

// Legacy compatibility shell for older snapshot consumers. BattleResult is the exact battle-owned contract.
public sealed class PrototypeBattleResultSnapshot
{
    public string OutcomeKey = PrototypeBattleOutcomeKeys.None;
    public string ResultStateKey = PrototypeBattleOutcomeKeys.None;
    public string EncounterId = string.Empty;
    public string EncounterName = string.Empty;
    public string RouteLabel = string.Empty;
    public string CurrentDungeonName = string.Empty;
    public string PartyMembersAtEndSummary = string.Empty;
    public string FinalLootSummary = string.Empty;
    public string EliteEncounterName = string.Empty;
    public string EliteRewardLabel = string.Empty;
    public string PreEliteChoiceSummary = string.Empty;
    public int TurnsTaken;
    public int SurvivingMemberCount;
    public int KnockedOutMemberCount;
    public int DefeatedEnemyCount;
    public bool EliteDefeated;
    public int TotalDamageDealt;
    public int TotalDamageTaken;
    public int TotalHealingDone;
    public string BattleContextSummary = string.Empty;
    public string BattleRuntimeSummary = string.Empty;
    public string LaneRuleSummary = string.Empty;
    public string ResourceDeltaSummary = string.Empty;
    public string StatusSummary = string.Empty;
    public string ConsumableUseSummary = string.Empty;
    public string NotableEventsSummary = string.Empty;
}
