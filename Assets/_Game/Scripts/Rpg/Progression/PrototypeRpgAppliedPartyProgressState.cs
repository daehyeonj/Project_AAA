using System;

public sealed class PrototypeRpgAppliedPartyProgressState
{
    public string SessionKey = string.Empty;
    public string PartyId = string.Empty;
    public string HomeCityId = string.Empty;
    public bool HasAppliedProgress;
    public string LastAppliedRunIdentity = string.Empty;
    public string LastResultStateKey = string.Empty;
    public string SummaryText = string.Empty;
    public PrototypeRpgAppliedPartyMemberProgressState[] Members = Array.Empty<PrototypeRpgAppliedPartyMemberProgressState>();
}

public sealed class PrototypeRpgAppliedPartyMemberProgressState
{
    public string MemberId = string.Empty;
    public string DisplayName = string.Empty;
    public string RoleTag = string.Empty;
    public string AppliedGrowthTrackId = string.Empty;
    public string AppliedJobId = string.Empty;
    public string AppliedEquipmentLoadoutId = string.Empty;
    public string AppliedSkillLoadoutId = string.Empty;
    public string AppliedRoleLabel = string.Empty;
    public string AppliedDefaultSkillId = string.Empty;
    public string AppliedSkillName = string.Empty;
    public string AppliedSkillShortText = string.Empty;
    public int MaxHpModifier;
    public int AttackModifier;
    public int DefenseModifier;
    public int SpeedModifier;
    public string RecentAppliedOfferId = string.Empty;
    public string RecentAppliedOfferType = string.Empty;
    public string PendingApplyKey = string.Empty;
    public string RecentAppliedSummaryText = string.Empty;
    public string LastAppliedRunIdentity = string.Empty;
    public bool HasAnyOverride;
}