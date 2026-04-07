using System;
using System.Collections.Generic;

public sealed partial class StaticPlaceholderWorldView
{
    private sealed class PrototypeWorldRecruitOfferData
    {
        public static readonly PrototypeWorldRecruitOfferData Empty = new PrototypeWorldRecruitOfferData(
            string.Empty,
            string.Empty,
            string.Empty,
            "None",
            "None",
            "None",
            "None",
            "None",
            "None",
            "none",
            "None",
            false,
            false,
            true);

        public readonly string OfferId;
        public readonly string CityId;
        public readonly string ArchetypeKey;
        public readonly string DisplayName;
        public readonly string RoleMixSummary;
        public readonly string RoleFitSummary;
        public readonly string RouteFitSummary;
        public readonly string PressureFitSummary;
        public readonly string FrictionSummary;
        public readonly string AvailabilityStateKey;
        public readonly string BlockedReasonText;
        public readonly bool IsRecommended;
        public readonly bool IsSelected;
        public readonly bool IsBlocked;

        public string SummaryText => RoleFitSummary + "\n" + RouteFitSummary + "\n" + PressureFitSummary + "\n" + FrictionSummary;

        public PrototypeWorldRecruitOfferData(
            string offerId,
            string cityId,
            string archetypeKey,
            string displayName,
            string roleMixSummary,
            string roleFitSummary,
            string routeFitSummary,
            string pressureFitSummary,
            string frictionSummary,
            string availabilityStateKey,
            string blockedReasonText,
            bool isRecommended,
            bool isSelected,
            bool isBlocked)
        {
            OfferId = string.IsNullOrEmpty(offerId) ? string.Empty : offerId;
            CityId = string.IsNullOrEmpty(cityId) ? string.Empty : cityId;
            ArchetypeKey = string.IsNullOrEmpty(archetypeKey) ? string.Empty : archetypeKey;
            DisplayName = string.IsNullOrEmpty(displayName) ? "None" : displayName;
            RoleMixSummary = string.IsNullOrEmpty(roleMixSummary) ? "None" : roleMixSummary;
            RoleFitSummary = string.IsNullOrEmpty(roleFitSummary) ? "None" : roleFitSummary;
            RouteFitSummary = string.IsNullOrEmpty(routeFitSummary) ? "None" : routeFitSummary;
            PressureFitSummary = string.IsNullOrEmpty(pressureFitSummary) ? "None" : pressureFitSummary;
            FrictionSummary = string.IsNullOrEmpty(frictionSummary) ? "None" : frictionSummary;
            AvailabilityStateKey = string.IsNullOrEmpty(availabilityStateKey) ? "none" : availabilityStateKey;
            BlockedReasonText = string.IsNullOrEmpty(blockedReasonText) ? "None" : blockedReasonText;
            IsRecommended = isRecommended;
            IsSelected = isSelected;
            IsBlocked = isBlocked;
        }
    }

    private sealed class PrototypeWorldRecruitmentBoardSurfaceData
    {
        public static readonly PrototypeWorldRecruitmentBoardSurfaceData Empty = new PrototypeWorldRecruitmentBoardSurfaceData(
            string.Empty,
            "None",
            string.Empty,
            "None",
            Array.Empty<PrototypeWorldRecruitOfferData>(),
            -1,
            "None",
            "None",
            "None",
            "None");

        public readonly string CityId;
        public readonly string CityLabel;
        public readonly string LinkedDungeonId;
        public readonly string LinkedDungeonLabel;
        public readonly PrototypeWorldRecruitOfferData[] Offers;
        public readonly int SelectedOfferIndex;
        public readonly string BoardSummaryText;
        public readonly string BlockedReasonText;
        public readonly string LastRecruitedSummaryText;
        public readonly string RefreshSummaryText;

        public PrototypeWorldRecruitmentBoardSurfaceData(
            string cityId,
            string cityLabel,
            string linkedDungeonId,
            string linkedDungeonLabel,
            PrototypeWorldRecruitOfferData[] offers,
            int selectedOfferIndex,
            string boardSummaryText,
            string blockedReasonText,
            string lastRecruitedSummaryText,
            string refreshSummaryText)
        {
            CityId = string.IsNullOrEmpty(cityId) ? string.Empty : cityId;
            CityLabel = string.IsNullOrEmpty(cityLabel) ? "None" : cityLabel;
            LinkedDungeonId = string.IsNullOrEmpty(linkedDungeonId) ? string.Empty : linkedDungeonId;
            LinkedDungeonLabel = string.IsNullOrEmpty(linkedDungeonLabel) ? "None" : linkedDungeonLabel;
            Offers = offers ?? Array.Empty<PrototypeWorldRecruitOfferData>();
            SelectedOfferIndex = selectedOfferIndex;
            BoardSummaryText = string.IsNullOrEmpty(boardSummaryText) ? "None" : boardSummaryText;
            BlockedReasonText = string.IsNullOrEmpty(blockedReasonText) ? "None" : blockedReasonText;
            LastRecruitedSummaryText = string.IsNullOrEmpty(lastRecruitedSummaryText) ? "None" : lastRecruitedSummaryText;
            RefreshSummaryText = string.IsNullOrEmpty(refreshSummaryText) ? "None" : refreshSummaryText;
        }
    }

    private sealed class PrototypeWorldRecruitedPartyData
    {
        public readonly string PartyId;
        public readonly string CityId;
        public readonly string ArchetypeKey;
        public readonly string DisplayName;
        public readonly string SummaryText;
        public readonly PrototypeRpgPartyDefinition PartyDefinition;

        public PrototypeWorldRecruitedPartyData(string partyId, string cityId, string archetypeKey, string displayName, string summaryText, PrototypeRpgPartyDefinition partyDefinition)
        {
            PartyId = string.IsNullOrEmpty(partyId) ? string.Empty : partyId;
            CityId = string.IsNullOrEmpty(cityId) ? string.Empty : cityId;
            ArchetypeKey = string.IsNullOrEmpty(archetypeKey) ? "balanced_company" : archetypeKey;
            DisplayName = string.IsNullOrEmpty(displayName) ? "None" : displayName;
            SummaryText = string.IsNullOrEmpty(summaryText) ? "None" : summaryText;
            PartyDefinition = partyDefinition;
        }
    }

    private static readonly string[] RecruitOfferArchetypeKeys =
    {
        "balanced_company",
        "vanguard_company",
        "skirmish_company",
        "sustain_company"
    };

    private readonly Dictionary<string, PrototypeWorldRecruitedPartyData> _recruitedPartyDataByPartyId = new Dictionary<string, PrototypeWorldRecruitedPartyData>();
    private readonly Dictionary<string, string> _lastRecruitedPartySummaryByCityId = new Dictionary<string, string>();
    private PrototypeWorldRecruitmentBoardSurfaceData _currentRecruitmentBoardSurface = PrototypeWorldRecruitmentBoardSurfaceData.Empty;
    private string _currentRecruitmentBoardSummaryText = "None";
    private string _currentSelectedRecruitOfferSummaryText = "None";
    private string _currentRecruitBlockedReasonSummaryText = "None";
    private string _currentLastRecruitedPartySummaryText = "None";
    private string _currentNextRecruitRefreshSummaryText = "None";
    private string _currentRecruitmentWorldPostureSummaryText = "None";
    private bool _isRecruitmentBoardOpen;
    private int _selectedRecruitOfferIndex = -1;

    public bool IsRecruitmentBoardOpen => _isRecruitmentBoardOpen;
    public bool CanOpenSelectedCityRecruitmentBoardAction => _selectedMarker != null && _selectedMarker.EntityData.Kind == WorldEntityKind.City;
    public bool CanConfirmSelectedRecruitOffer => ResolveSelectedRecruitOffer() != PrototypeWorldRecruitOfferData.Empty && !ResolveSelectedRecruitOffer().IsBlocked;
    public string CurrentRecruitmentBoardSummaryText => string.IsNullOrEmpty(_currentRecruitmentBoardSummaryText) ? "None" : _currentRecruitmentBoardSummaryText;
    public string CurrentSelectedRecruitOfferSummaryText => string.IsNullOrEmpty(_currentSelectedRecruitOfferSummaryText) ? "None" : _currentSelectedRecruitOfferSummaryText;
    public string CurrentRecruitBlockedReasonSummaryText => string.IsNullOrEmpty(_currentRecruitBlockedReasonSummaryText) ? "None" : _currentRecruitBlockedReasonSummaryText;
    public string CurrentLastRecruitedPartySummaryText => string.IsNullOrEmpty(_currentLastRecruitedPartySummaryText) ? "None" : _currentLastRecruitedPartySummaryText;
    public string CurrentNextRecruitRefreshSummaryText => string.IsNullOrEmpty(_currentNextRecruitRefreshSummaryText) ? "None" : _currentNextRecruitRefreshSummaryText;
    public string CurrentRecruitmentWorldPostureSummaryText => string.IsNullOrEmpty(_currentRecruitmentWorldPostureSummaryText) ? "None" : _currentRecruitmentWorldPostureSummaryText;
    public int RecruitOfferCount => _currentRecruitmentBoardSurface.Offers != null ? _currentRecruitmentBoardSurface.Offers.Length : 0;

    public string GetRecruitOfferDisplayText(int index)
    {
        PrototypeWorldRecruitOfferData offer = GetRecruitOffer(index);
        if (offer == PrototypeWorldRecruitOfferData.Empty)
        {
            return "None";
        }

        return offer.DisplayName + (offer.IsRecommended ? " | Recommended" : string.Empty);
    }

    public string GetRecruitOfferSummaryText(int index)
    {
        PrototypeWorldRecruitOfferData offer = GetRecruitOffer(index);
        if (offer == PrototypeWorldRecruitOfferData.Empty)
        {
            return "None";
        }

        string roleFit = CompactSurfaceText(offer.RoleFitSummary.Replace("Role Fit: ", string.Empty), 42);
        string routeFit = CompactSurfaceText(offer.RouteFitSummary.Replace("Route Fit: ", string.Empty), 62);
        string pressureFit = CompactSurfaceText(offer.PressureFitSummary.Replace("Pressure Fit: ", string.Empty), 62);
        string frictionFit = CompactSurfaceText(offer.FrictionSummary.Replace("Hire Friction: ", string.Empty), 62);
        return "Role: " + offer.RoleMixSummary + " | " + roleFit + "\n" +
               "Route: " + routeFit + "\n" +
               "Pressure: " + pressureFit + "\n" +
               "Friction: " + frictionFit;
    }

    public bool IsRecruitOfferRecommended(int index)
    {
        PrototypeWorldRecruitOfferData offer = GetRecruitOffer(index);
        return offer != PrototypeWorldRecruitOfferData.Empty && offer.IsRecommended;
    }

    public bool IsRecruitOfferSelected(int index)
    {
        PrototypeWorldRecruitOfferData offer = GetRecruitOffer(index);
        return offer != PrototypeWorldRecruitOfferData.Empty && offer.IsSelected;
    }

    public bool IsRecruitOfferBlocked(int index)
    {
        PrototypeWorldRecruitOfferData offer = GetRecruitOffer(index);
        return offer == PrototypeWorldRecruitOfferData.Empty || offer.IsBlocked;
    }

    public bool TryOpenSelectedCityRecruitmentBoard()
    {
        if (!CanOpenSelectedCityRecruitmentBoardAction)
        {
            return false;
        }

        _isPartyRosterBoardOpen = false;
        _isRecruitmentBoardOpen = true;
        RefreshWorldRecruitmentSurface();
        return _currentRecruitmentBoardSurface != PrototypeWorldRecruitmentBoardSurfaceData.Empty;
    }

    public void CancelRecruitmentBoard()
    {
        _isRecruitmentBoardOpen = false;
        RefreshWorldRecruitmentSurface();
    }

    public bool TrySelectRecruitOffer(int offerIndex)
    {
        if (_currentRecruitmentBoardSurface == PrototypeWorldRecruitmentBoardSurfaceData.Empty ||
            _currentRecruitmentBoardSurface.Offers == null ||
            offerIndex < 0 ||
            offerIndex >= _currentRecruitmentBoardSurface.Offers.Length)
        {
            return false;
        }

        _selectedRecruitOfferIndex = offerIndex;
        RefreshWorldRecruitmentSurface();
        return true;
    }
    public bool TryConfirmSelectedRecruitOffer()
    {
        if (_runtimeEconomyState == null || _selectedMarker == null || _selectedMarker.EntityData.Kind != WorldEntityKind.City)
        {
            return false;
        }

        PrototypeWorldRecruitOfferData offer = ResolveSelectedRecruitOffer();
        if (offer == PrototypeWorldRecruitOfferData.Empty || offer.IsBlocked)
        {
            RefreshWorldRecruitmentSurface();
            return false;
        }

        string cityId = _selectedMarker.EntityData.Id;
        string cityLabel = _selectedMarker.EntityData.DisplayName;
        CaptureWorldOperationBaseline("recruit");
        if (!_runtimeEconomyState.RecruitParty(cityId))
        {
            RefreshWorldRecruitmentSurface();
            return false;
        }

        string[] partyIds = _runtimeEconomyState.GetPartyIdsInCity(cityId);
        string partyId = partyIds != null && partyIds.Length > 0 ? partyIds[partyIds.Length - 1] : string.Empty;
        if (string.IsNullOrEmpty(partyId))
        {
            RefreshWorldRecruitmentSurface();
            return false;
        }

        PrototypeRpgPartyDefinition partyDefinition = PrototypeRpgPartyCatalog.CreatePlaceholderPartyByArchetype(partyId, offer.ArchetypeKey, cityLabel);
        string lastRecruitSummaryText = BuildLastRecruitedPartySummaryText(cityId, cityLabel, offer);
        _recruitedPartyDataByPartyId[partyId] = new PrototypeWorldRecruitedPartyData(partyId, cityId, offer.ArchetypeKey, offer.DisplayName, lastRecruitSummaryText, partyDefinition);
        _lastRecruitedPartySummaryByCityId[cityId] = lastRecruitSummaryText;
        GetOrCreateDungeonParty(cityId, partyId);
        HandleSuccessfulRecruitmentIntoRoster(cityId, partyId);
        _isRecruitmentBoardOpen = false;
        RefreshWorldOperationSurface();
        return true;
    }

    private void ResetWorldRecruitmentRuntimeState()
    {
        _recruitedPartyDataByPartyId.Clear();
        _lastRecruitedPartySummaryByCityId.Clear();
        ResetWorldRecruitmentPresentationState();
    }

    private void ResetWorldRecruitmentPresentationState()
    {
        _currentRecruitmentBoardSurface = PrototypeWorldRecruitmentBoardSurfaceData.Empty;
        _currentRecruitmentBoardSummaryText = "None";
        _currentSelectedRecruitOfferSummaryText = "None";
        _currentRecruitBlockedReasonSummaryText = "None";
        _currentLastRecruitedPartySummaryText = "None";
        _currentNextRecruitRefreshSummaryText = "None";
        _currentRecruitmentWorldPostureSummaryText = BuildRecruitmentWorldPostureSummaryText();
        _isRecruitmentBoardOpen = false;
        _selectedRecruitOfferIndex = -1;
    }

    private void HandleWorldRecruitmentSelectionChanged()
    {
        if (_selectedMarker == null || _selectedMarker.EntityData == null || _selectedMarker.EntityData.Kind != WorldEntityKind.City)
        {
            _isRecruitmentBoardOpen = false;
            _selectedRecruitOfferIndex = -1;
            return;
        }

        if (_currentRecruitmentBoardSurface != PrototypeWorldRecruitmentBoardSurfaceData.Empty &&
            !string.IsNullOrEmpty(_currentRecruitmentBoardSurface.CityId) &&
            !string.Equals(_currentRecruitmentBoardSurface.CityId, _selectedMarker.EntityData.Id, StringComparison.Ordinal))
        {
            _isRecruitmentBoardOpen = false;
            _selectedRecruitOfferIndex = -1;
        }
    }

    private void HandleWorldRecruitmentVisibilityChanged(bool isVisible)
    {
        if (isVisible)
        {
            RefreshWorldRecruitmentSurface();
            return;
        }

        _isRecruitmentBoardOpen = false;
        _selectedRecruitOfferIndex = -1;
    }

    private void RefreshWorldRecruitmentSurface()
    {
        _currentRecruitmentBoardSurface = PrototypeWorldRecruitmentBoardSurfaceData.Empty;
        _currentRecruitmentBoardSummaryText = "None";
        _currentSelectedRecruitOfferSummaryText = "None";
        _currentRecruitBlockedReasonSummaryText = "None";
        _currentLastRecruitedPartySummaryText = "None";
        _currentNextRecruitRefreshSummaryText = "None";
        _currentRecruitmentWorldPostureSummaryText = BuildRecruitmentWorldPostureSummaryText();

        if (_selectedMarker == null || _selectedMarker.EntityData == null || _selectedMarker.EntityData.Kind != WorldEntityKind.City)
        {
            return;
        }

        WorldEntityData city = _selectedMarker.EntityData;
        _currentRecruitmentBoardSurface = BuildRecruitmentBoardSurface(city);
        _currentRecruitmentBoardSummaryText = _currentRecruitmentBoardSurface.BoardSummaryText;
        _currentRecruitBlockedReasonSummaryText = _currentRecruitmentBoardSurface.BlockedReasonText;
        _currentLastRecruitedPartySummaryText = _currentRecruitmentBoardSurface.LastRecruitedSummaryText;
        _currentNextRecruitRefreshSummaryText = _currentRecruitmentBoardSurface.RefreshSummaryText;

        PrototypeWorldRecruitOfferData selectedOffer = ResolveSelectedRecruitOffer();
        if (selectedOffer != PrototypeWorldRecruitOfferData.Empty)
        {
            _currentSelectedRecruitOfferSummaryText = BuildSelectedRecruitOfferSummaryText(city.DisplayName, selectedOffer);
            if (selectedOffer.IsBlocked && HasMeaningfulSurfaceText(selectedOffer.BlockedReasonText))
            {
                _currentRecruitBlockedReasonSummaryText = selectedOffer.BlockedReasonText;
            }
        }
    }

    private PrototypeWorldRecruitmentBoardSurfaceData BuildRecruitmentBoardSurface(WorldEntityData city)
    {
        if (city == null || city.Kind != WorldEntityKind.City)
        {
            return PrototypeWorldRecruitmentBoardSurfaceData.Empty;
        }

        string cityId = city.Id;
        string dungeonId = GetLinkedDungeonIdForCity(cityId);
        WorldEntityData dungeon = FindEntity(dungeonId);
        bool hasLinkedDungeon = dungeon != null;
        string linkedDungeonLabel = hasLinkedDungeon ? dungeon.DisplayName : "None";
        string recommendedArchetypeKey = ResolveRecruitOfferRecommendation(cityId, dungeonId);
        string blockedReasonText = BuildRecruitmentBlockedReasonText(cityId, hasLinkedDungeon);
        string lastRecruitedSummaryText = ResolveLastRecruitedPartySummaryText(cityId);
        string refreshSummaryText = BuildRecruitmentRefreshSummaryText(cityId);
        int resolvedSelectedIndex = ResolveRecruitOfferSelectionIndex(recommendedArchetypeKey);

        PrototypeWorldRecruitOfferData[] offers = new PrototypeWorldRecruitOfferData[RecruitOfferArchetypeKeys.Length];
        for (int i = 0; i < RecruitOfferArchetypeKeys.Length; i++)
        {
            string archetypeKey = RecruitOfferArchetypeKeys[i];
            bool isRecommended = string.Equals(archetypeKey, recommendedArchetypeKey, StringComparison.OrdinalIgnoreCase);
            bool isSelected = i == resolvedSelectedIndex;
            offers[i] = BuildRecruitOfferData(city, dungeon, archetypeKey, isRecommended, isSelected, blockedReasonText);
        }

        string boardSummaryText = BuildRecruitmentBoardSummaryText(city, linkedDungeonLabel, recommendedArchetypeKey, blockedReasonText);
        return new PrototypeWorldRecruitmentBoardSurfaceData(
            cityId,
            city.DisplayName,
            dungeonId,
            linkedDungeonLabel,
            offers,
            resolvedSelectedIndex,
            boardSummaryText,
            blockedReasonText,
            lastRecruitedSummaryText,
            refreshSummaryText);
    }

    private PrototypeWorldRecruitOfferData BuildRecruitOfferData(WorldEntityData city, WorldEntityData dungeon, string archetypeKey, bool isRecommended, bool isSelected, string blockedReasonText)
    {
        string cityId = city != null ? city.Id : string.Empty;
        string offerId = (cityId ?? string.Empty) + ":" + archetypeKey;
        bool isBlocked = !ResolveCanRecruitSelectedCityParty(cityId) || dungeon == null;
        string displayName = PrototypeRpgPartyCatalog.GetPlaceholderPartyArchetypeDisplayName(archetypeKey);
        string roleMixSummary = BuildRecruitRoleMixSummary(archetypeKey);
        string roleFitSummary = BuildRecruitRoleFitSummary(archetypeKey);
        string routeFitSummary = BuildRecruitRouteFitSummary(city, dungeon, archetypeKey);
        string pressureFitSummary = BuildRecruitPressureFitSummary(cityId, archetypeKey);
        string frictionSummary = BuildRecruitHireFrictionSummary(cityId, archetypeKey, isBlocked);
        string availabilityStateKey = isBlocked ? ResolveRecruitBlockedStateKey(cityId, dungeon != null) : "ready";

        return new PrototypeWorldRecruitOfferData(
            offerId,
            cityId,
            archetypeKey,
            displayName,
            roleMixSummary,
            roleFitSummary,
            routeFitSummary,
            pressureFitSummary,
            frictionSummary,
            availabilityStateKey,
            isBlocked ? blockedReasonText : "Slot open. Confirm to create a 4-member company in this city.",
            isRecommended,
            isSelected,
            isBlocked);
    }

    private int ResolveRecruitOfferSelectionIndex(string recommendedArchetypeKey)
    {
        if (_selectedRecruitOfferIndex >= 0 && _selectedRecruitOfferIndex < RecruitOfferArchetypeKeys.Length)
        {
            return _selectedRecruitOfferIndex;
        }

        for (int i = 0; i < RecruitOfferArchetypeKeys.Length; i++)
        {
            if (string.Equals(RecruitOfferArchetypeKeys[i], recommendedArchetypeKey, StringComparison.OrdinalIgnoreCase))
            {
                _selectedRecruitOfferIndex = i;
                return i;
            }
        }

        _selectedRecruitOfferIndex = 0;
        return 0;
    }

    private string ResolveRecruitOfferRecommendation(string cityId, string dungeonId)
    {
        DispatchReadinessState readinessState = GetDispatchReadinessState(cityId);
        string needPressureText = BuildNeedPressureText(cityId);
        string routeId = !string.IsNullOrEmpty(dungeonId) ? GetRecommendedRouteId(cityId, dungeonId) : string.Empty;

        if (readinessState == DispatchReadinessState.Strained || readinessState == DispatchReadinessState.Recovering)
        {
            return "sustain_company";
        }

        if (string.Equals(routeId, RiskyRouteId, StringComparison.OrdinalIgnoreCase) && readinessState == DispatchReadinessState.Ready)
        {
            return "skirmish_company";
        }

        if (needPressureText.IndexOf("Urgent", StringComparison.OrdinalIgnoreCase) >= 0)
        {
            return "vanguard_company";
        }

        return "balanced_company";
    }
    private string BuildRecruitRoleMixSummary(string archetypeKey)
    {
        switch (NormalizeRecruitArchetypeKey(archetypeKey))
        {
            case "vanguard_company":
                return "2 Warriors / Mage / Cleric";
            case "skirmish_company":
                return "Warrior / 2 Rogues / Mage";
            case "sustain_company":
                return "Warrior / Mage / 2 Clerics";
            default:
                return "Warrior / Rogue / Mage / Cleric";
        }
    }

    private string BuildRecruitRoleFitSummary(string archetypeKey)
    {
        switch (NormalizeRecruitArchetypeKey(archetypeKey))
        {
            case "vanguard_company":
                return "Role Fit: heavier frontline cover for early room stability and safer opening tempo.";
            case "skirmish_company":
                return "Role Fit: burst-heavy package that leans into pickoffs, tempo, and risky lane pressure.";
            case "sustain_company":
                return "Role Fit: recovery-first package with extra sustain when the city needs a steadier run.";
            default:
                return "Role Fit: balanced 4-role cover that can pivot if the board changes after hiring.";
        }
    }

    private string BuildRecruitRouteFitSummary(WorldEntityData city, WorldEntityData dungeon, string archetypeKey)
    {
        if (city == null || dungeon == null)
        {
            return "Route Fit: no linked dungeon is ready, so route fit cannot resolve yet.";
        }

        string recommendedRouteId = GetRecommendedRouteId(city.Id, dungeon.Id);
        string recommendedRouteSummary = BuildRecommendedRouteSummaryText(city.Id, dungeon.Id);
        bool riskyRoute = string.Equals(recommendedRouteId, RiskyRouteId, StringComparison.OrdinalIgnoreCase);
        switch (NormalizeRecruitArchetypeKey(archetypeKey))
        {
            case "vanguard_company":
                return riskyRoute
                    ? "Route Fit: can force the risky lane, but it is more efficient when the board swings back toward safer routing."
                    : "Route Fit: strong match for " + CompactSurfaceText(recommendedRouteSummary, 72) + " and slower, safer dungeon pacing.";
            case "skirmish_company":
                return riskyRoute
                    ? "Route Fit: strongest match for " + CompactSurfaceText(recommendedRouteSummary, 72) + " and higher-reward pushes."
                    : "Route Fit: the current route leans safer than this company wants, so some burst value will sit idle.";
            case "sustain_company":
                return riskyRoute
                    ? "Route Fit: can survive the current risky lane, but it trades away tempo for recovery margin."
                    : "Route Fit: best for " + CompactSurfaceText(recommendedRouteSummary, 72) + " and long relief-oriented runs.";
            default:
                return "Route Fit: stays flexible around " + CompactSurfaceText(recommendedRouteSummary, 72) + " without overcommitting to one lane.";
        }
    }

    private string BuildRecruitPressureFitSummary(string cityId, string archetypeKey)
    {
        DispatchReadinessState readinessState = GetDispatchReadinessState(cityId);
        string needPressureText = BuildNeedPressureText(cityId);
        switch (NormalizeRecruitArchetypeKey(archetypeKey))
        {
            case "vanguard_company":
                return readinessState == DispatchReadinessState.Ready && needPressureText.IndexOf("Urgent", StringComparison.OrdinalIgnoreCase) >= 0
                    ? "Pressure Fit: good when the city is ready and must reopen the lane right away."
                    : "Pressure Fit: weaker during recovery windows because its value depends on immediate tempo.";
            case "skirmish_company":
                return readinessState == DispatchReadinessState.Ready
                    ? "Pressure Fit: best once the city is ready to convert pressure into a faster reward push."
                    : "Pressure Fit: weak while readiness is still recovering because burst arrives before the city can exploit it.";
            case "sustain_company":
                return readinessState != DispatchReadinessState.Ready || needPressureText.IndexOf("Stable", StringComparison.OrdinalIgnoreCase) >= 0
                    ? "Pressure Fit: best for recovery windows, safer relief, and cities that cannot force a greedy route yet."
                    : "Pressure Fit: stabilizes pressure well, but it gives up some immediate burst when the city is ready to push.";
            default:
                return needPressureText.IndexOf("Urgent", StringComparison.OrdinalIgnoreCase) >= 0
                    ? "Pressure Fit: reliable middle ground when pressure is active but the board may still drift."
                    : "Pressure Fit: keeps the city flexible while pressure and readiness are still readable rather than extreme.";
        }
    }

    private string BuildRecruitHireFrictionSummary(string cityId, string archetypeKey, bool isBlocked)
    {
        string cityLabel = ResolveEntityDisplayName(cityId, "City");
        int capacity = _runtimeEconomyState != null ? _runtimeEconomyState.GetPartyCapacityForCity(cityId) : 0;
        int currentCount = _runtimeEconomyState != null ? _runtimeEconomyState.GetPartyCountInCity(cityId) : 0;
        if (isBlocked)
        {
            return "Hire Friction: " + cityLabel + " already uses " + currentCount + "/" + capacity + " roster slots, so no new reserve company can be added right now.";
        }

        string cautionText;
        switch (NormalizeRecruitArchetypeKey(archetypeKey))
        {
            case "vanguard_company":
                cautionText = "leans the next reserve slot toward frontline tempo over flexibility";
                break;
            case "skirmish_company":
                cautionText = "leans the next reserve slot toward burst and route variance";
                break;
            case "sustain_company":
                cautionText = "leans the next reserve slot toward slower, safer relief";
                break;
            default:
                cautionText = "keeps the reserve bench flexible without specializing too early";
                break;
        }

        return "Hire Friction: hiring fills roster slot " + (currentCount + 1) + "/" + capacity + " and " + cautionText + ".";
    }

    private string BuildRecruitmentBlockedReasonText(string cityId, bool hasLinkedDungeon)
    {
        if (!hasLinkedDungeon)
        {
            return ResolveEntityDisplayName(cityId, "This city") + " has no linked dungeon, so recruitment cannot feed into a dispatch lane yet.";
        }

        if (_runtimeEconomyState == null || string.IsNullOrEmpty(cityId))
        {
            return "Recruitment data is unavailable right now.";
        }

        int partyCount = _runtimeEconomyState.GetPartyCountInCity(cityId);
        int capacity = _runtimeEconomyState.GetPartyCapacityForCity(cityId);
        if (partyCount >= capacity)
        {
            return ResolveEntityDisplayName(cityId, "This city") + " already uses all " + capacity + " roster slots. Compare or stage existing companies before hiring again.";
        }

        int activeExpeditionCount = _runtimeEconomyState.GetActiveExpeditionCountFromCity(cityId);
        return activeExpeditionCount > 0
            ? "Reserve hiring is still available while another company is out. Add depth now, then stage the next company for the return window."
            : "Reserve slot open. Choose a company package to recruit.";
    }

    private string ResolveRecruitBlockedStateKey(string cityId, bool hasLinkedDungeon)
    {
        if (!hasLinkedDungeon)
        {
            return "blocked_no_linked_dungeon";
        }

        int partyCount = _runtimeEconomyState != null ? _runtimeEconomyState.GetPartyCountInCity(cityId) : 0;
        int capacity = _runtimeEconomyState != null ? _runtimeEconomyState.GetPartyCapacityForCity(cityId) : 0;
        return partyCount >= capacity ? "blocked_city_cap" : "ready";
    }

    private string BuildRecruitmentBoardSummaryText(WorldEntityData city, string linkedDungeonLabel, string recommendedArchetypeKey, string blockedReasonText)
    {
        string recommendedLabel = PrototypeRpgPartyCatalog.GetPlaceholderPartyArchetypeDisplayName(recommendedArchetypeKey);
        string routeSummary = string.IsNullOrEmpty(linkedDungeonLabel) || linkedDungeonLabel == "None"
            ? "No linked dungeon"
            : BuildRecommendedRouteSummaryText(city.Id, GetLinkedDungeonIdForCity(city.Id));
        string queueLinkText = ResolveRecruitQueueLinkSummary(city.Id, recommendedLabel);
        string blockText = ResolveCanRecruitSelectedCityParty(city.Id)
            ? queueLinkText
            : blockedReasonText;
        return city.DisplayName + " board | Linked: " + linkedDungeonLabel + " | Route: " + CompactSurfaceText(routeSummary, 64) + " | Best hire: " + recommendedLabel + " | " + CompactSurfaceText(blockText, 72);
    }

    private string ResolveRecruitQueueLinkSummary(string cityId, string recommendedLabel)
    {
        if (_currentCommitmentQueueSurface != null &&
            _currentCommitmentQueueSurface.ImmediateOperation != null &&
            string.Equals(_currentCommitmentQueueSurface.ImmediateOperation.CityId, cityId, StringComparison.Ordinal))
        {
            return "Queue link: hiring " + recommendedLabel + " should flip this city toward the immediate lane.";
        }

        if (_currentOpportunityLadderSurface != null &&
            _currentOpportunityLadderSurface.PrimaryCandidate != null &&
            string.Equals(_currentOpportunityLadderSurface.PrimaryCandidate.CityId, cityId, StringComparison.Ordinal) &&
            string.Equals(_currentOpportunityLadderSurface.PrimaryCandidate.ActionStateKey, "recruit_now", StringComparison.OrdinalIgnoreCase))
        {
            return "Opportunity link: hiring here supports the board's current best move.";
        }

        return "Queue link: hiring here prepares the next dungeon-ready idle party package.";
    }

    private string BuildSelectedRecruitOfferSummaryText(string cityLabel, PrototypeWorldRecruitOfferData offer)
    {
        if (offer == PrototypeWorldRecruitOfferData.Empty)
        {
            return "None";
        }

        string recommendationText = offer.IsRecommended ? "Recommended board pick." : "Alternate board pick.";
        return cityLabel + " | " + offer.DisplayName + " | " + offer.RoleMixSummary + "\n" +
               recommendationText + " " + CompactSurfaceText(offer.RouteFitSummary, 96) + "\n" +
               CompactSurfaceText(offer.PressureFitSummary, 96) + "\n" +
               CompactSurfaceText(offer.FrictionSummary, 96);
    }

    private string BuildLastRecruitedPartySummaryText(string cityId, string cityLabel, PrototypeWorldRecruitOfferData offer)
    {
        string linkedDungeonId = GetLinkedDungeonIdForCity(cityId);
        string routeFit = !string.IsNullOrEmpty(linkedDungeonId) ? BuildRecommendedRouteSummaryText(cityId, linkedDungeonId) : "None";
        return cityLabel + " hired " + offer.DisplayName + " | " + offer.RoleMixSummary + " | Next route: " + CompactSurfaceText(routeFit, 52);
    }
    private string ResolveLastRecruitedPartySummaryText(string cityId)
    {
        return !string.IsNullOrEmpty(cityId) && _lastRecruitedPartySummaryByCityId.TryGetValue(cityId, out string summaryText)
            ? summaryText
            : "None";
    }

    private string BuildRecruitmentRefreshSummaryText(string cityId)
    {
        string cityLabel = ResolveEntityDisplayName(cityId, "This city");
        if (_runtimeEconomyState == null)
        {
            return "None";
        }

        int partyCount = _runtimeEconomyState.GetPartyCountInCity(cityId);
        int capacity = _runtimeEconomyState.GetPartyCapacityForCity(cityId);
        if (partyCount >= capacity)
        {
            return cityLabel + " already filled its " + capacity + " company slots. Compare, stage, or commit the current roster before refreshing hires.";
        }

        return "Next board refresh: Day " + (_runtimeEconomyState.WorldDayCount + 1) + ". Fit previews re-evaluate when pressure, readiness, or recommendation changes. Open slots: " + (capacity - partyCount) + ".";
    }

    private string BuildRecruitmentWorldPostureSummaryText()
    {
        if (_worldData == null || _worldData.Entities == null)
        {
            return "None";
        }

        int recruitableCount = 0;
        int cappedCount = 0;
        int activeCount = 0;
        string leadCityLabel = "None";
        string leadArchetypeLabel = "None";

        for (int i = 0; i < _worldData.Entities.Length; i++)
        {
            WorldEntityData entity = _worldData.Entities[i];
            if (entity == null || entity.Kind != WorldEntityKind.City)
            {
                continue;
            }

            string cityId = entity.Id;
            int activeExpeditions = _runtimeEconomyState != null ? _runtimeEconomyState.GetActiveExpeditionCountFromCity(cityId) : 0;
            int partyCount = _runtimeEconomyState != null ? _runtimeEconomyState.GetPartyCountInCity(cityId) : 0;
            if (ResolveCanRecruitSelectedCityParty(cityId))
            {
                recruitableCount += 1;
                if (leadCityLabel == "None")
                {
                    leadCityLabel = entity.DisplayName;
                    leadArchetypeLabel = PrototypeRpgPartyCatalog.GetPlaceholderPartyArchetypeDisplayName(ResolveRecruitOfferRecommendation(cityId, GetLinkedDungeonIdForCity(cityId)));
                }
                continue;
            }

            if (activeExpeditions > 0)
            {
                activeCount += 1;
            }
            else if (partyCount > 0)
            {
                cappedCount += 1;
            }
        }

        string leadText = recruitableCount > 0
            ? "Next board: " + leadCityLabel + " -> " + leadArchetypeLabel
            : "No city currently has an open reserve slot.";
        return recruitableCount + " recruitable | " + cappedCount + " cap-locked | " + activeCount + " active-return | " + leadText;
    }

    private string NormalizeRecruitArchetypeKey(string archetypeKey)
    {
        return string.IsNullOrWhiteSpace(archetypeKey) ? "balanced_company" : archetypeKey.Trim().ToLowerInvariant();
    }

    private string ResolveEntityDisplayName(string entityId, string fallbackLabel)
    {
        WorldEntityData entity = FindEntity(entityId);
        return entity != null ? entity.DisplayName : fallbackLabel;
    }

    private PrototypeWorldRecruitOfferData ResolveSelectedRecruitOffer()
    {
        return GetRecruitOffer(_selectedRecruitOfferIndex);
    }

    private PrototypeWorldRecruitOfferData GetRecruitOffer(int index)
    {
        if (_currentRecruitmentBoardSurface == PrototypeWorldRecruitmentBoardSurfaceData.Empty ||
            _currentRecruitmentBoardSurface.Offers == null ||
            index < 0 ||
            index >= _currentRecruitmentBoardSurface.Offers.Length)
        {
            return PrototypeWorldRecruitOfferData.Empty;
        }

        return _currentRecruitmentBoardSurface.Offers[index] ?? PrototypeWorldRecruitOfferData.Empty;
    }

    private PrototypeRpgPartyDefinition ResolveRecruitedPartyDefinition(string partyId)
    {
        return !string.IsNullOrEmpty(partyId) && _recruitedPartyDataByPartyId.TryGetValue(partyId, out PrototypeWorldRecruitedPartyData recruitedPartyData) && recruitedPartyData != null && recruitedPartyData.PartyDefinition != null
            ? recruitedPartyData.PartyDefinition
            : null;
    }
}


