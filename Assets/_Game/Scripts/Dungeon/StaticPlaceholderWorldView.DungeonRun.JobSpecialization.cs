using System;
using System.Collections.Generic;

public sealed partial class StaticPlaceholderWorldView
{
    private PrototypeRpgJobUnlockRuntimeState _sessionRpgJobUnlockRuntimeState = new PrototypeRpgJobUnlockRuntimeState();

    public string CurrentJobSpecializationSummaryText => string.IsNullOrEmpty(_sessionRpgJobUnlockRuntimeState.JobSpecializationSummaryText) ? "None" : _sessionRpgJobUnlockRuntimeState.JobSpecializationSummaryText;
    public string CurrentPassiveSkillSlotSummaryText => string.IsNullOrEmpty(_sessionRpgJobUnlockRuntimeState.PassiveSkillSlotSummaryText) ? "None" : _sessionRpgJobUnlockRuntimeState.PassiveSkillSlotSummaryText;
    public string CurrentRuntimeSynergySummaryText => string.IsNullOrEmpty(_sessionRpgJobUnlockRuntimeState.RuntimeSynergySummaryText) ? "None" : _sessionRpgJobUnlockRuntimeState.RuntimeSynergySummaryText;
    public string CurrentNextRunSpecializationPreviewText => string.IsNullOrEmpty(_sessionRpgJobUnlockRuntimeState.NextRunSpecializationPreviewText) ? "None" : _sessionRpgJobUnlockRuntimeState.NextRunSpecializationPreviewText;
    public PrototypeRpgJobUnlockRuntimeState LatestRpgJobUnlockRuntimeState => CopyRpgJobUnlockRuntimeState(_sessionRpgJobUnlockRuntimeState);

    private void ResetRpgJobSpecializationRuntimeState()
    {
        _sessionRpgJobUnlockRuntimeState = new PrototypeRpgJobUnlockRuntimeState();
    }

    private PrototypeRpgJobUnlockRuntimeState RefreshRpgJobSpecializationRuntimeState(
        PrototypeRpgPartyDefinition partyDefinition,
        PrototypeRpgAppliedPartyProgressState appliedState,
        PrototypeRpgPartyLevelRuntimeState partyLevelState,
        PrototypeRpgRunResultSnapshot runResultSnapshot)
    {
        if (partyDefinition == null)
        {
            ResetRpgJobSpecializationRuntimeState();
            return LatestRpgJobUnlockRuntimeState;
        }

        _sessionRpgJobUnlockRuntimeState.PartyId = string.IsNullOrEmpty(partyDefinition.PartyId) ? string.Empty : partyDefinition.PartyId;
        _sessionRpgJobUnlockRuntimeState.DisplayName = string.IsNullOrEmpty(partyDefinition.DisplayName) ? _sessionRpgJobUnlockRuntimeState.PartyId : partyDefinition.DisplayName;
        _sessionRpgJobUnlockRuntimeState.LastRunIdentity = runResultSnapshot != null ? BuildRpgOfferGenerationRunIdentity(runResultSnapshot) : string.Empty;

        PrototypeRpgPartyMemberDefinition[] members = partyDefinition.Members ?? Array.Empty<PrototypeRpgPartyMemberDefinition>();
        PrototypeRpgJobUnlockMemberState[] memberStates = new PrototypeRpgJobUnlockMemberState[members.Length];
        List<string> specializationParts = new List<string>();
        List<string> passiveParts = new List<string>();
        List<string> synergyParts = new List<string>();
        List<string> previewParts = new List<string>();
        bool riskyRoute = string.Equals(runResultSnapshot != null ? runResultSnapshot.RouteId : _selectedRouteId, RiskyRouteId, StringComparison.Ordinal);
        bool eliteCleared = runResultSnapshot != null && runResultSnapshot.EliteOutcome != null && runResultSnapshot.EliteOutcome.IsEliteDefeated;
        bool recoveryRun = runResultSnapshot != null && (runResultSnapshot.ResultStateKey == PrototypeBattleOutcomeKeys.RunDefeat || runResultSnapshot.ResultStateKey == PrototypeBattleOutcomeKeys.RunRetreat);

        for (int i = 0; i < members.Length; i++)
        {
            PrototypeRpgPartyMemberDefinition memberDefinition = members[i];
            PrototypeRpgAppliedPartyMemberProgressState appliedMemberState = GetRpgAppliedPartyMemberProgressState(appliedState, memberDefinition != null ? memberDefinition.MemberId : string.Empty);
            PrototypeRpgPartyMemberLevelRuntimeState levelMemberState = GetRpgMemberLevelRuntimeState(partyLevelState, memberDefinition != null ? memberDefinition.MemberId : string.Empty);
            PrototypeRpgJobUnlockMemberState memberState = BuildRpgJobUnlockMemberState(memberDefinition, appliedMemberState, levelMemberState, riskyRoute, eliteCleared, recoveryRun);
            memberStates[i] = memberState;

            if (memberDefinition == null)
            {
                continue;
            }

            if (appliedMemberState != null)
            {
                appliedMemberState.LastUnlockedSpecializationKey = memberState.CurrentSpecializationKey;
                appliedMemberState.AppliedPassiveSkillId = string.IsNullOrEmpty(appliedMemberState.AppliedPassiveSkillId) ? memberState.CurrentPassiveSkillId : appliedMemberState.AppliedPassiveSkillId;
                appliedMemberState.SpecializationSummaryText = memberState.SpecializationSummaryText;
                appliedMemberState.PassiveSkillSlotSummaryText = memberState.PassiveSlotSummaryText;
                appliedMemberState.RuntimeSynergySummaryText = memberState.RuntimeSynergySummaryText;
                appliedMemberState.NextRunSpecializationPreviewText = memberState.NextRunPreviewText;
                appliedMemberState.UnlockedSpecializationKeys = CopyRpgStringArray(memberState.UnlockedSpecializationKeys);
            }

            if (levelMemberState != null)
            {
                levelMemberState.JobSpecializationKey = memberState.CurrentSpecializationKey;
                levelMemberState.JobSpecializationSummaryText = memberState.SpecializationSummaryText;
                levelMemberState.PassiveSkillId = memberState.CurrentPassiveSkillId;
                levelMemberState.PassiveSkillSlotSummaryText = memberState.PassiveSlotSummaryText;
                levelMemberState.RuntimeSynergySummaryText = memberState.RuntimeSynergySummaryText;
                levelMemberState.NextRunSpecializationPreviewText = memberState.NextRunPreviewText;
                levelMemberState.PendingUnlockHintText = memberState.PendingUnlockHintText;
            }

            AddRpgSummaryPart(specializationParts, memberState.SpecializationSummaryText);
            AddRpgSummaryPart(passiveParts, memberState.PassiveSlotSummaryText);
            AddRpgSummaryPart(synergyParts, memberState.RuntimeSynergySummaryText);
            AddRpgSummaryPart(previewParts, memberState.NextRunPreviewText);
        }

        _sessionRpgJobUnlockRuntimeState.Members = memberStates;
        _sessionRpgJobUnlockRuntimeState.JobSpecializationSummaryText = specializationParts.Count <= 0 ? string.Empty : string.Join(" | ", specializationParts.ToArray());
        _sessionRpgJobUnlockRuntimeState.PassiveSkillSlotSummaryText = passiveParts.Count <= 0 ? string.Empty : string.Join(" | ", passiveParts.ToArray());
        _sessionRpgJobUnlockRuntimeState.RuntimeSynergySummaryText = synergyParts.Count <= 0 ? string.Empty : string.Join(" | ", synergyParts.ToArray());
        _sessionRpgJobUnlockRuntimeState.NextRunSpecializationPreviewText = previewParts.Count <= 0 ? string.Empty : string.Join(" | ", previewParts.ToArray());
        _sessionRpgJobUnlockRuntimeState.SummaryText = BuildRpgCompactSummaryText(
            _sessionRpgJobUnlockRuntimeState.JobSpecializationSummaryText,
            _sessionRpgJobUnlockRuntimeState.PassiveSkillSlotSummaryText,
            _sessionRpgJobUnlockRuntimeState.RuntimeSynergySummaryText,
            _sessionRpgJobUnlockRuntimeState.NextRunSpecializationPreviewText);
        return LatestRpgJobUnlockRuntimeState;
    }

    private void ApplyRpgJobSpecializationAndPassiveBonuses(
        PrototypeRpgPartyMemberDefinition memberDefinition,
        PrototypeRpgAppliedPartyMemberProgressState appliedMemberState,
        PrototypeRpgPartyMemberLevelRuntimeState memberLevelState,
        PrototypeRpgStatModifierBundle bundle)
    {
        if (memberDefinition == null || bundle == null)
        {
            return;
        }

        PrototypeRpgJobSpecializationDefinition specializationDefinition = ResolveCurrentRpgJobSpecializationDefinition(memberDefinition, appliedMemberState);
        PrototypeRpgPassiveSkillDefinition passiveDefinition = ResolveCurrentRpgPassiveDefinition(memberDefinition, appliedMemberState, specializationDefinition, memberLevelState, string.Equals(_selectedRouteId, RiskyRouteId, StringComparison.Ordinal), false, false);
        PrototypeRpgPassiveModifierContribution passiveContribution = PrototypeRpgPassiveSkillCatalog.CreateContribution(passiveDefinition != null ? passiveDefinition.PassiveSkillId : string.Empty);

        if (specializationDefinition != null)
        {
            bundle.JobSpecializationKey = specializationDefinition.SpecializationKey;
            bundle.JobSpecializationSummaryText = specializationDefinition.DisplayLabel + " | " + specializationDefinition.SummaryText;
            bundle.BonusAttack += specializationDefinition.BonusAttack;
            bundle.BonusDefense += specializationDefinition.BonusDefense;
            bundle.BonusSpeed += specializationDefinition.BonusSpeed;
            bundle.BonusSkillPower += specializationDefinition.BonusSkillPower;
            bundle.PassiveHintText = BuildRpgCompactSummaryText(bundle.PassiveHintText, specializationDefinition.RuntimeSynergyText);
            bundle.BattleLabelHint = BuildRpgCompactSummaryText(bundle.BattleLabelHint, specializationDefinition.DisplayLabel);
        }

        if (!string.IsNullOrEmpty(passiveContribution.PassiveSkillId))
        {
            bundle.PassiveSkillId = passiveContribution.PassiveSkillId;
            bundle.PassiveSkillSlotSummaryText = "Passive slot: " + passiveContribution.DisplayLabel + " | " + passiveContribution.SummaryText;
            bundle.BonusAttack += passiveContribution.BonusAttack;
            bundle.BonusDefense += passiveContribution.BonusDefense;
            bundle.BonusSpeed += passiveContribution.BonusSpeed;
            bundle.BonusSkillPower += passiveContribution.BonusSkillPower;
            bundle.PassiveHintText = BuildRpgCompactSummaryText(bundle.PassiveHintText, passiveContribution.PassiveHintText, bundle.PassiveSkillSlotSummaryText);
            bundle.BattleLabelHint = BuildRpgCompactSummaryText(bundle.BattleLabelHint, passiveContribution.BattleLabelHint);
        }

        bundle.RuntimeSynergySummaryText = BuildRpgCompactSummaryText(
            specializationDefinition != null ? specializationDefinition.RuntimeSynergyText : string.Empty,
            passiveContribution.SummaryText,
            memberLevelState != null ? memberLevelState.GearAffixLiteSummaryText : string.Empty,
            memberLevelState != null ? BuildRpgProgressionIdLabel(memberLevelState.SkillLoadoutId) : string.Empty);

        if (memberLevelState != null)
        {
            memberLevelState.JobSpecializationKey = bundle.JobSpecializationKey;
            memberLevelState.JobSpecializationSummaryText = bundle.JobSpecializationSummaryText;
            memberLevelState.PassiveSkillId = bundle.PassiveSkillId;
            memberLevelState.PassiveSkillSlotSummaryText = bundle.PassiveSkillSlotSummaryText;
            memberLevelState.RuntimeSynergySummaryText = bundle.RuntimeSynergySummaryText;
            memberLevelState.RuntimeSkillPowerBonus = bundle.BonusSkillPower;
        }
    }

    private PrototypeRpgJobUnlockMemberState BuildRpgJobUnlockMemberState(
        PrototypeRpgPartyMemberDefinition memberDefinition,
        PrototypeRpgAppliedPartyMemberProgressState appliedMemberState,
        PrototypeRpgPartyMemberLevelRuntimeState levelMemberState,
        bool riskyRoute,
        bool eliteCleared,
        bool recoveryRun)
    {
        PrototypeRpgJobUnlockMemberState memberState = new PrototypeRpgJobUnlockMemberState();
        if (memberDefinition == null)
        {
            return memberState;
        }

        int level = levelMemberState != null && levelMemberState.Experience != null ? Math.Max(1, levelMemberState.Experience.Level) : 1;
        PrototypeRpgJobSpecializationDefinition specializationDefinition = ResolveCurrentRpgJobSpecializationDefinition(memberDefinition, appliedMemberState);
        PrototypeRpgPassiveSkillDefinition passiveDefinition = ResolveCurrentRpgPassiveDefinition(memberDefinition, appliedMemberState, specializationDefinition, levelMemberState, riskyRoute, eliteCleared, recoveryRun);
        PrototypeRpgJobSpecializationDefinition unlockCandidate = ResolveRpgJobSpecializationUnlockCandidate(memberDefinition, appliedMemberState, levelMemberState, riskyRoute, eliteCleared, recoveryRun);

        memberState.MemberId = string.IsNullOrEmpty(memberDefinition.MemberId) ? string.Empty : memberDefinition.MemberId;
        memberState.DisplayName = string.IsNullOrEmpty(memberDefinition.DisplayName) ? "Adventurer" : memberDefinition.DisplayName;
        memberState.AppliedJobId = ResolveAppliedOrDefinitionId(appliedMemberState != null ? appliedMemberState.AppliedJobId : string.Empty, memberDefinition.JobId);
        memberState.CurrentSpecializationKey = specializationDefinition != null ? specializationDefinition.SpecializationKey : string.Empty;
        memberState.CurrentSpecializationLabel = specializationDefinition != null ? specializationDefinition.DisplayLabel : BuildRpgProgressionIdLabel(memberState.AppliedJobId);
        memberState.CurrentPassiveSkillId = passiveDefinition != null ? passiveDefinition.PassiveSkillId : string.Empty;
        memberState.CurrentPassiveSkillLabel = passiveDefinition != null ? passiveDefinition.DisplayLabel : string.Empty;
        memberState.SpecializationSummaryText = BuildRpgJobSpecializationSummaryText(memberState.DisplayName, specializationDefinition, level, levelMemberState);
        memberState.PassiveSlotSummaryText = BuildRpgPassiveSkillSlotSummaryText(memberState.DisplayName, passiveDefinition, levelMemberState);
        memberState.RuntimeSynergySummaryText = BuildRpgJobPassiveRuntimeSynergySummaryText(memberState.DisplayName, specializationDefinition, passiveDefinition, levelMemberState);
        memberState.PendingUnlockHintText = BuildRpgJobSpecializationPendingUnlockHintText(memberState.DisplayName, unlockCandidate, level, eliteCleared, riskyRoute);
        memberState.NextRunPreviewText = BuildRpgJobSpecializationNextRunPreviewText(memberState.DisplayName, specializationDefinition, passiveDefinition, unlockCandidate);
        memberState.LastUnlockedRunIdentity = string.IsNullOrEmpty(_sessionRpgJobUnlockRuntimeState.LastRunIdentity) ? string.Empty : _sessionRpgJobUnlockRuntimeState.LastRunIdentity;
        memberState.UnlockedSpecializationKeys = MergeRpgUnlockedSpecializationKeys(
            appliedMemberState != null ? appliedMemberState.UnlockedSpecializationKeys : Array.Empty<string>(),
            specializationDefinition,
            unlockCandidate,
            level,
            eliteCleared,
            riskyRoute);
        return memberState;
    }

    private PrototypeRpgJobSpecializationDefinition ResolveCurrentRpgJobSpecializationDefinition(PrototypeRpgPartyMemberDefinition memberDefinition, PrototypeRpgAppliedPartyMemberProgressState appliedMemberState)
    {
        string roleTag = memberDefinition != null ? memberDefinition.RoleTag : string.Empty;
        string growthTrackId = ResolveAppliedOrDefinitionId(appliedMemberState != null ? appliedMemberState.AppliedGrowthTrackId : string.Empty, memberDefinition != null ? memberDefinition.GrowthTrackId : string.Empty);
        string jobId = ResolveAppliedOrDefinitionId(appliedMemberState != null ? appliedMemberState.AppliedJobId : string.Empty, memberDefinition != null ? memberDefinition.JobId : string.Empty);
        return PrototypeRpgJobSpecializationCatalog.ResolveCurrentDefinition(roleTag, growthTrackId, jobId);
    }

    private PrototypeRpgPassiveSkillDefinition ResolveCurrentRpgPassiveDefinition(
        PrototypeRpgPartyMemberDefinition memberDefinition,
        PrototypeRpgAppliedPartyMemberProgressState appliedMemberState,
        PrototypeRpgJobSpecializationDefinition specializationDefinition,
        PrototypeRpgPartyMemberLevelRuntimeState levelMemberState,
        bool riskyRoute,
        bool eliteCleared,
        bool recoveryRun)
    {
        PrototypeRpgPassiveSkillDefinition appliedDefinition = PrototypeRpgPassiveSkillCatalog.GetDefinition(appliedMemberState != null ? appliedMemberState.AppliedPassiveSkillId : string.Empty);
        if (appliedDefinition != null)
        {
            return appliedDefinition;
        }

        PrototypeRpgPassiveSkillDefinition specializationPassive = PrototypeRpgPassiveSkillCatalog.GetDefinition(specializationDefinition != null ? specializationDefinition.PassiveSkillId : string.Empty);
        if (specializationPassive != null)
        {
            return specializationPassive;
        }

        string roleTag = memberDefinition != null ? memberDefinition.RoleTag : string.Empty;
        string growthTrackId = memberDefinition != null ? memberDefinition.GrowthTrackId : string.Empty;
        string equipmentLoadoutId = levelMemberState != null ? levelMemberState.EquipmentLoadoutId : string.Empty;
        string skillLoadoutId = levelMemberState != null ? levelMemberState.SkillLoadoutId : string.Empty;
        if (ShouldBiasRpgSpecializationDefense(roleTag, growthTrackId, equipmentLoadoutId, skillLoadoutId, riskyRoute, eliteCleared, recoveryRun))
        {
            return PrototypeRpgPassiveSkillCatalog.GetDefinition("low_hp_guard_bonus");
        }

        if (ShouldBiasRpgSpecializationTempo(roleTag, growthTrackId, equipmentLoadoutId, skillLoadoutId, riskyRoute))
        {
            return PrototypeRpgPassiveSkillCatalog.GetDefinition("first_turn_speed_bonus");
        }

        if (string.Equals(roleTag, "cleric", StringComparison.Ordinal))
        {
            return PrototypeRpgPassiveSkillCatalog.GetDefinition("heal_bonus_small");
        }

        if (string.Equals(roleTag, "mage", StringComparison.Ordinal))
        {
            return PrototypeRpgPassiveSkillCatalog.GetDefinition("skill_damage_bonus_small");
        }

        if (string.Equals(roleTag, "rogue", StringComparison.Ordinal))
        {
            return PrototypeRpgPassiveSkillCatalog.GetDefinition("flat_speed_small");
        }

        return PrototypeRpgPassiveSkillCatalog.GetDefinition("flat_attack_small");
    }

    private PrototypeRpgJobSpecializationDefinition ResolveRpgJobSpecializationUnlockCandidate(
        PrototypeRpgPartyMemberDefinition memberDefinition,
        PrototypeRpgAppliedPartyMemberProgressState appliedMemberState,
        PrototypeRpgPartyMemberLevelRuntimeState levelMemberState,
        bool riskyRoute,
        bool eliteCleared,
        bool recoveryRun)
    {
        if (memberDefinition == null)
        {
            return null;
        }

        string roleTag = memberDefinition.RoleTag;
        string growthTrackId = ResolveAppliedOrDefinitionId(appliedMemberState != null ? appliedMemberState.AppliedGrowthTrackId : string.Empty, memberDefinition.GrowthTrackId);
        string equipmentLoadoutId = levelMemberState != null ? levelMemberState.EquipmentLoadoutId : ResolveAppliedOrDefinitionId(appliedMemberState != null ? appliedMemberState.AppliedEquipmentLoadoutId : string.Empty, memberDefinition.EquipmentLoadoutId);
        string skillLoadoutId = levelMemberState != null ? levelMemberState.SkillLoadoutId : ResolveAppliedOrDefinitionId(appliedMemberState != null ? appliedMemberState.AppliedSkillLoadoutId : string.Empty, memberDefinition.SkillLoadoutId);
        int level = levelMemberState != null && levelMemberState.Experience != null ? Math.Max(1, levelMemberState.Experience.Level) : 1;
        bool defenseBias = ShouldBiasRpgSpecializationDefense(roleTag, growthTrackId, equipmentLoadoutId, skillLoadoutId, riskyRoute, eliteCleared, recoveryRun);
        bool tempoBias = ShouldBiasRpgSpecializationTempo(roleTag, growthTrackId, equipmentLoadoutId, skillLoadoutId, riskyRoute);
        bool offenseBias = ShouldBiasRpgSpecializationOffense(roleTag, growthTrackId, equipmentLoadoutId, skillLoadoutId, riskyRoute);

        if (string.Equals(roleTag, "warrior", StringComparison.Ordinal))
        {
            if (level >= 3 && eliteCleared && defenseBias) return PrototypeRpgJobSpecializationCatalog.GetDefinition("spec_bastion");
            if (level >= 3 && offenseBias) return PrototypeRpgJobSpecializationCatalog.GetDefinition("spec_vanguard");
            if (level >= 2 && defenseBias) return PrototypeRpgJobSpecializationCatalog.GetDefinition("spec_guardian");
            if (level >= 2) return PrototypeRpgJobSpecializationCatalog.GetDefinition("spec_breaker");
        }
        else if (string.Equals(roleTag, "rogue", StringComparison.Ordinal))
        {
            if (level >= 3 && eliteCleared && tempoBias) return PrototypeRpgJobSpecializationCatalog.GetDefinition("spec_phantom");
            if (level >= 3 && offenseBias) return PrototypeRpgJobSpecializationCatalog.GetDefinition("spec_saboteur");
            if (level >= 2 && tempoBias) return PrototypeRpgJobSpecializationCatalog.GetDefinition("spec_shadow");
            if (level >= 2) return PrototypeRpgJobSpecializationCatalog.GetDefinition("spec_executioner");
        }
        else if (string.Equals(roleTag, "mage", StringComparison.Ordinal))
        {
            if (level >= 3 && eliteCleared && defenseBias) return PrototypeRpgJobSpecializationCatalog.GetDefinition("spec_astral");
            if (level >= 3 && offenseBias) return PrototypeRpgJobSpecializationCatalog.GetDefinition("spec_tempest");
            if (level >= 2 && defenseBias) return PrototypeRpgJobSpecializationCatalog.GetDefinition("spec_spellguard");
            if (level >= 2) return PrototypeRpgJobSpecializationCatalog.GetDefinition("spec_arcanist");
        }
        else if (string.Equals(roleTag, "cleric", StringComparison.Ordinal))
        {
            if (level >= 3 && eliteCleared && defenseBias) return PrototypeRpgJobSpecializationCatalog.GetDefinition("spec_saint");
            if (level >= 3 && (offenseBias || tempoBias)) return PrototypeRpgJobSpecializationCatalog.GetDefinition("spec_beacon");
            if (level >= 2 && defenseBias) return PrototypeRpgJobSpecializationCatalog.GetDefinition("spec_warden");
            if (level >= 2) return PrototypeRpgJobSpecializationCatalog.GetDefinition("spec_chaplain");
        }

        return PrototypeRpgJobSpecializationCatalog.ResolveCurrentDefinition(roleTag, growthTrackId, memberDefinition.JobId);
    }

    private bool ShouldBiasRpgSpecializationOffense(string roleTag, string growthTrackId, string equipmentLoadoutId, string skillLoadoutId, bool riskyRoute)
    {
        return riskyRoute ||
               ContainsAnyKeyword(growthTrackId, "breaker", "execution", "surge", "vanguard", "saboteur", "tempest", "beacon") ||
               ContainsAnyKeyword(equipmentLoadoutId, "brutal", "finisher", "focus", "vanguard", "saboteur", "tempest", "beacon") ||
               ContainsAnyKeyword(skillLoadoutId, "break", "finisher", "surge", "vanguard", "tempest", "beacon") ||
               string.Equals(roleTag, "mage", StringComparison.Ordinal);
    }

    private bool ShouldBiasRpgSpecializationDefense(string roleTag, string growthTrackId, string equipmentLoadoutId, string skillLoadoutId, bool riskyRoute, bool eliteCleared, bool recoveryRun)
    {
        return recoveryRun ||
               eliteCleared ||
               ContainsAnyKeyword(growthTrackId, "guard", "ward", "bastion", "astral", "saint") ||
               ContainsAnyKeyword(equipmentLoadoutId, "shielded", "warded", "guarded", "bastion", "astral", "saint") ||
               ContainsAnyKeyword(skillLoadoutId, "guard", "ward", "bastion", "astral", "saint") ||
               (!riskyRoute && string.Equals(roleTag, "cleric", StringComparison.Ordinal));
    }

    private bool ShouldBiasRpgSpecializationTempo(string roleTag, string growthTrackId, string equipmentLoadoutId, string skillLoadoutId, bool riskyRoute)
    {
        return riskyRoute ||
               ContainsAnyKeyword(growthTrackId, "shadow", "phantom", "saboteur", "tempest", "beacon") ||
               ContainsAnyKeyword(equipmentLoadoutId, "evasion", "phantom", "saboteur", "tempest", "beacon") ||
               ContainsAnyKeyword(skillLoadoutId, "evasion", "phantom", "saboteur", "tempest", "beacon") ||
               string.Equals(roleTag, "rogue", StringComparison.Ordinal);
    }

    private bool ContainsAnyKeyword(string value, params string[] keywords)
    {
        if (string.IsNullOrEmpty(value) || keywords == null || keywords.Length <= 0)
        {
            return false;
        }

        for (int i = 0; i < keywords.Length; i++)
        {
            string keyword = keywords[i];
            if (!string.IsNullOrEmpty(keyword) && value.IndexOf(keyword, StringComparison.OrdinalIgnoreCase) >= 0)
            {
                return true;
            }
        }

        return false;
    }

    private string[] MergeRpgUnlockedSpecializationKeys(string[] existingKeys, PrototypeRpgJobSpecializationDefinition currentDefinition, PrototypeRpgJobSpecializationDefinition unlockCandidate, int level, bool eliteCleared, bool riskyRoute)
    {
        List<string> keys = new List<string>();
        AddRpgUnlockedSpecializationKey(keys, existingKeys);
        if (currentDefinition != null && level >= currentDefinition.RequiredLevel)
        {
            AddRpgUnlockedSpecializationKey(keys, currentDefinition.SpecializationKey);
        }

        if (unlockCandidate != null && level >= unlockCandidate.RequiredLevel)
        {
            if (!unlockCandidate.RequiresEliteClear || eliteCleared)
            {
                if (!unlockCandidate.PrefersRiskyRoute || riskyRoute || eliteCleared)
                {
                    AddRpgUnlockedSpecializationKey(keys, unlockCandidate.SpecializationKey);
                }
            }
        }

        return keys.Count <= 0 ? Array.Empty<string>() : keys.ToArray();
    }

    private void AddRpgUnlockedSpecializationKey(List<string> keys, string[] source)
    {
        if (keys == null || source == null || source.Length <= 0)
        {
            return;
        }

        for (int i = 0; i < source.Length; i++)
        {
            AddRpgUnlockedSpecializationKey(keys, source[i]);
        }
    }

    private void AddRpgUnlockedSpecializationKey(List<string> keys, string key)
    {
        string normalizedKey = string.IsNullOrWhiteSpace(key) ? string.Empty : key.Trim().ToLowerInvariant();
        if (keys == null || string.IsNullOrEmpty(normalizedKey) || keys.Contains(normalizedKey))
        {
            return;
        }

        keys.Add(normalizedKey);
    }

    private string BuildRpgJobSpecializationSummaryText(string displayName, PrototypeRpgJobSpecializationDefinition definition, int level, PrototypeRpgPartyMemberLevelRuntimeState levelMemberState)
    {
        if (definition == null)
        {
            return string.IsNullOrEmpty(displayName) ? string.Empty : displayName + " specialization unresolved.";
        }

        return BuildRpgCompactSummaryText(
            displayName + " " + definition.DisplayLabel,
            "Lv " + Math.Max(1, level),
            definition.SummaryText,
            levelMemberState != null ? levelMemberState.EquipmentSummaryText : string.Empty);
    }

    private string BuildRpgPassiveSkillSlotSummaryText(string displayName, PrototypeRpgPassiveSkillDefinition definition, PrototypeRpgPartyMemberLevelRuntimeState levelMemberState)
    {
        if (definition == null)
        {
            return string.IsNullOrEmpty(displayName) ? string.Empty : displayName + " passive slot unresolved.";
        }

        return BuildRpgCompactSummaryText(
            displayName + " passive slot: " + definition.DisplayLabel,
            definition.SummaryText,
            levelMemberState != null ? BuildRpgProgressionIdLabel(levelMemberState.SkillLoadoutId) : string.Empty);
    }

    private string BuildRpgJobPassiveRuntimeSynergySummaryText(string displayName, PrototypeRpgJobSpecializationDefinition specializationDefinition, PrototypeRpgPassiveSkillDefinition passiveDefinition, PrototypeRpgPartyMemberLevelRuntimeState levelMemberState)
    {
        return BuildRpgCompactSummaryText(
            string.IsNullOrEmpty(displayName) ? string.Empty : displayName + " synergy",
            specializationDefinition != null ? specializationDefinition.RuntimeSynergyText : string.Empty,
            passiveDefinition != null ? passiveDefinition.PassiveHintText : string.Empty,
            levelMemberState != null ? levelMemberState.GearAffixLiteSummaryText : string.Empty,
            levelMemberState != null ? BuildRpgProgressionIdLabel(levelMemberState.SkillLoadoutId) : string.Empty);
    }

    private string BuildRpgJobSpecializationPendingUnlockHintText(string displayName, PrototypeRpgJobSpecializationDefinition unlockCandidate, int level, bool eliteCleared, bool riskyRoute)
    {
        if (unlockCandidate == null)
        {
            return string.Empty;
        }

        string requirementText = level < unlockCandidate.RequiredLevel
            ? "needs Lv " + unlockCandidate.RequiredLevel
            : unlockCandidate.RequiresEliteClear && !eliteCleared
                ? "needs elite clear"
                : unlockCandidate.PrefersRiskyRoute && !riskyRoute && !eliteCleared
                    ? "prefers risky or elite momentum"
                    : "ready";
        return BuildRpgCompactSummaryText(displayName + " unlock hint: " + unlockCandidate.DisplayLabel, requirementText, unlockCandidate.UnlockHintText);
    }

    private string BuildRpgJobSpecializationNextRunPreviewText(string displayName, PrototypeRpgJobSpecializationDefinition currentDefinition, PrototypeRpgPassiveSkillDefinition currentPassive, PrototypeRpgJobSpecializationDefinition unlockCandidate)
    {
        if (unlockCandidate == null || currentDefinition == null || unlockCandidate.SpecializationKey == currentDefinition.SpecializationKey)
        {
            return BuildRpgCompactSummaryText(displayName + " next run keeps " + (currentDefinition != null ? currentDefinition.DisplayLabel : "current specialization") + ".", currentPassive != null ? "Passive " + currentPassive.DisplayLabel : string.Empty);
        }

        PrototypeRpgPassiveSkillDefinition unlockPassive = PrototypeRpgPassiveSkillCatalog.GetDefinition(unlockCandidate.PassiveSkillId);
        return BuildRpgCompactSummaryText(displayName + " unlock ready: " + unlockCandidate.DisplayLabel, unlockPassive != null ? "Passive " + unlockPassive.DisplayLabel : string.Empty, unlockCandidate.RuntimeSynergyText);
    }

    private PrototypeRpgJobUnlockRuntimeState CopyRpgJobUnlockRuntimeState(PrototypeRpgJobUnlockRuntimeState source)
    {
        PrototypeRpgJobUnlockRuntimeState copy = new PrototypeRpgJobUnlockRuntimeState();
        if (source == null)
        {
            return copy;
        }

        copy.PartyId = string.IsNullOrEmpty(source.PartyId) ? string.Empty : source.PartyId;
        copy.DisplayName = string.IsNullOrEmpty(source.DisplayName) ? string.Empty : source.DisplayName;
        copy.LastRunIdentity = string.IsNullOrEmpty(source.LastRunIdentity) ? string.Empty : source.LastRunIdentity;
        copy.JobSpecializationSummaryText = string.IsNullOrEmpty(source.JobSpecializationSummaryText) ? string.Empty : source.JobSpecializationSummaryText;
        copy.PassiveSkillSlotSummaryText = string.IsNullOrEmpty(source.PassiveSkillSlotSummaryText) ? string.Empty : source.PassiveSkillSlotSummaryText;
        copy.RuntimeSynergySummaryText = string.IsNullOrEmpty(source.RuntimeSynergySummaryText) ? string.Empty : source.RuntimeSynergySummaryText;
        copy.NextRunSpecializationPreviewText = string.IsNullOrEmpty(source.NextRunSpecializationPreviewText) ? string.Empty : source.NextRunSpecializationPreviewText;
        copy.SummaryText = string.IsNullOrEmpty(source.SummaryText) ? string.Empty : source.SummaryText;
        PrototypeRpgJobUnlockMemberState[] members = source.Members ?? Array.Empty<PrototypeRpgJobUnlockMemberState>();
        if (members.Length <= 0)
        {
            copy.Members = Array.Empty<PrototypeRpgJobUnlockMemberState>();
            return copy;
        }

        PrototypeRpgJobUnlockMemberState[] memberCopies = new PrototypeRpgJobUnlockMemberState[members.Length];
        for (int i = 0; i < members.Length; i++)
        {
            memberCopies[i] = CopyRpgJobUnlockMemberState(members[i]);
        }

        copy.Members = memberCopies;
        return copy;
    }

    private PrototypeRpgJobUnlockMemberState CopyRpgJobUnlockMemberState(PrototypeRpgJobUnlockMemberState source)
    {
        PrototypeRpgJobUnlockMemberState copy = new PrototypeRpgJobUnlockMemberState();
        if (source == null)
        {
            return copy;
        }

        copy.MemberId = string.IsNullOrEmpty(source.MemberId) ? string.Empty : source.MemberId;
        copy.DisplayName = string.IsNullOrEmpty(source.DisplayName) ? string.Empty : source.DisplayName;
        copy.AppliedJobId = string.IsNullOrEmpty(source.AppliedJobId) ? string.Empty : source.AppliedJobId;
        copy.CurrentSpecializationKey = string.IsNullOrEmpty(source.CurrentSpecializationKey) ? string.Empty : source.CurrentSpecializationKey;
        copy.CurrentSpecializationLabel = string.IsNullOrEmpty(source.CurrentSpecializationLabel) ? string.Empty : source.CurrentSpecializationLabel;
        copy.CurrentPassiveSkillId = string.IsNullOrEmpty(source.CurrentPassiveSkillId) ? string.Empty : source.CurrentPassiveSkillId;
        copy.CurrentPassiveSkillLabel = string.IsNullOrEmpty(source.CurrentPassiveSkillLabel) ? string.Empty : source.CurrentPassiveSkillLabel;
        copy.SpecializationSummaryText = string.IsNullOrEmpty(source.SpecializationSummaryText) ? string.Empty : source.SpecializationSummaryText;
        copy.PassiveSlotSummaryText = string.IsNullOrEmpty(source.PassiveSlotSummaryText) ? string.Empty : source.PassiveSlotSummaryText;
        copy.RuntimeSynergySummaryText = string.IsNullOrEmpty(source.RuntimeSynergySummaryText) ? string.Empty : source.RuntimeSynergySummaryText;
        copy.PendingUnlockHintText = string.IsNullOrEmpty(source.PendingUnlockHintText) ? string.Empty : source.PendingUnlockHintText;
        copy.NextRunPreviewText = string.IsNullOrEmpty(source.NextRunPreviewText) ? string.Empty : source.NextRunPreviewText;
        copy.LastUnlockedRunIdentity = string.IsNullOrEmpty(source.LastUnlockedRunIdentity) ? string.Empty : source.LastUnlockedRunIdentity;
        copy.UnlockedSpecializationKeys = CopyRpgStringArray(source.UnlockedSpecializationKeys);
        return copy;
    }
}
