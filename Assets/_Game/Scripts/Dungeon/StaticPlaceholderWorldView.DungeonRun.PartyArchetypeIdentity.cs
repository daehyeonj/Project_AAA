using System;
using System.Collections.Generic;

public sealed partial class StaticPlaceholderWorldView
{
    private PrototypeRpgPartyArchetypeRuntimeState _sessionRpgPartyArchetypeRuntimeState = new PrototypeRpgPartyArchetypeRuntimeState();

    public string CurrentPartyArchetypeSummaryText => string.IsNullOrEmpty(_sessionRpgPartyArchetypeRuntimeState.PartyArchetypeSummaryText) ? "None" : _sessionRpgPartyArchetypeRuntimeState.PartyArchetypeSummaryText;
    public string CurrentFormationCoherenceSummaryText => string.IsNullOrEmpty(_sessionRpgPartyArchetypeRuntimeState.FormationCoherenceSummaryText) ? "None" : _sessionRpgPartyArchetypeRuntimeState.FormationCoherenceSummaryText;
    public string CurrentArchetypeCommitmentStrengthSummaryText => string.IsNullOrEmpty(_sessionRpgPartyArchetypeRuntimeState.CommitmentStrengthSummaryText) ? "None" : _sessionRpgPartyArchetypeRuntimeState.CommitmentStrengthSummaryText;
    public string CurrentArchetypeFlexRescueSummaryText => string.IsNullOrEmpty(_sessionRpgPartyArchetypeRuntimeState.FlexRescueSummaryText) ? "None" : _sessionRpgPartyArchetypeRuntimeState.FlexRescueSummaryText;
    public string CurrentWeightedPrioritySummaryText => string.IsNullOrEmpty(_sessionRpgPartyArchetypeRuntimeState.WeightedPrioritySummaryText) ? "None" : _sessionRpgPartyArchetypeRuntimeState.WeightedPrioritySummaryText;
    public string CurrentRareSlotSummaryText => string.IsNullOrEmpty(_sessionRpgPartyArchetypeRuntimeState.RareSlotSummaryText) ? "None" : _sessionRpgPartyArchetypeRuntimeState.RareSlotSummaryText;
    public string CurrentHighImpactSequencingSummaryText => string.IsNullOrEmpty(_sessionRpgPartyArchetypeRuntimeState.HighImpactSequencingSummaryText) ? "None" : _sessionRpgPartyArchetypeRuntimeState.HighImpactSequencingSummaryText;
    public string CurrentRecoverySafeguardSummaryText => string.IsNullOrEmpty(_sessionRpgPartyArchetypeRuntimeState.RecoverySafeguardSummaryText) ? "None" : _sessionRpgPartyArchetypeRuntimeState.RecoverySafeguardSummaryText;
    public PrototypeRpgPartyArchetypeRuntimeState LatestRpgPartyArchetypeRuntimeState => CopyRpgPartyArchetypeRuntimeState(_sessionRpgPartyArchetypeRuntimeState);

    private void ResetRpgPartyArchetypeRuntimeState()
    {
        _sessionRpgPartyArchetypeRuntimeState = new PrototypeRpgPartyArchetypeRuntimeState();
    }

    private PrototypeRpgPartyArchetypeRuntimeState CopyRpgPartyArchetypeRuntimeState(PrototypeRpgPartyArchetypeRuntimeState source)
    {
        PrototypeRpgPartyArchetypeRuntimeState copy = new PrototypeRpgPartyArchetypeRuntimeState();
        if (source == null)
        {
            return copy;
        }

        copy.SessionKey = CopyText(source.SessionKey);
        copy.PartyId = CopyText(source.PartyId);
        copy.DisplayName = CopyText(source.DisplayName);
        copy.LastResolvedRunIdentity = CopyText(source.LastResolvedRunIdentity);
        copy.PrimaryArchetypeKey = CopyText(source.PrimaryArchetypeKey);
        copy.SecondaryArchetypeKey = CopyText(source.SecondaryArchetypeKey);
        copy.FormationCoherenceScore = ClampNonNegative(source.FormationCoherenceScore);
        copy.CommitmentStrength = ClampNonNegative(source.CommitmentStrength);
        copy.BurstScore = ClampNonNegative(source.BurstScore);
        copy.SustainScore = ClampNonNegative(source.SustainScore);
        copy.SupportScore = ClampNonNegative(source.SupportScore);
        copy.AoeScore = ClampNonNegative(source.AoeScore);
        copy.PrecisionScore = ClampNonNegative(source.PrecisionScore);
        copy.RecoveryScore = ClampNonNegative(source.RecoveryScore);
        copy.FlexScore = ClampNonNegative(source.FlexScore);
        copy.PartyArchetypeSummaryText = CopyText(source.PartyArchetypeSummaryText);
        copy.FormationCoherenceSummaryText = CopyText(source.FormationCoherenceSummaryText);
        copy.CommitmentStrengthSummaryText = CopyText(source.CommitmentStrengthSummaryText);
        copy.FlexRescueSummaryText = CopyText(source.FlexRescueSummaryText);
        copy.TopArchetypeHintKey = CopyText(source.TopArchetypeHintKey);
        copy.TopFlexHintKey = CopyText(source.TopFlexHintKey);
        copy.LastPriorityRunIdentity = CopyText(source.LastPriorityRunIdentity);
        copy.LastTopPriorityBucketKey = CopyText(source.LastTopPriorityBucketKey);
        copy.LastTopPriorityOfferTypeKey = CopyText(source.LastTopPriorityOfferTypeKey);
        copy.LastRareSlotOfferTypeKey = CopyText(source.LastRareSlotOfferTypeKey);
        copy.WeightedPrioritySummaryText = CopyText(source.WeightedPrioritySummaryText);
        copy.RareSlotSummaryText = CopyText(source.RareSlotSummaryText);
        copy.HighImpactSequencingSummaryText = CopyText(source.HighImpactSequencingSummaryText);
        copy.RecoverySafeguardSummaryText = CopyText(source.RecoverySafeguardSummaryText);
        copy.Members = CopyRpgPartyArchetypeMemberRuntimeStates(source.Members);
        return copy;
    }

    private PrototypeRpgPartyArchetypeMemberRuntimeState[] CopyRpgPartyArchetypeMemberRuntimeStates(PrototypeRpgPartyArchetypeMemberRuntimeState[] source)
    {
        return CopyArray(source, CopyRpgPartyArchetypeMemberRuntimeState);
    }

    private PrototypeRpgPartyArchetypeMemberRuntimeState CopyRpgPartyArchetypeMemberRuntimeState(PrototypeRpgPartyArchetypeMemberRuntimeState source)
    {
        PrototypeRpgPartyArchetypeMemberRuntimeState copy = new PrototypeRpgPartyArchetypeMemberRuntimeState();
        if (source == null)
        {
            return copy;
        }

        copy.MemberId = CopyText(source.MemberId);
        copy.DisplayName = CopyText(source.DisplayName);
        copy.RoleTag = CopyText(source.RoleTag);
        copy.RoleLabel = CopyText(source.RoleLabel);
        copy.CurrentLaneKey = CopyText(source.CurrentLaneKey);
        copy.SecondaryLaneKey = CopyText(source.SecondaryLaneKey);
        copy.PrimaryArchetypeKey = CopyText(source.PrimaryArchetypeKey);
        copy.SecondaryArchetypeKey = CopyText(source.SecondaryArchetypeKey);
        copy.CoverageTags = CopyRpgStringArray(source.CoverageTags);
        copy.SynergyTags = CopyRpgStringArray(source.SynergyTags);
        copy.ArchetypeContributionTags = CopyRpgStringArray(source.ArchetypeContributionTags);
        copy.CommitmentWeight = ClampNonNegative(source.CommitmentWeight);
        copy.FlexibilityWeight = ClampNonNegative(source.FlexibilityWeight);
        copy.RecoveryWindowOpen = source.RecoveryWindowOpen;
        copy.MemberArchetypeSummaryText = CopyText(source.MemberArchetypeSummaryText);
        copy.LastResolvedRunIdentity = CopyText(source.LastResolvedRunIdentity);
        return copy;
    }

    private PrototypeRpgPartyArchetypeRuntimeState GetOrCreateRpgPartyArchetypeRuntimeState(PrototypeRpgPartyDefinition partyDefinition)
    {
        if (partyDefinition == null)
        {
            return _sessionRpgPartyArchetypeRuntimeState ?? new PrototypeRpgPartyArchetypeRuntimeState();
        }

        bool rebuild = _sessionRpgPartyArchetypeRuntimeState == null ||
                       _sessionRpgPartyArchetypeRuntimeState.Members == null ||
                       _sessionRpgPartyArchetypeRuntimeState.PartyId != partyDefinition.PartyId;
        if (rebuild)
        {
            _sessionRpgPartyArchetypeRuntimeState = new PrototypeRpgPartyArchetypeRuntimeState();
            _sessionRpgPartyArchetypeRuntimeState.SessionKey = string.IsNullOrEmpty(partyDefinition.PartyId) ? string.Empty : partyDefinition.PartyId;
            _sessionRpgPartyArchetypeRuntimeState.PartyId = string.IsNullOrEmpty(partyDefinition.PartyId) ? string.Empty : partyDefinition.PartyId;
            _sessionRpgPartyArchetypeRuntimeState.DisplayName = string.IsNullOrEmpty(partyDefinition.DisplayName) ? _sessionRpgPartyArchetypeRuntimeState.PartyId : partyDefinition.DisplayName;
            _sessionRpgPartyArchetypeRuntimeState.Members = BuildDefaultRpgPartyArchetypeMemberStates(partyDefinition);
        }
        else
        {
            EnsureRpgPartyArchetypeMemberStates(partyDefinition, _sessionRpgPartyArchetypeRuntimeState);
        }

        return _sessionRpgPartyArchetypeRuntimeState;
    }

    private PrototypeRpgPartyArchetypeMemberRuntimeState[] BuildDefaultRpgPartyArchetypeMemberStates(PrototypeRpgPartyDefinition partyDefinition)
    {
        PrototypeRpgPartyMemberDefinition[] definitions = partyDefinition != null ? (partyDefinition.Members ?? Array.Empty<PrototypeRpgPartyMemberDefinition>()) : Array.Empty<PrototypeRpgPartyMemberDefinition>();
        if (definitions.Length <= 0)
        {
            return Array.Empty<PrototypeRpgPartyArchetypeMemberRuntimeState>();
        }

        PrototypeRpgPartyArchetypeMemberRuntimeState[] members = new PrototypeRpgPartyArchetypeMemberRuntimeState[definitions.Length];
        for (int i = 0; i < definitions.Length; i++)
        {
            PrototypeRpgPartyMemberDefinition definition = definitions[i];
            PrototypeRpgPartyArchetypeMemberRuntimeState state = new PrototypeRpgPartyArchetypeMemberRuntimeState();
            state.MemberId = string.IsNullOrEmpty(definition != null ? definition.MemberId : string.Empty) ? string.Empty : definition.MemberId;
            state.DisplayName = string.IsNullOrEmpty(definition != null ? definition.DisplayName : string.Empty) ? "Adventurer" : definition.DisplayName;
            state.RoleTag = string.IsNullOrEmpty(definition != null ? definition.RoleTag : string.Empty) ? string.Empty : definition.RoleTag;
            state.RoleLabel = string.IsNullOrEmpty(definition != null ? definition.RoleLabel : string.Empty) ? string.Empty : definition.RoleLabel;
            members[i] = state;
        }

        return members;
    }

    private void EnsureRpgPartyArchetypeMemberStates(PrototypeRpgPartyDefinition partyDefinition, PrototypeRpgPartyArchetypeRuntimeState state)
    {
        if (partyDefinition == null || state == null)
        {
            return;
        }

        PrototypeRpgPartyMemberDefinition[] definitions = partyDefinition.Members ?? Array.Empty<PrototypeRpgPartyMemberDefinition>();
        PrototypeRpgPartyArchetypeMemberRuntimeState[] current = state.Members ?? Array.Empty<PrototypeRpgPartyArchetypeMemberRuntimeState>();
        if (definitions.Length == current.Length)
        {
            return;
        }

        PrototypeRpgPartyArchetypeMemberRuntimeState[] rebuilt = new PrototypeRpgPartyArchetypeMemberRuntimeState[definitions.Length];
        for (int i = 0; i < definitions.Length; i++)
        {
            PrototypeRpgPartyMemberDefinition definition = definitions[i];
            PrototypeRpgPartyArchetypeMemberRuntimeState existing = GetRpgPartyArchetypeMemberRuntimeState(state, definition != null ? definition.MemberId : string.Empty);
            if (existing == null)
            {
                existing = new PrototypeRpgPartyArchetypeMemberRuntimeState();
                existing.MemberId = string.IsNullOrEmpty(definition != null ? definition.MemberId : string.Empty) ? string.Empty : definition.MemberId;
                existing.DisplayName = string.IsNullOrEmpty(definition != null ? definition.DisplayName : string.Empty) ? "Adventurer" : definition.DisplayName;
                existing.RoleTag = string.IsNullOrEmpty(definition != null ? definition.RoleTag : string.Empty) ? string.Empty : definition.RoleTag;
                existing.RoleLabel = string.IsNullOrEmpty(definition != null ? definition.RoleLabel : string.Empty) ? string.Empty : definition.RoleLabel;
            }

            rebuilt[i] = existing;
        }

        state.Members = rebuilt;
    }

    private PrototypeRpgPartyArchetypeMemberRuntimeState GetRpgPartyArchetypeMemberRuntimeState(PrototypeRpgPartyArchetypeRuntimeState state, string memberId)
    {
        if (state == null || state.Members == null || string.IsNullOrEmpty(memberId))
        {
            return null;
        }

        for (int i = 0; i < state.Members.Length; i++)
        {
            PrototypeRpgPartyArchetypeMemberRuntimeState memberState = state.Members[i];
            if (memberState != null && memberState.MemberId == memberId)
            {
                return memberState;
            }
        }

        return null;
    }

    private void EvaluateRpgPartyArchetypeState(PrototypeRpgGrowthChoiceContext context, PrototypeRpgRunResultSnapshot runResultSnapshot, PrototypeRpgPartyDefinition partyDefinition)
    {
        if (context == null || partyDefinition == null)
        {
            return;
        }

        PrototypeRpgPartyArchetypeRuntimeState partyState = GetOrCreateRpgPartyArchetypeRuntimeState(partyDefinition);
        PrototypeRpgGrowthChoiceMemberContext[] members = context.Members ?? Array.Empty<PrototypeRpgGrowthChoiceMemberContext>();
        PrototypeRpgPartyArchetypeMemberRuntimeState[] resolvedStates = new PrototypeRpgPartyArchetypeMemberRuntimeState[members.Length];
        int burst = 0;
        int sustain = 0;
        int support = 0;
        int aoe = 0;
        int precision = 0;
        int recovery = 0;
        int flex = 0;
        for (int i = 0; i < members.Length; i++)
        {
            PrototypeRpgGrowthChoiceMemberContext member = members[i] ?? new PrototypeRpgGrowthChoiceMemberContext();
            PrototypeRpgPartyArchetypeMemberRuntimeState state = GetRpgPartyArchetypeMemberRuntimeState(partyState, member.MemberId) ?? new PrototypeRpgPartyArchetypeMemberRuntimeState();
            PrototypeRpgMemberBuildLaneRuntimeState laneState = GetRpgMemberBuildLaneRuntimeState(_sessionRpgPartyBuildLaneRuntimeState, member.MemberId);
            PrototypeRpgMemberCoverageRuntimeState coverageState = GetRpgMemberCoverageRuntimeState(_sessionRpgPartyCoverageRuntimeState, member.MemberId);
            PrototypeRpgMemberLaneRecoveryRuntimeState recoveryState = GetRpgMemberLaneRecoveryRuntimeState(_sessionRpgPartyLaneRecoveryRuntimeState, member.MemberId);
            string primaryLaneKey = string.IsNullOrEmpty(member.ResolvedPrimaryLaneKey) ? PrototypeRpgGrowthChoiceRuleCatalog.ResolveRoleCoreLaneKey(member.RoleTag) : member.ResolvedPrimaryLaneKey;
            string secondaryLaneKey = string.IsNullOrEmpty(member.ResolvedSecondaryLaneKey) ? PrototypeRpgGrowthChoiceRuleCatalog.ResolveRoleAdjacentLaneKey(member.RoleTag) : member.ResolvedSecondaryLaneKey;
            string[] contributionTags = ResolveRpgMemberArchetypeContributionTags(member, coverageState, recoveryState);
            string memberPrimaryArchetypeKey = ResolveRpgMemberPrimaryArchetypeKey(member, primaryLaneKey, secondaryLaneKey, recoveryState, context);
            string memberSecondaryArchetypeKey = ResolveRpgMemberSecondaryArchetypeKey(member, primaryLaneKey, secondaryLaneKey, memberPrimaryArchetypeKey, context);
            int commitmentWeight = ResolveRpgMemberArchetypeCommitmentWeight(member, laneState, recoveryState);
            int flexibilityWeight = ResolveRpgMemberArchetypeFlexWeight(member, coverageState, recoveryState);

            ApplyRpgPartyArchetypeScore(memberPrimaryArchetypeKey, commitmentWeight, ref burst, ref sustain, ref support, ref aoe, ref precision, ref recovery, ref flex);
            ApplyRpgPartyArchetypeScore(memberSecondaryArchetypeKey, Math.Max(1, flexibilityWeight / 2), ref burst, ref sustain, ref support, ref aoe, ref precision, ref recovery, ref flex);

            state.MemberId = string.IsNullOrEmpty(member.MemberId) ? string.Empty : member.MemberId;
            state.DisplayName = string.IsNullOrEmpty(member.DisplayName) ? "Adventurer" : member.DisplayName;
            state.RoleTag = string.IsNullOrEmpty(member.RoleTag) ? string.Empty : member.RoleTag;
            state.RoleLabel = string.IsNullOrEmpty(member.RoleLabel) ? string.Empty : member.RoleLabel;
            state.CurrentLaneKey = string.IsNullOrEmpty(primaryLaneKey) ? string.Empty : primaryLaneKey;
            state.SecondaryLaneKey = string.IsNullOrEmpty(secondaryLaneKey) ? string.Empty : secondaryLaneKey;
            state.PrimaryArchetypeKey = string.IsNullOrEmpty(memberPrimaryArchetypeKey) ? string.Empty : memberPrimaryArchetypeKey;
            state.SecondaryArchetypeKey = string.IsNullOrEmpty(memberSecondaryArchetypeKey) ? string.Empty : memberSecondaryArchetypeKey;
            state.CoverageTags = coverageState != null ? CopyRpgCoverageTagArray(coverageState.CurrentCoverageTags) : CopyRpgCoverageTagArray(member.CurrentCoverageTags);
            state.SynergyTags = coverageState != null ? CopyRpgCoverageTagArray(coverageState.SynergyTags) : CopyRpgCoverageTagArray(member.SynergyTags);
            state.ArchetypeContributionTags = CopyRpgStringArray(contributionTags);
            state.CommitmentWeight = Math.Max(0, commitmentWeight);
            state.FlexibilityWeight = Math.Max(0, flexibilityWeight);
            state.RecoveryWindowOpen = recoveryState != null && recoveryState.SoftRespecWindowOpen;
            state.MemberArchetypeSummaryText = BuildRpgMemberArchetypeSummaryText(member, state);
            state.LastResolvedRunIdentity = string.IsNullOrEmpty(context.RunIdentity) ? string.Empty : context.RunIdentity;
            resolvedStates[i] = state;

            member.ArchetypeContributionTags = CopyRpgStringArray(contributionTags);
            member.ArchetypeCommitmentWeight = state.CommitmentWeight;
            member.ArchetypeFlexWeight = state.FlexibilityWeight;
            member.MemberArchetypeSummaryText = state.MemberArchetypeSummaryText;
        }

        string primaryArchetypeKey = ResolveRpgDominantPartyArchetypeKey(burst, sustain, support, aoe, precision, recovery, flex);
        string secondaryArchetypeKey = ResolveRpgSecondaryPartyArchetypeKey(primaryArchetypeKey, burst, sustain, support, aoe, precision, recovery, flex);
        int formationCoherenceScore = ResolveRpgFormationCoherenceScore(resolvedStates, context);
        int commitmentStrength = ResolveRpgArchetypeCommitmentStrength(resolvedStates);
        string topFlexHintKey = ResolveRpgTopFlexHintKey(primaryArchetypeKey, formationCoherenceScore, context);

        partyState.Members = resolvedStates;
        partyState.LastResolvedRunIdentity = string.IsNullOrEmpty(context.RunIdentity) ? string.Empty : context.RunIdentity;
        partyState.PrimaryArchetypeKey = string.IsNullOrEmpty(primaryArchetypeKey) ? string.Empty : primaryArchetypeKey;
        partyState.SecondaryArchetypeKey = string.IsNullOrEmpty(secondaryArchetypeKey) ? string.Empty : secondaryArchetypeKey;
        partyState.FormationCoherenceScore = Math.Max(0, formationCoherenceScore);
        partyState.CommitmentStrength = Math.Max(0, commitmentStrength);
        partyState.BurstScore = Math.Max(0, burst);
        partyState.SustainScore = Math.Max(0, sustain);
        partyState.SupportScore = Math.Max(0, support);
        partyState.AoeScore = Math.Max(0, aoe);
        partyState.PrecisionScore = Math.Max(0, precision);
        partyState.RecoveryScore = Math.Max(0, recovery);
        partyState.FlexScore = Math.Max(0, flex);
        partyState.TopArchetypeHintKey = string.IsNullOrEmpty(primaryArchetypeKey) ? string.Empty : primaryArchetypeKey;
        partyState.TopFlexHintKey = string.IsNullOrEmpty(topFlexHintKey) ? string.Empty : topFlexHintKey;
        partyState.PartyArchetypeSummaryText = BuildRpgPartyArchetypeSummaryText(partyState);
        partyState.FormationCoherenceSummaryText = BuildRpgFormationCoherenceSummaryText(partyState, context);
        partyState.CommitmentStrengthSummaryText = BuildRpgArchetypeCommitmentStrengthSummaryText(partyState);
        partyState.FlexRescueSummaryText = BuildRpgArchetypeFlexRescueSummaryText(partyState, context);

        context.PartyArchetypeSummaryText = partyState.PartyArchetypeSummaryText;
        context.FormationCoherenceSummaryText = partyState.FormationCoherenceSummaryText;
        context.CommitmentStrengthSummaryText = partyState.CommitmentStrengthSummaryText;
        context.FlexRescueSummaryText = partyState.FlexRescueSummaryText;
        context.PrimaryArchetypeKey = partyState.PrimaryArchetypeKey;
        context.SecondaryArchetypeKey = partyState.SecondaryArchetypeKey;
        context.FormationCoherenceScore = partyState.FormationCoherenceScore;
        context.CommitmentStrength = partyState.CommitmentStrength;
        context.TopArchetypeHintKey = partyState.TopArchetypeHintKey;
        context.TopFlexHintKey = partyState.TopFlexHintKey;
        context.MemberArchetypeStates = CopyRpgPartyArchetypeMemberRuntimeStates(resolvedStates);
        context.OfferCoherenceSummaryText = MergeRpgSummaryText(context.OfferCoherenceSummaryText, partyState.FormationCoherenceSummaryText);
        context.OfferCoherenceSummaryText = MergeRpgSummaryText(context.OfferCoherenceSummaryText, partyState.CommitmentStrengthSummaryText);
    }

    private string[] ResolveRpgMemberArchetypeContributionTags(PrototypeRpgGrowthChoiceMemberContext member, PrototypeRpgMemberCoverageRuntimeState coverageState, PrototypeRpgMemberLaneRecoveryRuntimeState recoveryState)
    {
        List<string> tags = new List<string>();
        if (member == null)
        {
            return Array.Empty<string>();
        }

        if (member.KillCount > 0 || member.DamageDealt >= 18) { tags.Add("finisher"); }
        if (member.DamageDealt >= Math.Max(10, member.DamageTaken)) { tags.Add("burst_pressure"); }
        if (member.HealingDone >= Math.Max(6, member.DamageDealt / 2) && member.HealingDone > 0) { tags.Add("support_sustain"); }
        if (member.ActionCount >= 3 && member.DamageTaken <= member.DamageDealt) { tags.Add("tempo_driver"); }
        if (member.ContributionShapeKey == "anchor" || member.DamageTaken >= Math.Max(8, member.DamageDealt)) { tags.Add("front_anchor"); }
        if (ArrayContainsRpgValue(coverageState != null ? coverageState.SynergyTags : member.SynergyTags, "support_arcane_core")) { tags.Add("arcane_link"); }
        if (ArrayContainsRpgValue(coverageState != null ? coverageState.SynergyTags : member.SynergyTags, "frontline_precision_core")) { tags.Add("execution_link"); }
        if (recoveryState != null && recoveryState.SoftRespecWindowOpen) { tags.Add("recovery_window"); }
        return tags.Count <= 0 ? Array.Empty<string>() : tags.ToArray();
    }

    private string ResolveRpgMemberPrimaryArchetypeKey(PrototypeRpgGrowthChoiceMemberContext member, string primaryLaneKey, string secondaryLaneKey, PrototypeRpgMemberLaneRecoveryRuntimeState recoveryState, PrototypeRpgGrowthChoiceContext context)
    {
        if (recoveryState != null && recoveryState.SoftRespecWindowOpen && (member == null || member.KnockedOut || (context != null && (context.ResultStateKey == PrototypeBattleOutcomeKeys.RunDefeat || context.ResultStateKey == PrototypeBattleOutcomeKeys.RunRetreat))))
        {
            return "rescue_override";
        }

        if (primaryLaneKey == "support" && member != null && member.HealingDone > 0)
        {
            return "support_anchor";
        }

        if (primaryLaneKey == "arcane")
        {
            return member != null && member.ActionCount >= 3 ? "arcane_sweep" : "hybrid_stabilize";
        }

        if (primaryLaneKey == "precision")
        {
            return member != null && (member.KillCount > 0 || member.DamageDealt >= 16) ? "precision_finisher" : "balanced_formation";
        }

        if (primaryLaneKey == "frontline")
        {
            return member != null && member.DamageDealt >= member.DamageTaken ? "burst_vanguard" : "sustain_anchor";
        }

        if (primaryLaneKey == "recovery" || secondaryLaneKey == "recovery")
        {
            return "sustain_anchor";
        }

        return "balanced_formation";
    }

    private string ResolveRpgMemberSecondaryArchetypeKey(PrototypeRpgGrowthChoiceMemberContext member, string primaryLaneKey, string secondaryLaneKey, string primaryArchetypeKey, PrototypeRpgGrowthChoiceContext context)
    {
        if (primaryArchetypeKey == "rescue_override")
        {
            return "hybrid_stabilize";
        }

        if (secondaryLaneKey == "support" || (member != null && member.HealingDone > 0))
        {
            return "support_anchor";
        }

        if (secondaryLaneKey == "recovery" || (context != null && context.ResultStateKey == PrototypeBattleOutcomeKeys.RunRetreat))
        {
            return "hybrid_stabilize";
        }

        if (secondaryLaneKey == "arcane")
        {
            return "arcane_sweep";
        }

        if (secondaryLaneKey == "precision")
        {
            return "precision_finisher";
        }

        return "balanced_formation";
    }

    private int ResolveRpgMemberArchetypeCommitmentWeight(PrototypeRpgGrowthChoiceMemberContext member, PrototypeRpgMemberBuildLaneRuntimeState laneState, PrototypeRpgMemberLaneRecoveryRuntimeState recoveryState)
    {
        int depth = laneState != null ? laneState.CommitmentDepth : (member != null ? member.CommitmentDepth : 0);
        int weight = Math.Max(1, depth + 1);
        if (member != null && member.KillCount > 0) { weight += 1; }
        if (recoveryState != null && recoveryState.SoftRespecWindowOpen) { weight = Math.Max(1, weight - 1); }
        return weight;
    }

    private int ResolveRpgMemberArchetypeFlexWeight(PrototypeRpgGrowthChoiceMemberContext member, PrototypeRpgMemberCoverageRuntimeState coverageState, PrototypeRpgMemberLaneRecoveryRuntimeState recoveryState)
    {
        int weight = 1;
        if (coverageState != null)
        {
            weight += Math.Max(0, coverageState.MissingCoverageBias / 4);
            weight += Math.Max(0, coverageState.SynergyBias / 6);
        }
        if (recoveryState != null && recoveryState.SoftRespecWindowOpen)
        {
            weight += 3;
        }
        if (member != null && member.HealingDone > 0)
        {
            weight += 1;
        }
        return Math.Max(1, weight);
    }

    private void ApplyRpgPartyArchetypeScore(string archetypeKey, int weight, ref int burst, ref int sustain, ref int support, ref int aoe, ref int precision, ref int recovery, ref int flex)
    {
        switch (string.IsNullOrEmpty(archetypeKey) ? string.Empty : archetypeKey)
        {
            case "burst_vanguard": burst += weight; break;
            case "sustain_anchor": sustain += weight; break;
            case "support_anchor": support += weight; break;
            case "arcane_sweep": aoe += weight; break;
            case "precision_finisher": precision += weight; break;
            case "rescue_override": recovery += weight; break;
            case "hybrid_stabilize": flex += weight + 1; break;
            default: flex += weight; break;
        }
    }

    private string ResolveRpgDominantPartyArchetypeKey(int burst, int sustain, int support, int aoe, int precision, int recovery, int flex)
    {
        string bestKey = "balanced_formation";
        int bestScore = flex;
        TrySelectRpgArchetypeKey("burst_vanguard", burst, ref bestKey, ref bestScore);
        TrySelectRpgArchetypeKey("sustain_anchor", sustain, ref bestKey, ref bestScore);
        TrySelectRpgArchetypeKey("support_anchor", support, ref bestKey, ref bestScore);
        TrySelectRpgArchetypeKey("arcane_sweep", aoe, ref bestKey, ref bestScore);
        TrySelectRpgArchetypeKey("precision_finisher", precision, ref bestKey, ref bestScore);
        TrySelectRpgArchetypeKey("rescue_override", recovery, ref bestKey, ref bestScore);
        return bestKey;
    }

    private string ResolveRpgSecondaryPartyArchetypeKey(string primaryKey, int burst, int sustain, int support, int aoe, int precision, int recovery, int flex)
    {
        string bestKey = "balanced_formation";
        int bestScore = int.MinValue;
        TrySelectRpgSecondaryArchetypeKey(primaryKey, "burst_vanguard", burst, ref bestKey, ref bestScore);
        TrySelectRpgSecondaryArchetypeKey(primaryKey, "sustain_anchor", sustain, ref bestKey, ref bestScore);
        TrySelectRpgSecondaryArchetypeKey(primaryKey, "support_anchor", support, ref bestKey, ref bestScore);
        TrySelectRpgSecondaryArchetypeKey(primaryKey, "arcane_sweep", aoe, ref bestKey, ref bestScore);
        TrySelectRpgSecondaryArchetypeKey(primaryKey, "precision_finisher", precision, ref bestKey, ref bestScore);
        TrySelectRpgSecondaryArchetypeKey(primaryKey, "rescue_override", recovery, ref bestKey, ref bestScore);
        TrySelectRpgSecondaryArchetypeKey(primaryKey, "hybrid_stabilize", flex, ref bestKey, ref bestScore);
        return bestKey;
    }

    private void TrySelectRpgArchetypeKey(string candidateKey, int score, ref string bestKey, ref int bestScore)
    {
        if (score > bestScore)
        {
            bestKey = candidateKey;
            bestScore = score;
        }
    }

    private void TrySelectRpgSecondaryArchetypeKey(string primaryKey, string candidateKey, int score, ref string bestKey, ref int bestScore)
    {
        if (candidateKey == primaryKey)
        {
            return;
        }
        if (score > bestScore)
        {
            bestKey = candidateKey;
            bestScore = score;
        }
    }

    private int ResolveRpgFormationCoherenceScore(PrototypeRpgPartyArchetypeMemberRuntimeState[] members, PrototypeRpgGrowthChoiceContext context)
    {
        if (members == null || members.Length <= 0)
        {
            return 0;
        }

        int score = 40;
        bool hasFrontline = false;
        bool hasSupport = false;
        bool hasPrecision = false;
        bool hasArcane = false;
        int rescueOpenCount = 0;
        for (int i = 0; i < members.Length; i++)
        {
            PrototypeRpgPartyArchetypeMemberRuntimeState member = members[i];
            if (member == null)
            {
                continue;
            }

            hasFrontline |= member.CurrentLaneKey == "frontline";
            hasSupport |= member.CurrentLaneKey == "support";
            hasPrecision |= member.CurrentLaneKey == "precision";
            hasArcane |= member.CurrentLaneKey == "arcane";
            rescueOpenCount += member.RecoveryWindowOpen ? 1 : 0;
            score += Math.Min(6, member.CommitmentWeight);
            score += Math.Min(4, member.FlexibilityWeight);
        }

        if (hasFrontline) { score += 8; }
        if (hasSupport) { score += 8; }
        if (hasPrecision || hasArcane) { score += 6; }
        if (hasPrecision && hasArcane) { score += 4; }
        if (context != null && !string.IsNullOrEmpty(context.TopMissingCoverageKey)) { score -= 6; }
        if (context != null && !string.IsNullOrEmpty(context.TopOverlapKey)) { score -= 5; }
        if (rescueOpenCount > 0) { score -= rescueOpenCount * 4; }
        return Math.Max(0, score);
    }

    private int ResolveRpgArchetypeCommitmentStrength(PrototypeRpgPartyArchetypeMemberRuntimeState[] members)
    {
        if (members == null || members.Length <= 0)
        {
            return 0;
        }

        int total = 0;
        for (int i = 0; i < members.Length; i++)
        {
            PrototypeRpgPartyArchetypeMemberRuntimeState member = members[i];
            if (member != null)
            {
                total += member.CommitmentWeight;
            }
        }
        return Math.Max(0, total);
    }

    private string ResolveRpgTopFlexHintKey(string primaryArchetypeKey, int formationCoherenceScore, PrototypeRpgGrowthChoiceContext context)
    {
        if (context != null && (context.ResultStateKey == PrototypeBattleOutcomeKeys.RunDefeat || context.ResultStateKey == PrototypeBattleOutcomeKeys.RunRetreat))
        {
            return "rescue_window";
        }
        if (formationCoherenceScore < 60)
        {
            return "formation_patch";
        }
        if (primaryArchetypeKey == "burst_vanguard" || primaryArchetypeKey == "precision_finisher")
        {
            return "stability_sidegrade";
        }
        return "keep_flex";
    }

    private string BuildRpgMemberArchetypeSummaryText(PrototypeRpgGrowthChoiceMemberContext member, PrototypeRpgPartyArchetypeMemberRuntimeState state)
    {
        string name = string.IsNullOrEmpty(state != null ? state.DisplayName : string.Empty) ? "Adventurer" : state.DisplayName;
        string primary = ResolveRpgArchetypeLabel(state != null ? state.PrimaryArchetypeKey : string.Empty);
        string secondary = ResolveRpgArchetypeLabel(state != null ? state.SecondaryArchetypeKey : string.Empty);
        string laneLabel = PrototypeRpgGrowthChoiceRuleCatalog.ResolveLaneLabel(state != null ? state.CurrentLaneKey : string.Empty);
        return name + " archetype: " + primary + " primary, " + secondary + " flex through " + laneLabel + ".";
    }

    private string BuildRpgPartyArchetypeSummaryText(PrototypeRpgPartyArchetypeRuntimeState state)
    {
        if (state == null)
        {
            return "Archetype identity: none.";
        }

        return "Archetype identity: " + ResolveRpgArchetypeLabel(state.PrimaryArchetypeKey) + " core, " + ResolveRpgArchetypeLabel(state.SecondaryArchetypeKey) + " flex.";
    }

    private string BuildRpgFormationCoherenceSummaryText(PrototypeRpgPartyArchetypeRuntimeState state, PrototypeRpgGrowthChoiceContext context)
    {
        if (state == null)
        {
            return "Formation coherence: none.";
        }

        string summary = "Formation coherence: score " + state.FormationCoherenceScore + ", " + ResolveRpgArchetypeLabel(state.PrimaryArchetypeKey) + " shell with " + ResolveRpgArchetypeLabel(state.SecondaryArchetypeKey) + " support.";
        if (context != null && !string.IsNullOrEmpty(context.TopMissingCoverageKey))
        {
            summary += " Gap watch: " + ResolveRpgCoverageLabel(context.TopMissingCoverageKey) + ".";
        }
        else if (context != null && !string.IsNullOrEmpty(context.TopSynergyKey))
        {
            summary += " Pairing: " + ResolveRpgSynergyLabel(context.TopSynergyKey) + ".";
        }
        return summary;
    }

    private string BuildRpgArchetypeCommitmentStrengthSummaryText(PrototypeRpgPartyArchetypeRuntimeState state)
    {
        if (state == null)
        {
            return "Archetype commitment: none.";
        }

        return "Archetype commitment: " + state.CommitmentStrength + " total depth across the current shell.";
    }

    private string BuildRpgArchetypeFlexRescueSummaryText(PrototypeRpgPartyArchetypeRuntimeState state, PrototypeRpgGrowthChoiceContext context)
    {
        if (state == null)
        {
            return "Flex rescue: none.";
        }

        string summary = "Flex rescue: " + ResolveRpgFlexHintLabel(state.TopFlexHintKey) + ".";
        if (context != null && (context.ResultStateKey == PrototypeBattleOutcomeKeys.RunDefeat || context.ResultStateKey == PrototypeBattleOutcomeKeys.RunRetreat))
        {
            summary += " Recovery windows stay open for safer sidegrades.";
        }
        return summary;
    }

    private int ScoreOfferAgainstPartyArchetype(PrototypeRpgGrowthChoiceContext context, PrototypeRpgUpgradeOfferCandidate candidate, PrototypeRpgGrowthChoiceMemberContext member)
    {
        if (context == null || candidate == null || member == null)
        {
            return 0;
        }

        PrototypeRpgPartyArchetypeMemberRuntimeState memberState = GetRpgPartyArchetypeMemberRuntimeState(_sessionRpgPartyArchetypeRuntimeState, member.MemberId);
        PrototypeRpgPartyArchetypeRuntimeState partyState = _sessionRpgPartyArchetypeRuntimeState ?? new PrototypeRpgPartyArchetypeRuntimeState();
        int archetypeScore = 0;
        int formationScore = 0;
        candidate.SupportsArchetypeFollowup = false;
        candidate.MaintainsHealthyFlex = false;
        candidate.SupportsRescueOverride = false;
        candidate.ArchetypeReasonText = string.Empty;
        candidate.FormationReasonText = string.Empty;

        bool supportsPrimary = DoesRpgCandidateSupportArchetype(candidate, partyState.PrimaryArchetypeKey, memberState, member);
        bool supportsSecondary = DoesRpgCandidateSupportArchetype(candidate, partyState.SecondaryArchetypeKey, memberState, member);
        if (supportsPrimary)
        {
            archetypeScore += 8 + Math.Max(0, partyState.CommitmentStrength / 6);
            candidate.SupportsArchetypeFollowup = true;
            candidate.ArchetypeReasonText = "Archetype follow-up: supports the current " + ResolveRpgArchetypeLabel(partyState.PrimaryArchetypeKey) + " shell.";
        }
        else if (supportsSecondary)
        {
            archetypeScore += 4;
            candidate.SupportsArchetypeFollowup = true;
            candidate.ArchetypeReasonText = "Archetype follow-up: leans into the current " + ResolveRpgArchetypeLabel(partyState.SecondaryArchetypeKey) + " flex path.";
        }

        if (partyState.PrimaryArchetypeKey == "rescue_override" && (candidate.IsRecoveryEscapeOffer || candidate.SupportsRecoveryWindow || candidate.LaneKey == "recovery"))
        {
            archetypeScore += 10;
            candidate.SupportsRescueOverride = true;
            candidate.ArchetypeReasonText = MergeRpgSummaryText(candidate.ArchetypeReasonText, "Rescue override: this offer respects the current recovery pressure.");
        }

        if (partyState.FormationCoherenceScore < 60)
        {
            if (candidate.ImprovesMissingCoverage || candidate.ReducesRoleOverlap)
            {
                formationScore += 8;
                candidate.MaintainsHealthyFlex = true;
                candidate.FormationReasonText = BuildRpgFormationReasonText(context, candidate, partyState, true);
            }
            else if (candidate.SupportsCrossMemberSynergy || candidate.IsAdjacentLaneOffer)
            {
                formationScore += 4;
                candidate.MaintainsHealthyFlex = true;
                candidate.FormationReasonText = BuildRpgFormationReasonText(context, candidate, partyState, false);
            }
        }
        else
        {
            if (candidate.SupportsCrossMemberSynergy)
            {
                formationScore += 4;
                candidate.FormationReasonText = BuildRpgFormationReasonText(context, candidate, partyState, false);
            }
            else if (candidate.IsContradictoryToLane && !candidate.ImprovesMissingCoverage && !candidate.ReducesRoleOverlap)
            {
                formationScore -= 4;
                candidate.FormationReasonText = "Formation coherence: this move jars against the current shell more than it fixes it.";
            }
        }

        candidate.ArchetypeBiasScore = archetypeScore;
        candidate.FormationCoherenceBiasScore = formationScore;
        candidate.ReasonText = MergeRpgSummaryText(candidate.ReasonText, candidate.ArchetypeReasonText);
        candidate.ContinuitySummaryText = MergeRpgSummaryText(candidate.ContinuitySummaryText, candidate.FormationReasonText);
        return archetypeScore + formationScore;
    }

    private bool DoesRpgCandidateSupportArchetype(PrototypeRpgUpgradeOfferCandidate candidate, string archetypeKey, PrototypeRpgPartyArchetypeMemberRuntimeState memberState, PrototypeRpgGrowthChoiceMemberContext member)
    {
        if (candidate == null || string.IsNullOrEmpty(archetypeKey))
        {
            return false;
        }

        switch (archetypeKey)
        {
            case "rescue_override":
                return candidate.IsRecoveryEscapeOffer || candidate.SupportsRecoveryWindow || candidate.LaneKey == "recovery";
            case "support_anchor":
            case "sustain_anchor":
                return candidate.LaneKey == "support" || candidate.LaneKey == "recovery" || candidate.SupportsRecoveryWindow;
            case "arcane_sweep":
                return candidate.LaneKey == "arcane";
            case "precision_finisher":
                return candidate.LaneKey == "precision" || candidate.OfferTypeKey == "offense_path";
            case "burst_vanguard":
                return candidate.LaneKey == "frontline" || candidate.OfferTypeKey == "defense_path";
            case "hybrid_stabilize":
            case "balanced_formation":
                return candidate.ImprovesMissingCoverage || candidate.ReducesRoleOverlap || candidate.SupportsCrossMemberSynergy || candidate.IsAdjacentLaneOffer || (memberState != null && memberState.RecoveryWindowOpen && candidate.SupportsRecoveryWindow);
            default:
                return !string.IsNullOrEmpty(candidate.LaneKey) && candidate.LaneKey == (memberState != null ? memberState.CurrentLaneKey : (member != null ? member.ResolvedPrimaryLaneKey : string.Empty));
        }
    }

    private string BuildRpgFormationReasonText(PrototypeRpgGrowthChoiceContext context, PrototypeRpgUpgradeOfferCandidate candidate, PrototypeRpgPartyArchetypeRuntimeState partyState, bool repairGap)
    {
        if (repairGap)
        {
            if (candidate.ImprovesMissingCoverage && !string.IsNullOrEmpty(context != null ? context.TopMissingCoverageKey : string.Empty))
            {
                return "Formation repair: covers the current " + ResolveRpgCoverageLabel(context.TopMissingCoverageKey) + " gap while preserving archetype continuity.";
            }
            if (candidate.ReducesRoleOverlap && !string.IsNullOrEmpty(context != null ? context.TopOverlapKey : string.Empty))
            {
                return "Formation repair: relieves " + ResolveRpgCoverageLabel(context.TopOverlapKey) + " overlap without breaking the shell.";
            }
        }

        string primary = ResolveRpgArchetypeLabel(partyState != null ? partyState.PrimaryArchetypeKey : string.Empty);
        string secondary = ResolveRpgArchetypeLabel(partyState != null ? partyState.SecondaryArchetypeKey : string.Empty);
        return "Formation coherence: keeps " + primary + " primary and " + secondary + " flex within reach.";
    }

    private string ResolveRpgArchetypeLabel(string archetypeKey)
    {
        switch (string.IsNullOrEmpty(archetypeKey) ? string.Empty : archetypeKey)
        {
            case "burst_vanguard": return "Burst Vanguard";
            case "sustain_anchor": return "Sustain Anchor";
            case "support_anchor": return "Support Anchor";
            case "arcane_sweep": return "Arcane Sweep";
            case "precision_finisher": return "Precision Finisher";
            case "rescue_override": return "Rescue Override";
            case "hybrid_stabilize": return "Hybrid Stabilize";
            default: return "Balanced Formation";
        }
    }

    private string ResolveRpgFlexHintLabel(string hintKey)
    {
        switch (string.IsNullOrEmpty(hintKey) ? string.Empty : hintKey)
        {
            case "rescue_window": return "rescue windows stay open";
            case "formation_patch": return "patch the formation before deepening commitment";
            case "stability_sidegrade": return "keep one stabilizing sidegrade live";
            default: return "maintain a healthy flex slot";
        }
    }
}
