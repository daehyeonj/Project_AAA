using System.Collections.Generic;

public sealed class PrototypeRpgEnemyBehaviorHint
{
    public string BehaviorKey { get; }
    public string ShortLabel { get; }
    public string DisplayHintText { get; }

    public PrototypeRpgEnemyBehaviorHint(string behaviorKey, string shortLabel, string displayHintText)
    {
        BehaviorKey = string.IsNullOrWhiteSpace(behaviorKey) ? string.Empty : behaviorKey.Trim();
        ShortLabel = string.IsNullOrWhiteSpace(shortLabel) ? "Behavior" : shortLabel.Trim();
        DisplayHintText = string.IsNullOrWhiteSpace(displayHintText) ? string.Empty : displayHintText.Trim();
    }
}

public static class PrototypeRpgEnemyBehaviorHintCatalog
{
    private static readonly Dictionary<string, PrototypeRpgEnemyBehaviorHint> Definitions = new Dictionary<string, PrototypeRpgEnemyBehaviorHint>
    {
        ["behavior_front_guard"] = new PrototypeRpgEnemyBehaviorHint("behavior_front_guard", "Front Guard", "Frontline pressure"),
        ["behavior_flexible_flank"] = new PrototypeRpgEnemyBehaviorHint("behavior_flexible_flank", "Flexible Flank", "Flexible flank"),
        ["behavior_focused_execution"] = new PrototypeRpgEnemyBehaviorHint("behavior_focused_execution", "Focused Execution", "Focused execution"),
        ["behavior_unstable_pressure"] = new PrototypeRpgEnemyBehaviorHint("behavior_unstable_pressure", "Unstable Pressure", "Unstable focus"),
        ["behavior_royal_elite"] = new PrototypeRpgEnemyBehaviorHint("behavior_royal_elite", "Royal Elite", "Royal Slime Elite | Surging Pressure"),
        ["behavior_volatility_elite"] = new PrototypeRpgEnemyBehaviorHint("behavior_volatility_elite", "Volatile Elite", "Volatile Slime Elite | Surging Pressure"),
        ["behavior_command_elite"] = new PrototypeRpgEnemyBehaviorHint("behavior_command_elite", "Command Elite", "Goblin Elite | Focused Command"),
        ["behavior_execution_elite"] = new PrototypeRpgEnemyBehaviorHint("behavior_execution_elite", "Execution Elite", "Goblin Elite | Focused Execution")
    };

    public static PrototypeRpgEnemyBehaviorHint ResolveDefinition(string behaviorKey)
    {
        if (string.IsNullOrWhiteSpace(behaviorKey))
        {
            return null;
        }

        Definitions.TryGetValue(behaviorKey.Trim(), out PrototypeRpgEnemyBehaviorHint definition);
        return definition;
    }
}

public sealed class PrototypeRpgEnemyIntentDefinition
{
    public string IntentKey { get; }
    public string ShortLabel { get; }
    public string TargetPolicyKey { get; }
    public string EffectHintKey { get; }
    public string DisplayHintText { get; }
    public string ActionLabel { get; }

    public PrototypeRpgEnemyIntentDefinition(string intentKey, string shortLabel, string targetPolicyKey, string effectHintKey, string displayHintText, string actionLabel)
    {
        IntentKey = string.IsNullOrWhiteSpace(intentKey) ? string.Empty : intentKey.Trim();
        ShortLabel = string.IsNullOrWhiteSpace(shortLabel) ? "Intent" : shortLabel.Trim();
        TargetPolicyKey = string.IsNullOrWhiteSpace(targetPolicyKey) ? string.Empty : targetPolicyKey.Trim();
        EffectHintKey = string.IsNullOrWhiteSpace(effectHintKey) ? string.Empty : effectHintKey.Trim();
        DisplayHintText = string.IsNullOrWhiteSpace(displayHintText) ? string.Empty : displayHintText.Trim();
        ActionLabel = string.IsNullOrWhiteSpace(actionLabel) ? "Attack" : actionLabel.Trim();
    }
}

public static class PrototypeRpgEnemyIntentCatalog
{
    private static readonly Dictionary<string, PrototypeRpgEnemyIntentDefinition> Definitions = new Dictionary<string, PrototypeRpgEnemyIntentDefinition>
    {
        ["intent_frontline_pressure"] = new PrototypeRpgEnemyIntentDefinition("intent_frontline_pressure", "Frontline Pressure", "frontmost_living", "damage", "Frontline pressure.", "Attack"),
        ["intent_unstable_focus"] = new PrototypeRpgEnemyIntentDefinition("intent_unstable_focus", "Unstable Focus", "random_living", "damage", "Unstable focus.", "Attack"),
        ["intent_finish_weakest"] = new PrototypeRpgEnemyIntentDefinition("intent_finish_weakest", "Finish Weakest", "lowest_hp_living", "damage", "Pressure lowest HP.", "Attack"),
        ["intent_heavy_strike"] = new PrototypeRpgEnemyIntentDefinition("intent_heavy_strike", "Heavy Strike", "lowest_hp_living", "special_damage", "Heavy strike incoming.", "Heavy Strike"),
        ["intent_crushing_blow"] = new PrototypeRpgEnemyIntentDefinition("intent_crushing_blow", "Crushing Blow", "lowest_hp_living", "special_damage", "Crushing blow incoming.", "Crushing Blow"),
        ["intent_rending_blow"] = new PrototypeRpgEnemyIntentDefinition("intent_rending_blow", "Rending Blow", "lowest_hp_living", "special_damage", "Rending blow incoming.", "Rending Blow"),
        ["intent_execution_strike"] = new PrototypeRpgEnemyIntentDefinition("intent_execution_strike", "Execution Strike", "lowest_hp_living", "special_damage", "Focused execution incoming.", "Execution Strike"),
        ["intent_core_rupture"] = new PrototypeRpgEnemyIntentDefinition("intent_core_rupture", "Core Rupture", "lowest_hp_living", "special_damage", "Core rupture incoming.", "Core Rupture"),
        ["intent_royal_wave"] = new PrototypeRpgEnemyIntentDefinition("intent_royal_wave", "Royal Wave", "all_living_allies", "special_damage", "Royal wave incoming.", "Royal Wave"),
        ["intent_command_strike"] = new PrototypeRpgEnemyIntentDefinition("intent_command_strike", "Command Strike", "lowest_hp_living", "special_damage", "Command strike incoming.", "Command Strike")
    };

    public static PrototypeRpgEnemyIntentDefinition ResolveDefinition(string intentKey)
    {
        if (string.IsNullOrWhiteSpace(intentKey))
        {
            return null;
        }

        Definitions.TryGetValue(intentKey.Trim(), out PrototypeRpgEnemyIntentDefinition definition);
        return definition;
    }
}

public sealed class PrototypeRpgEnemyDefinition
{
    public string EnemyId { get; }
    public string DisplayName { get; }
    public string TypeLabel { get; }
    public string RoleTag { get; }
    public string RoleLabel { get; }
    public int MaxHp { get; }
    public int AttackPower { get; }
    public bool IsElite { get; }
    public string DefaultIntentKey { get; }
    public string SpecialIntentKey { get; }
    public string BehaviorHintKey { get; }
    public string RewardLabel { get; }
    public string RewardResourceId { get; }
    public int RewardAmountHint { get; }
    public string SpecialActionLabel { get; }
    public int SpecialPowerHint { get; }
    public string TraitText { get; }
    public PrototypeRpgEnemyBehaviorHint BehaviorHint => PrototypeRpgEnemyBehaviorHintCatalog.ResolveDefinition(BehaviorHintKey);
    public PrototypeRpgEnemyIntentDefinition DefaultIntentDefinition => PrototypeRpgEnemyIntentCatalog.ResolveDefinition(DefaultIntentKey);
    public PrototypeRpgEnemyIntentDefinition SpecialIntentDefinition => PrototypeRpgEnemyIntentCatalog.ResolveDefinition(SpecialIntentKey);

    public PrototypeRpgEnemyDefinition(string enemyId, string displayName, string typeLabel, string roleTag, string roleLabel, int maxHp, int attackPower, bool isElite, string defaultIntentKey, string specialIntentKey, string behaviorHintKey, string rewardLabel, string rewardResourceId, int rewardAmountHint, string specialActionLabel, int specialPowerHint, string traitText)
    {
        EnemyId = string.IsNullOrWhiteSpace(enemyId) ? string.Empty : enemyId.Trim();
        DisplayName = string.IsNullOrWhiteSpace(displayName) ? "Monster" : displayName.Trim();
        TypeLabel = string.IsNullOrWhiteSpace(typeLabel) ? "Monster" : typeLabel.Trim();
        RoleTag = string.IsNullOrWhiteSpace(roleTag) ? "bulwark" : roleTag.Trim();
        RoleLabel = string.IsNullOrWhiteSpace(roleLabel) ? "Bulwark" : roleLabel.Trim();
        MaxHp = maxHp > 0 ? maxHp : 1;
        AttackPower = attackPower > 0 ? attackPower : 1;
        IsElite = isElite;
        DefaultIntentKey = string.IsNullOrWhiteSpace(defaultIntentKey) ? string.Empty : defaultIntentKey.Trim();
        SpecialIntentKey = string.IsNullOrWhiteSpace(specialIntentKey) ? string.Empty : specialIntentKey.Trim();
        BehaviorHintKey = string.IsNullOrWhiteSpace(behaviorHintKey) ? string.Empty : behaviorHintKey.Trim();
        RewardLabel = string.IsNullOrWhiteSpace(rewardLabel) ? string.Empty : rewardLabel.Trim();
        RewardResourceId = string.IsNullOrWhiteSpace(rewardResourceId) ? string.Empty : rewardResourceId.Trim();
        RewardAmountHint = rewardAmountHint > 0 ? rewardAmountHint : 0;
        SpecialActionLabel = string.IsNullOrWhiteSpace(specialActionLabel) ? string.Empty : specialActionLabel.Trim();
        SpecialPowerHint = specialPowerHint > 0 ? specialPowerHint : 0;
        TraitText = string.IsNullOrWhiteSpace(traitText) ? string.Empty : traitText.Trim();
    }
}

public static class PrototypeRpgEnemyCatalog
{
    private static readonly Dictionary<string, PrototypeRpgEnemyDefinition> Definitions = new Dictionary<string, PrototypeRpgEnemyDefinition>
    {
        ["alpha-safe-room1-slime-bulwark-a"] = new PrototypeRpgEnemyDefinition("alpha-safe-room1-slime-bulwark-a", "Slime A", "Slime", "bulwark", "Bulwark", 19, 2, false, "intent_unstable_focus", string.Empty, "behavior_unstable_pressure", "Slime Residue", "mana_shard", 2, string.Empty, 0, "Unstable focus"),
        ["alpha-safe-room1-slime-bulwark-b"] = new PrototypeRpgEnemyDefinition("alpha-safe-room1-slime-bulwark-b", "Slime B", "Slime", "bulwark", "Bulwark", 18, 2, false, "intent_unstable_focus", string.Empty, "behavior_unstable_pressure", "Slime Residue", "mana_shard", 2, string.Empty, 0, "Unstable focus"),
        ["alpha-safe-room2-slime-bulwark"] = new PrototypeRpgEnemyDefinition("alpha-safe-room2-slime-bulwark", "Slime C", "Slime", "bulwark", "Bulwark", 18, 2, false, "intent_unstable_focus", string.Empty, "behavior_unstable_pressure", "Slime Residue", "mana_shard", 2, string.Empty, 0, "Unstable focus"),
        ["alpha-safe-room2-slime-skirmisher"] = new PrototypeRpgEnemyDefinition("alpha-safe-room2-slime-skirmisher", "Slime D", "Slime", "skirmisher", "Skirmisher", 12, 3, false, "intent_finish_weakest", string.Empty, "behavior_flexible_flank", "Slime Residue", "mana_shard", 2, string.Empty, 0, "Pressure lowest HP"),
        ["alpha-safe-elite"] = new PrototypeRpgEnemyDefinition("alpha-safe-elite", "Slime Monarch", "Slime", "elite_bulwark", "Elite", 34, 5, true, "intent_unstable_focus", "intent_royal_wave", "behavior_royal_elite", "Royal Gel Cache", "mana_shard", 6, "Royal Wave", 9, "Royal Slime Elite | Surging Pressure"),
        ["alpha-risky-room1-slime-bulwark"] = new PrototypeRpgEnemyDefinition("alpha-risky-room1-slime-bulwark", "Slime A", "Slime", "bulwark", "Bulwark", 18, 2, false, "intent_unstable_focus", string.Empty, "behavior_unstable_pressure", "Slime Residue", "mana_shard", 2, string.Empty, 0, "Unstable focus"),
        ["alpha-risky-room1-goblin-striker"] = new PrototypeRpgEnemyDefinition("alpha-risky-room1-goblin-striker", "Goblin A", "Goblin", "striker", "Striker", 14, 3, false, "intent_finish_weakest", "intent_heavy_strike", "behavior_focused_execution", "Goblin Trophies", "mana_shard", 3, "Heavy Strike", 7, "Focused execution"),
        ["alpha-risky-room2-goblin-skirmisher"] = new PrototypeRpgEnemyDefinition("alpha-risky-room2-goblin-skirmisher", "Goblin B", "Goblin", "skirmisher", "Skirmisher", 12, 4, false, "intent_finish_weakest", string.Empty, "behavior_flexible_flank", "Goblin Trophies", "mana_shard", 2, string.Empty, 0, "Pressure lowest HP"),
        ["alpha-risky-room2-goblin-striker"] = new PrototypeRpgEnemyDefinition("alpha-risky-room2-goblin-striker", "Goblin C", "Goblin", "striker", "Striker", 14, 4, false, "intent_finish_weakest", "intent_rending_blow", "behavior_focused_execution", "Goblin Trophies", "mana_shard", 2, "Rending Blow", 8, "Focused execution"),
        ["alpha-risky-elite"] = new PrototypeRpgEnemyDefinition("alpha-risky-elite", "Gel Core", "Slime", "elite_striker", "Elite", 30, 5, true, "intent_finish_weakest", "intent_core_rupture", "behavior_volatility_elite", "Volatile Core Cache", "mana_shard", 8, "Core Rupture", 10, "Volatile Slime Elite | Surging Pressure"),
        ["beta-safe-room1-slime-bulwark"] = new PrototypeRpgEnemyDefinition("beta-safe-room1-slime-bulwark", "Slime Scout", "Slime", "bulwark", "Bulwark", 18, 2, false, "intent_unstable_focus", string.Empty, "behavior_unstable_pressure", "Slime Residue", "mana_shard", 2, string.Empty, 0, "Unstable focus"),
        ["beta-safe-room1-goblin-skirmisher"] = new PrototypeRpgEnemyDefinition("beta-safe-room1-goblin-skirmisher", "Goblin Watch", "Goblin", "skirmisher", "Skirmisher", 12, 4, false, "intent_finish_weakest", string.Empty, "behavior_flexible_flank", "Goblin Trophies", "mana_shard", 3, string.Empty, 0, "Pressure lowest HP"),
        ["beta-safe-room2-goblin-bulwark"] = new PrototypeRpgEnemyDefinition("beta-safe-room2-goblin-bulwark", "Goblin Guard", "Goblin", "bulwark", "Bulwark", 20, 3, false, "intent_frontline_pressure", string.Empty, "behavior_front_guard", "Goblin Trophies", "mana_shard", 3, string.Empty, 0, "Frontline pressure"),
        ["beta-safe-room2-goblin-skirmisher"] = new PrototypeRpgEnemyDefinition("beta-safe-room2-goblin-skirmisher", "Goblin Raider", "Goblin", "skirmisher", "Skirmisher", 13, 4, false, "intent_finish_weakest", string.Empty, "behavior_flexible_flank", "Goblin Trophies", "mana_shard", 3, string.Empty, 0, "Pressure lowest HP"),
        ["beta-safe-elite"] = new PrototypeRpgEnemyDefinition("beta-safe-elite", "Goblin Captain", "Goblin", "elite_bulwark", "Elite", 32, 5, true, "intent_finish_weakest", "intent_command_strike", "behavior_command_elite", "Captain's Stash", "mana_shard", 9, "Command Strike", 9, "Goblin Elite | Focused Command"),
        ["beta-risky-room1-goblin-skirmisher"] = new PrototypeRpgEnemyDefinition("beta-risky-room1-goblin-skirmisher", "Goblin Scout A", "Goblin", "skirmisher", "Skirmisher", 12, 4, false, "intent_finish_weakest", string.Empty, "behavior_flexible_flank", "Goblin Trophies", "mana_shard", 3, string.Empty, 0, "Pressure lowest HP"),
        ["beta-risky-room1-goblin-striker"] = new PrototypeRpgEnemyDefinition("beta-risky-room1-goblin-striker", "Goblin Striker A", "Goblin", "striker", "Striker", 14, 3, false, "intent_finish_weakest", "intent_heavy_strike", "behavior_focused_execution", "Goblin Trophies", "mana_shard", 4, "Heavy Strike", 7, "Focused execution"),
        ["beta-risky-room2-goblin-bulwark"] = new PrototypeRpgEnemyDefinition("beta-risky-room2-goblin-bulwark", "Goblin Guard", "Goblin", "bulwark", "Bulwark", 21, 3, false, "intent_frontline_pressure", string.Empty, "behavior_front_guard", "Goblin Trophies", "mana_shard", 3, string.Empty, 0, "Frontline pressure"),
        ["beta-risky-room2-goblin-striker"] = new PrototypeRpgEnemyDefinition("beta-risky-room2-goblin-striker", "Goblin Striker B", "Goblin", "striker", "Striker", 15, 4, false, "intent_finish_weakest", "intent_crushing_blow", "behavior_focused_execution", "Goblin Trophies", "mana_shard", 4, "Crushing Blow", 8, "Focused execution"),
        ["beta-risky-elite"] = new PrototypeRpgEnemyDefinition("beta-risky-elite", "Raider Chief", "Goblin", "elite_striker", "Elite", 36, 6, true, "intent_finish_weakest", "intent_execution_strike", "behavior_execution_elite", "Raider War Spoils", "mana_shard", 11, "Execution Strike", 11, "Goblin Elite | Focused Execution")
    };

    public static PrototypeRpgEnemyDefinition ResolveDefinition(string enemyId)
    {
        if (string.IsNullOrWhiteSpace(enemyId))
        {
            return null;
        }

        Definitions.TryGetValue(enemyId.Trim(), out PrototypeRpgEnemyDefinition definition);
        return definition;
    }

    public static PrototypeRpgEnemyDefinition BuildFallbackDefinition(string enemyId, string displayName, string typeLabel, string roleTag, string roleLabel, int maxHp, int attackPower, bool isElite, string defaultIntentKey, string specialIntentKey, string behaviorHintKey, string rewardLabel, string rewardResourceId, int rewardAmountHint, string specialActionLabel, int specialPowerHint, string traitText)
    {
        return new PrototypeRpgEnemyDefinition(enemyId, displayName, typeLabel, roleTag, roleLabel, maxHp, attackPower, isElite, defaultIntentKey, specialIntentKey, behaviorHintKey, rewardLabel, rewardResourceId, rewardAmountHint, specialActionLabel, specialPowerHint, traitText);
    }
}