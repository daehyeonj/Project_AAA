using System;

public sealed class PrototypeRpgProgressionSeedSnapshot
{
    public string ResultStateKey = string.Empty;
    public string DungeonId = string.Empty;
    public string DungeonLabel = string.Empty;
    public string DungeonDangerLabel = string.Empty;
    public string RouteId = string.Empty;
    public string RouteLabel = string.Empty;
    public string RouteRiskLabel = string.Empty;
    public int TotalTurnsTaken;
    public int ClearedEncounterCount;
    public bool EliteDefeated;
    public string EliteName = string.Empty;
    public string EliteTypeLabel = string.Empty;
    public string PartyConditionText = string.Empty;
    public int SurvivingMemberCount;
    public int KnockedOutMemberCount;
    public string EliteClearBonusHint = string.Empty;
    public string RouteRiskHint = string.Empty;
    public string DungeonDangerHint = string.Empty;
    public PrototypeRpgLootSeed Loot = new PrototypeRpgLootSeed();
    public PrototypeRpgMemberProgressionSeed[] Members = Array.Empty<PrototypeRpgMemberProgressionSeed>();
    public PrototypeRpgUnlockSeedSnapshot[] UnlockSeeds = Array.Empty<PrototypeRpgUnlockSeedSnapshot>();
    public string[] RewardTags = Array.Empty<string>();
    public string[] GrowthTags = Array.Empty<string>();
}

public sealed class PrototypeRpgMemberProgressionSeed
{
    public string MemberId = string.Empty;
    public string DisplayName = string.Empty;
    public string RoleTag = string.Empty;
    public string RoleLabel = string.Empty;
    public string DefaultSkillId = string.Empty;
    public string GrowthTrackId = string.Empty;
    public string JobId = string.Empty;
    public string EquipmentLoadoutId = string.Empty;
    public string SkillLoadoutId = string.Empty;
    public bool Survived;
    public bool KnockedOut;
    public int CurrentHp;
    public int MaxHp = 1;
    public PrototypeRpgCombatContributionSeed Combat = new PrototypeRpgCombatContributionSeed();
}

public sealed class PrototypeRpgCombatContributionSeed
{
    public int DamageDealt;
    public int DamageTaken;
    public int HealingDone;
    public int ActionCount;
    public int KillCount;
}

public sealed class PrototypeRpgLootSeed
{
    public int TotalLootGained;
    public int BattleLootGained;
    public int ChestLootGained;
    public int EventLootGained;
    public int EliteRewardAmount;
    public int EliteBonusRewardAmount;
    public int PendingBonusRewardLostAmount;
    public string CarryoverHintText = string.Empty;
    public string LootBreakdownSummary = string.Empty;
}

public sealed class PrototypeRpgUnlockSeedSnapshot
{
    public string SourceRunIdentity = string.Empty;
    public string SourceEncounterId = string.Empty;
    public string DungeonId = string.Empty;
    public string RouteId = string.Empty;
    public string EliteId = string.Empty;
    public string UnlockCategoryKey = string.Empty;
    public string UnlockTargetKey = string.Empty;
    public string UnlockReasonText = string.Empty;
    public string ProgressionDependencyHint = string.Empty;
    public bool IsPreviewOnly = true;
}

public sealed class PrototypeRpgCombatContributionSnapshot
{
    public string ResultStateKey = string.Empty;
    public string DungeonId = string.Empty;
    public string DungeonLabel = string.Empty;
    public string RouteId = string.Empty;
    public string RouteLabel = string.Empty;
    public int TotalTurnsTaken;
    public int SurvivingMemberCount;
    public int KnockedOutMemberCount;
    public int TotalDamageDealt;
    public int TotalDamageTaken;
    public int TotalHealingDone;
    public PrototypeRpgMemberContributionSnapshot[] Members = Array.Empty<PrototypeRpgMemberContributionSnapshot>();
    public PrototypeBattleEventRecord[] RecentEvents = Array.Empty<PrototypeBattleEventRecord>();
}

public sealed class PrototypeRpgMemberContributionSnapshot
{
    public string MemberId = string.Empty;
    public string DisplayName = string.Empty;
    public string RoleTag = string.Empty;
    public string RoleLabel = string.Empty;
    public string DefaultSkillId = string.Empty;
    public int DamageDealt;
    public int DamageTaken;
    public int HealingDone;
    public int ActionCount;
    public int KillCount;
    public bool KnockedOut;
    public bool Survived;
    public bool EliteVictor;
    public string ContributionSummaryText = string.Empty;
}

public sealed class PrototypeRpgProgressionPreviewSnapshot
{
    public string ResultStateKey = string.Empty;
    public string DungeonId = string.Empty;
    public string DungeonLabel = string.Empty;
    public string DungeonDangerLabel = string.Empty;
    public string RouteId = string.Empty;
    public string RouteLabel = string.Empty;
    public string RouteRiskLabel = string.Empty;
    public bool EliteDefeated;
    public int TotalLootGained;
    public bool HasAppliedProgress;
    public string AppliedProgressSummaryText = string.Empty;
    public string AppliedLastRunIdentity = string.Empty;
    public string ProgressionPreviewText = string.Empty;
    public string UnlockPreviewSummaryText = string.Empty;
    public string RewardCarryoverHintText = string.Empty;
    public string GrowthCandidateSummaryText = string.Empty;
    public string UpgradeCandidateSummaryText = string.Empty;
    public string UpgradeOfferSummaryText = string.Empty;
    public string ApplyReadySummaryText = string.Empty;
    public string[] RewardHintTags = Array.Empty<string>();
    public string[] GrowthHintTags = Array.Empty<string>();
    public PrototypeRpgUnlockPreviewSnapshot[] UnlockPreviews = Array.Empty<PrototypeRpgUnlockPreviewSnapshot>();
    public PrototypeRpgMemberProgressPreview[] Members = Array.Empty<PrototypeRpgMemberProgressPreview>();
}

public sealed class PrototypeRpgUnlockPreviewSnapshot
{
    public string SourceRunIdentity = string.Empty;
    public string SourceEncounterId = string.Empty;
    public string DungeonId = string.Empty;
    public string RouteId = string.Empty;
    public string EliteId = string.Empty;
    public string UnlockCategoryKey = string.Empty;
    public string UnlockTargetKey = string.Empty;
    public string UnlockReasonText = string.Empty;
    public string ProgressionDependencyHint = string.Empty;
    public string DisplayLabel = string.Empty;
    public string SummaryText = string.Empty;
    public bool IsPreviewOnly = true;
}

public sealed class PrototypeRpgGrowthPathCandidate
{
    public string CandidateKey = string.Empty;
    public string CandidateTypeKey = string.Empty;
    public string SourceHookId = string.Empty;
    public string CandidateTargetId = string.Empty;
    public string PreviewLabel = string.Empty;
    public string PreviewText = string.Empty;
    public string RecommendedBecauseText = string.Empty;
    public int Priority;
    public bool AvailableLater = true;
    public string BlockedReasonHint = string.Empty;
}

public sealed class PrototypeRpgUpgradeCandidate
{
    public string CandidateKey = string.Empty;
    public string CandidateTypeKey = string.Empty;
    public string SourceHookId = string.Empty;
    public string CandidateTargetId = string.Empty;
    public string PreviewLabel = string.Empty;
    public string PreviewText = string.Empty;
    public string RecommendedBecauseText = string.Empty;
    public int Priority;
    public bool AvailableLater = true;
    public string BlockedReasonHint = string.Empty;
}

public sealed class PrototypeRpgMemberProgressPreview
{
    public string MemberId = string.Empty;
    public string DisplayName = string.Empty;
    public string RoleTag = string.Empty;
    public string RoleLabel = string.Empty;
    public string GrowthTrackId = string.Empty;
    public string JobId = string.Empty;
    public string EquipmentLoadoutId = string.Empty;
    public string SkillLoadoutId = string.Empty;
    public bool Survived;
    public PrototypeRpgMemberContributionSnapshot Contribution = new PrototypeRpgMemberContributionSnapshot();
    public string[] SuggestedGrowthHintTags = Array.Empty<string>();
    public string[] SuggestedRewardHintTags = Array.Empty<string>();
    public string NotableOutcomeKey = string.Empty;
    public bool HasAppliedProgress;
    public string AppliedProgressSummaryText = string.Empty;
    public string AppliedRoleLabel = string.Empty;
    public string AppliedDefaultSkillId = string.Empty;
    public string PreviewSummaryText = string.Empty;
    public string GrowthDirectionLabel = string.Empty;
    public string RewardCarryoverHintText = string.Empty;
    public string UpgradeCandidateSummaryText = string.Empty;
    public string UpgradeOfferSummaryText = string.Empty;
    public string ApplyReadySummaryText = string.Empty;
    public string NextGrowthTrackHint = string.Empty;
    public string NextJobHint = string.Empty;
    public string NextSkillLoadoutHint = string.Empty;
    public string NextEquipmentLoadoutHint = string.Empty;
    public PrototypeRpgGrowthPathCandidate[] GrowthPathCandidates = Array.Empty<PrototypeRpgGrowthPathCandidate>();
    public PrototypeRpgUpgradeCandidate[] UpgradeCandidates = Array.Empty<PrototypeRpgUpgradeCandidate>();
}