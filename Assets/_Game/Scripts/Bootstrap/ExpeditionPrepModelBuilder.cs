using System;
using System.Collections.Generic;

public static class ExpeditionPrepModelBuilder
{
    public static ExpeditionPrepReadModel BuildReadModel(
        WorldBoardReadModel board,
        CityStatusReadModel city,
        DungeonStatusReadModel dungeon,
        string originCityLabel,
        string targetDungeonLabel,
        string partyId,
        string partySummaryText,
        string dispatchPolicyText,
        ApproachChoice approachChoice,
        RouteChoice[] routeChoices)
    {
        ExpeditionPrepReadModel model = new ExpeditionPrepReadModel();
        model.OriginCityId = city != null ? city.CityId ?? string.Empty : string.Empty;
        model.OriginCityLabel = HasText(originCityLabel)
            ? originCityLabel
            : city != null && HasText(city.DisplayName)
                ? city.DisplayName
                : "None";
        model.TargetDungeonId = dungeon != null ? dungeon.DungeonId ?? string.Empty : string.Empty;
        model.TargetDungeonLabel = HasText(targetDungeonLabel)
            ? targetDungeonLabel
            : dungeon != null && HasText(dungeon.DisplayName)
                ? dungeon.DisplayName
                : "None";
        model.DungeonDangerText = dungeon != null && dungeon.DangerLevel > 0 ? dungeon.DangerLevel.ToString() : "None";
        model.NeedPressureText = city != null && HasText(city.NeedPressureStateId) ? city.NeedPressureStateId : "None";
        model.DispatchReadinessText = city != null && HasText(city.DispatchReadinessStateId) ? city.DispatchReadinessStateId : "None";
        model.DispatchPolicyText = HasText(dispatchPolicyText)
            ? dispatchPolicyText
            : city != null && HasText(city.DispatchPolicyStateId)
                ? city.DispatchPolicyStateId
                : "None";
        model.PartyId = partyId ?? string.Empty;
        model.PartySummaryText = HasText(partySummaryText) ? partySummaryText : (HasText(partyId) ? partyId : "None");
        model.PartyReadinessText = HasText(partyId)
            ? "Party ready from " + model.OriginCityLabel + "."
            : "No idle party is ready to launch.";
        model.RouteChoices = routeChoices ?? Array.Empty<RouteChoice>();
        model.ApproachChoice = approachChoice ?? new ApproachChoice();

        CityDecisionReadModel decision = city != null && city.Decision != null
            ? city.Decision
            : CityDecisionModelBuilder.Build(board ?? WorldBoardReadModel.Empty, city);
        model.LinkedBottlenecks = decision != null && decision.Bottlenecks != null
            ? decision.Bottlenecks
            : Array.Empty<CityBottleneckSignal>();
        model.LinkedOpportunities = FilterOpportunities(decision != null ? decision.Opportunities : null, model.TargetDungeonId);
        model.RecommendedActions = decision != null && decision.RecommendedActions != null
            ? decision.RecommendedActions
            : Array.Empty<CityActionRecommendation>();

        RouteChoice selectedRoute = FindSelectedRoute(model.RouteChoices);
        RouteChoice recommendedRoute = FindRecommendedRoute(model.RouteChoices);
        model.SelectedRouteId = selectedRoute != null ? selectedRoute.RouteId : string.Empty;
        model.RecommendedRouteId = recommendedRoute != null ? recommendedRoute.RouteId : string.Empty;

        RecentImpactSummary primaryImpact = FindPrimaryRecentImpact(decision != null ? decision.RecentImpacts : null, model.TargetDungeonId);
        CityBottleneckSignal primaryBottleneck = FindPrimaryBottleneck(model.LinkedBottlenecks);
        CityOpportunitySignal primaryOpportunity = FindPrimaryOpportunity(model.LinkedOpportunities, model.TargetDungeonId);
        CityActionRecommendation primaryRecommendation = FindPrimaryRecommendation(model.RecommendedActions, model.OriginCityId, model.TargetDungeonId);
        GoldenPathChainDefinition contentDefinition = ResolveContentDefinition(model.OriginCityId, model.TargetDungeonId);
        GoldenPathChainDefinition routeAwareContentDefinition = ResolveContentDefinitionForRoute(
            model.OriginCityId,
            model.TargetDungeonId,
            selectedRoute != null && HasText(selectedRoute.RouteId)
                ? selectedRoute.RouteId
                : recommendedRoute != null
                    ? recommendedRoute.RouteId
                    : string.Empty);

        model.ObjectiveText = BuildObjectiveText(model.OriginCityLabel, model.TargetDungeonLabel, primaryBottleneck, primaryOpportunity, contentDefinition);
        model.RecentImpactSummaryText = primaryImpact != null && HasText(primaryImpact.SummaryText)
            ? primaryImpact.SummaryText
            : "None";
        model.RecentImpactHintText = primaryImpact != null && HasText(primaryImpact.NextDecisionHintText)
            ? primaryImpact.NextDecisionHintText
            : "None";
        model.RecommendedActionSummaryText = primaryRecommendation != null && HasText(primaryRecommendation.SummaryText)
            ? primaryRecommendation.SummaryText
            : "None";
        model.RecommendedActionReasonText = primaryRecommendation != null && HasText(primaryRecommendation.ReasonText)
            ? primaryRecommendation.ReasonText
            : "None";
        model.WhyNowText = BuildWhyNowText(
            decision,
            model.RecentImpactSummaryText,
            model.RecentImpactHintText,
            model.RecommendedActionSummaryText,
            model.RecommendedActionReasonText);
        model.ExpectedUsefulnessText = BuildExpectedUsefulnessText(
            primaryOpportunity,
            model.RecommendedActionReasonText,
            model.RecentImpactHintText,
            routeAwareContentDefinition ?? contentDefinition);
        model.RiskRewardPreviewText = BuildRiskRewardPreview(selectedRoute ?? recommendedRoute);
        model.LaunchReadiness = BuildLaunchReadiness(city, dungeon, partyId, model.RouteChoices, model.SelectedRouteId, model.RecommendedRouteId);
        return model;
    }

    public static ExpeditionPlan BuildPlan(ExpeditionPrepReadModel prepReadModel, int launchWorldDay, bool isConfirmed)
    {
        ExpeditionPrepReadModel source = prepReadModel ?? new ExpeditionPrepReadModel();
        RouteChoice selectedRoute = FindSelectedRoute(source.RouteChoices) ?? FindRecommendedRoute(source.RouteChoices) ?? new RouteChoice();

        ExpeditionPlan plan = new ExpeditionPlan();
        plan.PlanId = BuildPlanId(source.OriginCityId, source.TargetDungeonId, selectedRoute.RouteId, launchWorldDay);
        plan.LaunchWorldDay = launchWorldDay;
        plan.IsConfirmed = isConfirmed;
        plan.OriginCityId = source.OriginCityId ?? string.Empty;
        plan.OriginCityLabel = HasText(source.OriginCityLabel) ? source.OriginCityLabel : "None";
        plan.TargetDungeonId = source.TargetDungeonId ?? string.Empty;
        plan.TargetDungeonLabel = HasText(source.TargetDungeonLabel) ? source.TargetDungeonLabel : "None";
        plan.PartyId = source.PartyId ?? string.Empty;
        plan.PartySummaryText = HasText(source.PartySummaryText) ? source.PartySummaryText : "None";
        plan.ObjectiveText = HasText(source.ObjectiveText) ? source.ObjectiveText : "None";
        plan.WhyNowText = HasText(source.WhyNowText) ? source.WhyNowText : "None";
        plan.ExpectedUsefulnessText = HasText(source.ExpectedUsefulnessText) ? source.ExpectedUsefulnessText : "None";
        plan.RiskRewardPreviewText = HasText(source.RiskRewardPreviewText)
            ? source.RiskRewardPreviewText
            : BuildRiskRewardPreview(selectedRoute);
        plan.RecommendedRouteId = source.RecommendedRouteId ?? string.Empty;
        plan.FollowedRecommendation = HasText(source.RecommendedRouteId) && source.RecommendedRouteId == selectedRoute.RouteId;
        plan.ExpectedRewardTags = BuildExpectedRewardTags(source, selectedRoute);
        plan.RequirementSummary = BuildRequirementSummary(source, selectedRoute);
        plan.ReadinessFlags = BuildReadinessFlags(source);
        plan.LinkedBottlenecks = source.LinkedBottlenecks ?? Array.Empty<CityBottleneckSignal>();
        plan.LinkedOpportunities = source.LinkedOpportunities ?? Array.Empty<CityOpportunitySignal>();
        plan.SelectedRoute = selectedRoute;
        plan.ApproachChoice = source.ApproachChoice ?? new ApproachChoice();
        plan.LaunchReadiness = source.LaunchReadiness ?? new LaunchReadiness();
        plan.SummaryText = BuildPlanSummary(plan);
        return plan;
    }

    private static LaunchReadiness BuildLaunchReadiness(
        CityStatusReadModel city,
        DungeonStatusReadModel dungeon,
        string partyId,
        RouteChoice[] routeChoices,
        string selectedRouteId,
        string recommendedRouteId)
    {
        List<PrepBlocker> blockingIssues = new List<PrepBlocker>();
        List<PrepBlocker> warningIssues = new List<PrepBlocker>();

        if (city == null || !HasText(city.CityId))
        {
            blockingIssues.Add(CreateIssue("missing_city", "City", "Select a city before launching.", ExpeditionConstraintSeverity.Blocking, true));
        }

        if (dungeon == null || !HasText(dungeon.DungeonId))
        {
            blockingIssues.Add(CreateIssue("missing_dungeon", "Dungeon", "Select a linked dungeon before launching.", ExpeditionConstraintSeverity.Blocking, true));
        }

        if (!HasText(partyId))
        {
            blockingIssues.Add(CreateIssue("insufficient_party", "Party", "No idle party is available for this expedition.", ExpeditionConstraintSeverity.Blocking, true));
        }

        if (!HasText(selectedRouteId))
        {
            blockingIssues.Add(CreateIssue("missing_route", "Route", "Pick a route before dispatching the party.", ExpeditionConstraintSeverity.Blocking, true));
        }

        RouteChoice selectedRoute = FindSelectedRoute(routeChoices);
        if (selectedRoute == null && HasText(selectedRouteId))
        {
            blockingIssues.Add(CreateIssue("unknown_route", "Route", "The selected route could not be resolved.", ExpeditionConstraintSeverity.Blocking, true));
        }

        if (city != null && HasText(city.DispatchReadinessStateId) && city.DispatchReadinessStateId != "Ready")
        {
            warningIssues.Add(CreateIssue(
                "dispatch_recovery",
                "Dispatch",
                city.DispatchReadinessStateId == "Recovering"
                    ? "The city is still recovering. Launch is possible, but the next dispatch window will stay tight."
                    : "The city is strained. Launch is still possible in the prototype, but the run is riskier for the next loop.",
                ExpeditionConstraintSeverity.Warning,
                false));
        }

        if (city != null && city.AvailableContractSlots < 1)
        {
            warningIssues.Add(CreateIssue("contract_lane_full", "Contract", "The linked contract lane is already full, so this launch is operating outside the city lane preview.", ExpeditionConstraintSeverity.Warning, false));
        }

        if (city != null && HasText(recommendedRouteId) && HasText(selectedRouteId) && recommendedRouteId != selectedRouteId)
        {
            warningIssues.Add(CreateIssue("off_recommendation", "Route", "The selected route deviates from the current city recommendation.", ExpeditionConstraintSeverity.Warning, false));
        }

        CityBottleneckSignal routeCapacitySignal = FindSignal(city != null && city.Decision != null ? city.Decision.Bottlenecks : null, CityBottleneckSignalType.RouteCapacity);
        if (routeCapacitySignal != null)
        {
            warningIssues.Add(CreateIssue("route_capacity", "Road", routeCapacitySignal.SummaryText, ExpeditionConstraintSeverity.Warning, false));
        }

        LaunchReadiness readiness = new LaunchReadiness();
        readiness.CanLaunch = blockingIssues.Count == 0;
        readiness.HasWarnings = warningIssues.Count > 0;
        readiness.BlockingIssues = blockingIssues.ToArray();
        readiness.WarningIssues = warningIssues.ToArray();
        readiness.SummaryText = readiness.CanLaunch
            ? readiness.HasWarnings
                ? "Launch ready with caution."
                : "Launch ready."
            : blockingIssues[0].SummaryText;
        return readiness;
    }

    private static PrepBlocker CreateIssue(string code, string sourceText, string summaryText, ExpeditionConstraintSeverity severity, bool isBlocking)
    {
        PrepBlocker issue = new PrepBlocker();
        issue.Code = code ?? string.Empty;
        issue.SourceText = HasText(sourceText) ? sourceText : "None";
        issue.SummaryText = HasText(summaryText) ? summaryText : "None";
        issue.Severity = severity;
        issue.IsBlocking = isBlocking;
        return issue;
    }

    private static CityBottleneckSignal FindPrimaryBottleneck(CityBottleneckSignal[] signals)
    {
        if (signals == null || signals.Length == 0)
        {
            return null;
        }

        return signals[0];
    }

    private static CityOpportunitySignal FindPrimaryOpportunity(CityOpportunitySignal[] signals, string dungeonId)
    {
        if (signals == null || signals.Length == 0)
        {
            return null;
        }

        for (int i = 0; i < signals.Length; i++)
        {
            CityOpportunitySignal signal = signals[i];
            if (signal != null && (!HasText(dungeonId) || signal.DungeonId == dungeonId))
            {
                return signal;
            }
        }

        return signals[0];
    }

    private static CityActionRecommendation FindPrimaryRecommendation(CityActionRecommendation[] recommendations, string cityId, string dungeonId)
    {
        if (recommendations == null || recommendations.Length == 0)
        {
            return null;
        }

        for (int i = 0; i < recommendations.Length; i++)
        {
            CityActionRecommendation recommendation = recommendations[i];
            if (recommendation == null)
            {
                continue;
            }

            bool cityMatches = !HasText(cityId) || recommendation.TargetCityId == cityId;
            bool dungeonMatches = !HasText(dungeonId) || !HasText(recommendation.TargetDungeonId) || recommendation.TargetDungeonId == dungeonId;
            if (cityMatches && dungeonMatches)
            {
                return recommendation;
            }
        }

        return recommendations[0];
    }

    private static RecentImpactSummary FindPrimaryRecentImpact(RecentImpactSummary[] impacts, string dungeonId)
    {
        if (impacts == null || impacts.Length == 0)
        {
            return null;
        }

        for (int i = 0; i < impacts.Length; i++)
        {
            RecentImpactSummary impact = impacts[i];
            if (impact != null &&
                impact.ShouldAffectNextDecision &&
                (!HasText(dungeonId) || !HasText(impact.RelatedDungeonId) || impact.RelatedDungeonId == dungeonId))
            {
                return impact;
            }
        }

        for (int i = 0; i < impacts.Length; i++)
        {
            RecentImpactSummary impact = impacts[i];
            if (impact != null &&
                (!HasText(dungeonId) || !HasText(impact.RelatedDungeonId) || impact.RelatedDungeonId == dungeonId))
            {
                return impact;
            }
        }

        for (int i = 0; i < impacts.Length; i++)
        {
            if (impacts[i] != null)
            {
                return impacts[i];
            }
        }

        return null;
    }

    private static CityOpportunitySignal[] FilterOpportunities(CityOpportunitySignal[] opportunities, string dungeonId)
    {
        if (opportunities == null || opportunities.Length == 0)
        {
            return Array.Empty<CityOpportunitySignal>();
        }

        List<CityOpportunitySignal> filtered = new List<CityOpportunitySignal>();
        for (int i = 0; i < opportunities.Length; i++)
        {
            CityOpportunitySignal opportunity = opportunities[i];
            if (opportunity == null)
            {
                continue;
            }

            if (!HasText(dungeonId) || !HasText(opportunity.DungeonId) || opportunity.DungeonId == dungeonId)
            {
                filtered.Add(opportunity);
            }
        }

        return filtered.Count > 0 ? filtered.ToArray() : opportunities;
    }

    private static string BuildObjectiveText(
        string cityLabel,
        string dungeonLabel,
        CityBottleneckSignal bottleneck,
        CityOpportunitySignal opportunity,
        GoldenPathChainDefinition contentDefinition)
    {
        if (contentDefinition != null && HasText(contentDefinition.MissionObjectiveText))
        {
            return contentDefinition.MissionObjectiveText;
        }

        if (opportunity != null && HasText(opportunity.RelatedBottleneckResourceId))
        {
            return "Launch " + dungeonLabel + " to relieve the " + opportunity.RelatedBottleneckResourceId + " shortage in " + cityLabel + ".";
        }

        if (opportunity != null && HasText(opportunity.ExpectedRewardResourceId))
        {
            return "Launch " + dungeonLabel + " to bring back " + opportunity.ExpectedRewardResourceId + " for " + cityLabel + ".";
        }

        if (bottleneck != null && HasText(bottleneck.ResourceId))
        {
            return "Launch " + dungeonLabel + " to answer the " + bottleneck.ResourceId + " pressure in " + cityLabel + ".";
        }

        return "Turn the current city pressure into a committed run before entering the dungeon.";
    }

    private static string BuildWhyNowText(
        CityDecisionReadModel decision,
        string recentImpactSummaryText,
        string recentImpactHintText,
        string recommendedActionSummaryText,
        string recommendedActionReasonText)
    {
        string leadClause = GetFirstReadableClause(recentImpactSummaryText);
        string actionClause = GetFirstReadableClause(recommendedActionSummaryText, leadClause);
        if (!HasText(actionClause))
        {
            actionClause = GetFirstActionableClause(recentImpactHintText, leadClause);
        }

        if (!HasText(actionClause))
        {
            actionClause = GetFirstActionableClause(recommendedActionReasonText, leadClause);
        }

        if (!HasText(actionClause))
        {
            actionClause = GetFirstReadableClause(recentImpactHintText, leadClause);
        }

        if (!HasText(actionClause))
        {
            actionClause = GetFirstActionableClause(decision != null ? decision.WhyCityMattersText : string.Empty, leadClause);
        }

        if (!HasText(actionClause))
        {
            actionClause = GetFirstReadableClause(recommendedActionReasonText, leadClause);
        }

        List<string> clauses = new List<string>();
        AddDistinctReadableClause(clauses, leadClause);
        AddDistinctReadableClause(clauses, actionClause);

        return CombineReadableClauses(
            clauses,
            2,
            "Act on the city's latest change before the signal goes stale.",
            120);
    }

    private static string BuildExpectedUsefulnessText(
        CityOpportunitySignal primaryOpportunity,
        string recommendedActionReasonText,
        string recentImpactHintText,
        GoldenPathChainDefinition contentDefinition)
    {
        if (contentDefinition != null && HasText(contentDefinition.RewardMeaningText))
        {
            return contentDefinition.RewardMeaningText;
        }

        GoldenPathOutcomeMeaningDefinition outcomeMeaning = ResolveOutcomeMeaning(contentDefinition);
        if (outcomeMeaning != null && HasText(outcomeMeaning.RewardMeaningText))
        {
            return outcomeMeaning.RewardMeaningText;
        }

        if (primaryOpportunity != null && HasText(primaryOpportunity.WhyItMattersText))
        {
            return CombineSentences(primaryOpportunity.WhyItMattersText, recommendedActionReasonText);
        }

        if (HasText(recommendedActionReasonText))
        {
            return recommendedActionReasonText;
        }

        if (HasText(recentImpactHintText))
        {
            return recentImpactHintText;
        }

        return "Clearing the linked dungeon should feed the next city decision with a concrete result.";
    }

    private static string BuildRiskRewardPreview(RouteChoice routeChoice)
    {
        if (routeChoice == null || !HasText(routeChoice.RouteId))
        {
            return "Pick a route to preview the risk and reward tradeoff.";
        }

        return routeChoice.RouteLabel + " | " + routeChoice.RiskLevelText + " risk | " + routeChoice.RewardPreviewText + " | " + routeChoice.ExpectedNeedImpactText;
    }

    private static string[] BuildExpectedRewardTags(ExpeditionPrepReadModel prepReadModel, RouteChoice selectedRoute)
    {
        List<string> tags = new List<string>();
        if (prepReadModel != null && prepReadModel.LinkedOpportunities != null)
        {
            for (int i = 0; i < prepReadModel.LinkedOpportunities.Length; i++)
            {
                CityOpportunitySignal opportunity = prepReadModel.LinkedOpportunities[i];
                if (opportunity != null && HasText(opportunity.ExpectedRewardResourceId))
                {
                    tags.Add(opportunity.ExpectedRewardResourceId);
                }
            }
        }

        if (selectedRoute != null && HasText(selectedRoute.RiskLevelText))
        {
            tags.Add(selectedRoute.RiskLevelText + " risk");
        }

        return tags.Count > 0 ? tags.ToArray() : new[] { "mana_shard" };
    }

    private static string[] BuildRequirementSummary(ExpeditionPrepReadModel prepReadModel, RouteChoice selectedRoute)
    {
        List<string> requirements = new List<string>();
        if (prepReadModel != null)
        {
            requirements.Add(HasText(prepReadModel.PartyId) ? "Idle party locked" : "Idle party missing");
            requirements.Add(HasText(prepReadModel.SelectedRouteId) ? "Route locked" : "Route missing");
            requirements.Add(HasText(prepReadModel.DispatchPolicyText) ? prepReadModel.DispatchPolicyText + " policy active" : "No policy");
        }

        if (selectedRoute != null && HasText(selectedRoute.TravelImplicationText))
        {
            requirements.Add("Room plan: " + selectedRoute.TravelImplicationText);
        }

        return requirements.ToArray();
    }

    private static string[] BuildReadinessFlags(ExpeditionPrepReadModel prepReadModel)
    {
        List<string> flags = new List<string>();
        if (prepReadModel == null)
        {
            return flags.ToArray();
        }

        if (HasText(prepReadModel.NeedPressureText))
        {
            flags.Add("Need " + prepReadModel.NeedPressureText);
        }

        if (HasText(prepReadModel.DispatchReadinessText))
        {
            flags.Add("Dispatch " + prepReadModel.DispatchReadinessText);
        }

        if (HasText(prepReadModel.SelectedRouteId))
        {
            flags.Add("Route locked");
        }

        if (prepReadModel.LaunchReadiness != null && prepReadModel.LaunchReadiness.HasWarnings)
        {
            flags.Add("Warnings present");
        }

        return flags.ToArray();
    }

    private static string BuildPlanSummary(ExpeditionPlan plan)
    {
        if (plan == null)
        {
            return "None";
        }

        string routeLabel = plan.SelectedRoute != null && HasText(plan.SelectedRoute.RouteLabel)
            ? plan.SelectedRoute.RouteLabel
            : "No route";
        string approachLabel = plan.ApproachChoice != null && HasText(plan.ApproachChoice.ApproachLabel)
            ? plan.ApproachChoice.ApproachLabel
            : "No approach";
        string objectiveText = HasText(plan.ObjectiveText) ? plan.ObjectiveText : "No objective";
        return plan.OriginCityLabel + " -> " + plan.TargetDungeonLabel + " | " + routeLabel + " | " + approachLabel + " | " + objectiveText;
    }

    private static string BuildPlanId(string cityId, string dungeonId, string routeId, int launchWorldDay)
    {
        return (HasText(cityId) ? cityId : "city") + "-" +
               (HasText(dungeonId) ? dungeonId : "dungeon") + "-" +
               (HasText(routeId) ? routeId : "route") + "-d" + launchWorldDay;
    }

    private static RouteChoice FindSelectedRoute(RouteChoice[] routeChoices)
    {
        if (routeChoices == null || routeChoices.Length == 0)
        {
            return null;
        }

        for (int i = 0; i < routeChoices.Length; i++)
        {
            RouteChoice route = routeChoices[i];
            if (route != null && route.IsSelected)
            {
                return route;
            }
        }

        return null;
    }

    private static RouteChoice FindRecommendedRoute(RouteChoice[] routeChoices)
    {
        if (routeChoices == null || routeChoices.Length == 0)
        {
            return null;
        }

        for (int i = 0; i < routeChoices.Length; i++)
        {
            RouteChoice route = routeChoices[i];
            if (route != null && route.IsRecommended)
            {
                return route;
            }
        }

        return null;
    }

    private static CityBottleneckSignal FindSignal(CityBottleneckSignal[] signals, CityBottleneckSignalType type)
    {
        if (signals == null || signals.Length == 0)
        {
            return null;
        }

        for (int i = 0; i < signals.Length; i++)
        {
            CityBottleneckSignal signal = signals[i];
            if (signal != null && signal.Type == type)
            {
                return signal;
            }
        }

        return null;
    }

    private static string CombineSentences(string primaryText, string secondaryText)
    {
        if (!HasText(primaryText))
        {
            return HasText(secondaryText) ? secondaryText : "None";
        }

        if (!HasText(secondaryText) ||
            string.Equals(primaryText, secondaryText, StringComparison.Ordinal) ||
            primaryText.IndexOf(secondaryText, StringComparison.OrdinalIgnoreCase) >= 0)
        {
            return primaryText;
        }

        if (secondaryText.IndexOf(primaryText, StringComparison.OrdinalIgnoreCase) >= 0)
        {
            return secondaryText;
        }

        return primaryText + " | " + secondaryText;
    }

    private static string GetFirstActionableClause(string text, params string[] excludedClauses)
    {
        if (!HasText(text))
        {
            return string.Empty;
        }

        string normalizedText = text.Replace("|", ".");
        string[] rawClauses = normalizedText.Split(new[] { '.' }, StringSplitOptions.RemoveEmptyEntries);
        for (int i = 0; i < rawClauses.Length; i++)
        {
            string clause = rawClauses[i].Trim();
            if (!IsUsableReadableClause(clause) ||
                !IsActionableWhyNowClause(clause) ||
                IsExcludedReadableClause(clause, excludedClauses))
            {
                continue;
            }

            return TrimReadableClause(clause);
        }

        return string.Empty;
    }

    private static string GetFirstReadableClause(string text, params string[] excludedClauses)
    {
        if (!HasText(text))
        {
            return string.Empty;
        }

        string normalizedText = text.Replace("|", ".");
        string[] rawClauses = normalizedText.Split(new[] { '.' }, StringSplitOptions.RemoveEmptyEntries);
        for (int i = 0; i < rawClauses.Length; i++)
        {
            string clause = rawClauses[i].Trim();
            if (!IsUsableReadableClause(clause) || IsExcludedReadableClause(clause, excludedClauses))
            {
                continue;
            }

            return TrimReadableClause(clause);
        }

        return string.Empty;
    }

    private static void AddDistinctReadableClause(List<string> clauses, string clause)
    {
        if (clauses == null || !HasText(clause))
        {
            return;
        }

        string normalizedClause = NormalizeReadableClause(clause);
        if (!HasText(normalizedClause))
        {
            return;
        }

        for (int i = 0; i < clauses.Count; i++)
        {
            string existingClause = clauses[i];
            string normalizedExistingClause = NormalizeReadableClause(existingClause);
            if (!HasText(normalizedExistingClause))
            {
                continue;
            }

            if (string.Equals(normalizedExistingClause, normalizedClause, StringComparison.Ordinal) ||
                normalizedExistingClause.Contains(normalizedClause) ||
                normalizedClause.Contains(normalizedExistingClause))
            {
                return;
            }
        }

        clauses.Add(TrimReadableClause(clause));
    }

    private static bool IsUsableReadableClause(string clause)
    {
        return HasText(clause) &&
               clause.Length >= 8 &&
               !clause.StartsWith("World delta:", StringComparison.OrdinalIgnoreCase);
    }

    private static bool IsExcludedReadableClause(string clause, string[] excludedClauses)
    {
        if (!HasText(clause) || excludedClauses == null || excludedClauses.Length == 0)
        {
            return false;
        }

        string normalizedClause = NormalizeReadableClause(clause);
        for (int i = 0; i < excludedClauses.Length; i++)
        {
            string excludedClause = excludedClauses[i];
            string normalizedExcludedClause = NormalizeReadableClause(excludedClause);
            if (!HasText(normalizedExcludedClause))
            {
                continue;
            }

            if (normalizedExcludedClause == normalizedClause ||
                normalizedExcludedClause.Contains(normalizedClause) ||
                normalizedClause.Contains(normalizedExcludedClause))
            {
                return true;
            }
        }

        return false;
    }

    private static bool IsActionableWhyNowClause(string clause)
    {
        string normalizedClause = NormalizeReadableClause(clause);
        if (!HasText(normalizedClause))
        {
            return false;
        }

        return normalizedClause.Contains("dispatch") ||
               normalizedClause.Contains("recover") ||
               normalizedClause.Contains("pressure") ||
               normalizedClause.Contains("stable") ||
               normalizedClause.Contains("urgent") ||
               normalizedClause.Contains("window") ||
               normalizedClause.Contains("readiness") ||
               normalizedClause.Contains("bottleneck") ||
               normalizedClause.Contains("shortage") ||
               normalizedClause.Contains("next push");
    }

    private static string CombineReadableClauses(List<string> clauses, int maxClauses, string fallbackText, int maxLength = int.MaxValue)
    {
        if (clauses == null || clauses.Count == 0 || maxClauses < 1)
        {
            return fallbackText;
        }

        int count = clauses.Count < maxClauses ? clauses.Count : maxClauses;
        string[] selectedClauses = new string[count];
        for (int i = 0; i < count; i++)
        {
            selectedClauses[i] = clauses[i];
        }

        for (int selectedCount = count; selectedCount >= 1; selectedCount--)
        {
            string candidate = string.Join(". ", selectedClauses, 0, selectedCount) + ".";
            if (candidate.Length <= maxLength)
            {
                return candidate;
            }
        }

        string compactFallback = CompactReadableText(selectedClauses[0], maxLength);
        if (HasText(compactFallback))
        {
            return compactFallback;
        }

        return CompactReadableText(fallbackText, maxLength);
    }

    private static string CompactReadableText(string text, int maxLength)
    {
        if (!HasText(text))
        {
            return "None";
        }

        string trimmed = TrimReadableClause(text);
        if (!HasText(trimmed))
        {
            return "None";
        }

        string sentence = trimmed + ".";
        if (sentence.Length <= maxLength)
        {
            return sentence;
        }

        if (maxLength <= 3)
        {
            return sentence.Substring(0, maxLength);
        }

        return sentence.Substring(0, maxLength - 3).TrimEnd() + "...";
    }

    private static string NormalizeReadableClause(string text)
    {
        if (!HasText(text))
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
                continue;
            }

            if (char.IsWhiteSpace(character) && count > 0 && buffer[count - 1] != ' ')
            {
                buffer[count++] = ' ';
            }
        }

        return new string(buffer, 0, count).Trim();
    }

    private static string TrimReadableClause(string text)
    {
        if (!HasText(text))
        {
            return "None";
        }

        return text.Trim().TrimEnd('.', '!', '?');
    }

    private static bool HasText(string value)
    {
        return !string.IsNullOrEmpty(value) && value != "None";
    }

    private static GoldenPathChainDefinition ResolveContentDefinition(string cityId, string dungeonId)
    {
        return GoldenPathContentRegistry.TryGetChain(cityId, dungeonId, out GoldenPathChainDefinition definition)
            ? definition
            : null;
    }

    private static GoldenPathChainDefinition ResolveContentDefinitionForRoute(string cityId, string dungeonId, string routeId)
    {
        return GoldenPathContentRegistry.TryGetChainForRoute(cityId, dungeonId, routeId, out GoldenPathChainDefinition definition)
            ? definition
            : null;
    }

    private static GoldenPathOutcomeMeaningDefinition ResolveOutcomeMeaning(GoldenPathChainDefinition contentDefinition)
    {
        return contentDefinition != null &&
               HasText(contentDefinition.OutcomeMeaningId) &&
               GoldenPathContentRegistry.TryGetOutcomeMeaning(contentDefinition.OutcomeMeaningId, out GoldenPathOutcomeMeaningDefinition outcomeMeaning)
            ? outcomeMeaning
            : null;
    }
}
