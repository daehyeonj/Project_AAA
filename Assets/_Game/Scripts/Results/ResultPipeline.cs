public static class ResultPipeline
{
    public static ExpeditionResult BuildExpeditionResult(PostRunResolutionInput handoffInput)
    {
        PostRunResolutionInput safeInput = handoffInput ?? new PostRunResolutionInput();
        return BuildExpeditionResult(
            safeInput.CompatibilityRunResultContext,
            safeInput.BattleResult,
            safeInput.SourceCityId,
            safeInput.SourceCityLabel,
            safeInput.RewardResourceId,
            safeInput.Success,
            safeInput.ReturnedLootAmount,
            safeInput.ElapsedDays);
    }

    public static ExpeditionResult BuildExpeditionResult(
        PrototypeDungeonRunResultContext runResultContext,
        BattleResult battleResult,
        string sourceCityId,
        string sourceCityLabel,
        string rewardResourceId,
        bool success,
        int returnedLootAmount,
        int elapsedDays)
    {
        PrototypeDungeonRunResultContext context = runResultContext ?? new PrototypeDungeonRunResultContext();
        PrototypeRpgRunResultSnapshot runResult = context.CanonicalRunResult ?? new PrototypeRpgRunResultSnapshot();
        PrototypeRpgPartyOutcomeSnapshot partyOutcome = runResult.PartyOutcome ?? new PrototypeRpgPartyOutcomeSnapshot();
        PrototypeRpgLootOutcomeSnapshot lootOutcome = runResult.LootOutcome ?? new PrototypeRpgLootOutcomeSnapshot();
        PrototypeRpgEliteOutcomeSnapshot eliteOutcome = runResult.EliteOutcome ?? new PrototypeRpgEliteOutcomeSnapshot();
        PrototypeRpgEncounterOutcomeSnapshot encounterOutcome = runResult.EncounterOutcome ?? new PrototypeRpgEncounterOutcomeSnapshot();
        BattleResult safeBattleResult = battleResult ?? new BattleResult();

        ExpeditionResult result = new ExpeditionResult();
        result.SourceCityId = ChooseValue(sourceCityId, string.Empty);
        result.SourceCityLabel = ChooseText(sourceCityLabel, "None");
        result.ResultStateKey = ChooseValue(context.RunOutcomeKey, runResult.ResultStateKey);
        result.DungeonId = ChooseValue(context.DungeonId, runResult.DungeonId);
        result.DungeonLabel = ChooseText(context.DungeonLabel, runResult.DungeonLabel);
        result.RouteId = ChooseValue(context.RouteId, runResult.RouteId);
        result.RouteLabel = ChooseText(context.RouteLabel, runResult.RouteLabel);
        result.RewardResourceId = ChooseValue(rewardResourceId, string.Empty);
        result.ElapsedDays = elapsedDays > 0 ? elapsedDays : 1;
        result.SelectedRouteSummaryText = ChooseText(context.SelectedRouteSummaryText, result.RouteLabel);
        result.FollowedRecommendationSummaryText = ChooseText(context.FollowedRecommendationSummaryText, "No");
        result.FollowedRecommendation = context.FollowedRecommendationSummaryText == "Yes";
        result.Success = success;
        result.TotalTurnsTaken = Max(context.TimeCostTurns, runResult.TotalTurnsTaken);
        result.BattlesFoughtCount = context.BattlesFoughtCount > 0 ? context.BattlesFoughtCount : Max(encounterOutcome.ClearedEncounterCount, 0);
        result.ReturnedLootAmount = returnedLootAmount > 0 ? returnedLootAmount : 0;
        result.TotalLootGained = Max(lootOutcome.TotalLootGained, result.ReturnedLootAmount);
        result.BattleLootGained = Max(lootOutcome.BattleLootGained, 0);
        result.ChestLootGained = Max(lootOutcome.ChestLootGained, 0);
        result.EventLootGained = Max(lootOutcome.EventLootGained, 0);
        result.EliteRewardAmount = Max(lootOutcome.EliteRewardAmount, 0);
        result.EliteDefeated = eliteOutcome.IsEliteDefeated;
        result.EliteName = ChooseText(eliteOutcome.EliteName, "None");
        result.EliteTypeLabel = ChooseText(eliteOutcome.EliteTypeLabel, "None");
        result.EliteRewardLabel = ChooseText(eliteOutcome.EliteRewardLabel, "None");
        result.EliteBonusRewardEarned = eliteOutcome.EliteBonusRewardEarned;
        result.EliteBonusRewardAmount = Max(eliteOutcome.EliteBonusRewardAmount, 0);
        result.ClearedEncounterCount = Max(encounterOutcome.ClearedEncounterCount, 0);
        result.ClearedEncounterSummaryText = ChooseText(encounterOutcome.ClearedEncounterSummary, "None");
        result.OpenedChestCount = Max(encounterOutcome.OpenedChestCount, 0);
        result.RoomPathSummaryText = ChooseText(encounterOutcome.RoomPathSummary, "None");
        result.SelectedEventChoiceText = ChooseText(encounterOutcome.SelectedEventChoice, "None");
        result.SelectedPreEliteChoiceText = ChooseText(encounterOutcome.SelectedPreEliteChoice, "None");
        result.PreEliteHealAmount = Max(encounterOutcome.PreEliteHealAmount, 0);
        result.ResultSummaryText = ChooseText(context.ResultSummaryText, runResult.ResultSummary);
        result.SurvivingMembersSummaryText = ChooseText(context.SurvivingMembersSummaryText, runResult.SurvivingMembersSummary);
        result.SelectedPartySummaryText = ChooseText(partyOutcome.PartyMembersAtEndSummary, result.SurvivingMembersSummaryText);
        result.ReturnedLootSummaryText = returnedLootAmount > 0 && !string.IsNullOrEmpty(result.RewardResourceId)
            ? result.RewardResourceId + " x" + returnedLootAmount
            : ChooseText(context.ResourceDeltaSummaryText, lootOutcome.FinalLootSummary);
        result.PartyConditionText = ChooseText(partyOutcome.PartyConditionText, context.InjurySummaryText);
        result.PartyHpSummaryText = ChooseText(partyOutcome.PartyHpSummaryText, "None");
        result.PartyMembersAtEndSummaryText = ChooseText(partyOutcome.PartyMembersAtEndSummary, context.CasualtySummaryText);
        result.LootBreakdownSummaryText = ChooseText(context.ResourceDeltaSummaryText, lootOutcome.FinalLootSummary);
        result.DungeonSummaryText = ChooseText(context.WorldWritebackDungeonSummaryText, result.DungeonLabel);
        result.EncounterRequestSummaryText = ChooseText(context.EncounterRequestSummaryText, runResult.EncounterRequestSummaryText);
        result.BattleContextSummaryText = ChooseText(context.BattleContextSummaryText, safeBattleResult.BattleContextSummaryText);
        result.BattleRuntimeSummaryText = ChooseText(context.BattleRuntimeSummaryText, safeBattleResult.RuntimeSummaryText);
        result.BattleRuleSummaryText = ChooseText(context.BattleRuleSummaryText, safeBattleResult.LaneRuleSummaryText);
        result.BattleResultCoreSummaryText = ChooseText(context.BattleResultCoreSummaryText, safeBattleResult.ResultSummaryText);
        result.BattleAbsorbSummaryText = ChooseText(context.BattleAbsorbSummaryText, safeBattleResult.AbsorbSummaryText);
        result.NotableBattleEventsSummaryText = ChooseText(context.NotableBattleEventsSummaryText, safeBattleResult.NotableEventsSummaryText);
        result.ChoiceOutcomeSummaryText = ChooseText(context.ChoiceOutcomeSummaryText, "None");
        result.EventResolutionSummaryText = ChooseText(context.EventResolutionSummaryText, "None");
        result.ExtractionSummaryText = ChooseText(context.ExtractionSummaryText, "None");
        result.ExtractionPressureSummaryText = ChooseText(context.ExtractionPressureSummaryText, "None");
        result.SupplyPressureSummaryText = ChooseText(context.SupplyPressureSummaryText, "None");
        result.InjurySummaryText = ChooseText(context.InjurySummaryText, result.PartyConditionText);
        result.CasualtySummaryText = ChooseText(context.CasualtySummaryText, result.PartyMembersAtEndSummaryText);
        result.DiscoveredFlagsSummaryText = ChooseText(context.DiscoveredFlagsSummaryText, "None");
        result.ThreatProgressSummaryText = ChooseText(context.ThreatProgressSummaryText, "None");
        result.ClearProgressSummaryText = ChooseText(context.ClearProgressSummaryText, "None");
        result.EventLogSummaryText = ChooseText(context.EventLogSummaryText, "None");
        result.KeyEncounterSummaryText = ChooseText(context.KeyEncounterSummaryText, "None");
        result.EliteOutcomeSummaryText = eliteOutcome.IsEliteDefeated
            ? ChooseText(eliteOutcome.EliteRewardLabel, ChooseText(eliteOutcome.EliteName, "Elite defeated"))
            : ChooseText(eliteOutcome.EliteTypeLabel, "None");
        result.GearRewardCandidateSummaryText = ChooseText(context.GearRewardCandidateSummaryText, "None");
        result.EquipSwapChoiceSummaryText = ChooseText(context.EquipSwapChoiceSummaryText, "None");
        result.GearCarryContinuitySummaryText = ChooseText(context.GearCarryContinuitySummaryText, "None");
        result.BattleResult = CopyBattleResult(safeBattleResult);
        return result;
    }

    private static BattleResult CopyBattleResult(BattleResult source)
    {
        BattleResult copy = new BattleResult();
        if (source == null)
        {
            return copy;
        }

        copy.OutcomeKey = source.OutcomeKey;
        copy.ResultStateKey = source.ResultStateKey;
        copy.BattleId = source.BattleId;
        copy.EncounterId = source.EncounterId;
        copy.EncounterName = source.EncounterName;
        copy.EliteEncounterName = source.EliteEncounterName;
        copy.EliteRewardLabel = source.EliteRewardLabel;
        copy.DungeonId = source.DungeonId;
        copy.DungeonLabel = source.DungeonLabel;
        copy.RouteId = source.RouteId;
        copy.RouteLabel = source.RouteLabel;
        copy.TurnCount = source.TurnCount;
        copy.SurvivingMemberCount = source.SurvivingMemberCount;
        copy.KnockedOutMemberCount = source.KnockedOutMemberCount;
        copy.DefeatedEnemyCount = source.DefeatedEnemyCount;
        copy.EliteDefeated = source.EliteDefeated;
        copy.TotalDamageDealt = source.TotalDamageDealt;
        copy.TotalDamageTaken = source.TotalDamageTaken;
        copy.TotalHealingDone = source.TotalHealingDone;
        copy.ResultSummaryText = source.ResultSummaryText;
        copy.RuntimeSummaryText = source.RuntimeSummaryText;
        copy.PartyAftermathSummaryText = source.PartyAftermathSummaryText;
        copy.ResourceDeltaSummaryText = source.ResourceDeltaSummaryText;
        copy.StatusSummaryText = source.StatusSummaryText;
        copy.ConsumableUseSummaryText = source.ConsumableUseSummaryText;
        copy.LootRewardSummaryText = source.LootRewardSummaryText;
        copy.NotableEventsSummaryText = source.NotableEventsSummaryText;
        copy.PartyConditionAfterBattleText = source.PartyConditionAfterBattleText;
        copy.BattleContextSummaryText = source.BattleContextSummaryText;
        copy.LaneRuleSummaryText = source.LaneRuleSummaryText;
        copy.AbsorbSummaryText = source.AbsorbSummaryText;
        return copy;
    }

    public static CityWriteback BuildCityWriteback(
        string cityId,
        string cityLabel,
        string resultStateKey,
        string rewardResourceId,
        int lootReturned,
        string lootSummaryText,
        string partyOutcomeSummaryText,
        string stockReactionSummaryText,
        int stockBefore,
        int stockAfter,
        int stockDelta,
        string needPressureBeforeText,
        string needPressureAfterText,
        string dispatchReadinessBeforeText,
        string dispatchReadinessAfterText,
        string recoveryEtaText,
        string followUpHintText,
        string summaryText)
    {
        CityWriteback result = new CityWriteback();
        result.CityId = ChooseValue(cityId, string.Empty);
        result.CityLabel = ChooseText(cityLabel, "None");
        result.ResultStateKey = ChooseValue(resultStateKey, string.Empty);
        result.RewardResourceId = ChooseValue(rewardResourceId, string.Empty);
        result.LootReturned = lootReturned > 0 ? lootReturned : 0;
        result.LootSummaryText = ChooseText(lootSummaryText, "None");
        result.PartyOutcomeSummaryText = ChooseText(partyOutcomeSummaryText, "None");
        result.StockReactionSummaryText = ChooseText(stockReactionSummaryText, "None");
        result.StockBefore = stockBefore;
        result.StockAfter = stockAfter;
        result.StockDelta = stockDelta;
        result.NeedPressureBeforeText = ChooseText(needPressureBeforeText, "None");
        result.NeedPressureAfterText = ChooseText(needPressureAfterText, "None");
        result.DispatchReadinessBeforeText = ChooseText(dispatchReadinessBeforeText, "None");
        result.DispatchReadinessAfterText = ChooseText(dispatchReadinessAfterText, "None");
        result.RecoveryEtaText = ChooseText(recoveryEtaText, "None");
        result.FollowUpHintText = ChooseText(followUpHintText, "None");
        result.SummaryText = ChooseText(summaryText, "None");
        return result;
    }

    public static WorldWriteback BuildWorldWriteback(
        ExpeditionResult expeditionResult,
        CityWriteback cityWriteback,
        string writebackSummaryText,
        string dungeonStatusKey,
        string dungeonStatusSummaryText,
        string dungeonAvailabilitySummaryText,
        string dungeonLastOutcomeSummaryText)
    {
        ExpeditionResult safeExpeditionResult = expeditionResult ?? new ExpeditionResult();
        CityWriteback safeCityWriteback = cityWriteback ?? new CityWriteback();
        WorldWriteback result = new WorldWriteback();
        result.RunResultStateKey = ChooseValue(safeExpeditionResult.ResultStateKey, safeCityWriteback.ResultStateKey);
        result.Success = safeExpeditionResult.Success;
        result.SourceCityId = ChooseValue(safeExpeditionResult.SourceCityId, safeCityWriteback.CityId);
        result.SourceCityLabel = ChooseText(safeExpeditionResult.SourceCityLabel, safeCityWriteback.CityLabel);
        result.TargetDungeonId = ChooseValue(safeExpeditionResult.DungeonId, string.Empty);
        result.TargetDungeonLabel = ChooseText(safeExpeditionResult.DungeonLabel, safeExpeditionResult.DungeonSummaryText);
        result.ChosenRouteId = ChooseValue(safeExpeditionResult.RouteId, string.Empty);
        result.ChosenRouteLabel = ChooseText(safeExpeditionResult.SelectedRouteSummaryText, safeExpeditionResult.RouteLabel);
        result.RouteSummaryText = ChooseText(safeExpeditionResult.SelectedRouteSummaryText, result.ChosenRouteLabel);
        result.DungeonSummaryText = ChooseText(safeExpeditionResult.DungeonSummaryText, result.TargetDungeonLabel);
        result.ResultSummaryText = ChooseText(safeExpeditionResult.ResultSummaryText, "None");
        result.WritebackSummaryText = ChooseText(writebackSummaryText, "None");
        result.DungeonStatusKey = ChooseValue(dungeonStatusKey, string.Empty);
        result.DungeonStatusSummaryText = ChooseText(dungeonStatusSummaryText, "None");
        result.DungeonAvailabilitySummaryText = ChooseText(dungeonAvailabilitySummaryText, "None");
        result.DungeonLastOutcomeSummaryText = ChooseText(dungeonLastOutcomeSummaryText, "None");
        result.RewardResourceId = ChooseValue(safeExpeditionResult.RewardResourceId, safeCityWriteback.RewardResourceId);
        result.LootReturned = safeCityWriteback.LootReturned > 0 ? safeCityWriteback.LootReturned : Max(safeExpeditionResult.ReturnedLootAmount, 0);
        result.LootSummaryText = ChooseText(safeCityWriteback.LootSummaryText, safeExpeditionResult.LootBreakdownSummaryText);
        result.SurvivingMembersSummaryText = ChooseText(safeExpeditionResult.SurvivingMembersSummaryText, safeCityWriteback.PartyOutcomeSummaryText);
        result.ClearedEncountersSummaryText = ChooseText(safeExpeditionResult.ClearedEncounterSummaryText, "None");
        result.EventChoiceSummaryText = ChooseText(safeExpeditionResult.SelectedEventChoiceText, "None");
        result.LootBreakdownSummaryText = ChooseText(safeExpeditionResult.LootBreakdownSummaryText, result.LootSummaryText);
        result.EconomySummaryText = ChooseText(safeCityWriteback.StockReactionSummaryText, safeCityWriteback.SummaryText);
        result.ElapsedDays = safeExpeditionResult.ElapsedDays > 0 ? safeExpeditionResult.ElapsedDays : 1;
        result.CityWriteback = safeCityWriteback;
        return result;
    }

    public static OutcomeReadback BuildOutcomeReadback(
        ExpeditionResult expeditionResult,
        CityWriteback cityWriteback,
        WorldWriteback selectedWorldWriteback,
        string worldWritebackSummaryText,
        string latestReturnAftermathText,
        string cityId,
        string cityLabel,
        string resultStateKey,
        string acknowledgementText,
        string nextSuggestedActionText,
        string followUpHintText,
        string lastExpeditionResultText,
        string survivingMembersSummaryText,
        string clearedEncountersSummaryText,
        string eventChoiceSummaryText,
        string lootBreakdownSummaryText,
        string dungeonSummaryText,
        string routeSummaryText,
        string stockBeforeText,
        string stockAfterText,
        string stockDeltaText,
        string gearRewardSummaryText,
        string equipSwapSummaryText,
        string gearContinuitySummaryText,
        string recentExpeditionLog1Text,
        string recentExpeditionLog2Text,
        string recentExpeditionLog3Text,
        string recentWorldWritebackLog1Text,
        string recentWorldWritebackLog2Text,
        string recentWorldWritebackLog3Text)
    {
        ExpeditionResult safeExpeditionResult = expeditionResult ?? new ExpeditionResult();
        CityWriteback safeCityWriteback = cityWriteback ?? new CityWriteback();
        WorldWriteback safeSelectedWorldWriteback = selectedWorldWriteback ?? new WorldWriteback();
        OutcomeReadback result = new OutcomeReadback();
        result.CityId = ChooseValue(cityId, ChooseValue(safeExpeditionResult.SourceCityId, safeCityWriteback.CityId));
        result.CityLabel = ChooseText(cityLabel, ChooseText(safeExpeditionResult.SourceCityLabel, safeCityWriteback.CityLabel));
        result.ResultStateKey = ChooseValue(resultStateKey, ChooseValue(safeExpeditionResult.ResultStateKey, safeCityWriteback.ResultStateKey));
        result.AcknowledgementText = ChooseText(acknowledgementText, "None");
        result.LatestReturnAftermathText = ChooseText(latestReturnAftermathText, worldWritebackSummaryText);
        result.PostRunSummaryText = ChooseText(safeSelectedWorldWriteback.ResultSummaryText, ChooseText(safeExpeditionResult.ResultSummaryText, lastExpeditionResultText));
        result.NextSuggestedActionText = ChooseText(nextSuggestedActionText, "None");
        result.FollowUpHintText = ChooseText(followUpHintText, nextSuggestedActionText);
        result.LastExpeditionResultText = ChooseText(lastExpeditionResultText, result.PostRunSummaryText);
        result.WorldWritebackSummaryText = ChooseText(worldWritebackSummaryText, "None");
        result.SelectedWorldWritebackText = ChooseText(safeSelectedWorldWriteback.WritebackSummaryText, "None");
        result.DungeonStatusText = ChooseText(safeSelectedWorldWriteback.DungeonStatusSummaryText, "None");
        result.DungeonAvailabilityText = ChooseText(safeSelectedWorldWriteback.DungeonAvailabilitySummaryText, "None");
        result.DungeonLastOutcomeText = ChooseText(safeSelectedWorldWriteback.DungeonLastOutcomeSummaryText, ChooseText(safeExpeditionResult.DungeonSummaryText, safeSelectedWorldWriteback.DungeonSummaryText));
        result.SurvivingMembersSummaryText = ChooseText(safeSelectedWorldWriteback.SurvivingMembersSummaryText, ChooseText(safeExpeditionResult.SurvivingMembersSummaryText, survivingMembersSummaryText));
        result.ClearedEncountersSummaryText = ChooseText(safeSelectedWorldWriteback.ClearedEncountersSummaryText, ChooseText(safeExpeditionResult.ClearedEncounterSummaryText, clearedEncountersSummaryText));
        result.EventChoiceSummaryText = ChooseText(safeSelectedWorldWriteback.EventChoiceSummaryText, ChooseText(safeExpeditionResult.SelectedEventChoiceText, eventChoiceSummaryText));
        result.LootBreakdownSummaryText = ChooseText(safeSelectedWorldWriteback.LootBreakdownSummaryText, ChooseText(safeExpeditionResult.LootBreakdownSummaryText, lootBreakdownSummaryText));
        result.DungeonSummaryText = ChooseText(safeSelectedWorldWriteback.DungeonSummaryText, ChooseText(safeExpeditionResult.DungeonSummaryText, dungeonSummaryText));
        result.RouteSummaryText = ChooseText(safeSelectedWorldWriteback.RouteSummaryText, ChooseText(safeExpeditionResult.SelectedRouteSummaryText, routeSummaryText));
        result.StockBeforeText = ChooseText(stockBeforeText, "None");
        result.StockAfterText = ChooseText(stockAfterText, "None");
        result.StockDeltaText = ChooseText(stockDeltaText, "None");
        result.NeedPressureBeforeText = ChooseText(safeCityWriteback.NeedPressureBeforeText, "None");
        result.NeedPressureAfterText = ChooseText(safeCityWriteback.NeedPressureAfterText, "None");
        result.DispatchReadinessBeforeText = ChooseText(safeCityWriteback.DispatchReadinessBeforeText, "None");
        result.DispatchReadinessAfterText = ChooseText(safeCityWriteback.DispatchReadinessAfterText, "None");
        result.RecoveryEtaText = ChooseText(safeCityWriteback.RecoveryEtaText, "None");
        result.GearRewardSummaryText = ChooseText(gearRewardSummaryText, "None");
        result.EquipSwapSummaryText = ChooseText(equipSwapSummaryText, "None");
        result.GearContinuitySummaryText = ChooseText(gearContinuitySummaryText, "None");
        result.RecentExpeditionLog1Text = ChooseText(recentExpeditionLog1Text, "None");
        result.RecentExpeditionLog2Text = ChooseText(recentExpeditionLog2Text, "None");
        result.RecentExpeditionLog3Text = ChooseText(recentExpeditionLog3Text, "None");
        result.RecentWorldWritebackLog1Text = ChooseText(recentWorldWritebackLog1Text, "None");
        result.RecentWorldWritebackLog2Text = ChooseText(recentWorldWritebackLog2Text, "None");
        result.RecentWorldWritebackLog3Text = ChooseText(recentWorldWritebackLog3Text, "None");
        return result;
    }

    public static ExpeditionResult BuildReturnConsumedExpeditionResult(
        ExpeditionResult expeditionResult,
        CityWriteback cityWriteback,
        WorldWriteback worldWriteback,
        OutcomeReadback outcomeReadback)
    {
        ExpeditionResult result = CopyExpeditionResult(expeditionResult);
        CityWriteback safeCityWriteback = cityWriteback ?? new CityWriteback();
        WorldWriteback safeWorldWriteback = worldWriteback ?? new WorldWriteback();
        OutcomeReadback safeOutcomeReadback = outcomeReadback ?? new OutcomeReadback();

        result.ResultSummaryText = ChooseMeaningfulText(
            safeOutcomeReadback.PostRunSummaryText,
            result.ResultSummaryText);
        result.DungeonSummaryText = ChooseMeaningfulText(
            safeOutcomeReadback.DungeonSummaryText,
            result.DungeonSummaryText);
        result.SelectedRouteSummaryText = ChooseMeaningfulText(
            safeOutcomeReadback.RouteSummaryText,
            result.SelectedRouteSummaryText);
        result.SurvivingMembersSummaryText = ChooseMeaningfulText(
            safeOutcomeReadback.SurvivingMembersSummaryText,
            result.SurvivingMembersSummaryText);
        result.SelectedPartySummaryText = ChooseMeaningfulText(
            safeOutcomeReadback.SurvivingMembersSummaryText,
            ChooseMeaningfulText(result.SelectedPartySummaryText, result.PartyMembersAtEndSummaryText));
        result.ClearedEncounterSummaryText = ChooseMeaningfulText(
            safeOutcomeReadback.ClearedEncountersSummaryText,
            result.ClearedEncounterSummaryText);
        result.SelectedEventChoiceText = ChooseMeaningfulText(
            safeOutcomeReadback.EventChoiceSummaryText,
            result.SelectedEventChoiceText);
        result.ReturnedLootAmount = safeCityWriteback.LootReturned > 0
            ? safeCityWriteback.LootReturned
            : result.ReturnedLootAmount;
        result.ReturnedLootSummaryText = ChooseMeaningfulText(
            safeCityWriteback.LootSummaryText,
            ChooseMeaningfulText(
                safeWorldWriteback.LootSummaryText,
                ChooseMeaningfulText(safeOutcomeReadback.LootBreakdownSummaryText, result.ReturnedLootSummaryText)));
        result.LootBreakdownSummaryText = ChooseMeaningfulText(
            safeOutcomeReadback.LootBreakdownSummaryText,
            result.LootBreakdownSummaryText);
        result.EliteOutcomeSummaryText = ChooseMeaningfulText(
            result.EliteOutcomeSummaryText,
            ChooseMeaningfulText(result.EliteRewardLabel, result.EliteName));
        result.GearRewardCandidateSummaryText = ChooseMeaningfulText(
            safeOutcomeReadback.GearRewardSummaryText,
            result.GearRewardCandidateSummaryText);
        result.EquipSwapChoiceSummaryText = ChooseMeaningfulText(
            safeOutcomeReadback.EquipSwapSummaryText,
            result.EquipSwapChoiceSummaryText);
        result.GearCarryContinuitySummaryText = ChooseMeaningfulText(
            safeOutcomeReadback.GearContinuitySummaryText,
            result.GearCarryContinuitySummaryText);
        result.WorldWritebackSummaryText = ChooseMeaningfulText(
            safeOutcomeReadback.SelectedWorldWritebackText,
            ChooseMeaningfulText(
                safeOutcomeReadback.WorldWritebackSummaryText,
                safeWorldWriteback.WritebackSummaryText));
        result.LatestReturnAftermathSummaryText = ChooseMeaningfulText(
            safeOutcomeReadback.LatestReturnAftermathText,
            result.WorldWritebackSummaryText);
        result.NextSuggestedActionText = ChooseMeaningfulText(
            safeOutcomeReadback.NextSuggestedActionText,
            safeCityWriteback.FollowUpHintText);
        result.NextPrepFollowUpSummaryText = ChooseMeaningfulText(
            safeOutcomeReadback.FollowUpHintText,
            result.NextSuggestedActionText);
        return result;
    }

    private static ExpeditionResult CopyExpeditionResult(ExpeditionResult source)
    {
        ExpeditionResult copy = new ExpeditionResult();
        if (source == null)
        {
            return copy;
        }

        copy.SourceCityId = source.SourceCityId;
        copy.SourceCityLabel = source.SourceCityLabel;
        copy.ResultStateKey = source.ResultStateKey;
        copy.DungeonId = source.DungeonId;
        copy.DungeonLabel = source.DungeonLabel;
        copy.RouteId = source.RouteId;
        copy.RouteLabel = source.RouteLabel;
        copy.RewardResourceId = source.RewardResourceId;
        copy.ElapsedDays = source.ElapsedDays;
        copy.SelectedRouteSummaryText = source.SelectedRouteSummaryText;
        copy.FollowedRecommendationSummaryText = source.FollowedRecommendationSummaryText;
        copy.FollowedRecommendation = source.FollowedRecommendation;
        copy.Success = source.Success;
        copy.TotalTurnsTaken = source.TotalTurnsTaken;
        copy.BattlesFoughtCount = source.BattlesFoughtCount;
        copy.ReturnedLootAmount = source.ReturnedLootAmount;
        copy.TotalLootGained = source.TotalLootGained;
        copy.BattleLootGained = source.BattleLootGained;
        copy.ChestLootGained = source.ChestLootGained;
        copy.EventLootGained = source.EventLootGained;
        copy.EliteRewardAmount = source.EliteRewardAmount;
        copy.EliteDefeated = source.EliteDefeated;
        copy.EliteName = source.EliteName;
        copy.EliteTypeLabel = source.EliteTypeLabel;
        copy.EliteRewardLabel = source.EliteRewardLabel;
        copy.EliteBonusRewardEarned = source.EliteBonusRewardEarned;
        copy.EliteBonusRewardAmount = source.EliteBonusRewardAmount;
        copy.ClearedEncounterCount = source.ClearedEncounterCount;
        copy.ClearedEncounterSummaryText = source.ClearedEncounterSummaryText;
        copy.OpenedChestCount = source.OpenedChestCount;
        copy.RoomPathSummaryText = source.RoomPathSummaryText;
        copy.SelectedEventChoiceText = source.SelectedEventChoiceText;
        copy.SelectedPreEliteChoiceText = source.SelectedPreEliteChoiceText;
        copy.PreEliteHealAmount = source.PreEliteHealAmount;
        copy.ResultSummaryText = source.ResultSummaryText;
        copy.SurvivingMembersSummaryText = source.SurvivingMembersSummaryText;
        copy.SelectedPartySummaryText = source.SelectedPartySummaryText;
        copy.ReturnedLootSummaryText = source.ReturnedLootSummaryText;
        copy.PartyConditionText = source.PartyConditionText;
        copy.PartyHpSummaryText = source.PartyHpSummaryText;
        copy.PartyMembersAtEndSummaryText = source.PartyMembersAtEndSummaryText;
        copy.LootBreakdownSummaryText = source.LootBreakdownSummaryText;
        copy.DungeonSummaryText = source.DungeonSummaryText;
        copy.EncounterRequestSummaryText = source.EncounterRequestSummaryText;
        copy.BattleContextSummaryText = source.BattleContextSummaryText;
        copy.BattleRuntimeSummaryText = source.BattleRuntimeSummaryText;
        copy.BattleRuleSummaryText = source.BattleRuleSummaryText;
        copy.BattleResultCoreSummaryText = source.BattleResultCoreSummaryText;
        copy.BattleAbsorbSummaryText = source.BattleAbsorbSummaryText;
        copy.NotableBattleEventsSummaryText = source.NotableBattleEventsSummaryText;
        copy.ChoiceOutcomeSummaryText = source.ChoiceOutcomeSummaryText;
        copy.EventResolutionSummaryText = source.EventResolutionSummaryText;
        copy.ExtractionSummaryText = source.ExtractionSummaryText;
        copy.ExtractionPressureSummaryText = source.ExtractionPressureSummaryText;
        copy.SupplyPressureSummaryText = source.SupplyPressureSummaryText;
        copy.InjurySummaryText = source.InjurySummaryText;
        copy.CasualtySummaryText = source.CasualtySummaryText;
        copy.DiscoveredFlagsSummaryText = source.DiscoveredFlagsSummaryText;
        copy.ThreatProgressSummaryText = source.ThreatProgressSummaryText;
        copy.ClearProgressSummaryText = source.ClearProgressSummaryText;
        copy.EventLogSummaryText = source.EventLogSummaryText;
        copy.KeyEncounterSummaryText = source.KeyEncounterSummaryText;
        copy.EliteOutcomeSummaryText = source.EliteOutcomeSummaryText;
        copy.GearRewardCandidateSummaryText = source.GearRewardCandidateSummaryText;
        copy.EquipSwapChoiceSummaryText = source.EquipSwapChoiceSummaryText;
        copy.GearCarryContinuitySummaryText = source.GearCarryContinuitySummaryText;
        copy.WorldWritebackSummaryText = source.WorldWritebackSummaryText;
        copy.LatestReturnAftermathSummaryText = source.LatestReturnAftermathSummaryText;
        copy.NextSuggestedActionText = source.NextSuggestedActionText;
        copy.NextPrepFollowUpSummaryText = source.NextPrepFollowUpSummaryText;
        copy.BattleResult = CopyBattleResult(source.BattleResult);
        return copy;
    }

    private static string ChooseMeaningfulText(string primary, string fallback)
    {
        if (IsMeaningfulText(primary))
        {
            return primary;
        }

        return IsMeaningfulText(fallback) ? fallback : "None";
    }

    private static string ChooseValue(string primary, string fallback)
    {
        return !string.IsNullOrEmpty(primary) ? primary : !string.IsNullOrEmpty(fallback) ? fallback : string.Empty;
    }

    private static string ChooseText(string primary, string fallback)
    {
        string value = ChooseValue(primary, fallback);
        return string.IsNullOrEmpty(value) ? "None" : value;
    }

    private static int Max(int left, int right)
    {
        return left > right ? left : right;
    }

    private static bool IsMeaningfulText(string value)
    {
        return !string.IsNullOrEmpty(value) && value != "None" && value != "(missing)";
    }
}
