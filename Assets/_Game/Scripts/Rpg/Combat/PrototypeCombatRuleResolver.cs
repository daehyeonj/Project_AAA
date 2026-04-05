using System;

public static class PrototypeCombatActionKeys
{
    public const string Attack = "attack";
    public const string Skill = "skill";
    public const string Retreat = "retreat";
}

public static class PrototypeCombatEffectKeys
{
    public const string Damage = "damage";
    public const string SpecialDamage = "special_damage";
    public const string FinisherDamage = "finisher_damage";
    public const string Heal = "heal";
    public const string Retreat = "retreat";
}

public static class PrototypeCombatConditionKeys
{
    public const string TargetWeakened = "target_weakened";
    public const string InjuredAllies = "injured_allies";
}

public static class PrototypeCombatPatternKeys
{
    public const string BasicStrike = "basic_strike";
    public const string HeavySingle = "heavy_single";
    public const string PrecisionFinisher = "precision_finisher";
    public const string ArcaneVolley = "arcane_volley";
    public const string PartyRecovery = "party_recovery";
    public const string Retreat = "retreat";
}

public sealed class PrototypeCombatResolutionRecord
{
    public string ActorId = string.Empty;
    public int ActorIndex = -1;
    public string ActorLabel = string.Empty;
    public string ActionKey = string.Empty;
    public string ActionLabel = string.Empty;
    public string TargetKind = string.Empty;
    public string TargetPolicyKey = string.Empty;
    public string TargetId = string.Empty;
    public string TargetLabel = string.Empty;
    public int TargetIndex = -1;
    public string SkillId = string.Empty;
    public string EffectTypeKey = string.Empty;
    public string ConditionKey = string.Empty;
    public string PatternKey = string.Empty;
    public int PowerValue;
    public int ResolvedAmount;
    public int TotalResolvedAmount;
    public int AffectedCount;
    public string FeedbackText = string.Empty;
    public string LogText = string.Empty;
    public string SummaryText = string.Empty;
    public bool RequiresTarget;
    public bool IsSkillAction;
    public bool IsRetreat;
    public bool DidResolve;
    public bool ConditionApplied;
}

public static class PrototypeCombatRuleResolver
{
    public static string ResolveSkillTargetKind(PrototypeRpgSkillDefinition skillDefinition, string fallbackTargetKind)
    {
        if (skillDefinition != null && !string.IsNullOrWhiteSpace(skillDefinition.TargetKind))
        {
            return PrototypeCombatTargetPolicy.NormalizeTargetKind(skillDefinition.TargetKind);
        }

        return PrototypeCombatTargetPolicy.NormalizeTargetKind(fallbackTargetKind);
    }

    public static string ResolveSkillEffectType(PrototypeRpgSkillDefinition skillDefinition, string fallbackEffectType)
    {
        if (skillDefinition != null && !string.IsNullOrWhiteSpace(skillDefinition.EffectType))
        {
            return skillDefinition.EffectType.Trim();
        }

        return string.IsNullOrWhiteSpace(fallbackEffectType) ? PrototypeCombatEffectKeys.Damage : fallbackEffectType.Trim();
    }

    public static int ResolveSkillPower(PrototypeRpgSkillDefinition skillDefinition, int fallbackPowerValue)
    {
        if (skillDefinition != null && skillDefinition.PowerValue > 0)
        {
            return skillDefinition.PowerValue;
        }

        return fallbackPowerValue > 0 ? fallbackPowerValue : 1;
    }

    public static string ResolveSkillConditionKey(PrototypeRpgSkillDefinition skillDefinition, string fallbackConditionKey)
    {
        if (skillDefinition != null && !string.IsNullOrWhiteSpace(skillDefinition.ConditionKey))
        {
            return skillDefinition.ConditionKey.Trim();
        }

        return string.IsNullOrWhiteSpace(fallbackConditionKey) ? string.Empty : fallbackConditionKey.Trim();
    }

    public static string ResolveSkillPatternKey(PrototypeRpgSkillDefinition skillDefinition, string fallbackPatternKey, string targetKind, string effectTypeKey)
    {
        if (skillDefinition != null && !string.IsNullOrWhiteSpace(skillDefinition.PatternKey))
        {
            return skillDefinition.PatternKey.Trim();
        }

        if (!string.IsNullOrWhiteSpace(fallbackPatternKey))
        {
            return fallbackPatternKey.Trim();
        }

        string normalizedTargetKind = PrototypeCombatTargetPolicy.NormalizeTargetKind(targetKind);
        string normalizedEffectTypeKey = string.IsNullOrWhiteSpace(effectTypeKey) ? string.Empty : effectTypeKey.Trim();
        if (normalizedTargetKind == PrototypeCombatTargetKindKeys.AllEnemies)
        {
            return PrototypeCombatPatternKeys.ArcaneVolley;
        }

        if (normalizedTargetKind == PrototypeCombatTargetKindKeys.AllAllies)
        {
            return PrototypeCombatPatternKeys.PartyRecovery;
        }

        if (string.Equals(normalizedEffectTypeKey, PrototypeCombatEffectKeys.FinisherDamage, StringComparison.OrdinalIgnoreCase))
        {
            return PrototypeCombatPatternKeys.PrecisionFinisher;
        }

        return PrototypeCombatPatternKeys.HeavySingle;
    }

    public static PrototypeCombatResolutionRecord BuildAttackAction(string actorId, int actorIndex, string actorLabel, int attackPower)
    {
        PrototypeCombatResolutionRecord record = new PrototypeCombatResolutionRecord();
        record.ActorId = string.IsNullOrWhiteSpace(actorId) ? string.Empty : actorId.Trim();
        record.ActorIndex = actorIndex;
        record.ActorLabel = string.IsNullOrWhiteSpace(actorLabel) ? "Actor" : actorLabel.Trim();
        record.ActionKey = PrototypeCombatActionKeys.Attack;
        record.ActionLabel = "Attack";
        record.TargetKind = PrototypeCombatTargetKindKeys.SingleEnemy;
        record.TargetPolicyKey = PrototypeCombatTargetPolicyKeys.FrontmostLiving;
        record.EffectTypeKey = PrototypeCombatEffectKeys.Damage;
        record.PatternKey = PrototypeCombatPatternKeys.BasicStrike;
        record.PowerValue = attackPower > 0 ? attackPower : 1;
        record.RequiresTarget = true;
        record.FeedbackText = "Attack selected.";
        record.LogText = record.ActorLabel + " selected Attack.";
        record.SummaryText = "Basic attack";
        return record;
    }

    public static PrototypeCombatResolutionRecord BuildSkillAction(string actorId, int actorIndex, string actorLabel, PrototypeRpgSkillDefinition skillDefinition, string fallbackSkillId, string fallbackSkillLabel, string fallbackTargetKind, string fallbackEffectType, int fallbackPowerValue)
    {
        PrototypeCombatResolutionRecord record = new PrototypeCombatResolutionRecord();
        record.ActorId = string.IsNullOrWhiteSpace(actorId) ? string.Empty : actorId.Trim();
        record.ActorIndex = actorIndex;
        record.ActorLabel = string.IsNullOrWhiteSpace(actorLabel) ? "Actor" : actorLabel.Trim();
        record.ActionKey = PrototypeCombatActionKeys.Skill;
        record.ActionLabel = skillDefinition != null && !string.IsNullOrWhiteSpace(skillDefinition.DisplayName)
            ? skillDefinition.DisplayName.Trim()
            : string.IsNullOrWhiteSpace(fallbackSkillLabel) ? "Skill" : fallbackSkillLabel.Trim();
        record.SkillId = skillDefinition != null && !string.IsNullOrWhiteSpace(skillDefinition.SkillId)
            ? skillDefinition.SkillId.Trim()
            : string.IsNullOrWhiteSpace(fallbackSkillId) ? string.Empty : fallbackSkillId.Trim();
        record.TargetKind = ResolveSkillTargetKind(skillDefinition, fallbackTargetKind);
        record.TargetPolicyKey = record.TargetKind == PrototypeCombatTargetKindKeys.SingleEnemy
            ? PrototypeCombatTargetPolicyKeys.FrontmostLiving
            : record.TargetKind == PrototypeCombatTargetKindKeys.AllAllies
                ? PrototypeCombatTargetPolicyKeys.AllLivingAllies
                : string.Empty;
        record.EffectTypeKey = ResolveSkillEffectType(skillDefinition, fallbackEffectType);
        record.ConditionKey = ResolveSkillConditionKey(skillDefinition, string.Empty);
        record.PatternKey = ResolveSkillPatternKey(skillDefinition, string.Empty, record.TargetKind, record.EffectTypeKey);
        record.PowerValue = ResolveSkillPower(skillDefinition, fallbackPowerValue);
        record.RequiresTarget = PrototypeCombatTargetPolicy.RequiresTargetSelection(record.TargetKind);
        record.IsSkillAction = true;
        record.FeedbackText = record.ActionLabel + " selected.";
        record.LogText = record.ActorLabel + " selected " + record.ActionLabel + ".";
        record.SummaryText = record.ActionLabel;
        return record;
    }

    public static PrototypeCombatResolutionRecord BuildRetreatAction(string actorId, int actorIndex, string actorLabel)
    {
        PrototypeCombatResolutionRecord record = new PrototypeCombatResolutionRecord();
        record.ActorId = string.IsNullOrWhiteSpace(actorId) ? string.Empty : actorId.Trim();
        record.ActorIndex = actorIndex;
        record.ActorLabel = string.IsNullOrWhiteSpace(actorLabel) ? "Party" : actorLabel.Trim();
        record.ActionKey = PrototypeCombatActionKeys.Retreat;
        record.ActionLabel = "Retreat";
        record.TargetKind = PrototypeCombatTargetKindKeys.Party;
        record.EffectTypeKey = PrototypeCombatEffectKeys.Retreat;
        record.PatternKey = PrototypeCombatPatternKeys.Retreat;
        record.RequiresTarget = false;
        record.IsRetreat = true;
        record.FeedbackText = "Retreat selected.";
        record.LogText = record.ActorLabel + " selected Retreat.";
        record.SummaryText = "Retreat";
        return record;
    }

    public static PrototypeCombatResolutionRecord BuildEnemyAction(string actorId, int actorIndex, string actorLabel, string actionLabel, string targetPolicyKey, string targetKind, string effectTypeKey, int powerValue, string targetId, string targetLabel, int targetIndex, string patternKey = "")
    {
        PrototypeCombatResolutionRecord record = new PrototypeCombatResolutionRecord();
        record.ActorId = string.IsNullOrWhiteSpace(actorId) ? string.Empty : actorId.Trim();
        record.ActorIndex = actorIndex;
        record.ActorLabel = string.IsNullOrWhiteSpace(actorLabel) ? "Enemy" : actorLabel.Trim();
        record.ActionKey = string.Equals(effectTypeKey, PrototypeCombatEffectKeys.SpecialDamage, StringComparison.OrdinalIgnoreCase)
            ? PrototypeCombatActionKeys.Skill
            : PrototypeCombatActionKeys.Attack;
        record.ActionLabel = string.IsNullOrWhiteSpace(actionLabel) ? "Attack" : actionLabel.Trim();
        record.TargetPolicyKey = PrototypeCombatTargetPolicy.NormalizeTargetPolicyKey(targetPolicyKey, targetKind == PrototypeCombatTargetKindKeys.AllAllies);
        record.TargetKind = PrototypeCombatTargetPolicy.NormalizeTargetKind(targetKind);
        record.EffectTypeKey = string.IsNullOrWhiteSpace(effectTypeKey) ? PrototypeCombatEffectKeys.Damage : effectTypeKey.Trim();
        record.PatternKey = string.IsNullOrWhiteSpace(patternKey) ? string.Empty : patternKey.Trim();
        record.PowerValue = powerValue > 0 ? powerValue : 1;
        record.TargetId = string.IsNullOrWhiteSpace(targetId) ? string.Empty : targetId.Trim();
        record.TargetLabel = string.IsNullOrWhiteSpace(targetLabel) ? string.Empty : targetLabel.Trim();
        record.TargetIndex = targetIndex;
        record.RequiresTarget = record.TargetKind == PrototypeCombatTargetKindKeys.SingleEnemy;
        record.SummaryText = record.ActionLabel;
        return record;
    }

    public static bool IsConditionApplied(string conditionKey, int actorAttack, int targetCurrentHp, int affectedCount, int totalResolvedAmount)
    {
        string normalizedConditionKey = string.IsNullOrWhiteSpace(conditionKey) ? string.Empty : conditionKey.Trim();
        if (string.Equals(normalizedConditionKey, PrototypeCombatConditionKeys.TargetWeakened, StringComparison.OrdinalIgnoreCase))
        {
            return targetCurrentHp > 0 && targetCurrentHp <= Math.Max(1, actorAttack);
        }

        if (string.Equals(normalizedConditionKey, PrototypeCombatConditionKeys.InjuredAllies, StringComparison.OrdinalIgnoreCase))
        {
            return affectedCount > 0 || totalResolvedAmount > 0;
        }

        return false;
    }

    public static int ResolveSingleTargetDamage(string effectTypeKey, int powerValue, int actorAttack, int targetCurrentHp)
    {
        return ResolveSingleTargetDamage(effectTypeKey, string.Empty, powerValue, actorAttack, targetCurrentHp);
    }

    public static int ResolveSingleTargetDamage(string effectTypeKey, string conditionKey, int powerValue, int actorAttack, int targetCurrentHp)
    {
        int safePowerValue = powerValue > 0 ? powerValue : 1;
        bool conditionApplied = IsConditionApplied(conditionKey, actorAttack, targetCurrentHp, 0, 0);
        if ((string.Equals(effectTypeKey, PrototypeCombatEffectKeys.FinisherDamage, StringComparison.OrdinalIgnoreCase) || conditionApplied) && conditionApplied)
        {
            return safePowerValue + 2;
        }

        return safePowerValue;
    }

    public static string BuildResolutionShortText(PrototypeCombatResolutionRecord record, bool conditionApplied)
    {
        string actionLabel = record != null && !string.IsNullOrWhiteSpace(record.ActionLabel) ? record.ActionLabel.Trim() : "Action";
        return conditionApplied ? actionLabel + "!" : actionLabel;
    }

    public static string BuildPartyResolutionFeedbackText(PrototypeCombatResolutionRecord record, string targetLabel, int totalResolvedAmount, int affectedCount, bool conditionApplied)
    {
        string actionLabel = record != null && !string.IsNullOrWhiteSpace(record.ActionLabel) ? record.ActionLabel.Trim() : "Action";
        string safeTargetLabel = string.IsNullOrWhiteSpace(targetLabel) ? "target" : targetLabel.Trim();
        if (record != null && record.TargetKind == PrototypeCombatTargetKindKeys.AllEnemies)
        {
            return actionLabel + " hit " + Math.Max(0, affectedCount) + " enemies for " + Math.Max(0, totalResolvedAmount) + " total damage.";
        }

        if (record != null && record.TargetKind == PrototypeCombatTargetKindKeys.AllAllies)
        {
            return totalResolvedAmount > 0
                ? actionLabel + " restored " + Math.Max(0, totalResolvedAmount) + " HP across " + Math.Max(0, affectedCount) + " allies."
                : actionLabel + " found no wounded allies.";
        }

        if (conditionApplied)
        {
            return actionLabel + " exploited a weakened target for " + Math.Max(0, totalResolvedAmount) + " damage.";
        }

        return actionLabel + " dealt " + Math.Max(0, totalResolvedAmount) + " to " + safeTargetLabel + ".";
    }

    public static string BuildPartyResolutionLogText(PrototypeCombatResolutionRecord record, string actorLabel, string targetLabel, int totalResolvedAmount, int affectedCount, bool conditionApplied)
    {
        string safeActorLabel = string.IsNullOrWhiteSpace(actorLabel) ? "Party" : actorLabel.Trim();
        string actionLabel = record != null && !string.IsNullOrWhiteSpace(record.ActionLabel) ? record.ActionLabel.Trim() : "Action";
        string safeTargetLabel = string.IsNullOrWhiteSpace(targetLabel) ? "target" : targetLabel.Trim();
        if (record != null && record.TargetKind == PrototypeCombatTargetKindKeys.AllEnemies)
        {
            return safeActorLabel + " used " + actionLabel + " on all enemies for " + Math.Max(0, totalResolvedAmount) + " total damage.";
        }

        if (record != null && record.TargetKind == PrototypeCombatTargetKindKeys.AllAllies)
        {
            return safeActorLabel + " used " + actionLabel + " and restored " + Math.Max(0, totalResolvedAmount) + " HP across " + Math.Max(0, affectedCount) + " allies.";
        }

        if (conditionApplied)
        {
            return safeActorLabel + " used " + actionLabel + " on " + safeTargetLabel + " for " + Math.Max(0, totalResolvedAmount) + " damage and exploited a weakened target.";
        }

        return safeActorLabel + " used " + actionLabel + " on " + safeTargetLabel + " for " + Math.Max(0, totalResolvedAmount) + " damage.";
    }

    public static string BuildEnemyResolutionFeedbackText(PrototypeCombatResolutionRecord record, string actorLabel, string targetLabel, int totalResolvedAmount, int affectedCount)
    {
        string safeActorLabel = string.IsNullOrWhiteSpace(actorLabel) ? "Enemy" : actorLabel.Trim();
        string actionLabel = record != null && !string.IsNullOrWhiteSpace(record.ActionLabel) ? record.ActionLabel.Trim() : "Attack";
        string safeTargetLabel = string.IsNullOrWhiteSpace(targetLabel) ? "target" : targetLabel.Trim();
        if (record != null && record.TargetKind == PrototypeCombatTargetKindKeys.AllAllies)
        {
            return safeActorLabel + " unleashed " + actionLabel + " on " + Math.Max(0, affectedCount) + " allies.";
        }

        return safeActorLabel + " used " + actionLabel + " on " + safeTargetLabel + ".";
    }

    public static string BuildEnemyResolutionLogText(PrototypeCombatResolutionRecord record, string actorLabel, string targetLabel, int totalResolvedAmount, int affectedCount)
    {
        string safeActorLabel = string.IsNullOrWhiteSpace(actorLabel) ? "Enemy" : actorLabel.Trim();
        string actionLabel = record != null && !string.IsNullOrWhiteSpace(record.ActionLabel) ? record.ActionLabel.Trim() : "Attack";
        string safeTargetLabel = string.IsNullOrWhiteSpace(targetLabel) ? "target" : targetLabel.Trim();
        if (record != null && record.TargetKind == PrototypeCombatTargetKindKeys.AllAllies)
        {
            return safeActorLabel + " used " + actionLabel + " on all living allies for " + Math.Max(0, totalResolvedAmount) + " total damage.";
        }

        return safeActorLabel + " used " + actionLabel + " on " + safeTargetLabel + " for " + Math.Max(0, totalResolvedAmount) + " damage.";
    }
}
