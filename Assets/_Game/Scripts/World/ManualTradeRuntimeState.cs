using System;
using System.Collections.Generic;
using UnityEngine;

public sealed class ManualTradeRuntimeState
{
    private const float DefaultTickIntervalSeconds = 1.25f;
    private const int RecentDayLogLimit = 3;
    private const int RecentExpeditionLogLimit = 3;
    private const int RecentWorldWritebackLogLimit = 5;
    private const int ReserveStockFloor = 1;
    private const int DefaultPartyPower = 3;
    private const int DefaultPartyCarryCapacity = 2;

    private enum PartyState
    {
        Idle,
        OnExpedition
    }

    private sealed class PartyMemberProgressionData
    {
        public string MemberId;
        public string DisplayName;
        public string RoleTag;
        public int Level;
        public int Experience;
        public readonly Dictionary<string, string> EquippedItemInstanceIdBySlotKey;

        public PartyMemberProgressionData(string memberId, string displayName, string roleTag)
        {
            MemberId = string.IsNullOrWhiteSpace(memberId) ? string.Empty : memberId.Trim();
            DisplayName = string.IsNullOrWhiteSpace(displayName) ? "Adventurer" : displayName.Trim();
            RoleTag = string.IsNullOrWhiteSpace(roleTag) ? "adventurer" : roleTag.Trim().ToLowerInvariant();
            Level = PrototypeRpgMemberProgressionRules.GetStartingLevel();
            Experience = PrototypeRpgMemberProgressionRules.GetStartingExperience();
            EquippedItemInstanceIdBySlotKey = new Dictionary<string, string>();
        }
    }

    private sealed class PartyInventoryItemData
    {
        public string ItemInstanceId;
        public string EquipmentLoadoutId;
        public string EquippedMemberId;
        public string EquippedSlotKey;

        public PartyInventoryItemData(string itemInstanceId, string equipmentLoadoutId)
        {
            ItemInstanceId = string.IsNullOrWhiteSpace(itemInstanceId) ? string.Empty : itemInstanceId.Trim();
            EquipmentLoadoutId = string.IsNullOrWhiteSpace(equipmentLoadoutId) ? string.Empty : equipmentLoadoutId.Trim();
            EquippedMemberId = string.Empty;
            EquippedSlotKey = string.Empty;
        }
    }

    private sealed class EquipmentRewardSelectionData
    {
        public PartyMemberProgressionData Member;
        public string SlotKey;
        public int PriorityIndex = int.MaxValue;
        public PrototypeRpgEquipmentDefinition CurrentDefinition;
        public PrototypeRpgEquipmentDefinition RewardDefinition;
        public int ScoreDelta;
    }

    private sealed class PartyRuntimeData
    {
        public string PartyId;
        public string DisplayName;
        public string HomeCityId;
        public string ArchetypeId;
        public string PromotionStateId;
        public PartyState State;
        public int Power;
        public int CarryCapacity;
        public string TargetDungeonId;
        public string ActiveRouteId;
        public int DaysRemaining;
        public int DepartureDay;
        public int ProjectedReturnDay;
        public string LastResultSummary;
        public PartyMemberProgressionData[] Members;
        public readonly Dictionary<string, int> PendingRewardInventory;
        public readonly List<PartyInventoryItemData> InventoryItems;
        public string PendingRewardSummaryText;
        public string InventorySummaryText;
        public string IdentitySummaryText;
        public string RoleSummaryText;
        public string LoadoutSummaryText;
        public string LatestGearRewardSummaryText;
        public string LatestEquipSwapSummaryText;
        public string LatestGearContinuitySummaryText;
        public string LatestManualEquipmentActionSummaryText;
        public int NextEquipmentItemSequence;
        public int InventoryRevision;

        public PartyRuntimeData(string partyId, string displayName, string homeCityId, string archetypeId, string promotionStateId)
        {
            PartyId = partyId ?? string.Empty;
            DisplayName = string.IsNullOrWhiteSpace(displayName) ? PartyId : displayName.Trim();
            HomeCityId = homeCityId ?? string.Empty;
            ArchetypeId = string.IsNullOrWhiteSpace(archetypeId) ? PrototypeRpgPartyCatalog.ResolveArchetypeIdForSeed(homeCityId, 1) : archetypeId.Trim().ToLowerInvariant();
            PromotionStateId = string.IsNullOrWhiteSpace(promotionStateId) ? PrototypeRpgPartyCatalog.GetInitialPromotionStateId() : promotionStateId.Trim().ToLowerInvariant();
            State = PartyState.Idle;
            Power = 1;
            CarryCapacity = 1;
            TargetDungeonId = string.Empty;
            ActiveRouteId = string.Empty;
            DaysRemaining = 0;
            DepartureDay = -1;
            ProjectedReturnDay = -1;
            LastResultSummary = "None";
            Members = System.Array.Empty<PartyMemberProgressionData>();
            PendingRewardInventory = new Dictionary<string, int>();
            InventoryItems = new List<PartyInventoryItemData>();
            PendingRewardSummaryText = "None";
            InventorySummaryText = "Owned 0 | Equipped 0 | Stored 0";
            IdentitySummaryText = "None";
            RoleSummaryText = "None";
            LoadoutSummaryText = "None";
            LatestGearRewardSummaryText = "None";
            LatestEquipSwapSummaryText = "None";
            LatestGearContinuitySummaryText = "Owned 0 | Equipped 0 | Stored 0";
            LatestManualEquipmentActionSummaryText = "None";
            NextEquipmentItemSequence = 1;
            InventoryRevision = 1;
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

    private enum WorldEventRecordType
    {
        RunWriteback,
        CityDelta,
        DungeonDelta,
        TimeAdvance
    }

    private sealed class CityWritebackDelta
    {
        public string CityId = string.Empty;
        public string CityLabel = "None";
        public string ResultStateKey = "none";
        public string RewardResourceId = string.Empty;
        public int LootReturned;
        public string LootSummaryText = "None";
        public string PartyOutcomeSummaryText = "None";
        public string StockReactionSummaryText = "None";
        public string SummaryText = "None";
    }

    private sealed class DungeonWritebackDelta
    {
        public string DungeonId = string.Empty;
        public string DungeonLabel = "None";
        public string ResultStateKey = "none";
        public string RouteId = string.Empty;
        public string RouteLabel = "None";
        public string StatusKey = "unknown";
        public string StatusSummaryText = "None";
        public string AvailabilitySummaryText = "None";
        public string LastOutcomeSummaryText = "None";
    }

    private sealed class EconomyDelta
    {
        public string RewardResourceId = string.Empty;
        public int LootReturned;
        public string LootSummaryText = "None";
        public string SummaryText = "None";
    }

    private sealed class TimeAdvanceResult
    {
        public int DayBefore;
        public int DayAfter;
        public int ElapsedDays;
        public string SummaryText = "None";
    }

    private sealed class WorldWriteback
    {
        public string RunResultStateKey = "none";
        public string SourceCityId = string.Empty;
        public string SourceCityLabel = "None";
        public string TargetDungeonId = string.Empty;
        public string TargetDungeonLabel = "None";
        public string ChosenRouteId = string.Empty;
        public string ChosenRouteLabel = "None";
        public string ResultSummaryText = "None";
        public string WritebackSummaryText = "None";
        public CityWritebackDelta CityDelta = new CityWritebackDelta();
        public DungeonWritebackDelta DungeonDelta = new DungeonWritebackDelta();
        public EconomyDelta Economy = new EconomyDelta();
        public TimeAdvanceResult TimeAdvance = new TimeAdvanceResult();
    }

    private sealed class WorldWritebackLogRecord
    {
        public WorldEventRecordType EventType;
        public int DayIndex;
        public string CityId = string.Empty;
        public string DungeonId = string.Empty;
        public string ResultStateKey = "none";
        public string DeltaSummary = "None";
        public string DisplayText = "None";
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
    private readonly Dictionary<string, ExpeditionResult> _latestExpeditionResultByCityId;
    private readonly Dictionary<string, int> _expeditionLootReturnedByCityId;
    private readonly Dictionary<string, string> _lastRunLootSummaryByCityId;
    private readonly Dictionary<string, string> _lastRunSurvivingMembersByCityId;
    private readonly Dictionary<string, string> _lastRunClearedEncountersByCityId;
    private readonly Dictionary<string, string> _lastRunEventChoiceByCityId;
    private readonly Dictionary<string, string> _lastRunLootBreakdownByCityId;
    private readonly Dictionary<string, string> _lastRunRouteByCityId;
    private readonly Dictionary<string, string> _lastRunDungeonByCityId;
    private readonly List<WorldWritebackLogRecord> _recentWorldWritebackLogs;
    private readonly Dictionary<string, string> _latestWorldWritebackByCityId;
    private readonly Dictionary<string, string> _latestWorldWritebackByDungeonId;
    private readonly Dictionary<string, string> _dungeonStatusSummaryByDungeonId;
    private readonly Dictionary<string, string> _dungeonAvailabilitySummaryByDungeonId;
    private readonly Dictionary<string, string> _dungeonLastOutcomeSummaryByDungeonId;
    private WorldWriteback _latestWorldWriteback;
    private ExpeditionResult _latestExpeditionResult;
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
        _latestExpeditionResultByCityId = new Dictionary<string, ExpeditionResult>();
        _expeditionLootReturnedByCityId = new Dictionary<string, int>();
        _lastRunLootSummaryByCityId = new Dictionary<string, string>();
        _lastRunSurvivingMembersByCityId = new Dictionary<string, string>();
        _lastRunClearedEncountersByCityId = new Dictionary<string, string>();
        _lastRunEventChoiceByCityId = new Dictionary<string, string>();
        _lastRunLootBreakdownByCityId = new Dictionary<string, string>();
        _lastRunRouteByCityId = new Dictionary<string, string>();
        _lastRunDungeonByCityId = new Dictionary<string, string>();
        _recentWorldWritebackLogs = new List<WorldWritebackLogRecord>(RecentWorldWritebackLogLimit);
        _latestWorldWritebackByCityId = new Dictionary<string, string>();
        _latestWorldWritebackByDungeonId = new Dictionary<string, string>();
        _dungeonStatusSummaryByDungeonId = new Dictionary<string, string>();
        _dungeonAvailabilitySummaryByDungeonId = new Dictionary<string, string>();
        _dungeonLastOutcomeSummaryByDungeonId = new Dictionary<string, string>();
        _latestWorldWriteback = null;
        _latestExpeditionResult = null;
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
        _latestExpeditionResultByCityId.Clear();
        _expeditionLootReturnedByCityId.Clear();
        _lastRunLootSummaryByCityId.Clear();
        _lastRunSurvivingMembersByCityId.Clear();
        _lastRunClearedEncountersByCityId.Clear();
        _lastRunEventChoiceByCityId.Clear();
        _lastRunLootBreakdownByCityId.Clear();
        _lastRunRouteByCityId.Clear();
        _lastRunDungeonByCityId.Clear();
        _recentWorldWritebackLogs.Clear();
        _latestWorldWritebackByCityId.Clear();
        _latestWorldWritebackByDungeonId.Clear();
        _dungeonStatusSummaryByDungeonId.Clear();
        _dungeonAvailabilitySummaryByDungeonId.Clear();
        _dungeonLastOutcomeSummaryByDungeonId.Clear();
        _latestWorldWriteback = null;
        _latestExpeditionResult = null;
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

            if (pair.Value.Kind == WorldEntityKind.City && !_latestWorldWritebackByCityId.ContainsKey(pair.Key))
            {
                _latestWorldWritebackByCityId.Add(pair.Key, "None");
            }

            if (pair.Value.Kind == WorldEntityKind.Dungeon && !_latestWorldWritebackByDungeonId.ContainsKey(pair.Key))
            {
                _latestWorldWritebackByDungeonId.Add(pair.Key, "None");
            }

            if (pair.Value.Kind == WorldEntityKind.Dungeon && !_dungeonStatusSummaryByDungeonId.ContainsKey(pair.Key))
            {
                _dungeonStatusSummaryByDungeonId.Add(pair.Key, "Dormant");
            }

            if (pair.Value.Kind == WorldEntityKind.Dungeon && !_dungeonAvailabilitySummaryByDungeonId.ContainsKey(pair.Key))
            {
                _dungeonAvailabilitySummaryByDungeonId.Add(pair.Key, "Open for expedition");
            }

            if (pair.Value.Kind == WorldEntityKind.Dungeon && !_dungeonLastOutcomeSummaryByDungeonId.ContainsKey(pair.Key))
            {
                _dungeonLastOutcomeSummaryByDungeonId.Add(pair.Key, "None");
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
        string partyId = "Party " + _nextPartySequence;
        string archetypeId = PrototypeRpgPartyCatalog.ResolveArchetypeIdForSeed(cityId, _nextPartySequence);
        string promotionStateId = PrototypeRpgPartyCatalog.GetInitialPromotionStateId();
        PartyRuntimeData party = new PartyRuntimeData(partyId, partyId, cityId, archetypeId, promotionStateId);
        InitializePartyMembers(party);
        UpdatePartyDerivedStats(party);
        party.LastResultSummary = "Ready in " + city.DisplayName;
        _parties.Add(party);
        _lastExpeditionResultByCityId[cityId] = party.LastResultSummary;
        AppendRecentExpeditionLog(
            "Day " + WorldDayCount + " | " + city.DisplayName +
            " recruited " + party.PartyId +
            " [" + PrototypeRpgPartyCatalog.GetArchetypeLabel(party.ArchetypeId) + "]" +
            " (" + PrototypeRpgPartyCatalog.GetPromotionStateLabel(party.PromotionStateId) +
            ", Power " + party.Power +
            ", Carry " + party.CarryCapacity + ")");
        return true;
    }

    public string BeginDungeonRun(string cityId, string dungeonId, string routeId = null)
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
        party.ActiveRouteId = string.IsNullOrWhiteSpace(routeId) ? string.Empty : routeId.Trim().ToLowerInvariant();
        party.DaysRemaining = 1;
        party.DepartureDay = WorldDayCount;
        party.ProjectedReturnDay = WorldDayCount + party.DaysRemaining;
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
        _latestExpeditionResultByCityId.Remove(cityId);

        if (_latestExpeditionResult != null && _latestExpeditionResult.SourceCityId == cityId)
        {
            _latestExpeditionResult = null;
        }

        string routeEcho = string.IsNullOrEmpty(party.ActiveRouteId) ? string.Empty : " via " + party.ActiveRouteId;
        AppendRecentExpeditionLog("Day " + WorldDayCount + " | " + city.DisplayName + " sent " + party.PartyId + " into " + dungeon.DisplayName + routeEcho);
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
        party.ActiveRouteId = string.Empty;
        party.DaysRemaining = profile.ExpeditionDurationDays;
        party.DepartureDay = WorldDayCount;
        party.ProjectedReturnDay = WorldDayCount + party.DaysRemaining;
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

    public void RecordRecentExpeditionContextLog(string logText)
    {
        AppendRecentExpeditionLog(logText);
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

    public string GetLastRunGearRewardSummaryForCity(string cityId)
    {
        ExpeditionResult result = GetLatestExpeditionResultForCity(cityId);
        return result != null && !string.IsNullOrEmpty(result.GearRewardCandidateSummaryText)
            ? result.GearRewardCandidateSummaryText
            : "None";
    }

    public string GetLastRunEquipSwapSummaryForCity(string cityId)
    {
        ExpeditionResult result = GetLatestExpeditionResultForCity(cityId);
        return result != null && !string.IsNullOrEmpty(result.EquipSwapChoiceSummaryText)
            ? result.EquipSwapChoiceSummaryText
            : "None";
    }

    public string GetLastRunGearContinuitySummaryForCity(string cityId)
    {
        ExpeditionResult result = GetLatestExpeditionResultForCity(cityId);
        return result != null && !string.IsNullOrEmpty(result.GearCarryContinuitySummaryText)
            ? result.GearCarryContinuitySummaryText
            : "None";
    }

    public ExpeditionResult GetLatestExpeditionResultForCity(string cityId)
    {
        return !string.IsNullOrEmpty(cityId) && _latestExpeditionResultByCityId.TryGetValue(cityId, out ExpeditionResult value)
            ? value
            : null;
    }

    public ExpeditionResult GetLatestExpeditionResult()
    {
        return _latestExpeditionResult;
    }

    public ExpeditionOutcome GetLatestExpeditionOutcomeForCity(string cityId)
    {
        return CreatePublicExpeditionOutcome(GetLatestExpeditionResultForCity(cityId));
    }

    public ExpeditionOutcome GetLatestExpeditionOutcome()
    {
        return CreatePublicExpeditionOutcome(_latestExpeditionResult);
    }

    public OutcomeReadback GetLatestOutcomeReadback()
    {
        string cityId = !string.IsNullOrEmpty(GetLatestWorldWritebackCityId())
            ? GetLatestWorldWritebackCityId()
            : _latestExpeditionResult != null
                ? _latestExpeditionResult.SourceCityId
                : string.Empty;
        return CreatePublicOutcomeReadback(cityId, GetLatestExpeditionResult(), GetLatestWorldWriteback());
    }

    public OutcomeReadback GetLatestOutcomeReadbackForCity(string cityId)
    {
        if (string.IsNullOrEmpty(cityId))
        {
            return new OutcomeReadback();
        }

        return CreatePublicOutcomeReadback(cityId, GetLatestExpeditionResultForCity(cityId), GetLatestWorldWritebackForCity(cityId));
    }

    public global::WorldWriteback GetLatestWorldWriteback()
    {
        ExpeditionResult expeditionResult = _latestWorldWriteback != null && !string.IsNullOrEmpty(_latestWorldWriteback.SourceCityId)
            ? GetLatestExpeditionResultForCity(_latestWorldWriteback.SourceCityId)
            : _latestExpeditionResult;
        return CreatePublicWorldWriteback(_latestWorldWriteback, expeditionResult);
    }

    public global::WorldWriteback GetLatestWorldWritebackForCity(string cityId)
    {
        if (string.IsNullOrEmpty(cityId))
        {
            return new global::WorldWriteback();
        }

        if (_latestWorldWriteback != null && _latestWorldWriteback.SourceCityId == cityId)
        {
            return CreatePublicWorldWriteback(_latestWorldWriteback, GetLatestExpeditionResultForCity(cityId));
        }

        ExpeditionResult expeditionResult = GetLatestExpeditionResultForCity(cityId);
        return CreateStoredCityWorldWriteback(cityId, expeditionResult);
    }

    public string GetLatestWorldWritebackSummary()
    {
        global::WorldWriteback writeback = GetLatestWorldWriteback();
        return writeback != null && !string.IsNullOrEmpty(writeback.WritebackSummaryText)
            ? writeback.WritebackSummaryText
            : "None";
    }

    public string GetLatestWorldWritebackSummaryForCity(string cityId)
    {
        return !string.IsNullOrEmpty(cityId) && _latestWorldWritebackByCityId.TryGetValue(cityId, out string value)
            ? value
            : "None";
    }

    public string GetLatestWorldWritebackStateKeyForCity(string cityId)
    {
        global::WorldWriteback writeback = GetLatestWorldWritebackForCity(cityId);
        return writeback != null ? writeback.RunResultStateKey ?? string.Empty : string.Empty;
    }

    public string GetLatestWorldWritebackRewardResourceIdForCity(string cityId)
    {
        global::WorldWriteback writeback = GetLatestWorldWritebackForCity(cityId);
        return writeback != null ? writeback.RewardResourceId ?? string.Empty : string.Empty;
    }

    public int GetLatestWorldWritebackLootReturnedForCity(string cityId)
    {
        global::WorldWriteback writeback = GetLatestWorldWritebackForCity(cityId);
        return writeback != null ? writeback.LootReturned : 0;
    }

    public string GetLatestWorldWritebackLootSummaryForCity(string cityId)
    {
        global::WorldWriteback writeback = GetLatestWorldWritebackForCity(cityId);
        return writeback != null && !string.IsNullOrEmpty(writeback.LootSummaryText)
            ? writeback.LootSummaryText
            : "None";
    }

    public string GetLatestWorldWritebackPartyOutcomeSummaryForCity(string cityId)
    {
        global::WorldWriteback writeback = GetLatestWorldWritebackForCity(cityId);
        return writeback != null && !string.IsNullOrEmpty(writeback.SurvivingMembersSummaryText)
            ? writeback.SurvivingMembersSummaryText
            : "None";
    }

    public string GetLatestWorldWritebackStockReactionSummaryForCity(string cityId)
    {
        global::WorldWriteback writeback = GetLatestWorldWritebackForCity(cityId);
        return writeback != null &&
            writeback.CityWriteback != null &&
            !string.IsNullOrEmpty(writeback.CityWriteback.StockReactionSummaryText)
            ? writeback.CityWriteback.StockReactionSummaryText
            : "None";
    }

    public string GetLatestWorldWritebackSummaryForDungeon(string dungeonId)
    {
        return !string.IsNullOrEmpty(dungeonId) && _latestWorldWritebackByDungeonId.TryGetValue(dungeonId, out string value)
            ? value
            : "None";
    }

    public string GetRecentWorldWritebackLogText(int index)
    {
        return index >= 0 && index < _recentWorldWritebackLogs.Count
            ? _recentWorldWritebackLogs[index].DisplayText
            : "None";
    }

    public string GetDungeonWorldStatusText(string dungeonId)
    {
        return !string.IsNullOrEmpty(dungeonId) && _dungeonStatusSummaryByDungeonId.TryGetValue(dungeonId, out string value)
            ? value
            : "None";
    }

    public string GetDungeonWorldAvailabilityText(string dungeonId)
    {
        return !string.IsNullOrEmpty(dungeonId) && _dungeonAvailabilitySummaryByDungeonId.TryGetValue(dungeonId, out string value)
            ? value
            : "None";
    }

    public string GetDungeonLastWorldOutcomeText(string dungeonId)
    {
        return !string.IsNullOrEmpty(dungeonId) && _dungeonLastOutcomeSummaryByDungeonId.TryGetValue(dungeonId, out string value)
            ? value
            : "None";
    }

    public string GetLatestWorldWritebackCityId()
    {
        return _latestWorldWriteback != null ? _latestWorldWriteback.SourceCityId : string.Empty;
    }

    public string GetLatestWorldWritebackDungeonId()
    {
        return _latestWorldWriteback != null ? _latestWorldWriteback.TargetDungeonId : string.Empty;
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

    public int GetReadyPartyPowerForCity(string cityId)
    {
        PartyRuntimeData party = FindIdleParty(cityId);
        return party != null ? party.Power : 0;
    }

    public int GetReadyPartyCarryCapacityForCity(string cityId)
    {
        PartyRuntimeData party = FindIdleParty(cityId);
        return party != null ? party.CarryCapacity : 0;
    }

    public int GetActivePartyPowerForCity(string cityId)
    {
        PartyRuntimeData party = FindActivePartyForCity(cityId);
        return party != null ? party.Power : 0;
    }

    public int GetActivePartyCarryCapacityForCity(string cityId)
    {
        PartyRuntimeData party = FindActivePartyForCity(cityId);
        return party != null ? party.CarryCapacity : 0;
    }

    public string GetPartyDisplayName(string partyId)
    {
        PartyRuntimeData party = FindPartyById(partyId);
        return party != null && !string.IsNullOrEmpty(party.DisplayName) ? party.DisplayName : string.Empty;
    }

    public string GetPartyHomeCityId(string partyId)
    {
        PartyRuntimeData party = FindPartyById(partyId);
        return party != null ? party.HomeCityId : string.Empty;
    }

    public string GetPartyArchetypeId(string partyId)
    {
        PartyRuntimeData party = FindPartyById(partyId);
        return party != null ? party.ArchetypeId : string.Empty;
    }

    public string GetPartyPromotionStateId(string partyId)
    {
        PartyRuntimeData party = FindPartyById(partyId);
        return party != null ? party.PromotionStateId : string.Empty;
    }

    public int GetPartyMemberCount(string partyId)
    {
        PartyRuntimeData party = FindPartyById(partyId);
        return party != null && party.Members != null ? party.Members.Length : 0;
    }

    public string GetPartyIdentitySummary(string partyId)
    {
        PartyRuntimeData party = FindPartyById(partyId);
        return party != null && !string.IsNullOrEmpty(party.IdentitySummaryText)
            ? party.IdentitySummaryText
            : "None";
    }

    public string GetPartyRoleSummary(string partyId)
    {
        PartyRuntimeData party = FindPartyById(partyId);
        return party != null && !string.IsNullOrEmpty(party.RoleSummaryText)
            ? party.RoleSummaryText
            : "None";
    }

    public string GetPartyLoadoutSummary(string partyId)
    {
        PartyRuntimeData party = FindPartyById(partyId);
        return party != null && !string.IsNullOrEmpty(party.LoadoutSummaryText)
            ? party.LoadoutSummaryText
            : "None";
    }

    public string GetPartyRouteRoleFitSummary(string partyId, string routeId, string routeRiskLabel)
    {
        PartyRuntimeData party = FindPartyById(partyId);
        if (party == null || party.Members == null || party.Members.Length <= 0)
        {
            return string.Empty;
        }

        return PrototypeRpgRoleIdentity.BuildRouteRoleFitSummary(
            routeId,
            routeRiskLabel,
            party.Members,
            member => member != null ? member.DisplayName : string.Empty,
            member => member != null ? member.RoleTag : string.Empty);
    }

    public int GetPartyPower(string partyId)
    {
        PartyRuntimeData party = FindPartyById(partyId);
        return party != null ? party.Power : 0;
    }

    public int GetPartyCarryCapacity(string partyId)
    {
        PartyRuntimeData party = FindPartyById(partyId);
        return party != null ? party.CarryCapacity : 0;
    }

    public string GetPartyPendingRewardSummary(string partyId)
    {
        PartyRuntimeData party = FindPartyById(partyId);
        return party != null && !string.IsNullOrEmpty(party.PendingRewardSummaryText)
            ? party.PendingRewardSummaryText
            : "None";
    }

    public string GetPartyInventorySummary(string partyId)
    {
        PartyRuntimeData party = FindPartyById(partyId);
        return party != null && !string.IsNullOrEmpty(party.InventorySummaryText)
            ? party.InventorySummaryText
            : "Owned 0 | Equipped 0 | Stored 0";
    }

    public int GetPartyInventoryRevision(string partyId)
    {
        PartyRuntimeData party = FindPartyById(partyId);
        return party != null ? Mathf.Max(1, party.InventoryRevision) : 0;
    }

    public string GetPartyLatestManualEquipmentActionSummary(string partyId)
    {
        PartyRuntimeData party = FindPartyById(partyId);
        return party != null && !string.IsNullOrEmpty(party.LatestManualEquipmentActionSummaryText)
            ? party.LatestManualEquipmentActionSummaryText
            : "None";
    }

    public PrototypeRpgInventorySurfaceData BuildPartyInventorySurface(
        string partyId,
        string selectedMemberId,
        string selectedSlotKey,
        string selectedItemInstanceId,
        bool isReadOnly,
        string feedbackText)
    {
        PrototypeRpgInventorySurfaceData surface = new PrototypeRpgInventorySurfaceData();
        PartyRuntimeData party = FindPartyById(partyId);
        if (party == null)
        {
            surface.FeedbackText = string.IsNullOrEmpty(feedbackText) ? "No party inventory is available." : feedbackText;
            return surface;
        }

        if (party.Members == null || party.Members.Length <= 0)
        {
            InitializePartyMembers(party);
        }

        surface.IsOpen = true;
        surface.IsReadOnly = isReadOnly;
        surface.ContextLabel = isReadOnly ? "Battle Read-Only Inspection" : "Manual Equipment";
        surface.PartyId = party.PartyId;
        surface.PartyLabel = party.DisplayName;
        surface.InventorySummaryText = GetPartyInventorySummary(party.PartyId);
        surface.LatestRewardSummaryText = string.IsNullOrEmpty(party.LatestGearRewardSummaryText) ? "None" : party.LatestGearRewardSummaryText;
        surface.LatestAutoEquipSummaryText = string.IsNullOrEmpty(party.LatestEquipSwapSummaryText) ? "None" : party.LatestEquipSwapSummaryText;
        surface.LatestContinuitySummaryText = string.IsNullOrEmpty(party.LatestGearContinuitySummaryText) ? surface.InventorySummaryText : party.LatestGearContinuitySummaryText;
        surface.LatestManualActionSummaryText = string.IsNullOrEmpty(party.LatestManualEquipmentActionSummaryText) ? "None" : party.LatestManualEquipmentActionSummaryText;
        surface.FeedbackText = string.IsNullOrEmpty(feedbackText) ? surface.LatestManualActionSummaryText : feedbackText;

        PartyMemberProgressionData selectedMember = ResolveInventorySelectedMember(party, selectedMemberId);
        string resolvedSlotKey = ResolveInventorySelectedSlotKey(selectedSlotKey);
        if (selectedMember == null)
        {
            surface.FeedbackText = string.IsNullOrEmpty(surface.FeedbackText) || surface.FeedbackText == "None"
                ? "No party member is available."
                : surface.FeedbackText;
            return surface;
        }

        PrototypeRpgInventoryMemberSurfaceData[] memberSurface = BuildInventoryMemberSurfaceArray(party, selectedMember);
        surface.Members = memberSurface;
        surface.SelectedMemberId = selectedMember.MemberId;

        PrototypeRpgPartyDefinition partyDefinition = PrototypeRpgPartyCatalog.CreateRuntimeParty(
            party.PartyId,
            party.ArchetypeId,
            party.PromotionStateId,
            party.DisplayName);
        PrototypeRpgPartyMemberDefinition memberDefinition = FindPartyMemberDefinition(partyDefinition, selectedMember.MemberId);
        ResolveMemberInventoryStatBreakdown(
            party,
            selectedMember,
            memberDefinition,
            out int baseMaxHp,
            out int baseAttack,
            out int baseDefense,
            out int baseSpeed,
            out int growthMaxHp,
            out int growthAttack,
            out int growthDefense,
            out int growthSpeed,
            out int gearMaxHp,
            out int gearAttack,
            out int gearDefense,
            out int gearSpeed,
            out int gearSkillPower,
            out int resolvedMaxHp,
            out int resolvedAttack,
            out int resolvedDefense,
            out int resolvedSpeed,
            out int resolvedSkillPower);

        surface.SelectedMemberHeaderText = selectedMember.DisplayName + "  Lv" + Mathf.Max(1, selectedMember.Level);
        surface.SelectedMemberProgressText = "XP " + Mathf.Max(0, selectedMember.Experience) + "/" + PrototypeRpgMemberProgressionRules.GetNextLevelExperience(Mathf.Max(1, selectedMember.Level));
        surface.SelectedMemberRoleText = PrototypeRpgRoleIdentity.BuildRoleIdentityLabel(
            memberDefinition != null ? memberDefinition.RoleLabel : string.Empty,
            selectedMember.RoleTag) + " | " + PrototypeRpgRoleIdentity.BuildRoleIdentityText(selectedMember.RoleTag);
        surface.SelectedMemberGearPreferenceText = PrototypeRpgRoleIdentity.BuildGearPreferenceText(selectedMember.RoleTag);
        surface.SelectedItemHeaderText = "Select an item.";
        surface.SelectedItemMetaText = "No item selected.";
        surface.SelectedItemDetailText = "Select stored gear to inspect bonus, delta, and equip fit.";
        surface.SelectedItemFitText = "Fit: Select stored gear to inspect the current role.";
        surface.SelectedItemOwnerText = "Stored";
        surface.FooterSummaryText = "Power " + Mathf.Max(1, party.Power) + " | Carry " + Mathf.Max(1, party.CarryCapacity) + " | " + surface.InventorySummaryText;

        PrototypeRpgInventorySlotSurfaceData[] slotSurface = BuildInventorySlotSurfaceArray(party, selectedMember, resolvedSlotKey);
        surface.Slots = slotSurface;
        surface.SelectedSlotKey = resolvedSlotKey;

        PrototypeRpgInventoryItemSurfaceData[] itemSurface = BuildInventoryItemSurfaceArray(party, selectedMember, resolvedSlotKey, selectedItemInstanceId);
        surface.InventoryItems = itemSurface;
        PartyInventoryItemData selectedInventoryItem = ResolveSelectedInventoryItem(party, itemSurface);
        PrototypeRpgEquipmentDefinition currentDefinition = GetActualEquippedDefinition(party, selectedMember, resolvedSlotKey);
        PrototypeRpgEquipmentDefinition candidateDefinition = ResolvePartyInventoryItemDefinition(selectedInventoryItem, selectedMember.RoleTag);
        bool canEquip = CanManualEquipInventoryItem(party, selectedMember, resolvedSlotKey, selectedInventoryItem, candidateDefinition, out string equipReasonText);
        bool canUnequip = HasActualEquippedItem(party, selectedMember, resolvedSlotKey) && !isReadOnly;
        surface.CanEquipSelectedItem = canEquip;
        surface.CanUnequipSelectedSlot = canUnequip;

        surface.InputHintText = isReadOnly
            ? "[I] Toggle | [Q/E] Member | [1-7] Slot | [Up/Down] Item | [Esc] Close"
            : "[I] Toggle | [Q/E] Member | [1-7] Slot | [Up/Down] Item | [Enter] Equip | [U] Unequip | [Esc] Close";

        string baseText = BuildResolvedStatText(baseMaxHp, baseAttack, baseDefense, baseSpeed, false);
        string growthText = PrototypeRpgEquipmentCatalog.BuildStatContributionSummary(growthMaxHp, growthAttack, growthDefense, growthSpeed, 0);
        string gearText = PrototypeRpgEquipmentCatalog.BuildStatContributionSummary(gearMaxHp, gearAttack, gearDefense, gearSpeed, gearSkillPower);
        string resolvedText = BuildResolvedStatText(resolvedMaxHp, resolvedAttack, resolvedDefense, resolvedSpeed, true);
        for (int i = 0; i < surface.Members.Length; i++)
        {
            if (!surface.Members[i].IsSelected)
            {
                continue;
            }

            surface.Members[i].BaseStatsText = "Base: " + baseText;
            surface.Members[i].LevelBonusText = "Level: " + growthText;
            surface.Members[i].GearBonusText = "Gear: " + gearText;
            surface.Members[i].ResolvedStatsText = "Resolved: " + resolvedText;
            surface.Members[i].SummaryText = surface.SelectedMemberHeaderText + " | " + resolvedText;
            break;
        }

        PrototypeRpgInventoryComparisonSurfaceData comparison = BuildInventoryComparisonSurface(
            party,
            selectedMember,
            resolvedSlotKey,
            currentDefinition,
            selectedInventoryItem,
            candidateDefinition,
            canEquip,
            canUnequip,
            equipReasonText,
            isReadOnly);
        surface.Comparison = comparison;
        if (selectedInventoryItem != null && candidateDefinition != null)
        {
            surface.SelectedItemInstanceId = selectedInventoryItem.ItemInstanceId;
            surface.SelectedItemHeaderText = candidateDefinition.DisplayName;
            surface.SelectedItemMetaText = candidateDefinition.SlotLabel + " | " + candidateDefinition.TierLabel;
            surface.SelectedItemDetailText = BuildInventorySelectedItemDetailText(party, selectedInventoryItem, candidateDefinition, comparison);
            surface.SelectedItemFitText = PrototypeRpgRoleIdentity.BuildEquipmentFitText(selectedMember.DisplayName, selectedMember.RoleTag, candidateDefinition);
            surface.SelectedItemOwnerText = BuildInventoryItemOwnerText(party, selectedInventoryItem);
        }

        return surface;
    }

    public bool TryManualEquipPartyInventoryItem(
        string partyId,
        string memberId,
        string slotKey,
        string itemInstanceId,
        out string feedbackText)
    {
        feedbackText = "Equip unavailable.";
        PartyRuntimeData party = FindPartyById(partyId);
        PartyMemberProgressionData member = FindPartyMemberProgression(partyId, memberId);
        PartyInventoryItemData item = FindPartyInventoryItem(party, itemInstanceId);
        PrototypeRpgEquipmentDefinition definition = ResolvePartyInventoryItemDefinition(item, member != null ? member.RoleTag : string.Empty);
        string normalizedSlotKey = NormalizeEquipmentSlotKey(slotKey);
        if (party == null || member == null || item == null || definition == null)
        {
            feedbackText = "Equip target is missing.";
            return false;
        }

        if (!CanManualEquipInventoryItem(party, member, normalizedSlotKey, item, definition, out string reasonText))
        {
            feedbackText = reasonText;
            return false;
        }

        PrototypeRpgEquipmentDefinition currentDefinition = GetActualEquippedDefinition(party, member, normalizedSlotKey);
        if (currentDefinition != null &&
            member.EquippedItemInstanceIdBySlotKey.TryGetValue(normalizedSlotKey, out string currentItemInstanceId) &&
            currentItemInstanceId == item.ItemInstanceId)
        {
            feedbackText = member.DisplayName + " already has " + definition.DisplayName + " equipped.";
            return false;
        }

        if (member.EquippedItemInstanceIdBySlotKey.TryGetValue(normalizedSlotKey, out string previousItemInstanceId))
        {
            PartyInventoryItemData previousItem = FindPartyInventoryItem(party, previousItemInstanceId);
            if (previousItem != null)
            {
                previousItem.EquippedMemberId = string.Empty;
                previousItem.EquippedSlotKey = string.Empty;
            }
        }

        member.EquippedItemInstanceIdBySlotKey[normalizedSlotKey] = item.ItemInstanceId;
        item.EquippedMemberId = member.MemberId;
        item.EquippedSlotKey = normalizedSlotKey;
        party.LatestManualEquipmentActionSummaryText = BuildManualEquipResultText(party, member, currentDefinition, definition);
        feedbackText = party.LatestManualEquipmentActionSummaryText;
        RefreshPartyInventorySummary(party);
        UpdatePartyDerivedStats(party);
        return true;
    }

    public bool TryManualUnequipPartyMemberSlot(
        string partyId,
        string memberId,
        string slotKey,
        out string feedbackText)
    {
        feedbackText = "Unequip unavailable.";
        PartyRuntimeData party = FindPartyById(partyId);
        PartyMemberProgressionData member = FindPartyMemberProgression(partyId, memberId);
        string normalizedSlotKey = NormalizeEquipmentSlotKey(slotKey);
        if (party == null || member == null)
        {
            feedbackText = "Unequip target is missing.";
            return false;
        }

        if (!member.EquippedItemInstanceIdBySlotKey.TryGetValue(normalizedSlotKey, out string itemInstanceId) || string.IsNullOrEmpty(itemInstanceId))
        {
            feedbackText = PrototypeRpgEquipmentSlotKeys.ToDisplayLabel(normalizedSlotKey) + " is already empty.";
            return false;
        }

        PartyInventoryItemData item = FindPartyInventoryItem(party, itemInstanceId);
        PrototypeRpgEquipmentDefinition currentDefinition = ResolvePartyInventoryItemDefinition(item, member.RoleTag);
        if (item != null)
        {
            item.EquippedMemberId = string.Empty;
            item.EquippedSlotKey = string.Empty;
        }

        member.EquippedItemInstanceIdBySlotKey.Remove(normalizedSlotKey);
        party.LatestManualEquipmentActionSummaryText = BuildManualUnequipResultText(party, member, currentDefinition);
        feedbackText = party.LatestManualEquipmentActionSummaryText;
        RefreshPartyInventorySummary(party);
        UpdatePartyDerivedStats(party);
        return true;
    }

    public int GetPartyMemberLevel(string partyId, string memberId)
    {
        PartyMemberProgressionData member = FindPartyMemberProgression(partyId, memberId);
        return member != null ? member.Level : PrototypeRpgMemberProgressionRules.GetStartingLevel();
    }

    public int GetPartyMemberExperience(string partyId, string memberId)
    {
        PartyMemberProgressionData member = FindPartyMemberProgression(partyId, memberId);
        return member != null ? member.Experience : PrototypeRpgMemberProgressionRules.GetStartingExperience();
    }

    public string GetPartyMemberEquipmentLoadoutId(string partyId, string memberId)
    {
        PartyRuntimeData party = FindPartyById(partyId);
        PartyMemberProgressionData member = FindPartyMemberProgression(partyId, memberId);
        PrototypeRpgEquipmentDefinition definition = GetFeaturedEquipmentDefinition(party, member);
        return definition != null ? definition.LoadoutId : string.Empty;
    }

    public string GetPartyMemberEquipmentSummary(string partyId, string memberId)
    {
        PartyRuntimeData party = FindPartyById(partyId);
        PartyMemberProgressionData member = FindPartyMemberProgression(partyId, memberId);
        return BuildMemberEquipmentSummaryText(party, member);
    }

    public string GetPartyMemberEquipmentSlotSummary(string partyId, string memberId)
    {
        PartyRuntimeData party = FindPartyById(partyId);
        PartyMemberProgressionData member = FindPartyMemberProgression(partyId, memberId);
        return BuildMemberEquipmentSlotSummaryText(party, member);
    }

    public string GetPartyMemberGearContributionSummary(string partyId, string memberId)
    {
        PartyRuntimeData party = FindPartyById(partyId);
        PartyMemberProgressionData member = FindPartyMemberProgression(partyId, memberId);
        ResolveMemberEquipmentBonuses(
            party,
            member,
            out int maxHpBonus,
            out int attackBonus,
            out int defenseBonus,
            out int speedBonus,
            out int skillPowerBonus);
        return PrototypeRpgEquipmentCatalog.BuildStatContributionSummary(
            maxHpBonus,
            attackBonus,
            defenseBonus,
            speedBonus,
            skillPowerBonus);
    }

    public int GetPartyMemberEquipmentMaxHpBonus(string partyId, string memberId)
    {
        ResolvePartyMemberEquipmentBonuses(
            partyId,
            memberId,
            out int maxHpBonus,
            out _,
            out _,
            out _,
            out _);
        return maxHpBonus;
    }

    public int GetPartyMemberEquipmentAttackBonus(string partyId, string memberId)
    {
        ResolvePartyMemberEquipmentBonuses(
            partyId,
            memberId,
            out _,
            out int attackBonus,
            out _,
            out _,
            out _);
        return attackBonus;
    }

    public int GetPartyMemberEquipmentDefenseBonus(string partyId, string memberId)
    {
        ResolvePartyMemberEquipmentBonuses(
            partyId,
            memberId,
            out _,
            out _,
            out int defenseBonus,
            out _,
            out _);
        return defenseBonus;
    }

    public int GetPartyMemberEquipmentSpeedBonus(string partyId, string memberId)
    {
        ResolvePartyMemberEquipmentBonuses(
            partyId,
            memberId,
            out _,
            out _,
            out _,
            out int speedBonus,
            out _);
        return speedBonus;
    }

    public int GetPartyMemberEquipmentSkillPowerBonus(string partyId, string memberId)
    {
        ResolvePartyMemberEquipmentBonuses(
            partyId,
            memberId,
            out _,
            out _,
            out _,
            out _,
            out int skillPowerBonus);
        return skillPowerBonus;
    }

    public string GetReadyPartyLastResultSummaryForCity(string cityId)
    {
        PartyRuntimeData party = FindIdleParty(cityId);
        if (party == null)
        {
            party = FindAnyPartyByHomeCity(cityId);
        }

        return party != null && !string.IsNullOrEmpty(party.LastResultSummary)
            ? party.LastResultSummary
            : "None";
    }

    public string GetActivePartyStatusTextForCity(string cityId)
    {
        PartyRuntimeData party = FindActivePartyForCity(cityId);
        return party != null ? BuildActivePartyStatus(party) : "None";
    }

    public string GetActivePartyIdForCity(string cityId)
    {
        PartyRuntimeData party = FindActivePartyForCity(cityId);
        return party != null ? party.PartyId : string.Empty;
    }

    public string GetActiveDungeonIdForCity(string cityId)
    {
        PartyRuntimeData party = FindActivePartyForCity(cityId);
        return party != null ? party.TargetDungeonId : string.Empty;
    }

    public string GetActiveRouteIdForCity(string cityId)
    {
        PartyRuntimeData party = FindActivePartyForCity(cityId);
        return party != null ? party.ActiveRouteId : string.Empty;
    }

    public int GetActiveDaysRemainingForCity(string cityId)
    {
        PartyRuntimeData party = FindActivePartyForCity(cityId);
        return party != null ? party.DaysRemaining : 0;
    }

    public int GetActiveExpeditionDaysRemainingForCity(string cityId)
    {
        return GetActiveDaysRemainingForCity(cityId);
    }

    public int GetActiveDepartureDayForCity(string cityId)
    {
        PartyRuntimeData party = FindActivePartyForCity(cityId);
        return party != null ? party.DepartureDay : -1;
    }

    public int GetActiveProjectedReturnDayForCity(string cityId)
    {
        PartyRuntimeData party = FindActivePartyForCity(cityId);
        return party != null ? party.ProjectedReturnDay : -1;
    }

    public string GetActivePartyIdForDungeon(string dungeonId)
    {
        PartyRuntimeData party = FindActivePartyForDungeon(dungeonId);
        return party != null ? party.PartyId : string.Empty;
    }

    public string GetActiveRouteIdForDungeon(string dungeonId)
    {
        PartyRuntimeData party = FindActivePartyForDungeon(dungeonId);
        return party != null ? party.ActiveRouteId : string.Empty;
    }

    public int GetActiveDaysRemainingForDungeon(string dungeonId)
    {
        PartyRuntimeData party = FindActivePartyForDungeon(dungeonId);
        return party != null ? party.DaysRemaining : 0;
    }

    public int GetActiveProjectedReturnDayForDungeon(string dungeonId)
    {
        PartyRuntimeData party = FindActivePartyForDungeon(dungeonId);
        return party != null ? party.ProjectedReturnDay : -1;
    }

    public string GetFirstActiveExpeditionCityId()
    {
        PartyRuntimeData party = FindFirstActiveParty();
        return party != null ? party.HomeCityId : string.Empty;
    }

    public string GetFirstActiveExpeditionDungeonId()
    {
        PartyRuntimeData party = FindFirstActiveParty();
        return party != null ? party.TargetDungeonId : string.Empty;
    }


    public string GetRewardPreviewText(string dungeonId)
    {
        return TryGetDungeonProfile(dungeonId, out DungeonExpeditionProfile profile) ? BuildRewardPreview(profile, DefaultPartyCarryCapacity) : "None";
    }

    public int GetMaxActiveExpeditionsForDungeon(string dungeonId)
    {
        return TryGetDungeonProfile(dungeonId, out DungeonExpeditionProfile profile) ? profile.MaxActiveParties : 0;
    }

    public int GetAvailableContractSlotsForDungeon(string dungeonId)
    {
        int available = GetMaxActiveExpeditionsForDungeon(dungeonId) - CountActiveExpeditionsForDungeonInternal(dungeonId);
        return available > 0 ? available : 0;
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
        party.ActiveRouteId = string.Empty;
        party.DaysRemaining = 0;
        party.DepartureDay = -1;
        party.ProjectedReturnDay = -1;
        party.LastResultSummary = resultSummary;
        _lastExpeditionResultByCityId[homeCityId] = resultSummary;
        _lastExpeditionResultByDungeonId[dungeonId] = resultSummary;
        AppendRecentExpeditionLog("Day " + WorldDayCount + " | " + resultSummary);
    }

    public void ResolveDungeonRun(ExpeditionResult expeditionResult)
    {
        if (expeditionResult == null)
        {
            return;
        }

        string cityId = expeditionResult.SourceCityId ?? string.Empty;
        string dungeonId = expeditionResult.DungeonId ?? string.Empty;
        PartyRuntimeData party = FindActivePartyForCity(cityId);
        if (party == null)
        {
            party = FindActivePartyForDungeon(dungeonId);
        }

        if (party == null)
        {
            return;
        }

        string rewardResourceId = expeditionResult.RewardResourceId ?? string.Empty;
        bool success = expeditionResult.Success;
        _latestExpeditionResult = expeditionResult;
        _latestExpeditionResultByCityId[cityId] = expeditionResult;
        string safeResultSummary = string.IsNullOrEmpty(expeditionResult.ResultSummaryText) || expeditionResult.ResultSummaryText == "None"
            ? party.PartyId + " returned."
            : expeditionResult.ResultSummaryText;
        int safeLootReturned = success && expeditionResult.ReturnedLootAmount > 0 ? expeditionResult.ReturnedLootAmount : 0;
        string lootSummary = success && !string.IsNullOrEmpty(rewardResourceId) && safeLootReturned > 0
            ? rewardResourceId + " x" + safeLootReturned
            : "None";
        ApplyMemberProgressionResults(party, expeditionResult.MemberProgressionResults);
        ApplyPendingRewardBundles(party, expeditionResult.PendingRewardBundles);
        bool partyPromoted = success && TryAdvancePartyPromotion(party);
        ApplyEquipmentRewardDrops(party, expeditionResult);
        ResultPipeline.RefreshGrowthRevealSummaries(expeditionResult);
        string promotionEcho = partyPromoted
            ? " | " + PrototypeRpgPartyCatalog.GetPromotionStateLabel(party.PromotionStateId)
            : string.Empty;
        string pendingRewardEcho = !string.IsNullOrEmpty(party.PendingRewardSummaryText) && party.PendingRewardSummaryText != "None"
            ? " | Stash " + party.PendingRewardSummaryText
            : string.Empty;
        string gearEcho = !string.IsNullOrEmpty(party.LatestEquipSwapSummaryText) && party.LatestEquipSwapSummaryText != "None"
            ? " | Gear " + party.LatestEquipSwapSummaryText
            : string.Empty;

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

        string safeRouteSummary = !string.IsNullOrEmpty(expeditionResult.SelectedRouteSummaryText) && expeditionResult.SelectedRouteSummaryText != "None"
            ? expeditionResult.SelectedRouteSummaryText
            : !string.IsNullOrEmpty(expeditionResult.RouteLabel) && expeditionResult.RouteLabel != "None"
                ? expeditionResult.RouteLabel
                : "None";
        string safeDungeonSummary = !string.IsNullOrEmpty(expeditionResult.DungeonSummaryText) && expeditionResult.DungeonSummaryText != "None"
            ? expeditionResult.DungeonSummaryText
            : !string.IsNullOrEmpty(dungeonId) && TryGetEntity(dungeonId, out WorldEntityData resolvedDungeon)
                ? resolvedDungeon.DisplayName
                : "None";

        _lastRunLootSummaryByCityId[cityId] = lootSummary;
        _lastRunSurvivingMembersByCityId[cityId] = string.IsNullOrEmpty(expeditionResult.SurvivingMembersSummaryText) ? "None" : expeditionResult.SurvivingMembersSummaryText;
        _lastRunClearedEncountersByCityId[cityId] = string.IsNullOrEmpty(expeditionResult.ClearedEncounterSummaryText) ? "None" : expeditionResult.ClearedEncounterSummaryText;
        _lastRunEventChoiceByCityId[cityId] = string.IsNullOrEmpty(expeditionResult.SelectedEventChoiceText) ? "None" : expeditionResult.SelectedEventChoiceText;
        _lastRunLootBreakdownByCityId[cityId] = string.IsNullOrEmpty(expeditionResult.LootBreakdownSummaryText) ? "None" : expeditionResult.LootBreakdownSummaryText;
        _lastRunRouteByCityId[cityId] = safeRouteSummary;
        _lastRunDungeonByCityId[cityId] = safeDungeonSummary;

        WorldWriteback writeback = BuildWorldWritebackFromExpeditionResult(
            expeditionResult,
            safeResultSummary,
            _lastRunSurvivingMembersByCityId[cityId],
            safeRouteSummary,
            safeDungeonSummary,
            safeLootReturned);
        ApplyWorldWriteback(writeback);

        party.State = PartyState.Idle;
        party.TargetDungeonId = string.Empty;
        party.ActiveRouteId = string.Empty;
        party.DaysRemaining = 0;
        party.DepartureDay = -1;
        party.ProjectedReturnDay = -1;
        party.LastResultSummary = safeResultSummary + promotionEcho + pendingRewardEcho + gearEcho;
        _lastExpeditionResultByCityId[cityId] = safeResultSummary;

        if (!string.IsNullOrEmpty(dungeonId))
        {
            _lastExpeditionResultByDungeonId[dungeonId] = safeResultSummary;
        }

        AppendRecentExpeditionLog("Day " + WorldDayCount + " | " + safeResultSummary + promotionEcho + gearEcho);
    }

    private WorldWriteback BuildWorldWritebackFromExpeditionResult(ExpeditionResult expeditionResult, string resultSummary, string survivingMembersSummary, string routeSummary, string dungeonSummary, int lootReturned)
    {
        string cityId = expeditionResult != null ? expeditionResult.SourceCityId ?? string.Empty : string.Empty;
        string dungeonId = expeditionResult != null ? expeditionResult.DungeonId ?? string.Empty : string.Empty;
        string rewardResourceId = expeditionResult != null ? expeditionResult.RewardResourceId ?? string.Empty : string.Empty;
        bool success = expeditionResult != null && expeditionResult.Success;
        string resultStateKey = expeditionResult != null ? expeditionResult.ResultStateKey ?? string.Empty : string.Empty;
        string chosenRouteId = expeditionResult != null ? expeditionResult.RouteId ?? string.Empty : string.Empty;
        int elapsedDays = expeditionResult != null ? expeditionResult.ElapsedDays : 1;
        string normalizedStateKey = NormalizeWorldWritebackStateKey(resultStateKey, success);
        string cityLabel = ResolveEntityDisplayName(cityId);
        string dungeonLabel = ResolveEntityDisplayName(dungeonId);
        string routeLabel = ResolveRouteDisplayLabel(chosenRouteId, routeSummary);
        int safeElapsedDays = elapsedDays > 0 ? elapsedDays : 1;
        int dayBefore = WorldDayCount;
        int dayAfter = dayBefore + safeElapsedDays;
        string lootSummary = !string.IsNullOrEmpty(rewardResourceId) && lootReturned > 0
            ? rewardResourceId + " x" + lootReturned
            : "None";

        WorldWriteback writeback = new WorldWriteback();
        writeback.RunResultStateKey = normalizedStateKey;
        writeback.SourceCityId = cityId ?? string.Empty;
        writeback.SourceCityLabel = cityLabel;
        writeback.TargetDungeonId = dungeonId ?? string.Empty;
        writeback.TargetDungeonLabel = dungeonLabel;
        writeback.ChosenRouteId = string.IsNullOrEmpty(chosenRouteId) ? string.Empty : chosenRouteId;
        writeback.ChosenRouteLabel = routeLabel;
        writeback.ResultSummaryText = string.IsNullOrEmpty(resultSummary) ? "None" : resultSummary;
        writeback.CityDelta.CityId = writeback.SourceCityId;
        writeback.CityDelta.CityLabel = cityLabel;
        writeback.CityDelta.ResultStateKey = normalizedStateKey;
        writeback.CityDelta.RewardResourceId = rewardResourceId ?? string.Empty;
        writeback.CityDelta.LootReturned = lootReturned;
        writeback.CityDelta.LootSummaryText = lootSummary;
        writeback.CityDelta.PartyOutcomeSummaryText = string.IsNullOrEmpty(survivingMembersSummary) ? "None" : survivingMembersSummary;
        writeback.CityDelta.StockReactionSummaryText = lootReturned > 0
            ? cityLabel + " absorbed " + lootSummary + "."
            : cityLabel + " absorbed no dungeon loot.";
        writeback.CityDelta.SummaryText = BuildCityWritebackSummaryText(cityLabel, normalizedStateKey, lootSummary, writeback.CityDelta.PartyOutcomeSummaryText);
        writeback.DungeonDelta.DungeonId = writeback.TargetDungeonId;
        writeback.DungeonDelta.DungeonLabel = dungeonLabel;
        writeback.DungeonDelta.ResultStateKey = normalizedStateKey;
        writeback.DungeonDelta.RouteId = string.IsNullOrEmpty(chosenRouteId) ? string.Empty : chosenRouteId;
        writeback.DungeonDelta.RouteLabel = routeLabel;
        writeback.DungeonDelta.StatusKey = BuildDungeonStatusKey(normalizedStateKey);
        writeback.DungeonDelta.StatusSummaryText = BuildDungeonStatusSummaryText(dungeonLabel, normalizedStateKey, routeLabel);
        writeback.DungeonDelta.AvailabilitySummaryText = BuildDungeonAvailabilitySummaryText(normalizedStateKey, routeLabel);
        writeback.DungeonDelta.LastOutcomeSummaryText = string.IsNullOrEmpty(dungeonSummary) ? writeback.DungeonDelta.StatusSummaryText : dungeonSummary;
        writeback.Economy.RewardResourceId = rewardResourceId ?? string.Empty;
        writeback.Economy.LootReturned = lootReturned;
        writeback.Economy.LootSummaryText = lootSummary;
        writeback.Economy.SummaryText = lootReturned > 0
            ? lootSummary + " returned to " + cityLabel
            : "No loot returned to " + cityLabel;
        writeback.TimeAdvance.DayBefore = dayBefore;
        writeback.TimeAdvance.DayAfter = dayAfter;
        writeback.TimeAdvance.ElapsedDays = safeElapsedDays;
        writeback.TimeAdvance.SummaryText = BuildTimeAdvanceSummaryText(dayBefore, dayAfter, safeElapsedDays);
        writeback.WritebackSummaryText = BuildWorldWritebackSummaryText(writeback);
        return writeback;
    }

    private void ApplyWorldWriteback(WorldWriteback writeback)
    {
        if (writeback == null)
        {
            return;
        }

        AdvanceWorldTimeForWriteback(writeback.TimeAdvance.ElapsedDays);
        _latestWorldWriteback = writeback;

        if (!string.IsNullOrEmpty(writeback.SourceCityId))
        {
            _latestWorldWritebackByCityId[writeback.SourceCityId] = writeback.CityDelta.SummaryText;
        }

        if (!string.IsNullOrEmpty(writeback.TargetDungeonId))
        {
            _latestWorldWritebackByDungeonId[writeback.TargetDungeonId] = writeback.WritebackSummaryText;
            _dungeonStatusSummaryByDungeonId[writeback.TargetDungeonId] = writeback.DungeonDelta.StatusSummaryText;
            _dungeonAvailabilitySummaryByDungeonId[writeback.TargetDungeonId] = writeback.DungeonDelta.AvailabilitySummaryText;
            _dungeonLastOutcomeSummaryByDungeonId[writeback.TargetDungeonId] = writeback.DungeonDelta.LastOutcomeSummaryText;
        }

        AppendWorldWritebackLog(WorldEventRecordType.RunWriteback, writeback);
        AppendWorldWritebackLog(WorldEventRecordType.CityDelta, writeback);
        AppendWorldWritebackLog(WorldEventRecordType.DungeonDelta, writeback);
    }

    private void AdvanceWorldTimeForWriteback(int elapsedDays)
    {
        if (elapsedDays > 0)
        {
            WorldDayCount += elapsedDays;
        }
    }

    private void AppendWorldWritebackLog(WorldEventRecordType eventType, WorldWriteback writeback)
    {
        if (writeback == null)
        {
            return;
        }

        WorldWritebackLogRecord record = new WorldWritebackLogRecord();
        record.EventType = eventType;
        record.DayIndex = writeback.TimeAdvance.DayAfter;
        record.CityId = writeback.SourceCityId;
        record.DungeonId = writeback.TargetDungeonId;
        record.ResultStateKey = writeback.RunResultStateKey;
        record.DeltaSummary = eventType == WorldEventRecordType.CityDelta
            ? writeback.CityDelta.SummaryText
            : eventType == WorldEventRecordType.DungeonDelta
                ? writeback.DungeonDelta.StatusSummaryText
                : writeback.WritebackSummaryText;
        record.DisplayText = eventType == WorldEventRecordType.CityDelta
            ? "Day " + record.DayIndex + " | City writeback: " + writeback.CityDelta.SummaryText
            : eventType == WorldEventRecordType.DungeonDelta
                ? "Day " + record.DayIndex + " | Dungeon writeback: " + writeback.DungeonDelta.StatusSummaryText
                : "Day " + record.DayIndex + " | World writeback: " + writeback.WritebackSummaryText;
        _recentWorldWritebackLogs.Insert(0, record);
        while (_recentWorldWritebackLogs.Count > RecentWorldWritebackLogLimit)
        {
            _recentWorldWritebackLogs.RemoveAt(_recentWorldWritebackLogs.Count - 1);
        }
    }

    private string BuildWorldWritebackSummaryText(WorldWriteback writeback)
    {
        string stateLabel = BuildWorldWritebackStateLabel(writeback.RunResultStateKey);
        string routePart = writeback.ChosenRouteLabel == "None" ? string.Empty : " via " + writeback.ChosenRouteLabel;
        string lootPart = writeback.Economy.LootSummaryText == "None" ? "no dungeon loot" : writeback.Economy.LootSummaryText;
        return writeback.SourceCityLabel + " absorbed " + stateLabel + routePart + " | " + lootPart + " | " + writeback.TimeAdvance.SummaryText + " | " + writeback.DungeonDelta.StatusSummaryText;
    }

    private string BuildCityWritebackSummaryText(string cityLabel, string stateKey, string lootSummary, string partyOutcomeSummary)
    {
        string lootPart = lootSummary == "None" ? "No loot returned" : lootSummary + " returned";
        string partyPart = string.IsNullOrEmpty(partyOutcomeSummary) || partyOutcomeSummary == "None" ? "party returned" : partyOutcomeSummary;
        return cityLabel + " | " + BuildWorldWritebackStateLabel(stateKey) + " | " + lootPart + " | " + partyPart;
    }

    private string BuildDungeonStatusKey(string stateKey)
    {
        if (stateKey == "clear")
        {
            return "stabilized";
        }

        if (stateKey == "retreat")
        {
            return "contested";
        }

        return stateKey == "defeat" ? "threatened" : "observed";
    }

    private string BuildDungeonStatusSummaryText(string dungeonLabel, string stateKey, string routeLabel)
    {
        string routePart = routeLabel == "None" ? string.Empty : " via " + routeLabel;
        if (stateKey == "clear")
        {
            return dungeonLabel + " stabilized after a successful run" + routePart + ".";
        }

        if (stateKey == "retreat")
        {
            return dungeonLabel + " remains contested after the retreat" + routePart + ".";
        }

        if (stateKey == "defeat")
        {
            return dungeonLabel + " shows elevated threat after the failed run" + routePart + ".";
        }

        return dungeonLabel + " recorded a new expedition outcome" + routePart + ".";
    }

    private string BuildDungeonAvailabilitySummaryText(string stateKey, string routeLabel)
    {
        string routePart = routeLabel == "None" ? string.Empty : " | Last route: " + routeLabel;
        if (stateKey == "clear")
        {
            return "Available after stabilization" + routePart;
        }

        if (stateKey == "retreat")
        {
            return "Available, but caution is advised" + routePart;
        }

        if (stateKey == "defeat")
        {
            return "Available with elevated threat" + routePart;
        }

        return "Available" + routePart;
    }

    private string BuildTimeAdvanceSummaryText(int dayBefore, int dayAfter, int elapsedDays)
    {
        return "Day " + dayBefore + " -> Day " + dayAfter + " (+" + elapsedDays + "d)";
    }

    private string BuildWorldWritebackStateLabel(string stateKey)
    {
        if (stateKey == "clear")
        {
            return "Clear";
        }

        if (stateKey == "retreat")
        {
            return "Retreat";
        }

        if (stateKey == "defeat")
        {
            return "Defeat";
        }

        return "Run Complete";
    }

    private string NormalizeWorldWritebackStateKey(string stateKey, bool success)
    {
        string normalized = string.IsNullOrWhiteSpace(stateKey) ? string.Empty : stateKey.Trim().ToLowerInvariant();
        if (normalized == "victory" || normalized == "success" || normalized == "run_clear")
        {
            return "clear";
        }

        if (normalized == "run_retreat")
        {
            return "retreat";
        }

        if (normalized == "run_defeat")
        {
            return "defeat";
        }

        if (normalized == "clear" || normalized == "retreat" || normalized == "defeat")
        {
            return normalized;
        }

        return success ? "clear" : "defeat";
    }

    private global::WorldWriteback CreatePublicWorldWriteback(WorldWriteback writeback, ExpeditionResult expeditionResult)
    {
        if (writeback == null)
        {
            return expeditionResult != null && !string.IsNullOrEmpty(expeditionResult.SourceCityId)
                ? CreateStoredCityWorldWriteback(expeditionResult.SourceCityId, expeditionResult)
                : new global::WorldWriteback();
        }

        string cityId = writeback.SourceCityId ?? string.Empty;
        string routeSummary = GetLastRunRouteSummaryForCity(cityId);
        string dungeonSummary = GetLastRunDungeonSummaryForCity(cityId);
        string survivingMembersSummary = GetLastRunSurvivingMembersSummaryForCity(cityId);
        string clearedEncountersSummary = GetLastRunClearedEncountersSummaryForCity(cityId);
        string eventChoiceSummary = GetLastRunEventChoiceSummaryForCity(cityId);
        string lootBreakdownSummary = GetLastRunLootBreakdownSummaryForCity(cityId);
        string lootSummary = IsMeaningfulWorldWritebackText(writeback.CityDelta.LootSummaryText)
            ? writeback.CityDelta.LootSummaryText
            : GetLastRunLootSummaryForCity(cityId);
        string routeLabel = IsMeaningfulWorldWritebackText(writeback.ChosenRouteLabel)
            ? writeback.ChosenRouteLabel
            : ResolveRouteDisplayLabel(writeback.ChosenRouteId, routeSummary);
        string dungeonLabel = IsMeaningfulWorldWritebackText(writeback.TargetDungeonLabel)
            ? writeback.TargetDungeonLabel
            : ResolveEntityDisplayName(writeback.TargetDungeonId);
        string resultSummary = IsMeaningfulWorldWritebackText(writeback.ResultSummaryText)
            ? writeback.ResultSummaryText
            : ChooseStoredResultSummary(cityId, expeditionResult);
        string writebackSummary = IsMeaningfulWorldWritebackText(writeback.WritebackSummaryText)
            ? writeback.WritebackSummaryText
            : GetStoredWorldWritebackSummaryForCity(cityId);
        string dungeonStatusSummary = IsMeaningfulWorldWritebackText(writeback.DungeonDelta.StatusSummaryText)
            ? writeback.DungeonDelta.StatusSummaryText
            : GetDungeonWorldStatusText(writeback.TargetDungeonId);
        string dungeonAvailabilitySummary = IsMeaningfulWorldWritebackText(writeback.DungeonDelta.AvailabilitySummaryText)
            ? writeback.DungeonDelta.AvailabilitySummaryText
            : GetDungeonWorldAvailabilityText(writeback.TargetDungeonId);
        string dungeonLastOutcomeSummary = IsMeaningfulWorldWritebackText(writeback.DungeonDelta.LastOutcomeSummaryText)
            ? writeback.DungeonDelta.LastOutcomeSummaryText
            : GetDungeonLastWorldOutcomeText(writeback.TargetDungeonId);
        string cityLabel = IsMeaningfulWorldWritebackText(writeback.SourceCityLabel)
            ? writeback.SourceCityLabel
            : ResolveEntityDisplayName(cityId);
        string stockReactionSummary = IsMeaningfulWorldWritebackText(writeback.CityDelta.StockReactionSummaryText)
            ? writeback.CityDelta.StockReactionSummaryText
            : BuildStoredStockReactionSummary(cityLabel, lootSummary);
        string citySummary = IsMeaningfulWorldWritebackText(writeback.CityDelta.SummaryText)
            ? writeback.CityDelta.SummaryText
            : BuildCityWritebackSummaryText(cityLabel, writeback.RunResultStateKey, lootSummary, survivingMembersSummary);

        global::WorldWriteback result = new global::WorldWriteback();
        result.RunResultStateKey = writeback.RunResultStateKey ?? string.Empty;
        result.Success = expeditionResult != null ? expeditionResult.Success : IsSuccessfulWorldWritebackState(result.RunResultStateKey);
        result.SourceCityId = cityId;
        result.SourceCityLabel = cityLabel;
        result.TargetDungeonId = writeback.TargetDungeonId ?? string.Empty;
        result.TargetDungeonLabel = dungeonLabel;
        result.ChosenRouteId = writeback.ChosenRouteId ?? string.Empty;
        result.ChosenRouteLabel = routeLabel;
        result.RouteSummaryText = routeSummary != "None" ? routeSummary : routeLabel;
        result.DungeonSummaryText = dungeonSummary != "None" ? dungeonSummary : dungeonLabel;
        result.ResultSummaryText = resultSummary;
        result.WritebackSummaryText = writebackSummary;
        result.DungeonStatusKey = IsMeaningfulWorldWritebackText(writeback.DungeonDelta.StatusKey)
            ? writeback.DungeonDelta.StatusKey
            : BuildDungeonStatusKey(result.RunResultStateKey);
        result.DungeonStatusSummaryText = dungeonStatusSummary;
        result.DungeonAvailabilitySummaryText = dungeonAvailabilitySummary;
        result.DungeonLastOutcomeSummaryText = dungeonLastOutcomeSummary;
        result.RewardResourceId = writeback.CityDelta.RewardResourceId ?? string.Empty;
        result.LootReturned = writeback.CityDelta.LootReturned;
        result.LootSummaryText = lootSummary;
        result.SurvivingMembersSummaryText = survivingMembersSummary;
        result.ClearedEncountersSummaryText = clearedEncountersSummary;
        result.EventChoiceSummaryText = eventChoiceSummary;
        result.LootBreakdownSummaryText = lootBreakdownSummary;
        result.EconomySummaryText = IsMeaningfulWorldWritebackText(writeback.Economy.SummaryText)
            ? writeback.Economy.SummaryText
            : stockReactionSummary;
        result.DayBefore = writeback.TimeAdvance.DayBefore;
        result.DayAfter = writeback.TimeAdvance.DayAfter;
        result.ElapsedDays = writeback.TimeAdvance.ElapsedDays > 0
            ? writeback.TimeAdvance.ElapsedDays
            : expeditionResult != null && expeditionResult.ElapsedDays > 0
                ? expeditionResult.ElapsedDays
                : 1;
        result.TimeAdvanceSummaryText = IsMeaningfulWorldWritebackText(writeback.TimeAdvance.SummaryText)
            ? writeback.TimeAdvance.SummaryText
            : result.DayBefore >= 0 && result.DayAfter >= 0
                ? BuildTimeAdvanceSummaryText(result.DayBefore, result.DayAfter, result.ElapsedDays)
                : "None";
        result.CityWriteback = CreatePublicCityWriteback(
            cityId,
            cityLabel,
            result.RunResultStateKey,
            result.RewardResourceId,
            result.LootReturned,
            result.LootSummaryText,
            result.SurvivingMembersSummaryText,
            stockReactionSummary,
            citySummary);
        return result;
    }

    private global::WorldWriteback CreateStoredCityWorldWriteback(string cityId, ExpeditionResult expeditionResult)
    {
        if (string.IsNullOrEmpty(cityId) || expeditionResult == null)
        {
            return new global::WorldWriteback();
        }

        string cityLabel = ResolveEntityDisplayName(cityId);
        string normalizedStateKey = NormalizeWorldWritebackStateKey(expeditionResult.ResultStateKey, expeditionResult.Success);
        string dungeonId = expeditionResult.DungeonId ?? string.Empty;
        string dungeonLabel = ResolveEntityDisplayName(dungeonId);
        string routeSummary = GetLastRunRouteSummaryForCity(cityId);
        string routeLabel = routeSummary != "None"
            ? routeSummary
            : ResolveRouteDisplayLabel(expeditionResult.RouteId, expeditionResult.RouteLabel);
        string dungeonSummary = GetLastRunDungeonSummaryForCity(cityId);
        string lootSummary = GetLastRunLootSummaryForCity(cityId);
        if (!IsMeaningfulWorldWritebackText(lootSummary) &&
            expeditionResult.Success &&
            !string.IsNullOrEmpty(expeditionResult.RewardResourceId) &&
            expeditionResult.ReturnedLootAmount > 0)
        {
            lootSummary = expeditionResult.RewardResourceId + " x" + expeditionResult.ReturnedLootAmount;
        }

        string survivingMembersSummary = GetLastRunSurvivingMembersSummaryForCity(cityId);
        string clearedEncountersSummary = GetLastRunClearedEncountersSummaryForCity(cityId);
        string eventChoiceSummary = GetLastRunEventChoiceSummaryForCity(cityId);
        string lootBreakdownSummary = GetLastRunLootBreakdownSummaryForCity(cityId);
        string stockReactionSummary = BuildStoredStockReactionSummary(cityLabel, lootSummary);
        string citySummary = BuildCityWritebackSummaryText(cityLabel, normalizedStateKey, lootSummary, survivingMembersSummary);

        global::WorldWriteback result = new global::WorldWriteback();
        result.RunResultStateKey = normalizedStateKey;
        result.Success = expeditionResult.Success;
        result.SourceCityId = cityId;
        result.SourceCityLabel = cityLabel;
        result.TargetDungeonId = dungeonId;
        result.TargetDungeonLabel = dungeonSummary != "None" ? dungeonSummary : dungeonLabel;
        result.ChosenRouteId = expeditionResult.RouteId ?? string.Empty;
        result.ChosenRouteLabel = routeLabel;
        result.RouteSummaryText = routeSummary != "None" ? routeSummary : routeLabel;
        result.DungeonSummaryText = dungeonSummary != "None" ? dungeonSummary : dungeonLabel;
        result.ResultSummaryText = ChooseStoredResultSummary(cityId, expeditionResult);
        result.WritebackSummaryText = GetStoredWorldWritebackSummaryForCity(cityId);
        result.DungeonStatusKey = BuildDungeonStatusKey(normalizedStateKey);
        result.DungeonStatusSummaryText = GetDungeonWorldStatusText(dungeonId);
        result.DungeonAvailabilitySummaryText = GetDungeonWorldAvailabilityText(dungeonId);
        result.DungeonLastOutcomeSummaryText = GetDungeonLastWorldOutcomeText(dungeonId);
        result.RewardResourceId = expeditionResult.RewardResourceId ?? string.Empty;
        result.LootReturned = expeditionResult.Success && expeditionResult.ReturnedLootAmount > 0 ? expeditionResult.ReturnedLootAmount : 0;
        result.LootSummaryText = IsMeaningfulWorldWritebackText(lootSummary) ? lootSummary : "None";
        result.SurvivingMembersSummaryText = survivingMembersSummary;
        result.ClearedEncountersSummaryText = clearedEncountersSummary;
        result.EventChoiceSummaryText = eventChoiceSummary;
        result.LootBreakdownSummaryText = lootBreakdownSummary;
        result.EconomySummaryText = stockReactionSummary;
        result.DayBefore = -1;
        result.DayAfter = -1;
        result.ElapsedDays = expeditionResult.ElapsedDays > 0 ? expeditionResult.ElapsedDays : 1;
        result.TimeAdvanceSummaryText = "None";
        result.CityWriteback = CreatePublicCityWriteback(
            cityId,
            cityLabel,
            result.RunResultStateKey,
            result.RewardResourceId,
            result.LootReturned,
            result.LootSummaryText,
            result.SurvivingMembersSummaryText,
            stockReactionSummary,
            citySummary);
        return result;
    }

    private global::CityWriteback CreatePublicCityWriteback(
        string cityId,
        string cityLabel,
        string resultStateKey,
        string rewardResourceId,
        int lootReturned,
        string lootSummary,
        string partyOutcomeSummary,
        string stockReactionSummary,
        string summaryText)
    {
        global::CityWriteback result = new global::CityWriteback();
        result.CityId = cityId ?? string.Empty;
        result.CityLabel = string.IsNullOrEmpty(cityLabel) ? "None" : cityLabel;
        result.ResultStateKey = resultStateKey ?? string.Empty;
        result.RewardResourceId = rewardResourceId ?? string.Empty;
        result.LootReturned = lootReturned > 0 ? lootReturned : 0;
        result.LootSummaryText = IsMeaningfulWorldWritebackText(lootSummary) ? lootSummary : "None";
        result.PartyOutcomeSummaryText = IsMeaningfulWorldWritebackText(partyOutcomeSummary) ? partyOutcomeSummary : "None";
        result.StockReactionSummaryText = IsMeaningfulWorldWritebackText(stockReactionSummary) ? stockReactionSummary : "None";
        result.SummaryText = IsMeaningfulWorldWritebackText(summaryText) ? summaryText : "None";
        return result;
    }

    private string ChooseStoredResultSummary(string cityId, ExpeditionResult expeditionResult)
    {
        if (expeditionResult != null && IsMeaningfulWorldWritebackText(expeditionResult.ResultSummaryText))
        {
            return expeditionResult.ResultSummaryText;
        }

        return !string.IsNullOrEmpty(cityId) && _lastExpeditionResultByCityId.TryGetValue(cityId, out string value) && IsMeaningfulWorldWritebackText(value)
            ? value
            : "None";
    }

    private string GetStoredWorldWritebackSummaryForCity(string cityId)
    {
        return !string.IsNullOrEmpty(cityId) && _latestWorldWritebackByCityId.TryGetValue(cityId, out string value) && IsMeaningfulWorldWritebackText(value)
            ? value
            : "None";
    }

    private string BuildStoredStockReactionSummary(string cityLabel, string lootSummary)
    {
        return IsMeaningfulWorldWritebackText(lootSummary)
            ? cityLabel + " absorbed " + lootSummary + "."
            : cityLabel + " absorbed no dungeon loot.";
    }

    private ExpeditionOutcome CreatePublicExpeditionOutcome(ExpeditionResult expeditionResult)
    {
        if (expeditionResult == null)
        {
            return new ExpeditionOutcome();
        }

        ExpeditionOutcome result = ResultPipeline.BuildExpeditionOutcome(
            expeditionResult.SourceCityId,
            expeditionResult.SourceCityLabel,
            expeditionResult.DungeonId,
            expeditionResult.DungeonLabel,
            expeditionResult.RewardResourceId,
            expeditionResult.ReturnedLootAmount,
            expeditionResult.Success,
            expeditionResult.ResultStateKey,
            expeditionResult.ResultSummaryText,
            expeditionResult.SurvivingMembersSummaryText,
            expeditionResult.ClearedEncounterSummaryText,
            expeditionResult.SelectedEventChoiceText,
            expeditionResult.LootBreakdownSummaryText,
            expeditionResult.SelectedRouteSummaryText,
            expeditionResult.DungeonSummaryText);
        result.TotalTurnsTaken = expeditionResult.TotalTurnsTaken;
        result.ClearedEncounterCount = expeditionResult.ClearedEncounterCount;
        result.OpenedChestCount = expeditionResult.OpenedChestCount;
        result.SurvivingMemberCount = expeditionResult.BattleResult != null ? expeditionResult.BattleResult.SurvivingMemberCount : 0;
        result.KnockedOutMemberCount = expeditionResult.BattleResult != null ? expeditionResult.BattleResult.KnockedOutMemberCount : 0;
        result.EliteDefeated = expeditionResult.EliteDefeated;
        result.MissionObjectiveText = ChooseMeaningfulWorldText(expeditionResult.KeyEncounterSummaryText, expeditionResult.DungeonSummaryText);
        result.MissionRelevanceText = ChooseMeaningfulWorldText(expeditionResult.WorldWritebackSummaryText, expeditionResult.NextSuggestedActionText);
        result.RiskRewardContextText = ChooseMeaningfulWorldText(expeditionResult.BattleContextSummaryText, expeditionResult.ReturnedLootSummaryText);
        result.RunPathSummaryText = ChooseMeaningfulWorldText(expeditionResult.RoomPathSummaryText, expeditionResult.SelectedRouteSummaryText);
        result.PartyConditionText = ChooseMeaningfulWorldText(expeditionResult.PartyConditionText, expeditionResult.InjurySummaryText);
        result.PartyHpSummaryText = expeditionResult.PartyHpSummaryText;
        result.EliteSummaryText = ChooseMeaningfulWorldText(expeditionResult.EliteOutcomeSummaryText, expeditionResult.EliteRewardLabel);
        return result;
    }

    private OutcomeReadback CreatePublicOutcomeReadback(string cityId, ExpeditionResult expeditionResult, global::WorldWriteback worldWriteback)
    {
        ExpeditionResult safeExpeditionResult = expeditionResult ?? new ExpeditionResult();
        global::WorldWriteback safeWorldWriteback = worldWriteback ?? new global::WorldWriteback();
        global::CityWriteback safeCityWriteback = safeWorldWriteback.CityWriteback ?? new global::CityWriteback();
        string resolvedCityId = !string.IsNullOrEmpty(cityId)
            ? cityId
            : !string.IsNullOrEmpty(safeExpeditionResult.SourceCityId)
                ? safeExpeditionResult.SourceCityId
                : safeWorldWriteback.SourceCityId;
        string resolvedCityLabel = IsMeaningfulWorldWritebackText(safeWorldWriteback.SourceCityLabel)
            ? safeWorldWriteback.SourceCityLabel
            : IsMeaningfulWorldWritebackText(safeExpeditionResult.SourceCityLabel)
                ? safeExpeditionResult.SourceCityLabel
                : ResolveEntityDisplayName(resolvedCityId);
        string resultStateKey = !string.IsNullOrEmpty(safeWorldWriteback.RunResultStateKey)
            ? safeWorldWriteback.RunResultStateKey
            : safeExpeditionResult.ResultStateKey;
        string followUpHint = ChooseMeaningfulWorldText(safeExpeditionResult.NextPrepFollowUpSummaryText, safeCityWriteback.FollowUpHintText);

        return ResultPipeline.BuildOutcomeReadback(
            safeExpeditionResult,
            safeCityWriteback,
            safeWorldWriteback,
            safeWorldWriteback.WritebackSummaryText,
            ChooseMeaningfulWorldText(safeExpeditionResult.LatestReturnAftermathSummaryText, safeWorldWriteback.WritebackSummaryText),
            resolvedCityId,
            resolvedCityLabel,
            resultStateKey,
            ChooseMeaningfulWorldText(safeCityWriteback.SummaryText, safeExpeditionResult.ResultSummaryText),
            safeExpeditionResult.NextSuggestedActionText,
            followUpHint,
            ChooseMeaningfulWorldText(safeExpeditionResult.ResultSummaryText, safeWorldWriteback.ResultSummaryText),
            safeWorldWriteback.SurvivingMembersSummaryText,
            safeWorldWriteback.ClearedEncountersSummaryText,
            safeWorldWriteback.EventChoiceSummaryText,
            safeWorldWriteback.LootBreakdownSummaryText,
            safeWorldWriteback.DungeonSummaryText,
            safeWorldWriteback.RouteSummaryText,
            FormatOptionalCountText(safeCityWriteback.StockBefore),
            FormatOptionalCountText(safeCityWriteback.StockAfter),
            FormatSignedCountText(safeCityWriteback.StockDelta),
            GetLastRunGearRewardSummaryForCity(resolvedCityId),
            GetLastRunEquipSwapSummaryForCity(resolvedCityId),
            GetLastRunGearContinuitySummaryForCity(resolvedCityId),
            GetRecentExpeditionLogText(0),
            GetRecentExpeditionLogText(1),
            GetRecentExpeditionLogText(2),
            GetRecentWorldWritebackLogText(0),
            GetRecentWorldWritebackLogText(1),
            GetRecentWorldWritebackLogText(2));
    }

    private string ChooseMeaningfulWorldText(string primary, string fallback)
    {
        return IsMeaningfulWorldWritebackText(primary)
            ? primary
            : IsMeaningfulWorldWritebackText(fallback)
                ? fallback
                : "None";
    }

    private string FormatOptionalCountText(int value)
    {
        return value > 0 ? value.ToString() : "None";
    }

    private string FormatSignedCountText(int value)
    {
        return value != 0 ? value.ToString() : "None";
    }

    private bool IsSuccessfulWorldWritebackState(string stateKey)
    {
        return NormalizeWorldWritebackStateKey(stateKey, false) == "clear";
    }

    private bool IsMeaningfulWorldWritebackText(string value)
    {
        return !string.IsNullOrEmpty(value) && value != "None";
    }

    private string ResolveEntityDisplayName(string entityId)
    {
        return !string.IsNullOrEmpty(entityId) && TryGetEntity(entityId, out WorldEntityData entity) && !string.IsNullOrEmpty(entity.DisplayName)
            ? entity.DisplayName
            : (string.IsNullOrEmpty(entityId) ? "None" : entityId);
    }

    private string ResolveRouteDisplayLabel(string chosenRouteId, string routeSummary)
    {
        if (!string.IsNullOrWhiteSpace(routeSummary) && routeSummary != "None")
        {
            return routeSummary;
        }

        if (string.IsNullOrWhiteSpace(chosenRouteId))
        {
            return "None";
        }

        string normalized = chosenRouteId.Trim().ToLowerInvariant();
        if (normalized == "safe")
        {
            return "Safe Route";
        }

        if (normalized == "risky")
        {
            return "Risky Route";
        }

        return chosenRouteId;
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

    private PartyRuntimeData FindPartyById(string partyId)
    {
        if (string.IsNullOrEmpty(partyId))
        {
            return null;
        }

        for (int i = 0; i < _parties.Count; i++)
        {
            PartyRuntimeData party = _parties[i];
            if (party != null && party.PartyId == partyId)
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

    private void UpdatePartyDerivedStats(PartyRuntimeData party)
    {
        if (party == null)
        {
            return;
        }

        party.Power = PrototypeRpgPartyCatalog.ResolveDerivedPower(party.ArchetypeId, party.PromotionStateId)
            + CalculatePartyLevelPowerBonus(party)
            + CalculatePartyEquipmentPowerBonus(party);
        party.CarryCapacity = PrototypeRpgPartyCatalog.ResolveDerivedCarryCapacity(party.ArchetypeId, party.PromotionStateId);
        RefreshPartyWorldSummaryCache(party);
    }

    private void RefreshPartyWorldSummaryCache(PartyRuntimeData party)
    {
        if (party == null)
        {
            return;
        }

        string archetypeLabel = PrototypeRpgPartyCatalog.GetArchetypeLabel(party.ArchetypeId);
        string promotionLabel = PrototypeRpgPartyCatalog.GetPromotionStateLabel(party.PromotionStateId);
        if (!string.IsNullOrEmpty(archetypeLabel) && !string.IsNullOrEmpty(promotionLabel))
        {
            party.IdentitySummaryText = archetypeLabel + " / " + promotionLabel;
        }
        else if (!string.IsNullOrEmpty(archetypeLabel))
        {
            party.IdentitySummaryText = archetypeLabel;
        }
        else if (!string.IsNullOrEmpty(promotionLabel))
        {
            party.IdentitySummaryText = promotionLabel;
        }
        else
        {
            party.IdentitySummaryText = !string.IsNullOrEmpty(party.DisplayName) ? party.DisplayName : "None";
        }

        party.RoleSummaryText = party.Members != null && party.Members.Length > 0
            ? PrototypeRpgRoleIdentity.BuildPartyRoleSummary(
                party.Members,
                member => member != null ? member.DisplayName : string.Empty,
                member => member != null ? member.RoleTag : string.Empty)
            : "None";
        party.LoadoutSummaryText = BuildPartyLoadoutSummaryText(party);
    }

    private void InitializePartyMembers(PartyRuntimeData party)
    {
        if (party == null)
        {
            return;
        }

        PrototypeRpgPartyDefinition partyDefinition = PrototypeRpgPartyCatalog.CreateRuntimeParty(
            party.PartyId,
            party.ArchetypeId,
            party.PromotionStateId,
            party.DisplayName);
        PrototypeRpgPartyMemberDefinition[] members = partyDefinition != null
            ? partyDefinition.Members ?? System.Array.Empty<PrototypeRpgPartyMemberDefinition>()
            : System.Array.Empty<PrototypeRpgPartyMemberDefinition>();
        if (members.Length <= 0)
        {
            party.Members = System.Array.Empty<PartyMemberProgressionData>();
            return;
        }

        PartyMemberProgressionData[] progressionMembers = new PartyMemberProgressionData[members.Length];
        for (int i = 0; i < members.Length; i++)
        {
            PrototypeRpgPartyMemberDefinition member = members[i];
            progressionMembers[i] = new PartyMemberProgressionData(
                member != null ? member.MemberId : string.Empty,
                member != null ? member.DisplayName : string.Empty,
                member != null ? member.RoleTag : string.Empty);
        }

        party.Members = progressionMembers;
        RefreshPartyInventorySummary(party);
    }

    private int CalculatePartyLevelPowerBonus(PartyRuntimeData party)
    {
        if (party == null || party.Members == null || party.Members.Length <= 0)
        {
            return 0;
        }

        int levelSteps = 0;
        for (int i = 0; i < party.Members.Length; i++)
        {
            PartyMemberProgressionData member = party.Members[i];
            if (member == null)
            {
                continue;
            }

            levelSteps += Mathf.Max(0, member.Level - PrototypeRpgMemberProgressionRules.GetStartingLevel());
        }

        return levelSteps / 2;
    }

    private int CalculatePartyEquipmentPowerBonus(PartyRuntimeData party)
    {
        if (party == null || party.Members == null || party.Members.Length <= 0)
        {
            return 0;
        }

        int totalEquipmentScore = 0;
        for (int i = 0; i < party.Members.Length; i++)
        {
            PartyMemberProgressionData member = party.Members[i];
            if (member == null)
            {
                continue;
            }

            ResolveMemberEquipmentBonuses(
                party,
                member,
                out int maxHpBonus,
                out int attackBonus,
                out int defenseBonus,
                out int speedBonus,
                out int skillPowerBonus);
            totalEquipmentScore += maxHpBonus +
                                   (attackBonus * 2) +
                                   (defenseBonus * 2) +
                                   speedBonus +
                                   (skillPowerBonus * 2);
        }

        return totalEquipmentScore / 8;
    }

    private PartyMemberProgressionData FindPartyMemberProgression(string partyId, string memberId)
    {
        PartyRuntimeData party = FindPartyById(partyId);
        if (party == null || party.Members == null || party.Members.Length <= 0)
        {
            return null;
        }

        for (int i = 0; i < party.Members.Length; i++)
        {
            PartyMemberProgressionData member = party.Members[i];
            if (member != null && member.MemberId == memberId)
            {
                return member;
            }
        }

        return null;
    }

    private void ApplyMemberProgressionResults(PartyRuntimeData party, PrototypeRpgMemberProgressionResult[] memberResults)
    {
        if (party == null || memberResults == null || memberResults.Length <= 0)
        {
            return;
        }

        if (party.Members == null || party.Members.Length <= 0)
        {
            InitializePartyMembers(party);
        }

        for (int i = 0; i < memberResults.Length; i++)
        {
            PrototypeRpgMemberProgressionResult result = memberResults[i];
            if (result == null)
            {
                continue;
            }

            PartyMemberProgressionData member = FindPartyMemberProgression(party.PartyId, result.MemberId);
            if (member == null)
            {
                continue;
            }

            member.Level = result.LevelAfter > 0 ? result.LevelAfter : member.Level;
            member.Experience = result.ExperienceAfter > 0 ? result.ExperienceAfter : 0;
        }

        UpdatePartyDerivedStats(party);
        RefreshPartyInventorySummary(party);
    }

    private void ApplyPendingRewardBundles(PartyRuntimeData party, PrototypeRpgRewardBundle[] rewardBundles)
    {
        if (party == null || rewardBundles == null || rewardBundles.Length <= 0)
        {
            return;
        }

        for (int i = 0; i < rewardBundles.Length; i++)
        {
            PrototypeRpgRewardBundle bundle = rewardBundles[i];
            if (bundle == null || string.IsNullOrWhiteSpace(bundle.RewardId) || bundle.Amount <= 0)
            {
                continue;
            }

            string rewardId = bundle.RewardId.Trim().ToLowerInvariant();
            if (!party.PendingRewardInventory.ContainsKey(rewardId))
            {
                party.PendingRewardInventory.Add(rewardId, 0);
            }

            party.PendingRewardInventory[rewardId] += bundle.Amount;
        }

        party.PendingRewardSummaryText = BuildPendingRewardSummaryText(party.PendingRewardInventory);
    }

    private string BuildPendingRewardSummaryText(Dictionary<string, int> rewardInventory)
    {
        if (rewardInventory == null || rewardInventory.Count <= 0)
        {
            return "None";
        }

        List<PrototypeRpgRewardBundle> bundles = new List<PrototypeRpgRewardBundle>();
        foreach (KeyValuePair<string, int> pair in rewardInventory)
        {
            if (string.IsNullOrWhiteSpace(pair.Key) || pair.Value <= 0)
            {
                continue;
            }

            bundles.Add(new PrototypeRpgRewardBundle
            {
                RewardId = pair.Key,
                RewardLabel = ResolveRewardLabel(pair.Key),
                Amount = pair.Value
            });
        }

        return PrototypeRpgMemberProgressionRules.BuildRewardBundleSummaryText(bundles.ToArray());
    }

    private void ResolvePartyMemberEquipmentBonuses(
        string partyId,
        string memberId,
        out int maxHpBonus,
        out int attackBonus,
        out int defenseBonus,
        out int speedBonus,
        out int skillPowerBonus)
    {
        PartyRuntimeData party = FindPartyById(partyId);
        PartyMemberProgressionData member = FindPartyMemberProgression(partyId, memberId);
        ResolveMemberEquipmentBonuses(
            party,
            member,
            out maxHpBonus,
            out attackBonus,
            out defenseBonus,
            out speedBonus,
            out skillPowerBonus);
    }

    private void ResolveMemberEquipmentBonuses(
        PartyRuntimeData party,
        PartyMemberProgressionData member,
        out int maxHpBonus,
        out int attackBonus,
        out int defenseBonus,
        out int speedBonus,
        out int skillPowerBonus)
    {
        maxHpBonus = 0;
        attackBonus = 0;
        defenseBonus = 0;
        speedBonus = 0;
        skillPowerBonus = 0;

        if (member == null)
        {
            return;
        }

        string[] slotKeys = PrototypeRpgEquipmentSlotKeys.OrderedKeys;
        for (int i = 0; i < slotKeys.Length; i++)
        {
            PrototypeRpgEquipmentDefinition definition = GetEquippedDefinition(party, member, slotKeys[i]);
            if (definition == null)
            {
                continue;
            }

            maxHpBonus += definition.MaxHpBonus;
            attackBonus += definition.AttackBonus;
            defenseBonus += definition.DefenseBonus;
            speedBonus += definition.SpeedBonus;
            skillPowerBonus += definition.SkillPowerBonus;
        }
    }

    private void ApplyEquipmentRewardDrops(PartyRuntimeData party, ExpeditionResult expeditionResult)
    {
        if (party == null)
        {
            return;
        }

        if (party.Members == null || party.Members.Length <= 0)
        {
            InitializePartyMembers(party);
        }

        party.LatestGearRewardSummaryText = "None";
        party.LatestEquipSwapSummaryText = "None";
        RefreshPartyInventorySummary(party);
        party.LatestGearContinuitySummaryText = party.InventorySummaryText;

        if (expeditionResult == null)
        {
            UpdatePartyDerivedStats(party);
            return;
        }

        if (!expeditionResult.Success)
        {
            expeditionResult.GearRewardCandidateSummaryText = "None";
            expeditionResult.EquipSwapChoiceSummaryText = "None";
            expeditionResult.GearCarryContinuitySummaryText = party.InventorySummaryText;
            return;
        }

        int rewardCount = ResolveEquipmentRewardCount(expeditionResult);
        if (rewardCount <= 0)
        {
            expeditionResult.GearRewardCandidateSummaryText = "None";
            expeditionResult.EquipSwapChoiceSummaryText = "None";
            expeditionResult.GearCarryContinuitySummaryText = party.InventorySummaryText;
            return;
        }

        List<string> rewardParts = new List<string>();
        List<string> equipParts = new List<string>();
        int autoEquipCount = 0;
        HashSet<string> reservedMemberIds = new HashSet<string>();
        HashSet<string> reservedMemberSlotKeys = new HashSet<string>();

        for (int i = 0; i < rewardCount; i++)
        {
            EquipmentRewardSelectionData selection = FindBestEquipmentRewardSelection(
                    party,
                    expeditionResult,
                    reservedMemberIds,
                    reservedMemberSlotKeys,
                    false)
                ?? FindBestEquipmentRewardSelection(
                    party,
                    expeditionResult,
                    reservedMemberIds,
                    reservedMemberSlotKeys,
                    true);
            if (selection == null || selection.Member == null || selection.RewardDefinition == null)
            {
                break;
            }

            PartyInventoryItemData inventoryItem = CreatePartyInventoryItem(party, selection.RewardDefinition);
            if (inventoryItem == null)
            {
                continue;
            }

            party.InventoryItems.Add(inventoryItem);
            reservedMemberIds.Add(selection.Member.MemberId);
            reservedMemberSlotKeys.Add(BuildEquipmentReservationKey(selection.Member.MemberId, selection.SlotKey));
            rewardParts.Add(BuildEquipmentRewardEntryText(selection.Member, selection.RewardDefinition));

            if (TryAutoEquipInventoryItem(
                party,
                selection.Member,
                selection.SlotKey,
                inventoryItem,
                selection.CurrentDefinition,
                out string equipText))
            {
                equipParts.Add(equipText);
                autoEquipCount += 1;
            }
            else
            {
                equipParts.Add(BuildStoredEquipmentResultText(selection.Member, selection.CurrentDefinition, selection.RewardDefinition));
            }
        }

        RefreshPartyInventorySummary(party);
        UpdatePartyDerivedStats(party);

        party.LatestGearRewardSummaryText = JoinSummaryText(rewardParts, " | ", "None");
        party.LatestEquipSwapSummaryText = JoinSummaryText(equipParts, " | ", "None");
        party.LatestGearContinuitySummaryText = BuildGearContinuitySummaryText(party, rewardParts.Count, autoEquipCount);

        expeditionResult.GearRewardCandidateSummaryText = party.LatestGearRewardSummaryText;
        expeditionResult.EquipSwapChoiceSummaryText = party.LatestEquipSwapSummaryText;
        expeditionResult.GearCarryContinuitySummaryText = party.LatestGearContinuitySummaryText;
        expeditionResult.PendingRewardSummaryText = party.PendingRewardSummaryText;
    }

    private int ResolveEquipmentRewardCount(ExpeditionResult expeditionResult)
    {
        if (expeditionResult == null || !expeditionResult.Success)
        {
            return 0;
        }

        int rewardCount = 1;
        if (expeditionResult.OpenedChestCount > 0)
        {
            rewardCount += 1;
        }

        if (expeditionResult.EliteDefeated)
        {
            rewardCount += 1;
        }

        return Mathf.Clamp(rewardCount, 1, 3);
    }

    private EquipmentRewardSelectionData FindBestEquipmentRewardSelection(
        PartyRuntimeData party,
        ExpeditionResult expeditionResult,
        HashSet<string> reservedMemberIds,
        HashSet<string> reservedMemberSlotKeys,
        bool allowReservedMembers)
    {
        if (party == null || expeditionResult == null || party.Members == null || party.Members.Length <= 0)
        {
            return null;
        }

        EquipmentRewardSelectionData bestSelection = null;
        for (int i = 0; i < party.Members.Length; i++)
        {
            PartyMemberProgressionData member = party.Members[i];
            if (member == null || string.IsNullOrEmpty(member.MemberId))
            {
                continue;
            }

            if (!allowReservedMembers && reservedMemberIds != null && reservedMemberIds.Contains(member.MemberId))
            {
                continue;
            }

            string[] slotPriority = PrototypeRpgEquipmentCatalog.GetRewardSlotPriority(member.RoleTag);
            for (int priorityIndex = 0; priorityIndex < slotPriority.Length; priorityIndex++)
            {
                string slotKey = NormalizeEquipmentSlotKey(slotPriority[priorityIndex]);
                if (string.IsNullOrEmpty(slotKey))
                {
                    continue;
                }

                string reservationKey = BuildEquipmentReservationKey(member.MemberId, slotKey);
                if (reservedMemberSlotKeys != null && reservedMemberSlotKeys.Contains(reservationKey))
                {
                    continue;
                }

                PrototypeRpgEquipmentDefinition currentDefinition = GetEquippedDefinition(party, member, slotKey);
                int currentTierRank = currentDefinition != null ? currentDefinition.TierRank : 1;
                PrototypeRpgEquipmentDefinition rewardDefinition = PrototypeRpgEquipmentCatalog.GetSlotUpgradeDefinition(
                    member.RoleTag,
                    slotKey,
                    expeditionResult.EliteDefeated,
                    expeditionResult.RouteId,
                    expeditionResult.OpenedChestCount,
                    currentTierRank);
                if (!PrototypeRpgEquipmentCatalog.IsLegalForRole(rewardDefinition, member.RoleTag))
                {
                    continue;
                }

                int currentScore = currentDefinition != null ? currentDefinition.GetScore() : 0;
                int rewardScore = rewardDefinition != null ? rewardDefinition.GetScore() : 0;
                int scoreDelta = rewardScore - currentScore;
                if (scoreDelta <= 0)
                {
                    continue;
                }

                if (bestSelection == null ||
                    rewardDefinition.TierRank > bestSelection.RewardDefinition.TierRank ||
                    (rewardDefinition.TierRank == bestSelection.RewardDefinition.TierRank && scoreDelta > bestSelection.ScoreDelta) ||
                    (rewardDefinition.TierRank == bestSelection.RewardDefinition.TierRank && scoreDelta == bestSelection.ScoreDelta && priorityIndex < bestSelection.PriorityIndex))
                {
                    bestSelection = new EquipmentRewardSelectionData
                    {
                        Member = member,
                        SlotKey = slotKey,
                        PriorityIndex = priorityIndex,
                        CurrentDefinition = currentDefinition,
                        RewardDefinition = rewardDefinition,
                        ScoreDelta = scoreDelta
                    };
                }
            }
        }

        return bestSelection;
    }

    private PartyInventoryItemData CreatePartyInventoryItem(PartyRuntimeData party, PrototypeRpgEquipmentDefinition definition)
    {
        if (party == null || definition == null || string.IsNullOrEmpty(definition.LoadoutId))
        {
            return null;
        }

        string itemInstanceId = party.PartyId + "-gear-" + party.NextEquipmentItemSequence.ToString("000");
        party.NextEquipmentItemSequence += 1;
        return new PartyInventoryItemData(itemInstanceId, definition.LoadoutId);
    }

    private bool TryAutoEquipInventoryItem(
        PartyRuntimeData party,
        PartyMemberProgressionData member,
        string slotKey,
        PartyInventoryItemData inventoryItem,
        PrototypeRpgEquipmentDefinition currentDefinition,
        out string equipText)
    {
        equipText = string.Empty;

        if (party == null || member == null || inventoryItem == null)
        {
            return false;
        }

        string normalizedSlotKey = NormalizeEquipmentSlotKey(slotKey);
        PrototypeRpgEquipmentDefinition rewardDefinition = ResolvePartyInventoryItemDefinition(inventoryItem, member.RoleTag);
        if (!PrototypeRpgEquipmentCatalog.IsLegalForRole(rewardDefinition, member.RoleTag) ||
            NormalizeEquipmentSlotKey(rewardDefinition != null ? rewardDefinition.SlotKey : string.Empty) != normalizedSlotKey)
        {
            return false;
        }

        PrototypeRpgEquipmentDefinition safeCurrentDefinition = currentDefinition ?? GetEquippedDefinition(party, member, normalizedSlotKey);
        int currentScore = safeCurrentDefinition != null ? safeCurrentDefinition.GetScore() : 0;
        if (rewardDefinition.GetScore() <= currentScore)
        {
            return false;
        }

        if (member.EquippedItemInstanceIdBySlotKey.TryGetValue(normalizedSlotKey, out string previousItemInstanceId))
        {
            PartyInventoryItemData previousItem = FindPartyInventoryItem(party, previousItemInstanceId);
            if (previousItem != null)
            {
                previousItem.EquippedMemberId = string.Empty;
                previousItem.EquippedSlotKey = string.Empty;
            }
        }

        member.EquippedItemInstanceIdBySlotKey[normalizedSlotKey] = inventoryItem.ItemInstanceId;
        inventoryItem.EquippedMemberId = member.MemberId;
        inventoryItem.EquippedSlotKey = normalizedSlotKey;
        equipText = BuildAutoEquipResultText(party, member, safeCurrentDefinition, rewardDefinition);
        return true;
    }

    private string BuildAutoEquipResultText(
        PartyRuntimeData party,
        PartyMemberProgressionData member,
        PrototypeRpgEquipmentDefinition currentDefinition,
        PrototypeRpgEquipmentDefinition rewardDefinition)
    {
        if (member == null || rewardDefinition == null)
        {
            return "Auto-equip applied.";
        }

        string rewardLabel = PrototypeRpgEquipmentCatalog.BuildCompactReadbackText(rewardDefinition);
        string statDeltaText = BuildEquipmentStatDeltaText(party, member, currentDefinition, rewardDefinition);
        return string.IsNullOrEmpty(statDeltaText)
            ? member.DisplayName + " equipped " + rewardLabel
            : member.DisplayName + " equipped " + rewardLabel + " | " + statDeltaText;
    }

    private string BuildStoredEquipmentResultText(
        PartyMemberProgressionData member,
        PrototypeRpgEquipmentDefinition currentDefinition,
        PrototypeRpgEquipmentDefinition rewardDefinition)
    {
        if (member == null || rewardDefinition == null)
        {
            return "Stored.";
        }

        string currentLabel =
            currentDefinition != null && currentDefinition.TierRank > 1
                ? PrototypeRpgEquipmentCatalog.BuildCompactReadbackText(currentDefinition)
                : "current gear";
        return member.DisplayName +
            " kept " + currentLabel +
            " | " + PrototypeRpgEquipmentCatalog.BuildCompactReadbackText(rewardDefinition) +
            " stored.";
    }

    private string BuildEquipmentStatDeltaText(
        PartyRuntimeData party,
        PartyMemberProgressionData member,
        PrototypeRpgEquipmentDefinition currentDefinition,
        PrototypeRpgEquipmentDefinition rewardDefinition)
    {
        ResolveMemberResolvedStats(
            party,
            member,
            out int currentMaxHp,
            out int currentAttack,
            out int currentDefense,
            out int currentSpeed,
            out int currentSkillPower);

        int nextMaxHp = Mathf.Max(1, currentMaxHp - (currentDefinition != null ? currentDefinition.MaxHpBonus : 0) + (rewardDefinition != null ? rewardDefinition.MaxHpBonus : 0));
        int nextAttack = Mathf.Max(1, currentAttack - (currentDefinition != null ? currentDefinition.AttackBonus : 0) + (rewardDefinition != null ? rewardDefinition.AttackBonus : 0));
        int nextDefense = Mathf.Max(0, currentDefense - (currentDefinition != null ? currentDefinition.DefenseBonus : 0) + (rewardDefinition != null ? rewardDefinition.DefenseBonus : 0));
        int nextSpeed = Mathf.Max(0, currentSpeed - (currentDefinition != null ? currentDefinition.SpeedBonus : 0) + (rewardDefinition != null ? rewardDefinition.SpeedBonus : 0));
        int nextSkillPower = Mathf.Max(1, currentSkillPower - (currentDefinition != null ? currentDefinition.SkillPowerBonus : 0) + (rewardDefinition != null ? rewardDefinition.SkillPowerBonus : 0));

        List<string> parts = new List<string>();
        AddEquipmentStatDeltaPart(parts, "HP", currentMaxHp, nextMaxHp);
        AddEquipmentStatDeltaPart(parts, "ATK", currentAttack, nextAttack);
        AddEquipmentStatDeltaPart(parts, "DEF", currentDefense, nextDefense);
        AddEquipmentStatDeltaPart(parts, "SPD", currentSpeed, nextSpeed);
        AddEquipmentStatDeltaPart(parts, "POW", currentSkillPower, nextSkillPower);
        return parts.Count > 0 ? string.Join(" | ", parts.ToArray()) : string.Empty;
    }

    private void ResolveMemberResolvedStats(
        PartyRuntimeData party,
        PartyMemberProgressionData member,
        out int maxHp,
        out int attack,
        out int defense,
        out int speed,
        out int skillPower)
    {
        maxHp = 1;
        attack = 1;
        defense = 0;
        speed = 0;
        skillPower = 1;
        if (party == null || member == null)
        {
            return;
        }

        PrototypeRpgPartyDefinition partyDefinition = PrototypeRpgPartyCatalog.CreateRuntimeParty(
            party.PartyId,
            party.ArchetypeId,
            party.PromotionStateId,
            party.DisplayName);
        PrototypeRpgPartyMemberDefinition memberDefinition = ResolvePartyMemberDefinition(partyDefinition, member);
        PrototypeRpgStatBlock baseStats = memberDefinition != null && memberDefinition.BaseStats != null
            ? memberDefinition.BaseStats
            : new PrototypeRpgStatBlock(1, 1, 0, 0);
        PrototypeRpgLevelStatBonus growthBonus = PrototypeRpgMemberProgressionRules.ResolveLevelStatBonus(
            member.RoleTag,
            party.ArchetypeId,
            member.Level);
        PrototypeRpgSkillDefinition skillDefinition = PrototypeRpgSkillCatalog.ResolveDefinition(
                memberDefinition != null ? memberDefinition.DefaultSkillId : string.Empty,
                member.RoleTag)
            ?? PrototypeRpgSkillCatalog.GetFallbackDefinitionForRoleTag(member.RoleTag);

        ResolveMemberEquipmentBonuses(
            party,
            member,
            out int maxHpBonus,
            out int attackBonus,
            out int defenseBonus,
            out int speedBonus,
            out int skillPowerBonus);

        maxHp = Mathf.Max(1, baseStats.MaxHp + growthBonus.MaxHpBonus + maxHpBonus);
        attack = Mathf.Max(1, baseStats.Attack + growthBonus.AttackBonus + attackBonus);
        defense = Mathf.Max(0, baseStats.Defense + growthBonus.DefenseBonus + defenseBonus);
        speed = Mathf.Max(0, baseStats.Speed + growthBonus.SpeedBonus + speedBonus);

        int baseSkillPower = skillDefinition != null && skillDefinition.PowerValue > 0
            ? skillDefinition.PowerValue
            : Mathf.Max(1, baseStats.Attack + 1);
        skillPower = Mathf.Max(1, baseSkillPower + growthBonus.AttackBonus + skillPowerBonus);
    }

    private PrototypeRpgPartyMemberDefinition ResolvePartyMemberDefinition(
        PrototypeRpgPartyDefinition partyDefinition,
        PartyMemberProgressionData member)
    {
        PrototypeRpgPartyMemberDefinition[] members = partyDefinition != null
            ? partyDefinition.Members ?? System.Array.Empty<PrototypeRpgPartyMemberDefinition>()
            : System.Array.Empty<PrototypeRpgPartyMemberDefinition>();
        for (int i = 0; i < members.Length; i++)
        {
            PrototypeRpgPartyMemberDefinition definition = members[i];
            if (definition == null)
            {
                continue;
            }

            if (!string.IsNullOrEmpty(member.MemberId) && definition.MemberId == member.MemberId)
            {
                return definition;
            }

            if (!string.IsNullOrEmpty(member.DisplayName) && definition.DisplayName == member.DisplayName)
            {
                return definition;
            }
        }

        return members.Length > 0 ? members[0] : null;
    }

    private void AddEquipmentStatDeltaPart(List<string> parts, string label, int beforeValue, int afterValue)
    {
        if (parts == null || beforeValue == afterValue)
        {
            return;
        }

        parts.Add(label + " " + beforeValue + " -> " + afterValue);
    }

    private PrototypeRpgEquipmentDefinition GetFeaturedEquipmentDefinition(PartyRuntimeData party, PartyMemberProgressionData member)
    {
        if (member == null)
        {
            return null;
        }

        PrototypeRpgEquipmentDefinition featuredDefinition = null;
        string[] slotPriority = PrototypeRpgEquipmentCatalog.GetRewardSlotPriority(member.RoleTag);
        for (int i = 0; i < slotPriority.Length; i++)
        {
            PrototypeRpgEquipmentDefinition definition = GetEquippedDefinition(party, member, slotPriority[i]);
            if (definition == null || definition.TierRank <= 1)
            {
                continue;
            }

            if (featuredDefinition == null ||
                definition.TierRank > featuredDefinition.TierRank ||
                (definition.TierRank == featuredDefinition.TierRank && definition.GetScore() > featuredDefinition.GetScore()))
            {
                featuredDefinition = definition;
            }
        }

        return featuredDefinition ?? PrototypeRpgEquipmentCatalog.GetFallbackDefinitionForRoleTag(member.RoleTag);
    }

    private string BuildMemberEquipmentSummaryText(PartyRuntimeData party, PartyMemberProgressionData member)
    {
        if (member == null)
        {
            return "No gear";
        }

        int upgradedSlotCount = CountUpgradedEquipmentSlots(party, member);
        PrototypeRpgEquipmentDefinition featuredDefinition = GetFeaturedEquipmentDefinition(party, member);
        string featuredSummary = PrototypeRpgEquipmentCatalog.BuildEquipmentSummaryText(featuredDefinition);
        return upgradedSlotCount <= 0
            ? featuredSummary
            : featuredSummary + " | " + upgradedSlotCount + (upgradedSlotCount == 1 ? " upgraded slot" : " upgraded slots");
    }

    private string BuildMemberEquipmentSlotSummaryText(PartyRuntimeData party, PartyMemberProgressionData member)
    {
        if (member == null)
        {
            return "No gear";
        }

        List<string> upgradedParts = new List<string>();
        List<string> starterSlots = new List<string>();
        string[] slotKeys = PrototypeRpgEquipmentSlotKeys.OrderedKeys;
        for (int i = 0; i < slotKeys.Length; i++)
        {
            string slotKey = slotKeys[i];
            PrototypeRpgEquipmentDefinition definition = GetEquippedDefinition(party, member, slotKey);
            string shortLabel = PrototypeRpgEquipmentSlotKeys.ToShortLabel(slotKey);
            if (definition != null && definition.TierRank > 1)
            {
                upgradedParts.Add(shortLabel + " " + definition.DisplayName);
            }
            else
            {
                starterSlots.Add(shortLabel);
            }
        }

        if (upgradedParts.Count <= 0)
        {
            return "Starter set across " + string.Join("/", starterSlots.ToArray());
        }

        string summary = "Slots " + string.Join(" / ", upgradedParts.ToArray());
        if (starterSlots.Count > 0)
        {
            summary += " | Base " + string.Join("/", starterSlots.ToArray());
        }

        return summary;
    }

    private string BuildPartyLoadoutSummaryText(PartyRuntimeData party)
    {
        if (party == null || party.Members == null || party.Members.Length <= 0)
        {
            return "None";
        }

        List<string> parts = new List<string>();
        for (int i = 0; i < party.Members.Length; i++)
        {
            PartyMemberProgressionData member = party.Members[i];
            if (member == null)
            {
                continue;
            }

            string label = !string.IsNullOrEmpty(member.DisplayName)
                ? member.DisplayName
                : PrototypeRpgRoleIdentity.BuildRoleIdentityLabel(string.Empty, member.RoleTag);
            string gearSummary = BuildMemberEquipmentSlotSummaryText(party, member);
            parts.Add(label + ": " + (string.IsNullOrEmpty(gearSummary) ? "No gear" : gearSummary));
        }

        string summary = JoinSummaryText(parts, " | ", "None");
        string inventorySummary = !string.IsNullOrEmpty(party.InventorySummaryText)
            ? party.InventorySummaryText
            : "Owned 0 | Equipped 0 | Stored 0";
        if (summary == "None")
        {
            return "Inventory " + inventorySummary;
        }

        return summary + " | Inventory " + inventorySummary;
    }

    private int CountUpgradedEquipmentSlots(PartyRuntimeData party, PartyMemberProgressionData member)
    {
        if (member == null)
        {
            return 0;
        }

        int upgradedSlots = 0;
        string[] slotKeys = PrototypeRpgEquipmentSlotKeys.OrderedKeys;
        for (int i = 0; i < slotKeys.Length; i++)
        {
            PrototypeRpgEquipmentDefinition definition = GetEquippedDefinition(party, member, slotKeys[i]);
            if (definition != null && definition.TierRank > 1)
            {
                upgradedSlots += 1;
            }
        }

        return upgradedSlots;
    }

    private PartyMemberProgressionData ResolveInventorySelectedMember(PartyRuntimeData party, string selectedMemberId)
    {
        if (party == null || party.Members == null || party.Members.Length <= 0)
        {
            return null;
        }

        if (!string.IsNullOrEmpty(selectedMemberId))
        {
            PartyMemberProgressionData explicitMember = FindPartyMemberProgression(party.PartyId, selectedMemberId);
            if (explicitMember != null)
            {
                return explicitMember;
            }
        }

        return party.Members[0];
    }

    private string ResolveInventorySelectedSlotKey(string slotKey)
    {
        string normalizedSlotKey = NormalizeEquipmentSlotKey(slotKey);
        return string.IsNullOrEmpty(normalizedSlotKey)
            ? PrototypeRpgEquipmentSlotKeys.OrderedKeys[0]
            : normalizedSlotKey;
    }

    private PrototypeRpgInventoryMemberSurfaceData[] BuildInventoryMemberSurfaceArray(PartyRuntimeData party, PartyMemberProgressionData selectedMember)
    {
        if (party == null || party.Members == null || party.Members.Length <= 0)
        {
            return System.Array.Empty<PrototypeRpgInventoryMemberSurfaceData>();
        }

        PrototypeRpgPartyDefinition partyDefinition = PrototypeRpgPartyCatalog.CreateRuntimeParty(
            party.PartyId,
            party.ArchetypeId,
            party.PromotionStateId,
            party.DisplayName);
        PrototypeRpgInventoryMemberSurfaceData[] members = new PrototypeRpgInventoryMemberSurfaceData[party.Members.Length];
        for (int i = 0; i < party.Members.Length; i++)
        {
            PartyMemberProgressionData member = party.Members[i];
            PrototypeRpgPartyMemberDefinition memberDefinition = FindPartyMemberDefinition(partyDefinition, member != null ? member.MemberId : string.Empty);
            ResolveMemberInventoryStatBreakdown(
                party,
                member,
                memberDefinition,
                out int baseMaxHp,
                out int baseAttack,
                out int baseDefense,
                out int baseSpeed,
                out int growthMaxHp,
                out int growthAttack,
                out int growthDefense,
                out int growthSpeed,
                out int gearMaxHp,
                out int gearAttack,
                out int gearDefense,
                out int gearSpeed,
                out int _,
                out int resolvedMaxHp,
                out int resolvedAttack,
                out int resolvedDefense,
                out int resolvedSpeed,
                out int _);

            PrototypeRpgInventoryMemberSurfaceData data = new PrototypeRpgInventoryMemberSurfaceData();
            data.MemberId = member != null ? member.MemberId : string.Empty;
            data.DisplayName = member != null && !string.IsNullOrEmpty(member.DisplayName) ? member.DisplayName : "Adventurer";
            data.RoleLabel = memberDefinition != null && !string.IsNullOrEmpty(memberDefinition.RoleLabel)
                ? memberDefinition.RoleLabel
                : member != null && !string.IsNullOrEmpty(member.RoleTag)
                    ? member.RoleTag
                    : "Adventurer";
            data.RoleIdentityText = PrototypeRpgRoleIdentity.BuildRoleIdentityLabel(data.RoleLabel, member != null ? member.RoleTag : string.Empty);
            data.GearPreferenceText = PrototypeRpgRoleIdentity.BuildGearPreferenceText(member != null ? member.RoleTag : string.Empty);
            data.Level = member != null && member.Level > 0 ? member.Level : PrototypeRpgMemberProgressionRules.GetStartingLevel();
            data.CurrentExperience = member != null ? Mathf.Max(0, member.Experience) : PrototypeRpgMemberProgressionRules.GetStartingExperience();
            data.NextLevelExperience = PrototypeRpgMemberProgressionRules.GetNextLevelExperience(data.Level);
            data.BaseStatsText = "Base: " + BuildResolvedStatText(baseMaxHp, baseAttack, baseDefense, baseSpeed, false);
            data.LevelBonusText = "Level: " + PrototypeRpgEquipmentCatalog.BuildStatContributionSummary(growthMaxHp, growthAttack, growthDefense, growthSpeed, 0);
            data.GearBonusText = "Gear: " + PrototypeRpgEquipmentCatalog.BuildStatContributionSummary(gearMaxHp, gearAttack, gearDefense, gearSpeed, 0);
            data.ResolvedStatsText = "Resolved: " + BuildResolvedStatText(resolvedMaxHp, resolvedAttack, resolvedDefense, resolvedSpeed, true);
            data.SummaryText = data.DisplayName + " Lv" + data.Level + " | " + BuildResolvedStatText(resolvedMaxHp, resolvedAttack, resolvedDefense, resolvedSpeed, true);
            data.IsSelected = member != null && selectedMember != null && member.MemberId == selectedMember.MemberId;
            members[i] = data;
        }

        return members;
    }

    private PrototypeRpgInventorySlotSurfaceData[] BuildInventorySlotSurfaceArray(PartyRuntimeData party, PartyMemberProgressionData member, string selectedSlotKey)
    {
        string[] orderedKeys = PrototypeRpgEquipmentSlotKeys.OrderedKeys;
        PrototypeRpgInventorySlotSurfaceData[] slots = new PrototypeRpgInventorySlotSurfaceData[orderedKeys.Length];
        for (int i = 0; i < orderedKeys.Length; i++)
        {
            string slotKey = orderedKeys[i];
            PrototypeRpgEquipmentDefinition equippedDefinition = GetActualEquippedDefinition(party, member, slotKey);
            PrototypeRpgInventorySlotSurfaceData data = new PrototypeRpgInventorySlotSurfaceData();
            data.SlotKey = slotKey;
            data.SlotLabel = PrototypeRpgEquipmentSlotKeys.ToDisplayLabel(slotKey);
            data.HotkeyLabel = "[" + (i + 1) + "]";
            data.HasEquippedItem = equippedDefinition != null;
            data.EquippedItemName = equippedDefinition != null ? equippedDefinition.DisplayName : "Empty";
            data.EquippedItemSummaryText = equippedDefinition != null
                ? PrototypeRpgEquipmentCatalog.BuildCompactReadbackText(equippedDefinition)
                : "Empty";
            data.StatBonusText = equippedDefinition != null
                ? PrototypeRpgEquipmentCatalog.BuildStatContributionSummary(equippedDefinition)
                : "No bonus";
            if (member != null &&
                member.EquippedItemInstanceIdBySlotKey.TryGetValue(slotKey, out string equippedItemInstanceId) &&
                !string.IsNullOrEmpty(equippedItemInstanceId))
            {
                data.EquippedItemInstanceId = equippedItemInstanceId;
            }

            data.IsSelected = slotKey == selectedSlotKey;
            slots[i] = data;
        }

        return slots;
    }

    private PrototypeRpgInventoryItemSurfaceData[] BuildInventoryItemSurfaceArray(
        PartyRuntimeData party,
        PartyMemberProgressionData member,
        string selectedSlotKey,
        string selectedItemInstanceId)
    {
        if (party == null || party.InventoryItems == null || party.InventoryItems.Count <= 0)
        {
            return System.Array.Empty<PrototypeRpgInventoryItemSurfaceData>();
        }

        List<PrototypeRpgInventoryItemSurfaceData> items = new List<PrototypeRpgInventoryItemSurfaceData>(party.InventoryItems.Count);
        string normalizedSlotKey = NormalizeEquipmentSlotKey(selectedSlotKey);
        for (int i = 0; i < party.InventoryItems.Count; i++)
        {
            PartyInventoryItemData item = party.InventoryItems[i];
            PrototypeRpgEquipmentDefinition definition = ResolvePartyInventoryItemDefinition(item, member != null ? member.RoleTag : string.Empty);
            if (item == null || definition == null)
            {
                continue;
            }

            PrototypeRpgInventoryItemSurfaceData data = new PrototypeRpgInventoryItemSurfaceData();
            data.ItemInstanceId = item.ItemInstanceId;
            data.DisplayName = definition.DisplayName;
            data.SlotKey = definition.SlotKey;
            data.SlotLabel = definition.SlotLabel;
            data.TierLabel = definition.TierLabel;
            data.StatBonusText = PrototypeRpgEquipmentCatalog.BuildStatContributionSummary(definition);
            data.SourceSummaryText = BuildInventoryDefinitionSourceText(definition);
            data.EquippedBySummaryText = BuildInventoryItemOwnerText(party, item);
            data.ScoreSummaryText = "Score " + definition.GetScore();
            data.SummaryText = definition.DisplayName + " | " + definition.SlotLabel + " | " + data.StatBonusText;
            data.IsCompatible = member != null &&
                PrototypeRpgEquipmentCatalog.IsLegalForRole(definition, member.RoleTag) &&
                NormalizeEquipmentSlotKey(definition.SlotKey) == normalizedSlotKey;
            data.IsEquipped = !string.IsNullOrEmpty(item.EquippedMemberId);
            data.IsSelected = item.ItemInstanceId == selectedItemInstanceId;
            items.Add(data);
        }

        items.Sort((left, right) =>
        {
            int compatibilityCompare = CompareInventoryCompatibility(left, right);
            if (compatibilityCompare != 0)
            {
                return compatibilityCompare;
            }

            int equippedCompare = CompareInventoryEquippedState(left, right);
            if (equippedCompare != 0)
            {
                return equippedCompare;
            }

            return string.Compare(left.DisplayName, right.DisplayName, StringComparison.OrdinalIgnoreCase);
        });

        if (items.Count > 0 && string.IsNullOrEmpty(selectedItemInstanceId))
        {
            items[0].IsSelected = true;
        }

        bool hasSelected = false;
        for (int i = 0; i < items.Count; i++)
        {
            if (items[i].IsSelected)
            {
                hasSelected = true;
                break;
            }
        }

        if (!hasSelected && items.Count > 0)
        {
            items[0].IsSelected = true;
        }

        return items.ToArray();
    }

    private PrototypeRpgInventoryComparisonSurfaceData BuildInventoryComparisonSurface(
        PartyRuntimeData party,
        PartyMemberProgressionData member,
        string slotKey,
        PrototypeRpgEquipmentDefinition currentDefinition,
        PartyInventoryItemData selectedInventoryItem,
        PrototypeRpgEquipmentDefinition candidateDefinition,
        bool canEquip,
        bool canUnequip,
        string equipReasonText,
        bool isReadOnly)
    {
        PrototypeRpgInventoryComparisonSurfaceData comparison = new PrototypeRpgInventoryComparisonSurfaceData();
        string currentLabel = currentDefinition != null
            ? PrototypeRpgEquipmentCatalog.BuildCompactReadbackText(currentDefinition)
            : "Empty";
        comparison.CurrentText = "Current: " + currentLabel;

        if (selectedInventoryItem == null || candidateDefinition == null)
        {
            comparison.CandidateText = "Candidate: None";
            comparison.DeltaText = "No stat change";
            comparison.ReasonText = "Select an item.";
            comparison.ActionHintText = isReadOnly
                ? "[I] Close | Read-only during battle"
                : canUnequip
                    ? "[U] Unequip | [Esc] Close"
                    : "[Esc] Close";
            return comparison;
        }

        comparison.CandidateText = "Candidate: " + PrototypeRpgEquipmentCatalog.BuildCompactReadbackText(candidateDefinition);
        comparison.DeltaText = BuildInventoryDeltaText(party, member, currentDefinition, candidateDefinition);
        comparison.ReasonText = canEquip ? "Equip ready." : equipReasonText;
        comparison.ActionHintText = isReadOnly
            ? "[I] Close | Read-only during battle"
            : canEquip
                ? "[Enter] Equip | [U] Unequip | [Esc] Close"
                : canUnequip
                    ? "[U] Unequip | [Esc] Close"
                    : "[Esc] Close";
        return comparison;
    }

    private PartyInventoryItemData ResolveSelectedInventoryItem(PartyRuntimeData party, PrototypeRpgInventoryItemSurfaceData[] items)
    {
        if (party == null || items == null || items.Length <= 0)
        {
            return null;
        }

        for (int i = 0; i < items.Length; i++)
        {
            if (items[i] != null && items[i].IsSelected)
            {
                return FindPartyInventoryItem(party, items[i].ItemInstanceId);
            }
        }

        return FindPartyInventoryItem(party, items[0].ItemInstanceId);
    }

    private PrototypeRpgPartyMemberDefinition FindPartyMemberDefinition(PrototypeRpgPartyDefinition partyDefinition, string memberId)
    {
        if (partyDefinition == null || partyDefinition.Members == null || string.IsNullOrEmpty(memberId))
        {
            return null;
        }

        for (int i = 0; i < partyDefinition.Members.Length; i++)
        {
            PrototypeRpgPartyMemberDefinition member = partyDefinition.Members[i];
            if (member != null && member.MemberId == memberId)
            {
                return member;
            }
        }

        return null;
    }

    private void ResolveMemberInventoryStatBreakdown(
        PartyRuntimeData party,
        PartyMemberProgressionData member,
        PrototypeRpgPartyMemberDefinition memberDefinition,
        out int baseMaxHp,
        out int baseAttack,
        out int baseDefense,
        out int baseSpeed,
        out int growthMaxHp,
        out int growthAttack,
        out int growthDefense,
        out int growthSpeed,
        out int gearMaxHp,
        out int gearAttack,
        out int gearDefense,
        out int gearSpeed,
        out int gearSkillPower,
        out int resolvedMaxHp,
        out int resolvedAttack,
        out int resolvedDefense,
        out int resolvedSpeed,
        out int resolvedSkillPower)
    {
        PrototypeRpgStatBlock baseStats = memberDefinition != null && memberDefinition.BaseStats != null
            ? memberDefinition.BaseStats
            : new PrototypeRpgStatBlock(1, 1, 0, 0);
        PrototypeRpgLevelStatBonus growthBonus = PrototypeRpgMemberProgressionRules.ResolveLevelStatBonus(
            member != null ? member.RoleTag : string.Empty,
            party != null ? party.ArchetypeId : string.Empty,
            member != null && member.Level > 0 ? member.Level : PrototypeRpgMemberProgressionRules.GetStartingLevel());
        ResolveMemberEquipmentBonuses(
            party,
            member,
            out gearMaxHp,
            out gearAttack,
            out gearDefense,
            out gearSpeed,
            out gearSkillPower);

        baseMaxHp = Mathf.Max(1, baseStats.MaxHp);
        baseAttack = Mathf.Max(1, baseStats.Attack);
        baseDefense = Mathf.Max(0, baseStats.Defense);
        baseSpeed = Mathf.Max(0, baseStats.Speed);
        growthMaxHp = growthBonus.MaxHpBonus;
        growthAttack = growthBonus.AttackBonus;
        growthDefense = growthBonus.DefenseBonus;
        growthSpeed = growthBonus.SpeedBonus;
        resolvedMaxHp = Mathf.Max(1, baseMaxHp + growthMaxHp + gearMaxHp);
        resolvedAttack = Mathf.Max(1, baseAttack + growthAttack + gearAttack);
        resolvedDefense = Mathf.Max(0, baseDefense + growthDefense + gearDefense);
        resolvedSpeed = Mathf.Max(0, baseSpeed + growthSpeed + gearSpeed);
        int baseSkillPower = memberDefinition != null && !string.IsNullOrEmpty(memberDefinition.DefaultSkillId)
            ? Mathf.Max(1, baseAttack + 1)
            : Mathf.Max(1, baseAttack + 1);
        resolvedSkillPower = Mathf.Max(1, baseSkillPower + growthAttack + gearSkillPower);
    }

    private string BuildResolvedStatText(int maxHp, int attack, int defense, int speed, bool useCompactLabels)
    {
        string hpLabel = useCompactLabels ? "HP" : "HP";
        string attackLabel = useCompactLabels ? "ATK" : "ATK";
        string defenseLabel = useCompactLabels ? "DEF" : "DEF";
        string speedLabel = useCompactLabels ? "SPD" : "SPD";
        return hpLabel + " " + maxHp + " " + attackLabel + " " + attack + " " + defenseLabel + " " + defense + " " + speedLabel + " " + speed;
    }

    private PrototypeRpgEquipmentDefinition GetActualEquippedDefinition(PartyRuntimeData party, PartyMemberProgressionData member, string slotKey)
    {
        if (party == null || member == null)
        {
            return null;
        }

        string normalizedSlotKey = NormalizeEquipmentSlotKey(slotKey);
        if (!member.EquippedItemInstanceIdBySlotKey.TryGetValue(normalizedSlotKey, out string itemInstanceId) || string.IsNullOrEmpty(itemInstanceId))
        {
            return null;
        }

        PartyInventoryItemData item = FindPartyInventoryItem(party, itemInstanceId);
        PrototypeRpgEquipmentDefinition definition = ResolvePartyInventoryItemDefinition(item, member.RoleTag);
        return definition != null && NormalizeEquipmentSlotKey(definition.SlotKey) == normalizedSlotKey
            ? definition
            : null;
    }

    private bool HasActualEquippedItem(PartyRuntimeData party, PartyMemberProgressionData member, string slotKey)
    {
        return GetActualEquippedDefinition(party, member, slotKey) != null;
    }

    private bool CanManualEquipInventoryItem(
        PartyRuntimeData party,
        PartyMemberProgressionData member,
        string slotKey,
        PartyInventoryItemData item,
        PrototypeRpgEquipmentDefinition definition,
        out string reasonText)
    {
        reasonText = "Select an item.";
        if (party == null || member == null || item == null || definition == null)
        {
            return false;
        }

        string normalizedSlotKey = NormalizeEquipmentSlotKey(slotKey);
        if (NormalizeEquipmentSlotKey(definition.SlotKey) != normalizedSlotKey)
        {
            reasonText = "Slot mismatch. Select " + PrototypeRpgEquipmentSlotKeys.ToDisplayLabel(normalizedSlotKey) + ".";
            return false;
        }

        if (!PrototypeRpgEquipmentCatalog.IsLegalForRole(definition, member.RoleTag))
        {
            reasonText = definition.DisplayName + " does not fit " + member.DisplayName + ".";
            return false;
        }

        if (!string.IsNullOrEmpty(item.EquippedMemberId) && item.EquippedMemberId != member.MemberId)
        {
            PartyMemberProgressionData equippedMember = FindPartyMemberProgression(party.PartyId, item.EquippedMemberId);
            string equippedBy = equippedMember != null ? equippedMember.DisplayName : item.EquippedMemberId;
            reasonText = "Equipped by " + equippedBy + ". Swap support reserved.";
            return false;
        }

        reasonText = "Equip ready.";
        return true;
    }

    private string BuildInventoryDeltaText(
        PartyRuntimeData party,
        PartyMemberProgressionData member,
        PrototypeRpgEquipmentDefinition currentDefinition,
        PrototypeRpgEquipmentDefinition candidateDefinition)
    {
        string deltaText = BuildEquipmentStatDeltaText(party, member, currentDefinition, candidateDefinition);
        return string.IsNullOrEmpty(deltaText) ? "No stat change" : deltaText;
    }

    private string BuildInventoryDefinitionSourceText(PrototypeRpgEquipmentDefinition definition)
    {
        if (definition == null)
        {
            return "Source: None";
        }

        return "Source: " + definition.TierLabel + " " + definition.SlotLabel + " gear";
    }

    private string BuildInventoryDefinitionDetailText(PrototypeRpgEquipmentDefinition definition)
    {
        if (definition == null)
        {
            return "No item selected.";
        }

        return "Bonus: " + PrototypeRpgEquipmentCatalog.BuildStatContributionSummary(definition) + " | " +
               BuildInventoryDefinitionSourceText(definition) + " | Fit " + definition.GetScore();
    }

    private string BuildInventorySelectedItemDetailText(
        PartyRuntimeData party,
        PartyInventoryItemData item,
        PrototypeRpgEquipmentDefinition definition,
        PrototypeRpgInventoryComparisonSurfaceData comparison)
    {
        string deltaText = comparison != null && !string.IsNullOrEmpty(comparison.DeltaText)
            ? comparison.DeltaText
            : "No stat change";
        string stateText = BuildInventoryItemOwnerText(party, item);
        string actionText = comparison != null && !string.IsNullOrEmpty(comparison.ReasonText) && comparison.ReasonText != "Select an item."
            ? comparison.ReasonText
            : comparison != null && !string.IsNullOrEmpty(comparison.ActionHintText)
                ? comparison.ActionHintText
                : "No action available.";
        return BuildInventoryDefinitionDetailText(definition) + " | Delta: " + deltaText + " | State: " + stateText + " | Action: " + actionText;
    }

    private string BuildInventoryItemOwnerText(PartyRuntimeData party, PartyInventoryItemData item)
    {
        if (party == null || item == null || string.IsNullOrEmpty(item.EquippedMemberId))
        {
            return "Stored in party inventory";
        }

        PartyMemberProgressionData equippedMember = FindPartyMemberProgression(party.PartyId, item.EquippedMemberId);
        string equippedBy = equippedMember != null ? equippedMember.DisplayName : item.EquippedMemberId;
        return "Equipped by " + equippedBy;
    }

    private string BuildManualEquipResultText(
        PartyRuntimeData party,
        PartyMemberProgressionData member,
        PrototypeRpgEquipmentDefinition currentDefinition,
        PrototypeRpgEquipmentDefinition nextDefinition)
    {
        if (member == null || nextDefinition == null)
        {
            return "Manual equip updated.";
        }

        string nextLabel = PrototypeRpgEquipmentCatalog.BuildCompactReadbackText(nextDefinition);
        string statDeltaText = BuildEquipmentStatDeltaText(party, member, currentDefinition, nextDefinition);
        return string.IsNullOrEmpty(statDeltaText)
            ? "Manual equip: " + member.DisplayName + " equipped " + nextLabel
            : "Manual equip: " + member.DisplayName + " equipped " + nextLabel + " | " + statDeltaText;
    }

    private string BuildManualUnequipResultText(
        PartyRuntimeData party,
        PartyMemberProgressionData member,
        PrototypeRpgEquipmentDefinition currentDefinition)
    {
        if (member == null || currentDefinition == null)
        {
            return "Manual equip: slot cleared.";
        }

        string currentLabel = PrototypeRpgEquipmentCatalog.BuildCompactReadbackText(currentDefinition);
        string statDeltaText = BuildEquipmentStatDeltaText(party, member, currentDefinition, null);
        return string.IsNullOrEmpty(statDeltaText)
            ? "Manual equip: " + member.DisplayName + " unequipped " + currentLabel
            : "Manual equip: " + member.DisplayName + " unequipped " + currentLabel + " | " + statDeltaText;
    }

    private static int CompareInventoryCompatibility(PrototypeRpgInventoryItemSurfaceData left, PrototypeRpgInventoryItemSurfaceData right)
    {
        int leftRank = left != null && left.IsCompatible ? 0 : 1;
        int rightRank = right != null && right.IsCompatible ? 0 : 1;
        return leftRank.CompareTo(rightRank);
    }

    private static int CompareInventoryEquippedState(PrototypeRpgInventoryItemSurfaceData left, PrototypeRpgInventoryItemSurfaceData right)
    {
        int leftRank = left != null && left.IsEquipped ? 1 : 0;
        int rightRank = right != null && right.IsEquipped ? 1 : 0;
        return leftRank.CompareTo(rightRank);
    }

    private string BuildInventorySummaryText(PartyRuntimeData party)
    {
        if (party == null || party.InventoryItems == null || party.InventoryItems.Count <= 0)
        {
            return "Owned 0 | Equipped 0 | Stored 0";
        }

        int ownedCount = 0;
        int equippedCount = 0;
        List<string> storedParts = new List<string>();
        for (int i = 0; i < party.InventoryItems.Count; i++)
        {
            PartyInventoryItemData item = party.InventoryItems[i];
            if (item == null || string.IsNullOrEmpty(item.ItemInstanceId))
            {
                continue;
            }

            ownedCount += 1;
            if (!string.IsNullOrEmpty(item.EquippedMemberId))
            {
                equippedCount += 1;
                continue;
            }

            PrototypeRpgEquipmentDefinition definition = ResolvePartyInventoryItemDefinition(item, string.Empty);
            if (definition != null)
            {
                storedParts.Add(PrototypeRpgEquipmentCatalog.BuildEquipmentSummaryText(definition));
            }
        }

        int storedCount = Mathf.Max(0, ownedCount - equippedCount);
        string summary = "Owned " + ownedCount + " | Equipped " + equippedCount + " | Stored " + storedCount;
        if (storedParts.Count <= 0)
        {
            return summary;
        }

        int previewCount = Mathf.Min(2, storedParts.Count);
        string[] previewParts = storedParts.GetRange(0, previewCount).ToArray();
        string previewText = string.Join(" / ", previewParts);
        if (storedParts.Count > previewCount)
        {
            previewText += " +" + (storedParts.Count - previewCount) + " more";
        }

        return summary + " | " + previewText;
    }

    private void RefreshPartyInventorySummary(PartyRuntimeData party)
    {
        if (party == null)
        {
            return;
        }

        party.InventorySummaryText = BuildInventorySummaryText(party);
        party.InventoryRevision = Mathf.Max(1, party.InventoryRevision + 1);
        if (string.IsNullOrEmpty(party.LatestGearContinuitySummaryText) || party.LatestGearContinuitySummaryText == "None")
        {
            party.LatestGearContinuitySummaryText = party.InventorySummaryText;
        }

        RefreshPartyWorldSummaryCache(party);
    }

    private string BuildGearContinuitySummaryText(PartyRuntimeData party, int rewardCount, int autoEquipCount)
    {
        string inventorySummary = party != null && !string.IsNullOrEmpty(party.InventorySummaryText)
            ? party.InventorySummaryText
            : "Owned 0 | Equipped 0 | Stored 0";

        if (rewardCount <= 0 && autoEquipCount <= 0)
        {
            return inventorySummary;
        }

        return inventorySummary + " | New " + rewardCount + " | Auto-equipped " + autoEquipCount;
    }

    private PrototypeRpgEquipmentDefinition GetEquippedDefinition(PartyRuntimeData party, PartyMemberProgressionData member, string slotKey)
    {
        if (member == null)
        {
            return null;
        }

        string normalizedSlotKey = NormalizeEquipmentSlotKey(slotKey);
        if (party != null &&
            member.EquippedItemInstanceIdBySlotKey.TryGetValue(normalizedSlotKey, out string itemInstanceId))
        {
            PartyInventoryItemData inventoryItem = FindPartyInventoryItem(party, itemInstanceId);
            PrototypeRpgEquipmentDefinition inventoryDefinition = ResolvePartyInventoryItemDefinition(inventoryItem, member.RoleTag);
            if (inventoryDefinition != null &&
                NormalizeEquipmentSlotKey(inventoryDefinition.SlotKey) == normalizedSlotKey &&
                PrototypeRpgEquipmentCatalog.IsLegalForRole(inventoryDefinition, member.RoleTag))
            {
                return inventoryDefinition;
            }
        }

        return PrototypeRpgEquipmentCatalog.GetStarterDefinition(member.RoleTag, normalizedSlotKey)
            ?? PrototypeRpgEquipmentCatalog.GetFallbackDefinitionForRoleTag(member.RoleTag);
    }

    private PrototypeRpgEquipmentDefinition ResolvePartyInventoryItemDefinition(PartyInventoryItemData item, string roleTag)
    {
        return item == null || string.IsNullOrEmpty(item.EquipmentLoadoutId)
            ? null
            : PrototypeRpgEquipmentCatalog.ResolveDefinition(item.EquipmentLoadoutId, roleTag);
    }

    private PartyInventoryItemData FindPartyInventoryItem(PartyRuntimeData party, string itemInstanceId)
    {
        if (party == null || party.InventoryItems == null || party.InventoryItems.Count <= 0 || string.IsNullOrEmpty(itemInstanceId))
        {
            return null;
        }

        for (int i = 0; i < party.InventoryItems.Count; i++)
        {
            PartyInventoryItemData item = party.InventoryItems[i];
            if (item != null && item.ItemInstanceId == itemInstanceId)
            {
                return item;
            }
        }

        return null;
    }

    private string BuildEquipmentRewardEntryText(PartyMemberProgressionData member, PrototypeRpgEquipmentDefinition definition)
    {
        if (member == null || definition == null)
        {
            return "None";
        }

        return member.DisplayName + " found " + PrototypeRpgEquipmentCatalog.BuildCompactReadbackText(definition);
    }

    private string BuildReplacementEquipmentLabel(PrototypeRpgEquipmentDefinition currentDefinition, PartyMemberProgressionData member, string slotKey)
    {
        if (currentDefinition != null)
        {
            return currentDefinition.DisplayName;
        }

        PrototypeRpgEquipmentDefinition starterDefinition = member != null
            ? PrototypeRpgEquipmentCatalog.GetStarterDefinition(member.RoleTag, slotKey)
            : null;
        return starterDefinition != null ? starterDefinition.DisplayName : "Starter gear";
    }

    private string BuildEquipmentReservationKey(string memberId, string slotKey)
    {
        return (string.IsNullOrEmpty(memberId) ? string.Empty : memberId) + "|" + NormalizeEquipmentSlotKey(slotKey);
    }

    private string NormalizeEquipmentSlotKey(string slotKey)
    {
        return string.IsNullOrWhiteSpace(slotKey) ? string.Empty : slotKey.Trim().ToLowerInvariant();
    }

    private string JoinSummaryText(List<string> parts, string separator, string fallback)
    {
        if (parts == null || parts.Count <= 0)
        {
            return fallback;
        }

        string summary = string.Empty;
        for (int i = 0; i < parts.Count; i++)
        {
            string part = parts[i];
            if (string.IsNullOrEmpty(part))
            {
                continue;
            }

            summary = string.IsNullOrEmpty(summary) ? part : summary + separator + part;
        }

        return string.IsNullOrEmpty(summary) ? fallback : summary;
    }

    private string ResolveRewardLabel(string rewardId)
    {
        switch (rewardId)
        {
            case "gear_fragment":
                return "Gear Fragment";
            case "pressure_token":
                return "Pressure Token";
            case "arcane_shard":
                return "Arcane Shard";
            case "recovery_token":
                return "Recovery Token";
            case "volatile_essence":
                return "Volatile Essence";
            case "cache_token":
                return "Cache Token";
            case "field_scrap":
                return "Field Scrap";
            default:
                return string.IsNullOrWhiteSpace(rewardId) ? "None" : rewardId;
        }
    }

    private bool TryAdvancePartyPromotion(PartyRuntimeData party)
    {
        if (party == null)
        {
            return false;
        }

        string nextPromotionStateId = PrototypeRpgPartyCatalog.GetNextPromotionStateId(party.PromotionStateId);
        if (string.IsNullOrEmpty(nextPromotionStateId) || nextPromotionStateId == party.PromotionStateId)
        {
            return false;
        }

        party.PromotionStateId = nextPromotionStateId;
        UpdatePartyDerivedStats(party);
        return true;
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

    private PartyRuntimeData FindFirstActiveParty()
    {
        for (int i = 0; i < _parties.Count; i++)
        {
            PartyRuntimeData party = _parties[i];
            if (party != null && party.State == PartyState.OnExpedition)
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









