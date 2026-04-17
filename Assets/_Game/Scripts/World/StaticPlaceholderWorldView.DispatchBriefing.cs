using System.Collections.Generic;

public sealed partial class StaticPlaceholderWorldView
{
    private enum PrototypeWorldDispatchLaunchLockState
    {
        Clear,
        Warning,
        Locked
    }

    private sealed class PrototypeWorldDispatchFitSummary
    {
        public string RouteFitLabel = "None";
        public string ProjectedRiskLabel = "None";
        public string ProjectedRewardLabel = "None";
        public string ProjectedDurationText = "None";
    }

    private sealed class PrototypeWorldDispatchBriefingSnapshot
    {
        public string CityId = string.Empty;
        public string CityLabel = "None";
        public string PartyId = string.Empty;
        public string PartyLabel = "None";
        public string PartyRoleSummary = "None";
        public string DungeonId = string.Empty;
        public string DungeonLabel = "None";
        public string SelectedRouteId = string.Empty;
        public string SelectedRouteLabel = "None";
        public string RecommendationRouteId = string.Empty;
        public string RecommendationRouteLabel = "None";
        public string RouteFitLabel = "None";
        public string ReadinessLabel = "None";
        public string RecoveryLabel = "None";
        public string ProjectedRiskLabel = "None";
        public string ProjectedRewardLabel = "None";
        public string ProjectedDurationText = "None";
        public readonly List<string> HardBlockerKeys = new List<string>();
        public readonly List<string> SoftWarningKeys = new List<string>();
        public bool CanOpenPlanner;
        public bool CommitAllowed;
        public PrototypeWorldDispatchLaunchLockState LaunchLockState;
        public string PartySummaryText = "None";
        public string BriefingSummaryText = "None";
        public string RouteFitSummaryText = "None";
        public string LaunchLockSummaryText = "None";
        public string ProjectedOutcomeSummaryText = "None";
    }

    public bool CanOpenSelectedCityDungeonAction => BuildCurrentDispatchBriefingSnapshot().CanOpenPlanner;
    public string SelectedDispatchPartyText => BuildCurrentDispatchBriefingSnapshot().PartySummaryText;
    public string SelectedDispatchBriefingText => BuildCurrentDispatchBriefingSnapshot().BriefingSummaryText;
    public string SelectedDispatchRouteFitText => BuildCurrentDispatchBriefingSnapshot().RouteFitSummaryText;
    public string SelectedDispatchLaunchLockText => BuildCurrentDispatchBriefingSnapshot().LaunchLockSummaryText;
    public string SelectedDispatchProjectedOutcomeText => BuildCurrentDispatchBriefingSnapshot().ProjectedOutcomeSummaryText;

    private PrototypeWorldDispatchBriefingSnapshot BuildCurrentDispatchBriefingSnapshot()
    {
        string cityId = ResolveDispatchBriefingCityId();
        string dungeonId = ResolveDispatchBriefingDungeonId(cityId);
        return BuildDispatchBriefingSnapshot(cityId, dungeonId, string.Empty, false);
    }

    private PrototypeWorldDispatchBriefingSnapshot BuildCurrentPlannerDispatchBriefingSnapshot()
    {
        return BuildDispatchBriefingSnapshot(_currentHomeCityId, _currentDungeonId, _selectedRouteChoiceId, true);
    }

    private PrototypeWorldDispatchBriefingSnapshot BuildDispatchBriefingSnapshot(string cityId, string dungeonId, string selectedRouteId, bool requireSelectedRoute)
    {
        PrototypeWorldDispatchBriefingSnapshot snapshot = new PrototypeWorldDispatchBriefingSnapshot();
        snapshot.CityId = cityId ?? string.Empty;
        snapshot.CityLabel = ResolveDispatchEntityDisplayName(cityId);
        snapshot.DungeonId = dungeonId ?? string.Empty;
        snapshot.DungeonLabel = ResolveDispatchEntityDisplayName(dungeonId);
        snapshot.RecommendationRouteId = ResolveDispatchRecommendedRouteId(cityId, dungeonId);
        snapshot.RecommendationRouteLabel = BuildDispatchRouteLabel(dungeonId, snapshot.RecommendationRouteId);

        string resolvedRouteId = NormalizeRouteChoiceId(selectedRouteId);
        if (string.IsNullOrEmpty(resolvedRouteId) && !requireSelectedRoute)
        {
            resolvedRouteId = snapshot.RecommendationRouteId;
        }

        snapshot.SelectedRouteId = resolvedRouteId;
        snapshot.SelectedRouteLabel = BuildDispatchRouteLabel(dungeonId, resolvedRouteId);
        snapshot.PartyId = ResolveDispatchReadyPartyId(cityId);
        snapshot.PartyLabel = string.IsNullOrEmpty(snapshot.PartyId) ? "None" : snapshot.PartyId;
        snapshot.PartyRoleSummary = ResolveDispatchPartyRoleSummary(snapshot.PartyId);
        snapshot.ReadinessLabel = string.IsNullOrEmpty(cityId) ? "None" : GetDispatchReadinessText(cityId);
        snapshot.RecoveryLabel = string.IsNullOrEmpty(cityId) ? "None" : BuildDispatchRecoveryProgressText(cityId);
        snapshot.CanOpenPlanner = !string.IsNullOrEmpty(cityId) && !string.IsNullOrEmpty(dungeonId) && !string.IsNullOrEmpty(snapshot.PartyId);

        if (string.IsNullOrEmpty(cityId))
        {
            snapshot.HardBlockerKeys.Add("no_city_selected");
        }

        if (!string.IsNullOrEmpty(cityId) && string.IsNullOrEmpty(dungeonId))
        {
            snapshot.HardBlockerKeys.Add("dungeon_unavailable");
        }

        if (!string.IsNullOrEmpty(cityId) && string.IsNullOrEmpty(snapshot.PartyId))
        {
            snapshot.HardBlockerKeys.Add(GetDispatchPartyBlockerKey(cityId));
        }

        if (!string.IsNullOrEmpty(dungeonId) && requireSelectedRoute && string.IsNullOrEmpty(resolvedRouteId))
        {
            snapshot.HardBlockerKeys.Add("no_route_selected");
        }
        else if (!string.IsNullOrEmpty(resolvedRouteId) && GetRouteTemplateById(dungeonId, resolvedRouteId) == null)
        {
            snapshot.HardBlockerKeys.Add("route_unavailable");
        }

        PrototypeWorldDispatchFitSummary fitSummary = BuildDispatchFitSummary(cityId, dungeonId, resolvedRouteId, snapshot.PartyId);
        snapshot.RouteFitLabel = fitSummary.RouteFitLabel;
        snapshot.ProjectedRiskLabel = fitSummary.ProjectedRiskLabel;
        snapshot.ProjectedRewardLabel = fitSummary.ProjectedRewardLabel;
        snapshot.ProjectedDurationText = fitSummary.ProjectedDurationText;

        if (!string.IsNullOrEmpty(resolvedRouteId) && resolvedRouteId == RiskyRouteId)
        {
            snapshot.SoftWarningKeys.Add("risky_route");
        }

        int recommendedPower = _runtimeEconomyState != null && !string.IsNullOrEmpty(dungeonId)
            ? _runtimeEconomyState.GetRecommendedPower(dungeonId)
            : 0;
        int partyPower = _runtimeEconomyState != null && !string.IsNullOrEmpty(cityId)
            ? _runtimeEconomyState.GetReadyPartyPowerForCity(cityId)
            : 0;
        if (recommendedPower > 0 && partyPower > 0 && partyPower < recommendedPower)
        {
            snapshot.SoftWarningKeys.Add("under_recommended_power");
        }

        if (!string.IsNullOrEmpty(cityId) && GetDispatchReadinessState(cityId) != DispatchReadinessState.Ready)
        {
            snapshot.SoftWarningKeys.Add("low_readiness");
        }

        string partyResultEcho = _runtimeEconomyState != null && !string.IsNullOrEmpty(cityId)
            ? _runtimeEconomyState.GetReadyPartyLastResultSummaryForCity(cityId)
            : "None";
        if (ContainsDispatchFailureEcho(partyResultEcho))
        {
            snapshot.SoftWarningKeys.Add("recent_failure_chain");
        }

        snapshot.CommitAllowed = snapshot.HardBlockerKeys.Count == 0;
        snapshot.LaunchLockState = snapshot.HardBlockerKeys.Count > 0
            ? PrototypeWorldDispatchLaunchLockState.Locked
            : snapshot.SoftWarningKeys.Count > 0
                ? PrototypeWorldDispatchLaunchLockState.Warning
                : PrototypeWorldDispatchLaunchLockState.Clear;
        snapshot.PartySummaryText = BuildDispatchPartySummaryText(snapshot, partyResultEcho);
        snapshot.BriefingSummaryText = BuildDispatchBriefingSummaryText(snapshot, requireSelectedRoute);
        snapshot.RouteFitSummaryText = BuildDispatchRouteFitSummaryText(snapshot);
        snapshot.LaunchLockSummaryText = BuildDispatchLaunchLockSummaryText(snapshot);
        snapshot.ProjectedOutcomeSummaryText = BuildDispatchProjectedOutcomeSummaryText(snapshot, cityId, dungeonId, resolvedRouteId);
        return snapshot;
    }

    private string ResolveDispatchBriefingCityId()
    {
        if (_selectedMarker == null)
        {
            return string.Empty;
        }

        if (_selectedMarker.EntityData.Kind == WorldEntityKind.City)
        {
            return _selectedMarker.EntityData.Id;
        }

        return _selectedMarker.EntityData.Kind == WorldEntityKind.Dungeon
            ? _selectedMarker.EntityData.LinkedCityId ?? string.Empty
            : string.Empty;
    }

    private string ResolveDispatchBriefingDungeonId(string cityId)
    {
        if (_selectedMarker == null)
        {
            return string.Empty;
        }

        if (_selectedMarker.EntityData.Kind == WorldEntityKind.Dungeon)
        {
            return _selectedMarker.EntityData.Id;
        }

        return _selectedMarker.EntityData.Kind == WorldEntityKind.City
            ? GetLinkedDungeonIdForCity(cityId)
            : string.Empty;
    }

    private string ResolveDispatchEntityDisplayName(string entityId)
    {
        if (string.IsNullOrEmpty(entityId) || _worldData == null || _worldData.Entities == null)
        {
            return "None";
        }

        for (int i = 0; i < _worldData.Entities.Length; i++)
        {
            WorldEntityData entity = _worldData.Entities[i];
            if (entity != null && entity.Id == entityId)
            {
                return string.IsNullOrEmpty(entity.DisplayName) ? entityId : entity.DisplayName;
            }
        }

        return entityId;
    }

    private string ResolveDispatchRecommendedRouteId(string cityId, string dungeonId)
    {
        return string.IsNullOrEmpty(cityId) || string.IsNullOrEmpty(dungeonId)
            ? string.Empty
            : GetRecommendedRouteId(cityId, dungeonId);
    }

    private string BuildDispatchRouteLabel(string dungeonId, string routeId)
    {
        DungeonRouteTemplate template = GetRouteTemplateById(dungeonId, routeId);
        return template == null ? "None" : template.RouteLabel + " | " + template.RiskLabel + " Risk";
    }

    private string ResolveDispatchReadyPartyId(string cityId)
    {
        if (string.IsNullOrEmpty(cityId))
        {
            return string.Empty;
        }

        if (IsExpeditionPrepRouteSelectionActive() && _currentHomeCityId == cityId && _activeDungeonParty != null && !string.IsNullOrEmpty(_activeDungeonParty.PartyId))
        {
            return _activeDungeonParty.PartyId;
        }

        return _runtimeEconomyState != null ? _runtimeEconomyState.GetIdlePartyIdInCity(cityId) : string.Empty;
    }

    private string ResolveDispatchPartyRoleSummary(string partyId)
    {
        if (_activeDungeonParty != null && _activeDungeonParty.PartyId == partyId && _activeDungeonParty.Members != null && _activeDungeonParty.Members.Length > 0)
        {
            string text = string.Empty;
            for (int i = 0; i < _activeDungeonParty.Members.Length; i++)
            {
                DungeonPartyMemberRuntimeData member = _activeDungeonParty.Members[i];
                if (member == null || string.IsNullOrEmpty(member.RoleLabel))
                {
                    continue;
                }

                text = string.IsNullOrEmpty(text) ? member.RoleLabel : text + " / " + member.RoleLabel;
            }

            if (!string.IsNullOrEmpty(text))
            {
                return text;
            }
        }

        return ResolveRuntimePartyRoleSummary(partyId);
    }

    private string GetDispatchPartyBlockerKey(string cityId)
    {
        return _runtimeEconomyState != null && _runtimeEconomyState.GetActiveExpeditionCountFromCity(cityId) > 0
            ? "party_on_expedition"
            : "no_ready_party";
    }

    private PrototypeWorldDispatchFitSummary BuildDispatchFitSummary(string cityId, string dungeonId, string routeId, string partyId)
    {
        PrototypeWorldDispatchFitSummary summary = new PrototypeWorldDispatchFitSummary();
        if (string.IsNullOrEmpty(dungeonId))
        {
            summary.RouteFitLabel = "No linked dungeon is available for briefing.";
            return summary;
        }

        if (string.IsNullOrEmpty(routeId))
        {
            summary.RouteFitLabel = "Choose a route to compare fit and launch pressure.";
            return summary;
        }

        DungeonRouteTemplate template = GetRouteTemplateById(dungeonId, routeId);
        if (template == null)
        {
            summary.RouteFitLabel = "Selected route data is unavailable.";
            return summary;
        }

        summary.ProjectedRiskLabel = template.RiskLabel + " risk";
        summary.ProjectedRewardLabel = template.RewardPreview;
        int durationDays = _runtimeEconomyState != null ? _runtimeEconomyState.GetExpeditionDurationDays(dungeonId) : 0;
        summary.ProjectedDurationText = durationDays > 0 ? BuildDayCountText(durationDays) : "Unknown duration";

        if (string.IsNullOrEmpty(partyId) || _runtimeEconomyState == null)
        {
            summary.RouteFitLabel = "No ready party is available to compare against this route.";
            return summary;
        }

        int recommendedPower = _runtimeEconomyState.GetRecommendedPower(dungeonId);
        int partyPower = _runtimeEconomyState.GetReadyPartyPowerForCity(cityId);
        DispatchReadinessState readiness = GetDispatchReadinessState(cityId);
        bool recentFailure = ContainsDispatchFailureEcho(_runtimeEconomyState.GetReadyPartyLastResultSummaryForCity(cityId));
        string routeTone = BuildScenarioSentenceText(
            TryBuildRouteChooseWhenTextFromContent(cityId, dungeonId, routeId),
            TryBuildRouteWatchOutTextFromContent(cityId, dungeonId, routeId));
        if (!HasText(routeTone))
        {
            routeTone = routeId == SafeRouteId
                ? "Safe route keeps the launch steadier."
                : "Risky route leans into the higher payout lane.";
        }

        string powerTone = recommendedPower > 0
            ? partyPower >= recommendedPower
                ? "Party power meets the recommendation."
                : "Party power trails the recommendation."
            : "No power line is defined.";
        string readinessTone = readiness == DispatchReadinessState.Ready
            ? "Readiness is clear."
            : readiness == DispatchReadinessState.Recovering
                ? "Readiness is still recovering."
                : "Readiness is strained.";
        string echoTone = recentFailure
            ? "Recent failure suggests a steadier launch."
            : "Last run echo is stable.";
        string partyFitTone = BuildRuntimePartyRouteFitText(partyId, dungeonId, routeId);
        summary.RouteFitLabel = BuildScenarioSentenceText(routeTone, powerTone, readinessTone, echoTone, partyFitTone);
        return summary;
    }

    private string BuildDispatchPartySummaryText(PrototypeWorldDispatchBriefingSnapshot snapshot, string partyResultEcho)
    {
        if (snapshot == null || string.IsNullOrEmpty(snapshot.CityId))
        {
            return "None";
        }

        if (string.IsNullOrEmpty(snapshot.PartyId) || _runtimeEconomyState == null)
        {
            string activeSummary = _runtimeEconomyState != null ? _runtimeEconomyState.GetActivePartyStatusTextForCity(snapshot.CityId) : "None";
            return activeSummary != "None"
                ? "No ready party | Active: " + activeSummary
                : "No ready party in " + snapshot.CityLabel + ".";
        }

        PrototypeRpgPartyRuntimeResolveSurface partySurface = BuildRuntimePartyResolveSurface(snapshot.PartyId);
        int partyPower = _runtimeEconomyState.GetReadyPartyPowerForCity(snapshot.CityId);
        int carryCapacity = _runtimeEconomyState.GetReadyPartyCarryCapacityForCity(snapshot.CityId);
        string lastEcho = string.IsNullOrEmpty(partyResultEcho) || partyResultEcho == "None" || partyResultEcho.StartsWith("Ready in ")
            ? string.Empty
            : " | Last: " + partyResultEcho;
        if (partySurface == null)
        {
            return snapshot.PartyId + " | " + snapshot.PartyRoleSummary + " | Power " + partyPower + " | Carry " + carryCapacity + lastEcho;
        }

        return snapshot.PartyId +
            " | " + partySurface.ArchetypeLabel +
            " | " + partySurface.PromotionStateLabel +
            " | Power " + partyPower +
            " | Carry " + carryCapacity +
            " | " + partySurface.StrengthSummaryText +
            lastEcho;
    }

    private string BuildDispatchBriefingSummaryText(PrototypeWorldDispatchBriefingSnapshot snapshot, bool requireSelectedRoute)
    {
        if (snapshot == null || string.IsNullOrEmpty(snapshot.CityId))
        {
            return "None";
        }

        string partyText = string.IsNullOrEmpty(snapshot.PartyId) ? "No ready party" : snapshot.PartyId;
        string routeText = string.IsNullOrEmpty(snapshot.SelectedRouteId)
            ? requireSelectedRoute
                ? "Route pending"
                : snapshot.RecommendationRouteLabel
            : snapshot.SelectedRouteLabel;
        string routeSuffix = !string.IsNullOrEmpty(snapshot.SelectedRouteId) && snapshot.SelectedRouteId == snapshot.RecommendationRouteId
            ? " (recommended)"
            : string.Empty;
        string continuityHint = BuildDispatchContinuityHintText(snapshot.CityId, snapshot.DungeonId, snapshot.SelectedRouteId, snapshot);
        string continuitySuffix = string.IsNullOrEmpty(continuityHint) || continuityHint == "None" ? string.Empty : " | " + continuityHint;
        return snapshot.CityLabel + " -> " + snapshot.DungeonLabel + " | " + partyText + " | " + routeText + routeSuffix + continuitySuffix;
    }

    private string BuildDispatchRouteFitSummaryText(PrototypeWorldDispatchBriefingSnapshot snapshot)
    {
        if (snapshot == null)
        {
            return "None";
        }

        string worldModifierText = BuildDispatchWorldModifierSummaryText(snapshot.CityId, snapshot.DungeonId, snapshot.SelectedRouteId, snapshot);
        string baseText = string.IsNullOrEmpty(snapshot.RouteFitLabel) ? "None" : snapshot.RouteFitLabel;
        if (string.IsNullOrEmpty(worldModifierText) || worldModifierText == "None")
        {
            return baseText;
        }

        return baseText == "None"
            ? worldModifierText
            : baseText + " | " + worldModifierText;
    }

    private string BuildDispatchLaunchLockSummaryText(PrototypeWorldDispatchBriefingSnapshot snapshot)
    {
        if (snapshot == null)
        {
            return "None";
        }

        if (snapshot.HardBlockerKeys.Count > 0)
        {
            return "Locked: " + BuildDispatchReasonListText(snapshot.HardBlockerKeys, true);
        }

        if (snapshot.SoftWarningKeys.Count > 0)
        {
            return "Commit allowed with warnings: " + BuildDispatchReasonListText(snapshot.SoftWarningKeys, false);
        }

        return "Commit allowed. Launch window is clear.";
    }

    private string BuildDispatchProjectedOutcomeSummaryText(PrototypeWorldDispatchBriefingSnapshot snapshot, string cityId, string dungeonId, string routeId)
    {
        if (snapshot == null || string.IsNullOrEmpty(dungeonId) || string.IsNullOrEmpty(routeId))
        {
            return "Pick a route to compare risk, duration, and pressure impact.";
        }

        string expectedImpact = BuildExpectedNeedImpactText(cityId, dungeonId, routeId);
        string continuityHint = BuildDispatchContinuityHintText(cityId, dungeonId, routeId, snapshot);
        string continuitySuffix = string.IsNullOrEmpty(continuityHint) || continuityHint == "None" ? string.Empty : " | " + continuityHint;
        return snapshot.ProjectedRiskLabel + " | " + snapshot.ProjectedDurationText + " | " + snapshot.ProjectedRewardLabel + " | " + expectedImpact + continuitySuffix;
    }

    private string BuildDispatchReasonListText(List<string> reasonKeys, bool blockers)
    {
        if (reasonKeys == null || reasonKeys.Count == 0)
        {
            return blockers ? "No blockers." : "No warnings.";
        }

        string text = string.Empty;
        for (int i = 0; i < reasonKeys.Count; i++)
        {
            string segment = blockers
                ? BuildDispatchBlockerReasonText(reasonKeys[i])
                : BuildDispatchWarningReasonText(reasonKeys[i]);
            if (string.IsNullOrEmpty(segment))
            {
                continue;
            }

            text = string.IsNullOrEmpty(text) ? segment : text + ", " + segment;
        }

        return string.IsNullOrEmpty(text)
            ? blockers ? "No blockers." : "No warnings."
            : text;
    }

    private string BuildDispatchBlockerReasonText(string blockerKey)
    {
        switch (blockerKey)
        {
            case "no_city_selected":
                return "select a city first";
            case "dungeon_unavailable":
                return "no linked dungeon is available";
            case "party_on_expedition":
                return "the current party is still on expedition";
            case "no_ready_party":
                return "no ready party is waiting in the city";
            case "no_route_selected":
                return "choose a route before dispatch";
            case "route_unavailable":
                return "the selected route is unavailable";
            default:
                return string.IsNullOrEmpty(blockerKey) ? string.Empty : blockerKey;
        }
    }

    private string BuildDispatchWarningReasonText(string warningKey)
    {
        switch (warningKey)
        {
            case "risky_route":
                return "risky route selected";
            case "under_recommended_power":
                return "party power is below the recommendation";
            case "low_readiness":
                return "city readiness is not fully recovered";
            case "recent_failure_chain":
                return "recent expedition failure is still echoing";
            default:
                return string.IsNullOrEmpty(warningKey) ? string.Empty : warningKey;
        }
    }

    private bool ContainsDispatchFailureEcho(string text)
    {
        if (string.IsNullOrEmpty(text) || text == "None")
        {
            return false;
        }

        string lowered = text.ToLowerInvariant();
        return lowered.Contains("failed") || lowered.Contains("failing") || lowered.Contains("defeat");
    }
}


