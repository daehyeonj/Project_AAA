public sealed partial class StaticPlaceholderWorldView
{
    public BattleResult BuildBattleResult()
    {
        return BuildCurrentBattleResultContractView();
    }

    private BattleResult BuildCurrentBattleResultContractView()
    {
        return CopyRpgOwnedBattleResult(BuildRpgOwnedCurrentBattleResultContract());
    }

    public PrototypeBattleResultSnapshot BuildBattleResultSnapshot()
    {
        return BuildCurrentBattleResultSnapshotView();
    }

    private PrototypeBattleResultSnapshot BuildCurrentBattleResultSnapshotView()
    {
        return BuildRpgOwnedCurrentBattleResultSnapshotView();
    }

    private void UpdateBattleResultSnapshot(string outcomeKey)
    {
        UpdateRpgOwnedBattleResultSnapshot(outcomeKey);
    }

    private PrototypeBattleResultSnapshot BuildBattleResultSnapshot(string outcomeKey)
    {
        return BuildRpgOwnedBattleResultSnapshot(outcomeKey);
    }
}
