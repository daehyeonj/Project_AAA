using System;

public sealed class PrototypeRpgOfferExposureHistoryState
{
    public string SessionKey = string.Empty;
    public string PartyId = string.Empty;
    public string DisplayName = string.Empty;
    public string LastRunIdentity = string.Empty;
    public string SummaryText = string.Empty;
    public PrototypeRpgMemberOfferExposureState[] Members = Array.Empty<PrototypeRpgMemberOfferExposureState>();
}

public sealed class PrototypeRpgMemberOfferExposureState
{
    public string MemberId = string.Empty;
    public string DisplayName = string.Empty;
    public string[] RecentlyOfferedIds = Array.Empty<string>();
    public string[] RecentlyOfferedGroupKeys = Array.Empty<string>();
    public string[] RecentlyOfferedTypeKeys = Array.Empty<string>();
    public string RecentRunIdentity = string.Empty;
    public int RecentOfferCount;
    public string RecentSelectedOfferId = string.Empty;
    public string ExposureSummaryText = string.Empty;
}
