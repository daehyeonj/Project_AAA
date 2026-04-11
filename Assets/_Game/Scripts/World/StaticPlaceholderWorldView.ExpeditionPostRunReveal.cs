public sealed partial class StaticPlaceholderWorldView
{
    private string _latestExpeditionPostRunRevealToken = string.Empty;
    private string _acknowledgedExpeditionPostRunRevealToken = string.Empty;

    public void AcknowledgePendingExpeditionPostRunReveal()
    {
        ExpeditionPostRunRevealState reveal = BuildLatestExpeditionPostRunRevealState();
        if (reveal.HasPendingReveal && !string.IsNullOrEmpty(reveal.RevealToken))
        {
            _acknowledgedExpeditionPostRunRevealToken = reveal.RevealToken;
        }
    }

    private void PreparePendingExpeditionPostRunRevealForWorldReturn()
    {
        ExpeditionPostRunRevealState reveal = BuildLatestExpeditionPostRunRevealState();
        if (!reveal.HasPendingReveal)
        {
            return;
        }

        FocusExpeditionPostRunRevealTarget(reveal);
    }

    private bool FocusPendingExpeditionPostRunRevealTarget()
    {
        ExpeditionPostRunRevealState reveal = BuildLatestExpeditionPostRunRevealState();
        return reveal.HasPendingReveal && FocusExpeditionPostRunRevealTarget(reveal);
    }

    private bool FocusExpeditionPostRunRevealTarget(ExpeditionPostRunRevealState reveal)
    {
        if (reveal == null)
        {
            return false;
        }

        string targetEntityId = !string.IsNullOrEmpty(reveal.CityId)
            ? reveal.CityId
            : reveal.DungeonId;
        return TrySelectWorldEntityById(targetEntityId);
    }

    private bool TrySelectWorldEntityById(string entityId)
    {
        if (string.IsNullOrEmpty(entityId) ||
            !_markersByEntityId.TryGetValue(entityId, out WorldSelectableMarker marker) ||
            marker == null)
        {
            return false;
        }

        SetSelected(marker);
        return true;
    }

    private ExpeditionPostRunRevealState BuildLatestExpeditionPostRunRevealState()
    {
        WorldWriteback latestWorldWriteback = _runtimeEconomyState != null
            ? _runtimeEconomyState.GetLatestWorldWriteback()
            : new WorldWriteback();
        if (latestWorldWriteback == null || !IsMeaningfulSnapshotText(latestWorldWriteback.RunResultStateKey))
        {
            _latestExpeditionPostRunRevealToken = string.Empty;
            return new ExpeditionPostRunRevealState();
        }

        string cityId = latestWorldWriteback.SourceCityId ?? string.Empty;
        ExpeditionResult latestExpeditionResult = _runtimeEconomyState != null && !string.IsNullOrEmpty(cityId)
            ? _runtimeEconomyState.GetLatestExpeditionResultForCity(cityId)
            : _runtimeEconomyState != null
                ? _runtimeEconomyState.GetLatestExpeditionResult()
                : null;
        if (string.IsNullOrEmpty(cityId) && latestExpeditionResult != null && !string.IsNullOrEmpty(latestExpeditionResult.SourceCityId))
        {
            cityId = latestExpeditionResult.SourceCityId;
        }

        string dungeonId = !string.IsNullOrEmpty(latestWorldWriteback.TargetDungeonId)
            ? latestWorldWriteback.TargetDungeonId
            : latestExpeditionResult != null
                ? latestExpeditionResult.DungeonId
                : string.Empty;
        string cityLabel = IsMeaningfulSnapshotText(latestWorldWriteback.SourceCityLabel)
            ? latestWorldWriteback.SourceCityLabel
            : ResolveDispatchEntityDisplayName(cityId);
        string nextSuggestedActionText = ResolveOutcomeReadbackNextSuggestedActionText(null, cityId, dungeonId);
        string latestReturnAftermathText = ResolveOutcomeReadbackLatestReturnAftermathText(null, cityId, dungeonId);
        CityWriteback cityWriteback = BuildCityWritebackSurfaceForCity(cityId, nextSuggestedActionText);
        OutcomeReadback outcomeReadback = BuildOutcomeReadbackSurfaceForContext(
            cityId,
            dungeonId,
            cityLabel,
            string.IsNullOrEmpty(cityId)
                ? "No city selected for post-run reveal."
                : cityLabel + " post-run reveal is ready.",
            nextSuggestedActionText,
            latestReturnAftermathText,
            cityWriteback);
        ExpeditionResult consumedExpeditionResult = ResultPipeline.BuildReturnConsumedExpeditionResult(
            latestExpeditionResult,
            cityWriteback,
            latestWorldWriteback,
            outcomeReadback);
        PrototypeWorldDispatchBriefingSnapshot briefing = BuildDispatchBriefingSnapshot(cityId, dungeonId, string.Empty, false);
        bool canOpenExpeditionPrep = briefing != null && briefing.CanOpenPlanner;
        return BuildExpeditionPostRunRevealState(
            cityId,
            cityLabel,
            dungeonId,
            consumedExpeditionResult,
            cityWriteback,
            latestWorldWriteback,
            outcomeReadback,
            canOpenExpeditionPrep);
    }

    private ExpeditionPostRunRevealState BuildExpeditionPostRunRevealState(
        string cityId,
        string cityLabel,
        string dungeonId,
        ExpeditionResult latestExpeditionResult,
        CityWriteback cityWriteback,
        WorldWriteback latestWorldWriteback,
        OutcomeReadback outcomeReadback,
        bool canOpenExpeditionPrep)
    {
        ExpeditionPostRunRevealState reveal = new ExpeditionPostRunRevealState();
        ExpeditionResult safeExpeditionResult = latestExpeditionResult ?? new ExpeditionResult();
        CityWriteback safeCityWriteback = cityWriteback ?? new CityWriteback();
        WorldWriteback safeWorldWriteback = latestWorldWriteback ?? new WorldWriteback();
        OutcomeReadback safeOutcomeReadback = outcomeReadback ?? new OutcomeReadback();
        string revealToken = BuildExpeditionPostRunRevealToken(
            safeWorldWriteback,
            safeExpeditionResult,
            safeOutcomeReadback);

        _latestExpeditionPostRunRevealToken = revealToken;
        reveal.RevealToken = revealToken;
        reveal.HasLatestResult = IsMeaningfulSnapshotText(revealToken);
        reveal.HasPendingReveal = reveal.HasLatestResult && revealToken != _acknowledgedExpeditionPostRunRevealToken;
        reveal.CanOpenExpeditionPrep = canOpenExpeditionPrep;
        reveal.ResultStateKey = safeWorldWriteback.RunResultStateKey ?? string.Empty;
        reveal.HeadlineText = BuildExpeditionPostRunRevealHeadlineText(
            safeWorldWriteback.RunResultStateKey,
            cityLabel,
            safeWorldWriteback.TargetDungeonLabel,
            safeExpeditionResult.DungeonLabel);
        reveal.CityId = string.IsNullOrEmpty(cityId) ? safeWorldWriteback.SourceCityId : cityId;
        reveal.CityLabel = ChooseExpeditionPostRunRevealText(cityLabel, safeWorldWriteback.SourceCityLabel);
        reveal.DungeonId = string.IsNullOrEmpty(dungeonId) ? safeWorldWriteback.TargetDungeonId : dungeonId;
        reveal.DungeonLabel = ChooseExpeditionPostRunRevealText(
            safeWorldWriteback.TargetDungeonLabel,
            safeExpeditionResult.DungeonLabel);
        reveal.LatestExpeditionResult = safeExpeditionResult;
        reveal.OutcomeReadback = safeOutcomeReadback;
        reveal.CityWriteback = safeCityWriteback;
        reveal.WorldWriteback = safeWorldWriteback;
        return reveal;
    }

    private string BuildExpeditionPostRunRevealToken(
        WorldWriteback latestWorldWriteback,
        ExpeditionResult latestExpeditionResult,
        OutcomeReadback outcomeReadback)
    {
        if (latestWorldWriteback == null || !IsMeaningfulSnapshotText(latestWorldWriteback.RunResultStateKey))
        {
            return string.Empty;
        }

        string summaryText = ChooseExpeditionPostRunRevealText(
            latestWorldWriteback.WritebackSummaryText,
            ChooseExpeditionPostRunRevealText(
                outcomeReadback != null ? outcomeReadback.PostRunSummaryText : "None",
                latestExpeditionResult != null ? latestExpeditionResult.ResultSummaryText : "None"));
        return (latestWorldWriteback.SourceCityId ?? string.Empty) + "|" +
            (latestWorldWriteback.TargetDungeonId ?? string.Empty) + "|" +
            (latestWorldWriteback.RunResultStateKey ?? string.Empty) + "|" +
            latestWorldWriteback.DayAfter + "|" +
            summaryText;
    }

    private string BuildExpeditionPostRunRevealHeadlineText(
        string resultStateKey,
        string cityLabel,
        string worldDungeonLabel,
        string expeditionDungeonLabel)
    {
        string safeCityLabel = ChooseExpeditionPostRunRevealText(cityLabel, "Selected City");
        string safeDungeonLabel = ChooseExpeditionPostRunRevealText(worldDungeonLabel, expeditionDungeonLabel);
        if (resultStateKey == "clear")
        {
            return "Return Spotlight | " + safeCityLabel + " stabilized " + safeDungeonLabel;
        }

        if (resultStateKey == "defeat")
        {
            return "Return Spotlight | " + safeCityLabel + " needs recovery after " + safeDungeonLabel;
        }

        if (resultStateKey == "retreat")
        {
            return "Return Spotlight | " + safeCityLabel + " regrouped from " + safeDungeonLabel;
        }

        return "Return Spotlight | " + safeCityLabel + " updated from " + safeDungeonLabel;
    }

    private string ChooseExpeditionPostRunRevealText(string primary, string fallback)
    {
        if (IsMeaningfulSnapshotText(primary))
        {
            return primary;
        }

        return IsMeaningfulSnapshotText(fallback) ? fallback : "None";
    }
}
