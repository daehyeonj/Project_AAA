using System;

public sealed class PrototypeRpgPartyBuildLaneRuntimeState
{
    public string SessionKey = string.Empty;
    public string PartyId = string.Empty;
    public string DisplayName = string.Empty;
    public string LastResolvedRunIdentity = string.Empty;
    public string LaneSummaryText = string.Empty;
    public string ArchetypeCommitmentSummaryText = string.Empty;
    public string OfferCoherenceSummaryText = string.Empty;
    public PrototypeRpgMemberBuildLaneRuntimeState[] Members = Array.Empty<PrototypeRpgMemberBuildLaneRuntimeState>();
}

public sealed class PrototypeRpgMemberBuildLaneRuntimeState
{
    public string MemberId = string.Empty;
    public string DisplayName = string.Empty;
    public string ResolvedPrimaryLaneKey = string.Empty;
    public string ResolvedSecondaryLaneKey = string.Empty;
    public int LaneScoreFrontline;
    public int LaneScorePrecision;
    public int LaneScoreArcane;
    public int LaneScoreSupport;
    public int LaneScoreRecovery;
    public string RecentLaneReasonText = string.Empty;
    public string LaneSummaryText = string.Empty;
    public string CoherenceSummaryText = string.Empty;
    public string LastLaneResolvedRunIdentity = string.Empty;
    public int CommitmentDepth;
    public string LastArchetypeShiftKey = string.Empty;
}

public sealed class PrototypeRpgPartyLaneRecoveryRuntimeState
{
    public string SessionKey = string.Empty;
    public string PartyId = string.Empty;
    public string DisplayName = string.Empty;
    public string LastResolvedRunIdentity = string.Empty;
    public string LaneRecoverySummaryText = string.Empty;
    public string SoftRespecWindowSummaryText = string.Empty;
    public string AntiTrapSafetySummaryText = string.Empty;
    public PrototypeRpgMemberLaneRecoveryRuntimeState[] Members = Array.Empty<PrototypeRpgMemberLaneRecoveryRuntimeState>();
}

public sealed class PrototypeRpgMemberLaneRecoveryRuntimeState
{
    public string MemberId = string.Empty;
    public string DisplayName = string.Empty;
    public int RecoveryPressureScore;
    public int ConsecutiveDefeatCount;
    public int ConsecutiveRetreatCount;
    public int ConsecutiveKnockOutCount;
    public int ConsecutiveStalledOfferCount;
    public int ConsecutiveNoImprovementRunCount;
    public bool SoftRespecWindowOpen;
    public int SoftRespecWindowTier;
    public string RecoveryWindowSourceRunIdentity = string.Empty;
    public string RecentRecoveryReasonText = string.Empty;
    public string LastRecoveryTriggerKey = string.Empty;
    public string LastRecoveryResolvedRunIdentity = string.Empty;
    public int RecoveryBiasWeight;
    public int LaneLockRelaxWeight;
    public string RescueOfferGroupKey = string.Empty;
    public string[] RescueAdjacentLaneKeys = Array.Empty<string>();
    public string RecoverySafetySummaryText = string.Empty;
}

public sealed class PrototypeRpgPartyCoverageRuntimeState
{
    public string SessionKey = string.Empty;
    public string PartyId = string.Empty;
    public string DisplayName = string.Empty;
    public string LastResolvedRunIdentity = string.Empty;
    public string PartyCoverageSummaryText = string.Empty;
    public string RoleOverlapWarningSummaryText = string.Empty;
    public string CrossMemberSynergySummaryText = string.Empty;
    public string OfferCoherenceSummaryText = string.Empty;
    public string TopMissingCoverageKey = string.Empty;
    public string TopOverlapKey = string.Empty;
    public string TopSynergyKey = string.Empty;
    public PrototypeRpgMemberCoverageRuntimeState[] Members = Array.Empty<PrototypeRpgMemberCoverageRuntimeState>();
}

public sealed class PrototypeRpgMemberCoverageRuntimeState
{
    public string MemberId = string.Empty;
    public string DisplayName = string.Empty;
    public string RoleTag = string.Empty;
    public string ResolvedPrimaryLaneKey = string.Empty;
    public string ResolvedSecondaryLaneKey = string.Empty;
    public string ContributionShapeKey = string.Empty;
    public string[] CurrentCoverageTags = Array.Empty<string>();
    public string[] OverlapTags = Array.Empty<string>();
    public string[] SynergyTags = Array.Empty<string>();
    public int MissingCoverageBias;
    public int OverlapPenalty;
    public int SynergyBias;
    public string CoverageSummaryText = string.Empty;
    public string LastResolvedRunIdentity = string.Empty;
}
public sealed class PrototypeRpgPartyArchetypeRuntimeState
{
    public string SessionKey = string.Empty;
    public string PartyId = string.Empty;
    public string DisplayName = string.Empty;
    public string LastResolvedRunIdentity = string.Empty;
    public string PrimaryArchetypeKey = string.Empty;
    public string SecondaryArchetypeKey = string.Empty;
    public int FormationCoherenceScore;
    public int CommitmentStrength;
    public int BurstScore;
    public int SustainScore;
    public int SupportScore;
    public int AoeScore;
    public int PrecisionScore;
    public int RecoveryScore;
    public int FlexScore;
    public string PartyArchetypeSummaryText = string.Empty;
    public string FormationCoherenceSummaryText = string.Empty;
    public string CommitmentStrengthSummaryText = string.Empty;
    public string FlexRescueSummaryText = string.Empty;
    public string TopArchetypeHintKey = string.Empty;
    public string TopFlexHintKey = string.Empty;
    public string LastPriorityRunIdentity = string.Empty;
    public string LastTopPriorityBucketKey = string.Empty;
    public string LastTopPriorityOfferTypeKey = string.Empty;
    public string LastRareSlotOfferTypeKey = string.Empty;
    public string WeightedPrioritySummaryText = string.Empty;
    public string RareSlotSummaryText = string.Empty;
    public string HighImpactSequencingSummaryText = string.Empty;
    public string RecoverySafeguardSummaryText = string.Empty;
    public PrototypeRpgPartyArchetypeMemberRuntimeState[] Members = Array.Empty<PrototypeRpgPartyArchetypeMemberRuntimeState>();
}

public sealed class PrototypeRpgPartyArchetypeMemberRuntimeState
{
    public string MemberId = string.Empty;
    public string DisplayName = string.Empty;
    public string RoleTag = string.Empty;
    public string RoleLabel = string.Empty;
    public string CurrentLaneKey = string.Empty;
    public string SecondaryLaneKey = string.Empty;
    public string PrimaryArchetypeKey = string.Empty;
    public string SecondaryArchetypeKey = string.Empty;
    public string[] CoverageTags = Array.Empty<string>();
    public string[] SynergyTags = Array.Empty<string>();
    public string[] ArchetypeContributionTags = Array.Empty<string>();
    public int CommitmentWeight;
    public int FlexibilityWeight;
    public bool RecoveryWindowOpen;
    public string MemberArchetypeSummaryText = string.Empty;
    public string LastResolvedRunIdentity = string.Empty;
}
