using System;

[Flags]
public enum PrototypeRpgAvailableSurfaceFlags
{
    None = 0,
    BattleSurface = 1 << 0,
    PostRunSummarySurface = 1 << 1,
    PendingRewardDeltaPack = 1 << 2,
    RunResultSnapshot = 1 << 3,
    CombatContributionSnapshot = 1 << 4,
    ProgressionPreviewSnapshot = 1 << 5,
    BattleInteractionSurface = 1 << 6
}

public enum PrototypeRpgPresentationPhase
{
    None,
    Battle,
    ResultPanel,
    PostRunSummary
}

public sealed class PrototypeRpgPostRunMemberCardData
{
    public string MemberId = string.Empty;
    public string DisplayName = string.Empty;
    public string RoleLabel = string.Empty;
    public string OutcomeLabel = string.Empty;
    public string HpText = string.Empty;
    public int CurrentHp;
    public int MaxHp = 1;
    public bool Survived;
    public bool KnockedOut;
    public int DamageDealt;
    public int DamageTaken;
    public int HealingDone;
    public int ActionCount;
    public int KillCount;
    public string GrowthTrackId = string.Empty;
    public string JobId = string.Empty;
    public string EquipmentLoadoutId = string.Empty;
    public string SkillLoadoutId = string.Empty;
    public string ContributionSummaryText = string.Empty;
    public string ProgressionSummaryText = string.Empty;
    public string UnlockSummaryText = string.Empty;
    public string GrowthDirectionLabel = string.Empty;
    public string RewardCarryoverHintText = string.Empty;
    public string UpgradeCandidateSummaryText = string.Empty;
    public string UpgradeOfferSummaryText = string.Empty;
    public string ApplyReadySummaryText = string.Empty;
    public string NextGrowthTrackHint = string.Empty;
    public string NextJobHint = string.Empty;
    public string NextSkillLoadoutHint = string.Empty;
    public string NextEquipmentLoadoutHint = string.Empty;
    public string NotableOutcomeKey = string.Empty;
    public string[] GrowthHintTags = Array.Empty<string>();
    public string[] RewardHintTags = Array.Empty<string>();
    public PrototypeRpgGrowthPathCandidate[] GrowthPathCandidates = Array.Empty<PrototypeRpgGrowthPathCandidate>();
    public PrototypeRpgUpgradeCandidate[] UpgradeCandidates = Array.Empty<PrototypeRpgUpgradeCandidate>();
}

public sealed class PrototypeRpgRewardGrantEntryData
{
    public string GrantKey = string.Empty;
    public string DisplayLabel = string.Empty;
    public string ResourceId = string.Empty;
    public int Amount;
    public string StateKey = string.Empty;
    public string SummaryText = string.Empty;
}

public sealed class PrototypeRpgMemberPendingDeltaData
{
    public string MemberId = string.Empty;
    public string DisplayName = string.Empty;
    public string RoleLabel = string.Empty;
    public bool Survived;
    public bool KnockedOut;
    public string GrowthTrackId = string.Empty;
    public string JobId = string.Empty;
    public string EquipmentLoadoutId = string.Empty;
    public string SkillLoadoutId = string.Empty;
    public string ContributionSummaryText = string.Empty;
    public string PendingDeltaSummaryText = string.Empty;
    public string GrowthDirectionLabel = string.Empty;
    public string RewardCarryoverHintText = string.Empty;
    public string UpgradeCandidateSummaryText = string.Empty;
    public string UpgradeOfferSummaryText = string.Empty;
    public string ApplyReadySummaryText = string.Empty;
    public string NotableOutcomeKey = string.Empty;
    public string[] SuggestedGrowthHintTags = Array.Empty<string>();
    public string[] SuggestedRewardHintTags = Array.Empty<string>();
    public PrototypeRpgGrowthPathCandidate[] GrowthPathCandidates = Array.Empty<PrototypeRpgGrowthPathCandidate>();
    public PrototypeRpgUpgradeCandidate[] UpgradeCandidates = Array.Empty<PrototypeRpgUpgradeCandidate>();
}

public sealed class PrototypeRpgPostRunSummarySurfaceData
{
    public bool HasResult;
    public string ResultStateKey = string.Empty;
    public string Headline = "None";
    public string Subheadline = string.Empty;
    public string ResultSummaryText = string.Empty;
    public string DungeonLabel = string.Empty;
    public string RouteLabel = string.Empty;
    public string PartyOutcomeSummaryText = string.Empty;
    public string PartyConditionText = string.Empty;
    public string PartyHpSummaryText = string.Empty;
    public string SurvivingMembersText = string.Empty;
    public string ContributionSummaryText = string.Empty;
    public string ProgressionPreviewSummaryText = string.Empty;
    public string UnlockPreviewSummaryText = string.Empty;
    public string RewardCarryoverSummaryText = string.Empty;
    public string GrowthCandidateSummaryText = string.Empty;
    public string UpgradeCandidateSummaryText = string.Empty;
    public string UpgradeOfferSummaryText = string.Empty;
    public string ApplyReadySummaryText = string.Empty;
    public string LootSummaryText = string.Empty;
    public string LootBreakdownText = string.Empty;
    public string EliteOutcomeSummaryText = string.Empty;
    public string EliteRewardIdentityText = string.Empty;
    public string EliteRewardAmountText = string.Empty;
    public string EventSummaryText = string.Empty;
    public string EventChoiceText = string.Empty;
    public string PreEliteSummaryText = string.Empty;
    public string PreEliteChoiceText = string.Empty;
    public string RoomPathSummaryText = string.Empty;
    public string TurnsTakenText = "0";
    public string[] TopHighlightKeys = Array.Empty<string>();
    public PrototypeRpgPostRunMemberCardData[] MemberCards = Array.Empty<PrototypeRpgPostRunMemberCardData>();
}

public sealed class PrototypeRpgPendingRewardDeltaPack
{
    public bool HasAnyRewardDelta;
    public bool HasPendingReward;
    public bool HasPendingRewards;
    public bool HasGrantedReward;
    public bool HasCarryoverDelta;
    public string ResultStateKey = string.Empty;
    public string DungeonLabel = string.Empty;
    public string RouteLabel = string.Empty;
    public string RewardLabel = string.Empty;
    public string RewardResourceId = string.Empty;
    public int BaseRewardAmount;
    public int PendingBonusAmount;
    public int GrantedBonusAmount;
    public int BattleLootAmount;
    public int ChestLootAmount;
    public int EventLootAmount;
    public int EliteRewardAmount;
    public int EliteBonusRewardAmount;
    public int TotalReturnedAmount;
    public int LostPendingAmount;
    public string CarryoverHintText = string.Empty;
    public string TotalLootSummaryText = string.Empty;
    public string LootBreakdownText = string.Empty;
    public string EliteRewardSummaryText = string.Empty;
    public string BonusRewardSummaryText = string.Empty;
    public string SummaryText = string.Empty;
    public string[] RewardHintTags = Array.Empty<string>();
    public string[] GrowthHintTags = Array.Empty<string>();
    public string[] TopHighlightKeys = Array.Empty<string>();
    public PrototypeRpgRewardGrantEntryData[] RewardGrantEntries = Array.Empty<PrototypeRpgRewardGrantEntryData>();
    public PrototypeRpgMemberPendingDeltaData[] MemberPendingDeltas = Array.Empty<PrototypeRpgMemberPendingDeltaData>();
}

public sealed class PrototypeRpgPresentationGatewayData
{
    public PrototypeRpgPresentationPhase Phase = PrototypeRpgPresentationPhase.None;
    public string PhaseKey = "none";
    public PrototypeRpgAvailableSurfaceFlags AvailableSurfaces = PrototypeRpgAvailableSurfaceFlags.None;
    public bool HasBattleSurface;
    public bool HasBattleInteractionSurface;
    public bool HasPostRunSummarySurface;
    public bool HasPendingRewardDeltaPack;
    public string CurrentRunIdentity = string.Empty;
    public string DungeonLabel = string.Empty;
    public string RouteLabel = string.Empty;
    public string RunStateKey = "none";
    public string Headline = "None";
    public string Subheadline = string.Empty;
    public string[] TopHighlightKeys = Array.Empty<string>();
    public PrototypeBattleUiSurfaceData BattleSurface = new PrototypeBattleUiSurfaceData();
    public PrototypeBattleInteractionSurfaceData BattleInteractionSurface = new PrototypeBattleInteractionSurfaceData();
    public PrototypeRpgPostRunSummarySurfaceData PostRunSummarySurface = new PrototypeRpgPostRunSummarySurfaceData();
    public PrototypeRpgPendingRewardDeltaPack PendingRewardDeltaPack = new PrototypeRpgPendingRewardDeltaPack();
    public PrototypeRpgRunResultSnapshot RunResultSnapshot = new PrototypeRpgRunResultSnapshot();
    public PrototypeRpgCombatContributionSnapshot CombatContributionSnapshot = new PrototypeRpgCombatContributionSnapshot();
    public PrototypeRpgProgressionPreviewSnapshot ProgressionPreviewSnapshot = new PrototypeRpgProgressionPreviewSnapshot();
}
