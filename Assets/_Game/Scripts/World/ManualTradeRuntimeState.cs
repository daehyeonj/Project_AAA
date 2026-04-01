using System.Collections.Generic;
using UnityEngine;

public sealed class ManualTradeRuntimeState
{
    private const float DefaultTickIntervalSeconds = 1.25f;
    private const int RecentDayLogLimit = 3;
    private const int RecentExpeditionLogLimit = 3;
    private const int ReserveStockFloor = 1;
    private const int DefaultPartyPower = 3;
    private const int DefaultPartyCarryCapacity = 2;

    private enum PartyState
    {
        Idle,
        OnExpedition
    }

    private sealed class PartyRuntimeData
    {
        public string PartyId;
        public string HomeCityId;
        public PartyState State;
        public int Power;
        public int CarryCapacity;
        public string TargetDungeonId;
        public int DaysRemaining;
        public string LastResultSummary;

        public PartyRuntimeData(string partyId, string homeCityId, int power, int carryCapacity)
        {
            PartyId = partyId ?? string.Empty;
            HomeCityId = homeCityId ?? string.Empty;
            State = PartyState.Idle;
            Power = power > 0 ? power : 1;
            CarryCapacity = carryCapacity > 0 ? carryCapacity : 1;
            TargetDungeonId = string.Empty;
            DaysRemaining = 0;
            LastResultSummary = "None";
        }
    }

    private sealed class DungeonExpeditionProfile
    {
        public readonly string DungeonId;
        public readonly int RecommendedPower;
        public readonly int ExpeditionDurationDays;
        public readonly string[] RewardResourceIds;
        public readonly int MaxActiveParties;

        public DungeonExpeditionProfile(string dungeonId, int recommendedPower, int expeditionDurationDays, string[] rewardResourceIds, int maxActiveParties)
        {
            DungeonId = dungeonId ?? string.Empty;
            RecommendedPower = recommendedPower > 0 ? recommendedPower : 1;
            ExpeditionDurationDays = expeditionDurationDays > 0 ? expeditionDurationDays : 1;
            RewardResourceIds = rewardResourceIds ?? System.Array.Empty<string>();
            MaxActiveParties = maxActiveParties > 0 ? maxActiveParties : 1;
        }
    }

    private sealed class ExpeditionContractData
    {
        public readonly string CityId;
        public readonly string DungeonId;

        public ExpeditionContractData(string cityId, string dungeonId)
        {
            CityId = cityId ?? string.Empty;
            DungeonId = dungeonId ?? string.Empty;
        }
    }

    private sealed class EntityDaySummary
    {
        public int Produced;
        public int DungeonImported;
        public int Imported;
        public int Exported;
        public int ProcessedIn;
        public int ProcessedOut;
        public int ProcessedTotal;
        public int ProcessingBlocked;
        public int ProcessingReserved;
        public int Consumed;
        public int CriticalFulfilled;
        public int CriticalUnmet;
        public int NormalFulfilled;
        public int NormalUnmet;
        public int Fulfilled;
        public int Unmet;
        public int Shortages;
        public int ClaimedOut;
    }

    private sealed class ResourceDaySummary
    {
        public int Produced;
        public int DungeonImported;
        public int Imported;
        public int Exported;
        public int ProcessedIn;
        public int ProcessedOut;
        public int Consumed;
        public int Fulfilled;
        public int Unmet;
        public int Shortages;
        public int ClaimedOut;
    }

    private sealed class TradeEvent
    {
        public readonly string SupplierEntityId;
        public readonly string ConsumerEntityId;
        public readonly string ResourceId;

        public TradeEvent(string supplierEntityId, string consumerEntityId, string resourceId)
        {
            SupplierEntityId = supplierEntityId ?? string.Empty;
            ConsumerEntityId = consumerEntityId ?? string.Empty;
            ResourceId = resourceId ?? string.Empty;
        }
    }

    private sealed class ClaimEvent
    {
        public readonly string DungeonEntityId;
        public readonly string CityEntityId;
        public readonly string ResourceId;

        public ClaimEvent(string dungeonEntityId, string cityEntityId, string resourceId)
        {
            DungeonEntityId = dungeonEntityId ?? string.Empty;
            CityEntityId = cityEntityId ?? string.Empty;
            ResourceId = resourceId ?? string.Empty;
        }
    }

    private readonly WorldData _worldData;
    private readonly string[] _resourceIds;
    private readonly Dictionary<string, WorldEntityData> _entityById;
    private readonly Dictionary<string, WorldRouteData> _routeById;
    private readonly Dictionary<string, Dictionary<string, int>> _defaultStocks;
    private readonly Dictionary<string, Dictionary<string, int>> _stockByEntityId;
    private readonly Dictionary<string, EntityDaySummary> _lastDayEntitySummaries;
    private readonly Dictionary<string, Dictionary<string, ResourceDaySummary>> _lastDayResourceSummariesByEntityId;
    private readonly Dictionary<string, int> _totalShortagesByEntityId;
    private readonly Dictionary<string, int> _totalFulfilledByEntityId;
    private readonly Dictionary<string, int> _totalUnmetByEntityId;
    private readonly Dictionary<string, int> _totalCriticalUnmetByEntityId;
    private readonly Dictionary<string, int> _totalNormalUnmetByEntityId;
    private readonly Dictionary<string, int> _lastDayRouteUsageByRouteId;
    private readonly List<string> _recentDayLogs;
    private readonly List<TradeEvent> _lastDayTradeEvents;
    private readonly List<ClaimEvent> _lastDayClaimEvents;
    private readonly Dictionary<string, DungeonExpeditionProfile> _dungeonExpeditionProfilesByDungeonId;
    private readonly List<ExpeditionContractData> _expeditionContracts;
    private readonly List<PartyRuntimeData> _parties;
    private readonly List<string> _recentExpeditionLogs;
    private readonly Dictionary<string, string> _lastExpeditionResultByCityId;
    private readonly Dictionary<string, string> _lastExpeditionResultByDungeonId;
    private readonly Dictionary<string, int> _expeditionLootReturnedByCityId;
    private readonly Dictionary<string, string> _lastRunLootSummaryByCityId;
    private readonly Dictionary<string, string> _lastRunSurvivingMembersByCityId;
    private readonly Dictionary<string, string> _lastRunClearedEncountersByCityId;
    private readonly Dictionary<string, string> _lastRunEventChoiceByCityId;
    private readonly Dictionary<string, string> _lastRunLootBreakdownByCityId;
    private readonly Dictionary<string, string> _lastRunRouteByCityId;
    private readonly Dictionary<string, string> _lastRunDungeonByCityId;
    private DirectTradeScanResult _currentTradeScanResult;
    private int _nextPartySequence;

    public bool AutoTickEnabled { get; private set; }
    public bool AutoTickPaused { get; private set; }
    public float TickIntervalSeconds { get; private set; }
    public float TickTimer { get; private set; }
    public int AutoTickCount { get; private set; }
    public int TradeStepCount { get; private set; }
    public int WorldDayCount { get; private set; }
    public int LastDayProducedTotal { get; private set; }
    public int LastDayClaimedDungeonOutputsTotal { get; private set; }
    public int LastDayTradedTotal { get; private set; }
    public int LastDayProcessedTotal { get; private set; }
    public int LastDayConsumedTotal { get; private set; }
    public int LastDayCriticalFulfilledTotal { get; private set; }
    public int LastDayCriticalUnmetTotal { get; private set; }
    public int LastDayNormalFulfilledTotal { get; private set; }
    public int LastDayNormalUnmetTotal { get; private set; }
    public int LastDayFulfilledTotal { get; private set; }
    public int LastDayUnmetTotal { get; private set; }
    public int LastDayShortagesTotal { get; private set; }
    public int LastDayProcessingBlockedTotal { get; private set; }
    public int LastDayProcessingReservedTotal { get; private set; }
    public int LastDayRouteCapacityUsedTotal { get; private set; }
    public int CurrentUnclaimedDungeonOutputsTotal { get; private set; }
    public int TotalParties => _parties.Count;
    public int IdleParties => CountIdleParties();
    public int ActiveExpeditions => CountActiveExpeditions();
    public int AvailableContracts => CountAvailableContracts();
    public int ExpeditionSuccessCount { get; private set; }
    public int ExpeditionFailureCount { get; private set; }
    public int ExpeditionLootReturnedTotal { get; private set; }
    public int TotalShortages => SumDictionaryValues(_totalShortagesByEntityId);
    public int TotalFulfilled => SumDictionaryValues(_totalFulfilledByEntityId);
    public int TotalUnmet => SumDictionaryValues(_totalUnmetByEntityId);
    public int TotalCriticalUnmet => SumDictionaryValues(_totalCriticalUnmetByEntityId);
    public int TotalNormalUnmet => SumDictionaryValues(_totalNormalUnmetByEntityId);
    public DirectTradeScanResult CurrentTradeScanResult => _currentTradeScanResult ?? DirectTradeScanResult.Empty;

    public ManualTradeRuntimeState(WorldData worldData, ResourceData[] resources)
    {
        _worldData = worldData ?? new WorldData(System.Array.Empty<WorldEntityData>(), System.Array.Empty<WorldRouteData>());
        _resourceIds = BuildResourceIds(resources);
        _entityById = BuildEntityLookup(_worldData);
        _routeById = BuildRouteLookup(_worldData);
        _defaultStocks = BuildDefaultStocks(_worldData, _resourceIds);
        _stockByEntityId = new Dictionary<string, Dictionary<string, int>>();
        _lastDayEntitySummaries = new Dictionary<string, EntityDaySummary>();
        _lastDayResourceSummariesByEntityId = new Dictionary<string, Dictionary<string, ResourceDaySummary>>();
        _totalShortagesByEntityId = new Dictionary<string, int>();
        _totalFulfilledByEntityId = new Dictionary<string, int>();
        _totalUnmetByEntityId = new Dictionary<string, int>();
        _totalCriticalUnmetByEntityId = new Dictionary<string, int>();
        _totalNormalUnmetByEntityId = new Dictionary<string, int>();
        _lastDayRouteUsageByRouteId = new Dictionary<string, int>();
        _recentDayLogs = new List<string>(RecentDayLogLimit);
        _lastDayTradeEvents = new List<TradeEvent>();
        _lastDayClaimEvents = new List<ClaimEvent>();
        _dungeonExpeditionProfilesByDungeonId = BuildDungeonExpeditionProfiles(_worldData);
        _expeditionContracts = BuildExpeditionContracts(_worldData, _dungeonExpeditionProfilesByDungeonId);
        _parties = new List<PartyRuntimeData>();
        _recentExpeditionLogs = new List<string>(RecentExpeditionLogLimit);
        _lastExpeditionResultByCityId = new Dictionary<string, string>();
        _lastExpeditionResultByDungeonId = new Dictionary<string, string>();
        _expeditionLootReturnedByCityId = new Dictionary<string, int>();
        _lastRunLootSummaryByCityId = new Dictionary<string, string>();
        _lastRunSurvivingMembersByCityId = new Dictionary<string, string>();
        _lastRunClearedEncountersByCityId = new Dictionary<string, string>();
        _lastRunEventChoiceByCityId = new Dictionary<string, string>();
        _lastRunLootBreakdownByCityId = new Dictionary<string, string>();
        _lastRunRouteByCityId = new Dictionary<string, string>();
        _lastRunDungeonByCityId = new Dictionary<string, string>();
        _currentTradeScanResult = DirectTradeScanResult.Empty;
        TickIntervalSeconds = DefaultTickIntervalSeconds;
        AutoTickEnabled = false;
        AutoTickPaused = false;
        Reset();
    }

    public void Reset()
    {
        WorldDayCount = 0;
        TradeStepCount = 0;
        AutoTickCount = 0;
        TickTimer = 0f;
        LastDayProducedTotal = 0;
        LastDayClaimedDungeonOutputsTotal = 0;
        LastDayTradedTotal = 0;
        LastDayProcessedTotal = 0;
        LastDayConsumedTotal = 0;
        LastDayCriticalFulfilledTotal = 0;
        LastDayCriticalUnmetTotal = 0;
        LastDayNormalFulfilledTotal = 0;
        LastDayNormalUnmetTotal = 0;
        LastDayFulfilledTotal = 0;
        LastDayUnmetTotal = 0;
        LastDayShortagesTotal = 0;
        LastDayProcessingBlockedTotal = 0;
        LastDayProcessingReservedTotal = 0;
        LastDayRouteCapacityUsedTotal = 0;
        _recentDayLogs.Clear();
        _lastDayTradeEvents.Clear();
        _lastDayClaimEvents.Clear();
        _lastDayEntitySummaries.Clear();
        _lastDayResourceSummariesByEntityId.Clear();
        _totalShortagesByEntityId.Clear();
        _totalFulfilledByEntityId.Clear();
        _totalUnmetByEntityId.Clear();
        _totalCriticalUnmetByEntityId.Clear();
        _totalNormalUnmetByEntityId.Clear();
        _lastDayRouteUsageByRouteId.Clear();
        _stockByEntityId.Clear();
        _parties.Clear();
        _recentExpeditionLogs.Clear();
        _lastExpeditionResultByCityId.Clear();
        _lastExpeditionResultByDungeonId.Clear();
        _expeditionLootReturnedByCityId.Clear();
        _lastRunLootSummaryByCityId.Clear();
        _lastRunSurvivingMembersByCityId.Clear();
        _lastRunClearedEncountersByCityId.Clear();
        _lastRunEventChoiceByCityId.Clear();
        _lastRunLootBreakdownByCityId.Clear();
        _lastRunRouteByCityId.Clear();
        _lastRunDungeonByCityId.Clear();
        _nextPartySequence = 0;
        ExpeditionSuccessCount = 0;
        ExpeditionFailureCount = 0;
        ExpeditionLootReturnedTotal = 0;

        foreach (KeyValuePair<string, WorldEntityData> pair in _entityById)
        {
            if (!_totalShortagesByEntityId.ContainsKey(pair.Key))
            {
                _totalShortagesByEntityId.Add(pair.Key, 0);
            }

            if (!_totalFulfilledByEntityId.ContainsKey(pair.Key))
            {
                _totalFulfilledByEntityId.Add(pair.Key, 0);
            }

            if (!_totalUnmetByEntityId.ContainsKey(pair.Key))
            {
                _totalUnmetByEntityId.Add(pair.Key, 0);
            }

            if (!_totalCriticalUnmetByEntityId.ContainsKey(pair.Key))
            {
                _totalCriticalUnmetByEntityId.Add(pair.Key, 0);
            }

            if (!_totalNormalUnmetByEntityId.ContainsKey(pair.Key))
            {
                _totalNormalUnmetByEntityId.Add(pair.Key, 0);
            }

            if (pair.Value.Kind == WorldEntityKind.City && !_expeditionLootReturnedByCityId.ContainsKey(pair.Key))
            {
                _expeditionLootReturnedByCityId.Add(pair.Key, 0);
            }

            if (pair.Value.Kind == WorldEntityKind.City && !_lastRunLootSummaryByCityId.ContainsKey(pair.Key))
            {
                _lastRunLootSummaryByCityId.Add(pair.Key, "None");
            }

            if (pair.Value.Kind == WorldEntityKind.City && !_lastRunSurvivingMembersByCityId.ContainsKey(pair.Key))
            {
                _lastRunSurvivingMembersByCityId.Add(pair.Key, "None");
            }

            if (pair.Value.Kind == WorldEntityKind.City && !_lastRunClearedEncountersByCityId.ContainsKey(pair.Key))
            {
                _lastRunClearedEncountersByCityId.Add(pair.Key, "None");
            }

            if (pair.Value.Kind == WorldEntityKind.City && !_lastRunEventChoiceByCityId.ContainsKey(pair.Key))
            {
                _lastRunEventChoiceByCityId.Add(pair.Key, "None");
            }

            if (pair.Value.Kind == WorldEntityKind.City && !_lastRunLootBreakdownByCityId.ContainsKey(pair.Key))
            {
                _lastRunLootBreakdownByCityId.Add(pair.Key, "None");
            }

            if (pair.Value.Kind == WorldEntityKind.City && !_lastRunRouteByCityId.ContainsKey(pair.Key))
            {
                _lastRunRouteByCityId.Add(pair.Key, "None");
            }

            if (pair.Value.Kind == WorldEntityKind.City && !_lastRunDungeonByCityId.ContainsKey(pair.Key))
            {
                _lastRunDungeonByCityId.Add(pair.Key, "None");
            }
        }

        foreach (KeyValuePair<string, WorldRouteData> pair in _routeById)
        {
            _lastDayRouteUsageByRouteId[pair.Key] = 0;
        }

        foreach (KeyValuePair<string, Dictionary<string, int>> pair in _defaultStocks)
        {
            Dictionary<string, int> stockCopy = new Dictionary<string, int>();
            foreach (KeyValuePair<string, int> stockPair in pair.Value)
            {
                stockCopy[stockPair.Key] = stockPair.Value;
            }

            _stockByEntityId[pair.Key] = stockCopy;
        }

        CurrentUnclaimedDungeonOutputsTotal = CalculateCurrentUnclaimedDungeonOutputsTotal();
        RefreshCurrentTradeScanResult();
    }

    public void ToggleAutoTickEnabled()
    {
        AutoTickEnabled = !AutoTickEnabled;
        TickTimer = 0f;

        if (!AutoTickEnabled)
        {
            AutoTickPaused = false;
        }
    }

    public void ToggleAutoTickPaused()
    {
        if (!AutoTickEnabled)
        {
            return;
        }

        AutoTickPaused = !AutoTickPaused;
    }

    public void UpdateAutoTick(float deltaTime)
    {
        if (!AutoTickEnabled || AutoTickPaused || TickIntervalSeconds <= 0f || deltaTime <= 0f)
        {
            return;
        }

        TickTimer += deltaTime;
        if (TickTimer < TickIntervalSeconds)
        {
            return;
        }

        TickTimer -= TickIntervalSeconds;
        RunEconomyDay(true);
    }

    public void RunEconomyDay()
    {
        RunEconomyDay(false);
    }

    public bool RecruitParty(string cityId)
    {
        if (!TryGetEntity(cityId, out WorldEntityData city) || city.Kind != WorldEntityKind.City || FindAnyPartyByHomeCity(cityId) != null)
        {
            return false;
        }

        _nextPartySequence += 1;
        PartyRuntimeData party = new PartyRuntimeData("Party " + _nextPartySequence, cityId, DefaultPartyPower, DefaultPartyCarryCapacity);
        party.LastResultSummary = "Ready in " + city.DisplayName;
        _parties.Add(party);
        _lastExpeditionResultByCityId[cityId] = party.LastResultSummary;
        AppendRecentExpeditionLog("Day " + WorldDayCount + " | " + city.DisplayName + " recruited " + party.PartyId + " (Power " + party.Power + ", Carry " + party.CarryCapacity + ")");
        return true;
    }

    public string BeginDungeonRun(string cityId, string dungeonId)
    {
        if (!TryGetEntity(cityId, out WorldEntityData city) || city.Kind != WorldEntityKind.City ||
            !TryGetEntity(dungeonId, out WorldEntityData dungeon) || dungeon.Kind != WorldEntityKind.Dungeon)
        {
            return string.Empty;
        }

        PartyRuntimeData party = FindIdleParty(cityId);
        if (party == null)
        {
            return string.Empty;
        }

        party.State = PartyState.OnExpedition;
        party.TargetDungeonId = dungeonId;
        party.DaysRemaining = 1;
        party.LastResultSummary = party.PartyId + " entered " + dungeon.DisplayName;
        _lastExpeditionResultByCityId[cityId] = party.LastResultSummary;
        _lastExpeditionResultByDungeonId[dungeonId] = party.LastResultSummary;
        _lastRunLootSummaryByCityId[cityId] = "None";
        _lastRunSurvivingMembersByCityId[cityId] = "None";
        _lastRunClearedEncountersByCityId[cityId] = "None";
        _lastRunEventChoiceByCityId[cityId] = "None";
        _lastRunLootBreakdownByCityId[cityId] = "None";
        _lastRunRouteByCityId[cityId] = "None";
        _lastRunDungeonByCityId[cityId] = "None";
        AppendRecentExpeditionLog("Day " + WorldDayCount + " | " + city.DisplayName + " sent " + party.PartyId + " into " + dungeon.DisplayName);
        return party.PartyId;
    }

    public bool DispatchExpedition(string cityId)
    {
        if (!TryGetEntity(cityId, out WorldEntityData city) || city.Kind != WorldEntityKind.City)
        {
            return false;
        }

        ExpeditionContractData contract = GetContractForCity(cityId);
        PartyRuntimeData party = FindIdleParty(cityId);
        if (contract == null || party == null || !TryGetDungeonProfile(contract.DungeonId, out DungeonExpeditionProfile profile))
        {
            return false;
        }

        if (CountActiveExpeditionsForDungeonInternal(contract.DungeonId) >= profile.MaxActiveParties || !TryGetEntity(contract.DungeonId, out WorldEntityData dungeon))
        {
            return false;
        }

        party.State = PartyState.OnExpedition;
        party.TargetDungeonId = contract.DungeonId;
        party.DaysRemaining = profile.ExpeditionDurationDays;
        party.LastResultSummary = party.PartyId + " -> " + dungeon.DisplayName + " (" + party.DaysRemaining + "d remaining)";
        _lastExpeditionResultByCityId[cityId] = party.LastResultSummary;
        _lastExpeditionResultByDungeonId[contract.DungeonId] = party.LastResultSummary;
        AppendRecentExpeditionLog("Day " + WorldDayCount + " | " + city.DisplayName + " dispatched " + party.PartyId + " to " + dungeon.DisplayName + " (" + profile.ExpeditionDurationDays + "d)");
        return true;
    }

    public string GetStocksText(string entityId)
    {
        if (string.IsNullOrEmpty(entityId) || _resourceIds.Length == 0 || !_stockByEntityId.TryGetValue(entityId, out Dictionary<string, int> stockMap))
        {
            return "None";
        }

        string[] parts = new string[_resourceIds.Length];
        for (int i = 0; i < _resourceIds.Length; i++)
        {
            string resourceId = _resourceIds[i];
            stockMap.TryGetValue(resourceId, out int stockAmount);
            parts[i] = resourceId + "=" + stockAmount;
        }

        return string.Join(", ", parts);
    }

    public int GetStockAmount(string entityId, string resourceId)
    {
        return GetStock(entityId, resourceId);
    }

    public int GetExportableAmount(string entityId, string resourceId)
    {
        if (!TryGetEntity(entityId, out WorldEntityData entity) || entity.Kind != WorldEntityKind.City || string.IsNullOrEmpty(resourceId))
        {
            return 0;
        }

        int stock = GetStock(entityId, resourceId);
        int reserveFloor = GetReserveFloor(entity, resourceId);
        int exportable = stock - reserveFloor;
        return exportable > 0 ? exportable : 0;
    }

    public string GetReserveStockRuleText(string entityId)
    {
        if (!TryGetEntity(entityId, out WorldEntityData entity) || entity.Kind != WorldEntityKind.City)
        {
            return "None";
        }

        return "Keep 1 of supply/processed outputs";
    }

    public string GetRecentDayLogText(int index)
    {
        return index >= 0 && index < _recentDayLogs.Count ? _recentDayLogs[index] : "None";
    }

    public string GetRecentExpeditionLogText(int index)
    {
        return index >= 0 && index < _recentExpeditionLogs.Count ? _recentExpeditionLogs[index] : "None";
    }

    public string GetIdlePartyIdInCity(string cityId)
    {
        PartyRuntimeData party = FindIdleParty(cityId);
        return party != null ? party.PartyId : string.Empty;
    }

    public int GetPartyCountInCity(string cityId)
    {
        if (string.IsNullOrEmpty(cityId))
        {
            return 0;
        }

        int count = 0;
        for (int i = 0; i < _parties.Count; i++)
        {
            PartyRuntimeData party = _parties[i];
            if (party != null && party.HomeCityId == cityId)
            {
                count += 1;
            }
        }

        return count;
    }

    public int GetIdlePartyCountInCity(string cityId)
    {
        if (string.IsNullOrEmpty(cityId))
        {
            return 0;
        }

        int count = 0;
        for (int i = 0; i < _parties.Count; i++)
        {
            PartyRuntimeData party = _parties[i];
            if (party != null && party.HomeCityId == cityId && party.State == PartyState.Idle)
            {
                count += 1;
            }
        }

        return count;
    }

    public int GetActiveExpeditionCountFromCity(string cityId)
    {
        if (string.IsNullOrEmpty(cityId))
        {
            return 0;
        }

        int count = 0;
        for (int i = 0; i < _parties.Count; i++)
        {
            PartyRuntimeData party = _parties[i];
            if (party != null && party.HomeCityId == cityId && party.State == PartyState.OnExpedition)
            {
                count += 1;
            }
        }

        return count;
    }

    public int GetActiveExpeditionCountForDungeon(string dungeonId)
    {
        return CountActiveExpeditionsForDungeonInternal(dungeonId);
    }

    public int GetExpeditionLootReturnedTotalForCity(string cityId)
    {
        return !string.IsNullOrEmpty(cityId) && _expeditionLootReturnedByCityId.TryGetValue(cityId, out int value) ? value : 0;
    }

    public string GetLastRunLootSummaryForCity(string cityId)
    {
        return !string.IsNullOrEmpty(cityId) && _lastRunLootSummaryByCityId.TryGetValue(cityId, out string value)
            ? value
            : "None";
    }

    public string GetLastRunSurvivingMembersSummaryForCity(string cityId)
    {
        return !string.IsNullOrEmpty(cityId) && _lastRunSurvivingMembersByCityId.TryGetValue(cityId, out string value)
            ? value
            : "None";
    }

    public string GetLastRunClearedEncountersSummaryForCity(string cityId)
    {
        return !string.IsNullOrEmpty(cityId) && _lastRunClearedEncountersByCityId.TryGetValue(cityId, out string value)
            ? value
            : "None";
    }

    public string GetLastRunEventChoiceSummaryForCity(string cityId)
    {
        return !string.IsNullOrEmpty(cityId) && _lastRunEventChoiceByCityId.TryGetValue(cityId, out string value)
            ? value
            : "None";
    }

    public string GetLastRunLootBreakdownSummaryForCity(string cityId)
    {
        return !string.IsNullOrEmpty(cityId) && _lastRunLootBreakdownByCityId.TryGetValue(cityId, out string value)
            ? value
            : "None";
    }

    public string GetLastRunRouteSummaryForCity(string cityId)
    {
        return !string.IsNullOrEmpty(cityId) && _lastRunRouteByCityId.TryGetValue(cityId, out string value)
            ? value
            : "None";
    }

    public string GetLastRunDungeonSummaryForCity(string cityId)
    {
        return !string.IsNullOrEmpty(cityId) && _lastRunDungeonByCityId.TryGetValue(cityId, out string value)
            ? value
            : "None";
    }

    public string GetAvailableContractTextForCity(string cityId)
    {
        ExpeditionContractData contract = GetContractForCity(cityId);
        return BuildContractStatusText(contract);
    }

    public string GetAvailableContractTextForDungeon(string dungeonId)
    {
        ExpeditionContractData contract = GetContractForDungeon(dungeonId);
        return BuildContractStatusText(contract);
    }

    public string GetLinkedDungeonText(string cityId)
    {
        ExpeditionContractData contract = GetContractForCity(cityId);
        if (contract == null || !TryGetEntity(contract.DungeonId, out WorldEntityData dungeon))
        {
            return "None";
        }

        return dungeon.DisplayName;
    }

    public int GetRecommendedPower(string dungeonId)
    {
        return TryGetDungeonProfile(dungeonId, out DungeonExpeditionProfile profile) ? profile.RecommendedPower : 0;
    }

    public int GetExpeditionDurationDays(string dungeonId)
    {
        return TryGetDungeonProfile(dungeonId, out DungeonExpeditionProfile profile) ? profile.ExpeditionDurationDays : 0;
    }

    public string GetRewardPreviewText(string dungeonId)
    {
        return TryGetDungeonProfile(dungeonId, out DungeonExpeditionProfile profile) ? BuildRewardPreview(profile, DefaultPartyCarryCapacity) : "None";
    }

    public string GetExpeditionStatusTextForCity(string cityId)
    {
        PartyRuntimeData activeParty = FindActivePartyForCity(cityId);
        if (activeParty != null)
        {
            return BuildActivePartyStatus(activeParty);
        }

        return !string.IsNullOrEmpty(cityId) && _lastExpeditionResultByCityId.TryGetValue(cityId, out string value) ? value : "None";
    }

    public string GetExpeditionStatusTextForDungeon(string dungeonId)
    {
        PartyRuntimeData activeParty = FindActivePartyForDungeon(dungeonId);
        if (activeParty != null)
        {
            return BuildActivePartyStatus(activeParty);
        }

        return !string.IsNullOrEmpty(dungeonId) && _lastExpeditionResultByDungeonId.TryGetValue(dungeonId, out string value) ? value : "None";
    }

    public string GetLastDayTradeEventText(int index)
    {
        return index >= 0 && index < _lastDayTradeEvents.Count ? FormatTradeEvent(_lastDayTradeEvents[index]) : "None";
    }

    public string GetLastDayClaimEventText(int index)
    {
        return index >= 0 && index < _lastDayClaimEvents.Count ? FormatClaimEvent(_lastDayClaimEvents[index]) : "None";
    }

    public string GetLastDayIncomingTradeText(string entityId)
    {
        if (string.IsNullOrEmpty(entityId) || _lastDayTradeEvents.Count == 0)
        {
            return "None";
        }

        string text = string.Empty;
        for (int i = 0; i < _lastDayTradeEvents.Count; i++)
        {
            TradeEvent tradeEvent = _lastDayTradeEvents[i];
            if (tradeEvent.ConsumerEntityId != entityId || string.IsNullOrEmpty(tradeEvent.ResourceId))
            {
                continue;
            }

            string segment = tradeEvent.ResourceId + " <- " + GetEntityDisplayName(tradeEvent.SupplierEntityId);
            text = string.IsNullOrEmpty(text) ? segment : text + " | " + segment;
        }

        return string.IsNullOrEmpty(text) ? "None" : text;
    }

    public string GetLastDayOutgoingTradeText(string entityId)
    {
        if (string.IsNullOrEmpty(entityId) || _lastDayTradeEvents.Count == 0)
        {
            return "None";
        }

        string text = string.Empty;
        for (int i = 0; i < _lastDayTradeEvents.Count; i++)
        {
            TradeEvent tradeEvent = _lastDayTradeEvents[i];
            if (tradeEvent.SupplierEntityId != entityId || string.IsNullOrEmpty(tradeEvent.ResourceId))
            {
                continue;
            }

            string segment = tradeEvent.ResourceId + " -> " + GetEntityDisplayName(tradeEvent.ConsumerEntityId);
            text = string.IsNullOrEmpty(text) ? segment : text + " | " + segment;
        }

        return string.IsNullOrEmpty(text) ? "None" : text;
    }

    public int GetLastDayProduced(string entityId)
    {
        return GetEntityDaySummary(entityId).Produced;
    }

    public int GetLastDayDungeonImported(string entityId)
    {
        return GetEntityDaySummary(entityId).DungeonImported;
    }

    public int GetLastDayImported(string entityId)
    {
        return GetEntityDaySummary(entityId).Imported;
    }

    public int GetLastDayExported(string entityId)
    {
        return GetEntityDaySummary(entityId).Exported;
    }

    public int GetLastDayProcessedIn(string entityId)
    {
        return GetEntityDaySummary(entityId).ProcessedIn;
    }

    public int GetLastDayProcessedOut(string entityId)
    {
        return GetEntityDaySummary(entityId).ProcessedOut;
    }

    public int GetLastDayProcessedTotal(string entityId)
    {
        return GetEntityDaySummary(entityId).ProcessedTotal;
    }

    public int GetLastDayConsumed(string entityId)
    {
        return GetEntityDaySummary(entityId).Consumed;
    }

    public int GetLastDayCriticalFulfilled(string entityId)
    {
        return GetEntityDaySummary(entityId).CriticalFulfilled;
    }

    public int GetLastDayCriticalUnmet(string entityId)
    {
        return GetEntityDaySummary(entityId).CriticalUnmet;
    }

    public int GetLastDayNormalFulfilled(string entityId)
    {
        return GetEntityDaySummary(entityId).NormalFulfilled;
    }

    public int GetLastDayNormalUnmet(string entityId)
    {
        return GetEntityDaySummary(entityId).NormalUnmet;
    }

    public int GetLastDayFulfilled(string entityId)
    {
        return GetEntityDaySummary(entityId).Fulfilled;
    }

    public int GetLastDayUnmet(string entityId)
    {
        return GetEntityDaySummary(entityId).Unmet;
    }

    public int GetLastDayShortages(string entityId)
    {
        return GetEntityDaySummary(entityId).Shortages;
    }

    public int GetLastDayProcessingBlocked(string entityId)
    {
        return GetEntityDaySummary(entityId).ProcessingBlocked;
    }

    public int GetLastDayProcessingReserved(string entityId)
    {
        return GetEntityDaySummary(entityId).ProcessingReserved;
    }

    public int GetTotalShortages(string entityId)
    {
        return GetDictionaryValue(_totalShortagesByEntityId, entityId);
    }

    public int GetTotalFulfilled(string entityId)
    {
        return GetDictionaryValue(_totalFulfilledByEntityId, entityId);
    }

    public int GetTotalUnmet(string entityId)
    {
        return GetDictionaryValue(_totalUnmetByEntityId, entityId);
    }

    public int GetTotalCriticalUnmet(string entityId)
    {
        return GetDictionaryValue(_totalCriticalUnmetByEntityId, entityId);
    }

    public int GetTotalNormalUnmet(string entityId)
    {
        return GetDictionaryValue(_totalNormalUnmetByEntityId, entityId);
    }

    public int GetLastDayShortageCount(string entityId, string resourceId)
    {
        return GetResourceDaySummary(entityId, resourceId).Shortages;
    }

    public int GetLastDayUnmetCount(string entityId, string resourceId)
    {
        return GetResourceDaySummary(entityId, resourceId).Unmet;
    }

    public int GetLastDayClaimedOut(string entityId)
    {
        return GetEntityDaySummary(entityId).ClaimedOut;
    }

    public int GetRouteCapacityPerDay(string routeId)
    {
        return TryGetRoute(routeId, out WorldRouteData route) ? route.CapacityPerDay : 0;
    }

    public int GetLastDayRouteUsage(string routeId)
    {
        return !string.IsNullOrEmpty(routeId) && _lastDayRouteUsageByRouteId.TryGetValue(routeId, out int usage) ? usage : 0;
    }

    public string GetRouteUtilizationText(string routeId)
    {
        if (!TryGetRoute(routeId, out WorldRouteData route) || route.CapacityPerDay < 1)
        {
            return "None";
        }

        int usage = GetLastDayRouteUsage(routeId);
        int percent = Mathf.RoundToInt((usage * 100f) / route.CapacityPerDay);
        return usage + "/" + route.CapacityPerDay + (usage >= route.CapacityPerDay ? " (Saturated)" : " (" + percent + "%)");
    }

    private void RunEconomyDay(bool isAutoTick)
    {
        WorldDayCount += 1;
        TradeStepCount += 1;
        TickTimer = 0f;

        if (isAutoTick)
        {
            AutoTickCount += 1;
        }

        ResetLastDayTotals();

        WorldEntityData[] entities = _worldData.Entities ?? System.Array.Empty<WorldEntityData>();
        RunLocalProductionPhase(entities);
        RunDungeonClaimPhase(entities);
        RefreshCurrentTradeScanResult();
        RunDirectTradePhase(CurrentTradeScanResult);
        RunLocalProcessingPhase(entities);
        RunLocalConsumptionPhase(entities);

        CurrentUnclaimedDungeonOutputsTotal = CalculateCurrentUnclaimedDungeonOutputsTotal();
        RefreshCurrentTradeScanResult();
        ApplyEndOfDayShortagePressure(CurrentTradeScanResult);
        RunExpeditionPhase();
        RefreshCurrentTradeScanResult();
        AppendRecentDayLog(BuildRecentDayLog());
    }

    private void ResetLastDayTotals()
    {
        LastDayProducedTotal = 0;
        LastDayClaimedDungeonOutputsTotal = 0;
        LastDayTradedTotal = 0;
        LastDayProcessedTotal = 0;
        LastDayConsumedTotal = 0;
        LastDayCriticalFulfilledTotal = 0;
        LastDayCriticalUnmetTotal = 0;
        LastDayNormalFulfilledTotal = 0;
        LastDayNormalUnmetTotal = 0;
        LastDayFulfilledTotal = 0;
        LastDayUnmetTotal = 0;
        LastDayShortagesTotal = 0;
        LastDayProcessingBlockedTotal = 0;
        LastDayProcessingReservedTotal = 0;
        LastDayRouteCapacityUsedTotal = 0;
        PrepareLastDaySummaries();
        _lastDayTradeEvents.Clear();
        _lastDayClaimEvents.Clear();

        List<string> routeIds = new List<string>(_lastDayRouteUsageByRouteId.Keys);
        for (int i = 0; i < routeIds.Count; i++)
        {
            _lastDayRouteUsageByRouteId[routeIds[i]] = 0;
        }
    }

    private void RunLocalProductionPhase(WorldEntityData[] entities)
    {
        if (entities == null)
        {
            return;
        }

        for (int i = 0; i < entities.Length; i++)
        {
            WorldEntityData entity = entities[i];
            if (entity == null)
            {
                continue;
            }

            if (entity.Kind == WorldEntityKind.City)
            {
                ProduceResources(entity.Id, entity.SupplyResourceIds);
            }
            else if (entity.Kind == WorldEntityKind.Dungeon)
            {
                ProduceResources(entity.Id, entity.OutputResourceIds);
            }
        }
    }

    private void RunDungeonClaimPhase(WorldEntityData[] entities)
    {
        if (entities == null)
        {
            return;
        }

        for (int i = 0; i < entities.Length; i++)
        {
            WorldEntityData dungeon = entities[i];
            if (dungeon == null || dungeon.Kind != WorldEntityKind.Dungeon || string.IsNullOrEmpty(dungeon.LinkedCityId))
            {
                continue;
            }

            if (!TryGetEntity(dungeon.LinkedCityId, out WorldEntityData linkedCity) || linkedCity.Kind != WorldEntityKind.City)
            {
                continue;
            }

            for (int outputIndex = 0; outputIndex < dungeon.OutputResourceIds.Length; outputIndex++)
            {
                string resourceId = dungeon.OutputResourceIds[outputIndex];
                if (string.IsNullOrEmpty(resourceId))
                {
                    continue;
                }

                int currentStock = GetStock(dungeon.Id, resourceId);
                if (currentStock < 1)
                {
                    continue;
                }

                SetStock(dungeon.Id, resourceId, currentStock - 1);
                SetStock(linkedCity.Id, resourceId, GetStock(linkedCity.Id, resourceId) + 1);
                LastDayClaimedDungeonOutputsTotal += 1;
                GetEntityDaySummary(linkedCity.Id).DungeonImported += 1;
                GetEntityDaySummary(dungeon.Id).ClaimedOut += 1;
                GetResourceDaySummary(linkedCity.Id, resourceId).DungeonImported += 1;
                GetResourceDaySummary(dungeon.Id, resourceId).ClaimedOut += 1;
                _lastDayClaimEvents.Add(new ClaimEvent(dungeon.Id, linkedCity.Id, resourceId));
            }
        }
    }

    private void RunDirectTradePhase(DirectTradeScanResult tradeScanResult)
    {
        if (tradeScanResult == null)
        {
            return;
        }

        List<DirectTradeOpportunityData> prioritizedOpportunities = BuildPrioritizedTradeOpportunities(tradeScanResult.Opportunities);
        for (int i = 0; i < prioritizedOpportunities.Count; i++)
        {
            DirectTradeOpportunityData opportunity = prioritizedOpportunities[i];
            if (opportunity == null || string.IsNullOrEmpty(opportunity.ResourceId) || !TryGetRoute(opportunity.RouteId, out WorldRouteData route))
            {
                continue;
            }

            int currentUsage = GetLastDayRouteUsage(route.Id);
            if (currentUsage >= route.CapacityPerDay || !CanExportResourceForTrade(opportunity.SupplierEntityId, opportunity.ResourceId))
            {
                continue;
            }

            int supplierStock = GetStock(opportunity.SupplierEntityId, opportunity.ResourceId);
            if (supplierStock < 1)
            {
                continue;
            }

            SetStock(opportunity.SupplierEntityId, opportunity.ResourceId, supplierStock - 1);
            SetStock(opportunity.ConsumerEntityId, opportunity.ResourceId, GetStock(opportunity.ConsumerEntityId, opportunity.ResourceId) + 1);
            _lastDayRouteUsageByRouteId[route.Id] = currentUsage + 1;
            LastDayRouteCapacityUsedTotal += 1;
            LastDayTradedTotal += 1;
            GetEntityDaySummary(opportunity.SupplierEntityId).Exported += 1;
            GetEntityDaySummary(opportunity.ConsumerEntityId).Imported += 1;
            GetResourceDaySummary(opportunity.SupplierEntityId, opportunity.ResourceId).Exported += 1;
            GetResourceDaySummary(opportunity.ConsumerEntityId, opportunity.ResourceId).Imported += 1;
            _lastDayTradeEvents.Add(new TradeEvent(opportunity.SupplierEntityId, opportunity.ConsumerEntityId, opportunity.ResourceId));
        }
    }

    private List<DirectTradeOpportunityData> BuildPrioritizedTradeOpportunities(DirectTradeOpportunityData[] opportunities)
    {
        List<DirectTradeOpportunityData> prioritized = new List<DirectTradeOpportunityData>();
        if (opportunities == null || opportunities.Length == 0)
        {
            return prioritized;
        }

        for (int i = 0; i < opportunities.Length; i++)
        {
            if (opportunities[i] != null)
            {
                prioritized.Add(opportunities[i]);
            }
        }

        prioritized.Sort(CompareTradeOpportunities);
        return prioritized;
    }

    private int CompareTradeOpportunities(DirectTradeOpportunityData left, DirectTradeOpportunityData right)
    {
        int leftPriority = GetTradePriority(left);
        int rightPriority = GetTradePriority(right);
        if (leftPriority != rightPriority)
        {
            return rightPriority.CompareTo(leftPriority);
        }

        int leftConsumerStock = left != null ? GetStock(left.ConsumerEntityId, left.ResourceId) : 0;
        int rightConsumerStock = right != null ? GetStock(right.ConsumerEntityId, right.ResourceId) : 0;
        if (leftConsumerStock != rightConsumerStock)
        {
            return leftConsumerStock.CompareTo(rightConsumerStock);
        }

        int leftExportable = left != null ? GetExportableAmount(left.SupplierEntityId, left.ResourceId) : 0;
        int rightExportable = right != null ? GetExportableAmount(right.SupplierEntityId, right.ResourceId) : 0;
        if (leftExportable != rightExportable)
        {
            return rightExportable.CompareTo(leftExportable);
        }

        string leftRouteId = left != null ? left.RouteId : string.Empty;
        string rightRouteId = right != null ? right.RouteId : string.Empty;
        int routeCompare = string.CompareOrdinal(leftRouteId, rightRouteId);
        if (routeCompare != 0)
        {
            return routeCompare;
        }

        string leftConsumerId = left != null ? left.ConsumerEntityId : string.Empty;
        string rightConsumerId = right != null ? right.ConsumerEntityId : string.Empty;
        int consumerCompare = string.CompareOrdinal(leftConsumerId, rightConsumerId);
        if (consumerCompare != 0)
        {
            return consumerCompare;
        }

        string leftSupplierId = left != null ? left.SupplierEntityId : string.Empty;
        string rightSupplierId = right != null ? right.SupplierEntityId : string.Empty;
        int supplierCompare = string.CompareOrdinal(leftSupplierId, rightSupplierId);
        if (supplierCompare != 0)
        {
            return supplierCompare;
        }

        string leftResourceId = left != null ? left.ResourceId : string.Empty;
        string rightResourceId = right != null ? right.ResourceId : string.Empty;
        return string.CompareOrdinal(leftResourceId, rightResourceId);
    }

    private int GetTradePriority(DirectTradeOpportunityData opportunity)
    {
        if (opportunity == null || string.IsNullOrEmpty(opportunity.ConsumerEntityId) || string.IsNullOrEmpty(opportunity.ResourceId))
        {
            return int.MinValue;
        }

        int priority = 0;
        if (IsHighPriorityNeedResource(opportunity.ConsumerEntityId, opportunity.ResourceId))
        {
            priority += 125;
        }
        else if (IsDirectNeedResource(opportunity.ConsumerEntityId, opportunity.ResourceId))
        {
            priority += 100;
        }

        if (GetStock(opportunity.ConsumerEntityId, opportunity.ResourceId) < 1)
        {
            priority += 20;
        }

        if (GetLastDayUnmetCount(opportunity.ConsumerEntityId, opportunity.ResourceId) > 0)
        {
            priority += 10;
        }

        if (IsProcessingInputResource(opportunity.ConsumerEntityId, opportunity.ResourceId))
        {
            priority += 5;
        }

        return priority;
    }

    private bool IsDirectNeedResource(string entityId, string resourceId)
    {
        if (!TryGetEntity(entityId, out WorldEntityData entity) || entity.Kind != WorldEntityKind.City)
        {
            return false;
        }

        return ContainsResourceId(entity.NeedResourceIds, resourceId);
    }

    private bool IsHighPriorityNeedResource(string entityId, string resourceId)
    {
        if (!TryGetEntity(entityId, out WorldEntityData entity) || entity.Kind != WorldEntityKind.City)
        {
            return false;
        }

        return IsHighPriorityNeedResource(entity, resourceId);
    }

    private bool IsHighPriorityNeedResource(WorldEntityData entity, string resourceId)
    {
        return entity != null && entity.Kind == WorldEntityKind.City && ContainsResourceId(entity.HighPriorityNeedResourceIds, resourceId);
    }

    private bool IsProcessingInputResource(string entityId, string resourceId)
    {
        if (!TryGetEntity(entityId, out WorldEntityData entity) || entity.Kind != WorldEntityKind.City || entity.ProcessingRules == null)
        {
            return false;
        }

        for (int i = 0; i < entity.ProcessingRules.Length; i++)
        {
            LocalProcessingRuleData rule = entity.ProcessingRules[i];
            if (rule != null && rule.InputResourceId == resourceId)
            {
                return true;
            }
        }

        return false;
    }

    private void RunLocalProcessingPhase(WorldEntityData[] entities)
    {
        if (entities == null)
        {
            return;
        }

        for (int i = 0; i < entities.Length; i++)
        {
            WorldEntityData entity = entities[i];
            if (entity == null || entity.Kind != WorldEntityKind.City || entity.ProcessingRules == null)
            {
                continue;
            }

            EntityDaySummary summary = GetEntityDaySummary(entity.Id);
            for (int ruleIndex = 0; ruleIndex < entity.ProcessingRules.Length; ruleIndex++)
            {
                LocalProcessingRuleData rule = entity.ProcessingRules[ruleIndex];
                if (rule == null || string.IsNullOrEmpty(rule.InputResourceId) || string.IsNullOrEmpty(rule.OutputResourceId))
                {
                    continue;
                }

                int runLimit = rule.MaxRunsPerDay > 0 ? rule.MaxRunsPerDay : 1;
                for (int runIndex = 0; runIndex < runLimit; runIndex++)
                {
                    int inputStock = GetStock(entity.Id, rule.InputResourceId);
                    if (inputStock < 1)
                    {
                        LastDayProcessingBlockedTotal += 1;
                        summary.ProcessingBlocked += 1;
                        break;
                    }

                    SetStock(entity.Id, rule.InputResourceId, inputStock - 1);
                    SetStock(entity.Id, rule.OutputResourceId, GetStock(entity.Id, rule.OutputResourceId) + 1);
                    LastDayProcessedTotal += 1;
                    LastDayProcessingReservedTotal += 1;
                    summary.ProcessedIn += 1;
                    summary.ProcessedOut += 1;
                    summary.ProcessedTotal += 1;
                    summary.ProcessingReserved += 1;
                    GetResourceDaySummary(entity.Id, rule.InputResourceId).ProcessedIn += 1;
                    GetResourceDaySummary(entity.Id, rule.OutputResourceId).ProcessedOut += 1;
                }
            }
        }
    }

    private void RunLocalConsumptionPhase(WorldEntityData[] entities)
    {
        if (entities == null)
        {
            return;
        }

        for (int i = 0; i < entities.Length; i++)
        {
            WorldEntityData entity = entities[i];
            if (entity == null || entity.Kind != WorldEntityKind.City)
            {
                continue;
            }

            List<string> orderedNeeds = BuildOrderedNeedResourceIds(entity);
            for (int needIndex = 0; needIndex < orderedNeeds.Count; needIndex++)
            {
                string resourceId = orderedNeeds[needIndex];
                if (string.IsNullOrEmpty(resourceId))
                {
                    continue;
                }

                bool isCriticalNeed = IsHighPriorityNeedResource(entity, resourceId);
                int currentStock = GetStock(entity.Id, resourceId);
                if (currentStock > 0)
                {
                    SetStock(entity.Id, resourceId, currentStock - 1);
                    RecordNeedFulfilled(entity.Id, resourceId, isCriticalNeed);
                }
                else
                {
                    RecordNeedUnmet(entity.Id, resourceId, isCriticalNeed);
                }
            }
        }
    }

    private void ApplyEndOfDayShortagePressure(DirectTradeScanResult tradeScanResult)
    {
        if (tradeScanResult == null || tradeScanResult.UnmetCityNeeds == null)
        {
            return;
        }

        for (int i = 0; i < tradeScanResult.UnmetCityNeeds.Length; i++)
        {
            UnmetCityNeedData unmetNeed = tradeScanResult.UnmetCityNeeds[i];
            if (unmetNeed == null || string.IsNullOrEmpty(unmetNeed.CityEntityId) || string.IsNullOrEmpty(unmetNeed.ResourceId))
            {
                continue;
            }

            if (!TryGetEntity(unmetNeed.CityEntityId, out WorldEntityData city) || city.Kind != WorldEntityKind.City)
            {
                continue;
            }

            if (GetStock(unmetNeed.CityEntityId, unmetNeed.ResourceId) > 0)
            {
                continue;
            }

            if (GetLastDayShortageCount(unmetNeed.CityEntityId, unmetNeed.ResourceId) > 0)
            {
                continue;
            }

            if (GetResourceDaySummary(unmetNeed.CityEntityId, unmetNeed.ResourceId).Consumed > 0)
            {
                continue;
            }

            RecordNeedUnmet(unmetNeed.CityEntityId, unmetNeed.ResourceId, IsHighPriorityNeedResource(city, unmetNeed.ResourceId));
        }
    }

    private void RefreshCurrentTradeScanResult()
    {
        _currentTradeScanResult = DirectTradeScanner.Scan(_worldData, GetStockAmount, CanExportResourceForTrade);
    }

    private void PrepareLastDaySummaries()
    {
        _lastDayEntitySummaries.Clear();
        _lastDayResourceSummariesByEntityId.Clear();

        foreach (KeyValuePair<string, WorldEntityData> pair in _entityById)
        {
            if (_lastDayEntitySummaries.ContainsKey(pair.Key))
            {
                continue;
            }

            _lastDayEntitySummaries.Add(pair.Key, new EntityDaySummary());
            _lastDayResourceSummariesByEntityId.Add(pair.Key, new Dictionary<string, ResourceDaySummary>());
        }
    }

    private List<string> BuildOrderedNeedResourceIds(WorldEntityData entity)
    {
        List<string> ordered = new List<string>();
        if (entity == null || entity.Kind != WorldEntityKind.City)
        {
            return ordered;
        }

        AddOrderedNeedResourceIds(ordered, entity.HighPriorityNeedResourceIds, entity.NeedResourceIds);
        AddOrderedNeedResourceIds(ordered, entity.NeedResourceIds, entity.NeedResourceIds);
        return ordered;
    }

    private void AddOrderedNeedResourceIds(List<string> orderedNeeds, string[] sourceResourceIds, string[] allowedNeedIds)
    {
        if (orderedNeeds == null || sourceResourceIds == null || allowedNeedIds == null)
        {
            return;
        }

        for (int i = 0; i < sourceResourceIds.Length; i++)
        {
            string resourceId = sourceResourceIds[i];
            if (string.IsNullOrEmpty(resourceId) || orderedNeeds.Contains(resourceId) || !ContainsResourceId(allowedNeedIds, resourceId))
            {
                continue;
            }

            orderedNeeds.Add(resourceId);
        }
    }

    private void RecordNeedFulfilled(string entityId, string resourceId, bool isCriticalNeed)
    {
        if (string.IsNullOrEmpty(entityId) || string.IsNullOrEmpty(resourceId))
        {
            return;
        }

        LastDayConsumedTotal += 1;
        LastDayFulfilledTotal += 1;

        EntityDaySummary entitySummary = GetEntityDaySummary(entityId);
        entitySummary.Consumed += 1;
        entitySummary.Fulfilled += 1;

        if (isCriticalNeed)
        {
            LastDayCriticalFulfilledTotal += 1;
            entitySummary.CriticalFulfilled += 1;
        }
        else
        {
            LastDayNormalFulfilledTotal += 1;
            entitySummary.NormalFulfilled += 1;
        }

        ResourceDaySummary resourceSummary = GetResourceDaySummary(entityId, resourceId);
        resourceSummary.Consumed += 1;
        resourceSummary.Fulfilled += 1;
        _totalFulfilledByEntityId[entityId] = GetTotalFulfilled(entityId) + 1;
    }

    private void RecordNeedUnmet(string entityId, string resourceId, bool isCriticalNeed)
    {
        if (string.IsNullOrEmpty(entityId) || string.IsNullOrEmpty(resourceId))
        {
            return;
        }

        LastDayUnmetTotal += 1;
        LastDayShortagesTotal += 1;

        EntityDaySummary entitySummary = GetEntityDaySummary(entityId);
        entitySummary.Unmet += 1;
        entitySummary.Shortages += 1;

        if (isCriticalNeed)
        {
            LastDayCriticalUnmetTotal += 1;
            entitySummary.CriticalUnmet += 1;
            _totalCriticalUnmetByEntityId[entityId] = GetTotalCriticalUnmet(entityId) + 1;
        }
        else
        {
            LastDayNormalUnmetTotal += 1;
            entitySummary.NormalUnmet += 1;
            _totalNormalUnmetByEntityId[entityId] = GetTotalNormalUnmet(entityId) + 1;
        }

        ResourceDaySummary resourceSummary = GetResourceDaySummary(entityId, resourceId);
        resourceSummary.Unmet += 1;
        resourceSummary.Shortages += 1;
        _totalUnmetByEntityId[entityId] = GetTotalUnmet(entityId) + 1;
        _totalShortagesByEntityId[entityId] = GetTotalShortages(entityId) + 1;
    }

    private void ProduceResources(string entityId, string[] resourceIds)
    {
        if (string.IsNullOrEmpty(entityId) || resourceIds == null)
        {
            return;
        }

        EntityDaySummary summary = GetEntityDaySummary(entityId);
        for (int i = 0; i < resourceIds.Length; i++)
        {
            string resourceId = resourceIds[i];
            if (string.IsNullOrEmpty(resourceId))
            {
                continue;
            }

            SetStock(entityId, resourceId, GetStock(entityId, resourceId) + 1);
            LastDayProducedTotal += 1;
            summary.Produced += 1;
            GetResourceDaySummary(entityId, resourceId).Produced += 1;
        }
    }

    private void RunExpeditionPhase()
    {
        if (_parties.Count == 0)
        {
            return;
        }

        for (int i = 0; i < _parties.Count; i++)
        {
            PartyRuntimeData party = _parties[i];
            if (party == null || party.State != PartyState.OnExpedition)
            {
                continue;
            }

            if (party.DaysRemaining > 0)
            {
                party.DaysRemaining -= 1;
            }

            if (party.DaysRemaining > 0)
            {
                party.LastResultSummary = BuildActivePartyStatus(party);
                _lastExpeditionResultByCityId[party.HomeCityId] = party.LastResultSummary;
                _lastExpeditionResultByDungeonId[party.TargetDungeonId] = party.LastResultSummary;
                continue;
            }

            ResolveExpedition(party);
        }
    }

    private void ResolveExpedition(PartyRuntimeData party)
    {
        if (party == null)
        {
            return;
        }

        string homeCityId = party.HomeCityId;
        string dungeonId = party.TargetDungeonId;
        TryGetEntity(homeCityId, out WorldEntityData homeCity);
        TryGetEntity(dungeonId, out WorldEntityData dungeon);
        TryGetDungeonProfile(dungeonId, out DungeonExpeditionProfile profile);

        bool success = profile != null && party.Power >= profile.RecommendedPower;
        int lootReturned = success && homeCity != null ? GrantExpeditionLoot(homeCity.Id, profile, party.CarryCapacity) : 0;
        string dungeonName = dungeon != null && !string.IsNullOrEmpty(dungeon.DisplayName) ? dungeon.DisplayName : (string.IsNullOrEmpty(dungeonId) ? "Unknown Dungeon" : dungeonId);
        string cityName = homeCity != null && !string.IsNullOrEmpty(homeCity.DisplayName) ? homeCity.DisplayName : (string.IsNullOrEmpty(homeCityId) ? "Unknown City" : homeCityId);
        string resultSummary = success
            ? party.PartyId + " returned to " + cityName + " with " + BuildRewardPreview(profile, lootReturned)
            : party.PartyId + " returned to " + cityName + " after failing " + dungeonName;

        if (success)
        {
            ExpeditionSuccessCount += 1;
            ExpeditionLootReturnedTotal += lootReturned;
            _expeditionLootReturnedByCityId[homeCityId] = GetExpeditionLootReturnedTotalForCity(homeCityId) + lootReturned;
        }
        else
        {
            ExpeditionFailureCount += 1;
        }

        party.State = PartyState.Idle;
        party.TargetDungeonId = string.Empty;
        party.DaysRemaining = 0;
        party.LastResultSummary = resultSummary;
        _lastExpeditionResultByCityId[homeCityId] = resultSummary;
        _lastExpeditionResultByDungeonId[dungeonId] = resultSummary;
        AppendRecentExpeditionLog("Day " + WorldDayCount + " | " + resultSummary);
    }

    public void ResolveDungeonRun(string cityId, string dungeonId, string rewardResourceId, int lootReturned, bool success, string resultSummary, string survivingMembersSummary = null, string clearedEncounterSummary = null, string eventChoiceSummary = null, string lootBreakdownSummary = null, string routeSummary = null)
    {
        PartyRuntimeData party = FindActivePartyForCity(cityId);
        if (party == null)
        {
            party = FindActivePartyForDungeon(dungeonId);
        }

        if (party == null)
        {
            return;
        }

        string safeResultSummary = string.IsNullOrEmpty(resultSummary) ? party.PartyId + " returned." : resultSummary;
        int safeLootReturned = success && lootReturned > 0 ? lootReturned : 0;
        string lootSummary = success && !string.IsNullOrEmpty(rewardResourceId) && safeLootReturned > 0
            ? rewardResourceId + " x" + safeLootReturned
            : "None";

        if (success)
        {
            ExpeditionSuccessCount += 1;
            ExpeditionLootReturnedTotal += safeLootReturned;
            _expeditionLootReturnedByCityId[cityId] = GetExpeditionLootReturnedTotalForCity(cityId) + safeLootReturned;

            if (!string.IsNullOrEmpty(rewardResourceId) && safeLootReturned > 0)
            {
                SetStock(cityId, rewardResourceId, GetStock(cityId, rewardResourceId) + safeLootReturned);
            }
        }
        else
        {
            ExpeditionFailureCount += 1;
        }

        _lastRunLootSummaryByCityId[cityId] = lootSummary;
        _lastRunSurvivingMembersByCityId[cityId] = string.IsNullOrEmpty(survivingMembersSummary) ? "None" : survivingMembersSummary;
        _lastRunClearedEncountersByCityId[cityId] = string.IsNullOrEmpty(clearedEncounterSummary) ? "None" : clearedEncounterSummary;
        _lastRunEventChoiceByCityId[cityId] = string.IsNullOrEmpty(eventChoiceSummary) ? "None" : eventChoiceSummary;
        _lastRunLootBreakdownByCityId[cityId] = string.IsNullOrEmpty(lootBreakdownSummary) ? "None" : lootBreakdownSummary;
        _lastRunRouteByCityId[cityId] = string.IsNullOrEmpty(routeSummary) ? "None" : routeSummary;
        _lastRunDungeonByCityId[cityId] = !string.IsNullOrEmpty(dungeonId) && TryGetEntity(dungeonId, out WorldEntityData resolvedDungeon)
            ? resolvedDungeon.DisplayName
            : "None";
        party.State = PartyState.Idle;
        party.TargetDungeonId = string.Empty;
        party.DaysRemaining = 0;
        party.LastResultSummary = safeResultSummary;
        _lastExpeditionResultByCityId[cityId] = safeResultSummary;

        if (!string.IsNullOrEmpty(dungeonId))
        {
            _lastExpeditionResultByDungeonId[dungeonId] = safeResultSummary;
        }

        AppendRecentExpeditionLog("Day " + WorldDayCount + " | " + safeResultSummary);
    }

    private int GrantExpeditionLoot(string cityId, DungeonExpeditionProfile profile, int carryCapacity)
    {
        if (string.IsNullOrEmpty(cityId) || profile == null || profile.RewardResourceIds == null || profile.RewardResourceIds.Length == 0 || carryCapacity < 1)
        {
            return 0;
        }

        int granted = 0;
        for (int i = 0; i < carryCapacity; i++)
        {
            string resourceId = profile.RewardResourceIds[i % profile.RewardResourceIds.Length];
            if (string.IsNullOrEmpty(resourceId))
            {
                continue;
            }

            SetStock(cityId, resourceId, GetStock(cityId, resourceId) + 1);
            granted += 1;
        }

        return granted;
    }

    private string BuildContractStatusText(ExpeditionContractData contract)
    {
        if (contract == null || !TryGetEntity(contract.CityId, out WorldEntityData city) || !TryGetEntity(contract.DungeonId, out WorldEntityData dungeon) || !TryGetDungeonProfile(contract.DungeonId, out DungeonExpeditionProfile profile))
        {
            return "None";
        }

        int active = CountActiveExpeditionsForDungeonInternal(contract.DungeonId);
        string stateText = active < profile.MaxActiveParties
            ? "Open " + active + "/" + profile.MaxActiveParties
            : "Full " + active + "/" + profile.MaxActiveParties;
        return city.DisplayName + " -> " + dungeon.DisplayName + " (" + stateText + ")";
    }

    private string BuildActivePartyStatus(PartyRuntimeData party)
    {
        if (party == null)
        {
            return "None";
        }

        string dungeonName = TryGetEntity(party.TargetDungeonId, out WorldEntityData dungeon) && !string.IsNullOrEmpty(dungeon.DisplayName)
            ? dungeon.DisplayName
            : (string.IsNullOrEmpty(party.TargetDungeonId) ? "Unknown Dungeon" : party.TargetDungeonId);
        return party.PartyId + " -> " + dungeonName + " (" + party.DaysRemaining + "d remaining)";
    }

    private string BuildRewardPreview(DungeonExpeditionProfile profile, int carryCapacity)
    {
        if (profile == null || profile.RewardResourceIds == null || profile.RewardResourceIds.Length == 0 || carryCapacity < 1)
        {
            return "None";
        }

        Dictionary<string, int> countsByResourceId = new Dictionary<string, int>();
        List<string> orderedResourceIds = new List<string>();
        for (int i = 0; i < carryCapacity; i++)
        {
            string resourceId = profile.RewardResourceIds[i % profile.RewardResourceIds.Length];
            if (string.IsNullOrEmpty(resourceId))
            {
                continue;
            }

            if (!countsByResourceId.ContainsKey(resourceId))
            {
                countsByResourceId.Add(resourceId, 0);
                orderedResourceIds.Add(resourceId);
            }

            countsByResourceId[resourceId] += 1;
        }

        string text = string.Empty;
        for (int i = 0; i < orderedResourceIds.Count; i++)
        {
            string resourceId = orderedResourceIds[i];
            string segment = resourceId + " x" + countsByResourceId[resourceId];
            text = string.IsNullOrEmpty(text) ? segment : text + ", " + segment;
        }

        return string.IsNullOrEmpty(text) ? "None" : text;
    }

    private ExpeditionContractData GetContractForCity(string cityId)
    {
        if (string.IsNullOrEmpty(cityId))
        {
            return null;
        }

        for (int i = 0; i < _expeditionContracts.Count; i++)
        {
            ExpeditionContractData contract = _expeditionContracts[i];
            if (contract != null && contract.CityId == cityId)
            {
                return contract;
            }
        }

        return null;
    }

    private ExpeditionContractData GetContractForDungeon(string dungeonId)
    {
        if (string.IsNullOrEmpty(dungeonId))
        {
            return null;
        }

        for (int i = 0; i < _expeditionContracts.Count; i++)
        {
            ExpeditionContractData contract = _expeditionContracts[i];
            if (contract != null && contract.DungeonId == dungeonId)
            {
                return contract;
            }
        }

        return null;
    }

    private PartyRuntimeData FindAnyPartyByHomeCity(string cityId)
    {
        if (string.IsNullOrEmpty(cityId))
        {
            return null;
        }

        for (int i = 0; i < _parties.Count; i++)
        {
            PartyRuntimeData party = _parties[i];
            if (party != null && party.HomeCityId == cityId)
            {
                return party;
            }
        }

        return null;
    }

    private PartyRuntimeData FindIdleParty(string cityId)
    {
        if (string.IsNullOrEmpty(cityId))
        {
            return null;
        }

        for (int i = 0; i < _parties.Count; i++)
        {
            PartyRuntimeData party = _parties[i];
            if (party != null && party.HomeCityId == cityId && party.State == PartyState.Idle)
            {
                return party;
            }
        }

        return null;
    }

    private PartyRuntimeData FindActivePartyForCity(string cityId)
    {
        if (string.IsNullOrEmpty(cityId))
        {
            return null;
        }

        for (int i = 0; i < _parties.Count; i++)
        {
            PartyRuntimeData party = _parties[i];
            if (party != null && party.HomeCityId == cityId && party.State == PartyState.OnExpedition)
            {
                return party;
            }
        }

        return null;
    }

    private PartyRuntimeData FindActivePartyForDungeon(string dungeonId)
    {
        if (string.IsNullOrEmpty(dungeonId))
        {
            return null;
        }

        for (int i = 0; i < _parties.Count; i++)
        {
            PartyRuntimeData party = _parties[i];
            if (party != null && party.TargetDungeonId == dungeonId && party.State == PartyState.OnExpedition)
            {
                return party;
            }
        }

        return null;
    }

    private int CountIdleParties()
    {
        int count = 0;
        for (int i = 0; i < _parties.Count; i++)
        {
            PartyRuntimeData party = _parties[i];
            if (party != null && party.State == PartyState.Idle)
            {
                count += 1;
            }
        }

        return count;
    }

    private int CountActiveExpeditions()
    {
        int count = 0;
        for (int i = 0; i < _parties.Count; i++)
        {
            PartyRuntimeData party = _parties[i];
            if (party != null && party.State == PartyState.OnExpedition)
            {
                count += 1;
            }
        }

        return count;
    }

    private int CountAvailableContracts()
    {
        int count = 0;
        for (int i = 0; i < _expeditionContracts.Count; i++)
        {
            ExpeditionContractData contract = _expeditionContracts[i];
            if (contract == null || !TryGetDungeonProfile(contract.DungeonId, out DungeonExpeditionProfile profile))
            {
                continue;
            }

            if (CountActiveExpeditionsForDungeonInternal(contract.DungeonId) < profile.MaxActiveParties)
            {
                count += 1;
            }
        }

        return count;
    }

    private int CountActiveExpeditionsForDungeonInternal(string dungeonId)
    {
        if (string.IsNullOrEmpty(dungeonId))
        {
            return 0;
        }

        int count = 0;
        for (int i = 0; i < _parties.Count; i++)
        {
            PartyRuntimeData party = _parties[i];
            if (party != null && party.TargetDungeonId == dungeonId && party.State == PartyState.OnExpedition)
            {
                count += 1;
            }
        }

        return count;
    }

    private bool TryGetDungeonProfile(string dungeonId, out DungeonExpeditionProfile profile)
    {
        if (!string.IsNullOrEmpty(dungeonId) && _dungeonExpeditionProfilesByDungeonId.TryGetValue(dungeonId, out profile))
        {
            return true;
        }

        profile = null;
        return false;
    }

    private static Dictionary<string, DungeonExpeditionProfile> BuildDungeonExpeditionProfiles(WorldData worldData)
    {
        Dictionary<string, DungeonExpeditionProfile> profiles = new Dictionary<string, DungeonExpeditionProfile>();
        if (worldData == null || worldData.Entities == null)
        {
            return profiles;
        }

        for (int i = 0; i < worldData.Entities.Length; i++)
        {
            WorldEntityData entity = worldData.Entities[i];
            if (entity == null || entity.Kind != WorldEntityKind.Dungeon || string.IsNullOrEmpty(entity.Id) || profiles.ContainsKey(entity.Id))
            {
                continue;
            }

            string[] rewards = entity.OutputResourceIds != null && entity.OutputResourceIds.Length > 0
                ? entity.OutputResourceIds
                : new[] { "mana_shard" };
            int recommendedPower = entity.PrimaryStatValue > 0 ? entity.PrimaryStatValue : 1;
            profiles.Add(entity.Id, new DungeonExpeditionProfile(entity.Id, recommendedPower, 2, rewards, 1));
        }

        return profiles;
    }

    private static List<ExpeditionContractData> BuildExpeditionContracts(WorldData worldData, Dictionary<string, DungeonExpeditionProfile> profiles)
    {
        List<ExpeditionContractData> contracts = new List<ExpeditionContractData>();
        if (worldData == null || worldData.Entities == null || profiles == null)
        {
            return contracts;
        }

        for (int i = 0; i < worldData.Entities.Length; i++)
        {
            WorldEntityData entity = worldData.Entities[i];
            if (entity == null || entity.Kind != WorldEntityKind.Dungeon || string.IsNullOrEmpty(entity.LinkedCityId) || !profiles.ContainsKey(entity.Id))
            {
                continue;
            }

            contracts.Add(new ExpeditionContractData(entity.LinkedCityId, entity.Id));
        }

        return contracts;
    }

    private int CalculateCurrentUnclaimedDungeonOutputsTotal()
    {
        int total = 0;
        WorldEntityData[] entities = _worldData.Entities ?? System.Array.Empty<WorldEntityData>();
        for (int i = 0; i < entities.Length; i++)
        {
            WorldEntityData entity = entities[i];
            if (entity == null || entity.Kind != WorldEntityKind.Dungeon)
            {
                continue;
            }

            for (int outputIndex = 0; outputIndex < entity.OutputResourceIds.Length; outputIndex++)
            {
                string resourceId = entity.OutputResourceIds[outputIndex];
                if (!string.IsNullOrEmpty(resourceId))
                {
                    total += GetStock(entity.Id, resourceId);
                }
            }
        }

        return total;
    }

    private string BuildRecentDayLog()
    {
        return "Day " + WorldDayCount +
            " | Prod " + LastDayProducedTotal +
            " | Claim " + LastDayClaimedDungeonOutputsTotal +
            " | Trade " + LastDayTradedTotal +
            " | Route " + LastDayRouteCapacityUsedTotal + "/" + GetTotalRouteCapacityPerDay() + " (Sat " + CountSaturatedRoutes() + ")" +
            " | Proc " + LastDayProcessedTotal +
            " | Block " + LastDayProcessingBlockedTotal +
            " | PReserve " + LastDayProcessingReservedTotal +
            " | Crit " + LastDayCriticalFulfilledTotal + "/" + LastDayCriticalUnmetTotal +
            " | Norm " + LastDayNormalFulfilledTotal + "/" + LastDayNormalUnmetTotal +
            " | Fulfill " + LastDayFulfilledTotal +
            " | Unmet " + LastDayUnmetTotal +
            " | Short " + LastDayShortagesTotal + " (" + CountCitiesWithLastDayShortages() + " city)" +
            " | Unclaimed " + CurrentUnclaimedDungeonOutputsTotal;
    }

    private int CountCitiesWithLastDayShortages()
    {
        int count = 0;
        WorldEntityData[] entities = _worldData.Entities ?? System.Array.Empty<WorldEntityData>();
        for (int i = 0; i < entities.Length; i++)
        {
            WorldEntityData entity = entities[i];
            if (entity == null || entity.Kind != WorldEntityKind.City)
            {
                continue;
            }

            if (GetLastDayShortages(entity.Id) > 0)
            {
                count += 1;
            }
        }

        return count;
    }

    private int CountSaturatedRoutes()
    {
        int count = 0;
        foreach (KeyValuePair<string, WorldRouteData> pair in _routeById)
        {
            if (GetLastDayRouteUsage(pair.Key) >= pair.Value.CapacityPerDay)
            {
                count += 1;
            }
        }

        return count;
    }

    private int GetTotalRouteCapacityPerDay()
    {
        int total = 0;
        foreach (KeyValuePair<string, WorldRouteData> pair in _routeById)
        {
            total += pair.Value.CapacityPerDay;
        }

        return total;
    }

    private bool CanExportResourceForTrade(string entityId, string resourceId)
    {
        return GetExportableAmount(entityId, resourceId) > 0;
    }

    private int GetReserveFloor(WorldEntityData entity, string resourceId)
    {
        return IsReserveProtectedResource(entity, resourceId) ? ReserveStockFloor : 0;
    }

    private bool IsReserveProtectedResource(WorldEntityData entity, string resourceId)
    {
        if (entity == null || entity.Kind != WorldEntityKind.City || string.IsNullOrEmpty(resourceId))
        {
            return false;
        }

        if (ContainsResourceId(entity.SupplyResourceIds, resourceId))
        {
            return true;
        }

        if (entity.ProcessingRules == null)
        {
            return false;
        }

        for (int i = 0; i < entity.ProcessingRules.Length; i++)
        {
            LocalProcessingRuleData rule = entity.ProcessingRules[i];
            if (rule != null && rule.OutputResourceId == resourceId)
            {
                return true;
            }
        }

        return false;
    }

    private string FormatTradeEvent(TradeEvent tradeEvent)
    {
        if (tradeEvent == null || string.IsNullOrEmpty(tradeEvent.ResourceId))
        {
            return "None";
        }

        return GetEntityDisplayName(tradeEvent.SupplierEntityId) + " -> " + GetEntityDisplayName(tradeEvent.ConsumerEntityId) + " : " + tradeEvent.ResourceId;
    }

    private string FormatClaimEvent(ClaimEvent claimEvent)
    {
        if (claimEvent == null || string.IsNullOrEmpty(claimEvent.ResourceId))
        {
            return "None";
        }

        return GetEntityDisplayName(claimEvent.DungeonEntityId) + " -> " + GetEntityDisplayName(claimEvent.CityEntityId) + " : " + claimEvent.ResourceId;
    }

    private string GetEntityDisplayName(string entityId)
    {
        if (TryGetEntity(entityId, out WorldEntityData entity) && !string.IsNullOrEmpty(entity.DisplayName))
        {
            return entity.DisplayName;
        }

        return string.IsNullOrEmpty(entityId) ? "Unknown" : entityId;
    }

    private EntityDaySummary GetEntityDaySummary(string entityId)
    {
        if (string.IsNullOrEmpty(entityId))
        {
            return new EntityDaySummary();
        }

        if (!_lastDayEntitySummaries.TryGetValue(entityId, out EntityDaySummary summary))
        {
            summary = new EntityDaySummary();
            _lastDayEntitySummaries[entityId] = summary;
        }

        return summary;
    }

    private ResourceDaySummary GetResourceDaySummary(string entityId, string resourceId)
    {
        if (string.IsNullOrEmpty(entityId) || string.IsNullOrEmpty(resourceId))
        {
            return new ResourceDaySummary();
        }

        if (!_lastDayResourceSummariesByEntityId.TryGetValue(entityId, out Dictionary<string, ResourceDaySummary> resourceSummaries))
        {
            resourceSummaries = new Dictionary<string, ResourceDaySummary>();
            _lastDayResourceSummariesByEntityId[entityId] = resourceSummaries;
        }

        if (!resourceSummaries.TryGetValue(resourceId, out ResourceDaySummary summary))
        {
            summary = new ResourceDaySummary();
            resourceSummaries[resourceId] = summary;
        }

        return summary;
    }

    private bool TryGetEntity(string entityId, out WorldEntityData entity)
    {
        if (!string.IsNullOrEmpty(entityId) && _entityById.TryGetValue(entityId, out entity))
        {
            return true;
        }

        entity = null;
        return false;
    }

    private bool TryGetRoute(string routeId, out WorldRouteData route)
    {
        if (!string.IsNullOrEmpty(routeId) && _routeById.TryGetValue(routeId, out route))
        {
            return true;
        }

        route = null;
        return false;
    }

    private static int SumDictionaryValues(Dictionary<string, int> values)
    {
        int total = 0;
        if (values == null)
        {
            return total;
        }

        foreach (KeyValuePair<string, int> pair in values)
        {
            total += pair.Value;
        }

        return total;
    }

    private static int GetDictionaryValue(Dictionary<string, int> values, string key)
    {
        if (values == null || string.IsNullOrEmpty(key))
        {
            return 0;
        }

        return values.TryGetValue(key, out int value) ? value : 0;
    }

    private static bool ContainsResourceId(string[] resourceIds, string targetResourceId)
    {
        if (resourceIds == null || string.IsNullOrEmpty(targetResourceId))
        {
            return false;
        }

        for (int i = 0; i < resourceIds.Length; i++)
        {
            if (resourceIds[i] == targetResourceId)
            {
                return true;
            }
        }

        return false;
    }

    private static string[] BuildResourceIds(ResourceData[] resources)
    {
        if (resources == null || resources.Length == 0)
        {
            return System.Array.Empty<string>();
        }

        List<string> ids = new List<string>();
        for (int i = 0; i < resources.Length; i++)
        {
            ResourceData resource = resources[i];
            if (resource == null || string.IsNullOrEmpty(resource.Id) || ids.Contains(resource.Id))
            {
                continue;
            }

            ids.Add(resource.Id);
        }

        return ids.ToArray();
    }

    private static Dictionary<string, WorldEntityData> BuildEntityLookup(WorldData worldData)
    {
        Dictionary<string, WorldEntityData> lookup = new Dictionary<string, WorldEntityData>();
        if (worldData == null || worldData.Entities == null)
        {
            return lookup;
        }

        for (int i = 0; i < worldData.Entities.Length; i++)
        {
            WorldEntityData entity = worldData.Entities[i];
            if (entity == null || string.IsNullOrEmpty(entity.Id) || lookup.ContainsKey(entity.Id))
            {
                continue;
            }

            lookup.Add(entity.Id, entity);
        }

        return lookup;
    }

    private static Dictionary<string, WorldRouteData> BuildRouteLookup(WorldData worldData)
    {
        Dictionary<string, WorldRouteData> lookup = new Dictionary<string, WorldRouteData>();
        if (worldData == null || worldData.Routes == null)
        {
            return lookup;
        }

        for (int i = 0; i < worldData.Routes.Length; i++)
        {
            WorldRouteData route = worldData.Routes[i];
            if (route == null || string.IsNullOrEmpty(route.Id) || lookup.ContainsKey(route.Id))
            {
                continue;
            }

            lookup.Add(route.Id, route);
        }

        return lookup;
    }

    private static Dictionary<string, Dictionary<string, int>> BuildDefaultStocks(WorldData worldData, string[] resourceIds)
    {
        Dictionary<string, Dictionary<string, int>> defaults = new Dictionary<string, Dictionary<string, int>>();
        if (worldData != null && worldData.Entities != null)
        {
            for (int i = 0; i < worldData.Entities.Length; i++)
            {
                WorldEntityData entity = worldData.Entities[i];
                if (entity == null || string.IsNullOrEmpty(entity.Id) || defaults.ContainsKey(entity.Id))
                {
                    continue;
                }

                Dictionary<string, int> stockMap = new Dictionary<string, int>();
                for (int resourceIndex = 0; resourceIndex < resourceIds.Length; resourceIndex++)
                {
                    stockMap[resourceIds[resourceIndex]] = 0;
                }

                defaults.Add(entity.Id, stockMap);
            }
        }

        SetDefaultStock(defaults, "city-a", "grain", 3);
        SetDefaultStock(defaults, "city-a", "iron_ore", 0);
        SetDefaultStock(defaults, "city-a", "mana_shard", 0);
        SetDefaultStock(defaults, "city-a", "refined_mana", 0);
        SetDefaultStock(defaults, "city-b", "grain", 0);
        SetDefaultStock(defaults, "city-b", "iron_ore", 3);
        SetDefaultStock(defaults, "city-b", "mana_shard", 0);
        SetDefaultStock(defaults, "city-b", "refined_mana", 0);
        SetDefaultStock(defaults, "dungeon-alpha", "grain", 0);
        SetDefaultStock(defaults, "dungeon-alpha", "iron_ore", 0);
        SetDefaultStock(defaults, "dungeon-alpha", "mana_shard", 2);
        SetDefaultStock(defaults, "dungeon-alpha", "refined_mana", 0);
        SetDefaultStock(defaults, "dungeon-beta", "grain", 0);
        SetDefaultStock(defaults, "dungeon-beta", "iron_ore", 0);
        SetDefaultStock(defaults, "dungeon-beta", "mana_shard", 3);
        SetDefaultStock(defaults, "dungeon-beta", "refined_mana", 0);

        return defaults;
    }

    private static void SetDefaultStock(Dictionary<string, Dictionary<string, int>> defaults, string entityId, string resourceId, int value)
    {
        if (defaults == null || string.IsNullOrEmpty(entityId) || string.IsNullOrEmpty(resourceId))
        {
            return;
        }

        if (!defaults.TryGetValue(entityId, out Dictionary<string, int> stockMap))
        {
            stockMap = new Dictionary<string, int>();
            defaults[entityId] = stockMap;
        }

        stockMap[resourceId] = value;
    }

    private void SetStock(string entityId, string resourceId, int value)
    {
        if (string.IsNullOrEmpty(entityId) || string.IsNullOrEmpty(resourceId))
        {
            return;
        }

        if (!_stockByEntityId.TryGetValue(entityId, out Dictionary<string, int> stockMap))
        {
            stockMap = new Dictionary<string, int>();
            _stockByEntityId[entityId] = stockMap;
        }

        stockMap[resourceId] = value;
    }

    private int GetStock(string entityId, string resourceId)
    {
        if (string.IsNullOrEmpty(entityId) || string.IsNullOrEmpty(resourceId))
        {
            return 0;
        }

        if (_stockByEntityId.TryGetValue(entityId, out Dictionary<string, int> stockMap) && stockMap.TryGetValue(resourceId, out int value))
        {
            return value;
        }

        return 0;
    }

    private void AppendRecentDayLog(string logText)
    {
        if (string.IsNullOrEmpty(logText))
        {
            return;
        }

        _recentDayLogs.Insert(0, logText);
        while (_recentDayLogs.Count > RecentDayLogLimit)
        {
            _recentDayLogs.RemoveAt(_recentDayLogs.Count - 1);
        }
    }

    private void AppendRecentExpeditionLog(string logText)
    {
        if (string.IsNullOrEmpty(logText))
        {
            return;
        }

        _recentExpeditionLogs.Insert(0, logText);
        while (_recentExpeditionLogs.Count > RecentExpeditionLogLimit)
        {
            _recentExpeditionLogs.RemoveAt(_recentExpeditionLogs.Count - 1);
        }
    }
}







