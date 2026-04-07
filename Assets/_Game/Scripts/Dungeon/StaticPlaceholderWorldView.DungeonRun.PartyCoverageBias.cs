using System;
using System.Collections.Generic;

public sealed partial class StaticPlaceholderWorldView
{
    private PrototypeRpgPartyCoverageRuntimeState _sessionRpgPartyCoverageRuntimeState = new PrototypeRpgPartyCoverageRuntimeState();

    public string CurrentPartyCoverageSummaryText => string.IsNullOrEmpty(_sessionRpgPartyCoverageRuntimeState.PartyCoverageSummaryText) ? "None" : _sessionRpgPartyCoverageRuntimeState.PartyCoverageSummaryText;
    public string CurrentRoleOverlapWarningSummaryText => string.IsNullOrEmpty(_sessionRpgPartyCoverageRuntimeState.RoleOverlapWarningSummaryText) ? "None" : _sessionRpgPartyCoverageRuntimeState.RoleOverlapWarningSummaryText;
    public string CurrentCrossMemberSynergySummaryText => string.IsNullOrEmpty(_sessionRpgPartyCoverageRuntimeState.CrossMemberSynergySummaryText) ? "None" : _sessionRpgPartyCoverageRuntimeState.CrossMemberSynergySummaryText;
    public PrototypeRpgPartyCoverageRuntimeState LatestRpgPartyCoverageRuntimeState => CopyRpgPartyCoverageRuntimeState(_sessionRpgPartyCoverageRuntimeState);

    private void ResetRpgPartyCoverageRuntimeState()
    {
        _sessionRpgPartyCoverageRuntimeState = new PrototypeRpgPartyCoverageRuntimeState();
    }

    private PrototypeRpgPartyCoverageRuntimeState CopyRpgPartyCoverageRuntimeState(PrototypeRpgPartyCoverageRuntimeState source)
    {
        PrototypeRpgPartyCoverageRuntimeState copy = new PrototypeRpgPartyCoverageRuntimeState();
        if (source == null)
        {
            return copy;
        }

        copy.SessionKey = CopyText(source.SessionKey);
        copy.PartyId = CopyText(source.PartyId);
        copy.DisplayName = CopyText(source.DisplayName);
        copy.LastResolvedRunIdentity = CopyText(source.LastResolvedRunIdentity);
        copy.PartyCoverageSummaryText = CopyText(source.PartyCoverageSummaryText);
        copy.RoleOverlapWarningSummaryText = CopyText(source.RoleOverlapWarningSummaryText);
        copy.CrossMemberSynergySummaryText = CopyText(source.CrossMemberSynergySummaryText);
        copy.OfferCoherenceSummaryText = CopyText(source.OfferCoherenceSummaryText);
        copy.TopMissingCoverageKey = CopyText(source.TopMissingCoverageKey);
        copy.TopOverlapKey = CopyText(source.TopOverlapKey);
        copy.TopSynergyKey = CopyText(source.TopSynergyKey);
        copy.Members = CopyRpgMemberCoverageRuntimeStates(source.Members);
        return copy;
    }

    private PrototypeRpgMemberCoverageRuntimeState CopyRpgMemberCoverageRuntimeState(PrototypeRpgMemberCoverageRuntimeState source)
    {
        PrototypeRpgMemberCoverageRuntimeState copy = new PrototypeRpgMemberCoverageRuntimeState();
        if (source == null)
        {
            return copy;
        }

        copy.MemberId = CopyText(source.MemberId);
        copy.DisplayName = CopyText(source.DisplayName);
        copy.RoleTag = CopyText(source.RoleTag);
        copy.ResolvedPrimaryLaneKey = CopyText(source.ResolvedPrimaryLaneKey);
        copy.ResolvedSecondaryLaneKey = CopyText(source.ResolvedSecondaryLaneKey);
        copy.ContributionShapeKey = CopyText(source.ContributionShapeKey);
        copy.CurrentCoverageTags = CopyRpgCoverageTagArray(source.CurrentCoverageTags);
        copy.OverlapTags = CopyRpgCoverageTagArray(source.OverlapTags);
        copy.SynergyTags = CopyRpgCoverageTagArray(source.SynergyTags);
        copy.MissingCoverageBias = ClampNonNegative(source.MissingCoverageBias);
        copy.OverlapPenalty = ClampNonNegative(source.OverlapPenalty);
        copy.SynergyBias = ClampNonNegative(source.SynergyBias);
        copy.CoverageSummaryText = CopyText(source.CoverageSummaryText);
        copy.LastResolvedRunIdentity = CopyText(source.LastResolvedRunIdentity);
        return copy;
    }

    private string[] CopyRpgCoverageTagArray(string[] values)
    {
        return CopyArray(values, CopyText);
    }

    private PrototypeRpgMemberCoverageRuntimeState[] CopyRpgMemberCoverageRuntimeStates(PrototypeRpgMemberCoverageRuntimeState[] source)
    {
        return CopyArray(source, CopyRpgMemberCoverageRuntimeState);
    }

    private PrototypeRpgPartyCoverageRuntimeState GetOrCreateRpgPartyCoverageRuntimeState(PrototypeRpgPartyDefinition partyDefinition)
    {
        if (partyDefinition == null)
        {
            return _sessionRpgPartyCoverageRuntimeState ?? new PrototypeRpgPartyCoverageRuntimeState();
        }

        bool rebuild = _sessionRpgPartyCoverageRuntimeState == null ||
                       _sessionRpgPartyCoverageRuntimeState.Members == null ||
                       _sessionRpgPartyCoverageRuntimeState.PartyId != partyDefinition.PartyId;
        if (rebuild)
        {
            _sessionRpgPartyCoverageRuntimeState = new PrototypeRpgPartyCoverageRuntimeState();
            _sessionRpgPartyCoverageRuntimeState.SessionKey = string.IsNullOrEmpty(partyDefinition.PartyId) ? string.Empty : partyDefinition.PartyId;
            _sessionRpgPartyCoverageRuntimeState.PartyId = string.IsNullOrEmpty(partyDefinition.PartyId) ? string.Empty : partyDefinition.PartyId;
            _sessionRpgPartyCoverageRuntimeState.DisplayName = string.IsNullOrEmpty(partyDefinition.DisplayName) ? _sessionRpgPartyCoverageRuntimeState.PartyId : partyDefinition.DisplayName;
            _sessionRpgPartyCoverageRuntimeState.Members = BuildDefaultRpgMemberCoverageRuntimeStates(partyDefinition);
        }
        else
        {
            EnsureRpgMemberCoverageRuntimeStates(partyDefinition, _sessionRpgPartyCoverageRuntimeState);
        }

        return _sessionRpgPartyCoverageRuntimeState;
    }

    private PrototypeRpgMemberCoverageRuntimeState[] BuildDefaultRpgMemberCoverageRuntimeStates(PrototypeRpgPartyDefinition partyDefinition)
    {
        PrototypeRpgPartyMemberDefinition[] definitions = partyDefinition != null ? (partyDefinition.Members ?? Array.Empty<PrototypeRpgPartyMemberDefinition>()) : Array.Empty<PrototypeRpgPartyMemberDefinition>();
        if (definitions.Length <= 0)
        {
            return Array.Empty<PrototypeRpgMemberCoverageRuntimeState>();
        }

        PrototypeRpgMemberCoverageRuntimeState[] members = new PrototypeRpgMemberCoverageRuntimeState[definitions.Length];
        for (int i = 0; i < definitions.Length; i++)
        {
            PrototypeRpgPartyMemberDefinition definition = definitions[i];
            PrototypeRpgMemberCoverageRuntimeState state = new PrototypeRpgMemberCoverageRuntimeState();
            state.MemberId = string.IsNullOrEmpty(definition != null ? definition.MemberId : string.Empty) ? string.Empty : definition.MemberId;
            state.DisplayName = string.IsNullOrEmpty(definition != null ? definition.DisplayName : string.Empty) ? "Adventurer" : definition.DisplayName;
            state.RoleTag = string.IsNullOrEmpty(definition != null ? definition.RoleTag : string.Empty) ? string.Empty : definition.RoleTag;
            members[i] = state;
        }

        return members;
    }

    private void EnsureRpgMemberCoverageRuntimeStates(PrototypeRpgPartyDefinition partyDefinition, PrototypeRpgPartyCoverageRuntimeState state)
    {
        if (partyDefinition == null || state == null)
        {
            return;
        }

        PrototypeRpgPartyMemberDefinition[] definitions = partyDefinition.Members ?? Array.Empty<PrototypeRpgPartyMemberDefinition>();
        PrototypeRpgMemberCoverageRuntimeState[] current = state.Members ?? Array.Empty<PrototypeRpgMemberCoverageRuntimeState>();
        if (definitions.Length == current.Length)
        {
            return;
        }

        PrototypeRpgMemberCoverageRuntimeState[] rebuilt = new PrototypeRpgMemberCoverageRuntimeState[definitions.Length];
        for (int i = 0; i < definitions.Length; i++)
        {
            PrototypeRpgPartyMemberDefinition definition = definitions[i];
            PrototypeRpgMemberCoverageRuntimeState existing = GetRpgMemberCoverageRuntimeState(state, definition != null ? definition.MemberId : string.Empty);
            if (existing == null)
            {
                existing = new PrototypeRpgMemberCoverageRuntimeState();
                existing.MemberId = string.IsNullOrEmpty(definition != null ? definition.MemberId : string.Empty) ? string.Empty : definition.MemberId;
                existing.DisplayName = string.IsNullOrEmpty(definition != null ? definition.DisplayName : string.Empty) ? "Adventurer" : definition.DisplayName;
                existing.RoleTag = string.IsNullOrEmpty(definition != null ? definition.RoleTag : string.Empty) ? string.Empty : definition.RoleTag;
            }

            rebuilt[i] = existing;
        }

        state.Members = rebuilt;
    }

    private PrototypeRpgMemberCoverageRuntimeState GetRpgMemberCoverageRuntimeState(PrototypeRpgPartyCoverageRuntimeState state, string memberId)
    {
        if (state == null || state.Members == null || string.IsNullOrEmpty(memberId))
        {
            return null;
        }

        for (int i = 0; i < state.Members.Length; i++)
        {
            PrototypeRpgMemberCoverageRuntimeState candidate = state.Members[i];
            if (candidate != null && candidate.MemberId == memberId)
            {
                return candidate;
            }
        }

        return null;
    }

    private void EvaluateRpgPartyCoverageState(PrototypeRpgGrowthChoiceContext context, PrototypeRpgRunResultSnapshot runResultSnapshot, PrototypeRpgPartyDefinition partyDefinition)
    {
        if (context == null || partyDefinition == null)
        {
            return;
        }

        PrototypeRpgPartyCoverageRuntimeState partyState = GetOrCreateRpgPartyCoverageRuntimeState(partyDefinition);
        PrototypeRpgGrowthChoiceMemberContext[] members = context.Members ?? Array.Empty<PrototypeRpgGrowthChoiceMemberContext>();
        Dictionary<string, int> laneCounts = BuildRpgCoverageLaneCountMap(members);
        string topMissingCoverageKey = ResolveRpgTopMissingCoverageKey(laneCounts, context, members);
        string topOverlapKey = ResolveRpgTopOverlapKey(laneCounts);
        string topSynergyKey = ResolveRpgTopSynergyKey(laneCounts, context, members);
        PrototypeRpgMemberCoverageRuntimeState[] resolvedStates = new PrototypeRpgMemberCoverageRuntimeState[members.Length];
        for (int i = 0; i < members.Length; i++)
        {
            PrototypeRpgGrowthChoiceMemberContext member = members[i] ?? new PrototypeRpgGrowthChoiceMemberContext();
            PrototypeRpgMemberCoverageRuntimeState existing = GetRpgMemberCoverageRuntimeState(partyState, member.MemberId) ?? new PrototypeRpgMemberCoverageRuntimeState();
            string primaryLaneKey = string.IsNullOrEmpty(member.ResolvedPrimaryLaneKey)
                ? PrototypeRpgGrowthChoiceRuleCatalog.ResolveRoleCoreLaneKey(member.RoleTag)
                : member.ResolvedPrimaryLaneKey;
            string secondaryLaneKey = string.IsNullOrEmpty(member.ResolvedSecondaryLaneKey)
                ? PrototypeRpgGrowthChoiceRuleCatalog.ResolveRoleAdjacentLaneKey(member.RoleTag)
                : member.ResolvedSecondaryLaneKey;
            string contributionShapeKey = ResolveRpgContributionShapeKey(member, context);
            string[] coverageTags = ResolveRpgCurrentCoverageTags(primaryLaneKey, secondaryLaneKey, contributionShapeKey);
            string[] overlapTags = ResolveRpgOverlapTags(primaryLaneKey, topOverlapKey, laneCounts);
            string[] synergyTags = ResolveRpgSynergyTags(primaryLaneKey, secondaryLaneKey, topSynergyKey);
            int missingCoverageBias = ResolveRpgMemberMissingCoverageBias(member, primaryLaneKey, secondaryLaneKey, topMissingCoverageKey, context);
            int overlapPenalty = ResolveRpgMemberOverlapPenalty(primaryLaneKey, topOverlapKey, laneCounts);
            int synergyBias = ResolveRpgMemberSynergyBias(primaryLaneKey, secondaryLaneKey, topSynergyKey);
            string coverageSummaryText = BuildRpgMemberCoverageSummaryText(member, primaryLaneKey, secondaryLaneKey, contributionShapeKey, missingCoverageBias, overlapPenalty, synergyBias, topMissingCoverageKey, topOverlapKey, topSynergyKey);

            existing.MemberId = string.IsNullOrEmpty(member.MemberId) ? string.Empty : member.MemberId;
            existing.DisplayName = string.IsNullOrEmpty(member.DisplayName) ? "Adventurer" : member.DisplayName;
            existing.RoleTag = string.IsNullOrEmpty(member.RoleTag) ? string.Empty : member.RoleTag;
            existing.ResolvedPrimaryLaneKey = primaryLaneKey;
            existing.ResolvedSecondaryLaneKey = secondaryLaneKey;
            existing.ContributionShapeKey = contributionShapeKey;
            existing.CurrentCoverageTags = coverageTags;
            existing.OverlapTags = overlapTags;
            existing.SynergyTags = synergyTags;
            existing.MissingCoverageBias = Math.Max(0, missingCoverageBias);
            existing.OverlapPenalty = Math.Max(0, overlapPenalty);
            existing.SynergyBias = Math.Max(0, synergyBias);
            existing.CoverageSummaryText = coverageSummaryText;
            existing.LastResolvedRunIdentity = string.IsNullOrEmpty(context.RunIdentity) ? string.Empty : context.RunIdentity;
            resolvedStates[i] = existing;

            member.ContributionShapeKey = contributionShapeKey;
            member.CurrentCoverageTags = CopyRpgCoverageTagArray(coverageTags);
            member.OverlapTags = CopyRpgCoverageTagArray(overlapTags);
            member.SynergyTags = CopyRpgCoverageTagArray(synergyTags);
            member.MissingCoverageBias = Math.Max(0, missingCoverageBias);
            member.OverlapPenalty = Math.Max(0, overlapPenalty);
            member.SynergyBias = Math.Max(0, synergyBias);
            member.CoverageSummaryText = coverageSummaryText;
        }

        partyState.Members = resolvedStates;
        partyState.LastResolvedRunIdentity = string.IsNullOrEmpty(context.RunIdentity) ? string.Empty : context.RunIdentity;
        partyState.TopMissingCoverageKey = string.IsNullOrEmpty(topMissingCoverageKey) ? string.Empty : topMissingCoverageKey;
        partyState.TopOverlapKey = string.IsNullOrEmpty(topOverlapKey) ? string.Empty : topOverlapKey;
        partyState.TopSynergyKey = string.IsNullOrEmpty(topSynergyKey) ? string.Empty : topSynergyKey;
        partyState.PartyCoverageSummaryText = BuildRpgPartyCoverageSummaryText(laneCounts, topMissingCoverageKey, context);
        partyState.RoleOverlapWarningSummaryText = BuildRpgRoleOverlapWarningSummaryText(laneCounts, topOverlapKey);
        partyState.CrossMemberSynergySummaryText = BuildRpgCrossMemberSynergySummaryText(topSynergyKey, context);
        partyState.OfferCoherenceSummaryText = BuildRpgPartyCoverageOfferCoherenceSummary(topMissingCoverageKey, topOverlapKey, topSynergyKey);

        context.PartyCoverageSummaryText = partyState.PartyCoverageSummaryText;
        context.RoleOverlapWarningSummaryText = partyState.RoleOverlapWarningSummaryText;
        context.CrossMemberSynergySummaryText = partyState.CrossMemberSynergySummaryText;
        context.TopMissingCoverageKey = partyState.TopMissingCoverageKey;
        context.TopOverlapKey = partyState.TopOverlapKey;
        context.TopSynergyKey = partyState.TopSynergyKey;
        context.MemberCoverageStates = CopyRpgMemberCoverageRuntimeStates(resolvedStates);
        context.OfferCoherenceSummaryText = MergeRpgSummaryText(context.OfferCoherenceSummaryText, partyState.OfferCoherenceSummaryText);
    }

    private Dictionary<string, int> BuildRpgCoverageLaneCountMap(PrototypeRpgGrowthChoiceMemberContext[] members)
    {
        Dictionary<string, int> counts = CreateRpgCoverageLaneMap();
        if (members == null || members.Length <= 0)
        {
            return counts;
        }

        for (int i = 0; i < members.Length; i++)
        {
            PrototypeRpgGrowthChoiceMemberContext member = members[i];
            if (member == null)
            {
                continue;
            }

            string laneKey = string.IsNullOrEmpty(member.ResolvedPrimaryLaneKey)
                ? PrototypeRpgGrowthChoiceRuleCatalog.ResolveRoleCoreLaneKey(member.RoleTag)
                : member.ResolvedPrimaryLaneKey;
            if (!string.IsNullOrEmpty(laneKey))
            {
                IncrementRpgCoverageLaneCount(counts, laneKey);
            }
        }

        return counts;
    }

    private Dictionary<string, int> CreateRpgCoverageLaneMap()
    {
        return new Dictionary<string, int>(StringComparer.Ordinal)
        {
            ["frontline"] = 0,
            ["precision"] = 0,
            ["arcane"] = 0,
            ["support"] = 0,
            ["recovery"] = 0,
        };
    }

    private void IncrementRpgCoverageLaneCount(Dictionary<string, int> counts, string laneKey)
    {
        if (counts == null || string.IsNullOrEmpty(laneKey))
        {
            return;
        }

        int current;
        counts.TryGetValue(laneKey, out current);
        counts[laneKey] = current + 1;
    }

    private int GetRpgCoverageLaneCount(Dictionary<string, int> counts, string laneKey)
    {
        if (counts == null || string.IsNullOrEmpty(laneKey))
        {
            return 0;
        }

        int count;
        return counts.TryGetValue(laneKey, out count) ? count : 0;
    }

    private bool HasRpgRecoveryPressure(PrototypeRpgGrowthChoiceContext context, PrototypeRpgGrowthChoiceMemberContext[] members)
    {
        if (context != null && (context.ResultStateKey == PrototypeBattleOutcomeKeys.RunDefeat || context.ResultStateKey == PrototypeBattleOutcomeKeys.RunRetreat))
        {
            return true;
        }

        if (members == null)
        {
            return false;
        }

        for (int i = 0; i < members.Length; i++)
        {
            PrototypeRpgGrowthChoiceMemberContext member = members[i];
            if (member != null && (member.KnockedOut || !member.Survived || member.DamageTaken > member.DamageDealt))
            {
                return true;
            }
        }

        return false;
    }
    private string ResolveRpgTopMissingCoverageKey(Dictionary<string, int> laneCounts, PrototypeRpgGrowthChoiceContext context, PrototypeRpgGrowthChoiceMemberContext[] members)
    {
        bool recoveryPressure = HasRpgRecoveryPressure(context, members);
        string[] laneOrder = new[] { "frontline", "support", "precision", "arcane", "recovery" };
        string bestLane = string.Empty;
        int bestScore = 5;
        for (int i = 0; i < laneOrder.Length; i++)
        {
            string laneKey = laneOrder[i];
            int count = GetRpgCoverageLaneCount(laneCounts, laneKey);
            int score = 0;
            switch (laneKey)
            {
                case "frontline":
                    score = count <= 0 ? 22 : 0;
                    if (recoveryPressure && count <= 0) { score += 4; }
                    break;
                case "support":
                    score = count <= 0 ? 20 : 0;
                    if (recoveryPressure && count <= 0) { score += 6; }
                    break;
                case "precision":
                    score = count <= 0 ? 12 : 0;
                    if (context != null && context.EliteDefeated && count <= 0) { score += 4; }
                    break;
                case "arcane":
                    score = count <= 0 ? 12 : 0;
                    if (context != null && context.RiskyRoute && count <= 0) { score += 4; }
                    break;
                case "recovery":
                    score = count <= 0 ? 4 : 0;
                    if (recoveryPressure) { score += count <= 0 ? 12 : 4; }
                    break;
            }

            if (score > bestScore)
            {
                bestScore = score;
                bestLane = laneKey;
            }
        }

        return bestLane;
    }

    private string ResolveRpgTopOverlapKey(Dictionary<string, int> laneCounts)
    {
        string[] laneOrder = new[] { "frontline", "precision", "arcane", "support", "recovery" };
        string bestLane = string.Empty;
        int bestScore = 0;
        for (int i = 0; i < laneOrder.Length; i++)
        {
            string laneKey = laneOrder[i];
            int count = GetRpgCoverageLaneCount(laneCounts, laneKey);
            if (count <= 1) { continue; }
            int score = (count - 1) * 12;
            if (score > bestScore)
            {
                bestScore = score;
                bestLane = laneKey;
            }
        }

        return bestLane;
    }

    private string ResolveRpgTopSynergyKey(Dictionary<string, int> laneCounts, PrototypeRpgGrowthChoiceContext context, PrototypeRpgGrowthChoiceMemberContext[] members)
    {
        bool recoveryPressure = HasRpgRecoveryPressure(context, members);
        int frontline = GetRpgCoverageLaneCount(laneCounts, "frontline");
        int precision = GetRpgCoverageLaneCount(laneCounts, "precision");
        int arcane = GetRpgCoverageLaneCount(laneCounts, "arcane");
        int support = GetRpgCoverageLaneCount(laneCounts, "support");
        if (recoveryPressure && frontline > 0 && support > 0) { return "frontline_support"; }
        if (frontline > 0 && precision > 0 && context != null && context.ResultStateKey == PrototypeBattleOutcomeKeys.RunClear) { return "frontline_precision"; }
        if (support > 0 && arcane > 0) { return "support_arcane"; }
        if (frontline > 0 && arcane > 0) { return "frontline_arcane"; }
        if (support > 0 && precision > 0) { return "support_precision"; }
        return string.Empty;
    }

    private string ResolveRpgContributionShapeKey(PrototypeRpgGrowthChoiceMemberContext member, PrototypeRpgGrowthChoiceContext context)
    {
        if (member == null) { return string.Empty; }
        if (member.HealingDone >= Math.Max(6, member.DamageDealt / 2) && member.HealingDone > 0) { return "healer"; }
        if (member.DamageTaken >= Math.Max(8, member.DamageDealt) && member.DamageTaken > 0) { return "anchor"; }
        if (member.KillCount > 0 || member.DamageDealt >= 18) { return "finisher"; }
        if (member.DamageDealt > 0) { return "pressure"; }
        if (context != null && (context.ResultStateKey == PrototypeBattleOutcomeKeys.RunDefeat || context.ResultStateKey == PrototypeBattleOutcomeKeys.RunRetreat)) { return "stabilizer"; }
        return "balanced";
    }

    private string[] ResolveRpgCurrentCoverageTags(string primaryLaneKey, string secondaryLaneKey, string contributionShapeKey)
    {
        List<string> tags = new List<string>();
        if (!string.IsNullOrEmpty(primaryLaneKey)) { tags.Add(primaryLaneKey + "_core"); }
        if (!string.IsNullOrEmpty(secondaryLaneKey) && secondaryLaneKey != primaryLaneKey) { tags.Add(secondaryLaneKey + "_flex"); }
        if (!string.IsNullOrEmpty(contributionShapeKey)) { tags.Add(contributionShapeKey); }
        return tags.Count <= 0 ? Array.Empty<string>() : tags.ToArray();
    }

    private string[] ResolveRpgOverlapTags(string primaryLaneKey, string topOverlapKey, Dictionary<string, int> laneCounts)
    {
        if (string.IsNullOrEmpty(primaryLaneKey) || string.IsNullOrEmpty(topOverlapKey) || primaryLaneKey != topOverlapKey) { return Array.Empty<string>(); }
        int count = GetRpgCoverageLaneCount(laneCounts, primaryLaneKey);
        return count <= 1 ? Array.Empty<string>() : new[] { primaryLaneKey + "_overlap", primaryLaneKey + "_stack_" + count };
    }

    private string[] ResolveRpgSynergyTags(string primaryLaneKey, string secondaryLaneKey, string topSynergyKey)
    {
        if (string.IsNullOrEmpty(topSynergyKey)) { return Array.Empty<string>(); }
        List<string> tags = new List<string>();
        if (DoesRpgSynergyPairContainLane(topSynergyKey, primaryLaneKey)) { tags.Add(topSynergyKey + "_core"); }
        if (!string.IsNullOrEmpty(secondaryLaneKey) && secondaryLaneKey != primaryLaneKey && DoesRpgSynergyPairContainLane(topSynergyKey, secondaryLaneKey)) { tags.Add(topSynergyKey + "_flex"); }
        return tags.Count <= 0 ? Array.Empty<string>() : tags.ToArray();
    }

    private int ResolveRpgMemberMissingCoverageBias(PrototypeRpgGrowthChoiceMemberContext member, string primaryLaneKey, string secondaryLaneKey, string topMissingCoverageKey, PrototypeRpgGrowthChoiceContext context)
    {
        if (member == null || string.IsNullOrEmpty(topMissingCoverageKey)) { return 0; }
        if (topMissingCoverageKey == primaryLaneKey) { return 4; }
        if (topMissingCoverageKey == secondaryLaneKey) { return 10; }
        if (topMissingCoverageKey == PrototypeRpgGrowthChoiceRuleCatalog.ResolveRoleAdjacentLaneKey(member.RoleTag)) { return 8; }
        if (topMissingCoverageKey == "recovery" && (member.KnockedOut || (context != null && (context.ResultStateKey == PrototypeBattleOutcomeKeys.RunDefeat || context.ResultStateKey == PrototypeBattleOutcomeKeys.RunRetreat)))) { return 6; }
        return 0;
    }

    private int ResolveRpgMemberOverlapPenalty(string primaryLaneKey, string topOverlapKey, Dictionary<string, int> laneCounts)
    {
        if (string.IsNullOrEmpty(primaryLaneKey) || string.IsNullOrEmpty(topOverlapKey) || primaryLaneKey != topOverlapKey) { return 0; }
        int count = GetRpgCoverageLaneCount(laneCounts, primaryLaneKey);
        return count <= 1 ? 0 : 8 + (count - 1) * 4;
    }

    private int ResolveRpgMemberSynergyBias(string primaryLaneKey, string secondaryLaneKey, string topSynergyKey)
    {
        if (string.IsNullOrEmpty(topSynergyKey)) { return 0; }
        if (DoesRpgSynergyPairContainLane(topSynergyKey, secondaryLaneKey) && secondaryLaneKey != primaryLaneKey) { return 8; }
        if (DoesRpgSynergyPairContainLane(topSynergyKey, primaryLaneKey)) { return 5; }
        return 0;
    }

    private string BuildRpgMemberCoverageSummaryText(PrototypeRpgGrowthChoiceMemberContext member, string primaryLaneKey, string secondaryLaneKey, string contributionShapeKey, int missingCoverageBias, int overlapPenalty, int synergyBias, string topMissingCoverageKey, string topOverlapKey, string topSynergyKey)
    {
        string name = string.IsNullOrEmpty(member != null ? member.DisplayName : string.Empty) ? "Adventurer" : member.DisplayName;
        string summary = name + " coverage: " + ResolveRpgCoverageLabel(primaryLaneKey) + " core, " + ResolveRpgCoverageLabel(secondaryLaneKey) + " flex, " + ResolveRpgContributionShapeLabel(contributionShapeKey) + " contribution.";
        if (!string.IsNullOrEmpty(topMissingCoverageKey) && missingCoverageBias > 0) { summary += " Can flex toward missing " + ResolveRpgCoverageLabel(topMissingCoverageKey) + "."; }
        if (!string.IsNullOrEmpty(topOverlapKey) && overlapPenalty > 0) { summary += " Currently contributes to " + ResolveRpgCoverageLabel(topOverlapKey) + " overlap."; }
        if (!string.IsNullOrEmpty(topSynergyKey) && synergyBias > 0) { summary += " Supports " + ResolveRpgSynergyLabel(topSynergyKey) + "."; }
        return summary;
    }

    private string BuildRpgPartyCoverageSummaryText(Dictionary<string, int> laneCounts, string topMissingCoverageKey, PrototypeRpgGrowthChoiceContext context)
    {
        string summary = "Coverage: " + ResolveRpgCoverageLabel("frontline") + " " + GetRpgCoverageLaneCount(laneCounts, "frontline") + ", " + ResolveRpgCoverageLabel("precision") + " " + GetRpgCoverageLaneCount(laneCounts, "precision") + ", " + ResolveRpgCoverageLabel("arcane") + " " + GetRpgCoverageLaneCount(laneCounts, "arcane") + ", " + ResolveRpgCoverageLabel("support") + " " + GetRpgCoverageLaneCount(laneCounts, "support") + ", " + ResolveRpgCoverageLabel("recovery") + " " + GetRpgCoverageLaneCount(laneCounts, "recovery") + ".";
        if (!string.IsNullOrEmpty(topMissingCoverageKey)) { summary += " Gap: " + ResolveRpgCoverageLabel(topMissingCoverageKey) + "."; }
        else if (context != null && context.ResultStateKey == PrototypeBattleOutcomeKeys.RunClear) { summary += " No urgent lane gap."; }
        else { summary += " Coverage is serviceable but could shift if pressure rises."; }
        return summary;
    }

    private string BuildRpgRoleOverlapWarningSummaryText(Dictionary<string, int> laneCounts, string topOverlapKey)
    {
        if (string.IsNullOrEmpty(topOverlapKey)) { return "Overlap guard: no major lane overlap."; }
        int count = GetRpgCoverageLaneCount(laneCounts, topOverlapKey);
        return "Overlap guard: " + ResolveRpgCoverageLabel(topOverlapKey) + " is stacked across " + count + " member(s); prefer sidegrade or coverage fill over deeper overlap.";
    }
    private string BuildRpgCrossMemberSynergySummaryText(string topSynergyKey, PrototypeRpgGrowthChoiceContext context)
    {
        if (string.IsNullOrEmpty(topSynergyKey)) { return "Synergy: current build has no urgent pair request."; }
        string summary = "Synergy: " + ResolveRpgSynergyLabel(topSynergyKey) + " is the clearest cross-member pair.";
        if (context != null && context.EliteDefeated) { summary += " Elite momentum keeps that pairing attractive."; }
        else if (context != null && (context.ResultStateKey == PrototypeBattleOutcomeKeys.RunDefeat || context.ResultStateKey == PrototypeBattleOutcomeKeys.RunRetreat)) { summary += " Recovery pressure still wants a safer paired shell."; }
        return summary;
    }

    private string BuildRpgPartyCoverageOfferCoherenceSummary(string topMissingCoverageKey, string topOverlapKey, string topSynergyKey)
    {
        string coverageText = string.IsNullOrEmpty(topMissingCoverageKey) ? "no urgent gap" : "fill " + ResolveRpgCoverageLabel(topMissingCoverageKey);
        string overlapText = string.IsNullOrEmpty(topOverlapKey) ? "avoid unnecessary overlap" : "relieve " + ResolveRpgCoverageLabel(topOverlapKey) + " overlap";
        string synergyText = string.IsNullOrEmpty(topSynergyKey) ? "keep the shell balanced" : "reinforce " + ResolveRpgSynergyLabel(topSynergyKey);
        return "Party coherence: " + coverageText + ", " + overlapText + ", " + synergyText + ".";
    }

    private string ResolveRpgCoverageLabel(string laneKey)
    {
        return string.IsNullOrEmpty(laneKey) ? "Balanced" : PrototypeRpgGrowthChoiceRuleCatalog.ResolveLaneLabel(laneKey);
    }

    private string ResolveRpgContributionShapeLabel(string contributionShapeKey)
    {
        switch (contributionShapeKey)
        {
            case "healer": return "healing";
            case "anchor": return "anchor";
            case "finisher": return "finisher";
            case "pressure": return "pressure";
            case "stabilizer": return "stabilizer";
            default: return "balanced";
        }
    }

    private string ResolveRpgSynergyLabel(string synergyKey)
    {
        switch (synergyKey)
        {
            case "frontline_support": return ResolveRpgCoverageLabel("frontline") + " + " + ResolveRpgCoverageLabel("support");
            case "frontline_precision": return ResolveRpgCoverageLabel("frontline") + " + " + ResolveRpgCoverageLabel("precision");
            case "frontline_arcane": return ResolveRpgCoverageLabel("frontline") + " + " + ResolveRpgCoverageLabel("arcane");
            case "support_arcane": return ResolveRpgCoverageLabel("support") + " + " + ResolveRpgCoverageLabel("arcane");
            case "support_precision": return ResolveRpgCoverageLabel("support") + " + " + ResolveRpgCoverageLabel("precision");
            default: return "party pairing";
        }
    }

    private bool DoesRpgSynergyPairContainLane(string synergyKey, string laneKey)
    {
        return !string.IsNullOrEmpty(synergyKey) && !string.IsNullOrEmpty(laneKey) && synergyKey.IndexOf(laneKey, StringComparison.Ordinal) >= 0;
    }

    private int ScoreOfferAgainstPartyCoverage(PrototypeRpgGrowthChoiceContext context, PrototypeRpgUpgradeOfferCandidate candidate, PrototypeRpgGrowthChoiceMemberContext member)
    {
        if (context == null || candidate == null || member == null) { return 0; }
        PrototypeRpgMemberCoverageRuntimeState coverageState = GetRpgMemberCoverageRuntimeState(_sessionRpgPartyCoverageRuntimeState, member.MemberId);
        string topMissingCoverageKey = !string.IsNullOrEmpty(context.TopMissingCoverageKey) ? context.TopMissingCoverageKey : (_sessionRpgPartyCoverageRuntimeState != null ? _sessionRpgPartyCoverageRuntimeState.TopMissingCoverageKey : string.Empty);
        string topOverlapKey = !string.IsNullOrEmpty(context.TopOverlapKey) ? context.TopOverlapKey : (_sessionRpgPartyCoverageRuntimeState != null ? _sessionRpgPartyCoverageRuntimeState.TopOverlapKey : string.Empty);
        string topSynergyKey = !string.IsNullOrEmpty(context.TopSynergyKey) ? context.TopSynergyKey : (_sessionRpgPartyCoverageRuntimeState != null ? _sessionRpgPartyCoverageRuntimeState.TopSynergyKey : string.Empty);
        int coverageScore = 0;
        int overlapScore = 0;
        int synergyScore = 0;
        candidate.ImprovesMissingCoverage = false;
        candidate.ReducesRoleOverlap = false;
        candidate.SupportsCrossMemberSynergy = false;
        candidate.CoverageReasonText = string.Empty;
        candidate.OverlapReasonText = string.Empty;
        candidate.SynergyReasonText = string.Empty;

        if (!string.IsNullOrEmpty(topMissingCoverageKey))
        {
            if (candidate.LaneKey == topMissingCoverageKey)
            {
                coverageScore += 10 + (coverageState != null ? coverageState.MissingCoverageBias : member.MissingCoverageBias);
                candidate.ImprovesMissingCoverage = true;
                candidate.CoverageReasonText = "Coverage bias: adds missing " + ResolveRpgCoverageLabel(topMissingCoverageKey) + " coverage to the party shell.";
            }
            else if (candidate.AdjacentLaneKey == topMissingCoverageKey || member.ResolvedSecondaryLaneKey == topMissingCoverageKey)
            {
                coverageScore += 5 + Math.Max(0, (coverageState != null ? coverageState.MissingCoverageBias : member.MissingCoverageBias) / 2);
                candidate.CoverageReasonText = "Coverage bias: stays close to missing " + ResolveRpgCoverageLabel(topMissingCoverageKey) + " coverage without hard-swerving the current lane.";
            }
            else if (candidate.IsNoOp && member.ResolvedPrimaryLaneKey == topMissingCoverageKey)
            {
                coverageScore += 4;
                candidate.CoverageReasonText = "Coverage bias: holds the current " + ResolveRpgCoverageLabel(topMissingCoverageKey) + " coverage that the party still needs.";
            }
        }

        if (!string.IsNullOrEmpty(topOverlapKey) && member.ResolvedPrimaryLaneKey == topOverlapKey)
        {
            if (candidate.LaneKey == topOverlapKey && !candidate.IsRecoveryEscapeOffer)
            {
                overlapScore -= 8 + (coverageState != null ? coverageState.OverlapPenalty : member.OverlapPenalty);
                candidate.OverlapReasonText = "Overlap guard: more " + ResolveRpgCoverageLabel(topOverlapKey) + " emphasis would deepen current role overlap.";
            }
            else if (!string.IsNullOrEmpty(candidate.LaneKey) && candidate.LaneKey != topOverlapKey && (candidate.IsAdjacentLaneOffer || candidate.LaneKey == topMissingCoverageKey || candidate.IsRecoveryEscapeOffer))
            {
                overlapScore += 6 + Math.Max(0, (coverageState != null ? coverageState.OverlapPenalty : member.OverlapPenalty) / 2);
                candidate.ReducesRoleOverlap = true;
                candidate.OverlapReasonText = "Overlap guard: shifting away from stacked " + ResolveRpgCoverageLabel(topOverlapKey) + " lowers role overlap pressure.";
            }
        }

        if (!string.IsNullOrEmpty(topSynergyKey) && !string.IsNullOrEmpty(candidate.LaneKey) && DoesRpgSynergyPairContainLane(topSynergyKey, candidate.LaneKey))
        {
            synergyScore += 6 + (coverageState != null ? coverageState.SynergyBias : member.SynergyBias);
            candidate.SupportsCrossMemberSynergy = true;
            candidate.SynergyReasonText = "Synergy bias: reinforces " + ResolveRpgSynergyLabel(topSynergyKey) + ".";
        }
        else if (!string.IsNullOrEmpty(topSynergyKey) && !string.IsNullOrEmpty(candidate.AdjacentLaneKey) && DoesRpgSynergyPairContainLane(topSynergyKey, candidate.AdjacentLaneKey))
        {
            synergyScore += 3 + Math.Max(0, (coverageState != null ? coverageState.SynergyBias : member.SynergyBias) / 2);
            candidate.SupportsCrossMemberSynergy = true;
            candidate.SynergyReasonText = "Synergy bias: keeps " + ResolveRpgSynergyLabel(topSynergyKey) + " within sidegrade reach.";
        }

        candidate.CoverageBiasScore = coverageScore;
        candidate.OverlapPenaltyScore = overlapScore;
        candidate.SynergyBiasScore = synergyScore;
        candidate.ReasonText = MergeRpgSummaryText(candidate.ReasonText, candidate.CoverageReasonText);
        candidate.ReasonText = MergeRpgSummaryText(candidate.ReasonText, candidate.OverlapReasonText);
        candidate.ContinuitySummaryText = MergeRpgSummaryText(candidate.ContinuitySummaryText, candidate.SynergyReasonText);
        return coverageScore + overlapScore + synergyScore;
    }

    private string MergeRpgSummaryText(string left, string right)
    {
        if (string.IsNullOrEmpty(left)) { return string.IsNullOrEmpty(right) ? string.Empty : right; }
        if (string.IsNullOrEmpty(right) || left.IndexOf(right, StringComparison.Ordinal) >= 0) { return left; }
        return left + " | " + right;
    }
}

