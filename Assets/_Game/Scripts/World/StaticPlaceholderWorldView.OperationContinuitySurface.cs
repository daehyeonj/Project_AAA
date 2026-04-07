using System;
using System.Collections.Generic;
using UnityEngine;

public sealed partial class StaticPlaceholderWorldView
{
    private string _currentExpeditionStagingSummaryText = "None";
    private string _currentReturnAftermathSummaryText = "None";
    private string _currentAlertRibbonSummaryText = "None";
    private string _latestReturnAftermathCityId = string.Empty;
    private string _latestReturnAftermathCityLabel = "None";
    private string _latestReturnAftermathDungeonId = string.Empty;
    private string _latestReturnAftermathDungeonLabel = "None";
    private string _latestReturnAftermathRouteText = "None";
    private string _latestReturnAftermathSummaryText = "None";

    public string CurrentExpeditionStagingSummaryText => string.IsNullOrEmpty(_currentExpeditionStagingSummaryText) ? "None" : _currentExpeditionStagingSummaryText;
    public string CurrentReturnAftermathSummaryText => string.IsNullOrEmpty(_currentReturnAftermathSummaryText) ? "None" : _currentReturnAftermathSummaryText;
    public string CurrentAlertRibbonSummaryText => string.IsNullOrEmpty(_currentAlertRibbonSummaryText) ? "None" : _currentAlertRibbonSummaryText;

    private string BuildExpeditionStagingSummaryText(PrototypeWorldSimOperationChainSurfaceData chain)
    {
        if (chain == null || !chain.HasSelection)
        {
            return "Select a city or dungeon to read the current staging lane.";
        }

        string cityId = GetChainCityId(chain);
        string dungeonId = GetChainDungeonId(chain);
        string cityLabel = GetChainCityLabel(chain);
        string dungeonLabel = GetChainDungeonLabel(chain);
        int idlePartyCount = !string.IsNullOrEmpty(cityId) && _runtimeEconomyState != null ? _runtimeEconomyState.GetIdlePartyCountInCity(cityId) : 0;
        int activeExpeditionCount = !string.IsNullOrEmpty(cityId) && _runtimeEconomyState != null ? _runtimeEconomyState.GetActiveExpeditionCountFromCity(cityId) : 0;
        string actionStateKey = ResolvePrimaryWorldSimActionState(chain, activeExpeditionCount);
        string activePartySummaryText = BuildActivePartySummaryText(cityId, idlePartyCount, activeExpeditionCount);
        string stagingHintText = BuildStagingHintText(actionStateKey, cityLabel, dungeonLabel, chain.ReadinessSummaryText, chain.RouteSummaryText);
        string laneSummary = !string.IsNullOrEmpty(cityLabel) && cityLabel != "None" && !string.IsNullOrEmpty(dungeonLabel) && dungeonLabel != "None"
            ? cityLabel + " -> " + dungeonLabel
            : chain.SelectedEntityLabel;

        return laneSummary + "\n" +
               "Party: " + CompactSurfaceText(activePartySummaryText, 104) + "\n" +
               "Readiness: " + CompactSurfaceText(chain.ReadinessSummaryText, 72) + "\n" +
               "Staging Hint: " + CompactSurfaceText(stagingHintText, 112);
    }

    private string BuildReturnAftermathSummaryText(PrototypeWorldSimOperationChainSurfaceData chain)
    {
        if (!HasLatestReturnAftermath())
        {
            return "None";
        }

        string prefix = chain != null && chain.HasSelection && MatchesLatestReturnAftermathChain(chain)
            ? "Pinned to current chain"
            : "Latest return remains on board";
        return prefix + ":\n" + _latestReturnAftermathSummaryText;
    }

    private string BuildAlertRibbonSummaryText(PrototypeWorldSimOperationChainSurfaceData chain)
    {
        List<string> tokens = new List<string>(5);
        tokens.Add(AutoTickEnabled ? (AutoTickPaused ? "Auto Tick paused" : "Auto Tick running") : "Manual day step");
        if (HasMeaningfulSurfaceText(_currentNetworkPressureSummaryText))
        {
            tokens.Add("Pressure: " + CompactSurfaceText(_currentNetworkPressureSummaryText, 44));
        }

        if (HasMeaningfulSurfaceText(_currentRouteSaturationSummaryText))
        {
            tokens.Add("Route: " + CompactSurfaceText(_currentRouteSaturationSummaryText, 40));
        }

        if (HasMeaningfulSurfaceText(_currentRecoveryForecastSummaryText))
        {
            tokens.Add("Forecast: " + CompactSurfaceText(_currentRecoveryForecastSummaryText, 40));
        }

        tokens.Add(chain != null && chain.HasSelection ? BuildAlertTokenForChain(chain) : "Board idle");

        if (HasLatestReturnAftermath())
        {
            tokens.Add("Return pinned");
        }
        else if (ActiveExpeditions > 0)
        {
            tokens.Add(ActiveExpeditions == 1 ? "1 active expedition" : ActiveExpeditions + " active expeditions");
        }

        return string.Join(" | ", tokens.ToArray());
    }

    private string BuildAlertTokenForChain(PrototypeWorldSimOperationChainSurfaceData chain)
    {
        if (chain == null || !chain.HasSelection)
        {
            return "Board idle";
        }

        string actionStateKey = ResolvePrimaryWorldSimActionState(chain, GetChainActiveExpeditionCount(chain));
        string cityLabel = GetChainCityLabel(chain);

        if (actionStateKey == "enter_ready" || actionStateKey == "inspect_linked_city")
        {
            return cityLabel + " commit-ready";
        }

        if (actionStateKey == "stage_party")
        {
            return cityLabel + " roster compare ready";
        }

        if (actionStateKey == "stage_waiting_lane")
        {
            return cityLabel + " reserve staged behind return";
        }

        if (actionStateKey == "recruit_ready")
        {
            return cityLabel + " needs recruitment";
        }

        if (actionStateKey == "wait_active_party")
        {
            return cityLabel + " awaiting return";
        }

        if (actionStateKey == "blocked_no_linked_dungeon" || actionStateKey == "blocked_no_linked_city")
        {
            return "Link missing on current chain";
        }

        return "Advance world for next operation";
    }

    private string BuildActivePartySummaryText(string cityId, int idlePartyCount, int activeExpeditionCount)
    {
        if (_runtimeEconomyState == null || string.IsNullOrEmpty(cityId))
        {
            return "No staged party.";
        }

        if (activeExpeditionCount > 0)
        {
            return "Active expedition: " + CompactSurfaceText(_runtimeEconomyState.GetExpeditionStatusTextForCity(cityId), 78);
        }

        string stagedPartyId = ResolveStagedPartyIdForCity(cityId);
        if (!string.IsNullOrEmpty(stagedPartyId))
        {
            return "Staged reserve: " + ResolveWorldPartyDisplayName(stagedPartyId);
        }

        string idlePartyId = _runtimeEconomyState.GetIdlePartyIdInCity(cityId);
        if (!string.IsNullOrEmpty(idlePartyId))
        {
            return "Ready reserve: " + BuildPartyLabelText(idlePartyId);
        }

        return idlePartyCount > 0 ? idlePartyCount + " ready reserve companies." : "No staged party.";
    }

    private string BuildPartyLabelText(string partyId)
    {
        if (string.IsNullOrEmpty(partyId))
        {
            return "Party";
        }

        string safeLabel = partyId.Replace('-', ' ').Replace('_', ' ').Trim();
        if (safeLabel.Length <= 0)
        {
            return "Party";
        }

        return char.ToUpperInvariant(safeLabel[0]) + (safeLabel.Length > 1 ? safeLabel.Substring(1) : string.Empty);
    }

    private string BuildStagingHintText(string actionStateKey, string cityLabel, string dungeonLabel, string readinessSummary, string routeSummary)
    {
        if (actionStateKey == "enter_ready" || actionStateKey == "inspect_linked_city")
        {
            return "Lane primed for " + dungeonLabel + ". Route: " + CompactSurfaceText(routeSummary, 52) + ".";
        }

        if (actionStateKey == "stage_party")
        {
            return "Compare reserve companies, then stage the best fit before committing this lane.";
        }

        if (actionStateKey == "stage_waiting_lane")
        {
            return "Another expedition is still out. Keep one reserve company staged so the return window is ready.";
        }

        if (actionStateKey == "recruit_ready")
        {
            return cityLabel + " needs one recruit to open the dispatch lane.";
        }

        if (actionStateKey == "wait_active_party")
        {
            return "Another expedition still occupies the lane. Wait for the return to resolve.";
        }

        if (actionStateKey == "blocked_no_linked_dungeon")
        {
            return cityLabel + " has no linked dungeon lane yet.";
        }

        if (actionStateKey == "blocked_no_linked_city")
        {
            return dungeonLabel + " has no linked city staging lane yet.";
        }

        return "Readiness sits at " + CompactSurfaceText(readinessSummary, 40) + ". Advance the world to reopen the lane.";
    }

    private string ResolvePrimaryWorldSimActionState(PrototypeWorldSimOperationChainSurfaceData chain, int activeExpeditionCount)
    {
        if (chain == null || !chain.HasSelection)
        {
            return "none";
        }

        if (HasMeaningfulSurfaceText(chain.ActionStateKey) && !string.Equals(chain.ActionStateKey, "none", StringComparison.OrdinalIgnoreCase))
        {
            return chain.ActionStateKey;
        }

        string cityId = GetChainCityId(chain);
        string dungeonId = GetChainDungeonId(chain);
        bool hasLinkedNode = !string.IsNullOrEmpty(cityId) && !string.IsNullOrEmpty(dungeonId);
        bool canRecruit = !string.IsNullOrEmpty(cityId) && ResolveCanRecruitSelectedCityParty(cityId);
        bool canCommit = !string.IsNullOrEmpty(cityId) && ResolveCanCommitSelectedCityDungeon(cityId);
        bool hasRoster = !string.IsNullOrEmpty(cityId) && ResolveCanOpenCityPartyRosterBoard(cityId);
        string readyStateKey = chain.SelectedEntityKind == nameof(WorldEntityKind.City) ? "enter_ready" : "inspect_linked_city";
        string blockedStateKey = chain.SelectedEntityKind == nameof(WorldEntityKind.City) ? "blocked_no_linked_dungeon" : "blocked_no_linked_city";

        if (canCommit)
        {
            return readyStateKey;
        }

        if (!hasLinkedNode)
        {
            return blockedStateKey;
        }

        if (hasRoster)
        {
            return activeExpeditionCount > 0 ? "stage_waiting_lane" : "stage_party";
        }

        if (canRecruit)
        {
            return "recruit_ready";
        }

        if (activeExpeditionCount > 0)
        {
            return "wait_active_party";
        }

        return "advance_world";
    }

    private int GetChainActiveExpeditionCount(PrototypeWorldSimOperationChainSurfaceData chain)
    {
        string cityId = GetChainCityId(chain);
        return !string.IsNullOrEmpty(cityId) && _runtimeEconomyState != null ? _runtimeEconomyState.GetActiveExpeditionCountFromCity(cityId) : 0;
    }

    private string GetChainCityId(PrototypeWorldSimOperationChainSurfaceData chain)
    {
        if (chain == null || !chain.HasSelection)
        {
            return string.Empty;
        }

        return chain.SelectedEntityKind == nameof(WorldEntityKind.City) ? chain.SelectedEntityId : chain.LinkedEntityId;
    }

    private string GetChainDungeonId(PrototypeWorldSimOperationChainSurfaceData chain)
    {
        if (chain == null || !chain.HasSelection)
        {
            return string.Empty;
        }

        return chain.SelectedEntityKind == nameof(WorldEntityKind.Dungeon) ? chain.SelectedEntityId : chain.LinkedEntityId;
    }

    private string GetChainCityLabel(PrototypeWorldSimOperationChainSurfaceData chain)
    {
        if (chain == null || !chain.HasSelection)
        {
            return "None";
        }

        return chain.SelectedEntityKind == nameof(WorldEntityKind.City) ? chain.SelectedEntityLabel : chain.LinkedEntityLabel;
    }

    private string GetChainDungeonLabel(PrototypeWorldSimOperationChainSurfaceData chain)
    {
        if (chain == null || !chain.HasSelection)
        {
            return "None";
        }

        return chain.SelectedEntityKind == nameof(WorldEntityKind.Dungeon) ? chain.SelectedEntityLabel : chain.LinkedEntityLabel;
    }

    private void ApplySelectedOutcomePins(PrototypeWorldSimOperationChainSurfaceData chain)
    {
        if (chain == null || !chain.HasSelection)
        {
            return;
        }

        if (_markerByEntityId.TryGetValue(chain.SelectedEntityId, out WorldSelectableMarker selectedMarker) && selectedMarker != null)
        {
            selectedMarker.SetOutcomePinned(true);
        }

        if (!string.IsNullOrEmpty(chain.LinkedEntityId) && _markerByEntityId.TryGetValue(chain.LinkedEntityId, out WorldSelectableMarker linkedMarker) && linkedMarker != null)
        {
            linkedMarker.SetOutcomePinned(true);
        }
    }

    private void CaptureWorldReturnAftermath(string cityId, string cityLabel, string dungeonId, string dungeonLabel, string routeSummaryText, string resultSummaryText, string survivingSummaryText, string lootSummaryText, string notableSummaryText)
    {
        _latestReturnAftermathCityId = string.IsNullOrEmpty(cityId) ? string.Empty : cityId;
        _latestReturnAftermathCityLabel = string.IsNullOrEmpty(cityLabel) ? "None" : cityLabel;
        _latestReturnAftermathDungeonId = string.IsNullOrEmpty(dungeonId) ? string.Empty : dungeonId;
        _latestReturnAftermathDungeonLabel = string.IsNullOrEmpty(dungeonLabel) ? "None" : dungeonLabel;
        _latestReturnAftermathRouteText = string.IsNullOrEmpty(routeSummaryText) ? "None" : routeSummaryText;

        string header = CompactSurfaceText(_latestReturnAftermathCityLabel + " -> " + _latestReturnAftermathDungeonLabel + " | " + _latestReturnAftermathRouteText, 112);
        List<string> detailParts = new List<string>(4);
        if (HasMeaningfulSurfaceText(resultSummaryText))
        {
            detailParts.Add(CompactSurfaceText(resultSummaryText, 56));
        }

        if (HasMeaningfulSurfaceText(survivingSummaryText))
        {
            detailParts.Add("Survivors: " + CompactSurfaceText(survivingSummaryText, 40));
        }

        if (HasMeaningfulSurfaceText(lootSummaryText))
        {
            detailParts.Add("Loot: " + CompactSurfaceText(lootSummaryText, 40));
        }

        if (HasMeaningfulSurfaceText(notableSummaryText))
        {
            detailParts.Add("Notable: " + CompactSurfaceText(notableSummaryText, 40));
        }

        _latestReturnAftermathSummaryText = header + (detailParts.Count > 0 ? "\n" + string.Join(" | ", detailParts.ToArray()) : string.Empty);

        if (_root != null && _root.activeInHierarchy)
        {
            RefreshWorldOperationSurface();
        }
    }

    private void ClearLatestReturnAftermathState(bool refreshSurface)
    {
        _latestReturnAftermathCityId = string.Empty;
        _latestReturnAftermathCityLabel = "None";
        _latestReturnAftermathDungeonId = string.Empty;
        _latestReturnAftermathDungeonLabel = "None";
        _latestReturnAftermathRouteText = "None";
        _latestReturnAftermathSummaryText = "None";

        if (refreshSurface)
        {
            RefreshWorldOperationSurface();
        }
    }

    private void ResetWorldOperationContinuityState()
    {
        _currentExpeditionStagingSummaryText = "None";
        _currentReturnAftermathSummaryText = "None";
        _currentAlertRibbonSummaryText = "None";
        ClearLatestReturnAftermathState(false);
    }

    private bool HasLatestReturnAftermath()
    {
        return HasMeaningfulSurfaceText(_latestReturnAftermathSummaryText) &&
               (!string.IsNullOrEmpty(_latestReturnAftermathCityId) || !string.IsNullOrEmpty(_latestReturnAftermathDungeonId));
    }

    private bool MatchesLatestReturnAftermathChain(PrototypeWorldSimOperationChainSurfaceData chain)
    {
        if (chain == null || !chain.HasSelection || !HasLatestReturnAftermath())
        {
            return false;
        }

        return MatchesLatestReturnAftermathEntity(chain.SelectedEntityId) || MatchesLatestReturnAftermathEntity(chain.LinkedEntityId);
    }

    private bool MatchesLatestReturnAftermathEntity(string entityId)
    {
        if (string.IsNullOrEmpty(entityId) || !HasLatestReturnAftermath())
        {
            return false;
        }

        return string.Equals(entityId, _latestReturnAftermathCityId, StringComparison.OrdinalIgnoreCase) ||
               string.Equals(entityId, _latestReturnAftermathDungeonId, StringComparison.OrdinalIgnoreCase);
    }

    private bool MatchesLatestReturnAftermathConnector(string cityId, string dungeonId)
    {
        if (!HasLatestReturnAftermath())
        {
            return false;
        }

        return string.Equals(cityId, _latestReturnAftermathCityId, StringComparison.OrdinalIgnoreCase) &&
               string.Equals(dungeonId, _latestReturnAftermathDungeonId, StringComparison.OrdinalIgnoreCase);
    }

    private void ApplyLatestReturnAftermathPins(string selectedConnectorKey)
    {
        if (!string.IsNullOrEmpty(_latestReturnAftermathCityId) && _markerByEntityId.TryGetValue(_latestReturnAftermathCityId, out WorldSelectableMarker cityMarker) && cityMarker != null)
        {
            cityMarker.SetOutcomePinned(true);
        }

        if (!string.IsNullOrEmpty(_latestReturnAftermathDungeonId) && _markerByEntityId.TryGetValue(_latestReturnAftermathDungeonId, out WorldSelectableMarker dungeonMarker) && dungeonMarker != null)
        {
            dungeonMarker.SetOutcomePinned(true);
        }

        string connectorKey = BuildLinkedConnectorKey(_latestReturnAftermathCityId, _latestReturnAftermathDungeonId);
        if (!string.IsNullOrEmpty(connectorKey) && _linkedConnectorByKey.TryGetValue(connectorKey, out WorldBoardConnectorPulseVisual connectorVisual) && connectorVisual != null)
        {
            connectorVisual.ApplyPulse(connectorKey == selectedConnectorKey, true);
        }
    }
}



