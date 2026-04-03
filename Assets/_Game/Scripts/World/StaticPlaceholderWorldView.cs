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

        CreateWorldBackdrop();

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

    private Rect GetWorldBoardBounds()
    {
        if (_worldData == null || _worldData.Entities == null || _worldData.Entities.Length == 0)
        {
            return new Rect(-6.5f, -5f, 13f, 10f);
        }

        float minX = float.MaxValue;
        float maxX = float.MinValue;
        float minY = float.MaxValue;
        float maxY = float.MinValue;
        for (int i = 0; i < _worldData.Entities.Length; i++)
        {
            WorldEntityData entity = _worldData.Entities[i];
            if (entity == null)
            {
                continue;
            }

            minX = Mathf.Min(minX, entity.Position.x);
            maxX = Mathf.Max(maxX, entity.Position.x);
            minY = Mathf.Min(minY, entity.Position.y);
            maxY = Mathf.Max(maxY, entity.Position.y);
        }

        if (minX > maxX || minY > maxY)
        {
            return new Rect(-6.5f, -5f, 13f, 10f);
        }

        const float horizontalPadding = 3.1f;
        const float verticalPadding = 2.8f;
        return Rect.MinMaxRect(minX - horizontalPadding, minY - verticalPadding, maxX + horizontalPadding, maxY + verticalPadding);
    }

    private void CreateWorldBackdrop()
    {
        if (_root == null || _sprite == null)
        {
            return;
        }

        Rect bounds = GetWorldBoardBounds();
        Vector2 center = bounds.center;
        Vector2 size = bounds.size;
        CreateWorldVisual(_root.transform, "BoardShadow", center + new Vector2(0.18f, -0.16f), size + new Vector2(1.2f, 1.0f), new Color(0.01f, 0.02f, 0.03f, 0.68f), -30);
        CreateWorldVisual(_root.transform, "BoardFrame", center, size + new Vector2(0.6f, 0.5f), new Color(0.08f, 0.11f, 0.15f, 0.98f), -29);
        CreateWorldVisual(_root.transform, "BoardSurface", center, size, new Color(0.13f, 0.18f, 0.18f, 0.96f), -28);
        CreateWorldVisual(_root.transform, "BoardInset", center, size - new Vector2(0.45f, 0.40f), new Color(0.10f, 0.13f, 0.16f, 0.72f), -27);
        CreateWorldVisual(_root.transform, "BoardBandTop", center + new Vector2(0f, size.y * 0.28f), new Vector2(size.x - 1.4f, 0.34f), new Color(0.28f, 0.22f, 0.16f, 0.20f), -26);
        CreateWorldVisual(_root.transform, "BoardBandBottom", center + new Vector2(0f, -size.y * 0.28f), new Vector2(size.x - 1.8f, 0.24f), new Color(0.18f, 0.28f, 0.30f, 0.16f), -26);

        for (int i = 0; i < 4; i++)
        {
            float y = bounds.yMin + 1.25f + (i * (size.y - 2.5f) / 3f);
            CreateWorldVisual(_root.transform, "BoardGuide_" + i, new Vector2(center.x, y), new Vector2(size.x - 1.6f, 0.03f), new Color(0.88f, 0.92f, 0.90f, 0.08f), -25);
        }

        CreateWorldVisual(_root.transform, "BoardPin_TL", new Vector2(bounds.xMin + 0.5f, bounds.yMax - 0.5f), new Vector2(0.22f, 0.22f), new Color(0.86f, 0.72f, 0.34f, 0.92f), -24, 45f);
        CreateWorldVisual(_root.transform, "BoardPin_TR", new Vector2(bounds.xMax - 0.5f, bounds.yMax - 0.5f), new Vector2(0.22f, 0.22f), new Color(0.36f, 0.74f, 0.88f, 0.92f), -24, 45f);
        CreateWorldVisual(_root.transform, "BoardPin_BL", new Vector2(bounds.xMin + 0.5f, bounds.yMin + 0.5f), new Vector2(0.22f, 0.22f), new Color(0.82f, 0.42f, 0.50f, 0.92f), -24, 45f);
        CreateWorldVisual(_root.transform, "BoardPin_BR", new Vector2(bounds.xMax - 0.5f, bounds.yMin + 0.5f), new Vector2(0.22f, 0.22f), new Color(0.46f, 0.78f, 0.46f, 0.92f), -24, 45f);
    }

    private GameObject CreateWorldVisual(Transform parent, string name, Vector2 localPosition, Vector2 localScale, Color color, int sortingOrder, float rotationDegrees = 0f)
    {
        GameObject visual = new GameObject(name);
        visual.transform.SetParent(parent, false);
        visual.transform.localPosition = new Vector3(localPosition.x, localPosition.y, 0f);
        visual.transform.localRotation = Quaternion.Euler(0f, 0f, rotationDegrees);
        visual.transform.localScale = new Vector3(localScale.x, localScale.y, 1f);

        SpriteRenderer renderer = visual.AddComponent<SpriteRenderer>();
        renderer.sprite = _sprite;
        renderer.color = color;
        renderer.sortingOrder = sortingOrder;
        return visual;
    }

    private TextMesh CreateWorldText(Transform parent, string name, Vector2 localPosition, string text, int fontSize, float characterSize, Color color, int sortingOrder)
    {
        GameObject labelRoot = new GameObject(name);
        labelRoot.transform.SetParent(parent, false);
        labelRoot.transform.localPosition = new Vector3(localPosition.x, localPosition.y, 0f);

        TextMesh textMesh = labelRoot.AddComponent<TextMesh>();
        textMesh.anchor = TextAnchor.MiddleCenter;
        textMesh.alignment = TextAlignment.Center;
        textMesh.characterSize = characterSize;
        textMesh.fontSize = fontSize;
        textMesh.color = color;
        textMesh.text = string.IsNullOrEmpty(text) ? string.Empty : text;

        MeshRenderer meshRenderer = labelRoot.GetComponent<MeshRenderer>();
        if (meshRenderer != null)
        {
            meshRenderer.sortingOrder = sortingOrder;
        }

        return textMesh;
    }

    private string BuildRouteVisualLabel(WorldRouteData route)
    {
        if (route == null || string.IsNullOrEmpty(route.Id))
        {
            return string.Empty;
        }

        return route.Id.StartsWith("road-")
            ? "Route " + route.Id.Substring("road-".Length)
            : route.Id.Replace('-', ' ');
    }

    private string BuildEntitySecondaryText(WorldEntityData entity)
    {
        if (entity == null)
        {
            return string.Empty;
        }

        return entity.Kind == WorldEntityKind.City
            ? "Pop " + entity.PrimaryStatValue
            : "Danger " + entity.PrimaryStatValue;
    }

    private Color GetEntitySurfaceColor(WorldEntityData entity)
    {
        if (entity == null)
        {
            return new Color(0.12f, 0.14f, 0.18f, 0.96f);
        }

        if (entity.Kind == WorldEntityKind.Dungeon)
        {
            return entity.Id == "dungeon-beta"
                ? new Color(0.10f, 0.18f, 0.14f, 0.98f)
                : new Color(0.18f, 0.11f, 0.14f, 0.98f);
        }

        return entity.Id == "city-b"
            ? new Color(0.20f, 0.16f, 0.11f, 0.98f)
            : new Color(0.11f, 0.16f, 0.21f, 0.98f);
    }

    private Color GetEntityTrimColor(WorldEntityData entity)
    {
        return entity != null && entity.Kind == WorldEntityKind.Dungeon
            ? new Color(0.92f, 0.78f, 0.40f, 0.98f)
            : new Color(0.84f, 0.90f, 0.94f, 0.98f);
    }

    private Color GetEntityGlowColor(WorldEntityData entity)
    {
        Color accent = GetEntityColor(entity);
        return new Color(accent.r, accent.g, accent.b, 0.16f);
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
        marker.transform.localRotation = Quaternion.identity;
        marker.transform.localScale = Vector3.one;

        Color accentColor = GetEntityColor(entity);
        Color surfaceColor = GetEntitySurfaceColor(entity);
        Color trimColor = GetEntityTrimColor(entity);
        Color glowColor = GetEntityGlowColor(entity);
        bool isDungeon = entity.Kind == WorldEntityKind.Dungeon;
        float markerRotation = isDungeon ? 45f : 0f;

        CreateWorldVisual(marker.transform, "Shadow", new Vector2(0.08f, -0.08f), new Vector2(1.56f, 1.16f), new Color(0f, 0f, 0f, 0.42f), 4, markerRotation);
        SpriteRenderer glowRenderer = CreateWorldVisual(marker.transform, "Glow", Vector2.zero, new Vector2(1.76f, 1.34f), glowColor, 5, markerRotation).GetComponent<SpriteRenderer>();
        CreateWorldVisual(marker.transform, "Plate", Vector2.zero, new Vector2(1.38f, 1.02f), surfaceColor, 6, markerRotation);
        SpriteRenderer renderer = CreateWorldVisual(marker.transform, "Core", Vector2.zero, new Vector2(0.92f, 0.92f), accentColor, 7, markerRotation).GetComponent<SpriteRenderer>();
        SpriteRenderer selectionRenderer = CreateWorldVisual(marker.transform, "Selection", Vector2.zero, new Vector2(1.95f, 1.46f), new Color(trimColor.r, trimColor.g, trimColor.b, 0.26f), 9, markerRotation).GetComponent<SpriteRenderer>();
        selectionRenderer.enabled = false;

        if (isDungeon)
        {
            CreateWorldVisual(marker.transform, "RuneSpine", new Vector2(0f, 0.02f), new Vector2(0.18f, 0.58f), new Color(0.16f, 0.12f, 0.18f, 0.96f), 8);
            CreateWorldVisual(marker.transform, "Threshold", new Vector2(0f, -0.30f), new Vector2(0.42f, 0.12f), trimColor, 8);
            CreateWorldVisual(marker.transform, "Shard", new Vector2(0f, 0.54f), new Vector2(0.22f, 0.22f), new Color(0.98f, 0.90f, 0.58f, 0.96f), 8, 45f);
        }
        else
        {
            CreateWorldVisual(marker.transform, "Banner", new Vector2(0f, 0.48f), new Vector2(0.46f, 0.18f), trimColor, 8);
            CreateWorldVisual(marker.transform, "Keep", new Vector2(0f, 0.12f), new Vector2(0.28f, 0.46f), new Color(0.92f, 0.94f, 0.90f, 0.96f), 8);
            CreateWorldVisual(marker.transform, "Gate", new Vector2(0f, -0.20f), new Vector2(0.48f, 0.14f), new Color(0.12f, 0.16f, 0.18f, 0.96f), 8);
        }

        CreateWorldVisual(marker.transform, "LabelPlate", new Vector2(0f, -0.98f), new Vector2(1.86f, 0.34f), new Color(0.06f, 0.08f, 0.10f, 0.82f), 8);
        CreateWorldVisual(marker.transform, "DetailPlate", new Vector2(0f, -1.28f), new Vector2(1.18f, 0.18f), new Color(0.10f, 0.12f, 0.16f, 0.70f), 8);
        TextMesh labelText = CreateWorldText(marker.transform, "LabelText", new Vector2(0f, -0.98f), entity.DisplayName, 48, 0.09f, new Color(0.94f, 0.96f, 0.95f, 1f), 10);
        TextMesh detailText = CreateWorldText(marker.transform, "DetailText", new Vector2(0f, -1.28f), BuildEntitySecondaryText(entity), 32, 0.07f, new Color(0.74f, 0.80f, 0.82f, 0.96f), 10);

        BoxCollider2D collider = marker.AddComponent<BoxCollider2D>();
        collider.size = new Vector2(1.8f, 1.8f);

        WorldSelectableMarker selectableMarker = marker.AddComponent<WorldSelectableMarker>();
        selectableMarker.Initialize(entity, renderer, glowRenderer, selectionRenderer, labelText, detailText);
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
        Vector2 normal = delta.sqrMagnitude > 0.0001f ? new Vector2(-delta.y, delta.x).normalized : Vector2.up;

        CreateWorldVisual(_root.transform, route.Id + "_Shadow", midpoint + new Vector2(0.08f, -0.08f), new Vector2(length, 0.30f), new Color(0f, 0f, 0f, 0.34f), 0, angle);
        CreateWorldVisual(_root.transform, route.Id + "_Base", midpoint, new Vector2(length, 0.22f), new Color(0.26f, 0.24f, 0.22f, 0.96f), 1, angle);
        CreateWorldVisual(_root.transform, route.Id + "_Inner", midpoint, new Vector2(Mathf.Max(0.12f, length - 0.10f), 0.08f), new Color(0.72f, 0.62f, 0.42f, 0.78f), 2, angle);
        CreateWorldVisual(_root.transform, route.Id + "_Seal", midpoint, new Vector2(0.34f, 0.34f), new Color(color.r, color.g, color.b, 0.92f), 3, 45f);
        CreateWorldText(_root.transform, route.Id + "_Label", midpoint + (normal * 0.34f), BuildRouteVisualLabel(route), 34, 0.07f, new Color(0.90f, 0.84f, 0.76f, 0.96f), 4);
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
    private SpriteRenderer _glowRenderer;
    private SpriteRenderer _selectionRenderer;
    private TextMesh _labelText;
    private TextMesh _detailText;
    private Vector3 _baseScale;
    private Color _baseColor;
    private Color _glowColor;
    private Color _labelColor;
    private Color _detailColor;

    public void Initialize(WorldEntityData entityData, SpriteRenderer renderer, SpriteRenderer glowRenderer, SpriteRenderer selectionRenderer, TextMesh labelText, TextMesh detailText)
    {
        EntityData = entityData;
        _renderer = renderer;
        _glowRenderer = glowRenderer;
        _selectionRenderer = selectionRenderer;
        _labelText = labelText;
        _detailText = detailText;
        _baseScale = transform.localScale;
        _baseColor = renderer != null ? renderer.color : Color.white;
        _glowColor = glowRenderer != null ? glowRenderer.color : Color.clear;
        _labelColor = labelText != null ? labelText.color : Color.white;
        _detailColor = detailText != null ? detailText.color : Color.white;

        if (_selectionRenderer != null)
        {
            _selectionRenderer.enabled = false;
        }
    }

    public void SetSelected(bool isSelected)
    {
        transform.localScale = isSelected ? _baseScale * 1.08f : _baseScale;

        if (_renderer != null)
        {
            _renderer.color = isSelected ? Color.Lerp(_baseColor, Color.white, 0.28f) : _baseColor;
        }

        if (_glowRenderer != null)
        {
            _glowRenderer.color = isSelected
                ? new Color(_glowColor.r, _glowColor.g, _glowColor.b, Mathf.Max(0.34f, _glowColor.a + 0.18f))
                : _glowColor;
        }

        if (_selectionRenderer != null)
        {
            _selectionRenderer.enabled = isSelected;
        }

        if (_labelText != null)
        {
            _labelText.color = isSelected ? Color.Lerp(_labelColor, Color.white, 0.25f) : _labelColor;
        }

        if (_detailText != null)
        {
            _detailText.color = isSelected ? Color.Lerp(_detailColor, Color.white, 0.18f) : _detailColor;
        }
    }
}






