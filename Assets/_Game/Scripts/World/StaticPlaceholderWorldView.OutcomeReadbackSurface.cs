using System;
using System.Collections.Generic;

public sealed partial class StaticPlaceholderWorldView
{
    private sealed class PrototypeWorldSimOutcomeCityBaselineData
    {
        public static readonly PrototypeWorldSimOutcomeCityBaselineData Empty = new PrototypeWorldSimOutcomeCityBaselineData(string.Empty, "None", "stable", "None", "None", 0, 0);

        public readonly string CityId;
        public readonly string CityLabel;
        public readonly string StateKey;
        public readonly string NeedPressureText;
        public readonly string PressureText;
        public readonly int RecoveryDays;
        public readonly int Severity;

        public PrototypeWorldSimOutcomeCityBaselineData(string cityId, string cityLabel, string stateKey, string needPressureText, string pressureText, int recoveryDays, int severity)
        {
            CityId = string.IsNullOrEmpty(cityId) ? string.Empty : cityId;
            CityLabel = string.IsNullOrEmpty(cityLabel) ? "None" : cityLabel;
            StateKey = string.IsNullOrEmpty(stateKey) ? "stable" : stateKey;
            NeedPressureText = string.IsNullOrEmpty(needPressureText) ? "None" : needPressureText;
            PressureText = string.IsNullOrEmpty(pressureText) ? "None" : pressureText;
            RecoveryDays = Math.Max(0, recoveryDays);
            Severity = Math.Max(0, severity);
        }
    }

    private sealed class PrototypeWorldSimOutcomeBaselineData
    {
        public static readonly PrototypeWorldSimOutcomeBaselineData Empty = new PrototypeWorldSimOutcomeBaselineData(
            string.Empty,
            string.Empty,
            string.Empty,
            string.Empty,
            string.Empty,
            string.Empty,
            string.Empty,
            string.Empty,
            string.Empty,
            string.Empty,
            string.Empty,
            string.Empty,
            string.Empty,
            string.Empty,
            string.Empty,
            string.Empty,
            string.Empty,
            false,
            string.Empty,
            string.Empty,
            string.Empty,
            string.Empty,
            string.Empty,
            string.Empty,
            0,
            "None",
            "None",
            "None",
            Array.Empty<PrototypeWorldSimOutcomeCityBaselineData>());

        public readonly string TriggerKey;
        public readonly string SelectedEntityId;
        public readonly string SelectedEntityLabel;
        public readonly string SelectedEntityKind;
        public readonly string CityId;
        public readonly string CityLabel;
        public readonly string DungeonId;
        public readonly string DungeonLabel;
        public readonly string RouteId;
        public readonly string RouteLabel;
        public readonly string OperationLabel;
        public readonly string BaselineReasonText;
        public readonly string ExpectedOutcomeText;
        public readonly string BestMoveCandidateId;
        public readonly string BestMoveLabel;
        public readonly string BestMoveCityId;
        public readonly string BestMovePriorityBandKey;
        public readonly bool BestMoveWasBlocked;
        public readonly string SoonCandidateId;
        public readonly string SoonCandidateLabel;
        public readonly string DeferredCandidateId;
        public readonly string DeferredCandidateLabel;
        public readonly string BlockedHotspotLabel;
        public readonly string NetworkRouteSummaryText;
        public readonly int RecoveryDays;
        public readonly string NeedPressureText;
        public readonly string ReadinessText;
        public readonly string RoutePlanSummaryText;
        public readonly PrototypeWorldSimOutcomeCityBaselineData[] CityBaselines;

        public bool HasBaseline => !string.IsNullOrEmpty(TriggerKey);

        public PrototypeWorldSimOutcomeBaselineData(
            string triggerKey,
            string selectedEntityId,
            string selectedEntityLabel,
            string selectedEntityKind,
            string cityId,
            string cityLabel,
            string dungeonId,
            string dungeonLabel,
            string routeId,
            string routeLabel,
            string operationLabel,
            string baselineReasonText,
            string expectedOutcomeText,
            string bestMoveCandidateId,
            string bestMoveLabel,
            string bestMoveCityId,
            string bestMovePriorityBandKey,
            bool bestMoveWasBlocked,
            string soonCandidateId,
            string soonCandidateLabel,
            string deferredCandidateId,
            string deferredCandidateLabel,
            string blockedHotspotLabel,
            string networkRouteSummaryText,
            int recoveryDays,
            string needPressureText,
            string readinessText,
            string routePlanSummaryText,
            PrototypeWorldSimOutcomeCityBaselineData[] cityBaselines)
        {
            TriggerKey = string.IsNullOrEmpty(triggerKey) ? string.Empty : triggerKey;
            SelectedEntityId = string.IsNullOrEmpty(selectedEntityId) ? string.Empty : selectedEntityId;
            SelectedEntityLabel = string.IsNullOrEmpty(selectedEntityLabel) ? "None" : selectedEntityLabel;
            SelectedEntityKind = string.IsNullOrEmpty(selectedEntityKind) ? "None" : selectedEntityKind;
            CityId = string.IsNullOrEmpty(cityId) ? string.Empty : cityId;
            CityLabel = string.IsNullOrEmpty(cityLabel) ? "None" : cityLabel;
            DungeonId = string.IsNullOrEmpty(dungeonId) ? string.Empty : dungeonId;
            DungeonLabel = string.IsNullOrEmpty(dungeonLabel) ? "None" : dungeonLabel;
            RouteId = string.IsNullOrEmpty(routeId) ? string.Empty : routeId;
            RouteLabel = string.IsNullOrEmpty(routeLabel) ? "None" : routeLabel;
            OperationLabel = string.IsNullOrEmpty(operationLabel) ? "None" : operationLabel;
            BaselineReasonText = string.IsNullOrEmpty(baselineReasonText) ? "None" : baselineReasonText;
            ExpectedOutcomeText = string.IsNullOrEmpty(expectedOutcomeText) ? "None" : expectedOutcomeText;
            BestMoveCandidateId = string.IsNullOrEmpty(bestMoveCandidateId) ? string.Empty : bestMoveCandidateId;
            BestMoveLabel = string.IsNullOrEmpty(bestMoveLabel) ? "None" : bestMoveLabel;
            BestMoveCityId = string.IsNullOrEmpty(bestMoveCityId) ? string.Empty : bestMoveCityId;
            BestMovePriorityBandKey = string.IsNullOrEmpty(bestMovePriorityBandKey) ? "none" : bestMovePriorityBandKey;
            BestMoveWasBlocked = bestMoveWasBlocked;
            SoonCandidateId = string.IsNullOrEmpty(soonCandidateId) ? string.Empty : soonCandidateId;
            SoonCandidateLabel = string.IsNullOrEmpty(soonCandidateLabel) ? "None" : soonCandidateLabel;
            DeferredCandidateId = string.IsNullOrEmpty(deferredCandidateId) ? string.Empty : deferredCandidateId;
            DeferredCandidateLabel = string.IsNullOrEmpty(deferredCandidateLabel) ? "None" : deferredCandidateLabel;
            BlockedHotspotLabel = string.IsNullOrEmpty(blockedHotspotLabel) ? "None" : blockedHotspotLabel;
            NetworkRouteSummaryText = string.IsNullOrEmpty(networkRouteSummaryText) ? "None" : networkRouteSummaryText;
            RecoveryDays = Math.Max(0, recoveryDays);
            NeedPressureText = string.IsNullOrEmpty(needPressureText) ? "None" : needPressureText;
            ReadinessText = string.IsNullOrEmpty(readinessText) ? "None" : readinessText;
            RoutePlanSummaryText = string.IsNullOrEmpty(routePlanSummaryText) ? "None" : routePlanSummaryText;
            CityBaselines = cityBaselines ?? Array.Empty<PrototypeWorldSimOutcomeCityBaselineData>();
        }
    }

    private sealed class PrototypeWorldSimOutcomeCityChangeData
    {
        public static readonly PrototypeWorldSimOutcomeCityChangeData Empty = new PrototypeWorldSimOutcomeCityChangeData(
            string.Empty,
            "None",
            "stable",
            "stable",
            "None",
            "None",
            "None",
            "None",
            0,
            0,
            0,
            0);

        public readonly string CityId;
        public readonly string CityLabel;
        public readonly string BaselineStateKey;
        public readonly string CurrentStateKey;
        public readonly string BaselineNeedPressureText;
        public readonly string CurrentNeedPressureText;
        public readonly string BaselinePressureText;
        public readonly string CurrentPressureText;
        public readonly int BaselineRecoveryDays;
        public readonly int CurrentRecoveryDays;
        public readonly int BaselineSeverity;
        public readonly int CurrentSeverity;

        public int ReliefScore => Math.Max(0, BaselineSeverity - CurrentSeverity) + (BaselineRecoveryDays > CurrentRecoveryDays ? 1 : 0);
        public int ReboundScore => Math.Max(0, CurrentSeverity - BaselineSeverity) + (CurrentRecoveryDays > BaselineRecoveryDays ? 1 : 0);
        public bool HasChange => !string.IsNullOrEmpty(CityId);

        public PrototypeWorldSimOutcomeCityChangeData(string cityId, string cityLabel, string baselineStateKey, string currentStateKey, string baselineNeedPressureText, string currentNeedPressureText, string baselinePressureText, string currentPressureText, int baselineRecoveryDays, int currentRecoveryDays, int baselineSeverity, int currentSeverity)
        {
            CityId = string.IsNullOrEmpty(cityId) ? string.Empty : cityId;
            CityLabel = string.IsNullOrEmpty(cityLabel) ? "None" : cityLabel;
            BaselineStateKey = string.IsNullOrEmpty(baselineStateKey) ? "stable" : baselineStateKey;
            CurrentStateKey = string.IsNullOrEmpty(currentStateKey) ? "stable" : currentStateKey;
            BaselineNeedPressureText = string.IsNullOrEmpty(baselineNeedPressureText) ? "None" : baselineNeedPressureText;
            CurrentNeedPressureText = string.IsNullOrEmpty(currentNeedPressureText) ? "None" : currentNeedPressureText;
            BaselinePressureText = string.IsNullOrEmpty(baselinePressureText) ? "None" : baselinePressureText;
            CurrentPressureText = string.IsNullOrEmpty(currentPressureText) ? "None" : currentPressureText;
            BaselineRecoveryDays = Math.Max(0, baselineRecoveryDays);
            CurrentRecoveryDays = Math.Max(0, currentRecoveryDays);
            BaselineSeverity = Math.Max(0, baselineSeverity);
            CurrentSeverity = Math.Max(0, currentSeverity);
        }
    }

    private sealed class PrototypeWorldSimOutcomeReadbackSurfaceData
    {
        public static readonly PrototypeWorldSimOutcomeReadbackSurfaceData Empty = new PrototypeWorldSimOutcomeReadbackSurfaceData(
            PrototypeWorldSimOutcomeBaselineData.Empty,
            "none",
            "None",
            "None",
            "None",
            "None",
            "None",
            "None",
            string.Empty,
            string.Empty,
            string.Empty,
            string.Empty,
            string.Empty,
            string.Empty);

        public readonly PrototypeWorldSimOutcomeBaselineData Baseline;
        public readonly string ConfirmationStateKey;
        public readonly string OutcomeReadbackSummaryText;
        public readonly string ConfirmationSummaryText;
        public readonly string ChangedBestMoveSummaryText;
        public readonly string RelievedHotspotSummaryText;
        public readonly string ReboundHotspotSummaryText;
        public readonly string CorrectiveFollowUpSummaryText;
        public readonly string RelievedCityId;
        public readonly string ReboundCityId;
        public readonly string FollowUpCityId;
        public readonly string FollowUpDungeonId;
        public readonly string FollowUpRouteId;
        public readonly string FollowUpPriorityBandKey;

        public PrototypeWorldSimOutcomeReadbackSurfaceData(PrototypeWorldSimOutcomeBaselineData baseline, string confirmationStateKey, string outcomeReadbackSummaryText, string confirmationSummaryText, string changedBestMoveSummaryText, string relievedHotspotSummaryText, string reboundHotspotSummaryText, string correctiveFollowUpSummaryText, string relievedCityId, string reboundCityId, string followUpCityId, string followUpDungeonId, string followUpRouteId, string followUpPriorityBandKey)
        {
            Baseline = baseline ?? PrototypeWorldSimOutcomeBaselineData.Empty;
            ConfirmationStateKey = string.IsNullOrEmpty(confirmationStateKey) ? "none" : confirmationStateKey;
            OutcomeReadbackSummaryText = string.IsNullOrEmpty(outcomeReadbackSummaryText) ? "None" : outcomeReadbackSummaryText;
            ConfirmationSummaryText = string.IsNullOrEmpty(confirmationSummaryText) ? "None" : confirmationSummaryText;
            ChangedBestMoveSummaryText = string.IsNullOrEmpty(changedBestMoveSummaryText) ? "None" : changedBestMoveSummaryText;
            RelievedHotspotSummaryText = string.IsNullOrEmpty(relievedHotspotSummaryText) ? "None" : relievedHotspotSummaryText;
            ReboundHotspotSummaryText = string.IsNullOrEmpty(reboundHotspotSummaryText) ? "None" : reboundHotspotSummaryText;
            CorrectiveFollowUpSummaryText = string.IsNullOrEmpty(correctiveFollowUpSummaryText) ? "None" : correctiveFollowUpSummaryText;
            RelievedCityId = string.IsNullOrEmpty(relievedCityId) ? string.Empty : relievedCityId;
            ReboundCityId = string.IsNullOrEmpty(reboundCityId) ? string.Empty : reboundCityId;
            FollowUpCityId = string.IsNullOrEmpty(followUpCityId) ? string.Empty : followUpCityId;
            FollowUpDungeonId = string.IsNullOrEmpty(followUpDungeonId) ? string.Empty : followUpDungeonId;
            FollowUpRouteId = string.IsNullOrEmpty(followUpRouteId) ? string.Empty : followUpRouteId;
            FollowUpPriorityBandKey = string.IsNullOrEmpty(followUpPriorityBandKey) ? "none" : followUpPriorityBandKey;
        }
    }

    private PrototypeWorldSimOutcomeBaselineData _currentOutcomeBaseline = PrototypeWorldSimOutcomeBaselineData.Empty;
    private PrototypeWorldSimOutcomeReadbackSurfaceData _currentOutcomeReadbackSurface = PrototypeWorldSimOutcomeReadbackSurfaceData.Empty;
    private string _currentOutcomeReadbackSummaryText = "None";
    private string _currentConfirmationStateSummaryText = "None";
    private string _currentChangedBestMoveSummaryText = "None";
    private string _currentRelievedHotspotSummaryText = "None";
    private string _currentReboundHotspotSummaryText = "None";
    private string _currentCorrectiveFollowUpSummaryText = "None";

    public string CurrentOutcomeReadbackSummaryText => string.IsNullOrEmpty(_currentOutcomeReadbackSummaryText) ? "None" : _currentOutcomeReadbackSummaryText;
    public string CurrentConfirmationStateSummaryText => string.IsNullOrEmpty(_currentConfirmationStateSummaryText) ? "None" : _currentConfirmationStateSummaryText;
    public string CurrentChangedBestMoveSummaryText => string.IsNullOrEmpty(_currentChangedBestMoveSummaryText) ? "None" : _currentChangedBestMoveSummaryText;
    public string CurrentRelievedHotspotSummaryText => string.IsNullOrEmpty(_currentRelievedHotspotSummaryText) ? "None" : _currentRelievedHotspotSummaryText;
    public string CurrentReboundHotspotSummaryText => string.IsNullOrEmpty(_currentReboundHotspotSummaryText) ? "None" : _currentReboundHotspotSummaryText;
    public string CurrentCorrectiveFollowUpSummaryText => string.IsNullOrEmpty(_currentCorrectiveFollowUpSummaryText) ? "None" : _currentCorrectiveFollowUpSummaryText;

    private void ResetWorldOutcomeReadbackSurfaceValues()
    {
        _currentOutcomeReadbackSurface = PrototypeWorldSimOutcomeReadbackSurfaceData.Empty;
        _currentOutcomeReadbackSummaryText = "None";
        _currentConfirmationStateSummaryText = "None";
        _currentChangedBestMoveSummaryText = "None";
        _currentRelievedHotspotSummaryText = "None";
        _currentReboundHotspotSummaryText = "None";
        _currentCorrectiveFollowUpSummaryText = "None";
    }

    private void ResetWorldOutcomeReadbackState()
    {
        _currentOutcomeBaseline = PrototypeWorldSimOutcomeBaselineData.Empty;
        ResetWorldOutcomeReadbackSurfaceValues();
    }
}
public sealed partial class StaticPlaceholderWorldView
{
    private void CaptureWorldOperationBaseline(string triggerKey, string routeIdOverride = null)
    {
        PrototypeWorldSimOperationChainSurfaceData chain = _currentOperationChainSurface != null
            ? _currentOperationChainSurface
            : ResolveSelectedOperationChain();
        PrototypeWorldSimQueuedOperationData immediateOperation = _currentCommitmentQueueSurface != null
            ? _currentCommitmentQueueSurface.ImmediateOperation
            : PrototypeWorldSimQueuedOperationData.Empty;
        PrototypeWorldSimQueuedOperationData soonOperation = _currentCommitmentQueueSurface != null
            ? _currentCommitmentQueueSurface.SoonOperation
            : PrototypeWorldSimQueuedOperationData.Empty;
        PrototypeWorldSimQueuedOperationData deferredOperation = _currentCommitmentQueueSurface != null
            ? _currentCommitmentQueueSurface.DeferredOperation
            : PrototypeWorldSimQueuedOperationData.Empty;

        string cityId = GetChainCityId(chain);
        if (string.IsNullOrEmpty(cityId) && immediateOperation != null && immediateOperation != PrototypeWorldSimQueuedOperationData.Empty)
        {
            cityId = immediateOperation.CityId;
        }

        string dungeonId = GetChainDungeonId(chain);
        if (string.IsNullOrEmpty(dungeonId) && immediateOperation != null && immediateOperation != PrototypeWorldSimQueuedOperationData.Empty)
        {
            dungeonId = immediateOperation.DungeonId;
        }

        string routeId = !string.IsNullOrEmpty(routeIdOverride)
            ? routeIdOverride
            : !string.IsNullOrEmpty(_selectedRouteId)
                ? _selectedRouteId
                : immediateOperation != null && immediateOperation != PrototypeWorldSimQueuedOperationData.Empty
                    ? immediateOperation.RouteId
                    : string.Empty;
        string cityLabel = ResolveEntityLabel(cityId);
        string dungeonLabel = ResolveEntityLabel(dungeonId);
        string routeLabel = ResolveDungeonRouteLabel(dungeonId, routeId);
        string operationLabel = BuildOutcomeBaselineOperationLabel(triggerKey, chain, immediateOperation, cityLabel, dungeonLabel, routeLabel);
        string baselineReasonText = BuildOutcomeBaselineReasonText(triggerKey, chain, immediateOperation, soonOperation);
        string expectedOutcomeText = BuildOutcomeBaselineExpectedText(triggerKey, cityId, dungeonId, routeId, immediateOperation, soonOperation);
        string routePlanSummaryText = BuildOutcomeBaselineRoutePlanSummary(cityId, dungeonId, routeId);
        string needPressureText = !string.IsNullOrEmpty(cityId) ? BuildNeedPressureText(cityId) : "None";
        string readinessText = !string.IsNullOrEmpty(cityId) ? GetDispatchReadinessText(cityId) : "None";
        int recoveryDays = !string.IsNullOrEmpty(cityId) ? GetRecoveryDaysToReady(cityId) : 0;

        _currentOutcomeBaseline = new PrototypeWorldSimOutcomeBaselineData(
            string.IsNullOrEmpty(triggerKey) ? "world" : triggerKey,
            chain != null ? chain.SelectedEntityId : string.Empty,
            chain != null ? chain.SelectedEntityLabel : "None",
            chain != null ? chain.SelectedEntityKind : "None",
            cityId,
            cityLabel,
            dungeonId,
            dungeonLabel,
            routeId,
            routeLabel,
            operationLabel,
            baselineReasonText,
            expectedOutcomeText,
            immediateOperation != null ? immediateOperation.CandidateId : string.Empty,
            immediateOperation != null ? immediateOperation.OperationLabel : "None",
            immediateOperation != null ? immediateOperation.CityId : string.Empty,
            immediateOperation != null ? immediateOperation.PriorityBandKey : "none",
            immediateOperation != null && immediateOperation != PrototypeWorldSimQueuedOperationData.Empty && immediateOperation.IsBlocked,
            soonOperation != null ? soonOperation.CandidateId : string.Empty,
            soonOperation != null ? soonOperation.OperationLabel : "None",
            deferredOperation != null ? deferredOperation.CandidateId : string.Empty,
            deferredOperation != null ? deferredOperation.OperationLabel : "None",
            _currentCommitmentQueueSurface != null ? _currentCommitmentQueueSurface.BlockedHotspotLabel : "None",
            _currentRouteSaturationSummaryText,
            recoveryDays,
            needPressureText,
            readinessText,
            routePlanSummaryText,
            CaptureOutcomeCityBaselineSnapshot());
        ResetWorldOutcomeReadbackSurfaceValues();
    }

    private void RefreshWorldOutcomeReadbackSurface(PrototypeWorldSimOperationChainSurfaceData chain)
    {
        ResetWorldOutcomeReadbackSurfaceValues();
        if (_currentOutcomeBaseline == null || !_currentOutcomeBaseline.HasBaseline)
        {
            return;
        }

        PrototypeWorldSimQueuedOperationData immediateOperation = _currentCommitmentQueueSurface != null
            ? _currentCommitmentQueueSurface.ImmediateOperation
            : PrototypeWorldSimQueuedOperationData.Empty;
        PrototypeWorldSimQueuedOperationData soonOperation = _currentCommitmentQueueSurface != null
            ? _currentCommitmentQueueSurface.SoonOperation
            : PrototypeWorldSimQueuedOperationData.Empty;
        PrototypeWorldSimQueuedOperationData deferredOperation = _currentCommitmentQueueSurface != null
            ? _currentCommitmentQueueSurface.DeferredOperation
            : PrototypeWorldSimQueuedOperationData.Empty;
        PrototypeWorldSimOutcomeCityChangeData focusChange = ResolveOutcomeFocusCityChange(_currentOutcomeBaseline);
        PrototypeWorldSimOutcomeCityChangeData relievedChange = ResolveMostRelievedHotspot(_currentOutcomeBaseline, focusChange);
        PrototypeWorldSimOutcomeCityChangeData reboundChange = ResolveMostReboundHotspot(_currentOutcomeBaseline, focusChange, immediateOperation);
        string actualHeadline = BuildOutcomeActualHeadline(_currentOutcomeBaseline, focusChange, immediateOperation);
        string actualDeltaText = BuildOutcomeActualDeltaText(_currentOutcomeBaseline, focusChange, immediateOperation);
        string changedBestMoveSummaryText = BuildChangedBestMoveSummary(_currentOutcomeBaseline, immediateOperation, soonOperation, deferredOperation);
        string relievedHotspotSummaryText = BuildRelievedHotspotSummary(_currentOutcomeBaseline, focusChange, relievedChange);
        string reboundHotspotSummaryText = BuildReboundHotspotSummary(_currentOutcomeBaseline, reboundChange);
        string correctiveFollowUpSummaryText = BuildCorrectiveFollowUpSummary(immediateOperation, soonOperation, deferredOperation);
        string confirmationStateKey = ResolveOutcomeConfirmationStateKey(_currentOutcomeBaseline, focusChange, relievedChange, reboundChange, immediateOperation, soonOperation);
        string confirmationSummaryText = BuildOutcomeConfirmationSummary(confirmationStateKey, actualDeltaText, changedBestMoveSummaryText, relievedHotspotSummaryText, reboundHotspotSummaryText, correctiveFollowUpSummaryText);
        string outcomeReadbackSummaryText = BuildOutcomeReadbackSummary(_currentOutcomeBaseline, actualHeadline, actualDeltaText);

        _currentOutcomeReadbackSurface = new PrototypeWorldSimOutcomeReadbackSurfaceData(
            _currentOutcomeBaseline,
            confirmationStateKey,
            outcomeReadbackSummaryText,
            confirmationSummaryText,
            changedBestMoveSummaryText,
            relievedHotspotSummaryText,
            reboundHotspotSummaryText,
            correctiveFollowUpSummaryText,
            relievedChange != null ? relievedChange.CityId : string.Empty,
            reboundChange != null ? reboundChange.CityId : string.Empty,
            immediateOperation != null ? immediateOperation.CityId : string.Empty,
            immediateOperation != null ? immediateOperation.DungeonId : string.Empty,
            immediateOperation != null ? immediateOperation.RouteId : string.Empty,
            immediateOperation != null ? immediateOperation.PriorityBandKey : "none");
        _currentOutcomeReadbackSummaryText = outcomeReadbackSummaryText;
        _currentConfirmationStateSummaryText = confirmationSummaryText;
        _currentChangedBestMoveSummaryText = changedBestMoveSummaryText;
        _currentRelievedHotspotSummaryText = relievedHotspotSummaryText;
        _currentReboundHotspotSummaryText = reboundHotspotSummaryText;
        _currentCorrectiveFollowUpSummaryText = correctiveFollowUpSummaryText;
    }

    private PrototypeWorldSimOutcomeCityBaselineData[] CaptureOutcomeCityBaselineSnapshot()
    {
        if (_cityPressureByEntityId.Count <= 0)
        {
            return Array.Empty<PrototypeWorldSimOutcomeCityBaselineData>();
        }

        List<PrototypeWorldSimOutcomeCityBaselineData> result = new List<PrototypeWorldSimOutcomeCityBaselineData>(_cityPressureByEntityId.Count);
        foreach (KeyValuePair<string, PrototypeWorldSimCityPressureSurfaceData> pair in _cityPressureByEntityId)
        {
            PrototypeWorldSimCityPressureSurfaceData city = pair.Value;
            if (city == null || city == PrototypeWorldSimCityPressureSurfaceData.Empty)
            {
                continue;
            }

            result.Add(new PrototypeWorldSimOutcomeCityBaselineData(
                city.CityId,
                city.CityLabel,
                city.StateKey,
                BuildNeedPressureText(city.CityId),
                city.PressureText,
                city.RecoveryDays,
                city.Severity));
        }

        return result.ToArray();
    }

    private PrototypeWorldSimOutcomeCityBaselineData ResolveOutcomeBaselineCity(string cityId)
    {
        if (string.IsNullOrEmpty(cityId) || _currentOutcomeBaseline == null || _currentOutcomeBaseline.CityBaselines == null)
        {
            return PrototypeWorldSimOutcomeCityBaselineData.Empty;
        }

        for (int i = 0; i < _currentOutcomeBaseline.CityBaselines.Length; i++)
        {
            PrototypeWorldSimOutcomeCityBaselineData city = _currentOutcomeBaseline.CityBaselines[i];
            if (city != null && string.Equals(city.CityId, cityId, StringComparison.OrdinalIgnoreCase))
            {
                return city;
            }
        }

        return PrototypeWorldSimOutcomeCityBaselineData.Empty;
    }

    private PrototypeWorldSimOutcomeCityChangeData ResolveOutcomeFocusCityChange(PrototypeWorldSimOutcomeBaselineData baseline)
    {
        if (baseline == null || !baseline.HasBaseline)
        {
            return PrototypeWorldSimOutcomeCityChangeData.Empty;
        }

        string cityId = !string.IsNullOrEmpty(baseline.CityId)
            ? baseline.CityId
            : !string.IsNullOrEmpty(baseline.BestMoveCityId)
                ? baseline.BestMoveCityId
                : string.Empty;
        return BuildOutcomeCityChangeData(ResolveOutcomeBaselineCity(cityId));
    }
}
public sealed partial class StaticPlaceholderWorldView
{
    private PrototypeWorldSimOutcomeCityChangeData ResolveMostRelievedHotspot(PrototypeWorldSimOutcomeBaselineData baseline, PrototypeWorldSimOutcomeCityChangeData preferredChange)
    {
        PrototypeWorldSimOutcomeCityChangeData best = PrototypeWorldSimOutcomeCityChangeData.Empty;
        int bestScore = 0;
        if (baseline == null || baseline.CityBaselines == null)
        {
            return preferredChange != null && preferredChange.ReliefScore > 0 ? preferredChange : PrototypeWorldSimOutcomeCityChangeData.Empty;
        }

        for (int i = 0; i < baseline.CityBaselines.Length; i++)
        {
            PrototypeWorldSimOutcomeCityBaselineData baselineCity = baseline.CityBaselines[i];
            PrototypeWorldSimOutcomeCityChangeData change = BuildOutcomeCityChangeData(baselineCity);
            if (change == null || !change.HasChange || change.ReliefScore <= 0)
            {
                continue;
            }

            if (best == PrototypeWorldSimOutcomeCityChangeData.Empty || change.ReliefScore > bestScore || (change.ReliefScore == bestScore && change.BaselineSeverity > best.BaselineSeverity))
            {
                best = change;
                bestScore = change.ReliefScore;
            }
        }

        if (best == PrototypeWorldSimOutcomeCityChangeData.Empty && preferredChange != null && preferredChange.ReliefScore > 0)
        {
            return preferredChange;
        }

        return best;
    }

    private PrototypeWorldSimOutcomeCityChangeData ResolveMostReboundHotspot(PrototypeWorldSimOutcomeBaselineData baseline, PrototypeWorldSimOutcomeCityChangeData preferredChange, PrototypeWorldSimQueuedOperationData immediateOperation)
    {
        PrototypeWorldSimOutcomeCityChangeData best = PrototypeWorldSimOutcomeCityChangeData.Empty;
        int bestScore = 0;
        if (baseline != null && baseline.CityBaselines != null)
        {
            for (int i = 0; i < baseline.CityBaselines.Length; i++)
            {
                PrototypeWorldSimOutcomeCityBaselineData baselineCity = baseline.CityBaselines[i];
                PrototypeWorldSimOutcomeCityChangeData change = BuildOutcomeCityChangeData(baselineCity);
                if (change == null || !change.HasChange || change.ReboundScore <= 0)
                {
                    continue;
                }

                if (best == PrototypeWorldSimOutcomeCityChangeData.Empty || change.ReboundScore > bestScore || (change.ReboundScore == bestScore && change.CurrentSeverity > best.CurrentSeverity))
                {
                    best = change;
                    bestScore = change.ReboundScore;
                }
            }
        }

        if (best != PrototypeWorldSimOutcomeCityChangeData.Empty)
        {
            return best;
        }

        if (immediateOperation != null && immediateOperation != PrototypeWorldSimQueuedOperationData.Empty && !string.IsNullOrEmpty(immediateOperation.CityId))
        {
            PrototypeWorldSimOutcomeCityChangeData immediateChange = BuildOutcomeCityChangeData(ResolveOutcomeBaselineCity(immediateOperation.CityId));
            if (immediateChange != null && immediateChange.HasChange && immediateChange.CurrentSeverity >= 3 && !string.Equals(immediateChange.CityId, preferredChange != null ? preferredChange.CityId : string.Empty, StringComparison.OrdinalIgnoreCase))
            {
                return immediateChange;
            }
        }

        return PrototypeWorldSimOutcomeCityChangeData.Empty;
    }

    private PrototypeWorldSimOutcomeCityChangeData BuildOutcomeCityChangeData(PrototypeWorldSimOutcomeCityBaselineData baselineCity)
    {
        if (baselineCity == null || baselineCity == PrototypeWorldSimOutcomeCityBaselineData.Empty || string.IsNullOrEmpty(baselineCity.CityId))
        {
            return PrototypeWorldSimOutcomeCityChangeData.Empty;
        }

        PrototypeWorldSimCityPressureSurfaceData currentCity = _cityPressureByEntityId.TryGetValue(baselineCity.CityId, out PrototypeWorldSimCityPressureSurfaceData foundCity) && foundCity != null
            ? foundCity
            : PrototypeWorldSimCityPressureSurfaceData.Empty;
        string currentNeedPressureText = currentCity != PrototypeWorldSimCityPressureSurfaceData.Empty
            ? BuildNeedPressureText(currentCity.CityId)
            : baselineCity.NeedPressureText;

        return new PrototypeWorldSimOutcomeCityChangeData(
            baselineCity.CityId,
            baselineCity.CityLabel,
            baselineCity.StateKey,
            currentCity != PrototypeWorldSimCityPressureSurfaceData.Empty ? currentCity.StateKey : baselineCity.StateKey,
            baselineCity.NeedPressureText,
            currentNeedPressureText,
            baselineCity.PressureText,
            currentCity != PrototypeWorldSimCityPressureSurfaceData.Empty ? currentCity.PressureText : baselineCity.PressureText,
            baselineCity.RecoveryDays,
            currentCity != PrototypeWorldSimCityPressureSurfaceData.Empty ? currentCity.RecoveryDays : baselineCity.RecoveryDays,
            baselineCity.Severity,
            currentCity != PrototypeWorldSimCityPressureSurfaceData.Empty ? currentCity.Severity : baselineCity.Severity);
    }

    private string BuildOutcomeBaselineOperationLabel(string triggerKey, PrototypeWorldSimOperationChainSurfaceData chain, PrototypeWorldSimQueuedOperationData immediateOperation, string cityLabel, string dungeonLabel, string routeLabel)
    {
        if (string.Equals(triggerKey, "dispatch_run", StringComparison.OrdinalIgnoreCase))
        {
            return cityLabel + " -> " + dungeonLabel + " | " + routeLabel;
        }

        if (string.Equals(triggerKey, "recruit", StringComparison.OrdinalIgnoreCase))
        {
            return "Recruit " + cityLabel;
        }

        if (string.Equals(triggerKey, "run_day", StringComparison.OrdinalIgnoreCase))
        {
            return "Advance 1 day";
        }

        if (immediateOperation != null && immediateOperation != PrototypeWorldSimQueuedOperationData.Empty)
        {
            return immediateOperation.OperationLabel;
        }

        if (chain != null && chain.HasSelection)
        {
            return chain.PrimaryActionLabel + " | " + chain.SelectedEntityLabel;
        }

        return "World update";
    }

    private string BuildOutcomeBaselineReasonText(string triggerKey, PrototypeWorldSimOperationChainSurfaceData chain, PrototypeWorldSimQueuedOperationData immediateOperation, PrototypeWorldSimQueuedOperationData soonOperation)
    {
        if (string.Equals(triggerKey, "run_day", StringComparison.OrdinalIgnoreCase))
        {
            if (immediateOperation != null && immediateOperation != PrototypeWorldSimQueuedOperationData.Empty)
            {
                return CompactSurfaceText(immediateOperation.ReasonText, 112);
            }

            if (soonOperation != null && soonOperation != PrototypeWorldSimQueuedOperationData.Empty)
            {
                return CompactSurfaceText(soonOperation.WaitReasonText, 112);
            }
        }

        if (HasMeaningfulSurfaceText(_currentCommitReasonSummaryText))
        {
            return CompactSurfaceText(_currentCommitReasonSummaryText, 112);
        }

        if (chain != null && chain.HasSelection && HasMeaningfulSurfaceText(chain.PrimaryActionReasonText))
        {
            return CompactSurfaceText(chain.PrimaryActionReasonText, 112);
        }

        return HasMeaningfulSurfaceText(_currentActionReasonSummaryText)
            ? CompactSurfaceText(_currentActionReasonSummaryText, 112)
            : "None";
    }

    private string BuildOutcomeBaselineExpectedText(string triggerKey, string cityId, string dungeonId, string routeId, PrototypeWorldSimQueuedOperationData immediateOperation, PrototypeWorldSimQueuedOperationData soonOperation)
    {
        if (!string.IsNullOrEmpty(cityId) && !string.IsNullOrEmpty(dungeonId) && !string.IsNullOrEmpty(routeId))
        {
            string impactText = CompactSurfaceText(BuildExpectedNeedImpactText(cityId, dungeonId, routeId), 58);
            string previewText = CompactSurfaceText(BuildRoutePreviewSummaryText(dungeonId, routeId), 76);
            if (string.Equals(triggerKey, "recruit", StringComparison.OrdinalIgnoreCase))
            {
                return "Recruit opens " + previewText + " | Impact " + impactText;
            }

            if (string.Equals(triggerKey, "dispatch_run", StringComparison.OrdinalIgnoreCase))
            {
                return "Expected " + previewText + " | Impact " + impactText;
            }

            return previewText + " | Impact " + impactText;
        }

        if (string.Equals(triggerKey, "run_day", StringComparison.OrdinalIgnoreCase))
        {
            List<string> parts = new List<string>(2);
            if (HasMeaningfulSurfaceText(_currentRecoveryForecastSummaryText))
            {
                parts.Add("Forecast " + CompactSurfaceText(_currentRecoveryForecastSummaryText, 64));
            }

            if (soonOperation != null && soonOperation != PrototypeWorldSimQueuedOperationData.Empty)
            {
                parts.Add("Soon " + CompactSurfaceText(soonOperation.OperationLabel, 44));
            }

            if (parts.Count > 0)
            {
                return string.Join(" | ", parts.ToArray());
            }
        }

        return HasMeaningfulSurfaceText(_currentConsequencePreviewSummaryText)
            ? CompactSurfaceText(_currentConsequencePreviewSummaryText.Replace("\n", " | "), 112)
            : "None";
    }

    private string BuildOutcomeBaselineRoutePlanSummary(string cityId, string dungeonId, string routeId)
    {
        if (!string.IsNullOrEmpty(cityId) && !string.IsNullOrEmpty(dungeonId))
        {
            string recommendedRouteId = GetRecommendedRouteId(cityId, dungeonId);
            string recommendedLabel = ResolveDungeonRouteLabel(dungeonId, recommendedRouteId);
            string chosenLabel = ResolveDungeonRouteLabel(dungeonId, routeId);
            if (!string.IsNullOrEmpty(routeId) && routeId != recommendedRouteId)
            {
                return "Chosen " + chosenLabel + " | Recommended " + recommendedLabel;
            }

            return "Route " + recommendedLabel;
        }

        return HasMeaningfulSurfaceText(_currentRouteFocusSummaryText)
            ? CompactSurfaceText(_currentRouteFocusSummaryText.Replace("\n", " | "), 112)
            : "None";
    }

    private string BuildOutcomeActualHeadline(PrototypeWorldSimOutcomeBaselineData baseline, PrototypeWorldSimOutcomeCityChangeData focusChange, PrototypeWorldSimQueuedOperationData immediateOperation)
    {
        if (baseline == null || !baseline.HasBaseline)
        {
            return "None";
        }

        if (string.Equals(baseline.TriggerKey, "dispatch_run", StringComparison.OrdinalIgnoreCase))
        {
            if (HasLatestReturnAftermath() && (MatchesLatestReturnAftermathEntity(baseline.CityId) || MatchesLatestReturnAftermathEntity(baseline.DungeonId)))
            {
                return CompactSurfaceText(_latestReturnAftermathSummaryText.Replace("\n", " | "), 112);
            }

            string dispatchImpactText = !string.IsNullOrEmpty(baseline.CityId) ? GetLastDispatchImpactText(baseline.CityId) : "None";
            if (HasMeaningfulSurfaceText(dispatchImpactText))
            {
                return baseline.CityLabel + " | " + CompactSurfaceText(dispatchImpactText, 92);
            }
        }

        if (string.Equals(baseline.TriggerKey, "recruit", StringComparison.OrdinalIgnoreCase))
        {
            int idlePartyCount = _runtimeEconomyState != null && !string.IsNullOrEmpty(baseline.CityId)
                ? _runtimeEconomyState.GetIdlePartyCountInCity(baseline.CityId)
                : 0;
            bool laneOpened = !string.IsNullOrEmpty(baseline.CityId) && ResolveCanEnterSelectedCityDungeon(baseline.CityId);
            return baseline.CityLabel + " staged " + idlePartyCount + " idle part" + (idlePartyCount == 1 ? "y" : "ies") + ". " + (laneOpened ? "Dispatch lane opened." : "Route review reopened.");
        }

        if (string.Equals(baseline.TriggerKey, "run_day", StringComparison.OrdinalIgnoreCase))
        {
            if (HasMeaningfulSurfaceText(_currentNetworkPressureSurface.RecentShiftText))
            {
                return CompactSurfaceText(_currentNetworkPressureSurface.RecentShiftText, 112);
            }

            if (HasMeaningfulSurfaceText(RecentDayLog1Text))
            {
                return CompactSurfaceText(RecentDayLog1Text, 112);
            }
        }

        if (focusChange != null && focusChange.HasChange && focusChange.ReliefScore > 0)
        {
            return focusChange.CityLabel + " eased into " + ResolvePressureStateLabel(focusChange.CurrentStateKey) + ".";
        }

        if (HasMeaningfulSurfaceText(_currentReturnAftermathSummaryText))
        {
            return CompactSurfaceText(_currentReturnAftermathSummaryText.Replace("\n", " | "), 112);
        }

        if (immediateOperation != null && immediateOperation != PrototypeWorldSimQueuedOperationData.Empty)
        {
            return "Board now points to " + CompactSurfaceText(immediateOperation.OperationLabel, 84) + ".";
        }

        return HasMeaningfulSurfaceText(_currentRecentEventFeedSummaryText)
            ? CompactSurfaceText(_currentRecentEventFeedSummaryText.Replace("\n", " | "), 112)
            : "Outcome still resolving.";
    }

    private string BuildOutcomeActualDeltaText(PrototypeWorldSimOutcomeBaselineData baseline, PrototypeWorldSimOutcomeCityChangeData focusChange, PrototypeWorldSimQueuedOperationData immediateOperation)
    {
        List<string> parts = new List<string>(5);
        if (baseline == null || !baseline.HasBaseline)
        {
            return "None";
        }

        if (focusChange != null && focusChange.HasChange)
        {
            if (!string.Equals(baseline.NeedPressureText, focusChange.CurrentNeedPressureText, StringComparison.OrdinalIgnoreCase) && HasMeaningfulSurfaceText(baseline.NeedPressureText) && HasMeaningfulSurfaceText(focusChange.CurrentNeedPressureText))
            {
                parts.Add("Need " + baseline.NeedPressureText + " -> " + focusChange.CurrentNeedPressureText);
            }

            if (baseline.RecoveryDays != focusChange.CurrentRecoveryDays)
            {
                parts.Add("ETA " + ResolveRecoveryWindowLabel(baseline.RecoveryDays) + " -> " + ResolveRecoveryWindowLabel(focusChange.CurrentRecoveryDays));
            }
        }

        if (string.Equals(baseline.TriggerKey, "dispatch_run", StringComparison.OrdinalIgnoreCase) && !string.IsNullOrEmpty(baseline.CityId))
        {
            string needChangeText = GetLastNeedPressureChangeText(baseline.CityId);
            if (HasMeaningfulSurfaceText(needChangeText) && needChangeText != "None -> None")
            {
                parts.Add("Pressure " + CompactSurfaceText(needChangeText, 48));
            }

            string readinessChangeText = GetLastDispatchReadinessChangeText(baseline.CityId);
            if (HasMeaningfulSurfaceText(readinessChangeText) && readinessChangeText != "None -> None")
            {
                parts.Add("Readiness " + CompactSurfaceText(readinessChangeText, 52));
            }
        }

        string currentRoutePlanSummary = BuildOutcomeCurrentRoutePlanSummary(baseline.CityId, baseline.DungeonId, baseline.RouteId);
        if (HasMeaningfulSurfaceText(currentRoutePlanSummary) && !string.Equals(currentRoutePlanSummary, baseline.RoutePlanSummaryText, StringComparison.OrdinalIgnoreCase))
        {
            parts.Add(CompactSurfaceText(currentRoutePlanSummary, 88));
        }

        if (HasMeaningfulSurfaceText(baseline.NetworkRouteSummaryText) && HasMeaningfulSurfaceText(_currentRouteSaturationSummaryText) && !string.Equals(baseline.NetworkRouteSummaryText, _currentRouteSaturationSummaryText, StringComparison.OrdinalIgnoreCase))
        {
            parts.Add("Route lane " + CompactSurfaceText(_currentRouteSaturationSummaryText, 56));
        }

        if (baseline.BestMoveWasBlocked && immediateOperation != null && immediateOperation != PrototypeWorldSimQueuedOperationData.Empty && !immediateOperation.IsBlocked)
        {
            parts.Add("Immediate lane reopened");
        }
        else if (!baseline.BestMoveWasBlocked && immediateOperation != null && immediateOperation != PrototypeWorldSimQueuedOperationData.Empty && immediateOperation.IsBlocked)
        {
            parts.Add("Immediate lane stalled");
        }

        if (parts.Count <= 0 && HasMeaningfulSurfaceText(_currentNetworkPressureSurface.RecentShiftText))
        {
            parts.Add(CompactSurfaceText(_currentNetworkPressureSurface.RecentShiftText, 112));
        }

        return parts.Count > 0 ? string.Join(" | ", parts.ToArray()) : "No major delta yet.";
    }
}
public sealed partial class StaticPlaceholderWorldView
{
    private string BuildChangedBestMoveSummary(PrototypeWorldSimOutcomeBaselineData baseline, PrototypeWorldSimQueuedOperationData immediateOperation, PrototypeWorldSimQueuedOperationData soonOperation, PrototypeWorldSimQueuedOperationData deferredOperation)
    {
        if (immediateOperation == null || immediateOperation == PrototypeWorldSimQueuedOperationData.Empty)
        {
            return "No best move is exposed right now.";
        }

        if (baseline == null || !baseline.HasBaseline || string.IsNullOrEmpty(baseline.BestMoveCandidateId))
        {
            return "Best move now: " + CompactSurfaceText(immediateOperation.OperationLabel, 80) + ".";
        }

        if (string.Equals(baseline.BestMoveCandidateId, immediateOperation.CandidateId, StringComparison.OrdinalIgnoreCase))
        {
            if (baseline.BestMoveWasBlocked != immediateOperation.IsBlocked)
            {
                return baseline.BestMoveWasBlocked
                    ? "Best move unlocked: " + CompactSurfaceText(immediateOperation.OperationLabel, 80) + "."
                    : "Best move tightened but still holds: " + CompactSurfaceText(immediateOperation.OperationLabel, 76) + ".";
            }

            return "Best move held: " + CompactSurfaceText(immediateOperation.OperationLabel, 86) + ".";
        }

        if (soonOperation != null && soonOperation != PrototypeWorldSimQueuedOperationData.Empty && string.Equals(baseline.BestMoveCandidateId, soonOperation.CandidateId, StringComparison.OrdinalIgnoreCase))
        {
            return "Best move drifted: " + CompactSurfaceText(baseline.BestMoveLabel, 34) + " moved to soon. " + CompactSurfaceText(immediateOperation.OperationLabel, 42) + " is best now.";
        }

        if (deferredOperation != null && deferredOperation != PrototypeWorldSimQueuedOperationData.Empty && string.Equals(baseline.BestMoveCandidateId, deferredOperation.CandidateId, StringComparison.OrdinalIgnoreCase))
        {
            return "Best move drifted: " + CompactSurfaceText(baseline.BestMoveLabel, 34) + " dropped to deferred. " + CompactSurfaceText(immediateOperation.OperationLabel, 42) + " is best now.";
        }

        return "Best move changed: " + CompactSurfaceText(baseline.BestMoveLabel, 34) + " -> " + CompactSurfaceText(immediateOperation.OperationLabel, 44) + ".";
    }

    private string BuildRelievedHotspotSummary(PrototypeWorldSimOutcomeBaselineData baseline, PrototypeWorldSimOutcomeCityChangeData focusChange, PrototypeWorldSimOutcomeCityChangeData relievedChange)
    {
        PrototypeWorldSimOutcomeCityChangeData source = relievedChange != null && relievedChange != PrototypeWorldSimOutcomeCityChangeData.Empty
            ? relievedChange
            : focusChange != null && focusChange.ReliefScore > 0
                ? focusChange
                : PrototypeWorldSimOutcomeCityChangeData.Empty;
        if (source == null || source == PrototypeWorldSimOutcomeCityChangeData.Empty || source.ReliefScore <= 0)
        {
            return "No hotspot eased beyond the baseline yet.";
        }

        List<string> parts = new List<string>(3)
        {
            source.CityLabel + " eased"
        };
        if (!string.Equals(source.BaselineNeedPressureText, source.CurrentNeedPressureText, StringComparison.OrdinalIgnoreCase) && HasMeaningfulSurfaceText(source.CurrentNeedPressureText))
        {
            parts.Add("Need " + source.BaselineNeedPressureText + " -> " + source.CurrentNeedPressureText);
        }

        if (source.BaselineRecoveryDays != source.CurrentRecoveryDays)
        {
            parts.Add("ETA " + ResolveRecoveryWindowLabel(source.BaselineRecoveryDays) + " -> " + ResolveRecoveryWindowLabel(source.CurrentRecoveryDays));
        }

        return string.Join(" | ", parts.ToArray());
    }

    private string BuildReboundHotspotSummary(PrototypeWorldSimOutcomeBaselineData baseline, PrototypeWorldSimOutcomeCityChangeData reboundChange)
    {
        if (reboundChange == null || reboundChange == PrototypeWorldSimOutcomeCityChangeData.Empty || reboundChange.ReboundScore <= 0)
        {
            if (baseline != null && baseline.HasBaseline && HasMeaningfulSurfaceText(baseline.BlockedHotspotLabel) && HasMeaningfulSurfaceText(_currentBlockedReasonSummaryText) && _currentBlockedReasonSummaryText != "No major block is exposed right now.")
            {
                return "No new rebound. Current block: " + CompactSurfaceText(_currentBlockedReasonSummaryText, 84);
            }

            return "No new rebound hotspot is visible.";
        }

        return reboundChange.CityLabel + " rebounded into " + ResolvePressureStateLabel(reboundChange.CurrentStateKey) + " | Need " + reboundChange.BaselineNeedPressureText + " -> " + reboundChange.CurrentNeedPressureText;
    }

    private string BuildCorrectiveFollowUpSummary(PrototypeWorldSimQueuedOperationData immediateOperation, PrototypeWorldSimQueuedOperationData soonOperation, PrototypeWorldSimQueuedOperationData deferredOperation)
    {
        if (immediateOperation != null && immediateOperation != PrototypeWorldSimQueuedOperationData.Empty && !immediateOperation.IsBlocked)
        {
            return "Now: " + CompactSurfaceText(immediateOperation.OperationLabel, 42) + " | " + CompactSurfaceText(ResolveQueuedOperationReasonText(immediateOperation), 66);
        }

        if (soonOperation != null && soonOperation != PrototypeWorldSimQueuedOperationData.Empty)
        {
            return "Soon: " + CompactSurfaceText(soonOperation.OperationLabel, 42) + " | " + CompactSurfaceText(ResolveQueuedOperationReasonText(soonOperation), 66);
        }

        if (deferredOperation != null && deferredOperation != PrototypeWorldSimQueuedOperationData.Empty)
        {
            return "Hold: " + CompactSurfaceText(deferredOperation.OperationLabel, 42) + " | " + CompactSurfaceText(ResolveQueuedOperationReasonText(deferredOperation), 66);
        }

        return HasMeaningfulSurfaceText(_currentCommitReasonSummaryText)
            ? CompactSurfaceText(_currentCommitReasonSummaryText, 112)
            : "No corrective follow-up is visible.";
    }

    private string ResolveOutcomeConfirmationStateKey(PrototypeWorldSimOutcomeBaselineData baseline, PrototypeWorldSimOutcomeCityChangeData focusChange, PrototypeWorldSimOutcomeCityChangeData relievedChange, PrototypeWorldSimOutcomeCityChangeData reboundChange, PrototypeWorldSimQueuedOperationData immediateOperation, PrototypeWorldSimQueuedOperationData soonOperation)
    {
        bool bestMoveChanged = baseline != null && baseline.HasBaseline && immediateOperation != null && immediateOperation != PrototypeWorldSimQueuedOperationData.Empty &&
                               !string.IsNullOrEmpty(baseline.BestMoveCandidateId) && !string.Equals(baseline.BestMoveCandidateId, immediateOperation.CandidateId, StringComparison.OrdinalIgnoreCase);
        bool hasRelief = (relievedChange != null && relievedChange.ReliefScore > 0) || (focusChange != null && focusChange.ReliefScore > 0);
        bool strongRelief = focusChange != null && focusChange.ReliefScore > 0 && focusChange.CurrentSeverity == 0 && focusChange.CurrentRecoveryDays == 0;
        bool hasRebound = reboundChange != null && reboundChange.ReboundScore > 0;
        bool followUpOpen = immediateOperation != null && immediateOperation != PrototypeWorldSimQueuedOperationData.Empty && !immediateOperation.IsBlocked;
        bool stillBlocked = (immediateOperation != null && immediateOperation != PrototypeWorldSimQueuedOperationData.Empty && immediateOperation.IsBlocked) ||
                            (soonOperation == null || soonOperation == PrototypeWorldSimQueuedOperationData.Empty) && HasMeaningfulSurfaceText(_currentBlockedReasonSummaryText) && _currentBlockedReasonSummaryText != "No major block is exposed right now.";

        if (hasRebound && reboundChange.CurrentSeverity >= 3)
        {
            return "rebound_pressure";
        }

        if (bestMoveChanged)
        {
            return "drifted_priority";
        }

        if (strongRelief)
        {
            return "confirmed_relief";
        }

        if (hasRelief)
        {
            return "partial_relief";
        }

        if (stillBlocked)
        {
            return "still_blocked";
        }

        return followUpOpen ? "follow_up_now" : "still_blocked";
    }

    private string BuildOutcomeConfirmationSummary(string confirmationStateKey, string actualDeltaText, string changedBestMoveSummaryText, string relievedHotspotSummaryText, string reboundHotspotSummaryText, string correctiveFollowUpSummaryText)
    {
        if (string.Equals(confirmationStateKey, "confirmed_relief", StringComparison.OrdinalIgnoreCase))
        {
            return "Confirmed relief. " + CompactSurfaceText(relievedHotspotSummaryText, 96) + " Next: " + CompactSurfaceText(correctiveFollowUpSummaryText, 72);
        }

        if (string.Equals(confirmationStateKey, "partial_relief", StringComparison.OrdinalIgnoreCase))
        {
            return "Partial relief. " + CompactSurfaceText(actualDeltaText, 88) + " Next: " + CompactSurfaceText(correctiveFollowUpSummaryText, 68);
        }

        if (string.Equals(confirmationStateKey, "drifted_priority", StringComparison.OrdinalIgnoreCase))
        {
            return "Priority drifted. " + CompactSurfaceText(changedBestMoveSummaryText, 92);
        }

        if (string.Equals(confirmationStateKey, "rebound_pressure", StringComparison.OrdinalIgnoreCase))
        {
            return "Rebound pressure. " + CompactSurfaceText(reboundHotspotSummaryText, 88) + " Follow-up: " + CompactSurfaceText(correctiveFollowUpSummaryText, 62);
        }

        if (string.Equals(confirmationStateKey, "still_blocked", StringComparison.OrdinalIgnoreCase))
        {
            return "Still blocked. " + CompactSurfaceText(_currentBlockedReasonSummaryText, 92);
        }

        return "Follow-up now. " + CompactSurfaceText(correctiveFollowUpSummaryText, 96);
    }

    private string BuildOutcomeReadbackSummary(PrototypeWorldSimOutcomeBaselineData baseline, string actualHeadline, string actualDeltaText)
    {
        if (baseline == null || !baseline.HasBaseline)
        {
            return "None";
        }

        return "Baseline: " + CompactSurfaceText(baseline.OperationLabel + " | " + baseline.ExpectedOutcomeText, 112) + "\n" +
               "Actual: " + CompactSurfaceText(actualHeadline, 112) + "\n" +
               "Delta: " + CompactSurfaceText(actualDeltaText, 112);
    }

    private string BuildOutcomeCurrentRoutePlanSummary(string cityId, string dungeonId, string baselineRouteId)
    {
        if (string.IsNullOrEmpty(cityId) || string.IsNullOrEmpty(dungeonId))
        {
            return HasMeaningfulSurfaceText(_currentRouteFocusSummaryText)
                ? CompactSurfaceText(_currentRouteFocusSummaryText.Replace("\n", " | "), 112)
                : "None";
        }

        string recommendedRouteId = GetRecommendedRouteId(cityId, dungeonId);
        string recommendedLabel = ResolveDungeonRouteLabel(dungeonId, recommendedRouteId);
        if (!string.IsNullOrEmpty(baselineRouteId) && !string.Equals(recommendedRouteId, baselineRouteId, StringComparison.OrdinalIgnoreCase))
        {
            return "Recommended route shifted to " + recommendedLabel;
        }

        return "Recommended route holds on " + recommendedLabel;
    }

    private string ResolveQueuedOperationReasonText(PrototypeWorldSimQueuedOperationData operation)
    {
        if (operation == null || operation == PrototypeWorldSimQueuedOperationData.Empty)
        {
            return "None";
        }

        string preferred = operation.IsBlocked ? operation.WaitReasonText : operation.ReasonText;
        if (!HasMeaningfulSurfaceText(preferred))
        {
            preferred = operation.IsBlocked ? operation.ReasonText : operation.WaitReasonText;
        }

        return preferred;
    }

    private string ResolveEntityLabel(string entityId)
    {
        if (string.IsNullOrEmpty(entityId))
        {
            return "None";
        }

        WorldEntityData entity = FindEntity(entityId);
        return entity != null ? entity.DisplayName : entityId;
    }

    private string ResolveDungeonRouteLabel(string dungeonId, string routeId)
    {
        if (string.IsNullOrEmpty(routeId))
        {
            return "None";
        }

        DungeonRouteTemplate template = !string.IsNullOrEmpty(dungeonId) ? GetRouteTemplateById(dungeonId, routeId) : null;
        return template != null ? template.RouteLabel : routeId;
    }

    private string ResolveRecoveryWindowLabel(int recoveryDays)
    {
        return recoveryDays > 0 ? BuildRecoveryEtaText(recoveryDays) : "ready now";
    }

    private string ResolvePressureStateLabel(string stateKey)
    {
        return string.Equals(stateKey, "critical", StringComparison.OrdinalIgnoreCase)
            ? "critical pressure"
            : string.Equals(stateKey, "strained", StringComparison.OrdinalIgnoreCase)
                ? "strained pressure"
                : string.Equals(stateKey, "recovering", StringComparison.OrdinalIgnoreCase)
                    ? "recovering pressure"
                    : string.Equals(stateKey, "watch", StringComparison.OrdinalIgnoreCase)
                        ? "watch pressure"
                        : "stable pressure";
    }

    private void ApplyWorldOutcomeReadbackPulseState()
    {
        if (_currentOutcomeReadbackSurface == null || _currentOutcomeReadbackSurface == PrototypeWorldSimOutcomeReadbackSurfaceData.Empty)
        {
            return;
        }

        ApplyOutcomeReliefPulse(_currentOutcomeReadbackSurface.RelievedCityId);
        ApplyOutcomeReboundPulse(_currentOutcomeReadbackSurface.ReboundCityId);
    }

    private void ApplyOutcomeReliefPulse(string cityId)
    {
        if (string.IsNullOrEmpty(cityId))
        {
            return;
        }

        if (_markerByEntityId.TryGetValue(cityId, out WorldSelectableMarker cityMarker) && cityMarker != null)
        {
            cityMarker.SetOutcomePinned(true);
        }

        string dungeonId = GetLinkedDungeonIdForCity(cityId);
        if (!string.IsNullOrEmpty(dungeonId) && _markerByEntityId.TryGetValue(dungeonId, out WorldSelectableMarker dungeonMarker) && dungeonMarker != null)
        {
            dungeonMarker.SetOutcomePinned(true);
        }

        string connectorKey = BuildLinkedConnectorKey(cityId, dungeonId);
        if (!string.IsNullOrEmpty(connectorKey) && _linkedConnectorByKey.TryGetValue(connectorKey, out WorldBoardConnectorPulseVisual connectorVisual) && connectorVisual != null)
        {
            connectorVisual.ApplyPulse(false, true);
        }
    }

    private void ApplyOutcomeReboundPulse(string cityId)
    {
        if (string.IsNullOrEmpty(cityId))
        {
            return;
        }

        if (_cityPressureByEntityId.TryGetValue(cityId, out PrototypeWorldSimCityPressureSurfaceData cityPressure) && cityPressure != null && _markerByEntityId.TryGetValue(cityId, out WorldSelectableMarker cityMarker) && cityMarker != null)
        {
            cityMarker.SetPressureState(cityPressure.StateKey, false);
            cityMarker.SetLinkedFocus(true);
        }

        string dungeonId = GetLinkedDungeonIdForCity(cityId);
        if (!string.IsNullOrEmpty(dungeonId) && _markerByEntityId.TryGetValue(dungeonId, out WorldSelectableMarker dungeonMarker) && dungeonMarker != null)
        {
            dungeonMarker.SetLinkedFocus(true);
        }

        string connectorKey = BuildLinkedConnectorKey(cityId, dungeonId);
        if (!string.IsNullOrEmpty(connectorKey) && _linkedConnectorByKey.TryGetValue(connectorKey, out WorldBoardConnectorPulseVisual connectorVisual) && connectorVisual != null)
        {
            connectorVisual.ApplyPulse(true, false);
        }
    }
}
