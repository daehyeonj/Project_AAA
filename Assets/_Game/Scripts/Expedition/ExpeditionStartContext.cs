public sealed class ExpeditionPartyMemberManifest
{
    public string MemberId = string.Empty;
    public string DisplayName = "Adventurer";
    public string RoleTag = "adventurer";
    public string RoleLabel = "Adventurer";
    public int PartySlotIndex;
    public string GrowthTrackId = string.Empty;
    public string JobId = string.Empty;
    public string EquipmentLoadoutId = string.Empty;
    public string SkillLoadoutId = string.Empty;
    public string DefaultSkillId = string.Empty;
    public string ResolvedSkillName = "Skill";
    public string ResolvedSkillShortText = "None";
    public string EquipmentSummaryText = "No gear";
    public string GearContributionSummaryText = "No bonus";
    public string AppliedProgressionSummaryText = "None";
    public string CurrentRunSummaryText = "None";
    public string NextRunPreviewSummaryText = "None";
    public int Level = 1;
    public int CurrentExperience;
    public int NextLevelExperience = 18;
    public int MaxHp = 1;
    public int Attack = 1;
    public int Defense;
    public int Speed;
    public int SkillPower = 1;
    public string SummaryText = "None";
}

public sealed class ExpeditionPartyManifest
{
    public string PartyId = string.Empty;
    public string PartyLabel = "None";
    public int MemberCount;
    public string LoadoutSummaryText = "None";
    public string MemberSummaryText = "None";
    public string AppliedProgressionSummaryText = "None";
    public string CurrentRunSummaryText = "None";
    public string NextRunPreviewSummaryText = "None";
    public string ManifestSummaryText = "None";
    public ExpeditionPartyMemberManifest[] Members = new ExpeditionPartyMemberManifest[0];
}

public sealed class ExpeditionStartContext
{
    public string StartCityId = string.Empty;
    public string StartCityLabel = "None";
    public string DungeonId = string.Empty;
    public string DungeonLabel = "None";
    public string PartyId = string.Empty;
    public string PartyLabel = "None";
    public string PartyLoadoutSummaryText = "None";
    public string StagedPartySummaryText = "None";
    public bool FollowedRecommendation;
    public string LaunchManifestSummaryText = "None";
    public ExpeditionPartyManifest PartyManifest = new ExpeditionPartyManifest();
    public string SelectedRouteId = string.Empty;
    public string SelectedRouteLabel = "None";
    public string RecommendedRouteId = string.Empty;
    public string RecommendedRouteLabel = "None";
    public string NeedPressureText = "None";
    public string DispatchReadinessText = "None";
    public string RecoveryProgressText = "None";
    public string RecoveryEtaText = "None";
    public string DispatchPolicyText = "None";
    public string RecommendationReasonText = "None";
    public string ExpectedNeedImpactText = "None";
    public string RouteRiskLabel = "None";
    public string DungeonDangerLabel = "None";
    public string SupplySummaryText = "None";
    public string SupplyPressureSummaryText = "None";
    public string TimePressureSummaryText = "None";
    public string ThreatPressureSummaryText = "None";
    public string DiscoverySummaryText = "None";
    public string ExtractionPressureSummaryText = "None";
    public string WorldModifierSummaryText = "None";
    public string RoutePreviewSummaryText = "None";
    public string RewardPreviewSummaryText = "None";
    public string EventPreviewSummaryText = "None";
    public string PartySummaryText = "None";
    public string BriefingSummaryText = "None";
    public string RouteFitSummaryText = "None";
    public string LaunchLockSummaryText = "None";
    public string ProjectedOutcomeSummaryText = "None";
    public string ContextSummaryText = "None";
}
