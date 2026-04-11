public sealed partial class StaticPlaceholderWorldView
{
    public CityPartyRosterSurfaceData BuildSelectedCityPartyRosterSurfaceData()
    {
        CityPartyRosterSurfaceData data = new CityPartyRosterSurfaceData();
        string cityId = ResolveDispatchBriefingCityId();
        if (string.IsNullOrEmpty(cityId) || _runtimeEconomyState == null)
        {
            return data;
        }

        data.CityId = cityId;
        data.CityLabel = ResolveDispatchEntityDisplayName(cityId);
        data.TotalPartyCount = _runtimeEconomyState.GetPartyCountInCity(cityId);
        data.IdlePartyCount = _runtimeEconomyState.GetIdlePartyCountInCity(cityId);
        data.ActiveExpeditionCount = _runtimeEconomyState.GetActiveExpeditionCountFromCity(cityId);
        data.ReadyPartyId = _runtimeEconomyState.GetIdlePartyIdInCity(cityId);
        data.HasReadyParty = !string.IsNullOrEmpty(data.ReadyPartyId);
        data.ReadyPartyLabel = string.IsNullOrEmpty(data.ReadyPartyId) ? "None" : data.ReadyPartyId;
        data.ReadyPartyPower = _runtimeEconomyState.GetReadyPartyPowerForCity(cityId);
        data.ReadyPartyCarryCapacity = _runtimeEconomyState.GetReadyPartyCarryCapacityForCity(cityId);
        data.ReadyPartyRoleSummaryText = string.IsNullOrEmpty(data.ReadyPartyId)
            ? "None"
            : ResolveDispatchPartyRoleSummary(data.ReadyPartyId);
        data.ReadyPartyLoadoutSummaryText = BuildPartyLoadoutSummaryText(data.ReadyPartyId);
        data.ActivePartyStatusText = _runtimeEconomyState.GetActivePartyStatusTextForCity(cityId);

        if (data.TotalPartyCount <= 0)
        {
            data.AvailabilitySummaryText = "No party recruited for this city yet.";
        }
        else if (data.IdlePartyCount > 0)
        {
            data.AvailabilitySummaryText = data.ReadyPartyLabel +
                " ready | Idle " + data.IdlePartyCount +
                " | Active " + data.ActiveExpeditionCount;
        }
        else
        {
            data.AvailabilitySummaryText = "No idle party available | Active " + data.ActiveExpeditionCount;
        }

        return data;
    }
}
