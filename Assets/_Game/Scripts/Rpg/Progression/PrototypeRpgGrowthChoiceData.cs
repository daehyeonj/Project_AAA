using System;
using System.Collections.Generic;

public sealed class PrototypeRpgGrowthChoiceRuleDefinition
{
    public string RuleId { get; }
    public string OfferGroupId { get; }
    public string OfferTypeKey { get; }
    public string RoleTag { get; }
    public string OfferLabel { get; }
    public string ReasonText { get; }
    public string ContinuityText { get; }
    public string TargetGrowthTrackId { get; }
    public string TargetJobId { get; }
    public string TargetEquipmentLoadoutId { get; }
    public string TargetSkillLoadoutId { get; }
    public bool IsFallback { get; }
    public string UnlockGroupKey { get; }
    public string UnlockTierKey { get; }
    public string RequiredTierKey { get; }
    public int UnlockPriority { get; }
    public string UnlockReasonHint { get; }
    public int RepeatExposureThreshold { get; }

    public PrototypeRpgGrowthChoiceRuleDefinition(
        string ruleId,
        string offerGroupId,
        string offerTypeKey,
        string roleTag,
        string offerLabel,
        string reasonText,
        string continuityText,
        string targetGrowthTrackId,
        string targetJobId,
        string targetEquipmentLoadoutId,
        string targetSkillLoadoutId,
        bool isFallback = false,
        string unlockGroupKey = "",
        string unlockTierKey = "",
        string requiredTierKey = "",
        int unlockPriority = 0,
        string unlockReasonHint = "",
        int repeatExposureThreshold = 0)
    {
        RuleId = string.IsNullOrWhiteSpace(ruleId) ? string.Empty : ruleId.Trim();
        OfferGroupId = string.IsNullOrWhiteSpace(offerGroupId) ? string.Empty : offerGroupId.Trim();
        OfferTypeKey = string.IsNullOrWhiteSpace(offerTypeKey) ? string.Empty : offerTypeKey.Trim();
        RoleTag = string.IsNullOrWhiteSpace(roleTag) ? string.Empty : roleTag.Trim().ToLowerInvariant();
        OfferLabel = string.IsNullOrWhiteSpace(offerLabel) ? "Upgrade" : offerLabel.Trim();
        ReasonText = string.IsNullOrWhiteSpace(reasonText) ? string.Empty : reasonText.Trim();
        ContinuityText = string.IsNullOrWhiteSpace(continuityText) ? string.Empty : continuityText.Trim();
        TargetGrowthTrackId = string.IsNullOrWhiteSpace(targetGrowthTrackId) ? string.Empty : targetGrowthTrackId.Trim();
        TargetJobId = string.IsNullOrWhiteSpace(targetJobId) ? string.Empty : targetJobId.Trim();
        TargetEquipmentLoadoutId = string.IsNullOrWhiteSpace(targetEquipmentLoadoutId) ? string.Empty : targetEquipmentLoadoutId.Trim();
        TargetSkillLoadoutId = string.IsNullOrWhiteSpace(targetSkillLoadoutId) ? string.Empty : targetSkillLoadoutId.Trim();
        IsFallback = isFallback;
        UnlockGroupKey = string.IsNullOrWhiteSpace(unlockGroupKey) ? string.Empty : unlockGroupKey.Trim();
        UnlockTierKey = string.IsNullOrWhiteSpace(unlockTierKey) ? string.Empty : unlockTierKey.Trim();
        RequiredTierKey = string.IsNullOrWhiteSpace(requiredTierKey) ? string.Empty : requiredTierKey.Trim();
        UnlockPriority = unlockPriority;
        UnlockReasonHint = string.IsNullOrWhiteSpace(unlockReasonHint) ? string.Empty : unlockReasonHint.Trim();
        RepeatExposureThreshold = repeatExposureThreshold;
    }
}

public static class PrototypeRpgGrowthChoiceRuleCatalog
{
    private static readonly PrototypeRpgGrowthChoiceRuleDefinition WarriorOffense = new PrototypeRpgGrowthChoiceRuleDefinition(
        "warrior_breaker",
        "warrior_path",
        "offense_path",
        "warrior",
        "Breaker Path",
        "Damage-heavy frontline run. Push toward a stronger break line.",
        "Refresh off the latest applied frontline pressure.",
        "growth_frontline_breaker",
        "job_breaker",
        "equip_warrior_brutal",
        "skillloadout_warrior_break");

    private static readonly PrototypeRpgGrowthChoiceRuleDefinition WarriorDefense = new PrototypeRpgGrowthChoiceRuleDefinition(
        "warrior_guardian",
        "warrior_path",
        "defense_path",
        "warrior",
        "Guardian Path",
        "Damage taken or collapse pressure suggests a sturdier guardian line.",
        "Refresh off the latest applied guardian line.",
        "growth_frontline_guard",
        "job_guardian",
        "equip_warrior_shielded",
        "skillloadout_warrior_guard");

    private static readonly PrototypeRpgGrowthChoiceRuleDefinition WarriorVanguard = new PrototypeRpgGrowthChoiceRuleDefinition(
        "warrior_vanguard",
        "warrior_vanguard",
        "advanced_path",
        "warrior",
        "Vanguard Line",
        "Repeated frontline pressure unlocked a faster vanguard branch.",
        "Advance off the committed frontline into a faster pressure line.",
        "growth_frontline_vanguard",
        "job_vanguard",
        "equip_warrior_vanguard",
        "skillloadout_warrior_vanguard",
        false,
        "warrior_vanguard",
        "tier_2",
        "tier_2",
        3,
        "Unlock: repeated frontline pressure opened Vanguard Line.",
        2);

    private static readonly PrototypeRpgGrowthChoiceRuleDefinition WarriorBastion = new PrototypeRpgGrowthChoiceRuleDefinition(
        "warrior_bastion",
        "warrior_bastion",
        "elite_path",
        "warrior",
        "Bastion Oath",
        "Elite momentum or long-session pressure unlocked a bastion branch.",
        "Escalate the committed frontline into a heavier bastion oath.",
        "growth_frontline_bastion",
        "job_bastion",
        "equip_warrior_bastion",
        "skillloadout_warrior_bastion",
        false,
        "warrior_bastion",
        "tier_3",
        "tier_3",
        5,
        "Tier escalation: bastion oath unlocked for longer-session frontline variety.",
        3);

    private static readonly PrototypeRpgGrowthChoiceRuleDefinition RogueOffense = new PrototypeRpgGrowthChoiceRuleDefinition(
        "rogue_executioner",
        "rogue_path",
        "offense_path",
        "rogue",
        "Executioner Path",
        "Finisher pressure suggests a sharper execution line.",
        "Refresh off the latest applied execution route.",
        "growth_precision_execution",
        "job_executioner",
        "equip_rogue_finisher",
        "skillloadout_rogue_finisher");

    private static readonly PrototypeRpgGrowthChoiceRuleDefinition RogueDefense = new PrototypeRpgGrowthChoiceRuleDefinition(
        "rogue_shadow",
        "rogue_path",
        "defense_path",
        "rogue",
        "Shadow Path",
        "Pressure and recovery risk suggest a safer evasive route.",
        "Refresh off the latest applied shadow route.",
        "growth_precision_shadow",
        "job_shadow",
        "equip_rogue_evasion",
        "skillloadout_rogue_evasion");

    private static readonly PrototypeRpgGrowthChoiceRuleDefinition RogueSaboteur = new PrototypeRpgGrowthChoiceRuleDefinition(
        "rogue_saboteur",
        "rogue_saboteur",
        "utility_path",
        "rogue",
        "Saboteur Route",
        "Repeated execution loops unlocked a fresher sabotage route.",
        "Slide the committed rogue line into a more disruptive sabotage route.",
        "growth_precision_saboteur",
        "job_saboteur",
        "equip_rogue_saboteur",
        "skillloadout_rogue_saboteur",
        false,
        "rogue_saboteur",
        "tier_2",
        "tier_2",
        3,
        "Unlock: exposure pressure opened Saboteur Route.",
        2);

    private static readonly PrototypeRpgGrowthChoiceRuleDefinition RoguePhantom = new PrototypeRpgGrowthChoiceRuleDefinition(
        "rogue_phantom",
        "rogue_phantom",
        "elite_path",
        "rogue",
        "Phantom Route",
        "Elite tempo unlocked a higher-tier phantom route.",
        "Escalate the rogue line into a harder-to-pin phantom route.",
        "growth_precision_phantom",
        "job_phantom",
        "equip_rogue_phantom",
        "skillloadout_rogue_phantom",
        false,
        "rogue_phantom",
        "tier_3",
        "tier_3",
        5,
        "Tier escalation: Phantom Route is now in the session pool.",
        3);

    private static readonly PrototypeRpgGrowthChoiceRuleDefinition MageOffense = new PrototypeRpgGrowthChoiceRuleDefinition(
        "mage_surge",
        "mage_path",
        "offense_path",
        "mage",
        "Surge Path",
        "High damage output supports a stronger surge casting line.",
        "Refresh off the latest applied surge route.",
        "growth_arcane_surge",
        "job_arcanist",
        "equip_mage_focus",
        "skillloadout_mage_surge");

    private static readonly PrototypeRpgGrowthChoiceRuleDefinition MageDefense = new PrototypeRpgGrowthChoiceRuleDefinition(
        "mage_ward",
        "mage_path",
        "defense_path",
        "mage",
        "Ward Path",
        "Incoming pressure supports a steadier warded casting line.",
        "Refresh off the latest applied ward route.",
        "growth_arcane_ward",
        "job_spellguard",
        "equip_mage_warded",
        "skillloadout_mage_ward");

    private static readonly PrototypeRpgGrowthChoiceRuleDefinition MageTempest = new PrototypeRpgGrowthChoiceRuleDefinition(
        "mage_tempest",
        "mage_tempest",
        "advanced_path",
        "mage",
        "Tempest Path",
        "Repeated burst cadence unlocked a stronger tempest branch.",
        "Escalate the committed mage line into a faster tempest branch.",
        "growth_arcane_tempest",
        "job_tempest",
        "equip_mage_tempest",
        "skillloadout_mage_tempest",
        false,
        "mage_tempest",
        "tier_2",
        "tier_2",
        3,
        "Unlock: repeated arcane pressure opened Tempest Path.",
        2);

    private static readonly PrototypeRpgGrowthChoiceRuleDefinition MageAstral = new PrototypeRpgGrowthChoiceRuleDefinition(
        "mage_astral",
        "mage_astral",
        "elite_path",
        "mage",
        "Astral Path",
        "Elite or carryover pressure unlocked a higher-tier astral branch.",
        "Escalate the mage line into a steadier astral branch.",
        "growth_arcane_astral",
        "job_astral",
        "equip_mage_astral",
        "skillloadout_mage_astral",
        false,
        "mage_astral",
        "tier_3",
        "tier_3",
        5,
        "Tier escalation: Astral Path is now available this session.",
        3);

    private static readonly PrototypeRpgGrowthChoiceRuleDefinition ClericSupport = new PrototypeRpgGrowthChoiceRuleDefinition(
        "cleric_choir",
        "cleric_path",
        "support_path",
        "cleric",
        "Choir Path",
        "Party healing supports a stronger choir support line.",
        "Refresh off the latest applied choir route.",
        "growth_support_choir",
        "job_chaplain",
        "equip_cleric_relic",
        "skillloadout_cleric_choir");

    private static readonly PrototypeRpgGrowthChoiceRuleDefinition ClericDefense = new PrototypeRpgGrowthChoiceRuleDefinition(
        "cleric_warden",
        "cleric_path",
        "defense_path",
        "cleric",
        "Warden Path",
        "Retreat or collapse pressure suggests a safer warden line.",
        "Refresh off the latest applied warden route.",
        "growth_support_guard",
        "job_warden",
        "equip_cleric_guarded",
        "skillloadout_cleric_guard");

    private static readonly PrototypeRpgGrowthChoiceRuleDefinition ClericBeacon = new PrototypeRpgGrowthChoiceRuleDefinition(
        "cleric_beacon",
        "cleric_beacon",
        "utility_path",
        "cleric",
        "Beacon Path",
        "Long-session sustain opened a fresher beacon branch.",
        "Advance the committed support line into a brighter beacon branch.",
        "growth_support_beacon",
        "job_beacon",
        "equip_cleric_beacon",
        "skillloadout_cleric_beacon",
        false,
        "cleric_beacon",
        "tier_2",
        "tier_2",
        3,
        "Unlock: sustained support pressure opened Beacon Path.",
        2);

    private static readonly PrototypeRpgGrowthChoiceRuleDefinition ClericSaint = new PrototypeRpgGrowthChoiceRuleDefinition(
        "cleric_saint",
        "cleric_saint",
        "elite_path",
        "cleric",
        "Saint Path",
        "Elite clear or long-session recovery unlocked a saint branch.",
        "Escalate the support line into a saint branch for the next run.",
        "growth_support_saint",
        "job_saint",
        "equip_cleric_saint",
        "skillloadout_cleric_saint",
        false,
        "cleric_saint",
        "tier_3",
        "tier_3",
        5,
        "Tier escalation: Saint Path is now unlocked for this session.",
        3);

    public static PrototypeRpgGrowthChoiceRuleDefinition ResolvePrimaryRule(string roleTag, bool defensiveBias, bool offensiveBias, bool supportBias)
    {
        string normalizedRoleTag = NormalizeRoleTag(roleTag);
        switch (normalizedRoleTag)
        {
            case "warrior":
                return defensiveBias ? WarriorDefense : WarriorOffense;
            case "rogue":
                return defensiveBias ? RogueDefense : RogueOffense;
            case "mage":
                return defensiveBias ? MageDefense : MageOffense;
            case "cleric":
                return supportBias || !defensiveBias ? ClericSupport : ClericDefense;
            default:
                return null;
        }
    }

    public static PrototypeRpgGrowthChoiceRuleDefinition ResolveAlternateRule(PrototypeRpgGrowthChoiceRuleDefinition primary)
    {
        if (primary == null)
        {
            return null;
        }

        switch (primary.RuleId)
        {
            case "warrior_breaker":
                return WarriorDefense;
            case "warrior_guardian":
                return WarriorOffense;
            case "rogue_executioner":
                return RogueDefense;
            case "rogue_shadow":
                return RogueOffense;
            case "mage_surge":
                return MageDefense;
            case "mage_ward":
                return MageOffense;
            case "cleric_choir":
                return ClericDefense;
            case "cleric_warden":
                return ClericSupport;
            default:
                return null;
        }
    }

    public static PrototypeRpgGrowthChoiceRuleDefinition[] ResolveUnlockedRules(string roleTag, string[] unlockedGroupKeys, string[] unlockedTierKeys)
    {
        string normalizedRoleTag = NormalizeRoleTag(roleTag);
        List<PrototypeRpgGrowthChoiceRuleDefinition> rules = new List<PrototypeRpgGrowthChoiceRuleDefinition>();
        switch (normalizedRoleTag)
        {
            case "warrior":
                AddUnlockedRuleIfAvailable(rules, WarriorVanguard, unlockedGroupKeys, unlockedTierKeys);
                AddUnlockedRuleIfAvailable(rules, WarriorBastion, unlockedGroupKeys, unlockedTierKeys);
                break;
            case "rogue":
                AddUnlockedRuleIfAvailable(rules, RogueSaboteur, unlockedGroupKeys, unlockedTierKeys);
                AddUnlockedRuleIfAvailable(rules, RoguePhantom, unlockedGroupKeys, unlockedTierKeys);
                break;
            case "mage":
                AddUnlockedRuleIfAvailable(rules, MageTempest, unlockedGroupKeys, unlockedTierKeys);
                AddUnlockedRuleIfAvailable(rules, MageAstral, unlockedGroupKeys, unlockedTierKeys);
                break;
            case "cleric":
                AddUnlockedRuleIfAvailable(rules, ClericBeacon, unlockedGroupKeys, unlockedTierKeys);
                AddUnlockedRuleIfAvailable(rules, ClericSaint, unlockedGroupKeys, unlockedTierKeys);
                break;
        }

        return rules.Count <= 0 ? Array.Empty<PrototypeRpgGrowthChoiceRuleDefinition>() : rules.ToArray();
    }

    public static string ResolveTierTwoGroupKey(string roleTag)
    {
        switch (NormalizeRoleTag(roleTag))
        {
            case "warrior":
                return "warrior_vanguard";
            case "rogue":
                return "rogue_saboteur";
            case "mage":
                return "mage_tempest";
            case "cleric":
                return "cleric_beacon";
            default:
                return string.Empty;
        }
    }

    public static string ResolveTierThreeGroupKey(string roleTag)
    {
        switch (NormalizeRoleTag(roleTag))
        {
            case "warrior":
                return "warrior_bastion";
            case "rogue":
                return "rogue_phantom";
            case "mage":
                return "mage_astral";
            case "cleric":
                return "cleric_saint";
            default:
                return string.Empty;
        }
    }

    public static string ResolveTierLabel(string tierKey)
    {
        switch (string.IsNullOrWhiteSpace(tierKey) ? string.Empty : tierKey.Trim())
        {
            case "tier_3":
                return "Tier 3";
            case "tier_2":
                return "Tier 2";
            case "tier_1":
                return "Tier 1";
            default:
                return "Tier 1";
        }
    }

    public static string ResolveRoleCoreLaneKey(string roleTag)
    {
        switch (NormalizeRoleTag(roleTag))
        {
            case "warrior":
                return "frontline";
            case "rogue":
                return "precision";
            case "mage":
                return "arcane";
            case "cleric":
                return "support";
            default:
                return string.Empty;
        }
    }

    public static string ResolveRoleAdjacentLaneKey(string roleTag)
    {
        switch (NormalizeRoleTag(roleTag))
        {
            case "warrior":
                return "recovery";
            case "rogue":
                return "frontline";
            case "mage":
                return "support";
            case "cleric":
                return "recovery";
            default:
                return string.Empty;
        }
    }

    public static string ResolveRuleLaneKey(PrototypeRpgGrowthChoiceRuleDefinition rule)
    {
        if (rule == null)
        {
            return string.Empty;
        }

        string laneKey = ResolveLaneFromText(rule.TargetGrowthTrackId + " " + rule.TargetJobId + " " + rule.TargetEquipmentLoadoutId + " " + rule.TargetSkillLoadoutId);
        if (string.IsNullOrEmpty(laneKey))
        {
            switch (rule.OfferTypeKey)
            {
                case "support_path":
                    laneKey = "support";
                    break;
                case "defense_path":
                    laneKey = NormalizeRoleTag(rule.RoleTag) == "warrior" ? "frontline" : "recovery";
                    break;
                case "utility_path":
                    laneKey = ResolveRoleAdjacentLaneKey(rule.RoleTag);
                    break;
                default:
                    laneKey = ResolveRoleCoreLaneKey(rule.RoleTag);
                    break;
            }
        }

        return laneKey;
    }

    public static string ResolveRuleAdjacentLaneKey(PrototypeRpgGrowthChoiceRuleDefinition rule)
    {
        string laneKey = ResolveRuleLaneKey(rule);
        switch (laneKey)
        {
            case "frontline":
                return rule != null && rule.OfferTypeKey == "offense_path" ? "precision" : "recovery";
            case "precision":
                return rule != null && rule.OfferTypeKey == "utility_path" ? "arcane" : "frontline";
            case "arcane":
                return rule != null && rule.OfferTypeKey == "utility_path" ? "support" : "recovery";
            case "support":
                return "recovery";
            case "recovery":
                return ResolveRoleCoreLaneKey(rule != null ? rule.RoleTag : string.Empty);
            default:
                return ResolveRoleAdjacentLaneKey(rule != null ? rule.RoleTag : string.Empty);
        }
    }

    public static int ResolveRuleCommitmentWeight(PrototypeRpgGrowthChoiceRuleDefinition rule)
    {
        if (rule == null || rule.IsFallback)
        {
            return 0;
        }

        switch (rule.OfferTypeKey)
        {
            case "elite_path":
                return 5;
            case "advanced_path":
                return 4;
            case "offense_path":
            case "support_path":
                return 3;
            case "defense_path":
                return 2;
            case "utility_path":
                return 1;
            default:
                return 1;
        }
    }

    public static int ResolveRuleContradictionPenalty(PrototypeRpgGrowthChoiceRuleDefinition rule)
    {
        if (rule == null || rule.IsFallback)
        {
            return 0;
        }

        switch (rule.OfferTypeKey)
        {
            case "elite_path":
                return 20;
            case "advanced_path":
                return 16;
            case "utility_path":
                return 6;
            case "defense_path":
                return 8;
            default:
                return 12;
        }
    }

    public static int ResolveRuleSidegradeAllowance(PrototypeRpgGrowthChoiceRuleDefinition rule)
    {
        if (rule == null)
        {
            return 0;
        }

        switch (rule.OfferTypeKey)
        {
            case "utility_path":
                return 10;
            case "defense_path":
                return 6;
            case "support_path":
                return 5;
            default:
                return 2;
        }
    }

    public static int ResolveRuleRecoveryEscapeWeight(PrototypeRpgGrowthChoiceRuleDefinition rule)
    {
        if (rule == null)
        {
            return 0;
        }

        switch (rule.OfferTypeKey)
        {
            case "defense_path":
                return 12;
            case "support_path":
                return 8;
            case "utility_path":
                return 5;
            default:
                return 2;
        }
    }

    public static string BuildRuleCoherenceReasonHint(PrototypeRpgGrowthChoiceRuleDefinition rule)
    {
        if (rule == null)
        {
            return string.Empty;
        }

        string laneLabel = ResolveLaneLabel(ResolveRuleLaneKey(rule));
        switch (rule.OfferTypeKey)
        {
            case "elite_path":
                return laneLabel + " commitment deepened into an elite branch.";
            case "advanced_path":
                return laneLabel + " commitment opened a deeper specialization.";
            case "utility_path":
                return laneLabel + " commitment widened into an adjacent sidegrade.";
            case "defense_path":
                return laneLabel + " pressure left a guarded recovery escape hatch.";
            case "support_path":
                return laneLabel + " sustain stayed coherent with the recent run.";
            default:
                return laneLabel + " pressure remained the clearest lane for the next offer.";
        }
    }

    public static string ResolveLaneLabel(string laneKey)
    {
        switch (string.IsNullOrWhiteSpace(laneKey) ? string.Empty : laneKey.Trim().ToLowerInvariant())
        {
            case "frontline":
                return "Frontline";
            case "precision":
                return "Precision";
            case "arcane":
                return "Arcane";
            case "support":
                return "Support";
            case "recovery":
                return "Recovery";
            default:
                return "Unaligned";
        }
    }

    private static string ResolveLaneFromText(string text)
    {
        string normalized = string.IsNullOrWhiteSpace(text) ? string.Empty : text.Trim().ToLowerInvariant();
        if (string.IsNullOrEmpty(normalized))
        {
            return string.Empty;
        }

        if (normalized.Contains("frontline") || normalized.Contains("breaker") || normalized.Contains("guardian") || normalized.Contains("vanguard") || normalized.Contains("bastion"))
        {
            return "frontline";
        }

        if (normalized.Contains("precision") || normalized.Contains("execution") || normalized.Contains("shadow") || normalized.Contains("saboteur") || normalized.Contains("phantom"))
        {
            return "precision";
        }

        if (normalized.Contains("arcane") || normalized.Contains("surge") || normalized.Contains("ward") || normalized.Contains("tempest") || normalized.Contains("astral"))
        {
            return "arcane";
        }

        if (normalized.Contains("support") || normalized.Contains("choir") || normalized.Contains("beacon") || normalized.Contains("chaplain") || normalized.Contains("saint"))
        {
            return "support";
        }

        if (normalized.Contains("recover") || normalized.Contains("guard") || normalized.Contains("warden") || normalized.Contains("warded") || normalized.Contains("guarded"))
        {
            return "recovery";
        }

        return string.Empty;
    }
    public static PrototypeRpgGrowthChoiceRuleDefinition ResolveFallbackRule(string roleTag)
    {
        string normalizedRoleTag = NormalizeRoleTag(roleTag);
        switch (normalizedRoleTag)
        {
            case "warrior":
                return new PrototypeRpgGrowthChoiceRuleDefinition(
                    "warrior_hold",
                    "warrior_hold",
                    "continuity_hold",
                    normalizedRoleTag,
                    "Hold Guardian Line",
                    "Fallback: the current warrior line already covers the strongest upgrade angle.",
                    "Keep the committed frontline line for the next run.",
                    string.Empty,
                    string.Empty,
                    string.Empty,
                    string.Empty,
                    true);
            case "rogue":
                return new PrototypeRpgGrowthChoiceRuleDefinition(
                    "rogue_hold",
                    "rogue_hold",
                    "continuity_hold",
                    normalizedRoleTag,
                    "Hold Shadow Line",
                    "Fallback: the current rogue line already covers the cleanest upgrade angle.",
                    "Keep the committed rogue line for the next run.",
                    string.Empty,
                    string.Empty,
                    string.Empty,
                    string.Empty,
                    true);
            case "mage":
                return new PrototypeRpgGrowthChoiceRuleDefinition(
                    "mage_hold",
                    "mage_hold",
                    "continuity_hold",
                    normalizedRoleTag,
                    "Hold Ward Line",
                    "Fallback: the current mage line already covers the safest or strongest upgrade angle.",
                    "Keep the committed mage line for the next run.",
                    string.Empty,
                    string.Empty,
                    string.Empty,
                    string.Empty,
                    true);
            case "cleric":
                return new PrototypeRpgGrowthChoiceRuleDefinition(
                    "cleric_hold",
                    "cleric_hold",
                    "continuity_hold",
                    normalizedRoleTag,
                    "Hold Choir Line",
                    "Fallback: the current cleric line already covers the clearest support upgrade angle.",
                    "Keep the committed support line for the next run.",
                    string.Empty,
                    string.Empty,
                    string.Empty,
                    string.Empty,
                    true);
            default:
                return new PrototypeRpgGrowthChoiceRuleDefinition(
                    "general_hold",
                    "general_hold",
                    "continuity_hold",
                    normalizedRoleTag,
                    "Hold Current Line",
                    "Fallback: keep the currently committed line for one more run.",
                    "Keep the committed line for the next run.",
                    string.Empty,
                    string.Empty,
                    string.Empty,
                    string.Empty,
                    true);
        }
    }

    private static void AddUnlockedRuleIfAvailable(List<PrototypeRpgGrowthChoiceRuleDefinition> rules, PrototypeRpgGrowthChoiceRuleDefinition candidate, string[] unlockedGroupKeys, string[] unlockedTierKeys)
    {
        if (rules == null || candidate == null)
        {
            return;
        }

        bool hasGroup = string.IsNullOrEmpty(candidate.UnlockGroupKey) || ContainsValue(unlockedGroupKeys, candidate.UnlockGroupKey);
        bool hasTier = string.IsNullOrEmpty(candidate.RequiredTierKey) || ContainsValue(unlockedTierKeys, candidate.RequiredTierKey);
        if (!hasGroup || !hasTier)
        {
            return;
        }

        rules.Add(candidate);
    }

    private static bool ContainsValue(string[] values, string value)
    {
        if (values == null || values.Length <= 0 || string.IsNullOrEmpty(value))
        {
            return false;
        }

        for (int i = 0; i < values.Length; i++)
        {
            if (!string.IsNullOrEmpty(values[i]) && values[i] == value)
            {
                return true;
            }
        }

        return false;
    }

    private static string NormalizeRoleTag(string roleTag)
    {
        return string.IsNullOrWhiteSpace(roleTag) ? string.Empty : roleTag.Trim().ToLowerInvariant();
    }
}

public sealed class PrototypeRpgGrowthChoiceContext
{
    public string PartyId = string.Empty;
    public string RunIdentity = string.Empty;
    public string ResultStateKey = string.Empty;
    public bool EliteDefeated;
    public bool RiskyRoute;
    public bool HasRewardCarryover;
    public string AppliedPartySummaryText = string.Empty;
    public string OfferBasisSummaryText = string.Empty;
    public string OfferRotationSummaryText = string.Empty;
    public string RecentOfferExposureSummaryText = string.Empty;
    public string OfferUnlockSummaryText = string.Empty;
    public string TierEscalationSummaryText = string.Empty;
    public string PoolExpansionSummaryText = string.Empty;
    public string PoolExhaustionSummaryText = string.Empty;
    public string BuildLaneSummaryText = string.Empty;
    public string ArchetypeCommitmentSummaryText = string.Empty;
    public string OfferCoherenceSummaryText = string.Empty;
    public string LaneRecoverySummaryText = string.Empty;
    public string SoftRespecWindowSummaryText = string.Empty;
    public string AntiTrapSafetySummaryText = string.Empty;
    public string PartyCoverageSummaryText = string.Empty;
    public string RoleOverlapWarningSummaryText = string.Empty;
    public string CrossMemberSynergySummaryText = string.Empty;
    public string PartyArchetypeSummaryText = string.Empty;
    public string FormationCoherenceSummaryText = string.Empty;
    public string CommitmentStrengthSummaryText = string.Empty;
    public string FlexRescueSummaryText = string.Empty;
    public string WeightedPrioritySummaryText = string.Empty;
    public string RareSlotSummaryText = string.Empty;
    public string HighImpactSequencingSummaryText = string.Empty;
    public string RecoverySafeguardSummaryText = string.Empty;
    public string StreakSensitivePacingSummaryText = string.Empty;
    public string ClearStreakDampeningSummaryText = string.Empty;
    public string ComebackReliefSummaryText = string.Empty;
    public string MomentumBalancingSummaryText = string.Empty;
    public string CurrentMomentumTierSummaryText = string.Empty;
    public string NextOfferIntensityHintText = string.Empty;
    public int RecentRunCount;
    public int ConsecutiveClearCount;
    public int ConsecutiveDefeatCount;
    public int ConsecutiveRetreatCount;
    public int RecentLowValueOfferCount;
    public int RecentHighImpactOfferCount;
    public int CurrentMomentumTier;
    public int ComebackReliefWindowCount;
    public int MomentumDampeningWeight;
    public string TopMissingCoverageKey = string.Empty;
    public string TopOverlapKey = string.Empty;
    public string TopSynergyKey = string.Empty;
    public string PrimaryArchetypeKey = string.Empty;
    public string SecondaryArchetypeKey = string.Empty;
    public int FormationCoherenceScore;
    public int CommitmentStrength;
    public string TopArchetypeHintKey = string.Empty;
    public string TopFlexHintKey = string.Empty;
    public PrototypeRpgMemberBuildLaneRuntimeState[] MemberBuildLanes = Array.Empty<PrototypeRpgMemberBuildLaneRuntimeState>();
    public PrototypeRpgMemberLaneRecoveryRuntimeState[] MemberLaneRecoveryStates = Array.Empty<PrototypeRpgMemberLaneRecoveryRuntimeState>();
    public PrototypeRpgMemberCoverageRuntimeState[] MemberCoverageStates = Array.Empty<PrototypeRpgMemberCoverageRuntimeState>();
    public PrototypeRpgPartyArchetypeMemberRuntimeState[] MemberArchetypeStates = Array.Empty<PrototypeRpgPartyArchetypeMemberRuntimeState>();
    public PrototypeRpgGrowthChoiceMemberContext[] Members = Array.Empty<PrototypeRpgGrowthChoiceMemberContext>();
}

public sealed class PrototypeRpgGrowthChoiceMemberContext
{
    public string MemberId = string.Empty;
    public string DisplayName = string.Empty;
    public string RoleTag = string.Empty;
    public string RoleLabel = string.Empty;
    public string CurrentGrowthTrackId = string.Empty;
    public string CurrentJobId = string.Empty;
    public string CurrentEquipmentLoadoutId = string.Empty;
    public string CurrentSkillLoadoutId = string.Empty;
    public string ContributionSummaryText = string.Empty;
    public string NotableOutcomeKey = string.Empty;
    public bool Survived;
    public bool KnockedOut;
    public int DamageDealt;
    public int DamageTaken;
    public int HealingDone;
    public int ActionCount;
    public int KillCount;
    public int RecentOfferCount;
    public string LastOfferedId = string.Empty;
    public string LastOfferedGroupId = string.Empty;
    public string LastOfferedTypeKey = string.Empty;
    public string[] RecentOfferedIds = Array.Empty<string>();
    public string[] RecentOfferedGroupIds = Array.Empty<string>();
    public string[] RecentOfferedTypeKeys = Array.Empty<string>();
    public string RecentOfferExposureSummaryText = string.Empty;
    public string[] UnlockedGroupKeys = Array.Empty<string>();
    public string[] UnlockedTierKeys = Array.Empty<string>();
    public int UnlockCount;
    public string LastEscalatedTierKey = string.Empty;
    public string RecentUnlockReasonText = string.Empty;
    public string UnlockSummaryText = string.Empty;
    public string LastAppliedOfferTypeKey = string.Empty;
    public string ResolvedPrimaryLaneKey = string.Empty;
    public string ResolvedSecondaryLaneKey = string.Empty;
    public int LaneScoreFrontline;
    public int LaneScorePrecision;
    public int LaneScoreArcane;
    public int LaneScoreSupport;
    public int LaneScoreRecovery;
    public int CommitmentDepth;
    public string RecentLaneReasonText = string.Empty;
    public string LaneSummaryText = string.Empty;
    public int RecoveryPressureScore;
    public int ConsecutiveDefeatCount;
    public int ConsecutiveRetreatCount;
    public int ConsecutiveKnockOutCount;
    public int ConsecutiveStalledOfferCount;
    public int ConsecutiveNoImprovementRunCount;
    public bool SoftRespecWindowOpen;
    public int SoftRespecWindowTier;
    public string RecoveryWindowSourceRunIdentity = string.Empty;
    public string RecentRecoveryReasonText = string.Empty;
    public string LastRecoveryTriggerKey = string.Empty;
    public string LastRecoveryResolvedRunIdentity = string.Empty;
    public int RecoveryBiasWeight;
    public int LaneLockRelaxWeight;
    public string RescueOfferGroupKey = string.Empty;
    public string[] RescueAdjacentLaneKeys = Array.Empty<string>();
    public string RecoverySafetySummaryText = string.Empty;
    public string ContributionShapeKey = string.Empty;
    public string[] CurrentCoverageTags = Array.Empty<string>();
    public string[] OverlapTags = Array.Empty<string>();
    public string[] SynergyTags = Array.Empty<string>();
    public int MissingCoverageBias;
    public int OverlapPenalty;
    public int SynergyBias;
    public string CoverageSummaryText = string.Empty;
    public string[] ArchetypeContributionTags = Array.Empty<string>();
    public int ArchetypeCommitmentWeight;
    public int ArchetypeFlexWeight;
    public string MemberArchetypeSummaryText = string.Empty;
    public int WeightedPriorityBias;
    public int RareSlotTrustBias;
    public int RecoverySafeguardBias;
    public int RecentRunCount;
    public int ConsecutiveClearCount;
    public int RecentLowValueOfferCount;
    public int RecentHighImpactOfferCount;
    public int CurrentMomentumTier;
    public int ComebackReliefWindowCount;
    public int MomentumDampeningWeight;
    public int MomentumIntensityBias;
    public int ComebackReliefBias;
    public string MomentumMemberSummaryText = string.Empty;
}

public sealed class PrototypeRpgUpgradeOfferCandidate
{
    public string OfferId = string.Empty;
    public string OfferGroupId = string.Empty;
    public string OfferTypeKey = string.Empty;
    public string MemberId = string.Empty;
    public string DisplayName = string.Empty;
    public string OfferLabel = string.Empty;
    public string SummaryText = string.Empty;
    public string ReasonText = string.Empty;
    public string ContinuitySummaryText = string.Empty;
    public string RotationReasonText = string.Empty;
    public string ExposureSummaryText = string.Empty;
    public string UnlockGroupKey = string.Empty;
    public string UnlockTierKey = string.Empty;
    public string RequiredTierKey = string.Empty;
    public string UnlockReasonText = string.Empty;
    public string TierEscalationReasonText = string.Empty;
    public string PoolExhaustionReasonText = string.Empty;
    public string TargetGrowthTrackId = string.Empty;
    public string TargetJobId = string.Empty;
    public string TargetEquipmentLoadoutId = string.Empty;
    public string TargetSkillLoadoutId = string.Empty;
    public bool IsFallback;
    public bool IsNoOp;
    public bool UsedRotationFallback;
    public bool UsedPoolExhaustionFallback;
    public bool UsedUnlockExpansion;
    public bool UsedTierEscalation;
    public int RotationScore;
    public int UnlockPriority;
    public int CommitmentWeight;
    public int ContradictionPenalty;
    public int SidegradeAllowance;
    public int RecoveryEscapeWeight;
    public string CoherenceReasonHint = string.Empty;
    public string LaneKey = string.Empty;
    public string AdjacentLaneKey = string.Empty;
    public string PrimaryLaneKeyAtSelection = string.Empty;
    public string SecondaryLaneKeyAtSelection = string.Empty;
    public string LaneCoherenceReasonText = string.Empty;
    public int LaneBiasScore;
    public int CommitmentDepthAtSelection;
    public bool IsAdjacentLaneOffer;
    public bool IsRecoveryEscapeOffer;
    public bool IsContradictoryToLane;
    public int RecoveryBiasScore;
    public string RecoveryReasonText = string.Empty;
    public string RecoveryTriggerKeyAtSelection = string.Empty;
    public int RecoveryWindowTierAtSelection;
    public bool LaneLockRelaxed;
    public bool IsRescueOffer;
    public bool MatchesRecoveryRescueGroup;
    public bool SupportsRecoveryWindow;
    public int CoverageBiasScore;
    public int OverlapPenaltyScore;
    public int SynergyBiasScore;
    public string CoverageReasonText = string.Empty;
    public string OverlapReasonText = string.Empty;
    public string SynergyReasonText = string.Empty;
    public bool ImprovesMissingCoverage;
    public bool ReducesRoleOverlap;
    public bool SupportsCrossMemberSynergy;
    public int ArchetypeBiasScore;
    public int FormationCoherenceBiasScore;
    public int PriorityWeightScore;
    public int RareSlotAllocationScore;
    public int HighImpactSequenceScore;
    public string PriorityBucketKey = string.Empty;
    public string PriorityBucketLabel = string.Empty;
    public string ArchetypeReasonText = string.Empty;
    public string FormationReasonText = string.Empty;
    public string TopPickReasonText = string.Empty;
    public string RareSlotReasonText = string.Empty;
    public string RecoverySafeguardReasonText = string.Empty;
    public bool SupportsArchetypeFollowup;
    public bool MaintainsHealthyFlex;
    public bool SupportsRescueOverride;
    public bool IsTopPriorityPick;
    public bool IsRecommendedSlot;
    public bool IsRareSlotOffer;
    public bool IsRecoverySafeguardSlot;
    public bool IsArchetypeCommitmentSlot;
    public int MomentumDampeningScore;
    public int ComebackReliefScore;
    public string MomentumReasonText = string.Empty;
    public string IntensityHintText = string.Empty;
    public bool IsComebackReliefOffer;
    public bool IsMomentumDampened;
}

public sealed class PrototypeRpgApplyReadyChoice
{
    public string OfferId = string.Empty;
    public string OfferGroupId = string.Empty;
    public string MemberId = string.Empty;
    public string DisplayName = string.Empty;
    public string AppliedGrowthTrackId = string.Empty;
    public string AppliedJobId = string.Empty;
    public string AppliedEquipmentLoadoutId = string.Empty;
    public string AppliedSkillLoadoutId = string.Empty;
    public string AppliedSummaryText = string.Empty;
    public string ContinuitySummaryText = string.Empty;
    public bool HasChanges;
}

public sealed class PrototypeRpgPostRunUpgradeOfferSurface
{
    public string RunIdentity = string.Empty;
    public string AppliedPartySummaryText = string.Empty;
    public string OfferBasisSummaryText = string.Empty;
    public string OfferRefreshSummaryText = string.Empty;
    public string ApplyReadySummaryText = string.Empty;
    public string OfferContinuitySummaryText = string.Empty;
    public string OfferRotationSummaryText = string.Empty;
    public string RecentOfferExposureSummaryText = string.Empty;
    public string OfferUnlockSummaryText = string.Empty;
    public string TierEscalationSummaryText = string.Empty;
    public string PoolExpansionSummaryText = string.Empty;
    public string PoolExhaustionSummaryText = string.Empty;
    public string BuildLaneSummaryText = string.Empty;
    public string ArchetypeCommitmentSummaryText = string.Empty;
    public string OfferCoherenceSummaryText = string.Empty;
    public string LaneRecoverySummaryText = string.Empty;
    public string SoftRespecWindowSummaryText = string.Empty;
    public string AntiTrapSafetySummaryText = string.Empty;
    public string PartyCoverageSummaryText = string.Empty;
    public string RoleOverlapWarningSummaryText = string.Empty;
    public string CrossMemberSynergySummaryText = string.Empty;
    public string PartyArchetypeSummaryText = string.Empty;
    public string FormationCoherenceSummaryText = string.Empty;
    public string CommitmentStrengthSummaryText = string.Empty;
    public string FlexRescueSummaryText = string.Empty;
    public string WeightedPrioritySummaryText = string.Empty;
    public string RareSlotSummaryText = string.Empty;
    public string HighImpactSequencingSummaryText = string.Empty;
    public string RecoverySafeguardSummaryText = string.Empty;
    public string StreakSensitivePacingSummaryText = string.Empty;
    public string ClearStreakDampeningSummaryText = string.Empty;
    public string ComebackReliefSummaryText = string.Empty;
    public string MomentumBalancingSummaryText = string.Empty;
    public string CurrentMomentumTierSummaryText = string.Empty;
    public string NextOfferIntensityHintText = string.Empty;
    public string FallbackReasonText = string.Empty;
    public PrototypeRpgUpgradeOfferCandidate[] Offers = Array.Empty<PrototypeRpgUpgradeOfferCandidate>();
    public PrototypeRpgApplyReadyChoice[] ApplyReadyChoices = Array.Empty<PrototypeRpgApplyReadyChoice>();
}
