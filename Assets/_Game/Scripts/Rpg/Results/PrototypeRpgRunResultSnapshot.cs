using System;

public sealed class PrototypeRpgRunResultSnapshot
{
    public string ResultStateKey = string.Empty;
    public string DungeonId = string.Empty;
    public string DungeonLabel = string.Empty;
    public string RouteId = string.Empty;
    public string RouteLabel = string.Empty;
    public int TotalTurnsTaken;
    public string ResultSummary = string.Empty;
    public string SurvivingMembersSummary = string.Empty;
    public string BattleContextSummaryText = string.Empty;
    public string EncounterRequestSummaryText = string.Empty;
    public string BattleRuntimeSummaryText = string.Empty;
    public string BattleRuleSummaryText = string.Empty;
    public string BattleResultCoreSummaryText = string.Empty;
    public string BattleAbsorbSummaryText = string.Empty;
    public string NotableBattleEventsSummaryText = string.Empty;
    public string GearRewardCandidateSummaryText = string.Empty;
    public string EquipSwapChoiceSummaryText = string.Empty;
    public string GearCarryContinuitySummaryText = string.Empty;
    public string PendingRewardSummaryText = string.Empty;
    public PrototypeRpgPartyOutcomeSnapshot PartyOutcome = new PrototypeRpgPartyOutcomeSnapshot();
    public PrototypeRpgLootOutcomeSnapshot LootOutcome = new PrototypeRpgLootOutcomeSnapshot();
    public PrototypeRpgEliteOutcomeSnapshot EliteOutcome = new PrototypeRpgEliteOutcomeSnapshot();
    public PrototypeRpgEncounterOutcomeSnapshot EncounterOutcome = new PrototypeRpgEncounterOutcomeSnapshot();
    public PrototypeRpgRewardBundle[] PendingRewardBundles = Array.Empty<PrototypeRpgRewardBundle>();
}

public sealed class PrototypeRpgPartyOutcomeSnapshot
{
    public string PartyConditionText = string.Empty;
    public string PartyHpSummaryText = string.Empty;
    public string PartyMembersAtEndSummary = string.Empty;
    public int SurvivingMemberCount;
    public int KnockedOutMemberCount;
    public PrototypeRpgPartyMemberOutcomeSnapshot[] Members = Array.Empty<PrototypeRpgPartyMemberOutcomeSnapshot>();
}

public sealed class PrototypeRpgPartyMemberOutcomeSnapshot
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
    public string ResolvedSkillName = string.Empty;
    public string ResolvedSkillShortText = string.Empty;
    public string EquipmentSummaryText = string.Empty;
    public string GearContributionSummaryText = string.Empty;
    public string AppliedProgressionSummaryText = string.Empty;
    public string CurrentRunSummaryText = string.Empty;
    public string NextRunPreviewSummaryText = string.Empty;
    public int Level = 1;
    public int Experience;
    public int NextLevelExperience = 18;
    public int GrowthBonusMaxHp;
    public int GrowthBonusAttack;
    public int GrowthBonusDefense;
    public int GrowthBonusSpeed;
    public int CurrentHp;
    public int MaxHp = 1;
    public bool Survived;
    public bool KnockedOut;
}

public sealed class PrototypeRpgLootOutcomeSnapshot
{
    public int TotalLootGained;
    public int BattleLootGained;
    public int ChestLootGained;
    public int EventLootGained;
    public int EliteRewardAmount;
    public int EliteBonusRewardAmount;
    public string FinalLootSummary = string.Empty;
}

public sealed class PrototypeRpgEliteOutcomeSnapshot
{
    public bool IsEliteDefeated;
    public string EliteName = string.Empty;
    public string EliteTypeLabel = string.Empty;
    public string EliteRewardLabel = string.Empty;
    public int EliteRewardAmount;
    public bool EliteBonusRewardEarned;
    public int EliteBonusRewardAmount;
}

public sealed class PrototypeRpgEncounterOutcomeSnapshot
{
    public int ClearedEncounterCount;
    public string ClearedEncounterSummary = string.Empty;
    public int OpenedChestCount;
    public string RoomPathSummary = string.Empty;
    public string SelectedEventChoice = string.Empty;
    public string SelectedPreEliteChoice = string.Empty;
    public int PreEliteHealAmount;
}

