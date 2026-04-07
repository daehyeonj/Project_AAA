using System;

public sealed class PrototypeRpgEnemySkillDefinition
{
    public string SkillId { get; }
    public string DisplayName { get; }
    public string TargetKind { get; }
    public string EffectType { get; }
    public int PowerValue { get; }
    public int CooldownTurns { get; }
    public int EncounterCharges { get; }
    public string IntentLabel { get; }
    public string ActionBiasKey { get; }
    public string PredictedStatusText { get; }
    public string SummaryText { get; }

    public PrototypeRpgEnemySkillDefinition(
        string skillId,
        string displayName,
        string targetKind,
        string effectType,
        int powerValue,
        int cooldownTurns,
        int encounterCharges,
        string intentLabel,
        string actionBiasKey,
        string predictedStatusText,
        string summaryText)
    {
        SkillId = string.IsNullOrWhiteSpace(skillId) ? string.Empty : skillId.Trim();
        DisplayName = string.IsNullOrWhiteSpace(displayName) ? "Enemy Skill" : displayName.Trim();
        TargetKind = string.IsNullOrWhiteSpace(targetKind) ? "single_party_member" : targetKind.Trim();
        EffectType = string.IsNullOrWhiteSpace(effectType) ? "damage" : effectType.Trim();
        PowerValue = powerValue >= 0 ? powerValue : 0;
        CooldownTurns = cooldownTurns >= 0 ? cooldownTurns : 0;
        EncounterCharges = encounterCharges >= 0 ? encounterCharges : 0;
        IntentLabel = string.IsNullOrWhiteSpace(intentLabel) ? DisplayName : intentLabel.Trim();
        ActionBiasKey = string.IsNullOrWhiteSpace(actionBiasKey) ? "basic_attack" : actionBiasKey.Trim();
        PredictedStatusText = string.IsNullOrWhiteSpace(predictedStatusText) ? string.Empty : predictedStatusText.Trim();
        SummaryText = string.IsNullOrWhiteSpace(summaryText) ? IntentLabel : summaryText.Trim();
    }
}

public sealed class PrototypeRpgEnemySkillLoadoutDefinition
{
    public string LoadoutId { get; }
    public string DisplayName { get; }
    public string MonsterType { get; }
    public string RoleKey { get; }
    public bool EliteOnly { get; }
    public string[] SkillIds { get; }
    public string SummaryText { get; }

    public PrototypeRpgEnemySkillLoadoutDefinition(
        string loadoutId,
        string displayName,
        string monsterType,
        string roleKey,
        bool eliteOnly,
        string[] skillIds,
        string summaryText)
    {
        LoadoutId = string.IsNullOrWhiteSpace(loadoutId) ? string.Empty : loadoutId.Trim();
        DisplayName = string.IsNullOrWhiteSpace(displayName) ? "Enemy Loadout" : displayName.Trim();
        MonsterType = string.IsNullOrWhiteSpace(monsterType) ? string.Empty : monsterType.Trim();
        RoleKey = string.IsNullOrWhiteSpace(roleKey) ? string.Empty : roleKey.Trim();
        EliteOnly = eliteOnly;
        SkillIds = skillIds ?? Array.Empty<string>();
        SummaryText = string.IsNullOrWhiteSpace(summaryText) ? DisplayName : summaryText.Trim();
    }
}

public sealed class PrototypeRpgEnemySkillRuntimeState
{
    public string SkillId = string.Empty;
    public string DisplayName = string.Empty;
    public string TargetKind = "single_party_member";
    public string EffectType = string.Empty;
    public int PowerValue;
    public int CooldownTurns;
    public int CooldownRemaining;
    public int MaxEncounterCharges;
    public int EncounterChargesRemaining;
    public string IntentLabel = string.Empty;
    public string ActionBiasKey = string.Empty;
    public string PredictedStatusText = string.Empty;
    public string SummaryText = string.Empty;
    public bool IsAvailable;
    public string AvailabilitySummaryText = string.Empty;

    public bool IsSkillAction => !string.Equals(ActionBiasKey, "basic_attack", StringComparison.Ordinal);
}

public static class PrototypeRpgEnemySkillCatalog
{
    private static readonly PrototypeRpgEnemySkillDefinition[] SharedDefinitions =
    {
        new PrototypeRpgEnemySkillDefinition("enemy_basic_strike", "Attack", "single_party_member", "damage", 0, 0, 0, "Attack", "basic_attack", string.Empty, "Basic frontline attack."),
        new PrototypeRpgEnemySkillDefinition("enemy_marking_gouge", "Marking Gouge", "single_party_member", "debuff", 4, 1, 2, "Mark weakest target", "debuff", "Applies Mark + Weaken.", "Precision debuff into focus fire."),
        new PrototypeRpgEnemySkillDefinition("enemy_guard_howl", "Guard Howl", "self", "self_buff", 0, 2, 1, "Raise guard", "self_buff", "Applies Guard Up + Regen.", "Self-buff to stabilize the next exchange."),
        new PrototypeRpgEnemySkillDefinition("enemy_purging_gel", "Purging Gel", "self", "cleanse_buff", 0, 2, 1, "Purge and recover", "cleanse", "Cleanses debuffs and applies Regen.", "Cleanses debuffs before the next action."),
        new PrototypeRpgEnemySkillDefinition("enemy_cinder_wave", "Cinder Wave", "all_party_members", "aoe_damage", 5, 2, 1, "Sweep the whole party", "aoe", "Applies Burn.", "AOE pressure with a burn rider."),
        new PrototypeRpgEnemySkillDefinition("enemy_execution_drive", "Execution Drive", "single_party_member", "finisher_damage", 8, 2, 1, "Execute weakened target", "finisher", string.Empty, "High-pressure finisher against weakened prey."),
        new PrototypeRpgEnemySkillDefinition("enemy_royal_wave", "Royal Wave", "all_party_members", "aoe_damage", 6, 2, 1, "Royal wave on all allies", "aoe", "Applies Burn.", "Elite-wide pressure swing."),
        new PrototypeRpgEnemySkillDefinition("enemy_command_rupture", "Command Rupture", "single_party_member", "debuff", 5, 2, 1, "Break guard on target", "debuff", "Applies Weaken.", "Elite command strike that opens the target."),
    };

    private static readonly PrototypeRpgEnemySkillLoadoutDefinition[] Loadouts =
    {
        new PrototypeRpgEnemySkillLoadoutDefinition("enemy_loadout_goblin_skirmisher", "Goblin Skirmisher Kit", "Goblin", "Skirmisher", false, new[] { "enemy_basic_strike", "enemy_marking_gouge" }, "Fast pressure with a setup gouge."),
        new PrototypeRpgEnemySkillLoadoutDefinition("enemy_loadout_goblin_striker", "Goblin Striker Kit", "Goblin", "Striker", false, new[] { "enemy_basic_strike", "enemy_execution_drive", "enemy_marking_gouge" }, "Pressure into execute windows."),
        new PrototypeRpgEnemySkillLoadoutDefinition("enemy_loadout_goblin_bulwark", "Goblin Bulwark Kit", "Goblin", "Bulwark", false, new[] { "enemy_basic_strike", "enemy_guard_howl" }, "Front guard that braces before trading."),
        new PrototypeRpgEnemySkillLoadoutDefinition("enemy_loadout_slime_bulwark", "Slime Bulwark Kit", "Slime", "Bulwark", false, new[] { "enemy_basic_strike", "enemy_purging_gel" }, "Sticky defense with a purge window."),
        new PrototypeRpgEnemySkillLoadoutDefinition("enemy_loadout_slime_skirmisher", "Slime Skirmisher Kit", "Slime", "Skirmisher", false, new[] { "enemy_basic_strike", "enemy_cinder_wave" }, "Slime pressure that can splash the whole party."),
        new PrototypeRpgEnemySkillLoadoutDefinition("enemy_loadout_elite_striker", "Elite Striker Kit", string.Empty, "Striker", true, new[] { "enemy_basic_strike", "enemy_execution_drive", "enemy_cinder_wave", "enemy_command_rupture" }, "Elite finisher plus sweep pressure."),
        new PrototypeRpgEnemySkillLoadoutDefinition("enemy_loadout_elite_bulwark", "Elite Bulwark Kit", string.Empty, "Bulwark", true, new[] { "enemy_basic_strike", "enemy_guard_howl", "enemy_royal_wave", "enemy_command_rupture" }, "Elite guard shell with a royal wave."),
    };

    public static PrototypeRpgEnemySkillDefinition GetSkillDefinition(string skillId)
    {
        if (string.IsNullOrWhiteSpace(skillId))
        {
            return null;
        }

        string trimmed = skillId.Trim();
        for (int i = 0; i < SharedDefinitions.Length; i++)
        {
            PrototypeRpgEnemySkillDefinition definition = SharedDefinitions[i];
            if (definition != null && string.Equals(definition.SkillId, trimmed, StringComparison.Ordinal))
            {
                return definition;
            }
        }

        return null;
    }

    public static PrototypeRpgEnemySkillLoadoutDefinition ResolveLoadout(string monsterType, string roleKey, bool isElite)
    {
        string safeMonsterType = string.IsNullOrWhiteSpace(monsterType) ? string.Empty : monsterType.Trim();
        string safeRoleKey = string.IsNullOrWhiteSpace(roleKey) ? string.Empty : roleKey.Trim();

        if (isElite)
        {
            for (int i = 0; i < Loadouts.Length; i++)
            {
                PrototypeRpgEnemySkillLoadoutDefinition loadout = Loadouts[i];
                if (loadout != null && loadout.EliteOnly && string.Equals(loadout.RoleKey, safeRoleKey, StringComparison.Ordinal))
                {
                    return loadout;
                }
            }
        }

        for (int i = 0; i < Loadouts.Length; i++)
        {
            PrototypeRpgEnemySkillLoadoutDefinition loadout = Loadouts[i];
            if (loadout == null || loadout.EliteOnly)
            {
                continue;
            }

            bool typeMatches = string.IsNullOrEmpty(loadout.MonsterType) || string.Equals(loadout.MonsterType, safeMonsterType, StringComparison.Ordinal);
            bool roleMatches = string.IsNullOrEmpty(loadout.RoleKey) || string.Equals(loadout.RoleKey, safeRoleKey, StringComparison.Ordinal);
            if (typeMatches && roleMatches)
            {
                return loadout;
            }
        }

        return new PrototypeRpgEnemySkillLoadoutDefinition(
            "enemy_loadout_fallback",
            "Fallback Enemy Kit",
            safeMonsterType,
            safeRoleKey,
            false,
            new[] { "enemy_basic_strike" },
            "Fallback enemy attack kit.");
    }
}
