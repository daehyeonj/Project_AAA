public static class ExpeditionLaunchCoordinator
{
    public static bool CanLaunch(ExpeditionPrepReadModel prepReadModel, out string failureSummary)
    {
        failureSummary = "Launch is blocked.";
        if (!HasMeaningfulPrepReadModel(prepReadModel))
        {
            failureSummary = "Expedition prep context is missing.";
            return false;
        }

        LaunchReadiness readiness = prepReadModel.LaunchReadiness ?? new LaunchReadiness();
        if (!readiness.CanLaunch)
        {
            failureSummary = HasText(readiness.SummaryText) ? readiness.SummaryText : failureSummary;
            return false;
        }

        RouteChoice selectedRoute = FindSelectedRoute(prepReadModel.RouteChoices);
        if (selectedRoute == null || !HasText(selectedRoute.RouteId))
        {
            failureSummary = "Select a route before confirming the expedition.";
            return false;
        }

        failureSummary = HasText(readiness.SummaryText) ? readiness.SummaryText : "Launch ready.";
        return true;
    }

    public static ExpeditionPlan BuildConfirmedPlan(
        ExpeditionPrepReadModel prepReadModel,
        int launchWorldDay,
        string partyId,
        string fallbackPartySummaryText)
    {
        ExpeditionPlan plan = ExpeditionPrepModelBuilder.BuildPlan(prepReadModel, launchWorldDay, true);
        plan.IsConfirmed = true;
        plan.PartyId = HasText(partyId) ? partyId : plan.PartyId;
        if (!HasText(plan.PartySummaryText))
        {
            plan.PartySummaryText = HasText(fallbackPartySummaryText) ? fallbackPartySummaryText : HasText(partyId) ? partyId : "None";
        }

        return plan;
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

    private static bool HasMeaningfulPrepReadModel(ExpeditionPrepReadModel prepReadModel)
    {
        return prepReadModel != null &&
               HasText(prepReadModel.OriginCityId) &&
               HasText(prepReadModel.TargetDungeonId);
    }

    private static bool HasText(string value)
    {
        return !string.IsNullOrEmpty(value) && value != "None";
    }
}
