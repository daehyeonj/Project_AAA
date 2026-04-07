using System;

public sealed class PrototypeRpgPassiveSkillDefinition
{
    public string PassiveSkillId = string.Empty;
    public string DisplayLabel = string.Empty;
    public string SlotKey = string.Empty;
    public int BonusAttack;
    public int BonusDefense;
    public int BonusSpeed;
    public int BonusSkillPower;
    public string PassiveHintText = string.Empty;
    public string BattleLabelHint = string.Empty;
    public string SummaryText = string.Empty;

    public PrototypeRpgPassiveSkillDefinition(string passiveSkillId, string displayLabel, string slotKey, int bonusAttack, int bonusDefense, int bonusSpeed, int bonusSkillPower, string passiveHintText, string battleLabelHint, string summaryText)
    {
        PassiveSkillId = Normalize(passiveSkillId);
        DisplayLabel = string.IsNullOrWhiteSpace(displayLabel) ? "Passive" : displayLabel.Trim();
        SlotKey = Normalize(slotKey);
        BonusAttack = bonusAttack;
        BonusDefense = bonusDefense;
        BonusSpeed = bonusSpeed;
        BonusSkillPower = bonusSkillPower;
        PassiveHintText = string.IsNullOrWhiteSpace(passiveHintText) ? string.Empty : passiveHintText.Trim();
        BattleLabelHint = string.IsNullOrWhiteSpace(battleLabelHint) ? string.Empty : battleLabelHint.Trim();
        SummaryText = string.IsNullOrWhiteSpace(summaryText) ? string.Empty : summaryText.Trim();
    }

    private static string Normalize(string value)
    {
        return string.IsNullOrWhiteSpace(value) ? string.Empty : value.Trim().ToLowerInvariant();
    }
}

public sealed class PrototypeRpgPassiveModifierContribution
{
    public string PassiveSkillId = string.Empty;
    public string DisplayLabel = string.Empty;
    public int BonusAttack;
    public int BonusDefense;
    public int BonusSpeed;
    public int BonusSkillPower;
    public string PassiveHintText = string.Empty;
    public string BattleLabelHint = string.Empty;
    public string SummaryText = string.Empty;
}

public sealed class PrototypeRpgJobSpecializationDefinition
{
    public string SpecializationKey = string.Empty;
    public string JobId = string.Empty;
    public string RoleTag = string.Empty;
    public string DisplayLabel = string.Empty;
    public string PassiveSkillId = string.Empty;
    public int RequiredLevel = 1;
    public bool RequiresEliteClear;
    public bool PrefersRiskyRoute;
    public string PreferredGrowthTrackId = string.Empty;
    public string PreferredEquipmentKeyword = string.Empty;
    public string PreferredSkillLoadoutKeyword = string.Empty;
    public int BonusAttack;
    public int BonusDefense;
    public int BonusSpeed;
    public int BonusSkillPower;
    public string SummaryText = string.Empty;
    public string UnlockHintText = string.Empty;
    public string RuntimeSynergyText = string.Empty;

    public PrototypeRpgJobSpecializationDefinition(
        string specializationKey,
        string jobId,
        string roleTag,
        string displayLabel,
        string passiveSkillId,
        int requiredLevel,
        bool requiresEliteClear,
        bool prefersRiskyRoute,
        string preferredGrowthTrackId,
        string preferredEquipmentKeyword,
        string preferredSkillLoadoutKeyword,
        int bonusAttack,
        int bonusDefense,
        int bonusSpeed,
        int bonusSkillPower,
        string summaryText,
        string unlockHintText,
        string runtimeSynergyText)
    {
        SpecializationKey = Normalize(specializationKey);
        JobId = Normalize(jobId);
        RoleTag = Normalize(roleTag);
        DisplayLabel = string.IsNullOrWhiteSpace(displayLabel) ? "Specialist" : displayLabel.Trim();
        PassiveSkillId = Normalize(passiveSkillId);
        RequiredLevel = requiredLevel > 0 ? requiredLevel : 1;
        RequiresEliteClear = requiresEliteClear;
        PrefersRiskyRoute = prefersRiskyRoute;
        PreferredGrowthTrackId = Normalize(preferredGrowthTrackId);
        PreferredEquipmentKeyword = Normalize(preferredEquipmentKeyword);
        PreferredSkillLoadoutKeyword = Normalize(preferredSkillLoadoutKeyword);
        BonusAttack = bonusAttack;
        BonusDefense = bonusDefense;
        BonusSpeed = bonusSpeed;
        BonusSkillPower = bonusSkillPower;
        SummaryText = string.IsNullOrWhiteSpace(summaryText) ? string.Empty : summaryText.Trim();
        UnlockHintText = string.IsNullOrWhiteSpace(unlockHintText) ? string.Empty : unlockHintText.Trim();
        RuntimeSynergyText = string.IsNullOrWhiteSpace(runtimeSynergyText) ? string.Empty : runtimeSynergyText.Trim();
    }

    private static string Normalize(string value)
    {
        return string.IsNullOrWhiteSpace(value) ? string.Empty : value.Trim().ToLowerInvariant();
    }
}

public sealed class PrototypeRpgPassiveSkillSlotState
{
    public string PassiveSkillId = string.Empty;
    public string DisplayLabel = string.Empty;
    public string SlotKey = string.Empty;
    public string SummaryText = string.Empty;
    public string RuntimeHintText = string.Empty;
}

public sealed class PrototypeRpgJobUnlockMemberState
{
    public string MemberId = string.Empty;
    public string DisplayName = string.Empty;
    public string AppliedJobId = string.Empty;
    public string CurrentSpecializationKey = string.Empty;
    public string CurrentSpecializationLabel = string.Empty;
    public string CurrentPassiveSkillId = string.Empty;
    public string CurrentPassiveSkillLabel = string.Empty;
    public string SpecializationSummaryText = string.Empty;
    public string PassiveSlotSummaryText = string.Empty;
    public string RuntimeSynergySummaryText = string.Empty;
    public string PendingUnlockHintText = string.Empty;
    public string NextRunPreviewText = string.Empty;
    public string LastUnlockedRunIdentity = string.Empty;
    public string[] UnlockedSpecializationKeys = Array.Empty<string>();
}

public sealed class PrototypeRpgJobUnlockRuntimeState
{
    public string PartyId = string.Empty;
    public string DisplayName = string.Empty;
    public string LastRunIdentity = string.Empty;
    public string JobSpecializationSummaryText = string.Empty;
    public string PassiveSkillSlotSummaryText = string.Empty;
    public string RuntimeSynergySummaryText = string.Empty;
    public string NextRunSpecializationPreviewText = string.Empty;
    public string SummaryText = string.Empty;
    public PrototypeRpgJobUnlockMemberState[] Members = Array.Empty<PrototypeRpgJobUnlockMemberState>();
}

public static class PrototypeRpgPassiveSkillCatalog
{
    private static readonly PrototypeRpgPassiveSkillDefinition[] Definitions =
    {
        new PrototypeRpgPassiveSkillDefinition("flat_attack_small", "Attack Drill", "passive_primary", 1, 0, 0, 0, "Passive ATK +1 keeps direct actions punchy.", "Aggressive", "ATK+1 passive."),
        new PrototypeRpgPassiveSkillDefinition("flat_defense_small", "Guard Lining", "passive_primary", 0, 1, 0, 0, "Passive DEF +1 steadies pressure turns.", "Guarded", "DEF+1 passive."),
        new PrototypeRpgPassiveSkillDefinition("flat_speed_small", "Quick Step", "passive_primary", 0, 0, 1, 0, "Passive SPD +1 helps openers stay ahead.", "Quick", "SPD+1 passive."),
        new PrototypeRpgPassiveSkillDefinition("skill_damage_bonus_small", "Skill Edge", "passive_primary", 0, 0, 0, 1, "Passive skill output +1 sharpens burst windows.", "Empowered", "Skill power +1 passive."),
        new PrototypeRpgPassiveSkillDefinition("heal_bonus_small", "Sanctified Echo", "passive_primary", 0, 0, 0, 1, "Passive sustain +1 strengthens restorative turns.", "Sanctified", "Heal-facing power +1 passive."),
        new PrototypeRpgPassiveSkillDefinition("first_turn_speed_bonus", "First Turn Tempo", "passive_primary", 0, 0, 1, 0, "First-turn tempo prototype grants +1 SPD in this runtime.", "Tempo", "Opener speed passive."),
        new PrototypeRpgPassiveSkillDefinition("low_hp_guard_bonus", "Last Stand Guard", "passive_primary", 0, 1, 0, 0, "Low-HP guard prototype grants +1 DEF in this runtime.", "Last Stand", "Pressure guard passive.")
    };

    public static PrototypeRpgPassiveSkillDefinition GetDefinition(string passiveSkillId)
    {
        string normalizedId = Normalize(passiveSkillId);
        if (string.IsNullOrEmpty(normalizedId))
        {
            return null;
        }

        for (int i = 0; i < Definitions.Length; i++)
        {
            PrototypeRpgPassiveSkillDefinition definition = Definitions[i];
            if (definition != null && definition.PassiveSkillId == normalizedId)
            {
                return definition;
            }
        }

        return null;
    }

    public static PrototypeRpgPassiveModifierContribution CreateContribution(string passiveSkillId)
    {
        PrototypeRpgPassiveSkillDefinition definition = GetDefinition(passiveSkillId);
        PrototypeRpgPassiveModifierContribution contribution = new PrototypeRpgPassiveModifierContribution();
        if (definition == null)
        {
            return contribution;
        }

        contribution.PassiveSkillId = definition.PassiveSkillId;
        contribution.DisplayLabel = definition.DisplayLabel;
        contribution.BonusAttack = definition.BonusAttack;
        contribution.BonusDefense = definition.BonusDefense;
        contribution.BonusSpeed = definition.BonusSpeed;
        contribution.BonusSkillPower = definition.BonusSkillPower;
        contribution.PassiveHintText = definition.PassiveHintText;
        contribution.BattleLabelHint = definition.BattleLabelHint;
        contribution.SummaryText = definition.SummaryText;
        return contribution;
    }

    private static string Normalize(string value)
    {
        return string.IsNullOrWhiteSpace(value) ? string.Empty : value.Trim().ToLowerInvariant();
    }
}

public static class PrototypeRpgJobSpecializationCatalog
{
    private static readonly PrototypeRpgJobSpecializationDefinition[] Definitions =
    {
        new PrototypeRpgJobSpecializationDefinition("spec_warrior_initiate", "job_warrior_novice", "warrior", "Warrior Initiate", "flat_attack_small", 1, false, false, "growth_frontline_guard", "shield", "guard", 1, 1, 0, 0, "Baseline warrior lane that can branch into Guardian or Breaker.", "Unlock Guardian or Breaker at Lv 2.", "Warrior core keeps frontline and strike pressure balanced."),
        new PrototypeRpgJobSpecializationDefinition("spec_rogue_initiate", "job_rogue_novice", "rogue", "Rogue Initiate", "flat_speed_small", 1, false, false, "growth_precision_shadow", "armor", "precision", 0, 0, 1, 0, "Baseline rogue lane that can branch into Shadow or Executioner.", "Unlock Shadow or Executioner at Lv 2.", "Rogue core keeps tempo and setup pressure aligned."),
        new PrototypeRpgJobSpecializationDefinition("spec_mage_initiate", "job_mage_novice", "mage", "Mage Initiate", "skill_damage_bonus_small", 1, false, false, "growth_arcane_surge", "focus", "surge", 1, 0, 0, 1, "Baseline mage lane that can branch into Arcanist or Spellguard.", "Unlock Arcanist or Spellguard at Lv 2.", "Mage core keeps burst and focus scaling aligned."),
        new PrototypeRpgJobSpecializationDefinition("spec_cleric_initiate", "job_cleric_novice", "cleric", "Cleric Initiate", "heal_bonus_small", 1, false, false, "growth_support_choir", "relic", "choir", 0, 1, 0, 1, "Baseline cleric lane that can branch into Chaplain or Warden.", "Unlock Chaplain or Warden at Lv 2.", "Cleric core keeps sustain and recovery timing aligned."),
        new PrototypeRpgJobSpecializationDefinition("spec_guardian", "job_guardian", "warrior", "Guardian Bulwark", "low_hp_guard_bonus", 2, false, false, "growth_frontline_guard", "shield", "guard", 0, 2, 0, 1, "Guardian bulwark steadies the frontline and reinforces guard windows.", "Guardian unlock leans toward shield and guard routes.", "Guardian + shield affix + guard route produces steadier frontline turns."),
        new PrototypeRpgJobSpecializationDefinition("spec_breaker", "job_breaker", "warrior", "Breaker Discipline", "flat_attack_small", 2, false, true, "growth_frontline_breaker", "weapon", "break", 2, 0, 0, 1, "Breaker discipline sharpens single-target pressure and finisher tempo.", "Breaker unlock prefers risky offensive clears.", "Breaker + weapon affix + break route produces harder single-target pushes."),
        new PrototypeRpgJobSpecializationDefinition("spec_shadow", "job_shadow", "rogue", "Shadow Tempo", "first_turn_speed_bonus", 2, false, false, "growth_precision_shadow", "armor", "evasion", 0, 0, 2, 0, "Shadow tempo favors safer openings and evasive repositioning.", "Shadow unlock prefers fast precision routes.", "Shadow + evasive gear + opener bias keeps the rogue ahead on tempo."),
        new PrototypeRpgJobSpecializationDefinition("spec_executioner", "job_executioner", "rogue", "Execution Focus", "skill_damage_bonus_small", 2, false, true, "growth_precision_execution", "weapon", "finisher", 1, 0, 1, 1, "Execution focus converts setup into sharper finisher damage.", "Executioner unlock prefers risky finish pressure.", "Executioner + finisher loadout + attack affix sharpens lethal windows."),
        new PrototypeRpgJobSpecializationDefinition("spec_arcanist", "job_arcanist", "mage", "Arcanist Surge", "skill_damage_bonus_small", 2, false, true, "growth_arcane_surge", "focus", "surge", 2, 0, 0, 1, "Arcanist surge amplifies burst turns and offensive casting.", "Arcanist unlock prefers focus gear and risky spell pressure.", "Arcanist + focus affix + surge route spikes burst casting."),
        new PrototypeRpgJobSpecializationDefinition("spec_spellguard", "job_spellguard", "mage", "Spellguard Ward", "flat_defense_small", 2, false, false, "growth_arcane_ward", "armor", "ward", 0, 1, 0, 1, "Spellguard ward trades raw tempo for safer casting stability.", "Spellguard unlock prefers defensive arcane paths.", "Spellguard + ward gear + sustain casting steadies burst windows."),
        new PrototypeRpgJobSpecializationDefinition("spec_chaplain", "job_chaplain", "cleric", "Chaplain Chorus", "heal_bonus_small", 2, false, false, "growth_support_choir", "relic", "choir", 0, 0, 0, 2, "Chaplain chorus deepens whole-party recovery output.", "Chaplain unlock prefers choir and relic synergy.", "Chaplain + relic affix + choir route strengthens group sustain."),
        new PrototypeRpgJobSpecializationDefinition("spec_warden", "job_warden", "cleric", "Warden Shelter", "low_hp_guard_bonus", 2, false, false, "growth_support_guard", "armor", "guard", 0, 2, 0, 1, "Warden shelter protects the party through longer pressure windows.", "Warden unlock prefers guard-heavy support routes.", "Warden + guarded vestments + rescue timing improves party stability."),
        new PrototypeRpgJobSpecializationDefinition("spec_vanguard", "job_vanguard", "warrior", "Vanguard Advance", "flat_attack_small", 3, false, true, "growth_frontline_vanguard", "armor", "vanguard", 2, 1, 1, 1, "Vanguard advance rewards aggressive frontline momentum.", "Vanguard unlock leans on risky clears and tempo gear.", "Vanguard + balanced gear + pressure lane keeps the front moving."),
        new PrototypeRpgJobSpecializationDefinition("spec_bastion", "job_bastion", "warrior", "Bastion Hold", "flat_defense_small", 3, true, false, "growth_frontline_bastion", "armor", "bastion", 0, 3, 0, 1, "Bastion hold turns elite clears into tougher frontline continuity.", "Bastion unlock needs elite progress or strong defensive continuity.", "Bastion + heavy gear + guarded route widens survival margins."),
        new PrototypeRpgJobSpecializationDefinition("spec_saboteur", "job_saboteur", "rogue", "Saboteur Relay", "flat_speed_small", 3, false, true, "growth_precision_saboteur", "utility", "saboteur", 1, 0, 2, 1, "Saboteur relay keeps setup pressure alive across longer runs.", "Saboteur unlock prefers risky tempo chains.", "Saboteur + utility gear + setup route sustains tempo pressure."),
        new PrototypeRpgJobSpecializationDefinition("spec_phantom", "job_phantom", "rogue", "Phantom Slip", "first_turn_speed_bonus", 3, true, false, "growth_precision_phantom", "armor", "phantom", 1, 0, 2, 0, "Phantom slip rewards elite clears with cleaner opener control.", "Phantom unlock needs elite progress or deep evasive play.", "Phantom + speed affix + evasive route sharpens opener control."),
        new PrototypeRpgJobSpecializationDefinition("spec_tempest", "job_tempest", "mage", "Tempest Weave", "skill_damage_bonus_small", 3, false, true, "growth_arcane_tempest", "focus", "tempest", 2, 0, 1, 2, "Tempest weave widens multi-target burst sequencing.", "Tempest unlock prefers risky arcane pressure.", "Tempest + focus affix + burst route raises multi-target output."),
        new PrototypeRpgJobSpecializationDefinition("spec_astral", "job_astral", "mage", "Astral Ward", "flat_defense_small", 3, true, false, "growth_arcane_astral", "focus", "astral", 1, 1, 0, 2, "Astral ward converts elite clears into safer late-run casting.", "Astral unlock needs elite progress or steady arcane continuity.", "Astral + warded casting + affix support stabilizes burst uptime."),
        new PrototypeRpgJobSpecializationDefinition("spec_beacon", "job_beacon", "cleric", "Beacon Rhythm", "heal_bonus_small", 3, false, true, "growth_support_beacon", "relic", "beacon", 0, 1, 1, 2, "Beacon rhythm accelerates support timing and recovery tempo.", "Beacon unlock prefers risky support tempo.", "Beacon + relic affix + rescue turns quicken party recovery."),
        new PrototypeRpgJobSpecializationDefinition("spec_saint", "job_saint", "cleric", "Saint Mantle", "low_hp_guard_bonus", 3, true, false, "growth_support_saint", "armor", "saint", 0, 2, 0, 2, "Saint mantle turns elite clears into broader party protection.", "Saint unlock needs elite progress or durable sustain routing.", "Saint + saint gear + guarded sustain broadens party protection.")
    };

    public static PrototypeRpgJobSpecializationDefinition ResolveCurrentDefinition(string roleTag, string growthTrackId, string jobId)
    {
        PrototypeRpgJobSpecializationDefinition jobDefinition = GetDefinitionByJobId(jobId);
        if (jobDefinition != null)
        {
            return jobDefinition;
        }

        string normalizedRoleTag = Normalize(roleTag);
        string normalizedGrowthTrackId = Normalize(growthTrackId);
        if (normalizedRoleTag == "warrior")
        {
            return normalizedGrowthTrackId.Contains("breaker") ? GetDefinition("spec_breaker") :
                normalizedGrowthTrackId.Contains("bastion") ? GetDefinition("spec_bastion") :
                normalizedGrowthTrackId.Contains("vanguard") ? GetDefinition("spec_vanguard") :
                normalizedGrowthTrackId.Contains("guard") ? GetDefinition("spec_guardian") :
                GetDefinition("spec_warrior_initiate");
        }

        if (normalizedRoleTag == "rogue")
        {
            return normalizedGrowthTrackId.Contains("execution") ? GetDefinition("spec_executioner") :
                normalizedGrowthTrackId.Contains("saboteur") ? GetDefinition("spec_saboteur") :
                normalizedGrowthTrackId.Contains("phantom") ? GetDefinition("spec_phantom") :
                normalizedGrowthTrackId.Contains("shadow") ? GetDefinition("spec_shadow") :
                GetDefinition("spec_rogue_initiate");
        }

        if (normalizedRoleTag == "mage")
        {
            return normalizedGrowthTrackId.Contains("ward") ? GetDefinition("spec_spellguard") :
                normalizedGrowthTrackId.Contains("tempest") ? GetDefinition("spec_tempest") :
                normalizedGrowthTrackId.Contains("astral") ? GetDefinition("spec_astral") :
                normalizedGrowthTrackId.Contains("surge") ? GetDefinition("spec_arcanist") :
                GetDefinition("spec_mage_initiate");
        }

        if (normalizedRoleTag == "cleric")
        {
            return normalizedGrowthTrackId.Contains("guard") ? GetDefinition("spec_warden") :
                normalizedGrowthTrackId.Contains("beacon") ? GetDefinition("spec_beacon") :
                normalizedGrowthTrackId.Contains("saint") ? GetDefinition("spec_saint") :
                normalizedGrowthTrackId.Contains("choir") ? GetDefinition("spec_chaplain") :
                GetDefinition("spec_cleric_initiate");
        }

        return null;
    }

    public static PrototypeRpgJobSpecializationDefinition GetDefinition(string specializationKey)
    {
        string normalizedKey = Normalize(specializationKey);
        if (string.IsNullOrEmpty(normalizedKey))
        {
            return null;
        }

        for (int i = 0; i < Definitions.Length; i++)
        {
            PrototypeRpgJobSpecializationDefinition definition = Definitions[i];
            if (definition != null && definition.SpecializationKey == normalizedKey)
            {
                return definition;
            }
        }

        return null;
    }

    public static PrototypeRpgJobSpecializationDefinition GetDefinitionByJobId(string jobId)
    {
        string normalizedJobId = Normalize(jobId);
        if (string.IsNullOrEmpty(normalizedJobId))
        {
            return null;
        }

        for (int i = 0; i < Definitions.Length; i++)
        {
            PrototypeRpgJobSpecializationDefinition definition = Definitions[i];
            if (definition != null && definition.JobId == normalizedJobId)
            {
                return definition;
            }
        }

        return null;
    }

    private static string Normalize(string value)
    {
        return string.IsNullOrWhiteSpace(value) ? string.Empty : value.Trim().ToLowerInvariant();
    }
}
