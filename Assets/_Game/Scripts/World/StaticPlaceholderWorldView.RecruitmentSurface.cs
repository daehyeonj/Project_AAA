public sealed partial class StaticPlaceholderWorldView
{
    public CityInteractionSurfaceData BuildSelectedCityInteractionSurfaceData()
    {
        CityHubSurfaceData cityHub = BuildSelectedCityHubSurfaceData();
        return cityHub != null && cityHub.CityInteraction != null
            ? cityHub.CityInteraction
            : new CityInteractionSurfaceData();
    }

    private CityInteractionSurfaceData BuildSelectedCityInteractionSurfaceData(
        WorldObservationSurfaceData observation,
        CityPartyRosterSurfaceData roster)
    {
        CityInteractionSurfaceData data = new CityInteractionSurfaceData();
        string cityId = ResolveDispatchBriefingCityId();
        if (string.IsNullOrEmpty(cityId) || _runtimeEconomyState == null)
        {
            return data;
        }

        CityPartyRosterSurfaceData safeRoster = roster ?? new CityPartyRosterSurfaceData();
        ExpeditionPrepSurfaceData prep = observation != null ? observation.ExpeditionPrep : new ExpeditionPrepSurfaceData();
        data.CanRecruitParty = safeRoster.TotalPartyCount == 0;
        data.CanOpenPartyRoster = true;
        data.CanOpenExpeditionPrep = prep != null && prep.CanOpenBoard;
        data.BlockedReasonText = data.CanOpenExpeditionPrep
            ? "None"
            : prep != null && IsMeaningfulSnapshotText(prep.BlockedReasonText)
                ? prep.BlockedReasonText
                : BuildCurrentBlockedLaunchReasonSummaryText();
        data.RecruitmentActionText = data.CanRecruitParty
            ? "CityHub can recruit the first party for this city."
            : "Recruitment is blocked while the current city party still exists.";
        data.PartyRosterActionText = prep != null && IsMeaningfulSnapshotText(prep.StagedPartySummaryText)
            ? prep.StagedPartySummaryText
            : safeRoster.AvailabilitySummaryText;
        data.InteractionSummaryText = data.CanOpenExpeditionPrep
            ? "CityInteraction can open the party roster and hand this city context to ExpeditionPrep."
            : prep != null && IsMeaningfulSnapshotText(prep.RecommendedNextActionText)
                ? prep.RecommendedNextActionText
                : "CityInteraction can open the party roster, but ExpeditionPrep is waiting for a ready party and linked dungeon.";
        return data;
    }
}
