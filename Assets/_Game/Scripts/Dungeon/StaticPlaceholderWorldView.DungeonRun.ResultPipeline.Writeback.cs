public sealed partial class StaticPlaceholderWorldView
{
    private ExpeditionResult BuildDungeonRunExpeditionResult(PostRunResolutionInput handoffInput)
    {
        return ResultPipeline.BuildExpeditionResult(handoffInput);
    }

    private void ApplyDungeonRunResultPipelineWriteback(PostRunResolutionInput handoffInput)
    {
        PostRunResolutionInput safeHandoffInput = handoffInput ?? new PostRunResolutionInput();
        int safeReturnedLoot = safeHandoffInput.ReturnedLootAmount > 0 ? safeHandoffInput.ReturnedLootAmount : 0;

        if (_runtimeEconomyState != null && !string.IsNullOrEmpty(_currentHomeCityId) && !string.IsNullOrEmpty(_currentDungeonId))
        {
            ExpeditionResult expeditionResult = BuildDungeonRunExpeditionResult(safeHandoffInput);
            _runtimeEconomyState.ResolveDungeonRun(expeditionResult);
            SyncRuntimeEquipmentWritebackToLatestRunResult(expeditionResult);
        }

        _resultStockBefore = _preRunManaShardStock;
        _resultStockAfter = string.IsNullOrEmpty(_currentHomeCityId) ? 0 : GetCityManaShardStock(_currentHomeCityId);
        _resultStockDelta = _resultStockAfter - _resultStockBefore;
        _resultNeedPressureBeforeText = string.IsNullOrEmpty(_preRunNeedPressureText) ? GetNeedPressureTextForStock(_resultStockBefore) : _preRunNeedPressureText;
        _resultNeedPressureAfterText = string.IsNullOrEmpty(_currentHomeCityId) ? "None" : GetNeedPressureTextForStock(_resultStockAfter);
        _resultDispatchReadinessBeforeText = string.IsNullOrEmpty(_preRunDispatchReadinessText) ? GetDispatchReadinessText(_currentHomeCityId) : _preRunDispatchReadinessText;
        _resultDispatchReadinessAfterText = string.IsNullOrEmpty(_currentHomeCityId) ? "None" : GetDispatchReadinessText(_currentHomeCityId);
        _resultConsecutiveDispatchBefore = _preRunConsecutiveDispatchCount;
        _resultConsecutiveDispatchAfter = string.IsNullOrEmpty(_currentHomeCityId) ? 0 : GetConsecutiveDispatchCount(_currentHomeCityId);
        _resultRecoveryEtaDays = string.IsNullOrEmpty(_currentHomeCityId) ? 0 : GetRecoveryDaysToReady(_currentHomeCityId);

        if (string.IsNullOrEmpty(_currentHomeCityId))
        {
            return;
        }

        CityWriteback cityWriteback = BuildDungeonRunResultCityWritebackPreviewSurface();
        OutcomeReadback outcomeReadback = BuildDungeonRunResultOutcomeReadbackPreviewSurface();
        _resultStockBefore = cityWriteback.StockBefore;
        _resultStockAfter = cityWriteback.StockAfter;
        _resultStockDelta = cityWriteback.StockDelta;
        _resultNeedPressureBeforeText = IsMeaningfulSnapshotText(cityWriteback.NeedPressureBeforeText)
            ? cityWriteback.NeedPressureBeforeText
            : _resultNeedPressureBeforeText;
        _resultNeedPressureAfterText = IsMeaningfulSnapshotText(cityWriteback.NeedPressureAfterText)
            ? cityWriteback.NeedPressureAfterText
            : _resultNeedPressureAfterText;
        _resultDispatchReadinessBeforeText = IsMeaningfulSnapshotText(cityWriteback.DispatchReadinessBeforeText)
            ? cityWriteback.DispatchReadinessBeforeText
            : _resultDispatchReadinessBeforeText;
        _resultDispatchReadinessAfterText = IsMeaningfulSnapshotText(cityWriteback.DispatchReadinessAfterText)
            ? cityWriteback.DispatchReadinessAfterText
            : _resultDispatchReadinessAfterText;
        _lastDispatchStockDeltaByCityId[_currentHomeCityId] = IsMeaningfulSnapshotText(outcomeReadback.StockDeltaText)
            ? outcomeReadback.StockDeltaText
            : BuildSignedLootAmountText(cityWriteback.StockDelta);
        _lastNeedPressureChangeByCityId[_currentHomeCityId] = BuildNeedPressureChangeSummary(
            cityWriteback.NeedPressureBeforeText,
            cityWriteback.NeedPressureAfterText);
        _lastDispatchReadinessChangeByCityId[_currentHomeCityId] = BuildDispatchReadinessChangeSummary(
            cityWriteback.DispatchReadinessBeforeText,
            cityWriteback.DispatchReadinessAfterText) +
            " | Streak " + _resultConsecutiveDispatchBefore + " -> " + _resultConsecutiveDispatchAfter;
        _lastDispatchImpactByCityId[_currentHomeCityId] = IsMeaningfulSnapshotText(outcomeReadback.LatestReturnAftermathText)
            ? outcomeReadback.LatestReturnAftermathText
            : BuildLastDispatchImpactSummary(
                cityWriteback.StockBefore,
                cityWriteback.StockAfter,
                cityWriteback.StockDelta,
                cityWriteback.NeedPressureBeforeText,
                cityWriteback.NeedPressureAfterText);
    }

    private void SyncRuntimeEquipmentWritebackToLatestRunResult(ExpeditionResult expeditionResult)
    {
        if (_latestRpgRunResultSnapshot == null || expeditionResult == null)
        {
            return;
        }

        _latestRpgRunResultSnapshot.GearRewardCandidateSummaryText = string.IsNullOrEmpty(expeditionResult.GearRewardCandidateSummaryText)
            ? string.Empty
            : expeditionResult.GearRewardCandidateSummaryText;
        _latestRpgRunResultSnapshot.EquipSwapChoiceSummaryText = string.IsNullOrEmpty(expeditionResult.EquipSwapChoiceSummaryText)
            ? string.Empty
            : expeditionResult.EquipSwapChoiceSummaryText;
        _latestRpgRunResultSnapshot.GearCarryContinuitySummaryText = string.IsNullOrEmpty(expeditionResult.GearCarryContinuitySummaryText)
            ? string.Empty
            : expeditionResult.GearCarryContinuitySummaryText;
        _latestRpgRunResultSnapshot.PendingRewardSummaryText = string.IsNullOrEmpty(expeditionResult.PendingRewardSummaryText)
            ? _latestRpgRunResultSnapshot.PendingRewardSummaryText
            : expeditionResult.PendingRewardSummaryText;

        string partyId = ResolveLatestRunResultPartyId();
        if (string.IsNullOrEmpty(partyId))
        {
            return;
        }

        PrototypeRpgPartyRuntimeResolveSurface runtimePartySurface = BuildRuntimePartyResolveSurface(partyId);
        if (runtimePartySurface == null || runtimePartySurface.Members == null || runtimePartySurface.Members.Length <= 0)
        {
            return;
        }

        PrototypeRpgPartyOutcomeSnapshot partyOutcome = _latestRpgRunResultSnapshot.PartyOutcome ?? new PrototypeRpgPartyOutcomeSnapshot();
        PrototypeRpgPartyMemberOutcomeSnapshot[] snapshotMembers = partyOutcome.Members ?? System.Array.Empty<PrototypeRpgPartyMemberOutcomeSnapshot>();
        if (snapshotMembers.Length <= 0)
        {
            snapshotMembers = new PrototypeRpgPartyMemberOutcomeSnapshot[runtimePartySurface.Members.Length];
        }

        for (int i = 0; i < runtimePartySurface.Members.Length; i++)
        {
            PrototypeRpgMemberRuntimeResolveSurface runtimeMember = runtimePartySurface.Members[i];
            if (runtimeMember == null)
            {
                continue;
            }

            int snapshotIndex = FindPartyOutcomeMemberIndex(snapshotMembers, runtimeMember.MemberId, i);
            PrototypeRpgPartyMemberOutcomeSnapshot snapshotMember = snapshotMembers[snapshotIndex] ?? new PrototypeRpgPartyMemberOutcomeSnapshot();
            snapshotMember.MemberId = string.IsNullOrEmpty(snapshotMember.MemberId) ? runtimeMember.MemberId : snapshotMember.MemberId;
            snapshotMember.DisplayName = string.IsNullOrEmpty(runtimeMember.DisplayName) ? snapshotMember.DisplayName : runtimeMember.DisplayName;
            snapshotMember.RoleTag = string.IsNullOrEmpty(runtimeMember.RoleTag) ? snapshotMember.RoleTag : runtimeMember.RoleTag;
            snapshotMember.RoleLabel = string.IsNullOrEmpty(runtimeMember.RoleLabel) ? snapshotMember.RoleLabel : runtimeMember.RoleLabel;
            snapshotMember.DefaultSkillId = string.IsNullOrEmpty(runtimeMember.DefaultSkillId) ? snapshotMember.DefaultSkillId : runtimeMember.DefaultSkillId;
            snapshotMember.GrowthTrackId = string.IsNullOrEmpty(runtimeMember.GrowthTrackId) ? snapshotMember.GrowthTrackId : runtimeMember.GrowthTrackId;
            snapshotMember.JobId = string.IsNullOrEmpty(runtimeMember.JobId) ? snapshotMember.JobId : runtimeMember.JobId;
            snapshotMember.EquipmentLoadoutId = string.IsNullOrEmpty(runtimeMember.EquipmentLoadoutId) ? snapshotMember.EquipmentLoadoutId : runtimeMember.EquipmentLoadoutId;
            snapshotMember.SkillLoadoutId = string.IsNullOrEmpty(runtimeMember.SkillLoadoutId) ? snapshotMember.SkillLoadoutId : runtimeMember.SkillLoadoutId;
            snapshotMember.ResolvedSkillName = string.IsNullOrEmpty(runtimeMember.ResolvedSkillName) ? snapshotMember.ResolvedSkillName : runtimeMember.ResolvedSkillName;
            snapshotMember.ResolvedSkillShortText = string.IsNullOrEmpty(runtimeMember.ResolvedSkillShortText) ? snapshotMember.ResolvedSkillShortText : runtimeMember.ResolvedSkillShortText;
            snapshotMember.EquipmentSummaryText = string.IsNullOrEmpty(runtimeMember.EquipmentSummaryText) ? snapshotMember.EquipmentSummaryText : runtimeMember.EquipmentSummaryText;
            snapshotMember.GearContributionSummaryText = string.IsNullOrEmpty(runtimeMember.GearContributionSummaryText) ? snapshotMember.GearContributionSummaryText : runtimeMember.GearContributionSummaryText;
            snapshotMember.AppliedProgressionSummaryText = string.IsNullOrEmpty(runtimeMember.AppliedProgressionSummaryText) ? snapshotMember.AppliedProgressionSummaryText : runtimeMember.AppliedProgressionSummaryText;
            snapshotMember.CurrentRunSummaryText = string.IsNullOrEmpty(runtimeMember.CurrentRunSummaryText) ? snapshotMember.CurrentRunSummaryText : runtimeMember.CurrentRunSummaryText;
            snapshotMember.NextRunPreviewSummaryText = string.IsNullOrEmpty(runtimeMember.NextRunPreviewSummaryText) ? snapshotMember.NextRunPreviewSummaryText : runtimeMember.NextRunPreviewSummaryText;
            snapshotMember.Level = runtimeMember.Level > 0 ? runtimeMember.Level : snapshotMember.Level;
            snapshotMember.Experience = runtimeMember.CurrentExperience >= 0 ? runtimeMember.CurrentExperience : snapshotMember.Experience;
            snapshotMember.NextLevelExperience = runtimeMember.NextLevelExperience > 0 ? runtimeMember.NextLevelExperience : snapshotMember.NextLevelExperience;
            snapshotMember.GrowthBonusMaxHp = runtimeMember.GrowthBonusMaxHp;
            snapshotMember.GrowthBonusAttack = runtimeMember.GrowthBonusAttack;
            snapshotMember.GrowthBonusDefense = runtimeMember.GrowthBonusDefense;
            snapshotMember.GrowthBonusSpeed = runtimeMember.GrowthBonusSpeed;
            snapshotMember.MaxHp = runtimeMember.MaxHp > 0 ? runtimeMember.MaxHp : snapshotMember.MaxHp;
            snapshotMember.CurrentHp = snapshotMember.CurrentHp > snapshotMember.MaxHp ? snapshotMember.MaxHp : snapshotMember.CurrentHp;
            snapshotMembers[snapshotIndex] = snapshotMember;
        }

        partyOutcome.Members = snapshotMembers;
        _latestRpgRunResultSnapshot.PartyOutcome = partyOutcome;
    }

    private int FindPartyOutcomeMemberIndex(PrototypeRpgPartyMemberOutcomeSnapshot[] snapshotMembers, string memberId, int fallbackIndex)
    {
        if (snapshotMembers != null && !string.IsNullOrEmpty(memberId))
        {
            for (int i = 0; i < snapshotMembers.Length; i++)
            {
                PrototypeRpgPartyMemberOutcomeSnapshot member = snapshotMembers[i];
                if (member != null && member.MemberId == memberId)
                {
                    return i;
                }
            }
        }

        if (snapshotMembers == null || snapshotMembers.Length <= 0)
        {
            return 0;
        }

        if (fallbackIndex < 0)
        {
            return 0;
        }

        return fallbackIndex >= snapshotMembers.Length ? snapshotMembers.Length - 1 : fallbackIndex;
    }

    private string ResolveLatestRunResultPartyId()
    {
        if (_activeDungeonParty != null && !string.IsNullOrEmpty(_activeDungeonParty.PartyId))
        {
            return _activeDungeonParty.PartyId;
        }

        return _runtimeEconomyState != null && !string.IsNullOrEmpty(_currentHomeCityId)
            ? _runtimeEconomyState.GetIdlePartyIdInCity(_currentHomeCityId)
            : string.Empty;
    }

    private CityWriteback BuildDungeonRunResultCityWritebackPreviewSurface()
    {
        string cityId = _currentHomeCityId;
        string dungeonId = _currentDungeonId;
        string nextSuggestedActionText = ResolveOutcomeReadbackNextSuggestedActionText(null, cityId, dungeonId);
        return BuildCityWritebackSurfaceForCity(cityId, nextSuggestedActionText);
    }

    private OutcomeReadback BuildDungeonRunResultOutcomeReadbackPreviewSurface()
    {
        string cityId = _currentHomeCityId;
        string dungeonId = _currentDungeonId;
        string cityLabel = string.IsNullOrEmpty(cityId) ? "None" : ResolveDispatchEntityDisplayName(cityId);
        string nextSuggestedActionText = ResolveOutcomeReadbackNextSuggestedActionText(null, cityId, dungeonId);
        string latestReturnAftermathText = ResolveOutcomeReadbackLatestReturnAftermathText(null, cityId, dungeonId);
        CityWriteback cityWriteback = BuildCityWritebackSurfaceForCity(cityId, nextSuggestedActionText);
        return BuildOutcomeReadbackSurfaceForContext(
            cityId,
            dungeonId,
            cityLabel,
            string.IsNullOrEmpty(cityId)
                ? "World return preview unavailable."
                : cityLabel + " return aftermath is ready for the world board.",
            nextSuggestedActionText,
            latestReturnAftermathText,
            cityWriteback);
    }
}
