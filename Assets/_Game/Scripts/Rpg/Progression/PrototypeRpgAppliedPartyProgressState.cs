using System;

public sealed class PrototypeRpgAppliedPartyProgressState
{
    public string PartyId = string.Empty;
    public string DisplayName = string.Empty;
    public string LastCommittedRunIdentity = string.Empty;
    public string LastCommittedResultStateKey = string.Empty;
    public string AppliedSummaryText = string.Empty;
    public string OfferContinuitySummaryText = string.Empty;
    public PrototypeRpgAppliedPartyMemberProgressState[] Members = Array.Empty<PrototypeRpgAppliedPartyMemberProgressState>();
}

public sealed class PrototypeRpgAppliedPartyMemberProgressState
{
    public string MemberId = string.Empty;
    public string DisplayName = string.Empty;
    public string AppliedGrowthTrackId = string.Empty;
    public string AppliedJobId = string.Empty;
    public string AppliedPassiveSkillId = string.Empty;
    public string AppliedEquipmentLoadoutId = string.Empty;
    public string AppliedSkillLoadoutId = string.Empty;
    public string LastAppliedOfferId = string.Empty;
    public string LastAppliedOfferGroupId = string.Empty;
    public string LastAppliedOfferTypeKey = string.Empty;
    public string LastUnlockedSpecializationKey = string.Empty;
    public string LastAppliedRunIdentity = string.Empty;
    public string LastAppliedSummaryText = string.Empty;
    public string SpecializationSummaryText = string.Empty;
    public string PassiveSkillSlotSummaryText = string.Empty;
    public string RuntimeSynergySummaryText = string.Empty;
    public string NextRunSpecializationPreviewText = string.Empty;
    public string[] UnlockedSpecializationKeys = Array.Empty<string>();
}


