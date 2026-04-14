// Neutral handoff input. Battle publishes BattleResult; downstream consumers own post-run derivations.
public sealed class PostRunResolutionInput
{
    public PrototypeDungeonRunResultContext CompatibilityRunResultContext = new PrototypeDungeonRunResultContext();
    public BattleResult BattleResult = new BattleResult();
    public PostRunProgressionInput ProgressionInput = new PostRunProgressionInput();
    public string SourceCityId = string.Empty;
    public string SourceCityLabel = "None";
    public string RewardResourceId = string.Empty;
    public bool Success;
    public int ReturnedLootAmount;
    public int ElapsedDays = 1;
}
