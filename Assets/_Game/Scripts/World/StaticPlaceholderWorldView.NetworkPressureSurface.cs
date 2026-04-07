using System;
using System.Collections.Generic;
using UnityEngine;

public sealed partial class StaticPlaceholderWorldView
{
    private sealed class PrototypeWorldSimCityPressureSurfaceData
    {
        public static readonly PrototypeWorldSimCityPressureSurfaceData Empty = new PrototypeWorldSimCityPressureSurfaceData(string.Empty, "None", "stable", "None", "None", "None", "None", "None", 0, 0, 0, false);
        public readonly string CityId;
        public readonly string CityLabel;
        public readonly string StateKey;
        public readonly string PressureText;
        public readonly string RecoveryText;
        public readonly string OpportunityText;
        public readonly string BlockedText;
        public readonly string RecentShiftText;
        public readonly int RecoveryDays;
        public readonly int Severity;
        public readonly int OpportunityScore;
        public readonly bool IsOpportunity;

        public PrototypeWorldSimCityPressureSurfaceData(string cityId, string cityLabel, string stateKey, string pressureText, string recoveryText, string opportunityText, string blockedText, string recentShiftText, int recoveryDays, int severity, int opportunityScore, bool isOpportunity)
        {
            CityId = string.IsNullOrEmpty(cityId) ? string.Empty : cityId;
            CityLabel = string.IsNullOrEmpty(cityLabel) ? "None" : cityLabel;
            StateKey = string.IsNullOrEmpty(stateKey) ? "stable" : stateKey;
            PressureText = string.IsNullOrEmpty(pressureText) ? "None" : pressureText;
            RecoveryText = string.IsNullOrEmpty(recoveryText) ? "None" : recoveryText;
            OpportunityText = string.IsNullOrEmpty(opportunityText) ? "None" : opportunityText;
            BlockedText = string.IsNullOrEmpty(blockedText) ? "None" : blockedText;
            RecentShiftText = string.IsNullOrEmpty(recentShiftText) ? "None" : recentShiftText;
            RecoveryDays = Mathf.Max(0, recoveryDays);
            Severity = Mathf.Max(0, severity);
            OpportunityScore = Mathf.Max(0, opportunityScore);
            IsOpportunity = isOpportunity;
        }
    }

    private sealed class PrototypeWorldSimRoutePressureSurfaceData
    {
        public static readonly PrototypeWorldSimRoutePressureSurfaceData Empty = new PrototypeWorldSimRoutePressureSurfaceData(string.Empty, "None", "idle", "None", false, false, 0);
        public readonly string RouteId;
        public readonly string RouteLabel;
        public readonly string StateKey;
        public readonly string SummaryText;
        public readonly bool TouchesSelection;
        public readonly bool IsOpportunity;
        public readonly int Severity;

        public PrototypeWorldSimRoutePressureSurfaceData(string routeId, string routeLabel, string stateKey, string summaryText, bool touchesSelection, bool isOpportunity, int severity)
        {
            RouteId = string.IsNullOrEmpty(routeId) ? string.Empty : routeId;
            RouteLabel = string.IsNullOrEmpty(routeLabel) ? "None" : routeLabel;
            StateKey = string.IsNullOrEmpty(stateKey) ? "idle" : stateKey;
            SummaryText = string.IsNullOrEmpty(summaryText) ? "None" : summaryText;
            TouchesSelection = touchesSelection;
            IsOpportunity = isOpportunity;
            Severity = Mathf.Max(0, severity);
        }
    }

    private sealed class PrototypeWorldSimNetworkPressureSurfaceData
    {
        public static readonly PrototypeWorldSimNetworkPressureSurfaceData Empty = new PrototypeWorldSimNetworkPressureSurfaceData("stable", "None", "None", "None", "None", "None", "None", "None", PrototypeWorldSimCityPressureSurfaceData.Empty, PrototypeWorldSimRoutePressureSurfaceData.Empty);
        public readonly string OverallStateKey;
        public readonly string OverallText;
        public readonly string RecoveryText;
        public readonly string RouteText;
        public readonly string OpportunityText;
        public readonly string BlockedText;
        public readonly string PrimaryOpportunityLabel;
        public readonly string RecentShiftText;
        public readonly PrototypeWorldSimCityPressureSurfaceData SelectedCity;
        public readonly PrototypeWorldSimRoutePressureSurfaceData FocusedRoute;

        public PrototypeWorldSimNetworkPressureSurfaceData(string overallStateKey, string overallText, string recoveryText, string routeText, string opportunityText, string blockedText, string primaryOpportunityLabel, string recentShiftText, PrototypeWorldSimCityPressureSurfaceData selectedCity, PrototypeWorldSimRoutePressureSurfaceData focusedRoute)
        {
            OverallStateKey = string.IsNullOrEmpty(overallStateKey) ? "stable" : overallStateKey;
            OverallText = string.IsNullOrEmpty(overallText) ? "None" : overallText;
            RecoveryText = string.IsNullOrEmpty(recoveryText) ? "None" : recoveryText;
            RouteText = string.IsNullOrEmpty(routeText) ? "None" : routeText;
            OpportunityText = string.IsNullOrEmpty(opportunityText) ? "None" : opportunityText;
            BlockedText = string.IsNullOrEmpty(blockedText) ? "None" : blockedText;
            PrimaryOpportunityLabel = string.IsNullOrEmpty(primaryOpportunityLabel) ? "None" : primaryOpportunityLabel;
            RecentShiftText = string.IsNullOrEmpty(recentShiftText) ? "None" : recentShiftText;
            SelectedCity = selectedCity ?? PrototypeWorldSimCityPressureSurfaceData.Empty;
            FocusedRoute = focusedRoute ?? PrototypeWorldSimRoutePressureSurfaceData.Empty;
        }
    }

    private sealed class WorldBoardRoutePressureVisual
    {
        private readonly SpriteRenderer _baseRenderer;
        private readonly SpriteRenderer _innerRenderer;
        private readonly SpriteRenderer _sealRenderer;
        private readonly TextMesh _labelText;
        private readonly Color _baseColor;
        private readonly Color _innerColor;
        private readonly Color _sealColor;
        private readonly Color _labelColor;

        public WorldBoardRoutePressureVisual(SpriteRenderer baseRenderer, SpriteRenderer innerRenderer, SpriteRenderer sealRenderer, TextMesh labelText)
        {
            _baseRenderer = baseRenderer;
            _innerRenderer = innerRenderer;
            _sealRenderer = sealRenderer;
            _labelText = labelText;
            _baseColor = baseRenderer != null ? baseRenderer.color : Color.clear;
            _innerColor = innerRenderer != null ? innerRenderer.color : Color.clear;
            _sealColor = sealRenderer != null ? sealRenderer.color : Color.clear;
            _labelColor = labelText != null ? labelText.color : Color.white;
        }

        public void ApplyState(string stateKey, bool isFocused, bool isOpportunity)
        {
            Color tint = stateKey == "saturated" ? new Color(0.96f, 0.54f, 0.36f, 1f)
                : stateKey == "strained" ? new Color(0.90f, 0.74f, 0.36f, 1f)
                : stateKey == "open" ? (isOpportunity ? new Color(0.48f, 0.82f, 0.62f, 1f) : new Color(0.54f, 0.78f, 0.84f, 1f))
                : new Color(0.58f, 0.62f, 0.66f, 1f);
            float mix = stateKey == "saturated" ? 0.44f : stateKey == "strained" ? 0.30f : stateKey == "open" ? (isOpportunity ? 0.22f : 0.16f) : 0.06f;
            float innerAlpha = stateKey == "saturated" ? 0.88f : stateKey == "strained" ? 0.78f : stateKey == "open" ? (isOpportunity ? 0.64f : 0.52f) : 0.24f;
            float sealAlpha = stateKey == "saturated" ? 0.98f : stateKey == "strained" ? 0.92f : stateKey == "open" ? (isOpportunity ? 0.88f : 0.74f) : 0.42f;

            if (_baseRenderer != null)
            {
                Color baseColor = string.IsNullOrEmpty(stateKey) ? _baseColor : Color.Lerp(_baseColor, tint, mix);
                _baseRenderer.color = isFocused ? Color.Lerp(baseColor, Color.white, 0.14f) : baseColor;
            }

            if (_innerRenderer != null)
            {
                Color innerColor = string.IsNullOrEmpty(stateKey) ? _innerColor : Color.Lerp(_innerColor, tint, mix + 0.10f);
                _innerRenderer.color = new Color(innerColor.r, innerColor.g, innerColor.b, Mathf.Clamp01(innerAlpha + (isFocused ? 0.10f : 0f)));
            }

            if (_sealRenderer != null)
            {
                Color sealColor = string.IsNullOrEmpty(stateKey) ? _sealColor : Color.Lerp(_sealColor, tint, mix + 0.18f);
                if (isFocused)
                {
                    sealColor = Color.Lerp(sealColor, Color.white, 0.18f);
                }

                _sealRenderer.color = new Color(sealColor.r, sealColor.g, sealColor.b, Mathf.Clamp01(sealAlpha + (isFocused ? 0.08f : 0f)));
            }

            if (_labelText != null)
            {
                Color labelColor = string.IsNullOrEmpty(stateKey) ? _labelColor : Color.Lerp(_labelColor, tint, mix + 0.08f);
                _labelText.color = isFocused ? Color.Lerp(labelColor, Color.white, 0.16f) : labelColor;
            }
        }
    }

    private readonly Dictionary<string, PrototypeWorldSimCityPressureSurfaceData> _cityPressureByEntityId = new Dictionary<string, PrototypeWorldSimCityPressureSurfaceData>();
    private readonly Dictionary<string, PrototypeWorldSimRoutePressureSurfaceData> _routePressureByRouteId = new Dictionary<string, PrototypeWorldSimRoutePressureSurfaceData>();
    private readonly Dictionary<string, WorldBoardRoutePressureVisual> _routePulseByRouteId = new Dictionary<string, WorldBoardRoutePressureVisual>();
    private PrototypeWorldSimNetworkPressureSurfaceData _currentNetworkPressureSurface = PrototypeWorldSimNetworkPressureSurfaceData.Empty;
    private string _currentNetworkPressureSummaryText = "None";
    private string _currentRouteSaturationSummaryText = "None";
    private string _currentRecoveryForecastSummaryText = "None";
    private string _currentOpportunitySummaryText = "None";

    private void RegisterWorldRoutePulse(string routeId, WorldBoardRoutePressureVisual visual)
    {
        if (!string.IsNullOrEmpty(routeId) && visual != null)
        {
            _routePulseByRouteId[routeId] = visual;
        }
    }

    private void ResetWorldNetworkPressureState()
    {
        _cityPressureByEntityId.Clear();
        _routePressureByRouteId.Clear();
        _routePulseByRouteId.Clear();
        _currentNetworkPressureSurface = PrototypeWorldSimNetworkPressureSurfaceData.Empty;
        _currentNetworkPressureSummaryText = "None";
        _currentRouteSaturationSummaryText = "None";
        _currentRecoveryForecastSummaryText = "None";
        _currentOpportunitySummaryText = "None";
    }
    private void RefreshWorldNetworkPressureSurface(PrototypeWorldSimOperationChainSurfaceData chain)
    {
        _cityPressureByEntityId.Clear();
        _routePressureByRouteId.Clear();
        _currentNetworkPressureSurface = PrototypeWorldSimNetworkPressureSurfaceData.Empty;
        _currentNetworkPressureSummaryText = "None";
        _currentRouteSaturationSummaryText = "None";
        _currentRecoveryForecastSummaryText = "None";
        _currentOpportunitySummaryText = "None";

        if (_worldData == null || _worldData.Entities == null)
        {
            return;
        }

        string selectedCityId = GetChainCityId(chain);
        PrototypeWorldSimCityPressureSurfaceData selectedCity = PrototypeWorldSimCityPressureSurfaceData.Empty;
        for (int i = 0; i < _worldData.Entities.Length; i++)
        {
            WorldEntityData entity = _worldData.Entities[i];
            if (entity == null || entity.Kind != WorldEntityKind.City)
            {
                continue;
            }

            PrototypeWorldSimCityPressureSurfaceData cityData = BuildCityPressureSurfaceData(entity);
            _cityPressureByEntityId[entity.Id] = cityData;
            if (!string.IsNullOrEmpty(selectedCityId) && string.Equals(entity.Id, selectedCityId, StringComparison.OrdinalIgnoreCase))
            {
                selectedCity = cityData;
            }
        }

        WorldRouteData[] routes = _worldData.Routes ?? Array.Empty<WorldRouteData>();
        for (int i = 0; i < routes.Length; i++)
        {
            WorldRouteData route = routes[i];
            if (route == null || string.IsNullOrEmpty(route.Id))
            {
                continue;
            }

            _routePressureByRouteId[route.Id] = BuildRoutePressureSurfaceData(route, selectedCityId);
        }

        PrototypeWorldSimRoutePressureSurfaceData focusedRoute = ResolveFocusedRoutePressureSurface(selectedCityId);
        PrototypeWorldSimCityPressureSurfaceData primaryOpportunity = ResolvePrimaryOpportunityCityPressure(selectedCityId, selectedCity);
        string overallStateKey = ResolveOverallPressureStateKey(selectedCity, focusedRoute);
        string overallText = BuildOverallPressureSummaryText(chain, selectedCity, focusedRoute);
        string recoveryText = BuildRecoveryForecastSummaryText(selectedCity, primaryOpportunity);
        string routeText = BuildRouteSaturationSummaryText(focusedRoute);
        string opportunityText = BuildOpportunitySummaryText(selectedCity, primaryOpportunity);
        string blockedText = BuildBlockedReasonSummaryText(selectedCity, focusedRoute);
        string recentShiftText = BuildRecentPressureShiftSummaryText(selectedCity, primaryOpportunity);

        _currentNetworkPressureSurface = new PrototypeWorldSimNetworkPressureSurfaceData(
            overallStateKey,
            overallText,
            recoveryText,
            routeText,
            opportunityText,
            blockedText,
            primaryOpportunity != null ? primaryOpportunity.CityLabel : "None",
            recentShiftText,
            selectedCity,
            focusedRoute);
        _currentNetworkPressureSummaryText = overallText;
        _currentRouteSaturationSummaryText = routeText;
        _currentRecoveryForecastSummaryText = recoveryText;
        _currentOpportunitySummaryText = opportunityText;
    }

    private PrototypeWorldSimCityPressureSurfaceData BuildCityPressureSurfaceData(WorldEntityData city)
    {
        if (city == null || city.Kind != WorldEntityKind.City)
        {
            return PrototypeWorldSimCityPressureSurfaceData.Empty;
        }

        string cityId = city.Id ?? string.Empty;
        string linkedDungeonId = GetLinkedDungeonIdForCity(cityId);
        bool hasLinkedDungeon = !string.IsNullOrEmpty(linkedDungeonId) && FindEntity(linkedDungeonId) != null;
        string needPressure = BuildNeedPressureText(cityId);
        DispatchReadinessState readinessState = GetDispatchReadinessState(cityId);
        string readinessText = BuildDispatchReadinessText(readinessState);
        int recoveryDays = GetRecoveryDaysToReady(cityId);
        int criticalUnmet = _runtimeEconomyState != null ? _runtimeEconomyState.GetLastDayCriticalUnmet(cityId) : 0;
        int totalCriticalUnmet = _runtimeEconomyState != null ? _runtimeEconomyState.GetTotalCriticalUnmet(cityId) : 0;
        int shortages = _runtimeEconomyState != null ? _runtimeEconomyState.GetLastDayShortages(cityId) : 0;
        int totalShortages = _runtimeEconomyState != null ? _runtimeEconomyState.GetTotalShortages(cityId) : 0;
        int processingBlocked = _runtimeEconomyState != null ? _runtimeEconomyState.GetLastDayProcessingBlocked(cityId) : 0;
        int activeExpeditions = _runtimeEconomyState != null ? _runtimeEconomyState.GetActiveExpeditionCountFromCity(cityId) : 0;
        bool hasSurplus = HasSurplusResource(city);
        bool canRecruit = ResolveCanRecruitSelectedCityParty(cityId);
        bool canEnter = ResolveCanEnterSelectedCityDungeon(cityId);
        string routeSummary = hasLinkedDungeon ? BuildRecommendedRouteSummaryText(cityId, linkedDungeonId) : "None";

        string stateKey = criticalUnmet > 0 || totalCriticalUnmet > 0 || (needPressure == "Urgent" && readinessState == DispatchReadinessState.Strained)
            ? "critical"
            : readinessState == DispatchReadinessState.Strained || shortages > 0 || totalShortages > 0 || processingBlocked > 0
                ? "strained"
                : readinessState == DispatchReadinessState.Recovering
                    ? "recovering"
                    : needPressure == "Watch" || activeExpeditions > 0
                        ? "watch"
                        : "stable";
        int severity = stateKey == "critical" ? 4 : stateKey == "strained" ? 3 : stateKey == "recovering" ? 2 : stateKey == "watch" ? 1 : 0;
        int opportunityScore = hasLinkedDungeon
            ? (canEnter ? 90 : canRecruit ? 72 : 0) +
              (needPressure == "Urgent" ? 34 : needPressure == "Watch" ? 18 : hasSurplus ? 12 : 0) +
              (readinessState == DispatchReadinessState.Ready ? 20 : readinessState == DispatchReadinessState.Recovering ? Mathf.Max(4, 14 - (recoveryDays * 3)) : 2) -
              (activeExpeditions > 0 ? 36 : 0)
            : 0;
        opportunityScore = Mathf.Max(0, opportunityScore);
        bool isOpportunity = opportunityScore > 0 && readinessState == DispatchReadinessState.Ready;

        List<string> pressureParts = new List<string>(5);
        pressureParts.Add(needPressure + " stock");
        int criticalValue = criticalUnmet > 0 ? criticalUnmet : totalCriticalUnmet;
        if (criticalValue > 0)
        {
            pressureParts.Add(criticalValue + " critical unmet");
        }
        else
        {
            int shortageValue = shortages > 0 ? shortages : totalShortages;
            if (shortageValue > 0)
            {
                pressureParts.Add(shortageValue + " shortages");
            }
        }

        if (processingBlocked > 0)
        {
            pressureParts.Add("Proc block " + processingBlocked);
        }

        pressureParts.Add(readinessText);
        if (recoveryDays > 0)
        {
            pressureParts.Add("ETA " + BuildRecoveryEtaText(recoveryDays));
        }

        string pressureText = string.Join(" | ", pressureParts.ToArray());
        string recoveryText = recoveryDays > 0
            ? readinessText + " | ETA " + BuildRecoveryEtaText(recoveryDays)
            : readinessText + " | " + CompactSurfaceText(BuildDispatchRecoveryProgressText(cityId), 72);
        string opportunityText = !hasLinkedDungeon
            ? city.DisplayName + " has no linked dungeon lane yet."
            : canEnter
                ? city.DisplayName + " can dispatch now via " + CompactSurfaceText(routeSummary, 42) + "."
                : canRecruit
                    ? "Recruit 1 party in " + city.DisplayName + " to reopen the lane."
                    : activeExpeditions > 0
                        ? city.DisplayName + " already has an expedition out. Hold until it returns."
                        : recoveryDays > 0
                            ? city.DisplayName + " recovers in " + BuildRecoveryEtaText(recoveryDays) + "."
                            : hasSurplus
                                ? city.DisplayName + " holds surplus stock for the next push."
                                : city.DisplayName + " can hold the lane and watch the next day step.";
        string blockedText = !hasLinkedDungeon
            ? "No linked dungeon lane."
            : activeExpeditions > 0
                ? (activeExpeditions == 1 ? "1 expedition still holds this lane." : activeExpeditions + " expeditions still hold this lane.")
                : readinessState == DispatchReadinessState.Strained
                    ? city.DisplayName + " is strained. Recovery first."
                    : readinessState == DispatchReadinessState.Recovering
                        ? city.DisplayName + " recovers in " + BuildRecoveryEtaText(recoveryDays) + "."
                        : "Lane open.";
        List<string> shiftParts = new List<string>(3);
        string impactText = GetLastDispatchImpactText(cityId);
        if (HasMeaningfulSurfaceText(impactText))
        {
            shiftParts.Add(CompactSurfaceText(impactText, 54));
        }

        string needShiftText = GetLastNeedPressureChangeText(cityId);
        if (HasMeaningfulSurfaceText(needShiftText) && needShiftText != "None -> None")
        {
            shiftParts.Add("Pressure " + CompactSurfaceText(needShiftText, 42));
        }

        string readinessShiftText = GetLastDispatchReadinessChangeText(cityId);
        if (HasMeaningfulSurfaceText(readinessShiftText) && readinessShiftText != "None -> None")
        {
            shiftParts.Add("Readiness " + CompactSurfaceText(readinessShiftText, 42));
        }

        string recentShiftText = shiftParts.Count > 0 ? string.Join(" | ", shiftParts.ToArray()) : "None";
        return new PrototypeWorldSimCityPressureSurfaceData(cityId, city.DisplayName, stateKey, pressureText, recoveryText, opportunityText, blockedText, recentShiftText, recoveryDays, severity, opportunityScore, isOpportunity);
    }

    private PrototypeWorldSimRoutePressureSurfaceData BuildRoutePressureSurfaceData(WorldRouteData route, string selectedCityId)
    {
        int usage = _runtimeEconomyState != null ? _runtimeEconomyState.GetLastDayRouteUsage(route.Id) : 0;
        int capacity = Mathf.Max(0, route.CapacityPerDay);
        bool touchesSelection = !string.IsNullOrEmpty(selectedCityId) &&
                               (string.Equals(route.FromEntityId, selectedCityId, StringComparison.OrdinalIgnoreCase) || string.Equals(route.ToEntityId, selectedCityId, StringComparison.OrdinalIgnoreCase));
        string stateKey = usage <= 0 || capacity <= 0
            ? "idle"
            : usage >= capacity
                ? "saturated"
                : usage >= Mathf.Max(1, capacity - 1) || (usage / (float)capacity) >= 0.75f
                    ? "strained"
                    : "open";
        int severity = stateKey == "saturated" ? 3 : stateKey == "strained" ? 2 : stateKey == "open" ? 1 : 0;
        string summaryText = BuildRouteVisualLabel(route) + " | " + usage + "/" + capacity + " | " + (stateKey == "saturated" ? "Saturated" : stateKey == "strained" ? "Strained" : stateKey == "open" ? "Open" : "Idle") + " | " + GetRouteLinkText(route);
        return new PrototypeWorldSimRoutePressureSurfaceData(route.Id, BuildRouteVisualLabel(route), stateKey, summaryText, touchesSelection, touchesSelection && stateKey == "open", severity);
    }
    private PrototypeWorldSimRoutePressureSurfaceData ResolveFocusedRoutePressureSurface(string selectedCityId)
    {
        PrototypeWorldSimRoutePressureSurfaceData focused = PrototypeWorldSimRoutePressureSurfaceData.Empty;
        foreach (KeyValuePair<string, PrototypeWorldSimRoutePressureSurfaceData> pair in _routePressureByRouteId)
        {
            PrototypeWorldSimRoutePressureSurfaceData route = pair.Value;
            if (route == null)
            {
                continue;
            }

            if (!string.IsNullOrEmpty(selectedCityId) && route.TouchesSelection)
            {
                if (focused == PrototypeWorldSimRoutePressureSurfaceData.Empty || route.Severity > focused.Severity)
                {
                    focused = route;
                }

                continue;
            }

            if (string.IsNullOrEmpty(selectedCityId) && (focused == PrototypeWorldSimRoutePressureSurfaceData.Empty || route.Severity > focused.Severity))
            {
                focused = route;
            }
        }

        return focused;
    }

    private PrototypeWorldSimCityPressureSurfaceData ResolvePrimaryOpportunityCityPressure(string selectedCityId, PrototypeWorldSimCityPressureSurfaceData selectedCity)
    {
        if (!string.IsNullOrEmpty(selectedCityId) && selectedCity != null && selectedCity != PrototypeWorldSimCityPressureSurfaceData.Empty)
        {
            return selectedCity;
        }

        PrototypeWorldSimCityPressureSurfaceData best = PrototypeWorldSimCityPressureSurfaceData.Empty;
        foreach (KeyValuePair<string, PrototypeWorldSimCityPressureSurfaceData> pair in _cityPressureByEntityId)
        {
            PrototypeWorldSimCityPressureSurfaceData city = pair.Value;
            if (city == null || city == PrototypeWorldSimCityPressureSurfaceData.Empty || city.OpportunityScore <= 0)
            {
                continue;
            }

            if (best == PrototypeWorldSimCityPressureSurfaceData.Empty || city.OpportunityScore > best.OpportunityScore || (city.OpportunityScore == best.OpportunityScore && city.Severity > best.Severity))
            {
                best = city;
            }
        }

        return best;
    }

    private string ResolveOverallPressureStateKey(PrototypeWorldSimCityPressureSurfaceData selectedCity, PrototypeWorldSimRoutePressureSurfaceData focusedRoute)
    {
        if (selectedCity != null && selectedCity != PrototypeWorldSimCityPressureSurfaceData.Empty)
        {
            if (selectedCity.StateKey == "critical")
            {
                return "critical";
            }

            if (selectedCity.StateKey == "strained" || focusedRoute.StateKey == "saturated")
            {
                return "strained";
            }

            if (selectedCity.StateKey == "recovering" || focusedRoute.StateKey == "strained")
            {
                return "recovering";
            }

            return selectedCity.StateKey == "watch" ? "watch" : "stable";
        }

        if (HasMeaningfulSurfaceText(CitiesWithCriticalUnmetText))
        {
            return "critical";
        }

        if (HasMeaningfulSurfaceText(CitiesWithShortagesText) || focusedRoute.StateKey == "saturated")
        {
            return "strained";
        }

        if (focusedRoute.StateKey == "strained")
        {
            return "recovering";
        }

        return HasMeaningfulSurfaceText(CitiesWithSurplusText) ? "watch" : "stable";
    }

    private string BuildOverallPressureSummaryText(PrototypeWorldSimOperationChainSurfaceData chain, PrototypeWorldSimCityPressureSurfaceData selectedCity, PrototypeWorldSimRoutePressureSurfaceData focusedRoute)
    {
        if (selectedCity != null && selectedCity != PrototypeWorldSimCityPressureSurfaceData.Empty)
        {
            string prefix = chain != null && chain.SelectedEntityKind == nameof(WorldEntityKind.Dungeon)
                ? selectedCity.CityLabel + " backs the selected dungeon"
                : selectedCity.CityLabel;
            return prefix + " | " + CompactSurfaceText(selectedCity.PressureText, 78);
        }

        if (HasMeaningfulSurfaceText(CitiesWithCriticalUnmetText))
        {
            return "Critical pressure on " + CompactSurfaceText(CitiesWithCriticalUnmetText, 68) + ".";
        }

        if (HasMeaningfulSurfaceText(CitiesWithShortagesText))
        {
            return "Shortages press " + CompactSurfaceText(CitiesWithShortagesText, 68) + ".";
        }

        if (focusedRoute != null && focusedRoute != PrototypeWorldSimRoutePressureSurfaceData.Empty && focusedRoute.Severity > 0)
        {
            return CompactSurfaceText(focusedRoute.SummaryText, 92);
        }

        if (HasMeaningfulSurfaceText(CitiesWithSurplusText))
        {
            return "Opportunity in " + CompactSurfaceText(CitiesWithSurplusText, 68) + ".";
        }

        return "Network stable. No acute pressure.";
    }

    private string BuildRecoveryForecastSummaryText(PrototypeWorldSimCityPressureSurfaceData selectedCity, PrototypeWorldSimCityPressureSurfaceData primaryOpportunity)
    {
        if (selectedCity != null && selectedCity != PrototypeWorldSimCityPressureSurfaceData.Empty)
        {
            return selectedCity.CityLabel + " | " + CompactSurfaceText(selectedCity.RecoveryText, 78);
        }

        PrototypeWorldSimCityPressureSurfaceData recoveringCity = PrototypeWorldSimCityPressureSurfaceData.Empty;
        foreach (KeyValuePair<string, PrototypeWorldSimCityPressureSurfaceData> pair in _cityPressureByEntityId)
        {
            PrototypeWorldSimCityPressureSurfaceData city = pair.Value;
            if (city == null || city == PrototypeWorldSimCityPressureSurfaceData.Empty || city.RecoveryDays <= 0)
            {
                continue;
            }

            if (recoveringCity == PrototypeWorldSimCityPressureSurfaceData.Empty || city.RecoveryDays < recoveringCity.RecoveryDays)
            {
                recoveringCity = city;
            }
        }

        if (recoveringCity != PrototypeWorldSimCityPressureSurfaceData.Empty)
        {
            return recoveringCity.CityLabel + " | ETA " + BuildRecoveryEtaText(recoveringCity.RecoveryDays) + " | " + CompactSurfaceText(recoveringCity.RecoveryText, 56);
        }

        if (primaryOpportunity != null && primaryOpportunity != PrototypeWorldSimCityPressureSurfaceData.Empty)
        {
            return primaryOpportunity.CityLabel + " | " + CompactSurfaceText(primaryOpportunity.RecoveryText, 78);
        }

        return "All visible cities are ready now.";
    }

    private string BuildRouteSaturationSummaryText(PrototypeWorldSimRoutePressureSurfaceData focusedRoute)
    {
        return focusedRoute != null && focusedRoute != PrototypeWorldSimRoutePressureSurfaceData.Empty
            ? focusedRoute.SummaryText
            : HasMeaningfulSurfaceText(RouteCapacityUsedText)
                ? "Routes | " + CompactSurfaceText(RouteCapacityUsedText, 42)
                : "No visible route signal.";
    }

    private string BuildOpportunitySummaryText(PrototypeWorldSimCityPressureSurfaceData selectedCity, PrototypeWorldSimCityPressureSurfaceData primaryOpportunity)
    {
        if (selectedCity != null && selectedCity != PrototypeWorldSimCityPressureSurfaceData.Empty)
        {
            return selectedCity.OpportunityText;
        }

        if (primaryOpportunity != null && primaryOpportunity != PrototypeWorldSimCityPressureSurfaceData.Empty)
        {
            return primaryOpportunity.OpportunityText;
        }

        return HasMeaningfulSurfaceText(CitiesWithSurplusText)
            ? "Surplus stock sits in " + CompactSurfaceText(CitiesWithSurplusText, 64) + "."
            : "No immediate opportunity lane is visible.";
    }

    private string BuildBlockedReasonSummaryText(PrototypeWorldSimCityPressureSurfaceData selectedCity, PrototypeWorldSimRoutePressureSurfaceData focusedRoute)
    {
        if (selectedCity != null && selectedCity != PrototypeWorldSimCityPressureSurfaceData.Empty)
        {
            if (focusedRoute != null && focusedRoute != PrototypeWorldSimRoutePressureSurfaceData.Empty && focusedRoute.StateKey == "saturated")
            {
                return selectedCity.BlockedText + " Route lane is saturated.";
            }

            if (focusedRoute != null && focusedRoute != PrototypeWorldSimRoutePressureSurfaceData.Empty && focusedRoute.StateKey == "strained")
            {
                return selectedCity.BlockedText + " Route lane is strained.";
            }

            return selectedCity.BlockedText;
        }

        if (focusedRoute != null && focusedRoute != PrototypeWorldSimRoutePressureSurfaceData.Empty && focusedRoute.StateKey == "saturated")
        {
            return CompactSurfaceText(focusedRoute.RouteLabel + " is saturated and constrains network flow.", 96);
        }

        if (HasMeaningfulSurfaceText(CitiesWithCriticalUnmetText))
        {
            return "Critical unmet persists in " + CompactSurfaceText(CitiesWithCriticalUnmetText, 68) + ".";
        }

        return HasMeaningfulSurfaceText(CitiesWithShortagesText)
            ? "Shortages still bind " + CompactSurfaceText(CitiesWithShortagesText, 68) + "."
            : "No major block is visible right now.";
    }

    private string BuildRecentPressureShiftSummaryText(PrototypeWorldSimCityPressureSurfaceData selectedCity, PrototypeWorldSimCityPressureSurfaceData primaryOpportunity)
    {
        if (selectedCity != null && selectedCity != PrototypeWorldSimCityPressureSurfaceData.Empty && HasMeaningfulSurfaceText(selectedCity.RecentShiftText))
        {
            return selectedCity.RecentShiftText;
        }

        if (primaryOpportunity != null && primaryOpportunity != PrototypeWorldSimCityPressureSurfaceData.Empty && HasMeaningfulSurfaceText(primaryOpportunity.RecentShiftText))
        {
            return primaryOpportunity.CityLabel + " | " + primaryOpportunity.RecentShiftText;
        }

        if (HasMeaningfulSurfaceText(RecentDayLog1Text))
        {
            return CompactSurfaceText(RecentDayLog1Text, 96);
        }

        return HasMeaningfulSurfaceText(RecentExpeditionLog1Text)
            ? CompactSurfaceText(RecentExpeditionLog1Text, 96)
            : "None";
    }

    private void ApplyWorldNetworkPressurePulseStates(string selectedCityId)
    {
        foreach (KeyValuePair<string, PrototypeWorldSimCityPressureSurfaceData> pair in _cityPressureByEntityId)
        {
            if (!_markerByEntityId.TryGetValue(pair.Key, out WorldSelectableMarker marker) || marker == null)
            {
                continue;
            }

            PrototypeWorldSimCityPressureSurfaceData city = pair.Value;
            bool isPrimaryOpportunity = city != null && city.IsOpportunity && (string.IsNullOrEmpty(selectedCityId)
                ? string.Equals(city.CityLabel, _currentNetworkPressureSurface.PrimaryOpportunityLabel, StringComparison.OrdinalIgnoreCase)
                : string.Equals(pair.Key, selectedCityId, StringComparison.OrdinalIgnoreCase));
            marker.SetPressureState(city != null ? city.StateKey : string.Empty, isPrimaryOpportunity);
        }

        foreach (KeyValuePair<string, PrototypeWorldSimRoutePressureSurfaceData> pair in _routePressureByRouteId)
        {
            if (!_routePulseByRouteId.TryGetValue(pair.Key, out WorldBoardRoutePressureVisual routeVisual) || routeVisual == null)
            {
                continue;
            }

            PrototypeWorldSimRoutePressureSurfaceData route = pair.Value;
            routeVisual.ApplyState(route != null ? route.StateKey : string.Empty, route != null && (route.TouchesSelection || route.StateKey == "saturated"), route != null && route.IsOpportunity);
        }
    }
}

