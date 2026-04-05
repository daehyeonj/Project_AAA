using System;

public static class PrototypeRpgGrowthChoiceTriggerKeys
{
    public const string Always = "always";
    public const string Offense = "offense";
    public const string Sustain = "sustain";
    public const string Support = "support";
    public const string Recovery = "recovery";
    public const string Elite = "elite";
    public const string Risk = "risk";
    public const string Carryover = "carryover";
}

public static class PrototypeRpgUpgradeOfferTypeKeys
{
    public const string GrowthTrack = "growth_track";
    public const string Job = "job";
    public const string EquipmentLoadout = "equipment_loadout";
    public const string SkillLoadout = "skill_loadout";
}

public sealed class PrototypeRpgGrowthChoiceContext
{
    public string RunIdentity = string.Empty;
    public string ResultStateKey = string.Empty;
    public string PartyId = string.Empty;
    public string MemberId = string.Empty;
    public string DisplayName = string.Empty;
    public string RoleTag = string.Empty;
    public string RoleLabel = string.Empty;
    public string GrowthTrackId = string.Empty;
    public string JobId = string.Empty;
    public string EquipmentLoadoutId = string.Empty;
    public string SkillLoadoutId = string.Empty;
    public string RouteId = string.Empty;
    public string RouteLabel = string.Empty;
    public string EventChoice = string.Empty;
    public string TriggerKind = string.Empty;
    public int DamageDealt;
    public int DamageTaken;
    public int HealingDone;
    public int ActionCount;
    public int KillCount;
    public bool Survived;
    public bool KnockedOut;
    public bool EliteDefeated;
    public bool EliteVictor;
    public bool RiskyRoute;
    public bool HasCarryover;
    public bool LostPendingReward;
    public string ReasonText = string.Empty;
}

public sealed class PrototypeRpgGrowthChoiceRule
{
    public string RuleId = string.Empty;
    public string RuleLabel = string.Empty;
    public string GrowthTrackId = string.Empty;
    public string JobId = string.Empty;
    public string EquipmentLoadoutId = string.Empty;
    public string SkillLoadoutId = string.Empty;
    public string PartyRoleTag = string.Empty;
    public string TriggerKind = string.Empty;
    public int Priority;
    public int OfferCount = 1;
    public string OfferGroupKey = string.Empty;
    public string ExclusionGroupKey = string.Empty;
    public string DisplayHint = string.Empty;
}

public static class PrototypeRpgGrowthChoiceRuleCatalog
{
    private static readonly PrototypeRpgGrowthChoiceRule[] SharedRules =
    {
        new PrototypeRpgGrowthChoiceRule
        {
            RuleId = "growth_offer_frontline_offense",
            RuleLabel = "Frontline Pressure",
            GrowthTrackId = "growth_frontline",
            PartyRoleTag = "warrior",
            TriggerKind = PrototypeRpgGrowthChoiceTriggerKeys.Offense,
            Priority = 320,
            OfferCount = 2,
            OfferGroupKey = "frontline_growth",
            ExclusionGroupKey = "role_core",
            DisplayHint = "Frontline pressure should surface heavier assault growth offers."
        },
        new PrototypeRpgGrowthChoiceRule
        {
            RuleId = "growth_offer_precision_execution",
            RuleLabel = "Precision Execution",
            JobId = "job_rogue_novice",
            SkillLoadoutId = "skillloadout_rogue_placeholder",
            PartyRoleTag = "rogue",
            TriggerKind = PrototypeRpgGrowthChoiceTriggerKeys.Offense,
            Priority = 310,
            OfferCount = 2,
            OfferGroupKey = "precision_upgrade",
            ExclusionGroupKey = "role_core",
            DisplayHint = "Execution-heavy rogue runs should surface precision job and skill offers."
        },
        new PrototypeRpgGrowthChoiceRule
        {
            RuleId = "growth_offer_arcane_focus",
            RuleLabel = "Arcane Focus",
            SkillLoadoutId = "skillloadout_mage_placeholder",
            EquipmentLoadoutId = "equip_mage_placeholder",
            PartyRoleTag = "mage",
            TriggerKind = PrototypeRpgGrowthChoiceTriggerKeys.Offense,
            Priority = 300,
            OfferCount = 2,
            OfferGroupKey = "arcane_upgrade",
            ExclusionGroupKey = "role_core",
            DisplayHint = "Arcane pressure should surface spell and catalyst offers."
        },
        new PrototypeRpgGrowthChoiceRule
        {
            RuleId = "growth_offer_support_sanctuary",
            RuleLabel = "Sanctuary Support",
            GrowthTrackId = "growth_support",
            SkillLoadoutId = "skillloadout_cleric_placeholder",
            PartyRoleTag = "cleric",
            TriggerKind = PrototypeRpgGrowthChoiceTriggerKeys.Support,
            Priority = 300,
            OfferCount = 2,
            OfferGroupKey = "support_upgrade",
            ExclusionGroupKey = "role_core",
            DisplayHint = "Healing-heavy runs should surface support growth and loadout offers."
        },
        new PrototypeRpgGrowthChoiceRule
        {
            RuleId = "growth_offer_recovery_buffer",
            RuleLabel = "Recovery Buffer",
            EquipmentLoadoutId = string.Empty,
            TriggerKind = PrototypeRpgGrowthChoiceTriggerKeys.Recovery,
            Priority = 340,
            OfferCount = 1,
            OfferGroupKey = "recovery_upgrade",
            ExclusionGroupKey = "recovery_upgrade",
            DisplayHint = "KO or defeat should surface safer recovery-oriented upgrades."
        },
        new PrototypeRpgGrowthChoiceRule
        {
            RuleId = "growth_offer_elite_unlock",
            RuleLabel = "Elite Unlock",
            JobId = string.Empty,
            TriggerKind = PrototypeRpgGrowthChoiceTriggerKeys.Elite,
            Priority = 260,
            OfferCount = 1,
            OfferGroupKey = "elite_upgrade",
            ExclusionGroupKey = string.Empty,
            DisplayHint = "Elite victory should surface a stronger unlock-ready offer."
        },
        new PrototypeRpgGrowthChoiceRule
        {
            RuleId = "growth_offer_reward_carryover",
            RuleLabel = "Carryover Bias",
            EquipmentLoadoutId = string.Empty,
            TriggerKind = PrototypeRpgGrowthChoiceTriggerKeys.Carryover,
            Priority = 220,
            OfferCount = 1,
            OfferGroupKey = "carryover_upgrade",
            ExclusionGroupKey = string.Empty,
            DisplayHint = "Carryover reward pressure can bias the next upgrade offer."
        },
        new PrototypeRpgGrowthChoiceRule
        {
            RuleId = "growth_offer_risky_route",
            RuleLabel = "Risk Route Pressure",
            SkillLoadoutId = string.Empty,
            TriggerKind = PrototypeRpgGrowthChoiceTriggerKeys.Risk,
            Priority = 200,
            OfferCount = 1,
            OfferGroupKey = "risk_upgrade",
            ExclusionGroupKey = string.Empty,
            DisplayHint = "Risky routes can bias more aggressive post-run offers."
        }
    };

    public static PrototypeRpgGrowthChoiceRule[] GetRules()
    {
        if (SharedRules.Length <= 0)
        {
            return Array.Empty<PrototypeRpgGrowthChoiceRule>();
        }

        PrototypeRpgGrowthChoiceRule[] copies = new PrototypeRpgGrowthChoiceRule[SharedRules.Length];
        for (int i = 0; i < SharedRules.Length; i++)
        {
            PrototypeRpgGrowthChoiceRule source = SharedRules[i] ?? new PrototypeRpgGrowthChoiceRule();
            copies[i] = new PrototypeRpgGrowthChoiceRule
            {
                RuleId = source.RuleId ?? string.Empty,
                RuleLabel = source.RuleLabel ?? string.Empty,
                GrowthTrackId = source.GrowthTrackId ?? string.Empty,
                JobId = source.JobId ?? string.Empty,
                EquipmentLoadoutId = source.EquipmentLoadoutId ?? string.Empty,
                SkillLoadoutId = source.SkillLoadoutId ?? string.Empty,
                PartyRoleTag = source.PartyRoleTag ?? string.Empty,
                TriggerKind = source.TriggerKind ?? string.Empty,
                Priority = source.Priority,
                OfferCount = source.OfferCount,
                OfferGroupKey = source.OfferGroupKey ?? string.Empty,
                ExclusionGroupKey = source.ExclusionGroupKey ?? string.Empty,
                DisplayHint = source.DisplayHint ?? string.Empty
            };
        }

        return copies;
    }
}

public sealed class PrototypeRpgUpgradeOfferDefinition
{
    public string OfferId = string.Empty;
    public string MemberId = string.Empty;
    public string DisplayName = string.Empty;
    public string OfferLabel = string.Empty;
    public string OfferType = string.Empty;
    public string SourceRuleId = string.Empty;
    public string GrowthTrackId = string.Empty;
    public string JobId = string.Empty;
    public string EquipmentLoadoutId = string.Empty;
    public string SkillLoadoutId = string.Empty;
    public string EffectPreviewText = string.Empty;
    public string ReasonText = string.Empty;
    public bool IsRecommended;
    public bool IsLocked;
    public string LockReason = string.Empty;
    public string GroupKey = string.Empty;
    public string ExclusionGroupKey = string.Empty;
    public int Priority;
}

public sealed class PrototypeRpgUpgradeOfferCandidate
{
    public string OfferId = string.Empty;
    public string MemberId = string.Empty;
    public string DisplayName = string.Empty;
    public string OfferLabel = string.Empty;
    public string OfferType = string.Empty;
    public string SourceRuleId = string.Empty;
    public string GrowthTrackId = string.Empty;
    public string JobId = string.Empty;
    public string EquipmentLoadoutId = string.Empty;
    public string SkillLoadoutId = string.Empty;
    public string EffectPreviewText = string.Empty;
    public string ReasonText = string.Empty;
    public bool IsRecommended;
    public bool IsLocked;
    public string LockReason = string.Empty;
    public string GroupKey = string.Empty;
    public int Priority;
}

public sealed class PrototypeRpgUpgradeSelectionRequest
{
    public string RunIdentity = string.Empty;
    public string PartyId = string.Empty;
    public string MemberId = string.Empty;
    public string SelectedOfferId = string.Empty;
    public string SelectedOfferType = string.Empty;
    public string SourceRuleId = string.Empty;
    public string PreviewText = string.Empty;
    public string PendingApplyKey = string.Empty;
    public string WouldAffectGrowthTrackId = string.Empty;
    public string WouldAffectJobId = string.Empty;
    public string WouldAffectEquipmentLoadoutId = string.Empty;
    public string WouldAffectSkillLoadoutId = string.Empty;
}

public sealed class PrototypeRpgUpgradeSelectionPreview
{
    public string RunIdentity = string.Empty;
    public string MemberId = string.Empty;
    public string DisplayName = string.Empty;
    public string OfferLabel = string.Empty;
    public string SelectedOfferId = string.Empty;
    public string SelectedOfferType = string.Empty;
    public string SourceRuleId = string.Empty;
    public string PreviewText = string.Empty;
    public string PendingApplyKey = string.Empty;
    public string WouldAffectGrowthTrackId = string.Empty;
    public string WouldAffectJobId = string.Empty;
    public string WouldAffectEquipmentLoadoutId = string.Empty;
    public string WouldAffectSkillLoadoutId = string.Empty;
    public bool IsLocked;
    public string LockReason = string.Empty;
}

public sealed class PrototypeRpgApplyReadyUpgradeChoice
{
    public string ChoiceKey = string.Empty;
    public string MemberId = string.Empty;
    public string DisplayName = string.Empty;
    public string OfferLabel = string.Empty;
    public string SelectedOfferId = string.Empty;
    public string SelectedOfferType = string.Empty;
    public string PendingApplyKey = string.Empty;
    public bool IsReady;
    public string LockReason = string.Empty;
    public string SummaryText = string.Empty;
    public PrototypeRpgUpgradeSelectionRequest Request = new PrototypeRpgUpgradeSelectionRequest();
    public PrototypeRpgUpgradeSelectionPreview Preview = new PrototypeRpgUpgradeSelectionPreview();
}

public sealed class PrototypeRpgPostRunUpgradeOfferSurface
{
    public bool HasOfferSurface;
    public string RunIdentity = string.Empty;
    public string ResultStateKey = string.Empty;
    public string PartyId = string.Empty;
    public string DungeonId = string.Empty;
    public string DungeonLabel = string.Empty;
    public string RouteId = string.Empty;
    public string RouteLabel = string.Empty;
    public string SummaryText = string.Empty;
    public string ApplyReadySummaryText = string.Empty;
    public string[] HighlightKeys = Array.Empty<string>();
    public PrototypeRpgUpgradeOfferCandidate[] Offers = Array.Empty<PrototypeRpgUpgradeOfferCandidate>();
    public PrototypeRpgApplyReadyUpgradeChoice[] ApplyReadyChoices = Array.Empty<PrototypeRpgApplyReadyUpgradeChoice>();
}
