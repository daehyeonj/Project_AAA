using System;
using System.Collections.Generic;
using UnityEngine;

public sealed partial class StaticPlaceholderWorldView
{
    private sealed class PrototypeWorldSimOpportunityCandidateData
    {
        public static readonly PrototypeWorldSimOpportunityCandidateData Empty = new PrototypeWorldSimOpportunityCandidateData(
            string.Empty,
            string.Empty,
            "None",
            string.Empty,
            "None",
            string.Empty,
            "None",
            "none",
            "None",
            "None",
            "None",
            "None",
            "None",
            "None",
            "none",
            "None",
            0,
            true);

        public readonly string CandidateId;
        public readonly string CityId;
        public readonly string CityLabel;
        public readonly string DungeonId;
        public readonly string DungeonLabel;
        public readonly string RouteId;
        public readonly string RouteLabel;
        public readonly string ActionStateKey;
        public readonly string ActionLabel;
        public readonly string CandidateLabel;
        public readonly string ReasonText;
        public readonly string BlockedReasonText;
        public readonly string ConsequenceText;
        public readonly string ComparisonText;
        public readonly string RiskStateKey;
        public readonly string ReadinessStateKey;
        public readonly int Score;
        public readonly bool IsBlocked;

        public bool HasCity => !string.IsNullOrEmpty(CityId);
        public bool HasDungeon => !string.IsNullOrEmpty(DungeonId);
        public bool HasRoute => !string.IsNullOrEmpty(RouteId);

        public PrototypeWorldSimOpportunityCandidateData(
            string candidateId,
            string cityId,
            string cityLabel,
            string dungeonId,
            string dungeonLabel,
            string routeId,
            string routeLabel,
            string actionStateKey,
            string actionLabel,
            string candidateLabel,
            string reasonText,
            string blockedReasonText,
            string consequenceText,
            string comparisonText,
            string riskStateKey,
            string readinessStateKey,
            int score,
            bool isBlocked)
        {
            CandidateId = string.IsNullOrEmpty(candidateId) ? string.Empty : candidateId;
            CityId = string.IsNullOrEmpty(cityId) ? string.Empty : cityId;
            CityLabel = string.IsNullOrEmpty(cityLabel) ? "None" : cityLabel;
            DungeonId = string.IsNullOrEmpty(dungeonId) ? string.Empty : dungeonId;
            DungeonLabel = string.IsNullOrEmpty(dungeonLabel) ? "None" : dungeonLabel;
            RouteId = string.IsNullOrEmpty(routeId) ? string.Empty : routeId;
            RouteLabel = string.IsNullOrEmpty(routeLabel) ? "None" : routeLabel;
            ActionStateKey = string.IsNullOrEmpty(actionStateKey) ? "none" : actionStateKey;
            ActionLabel = string.IsNullOrEmpty(actionLabel) ? "None" : actionLabel;
            CandidateLabel = string.IsNullOrEmpty(candidateLabel) ? "None" : candidateLabel;
            ReasonText = string.IsNullOrEmpty(reasonText) ? "None" : reasonText;
            BlockedReasonText = string.IsNullOrEmpty(blockedReasonText) ? "None" : blockedReasonText;
            ConsequenceText = string.IsNullOrEmpty(consequenceText) ? "None" : consequenceText;
            ComparisonText = string.IsNullOrEmpty(comparisonText) ? "None" : comparisonText;
            RiskStateKey = string.IsNullOrEmpty(riskStateKey) ? "none" : riskStateKey;
            ReadinessStateKey = string.IsNullOrEmpty(readinessStateKey) ? "None" : readinessStateKey;
            Score = score;
            IsBlocked = isBlocked;
        }
    }

    private sealed class PrototypeWorldSimOpportunityLadderSurfaceData
    {
        public static readonly PrototypeWorldSimOpportunityLadderSurfaceData Empty = new PrototypeWorldSimOpportunityLadderSurfaceData(
            string.Empty,
            "None",
            "None",
            PrototypeWorldSimOpportunityCandidateData.Empty,
            PrototypeWorldSimOpportunityCandidateData.Empty,
            PrototypeWorldSimOpportunityCandidateData.Empty,
            "None",
            "None",
            "None",
            "None",
            "None",
            "None");

        public readonly string SelectedEntityId;
        public readonly string SelectedEntityLabel;
        public readonly string SelectedEntityKind;
        public readonly PrototypeWorldSimOpportunityCandidateData PrimaryCandidate;
        public readonly PrototypeWorldSimOpportunityCandidateData SecondaryCandidate;
        public readonly PrototypeWorldSimOpportunityCandidateData TertiaryCandidate;
        public readonly string LadderSummaryText;
        public readonly string ConsequencePreviewText;
        public readonly string CommitReasonSummaryText;
        public readonly string BlockedReasonSummaryText;
        public readonly string AlternateMoveSummaryText;
        public readonly string NextMoveSummaryText;

        public PrototypeWorldSimOpportunityLadderSurfaceData(
            string selectedEntityId,
            string selectedEntityLabel,
            string selectedEntityKind,
            PrototypeWorldSimOpportunityCandidateData primaryCandidate,
            PrototypeWorldSimOpportunityCandidateData secondaryCandidate,
            PrototypeWorldSimOpportunityCandidateData tertiaryCandidate,
            string ladderSummaryText,
            string consequencePreviewText,
            string commitReasonSummaryText,
            string blockedReasonSummaryText,
            string alternateMoveSummaryText,
            string nextMoveSummaryText)
        {
            SelectedEntityId = string.IsNullOrEmpty(selectedEntityId) ? string.Empty : selectedEntityId;
            SelectedEntityLabel = string.IsNullOrEmpty(selectedEntityLabel) ? "None" : selectedEntityLabel;
            SelectedEntityKind = string.IsNullOrEmpty(selectedEntityKind) ? "None" : selectedEntityKind;
            PrimaryCandidate = primaryCandidate ?? PrototypeWorldSimOpportunityCandidateData.Empty;
            SecondaryCandidate = secondaryCandidate ?? PrototypeWorldSimOpportunityCandidateData.Empty;
            TertiaryCandidate = tertiaryCandidate ?? PrototypeWorldSimOpportunityCandidateData.Empty;
            LadderSummaryText = string.IsNullOrEmpty(ladderSummaryText) ? "None" : ladderSummaryText;
            ConsequencePreviewText = string.IsNullOrEmpty(consequencePreviewText) ? "None" : consequencePreviewText;
            CommitReasonSummaryText = string.IsNullOrEmpty(commitReasonSummaryText) ? "None" : commitReasonSummaryText;
            BlockedReasonSummaryText = string.IsNullOrEmpty(blockedReasonSummaryText) ? "None" : blockedReasonSummaryText;
            AlternateMoveSummaryText = string.IsNullOrEmpty(alternateMoveSummaryText) ? "None" : alternateMoveSummaryText;
            NextMoveSummaryText = string.IsNullOrEmpty(nextMoveSummaryText) ? "None" : nextMoveSummaryText;
        }
    }

    private PrototypeWorldSimOpportunityLadderSurfaceData _currentOpportunityLadderSurface = PrototypeWorldSimOpportunityLadderSurfaceData.Empty;
    private string _currentOpportunityLadderSummaryText = "None";
    private string _currentConsequencePreviewSummaryText = "None";
    private string _currentCommitReasonSummaryText = "None";
    private string _currentBlockedReasonSummaryText = "None";
    private string _currentAlternateMoveSummaryText = "None";

    public string CurrentOpportunityLadderSummaryText => string.IsNullOrEmpty(_currentOpportunityLadderSummaryText) ? "None" : _currentOpportunityLadderSummaryText;
    public string CurrentConsequencePreviewSummaryText => string.IsNullOrEmpty(_currentConsequencePreviewSummaryText) ? "None" : _currentConsequencePreviewSummaryText;
    public string CurrentCommitReasonSummaryText => string.IsNullOrEmpty(_currentCommitReasonSummaryText) ? "None" : _currentCommitReasonSummaryText;
    public string CurrentBlockedReasonSummaryText => string.IsNullOrEmpty(_currentBlockedReasonSummaryText) ? "None" : _currentBlockedReasonSummaryText;
    public string CurrentAlternateMoveSummaryText => string.IsNullOrEmpty(_currentAlternateMoveSummaryText) ? "None" : _currentAlternateMoveSummaryText;

    private void ResetWorldOpportunityLadderState()
    {
        _currentOpportunityLadderSurface = PrototypeWorldSimOpportunityLadderSurfaceData.Empty;
        _currentOpportunityLadderSummaryText = "None";
        _currentConsequencePreviewSummaryText = "None";
        _currentCommitReasonSummaryText = "None";
        _currentBlockedReasonSummaryText = "None";
        _currentAlternateMoveSummaryText = "None";
    }

    private void RefreshWorldOpportunityLadderSurface(PrototypeWorldSimOperationChainSurfaceData chain)
    {
        ResetWorldOpportunityLadderState();

        if (_worldData == null || _worldData.Entities == null)
        {
            return;
        }

        List<PrototypeWorldSimOpportunityCandidateData> candidates = BuildWorldOpportunityCandidates();
        PrototypeWorldSimOpportunityCandidateData systemCandidate = BuildWorldAdvanceCandidate();
        if (systemCandidate != PrototypeWorldSimOpportunityCandidateData.Empty)
        {
            candidates.Add(systemCandidate);
        }

        if (candidates.Count <= 0)
        {
            return;
        }

        candidates.Sort(CompareOpportunityCandidates);
        PrototypeWorldSimOpportunityCandidateData contextCandidate = ResolveContextOpportunityCandidate(chain, candidates);
        PrototypeWorldSimOpportunityCandidateData primaryCandidate = contextCandidate != PrototypeWorldSimOpportunityCandidateData.Empty
            ? contextCandidate
            : candidates[0];
        PrototypeWorldSimOpportunityCandidateData secondaryCandidate = ResolveDistinctOpportunityCandidate(candidates, primaryCandidate);
        PrototypeWorldSimOpportunityCandidateData tertiaryCandidate = ResolveDistinctOpportunityCandidate(candidates, primaryCandidate, secondaryCandidate);

        string ladderSummaryText = BuildOpportunityLadderSummaryText(chain, primaryCandidate, secondaryCandidate, tertiaryCandidate);
        string consequencePreviewText = BuildSelectedOperationConsequencePreview(primaryCandidate);
        string commitReasonSummaryText = ResolveCommitReasonSummary(primaryCandidate, secondaryCandidate);
        string blockedReasonSummaryText = ResolveBlockedReasonSummary(primaryCandidate, secondaryCandidate);
        string alternateMoveSummaryText = BuildAlternateMoveSummary(primaryCandidate, secondaryCandidate, tertiaryCandidate);
        string nextMoveSummaryText = BuildWorldNextMoveSummary(primaryCandidate, secondaryCandidate);

        _currentOpportunityLadderSurface = new PrototypeWorldSimOpportunityLadderSurfaceData(
            chain != null ? chain.SelectedEntityId : string.Empty,
            chain != null ? chain.SelectedEntityLabel : "None",
            chain != null ? chain.SelectedEntityKind : "None",
            primaryCandidate,
            secondaryCandidate,
            tertiaryCandidate,
            ladderSummaryText,
            consequencePreviewText,
            commitReasonSummaryText,
            blockedReasonSummaryText,
            alternateMoveSummaryText,
            nextMoveSummaryText);
        _currentOpportunityLadderSummaryText = ladderSummaryText;
        _currentConsequencePreviewSummaryText = consequencePreviewText;
        _currentCommitReasonSummaryText = commitReasonSummaryText;
        _currentBlockedReasonSummaryText = blockedReasonSummaryText;
        _currentAlternateMoveSummaryText = alternateMoveSummaryText;
    }
    private List<PrototypeWorldSimOpportunityCandidateData> BuildWorldOpportunityCandidates()
    {
        List<PrototypeWorldSimOpportunityCandidateData> candidates = new List<PrototypeWorldSimOpportunityCandidateData>();
        for (int i = 0; i < _worldData.Entities.Length; i++)
        {
            WorldEntityData entity = _worldData.Entities[i];
            if (entity == null || entity.Kind != WorldEntityKind.City)
            {
                continue;
            }

            candidates.Add(BuildOpportunityCandidateForCity(entity));
        }

        return candidates;
    }

    private PrototypeWorldSimOpportunityCandidateData BuildOpportunityCandidateForCity(WorldEntityData city)
    {
        if (city == null || city.Kind != WorldEntityKind.City)
        {
            return PrototypeWorldSimOpportunityCandidateData.Empty;
        }

        string cityId = city.Id ?? string.Empty;
        string dungeonId = GetLinkedDungeonIdForCity(cityId);
        WorldEntityData dungeon = FindEntity(dungeonId);
        bool hasLinkedDungeon = dungeon != null;
        DispatchReadinessState readinessState = GetDispatchReadinessState(cityId);
        string readinessText = BuildDispatchReadinessText(readinessState);
        int recoveryDays = GetRecoveryDaysToReady(cityId);
        int idlePartyCount = _runtimeEconomyState != null ? _runtimeEconomyState.GetIdlePartyCountInCity(cityId) : 0;
        int activeExpeditionCount = _runtimeEconomyState != null ? _runtimeEconomyState.GetActiveExpeditionCountFromCity(cityId) : 0;
        bool canRecruit = ResolveCanRecruitSelectedCityParty(cityId);
        bool canEnter = ResolveCanEnterSelectedCityDungeon(cityId);
        string needPressure = BuildNeedPressureText(cityId);
        string routeId = hasLinkedDungeon ? GetRecommendedRouteId(cityId, dungeonId) : string.Empty;
        DungeonRouteTemplate routeTemplate = hasLinkedDungeon ? GetRouteTemplateById(dungeonId, routeId) : null;
        string routeLabel = routeTemplate == null ? "None" : routeTemplate.RouteLabel + " | " + routeTemplate.RiskLabel + " Risk";
        string riskStateKey = string.Equals(routeId, RiskyRouteId, StringComparison.OrdinalIgnoreCase)
            ? "risky"
            : string.Equals(routeId, SafeRouteId, StringComparison.OrdinalIgnoreCase)
                ? "safe"
                : "balanced";

        string actionStateKey;
        string actionLabel;
        bool isBlocked;
        if (!hasLinkedDungeon)
        {
            actionStateKey = "blocked_no_linked_dungeon";
            actionLabel = "Link Required";
            isBlocked = true;
        }
        else if (canEnter)
        {
            actionStateKey = "dispatch_now";
            actionLabel = "Dispatch Now";
            isBlocked = false;
        }
        else if (canRecruit)
        {
            actionStateKey = "recruit_now";
            actionLabel = "Recruit Party";
            isBlocked = false;
        }
        else if (activeExpeditionCount > 0)
        {
            actionStateKey = "wait_active_return";
            actionLabel = "Wait Return";
            isBlocked = true;
        }
        else if (readinessState == DispatchReadinessState.Recovering)
        {
            actionStateKey = "recover_then_dispatch";
            actionLabel = "Recover First";
            isBlocked = true;
        }
        else if (readinessState == DispatchReadinessState.Strained)
        {
            actionStateKey = "blocked_strained";
            actionLabel = "Hold the Lane";
            isBlocked = true;
        }
        else if (idlePartyCount > 0)
        {
            actionStateKey = "review_route";
            actionLabel = "Review Route";
            isBlocked = false;
        }
        else
        {
            actionStateKey = "advance_world";
            actionLabel = "Run Day";
            isBlocked = false;
        }

        string candidateLabel = BuildCandidateLabel(city.DisplayName, dungeon != null ? dungeon.DisplayName : "None", routeTemplate != null ? routeTemplate.RouteLabel : "None", actionStateKey);
        string reasonText = BuildCandidateReasonText(city.DisplayName, dungeon != null ? dungeon.DisplayName : "None", routeLabel, cityId, dungeonId, actionStateKey, readinessText, recoveryDays, activeExpeditionCount);
        string blockedReasonText = BuildCandidateBlockedReasonText(city.DisplayName, dungeon != null ? dungeon.DisplayName : "None", readinessText, recoveryDays, actionStateKey, activeExpeditionCount);
        string consequenceText = BuildCandidateConsequenceText(city.DisplayName, cityId, dungeonId, routeId, actionStateKey, recoveryDays);
        string comparisonText = BuildCandidateComparisonText(cityId, dungeonId, routeId, actionStateKey, recoveryDays);
        int score = BuildOpportunityCandidateScore(cityId, hasLinkedDungeon, canRecruit, canEnter, readinessState, recoveryDays, activeExpeditionCount, needPressure, isBlocked);

        return new PrototypeWorldSimOpportunityCandidateData(
            "city:" + cityId,
            cityId,
            city.DisplayName,
            dungeon != null ? dungeon.Id : string.Empty,
            dungeon != null ? dungeon.DisplayName : "None",
            routeId,
            routeLabel,
            actionStateKey,
            actionLabel,
            candidateLabel,
            reasonText,
            blockedReasonText,
            consequenceText,
            comparisonText,
            riskStateKey,
            readinessState.ToString(),
            score,
            isBlocked);
    }

    private PrototypeWorldSimOpportunityCandidateData BuildWorldAdvanceCandidate()
    {
        int recoveryDays = ResolveWorldNextRecoveryDays();
        int score = ActiveExpeditions > 0 ? 56 : recoveryDays > 0 ? Mathf.Max(28, 48 - (recoveryDays * 4)) : 18;
        string reasonText = ActiveExpeditions > 0
            ? (ActiveExpeditions == 1 ? "Advance 1 day to move the active expedition toward return." : "Advance 1 day to progress active expeditions and recovery lanes.")
            : recoveryDays > 0
                ? "Advance 1 day to reopen the next recovering city. ETA " + BuildRecoveryEtaText(recoveryDays) + "."
                : "Advance 1 day to refresh pressure, trade flow, and route usage.";

        return new PrototypeWorldSimOpportunityCandidateData(
            "system:advance-day",
            string.Empty,
            "World",
            string.Empty,
            "None",
            string.Empty,
            "None",
            "advance_world",
            "Run Day",
            "Advance 1 day",
            reasonText,
            "None",
            "Advancing the world progresses recovery, route usage, and expedition returns.",
            "Wait alternative: advance 1 day before committing the next run.",
            "none",
            "World",
            score,
            false);
    }

    private int ResolveWorldNextRecoveryDays()
    {
        int best = int.MaxValue;
        foreach (KeyValuePair<string, PrototypeWorldSimCityPressureSurfaceData> pair in _cityPressureByEntityId)
        {
            PrototypeWorldSimCityPressureSurfaceData city = pair.Value;
            if (city == null || city == PrototypeWorldSimCityPressureSurfaceData.Empty || city.RecoveryDays <= 0)
            {
                continue;
            }

            if (city.RecoveryDays < best)
            {
                best = city.RecoveryDays;
            }
        }

        return best == int.MaxValue ? 0 : best;
    }

    private int BuildOpportunityCandidateScore(string cityId, bool hasLinkedDungeon, bool canRecruit, bool canEnter, DispatchReadinessState readinessState, int recoveryDays, int activeExpeditionCount, string needPressure, bool isBlocked)
    {
        int score = 0;
        if (!string.IsNullOrEmpty(cityId) && _cityPressureByEntityId.TryGetValue(cityId, out PrototypeWorldSimCityPressureSurfaceData cityPressure) && cityPressure != null)
        {
            score += cityPressure.OpportunityScore;
            score += cityPressure.Severity * 4;
        }

        if (canEnter)
        {
            score += 120;
        }
        else if (canRecruit)
        {
            score += 88;
        }
        else if (readinessState == DispatchReadinessState.Recovering)
        {
            score += Mathf.Max(18, 42 - (recoveryDays * 4));
        }
        else if (readinessState == DispatchReadinessState.Strained)
        {
            score += 12;
        }
        else
        {
            score += 20;
        }

        if (needPressure == "Urgent")
        {
            score += 26;
        }
        else if (needPressure == "Watch")
        {
            score += 14;
        }

        if (!hasLinkedDungeon)
        {
            score -= 72;
        }

        if (activeExpeditionCount > 0)
        {
            score -= 48;
        }

        if (readinessState == DispatchReadinessState.Strained)
        {
            score -= 28;
        }

        if (isBlocked)
        {
            score -= 8;
        }

        return score;
    }
    private PrototypeWorldSimOpportunityCandidateData ResolveContextOpportunityCandidate(PrototypeWorldSimOperationChainSurfaceData chain, List<PrototypeWorldSimOpportunityCandidateData> candidates)
    {
        string cityId = GetChainCityId(chain);
        if (string.IsNullOrEmpty(cityId) || candidates == null)
        {
            return PrototypeWorldSimOpportunityCandidateData.Empty;
        }

        for (int i = 0; i < candidates.Count; i++)
        {
            PrototypeWorldSimOpportunityCandidateData candidate = candidates[i];
            if (candidate != null && string.Equals(candidate.CityId, cityId, StringComparison.OrdinalIgnoreCase))
            {
                return candidate;
            }
        }

        return PrototypeWorldSimOpportunityCandidateData.Empty;
    }

    private PrototypeWorldSimOpportunityCandidateData ResolveDistinctOpportunityCandidate(List<PrototypeWorldSimOpportunityCandidateData> candidates, params PrototypeWorldSimOpportunityCandidateData[] excluded)
    {
        if (candidates == null)
        {
            return PrototypeWorldSimOpportunityCandidateData.Empty;
        }

        for (int i = 0; i < candidates.Count; i++)
        {
            PrototypeWorldSimOpportunityCandidateData candidate = candidates[i];
            if (candidate == null || candidate == PrototypeWorldSimOpportunityCandidateData.Empty)
            {
                continue;
            }

            bool skip = false;
            if (excluded != null)
            {
                for (int j = 0; j < excluded.Length; j++)
                {
                    PrototypeWorldSimOpportunityCandidateData other = excluded[j];
                    if (other == null || other == PrototypeWorldSimOpportunityCandidateData.Empty)
                    {
                        continue;
                    }

                    if (string.Equals(candidate.CandidateId, other.CandidateId, StringComparison.OrdinalIgnoreCase))
                    {
                        skip = true;
                        break;
                    }
                }
            }

            if (!skip)
            {
                return candidate;
            }
        }

        return PrototypeWorldSimOpportunityCandidateData.Empty;
    }

    private int CompareOpportunityCandidates(PrototypeWorldSimOpportunityCandidateData left, PrototypeWorldSimOpportunityCandidateData right)
    {
        if (ReferenceEquals(left, right))
        {
            return 0;
        }

        if (left == null || left == PrototypeWorldSimOpportunityCandidateData.Empty)
        {
            return 1;
        }

        if (right == null || right == PrototypeWorldSimOpportunityCandidateData.Empty)
        {
            return -1;
        }

        int scoreCompare = right.Score.CompareTo(left.Score);
        if (scoreCompare != 0)
        {
            return scoreCompare;
        }

        if (left.IsBlocked != right.IsBlocked)
        {
            return left.IsBlocked ? 1 : -1;
        }

        return string.Compare(left.CandidateLabel, right.CandidateLabel, StringComparison.OrdinalIgnoreCase);
    }

    private string BuildOpportunityLadderSummaryText(PrototypeWorldSimOperationChainSurfaceData chain, PrototypeWorldSimOpportunityCandidateData primaryCandidate, PrototypeWorldSimOpportunityCandidateData secondaryCandidate, PrototypeWorldSimOpportunityCandidateData tertiaryCandidate)
    {
        if (primaryCandidate == null || primaryCandidate == PrototypeWorldSimOpportunityCandidateData.Empty)
        {
            return "None";
        }

        bool hasSelection = chain != null && chain.HasSelection;
        List<string> lines = new List<string>(3)
        {
            (hasSelection ? "Current" : "1") + ": " + primaryCandidate.CandidateLabel + " | " + CompactSurfaceText(primaryCandidate.ReasonText, hasSelection ? 86 : 78)
        };

        if (secondaryCandidate != null && secondaryCandidate != PrototypeWorldSimOpportunityCandidateData.Empty)
        {
            lines.Add((hasSelection ? "Next" : "2") + ": " + secondaryCandidate.CandidateLabel + " | " + CompactSurfaceText(secondaryCandidate.ReasonText, 74));
        }

        if (tertiaryCandidate != null && tertiaryCandidate != PrototypeWorldSimOpportunityCandidateData.Empty)
        {
            lines.Add((hasSelection ? "Hold" : "3") + ": " + tertiaryCandidate.CandidateLabel + " | " + CompactSurfaceText(tertiaryCandidate.ReasonText, 66));
        }

        return string.Join("\n", lines.ToArray());
    }

    private string BuildSelectedOperationConsequencePreview(PrototypeWorldSimOpportunityCandidateData primaryCandidate)
    {
        return primaryCandidate == null || primaryCandidate == PrototypeWorldSimOpportunityCandidateData.Empty
            ? "Select a city or dungeon to preview the next commitment."
            : primaryCandidate.ConsequenceText;
    }

    private string ResolveCommitReasonSummary(PrototypeWorldSimOpportunityCandidateData primaryCandidate, PrototypeWorldSimOpportunityCandidateData secondaryCandidate)
    {
        if (primaryCandidate == null || primaryCandidate == PrototypeWorldSimOpportunityCandidateData.Empty)
        {
            return "Select a city to open the next move ladder.";
        }

        if (!primaryCandidate.IsBlocked)
        {
            return primaryCandidate.ActionStateKey == "dispatch_now"
                ? "Commit now. " + CompactSurfaceText(primaryCandidate.ReasonText, 120)
                : primaryCandidate.ActionStateKey == "recruit_now"
                    ? "Commit to recruitment. " + CompactSurfaceText(primaryCandidate.ReasonText, 120)
                    : CompactSurfaceText(primaryCandidate.ReasonText, 120);
        }

        if (secondaryCandidate != null && secondaryCandidate != PrototypeWorldSimOpportunityCandidateData.Empty)
        {
            return "Do not commit here yet. " + CompactSurfaceText(primaryCandidate.BlockedReasonText, 72) + " Next best: " + CompactSurfaceText(secondaryCandidate.CandidateLabel, 40) + ".";
        }

        return "Do not commit here yet. " + CompactSurfaceText(primaryCandidate.BlockedReasonText, 96);
    }

    private string ResolveBlockedReasonSummary(PrototypeWorldSimOpportunityCandidateData primaryCandidate, PrototypeWorldSimOpportunityCandidateData secondaryCandidate)
    {
        if (primaryCandidate != null && primaryCandidate != PrototypeWorldSimOpportunityCandidateData.Empty && primaryCandidate.IsBlocked)
        {
            return primaryCandidate.BlockedReasonText;
        }

        if (HasMeaningfulSurfaceText(_currentNetworkPressureSurface.BlockedText))
        {
            return _currentNetworkPressureSurface.BlockedText;
        }

        if (secondaryCandidate != null && secondaryCandidate != PrototypeWorldSimOpportunityCandidateData.Empty && secondaryCandidate.IsBlocked)
        {
            return secondaryCandidate.BlockedReasonText;
        }

        return "No major block is exposed right now.";
    }

    private string BuildAlternateMoveSummary(PrototypeWorldSimOpportunityCandidateData primaryCandidate, PrototypeWorldSimOpportunityCandidateData secondaryCandidate, PrototypeWorldSimOpportunityCandidateData tertiaryCandidate)
    {
        List<string> parts = new List<string>(2);
        if (primaryCandidate != null && primaryCandidate != PrototypeWorldSimOpportunityCandidateData.Empty && HasMeaningfulSurfaceText(primaryCandidate.ComparisonText))
        {
            parts.Add(primaryCandidate.ComparisonText);
        }

        PrototypeWorldSimOpportunityCandidateData alternateCandidate = secondaryCandidate != null && secondaryCandidate != PrototypeWorldSimOpportunityCandidateData.Empty
            ? secondaryCandidate
            : tertiaryCandidate;
        if (alternateCandidate != null && alternateCandidate != PrototypeWorldSimOpportunityCandidateData.Empty)
        {
            parts.Add("Alt Move: " + alternateCandidate.CandidateLabel + " | " + CompactSurfaceText(alternateCandidate.ReasonText, 72));
        }

        return parts.Count > 0 ? string.Join("\n", parts.ToArray()) : "None";
    }

    private string BuildWorldNextMoveSummary(PrototypeWorldSimOpportunityCandidateData primaryCandidate, PrototypeWorldSimOpportunityCandidateData secondaryCandidate)
    {
        if (primaryCandidate == null || primaryCandidate == PrototypeWorldSimOpportunityCandidateData.Empty)
        {
            return "None";
        }

        string text = "Next: " + primaryCandidate.CandidateLabel;
        if (secondaryCandidate != null && secondaryCandidate != PrototypeWorldSimOpportunityCandidateData.Empty)
        {
            text += " | Alt: " + secondaryCandidate.CandidateLabel;
        }

        return text;
    }

    private string BuildCandidateLabel(string cityLabel, string dungeonLabel, string routeLabel, string actionStateKey)
    {
        if (actionStateKey == "dispatch_now")
        {
            return cityLabel + " -> " + dungeonLabel + " | " + routeLabel;
        }

        if (actionStateKey == "recruit_now")
        {
            return "Recruit in " + cityLabel;
        }

        if (actionStateKey == "wait_active_return")
        {
            return "Wait for " + cityLabel + " return";
        }

        if (actionStateKey == "recover_then_dispatch" || actionStateKey == "blocked_strained")
        {
            return "Recover " + cityLabel;
        }

        if (actionStateKey == "blocked_no_linked_dungeon")
        {
            return cityLabel + " link missing";
        }

        if (actionStateKey == "advance_world")
        {
            return "Advance 1 day";
        }

        return cityLabel + " | " + routeLabel;
    }

    private string BuildCandidateReasonText(string cityLabel, string dungeonLabel, string routeLabel, string cityId, string dungeonId, string actionStateKey, string readinessText, int recoveryDays, int activeExpeditionCount)
    {
        if (actionStateKey == "dispatch_now")
        {
            return cityLabel + " can dispatch now via " + routeLabel + ". " + BuildRecommendationReasonText(cityId, dungeonId);
        }

        if (actionStateKey == "recruit_now")
        {
            return "Recruit 1 party in " + cityLabel + ". " + dungeonLabel + " is the next open lane via " + routeLabel + ".";
        }

        if (actionStateKey == "wait_active_return")
        {
            return cityLabel + " already has " + activeExpeditionCount + " expedition" + (activeExpeditionCount == 1 ? string.Empty : "s") + " in motion. Hold until the return resolves.";
        }

        if (actionStateKey == "recover_then_dispatch")
        {
            return cityLabel + " is recovering. ETA " + BuildRecoveryEtaText(recoveryDays) + " before " + routeLabel + " opens again.";
        }
        if (actionStateKey == "blocked_strained")
        {
            return cityLabel + " is strained. Let readiness recover before committing another route.";
        }

        if (actionStateKey == "blocked_no_linked_dungeon")
        {
            return cityLabel + " has no linked dungeon lane yet.";
        }

        if (actionStateKey == "advance_world")
        {
            return "Run a day to refresh readiness and reopen the next lane. Current readiness: " + CompactSurfaceText(readinessText, 48) + ".";
        }

        return cityLabel + " can review " + routeLabel + " for " + dungeonLabel + ".";
    }

    private string BuildCandidateBlockedReasonText(string cityLabel, string dungeonLabel, string readinessText, int recoveryDays, string actionStateKey, int activeExpeditionCount)
    {
        if (actionStateKey == "blocked_no_linked_dungeon")
        {
            return cityLabel + " cannot dispatch because no linked dungeon lane exists.";
        }

        if (actionStateKey == "wait_active_return")
        {
            return cityLabel + " is blocked by " + activeExpeditionCount + " active expedition" + (activeExpeditionCount == 1 ? string.Empty : "s") + ".";
        }

        if (actionStateKey == "recover_then_dispatch")
        {
            return cityLabel + " is recovering. Readiness " + readinessText + " | ETA " + BuildRecoveryEtaText(recoveryDays) + ".";
        }

        if (actionStateKey == "blocked_strained")
        {
            return cityLabel + " is strained. Readiness must recover before dispatch.";
        }

        if (actionStateKey == "dispatch_now")
        {
            return cityLabel + " is clear to dispatch into " + dungeonLabel + ".";
        }

        if (actionStateKey == "recruit_now")
        {
            return cityLabel + " is ready to recruit, but no idle party is staged yet.";
        }

        return "No hard block is visible on this lane.";
    }

    private string BuildCandidateConsequenceText(string cityLabel, string cityId, string dungeonId, string routeId, string actionStateKey, int recoveryDays)
    {
        if (string.IsNullOrEmpty(dungeonId) || string.IsNullOrEmpty(routeId))
        {
            return actionStateKey == "blocked_no_linked_dungeon"
                ? "No consequence preview until a linked dungeon lane is assigned."
                : "No route consequence preview is visible yet.";
        }

        string primaryPreview = CompactSurfaceText(BuildRoutePreviewSummaryText(dungeonId, routeId), 98);
        string primaryImpact = CompactSurfaceText(BuildExpectedNeedImpactText(cityId, dungeonId, routeId), 52);
        string alternateRouteId = string.Equals(routeId, SafeRouteId, StringComparison.OrdinalIgnoreCase) ? RiskyRouteId : SafeRouteId;
        string alternatePreview = CompactSurfaceText(BuildRoutePreviewSummaryText(dungeonId, alternateRouteId), 94);
        string alternateImpact = CompactSurfaceText(BuildExpectedNeedImpactText(cityId, dungeonId, alternateRouteId), 48);

        if (actionStateKey == "recruit_now")
        {
            return "After recruit: " + primaryPreview + " | Impact " + primaryImpact + "\nAlt Route: " + alternatePreview + " | Impact " + alternateImpact;
        }

        if (actionStateKey == "recover_then_dispatch" || actionStateKey == "blocked_strained")
        {
            return "After " + BuildRecoveryEtaText(recoveryDays) + ": " + primaryPreview + " | Impact " + primaryImpact + "\nAlt Route: " + alternatePreview + " | Impact " + alternateImpact;
        }

        if (actionStateKey == "wait_active_return")
        {
            return "After the return resolves: " + primaryPreview + " | Impact " + primaryImpact + "\nAlt Route: " + alternatePreview + " | Impact " + alternateImpact;
        }

        return "Commit: " + primaryPreview + " | Impact " + primaryImpact + "\nAlt Route: " + alternatePreview + " | Impact " + alternateImpact;
    }

    private string BuildCandidateComparisonText(string cityId, string dungeonId, string primaryRouteId, string actionStateKey, int recoveryDays)
    {
        if (string.IsNullOrEmpty(dungeonId) || string.IsNullOrEmpty(primaryRouteId))
        {
            return actionStateKey == "advance_world"
                ? "Wait option: advance 1 day before committing the next run."
                : "No alternate route preview.";
        }

        string alternateRouteId = string.Equals(primaryRouteId, SafeRouteId, StringComparison.OrdinalIgnoreCase) ? RiskyRouteId : SafeRouteId;
        DungeonRouteTemplate alternateTemplate = GetRouteTemplateById(dungeonId, alternateRouteId);
        string alternateLabel = alternateTemplate == null ? alternateRouteId : alternateTemplate.RouteLabel;
        string alternateImpact = CompactSurfaceText(BuildExpectedNeedImpactText(cityId, dungeonId, alternateRouteId), 42);
        if (actionStateKey == "recover_then_dispatch" || actionStateKey == "blocked_strained")
        {
            return "Wait " + BuildRecoveryEtaText(recoveryDays) + ", then compare against " + alternateLabel + " | Impact " + alternateImpact + ".";
        }

        return "Alt Route: " + alternateLabel + " | Impact " + alternateImpact + ".";
    }

    private void ApplyWorldOpportunityLadderPulseState()
    {
        ApplyPrimaryOpportunityCandidatePulse(_currentOpportunityLadderSurface.PrimaryCandidate);
        ApplySecondaryOpportunityCandidatePulse(_currentOpportunityLadderSurface.PrimaryCandidate, _currentOpportunityLadderSurface.SecondaryCandidate);
    }

    private void ApplyPrimaryOpportunityCandidatePulse(PrototypeWorldSimOpportunityCandidateData candidate)
    {
        if (candidate == null || candidate == PrototypeWorldSimOpportunityCandidateData.Empty)
        {
            return;
        }

        if (!string.IsNullOrEmpty(candidate.CityId) && _markerByEntityId.TryGetValue(candidate.CityId, out WorldSelectableMarker cityMarker) && cityMarker != null)
        {
            cityMarker.SetActionReady(candidate.ActionStateKey == "dispatch_now" || candidate.ActionStateKey == "recruit_now");
            if (candidate.IsBlocked && _cityPressureByEntityId.TryGetValue(candidate.CityId, out PrototypeWorldSimCityPressureSurfaceData cityPressure) && cityPressure != null)
            {
                cityMarker.SetPressureState(cityPressure.StateKey, true);
            }
        }

        if (!string.IsNullOrEmpty(candidate.DungeonId) && _markerByEntityId.TryGetValue(candidate.DungeonId, out WorldSelectableMarker dungeonMarker) && dungeonMarker != null)
        {
            dungeonMarker.SetLinkedFocus(true);
        }

        if (!string.IsNullOrEmpty(candidate.RouteId) && _routePulseByRouteId.TryGetValue(candidate.RouteId, out WorldBoardRoutePressureVisual routeVisual) && routeVisual != null)
        {
            string stateKey = _routePressureByRouteId.TryGetValue(candidate.RouteId, out PrototypeWorldSimRoutePressureSurfaceData routePressure) && routePressure != null
                ? routePressure.StateKey
                : "open";
            routeVisual.ApplyState(stateKey, true, !candidate.IsBlocked);
        }

        string connectorKey = BuildLinkedConnectorKey(candidate.CityId, candidate.DungeonId);
        if (!string.IsNullOrEmpty(connectorKey) && _linkedConnectorByKey.TryGetValue(connectorKey, out WorldBoardConnectorPulseVisual connectorVisual) && connectorVisual != null)
        {
            connectorVisual.ApplyPulse(true, false);
        }
    }

    private void ApplySecondaryOpportunityCandidatePulse(PrototypeWorldSimOpportunityCandidateData primaryCandidate, PrototypeWorldSimOpportunityCandidateData candidate)
    {
        if (candidate == null || candidate == PrototypeWorldSimOpportunityCandidateData.Empty)
        {
            return;
        }

        if (primaryCandidate != null && primaryCandidate != PrototypeWorldSimOpportunityCandidateData.Empty && string.Equals(candidate.CandidateId, primaryCandidate.CandidateId, StringComparison.OrdinalIgnoreCase))
        {
            return;
        }

        if (!string.IsNullOrEmpty(candidate.CityId) && _markerByEntityId.TryGetValue(candidate.CityId, out WorldSelectableMarker cityMarker) && cityMarker != null)
        {
            string pressureStateKey = _cityPressureByEntityId.TryGetValue(candidate.CityId, out PrototypeWorldSimCityPressureSurfaceData cityPressure) && cityPressure != null
                ? cityPressure.StateKey
                : string.Empty;
            cityMarker.SetPressureState(pressureStateKey, true);
        }

        if (!string.IsNullOrEmpty(candidate.RouteId) && _routePulseByRouteId.TryGetValue(candidate.RouteId, out WorldBoardRoutePressureVisual routeVisual) && routeVisual != null)
        {
            string stateKey = _routePressureByRouteId.TryGetValue(candidate.RouteId, out PrototypeWorldSimRoutePressureSurfaceData routePressure) && routePressure != null
                ? routePressure.StateKey
                : "open";
            routeVisual.ApplyState(stateKey, false, true);
        }
    }
}

