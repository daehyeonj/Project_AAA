using System;
using System.Collections.Generic;

public sealed partial class StaticPlaceholderWorldView
{
    private sealed class PrototypeWorldPartyRosterEntryData
    {
        public readonly string PartyId;
        public readonly string CityId;
        public readonly string DisplayName;
        public readonly string ArchetypeKey;
        public readonly string RoleMixSummary;
        public readonly string RouteFitSummary;
        public readonly string DungeonFitSummary;
        public readonly string ReadinessFitSummary;
        public readonly string RecoverySummary;
        public readonly string LastRunSummary;
        public readonly string StateKey;
        public readonly string BlockedReasonText;
        public readonly bool IsRecommended;
        public readonly bool IsSelected;
        public readonly bool IsStaged;
        public readonly bool IsBlocked;

        public PrototypeWorldPartyRosterEntryData(
            string partyId,
            string cityId,
            string displayName,
            string archetypeKey,
            string roleMixSummary,
            string routeFitSummary,
            string dungeonFitSummary,
            string readinessFitSummary,
            string recoverySummary,
            string lastRunSummary,
            string stateKey,
            string blockedReasonText,
            bool isRecommended,
            bool isSelected,
            bool isStaged,
            bool isBlocked)
        {
            PartyId = string.IsNullOrEmpty(partyId) ? string.Empty : partyId;
            CityId = string.IsNullOrEmpty(cityId) ? string.Empty : cityId;
            DisplayName = string.IsNullOrEmpty(displayName) ? "None" : displayName;
            ArchetypeKey = string.IsNullOrEmpty(archetypeKey) ? "balanced_company" : archetypeKey;
            RoleMixSummary = string.IsNullOrEmpty(roleMixSummary) ? "None" : roleMixSummary;
            RouteFitSummary = string.IsNullOrEmpty(routeFitSummary) ? "None" : routeFitSummary;
            DungeonFitSummary = string.IsNullOrEmpty(dungeonFitSummary) ? "None" : dungeonFitSummary;
            ReadinessFitSummary = string.IsNullOrEmpty(readinessFitSummary) ? "None" : readinessFitSummary;
            RecoverySummary = string.IsNullOrEmpty(recoverySummary) ? "None" : recoverySummary;
            LastRunSummary = string.IsNullOrEmpty(lastRunSummary) ? "None" : lastRunSummary;
            StateKey = string.IsNullOrEmpty(stateKey) ? "none" : stateKey;
            BlockedReasonText = string.IsNullOrEmpty(blockedReasonText) ? "None" : blockedReasonText;
            IsRecommended = isRecommended;
            IsSelected = isSelected;
            IsStaged = isStaged;
            IsBlocked = isBlocked;
        }
    }

    private readonly Dictionary<string, string> _stagedPartyIdByCityId = new Dictionary<string, string>();
    private readonly Dictionary<string, string> _lastCommittedPartyIdByCityId = new Dictionary<string, string>();
    private readonly Dictionary<string, string> _lastCommittedPartySummaryByCityId = new Dictionary<string, string>();
    private readonly Dictionary<string, string> _partyLastDispatchResidueByPartyId = new Dictionary<string, string>();
    private PrototypeWorldPartyRosterEntryData[] _currentPartyRosterEntries = Array.Empty<PrototypeWorldPartyRosterEntryData>();
    private string _currentPartyRosterCityId = string.Empty;
    private string _currentPartyRosterSummaryText = "None";
    private string _currentSelectedPartyRosterSummaryText = "None";
    private string _currentStagedPartySummaryText = "None";
    private string _currentPartyRosterBlockedReasonSummaryText = "None";
    private string _currentLastCommittedPartySummaryText = "None";
    private string _currentPartyRosterWorldPostureSummaryText = "None";
    private bool _isPartyRosterBoardOpen;
    private int _selectedPartyRosterIndex = -1;

    public bool IsPartyRosterBoardOpen => _isPartyRosterBoardOpen;
    public bool CanOpenSelectedCityPartyRosterBoardAction => _selectedMarker != null && _selectedMarker.EntityData.Kind == WorldEntityKind.City && ResolveCanOpenCityPartyRosterBoard(_selectedMarker.EntityData.Id);
    public bool CanStageSelectedPartyRosterEntry => ResolveSelectedPartyRosterEntry() != null && !string.Equals(ResolveSelectedPartyRosterEntry().StateKey, "active", StringComparison.OrdinalIgnoreCase);
    public string CurrentPartyRosterSummaryText => string.IsNullOrEmpty(_currentPartyRosterSummaryText) ? "None" : _currentPartyRosterSummaryText;
    public string CurrentSelectedPartyRosterSummaryText => string.IsNullOrEmpty(_currentSelectedPartyRosterSummaryText) ? "None" : _currentSelectedPartyRosterSummaryText;
    public string CurrentStagedPartySummaryText => string.IsNullOrEmpty(_currentStagedPartySummaryText) ? "None" : _currentStagedPartySummaryText;
    public string CurrentPartyRosterBlockedReasonSummaryText => string.IsNullOrEmpty(_currentPartyRosterBlockedReasonSummaryText) ? "None" : _currentPartyRosterBlockedReasonSummaryText;
    public string CurrentLastCommittedPartySummaryText => string.IsNullOrEmpty(_currentLastCommittedPartySummaryText) ? "None" : _currentLastCommittedPartySummaryText;
    public string CurrentPartyRosterWorldPostureSummaryText => string.IsNullOrEmpty(_currentPartyRosterWorldPostureSummaryText) ? "None" : _currentPartyRosterWorldPostureSummaryText;
    public int PartyRosterEntryCount => _currentPartyRosterEntries != null ? _currentPartyRosterEntries.Length : 0;

    public string GetPartyRosterEntryDisplayText(int index)
    {
        PrototypeWorldPartyRosterEntryData entry = GetPartyRosterEntry(index);
        if (entry == null)
        {
            return "None";
        }

        string stateBadge = entry.IsStaged
            ? "Staged"
            : entry.IsRecommended
                ? "Recommended"
                : ResolveRosterStateDisplayName(entry.StateKey);
        return string.IsNullOrEmpty(stateBadge) ? entry.DisplayName : entry.DisplayName + " | " + stateBadge;
    }

    public string GetPartyRosterEntrySummaryText(int index)
    {
        PrototypeWorldPartyRosterEntryData entry = GetPartyRosterEntry(index);
        if (entry == null)
        {
            return "None";
        }

        return "Role: " + CompactSurfaceText(entry.RoleMixSummary, 54) + "\n" +
               "Route: " + CompactSurfaceText(entry.RouteFitSummary.Replace("Route Fit: ", string.Empty), 74) + "\n" +
               "Dungeon: " + CompactSurfaceText(entry.DungeonFitSummary.Replace("Dungeon Fit: ", string.Empty), 74) + "\n" +
               "State: " + CompactSurfaceText(entry.ReadinessFitSummary.Replace("Readiness Fit: ", string.Empty), 74);
    }

    public bool IsPartyRosterEntryRecommended(int index)
    {
        PrototypeWorldPartyRosterEntryData entry = GetPartyRosterEntry(index);
        return entry != null && entry.IsRecommended;
    }

    public bool IsPartyRosterEntrySelected(int index)
    {
        PrototypeWorldPartyRosterEntryData entry = GetPartyRosterEntry(index);
        return entry != null && entry.IsSelected;
    }

    public bool IsPartyRosterEntryStaged(int index)
    {
        PrototypeWorldPartyRosterEntryData entry = GetPartyRosterEntry(index);
        return entry != null && entry.IsStaged;
    }

    public bool IsPartyRosterEntryBlocked(int index)
    {
        PrototypeWorldPartyRosterEntryData entry = GetPartyRosterEntry(index);
        return entry == null || entry.IsBlocked;
    }

    public bool TryOpenSelectedCityPartyRosterBoard()
    {
        if (!CanOpenSelectedCityPartyRosterBoardAction)
        {
            return false;
        }

        _isRecruitmentBoardOpen = false;
        _isPartyRosterBoardOpen = true;
        RefreshWorldPartyRosterSurface();
        return _currentPartyRosterEntries.Length > 0;
    }

    public void CancelPartyRosterBoard()
    {
        _isPartyRosterBoardOpen = false;
        RefreshWorldPartyRosterSurface();
    }

    public bool TrySelectPartyRosterEntry(int entryIndex)
    {
        if (_currentPartyRosterEntries == null || entryIndex < 0 || entryIndex >= _currentPartyRosterEntries.Length)
        {
            return false;
        }

        _selectedPartyRosterIndex = entryIndex;
        RefreshWorldPartyRosterSurface();
        return true;
    }

    public bool TryStageSelectedPartyRosterEntry()
    {
        PrototypeWorldPartyRosterEntryData entry = ResolveSelectedPartyRosterEntry();
        if (_runtimeEconomyState == null || entry == null || !_runtimeEconomyState.IsPartyIdle(entry.PartyId) || string.Equals(entry.StateKey, "active", StringComparison.OrdinalIgnoreCase))
        {
            RefreshWorldPartyRosterSurface();
            return false;
        }

        _stagedPartyIdByCityId[entry.CityId] = entry.PartyId;
        _isPartyRosterBoardOpen = false;
        RefreshWorldOperationSurface();
        return true;
    }

    private void ResetWorldPartyRosterRuntimeState()
    {
        _stagedPartyIdByCityId.Clear();
        _lastCommittedPartyIdByCityId.Clear();
        _lastCommittedPartySummaryByCityId.Clear();
        _partyLastDispatchResidueByPartyId.Clear();
        ResetWorldPartyRosterPresentationState();
    }

    private void ResetWorldPartyRosterPresentationState()
    {
        _currentPartyRosterEntries = Array.Empty<PrototypeWorldPartyRosterEntryData>();
        _currentPartyRosterCityId = string.Empty;
        _currentPartyRosterSummaryText = "None";
        _currentSelectedPartyRosterSummaryText = "None";
        _currentStagedPartySummaryText = "None";
        _currentPartyRosterBlockedReasonSummaryText = "None";
        _currentLastCommittedPartySummaryText = "None";
        _currentPartyRosterWorldPostureSummaryText = BuildPartyRosterWorldPostureSummaryText();
        _isPartyRosterBoardOpen = false;
        _selectedPartyRosterIndex = -1;
    }

    private void HandleWorldPartyRosterSelectionChanged()
    {
        if (_selectedMarker == null || _selectedMarker.EntityData == null || _selectedMarker.EntityData.Kind != WorldEntityKind.City)
        {
            _isPartyRosterBoardOpen = false;
            _selectedPartyRosterIndex = -1;
            return;
        }

        if (!string.IsNullOrEmpty(_currentPartyRosterCityId) && !string.Equals(_currentPartyRosterCityId, _selectedMarker.EntityData.Id, StringComparison.Ordinal))
        {
            _isPartyRosterBoardOpen = false;
            _selectedPartyRosterIndex = -1;
        }
    }

    private void HandleWorldPartyRosterVisibilityChanged(bool isVisible)
    {
        if (isVisible)
        {
            RefreshWorldPartyRosterSurface();
            return;
        }

        _isPartyRosterBoardOpen = false;
        _selectedPartyRosterIndex = -1;
    }

    private void RefreshWorldPartyRosterSurface()
    {
        _currentPartyRosterEntries = Array.Empty<PrototypeWorldPartyRosterEntryData>();
        _currentPartyRosterCityId = string.Empty;
        _currentPartyRosterSummaryText = "None";
        _currentSelectedPartyRosterSummaryText = "None";
        _currentStagedPartySummaryText = "None";
        _currentPartyRosterBlockedReasonSummaryText = "None";
        _currentLastCommittedPartySummaryText = "None";
        _currentPartyRosterWorldPostureSummaryText = BuildPartyRosterWorldPostureSummaryText();

        if (_selectedMarker == null || _selectedMarker.EntityData == null || _selectedMarker.EntityData.Kind != WorldEntityKind.City || _runtimeEconomyState == null)
        {
            return;
        }

        BuildPartyRosterSurface(_selectedMarker.EntityData);
        PrototypeWorldPartyRosterEntryData selectedEntry = ResolveSelectedPartyRosterEntry();
        if (selectedEntry != null)
        {
            _currentSelectedPartyRosterSummaryText = BuildSelectedPartyRosterSummaryText(_selectedMarker.EntityData.DisplayName, selectedEntry);
            _currentPartyRosterBlockedReasonSummaryText = selectedEntry.BlockedReasonText;
        }
    }
    private void BuildPartyRosterSurface(WorldEntityData city)
    {
        _currentPartyRosterCityId = city != null ? city.Id : string.Empty;
        if (city == null || _runtimeEconomyState == null)
        {
            return;
        }

        string cityId = city.Id;
        string dungeonId = GetLinkedDungeonIdForCity(cityId);
        WorldEntityData dungeon = FindEntity(dungeonId);
        string[] partyIds = _runtimeEconomyState.GetPartyIdsInCity(cityId);
        string recommendedPartyId = ResolveRecommendedRosterPartyId(cityId, dungeonId, partyIds);
        string stagedPartyId = ResolveStagedPartyIdForCity(cityId);
        bool cityHasActiveExpedition = _runtimeEconomyState.GetActiveExpeditionCountFromCity(cityId) > 0;
        if (_selectedPartyRosterIndex >= partyIds.Length)
        {
            _selectedPartyRosterIndex = -1;
        }

        _currentPartyRosterEntries = new PrototypeWorldPartyRosterEntryData[partyIds.Length];
        for (int i = 0; i < partyIds.Length; i++)
        {
            bool isSelected = (_selectedPartyRosterIndex >= 0 ? _selectedPartyRosterIndex : 0) == i;
            _currentPartyRosterEntries[i] = BuildPartyRosterEntry(city, dungeon, partyIds[i], recommendedPartyId, stagedPartyId, cityHasActiveExpedition, isSelected);
        }

        if (_selectedPartyRosterIndex < 0 && _currentPartyRosterEntries.Length > 0)
        {
            _selectedPartyRosterIndex = 0;
            _currentPartyRosterEntries[0] = BuildPartyRosterEntry(city, dungeon, partyIds[0], recommendedPartyId, stagedPartyId, cityHasActiveExpedition, true);
        }

        int idleCount = _runtimeEconomyState.GetIdlePartyCountInCity(cityId);
        int activeCount = _runtimeEconomyState.GetActiveExpeditionCountFromCity(cityId);
        string routeSummary = dungeon != null ? BuildRecommendedRouteSummaryText(cityId, dungeon.Id) : "No linked dungeon";
        _currentPartyRosterSummaryText = city.DisplayName + " roster | " + partyIds.Length + "/" + _runtimeEconomyState.GetPartyCapacityForCity(cityId) + " companies | Idle " + idleCount + " | Active " + activeCount + " | Best dispatch: " + ResolveWorldPartyDisplayName(recommendedPartyId) + " | Route: " + CompactSurfaceText(routeSummary, 54);
        _currentStagedPartySummaryText = BuildCurrentStagedPartySummaryText(cityId, dungeon != null ? dungeon.DisplayName : "None");
        _currentPartyRosterBlockedReasonSummaryText = BuildPartyRosterBlockedSummaryText(cityId, dungeon != null);
        _currentLastCommittedPartySummaryText = ResolveLastCommittedPartySummaryText(cityId);
    }

    private PrototypeWorldPartyRosterEntryData BuildPartyRosterEntry(WorldEntityData city, WorldEntityData dungeon, string partyId, string recommendedPartyId, string stagedPartyId, bool cityHasActiveExpedition, bool isSelected)
    {
        string cityId = city != null ? city.Id : string.Empty;
        PrototypeWorldRecruitedPartyData recruitedPartyData = ResolveRecruitedPartyData(partyId);
        PrototypeRpgPartyDefinition partyDefinition = recruitedPartyData != null && recruitedPartyData.PartyDefinition != null
            ? recruitedPartyData.PartyDefinition
            : ResolveRecruitedPartyDefinition(partyId) ?? PrototypeRpgPartyCatalog.CreateDefaultPlaceholderParty(partyId);
        string archetypeKey = recruitedPartyData != null ? recruitedPartyData.ArchetypeKey : "balanced_company";
        bool isRecommended = string.Equals(recommendedPartyId, partyId, StringComparison.Ordinal);
        bool isStaged = string.Equals(stagedPartyId, partyId, StringComparison.Ordinal);
        bool isActive = _runtimeEconomyState.IsPartyOnExpedition(partyId);
        bool isRecovering = ResolveIsRosterPartyRecovering(cityId, partyId);
        bool isBlocked = !isActive && !isStaged && cityHasActiveExpedition;
        string stateKey = isActive ? "active" : isStaged ? "staged" : isRecovering ? "recovering" : isBlocked ? "blocked" : isRecommended ? "ready" : "reserve";
        return new PrototypeWorldPartyRosterEntryData(
            partyId,
            cityId,
            ResolveWorldPartyDisplayName(partyId),
            archetypeKey,
            BuildPartyRoleMixSummary(partyDefinition),
            BuildPartyRouteFitSummary(city, dungeon, archetypeKey),
            BuildPartyDungeonFitSummary(dungeon, archetypeKey),
            BuildPartyReadinessFitSummary(cityId, archetypeKey, partyId, isRecommended, isStaged),
            BuildPartyRecoverySummary(cityId, partyId),
            ResolveRosterPartyResidueSummary(cityId, partyId),
            stateKey,
            BuildPartyBlockedReasonText(city, dungeon, partyId, isStaged),
            isRecommended,
            isSelected,
            isStaged,
            isBlocked);
    }

    private string ResolveRecommendedRosterPartyId(string cityId, string dungeonId, string[] partyIds)
    {
        if (_runtimeEconomyState == null || partyIds == null || partyIds.Length <= 0)
        {
            return string.Empty;
        }

        string routeId = !string.IsNullOrEmpty(dungeonId) ? GetRecommendedRouteId(cityId, dungeonId) : string.Empty;
        DispatchReadinessState readinessState = GetDispatchReadinessState(cityId);
        string needPressureText = BuildNeedPressureText(cityId);
        string bestPartyId = string.Empty;
        int bestScore = int.MinValue;
        for (int i = 0; i < partyIds.Length; i++)
        {
            string partyId = partyIds[i];
            if (!_runtimeEconomyState.IsPartyIdle(partyId))
            {
                continue;
            }

            string archetypeKey = ResolveRecruitedPartyData(partyId) != null ? ResolveRecruitedPartyData(partyId).ArchetypeKey : "balanced_company";
            int score = 10;
            switch (NormalizeRecruitArchetypeKey(archetypeKey))
            {
                case "vanguard_company":
                    score += needPressureText.IndexOf("Urgent", StringComparison.OrdinalIgnoreCase) >= 0 ? 3 : 1;
                    score += string.Equals(routeId, SafeRouteId, StringComparison.OrdinalIgnoreCase) ? 2 : 0;
                    break;
                case "skirmish_company":
                    score += string.Equals(routeId, RiskyRouteId, StringComparison.OrdinalIgnoreCase) ? 4 : 0;
                    score += readinessState == DispatchReadinessState.Ready ? 2 : -1;
                    break;
                case "sustain_company":
                    score += readinessState != DispatchReadinessState.Ready ? 4 : 0;
                    score += needPressureText.IndexOf("Stable", StringComparison.OrdinalIgnoreCase) >= 0 ? 2 : 0;
                    break;
                default:
                    score += 2;
                    break;
            }

            string residueText = ResolveRosterPartyResidueSummary(cityId, partyId);
            if (residueText.IndexOf("defeat", StringComparison.OrdinalIgnoreCase) >= 0)
            {
                score -= 2;
            }
            else if (residueText.IndexOf("retreat", StringComparison.OrdinalIgnoreCase) >= 0)
            {
                score -= 1;
            }

            if (ResolveIsRosterPartyRecovering(cityId, partyId))
            {
                score -= 1;
            }

            if (bestPartyId.Length <= 0 || score > bestScore)
            {
                bestPartyId = partyId;
                bestScore = score;
            }
        }

        return bestPartyId;
    }

    private string BuildPartyRouteFitSummary(WorldEntityData city, WorldEntityData dungeon, string archetypeKey)
    {
        if (city == null || dungeon == null)
        {
            return "Route Fit: no linked dungeon lane is available yet.";
        }

        string routeId = GetRecommendedRouteId(city.Id, dungeon.Id);
        string routeSummary = BuildRecommendedRouteSummaryText(city.Id, dungeon.Id);
        switch (NormalizeRecruitArchetypeKey(archetypeKey))
        {
            case "vanguard_company":
                return string.Equals(routeId, SafeRouteId, StringComparison.OrdinalIgnoreCase)
                    ? "Route Fit: strongest on the safer lane where double frontline cover stabilizes the route."
                    : "Route Fit: can force the risky lane, but it prefers calmer routing than the current recommendation.";
            case "skirmish_company":
                return string.Equals(routeId, RiskyRouteId, StringComparison.OrdinalIgnoreCase)
                    ? "Route Fit: best match for " + CompactSurfaceText(routeSummary, 62) + " and a faster reward push."
                    : "Route Fit: the current safe lane leaves some burst value sitting in reserve.";
            case "sustain_company":
                return string.Equals(routeId, RiskyRouteId, StringComparison.OrdinalIgnoreCase)
                    ? "Route Fit: survives the risky lane, but trades tempo for a thicker safety buffer."
                    : "Route Fit: best on safer lanes and longer relief-oriented runs.";
            default:
                return "Route Fit: flexible around " + CompactSurfaceText(routeSummary, 62) + " without locking the lane in.";
        }
    }

    private string BuildPartyDungeonFitSummary(WorldEntityData dungeon, string archetypeKey)
    {
        if (dungeon == null)
        {
            return "Dungeon Fit: linked dungeon identity is not readable yet.";
        }

        bool isBeta = string.Equals(dungeon.Id, "dungeon-beta", StringComparison.OrdinalIgnoreCase);
        switch (NormalizeRecruitArchetypeKey(archetypeKey))
        {
            case "vanguard_company":
                return isBeta ? "Dungeon Fit: steadier into Goblin pressure, but less explosive than greedier companies." : "Dungeon Fit: strong into Alpha's safer slime baseline and slower room tempo.";
            case "skirmish_company":
                return isBeta ? "Dungeon Fit: strongest into Beta's higher-pressure reward lane." : "Dungeon Fit: still viable in Alpha, but more aggressive than the dungeon demands.";
            case "sustain_company":
                return isBeta ? "Dungeon Fit: keeps Beta survivable when the city cannot force a hot entry." : "Dungeon Fit: excellent for Alpha stabilization and recovery loops.";
            default:
                return "Dungeon Fit: balanced package that stays readable in either dungeon identity.";
        }
    }

    private string BuildPartyReadinessFitSummary(string cityId, string archetypeKey, string partyId, bool isRecommended, bool isStaged)
    {
        DispatchReadinessState readinessState = GetDispatchReadinessState(cityId);
        switch (NormalizeRecruitArchetypeKey(archetypeKey))
        {
            case "vanguard_company":
                return readinessState == DispatchReadinessState.Ready ? "Readiness Fit: good immediate dispatch pick while the city can reopen pressure fast." : "Readiness Fit: weaker during recovery because the company wants a ready city.";
            case "skirmish_company":
                return readinessState == DispatchReadinessState.Ready ? "Readiness Fit: best once the city is ready to cash in a riskier route." : "Readiness Fit: low while recovery is still muting the board.";
            case "sustain_company":
                return readinessState != DispatchReadinessState.Ready ? "Readiness Fit: strongest while the city is recovering and wants safer relief." : "Readiness Fit: usable now, but more conservative than the current board asks for.";
            default:
                return isStaged ? "Readiness Fit: staged as the current middle-ground commit choice." : isRecommended ? "Readiness Fit: recommended as the safest all-purpose dispatch pick." : "Readiness Fit: reserve option while the board is still drifting.";
        }
    }

    private string BuildPartyRecoverySummary(string cityId, string partyId)
    {
        if (ResolveIsRosterPartyRecovering(cityId, partyId))
        {
            int recoveryDays = GetRecoveryDaysToReady(cityId);
            return recoveryDays > 0 ? "Recovery: last committed here and the city still needs " + recoveryDays + "d to fully settle." : "Recovery: last committed here and still reading as a softer recovery pick.";
        }

        return "Recovery: no strong fallout is attached to this company right now.";
    }

    private string ResolveRosterPartyResidueSummary(string cityId, string partyId)
    {
        if (!string.IsNullOrEmpty(partyId) && _partyLastDispatchResidueByPartyId.TryGetValue(partyId, out string residueText) && HasMeaningfulSurfaceText(residueText))
        {
            return residueText;
        }

        return _runtimeEconomyState != null ? _runtimeEconomyState.GetPartyLastResultSummary(partyId) : "None";
    }

    private bool ResolveIsRosterPartyRecovering(string cityId, string partyId)
    {
        return !string.IsNullOrEmpty(cityId) && !string.IsNullOrEmpty(partyId) && string.Equals(ResolveLastCommittedPartyIdForCity(cityId), partyId, StringComparison.Ordinal) && GetDispatchReadinessState(cityId) != DispatchReadinessState.Ready;
    }

    private string BuildPartyBlockedReasonText(WorldEntityData city, WorldEntityData dungeon, string partyId, bool isStaged)
    {
        if (_runtimeEconomyState == null)
        {
            return "Roster data is unavailable right now.";
        }

        if (city == null || dungeon == null)
        {
            return "No linked dungeon lane is available, so this company cannot commit yet.";
        }

        if (_runtimeEconomyState.IsPartyOnExpedition(partyId))
        {
            return ResolveWorldPartyDisplayName(partyId) + " is already on expedition toward " + ResolveEntityDisplayName(_runtimeEconomyState.GetPartyTargetDungeonId(partyId), "the linked dungeon") + ".";
        }

        if (_runtimeEconomyState.GetActiveExpeditionCountFromCity(city.Id) > 0 && !isStaged)
        {
            return "Another company already occupies " + city.DisplayName + "'s lane. Stage this one now, then commit after the return.";
        }

        if (ResolveIsRosterPartyRecovering(city.Id, partyId) && !isStaged)
        {
            return ResolveWorldPartyDisplayName(partyId) + " can still be staged now, but the city is in a recovery window after its last commit.";
        }

        return ResolveWorldPartyDisplayName(partyId) + " is ready to stage for " + dungeon.DisplayName + ".";
    }

    private string BuildCurrentStagedPartySummaryText(string cityId, string dungeonLabel)
    {
        string stagedPartyId = ResolveStagedPartyIdForCity(cityId);
        if (string.IsNullOrEmpty(stagedPartyId))
        {
            return "No company staged. Open the roster board to compare reserve companies before entry.";
        }

        bool laneOccupied = _runtimeEconomyState != null && _runtimeEconomyState.GetActiveExpeditionCountFromCity(cityId) > 0;
        bool canCommit = _runtimeEconomyState != null && _runtimeEconomyState.IsPartyIdle(stagedPartyId) && !laneOccupied;
        return ResolveWorldPartyDisplayName(stagedPartyId) + " | " + BuildWorldPartyRoleMixSummary(stagedPartyId) + " | " + (canCommit ? "Commit ready" : "Queued for next opening") + " | Target: " + dungeonLabel;
    }

    private string BuildPartyRosterBlockedSummaryText(string cityId, bool hasLinkedDungeon)
    {
        if (!hasLinkedDungeon)
        {
            return ResolveEntityDisplayName(cityId, "This city") + " has no linked dungeon, so staging cannot resolve into an entry lane.";
        }

        if (_runtimeEconomyState == null || _runtimeEconomyState.GetPartyCountInCity(cityId) <= 0)
        {
            return ResolveEntityDisplayName(cityId, "This city") + " has no recruited companies yet. Recruit first, then compare the roster.";
        }

        if (_runtimeEconomyState.GetActiveExpeditionCountFromCity(cityId) > 0)
        {
            return ResolveEntityDisplayName(cityId, "This city") + " already has an active expedition. You can still stage a reserve company, but commit waits for the return.";
        }

        return string.IsNullOrEmpty(ResolveStagedPartyIdForCity(cityId)) ? "No company is staged yet. Compare the roster and stage one before entering the dungeon." : "Staged company is ready. Press X to enter the linked dungeon.";
    }

    private string BuildSelectedPartyRosterSummaryText(string cityLabel, PrototypeWorldPartyRosterEntryData entry)
    {
        string selectionReason = entry.IsRecommended ? "Why this party: best current fit across route, dungeon risk, and city pressure." : entry.IsStaged ? "Why this party: manually staged ahead of the reserve lane for the next commit." : "Why not this party: reserve alternative that trails the current recommended fit.";
        return cityLabel + " | " + entry.DisplayName + " | " + entry.RoleMixSummary + "\n" + selectionReason + "\n" + CompactSurfaceText(entry.RouteFitSummary, 100) + "\n" + CompactSurfaceText(entry.DungeonFitSummary, 100) + "\n" + CompactSurfaceText(entry.ReadinessFitSummary, 100);
    }

    private string BuildPartyRosterWorldPostureSummaryText()
    {
        if (_worldData == null || _worldData.Entities == null || _runtimeEconomyState == null)
        {
            return "None";
        }

        int readyCount = 0;
        int stagedCityCount = 0;
        int recoveringCount = 0;
        int blockedCityCount = 0;
        for (int i = 0; i < _worldData.Entities.Length; i++)
        {
            WorldEntityData entity = _worldData.Entities[i];
            if (entity == null || entity.Kind != WorldEntityKind.City)
            {
                continue;
            }

            string cityId = entity.Id;
            readyCount += _runtimeEconomyState.GetIdlePartyCountInCity(cityId);
            if (!string.IsNullOrEmpty(ResolveStagedPartyIdForCity(cityId)))
            {
                stagedCityCount += 1;
            }

            string[] partyIds = _runtimeEconomyState.GetPartyIdsInCity(cityId);
            for (int partyIndex = 0; partyIndex < partyIds.Length; partyIndex++)
            {
                if (ResolveIsRosterPartyRecovering(cityId, partyIds[partyIndex]))
                {
                    recoveringCount += 1;
                }
            }

            if (_runtimeEconomyState.GetPartyCountInCity(cityId) > 0 && _runtimeEconomyState.GetIdlePartyCountInCity(cityId) <= 0)
            {
                blockedCityCount += 1;
            }
        }

        return "Ready parties " + readyCount + " | Staged cities " + stagedCityCount + " | Recovering parties " + recoveringCount + " | No-ready cities " + blockedCityCount;
    }

    private bool ResolveCanOpenCityPartyRosterBoard(string cityId)
    {
        return _runtimeEconomyState != null && !string.IsNullOrEmpty(cityId) && _runtimeEconomyState.GetPartyCountInCity(cityId) > 0;
    }

    private string ResolveStagedPartyIdForCity(string cityId)
    {
        if (string.IsNullOrEmpty(cityId) || !_stagedPartyIdByCityId.TryGetValue(cityId, out string partyId) || _runtimeEconomyState == null)
        {
            return string.Empty;
        }

        return string.Equals(_runtimeEconomyState.GetPartyHomeCityId(partyId), cityId, StringComparison.Ordinal) ? partyId : string.Empty;
    }

    private string ResolveLastCommittedPartyIdForCity(string cityId)
    {
        return !string.IsNullOrEmpty(cityId) && _lastCommittedPartyIdByCityId.TryGetValue(cityId, out string partyId) ? partyId : string.Empty;
    }

    private string ResolveLastCommittedPartySummaryText(string cityId)
    {
        return !string.IsNullOrEmpty(cityId) && _lastCommittedPartySummaryByCityId.TryGetValue(cityId, out string summaryText) ? summaryText : "None";
    }

    private PrototypeWorldPartyRosterEntryData ResolveSelectedPartyRosterEntry()
    {
        if (_currentPartyRosterEntries == null || _currentPartyRosterEntries.Length <= 0)
        {
            return null;
        }

        int index = _selectedPartyRosterIndex >= 0 && _selectedPartyRosterIndex < _currentPartyRosterEntries.Length ? _selectedPartyRosterIndex : 0;
        return _currentPartyRosterEntries[index];
    }

    private PrototypeWorldPartyRosterEntryData GetPartyRosterEntry(int index)
    {
        return _currentPartyRosterEntries != null && index >= 0 && index < _currentPartyRosterEntries.Length ? _currentPartyRosterEntries[index] : null;
    }

    private string ResolveWorldPartyDisplayName(string partyId)
    {
        if (string.IsNullOrEmpty(partyId))
        {
            return "None";
        }

        PrototypeWorldRecruitedPartyData recruitedPartyData = ResolveRecruitedPartyData(partyId);
        if (recruitedPartyData != null && HasMeaningfulSurfaceText(recruitedPartyData.DisplayName))
        {
            return recruitedPartyData.DisplayName;
        }

        PrototypeRpgPartyDefinition partyDefinition = ResolveRecruitedPartyDefinition(partyId);
        if (partyDefinition != null && HasMeaningfulSurfaceText(partyDefinition.DisplayName))
        {
            return partyDefinition.DisplayName;
        }

        return BuildPartyLabelText(partyId);
    }

    private string BuildWorldPartyRoleMixSummary(string partyId)
    {
        PrototypeWorldRecruitedPartyData recruitedPartyData = ResolveRecruitedPartyData(partyId);
        PrototypeRpgPartyDefinition partyDefinition = recruitedPartyData != null && recruitedPartyData.PartyDefinition != null ? recruitedPartyData.PartyDefinition : ResolveRecruitedPartyDefinition(partyId);
        return BuildPartyRoleMixSummary(partyDefinition);
    }

    private string BuildPartyRoleMixSummary(PrototypeRpgPartyDefinition partyDefinition)
    {
        if (partyDefinition == null || partyDefinition.Members == null || partyDefinition.Members.Length <= 0)
        {
            return "Unknown mix";
        }

        List<string> parts = new List<string>(partyDefinition.Members.Length);
        for (int i = 0; i < partyDefinition.Members.Length; i++)
        {
            PrototypeRpgPartyMemberDefinition member = partyDefinition.Members[i];
            if (member == null)
            {
                continue;
            }

            parts.Add(!string.IsNullOrWhiteSpace(member.RoleLabel) ? member.RoleLabel.Trim() : (!string.IsNullOrWhiteSpace(member.RoleTag) ? char.ToUpperInvariant(member.RoleTag[0]) + member.RoleTag.Substring(1) : "Adventurer"));
        }

        return parts.Count > 0 ? string.Join(" / ", parts.ToArray()) : "Unknown mix";
    }

    private string ResolveRosterStateDisplayName(string stateKey)
    {
        switch (stateKey)
        {
            case "ready": return "Ready";
            case "reserve": return "Reserve";
            case "staged": return "Staged";
            case "recovering": return "Recovering";
            case "blocked": return "Blocked";
            case "active": return "Active";
            default: return string.Empty;
        }
    }

    private PrototypeWorldRecruitedPartyData ResolveRecruitedPartyData(string partyId)
    {
        return !string.IsNullOrEmpty(partyId) && _recruitedPartyDataByPartyId.TryGetValue(partyId, out PrototypeWorldRecruitedPartyData recruitedPartyData) ? recruitedPartyData : null;
    }

    private void HandleSuccessfulRecruitmentIntoRoster(string cityId, string partyId)
    {
        if (string.IsNullOrEmpty(cityId) || string.IsNullOrEmpty(partyId))
        {
            return;
        }

        if (string.IsNullOrEmpty(ResolveStagedPartyIdForCity(cityId)))
        {
            _stagedPartyIdByCityId[cityId] = partyId;
        }

        _selectedPartyRosterIndex = -1;
    }

    private void CapturePartyDispatchCommit(string cityId, string partyId, string dungeonLabel, string routeSummaryText)
    {
        if (string.IsNullOrEmpty(cityId) || string.IsNullOrEmpty(partyId))
        {
            return;
        }

        _stagedPartyIdByCityId[cityId] = partyId;
        _lastCommittedPartyIdByCityId[cityId] = partyId;
        _lastCommittedPartySummaryByCityId[cityId] = ResolveEntityDisplayName(cityId, "City") + " committed " + ResolveWorldPartyDisplayName(partyId) + " | " + CompactSurfaceText(routeSummaryText, 46) + " | Target: " + CompactSurfaceText(dungeonLabel, 24);
    }

    private void CapturePartyDispatchResidue(string cityId, string partyId, string resultSummaryText, string survivingSummaryText, string lootSummaryText, string routeSummaryText)
    {
        if (string.IsNullOrEmpty(cityId) || string.IsNullOrEmpty(partyId))
        {
            return;
        }

        _partyLastDispatchResidueByPartyId[partyId] = ResolveWorldPartyDisplayName(partyId) + " | " + CompactSurfaceText(resultSummaryText, 40) + " | Survivors: " + CompactSurfaceText(survivingSummaryText, 24) + " | Loot: " + CompactSurfaceText(lootSummaryText, 24);
        _lastCommittedPartySummaryByCityId[cityId] = ResolveEntityDisplayName(cityId, "City") + " last committed " + ResolveWorldPartyDisplayName(partyId) + " | " + CompactSurfaceText(routeSummaryText, 40) + " | " + CompactSurfaceText(resultSummaryText, 40);
    }
}

