public sealed partial class StaticPlaceholderWorldView
{
    public BattleResult LatestBattleResult => BuildBattleResult();
    public PrototypeBattleResultSnapshot LatestBattleResultSnapshot => BuildBattleResultSnapshot();
    public PrototypeEnemyIntentSnapshot CurrentEnemyIntentSnapshot => BuildCurrentEnemyIntentSnapshotView();
    public PrototypeBattleEventRecord[] RecentBattleEventSnapshotRecords => GetRecentBattleEventRecords();
    public PrototypeBattleEventRecord LatestBattleEventSnapshot => GetLatestBattleEventRecord();

    public PrototypeRpgMemberContributionSnapshot GetLatestRpgMemberContributionSnapshot(int memberIndex)
    {
        PrototypeRpgCombatContributionSnapshot snapshot = LatestRpgCombatContributionSnapshot;
        PrototypeRpgMemberContributionSnapshot[] members = snapshot != null && snapshot.Members != null
            ? snapshot.Members
            : System.Array.Empty<PrototypeRpgMemberContributionSnapshot>();

        if (memberIndex < 0 || memberIndex >= members.Length)
        {
            return new PrototypeRpgMemberContributionSnapshot();
        }

        return CopyRpgMemberContributionSnapshot(members[memberIndex]);
    }

    public PrototypeRpgMemberProgressionSeed GetLatestRpgMemberProgressionSeed(int memberIndex)
    {
        PrototypeRpgProgressionSeedSnapshot snapshot = LatestRpgProgressionSeedSnapshot;
        PrototypeRpgMemberProgressionSeed[] members = snapshot != null && snapshot.Members != null
            ? snapshot.Members
            : System.Array.Empty<PrototypeRpgMemberProgressionSeed>();

        if (memberIndex < 0 || memberIndex >= members.Length)
        {
            return new PrototypeRpgMemberProgressionSeed();
        }

        return CopyRpgMemberProgressionSeed(members[memberIndex]);
    }

    public PrototypeRpgMemberProgressPreview GetLatestRpgMemberProgressPreview(int memberIndex)
    {
        PrototypeRpgProgressionPreviewSnapshot snapshot = LatestRpgProgressionPreviewSnapshot;
        PrototypeRpgMemberProgressPreview[] members = snapshot != null && snapshot.Members != null
            ? snapshot.Members
            : System.Array.Empty<PrototypeRpgMemberProgressPreview>();

        if (memberIndex < 0 || memberIndex >= members.Length)
        {
            return new PrototypeRpgMemberProgressPreview();
        }

        return CopyRpgMemberProgressPreview(members[memberIndex]);
    }
}
