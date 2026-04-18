using System;
using System.Collections.Generic;

public sealed class PrototypeRpgLevelStatBonus
{
    public int MaxHpBonus;
    public int AttackBonus;
    public int DefenseBonus;
    public int SpeedBonus;

    public bool HasAnyBonus =>
        MaxHpBonus != 0 ||
        AttackBonus != 0 ||
        DefenseBonus != 0 ||
        SpeedBonus != 0;
}

public sealed class PrototypeRpgRewardBundle
{
    public string RewardId = string.Empty;
    public string RewardLabel = "None";
    public int Amount;
}

public sealed class PrototypeRpgMemberProgressionResult
{
    public string MemberId = string.Empty;
    public string DisplayName = "Adventurer";
    public string RoleTag = "adventurer";
    public string RoleLabel = "Adventurer";
    public int LevelBefore = 1;
    public int LevelAfter = 1;
    public int ExperienceBefore;
    public int ExperienceAfter;
    public int ExperienceGained;
    public int NextLevelExperience = 18;
    public int GrowthBonusMaxHp;
    public int GrowthBonusAttack;
    public int GrowthBonusDefense;
    public int GrowthBonusSpeed;
    public bool LeveledUp;
    public string GrowthSummaryText = "None";
    public string RewardDropSummaryText = "None";
    public PrototypeRpgRewardBundle[] RewardBundles = Array.Empty<PrototypeRpgRewardBundle>();
}

public static class PrototypeRpgMemberProgressionRules
{
    private const int StartingLevel = 1;
    private const int StartingExperience = 0;

    public static int GetStartingLevel()
    {
        return StartingLevel;
    }

    public static int GetStartingExperience()
    {
        return StartingExperience;
    }

    public static int GetNextLevelExperience(int level)
    {
        int safeLevel = level > 0 ? level : StartingLevel;
        return 18 + ((safeLevel - 1) * 8);
    }

    public static PrototypeRpgLevelStatBonus ResolveLevelStatBonus(string roleTag, string archetypeId, int level)
    {
        int safeLevel = level > 0 ? level : StartingLevel;
        PrototypeRpgLevelStatBonus bonus = new PrototypeRpgLevelStatBonus();
        for (int currentLevel = 2; currentLevel <= safeLevel; currentLevel++)
        {
            ApplyRoleGrowth(roleTag, currentLevel, bonus);
            ApplyArchetypeBias(archetypeId, roleTag, currentLevel, bonus);
        }

        return bonus;
    }

    public static int CalculateExperienceReward(
        string roleTag,
        string routeId,
        bool success,
        bool survived,
        bool knockedOut,
        bool eliteDefeated,
        int totalLootGained,
        PrototypeRpgCombatContributionSeed contribution)
    {
        PrototypeRpgCombatContributionSeed safeContribution = contribution ?? new PrototypeRpgCombatContributionSeed();
        int experience = success ? 10 : 4;
        experience += Clamp(safeContribution.ActionCount, 0, 4);
        experience += survived ? 3 : 0;
        experience -= knockedOut ? 1 : 0;

        switch (Normalize(roleTag))
        {
            case "warrior":
                experience += Clamp(safeContribution.DamageTaken / 4, 0, 6);
                experience += Clamp(safeContribution.DamageDealt / 10, 0, 3);
                break;

            case "rogue":
                experience += Clamp(safeContribution.DamageDealt / 6, 0, 6);
                experience += Clamp(safeContribution.KillCount * 2, 0, 6);
                break;

            case "mage":
                experience += Clamp(safeContribution.DamageDealt / 5, 0, 7);
                break;

            case "cleric":
                experience += Clamp(safeContribution.HealingDone / 3, 0, 7);
                experience += Clamp(safeContribution.DamageDealt / 12, 0, 2);
                break;

            default:
                experience += Clamp(safeContribution.DamageDealt / 8, 0, 4);
                break;
        }

        if (eliteDefeated && survived)
        {
            experience += 5;
        }

        if (Normalize(routeId) == "risky")
        {
            experience += 2;
        }

        experience += Clamp(totalLootGained / 3, 0, 3);
        return experience > 0 ? experience : 1;
    }

    public static PrototypeRpgMemberProgressionResult ResolveMemberProgression(
        string memberId,
        string displayName,
        string roleTag,
        string roleLabel,
        string archetypeId,
        string routeId,
        string dungeonId,
        bool success,
        bool survived,
        bool knockedOut,
        bool eliteDefeated,
        int defeatedEnemyCount,
        int openedChestCount,
        int totalTurnsTaken,
        int totalLootGained,
        int levelBefore,
        int experienceBefore,
        PrototypeRpgCombatContributionSeed contribution,
        int memberIndex)
    {
        int safeLevelBefore = levelBefore > 0 ? levelBefore : StartingLevel;
        int safeExperienceBefore = experienceBefore > 0 ? experienceBefore : StartingExperience;
        int experienceGained = CalculateExperienceReward(
            roleTag,
            routeId,
            success,
            survived,
            knockedOut,
            eliteDefeated,
            totalLootGained,
            contribution);
        int currentLevel = safeLevelBefore;
        int experiencePool = safeExperienceBefore + experienceGained;

        while (experiencePool >= GetNextLevelExperience(currentLevel))
        {
            experiencePool -= GetNextLevelExperience(currentLevel);
            currentLevel += 1;
        }

        PrototypeRpgLevelStatBonus bonus = ResolveLevelStatBonus(roleTag, archetypeId, currentLevel);
        PrototypeRpgRewardBundle[] rewardBundles = BuildRewardBundles(
            roleTag,
            routeId,
            dungeonId,
            success,
            survived,
            eliteDefeated,
            defeatedEnemyCount,
            openedChestCount,
            contribution,
            totalTurnsTaken,
            memberIndex);

        PrototypeRpgMemberProgressionResult result = new PrototypeRpgMemberProgressionResult();
        result.MemberId = string.IsNullOrWhiteSpace(memberId) ? string.Empty : memberId.Trim();
        result.DisplayName = string.IsNullOrWhiteSpace(displayName) ? "Adventurer" : displayName.Trim();
        result.RoleTag = string.IsNullOrWhiteSpace(roleTag) ? "adventurer" : roleTag.Trim().ToLowerInvariant();
        result.RoleLabel = string.IsNullOrWhiteSpace(roleLabel) ? "Adventurer" : roleLabel.Trim();
        result.LevelBefore = safeLevelBefore;
        result.LevelAfter = currentLevel;
        result.ExperienceBefore = safeExperienceBefore;
        result.ExperienceAfter = experiencePool;
        result.ExperienceGained = experienceGained;
        result.NextLevelExperience = GetNextLevelExperience(currentLevel);
        result.GrowthBonusMaxHp = bonus.MaxHpBonus;
        result.GrowthBonusAttack = bonus.AttackBonus;
        result.GrowthBonusDefense = bonus.DefenseBonus;
        result.GrowthBonusSpeed = bonus.SpeedBonus;
        result.LeveledUp = currentLevel > safeLevelBefore;
        result.GrowthSummaryText = BuildGrowthSummaryText(result);
        result.RewardBundles = rewardBundles;
        result.RewardDropSummaryText = BuildRewardBundleSummaryText(rewardBundles);
        return result;
    }

    public static PrototypeRpgRewardBundle[] AggregateRewardBundles(PrototypeRpgMemberProgressionResult[] memberResults)
    {
        PrototypeRpgMemberProgressionResult[] safeResults = memberResults ?? Array.Empty<PrototypeRpgMemberProgressionResult>();
        Dictionary<string, PrototypeRpgRewardBundle> aggregated = new Dictionary<string, PrototypeRpgRewardBundle>();

        for (int i = 0; i < safeResults.Length; i++)
        {
            PrototypeRpgRewardBundle[] bundles = safeResults[i] != null
                ? safeResults[i].RewardBundles ?? Array.Empty<PrototypeRpgRewardBundle>()
                : Array.Empty<PrototypeRpgRewardBundle>();
            for (int bundleIndex = 0; bundleIndex < bundles.Length; bundleIndex++)
            {
                PrototypeRpgRewardBundle bundle = bundles[bundleIndex];
                if (bundle == null || string.IsNullOrWhiteSpace(bundle.RewardId) || bundle.Amount <= 0)
                {
                    continue;
                }

                string rewardId = bundle.RewardId.Trim().ToLowerInvariant();
                if (!aggregated.TryGetValue(rewardId, out PrototypeRpgRewardBundle aggregate))
                {
                    aggregate = new PrototypeRpgRewardBundle
                    {
                        RewardId = rewardId,
                        RewardLabel = string.IsNullOrWhiteSpace(bundle.RewardLabel) ? rewardId : bundle.RewardLabel.Trim(),
                        Amount = 0
                    };
                    aggregated.Add(rewardId, aggregate);
                }

                aggregate.Amount += bundle.Amount;
            }
        }

        if (aggregated.Count <= 0)
        {
            return Array.Empty<PrototypeRpgRewardBundle>();
        }

        PrototypeRpgRewardBundle[] results = new PrototypeRpgRewardBundle[aggregated.Count];
        int writeIndex = 0;
        foreach (KeyValuePair<string, PrototypeRpgRewardBundle> pair in aggregated)
        {
            results[writeIndex++] = new PrototypeRpgRewardBundle
            {
                RewardId = pair.Value.RewardId,
                RewardLabel = pair.Value.RewardLabel,
                Amount = pair.Value.Amount
            };
        }

        return results;
    }

    public static string BuildRewardBundleSummaryText(PrototypeRpgRewardBundle[] bundles)
    {
        PrototypeRpgRewardBundle[] safeBundles = bundles ?? Array.Empty<PrototypeRpgRewardBundle>();
        List<string> parts = new List<string>();
        for (int i = 0; i < safeBundles.Length; i++)
        {
            PrototypeRpgRewardBundle bundle = safeBundles[i];
            if (bundle == null || bundle.Amount <= 0)
            {
                continue;
            }

            string rewardLabel = string.IsNullOrWhiteSpace(bundle.RewardLabel) ? bundle.RewardId : bundle.RewardLabel;
            if (string.IsNullOrWhiteSpace(rewardLabel))
            {
                continue;
            }

            parts.Add(rewardLabel.Trim() + " x" + bundle.Amount);
        }

        return parts.Count > 0 ? string.Join(" | ", parts.ToArray()) : "None";
    }

    public static string BuildGrowthBonusSummaryText(int maxHpBonus, int attackBonus, int defenseBonus, int speedBonus)
    {
        List<string> parts = new List<string>();
        AddSignedPart(parts, "HP", maxHpBonus);
        AddSignedPart(parts, "ATK", attackBonus);
        AddSignedPart(parts, "DEF", defenseBonus);
        AddSignedPart(parts, "SPD", speedBonus);
        return parts.Count > 0 ? string.Join(" ", parts.ToArray()) : "No bonus";
    }

    public static string BuildLevelProgressText(int level, int currentExperience, int nextLevelExperience)
    {
        int safeLevel = level > 0 ? level : StartingLevel;
        int safeCurrentExperience = currentExperience > 0 ? currentExperience : 0;
        int safeNextLevelExperience = nextLevelExperience > 0 ? nextLevelExperience : GetNextLevelExperience(safeLevel);
        return "Lv " + safeLevel + " | XP " + safeCurrentExperience + "/" + safeNextLevelExperience;
    }

    public static string BuildNextLevelHintText(int level, int currentExperience, int nextLevelExperience)
    {
        int safeLevel = level > 0 ? level : StartingLevel;
        int safeCurrentExperience = currentExperience > 0 ? currentExperience : 0;
        int safeNextLevelExperience = nextLevelExperience > 0 ? nextLevelExperience : GetNextLevelExperience(safeLevel);
        int remainingExperience = safeNextLevelExperience - safeCurrentExperience;
        if (remainingExperience < 0)
        {
            remainingExperience = 0;
        }

        return remainingExperience + " XP to Lv " + (safeLevel + 1);
    }

    private static string BuildGrowthSummaryText(PrototypeRpgMemberProgressionResult result)
    {
        if (result == null)
        {
            return "None";
        }

        string growthBonusText = BuildGrowthBonusSummaryText(
            result.GrowthBonusMaxHp,
            result.GrowthBonusAttack,
            result.GrowthBonusDefense,
            result.GrowthBonusSpeed);
        string levelText = result.LevelBefore != result.LevelAfter
            ? "Lv " + result.LevelBefore + " -> " + result.LevelAfter
            : "Lv " + result.LevelAfter;
        return levelText +
               " | XP +" + result.ExperienceGained +
               " (" + result.ExperienceAfter + "/" + result.NextLevelExperience + ")" +
               " | " + growthBonusText;
    }

    private static PrototypeRpgRewardBundle[] BuildRewardBundles(
        string roleTag,
        string routeId,
        string dungeonId,
        bool success,
        bool survived,
        bool eliteDefeated,
        int defeatedEnemyCount,
        int openedChestCount,
        PrototypeRpgCombatContributionSeed contribution,
        int totalTurnsTaken,
        int memberIndex)
    {
        if (!success)
        {
            return Array.Empty<PrototypeRpgRewardBundle>();
        }

        PrototypeRpgCombatContributionSeed safeContribution = contribution ?? new PrototypeRpgCombatContributionSeed();
        List<PrototypeRpgRewardBundle> bundles = new List<PrototypeRpgRewardBundle>();
        int seed = BuildStableSeed(roleTag, routeId, dungeonId, totalTurnsTaken, defeatedEnemyCount, openedChestCount, safeContribution, memberIndex);
        int baseAmount = 1 + (survived ? 1 : 0);

        switch (Normalize(roleTag))
        {
            case "warrior":
                baseAmount += Clamp(safeContribution.DamageTaken / 10, 0, 1);
                bundles.Add(CreateRewardBundle("gear_fragment", "Gear Fragment", baseAmount));
                break;

            case "rogue":
                baseAmount += Clamp(safeContribution.KillCount, 0, 2);
                bundles.Add(CreateRewardBundle("pressure_token", "Pressure Token", baseAmount));
                break;

            case "mage":
                baseAmount += Clamp(safeContribution.DamageDealt / 12, 0, 2);
                bundles.Add(CreateRewardBundle("arcane_shard", "Arcane Shard", baseAmount));
                break;

            case "cleric":
                baseAmount += Clamp(safeContribution.HealingDone / 6, 0, 2);
                bundles.Add(CreateRewardBundle("recovery_token", "Recovery Token", baseAmount));
                break;

            default:
                bundles.Add(CreateRewardBundle("field_scrap", "Field Scrap", baseAmount));
                break;
        }

        if (eliteDefeated || Normalize(routeId) == "risky")
        {
            int essenceRoll = PositiveModulo(seed, 3);
            if (essenceRoll > 0)
            {
                bundles.Add(CreateRewardBundle("volatile_essence", "Volatile Essence", essenceRoll));
            }
        }

        if (openedChestCount > 0)
        {
            int chestBonus = 1 + PositiveModulo(seed / 3, 2);
            bundles.Add(CreateRewardBundle("cache_token", "Cache Token", chestBonus));
        }

        return bundles.ToArray();
    }

    private static PrototypeRpgRewardBundle CreateRewardBundle(string rewardId, string rewardLabel, int amount)
    {
        return new PrototypeRpgRewardBundle
        {
            RewardId = string.IsNullOrWhiteSpace(rewardId) ? string.Empty : rewardId.Trim().ToLowerInvariant(),
            RewardLabel = string.IsNullOrWhiteSpace(rewardLabel) ? "None" : rewardLabel.Trim(),
            Amount = amount > 0 ? amount : 0
        };
    }

    private static void ApplyRoleGrowth(string roleTag, int level, PrototypeRpgLevelStatBonus bonus)
    {
        if (bonus == null)
        {
            return;
        }

        switch (Normalize(roleTag))
        {
            case "warrior":
                bonus.MaxHpBonus += 3;
                bonus.AttackBonus += 1;
                bonus.DefenseBonus += 1;
                if (level % 3 == 0)
                {
                    bonus.SpeedBonus += 1;
                }
                break;

            case "rogue":
                bonus.MaxHpBonus += 2;
                bonus.AttackBonus += 1;
                bonus.SpeedBonus += 1;
                if (level % 3 == 0)
                {
                    bonus.DefenseBonus += 1;
                }
                break;

            case "mage":
                bonus.MaxHpBonus += 1;
                bonus.AttackBonus += 2;
                if (level % 2 == 0)
                {
                    bonus.SpeedBonus += 1;
                }
                break;

            case "cleric":
                bonus.MaxHpBonus += 2;
                bonus.DefenseBonus += 1;
                if (level % 2 == 0)
                {
                    bonus.AttackBonus += 1;
                }
                if (level % 3 == 0)
                {
                    bonus.SpeedBonus += 1;
                }
                break;

            default:
                bonus.MaxHpBonus += 1;
                bonus.AttackBonus += 1;
                break;
        }
    }

    private static void ApplyArchetypeBias(string archetypeId, string roleTag, int level, PrototypeRpgLevelStatBonus bonus)
    {
        if (bonus == null)
        {
            return;
        }

        switch (Normalize(archetypeId))
        {
            case "outrider":
                switch (Normalize(roleTag))
                {
                    case "warrior":
                        if (level % 2 == 0)
                        {
                            bonus.AttackBonus += 1;
                        }
                        break;

                    case "rogue":
                        if (level % 2 == 0)
                        {
                            bonus.SpeedBonus += 1;
                        }
                        break;

                    case "mage":
                        if (level % 2 == 0)
                        {
                            bonus.AttackBonus += 1;
                        }
                        break;

                    case "cleric":
                        if (level % 3 == 0)
                        {
                            bonus.SpeedBonus += 1;
                        }
                        break;
                }
                break;

            case "salvager":
                switch (Normalize(roleTag))
                {
                    case "warrior":
                        bonus.MaxHpBonus += 1;
                        break;

                    case "rogue":
                        if (level % 3 == 0)
                        {
                            bonus.DefenseBonus += 1;
                        }
                        break;

                    case "mage":
                        if (level % 2 == 0)
                        {
                            bonus.MaxHpBonus += 1;
                        }
                        break;

                    case "cleric":
                        bonus.MaxHpBonus += 1;
                        break;
                }
                break;

            default:
                switch (Normalize(roleTag))
                {
                    case "warrior":
                        if (level % 2 == 0)
                        {
                            bonus.DefenseBonus += 1;
                        }
                        break;

                    case "rogue":
                        if (level % 3 == 0)
                        {
                            bonus.MaxHpBonus += 1;
                        }
                        break;

                    case "mage":
                        if (level % 3 == 0)
                        {
                            bonus.AttackBonus += 1;
                        }
                        break;

                    case "cleric":
                        if (level % 2 == 0)
                        {
                            bonus.DefenseBonus += 1;
                        }
                        break;
                }
                break;
        }
    }

    private static int BuildStableSeed(
        string roleTag,
        string routeId,
        string dungeonId,
        int totalTurnsTaken,
        int defeatedEnemyCount,
        int openedChestCount,
        PrototypeRpgCombatContributionSeed contribution,
        int memberIndex)
    {
        PrototypeRpgCombatContributionSeed safeContribution = contribution ?? new PrototypeRpgCombatContributionSeed();
        int seed = 17;
        seed = MixSeed(seed, roleTag);
        seed = MixSeed(seed, routeId);
        seed = MixSeed(seed, dungeonId);
        seed = (seed * 31) + totalTurnsTaken;
        seed = (seed * 31) + defeatedEnemyCount;
        seed = (seed * 31) + openedChestCount;
        seed = (seed * 31) + safeContribution.DamageDealt;
        seed = (seed * 31) + safeContribution.DamageTaken;
        seed = (seed * 31) + safeContribution.HealingDone;
        seed = (seed * 31) + safeContribution.ActionCount;
        seed = (seed * 31) + safeContribution.KillCount;
        seed = (seed * 31) + memberIndex;
        return seed;
    }

    private static int MixSeed(int seed, string text)
    {
        string safeText = string.IsNullOrWhiteSpace(text) ? string.Empty : text.Trim().ToLowerInvariant();
        unchecked
        {
            for (int i = 0; i < safeText.Length; i++)
            {
                seed = (seed * 31) + safeText[i];
            }
        }

        return seed;
    }

    private static int Clamp(int value, int minimum, int maximum)
    {
        if (value < minimum)
        {
            return minimum;
        }

        return value > maximum ? maximum : value;
    }

    private static int PositiveModulo(int value, int modulo)
    {
        if (modulo <= 0)
        {
            return 0;
        }

        int remainder = value % modulo;
        return remainder < 0 ? remainder + modulo : remainder;
    }

    private static void AddSignedPart(List<string> parts, string label, int value)
    {
        if (parts == null || value == 0)
        {
            return;
        }

        parts.Add(label + " " + (value > 0 ? "+" : string.Empty) + value);
    }

    private static string Normalize(string value)
    {
        return string.IsNullOrWhiteSpace(value) ? string.Empty : value.Trim().ToLowerInvariant();
    }
}
