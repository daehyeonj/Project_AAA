using System;
using UnityEngine;

public sealed partial class StaticPlaceholderWorldView
{
    private PrototypeRpgOfferMomentumRuntimeState _sessionRpgOfferMomentumRuntimeState = new PrototypeRpgOfferMomentumRuntimeState();

    public string CurrentStreakSensitivePacingSummaryText => string.IsNullOrEmpty(_sessionRpgOfferMomentumRuntimeState.StreakSensitivePacingSummaryText) ? "None" : _sessionRpgOfferMomentumRuntimeState.StreakSensitivePacingSummaryText;
    public string CurrentClearStreakDampeningSummaryText => string.IsNullOrEmpty(_sessionRpgOfferMomentumRuntimeState.ClearStreakDampeningSummaryText) ? "None" : _sessionRpgOfferMomentumRuntimeState.ClearStreakDampeningSummaryText;
    public string CurrentComebackReliefSummaryText => string.IsNullOrEmpty(_sessionRpgOfferMomentumRuntimeState.ComebackReliefSummaryText) ? "None" : _sessionRpgOfferMomentumRuntimeState.ComebackReliefSummaryText;
    public string CurrentMomentumBalancingSummaryText => string.IsNullOrEmpty(_sessionRpgOfferMomentumRuntimeState.MomentumBalancingSummaryText) ? "None" : _sessionRpgOfferMomentumRuntimeState.MomentumBalancingSummaryText;
    public string CurrentMomentumTierSummaryText => string.IsNullOrEmpty(_sessionRpgOfferMomentumRuntimeState.CurrentMomentumTierSummaryText) ? "None" : _sessionRpgOfferMomentumRuntimeState.CurrentMomentumTierSummaryText;
    public string CurrentNextOfferIntensityHintText => string.IsNullOrEmpty(_sessionRpgOfferMomentumRuntimeState.NextOfferIntensityHintText) ? "None" : _sessionRpgOfferMomentumRuntimeState.NextOfferIntensityHintText;
    public PrototypeRpgOfferMomentumRuntimeState LatestRpgOfferMomentumRuntimeState => CopyRpgOfferMomentumRuntimeState(_sessionRpgOfferMomentumRuntimeState);

    private void ResetRpgOfferMomentumRuntimeState()
    {
        _sessionRpgOfferMomentumRuntimeState = new PrototypeRpgOfferMomentumRuntimeState();
    }

    private PrototypeRpgOfferMomentumRuntimeState CopyRpgOfferMomentumRuntimeState(PrototypeRpgOfferMomentumRuntimeState source)
    {
        PrototypeRpgOfferMomentumRuntimeState copy = new PrototypeRpgOfferMomentumRuntimeState();
        if (source == null)
        {
            return copy;
        }

        copy.SessionKey = CopyText(source.SessionKey);
        copy.PartyId = CopyText(source.PartyId);
        copy.DisplayName = CopyText(source.DisplayName);
        copy.LastRunIdentity = CopyText(source.LastRunIdentity);
        copy.RecentRunCount = ClampNonNegative(source.RecentRunCount);
        copy.ConsecutiveClearCount = ClampNonNegative(source.ConsecutiveClearCount);
        copy.ConsecutiveDefeatCount = ClampNonNegative(source.ConsecutiveDefeatCount);
        copy.ConsecutiveRetreatCount = ClampNonNegative(source.ConsecutiveRetreatCount);
        copy.RecentLowValueOfferCount = ClampNonNegative(source.RecentLowValueOfferCount);
        copy.RecentHighImpactOfferCount = ClampNonNegative(source.RecentHighImpactOfferCount);
        copy.CurrentMomentumTier = ClampNonNegative(source.CurrentMomentumTier);
        copy.ComebackReliefWindowCount = ClampNonNegative(source.ComebackReliefWindowCount);
        copy.MomentumDampeningWeight = ClampNonNegative(source.MomentumDampeningWeight);
        copy.LastMomentumReasonKey = CopyText(source.LastMomentumReasonKey);
        copy.StreakSensitivePacingSummaryText = CopyText(source.StreakSensitivePacingSummaryText);
        copy.ClearStreakDampeningSummaryText = CopyText(source.ClearStreakDampeningSummaryText);
        copy.ComebackReliefSummaryText = CopyText(source.ComebackReliefSummaryText);
        copy.MomentumBalancingSummaryText = CopyText(source.MomentumBalancingSummaryText);
        copy.CurrentMomentumTierSummaryText = CopyText(source.CurrentMomentumTierSummaryText);
        copy.NextOfferIntensityHintText = CopyText(source.NextOfferIntensityHintText);
        copy.Members = CopyRpgMemberOfferMomentumBiasStates(source.Members);
        return copy;
    }

    private PrototypeRpgMemberOfferMomentumBiasState[] CopyRpgMemberOfferMomentumBiasStates(PrototypeRpgMemberOfferMomentumBiasState[] source)
    {
        return CopyArray(source, CopyRpgMemberOfferMomentumBiasState);
    }

    private PrototypeRpgMemberOfferMomentumBiasState CopyRpgMemberOfferMomentumBiasState(PrototypeRpgMemberOfferMomentumBiasState source)
    {
        PrototypeRpgMemberOfferMomentumBiasState copy = new PrototypeRpgMemberOfferMomentumBiasState();
        if (source == null)
        {
            return copy;
        }

        copy.MemberId = CopyText(source.MemberId);
        copy.DisplayName = CopyText(source.DisplayName);
        copy.RecentRunCount = ClampNonNegative(source.RecentRunCount);
        copy.ConsecutiveClearCount = ClampNonNegative(source.ConsecutiveClearCount);
        copy.ConsecutiveDefeatCount = ClampNonNegative(source.ConsecutiveDefeatCount);
        copy.ConsecutiveRetreatCount = ClampNonNegative(source.ConsecutiveRetreatCount);
        copy.RecentLowValueOfferCount = ClampNonNegative(source.RecentLowValueOfferCount);
        copy.RecentHighImpactOfferCount = ClampNonNegative(source.RecentHighImpactOfferCount);
        copy.CurrentMomentumTier = ClampNonNegative(source.CurrentMomentumTier);
        copy.ComebackReliefWindowCount = ClampNonNegative(source.ComebackReliefWindowCount);
        copy.MomentumDampeningWeight = ClampNonNegative(source.MomentumDampeningWeight);
        copy.IntensityBiasWeight = source.IntensityBiasWeight;
        copy.ComebackReliefBiasWeight = ClampNonNegative(source.ComebackReliefBiasWeight);
        copy.LastMomentumReasonKey = CopyText(source.LastMomentumReasonKey);
        copy.MomentumMemberSummaryText = CopyText(source.MomentumMemberSummaryText);
        copy.LastResolvedRunIdentity = CopyText(source.LastResolvedRunIdentity);
        return copy;
    }

    private PrototypeRpgOfferMomentumRuntimeState GetOrCreateRpgOfferMomentumRuntimeState(PrototypeRpgPartyDefinition partyDefinition)
    {
        if (partyDefinition == null)
        {
            return _sessionRpgOfferMomentumRuntimeState ?? new PrototypeRpgOfferMomentumRuntimeState();
        }

        bool rebuildState = _sessionRpgOfferMomentumRuntimeState == null ||
                            _sessionRpgOfferMomentumRuntimeState.Members == null ||
                            _sessionRpgOfferMomentumRuntimeState.PartyId != partyDefinition.PartyId;
        if (rebuildState)
        {
            _sessionRpgOfferMomentumRuntimeState = new PrototypeRpgOfferMomentumRuntimeState();
            _sessionRpgOfferMomentumRuntimeState.SessionKey = string.IsNullOrEmpty(partyDefinition.PartyId) ? string.Empty : partyDefinition.PartyId;
            _sessionRpgOfferMomentumRuntimeState.PartyId = string.IsNullOrEmpty(partyDefinition.PartyId) ? string.Empty : partyDefinition.PartyId;
            _sessionRpgOfferMomentumRuntimeState.DisplayName = string.IsNullOrEmpty(partyDefinition.DisplayName) ? _sessionRpgOfferMomentumRuntimeState.PartyId : partyDefinition.DisplayName;
            _sessionRpgOfferMomentumRuntimeState.Members = BuildDefaultRpgMemberOfferMomentumBiasStates(partyDefinition);
        }
        else
        {
            EnsureRpgMemberOfferMomentumBiasStates(partyDefinition, _sessionRpgOfferMomentumRuntimeState);
        }

        return _sessionRpgOfferMomentumRuntimeState;
    }

    private PrototypeRpgMemberOfferMomentumBiasState[] BuildDefaultRpgMemberOfferMomentumBiasStates(PrototypeRpgPartyDefinition partyDefinition)
    {
        PrototypeRpgPartyMemberDefinition[] definitions = partyDefinition != null ? (partyDefinition.Members ?? Array.Empty<PrototypeRpgPartyMemberDefinition>()) : Array.Empty<PrototypeRpgPartyMemberDefinition>();
        if (definitions.Length <= 0)
        {
            return Array.Empty<PrototypeRpgMemberOfferMomentumBiasState>();
        }

        PrototypeRpgMemberOfferMomentumBiasState[] members = new PrototypeRpgMemberOfferMomentumBiasState[definitions.Length];
        for (int i = 0; i < definitions.Length; i++)
        {
            PrototypeRpgPartyMemberDefinition definition = definitions[i];
            PrototypeRpgMemberOfferMomentumBiasState state = new PrototypeRpgMemberOfferMomentumBiasState();
            state.MemberId = string.IsNullOrEmpty(definition != null ? definition.MemberId : string.Empty) ? string.Empty : definition.MemberId;
            state.DisplayName = string.IsNullOrEmpty(definition != null ? definition.DisplayName : string.Empty) ? "Adventurer" : definition.DisplayName;
            members[i] = state;
        }

        return members;
    }

    private void EnsureRpgMemberOfferMomentumBiasStates(PrototypeRpgPartyDefinition partyDefinition, PrototypeRpgOfferMomentumRuntimeState state)
    {
        if (partyDefinition == null || state == null)
        {
            return;
        }

        PrototypeRpgPartyMemberDefinition[] definitions = partyDefinition.Members ?? Array.Empty<PrototypeRpgPartyMemberDefinition>();
        PrototypeRpgMemberOfferMomentumBiasState[] currentMembers = state.Members ?? Array.Empty<PrototypeRpgMemberOfferMomentumBiasState>();
        if (definitions.Length == currentMembers.Length)
        {
            return;
        }

        PrototypeRpgMemberOfferMomentumBiasState[] rebuilt = new PrototypeRpgMemberOfferMomentumBiasState[definitions.Length];
        for (int i = 0; i < definitions.Length; i++)
        {
            PrototypeRpgPartyMemberDefinition definition = definitions[i];
            PrototypeRpgMemberOfferMomentumBiasState existing = GetRpgMemberOfferMomentumBiasState(state, definition != null ? definition.MemberId : string.Empty);
            if (existing == null)
            {
                existing = new PrototypeRpgMemberOfferMomentumBiasState();
                existing.MemberId = string.IsNullOrEmpty(definition != null ? definition.MemberId : string.Empty) ? string.Empty : definition.MemberId;
                existing.DisplayName = string.IsNullOrEmpty(definition != null ? definition.DisplayName : string.Empty) ? "Adventurer" : definition.DisplayName;
            }

            rebuilt[i] = existing;
        }

        state.Members = rebuilt;
    }

    private PrototypeRpgMemberOfferMomentumBiasState GetRpgMemberOfferMomentumBiasState(PrototypeRpgOfferMomentumRuntimeState state, string memberId)
    {
        if (state == null || state.Members == null || string.IsNullOrEmpty(memberId))
        {
            return null;
        }

        for (int i = 0; i < state.Members.Length; i++)
        {
            PrototypeRpgMemberOfferMomentumBiasState member = state.Members[i];
            if (member != null && member.MemberId == memberId)
            {
                return member;
            }
        }

        return null;
    }

    private void EvaluateRpgOfferMomentumState(PrototypeRpgGrowthChoiceContext context, PrototypeRpgRunResultSnapshot runResultSnapshot, PrototypeRpgPartyDefinition partyDefinition)
    {
        if (context == null || partyDefinition == null)
        {
            return;
        }

        PrototypeRpgOfferMomentumRuntimeState state = GetOrCreateRpgOfferMomentumRuntimeState(partyDefinition);
        string runIdentity = string.IsNullOrEmpty(context.RunIdentity) ? string.Empty : context.RunIdentity;
        bool processRun = !string.IsNullOrEmpty(runIdentity) && state.LastRunIdentity != runIdentity;
        if (processRun)
        {
            string resultStateKey = runResultSnapshot != null && !string.IsNullOrEmpty(runResultSnapshot.ResultStateKey)
                ? runResultSnapshot.ResultStateKey
                : context.ResultStateKey;
            UpdateRpgOfferMomentumResultStreak(state, resultStateKey);
            UpdateRpgOfferMomentumPressure(state, _latestRpgPostRunUpgradeOfferSurface);
            state.CurrentMomentumTier = ResolveRpgOfferMomentumTier(state);
            state.ComebackReliefWindowCount = ResolveRpgComebackReliefWindowCount(state);
            state.MomentumDampeningWeight = ResolveRpgMomentumDampeningWeight(state);
            state.LastMomentumReasonKey = ResolveRpgMomentumReasonKey(state);
            state.LastRunIdentity = runIdentity;
        }

        PrototypeRpgGrowthChoiceMemberContext[] members = context.Members ?? Array.Empty<PrototypeRpgGrowthChoiceMemberContext>();
        for (int i = 0; i < members.Length; i++)
        {
            PrototypeRpgGrowthChoiceMemberContext member = members[i];
            PrototypeRpgMemberOfferMomentumBiasState memberState = GetRpgMemberOfferMomentumBiasState(state, member != null ? member.MemberId : string.Empty);
            if (member == null || memberState == null)
            {
                continue;
            }

            if (processRun)
            {
                PrototypeRpgUpgradeOfferCandidate previousOffer = FindRpgUpgradeOfferCandidate(_latestRpgPostRunUpgradeOfferSurface, member.MemberId);
                UpdateRpgMemberOfferMomentumState(memberState, member, runResultSnapshot, state, previousOffer, runIdentity);
            }

            member.RecentRunCount = memberState.RecentRunCount;
            member.ConsecutiveClearCount = memberState.ConsecutiveClearCount;
            member.RecentLowValueOfferCount = memberState.RecentLowValueOfferCount;
            member.RecentHighImpactOfferCount = memberState.RecentHighImpactOfferCount;
            member.CurrentMomentumTier = memberState.CurrentMomentumTier;
            member.ComebackReliefWindowCount = memberState.ComebackReliefWindowCount;
            member.MomentumDampeningWeight = memberState.MomentumDampeningWeight;
            member.MomentumIntensityBias = memberState.IntensityBiasWeight;
            member.ComebackReliefBias = memberState.ComebackReliefBiasWeight;
            member.MomentumMemberSummaryText = string.IsNullOrEmpty(memberState.MomentumMemberSummaryText) ? string.Empty : memberState.MomentumMemberSummaryText;
        }

        state.StreakSensitivePacingSummaryText = BuildRpgStreakSensitivePacingSummaryText(state);
        state.ClearStreakDampeningSummaryText = BuildRpgClearStreakDampeningSummaryText(state);
        state.ComebackReliefSummaryText = BuildRpgComebackReliefSummaryText(state);
        state.MomentumBalancingSummaryText = BuildRpgMomentumBalancingSummaryText(state);
        state.CurrentMomentumTierSummaryText = BuildRpgCurrentMomentumTierSummaryText(state);
        state.NextOfferIntensityHintText = BuildRpgNextOfferIntensityHintText(context, null);

        context.StreakSensitivePacingSummaryText = state.StreakSensitivePacingSummaryText;
        context.ClearStreakDampeningSummaryText = state.ClearStreakDampeningSummaryText;
        context.ComebackReliefSummaryText = state.ComebackReliefSummaryText;
        context.MomentumBalancingSummaryText = state.MomentumBalancingSummaryText;
        context.CurrentMomentumTierSummaryText = state.CurrentMomentumTierSummaryText;
        context.NextOfferIntensityHintText = state.NextOfferIntensityHintText;
        context.RecentRunCount = state.RecentRunCount;
        context.ConsecutiveClearCount = state.ConsecutiveClearCount;
        context.ConsecutiveDefeatCount = state.ConsecutiveDefeatCount;
        context.ConsecutiveRetreatCount = state.ConsecutiveRetreatCount;
        context.RecentLowValueOfferCount = state.RecentLowValueOfferCount;
        context.RecentHighImpactOfferCount = state.RecentHighImpactOfferCount;
        context.CurrentMomentumTier = state.CurrentMomentumTier;
        context.ComebackReliefWindowCount = state.ComebackReliefWindowCount;
        context.MomentumDampeningWeight = state.MomentumDampeningWeight;
    }

    private void UpdateRpgOfferMomentumResultStreak(PrototypeRpgOfferMomentumRuntimeState state, string resultStateKey)
    {
        state.RecentRunCount = Mathf.Clamp(state.RecentRunCount + 1, 0, 99);
        switch (string.IsNullOrEmpty(resultStateKey) ? string.Empty : resultStateKey)
        {
            case PrototypeBattleOutcomeKeys.RunClear:
                state.ConsecutiveClearCount = Mathf.Clamp(state.ConsecutiveClearCount + 1, 0, 9);
                state.ConsecutiveDefeatCount = 0;
                state.ConsecutiveRetreatCount = 0;
                break;
            case PrototypeBattleOutcomeKeys.RunDefeat:
                state.ConsecutiveDefeatCount = Mathf.Clamp(state.ConsecutiveDefeatCount + 1, 0, 9);
                state.ConsecutiveClearCount = 0;
                state.ConsecutiveRetreatCount = 0;
                break;
            case PrototypeBattleOutcomeKeys.RunRetreat:
                state.ConsecutiveRetreatCount = Mathf.Clamp(state.ConsecutiveRetreatCount + 1, 0, 9);
                state.ConsecutiveClearCount = 0;
                state.ConsecutiveDefeatCount = 0;
                break;
            default:
                state.ConsecutiveClearCount = 0;
                state.ConsecutiveDefeatCount = 0;
                state.ConsecutiveRetreatCount = 0;
                break;
        }
    }

    private void UpdateRpgOfferMomentumPressure(PrototypeRpgOfferMomentumRuntimeState state, PrototypeRpgPostRunUpgradeOfferSurface previousSurface)
    {
        int highImpactCount = CountRpgHighImpactOffers(previousSurface);
        int lowValueCount = CountRpgLowValueOffers(previousSurface);
        if (highImpactCount > 0)
        {
            state.RecentHighImpactOfferCount = Mathf.Clamp(state.RecentHighImpactOfferCount + 1, 0, 4);
        }
        else
        {
            state.RecentHighImpactOfferCount = Mathf.Clamp(state.RecentHighImpactOfferCount - 1, 0, 4);
        }

        if (lowValueCount > 0)
        {
            state.RecentLowValueOfferCount = Mathf.Clamp(state.RecentLowValueOfferCount + 1, 0, 4);
        }
        else
        {
            state.RecentLowValueOfferCount = Mathf.Clamp(state.RecentLowValueOfferCount - 1, 0, 4);
        }
    }

    private void UpdateRpgMemberOfferMomentumState(
        PrototypeRpgMemberOfferMomentumBiasState memberState,
        PrototypeRpgGrowthChoiceMemberContext member,
        PrototypeRpgRunResultSnapshot runResultSnapshot,
        PrototypeRpgOfferMomentumRuntimeState partyState,
        PrototypeRpgUpgradeOfferCandidate previousOffer,
        string runIdentity)
    {
        string resultStateKey = runResultSnapshot != null && !string.IsNullOrEmpty(runResultSnapshot.ResultStateKey)
            ? runResultSnapshot.ResultStateKey
            : string.Empty;
        memberState.RecentRunCount = Mathf.Clamp(memberState.RecentRunCount + 1, 0, 99);
        switch (resultStateKey)
        {
            case PrototypeBattleOutcomeKeys.RunClear:
                memberState.ConsecutiveClearCount = Mathf.Clamp(memberState.ConsecutiveClearCount + 1, 0, 9);
                memberState.ConsecutiveDefeatCount = 0;
                memberState.ConsecutiveRetreatCount = 0;
                break;
            case PrototypeBattleOutcomeKeys.RunDefeat:
                memberState.ConsecutiveDefeatCount = Mathf.Clamp(memberState.ConsecutiveDefeatCount + 1, 0, 9);
                memberState.ConsecutiveClearCount = 0;
                memberState.ConsecutiveRetreatCount = 0;
                break;
            case PrototypeBattleOutcomeKeys.RunRetreat:
                memberState.ConsecutiveRetreatCount = Mathf.Clamp(memberState.ConsecutiveRetreatCount + 1, 0, 9);
                memberState.ConsecutiveClearCount = 0;
                memberState.ConsecutiveDefeatCount = 0;
                break;
            default:
                memberState.ConsecutiveClearCount = 0;
                memberState.ConsecutiveDefeatCount = 0;
                memberState.ConsecutiveRetreatCount = 0;
                break;
        }

        if (IsRpgHighImpactOfferCandidate(previousOffer))
        {
            memberState.RecentHighImpactOfferCount = Mathf.Clamp(memberState.RecentHighImpactOfferCount + 1, 0, 4);
        }
        else
        {
            memberState.RecentHighImpactOfferCount = Mathf.Clamp(memberState.RecentHighImpactOfferCount - 1, 0, 4);
        }

        if (IsRpgLowValueOfferCandidate(previousOffer))
        {
            memberState.RecentLowValueOfferCount = Mathf.Clamp(memberState.RecentLowValueOfferCount + 1, 0, 4);
        }
        else
        {
            memberState.RecentLowValueOfferCount = Mathf.Clamp(memberState.RecentLowValueOfferCount - 1, 0, 4);
        }

        if (member != null && member.KnockedOut)
        {
            memberState.ComebackReliefWindowCount = Mathf.Clamp(memberState.ComebackReliefWindowCount + 2, 0, 4);
        }
        else if (resultStateKey == PrototypeBattleOutcomeKeys.RunDefeat)
        {
            memberState.ComebackReliefWindowCount = Mathf.Clamp(memberState.ComebackReliefWindowCount + 2, 0, 4);
        }
        else if (resultStateKey == PrototypeBattleOutcomeKeys.RunRetreat)
        {
            memberState.ComebackReliefWindowCount = Mathf.Clamp(memberState.ComebackReliefWindowCount + 1, 0, 3);
        }
        else if (resultStateKey == PrototypeBattleOutcomeKeys.RunClear)
        {
            memberState.ComebackReliefWindowCount = Mathf.Clamp(memberState.ComebackReliefWindowCount - 1, 0, 4);
        }

        memberState.CurrentMomentumTier = partyState != null ? partyState.CurrentMomentumTier : 0;
        memberState.MomentumDampeningWeight = Mathf.Clamp((partyState != null ? partyState.MomentumDampeningWeight : 0) - (memberState.ComebackReliefWindowCount > 0 ? 2 : 0), 0, 12);
        memberState.IntensityBiasWeight = Mathf.Clamp(memberState.CurrentMomentumTier * 3 - memberState.ComebackReliefWindowCount * 2 - memberState.RecentLowValueOfferCount, -6, 12);
        memberState.ComebackReliefBiasWeight = Mathf.Clamp(memberState.ComebackReliefWindowCount * 3 + ((member != null && member.KnockedOut) ? 2 : 0), 0, 12);
        memberState.LastMomentumReasonKey = ResolveRpgMemberMomentumReasonKey(memberState, resultStateKey, member != null && member.KnockedOut);
        memberState.MomentumMemberSummaryText = BuildRpgMemberMomentumSummaryText(memberState);
        memberState.LastResolvedRunIdentity = string.IsNullOrEmpty(runIdentity) ? string.Empty : runIdentity;
    }

    private int ResolveRpgOfferMomentumTier(PrototypeRpgOfferMomentumRuntimeState state)
    {
        if (state == null)
        {
            return 0;
        }

        int tier = 0;
        if (state.ConsecutiveClearCount >= 3 || state.RecentHighImpactOfferCount >= 3)
        {
            tier = 3;
        }
        else if (state.ConsecutiveClearCount >= 2 || state.RecentHighImpactOfferCount >= 2)
        {
            tier = 2;
        }
        else if (state.ConsecutiveClearCount >= 1)
        {
            tier = 1;
        }

        if (state.ConsecutiveDefeatCount >= 2 || state.ConsecutiveRetreatCount >= 2)
        {
            tier = Mathf.Clamp(tier - 1, 0, 3);
        }

        return tier;
    }

    private int ResolveRpgComebackReliefWindowCount(PrototypeRpgOfferMomentumRuntimeState state)
    {
        if (state == null)
        {
            return 0;
        }

        int relief = state.ComebackReliefWindowCount;
        relief = Mathf.Max(relief, state.ConsecutiveDefeatCount >= 2 ? 3 : 0);
        relief = Mathf.Max(relief, state.ConsecutiveDefeatCount == 1 ? 2 : 0);
        relief = Mathf.Max(relief, state.ConsecutiveRetreatCount >= 2 ? 2 : 0);
        relief = Mathf.Max(relief, state.ConsecutiveRetreatCount == 1 ? 1 : 0);
        if (state.ConsecutiveClearCount > 0 && relief > 0)
        {
            relief = Mathf.Max(0, relief - 1);
        }

        return Mathf.Clamp(relief, 0, 4);
    }

    private int ResolveRpgMomentumDampeningWeight(PrototypeRpgOfferMomentumRuntimeState state)
    {
        if (state == null)
        {
            return 0;
        }

        int weight = state.ConsecutiveClearCount * 2 + state.RecentHighImpactOfferCount * 2;
        weight -= state.RecentLowValueOfferCount;
        weight -= state.ComebackReliefWindowCount * 2;
        return Mathf.Clamp(weight, 0, 12);
    }

    private string ResolveRpgMomentumReasonKey(PrototypeRpgOfferMomentumRuntimeState state)
    {
        if (state == null)
        {
            return string.Empty;
        }

        if (state.ComebackReliefWindowCount >= 2)
        {
            return "comeback_relief";
        }

        if (state.ConsecutiveClearCount >= 3 || state.MomentumDampeningWeight >= 6)
        {
            return "clear_streak_dampening";
        }

        if (state.RecentLowValueOfferCount >= 2)
        {
            return "relief_after_low_value_pressure";
        }

        if (state.ConsecutiveRetreatCount > 0)
        {
            return "retreat_bridge";
        }

        return "steady_tempo";
    }

    private string ResolveRpgMemberMomentumReasonKey(PrototypeRpgMemberOfferMomentumBiasState memberState, string resultStateKey, bool knockedOut)
    {
        if (memberState == null)
        {
            return string.Empty;
        }

        if (knockedOut)
        {
            return "knockout_relief";
        }

        if (resultStateKey == PrototypeBattleOutcomeKeys.RunDefeat)
        {
            return "defeat_relief";
        }

        if (resultStateKey == PrototypeBattleOutcomeKeys.RunRetreat)
        {
            return "retreat_bridge";
        }

        if (memberState.ConsecutiveClearCount >= 2 && memberState.RecentHighImpactOfferCount >= 2)
        {
            return "member_clear_dampening";
        }

        return "member_steady";
    }

    private string BuildRpgStreakSensitivePacingSummaryText(PrototypeRpgOfferMomentumRuntimeState state)
    {
        if (state == null)
        {
            return "Session pacing: none.";
        }

        return "Session pacing: clears x" + state.ConsecutiveClearCount + ", defeats x" + state.ConsecutiveDefeatCount + ", retreats x" + state.ConsecutiveRetreatCount + ", recent high-impact x" + state.RecentHighImpactOfferCount + ", recent low-value x" + state.RecentLowValueOfferCount + ".";
    }

    private string BuildRpgClearStreakDampeningSummaryText(PrototypeRpgOfferMomentumRuntimeState state)
    {
        if (state == null || (state.ConsecutiveClearCount < 2 && state.MomentumDampeningWeight <= 0))
        {
            return "Clear streak dampening: not needed.";
        }

        return "Clear streak dampening: repeated clears are trimming rare-slot pressure so the next offer wave stays readable.";
    }

    private string BuildRpgComebackReliefSummaryText(PrototypeRpgOfferMomentumRuntimeState state)
    {
        if (state == null || state.ComebackReliefWindowCount <= 0)
        {
            return "Comeback relief: closed.";
        }

        return "Comeback relief: setback pressure keeps a stabilizer or bridge offer visible for the next run.";
    }

    private string BuildRpgMomentumBalancingSummaryText(PrototypeRpgOfferMomentumRuntimeState state)
    {
        if (state == null)
        {
            return "Momentum balancing: none.";
        }

        if (state.ComebackReliefWindowCount > 0 && state.MomentumDampeningWeight > 0)
        {
            return "Momentum balancing: cap rare pressure while preserving one comeback bridge.";
        }

        if (state.ComebackReliefWindowCount > 0)
        {
            return "Momentum balancing: lean softer until the party stabilizes.";
        }

        if (state.MomentumDampeningWeight > 0)
        {
            return "Momentum balancing: keep intensity positive, but bounded.";
        }

        return "Momentum balancing: steady mid-pressure pacing.";
    }

    private string BuildRpgCurrentMomentumTierSummaryText(PrototypeRpgOfferMomentumRuntimeState state)
    {
        if (state == null)
        {
            return "Momentum tier: none.";
        }

        if (state.ComebackReliefWindowCount > 0)
        {
            return "Momentum tier: relief-biased after recent setbacks.";
        }

        switch (state.CurrentMomentumTier)
        {
            case 3:
                return "Momentum tier: capped high-pressure sequencing.";
            case 2:
                return "Momentum tier: hot, but controlled.";
            case 1:
                return "Momentum tier: warm follow-up pressure.";
            default:
                return "Momentum tier: steady baseline.";
        }
    }

    private string BuildRpgNextOfferIntensityHintText(PrototypeRpgGrowthChoiceContext context, PrototypeRpgUpgradeOfferCandidate[] offers)
    {
        int rareCount = 0;
        int reliefCount = 0;
        for (int i = 0; i < (offers != null ? offers.Length : 0); i++)
        {
            PrototypeRpgUpgradeOfferCandidate offer = offers[i];
            if (IsRpgHighImpactOfferCandidate(offer))
            {
                rareCount += 1;
            }

            if (offer != null && offer.IsComebackReliefOffer)
            {
                reliefCount += 1;
            }
        }

        if (reliefCount > 0 && rareCount > 0)
        {
            return "Next intensity: mixed. Keep one comeback bridge alive while letting one sharper branch through.";
        }

        if (reliefCount > 0 || (context != null && context.ComebackReliefWindowCount > 0))
        {
            return "Next intensity: relief-biased. Keep at least one stabilizer visible.";
        }

        if (context != null && context.CurrentMomentumTier >= 2 && rareCount > 0)
        {
            return "Next intensity: high-impact pressure is still live, but capped.";
        }

        if (rareCount > 0)
        {
            return "Next intensity: one sharper branch can still break through.";
        }

        return context != null && context.RecentLowValueOfferCount >= 2
            ? "Next intensity: allow a slightly stronger sidegrade back in after a low-value stretch."
            : "Next intensity: steady sidegrades until momentum climbs again.";
    }

    private string BuildRpgMemberMomentumSummaryText(PrototypeRpgMemberOfferMomentumBiasState memberState)
    {
        if (memberState == null)
        {
            return string.Empty;
        }

        if (memberState.ComebackReliefWindowCount > 0)
        {
            return memberState.DisplayName + ": comeback window x" + memberState.ComebackReliefWindowCount + " keeps a softer branch available.";
        }

        if (memberState.CurrentMomentumTier >= 2)
        {
            return memberState.DisplayName + ": clear-streak heat is trimming one layer of high-impact pressure.";
        }

        if (memberState.RecentLowValueOfferCount >= 2)
        {
            return memberState.DisplayName + ": recent low-value pacing allows a slightly stronger follow-up.";
        }

        return memberState.DisplayName + ": steady offer tempo.";
    }

    private bool IsRpgHighImpactOfferCandidate(PrototypeRpgUpgradeOfferCandidate offer)
    {
        return offer != null && !offer.IsFallback && !offer.IsNoOp &&
               (offer.IsRareSlotOffer ||
                offer.PriorityBucketKey == "rare_slot" ||
                offer.PriorityBucketKey == "archetype_commitment_slot" ||
                offer.UsedTierEscalation ||
                offer.UsedUnlockExpansion ||
                offer.UnlockPriority >= 4);
    }

    private bool IsRpgLowValueOfferCandidate(PrototypeRpgUpgradeOfferCandidate offer)
    {
        return offer != null && (offer.IsFallback || offer.IsNoOp || offer.UsedPoolExhaustionFallback || offer.PriorityBucketKey == "common_slot");
    }

    private bool IsRpgComebackReliefOfferCandidate(PrototypeRpgUpgradeOfferCandidate offer)
    {
        return offer != null &&
               (offer.IsRecoveryEscapeOffer ||
                offer.SupportsRecoveryWindow ||
                offer.SupportsRescueOverride ||
                offer.ImprovesMissingCoverage ||
                offer.ReducesRoleOverlap ||
                offer.MaintainsHealthyFlex ||
                offer.SupportsCrossMemberSynergy ||
                offer.LaneKey == "support" ||
                offer.LaneKey == "recovery" ||
                offer.LaneKey == "defense" ||
                offer.AdjacentLaneKey == "recovery");
    }

    private int CountRpgHighImpactOffers(PrototypeRpgPostRunUpgradeOfferSurface surface)
    {
        PrototypeRpgUpgradeOfferCandidate[] offers = surface != null ? (surface.Offers ?? Array.Empty<PrototypeRpgUpgradeOfferCandidate>()) : Array.Empty<PrototypeRpgUpgradeOfferCandidate>();
        int count = 0;
        for (int i = 0; i < offers.Length; i++)
        {
            if (IsRpgHighImpactOfferCandidate(offers[i]))
            {
                count += 1;
            }
        }

        return count;
    }

    private int CountRpgLowValueOffers(PrototypeRpgPostRunUpgradeOfferSurface surface)
    {
        PrototypeRpgUpgradeOfferCandidate[] offers = surface != null ? (surface.Offers ?? Array.Empty<PrototypeRpgUpgradeOfferCandidate>()) : Array.Empty<PrototypeRpgUpgradeOfferCandidate>();
        int count = 0;
        for (int i = 0; i < offers.Length; i++)
        {
            if (IsRpgLowValueOfferCandidate(offers[i]))
            {
                count += 1;
            }
        }

        return count;
    }

    private int ScoreOfferAgainstSessionMomentum(PrototypeRpgGrowthChoiceContext context, PrototypeRpgUpgradeOfferCandidate candidate, PrototypeRpgGrowthChoiceMemberContext member)
    {
        if (context == null || candidate == null || member == null)
        {
            return 0;
        }

        bool highImpactOffer = IsRpgHighImpactOfferCandidate(candidate);
        bool reliefOffer = IsRpgComebackReliefOfferCandidate(candidate);
        int dampeningScore = 0;
        int reliefScore = 0;
        int maxReliefWindow = Mathf.Max(context.ComebackReliefWindowCount, member.ComebackReliefWindowCount);
        if (highImpactOffer && context.CurrentMomentumTier >= 2)
        {
            dampeningScore -= 2 + context.MomentumDampeningWeight / 2 + member.MomentumDampeningWeight / 3;
            if (candidate.ImprovesMissingCoverage || candidate.SupportsRescueOverride || candidate.MaintainsHealthyFlex)
            {
                dampeningScore += 3;
            }
            if (context.RecentLowValueOfferCount >= 2)
            {
                dampeningScore += 2;
            }
            if (context.ConsecutiveClearCount >= 3 && candidate.PriorityBucketKey == "rare_slot")
            {
                dampeningScore -= 3;
            }
        }

        if (maxReliefWindow > 0)
        {
            if (reliefOffer)
            {
                reliefScore += 4 + maxReliefWindow * 2;
                if (member.KnockedOut)
                {
                    reliefScore += 2;
                }
                if (context.ResultStateKey == PrototypeBattleOutcomeKeys.RunDefeat)
                {
                    reliefScore += 2;
                }
                if (context.ResultStateKey == PrototypeBattleOutcomeKeys.RunRetreat)
                {
                    reliefScore += 1;
                }
            }
            else if (highImpactOffer && !candidate.ImprovesMissingCoverage && !candidate.SupportsRecoveryWindow && !candidate.SupportsRescueOverride)
            {
                reliefScore -= 2 + maxReliefWindow;
            }
        }

        if (context.RecentLowValueOfferCount >= 2 && highImpactOffer && !candidate.IsContradictoryToLane)
        {
            reliefScore += 3;
        }

        candidate.MomentumDampeningScore = dampeningScore;
        candidate.ComebackReliefScore = reliefScore;
        candidate.IsMomentumDampened = dampeningScore < 0;
        candidate.IsComebackReliefOffer = reliefOffer && reliefScore > 0;
        candidate.MomentumReasonText = BuildRpgMomentumReasonText(context, candidate, member, dampeningScore, reliefScore);
        candidate.IntensityHintText = BuildRpgCandidateIntensityHintText(context, candidate, member, dampeningScore, reliefScore);
        return dampeningScore + reliefScore;
    }

    private string BuildRpgMomentumReasonText(PrototypeRpgGrowthChoiceContext context, PrototypeRpgUpgradeOfferCandidate candidate, PrototypeRpgGrowthChoiceMemberContext member, int dampeningScore, int reliefScore)
    {
        if (candidate == null)
        {
            return string.Empty;
        }

        if (candidate.IsComebackReliefOffer && reliefScore > 0)
        {
            return "Comeback relief: keep a softer bridge visible so the next run can repair tempo cleanly.";
        }

        if (candidate.IsMomentumDampened && dampeningScore < 0)
        {
            return "Momentum cap: repeated clears trimmed one layer of rare-slot pressure.";
        }

        if (context != null && context.RecentLowValueOfferCount >= 2 && IsRpgHighImpactOfferCandidate(candidate))
        {
            return "Momentum release: recent low-value pacing allows one sharper branch back in.";
        }

        return string.Empty;
    }

    private string BuildRpgCandidateIntensityHintText(PrototypeRpgGrowthChoiceContext context, PrototypeRpgUpgradeOfferCandidate candidate, PrototypeRpgGrowthChoiceMemberContext member, int dampeningScore, int reliefScore)
    {
        if (candidate == null)
        {
            return string.Empty;
        }

        if (candidate.IsComebackReliefOffer && reliefScore > 0)
        {
            return "Intensity hint: relief-biased slot.";
        }

        if (candidate.IsMomentumDampened)
        {
            return "Intensity hint: capped high-impact pressure.";
        }

        if (IsRpgHighImpactOfferCandidate(candidate))
        {
            return "Intensity hint: one sharper branch remains live.";
        }

        return string.Empty;
    }

    private void ApplyRpgOfferMomentumSurface(PrototypeRpgGrowthChoiceContext context, PrototypeRpgPostRunUpgradeOfferSurface surface)
    {
        if (surface == null)
        {
            return;
        }

        surface.StreakSensitivePacingSummaryText = string.IsNullOrEmpty(context != null ? context.StreakSensitivePacingSummaryText : string.Empty) ? CurrentStreakSensitivePacingSummaryText : context.StreakSensitivePacingSummaryText;
        surface.ClearStreakDampeningSummaryText = string.IsNullOrEmpty(context != null ? context.ClearStreakDampeningSummaryText : string.Empty) ? CurrentClearStreakDampeningSummaryText : context.ClearStreakDampeningSummaryText;
        surface.ComebackReliefSummaryText = string.IsNullOrEmpty(context != null ? context.ComebackReliefSummaryText : string.Empty) ? CurrentComebackReliefSummaryText : context.ComebackReliefSummaryText;
        surface.MomentumBalancingSummaryText = string.IsNullOrEmpty(context != null ? context.MomentumBalancingSummaryText : string.Empty) ? CurrentMomentumBalancingSummaryText : context.MomentumBalancingSummaryText;
        surface.CurrentMomentumTierSummaryText = string.IsNullOrEmpty(context != null ? context.CurrentMomentumTierSummaryText : string.Empty) ? CurrentMomentumTierSummaryText : context.CurrentMomentumTierSummaryText;
        surface.NextOfferIntensityHintText = BuildRpgNextOfferIntensityHintText(context, surface.Offers);

        PrototypeRpgOfferMomentumRuntimeState state = _sessionRpgOfferMomentumRuntimeState ?? new PrototypeRpgOfferMomentumRuntimeState();
        state.StreakSensitivePacingSummaryText = surface.StreakSensitivePacingSummaryText;
        state.ClearStreakDampeningSummaryText = surface.ClearStreakDampeningSummaryText;
        state.ComebackReliefSummaryText = surface.ComebackReliefSummaryText;
        state.MomentumBalancingSummaryText = surface.MomentumBalancingSummaryText;
        state.CurrentMomentumTierSummaryText = surface.CurrentMomentumTierSummaryText;
        state.NextOfferIntensityHintText = surface.NextOfferIntensityHintText;
        _sessionRpgOfferMomentumRuntimeState = state;
    }
}
