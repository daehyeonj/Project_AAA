using System;

public static class PrototypeBattleEventKeys
{
    public const string TurnStart = "turn_start";
    public const string ActionSelected = "action_selected";
    public const string TargetSelected = "target_selected";
    public const string DamageApplied = "damage_applied";
    public const string HealApplied = "heal_applied";
    public const string KnockOut = "knock_out";
    public const string EnemyDefeated = "enemy_defeated";
    public const string EnemyIntentShown = "enemy_intent_shown";
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
    public string EncounterId = string.Empty;
    public string ActorId = string.Empty;
    public string ActorName = string.Empty;
    public string TargetId = string.Empty;
    public string TargetName = string.Empty;
    public string ActionKey = string.Empty;
    public string SkillId = string.Empty;
    public int Amount;
    public int Value;
    public int DeltaHp;
    public int StepIndex;
    public int TurnIndex;
    public bool IsHeal;
    public bool IsDamage;
    public bool DidDefeat;
    public bool DidKnockOut;
    public string ShortText = string.Empty;
    public string Summary = string.Empty;
    public string DetailText = string.Empty;
}

public sealed class PrototypeEnemyIntentSnapshot
{
    public string IntentKey = string.Empty;
    public string DisplayLabel = string.Empty;
    public string TargetPatternKey = string.Empty;
    public string TargetPolicyKey = string.Empty;
    public string PreviewText = string.Empty;
    public int PredictedValue;
    public int PowerValue;
    public string TargetId = string.Empty;
    public string TargetName = string.Empty;
    public string TargetDisplayName = string.Empty;
    public string SourceEnemyId = string.Empty;
    public string SourceEnemyName = string.Empty;
    public string ActorMonsterId = string.Empty;
    public string ActorDisplayName = string.Empty;
    public string ActionKey = string.Empty;
    public string EffectTypeKey = string.Empty;
    public string SpecialActionLabel = string.Empty;
    public bool IsTelegraphed;
}

public sealed class PrototypeBattleResultSnapshot
{
    public string OutcomeKey = PrototypeBattleOutcomeKeys.None;
    public string ResultStateKey = PrototypeBattleOutcomeKeys.None;
    public string ResultSummaryText = string.Empty;
    public string EncounterId = string.Empty;
    public string EncounterName = string.Empty;
    public string RouteLabel = string.Empty;
    public string CurrentDungeonName = string.Empty;
    public string PartyMembersAtEndSummary = string.Empty;
    public string FinalLootSummary = string.Empty;
    public string EliteEncounterId = string.Empty;
    public string EliteEncounterName = string.Empty;
    public string EliteRewardLabel = string.Empty;
    public string EliteTypeLabel = string.Empty;
    public string PreEliteChoiceSummary = string.Empty;
    public string SurvivingMembersText = string.Empty;
    public string PartyHpSummaryText = string.Empty;
    public string PartyConditionText = string.Empty;
    public string RoomPathSummaryText = string.Empty;
    public string EventChoiceText = string.Empty;
    public string PreEliteChoiceText = string.Empty;
    public int RewardAmount;
    public int EliteBonusRewardAmount;
    public int PendingRewardAmount;
    public int ReturnedLootAmount;
    public int TurnsTaken;
    public int SurvivingMemberCount;
    public int KnockedOutMemberCount;
    public int DefeatedEnemyCount;
    public bool EliteDefeated;
    public int TotalDamageDealt;
    public int TotalDamageTaken;
    public int TotalHealingDone;
}
