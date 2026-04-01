using UnityEngine;

public sealed partial class StaticPlaceholderWorldView
{
    public const int CityCount = 2;
    public const int DungeonCount = 2;
    public const int RoadCount = 1;

    private readonly WorldData _worldData;
    private readonly ManualTradeRuntimeState _runtimeEconomyState;
    private GameObject _root;
    private Sprite _sprite;
    private Texture2D _texture;
    private WorldSelectableMarker _selectedMarker;

    public StaticPlaceholderWorldView(ResourceData[] resources)
    {
        _worldData = PlaceholderWorldDataFactory.Create();
        _runtimeEconomyState = new ManualTradeRuntimeState(_worldData, resources);
        InitializeDungeonRunSystems();
    }

    public int EntityCount => _worldData != null ? _worldData.Entities.Length : 0;
    public int RouteCount => _worldData != null ? _worldData.Routes.Length : 0;
    public int TradeOpportunityCount => CurrentTradeScanResult.Opportunities.Length;
    public int ActiveTradeOpportunityCount => CurrentTradeScanResult.Opportunities.Length;
    public int UnmetCityNeedCount => CurrentTradeScanResult.UnmetCityNeeds.Length;
    public bool AutoTickEnabled => _runtimeEconomyState != null && _runtimeEconomyState.AutoTickEnabled;
    public bool AutoTickPaused => _runtimeEconomyState != null && _runtimeEconomyState.AutoTickPaused;
    public float TickIntervalSeconds => _runtimeEconomyState != null ? _runtimeEconomyState.TickIntervalSeconds : 0f;
    public int AutoTickCount => _runtimeEconomyState != null ? _runtimeEconomyState.AutoTickCount : 0;
    public int TradeStepCount => _runtimeEconomyState != null ? _runtimeEconomyState.TradeStepCount : 0;
    public int WorldDayCount => _runtimeEconomyState != null ? _runtimeEconomyState.WorldDayCount : 0;
    public int LastDayProducedTotal => _runtimeEconomyState != null ? _runtimeEconomyState.LastDayProducedTotal : 0;
    public int LastDayClaimedDungeonOutputsTotal => _runtimeEconomyState != null ? _runtimeEconomyState.LastDayClaimedDungeonOutputsTotal : 0;
    public int LastDayTradedTotal => _runtimeEconomyState != null ? _runtimeEconomyState.LastDayTradedTotal : 0;
    public int LastDayProcessedTotal => _runtimeEconomyState != null ? _runtimeEconomyState.LastDayProcessedTotal : 0;
    public int LastDayConsumedTotal => _runtimeEconomyState != null ? _runtimeEconomyState.LastDayConsumedTotal : 0;
    public int LastDayCriticalFulfilledTotal => _runtimeEconomyState != null ? _runtimeEconomyState.LastDayCriticalFulfilledTotal : 0;
    public int LastDayCriticalUnmetTotal => _runtimeEconomyState != null ? _runtimeEconomyState.LastDayCriticalUnmetTotal : 0;
    public int LastDayNormalFulfilledTotal => _runtimeEconomyState != null ? _runtimeEconomyState.LastDayNormalFulfilledTotal : 0;
    public int LastDayNormalUnmetTotal => _runtimeEconomyState != null ? _runtimeEconomyState.LastDayNormalUnmetTotal : 0;
    public int LastDayFulfilledTotal => _runtimeEconomyState != null ? _runtimeEconomyState.LastDayFulfilledTotal : 0;
    public int LastDayUnmetTotal => _runtimeEconomyState != null ? _runtimeEconomyState.LastDayUnmetTotal : 0;
    public int LastDayShortagesTotal => _runtimeEconomyState != null ? _runtimeEconomyState.LastDayShortagesTotal : 0;
    public int LastDayProcessingBlockedTotal => _runtimeEconomyState != null ? _runtimeEconomyState.LastDayProcessingBlockedTotal : 0;
    public int LastDayProcessingReservedTotal => _runtimeEconomyState != null ? _runtimeEconomyState.LastDayProcessingReservedTotal : 0;
    public int TotalShortages => _runtimeEconomyState != null ? _runtimeEconomyState.TotalShortages : 0;
    public int CurrentUnclaimedDungeonOutputsTotal => _runtimeEconomyState != null ? _runtimeEconomyState.CurrentUnclaimedDungeonOutputsTotal : 0;
    public string CitiesWithShortagesText => GetCitiesWithShortagesText();
    public string CitiesWithSurplusText => GetCitiesWithSurplusText();
    public string CitiesWithProcessingText => GetCitiesWithProcessingText();
    public string CitiesWithCriticalUnmetText => GetCitiesWithCriticalUnmetText();
    public string FirstRouteId => GetFirstRoute() != null ? GetFirstRoute().Id : "None";
    public string FirstRouteTagsText => GetFirstRoute() != null && GetFirstRoute().Tags.Length > 0
        ? string.Join(", ", GetFirstRoute().Tags)
        : "None";
    public string FirstRouteLinkText => GetRouteLinkText(GetFirstRoute());
    public string FirstRouteCapacityText => GetFirstRouteCapacityText();
    public string FirstRouteUsageText => GetFirstRouteUsageText();
    public string FirstRouteUtilizationText => GetFirstRouteUtilizationText();
    public string RouteCapacityUsedText => GetRouteCapacityUsedText();
    public string SaturatedRoutesText => GetSaturatedRoutesText();
    public string TradeLink1Text => GetTradeLinkText(0);
    public string TradeLink2Text => GetTradeLinkText(1);
    public string DungeonOutputHintText => BuildDungeonOutputHint();
    public string RecentDayLog1Text => _runtimeEconomyState != null ? _runtimeEconomyState.GetRecentDayLogText(0) : "None";
    public string RecentDayLog2Text => _runtimeEconomyState != null ? _runtimeEconomyState.GetRecentDayLogText(1) : "None";
    public string RecentDayLog3Text => _runtimeEconomyState != null ? _runtimeEconomyState.GetRecentDayLogText(2) : "None";
    public int TotalParties => _runtimeEconomyState != null ? _runtimeEconomyState.TotalParties : 0;
    public int IdleParties => _runtimeEconomyState != null ? _runtimeEconomyState.IdleParties : 0;
    public int ActiveExpeditions => _runtimeEconomyState != null ? _runtimeEconomyState.ActiveExpeditions : 0;
    public int AvailableContracts => _runtimeEconomyState != null ? _runtimeEconomyState.AvailableContracts : 0;
    public int ExpeditionSuccessCount => _runtimeEconomyState != null ? _runtimeEconomyState.ExpeditionSuccessCount : 0;
    public int ExpeditionFailureCount => _runtimeEconomyState != null ? _runtimeEconomyState.ExpeditionFailureCount : 0;
    public int ExpeditionLootReturnedTotal => _runtimeEconomyState != null ? _runtimeEconomyState.ExpeditionLootReturnedTotal : 0;
    public string RecentExpeditionLog1Text => _runtimeEconomyState != null ? _runtimeEconomyState.GetRecentExpeditionLogText(0) : "None";
    public string RecentExpeditionLog2Text => _runtimeEconomyState != null ? _runtimeEconomyState.GetRecentExpeditionLogText(1) : "None";
    public string RecentExpeditionLog3Text => _runtimeEconomyState != null ? _runtimeEconomyState.GetRecentExpeditionLogText(2) : "None";
    public string SelectedDisplayName => _selectedMarker != null ? _selectedMarker.EntityData.DisplayName : "None";
    public string SelectedId => _selectedMarker != null ? _selectedMarker.EntityData.Id : "None";
    public string SelectedKind => _selectedMarker != null ? _selectedMarker.EntityData.Kind.ToString() : "None";
    public string SelectedPositionText => _selectedMarker != null
        ? _selectedMarker.EntityData.Position.x.ToString("0.00") + ", " + _selectedMarker.EntityData.Position.y.ToString("0.00")
        : "None";
    public string SelectedResourcesText => _selectedMarker != null && _selectedMarker.EntityData.RelatedResourceIds.Length > 0
        ? string.Join(", ", _selectedMarker.EntityData.RelatedResourceIds)
        : "None";
    public string SelectedResourceRolesText => _selectedMarker != null && _selectedMarker.EntityData.ResourceRoleTags.Length > 0
        ? string.Join(", ", _selectedMarker.EntityData.ResourceRoleTags)
        : "None";
    public string SelectedSupplyText => _selectedMarker != null && _selectedMarker.EntityData.SupplyResourceIds.Length > 0
        ? string.Join(", ", _selectedMarker.EntityData.SupplyResourceIds)
        : "None";
    public string SelectedNeedsText => _selectedMarker != null && _selectedMarker.EntityData.NeedResourceIds.Length > 0
        ? string.Join(", ", _selectedMarker.EntityData.NeedResourceIds)
        : "None";
    public string SelectedHighPriorityNeedsText => GetSelectedHighPriorityNeedsText();
    public string SelectedNormalPriorityNeedsText => GetSelectedNormalPriorityNeedsText();
    public string SelectedOutputText => GetSelectedOutputText();
    public string SelectedProcessingText => GetSelectedProcessingText();
    public string SelectedLinkedCityText => GetSelectedLinkedCityText();
    public string SelectedSurplusText => GetSelectedSurplusText();
    public string SelectedDeficitText => GetSelectedDeficitText();
    public string SelectedIdentityText => GetSelectedIdentityText();
    public string SelectedReserveStockRuleText => GetSelectedReserveStockRuleText();
    public string SelectedProcessingPreferenceText => GetSelectedProcessingPreferenceText();
    public string SelectedTotalFulfilledText => GetSelectedTotalFulfilledText();
    public string SelectedTotalUnmetText => GetSelectedTotalUnmetText();
    public string SelectedTotalCriticalUnmetText => GetSelectedTotalCriticalUnmetText();
    public string SelectedTotalNormalUnmetText => GetSelectedTotalNormalUnmetText();
    public string SelectedTotalShortagesText => GetSelectedTotalShortagesText();
    public string SelectedStocksText => _runtimeEconomyState != null && _selectedMarker != null
        ? _runtimeEconomyState.GetStocksText(_selectedMarker.EntityData.Id)
        : "None";
    public string SelectedLastDayProducedText => GetSelectedLastDayValueText(SelectedLastDayMetric.Produced);
    public string SelectedLastDayDungeonImportedText => GetSelectedLastDayValueText(SelectedLastDayMetric.DungeonImported);
    public string SelectedLastDayImportedText => GetSelectedLastDayValueText(SelectedLastDayMetric.Imported);
    public string SelectedLastDayExportedText => GetSelectedLastDayValueText(SelectedLastDayMetric.Exported);
    public string SelectedLastDayProcessedInText => GetSelectedLastDayValueText(SelectedLastDayMetric.ProcessedIn);
    public string SelectedLastDayProcessedOutText => GetSelectedLastDayValueText(SelectedLastDayMetric.ProcessedOut);
    public string SelectedLastDayProcessedTotalText => GetSelectedLastDayValueText(SelectedLastDayMetric.ProcessedTotal);
    public string SelectedLastDayConsumedText => GetSelectedLastDayValueText(SelectedLastDayMetric.Consumed);
    public string SelectedLastDayCriticalFulfilledText => GetSelectedLastDayValueText(SelectedLastDayMetric.CriticalFulfilled);
    public string SelectedLastDayCriticalUnmetText => GetSelectedLastDayValueText(SelectedLastDayMetric.CriticalUnmet);
    public string SelectedLastDayNormalFulfilledText => GetSelectedLastDayValueText(SelectedLastDayMetric.NormalFulfilled);
    public string SelectedLastDayNormalUnmetText => GetSelectedLastDayValueText(SelectedLastDayMetric.NormalUnmet);
    public string SelectedLastDayFulfilledText => GetSelectedLastDayValueText(SelectedLastDayMetric.Fulfilled);
    public string SelectedLastDayUnmetText => GetSelectedLastDayValueText(SelectedLastDayMetric.Unmet);
    public string SelectedLastDayShortagesText => GetSelectedLastDayValueText(SelectedLastDayMetric.Shortages);
    public string SelectedLastDayProcessingBlockedText => GetSelectedLastDayValueText(SelectedLastDayMetric.ProcessingBlocked);
    public string SelectedLastDayProcessingReservedText => GetSelectedLastDayValueText(SelectedLastDayMetric.ProcessingReserved);
    public string SelectedLastDayClaimedOutText => GetSelectedLastDayValueText(SelectedLastDayMetric.ClaimedOut);
    public string SelectedIncomingTradeText => GetSelectedIncomingTradeText();
    public string SelectedOutgoingTradeText => GetSelectedOutgoingTradeText();
    public string SelectedUnmetNeedsText => GetSelectedUnmetNeedsText();
    public string SelectedTagsText => _selectedMarker != null && _selectedMarker.EntityData.Tags.Length > 0
        ? string.Join(", ", _selectedMarker.EntityData.Tags)
        : "None";
    public string SelectedStatText => _selectedMarker != null && !string.IsNullOrEmpty(_selectedMarker.EntityData.PrimaryStatName)
        ? _selectedMarker.EntityData.PrimaryStatName + " = " + _selectedMarker.EntityData.PrimaryStatValue
        : "None";
    public string SelectedPartiesInCityText => GetSelectedPartiesInCityText();
    public string SelectedIdlePartiesText => GetSelectedIdlePartiesText();
    public string SelectedActiveExpeditionsFromCityText => GetSelectedActiveExpeditionsFromCityText();
    public string SelectedAvailableContractText => GetSelectedAvailableContractText();
    public string SelectedLinkedDungeonText => GetSelectedLinkedDungeonText();
    public string SelectedDungeonDangerText => GetSelectedDungeonDangerText();
    public string SelectedCityManaShardStockText => GetSelectedCityManaShardStockText();

    public string SelectedNeedPressureText => GetSelectedNeedPressureText();
    public string SelectedDispatchReadinessText => GetSelectedDispatchReadinessText();
    public string SelectedDispatchRecoveryProgressText => GetSelectedDispatchRecoveryProgressText();
    public string SelectedRecommendedRouteText => GetSelectedRecommendedRouteText();

    public string SelectedRecommendedRouteForLinkedCityText => GetSelectedRecommendedRouteForLinkedCityText();
    public string SelectedRecommendationReasonText => GetSelectedRecommendationReasonText();
    public string SelectedLastDispatchImpactText => GetSelectedLastDispatchImpactText();
    public string SelectedLastDispatchStockDeltaText => GetSelectedLastDispatchStockDeltaText();
    public string SelectedLastNeedPressureChangeText => GetSelectedLastNeedPressureChangeText();
    public string SelectedLastDispatchReadinessChangeText => GetSelectedLastDispatchReadinessChangeText();
    public string SelectedRecoveryEtaText => GetSelectedRecoveryEtaText();
    public string SelectedRecommendedPowerText => GetSelectedRecommendedPowerText();
    public string SelectedExpeditionDurationDaysText => GetSelectedExpeditionDurationDaysText();
    public string SelectedRewardPreviewText => GetSelectedRewardPreviewText();
    public string SelectedEventPreviewText => GetSelectedEventPreviewText();
    public string SelectedRoutePreview1Text => GetSelectedRoutePreview1Text();
    public string SelectedRoutePreview2Text => GetSelectedRoutePreview2Text();
    public string SelectedActiveExpeditionsText => GetSelectedActiveExpeditionsText();
    public string SelectedLastExpeditionResultText => GetSelectedLastExpeditionResultText();
    public string SelectedExpeditionLootReturnedText => GetSelectedExpeditionLootReturnedText();
    public string SelectedLastRunSurvivingMembersText => GetSelectedLastRunSurvivingMembersText();
    public string SelectedLastRunClearedEncountersText => GetSelectedLastRunClearedEncountersText();
    public string SelectedLastRunEventChoiceText => GetSelectedLastRunEventChoiceText();
    public string SelectedLastRunLootBreakdownText => GetSelectedLastRunLootBreakdownText();
    public string SelectedLastRunDungeonText => GetSelectedLastRunDungeonText();
    public string SelectedLastRunRouteText => GetSelectedLastRunRouteText();
    private DirectTradeScanResult CurrentTradeScanResult => _runtimeEconomyState != null
        ? _runtimeEconomyState.CurrentTradeScanResult
        : DirectTradeScanResult.Empty;

    private enum SelectedLastDayMetric
    {
        Produced,
        DungeonImported,
        Imported,
        Exported,
        ProcessedIn,
        ProcessedOut,
        ProcessedTotal,
        Consumed,
        CriticalFulfilled,
        CriticalUnmet,
        NormalFulfilled,
        NormalUnmet,
        Fulfilled,
        Unmet,
        Shortages,
        ProcessingBlocked,
        ProcessingReserved,
        ClaimedOut
    }

    public void SetVisible(bool isVisible)
    {
        if (isVisible)
        {
            EnsureCreated();
        }
        else
        {
            SetSelected(null);
        }

        if (_root != null)
        {
            _root.SetActive(isVisible);
        }
    }

    public void SelectAtScreenPosition(Camera worldCamera, Vector2 screenPosition)
    {
        if (_root == null || !_root.activeInHierarchy || worldCamera == null)
        {
            return;
        }

        Vector3 worldPoint = worldCamera.ScreenToWorldPoint(screenPosition);
        Collider2D hit = Physics2D.OverlapPoint(new Vector2(worldPoint.x, worldPoint.y));
        WorldSelectableMarker marker = hit != null ? hit.GetComponent<WorldSelectableMarker>() : null;
        SetSelected(marker);
    }

    public void RunEconomyDay()
    {
        if (_runtimeEconomyState == null)
        {
            return;
        }

        int previousWorldDayCount = _runtimeEconomyState.WorldDayCount;
        _runtimeEconomyState.RunEconomyDay();
        AdvanceDispatchRecoveryForEconomyDays(previousWorldDayCount, _runtimeEconomyState.WorldDayCount);
    }

    public void UpdateAutoTick(float deltaTime)
    {
        if (_runtimeEconomyState == null)
        {
            return;
        }

        int previousWorldDayCount = _runtimeEconomyState.WorldDayCount;
        _runtimeEconomyState.UpdateAutoTick(deltaTime);
        AdvanceDispatchRecoveryForEconomyDays(previousWorldDayCount, _runtimeEconomyState.WorldDayCount);
    }

    public void ToggleAutoTickEnabled()
    {
        if (_runtimeEconomyState == null)
        {
            return;
        }

        _runtimeEconomyState.ToggleAutoTickEnabled();
    }

    public void ToggleAutoTickPaused()
    {
        if (_runtimeEconomyState == null)
        {
            return;
        }

        _runtimeEconomyState.ToggleAutoTickPaused();
    }

    public void ResetRuntimeEconomy()
    {
        if (_runtimeEconomyState == null)
        {
            return;
        }

        _runtimeEconomyState.Reset();
        ResetDungeonRunSystems();
    }

    public void RecruitSelectedCityParty()
    {
        if (_runtimeEconomyState == null || _selectedMarker == null || _selectedMarker.EntityData.Kind != WorldEntityKind.City)
        {
            return;
        }

        string cityId = _selectedMarker.EntityData.Id;
        if (!_runtimeEconomyState.RecruitParty(cityId))
        {
            return;
        }

        string partyId = _runtimeEconomyState.GetIdlePartyIdInCity(cityId);
        GetOrCreateDungeonParty(cityId, partyId);
    }

    public void DispatchSelectedCityExpedition()
    {
        if (_runtimeEconomyState == null || _selectedMarker == null || _selectedMarker.EntityData.Kind != WorldEntityKind.City)
        {
            return;
        }

        _runtimeEconomyState.DispatchExpedition(_selectedMarker.EntityData.Id);
    }

    public void Dispose()
    {
        SetSelected(null);

        if (_root != null)
        {
            Object.Destroy(_root);
            _root = null;
        }

        if (_sprite != null)
        {
            Object.Destroy(_sprite);
            _sprite = null;
        }

        if (_texture != null)
        {
            Object.Destroy(_texture);
            _texture = null;
        }

        DisposeDungeonRunSystems();
    }

    private void EnsureCreated()
    {
        if (_root != null)
        {
            return;
        }

        _texture = new Texture2D(1, 1, TextureFormat.RGBA32, false);
        _texture.name = "PlaceholderWorldTexture";
        _texture.filterMode = FilterMode.Point;
        _texture.wrapMode = TextureWrapMode.Clamp;
        _texture.SetPixel(0, 0, Color.white);
        _texture.Apply();

        _sprite = Sprite.Create(_texture, new Rect(0f, 0f, 1f, 1f), new Vector2(0.5f, 0.5f), 1f);
        _sprite.name = "PlaceholderWorldSprite";

        _root = new GameObject("StaticPlaceholderWorld");

        if (_worldData == null)
        {
            return;
        }

        for (int i = 0; i < _worldData.Routes.Length; i++)
        {
            WorldRouteData route = _worldData.Routes[i];
            WorldEntityData fromEntity = FindEntity(route.FromEntityId);
            WorldEntityData toEntity = FindEntity(route.ToEntityId);
            if (fromEntity == null || toEntity == null)
            {
                continue;
            }

            CreateRoad(route, fromEntity.Position, toEntity.Position, new Color(0.58f, 0.52f, 0.42f, 1f));
        }

        for (int i = 0; i < _worldData.Entities.Length; i++)
        {
            CreateSelectableMarker(_worldData.Entities[i]);
        }
    }

    private WorldRouteData GetFirstRoute()
    {
        return _worldData != null && _worldData.Routes.Length > 0 ? _worldData.Routes[0] : null;
    }

    private DirectTradeOpportunityData GetTradeOpportunity(int index)
    {
        return index >= 0 && index < CurrentTradeScanResult.Opportunities.Length
            ? CurrentTradeScanResult.Opportunities[index]
            : null;
    }

    private string GetRouteLinkText(WorldRouteData route)
    {
        if (route == null)
        {
            return "None";
        }

        WorldEntityData fromEntity = FindEntity(route.FromEntityId);
        WorldEntityData toEntity = FindEntity(route.ToEntityId);
        if (fromEntity == null || toEntity == null)
        {
            return "None";
        }

        return fromEntity.DisplayName + " <-> " + toEntity.DisplayName;
    }

    private string GetFirstRouteCapacityText()
    {
        WorldRouteData route = GetFirstRoute();
        return route != null ? route.CapacityPerDay.ToString() : "None";
    }

    private string GetFirstRouteUsageText()
    {
        if (_runtimeEconomyState == null)
        {
            return "None";
        }

        WorldRouteData route = GetFirstRoute();
        return route != null ? _runtimeEconomyState.GetLastDayRouteUsage(route.Id).ToString() : "None";
    }

    private string GetFirstRouteUtilizationText()
    {
        if (_runtimeEconomyState == null)
        {
            return "None";
        }

        WorldRouteData route = GetFirstRoute();
        return route != null ? _runtimeEconomyState.GetRouteUtilizationText(route.Id) : "None";
    }

    private string GetRouteCapacityUsedText()
    {
        if (_worldData == null || _runtimeEconomyState == null)
        {
            return "None";
        }

        int totalCapacity = 0;
        WorldRouteData[] routes = _worldData.Routes ?? System.Array.Empty<WorldRouteData>();
        for (int i = 0; i < routes.Length; i++)
        {
            if (routes[i] != null)
            {
                totalCapacity += routes[i].CapacityPerDay;
            }
        }

        return _runtimeEconomyState.LastDayRouteCapacityUsedTotal + "/" + totalCapacity;
    }

    private string GetSaturatedRoutesText()
    {
        if (_worldData == null || _runtimeEconomyState == null)
        {
            return "None";
        }

        string text = string.Empty;
        WorldRouteData[] routes = _worldData.Routes ?? System.Array.Empty<WorldRouteData>();
        for (int i = 0; i < routes.Length; i++)
        {
            WorldRouteData route = routes[i];
            if (route == null || route.CapacityPerDay < 1 || _runtimeEconomyState.GetLastDayRouteUsage(route.Id) < route.CapacityPerDay)
            {
                continue;
            }

            text = string.IsNullOrEmpty(text) ? route.Id : text + ", " + route.Id;
        }

        return string.IsNullOrEmpty(text) ? "None" : text;
    }

    private string GetTradeLinkText(int index)
    {
        if (_runtimeEconomyState != null)
        {
            string lastDayText = _runtimeEconomyState.GetLastDayTradeEventText(index);
            if (lastDayText != "None")
            {
                return lastDayText;
            }
        }

        DirectTradeOpportunityData opportunity = GetTradeOpportunity(index);
        if (opportunity == null)
        {
            return "None";
        }

        WorldEntityData supplier = FindEntity(opportunity.SupplierEntityId);
        WorldEntityData consumer = FindEntity(opportunity.ConsumerEntityId);
        if (supplier == null || consumer == null)
        {
            return "None";
        }

        return supplier.DisplayName + " -> " + consumer.DisplayName + " : " + opportunity.ResourceId;
    }

    private string BuildDungeonOutputHint()
    {
        if (_runtimeEconomyState != null)
        {
            string lastDayText = _runtimeEconomyState.GetLastDayClaimEventText(0);
            if (lastDayText != "None")
            {
                return lastDayText;
            }
        }

        if (_runtimeEconomyState == null || _worldData == null)
        {
            return "None";
        }

        WorldEntityData[] entities = _worldData.Entities ?? System.Array.Empty<WorldEntityData>();
        for (int entityIndex = 0; entityIndex < entities.Length; entityIndex++)
        {
            WorldEntityData dungeon = entities[entityIndex];
            if (dungeon == null || dungeon.Kind != WorldEntityKind.Dungeon)
            {
                continue;
            }

            string[] outputResourceIds = dungeon.OutputResourceIds ?? System.Array.Empty<string>();
            for (int outputIndex = 0; outputIndex < outputResourceIds.Length; outputIndex++)
            {
                string resourceId = outputResourceIds[outputIndex];
                if (string.IsNullOrEmpty(resourceId) || _runtimeEconomyState.GetStockAmount(dungeon.Id, resourceId) < 1)
                {
                    continue;
                }

                WorldEntityData linkedCity = FindEntity(dungeon.LinkedCityId);
                if (linkedCity != null)
                {
                    return dungeon.DisplayName + " -> " + linkedCity.DisplayName + " : " + resourceId;
                }

                return dungeon.DisplayName + " outputs " + resourceId + " (unclaimed)";
            }
        }

        return "None";
    }

    private string GetCitiesWithShortagesText()
    {
        if (_worldData == null || _runtimeEconomyState == null)
        {
            return "None";
        }

        string text = string.Empty;
        for (int i = 0; i < _worldData.Entities.Length; i++)
        {
            WorldEntityData entity = _worldData.Entities[i];
            if (entity == null || entity.Kind != WorldEntityKind.City || _runtimeEconomyState.GetLastDayShortages(entity.Id) < 1)
            {
                continue;
            }

            text = string.IsNullOrEmpty(text) ? entity.DisplayName : text + ", " + entity.DisplayName;
        }

        return string.IsNullOrEmpty(text) ? "None" : text;
    }

    private string GetCitiesWithSurplusText()
    {
        if (_worldData == null || _runtimeEconomyState == null)
        {
            return "None";
        }

        string text = string.Empty;
        for (int i = 0; i < _worldData.Entities.Length; i++)
        {
            WorldEntityData entity = _worldData.Entities[i];
            if (entity == null || entity.Kind != WorldEntityKind.City || !HasSurplusResource(entity))
            {
                continue;
            }

            text = string.IsNullOrEmpty(text) ? entity.DisplayName : text + ", " + entity.DisplayName;
        }

        return string.IsNullOrEmpty(text) ? "None" : text;
    }

    private string GetCitiesWithProcessingText()
    {
        if (_worldData == null)
        {
            return "None";
        }

        string text = string.Empty;
        for (int i = 0; i < _worldData.Entities.Length; i++)
        {
            WorldEntityData entity = _worldData.Entities[i];
            if (!HasProcessingRule(entity))
            {
                continue;
            }

            text = string.IsNullOrEmpty(text) ? entity.DisplayName : text + ", " + entity.DisplayName;
        }

        return string.IsNullOrEmpty(text) ? "None" : text;
    }

    private string GetCitiesWithCriticalUnmetText()
    {
        if (_worldData == null || _runtimeEconomyState == null)
        {
            return "None";
        }

        string text = string.Empty;
        for (int i = 0; i < _worldData.Entities.Length; i++)
        {
            WorldEntityData entity = _worldData.Entities[i];
            if (entity == null || entity.Kind != WorldEntityKind.City || _runtimeEconomyState.GetLastDayCriticalUnmet(entity.Id) < 1)
            {
                continue;
            }

            text = string.IsNullOrEmpty(text) ? entity.DisplayName : text + ", " + entity.DisplayName;
        }

        return string.IsNullOrEmpty(text) ? "None" : text;
    }

    private string GetSelectedHighPriorityNeedsText()
    {
        if (_selectedMarker == null || _selectedMarker.EntityData.Kind != WorldEntityKind.City)
        {
            return "None";
        }

        return BuildNeedTierText(_selectedMarker.EntityData, true);
    }

    private string GetSelectedNormalPriorityNeedsText()
    {
        if (_selectedMarker == null || _selectedMarker.EntityData.Kind != WorldEntityKind.City)
        {
            return "None";
        }

        return BuildNeedTierText(_selectedMarker.EntityData, false);
    }

    private string GetSelectedOutputText()
    {
        if (_selectedMarker == null || _selectedMarker.EntityData.OutputResourceIds.Length == 0)
        {
            return "None";
        }

        string outputText = string.Join(", ", _selectedMarker.EntityData.OutputResourceIds);
        if (_selectedMarker.EntityData.Kind == WorldEntityKind.Dungeon)
        {
            WorldEntityData linkedCity = FindEntity(_selectedMarker.EntityData.LinkedCityId);
            if (linkedCity != null)
            {
                return outputText + " -> " + linkedCity.DisplayName + " (linked inflow)";
            }

            return outputText + " (unclaimed)";
        }

        return outputText;
    }

    private string GetSelectedProcessingText()
    {
        if (_selectedMarker == null || _selectedMarker.EntityData.Kind != WorldEntityKind.City)
        {
            return "None";
        }

        return BuildProcessingText(_selectedMarker.EntityData.ProcessingRules);
    }

    private string GetSelectedLinkedCityText()
    {
        if (_selectedMarker == null || _selectedMarker.EntityData.Kind != WorldEntityKind.Dungeon)
        {
            return "None";
        }

        if (string.IsNullOrEmpty(_selectedMarker.EntityData.LinkedCityId))
        {
            return "None";
        }

        WorldEntityData linkedCity = FindEntity(_selectedMarker.EntityData.LinkedCityId);
        return linkedCity != null ? linkedCity.DisplayName : _selectedMarker.EntityData.LinkedCityId;
    }

    private string GetSelectedPartiesInCityText()
    {
        return _selectedMarker != null && _runtimeEconomyState != null && _selectedMarker.EntityData.Kind == WorldEntityKind.City
            ? _runtimeEconomyState.GetPartyCountInCity(_selectedMarker.EntityData.Id).ToString()
            : "None";
    }

    private string GetSelectedIdlePartiesText()
    {
        return _selectedMarker != null && _runtimeEconomyState != null && _selectedMarker.EntityData.Kind == WorldEntityKind.City
            ? _runtimeEconomyState.GetIdlePartyCountInCity(_selectedMarker.EntityData.Id).ToString()
            : "None";
    }

    private string GetSelectedActiveExpeditionsFromCityText()
    {
        return _selectedMarker != null && _runtimeEconomyState != null && _selectedMarker.EntityData.Kind == WorldEntityKind.City
            ? _runtimeEconomyState.GetActiveExpeditionCountFromCity(_selectedMarker.EntityData.Id).ToString()
            : "None";
    }

    private string GetSelectedAvailableContractText()
    {
        if (_selectedMarker == null || _runtimeEconomyState == null)
        {
            return "None";
        }

        return _selectedMarker.EntityData.Kind == WorldEntityKind.City
            ? _runtimeEconomyState.GetAvailableContractTextForCity(_selectedMarker.EntityData.Id)
            : _runtimeEconomyState.GetAvailableContractTextForDungeon(_selectedMarker.EntityData.Id);
    }

    private string GetSelectedLinkedDungeonText()
    {
        return _selectedMarker != null && _runtimeEconomyState != null && _selectedMarker.EntityData.Kind == WorldEntityKind.City
            ? _runtimeEconomyState.GetLinkedDungeonText(_selectedMarker.EntityData.Id)
            : "None";
    }

    private string GetSelectedCityManaShardStockText()
    {
        return _selectedMarker != null && _selectedMarker.EntityData.Kind == WorldEntityKind.City
            ? BuildCityManaShardStockText(_selectedMarker.EntityData.Id)
            : "None";
    }

    private string GetSelectedNeedPressureText()
    {
        return _selectedMarker != null && _selectedMarker.EntityData.Kind == WorldEntityKind.City
            ? BuildNeedPressureText(_selectedMarker.EntityData.Id)
            : "None";
    }

    private string GetSelectedDispatchReadinessText()
    {
        return _selectedMarker != null && _selectedMarker.EntityData.Kind == WorldEntityKind.City
            ? GetDispatchReadinessText(_selectedMarker.EntityData.Id)
            : "None";
    }

    private string GetSelectedDispatchRecoveryProgressText()
    {
        return _selectedMarker != null && _selectedMarker.EntityData.Kind == WorldEntityKind.City
            ? BuildDispatchRecoveryProgressText(_selectedMarker.EntityData.Id)
            : "None";
    }

    private string GetSelectedRecommendedRouteText()
    {
        string dungeonId = GetSelectedExpeditionDungeonId();
        return _selectedMarker != null && _selectedMarker.EntityData.Kind == WorldEntityKind.City && !string.IsNullOrEmpty(dungeonId)
            ? BuildRecommendedRouteSummaryText(_selectedMarker.EntityData.Id, dungeonId)
            : "None";
    }

    private string GetSelectedRecommendedRouteForLinkedCityText()
    {
        return _selectedMarker != null && _selectedMarker.EntityData.Kind == WorldEntityKind.Dungeon && !string.IsNullOrEmpty(_selectedMarker.EntityData.LinkedCityId)
            ? BuildRecommendedRouteSummaryText(_selectedMarker.EntityData.LinkedCityId, _selectedMarker.EntityData.Id)
            : "None";
    }

    private string GetSelectedRecommendationReasonText()
    {
        string dungeonId = GetSelectedExpeditionDungeonId();
        return _selectedMarker != null && _selectedMarker.EntityData.Kind == WorldEntityKind.City && !string.IsNullOrEmpty(dungeonId)
            ? BuildRecommendationReasonText(_selectedMarker.EntityData.Id, dungeonId)
            : "None";
    }

    private string GetSelectedLastDispatchImpactText()
    {
        return _selectedMarker != null && _selectedMarker.EntityData.Kind == WorldEntityKind.City
            ? GetLastDispatchImpactText(_selectedMarker.EntityData.Id)
            : "None";
    }

    private string GetSelectedLastDispatchStockDeltaText()
    {
        return _selectedMarker != null && _selectedMarker.EntityData.Kind == WorldEntityKind.City
            ? GetLastDispatchStockDeltaText(_selectedMarker.EntityData.Id)
            : "None";
    }

    private string GetSelectedLastNeedPressureChangeText()
    {
        return _selectedMarker != null && _selectedMarker.EntityData.Kind == WorldEntityKind.City
            ? GetLastNeedPressureChangeText(_selectedMarker.EntityData.Id)
            : "None";
    }

    private string GetSelectedLastDispatchReadinessChangeText()
    {
        return _selectedMarker != null && _selectedMarker.EntityData.Kind == WorldEntityKind.City
            ? GetLastDispatchReadinessChangeText(_selectedMarker.EntityData.Id)
            : "None";
    }

    private string GetSelectedRecoveryEtaText()
    {
        return _selectedMarker != null && _selectedMarker.EntityData.Kind == WorldEntityKind.City
            ? BuildRecoveryEtaText(GetRecoveryDaysToReady(_selectedMarker.EntityData.Id))
            : "None";
    }

    private string GetSelectedDungeonDangerText()
    {
        string dungeonId = GetSelectedExpeditionDungeonId();
        return string.IsNullOrEmpty(dungeonId)
            ? "None"
            : BuildDungeonDangerSummaryText(dungeonId);
    }

    private string GetSelectedRecommendedPowerText()
    {
        string dungeonId = GetSelectedExpeditionDungeonId();
        if (_runtimeEconomyState == null || string.IsNullOrEmpty(dungeonId))
        {
            return "None";
        }

        int value = _runtimeEconomyState.GetRecommendedPower(dungeonId);
        return value > 0 ? value.ToString() : "None";
    }

    private string GetSelectedExpeditionDurationDaysText()
    {
        string dungeonId = GetSelectedExpeditionDungeonId();
        if (_runtimeEconomyState == null || string.IsNullOrEmpty(dungeonId))
        {
            return "None";
        }

        int value = _runtimeEconomyState.GetExpeditionDurationDays(dungeonId);
        return value > 0 ? value.ToString() : "None";
    }

    private string GetSelectedRewardPreviewText()
    {
        string dungeonId = GetSelectedExpeditionDungeonId();
        return string.IsNullOrEmpty(dungeonId)
            ? "None"
            : BuildRouteRewardPreviewText(dungeonId);
    }

    private string GetSelectedEventPreviewText()
    {
        string dungeonId = GetSelectedExpeditionDungeonId();
        return string.IsNullOrEmpty(dungeonId)
            ? "None"
            : BuildRouteEventPreviewText(dungeonId);
    }

    private string GetSelectedRoutePreview1Text()
    {
        string dungeonId = GetSelectedExpeditionDungeonId();
        return string.IsNullOrEmpty(dungeonId)
            ? "None"
            : BuildRoutePreviewSummaryText(dungeonId, SafeRouteId);
    }

    private string GetSelectedRoutePreview2Text()
    {
        string dungeonId = GetSelectedExpeditionDungeonId();
        return string.IsNullOrEmpty(dungeonId)
            ? "None"
            : BuildRoutePreviewSummaryText(dungeonId, RiskyRouteId);
    }
    private string GetSelectedActiveExpeditionsText()
    {
        if (_selectedMarker == null || _runtimeEconomyState == null)
        {
            return "None";
        }

        return _selectedMarker.EntityData.Kind == WorldEntityKind.City
            ? _runtimeEconomyState.GetActiveExpeditionCountFromCity(_selectedMarker.EntityData.Id).ToString()
            : _runtimeEconomyState.GetActiveExpeditionCountForDungeon(_selectedMarker.EntityData.Id).ToString();
    }

    private string GetSelectedLastExpeditionResultText()
    {
        if (_selectedMarker == null || _runtimeEconomyState == null)
        {
            return "None";
        }

        return _selectedMarker.EntityData.Kind == WorldEntityKind.City
            ? _runtimeEconomyState.GetExpeditionStatusTextForCity(_selectedMarker.EntityData.Id)
            : _runtimeEconomyState.GetExpeditionStatusTextForDungeon(_selectedMarker.EntityData.Id);
    }

    private string GetSelectedExpeditionLootReturnedText()
    {
        return _selectedMarker != null && _runtimeEconomyState != null && _selectedMarker.EntityData.Kind == WorldEntityKind.City
            ? _runtimeEconomyState.GetLastRunLootSummaryForCity(_selectedMarker.EntityData.Id)
            : "None";
    }

    private string GetSelectedLastRunSurvivingMembersText()
    {
        return _selectedMarker != null && _runtimeEconomyState != null && _selectedMarker.EntityData.Kind == WorldEntityKind.City
            ? _runtimeEconomyState.GetLastRunSurvivingMembersSummaryForCity(_selectedMarker.EntityData.Id)
            : "None";
    }

    private string GetSelectedLastRunClearedEncountersText()
    {
        return _selectedMarker != null && _runtimeEconomyState != null && _selectedMarker.EntityData.Kind == WorldEntityKind.City
            ? _runtimeEconomyState.GetLastRunClearedEncountersSummaryForCity(_selectedMarker.EntityData.Id)
            : "None";
    }

    private string GetSelectedLastRunEventChoiceText()
    {
        return _selectedMarker != null && _runtimeEconomyState != null && _selectedMarker.EntityData.Kind == WorldEntityKind.City
            ? _runtimeEconomyState.GetLastRunEventChoiceSummaryForCity(_selectedMarker.EntityData.Id)
            : "None";
    }

    private string GetSelectedLastRunLootBreakdownText()
    {
        return _selectedMarker != null && _runtimeEconomyState != null && _selectedMarker.EntityData.Kind == WorldEntityKind.City
            ? _runtimeEconomyState.GetLastRunLootBreakdownSummaryForCity(_selectedMarker.EntityData.Id)
            : "None";
    }

    private string GetSelectedLastRunDungeonText()
    {
        return _selectedMarker != null && _runtimeEconomyState != null && _selectedMarker.EntityData.Kind == WorldEntityKind.City
            ? _runtimeEconomyState.GetLastRunDungeonSummaryForCity(_selectedMarker.EntityData.Id)
            : "None";
    }

    private string GetSelectedLastRunRouteText()
    {
        return _selectedMarker != null && _runtimeEconomyState != null && _selectedMarker.EntityData.Kind == WorldEntityKind.City
            ? _runtimeEconomyState.GetLastRunRouteSummaryForCity(_selectedMarker.EntityData.Id)
            : "None";
    }

    private string GetSelectedExpeditionDungeonId()
    {
        if (_selectedMarker == null)
        {
            return string.Empty;
        }

        return _selectedMarker.EntityData.Kind == WorldEntityKind.Dungeon
            ? _selectedMarker.EntityData.Id
            : _selectedMarker.EntityData.Kind == WorldEntityKind.City && _runtimeEconomyState != null
                ? GetLinkedDungeonIdForCity(_selectedMarker.EntityData.Id)
                : string.Empty;
    }

    private string GetLinkedDungeonIdForCity(string cityId)
    {
        if (_worldData == null || string.IsNullOrEmpty(cityId))
        {
            return string.Empty;
        }

        WorldEntityData[] entities = _worldData.Entities ?? System.Array.Empty<WorldEntityData>();
        for (int i = 0; i < entities.Length; i++)
        {
            WorldEntityData entity = entities[i];
            if (entity != null && entity.Kind == WorldEntityKind.Dungeon && entity.LinkedCityId == cityId)
            {
                return entity.Id;
            }
        }

        return string.Empty;
    }

    private string GetSelectedSurplusText()
    {
        if (_selectedMarker == null || _runtimeEconomyState == null || _selectedMarker.EntityData.Kind != WorldEntityKind.City)
        {
            return "None";
        }

        string text = string.Empty;
        for (int i = 0; i < _selectedMarker.EntityData.SupplyResourceIds.Length; i++)
        {
            string resourceId = _selectedMarker.EntityData.SupplyResourceIds[i];
            if (string.IsNullOrEmpty(resourceId))
            {
                continue;
            }

            int exportable = _runtimeEconomyState.GetExportableAmount(_selectedMarker.EntityData.Id, resourceId);
            if (exportable < 1)
            {
                continue;
            }

            string segment = resourceId + "=" + exportable + " exportable";
            text = string.IsNullOrEmpty(text) ? segment : text + ", " + segment;
        }

        return string.IsNullOrEmpty(text) ? "None" : text;
    }

    private string GetSelectedDeficitText()
    {
        if (_selectedMarker == null || _runtimeEconomyState == null || _selectedMarker.EntityData.Kind != WorldEntityKind.City)
        {
            return "None";
        }

        string text = string.Empty;
        for (int i = 0; i < _selectedMarker.EntityData.NeedResourceIds.Length; i++)
        {
            string resourceId = _selectedMarker.EntityData.NeedResourceIds[i];
            if (string.IsNullOrEmpty(resourceId))
            {
                continue;
            }

            int unmetCount = _runtimeEconomyState.GetLastDayUnmetCount(_selectedMarker.EntityData.Id, resourceId);
            if (unmetCount < 1)
            {
                continue;
            }

            string segment = resourceId + " x" + unmetCount;
            text = string.IsNullOrEmpty(text) ? segment : text + ", " + segment;
        }

        return string.IsNullOrEmpty(text) ? "None" : text;
    }

    private string GetSelectedIdentityText()
    {
        if (_selectedMarker == null || _selectedMarker.EntityData.Kind != WorldEntityKind.City)
        {
            return "None";
        }

        return BuildCityIdentityText(_selectedMarker.EntityData);
    }

    private string GetSelectedReserveStockRuleText()
    {
        if (_selectedMarker == null || _runtimeEconomyState == null)
        {
            return "None";
        }

        return _runtimeEconomyState.GetReserveStockRuleText(_selectedMarker.EntityData.Id);
    }

    private string GetSelectedProcessingPreferenceText()
    {
        if (_selectedMarker == null || _selectedMarker.EntityData.Kind != WorldEntityKind.City || !HasProcessingRule(_selectedMarker.EntityData))
        {
            return "None";
        }

        return "Process inputs before normal need consumption";
    }

    private string GetSelectedTotalFulfilledText()
    {
        if (_selectedMarker == null || _runtimeEconomyState == null || _selectedMarker.EntityData.Kind != WorldEntityKind.City)
        {
            return "None";
        }

        return _runtimeEconomyState.GetTotalFulfilled(_selectedMarker.EntityData.Id).ToString();
    }

    private string GetSelectedTotalUnmetText()
    {
        if (_selectedMarker == null || _runtimeEconomyState == null || _selectedMarker.EntityData.Kind != WorldEntityKind.City)
        {
            return "None";
        }

        return _runtimeEconomyState.GetTotalUnmet(_selectedMarker.EntityData.Id).ToString();
    }

    private string GetSelectedTotalCriticalUnmetText()
    {
        if (_selectedMarker == null || _runtimeEconomyState == null || _selectedMarker.EntityData.Kind != WorldEntityKind.City)
        {
            return "None";
        }

        return _runtimeEconomyState.GetTotalCriticalUnmet(_selectedMarker.EntityData.Id).ToString();
    }

    private string GetSelectedTotalNormalUnmetText()
    {
        if (_selectedMarker == null || _runtimeEconomyState == null || _selectedMarker.EntityData.Kind != WorldEntityKind.City)
        {
            return "None";
        }

        return _runtimeEconomyState.GetTotalNormalUnmet(_selectedMarker.EntityData.Id).ToString();
    }

    private string GetSelectedTotalShortagesText()
    {
        if (_selectedMarker == null || _runtimeEconomyState == null || _selectedMarker.EntityData.Kind != WorldEntityKind.City)
        {
            return "None";
        }

        return _runtimeEconomyState.GetTotalShortages(_selectedMarker.EntityData.Id).ToString();
    }

    private string GetSelectedLastDayValueText(SelectedLastDayMetric metric)
    {
        if (_selectedMarker == null || _runtimeEconomyState == null)
        {
            return "None";
        }

        string entityId = _selectedMarker.EntityData.Id;
        int value = metric == SelectedLastDayMetric.Produced
            ? _runtimeEconomyState.GetLastDayProduced(entityId)
            : metric == SelectedLastDayMetric.DungeonImported
                ? _runtimeEconomyState.GetLastDayDungeonImported(entityId)
                : metric == SelectedLastDayMetric.Imported
                    ? _runtimeEconomyState.GetLastDayImported(entityId)
                    : metric == SelectedLastDayMetric.Exported
                        ? _runtimeEconomyState.GetLastDayExported(entityId)
                        : metric == SelectedLastDayMetric.ProcessedIn
                            ? _runtimeEconomyState.GetLastDayProcessedIn(entityId)
                            : metric == SelectedLastDayMetric.ProcessedOut
                                ? _runtimeEconomyState.GetLastDayProcessedOut(entityId)
                                : metric == SelectedLastDayMetric.ProcessedTotal
                                    ? _runtimeEconomyState.GetLastDayProcessedTotal(entityId)
                                    : metric == SelectedLastDayMetric.Consumed
                                        ? _runtimeEconomyState.GetLastDayConsumed(entityId)
                                        : metric == SelectedLastDayMetric.CriticalFulfilled
                                            ? _runtimeEconomyState.GetLastDayCriticalFulfilled(entityId)
                                            : metric == SelectedLastDayMetric.CriticalUnmet
                                                ? _runtimeEconomyState.GetLastDayCriticalUnmet(entityId)
                                                : metric == SelectedLastDayMetric.NormalFulfilled
                                                    ? _runtimeEconomyState.GetLastDayNormalFulfilled(entityId)
                                                    : metric == SelectedLastDayMetric.NormalUnmet
                                                        ? _runtimeEconomyState.GetLastDayNormalUnmet(entityId)
                                                        : metric == SelectedLastDayMetric.Fulfilled
                                                            ? _runtimeEconomyState.GetLastDayFulfilled(entityId)
                                                            : metric == SelectedLastDayMetric.Unmet
                                                                ? _runtimeEconomyState.GetLastDayUnmet(entityId)
                                                                : metric == SelectedLastDayMetric.Shortages
                                                                    ? _runtimeEconomyState.GetLastDayShortages(entityId)
                                                                    : metric == SelectedLastDayMetric.ProcessingBlocked
                                                                        ? _runtimeEconomyState.GetLastDayProcessingBlocked(entityId)
                                                                        : metric == SelectedLastDayMetric.ProcessingReserved
                                                                            ? _runtimeEconomyState.GetLastDayProcessingReserved(entityId)
                                                                            : _runtimeEconomyState.GetLastDayClaimedOut(entityId);

        return value.ToString();
    }

    private string GetSelectedIncomingTradeText()
    {
        if (_selectedMarker == null || _selectedMarker.EntityData.Kind != WorldEntityKind.City)
        {
            return "None";
        }

        if (_runtimeEconomyState != null)
        {
            string lastDayText = _runtimeEconomyState.GetLastDayIncomingTradeText(_selectedMarker.EntityData.Id);
            if (lastDayText != "None")
            {
                return lastDayText;
            }
        }

        string text = string.Empty;
        for (int i = 0; i < CurrentTradeScanResult.Opportunities.Length; i++)
        {
            DirectTradeOpportunityData opportunity = CurrentTradeScanResult.Opportunities[i];
            if (opportunity.ConsumerEntityId != _selectedMarker.EntityData.Id)
            {
                continue;
            }

            WorldEntityData supplier = FindEntity(opportunity.SupplierEntityId);
            string segment = opportunity.ResourceId + " <- " + (supplier != null ? supplier.DisplayName : opportunity.SupplierEntityId);
            text = string.IsNullOrEmpty(text) ? segment : text + " | " + segment;
        }

        return string.IsNullOrEmpty(text) ? "None" : text;
    }

    private string GetSelectedOutgoingTradeText()
    {
        if (_selectedMarker == null || _selectedMarker.EntityData.Kind != WorldEntityKind.City)
        {
            return "None";
        }

        if (_runtimeEconomyState != null)
        {
            string lastDayText = _runtimeEconomyState.GetLastDayOutgoingTradeText(_selectedMarker.EntityData.Id);
            if (lastDayText != "None")
            {
                return lastDayText;
            }
        }

        string text = string.Empty;
        for (int i = 0; i < CurrentTradeScanResult.Opportunities.Length; i++)
        {
            DirectTradeOpportunityData opportunity = CurrentTradeScanResult.Opportunities[i];
            if (opportunity.SupplierEntityId != _selectedMarker.EntityData.Id)
            {
                continue;
            }

            WorldEntityData consumer = FindEntity(opportunity.ConsumerEntityId);
            string segment = opportunity.ResourceId + " -> " + (consumer != null ? consumer.DisplayName : opportunity.ConsumerEntityId);
            text = string.IsNullOrEmpty(text) ? segment : text + " | " + segment;
        }

        return string.IsNullOrEmpty(text) ? "None" : text;
    }

    private string GetSelectedUnmetNeedsText()
    {
        if (_selectedMarker == null || _selectedMarker.EntityData.Kind != WorldEntityKind.City)
        {
            return "None";
        }

        string text = string.Empty;
        for (int i = 0; i < CurrentTradeScanResult.UnmetCityNeeds.Length; i++)
        {
            UnmetCityNeedData unmetNeed = CurrentTradeScanResult.UnmetCityNeeds[i];
            if (unmetNeed.CityEntityId != _selectedMarker.EntityData.Id)
            {
                continue;
            }

            text = string.IsNullOrEmpty(text) ? unmetNeed.ResourceId : text + ", " + unmetNeed.ResourceId;
        }

        return string.IsNullOrEmpty(text) ? "None" : text;
    }

    private bool HasSurplusResource(WorldEntityData entity)
    {
        if (entity == null || entity.Kind != WorldEntityKind.City || _runtimeEconomyState == null)
        {
            return false;
        }

        for (int i = 0; i < entity.SupplyResourceIds.Length; i++)
        {
            string resourceId = entity.SupplyResourceIds[i];
            if (!string.IsNullOrEmpty(resourceId) && _runtimeEconomyState.GetExportableAmount(entity.Id, resourceId) > 0)
            {
                return true;
            }
        }

        return false;
    }

    private bool HasProcessingRule(WorldEntityData entity)
    {
        return entity != null && entity.Kind == WorldEntityKind.City && entity.ProcessingRules != null && entity.ProcessingRules.Length > 0;
    }

    private bool HasGatewayLink(WorldEntityData entity)
    {
        if (_worldData == null || entity == null || entity.Kind != WorldEntityKind.City)
        {
            return false;
        }

        for (int i = 0; i < _worldData.Entities.Length; i++)
        {
            WorldEntityData other = _worldData.Entities[i];
            if (other != null && other.Kind == WorldEntityKind.Dungeon && other.LinkedCityId == entity.Id)
            {
                return true;
            }
        }

        return false;
    }

    private string BuildNeedTierText(WorldEntityData entity, bool highPriorityOnly)
    {
        if (entity == null || entity.Kind != WorldEntityKind.City || entity.NeedResourceIds == null || entity.NeedResourceIds.Length == 0)
        {
            return "None";
        }

        string text = string.Empty;
        for (int i = 0; i < entity.NeedResourceIds.Length; i++)
        {
            string resourceId = entity.NeedResourceIds[i];
            if (string.IsNullOrEmpty(resourceId))
            {
                continue;
            }

            bool isHigh = entity.HighPriorityNeedResourceIds != null && System.Array.IndexOf(entity.HighPriorityNeedResourceIds, resourceId) >= 0;
            if (highPriorityOnly != isHigh)
            {
                continue;
            }

            text = string.IsNullOrEmpty(text) ? resourceId : text + ", " + resourceId;
        }

        return string.IsNullOrEmpty(text) ? "None" : text;
    }

    private string BuildProcessingText(LocalProcessingRuleData[] rules)
    {
        if (rules == null || rules.Length == 0)
        {
            return "None";
        }

        string text = string.Empty;
        for (int i = 0; i < rules.Length; i++)
        {
            LocalProcessingRuleData rule = rules[i];
            if (rule == null || string.IsNullOrEmpty(rule.InputResourceId) || string.IsNullOrEmpty(rule.OutputResourceId))
            {
                continue;
            }

            string segment = rule.InputResourceId + " -> " + rule.OutputResourceId + " (" + rule.MaxRunsPerDay + "/day)";
            text = string.IsNullOrEmpty(text) ? segment : text + ", " + segment;
        }

        return string.IsNullOrEmpty(text) ? "None" : text;
    }

    private string BuildCityIdentityText(WorldEntityData entity)
    {
        if (entity == null || entity.Kind != WorldEntityKind.City)
        {
            return "None";
        }

        string text = string.Empty;
        if (entity.SupplyResourceIds.Length > 0)
        {
            text = "Producer";
        }

        if (entity.NeedResourceIds.Length > 0 || (_runtimeEconomyState != null && _runtimeEconomyState.GetTotalShortages(entity.Id) > 0))
        {
            text = string.IsNullOrEmpty(text) ? "Consumer" : text + ", Consumer";
        }

        if (HasProcessingRule(entity))
        {
            text = string.IsNullOrEmpty(text) ? "Processor" : text + ", Processor";
        }

        if (HasGatewayLink(entity))
        {
            text = string.IsNullOrEmpty(text) ? "Gateway" : text + ", Gateway";
        }

        if (_runtimeEconomyState != null &&
            _runtimeEconomyState.GetLastDayImported(entity.Id) > 0 &&
            _runtimeEconomyState.GetLastDayProduced(entity.Id) < entity.NeedResourceIds.Length)
        {
            text = string.IsNullOrEmpty(text) ? "Import Dependent" : text + ", Import Dependent";
        }

        if (_runtimeEconomyState != null &&
            _runtimeEconomyState.GetLastDayImported(entity.Id) == 0 &&
            _runtimeEconomyState.GetLastDayFulfilled(entity.Id) > 0 &&
            _runtimeEconomyState.GetLastDayUnmet(entity.Id) == 0 &&
            _runtimeEconomyState.GetLastDayShortages(entity.Id) == 0)
        {
            text = string.IsNullOrEmpty(text) ? "Self-Sustaining" : text + ", Self-Sustaining";
        }

        return string.IsNullOrEmpty(text) ? "None" : text;
    }

    private WorldEntityData FindEntity(string entityId)
    {
        if (_worldData == null || string.IsNullOrEmpty(entityId))
        {
            return null;
        }

        for (int i = 0; i < _worldData.Entities.Length; i++)
        {
            WorldEntityData entity = _worldData.Entities[i];
            if (entity != null && entity.Id == entityId)
            {
                return entity;
            }
        }

        return null;
    }

    private void CreateSelectableMarker(WorldEntityData entity)
    {
        if (_root == null || _sprite == null || entity == null)
        {
            return;
        }

        GameObject marker = new GameObject(entity.DisplayName);
        marker.transform.SetParent(_root.transform, false);
        marker.transform.localPosition = new Vector3(entity.Position.x, entity.Position.y, 0f);
        marker.transform.localRotation = entity.Kind == WorldEntityKind.Dungeon
            ? Quaternion.Euler(0f, 0f, 45f)
            : Quaternion.identity;
        marker.transform.localScale = entity.Kind == WorldEntityKind.Dungeon
            ? new Vector3(0.85f, 0.85f, 1f)
            : new Vector3(0.9f, 0.9f, 1f);

        SpriteRenderer renderer = marker.AddComponent<SpriteRenderer>();
        renderer.sprite = _sprite;
        renderer.color = GetEntityColor(entity);
        renderer.sortingOrder = 1;

        marker.AddComponent<BoxCollider2D>();

        WorldSelectableMarker selectableMarker = marker.AddComponent<WorldSelectableMarker>();
        selectableMarker.Initialize(entity, renderer);
    }

    private void CreateRoad(WorldRouteData route, Vector2 start, Vector2 end, Color color)
    {
        if (_root == null || _sprite == null || route == null)
        {
            return;
        }

        Vector2 delta = end - start;
        float length = Mathf.Max(0.01f, delta.magnitude);
        float angle = Mathf.Atan2(delta.y, delta.x) * Mathf.Rad2Deg;
        Vector2 midpoint = (start + end) * 0.5f;

        GameObject road = new GameObject(route.Id);
        road.transform.SetParent(_root.transform, false);
        road.transform.localPosition = new Vector3(midpoint.x, midpoint.y, 0f);
        road.transform.localRotation = Quaternion.Euler(0f, 0f, angle);
        road.transform.localScale = new Vector3(length, 0.18f, 1f);

        SpriteRenderer renderer = road.AddComponent<SpriteRenderer>();
        renderer.sprite = _sprite;
        renderer.color = color;
        renderer.sortingOrder = 0;
    }

    private void SetSelected(WorldSelectableMarker nextMarker)
    {
        if (_selectedMarker == nextMarker)
        {
            return;
        }

        if (_selectedMarker != null)
        {
            _selectedMarker.SetSelected(false);
        }

        _selectedMarker = nextMarker;

        if (_selectedMarker != null)
        {
            _selectedMarker.SetSelected(true);
        }
    }

    private Color GetEntityColor(WorldEntityData entity)
    {
        if (entity == null)
        {
            return Color.white;
        }

        if (entity.Kind == WorldEntityKind.Dungeon)
        {
            return entity.Id == "dungeon-beta"
                ? new Color(0.44f, 0.74f, 0.35f, 1f)
                : new Color(0.9f, 0.35f, 0.45f, 1f);
        }

        return entity.Id == "city-b"
            ? new Color(0.95f, 0.71f, 0.31f, 1f)
            : new Color(0.3f, 0.72f, 0.95f, 1f);
    }
}

public sealed class WorldSelectableMarker : MonoBehaviour
{
    public WorldEntityData EntityData { get; private set; }

    private SpriteRenderer _renderer;
    private Vector3 _baseScale;
    private Color _baseColor;

    public void Initialize(WorldEntityData entityData, SpriteRenderer renderer)
    {
        EntityData = entityData;
        _renderer = renderer;
        _baseScale = transform.localScale;
        _baseColor = renderer != null ? renderer.color : Color.white;
    }

    public void SetSelected(bool isSelected)
    {
        transform.localScale = isSelected ? _baseScale * 1.15f : _baseScale;

        if (_renderer != null)
        {
            _renderer.color = isSelected ? Color.Lerp(_baseColor, Color.white, 0.35f) : _baseColor;
        }
    }
}






