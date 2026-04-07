using System;
using System.Collections.Generic;
using UnityEngine;

public sealed partial class StaticPlaceholderWorldView
{
    private sealed class PrototypeWorldSimOperationStepSurfaceData
    {
        public readonly string StepKey;
        public readonly string StepLabel;
        public readonly string SummaryText;
        public readonly bool IsBlocked;

        public PrototypeWorldSimOperationStepSurfaceData(string stepKey, string stepLabel, string summaryText, bool isBlocked = false)
        {
            StepKey = string.IsNullOrEmpty(stepKey) ? "step" : stepKey;
            StepLabel = string.IsNullOrEmpty(stepLabel) ? "Step" : stepLabel;
            SummaryText = string.IsNullOrEmpty(summaryText) ? "None" : summaryText;
            IsBlocked = isBlocked;
        }
    }

    private sealed class PrototypeWorldSimOperationChainSurfaceData
    {
        public static readonly PrototypeWorldSimOperationChainSurfaceData Empty = new PrototypeWorldSimOperationChainSurfaceData(
            string.Empty,
            "None",
            "None",
            string.Empty,
            "None",
            "None",
            "None",
            "none",
            "None",
            "None",
            "None",
            Array.Empty<PrototypeWorldSimOperationStepSurfaceData>());

        public readonly string SelectedEntityId;
        public readonly string SelectedEntityLabel;
        public readonly string SelectedEntityKind;
        public readonly string LinkedEntityId;
        public readonly string LinkedEntityLabel;
        public readonly string RouteSummaryText;
        public readonly string ReadinessSummaryText;
        public readonly string ActionStateKey;
        public readonly string PrimaryActionLabel;
        public readonly string PrimaryActionReasonText;
        public readonly string RecentOutcomeSummaryText;
        public readonly PrototypeWorldSimOperationStepSurfaceData[] Steps;

        public bool HasSelection => !string.IsNullOrEmpty(SelectedEntityId);

        public PrototypeWorldSimOperationChainSurfaceData(
            string selectedEntityId,
            string selectedEntityLabel,
            string selectedEntityKind,
            string linkedEntityId,
            string linkedEntityLabel,
            string routeSummaryText,
            string readinessSummaryText,
            string actionStateKey,
            string primaryActionLabel,
            string primaryActionReasonText,
            string recentOutcomeSummaryText,
            PrototypeWorldSimOperationStepSurfaceData[] steps)
        {
            SelectedEntityId = string.IsNullOrEmpty(selectedEntityId) ? string.Empty : selectedEntityId;
            SelectedEntityLabel = string.IsNullOrEmpty(selectedEntityLabel) ? "None" : selectedEntityLabel;
            SelectedEntityKind = string.IsNullOrEmpty(selectedEntityKind) ? "None" : selectedEntityKind;
            LinkedEntityId = string.IsNullOrEmpty(linkedEntityId) ? string.Empty : linkedEntityId;
            LinkedEntityLabel = string.IsNullOrEmpty(linkedEntityLabel) ? "None" : linkedEntityLabel;
            RouteSummaryText = string.IsNullOrEmpty(routeSummaryText) ? "None" : routeSummaryText;
            ReadinessSummaryText = string.IsNullOrEmpty(readinessSummaryText) ? "None" : readinessSummaryText;
            ActionStateKey = string.IsNullOrEmpty(actionStateKey) ? "none" : actionStateKey;
            PrimaryActionLabel = string.IsNullOrEmpty(primaryActionLabel) ? "None" : primaryActionLabel;
            PrimaryActionReasonText = string.IsNullOrEmpty(primaryActionReasonText) ? "None" : primaryActionReasonText;
            RecentOutcomeSummaryText = string.IsNullOrEmpty(recentOutcomeSummaryText) ? "None" : recentOutcomeSummaryText;
            Steps = steps ?? Array.Empty<PrototypeWorldSimOperationStepSurfaceData>();
        }
    }

    private sealed class WorldBoardConnectorPulseVisual
    {
        private readonly SpriteRenderer _baseRenderer;
        private readonly SpriteRenderer _innerRenderer;
        private readonly SpriteRenderer _sealRenderer;
        private readonly Color _baseColor;
        private readonly Color _innerColor;
        private readonly Color _sealColor;

        public WorldBoardConnectorPulseVisual(SpriteRenderer baseRenderer, SpriteRenderer innerRenderer, SpriteRenderer sealRenderer)
        {
            _baseRenderer = baseRenderer;
            _innerRenderer = innerRenderer;
            _sealRenderer = sealRenderer;
            _baseColor = baseRenderer != null ? baseRenderer.color : Color.clear;
            _innerColor = innerRenderer != null ? innerRenderer.color : Color.clear;
            _sealColor = sealRenderer != null ? sealRenderer.color : Color.clear;
        }

        public void ApplyPulse(bool isFocused, bool hasOutcomePin)
        {
            if (_baseRenderer != null)
            {
                _baseRenderer.color = isFocused ? Color.Lerp(_baseColor, Color.white, 0.18f) : _baseColor;
            }

            if (_innerRenderer != null)
            {
                float alpha = _innerColor.a;
                if (isFocused)
                {
                    alpha = Mathf.Clamp01(alpha + 0.30f);
                }
                else if (hasOutcomePin)
                {
                    alpha = Mathf.Clamp01(alpha + 0.12f);
                }

                _innerRenderer.color = new Color(_innerColor.r, _innerColor.g, _innerColor.b, alpha);
            }

            if (_sealRenderer != null)
            {
                Color target = _sealColor;
                if (hasOutcomePin)
                {
                    target = Color.Lerp(target, new Color(0.96f, 0.82f, 0.46f, target.a), 0.42f);
                }

                if (isFocused)
                {
                    target = Color.Lerp(target, Color.white, 0.22f);
                }

                _sealRenderer.color = target;
            }
        }
    }

    private readonly Dictionary<string, WorldSelectableMarker> _markerByEntityId = new Dictionary<string, WorldSelectableMarker>();
    private readonly Dictionary<string, WorldBoardConnectorPulseVisual> _linkedConnectorByKey = new Dictionary<string, WorldBoardConnectorPulseVisual>();
    private PrototypeWorldSimOperationChainSurfaceData _currentOperationChainSurface = PrototypeWorldSimOperationChainSurfaceData.Empty;
    private string _currentOperationChainSummaryText = "None";
    private string _currentRouteFocusSummaryText = "None";
    private string _currentBoardPulseSummaryText = "None";
    private string _currentRecentEventFeedSummaryText = "None";
    private string _currentActionReasonSummaryText = "None";

    public string CurrentOperationChainSummaryText => string.IsNullOrEmpty(_currentOperationChainSummaryText) ? "None" : _currentOperationChainSummaryText;
    public string CurrentRouteFocusSummaryText => string.IsNullOrEmpty(_currentRouteFocusSummaryText) ? "None" : _currentRouteFocusSummaryText;
    public string CurrentBoardPulseSummaryText => string.IsNullOrEmpty(_currentBoardPulseSummaryText) ? "None" : _currentBoardPulseSummaryText;
    public string CurrentRecentEventFeedSummaryText => string.IsNullOrEmpty(_currentRecentEventFeedSummaryText) ? "None" : _currentRecentEventFeedSummaryText;
    public string CurrentActionReasonSummaryText => string.IsNullOrEmpty(_currentActionReasonSummaryText) ? "None" : _currentActionReasonSummaryText;
    public bool CanRecruitSelectedCityPartyAction => _selectedMarker != null && _selectedMarker.EntityData.Kind == WorldEntityKind.City && ResolveCanRecruitSelectedCityParty(_selectedMarker.EntityData.Id);
    public bool CanEnterSelectedCityDungeonAction => _selectedMarker != null && _selectedMarker.EntityData.Kind == WorldEntityKind.City && ResolveCanEnterSelectedCityDungeon(_selectedMarker.EntityData.Id);

    private void RegisterWorldMarker(WorldSelectableMarker marker)
    {
        if (marker == null || marker.EntityData == null || string.IsNullOrEmpty(marker.EntityData.Id))
        {
            return;
        }

        _markerByEntityId[marker.EntityData.Id] = marker;
    }

    private void ClearWorldOperationVisualRegistry()
    {
        _markerByEntityId.Clear();
        _linkedConnectorByKey.Clear();
        _currentOperationChainSurface = PrototypeWorldSimOperationChainSurfaceData.Empty;
        _currentOperationChainSummaryText = "None";
        _currentRouteFocusSummaryText = "None";
        _currentBoardPulseSummaryText = "None";
        _currentRecentEventFeedSummaryText = "None";
        _currentActionReasonSummaryText = "None";
        ResetWorldNetworkPressureState();
        ResetWorldOpportunityLadderState();
        ResetWorldCommitmentQueueState();
        ResetWorldOutcomeReadbackState();
        ResetWorldOperationContinuityState();
        ResetWorldRecruitmentRuntimeState();
        ResetWorldPartyRosterRuntimeState();
    }

    private void HandleWorldSimVisibilityChanged(bool isVisible)
    {
        if (isVisible)
        {
            EnsureWorldOperationSurfaceVisuals();
            RefreshWorldOperationSurface();
            HandleWorldRecruitmentVisibilityChanged(true);
            HandleWorldPartyRosterVisibilityChanged(true);
            return;
        }

        HandleWorldRecruitmentVisibilityChanged(false);
        HandleWorldPartyRosterVisibilityChanged(false);
        ClearWorldOperationPulseVisuals();
    }

    private void EnsureWorldOperationSurfaceVisuals()
    {
        if (_root == null || _sprite == null || _worldData == null || _worldData.Entities == null)
        {
            return;
        }

        WorldEntityData[] entities = _worldData.Entities;
        for (int i = 0; i < entities.Length; i++)
        {
            WorldEntityData entity = entities[i];
            if (entity == null || entity.Kind != WorldEntityKind.Dungeon || string.IsNullOrEmpty(entity.LinkedCityId))
            {
                continue;
            }

            WorldEntityData city = FindEntity(entity.LinkedCityId);
            if (city == null)
            {
                continue;
            }

            CreateLinkedDungeonConnector(city, entity);
        }
    }

    private void CreateLinkedDungeonConnector(WorldEntityData city, WorldEntityData dungeon)
    {
        if (city == null || dungeon == null || _root == null || _sprite == null)
        {
            return;
        }

        string key = BuildLinkedConnectorKey(city.Id, dungeon.Id);
        if (_linkedConnectorByKey.ContainsKey(key))
        {
            return;
        }

        Vector2 start = city.Position;
        Vector2 end = dungeon.Position;
        Vector2 delta = end - start;
        float length = Mathf.Max(0.01f, delta.magnitude);
        float angle = Mathf.Atan2(delta.y, delta.x) * Mathf.Rad2Deg;
        Vector2 midpoint = (start + end) * 0.5f;

        CreateWorldVisual(_root.transform, key + "_Shadow", midpoint + new Vector2(0.05f, -0.05f), new Vector2(length, 0.16f), new Color(0f, 0f, 0f, 0.18f), -1, angle);
        SpriteRenderer baseRenderer = CreateWorldVisual(_root.transform, key + "_Base", midpoint, new Vector2(length, 0.12f), new Color(0.16f, 0.22f, 0.26f, 0.24f), 0, angle).GetComponent<SpriteRenderer>();
        SpriteRenderer innerRenderer = CreateWorldVisual(_root.transform, key + "_Inner", midpoint, new Vector2(Mathf.Max(0.08f, length - 0.10f), 0.04f), new Color(0.48f, 0.78f, 0.86f, 0.16f), 1, angle).GetComponent<SpriteRenderer>();
        SpriteRenderer sealRenderer = CreateWorldVisual(_root.transform, key + "_Seal", midpoint, new Vector2(0.18f, 0.18f), new Color(0.94f, 0.76f, 0.42f, 0.22f), 2, 45f).GetComponent<SpriteRenderer>();
        _linkedConnectorByKey[key] = new WorldBoardConnectorPulseVisual(baseRenderer, innerRenderer, sealRenderer);
    }

    private string BuildLinkedConnectorKey(string cityId, string dungeonId)
    {
        return string.IsNullOrEmpty(cityId) || string.IsNullOrEmpty(dungeonId)
            ? string.Empty
            : cityId + "=>" + dungeonId;
    }

    private void RefreshWorldOperationSurface()
    {
        EnsureWorldOperationSurfaceVisuals();
        _currentOperationChainSurface = ResolveSelectedOperationChain();
        RefreshWorldNetworkPressureSurface(_currentOperationChainSurface);
        RefreshWorldOpportunityLadderSurface(_currentOperationChainSurface);
        RefreshWorldCommitmentQueueSurface(_currentOperationChainSurface);
        RefreshWorldOutcomeReadbackSurface(_currentOperationChainSurface);
        RefreshWorldRecruitmentSurface();
        RefreshWorldPartyRosterSurface();
        _currentOperationChainSurface = ResolveSelectedOperationChain();
        _currentOperationChainSummaryText = BuildOperationChainSummaryText(_currentOperationChainSurface);
        _currentRouteFocusSummaryText = BuildRouteFocusSummaryText(_currentOperationChainSurface);
        _currentBoardPulseSummaryText = BuildBoardPulseSummaryText(_currentOperationChainSurface);
        _currentRecentEventFeedSummaryText = BuildRecentEventFeedSummaryText(_currentOperationChainSurface);
        _currentActionReasonSummaryText = BuildActionReasonSummaryText(_currentOperationChainSurface);
        _currentExpeditionStagingSummaryText = BuildExpeditionStagingSummaryText(_currentOperationChainSurface);
        _currentReturnAftermathSummaryText = BuildReturnAftermathSummaryText(_currentOperationChainSurface);
        _currentAlertRibbonSummaryText = BuildAlertRibbonSummaryText(_currentOperationChainSurface);
        ApplyWorldOperationPulseState(_currentOperationChainSurface);
    }

    private void ClearWorldOperationPulseVisuals()
    {
        foreach (KeyValuePair<string, WorldSelectableMarker> pair in _markerByEntityId)
        {
            if (pair.Value == null)
            {
                continue;
            }

            pair.Value.SetLinkedFocus(false);
            pair.Value.SetOutcomePinned(false);
            pair.Value.SetActionReady(false);
            pair.Value.SetPressureState(string.Empty, false);
        }

        foreach (KeyValuePair<string, WorldBoardRoutePressureVisual> pair in _routePulseByRouteId)
        {
            if (pair.Value != null)
            {
                pair.Value.ApplyState(string.Empty, false, false);
            }
        }

        foreach (KeyValuePair<string, WorldBoardConnectorPulseVisual> pair in _linkedConnectorByKey)
        {
            if (pair.Value != null)
            {
                pair.Value.ApplyPulse(false, false);
            }
        }
    }

    private void ApplyWorldOperationPulseState(PrototypeWorldSimOperationChainSurfaceData chain)
    {
        ClearWorldOperationPulseVisuals();

        string selectedCityId = string.Empty;
        string selectedDungeonId = string.Empty;
        string selectedConnectorKey = string.Empty;
        bool selectedOutcomePin = false;

        if (chain != null && chain.HasSelection)
        {
            if (_markerByEntityId.TryGetValue(chain.SelectedEntityId, out WorldSelectableMarker selectedMarker) && selectedMarker != null)
            {
                bool actionReady = chain.ActionStateKey == "enter_ready" || chain.ActionStateKey == "recruit_ready" || chain.ActionStateKey == "inspect_linked_city";
                selectedMarker.SetActionReady(actionReady);
            }

            if (!string.IsNullOrEmpty(chain.LinkedEntityId) && _markerByEntityId.TryGetValue(chain.LinkedEntityId, out WorldSelectableMarker linkedMarker) && linkedMarker != null)
            {
                linkedMarker.SetLinkedFocus(true);
            }

            selectedCityId = chain.SelectedEntityKind == nameof(WorldEntityKind.City) ? chain.SelectedEntityId : chain.LinkedEntityId;
            selectedDungeonId = chain.SelectedEntityKind == nameof(WorldEntityKind.Dungeon) ? chain.SelectedEntityId : chain.LinkedEntityId;
            selectedConnectorKey = BuildLinkedConnectorKey(selectedCityId, selectedDungeonId);
            selectedOutcomePin = !HasLatestReturnAftermath() && HasMeaningfulSurfaceText(chain.RecentOutcomeSummaryText);

            if (selectedOutcomePin)
            {
                ApplySelectedOutcomePins(chain);
            }
        }

        ApplyWorldNetworkPressurePulseStates(selectedCityId);
        ApplyWorldOpportunityLadderPulseState();
        ApplyWorldCommitmentQueuePulseState();
        ApplyWorldOutcomeReadbackPulseState();

        if (!string.IsNullOrEmpty(selectedConnectorKey) && _linkedConnectorByKey.TryGetValue(selectedConnectorKey, out WorldBoardConnectorPulseVisual selectedConnectorVisual) && selectedConnectorVisual != null)
        {
            selectedConnectorVisual.ApplyPulse(true, selectedOutcomePin || MatchesLatestReturnAftermathConnector(selectedCityId, selectedDungeonId));
        }

        if (HasLatestReturnAftermath())
        {
            ApplyLatestReturnAftermathPins(selectedConnectorKey);
        }
    }

    private PrototypeWorldSimOperationChainSurfaceData ResolveSelectedOperationChain()
    {
        if (_selectedMarker == null || _selectedMarker.EntityData == null)
        {
            return PrototypeWorldSimOperationChainSurfaceData.Empty;
        }

        WorldEntityData entity = _selectedMarker.EntityData;
        return entity.Kind == WorldEntityKind.City
            ? BuildCityOperationChainSurface(entity)
            : BuildDungeonOperationChainSurface(entity);
    }

    private PrototypeWorldSimOperationChainSurfaceData BuildCityOperationChainSurface(WorldEntityData city)
    {
        string cityId = city != null ? city.Id : string.Empty;
        string dungeonId = city != null ? GetLinkedDungeonIdForCity(city.Id) : string.Empty;
        WorldEntityData dungeon = FindEntity(dungeonId);
        bool hasLinkedDungeon = dungeon != null;
        bool canRecruit = city != null && ResolveCanRecruitSelectedCityParty(city.Id);
        bool canCommit = city != null && ResolveCanCommitSelectedCityDungeon(city.Id);
        bool hasRoster = city != null && ResolveCanOpenCityPartyRosterBoard(city.Id);
        int idlePartyCount = city != null && _runtimeEconomyState != null ? _runtimeEconomyState.GetIdlePartyCountInCity(city.Id) : 0;
        int activeExpeditionCount = city != null && _runtimeEconomyState != null ? _runtimeEconomyState.GetActiveExpeditionCountFromCity(city.Id) : 0;
        string routeSummary = hasLinkedDungeon ? BuildRecommendedRouteSummaryText(city.Id, dungeonId) : "No linked dungeon";
        string readinessSummary = city != null ? GetDispatchReadinessText(city.Id) : "None";
        string primaryActionLabel = canCommit
            ? "Enter Dungeon"
            : hasRoster
                ? "Stage Party"
                : canRecruit
                    ? "Recruit Party"
                    : "Run Day";
        string actionStateKey = canCommit
            ? "enter_ready"
            : hasRoster
                ? activeExpeditionCount > 0
                    ? "stage_waiting_lane"
                    : "stage_party"
                : canRecruit
                    ? "recruit_ready"
                    : hasLinkedDungeon
                        ? activeExpeditionCount > 0
                            ? "wait_active_party"
                            : "advance_world"
                        : "blocked_no_linked_dungeon";
        string primaryActionReasonText = BuildCityPrimaryActionReason(city, dungeon, routeSummary, canRecruit, canCommit, idlePartyCount, activeExpeditionCount);
        string recentOutcomeSummaryText = BuildCityRecentOutcomeSummary(cityId);
        string rosterSummary = hasRoster ? CurrentPartyRosterSummaryText : "No roster yet.";
        PrototypeWorldSimOperationStepSurfaceData[] steps = new[]
        {
            new PrototypeWorldSimOperationStepSurfaceData("city", "City", city != null ? city.DisplayName + " | " + BuildNeedPressureText(city.Id) : "None"),
            new PrototypeWorldSimOperationStepSurfaceData("dungeon", "Dungeon", hasLinkedDungeon ? dungeon.DisplayName + " | " + BuildDungeonDangerSummaryText(dungeonId) : "No linked dungeon", !hasLinkedDungeon),
            new PrototypeWorldSimOperationStepSurfaceData("route", "Route", routeSummary, !hasLinkedDungeon),
            new PrototypeWorldSimOperationStepSurfaceData("roster", "Roster", hasRoster ? rosterSummary : "No company has been recruited into this city yet.", !hasRoster),
            new PrototypeWorldSimOperationStepSurfaceData("action", "Action", primaryActionLabel + " | " + primaryActionReasonText, actionStateKey == "blocked_no_linked_dungeon"),
            new PrototypeWorldSimOperationStepSurfaceData("outcome", "Outcome", recentOutcomeSummaryText)
        };

        return new PrototypeWorldSimOperationChainSurfaceData(
            cityId,
            city != null ? city.DisplayName : "None",
            nameof(WorldEntityKind.City),
            dungeon != null ? dungeon.Id : string.Empty,
            dungeon != null ? dungeon.DisplayName : "None",
            routeSummary,
            readinessSummary,
            actionStateKey,
            primaryActionLabel,
            primaryActionReasonText,
            recentOutcomeSummaryText,
            steps);
    }

    private PrototypeWorldSimOperationChainSurfaceData BuildDungeonOperationChainSurface(WorldEntityData dungeon)
    {
        string linkedCityId = dungeon != null ? dungeon.LinkedCityId : string.Empty;
        WorldEntityData city = FindEntity(linkedCityId);
        bool hasLinkedCity = city != null;
        bool canRecruit = city != null && ResolveCanRecruitSelectedCityParty(city.Id);
        bool canCommit = city != null && ResolveCanCommitSelectedCityDungeon(city.Id);
        bool hasRoster = city != null && ResolveCanOpenCityPartyRosterBoard(city.Id);
        int activeExpeditionCount = city != null && _runtimeEconomyState != null ? _runtimeEconomyState.GetActiveExpeditionCountFromCity(city.Id) : 0;
        string routeSummary = hasLinkedCity ? BuildRecommendedRouteSummaryText(city.Id, dungeon.Id) : "No linked city";
        string readinessSummary = hasLinkedCity ? GetDispatchReadinessText(city.Id) : "None";
        string primaryActionLabel = canCommit
            ? "Select Linked City"
            : hasRoster
                ? "Stage Party"
                : canRecruit
                    ? "Recruit Party"
                    : "Run Day";
        string actionStateKey = canCommit
            ? "inspect_linked_city"
            : hasRoster
                ? activeExpeditionCount > 0
                    ? "stage_waiting_lane"
                    : "stage_party"
                : canRecruit
                    ? "recruit_ready"
                    : hasLinkedCity
                        ? activeExpeditionCount > 0
                            ? "wait_active_party"
                            : "advance_world"
                        : "blocked_no_linked_city";
        string primaryActionReasonText = BuildDungeonPrimaryActionReason(dungeon, city, routeSummary, canRecruit, canCommit, activeExpeditionCount);
        string recentOutcomeSummaryText = BuildDungeonRecentOutcomeSummary(dungeon != null ? dungeon.Id : string.Empty, city != null ? city.Id : string.Empty);
        string rosterSummary = hasLinkedCity && hasRoster ? CurrentPartyRosterSummaryText : "No city roster is ready on this lane.";
        PrototypeWorldSimOperationStepSurfaceData[] steps = new[]
        {
            new PrototypeWorldSimOperationStepSurfaceData("dungeon", "Dungeon", dungeon != null ? dungeon.DisplayName + " | " + BuildDungeonDangerSummaryText(dungeon.Id) : "None"),
            new PrototypeWorldSimOperationStepSurfaceData("city", "City", hasLinkedCity ? city.DisplayName + " | " + readinessSummary : "No linked city", !hasLinkedCity),
            new PrototypeWorldSimOperationStepSurfaceData("route", "Route", routeSummary, !hasLinkedCity),
            new PrototypeWorldSimOperationStepSurfaceData("roster", "Roster", rosterSummary, !hasRoster),
            new PrototypeWorldSimOperationStepSurfaceData("action", "Action", primaryActionLabel + " | " + primaryActionReasonText, actionStateKey == "blocked_no_linked_city"),
            new PrototypeWorldSimOperationStepSurfaceData("outcome", "Outcome", recentOutcomeSummaryText)
        };

        return new PrototypeWorldSimOperationChainSurfaceData(
            dungeon != null ? dungeon.Id : string.Empty,
            dungeon != null ? dungeon.DisplayName : "None",
            nameof(WorldEntityKind.Dungeon),
            city != null ? city.Id : string.Empty,
            city != null ? city.DisplayName : "None",
            routeSummary,
            readinessSummary,
            actionStateKey,
            primaryActionLabel,
            primaryActionReasonText,
            recentOutcomeSummaryText,
            steps);
    }

    private string BuildCityPrimaryActionReason(WorldEntityData city, WorldEntityData dungeon, string routeSummary, bool canRecruit, bool canEnter, int idlePartyCount, int activeExpeditionCount)
    {
        if (city == null)
        {
            return "Select a city to open a dispatch chain.";
        }

        if (dungeon == null)
        {
            return "No linked dungeon is assigned to this city yet.";
        }

        string stagedPartyId = ResolveStagedPartyIdForCity(city.Id);
        bool hasStagedParty = !string.IsNullOrEmpty(stagedPartyId);
        bool stagedPartyReady = hasStagedParty && _runtimeEconomyState != null && _runtimeEconomyState.IsPartyIdle(stagedPartyId) && activeExpeditionCount <= 0;
        int partyCount = _runtimeEconomyState != null ? _runtimeEconomyState.GetPartyCountInCity(city.Id) : 0;

        if (stagedPartyReady && canEnter)
        {
            return ResolveWorldPartyDisplayName(stagedPartyId) + " is staged and can enter " + dungeon.DisplayName + " through " + routeSummary + ".";
        }

        if (activeExpeditionCount > 0 && hasStagedParty)
        {
            return ResolveWorldPartyDisplayName(stagedPartyId) + " is staged behind the active lane. X reopens the roster so you can compare or keep the queued reserve.";
        }

        if (partyCount > 0)
        {
            return hasStagedParty
                ? ResolveWorldPartyDisplayName(stagedPartyId) + " is staged, but the lane still needs time before commit. Compare the roster if you want to swap the reserve choice."
                : partyCount == 1
                    ? "One company is available. X opens the roster so you can stage it before dispatch."
                    : partyCount + " companies are available. X opens the roster so you can compare and stage the best fit for " + dungeon.DisplayName + ".";
        }

        if (canRecruit)
        {
            return "No company is stationed here yet. Recruit before dispatch.";
        }

        if (activeExpeditionCount > 0)
        {
            return "This city already has an expedition in progress. Run a day to progress it.";
        }

        if (idlePartyCount > 0)
        {
            return "Reserve companies are present, but no staged dispatch choice is locked in yet.";
        }

        return "Run a day to refresh readiness and operational tempo.";
    }

    private string BuildDungeonPrimaryActionReason(WorldEntityData dungeon, WorldEntityData city, string routeSummary, bool canRecruit, bool canEnter, int activeExpeditionCount)
    {
        if (dungeon == null)
        {
            return "Select a dungeon to trace the reverse chain.";
        }

        if (city == null)
        {
            return "This dungeon does not expose a linked city.";
        }

        string stagedPartyId = ResolveStagedPartyIdForCity(city.Id);
        bool hasStagedParty = !string.IsNullOrEmpty(stagedPartyId);
        int partyCount = _runtimeEconomyState != null ? _runtimeEconomyState.GetPartyCountInCity(city.Id) : 0;

        if (hasStagedParty && canEnter)
        {
            return ResolveWorldPartyDisplayName(stagedPartyId) + " is staged in " + city.DisplayName + ". Select the city and enter through " + routeSummary + ".";
        }

        if (activeExpeditionCount > 0 && hasStagedParty)
        {
            return city.DisplayName + " already staged " + ResolveWorldPartyDisplayName(stagedPartyId) + ", but another expedition still occupies the lane.";
        }

        if (partyCount > 0)
        {
            return city.DisplayName + " has reserve companies. Select the city and compare them before entering " + dungeon.DisplayName + ".";
        }

        if (canRecruit)
        {
            return city.DisplayName + " needs a company before this dungeon can be entered.";
        }

        if (activeExpeditionCount > 0)
        {
            return city.DisplayName + " already has an expedition in motion. Run a day to progress it.";
        }

        return "Review the linked city before entering this dungeon.";
    }

    private bool ResolveCanRecruitSelectedCityParty(string cityId)
    {
        if (_runtimeEconomyState == null || string.IsNullOrEmpty(cityId))
        {
            return false;
        }

        int capacity = _runtimeEconomyState.GetPartyCapacityForCity(cityId);
        return capacity > 0 && _runtimeEconomyState.GetPartyCountInCity(cityId) < capacity;
    }

    private bool ResolveCanEnterSelectedCityDungeon(string cityId)
    {
        if (_runtimeEconomyState == null || string.IsNullOrEmpty(cityId) || IsDungeonRunActive)
        {
            return false;
        }

        string dungeonId = GetLinkedDungeonIdForCity(cityId);
        if (string.IsNullOrEmpty(dungeonId) || FindEntity(dungeonId) == null)
        {
            return false;
        }

        return ResolveCanCommitSelectedCityDungeon(cityId) || ResolveCanOpenCityPartyRosterBoard(cityId);
    }

    private bool ResolveCanCommitSelectedCityDungeon(string cityId)
    {
        if (_runtimeEconomyState == null || string.IsNullOrEmpty(cityId) || IsDungeonRunActive)
        {
            return false;
        }

        string dungeonId = GetLinkedDungeonIdForCity(cityId);
        if (string.IsNullOrEmpty(dungeonId) || FindEntity(dungeonId) == null)
        {
            return false;
        }

        string stagedPartyId = ResolveStagedPartyIdForCity(cityId);
        return !string.IsNullOrEmpty(stagedPartyId) &&
               _runtimeEconomyState.IsPartyIdle(stagedPartyId) &&
               _runtimeEconomyState.GetActiveExpeditionCountFromCity(cityId) <= 0;
    }

    private string BuildOperationChainSummaryText(PrototypeWorldSimOperationChainSurfaceData chain)
    {
        if (chain == null || !chain.HasSelection || chain.Steps == null || chain.Steps.Length <= 0)
        {
            return "None";
        }

        List<string> lines = new List<string>(chain.Steps.Length);
        for (int i = 0; i < chain.Steps.Length; i++)
        {
            PrototypeWorldSimOperationStepSurfaceData step = chain.Steps[i];
            if (step == null || !HasMeaningfulSurfaceText(step.SummaryText))
            {
                continue;
            }

            lines.Add(step.StepLabel + ": " + CompactSurfaceText(step.SummaryText, 112));
        }

        return lines.Count > 0 ? string.Join("\n", lines.ToArray()) : "None";
    }

    private string BuildRouteFocusSummaryText(PrototypeWorldSimOperationChainSurfaceData chain)
    {
        if ((chain == null || !chain.HasSelection || !HasMeaningfulSurfaceText(chain.RouteSummaryText)) && !HasMeaningfulSurfaceText(_currentRouteSaturationSummaryText))
        {
            return "None";
        }

        List<string> lines = new List<string>(3);
        if (chain != null && chain.HasSelection && HasMeaningfulSurfaceText(chain.RouteSummaryText))
        {
            lines.Add(CompactSurfaceText(chain.RouteSummaryText, 112));
            string previewText = ResolveFocusedRoutePreviewText(chain);
            if (HasMeaningfulSurfaceText(previewText))
            {
                lines.Add(CompactSurfaceText(previewText, 112));
            }
        }

        if (HasMeaningfulSurfaceText(_currentRouteSaturationSummaryText))
        {
            lines.Add(CompactSurfaceText(_currentRouteSaturationSummaryText, 112));
        }

        if (HasMeaningfulSurfaceText(_currentConsequencePreviewSummaryText))
        {
            lines.Add(CompactSurfaceText(_currentConsequencePreviewSummaryText.Replace("\n", " | "), 112));
        }

        return lines.Count > 0 ? string.Join("\n", lines.ToArray()) : "None";
    }

    private string ResolveFocusedRoutePreviewText(PrototypeWorldSimOperationChainSurfaceData chain)
    {
        if (chain == null || !chain.HasSelection)
        {
            return "None";
        }

        string cityId = chain.SelectedEntityKind == nameof(WorldEntityKind.City) ? chain.SelectedEntityId : chain.LinkedEntityId;
        string dungeonId = chain.SelectedEntityKind == nameof(WorldEntityKind.Dungeon) ? chain.SelectedEntityId : chain.LinkedEntityId;
        if (string.IsNullOrEmpty(cityId) || string.IsNullOrEmpty(dungeonId))
        {
            return "None";
        }

        string routeId = GetRecommendedRouteId(cityId, dungeonId);
        return string.IsNullOrEmpty(routeId) ? "None" : BuildRoutePreviewSummaryText(dungeonId, routeId);
    }

    private string BuildBoardPulseSummaryText(PrototypeWorldSimOperationChainSurfaceData chain)
    {
        List<string> parts = new List<string>(5);
        if (chain == null || !chain.HasSelection)
        {
            parts.Add(HasMeaningfulSurfaceText(_currentNetworkPressureSummaryText)
                ? CompactSurfaceText(_currentNetworkPressureSummaryText, 96)
                : "Pulse idle. Select a city or dungeon to trace an operation chain.");
            if (HasMeaningfulSurfaceText(_currentRecoveryForecastSummaryText))
            {
                parts.Add("Forecast " + CompactSurfaceText(_currentRecoveryForecastSummaryText, 72));
            }

            if (HasMeaningfulSurfaceText(_currentRouteSaturationSummaryText))
            {
                parts.Add(CompactSurfaceText(_currentRouteSaturationSummaryText, 72));
            }

            if (HasLatestReturnAftermath())
            {
                parts.Add("Aftermath pinned");
            }

            return string.Join(" | ", parts.ToArray());
        }

        parts.Add(HasMeaningfulSurfaceText(chain.LinkedEntityLabel)
            ? chain.SelectedEntityLabel + " -> " + chain.LinkedEntityLabel
            : chain.SelectedEntityLabel + " has no linked node.");
        parts.Add(HasMeaningfulSurfaceText(_currentNetworkPressureSummaryText)
            ? CompactSurfaceText(_currentNetworkPressureSummaryText, 72)
            : chain.ActionStateKey == "enter_ready"
                ? "Entry ready."
                : chain.ActionStateKey == "recruit_ready"
                    ? "Recruitment ready."
                    : chain.ActionStateKey == "inspect_linked_city"
                        ? "Linked city can dispatch."
                        : chain.ActionStateKey == "wait_active_party"
                            ? "Existing expedition still holds the chain."
                            : chain.ActionStateKey == "advance_world"
                                ? "Advance the world to reopen the chain."
                                : "Chain blocked.");
        if (HasMeaningfulSurfaceText(_currentRecoveryForecastSummaryText))
        {
            parts.Add("Forecast " + CompactSurfaceText(_currentRecoveryForecastSummaryText, 64));
        }

        if (HasMeaningfulSurfaceText(_currentRouteSaturationSummaryText))
        {
            parts.Add(CompactSurfaceText(_currentRouteSaturationSummaryText, 64));
        }

        if (HasMeaningfulSurfaceText(_currentNetworkPressureSurface.RecentShiftText))
        {
            parts.Add("Shift " + CompactSurfaceText(_currentNetworkPressureSurface.RecentShiftText, 48));
        }

        if (HasMeaningfulSurfaceText(_currentRelievedHotspotSummaryText) && _currentRelievedHotspotSummaryText != "No hotspot eased beyond the baseline yet.")
        {
            parts.Add("Relief " + CompactSurfaceText(_currentRelievedHotspotSummaryText, 48));
        }

        if (HasMeaningfulSurfaceText(_currentReboundHotspotSummaryText) && _currentReboundHotspotSummaryText != "No new rebound hotspot is visible.")
        {
            parts.Add("Rebound " + CompactSurfaceText(_currentReboundHotspotSummaryText, 48));
        }

        if (HasLatestReturnAftermath() && MatchesLatestReturnAftermathChain(chain))
        {
            parts.Add("Aftermath pinned");
        }
        else if (HasMeaningfulSurfaceText(chain.RecentOutcomeSummaryText))
        {
            parts.Add("Outcome " + CompactSurfaceText(chain.RecentOutcomeSummaryText, 56));
        }

        return string.Join(" | ", parts.ToArray());
    }

    private string BuildActionReasonSummaryText(PrototypeWorldSimOperationChainSurfaceData chain)
    {
        string worldStateReason = AutoTickEnabled
            ? AutoTickPaused
                ? "Auto Tick is enabled but paused."
                : "Auto Tick is advancing the world."
            : "Auto Tick is off. Use Run Day for manual progress.";
        List<string> parts = new List<string>(5);
        if (chain == null || !chain.HasSelection)
        {
            parts.Add("Select a city to recruit or dispatch. Dungeon selection shows the reverse chain.");
        }
        else
        {
            parts.Add(chain.PrimaryActionLabel + ": " + chain.PrimaryActionReasonText);
        }

        if (HasMeaningfulSurfaceText(_currentCommitReasonSummaryText))
        {
            parts.Add("Commit: " + CompactSurfaceText(_currentCommitReasonSummaryText, 72));
        }

        if (HasMeaningfulSurfaceText(_currentBlockedReasonSummaryText) && _currentBlockedReasonSummaryText != "No major block is exposed right now.")
        {
            parts.Add("Block: " + CompactSurfaceText(_currentBlockedReasonSummaryText, 72));
        }

        if (HasMeaningfulSurfaceText(_currentAlternateMoveSummaryText))
        {
            parts.Add("Alt: " + CompactSurfaceText(_currentAlternateMoveSummaryText.Replace("\n", " | "), 72));
        }

        if (HasMeaningfulSurfaceText(_currentCorrectiveFollowUpSummaryText) && _currentCorrectiveFollowUpSummaryText != "No corrective follow-up is visible.")
        {
            parts.Add("Follow-up: " + CompactSurfaceText(_currentCorrectiveFollowUpSummaryText, 72));
        }

        parts.Add(worldStateReason);
        return string.Join(" | ", parts.ToArray());
    }

    private string BuildRecentEventFeedSummaryText(PrototypeWorldSimOperationChainSurfaceData chain)
    {
        List<string> lines = new List<string>(4);
        bool hasAftermath = HasLatestReturnAftermath();

        if (HasMeaningfulSurfaceText(RecentExpeditionLog1Text))
        {
            lines.Add("Expedition: " + CompactSurfaceText(RecentExpeditionLog1Text, 108));
        }

        if (HasMeaningfulSurfaceText(RecentDayLog1Text))
        {
            lines.Add("Day: " + CompactSurfaceText(RecentDayLog1Text, 108));
        }

        if (!hasAftermath && chain != null && HasMeaningfulSurfaceText(chain.RecentOutcomeSummaryText))
        {
            lines.Add("Selected: " + CompactSurfaceText(chain.RecentOutcomeSummaryText, 108));
        }

        if (!hasAftermath && chain != null && chain.SelectedEntityKind == nameof(WorldEntityKind.City) && HasMeaningfulSurfaceText(SelectedLastRunDungeonText) && HasMeaningfulSurfaceText(SelectedLastRunRouteText))
        {
            lines.Add("Last Run: " + CompactSurfaceText(SelectedLastRunDungeonText + " | " + SelectedLastRunRouteText, 108));
        }
        else if (hasAftermath && chain != null && chain.HasSelection && !MatchesLatestReturnAftermathChain(chain))
        {
            lines.Add("Selection: " + CompactSurfaceText(chain.SelectedEntityLabel + " | " + chain.RouteSummaryText, 108));
        }

        return lines.Count > 0 ? string.Join("\n", lines.ToArray()) : "None";
    }

    private string BuildCityRecentOutcomeSummary(string cityId)
    {
        if (_runtimeEconomyState == null || string.IsNullOrEmpty(cityId))
        {
            return "None";
        }

        List<string> parts = new List<string>(3);
        string expeditionResult = _runtimeEconomyState.GetExpeditionStatusTextForCity(cityId);
        if (HasMeaningfulSurfaceText(expeditionResult))
        {
            parts.Add(expeditionResult);
        }

        string routeSummary = _runtimeEconomyState.GetLastRunRouteSummaryForCity(cityId);
        if (HasMeaningfulSurfaceText(routeSummary))
        {
            parts.Add(routeSummary);
        }

        string lootSummary = _runtimeEconomyState.GetLastRunLootSummaryForCity(cityId);
        if (HasMeaningfulSurfaceText(lootSummary))
        {
            parts.Add(lootSummary);
        }

        return parts.Count > 0 ? string.Join(" | ", parts.ToArray()) : "None";
    }

    private string BuildDungeonRecentOutcomeSummary(string dungeonId, string linkedCityId)
    {
        if (_runtimeEconomyState == null || string.IsNullOrEmpty(dungeonId))
        {
            return "None";
        }

        List<string> parts = new List<string>(3);
        string expeditionResult = _runtimeEconomyState.GetExpeditionStatusTextForDungeon(dungeonId);
        if (HasMeaningfulSurfaceText(expeditionResult))
        {
            parts.Add(expeditionResult);
        }

        if (!string.IsNullOrEmpty(linkedCityId))
        {
            WorldEntityData dungeon = FindEntity(dungeonId);
            string lastDungeonSummary = _runtimeEconomyState.GetLastRunDungeonSummaryForCity(linkedCityId);
            if (dungeon != null && HasMeaningfulSurfaceText(lastDungeonSummary) && string.Equals(lastDungeonSummary, dungeon.DisplayName, StringComparison.OrdinalIgnoreCase))
            {
                string routeSummary = _runtimeEconomyState.GetLastRunRouteSummaryForCity(linkedCityId);
                if (HasMeaningfulSurfaceText(routeSummary))
                {
                    parts.Add(routeSummary);
                }

                string lootSummary = _runtimeEconomyState.GetLastRunLootSummaryForCity(linkedCityId);
                if (HasMeaningfulSurfaceText(lootSummary))
                {
                    parts.Add(lootSummary);
                }
            }
        }

        return parts.Count > 0 ? string.Join(" | ", parts.ToArray()) : "None";
    }

    private bool HasMeaningfulSurfaceText(string value)
    {
        return !string.IsNullOrEmpty(value) && value != "None" && value != "(missing)";
    }

    private string CompactSurfaceText(string value, int maxLength)
    {
        if (string.IsNullOrEmpty(value))
        {
            return "None";
        }

        string compact = value.Replace('\r', ' ').Replace('\n', ' ').Trim();
        while (compact.Contains("  "))
        {
            compact = compact.Replace("  ", " ");
        }

        if (maxLength > 3 && compact.Length > maxLength)
        {
            compact = compact.Substring(0, maxLength - 3) + "...";
        }

        return string.IsNullOrEmpty(compact) ? "None" : compact;
    }
}













