public sealed class ResourceData
{
    public string Id { get; }
    public string DisplayName { get; }
    public string Category { get; }

    public ResourceData(string id, string displayName, string category)
    {
        Id = id ?? string.Empty;
        DisplayName = displayName ?? string.Empty;
        Category = category ?? string.Empty;
    }
}

public static class PlaceholderResourceDataFactory
{
    public static ResourceData[] Create()
    {
        return new[]
        {
            new ResourceData("grain", "Grain", "staple"),
            new ResourceData("iron_ore", "Iron Ore", "material"),
            new ResourceData("mana_shard", "Mana Shard", "arcane"),
            new ResourceData("refined_mana", "Refined Mana", "arcane-refined")
        };
    }
}
