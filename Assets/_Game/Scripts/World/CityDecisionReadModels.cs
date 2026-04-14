using System;

public enum CityBottleneckSignalType
{
    ResourceShortage,
    ProcessingBlocker,
    RouteCapacity,
    DispatchRecovery,
    ExpeditionWindow
}

public enum CityOpportunitySignalType
{
    LinkedDungeonExpedition,
    ActiveExpeditionFollowUp
}

public enum CityRecommendedActionType
{
    RecruitParty,
    LaunchLinkedDungeonExpedition,
    WaitForRecovery,
    InspectSupplyRoute,
    AdvanceWorldDay,
    ReviewRecentImpact
}

public enum CityRecentImpactType
{
    ResourceGain,
    BottleneckRelief,
    ExpeditionFailure,
    DispatchShift
}

public sealed class CityBottleneckSignal
{
    public CityBottleneckSignalType Type = CityBottleneckSignalType.ResourceShortage;
    public string ResourceId = string.Empty;
    public string RelatedEntityId = string.Empty;
    public string RelatedEntityDisplayName = "None";
    public string ReasonCode = string.Empty;
    public string AffectedActionArea = "None";
    public int Severity;
    public bool IsBlocking;
    public string SummaryText = "None";
}

public sealed class CityOpportunitySignal
{
    public CityOpportunitySignalType Type = CityOpportunitySignalType.LinkedDungeonExpedition;
    public string DungeonId = string.Empty;
    public string DungeonDisplayName = "None";
    public string RouteId = string.Empty;
    public string RouteLabel = "None";
    public string RelatedBottleneckResourceId = string.Empty;
    public string ExpectedRewardResourceId = string.Empty;
    public string ContentSourceLabel = "None";
    public string ReadinessStateId = "None";
    public string GatingConstraintText = "None";
    public int Priority;
    public bool IsReady;
    public string SummaryText = "None";
    public string WhyItMattersText = "None";
}

public sealed class CityActionRecommendation
{
    public CityRecommendedActionType ActionType = CityRecommendedActionType.ReviewRecentImpact;
    public string TargetCityId = string.Empty;
    public string TargetDungeonId = string.Empty;
    public string RelatedResourceId = string.Empty;
    public int Priority;
    public bool IsAvailable;
    public string SummaryText = "None";
    public string ReasonText = "None";
}

public sealed class RecentImpactSummary
{
    public CityRecentImpactType Type = CityRecentImpactType.ResourceGain;
    public string ResultStateKey = string.Empty;
    public string ResourceId = string.Empty;
    public string RelatedDungeonId = string.Empty;
    public int Priority;
    public bool ShouldAffectNextDecision;
    public string SummaryText = "None";
    public string NextDecisionHintText = "None";
}

public sealed class CityDecisionReadModel
{
    public string CityId = string.Empty;
    public string CityDisplayName = "None";
    public string LinkedDungeonId = string.Empty;
    public string LinkedDungeonDisplayName = "None";
    public string NeedPressureStateId = "None";
    public string DispatchReadinessStateId = "None";
    public string DispatchPolicyStateId = "None";
    public int DispatchRecoveryDaysRemaining;
    public int ActiveExpeditionCount;
    public int IdlePartyCount;
    public string WhyCityMattersText = "None";
    public CityBottleneckSignal[] Bottlenecks = Array.Empty<CityBottleneckSignal>();
    public CityOpportunitySignal[] Opportunities = Array.Empty<CityOpportunitySignal>();
    public CityActionRecommendation[] RecommendedActions = Array.Empty<CityActionRecommendation>();
    public RecentImpactSummary[] RecentImpacts = Array.Empty<RecentImpactSummary>();

    public bool HasDecisionSignals =>
        Bottlenecks.Length > 0 ||
        Opportunities.Length > 0 ||
        RecommendedActions.Length > 0 ||
        RecentImpacts.Length > 0;
}
