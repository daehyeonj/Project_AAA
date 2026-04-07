using System;

public sealed partial class StaticPlaceholderWorldView
{
    private PrototypeRpgPartyBuildLaneRuntimeState _sessionRpgPartyBuildLaneRuntimeState = new PrototypeRpgPartyBuildLaneRuntimeState();

    public string CurrentBuildLaneSummaryText => string.IsNullOrEmpty(_sessionRpgPartyBuildLaneRuntimeState.LaneSummaryText) ? "None" : _sessionRpgPartyBuildLaneRuntimeState.LaneSummaryText;
    public string CurrentArchetypeCommitmentSummaryText => string.IsNullOrEmpty(_sessionRpgPartyBuildLaneRuntimeState.ArchetypeCommitmentSummaryText) ? "None" : _sessionRpgPartyBuildLaneRuntimeState.ArchetypeCommitmentSummaryText;
    public PrototypeRpgPartyBuildLaneRuntimeState LatestRpgPartyBuildLaneRuntimeState => CopyRpgPartyBuildLaneRuntimeState(_sessionRpgPartyBuildLaneRuntimeState);

    private void ResetRpgBuildLaneRuntimeState()
    {
        _sessionRpgPartyBuildLaneRuntimeState = new PrototypeRpgPartyBuildLaneRuntimeState();
    }

    private PrototypeRpgPartyBuildLaneRuntimeState CopyRpgPartyBuildLaneRuntimeState(PrototypeRpgPartyBuildLaneRuntimeState source)
    {
        PrototypeRpgPartyBuildLaneRuntimeState copy = new PrototypeRpgPartyBuildLaneRuntimeState();
        if (source == null)
        {
            return copy;
        }

        copy.SessionKey = CopyText(source.SessionKey);
        copy.PartyId = CopyText(source.PartyId);
        copy.DisplayName = CopyText(source.DisplayName);
        copy.LastResolvedRunIdentity = CopyText(source.LastResolvedRunIdentity);
        copy.LaneSummaryText = CopyText(source.LaneSummaryText);
        copy.ArchetypeCommitmentSummaryText = CopyText(source.ArchetypeCommitmentSummaryText);
        copy.OfferCoherenceSummaryText = CopyText(source.OfferCoherenceSummaryText);
        copy.Members = CopyArray(source.Members, CopyRpgMemberBuildLaneRuntimeState);
        return copy;
    }

    private PrototypeRpgMemberBuildLaneRuntimeState CopyRpgMemberBuildLaneRuntimeState(PrototypeRpgMemberBuildLaneRuntimeState source)
    {
        PrototypeRpgMemberBuildLaneRuntimeState copy = new PrototypeRpgMemberBuildLaneRuntimeState();
        if (source == null)
        {
            return copy;
        }

        copy.MemberId = CopyText(source.MemberId);
        copy.DisplayName = CopyText(source.DisplayName);
        copy.ResolvedPrimaryLaneKey = CopyText(source.ResolvedPrimaryLaneKey);
        copy.ResolvedSecondaryLaneKey = CopyText(source.ResolvedSecondaryLaneKey);
        copy.LaneScoreFrontline = ClampNonNegative(source.LaneScoreFrontline);
        copy.LaneScorePrecision = ClampNonNegative(source.LaneScorePrecision);
        copy.LaneScoreArcane = ClampNonNegative(source.LaneScoreArcane);
        copy.LaneScoreSupport = ClampNonNegative(source.LaneScoreSupport);
        copy.LaneScoreRecovery = ClampNonNegative(source.LaneScoreRecovery);
        copy.RecentLaneReasonText = CopyText(source.RecentLaneReasonText);
        copy.LaneSummaryText = CopyText(source.LaneSummaryText);
        copy.CoherenceSummaryText = CopyText(source.CoherenceSummaryText);
        copy.LastLaneResolvedRunIdentity = CopyText(source.LastLaneResolvedRunIdentity);
        copy.CommitmentDepth = ClampNonNegative(source.CommitmentDepth);
        copy.LastArchetypeShiftKey = CopyText(source.LastArchetypeShiftKey);
        return copy;
    }

    private PrototypeRpgPartyBuildLaneRuntimeState GetOrCreateRpgPartyBuildLaneRuntimeState(PrototypeRpgPartyDefinition partyDefinition)
    {
        if (partyDefinition == null)
        {
            return _sessionRpgPartyBuildLaneRuntimeState ?? new PrototypeRpgPartyBuildLaneRuntimeState();
        }

        bool rebuild = _sessionRpgPartyBuildLaneRuntimeState == null ||
                       _sessionRpgPartyBuildLaneRuntimeState.Members == null ||
                       _sessionRpgPartyBuildLaneRuntimeState.PartyId != partyDefinition.PartyId;
        if (rebuild)
        {
            _sessionRpgPartyBuildLaneRuntimeState = new PrototypeRpgPartyBuildLaneRuntimeState();
            _sessionRpgPartyBuildLaneRuntimeState.SessionKey = string.IsNullOrEmpty(partyDefinition.PartyId) ? string.Empty : partyDefinition.PartyId;
            _sessionRpgPartyBuildLaneRuntimeState.PartyId = string.IsNullOrEmpty(partyDefinition.PartyId) ? string.Empty : partyDefinition.PartyId;
            _sessionRpgPartyBuildLaneRuntimeState.DisplayName = string.IsNullOrEmpty(partyDefinition.DisplayName) ? _sessionRpgPartyBuildLaneRuntimeState.PartyId : partyDefinition.DisplayName;
            _sessionRpgPartyBuildLaneRuntimeState.Members = BuildDefaultRpgMemberBuildLaneStates(partyDefinition);
        }
        else
        {
            EnsureRpgMemberBuildLaneStates(partyDefinition, _sessionRpgPartyBuildLaneRuntimeState);
        }

        return _sessionRpgPartyBuildLaneRuntimeState;
    }

    private PrototypeRpgMemberBuildLaneRuntimeState[] BuildDefaultRpgMemberBuildLaneStates(PrototypeRpgPartyDefinition partyDefinition)
    {
        PrototypeRpgPartyMemberDefinition[] definitions = partyDefinition != null ? (partyDefinition.Members ?? Array.Empty<PrototypeRpgPartyMemberDefinition>()) : Array.Empty<PrototypeRpgPartyMemberDefinition>();
        if (definitions.Length <= 0)
        {
            return Array.Empty<PrototypeRpgMemberBuildLaneRuntimeState>();
        }

        PrototypeRpgMemberBuildLaneRuntimeState[] members = new PrototypeRpgMemberBuildLaneRuntimeState[definitions.Length];
        for (int i = 0; i < definitions.Length; i++)
        {
            PrototypeRpgPartyMemberDefinition definition = definitions[i];
            PrototypeRpgMemberBuildLaneRuntimeState state = new PrototypeRpgMemberBuildLaneRuntimeState();
            state.MemberId = string.IsNullOrEmpty(definition != null ? definition.MemberId : string.Empty) ? string.Empty : definition.MemberId;
            state.DisplayName = string.IsNullOrEmpty(definition != null ? definition.DisplayName : string.Empty) ? "Adventurer" : definition.DisplayName;
            members[i] = state;
        }

        return members;
    }
    private void EnsureRpgMemberBuildLaneStates(PrototypeRpgPartyDefinition partyDefinition, PrototypeRpgPartyBuildLaneRuntimeState state)
    {
        if (partyDefinition == null || state == null)
        {
            return;
        }

        PrototypeRpgPartyMemberDefinition[] definitions = partyDefinition.Members ?? Array.Empty<PrototypeRpgPartyMemberDefinition>();
        PrototypeRpgMemberBuildLaneRuntimeState[] current = state.Members ?? Array.Empty<PrototypeRpgMemberBuildLaneRuntimeState>();
        if (definitions.Length == current.Length)
        {
            return;
        }

        PrototypeRpgMemberBuildLaneRuntimeState[] rebuilt = new PrototypeRpgMemberBuildLaneRuntimeState[definitions.Length];
        for (int i = 0; i < definitions.Length; i++)
        {
            PrototypeRpgPartyMemberDefinition definition = definitions[i];
            PrototypeRpgMemberBuildLaneRuntimeState existing = GetRpgMemberBuildLaneRuntimeState(state, definition != null ? definition.MemberId : string.Empty);
            if (existing == null)
            {
                existing = new PrototypeRpgMemberBuildLaneRuntimeState();
                existing.MemberId = string.IsNullOrEmpty(definition != null ? definition.MemberId : string.Empty) ? string.Empty : definition.MemberId;
                existing.DisplayName = string.IsNullOrEmpty(definition != null ? definition.DisplayName : string.Empty) ? "Adventurer" : definition.DisplayName;
            }

            rebuilt[i] = existing;
        }

        state.Members = rebuilt;
    }

    private PrototypeRpgMemberBuildLaneRuntimeState GetRpgMemberBuildLaneRuntimeState(PrototypeRpgPartyBuildLaneRuntimeState state, string memberId)
    {
        if (state == null || state.Members == null || string.IsNullOrEmpty(memberId))
        {
            return null;
        }

        for (int i = 0; i < state.Members.Length; i++)
        {
            PrototypeRpgMemberBuildLaneRuntimeState candidate = state.Members[i];
            if (candidate != null && candidate.MemberId == memberId)
            {
                return candidate;
            }
        }

        return null;
    }

    private void EvaluateRpgBuildLaneState(PrototypeRpgGrowthChoiceContext context, PrototypeRpgRunResultSnapshot runResultSnapshot, PrototypeRpgPartyDefinition partyDefinition)
    {
        if (context == null || partyDefinition == null)
        {
            return;
        }

        PrototypeRpgPartyBuildLaneRuntimeState partyState = GetOrCreateRpgPartyBuildLaneRuntimeState(partyDefinition);
        PrototypeRpgGrowthChoiceMemberContext[] members = context.Members ?? Array.Empty<PrototypeRpgGrowthChoiceMemberContext>();
        PrototypeRpgMemberBuildLaneRuntimeState[] resolvedStates = new PrototypeRpgMemberBuildLaneRuntimeState[members.Length];
        for (int i = 0; i < members.Length; i++)
        {
            PrototypeRpgGrowthChoiceMemberContext member = members[i] ?? new PrototypeRpgGrowthChoiceMemberContext();
            PrototypeRpgMemberBuildLaneRuntimeState existing = GetRpgMemberBuildLaneRuntimeState(partyState, member.MemberId) ?? new PrototypeRpgMemberBuildLaneRuntimeState();
            int frontline = 0;
            int precision = 0;
            int arcane = 0;
            int support = 0;
            int recovery = 0;

            ApplyRpgBaseRoleLaneBias(member.RoleTag, ref frontline, ref precision, ref arcane, ref support, ref recovery);
            ApplyRpgCommitmentMemoryBias(existing, ref frontline, ref precision, ref arcane, ref support, ref recovery);
            ApplyRpgIdentityLaneBias(member.CurrentGrowthTrackId, member.CurrentJobId, member.CurrentEquipmentLoadoutId, member.CurrentSkillLoadoutId, ref frontline, ref precision, ref arcane, ref support, ref recovery);
            ApplyRpgOfferTypeLaneBias(member.LastAppliedOfferTypeKey, member.RoleTag, ref frontline, ref precision, ref arcane, ref support, ref recovery);
            ApplyRpgContributionLaneBias(member, context, ref frontline, ref precision, ref arcane, ref support, ref recovery);

            string primaryLane = ResolveRpgHighestLaneKey(frontline, precision, arcane, support, recovery, member.RoleTag, true);
            string secondaryLane = ResolveRpgHighestLaneKey(frontline, precision, arcane, support, recovery, member.RoleTag, false, primaryLane);
            int commitmentDepth = ResolveRpgArchetypeCommitmentDepth(existing, primaryLane, secondaryLane, frontline, precision, arcane, support, recovery, context, member);
            string shiftKey = !string.IsNullOrEmpty(existing.ResolvedPrimaryLaneKey) && existing.ResolvedPrimaryLaneKey != primaryLane ? existing.ResolvedPrimaryLaneKey + '>' + primaryLane : string.Empty;
            string laneReason = BuildRpgMemberLaneReasonText(member, context, primaryLane, secondaryLane, commitmentDepth, existing);
            string laneSummary = BuildRpgMemberLaneSummaryText(member.DisplayName, primaryLane, secondaryLane, commitmentDepth, laneReason);

            existing.MemberId = string.IsNullOrEmpty(member.MemberId) ? string.Empty : member.MemberId;
            existing.DisplayName = string.IsNullOrEmpty(member.DisplayName) ? "Adventurer" : member.DisplayName;
            existing.ResolvedPrimaryLaneKey = primaryLane;
            existing.ResolvedSecondaryLaneKey = secondaryLane;
            existing.LaneScoreFrontline = Math.Max(0, frontline);
            existing.LaneScorePrecision = Math.Max(0, precision);
            existing.LaneScoreArcane = Math.Max(0, arcane);
            existing.LaneScoreSupport = Math.Max(0, support);
            existing.LaneScoreRecovery = Math.Max(0, recovery);
            existing.RecentLaneReasonText = laneReason;
            existing.LaneSummaryText = laneSummary;
            existing.CoherenceSummaryText = BuildRpgMemberLaneCoherenceSummary(primaryLane, secondaryLane, commitmentDepth);
            existing.LastLaneResolvedRunIdentity = string.IsNullOrEmpty(context.RunIdentity) ? string.Empty : context.RunIdentity;
            existing.CommitmentDepth = commitmentDepth;
            existing.LastArchetypeShiftKey = shiftKey;
            resolvedStates[i] = existing;

            member.ResolvedPrimaryLaneKey = primaryLane;
            member.ResolvedSecondaryLaneKey = secondaryLane;
            member.LaneScoreFrontline = existing.LaneScoreFrontline;
            member.LaneScorePrecision = existing.LaneScorePrecision;
            member.LaneScoreArcane = existing.LaneScoreArcane;
            member.LaneScoreSupport = existing.LaneScoreSupport;
            member.LaneScoreRecovery = existing.LaneScoreRecovery;
            member.CommitmentDepth = commitmentDepth;
            member.RecentLaneReasonText = laneReason;
            member.LaneSummaryText = laneSummary;
        }

        partyState.Members = resolvedStates;
        partyState.LastResolvedRunIdentity = string.IsNullOrEmpty(context.RunIdentity) ? string.Empty : context.RunIdentity;
        partyState.LaneSummaryText = BuildRpgPartyBuildLaneSummaryText(resolvedStates);
        partyState.ArchetypeCommitmentSummaryText = BuildRpgArchetypeCommitmentSummaryText(resolvedStates);
        partyState.OfferCoherenceSummaryText = BuildRpgLaneOfferCoherenceSummaryText(resolvedStates);
        context.BuildLaneSummaryText = partyState.LaneSummaryText;
        context.ArchetypeCommitmentSummaryText = partyState.ArchetypeCommitmentSummaryText;
        context.OfferCoherenceSummaryText = partyState.OfferCoherenceSummaryText;
        context.MemberBuildLanes = CopyRpgMemberBuildLaneRuntimeStates(resolvedStates);
    }

    private PrototypeRpgMemberBuildLaneRuntimeState[] CopyRpgMemberBuildLaneRuntimeStates(PrototypeRpgMemberBuildLaneRuntimeState[] source)
    {
        if (source == null || source.Length <= 0)
        {
            return Array.Empty<PrototypeRpgMemberBuildLaneRuntimeState>();
        }

        PrototypeRpgMemberBuildLaneRuntimeState[] copies = new PrototypeRpgMemberBuildLaneRuntimeState[source.Length];
        for (int i = 0; i < source.Length; i++)
        {
            copies[i] = CopyRpgMemberBuildLaneRuntimeState(source[i]);
        }

        return copies;
    }

    private void ApplyRpgBaseRoleLaneBias(string roleTag, ref int frontline, ref int precision, ref int arcane, ref int support, ref int recovery)
    {
        switch (PrototypeRpgGrowthChoiceRuleCatalog.ResolveRoleCoreLaneKey(roleTag))
        {
            case "frontline":
                frontline += 18;
                recovery += 4;
                break;
            case "precision":
                precision += 18;
                frontline += 3;
                break;
            case "arcane":
                arcane += 18;
                support += 3;
                recovery += 2;
                break;
            case "support":
                support += 18;
                recovery += 8;
                break;
            default:
                recovery += 4;
                break;
        }
    }
    private void ApplyRpgCommitmentMemoryBias(PrototypeRpgMemberBuildLaneRuntimeState existing, ref int frontline, ref int precision, ref int arcane, ref int support, ref int recovery)
    {
        if (existing == null || existing.CommitmentDepth <= 0)
        {
            return;
        }

        ApplyRpgLaneDelta(existing.ResolvedPrimaryLaneKey, 4 + existing.CommitmentDepth * 3, ref frontline, ref precision, ref arcane, ref support, ref recovery);
        ApplyRpgLaneDelta(existing.ResolvedSecondaryLaneKey, 2 + existing.CommitmentDepth, ref frontline, ref precision, ref arcane, ref support, ref recovery);
    }

    private void ApplyRpgIdentityLaneBias(string growthTrackId, string jobId, string equipmentLoadoutId, string skillLoadoutId, ref int frontline, ref int precision, ref int arcane, ref int support, ref int recovery)
    {
        ApplyRpgIdentityTextLaneBias(growthTrackId, 12, ref frontline, ref precision, ref arcane, ref support, ref recovery);
        ApplyRpgIdentityTextLaneBias(jobId, 10, ref frontline, ref precision, ref arcane, ref support, ref recovery);
        ApplyRpgIdentityTextLaneBias(equipmentLoadoutId, 8, ref frontline, ref precision, ref arcane, ref support, ref recovery);
        ApplyRpgIdentityTextLaneBias(skillLoadoutId, 8, ref frontline, ref precision, ref arcane, ref support, ref recovery);
    }

    private void ApplyRpgIdentityTextLaneBias(string value, int amount, ref int frontline, ref int precision, ref int arcane, ref int support, ref int recovery)
    {
        string normalized = string.IsNullOrWhiteSpace(value) ? string.Empty : value.Trim().ToLowerInvariant();
        if (string.IsNullOrEmpty(normalized))
        {
            return;
        }

        if (normalized.Contains("frontline") || normalized.Contains("breaker") || normalized.Contains("guardian") || normalized.Contains("vanguard") || normalized.Contains("bastion"))
        {
            frontline += amount;
        }

        if (normalized.Contains("precision") || normalized.Contains("execution") || normalized.Contains("shadow") || normalized.Contains("saboteur") || normalized.Contains("phantom"))
        {
            precision += amount;
        }

        if (normalized.Contains("arcane") || normalized.Contains("surge") || normalized.Contains("ward") || normalized.Contains("tempest") || normalized.Contains("astral"))
        {
            arcane += amount;
        }

        if (normalized.Contains("support") || normalized.Contains("choir") || normalized.Contains("beacon") || normalized.Contains("chaplain") || normalized.Contains("saint"))
        {
            support += amount;
        }

        if (normalized.Contains("recover") || normalized.Contains("guard") || normalized.Contains("warden") || normalized.Contains("warded") || normalized.Contains("guarded"))
        {
            recovery += Math.Max(4, amount / 2);
        }
    }

    private void ApplyRpgOfferTypeLaneBias(string offerTypeKey, string roleTag, ref int frontline, ref int precision, ref int arcane, ref int support, ref int recovery)
    {
        string normalized = string.IsNullOrWhiteSpace(offerTypeKey) ? string.Empty : offerTypeKey.Trim().ToLowerInvariant();
        string coreLane = PrototypeRpgGrowthChoiceRuleCatalog.ResolveRoleCoreLaneKey(roleTag);
        switch (normalized)
        {
            case "offense_path":
                ApplyRpgLaneDelta(coreLane, 8, ref frontline, ref precision, ref arcane, ref support, ref recovery);
                break;
            case "defense_path":
                ApplyRpgLaneDelta(coreLane == "frontline" ? "frontline" : "recovery", 8, ref frontline, ref precision, ref arcane, ref support, ref recovery);
                recovery += 4;
                break;
            case "support_path":
                support += 10;
                recovery += 4;
                break;
            case "utility_path":
                ApplyRpgLaneDelta(PrototypeRpgGrowthChoiceRuleCatalog.ResolveRoleAdjacentLaneKey(roleTag), 7, ref frontline, ref precision, ref arcane, ref support, ref recovery);
                break;
            case "advanced_path":
                ApplyRpgLaneDelta(coreLane, 10, ref frontline, ref precision, ref arcane, ref support, ref recovery);
                break;
            case "elite_path":
                ApplyRpgLaneDelta(coreLane, 12, ref frontline, ref precision, ref arcane, ref support, ref recovery);
                break;
        }
    }

    private void ApplyRpgContributionLaneBias(PrototypeRpgGrowthChoiceMemberContext member, PrototypeRpgGrowthChoiceContext context, ref int frontline, ref int precision, ref int arcane, ref int support, ref int recovery)
    {
        if (member == null)
        {
            return;
        }

        string coreLane = PrototypeRpgGrowthChoiceRuleCatalog.ResolveRoleCoreLaneKey(member.RoleTag);
        ApplyRpgLaneDelta(coreLane, Math.Max(0, member.DamageDealt / 4), ref frontline, ref precision, ref arcane, ref support, ref recovery);
        ApplyRpgLaneDelta(coreLane, member.KillCount * 3, ref frontline, ref precision, ref arcane, ref support, ref recovery);
        support += Math.Max(0, member.HealingDone / 3);
        recovery += member.KnockedOut ? 16 : 0;
        recovery += member.DamageTaken > member.DamageDealt ? 8 : 0;
        recovery += !member.Survived ? 6 : 0;
        if (context != null && (context.ResultStateKey == PrototypeBattleOutcomeKeys.RunDefeat || context.ResultStateKey == PrototypeBattleOutcomeKeys.RunRetreat))
        {
            recovery += 6;
            support += 2;
        }

        if (context != null && context.EliteDefeated)
        {
            ApplyRpgLaneDelta(coreLane, 4, ref frontline, ref precision, ref arcane, ref support, ref recovery);
        }

        if (context != null && context.RiskyRoute && context.ResultStateKey == PrototypeBattleOutcomeKeys.RunClear)
        {
            ApplyRpgLaneDelta(coreLane, 4, ref frontline, ref precision, ref arcane, ref support, ref recovery);
        }

        if (context != null && context.HasRewardCarryover)
        {
            ApplyRpgLaneDelta(coreLane, 2, ref frontline, ref precision, ref arcane, ref support, ref recovery);
        }
    }

    private void ApplyRpgLaneDelta(string laneKey, int amount, ref int frontline, ref int precision, ref int arcane, ref int support, ref int recovery)
    {
        if (amount == 0)
        {
            return;
        }

        switch (string.IsNullOrWhiteSpace(laneKey) ? string.Empty : laneKey.Trim().ToLowerInvariant())
        {
            case "frontline":
                frontline += amount;
                break;
            case "precision":
                precision += amount;
                break;
            case "arcane":
                arcane += amount;
                break;
            case "support":
                support += amount;
                break;
            case "recovery":
                recovery += amount;
                break;
        }
    }

    private string ResolveRpgHighestLaneKey(int frontline, int precision, int arcane, int support, int recovery, string roleTag, bool primary, string excludedLaneKey = "")
    {
        string[] laneOrder = new[]
        {
            PrototypeRpgGrowthChoiceRuleCatalog.ResolveRoleCoreLaneKey(roleTag),
            PrototypeRpgGrowthChoiceRuleCatalog.ResolveRoleAdjacentLaneKey(roleTag),
            "frontline",
            "precision",
            "arcane",
            "support",
            "recovery"
        };

        int bestScore = int.MinValue;
        string bestLane = string.Empty;
        for (int i = 0; i < laneOrder.Length; i++)
        {
            string lane = laneOrder[i];
            if (string.IsNullOrEmpty(lane) || lane == excludedLaneKey)
            {
                continue;
            }

            int score = GetRpgLaneScore(lane, frontline, precision, arcane, support, recovery);
            if (score > bestScore)
            {
                bestScore = score;
                bestLane = lane;
            }
        }

        if (string.IsNullOrEmpty(bestLane))
        {
            return primary ? PrototypeRpgGrowthChoiceRuleCatalog.ResolveRoleCoreLaneKey(roleTag) : PrototypeRpgGrowthChoiceRuleCatalog.ResolveRoleAdjacentLaneKey(roleTag);
        }

        return bestLane;
    }
    private int GetRpgLaneScore(string laneKey, int frontline, int precision, int arcane, int support, int recovery)
    {
        switch (laneKey)
        {
            case "frontline":
                return frontline;
            case "precision":
                return precision;
            case "arcane":
                return arcane;
            case "support":
                return support;
            case "recovery":
                return recovery;
            default:
                return int.MinValue;
        }
    }

    private int ResolveRpgArchetypeCommitmentDepth(PrototypeRpgMemberBuildLaneRuntimeState existing, string primaryLane, string secondaryLane, int frontline, int precision, int arcane, int support, int recovery, PrototypeRpgGrowthChoiceContext context, PrototypeRpgGrowthChoiceMemberContext member)
    {
        int currentDepth = existing != null ? existing.CommitmentDepth : 0;
        int primaryScore = GetRpgLaneScore(primaryLane, frontline, precision, arcane, support, recovery);
        int secondaryScore = GetRpgLaneScore(secondaryLane, frontline, precision, arcane, support, recovery);
        if (string.IsNullOrEmpty(primaryLane))
        {
            return 0;
        }

        if (existing != null && existing.ResolvedPrimaryLaneKey == primaryLane)
        {
            currentDepth += primaryScore >= secondaryScore + 6 ? 1 : 0;
        }
        else if (!string.IsNullOrEmpty(existing != null ? existing.ResolvedPrimaryLaneKey : string.Empty) && existing.ResolvedPrimaryLaneKey != primaryLane)
        {
            currentDepth = Math.Max(1, currentDepth - 1);
        }
        else if (primaryScore >= 18)
        {
            currentDepth = 1;
        }

        if (context != null && context.EliteDefeated && primaryLane != "recovery")
        {
            currentDepth += 1;
        }

        if (context != null && context.RiskyRoute && context.ResultStateKey == PrototypeBattleOutcomeKeys.RunClear && primaryLane != "recovery")
        {
            currentDepth += 1;
        }

        if (member != null && (member.KnockedOut || (context != null && (context.ResultStateKey == PrototypeBattleOutcomeKeys.RunDefeat || context.ResultStateKey == PrototypeBattleOutcomeKeys.RunRetreat))))
        {
            if (primaryLane == "recovery")
            {
                currentDepth = Math.Max(1, currentDepth);
            }
            else
            {
                currentDepth = Math.Max(0, currentDepth - 1);
            }
        }

        return Math.Max(0, Math.Min(5, currentDepth));
    }

    private string BuildRpgMemberLaneReasonText(PrototypeRpgGrowthChoiceMemberContext member, PrototypeRpgGrowthChoiceContext context, string primaryLane, string secondaryLane, int commitmentDepth, PrototypeRpgMemberBuildLaneRuntimeState existing)
    {
        string primaryLabel = PrototypeRpgGrowthChoiceRuleCatalog.ResolveLaneLabel(primaryLane);
        string secondaryLabel = PrototypeRpgGrowthChoiceRuleCatalog.ResolveLaneLabel(secondaryLane);
        string shiftText = !string.IsNullOrEmpty(existing != null ? existing.ResolvedPrimaryLaneKey : string.Empty) && existing.ResolvedPrimaryLaneKey != primaryLane
            ? "Shifted from " + PrototypeRpgGrowthChoiceRuleCatalog.ResolveLaneLabel(existing.ResolvedPrimaryLaneKey) + " toward " + primaryLabel + "."
            : primaryLabel + " stayed the clearest lane.";
        string resultBiasText = member != null && member.KnockedOut
            ? " KO pressure kept a recovery escape hatch open."
            : (context != null && context.ResultStateKey == PrototypeBattleOutcomeKeys.RunDefeat
                ? " Defeat pressure softened hard commitment."
                : (context != null && context.ResultStateKey == PrototypeBattleOutcomeKeys.RunRetreat
                    ? " Retreat pressure kept recovery and sidegrade routes visible."
                    : string.Empty));
        string depthText = " Depth " + commitmentDepth + " with " + secondaryLabel + " as the secondary lane.";
        return shiftText + depthText + resultBiasText;
    }

    private string BuildRpgMemberLaneSummaryText(string displayName, string primaryLane, string secondaryLane, int commitmentDepth, string laneReason)
    {
        return (string.IsNullOrEmpty(displayName) ? "Adventurer" : displayName) + " lane: " + PrototypeRpgGrowthChoiceRuleCatalog.ResolveLaneLabel(primaryLane) + " / " + PrototypeRpgGrowthChoiceRuleCatalog.ResolveLaneLabel(secondaryLane) + " (depth " + commitmentDepth + ") | " + laneReason;
    }

    private string BuildRpgMemberLaneCoherenceSummary(string primaryLane, string secondaryLane, int commitmentDepth)
    {
        return "Lane coherence: " + PrototypeRpgGrowthChoiceRuleCatalog.ResolveLaneLabel(primaryLane) + " core, " + PrototypeRpgGrowthChoiceRuleCatalog.ResolveLaneLabel(secondaryLane) + " sidegrade, depth " + commitmentDepth + ".";
    }

    private string BuildRpgPartyBuildLaneSummaryText(PrototypeRpgMemberBuildLaneRuntimeState[] members)
    {
        if (members == null || members.Length <= 0)
        {
            return "Build lanes: none.";
        }

        string summary = string.Empty;
        for (int i = 0; i < members.Length; i++)
        {
            PrototypeRpgMemberBuildLaneRuntimeState member = members[i];
            if (member == null)
            {
                continue;
            }

            string chunk = (string.IsNullOrEmpty(member.DisplayName) ? "Adventurer" : member.DisplayName) + " " + PrototypeRpgGrowthChoiceRuleCatalog.ResolveLaneLabel(member.ResolvedPrimaryLaneKey) + " d" + Math.Max(0, member.CommitmentDepth);
            summary = string.IsNullOrEmpty(summary) ? chunk : summary + " | " + chunk;
        }

        return string.IsNullOrEmpty(summary) ? "Build lanes: none." : "Build lanes: " + summary;
    }

    private string BuildRpgArchetypeCommitmentSummaryText(PrototypeRpgMemberBuildLaneRuntimeState[] members)
    {
        if (members == null || members.Length <= 0)
        {
            return "Commitment: none.";
        }

        int committedCount = 0;
        int flexibleCount = 0;
        int recoveryCount = 0;
        for (int i = 0; i < members.Length; i++)
        {
            PrototypeRpgMemberBuildLaneRuntimeState member = members[i];
            if (member == null)
            {
                continue;
            }

            if (member.ResolvedPrimaryLaneKey == "recovery")
            {
                recoveryCount += 1;
            }

            if (member.CommitmentDepth >= 3)
            {
                committedCount += 1;
            }
            else
            {
                flexibleCount += 1;
            }
        }

        return "Commitment: " + committedCount + " committed, " + flexibleCount + " flexible, " + recoveryCount + " recovery-leaning.";
    }

    private string BuildRpgLaneOfferCoherenceSummaryText(PrototypeRpgMemberBuildLaneRuntimeState[] members)
    {
        if (members == null || members.Length <= 0)
        {
            return "Offer coherence: none.";
        }

        int deepCount = 0;
        int broadCount = 0;
        for (int i = 0; i < members.Length; i++)
        {
            PrototypeRpgMemberBuildLaneRuntimeState member = members[i];
            if (member == null)
            {
                continue;
            }

            if (member.CommitmentDepth >= 3)
            {
                deepCount += 1;
            }
            else
            {
                broadCount += 1;
            }
        }

        return "Offer coherence: " + deepCount + " deeper core lane(s), " + broadCount + " broader sidegrade lane(s).";
    }
    private int ScoreOfferAgainstBuildLane(PrototypeRpgGrowthChoiceContext context, PrototypeRpgUpgradeOfferCandidate candidate, PrototypeRpgGrowthChoiceMemberContext member)
    {
        if (candidate == null || member == null)
        {
            return 0;
        }

        candidate.PrimaryLaneKeyAtSelection = string.IsNullOrEmpty(member.ResolvedPrimaryLaneKey) ? string.Empty : member.ResolvedPrimaryLaneKey;
        candidate.SecondaryLaneKeyAtSelection = string.IsNullOrEmpty(member.ResolvedSecondaryLaneKey) ? string.Empty : member.ResolvedSecondaryLaneKey;
        candidate.CommitmentDepthAtSelection = Math.Max(0, member.CommitmentDepth);
        candidate.IsAdjacentLaneOffer = !string.IsNullOrEmpty(candidate.LaneKey) && (candidate.LaneKey == member.ResolvedSecondaryLaneKey || candidate.AdjacentLaneKey == member.ResolvedPrimaryLaneKey);
        candidate.IsRecoveryEscapeOffer = candidate.LaneKey == "recovery" || ((context != null && (context.ResultStateKey == PrototypeBattleOutcomeKeys.RunDefeat || context.ResultStateKey == PrototypeBattleOutcomeKeys.RunRetreat)) && candidate.OfferTypeKey == "defense_path");
        candidate.IsContradictoryToLane = !string.IsNullOrEmpty(member.ResolvedPrimaryLaneKey) && !candidate.IsNoOp && candidate.LaneKey != member.ResolvedPrimaryLaneKey && !candidate.IsAdjacentLaneOffer && !candidate.IsRecoveryEscapeOffer;

        int score = 0;
        if (candidate.LaneKey == member.ResolvedPrimaryLaneKey)
        {
            score += 18 + member.CommitmentDepth * 6 + candidate.CommitmentWeight * 2;
        }
        else if (candidate.IsAdjacentLaneOffer)
        {
            score += 8 + candidate.SidegradeAllowance + Math.Max(0, 3 - member.CommitmentDepth);
        }
        else if (candidate.IsRecoveryEscapeOffer)
        {
            score += candidate.RecoveryEscapeWeight + (member.KnockedOut ? 8 : 0) + ((context != null && (context.ResultStateKey == PrototypeBattleOutcomeKeys.RunDefeat || context.ResultStateKey == PrototypeBattleOutcomeKeys.RunRetreat)) ? 6 : 0);
        }
        else if (candidate.IsContradictoryToLane)
        {
            score -= candidate.ContradictionPenalty + member.CommitmentDepth * 4;
        }
        else
        {
            score -= 2;
        }

        if (candidate.UsedUnlockExpansion && candidate.LaneKey == member.ResolvedPrimaryLaneKey)
        {
            score += 4;
        }

        if (candidate.UsedTierEscalation && candidate.LaneKey == member.ResolvedPrimaryLaneKey)
        {
            score += 6;
        }

        if (context != null && context.EliteDefeated && candidate.LaneKey == member.ResolvedPrimaryLaneKey)
        {
            score += 4;
        }

        candidate.LaneBiasScore = score;
        candidate.LaneCoherenceReasonText = BuildRpgLaneCoherenceReasonText(context, candidate, member);
        return score;
    }

    private bool ShouldSuppressRpgContradictoryOfferCandidate(PrototypeRpgGrowthChoiceContext context, PrototypeRpgUpgradeOfferCandidate candidate, PrototypeRpgGrowthChoiceMemberContext member)
    {
        if (candidate == null || member == null || candidate.IsFallback || candidate.IsNoOp)
        {
            return false;
        }

        if (member.CommitmentDepth < 3)
        {
            return false;
        }

        if (candidate.LaneKey == member.ResolvedPrimaryLaneKey || candidate.LaneKey == member.ResolvedSecondaryLaneKey)
        {
            return false;
        }

        if (candidate.LaneKey == "recovery" && (member.KnockedOut || (context != null && (context.ResultStateKey == PrototypeBattleOutcomeKeys.RunDefeat || context.ResultStateKey == PrototypeBattleOutcomeKeys.RunRetreat))))
        {
            return false;
        }

        return true;
    }

    private string BuildRpgLaneCoherenceReasonText(PrototypeRpgGrowthChoiceContext context, PrototypeRpgUpgradeOfferCandidate candidate, PrototypeRpgGrowthChoiceMemberContext member)
    {
        if (candidate == null || member == null)
        {
            return string.Empty;
        }

        string primaryLabel = PrototypeRpgGrowthChoiceRuleCatalog.ResolveLaneLabel(member.ResolvedPrimaryLaneKey);
        string secondaryLabel = PrototypeRpgGrowthChoiceRuleCatalog.ResolveLaneLabel(member.ResolvedSecondaryLaneKey);
        string laneLabel = PrototypeRpgGrowthChoiceRuleCatalog.ResolveLaneLabel(candidate.LaneKey);
        if (candidate.LaneKey == member.ResolvedPrimaryLaneKey)
        {
            return "Lane coherence: " + laneLabel + " stays the primary build lane at depth " + member.CommitmentDepth + ".";
        }

        if (candidate.IsAdjacentLaneOffer)
        {
            return "Adjacent sidegrade: " + laneLabel + " stays next to the committed " + primaryLabel + " lane while keeping " + secondaryLabel + " flexible.";
        }

        if (candidate.IsRecoveryEscapeOffer)
        {
            return "Recovery escape: sustain pressure keeps a guarded fallback available beside " + primaryLabel + ".";
        }

        if (candidate.IsContradictoryToLane)
        {
            return "Contradiction penalty: " + laneLabel + " cuts across the committed " + primaryLabel + " lane too sharply right now.";
        }

        return string.IsNullOrEmpty(candidate.CoherenceReasonHint) ? "Lane coherence: offer stays loosely aligned with the current build." : candidate.CoherenceReasonHint;
    }
}
