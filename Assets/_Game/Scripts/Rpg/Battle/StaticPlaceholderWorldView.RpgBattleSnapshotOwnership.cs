public sealed partial class StaticPlaceholderWorldView
{
    private PrototypeBattleEventRecord CopyRpgOwnedBattleEventRecord(PrototypeBattleEventRecord source)
    {
        PrototypeBattleEventRecord copy = new PrototypeBattleEventRecord();
        if (source == null)
        {
            return copy;
        }

        copy.EventId = source.EventId;
        copy.Sequence = source.Sequence;
        copy.EventKey = source.EventKey;
        copy.EventType = source.EventType;
        copy.PhaseKey = source.PhaseKey;
        copy.ActorId = source.ActorId;
        copy.ActorName = source.ActorName;
        copy.TargetId = source.TargetId;
        copy.TargetName = source.TargetName;
        copy.ActionKey = source.ActionKey;
        copy.SkillId = source.SkillId;
        copy.Amount = source.Amount;
        copy.Value = source.Value;
        copy.StepIndex = source.StepIndex;
        copy.TurnIndex = source.TurnIndex;
        copy.ShortText = source.ShortText;
        copy.Summary = source.Summary;
        return copy;
    }

    private PrototypeBattleResultSnapshot CopyRpgOwnedBattleResultSnapshot(PrototypeBattleResultSnapshot source)
    {
        PrototypeBattleResultSnapshot copy = new PrototypeBattleResultSnapshot();
        if (source == null)
        {
            return copy;
        }

        copy.OutcomeKey = source.OutcomeKey;
        copy.ResultStateKey = source.ResultStateKey;
        copy.EncounterId = source.EncounterId;
        copy.EncounterName = source.EncounterName;
        copy.RouteLabel = source.RouteLabel;
        copy.CurrentDungeonName = source.CurrentDungeonName;
        copy.PartyMembersAtEndSummary = source.PartyMembersAtEndSummary;
        copy.FinalLootSummary = source.FinalLootSummary;
        copy.EliteEncounterName = source.EliteEncounterName;
        copy.EliteRewardLabel = source.EliteRewardLabel;
        copy.PreEliteChoiceSummary = source.PreEliteChoiceSummary;
        copy.TurnsTaken = source.TurnsTaken;
        copy.SurvivingMemberCount = source.SurvivingMemberCount;
        copy.KnockedOutMemberCount = source.KnockedOutMemberCount;
        copy.DefeatedEnemyCount = source.DefeatedEnemyCount;
        copy.EliteDefeated = source.EliteDefeated;
        copy.TotalDamageDealt = source.TotalDamageDealt;
        copy.TotalDamageTaken = source.TotalDamageTaken;
        copy.TotalHealingDone = source.TotalHealingDone;
        copy.BattleContextSummary = source.BattleContextSummary;
        copy.BattleRuntimeSummary = source.BattleRuntimeSummary;
        copy.LaneRuleSummary = source.LaneRuleSummary;
        copy.ResourceDeltaSummary = source.ResourceDeltaSummary;
        copy.StatusSummary = source.StatusSummary;
        copy.ConsumableUseSummary = source.ConsumableUseSummary;
        copy.NotableEventsSummary = source.NotableEventsSummary;
        return copy;
    }

    private BattleResult CopyRpgOwnedBattleResult(BattleResult source)
    {
        BattleResult copy = new BattleResult();
        if (source == null)
        {
            return copy;
        }

        copy.OutcomeKey = source.OutcomeKey;
        copy.ResultStateKey = source.ResultStateKey;
        copy.BattleId = source.BattleId;
        copy.EncounterId = source.EncounterId;
        copy.EncounterName = source.EncounterName;
        copy.EliteEncounterName = source.EliteEncounterName;
        copy.EliteRewardLabel = source.EliteRewardLabel;
        copy.DungeonId = source.DungeonId;
        copy.DungeonLabel = source.DungeonLabel;
        copy.RouteId = source.RouteId;
        copy.RouteLabel = source.RouteLabel;
        copy.TurnCount = source.TurnCount;
        copy.SurvivingMemberCount = source.SurvivingMemberCount;
        copy.KnockedOutMemberCount = source.KnockedOutMemberCount;
        copy.DefeatedEnemyCount = source.DefeatedEnemyCount;
        copy.EliteDefeated = source.EliteDefeated;
        copy.TotalDamageDealt = source.TotalDamageDealt;
        copy.TotalDamageTaken = source.TotalDamageTaken;
        copy.TotalHealingDone = source.TotalHealingDone;
        copy.ResultSummaryText = source.ResultSummaryText;
        copy.RuntimeSummaryText = source.RuntimeSummaryText;
        copy.PartyAftermathSummaryText = source.PartyAftermathSummaryText;
        copy.ResourceDeltaSummaryText = source.ResourceDeltaSummaryText;
        copy.StatusSummaryText = source.StatusSummaryText;
        copy.ConsumableUseSummaryText = source.ConsumableUseSummaryText;
        copy.LootRewardSummaryText = source.LootRewardSummaryText;
        copy.NotableEventsSummaryText = source.NotableEventsSummaryText;
        copy.PartyConditionAfterBattleText = source.PartyConditionAfterBattleText;
        copy.BattleContextSummaryText = source.BattleContextSummaryText;
        copy.LaneRuleSummaryText = source.LaneRuleSummaryText;
        copy.AbsorbSummaryText = source.AbsorbSummaryText;
        return copy;
    }

    private PrototypeEnemyIntentSnapshot CopyRpgOwnedEnemyIntentSnapshot(PrototypeEnemyIntentSnapshot source)
    {
        PrototypeEnemyIntentSnapshot copy = new PrototypeEnemyIntentSnapshot();
        if (source == null)
        {
            return copy;
        }

        copy.IntentKey = source.IntentKey;
        copy.TargetPatternKey = source.TargetPatternKey;
        copy.PreviewText = source.PreviewText;
        copy.PredictedValue = source.PredictedValue;
        copy.TargetId = source.TargetId;
        copy.TargetName = source.TargetName;
        copy.SourceEnemyId = source.SourceEnemyId;
        copy.SourceEnemyName = source.SourceEnemyName;
        copy.ActionKey = source.ActionKey;
        copy.ActionLabel = source.ActionLabel;
        copy.EffectKey = source.EffectKey;
        copy.SourceRoleLabel = source.SourceRoleLabel;
        copy.RangeKey = source.RangeKey;
        copy.LaneRuleKey = source.LaneRuleKey;
        copy.ThreatLaneKey = source.ThreatLaneKey;
        copy.ThreatLaneLabel = source.ThreatLaneLabel;
        copy.RangeText = source.RangeText;
        copy.PredictedRangeText = source.PredictedRangeText;
        copy.PredictedReachabilityText = source.PredictedReachabilityText;
        copy.TargetRuleText = source.TargetRuleText;
        return copy;
    }
}
