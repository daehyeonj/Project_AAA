using System.Collections.Generic;
using UnityEngine;

public sealed partial class StaticPlaceholderWorldView
{
    private PrototypeRpgPostRunUpgradeOfferSurface _latestRpgPostRunUpgradeOfferSurface = new PrototypeRpgPostRunUpgradeOfferSurface();
    private PrototypeRpgAppliedPartyProgressState _sessionAppliedRpgPartyProgressState = new PrototypeRpgAppliedPartyProgressState();
    private PrototypeRpgOfferExposureHistoryState _sessionRpgOfferExposureHistoryState = new PrototypeRpgOfferExposureHistoryState();
    private PrototypeRpgOfferUnlockRuntimeState _sessionRpgOfferUnlockRuntimeState = new PrototypeRpgOfferUnlockRuntimeState();
    private PrototypeRpgPartyLaneRecoveryRuntimeState _sessionRpgPartyLaneRecoveryRuntimeState = new PrototypeRpgPartyLaneRecoveryRuntimeState();

    public string CurrentAppliedIdentitySummaryText => string.IsNullOrEmpty(_sessionAppliedRpgPartyProgressState.AppliedSummaryText) ? "None" : _sessionAppliedRpgPartyProgressState.AppliedSummaryText;
    public string PostRunUpgradeOfferSummaryText => _runResultState == RunResultState.None ? "None" : (string.IsNullOrEmpty(_latestRpgPostRunUpgradeOfferSurface.OfferRefreshSummaryText) ? "None" : _latestRpgPostRunUpgradeOfferSurface.OfferRefreshSummaryText);
    public string ApplyReadyOfferSummaryText => _runResultState == RunResultState.None ? "None" : (string.IsNullOrEmpty(_latestRpgPostRunUpgradeOfferSurface.ApplyReadySummaryText) ? "None" : _latestRpgPostRunUpgradeOfferSurface.ApplyReadySummaryText);
    public string OfferContinuitySummaryText => _runResultState == RunResultState.None ? "None" : (string.IsNullOrEmpty(_latestRpgPostRunUpgradeOfferSurface.OfferContinuitySummaryText) ? "None" : _latestRpgPostRunUpgradeOfferSurface.OfferContinuitySummaryText);
    public string OfferRotationSummaryText => _runResultState == RunResultState.None ? "None" : (string.IsNullOrEmpty(_latestRpgPostRunUpgradeOfferSurface.OfferRotationSummaryText) ? "None" : _latestRpgPostRunUpgradeOfferSurface.OfferRotationSummaryText);
    public string RecentOfferExposureSummaryText => _runResultState == RunResultState.None ? "None" : (string.IsNullOrEmpty(_latestRpgPostRunUpgradeOfferSurface.RecentOfferExposureSummaryText) ? "None" : _latestRpgPostRunUpgradeOfferSurface.RecentOfferExposureSummaryText);
    public string OfferUnlockSummaryText => _runResultState == RunResultState.None ? "None" : (string.IsNullOrEmpty(_latestRpgPostRunUpgradeOfferSurface.OfferUnlockSummaryText) ? "None" : _latestRpgPostRunUpgradeOfferSurface.OfferUnlockSummaryText);
    public string TierEscalationSummaryText => _runResultState == RunResultState.None ? "None" : (string.IsNullOrEmpty(_latestRpgPostRunUpgradeOfferSurface.TierEscalationSummaryText) ? "None" : _latestRpgPostRunUpgradeOfferSurface.TierEscalationSummaryText);
    public string PoolExhaustionSummaryText => _runResultState == RunResultState.None ? "None" : (string.IsNullOrEmpty(_latestRpgPostRunUpgradeOfferSurface.PoolExhaustionSummaryText) ? "None" : _latestRpgPostRunUpgradeOfferSurface.PoolExhaustionSummaryText);
    public string CurrentLaneRecoverySummaryText => string.IsNullOrEmpty(_sessionRpgPartyLaneRecoveryRuntimeState.LaneRecoverySummaryText) ? "None" : _sessionRpgPartyLaneRecoveryRuntimeState.LaneRecoverySummaryText;
    public string CurrentSoftRespecWindowSummaryText => string.IsNullOrEmpty(_sessionRpgPartyLaneRecoveryRuntimeState.SoftRespecWindowSummaryText) ? "None" : _sessionRpgPartyLaneRecoveryRuntimeState.SoftRespecWindowSummaryText;
    public string CurrentAntiTrapSafetySummaryText => string.IsNullOrEmpty(_sessionRpgPartyLaneRecoveryRuntimeState.AntiTrapSafetySummaryText) ? "None" : _sessionRpgPartyLaneRecoveryRuntimeState.AntiTrapSafetySummaryText;
    public PrototypeRpgPostRunUpgradeOfferSurface LatestRpgPostRunUpgradeOfferSurface => CopyRpgPostRunUpgradeOfferSurface(_latestRpgPostRunUpgradeOfferSurface);
    public PrototypeRpgAppliedPartyProgressState LatestRpgAppliedPartyProgressState => CopyRpgAppliedPartyProgressState(_sessionAppliedRpgPartyProgressState);
    public PrototypeRpgOfferExposureHistoryState LatestRpgOfferExposureHistoryState => CopyRpgOfferExposureHistoryState(_sessionRpgOfferExposureHistoryState);
    public PrototypeRpgOfferUnlockRuntimeState LatestRpgOfferUnlockRuntimeState => CopyRpgOfferUnlockRuntimeState(_sessionRpgOfferUnlockRuntimeState);
    public PrototypeRpgPartyLaneRecoveryRuntimeState LatestRpgPartyLaneRecoveryRuntimeState => CopyRpgPartyLaneRecoveryRuntimeState(_sessionRpgPartyLaneRecoveryRuntimeState);

    private void ResetRpgAppliedProgressState()
    {
        _sessionAppliedRpgPartyProgressState = new PrototypeRpgAppliedPartyProgressState();
        _sessionRpgOfferExposureHistoryState = new PrototypeRpgOfferExposureHistoryState();
        _sessionRpgOfferUnlockRuntimeState = new PrototypeRpgOfferUnlockRuntimeState();
        _latestRpgPostRunUpgradeOfferSurface = new PrototypeRpgPostRunUpgradeOfferSurface();
        ResetRpgBuildLaneRuntimeState();
        ResetRpgLaneRecoveryRuntimeState();
        ResetRpgOfferMomentumRuntimeState();
        ResetRpgPartyCoverageRuntimeState();
        ResetRpgPartyArchetypeRuntimeState();
        ResetRpgJobSpecializationRuntimeState();
        ResetRpgBackboneRuntimeState();
        ResetRpgGearRewardSessionState();
    }

    private PrototypeRpgAppliedPartyProgressState GetOrCreateRpgAppliedPartyProgressState(PrototypeRpgPartyDefinition partyDefinition)
    {
        if (partyDefinition == null)
        {
            return _sessionAppliedRpgPartyProgressState ?? new PrototypeRpgAppliedPartyProgressState();
        }

        bool rebuildState = _sessionAppliedRpgPartyProgressState == null ||
                            _sessionAppliedRpgPartyProgressState.Members == null ||
                            _sessionAppliedRpgPartyProgressState.PartyId != partyDefinition.PartyId;
        if (rebuildState)
        {
            PrototypeRpgAppliedPartyMemberProgressState[] members = BuildDefaultRpgAppliedPartyMembers(partyDefinition);
            _sessionAppliedRpgPartyProgressState = new PrototypeRpgAppliedPartyProgressState();
            _sessionAppliedRpgPartyProgressState.PartyId = string.IsNullOrEmpty(partyDefinition.PartyId) ? string.Empty : partyDefinition.PartyId;
            _sessionAppliedRpgPartyProgressState.DisplayName = string.IsNullOrEmpty(partyDefinition.DisplayName) ? _sessionAppliedRpgPartyProgressState.PartyId : partyDefinition.DisplayName;
            _sessionAppliedRpgPartyProgressState.Members = members;
        }
        else
        {
            EnsureRpgAppliedPartyMembers(partyDefinition, _sessionAppliedRpgPartyProgressState);
        }

        _sessionAppliedRpgPartyProgressState.AppliedSummaryText = BuildRpgAppliedPartySummaryText(_sessionAppliedRpgPartyProgressState, partyDefinition);
        return _sessionAppliedRpgPartyProgressState;
    }

    private PrototypeRpgAppliedPartyMemberProgressState[] BuildDefaultRpgAppliedPartyMembers(PrototypeRpgPartyDefinition partyDefinition)
    {
        PrototypeRpgPartyMemberDefinition[] definitions = partyDefinition != null ? (partyDefinition.Members ?? System.Array.Empty<PrototypeRpgPartyMemberDefinition>()) : System.Array.Empty<PrototypeRpgPartyMemberDefinition>();
        if (definitions.Length <= 0)
        {
            return System.Array.Empty<PrototypeRpgAppliedPartyMemberProgressState>();
        }

        PrototypeRpgAppliedPartyMemberProgressState[] members = new PrototypeRpgAppliedPartyMemberProgressState[definitions.Length];
        for (int i = 0; i < definitions.Length; i++)
        {
            PrototypeRpgPartyMemberDefinition definition = definitions[i] ?? new PrototypeRpgPartyMemberDefinition(string.Empty, "Adventurer", "adventurer", "Adventurer", i, new PrototypeRpgStatBlock(1, 1, 0, 0), string.Empty, string.Empty, string.Empty, string.Empty, string.Empty, string.Empty, string.Empty);
            PrototypeRpgAppliedPartyMemberProgressState memberState = new PrototypeRpgAppliedPartyMemberProgressState();
            memberState.MemberId = string.IsNullOrEmpty(definition.MemberId) ? string.Empty : definition.MemberId;
            memberState.DisplayName = string.IsNullOrEmpty(definition.DisplayName) ? "Adventurer" : definition.DisplayName;
            memberState.AppliedGrowthTrackId = string.IsNullOrEmpty(definition.GrowthTrackId) ? string.Empty : definition.GrowthTrackId;
            memberState.AppliedJobId = string.IsNullOrEmpty(definition.JobId) ? string.Empty : definition.JobId;
            memberState.AppliedEquipmentLoadoutId = string.IsNullOrEmpty(definition.EquipmentLoadoutId) ? string.Empty : definition.EquipmentLoadoutId;
            memberState.AppliedSkillLoadoutId = string.IsNullOrEmpty(definition.SkillLoadoutId) ? string.Empty : definition.SkillLoadoutId;
            PrototypeRpgJobSpecializationDefinition specializationDefinition = PrototypeRpgJobSpecializationCatalog.ResolveCurrentDefinition(definition.RoleTag, definition.GrowthTrackId, definition.JobId);
            memberState.AppliedPassiveSkillId = specializationDefinition != null ? specializationDefinition.PassiveSkillId : string.Empty;
            members[i] = memberState;
        }

        return members;
    }

    private void EnsureRpgAppliedPartyMembers(PrototypeRpgPartyDefinition partyDefinition, PrototypeRpgAppliedPartyProgressState appliedState)
    {
        if (partyDefinition == null || appliedState == null)
        {
            return;
        }

        PrototypeRpgPartyMemberDefinition[] definitions = partyDefinition.Members ?? System.Array.Empty<PrototypeRpgPartyMemberDefinition>();
        PrototypeRpgAppliedPartyMemberProgressState[] currentMembers = appliedState.Members ?? System.Array.Empty<PrototypeRpgAppliedPartyMemberProgressState>();
        if (currentMembers.Length == definitions.Length)
        {
            return;
        }

        PrototypeRpgAppliedPartyMemberProgressState[] rebuilt = new PrototypeRpgAppliedPartyMemberProgressState[definitions.Length];
        for (int i = 0; i < definitions.Length; i++)
        {
            PrototypeRpgPartyMemberDefinition definition = definitions[i];
            PrototypeRpgAppliedPartyMemberProgressState existing = GetRpgAppliedPartyMemberProgressState(appliedState, definition != null ? definition.MemberId : string.Empty);
            if (existing == null)
            {
                existing = new PrototypeRpgAppliedPartyMemberProgressState();
                if (definition != null)
                {
                    existing.MemberId = string.IsNullOrEmpty(definition.MemberId) ? string.Empty : definition.MemberId;
                    existing.DisplayName = string.IsNullOrEmpty(definition.DisplayName) ? "Adventurer" : definition.DisplayName;
                    existing.AppliedGrowthTrackId = string.IsNullOrEmpty(definition.GrowthTrackId) ? string.Empty : definition.GrowthTrackId;
                    existing.AppliedJobId = string.IsNullOrEmpty(definition.JobId) ? string.Empty : definition.JobId;
                    existing.AppliedEquipmentLoadoutId = string.IsNullOrEmpty(definition.EquipmentLoadoutId) ? string.Empty : definition.EquipmentLoadoutId;
                    existing.AppliedSkillLoadoutId = string.IsNullOrEmpty(definition.SkillLoadoutId) ? string.Empty : definition.SkillLoadoutId;
                    if (string.IsNullOrEmpty(existing.AppliedPassiveSkillId))
                    {
                        PrototypeRpgJobSpecializationDefinition specializationDefinition = PrototypeRpgJobSpecializationCatalog.ResolveCurrentDefinition(definition.RoleTag, definition.GrowthTrackId, definition.JobId);
                        existing.AppliedPassiveSkillId = specializationDefinition != null ? specializationDefinition.PassiveSkillId : string.Empty;
                    }
                }
            }

            rebuilt[i] = existing;
        }

        appliedState.Members = rebuilt;
    }

    private PrototypeRpgAppliedPartyMemberProgressState GetRpgAppliedPartyMemberProgressState(PrototypeRpgAppliedPartyProgressState appliedState, string memberId)
    {
        if (appliedState == null || appliedState.Members == null || string.IsNullOrEmpty(memberId))
        {
            return null;
        }

        for (int i = 0; i < appliedState.Members.Length; i++)
        {
            PrototypeRpgAppliedPartyMemberProgressState memberState = appliedState.Members[i];
            if (memberState != null && memberState.MemberId == memberId)
            {
                return memberState;
            }
        }

        return null;
    }

    private void ApplyRpgAppliedPartyMemberRuntimeOverrides(
        PrototypeRpgPartyMemberDefinition memberDefinition,
        PrototypeRpgAppliedPartyMemberProgressState appliedState,
        ref string roleLabel,
        ref string defaultSkillId,
        ref string skillName,
        ref string skillShortText,
        ref PartySkillType skillType,
        ref int maxHp,
        ref int attack,
        ref int defense,
        ref int speed,
        ref int skillPower,
        ref Color viewColor)
    {
        if (memberDefinition == null)
        {
            return;
        }

        string growthTrackId = !string.IsNullOrEmpty(appliedState != null ? appliedState.AppliedGrowthTrackId : string.Empty)
            ? appliedState.AppliedGrowthTrackId
            : memberDefinition.GrowthTrackId;
        string jobId = !string.IsNullOrEmpty(appliedState != null ? appliedState.AppliedJobId : string.Empty)
            ? appliedState.AppliedJobId
            : memberDefinition.JobId;
        string equipmentLoadoutId = !string.IsNullOrEmpty(appliedState != null ? appliedState.AppliedEquipmentLoadoutId : string.Empty)
            ? appliedState.AppliedEquipmentLoadoutId
            : memberDefinition.EquipmentLoadoutId;
        string skillLoadoutId = !string.IsNullOrEmpty(appliedState != null ? appliedState.AppliedSkillLoadoutId : string.Empty)
            ? appliedState.AppliedSkillLoadoutId
            : memberDefinition.SkillLoadoutId;

        string resolvedRoleLabel = ResolveRpgAppliedRoleLabel(memberDefinition.RoleTag, growthTrackId, jobId);
        if (!string.IsNullOrEmpty(resolvedRoleLabel))
        {
            roleLabel = resolvedRoleLabel;
        }

        switch (growthTrackId)
        {
            case "growth_frontline_guard":
                maxHp += 3;
                defense += 1;
                break;
            case "growth_frontline_breaker":
                attack += 2;
                break;
            case "growth_precision_shadow":
                speed += 1;
                defense += 1;
                break;
            case "growth_precision_execution":
                attack += 1;
                speed += 1;
                break;
            case "growth_arcane_surge":
                attack += 2;
                break;
            case "growth_arcane_ward":
                maxHp += 2;
                defense += 1;
                break;
            case "growth_support_choir":
                skillPower += 2;
                break;
            case "growth_support_guard":
                maxHp += 2;
                defense += 1;
                break;
            case "growth_frontline_vanguard":
                attack += 2;
                speed += 1;
                break;
            case "growth_frontline_bastion":
                maxHp += 4;
                defense += 2;
                break;
            case "growth_precision_saboteur":
                attack += 1;
                speed += 2;
                break;
            case "growth_precision_phantom":
                speed += 2;
                defense += 1;
                break;
            case "growth_arcane_tempest":
                attack += 3;
                break;
            case "growth_arcane_astral":
                maxHp += 2;
                defense += 1;
                skillPower += 1;
                break;
            case "growth_support_beacon":
                skillPower += 2;
                defense += 1;
                break;
            case "growth_support_saint":
                maxHp += 2;
                skillPower += 2;
                break;
        }

        switch (jobId)
        {
            case "job_guardian":
            case "job_warden":
            case "job_spellguard":
                defense += 1;
                break;
            case "job_breaker":
            case "job_executioner":
            case "job_arcanist":
                attack += 1;
                break;
            case "job_shadow":
                speed += 1;
                break;
            case "job_chaplain":
                skillPower += 1;
                break;
            case "job_vanguard":
            case "job_saboteur":
            case "job_tempest":
                attack += 1;
                break;
            case "job_bastion":
            case "job_astral":
                defense += 1;
                break;
            case "job_phantom":
                speed += 1;
                break;
            case "job_beacon":
                skillPower += 1;
                break;
            case "job_saint":
                defense += 1;
                skillPower += 1;
                break;
        }

        switch (equipmentLoadoutId)
        {
            case "equip_warrior_shielded":
            case "equip_mage_warded":
            case "equip_cleric_guarded":
                maxHp += 2;
                defense += 1;
                break;
            case "equip_warrior_brutal":
            case "equip_rogue_finisher":
            case "equip_mage_focus":
                attack += 1;
                break;
            case "equip_rogue_evasion":
                speed += 1;
                break;
            case "equip_cleric_relic":
                skillPower += 1;
                break;
            case "equip_warrior_vanguard":
            case "equip_rogue_saboteur":
            case "equip_mage_tempest":
                attack += 1;
                speed += 1;
                break;
            case "equip_warrior_bastion":
            case "equip_mage_astral":
            case "equip_cleric_saint":
                maxHp += 2;
                defense += 1;
                break;
            case "equip_rogue_phantom":
                speed += 2;
                break;
            case "equip_cleric_beacon":
                skillPower += 2;
                break;
        }

        switch (skillLoadoutId)
        {
            case "skillloadout_warrior_guard":
                skillShortText = "Steadier frontline strike with stronger guard follow-through.";
                skillPower += 1;
                break;
            case "skillloadout_warrior_break":
                skillShortText = "Breaker swing tuned for heavier single-target pressure.";
                skillPower += 2;
                break;
            case "skillloadout_rogue_evasion":
                skillShortText = "Safer evasive finisher route that preserves tempo.";
                break;
            case "skillloadout_rogue_finisher":
                skillShortText = "Execution route that sharpens the finishing window.";
                skillPower += 1;
                break;
            case "skillloadout_mage_ward":
                skillShortText = "Warded burst that trades speed for safer casting.";
                break;
            case "skillloadout_mage_surge":
                skillShortText = "Surge burst tuned for stronger arcane output.";
                skillPower += 2;
                break;
            case "skillloadout_cleric_guard":
                skillShortText = "Protective hymn that supports steadier party sustain.";
                skillPower += 1;
                break;
            case "skillloadout_cleric_choir":
                skillShortText = "Choir hymn tuned for larger whole-party recovery.";
                skillPower += 2;
                break;
            case "skillloadout_warrior_vanguard":
                skillShortText = "Vanguard break route that trades guard for faster frontline tempo.";
                skillPower += 2;
                break;
            case "skillloadout_warrior_bastion":
                skillShortText = "Bastion route that reinforces the frontline before the follow-through.";
                skillPower += 1;
                break;
            case "skillloadout_rogue_saboteur":
                skillShortText = "Saboteur route that pressures openings with a fresher utility angle.";
                skillPower += 1;
                break;
            case "skillloadout_rogue_phantom":
                skillShortText = "Phantom route that keeps lethal tempo while staying hard to pin down.";
                break;
            case "skillloadout_mage_tempest":
                skillShortText = "Tempest route that widens the arcane burst line.";
                skillPower += 3;
                break;
            case "skillloadout_mage_astral":
                skillShortText = "Astral route that steadies casting while preserving late-run output.";
                skillPower += 1;
                break;
            case "skillloadout_cleric_beacon":
                skillShortText = "Beacon route that pushes brighter sustain and cleaner recovery timing.";
                skillPower += 2;
                break;
            case "skillloadout_cleric_saint":
                skillShortText = "Saint route that steadies the party before a stronger restorative crest.";
                skillPower += 2;
                break;
        }

        PrototypeRpgSkillDefinition skillDefinition = PrototypeRpgSkillCatalog.ResolveDefinition(defaultSkillId, memberDefinition.RoleTag);
        if (skillDefinition != null)
        {
            defaultSkillId = skillDefinition.SkillId;
            skillName = skillDefinition.DisplayName;
            skillType = ResolveSharedSkillType(skillDefinition);
            viewColor = ResolveSharedSkillViewColor(skillDefinition);
        }
    }

    private string ResolveRpgAppliedRoleLabel(string roleTag, string growthTrackId, string jobId)
    {
        switch (jobId)
        {
            case "job_guardian":
                return "Guardian";
            case "job_breaker":
                return "Breaker";
            case "job_executioner":
                return "Executioner";
            case "job_shadow":
                return "Shadow";
            case "job_arcanist":
                return "Arcanist";
            case "job_spellguard":
                return "Spellguard";
            case "job_chaplain":
                return "Chaplain";
            case "job_warden":
                return "Warden";
            case "job_vanguard":
                return "Vanguard";
            case "job_bastion":
                return "Bastion";
            case "job_saboteur":
                return "Saboteur";
            case "job_phantom":
                return "Phantom";
            case "job_tempest":
                return "Tempest";
            case "job_astral":
                return "Astral";
            case "job_beacon":
                return "Beacon";
            case "job_saint":
                return "Saint";
        }

        switch (growthTrackId)
        {
            case "growth_frontline_guard":
                return "Guardian";
            case "growth_frontline_breaker":
                return "Breaker";
            case "growth_precision_shadow":
                return "Shadow";
            case "growth_precision_execution":
                return "Executioner";
            case "growth_arcane_surge":
                return "Arcanist";
            case "growth_arcane_ward":
                return "Spellguard";
            case "growth_support_choir":
                return "Chaplain";
            case "growth_support_guard":
                return "Warden";
            case "growth_frontline_vanguard":
                return "Vanguard";
            case "growth_frontline_bastion":
                return "Bastion";
            case "growth_precision_saboteur":
                return "Saboteur";
            case "growth_precision_phantom":
                return "Phantom";
            case "growth_arcane_tempest":
                return "Tempest";
            case "growth_arcane_astral":
                return "Astral";
            case "growth_support_beacon":
                return "Beacon";
            case "growth_support_saint":
                return "Saint";
            default:
                return string.IsNullOrEmpty(roleTag) ? "Adventurer" : char.ToUpperInvariant(roleTag[0]) + roleTag.Substring(1);
        }
    }

    private string BuildRpgProgressionIdLabel(string id)
    {
        if (string.IsNullOrEmpty(id))
        {
            return "None";
        }

        string text = id.Replace("growth_", string.Empty)
                        .Replace("job_", string.Empty)
                        .Replace("equip_", string.Empty)
                        .Replace("skillloadout_", string.Empty)
                        .Replace("_", " ");
        string[] parts = text.Split(new[] { ' ' }, System.StringSplitOptions.RemoveEmptyEntries);
        if (parts.Length <= 0)
        {
            return "None";
        }

        string label = string.Empty;
        for (int i = 0; i < parts.Length; i++)
        {
            string part = parts[i];
            if (string.IsNullOrEmpty(part))
            {
                continue;
            }

            string titled = char.ToUpperInvariant(part[0]) + (part.Length > 1 ? part.Substring(1) : string.Empty);
            label = string.IsNullOrEmpty(label) ? titled : label + " " + titled;
        }

        return string.IsNullOrEmpty(label) ? "None" : label;
    }

    private string BuildRpgAppliedMemberIdentitySummaryText(PrototypeRpgPartyMemberDefinition memberDefinition, PrototypeRpgAppliedPartyMemberProgressState memberState)
    {
        if (memberDefinition == null)
        {
            return "None";
        }

        string growthTrackId = !string.IsNullOrEmpty(memberState != null ? memberState.AppliedGrowthTrackId : string.Empty) ? memberState.AppliedGrowthTrackId : memberDefinition.GrowthTrackId;
        string jobId = !string.IsNullOrEmpty(memberState != null ? memberState.AppliedJobId : string.Empty) ? memberState.AppliedJobId : memberDefinition.JobId;
        string equipmentLoadoutId = !string.IsNullOrEmpty(memberState != null ? memberState.AppliedEquipmentLoadoutId : string.Empty) ? memberState.AppliedEquipmentLoadoutId : memberDefinition.EquipmentLoadoutId;
        string skillLoadoutId = !string.IsNullOrEmpty(memberState != null ? memberState.AppliedSkillLoadoutId : string.Empty) ? memberState.AppliedSkillLoadoutId : memberDefinition.SkillLoadoutId;
        string roleLabel = ResolveRpgAppliedRoleLabel(memberDefinition.RoleTag, growthTrackId, jobId);
        return BuildRpgCompactSummaryText(
            memberDefinition.DisplayName + " -> " + roleLabel,
            BuildRpgProgressionIdLabel(growthTrackId),
            BuildRpgProgressionIdLabel(equipmentLoadoutId),
            BuildRpgProgressionIdLabel(skillLoadoutId),
            memberState != null ? memberState.SpecializationSummaryText : string.Empty,
            memberState != null ? memberState.PassiveSkillSlotSummaryText : string.Empty);
    }

    private string BuildRpgAppliedPartySummaryText(PrototypeRpgAppliedPartyProgressState appliedState, PrototypeRpgPartyDefinition partyDefinition)
    {
        if (appliedState == null || partyDefinition == null || partyDefinition.Members == null || partyDefinition.Members.Length <= 0)
        {
            return "No applied progression.";
        }

        string summary = string.Empty;
        for (int i = 0; i < partyDefinition.Members.Length; i++)
        {
            PrototypeRpgPartyMemberDefinition memberDefinition = partyDefinition.Members[i];
            if (memberDefinition == null)
            {
                continue;
            }

            PrototypeRpgAppliedPartyMemberProgressState memberState = GetRpgAppliedPartyMemberProgressState(appliedState, memberDefinition.MemberId);
            string memberSummary = BuildRpgAppliedMemberIdentitySummaryText(memberDefinition, memberState);
            summary = string.IsNullOrEmpty(summary) ? memberSummary : summary + " || " + memberSummary;
        }

        return string.IsNullOrEmpty(summary) ? "No applied progression." : summary;
    }

    private PrototypeRpgAppliedPartyProgressState CopyRpgAppliedPartyProgressState(PrototypeRpgAppliedPartyProgressState source)
    {
        PrototypeRpgAppliedPartyProgressState copy = new PrototypeRpgAppliedPartyProgressState();
        if (source == null)
        {
            return copy;
        }

        copy.PartyId = string.IsNullOrEmpty(source.PartyId) ? string.Empty : source.PartyId;
        copy.DisplayName = string.IsNullOrEmpty(source.DisplayName) ? string.Empty : source.DisplayName;
        copy.LastCommittedRunIdentity = string.IsNullOrEmpty(source.LastCommittedRunIdentity) ? string.Empty : source.LastCommittedRunIdentity;
        copy.LastCommittedResultStateKey = string.IsNullOrEmpty(source.LastCommittedResultStateKey) ? string.Empty : source.LastCommittedResultStateKey;
        copy.AppliedSummaryText = string.IsNullOrEmpty(source.AppliedSummaryText) ? string.Empty : source.AppliedSummaryText;
        copy.OfferContinuitySummaryText = string.IsNullOrEmpty(source.OfferContinuitySummaryText) ? string.Empty : source.OfferContinuitySummaryText;
        PrototypeRpgAppliedPartyMemberProgressState[] members = source.Members ?? System.Array.Empty<PrototypeRpgAppliedPartyMemberProgressState>();
        if (members.Length <= 0)
        {
            copy.Members = System.Array.Empty<PrototypeRpgAppliedPartyMemberProgressState>();
            return copy;
        }

        PrototypeRpgAppliedPartyMemberProgressState[] memberCopies = new PrototypeRpgAppliedPartyMemberProgressState[members.Length];
        for (int i = 0; i < members.Length; i++)
        {
            memberCopies[i] = CopyRpgAppliedPartyMemberProgressState(members[i]);
        }

        copy.Members = memberCopies;
        return copy;
    }

    private PrototypeRpgOfferExposureHistoryState CopyRpgOfferExposureHistoryState(PrototypeRpgOfferExposureHistoryState source)
    {
        PrototypeRpgOfferExposureHistoryState copy = new PrototypeRpgOfferExposureHistoryState();
        if (source == null)
        {
            return copy;
        }

        copy.SessionKey = string.IsNullOrEmpty(source.SessionKey) ? string.Empty : source.SessionKey;
        copy.PartyId = string.IsNullOrEmpty(source.PartyId) ? string.Empty : source.PartyId;
        copy.DisplayName = string.IsNullOrEmpty(source.DisplayName) ? string.Empty : source.DisplayName;
        copy.LastRunIdentity = string.IsNullOrEmpty(source.LastRunIdentity) ? string.Empty : source.LastRunIdentity;
        copy.SummaryText = string.IsNullOrEmpty(source.SummaryText) ? string.Empty : source.SummaryText;
        PrototypeRpgMemberOfferExposureState[] members = source.Members ?? System.Array.Empty<PrototypeRpgMemberOfferExposureState>();
        if (members.Length <= 0)
        {
            copy.Members = System.Array.Empty<PrototypeRpgMemberOfferExposureState>();
            return copy;
        }

        PrototypeRpgMemberOfferExposureState[] memberCopies = new PrototypeRpgMemberOfferExposureState[members.Length];
        for (int i = 0; i < members.Length; i++)
        {
            memberCopies[i] = CopyRpgMemberOfferExposureState(members[i]);
        }

        copy.Members = memberCopies;
        return copy;
    }

    private PrototypeRpgMemberOfferExposureState CopyRpgMemberOfferExposureState(PrototypeRpgMemberOfferExposureState source)
    {
        PrototypeRpgMemberOfferExposureState copy = new PrototypeRpgMemberOfferExposureState();
        if (source == null)
        {
            return copy;
        }

        copy.MemberId = string.IsNullOrEmpty(source.MemberId) ? string.Empty : source.MemberId;
        copy.DisplayName = string.IsNullOrEmpty(source.DisplayName) ? string.Empty : source.DisplayName;
        copy.RecentlyOfferedIds = CopyRpgStringArray(source.RecentlyOfferedIds);
        copy.RecentlyOfferedGroupKeys = CopyRpgStringArray(source.RecentlyOfferedGroupKeys);
        copy.RecentlyOfferedTypeKeys = CopyRpgStringArray(source.RecentlyOfferedTypeKeys);
        copy.RecentRunIdentity = string.IsNullOrEmpty(source.RecentRunIdentity) ? string.Empty : source.RecentRunIdentity;
        copy.RecentOfferCount = source.RecentOfferCount;
        copy.RecentSelectedOfferId = string.IsNullOrEmpty(source.RecentSelectedOfferId) ? string.Empty : source.RecentSelectedOfferId;
        copy.ExposureSummaryText = string.IsNullOrEmpty(source.ExposureSummaryText) ? string.Empty : source.ExposureSummaryText;
        return copy;
    }

    private PrototypeRpgOfferExposureHistoryState GetOrCreateRpgOfferExposureHistoryState(PrototypeRpgPartyDefinition partyDefinition)
    {
        if (partyDefinition == null)
        {
            return _sessionRpgOfferExposureHistoryState ?? new PrototypeRpgOfferExposureHistoryState();
        }

        bool rebuildState = _sessionRpgOfferExposureHistoryState == null || _sessionRpgOfferExposureHistoryState.Members == null || _sessionRpgOfferExposureHistoryState.PartyId != partyDefinition.PartyId;
        if (rebuildState)
        {
            _sessionRpgOfferExposureHistoryState = new PrototypeRpgOfferExposureHistoryState();
            _sessionRpgOfferExposureHistoryState.SessionKey = "runtime-session";
            _sessionRpgOfferExposureHistoryState.PartyId = string.IsNullOrEmpty(partyDefinition.PartyId) ? string.Empty : partyDefinition.PartyId;
            _sessionRpgOfferExposureHistoryState.DisplayName = string.IsNullOrEmpty(partyDefinition.DisplayName) ? _sessionRpgOfferExposureHistoryState.PartyId : partyDefinition.DisplayName;
            _sessionRpgOfferExposureHistoryState.Members = BuildDefaultRpgOfferExposureMembers(partyDefinition);
        }
        else
        {
            EnsureRpgOfferExposureMembers(partyDefinition, _sessionRpgOfferExposureHistoryState);
        }

        _sessionRpgOfferExposureHistoryState.SummaryText = BuildRpgOfferExposureSummaryText(_sessionRpgOfferExposureHistoryState);
        return _sessionRpgOfferExposureHistoryState;
    }

    private PrototypeRpgMemberOfferExposureState[] BuildDefaultRpgOfferExposureMembers(PrototypeRpgPartyDefinition partyDefinition)
    {
        PrototypeRpgPartyMemberDefinition[] definitions = partyDefinition != null ? (partyDefinition.Members ?? System.Array.Empty<PrototypeRpgPartyMemberDefinition>()) : System.Array.Empty<PrototypeRpgPartyMemberDefinition>();
        if (definitions.Length <= 0)
        {
            return System.Array.Empty<PrototypeRpgMemberOfferExposureState>();
        }

        PrototypeRpgMemberOfferExposureState[] members = new PrototypeRpgMemberOfferExposureState[definitions.Length];
        for (int i = 0; i < definitions.Length; i++)
        {
            PrototypeRpgPartyMemberDefinition definition = definitions[i];
            PrototypeRpgMemberOfferExposureState memberState = new PrototypeRpgMemberOfferExposureState();
            memberState.MemberId = string.IsNullOrEmpty(definition != null ? definition.MemberId : string.Empty) ? string.Empty : definition.MemberId;
            memberState.DisplayName = string.IsNullOrEmpty(definition != null ? definition.DisplayName : string.Empty) ? "Adventurer" : definition.DisplayName;
            members[i] = memberState;
        }

        return members;
    }

    private void EnsureRpgOfferExposureMembers(PrototypeRpgPartyDefinition partyDefinition, PrototypeRpgOfferExposureHistoryState exposureState)
    {
        if (partyDefinition == null || exposureState == null)
        {
            return;
        }

        PrototypeRpgPartyMemberDefinition[] definitions = partyDefinition.Members ?? System.Array.Empty<PrototypeRpgPartyMemberDefinition>();
        PrototypeRpgMemberOfferExposureState[] currentMembers = exposureState.Members ?? System.Array.Empty<PrototypeRpgMemberOfferExposureState>();
        if (currentMembers.Length == definitions.Length)
        {
            return;
        }

        PrototypeRpgMemberOfferExposureState[] rebuilt = new PrototypeRpgMemberOfferExposureState[definitions.Length];
        for (int i = 0; i < definitions.Length; i++)
        {
            PrototypeRpgPartyMemberDefinition definition = definitions[i];
            PrototypeRpgMemberOfferExposureState existing = GetRpgMemberOfferExposureState(exposureState, definition != null ? definition.MemberId : string.Empty);
            if (existing == null)
            {
                existing = new PrototypeRpgMemberOfferExposureState();
                if (definition != null)
                {
                    existing.MemberId = string.IsNullOrEmpty(definition.MemberId) ? string.Empty : definition.MemberId;
                    existing.DisplayName = string.IsNullOrEmpty(definition.DisplayName) ? "Adventurer" : definition.DisplayName;
                }
            }

            rebuilt[i] = existing;
        }

        exposureState.Members = rebuilt;
    }

    private PrototypeRpgMemberOfferExposureState GetRpgMemberOfferExposureState(PrototypeRpgOfferExposureHistoryState exposureState, string memberId)
    {
        if (exposureState == null || exposureState.Members == null || string.IsNullOrEmpty(memberId))
        {
            return null;
        }

        for (int i = 0; i < exposureState.Members.Length; i++)
        {
            PrototypeRpgMemberOfferExposureState memberState = exposureState.Members[i];
            if (memberState != null && memberState.MemberId == memberId)
            {
                return memberState;
            }
        }

        return null;
    }

    private string[] CopyRpgStringArray(string[] source)
    {
        if (source == null || source.Length <= 0)
        {
            return System.Array.Empty<string>();
        }

        string[] copy = new string[source.Length];
        for (int i = 0; i < source.Length; i++)
        {
            copy[i] = string.IsNullOrEmpty(source[i]) ? string.Empty : source[i];
        }

        return copy;
    }

    private string GetMostRecentRpgHistoryValue(string[] values)
    {
        if (values == null || values.Length <= 0)
        {
            return string.Empty;
        }

        for (int i = 0; i < values.Length; i++)
        {
            if (!string.IsNullOrEmpty(values[i]))
            {
                return values[i];
            }
        }

        return string.Empty;
    }

    private string[] PushRpgRecentHistoryValue(string[] values, string value, int maxCount)
    {
        if (maxCount <= 0)
        {
            return System.Array.Empty<string>();
        }

        List<string> next = new List<string>();
        if (!string.IsNullOrEmpty(value))
        {
            next.Add(value);
        }

        string[] currentValues = values ?? System.Array.Empty<string>();
        for (int i = 0; i < currentValues.Length && next.Count < maxCount; i++)
        {
            string currentValue = currentValues[i];
            if (string.IsNullOrEmpty(currentValue) || next.Contains(currentValue))
            {
                continue;
            }

            next.Add(currentValue);
        }

        return next.Count <= 0 ? System.Array.Empty<string>() : next.ToArray();
    }

    private string BuildRpgOfferExposureSummaryText(PrototypeRpgOfferExposureHistoryState exposureState)
    {
        if (exposureState == null || exposureState.Members == null || exposureState.Members.Length <= 0)
        {
            return "Recent exposure: none.";
        }

        int trackedMemberCount = 0;
        int totalOfferCount = 0;
        string busiestMember = string.Empty;
        int busiestCount = 0;
        for (int i = 0; i < exposureState.Members.Length; i++)
        {
            PrototypeRpgMemberOfferExposureState memberState = exposureState.Members[i];
            if (memberState == null || memberState.RecentOfferCount <= 0)
            {
                continue;
            }

            trackedMemberCount += 1;
            totalOfferCount += memberState.RecentOfferCount;
            if (memberState.RecentOfferCount > busiestCount)
            {
                busiestCount = memberState.RecentOfferCount;
                busiestMember = string.IsNullOrEmpty(memberState.DisplayName) ? "Adventurer" : memberState.DisplayName;
            }
        }

        if (trackedMemberCount <= 0)
        {
            return "Recent exposure: none.";
        }

        string summary = "Recent exposure: " + trackedMemberCount + " member(s), " + totalOfferCount + " tracked offer(s).";
        if (!string.IsNullOrEmpty(busiestMember) && busiestCount > 0)
        {
            summary += " Highest pressure: " + busiestMember + " x" + busiestCount + ".";
        }

        return summary;
    }

    private string BuildRpgOfferExposureMemberSummaryText(PrototypeRpgMemberOfferExposureState memberState)
    {
        if (memberState == null)
        {
            return "No recent offers.";
        }

        return BuildRpgOfferExposureMemberSummaryText(
            string.IsNullOrEmpty(memberState.DisplayName) ? "Adventurer" : memberState.DisplayName,
            memberState.RecentOfferCount,
            GetMostRecentRpgHistoryValue(memberState.RecentlyOfferedTypeKeys),
            GetMostRecentRpgHistoryValue(memberState.RecentlyOfferedGroupKeys));
    }

    private string BuildRpgOfferExposureMemberSummaryText(string displayName, int recentOfferCount, string offerTypeKey, string offerGroupKey)
    {
        if (recentOfferCount <= 0)
        {
            return string.IsNullOrEmpty(displayName) ? "No recent offers." : displayName + ": no recent offers.";
        }

        string typeLabel = BuildRpgProgressionIdLabel(offerTypeKey);
        string groupLabel = BuildRpgProgressionIdLabel(offerGroupKey);
        return (string.IsNullOrEmpty(displayName) ? "Adventurer" : displayName) + ": " + recentOfferCount + " recent offer(s), last " + typeLabel + " / " + groupLabel + ".";
    }

    private PrototypeRpgOfferUnlockRuntimeState CopyRpgOfferUnlockRuntimeState(PrototypeRpgOfferUnlockRuntimeState source)
    {
        PrototypeRpgOfferUnlockRuntimeState copy = new PrototypeRpgOfferUnlockRuntimeState();
        if (source == null)
        {
            return copy;
        }

        copy.SessionKey = string.IsNullOrEmpty(source.SessionKey) ? string.Empty : source.SessionKey;
        copy.PartyId = string.IsNullOrEmpty(source.PartyId) ? string.Empty : source.PartyId;
        copy.DisplayName = string.IsNullOrEmpty(source.DisplayName) ? string.Empty : source.DisplayName;
        copy.LastRunIdentity = string.IsNullOrEmpty(source.LastRunIdentity) ? string.Empty : source.LastRunIdentity;
        copy.OfferUnlockSummaryText = string.IsNullOrEmpty(source.OfferUnlockSummaryText) ? string.Empty : source.OfferUnlockSummaryText;
        copy.TierEscalationSummaryText = string.IsNullOrEmpty(source.TierEscalationSummaryText) ? string.Empty : source.TierEscalationSummaryText;
        copy.PoolExpansionSummaryText = string.IsNullOrEmpty(source.PoolExpansionSummaryText) ? string.Empty : source.PoolExpansionSummaryText;
        PrototypeRpgMemberOfferUnlockState[] members = source.Members ?? System.Array.Empty<PrototypeRpgMemberOfferUnlockState>();
        if (members.Length > 0)
        {
            PrototypeRpgMemberOfferUnlockState[] copies = new PrototypeRpgMemberOfferUnlockState[members.Length];
            for (int i = 0; i < members.Length; i++)
            {
                copies[i] = CopyRpgMemberOfferUnlockState(members[i]);
            }

            copy.Members = copies;
        }
        else
        {
            copy.Members = System.Array.Empty<PrototypeRpgMemberOfferUnlockState>();
        }

        return copy;
    }

    private PrototypeRpgMemberOfferUnlockState CopyRpgMemberOfferUnlockState(PrototypeRpgMemberOfferUnlockState source)
    {
        PrototypeRpgMemberOfferUnlockState copy = new PrototypeRpgMemberOfferUnlockState();
        if (source == null)
        {
            return copy;
        }

        copy.MemberId = string.IsNullOrEmpty(source.MemberId) ? string.Empty : source.MemberId;
        copy.DisplayName = string.IsNullOrEmpty(source.DisplayName) ? string.Empty : source.DisplayName;
        copy.UnlockedGroupKeys = CopyRpgStringArray(source.UnlockedGroupKeys);
        copy.UnlockedTierKeys = CopyRpgStringArray(source.UnlockedTierKeys);
        copy.UnlockedOfferIds = CopyRpgStringArray(source.UnlockedOfferIds);
        copy.RecentUnlockReasonText = string.IsNullOrEmpty(source.RecentUnlockReasonText) ? string.Empty : source.RecentUnlockReasonText;
        copy.LastUnlockRunIdentity = string.IsNullOrEmpty(source.LastUnlockRunIdentity) ? string.Empty : source.LastUnlockRunIdentity;
        copy.UnlockCount = source.UnlockCount;
        copy.LastEscalatedTierKey = string.IsNullOrEmpty(source.LastEscalatedTierKey) ? string.Empty : source.LastEscalatedTierKey;
        copy.UnlockSummaryText = string.IsNullOrEmpty(source.UnlockSummaryText) ? string.Empty : source.UnlockSummaryText;
        return copy;
    }

    private PrototypeRpgOfferUnlockRuntimeState GetOrCreateRpgOfferUnlockRuntimeState(PrototypeRpgPartyDefinition partyDefinition)
    {
        if (partyDefinition == null)
        {
            return _sessionRpgOfferUnlockRuntimeState ?? new PrototypeRpgOfferUnlockRuntimeState();
        }

        bool rebuildState = _sessionRpgOfferUnlockRuntimeState == null ||
                            _sessionRpgOfferUnlockRuntimeState.Members == null ||
                            _sessionRpgOfferUnlockRuntimeState.PartyId != partyDefinition.PartyId;
        if (rebuildState)
        {
            PrototypeRpgMemberOfferUnlockState[] members = BuildDefaultRpgOfferUnlockMembers(partyDefinition);
            _sessionRpgOfferUnlockRuntimeState = new PrototypeRpgOfferUnlockRuntimeState();
            _sessionRpgOfferUnlockRuntimeState.SessionKey = string.IsNullOrEmpty(partyDefinition.PartyId) ? string.Empty : partyDefinition.PartyId;
            _sessionRpgOfferUnlockRuntimeState.PartyId = string.IsNullOrEmpty(partyDefinition.PartyId) ? string.Empty : partyDefinition.PartyId;
            _sessionRpgOfferUnlockRuntimeState.DisplayName = string.IsNullOrEmpty(partyDefinition.DisplayName) ? _sessionRpgOfferUnlockRuntimeState.PartyId : partyDefinition.DisplayName;
            _sessionRpgOfferUnlockRuntimeState.Members = members;
        }
        else
        {
            EnsureRpgOfferUnlockMembers(partyDefinition, _sessionRpgOfferUnlockRuntimeState);
        }

        return _sessionRpgOfferUnlockRuntimeState;
    }

    private PrototypeRpgMemberOfferUnlockState[] BuildDefaultRpgOfferUnlockMembers(PrototypeRpgPartyDefinition partyDefinition)
    {
        PrototypeRpgPartyMemberDefinition[] definitions = partyDefinition != null ? (partyDefinition.Members ?? System.Array.Empty<PrototypeRpgPartyMemberDefinition>()) : System.Array.Empty<PrototypeRpgPartyMemberDefinition>();
        if (definitions.Length <= 0)
        {
            return System.Array.Empty<PrototypeRpgMemberOfferUnlockState>();
        }

        PrototypeRpgMemberOfferUnlockState[] members = new PrototypeRpgMemberOfferUnlockState[definitions.Length];
        for (int i = 0; i < definitions.Length; i++)
        {
            PrototypeRpgPartyMemberDefinition definition = definitions[i];
            PrototypeRpgMemberOfferUnlockState memberState = new PrototypeRpgMemberOfferUnlockState();
            memberState.MemberId = string.IsNullOrEmpty(definition != null ? definition.MemberId : string.Empty) ? string.Empty : definition.MemberId;
            memberState.DisplayName = string.IsNullOrEmpty(definition != null ? definition.DisplayName : string.Empty) ? "Adventurer" : definition.DisplayName;
            memberState.UnlockedTierKeys = new[] { "tier_1" };
            members[i] = memberState;
        }

        return members;
    }

    private void EnsureRpgOfferUnlockMembers(PrototypeRpgPartyDefinition partyDefinition, PrototypeRpgOfferUnlockRuntimeState unlockState)
    {
        if (partyDefinition == null || unlockState == null)
        {
            return;
        }

        PrototypeRpgPartyMemberDefinition[] definitions = partyDefinition.Members ?? System.Array.Empty<PrototypeRpgPartyMemberDefinition>();
        PrototypeRpgMemberOfferUnlockState[] currentMembers = unlockState.Members ?? System.Array.Empty<PrototypeRpgMemberOfferUnlockState>();
        if (currentMembers.Length == definitions.Length)
        {
            return;
        }

        PrototypeRpgMemberOfferUnlockState[] rebuilt = new PrototypeRpgMemberOfferUnlockState[definitions.Length];
        for (int i = 0; i < definitions.Length; i++)
        {
            PrototypeRpgPartyMemberDefinition definition = definitions[i];
            PrototypeRpgMemberOfferUnlockState existing = GetRpgMemberOfferUnlockState(unlockState, definition != null ? definition.MemberId : string.Empty);
            if (existing == null)
            {
                existing = new PrototypeRpgMemberOfferUnlockState();
                if (definition != null)
                {
                    existing.MemberId = string.IsNullOrEmpty(definition.MemberId) ? string.Empty : definition.MemberId;
                    existing.DisplayName = string.IsNullOrEmpty(definition.DisplayName) ? "Adventurer" : definition.DisplayName;
                }

                existing.UnlockedTierKeys = new[] { "tier_1" };
            }

            rebuilt[i] = existing;
        }

        unlockState.Members = rebuilt;
    }

    private PrototypeRpgMemberOfferUnlockState GetRpgMemberOfferUnlockState(PrototypeRpgOfferUnlockRuntimeState unlockState, string memberId)
    {
        if (unlockState == null || unlockState.Members == null || string.IsNullOrEmpty(memberId))
        {
            return null;
        }

        for (int i = 0; i < unlockState.Members.Length; i++)
        {
            PrototypeRpgMemberOfferUnlockState memberState = unlockState.Members[i];
            if (memberState != null && memberState.MemberId == memberId)
            {
                return memberState;
            }
        }

        return null;
    }

    private bool TryUnlockRpgGroup(PrototypeRpgMemberOfferUnlockState memberState, string groupKey)
    {
        if (memberState == null || string.IsNullOrEmpty(groupKey) || ArrayContainsRpgValue(memberState.UnlockedGroupKeys, groupKey))
        {
            return false;
        }

        memberState.UnlockedGroupKeys = PushRpgRecentHistoryValue(memberState.UnlockedGroupKeys, groupKey, 12);
        return true;
    }

    private bool TryUnlockRpgTier(PrototypeRpgMemberOfferUnlockState memberState, string tierKey)
    {
        if (memberState == null || string.IsNullOrEmpty(tierKey) || ArrayContainsRpgValue(memberState.UnlockedTierKeys, tierKey))
        {
            return false;
        }

        memberState.UnlockedTierKeys = PushRpgRecentHistoryValue(memberState.UnlockedTierKeys, tierKey, 6);
        return true;
    }

    private string ResolveRpgHighestUnlockedTierKey(string[] tierKeys)
    {
        if (ArrayContainsRpgValue(tierKeys, "tier_3"))
        {
            return "tier_3";
        }

        if (ArrayContainsRpgValue(tierKeys, "tier_2"))
        {
            return "tier_2";
        }

        return "tier_1";
    }

    private string BuildRpgOfferUnlockReasonText(PrototypeRpgGrowthChoiceMemberContext member, PrototypeRpgGrowthChoiceContext context, string unlockStage)
    {
        string displayName = string.IsNullOrEmpty(member != null ? member.DisplayName : string.Empty) ? "Adventurer" : member.DisplayName;
        switch (unlockStage)
        {
            case "tier_3":
                if (context != null && context.EliteDefeated)
                {
                    return displayName + ": elite clear escalated the offer pool to Tier 3.";
                }

                if (context != null && context.HasRewardCarryover)
                {
                    return displayName + ": carryover pressure escalated the offer pool to Tier 3.";
                }

                return displayName + ": long-session momentum escalated the offer pool to Tier 3.";
            case "group_tier_3":
                return displayName + ": apex branch unlocked after extended session pressure.";
            case "tier_2":
                if (context != null && context.RiskyRoute)
                {
                    return displayName + ": risky route pressure opened Tier 2 branches.";
                }

                if (context != null && (context.ResultStateKey == PrototypeBattleOutcomeKeys.RunDefeat || context.ResultStateKey == PrototypeBattleOutcomeKeys.RunRetreat || (member != null && member.KnockedOut)))
                {
                    return displayName + ": recovery pressure opened Tier 2 branches.";
                }

                return displayName + ": repeated exposure opened Tier 2 branches.";
            case "group_tier_2":
                return displayName + ": a fresher side branch joined the pool.";
            default:
                return displayName + ": session variety expanded the offer pool.";
        }
    }

    private string BuildRpgOfferUnlockMemberSummaryText(PrototypeRpgMemberOfferUnlockState memberState)
    {
        if (memberState == null)
        {
            return "Unlocks: none.";
        }

        string displayName = string.IsNullOrEmpty(memberState.DisplayName) ? "Adventurer" : memberState.DisplayName;
        int groupCount = memberState.UnlockedGroupKeys != null ? memberState.UnlockedGroupKeys.Length : 0;
        string tierLabel = PrototypeRpgGrowthChoiceRuleCatalog.ResolveTierLabel(ResolveRpgHighestUnlockedTierKey(memberState.UnlockedTierKeys));
        string reasonText = string.IsNullOrEmpty(memberState.RecentUnlockReasonText) ? "No new unlocks yet." : memberState.RecentUnlockReasonText;
        return displayName + ": " + groupCount + " unlocked group(s), top tier " + tierLabel + ". " + reasonText;
    }

    private string BuildRpgOfferUnlockSummaryText(PrototypeRpgOfferUnlockRuntimeState unlockState)
    {
        if (unlockState == null || unlockState.Members == null || unlockState.Members.Length <= 0)
        {
            return "Unlocks: base pool only.";
        }

        int memberCount = 0;
        int groupCount = 0;
        for (int i = 0; i < unlockState.Members.Length; i++)
        {
            PrototypeRpgMemberOfferUnlockState member = unlockState.Members[i];
            int unlockedGroups = member != null && member.UnlockedGroupKeys != null ? member.UnlockedGroupKeys.Length : 0;
            if (unlockedGroups <= 0)
            {
                continue;
            }

            memberCount += 1;
            groupCount += unlockedGroups;
        }

        return memberCount <= 0 ? "Unlocks: base pool only." : "Unlocks: " + memberCount + " member(s) opened " + groupCount + " extra group(s).";
    }

    private string BuildRpgTierEscalationSurfaceSummary(PrototypeRpgOfferUnlockRuntimeState unlockState)
    {
        if (unlockState == null || unlockState.Members == null || unlockState.Members.Length <= 0)
        {
            return "Tier escalation: none.";
        }

        int tierTwoCount = 0;
        int tierThreeCount = 0;
        for (int i = 0; i < unlockState.Members.Length; i++)
        {
            PrototypeRpgMemberOfferUnlockState member = unlockState.Members[i];
            string tierKey = ResolveRpgHighestUnlockedTierKey(member != null ? member.UnlockedTierKeys : null);
            if (tierKey == "tier_3")
            {
                tierThreeCount += 1;
            }
            else if (tierKey == "tier_2")
            {
                tierTwoCount += 1;
            }
        }

        if (tierTwoCount <= 0 && tierThreeCount <= 0)
        {
            return "Tier escalation: base tier only.";
        }

        return "Tier escalation: " + tierTwoCount + " member(s) at Tier 2, " + tierThreeCount + " at Tier 3.";
    }

    private string BuildRpgPoolExpansionSurfaceSummary(PrototypeRpgOfferUnlockRuntimeState unlockState)
    {
        if (unlockState == null || unlockState.Members == null || unlockState.Members.Length <= 0)
        {
            return "Pool expansion: base pool only.";
        }

        int expandedGroups = 0;
        for (int i = 0; i < unlockState.Members.Length; i++)
        {
            PrototypeRpgMemberOfferUnlockState member = unlockState.Members[i];
            expandedGroups += member != null && member.UnlockedGroupKeys != null ? member.UnlockedGroupKeys.Length : 0;
        }

        return expandedGroups <= 0 ? "Pool expansion: base pool only." : "Pool expansion: " + expandedGroups + " unlocked branch slot(s) are now in rotation.";
    }

    private void EvaluateRpgOfferUnlockState(PrototypeRpgGrowthChoiceContext context, PrototypeRpgRunResultSnapshot runResultSnapshot, PrototypeRpgPartyDefinition partyDefinition)
    {
        if (context == null || partyDefinition == null)
        {
            return;
        }

        PrototypeRpgOfferUnlockRuntimeState unlockState = GetOrCreateRpgOfferUnlockRuntimeState(partyDefinition);
        unlockState.SessionKey = string.IsNullOrEmpty(context.PartyId) ? string.Empty : context.PartyId;
        unlockState.LastRunIdentity = string.IsNullOrEmpty(context.RunIdentity) ? string.Empty : context.RunIdentity;
        unlockState.DisplayName = string.IsNullOrEmpty(partyDefinition.DisplayName) ? unlockState.PartyId : partyDefinition.DisplayName;
        PrototypeRpgGrowthChoiceMemberContext[] members = context.Members ?? System.Array.Empty<PrototypeRpgGrowthChoiceMemberContext>();
        for (int i = 0; i < members.Length; i++)
        {
            PrototypeRpgGrowthChoiceMemberContext member = members[i];
            PrototypeRpgMemberOfferUnlockState memberState = GetRpgMemberOfferUnlockState(unlockState, member != null ? member.MemberId : string.Empty);
            if (member == null || memberState == null)
            {
                continue;
            }

            TryUnlockRpgTier(memberState, "tier_1");
            bool repeatedExposure = member.RecentOfferCount >= 2;
            bool adversity = context.ResultStateKey == PrototypeBattleOutcomeKeys.RunDefeat ||
                             context.ResultStateKey == PrototypeBattleOutcomeKeys.RunRetreat ||
                             member.KnockedOut ||
                             member.DamageTaken >= member.DamageDealt + 4;
            bool momentum = context.EliteDefeated ||
                            member.KillCount >= 2 ||
                            member.DamageDealt >= 18 ||
                            member.HealingDone >= 10 ||
                            context.HasRewardCarryover;
            bool riskyMomentum = context.RiskyRoute;
            bool unlockedGroup = false;
            bool escalatedTier = false;
            string reasonText = string.IsNullOrEmpty(memberState.RecentUnlockReasonText) ? string.Empty : memberState.RecentUnlockReasonText;
            string tierTwoGroup = PrototypeRpgGrowthChoiceRuleCatalog.ResolveTierTwoGroupKey(member.RoleTag);
            string tierThreeGroup = PrototypeRpgGrowthChoiceRuleCatalog.ResolveTierThreeGroupKey(member.RoleTag);
            if (repeatedExposure || riskyMomentum || adversity)
            {
                if (TryUnlockRpgTier(memberState, "tier_2"))
                {
                    escalatedTier = true;
                    memberState.LastEscalatedTierKey = "tier_2";
                    reasonText = BuildRpgOfferUnlockReasonText(member, context, "tier_2");
                }

                if (TryUnlockRpgGroup(memberState, tierTwoGroup))
                {
                    unlockedGroup = true;
                    if (string.IsNullOrEmpty(reasonText))
                    {
                        reasonText = BuildRpgOfferUnlockReasonText(member, context, "group_tier_2");
                    }
                }
            }

            if (momentum || (memberState.UnlockCount > 0 && repeatedExposure))
            {
                if (TryUnlockRpgTier(memberState, "tier_3"))
                {
                    escalatedTier = true;
                    memberState.LastEscalatedTierKey = "tier_3";
                    reasonText = BuildRpgOfferUnlockReasonText(member, context, "tier_3");
                }

                if (TryUnlockRpgGroup(memberState, tierThreeGroup))
                {
                    unlockedGroup = true;
                    if (string.IsNullOrEmpty(reasonText) || reasonText.IndexOf("Tier 3", System.StringComparison.OrdinalIgnoreCase) < 0)
                    {
                        reasonText = BuildRpgOfferUnlockReasonText(member, context, "group_tier_3");
                    }
                }
            }

            if (unlockedGroup || escalatedTier)
            {
                memberState.UnlockCount += (unlockedGroup ? 1 : 0) + (escalatedTier ? 1 : 0);
                memberState.LastUnlockRunIdentity = string.IsNullOrEmpty(context.RunIdentity) ? string.Empty : context.RunIdentity;
                memberState.RecentUnlockReasonText = string.IsNullOrEmpty(reasonText) ? memberState.RecentUnlockReasonText : reasonText;
            }

            memberState.UnlockSummaryText = BuildRpgOfferUnlockMemberSummaryText(memberState);
            member.UnlockedGroupKeys = CopyRpgStringArray(memberState.UnlockedGroupKeys);
            member.UnlockedTierKeys = CopyRpgStringArray(memberState.UnlockedTierKeys);
            member.UnlockCount = memberState.UnlockCount;
            member.LastEscalatedTierKey = string.IsNullOrEmpty(memberState.LastEscalatedTierKey) ? ResolveRpgHighestUnlockedTierKey(memberState.UnlockedTierKeys) : memberState.LastEscalatedTierKey;
            member.RecentUnlockReasonText = string.IsNullOrEmpty(memberState.RecentUnlockReasonText) ? string.Empty : memberState.RecentUnlockReasonText;
            member.UnlockSummaryText = string.IsNullOrEmpty(memberState.UnlockSummaryText) ? string.Empty : memberState.UnlockSummaryText;
        }

        unlockState.OfferUnlockSummaryText = BuildRpgOfferUnlockSummaryText(unlockState);
        unlockState.TierEscalationSummaryText = BuildRpgTierEscalationSurfaceSummary(unlockState);
        unlockState.PoolExpansionSummaryText = BuildRpgPoolExpansionSurfaceSummary(unlockState);
        context.OfferUnlockSummaryText = unlockState.OfferUnlockSummaryText;
        context.TierEscalationSummaryText = unlockState.TierEscalationSummaryText;
        context.PoolExpansionSummaryText = unlockState.PoolExpansionSummaryText;
    }
    private PrototypeRpgAppliedPartyMemberProgressState CopyRpgAppliedPartyMemberProgressState(PrototypeRpgAppliedPartyMemberProgressState source)
    {
        PrototypeRpgAppliedPartyMemberProgressState copy = new PrototypeRpgAppliedPartyMemberProgressState();
        if (source == null)
        {
            return copy;
        }

        copy.MemberId = string.IsNullOrEmpty(source.MemberId) ? string.Empty : source.MemberId;
        copy.DisplayName = string.IsNullOrEmpty(source.DisplayName) ? string.Empty : source.DisplayName;
        copy.AppliedGrowthTrackId = string.IsNullOrEmpty(source.AppliedGrowthTrackId) ? string.Empty : source.AppliedGrowthTrackId;
        copy.AppliedJobId = string.IsNullOrEmpty(source.AppliedJobId) ? string.Empty : source.AppliedJobId;
        copy.AppliedPassiveSkillId = string.IsNullOrEmpty(source.AppliedPassiveSkillId) ? string.Empty : source.AppliedPassiveSkillId;
        copy.AppliedEquipmentLoadoutId = string.IsNullOrEmpty(source.AppliedEquipmentLoadoutId) ? string.Empty : source.AppliedEquipmentLoadoutId;
        copy.AppliedSkillLoadoutId = string.IsNullOrEmpty(source.AppliedSkillLoadoutId) ? string.Empty : source.AppliedSkillLoadoutId;
        copy.LastAppliedOfferId = string.IsNullOrEmpty(source.LastAppliedOfferId) ? string.Empty : source.LastAppliedOfferId;
        copy.LastAppliedOfferGroupId = string.IsNullOrEmpty(source.LastAppliedOfferGroupId) ? string.Empty : source.LastAppliedOfferGroupId;
        copy.LastAppliedOfferTypeKey = string.IsNullOrEmpty(source.LastAppliedOfferTypeKey) ? string.Empty : source.LastAppliedOfferTypeKey;
        copy.LastUnlockedSpecializationKey = string.IsNullOrEmpty(source.LastUnlockedSpecializationKey) ? string.Empty : source.LastUnlockedSpecializationKey;
        copy.LastAppliedRunIdentity = string.IsNullOrEmpty(source.LastAppliedRunIdentity) ? string.Empty : source.LastAppliedRunIdentity;
        copy.LastAppliedSummaryText = string.IsNullOrEmpty(source.LastAppliedSummaryText) ? string.Empty : source.LastAppliedSummaryText;
        copy.SpecializationSummaryText = string.IsNullOrEmpty(source.SpecializationSummaryText) ? string.Empty : source.SpecializationSummaryText;
        copy.PassiveSkillSlotSummaryText = string.IsNullOrEmpty(source.PassiveSkillSlotSummaryText) ? string.Empty : source.PassiveSkillSlotSummaryText;
        copy.RuntimeSynergySummaryText = string.IsNullOrEmpty(source.RuntimeSynergySummaryText) ? string.Empty : source.RuntimeSynergySummaryText;
        copy.NextRunSpecializationPreviewText = string.IsNullOrEmpty(source.NextRunSpecializationPreviewText) ? string.Empty : source.NextRunSpecializationPreviewText;
        copy.UnlockedSpecializationKeys = CopyRpgStringArray(source.UnlockedSpecializationKeys);
        return copy;
    }

    private PrototypeRpgPostRunUpgradeOfferSurface CopyRpgPostRunUpgradeOfferSurface(PrototypeRpgPostRunUpgradeOfferSurface source)
    {
        PrototypeRpgPostRunUpgradeOfferSurface copy = new PrototypeRpgPostRunUpgradeOfferSurface();
        if (source == null)
        {
            return copy;
        }

        copy.RunIdentity = string.IsNullOrEmpty(source.RunIdentity) ? string.Empty : source.RunIdentity;
        copy.AppliedPartySummaryText = string.IsNullOrEmpty(source.AppliedPartySummaryText) ? string.Empty : source.AppliedPartySummaryText;
        copy.OfferBasisSummaryText = string.IsNullOrEmpty(source.OfferBasisSummaryText) ? string.Empty : source.OfferBasisSummaryText;
        copy.OfferRefreshSummaryText = string.IsNullOrEmpty(source.OfferRefreshSummaryText) ? string.Empty : source.OfferRefreshSummaryText;
        copy.ApplyReadySummaryText = string.IsNullOrEmpty(source.ApplyReadySummaryText) ? string.Empty : source.ApplyReadySummaryText;
        copy.OfferContinuitySummaryText = string.IsNullOrEmpty(source.OfferContinuitySummaryText) ? string.Empty : source.OfferContinuitySummaryText;
        copy.OfferRotationSummaryText = string.IsNullOrEmpty(source.OfferRotationSummaryText) ? string.Empty : source.OfferRotationSummaryText;
        copy.RecentOfferExposureSummaryText = string.IsNullOrEmpty(source.RecentOfferExposureSummaryText) ? string.Empty : source.RecentOfferExposureSummaryText;
        copy.OfferUnlockSummaryText = string.IsNullOrEmpty(source.OfferUnlockSummaryText) ? string.Empty : source.OfferUnlockSummaryText;
        copy.TierEscalationSummaryText = string.IsNullOrEmpty(source.TierEscalationSummaryText) ? string.Empty : source.TierEscalationSummaryText;
        copy.PoolExpansionSummaryText = string.IsNullOrEmpty(source.PoolExpansionSummaryText) ? string.Empty : source.PoolExpansionSummaryText;
        copy.PoolExhaustionSummaryText = string.IsNullOrEmpty(source.PoolExhaustionSummaryText) ? string.Empty : source.PoolExhaustionSummaryText;
        copy.BuildLaneSummaryText = string.IsNullOrEmpty(source.BuildLaneSummaryText) ? string.Empty : source.BuildLaneSummaryText;
        copy.ArchetypeCommitmentSummaryText = string.IsNullOrEmpty(source.ArchetypeCommitmentSummaryText) ? string.Empty : source.ArchetypeCommitmentSummaryText;
        copy.OfferCoherenceSummaryText = string.IsNullOrEmpty(source.OfferCoherenceSummaryText) ? string.Empty : source.OfferCoherenceSummaryText;
        copy.LaneRecoverySummaryText = string.IsNullOrEmpty(source.LaneRecoverySummaryText) ? string.Empty : source.LaneRecoverySummaryText;
        copy.SoftRespecWindowSummaryText = string.IsNullOrEmpty(source.SoftRespecWindowSummaryText) ? string.Empty : source.SoftRespecWindowSummaryText;
        copy.AntiTrapSafetySummaryText = string.IsNullOrEmpty(source.AntiTrapSafetySummaryText) ? string.Empty : source.AntiTrapSafetySummaryText;
        copy.PartyArchetypeSummaryText = string.IsNullOrEmpty(source.PartyArchetypeSummaryText) ? string.Empty : source.PartyArchetypeSummaryText;
        copy.FormationCoherenceSummaryText = string.IsNullOrEmpty(source.FormationCoherenceSummaryText) ? string.Empty : source.FormationCoherenceSummaryText;
        copy.CommitmentStrengthSummaryText = string.IsNullOrEmpty(source.CommitmentStrengthSummaryText) ? string.Empty : source.CommitmentStrengthSummaryText;
        copy.FlexRescueSummaryText = string.IsNullOrEmpty(source.FlexRescueSummaryText) ? string.Empty : source.FlexRescueSummaryText;
        copy.WeightedPrioritySummaryText = string.IsNullOrEmpty(source.WeightedPrioritySummaryText) ? string.Empty : source.WeightedPrioritySummaryText;
        copy.RareSlotSummaryText = string.IsNullOrEmpty(source.RareSlotSummaryText) ? string.Empty : source.RareSlotSummaryText;
        copy.HighImpactSequencingSummaryText = string.IsNullOrEmpty(source.HighImpactSequencingSummaryText) ? string.Empty : source.HighImpactSequencingSummaryText;
        copy.RecoverySafeguardSummaryText = string.IsNullOrEmpty(source.RecoverySafeguardSummaryText) ? string.Empty : source.RecoverySafeguardSummaryText;
        copy.StreakSensitivePacingSummaryText = string.IsNullOrEmpty(source.StreakSensitivePacingSummaryText) ? string.Empty : source.StreakSensitivePacingSummaryText;
        copy.ClearStreakDampeningSummaryText = string.IsNullOrEmpty(source.ClearStreakDampeningSummaryText) ? string.Empty : source.ClearStreakDampeningSummaryText;
        copy.ComebackReliefSummaryText = string.IsNullOrEmpty(source.ComebackReliefSummaryText) ? string.Empty : source.ComebackReliefSummaryText;
        copy.MomentumBalancingSummaryText = string.IsNullOrEmpty(source.MomentumBalancingSummaryText) ? string.Empty : source.MomentumBalancingSummaryText;
        copy.CurrentMomentumTierSummaryText = string.IsNullOrEmpty(source.CurrentMomentumTierSummaryText) ? string.Empty : source.CurrentMomentumTierSummaryText;
        copy.NextOfferIntensityHintText = string.IsNullOrEmpty(source.NextOfferIntensityHintText) ? string.Empty : source.NextOfferIntensityHintText;
        copy.PartyCoverageSummaryText = string.IsNullOrEmpty(source.PartyCoverageSummaryText) ? string.Empty : source.PartyCoverageSummaryText;
        copy.RoleOverlapWarningSummaryText = string.IsNullOrEmpty(source.RoleOverlapWarningSummaryText) ? string.Empty : source.RoleOverlapWarningSummaryText;
        copy.CrossMemberSynergySummaryText = string.IsNullOrEmpty(source.CrossMemberSynergySummaryText) ? string.Empty : source.CrossMemberSynergySummaryText;
        copy.FallbackReasonText = string.IsNullOrEmpty(source.FallbackReasonText) ? string.Empty : source.FallbackReasonText;
        PrototypeRpgUpgradeOfferCandidate[] offers = source.Offers ?? System.Array.Empty<PrototypeRpgUpgradeOfferCandidate>();
        PrototypeRpgApplyReadyChoice[] choices = source.ApplyReadyChoices ?? System.Array.Empty<PrototypeRpgApplyReadyChoice>();
        if (offers.Length > 0)
        {
            PrototypeRpgUpgradeOfferCandidate[] offerCopies = new PrototypeRpgUpgradeOfferCandidate[offers.Length];
            for (int i = 0; i < offers.Length; i++)
            {
                offerCopies[i] = CopyRpgUpgradeOfferCandidate(offers[i]);
            }

            copy.Offers = offerCopies;
        }
        else
        {
            copy.Offers = System.Array.Empty<PrototypeRpgUpgradeOfferCandidate>();
        }

        if (choices.Length > 0)
        {
            PrototypeRpgApplyReadyChoice[] choiceCopies = new PrototypeRpgApplyReadyChoice[choices.Length];
            for (int i = 0; i < choices.Length; i++)
            {
                choiceCopies[i] = CopyRpgApplyReadyChoice(choices[i]);
            }

            copy.ApplyReadyChoices = choiceCopies;
        }
        else
        {
            copy.ApplyReadyChoices = System.Array.Empty<PrototypeRpgApplyReadyChoice>();
        }

        return copy;
    }
    private PrototypeRpgUpgradeOfferCandidate CopyRpgUpgradeOfferCandidate(PrototypeRpgUpgradeOfferCandidate source)
    {
        PrototypeRpgUpgradeOfferCandidate copy = new PrototypeRpgUpgradeOfferCandidate();
        if (source == null)
        {
            return copy;
        }

        copy.OfferId = string.IsNullOrEmpty(source.OfferId) ? string.Empty : source.OfferId;
        copy.OfferGroupId = string.IsNullOrEmpty(source.OfferGroupId) ? string.Empty : source.OfferGroupId;
        copy.OfferTypeKey = string.IsNullOrEmpty(source.OfferTypeKey) ? string.Empty : source.OfferTypeKey;
        copy.MemberId = string.IsNullOrEmpty(source.MemberId) ? string.Empty : source.MemberId;
        copy.DisplayName = string.IsNullOrEmpty(source.DisplayName) ? string.Empty : source.DisplayName;
        copy.OfferLabel = string.IsNullOrEmpty(source.OfferLabel) ? string.Empty : source.OfferLabel;
        copy.SummaryText = string.IsNullOrEmpty(source.SummaryText) ? string.Empty : source.SummaryText;
        copy.ReasonText = string.IsNullOrEmpty(source.ReasonText) ? string.Empty : source.ReasonText;
        copy.ContinuitySummaryText = string.IsNullOrEmpty(source.ContinuitySummaryText) ? string.Empty : source.ContinuitySummaryText;
        copy.RotationReasonText = string.IsNullOrEmpty(source.RotationReasonText) ? string.Empty : source.RotationReasonText;
        copy.ExposureSummaryText = string.IsNullOrEmpty(source.ExposureSummaryText) ? string.Empty : source.ExposureSummaryText;
        copy.UnlockGroupKey = string.IsNullOrEmpty(source.UnlockGroupKey) ? string.Empty : source.UnlockGroupKey;
        copy.UnlockTierKey = string.IsNullOrEmpty(source.UnlockTierKey) ? string.Empty : source.UnlockTierKey;
        copy.RequiredTierKey = string.IsNullOrEmpty(source.RequiredTierKey) ? string.Empty : source.RequiredTierKey;
        copy.UnlockReasonText = string.IsNullOrEmpty(source.UnlockReasonText) ? string.Empty : source.UnlockReasonText;
        copy.TierEscalationReasonText = string.IsNullOrEmpty(source.TierEscalationReasonText) ? string.Empty : source.TierEscalationReasonText;
        copy.PoolExhaustionReasonText = string.IsNullOrEmpty(source.PoolExhaustionReasonText) ? string.Empty : source.PoolExhaustionReasonText;
        copy.TargetGrowthTrackId = string.IsNullOrEmpty(source.TargetGrowthTrackId) ? string.Empty : source.TargetGrowthTrackId;
        copy.TargetJobId = string.IsNullOrEmpty(source.TargetJobId) ? string.Empty : source.TargetJobId;
        copy.TargetEquipmentLoadoutId = string.IsNullOrEmpty(source.TargetEquipmentLoadoutId) ? string.Empty : source.TargetEquipmentLoadoutId;
        copy.TargetSkillLoadoutId = string.IsNullOrEmpty(source.TargetSkillLoadoutId) ? string.Empty : source.TargetSkillLoadoutId;
        copy.IsFallback = source.IsFallback;
        copy.IsNoOp = source.IsNoOp;
        copy.UsedRotationFallback = source.UsedRotationFallback;
        copy.UsedPoolExhaustionFallback = source.UsedPoolExhaustionFallback;
        copy.UsedUnlockExpansion = source.UsedUnlockExpansion;
        copy.UsedTierEscalation = source.UsedTierEscalation;
        copy.CommitmentWeight = source.CommitmentWeight;
        copy.ContradictionPenalty = source.ContradictionPenalty;
        copy.SidegradeAllowance = source.SidegradeAllowance;
        copy.RecoveryEscapeWeight = source.RecoveryEscapeWeight;
        copy.CoherenceReasonHint = string.IsNullOrEmpty(source.CoherenceReasonHint) ? string.Empty : source.CoherenceReasonHint;
        copy.LaneKey = string.IsNullOrEmpty(source.LaneKey) ? string.Empty : source.LaneKey;
        copy.AdjacentLaneKey = string.IsNullOrEmpty(source.AdjacentLaneKey) ? string.Empty : source.AdjacentLaneKey;
        copy.PrimaryLaneKeyAtSelection = string.IsNullOrEmpty(source.PrimaryLaneKeyAtSelection) ? string.Empty : source.PrimaryLaneKeyAtSelection;
        copy.SecondaryLaneKeyAtSelection = string.IsNullOrEmpty(source.SecondaryLaneKeyAtSelection) ? string.Empty : source.SecondaryLaneKeyAtSelection;
        copy.LaneCoherenceReasonText = string.IsNullOrEmpty(source.LaneCoherenceReasonText) ? string.Empty : source.LaneCoherenceReasonText;
        copy.LaneBiasScore = source.LaneBiasScore;
        copy.CommitmentDepthAtSelection = source.CommitmentDepthAtSelection;
        copy.IsAdjacentLaneOffer = source.IsAdjacentLaneOffer;
        copy.IsRecoveryEscapeOffer = source.IsRecoveryEscapeOffer;
        copy.IsContradictoryToLane = source.IsContradictoryToLane;
        copy.RecoveryBiasScore = source.RecoveryBiasScore;
        copy.RecoveryReasonText = string.IsNullOrEmpty(source.RecoveryReasonText) ? string.Empty : source.RecoveryReasonText;
        copy.RecoveryTriggerKeyAtSelection = string.IsNullOrEmpty(source.RecoveryTriggerKeyAtSelection) ? string.Empty : source.RecoveryTriggerKeyAtSelection;
        copy.RecoveryWindowTierAtSelection = source.RecoveryWindowTierAtSelection;
        copy.LaneLockRelaxed = source.LaneLockRelaxed;
        copy.IsRescueOffer = source.IsRescueOffer;
        copy.MatchesRecoveryRescueGroup = source.MatchesRecoveryRescueGroup;
        copy.SupportsRecoveryWindow = source.SupportsRecoveryWindow;
        copy.CoverageBiasScore = source.CoverageBiasScore;
        copy.OverlapPenaltyScore = source.OverlapPenaltyScore;
        copy.SynergyBiasScore = source.SynergyBiasScore;
        copy.CoverageReasonText = string.IsNullOrEmpty(source.CoverageReasonText) ? string.Empty : source.CoverageReasonText;
        copy.OverlapReasonText = string.IsNullOrEmpty(source.OverlapReasonText) ? string.Empty : source.OverlapReasonText;
        copy.SynergyReasonText = string.IsNullOrEmpty(source.SynergyReasonText) ? string.Empty : source.SynergyReasonText;
        copy.ImprovesMissingCoverage = source.ImprovesMissingCoverage;
        copy.ReducesRoleOverlap = source.ReducesRoleOverlap;
        copy.SupportsCrossMemberSynergy = source.SupportsCrossMemberSynergy;
        copy.ArchetypeBiasScore = source.ArchetypeBiasScore;
        copy.FormationCoherenceBiasScore = source.FormationCoherenceBiasScore;
        copy.PriorityWeightScore = source.PriorityWeightScore;
        copy.RareSlotAllocationScore = source.RareSlotAllocationScore;
        copy.HighImpactSequenceScore = source.HighImpactSequenceScore;
        copy.PriorityBucketKey = string.IsNullOrEmpty(source.PriorityBucketKey) ? string.Empty : source.PriorityBucketKey;
        copy.PriorityBucketLabel = string.IsNullOrEmpty(source.PriorityBucketLabel) ? string.Empty : source.PriorityBucketLabel;
        copy.ArchetypeReasonText = string.IsNullOrEmpty(source.ArchetypeReasonText) ? string.Empty : source.ArchetypeReasonText;
        copy.FormationReasonText = string.IsNullOrEmpty(source.FormationReasonText) ? string.Empty : source.FormationReasonText;
        copy.TopPickReasonText = string.IsNullOrEmpty(source.TopPickReasonText) ? string.Empty : source.TopPickReasonText;
        copy.RareSlotReasonText = string.IsNullOrEmpty(source.RareSlotReasonText) ? string.Empty : source.RareSlotReasonText;
        copy.RecoverySafeguardReasonText = string.IsNullOrEmpty(source.RecoverySafeguardReasonText) ? string.Empty : source.RecoverySafeguardReasonText;
        copy.SupportsArchetypeFollowup = source.SupportsArchetypeFollowup;
        copy.MaintainsHealthyFlex = source.MaintainsHealthyFlex;
        copy.SupportsRescueOverride = source.SupportsRescueOverride;
        copy.IsTopPriorityPick = source.IsTopPriorityPick;
        copy.IsRecommendedSlot = source.IsRecommendedSlot;
        copy.IsRareSlotOffer = source.IsRareSlotOffer;
        copy.IsRecoverySafeguardSlot = source.IsRecoverySafeguardSlot;
        copy.IsArchetypeCommitmentSlot = source.IsArchetypeCommitmentSlot;
        copy.MomentumDampeningScore = source.MomentumDampeningScore;
        copy.ComebackReliefScore = source.ComebackReliefScore;
        copy.MomentumReasonText = string.IsNullOrEmpty(source.MomentumReasonText) ? string.Empty : source.MomentumReasonText;
        copy.IntensityHintText = string.IsNullOrEmpty(source.IntensityHintText) ? string.Empty : source.IntensityHintText;
        copy.IsComebackReliefOffer = source.IsComebackReliefOffer;
        copy.IsMomentumDampened = source.IsMomentumDampened;
        copy.RotationScore = source.RotationScore;
        return copy;
    }
    private PrototypeRpgApplyReadyChoice CopyRpgApplyReadyChoice(PrototypeRpgApplyReadyChoice source)
    {
        PrototypeRpgApplyReadyChoice copy = new PrototypeRpgApplyReadyChoice();
        if (source == null)
        {
            return copy;
        }

        copy.OfferId = string.IsNullOrEmpty(source.OfferId) ? string.Empty : source.OfferId;
        copy.OfferGroupId = string.IsNullOrEmpty(source.OfferGroupId) ? string.Empty : source.OfferGroupId;
        copy.MemberId = string.IsNullOrEmpty(source.MemberId) ? string.Empty : source.MemberId;
        copy.DisplayName = string.IsNullOrEmpty(source.DisplayName) ? string.Empty : source.DisplayName;
        copy.AppliedGrowthTrackId = string.IsNullOrEmpty(source.AppliedGrowthTrackId) ? string.Empty : source.AppliedGrowthTrackId;
        copy.AppliedJobId = string.IsNullOrEmpty(source.AppliedJobId) ? string.Empty : source.AppliedJobId;
        copy.AppliedEquipmentLoadoutId = string.IsNullOrEmpty(source.AppliedEquipmentLoadoutId) ? string.Empty : source.AppliedEquipmentLoadoutId;
        copy.AppliedSkillLoadoutId = string.IsNullOrEmpty(source.AppliedSkillLoadoutId) ? string.Empty : source.AppliedSkillLoadoutId;
        copy.AppliedSummaryText = string.IsNullOrEmpty(source.AppliedSummaryText) ? string.Empty : source.AppliedSummaryText;
        copy.ContinuitySummaryText = string.IsNullOrEmpty(source.ContinuitySummaryText) ? string.Empty : source.ContinuitySummaryText;
        copy.HasChanges = source.HasChanges;
        return copy;
    }

    private PrototypeRpgGrowthChoiceContext ResolveRpgOfferGenerationInput(PrototypeRpgRunResultSnapshot runResultSnapshot, PrototypeRpgCombatContributionSnapshot contributionSnapshot, PrototypeRpgProgressionPreviewSnapshot previewSnapshot)
    {
        return BuildRpgGrowthChoiceContext(runResultSnapshot, contributionSnapshot, previewSnapshot);
    }

    private PrototypeRpgGrowthChoiceContext BuildRpgGrowthChoiceContext(PrototypeRpgRunResultSnapshot runResultSnapshot, PrototypeRpgCombatContributionSnapshot contributionSnapshot, PrototypeRpgProgressionPreviewSnapshot previewSnapshot)
    {
        PrototypeRpgGrowthChoiceContext context = new PrototypeRpgGrowthChoiceContext();
        TestDungeonPartyData party = _activeDungeonParty;
        PrototypeRpgPartyDefinition partyDefinition = party != null ? party.PartyDefinition : null;
        if (partyDefinition == null)
        {
            return context;
        }

        PrototypeRpgAppliedPartyProgressState appliedState = GetOrCreateRpgAppliedPartyProgressState(partyDefinition);
        PrototypeRpgOfferExposureHistoryState exposureState = GetOrCreateRpgOfferExposureHistoryState(partyDefinition);
        PrototypeRpgPartyMemberDefinition[] definitions = partyDefinition.Members ?? System.Array.Empty<PrototypeRpgPartyMemberDefinition>();
        PrototypeRpgGrowthChoiceMemberContext[] members = new PrototypeRpgGrowthChoiceMemberContext[definitions.Length];
        for (int i = 0; i < definitions.Length; i++)
        {
            PrototypeRpgPartyMemberDefinition definition = definitions[i];
            PrototypeRpgAppliedPartyMemberProgressState appliedMemberState = GetRpgAppliedPartyMemberProgressState(appliedState, definition != null ? definition.MemberId : string.Empty);
            PrototypeRpgMemberOfferExposureState exposureMember = GetRpgMemberOfferExposureState(exposureState, definition != null ? definition.MemberId : string.Empty);
            PrototypeRpgMemberContributionSnapshot contributionMember = FindRpgContributionMemberSnapshot(contributionSnapshot, definition != null ? definition.MemberId : string.Empty);
            PrototypeRpgMemberProgressPreview previewMember = FindRpgProgressPreviewMember(previewSnapshot, definition != null ? definition.MemberId : string.Empty);
            PrototypeRpgGrowthChoiceMemberContext memberContext = new PrototypeRpgGrowthChoiceMemberContext();
            if (definition != null)
            {
                memberContext.MemberId = string.IsNullOrEmpty(definition.MemberId) ? string.Empty : definition.MemberId;
                memberContext.DisplayName = string.IsNullOrEmpty(definition.DisplayName) ? "Adventurer" : definition.DisplayName;
                memberContext.RoleTag = string.IsNullOrEmpty(definition.RoleTag) ? string.Empty : definition.RoleTag;
                memberContext.RoleLabel = string.IsNullOrEmpty(definition.RoleLabel) ? string.Empty : definition.RoleLabel;
                memberContext.CurrentGrowthTrackId = !string.IsNullOrEmpty(appliedMemberState != null ? appliedMemberState.AppliedGrowthTrackId : string.Empty) ? appliedMemberState.AppliedGrowthTrackId : definition.GrowthTrackId;
                memberContext.CurrentJobId = !string.IsNullOrEmpty(appliedMemberState != null ? appliedMemberState.AppliedJobId : string.Empty) ? appliedMemberState.AppliedJobId : definition.JobId;
                memberContext.CurrentEquipmentLoadoutId = !string.IsNullOrEmpty(appliedMemberState != null ? appliedMemberState.AppliedEquipmentLoadoutId : string.Empty) ? appliedMemberState.AppliedEquipmentLoadoutId : definition.EquipmentLoadoutId;
                memberContext.CurrentSkillLoadoutId = !string.IsNullOrEmpty(appliedMemberState != null ? appliedMemberState.AppliedSkillLoadoutId : string.Empty) ? appliedMemberState.AppliedSkillLoadoutId : definition.SkillLoadoutId;
            }

            if (contributionMember != null)
            {
                memberContext.DamageDealt = contributionMember.DamageDealt;
                memberContext.DamageTaken = contributionMember.DamageTaken;
                memberContext.HealingDone = contributionMember.HealingDone;
                memberContext.ActionCount = contributionMember.ActionCount;
                memberContext.KillCount = contributionMember.KillCount;
                memberContext.Survived = contributionMember.Survived;
                memberContext.KnockedOut = contributionMember.KnockedOut;
                memberContext.ContributionSummaryText = string.IsNullOrEmpty(contributionMember.ContributionSummaryText) ? string.Empty : contributionMember.ContributionSummaryText;
                memberContext.NotableOutcomeKey = string.IsNullOrEmpty(previewMember != null ? previewMember.NotableOutcomeKey : string.Empty)
                    ? BuildMemberNotableOutcomeKey(contributionMember, runResultSnapshot)
                    : previewMember.NotableOutcomeKey;
            }
            else if (previewMember != null)
            {
                memberContext.Survived = previewMember.Survived;
                memberContext.ContributionSummaryText = string.IsNullOrEmpty(previewMember.ContributionSummaryText) ? string.Empty : previewMember.ContributionSummaryText;
                memberContext.NotableOutcomeKey = string.IsNullOrEmpty(previewMember.NotableOutcomeKey) ? string.Empty : previewMember.NotableOutcomeKey;
            }

            string lastOfferId = exposureMember != null && !string.IsNullOrEmpty(exposureMember.RecentSelectedOfferId)
                ? exposureMember.RecentSelectedOfferId
                : (appliedMemberState != null && !string.IsNullOrEmpty(appliedMemberState.LastAppliedOfferId) ? appliedMemberState.LastAppliedOfferId : string.Empty);
            string lastGroupId = exposureMember != null ? GetMostRecentRpgHistoryValue(exposureMember.RecentlyOfferedGroupKeys) : string.Empty;
            if (string.IsNullOrEmpty(lastGroupId) && appliedMemberState != null)
            {
                lastGroupId = string.IsNullOrEmpty(appliedMemberState.LastAppliedOfferGroupId) ? string.Empty : appliedMemberState.LastAppliedOfferGroupId;
            }

            string lastTypeKey = exposureMember != null ? GetMostRecentRpgHistoryValue(exposureMember.RecentlyOfferedTypeKeys) : string.Empty;
            if (string.IsNullOrEmpty(lastTypeKey) && appliedMemberState != null)
            {
                lastTypeKey = string.IsNullOrEmpty(appliedMemberState.LastAppliedOfferTypeKey) ? string.Empty : appliedMemberState.LastAppliedOfferTypeKey;
            }

            memberContext.LastAppliedOfferTypeKey = string.IsNullOrEmpty(appliedMemberState != null ? appliedMemberState.LastAppliedOfferTypeKey : string.Empty) ? string.Empty : appliedMemberState.LastAppliedOfferTypeKey;
            memberContext.LastOfferedId = string.IsNullOrEmpty(lastOfferId) ? string.Empty : lastOfferId;
            memberContext.LastOfferedGroupId = string.IsNullOrEmpty(lastGroupId) ? string.Empty : lastGroupId;
            memberContext.LastOfferedTypeKey = string.IsNullOrEmpty(lastTypeKey) ? string.Empty : lastTypeKey;
            memberContext.RecentOfferedIds = exposureMember != null && exposureMember.RecentlyOfferedIds != null && exposureMember.RecentlyOfferedIds.Length > 0 ? CopyRpgStringArray(exposureMember.RecentlyOfferedIds) : (string.IsNullOrEmpty(memberContext.LastOfferedId) ? System.Array.Empty<string>() : new[] { memberContext.LastOfferedId });
            memberContext.RecentOfferedGroupIds = exposureMember != null && exposureMember.RecentlyOfferedGroupKeys != null && exposureMember.RecentlyOfferedGroupKeys.Length > 0 ? CopyRpgStringArray(exposureMember.RecentlyOfferedGroupKeys) : (string.IsNullOrEmpty(memberContext.LastOfferedGroupId) ? System.Array.Empty<string>() : new[] { memberContext.LastOfferedGroupId });
            memberContext.RecentOfferedTypeKeys = exposureMember != null && exposureMember.RecentlyOfferedTypeKeys != null && exposureMember.RecentlyOfferedTypeKeys.Length > 0 ? CopyRpgStringArray(exposureMember.RecentlyOfferedTypeKeys) : (string.IsNullOrEmpty(memberContext.LastOfferedTypeKey) ? System.Array.Empty<string>() : new[] { memberContext.LastOfferedTypeKey });
            memberContext.RecentOfferCount = exposureMember != null && exposureMember.RecentOfferCount > 0 ? exposureMember.RecentOfferCount : (string.IsNullOrEmpty(memberContext.LastOfferedId) ? 0 : 1);
            memberContext.RecentOfferExposureSummaryText = exposureMember != null && !string.IsNullOrEmpty(exposureMember.ExposureSummaryText) ? exposureMember.ExposureSummaryText : BuildRpgOfferExposureMemberSummaryText(memberContext.DisplayName, memberContext.RecentOfferCount, memberContext.LastOfferedTypeKey, memberContext.LastOfferedGroupId);
            members[i] = memberContext;
        }

        context.PartyId = string.IsNullOrEmpty(partyDefinition.PartyId) ? string.Empty : partyDefinition.PartyId;
        context.RunIdentity = BuildRpgOfferGenerationRunIdentity(runResultSnapshot);
        context.ResultStateKey = runResultSnapshot != null && !string.IsNullOrEmpty(runResultSnapshot.ResultStateKey) ? runResultSnapshot.ResultStateKey : string.Empty;
        context.EliteDefeated = runResultSnapshot != null && runResultSnapshot.EliteOutcome != null && runResultSnapshot.EliteOutcome.IsEliteDefeated;
        context.RiskyRoute = runResultSnapshot != null && !string.IsNullOrEmpty(runResultSnapshot.RouteId) && runResultSnapshot.RouteId.IndexOf("risky", System.StringComparison.OrdinalIgnoreCase) >= 0;
        context.HasRewardCarryover = runResultSnapshot != null && (!string.IsNullOrEmpty(runResultSnapshot.RewardCarryoverSummaryText) || (runResultSnapshot.LootOutcome != null && !string.IsNullOrEmpty(runResultSnapshot.LootOutcome.RewardCarryoverSummaryText)));
        context.AppliedPartySummaryText = BuildRpgAppliedPartySummaryText(appliedState, partyDefinition);
        context.OfferBasisSummaryText = BuildRpgOfferBasisSummaryText(runResultSnapshot, contributionSnapshot, appliedState, partyDefinition);
        context.OfferRotationSummaryText = "Rotation input: prefer lower recent exposure before repeating the same lane.";
        context.RecentOfferExposureSummaryText = exposureState != null && !string.IsNullOrEmpty(exposureState.SummaryText) ? exposureState.SummaryText : "Recent exposure: none.";
        context.OfferUnlockSummaryText = "Unlocks: base pool only.";
        context.TierEscalationSummaryText = "Tier escalation: base tier only.";
        context.PoolExpansionSummaryText = "Pool expansion: base pool only.";
        context.PoolExhaustionSummaryText = "Pool exhaustion: no fallback used.";
        context.Members = members;
        EvaluateRpgOfferUnlockState(context, runResultSnapshot, partyDefinition);
        EvaluateRpgBuildLaneState(context, runResultSnapshot, partyDefinition);
        EvaluateRpgLaneRecoveryState(context, runResultSnapshot, partyDefinition);
        EvaluateRpgPartyCoverageState(context, runResultSnapshot, partyDefinition);
        EvaluateRpgPartyArchetypeState(context, runResultSnapshot, partyDefinition);
        EvaluateRpgOfferMomentumState(context, runResultSnapshot, partyDefinition);
        return context;
    }


    private PrototypeRpgMemberContributionSnapshot FindRpgContributionMemberSnapshot(PrototypeRpgCombatContributionSnapshot contributionSnapshot, string memberId)
    {
        if (contributionSnapshot == null || contributionSnapshot.Members == null || string.IsNullOrEmpty(memberId))
        {
            return null;
        }

        for (int i = 0; i < contributionSnapshot.Members.Length; i++)
        {
            PrototypeRpgMemberContributionSnapshot member = contributionSnapshot.Members[i];
            if (member != null && member.MemberId == memberId)
            {
                return member;
            }
        }

        return null;
    }

    private PrototypeRpgMemberProgressPreview FindRpgProgressPreviewMember(PrototypeRpgProgressionPreviewSnapshot previewSnapshot, string memberId)
    {
        if (previewSnapshot == null || previewSnapshot.Members == null || string.IsNullOrEmpty(memberId))
        {
            return null;
        }

        for (int i = 0; i < previewSnapshot.Members.Length; i++)
        {
            PrototypeRpgMemberProgressPreview member = previewSnapshot.Members[i];
            if (member != null && member.MemberId == memberId)
            {
                return member;
            }
        }

        return null;
    }

    private string BuildRpgOfferGenerationRunIdentity(PrototypeRpgRunResultSnapshot runResultSnapshot)
    {
        if (runResultSnapshot == null)
        {
            return "none";
        }

        string dungeonId = string.IsNullOrEmpty(runResultSnapshot.DungeonId) ? "dungeon" : runResultSnapshot.DungeonId;
        string routeId = string.IsNullOrEmpty(runResultSnapshot.RouteId) ? "route" : runResultSnapshot.RouteId;
        string resultKey = string.IsNullOrEmpty(runResultSnapshot.ResultStateKey) ? "none" : runResultSnapshot.ResultStateKey;
        return dungeonId + ":" + routeId + ":" + resultKey + ":turns-" + runResultSnapshot.TotalTurnsTaken;
    }

    private string BuildRpgOfferBasisSummaryText(PrototypeRpgRunResultSnapshot runResultSnapshot, PrototypeRpgCombatContributionSnapshot contributionSnapshot, PrototypeRpgAppliedPartyProgressState appliedState, PrototypeRpgPartyDefinition partyDefinition)
    {
        string resultKey = runResultSnapshot != null && !string.IsNullOrEmpty(runResultSnapshot.ResultStateKey) ? runResultSnapshot.ResultStateKey : PrototypeBattleOutcomeKeys.None;
        string baseText = resultKey == PrototypeBattleOutcomeKeys.RunRetreat
            ? "Next basis: retreat recovery with committed lineup."
            : resultKey == PrototypeBattleOutcomeKeys.RunDefeat
                ? "Next basis: defeat recovery with committed lineup."
                : "Next basis: committed lineup + latest contribution refresh.";
        string partyText = BuildRpgAppliedPartySummaryText(appliedState, partyDefinition);
        return string.IsNullOrEmpty(partyText) ? baseText : baseText + " | " + partyText;
    }

    private PrototypeRpgUpgradeOfferCandidate[] BuildRpgUpgradeOfferCandidates(PrototypeRpgGrowthChoiceContext context)
    {
        PrototypeRpgGrowthChoiceMemberContext[] members = context != null ? (context.Members ?? System.Array.Empty<PrototypeRpgGrowthChoiceMemberContext>()) : System.Array.Empty<PrototypeRpgGrowthChoiceMemberContext>();
        if (members.Length <= 0)
        {
            return System.Array.Empty<PrototypeRpgUpgradeOfferCandidate>();
        }

        PrototypeRpgGrowthChoiceMemberContext[] orderedMembers = OrderRpgOfferMembersForSelection(members);
        Dictionary<string, PrototypeRpgUpgradeOfferCandidate> offersByMemberId = new Dictionary<string, PrototypeRpgUpgradeOfferCandidate>();
        Dictionary<string, int> assignedTypeCounts = new Dictionary<string, int>();
        Dictionary<string, int> assignedGroupCounts = new Dictionary<string, int>();
        for (int i = 0; i < orderedMembers.Length; i++)
        {
            PrototypeRpgGrowthChoiceMemberContext member = orderedMembers[i] ?? new PrototypeRpgGrowthChoiceMemberContext();
            bool defensiveBias = member.KnockedOut || member.DamageTaken > member.DamageDealt || context.ResultStateKey == PrototypeBattleOutcomeKeys.RunDefeat || context.ResultStateKey == PrototypeBattleOutcomeKeys.RunRetreat;
            bool supportBias = member.HealingDone > 0;
            bool offensiveBias = member.KillCount > 0 || member.DamageDealt >= member.DamageTaken;
            PrototypeRpgGrowthChoiceRuleDefinition primaryRule = PrototypeRpgGrowthChoiceRuleCatalog.ResolvePrimaryRule(member.RoleTag, defensiveBias, offensiveBias, supportBias);
            PrototypeRpgGrowthChoiceRuleDefinition alternateRule = PrototypeRpgGrowthChoiceRuleCatalog.ResolveAlternateRule(primaryRule);
            PrototypeRpgGrowthChoiceRuleDefinition fallbackRule = PrototypeRpgGrowthChoiceRuleCatalog.ResolveFallbackRule(member.RoleTag);
            PrototypeRpgUpgradeOfferCandidate candidate = SelectRpgOfferCandidate(context, member, primaryRule, alternateRule, fallbackRule, assignedTypeCounts, assignedGroupCounts);
            if (candidate == null)
            {
                candidate = new PrototypeRpgUpgradeOfferCandidate();
                candidate.MemberId = string.IsNullOrEmpty(member.MemberId) ? string.Empty : member.MemberId;
                candidate.DisplayName = string.IsNullOrEmpty(member.DisplayName) ? "Adventurer" : member.DisplayName;
            }

            offersByMemberId[candidate.MemberId] = candidate;
            IncrementRpgRotationCount(assignedTypeCounts, candidate.OfferTypeKey);
            IncrementRpgRotationCount(assignedGroupCounts, candidate.OfferGroupId);
        }

        PrototypeRpgUpgradeOfferCandidate[] offers = new PrototypeRpgUpgradeOfferCandidate[members.Length];
        for (int i = 0; i < members.Length; i++)
        {
            PrototypeRpgGrowthChoiceMemberContext member = members[i] ?? new PrototypeRpgGrowthChoiceMemberContext();
            PrototypeRpgUpgradeOfferCandidate offer = null;
            if (!string.IsNullOrEmpty(member.MemberId) && offersByMemberId.TryGetValue(member.MemberId, out offer))
            {
                offers[i] = offer;
            }
            else
            {
                offers[i] = new PrototypeRpgUpgradeOfferCandidate();
            }
        }

        return offers;
    }

    private PrototypeRpgGrowthChoiceMemberContext[] OrderRpgOfferMembersForSelection(PrototypeRpgGrowthChoiceMemberContext[] members)
    {
        PrototypeRpgGrowthChoiceMemberContext[] ordered = new PrototypeRpgGrowthChoiceMemberContext[members.Length];
        for (int i = 0; i < members.Length; i++)
        {
            ordered[i] = members[i] ?? new PrototypeRpgGrowthChoiceMemberContext();
        }

        System.Array.Sort(ordered, CompareRpgOfferMembersForSelection);
        return ordered;
    }

    private int CompareRpgOfferMembersForSelection(PrototypeRpgGrowthChoiceMemberContext left, PrototypeRpgGrowthChoiceMemberContext right)
    {
        int exposureCompare = (left != null ? left.RecentOfferCount : 0).CompareTo(right != null ? right.RecentOfferCount : 0);
        if (exposureCompare != 0)
        {
            return exposureCompare;
        }

        int historyCompare = (string.IsNullOrEmpty(left != null ? left.LastOfferedTypeKey : string.Empty) ? 0 : 1).CompareTo(string.IsNullOrEmpty(right != null ? right.LastOfferedTypeKey : string.Empty) ? 0 : 1);
        if (historyCompare != 0)
        {
            return historyCompare;
        }

        return string.Compare(left != null ? left.DisplayName : string.Empty, right != null ? right.DisplayName : string.Empty, System.StringComparison.Ordinal);
    }

    private PrototypeRpgUpgradeOfferCandidate SelectRpgOfferCandidate(
        PrototypeRpgGrowthChoiceContext context,
        PrototypeRpgGrowthChoiceMemberContext member,
        PrototypeRpgGrowthChoiceRuleDefinition primaryRule,
        PrototypeRpgGrowthChoiceRuleDefinition alternateRule,
        PrototypeRpgGrowthChoiceRuleDefinition fallbackRule,
        Dictionary<string, int> assignedTypeCounts,
        Dictionary<string, int> assignedGroupCounts)
    {
        PrototypeRpgUpgradeOfferCandidate primaryCandidate = BuildRpgUpgradeOfferCandidate(member, primaryRule);
        PrototypeRpgUpgradeOfferCandidate alternateCandidate = BuildRpgUpgradeOfferCandidate(member, alternateRule);
        PrototypeRpgUpgradeOfferCandidate fallbackCandidate = BuildRpgUpgradeOfferCandidate(member, fallbackRule);
        PrototypeRpgGrowthChoiceRuleDefinition[] unlockedRules = PrototypeRpgGrowthChoiceRuleCatalog.ResolveUnlockedRules(
            member != null ? member.RoleTag : string.Empty,
            member != null ? member.UnlockedGroupKeys : System.Array.Empty<string>(),
            member != null ? member.UnlockedTierKeys : System.Array.Empty<string>());

        List<PrototypeRpgUpgradeOfferCandidate> regularCandidates = new List<PrototypeRpgUpgradeOfferCandidate>();
        if (primaryCandidate != null)
        {
            regularCandidates.Add(primaryCandidate);
        }

        if (alternateCandidate != null)
        {
            regularCandidates.Add(alternateCandidate);
        }

        for (int i = 0; i < unlockedRules.Length; i++)
        {
            PrototypeRpgUpgradeOfferCandidate unlockedCandidate = BuildRpgUpgradeOfferCandidate(member, unlockedRules[i]);
            if (unlockedCandidate != null)
            {
                regularCandidates.Add(unlockedCandidate);
            }
        }

        PromoteRpgRecoveryAdjacentUtilityOffers(regularCandidates, member);

        PrototypeRpgUpgradeOfferCandidate bestRegular = null;
        int bestRegularScore = int.MinValue;
        for (int i = 0; i < regularCandidates.Count; i++)
        {
            PrototypeRpgUpgradeOfferCandidate candidate = regularCandidates[i];
            if (candidate == null)
            {
                continue;
            }

            int laneBiasScore = ScoreOfferAgainstBuildLane(context, candidate, member);
            int recoveryBiasScore = ScoreOfferAgainstRecoveryWindow(context, candidate, member);
            int coverageBiasScore = ScoreOfferAgainstPartyCoverage(context, candidate, member);
            int archetypeBiasScore = ScoreOfferAgainstPartyArchetype(context, candidate, member);
            int priorityBiasScore = ScoreOfferAgainstPriorityBucket(context, candidate, member, assignedTypeCounts, assignedGroupCounts);
            int momentumBiasScore = ScoreOfferAgainstSessionMomentum(context, candidate, member);
            bool laneLockRelaxed = RelaxLaneLockForRecoveryWindow(context, candidate, member);
            candidate.LaneLockRelaxed = laneLockRelaxed;
            if (ShouldSuppressRpgContradictoryOfferCandidate(context, candidate, member) &&
                !laneLockRelaxed &&
                !candidate.ImprovesMissingCoverage &&
                !candidate.ReducesRoleOverlap &&
                !candidate.SupportsCrossMemberSynergy &&
                !candidate.SupportsArchetypeFollowup &&
                !candidate.MaintainsHealthyFlex &&
                !candidate.SupportsRescueOverride)
            {
                candidate.IsContradictoryToLane = true;
                candidate.RotationScore = int.MinValue / 4;
                continue;
            }

            candidate.RotationScore = ScoreRpgUpgradeOfferCandidate(context, candidate, member, assignedTypeCounts, assignedGroupCounts) +
                                      laneBiasScore +
                                      recoveryBiasScore +
                                      coverageBiasScore +
                                      archetypeBiasScore +
                                      priorityBiasScore +
                                      momentumBiasScore;
            if (string.IsNullOrEmpty(candidate.RecoveryReasonText) && member != null && member.SoftRespecWindowOpen)
            {
                candidate.RecoveryReasonText = BuildRpgRecoveryOfferReasonText(context, candidate, member);
            }

            if (bestRegular == null || candidate.RotationScore > bestRegularScore)
            {
                bestRegular = candidate;
                bestRegularScore = candidate.RotationScore;
            }
        }

        if (bestRegular != null && !IsRpgUpgradeOfferCandidateRedundant(bestRegular, member) && (bestRegularScore > -35 || !DoesRpgCandidateTouchRecentExposure(bestRegular, member)))
        {
            bestRegular.ExposureSummaryText = string.IsNullOrEmpty(member.RecentOfferExposureSummaryText) ? BuildRpgOfferExposureMemberSummaryText(member.DisplayName, member.RecentOfferCount, member.LastOfferedTypeKey, member.LastOfferedGroupId) : member.RecentOfferExposureSummaryText;
            bestRegular.UsedRotationFallback = primaryCandidate != null && primaryCandidate.OfferId != bestRegular.OfferId;
            bestRegular.RotationReasonText = BuildRpgRotationReasonText(member, bestRegular, primaryCandidate, assignedTypeCounts, assignedGroupCounts);
            if (bestRegular.UsedUnlockExpansion && !string.IsNullOrEmpty(member.RecentUnlockReasonText))
            {
                bestRegular.UnlockReasonText = member.RecentUnlockReasonText;
            }
            else if (bestRegular.UsedUnlockExpansion && string.IsNullOrEmpty(bestRegular.UnlockReasonText))
            {
                bestRegular.UnlockReasonText = "Unlock: session variety opened a fresher branch.";
            }

            if (bestRegular.UsedTierEscalation)
            {
                bestRegular.TierEscalationReasonText = BuildRpgTierEscalationReasonText(member, bestRegular, context);
            }

            bestRegular.PoolExhaustionReasonText = string.Empty;
            if (string.IsNullOrEmpty(bestRegular.LaneCoherenceReasonText))
            {
                bestRegular.LaneCoherenceReasonText = BuildRpgLaneCoherenceReasonText(context, bestRegular, member);
            }
            if (string.IsNullOrEmpty(bestRegular.RecoveryReasonText) && member != null && member.SoftRespecWindowOpen)
            {
                bestRegular.RecoveryReasonText = BuildRpgRecoveryOfferReasonText(context, bestRegular, member);
            }
            bestRegular.ReasonText = MergeRpgSummaryText(bestRegular.ReasonText, bestRegular.TopPickReasonText);
            bestRegular.ReasonText = MergeRpgSummaryText(bestRegular.ReasonText, bestRegular.MomentumReasonText);
            bestRegular.ContinuitySummaryText = MergeRpgSummaryText(bestRegular.ContinuitySummaryText, bestRegular.RareSlotReasonText);
            bestRegular.ContinuitySummaryText = MergeRpgSummaryText(bestRegular.ContinuitySummaryText, bestRegular.RecoverySafeguardReasonText);
            bestRegular.ContinuitySummaryText = MergeRpgSummaryText(bestRegular.ContinuitySummaryText, bestRegular.IntensityHintText);
            return bestRegular;
        }

        if (fallbackCandidate == null)
        {
            fallbackCandidate = new PrototypeRpgUpgradeOfferCandidate();
            fallbackCandidate.MemberId = string.IsNullOrEmpty(member != null ? member.MemberId : string.Empty) ? string.Empty : member.MemberId;
            fallbackCandidate.DisplayName = string.IsNullOrEmpty(member != null ? member.DisplayName : string.Empty) ? "Adventurer" : member.DisplayName;
            fallbackCandidate.IsFallback = true;
            fallbackCandidate.IsNoOp = true;
        }

        fallbackCandidate.UsedRotationFallback = true;
        fallbackCandidate.UsedPoolExhaustionFallback = true;
        fallbackCandidate.RotationScore = bestRegularScore;
        fallbackCandidate.ExposureSummaryText = string.IsNullOrEmpty(member != null ? member.RecentOfferExposureSummaryText : string.Empty)
            ? BuildRpgOfferExposureMemberSummaryText(member != null ? member.DisplayName : "Adventurer", member != null ? member.RecentOfferCount : 0, member != null ? member.LastOfferedTypeKey : string.Empty, member != null ? member.LastOfferedGroupId : string.Empty)
            : member.RecentOfferExposureSummaryText;
        fallbackCandidate.RotationReasonText = "Rotation held the current line because fresher offers were exhausted.";
        fallbackCandidate.UnlockReasonText = string.IsNullOrEmpty(member != null ? member.RecentUnlockReasonText : string.Empty) ? string.Empty : member.RecentUnlockReasonText;
        fallbackCandidate.TierEscalationReasonText = !string.IsNullOrEmpty(member != null ? member.LastEscalatedTierKey : string.Empty)
            ? "Tier hold: " + PrototypeRpgGrowthChoiceRuleCatalog.ResolveTierLabel(member.LastEscalatedTierKey) + " options stayed exhausted or redundant."
            : string.Empty;
        fallbackCandidate.PoolExhaustionReasonText = BuildRpgPoolExhaustionReasonText(member, primaryCandidate, alternateCandidate);
        fallbackCandidate.ReasonText = string.IsNullOrEmpty(fallbackCandidate.ReasonText)
            ? fallbackCandidate.PoolExhaustionReasonText
            : fallbackCandidate.ReasonText + " | " + fallbackCandidate.PoolExhaustionReasonText;
        fallbackCandidate.ContinuitySummaryText = "Continuity hold after unlock-driven expansion and reoffer guard thinned the pool.";
        fallbackCandidate.LaneCoherenceReasonText = string.IsNullOrEmpty(member != null ? member.LaneSummaryText : string.Empty) ? string.Empty : member.LaneSummaryText;
        fallbackCandidate.PrimaryLaneKeyAtSelection = string.IsNullOrEmpty(member != null ? member.ResolvedPrimaryLaneKey : string.Empty) ? string.Empty : member.ResolvedPrimaryLaneKey;
        fallbackCandidate.SecondaryLaneKeyAtSelection = string.IsNullOrEmpty(member != null ? member.ResolvedSecondaryLaneKey : string.Empty) ? string.Empty : member.ResolvedSecondaryLaneKey;
        fallbackCandidate.CommitmentDepthAtSelection = member != null ? member.CommitmentDepth : 0;
        fallbackCandidate.RecoveryBiasScore = member != null ? member.RecoveryBiasWeight : 0;
        fallbackCandidate.RecoveryTriggerKeyAtSelection = string.IsNullOrEmpty(member != null ? member.LastRecoveryTriggerKey : string.Empty) ? string.Empty : member.LastRecoveryTriggerKey;
        fallbackCandidate.RecoveryWindowTierAtSelection = member != null ? member.SoftRespecWindowTier : 0;
        fallbackCandidate.LaneLockRelaxed = member != null && member.SoftRespecWindowOpen;
        fallbackCandidate.IsRescueOffer = false;
        fallbackCandidate.MatchesRecoveryRescueGroup = false;
        fallbackCandidate.SupportsRecoveryWindow = member != null && member.SoftRespecWindowOpen;
        fallbackCandidate.RecoveryReasonText = BuildRpgRecoveryOfferReasonText(context, fallbackCandidate, member);
        fallbackCandidate.CoverageReasonText = string.IsNullOrEmpty(member != null ? member.CoverageSummaryText : string.Empty) ? string.Empty : member.CoverageSummaryText;
        fallbackCandidate.OverlapReasonText = string.IsNullOrEmpty(CurrentRoleOverlapWarningSummaryText) ? string.Empty : CurrentRoleOverlapWarningSummaryText;
        fallbackCandidate.SynergyReasonText = string.IsNullOrEmpty(CurrentCrossMemberSynergySummaryText) ? string.Empty : CurrentCrossMemberSynergySummaryText;
        ScoreOfferAgainstPriorityBucket(context, fallbackCandidate, member, assignedTypeCounts, assignedGroupCounts);
        ScoreOfferAgainstSessionMomentum(context, fallbackCandidate, member);
        fallbackCandidate.ReasonText = MergeRpgSummaryText(fallbackCandidate.ReasonText, fallbackCandidate.TopPickReasonText);
        fallbackCandidate.ReasonText = MergeRpgSummaryText(fallbackCandidate.ReasonText, fallbackCandidate.MomentumReasonText);
        fallbackCandidate.ContinuitySummaryText = MergeRpgSummaryText(fallbackCandidate.ContinuitySummaryText, fallbackCandidate.RareSlotReasonText);
        fallbackCandidate.ContinuitySummaryText = MergeRpgSummaryText(fallbackCandidate.ContinuitySummaryText, fallbackCandidate.RecoverySafeguardReasonText);
        fallbackCandidate.ContinuitySummaryText = MergeRpgSummaryText(fallbackCandidate.ContinuitySummaryText, fallbackCandidate.IntensityHintText);
        return fallbackCandidate;
    }


    private int ScoreRpgUpgradeOfferCandidate(PrototypeRpgGrowthChoiceContext context, PrototypeRpgUpgradeOfferCandidate candidate, PrototypeRpgGrowthChoiceMemberContext member, Dictionary<string, int> assignedTypeCounts, Dictionary<string, int> assignedGroupCounts)
    {
        if (candidate == null)
        {
            return int.MinValue;
        }

        int score = 0;
        bool redundant = IsRpgUpgradeOfferCandidateRedundant(candidate, member);
        bool touchesRecent = DoesRpgCandidateTouchRecentExposure(candidate, member);
        score += candidate.IsFallback ? -60 : 12;
        score += candidate.IsNoOp ? -12 : 6;
        score += redundant ? -80 : 24;
        score += touchesRecent ? -32 : 14;
        if (!string.IsNullOrEmpty(candidate.OfferTypeKey) && candidate.OfferTypeKey == (member != null ? member.LastOfferedTypeKey : string.Empty))
        {
            score -= 12;
        }

        score -= GetRpgRotationCount(assignedTypeCounts, candidate.OfferTypeKey) * 10;
        score -= GetRpgRotationCount(assignedGroupCounts, candidate.OfferGroupId) * 8;
        score -= member != null ? member.RecentOfferCount * 2 : 0;
        if (candidate.UsedUnlockExpansion)
        {
            score += 8 + candidate.UnlockPriority * 4;
            score += member != null && member.RecentOfferCount >= 2 ? 10 : 0;
            score += context != null && context.RiskyRoute ? 4 : 0;
        }

        if (candidate.UsedTierEscalation)
        {
            score += context != null && context.EliteDefeated ? 12 : 6;
            score += context != null && context.HasRewardCarryover ? 4 : 0;
        }

        return score;
    }

    private bool DoesRpgCandidateTouchRecentExposure(PrototypeRpgUpgradeOfferCandidate candidate, PrototypeRpgGrowthChoiceMemberContext member)
    {
        if (candidate == null || member == null)
        {
            return false;
        }

        return ArrayContainsRpgValue(member.RecentOfferedIds, candidate.OfferId) ||
               ArrayContainsRpgValue(member.RecentOfferedGroupIds, candidate.OfferGroupId) ||
               ArrayContainsRpgValue(member.RecentOfferedTypeKeys, candidate.OfferTypeKey) ||
               (!string.IsNullOrEmpty(member.LastOfferedTypeKey) && candidate.OfferTypeKey == member.LastOfferedTypeKey) ||
               (!string.IsNullOrEmpty(member.LastOfferedGroupId) && candidate.OfferGroupId == member.LastOfferedGroupId);
    }

    private bool ArrayContainsRpgValue(string[] values, string value)
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

    private int GetRpgRotationCount(Dictionary<string, int> counts, string key)
    {
        if (counts == null || string.IsNullOrEmpty(key))
        {
            return 0;
        }

        int count;
        return counts.TryGetValue(key, out count) ? count : 0;
    }

    private void IncrementRpgRotationCount(Dictionary<string, int> counts, string key)
    {
        if (counts == null || string.IsNullOrEmpty(key))
        {
            return;
        }

        int count;
        counts.TryGetValue(key, out count);
        counts[key] = count + 1;
    }

    private string BuildRpgRotationReasonText(PrototypeRpgGrowthChoiceMemberContext member, PrototypeRpgUpgradeOfferCandidate chosenCandidate, PrototypeRpgUpgradeOfferCandidate primaryCandidate, Dictionary<string, int> assignedTypeCounts, Dictionary<string, int> assignedGroupCounts)
    {
        if (chosenCandidate == null)
        {
            return string.Empty;
        }

        if (chosenCandidate.IsFallback)
        {
            return "Rotation held the current line after fresher offers ran out.";
        }

        if (chosenCandidate.UsedTierEscalation)
        {
            return "Escalated into " + PrototypeRpgGrowthChoiceRuleCatalog.ResolveTierLabel(chosenCandidate.UnlockTierKey) + " after session pressure widened the pool.";
        }

        if (chosenCandidate.UsedUnlockExpansion && primaryCandidate != null && primaryCandidate.OfferId != chosenCandidate.OfferId)
        {
            return "Unlock expansion opened a fresher " + chosenCandidate.OfferLabel + " lane.";
        }

        bool primaryRepeated = DoesRpgCandidateTouchRecentExposure(primaryCandidate, member);
        bool chosenRepeated = DoesRpgCandidateTouchRecentExposure(chosenCandidate, member);
        int chosenTypePressure = GetRpgRotationCount(assignedTypeCounts, chosenCandidate.OfferTypeKey);
        int chosenGroupPressure = GetRpgRotationCount(assignedGroupCounts, chosenCandidate.OfferGroupId);
        int primaryTypePressure = GetRpgRotationCount(assignedTypeCounts, primaryCandidate != null ? primaryCandidate.OfferTypeKey : string.Empty);
        if (primaryCandidate != null && primaryCandidate.OfferId != chosenCandidate.OfferId)
        {
            if (primaryRepeated && !chosenRepeated)
            {
                return "Rotated off recent exposure into a fresher " + chosenCandidate.OfferLabel + " lane.";
            }

            if (chosenTypePressure < primaryTypePressure)
            {
                return "Shifted offer type to spread upgrades across the party.";
            }

            if (chosenGroupPressure <= 0)
            {
                return "Alternate lane chosen to avoid repeating the same offer group too quickly.";
            }

            return "Alternate lane chosen to keep the session offer loop varied.";
        }

        if (!chosenRepeated)
        {
            return "Primary lane remained fresh against recent exposure.";
        }

        return "Primary lane stayed the clearest fit despite recent exposure.";
    }

    private string BuildRpgPoolExhaustionReasonText(PrototypeRpgGrowthChoiceMemberContext member, PrototypeRpgUpgradeOfferCandidate primaryCandidate, PrototypeRpgUpgradeOfferCandidate alternateCandidate)
    {
        string exposureText = string.IsNullOrEmpty(member != null ? member.RecentOfferExposureSummaryText : string.Empty)
            ? "Recent exposure already covered the stronger paths."
            : member.RecentOfferExposureSummaryText;
        string unlockText = string.IsNullOrEmpty(member != null ? member.UnlockSummaryText : string.Empty) ? string.Empty : member.UnlockSummaryText;
        string primaryLabel = primaryCandidate != null && !string.IsNullOrEmpty(primaryCandidate.OfferLabel) ? primaryCandidate.OfferLabel : "primary lane";
        string alternateLabel = alternateCandidate != null && !string.IsNullOrEmpty(alternateCandidate.OfferLabel) ? alternateCandidate.OfferLabel : "alternate lane";
        string baseText = "Pool exhaustion: " + primaryLabel + " and " + alternateLabel + " were filtered by continuity or redundancy. " + exposureText;
        return string.IsNullOrEmpty(unlockText) ? baseText : baseText + " " + unlockText;
    }

    private string BuildRpgOfferRotationSummaryText(PrototypeRpgGrowthChoiceContext context, PrototypeRpgUpgradeOfferCandidate[] offers)
    {
        int rotatedCount = 0;
        int stableCount = 0;
        for (int i = 0; i < (offers != null ? offers.Length : 0); i++)
        {
            PrototypeRpgUpgradeOfferCandidate offer = offers[i];
            if (offer == null)
            {
                continue;
            }

            if (offer.UsedRotationFallback)
            {
                rotatedCount += 1;
            }
            else if (!offer.IsFallback)
            {
                stableCount += 1;
            }
        }

        return "Rotation: " + rotatedCount + " shifted for variety, " + stableCount + " held dominant fit.";
    }

    private string BuildRpgRecentOfferExposureSurfaceSummary(PrototypeRpgGrowthChoiceContext context, PrototypeRpgUpgradeOfferCandidate[] offers)
    {
        if (context != null && !string.IsNullOrEmpty(context.RecentOfferExposureSummaryText))
        {
            return context.RecentOfferExposureSummaryText;
        }

        return "Recent exposure: none.";
    }

    private string BuildRpgPoolExhaustionSummaryText(PrototypeRpgUpgradeOfferCandidate[] offers)
    {
        int fallbackCount = 0;
        for (int i = 0; i < (offers != null ? offers.Length : 0); i++)
        {
            PrototypeRpgUpgradeOfferCandidate offer = offers[i];
            if (offer != null && offer.UsedPoolExhaustionFallback)
            {
                fallbackCount += 1;
            }
        }

        return fallbackCount <= 0 ? "Pool exhaustion: no fallback used." : "Pool exhaustion: " + fallbackCount + " member(s) held continuity after the pool ran thin.";
    }
    private void ApplyRpgOfferPriorityBucketSurface(PrototypeRpgGrowthChoiceContext context, PrototypeRpgPostRunUpgradeOfferSurface surface)
    {
        if (surface == null)
        {
            return;
        }

        PrototypeRpgUpgradeOfferCandidate[] offers = surface.Offers ?? System.Array.Empty<PrototypeRpgUpgradeOfferCandidate>();
        PrototypeRpgUpgradeOfferCandidate topPick = null;
        PrototypeRpgUpgradeOfferCandidate rareSlot = null;
        PrototypeRpgUpgradeOfferCandidate recoverySlot = null;
        PrototypeRpgUpgradeOfferCandidate recommendedSlot = null;
        int topScore = int.MinValue;
        int rareScore = int.MinValue;
        int recoveryScore = int.MinValue;
        int recommendedScore = int.MinValue;
        for (int i = 0; i < offers.Length; i++)
        {
            PrototypeRpgUpgradeOfferCandidate offer = offers[i];
            if (offer == null)
            {
                continue;
            }

            offer.IsTopPriorityPick = false;
            offer.IsRecommendedSlot = false;
            offer.IsRareSlotOffer = false;
            offer.IsRecoverySafeguardSlot = false;
            offer.IsArchetypeCommitmentSlot = offer.PriorityBucketKey == "archetype_commitment_slot";
            int effectiveScore = ResolveRpgOfferEffectivePriorityScore(offer);
            if (topPick == null || effectiveScore > topScore)
            {
                topPick = offer;
                topScore = effectiveScore;
            }

            if (offer.PriorityBucketKey == "rare_slot" && (rareSlot == null || effectiveScore > rareScore))
            {
                rareSlot = offer;
                rareScore = effectiveScore;
            }

            if (offer.PriorityBucketKey == "recovery_safeguard_slot" && (recoverySlot == null || effectiveScore > recoveryScore))
            {
                recoverySlot = offer;
                recoveryScore = effectiveScore;
            }

            if ((offer.PriorityBucketKey == "recommended_slot" || offer.PriorityBucketKey == "archetype_commitment_slot") && (recommendedSlot == null || effectiveScore > recommendedScore))
            {
                recommendedSlot = offer;
                recommendedScore = effectiveScore;
            }
        }

        if (topPick != null)
        {
            topPick.IsTopPriorityPick = true;
            topPick.IsRecommendedSlot = topPick.PriorityBucketKey == "recommended_slot";
            topPick.IsArchetypeCommitmentSlot = topPick.PriorityBucketKey == "archetype_commitment_slot" || topPick.IsArchetypeCommitmentSlot;
            topPick.ReasonText = MergeRpgSummaryText(topPick.ReasonText, topPick.TopPickReasonText);
        }

        if (rareSlot != null)
        {
            rareSlot.IsRareSlotOffer = true;
            rareSlot.ReasonText = MergeRpgSummaryText(rareSlot.ReasonText, rareSlot.RareSlotReasonText);
            rareSlot.ContinuitySummaryText = MergeRpgSummaryText(rareSlot.ContinuitySummaryText, rareSlot.RareSlotReasonText);
        }

        if (recoverySlot != null)
        {
            recoverySlot.IsRecoverySafeguardSlot = true;
            recoverySlot.ReasonText = MergeRpgSummaryText(recoverySlot.ReasonText, recoverySlot.RecoverySafeguardReasonText);
            recoverySlot.ContinuitySummaryText = MergeRpgSummaryText(recoverySlot.ContinuitySummaryText, recoverySlot.RecoverySafeguardReasonText);
        }

        if (recommendedSlot != null)
        {
            recommendedSlot.IsRecommendedSlot = recommendedSlot.PriorityBucketKey == "recommended_slot" || recommendedSlot.IsRecommendedSlot;
            recommendedSlot.IsArchetypeCommitmentSlot = recommendedSlot.PriorityBucketKey == "archetype_commitment_slot" || recommendedSlot.IsArchetypeCommitmentSlot;
        }

        surface.WeightedPrioritySummaryText = BuildRpgWeightedPrioritySummaryText(context, topPick, recommendedSlot);
        surface.RareSlotSummaryText = BuildRpgRareSlotSummaryText(context, rareSlot);
        surface.HighImpactSequencingSummaryText = BuildRpgHighImpactSequencingSummaryText(context, topPick, rareSlot);
        surface.RecoverySafeguardSummaryText = BuildRpgRecoverySafeguardSummaryText(context, recoverySlot);

        if (context != null)
        {
            context.WeightedPrioritySummaryText = surface.WeightedPrioritySummaryText;
            context.RareSlotSummaryText = surface.RareSlotSummaryText;
            context.HighImpactSequencingSummaryText = surface.HighImpactSequencingSummaryText;
            context.RecoverySafeguardSummaryText = surface.RecoverySafeguardSummaryText;
        }

        PrototypeRpgPartyArchetypeRuntimeState partyState = _sessionRpgPartyArchetypeRuntimeState ?? new PrototypeRpgPartyArchetypeRuntimeState();
        partyState.LastPriorityRunIdentity = context != null && !string.IsNullOrEmpty(context.RunIdentity) ? context.RunIdentity : string.Empty;
        partyState.LastTopPriorityBucketKey = topPick != null && !string.IsNullOrEmpty(topPick.PriorityBucketKey) ? topPick.PriorityBucketKey : string.Empty;
        partyState.LastTopPriorityOfferTypeKey = topPick != null && !string.IsNullOrEmpty(topPick.OfferTypeKey) ? topPick.OfferTypeKey : string.Empty;
        partyState.LastRareSlotOfferTypeKey = rareSlot != null && !string.IsNullOrEmpty(rareSlot.OfferTypeKey) ? rareSlot.OfferTypeKey : string.Empty;
        partyState.WeightedPrioritySummaryText = surface.WeightedPrioritySummaryText;
        partyState.RareSlotSummaryText = surface.RareSlotSummaryText;
        partyState.HighImpactSequencingSummaryText = surface.HighImpactSequencingSummaryText;
        partyState.RecoverySafeguardSummaryText = surface.RecoverySafeguardSummaryText;
        _sessionRpgPartyArchetypeRuntimeState = partyState;
    }

    private int ResolveRpgOfferEffectivePriorityScore(PrototypeRpgUpgradeOfferCandidate candidate)
    {
        if (candidate == null)
        {
            return int.MinValue;
        }

        return candidate.RotationScore + candidate.PriorityWeightScore + candidate.RareSlotAllocationScore + candidate.HighImpactSequenceScore;
    }

    private int ScoreOfferAgainstPriorityBucket(
        PrototypeRpgGrowthChoiceContext context,
        PrototypeRpgUpgradeOfferCandidate candidate,
        PrototypeRpgGrowthChoiceMemberContext member,
        Dictionary<string, int> assignedTypeCounts,
        Dictionary<string, int> assignedGroupCounts)
    {
        if (context == null || candidate == null || member == null)
        {
            return 0;
        }

        PrototypeRpgPartyArchetypeRuntimeState partyState = _sessionRpgPartyArchetypeRuntimeState ?? new PrototypeRpgPartyArchetypeRuntimeState();
        string bucketKey = ResolveRpgPriorityBucketKey(context, candidate, member);
        string previousBucketKey = string.IsNullOrEmpty(partyState.LastTopPriorityBucketKey) ? string.Empty : partyState.LastTopPriorityBucketKey;
        string previousTopTypeKey = string.IsNullOrEmpty(partyState.LastTopPriorityOfferTypeKey) ? string.Empty : partyState.LastTopPriorityOfferTypeKey;
        string previousRareTypeKey = string.IsNullOrEmpty(partyState.LastRareSlotOfferTypeKey) ? string.Empty : partyState.LastRareSlotOfferTypeKey;
        int priorityScore = 0;
        int rareScore = 0;
        int sequenceScore = 0;
        candidate.PriorityBucketKey = bucketKey;
        candidate.PriorityBucketLabel = ResolveRpgPriorityBucketLabel(bucketKey);
        candidate.PriorityWeightScore = 0;
        candidate.RareSlotAllocationScore = 0;
        candidate.HighImpactSequenceScore = 0;
        candidate.TopPickReasonText = string.Empty;
        candidate.RareSlotReasonText = string.Empty;
        candidate.RecoverySafeguardReasonText = string.Empty;
        candidate.IsTopPriorityPick = false;
        candidate.IsRecommendedSlot = false;
        candidate.IsRareSlotOffer = false;
        candidate.IsRecoverySafeguardSlot = false;
        candidate.IsArchetypeCommitmentSlot = bucketKey == "archetype_commitment_slot";

        switch (bucketKey)
        {
            case "recovery_safeguard_slot":
                priorityScore += 18 + Mathf.Max(0, member.RecoveryBiasWeight / 2);
                priorityScore += context.ResultStateKey == PrototypeBattleOutcomeKeys.RunDefeat || context.ResultStateKey == PrototypeBattleOutcomeKeys.RunRetreat ? 8 : 0;
                candidate.RecoverySafeguardReasonText = BuildRpgRecoverySafeguardReasonText(context, candidate);
                break;
            case "rare_slot":
                priorityScore += 10 + candidate.UnlockPriority * 3 + Mathf.Max(0, partyState.CommitmentStrength / 8);
                priorityScore += context.EliteDefeated ? 6 : 0;
                rareScore += candidate.UsedTierEscalation ? 8 : 0;
                rareScore += candidate.UsedUnlockExpansion ? 5 : 0;
                rareScore += candidate.SupportsArchetypeFollowup ? 4 : 0;
                rareScore += candidate.ImprovesMissingCoverage || candidate.SupportsRescueOverride ? 5 : 0;
                candidate.RareSlotReasonText = BuildRpgRareSlotReasonText(context, candidate);
                break;
            case "archetype_commitment_slot":
                priorityScore += 14 + Mathf.Max(0, partyState.CommitmentStrength / 7);
                break;
            case "recommended_slot":
                priorityScore += 10;
                priorityScore += candidate.ImprovesMissingCoverage ? 6 : 0;
                priorityScore += candidate.ReducesRoleOverlap ? 4 : 0;
                priorityScore += candidate.SupportsCrossMemberSynergy ? 3 : 0;
                break;
            default:
                priorityScore += 2;
                break;
        }

        if (bucketKey == "rare_slot")
        {
            if (candidate.IsContradictoryToLane && !candidate.ImprovesMissingCoverage && !candidate.SupportsRescueOverride)
            {
                rareScore -= 10;
            }

            if (DoesRpgCandidateTouchRecentExposure(candidate, member))
            {
                rareScore -= 12;
            }

            rareScore -= GetRpgRotationCount(assignedTypeCounts, candidate.OfferTypeKey) * 4;
            rareScore -= GetRpgRotationCount(assignedGroupCounts, candidate.OfferGroupId) * 3;
        }

        if (!string.IsNullOrEmpty(previousBucketKey))
        {
            if (previousBucketKey == bucketKey)
            {
                sequenceScore -= 4;
            }

            if (!string.IsNullOrEmpty(previousTopTypeKey) && previousTopTypeKey == candidate.OfferTypeKey)
            {
                sequenceScore -= 4;
            }

            if (bucketKey == "rare_slot" && !string.IsNullOrEmpty(previousRareTypeKey) && previousRareTypeKey == candidate.OfferTypeKey)
            {
                sequenceScore -= 8;
            }

            if (previousBucketKey == "recovery_safeguard_slot" && (bucketKey == "archetype_commitment_slot" || bucketKey == "rare_slot") && !member.SoftRespecWindowOpen)
            {
                sequenceScore += 6;
            }
            else if (previousBucketKey == "rare_slot" && bucketKey == "recommended_slot" && (candidate.ImprovesMissingCoverage || candidate.ReducesRoleOverlap))
            {
                sequenceScore += 4;
            }
            else if (previousBucketKey == "archetype_commitment_slot" && bucketKey == "rare_slot" && (candidate.UsedTierEscalation || candidate.UsedUnlockExpansion))
            {
                sequenceScore += 5;
            }
            else if ((previousBucketKey == "rare_slot" || previousBucketKey == "archetype_commitment_slot") && bucketKey == "recovery_safeguard_slot" && member.SoftRespecWindowOpen)
            {
                sequenceScore += 6;
            }
        }

        candidate.PriorityWeightScore = priorityScore;
        candidate.RareSlotAllocationScore = rareScore;
        candidate.HighImpactSequenceScore = sequenceScore;
        candidate.TopPickReasonText = BuildRpgTopPickReasonText(context, candidate, member);
        if (bucketKey == "recovery_safeguard_slot" && string.IsNullOrEmpty(candidate.RecoverySafeguardReasonText))
        {
            candidate.RecoverySafeguardReasonText = BuildRpgRecoverySafeguardReasonText(context, candidate);
        }
        if (bucketKey == "rare_slot" && string.IsNullOrEmpty(candidate.RareSlotReasonText))
        {
            candidate.RareSlotReasonText = BuildRpgRareSlotReasonText(context, candidate);
        }

        member.WeightedPriorityBias = priorityScore;
        member.RareSlotTrustBias = rareScore;
        member.RecoverySafeguardBias = bucketKey == "recovery_safeguard_slot" ? priorityScore : 0;
        return priorityScore + rareScore + sequenceScore;
    }

    private string ResolveRpgPriorityBucketKey(PrototypeRpgGrowthChoiceContext context, PrototypeRpgUpgradeOfferCandidate candidate, PrototypeRpgGrowthChoiceMemberContext member)
    {
        bool needsRecovery = member != null &&
                             (member.SoftRespecWindowOpen ||
                              member.RecoveryPressureScore >= 6 ||
                              member.ConsecutiveKnockOutCount >= 2 ||
                              (context != null && (context.ResultStateKey == PrototypeBattleOutcomeKeys.RunDefeat || context.ResultStateKey == PrototypeBattleOutcomeKeys.RunRetreat)));
        bool rareEligible = candidate != null && !candidate.IsFallback && !candidate.IsNoOp && (candidate.UsedTierEscalation || candidate.UsedUnlockExpansion || candidate.UnlockPriority >= 4);
        bool commitmentEligible = candidate != null && candidate.SupportsArchetypeFollowup && !candidate.IsRecoveryEscapeOffer && ((_sessionRpgPartyArchetypeRuntimeState != null ? _sessionRpgPartyArchetypeRuntimeState.CommitmentStrength : 0) >= 45 || (member != null ? member.ArchetypeCommitmentWeight : 0) >= 6);
        bool recommendedEligible = candidate != null && (candidate.ImprovesMissingCoverage || candidate.ReducesRoleOverlap || candidate.SupportsCrossMemberSynergy || candidate.MaintainsHealthyFlex);

        if (needsRecovery && candidate != null && (candidate.IsRecoveryEscapeOffer || candidate.SupportsRecoveryWindow || candidate.LaneKey == "recovery" || candidate.ReducesRoleOverlap))
        {
            return "recovery_safeguard_slot";
        }

        if (rareEligible && candidate != null && (candidate.SupportsArchetypeFollowup || candidate.ImprovesMissingCoverage || candidate.SupportsRescueOverride || candidate.MaintainsHealthyFlex))
        {
            return "rare_slot";
        }

        if (commitmentEligible)
        {
            return "archetype_commitment_slot";
        }

        if (recommendedEligible)
        {
            return "recommended_slot";
        }

        return "common_slot";
    }

    private string ResolveRpgPriorityBucketLabel(string bucketKey)
    {
        switch (string.IsNullOrEmpty(bucketKey) ? string.Empty : bucketKey)
        {
            case "recommended_slot":
                return "Recommended Slot";
            case "rare_slot":
                return "Rare Slot";
            case "recovery_safeguard_slot":
                return "Recovery Safeguard Slot";
            case "archetype_commitment_slot":
                return "Archetype Commitment Slot";
            default:
                return "Common Slot";
        }
    }

    private string BuildRpgTopPickReasonText(PrototypeRpgGrowthChoiceContext context, PrototypeRpgUpgradeOfferCandidate candidate, PrototypeRpgGrowthChoiceMemberContext member)
    {
        if (candidate == null)
        {
            return string.Empty;
        }

        switch (candidate.PriorityBucketKey)
        {
            case "recovery_safeguard_slot":
                return "Top pick trust: keep a recovery safety slot visible while pressure still hangs over the party.";
            case "rare_slot":
                return "Top pick trust: reserve the rare slot for a higher-impact branch that still fits the current shell.";
            case "archetype_commitment_slot":
                return "Top pick trust: deepen the current archetype while commitment remains stable.";
            case "recommended_slot":
                return "Top pick trust: repair the formation before pushing a noisier branch.";
            default:
                return "Top pick trust: keep one lighter sidegrade visible without breaking continuity.";
        }
    }

    private string BuildRpgRareSlotReasonText(PrototypeRpgGrowthChoiceContext context, PrototypeRpgUpgradeOfferCandidate candidate)
    {
        if (candidate == null)
        {
            return string.Empty;
        }

        string impactText = candidate.UsedTierEscalation
            ? "a tier-escalated branch"
            : (candidate.UsedUnlockExpansion ? "a newly unlocked branch" : "a higher-impact branch");
        string archetypeText = string.IsNullOrEmpty(context != null ? context.PrimaryArchetypeKey : string.Empty)
            ? "the current formation"
            : ResolveRpgArchetypeLabel(context.PrimaryArchetypeKey);
        return "Rare slot: surface " + impactText + " because it still tracks " + archetypeText + " instead of drifting into noise.";
    }

    private string BuildRpgRecoverySafeguardReasonText(PrototypeRpgGrowthChoiceContext context, PrototypeRpgUpgradeOfferCandidate candidate)
    {
        if (candidate == null)
        {
            return string.Empty;
        }

        string triggerText = context != null && (context.ResultStateKey == PrototypeBattleOutcomeKeys.RunDefeat || context.ResultStateKey == PrototypeBattleOutcomeKeys.RunRetreat)
            ? "after the latest setback"
            : "while pressure remains uneven";
        return "Recovery safeguard: keep one escape hatch visible " + triggerText + ".";
    }

    private string BuildRpgWeightedPrioritySummaryText(PrototypeRpgGrowthChoiceContext context, PrototypeRpgUpgradeOfferCandidate topPick, PrototypeRpgUpgradeOfferCandidate recommendedSlot)
    {
        if (topPick == null)
        {
            return "Weighted priority: no top pick allocated.";
        }

        string summary = "Weighted priority: " + topPick.DisplayName + " -> " + topPick.OfferLabel + " is trusted as the " + ResolveRpgPriorityBucketLabel(topPick.PriorityBucketKey).ToLowerInvariant() + ".";
        if (recommendedSlot != null && recommendedSlot != topPick)
        {
            summary += " Secondary trust stays with " + recommendedSlot.DisplayName + " via " + ResolveRpgPriorityBucketLabel(recommendedSlot.PriorityBucketKey).ToLowerInvariant() + ".";
        }

        return summary;
    }

    private string BuildRpgRareSlotSummaryText(PrototypeRpgGrowthChoiceContext context, PrototypeRpgUpgradeOfferCandidate rareSlot)
    {
        if (rareSlot == null)
        {
            return "Rare slot: no coherent high-impact pick this run.";
        }

        return "Rare slot: " + rareSlot.DisplayName + " keeps " + rareSlot.OfferLabel + " visible because the branch stays coherent with the party shell.";
    }

    private string BuildRpgHighImpactSequencingSummaryText(PrototypeRpgGrowthChoiceContext context, PrototypeRpgUpgradeOfferCandidate topPick, PrototypeRpgUpgradeOfferCandidate rareSlot)
    {
        PrototypeRpgPartyArchetypeRuntimeState partyState = _sessionRpgPartyArchetypeRuntimeState ?? new PrototypeRpgPartyArchetypeRuntimeState();
        string previousBucket = string.IsNullOrEmpty(partyState.LastTopPriorityBucketKey) ? string.Empty : partyState.LastTopPriorityBucketKey;
        if (topPick == null)
        {
            return "High-impact sequencing: no top slot was resolved.";
        }

        if (string.IsNullOrEmpty(previousBucket))
        {
            return "High-impact sequencing: session starts by trusting the " + ResolveRpgPriorityBucketLabel(topPick.PriorityBucketKey).ToLowerInvariant() + ".";
        }

        if (previousBucket == topPick.PriorityBucketKey)
        {
            return "High-impact sequencing: hold the " + ResolveRpgPriorityBucketLabel(topPick.PriorityBucketKey).ToLowerInvariant() + " again because the shell still points the same way.";
        }

        return "High-impact sequencing: shift from " + ResolveRpgPriorityBucketLabel(previousBucket).ToLowerInvariant() + " into " + ResolveRpgPriorityBucketLabel(topPick.PriorityBucketKey).ToLowerInvariant() + ".";
    }

    private string BuildRpgRecoverySafeguardSummaryText(PrototypeRpgGrowthChoiceContext context, PrototypeRpgUpgradeOfferCandidate recoverySlot)
    {
        if (recoverySlot == null)
        {
            return context != null && (context.ResultStateKey == PrototypeBattleOutcomeKeys.RunDefeat || context.ResultStateKey == PrototypeBattleOutcomeKeys.RunRetreat)
                ? "Recovery safeguard: no separate safety slot survived the final sort."
                : "Recovery safeguard: no dedicated safety slot was needed this run.";
        }

        return "Recovery safeguard: " + recoverySlot.DisplayName + " keeps " + recoverySlot.OfferLabel + " visible as an anti-trap escape hatch.";
    }

    private string BuildRpgAppliedSlotSuffix(PrototypeRpgUpgradeOfferCandidate offer)
    {
        if (offer == null)
        {
            return string.Empty;
        }

        string summary = string.Empty;
        if (offer.IsTopPriorityPick)
        {
            summary = MergeRpgSummaryText(summary, "Top pick stays trusted.");
        }
        if (offer.IsRareSlotOffer)
        {
            summary = MergeRpgSummaryText(summary, "Rare slot stays visible.");
        }
        if (offer.IsRecoverySafeguardSlot)
        {
            summary = MergeRpgSummaryText(summary, "Recovery safeguard stays visible.");
        }

        return string.IsNullOrEmpty(summary) ? string.Empty : " " + summary;
    }


    private string BuildRpgOfferUnlockSurfaceSummary(PrototypeRpgGrowthChoiceContext context, PrototypeRpgUpgradeOfferCandidate[] offers)
    {
        int unlockedOfferCount = 0;
        for (int i = 0; i < (offers != null ? offers.Length : 0); i++)
        {
            PrototypeRpgUpgradeOfferCandidate offer = offers[i];
            if (offer != null && offer.UsedUnlockExpansion)
            {
                unlockedOfferCount += 1;
            }
        }

        string baseSummary = context != null && !string.IsNullOrEmpty(context.OfferUnlockSummaryText) ? context.OfferUnlockSummaryText : "Unlocks: base pool only.";
        return unlockedOfferCount <= 0 ? baseSummary : baseSummary + " Active unlocked offer(s): " + unlockedOfferCount + ".";
    }

    private string BuildRpgTierEscalationSurfaceSummary(PrototypeRpgGrowthChoiceContext context, PrototypeRpgUpgradeOfferCandidate[] offers)
    {
        int escalatedOfferCount = 0;
        for (int i = 0; i < (offers != null ? offers.Length : 0); i++)
        {
            PrototypeRpgUpgradeOfferCandidate offer = offers[i];
            if (offer != null && offer.UsedTierEscalation)
            {
                escalatedOfferCount += 1;
            }
        }

        string baseSummary = context != null && !string.IsNullOrEmpty(context.TierEscalationSummaryText) ? context.TierEscalationSummaryText : "Tier escalation: base tier only.";
        return escalatedOfferCount <= 0 ? baseSummary : baseSummary + " Active tiered offer(s): " + escalatedOfferCount + ".";
    }

    private string BuildRpgPoolExpansionSurfaceSummary(PrototypeRpgGrowthChoiceContext context, PrototypeRpgUpgradeOfferCandidate[] offers)
    {
        HashSet<string> expandedGroups = new HashSet<string>();
        for (int i = 0; i < (offers != null ? offers.Length : 0); i++)
        {
            PrototypeRpgUpgradeOfferCandidate offer = offers[i];
            if (offer == null || !offer.UsedUnlockExpansion || string.IsNullOrEmpty(offer.OfferGroupId))
            {
                continue;
            }

            expandedGroups.Add(offer.OfferGroupId);
        }

        string baseSummary = context != null && !string.IsNullOrEmpty(context.PoolExpansionSummaryText) ? context.PoolExpansionSummaryText : "Pool expansion: base pool only.";
        return expandedGroups.Count <= 0 ? baseSummary : baseSummary + " Active expanded group(s): " + expandedGroups.Count + ".";
    }

    private string BuildRpgTierEscalationReasonText(PrototypeRpgGrowthChoiceMemberContext member, PrototypeRpgUpgradeOfferCandidate candidate, PrototypeRpgGrowthChoiceContext context)
    {
        if (candidate == null || !candidate.UsedTierEscalation)
        {
            return string.Empty;
        }

        string tierLabel = PrototypeRpgGrowthChoiceRuleCatalog.ResolveTierLabel(candidate.UnlockTierKey);
        if (context != null && context.EliteDefeated)
        {
            return tierLabel + " unlocked after elite pressure.";
        }

        if (context != null && context.HasRewardCarryover)
        {
            return tierLabel + " unlocked from carryover momentum.";
        }

        if (member != null && member.RecentOfferCount >= 2)
        {
            return tierLabel + " unlocked after repeated exposure.";
        }

        return tierLabel + " unlocked for longer-session variety.";
    }
    private PrototypeRpgUpgradeOfferCandidate BuildRpgUpgradeOfferCandidate(PrototypeRpgGrowthChoiceMemberContext member, PrototypeRpgGrowthChoiceRuleDefinition rule)
    {
        if (member == null || rule == null)
        {
            return null;
        }

        PrototypeRpgUpgradeOfferCandidate candidate = new PrototypeRpgUpgradeOfferCandidate();
        candidate.OfferId = string.IsNullOrEmpty(member.MemberId) ? rule.RuleId : member.MemberId + ":" + rule.RuleId;
        candidate.OfferGroupId = string.IsNullOrEmpty(rule.OfferGroupId) ? rule.RuleId : rule.OfferGroupId;
        candidate.OfferTypeKey = string.IsNullOrEmpty(rule.OfferTypeKey) ? "growth_job_loadout" : rule.OfferTypeKey;
        candidate.MemberId = string.IsNullOrEmpty(member.MemberId) ? string.Empty : member.MemberId;
        candidate.DisplayName = string.IsNullOrEmpty(member.DisplayName) ? "Adventurer" : member.DisplayName;
        candidate.OfferLabel = string.IsNullOrEmpty(rule.OfferLabel) ? "Upgrade" : rule.OfferLabel;
        candidate.ReasonText = string.IsNullOrEmpty(rule.ReasonText) ? member.ContributionSummaryText : rule.ReasonText;
        candidate.ContinuitySummaryText = string.IsNullOrEmpty(rule.ContinuityText) ? "Continuity hold." : rule.ContinuityText;
        candidate.ExposureSummaryText = string.IsNullOrEmpty(member.RecentOfferExposureSummaryText) ? string.Empty : member.RecentOfferExposureSummaryText;
        candidate.UnlockGroupKey = string.IsNullOrEmpty(rule.UnlockGroupKey) ? string.Empty : rule.UnlockGroupKey;
        candidate.UnlockTierKey = string.IsNullOrEmpty(rule.UnlockTierKey) ? string.Empty : rule.UnlockTierKey;
        candidate.RequiredTierKey = string.IsNullOrEmpty(rule.RequiredTierKey) ? string.Empty : rule.RequiredTierKey;
        candidate.UnlockReasonText = string.IsNullOrEmpty(rule.UnlockReasonHint) ? string.Empty : rule.UnlockReasonHint;
        candidate.LaneKey = PrototypeRpgGrowthChoiceRuleCatalog.ResolveRuleLaneKey(rule);
        candidate.AdjacentLaneKey = PrototypeRpgGrowthChoiceRuleCatalog.ResolveRuleAdjacentLaneKey(rule);
        candidate.CommitmentWeight = PrototypeRpgGrowthChoiceRuleCatalog.ResolveRuleCommitmentWeight(rule);
        candidate.ContradictionPenalty = PrototypeRpgGrowthChoiceRuleCatalog.ResolveRuleContradictionPenalty(rule);
        candidate.SidegradeAllowance = PrototypeRpgGrowthChoiceRuleCatalog.ResolveRuleSidegradeAllowance(rule);
        candidate.RecoveryEscapeWeight = PrototypeRpgGrowthChoiceRuleCatalog.ResolveRuleRecoveryEscapeWeight(rule);
        candidate.CoherenceReasonHint = PrototypeRpgGrowthChoiceRuleCatalog.BuildRuleCoherenceReasonHint(rule);
        candidate.TargetGrowthTrackId = string.IsNullOrEmpty(rule.TargetGrowthTrackId) ? string.Empty : rule.TargetGrowthTrackId;
        candidate.TargetJobId = string.IsNullOrEmpty(rule.TargetJobId) ? string.Empty : rule.TargetJobId;
        candidate.TargetEquipmentLoadoutId = string.IsNullOrEmpty(rule.TargetEquipmentLoadoutId) ? string.Empty : rule.TargetEquipmentLoadoutId;
        candidate.TargetSkillLoadoutId = string.IsNullOrEmpty(rule.TargetSkillLoadoutId) ? string.Empty : rule.TargetSkillLoadoutId;
        candidate.IsFallback = rule.IsFallback;
        candidate.IsNoOp = rule.IsFallback && string.IsNullOrEmpty(rule.TargetGrowthTrackId) && string.IsNullOrEmpty(rule.TargetJobId) && string.IsNullOrEmpty(rule.TargetEquipmentLoadoutId) && string.IsNullOrEmpty(rule.TargetSkillLoadoutId);
        candidate.UsedUnlockExpansion = !string.IsNullOrEmpty(rule.UnlockGroupKey);
        candidate.UsedTierEscalation = !string.IsNullOrEmpty(rule.UnlockTierKey) && rule.UnlockTierKey != "tier_1";
        candidate.UnlockPriority = rule.UnlockPriority;
        string targetSummary = candidate.IsNoOp ? "keep current line" : BuildRpgOfferTargetSummary(candidate.TargetGrowthTrackId, candidate.TargetJobId, candidate.TargetEquipmentLoadoutId, candidate.TargetSkillLoadoutId);
        string tierPrefix = candidate.UsedTierEscalation ? " [" + PrototypeRpgGrowthChoiceRuleCatalog.ResolveTierLabel(candidate.UnlockTierKey) + "]" : string.Empty;
        string lanePrefix = string.IsNullOrEmpty(candidate.LaneKey) ? string.Empty : " {" + PrototypeRpgGrowthChoiceRuleCatalog.ResolveLaneLabel(candidate.LaneKey) + "}";
        candidate.SummaryText = candidate.DisplayName + " -> " + candidate.OfferLabel + tierPrefix + lanePrefix + " | " + targetSummary;
        if (member.SoftRespecWindowOpen)
        {
            candidate.RecoveryTriggerKeyAtSelection = string.IsNullOrEmpty(member.LastRecoveryTriggerKey) ? string.Empty : member.LastRecoveryTriggerKey;
            candidate.RecoveryWindowTierAtSelection = member.SoftRespecWindowTier;
            candidate.RecoveryReasonText = BuildRpgRecoveryOfferReasonText(null, candidate, member);
        }
        return candidate;
    }

    private bool IsRpgUpgradeOfferCandidateRedundant
(PrototypeRpgUpgradeOfferCandidate candidate, PrototypeRpgGrowthChoiceMemberContext member)
    {
        if (candidate == null || member == null || candidate.IsNoOp)
        {
            return false;
        }

        bool hasTarget = !string.IsNullOrEmpty(candidate.TargetGrowthTrackId) || !string.IsNullOrEmpty(candidate.TargetJobId) || !string.IsNullOrEmpty(candidate.TargetEquipmentLoadoutId) || !string.IsNullOrEmpty(candidate.TargetSkillLoadoutId);
        if (!hasTarget)
        {
            return true;
        }

        bool sameGrowth = string.IsNullOrEmpty(candidate.TargetGrowthTrackId) || candidate.TargetGrowthTrackId == member.CurrentGrowthTrackId;
        bool sameJob = string.IsNullOrEmpty(candidate.TargetJobId) || candidate.TargetJobId == member.CurrentJobId;
        bool sameEquipment = string.IsNullOrEmpty(candidate.TargetEquipmentLoadoutId) || candidate.TargetEquipmentLoadoutId == member.CurrentEquipmentLoadoutId;
        bool sameSkillLoadout = string.IsNullOrEmpty(candidate.TargetSkillLoadoutId) || candidate.TargetSkillLoadoutId == member.CurrentSkillLoadoutId;
        return sameGrowth && sameJob && sameEquipment && sameSkillLoadout;
    }

    private string BuildRpgOfferTargetSummary(string growthTrackId, string jobId, string equipmentLoadoutId, string skillLoadoutId)
    {
        string growthLabel = BuildRpgProgressionIdLabel(growthTrackId);
        string jobLabel = BuildRpgProgressionIdLabel(jobId);
        string equipmentLabel = BuildRpgProgressionIdLabel(equipmentLoadoutId);
        string skillLabel = BuildRpgProgressionIdLabel(skillLoadoutId);
        return growthLabel + " | " + jobLabel + " | " + equipmentLabel + " | " + skillLabel;
    }

    private PrototypeRpgApplyReadyChoice[] BuildRpgApplyReadyChoices(PrototypeRpgGrowthChoiceContext context, PrototypeRpgUpgradeOfferCandidate[] offers)
    {
        PrototypeRpgGrowthChoiceMemberContext[] members = context != null ? (context.Members ?? System.Array.Empty<PrototypeRpgGrowthChoiceMemberContext>()) : System.Array.Empty<PrototypeRpgGrowthChoiceMemberContext>();
        if (members.Length <= 0)
        {
            return System.Array.Empty<PrototypeRpgApplyReadyChoice>();
        }

        PrototypeRpgApplyReadyChoice[] choices = new PrototypeRpgApplyReadyChoice[members.Length];
        for (int i = 0; i < members.Length; i++)
        {
            PrototypeRpgGrowthChoiceMemberContext member = members[i] ?? new PrototypeRpgGrowthChoiceMemberContext();
            PrototypeRpgUpgradeOfferCandidate offer = offers != null && i < offers.Length ? offers[i] : null;
            PrototypeRpgApplyReadyChoice choice = new PrototypeRpgApplyReadyChoice();
            if (offer != null)
            {
                choice.OfferId = string.IsNullOrEmpty(offer.OfferId) ? string.Empty : offer.OfferId;
                choice.OfferGroupId = string.IsNullOrEmpty(offer.OfferGroupId) ? string.Empty : offer.OfferGroupId;
                choice.MemberId = string.IsNullOrEmpty(member.MemberId) ? string.Empty : member.MemberId;
                choice.DisplayName = string.IsNullOrEmpty(member.DisplayName) ? "Adventurer" : member.DisplayName;
                choice.HasChanges = !offer.IsNoOp;
                choice.AppliedGrowthTrackId = choice.HasChanges && !string.IsNullOrEmpty(offer.TargetGrowthTrackId) ? offer.TargetGrowthTrackId : member.CurrentGrowthTrackId;
                choice.AppliedJobId = choice.HasChanges && !string.IsNullOrEmpty(offer.TargetJobId) ? offer.TargetJobId : member.CurrentJobId;
                choice.AppliedEquipmentLoadoutId = choice.HasChanges && !string.IsNullOrEmpty(offer.TargetEquipmentLoadoutId) ? offer.TargetEquipmentLoadoutId : member.CurrentEquipmentLoadoutId;
                choice.AppliedSkillLoadoutId = choice.HasChanges && !string.IsNullOrEmpty(offer.TargetSkillLoadoutId) ? offer.TargetSkillLoadoutId : member.CurrentSkillLoadoutId;
                choice.AppliedSummaryText = choice.DisplayName + (choice.HasChanges ? " committed " + offer.OfferLabel + "." : " keeps the current line.") + BuildRpgAppliedSlotSuffix(offer);
                choice.ContinuitySummaryText = string.IsNullOrEmpty(offer.ContinuitySummaryText) ? "Continuity hold." : offer.ContinuitySummaryText;
                choice.ContinuitySummaryText = MergeRpgSummaryText(choice.ContinuitySummaryText, offer.TopPickReasonText);
                choice.ContinuitySummaryText = MergeRpgSummaryText(choice.ContinuitySummaryText, offer.RareSlotReasonText);
                choice.ContinuitySummaryText = MergeRpgSummaryText(choice.ContinuitySummaryText, offer.RecoverySafeguardReasonText);
                choice.ContinuitySummaryText = MergeRpgSummaryText(choice.ContinuitySummaryText, offer.MomentumReasonText);
                choice.ContinuitySummaryText = MergeRpgSummaryText(choice.ContinuitySummaryText, offer.IntensityHintText);
            }

            choices[i] = choice;
        }

        return choices;
    }

    private void CommitRpgApplyReadyChoices(PrototypeRpgGrowthChoiceContext context, PrototypeRpgPostRunUpgradeOfferSurface surface)
    {
        TestDungeonPartyData party = _activeDungeonParty;
        PrototypeRpgPartyDefinition partyDefinition = party != null ? party.PartyDefinition : null;
        PrototypeRpgAppliedPartyProgressState appliedState = GetOrCreateRpgAppliedPartyProgressState(partyDefinition);
        PrototypeRpgOfferExposureHistoryState exposureState = GetOrCreateRpgOfferExposureHistoryState(partyDefinition);
        PrototypeRpgOfferUnlockRuntimeState unlockState = GetOrCreateRpgOfferUnlockRuntimeState(partyDefinition);
        PrototypeRpgPartyLaneRecoveryRuntimeState recoveryState = GetOrCreateRpgPartyLaneRecoveryRuntimeState(partyDefinition);
        PrototypeRpgApplyReadyChoice[] choices = surface != null ? (surface.ApplyReadyChoices ?? System.Array.Empty<PrototypeRpgApplyReadyChoice>()) : System.Array.Empty<PrototypeRpgApplyReadyChoice>();
        for (int i = 0; i < choices.Length; i++)
        {
            PrototypeRpgApplyReadyChoice choice = choices[i];
            PrototypeRpgAppliedPartyMemberProgressState memberState = GetRpgAppliedPartyMemberProgressState(appliedState, choice != null ? choice.MemberId : string.Empty);
            PrototypeRpgMemberOfferExposureState exposureMember = GetRpgMemberOfferExposureState(exposureState, choice != null ? choice.MemberId : string.Empty);
            PrototypeRpgMemberOfferUnlockState unlockMember = GetRpgMemberOfferUnlockState(unlockState, choice != null ? choice.MemberId : string.Empty);
            PrototypeRpgMemberLaneRecoveryRuntimeState recoveryMember = GetRpgMemberLaneRecoveryRuntimeState(recoveryState, choice != null ? choice.MemberId : string.Empty);
            PrototypeRpgUpgradeOfferCandidate offer = FindRpgUpgradeOfferCandidate(surface, choice != null ? choice.MemberId : string.Empty);
            if (choice == null || memberState == null)
            {
                continue;
            }

            if (choice.HasChanges)
            {
                memberState.AppliedGrowthTrackId = string.IsNullOrEmpty(choice.AppliedGrowthTrackId) ? memberState.AppliedGrowthTrackId : choice.AppliedGrowthTrackId;
                memberState.AppliedJobId = string.IsNullOrEmpty(choice.AppliedJobId) ? memberState.AppliedJobId : choice.AppliedJobId;
                memberState.AppliedEquipmentLoadoutId = string.IsNullOrEmpty(choice.AppliedEquipmentLoadoutId) ? memberState.AppliedEquipmentLoadoutId : choice.AppliedEquipmentLoadoutId;
                memberState.AppliedSkillLoadoutId = string.IsNullOrEmpty(choice.AppliedSkillLoadoutId) ? memberState.AppliedSkillLoadoutId : choice.AppliedSkillLoadoutId;
            }

            memberState.LastAppliedOfferId = string.IsNullOrEmpty(choice.OfferId) ? string.Empty : choice.OfferId;
            memberState.LastAppliedOfferGroupId = string.IsNullOrEmpty(choice.OfferGroupId) ? string.Empty : choice.OfferGroupId;
            memberState.LastAppliedOfferTypeKey = string.IsNullOrEmpty(offer != null ? offer.OfferTypeKey : string.Empty) ? string.Empty : offer.OfferTypeKey;
            memberState.LastAppliedRunIdentity = context != null && !string.IsNullOrEmpty(context.RunIdentity) ? context.RunIdentity : string.Empty;
            memberState.LastAppliedSummaryText = string.IsNullOrEmpty(choice.AppliedSummaryText) ? string.Empty : choice.AppliedSummaryText;
            if (exposureMember != null && offer != null)
            {
                exposureMember.RecentlyOfferedIds = PushRpgRecentHistoryValue(exposureMember.RecentlyOfferedIds, offer.OfferId, 3);
                exposureMember.RecentlyOfferedGroupKeys = PushRpgRecentHistoryValue(exposureMember.RecentlyOfferedGroupKeys, offer.OfferGroupId, 3);
                exposureMember.RecentlyOfferedTypeKeys = PushRpgRecentHistoryValue(exposureMember.RecentlyOfferedTypeKeys, offer.OfferTypeKey, 3);
                exposureMember.RecentRunIdentity = context != null && !string.IsNullOrEmpty(context.RunIdentity) ? context.RunIdentity : string.Empty;
                exposureMember.RecentOfferCount = System.Math.Min(exposureMember.RecentOfferCount + 1, 99);
                exposureMember.RecentSelectedOfferId = string.IsNullOrEmpty(choice.OfferId) ? string.Empty : choice.OfferId;
                exposureMember.ExposureSummaryText = BuildRpgOfferExposureMemberSummaryText(exposureMember);
            }

            if (unlockMember != null && offer != null)
            {
                if (!string.IsNullOrEmpty(offer.OfferId) && (offer.UsedUnlockExpansion || offer.UsedTierEscalation))
                {
                    unlockMember.UnlockedOfferIds = PushRpgRecentHistoryValue(unlockMember.UnlockedOfferIds, offer.OfferId, 12);
                }

                if (!string.IsNullOrEmpty(offer.UnlockGroupKey))
                {
                    unlockMember.UnlockedGroupKeys = PushRpgRecentHistoryValue(unlockMember.UnlockedGroupKeys, offer.UnlockGroupKey, 12);
                }

                if (!string.IsNullOrEmpty(offer.UnlockTierKey))
                {
                    unlockMember.UnlockedTierKeys = PushRpgRecentHistoryValue(unlockMember.UnlockedTierKeys, offer.UnlockTierKey, 6);
                    unlockMember.LastEscalatedTierKey = offer.UnlockTierKey;
                }

                string unlockReasonText = string.Empty;
                if (!string.IsNullOrEmpty(offer.UnlockReasonText))
                {
                    unlockReasonText = offer.UnlockReasonText;
                }

                if (!string.IsNullOrEmpty(offer.TierEscalationReasonText))
                {
                    unlockReasonText = string.IsNullOrEmpty(unlockReasonText) ? offer.TierEscalationReasonText : unlockReasonText + " | " + offer.TierEscalationReasonText;
                }

                if (!string.IsNullOrEmpty(unlockReasonText))
                {
                    unlockMember.RecentUnlockReasonText = unlockReasonText;
                    unlockMember.LastUnlockRunIdentity = context != null && !string.IsNullOrEmpty(context.RunIdentity) ? context.RunIdentity : string.Empty;
                }

                unlockMember.UnlockSummaryText = BuildRpgOfferUnlockMemberSummaryText(unlockMember);
            }

            ApplyRpgRecoveryChoiceResolution(recoveryMember, offer, context);
        }

        EvaluateRpgBuildLaneState(context, null, partyDefinition);
        RefreshRpgLaneRecoveryStateConsistency(context, recoveryState);
        EvaluateRpgPartyCoverageState(context, null, partyDefinition);
        EvaluateRpgPartyArchetypeState(context, null, partyDefinition);
        appliedState.LastCommittedRunIdentity = context != null && !string.IsNullOrEmpty(context.RunIdentity) ? context.RunIdentity : string.Empty;
        appliedState.LastCommittedResultStateKey = context != null && !string.IsNullOrEmpty(context.ResultStateKey) ? context.ResultStateKey : string.Empty;
        appliedState.AppliedSummaryText = BuildRpgAppliedPartySummaryText(appliedState, partyDefinition);
        exposureState.LastRunIdentity = context != null && !string.IsNullOrEmpty(context.RunIdentity) ? context.RunIdentity : string.Empty;
        exposureState.SummaryText = BuildRpgOfferExposureSummaryText(exposureState);
        unlockState.LastRunIdentity = context != null && !string.IsNullOrEmpty(context.RunIdentity) ? context.RunIdentity : string.Empty;
        unlockState.OfferUnlockSummaryText = BuildRpgOfferUnlockSummaryText(unlockState);
        unlockState.TierEscalationSummaryText = BuildRpgTierEscalationSurfaceSummary(unlockState);
        unlockState.PoolExpansionSummaryText = BuildRpgPoolExpansionSurfaceSummary(unlockState);
        appliedState.OfferContinuitySummaryText = BuildRpgOfferContinuitySummaryText(context, surface, appliedState);
        if (surface != null)
        {
            surface.AppliedPartySummaryText = appliedState.AppliedSummaryText;
            surface.OfferRotationSummaryText = BuildRpgOfferRotationSummaryText(context, surface.Offers);
            surface.RecentOfferExposureSummaryText = exposureState.SummaryText;
            surface.OfferUnlockSummaryText = string.IsNullOrEmpty(surface.OfferUnlockSummaryText) ? unlockState.OfferUnlockSummaryText : surface.OfferUnlockSummaryText;
            surface.TierEscalationSummaryText = string.IsNullOrEmpty(surface.TierEscalationSummaryText) ? unlockState.TierEscalationSummaryText : surface.TierEscalationSummaryText;
            surface.PoolExpansionSummaryText = string.IsNullOrEmpty(surface.PoolExpansionSummaryText) ? unlockState.PoolExpansionSummaryText : surface.PoolExpansionSummaryText;
            surface.PoolExhaustionSummaryText = BuildRpgPoolExhaustionSummaryText(surface.Offers);
            surface.BuildLaneSummaryText = string.IsNullOrEmpty(context != null ? context.BuildLaneSummaryText : string.Empty) ? CurrentBuildLaneSummaryText : context.BuildLaneSummaryText;
            surface.ArchetypeCommitmentSummaryText = string.IsNullOrEmpty(context != null ? context.ArchetypeCommitmentSummaryText : string.Empty) ? CurrentArchetypeCommitmentSummaryText : context.ArchetypeCommitmentSummaryText;
            surface.OfferCoherenceSummaryText = string.IsNullOrEmpty(context != null ? context.OfferCoherenceSummaryText : string.Empty) ? string.Empty : context.OfferCoherenceSummaryText;
            surface.LaneRecoverySummaryText = string.IsNullOrEmpty(context != null ? context.LaneRecoverySummaryText : string.Empty) ? CurrentLaneRecoverySummaryText : context.LaneRecoverySummaryText;
            surface.SoftRespecWindowSummaryText = string.IsNullOrEmpty(context != null ? context.SoftRespecWindowSummaryText : string.Empty) ? CurrentSoftRespecWindowSummaryText : context.SoftRespecWindowSummaryText;
            surface.AntiTrapSafetySummaryText = string.IsNullOrEmpty(context != null ? context.AntiTrapSafetySummaryText : string.Empty) ? CurrentAntiTrapSafetySummaryText : context.AntiTrapSafetySummaryText;
            surface.PartyArchetypeSummaryText = string.IsNullOrEmpty(context != null ? context.PartyArchetypeSummaryText : string.Empty) ? CurrentPartyArchetypeSummaryText : context.PartyArchetypeSummaryText;
            surface.FormationCoherenceSummaryText = string.IsNullOrEmpty(context != null ? context.FormationCoherenceSummaryText : string.Empty) ? CurrentFormationCoherenceSummaryText : context.FormationCoherenceSummaryText;
            surface.CommitmentStrengthSummaryText = string.IsNullOrEmpty(context != null ? context.CommitmentStrengthSummaryText : string.Empty) ? CurrentArchetypeCommitmentStrengthSummaryText : context.CommitmentStrengthSummaryText;
            surface.FlexRescueSummaryText = string.IsNullOrEmpty(context != null ? context.FlexRescueSummaryText : string.Empty) ? CurrentArchetypeFlexRescueSummaryText : context.FlexRescueSummaryText;
            surface.OfferContinuitySummaryText = appliedState.OfferContinuitySummaryText;
            surface.ApplyReadySummaryText = BuildRpgApplyReadySummaryText(choices);
            surface.OfferRefreshSummaryText = BuildRpgOfferRefreshSummaryText(surface, choices);
            surface.FallbackReasonText = BuildRpgOfferFallbackSummary(surface.Offers);
        }
    }

    private string BuildRpgApplyReadySummaryText
(PrototypeRpgApplyReadyChoice[] choices)
    {
        if (choices == null || choices.Length <= 0)
        {
            return "Apply-ready: none.";
        }

        string summary = string.Empty;
        for (int i = 0; i < choices.Length; i++)
        {
            PrototypeRpgApplyReadyChoice choice = choices[i];
            if (choice == null || string.IsNullOrEmpty(choice.AppliedSummaryText))
            {
                continue;
            }

            summary = string.IsNullOrEmpty(summary) ? choice.AppliedSummaryText : summary + " | " + choice.AppliedSummaryText;
        }

        return string.IsNullOrEmpty(summary) ? "Apply-ready: none." : summary;
    }

    private string BuildRpgOfferRefreshSummaryText(PrototypeRpgPostRunUpgradeOfferSurface surface, PrototypeRpgApplyReadyChoice[] choices)
    {
        int changedCount = 0;
        int heldCount = 0;
        int rotatedCount = 0;
        int exhaustionFallbackCount = 0;
        if (choices != null)
        {
            for (int i = 0; i < choices.Length; i++)
            {
                PrototypeRpgApplyReadyChoice choice = choices[i];
                if (choice == null)
                {
                    continue;
                }

                if (choice.HasChanges)
                {
                    changedCount += 1;
                }
                else
                {
                    heldCount += 1;
                }
            }
        }

        PrototypeRpgUpgradeOfferCandidate[] offers = surface != null ? (surface.Offers ?? System.Array.Empty<PrototypeRpgUpgradeOfferCandidate>()) : System.Array.Empty<PrototypeRpgUpgradeOfferCandidate>();
        for (int i = 0; i < offers.Length; i++)
        {
            PrototypeRpgUpgradeOfferCandidate offer = offers[i];
            if (offer == null)
            {
                continue;
            }

            if (offer.UsedRotationFallback)
            {
                rotatedCount += 1;
            }

            if (offer.UsedPoolExhaustionFallback)
            {
                exhaustionFallbackCount += 1;
            }
        }

        string summary = "Offer refresh: " + changedCount + " change(s), " + heldCount + " continuity hold(s), " + rotatedCount + " rotated, " + exhaustionFallbackCount + " exhaustion fallback(s).";
        if (surface != null && !string.IsNullOrEmpty(surface.OfferCoherenceSummaryText)) { summary += " | " + surface.OfferCoherenceSummaryText; }
        if (surface != null && !string.IsNullOrEmpty(surface.OfferUnlockSummaryText)) { summary += " | " + surface.OfferUnlockSummaryText; }
        if (surface != null && !string.IsNullOrEmpty(surface.TierEscalationSummaryText)) { summary += " | " + surface.TierEscalationSummaryText; }
        if (surface != null && !string.IsNullOrEmpty(surface.LaneRecoverySummaryText)) { summary += " | " + surface.LaneRecoverySummaryText; }
        if (surface != null && !string.IsNullOrEmpty(surface.SoftRespecWindowSummaryText)) { summary += " | " + surface.SoftRespecWindowSummaryText; }
        if (surface != null && !string.IsNullOrEmpty(surface.AntiTrapSafetySummaryText)) { summary += " | " + surface.AntiTrapSafetySummaryText; }
        if (surface != null && !string.IsNullOrEmpty(surface.PartyArchetypeSummaryText)) { summary += " | " + surface.PartyArchetypeSummaryText; }
        if (surface != null && !string.IsNullOrEmpty(surface.FormationCoherenceSummaryText)) { summary += " | " + surface.FormationCoherenceSummaryText; }
        if (surface != null && !string.IsNullOrEmpty(surface.CommitmentStrengthSummaryText)) { summary += " | " + surface.CommitmentStrengthSummaryText; }
        if (surface != null && !string.IsNullOrEmpty(surface.FlexRescueSummaryText)) { summary += " | " + surface.FlexRescueSummaryText; }
        if (surface != null && !string.IsNullOrEmpty(surface.WeightedPrioritySummaryText)) { summary += " | " + surface.WeightedPrioritySummaryText; }
        if (surface != null && !string.IsNullOrEmpty(surface.RareSlotSummaryText)) { summary += " | " + surface.RareSlotSummaryText; }
        if (surface != null && !string.IsNullOrEmpty(surface.HighImpactSequencingSummaryText)) { summary += " | " + surface.HighImpactSequencingSummaryText; }
        if (surface != null && !string.IsNullOrEmpty(surface.RecoverySafeguardSummaryText)) { summary += " | " + surface.RecoverySafeguardSummaryText; }
        if (surface != null && !string.IsNullOrEmpty(surface.PartyCoverageSummaryText)) { summary += " | " + surface.PartyCoverageSummaryText; }
        if (surface != null && !string.IsNullOrEmpty(surface.RoleOverlapWarningSummaryText)) { summary += " | " + surface.RoleOverlapWarningSummaryText; }
        if (surface != null && !string.IsNullOrEmpty(surface.CrossMemberSynergySummaryText)) { summary += " | " + surface.CrossMemberSynergySummaryText; }
        if (surface != null && !string.IsNullOrEmpty(surface.StreakSensitivePacingSummaryText)) { summary += " | " + surface.StreakSensitivePacingSummaryText; }
        if (surface != null && !string.IsNullOrEmpty(surface.ClearStreakDampeningSummaryText)) { summary += " | " + surface.ClearStreakDampeningSummaryText; }
        if (surface != null && !string.IsNullOrEmpty(surface.ComebackReliefSummaryText)) { summary += " | " + surface.ComebackReliefSummaryText; }
        if (surface != null && !string.IsNullOrEmpty(surface.MomentumBalancingSummaryText)) { summary += " | " + surface.MomentumBalancingSummaryText; }
        if (surface != null && !string.IsNullOrEmpty(surface.CurrentMomentumTierSummaryText)) { summary += " | " + surface.CurrentMomentumTierSummaryText; }
        if (surface != null && !string.IsNullOrEmpty(surface.NextOfferIntensityHintText)) { summary += " | " + surface.NextOfferIntensityHintText; }
        return summary;
    }
    private string BuildRpgOfferFallbackSummary
(PrototypeRpgUpgradeOfferCandidate[] offers)
    {
        if (offers == null || offers.Length <= 0)
        {
            return string.Empty;
        }

        string summary = string.Empty;
        for (int i = 0; i < offers.Length; i++)
        {
            PrototypeRpgUpgradeOfferCandidate offer = offers[i];
            if (offer == null || !offer.IsFallback)
            {
                continue;
            }

            string reasonText = offer.UsedPoolExhaustionFallback && !string.IsNullOrEmpty(offer.PoolExhaustionReasonText) ? offer.PoolExhaustionReasonText : offer.ReasonText;
            string fallbackText = string.IsNullOrEmpty(reasonText) ? offer.SummaryText : offer.DisplayName + ": " + reasonText;
            summary = string.IsNullOrEmpty(summary) ? fallbackText : summary + " | " + fallbackText;
        }

        return summary;
    }

    private string BuildRpgOfferContinuitySummaryText(PrototypeRpgGrowthChoiceContext context, PrototypeRpgPostRunUpgradeOfferSurface surface, PrototypeRpgAppliedPartyProgressState appliedState)
    {
        string runIdentity = context != null && !string.IsNullOrEmpty(context.RunIdentity) ? context.RunIdentity : string.Empty;
        string partySummary = appliedState != null && !string.IsNullOrEmpty(appliedState.AppliedSummaryText) ? appliedState.AppliedSummaryText : "No applied progression.";
        string exposureSummary = _sessionRpgOfferExposureHistoryState != null && !string.IsNullOrEmpty(_sessionRpgOfferExposureHistoryState.SummaryText) ? _sessionRpgOfferExposureHistoryState.SummaryText : (context != null && !string.IsNullOrEmpty(context.RecentOfferExposureSummaryText) ? context.RecentOfferExposureSummaryText : "Recent exposure: none.");
        string rotationSummary = surface != null && !string.IsNullOrEmpty(surface.OfferRotationSummaryText) ? surface.OfferRotationSummaryText : (context != null && !string.IsNullOrEmpty(context.OfferRotationSummaryText) ? context.OfferRotationSummaryText : "Rotation: no shifts.");
        string unlockSummary = surface != null && !string.IsNullOrEmpty(surface.OfferUnlockSummaryText) ? surface.OfferUnlockSummaryText : (context != null && !string.IsNullOrEmpty(context.OfferUnlockSummaryText) ? context.OfferUnlockSummaryText : "Unlocks: base pool only.");
        string tierSummary = surface != null && !string.IsNullOrEmpty(surface.TierEscalationSummaryText) ? surface.TierEscalationSummaryText : (context != null && !string.IsNullOrEmpty(context.TierEscalationSummaryText) ? context.TierEscalationSummaryText : "Tier escalation: base tier only.");
        string poolSummary = surface != null && !string.IsNullOrEmpty(surface.PoolExpansionSummaryText) ? surface.PoolExpansionSummaryText : (context != null && !string.IsNullOrEmpty(context.PoolExpansionSummaryText) ? context.PoolExpansionSummaryText : "Pool expansion: base pool only.");
        string laneSummary = surface != null && !string.IsNullOrEmpty(surface.BuildLaneSummaryText) ? surface.BuildLaneSummaryText : (context != null && !string.IsNullOrEmpty(context.BuildLaneSummaryText) ? context.BuildLaneSummaryText : CurrentBuildLaneSummaryText);
        string commitmentSummary = surface != null && !string.IsNullOrEmpty(surface.ArchetypeCommitmentSummaryText) ? surface.ArchetypeCommitmentSummaryText : (context != null && !string.IsNullOrEmpty(context.ArchetypeCommitmentSummaryText) ? context.ArchetypeCommitmentSummaryText : CurrentArchetypeCommitmentSummaryText);
        string recoverySummary = surface != null && !string.IsNullOrEmpty(surface.LaneRecoverySummaryText) ? surface.LaneRecoverySummaryText : (context != null && !string.IsNullOrEmpty(context.LaneRecoverySummaryText) ? context.LaneRecoverySummaryText : CurrentLaneRecoverySummaryText);
        string respecSummary = surface != null && !string.IsNullOrEmpty(surface.SoftRespecWindowSummaryText) ? surface.SoftRespecWindowSummaryText : (context != null && !string.IsNullOrEmpty(context.SoftRespecWindowSummaryText) ? context.SoftRespecWindowSummaryText : CurrentSoftRespecWindowSummaryText);
        string antiTrapSummary = surface != null && !string.IsNullOrEmpty(surface.AntiTrapSafetySummaryText) ? surface.AntiTrapSafetySummaryText : (context != null && !string.IsNullOrEmpty(context.AntiTrapSafetySummaryText) ? context.AntiTrapSafetySummaryText : CurrentAntiTrapSafetySummaryText);
        string partyArchetypeSummary = surface != null && !string.IsNullOrEmpty(surface.PartyArchetypeSummaryText) ? surface.PartyArchetypeSummaryText : (context != null && !string.IsNullOrEmpty(context.PartyArchetypeSummaryText) ? context.PartyArchetypeSummaryText : CurrentPartyArchetypeSummaryText);
        string formationSummary = surface != null && !string.IsNullOrEmpty(surface.FormationCoherenceSummaryText) ? surface.FormationCoherenceSummaryText : (context != null && !string.IsNullOrEmpty(context.FormationCoherenceSummaryText) ? context.FormationCoherenceSummaryText : CurrentFormationCoherenceSummaryText);
        string strengthSummary = surface != null && !string.IsNullOrEmpty(surface.CommitmentStrengthSummaryText) ? surface.CommitmentStrengthSummaryText : (context != null && !string.IsNullOrEmpty(context.CommitmentStrengthSummaryText) ? context.CommitmentStrengthSummaryText : CurrentArchetypeCommitmentStrengthSummaryText);
        string flexSummary = surface != null && !string.IsNullOrEmpty(surface.FlexRescueSummaryText) ? surface.FlexRescueSummaryText : (context != null && !string.IsNullOrEmpty(context.FlexRescueSummaryText) ? context.FlexRescueSummaryText : CurrentArchetypeFlexRescueSummaryText);
        string weightedPrioritySummary = surface != null && !string.IsNullOrEmpty(surface.WeightedPrioritySummaryText) ? surface.WeightedPrioritySummaryText : (context != null && !string.IsNullOrEmpty(context.WeightedPrioritySummaryText) ? context.WeightedPrioritySummaryText : CurrentWeightedPrioritySummaryText);
        string rareSlotSummary = surface != null && !string.IsNullOrEmpty(surface.RareSlotSummaryText) ? surface.RareSlotSummaryText : (context != null && !string.IsNullOrEmpty(context.RareSlotSummaryText) ? context.RareSlotSummaryText : CurrentRareSlotSummaryText);
        string highImpactSummary = surface != null && !string.IsNullOrEmpty(surface.HighImpactSequencingSummaryText) ? surface.HighImpactSequencingSummaryText : (context != null && !string.IsNullOrEmpty(context.HighImpactSequencingSummaryText) ? context.HighImpactSequencingSummaryText : CurrentHighImpactSequencingSummaryText);
        string recoverySlotSummary = surface != null && !string.IsNullOrEmpty(surface.RecoverySafeguardSummaryText) ? surface.RecoverySafeguardSummaryText : (context != null && !string.IsNullOrEmpty(context.RecoverySafeguardSummaryText) ? context.RecoverySafeguardSummaryText : CurrentRecoverySafeguardSummaryText);
        string coverageSummary = surface != null && !string.IsNullOrEmpty(surface.PartyCoverageSummaryText) ? surface.PartyCoverageSummaryText : (context != null && !string.IsNullOrEmpty(context.PartyCoverageSummaryText) ? context.PartyCoverageSummaryText : CurrentPartyCoverageSummaryText);
        string overlapSummary = surface != null && !string.IsNullOrEmpty(surface.RoleOverlapWarningSummaryText) ? surface.RoleOverlapWarningSummaryText : (context != null && !string.IsNullOrEmpty(context.RoleOverlapWarningSummaryText) ? context.RoleOverlapWarningSummaryText : CurrentRoleOverlapWarningSummaryText);
        string synergySummary = surface != null && !string.IsNullOrEmpty(surface.CrossMemberSynergySummaryText) ? surface.CrossMemberSynergySummaryText : (context != null && !string.IsNullOrEmpty(context.CrossMemberSynergySummaryText) ? context.CrossMemberSynergySummaryText : CurrentCrossMemberSynergySummaryText);
        string streakPacingSummary = surface != null && !string.IsNullOrEmpty(surface.StreakSensitivePacingSummaryText) ? surface.StreakSensitivePacingSummaryText : (context != null && !string.IsNullOrEmpty(context.StreakSensitivePacingSummaryText) ? context.StreakSensitivePacingSummaryText : CurrentStreakSensitivePacingSummaryText);
        string clearDampeningSummary = surface != null && !string.IsNullOrEmpty(surface.ClearStreakDampeningSummaryText) ? surface.ClearStreakDampeningSummaryText : (context != null && !string.IsNullOrEmpty(context.ClearStreakDampeningSummaryText) ? context.ClearStreakDampeningSummaryText : CurrentClearStreakDampeningSummaryText);
        string comebackReliefSummary = surface != null && !string.IsNullOrEmpty(surface.ComebackReliefSummaryText) ? surface.ComebackReliefSummaryText : (context != null && !string.IsNullOrEmpty(context.ComebackReliefSummaryText) ? context.ComebackReliefSummaryText : CurrentComebackReliefSummaryText);
        string momentumBalancingSummary = surface != null && !string.IsNullOrEmpty(surface.MomentumBalancingSummaryText) ? surface.MomentumBalancingSummaryText : (context != null && !string.IsNullOrEmpty(context.MomentumBalancingSummaryText) ? context.MomentumBalancingSummaryText : CurrentMomentumBalancingSummaryText);
        string momentumTierSummary = surface != null && !string.IsNullOrEmpty(surface.CurrentMomentumTierSummaryText) ? surface.CurrentMomentumTierSummaryText : (context != null && !string.IsNullOrEmpty(context.CurrentMomentumTierSummaryText) ? context.CurrentMomentumTierSummaryText : CurrentMomentumTierSummaryText);
        string nextIntensitySummary = surface != null && !string.IsNullOrEmpty(surface.NextOfferIntensityHintText) ? surface.NextOfferIntensityHintText : (context != null && !string.IsNullOrEmpty(context.NextOfferIntensityHintText) ? context.NextOfferIntensityHintText : CurrentNextOfferIntensityHintText);
        return "Continuity: next clear refreshes from committed lineup " + runIdentity + " | " + partySummary + " | " + exposureSummary + " | " + rotationSummary + " | " + unlockSummary + " | " + tierSummary + " | " + poolSummary + " | " + laneSummary + " | " + commitmentSummary + " | " + recoverySummary + " | " + respecSummary + " | " + antiTrapSummary + " | " + partyArchetypeSummary + " | " + formationSummary + " | " + strengthSummary + " | " + flexSummary + " | " + weightedPrioritySummary + " | " + rareSlotSummary + " | " + highImpactSummary + " | " + recoverySlotSummary + " | " + coverageSummary + " | " + overlapSummary + " | " + synergySummary + " | " + streakPacingSummary + " | " + clearDampeningSummary + " | " + comebackReliefSummary + " | " + momentumBalancingSummary + " | " + momentumTierSummary + " | " + nextIntensitySummary;
    }


    private PrototypeRpgPostRunUpgradeOfferSurface BuildRpgPostRunUpgradeOfferSurface(PrototypeRpgGrowthChoiceContext context)
    {
        PrototypeRpgPostRunUpgradeOfferSurface surface = new PrototypeRpgPostRunUpgradeOfferSurface();
        if (context == null)
        {
            return surface;
        }

        surface.RunIdentity = string.IsNullOrEmpty(context.RunIdentity) ? string.Empty : context.RunIdentity;
        surface.OfferBasisSummaryText = string.IsNullOrEmpty(context.OfferBasisSummaryText) ? "Next basis: none." : context.OfferBasisSummaryText;
        surface.Offers = BuildRpgUpgradeOfferCandidates(context);
        ApplyRpgOfferPriorityBucketSurface(context, surface);
        ApplyRpgOfferMomentumSurface(context, surface);
        surface.ApplyReadyChoices = BuildRpgApplyReadyChoices(context, surface.Offers);
        surface.OfferRotationSummaryText = BuildRpgOfferRotationSummaryText(context, surface.Offers);
        surface.RecentOfferExposureSummaryText = BuildRpgRecentOfferExposureSurfaceSummary(context, surface.Offers);
        surface.PoolExhaustionSummaryText = BuildRpgPoolExhaustionSummaryText(surface.Offers);
        surface.BuildLaneSummaryText = string.IsNullOrEmpty(context.BuildLaneSummaryText) ? CurrentBuildLaneSummaryText : context.BuildLaneSummaryText;
        surface.ArchetypeCommitmentSummaryText = string.IsNullOrEmpty(context.ArchetypeCommitmentSummaryText) ? CurrentArchetypeCommitmentSummaryText : context.ArchetypeCommitmentSummaryText;
        surface.OfferCoherenceSummaryText = string.IsNullOrEmpty(context.OfferCoherenceSummaryText) ? string.Empty : context.OfferCoherenceSummaryText;
        surface.LaneRecoverySummaryText = string.IsNullOrEmpty(context.LaneRecoverySummaryText) ? CurrentLaneRecoverySummaryText : context.LaneRecoverySummaryText;
        surface.SoftRespecWindowSummaryText = string.IsNullOrEmpty(context.SoftRespecWindowSummaryText) ? CurrentSoftRespecWindowSummaryText : context.SoftRespecWindowSummaryText;
        surface.AntiTrapSafetySummaryText = string.IsNullOrEmpty(context.AntiTrapSafetySummaryText) ? CurrentAntiTrapSafetySummaryText : context.AntiTrapSafetySummaryText;
        surface.PartyArchetypeSummaryText = string.IsNullOrEmpty(context.PartyArchetypeSummaryText) ? CurrentPartyArchetypeSummaryText : context.PartyArchetypeSummaryText;
        surface.FormationCoherenceSummaryText = string.IsNullOrEmpty(context.FormationCoherenceSummaryText) ? CurrentFormationCoherenceSummaryText : context.FormationCoherenceSummaryText;
        surface.CommitmentStrengthSummaryText = string.IsNullOrEmpty(context.CommitmentStrengthSummaryText) ? CurrentArchetypeCommitmentStrengthSummaryText : context.CommitmentStrengthSummaryText;
        surface.FlexRescueSummaryText = string.IsNullOrEmpty(context.FlexRescueSummaryText) ? CurrentArchetypeFlexRescueSummaryText : context.FlexRescueSummaryText;
        surface.WeightedPrioritySummaryText = string.IsNullOrEmpty(surface.WeightedPrioritySummaryText) ? (string.IsNullOrEmpty(context.WeightedPrioritySummaryText) ? CurrentWeightedPrioritySummaryText : context.WeightedPrioritySummaryText) : surface.WeightedPrioritySummaryText;
        surface.RareSlotSummaryText = string.IsNullOrEmpty(surface.RareSlotSummaryText) ? (string.IsNullOrEmpty(context.RareSlotSummaryText) ? CurrentRareSlotSummaryText : context.RareSlotSummaryText) : surface.RareSlotSummaryText;
        surface.HighImpactSequencingSummaryText = string.IsNullOrEmpty(surface.HighImpactSequencingSummaryText) ? (string.IsNullOrEmpty(context.HighImpactSequencingSummaryText) ? CurrentHighImpactSequencingSummaryText : context.HighImpactSequencingSummaryText) : surface.HighImpactSequencingSummaryText;
        surface.RecoverySafeguardSummaryText = string.IsNullOrEmpty(surface.RecoverySafeguardSummaryText) ? (string.IsNullOrEmpty(context.RecoverySafeguardSummaryText) ? CurrentRecoverySafeguardSummaryText : context.RecoverySafeguardSummaryText) : surface.RecoverySafeguardSummaryText;
        surface.PartyCoverageSummaryText = string.IsNullOrEmpty(context.PartyCoverageSummaryText) ? CurrentPartyCoverageSummaryText : context.PartyCoverageSummaryText;
        surface.RoleOverlapWarningSummaryText = string.IsNullOrEmpty(context.RoleOverlapWarningSummaryText) ? CurrentRoleOverlapWarningSummaryText : context.RoleOverlapWarningSummaryText;
        surface.CrossMemberSynergySummaryText = string.IsNullOrEmpty(context.CrossMemberSynergySummaryText) ? CurrentCrossMemberSynergySummaryText : context.CrossMemberSynergySummaryText;
        if (!string.IsNullOrEmpty(surface.LaneRecoverySummaryText) && surface.OfferBasisSummaryText.IndexOf(surface.LaneRecoverySummaryText, System.StringComparison.Ordinal) < 0) { surface.OfferBasisSummaryText += " | " + surface.LaneRecoverySummaryText; }
        if (!string.IsNullOrEmpty(surface.SoftRespecWindowSummaryText) && surface.OfferBasisSummaryText.IndexOf(surface.SoftRespecWindowSummaryText, System.StringComparison.Ordinal) < 0) { surface.OfferBasisSummaryText += " | " + surface.SoftRespecWindowSummaryText; }
        if (!string.IsNullOrEmpty(surface.AntiTrapSafetySummaryText) && surface.OfferBasisSummaryText.IndexOf(surface.AntiTrapSafetySummaryText, System.StringComparison.Ordinal) < 0) { surface.OfferBasisSummaryText += " | " + surface.AntiTrapSafetySummaryText; }
        if (!string.IsNullOrEmpty(surface.PartyArchetypeSummaryText) && surface.OfferBasisSummaryText.IndexOf(surface.PartyArchetypeSummaryText, System.StringComparison.Ordinal) < 0) { surface.OfferBasisSummaryText += " | " + surface.PartyArchetypeSummaryText; }
        if (!string.IsNullOrEmpty(surface.FormationCoherenceSummaryText) && surface.OfferBasisSummaryText.IndexOf(surface.FormationCoherenceSummaryText, System.StringComparison.Ordinal) < 0) { surface.OfferBasisSummaryText += " | " + surface.FormationCoherenceSummaryText; }
        if (!string.IsNullOrEmpty(surface.CommitmentStrengthSummaryText) && surface.OfferBasisSummaryText.IndexOf(surface.CommitmentStrengthSummaryText, System.StringComparison.Ordinal) < 0) { surface.OfferBasisSummaryText += " | " + surface.CommitmentStrengthSummaryText; }
        if (!string.IsNullOrEmpty(surface.FlexRescueSummaryText) && surface.OfferBasisSummaryText.IndexOf(surface.FlexRescueSummaryText, System.StringComparison.Ordinal) < 0) { surface.OfferBasisSummaryText += " | " + surface.FlexRescueSummaryText; }
        if (!string.IsNullOrEmpty(surface.WeightedPrioritySummaryText) && surface.OfferBasisSummaryText.IndexOf(surface.WeightedPrioritySummaryText, System.StringComparison.Ordinal) < 0) { surface.OfferBasisSummaryText += " | " + surface.WeightedPrioritySummaryText; }
        if (!string.IsNullOrEmpty(surface.RareSlotSummaryText) && surface.OfferBasisSummaryText.IndexOf(surface.RareSlotSummaryText, System.StringComparison.Ordinal) < 0) { surface.OfferBasisSummaryText += " | " + surface.RareSlotSummaryText; }
        if (!string.IsNullOrEmpty(surface.HighImpactSequencingSummaryText) && surface.OfferBasisSummaryText.IndexOf(surface.HighImpactSequencingSummaryText, System.StringComparison.Ordinal) < 0) { surface.OfferBasisSummaryText += " | " + surface.HighImpactSequencingSummaryText; }
        if (!string.IsNullOrEmpty(surface.RecoverySafeguardSummaryText) && surface.OfferBasisSummaryText.IndexOf(surface.RecoverySafeguardSummaryText, System.StringComparison.Ordinal) < 0) { surface.OfferBasisSummaryText += " | " + surface.RecoverySafeguardSummaryText; }
        if (!string.IsNullOrEmpty(surface.StreakSensitivePacingSummaryText) && surface.OfferBasisSummaryText.IndexOf(surface.StreakSensitivePacingSummaryText, System.StringComparison.Ordinal) < 0) { surface.OfferBasisSummaryText += " | " + surface.StreakSensitivePacingSummaryText; }
        if (!string.IsNullOrEmpty(surface.ClearStreakDampeningSummaryText) && surface.OfferBasisSummaryText.IndexOf(surface.ClearStreakDampeningSummaryText, System.StringComparison.Ordinal) < 0) { surface.OfferBasisSummaryText += " | " + surface.ClearStreakDampeningSummaryText; }
        if (!string.IsNullOrEmpty(surface.ComebackReliefSummaryText) && surface.OfferBasisSummaryText.IndexOf(surface.ComebackReliefSummaryText, System.StringComparison.Ordinal) < 0) { surface.OfferBasisSummaryText += " | " + surface.ComebackReliefSummaryText; }
        if (!string.IsNullOrEmpty(surface.MomentumBalancingSummaryText) && surface.OfferBasisSummaryText.IndexOf(surface.MomentumBalancingSummaryText, System.StringComparison.Ordinal) < 0) { surface.OfferBasisSummaryText += " | " + surface.MomentumBalancingSummaryText; }
        if (!string.IsNullOrEmpty(surface.CurrentMomentumTierSummaryText) && surface.OfferBasisSummaryText.IndexOf(surface.CurrentMomentumTierSummaryText, System.StringComparison.Ordinal) < 0) { surface.OfferBasisSummaryText += " | " + surface.CurrentMomentumTierSummaryText; }
        if (!string.IsNullOrEmpty(surface.NextOfferIntensityHintText) && surface.OfferBasisSummaryText.IndexOf(surface.NextOfferIntensityHintText, System.StringComparison.Ordinal) < 0) { surface.OfferBasisSummaryText += " | " + surface.NextOfferIntensityHintText; }
        if (!string.IsNullOrEmpty(surface.PartyCoverageSummaryText) && surface.OfferBasisSummaryText.IndexOf(surface.PartyCoverageSummaryText, System.StringComparison.Ordinal) < 0) { surface.OfferBasisSummaryText += " | " + surface.PartyCoverageSummaryText; }
        if (!string.IsNullOrEmpty(surface.RoleOverlapWarningSummaryText) && surface.OfferBasisSummaryText.IndexOf(surface.RoleOverlapWarningSummaryText, System.StringComparison.Ordinal) < 0) { surface.OfferBasisSummaryText += " | " + surface.RoleOverlapWarningSummaryText; }
        if (!string.IsNullOrEmpty(surface.CrossMemberSynergySummaryText) && surface.OfferBasisSummaryText.IndexOf(surface.CrossMemberSynergySummaryText, System.StringComparison.Ordinal) < 0) { surface.OfferBasisSummaryText += " | " + surface.CrossMemberSynergySummaryText; }
        return surface;
    }


    private PrototypeRpgPostRunUpgradeOfferSurface RefreshRpgPostRunUpgradeOffers(PrototypeRpgRunResultSnapshot runResultSnapshot, PrototypeRpgCombatContributionSnapshot contributionSnapshot, PrototypeRpgProgressionPreviewSnapshot previewSnapshot)
    {
        PrototypeRpgGrowthChoiceContext context = ResolveRpgOfferGenerationInput(runResultSnapshot, contributionSnapshot, previewSnapshot);
        PrototypeRpgPostRunUpgradeOfferSurface surface = BuildRpgPostRunUpgradeOfferSurface(context);
        CommitRpgApplyReadyChoices(context, surface);
        ApplyRpgOfferSurfaceConsistency(runResultSnapshot, previewSnapshot, surface);
        return surface;
    }

    private void ApplyRpgOfferSurfaceConsistency(PrototypeRpgRunResultSnapshot runResultSnapshot, PrototypeRpgProgressionPreviewSnapshot previewSnapshot, PrototypeRpgPostRunUpgradeOfferSurface surface)
    {
        string refreshSummary = string.Empty;
        string continuitySummary = string.Empty;
        string nextBasisSummary = string.Empty;
        if (surface != null)
        {
            refreshSummary = string.IsNullOrEmpty(surface.OfferRefreshSummaryText) ? string.Empty : surface.OfferRefreshSummaryText;
            continuitySummary = string.IsNullOrEmpty(surface.OfferContinuitySummaryText) ? string.Empty : surface.OfferContinuitySummaryText;
            nextBasisSummary = string.IsNullOrEmpty(surface.OfferBasisSummaryText) ? string.Empty : surface.OfferBasisSummaryText;
            if (!string.IsNullOrEmpty(surface.OfferUnlockSummaryText) && (string.IsNullOrEmpty(refreshSummary) || refreshSummary.IndexOf(surface.OfferUnlockSummaryText, System.StringComparison.Ordinal) < 0)) { refreshSummary = string.IsNullOrEmpty(refreshSummary) ? surface.OfferUnlockSummaryText : refreshSummary + " | " + surface.OfferUnlockSummaryText; }
            if (!string.IsNullOrEmpty(surface.TierEscalationSummaryText) && (string.IsNullOrEmpty(refreshSummary) || refreshSummary.IndexOf(surface.TierEscalationSummaryText, System.StringComparison.Ordinal) < 0)) { refreshSummary = string.IsNullOrEmpty(refreshSummary) ? surface.TierEscalationSummaryText : refreshSummary + " | " + surface.TierEscalationSummaryText; }
            if (!string.IsNullOrEmpty(surface.LaneRecoverySummaryText) && (string.IsNullOrEmpty(refreshSummary) || refreshSummary.IndexOf(surface.LaneRecoverySummaryText, System.StringComparison.Ordinal) < 0)) { refreshSummary = string.IsNullOrEmpty(refreshSummary) ? surface.LaneRecoverySummaryText : refreshSummary + " | " + surface.LaneRecoverySummaryText; }
            if (!string.IsNullOrEmpty(surface.SoftRespecWindowSummaryText) && (string.IsNullOrEmpty(refreshSummary) || refreshSummary.IndexOf(surface.SoftRespecWindowSummaryText, System.StringComparison.Ordinal) < 0)) { refreshSummary = string.IsNullOrEmpty(refreshSummary) ? surface.SoftRespecWindowSummaryText : refreshSummary + " | " + surface.SoftRespecWindowSummaryText; }
            if (!string.IsNullOrEmpty(surface.PartyArchetypeSummaryText) && (string.IsNullOrEmpty(refreshSummary) || refreshSummary.IndexOf(surface.PartyArchetypeSummaryText, System.StringComparison.Ordinal) < 0)) { refreshSummary = string.IsNullOrEmpty(refreshSummary) ? surface.PartyArchetypeSummaryText : refreshSummary + " | " + surface.PartyArchetypeSummaryText; }
            if (!string.IsNullOrEmpty(surface.FormationCoherenceSummaryText) && (string.IsNullOrEmpty(refreshSummary) || refreshSummary.IndexOf(surface.FormationCoherenceSummaryText, System.StringComparison.Ordinal) < 0)) { refreshSummary = string.IsNullOrEmpty(refreshSummary) ? surface.FormationCoherenceSummaryText : refreshSummary + " | " + surface.FormationCoherenceSummaryText; }
            if (!string.IsNullOrEmpty(surface.CommitmentStrengthSummaryText) && (string.IsNullOrEmpty(refreshSummary) || refreshSummary.IndexOf(surface.CommitmentStrengthSummaryText, System.StringComparison.Ordinal) < 0)) { refreshSummary = string.IsNullOrEmpty(refreshSummary) ? surface.CommitmentStrengthSummaryText : refreshSummary + " | " + surface.CommitmentStrengthSummaryText; }
            if (!string.IsNullOrEmpty(surface.FlexRescueSummaryText) && (string.IsNullOrEmpty(refreshSummary) || refreshSummary.IndexOf(surface.FlexRescueSummaryText, System.StringComparison.Ordinal) < 0)) { refreshSummary = string.IsNullOrEmpty(refreshSummary) ? surface.FlexRescueSummaryText : refreshSummary + " | " + surface.FlexRescueSummaryText; }
            if (!string.IsNullOrEmpty(surface.WeightedPrioritySummaryText) && (string.IsNullOrEmpty(refreshSummary) || refreshSummary.IndexOf(surface.WeightedPrioritySummaryText, System.StringComparison.Ordinal) < 0)) { refreshSummary = string.IsNullOrEmpty(refreshSummary) ? surface.WeightedPrioritySummaryText : refreshSummary + " | " + surface.WeightedPrioritySummaryText; }
            if (!string.IsNullOrEmpty(surface.RareSlotSummaryText) && (string.IsNullOrEmpty(refreshSummary) || refreshSummary.IndexOf(surface.RareSlotSummaryText, System.StringComparison.Ordinal) < 0)) { refreshSummary = string.IsNullOrEmpty(refreshSummary) ? surface.RareSlotSummaryText : refreshSummary + " | " + surface.RareSlotSummaryText; }
            if (!string.IsNullOrEmpty(surface.HighImpactSequencingSummaryText) && (string.IsNullOrEmpty(refreshSummary) || refreshSummary.IndexOf(surface.HighImpactSequencingSummaryText, System.StringComparison.Ordinal) < 0)) { refreshSummary = string.IsNullOrEmpty(refreshSummary) ? surface.HighImpactSequencingSummaryText : refreshSummary + " | " + surface.HighImpactSequencingSummaryText; }
            if (!string.IsNullOrEmpty(surface.RecoverySafeguardSummaryText) && (string.IsNullOrEmpty(refreshSummary) || refreshSummary.IndexOf(surface.RecoverySafeguardSummaryText, System.StringComparison.Ordinal) < 0)) { refreshSummary = string.IsNullOrEmpty(refreshSummary) ? surface.RecoverySafeguardSummaryText : refreshSummary + " | " + surface.RecoverySafeguardSummaryText; }
            if (!string.IsNullOrEmpty(surface.PartyCoverageSummaryText) && (string.IsNullOrEmpty(refreshSummary) || refreshSummary.IndexOf(surface.PartyCoverageSummaryText, System.StringComparison.Ordinal) < 0)) { refreshSummary = string.IsNullOrEmpty(refreshSummary) ? surface.PartyCoverageSummaryText : refreshSummary + " | " + surface.PartyCoverageSummaryText; }
            if (!string.IsNullOrEmpty(surface.RoleOverlapWarningSummaryText) && (string.IsNullOrEmpty(refreshSummary) || refreshSummary.IndexOf(surface.RoleOverlapWarningSummaryText, System.StringComparison.Ordinal) < 0)) { refreshSummary = string.IsNullOrEmpty(refreshSummary) ? surface.RoleOverlapWarningSummaryText : refreshSummary + " | " + surface.RoleOverlapWarningSummaryText; }
            if (!string.IsNullOrEmpty(surface.CrossMemberSynergySummaryText) && (string.IsNullOrEmpty(refreshSummary) || refreshSummary.IndexOf(surface.CrossMemberSynergySummaryText, System.StringComparison.Ordinal) < 0)) { refreshSummary = string.IsNullOrEmpty(refreshSummary) ? surface.CrossMemberSynergySummaryText : refreshSummary + " | " + surface.CrossMemberSynergySummaryText; }
            if (!string.IsNullOrEmpty(surface.StreakSensitivePacingSummaryText) && (string.IsNullOrEmpty(refreshSummary) || refreshSummary.IndexOf(surface.StreakSensitivePacingSummaryText, System.StringComparison.Ordinal) < 0)) { refreshSummary = string.IsNullOrEmpty(refreshSummary) ? surface.StreakSensitivePacingSummaryText : refreshSummary + " | " + surface.StreakSensitivePacingSummaryText; }
            if (!string.IsNullOrEmpty(surface.ClearStreakDampeningSummaryText) && (string.IsNullOrEmpty(refreshSummary) || refreshSummary.IndexOf(surface.ClearStreakDampeningSummaryText, System.StringComparison.Ordinal) < 0)) { refreshSummary = string.IsNullOrEmpty(refreshSummary) ? surface.ClearStreakDampeningSummaryText : refreshSummary + " | " + surface.ClearStreakDampeningSummaryText; }
            if (!string.IsNullOrEmpty(surface.ComebackReliefSummaryText) && (string.IsNullOrEmpty(refreshSummary) || refreshSummary.IndexOf(surface.ComebackReliefSummaryText, System.StringComparison.Ordinal) < 0)) { refreshSummary = string.IsNullOrEmpty(refreshSummary) ? surface.ComebackReliefSummaryText : refreshSummary + " | " + surface.ComebackReliefSummaryText; }
            if (!string.IsNullOrEmpty(surface.MomentumBalancingSummaryText) && (string.IsNullOrEmpty(refreshSummary) || refreshSummary.IndexOf(surface.MomentumBalancingSummaryText, System.StringComparison.Ordinal) < 0)) { refreshSummary = string.IsNullOrEmpty(refreshSummary) ? surface.MomentumBalancingSummaryText : refreshSummary + " | " + surface.MomentumBalancingSummaryText; }
            if (!string.IsNullOrEmpty(surface.CurrentMomentumTierSummaryText) && (string.IsNullOrEmpty(refreshSummary) || refreshSummary.IndexOf(surface.CurrentMomentumTierSummaryText, System.StringComparison.Ordinal) < 0)) { refreshSummary = string.IsNullOrEmpty(refreshSummary) ? surface.CurrentMomentumTierSummaryText : refreshSummary + " | " + surface.CurrentMomentumTierSummaryText; }
            if (!string.IsNullOrEmpty(surface.NextOfferIntensityHintText) && (string.IsNullOrEmpty(refreshSummary) || refreshSummary.IndexOf(surface.NextOfferIntensityHintText, System.StringComparison.Ordinal) < 0)) { refreshSummary = string.IsNullOrEmpty(refreshSummary) ? surface.NextOfferIntensityHintText : refreshSummary + " | " + surface.NextOfferIntensityHintText; }
            if (!string.IsNullOrEmpty(surface.PoolExpansionSummaryText) && (string.IsNullOrEmpty(continuitySummary) || continuitySummary.IndexOf(surface.PoolExpansionSummaryText, System.StringComparison.Ordinal) < 0)) { continuitySummary = string.IsNullOrEmpty(continuitySummary) ? surface.PoolExpansionSummaryText : continuitySummary + " | " + surface.PoolExpansionSummaryText; }
            if (!string.IsNullOrEmpty(surface.ArchetypeCommitmentSummaryText) && (string.IsNullOrEmpty(continuitySummary) || continuitySummary.IndexOf(surface.ArchetypeCommitmentSummaryText, System.StringComparison.Ordinal) < 0)) { continuitySummary = string.IsNullOrEmpty(continuitySummary) ? surface.ArchetypeCommitmentSummaryText : continuitySummary + " | " + surface.ArchetypeCommitmentSummaryText; }
            if (!string.IsNullOrEmpty(surface.PartyArchetypeSummaryText) && (string.IsNullOrEmpty(continuitySummary) || continuitySummary.IndexOf(surface.PartyArchetypeSummaryText, System.StringComparison.Ordinal) < 0)) { continuitySummary = string.IsNullOrEmpty(continuitySummary) ? surface.PartyArchetypeSummaryText : continuitySummary + " | " + surface.PartyArchetypeSummaryText; }
            if (!string.IsNullOrEmpty(surface.FormationCoherenceSummaryText) && (string.IsNullOrEmpty(continuitySummary) || continuitySummary.IndexOf(surface.FormationCoherenceSummaryText, System.StringComparison.Ordinal) < 0)) { continuitySummary = string.IsNullOrEmpty(continuitySummary) ? surface.FormationCoherenceSummaryText : continuitySummary + " | " + surface.FormationCoherenceSummaryText; }
            if (!string.IsNullOrEmpty(surface.CommitmentStrengthSummaryText) && (string.IsNullOrEmpty(continuitySummary) || continuitySummary.IndexOf(surface.CommitmentStrengthSummaryText, System.StringComparison.Ordinal) < 0)) { continuitySummary = string.IsNullOrEmpty(continuitySummary) ? surface.CommitmentStrengthSummaryText : continuitySummary + " | " + surface.CommitmentStrengthSummaryText; }
            if (!string.IsNullOrEmpty(surface.FlexRescueSummaryText) && (string.IsNullOrEmpty(continuitySummary) || continuitySummary.IndexOf(surface.FlexRescueSummaryText, System.StringComparison.Ordinal) < 0)) { continuitySummary = string.IsNullOrEmpty(continuitySummary) ? surface.FlexRescueSummaryText : continuitySummary + " | " + surface.FlexRescueSummaryText; }
            if (!string.IsNullOrEmpty(surface.WeightedPrioritySummaryText) && (string.IsNullOrEmpty(continuitySummary) || continuitySummary.IndexOf(surface.WeightedPrioritySummaryText, System.StringComparison.Ordinal) < 0)) { continuitySummary = string.IsNullOrEmpty(continuitySummary) ? surface.WeightedPrioritySummaryText : continuitySummary + " | " + surface.WeightedPrioritySummaryText; }
            if (!string.IsNullOrEmpty(surface.RareSlotSummaryText) && (string.IsNullOrEmpty(continuitySummary) || continuitySummary.IndexOf(surface.RareSlotSummaryText, System.StringComparison.Ordinal) < 0)) { continuitySummary = string.IsNullOrEmpty(continuitySummary) ? surface.RareSlotSummaryText : continuitySummary + " | " + surface.RareSlotSummaryText; }
            if (!string.IsNullOrEmpty(surface.HighImpactSequencingSummaryText) && (string.IsNullOrEmpty(continuitySummary) || continuitySummary.IndexOf(surface.HighImpactSequencingSummaryText, System.StringComparison.Ordinal) < 0)) { continuitySummary = string.IsNullOrEmpty(continuitySummary) ? surface.HighImpactSequencingSummaryText : continuitySummary + " | " + surface.HighImpactSequencingSummaryText; }
            if (!string.IsNullOrEmpty(surface.RecoverySafeguardSummaryText) && (string.IsNullOrEmpty(continuitySummary) || continuitySummary.IndexOf(surface.RecoverySafeguardSummaryText, System.StringComparison.Ordinal) < 0)) { continuitySummary = string.IsNullOrEmpty(continuitySummary) ? surface.RecoverySafeguardSummaryText : continuitySummary + " | " + surface.RecoverySafeguardSummaryText; }
            if (!string.IsNullOrEmpty(surface.AntiTrapSafetySummaryText) && (string.IsNullOrEmpty(continuitySummary) || continuitySummary.IndexOf(surface.AntiTrapSafetySummaryText, System.StringComparison.Ordinal) < 0)) { continuitySummary = string.IsNullOrEmpty(continuitySummary) ? surface.AntiTrapSafetySummaryText : continuitySummary + " | " + surface.AntiTrapSafetySummaryText; }
            if (!string.IsNullOrEmpty(surface.RoleOverlapWarningSummaryText) && (string.IsNullOrEmpty(continuitySummary) || continuitySummary.IndexOf(surface.RoleOverlapWarningSummaryText, System.StringComparison.Ordinal) < 0)) { continuitySummary = string.IsNullOrEmpty(continuitySummary) ? surface.RoleOverlapWarningSummaryText : continuitySummary + " | " + surface.RoleOverlapWarningSummaryText; }
            if (!string.IsNullOrEmpty(surface.CrossMemberSynergySummaryText) && (string.IsNullOrEmpty(continuitySummary) || continuitySummary.IndexOf(surface.CrossMemberSynergySummaryText, System.StringComparison.Ordinal) < 0)) { continuitySummary = string.IsNullOrEmpty(continuitySummary) ? surface.CrossMemberSynergySummaryText : continuitySummary + " | " + surface.CrossMemberSynergySummaryText; }
            if (!string.IsNullOrEmpty(surface.StreakSensitivePacingSummaryText) && (string.IsNullOrEmpty(continuitySummary) || continuitySummary.IndexOf(surface.StreakSensitivePacingSummaryText, System.StringComparison.Ordinal) < 0)) { continuitySummary = string.IsNullOrEmpty(continuitySummary) ? surface.StreakSensitivePacingSummaryText : continuitySummary + " | " + surface.StreakSensitivePacingSummaryText; }
            if (!string.IsNullOrEmpty(surface.ClearStreakDampeningSummaryText) && (string.IsNullOrEmpty(continuitySummary) || continuitySummary.IndexOf(surface.ClearStreakDampeningSummaryText, System.StringComparison.Ordinal) < 0)) { continuitySummary = string.IsNullOrEmpty(continuitySummary) ? surface.ClearStreakDampeningSummaryText : continuitySummary + " | " + surface.ClearStreakDampeningSummaryText; }
            if (!string.IsNullOrEmpty(surface.ComebackReliefSummaryText) && (string.IsNullOrEmpty(continuitySummary) || continuitySummary.IndexOf(surface.ComebackReliefSummaryText, System.StringComparison.Ordinal) < 0)) { continuitySummary = string.IsNullOrEmpty(continuitySummary) ? surface.ComebackReliefSummaryText : continuitySummary + " | " + surface.ComebackReliefSummaryText; }
            if (!string.IsNullOrEmpty(surface.MomentumBalancingSummaryText) && (string.IsNullOrEmpty(continuitySummary) || continuitySummary.IndexOf(surface.MomentumBalancingSummaryText, System.StringComparison.Ordinal) < 0)) { continuitySummary = string.IsNullOrEmpty(continuitySummary) ? surface.MomentumBalancingSummaryText : continuitySummary + " | " + surface.MomentumBalancingSummaryText; }
            if (!string.IsNullOrEmpty(surface.CurrentMomentumTierSummaryText) && (string.IsNullOrEmpty(continuitySummary) || continuitySummary.IndexOf(surface.CurrentMomentumTierSummaryText, System.StringComparison.Ordinal) < 0)) { continuitySummary = string.IsNullOrEmpty(continuitySummary) ? surface.CurrentMomentumTierSummaryText : continuitySummary + " | " + surface.CurrentMomentumTierSummaryText; }
            if (!string.IsNullOrEmpty(surface.NextOfferIntensityHintText) && (string.IsNullOrEmpty(continuitySummary) || continuitySummary.IndexOf(surface.NextOfferIntensityHintText, System.StringComparison.Ordinal) < 0)) { continuitySummary = string.IsNullOrEmpty(continuitySummary) ? surface.NextOfferIntensityHintText : continuitySummary + " | " + surface.NextOfferIntensityHintText; }
            if (!string.IsNullOrEmpty(surface.OfferUnlockSummaryText) && (string.IsNullOrEmpty(nextBasisSummary) || nextBasisSummary.IndexOf(surface.OfferUnlockSummaryText, System.StringComparison.Ordinal) < 0)) { nextBasisSummary = string.IsNullOrEmpty(nextBasisSummary) ? surface.OfferUnlockSummaryText : nextBasisSummary + " | " + surface.OfferUnlockSummaryText; }
            if (!string.IsNullOrEmpty(surface.BuildLaneSummaryText) && (string.IsNullOrEmpty(nextBasisSummary) || nextBasisSummary.IndexOf(surface.BuildLaneSummaryText, System.StringComparison.Ordinal) < 0)) { nextBasisSummary = string.IsNullOrEmpty(nextBasisSummary) ? surface.BuildLaneSummaryText : nextBasisSummary + " | " + surface.BuildLaneSummaryText; }
            if (!string.IsNullOrEmpty(surface.PartyArchetypeSummaryText) && (string.IsNullOrEmpty(nextBasisSummary) || nextBasisSummary.IndexOf(surface.PartyArchetypeSummaryText, System.StringComparison.Ordinal) < 0)) { nextBasisSummary = string.IsNullOrEmpty(nextBasisSummary) ? surface.PartyArchetypeSummaryText : nextBasisSummary + " | " + surface.PartyArchetypeSummaryText; }
            if (!string.IsNullOrEmpty(surface.FormationCoherenceSummaryText) && (string.IsNullOrEmpty(nextBasisSummary) || nextBasisSummary.IndexOf(surface.FormationCoherenceSummaryText, System.StringComparison.Ordinal) < 0)) { nextBasisSummary = string.IsNullOrEmpty(nextBasisSummary) ? surface.FormationCoherenceSummaryText : nextBasisSummary + " | " + surface.FormationCoherenceSummaryText; }
            if (!string.IsNullOrEmpty(surface.CommitmentStrengthSummaryText) && (string.IsNullOrEmpty(nextBasisSummary) || nextBasisSummary.IndexOf(surface.CommitmentStrengthSummaryText, System.StringComparison.Ordinal) < 0)) { nextBasisSummary = string.IsNullOrEmpty(nextBasisSummary) ? surface.CommitmentStrengthSummaryText : nextBasisSummary + " | " + surface.CommitmentStrengthSummaryText; }
            if (!string.IsNullOrEmpty(surface.FlexRescueSummaryText) && (string.IsNullOrEmpty(nextBasisSummary) || nextBasisSummary.IndexOf(surface.FlexRescueSummaryText, System.StringComparison.Ordinal) < 0)) { nextBasisSummary = string.IsNullOrEmpty(nextBasisSummary) ? surface.FlexRescueSummaryText : nextBasisSummary + " | " + surface.FlexRescueSummaryText; }
            if (!string.IsNullOrEmpty(surface.WeightedPrioritySummaryText) && (string.IsNullOrEmpty(nextBasisSummary) || nextBasisSummary.IndexOf(surface.WeightedPrioritySummaryText, System.StringComparison.Ordinal) < 0)) { nextBasisSummary = string.IsNullOrEmpty(nextBasisSummary) ? surface.WeightedPrioritySummaryText : nextBasisSummary + " | " + surface.WeightedPrioritySummaryText; }
            if (!string.IsNullOrEmpty(surface.RareSlotSummaryText) && (string.IsNullOrEmpty(nextBasisSummary) || nextBasisSummary.IndexOf(surface.RareSlotSummaryText, System.StringComparison.Ordinal) < 0)) { nextBasisSummary = string.IsNullOrEmpty(nextBasisSummary) ? surface.RareSlotSummaryText : nextBasisSummary + " | " + surface.RareSlotSummaryText; }
            if (!string.IsNullOrEmpty(surface.HighImpactSequencingSummaryText) && (string.IsNullOrEmpty(nextBasisSummary) || nextBasisSummary.IndexOf(surface.HighImpactSequencingSummaryText, System.StringComparison.Ordinal) < 0)) { nextBasisSummary = string.IsNullOrEmpty(nextBasisSummary) ? surface.HighImpactSequencingSummaryText : nextBasisSummary + " | " + surface.HighImpactSequencingSummaryText; }
            if (!string.IsNullOrEmpty(surface.RecoverySafeguardSummaryText) && (string.IsNullOrEmpty(nextBasisSummary) || nextBasisSummary.IndexOf(surface.RecoverySafeguardSummaryText, System.StringComparison.Ordinal) < 0)) { nextBasisSummary = string.IsNullOrEmpty(nextBasisSummary) ? surface.RecoverySafeguardSummaryText : nextBasisSummary + " | " + surface.RecoverySafeguardSummaryText; }
            if (!string.IsNullOrEmpty(surface.LaneRecoverySummaryText) && (string.IsNullOrEmpty(nextBasisSummary) || nextBasisSummary.IndexOf(surface.LaneRecoverySummaryText, System.StringComparison.Ordinal) < 0)) { nextBasisSummary = string.IsNullOrEmpty(nextBasisSummary) ? surface.LaneRecoverySummaryText : nextBasisSummary + " | " + surface.LaneRecoverySummaryText; }
            if (!string.IsNullOrEmpty(surface.PartyCoverageSummaryText) && (string.IsNullOrEmpty(nextBasisSummary) || nextBasisSummary.IndexOf(surface.PartyCoverageSummaryText, System.StringComparison.Ordinal) < 0)) { nextBasisSummary = string.IsNullOrEmpty(nextBasisSummary) ? surface.PartyCoverageSummaryText : nextBasisSummary + " | " + surface.PartyCoverageSummaryText; }
            if (!string.IsNullOrEmpty(surface.CrossMemberSynergySummaryText) && (string.IsNullOrEmpty(nextBasisSummary) || nextBasisSummary.IndexOf(surface.CrossMemberSynergySummaryText, System.StringComparison.Ordinal) < 0)) { nextBasisSummary = string.IsNullOrEmpty(nextBasisSummary) ? surface.CrossMemberSynergySummaryText : nextBasisSummary + " | " + surface.CrossMemberSynergySummaryText; }
            if (!string.IsNullOrEmpty(surface.StreakSensitivePacingSummaryText) && (string.IsNullOrEmpty(nextBasisSummary) || nextBasisSummary.IndexOf(surface.StreakSensitivePacingSummaryText, System.StringComparison.Ordinal) < 0)) { nextBasisSummary = string.IsNullOrEmpty(nextBasisSummary) ? surface.StreakSensitivePacingSummaryText : nextBasisSummary + " | " + surface.StreakSensitivePacingSummaryText; }
            if (!string.IsNullOrEmpty(surface.ClearStreakDampeningSummaryText) && (string.IsNullOrEmpty(nextBasisSummary) || nextBasisSummary.IndexOf(surface.ClearStreakDampeningSummaryText, System.StringComparison.Ordinal) < 0)) { nextBasisSummary = string.IsNullOrEmpty(nextBasisSummary) ? surface.ClearStreakDampeningSummaryText : nextBasisSummary + " | " + surface.ClearStreakDampeningSummaryText; }
            if (!string.IsNullOrEmpty(surface.ComebackReliefSummaryText) && (string.IsNullOrEmpty(nextBasisSummary) || nextBasisSummary.IndexOf(surface.ComebackReliefSummaryText, System.StringComparison.Ordinal) < 0)) { nextBasisSummary = string.IsNullOrEmpty(nextBasisSummary) ? surface.ComebackReliefSummaryText : nextBasisSummary + " | " + surface.ComebackReliefSummaryText; }
            if (!string.IsNullOrEmpty(surface.MomentumBalancingSummaryText) && (string.IsNullOrEmpty(nextBasisSummary) || nextBasisSummary.IndexOf(surface.MomentumBalancingSummaryText, System.StringComparison.Ordinal) < 0)) { nextBasisSummary = string.IsNullOrEmpty(nextBasisSummary) ? surface.MomentumBalancingSummaryText : nextBasisSummary + " | " + surface.MomentumBalancingSummaryText; }
            if (!string.IsNullOrEmpty(surface.CurrentMomentumTierSummaryText) && (string.IsNullOrEmpty(nextBasisSummary) || nextBasisSummary.IndexOf(surface.CurrentMomentumTierSummaryText, System.StringComparison.Ordinal) < 0)) { nextBasisSummary = string.IsNullOrEmpty(nextBasisSummary) ? surface.CurrentMomentumTierSummaryText : nextBasisSummary + " | " + surface.CurrentMomentumTierSummaryText; }
            if (!string.IsNullOrEmpty(surface.NextOfferIntensityHintText) && (string.IsNullOrEmpty(nextBasisSummary) || nextBasisSummary.IndexOf(surface.NextOfferIntensityHintText, System.StringComparison.Ordinal) < 0)) { nextBasisSummary = string.IsNullOrEmpty(nextBasisSummary) ? surface.NextOfferIntensityHintText : nextBasisSummary + " | " + surface.NextOfferIntensityHintText; }
        }
        if (runResultSnapshot != null && surface != null)
        {
            runResultSnapshot.CurrentAppliedIdentitySummaryText = string.IsNullOrEmpty(surface.AppliedPartySummaryText) ? string.Empty : surface.AppliedPartySummaryText;
            runResultSnapshot.OfferRefreshSummaryText = refreshSummary;
            runResultSnapshot.ApplyReadySummaryText = string.IsNullOrEmpty(surface.ApplyReadySummaryText) ? string.Empty : surface.ApplyReadySummaryText;
            if (!string.IsNullOrEmpty(runResultSnapshot.LevelUpApplyReadySummaryText) && (string.IsNullOrEmpty(runResultSnapshot.ApplyReadySummaryText) || runResultSnapshot.ApplyReadySummaryText.IndexOf(runResultSnapshot.LevelUpApplyReadySummaryText, System.StringComparison.Ordinal) < 0)) { runResultSnapshot.ApplyReadySummaryText = string.IsNullOrEmpty(runResultSnapshot.ApplyReadySummaryText) ? runResultSnapshot.LevelUpApplyReadySummaryText : runResultSnapshot.ApplyReadySummaryText + " | " + runResultSnapshot.LevelUpApplyReadySummaryText; }
            runResultSnapshot.OfferContinuitySummaryText = continuitySummary;
            if (!string.IsNullOrEmpty(surface.ApplyReadySummaryText))
            {
                runResultSnapshot.ApplyTraceSummaryText = surface.ApplyReadySummaryText;
            }
            if (!string.IsNullOrEmpty(nextBasisSummary))
            {
                runResultSnapshot.NextOfferBasisSummaryText = nextBasisSummary;
            }
            if (runResultSnapshot.LootOutcome != null)
            {
                string baseSummary = string.IsNullOrEmpty(runResultSnapshot.LootOutcome.RewardGrantSummaryText) ? runResultSnapshot.LootOutcome.FinalLootSummary : runResultSnapshot.LootOutcome.RewardGrantSummaryText;
                if (!string.IsNullOrEmpty(surface.ApplyReadySummaryText))
                {
                    runResultSnapshot.LootOutcome.FinalLootSummary = string.IsNullOrEmpty(baseSummary) ? surface.ApplyReadySummaryText : baseSummary + " | " + surface.ApplyReadySummaryText;
                }
            }
        }

        if (previewSnapshot == null || surface == null)
        {
            return;
        }

        previewSnapshot.CurrentAppliedIdentitySummaryText = string.IsNullOrEmpty(surface.AppliedPartySummaryText) ? string.Empty : surface.AppliedPartySummaryText;
        previewSnapshot.OfferRefreshSummaryText = refreshSummary;
        previewSnapshot.ApplyReadySummaryText = string.IsNullOrEmpty(surface.ApplyReadySummaryText) ? string.Empty : surface.ApplyReadySummaryText;
        if (!string.IsNullOrEmpty(runResultSnapshot.LevelUpApplyReadySummaryText) && (string.IsNullOrEmpty(previewSnapshot.ApplyReadySummaryText) || previewSnapshot.ApplyReadySummaryText.IndexOf(runResultSnapshot.LevelUpApplyReadySummaryText, System.StringComparison.Ordinal) < 0)) { previewSnapshot.ApplyReadySummaryText = string.IsNullOrEmpty(previewSnapshot.ApplyReadySummaryText) ? runResultSnapshot.LevelUpApplyReadySummaryText : previewSnapshot.ApplyReadySummaryText + " | " + runResultSnapshot.LevelUpApplyReadySummaryText; }
        previewSnapshot.OfferContinuitySummaryText = continuitySummary;
        if (!string.IsNullOrEmpty(surface.ApplyReadySummaryText))
        {
            previewSnapshot.ApplyTraceSummaryText = surface.ApplyReadySummaryText;
        }
        if (!string.IsNullOrEmpty(nextBasisSummary))
        {
            previewSnapshot.NextOfferBasisSummaryText = nextBasisSummary;
            previewSnapshot.PreviewSummaryText = BuildRpgPostRunHeadlineText(runResultSnapshot) + " | " + nextBasisSummary;
        }

        PrototypeRpgMemberProgressPreview[] members = previewSnapshot.Members ?? System.Array.Empty<PrototypeRpgMemberProgressPreview>();
        for (int i = 0; i < members.Length; i++)
        {
            PrototypeRpgMemberProgressPreview member = members[i];
            if (member == null)
            {
                continue;
            }

            PrototypeRpgApplyReadyChoice choice = FindRpgApplyReadyChoice(surface, member.MemberId);
            PrototypeRpgUpgradeOfferCandidate offer = FindRpgUpgradeOfferCandidate(surface, member.MemberId);
            PrototypeRpgPartyDefinition partyDefinition = _activeDungeonParty != null ? _activeDungeonParty.PartyDefinition : null;
            PrototypeRpgPartyMemberDefinition memberDefinition = FindRpgPartyMemberDefinition(partyDefinition, member.MemberId);
            PrototypeRpgAppliedPartyMemberProgressState appliedMemberState = GetRpgAppliedPartyMemberProgressState(_sessionAppliedRpgPartyProgressState, member.MemberId);
            member.CurrentAppliedIdentitySummaryText = BuildRpgAppliedMemberIdentitySummaryText(memberDefinition, appliedMemberState);
            member.NextOfferSummaryText = offer != null && !string.IsNullOrEmpty(offer.SummaryText) ? offer.SummaryText : string.Empty;
            member.ApplyReadySummaryText = choice != null && !string.IsNullOrEmpty(choice.AppliedSummaryText) ? choice.AppliedSummaryText : string.Empty;
            string offerReasonText = string.Empty;
            if (offer != null)
            {
                if (!string.IsNullOrEmpty(offer.ReasonText))
                {
                    offerReasonText = offer.ReasonText;
                }
                if (!string.IsNullOrEmpty(offer.RotationReasonText))
                {
                    offerReasonText = string.IsNullOrEmpty(offerReasonText) ? offer.RotationReasonText : offerReasonText + " | " + offer.RotationReasonText;
                }
                if (!string.IsNullOrEmpty(offer.UnlockReasonText))
                {
                    offerReasonText = string.IsNullOrEmpty(offerReasonText) ? offer.UnlockReasonText : offerReasonText + " | " + offer.UnlockReasonText;
                }
                if (!string.IsNullOrEmpty(offer.TierEscalationReasonText))
                {
                    offerReasonText = string.IsNullOrEmpty(offerReasonText) ? offer.TierEscalationReasonText : offerReasonText + " | " + offer.TierEscalationReasonText;
                }
                if (!string.IsNullOrEmpty(offer.PoolExhaustionReasonText))
                {
                    offerReasonText = string.IsNullOrEmpty(offerReasonText) ? offer.PoolExhaustionReasonText : offerReasonText + " | " + offer.PoolExhaustionReasonText;
                }
                if (!string.IsNullOrEmpty(offer.LaneCoherenceReasonText))
                {
                    offerReasonText = string.IsNullOrEmpty(offerReasonText) ? offer.LaneCoherenceReasonText : offerReasonText + " | " + offer.LaneCoherenceReasonText;
                }
                if (!string.IsNullOrEmpty(offer.RecoveryReasonText))
                {
                    offerReasonText = string.IsNullOrEmpty(offerReasonText) ? offer.RecoveryReasonText : offerReasonText + " | " + offer.RecoveryReasonText;
                }
                if (!string.IsNullOrEmpty(offer.ArchetypeReasonText))
                {
                    offerReasonText = string.IsNullOrEmpty(offerReasonText) ? offer.ArchetypeReasonText : offerReasonText + " | " + offer.ArchetypeReasonText;
                }
                if (!string.IsNullOrEmpty(offer.FormationReasonText))
                {
                    offerReasonText = string.IsNullOrEmpty(offerReasonText) ? offer.FormationReasonText : offerReasonText + " | " + offer.FormationReasonText;
                }
                if (!string.IsNullOrEmpty(offer.TopPickReasonText))
                {
                    offerReasonText = string.IsNullOrEmpty(offerReasonText) ? offer.TopPickReasonText : offerReasonText + " | " + offer.TopPickReasonText;
                }
                if (!string.IsNullOrEmpty(offer.RareSlotReasonText))
                {
                    offerReasonText = string.IsNullOrEmpty(offerReasonText) ? offer.RareSlotReasonText : offerReasonText + " | " + offer.RareSlotReasonText;
                }
                if (!string.IsNullOrEmpty(offer.RecoverySafeguardReasonText))
                {
                    offerReasonText = string.IsNullOrEmpty(offerReasonText) ? offer.RecoverySafeguardReasonText : offerReasonText + " | " + offer.RecoverySafeguardReasonText;
                }
                if (!string.IsNullOrEmpty(offer.MomentumReasonText))
                {
                    offerReasonText = string.IsNullOrEmpty(offerReasonText) ? offer.MomentumReasonText : offerReasonText + " | " + offer.MomentumReasonText;
                }
                if (!string.IsNullOrEmpty(offer.IntensityHintText))
                {
                    offerReasonText = string.IsNullOrEmpty(offerReasonText) ? offer.IntensityHintText : offerReasonText + " | " + offer.IntensityHintText;
                }
            }

            member.OfferReasonText = offerReasonText;
            member.OfferContinuitySummaryText = offer != null && !string.IsNullOrEmpty(offer.ContinuitySummaryText) ? offer.ContinuitySummaryText : string.Empty;
            member.PreviewSummaryText = !string.IsNullOrEmpty(member.ApplyReadySummaryText)
                ? member.ContributionSummaryText + " | " + member.ApplyReadySummaryText + (string.IsNullOrEmpty(member.CurrentAppliedIdentitySummaryText) ? string.Empty : " | " + member.CurrentAppliedIdentitySummaryText)
                : (!string.IsNullOrEmpty(member.NextOfferSummaryText) && !string.IsNullOrEmpty(member.OfferReasonText)
                    ? member.ContributionSummaryText + " | " + member.NextOfferSummaryText + " | " + member.OfferReasonText + (string.IsNullOrEmpty(member.CurrentAppliedIdentitySummaryText) ? string.Empty : " | " + member.CurrentAppliedIdentitySummaryText)
                    : BuildRpgMemberPreviewSummaryText(member));
        }
    }

    private PrototypeRpgUpgradeOfferCandidate FindRpgUpgradeOfferCandidate
(PrototypeRpgPostRunUpgradeOfferSurface surface, string memberId)
    {
        if (surface == null || surface.Offers == null || string.IsNullOrEmpty(memberId))
        {
            return null;
        }

        for (int i = 0; i < surface.Offers.Length; i++)
        {
            PrototypeRpgUpgradeOfferCandidate offer = surface.Offers[i];
            if (offer != null && offer.MemberId == memberId)
            {
                return offer;
            }
        }

        return null;
    }

    private PrototypeRpgApplyReadyChoice FindRpgApplyReadyChoice(PrototypeRpgPostRunUpgradeOfferSurface surface, string memberId)
    {
        if (surface == null || surface.ApplyReadyChoices == null || string.IsNullOrEmpty(memberId))
        {
            return null;
        }

        for (int i = 0; i < surface.ApplyReadyChoices.Length; i++)
        {
            PrototypeRpgApplyReadyChoice choice = surface.ApplyReadyChoices[i];
            if (choice != null && choice.MemberId == memberId)
            {
                return choice;
            }
        }

        return null;
    }

    private PrototypeRpgPartyMemberDefinition FindRpgPartyMemberDefinition(PrototypeRpgPartyDefinition partyDefinition, string memberId)
    {
        if (partyDefinition == null || partyDefinition.Members == null || string.IsNullOrEmpty(memberId))
        {
            return null;
        }

        for (int i = 0; i < partyDefinition.Members.Length; i++)
        {
            PrototypeRpgPartyMemberDefinition member = partyDefinition.Members[i];
            if (member != null && member.MemberId == memberId)
            {
                return member;
            }
        }

        return null;
    }
}













































