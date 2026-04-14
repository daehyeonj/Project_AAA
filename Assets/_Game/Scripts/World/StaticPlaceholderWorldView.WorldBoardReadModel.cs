using System;
using System.Collections.Generic;

public sealed partial class StaticPlaceholderWorldView
{
    public WorldBoardReadModel BuildWorldBoardReadModel()
    {
        WorldBoardReadModel board = new WorldBoardReadModel();
        board.WorldEntityCount = EntityCount;
        board.WorldRouteCount = RouteCount;
        board.AutoTickEnabled = AutoTickEnabled;
        board.AutoTickPaused = AutoTickPaused;
        board.AutoTickCount = AutoTickCount;
        board.WorldDayCount = WorldDayCount;
        board.TradeStepCount = TradeStepCount;
        board.LastDayProducedTotal = LastDayProducedTotal;
        board.LastDayClaimedDungeonOutputsTotal = LastDayClaimedDungeonOutputsTotal;
        board.LastDayTradedTotal = LastDayTradedTotal;
        board.LastDayProcessedTotal = LastDayProcessedTotal;
        board.LastDayConsumedTotal = LastDayConsumedTotal;
        board.LastDayCriticalFulfilledTotal = LastDayCriticalFulfilledTotal;
        board.LastDayCriticalUnmetTotal = LastDayCriticalUnmetTotal;
        board.LastDayNormalFulfilledTotal = LastDayNormalFulfilledTotal;
        board.LastDayNormalUnmetTotal = LastDayNormalUnmetTotal;
        board.LastDayFulfilledTotal = LastDayFulfilledTotal;
        board.LastDayUnmetTotal = LastDayUnmetTotal;
        board.LastDayShortagesTotal = LastDayShortagesTotal;
        board.LastDayProcessingBlockedTotal = LastDayProcessingBlockedTotal;
        board.LastDayProcessingReservedTotal = LastDayProcessingReservedTotal;
        board.LastDayRouteCapacityUsedTotal = _runtimeEconomyState != null ? _runtimeEconomyState.LastDayRouteCapacityUsedTotal : 0;
        board.TotalRouteCapacityPerDay = CalculateTotalRouteCapacityPerDay();
        board.TotalParties = TotalParties;
        board.IdleParties = IdleParties;
        board.ActiveExpeditions = ActiveExpeditions;
        board.AvailableContracts = AvailableContracts;
        board.ExpeditionSuccessCount = ExpeditionSuccessCount;
        board.ExpeditionFailureCount = ExpeditionFailureCount;
        board.ExpeditionLootReturnedTotal = ExpeditionLootReturnedTotal;
        board.CurrentUnclaimedDungeonOutputsTotal = CurrentUnclaimedDungeonOutputsTotal;
        board.Selection = BuildWorldSelectionReadModel();
        board.TradeOpportunities = BuildTradeOpportunityReadModels();
        board.UnmetCityNeeds = BuildUnmetNeedReadModels();
        board.UnclaimedDungeonOutputs = BuildDungeonOutputReadModels();
        board.TradeOpportunityCount = board.TradeOpportunities.Length;
        board.UnmetCityNeedCount = board.UnmetCityNeeds.Length;
        board.UnclaimedDungeonOutputCount = board.UnclaimedDungeonOutputs.Length;
        board.ActiveExpeditionEntries = BuildActiveExpeditionReadModels();
        board.Cities = BuildCityStatusReadModels(board.ActiveExpeditionEntries);
        board.Dungeons = BuildDungeonStatusReadModels(board.ActiveExpeditionEntries);
        board.Roads = BuildRoadStatusReadModels(board.TradeOpportunities);
        board.VisibleCityCount = board.Cities.Length;
        board.VisibleDungeonCount = board.Dungeons.Length;
        board.VisibleRoadCount = board.Roads.Length;
        board.LatestResult = BuildExpeditionResultReadModel(
            _runtimeEconomyState != null ? _runtimeEconomyState.GetLatestExpeditionOutcome() : new ExpeditionOutcome(),
            _runtimeEconomyState != null ? _runtimeEconomyState.GetLatestOutcomeReadback() : new OutcomeReadback());
        HydrateCityDecisionReadModels(board);
        board.Signals = BuildWorldDecisionSignals(board);
        board.RecentDayLogs = BuildRecentLogTexts(isExpeditionLog: false);
        board.RecentExpeditionLogs = BuildRecentLogTexts(isExpeditionLog: true);
        return board;
    }

    private WorldSelectionReadModel BuildWorldSelectionReadModel()
    {
        WorldSelectionReadModel selection = new WorldSelectionReadModel();
        if (_selectedMarker == null || _selectedMarker.EntityData == null)
        {
            return selection;
        }

        WorldEntityData entity = _selectedMarker.EntityData;
        selection.HasSelection = true;
        selection.EntityId = entity.Id ?? string.Empty;
        selection.DisplayName = HasText(entity.DisplayName) ? entity.DisplayName : (HasText(entity.Id) ? entity.Id : "None");
        selection.Kind = entity.Kind;
        selection.LinkedCityId = entity.Kind == WorldEntityKind.Dungeon ? entity.LinkedCityId ?? string.Empty : string.Empty;
        selection.LinkedDungeonId = entity.Kind == WorldEntityKind.City ? GetLinkedDungeonIdForCity(entity.Id) : entity.Id ?? string.Empty;
        return selection;
    }

    private TradeOpportunityReadModel[] BuildTradeOpportunityReadModels()
    {
        if (_runtimeEconomyState == null)
        {
            return Array.Empty<TradeOpportunityReadModel>();
        }

        DirectTradeOpportunityData[] opportunities = CurrentTradeScanResult.Opportunities ?? Array.Empty<DirectTradeOpportunityData>();
        List<TradeOpportunityReadModel> models = new List<TradeOpportunityReadModel>(opportunities.Length);
        for (int i = 0; i < opportunities.Length; i++)
        {
            DirectTradeOpportunityData opportunity = opportunities[i];
            if (opportunity == null)
            {
                continue;
            }

            models.Add(new TradeOpportunityReadModel
            {
                SupplierEntityId = opportunity.SupplierEntityId ?? string.Empty,
                SupplierDisplayName = GetEntityDisplayNameSafe(opportunity.SupplierEntityId),
                ConsumerEntityId = opportunity.ConsumerEntityId ?? string.Empty,
                ConsumerDisplayName = GetEntityDisplayNameSafe(opportunity.ConsumerEntityId),
                ResourceId = opportunity.ResourceId ?? string.Empty,
                RouteId = opportunity.RouteId ?? string.Empty
            });
        }

        return models.ToArray();
    }

    private WorldUnmetNeedReadModel[] BuildUnmetNeedReadModels()
    {
        if (_runtimeEconomyState == null)
        {
            return Array.Empty<WorldUnmetNeedReadModel>();
        }

        UnmetCityNeedData[] unmetNeeds = CurrentTradeScanResult.UnmetCityNeeds ?? Array.Empty<UnmetCityNeedData>();
        List<WorldUnmetNeedReadModel> models = new List<WorldUnmetNeedReadModel>(unmetNeeds.Length);
        for (int i = 0; i < unmetNeeds.Length; i++)
        {
            UnmetCityNeedData unmetNeed = unmetNeeds[i];
            if (unmetNeed == null)
            {
                continue;
            }

            models.Add(new WorldUnmetNeedReadModel
            {
                CityId = unmetNeed.CityEntityId ?? string.Empty,
                CityDisplayName = GetEntityDisplayNameSafe(unmetNeed.CityEntityId),
                ResourceId = unmetNeed.ResourceId ?? string.Empty
            });
        }

        return models.ToArray();
    }

    private DungeonOutputReadModel[] BuildDungeonOutputReadModels()
    {
        if (_worldData == null || _runtimeEconomyState == null)
        {
            return Array.Empty<DungeonOutputReadModel>();
        }

        List<DungeonOutputReadModel> models = new List<DungeonOutputReadModel>();
        WorldEntityData[] entities = _worldData.Entities ?? Array.Empty<WorldEntityData>();
        for (int entityIndex = 0; entityIndex < entities.Length; entityIndex++)
        {
            WorldEntityData dungeon = entities[entityIndex];
            if (dungeon == null || dungeon.Kind != WorldEntityKind.Dungeon)
            {
                continue;
            }

            string[] outputResourceIds = dungeon.OutputResourceIds ?? Array.Empty<string>();
            for (int resourceIndex = 0; resourceIndex < outputResourceIds.Length; resourceIndex++)
            {
                string resourceId = outputResourceIds[resourceIndex];
                if (!HasText(resourceId) || _runtimeEconomyState.GetStockAmount(dungeon.Id, resourceId) < 1)
                {
                    continue;
                }

                models.Add(new DungeonOutputReadModel
                {
                    DungeonId = dungeon.Id ?? string.Empty,
                    DungeonDisplayName = GetEntityDisplayNameSafe(dungeon.Id),
                    LinkedCityId = dungeon.LinkedCityId ?? string.Empty,
                    LinkedCityDisplayName = GetEntityDisplayNameSafe(dungeon.LinkedCityId),
                    ResourceId = resourceId
                });
            }
        }

        return models.ToArray();
    }

    private ExpeditionStatusReadModel[] BuildActiveExpeditionReadModels()
    {
        if (_worldData == null || _runtimeEconomyState == null)
        {
            return Array.Empty<ExpeditionStatusReadModel>();
        }

        List<ExpeditionStatusReadModel> models = new List<ExpeditionStatusReadModel>();
        WorldEntityData[] entities = _worldData.Entities ?? Array.Empty<WorldEntityData>();
        for (int i = 0; i < entities.Length; i++)
        {
            WorldEntityData city = entities[i];
            if (city == null || city.Kind != WorldEntityKind.City || _runtimeEconomyState.GetActiveExpeditionCountFromCity(city.Id) < 1)
            {
                continue;
            }

            string dungeonId = _runtimeEconomyState.GetActiveDungeonIdForCity(city.Id);
            models.Add(new ExpeditionStatusReadModel
            {
                PartyId = _runtimeEconomyState.GetActivePartyIdForCity(city.Id),
                HomeCityId = city.Id ?? string.Empty,
                HomeCityDisplayName = GetEntityDisplayNameSafe(city.Id),
                TargetDungeonId = dungeonId,
                TargetDungeonDisplayName = GetEntityDisplayNameSafe(dungeonId),
                DaysRemaining = _runtimeEconomyState.GetActiveExpeditionDaysRemainingForCity(city.Id),
                Power = _runtimeEconomyState.GetActivePartyPowerForCity(city.Id),
                CarryCapacity = _runtimeEconomyState.GetActivePartyCarryCapacityForCity(city.Id),
                StatusText = _runtimeEconomyState.GetExpeditionStatusTextForCity(city.Id)
            });
        }

        models.Sort((left, right) =>
        {
            int dayCompare = left.DaysRemaining.CompareTo(right.DaysRemaining);
            return dayCompare != 0 ? dayCompare : string.CompareOrdinal(left.HomeCityId, right.HomeCityId);
        });
        return models.ToArray();
    }

    private CityStatusReadModel[] BuildCityStatusReadModels(ExpeditionStatusReadModel[] activeExpeditions)
    {
        if (_worldData == null || _runtimeEconomyState == null)
        {
            return Array.Empty<CityStatusReadModel>();
        }

        List<CityStatusReadModel> models = new List<CityStatusReadModel>();
        WorldEntityData[] entities = _worldData.Entities ?? Array.Empty<WorldEntityData>();
        for (int i = 0; i < entities.Length; i++)
        {
            WorldEntityData city = entities[i];
            if (city == null || city.Kind != WorldEntityKind.City)
            {
                continue;
            }

            string linkedDungeonId = GetLinkedDungeonIdForCity(city.Id);
            CityStatusReadModel model = new CityStatusReadModel();
            model.CityId = city.Id ?? string.Empty;
            model.DisplayName = HasText(city.DisplayName) ? city.DisplayName : (HasText(city.Id) ? city.Id : "None");
            model.PopulationValue = city.PrimaryStatValue;
            model.RelatedResourceIds = CloneStringArray(city.RelatedResourceIds);
            model.SupplyResourceIds = CloneStringArray(city.SupplyResourceIds);
            model.NeedResourceIds = CloneStringArray(city.NeedResourceIds);
            model.HighPriorityNeedResourceIds = CloneStringArray(city.HighPriorityNeedResourceIds);
            model.OutputResourceIds = CollectCityOutputResourceIds(city);
            model.KeyStocks = BuildKeyStockReadModels(city);
            model.TopShortages = BuildTopShortageReadModels(city);
            model.TopSurpluses = BuildTopSurplusReadModels(city);
            model.LastDayProduced = _runtimeEconomyState.GetLastDayProduced(city.Id);
            model.LastDayImported = _runtimeEconomyState.GetLastDayImported(city.Id);
            model.LastDayExported = _runtimeEconomyState.GetLastDayExported(city.Id);
            model.LastDayProcessedTotal = _runtimeEconomyState.GetLastDayProcessedTotal(city.Id);
            model.LastDayConsumed = _runtimeEconomyState.GetLastDayConsumed(city.Id);
            model.LastDayShortages = _runtimeEconomyState.GetLastDayShortages(city.Id);
            model.LastDayUnmet = _runtimeEconomyState.GetLastDayUnmet(city.Id);
            model.LastDayCriticalUnmet = _runtimeEconomyState.GetLastDayCriticalUnmet(city.Id);
            model.LastDayProcessingBlocked = _runtimeEconomyState.GetLastDayProcessingBlocked(city.Id);
            model.TotalShortages = _runtimeEconomyState.GetTotalShortages(city.Id);
            model.TotalUnmet = _runtimeEconomyState.GetTotalUnmet(city.Id);
            model.TotalCriticalUnmet = _runtimeEconomyState.GetTotalCriticalUnmet(city.Id);
            model.PartyCount = _runtimeEconomyState.GetPartyCountInCity(city.Id);
            model.IdlePartyCount = _runtimeEconomyState.GetIdlePartyCountInCity(city.Id);
            model.ActiveExpeditionCount = _runtimeEconomyState.GetActiveExpeditionCountFromCity(city.Id);
            model.ExpeditionLootReturnedTotal = _runtimeEconomyState.GetExpeditionLootReturnedTotalForCity(city.Id);
            model.LinkedDungeonId = linkedDungeonId;
            model.LinkedDungeonDisplayName = GetEntityDisplayNameSafe(linkedDungeonId);
            model.MaxActiveExpeditionSlots = HasText(linkedDungeonId) ? _runtimeEconomyState.GetMaxActiveExpeditionsForDungeon(linkedDungeonId) : 0;
            model.AvailableContractSlots = HasText(linkedDungeonId) ? _runtimeEconomyState.GetAvailableContractSlotsForDungeon(linkedDungeonId) : 0;
            model.NeedPressureStateId = BuildNeedPressureText(city.Id);
            model.DispatchReadinessStateId = GetDispatchReadinessText(city.Id);
            model.DispatchPolicyStateId = BuildDispatchPolicyText(GetDispatchPolicyState(city.Id));
            model.DispatchRecoveryDaysRemaining = GetRecoveryDaysToReady(city.Id);
            model.ConsecutiveDispatchCount = GetConsecutiveDispatchCount(city.Id);
            model.RecommendedRouteId = HasText(linkedDungeonId) ? GetRecommendedRouteId(city.Id, linkedDungeonId) : string.Empty;
            model.RecommendedRouteSummaryText = HasText(linkedDungeonId) ? BuildRecommendedRouteSummaryText(city.Id, linkedDungeonId) : "None";
            model.RecommendationReasonText = HasText(linkedDungeonId) ? BuildRecommendationReasonText(city.Id, linkedDungeonId) : "None";
            model.LastDispatchImpactText = GetLastDispatchImpactText(city.Id);
            model.LastNeedPressureChangeText = GetLastNeedPressureChangeText(city.Id);
            model.LastDispatchReadinessChangeText = GetLastDispatchReadinessChangeText(city.Id);
            model.ActiveExpedition = FindActiveExpeditionForCity(activeExpeditions, city.Id);
            model.LatestResult = BuildExpeditionResultReadModel(
                _runtimeEconomyState.GetLatestExpeditionOutcomeForCity(city.Id),
                _runtimeEconomyState.GetLatestOutcomeReadbackForCity(city.Id));
            model.ActionSignals = BuildCityActionSignals(model);
            models.Add(model);
        }

        models.Sort((left, right) =>
        {
            int shortageCompare = right.LastDayShortages.CompareTo(left.LastDayShortages);
            return shortageCompare != 0 ? shortageCompare : string.CompareOrdinal(left.CityId, right.CityId);
        });
        return models.ToArray();
    }

    private void HydrateCityDecisionReadModels(WorldBoardReadModel board)
    {
        CityStatusReadModel[] cities = board != null ? board.Cities : Array.Empty<CityStatusReadModel>();
        for (int i = 0; i < cities.Length; i++)
        {
            CityStatusReadModel city = cities[i];
            if (city == null)
            {
                continue;
            }

            city.Decision = CityDecisionModelBuilder.Build(board, city);
        }
    }

    private DungeonStatusReadModel[] BuildDungeonStatusReadModels(ExpeditionStatusReadModel[] activeExpeditions)
    {
        if (_worldData == null || _runtimeEconomyState == null)
        {
            return Array.Empty<DungeonStatusReadModel>();
        }

        List<DungeonStatusReadModel> models = new List<DungeonStatusReadModel>();
        WorldEntityData[] entities = _worldData.Entities ?? Array.Empty<WorldEntityData>();
        for (int i = 0; i < entities.Length; i++)
        {
            WorldEntityData dungeon = entities[i];
            if (dungeon == null || dungeon.Kind != WorldEntityKind.Dungeon)
            {
                continue;
            }

            DungeonStatusReadModel model = new DungeonStatusReadModel();
            model.DungeonId = dungeon.Id ?? string.Empty;
            model.DisplayName = HasText(dungeon.DisplayName) ? dungeon.DisplayName : (HasText(dungeon.Id) ? dungeon.Id : "None");
            model.LinkedCityId = dungeon.LinkedCityId ?? string.Empty;
            model.LinkedCityDisplayName = GetEntityDisplayNameSafe(dungeon.LinkedCityId);
            model.DangerLevel = dungeon.PrimaryStatValue;
            model.OutputResourceIds = CloneStringArray(dungeon.OutputResourceIds);
            model.RecommendedPower = _runtimeEconomyState.GetRecommendedPower(dungeon.Id);
            model.ExpeditionDurationDays = _runtimeEconomyState.GetExpeditionDurationDays(dungeon.Id);
            model.ActiveExpeditionCount = _runtimeEconomyState.GetActiveExpeditionCountForDungeon(dungeon.Id);
            model.MaxActiveExpeditionSlots = _runtimeEconomyState.GetMaxActiveExpeditionsForDungeon(dungeon.Id);
            model.AvailableContractSlots = _runtimeEconomyState.GetAvailableContractSlotsForDungeon(dungeon.Id);
            model.ActiveExpedition = FindActiveExpeditionForDungeon(activeExpeditions, dungeon.Id);
            model.LatestResult = BuildDungeonLatestResultReadModel(dungeon);
            models.Add(model);
        }

        models.Sort((left, right) =>
        {
            int dangerCompare = right.DangerLevel.CompareTo(left.DangerLevel);
            return dangerCompare != 0 ? dangerCompare : string.CompareOrdinal(left.DungeonId, right.DungeonId);
        });
        return models.ToArray();
    }

    private RoadStatusReadModel[] BuildRoadStatusReadModels(TradeOpportunityReadModel[] tradeOpportunities)
    {
        if (_worldData == null || _runtimeEconomyState == null)
        {
            return Array.Empty<RoadStatusReadModel>();
        }

        List<RoadStatusReadModel> models = new List<RoadStatusReadModel>();
        WorldRouteData[] routes = _worldData.Routes ?? Array.Empty<WorldRouteData>();
        for (int i = 0; i < routes.Length; i++)
        {
            WorldRouteData route = routes[i];
            if (route == null)
            {
                continue;
            }

            int capacity = route.CapacityPerDay;
            int usage = _runtimeEconomyState.GetLastDayRouteUsage(route.Id);
            RoadStatusReadModel model = new RoadStatusReadModel();
            model.RoadId = route.Id ?? string.Empty;
            model.Tags = CloneStringArray(route.Tags);
            model.FromEntityId = route.FromEntityId ?? string.Empty;
            model.FromEntityDisplayName = GetEntityDisplayNameSafe(route.FromEntityId);
            model.ToEntityId = route.ToEntityId ?? string.Empty;
            model.ToEntityDisplayName = GetEntityDisplayNameSafe(route.ToEntityId);
            model.CapacityPerDay = capacity;
            model.LastDayUsage = usage;
            model.AvailableCapacity = capacity > usage ? capacity - usage : 0;
            model.UtilizationPercent = capacity > 0 ? (usage * 100) / capacity : 0;
            model.IsSaturated = capacity > 0 && usage >= capacity;
            model.TradeOpportunityCount = CountTradeOpportunitiesForRoute(tradeOpportunities, route.Id);
            models.Add(model);
        }

        models.Sort((left, right) =>
        {
            int saturationCompare = right.IsSaturated.CompareTo(left.IsSaturated);
            if (saturationCompare != 0)
            {
                return saturationCompare;
            }

            int utilizationCompare = right.UtilizationPercent.CompareTo(left.UtilizationPercent);
            return utilizationCompare != 0 ? utilizationCompare : string.CompareOrdinal(left.RoadId, right.RoadId);
        });
        return models.ToArray();
    }

    private WorldDecisionSignalReadModel[] BuildCityActionSignals(CityStatusReadModel city)
    {
        if (city == null)
        {
            return Array.Empty<WorldDecisionSignalReadModel>();
        }

        List<WorldDecisionSignalReadModel> signals = new List<WorldDecisionSignalReadModel>();
        if (city.TopShortages.Length > 0 && city.TopShortages[0] != null && city.TopShortages[0].Amount > 0)
        {
            ResourceAmountReadModel topShortage = city.TopShortages[0];
            signals.Add(new WorldDecisionSignalReadModel
            {
                Kind = WorldDecisionSignalKind.CityShortage,
                EntityId = city.CityId,
                EntityDisplayName = city.DisplayName,
                ResourceId = topShortage.ResourceId,
                Magnitude = topShortage.Amount,
                Priority = 300 + topShortage.Amount + (topShortage.IsHighPriority ? 50 : 0) + city.LastDayCriticalUnmet,
                IsBlocking = true
            });
        }

        if (city.LastDayProcessingBlocked > 0)
        {
            signals.Add(new WorldDecisionSignalReadModel
            {
                Kind = WorldDecisionSignalKind.CityShortage,
                EntityId = city.CityId,
                EntityDisplayName = city.DisplayName,
                Magnitude = city.LastDayProcessingBlocked,
                Priority = 260 + city.LastDayProcessingBlocked,
                IsBlocking = true
            });
        }

        if (city.TopSurpluses.Length > 0 && city.TopSurpluses[0] != null && city.TopSurpluses[0].Amount > 0)
        {
            ResourceAmountReadModel topSurplus = city.TopSurpluses[0];
            signals.Add(new WorldDecisionSignalReadModel
            {
                Kind = WorldDecisionSignalKind.CitySurplus,
                EntityId = city.CityId,
                EntityDisplayName = city.DisplayName,
                ResourceId = topSurplus.ResourceId,
                Magnitude = topSurplus.Amount,
                Priority = 80 + topSurplus.Amount,
                IsBlocking = false
            });
        }

        if (city.ActiveExpedition != null && HasText(city.ActiveExpedition.TargetDungeonId))
        {
            signals.Add(new WorldDecisionSignalReadModel
            {
                Kind = WorldDecisionSignalKind.ActiveExpedition,
                EntityId = city.CityId,
                EntityDisplayName = city.DisplayName,
                RelatedEntityId = city.ActiveExpedition.TargetDungeonId,
                RelatedEntityDisplayName = city.ActiveExpedition.TargetDungeonDisplayName,
                Magnitude = city.ActiveExpedition.DaysRemaining,
                Priority = 140 + (city.ActiveExpedition.DaysRemaining > 0 ? 10 - city.ActiveExpedition.DaysRemaining : 10),
                IsBlocking = false
            });
        }

        if (city.LatestResult != null && city.LatestResult.HasResult)
        {
            signals.Add(new WorldDecisionSignalReadModel
            {
                Kind = WorldDecisionSignalKind.RecentResult,
                EntityId = city.CityId,
                EntityDisplayName = city.DisplayName,
                RelatedEntityId = city.LatestResult.TargetDungeonId,
                RelatedEntityDisplayName = city.LatestResult.TargetDungeonDisplayName,
                ResourceId = city.LatestResult.RewardResourceId,
                ResultStateKey = city.LatestResult.ResultStateKey,
                Magnitude = city.LatestResult.ReturnedLootAmount,
                Priority = 120 + city.LatestResult.ReturnedLootAmount,
                IsBlocking = false
            });
        }

        signals.Sort((left, right) =>
        {
            int priorityCompare = right.Priority.CompareTo(left.Priority);
            return priorityCompare != 0 ? priorityCompare : string.CompareOrdinal(left.EntityId, right.EntityId);
        });

        if (signals.Count > 3)
        {
            signals.RemoveRange(3, signals.Count - 3);
        }

        return signals.ToArray();
    }

    private WorldDecisionSignalReadModel[] BuildWorldDecisionSignals(WorldBoardReadModel board)
    {
        if (board == null)
        {
            return Array.Empty<WorldDecisionSignalReadModel>();
        }

        List<WorldDecisionSignalReadModel> signals = new List<WorldDecisionSignalReadModel>();

        CityStatusReadModel[] cities = board.Cities ?? Array.Empty<CityStatusReadModel>();
        for (int i = 0; i < cities.Length; i++)
        {
            CityStatusReadModel city = cities[i];
            WorldDecisionSignalReadModel[] citySignals = city != null ? city.ActionSignals : Array.Empty<WorldDecisionSignalReadModel>();
            for (int signalIndex = 0; signalIndex < citySignals.Length; signalIndex++)
            {
                if (citySignals[signalIndex] != null)
                {
                    signals.Add(citySignals[signalIndex]);
                }
            }
        }

        RoadStatusReadModel[] roads = board.Roads ?? Array.Empty<RoadStatusReadModel>();
        for (int i = 0; i < roads.Length; i++)
        {
            RoadStatusReadModel road = roads[i];
            if (road == null || road.CapacityPerDay < 1)
            {
                continue;
            }

            if (!road.IsSaturated && road.UtilizationPercent < 75)
            {
                continue;
            }

            signals.Add(new WorldDecisionSignalReadModel
            {
                Kind = WorldDecisionSignalKind.RoadCapacity,
                EntityId = road.RoadId,
                EntityDisplayName = road.FromEntityDisplayName + " <-> " + road.ToEntityDisplayName,
                Magnitude = road.UtilizationPercent,
                Priority = 200 + road.UtilizationPercent + (road.IsSaturated ? 40 : 0),
                IsBlocking = road.IsSaturated
            });
        }

        DungeonOutputReadModel[] dungeonOutputs = board.UnclaimedDungeonOutputs ?? Array.Empty<DungeonOutputReadModel>();
        for (int i = 0; i < dungeonOutputs.Length; i++)
        {
            DungeonOutputReadModel dungeonOutput = dungeonOutputs[i];
            if (dungeonOutput == null)
            {
                continue;
            }

            signals.Add(new WorldDecisionSignalReadModel
            {
                Kind = WorldDecisionSignalKind.DungeonOutput,
                EntityId = dungeonOutput.DungeonId,
                EntityDisplayName = dungeonOutput.DungeonDisplayName,
                RelatedEntityId = dungeonOutput.LinkedCityId,
                RelatedEntityDisplayName = dungeonOutput.LinkedCityDisplayName,
                ResourceId = dungeonOutput.ResourceId,
                Magnitude = 1,
                Priority = 60,
                IsBlocking = false
            });
        }

        if (board.LatestResult != null && board.LatestResult.HasResult)
        {
            signals.Add(new WorldDecisionSignalReadModel
            {
                Kind = WorldDecisionSignalKind.RecentResult,
                EntityId = board.LatestResult.SourceCityId,
                EntityDisplayName = board.LatestResult.SourceCityDisplayName,
                RelatedEntityId = board.LatestResult.TargetDungeonId,
                RelatedEntityDisplayName = board.LatestResult.TargetDungeonDisplayName,
                ResourceId = board.LatestResult.RewardResourceId,
                ResultStateKey = board.LatestResult.ResultStateKey,
                Magnitude = board.LatestResult.ReturnedLootAmount,
                Priority = 150 + board.LatestResult.ReturnedLootAmount,
                IsBlocking = false
            });
        }

        signals.Sort((left, right) =>
        {
            int priorityCompare = right.Priority.CompareTo(left.Priority);
            if (priorityCompare != 0)
            {
                return priorityCompare;
            }

            int blockingCompare = right.IsBlocking.CompareTo(left.IsBlocking);
            return blockingCompare != 0 ? blockingCompare : string.CompareOrdinal(left.EntityId, right.EntityId);
        });

        if (signals.Count > 8)
        {
            signals.RemoveRange(8, signals.Count - 8);
        }

        return signals.ToArray();
    }

    private ExpeditionResultReadModel BuildDungeonLatestResultReadModel(WorldEntityData dungeon)
    {
        if (dungeon == null || _runtimeEconomyState == null || !HasText(dungeon.LinkedCityId))
        {
            return new ExpeditionResultReadModel();
        }

        ExpeditionResultReadModel result = BuildExpeditionResultReadModel(
            _runtimeEconomyState.GetLatestExpeditionOutcomeForCity(dungeon.LinkedCityId),
            _runtimeEconomyState.GetLatestOutcomeReadbackForCity(dungeon.LinkedCityId));
        return result.HasResult && result.TargetDungeonId == dungeon.Id ? result : new ExpeditionResultReadModel();
    }

    private ExpeditionResultReadModel BuildExpeditionResultReadModel(ExpeditionOutcome expeditionOutcome, OutcomeReadback outcomeReadback)
    {
        ExpeditionOutcome safeOutcome = CopyExpeditionOutcome(expeditionOutcome);
        OutcomeReadback safeReadback = CopyOutcomeReadback(outcomeReadback);
        ExpeditionResultReadModel result = new ExpeditionResultReadModel();
        result.SourceCityId = HasText(safeOutcome.SourceCityId) ? safeOutcome.SourceCityId : safeReadback.SourceCityId;
        result.SourceCityDisplayName = HasText(safeOutcome.SourceCityLabel) ? safeOutcome.SourceCityLabel : safeReadback.SourceCityLabel;
        result.TargetDungeonId = HasText(safeOutcome.TargetDungeonId) ? safeOutcome.TargetDungeonId : safeReadback.TargetDungeonId;
        result.TargetDungeonDisplayName = HasText(safeOutcome.TargetDungeonLabel) ? safeOutcome.TargetDungeonLabel : safeReadback.TargetDungeonLabel;
        result.RewardResourceId = safeOutcome.RewardResourceId;
        result.ResultStateKey = HasText(safeOutcome.ResultStateKey) ? safeOutcome.ResultStateKey : safeReadback.ResultStateKey;
        result.Success = safeOutcome.Success || safeReadback.Success;
        result.ReturnedLootAmount = safeOutcome.ReturnedLootAmount;
        result.TotalTurnsTaken = safeOutcome.TotalTurnsTaken;
        result.ClearedEncounterCount = safeOutcome.ClearedEncounterCount;
        result.OpenedChestCount = safeOutcome.OpenedChestCount;
        result.SurvivingMemberCount = safeOutcome.SurvivingMemberCount;
        result.KnockedOutMemberCount = safeOutcome.KnockedOutMemberCount;
        result.EliteDefeated = safeOutcome.EliteDefeated;
        result.SummaryText = HasText(safeReadback.SummaryText) ? safeReadback.SummaryText : safeOutcome.ResultSummaryText;
        result.LootSummaryText = HasText(safeReadback.LootSummaryText) ? safeReadback.LootSummaryText : safeOutcome.LootSummaryText;
        result.SurvivingMembersSummaryText = HasText(safeReadback.SurvivingMembersSummaryText) ? safeReadback.SurvivingMembersSummaryText : safeOutcome.SurvivingMembersSummaryText;
        result.ClearedEncountersSummaryText = HasText(safeReadback.ClearedEncountersSummaryText) ? safeReadback.ClearedEncountersSummaryText : safeOutcome.ClearedEncountersSummaryText;
        result.EventChoiceSummaryText = HasText(safeReadback.EventChoiceSummaryText) ? safeReadback.EventChoiceSummaryText : safeOutcome.EventChoiceSummaryText;
        result.LootBreakdownSummaryText = HasText(safeReadback.LootBreakdownSummaryText) ? safeReadback.LootBreakdownSummaryText : safeOutcome.LootBreakdownSummaryText;
        result.RouteSummaryText = HasText(safeReadback.RouteSummaryText) ? safeReadback.RouteSummaryText : safeOutcome.RouteSummaryText;
        result.DungeonSummaryText = HasText(safeReadback.DungeonSummaryText) ? safeReadback.DungeonSummaryText : safeOutcome.DungeonSummaryText;
        result.MissionObjectiveText = HasText(safeReadback.MissionObjectiveText) ? safeReadback.MissionObjectiveText : safeOutcome.MissionObjectiveText;
        result.MissionRelevanceText = HasText(safeReadback.MissionRelevanceText) ? safeReadback.MissionRelevanceText : safeOutcome.MissionRelevanceText;
        result.RiskRewardContextText = HasText(safeReadback.RiskRewardContextText) ? safeReadback.RiskRewardContextText : safeOutcome.RiskRewardContextText;
        result.RunPathSummaryText = HasText(safeReadback.RunPathSummaryText) ? safeReadback.RunPathSummaryText : safeOutcome.RunPathSummaryText;
        result.OutcomeMeaningId = HasText(safeReadback.OutcomeMeaningId) ? safeReadback.OutcomeMeaningId : safeOutcome.OutcomeMeaningId;
        result.OutcomeRewardMeaningText = HasText(safeReadback.OutcomeRewardMeaningText) ? safeReadback.OutcomeRewardMeaningText : safeOutcome.OutcomeRewardMeaningText;
        result.CityImpactMeaningText = HasText(safeReadback.CityImpactMeaningText) ? safeReadback.CityImpactMeaningText : safeOutcome.CityImpactMeaningText;
        result.RecommendationShiftText = HasText(safeReadback.RecommendationShiftText) ? safeReadback.RecommendationShiftText : safeOutcome.RecommendationShiftText;
        result.CityStatusChangeSummaryText = safeReadback.CityStatusChangeSummaryText;
        result.WorldReturnSummaryText = BuildWorldReturnSummaryText(result);
        result.ExpeditionLogEntryText = safeReadback.ExpeditionLogEntryText;
        result.PartyConditionText = HasText(safeReadback.PartyConditionText) ? safeReadback.PartyConditionText : safeOutcome.PartyConditionText;
        result.PartyHpSummaryText = HasText(safeReadback.PartyHpSummaryText) ? safeReadback.PartyHpSummaryText : safeOutcome.PartyHpSummaryText;
        result.EliteSummaryText = HasText(safeReadback.EliteSummaryText) ? safeReadback.EliteSummaryText : safeOutcome.EliteSummaryText;
        result.HasResult = HasText(result.SourceCityId) || HasText(result.SummaryText) || HasText(result.ExpeditionLogEntryText);
        if (!HasText(result.SourceCityDisplayName))
        {
            result.SourceCityDisplayName = GetEntityDisplayNameSafe(result.SourceCityId);
        }

        if (!HasText(result.TargetDungeonDisplayName))
        {
            result.TargetDungeonDisplayName = GetEntityDisplayNameSafe(result.TargetDungeonId);
        }

        return result;
    }

    private string BuildWorldReturnSummaryText(ExpeditionResultReadModel result)
    {
        if (result == null)
        {
            return "None";
        }

        string impactText = HasText(result.CityStatusChangeSummaryText)
            ? result.CityStatusChangeSummaryText
            : HasText(result.SummaryText)
                ? result.SummaryText
                : string.Empty;
        string meaningText = HasText(result.CityImpactMeaningText)
            ? result.CityImpactMeaningText
            : HasText(result.MissionRelevanceText)
            ? result.MissionRelevanceText
            : HasText(result.MissionObjectiveText)
                ? result.MissionObjectiveText
                : string.Empty;

        if (HasText(impactText) && HasText(meaningText) && impactText != meaningText)
        {
            return impactText + " | " + meaningText;
        }

        if (HasText(impactText))
        {
            return impactText;
        }

        if (HasText(meaningText))
        {
            return meaningText;
        }

        return HasText(result.ResultStateKey) ? result.ResultStateKey : "None";
    }

    private ResourceAmountReadModel[] BuildKeyStockReadModels(WorldEntityData city)
    {
        if (city == null || _runtimeEconomyState == null)
        {
            return Array.Empty<ResourceAmountReadModel>();
        }

        string[] resourceIds = city.RelatedResourceIds ?? Array.Empty<string>();
        List<ResourceAmountReadModel> models = new List<ResourceAmountReadModel>(resourceIds.Length);
        for (int i = 0; i < resourceIds.Length; i++)
        {
            string resourceId = resourceIds[i];
            if (!HasText(resourceId))
            {
                continue;
            }

            int stockAmount = _runtimeEconomyState.GetStockAmount(city.Id, resourceId);
            models.Add(new ResourceAmountReadModel
            {
                ResourceId = resourceId,
                Amount = stockAmount,
                StockAmount = stockAmount,
                IsHighPriority = ContainsResourceId(city.HighPriorityNeedResourceIds, resourceId)
            });
        }

        models.Sort((left, right) =>
        {
            int amountCompare = right.Amount.CompareTo(left.Amount);
            return amountCompare != 0 ? amountCompare : string.CompareOrdinal(left.ResourceId, right.ResourceId);
        });
        return models.ToArray();
    }

    private ResourceAmountReadModel[] BuildTopShortageReadModels(WorldEntityData city)
    {
        if (city == null || _runtimeEconomyState == null)
        {
            return Array.Empty<ResourceAmountReadModel>();
        }

        List<string> demandResourceIds = CollectCityDemandResourceIds(city);
        List<ResourceAmountReadModel> models = new List<ResourceAmountReadModel>(demandResourceIds.Count);
        for (int i = 0; i < demandResourceIds.Count; i++)
        {
            string resourceId = demandResourceIds[i];
            int shortageAmount = _runtimeEconomyState.GetLastDayShortageCount(city.Id, resourceId);
            int unmetAmount = _runtimeEconomyState.GetLastDayUnmetCount(city.Id, resourceId);
            int severity = shortageAmount + unmetAmount;
            if (severity < 1)
            {
                continue;
            }

            models.Add(new ResourceAmountReadModel
            {
                ResourceId = resourceId,
                Amount = severity,
                StockAmount = _runtimeEconomyState.GetStockAmount(city.Id, resourceId),
                IsHighPriority = ContainsResourceId(city.HighPriorityNeedResourceIds, resourceId)
            });
        }

        models.Sort((left, right) =>
        {
            int priorityCompare = right.IsHighPriority.CompareTo(left.IsHighPriority);
            if (priorityCompare != 0)
            {
                return priorityCompare;
            }

            int amountCompare = right.Amount.CompareTo(left.Amount);
            return amountCompare != 0 ? amountCompare : string.CompareOrdinal(left.ResourceId, right.ResourceId);
        });

        if (models.Count > 3)
        {
            models.RemoveRange(3, models.Count - 3);
        }

        return models.ToArray();
    }

    private ResourceAmountReadModel[] BuildTopSurplusReadModels(WorldEntityData city)
    {
        if (city == null || _runtimeEconomyState == null)
        {
            return Array.Empty<ResourceAmountReadModel>();
        }

        List<string> resourceIds = CollectCitySupplyResourceIds(city);
        List<ResourceAmountReadModel> models = new List<ResourceAmountReadModel>(resourceIds.Count);
        for (int i = 0; i < resourceIds.Count; i++)
        {
            string resourceId = resourceIds[i];
            int exportableAmount = _runtimeEconomyState.GetExportableAmount(city.Id, resourceId);
            if (exportableAmount < 1)
            {
                continue;
            }

            models.Add(new ResourceAmountReadModel
            {
                ResourceId = resourceId,
                Amount = exportableAmount,
                StockAmount = _runtimeEconomyState.GetStockAmount(city.Id, resourceId)
            });
        }

        models.Sort((left, right) =>
        {
            int amountCompare = right.Amount.CompareTo(left.Amount);
            return amountCompare != 0 ? amountCompare : string.CompareOrdinal(left.ResourceId, right.ResourceId);
        });

        if (models.Count > 3)
        {
            models.RemoveRange(3, models.Count - 3);
        }

        return models.ToArray();
    }

    private ExpeditionStatusReadModel FindActiveExpeditionForCity(ExpeditionStatusReadModel[] activeExpeditions, string cityId)
    {
        if (activeExpeditions == null || !HasText(cityId))
        {
            return null;
        }

        for (int i = 0; i < activeExpeditions.Length; i++)
        {
            ExpeditionStatusReadModel expedition = activeExpeditions[i];
            if (expedition != null && expedition.HomeCityId == cityId)
            {
                return expedition;
            }
        }

        return null;
    }

    private ExpeditionStatusReadModel FindActiveExpeditionForDungeon(ExpeditionStatusReadModel[] activeExpeditions, string dungeonId)
    {
        if (activeExpeditions == null || !HasText(dungeonId))
        {
            return null;
        }

        for (int i = 0; i < activeExpeditions.Length; i++)
        {
            ExpeditionStatusReadModel expedition = activeExpeditions[i];
            if (expedition != null && expedition.TargetDungeonId == dungeonId)
            {
                return expedition;
            }
        }

        return null;
    }

    private int CalculateTotalRouteCapacityPerDay()
    {
        if (_worldData == null || _worldData.Routes == null)
        {
            return 0;
        }

        int totalCapacity = 0;
        for (int i = 0; i < _worldData.Routes.Length; i++)
        {
            WorldRouteData route = _worldData.Routes[i];
            if (route != null)
            {
                totalCapacity += route.CapacityPerDay;
            }
        }

        return totalCapacity;
    }

    private int CountTradeOpportunitiesForRoute(TradeOpportunityReadModel[] opportunities, string routeId)
    {
        if (opportunities == null || !HasText(routeId))
        {
            return 0;
        }

        int count = 0;
        for (int i = 0; i < opportunities.Length; i++)
        {
            TradeOpportunityReadModel opportunity = opportunities[i];
            if (opportunity != null && opportunity.RouteId == routeId)
            {
                count += 1;
            }
        }

        return count;
    }

    private string[] BuildRecentLogTexts(bool isExpeditionLog)
    {
        if (_runtimeEconomyState == null)
        {
            return Array.Empty<string>();
        }

        string[] logs = new string[3];
        for (int i = 0; i < logs.Length; i++)
        {
            logs[i] = isExpeditionLog
                ? _runtimeEconomyState.GetRecentExpeditionLogText(i)
                : _runtimeEconomyState.GetRecentDayLogText(i);
        }

        return logs;
    }

    private string[] CollectCityOutputResourceIds(WorldEntityData city)
    {
        if (city == null)
        {
            return Array.Empty<string>();
        }

        List<string> resourceIds = new List<string>();
        AddUniqueResourceIds(resourceIds, city.OutputResourceIds);
        if (city.ProcessingRules != null)
        {
            for (int i = 0; i < city.ProcessingRules.Length; i++)
            {
                LocalProcessingRuleData rule = city.ProcessingRules[i];
                if (rule != null && HasText(rule.OutputResourceId) && !resourceIds.Contains(rule.OutputResourceId))
                {
                    resourceIds.Add(rule.OutputResourceId);
                }
            }
        }

        return resourceIds.ToArray();
    }

    private List<string> CollectCityDemandResourceIds(WorldEntityData city)
    {
        List<string> resourceIds = new List<string>();
        if (city == null)
        {
            return resourceIds;
        }

        AddUniqueResourceIds(resourceIds, city.NeedResourceIds);
        if (city.ProcessingRules != null)
        {
            for (int i = 0; i < city.ProcessingRules.Length; i++)
            {
                LocalProcessingRuleData rule = city.ProcessingRules[i];
                if (rule != null && HasText(rule.InputResourceId) && !resourceIds.Contains(rule.InputResourceId))
                {
                    resourceIds.Add(rule.InputResourceId);
                }
            }
        }

        return resourceIds;
    }

    private List<string> CollectCitySupplyResourceIds(WorldEntityData city)
    {
        List<string> resourceIds = new List<string>();
        if (city == null)
        {
            return resourceIds;
        }

        AddUniqueResourceIds(resourceIds, city.SupplyResourceIds);
        AddUniqueResourceIds(resourceIds, city.OutputResourceIds);
        if (city.ProcessingRules != null)
        {
            for (int i = 0; i < city.ProcessingRules.Length; i++)
            {
                LocalProcessingRuleData rule = city.ProcessingRules[i];
                if (rule != null && HasText(rule.OutputResourceId) && !resourceIds.Contains(rule.OutputResourceId))
                {
                    resourceIds.Add(rule.OutputResourceId);
                }
            }
        }

        return resourceIds;
    }

    private static void AddUniqueResourceIds(List<string> destination, string[] resourceIds)
    {
        if (destination == null || resourceIds == null)
        {
            return;
        }

        for (int i = 0; i < resourceIds.Length; i++)
        {
            string resourceId = resourceIds[i];
            if (HasText(resourceId) && !destination.Contains(resourceId))
            {
                destination.Add(resourceId);
            }
        }
    }

    private static bool ContainsResourceId(string[] resourceIds, string resourceId)
    {
        if (resourceIds == null || !HasText(resourceId))
        {
            return false;
        }

        for (int i = 0; i < resourceIds.Length; i++)
        {
            if (resourceIds[i] == resourceId)
            {
                return true;
            }
        }

        return false;
    }

    private static string[] CloneStringArray(string[] values)
    {
        if (values == null || values.Length == 0)
        {
            return Array.Empty<string>();
        }

        string[] copy = new string[values.Length];
        Array.Copy(values, copy, values.Length);
        return copy;
    }

    private string GetEntityDisplayNameSafe(string entityId)
    {
        WorldEntityData entity = FindEntity(entityId);
        return entity != null && HasText(entity.DisplayName)
            ? entity.DisplayName
            : HasText(entityId)
                ? entityId
                : "None";
    }
}
