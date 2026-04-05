using System;
using System.Collections.Generic;

public sealed class PrototypeRpgEncounterTemplateDefinition
{
    public string TemplateId { get; }
    public string EncounterDefinitionId { get; }
    public string EncounterId { get; }
    public int RoomIndex { get; }
    public string DisplayName { get; }
    public string EncounterTypeLabel { get; }
    public string RoomTypeLabel { get; }

    public PrototypeRpgEncounterTemplateDefinition(string templateId, string encounterDefinitionId, string encounterId, int roomIndex, string displayName, string encounterTypeLabel, string roomTypeLabel)
    {
        TemplateId = string.IsNullOrWhiteSpace(templateId) ? string.Empty : templateId.Trim();
        EncounterDefinitionId = string.IsNullOrWhiteSpace(encounterDefinitionId) ? string.Empty : encounterDefinitionId.Trim();
        EncounterId = string.IsNullOrWhiteSpace(encounterId) ? string.Empty : encounterId.Trim();
        RoomIndex = roomIndex > 0 ? roomIndex : 1;
        DisplayName = string.IsNullOrWhiteSpace(displayName) ? "Encounter" : displayName.Trim();
        EncounterTypeLabel = string.IsNullOrWhiteSpace(encounterTypeLabel) ? "Skirmish" : encounterTypeLabel.Trim();
        RoomTypeLabel = string.IsNullOrWhiteSpace(roomTypeLabel) ? "Skirmish Room" : roomTypeLabel.Trim();
    }
}

public sealed class PrototypeRpgEliteTemplateDefinition
{
    public string TemplateId { get; }
    public string EncounterDefinitionId { get; }
    public string EncounterId { get; }
    public int RoomIndex { get; }
    public string DisplayName { get; }
    public string EliteStyleLabel { get; }
    public string RewardPreviewLabel { get; }
    public int RewardAmountPreviewHint { get; }
    public string DifficultyHintLabel { get; }
    public int RecommendedPowerHint { get; }

    public PrototypeRpgEliteTemplateDefinition(string templateId, string encounterDefinitionId, string encounterId, int roomIndex, string displayName, string eliteStyleLabel, string rewardPreviewLabel, int rewardAmountPreviewHint, string difficultyHintLabel, int recommendedPowerHint)
    {
        TemplateId = string.IsNullOrWhiteSpace(templateId) ? string.Empty : templateId.Trim();
        EncounterDefinitionId = string.IsNullOrWhiteSpace(encounterDefinitionId) ? string.Empty : encounterDefinitionId.Trim();
        EncounterId = string.IsNullOrWhiteSpace(encounterId) ? string.Empty : encounterId.Trim();
        RoomIndex = roomIndex > 0 ? roomIndex : 3;
        DisplayName = string.IsNullOrWhiteSpace(displayName) ? "Elite Encounter" : displayName.Trim();
        EliteStyleLabel = string.IsNullOrWhiteSpace(eliteStyleLabel) ? string.Empty : eliteStyleLabel.Trim();
        RewardPreviewLabel = string.IsNullOrWhiteSpace(rewardPreviewLabel) ? string.Empty : rewardPreviewLabel.Trim();
        RewardAmountPreviewHint = rewardAmountPreviewHint > 0 ? rewardAmountPreviewHint : 0;
        DifficultyHintLabel = string.IsNullOrWhiteSpace(difficultyHintLabel) ? string.Empty : difficultyHintLabel.Trim();
        RecommendedPowerHint = recommendedPowerHint > 0 ? recommendedPowerHint : 0;
    }
}

public sealed class PrototypeRpgEncounterVariationDefinition
{
    public string VariationId { get; }
    public string DungeonId { get; }
    public string RouteId { get; }
    public string RouteRiskLabel { get; }
    public string RewardPreviewLabel { get; }
    public string DifficultyHintLabel { get; }
    public int RecommendedPowerHint { get; }
    public PrototypeRpgEncounterTemplateDefinition[] RoomTemplates { get; }
    public PrototypeRpgEliteTemplateDefinition EliteTemplate { get; }

    public PrototypeRpgEncounterVariationDefinition(string variationId, string dungeonId, string routeId, string routeRiskLabel, string rewardPreviewLabel, string difficultyHintLabel, int recommendedPowerHint, PrototypeRpgEncounterTemplateDefinition[] roomTemplates, PrototypeRpgEliteTemplateDefinition eliteTemplate)
    {
        VariationId = string.IsNullOrWhiteSpace(variationId) ? string.Empty : variationId.Trim();
        DungeonId = string.IsNullOrWhiteSpace(dungeonId) ? string.Empty : dungeonId.Trim();
        RouteId = string.IsNullOrWhiteSpace(routeId) ? string.Empty : routeId.Trim();
        RouteRiskLabel = string.IsNullOrWhiteSpace(routeRiskLabel) ? string.Empty : routeRiskLabel.Trim();
        RewardPreviewLabel = string.IsNullOrWhiteSpace(rewardPreviewLabel) ? string.Empty : rewardPreviewLabel.Trim();
        DifficultyHintLabel = string.IsNullOrWhiteSpace(difficultyHintLabel) ? string.Empty : difficultyHintLabel.Trim();
        RecommendedPowerHint = recommendedPowerHint > 0 ? recommendedPowerHint : 0;
        RoomTemplates = roomTemplates != null && roomTemplates.Length > 0 ? (PrototypeRpgEncounterTemplateDefinition[])roomTemplates.Clone() : Array.Empty<PrototypeRpgEncounterTemplateDefinition>();
        EliteTemplate = eliteTemplate;
    }
}

public static class PrototypeRpgEncounterVariationCatalog
{
    private static readonly Dictionary<string, PrototypeRpgEncounterVariationDefinition> Definitions = new Dictionary<string, PrototypeRpgEncounterVariationDefinition>
    {
        ["dungeon-alpha-safe"] = new PrototypeRpgEncounterVariationDefinition(
            "dungeon-alpha-safe",
            "dungeon-alpha",
            "safe",
            "Safe",
            "Royal Gel Cache",
            "Safe Route | Stable Pressure",
            8,
            new[]
            {
                new PrototypeRpgEncounterTemplateDefinition("alpha-safe-room1-template", "alpha-safe-room1", "encounter-room-1", 1, "Slime Front", "Skirmish", "Skirmish Room"),
                new PrototypeRpgEncounterTemplateDefinition("alpha-safe-room2-template", "alpha-safe-room2", "encounter-room-2", 2, "Watch Hall", "Skirmish", "Skirmish Room")
            },
            new PrototypeRpgEliteTemplateDefinition("alpha-safe-elite-template", "alpha-safe-elite", "encounter-room-elite", 3, "Slime Monarch", "Royal Slime Elite | Surging Pressure", "Royal Gel Cache", 6, "Elite | Royal Pressure", 9)),
        ["dungeon-alpha-risky"] = new PrototypeRpgEncounterVariationDefinition(
            "dungeon-alpha-risky",
            "dungeon-alpha",
            "risky",
            "Risky",
            "Volatile Core Cache",
            "Risky Route | Volatile Pressure",
            10,
            new[]
            {
                new PrototypeRpgEncounterTemplateDefinition("alpha-risky-room1-template", "alpha-risky-room1", "encounter-room-1", 1, "Mixed Front", "Skirmish", "Skirmish Room"),
                new PrototypeRpgEncounterTemplateDefinition("alpha-risky-room2-template", "alpha-risky-room2", "encounter-room-2", 2, "Goblin Pair Hall", "Skirmish", "Skirmish Room")
            },
            new PrototypeRpgEliteTemplateDefinition("alpha-risky-elite-template", "alpha-risky-elite", "encounter-room-elite", 3, "Gel Core", "Volatile Slime Elite | Surging Pressure", "Volatile Core Cache", 8, "Elite | Volatile Pressure", 11)),
        ["dungeon-beta-safe"] = new PrototypeRpgEncounterVariationDefinition(
            "dungeon-beta-safe",
            "dungeon-beta",
            "safe",
            "Safe",
            "Captain's Stash",
            "Safe Route | Guarded Patrol",
            11,
            new[]
            {
                new PrototypeRpgEncounterTemplateDefinition("beta-safe-room1-template", "beta-safe-room1", "encounter-room-1", 1, "Scout Gate", "Skirmish", "Skirmish Room"),
                new PrototypeRpgEncounterTemplateDefinition("beta-safe-room2-template", "beta-safe-room2", "encounter-room-2", 2, "Guarded Vault", "Skirmish", "Skirmish Room")
            },
            new PrototypeRpgEliteTemplateDefinition("beta-safe-elite-template", "beta-safe-elite", "encounter-room-elite", 3, "Goblin Captain", "Goblin Elite | Focused Command", "Captain's Stash", 9, "Elite | Guarded Pressure", 12)),
        ["dungeon-beta-risky"] = new PrototypeRpgEncounterVariationDefinition(
            "dungeon-beta-risky",
            "dungeon-beta",
            "risky",
            "Risky",
            "Raider War Spoils",
            "Risky Route | Raider Pressure",
            13,
            new[]
            {
                new PrototypeRpgEncounterTemplateDefinition("beta-risky-room1-template", "beta-risky-room1", "encounter-room-1", 1, "Raider Gate", "Skirmish", "Skirmish Room"),
                new PrototypeRpgEncounterTemplateDefinition("beta-risky-room2-template", "beta-risky-room2", "encounter-room-2", 2, "Ambush Hall", "Skirmish", "Skirmish Room")
            },
            new PrototypeRpgEliteTemplateDefinition("beta-risky-elite-template", "beta-risky-elite", "encounter-room-elite", 3, "Raider Chief", "Goblin Elite | Focused Execution", "Raider War Spoils", 11, "Elite | Raider Pressure", 14))
    };

    public static PrototypeRpgEncounterVariationDefinition ResolveVariation(string variationId)
    {
        if (string.IsNullOrWhiteSpace(variationId))
        {
            return null;
        }

        Definitions.TryGetValue(variationId.Trim(), out PrototypeRpgEncounterVariationDefinition definition);
        return definition;
    }

    public static PrototypeRpgEncounterVariationDefinition ResolveVariation(string dungeonId, string routeId)
    {
        string variationId = BuildVariationId(dungeonId, routeId);
        return string.IsNullOrEmpty(variationId) ? null : ResolveVariation(variationId);
    }

    private static string BuildVariationId(string dungeonId, string routeId)
    {
        if (string.IsNullOrWhiteSpace(dungeonId))
        {
            return string.Empty;
        }

        string normalizedDungeonId = dungeonId.Trim();
        string normalizedRouteId = string.IsNullOrWhiteSpace(routeId) ? "safe" : routeId.Trim().ToLowerInvariant();
        return normalizedDungeonId + "-" + normalizedRouteId;
    }
}
