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
    public PrototypeRpgPartyOutcomeSnapshot PartyOutcome = new PrototypeRpgPartyOutcomeSnapshot();
    public PrototypeRpgLootOutcomeSnapshot LootOutcome = new PrototypeRpgLootOutcomeSnapshot();
    public PrototypeRpgEliteOutcomeSnapshot EliteOutcome = new PrototypeRpgEliteOutcomeSnapshot();
    public PrototypeRpgEncounterOutcomeSnapshot EncounterOutcome = new PrototypeRpgEncounterOutcomeSnapshot();
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
