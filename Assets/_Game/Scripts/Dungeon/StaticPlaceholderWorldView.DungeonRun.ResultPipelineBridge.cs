public sealed partial class StaticPlaceholderWorldView
{
    private void CaptureDungeonRunResultSnapshotState(string outcomeKey, int safeReturnedLoot, string safeResultSummary)
    {
        _latestRpgRunResultSnapshot = BuildRpgRunResultSnapshot(outcomeKey, safeReturnedLoot, safeResultSummary);
        ApplyRpgRunResultSnapshotToLegacyFields(_latestRpgRunResultSnapshot);
        CaptureLatestRunResultContext(_latestRpgRunResultSnapshot);
    }

    // Legacy progression shells stay behind the handoff boundary until consumer-owned systems replace them.
    private void CaptureDungeonRunCompatibilityShellState(string outcomeKey, PostRunResolutionInput handoffInput)
    {
        PostRunProgressionOutput progressionOutput = ResultPipelineProgression.Build(handoffInput);
        _latestRpgProgressionSeedSnapshot = progressionOutput != null
            ? progressionOutput.ProgressionSeed ?? new PrototypeRpgProgressionSeedSnapshot()
            : new PrototypeRpgProgressionSeedSnapshot();
        _latestRpgCombatContributionSnapshot = progressionOutput != null
            ? progressionOutput.CombatContribution ?? new PrototypeRpgCombatContributionSnapshot()
            : new PrototypeRpgCombatContributionSnapshot();
        _latestRpgProgressionPreviewSnapshot = progressionOutput != null
            ? progressionOutput.ProgressionPreview ?? new PrototypeRpgProgressionPreviewSnapshot()
            : new PrototypeRpgProgressionPreviewSnapshot();
        UpdateBattleResultSnapshot(outcomeKey);
    }

    // Raw battle facts plus a compatibility shell are emitted here; writeback/progression remain downstream consumers.
    private PostRunResolutionInput BuildDungeonRunPostRunResolutionInput(bool success, int safeReturnedLoot)
    {
        PostRunResolutionInput input = new PostRunResolutionInput();
        input.CompatibilityRunResultContext = BuildDungeonRunResultContext(_latestRpgRunResultSnapshot);
        input.BattleResult = BuildBattleResult();
        input.ProgressionInput = BuildDungeonRunPostRunProgressionInput(input.CompatibilityRunResultContext, input.BattleResult);
        input.SourceCityId = string.IsNullOrEmpty(_currentHomeCityId) ? string.Empty : _currentHomeCityId;
        input.SourceCityLabel = GetHomeCityDisplayName();
        input.RewardResourceId = DungeonRewardResourceId;
        input.Success = success;
        input.ReturnedLootAmount = safeReturnedLoot > 0 ? safeReturnedLoot : 0;
        input.ElapsedDays = 1;
        return input;
    }

    private PostRunProgressionInput BuildDungeonRunPostRunProgressionInput(
        PrototypeDungeonRunResultContext compatibilityContext,
        BattleResult battleResult)
    {
        PostRunProgressionInput input = new PostRunProgressionInput();
        input.CompatibilityRunResultContext = compatibilityContext ?? new PrototypeDungeonRunResultContext();
        input.BattleResult = battleResult ?? new BattleResult();
        input.DungeonDangerLabel = GetCurrentDungeonDangerText();
        input.RouteRiskLabel = string.IsNullOrEmpty(_selectedRouteRiskLabel) ? string.Empty : _selectedRouteRiskLabel;
        input.TotalDamageDealt = _totalDamageDealt;
        input.TotalDamageTaken = _totalDamageTaken;
        input.TotalHealingDone = _totalHealingDone;
        input.MemberDamageDealt = _runMemberDamageDealt != null ? (int[])_runMemberDamageDealt.Clone() : System.Array.Empty<int>();
        input.MemberDamageTaken = _runMemberDamageTaken != null ? (int[])_runMemberDamageTaken.Clone() : System.Array.Empty<int>();
        input.MemberHealingDone = _runMemberHealingDone != null ? (int[])_runMemberHealingDone.Clone() : System.Array.Empty<int>();
        input.MemberActionCount = _runMemberActionCount != null ? (int[])_runMemberActionCount.Clone() : System.Array.Empty<int>();
        input.MemberKillCount = _runMemberKillCount != null ? (int[])_runMemberKillCount.Clone() : System.Array.Empty<int>();
        input.RecentEvents = BuildRecentBattleEventRecords();
        return input;
    }

    private void EmitDungeonRunPostRunHandoff(string outcomeKey, bool success, int safeReturnedLoot)
    {
        PostRunResolutionInput handoffInput = BuildDungeonRunPostRunResolutionInput(success, safeReturnedLoot);
        CaptureDungeonRunCompatibilityShellState(outcomeKey, handoffInput);
        ApplyDungeonRunResultPipelineWriteback(handoffInput);
        CaptureLatestRunResultContext(_latestRpgRunResultSnapshot);
    }
}
