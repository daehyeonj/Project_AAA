using System;
using System.Collections.Generic;
using UnityEngine;

public sealed partial class StaticPlaceholderWorldView
{
    private sealed class PrototypeWorldSimQueuedOperationData
    {
        public static readonly PrototypeWorldSimQueuedOperationData Empty = new PrototypeWorldSimQueuedOperationData(
            "none",
            string.Empty,
            string.Empty,
            "None",
            string.Empty,
            "None",
            string.Empty,
            "None",
            "None",
            "None",
            "None",
            "None",
            "none",
            "none",
            "None",
            0,
            true);

        public readonly string QueueSlotKey;
        public readonly string CandidateId;
        public readonly string CityId;
        public readonly string CityLabel;
        public readonly string DungeonId;
        public readonly string DungeonLabel;
        public readonly string RouteId;
        public readonly string RouteLabel;
        public readonly string OperationLabel;
        public readonly string ReasonText;
        public readonly string WaitReasonText;
        public readonly string SummaryText;
        public readonly string PriorityBandKey;
        public readonly string RiskStateKey;
        public readonly string ReadinessStateKey;
        public readonly int RecoveryDays;
        public readonly bool IsBlocked;

        public bool HasCity => !string.IsNullOrEmpty(CityId);
        public bool HasDungeon => !string.IsNullOrEmpty(DungeonId);
        public bool HasRoute => !string.IsNullOrEmpty(RouteId);

        public PrototypeWorldSimQueuedOperationData(
            string queueSlotKey,
            string candidateId,
            string cityId,
            string cityLabel,
            string dungeonId,
            string dungeonLabel,
            string routeId,
            string routeLabel,
            string operationLabel,
            string reasonText,
            string waitReasonText,
            string summaryText,
            string priorityBandKey,
            string riskStateKey,
            string readinessStateKey,
            int recoveryDays,
            bool isBlocked)
        {
            QueueSlotKey = string.IsNullOrEmpty(queueSlotKey) ? "none" : queueSlotKey;
            CandidateId = string.IsNullOrEmpty(candidateId) ? string.Empty : candidateId;
            CityId = string.IsNullOrEmpty(cityId) ? string.Empty : cityId;
            CityLabel = string.IsNullOrEmpty(cityLabel) ? "None" : cityLabel;
            DungeonId = string.IsNullOrEmpty(dungeonId) ? string.Empty : dungeonId;
            DungeonLabel = string.IsNullOrEmpty(dungeonLabel) ? "None" : dungeonLabel;
            RouteId = string.IsNullOrEmpty(routeId) ? string.Empty : routeId;
            RouteLabel = string.IsNullOrEmpty(routeLabel) ? "None" : routeLabel;
            OperationLabel = string.IsNullOrEmpty(operationLabel) ? "None" : operationLabel;
            ReasonText = string.IsNullOrEmpty(reasonText) ? "None" : reasonText;
            WaitReasonText = string.IsNullOrEmpty(waitReasonText) ? "None" : waitReasonText;
            SummaryText = string.IsNullOrEmpty(summaryText) ? "None" : summaryText;
            PriorityBandKey = string.IsNullOrEmpty(priorityBandKey) ? "none" : priorityBandKey;
            RiskStateKey = string.IsNullOrEmpty(riskStateKey) ? "none" : riskStateKey;
            ReadinessStateKey = string.IsNullOrEmpty(readinessStateKey) ? "None" : readinessStateKey;
            RecoveryDays = Mathf.Max(0, recoveryDays);
            IsBlocked = isBlocked;
        }
    }

    private sealed class PrototypeWorldSimCommitmentQueueSurfaceData
    {
        public static readonly PrototypeWorldSimCommitmentQueueSurfaceData Empty = new PrototypeWorldSimCommitmentQueueSurfaceData(
            string.Empty,
            "None",
            PrototypeWorldSimQueuedOperationData.Empty,
            PrototypeWorldSimQueuedOperationData.Empty,
            PrototypeWorldSimQueuedOperationData.Empty,
            "None",
            "None",
            "None",
            "None",
            "None",
            "None",
            "None",
            "None");

        public readonly string SelectedEntityId;
        public readonly string SelectedEntityLabel;
        public readonly PrototypeWorldSimQueuedOperationData ImmediateOperation;
        public readonly PrototypeWorldSimQueuedOperationData SoonOperation;
        public readonly PrototypeWorldSimQueuedOperationData DeferredOperation;
        public readonly string QueueSummaryText;
        public readonly string DeferredSummaryText;
        public readonly string RecoveryWindowSummaryText;
        public readonly string PriorityClockSummaryText;
        public readonly string ShortHorizonSummaryText;
        public readonly string BlockedHotspotLabel;
        public readonly string BlockedHotspotReasonText;
        public readonly string WorldPostureSummaryText;

        public PrototypeWorldSimCommitmentQueueSurfaceData(
            string selectedEntityId,
            string selectedEntityLabel,
            PrototypeWorldSimQueuedOperationData immediateOperation,
            PrototypeWorldSimQueuedOperationData soonOperation,
            PrototypeWorldSimQueuedOperationData deferredOperation,
            string queueSummaryText,
            string deferredSummaryText,
            string recoveryWindowSummaryText,
            string priorityClockSummaryText,
            string shortHorizonSummaryText,
            string blockedHotspotLabel,
            string blockedHotspotReasonText,
            string worldPostureSummaryText)
        {
            SelectedEntityId = string.IsNullOrEmpty(selectedEntityId) ? string.Empty : selectedEntityId;
            SelectedEntityLabel = string.IsNullOrEmpty(selectedEntityLabel) ? "None" : selectedEntityLabel;
            ImmediateOperation = immediateOperation ?? PrototypeWorldSimQueuedOperationData.Empty;
            SoonOperation = soonOperation ?? PrototypeWorldSimQueuedOperationData.Empty;
            DeferredOperation = deferredOperation ?? PrototypeWorldSimQueuedOperationData.Empty;
            QueueSummaryText = string.IsNullOrEmpty(queueSummaryText) ? "None" : queueSummaryText;
            DeferredSummaryText = string.IsNullOrEmpty(deferredSummaryText) ? "None" : deferredSummaryText;
            RecoveryWindowSummaryText = string.IsNullOrEmpty(recoveryWindowSummaryText) ? "None" : recoveryWindowSummaryText;
            PriorityClockSummaryText = string.IsNullOrEmpty(priorityClockSummaryText) ? "None" : priorityClockSummaryText;
            ShortHorizonSummaryText = string.IsNullOrEmpty(shortHorizonSummaryText) ? "None" : shortHorizonSummaryText;
            BlockedHotspotLabel = string.IsNullOrEmpty(blockedHotspotLabel) ? "None" : blockedHotspotLabel;
            BlockedHotspotReasonText = string.IsNullOrEmpty(blockedHotspotReasonText) ? "None" : blockedHotspotReasonText;
            WorldPostureSummaryText = string.IsNullOrEmpty(worldPostureSummaryText) ? "None" : worldPostureSummaryText;
        }
    }

    private PrototypeWorldSimCommitmentQueueSurfaceData _currentCommitmentQueueSurface = PrototypeWorldSimCommitmentQueueSurfaceData.Empty;
    private string _currentCommitmentQueueSummaryText = "None";
    private string _currentDeferredOpportunitySummaryText = "None";
    private string _currentRecoveryWindowSummaryText = "None";
    private string _currentPriorityClockSummaryText = "None";
    private string _currentShortHorizonWorldSummaryText = "None";
    private string _currentWorldPostureSummaryText = "None";

    public string CurrentCommitmentQueueSummaryText => string.IsNullOrEmpty(_currentCommitmentQueueSummaryText) ? "None" : _currentCommitmentQueueSummaryText;
    public string CurrentDeferredOpportunitySummaryText => string.IsNullOrEmpty(_currentDeferredOpportunitySummaryText) ? "None" : _currentDeferredOpportunitySummaryText;
    public string CurrentRecoveryWindowSummaryText => string.IsNullOrEmpty(_currentRecoveryWindowSummaryText) ? "None" : _currentRecoveryWindowSummaryText;
    public string CurrentPriorityClockSummaryText => string.IsNullOrEmpty(_currentPriorityClockSummaryText) ? "None" : _currentPriorityClockSummaryText;
    public string CurrentShortHorizonWorldSummaryText => string.IsNullOrEmpty(_currentShortHorizonWorldSummaryText) ? "None" : _currentShortHorizonWorldSummaryText;
    public string CurrentWorldPostureSummaryText => string.IsNullOrEmpty(_currentWorldPostureSummaryText) ? "None" : _currentWorldPostureSummaryText;

    private void ResetWorldCommitmentQueueState()
    {
        _currentCommitmentQueueSurface = PrototypeWorldSimCommitmentQueueSurfaceData.Empty;
        _currentCommitmentQueueSummaryText = "None";
        _currentDeferredOpportunitySummaryText = "None";
        _currentRecoveryWindowSummaryText = "None";
        _currentPriorityClockSummaryText = "None";
        _currentShortHorizonWorldSummaryText = "None";
        _currentWorldPostureSummaryText = "None";
    }

    private void RefreshWorldCommitmentQueueSurface(PrototypeWorldSimOperationChainSurfaceData chain)
    {
        ResetWorldCommitmentQueueState();

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
        PrototypeWorldSimOpportunityCandidateData immediateCandidate = ResolveImmediateCommitmentCandidate(chain, candidates, contextCandidate);
        PrototypeWorldSimOpportunityCandidateData soonCandidate = ResolveSoonCommitmentCandidate(candidates, contextCandidate, immediateCandidate);
        PrototypeWorldSimOpportunityCandidateData deferredCandidate = ResolveDeferredCommitmentCandidate(candidates, contextCandidate, immediateCandidate, soonCandidate);
        PrototypeWorldSimOpportunityCandidateData blockedHotspotCandidate = ResolveBlockedHotspotCandidate(candidates, contextCandidate, immediateCandidate, soonCandidate, deferredCandidate);

        PrototypeWorldSimQueuedOperationData immediateOperation = BuildQueuedOperationData("immediate", immediateCandidate, immediateCandidate, soonCandidate, blockedHotspotCandidate);
        PrototypeWorldSimQueuedOperationData soonOperation = BuildQueuedOperationData("soon", soonCandidate, immediateCandidate, soonCandidate, blockedHotspotCandidate);
        PrototypeWorldSimQueuedOperationData deferredOperation = BuildQueuedOperationData("deferred", deferredCandidate, immediateCandidate, soonCandidate, blockedHotspotCandidate);

        string queueSummaryText = BuildCommitmentQueueSummaryText(chain, immediateOperation, soonOperation, deferredOperation);
        string recoveryWindowSummaryText = BuildRecoveryWindowLane(chain, soonOperation, blockedHotspotCandidate);
        string deferredSummaryText = BuildDeferredOpportunitySummary(deferredOperation, blockedHotspotCandidate, immediateOperation);
        string priorityClockSummaryText = BuildPriorityClockSummary(immediateOperation, soonOperation, deferredOperation, blockedHotspotCandidate);
        string blockedHotspotLabel = blockedHotspotCandidate != null && blockedHotspotCandidate != PrototypeWorldSimOpportunityCandidateData.Empty
            ? blockedHotspotCandidate.CandidateLabel
            : ResolveUrgentPressureNodeLabel();
        string blockedHotspotReasonText = blockedHotspotCandidate != null && blockedHotspotCandidate != PrototypeWorldSimOpportunityCandidateData.Empty
            ? blockedHotspotCandidate.BlockedReasonText
            : HasMeaningfulSurfaceText(_currentNetworkPressureSurface.BlockedText)
                ? _currentNetworkPressureSurface.BlockedText
                : "No major block is visible right now.";
        string shortHorizonSummaryText = BuildShortHorizonWorldSummary(immediateOperation, soonOperation, deferredOperation, blockedHotspotLabel, blockedHotspotReasonText);
        string worldPostureSummaryText = BuildWorldPostureSummary(immediateOperation, soonOperation, blockedHotspotLabel);

        _currentCommitmentQueueSurface = new PrototypeWorldSimCommitmentQueueSurfaceData(
            chain != null ? chain.SelectedEntityId : string.Empty,
            chain != null ? chain.SelectedEntityLabel : "None",
            immediateOperation,
            soonOperation,
            deferredOperation,
            queueSummaryText,
            deferredSummaryText,
            recoveryWindowSummaryText,
            priorityClockSummaryText,
            shortHorizonSummaryText,
            blockedHotspotLabel,
            blockedHotspotReasonText,
            worldPostureSummaryText);
        _currentCommitmentQueueSummaryText = queueSummaryText;
        _currentDeferredOpportunitySummaryText = deferredSummaryText;
        _currentRecoveryWindowSummaryText = recoveryWindowSummaryText;
        _currentPriorityClockSummaryText = priorityClockSummaryText;
        _currentShortHorizonWorldSummaryText = shortHorizonSummaryText;
        _currentWorldPostureSummaryText = worldPostureSummaryText;
    }

    private PrototypeWorldSimOpportunityCandidateData ResolveImmediateCommitmentCandidate(PrototypeWorldSimOperationChainSurfaceData chain, List<PrototypeWorldSimOpportunityCandidateData> candidates, PrototypeWorldSimOpportunityCandidateData contextCandidate)
    {
        if (chain != null && chain.HasSelection && IsMeaningfulOpportunityCandidate(contextCandidate) && !contextCandidate.IsBlocked)
        {
            return contextCandidate;
        }

        for (int i = 0; i < candidates.Count; i++)
        {
            PrototypeWorldSimOpportunityCandidateData candidate = candidates[i];
            if (!IsMeaningfulOpportunityCandidate(candidate) || candidate.IsBlocked || string.Equals(candidate.ActionStateKey, "advance_world", StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            return candidate;
        }

        for (int i = 0; i < candidates.Count; i++)
        {
            PrototypeWorldSimOpportunityCandidateData candidate = candidates[i];
            if (IsMeaningfulOpportunityCandidate(candidate) && !candidate.IsBlocked)
            {
                return candidate;
            }
        }

        if (IsMeaningfulOpportunityCandidate(contextCandidate))
        {
            return contextCandidate;
        }

        return candidates.Count > 0 ? candidates[0] : PrototypeWorldSimOpportunityCandidateData.Empty;
    }

    private PrototypeWorldSimOpportunityCandidateData ResolveSoonCommitmentCandidate(List<PrototypeWorldSimOpportunityCandidateData> candidates, PrototypeWorldSimOpportunityCandidateData contextCandidate, PrototypeWorldSimOpportunityCandidateData immediateCandidate)
    {
        if (IsMeaningfulOpportunityCandidate(contextCandidate) && !MatchesOpportunityCandidate(contextCandidate, immediateCandidate) && IsRecoveryWindowCandidate(contextCandidate))
        {
            return contextCandidate;
        }

        for (int i = 0; i < candidates.Count; i++)
        {
            PrototypeWorldSimOpportunityCandidateData candidate = candidates[i];
            if (!IsMeaningfulOpportunityCandidate(candidate) || MatchesOpportunityCandidate(candidate, immediateCandidate))
            {
                continue;
            }

            if (IsRecoveryWindowCandidate(candidate))
            {
                return candidate;
            }
        }

        for (int i = 0; i < candidates.Count; i++)
        {
            PrototypeWorldSimOpportunityCandidateData candidate = candidates[i];
            if (IsMeaningfulOpportunityCandidate(candidate) && !MatchesOpportunityCandidate(candidate, immediateCandidate) && candidate.IsBlocked)
            {
                return candidate;
            }
        }

        return PrototypeWorldSimOpportunityCandidateData.Empty;
    }
    private PrototypeWorldSimOpportunityCandidateData ResolveDeferredCommitmentCandidate(List<PrototypeWorldSimOpportunityCandidateData> candidates, PrototypeWorldSimOpportunityCandidateData contextCandidate, PrototypeWorldSimOpportunityCandidateData immediateCandidate, PrototypeWorldSimOpportunityCandidateData soonCandidate)
    {
        if (IsMeaningfulOpportunityCandidate(contextCandidate) && !MatchesOpportunityCandidate(contextCandidate, immediateCandidate) && !MatchesOpportunityCandidate(contextCandidate, soonCandidate) && contextCandidate.IsBlocked)
        {
            return contextCandidate;
        }

        for (int i = 0; i < candidates.Count; i++)
        {
            PrototypeWorldSimOpportunityCandidateData candidate = candidates[i];
            if (!IsMeaningfulOpportunityCandidate(candidate) || MatchesOpportunityCandidate(candidate, immediateCandidate) || MatchesOpportunityCandidate(candidate, soonCandidate))
            {
                continue;
            }

            if (IsHardBlockedCandidate(candidate))
            {
                return candidate;
            }
        }

        for (int i = 0; i < candidates.Count; i++)
        {
            PrototypeWorldSimOpportunityCandidateData candidate = candidates[i];
            if (IsMeaningfulOpportunityCandidate(candidate) && !MatchesOpportunityCandidate(candidate, immediateCandidate) && !MatchesOpportunityCandidate(candidate, soonCandidate))
            {
                return candidate;
            }
        }

        return PrototypeWorldSimOpportunityCandidateData.Empty;
    }

    private PrototypeWorldSimOpportunityCandidateData ResolveBlockedHotspotCandidate(List<PrototypeWorldSimOpportunityCandidateData> candidates, PrototypeWorldSimOpportunityCandidateData contextCandidate, PrototypeWorldSimOpportunityCandidateData immediateCandidate, PrototypeWorldSimOpportunityCandidateData soonCandidate, PrototypeWorldSimOpportunityCandidateData deferredCandidate)
    {
        if (IsMeaningfulOpportunityCandidate(contextCandidate) && contextCandidate.IsBlocked)
        {
            return contextCandidate;
        }

        PrototypeWorldSimOpportunityCandidateData bestCandidate = PrototypeWorldSimOpportunityCandidateData.Empty;
        int bestSeverity = int.MinValue;
        int bestScore = int.MinValue;
        for (int i = 0; i < candidates.Count; i++)
        {
            PrototypeWorldSimOpportunityCandidateData candidate = candidates[i];
            if (!IsMeaningfulOpportunityCandidate(candidate) || !candidate.IsBlocked || string.IsNullOrEmpty(candidate.CityId))
            {
                continue;
            }

            int severity = 0;
            if (_cityPressureByEntityId.TryGetValue(candidate.CityId, out PrototypeWorldSimCityPressureSurfaceData cityPressure) && cityPressure != null)
            {
                severity = cityPressure.Severity;
            }

            if (bestCandidate == PrototypeWorldSimOpportunityCandidateData.Empty || severity > bestSeverity || (severity == bestSeverity && candidate.Score > bestScore))
            {
                bestCandidate = candidate;
                bestSeverity = severity;
                bestScore = candidate.Score;
            }
        }

        if (bestCandidate != PrototypeWorldSimOpportunityCandidateData.Empty)
        {
            return bestCandidate;
        }

        if (IsMeaningfulOpportunityCandidate(soonCandidate) && soonCandidate.IsBlocked)
        {
            return soonCandidate;
        }

        if (IsMeaningfulOpportunityCandidate(deferredCandidate) && deferredCandidate.IsBlocked)
        {
            return deferredCandidate;
        }

        return IsMeaningfulOpportunityCandidate(immediateCandidate) && immediateCandidate.IsBlocked
            ? immediateCandidate
            : PrototypeWorldSimOpportunityCandidateData.Empty;
    }

    private PrototypeWorldSimQueuedOperationData BuildQueuedOperationData(string slotKey, PrototypeWorldSimOpportunityCandidateData candidate, PrototypeWorldSimOpportunityCandidateData immediateCandidate, PrototypeWorldSimOpportunityCandidateData soonCandidate, PrototypeWorldSimOpportunityCandidateData blockedHotspotCandidate)
    {
        if (!IsMeaningfulOpportunityCandidate(candidate))
        {
            return PrototypeWorldSimQueuedOperationData.Empty;
        }

        string reasonText = BuildQueuedOperationReasonText(slotKey, candidate, immediateCandidate, soonCandidate, blockedHotspotCandidate);
        string waitReasonText = BuildQueuedOperationWaitReasonText(slotKey, candidate, immediateCandidate, soonCandidate);
        string summaryText = candidate.CandidateLabel + " | " + CompactSurfaceText(slotKey == "immediate" ? reasonText : waitReasonText, 82);
        return new PrototypeWorldSimQueuedOperationData(
            slotKey,
            candidate.CandidateId,
            candidate.CityId,
            candidate.CityLabel,
            candidate.DungeonId,
            candidate.DungeonLabel,
            candidate.RouteId,
            candidate.RouteLabel,
            candidate.CandidateLabel,
            reasonText,
            waitReasonText,
            summaryText,
            ResolvePriorityBandKey(slotKey, candidate),
            candidate.RiskStateKey,
            candidate.ReadinessStateKey,
            ResolveCandidateRecoveryDays(candidate),
            candidate.IsBlocked);
    }

    private string BuildQueuedOperationReasonText(string slotKey, PrototypeWorldSimOpportunityCandidateData candidate, PrototypeWorldSimOpportunityCandidateData immediateCandidate, PrototypeWorldSimOpportunityCandidateData soonCandidate, PrototypeWorldSimOpportunityCandidateData blockedHotspotCandidate)
    {
        if (!IsMeaningfulOpportunityCandidate(candidate))
        {
            return "None";
        }

        if (string.Equals(slotKey, "immediate", StringComparison.OrdinalIgnoreCase))
        {
            return candidate.IsBlocked
                ? "Immediate lane is blocked. " + CompactSurfaceText(candidate.BlockedReasonText, 88)
                : CompactSurfaceText(candidate.ReasonText, 112);
        }

        if (string.Equals(slotKey, "soon", StringComparison.OrdinalIgnoreCase))
        {
            return ResolveWaitThenDispatchReason(candidate);
        }

        string reasonText = candidate.IsBlocked ? candidate.BlockedReasonText : candidate.ReasonText;
        string leaderText = IsMeaningfulOpportunityCandidate(immediateCandidate) && !MatchesOpportunityCandidate(candidate, immediateCandidate)
            ? "Held behind " + CompactSurfaceText(immediateCandidate.CandidateLabel, 42) + ". "
            : string.Empty;
        string reopenText = BuildDeferredRevisitTriggerText(candidate, soonCandidate, blockedHotspotCandidate);
        return leaderText + CompactSurfaceText(reasonText, 78) + (string.IsNullOrEmpty(reopenText) ? string.Empty : " Revisit " + reopenText + ".");
    }

    private string BuildQueuedOperationWaitReasonText(string slotKey, PrototypeWorldSimOpportunityCandidateData candidate, PrototypeWorldSimOpportunityCandidateData immediateCandidate, PrototypeWorldSimOpportunityCandidateData soonCandidate)
    {
        if (!IsMeaningfulOpportunityCandidate(candidate))
        {
            return "None";
        }

        if (string.Equals(slotKey, "soon", StringComparison.OrdinalIgnoreCase))
        {
            return ResolveWaitThenDispatchReason(candidate);
        }

        if (string.Equals(slotKey, "deferred", StringComparison.OrdinalIgnoreCase))
        {
            return BuildDeferredFollowThroughSummary(candidate, immediateCandidate, soonCandidate);
        }

        return candidate.IsBlocked ? candidate.BlockedReasonText : candidate.ReasonText;
    }

    private string BuildRecoveryWindowLane(PrototypeWorldSimOperationChainSurfaceData chain, PrototypeWorldSimQueuedOperationData soonOperation, PrototypeWorldSimOpportunityCandidateData blockedHotspotCandidate)
    {
        if (soonOperation != null && soonOperation != PrototypeWorldSimQueuedOperationData.Empty)
        {
            PrototypeWorldSimOpportunityCandidateData soonCandidate = ResolveOpportunityCandidateById(soonOperation.CandidateId);
            string actionStateKey = IsMeaningfulOpportunityCandidate(soonCandidate)
                ? soonCandidate.ActionStateKey
                : string.Equals(soonOperation.PriorityBandKey, "revisit_after_return", StringComparison.OrdinalIgnoreCase)
                    ? "wait_active_return"
                    : soonOperation.RecoveryDays > 0
                        ? "recover_then_dispatch"
                        : "advance_world";
            string routeDeltaText = !string.IsNullOrEmpty(soonOperation.CityId) && !string.IsNullOrEmpty(soonOperation.DungeonId) && !string.IsNullOrEmpty(soonOperation.RouteId)
                ? CompactSurfaceText(BuildCandidateComparisonText(soonOperation.CityId, soonOperation.DungeonId, soonOperation.RouteId, actionStateKey, soonOperation.RecoveryDays), 68)
                : "No alternate route delta is exposed yet.";
            return soonOperation.OperationLabel + " | " + CompactSurfaceText(soonOperation.WaitReasonText, 72) + " | " + routeDeltaText;
        }

        if (IsMeaningfulOpportunityCandidate(blockedHotspotCandidate) && IsRecoveryWindowCandidate(blockedHotspotCandidate))
        {
            return blockedHotspotCandidate.CandidateLabel + " | " + ResolveWaitThenDispatchReason(blockedHotspotCandidate);
        }

        if (HasMeaningfulSurfaceText(_currentRecoveryForecastSummaryText))
        {
            return _currentRecoveryForecastSummaryText;
        }

        return chain != null && chain.HasSelection
            ? "No short recovery window is visible on this selection."
            : "No near-term recovery window is visible right now.";
    }

    private string BuildDeferredOpportunitySummary(PrototypeWorldSimQueuedOperationData deferredOperation, PrototypeWorldSimOpportunityCandidateData blockedHotspotCandidate, PrototypeWorldSimQueuedOperationData immediateOperation)
    {
        if (deferredOperation != null && deferredOperation != PrototypeWorldSimQueuedOperationData.Empty)
        {
            return deferredOperation.OperationLabel + " | " + CompactSurfaceText(deferredOperation.WaitReasonText, 90);
        }

        if (IsMeaningfulOpportunityCandidate(blockedHotspotCandidate))
        {
            string prefix = immediateOperation != null && immediateOperation != PrototypeWorldSimQueuedOperationData.Empty
                ? "Held behind " + CompactSurfaceText(immediateOperation.OperationLabel, 36) + ". "
                : string.Empty;
            return prefix + CompactSurfaceText(blockedHotspotCandidate.CandidateLabel + " | " + blockedHotspotCandidate.BlockedReasonText, 100);
        }

        return HasMeaningfulSurfaceText(_currentOpportunitySummaryText)
            ? CompactSurfaceText(_currentOpportunitySummaryText, 100)
            : "No deferred opportunity is being tracked.";
    }

    private string BuildPriorityClockSummary(PrototypeWorldSimQueuedOperationData immediateOperation, PrototypeWorldSimQueuedOperationData soonOperation, PrototypeWorldSimQueuedOperationData deferredOperation, PrototypeWorldSimOpportunityCandidateData blockedHotspotCandidate)
    {
        List<string> tokens = new List<string>(4);
        if (immediateOperation != null && immediateOperation != PrototypeWorldSimQueuedOperationData.Empty)
        {
            tokens.Add("Immediate now: " + CompactSurfaceText(immediateOperation.OperationLabel, 28));
        }

        if (soonOperation != null && soonOperation != PrototypeWorldSimQueuedOperationData.Empty)
        {
            tokens.Add(ResolvePriorityBandLabel(soonOperation.PriorityBandKey) + ": " + CompactSurfaceText(soonOperation.OperationLabel, 28));
        }

        if (deferredOperation != null && deferredOperation != PrototypeWorldSimQueuedOperationData.Empty)
        {
            tokens.Add(ResolvePriorityBandLabel(deferredOperation.PriorityBandKey) + ": " + CompactSurfaceText(deferredOperation.OperationLabel, 26));
        }
        else if (IsMeaningfulOpportunityCandidate(blockedHotspotCandidate))
        {
            tokens.Add("Blocked hard: " + CompactSurfaceText(blockedHotspotCandidate.CityLabel, 24));
        }

        if (HasLatestReturnAftermath())
        {
            tokens.Add("Revisit after run return");
        }

        return tokens.Count > 0 ? string.Join(" | ", tokens.ToArray()) : "None";
    }
    private string BuildCommitmentQueueSummaryText(PrototypeWorldSimOperationChainSurfaceData chain, PrototypeWorldSimQueuedOperationData immediateOperation, PrototypeWorldSimQueuedOperationData soonOperation, PrototypeWorldSimQueuedOperationData deferredOperation)
    {
        List<string> lines = new List<string>(3);
        if (immediateOperation != null && immediateOperation != PrototypeWorldSimQueuedOperationData.Empty)
        {
            lines.Add((chain != null && chain.HasSelection ? "Now" : "Immediate") + ": " + immediateOperation.OperationLabel + " | " + CompactSurfaceText(immediateOperation.ReasonText, 74));
        }

        if (soonOperation != null && soonOperation != PrototypeWorldSimQueuedOperationData.Empty)
        {
            lines.Add("Soon: " + soonOperation.OperationLabel + " | " + CompactSurfaceText(soonOperation.WaitReasonText, 72));
        }

        if (deferredOperation != null && deferredOperation != PrototypeWorldSimQueuedOperationData.Empty)
        {
            lines.Add("Deferred: " + deferredOperation.OperationLabel + " | " + CompactSurfaceText(deferredOperation.WaitReasonText, 66));
        }

        return lines.Count > 0 ? string.Join("\n", lines.ToArray()) : "None";
    }

    private string BuildShortHorizonWorldSummary(PrototypeWorldSimQueuedOperationData immediateOperation, PrototypeWorldSimQueuedOperationData soonOperation, PrototypeWorldSimQueuedOperationData deferredOperation, string blockedHotspotLabel, string blockedHotspotReasonText)
    {
        List<string> lines = new List<string>(4);
        if (immediateOperation != null && immediateOperation != PrototypeWorldSimQueuedOperationData.Empty)
        {
            lines.Add("Now: " + immediateOperation.OperationLabel);
        }

        if (soonOperation != null && soonOperation != PrototypeWorldSimQueuedOperationData.Empty)
        {
            lines.Add("Soon: " + BuildReadySoonCandidateSummary(soonOperation));
        }

        if (HasMeaningfulSurfaceText(blockedHotspotLabel) && blockedHotspotLabel != "None")
        {
            lines.Add("Blocked: " + CompactSurfaceText(blockedHotspotLabel + " | " + blockedHotspotReasonText, 94));
        }

        string urgentPressureNode = ResolveUrgentPressureNodeLabel();
        if (HasMeaningfulSurfaceText(urgentPressureNode) && urgentPressureNode != "None")
        {
            lines.Add("Pressure: " + CompactSurfaceText(urgentPressureNode, 88));
        }
        else if (deferredOperation != null && deferredOperation != PrototypeWorldSimQueuedOperationData.Empty)
        {
            lines.Add("After wait: " + CompactSurfaceText(deferredOperation.OperationLabel, 88));
        }

        return lines.Count > 0 ? string.Join("\n", lines.ToArray()) : "None";
    }

    private string BuildWorldPostureSummary(PrototypeWorldSimQueuedOperationData immediateOperation, PrototypeWorldSimQueuedOperationData soonOperation, string blockedHotspotLabel)
    {
        string readyCountText = ResolveReadyCityCount().ToString();
        string soonCountText = ResolveSoonReadyCityCount().ToString();
        string bestMoveText = immediateOperation != null && immediateOperation != PrototypeWorldSimQueuedOperationData.Empty
            ? CompactSurfaceText(immediateOperation.OperationLabel, 26)
            : "None";
        string soonMoveText = soonOperation != null && soonOperation != PrototypeWorldSimQueuedOperationData.Empty
            ? CompactSurfaceText(soonOperation.OperationLabel, 20)
            : "None";
        string hotspotText = HasMeaningfulSurfaceText(blockedHotspotLabel) && blockedHotspotLabel != "None"
            ? CompactSurfaceText(blockedHotspotLabel, 18)
            : CompactSurfaceText(ResolveUrgentPressureNodeLabel(), 18);
        return "Ready " + readyCountText + " | Soon " + soonCountText + " | Best " + bestMoveText + " | Next " + soonMoveText + " | Hotspot " + hotspotText;
    }

    private string BuildReadySoonCandidateSummary(PrototypeWorldSimQueuedOperationData soonOperation)
    {
        if (soonOperation == null || soonOperation == PrototypeWorldSimQueuedOperationData.Empty)
        {
            return HasMeaningfulSurfaceText(_currentRecoveryForecastSummaryText)
                ? CompactSurfaceText(_currentRecoveryForecastSummaryText, 92)
                : "No soon-ready lane.";
        }

        string prefix = string.Equals(soonOperation.PriorityBandKey, "revisit_after_return", StringComparison.OrdinalIgnoreCase)
            ? "Return: "
            : "Window: ";
        return prefix + soonOperation.OperationLabel + " | " + CompactSurfaceText(soonOperation.WaitReasonText, 66);
    }

    private string ResolveWaitThenDispatchReason(PrototypeWorldSimOpportunityCandidateData candidate)
    {
        if (!IsMeaningfulOpportunityCandidate(candidate))
        {
            return "None";
        }

        if (string.Equals(candidate.ActionStateKey, "wait_active_return", StringComparison.OrdinalIgnoreCase))
        {
            return candidate.CityLabel + " reopens after the active expedition returns.";
        }

        if (string.Equals(candidate.ActionStateKey, "recover_then_dispatch", StringComparison.OrdinalIgnoreCase))
        {
            string etaText = candidate.Score > 0 ? BuildRecoveryEtaText(ResolveCandidateRecoveryDays(candidate)) : "soon";
            return candidate.CityLabel + " becomes available in " + etaText + ". " + CompactSurfaceText(candidate.ComparisonText, 72);
        }

        if (string.Equals(candidate.ActionStateKey, "blocked_strained", StringComparison.OrdinalIgnoreCase))
        {
            int recoveryDays = ResolveCandidateRecoveryDays(candidate);
            string etaText = recoveryDays > 0 ? BuildRecoveryEtaText(recoveryDays) : "the next day step";
            return candidate.CityLabel + " needs a shorter recovery window before dispatch. Recheck in " + etaText + ".";
        }

        if (candidate.IsBlocked)
        {
            return CompactSurfaceText(candidate.BlockedReasonText, 92);
        }

        return CompactSurfaceText(candidate.ReasonText, 92);
    }

    private string BuildDeferredFollowThroughSummary(PrototypeWorldSimOpportunityCandidateData candidate, PrototypeWorldSimOpportunityCandidateData immediateCandidate, PrototypeWorldSimOpportunityCandidateData soonCandidate)
    {
        if (!IsMeaningfulOpportunityCandidate(candidate))
        {
            return "None";
        }

        string reopenTrigger = BuildDeferredRevisitTriggerText(candidate, soonCandidate, PrototypeWorldSimOpportunityCandidateData.Empty);
        string reasonText = candidate.IsBlocked ? candidate.BlockedReasonText : candidate.ReasonText;
        string leaderText = IsMeaningfulOpportunityCandidate(immediateCandidate) && !MatchesOpportunityCandidate(immediateCandidate, candidate)
            ? "Held behind " + CompactSurfaceText(immediateCandidate.CandidateLabel, 32) + ". "
            : string.Empty;
        return leaderText + CompactSurfaceText(reasonText, 70) + (string.IsNullOrEmpty(reopenTrigger) ? string.Empty : " Revisit " + reopenTrigger + ".");
    }

    private string BuildDeferredRevisitTriggerText(PrototypeWorldSimOpportunityCandidateData candidate, PrototypeWorldSimOpportunityCandidateData soonCandidate, PrototypeWorldSimOpportunityCandidateData blockedHotspotCandidate)
    {
        if (!IsMeaningfulOpportunityCandidate(candidate))
        {
            return string.Empty;
        }

        if (string.Equals(candidate.ActionStateKey, "wait_active_return", StringComparison.OrdinalIgnoreCase))
        {
            return "after the run return resolves";
        }

        if (string.Equals(candidate.ActionStateKey, "recover_then_dispatch", StringComparison.OrdinalIgnoreCase) || string.Equals(candidate.ActionStateKey, "blocked_strained", StringComparison.OrdinalIgnoreCase))
        {
            int recoveryDays = ResolveCandidateRecoveryDays(candidate);
            return recoveryDays > 0 ? "after " + BuildRecoveryEtaText(recoveryDays) : "after a short recovery window";
        }

        if (string.Equals(candidate.ActionStateKey, "blocked_no_linked_dungeon", StringComparison.OrdinalIgnoreCase))
        {
            return "when a linked dungeon lane exists";
        }

        if (IsMeaningfulOpportunityCandidate(soonCandidate) && !MatchesOpportunityCandidate(candidate, soonCandidate))
        {
            return "after " + CompactSurfaceText(soonCandidate.CandidateLabel, 28).ToLowerInvariant();
        }

        if (IsMeaningfulOpportunityCandidate(blockedHotspotCandidate) && !MatchesOpportunityCandidate(candidate, blockedHotspotCandidate))
        {
            return "after the blocked hotspot cools";
        }

        return "after the current higher-priority move";
    }

    private string ResolvePriorityBandKey(string slotKey, PrototypeWorldSimOpportunityCandidateData candidate)
    {
        if (!IsMeaningfulOpportunityCandidate(candidate))
        {
            return "none";
        }

        if (string.Equals(slotKey, "immediate", StringComparison.OrdinalIgnoreCase) && !candidate.IsBlocked)
        {
            return "immediate_now";
        }

        if (string.Equals(candidate.ActionStateKey, "wait_active_return", StringComparison.OrdinalIgnoreCase) || MatchesLatestReturnAftermathEntity(candidate.CityId) || MatchesLatestReturnAftermathEntity(candidate.DungeonId))
        {
            return "revisit_after_return";
        }

        if (string.Equals(candidate.ActionStateKey, "recover_then_dispatch", StringComparison.OrdinalIgnoreCase) || string.Equals(candidate.ActionStateKey, "blocked_strained", StringComparison.OrdinalIgnoreCase) || ResolveCandidateRecoveryDays(candidate) > 0)
        {
            return "recover_soon";
        }

        if (!string.IsNullOrEmpty(candidate.CityId) && _cityPressureByEntityId.TryGetValue(candidate.CityId, out PrototypeWorldSimCityPressureSurfaceData cityPressure) && cityPressure != null && cityPressure.Severity >= 4)
        {
            return candidate.IsBlocked ? "blocked_hard" : "pressure_rising";
        }

        return candidate.IsBlocked ? "blocked_hard" : string.Equals(slotKey, "deferred", StringComparison.OrdinalIgnoreCase) ? "deferred_watch" : "watch";
    }

    private string ResolvePriorityBandLabel(string priorityBandKey)
    {
        return string.Equals(priorityBandKey, "immediate_now", StringComparison.OrdinalIgnoreCase)
            ? "Immediate now"
            : string.Equals(priorityBandKey, "recover_soon", StringComparison.OrdinalIgnoreCase)
                ? "Recover soon"
                : string.Equals(priorityBandKey, "pressure_rising", StringComparison.OrdinalIgnoreCase)
                    ? "Pressure rising"
                    : string.Equals(priorityBandKey, "blocked_hard", StringComparison.OrdinalIgnoreCase)
                        ? "Blocked hard"
                        : string.Equals(priorityBandKey, "revisit_after_return", StringComparison.OrdinalIgnoreCase)
                            ? "Revisit after return"
                            : string.Equals(priorityBandKey, "deferred_watch", StringComparison.OrdinalIgnoreCase)
                                ? "Deferred"
                                : "Watch";
    }
    private int ResolveCandidateRecoveryDays(PrototypeWorldSimOpportunityCandidateData candidate)
    {
        return IsMeaningfulOpportunityCandidate(candidate) && !string.IsNullOrEmpty(candidate.CityId)
            ? GetRecoveryDaysToReady(candidate.CityId)
            : 0;
    }

    private string ResolveCandidateActionStateKey(string candidateId)
    {
        if (string.IsNullOrEmpty(candidateId))
        {
            return "none";
        }

        if (_currentOpportunityLadderSurface.PrimaryCandidate != null && string.Equals(_currentOpportunityLadderSurface.PrimaryCandidate.CandidateId, candidateId, StringComparison.OrdinalIgnoreCase))
        {
            return _currentOpportunityLadderSurface.PrimaryCandidate.ActionStateKey;
        }

        if (_currentOpportunityLadderSurface.SecondaryCandidate != null && string.Equals(_currentOpportunityLadderSurface.SecondaryCandidate.CandidateId, candidateId, StringComparison.OrdinalIgnoreCase))
        {
            return _currentOpportunityLadderSurface.SecondaryCandidate.ActionStateKey;
        }

        if (_currentOpportunityLadderSurface.TertiaryCandidate != null && string.Equals(_currentOpportunityLadderSurface.TertiaryCandidate.CandidateId, candidateId, StringComparison.OrdinalIgnoreCase))
        {
            return _currentOpportunityLadderSurface.TertiaryCandidate.ActionStateKey;
        }

        return "none";
    }

    private PrototypeWorldSimOpportunityCandidateData ResolveOpportunityCandidateById(string candidateId)
    {
        if (string.IsNullOrEmpty(candidateId))
        {
            return PrototypeWorldSimOpportunityCandidateData.Empty;
        }

        if (_currentOpportunityLadderSurface.PrimaryCandidate != null && string.Equals(_currentOpportunityLadderSurface.PrimaryCandidate.CandidateId, candidateId, StringComparison.OrdinalIgnoreCase))
        {
            return _currentOpportunityLadderSurface.PrimaryCandidate;
        }

        if (_currentOpportunityLadderSurface.SecondaryCandidate != null && string.Equals(_currentOpportunityLadderSurface.SecondaryCandidate.CandidateId, candidateId, StringComparison.OrdinalIgnoreCase))
        {
            return _currentOpportunityLadderSurface.SecondaryCandidate;
        }

        if (_currentOpportunityLadderSurface.TertiaryCandidate != null && string.Equals(_currentOpportunityLadderSurface.TertiaryCandidate.CandidateId, candidateId, StringComparison.OrdinalIgnoreCase))
        {
            return _currentOpportunityLadderSurface.TertiaryCandidate;
        }

        return PrototypeWorldSimOpportunityCandidateData.Empty;
    }

    private string ResolveUrgentPressureNodeLabel()
    {
        PrototypeWorldSimCityPressureSurfaceData hottestCity = PrototypeWorldSimCityPressureSurfaceData.Empty;
        foreach (KeyValuePair<string, PrototypeWorldSimCityPressureSurfaceData> pair in _cityPressureByEntityId)
        {
            PrototypeWorldSimCityPressureSurfaceData city = pair.Value;
            if (city == null || city == PrototypeWorldSimCityPressureSurfaceData.Empty)
            {
                continue;
            }

            if (hottestCity == PrototypeWorldSimCityPressureSurfaceData.Empty || city.Severity > hottestCity.Severity)
            {
                hottestCity = city;
            }
        }

        if (hottestCity != PrototypeWorldSimCityPressureSurfaceData.Empty)
        {
            return hottestCity.CityLabel + " | " + CompactSurfaceText(hottestCity.PressureText, 56);
        }

        if (HasMeaningfulSurfaceText(CitiesWithCriticalUnmetText))
        {
            return CompactSurfaceText(CitiesWithCriticalUnmetText, 64);
        }

        return HasMeaningfulSurfaceText(CitiesWithShortagesText)
            ? CompactSurfaceText(CitiesWithShortagesText, 64)
            : "None";
    }

    private int ResolveReadyCityCount()
    {
        int count = 0;
        if (_worldData == null || _worldData.Entities == null)
        {
            return count;
        }

        for (int i = 0; i < _worldData.Entities.Length; i++)
        {
            WorldEntityData entity = _worldData.Entities[i];
            if (entity == null || entity.Kind != WorldEntityKind.City)
            {
                continue;
            }

            if (GetDispatchReadinessState(entity.Id) == DispatchReadinessState.Ready)
            {
                count++;
            }
        }

        return count;
    }

    private int ResolveSoonReadyCityCount()
    {
        int count = 0;
        foreach (KeyValuePair<string, PrototypeWorldSimCityPressureSurfaceData> pair in _cityPressureByEntityId)
        {
            PrototypeWorldSimCityPressureSurfaceData city = pair.Value;
            if (city == null || city == PrototypeWorldSimCityPressureSurfaceData.Empty)
            {
                continue;
            }

            if (city.RecoveryDays > 0 && city.RecoveryDays <= 2)
            {
                count++;
            }
        }

        return count;
    }

    private bool IsMeaningfulOpportunityCandidate(PrototypeWorldSimOpportunityCandidateData candidate)
    {
        return candidate != null && candidate != PrototypeWorldSimOpportunityCandidateData.Empty && !string.IsNullOrEmpty(candidate.CandidateId);
    }

    private bool MatchesOpportunityCandidate(PrototypeWorldSimOpportunityCandidateData left, PrototypeWorldSimOpportunityCandidateData right)
    {
        return IsMeaningfulOpportunityCandidate(left) && IsMeaningfulOpportunityCandidate(right) && string.Equals(left.CandidateId, right.CandidateId, StringComparison.OrdinalIgnoreCase);
    }

    private bool IsRecoveryWindowCandidate(PrototypeWorldSimOpportunityCandidateData candidate)
    {
        if (!IsMeaningfulOpportunityCandidate(candidate))
        {
            return false;
        }

        return string.Equals(candidate.ActionStateKey, "recover_then_dispatch", StringComparison.OrdinalIgnoreCase) ||
               string.Equals(candidate.ActionStateKey, "blocked_strained", StringComparison.OrdinalIgnoreCase) ||
               string.Equals(candidate.ActionStateKey, "wait_active_return", StringComparison.OrdinalIgnoreCase);
    }

    private bool IsHardBlockedCandidate(PrototypeWorldSimOpportunityCandidateData candidate)
    {
        if (!IsMeaningfulOpportunityCandidate(candidate))
        {
            return false;
        }

        return string.Equals(candidate.ActionStateKey, "blocked_no_linked_dungeon", StringComparison.OrdinalIgnoreCase) ||
               string.Equals(candidate.ActionStateKey, "blocked_strained", StringComparison.OrdinalIgnoreCase) ||
               (candidate.IsBlocked && !IsRecoveryWindowCandidate(candidate));
    }

    private void ApplyWorldCommitmentQueuePulseState()
    {
        ApplyQueuedOperationPulse(_currentCommitmentQueueSurface.ImmediateOperation, 2);
        ApplyQueuedOperationPulse(_currentCommitmentQueueSurface.SoonOperation, 1);
        ApplyQueuedOperationPulse(_currentCommitmentQueueSurface.DeferredOperation, 0);
    }

    private void ApplyQueuedOperationPulse(PrototypeWorldSimQueuedOperationData operation, int emphasisTier)
    {
        if (operation == null || operation == PrototypeWorldSimQueuedOperationData.Empty)
        {
            return;
        }

        string pressureStateKey = string.Empty;
        if (!string.IsNullOrEmpty(operation.CityId) && _cityPressureByEntityId.TryGetValue(operation.CityId, out PrototypeWorldSimCityPressureSurfaceData cityPressure) && cityPressure != null)
        {
            pressureStateKey = cityPressure.StateKey;
        }

        if (operation.HasCity && _markerByEntityId.TryGetValue(operation.CityId, out WorldSelectableMarker cityMarker) && cityMarker != null)
        {
            if (emphasisTier >= 2)
            {
                cityMarker.SetActionReady(!operation.IsBlocked);
                if (!string.IsNullOrEmpty(pressureStateKey))
                {
                    cityMarker.SetPressureState(pressureStateKey, !operation.IsBlocked);
                }
            }
            else if (emphasisTier == 1)
            {
                cityMarker.SetLinkedFocus(true);
                if (!string.IsNullOrEmpty(pressureStateKey))
                {
                    cityMarker.SetPressureState(pressureStateKey, true);
                }
            }
            else
            {
                cityMarker.SetOutcomePinned(true);
                if (!string.IsNullOrEmpty(pressureStateKey))
                {
                    cityMarker.SetPressureState(pressureStateKey, false);
                }
            }
        }

        if (operation.HasDungeon && _markerByEntityId.TryGetValue(operation.DungeonId, out WorldSelectableMarker dungeonMarker) && dungeonMarker != null)
        {
            if (emphasisTier >= 1)
            {
                dungeonMarker.SetLinkedFocus(true);
            }
            else
            {
                dungeonMarker.SetOutcomePinned(true);
            }
        }

        if (operation.HasRoute && _routePulseByRouteId.TryGetValue(operation.RouteId, out WorldBoardRoutePressureVisual routeVisual) && routeVisual != null)
        {
            string stateKey = _routePressureByRouteId.TryGetValue(operation.RouteId, out PrototypeWorldSimRoutePressureSurfaceData routePressure) && routePressure != null
                ? routePressure.StateKey
                : "open";
            routeVisual.ApplyState(stateKey, emphasisTier >= 2, emphasisTier == 1 || (!operation.IsBlocked && emphasisTier >= 0));
        }

        string connectorKey = BuildLinkedConnectorKey(operation.CityId, operation.DungeonId);
        if (!string.IsNullOrEmpty(connectorKey) && _linkedConnectorByKey.TryGetValue(connectorKey, out WorldBoardConnectorPulseVisual connectorVisual) && connectorVisual != null)
        {
            if (emphasisTier >= 2)
            {
                connectorVisual.ApplyPulse(true, false);
            }
            else if (emphasisTier == 1)
            {
                connectorVisual.ApplyPulse(false, true);
            }
        }
    }
}
