using UnityEngine;

public enum WorldEntityKind
{
    City,
    Dungeon
}

public sealed class LocalProcessingRuleData
{
    public string InputResourceId { get; }
    public string OutputResourceId { get; }
    public int MaxRunsPerDay { get; }

    public LocalProcessingRuleData(string inputResourceId, string outputResourceId, int maxRunsPerDay)
    {
        InputResourceId = inputResourceId ?? string.Empty;
        OutputResourceId = outputResourceId ?? string.Empty;
        MaxRunsPerDay = maxRunsPerDay > 0 ? maxRunsPerDay : 1;
    }
}

public sealed class WorldEntityData
{
    public string Id { get; }
    public string DisplayName { get; }
    public WorldEntityKind Kind { get; }
    public Vector2 Position { get; }
    public string[] Tags { get; }
    public string PrimaryStatName { get; }
    public int PrimaryStatValue { get; }
    public string[] RelatedResourceIds { get; }
    public string[] ResourceRoleTags { get; }
    public string[] SupplyResourceIds { get; }
    public string[] NeedResourceIds { get; }
    public string[] HighPriorityNeedResourceIds { get; }
    public string[] OutputResourceIds { get; }
    public LocalProcessingRuleData[] ProcessingRules { get; }
    public string LinkedCityId { get; }

    public WorldEntityData(
        string id,
        string displayName,
        WorldEntityKind kind,
        Vector2 position,
        string[] tags,
        string primaryStatName,
        int primaryStatValue,
        string[] relatedResourceIds,
        string[] resourceRoleTags,
        string[] supplyResourceIds,
        string[] needResourceIds,
        string[] highPriorityNeedResourceIds,
        string[] outputResourceIds,
        LocalProcessingRuleData[] processingRules,
        string linkedCityId)
    {
        Id = id ?? string.Empty;
        DisplayName = displayName ?? string.Empty;
        Kind = kind;
        Position = position;
        Tags = tags ?? System.Array.Empty<string>();
        PrimaryStatName = primaryStatName ?? string.Empty;
        PrimaryStatValue = primaryStatValue;
        RelatedResourceIds = relatedResourceIds ?? System.Array.Empty<string>();
        ResourceRoleTags = resourceRoleTags ?? System.Array.Empty<string>();
        SupplyResourceIds = supplyResourceIds ?? System.Array.Empty<string>();
        NeedResourceIds = needResourceIds ?? System.Array.Empty<string>();
        HighPriorityNeedResourceIds = highPriorityNeedResourceIds ?? System.Array.Empty<string>();
        OutputResourceIds = outputResourceIds ?? System.Array.Empty<string>();
        ProcessingRules = processingRules ?? System.Array.Empty<LocalProcessingRuleData>();
        LinkedCityId = linkedCityId ?? string.Empty;
    }
}

public sealed class WorldRouteData
{
    public string Id { get; }
    public string FromEntityId { get; }
    public string ToEntityId { get; }
    public string[] Tags { get; }
    public int CapacityPerDay { get; }

    public WorldRouteData(string id, string fromEntityId, string toEntityId, string[] tags, int capacityPerDay)
    {
        Id = id ?? string.Empty;
        FromEntityId = fromEntityId ?? string.Empty;
        ToEntityId = toEntityId ?? string.Empty;
        Tags = tags ?? System.Array.Empty<string>();
        CapacityPerDay = capacityPerDay > 0 ? capacityPerDay : 1;
    }
}

public sealed class WorldData
{
    public WorldEntityData[] Entities { get; }
    public WorldRouteData[] Routes { get; }

    public WorldData(WorldEntityData[] entities, WorldRouteData[] routes)
    {
        Entities = entities ?? System.Array.Empty<WorldEntityData>();
        Routes = routes ?? System.Array.Empty<WorldRouteData>();
    }
}

public static class PlaceholderWorldDataFactory
{
    public static WorldData Create()
    {
        WorldEntityData[] entities =
        {
            new WorldEntityData(
                "city-a",
                "City A",
                WorldEntityKind.City,
                new Vector2(-3f, 1f),
                new[] { "city", "starter", "market" },
                "population",
                1200,
                new[] { "grain", "iron_ore", "mana_shard" },
                new[] { "staple-focus", "market-hub", "gateway" },
                new[] { "grain" },
                new[] { "iron_ore" },
                System.Array.Empty<string>(),
                System.Array.Empty<string>(),
                System.Array.Empty<LocalProcessingRuleData>(),
                string.Empty),
            new WorldEntityData(
                "city-b",
                "City B",
                WorldEntityKind.City,
                new Vector2(3f, -1f),
                new[] { "city", "frontier", "processor" },
                "population",
                850,
                new[] { "grain", "iron_ore", "mana_shard", "refined_mana" },
                new[] { "staple-focus", "frontier-material", "arcane-processing" },
                new[] { "iron_ore" },
                new[] { "grain", "refined_mana" },
                new[] { "grain" },
                System.Array.Empty<string>(),
                new[] { new LocalProcessingRuleData("mana_shard", "refined_mana", 1) },
                string.Empty),
            new WorldEntityData(
                "dungeon-alpha",
                "Dungeon Alpha",
                WorldEntityKind.Dungeon,
                new Vector2(-1f, -2f),
                new[] { "dungeon", "ruins", "low-tier" },
                "dangerLevel",
                3,
                new[] { "mana_shard", "iron_ore" },
                new[] { "arcane-source", "relic-site" },
                System.Array.Empty<string>(),
                System.Array.Empty<string>(),
                System.Array.Empty<string>(),
                new[] { "mana_shard" },
                System.Array.Empty<LocalProcessingRuleData>(),
                "city-a"),
            new WorldEntityData(
                "dungeon-beta",
                "Dungeon Beta",
                WorldEntityKind.Dungeon,
                new Vector2(4.5f, 2f),
                new[] { "dungeon", "watchpost", "high-risk" },
                "dangerLevel",
                5,
                new[] { "mana_shard", "refined_mana" },
                new[] { "goblin-den", "vault-route" },
                System.Array.Empty<string>(),
                System.Array.Empty<string>(),
                System.Array.Empty<string>(),
                new[] { "mana_shard" },
                System.Array.Empty<LocalProcessingRuleData>(),
                "city-b")
        };

        WorldRouteData[] routes =
        {
            new WorldRouteData("road-1", "city-a", "city-b", new[] { "road", "local", "safe" }, 2)
        };

        return new WorldData(entities, routes);
    }
}
