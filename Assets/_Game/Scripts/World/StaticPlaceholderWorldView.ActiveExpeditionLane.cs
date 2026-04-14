public sealed partial class StaticPlaceholderWorldView
{
    private sealed class PrototypeWorldExpeditionTravelSummary
    {
        public string ActiveLaneText = "None";
        public string DepartureEchoText = "None";
        public string ReturnEtaText = "None";
        public string CityVacancyLabel = "None";
        public string DungeonInboundLabel = "None";
        public string RouteOccupancyLabel = "None";
    }

    private sealed class PrototypeWorldReturnWindowSnapshot
    {
        public string ReturnWindowLabel = "None";
        public string RecoveryAfterReturnLabel = "None";
        public string AftermathHintKey = "none";
    }

    private sealed class PrototypeWorldActiveExpeditionSnapshot
    {
        public string CityId = string.Empty;
        public string CityLabel = "None";
        public string PartyId = string.Empty;
        public string PartyLabel = "None";
        public string DungeonId = string.Empty;
        public string DungeonLabel = "None";
        public string RouteId = string.Empty;
        public string RouteLabel = "None";
        public string LaunchStateKey = "idle";
        public string TravelStateKey = "idle";
        public int DepartureDay = -1;
        public int ProjectedReturnDay = -1;
        public int DaysRemaining = 0;
        public bool IsInFlight;
        public bool IsReturning;
        public bool IsReadyForResolve;
        public PrototypeWorldExpeditionTravelSummary TravelSummary = new PrototypeWorldExpeditionTravelSummary();
        public PrototypeWorldReturnWindowSnapshot ReturnWindow = new PrototypeWorldReturnWindowSnapshot();
    }

    public string SelectedActiveExpeditionLaneText => BuildSelectedActiveExpeditionSnapshot().TravelSummary.ActiveLaneText;
    public string SelectedDepartureEchoText => BuildSelectedActiveExpeditionSnapshot().TravelSummary.DepartureEchoText;
    public string SelectedReturnEtaText => BuildSelectedActiveExpeditionSnapshot().TravelSummary.ReturnEtaText;
    public string SelectedCityVacancyText => BuildSelectedActiveExpeditionSnapshot().TravelSummary.CityVacancyLabel;
    public string SelectedDungeonInboundExpeditionText => BuildSelectedActiveExpeditionSnapshot().TravelSummary.DungeonInboundLabel;
    public string SelectedRouteOccupancyText => BuildSelectedActiveExpeditionSnapshot().TravelSummary.RouteOccupancyLabel;
    public string SelectedReturnWindowText => BuildSelectedActiveExpeditionSnapshot().ReturnWindow.ReturnWindowLabel;
    public string SelectedRecoveryAfterReturnText => BuildSelectedActiveExpeditionSnapshot().ReturnWindow.RecoveryAfterReturnLabel;
    public string WorldActiveExpeditionLaneText => BuildWorldActiveExpeditionSnapshot().TravelSummary.ActiveLaneText;
    public string WorldDepartureEchoText => BuildWorldActiveExpeditionSnapshot().TravelSummary.DepartureEchoText;
    public string WorldReturnEtaText => BuildWorldActiveExpeditionSnapshot().TravelSummary.ReturnEtaText;

    private PrototypeWorldActiveExpeditionSnapshot BuildSelectedActiveExpeditionSnapshot()
    {
        if (_selectedMarker == null)
        {
            return BuildWorldActiveExpeditionSnapshot();
        }

        string cityId = _selectedMarker.EntityData.Kind == WorldEntityKind.City
            ? _selectedMarker.EntityData.Id
            : _selectedMarker.EntityData.Kind == WorldEntityKind.Dungeon
                ? _selectedMarker.EntityData.LinkedCityId ?? string.Empty
                : string.Empty;
        string dungeonId = _selectedMarker.EntityData.Kind == WorldEntityKind.Dungeon
            ? _selectedMarker.EntityData.Id
            : _selectedMarker.EntityData.Kind == WorldEntityKind.City
                ? GetLinkedDungeonIdForCity(cityId)
                : string.Empty;
        return BuildActiveExpeditionSnapshot(cityId, dungeonId, false);
    }

    private PrototypeWorldActiveExpeditionSnapshot BuildWorldActiveExpeditionSnapshot()
    {
        string cityId = _runtimeEconomyState != null ? _runtimeEconomyState.GetFirstActiveExpeditionCityId() : string.Empty;
        string dungeonId = _runtimeEconomyState != null ? _runtimeEconomyState.GetFirstActiveExpeditionDungeonId() : string.Empty;
        return BuildActiveExpeditionSnapshot(cityId, dungeonId, true);
    }

    private PrototypeWorldActiveExpeditionSnapshot BuildActiveExpeditionSnapshot(string cityId, string dungeonId, bool isWorldFallback)
    {
        PrototypeWorldActiveExpeditionSnapshot snapshot = new PrototypeWorldActiveExpeditionSnapshot();
        snapshot.CityId = cityId ?? string.Empty;
        snapshot.CityLabel = ResolveDispatchEntityDisplayName(snapshot.CityId);
        snapshot.DungeonId = dungeonId ?? string.Empty;
        snapshot.DungeonLabel = ResolveDispatchEntityDisplayName(snapshot.DungeonId);
        snapshot.PartyId = ResolveActivePartyId(snapshot.CityId, snapshot.DungeonId);
        snapshot.PartyLabel = string.IsNullOrEmpty(snapshot.PartyId) ? "None" : snapshot.PartyId;
        snapshot.RouteId = ResolveActiveRouteId(snapshot.CityId, snapshot.DungeonId);
        snapshot.RouteLabel = BuildActiveRouteLabel(snapshot.DungeonId, snapshot.RouteId);
        snapshot.DaysRemaining = ResolveActiveDaysRemaining(snapshot.CityId, snapshot.DungeonId);
        snapshot.DepartureDay = ResolveActiveDepartureDay(snapshot.CityId);
        snapshot.ProjectedReturnDay = ResolveProjectedReturnDay(snapshot.CityId, snapshot.DungeonId);
        snapshot.IsInFlight = !string.IsNullOrEmpty(snapshot.PartyId) && snapshot.DaysRemaining > 0;
        snapshot.IsReturning = snapshot.IsInFlight && snapshot.DaysRemaining <= 1;
        snapshot.IsReadyForResolve = !string.IsNullOrEmpty(snapshot.PartyId) && snapshot.DaysRemaining <= 0;
        snapshot.LaunchStateKey = snapshot.IsInFlight ? "launched" : "idle";
        snapshot.TravelStateKey = !snapshot.IsInFlight
            ? "idle"
            : snapshot.IsReturning
                ? "returning"
                : "in_flight";

        snapshot.TravelSummary.ActiveLaneText = BuildActiveLaneText(snapshot, isWorldFallback);
        snapshot.TravelSummary.DepartureEchoText = BuildDepartureEchoText(snapshot, isWorldFallback);
        snapshot.TravelSummary.ReturnEtaText = BuildReturnEtaText(snapshot, isWorldFallback);
        snapshot.TravelSummary.CityVacancyLabel = BuildCityVacancyText(snapshot);
        snapshot.TravelSummary.DungeonInboundLabel = BuildDungeonInboundText(snapshot);
        snapshot.TravelSummary.RouteOccupancyLabel = BuildRouteOccupancyText(snapshot);
        snapshot.ReturnWindow.ReturnWindowLabel = BuildReturnWindowText(snapshot, isWorldFallback);
        snapshot.ReturnWindow.RecoveryAfterReturnLabel = BuildRecoveryAfterReturnText(snapshot);
        snapshot.ReturnWindow.AftermathHintKey = BuildAftermathHintKey(snapshot);
        return snapshot;
    }

    private string ResolveActivePartyId(string cityId, string dungeonId)
    {
        if (_runtimeEconomyState == null)
        {
            return string.Empty;
        }

        string partyId = !string.IsNullOrEmpty(cityId)
            ? _runtimeEconomyState.GetActivePartyIdForCity(cityId)
            : string.Empty;
        if (!string.IsNullOrEmpty(partyId))
        {
            return partyId;
        }

        return !string.IsNullOrEmpty(dungeonId)
            ? _runtimeEconomyState.GetActivePartyIdForDungeon(dungeonId)
            : string.Empty;
    }

    private string ResolveActiveRouteId(string cityId, string dungeonId)
    {
        if (_runtimeEconomyState == null)
        {
            return string.Empty;
        }

        string routeId = !string.IsNullOrEmpty(cityId)
            ? _runtimeEconomyState.GetActiveRouteIdForCity(cityId)
            : string.Empty;
        if (!string.IsNullOrEmpty(routeId))
        {
            return routeId;
        }

        return !string.IsNullOrEmpty(dungeonId)
            ? _runtimeEconomyState.GetActiveRouteIdForDungeon(dungeonId)
            : string.Empty;
    }

    private int ResolveActiveDaysRemaining(string cityId, string dungeonId)
    {
        if (_runtimeEconomyState == null)
        {
            return 0;
        }

        int daysRemaining = !string.IsNullOrEmpty(cityId)
            ? _runtimeEconomyState.GetActiveDaysRemainingForCity(cityId)
            : 0;
        if (daysRemaining > 0)
        {
            return daysRemaining;
        }

        return !string.IsNullOrEmpty(dungeonId)
            ? _runtimeEconomyState.GetActiveDaysRemainingForDungeon(dungeonId)
            : 0;
    }

    private int ResolveActiveDepartureDay(string cityId)
    {
        return _runtimeEconomyState != null && !string.IsNullOrEmpty(cityId)
            ? _runtimeEconomyState.GetActiveDepartureDayForCity(cityId)
            : -1;
    }

    private int ResolveProjectedReturnDay(string cityId, string dungeonId)
    {
        if (_runtimeEconomyState == null)
        {
            return -1;
        }

        int projectedReturnDay = !string.IsNullOrEmpty(cityId)
            ? _runtimeEconomyState.GetActiveProjectedReturnDayForCity(cityId)
            : -1;
        if (projectedReturnDay >= 0)
        {
            return projectedReturnDay;
        }

        return !string.IsNullOrEmpty(dungeonId)
            ? _runtimeEconomyState.GetActiveProjectedReturnDayForDungeon(dungeonId)
            : -1;
    }

    private string BuildActiveRouteLabel(string dungeonId, string routeId)
    {
        if (string.IsNullOrEmpty(routeId))
        {
            return "None";
        }

        string routeLabel = !string.IsNullOrEmpty(dungeonId) ? BuildDispatchRouteLabel(dungeonId, routeId) : "None";
        return routeLabel == "None"
            ? routeId.Replace('_', ' ')
            : routeLabel;
    }

    private string BuildActiveLaneText(PrototypeWorldActiveExpeditionSnapshot snapshot, bool isWorldFallback)
    {
        if (snapshot == null)
        {
            return "None";
        }

        if (snapshot.IsInFlight)
        {
            string routeEcho = snapshot.RouteLabel == "None" ? string.Empty : " via " + snapshot.RouteLabel;
            return snapshot.PartyLabel + " is en route to " + snapshot.DungeonLabel + routeEcho + ".";
        }

        return isWorldFallback
            ? "No expedition is currently in flight."
            : "No active expedition is moving from this selection.";
    }

    private string BuildDepartureEchoText(PrototypeWorldActiveExpeditionSnapshot snapshot, bool isWorldFallback)
    {
        if (snapshot == null)
        {
            return "None";
        }

        if (snapshot.IsInFlight)
        {
            string dayText = snapshot.DepartureDay >= 0 ? "Day " + snapshot.DepartureDay : "Recent launch";
            string routeEcho = snapshot.RouteLabel == "None" ? string.Empty : " via " + snapshot.RouteLabel;
            return dayText + " launch: " + snapshot.CityLabel + " sent " + snapshot.PartyLabel + " toward " + snapshot.DungeonLabel + routeEcho + ".";
        }

        string recentEcho = FindRecentExpeditionEcho(snapshot.CityLabel, snapshot.DungeonLabel, snapshot.PartyLabel);
        if (recentEcho != "None")
        {
            return recentEcho;
        }

        return isWorldFallback ? "No recent launch echo is highlighted." : "No recent launch echo is attached to this selection.";
    }

    private string BuildReturnEtaText(PrototypeWorldActiveExpeditionSnapshot snapshot, bool isWorldFallback)
    {
        if (snapshot == null)
        {
            return "None";
        }

        if (snapshot.IsInFlight)
        {
            string etaText = snapshot.ProjectedReturnDay >= 0
                ? "Day " + snapshot.ProjectedReturnDay
                : "pending return";
            return etaText + " expected | " + BuildDayCountText(snapshot.DaysRemaining) + " remaining.";
        }

        int recoveryDays = !string.IsNullOrEmpty(snapshot.CityId) ? GetRecoveryDaysToReady(snapshot.CityId) : 0;
        if (recoveryDays > 0)
        {
            return "No party is in flight. Recovery still needs " + BuildRecoveryEtaText(recoveryDays) + ".";
        }

        return isWorldFallback ? "No expedition return ETA is active." : "No return ETA is active for this selection.";
    }

    private string BuildCityVacancyText(PrototypeWorldActiveExpeditionSnapshot snapshot)
    {
        if (snapshot == null || string.IsNullOrEmpty(snapshot.CityId) || _runtimeEconomyState == null)
        {
            return "None";
        }

        int totalParties = _runtimeEconomyState.GetPartyCountInCity(snapshot.CityId);
        int idleParties = _runtimeEconomyState.GetIdlePartyCountInCity(snapshot.CityId);
        int activeExpeditions = _runtimeEconomyState.GetActiveExpeditionCountFromCity(snapshot.CityId);
        int recoveryDays = GetRecoveryDaysToReady(snapshot.CityId);
        if (activeExpeditions > 0 && idleParties <= 0)
        {
            return "No ready party is left in " + snapshot.CityLabel + ".";
        }

        if (activeExpeditions > 0 && idleParties > 0)
        {
            return snapshot.CityLabel + " still has reserve coverage while one party is away.";
        }

        if (recoveryDays > 0)
        {
            return snapshot.CityLabel + " is covered, but dispatch recovery is still ticking.";
        }

        if (totalParties > 0 && idleParties > 0)
        {
            return snapshot.CityLabel + " still has a ready party on standby.";
        }

        return "No party is stationed in " + snapshot.CityLabel + ".";
    }

    private string BuildDungeonInboundText(PrototypeWorldActiveExpeditionSnapshot snapshot)
    {
        if (snapshot == null || string.IsNullOrEmpty(snapshot.DungeonId))
        {
            return "None";
        }

        if (snapshot.IsInFlight)
        {
            string routeEcho = snapshot.RouteLabel == "None" ? string.Empty : " via " + snapshot.RouteLabel;
            return snapshot.PartyLabel + " is inbound from " + snapshot.CityLabel + routeEcho + ".";
        }

        string recentEcho = FindRecentExpeditionEcho(snapshot.CityLabel, snapshot.DungeonLabel, snapshot.PartyLabel);
        return recentEcho != "None"
            ? "No inbound party. Last echo: " + recentEcho
            : "No inbound expedition is targeting this dungeon.";
    }

    private string BuildRouteOccupancyText(PrototypeWorldActiveExpeditionSnapshot snapshot)
    {
        if (snapshot == null)
        {
            return "None";
        }

        if (snapshot.IsInFlight)
        {
            return snapshot.RouteLabel == "None"
                ? "A launch lane is occupied by an active expedition."
                : snapshot.RouteLabel + " is currently active.";
        }

        if (!string.IsNullOrEmpty(snapshot.CityId) && GetRecoveryDaysToReady(snapshot.CityId) > 0)
        {
            return "Launch lane is cooling after the last dispatch.";
        }

        return "Launch lane is idle.";
    }

    private string BuildReturnWindowText(PrototypeWorldActiveExpeditionSnapshot snapshot, bool isWorldFallback)
    {
        if (snapshot == null)
        {
            return "None";
        }

        if (snapshot.IsInFlight)
        {
            string dayText = snapshot.ProjectedReturnDay >= 0 ? "Day " + snapshot.ProjectedReturnDay : "Pending";
            return "Return window: " + dayText + " | " + BuildDayCountText(snapshot.DaysRemaining) + " remaining.";
        }

        int recoveryDays = !string.IsNullOrEmpty(snapshot.CityId) ? GetRecoveryDaysToReady(snapshot.CityId) : 0;
        if (recoveryDays > 0)
        {
            return "Return window closed. Recovery remains for " + BuildRecoveryEtaText(recoveryDays) + ".";
        }

        return isWorldFallback ? "No active return window is open." : "No return window is attached to this selection.";
    }

    private string BuildRecoveryAfterReturnText(PrototypeWorldActiveExpeditionSnapshot snapshot)
    {
        if (snapshot == null || string.IsNullOrEmpty(snapshot.CityId))
        {
            return "None";
        }

        int recoveryDays = GetRecoveryDaysToReady(snapshot.CityId);
        if (snapshot.IsInFlight)
        {
            return recoveryDays > 0
                ? "After return, recovery still points to " + BuildRecoveryEtaText(recoveryDays) + "."
                : "After return, the city should be ready for another dispatch.";
        }

        return recoveryDays > 0
            ? "Recovery after return: " + BuildRecoveryEtaText(recoveryDays)
            : "Recovery after return is clear.";
    }

    private string BuildAftermathHintKey(PrototypeWorldActiveExpeditionSnapshot snapshot)
    {
        if (snapshot == null)
        {
            return "none";
        }

        if (snapshot.IsInFlight)
        {
            return "in_flight";
        }

        return !string.IsNullOrEmpty(snapshot.CityId) && GetRecoveryDaysToReady(snapshot.CityId) > 0
            ? "recovery_after_return"
            : "idle";
    }

    private string FindRecentExpeditionEcho(string cityLabel, string dungeonLabel, string partyLabel)
    {
        if (_runtimeEconomyState == null)
        {
            return "None";
        }

        for (int index = 0; index < 3; index++)
        {
            string echo = _runtimeEconomyState.GetRecentExpeditionLogText(index);
            if (string.IsNullOrEmpty(echo) || echo == "None")
            {
                continue;
            }

            if ((!string.IsNullOrEmpty(cityLabel) && cityLabel != "None" && echo.Contains(cityLabel)) ||
                (!string.IsNullOrEmpty(dungeonLabel) && dungeonLabel != "None" && echo.Contains(dungeonLabel)) ||
                (!string.IsNullOrEmpty(partyLabel) && partyLabel != "None" && echo.Contains(partyLabel)))
            {
                return echo;
            }
        }

        return "None";
    }
}
