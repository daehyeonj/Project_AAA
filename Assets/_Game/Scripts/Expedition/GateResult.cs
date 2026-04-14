public sealed class GateResult
{
    public bool CanLaunch;
    public bool IsBlocked;
    public string SummaryText = "None";
    public string RecommendedActionText = "None";
    public BlockedReason BlockedReason = new BlockedReason();
}
