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
        if (handoffInput != null)
        {
            handoffInput.ProgressionOutput = progressionOutput ?? new PostRunProgressionOutput();
        }

        _latestRpgProgressionSeedSnapshot = progressionOutput != null
            ? progressionOutput.ProgressionSeed ?? new PrototypeRpgProgressionSeedSnapshot()
            : new PrototypeRpgProgressionSeedSnapshot();
        _latestRpgCombatContributionSnapshot = progressionOutput != null
            ? progressionOutput.CombatContribution ?? new PrototypeRpgCombatContributionSnapshot()
            : new PrototypeRpgCombatContributionSnapshot();
        _latestRpgProgressionPreviewSnapshot = progressionOutput != null
            ? progressionOutput.ProgressionPreview ?? new PrototypeRpgProgressionPreviewSnapshot()
            : new PrototypeRpgProgressionPreviewSnapshot();
        ApplyProgressionOutputToLatestRunResult(progressionOutput);
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
        ExpeditionRunState runState = BuildExpeditionRunStateInternal();
        ExpeditionPrepSurfaceData prepSurface = BuildSelectedExpeditionPrepSurfaceData();
        input.MissionObjectiveText = IsMeaningfulSnapshotText(runState != null ? runState.ObjectiveText : string.Empty)
            ? runState.ObjectiveText
            : IsMeaningfulSnapshotText(prepSurface != null ? prepSurface.BoardSummaryText : string.Empty)
                ? prepSurface.BoardSummaryText
                : "None";
        input.MissionRelevanceText = IsMeaningfulSnapshotText(runState != null ? runState.ExpectedUsefulnessText : string.Empty)
            ? runState.ExpectedUsefulnessText
            : IsMeaningfulSnapshotText(prepSurface != null ? prepSurface.ProjectedOutcomeSummaryText : string.Empty)
                ? prepSurface.ProjectedOutcomeSummaryText
                : IsMeaningfulSnapshotText(runState != null ? runState.WhyNowText : string.Empty)
                    ? runState.WhyNowText
                    : IsMeaningfulSnapshotText(prepSurface != null ? prepSurface.RecommendationReasonText : string.Empty)
                        ? prepSurface.RecommendationReasonText
                        : "None";
        input.RiskRewardContextText = IsMeaningfulSnapshotText(runState != null ? runState.RiskRewardPreviewText : string.Empty)
            ? runState.RiskRewardPreviewText
            : IsMeaningfulSnapshotText(prepSurface != null ? prepSurface.RoutePreviewSummaryText : string.Empty)
                ? prepSurface.RoutePreviewSummaryText
                : "None";
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

    private void ApplyProgressionOutputToLatestRunResult(PostRunProgressionOutput progressionOutput)
    {
        if (_latestRpgRunResultSnapshot == null || progressionOutput == null)
        {
            return;
        }

        _latestRpgRunResultSnapshot.PendingRewardSummaryText = string.IsNullOrEmpty(progressionOutput.PendingRewardSummaryText)
            ? string.Empty
            : progressionOutput.PendingRewardSummaryText;
        _latestRpgRunResultSnapshot.PendingRewardBundles = progressionOutput.PendingRewardBundles ?? System.Array.Empty<PrototypeRpgRewardBundle>();

        PrototypeRpgPartyOutcomeSnapshot partyOutcome = _latestRpgRunResultSnapshot.PartyOutcome ?? new PrototypeRpgPartyOutcomeSnapshot();
        PrototypeRpgPartyMemberOutcomeSnapshot[] members = partyOutcome.Members ?? System.Array.Empty<PrototypeRpgPartyMemberOutcomeSnapshot>();
        PrototypeRpgMemberProgressionResult[] results = progressionOutput.MemberProgressionResults ?? System.Array.Empty<PrototypeRpgMemberProgressionResult>();
        for (int i = 0; i < members.Length && i < results.Length; i++)
        {
            PrototypeRpgPartyMemberOutcomeSnapshot member = members[i] ?? new PrototypeRpgPartyMemberOutcomeSnapshot();
            PrototypeRpgMemberProgressionResult result = results[i] ?? new PrototypeRpgMemberProgressionResult();
            member.Level = result.LevelAfter > 0 ? result.LevelAfter : member.Level;
            member.Experience = result.ExperienceAfter > 0 ? result.ExperienceAfter : 0;
            member.NextLevelExperience = result.NextLevelExperience > 0 ? result.NextLevelExperience : member.NextLevelExperience;
            member.GrowthBonusMaxHp = result.GrowthBonusMaxHp;
            member.GrowthBonusAttack = result.GrowthBonusAttack;
            member.GrowthBonusDefense = result.GrowthBonusDefense;
            member.GrowthBonusSpeed = result.GrowthBonusSpeed;
            member.AppliedProgressionSummaryText = string.IsNullOrEmpty(result.GrowthSummaryText)
                ? member.AppliedProgressionSummaryText
                : "Level " + PrototypeRpgMemberProgressionRules.BuildLevelProgressText(member.Level, member.Experience, member.NextLevelExperience) +
                  " | Growth " + result.GrowthSummaryText;
            member.NextRunPreviewSummaryText = "Next Level " + PrototypeRpgMemberProgressionRules.BuildNextLevelHintText(
                member.Level,
                member.Experience,
                member.NextLevelExperience) +
                (string.IsNullOrEmpty(result.RewardDropSummaryText) || result.RewardDropSummaryText == "None"
                    ? string.Empty
                    : " | Loot " + result.RewardDropSummaryText);
            members[i] = member;
        }

        partyOutcome.Members = members;
        _latestRpgRunResultSnapshot.PartyOutcome = partyOutcome;
    }
}
