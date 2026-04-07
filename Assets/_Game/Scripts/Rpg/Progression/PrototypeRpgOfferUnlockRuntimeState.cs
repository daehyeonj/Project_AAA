using System;

public sealed class PrototypeRpgOfferUnlockRuntimeState
{
    public string SessionKey = string.Empty;
    public string PartyId = string.Empty;
    public string DisplayName = string.Empty;
    public string LastRunIdentity = string.Empty;
    public string OfferUnlockSummaryText = string.Empty;
    public string TierEscalationSummaryText = string.Empty;
    public string PoolExpansionSummaryText = string.Empty;
    public PrototypeRpgMemberOfferUnlockState[] Members = Array.Empty<PrototypeRpgMemberOfferUnlockState>();
}

public sealed class PrototypeRpgMemberOfferUnlockState
{
    public string MemberId = string.Empty;
    public string DisplayName = string.Empty;
    public string[] UnlockedGroupKeys = Array.Empty<string>();
    public string[] UnlockedTierKeys = Array.Empty<string>();
    public string[] UnlockedOfferIds = Array.Empty<string>();
    public string RecentUnlockReasonText = string.Empty;
    public string LastUnlockRunIdentity = string.Empty;
    public int UnlockCount;
    public string LastEscalatedTierKey = string.Empty;
    public string UnlockSummaryText = string.Empty;
}

public sealed class PrototypeRpgOfferPoolTierState
{
    public string TierKey = string.Empty;
    public string SummaryText = string.Empty;
    public bool IsUnlocked;
}
