public sealed class WorldWriteback
{
    public string RunResultStateKey = string.Empty;
    public bool Success;
    public string SourceCityId = string.Empty;
    public string SourceCityLabel = "None";
    public string TargetDungeonId = string.Empty;
    public string TargetDungeonLabel = "None";
    public string ChosenRouteId = string.Empty;
    public string ChosenRouteLabel = "None";
    public string RouteSummaryText = "None";
    public string DungeonSummaryText = "None";
    public string ResultSummaryText = "None";
    public string WritebackSummaryText = "None";
    public string DungeonStatusKey = string.Empty;
    public string DungeonStatusSummaryText = "None";
    public string DungeonAvailabilitySummaryText = "None";
    public string DungeonLastOutcomeSummaryText = "None";
    public string RewardResourceId = string.Empty;
    public int LootReturned;
    public string LootSummaryText = "None";
    public string SurvivingMembersSummaryText = "None";
    public string ClearedEncountersSummaryText = "None";
    public string EventChoiceSummaryText = "None";
    public string LootBreakdownSummaryText = "None";
    public string EconomySummaryText = "None";
    public int DayBefore;
    public int DayAfter;
    public int ElapsedDays;
    public string TimeAdvanceSummaryText = "None";
    public CityWriteback CityWriteback = new CityWriteback();
}
