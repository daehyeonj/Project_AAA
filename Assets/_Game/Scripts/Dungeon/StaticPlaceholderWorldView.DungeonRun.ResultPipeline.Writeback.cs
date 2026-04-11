public sealed partial class StaticPlaceholderWorldView
{
    private ExpeditionResult BuildDungeonRunExpeditionResult(PostRunResolutionInput handoffInput)
    {
        return ResultPipeline.BuildExpeditionResult(handoffInput);
    }

    private void ApplyDungeonRunResultPipelineWriteback(PostRunResolutionInput handoffInput)
    {
        PostRunResolutionInput safeHandoffInput = handoffInput ?? new PostRunResolutionInput();
        int safeReturnedLoot = safeHandoffInput.ReturnedLootAmount > 0 ? safeHandoffInput.ReturnedLootAmount : 0;

        if (_runtimeEconomyState != null && !string.IsNullOrEmpty(_currentHomeCityId) && !string.IsNullOrEmpty(_currentDungeonId))
        {
            ExpeditionResult expeditionResult = BuildDungeonRunExpeditionResult(safeHandoffInput);
            _runtimeEconomyState.ResolveDungeonRun(expeditionResult);
        }

        _resultStockBefore = _preRunManaShardStock;
        _resultStockAfter = string.IsNullOrEmpty(_currentHomeCityId) ? 0 : GetCityManaShardStock(_currentHomeCityId);
        _resultStockDelta = _resultStockAfter - _resultStockBefore;
        _resultNeedPressureBeforeText = string.IsNullOrEmpty(_preRunNeedPressureText) ? GetNeedPressureTextForStock(_resultStockBefore) : _preRunNeedPressureText;
        _resultNeedPressureAfterText = string.IsNullOrEmpty(_currentHomeCityId) ? "None" : GetNeedPressureTextForStock(_resultStockAfter);
        _resultDispatchReadinessBeforeText = string.IsNullOrEmpty(_preRunDispatchReadinessText) ? GetDispatchReadinessText(_currentHomeCityId) : _preRunDispatchReadinessText;
        _resultDispatchReadinessAfterText = string.IsNullOrEmpty(_currentHomeCityId) ? "None" : GetDispatchReadinessText(_currentHomeCityId);
        _resultConsecutiveDispatchBefore = _preRunConsecutiveDispatchCount;
        _resultConsecutiveDispatchAfter = string.IsNullOrEmpty(_currentHomeCityId) ? 0 : GetConsecutiveDispatchCount(_currentHomeCityId);
        _resultRecoveryEtaDays = string.IsNullOrEmpty(_currentHomeCityId) ? 0 : GetRecoveryDaysToReady(_currentHomeCityId);

        if (string.IsNullOrEmpty(_currentHomeCityId))
        {
            return;
        }

        CityWriteback cityWriteback = BuildDungeonRunResultCityWritebackPreviewSurface();
        OutcomeReadback outcomeReadback = BuildDungeonRunResultOutcomeReadbackPreviewSurface();
        _resultStockBefore = cityWriteback.StockBefore;
        _resultStockAfter = cityWriteback.StockAfter;
        _resultStockDelta = cityWriteback.StockDelta;
        _resultNeedPressureBeforeText = IsMeaningfulSnapshotText(cityWriteback.NeedPressureBeforeText)
            ? cityWriteback.NeedPressureBeforeText
            : _resultNeedPressureBeforeText;
        _resultNeedPressureAfterText = IsMeaningfulSnapshotText(cityWriteback.NeedPressureAfterText)
            ? cityWriteback.NeedPressureAfterText
            : _resultNeedPressureAfterText;
        _resultDispatchReadinessBeforeText = IsMeaningfulSnapshotText(cityWriteback.DispatchReadinessBeforeText)
            ? cityWriteback.DispatchReadinessBeforeText
            : _resultDispatchReadinessBeforeText;
        _resultDispatchReadinessAfterText = IsMeaningfulSnapshotText(cityWriteback.DispatchReadinessAfterText)
            ? cityWriteback.DispatchReadinessAfterText
            : _resultDispatchReadinessAfterText;
        _lastDispatchStockDeltaByCityId[_currentHomeCityId] = IsMeaningfulSnapshotText(outcomeReadback.StockDeltaText)
            ? outcomeReadback.StockDeltaText
            : BuildSignedLootAmountText(cityWriteback.StockDelta);
        _lastNeedPressureChangeByCityId[_currentHomeCityId] = BuildNeedPressureChangeSummary(
            cityWriteback.NeedPressureBeforeText,
            cityWriteback.NeedPressureAfterText);
        _lastDispatchReadinessChangeByCityId[_currentHomeCityId] = BuildDispatchReadinessChangeSummary(
            cityWriteback.DispatchReadinessBeforeText,
            cityWriteback.DispatchReadinessAfterText) +
            " | Streak " + _resultConsecutiveDispatchBefore + " -> " + _resultConsecutiveDispatchAfter;
        _lastDispatchImpactByCityId[_currentHomeCityId] = IsMeaningfulSnapshotText(outcomeReadback.LatestReturnAftermathText)
            ? outcomeReadback.LatestReturnAftermathText
            : BuildLastDispatchImpactSummary(
                cityWriteback.StockBefore,
                cityWriteback.StockAfter,
                cityWriteback.StockDelta,
                cityWriteback.NeedPressureBeforeText,
                cityWriteback.NeedPressureAfterText);
    }

    private CityWriteback BuildDungeonRunResultCityWritebackPreviewSurface()
    {
        string cityId = _currentHomeCityId;
        string dungeonId = _currentDungeonId;
        string nextSuggestedActionText = ResolveOutcomeReadbackNextSuggestedActionText(null, cityId, dungeonId);
        return BuildCityWritebackSurfaceForCity(cityId, nextSuggestedActionText);
    }

    private OutcomeReadback BuildDungeonRunResultOutcomeReadbackPreviewSurface()
    {
        string cityId = _currentHomeCityId;
        string dungeonId = _currentDungeonId;
        string cityLabel = string.IsNullOrEmpty(cityId) ? "None" : ResolveDispatchEntityDisplayName(cityId);
        string nextSuggestedActionText = ResolveOutcomeReadbackNextSuggestedActionText(null, cityId, dungeonId);
        string latestReturnAftermathText = ResolveOutcomeReadbackLatestReturnAftermathText(null, cityId, dungeonId);
        CityWriteback cityWriteback = BuildCityWritebackSurfaceForCity(cityId, nextSuggestedActionText);
        return BuildOutcomeReadbackSurfaceForContext(
            cityId,
            dungeonId,
            cityLabel,
            string.IsNullOrEmpty(cityId)
                ? "World return preview unavailable."
                : cityLabel + " return aftermath is ready for the world board.",
            nextSuggestedActionText,
            latestReturnAftermathText,
            cityWriteback);
    }
}
