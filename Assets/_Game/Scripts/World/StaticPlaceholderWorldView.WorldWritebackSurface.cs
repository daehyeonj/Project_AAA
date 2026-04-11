public sealed partial class StaticPlaceholderWorldView
{
    private sealed class PrototypeWorldWritebackObservationSurfaceData
    {
        public string WritebackText = "None";
        public string DungeonStatusText = "None";
        public string DungeonAvailabilityText = "None";
    }

    public string SelectedWorldWritebackText => BuildSelectedWorldWritebackObservationSurfaceData().WritebackText;
    public string SelectedDungeonStatusText => BuildSelectedWorldWritebackObservationSurfaceData().DungeonStatusText;
    public string SelectedDungeonAvailabilityText => BuildSelectedWorldWritebackObservationSurfaceData().DungeonAvailabilityText;
    public string WorldWritebackSummaryText => BuildLatestWorldWritebackObservationSurfaceData().WritebackText;
    public string RecentWorldWritebackLog1Text => _runtimeEconomyState != null ? _runtimeEconomyState.GetRecentWorldWritebackLogText(0) : "None";
    public string RecentWorldWritebackLog2Text => _runtimeEconomyState != null ? _runtimeEconomyState.GetRecentWorldWritebackLogText(1) : "None";
    public string RecentWorldWritebackLog3Text => _runtimeEconomyState != null ? _runtimeEconomyState.GetRecentWorldWritebackLogText(2) : "None";

    private PrototypeWorldWritebackObservationSurfaceData BuildSelectedWorldWritebackObservationSurfaceData()
    {
        if (_selectedMarker == null)
        {
            return BuildLatestWorldWritebackObservationSurfaceData();
        }

        string cityId = _selectedMarker.EntityData.Kind == WorldEntityKind.City
            ? _selectedMarker.EntityData.Id
            : string.Empty;
        string dungeonId = _selectedMarker.EntityData.Kind == WorldEntityKind.Dungeon
            ? _selectedMarker.EntityData.Id
            : _selectedMarker.EntityData.Kind == WorldEntityKind.City
                ? GetLinkedDungeonIdForCity(cityId)
                : string.Empty;
        return BuildObservationSurfaceData(cityId, dungeonId, false);
    }

    private PrototypeWorldWritebackObservationSurfaceData BuildLatestWorldWritebackObservationSurfaceData()
    {
        string cityId = _runtimeEconomyState != null ? _runtimeEconomyState.GetLatestWorldWritebackCityId() : string.Empty;
        string dungeonId = _runtimeEconomyState != null ? _runtimeEconomyState.GetLatestWorldWritebackDungeonId() : string.Empty;
        return BuildObservationSurfaceData(cityId, dungeonId, true);
    }

    private PrototypeWorldWritebackObservationSurfaceData BuildObservationSurfaceData(string cityId, string dungeonId, bool allowWorldFallback)
    {
        PrototypeWorldWritebackObservationSurfaceData data = new PrototypeWorldWritebackObservationSurfaceData();
        if (_runtimeEconomyState == null)
        {
            return data;
        }

        if (!string.IsNullOrEmpty(cityId))
        {
            data.WritebackText = _runtimeEconomyState.GetLatestWorldWritebackSummaryForCity(cityId);
        }

        if ((string.IsNullOrEmpty(data.WritebackText) || data.WritebackText == "None") && !string.IsNullOrEmpty(dungeonId))
        {
            data.WritebackText = _runtimeEconomyState.GetLatestWorldWritebackSummaryForDungeon(dungeonId);
        }

        if (allowWorldFallback && (string.IsNullOrEmpty(data.WritebackText) || data.WritebackText == "None"))
        {
            data.WritebackText = _runtimeEconomyState.GetLatestWorldWritebackSummary();
        }

        if (!string.IsNullOrEmpty(dungeonId))
        {
            data.DungeonStatusText = _runtimeEconomyState.GetDungeonWorldStatusText(dungeonId);
            data.DungeonAvailabilityText = _runtimeEconomyState.GetDungeonWorldAvailabilityText(dungeonId);
        }

        return data;
    }
}
