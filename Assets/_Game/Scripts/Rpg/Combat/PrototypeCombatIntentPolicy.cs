using System;

public static class PrototypeCombatEnemyCadenceKeys
{
    public const string None = "none";
    public const string EliteEveryOtherTurn = "elite_every_other_turn";
    public const string StrikerEveryOtherTurn = "striker_every_other_turn";
}

public static class PrototypeCombatEnemyPatternKeys
{
    public const string FrontPressure = "front_pressure";
    public const string FocusWeakest = "focus_weakest";
    public const string UnstableFocus = "unstable_focus";
    public const string EliteSweep = "elite_sweep";
}

public static class PrototypeCombatIntentPolicy
{
    public static string ResolveSpecialCadenceKey(bool isElite, string roleTag)
    {
        if (isElite)
        {
            return PrototypeCombatEnemyCadenceKeys.EliteEveryOtherTurn;
        }

        return !string.IsNullOrEmpty(roleTag) && roleTag.IndexOf("striker", StringComparison.OrdinalIgnoreCase) >= 0
            ? PrototypeCombatEnemyCadenceKeys.StrikerEveryOtherTurn
            : PrototypeCombatEnemyCadenceKeys.None;
    }

    public static bool ShouldUseSpecialAttack(bool isElite, string roleTag, int turnsActed)
    {
        string cadenceKey = ResolveSpecialCadenceKey(isElite, roleTag);
        if (cadenceKey == PrototypeCombatEnemyCadenceKeys.EliteEveryOtherTurn)
        {
            return turnsActed > 0 && turnsActed % 2 == 1;
        }

        if (cadenceKey == PrototypeCombatEnemyCadenceKeys.StrikerEveryOtherTurn)
        {
            return turnsActed > 0 && turnsActed % 2 == 0;
        }

        return false;
    }

    public static string ResolveFallbackTargetPolicyKey(string roleTag, bool isElite, bool useSpecial)
    {
        if (useSpecial && isElite && !string.IsNullOrEmpty(roleTag) && roleTag.IndexOf("bulwark", StringComparison.OrdinalIgnoreCase) >= 0)
        {
            return PrototypeCombatTargetPolicyKeys.AllLivingAllies;
        }

        if (!string.IsNullOrEmpty(roleTag))
        {
            if (roleTag.IndexOf("striker", StringComparison.OrdinalIgnoreCase) >= 0)
            {
                return PrototypeCombatTargetPolicyKeys.LowestHpLiving;
            }

            if (roleTag.IndexOf("skirmisher", StringComparison.OrdinalIgnoreCase) >= 0)
            {
                return PrototypeCombatTargetPolicyKeys.RandomLiving;
            }
        }

        return PrototypeCombatTargetPolicyKeys.FrontmostLiving;
    }

    public static string ResolveFallbackIntentKey(string roleTag, string targetPolicyKey, bool useSpecial)
    {
        string normalizedTargetPolicyKey = PrototypeCombatTargetPolicy.NormalizeTargetPolicyKey(targetPolicyKey);
        if (useSpecial && normalizedTargetPolicyKey == PrototypeCombatTargetPolicyKeys.AllLivingAllies)
        {
            return "intent_royal_wave";
        }

        switch (normalizedTargetPolicyKey)
        {
            case PrototypeCombatTargetPolicyKeys.LowestHpLiving:
                return "intent_finish_weakest";
            case PrototypeCombatTargetPolicyKeys.RandomLiving:
                return "intent_unstable_focus";
            default:
                return !string.IsNullOrEmpty(roleTag) && roleTag.IndexOf("striker", StringComparison.OrdinalIgnoreCase) >= 0
                    ? "intent_frontline_pressure"
                    : "intent_frontline_pressure";
        }
    }

    public static PrototypeRpgEnemyIntentDefinition ResolveIntentDefinition(PrototypeRpgEnemyDefinition enemyDefinition, string fallbackTargetPolicyKey, bool useSpecial)
    {
        if (enemyDefinition != null)
        {
            PrototypeRpgEnemyIntentDefinition preferred = useSpecial ? enemyDefinition.SpecialIntentDefinition : enemyDefinition.DefaultIntentDefinition;
            if (preferred != null)
            {
                return preferred;
            }

            string preferredIntentKey = useSpecial ? enemyDefinition.SpecialIntentKey : enemyDefinition.DefaultIntentKey;
            if (!string.IsNullOrWhiteSpace(preferredIntentKey))
            {
                PrototypeRpgEnemyIntentDefinition resolved = PrototypeRpgEnemyIntentCatalog.ResolveDefinition(preferredIntentKey);
                if (resolved != null)
                {
                    return resolved;
                }
            }
        }

        string roleTag = enemyDefinition != null ? enemyDefinition.RoleTag : string.Empty;
        return PrototypeRpgEnemyIntentCatalog.ResolveDefinition(ResolveFallbackIntentKey(roleTag, fallbackTargetPolicyKey, useSpecial));
    }

    public static string ResolveTargetPolicyKey(PrototypeRpgEnemyIntentDefinition intentDefinition, string fallbackTargetPolicyKey, bool isPartyWideAction)
    {
        if (intentDefinition != null && !string.IsNullOrWhiteSpace(intentDefinition.TargetPolicyKey))
        {
            return PrototypeCombatTargetPolicy.NormalizeTargetPolicyKey(intentDefinition.TargetPolicyKey, isPartyWideAction);
        }

        return PrototypeCombatTargetPolicy.NormalizeTargetPolicyKey(fallbackTargetPolicyKey, isPartyWideAction);
    }

    public static string ResolveEffectTypeKey(PrototypeRpgEnemyIntentDefinition intentDefinition, bool useSpecial)
    {
        if (intentDefinition != null && !string.IsNullOrWhiteSpace(intentDefinition.EffectHintKey))
        {
            return intentDefinition.EffectHintKey;
        }

        return useSpecial ? PrototypeCombatEffectKeys.SpecialDamage : PrototypeCombatEffectKeys.Damage;
    }

    public static int ResolveActionPower(PrototypeRpgEnemyDefinition enemyDefinition, PrototypeRpgEnemyIntentDefinition intentDefinition, bool useSpecial, int fallbackAttackPower, int fallbackSpecialPower)
    {
        int basePower = useSpecial
            ? (enemyDefinition != null && enemyDefinition.SpecialPowerHint > 0 ? enemyDefinition.SpecialPowerHint : fallbackSpecialPower)
            : (enemyDefinition != null && enemyDefinition.AttackPower > 0 ? enemyDefinition.AttackPower : fallbackAttackPower);
        return basePower > 0 ? basePower : 1;
    }

    public static string ResolveActionLabel(PrototypeRpgEnemyDefinition enemyDefinition, PrototypeRpgEnemyIntentDefinition intentDefinition, bool useSpecial, string fallbackActionLabel)
    {
        if (intentDefinition != null && !string.IsNullOrWhiteSpace(intentDefinition.ActionLabel))
        {
            return intentDefinition.ActionLabel;
        }

        if (useSpecial && enemyDefinition != null && !string.IsNullOrWhiteSpace(enemyDefinition.SpecialActionLabel))
        {
            return enemyDefinition.SpecialActionLabel;
        }

        return string.IsNullOrWhiteSpace(fallbackActionLabel) ? "Attack" : fallbackActionLabel.Trim();
    }

    public static string ResolveDisplayLabel(PrototypeRpgEnemyIntentDefinition intentDefinition, string fallbackActionLabel)
    {
        if (intentDefinition != null && !string.IsNullOrWhiteSpace(intentDefinition.ShortLabel))
        {
            return intentDefinition.ShortLabel;
        }

        return string.IsNullOrWhiteSpace(fallbackActionLabel) ? "Intent" : fallbackActionLabel.Trim();
    }

    public static string ResolvePatternKey(PrototypeRpgEnemyDefinition enemyDefinition, string targetPolicyKey, bool useSpecial)
    {
        string roleTag = enemyDefinition != null ? enemyDefinition.RoleTag : string.Empty;
        string normalizedTargetPolicyKey = PrototypeCombatTargetPolicy.NormalizeTargetPolicyKey(targetPolicyKey, useSpecial && enemyDefinition != null && enemyDefinition.IsElite && roleTag.IndexOf("bulwark", StringComparison.OrdinalIgnoreCase) >= 0);
        if (normalizedTargetPolicyKey == PrototypeCombatTargetPolicyKeys.AllLivingAllies)
        {
            return PrototypeCombatEnemyPatternKeys.EliteSweep;
        }

        if (normalizedTargetPolicyKey == PrototypeCombatTargetPolicyKeys.LowestHpLiving)
        {
            return PrototypeCombatEnemyPatternKeys.FocusWeakest;
        }

        if (normalizedTargetPolicyKey == PrototypeCombatTargetPolicyKeys.RandomLiving)
        {
            return PrototypeCombatEnemyPatternKeys.UnstableFocus;
        }

        return PrototypeCombatEnemyPatternKeys.FrontPressure;
    }

    public static string ResolvePatternHintLabel(string patternKey)
    {
        switch (string.IsNullOrWhiteSpace(patternKey) ? string.Empty : patternKey.Trim())
        {
            case PrototypeCombatEnemyPatternKeys.FocusWeakest:
                return "Focused execution";
            case PrototypeCombatEnemyPatternKeys.UnstableFocus:
                return "Unstable focus";
            case PrototypeCombatEnemyPatternKeys.EliteSweep:
                return "Party-wide surge";
            default:
                return "Frontline pressure";
        }
    }

    public static string BuildPreviewText(string actorDisplayName, string actorRoleLabel, string actionLabel, string targetPolicyKey, string targetDisplayName, string hintText, int powerValue, string patternKey)
    {
        string safeActorDisplayName = string.IsNullOrWhiteSpace(actorDisplayName) ? "Enemy" : actorDisplayName.Trim();
        string safeRoleLabel = string.IsNullOrWhiteSpace(actorRoleLabel) ? "Enemy" : actorRoleLabel.Trim();
        string safeActionLabel = string.IsNullOrWhiteSpace(actionLabel) ? "Attack" : actionLabel.Trim();
        string safeTargetDisplayName = string.IsNullOrWhiteSpace(targetDisplayName) ? "the party" : targetDisplayName.Trim();
        string normalizedTargetPolicyKey = PrototypeCombatTargetPolicy.NormalizeTargetPolicyKey(targetPolicyKey);

        string baseText = normalizedTargetPolicyKey == PrototypeCombatTargetPolicyKeys.AllLivingAllies
            ? safeActorDisplayName + " (" + safeRoleLabel + ") intends " + safeActionLabel + " on all living allies."
            : normalizedTargetPolicyKey == PrototypeCombatTargetPolicyKeys.LowestHpLiving
                ? safeActorDisplayName + " (" + safeRoleLabel + ") intends " + safeActionLabel + " on lowest HP target: " + safeTargetDisplayName + "."
                : normalizedTargetPolicyKey == PrototypeCombatTargetPolicyKeys.RandomLiving
                    ? safeActorDisplayName + " (" + safeRoleLabel + ") intends " + safeActionLabel + " on random target: " + safeTargetDisplayName + "."
                    : safeActorDisplayName + " (" + safeRoleLabel + ") intends " + safeActionLabel + " on " + safeTargetDisplayName + ".";

        string patternHint = ResolvePatternHintLabel(patternKey);
        string powerHint = powerValue > 0 ? " Power " + powerValue + "." : string.Empty;
        string extraHint = string.IsNullOrWhiteSpace(hintText) ? string.Empty : " " + hintText.Trim();
        return baseText + " " + patternHint + "." + powerHint + extraHint;
    }
}
