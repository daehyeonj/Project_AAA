using System.Collections.Generic;

public sealed partial class StaticPlaceholderWorldView
{
    private const int RecentLaunchContextLogLimit = 3;

    private sealed class PrototypeWorldModifierSummary
    {
        public readonly List<string> ModifierKeys = new List<string>();
        public string SummaryText = "None";
        public string BlockedReasonSummaryText = "None";
        public string RecommendationHintText = "None";
    }

    private sealed class PrototypeWorldSnapshotCityStateSummary
    {
        public string CityId = string.Empty;
        public string CityLabel = "None";
        public string NeedPressureText = "None";
        public string DispatchPolicyText = "None";
        public string ReadinessText = "None";
        public string RecoveryText = "None";
        public string StockText = "None";
        public string LinkedDungeonId = string.Empty;
        public string LinkedDungeonLabel = "None";
        public string LastWritebackText = "None";
    }

    private sealed class PrototypeWorldSnapshotDungeonDescriptor
    {
        public string DungeonId = string.Empty;
        public string DungeonLabel = "None";
        public string DangerText = "None";
        public string StatusText = "None";
        public string AvailabilityText = "None";
        public string LastOutcomeText = "None";
        public string RecommendedRouteText = "None";
    }

    private sealed class PrototypeWorldSnapshotPartyStateSummary
    {
        public string PartyId = string.Empty;
        public string PartyLabel = "None";
        public string PartySummaryText = "None";
        public string SupplySummaryText = "None";
        public string ActiveLaneText = "None";
        public string ReturnWindowText = "None";
        public string RecoveryAfterReturnText = "None";
    }

    private sealed class PrototypeWorldSnapshot
    {
        public int WorldDay;
        public string SelectedCityId = string.Empty;
        public string SelectedCityLabel = "None";
        public string SelectedDungeonId = string.Empty;
        public string SelectedDungeonLabel = "None";
        public string SelectedPartyId = string.Empty;
        public string SelectedRouteId = string.Empty;
        public string SelectedRouteLabel = "None";
        public string RecommendedRouteId = string.Empty;
        public string RecommendedRouteLabel = "None";
        public string RecentAftermathEchoText = "None";
        public string SnapshotSummaryText = "None";
        public PrototypeWorldSnapshotCityStateSummary CitySummary = new PrototypeWorldSnapshotCityStateSummary();
        public PrototypeWorldSnapshotDungeonDescriptor DungeonSummary = new PrototypeWorldSnapshotDungeonDescriptor();
        public PrototypeWorldSnapshotPartyStateSummary PartySummary = new PrototypeWorldSnapshotPartyStateSummary();
        public PrototypeWorldModifierSummary ModifierSummary = new PrototypeWorldModifierSummary();
    }

    private enum WorldLaunchRecordType
    {
        LaunchAllowed,
        LaunchBlocked,
        LaunchFailed
    }

    private sealed class WorldLaunchContextLogRecord
    {
        public WorldLaunchRecordType RecordType;
        public int WorldDay;
        public string CityId = string.Empty;
        public string DungeonId = string.Empty;
        public string PartyId = string.Empty;
        public string RouteId = string.Empty;
        public bool Allowed;
        public string ModifierSummaryText = "None";
        public string DisplayText = "None";
    }

    private readonly List<WorldLaunchContextLogRecord> _recentWorldLaunchContextLogs = new List<WorldLaunchContextLogRecord>(RecentLaunchContextLogLimit);

    public string CurrentWorldSnapshotSummaryText => BuildCurrentWorldSnapshot().SnapshotSummaryText;
    public string CurrentExpeditionStartContextSummaryText => BuildProjectedExpeditionStartContext().ContextSummaryText;
    public string CurrentBlockedLaunchReasonSummaryText => BuildCurrentBlockedLaunchReasonSummaryText();
    public string RecentLaunchContextLog1Text => GetRecentLaunchContextLogText(0);
    public string RecentLaunchContextLog2Text => GetRecentLaunchContextLogText(1);
    public string RecentLaunchContextLog3Text => GetRecentLaunchContextLogText(2);

    public WorldObservationSurfaceData BuildWorldObservationSurfaceData(bool includeDetailedPartyReadbacks = false)
    {
        bool includeDetailed = includeDetailedPartyReadbacks || _isExpeditionPrepBoardOpen;
        WorldObservationSurfaceData data = new WorldObservationSurfaceData();
        WorldBoardReadModel board = BuildWorldBoardReadModel();
        string cityId = !string.IsNullOrEmpty(_currentHomeCityId)
            ? _currentHomeCityId
            : ResolveDispatchBriefingCityId();
        string dungeonId = !string.IsNullOrEmpty(_currentDungeonId)
            ? _currentDungeonId
            : ResolveDispatchBriefingDungeonId(cityId);
        string routeId = ResolveWorldSnapshotRouteId(cityId, dungeonId);
        bool requireSelectedRoute = IsExpeditionPrepRouteSelectionActive() && !string.IsNullOrEmpty(routeId);
        PrototypeWorldDispatchBriefingSnapshot briefing = BuildDispatchBriefingSnapshot(
            cityId,
            dungeonId,
            routeId,
            requireSelectedRoute,
            includeDetailed);
        PrototypeWorldSnapshot snapshot = BuildWorldSnapshot(cityId, dungeonId, routeId, briefing);
        ExpeditionStartContext startContext = BuildProjectedExpeditionStartContext(snapshot, briefing, includeDetailed);

        data.CurrentWorldObservationSummaryText = snapshot != null && IsMeaningfulSnapshotText(snapshot.SnapshotSummaryText)
            ? snapshot.SnapshotSummaryText
            : "None";
        data.CurrentExpeditionStartContextSummaryText = startContext != null && IsMeaningfulSnapshotText(startContext.ContextSummaryText)
            ? startContext.ContextSummaryText
            : "None";
        data.AutoTickEnabled = AutoTickEnabled;
        data.WorldDayCount = WorldDayCount;
        data.TradeStepCount = TradeStepCount;
        data.TotalParties = TotalParties;
        data.IdleParties = IdleParties;
        data.ActiveExpeditions = ActiveExpeditions;
        data.CitiesWithShortagesText = IsMeaningfulSnapshotText(CitiesWithShortagesText) ? CitiesWithShortagesText : "None";
        data.CurrentExpeditionStartContext = CopyExpeditionStartContext(startContext);

        PopulateWorldObservationCurrentContext(data.CurrentContext, snapshot);
        PopulateWorldObservationSelectedEntity(data.SelectedEntity, snapshot);
        PopulateWorldObservationPriorityBoard(data.PriorityBoard, board);
        data.ExpeditionPrep = BuildCanonicalExpeditionPrepSurfaceData(snapshot, briefing, startContext, includeDetailed);
        PopulateWorldObservationLaunch(data.Launch, data.ExpeditionPrep, snapshot, briefing, startContext);
        PopulateWorldObservationActiveExpedition(data.ActiveExpedition);
        PopulateWorldObservationRecentOutcome(data.RecentOutcome, snapshot);
        PopulateWorldObservationCityHub(data.CityHub, data, board);
        PopulateWorldObservationLogs(data.Logs);
        return data;
    }

    private PrototypeWorldSnapshot BuildCurrentWorldSnapshot()
    {
        bool includeDetailed = _isExpeditionPrepBoardOpen;
        string cityId = !string.IsNullOrEmpty(_currentHomeCityId)
            ? _currentHomeCityId
            : ResolveDispatchBriefingCityId();
        string dungeonId = !string.IsNullOrEmpty(_currentDungeonId)
            ? _currentDungeonId
            : ResolveDispatchBriefingDungeonId(cityId);
        string routeId = ResolveWorldSnapshotRouteId(cityId, dungeonId);
        PrototypeWorldDispatchBriefingSnapshot briefing = BuildDispatchBriefingSnapshot(
            cityId,
            dungeonId,
            routeId,
            IsExpeditionPrepRouteSelectionActive() && !string.IsNullOrEmpty(routeId),
            includeDetailed);
        return BuildWorldSnapshot(cityId, dungeonId, routeId, briefing);
    }

    private PrototypeWorldSnapshot BuildWorldSnapshot(string cityId, string dungeonId, string selectedRouteId, PrototypeWorldDispatchBriefingSnapshot briefing)
    {
        PrototypeWorldSnapshot snapshot = new PrototypeWorldSnapshot();
        PrototypeWorldActiveExpeditionSnapshot activeSnapshot = BuildActiveExpeditionSnapshot(cityId, dungeonId, false);
        string normalizedRouteId = NormalizeRouteChoiceId(selectedRouteId);
        string routeLabel = BuildDispatchRouteLabel(dungeonId, normalizedRouteId);

        snapshot.WorldDay = _runtimeEconomyState != null ? _runtimeEconomyState.WorldDayCount : 0;
        snapshot.SelectedCityId = string.IsNullOrEmpty(cityId) ? string.Empty : cityId;
        snapshot.SelectedCityLabel = ResolveDispatchEntityDisplayName(cityId);
        snapshot.SelectedDungeonId = string.IsNullOrEmpty(dungeonId) ? string.Empty : dungeonId;
        snapshot.SelectedDungeonLabel = ResolveDispatchEntityDisplayName(dungeonId);
        snapshot.SelectedPartyId = briefing != null ? briefing.PartyId : string.Empty;
        snapshot.SelectedRouteId = string.IsNullOrEmpty(normalizedRouteId) ? string.Empty : normalizedRouteId;
        snapshot.SelectedRouteLabel = IsMeaningfulSnapshotText(routeLabel) ? routeLabel : "None";
        snapshot.RecommendedRouteId = briefing != null ? briefing.RecommendationRouteId : string.Empty;
        snapshot.RecommendedRouteLabel = briefing != null && IsMeaningfulSnapshotText(briefing.RecommendationRouteLabel)
            ? briefing.RecommendationRouteLabel
            : "None";

        snapshot.CitySummary.CityId = snapshot.SelectedCityId;
        snapshot.CitySummary.CityLabel = snapshot.SelectedCityLabel;
        snapshot.CitySummary.NeedPressureText = string.IsNullOrEmpty(cityId) ? "None" : BuildNeedPressureText(cityId);
        snapshot.CitySummary.DispatchPolicyText = string.IsNullOrEmpty(cityId) ? "None" : BuildDispatchPolicyText(GetDispatchPolicyState(cityId));
        snapshot.CitySummary.ReadinessText = string.IsNullOrEmpty(cityId) ? "None" : GetDispatchReadinessText(cityId);
        snapshot.CitySummary.RecoveryText = string.IsNullOrEmpty(cityId) ? "None" : BuildDispatchRecoveryProgressText(cityId);
        snapshot.CitySummary.StockText = string.IsNullOrEmpty(cityId) ? "None" : BuildCityManaShardStockText(cityId);
        snapshot.CitySummary.LinkedDungeonId = string.IsNullOrEmpty(cityId) ? string.Empty : GetLinkedDungeonIdForCity(cityId);
        snapshot.CitySummary.LinkedDungeonLabel = ResolveDispatchEntityDisplayName(snapshot.CitySummary.LinkedDungeonId);
        snapshot.CitySummary.LastWritebackText = _runtimeEconomyState != null && !string.IsNullOrEmpty(cityId)
            ? _runtimeEconomyState.GetLatestWorldWritebackSummaryForCity(cityId)
            : "None";

        snapshot.DungeonSummary.DungeonId = snapshot.SelectedDungeonId;
        snapshot.DungeonSummary.DungeonLabel = snapshot.SelectedDungeonLabel;
        snapshot.DungeonSummary.DangerText = string.IsNullOrEmpty(dungeonId) ? "None" : BuildDungeonDangerSummaryText(dungeonId);
        snapshot.DungeonSummary.StatusText = _runtimeEconomyState != null && !string.IsNullOrEmpty(dungeonId)
            ? _runtimeEconomyState.GetDungeonWorldStatusText(dungeonId)
            : "None";
        snapshot.DungeonSummary.AvailabilityText = _runtimeEconomyState != null && !string.IsNullOrEmpty(dungeonId)
            ? _runtimeEconomyState.GetDungeonWorldAvailabilityText(dungeonId)
            : "None";
        snapshot.DungeonSummary.LastOutcomeText = _runtimeEconomyState != null && !string.IsNullOrEmpty(dungeonId)
            ? _runtimeEconomyState.GetDungeonLastWorldOutcomeText(dungeonId)
            : "None";
        snapshot.DungeonSummary.RecommendedRouteText = snapshot.RecommendedRouteLabel;

        snapshot.PartySummary.PartyId = snapshot.SelectedPartyId;
        snapshot.PartySummary.PartyLabel = briefing != null && IsMeaningfulSnapshotText(briefing.PartyLabel)
            ? briefing.PartyLabel
            : (IsMeaningfulSnapshotText(snapshot.SelectedPartyId) ? snapshot.SelectedPartyId : "None");
        snapshot.PartySummary.PartySummaryText = briefing != null ? briefing.PartySummaryText : "None";
        snapshot.PartySummary.SupplySummaryText = BuildWorldSnapshotSupplySummaryText(cityId, briefing);
        snapshot.PartySummary.ActiveLaneText = activeSnapshot.TravelSummary.ActiveLaneText;
        snapshot.PartySummary.ReturnWindowText = activeSnapshot.ReturnWindow.ReturnWindowLabel;
        snapshot.PartySummary.RecoveryAfterReturnText = activeSnapshot.ReturnWindow.RecoveryAfterReturnLabel;

        snapshot.RecentAftermathEchoText = BuildWorldSnapshotAftermathEchoText(cityId, dungeonId, activeSnapshot);
        snapshot.ModifierSummary = BuildWorldModifierSummary(snapshot, briefing);
        snapshot.SnapshotSummaryText = BuildWorldSnapshotSummaryText(snapshot);
        return snapshot;
    }

    private ExpeditionStartContext BuildProjectedExpeditionStartContext(bool includeDetailedPartyReadbacks = false)
    {
        bool includeDetailed = includeDetailedPartyReadbacks || _isExpeditionPrepBoardOpen;
        string cityId = !string.IsNullOrEmpty(_currentHomeCityId)
            ? _currentHomeCityId
            : ResolveDispatchBriefingCityId();
        string dungeonId = !string.IsNullOrEmpty(_currentDungeonId)
            ? _currentDungeonId
            : ResolveDispatchBriefingDungeonId(cityId);
        string routeId = ResolveWorldSnapshotRouteId(cityId, dungeonId);
        bool requireSelectedRoute = IsExpeditionPrepRouteSelectionActive();
        PrototypeWorldDispatchBriefingSnapshot briefing = BuildDispatchBriefingSnapshot(
            cityId,
            dungeonId,
            routeId,
            requireSelectedRoute,
            includeDetailed);
        PrototypeWorldSnapshot snapshot = BuildWorldSnapshot(cityId, dungeonId, routeId, briefing);
        return BuildProjectedExpeditionStartContext(snapshot, briefing, includeDetailed);
    }

    private ExpeditionStartContext BuildProjectedExpeditionStartContext(
        PrototypeWorldSnapshot snapshot,
        PrototypeWorldDispatchBriefingSnapshot briefing,
        bool includeDetailedPartyReadbacks = false)
    {
        bool includeDetailed = includeDetailedPartyReadbacks || _isExpeditionPrepBoardOpen;
        ExpeditionStartContext context = new ExpeditionStartContext();
        if (snapshot == null)
        {
            return context;
        }

        context.StartCityId = snapshot.SelectedCityId;
        context.StartCityLabel = snapshot.SelectedCityLabel;
        context.DungeonId = snapshot.SelectedDungeonId;
        context.DungeonLabel = snapshot.SelectedDungeonLabel;
        context.PartyId = snapshot.SelectedPartyId;
        context.PartyLabel = snapshot.PartySummary.PartyLabel;
        context.PartyManifest = BuildExpeditionPartyManifest(
            snapshot.SelectedPartyId,
            snapshot.PartySummary.PartyLabel,
            includeDetailed);
        if (context.PartyManifest != null && IsMeaningfulSnapshotText(context.PartyManifest.PartyLabel))
        {
            context.PartyLabel = context.PartyManifest.PartyLabel;
        }

        context.PartyLoadoutSummaryText = context.PartyManifest != null && IsMeaningfulSnapshotText(context.PartyManifest.LoadoutSummaryText)
            ? context.PartyManifest.LoadoutSummaryText
            : BuildPartyLoadoutSummaryText(snapshot.SelectedPartyId);
        context.SelectedRouteId = snapshot.SelectedRouteId;
        context.SelectedRouteLabel = snapshot.SelectedRouteLabel;
        context.RecommendedRouteId = snapshot.RecommendedRouteId;
        context.RecommendedRouteLabel = snapshot.RecommendedRouteLabel;
        context.NeedPressureText = snapshot.CitySummary != null && IsMeaningfulSnapshotText(snapshot.CitySummary.NeedPressureText)
            ? snapshot.CitySummary.NeedPressureText
            : "None";
        context.DispatchReadinessText = snapshot.CitySummary != null && IsMeaningfulSnapshotText(snapshot.CitySummary.ReadinessText)
            ? snapshot.CitySummary.ReadinessText
            : "None";
        context.RecoveryProgressText = snapshot.CitySummary != null && IsMeaningfulSnapshotText(snapshot.CitySummary.RecoveryText)
            ? snapshot.CitySummary.RecoveryText
            : "None";
        context.RecoveryEtaText = IsMeaningfulSnapshotText(snapshot.SelectedCityId)
            ? BuildRecoveryEtaText(GetRecoveryDaysToReady(snapshot.SelectedCityId))
            : "None";
        context.DispatchPolicyText = snapshot.CitySummary != null && IsMeaningfulSnapshotText(snapshot.CitySummary.DispatchPolicyText)
            ? snapshot.CitySummary.DispatchPolicyText
            : "None";
        context.RecommendationReasonText = IsMeaningfulSnapshotText(snapshot.SelectedCityId) && IsMeaningfulSnapshotText(snapshot.SelectedDungeonId)
            ? BuildRecommendationReasonText(snapshot.SelectedCityId, snapshot.SelectedDungeonId)
            : "None";
        context.ExpectedNeedImpactText = IsMeaningfulSnapshotText(snapshot.SelectedCityId) && IsMeaningfulSnapshotText(snapshot.SelectedDungeonId)
            ? BuildExpectedNeedImpactText(
                snapshot.SelectedCityId,
                snapshot.SelectedDungeonId,
                IsMeaningfulSnapshotText(snapshot.SelectedRouteId) ? snapshot.SelectedRouteId : snapshot.RecommendedRouteId)
            : "None";
        context.RouteRiskLabel = IsMeaningfulSnapshotText(snapshot.SelectedRouteId)
            ? BuildRouteRiskSummaryForRoute(snapshot.SelectedDungeonId, snapshot.SelectedRouteId)
            : "None";
        context.DungeonDangerLabel = snapshot.DungeonSummary.DangerText;
        context.SupplySummaryText = snapshot.PartySummary.SupplySummaryText;
        context.SupplyPressureSummaryText = BuildProjectedExpeditionSupplyPressureSummaryText(snapshot, briefing);
        context.TimePressureSummaryText = BuildProjectedExpeditionTimePressureSummaryText(snapshot, briefing);
        context.ThreatPressureSummaryText = BuildProjectedExpeditionThreatPressureSummaryText(snapshot, briefing);
        context.DiscoverySummaryText = BuildProjectedExpeditionDiscoverySummaryText(snapshot, briefing);
        context.ExtractionPressureSummaryText = BuildProjectedExpeditionExtractionPressureSummaryText(snapshot, briefing);
        context.WorldModifierSummaryText = snapshot.ModifierSummary.SummaryText;
        context.RoutePreviewSummaryText = IsMeaningfulSnapshotText(snapshot.SelectedRouteId)
            ? BuildRoutePreviewSummaryText(snapshot.SelectedDungeonId, snapshot.SelectedRouteId)
            : "None";
        context.RewardPreviewSummaryText = string.IsNullOrEmpty(snapshot.SelectedDungeonId)
            ? "None"
            : BuildRouteRewardPreviewText(snapshot.SelectedDungeonId);
        context.EventPreviewSummaryText = string.IsNullOrEmpty(snapshot.SelectedDungeonId)
            ? "None"
            : BuildRouteEventPreviewText(snapshot.SelectedDungeonId);
        context.StagedPartySummaryText = context.PartyManifest != null && IsMeaningfulSnapshotText(context.PartyManifest.RoleSummaryText)
                ? context.PartyManifest.RoleSummaryText
            : briefing != null && IsMeaningfulSnapshotText(briefing.PartySummaryText)
                ? briefing.PartySummaryText
            : context.PartyManifest != null && IsMeaningfulSnapshotText(context.PartyManifest.ManifestSummaryText)
                ? context.PartyManifest.ManifestSummaryText
                : snapshot.PartySummary.PartySummaryText;
        context.PartySummaryText = context.StagedPartySummaryText;
        context.BriefingSummaryText = briefing != null ? briefing.BriefingSummaryText : snapshot.SnapshotSummaryText;
        context.RouteFitSummaryText = briefing != null ? briefing.RouteFitSummaryText : snapshot.ModifierSummary.SummaryText;
        context.LaunchLockSummaryText = briefing != null ? briefing.LaunchLockSummaryText : snapshot.ModifierSummary.BlockedReasonSummaryText;
        context.ProjectedOutcomeSummaryText = briefing != null ? briefing.ProjectedOutcomeSummaryText : snapshot.ModifierSummary.RecommendationHintText;
        context.FollowedRecommendation =
            IsMeaningfulSnapshotText(context.SelectedRouteId) &&
            IsMeaningfulSnapshotText(context.RecommendedRouteId) &&
            context.SelectedRouteId == context.RecommendedRouteId;
        context.LaunchManifestSummaryText = BuildExpeditionLaunchManifestSummaryText(context);
        context.ContextSummaryText = IsMeaningfulSnapshotText(context.LaunchManifestSummaryText)
            ? context.LaunchManifestSummaryText + " | " + snapshot.ModifierSummary.RecommendationHintText
            : snapshot.SnapshotSummaryText + " | " + snapshot.ModifierSummary.RecommendationHintText;
        return context;
    }

    private void PopulateWorldObservationPriorityBoard(WorldObservationPriorityBoardData priorityBoard, WorldBoardReadModel board)
    {
        if (priorityBoard == null)
        {
            return;
        }

        CityStatusReadModel priorityCity = FindHighestPriorityCity(board);
        if (priorityCity == null)
        {
            return;
        }

        CityDecisionReadModel decision = priorityCity.Decision ?? new CityDecisionReadModel();
        CityBottleneckSignal topBottleneck = GetFirstBoardItem(decision.Bottlenecks);
        CityActionRecommendation topAction = GetFirstBoardItem(decision.RecommendedActions);

        priorityBoard.HasPriorityCity = true;
        priorityBoard.CityId = priorityCity.CityId ?? string.Empty;
        priorityBoard.CityLabel = IsMeaningfulSnapshotText(priorityCity.DisplayName) ? priorityCity.DisplayName : "None";
        priorityBoard.UrgencyText = IsMeaningfulSnapshotText(topBottleneck != null ? topBottleneck.SummaryText : string.Empty)
            ? topBottleneck.SummaryText
            : BuildCityUrgencyFallbackText(priorityCity);
        priorityBoard.WhyCityMattersText = IsMeaningfulSnapshotText(decision.WhyCityMattersText)
            ? decision.WhyCityMattersText
            : priorityBoard.UrgencyText;
        priorityBoard.AnswerText = IsMeaningfulSnapshotText(topAction != null ? topAction.SummaryText : string.Empty)
            ? topAction.SummaryText
            : IsMeaningfulSnapshotText(priorityCity.RecommendedRouteSummaryText)
                ? priorityCity.RecommendedRouteSummaryText
                : "Review the linked dungeon and route answer.";
        priorityBoard.RecentResultEvidenceText = BuildCityRecentResultEvidenceText(priorityCity);
        priorityBoard.PressureChangeText = BuildCityPressureChangeText(priorityCity);
        priorityBoard.PartyReadinessText = BuildCityPartyReadinessSummaryText(priorityCity);
        priorityBoard.NextActionText = BuildCityBoardNextActionText(priorityCity, topAction);
        priorityBoard.SummaryText = BuildWorldPriorityBoardSummaryText(priorityBoard);
    }

    private CityStatusReadModel FindHighestPriorityCity(WorldBoardReadModel board)
    {
        CityStatusReadModel[] cities = board != null ? board.Cities : null;
        if (cities == null || cities.Length <= 0)
        {
            return null;
        }

        CityStatusReadModel bestCity = null;
        int bestScore = int.MinValue;
        for (int i = 0; i < cities.Length; i++)
        {
            CityStatusReadModel city = cities[i];
            if (city == null)
            {
                continue;
            }

            int score = CalculateCityPriorityScore(city);
            if (bestCity == null || score > bestScore)
            {
                bestCity = city;
                bestScore = score;
            }
        }

        return bestCity;
    }

    private int CalculateCityPriorityScore(CityStatusReadModel city)
    {
        if (city == null)
        {
            return int.MinValue;
        }

        CityDecisionReadModel decision = city.Decision ?? new CityDecisionReadModel();
        CityBottleneckSignal topBottleneck = GetFirstBoardItem(decision.Bottlenecks);
        CityActionRecommendation topAction = GetFirstBoardItem(decision.RecommendedActions);
        RecentImpactSummary topImpact = GetFirstBoardItem(decision.RecentImpacts);

        int score = 0;
        score += topBottleneck != null ? topBottleneck.Severity : 0;
        score += topAction != null ? topAction.Priority : 0;
        score += topImpact != null ? topImpact.Priority : 0;

        string needText = city.NeedPressureStateId != null ? city.NeedPressureStateId.ToLowerInvariant() : string.Empty;
        if (needText.Contains("urgent"))
        {
            score += 140;
        }
        else if (needText.Contains("elevated") || needText.Contains("high"))
        {
            score += 90;
        }
        else if (needText.Contains("watch"))
        {
            score += 40;
        }

        string readinessText = city.DispatchReadinessStateId != null ? city.DispatchReadinessStateId.ToLowerInvariant() : string.Empty;
        if (readinessText.Contains("strained"))
        {
            score += 80;
        }
        else if (readinessText.Contains("recover"))
        {
            score += 45;
        }

        if (city.ActiveExpeditionCount > 0)
        {
            score += 25;
        }

        return score;
    }

    private string BuildCityUrgencyFallbackText(CityStatusReadModel city)
    {
        if (city == null)
        {
            return "None";
        }

        List<string> parts = new List<string>();
        if (IsMeaningfulSnapshotText(city.NeedPressureStateId))
        {
            parts.Add("Need " + city.NeedPressureStateId);
        }

        if (IsMeaningfulSnapshotText(city.DispatchReadinessStateId))
        {
            parts.Add("Readiness " + city.DispatchReadinessStateId);
        }

        if (parts.Count == 0)
        {
            return "No pressure signal is currently dominant.";
        }

        return string.Join(" | ", parts.ToArray());
    }

    private string BuildCityRecentResultEvidenceText(CityStatusReadModel city)
    {
        if (city == null)
        {
            return "None";
        }

        ExpeditionResultReadModel latestResult = city.LatestResult;
        if (latestResult != null && latestResult.HasResult)
        {
            List<string> parts = new List<string>();
            string routeText = BuildCompactRouteResultLabel(latestResult);
            string resultText = BuildReadableResultStateText(latestResult);
            parts.Add(IsMeaningfulSnapshotText(routeText)
                ? resultText + ": " + routeText
                : resultText);

            string lootText = IsMeaningfulSnapshotText(latestResult.LootSummaryText)
                ? latestResult.LootSummaryText
                : latestResult.ReturnedLootAmount > 0 && IsMeaningfulSnapshotText(latestResult.RewardResourceId)
                    ? latestResult.RewardResourceId + " x" + latestResult.ReturnedLootAmount
                    : string.Empty;
            if (IsMeaningfulSnapshotText(lootText))
            {
                parts.Add("Returned " + lootText);
            }

            string partyText = IsMeaningfulSnapshotText(latestResult.PartyConditionText)
                ? latestResult.PartyConditionText
                : latestResult.SurvivingMemberCount > 0
                    ? latestResult.SurvivingMemberCount + " survivor(s)"
                    : latestResult.SurvivingMembersSummaryText;
            if (IsMeaningfulSnapshotText(partyText))
            {
                parts.Add("Party " + partyText);
            }

            return string.Join(" | ", parts.ToArray());
        }

        CityDecisionReadModel decision = city.Decision ?? new CityDecisionReadModel();
        RecentImpactSummary topImpact = GetFirstBoardItem(decision.RecentImpacts);
        if (topImpact != null && IsMeaningfulSnapshotText(topImpact.SummaryText))
        {
            return topImpact.SummaryText;
        }

        if (city.LatestResult != null && IsMeaningfulSnapshotText(city.LatestResult.WorldReturnSummaryText))
        {
            return city.LatestResult.WorldReturnSummaryText;
        }

        if (IsMeaningfulSnapshotText(city.LastDispatchImpactText))
        {
            return city.LastDispatchImpactText;
        }

        if (IsMeaningfulSnapshotText(city.LastNeedPressureChangeText))
        {
            return "Need pressure changed: " + city.LastNeedPressureChangeText;
        }

        if (IsMeaningfulSnapshotText(city.LastDispatchReadinessChangeText))
        {
            return "Dispatch status changed: " + city.LastDispatchReadinessChangeText;
        }

        return "No recent expedition evidence is reshaping this city yet.";
    }

    private string BuildCityPressureChangeText(CityStatusReadModel city)
    {
        if (city == null)
        {
            return "None";
        }

        List<string> parts = new List<string>();
        ExpeditionResultReadModel latestResult = city.LatestResult;
        if (latestResult != null && latestResult.HasResult)
        {
            if (latestResult.ReturnedLootAmount > 0 && IsMeaningfulSnapshotText(latestResult.RewardResourceId))
            {
                parts.Add("Stock +" + latestResult.ReturnedLootAmount + " " + latestResult.RewardResourceId);
            }
        }

        if (IsMeaningfulSnapshotText(city.LastNeedPressureChangeText))
        {
            parts.Add("Pressure " + city.LastNeedPressureChangeText);
        }

        if (IsMeaningfulSnapshotText(city.LastDispatchReadinessChangeText))
        {
            parts.Add("Readiness " + city.LastDispatchReadinessChangeText);
        }

        if (latestResult != null &&
            latestResult.HasResult &&
            parts.Count == 0 &&
            IsMeaningfulSnapshotText(latestResult.CityStatusChangeSummaryText))
        {
            parts.Add(latestResult.CityStatusChangeSummaryText);
        }

        if (parts.Count == 0 && IsMeaningfulSnapshotText(city.LastDispatchImpactText))
        {
            parts.Add(city.LastDispatchImpactText);
        }

        return parts.Count > 0
            ? string.Join(" | ", parts.ToArray())
            : "No pressure change recorded.";
    }

    private string BuildCityPartyReadinessSummaryText(CityStatusReadModel city)
    {
        if (city == null)
        {
            return "None";
        }

        string readinessText = IsMeaningfulSnapshotText(city.DispatchReadinessStateId)
            ? city.DispatchReadinessStateId
            : "Unknown";
        string recoveryText = city.DispatchRecoveryDaysRemaining > 0
            ? "recovery " + BuildDayCountText(city.DispatchRecoveryDaysRemaining)
            : "no recovery wait";
        if (city.IdlePartyCount < 1)
        {
            return city.ActiveExpeditionCount > 0
                ? "Blocked: party deployed | active expeditions " + city.ActiveExpeditionCount + " | readiness " + readinessText
                : "Blocked: no idle party | readiness " + readinessText;
        }

        if (IsMeaningfulSnapshotText(city.LinkedDungeonId) && city.AvailableContractSlots < 1)
        {
            return "Blocked: no contract slot | idle parties " + city.IdlePartyCount + " | readiness " + readinessText;
        }

        if (readinessText == "Strained")
        {
            return "Blocked: dispatch strained | " + recoveryText + " | idle parties " + city.IdlePartyCount;
        }

        if (city.DispatchRecoveryDaysRemaining > 0)
        {
            return "Ready: warning | " + recoveryText + " | party idle | route available";
        }

        return "Ready: party idle | route available | readiness " + readinessText;
    }

    private string BuildCityBoardNextActionText(CityStatusReadModel city, CityActionRecommendation topAction)
    {
        if (topAction != null && IsMeaningfulSnapshotText(topAction.SummaryText))
        {
            return topAction.SummaryText;
        }

        if (city == null)
        {
            return "Review the world board before the next dispatch.";
        }

        if (city.IdlePartyCount < 1)
        {
            return "Recruit or wait for a party before opening Expedition Prep.";
        }

        if (IsMeaningfulSnapshotText(city.LinkedDungeonId) && city.AvailableContractSlots < 1)
        {
            return "Wait for the linked dungeon contract slot to reopen.";
        }

        if (city.DispatchReadinessStateId == "Strained")
        {
            return city.DispatchRecoveryDaysRemaining > 0
                ? "Wait " + BuildDayCountText(city.DispatchRecoveryDaysRemaining) + " for dispatch recovery."
                : "Wait until dispatch readiness is no longer strained.";
        }

        if (IsMeaningfulSnapshotText(city.RecommendedRouteSummaryText))
        {
            return "Open Expedition Prep and review " + city.RecommendedRouteSummaryText + ".";
        }

        return BuildCityNextActionFallbackText(city);
    }

    private string BuildCityNextActionFallbackText(CityStatusReadModel city)
    {
        if (city == null)
        {
            return "Review the world board before the next dispatch.";
        }

        CityDecisionReadModel decision = city.Decision ?? new CityDecisionReadModel();
        RecentImpactSummary topImpact = GetFirstBoardItem(decision.RecentImpacts);
        if (topImpact != null && IsMeaningfulSnapshotText(topImpact.NextDecisionHintText))
        {
            return topImpact.NextDecisionHintText;
        }

        if (IsMeaningfulSnapshotText(city.RecommendationReasonText))
        {
            return city.RecommendationReasonText;
        }

        return "Review the latest result and commit the next dispatch from the board.";
    }

    private string BuildWorldPriorityBoardSummaryText(WorldObservationPriorityBoardData priorityBoard)
    {
        if (priorityBoard == null || !priorityBoard.HasPriorityCity)
        {
            return "None";
        }

        return BuildPressureBoardSummaryText(
            priorityBoard.CityLabel,
            priorityBoard.UrgencyText,
            priorityBoard.RecentResultEvidenceText,
            priorityBoard.PressureChangeText,
            priorityBoard.PartyReadinessText,
            priorityBoard.AnswerText,
            priorityBoard.NextActionText);
    }

    private string BuildPressureBoardSummaryText(
        string cityLabel,
        string urgencyText,
        string recentResultEvidenceText,
        string pressureChangeText,
        string readinessText,
        string answerText,
        string nextActionText)
    {
        List<string> parts = new List<string>();
        if (IsMeaningfulSnapshotText(cityLabel))
        {
            parts.Add(cityLabel + " pressure board");
        }

        if (IsMeaningfulSnapshotText(recentResultEvidenceText))
        {
            parts.Add("Latest: " + recentResultEvidenceText);
        }

        if (IsMeaningfulSnapshotText(pressureChangeText))
        {
            parts.Add("Changed: " + pressureChangeText);
        }

        string nextText = IsMeaningfulSnapshotText(nextActionText)
            ? nextActionText
            : answerText;
        if (IsMeaningfulSnapshotText(nextText))
        {
            parts.Add("Next: " + nextText);
        }

        if (IsMeaningfulSnapshotText(readinessText))
        {
            parts.Add(BuildPressureBoardReadyLine(readinessText));
        }

        if (IsMeaningfulSnapshotText(urgencyText))
        {
            parts.Add("Why: " + urgencyText);
        }

        return parts.Count > 0
            ? string.Join(" | ", parts.ToArray())
            : "None";
    }

    private string BuildReadableResultStateText(ExpeditionResultReadModel latestResult)
    {
        if (latestResult == null)
        {
            return "Result";
        }

        string resultKey = latestResult.ResultStateKey != null ? latestResult.ResultStateKey.ToLowerInvariant() : string.Empty;
        if (latestResult.Success || resultKey.Contains("clear") || resultKey.Contains("victory"))
        {
            return "Cleared";
        }

        if (resultKey.Contains("fail") || resultKey.Contains("defeat"))
        {
            return "Failed";
        }

        if (resultKey.Contains("retreat") || resultKey.Contains("abort"))
        {
            return "Returned";
        }

        return "Result";
    }

    private string BuildCompactRouteResultLabel(ExpeditionResultReadModel latestResult)
    {
        if (latestResult == null)
        {
            return "None";
        }

        string routeText = IsMeaningfulSnapshotText(latestResult.RouteSummaryText)
            ? latestResult.RouteSummaryText
            : IsMeaningfulSnapshotText(latestResult.TargetDungeonDisplayName)
                ? latestResult.TargetDungeonDisplayName
                : latestResult.TargetDungeonId;
        if (!IsMeaningfulSnapshotText(routeText))
        {
            return "None";
        }

        string[] parts = routeText.Split('|');
        if (parts.Length > 1 && IsMeaningfulSnapshotText(parts[1].Trim()))
        {
            return parts[1].Trim();
        }

        return parts[0].Trim();
    }

    private string BuildPressureBoardReadyLine(string readinessText)
    {
        if (!IsMeaningfulSnapshotText(readinessText))
        {
            return "Ready: unknown";
        }

        return readinessText.StartsWith("Ready:", System.StringComparison.OrdinalIgnoreCase)
            ? readinessText
            : "Ready: " + readinessText;
    }

    private static T GetFirstBoardItem<T>(T[] items) where T : class
    {
        return items != null && items.Length > 0 ? items[0] : null;
    }

    private void PopulateWorldObservationCurrentContext(WorldObservationCurrentContextData context, PrototypeWorldSnapshot snapshot)
    {
        if (context == null || snapshot == null)
        {
            return;
        }

        context.HasContext =
            IsMeaningfulSnapshotText(snapshot.SelectedCityId) ||
            IsMeaningfulSnapshotText(snapshot.SelectedDungeonId) ||
            IsMeaningfulSnapshotText(snapshot.SelectedRouteId);
        context.CityId = snapshot.SelectedCityId ?? string.Empty;
        context.CityLabel = IsMeaningfulSnapshotText(snapshot.SelectedCityLabel) ? snapshot.SelectedCityLabel : "None";
        context.DungeonId = snapshot.SelectedDungeonId ?? string.Empty;
        context.DungeonLabel = IsMeaningfulSnapshotText(snapshot.SelectedDungeonLabel) ? snapshot.SelectedDungeonLabel : "None";
        context.RouteId = snapshot.SelectedRouteId ?? string.Empty;
        context.RouteLabel = IsMeaningfulSnapshotText(snapshot.SelectedRouteLabel) ? snapshot.SelectedRouteLabel : "None";
        context.RecommendedRouteId = snapshot.RecommendedRouteId ?? string.Empty;
        context.RecommendedRouteLabel = IsMeaningfulSnapshotText(snapshot.RecommendedRouteLabel) ? snapshot.RecommendedRouteLabel : "None";
        context.PartyId = snapshot.SelectedPartyId ?? string.Empty;
        context.PartyLabel = snapshot.PartySummary != null && IsMeaningfulSnapshotText(snapshot.PartySummary.PartyLabel)
            ? snapshot.PartySummary.PartyLabel
            : "None";
    }

    private void PopulateWorldObservationSelectedEntity(WorldObservationSelectedEntityData selectedEntity, PrototypeWorldSnapshot snapshot)
    {
        if (selectedEntity == null)
        {
            return;
        }

        PrototypeWorldWritebackObservationSurfaceData writeback = BuildSelectedWorldWritebackObservationSurfaceData();
        selectedEntity.HasSelection = _selectedMarker != null;
        selectedEntity.SelectedKindKey = BuildSelectedEntityKindKey();
        selectedEntity.SelectedKindLabel = _selectedMarker != null && _selectedMarker.EntityData != null
            ? _selectedMarker.EntityData.Kind.ToString()
            : "None";
        selectedEntity.SelectedEntityId = _selectedMarker != null ? _selectedMarker.EntityData.Id ?? string.Empty : string.Empty;
        selectedEntity.SelectedEntityLabel = _selectedMarker != null
            ? (string.IsNullOrEmpty(_selectedMarker.EntityData.DisplayName) ? _selectedMarker.EntityData.Id : _selectedMarker.EntityData.DisplayName)
            : "None";
        selectedEntity.SelectedPositionText = SelectedPositionText;
        selectedEntity.SelectedResourcesText = SelectedResourcesText;
        selectedEntity.SelectedResourceRolesText = SelectedResourceRolesText;
        selectedEntity.SelectedSupplyText = SelectedSupplyText;
        selectedEntity.SelectedNeedsText = SelectedNeedsText;
        selectedEntity.SelectedHighPriorityNeedsText = SelectedHighPriorityNeedsText;
        selectedEntity.SelectedNormalPriorityNeedsText = SelectedNormalPriorityNeedsText;
        selectedEntity.SelectedOutputText = SelectedOutputText;
        selectedEntity.SelectedProcessingText = SelectedProcessingText;
        selectedEntity.SelectedStocksText = SelectedStocksText;
        selectedEntity.SelectedLinkedCityText = SelectedLinkedCityText;
        selectedEntity.SelectedPartiesInCityText = SelectedPartiesInCityText;
        selectedEntity.SelectedIdlePartiesText = SelectedIdlePartiesText;
        selectedEntity.SelectedActiveExpeditionsFromCityText = SelectedActiveExpeditionsFromCityText;
        selectedEntity.SelectedAvailableContractText = SelectedAvailableContractText;
        selectedEntity.SelectedLinkedDungeonText = SelectedLinkedDungeonText;
        selectedEntity.SelectedDungeonDangerText = SelectedDungeonDangerText;
        selectedEntity.SelectedCityManaShardStockText = SelectedCityManaShardStockText;
        selectedEntity.SelectedCityId = snapshot != null ? snapshot.SelectedCityId ?? string.Empty : string.Empty;
        selectedEntity.SelectedCityLabel = snapshot != null && IsMeaningfulSnapshotText(snapshot.SelectedCityLabel) ? snapshot.SelectedCityLabel : "None";
        selectedEntity.SelectedDungeonId = snapshot != null ? snapshot.SelectedDungeonId ?? string.Empty : string.Empty;
        selectedEntity.SelectedDungeonLabel = snapshot != null && IsMeaningfulSnapshotText(snapshot.SelectedDungeonLabel) ? snapshot.SelectedDungeonLabel : "None";
        selectedEntity.SelectedRouteId = snapshot != null ? snapshot.SelectedRouteId ?? string.Empty : string.Empty;
        selectedEntity.SelectedRouteLabel = snapshot != null && IsMeaningfulSnapshotText(snapshot.SelectedRouteLabel) ? snapshot.SelectedRouteLabel : "None";
        selectedEntity.SelectedPartyId = snapshot != null ? snapshot.SelectedPartyId ?? string.Empty : string.Empty;
        selectedEntity.SelectedPartyLabel = snapshot != null &&
            snapshot.PartySummary != null &&
            IsMeaningfulSnapshotText(snapshot.PartySummary.PartyLabel)
            ? snapshot.PartySummary.PartyLabel
            : "None";
        selectedEntity.SelectedWorldWritebackText = writeback != null && IsMeaningfulSnapshotText(writeback.WritebackText) ? writeback.WritebackText : "None";
        selectedEntity.SelectedDungeonStatusText = writeback != null && IsMeaningfulSnapshotText(writeback.DungeonStatusText) ? writeback.DungeonStatusText : "None";
        selectedEntity.SelectedDungeonAvailabilityText = writeback != null && IsMeaningfulSnapshotText(writeback.DungeonAvailabilityText) ? writeback.DungeonAvailabilityText : "None";
    }

    private void PopulateWorldObservationLaunch(
        WorldObservationLaunchData launch,
        ExpeditionPrepSurfaceData expeditionPrep,
        PrototypeWorldSnapshot snapshot,
        PrototypeWorldDispatchBriefingSnapshot briefing,
        ExpeditionStartContext startContext)
    {
        if (launch == null)
        {
            return;
        }

        string cityId = snapshot != null ? snapshot.SelectedCityId : string.Empty;
        string dungeonId = snapshot != null ? snapshot.SelectedDungeonId : string.Empty;
        LaunchReadiness launchReadiness = BuildLaunchReadiness(snapshot, briefing);
        ExpeditionPrepSurfaceData prep = expeditionPrep ?? new ExpeditionPrepSurfaceData();
        launch.LaunchReadiness = prep.LaunchReadiness != null ? CopyLaunchReadiness(prep.LaunchReadiness) : launchReadiness;
        launch.CanLaunch = prep.CanLaunch;
        launch.HasSelectedRoute = !string.IsNullOrEmpty(prep.SelectedRouteId);
        launch.LaunchRequest = BuildExpeditionLaunchRequestFromSurface(prep);
        launch.DispatchPolicyText = IsMeaningfulSnapshotText(prep.DispatchPolicyText) ? prep.DispatchPolicyText : "None";
        launch.NeedPressureText = IsMeaningfulSnapshotText(prep.NeedPressureText) ? prep.NeedPressureText : "None";
        launch.DispatchReadinessText = IsMeaningfulSnapshotText(prep.DispatchReadinessText) ? prep.DispatchReadinessText : launchReadiness.ReadinessText;
        launch.RecoveryProgressText = IsMeaningfulSnapshotText(prep.RecoveryProgressText) ? prep.RecoveryProgressText : launchReadiness.RecoveryProgressText;
        launch.RecoveryEtaText = IsMeaningfulSnapshotText(prep.RecoveryEtaText) ? prep.RecoveryEtaText : launchReadiness.RecoveryEtaText;
        launch.BlockedReasonSummaryText = IsMeaningfulSnapshotText(prep.BlockedReasonText) ? prep.BlockedReasonText : "None";
        launch.CurrentBlockedLaunchReasonSummaryText = launch.BlockedReasonSummaryText;
        launch.RecommendedNextActionSummaryText = IsMeaningfulSnapshotText(prep.RecommendedNextActionText) ? prep.RecommendedNextActionText : launchReadiness.RecommendedActionText;
        launch.DispatchPartySummaryText = IsMeaningfulSnapshotText(prep.StagedPartySummaryText) ? prep.StagedPartySummaryText : "None";
        launch.DispatchBriefingSummaryText = IsMeaningfulSnapshotText(prep.DispatchBriefingSummaryText) ? prep.DispatchBriefingSummaryText : "None";
        launch.RouteFitSummaryText = IsMeaningfulSnapshotText(prep.RouteFitSummaryText) ? prep.RouteFitSummaryText : "None";
        launch.LaunchGateSummaryText = IsMeaningfulSnapshotText(prep.LaunchGateSummaryText) ? prep.LaunchGateSummaryText : launch.BlockedReasonSummaryText;
        launch.ProjectedOutcomeSummaryText = IsMeaningfulSnapshotText(prep.ProjectedOutcomeSummaryText)
            ? prep.ProjectedOutcomeSummaryText
            : startContext != null && IsMeaningfulSnapshotText(startContext.ProjectedOutcomeSummaryText)
                ? startContext.ProjectedOutcomeSummaryText
                : "None";
        launch.RecommendedRouteId = prep.RecommendedRouteId ?? string.Empty;
        launch.RecommendedRouteLabel = IsMeaningfulSnapshotText(prep.RecommendedRouteLabel) ? prep.RecommendedRouteLabel : "None";
        launch.RecommendedRouteSummaryText = IsMeaningfulSnapshotText(prep.RecommendedRouteSummaryText)
            ? prep.RecommendedRouteSummaryText
            : !string.IsNullOrEmpty(cityId) && !string.IsNullOrEmpty(dungeonId)
                ? BuildRecommendedRouteSummaryText(cityId, dungeonId)
                : "None";
        launch.RecommendedRouteForLinkedCityText = launch.RecommendedRouteSummaryText;
        launch.RecommendationReasonText = IsMeaningfulSnapshotText(prep.RecommendationReasonText)
            ? prep.RecommendationReasonText
            : startContext != null && IsMeaningfulSnapshotText(startContext.RecommendationReasonText)
                ? startContext.RecommendationReasonText
                : "None";
        launch.ExpectedNeedImpactText = IsMeaningfulSnapshotText(prep.ExpectedNeedImpactText)
            ? prep.ExpectedNeedImpactText
            : startContext != null && IsMeaningfulSnapshotText(startContext.ExpectedNeedImpactText)
                ? startContext.ExpectedNeedImpactText
                : launchReadiness.ExpectedNeedImpactText;
        launch.RecommendedPower = _runtimeEconomyState != null && !string.IsNullOrEmpty(dungeonId)
            ? _runtimeEconomyState.GetRecommendedPower(dungeonId)
            : 0;
        launch.ExpeditionDurationDays = _runtimeEconomyState != null && !string.IsNullOrEmpty(dungeonId)
            ? _runtimeEconomyState.GetExpeditionDurationDays(dungeonId)
            : 0;
        launch.RewardPreviewText = IsMeaningfulSnapshotText(prep.RewardPreviewText)
            ? prep.RewardPreviewText
            : startContext != null && IsMeaningfulSnapshotText(startContext.RewardPreviewSummaryText)
                ? startContext.RewardPreviewSummaryText
                : "None";
        launch.EventPreviewText = IsMeaningfulSnapshotText(prep.EventPreviewText)
            ? prep.EventPreviewText
            : startContext != null && IsMeaningfulSnapshotText(startContext.EventPreviewSummaryText)
                ? startContext.EventPreviewSummaryText
                : "None";
    }

    private void PopulateWorldObservationActiveExpedition(WorldObservationActiveExpeditionData activeExpedition)
    {
        if (activeExpedition == null)
        {
            return;
        }

        PrototypeWorldActiveExpeditionSnapshot selectedSnapshot = BuildSelectedActiveExpeditionSnapshot();
        PrototypeWorldActiveExpeditionSnapshot worldSnapshot = BuildWorldActiveExpeditionSnapshot();

        activeExpedition.HasSelectedActiveExpedition = selectedSnapshot != null && !string.IsNullOrEmpty(selectedSnapshot.PartyId);
        activeExpedition.SelectedCityId = selectedSnapshot != null ? selectedSnapshot.CityId ?? string.Empty : string.Empty;
        activeExpedition.SelectedCityLabel = selectedSnapshot != null && IsMeaningfulSnapshotText(selectedSnapshot.CityLabel) ? selectedSnapshot.CityLabel : "None";
        activeExpedition.SelectedDungeonId = selectedSnapshot != null ? selectedSnapshot.DungeonId ?? string.Empty : string.Empty;
        activeExpedition.SelectedDungeonLabel = selectedSnapshot != null && IsMeaningfulSnapshotText(selectedSnapshot.DungeonLabel) ? selectedSnapshot.DungeonLabel : "None";
        activeExpedition.SelectedPartyId = selectedSnapshot != null ? selectedSnapshot.PartyId ?? string.Empty : string.Empty;
        activeExpedition.SelectedPartyLabel = selectedSnapshot != null && IsMeaningfulSnapshotText(selectedSnapshot.PartyLabel) ? selectedSnapshot.PartyLabel : "None";
        activeExpedition.SelectedRouteId = selectedSnapshot != null ? selectedSnapshot.RouteId ?? string.Empty : string.Empty;
        activeExpedition.SelectedRouteLabel = selectedSnapshot != null && IsMeaningfulSnapshotText(selectedSnapshot.RouteLabel) ? selectedSnapshot.RouteLabel : "None";
        activeExpedition.SelectedTravelStateKey = selectedSnapshot != null && IsMeaningfulSnapshotText(selectedSnapshot.TravelStateKey)
            ? selectedSnapshot.TravelStateKey
            : "idle";
        activeExpedition.SelectedDaysRemaining = selectedSnapshot != null ? selectedSnapshot.DaysRemaining : 0;
        activeExpedition.SelectedProjectedReturnDay = selectedSnapshot != null ? selectedSnapshot.ProjectedReturnDay : -1;
        activeExpedition.SelectedActiveExpeditionLaneText = selectedSnapshot != null && selectedSnapshot.TravelSummary != null && IsMeaningfulSnapshotText(selectedSnapshot.TravelSummary.ActiveLaneText)
            ? selectedSnapshot.TravelSummary.ActiveLaneText
            : "None";
        activeExpedition.SelectedDepartureEchoText = selectedSnapshot != null && selectedSnapshot.TravelSummary != null && IsMeaningfulSnapshotText(selectedSnapshot.TravelSummary.DepartureEchoText)
            ? selectedSnapshot.TravelSummary.DepartureEchoText
            : "None";
        activeExpedition.SelectedReturnEtaText = selectedSnapshot != null && selectedSnapshot.TravelSummary != null && IsMeaningfulSnapshotText(selectedSnapshot.TravelSummary.ReturnEtaText)
            ? selectedSnapshot.TravelSummary.ReturnEtaText
            : "None";
        activeExpedition.SelectedCityVacancyText = selectedSnapshot != null && selectedSnapshot.TravelSummary != null && IsMeaningfulSnapshotText(selectedSnapshot.TravelSummary.CityVacancyLabel)
            ? selectedSnapshot.TravelSummary.CityVacancyLabel
            : "None";
        activeExpedition.SelectedDungeonInboundExpeditionText = selectedSnapshot != null && selectedSnapshot.TravelSummary != null && IsMeaningfulSnapshotText(selectedSnapshot.TravelSummary.DungeonInboundLabel)
            ? selectedSnapshot.TravelSummary.DungeonInboundLabel
            : "None";
        activeExpedition.SelectedRouteOccupancyText = selectedSnapshot != null && selectedSnapshot.TravelSummary != null && IsMeaningfulSnapshotText(selectedSnapshot.TravelSummary.RouteOccupancyLabel)
            ? selectedSnapshot.TravelSummary.RouteOccupancyLabel
            : "None";
        activeExpedition.SelectedReturnWindowText = selectedSnapshot != null && selectedSnapshot.ReturnWindow != null && IsMeaningfulSnapshotText(selectedSnapshot.ReturnWindow.ReturnWindowLabel)
            ? selectedSnapshot.ReturnWindow.ReturnWindowLabel
            : "None";
        activeExpedition.SelectedRecoveryAfterReturnText = selectedSnapshot != null && selectedSnapshot.ReturnWindow != null && IsMeaningfulSnapshotText(selectedSnapshot.ReturnWindow.RecoveryAfterReturnLabel)
            ? selectedSnapshot.ReturnWindow.RecoveryAfterReturnLabel
            : "None";

        activeExpedition.HasWorldActiveExpedition = worldSnapshot != null && !string.IsNullOrEmpty(worldSnapshot.PartyId);
        activeExpedition.WorldCityId = worldSnapshot != null ? worldSnapshot.CityId ?? string.Empty : string.Empty;
        activeExpedition.WorldCityLabel = worldSnapshot != null && IsMeaningfulSnapshotText(worldSnapshot.CityLabel) ? worldSnapshot.CityLabel : "None";
        activeExpedition.WorldDungeonId = worldSnapshot != null ? worldSnapshot.DungeonId ?? string.Empty : string.Empty;
        activeExpedition.WorldDungeonLabel = worldSnapshot != null && IsMeaningfulSnapshotText(worldSnapshot.DungeonLabel) ? worldSnapshot.DungeonLabel : "None";
        activeExpedition.WorldPartyId = worldSnapshot != null ? worldSnapshot.PartyId ?? string.Empty : string.Empty;
        activeExpedition.WorldPartyLabel = worldSnapshot != null && IsMeaningfulSnapshotText(worldSnapshot.PartyLabel) ? worldSnapshot.PartyLabel : "None";
        activeExpedition.WorldRouteId = worldSnapshot != null ? worldSnapshot.RouteId ?? string.Empty : string.Empty;
        activeExpedition.WorldRouteLabel = worldSnapshot != null && IsMeaningfulSnapshotText(worldSnapshot.RouteLabel) ? worldSnapshot.RouteLabel : "None";
        activeExpedition.WorldTravelStateKey = worldSnapshot != null && IsMeaningfulSnapshotText(worldSnapshot.TravelStateKey)
            ? worldSnapshot.TravelStateKey
            : "idle";
        activeExpedition.WorldDaysRemaining = worldSnapshot != null ? worldSnapshot.DaysRemaining : 0;
        activeExpedition.WorldProjectedReturnDay = worldSnapshot != null ? worldSnapshot.ProjectedReturnDay : -1;
        activeExpedition.WorldActiveExpeditionLaneText = worldSnapshot != null && worldSnapshot.TravelSummary != null && IsMeaningfulSnapshotText(worldSnapshot.TravelSummary.ActiveLaneText)
            ? worldSnapshot.TravelSummary.ActiveLaneText
            : "None";
        activeExpedition.WorldDepartureEchoText = worldSnapshot != null && worldSnapshot.TravelSummary != null && IsMeaningfulSnapshotText(worldSnapshot.TravelSummary.DepartureEchoText)
            ? worldSnapshot.TravelSummary.DepartureEchoText
            : "None";
        activeExpedition.WorldReturnEtaText = worldSnapshot != null && worldSnapshot.TravelSummary != null && IsMeaningfulSnapshotText(worldSnapshot.TravelSummary.ReturnEtaText)
            ? worldSnapshot.TravelSummary.ReturnEtaText
            : "None";
    }

    private void PopulateWorldObservationRecentOutcome(WorldObservationRecentOutcomeData recentOutcome, PrototypeWorldSnapshot snapshot)
    {
        if (recentOutcome == null)
        {
            return;
        }

        string cityId = snapshot != null ? snapshot.SelectedCityId ?? string.Empty : string.Empty;
        global::WorldWriteback selectedWorldWriteback = _runtimeEconomyState != null && !string.IsNullOrEmpty(cityId)
            ? _runtimeEconomyState.GetLatestWorldWritebackForCity(cityId)
            : new global::WorldWriteback();
        global::WorldWriteback latestWorldWriteback = _runtimeEconomyState != null
            ? _runtimeEconomyState.GetLatestWorldWriteback()
            : new global::WorldWriteback();
        recentOutcome.HasSelectedWorldWriteback = selectedWorldWriteback != null && IsMeaningfulSnapshotText(selectedWorldWriteback.RunResultStateKey);
        recentOutcome.HasLatestWorldWriteback = latestWorldWriteback != null && IsMeaningfulSnapshotText(latestWorldWriteback.RunResultStateKey);
        recentOutcome.SelectedWorldWriteback = selectedWorldWriteback ?? new global::WorldWriteback();
        recentOutcome.LatestWorldWriteback = latestWorldWriteback ?? new global::WorldWriteback();

        string dungeonId = snapshot != null ? snapshot.SelectedDungeonId ?? string.Empty : string.Empty;
        string cityLabel = snapshot != null && IsMeaningfulSnapshotText(snapshot.SelectedCityLabel)
            ? snapshot.SelectedCityLabel
            : ResolveDispatchEntityDisplayName(cityId);
        string nextSuggestedActionText = ResolveOutcomeReadbackNextSuggestedActionText(null, cityId, dungeonId);
        string latestReturnAftermathText = snapshot != null && IsMeaningfulSnapshotText(snapshot.RecentAftermathEchoText)
            ? snapshot.RecentAftermathEchoText
            : "None";
        CityWriteback cityWriteback = BuildCityWritebackSurfaceForCity(cityId, nextSuggestedActionText);

        // Keep legacy observation strings for current consumers, but derive them from the typed readback first.
        OutcomeReadback outcomeReadback = BuildOutcomeReadbackSurfaceForContext(
            cityId,
            dungeonId,
            cityLabel,
            string.IsNullOrEmpty(cityId)
                ? "No city selected for recent outcome readback."
                : cityLabel + " recent outcome readback is ready.",
            nextSuggestedActionText,
            latestReturnAftermathText,
            cityWriteback);
        recentOutcome.ResultPipelineReadback = outcomeReadback ?? new OutcomeReadback();
        recentOutcome.PostRunReveal = BuildLatestExpeditionPostRunRevealState();
        recentOutcome.WorldWritebackSummaryText = outcomeReadback != null && IsMeaningfulSnapshotText(outcomeReadback.WorldWritebackSummaryText)
            ? outcomeReadback.WorldWritebackSummaryText
            : latestWorldWriteback != null && IsMeaningfulSnapshotText(latestWorldWriteback.WritebackSummaryText)
                ? latestWorldWriteback.WritebackSummaryText
                : "None";
        recentOutcome.RecentAftermathSummaryText = outcomeReadback != null && IsMeaningfulSnapshotText(outcomeReadback.LatestReturnAftermathText)
            ? outcomeReadback.LatestReturnAftermathText
            : latestReturnAftermathText;
        recentOutcome.SelectedLastDispatchImpactText = outcomeReadback != null && IsMeaningfulSnapshotText(outcomeReadback.LatestReturnAftermathText)
            ? outcomeReadback.LatestReturnAftermathText
            : GetSelectedLastDispatchImpactText();
        recentOutcome.SelectedLastDispatchStockDeltaText = outcomeReadback != null && IsMeaningfulSnapshotText(outcomeReadback.StockDeltaText)
            ? outcomeReadback.StockDeltaText
            : GetSelectedLastDispatchStockDeltaText();
        recentOutcome.SelectedLastNeedPressureChangeText = outcomeReadback != null &&
            IsMeaningfulSnapshotText(outcomeReadback.NeedPressureBeforeText) &&
            IsMeaningfulSnapshotText(outcomeReadback.NeedPressureAfterText)
            ? BuildNeedPressureChangeSummary(outcomeReadback.NeedPressureBeforeText, outcomeReadback.NeedPressureAfterText)
            : GetSelectedLastNeedPressureChangeText();
        recentOutcome.SelectedLastDispatchReadinessChangeText = outcomeReadback != null &&
            IsMeaningfulSnapshotText(outcomeReadback.DispatchReadinessBeforeText) &&
            IsMeaningfulSnapshotText(outcomeReadback.DispatchReadinessAfterText)
            ? BuildDispatchReadinessChangeSummary(outcomeReadback.DispatchReadinessBeforeText, outcomeReadback.DispatchReadinessAfterText)
            : GetSelectedLastDispatchReadinessChangeText();
        recentOutcome.SelectedActiveExpeditionsText = GetSelectedActiveExpeditionsText();
        recentOutcome.SelectedLastExpeditionResultText = outcomeReadback != null && IsMeaningfulSnapshotText(outcomeReadback.PostRunSummaryText)
            ? outcomeReadback.PostRunSummaryText
            : selectedWorldWriteback != null && IsMeaningfulSnapshotText(selectedWorldWriteback.ResultSummaryText)
                ? selectedWorldWriteback.ResultSummaryText
                : GetSelectedLastExpeditionResultText();
        recentOutcome.SelectedExpeditionLootReturnedText = cityWriteback != null && IsMeaningfulSnapshotText(cityWriteback.LootSummaryText)
            ? cityWriteback.LootSummaryText
            : selectedWorldWriteback != null && IsMeaningfulSnapshotText(selectedWorldWriteback.LootSummaryText)
                ? selectedWorldWriteback.LootSummaryText
                : SelectedExpeditionLootReturnedText;
        recentOutcome.SelectedLastRunSurvivingMembersText = outcomeReadback != null && IsMeaningfulSnapshotText(outcomeReadback.SurvivingMembersSummaryText)
            ? outcomeReadback.SurvivingMembersSummaryText
            : selectedWorldWriteback != null && IsMeaningfulSnapshotText(selectedWorldWriteback.SurvivingMembersSummaryText)
                ? selectedWorldWriteback.SurvivingMembersSummaryText
                : SelectedLastRunSurvivingMembersText;
        recentOutcome.SelectedLastRunClearedEncountersText = outcomeReadback != null && IsMeaningfulSnapshotText(outcomeReadback.ClearedEncountersSummaryText)
            ? outcomeReadback.ClearedEncountersSummaryText
            : selectedWorldWriteback != null && IsMeaningfulSnapshotText(selectedWorldWriteback.ClearedEncountersSummaryText)
                ? selectedWorldWriteback.ClearedEncountersSummaryText
                : SelectedLastRunClearedEncountersText;
        recentOutcome.SelectedLastRunEventChoiceText = outcomeReadback != null && IsMeaningfulSnapshotText(outcomeReadback.EventChoiceSummaryText)
            ? outcomeReadback.EventChoiceSummaryText
            : selectedWorldWriteback != null && IsMeaningfulSnapshotText(selectedWorldWriteback.EventChoiceSummaryText)
                ? selectedWorldWriteback.EventChoiceSummaryText
                : SelectedLastRunEventChoiceText;
        recentOutcome.SelectedLastRunLootBreakdownText = outcomeReadback != null && IsMeaningfulSnapshotText(outcomeReadback.LootBreakdownSummaryText)
            ? outcomeReadback.LootBreakdownSummaryText
            : selectedWorldWriteback != null && IsMeaningfulSnapshotText(selectedWorldWriteback.LootBreakdownSummaryText)
                ? selectedWorldWriteback.LootBreakdownSummaryText
                : SelectedLastRunLootBreakdownText;
        recentOutcome.SelectedLastRunDungeonText = outcomeReadback != null && IsMeaningfulSnapshotText(outcomeReadback.DungeonSummaryText)
            ? outcomeReadback.DungeonSummaryText
            : selectedWorldWriteback != null && IsMeaningfulSnapshotText(selectedWorldWriteback.DungeonSummaryText)
                ? selectedWorldWriteback.DungeonSummaryText
                : SelectedLastRunDungeonText;
        recentOutcome.SelectedLastRunRouteText = outcomeReadback != null && IsMeaningfulSnapshotText(outcomeReadback.RouteSummaryText)
            ? outcomeReadback.RouteSummaryText
            : selectedWorldWriteback != null && IsMeaningfulSnapshotText(selectedWorldWriteback.RouteSummaryText)
                ? selectedWorldWriteback.RouteSummaryText
                : SelectedLastRunRouteText;
    }

    private void PopulateWorldObservationLogs(WorldObservationLogData logs)
    {
        if (logs == null)
        {
            return;
        }

        logs.RecentWorldWritebackLog1Text = RecentWorldWritebackLog1Text;
        logs.RecentWorldWritebackLog2Text = RecentWorldWritebackLog2Text;
        logs.RecentWorldWritebackLog3Text = RecentWorldWritebackLog3Text;
        logs.RecentLaunchContextLog1Text = RecentLaunchContextLog1Text;
        logs.RecentLaunchContextLog2Text = RecentLaunchContextLog2Text;
        logs.RecentLaunchContextLog3Text = RecentLaunchContextLog3Text;
        logs.RecentExpeditionLog1Text = _runtimeEconomyState != null ? _runtimeEconomyState.GetRecentExpeditionLogText(0) : "None";
        logs.RecentExpeditionLog2Text = _runtimeEconomyState != null ? _runtimeEconomyState.GetRecentExpeditionLogText(1) : "None";
        logs.RecentExpeditionLog3Text = _runtimeEconomyState != null ? _runtimeEconomyState.GetRecentExpeditionLogText(2) : "None";
        logs.RecentDayLog1Text = RecentDayLog1Text;
        logs.RecentDayLog2Text = RecentDayLog2Text;
        logs.RecentDayLog3Text = RecentDayLog3Text;
    }

    private string BuildSelectedEntityKindKey()
    {
        if (_selectedMarker == null || _selectedMarker.EntityData == null)
        {
            return "none";
        }

        return _selectedMarker.EntityData.Kind == WorldEntityKind.City
            ? "city"
            : _selectedMarker.EntityData.Kind == WorldEntityKind.Dungeon
                ? "dungeon"
                : _selectedMarker.EntityData.Kind.ToString().ToLowerInvariant();
    }

    private bool TryConsumeProjectedLaunchContext(string routeId, out ExpeditionStartContext context)
    {
        string normalizedRouteId = NormalizeRouteChoiceId(routeId);
        PrototypeWorldDispatchBriefingSnapshot briefing = BuildDispatchBriefingSnapshot(
            _currentHomeCityId,
            _currentDungeonId,
            normalizedRouteId,
            true,
            true);
        PrototypeWorldSnapshot snapshot = BuildWorldSnapshot(_currentHomeCityId, _currentDungeonId, normalizedRouteId, briefing);
        context = BuildProjectedExpeditionStartContext(snapshot, briefing, true);

        if (briefing == null || !briefing.CommitAllowed || string.IsNullOrEmpty(normalizedRouteId))
        {
            AppendWorldLaunchContextLog(WorldLaunchRecordType.LaunchBlocked, snapshot, context, false, context.LaunchLockSummaryText);
            return false;
        }

        _latestExpeditionStartContext = CopyExpeditionStartContext(context);
        AppendWorldLaunchContextLog(WorldLaunchRecordType.LaunchAllowed, snapshot, context, true, context.WorldModifierSummaryText);
        return true;
    }

    private void RecordFailedProjectedLaunchContext(string routeId, string failureSummary)
    {
        string normalizedRouteId = NormalizeRouteChoiceId(routeId);
        PrototypeWorldDispatchBriefingSnapshot briefing = BuildDispatchBriefingSnapshot(
            _currentHomeCityId,
            _currentDungeonId,
            normalizedRouteId,
            true,
            true);
        PrototypeWorldSnapshot snapshot = BuildWorldSnapshot(_currentHomeCityId, _currentDungeonId, normalizedRouteId, briefing);
        ExpeditionStartContext context = BuildProjectedExpeditionStartContext(snapshot, briefing, true);
        AppendWorldLaunchContextLog(WorldLaunchRecordType.LaunchFailed, snapshot, context, false, failureSummary);
        CaptureLatestExpeditionStartContext();
    }

    private void ResetWorldSnapshotProjectionState()
    {
        _recentWorldLaunchContextLogs.Clear();
    }

    private string ResolveWorldSnapshotRouteId(string cityId, string dungeonId)
    {
        string routeId = ResolveExpeditionContextRouteId();
        if (string.IsNullOrEmpty(routeId) && !string.IsNullOrEmpty(cityId) && !string.IsNullOrEmpty(dungeonId))
        {
            routeId = ResolveDispatchRecommendedRouteId(cityId, dungeonId);
        }

        return NormalizeRouteChoiceId(routeId);
    }

    private string BuildWorldSnapshotSupplySummaryText(string cityId, PrototypeWorldDispatchBriefingSnapshot briefing)
    {
        if (string.IsNullOrEmpty(cityId))
        {
            return "None";
        }

        int stock = GetCityManaShardStock(cityId);
        int carry = _runtimeEconomyState != null ? _runtimeEconomyState.GetReadyPartyCarryCapacityForCity(cityId) : 0;
        string partyRoles = briefing != null && IsMeaningfulSnapshotText(briefing.PartyRoleSummary)
            ? briefing.PartyRoleSummary
            : "No party roles";
        return "Stock " + BuildLootAmountText(stock) + " | Carry " + carry + " | Roles " + partyRoles;
    }

    private string BuildWorldSnapshotAftermathEchoText(string cityId, string dungeonId, PrototypeWorldActiveExpeditionSnapshot activeSnapshot)
    {
        if (_runtimeEconomyState != null && !string.IsNullOrEmpty(cityId))
        {
            string cityWriteback = _runtimeEconomyState.GetLatestWorldWritebackSummaryForCity(cityId);
            if (IsMeaningfulSnapshotText(cityWriteback))
            {
                return cityWriteback;
            }

            string cityExpeditionStatus = _runtimeEconomyState.GetExpeditionStatusTextForCity(cityId);
            if (IsMeaningfulSnapshotText(cityExpeditionStatus))
            {
                return cityExpeditionStatus;
            }
        }

        if (_runtimeEconomyState != null && !string.IsNullOrEmpty(dungeonId))
        {
            string dungeonOutcome = _runtimeEconomyState.GetDungeonLastWorldOutcomeText(dungeonId);
            if (IsMeaningfulSnapshotText(dungeonOutcome))
            {
                return dungeonOutcome;
            }

            string dungeonStatus = _runtimeEconomyState.GetExpeditionStatusTextForDungeon(dungeonId);
            if (IsMeaningfulSnapshotText(dungeonStatus))
            {
                return dungeonStatus;
            }
        }

        if (activeSnapshot != null && IsMeaningfulSnapshotText(activeSnapshot.TravelSummary.DepartureEchoText) &&
            !activeSnapshot.TravelSummary.DepartureEchoText.StartsWith("No "))
        {
            return activeSnapshot.TravelSummary.DepartureEchoText;
        }

        return "No recent aftermath is shaping the next launch.";
    }

    private PrototypeWorldModifierSummary BuildWorldModifierSummary(PrototypeWorldSnapshot snapshot, PrototypeWorldDispatchBriefingSnapshot briefing)
    {
        PrototypeWorldModifierSummary modifier = new PrototypeWorldModifierSummary();
        if (snapshot == null)
        {
            return modifier;
        }

        string needText = snapshot.CitySummary.NeedPressureText;
        string readinessText = snapshot.CitySummary.ReadinessText;
        string availabilityText = snapshot.DungeonSummary.AvailabilityText;
        string aftermathText = snapshot.RecentAftermathEchoText;

        AddWorldModifierKeyIfMissing(modifier.ModifierKeys, BuildNeedModifierKey(needText));
        AddWorldModifierKeyIfMissing(modifier.ModifierKeys, BuildReadinessModifierKey(readinessText));
        AddWorldModifierKeyIfMissing(modifier.ModifierKeys, BuildAftermathModifierKey(aftermathText));
        AddWorldModifierKeyIfMissing(modifier.ModifierKeys, BuildAvailabilityModifierKey(availabilityText));
        if (briefing != null && snapshot.SelectedRouteId == briefing.RecommendationRouteId && IsMeaningfulSnapshotText(snapshot.SelectedRouteId))
        {
            AddWorldModifierKeyIfMissing(modifier.ModifierKeys, "following_recommendation");
        }
        else if (briefing != null && IsMeaningfulSnapshotText(snapshot.SelectedRouteId) && IsMeaningfulSnapshotText(briefing.RecommendationRouteId))
        {
            AddWorldModifierKeyIfMissing(modifier.ModifierKeys, "off_recommendation_route");
        }

        modifier.BlockedReasonSummaryText = briefing != null && briefing.HardBlockerKeys.Count > 0
            ? briefing.LaunchLockSummaryText
            : "None";

        modifier.RecommendationHintText = BuildWorldRecommendationHintText(snapshot, briefing, modifier.BlockedReasonSummaryText);
        modifier.SummaryText = BuildWorldModifierSummaryText(snapshot, modifier, availabilityText, aftermathText, needText, readinessText);
        return modifier;
    }

    private string BuildWorldRecommendationHintText(PrototypeWorldSnapshot snapshot, PrototypeWorldDispatchBriefingSnapshot briefing, string blockedReasonText)
    {
        if (IsMeaningfulSnapshotText(blockedReasonText) && blockedReasonText != "None")
        {
            return "Dispatch is blocked: " + blockedReasonText;
        }

        string aftermathText = snapshot != null ? snapshot.RecentAftermathEchoText.ToLowerInvariant() : string.Empty;
        if (aftermathText.Contains("defeat") || aftermathText.Contains("failed"))
        {
            return "Recent defeat fallout is still on the board, so rebuild on the steadier answer before the next push.";
        }

        if (aftermathText.Contains("retreat") || aftermathText.Contains("contested"))
        {
            return "Recent retreat fallout is still active, so respect recovery and avoid another forced push.";
        }

        if (briefing != null && IsMeaningfulSnapshotText(snapshot.SelectedRouteId) && IsMeaningfulSnapshotText(briefing.RecommendationRouteId) && snapshot.SelectedRouteId != briefing.RecommendationRouteId)
        {
            return "Current launch is off the recommended route, so the board expects extra risk drift.";
        }

        if (snapshot != null && IsMeaningfulSnapshotText(snapshot.DungeonSummary.AvailabilityText) && snapshot.DungeonSummary.AvailabilityText.ToLowerInvariant().Contains("elevated threat"))
        {
            return "Dungeon threat remains elevated, so the next launch should follow the current danger readback.";
        }

        return "Current pressure, readiness, and route answer are aligned for the next dispatch.";
    }

    private string BuildWorldModifierSummaryText(PrototypeWorldSnapshot snapshot, PrototypeWorldModifierSummary modifier, string availabilityText, string aftermathText, string needText, string readinessText)
    {
        List<string> parts = new List<string>();
        string answerText = snapshot != null && IsMeaningfulSnapshotText(snapshot.SelectedRouteLabel)
            ? snapshot.SelectedRouteLabel
            : snapshot != null && IsMeaningfulSnapshotText(snapshot.RecommendedRouteLabel)
                ? snapshot.RecommendedRouteLabel
                : string.Empty;

        if (IsMeaningfulSnapshotText(needText))
        {
            parts.Add("Need " + needText);
        }

        if (IsMeaningfulSnapshotText(readinessText))
        {
            parts.Add("Readiness " + readinessText);
        }

        if (IsMeaningfulSnapshotText(answerText))
        {
            parts.Add("Answer " + answerText);
        }

        if (IsMeaningfulSnapshotText(availabilityText))
        {
            parts.Add(availabilityText);
        }

        if (IsMeaningfulSnapshotText(aftermathText) && aftermathText != "No recent aftermath is shaping the next launch.")
        {
            parts.Add("Aftermath " + aftermathText);
        }

        if (parts.Count == 0)
        {
            return "None";
        }

        return string.Join(" | ", parts.ToArray());
    }

    private string BuildWorldSnapshotSummaryText(PrototypeWorldSnapshot snapshot)
    {
        if (snapshot == null || !IsMeaningfulSnapshotText(snapshot.SelectedCityLabel) || !IsMeaningfulSnapshotText(snapshot.SelectedDungeonLabel))
        {
            return "Day " + (snapshot != null ? snapshot.WorldDay : 0) + " | No launch context is currently projected.";
        }

        string routeText = IsMeaningfulSnapshotText(snapshot.SelectedRouteLabel)
            ? snapshot.SelectedRouteLabel
            : IsMeaningfulSnapshotText(snapshot.RecommendedRouteLabel)
                ? snapshot.RecommendedRouteLabel + " (recommended)"
                : "Route Pending";
        string modifierText = snapshot.ModifierSummary != null && IsMeaningfulSnapshotText(snapshot.ModifierSummary.RecommendationHintText)
            ? snapshot.ModifierSummary.RecommendationHintText
            : "Launch context pending.";
        string evidenceText = IsMeaningfulSnapshotText(snapshot.RecentAftermathEchoText) &&
                              snapshot.RecentAftermathEchoText != "No recent aftermath is shaping the next launch."
            ? snapshot.RecentAftermathEchoText
            : IsMeaningfulSnapshotText(snapshot.CitySummary.LastWritebackText)
                ? snapshot.CitySummary.LastWritebackText
                : "No recent return evidence.";
        return "Day " + snapshot.WorldDay +
               " | " + snapshot.SelectedCityLabel +
               " pressure board" +
               " | Need " + snapshot.CitySummary.NeedPressureText +
               " / Readiness " + snapshot.CitySummary.ReadinessText +
               " | Answer " + routeText +
               " | Evidence " + evidenceText +
               " | Next " + modifierText;
    }

    private string BuildCurrentBlockedLaunchReasonSummaryText()
    {
        LaunchReadiness launchReadiness = BuildProjectedExpeditionLaunchReadiness();
        return launchReadiness.GateResult != null && launchReadiness.GateResult.BlockedReason != null
            ? launchReadiness.GateResult.BlockedReason.SummaryText
            : "None";
    }

    private string BuildDispatchWorldModifierSummaryText(string cityId, string dungeonId, string selectedRouteId, PrototypeWorldDispatchBriefingSnapshot briefing)
    {
        return BuildWorldSnapshot(cityId, dungeonId, selectedRouteId, briefing).ModifierSummary.SummaryText;
    }

    private string BuildDispatchContinuityHintText(string cityId, string dungeonId, string selectedRouteId, PrototypeWorldDispatchBriefingSnapshot briefing)
    {
        return BuildWorldSnapshot(cityId, dungeonId, selectedRouteId, briefing).ModifierSummary.RecommendationHintText;
    }

    private string GetRecentLaunchContextLogText(int index)
    {
        return index >= 0 && index < _recentWorldLaunchContextLogs.Count
            ? _recentWorldLaunchContextLogs[index].DisplayText
            : "None";
    }

    private void AppendWorldLaunchContextLog(WorldLaunchRecordType recordType, PrototypeWorldSnapshot snapshot, ExpeditionStartContext context, bool allowed, string overrideSummary)
    {
        WorldLaunchContextLogRecord record = new WorldLaunchContextLogRecord();
        record.RecordType = recordType;
        record.WorldDay = snapshot != null ? snapshot.WorldDay : (_runtimeEconomyState != null ? _runtimeEconomyState.WorldDayCount : 0);
        record.CityId = context != null ? context.StartCityId : string.Empty;
        record.DungeonId = context != null ? context.DungeonId : string.Empty;
        record.PartyId = context != null ? context.PartyId : string.Empty;
        record.RouteId = context != null ? context.SelectedRouteId : string.Empty;
        record.Allowed = allowed;
        record.ModifierSummaryText = !string.IsNullOrEmpty(overrideSummary)
            ? overrideSummary
            : context != null && IsMeaningfulSnapshotText(context.WorldModifierSummaryText)
                ? context.WorldModifierSummaryText
                : "None";

        string typeLabel = recordType == WorldLaunchRecordType.LaunchAllowed
            ? "Launch Ready"
            : recordType == WorldLaunchRecordType.LaunchBlocked
                ? "Launch Blocked"
                : "Launch Failed";
        string contextText = context != null && IsMeaningfulSnapshotText(context.ContextSummaryText)
            ? context.ContextSummaryText
            : snapshot != null
                ? snapshot.SnapshotSummaryText
                : "No launch context.";
        string detailText = IsMeaningfulSnapshotText(record.ModifierSummaryText)
            ? record.ModifierSummaryText
            : context != null && IsMeaningfulSnapshotText(context.LaunchLockSummaryText)
                ? context.LaunchLockSummaryText
                : "None";
        record.DisplayText = "Day " + record.WorldDay + " | " + typeLabel + " | " + contextText + " | " + detailText;

        _recentWorldLaunchContextLogs.Insert(0, record);
        while (_recentWorldLaunchContextLogs.Count > RecentLaunchContextLogLimit)
        {
            _recentWorldLaunchContextLogs.RemoveAt(_recentWorldLaunchContextLogs.Count - 1);
        }

        if (_runtimeEconomyState != null)
        {
            _runtimeEconomyState.RecordRecentExpeditionContextLog(record.DisplayText);
        }
    }

    private static void AddWorldModifierKeyIfMissing(List<string> keys, string key)
    {
        if (keys == null || string.IsNullOrEmpty(key) || keys.Contains(key))
        {
            return;
        }

        keys.Add(key);
    }

    private static string BuildNeedModifierKey(string needText)
    {
        if (string.IsNullOrEmpty(needText) || needText == "None")
        {
            return string.Empty;
        }

        string lowered = needText.ToLowerInvariant();
        if (lowered.Contains("urgent"))
        {
            return "need_urgent";
        }

        if (lowered.Contains("stable"))
        {
            return "need_stable";
        }

        return "need_active";
    }

    private static string BuildReadinessModifierKey(string readinessText)
    {
        if (string.IsNullOrEmpty(readinessText) || readinessText == "None")
        {
            return string.Empty;
        }

        string lowered = readinessText.ToLowerInvariant();
        if (lowered.Contains("strained"))
        {
            return "readiness_strained";
        }

        if (lowered.Contains("recover"))
        {
            return "readiness_recovering";
        }

        if (lowered.Contains("ready"))
        {
            return "readiness_ready";
        }

        return string.Empty;
    }

    private static string BuildAftermathModifierKey(string aftermathText)
    {
        if (string.IsNullOrEmpty(aftermathText) || aftermathText == "None")
        {
            return string.Empty;
        }

        string lowered = aftermathText.ToLowerInvariant();
        if (lowered.Contains("defeat") || lowered.Contains("failed"))
        {
            return "aftermath_defeat";
        }

        if (lowered.Contains("retreat") || lowered.Contains("contested"))
        {
            return "aftermath_retreat";
        }

        if (lowered.Contains("clear") || lowered.Contains("stabilized") || lowered.Contains("successful"))
        {
            return "aftermath_clear";
        }

        return string.Empty;
    }

    private static string BuildAvailabilityModifierKey(string availabilityText)
    {
        if (string.IsNullOrEmpty(availabilityText) || availabilityText == "None")
        {
            return string.Empty;
        }

        string lowered = availabilityText.ToLowerInvariant();
        if (lowered.Contains("elevated threat"))
        {
            return "availability_elevated_threat";
        }

        if (lowered.Contains("caution"))
        {
            return "availability_caution";
        }

        if (lowered.Contains("stabilization"))
        {
            return "availability_stabilized";
        }

        if (lowered.Contains("available"))
        {
            return "availability_open";
        }

        return string.Empty;
    }

    private static bool IsMeaningfulSnapshotText(string text)
    {
        return !string.IsNullOrEmpty(text) && text != "None";
    }
}
