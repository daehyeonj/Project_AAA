using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public sealed class GoldenPathChainDefinition
{
    public string ChainId = string.Empty;
    public string CityId = string.Empty;
    public bool PrimaryCityDungeonDefinition = true;
    public bool SurfaceAsOpportunityVariant;
    public string BottleneckResourceId = string.Empty;
    public string BottleneckSummaryText = string.Empty;
    public string MissionObjectiveText = string.Empty;
    public string RewardResourceId = string.Empty;
    public string OutcomeMeaningId = string.Empty;
    public string CityDecisionMeaningId = string.Empty;
    public string RewardMeaningText = string.Empty;
    public string ResultImpactMeaningText = string.Empty;
    public string DungeonId = string.Empty;
    public string DungeonLabel = string.Empty;
    public string DangerLabel = string.Empty;
    public string StyleLabel = string.Empty;
    public GoldenPathRouteDefinition CanonicalRoute = new GoldenPathRouteDefinition();
}

[Serializable]
public sealed class GoldenPathRouteDefinition
{
    public string RouteId = string.Empty;
    public string RouteMeaningId = string.Empty;
    public string RouteLabel = string.Empty;
    public string Description = string.Empty;
    public string RiskLabel = string.Empty;
    public string EncounterPreview = string.Empty;
    public string EventFocus = string.Empty;
    public string RewardPreview = string.Empty;
    public int BattleLootAmount;
    public int RecoverAmount;
    public int BonusLootAmount;
    public int ChestRewardAmount;
    public string RepresentativeEncounterId = string.Empty;
    public string RepresentativeBattleLabel = string.Empty;
    public GoldenPathRoomDefinition[] Rooms = Array.Empty<GoldenPathRoomDefinition>();
}

[Serializable]
public sealed class GoldenPathRoomDefinition
{
    public string RoomId = string.Empty;
    public string DisplayName = string.Empty;
    public string RoomTypeLabel = string.Empty;
    public string RoomType = string.Empty;
    public string MarkerKey = string.Empty;
    public string EncounterId = string.Empty;
    public string EncounterProfileId = string.Empty;
    public string BattleSetupId = string.Empty;
}

[Serializable]
public sealed class GoldenPathRouteMeaningDefinition
{
    public string RouteMeaningId = string.Empty;
    public string ScenarioLabel = string.Empty;
    public string RiskLabel = string.Empty;
    public string ChooseWhenText = string.Empty;
    public string WatchOutText = string.Empty;
    public string FollowUpHintText = string.Empty;
    public string PartyFitText = string.Empty;
    public string CombatPlanText = string.Empty;
    public string EventFocus = string.Empty;
    public string RewardPreview = string.Empty;
    public string ExpectedNeedImpactText = string.Empty;
}

[Serializable]
public sealed class GoldenPathOutcomeMeaningDefinition
{
    public string OutcomeMeaningId = string.Empty;
    public string RewardResourceId = string.Empty;
    public string RewardMeaningText = string.Empty;
    public string CityImpactMeaningText = string.Empty;
    public string RecommendationShiftText = string.Empty;
}

[Serializable]
public sealed class GoldenPathCityDecisionMeaningDefinition
{
    public string CityDecisionMeaningId = string.Empty;
    public string BottleneckSummaryText = string.Empty;
    public string OpportunityReasonText = string.Empty;
    public string RecommendationRationaleText = string.Empty;
    public string WhyCityMattersText = string.Empty;
}

[Serializable]
public sealed class GoldenPathEncounterProfileDefinition
{
    public string EncounterProfileId = string.Empty;
    public string EncounterRoleTagsText = string.Empty;
    public string MissionRelevanceText = string.Empty;
    public string BattleContextText = string.Empty;
}

[Serializable]
public sealed class GoldenPathBattleSetupDefinition
{
    public string BattleSetupId = string.Empty;
    public string EnemyGroupLabel = string.Empty;
    public string SetupSummaryText = string.Empty;
    public bool RetreatAllowed = true;
    public string WinRelevanceText = string.Empty;
    public string LossRelevanceText = string.Empty;
    public string EliteTypeText = string.Empty;
    public string RewardHintText = string.Empty;
}

public static class GoldenPathContentRegistry
{
    private const string ChainResourceFolder = "Content/GoldenPathChains";
    private const string RouteMeaningResourceFolder = "Content/GoldenPathRouteMeanings";
    private const string OutcomeMeaningResourceFolder = "Content/GoldenPathOutcomeMeanings";
    private const string CityDecisionMeaningResourceFolder = "Content/GoldenPathCityDecisionMeanings";
    private const string EncounterProfileResourceFolder = "Content/GoldenPathEncounterProfiles";
    private const string BattleSetupResourceFolder = "Content/GoldenPathBattleSetups";
    private static readonly Dictionary<string, GoldenPathChainDefinition> ChainsByKey = new Dictionary<string, GoldenPathChainDefinition>(StringComparer.Ordinal);
    private static readonly Dictionary<string, GoldenPathChainDefinition> ChainsByRouteKey = new Dictionary<string, GoldenPathChainDefinition>(StringComparer.Ordinal);
    private static readonly Dictionary<string, GoldenPathRouteMeaningDefinition> RouteMeaningsById = new Dictionary<string, GoldenPathRouteMeaningDefinition>(StringComparer.Ordinal);
    private static readonly Dictionary<string, GoldenPathOutcomeMeaningDefinition> OutcomeMeaningsById = new Dictionary<string, GoldenPathOutcomeMeaningDefinition>(StringComparer.Ordinal);
    private static readonly Dictionary<string, GoldenPathCityDecisionMeaningDefinition> CityDecisionMeaningsById = new Dictionary<string, GoldenPathCityDecisionMeaningDefinition>(StringComparer.Ordinal);
    private static readonly Dictionary<string, GoldenPathEncounterProfileDefinition> EncounterProfilesById = new Dictionary<string, GoldenPathEncounterProfileDefinition>(StringComparer.Ordinal);
    private static readonly Dictionary<string, GoldenPathBattleSetupDefinition> BattleSetupsById = new Dictionary<string, GoldenPathBattleSetupDefinition>(StringComparer.Ordinal);
    private static bool _isLoaded;

    public static bool TryGetChain(string cityId, string dungeonId, out GoldenPathChainDefinition definition)
    {
        EnsureLoaded();
        return ChainsByKey.TryGetValue(BuildKey(cityId, dungeonId), out definition);
    }

    public static bool TryGetChainForRoute(string cityId, string dungeonId, string routeId, out GoldenPathChainDefinition definition)
    {
        EnsureLoaded();
        string normalizedRouteId = routeId ?? string.Empty;
        if (HasText(normalizedRouteId) &&
            ChainsByRouteKey.TryGetValue(BuildRouteKey(cityId, dungeonId, normalizedRouteId), out definition))
        {
            return true;
        }

        return ChainsByKey.TryGetValue(BuildKey(cityId, dungeonId), out definition);
    }

    public static bool TryGetCanonicalRoute(string cityId, string dungeonId, string routeId, out GoldenPathChainDefinition chainDefinition, out GoldenPathRouteDefinition routeDefinition)
    {
        routeDefinition = null;
        if (!TryGetChainForRoute(cityId, dungeonId, routeId, out chainDefinition) ||
            chainDefinition == null ||
            chainDefinition.CanonicalRoute == null ||
            !HasText(chainDefinition.CanonicalRoute.RouteId) ||
            !string.Equals(chainDefinition.CanonicalRoute.RouteId, routeId, StringComparison.Ordinal))
        {
            return false;
        }

        routeDefinition = chainDefinition.CanonicalRoute;
        return true;
    }

    public static string BuildContentSourceLabel(string cityId, string dungeonId, string routeId)
    {
        if (TryGetCanonicalRoute(cityId, dungeonId, routeId, out GoldenPathChainDefinition chainDefinition, out GoldenPathRouteDefinition routeDefinition))
        {
            return "data:" + SafeText(chainDefinition.ChainId) + "/" + SafeText(routeDefinition.RouteId);
        }

        return "fallback:hardcoded";
    }

    public static bool TryGetRouteMeaning(string routeMeaningId, out GoldenPathRouteMeaningDefinition definition)
    {
        EnsureLoaded();
        return RouteMeaningsById.TryGetValue(routeMeaningId ?? string.Empty, out definition);
    }

    public static bool TryGetOutcomeMeaning(string outcomeMeaningId, out GoldenPathOutcomeMeaningDefinition definition)
    {
        EnsureLoaded();
        return OutcomeMeaningsById.TryGetValue(outcomeMeaningId ?? string.Empty, out definition);
    }

    public static bool TryGetCityDecisionMeaning(string cityDecisionMeaningId, out GoldenPathCityDecisionMeaningDefinition definition)
    {
        EnsureLoaded();
        return CityDecisionMeaningsById.TryGetValue(cityDecisionMeaningId ?? string.Empty, out definition);
    }

    public static bool TryGetOutcomeMeaningForChain(
        string cityId,
        string dungeonId,
        out GoldenPathChainDefinition chainDefinition,
        out GoldenPathOutcomeMeaningDefinition definition)
    {
        definition = null;
        if (!TryGetChain(cityId, dungeonId, out chainDefinition) ||
            chainDefinition == null ||
            !HasText(chainDefinition.OutcomeMeaningId))
        {
            return false;
        }

        return TryGetOutcomeMeaning(chainDefinition.OutcomeMeaningId, out definition);
    }

    public static bool TryGetOutcomeMeaningForChain(
        string cityId,
        string dungeonId,
        string routeId,
        out GoldenPathChainDefinition chainDefinition,
        out GoldenPathOutcomeMeaningDefinition definition)
    {
        definition = null;
        if (!TryGetChainForRoute(cityId, dungeonId, routeId, out chainDefinition) ||
            chainDefinition == null ||
            !HasText(chainDefinition.OutcomeMeaningId))
        {
            return false;
        }

        return TryGetOutcomeMeaning(chainDefinition.OutcomeMeaningId, out definition);
    }

    public static bool TryGetCityDecisionMeaningForChain(
        string cityId,
        string dungeonId,
        out GoldenPathChainDefinition chainDefinition,
        out GoldenPathCityDecisionMeaningDefinition definition)
    {
        definition = null;
        if (!TryGetChain(cityId, dungeonId, out chainDefinition) ||
            chainDefinition == null ||
            !HasText(chainDefinition.CityDecisionMeaningId))
        {
            return false;
        }

        return TryGetCityDecisionMeaning(chainDefinition.CityDecisionMeaningId, out definition);
    }

    public static bool TryGetEncounterProfile(string encounterProfileId, out GoldenPathEncounterProfileDefinition definition)
    {
        EnsureLoaded();
        return EncounterProfilesById.TryGetValue(encounterProfileId ?? string.Empty, out definition);
    }

    public static bool TryGetBattleSetup(string battleSetupId, out GoldenPathBattleSetupDefinition definition)
    {
        EnsureLoaded();
        return BattleSetupsById.TryGetValue(battleSetupId ?? string.Empty, out definition);
    }

    public static bool TryGetCanonicalEncounterRoom(
        string cityId,
        string dungeonId,
        string routeId,
        string encounterId,
        out GoldenPathChainDefinition chainDefinition,
        out GoldenPathRouteDefinition routeDefinition,
        out GoldenPathRoomDefinition roomDefinition)
    {
        roomDefinition = null;
        if (!TryGetCanonicalRoute(cityId, dungeonId, routeId, out chainDefinition, out routeDefinition) ||
            routeDefinition == null ||
            routeDefinition.Rooms == null ||
            !HasText(encounterId))
        {
            return false;
        }

        for (int i = 0; i < routeDefinition.Rooms.Length; i++)
        {
            GoldenPathRoomDefinition candidate = routeDefinition.Rooms[i];
            if (candidate == null || !string.Equals(candidate.EncounterId, encounterId, StringComparison.Ordinal))
            {
                continue;
            }

            roomDefinition = candidate;
            return true;
        }

        return false;
    }

    public static bool TryGetRepresentativeEncounterRoom(
        string cityId,
        string dungeonId,
        string routeId,
        out GoldenPathChainDefinition chainDefinition,
        out GoldenPathRouteDefinition routeDefinition,
        out GoldenPathRoomDefinition roomDefinition)
    {
        roomDefinition = null;
        if (!TryGetCanonicalRoute(cityId, dungeonId, routeId, out chainDefinition, out routeDefinition) ||
            routeDefinition == null ||
            !HasText(routeDefinition.RepresentativeEncounterId))
        {
            return false;
        }

        return TryGetCanonicalEncounterRoom(cityId, dungeonId, routeId, routeDefinition.RepresentativeEncounterId, out chainDefinition, out routeDefinition, out roomDefinition);
    }

    private static void EnsureLoaded()
    {
        if (_isLoaded)
        {
            return;
        }

        _isLoaded = true;
        ChainsByKey.Clear();
        ChainsByRouteKey.Clear();
        RouteMeaningsById.Clear();
        OutcomeMeaningsById.Clear();
        CityDecisionMeaningsById.Clear();
        EncounterProfilesById.Clear();
        BattleSetupsById.Clear();

        TextAsset[] chainAssets = Resources.LoadAll<TextAsset>(ChainResourceFolder);
        for (int i = 0; i < chainAssets.Length; i++)
        {
            TextAsset asset = chainAssets[i];
            if (asset == null || string.IsNullOrWhiteSpace(asset.text))
            {
                continue;
            }

            GoldenPathChainDefinition definition = JsonUtility.FromJson<GoldenPathChainDefinition>(asset.text);
            if (definition == null || !HasText(definition.CityId) || !HasText(definition.DungeonId))
            {
                continue;
            }

            string chainKey = BuildKey(definition.CityId, definition.DungeonId);
            if (definition.PrimaryCityDungeonDefinition || !ChainsByKey.ContainsKey(chainKey))
            {
                ChainsByKey[chainKey] = definition;
            }

            if (definition.CanonicalRoute != null && HasText(definition.CanonicalRoute.RouteId))
            {
                ChainsByRouteKey[BuildRouteKey(definition.CityId, definition.DungeonId, definition.CanonicalRoute.RouteId)] = definition;
            }
        }

        TextAsset[] routeMeaningAssets = Resources.LoadAll<TextAsset>(RouteMeaningResourceFolder);
        for (int i = 0; i < routeMeaningAssets.Length; i++)
        {
            TextAsset asset = routeMeaningAssets[i];
            if (asset == null || string.IsNullOrWhiteSpace(asset.text))
            {
                continue;
            }

            GoldenPathRouteMeaningDefinition definition = JsonUtility.FromJson<GoldenPathRouteMeaningDefinition>(asset.text);
            if (definition == null || !HasText(definition.RouteMeaningId))
            {
                continue;
            }

            RouteMeaningsById[definition.RouteMeaningId] = definition;
        }

        TextAsset[] outcomeMeaningAssets = Resources.LoadAll<TextAsset>(OutcomeMeaningResourceFolder);
        for (int i = 0; i < outcomeMeaningAssets.Length; i++)
        {
            TextAsset asset = outcomeMeaningAssets[i];
            if (asset == null || string.IsNullOrWhiteSpace(asset.text))
            {
                continue;
            }

            GoldenPathOutcomeMeaningDefinition definition = JsonUtility.FromJson<GoldenPathOutcomeMeaningDefinition>(asset.text);
            if (definition == null || !HasText(definition.OutcomeMeaningId))
            {
                continue;
            }

            OutcomeMeaningsById[definition.OutcomeMeaningId] = definition;
        }

        TextAsset[] cityDecisionMeaningAssets = Resources.LoadAll<TextAsset>(CityDecisionMeaningResourceFolder);
        for (int i = 0; i < cityDecisionMeaningAssets.Length; i++)
        {
            TextAsset asset = cityDecisionMeaningAssets[i];
            if (asset == null || string.IsNullOrWhiteSpace(asset.text))
            {
                continue;
            }

            GoldenPathCityDecisionMeaningDefinition definition = JsonUtility.FromJson<GoldenPathCityDecisionMeaningDefinition>(asset.text);
            if (definition == null || !HasText(definition.CityDecisionMeaningId))
            {
                continue;
            }

            CityDecisionMeaningsById[definition.CityDecisionMeaningId] = definition;
        }

        TextAsset[] encounterProfileAssets = Resources.LoadAll<TextAsset>(EncounterProfileResourceFolder);
        for (int i = 0; i < encounterProfileAssets.Length; i++)
        {
            TextAsset asset = encounterProfileAssets[i];
            if (asset == null || string.IsNullOrWhiteSpace(asset.text))
            {
                continue;
            }

            GoldenPathEncounterProfileDefinition definition = JsonUtility.FromJson<GoldenPathEncounterProfileDefinition>(asset.text);
            if (definition == null || !HasText(definition.EncounterProfileId))
            {
                continue;
            }

            EncounterProfilesById[definition.EncounterProfileId] = definition;
        }

        TextAsset[] battleSetupAssets = Resources.LoadAll<TextAsset>(BattleSetupResourceFolder);
        for (int i = 0; i < battleSetupAssets.Length; i++)
        {
            TextAsset asset = battleSetupAssets[i];
            if (asset == null || string.IsNullOrWhiteSpace(asset.text))
            {
                continue;
            }

            GoldenPathBattleSetupDefinition definition = JsonUtility.FromJson<GoldenPathBattleSetupDefinition>(asset.text);
            if (definition == null || !HasText(definition.BattleSetupId))
            {
                continue;
            }

            BattleSetupsById[definition.BattleSetupId] = definition;
        }
    }

    private static string BuildKey(string cityId, string dungeonId)
    {
        return (cityId ?? string.Empty) + "::" + (dungeonId ?? string.Empty);
    }

    private static string BuildRouteKey(string cityId, string dungeonId, string routeId)
    {
        return BuildKey(cityId, dungeonId) + "::" + (routeId ?? string.Empty);
    }

    private static bool HasText(string value)
    {
        return !string.IsNullOrEmpty(value) && value != "None";
    }

    private static string SafeText(string value)
    {
        return string.IsNullOrEmpty(value) ? "unknown" : value;
    }
}
