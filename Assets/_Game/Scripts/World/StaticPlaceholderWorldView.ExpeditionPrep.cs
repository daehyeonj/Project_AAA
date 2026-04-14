using System.Collections.Generic;

public sealed partial class StaticPlaceholderWorldView
{
    private ExpeditionPrepReadModel _activeExpeditionPrepReadModel = new ExpeditionPrepReadModel();
    private ExpeditionPlan _confirmedExpeditionPlan = new ExpeditionPlan();

    private ExpeditionPrepReadModel GetCurrentExpeditionPrepReadModel()
    {
        if (_dungeonRunState == DungeonRunState.RouteChoice)
        {
            _activeExpeditionPrepReadModel = BuildExpeditionPrepReadModelInternal();
        }

        return HasMeaningfulPrepReadModel(_activeExpeditionPrepReadModel)
            ? _activeExpeditionPrepReadModel
            : new ExpeditionPrepReadModel();
    }

    private LaunchReadiness GetCurrentLaunchReadiness()
    {
        ExpeditionPrepReadModel prepReadModel = GetCurrentExpeditionPrepReadModel();
        return prepReadModel != null && prepReadModel.LaunchReadiness != null
            ? prepReadModel.LaunchReadiness
            : new LaunchReadiness();
    }

    private ExpeditionPlan GetCurrentExpeditionPlanForAppFlow()
    {
        if (HasMeaningfulConfirmedPlan(_confirmedExpeditionPlan))
        {
            return _confirmedExpeditionPlan;
        }

        ExpeditionPrepReadModel prepReadModel = GetCurrentExpeditionPrepReadModel();
        return HasMeaningfulPrepReadModel(prepReadModel)
            ? ExpeditionPrepModelBuilder.BuildPlan(prepReadModel, WorldDayCount, false)
            : new ExpeditionPlan();
    }

    private void CacheCurrentExpeditionPrepReadModel()
    {
        _activeExpeditionPrepReadModel = BuildExpeditionPrepReadModelInternal();
    }

    private void ResetExpeditionPrepContracts()
    {
        _activeExpeditionPrepReadModel = new ExpeditionPrepReadModel();
        _confirmedExpeditionPlan = new ExpeditionPlan();
    }

    private ExpeditionPrepReadModel BuildExpeditionPrepReadModelInternal()
    {
        WorldBoardReadModel board = BuildWorldBoardReadModel();
        CityStatusReadModel city = FindCityReadModel(board, _currentHomeCityId);
        DungeonStatusReadModel dungeon = FindDungeonReadModel(board, _currentDungeonId);
        string partyId = _activeDungeonParty != null && HasText(_activeDungeonParty.PartyId)
            ? _activeDungeonParty.PartyId
            : _runtimeEconomyState != null && HasText(_currentHomeCityId)
                ? _runtimeEconomyState.GetIdlePartyIdInCity(_currentHomeCityId)
                : string.Empty;
        string partySummaryText = _activeDungeonParty != null && HasText(_activeDungeonParty.DisplayName)
            ? _activeDungeonParty.DisplayName
            : HasText(partyId)
                ? partyId
                : "None";
        return ExpeditionPrepModelBuilder.BuildReadModel(
            board,
            city,
            dungeon,
            GetHomeCityDisplayName(),
            _currentDungeonName,
            partyId,
            partySummaryText,
            GetCurrentDispatchPolicyText(),
            BuildCurrentApproachChoice(),
            BuildCurrentRouteChoices());
    }

    private RouteChoice[] BuildCurrentRouteChoices()
    {
        List<RouteChoice> routeChoices = new List<RouteChoice>();
        DungeonRouteTemplate safeTemplate = GetRouteTemplateById(_currentDungeonId, SafeRouteId);
        DungeonRouteTemplate riskyTemplate = GetRouteTemplateById(_currentDungeonId, RiskyRouteId);
        if (safeTemplate != null)
        {
            routeChoices.Add(BuildRouteChoice(safeTemplate));
        }

        if (riskyTemplate != null)
        {
            routeChoices.Add(BuildRouteChoice(riskyTemplate));
        }

        return routeChoices.ToArray();
    }

    private RouteChoice BuildRouteChoice(DungeonRouteTemplate template)
    {
        RouteChoice routeChoice = new RouteChoice();
        if (template == null)
        {
            return routeChoice;
        }

        routeChoice.RouteId = template.RouteId ?? string.Empty;
        routeChoice.RouteLabel = HasText(template.RouteLabel) ? template.RouteLabel : "None";
        routeChoice.RiskLevelText = HasText(template.RiskLabel) ? template.RiskLabel : "None";
        routeChoice.DescriptionText = HasText(template.Description) ? template.Description : "None";
        routeChoice.TravelImplicationText = HasText(_currentDungeonId)
            ? BuildRoomPathPreviewText(_currentDungeonId, template.RouteId)
            : "None";
        routeChoice.EncounterProfileText = HasText(template.EncounterPreview) ? template.EncounterPreview : "None";
        routeChoice.EventFocusText = HasText(template.EventFocus) ? template.EventFocus : "None";
        routeChoice.RewardPreviewText = HasText(template.RewardPreview) ? template.RewardPreview : "None";
        routeChoice.ExpectedNeedImpactText = BuildExpectedNeedImpactText(_currentHomeCityId, _currentDungeonId, template.RouteId);
        routeChoice.IsRecommended = template.RouteId == _recommendedRouteId;
        routeChoice.IsSelected = template.RouteId == _selectedRouteChoiceId ||
                                 (!HasText(_selectedRouteChoiceId) && template.RouteId == _selectedRouteId);
        return routeChoice;
    }

    private ApproachChoice BuildCurrentApproachChoice()
    {
        string dispatchPolicyText = GetCurrentDispatchPolicyText();
        ApproachChoice choice = new ApproachChoice();
        choice.ApproachId = HasText(dispatchPolicyText) ? dispatchPolicyText.ToLowerInvariant() : "balanced";
        choice.ApproachLabel = HasText(dispatchPolicyText) ? dispatchPolicyText : "Balanced";

        if (dispatchPolicyText == "Safe")
        {
            choice.SummaryText = "Favor steadier launches so the city can keep acting after the run.";
            choice.RiskBiasText = "Bias toward lower route risk.";
            return choice;
        }

        if (dispatchPolicyText == "Profit")
        {
            choice.SummaryText = "Favor higher return launches when the city can afford the extra exposure.";
            choice.RiskBiasText = "Bias toward higher route reward.";
            return choice;
        }

        choice.SummaryText = "Keep the run aligned to both city pressure and route safety.";
        choice.RiskBiasText = "Bias toward balanced risk.";
        return choice;
    }

    private RouteChoice GetPreviewRouteChoice(ExpeditionPrepReadModel readModel)
    {
        RouteChoice[] routeChoices = readModel != null ? readModel.RouteChoices : null;
        if (routeChoices == null || routeChoices.Length == 0)
        {
            return null;
        }

        string previewRouteId = HasText(_hoverRouteChoiceId)
            ? _hoverRouteChoiceId
            : HasText(_selectedRouteChoiceId)
                ? _selectedRouteChoiceId
                : HasText(readModel.SelectedRouteId)
                    ? readModel.SelectedRouteId
                    : readModel.RecommendedRouteId;

        RouteChoice recommendedRoute = null;
        for (int i = 0; i < routeChoices.Length; i++)
        {
            RouteChoice routeChoice = routeChoices[i];
            if (routeChoice == null)
            {
                continue;
            }

            if (routeChoice.IsRecommended && recommendedRoute == null)
            {
                recommendedRoute = routeChoice;
            }

            if (HasText(previewRouteId) && routeChoice.RouteId == previewRouteId)
            {
                return routeChoice;
            }
        }

        return recommendedRoute ?? routeChoices[0];
    }

    private string BuildPrepIssueSummary(LaunchReadiness readiness, bool blocking, int maxCount)
    {
        PrepBlocker[] issues = readiness != null
            ? blocking
                ? readiness.BlockingIssues
                : readiness.WarningIssues
            : null;
        if (issues == null || issues.Length == 0 || maxCount < 1)
        {
            return "None";
        }

        List<string> summaries = new List<string>();
        for (int i = 0; i < issues.Length && summaries.Count < maxCount; i++)
        {
            PrepBlocker issue = issues[i];
            if (issue != null && HasText(issue.SummaryText))
            {
                summaries.Add(issue.SummaryText);
            }
        }

        return summaries.Count > 0 ? string.Join(" | ", summaries.ToArray()) : "None";
    }

    private string BuildExpeditionStringListSummary(string[] values, int maxCount)
    {
        if (values == null || values.Length == 0 || maxCount < 1)
        {
            return "None";
        }

        List<string> summaries = new List<string>();
        for (int i = 0; i < values.Length && summaries.Count < maxCount; i++)
        {
            if (HasText(values[i]))
            {
                summaries.Add(values[i]);
            }
        }

        return summaries.Count > 0 ? string.Join(" | ", summaries.ToArray()) : "None";
    }

    private CityStatusReadModel FindCityReadModel(WorldBoardReadModel board, string cityId)
    {
        CityStatusReadModel[] cities = board != null ? board.Cities : null;
        if (cities == null || !HasText(cityId))
        {
            return null;
        }

        for (int i = 0; i < cities.Length; i++)
        {
            CityStatusReadModel city = cities[i];
            if (city != null && city.CityId == cityId)
            {
                return city;
            }
        }

        return null;
    }

    private DungeonStatusReadModel FindDungeonReadModel(WorldBoardReadModel board, string dungeonId)
    {
        DungeonStatusReadModel[] dungeons = board != null ? board.Dungeons : null;
        if (dungeons == null || !HasText(dungeonId))
        {
            return null;
        }

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

    private bool HasMeaningfulPrepReadModel(ExpeditionPrepReadModel readModel)
    {
        return readModel != null &&
               HasText(readModel.OriginCityId) &&
               HasText(readModel.TargetDungeonId);
    }

    private bool HasMeaningfulConfirmedPlan(ExpeditionPlan plan)
    {
        return plan != null &&
               plan.IsConfirmed &&
               HasText(plan.OriginCityId) &&
               HasText(plan.TargetDungeonId);
    }
}
