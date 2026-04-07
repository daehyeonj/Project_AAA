using System;
using System.Collections.Generic;
using UnityEngine;

public sealed partial class StaticPlaceholderWorldView
{
    private void ResetRpgLaneRecoveryRuntimeState()
    {
        _sessionRpgPartyLaneRecoveryRuntimeState = new PrototypeRpgPartyLaneRecoveryRuntimeState();
    }

    private PrototypeRpgPartyLaneRecoveryRuntimeState CopyRpgPartyLaneRecoveryRuntimeState(PrototypeRpgPartyLaneRecoveryRuntimeState source)
    {
        PrototypeRpgPartyLaneRecoveryRuntimeState copy = new PrototypeRpgPartyLaneRecoveryRuntimeState();
        if (source == null)
        {
            return copy;
        }

        copy.SessionKey = CopyText(source.SessionKey);
        copy.PartyId = CopyText(source.PartyId);
        copy.DisplayName = CopyText(source.DisplayName);
        copy.LastResolvedRunIdentity = CopyText(source.LastResolvedRunIdentity);
        copy.LaneRecoverySummaryText = CopyText(source.LaneRecoverySummaryText);
        copy.SoftRespecWindowSummaryText = CopyText(source.SoftRespecWindowSummaryText);
        copy.AntiTrapSafetySummaryText = CopyText(source.AntiTrapSafetySummaryText);
        copy.Members = CopyRpgMemberLaneRecoveryRuntimeStates(source.Members);
        return copy;
    }

    private PrototypeRpgMemberLaneRecoveryRuntimeState[] CopyRpgMemberLaneRecoveryRuntimeStates(PrototypeRpgMemberLaneRecoveryRuntimeState[] source)
    {
        return CopyArray(source, CopyRpgMemberLaneRecoveryRuntimeState);
    }

    private PrototypeRpgMemberLaneRecoveryRuntimeState CopyRpgMemberLaneRecoveryRuntimeState(PrototypeRpgMemberLaneRecoveryRuntimeState source)
    {
        PrototypeRpgMemberLaneRecoveryRuntimeState copy = new PrototypeRpgMemberLaneRecoveryRuntimeState();
        if (source == null)
        {
            return copy;
        }

        copy.MemberId = CopyText(source.MemberId);
        copy.DisplayName = CopyText(source.DisplayName);
        copy.RecoveryPressureScore = ClampNonNegative(source.RecoveryPressureScore);
        copy.ConsecutiveDefeatCount = ClampNonNegative(source.ConsecutiveDefeatCount);
        copy.ConsecutiveRetreatCount = ClampNonNegative(source.ConsecutiveRetreatCount);
        copy.ConsecutiveKnockOutCount = ClampNonNegative(source.ConsecutiveKnockOutCount);
        copy.ConsecutiveStalledOfferCount = ClampNonNegative(source.ConsecutiveStalledOfferCount);
        copy.ConsecutiveNoImprovementRunCount = ClampNonNegative(source.ConsecutiveNoImprovementRunCount);
        copy.SoftRespecWindowOpen = source.SoftRespecWindowOpen;
        copy.SoftRespecWindowTier = ClampNonNegative(source.SoftRespecWindowTier);
        copy.RecoveryWindowSourceRunIdentity = CopyText(source.RecoveryWindowSourceRunIdentity);
        copy.RecentRecoveryReasonText = CopyText(source.RecentRecoveryReasonText);
        copy.LastRecoveryTriggerKey = CopyText(source.LastRecoveryTriggerKey);
        copy.LastRecoveryResolvedRunIdentity = CopyText(source.LastRecoveryResolvedRunIdentity);
        copy.RecoveryBiasWeight = ClampNonNegative(source.RecoveryBiasWeight);
        copy.LaneLockRelaxWeight = ClampNonNegative(source.LaneLockRelaxWeight);
        copy.RescueOfferGroupKey = CopyText(source.RescueOfferGroupKey);
        copy.RescueAdjacentLaneKeys = CopyRpgStringArray(source.RescueAdjacentLaneKeys);
        copy.RecoverySafetySummaryText = CopyText(source.RecoverySafetySummaryText);
        return copy;
    }

    private PrototypeRpgPartyLaneRecoveryRuntimeState GetOrCreateRpgPartyLaneRecoveryRuntimeState(PrototypeRpgPartyDefinition partyDefinition)
    {
        if (partyDefinition == null)
        {
            return _sessionRpgPartyLaneRecoveryRuntimeState ?? new PrototypeRpgPartyLaneRecoveryRuntimeState();
        }

        bool rebuildState = _sessionRpgPartyLaneRecoveryRuntimeState == null ||
                            _sessionRpgPartyLaneRecoveryRuntimeState.Members == null ||
                            _sessionRpgPartyLaneRecoveryRuntimeState.PartyId != partyDefinition.PartyId;
        if (rebuildState)
        {
            _sessionRpgPartyLaneRecoveryRuntimeState = new PrototypeRpgPartyLaneRecoveryRuntimeState();
            _sessionRpgPartyLaneRecoveryRuntimeState.SessionKey = string.IsNullOrEmpty(partyDefinition.PartyId) ? string.Empty : partyDefinition.PartyId;
            _sessionRpgPartyLaneRecoveryRuntimeState.PartyId = string.IsNullOrEmpty(partyDefinition.PartyId) ? string.Empty : partyDefinition.PartyId;
            _sessionRpgPartyLaneRecoveryRuntimeState.DisplayName = string.IsNullOrEmpty(partyDefinition.DisplayName) ? _sessionRpgPartyLaneRecoveryRuntimeState.PartyId : partyDefinition.DisplayName;
            _sessionRpgPartyLaneRecoveryRuntimeState.Members = BuildDefaultRpgMemberLaneRecoveryStates(partyDefinition);
        }
        else
        {
            EnsureRpgMemberLaneRecoveryStates(partyDefinition, _sessionRpgPartyLaneRecoveryRuntimeState);
        }

        return _sessionRpgPartyLaneRecoveryRuntimeState;
    }

    private PrototypeRpgMemberLaneRecoveryRuntimeState[] BuildDefaultRpgMemberLaneRecoveryStates(PrototypeRpgPartyDefinition partyDefinition)
    {
        PrototypeRpgPartyMemberDefinition[] definitions = partyDefinition != null ? (partyDefinition.Members ?? Array.Empty<PrototypeRpgPartyMemberDefinition>()) : Array.Empty<PrototypeRpgPartyMemberDefinition>();
        if (definitions.Length <= 0)
        {
            return Array.Empty<PrototypeRpgMemberLaneRecoveryRuntimeState>();
        }

        PrototypeRpgMemberLaneRecoveryRuntimeState[] members = new PrototypeRpgMemberLaneRecoveryRuntimeState[definitions.Length];
        for (int i = 0; i < definitions.Length; i++)
        {
            PrototypeRpgPartyMemberDefinition definition = definitions[i];
            PrototypeRpgMemberLaneRecoveryRuntimeState state = new PrototypeRpgMemberLaneRecoveryRuntimeState();
            state.MemberId = string.IsNullOrEmpty(definition != null ? definition.MemberId : string.Empty) ? string.Empty : definition.MemberId;
            state.DisplayName = string.IsNullOrEmpty(definition != null ? definition.DisplayName : string.Empty) ? "Adventurer" : definition.DisplayName;
            members[i] = state;
        }

        return members;
    }

    private void EnsureRpgMemberLaneRecoveryStates(PrototypeRpgPartyDefinition partyDefinition, PrototypeRpgPartyLaneRecoveryRuntimeState state)
    {
        if (partyDefinition == null || state == null)
        {
            return;
        }

        PrototypeRpgPartyMemberDefinition[] definitions = partyDefinition.Members ?? Array.Empty<PrototypeRpgPartyMemberDefinition>();
        PrototypeRpgMemberLaneRecoveryRuntimeState[] currentMembers = state.Members ?? Array.Empty<PrototypeRpgMemberLaneRecoveryRuntimeState>();
        if (definitions.Length == currentMembers.Length)
        {
            return;
        }

        PrototypeRpgMemberLaneRecoveryRuntimeState[] rebuilt = new PrototypeRpgMemberLaneRecoveryRuntimeState[definitions.Length];
        for (int i = 0; i < definitions.Length; i++)
        {
            PrototypeRpgPartyMemberDefinition definition = definitions[i];
            PrototypeRpgMemberLaneRecoveryRuntimeState existing = GetRpgMemberLaneRecoveryRuntimeState(state, definition != null ? definition.MemberId : string.Empty);
            if (existing == null)
            {
                existing = new PrototypeRpgMemberLaneRecoveryRuntimeState();
                if (definition != null)
                {
                    existing.MemberId = string.IsNullOrEmpty(definition.MemberId) ? string.Empty : definition.MemberId;
                    existing.DisplayName = string.IsNullOrEmpty(definition.DisplayName) ? "Adventurer" : definition.DisplayName;
                }
            }

            rebuilt[i] = existing;
        }

        state.Members = rebuilt;
    }

    private PrototypeRpgMemberLaneRecoveryRuntimeState GetRpgMemberLaneRecoveryRuntimeState(PrototypeRpgPartyLaneRecoveryRuntimeState state, string memberId)
    {
        if (state == null || state.Members == null || string.IsNullOrEmpty(memberId))
        {
            return null;
        }

        for (int i = 0; i < state.Members.Length; i++)
        {
            PrototypeRpgMemberLaneRecoveryRuntimeState memberState = state.Members[i];
            if (memberState != null && memberState.MemberId == memberId)
            {
                return memberState;
            }
        }

        return null;
    }
    private void EvaluateRpgLaneRecoveryState(PrototypeRpgGrowthChoiceContext context, PrototypeRpgRunResultSnapshot runResultSnapshot, PrototypeRpgPartyDefinition partyDefinition)
    {
        if (context == null || partyDefinition == null)
        {
            return;
        }

        PrototypeRpgPartyLaneRecoveryRuntimeState partyState = GetOrCreateRpgPartyLaneRecoveryRuntimeState(partyDefinition);
        PrototypeRpgGrowthChoiceMemberContext[] members = context.Members ?? Array.Empty<PrototypeRpgGrowthChoiceMemberContext>();
        PrototypeRpgMemberLaneRecoveryRuntimeState[] resolvedStates = new PrototypeRpgMemberLaneRecoveryRuntimeState[members.Length];
        bool defeat = context.ResultStateKey == PrototypeBattleOutcomeKeys.RunDefeat;
        bool retreat = context.ResultStateKey == PrototypeBattleOutcomeKeys.RunRetreat;
        bool cleanClear = context.ResultStateKey == PrototypeBattleOutcomeKeys.RunClear;
        for (int i = 0; i < members.Length; i++)
        {
            PrototypeRpgGrowthChoiceMemberContext member = members[i] ?? new PrototypeRpgGrowthChoiceMemberContext();
            PrototypeRpgMemberLaneRecoveryRuntimeState state = GetRpgMemberLaneRecoveryRuntimeState(partyState, member.MemberId) ?? new PrototypeRpgMemberLaneRecoveryRuntimeState();
            bool meaningfulImprovement = IsRpgMeaningfulRecoveryRun(member, context);
            bool noImprovement = !meaningfulImprovement && (member.KnockedOut || defeat || retreat || member.ActionCount <= 1 || member.DamageDealt + member.HealingDone + member.KillCount * 4 <= 6);
            bool stalledOffer = member.CommitmentDepth >= 3 && !string.IsNullOrEmpty(member.LastAppliedOfferTypeKey) && member.RecentOfferCount >= 2 && noImprovement;
            bool sustainedFailure = member.DamageTaken >= member.DamageDealt + 6 && member.HealingDone <= 0;

            state.MemberId = string.IsNullOrEmpty(member.MemberId) ? string.Empty : member.MemberId;
            state.DisplayName = string.IsNullOrEmpty(member.DisplayName) ? "Adventurer" : member.DisplayName;
            state.ConsecutiveDefeatCount = defeat ? state.ConsecutiveDefeatCount + 1 : (cleanClear ? Mathf.Max(0, state.ConsecutiveDefeatCount - 1) : 0);
            state.ConsecutiveRetreatCount = retreat ? state.ConsecutiveRetreatCount + 1 : (cleanClear ? Mathf.Max(0, state.ConsecutiveRetreatCount - 1) : 0);
            state.ConsecutiveKnockOutCount = member.KnockedOut ? state.ConsecutiveKnockOutCount + 1 : (cleanClear && member.Survived ? Mathf.Max(0, state.ConsecutiveKnockOutCount - 1) : 0);
            state.ConsecutiveStalledOfferCount = stalledOffer ? state.ConsecutiveStalledOfferCount + 1 : (meaningfulImprovement ? Mathf.Max(0, state.ConsecutiveStalledOfferCount - 1) : Mathf.Max(0, state.ConsecutiveStalledOfferCount));
            state.ConsecutiveNoImprovementRunCount = noImprovement ? state.ConsecutiveNoImprovementRunCount + 1 : (meaningfulImprovement ? Mathf.Max(0, state.ConsecutiveNoImprovementRunCount - 2) : Mathf.Max(0, state.ConsecutiveNoImprovementRunCount - 1));

            int pressure = Mathf.Max(0, state.RecoveryPressureScore);
            if (defeat)
            {
                pressure += 5;
            }
            if (retreat)
            {
                pressure += 4;
            }
            if (member.KnockedOut)
            {
                pressure += 5;
            }
            if (noImprovement)
            {
                pressure += 3;
            }
            if (stalledOffer)
            {
                pressure += 4;
            }
            if (sustainedFailure && member.CommitmentDepth >= 3)
            {
                pressure += 3;
            }
            if (member.RecentOfferCount >= 3 && !string.IsNullOrEmpty(member.LastAppliedOfferTypeKey) && member.LastAppliedOfferTypeKey == member.LastOfferedTypeKey)
            {
                pressure += 2;
            }
            if (cleanClear && meaningfulImprovement)
            {
                pressure = ApplyRecoveryDecayAfterSuccessfulRun(state, member, pressure);
            }
            else if (cleanClear && member.Survived && !member.KnockedOut)
            {
                pressure = Mathf.Max(0, pressure - 2);
            }

            state.RecoveryPressureScore = Mathf.Clamp(pressure, 0, 30);
            string triggerKey = ResolveRpgRecoveryTriggerKey(state, member);
            state.LastRecoveryTriggerKey = string.IsNullOrEmpty(triggerKey) ? string.Empty : triggerKey;
            state.SoftRespecWindowTier = ResolveSoftRespecWindowState(state, member, triggerKey);
            state.SoftRespecWindowOpen = state.SoftRespecWindowTier > 0;
            if (state.SoftRespecWindowOpen && !string.IsNullOrEmpty(context.RunIdentity))
            {
                state.RecoveryWindowSourceRunIdentity = context.RunIdentity;
            }
            state.LastRecoveryResolvedRunIdentity = string.IsNullOrEmpty(context.RunIdentity) ? string.Empty : context.RunIdentity;
            state.RecoveryBiasWeight = state.SoftRespecWindowOpen ? Mathf.Max(6, state.RecoveryPressureScore + state.SoftRespecWindowTier * 3) : 0;
            state.LaneLockRelaxWeight = state.SoftRespecWindowOpen ? Mathf.Max(2, state.SoftRespecWindowTier * 4 + state.ConsecutiveStalledOfferCount * 2 + state.ConsecutiveKnockOutCount) : 0;
            state.RescueOfferGroupKey = ResolveRpgRecoveryRescueGroupKey(member);
            state.RescueAdjacentLaneKeys = BuildRpgRecoveryAdjacentLaneKeys(member);
            state.RecentRecoveryReasonText = BuildRpgMemberRecoveryReasonText(state, member, meaningfulImprovement, noImprovement, stalledOffer);
            state.RecoverySafetySummaryText = BuildRpgMemberRecoverySafetySummary(state, member);
            resolvedStates[i] = state;

            member.RecoveryPressureScore = state.RecoveryPressureScore;
            member.ConsecutiveDefeatCount = state.ConsecutiveDefeatCount;
            member.ConsecutiveRetreatCount = state.ConsecutiveRetreatCount;
            member.ConsecutiveKnockOutCount = state.ConsecutiveKnockOutCount;
            member.ConsecutiveStalledOfferCount = state.ConsecutiveStalledOfferCount;
            member.ConsecutiveNoImprovementRunCount = state.ConsecutiveNoImprovementRunCount;
            member.SoftRespecWindowOpen = state.SoftRespecWindowOpen;
            member.SoftRespecWindowTier = state.SoftRespecWindowTier;
            member.RecoveryWindowSourceRunIdentity = string.IsNullOrEmpty(state.RecoveryWindowSourceRunIdentity) ? string.Empty : state.RecoveryWindowSourceRunIdentity;
            member.RecentRecoveryReasonText = string.IsNullOrEmpty(state.RecentRecoveryReasonText) ? string.Empty : state.RecentRecoveryReasonText;
            member.LastRecoveryTriggerKey = string.IsNullOrEmpty(state.LastRecoveryTriggerKey) ? string.Empty : state.LastRecoveryTriggerKey;
            member.LastRecoveryResolvedRunIdentity = string.IsNullOrEmpty(state.LastRecoveryResolvedRunIdentity) ? string.Empty : state.LastRecoveryResolvedRunIdentity;
            member.RecoveryBiasWeight = state.RecoveryBiasWeight;
            member.LaneLockRelaxWeight = state.LaneLockRelaxWeight;
            member.RescueOfferGroupKey = string.IsNullOrEmpty(state.RescueOfferGroupKey) ? string.Empty : state.RescueOfferGroupKey;
            member.RescueAdjacentLaneKeys = CopyRpgStringArray(state.RescueAdjacentLaneKeys);
            member.RecoverySafetySummaryText = string.IsNullOrEmpty(state.RecoverySafetySummaryText) ? string.Empty : state.RecoverySafetySummaryText;
        }

        partyState.Members = resolvedStates;
        partyState.LastResolvedRunIdentity = string.IsNullOrEmpty(context.RunIdentity) ? string.Empty : context.RunIdentity;
        RefreshRpgLaneRecoveryStateConsistency(context, partyState);
    }

    private bool IsRpgMeaningfulRecoveryRun(PrototypeRpgGrowthChoiceMemberContext member, PrototypeRpgGrowthChoiceContext context)
    {
        if (member == null)
        {
            return false;
        }

        if (!member.KnockedOut && member.Survived && member.HealingDone >= 8)
        {
            return true;
        }
        if (!member.KnockedOut && member.Survived && member.KillCount > 0)
        {
            return true;
        }
        if (!member.KnockedOut && member.Survived && member.DamageDealt >= Mathf.Max(8, member.DamageTaken))
        {
            return true;
        }
        if (!member.KnockedOut && member.Survived && context != null && context.ResultStateKey == PrototypeBattleOutcomeKeys.RunClear && member.ActionCount >= 2 && member.DamageTaken <= member.DamageDealt + 2)
        {
            return true;
        }

        return false;
    }

    private int ApplyRecoveryDecayAfterSuccessfulRun(PrototypeRpgMemberLaneRecoveryRuntimeState state, PrototypeRpgGrowthChoiceMemberContext member, int pressure)
    {
        if (state == null)
        {
            return Mathf.Max(0, pressure);
        }

        int decay = 4;
        if (member != null && (member.KillCount > 0 || member.HealingDone >= 8 || member.DamageDealt >= Mathf.Max(10, member.DamageTaken)))
        {
            decay += 2;
        }
        if (member != null && member.Survived && !member.KnockedOut)
        {
            decay += 1;
        }

        pressure = Mathf.Max(0, pressure - decay);
        state.ConsecutiveDefeatCount = Mathf.Max(0, state.ConsecutiveDefeatCount - 1);
        state.ConsecutiveRetreatCount = Mathf.Max(0, state.ConsecutiveRetreatCount - 1);
        state.ConsecutiveKnockOutCount = Mathf.Max(0, state.ConsecutiveKnockOutCount - (member != null && !member.KnockedOut ? 1 : 0));
        state.ConsecutiveStalledOfferCount = Mathf.Max(0, state.ConsecutiveStalledOfferCount - 1);
        state.ConsecutiveNoImprovementRunCount = Mathf.Max(0, state.ConsecutiveNoImprovementRunCount - 2);
        if (pressure <= 4)
        {
            state.SoftRespecWindowOpen = false;
            state.SoftRespecWindowTier = 0;
            state.LastRecoveryTriggerKey = string.Empty;
        }

        return pressure;
    }

    private string ResolveRpgRecoveryTriggerKey(PrototypeRpgMemberLaneRecoveryRuntimeState state, PrototypeRpgGrowthChoiceMemberContext member)
    {
        if (state == null)
        {
            return string.Empty;
        }

        if (state.ConsecutiveKnockOutCount >= 2)
        {
            return "repeat_knockout";
        }
        if (state.ConsecutiveDefeatCount >= 2)
        {
            return "repeat_defeat";
        }
        if (state.ConsecutiveRetreatCount >= 2)
        {
            return "repeat_retreat";
        }
        if (state.ConsecutiveStalledOfferCount >= 2)
        {
            return "stalled_progression";
        }
        if (state.ConsecutiveNoImprovementRunCount >= 2 && (member != null ? member.CommitmentDepth : 0) >= 3)
        {
            return "lane_trap";
        }
        if (state.RecoveryPressureScore >= 12 && (member != null ? member.CommitmentDepth : 0) >= 4)
        {
            return "trap_pressure";
        }
        if (state.RecoveryPressureScore >= 8)
        {
            return "soft_respec_pressure";
        }

        return string.Empty;
    }

    private int ResolveSoftRespecWindowState(PrototypeRpgMemberLaneRecoveryRuntimeState state, PrototypeRpgGrowthChoiceMemberContext member, string triggerKey)
    {
        if (state == null)
        {
            return 0;
        }

        int tier = 0;
        if (state.RecoveryPressureScore >= 14 || state.ConsecutiveKnockOutCount >= 2 || state.ConsecutiveStalledOfferCount >= 2)
        {
            tier = 2;
        }
        else if (state.RecoveryPressureScore >= 8 || state.ConsecutiveDefeatCount >= 2 || state.ConsecutiveRetreatCount >= 2 || state.ConsecutiveNoImprovementRunCount >= 2 || !string.IsNullOrEmpty(triggerKey))
        {
            tier = 1;
        }

        if (member != null && member.Survived && !member.KnockedOut && state.RecoveryPressureScore <= 4)
        {
            tier = 0;
        }

        return tier;
    }

    private string ResolveRpgRecoveryRescueGroupKey(PrototypeRpgGrowthChoiceMemberContext member)
    {
        string roleTag = member != null ? member.RoleTag : string.Empty;
        switch (string.IsNullOrEmpty(roleTag) ? string.Empty : roleTag.Trim().ToLowerInvariant())
        {
            case "warrior":
                return "warrior_path";
            case "rogue":
                return "rogue_path";
            case "mage":
                return "mage_path";
            case "cleric":
                return "cleric_path";
            default:
                return string.Empty;
        }
    }

    private string[] BuildRpgRecoveryAdjacentLaneKeys(PrototypeRpgGrowthChoiceMemberContext member)
    {
        List<string> keys = new List<string>();
        if (member == null)
        {
            return Array.Empty<string>();
        }

        string roleAdjacentLane = PrototypeRpgGrowthChoiceRuleCatalog.ResolveRoleAdjacentLaneKey(member.RoleTag);
        if (!string.IsNullOrEmpty(member.ResolvedSecondaryLaneKey) && !ContainsRpgRecoveryLaneKey(keys, member.ResolvedSecondaryLaneKey))
        {
            keys.Add(member.ResolvedSecondaryLaneKey);
        }
        if (!string.IsNullOrEmpty(roleAdjacentLane) && !ContainsRpgRecoveryLaneKey(keys, roleAdjacentLane))
        {
            keys.Add(roleAdjacentLane);
        }
        if (member.ResolvedPrimaryLaneKey != "recovery" && !ContainsRpgRecoveryLaneKey(keys, "recovery"))
        {
            keys.Add("recovery");
        }
        if (keys.Count <= 0)
        {
            string fallbackLane = PrototypeRpgGrowthChoiceRuleCatalog.ResolveRoleCoreLaneKey(member.RoleTag);
            if (!string.IsNullOrEmpty(fallbackLane))
            {
                keys.Add(fallbackLane);
            }
        }

        return keys.Count <= 0 ? Array.Empty<string>() : keys.ToArray();
    }

    private bool ContainsRpgRecoveryLaneKey(List<string> values, string laneKey)
    {
        if (values == null || string.IsNullOrEmpty(laneKey))
        {
            return false;
        }

        for (int i = 0; i < values.Count; i++)
        {
            if (!string.IsNullOrEmpty(values[i]) && values[i] == laneKey)
            {
                return true;
            }
        }

        return false;
    }
    private string ResolveRpgRecoveryTriggerLabel(string triggerKey)
    {
        switch (string.IsNullOrEmpty(triggerKey) ? string.Empty : triggerKey.Trim().ToLowerInvariant())
        {
            case "repeat_knockout":
                return "Repeated KO";
            case "repeat_defeat":
                return "Repeated defeat";
            case "repeat_retreat":
                return "Repeated retreat";
            case "stalled_progression":
                return "Stalled progression";
            case "lane_trap":
                return "Lane trap";
            case "trap_pressure":
                return "Trap pressure";
            case "soft_respec_pressure":
                return "Recovery pressure";
            default:
                return "Recovery pressure";
        }
    }

    private string BuildRpgMemberRecoveryReasonText(PrototypeRpgMemberLaneRecoveryRuntimeState state, PrototypeRpgGrowthChoiceMemberContext member, bool meaningfulImprovement, bool noImprovement, bool stalledOffer)
    {
        if (state == null)
        {
            return string.Empty;
        }

        string displayName = string.IsNullOrEmpty(member != null ? member.DisplayName : string.Empty)
            ? (string.IsNullOrEmpty(state.DisplayName) ? "Adventurer" : state.DisplayName)
            : member.DisplayName;
        string primaryLabel = PrototypeRpgGrowthChoiceRuleCatalog.ResolveLaneLabel(member != null ? member.ResolvedPrimaryLaneKey : string.Empty);
        string secondaryLabel = PrototypeRpgGrowthChoiceRuleCatalog.ResolveLaneLabel(member != null ? member.ResolvedSecondaryLaneKey : string.Empty);
        string triggerLabel = ResolveRpgRecoveryTriggerLabel(state.LastRecoveryTriggerKey);
        if (state.SoftRespecWindowOpen)
        {
            return displayName + ": " + triggerLabel + " opened Tier " + state.SoftRespecWindowTier + " soft respec toward " + secondaryLabel + " / Recovery. Pressure " + state.RecoveryPressureScore + ".";
        }
        if (meaningfulImprovement)
        {
            return displayName + ": stable clear cooled the previous " + primaryLabel + " pressure down to " + state.RecoveryPressureScore + ".";
        }
        if (stalledOffer)
        {
            return displayName + ": deep " + primaryLabel + " commitment stalled, so adjacent " + secondaryLabel + " and recovery paths are being watched.";
        }
        if (noImprovement && state.RecoveryPressureScore > 0)
        {
            return displayName + ": recovery pressure sits at " + state.RecoveryPressureScore + " while the " + primaryLabel + " lane is re-evaluated.";
        }

        return displayName + ": recovery pressure " + state.RecoveryPressureScore + ".";
    }

    private string BuildRpgMemberRecoverySafetySummary(PrototypeRpgMemberLaneRecoveryRuntimeState state, PrototypeRpgGrowthChoiceMemberContext member)
    {
        if (state == null)
        {
            return string.Empty;
        }

        string displayName = string.IsNullOrEmpty(member != null ? member.DisplayName : string.Empty)
            ? (string.IsNullOrEmpty(state.DisplayName) ? "Adventurer" : state.DisplayName)
            : member.DisplayName;
        string windowText = state.SoftRespecWindowOpen ? "Tier " + state.SoftRespecWindowTier + " open" : "closed";
        return displayName + ": pressure " + state.RecoveryPressureScore + " | soft respec " + windowText + ".";
    }

    private string BuildRpgLaneRecoverySummaryText(PrototypeRpgMemberLaneRecoveryRuntimeState[] members)
    {
        if (members == null || members.Length <= 0)
        {
            return "Lane recovery: stable.";
        }

        int pressuredCount = 0;
        int highestPressure = 0;
        string highestMember = string.Empty;
        for (int i = 0; i < members.Length; i++)
        {
            PrototypeRpgMemberLaneRecoveryRuntimeState member = members[i];
            if (member == null || member.RecoveryPressureScore <= 0)
            {
                continue;
            }

            pressuredCount += 1;
            if (member.RecoveryPressureScore > highestPressure)
            {
                highestPressure = member.RecoveryPressureScore;
                highestMember = string.IsNullOrEmpty(member.DisplayName) ? "Adventurer" : member.DisplayName;
            }
        }

        if (pressuredCount <= 0)
        {
            return "Lane recovery: stable.";
        }

        return "Lane recovery: " + pressuredCount + " member(s) under pressure. Highest " + highestMember + " @" + highestPressure + ".";
    }

    private string BuildRpgSoftRespecWindowSummaryText(PrototypeRpgMemberLaneRecoveryRuntimeState[] members)
    {
        if (members == null || members.Length <= 0)
        {
            return "Soft respec windows: closed.";
        }

        int tierOneCount = 0;
        int tierTwoCount = 0;
        for (int i = 0; i < members.Length; i++)
        {
            PrototypeRpgMemberLaneRecoveryRuntimeState member = members[i];
            if (member == null || !member.SoftRespecWindowOpen)
            {
                continue;
            }

            if (member.SoftRespecWindowTier >= 2)
            {
                tierTwoCount += 1;
            }
            else
            {
                tierOneCount += 1;
            }
        }

        int total = tierOneCount + tierTwoCount;
        return total <= 0
            ? "Soft respec windows: closed."
            : "Soft respec windows: " + total + " open (Tier 1 x" + tierOneCount + ", Tier 2 x" + tierTwoCount + ").";
    }

    private string BuildRpgAntiTrapSafetySummaryText(PrototypeRpgMemberLaneRecoveryRuntimeState[] members)
    {
        if (members == null || members.Length <= 0)
        {
            return "Anti-trap safety: lane lock stays stable.";
        }

        int openCount = 0;
        int tierTwoCount = 0;
        for (int i = 0; i < members.Length; i++)
        {
            PrototypeRpgMemberLaneRecoveryRuntimeState member = members[i];
            if (member == null || !member.SoftRespecWindowOpen)
            {
                continue;
            }

            openCount += 1;
            if (member.SoftRespecWindowTier >= 2)
            {
                tierTwoCount += 1;
            }
        }

        if (openCount <= 0)
        {
            return "Anti-trap safety: lane lock stays stable.";
        }

        return "Anti-trap safety: " + openCount + " member(s) reopened adjacent or recovery offers, including " + tierTwoCount + " stronger rescue window(s).";
    }

    private void RefreshRpgLaneRecoveryStateConsistency(PrototypeRpgGrowthChoiceContext context, PrototypeRpgPartyLaneRecoveryRuntimeState partyState)
    {
        if (partyState == null)
        {
            return;
        }

        PrototypeRpgMemberLaneRecoveryRuntimeState[] members = partyState.Members ?? Array.Empty<PrototypeRpgMemberLaneRecoveryRuntimeState>();
        partyState.LaneRecoverySummaryText = BuildRpgLaneRecoverySummaryText(members);
        partyState.SoftRespecWindowSummaryText = BuildRpgSoftRespecWindowSummaryText(members);
        partyState.AntiTrapSafetySummaryText = BuildRpgAntiTrapSafetySummaryText(members);
        if (context == null)
        {
            return;
        }

        context.LaneRecoverySummaryText = partyState.LaneRecoverySummaryText;
        context.SoftRespecWindowSummaryText = partyState.SoftRespecWindowSummaryText;
        context.AntiTrapSafetySummaryText = partyState.AntiTrapSafetySummaryText;
        context.MemberLaneRecoveryStates = CopyRpgMemberLaneRecoveryRuntimeStates(members);
        PrototypeRpgGrowthChoiceMemberContext[] contextMembers = context.Members ?? Array.Empty<PrototypeRpgGrowthChoiceMemberContext>();
        for (int i = 0; i < contextMembers.Length; i++)
        {
            PrototypeRpgGrowthChoiceMemberContext member = contextMembers[i];
            PrototypeRpgMemberLaneRecoveryRuntimeState state = GetRpgMemberLaneRecoveryRuntimeState(partyState, member != null ? member.MemberId : string.Empty);
            if (member == null || state == null)
            {
                continue;
            }

            member.RecoveryPressureScore = state.RecoveryPressureScore;
            member.ConsecutiveDefeatCount = state.ConsecutiveDefeatCount;
            member.ConsecutiveRetreatCount = state.ConsecutiveRetreatCount;
            member.ConsecutiveKnockOutCount = state.ConsecutiveKnockOutCount;
            member.ConsecutiveStalledOfferCount = state.ConsecutiveStalledOfferCount;
            member.ConsecutiveNoImprovementRunCount = state.ConsecutiveNoImprovementRunCount;
            member.SoftRespecWindowOpen = state.SoftRespecWindowOpen;
            member.SoftRespecWindowTier = state.SoftRespecWindowTier;
            member.RecoveryWindowSourceRunIdentity = string.IsNullOrEmpty(state.RecoveryWindowSourceRunIdentity) ? string.Empty : state.RecoveryWindowSourceRunIdentity;
            member.RecentRecoveryReasonText = string.IsNullOrEmpty(state.RecentRecoveryReasonText) ? string.Empty : state.RecentRecoveryReasonText;
            member.LastRecoveryTriggerKey = string.IsNullOrEmpty(state.LastRecoveryTriggerKey) ? string.Empty : state.LastRecoveryTriggerKey;
            member.LastRecoveryResolvedRunIdentity = string.IsNullOrEmpty(state.LastRecoveryResolvedRunIdentity) ? string.Empty : state.LastRecoveryResolvedRunIdentity;
            member.RecoveryBiasWeight = state.RecoveryBiasWeight;
            member.LaneLockRelaxWeight = state.LaneLockRelaxWeight;
            member.RescueOfferGroupKey = string.IsNullOrEmpty(state.RescueOfferGroupKey) ? string.Empty : state.RescueOfferGroupKey;
            member.RescueAdjacentLaneKeys = CopyRpgStringArray(state.RescueAdjacentLaneKeys);
            member.RecoverySafetySummaryText = string.IsNullOrEmpty(state.RecoverySafetySummaryText) ? string.Empty : state.RecoverySafetySummaryText;
        }
    }
    
    private int ScoreOfferAgainstRecoveryWindow(PrototypeRpgGrowthChoiceContext context, PrototypeRpgUpgradeOfferCandidate candidate, PrototypeRpgGrowthChoiceMemberContext member)
    {
        if (candidate == null || member == null)
        {
            return 0;
        }

        candidate.RecoveryTriggerKeyAtSelection = string.IsNullOrEmpty(member.LastRecoveryTriggerKey) ? string.Empty : member.LastRecoveryTriggerKey;
        candidate.RecoveryWindowTierAtSelection = Math.Max(0, member.SoftRespecWindowTier);
        candidate.MatchesRecoveryRescueGroup = !string.IsNullOrEmpty(member.RescueOfferGroupKey) && candidate.OfferGroupId == member.RescueOfferGroupKey;
        bool adjacentRecoveryLane = ArrayContainsRpgValue(member.RescueAdjacentLaneKeys, candidate.LaneKey) || ArrayContainsRpgValue(member.RescueAdjacentLaneKeys, candidate.AdjacentLaneKey);
        candidate.IsRescueOffer = candidate.MatchesRecoveryRescueGroup || candidate.LaneKey == "recovery" || candidate.IsRecoveryEscapeOffer;
        candidate.SupportsRecoveryWindow = member.SoftRespecWindowOpen && (candidate.IsRescueOffer || adjacentRecoveryLane || candidate.IsAdjacentLaneOffer || candidate.IsRecoveryEscapeOffer);
        if (!member.SoftRespecWindowOpen)
        {
            candidate.RecoveryBiasScore = 0;
            candidate.LaneLockRelaxed = false;
            return 0;
        }

        int score = 0;
        if (candidate.IsRescueOffer)
        {
            score += member.RecoveryBiasWeight + 4;
        }
        if (candidate.MatchesRecoveryRescueGroup)
        {
            score += 6;
        }
        if (candidate.LaneKey == "recovery" || candidate.IsRecoveryEscapeOffer)
        {
            score += 8 + member.SoftRespecWindowTier * 2;
        }
        if (adjacentRecoveryLane || candidate.IsAdjacentLaneOffer)
        {
            score += 5 + member.LaneLockRelaxWeight;
        }
        if (candidate.LaneKey == member.ResolvedPrimaryLaneKey && member.CommitmentDepth >= 4 && !candidate.IsRecoveryEscapeOffer)
        {
            score -= 4 + member.LaneLockRelaxWeight / 2;
        }
        if (candidate.IsContradictoryToLane && !adjacentRecoveryLane && !candidate.IsRescueOffer && !candidate.IsRecoveryEscapeOffer)
        {
            score -= 10;
        }

        candidate.RecoveryBiasScore = score;
        return score;
    }

    private bool RelaxLaneLockForRecoveryWindow(PrototypeRpgGrowthChoiceContext context, PrototypeRpgUpgradeOfferCandidate candidate, PrototypeRpgGrowthChoiceMemberContext member)
    {
        if (candidate == null || member == null || !member.SoftRespecWindowOpen)
        {
            return false;
        }

        if (candidate.IsFallback || candidate.IsNoOp)
        {
            return false;
        }

        if (candidate.LaneKey == member.ResolvedPrimaryLaneKey || candidate.LaneKey == member.ResolvedSecondaryLaneKey)
        {
            return true;
        }

        if (candidate.IsRecoveryEscapeOffer || candidate.LaneKey == "recovery")
        {
            return true;
        }

        if (!string.IsNullOrEmpty(member.RescueOfferGroupKey) && candidate.OfferGroupId == member.RescueOfferGroupKey)
        {
            return true;
        }

        if (ArrayContainsRpgValue(member.RescueAdjacentLaneKeys, candidate.LaneKey) || ArrayContainsRpgValue(member.RescueAdjacentLaneKeys, candidate.AdjacentLaneKey))
        {
            return true;
        }

        return member.SoftRespecWindowTier >= 2 && candidate.SidegradeAllowance >= 2;
    }

    private void PromoteRpgRecoveryAdjacentUtilityOffers(List<PrototypeRpgUpgradeOfferCandidate> candidates, PrototypeRpgGrowthChoiceMemberContext member)
    {
        if (candidates == null || member == null || !member.SoftRespecWindowOpen)
        {
            return;
        }

        for (int i = 0; i < candidates.Count; i++)
        {
            PrototypeRpgUpgradeOfferCandidate candidate = candidates[i];
            if (candidate == null)
            {
                continue;
            }

            bool rescueGroup = !string.IsNullOrEmpty(member.RescueOfferGroupKey) && candidate.OfferGroupId == member.RescueOfferGroupKey;
            bool adjacentRecoveryLane = ArrayContainsRpgValue(member.RescueAdjacentLaneKeys, candidate.LaneKey) || ArrayContainsRpgValue(member.RescueAdjacentLaneKeys, candidate.AdjacentLaneKey);
            if (rescueGroup || candidate.LaneKey == "recovery" || adjacentRecoveryLane)
            {
                candidate.RecoveryEscapeWeight += Math.Max(2, member.RecoveryBiasWeight / 2);
                candidate.SidegradeAllowance += Math.Max(1, member.LaneLockRelaxWeight / 2);
                candidate.MatchesRecoveryRescueGroup = rescueGroup;
                candidate.IsRescueOffer = rescueGroup || candidate.LaneKey == "recovery" || candidate.IsRecoveryEscapeOffer;
                if (string.IsNullOrEmpty(candidate.RecoveryReasonText))
                {
                    candidate.RecoveryReasonText = BuildRpgRecoveryOfferReasonText(null, candidate, member);
                }
            }
        }
    }

    private string BuildRpgRecoveryOfferReasonText(PrototypeRpgGrowthChoiceContext context, PrototypeRpgUpgradeOfferCandidate candidate, PrototypeRpgGrowthChoiceMemberContext member)
    {
        if (candidate == null)
        {
            return string.Empty;
        }

        if (member == null)
        {
            return string.IsNullOrEmpty(candidate.RecoveryReasonText) ? string.Empty : candidate.RecoveryReasonText;
        }

        string triggerLabel = ResolveRpgRecoveryTriggerLabel(member.LastRecoveryTriggerKey);
        string primaryLabel = PrototypeRpgGrowthChoiceRuleCatalog.ResolveLaneLabel(member.ResolvedPrimaryLaneKey);
        string secondaryLabel = PrototypeRpgGrowthChoiceRuleCatalog.ResolveLaneLabel(member.ResolvedSecondaryLaneKey);
        string candidateLaneLabel = PrototypeRpgGrowthChoiceRuleCatalog.ResolveLaneLabel(candidate.LaneKey);
        bool adjacentRecoveryLane = ArrayContainsRpgValue(member.RescueAdjacentLaneKeys, candidate.LaneKey) || ArrayContainsRpgValue(member.RescueAdjacentLaneKeys, candidate.AdjacentLaneKey);
        if (!member.SoftRespecWindowOpen)
        {
            return string.IsNullOrEmpty(member.RecentRecoveryReasonText)
                ? string.Empty
                : member.RecentRecoveryReasonText;
        }

        if (candidate.IsRescueOffer || candidate.MatchesRecoveryRescueGroup)
        {
            return triggerLabel + ": rescue offer reopens a safer " + (string.IsNullOrEmpty(candidateLaneLabel) ? "Recovery" : candidateLaneLabel) + " pivot beside " + primaryLabel + ".";
        }

        if (candidate.LaneKey == "recovery" || candidate.IsRecoveryEscapeOffer)
        {
            return triggerLabel + ": safety pivot keeps Recovery online while pressure stays at " + member.RecoveryPressureScore + ".";
        }

        if (adjacentRecoveryLane || candidate.IsAdjacentLaneOffer)
        {
            return triggerLabel + ": soft respec relaxes the lane lock from " + primaryLabel + " toward " + secondaryLabel + ".";
        }

        if (candidate.LaneKey == member.ResolvedPrimaryLaneKey)
        {
            return triggerLabel + ": primary lane can stay, but only with an anti-trap guarded step.";
        }

        return triggerLabel + ": anti-trap guard keeps " + (string.IsNullOrEmpty(candidateLaneLabel) ? "adjacent utility" : candidateLaneLabel) + " available beside " + primaryLabel + ".";
    }

    private void ApplyRpgRecoveryChoiceResolution(PrototypeRpgMemberLaneRecoveryRuntimeState recoveryMember, PrototypeRpgUpgradeOfferCandidate offer, PrototypeRpgGrowthChoiceContext context)
    {
        if (recoveryMember == null)
        {
            return;
        }

        string runIdentity = context != null && !string.IsNullOrEmpty(context.RunIdentity) ? context.RunIdentity : string.Empty;
        recoveryMember.LastRecoveryResolvedRunIdentity = runIdentity;
        if (offer == null)
        {
            return;
        }

        bool usedRecoveryWindow = recoveryMember.SoftRespecWindowOpen;
        bool resolvedByRecovery = offer.IsRescueOffer || offer.MatchesRecoveryRescueGroup || offer.SupportsRecoveryWindow || offer.IsRecoveryEscapeOffer || offer.LaneLockRelaxed;
        if (!usedRecoveryWindow)
        {
            if (!string.IsNullOrEmpty(offer.RecoveryReasonText))
            {
                recoveryMember.RecentRecoveryReasonText = offer.RecoveryReasonText;
            }
            return;
        }

        if (resolvedByRecovery)
        {
            int pressureDrop = 4 + Math.Max(0, recoveryMember.SoftRespecWindowTier * 2) + Math.Max(0, offer.RecoveryBiasScore / 6);
            recoveryMember.RecoveryPressureScore = Math.Max(0, recoveryMember.RecoveryPressureScore - pressureDrop);
            recoveryMember.ConsecutiveStalledOfferCount = Math.Max(0, recoveryMember.ConsecutiveStalledOfferCount - 1);
            recoveryMember.ConsecutiveNoImprovementRunCount = Math.Max(0, recoveryMember.ConsecutiveNoImprovementRunCount - 1);
            recoveryMember.LaneLockRelaxWeight = Math.Max(0, recoveryMember.LaneLockRelaxWeight - 2);
            recoveryMember.RecoveryBiasWeight = Math.Max(0, recoveryMember.RecoveryBiasWeight - 2);
            if (recoveryMember.RecoveryPressureScore <= 4)
            {
                recoveryMember.SoftRespecWindowOpen = false;
                recoveryMember.SoftRespecWindowTier = 0;
            }
            else if (recoveryMember.SoftRespecWindowTier > 1 && recoveryMember.RecoveryPressureScore <= 8)
            {
                recoveryMember.SoftRespecWindowTier = 1;
                recoveryMember.SoftRespecWindowOpen = true;
            }
            recoveryMember.RecentRecoveryReasonText = string.IsNullOrEmpty(offer.RecoveryReasonText)
                ? "Recovery window consumed by " + offer.OfferLabel + "."
                : offer.RecoveryReasonText;
        }
        else if (offer.IsNoOp)
        {
            recoveryMember.ConsecutiveStalledOfferCount += 1;
            recoveryMember.RecoveryPressureScore = Math.Min(30, recoveryMember.RecoveryPressureScore + 1);
            recoveryMember.RecentRecoveryReasonText = "Recovery window stayed open because continuity hold did not break the trap yet.";
        }
        else
        {
            recoveryMember.RecoveryPressureScore = Math.Max(0, recoveryMember.RecoveryPressureScore - 1);
            recoveryMember.RecentRecoveryReasonText = string.IsNullOrEmpty(offer.RecoveryReasonText)
                ? "Recovery state re-evaluated after " + offer.OfferLabel + "."
                : offer.RecoveryReasonText;
        }

        recoveryMember.RecoverySafetySummaryText = BuildRpgMemberRecoverySafetySummary(recoveryMember, null);
    }
}
