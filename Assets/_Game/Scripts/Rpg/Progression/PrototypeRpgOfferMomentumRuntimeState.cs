using System;

public sealed class PrototypeRpgOfferMomentumRuntimeState
{
    public string SessionKey = string.Empty;
    public string PartyId = string.Empty;
    public string DisplayName = string.Empty;
    public string LastRunIdentity = string.Empty;
    public int RecentRunCount;
    public int ConsecutiveClearCount;
    public int ConsecutiveDefeatCount;
    public int ConsecutiveRetreatCount;
    public int RecentLowValueOfferCount;
    public int RecentHighImpactOfferCount;
    public int CurrentMomentumTier;
    public int ComebackReliefWindowCount;
    public int MomentumDampeningWeight;
    public string LastMomentumReasonKey = string.Empty;
    public string StreakSensitivePacingSummaryText = string.Empty;
    public string ClearStreakDampeningSummaryText = string.Empty;
    public string ComebackReliefSummaryText = string.Empty;
    public string MomentumBalancingSummaryText = string.Empty;
    public string CurrentMomentumTierSummaryText = string.Empty;
    public string NextOfferIntensityHintText = string.Empty;
    public PrototypeRpgMemberOfferMomentumBiasState[] Members = Array.Empty<PrototypeRpgMemberOfferMomentumBiasState>();
}

public sealed class PrototypeRpgMemberOfferMomentumBiasState
{
    public string MemberId = string.Empty;
    public string DisplayName = string.Empty;
    public int RecentRunCount;
    public int ConsecutiveClearCount;
    public int ConsecutiveDefeatCount;
    public int ConsecutiveRetreatCount;
    public int RecentLowValueOfferCount;
    public int RecentHighImpactOfferCount;
    public int CurrentMomentumTier;
    public int ComebackReliefWindowCount;
    public int MomentumDampeningWeight;
    public int IntensityBiasWeight;
    public int ComebackReliefBiasWeight;
    public string LastMomentumReasonKey = string.Empty;
    public string MomentumMemberSummaryText = string.Empty;
    public string LastResolvedRunIdentity = string.Empty;
}
