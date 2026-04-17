using System;
using System.Collections.Generic;

public static class CityDecisionModelBuilder
{
    public static CityDecisionReadModel Build(WorldBoardReadModel board, CityStatusReadModel city)
    {
        CityDecisionReadModel model = new CityDecisionReadModel();
        if (city == null)
        {
            return model;
        }

        model.CityId = city.CityId ?? string.Empty;
        model.CityDisplayName = HasText(city.DisplayName) ? city.DisplayName : (HasText(city.CityId) ? city.CityId : "None");
        model.LinkedDungeonId = city.LinkedDungeonId ?? string.Empty;
        model.LinkedDungeonDisplayName = HasText(city.LinkedDungeonDisplayName) ? city.LinkedDungeonDisplayName : "None";
        model.NeedPressureStateId = HasText(city.NeedPressureStateId) ? city.NeedPressureStateId : "None";
        model.DispatchReadinessStateId = HasText(city.DispatchReadinessStateId) ? city.DispatchReadinessStateId : "None";
        model.DispatchPolicyStateId = HasText(city.DispatchPolicyStateId) ? city.DispatchPolicyStateId : "None";
        model.DispatchRecoveryDaysRemaining = city.DispatchRecoveryDaysRemaining;
        model.ActiveExpeditionCount = city.ActiveExpeditionCount;
        model.IdlePartyCount = city.IdlePartyCount;

        DungeonStatusReadModel linkedDungeon = FindDungeon(board, city.LinkedDungeonId);
        RoadStatusReadModel constrainedRoad = FindMostConstrainedRoad(board, city.CityId);
        GoldenPathChainDefinition contentDefinition = ResolveContentDefinition(model.CityId, model.LinkedDungeonId);
        GoldenPathCityDecisionMeaningDefinition cityDecisionMeaning = ResolveCityDecisionMeaning(contentDefinition);
        CityBottleneckSignal[] bottlenecks = BuildBottlenecks(city, constrainedRoad, contentDefinition, cityDecisionMeaning);
        CityOpportunitySignal[] opportunities = BuildOpportunities(city, linkedDungeon, cityDecisionMeaning);
        RecentImpactSummary[] impacts = BuildRecentImpacts(city, bottlenecks, opportunities);
        CityActionRecommendation[] recommendations = BuildRecommendations(city, constrainedRoad, bottlenecks, opportunities, impacts, cityDecisionMeaning);

        model.Bottlenecks = bottlenecks;
        model.Opportunities = opportunities;
        model.RecentImpacts = impacts;
        model.RecommendedActions = recommendations;
        model.WhyCityMattersText = BuildWhyCityMatters(city, bottlenecks, opportunities, impacts, cityDecisionMeaning);
        return model;
    }

    private static CityBottleneckSignal[] BuildBottlenecks(
        CityStatusReadModel city,
        RoadStatusReadModel constrainedRoad,
        GoldenPathChainDefinition contentDefinition,
        GoldenPathCityDecisionMeaningDefinition cityDecisionMeaning)
    {
        List<CityBottleneckSignal> signals = new List<CityBottleneckSignal>();
        ResourceAmountReadModel topShortage = GetPrimaryResource(city != null ? city.TopShortages : null);
        string sharedBottleneckSummaryText = ResolveBottleneckSummaryText(contentDefinition, cityDecisionMeaning);
        bool sharedBottleneckSummaryAssigned = false;

        if (topShortage != null && HasText(topShortage.ResourceId) && topShortage.Amount > 0)
        {
            signals.Add(new CityBottleneckSignal
            {
                Type = CityBottleneckSignalType.ResourceShortage,
                ResourceId = topShortage.ResourceId,
                ReasonCode = topShortage.IsHighPriority ? "high_priority_shortage" : "resource_shortage",
                AffectedActionArea = "Supply",
                Severity = 360 + topShortage.Amount + (topShortage.IsHighPriority ? 40 : 0),
                IsBlocking = true,
                SummaryText = UseSharedCityMeaningTextOnce(
                    sharedBottleneckSummaryText,
                    ref sharedBottleneckSummaryAssigned,
                    topShortage.ResourceId + " shortage is the main blocker (" + topShortage.Amount + " unmet).")
            });
        }

        if (city != null && city.LastDayProcessingBlocked > 0)
        {
            signals.Add(new CityBottleneckSignal
            {
                Type = CityBottleneckSignalType.ProcessingBlocker,
                ResourceId = topShortage != null ? topShortage.ResourceId : string.Empty,
                ReasonCode = "processing_blocked",
                AffectedActionArea = "Processing",
                Severity = 290 + city.LastDayProcessingBlocked,
                IsBlocking = true,
                SummaryText = UseSharedCityMeaningTextOnce(
                    sharedBottleneckSummaryText,
                    ref sharedBottleneckSummaryAssigned,
                    "Processing stalled " + city.LastDayProcessingBlocked + " time(s) because inputs did not line up.")
            });
        }

        if (constrainedRoad != null && (constrainedRoad.IsSaturated || constrainedRoad.UtilizationPercent >= 75))
        {
            string relatedName = GetRelatedRoadName(constrainedRoad, city != null ? city.CityId : string.Empty);
            signals.Add(new CityBottleneckSignal
            {
                Type = CityBottleneckSignalType.RouteCapacity,
                RelatedEntityId = constrainedRoad.RoadId,
                RelatedEntityDisplayName = constrainedRoad.FromEntityDisplayName + " <-> " + constrainedRoad.ToEntityDisplayName,
                ReasonCode = constrainedRoad.IsSaturated ? "route_saturated" : "route_constrained",
                AffectedActionArea = "Routing",
                Severity = 230 + constrainedRoad.UtilizationPercent + (constrainedRoad.IsSaturated ? 35 : 0),
                IsBlocking = constrainedRoad.IsSaturated,
                SummaryText = relatedName + " route ran at " + constrainedRoad.UtilizationPercent + "% capacity."
            });
        }

        if (city != null && HasText(city.DispatchReadinessStateId) && city.DispatchReadinessStateId != "Ready")
        {
            int severity = city.DispatchReadinessStateId == "Strained" ? 325 : 255;
            string recoveryText = city.DispatchRecoveryDaysRemaining > 0
                ? " Wait " + city.DispatchRecoveryDaysRemaining + " day(s) for full recovery."
                : string.Empty;
            signals.Add(new CityBottleneckSignal
            {
                Type = CityBottleneckSignalType.DispatchRecovery,
                ReasonCode = city.DispatchReadinessStateId == "Strained" ? "dispatch_strained" : "dispatch_recovering",
                AffectedActionArea = "Dispatch",
                Severity = severity + city.DispatchRecoveryDaysRemaining,
                IsBlocking = city.DispatchReadinessStateId == "Strained",
                SummaryText = "Dispatch readiness is " + city.DispatchReadinessStateId + "." + recoveryText
            });
        }

        if (city != null && city.IdlePartyCount < 1 && city.ActiveExpeditionCount < 1)
        {
            signals.Add(new CityBottleneckSignal
            {
                Type = CityBottleneckSignalType.ExpeditionWindow,
                ReasonCode = "no_idle_party",
                AffectedActionArea = "Recruitment",
                Severity = 270,
                IsBlocking = true,
                SummaryText = "No idle party is available, so the city cannot act on its dungeon window yet."
            });
        }
        else if (city != null && HasText(city.LinkedDungeonId) && city.AvailableContractSlots < 1)
        {
            signals.Add(new CityBottleneckSignal
            {
                Type = CityBottleneckSignalType.ExpeditionWindow,
                RelatedEntityId = city.LinkedDungeonId,
                RelatedEntityDisplayName = city.LinkedDungeonDisplayName,
                ReasonCode = "no_contract_slot",
                AffectedActionArea = "Dispatch",
                Severity = 245,
                IsBlocking = true,
                SummaryText = "The linked dungeon contract lane is full, so a new run must wait."
            });
        }

        signals.Sort((left, right) =>
        {
            int severityCompare = right.Severity.CompareTo(left.Severity);
            return severityCompare != 0 ? severityCompare : string.CompareOrdinal(left.ReasonCode, right.ReasonCode);
        });

        if (signals.Count > 3)
        {
            signals.RemoveRange(3, signals.Count - 3);
        }

        return signals.ToArray();
    }

    private static CityOpportunitySignal[] BuildOpportunities(
        CityStatusReadModel city,
        DungeonStatusReadModel linkedDungeon,
        GoldenPathCityDecisionMeaningDefinition cityDecisionMeaning)
    {
        List<CityOpportunitySignal> signals = new List<CityOpportunitySignal>();
        if (city == null)
        {
            return signals.ToArray();
        }

        string rewardResourceId = GetBestRewardResource(city, linkedDungeon);
        string gatingText = BuildOpportunityGateText(city, linkedDungeon);
        bool isReady = linkedDungeon != null &&
                       city.IdlePartyCount > 0 &&
                       city.AvailableContractSlots > 0 &&
                       city.DispatchReadinessStateId != "Strained";
        int readinessBias = isReady ? 40 : 0;
        int shortageBias = HasText(FindMatchingNeedResource(city, rewardResourceId)) ? 30 : 0;
        GoldenPathChainDefinition primaryContentDefinition = linkedDungeon != null
            ? ResolveContentDefinition(city.CityId, linkedDungeon.DungeonId)
            : null;
        GoldenPathRouteDefinition primaryRouteDefinition = primaryContentDefinition != null ? primaryContentDefinition.CanonicalRoute : null;

        if (linkedDungeon != null)
        {
            signals.Add(new CityOpportunitySignal
            {
                Type = CityOpportunitySignalType.LinkedDungeonExpedition,
                DungeonId = linkedDungeon.DungeonId,
                DungeonDisplayName = linkedDungeon.DisplayName,
                RouteId = primaryRouteDefinition != null ? primaryRouteDefinition.RouteId ?? string.Empty : string.Empty,
                RouteLabel = primaryRouteDefinition != null && HasText(primaryRouteDefinition.RouteLabel)
                    ? primaryRouteDefinition.RouteLabel
                    : "None",
                RelatedBottleneckResourceId = FindMatchingNeedResource(city, rewardResourceId),
                ExpectedRewardResourceId = rewardResourceId,
                ContentSourceLabel = BuildOpportunityContentSourceLabel(
                    city.CityId,
                    linkedDungeon.DungeonId,
                    primaryRouteDefinition != null ? primaryRouteDefinition.RouteId : string.Empty),
                ReadinessStateId = HasText(city.DispatchReadinessStateId) ? city.DispatchReadinessStateId : "None",
                GatingConstraintText = gatingText,
                Priority = 280 + readinessBias + shortageBias,
                IsReady = isReady,
                SummaryText = BuildOpportunitySummary(city, linkedDungeon, rewardResourceId, gatingText, isReady),
                WhyItMattersText = BuildOpportunityReason(city, linkedDungeon, rewardResourceId, cityDecisionMeaning)
            });

            AppendSurfacedRouteVariantOpportunities(
                signals,
                city,
                linkedDungeon,
                rewardResourceId,
                gatingText,
                isReady,
                readinessBias,
                shortageBias,
                primaryContentDefinition);
        }

        if (city.ActiveExpedition != null && HasText(city.ActiveExpedition.TargetDungeonId))
        {
            signals.Add(new CityOpportunitySignal
            {
                Type = CityOpportunitySignalType.ActiveExpeditionFollowUp,
                DungeonId = city.ActiveExpedition.TargetDungeonId,
                DungeonDisplayName = HasText(city.ActiveExpedition.TargetDungeonDisplayName) ? city.ActiveExpedition.TargetDungeonDisplayName : city.ActiveExpedition.TargetDungeonId,
                RelatedBottleneckResourceId = rewardResourceId,
                ExpectedRewardResourceId = rewardResourceId,
                ReadinessStateId = "InFlight",
                GatingConstraintText = city.ActiveExpedition.DaysRemaining > 0
                    ? "Active expedition returns in " + city.ActiveExpedition.DaysRemaining + " day(s)."
                    : "Active expedition return is pending.",
                Priority = 210,
                IsReady = false,
                SummaryText = "An expedition is already in flight to " + city.ActiveExpedition.TargetDungeonDisplayName + ".",
                WhyItMattersText = "The next city decision depends on the current expedition return and the stock it brings back."
            });
        }

        signals.Sort((left, right) =>
        {
            int priorityCompare = right.Priority.CompareTo(left.Priority);
            return priorityCompare != 0 ? priorityCompare : string.CompareOrdinal(left.DungeonId, right.DungeonId);
        });

        if (signals.Count > 2)
        {
            signals.RemoveRange(2, signals.Count - 2);
        }

        return signals.ToArray();
    }

    private static void AppendSurfacedRouteVariantOpportunities(
        List<CityOpportunitySignal> signals,
        CityStatusReadModel city,
        DungeonStatusReadModel linkedDungeon,
        string rewardResourceId,
        string gatingText,
        bool isReady,
        int readinessBias,
        int shortageBias,
        GoldenPathChainDefinition primaryContentDefinition)
    {
        if (signals == null || city == null || linkedDungeon == null)
        {
            return;
        }

        TryAddSurfacedRouteVariantOpportunity(
            signals,
            city,
            linkedDungeon,
            rewardResourceId,
            gatingText,
            isReady,
            readinessBias,
            shortageBias,
            primaryContentDefinition,
            "safe");
        TryAddSurfacedRouteVariantOpportunity(
            signals,
            city,
            linkedDungeon,
            rewardResourceId,
            gatingText,
            isReady,
            readinessBias,
            shortageBias,
            primaryContentDefinition,
            "risky");
    }

    private static void TryAddSurfacedRouteVariantOpportunity(
        List<CityOpportunitySignal> signals,
        CityStatusReadModel city,
        DungeonStatusReadModel linkedDungeon,
        string rewardResourceId,
        string gatingText,
        bool isReady,
        int readinessBias,
        int shortageBias,
        GoldenPathChainDefinition primaryContentDefinition,
        string routeId)
    {
        if (signals == null ||
            city == null ||
            linkedDungeon == null ||
            !HasText(routeId) ||
            !GoldenPathContentRegistry.TryGetCanonicalRoute(city.CityId, linkedDungeon.DungeonId, routeId, out GoldenPathChainDefinition chainDefinition, out GoldenPathRouteDefinition routeDefinition) ||
            chainDefinition == null ||
            routeDefinition == null ||
            !chainDefinition.SurfaceAsOpportunityVariant)
        {
            return;
        }

        if (primaryContentDefinition != null &&
            string.Equals(primaryContentDefinition.ChainId, chainDefinition.ChainId, StringComparison.Ordinal))
        {
            return;
        }

        GoldenPathCityDecisionMeaningDefinition cityDecisionMeaning = ResolveCityDecisionMeaning(chainDefinition);
        GoldenPathRouteMeaningDefinition routeMeaning = ResolveRouteMeaning(routeDefinition);
        string effectiveRewardResourceId = HasText(chainDefinition.RewardResourceId)
            ? chainDefinition.RewardResourceId
            : rewardResourceId;

        signals.Add(new CityOpportunitySignal
        {
            Type = CityOpportunitySignalType.LinkedDungeonExpedition,
            DungeonId = linkedDungeon.DungeonId,
            DungeonDisplayName = linkedDungeon.DisplayName,
            RouteId = routeDefinition.RouteId ?? string.Empty,
            RouteLabel = HasText(routeDefinition.RouteLabel) ? routeDefinition.RouteLabel : "None",
            RelatedBottleneckResourceId = FindMatchingNeedResource(city, effectiveRewardResourceId),
            ExpectedRewardResourceId = effectiveRewardResourceId,
            ContentSourceLabel = BuildOpportunityContentSourceLabel(city.CityId, linkedDungeon.DungeonId, routeDefinition.RouteId),
            ReadinessStateId = HasText(city.DispatchReadinessStateId) ? city.DispatchReadinessStateId : "None",
            GatingConstraintText = gatingText,
            Priority = 250 + readinessBias + shortageBias,
            IsReady = isReady,
            SummaryText = BuildRouteVariantOpportunitySummary(linkedDungeon, routeDefinition, effectiveRewardResourceId, gatingText, isReady),
            WhyItMattersText = BuildRouteVariantOpportunityReason(city, linkedDungeon, effectiveRewardResourceId, cityDecisionMeaning, routeMeaning)
        });
    }

    private static RecentImpactSummary[] BuildRecentImpacts(
        CityStatusReadModel city,
        CityBottleneckSignal[] bottlenecks,
        CityOpportunitySignal[] opportunities)
    {
        List<RecentImpactSummary> impacts = new List<RecentImpactSummary>();
        if (city == null)
        {
            return impacts.ToArray();
        }

        bool hasLatestResult = city.LatestResult != null && city.LatestResult.HasResult;
        string topShortageResourceId = GetPrimaryResourceId(city.TopShortages);
        if (hasLatestResult)
        {
            string summaryText = BuildRecentImpactSummaryText(city);
            string decisionHintText = BuildImpactDecisionHint(city, topShortageResourceId);
            string legacyImpactDetail = BuildRecentImpactLegacyDetailText(city, summaryText);
            if (HasText(legacyImpactDetail))
            {
                decisionHintText += " " + legacyImpactDetail;
            }

            impacts.Add(new RecentImpactSummary
            {
                Type = BuildRecentImpactType(city, topShortageResourceId),
                ResultStateKey = city.LatestResult.ResultStateKey ?? string.Empty,
                ResourceId = city.LatestResult.RewardResourceId ?? string.Empty,
                RelatedDungeonId = city.LatestResult.TargetDungeonId ?? string.Empty,
                Priority = city.LatestResult.Success ? 260 + city.LatestResult.ReturnedLootAmount : 240,
                ShouldAffectNextDecision = true,
                SummaryText = summaryText,
                NextDecisionHintText = decisionHintText
            });
        }

        if (HasText(city.LastNeedPressureChangeText))
        {
            impacts.Add(new RecentImpactSummary
            {
                Type = CityRecentImpactType.DispatchShift,
                ResultStateKey = hasLatestResult && city.LatestResult != null ? city.LatestResult.ResultStateKey : string.Empty,
                ResourceId = topShortageResourceId,
                RelatedDungeonId = city.LinkedDungeonId,
                Priority = 220,
                ShouldAffectNextDecision = true,
                SummaryText = "Need pressure changed: " + city.LastNeedPressureChangeText,
                NextDecisionHintText = city.NeedPressureStateId == "Urgent"
                    ? "Pressure is still urgent, so the next expedition remains part of the fix."
                    : "Pressure eased, so you can stabilize or take a safer follow-up."
            });
        }

        if (HasText(city.LastDispatchReadinessChangeText))
        {
            impacts.Add(new RecentImpactSummary
            {
                Type = CityRecentImpactType.DispatchShift,
                ResultStateKey = hasLatestResult && city.LatestResult != null ? city.LatestResult.ResultStateKey : string.Empty,
                ResourceId = topShortageResourceId,
                RelatedDungeonId = city.LinkedDungeonId,
                Priority = 210,
                ShouldAffectNextDecision = true,
                SummaryText = "Dispatch status changed: " + city.LastDispatchReadinessChangeText,
                NextDecisionHintText = city.DispatchReadinessStateId == "Ready"
                    ? "The city is ready to convert the next opportunity immediately."
                    : "Dispatch timing should react to the current readiness state."
            });
        }

        impacts.Sort((left, right) =>
        {
            int priorityCompare = right.Priority.CompareTo(left.Priority);
            return priorityCompare != 0 ? priorityCompare : string.CompareOrdinal(left.ResultStateKey, right.ResultStateKey);
        });

        if (impacts.Count > 2)
        {
            impacts.RemoveRange(2, impacts.Count - 2);
        }

        return impacts.ToArray();
    }

    private static CityActionRecommendation[] BuildRecommendations(
        CityStatusReadModel city,
        RoadStatusReadModel constrainedRoad,
        CityBottleneckSignal[] bottlenecks,
        CityOpportunitySignal[] opportunities,
        RecentImpactSummary[] impacts,
        GoldenPathCityDecisionMeaningDefinition cityDecisionMeaning)
    {
        List<CityActionRecommendation> recommendations = new List<CityActionRecommendation>();
        if (city == null)
        {
            return recommendations.ToArray();
        }

        CityBottleneckSignal topBottleneck = GetFirstItem(bottlenecks);
        CityOpportunitySignal topOpportunity = GetFirstItem(opportunities);
        RecentImpactSummary topImpact = GetFirstItem(impacts);

        bool shouldLaunchNow = topOpportunity != null &&
                               topOpportunity.IsReady &&
                               (city.DispatchReadinessStateId == "Ready" || city.NeedPressureStateId == "Urgent");
        if (shouldLaunchNow)
        {
            recommendations.Add(new CityActionRecommendation
            {
                ActionType = CityRecommendedActionType.LaunchLinkedDungeonExpedition,
                TargetCityId = city.CityId,
                TargetDungeonId = topOpportunity.DungeonId,
                RelatedResourceId = topOpportunity.ExpectedRewardResourceId,
                Priority = 340 + topOpportunity.Priority,
                IsAvailable = true,
                SummaryText = "Launch the linked dungeon expedition next.",
                ReasonText = BuildLaunchRecommendationReason(topOpportunity, topImpact, cityDecisionMeaning)
            });
        }

        if (city.IdlePartyCount < 1 && city.ActiveExpeditionCount < 1)
        {
            recommendations.Add(new CityActionRecommendation
            {
                ActionType = CityRecommendedActionType.RecruitParty,
                TargetCityId = city.CityId,
                TargetDungeonId = city.LinkedDungeonId,
                RelatedResourceId = topOpportunity != null ? topOpportunity.ExpectedRewardResourceId : GetPrimaryResourceId(city.TopShortages),
                Priority = 320,
                IsAvailable = true,
                SummaryText = "Recruit a party before solving the city's next bottleneck.",
                ReasonText = BuildRecruitRecommendationReason(city, topOpportunity, topImpact, cityDecisionMeaning)
            });
        }

        if (city.DispatchReadinessStateId == "Strained" || (city.DispatchReadinessStateId == "Recovering" && city.NeedPressureStateId != "Urgent"))
        {
            recommendations.Add(new CityActionRecommendation
            {
                ActionType = CityRecommendedActionType.WaitForRecovery,
                TargetCityId = city.CityId,
                TargetDungeonId = city.LinkedDungeonId,
                RelatedResourceId = topOpportunity != null ? topOpportunity.ExpectedRewardResourceId : string.Empty,
                Priority = city.DispatchReadinessStateId == "Strained" ? 330 : 290,
                IsAvailable = true,
                SummaryText = "Stabilize for 1 day before the next push.",
                ReasonText = BuildRecoveryRecommendationReason(city, topImpact, cityDecisionMeaning)
            });
        }

        if (constrainedRoad != null && (constrainedRoad.IsSaturated || constrainedRoad.UtilizationPercent >= 75))
        {
            recommendations.Add(new CityActionRecommendation
            {
                ActionType = CityRecommendedActionType.InspectSupplyRoute,
                TargetCityId = city.CityId,
                RelatedResourceId = topBottleneck != null ? topBottleneck.ResourceId : string.Empty,
                Priority = 230 + constrainedRoad.UtilizationPercent,
                IsAvailable = true,
                SummaryText = "Inspect route pressure before forcing another shortage response.",
                ReasonText = CombineSentences(
                    topImpact != null ? topImpact.NextDecisionHintText : string.Empty,
                    constrainedRoad.FromEntityDisplayName + " <-> " + constrainedRoad.ToEntityDisplayName +
                    " is running at " + constrainedRoad.UtilizationPercent + "% utilization.")
            });
        }

        if (city.ActiveExpeditionCount > 0 || (topImpact != null && topImpact.ShouldAffectNextDecision && city.NeedPressureStateId != "Urgent"))
        {
            recommendations.Add(new CityActionRecommendation
            {
                ActionType = CityRecommendedActionType.AdvanceWorldDay,
                TargetCityId = city.CityId,
                TargetDungeonId = city.LinkedDungeonId,
                RelatedResourceId = topImpact != null ? topImpact.ResourceId : string.Empty,
                Priority = city.ActiveExpeditionCount > 0 ? 250 : 205,
                IsAvailable = true,
                SummaryText = "Advance 1 day to resolve the current city state change.",
                ReasonText = BuildAdvanceWorldDayReason(city, topImpact, cityDecisionMeaning)
            });
        }

        if (recommendations.Count == 0)
        {
            recommendations.Add(new CityActionRecommendation
            {
                ActionType = CityRecommendedActionType.ReviewRecentImpact,
                TargetCityId = city.CityId,
                TargetDungeonId = city.LinkedDungeonId,
                RelatedResourceId = topOpportunity != null ? topOpportunity.ExpectedRewardResourceId : string.Empty,
                Priority = 180,
                IsAvailable = true,
                SummaryText = "Review the city's current pressure before changing the loop.",
                ReasonText = BuildReviewRecommendationReason(city, topBottleneck, topImpact, cityDecisionMeaning)
            });
        }

        recommendations.Sort((left, right) =>
        {
            int priorityCompare = right.Priority.CompareTo(left.Priority);
            return priorityCompare != 0 ? priorityCompare : string.CompareOrdinal(left.SummaryText, right.SummaryText);
        });

        if (recommendations.Count > 3)
        {
            recommendations.RemoveRange(3, recommendations.Count - 3);
        }

        return recommendations.ToArray();
    }

    private static string BuildWhyCityMatters(
        CityStatusReadModel city,
        CityBottleneckSignal[] bottlenecks,
        CityOpportunitySignal[] opportunities,
        RecentImpactSummary[] impacts,
        GoldenPathCityDecisionMeaningDefinition cityDecisionMeaning)
    {
        CityBottleneckSignal topBottleneck = GetFirstItem(bottlenecks);
        CityOpportunitySignal topOpportunity = GetFirstItem(opportunities);
        RecentImpactSummary topImpact = GetFirstItem(impacts);
        string sharedWhyCityMattersText = cityDecisionMeaning != null && HasText(cityDecisionMeaning.WhyCityMattersText)
            ? cityDecisionMeaning.WhyCityMattersText
            : string.Empty;
        if (topImpact != null && topImpact.ShouldAffectNextDecision)
        {
            string impactSummary = HasText(topImpact.SummaryText)
                ? TrimTrailingPeriod(topImpact.SummaryText)
                : "The latest expedition changed this city's state";
            string impactHint = HasText(topImpact.NextDecisionHintText)
                ? topImpact.NextDecisionHintText
                : "The next dispatch should react to that updated city state.";
            return CombineSentences(
                impactSummary + ", so the city decision surface should react to the updated result.",
                CombineSentences(sharedWhyCityMattersText, impactHint));
        }

        if (HasText(sharedWhyCityMattersText))
        {
            return sharedWhyCityMattersText;
        }

        if (topBottleneck != null &&
            topBottleneck.Type == CityBottleneckSignalType.ResourceShortage &&
            topOpportunity != null &&
            HasText(topOpportunity.ExpectedRewardResourceId) &&
            topOpportunity.ExpectedRewardResourceId == topBottleneck.ResourceId)
        {
            return topBottleneck.ResourceId + " is the current city bottleneck, and " +
                   topOpportunity.DungeonDisplayName + " is the clearest expedition answer.";
        }

        if (topBottleneck != null)
        {
            return city.DisplayName + " needs attention because " + TrimTrailingPeriod(topBottleneck.SummaryText) + ".";
        }

        if (topOpportunity != null)
        {
            return topOpportunity.WhyItMattersText;
        }

        return "This city is stable for now, but the next shortage or expedition return will set the next decision.";
    }

    private static DungeonStatusReadModel FindDungeon(WorldBoardReadModel board, string dungeonId)
    {
        DungeonStatusReadModel[] dungeons = board != null ? board.Dungeons : Array.Empty<DungeonStatusReadModel>();
        for (int i = 0; i < dungeons.Length; i++)
        {
            DungeonStatusReadModel dungeon = dungeons[i];
            if (dungeon != null && dungeon.DungeonId == dungeonId)
            {
                return dungeon;
            }
        }

        return null;
    }

    private static RoadStatusReadModel FindMostConstrainedRoad(WorldBoardReadModel board, string cityId)
    {
        RoadStatusReadModel[] roads = board != null ? board.Roads : Array.Empty<RoadStatusReadModel>();
        RoadStatusReadModel best = null;
        int bestScore = int.MinValue;
        for (int i = 0; i < roads.Length; i++)
        {
            RoadStatusReadModel road = roads[i];
            if (road == null || road.CapacityPerDay < 1)
            {
                continue;
            }

            if (road.FromEntityId != cityId && road.ToEntityId != cityId)
            {
                continue;
            }

            int score = road.UtilizationPercent + (road.IsSaturated ? 40 : 0);
            if (best == null || score > bestScore)
            {
                best = road;
                bestScore = score;
            }
        }

        return best;
    }

    private static string GetRelatedRoadName(RoadStatusReadModel road, string cityId)
    {
        if (road == null)
        {
            return "Connected";
        }

        if (road.FromEntityId == cityId && HasText(road.ToEntityDisplayName))
        {
            return road.ToEntityDisplayName;
        }

        if (road.ToEntityId == cityId && HasText(road.FromEntityDisplayName))
        {
            return road.FromEntityDisplayName;
        }

        return "Connected";
    }

    private static string GetBestRewardResource(CityStatusReadModel city, DungeonStatusReadModel linkedDungeon)
    {
        if (linkedDungeon == null)
        {
            return string.Empty;
        }

        string[] outputResourceIds = linkedDungeon.OutputResourceIds ?? Array.Empty<string>();
        for (int i = 0; i < outputResourceIds.Length; i++)
        {
            string resourceId = outputResourceIds[i];
            if (!HasText(resourceId))
            {
                continue;
            }

            if (HasText(FindMatchingNeedResource(city, resourceId)))
            {
                return resourceId;
            }
        }

        for (int i = 0; i < outputResourceIds.Length; i++)
        {
            if (HasText(outputResourceIds[i]))
            {
                return outputResourceIds[i];
            }
        }

        return string.Empty;
    }

    private static string FindMatchingNeedResource(CityStatusReadModel city, string resourceId)
    {
        if (city == null || !HasText(resourceId))
        {
            return string.Empty;
        }

        ResourceAmountReadModel[] shortages = city.TopShortages ?? Array.Empty<ResourceAmountReadModel>();
        for (int i = 0; i < shortages.Length; i++)
        {
            ResourceAmountReadModel shortage = shortages[i];
            if (shortage != null && shortage.ResourceId == resourceId)
            {
                return resourceId;
            }
        }

        string[] needs = city.NeedResourceIds ?? Array.Empty<string>();
        for (int i = 0; i < needs.Length; i++)
        {
            if (needs[i] == resourceId)
            {
                return resourceId;
            }
        }

        return string.Empty;
    }

    private static string BuildOpportunityGateText(CityStatusReadModel city, DungeonStatusReadModel linkedDungeon)
    {
        if (linkedDungeon == null)
        {
            return "No linked dungeon is available.";
        }

        if (city == null)
        {
            return "City status is unavailable.";
        }

        if (city.IdlePartyCount < 1)
        {
            return "Recruit a party first.";
        }

        if (city.AvailableContractSlots < 1)
        {
            return "Wait for a contract slot to open.";
        }

        if (city.DispatchReadinessStateId == "Strained")
        {
            return city.DispatchRecoveryDaysRemaining > 0
                ? "Let the city recover for " + city.DispatchRecoveryDaysRemaining + " day(s) first."
                : "Dispatch readiness is strained.";
        }

        return "Ready to launch.";
    }

    private static string BuildOpportunitySummary(
        CityStatusReadModel city,
        DungeonStatusReadModel linkedDungeon,
        string rewardResourceId,
        string gatingText,
        bool isReady)
    {
        string dungeonName = linkedDungeon != null && HasText(linkedDungeon.DisplayName)
            ? linkedDungeon.DisplayName
            : "the linked dungeon";
        if (isReady)
        {
            return "Best opportunity: dispatch to " + dungeonName +
                   (HasText(rewardResourceId) ? " for " + rewardResourceId + "." : ".");
        }

        return "Next opportunity: keep " + dungeonName +
               (HasText(rewardResourceId) ? " lined up for " + rewardResourceId + ", " : " ready, ") +
               TrimTrailingPeriod(gatingText) + ".";
    }

    private static string BuildOpportunityReason(
        CityStatusReadModel city,
        DungeonStatusReadModel linkedDungeon,
        string rewardResourceId,
        GoldenPathCityDecisionMeaningDefinition cityDecisionMeaning)
    {
        if (cityDecisionMeaning != null && HasText(cityDecisionMeaning.OpportunityReasonText))
        {
            return cityDecisionMeaning.OpportunityReasonText;
        }

        if (linkedDungeon == null)
        {
            return "No dungeon opportunity is connected to this city yet.";
        }

        string matchingNeed = FindMatchingNeedResource(city, rewardResourceId);
        if (HasText(matchingNeed))
        {
            return rewardResourceId + " from " + linkedDungeon.DisplayName + " directly supports the city's current shortage.";
        }

        if (city != null && city.LastDayProcessingBlocked > 0 && HasText(rewardResourceId))
        {
            return rewardResourceId + " from " + linkedDungeon.DisplayName + " helps reopen blocked processing lines.";
        }

        return linkedDungeon.DisplayName + " is the main expedition lever connected to this city right now.";
    }

    private static string BuildRouteVariantOpportunitySummary(
        DungeonStatusReadModel linkedDungeon,
        GoldenPathRouteDefinition routeDefinition,
        string rewardResourceId,
        string gatingText,
        bool isReady)
    {
        string dungeonName = linkedDungeon != null && HasText(linkedDungeon.DisplayName)
            ? linkedDungeon.DisplayName
            : "the linked dungeon";
        string routeName = routeDefinition != null && HasText(routeDefinition.RouteLabel)
            ? routeDefinition.RouteLabel
            : "the alternate route";

        if (isReady)
        {
            return "Alternative opportunity: dispatch to " + dungeonName + " via " + routeName +
                   (HasText(rewardResourceId) ? " for " + rewardResourceId + "." : ".");
        }

        return "Alternative opportunity: keep " + dungeonName + " via " + routeName +
               (HasText(rewardResourceId) ? " lined up for " + rewardResourceId + ", " : " ready, ") +
               TrimTrailingPeriod(gatingText) + ".";
    }

    private static string BuildRouteVariantOpportunityReason(
        CityStatusReadModel city,
        DungeonStatusReadModel linkedDungeon,
        string rewardResourceId,
        GoldenPathCityDecisionMeaningDefinition cityDecisionMeaning,
        GoldenPathRouteMeaningDefinition routeMeaning)
    {
        string sharedReason = cityDecisionMeaning != null && HasText(cityDecisionMeaning.OpportunityReasonText)
            ? cityDecisionMeaning.OpportunityReasonText
            : BuildOpportunityReason(city, linkedDungeon, rewardResourceId, null);
        string routeReason = routeMeaning != null && HasText(routeMeaning.ChooseWhenText)
            ? routeMeaning.ChooseWhenText
            : routeMeaning != null && HasText(routeMeaning.ExpectedNeedImpactText)
                ? routeMeaning.ExpectedNeedImpactText
                : routeMeaning != null && HasText(routeMeaning.RewardPreview)
                    ? routeMeaning.RewardPreview
                    : string.Empty;
        string followUpReason = routeMeaning != null && HasText(routeMeaning.FollowUpHintText)
            ? routeMeaning.FollowUpHintText
            : string.Empty;
        return CombineSentences(sharedReason, CombineSentences(routeReason, followUpReason));
    }

    private static string BuildLatestResultFallback(ExpeditionResultReadModel result)
    {
        if (result == null)
        {
            return "No recent expedition impact is available.";
        }

        string cityName = HasText(result.SourceCityDisplayName) ? result.SourceCityDisplayName : result.SourceCityId;
        string dungeonName = HasText(result.TargetDungeonDisplayName) ? result.TargetDungeonDisplayName : result.TargetDungeonId;
        if (HasText(cityName) && HasText(dungeonName))
        {
            return cityName + " returned from " + dungeonName + ".";
        }

        return "Recent expedition outcome applied to the city.";
    }

    private static string BuildRecentImpactSummaryText(CityStatusReadModel city)
    {
        if (city == null)
        {
            return "None";
        }

        ExpeditionResultReadModel latestResult = city.LatestResult;
        if (latestResult != null)
        {
            if (HasText(latestResult.WorldReturnSummaryText))
            {
                return latestResult.WorldReturnSummaryText;
            }

            if (HasText(latestResult.CityStatusChangeSummaryText))
            {
                return latestResult.CityStatusChangeSummaryText;
            }

            if (HasText(latestResult.SummaryText))
            {
                return latestResult.SummaryText;
            }
        }

        if (HasText(city.LastDispatchImpactText))
        {
            return city.LastDispatchImpactText;
        }

        return BuildLatestResultFallback(latestResult);
    }

    private static string BuildRecentImpactLegacyDetailText(CityStatusReadModel city, string summaryText)
    {
        if (city == null || !HasText(city.LastDispatchImpactText) || city.LastDispatchImpactText == summaryText)
        {
            return string.Empty;
        }

        return "World delta: " + city.LastDispatchImpactText;
    }

    private static string BuildImpactDecisionHint(CityStatusReadModel city, string topShortageResourceId)
    {
        if (city == null || city.LatestResult == null || !city.LatestResult.HasResult)
        {
            return "Use the updated city state before deciding the next dispatch.";
        }

        string missionRelevanceText = HasText(city.LatestResult.MissionRelevanceText)
            ? TrimTrailingPeriod(city.LatestResult.MissionRelevanceText)
            : string.Empty;
        string cityImpactMeaningText = HasText(city.LatestResult.CityImpactMeaningText)
            ? TrimTrailingPeriod(city.LatestResult.CityImpactMeaningText)
            : string.Empty;
        string recommendationShiftText = HasText(city.LatestResult.RecommendationShiftText)
            ? TrimTrailingPeriod(city.LatestResult.RecommendationShiftText)
            : string.Empty;

        if (HasText(recommendationShiftText))
        {
            return CombineSentences(cityImpactMeaningText, recommendationShiftText);
        }

        if (HasText(cityImpactMeaningText))
        {
            return CombineSentences(missionRelevanceText, cityImpactMeaningText);
        }

        if (!city.LatestResult.Success)
        {
            return CombineSentences(
                missionRelevanceText,
                "The last run failed to resolve the pressure. Recover or recruit before retrying.");
        }

        if (HasText(city.LatestResult.RewardResourceId) && city.LatestResult.RewardResourceId == topShortageResourceId)
        {
            return CombineSentences(
                missionRelevanceText,
                "The last run targeted the current shortage, so decide whether to stabilize or repeat the same fix.");
        }

        if (HasText(topShortageResourceId))
        {
            return CombineSentences(
                missionRelevanceText,
                topShortageResourceId + " is now the bottleneck to watch after the last run.");
        }

        if (HasText(city.LastNeedPressureChangeText))
        {
            return CombineSentences(
                missionRelevanceText,
                "Need pressure changed after the last run, so the next dispatch should react to that shift.");
        }

        if (HasText(city.LastDispatchReadinessChangeText))
        {
            return CombineSentences(
                missionRelevanceText,
                "Dispatch readiness changed after the last run, so pacing matters more than raw output.");
        }

        return CombineSentences(
            missionRelevanceText,
            "The city state changed after the last run. Reassess before sending the next party.");
    }

    private static CityRecentImpactType BuildRecentImpactType(CityStatusReadModel city, string topShortageResourceId)
    {
        if (city == null || city.LatestResult == null || !city.LatestResult.HasResult)
        {
            return CityRecentImpactType.ResourceGain;
        }

        if (!city.LatestResult.Success)
        {
            return CityRecentImpactType.ExpeditionFailure;
        }

        if (HasText(city.LatestResult.RewardResourceId) &&
            HasText(topShortageResourceId) &&
            city.LatestResult.RewardResourceId == topShortageResourceId)
        {
            return CityRecentImpactType.BottleneckRelief;
        }

        return CityRecentImpactType.ResourceGain;
    }

    private static string BuildLaunchRecommendationReason(
        CityOpportunitySignal topOpportunity,
        RecentImpactSummary topImpact,
        GoldenPathCityDecisionMeaningDefinition cityDecisionMeaning)
    {
        return CombineSentences(
            topImpact != null ? topImpact.NextDecisionHintText : string.Empty,
            cityDecisionMeaning != null && HasText(cityDecisionMeaning.RecommendationRationaleText)
                ? cityDecisionMeaning.RecommendationRationaleText
                : topOpportunity != null
                    ? topOpportunity.WhyItMattersText
                    : string.Empty);
    }

    private static string BuildRecruitRecommendationReason(
        CityStatusReadModel city,
        CityOpportunitySignal topOpportunity,
        RecentImpactSummary topImpact,
        GoldenPathCityDecisionMeaningDefinition cityDecisionMeaning)
    {
        string baseReason = topOpportunity != null && HasText(topOpportunity.ExpectedRewardResourceId)
            ? topOpportunity.ExpectedRewardResourceId + " is waiting behind the missing party slot."
            : "No idle party is available to turn city pressure into an expedition decision.";
        return CombineSentences(
            topImpact != null ? topImpact.SummaryText : string.Empty,
            CombineSentences(
                cityDecisionMeaning != null ? cityDecisionMeaning.RecommendationRationaleText : string.Empty,
                baseReason));
    }

    private static string BuildRecoveryRecommendationReason(
        CityStatusReadModel city,
        RecentImpactSummary topImpact,
        GoldenPathCityDecisionMeaningDefinition cityDecisionMeaning)
    {
        string recoveryReason = city != null && city.DispatchRecoveryDaysRemaining > 0
            ? "Recovery time is still active (" + city.DispatchRecoveryDaysRemaining + " day(s) remaining)."
            : "Current dispatch readiness is " + (city != null ? city.DispatchReadinessStateId : "None") + ".";
        return CombineSentences(
            topImpact != null ? topImpact.SummaryText : string.Empty,
            CombineSentences(
                cityDecisionMeaning != null ? cityDecisionMeaning.RecommendationRationaleText : string.Empty,
                recoveryReason));
    }

    private static string BuildAdvanceWorldDayReason(
        CityStatusReadModel city,
        RecentImpactSummary topImpact,
        GoldenPathCityDecisionMeaningDefinition cityDecisionMeaning)
    {
        if (city != null && city.ActiveExpeditionCount > 0)
        {
            return CombineSentences(
                cityDecisionMeaning != null ? cityDecisionMeaning.RecommendationRationaleText : string.Empty,
                "An expedition is already in motion, so the next decision depends on its return.");
        }

        return CombineSentences(
            topImpact != null ? topImpact.SummaryText : string.Empty,
            CombineSentences(
                cityDecisionMeaning != null ? cityDecisionMeaning.RecommendationRationaleText : string.Empty,
                topImpact != null
                    ? topImpact.NextDecisionHintText
                    : "Let the current city pressure settle before the next dispatch."));
    }

    private static string BuildReviewRecommendationReason(
        CityStatusReadModel city,
        CityBottleneckSignal topBottleneck,
        RecentImpactSummary topImpact,
        GoldenPathCityDecisionMeaningDefinition cityDecisionMeaning)
    {
        string baseReason;
        if (cityDecisionMeaning != null && HasText(cityDecisionMeaning.RecommendationRationaleText))
        {
            baseReason = cityDecisionMeaning.RecommendationRationaleText;
        }
        else if (HasText(city != null ? city.RecommendationReasonText : string.Empty))
        {
            baseReason = city.RecommendationReasonText;
        }
        else if (topBottleneck != null)
        {
            baseReason = topBottleneck.SummaryText;
        }
        else
        {
            baseReason = "No single blocker dominates yet, so use the city signal stack to decide the next move.";
        }

        return CombineSentences(topImpact != null ? topImpact.SummaryText : string.Empty, baseReason);
    }

    private static ResourceAmountReadModel GetPrimaryResource(ResourceAmountReadModel[] resources)
    {
        if (resources == null)
        {
            return null;
        }

        for (int i = 0; i < resources.Length; i++)
        {
            ResourceAmountReadModel resource = resources[i];
            if (resource != null && HasText(resource.ResourceId) && resource.Amount > 0)
            {
                return resource;
            }
        }

        return null;
    }

    private static string GetPrimaryResourceId(ResourceAmountReadModel[] resources)
    {
        ResourceAmountReadModel resource = GetPrimaryResource(resources);
        return resource != null ? resource.ResourceId : string.Empty;
    }

    private static GoldenPathChainDefinition ResolveContentDefinition(string cityId, string dungeonId)
    {
        return GoldenPathContentRegistry.TryGetChain(cityId, dungeonId, out GoldenPathChainDefinition definition)
            ? definition
            : null;
    }

    private static GoldenPathCityDecisionMeaningDefinition ResolveCityDecisionMeaning(GoldenPathChainDefinition contentDefinition)
    {
        return contentDefinition != null &&
               HasText(contentDefinition.CityDecisionMeaningId) &&
               GoldenPathContentRegistry.TryGetCityDecisionMeaning(contentDefinition.CityDecisionMeaningId, out GoldenPathCityDecisionMeaningDefinition cityDecisionMeaning)
            ? cityDecisionMeaning
            : null;
    }

    private static GoldenPathRouteMeaningDefinition ResolveRouteMeaning(GoldenPathRouteDefinition routeDefinition)
    {
        return routeDefinition != null &&
               HasText(routeDefinition.RouteMeaningId) &&
               GoldenPathContentRegistry.TryGetRouteMeaning(routeDefinition.RouteMeaningId, out GoldenPathRouteMeaningDefinition routeMeaning)
            ? routeMeaning
            : null;
    }

    private static string BuildOpportunityContentSourceLabel(string cityId, string dungeonId, string routeId)
    {
        return GoldenPathContentRegistry.BuildContentSourceLabel(cityId, dungeonId, routeId);
    }

    private static string ResolveBottleneckSummaryText(
        GoldenPathChainDefinition contentDefinition,
        GoldenPathCityDecisionMeaningDefinition cityDecisionMeaning)
    {
        if (cityDecisionMeaning != null && HasText(cityDecisionMeaning.BottleneckSummaryText))
        {
            return cityDecisionMeaning.BottleneckSummaryText;
        }

        return contentDefinition != null && HasText(contentDefinition.BottleneckSummaryText)
            ? contentDefinition.BottleneckSummaryText
            : string.Empty;
    }

    private static string UseSharedCityMeaningTextOnce(string sharedText, ref bool wasUsed, string fallbackText)
    {
        if (!wasUsed && HasText(sharedText))
        {
            wasUsed = true;
            return sharedText;
        }

        return fallbackText;
    }

    private static T GetFirstItem<T>(T[] values) where T : class
    {
        return values != null && values.Length > 0 ? values[0] : null;
    }

    private static string TrimTrailingPeriod(string value)
    {
        if (!HasText(value))
        {
            return "None";
        }

        return value.EndsWith(".") ? value.Substring(0, value.Length - 1) : value;
    }

    private static string CombineSentences(string primary, string secondary)
    {
        string first = HasText(primary) ? TrimTrailingPeriod(primary) : string.Empty;
        string second = HasText(secondary) ? TrimTrailingPeriod(secondary) : string.Empty;
        if (HasText(first) && HasText(second))
        {
            return first + ". " + second + ".";
        }

        if (HasText(first))
        {
            return first + ".";
        }

        if (HasText(second))
        {
            return second + ".";
        }

        return "None";
    }

    private static bool HasText(string value)
    {
        return !string.IsNullOrEmpty(value) && value != "None";
    }
}
