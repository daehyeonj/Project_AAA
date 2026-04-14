public sealed class WorldDelta
{
    public string SourceCityId = string.Empty;
    public string SourceCityLabel = "None";
    public string TargetDungeonId = string.Empty;
    public string TargetDungeonLabel = "None";
    public string RewardResourceId = string.Empty;
    public string ResultStateKey = string.Empty;
    public bool Success;
    public int AddedResourceAmount;
    public int ExpeditionSuccessCountDelta;
    public int ExpeditionFailureCountDelta;
    public bool ReturnPartyToIdle = true;
    public string ResultSummaryText = "None";
    public string LootSummaryText = "None";
    public string SurvivingMembersSummaryText = "None";
    public string ClearedEncountersSummaryText = "None";
    public string EventChoiceSummaryText = "None";
    public string LootBreakdownSummaryText = "None";
    public string RouteSummaryText = "None";
    public string DungeonSummaryText = "None";
    public string OutcomeMeaningId = string.Empty;
    public string OutcomeRewardMeaningText = "None";
    public string CityImpactMeaningText = "None";
    public string RecommendationShiftText = "None";
    public string CityStatusChangeSummaryText = "None";
    public string ExpeditionLogEntryText = "None";
}
