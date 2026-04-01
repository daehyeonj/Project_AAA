using System;
using System.Collections.Generic;

public sealed class DirectTradeOpportunityData
{
    public string SupplierEntityId { get; }
    public string ConsumerEntityId { get; }
    public string ResourceId { get; }
    public string RouteId { get; }

    public DirectTradeOpportunityData(string supplierEntityId, string consumerEntityId, string resourceId, string routeId)
    {
        SupplierEntityId = supplierEntityId ?? string.Empty;
        ConsumerEntityId = consumerEntityId ?? string.Empty;
        ResourceId = resourceId ?? string.Empty;
        RouteId = routeId ?? string.Empty;
    }
}

public sealed class UnmetCityNeedData
{
    public string CityEntityId { get; }
    public string ResourceId { get; }

    public UnmetCityNeedData(string cityEntityId, string resourceId)
    {
        CityEntityId = cityEntityId ?? string.Empty;
        ResourceId = resourceId ?? string.Empty;
    }
}

public sealed class UnclaimedDungeonOutputData
{
    public string DungeonEntityId { get; }
    public string ResourceId { get; }

    public UnclaimedDungeonOutputData(string dungeonEntityId, string resourceId)
    {
        DungeonEntityId = dungeonEntityId ?? string.Empty;
        ResourceId = resourceId ?? string.Empty;
    }
}

public sealed class DirectTradeScanResult
{
    public static readonly DirectTradeScanResult Empty = new DirectTradeScanResult(
        Array.Empty<DirectTradeOpportunityData>(),
        Array.Empty<UnmetCityNeedData>(),
        Array.Empty<UnclaimedDungeonOutputData>());

    public DirectTradeOpportunityData[] Opportunities { get; }
    public UnmetCityNeedData[] UnmetCityNeeds { get; }
    public UnclaimedDungeonOutputData[] UnclaimedDungeonOutputs { get; }

    public DirectTradeScanResult(
        DirectTradeOpportunityData[] opportunities,
        UnmetCityNeedData[] unmetCityNeeds,
        UnclaimedDungeonOutputData[] unclaimedDungeonOutputs)
    {
        Opportunities = opportunities ?? Array.Empty<DirectTradeOpportunityData>();
        UnmetCityNeeds = unmetCityNeeds ?? Array.Empty<UnmetCityNeedData>();
        UnclaimedDungeonOutputs = unclaimedDungeonOutputs ?? Array.Empty<UnclaimedDungeonOutputData>();
    }
}

public static class DirectTradeScanner
{
    public static DirectTradeScanResult Scan(WorldData worldData)
    {
        return Scan(worldData, null, null);
    }

    public static DirectTradeScanResult Scan(
        WorldData worldData,
        Func<string, string, int> getRuntimeStock,
        Func<string, string, bool> canExportResource)
    {
        if (worldData == null)
        {
            return DirectTradeScanResult.Empty;
        }

        Dictionary<string, WorldEntityData> entityLookup = BuildEntityLookup(worldData.Entities);
        List<DirectTradeOpportunityData> opportunities = new List<DirectTradeOpportunityData>();

        WorldRouteData[] routes = worldData.Routes ?? Array.Empty<WorldRouteData>();
        for (int i = 0; i < routes.Length; i++)
        {
            WorldRouteData route = routes[i];
            if (route == null)
            {
                continue;
            }

            if (!entityLookup.TryGetValue(route.FromEntityId, out WorldEntityData fromEntity) ||
                !entityLookup.TryGetValue(route.ToEntityId, out WorldEntityData toEntity))
            {
                continue;
            }

            ScanRouteSide(opportunities, fromEntity, toEntity, route, getRuntimeStock, canExportResource);
            ScanRouteSide(opportunities, toEntity, fromEntity, route, getRuntimeStock, canExportResource);
        }

        List<UnmetCityNeedData> unmetCityNeeds = new List<UnmetCityNeedData>();
        WorldEntityData[] entities = worldData.Entities ?? Array.Empty<WorldEntityData>();
        for (int i = 0; i < entities.Length; i++)
        {
            WorldEntityData entity = entities[i];
            if (entity == null || entity.Kind != WorldEntityKind.City)
            {
                continue;
            }

            for (int needIndex = 0; needIndex < entity.NeedResourceIds.Length; needIndex++)
            {
                string needId = entity.NeedResourceIds[needIndex];
                if (string.IsNullOrEmpty(needId) ||
                    HasIncomingOpportunity(opportunities, entity.Id, needId) ||
                    CanSatisfyNeedLocally(entity, needId, opportunities, getRuntimeStock))
                {
                    continue;
                }

                unmetCityNeeds.Add(new UnmetCityNeedData(entity.Id, needId));
            }
        }

        List<UnclaimedDungeonOutputData> unclaimedDungeonOutputs = new List<UnclaimedDungeonOutputData>();
        for (int i = 0; i < entities.Length; i++)
        {
            WorldEntityData entity = entities[i];
            if (entity == null || entity.Kind != WorldEntityKind.Dungeon)
            {
                continue;
            }

            for (int outputIndex = 0; outputIndex < entity.OutputResourceIds.Length; outputIndex++)
            {
                string outputId = entity.OutputResourceIds[outputIndex];
                if (!string.IsNullOrEmpty(outputId))
                {
                    unclaimedDungeonOutputs.Add(new UnclaimedDungeonOutputData(entity.Id, outputId));
                }
            }
        }

        return new DirectTradeScanResult(
            opportunities.ToArray(),
            unmetCityNeeds.ToArray(),
            unclaimedDungeonOutputs.ToArray());
    }

    private static Dictionary<string, WorldEntityData> BuildEntityLookup(WorldEntityData[] entities)
    {
        Dictionary<string, WorldEntityData> lookup = new Dictionary<string, WorldEntityData>();
        if (entities == null)
        {
            return lookup;
        }

        for (int i = 0; i < entities.Length; i++)
        {
            WorldEntityData entity = entities[i];
            if (entity == null || string.IsNullOrEmpty(entity.Id) || lookup.ContainsKey(entity.Id))
            {
                continue;
            }

            lookup.Add(entity.Id, entity);
        }

        return lookup;
    }

    private static void ScanRouteSide(
        List<DirectTradeOpportunityData> opportunities,
        WorldEntityData supplier,
        WorldEntityData consumer,
        WorldRouteData route,
        Func<string, string, int> getRuntimeStock,
        Func<string, string, bool> canExportResource)
    {
        if (opportunities == null || supplier == null || consumer == null || route == null)
        {
            return;
        }

        if (supplier.Kind != WorldEntityKind.City || consumer.Kind != WorldEntityKind.City)
        {
            return;
        }

        List<string> demandResourceIds = CollectDemandResourceIds(consumer);
        for (int demandIndex = 0; demandIndex < demandResourceIds.Count; demandIndex++)
        {
            string demandId = demandResourceIds[demandIndex];
            if (string.IsNullOrEmpty(demandId))
            {
                continue;
            }

            bool supplierCanExport = canExportResource != null
                ? canExportResource(supplier.Id, demandId)
                : ContainsResourceId(supplier.SupplyResourceIds, demandId) ||
                    (getRuntimeStock != null && getRuntimeStock(supplier.Id, demandId) > 0);
            if (!supplierCanExport)
            {
                continue;
            }

            if (HasOpportunity(opportunities, supplier.Id, consumer.Id, demandId, route.Id))
            {
                continue;
            }

            opportunities.Add(new DirectTradeOpportunityData(supplier.Id, consumer.Id, demandId, route.Id));
        }
    }

    private static List<string> CollectDemandResourceIds(WorldEntityData consumer)
    {
        List<string> ids = new List<string>();
        if (consumer == null)
        {
            return ids;
        }

        AddResourceIds(ids, consumer.NeedResourceIds);
        if (consumer.ProcessingRules != null)
        {
            for (int i = 0; i < consumer.ProcessingRules.Length; i++)
            {
                LocalProcessingRuleData rule = consumer.ProcessingRules[i];
                if (rule == null || string.IsNullOrEmpty(rule.InputResourceId) || ids.Contains(rule.InputResourceId))
                {
                    continue;
                }

                ids.Add(rule.InputResourceId);
            }
        }

        return ids;
    }

    private static void AddResourceIds(List<string> ids, string[] resourceIds)
    {
        if (ids == null || resourceIds == null)
        {
            return;
        }

        for (int i = 0; i < resourceIds.Length; i++)
        {
            string resourceId = resourceIds[i];
            if (string.IsNullOrEmpty(resourceId) || ids.Contains(resourceId))
            {
                continue;
            }

            ids.Add(resourceId);
        }
    }

    private static bool CanSatisfyNeedLocally(
        WorldEntityData entity,
        string resourceId,
        List<DirectTradeOpportunityData> opportunities,
        Func<string, string, int> getRuntimeStock)
    {
        if (entity == null || string.IsNullOrEmpty(resourceId) || entity.ProcessingRules == null)
        {
            return false;
        }

        for (int i = 0; i < entity.ProcessingRules.Length; i++)
        {
            LocalProcessingRuleData rule = entity.ProcessingRules[i];
            if (rule == null || rule.OutputResourceId != resourceId || string.IsNullOrEmpty(rule.InputResourceId))
            {
                continue;
            }

            bool hasInputStock = getRuntimeStock != null && getRuntimeStock(entity.Id, rule.InputResourceId) > 0;
            if (hasInputStock || HasIncomingOpportunity(opportunities, entity.Id, rule.InputResourceId))
            {
                return true;
            }
        }

        return false;
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

    private static bool HasOpportunity(
        List<DirectTradeOpportunityData> opportunities,
        string supplierEntityId,
        string consumerEntityId,
        string resourceId,
        string routeId)
    {
        for (int i = 0; i < opportunities.Count; i++)
        {
            DirectTradeOpportunityData opportunity = opportunities[i];
            if (opportunity.SupplierEntityId == supplierEntityId &&
                opportunity.ConsumerEntityId == consumerEntityId &&
                opportunity.ResourceId == resourceId &&
                opportunity.RouteId == routeId)
            {
                return true;
            }
        }

        return false;
    }

    private static bool HasIncomingOpportunity(List<DirectTradeOpportunityData> opportunities, string consumerEntityId, string resourceId)
    {
        for (int i = 0; i < opportunities.Count; i++)
        {
            DirectTradeOpportunityData opportunity = opportunities[i];
            if (opportunity.ConsumerEntityId == consumerEntityId && opportunity.ResourceId == resourceId)
            {
                return true;
            }
        }

        return false;
    }
}