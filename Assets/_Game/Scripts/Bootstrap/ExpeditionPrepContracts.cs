using System;

public enum ExpeditionConstraintSeverity
{
    Info,
    Warning,
    Blocking
}

public sealed class PrepBlocker
{
    public string Code = string.Empty;
    public string SourceText = "None";
    public string SummaryText = "None";
    public ExpeditionConstraintSeverity Severity = ExpeditionConstraintSeverity.Info;
    public bool IsBlocking;
}

public sealed class LaunchReadiness
{
    public bool CanLaunch;
    public bool HasWarnings;
    public string SummaryText = "None";
    public PrepBlocker[] BlockingIssues = Array.Empty<PrepBlocker>();
    public PrepBlocker[] WarningIssues = Array.Empty<PrepBlocker>();
}

public sealed class RouteChoice
{
    public string RouteId = string.Empty;
    public string RouteLabel = "None";
    public string RiskLevelText = "None";
    public string DescriptionText = "None";
    public string TravelImplicationText = "None";
    public string EncounterProfileText = "None";
    public string EventFocusText = "None";
    public string RewardPreviewText = "None";
    public string ExpectedNeedImpactText = "None";
    public bool IsRecommended;
    public bool IsSelected;
}

public sealed class ApproachChoice
{
    public string ApproachId = string.Empty;
    public string ApproachLabel = "None";
    public string SummaryText = "None";
    public string RiskBiasText = "None";
}

public sealed class ExpeditionPrepReadModel
{
    public string OriginCityId = string.Empty;
    public string OriginCityLabel = "None";
    public string TargetDungeonId = string.Empty;
    public string TargetDungeonLabel = "None";
    public string DungeonDangerText = "None";
    public string NeedPressureText = "None";
    public string DispatchReadinessText = "None";
    public string DispatchPolicyText = "None";
    public string PartyId = string.Empty;
    public string PartySummaryText = "None";
    public string PartyReadinessText = "None";
    public string ObjectiveText = "None";
    public string WhyNowText = "None";
    public string RecentImpactSummaryText = "None";
    public string RecentImpactHintText = "None";
    public string RecommendedActionSummaryText = "None";
    public string RecommendedActionReasonText = "None";
    public string ExpectedUsefulnessText = "None";
    public string RiskRewardPreviewText = "None";
    public string RecommendedRouteId = string.Empty;
    public string SelectedRouteId = string.Empty;
    public CityBottleneckSignal[] LinkedBottlenecks = Array.Empty<CityBottleneckSignal>();
    public CityOpportunitySignal[] LinkedOpportunities = Array.Empty<CityOpportunitySignal>();
    public CityActionRecommendation[] RecommendedActions = Array.Empty<CityActionRecommendation>();
    public RouteChoice[] RouteChoices = Array.Empty<RouteChoice>();
    public ApproachChoice ApproachChoice = new ApproachChoice();
    public LaunchReadiness LaunchReadiness = new LaunchReadiness();
}

public sealed class ExpeditionPlan
{
    public string PlanId = string.Empty;
    public int LaunchWorldDay;
    public bool IsConfirmed;
    public string OriginCityId = string.Empty;
    public string OriginCityLabel = "None";
    public string TargetDungeonId = string.Empty;
    public string TargetDungeonLabel = "None";
    public string PartyId = string.Empty;
    public string PartySummaryText = "None";
    public string ObjectiveText = "None";
    public string WhyNowText = "None";
    public string ExpectedUsefulnessText = "None";
    public string RiskRewardPreviewText = "None";
    public string RecommendedRouteId = string.Empty;
    public bool FollowedRecommendation;
    public string[] ExpectedRewardTags = Array.Empty<string>();
    public string[] RequirementSummary = Array.Empty<string>();
    public string[] ReadinessFlags = Array.Empty<string>();
    public CityBottleneckSignal[] LinkedBottlenecks = Array.Empty<CityBottleneckSignal>();
    public CityOpportunitySignal[] LinkedOpportunities = Array.Empty<CityOpportunitySignal>();
    public RouteChoice SelectedRoute = new RouteChoice();
    public ApproachChoice ApproachChoice = new ApproachChoice();
    public LaunchReadiness LaunchReadiness = new LaunchReadiness();
    public string SummaryText = "None";
}
