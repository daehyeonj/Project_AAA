using System;

public static class PrototypeCombatTargetPolicyKeys
{
    public const string FrontmostLiving = "frontmost_living";
    public const string RandomLiving = "random_living";
    public const string LowestHpLiving = "lowest_hp_living";
    public const string AllLivingAllies = "all_living_allies";
}

public static class PrototypeCombatTargetKindKeys
{
    public const string SingleEnemy = "single_enemy";
    public const string AllEnemies = "all_enemies";
    public const string AllAllies = "all_allies";
    public const string Party = "party";
}

public static class PrototypeCombatTargetPolicy
{
    public static string NormalizeTargetPolicyKey(string targetPolicyKey, bool isPartyWideAction = false)
    {
        if (isPartyWideAction)
        {
            return PrototypeCombatTargetPolicyKeys.AllLivingAllies;
        }

        string normalized = string.IsNullOrWhiteSpace(targetPolicyKey) ? string.Empty : targetPolicyKey.Trim();
        switch (normalized)
        {
            case PrototypeCombatTargetPolicyKeys.RandomLiving:
            case PrototypeCombatTargetPolicyKeys.LowestHpLiving:
            case PrototypeCombatTargetPolicyKeys.AllLivingAllies:
                return normalized;
            default:
                return PrototypeCombatTargetPolicyKeys.FrontmostLiving;
        }
    }

    public static string NormalizeTargetKind(string targetKind)
    {
        string normalized = string.IsNullOrWhiteSpace(targetKind) ? string.Empty : targetKind.Trim();
        switch (normalized)
        {
            case PrototypeCombatTargetKindKeys.AllEnemies:
            case PrototypeCombatTargetKindKeys.AllAllies:
            case PrototypeCombatTargetKindKeys.Party:
                return normalized;
            default:
                return PrototypeCombatTargetKindKeys.SingleEnemy;
        }
    }

    public static bool RequiresTargetSelection(string targetKind)
    {
        return NormalizeTargetKind(targetKind) == PrototypeCombatTargetKindKeys.SingleEnemy;
    }

    public static int ResolveLivingTargetIndex(string targetPolicyKey, int[] livingIndices, int[] currentHpValues, int randomRoll)
    {
        if (livingIndices == null || currentHpValues == null || livingIndices.Length == 0 || currentHpValues.Length == 0)
        {
            return -1;
        }

        int count = Math.Min(livingIndices.Length, currentHpValues.Length);
        if (count <= 0)
        {
            return -1;
        }

        string normalizedPolicyKey = NormalizeTargetPolicyKey(targetPolicyKey);
        if (normalizedPolicyKey == PrototypeCombatTargetPolicyKeys.RandomLiving)
        {
            int safeRoll = randomRoll < 0 ? -randomRoll : randomRoll;
            return livingIndices[safeRoll % count];
        }

        if (normalizedPolicyKey == PrototypeCombatTargetPolicyKeys.LowestHpLiving)
        {
            int bestIndex = livingIndices[0];
            int lowestHp = currentHpValues[0];
            for (int i = 1; i < count; i++)
            {
                if (currentHpValues[i] < lowestHp)
                {
                    lowestHp = currentHpValues[i];
                    bestIndex = livingIndices[i];
                }
            }

            return bestIndex;
        }

        return livingIndices[0];
    }
}
