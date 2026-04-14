using System;
using System.Collections.Generic;
using UnityEngine;

public sealed partial class StaticPlaceholderWorldView
{
    private string GetDungeonContentSourceLabel(string cityId, string dungeonId, string routeId)
    {
        return GoldenPathContentRegistry.BuildContentSourceLabel(cityId, dungeonId, NormalizeRouteChoiceId(routeId));
    }

    private GoldenPathRouteMeaningDefinition ResolveRouteMeaning(GoldenPathRouteDefinition routeDefinition)
    {
        return routeDefinition != null &&
               HasText(routeDefinition.RouteMeaningId) &&
               GoldenPathContentRegistry.TryGetRouteMeaning(routeDefinition.RouteMeaningId, out GoldenPathRouteMeaningDefinition routeMeaning)
            ? routeMeaning
            : null;
    }

    private GoldenPathOutcomeMeaningDefinition ResolveOutcomeMeaning(GoldenPathChainDefinition chainDefinition)
    {
        return chainDefinition != null &&
               HasText(chainDefinition.OutcomeMeaningId) &&
               GoldenPathContentRegistry.TryGetOutcomeMeaning(chainDefinition.OutcomeMeaningId, out GoldenPathOutcomeMeaningDefinition outcomeMeaning)
            ? outcomeMeaning
            : null;
    }

    private bool TryResolveEncounterBattleAuthoring(
        string cityId,
        string dungeonId,
        string routeId,
        string encounterId,
        out GoldenPathRoomDefinition roomDefinition,
        out GoldenPathEncounterProfileDefinition encounterProfile,
        out GoldenPathBattleSetupDefinition battleSetup)
    {
        roomDefinition = null;
        encounterProfile = null;
        battleSetup = null;

        if (!GoldenPathContentRegistry.TryGetCanonicalEncounterRoom(
                cityId,
                dungeonId,
                NormalizeRouteChoiceId(routeId),
                encounterId,
                out GoldenPathChainDefinition _,
                out GoldenPathRouteDefinition _,
                out roomDefinition) ||
            roomDefinition == null)
        {
            return false;
        }

        if (HasText(roomDefinition.EncounterProfileId))
        {
            GoldenPathContentRegistry.TryGetEncounterProfile(roomDefinition.EncounterProfileId, out encounterProfile);
        }

        if (HasText(roomDefinition.BattleSetupId))
        {
            GoldenPathContentRegistry.TryGetBattleSetup(roomDefinition.BattleSetupId, out battleSetup);
        }

        return encounterProfile != null || battleSetup != null;
    }

    private string BuildEncounterProfileSourceLabel(string encounterProfileId)
    {
        return HasText(encounterProfileId) ? "shared:" + encounterProfileId : "fallback:hardcoded";
    }

    private string BuildBattleSetupSourceLabel(string battleSetupId)
    {
        return HasText(battleSetupId) ? "shared:" + battleSetupId : "fallback:hardcoded";
    }

    private DungeonIdentityTemplate TryBuildDungeonTemplateFromContent(string dungeonId, DungeonIdentityTemplate fallbackTemplate)
    {
        if (fallbackTemplate == null)
        {
            return fallbackTemplate;
        }

        GoldenPathChainDefinition primaryChainDefinition = GoldenPathContentRegistry.TryGetChain(_currentHomeCityId, dungeonId, out GoldenPathChainDefinition primaryChain)
            ? primaryChain
            : null;
        GoldenPathChainDefinition safeChainDefinition = null;
        GoldenPathRouteDefinition safeRouteDefinition = null;
        GoldenPathChainDefinition riskyChainDefinition = null;
        GoldenPathRouteDefinition riskyRouteDefinition = null;

        GoldenPathContentRegistry.TryGetCanonicalRoute(_currentHomeCityId, dungeonId, SafeRouteId, out safeChainDefinition, out safeRouteDefinition);
        GoldenPathContentRegistry.TryGetCanonicalRoute(_currentHomeCityId, dungeonId, RiskyRouteId, out riskyChainDefinition, out riskyRouteDefinition);

        GoldenPathChainDefinition identityDefinition = primaryChainDefinition ?? safeChainDefinition ?? riskyChainDefinition;
        if (identityDefinition == null && safeRouteDefinition == null && riskyRouteDefinition == null)
        {
            return fallbackTemplate;
        }

        DungeonRouteTemplate routeOption1 = safeRouteDefinition != null
            ? BuildRouteTemplateFromContent(safeRouteDefinition, fallbackTemplate.RouteOption1)
            : fallbackTemplate.RouteOption1;
        DungeonRouteTemplate routeOption2 = riskyRouteDefinition != null
            ? BuildRouteTemplateFromContent(riskyRouteDefinition, fallbackTemplate.RouteOption2)
            : fallbackTemplate.RouteOption2;

        return new DungeonIdentityTemplate(
            HasText(identityDefinition != null ? identityDefinition.DungeonId : string.Empty) ? identityDefinition.DungeonId : fallbackTemplate.DungeonId,
            HasText(identityDefinition != null ? identityDefinition.DungeonLabel : string.Empty) ? identityDefinition.DungeonLabel : fallbackTemplate.DungeonLabel,
            HasText(identityDefinition != null ? identityDefinition.DangerLabel : string.Empty) ? identityDefinition.DangerLabel : fallbackTemplate.DangerLabel,
            HasText(identityDefinition != null ? identityDefinition.StyleLabel : string.Empty) ? identityDefinition.StyleLabel : fallbackTemplate.StyleLabel,
            routeOption1,
            routeOption2);
    }

    private bool TryBuildPlannedRoomSequenceFromContent(string cityId, string dungeonId, string routeId)
    {
        if (!GoldenPathContentRegistry.TryGetCanonicalRoute(cityId, dungeonId, NormalizeRouteChoiceId(routeId), out GoldenPathChainDefinition _, out GoldenPathRouteDefinition routeDefinition) ||
            routeDefinition == null ||
            routeDefinition.Rooms == null ||
            routeDefinition.Rooms.Length == 0)
        {
            return false;
        }

        for (int i = 0; i < routeDefinition.Rooms.Length; i++)
        {
            GoldenPathRoomDefinition roomDefinition = routeDefinition.Rooms[i];
            if (roomDefinition == null || !HasText(roomDefinition.RoomId))
            {
                continue;
            }

            AddPlannedRoomStep(
                roomDefinition.RoomId,
                HasText(roomDefinition.DisplayName) ? roomDefinition.DisplayName : "Room",
                HasText(roomDefinition.RoomTypeLabel) ? roomDefinition.RoomTypeLabel : BuildRoomTypeLabel(ParseRoomType(roomDefinition.RoomType)),
                ParseRoomType(roomDefinition.RoomType),
                ResolveMarkerPosition(roomDefinition.MarkerKey),
                roomDefinition.EncounterId ?? string.Empty);
        }

        return _plannedRooms.Count > 0;
    }

    private string TryBuildRoomPathPreviewFromContent(string cityId, string dungeonId, string routeId)
    {
        if (!GoldenPathContentRegistry.TryGetCanonicalRoute(cityId, dungeonId, NormalizeRouteChoiceId(routeId), out GoldenPathChainDefinition _, out GoldenPathRouteDefinition routeDefinition) ||
            routeDefinition == null ||
            routeDefinition.Rooms == null ||
            routeDefinition.Rooms.Length == 0)
        {
            return string.Empty;
        }

        List<string> roomLabels = new List<string>();
        for (int i = 0; i < routeDefinition.Rooms.Length; i++)
        {
            GoldenPathRoomDefinition roomDefinition = routeDefinition.Rooms[i];
            if (roomDefinition == null)
            {
                continue;
            }

            roomLabels.Add(BuildRoomPathTypeLabel(ParseRoomType(roomDefinition.RoomType)));
        }

        return roomLabels.Count > 0 ? string.Join(" -> ", roomLabels.ToArray()) : string.Empty;
    }

    private string TryBuildExpectedNeedImpactFromContent(string cityId, string dungeonId, string routeId)
    {
        if (!GoldenPathContentRegistry.TryGetCanonicalRoute(cityId, dungeonId, NormalizeRouteChoiceId(routeId), out GoldenPathChainDefinition chainDefinition, out GoldenPathRouteDefinition routeDefinition))
        {
            return string.Empty;
        }

        GoldenPathRouteMeaningDefinition routeMeaning = ResolveRouteMeaning(routeDefinition);
        if (routeMeaning != null && HasText(routeMeaning.ExpectedNeedImpactText))
        {
            return routeMeaning.ExpectedNeedImpactText;
        }

        GoldenPathOutcomeMeaningDefinition outcomeMeaning = ResolveOutcomeMeaning(chainDefinition);

        return HasText(chainDefinition.ResultImpactMeaningText)
            ? chainDefinition.ResultImpactMeaningText
            : outcomeMeaning != null && HasText(outcomeMeaning.CityImpactMeaningText)
                ? outcomeMeaning.CityImpactMeaningText
                : HasText(chainDefinition.RewardMeaningText)
                    ? chainDefinition.RewardMeaningText
                    : outcomeMeaning != null ? outcomeMeaning.RewardMeaningText : string.Empty;
    }

    private DungeonRouteTemplate BuildRouteTemplateFromContent(GoldenPathRouteDefinition routeDefinition, DungeonRouteTemplate fallbackTemplate)
    {
        if (routeDefinition == null)
        {
            return fallbackTemplate;
        }

        GoldenPathRouteMeaningDefinition routeMeaning = ResolveRouteMeaning(routeDefinition);

        return new DungeonRouteTemplate(
            HasText(routeDefinition.RouteId) ? routeDefinition.RouteId : fallbackTemplate != null ? fallbackTemplate.RouteId : string.Empty,
            HasText(routeDefinition.RouteLabel) ? routeDefinition.RouteLabel : fallbackTemplate != null ? fallbackTemplate.RouteLabel : "Route",
            HasText(routeDefinition.Description) ? routeDefinition.Description : fallbackTemplate != null ? fallbackTemplate.Description : "A fixed dungeon path.",
            HasText(routeDefinition.RiskLabel)
                ? routeDefinition.RiskLabel
                : routeMeaning != null && HasText(routeMeaning.RiskLabel)
                    ? routeMeaning.RiskLabel
                    : fallbackTemplate != null ? fallbackTemplate.RiskLabel : "Standard",
            HasText(routeDefinition.EncounterPreview) ? routeDefinition.EncounterPreview : fallbackTemplate != null ? fallbackTemplate.EncounterPreview : "Two encounters",
            HasText(routeDefinition.EventFocus)
                ? routeDefinition.EventFocus
                : routeMeaning != null && HasText(routeMeaning.EventFocus)
                    ? routeMeaning.EventFocus
                    : fallbackTemplate != null ? fallbackTemplate.EventFocus : "Balanced blessing",
            HasText(routeDefinition.RewardPreview)
                ? routeDefinition.RewardPreview
                : routeMeaning != null && HasText(routeMeaning.RewardPreview)
                    ? routeMeaning.RewardPreview
                    : fallbackTemplate != null ? fallbackTemplate.RewardPreview : "Balanced rewards",
            routeDefinition.BattleLootAmount > 0 ? routeDefinition.BattleLootAmount : fallbackTemplate != null ? fallbackTemplate.BattleLootAmount : DungeonRewardAmount * 4,
            routeDefinition.RecoverAmount > 0 ? routeDefinition.RecoverAmount : fallbackTemplate != null ? fallbackTemplate.RecoverAmount : ShrineEventRecoverAmount,
            routeDefinition.BonusLootAmount > 0 ? routeDefinition.BonusLootAmount : fallbackTemplate != null ? fallbackTemplate.BonusLootAmount : ShrineEventBonusLootAmount,
            routeDefinition.ChestRewardAmount > 0 ? routeDefinition.ChestRewardAmount : fallbackTemplate != null ? fallbackTemplate.ChestRewardAmount : ChestRewardAmount);
    }

    private DungeonRoomType ParseRoomType(string roomTypeText)
    {
        string normalized = NormalizeReadableRoomText(roomTypeText);
        switch (normalized)
        {
            case "start":
                return DungeonRoomType.Start;
            case "skirmish":
                return DungeonRoomType.Skirmish;
            case "cache":
                return DungeonRoomType.Cache;
            case "shrine":
                return DungeonRoomType.Shrine;
            case "preparation":
                return DungeonRoomType.Preparation;
            case "elite":
                return DungeonRoomType.Elite;
            default:
                return DungeonRoomType.Skirmish;
        }
    }

    private string BuildRoomTypeLabel(DungeonRoomType roomType)
    {
        switch (roomType)
        {
            case DungeonRoomType.Start:
                return "Start Room";
            case DungeonRoomType.Skirmish:
                return "Skirmish Room";
            case DungeonRoomType.Cache:
                return "Cache Room";
            case DungeonRoomType.Shrine:
                return "Shrine Room";
            case DungeonRoomType.Preparation:
                return "Preparation Room";
            case DungeonRoomType.Elite:
                return "Elite Chamber";
            default:
                return "Room";
        }
    }

    private string BuildRoomPathTypeLabel(DungeonRoomType roomType)
    {
        switch (roomType)
        {
            case DungeonRoomType.Start:
                return "Start";
            case DungeonRoomType.Skirmish:
                return "Skirmish";
            case DungeonRoomType.Cache:
                return "Cache";
            case DungeonRoomType.Shrine:
                return "Shrine";
            case DungeonRoomType.Preparation:
                return "Preparation";
            case DungeonRoomType.Elite:
                return "Elite";
            default:
                return "Room";
        }
    }

    private Vector2Int ResolveMarkerPosition(string markerKey)
    {
        switch (NormalizeReadableRoomText(markerKey))
        {
            case "room1encounter":
                return Room1EncounterMarkerPosition;
            case "room2encounter":
                return Room2EncounterMarkerPosition;
            case "shrine":
                return ShrineEventGridPosition;
            case "cache":
                return Room2ChestGridPosition;
            case "room2event":
                return Room2EventGridPosition;
            case "prep":
                return PreparationRoomGridPosition;
            case "elite":
                return EliteEncounterMarkerPosition;
            default:
                return Room1EncounterMarkerPosition;
        }
    }

    private string NormalizeReadableRoomText(string text)
    {
        if (string.IsNullOrEmpty(text))
        {
            return string.Empty;
        }

        char[] buffer = new char[text.Length];
        int count = 0;
        for (int i = 0; i < text.Length; i++)
        {
            char character = text[i];
            if (char.IsLetterOrDigit(character))
            {
                buffer[count++] = char.ToLowerInvariant(character);
            }
        }

        return new string(buffer, 0, count);
    }
}
