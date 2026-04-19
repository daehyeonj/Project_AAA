using System;
using System.Collections.Generic;

public static class ResultPipeline
{
    public static ExpeditionOutcome BuildExpeditionOutcome(
        ExpeditionRunState runState,
        string rewardResourceId,
        int returnedLootAmount,
        bool success,
        string resultSummaryText,
        PrototypeRpgRunResultSnapshot runResultSnapshot)
    {
        ExpeditionRunState safeState = runState ?? new ExpeditionRunState();
        ExpeditionPlan launchPlan = safeState.LaunchPlan ?? new ExpeditionPlan();
        PrototypeRpgRunResultSnapshot safeSnapshot = runResultSnapshot ?? safeState.ResultSnapshot ?? new PrototypeRpgRunResultSnapshot();
        ExpeditionOutcome result = BuildExpeditionOutcome(
            ChooseValue(safeState.OriginCityId, launchPlan.OriginCityId),
            ChooseText(safeState.OriginCityLabel, launchPlan.OriginCityLabel),
            ChooseValue(safeState.TargetDungeonId, launchPlan.TargetDungeonId),
            ChooseText(safeState.TargetDungeonLabel, launchPlan.TargetDungeonLabel),
            rewardResourceId,
            returnedLootAmount,
            success,
            ChooseText(resultSummaryText, safeState.ResultSummaryText),
            ChooseText(safeState.RouteLabel, launchPlan.SelectedRoute != null ? launchPlan.SelectedRoute.RouteLabel : string.Empty),
            safeSnapshot);

        result.MissionObjectiveText = ChooseText(safeState.ObjectiveText, launchPlan.ObjectiveText);
        result.MissionRelevanceText = ChooseText(
            safeState.ExpectedUsefulnessText,
            ChooseText(safeState.WhyNowText, launchPlan.ExpectedUsefulnessText));
        result.RiskRewardContextText = ChooseText(
            safeState.RiskRewardPreviewText,
            ChooseText(safeState.RouteContextText, safeState.RouteRiskText));
        result.RunPathSummaryText = ChooseText(
            safeState.RoomPathSummaryText,
            safeSnapshot.EncounterOutcome != null ? safeSnapshot.EncounterOutcome.RoomPathSummary : string.Empty);
        PopulateSharedOutcomeMeaning(
            result,
            ChooseValue(
                safeState.RouteId,
                ChooseValue(
                    safeSnapshot.RouteId,
                    launchPlan.SelectedRoute != null ? launchPlan.SelectedRoute.RouteId : string.Empty)));
        return result;
    }

    public static ExpeditionOutcome BuildExpeditionOutcome(
        string sourceCityId,
        string sourceCityLabel,
        string targetDungeonId,
        string targetDungeonLabel,
        string rewardResourceId,
        int returnedLootAmount,
        bool success,
        string resultSummaryText,
        string routeSummaryText,
        PrototypeRpgRunResultSnapshot runResultSnapshot)
    {
        PrototypeRpgRunResultSnapshot safeSnapshot = runResultSnapshot ?? new PrototypeRpgRunResultSnapshot();
        PrototypeRpgPartyOutcomeSnapshot partyOutcome = safeSnapshot.PartyOutcome ?? new PrototypeRpgPartyOutcomeSnapshot();
        PrototypeRpgLootOutcomeSnapshot lootOutcome = safeSnapshot.LootOutcome ?? new PrototypeRpgLootOutcomeSnapshot();
        PrototypeRpgEliteOutcomeSnapshot eliteOutcome = safeSnapshot.EliteOutcome ?? new PrototypeRpgEliteOutcomeSnapshot();
        PrototypeRpgEncounterOutcomeSnapshot encounterOutcome = safeSnapshot.EncounterOutcome ?? new PrototypeRpgEncounterOutcomeSnapshot();

        ExpeditionOutcome result = BuildExpeditionOutcome(
            sourceCityId,
            sourceCityLabel,
            ChooseValue(targetDungeonId, safeSnapshot.DungeonId),
            ChooseText(targetDungeonLabel, safeSnapshot.DungeonLabel),
            rewardResourceId,
            returnedLootAmount,
            success,
            safeSnapshot.ResultStateKey,
            ChooseText(resultSummaryText, safeSnapshot.ResultSummary),
            ChooseText(safeSnapshot.SurvivingMembersSummary, partyOutcome.PartyMembersAtEndSummary),
            encounterOutcome.ClearedEncounterSummary,
            encounterOutcome.SelectedEventChoice,
            lootOutcome.FinalLootSummary,
            ChooseText(routeSummaryText, safeSnapshot.RouteLabel),
            ChooseText(targetDungeonLabel, safeSnapshot.DungeonLabel));

        result.TotalTurnsTaken = NonNegative(safeSnapshot.TotalTurnsTaken);
        result.ClearedEncounterCount = NonNegative(encounterOutcome.ClearedEncounterCount);
        result.OpenedChestCount = NonNegative(encounterOutcome.OpenedChestCount);
        result.SurvivingMemberCount = NonNegative(partyOutcome.SurvivingMemberCount);
        result.KnockedOutMemberCount = NonNegative(partyOutcome.KnockedOutMemberCount);
        result.EliteDefeated = eliteOutcome.IsEliteDefeated;
        result.PartyConditionText = ChooseText(partyOutcome.PartyConditionText, "None");
        result.PartyHpSummaryText = ChooseText(partyOutcome.PartyHpSummaryText, "None");
        result.EliteSummaryText = BuildEliteSummary(eliteOutcome);
        PopulateSharedOutcomeMeaning(result, safeSnapshot.RouteId);
        return result;
    }

    public static ExpeditionOutcome BuildExpeditionOutcome(
        string sourceCityId,
        string sourceCityLabel,
        string targetDungeonId,
        string targetDungeonLabel,
        string rewardResourceId,
        int returnedLootAmount,
        bool success,
        string resultStateKey,
        string resultSummaryText,
        string survivingMembersSummaryText,
        string clearedEncountersSummaryText,
        string eventChoiceSummaryText,
        string lootBreakdownSummaryText,
        string routeSummaryText,
        string dungeonSummaryText)
    {
        ExpeditionOutcome result = new ExpeditionOutcome();
        result.SourceCityId = ChooseValue(sourceCityId, string.Empty);
        result.SourceCityLabel = ChooseText(sourceCityLabel, "None");
        result.TargetDungeonId = ChooseValue(targetDungeonId, string.Empty);
        result.TargetDungeonLabel = ChooseText(targetDungeonLabel, dungeonSummaryText);
        result.RewardResourceId = ChooseValue(rewardResourceId, string.Empty);
        result.ResultStateKey = ChooseValue(resultStateKey, success ? "run_clear" : "run_defeat");
        result.Success = success;
        result.ReturnedLootAmount = success ? NonNegative(returnedLootAmount) : 0;
        result.ResultSummaryText = ChooseText(resultSummaryText, "None");
        result.LootSummaryText = BuildReturnedLootSummary(result.RewardResourceId, result.ReturnedLootAmount);
        result.SurvivingMembersSummaryText = ChooseText(survivingMembersSummaryText, "None");
        result.ClearedEncountersSummaryText = ChooseText(clearedEncountersSummaryText, "None");
        result.EventChoiceSummaryText = ChooseText(eventChoiceSummaryText, "None");
        result.LootBreakdownSummaryText = ChooseText(lootBreakdownSummaryText, result.LootSummaryText);
        result.RouteSummaryText = ChooseText(routeSummaryText, "None");
        result.DungeonSummaryText = ChooseText(dungeonSummaryText, result.TargetDungeonLabel);
        result.MissionObjectiveText = "None";
        result.MissionRelevanceText = "None";
        result.RiskRewardContextText = "None";
        result.RunPathSummaryText = "None";
        PopulateSharedOutcomeMeaning(result, string.Empty);
        return result;
    }

    public static WorldDelta BuildWorldDelta(ExpeditionOutcome expeditionOutcome)
    {
        ExpeditionOutcome safeOutcome = expeditionOutcome ?? new ExpeditionOutcome();
        WorldDelta result = new WorldDelta();
        result.SourceCityId = ChooseValue(safeOutcome.SourceCityId, string.Empty);
        result.SourceCityLabel = ChooseText(safeOutcome.SourceCityLabel, "Unknown City");
        result.TargetDungeonId = ChooseValue(safeOutcome.TargetDungeonId, string.Empty);
        result.TargetDungeonLabel = ChooseText(safeOutcome.TargetDungeonLabel, safeOutcome.DungeonSummaryText);
        result.RewardResourceId = ChooseValue(safeOutcome.RewardResourceId, string.Empty);
        result.ResultStateKey = ChooseValue(safeOutcome.ResultStateKey, safeOutcome.Success ? "run_clear" : "run_defeat");
        result.Success = safeOutcome.Success;
        result.AddedResourceAmount = safeOutcome.Success ? NonNegative(safeOutcome.ReturnedLootAmount) : 0;
        result.ExpeditionSuccessCountDelta = safeOutcome.Success ? 1 : 0;
        result.ExpeditionFailureCountDelta = safeOutcome.Success ? 0 : 1;
        result.ResultSummaryText = ChooseText(safeOutcome.ResultSummaryText, "None");
        result.LootSummaryText = ChooseText(safeOutcome.LootSummaryText, "None");
        result.SurvivingMembersSummaryText = ChooseText(safeOutcome.SurvivingMembersSummaryText, "None");
        result.ClearedEncountersSummaryText = ChooseText(safeOutcome.ClearedEncountersSummaryText, "None");
        result.EventChoiceSummaryText = ChooseText(safeOutcome.EventChoiceSummaryText, "None");
        result.LootBreakdownSummaryText = ChooseText(safeOutcome.LootBreakdownSummaryText, "None");
        result.RouteSummaryText = ChooseText(safeOutcome.RouteSummaryText, "None");
        result.DungeonSummaryText = ChooseText(safeOutcome.DungeonSummaryText, result.TargetDungeonLabel);
        result.OutcomeMeaningId = ChooseValue(safeOutcome.OutcomeMeaningId, string.Empty);
        result.OutcomeRewardMeaningText = ChooseText(safeOutcome.OutcomeRewardMeaningText, "None");
        result.CityImpactMeaningText = ChooseText(safeOutcome.CityImpactMeaningText, "None");
        result.RecommendationShiftText = ChooseText(safeOutcome.RecommendationShiftText, "None");
        result.CityStatusChangeSummaryText = result.AddedResourceAmount > 0 && HasText(result.LootSummaryText)
            ? result.SourceCityLabel + " absorbed " + result.LootSummaryText + "."
            : result.SourceCityLabel + " absorbed no dungeon loot.";
        result.ExpeditionLogEntryText = ChooseText(result.ResultSummaryText, result.CityStatusChangeSummaryText);
        return result;
    }

    public static OutcomeReadback BuildOutcomeReadback(ExpeditionOutcome expeditionOutcome, WorldDelta worldDelta)
    {
        ExpeditionOutcome safeOutcome = expeditionOutcome ?? new ExpeditionOutcome();
        WorldDelta safeDelta = worldDelta ?? new WorldDelta();
        OutcomeReadback result = new OutcomeReadback();
        result.SourceCityId = ChooseValue(safeOutcome.SourceCityId, safeDelta.SourceCityId);
        result.SourceCityLabel = ChooseText(safeOutcome.SourceCityLabel, safeDelta.SourceCityLabel);
        result.TargetDungeonId = ChooseValue(safeOutcome.TargetDungeonId, safeDelta.TargetDungeonId);
        result.TargetDungeonLabel = ChooseText(safeOutcome.TargetDungeonLabel, safeDelta.TargetDungeonLabel);
        result.CityId = result.SourceCityId;
        result.CityLabel = result.SourceCityLabel;
        result.ResultStateKey = ChooseValue(safeOutcome.ResultStateKey, safeDelta.ResultStateKey);
        result.Success = safeOutcome.Success;
        result.SummaryText = ChooseText(safeOutcome.ResultSummaryText, safeDelta.ResultSummaryText);
        result.LootSummaryText = ChooseText(safeDelta.LootSummaryText, safeOutcome.LootSummaryText);
        result.SurvivingMembersSummaryText = ChooseText(safeOutcome.SurvivingMembersSummaryText, safeDelta.SurvivingMembersSummaryText);
        result.ClearedEncountersSummaryText = ChooseText(safeOutcome.ClearedEncountersSummaryText, safeDelta.ClearedEncountersSummaryText);
        result.EventChoiceSummaryText = ChooseText(safeOutcome.EventChoiceSummaryText, safeDelta.EventChoiceSummaryText);
        result.LootBreakdownSummaryText = ChooseText(safeOutcome.LootBreakdownSummaryText, safeDelta.LootBreakdownSummaryText);
        result.RouteSummaryText = ChooseText(safeOutcome.RouteSummaryText, safeDelta.RouteSummaryText);
        result.DungeonSummaryText = ChooseText(safeOutcome.DungeonSummaryText, safeDelta.DungeonSummaryText);
        result.MissionObjectiveText = ChooseText(safeOutcome.MissionObjectiveText, "None");
        result.MissionRelevanceText = ChooseText(safeOutcome.MissionRelevanceText, "None");
        result.RiskRewardContextText = ChooseText(safeOutcome.RiskRewardContextText, "None");
        result.RunPathSummaryText = ChooseText(safeOutcome.RunPathSummaryText, "None");
        result.OutcomeMeaningId = ChooseValue(safeOutcome.OutcomeMeaningId, safeDelta.OutcomeMeaningId);
        result.OutcomeRewardMeaningText = ChooseText(safeOutcome.OutcomeRewardMeaningText, safeDelta.OutcomeRewardMeaningText);
        result.CityImpactMeaningText = ChooseText(safeDelta.CityImpactMeaningText, safeOutcome.CityImpactMeaningText);
        result.RecommendationShiftText = ChooseText(safeDelta.RecommendationShiftText, safeOutcome.RecommendationShiftText);
        result.CityStatusChangeSummaryText = ChooseText(safeDelta.CityStatusChangeSummaryText, "None");
        result.ExpeditionLogEntryText = ChooseText(safeDelta.ExpeditionLogEntryText, result.SummaryText);
        result.PartyConditionText = ChooseText(safeOutcome.PartyConditionText, "None");
        result.PartyHpSummaryText = ChooseText(safeOutcome.PartyHpSummaryText, "None");
        result.EliteSummaryText = ChooseText(safeOutcome.EliteSummaryText, "None");
        result.AcknowledgementText = ChooseText(result.CityStatusChangeSummaryText, result.SummaryText);
        result.LatestReturnAftermathText = ChooseText(result.CityStatusChangeSummaryText, result.SummaryText);
        result.PostRunSummaryText = result.SummaryText;
        result.NextSuggestedActionText = ChooseText(result.RecommendationShiftText, result.CityImpactMeaningText);
        result.FollowUpHintText = ChooseText(result.CityImpactMeaningText, result.RecommendationShiftText);
        result.LastExpeditionResultText = result.SummaryText;
        result.WorldWritebackSummaryText = result.CityStatusChangeSummaryText;
        result.SelectedWorldWritebackText = result.CityStatusChangeSummaryText;
        result.DungeonStatusText = result.DungeonSummaryText;
        result.DungeonAvailabilityText = "None";
        result.DungeonLastOutcomeText = result.SummaryText;
        result.StockBeforeText = "None";
        result.StockAfterText = "None";
        result.StockDeltaText = "None";
        result.NeedPressureBeforeText = "None";
        result.NeedPressureAfterText = "None";
        result.DispatchReadinessBeforeText = "None";
        result.DispatchReadinessAfterText = "None";
        result.RecoveryEtaText = "None";
        result.GearRewardSummaryText = "None";
        result.EquipSwapSummaryText = "None";
        result.GearContinuitySummaryText = "None";
        result.RecentExpeditionLog1Text = result.ExpeditionLogEntryText;
        result.RecentExpeditionLog2Text = "None";
        result.RecentExpeditionLog3Text = "None";
        result.RecentWorldWritebackLog1Text = result.CityStatusChangeSummaryText;
        result.RecentWorldWritebackLog2Text = "None";
        result.RecentWorldWritebackLog3Text = "None";
        return result;
    }

    public static ExpeditionResult BuildExpeditionResult(PostRunResolutionInput handoffInput)
    {
        PostRunResolutionInput safeInput = handoffInput ?? new PostRunResolutionInput();
        ExpeditionResult result = BuildExpeditionResult(
            safeInput.CompatibilityRunResultContext,
            safeInput.BattleResult,
            safeInput.SourceCityId,
            safeInput.SourceCityLabel,
            safeInput.RewardResourceId,
            safeInput.Success,
            safeInput.ReturnedLootAmount,
            safeInput.ElapsedDays);
        ApplyProgressionOutput(result, safeInput.ProgressionOutput);
        RefreshGrowthRevealSummaries(result);
        return result;
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
        result.LatestReturnAftermathSummaryText = ChooseMeaningfulText(
            BuildPartyGrowthAftermathSummary(partyOutcome),
            result.ResultSummaryText);
        result.NextPrepFollowUpSummaryText = ChooseMeaningfulText(
            BuildPartyGrowthFollowUpSummary(partyOutcome),
            "None");
        result.NextSuggestedActionText = result.NextPrepFollowUpSummaryText;
        result.BattleResult = CopyBattleResult(safeBattleResult);
        return result;
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
        result.SourceCityId = result.CityId;
        result.SourceCityLabel = result.CityLabel;
        result.TargetDungeonId = ChooseValue(safeExpeditionResult.DungeonId, safeSelectedWorldWriteback.TargetDungeonId);
        result.TargetDungeonLabel = ChooseText(safeExpeditionResult.DungeonLabel, safeSelectedWorldWriteback.TargetDungeonLabel);
        result.ResultStateKey = ChooseValue(resultStateKey, ChooseValue(safeExpeditionResult.ResultStateKey, safeCityWriteback.ResultStateKey));
        result.Success = safeExpeditionResult.Success;
        result.AcknowledgementText = ChooseText(acknowledgementText, "None");
        result.LatestReturnAftermathText = ChooseMeaningfulText(
            latestReturnAftermathText,
            ChooseMeaningfulText(safeExpeditionResult.LatestReturnAftermathSummaryText, worldWritebackSummaryText));
        result.PostRunSummaryText = ChooseText(safeSelectedWorldWriteback.ResultSummaryText, ChooseText(safeExpeditionResult.ResultSummaryText, lastExpeditionResultText));
        result.SummaryText = result.PostRunSummaryText;
        result.NextSuggestedActionText = ChooseMeaningfulText(
            nextSuggestedActionText,
            ChooseMeaningfulText(safeExpeditionResult.NextSuggestedActionText, "None"));
        result.FollowUpHintText = ChooseMeaningfulText(
            followUpHintText,
            ChooseMeaningfulText(safeExpeditionResult.NextPrepFollowUpSummaryText, result.NextSuggestedActionText));
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
        result.LootSummaryText = ChooseText(safeCityWriteback.LootSummaryText, safeSelectedWorldWriteback.LootSummaryText);
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
        result.MissionObjectiveText = ChooseText(safeExpeditionResult.DungeonSummaryText, "None");
        result.MissionRelevanceText = ChooseText(result.SelectedWorldWritebackText, "None");
        result.RiskRewardContextText = ChooseText(result.WorldWritebackSummaryText, "None");
        result.RunPathSummaryText = ChooseText(safeExpeditionResult.RoomPathSummaryText, "None");
        result.OutcomeMeaningId = string.Empty;
        result.OutcomeRewardMeaningText = "None";
        result.CityImpactMeaningText = ChooseText(result.WorldWritebackSummaryText, "None");
        result.RecommendationShiftText = ChooseText(result.NextSuggestedActionText, result.FollowUpHintText);
        result.GearRewardSummaryText = ChooseText(gearRewardSummaryText, "None");
        result.EquipSwapSummaryText = ChooseText(equipSwapSummaryText, "None");
        result.GearContinuitySummaryText = ChooseText(gearContinuitySummaryText, "None");
        result.GrowthRevealSummaryText = ChooseMeaningfulText(
            safeExpeditionResult.GrowthRevealSummaryText,
            "None");
        result.LatestGrowthHighlightText = ChooseMeaningfulText(
            safeExpeditionResult.LatestGrowthHighlightText,
            result.LatestReturnAftermathText);
        result.NextRunGrowthPreviewText = ChooseMeaningfulText(
            safeExpeditionResult.NextRunGrowthPreviewText,
            result.FollowUpHintText);
        result.InspectEquipmentHintText = ChooseMeaningfulText(
            safeExpeditionResult.InspectEquipmentHintText,
            "None");
        result.RecentExpeditionLog1Text = ChooseText(recentExpeditionLog1Text, "None");
        result.RecentExpeditionLog2Text = ChooseText(recentExpeditionLog2Text, "None");
        result.RecentExpeditionLog3Text = ChooseText(recentExpeditionLog3Text, "None");
        result.RecentWorldWritebackLog1Text = ChooseText(recentWorldWritebackLog1Text, "None");
        result.RecentWorldWritebackLog2Text = ChooseText(recentWorldWritebackLog2Text, "None");
        result.RecentWorldWritebackLog3Text = ChooseText(recentWorldWritebackLog3Text, "None");
        result.CityStatusChangeSummaryText = ChooseText(result.WorldWritebackSummaryText, result.SelectedWorldWritebackText);
        result.ExpeditionLogEntryText = ChooseText(result.LastExpeditionResultText, result.PostRunSummaryText);
        result.PartyConditionText = ChooseText(safeExpeditionResult.PartyConditionText, "None");
        result.PartyHpSummaryText = ChooseText(safeExpeditionResult.PartyHpSummaryText, "None");
        result.EliteSummaryText = ChooseMeaningfulText(safeExpeditionResult.EliteOutcomeSummaryText, safeExpeditionResult.EliteRewardLabel);
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
            ChooseMeaningfulText(result.LatestReturnAftermathSummaryText, result.WorldWritebackSummaryText));
        result.NextSuggestedActionText = ChooseMeaningfulText(
            safeOutcomeReadback.NextSuggestedActionText,
            ChooseMeaningfulText(safeCityWriteback.FollowUpHintText, result.NextSuggestedActionText));
        result.NextPrepFollowUpSummaryText = ChooseMeaningfulText(
            safeOutcomeReadback.FollowUpHintText,
            ChooseMeaningfulText(result.NextPrepFollowUpSummaryText, result.NextSuggestedActionText));
        result.GrowthRevealSummaryText = ChooseMeaningfulText(
            safeOutcomeReadback.GrowthRevealSummaryText,
            result.GrowthRevealSummaryText);
        result.LatestGrowthHighlightText = ChooseMeaningfulText(
            safeOutcomeReadback.LatestGrowthHighlightText,
            ChooseMeaningfulText(result.LatestGrowthHighlightText, result.LatestReturnAftermathSummaryText));
        result.NextRunGrowthPreviewText = ChooseMeaningfulText(
            safeOutcomeReadback.NextRunGrowthPreviewText,
            ChooseMeaningfulText(result.NextRunGrowthPreviewText, result.NextPrepFollowUpSummaryText));
        result.InspectEquipmentHintText = ChooseMeaningfulText(
            safeOutcomeReadback.InspectEquipmentHintText,
            result.InspectEquipmentHintText);
        RefreshGrowthRevealSummaries(result);
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

    private static PrototypeRpgRewardBundle[] CopyRewardBundles(PrototypeRpgRewardBundle[] source)
    {
        PrototypeRpgRewardBundle[] safeSource = source ?? Array.Empty<PrototypeRpgRewardBundle>();
        if (safeSource.Length <= 0)
        {
            return Array.Empty<PrototypeRpgRewardBundle>();
        }

        PrototypeRpgRewardBundle[] copies = new PrototypeRpgRewardBundle[safeSource.Length];
        for (int i = 0; i < safeSource.Length; i++)
        {
            PrototypeRpgRewardBundle bundle = safeSource[i] ?? new PrototypeRpgRewardBundle();
            copies[i] = new PrototypeRpgRewardBundle
            {
                RewardId = Value(bundle.RewardId),
                RewardLabel = ChooseText(bundle.RewardLabel, "None"),
                Amount = NonNegative(bundle.Amount)
            };
        }

        return copies;
    }

    private static PrototypeRpgMemberProgressionResult[] CopyMemberProgressionResults(PrototypeRpgMemberProgressionResult[] source)
    {
        PrototypeRpgMemberProgressionResult[] safeSource = source ?? Array.Empty<PrototypeRpgMemberProgressionResult>();
        if (safeSource.Length <= 0)
        {
            return Array.Empty<PrototypeRpgMemberProgressionResult>();
        }

        PrototypeRpgMemberProgressionResult[] copies = new PrototypeRpgMemberProgressionResult[safeSource.Length];
        for (int i = 0; i < safeSource.Length; i++)
        {
            PrototypeRpgMemberProgressionResult item = safeSource[i] ?? new PrototypeRpgMemberProgressionResult();
            copies[i] = new PrototypeRpgMemberProgressionResult
            {
                MemberId = Value(item.MemberId),
                DisplayName = ChooseText(item.DisplayName, "Adventurer"),
                RoleTag = Value(item.RoleTag),
                RoleLabel = ChooseText(item.RoleLabel, "Adventurer"),
                LevelBefore = item.LevelBefore > 0 ? item.LevelBefore : 1,
                LevelAfter = item.LevelAfter > 0 ? item.LevelAfter : 1,
                ExperienceBefore = NonNegative(item.ExperienceBefore),
                ExperienceAfter = NonNegative(item.ExperienceAfter),
                ExperienceGained = NonNegative(item.ExperienceGained),
                NextLevelExperience = item.NextLevelExperience > 0 ? item.NextLevelExperience : PrototypeRpgMemberProgressionRules.GetNextLevelExperience(1),
                GrowthBonusMaxHp = item.GrowthBonusMaxHp,
                GrowthBonusAttack = item.GrowthBonusAttack,
                GrowthBonusDefense = item.GrowthBonusDefense,
                GrowthBonusSpeed = item.GrowthBonusSpeed,
                LeveledUp = item.LeveledUp,
                GrowthSummaryText = ChooseText(item.GrowthSummaryText, "None"),
                RewardDropSummaryText = ChooseText(item.RewardDropSummaryText, "None"),
                RewardBundles = CopyRewardBundles(item.RewardBundles)
            };
        }

        return copies;
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
        copy.PendingRewardSummaryText = source.PendingRewardSummaryText;
        copy.GrowthRevealSummaryText = source.GrowthRevealSummaryText;
        copy.LatestGrowthHighlightText = source.LatestGrowthHighlightText;
        copy.NextRunGrowthPreviewText = source.NextRunGrowthPreviewText;
        copy.InspectEquipmentHintText = source.InspectEquipmentHintText;
        copy.WorldWritebackSummaryText = source.WorldWritebackSummaryText;
        copy.LatestReturnAftermathSummaryText = source.LatestReturnAftermathSummaryText;
        copy.NextSuggestedActionText = source.NextSuggestedActionText;
        copy.NextPrepFollowUpSummaryText = source.NextPrepFollowUpSummaryText;
        copy.PendingRewardBundles = CopyRewardBundles(source.PendingRewardBundles);
        copy.MemberProgressionResults = CopyMemberProgressionResults(source.MemberProgressionResults);
        copy.BattleResult = CopyBattleResult(source.BattleResult);
        return copy;
    }

    private static void ApplyProgressionOutput(ExpeditionResult result, PostRunProgressionOutput progressionOutput)
    {
        if (result == null || progressionOutput == null)
        {
            return;
        }

        result.PendingRewardBundles = CopyRewardBundles(progressionOutput.PendingRewardBundles);
        result.PendingRewardSummaryText = ChooseMeaningfulText(progressionOutput.PendingRewardSummaryText, "None");
        result.MemberProgressionResults = CopyMemberProgressionResults(progressionOutput.MemberProgressionResults);
        if (IsMeaningfulText(result.PendingRewardSummaryText))
        {
            string stashText = "Hidden Stash " + result.PendingRewardSummaryText;
            result.LootBreakdownSummaryText = ChooseMeaningfulText(
                AppendSummarySegment(result.LootBreakdownSummaryText, stashText),
                result.LootBreakdownSummaryText);
        }

        string progressionAftermath = BuildProgressionAftermathSummary(result.MemberProgressionResults, result.PendingRewardSummaryText);
        string progressionFollowUp = BuildProgressionFollowUpSummary(result.MemberProgressionResults, result.PendingRewardSummaryText);
        result.LatestReturnAftermathSummaryText = ChooseMeaningfulText(
            progressionAftermath,
            result.LatestReturnAftermathSummaryText);
        result.NextPrepFollowUpSummaryText = ChooseMeaningfulText(
            progressionFollowUp,
            result.NextPrepFollowUpSummaryText);
        result.NextSuggestedActionText = ChooseMeaningfulText(result.NextPrepFollowUpSummaryText, result.NextSuggestedActionText);
    }

    public static void RefreshGrowthRevealSummaries(ExpeditionResult result)
    {
        if (result == null)
        {
            return;
        }

        result.GrowthRevealSummaryText = BuildGrowthRevealSummary(result.MemberProgressionResults);
        result.LatestGrowthHighlightText = BuildLatestGrowthHighlightSummary(
            result.MemberProgressionResults,
            result.GearRewardCandidateSummaryText,
            result.EquipSwapChoiceSummaryText,
            result.PendingRewardSummaryText);
        result.NextRunGrowthPreviewText = BuildNextRunGrowthPreviewSummary(
            result.MemberProgressionResults,
            result.EquipSwapChoiceSummaryText,
            result.GearCarryContinuitySummaryText);
        result.InspectEquipmentHintText = HasGrowthRevealContent(result)
            ? "Press [I] to inspect equipment."
            : "None";

        if (IsMeaningfulText(result.LatestGrowthHighlightText))
        {
            result.LatestReturnAftermathSummaryText = result.LatestGrowthHighlightText;
        }
        else if (!IsMeaningfulText(result.LatestReturnAftermathSummaryText) && IsMeaningfulText(result.GrowthRevealSummaryText))
        {
            result.LatestReturnAftermathSummaryText = CompactSummaryText(result.GrowthRevealSummaryText.Replace("\n", " | "), 156);
        }

        if (!IsMeaningfulText(result.NextPrepFollowUpSummaryText) && IsMeaningfulText(result.NextRunGrowthPreviewText))
        {
            result.NextPrepFollowUpSummaryText = result.NextRunGrowthPreviewText;
        }
    }

    private static string ChooseMeaningfulText(string primary, string fallback)
    {
        if (IsMeaningfulText(primary))
        {
            return primary;
        }

        return IsMeaningfulText(fallback) ? fallback : "None";
    }

    private static bool HasGrowthRevealContent(ExpeditionResult result)
    {
        return result != null &&
            (IsMeaningfulText(result.GrowthRevealSummaryText) ||
             IsMeaningfulText(result.GearRewardCandidateSummaryText) ||
             IsMeaningfulText(result.EquipSwapChoiceSummaryText) ||
             IsMeaningfulText(result.PendingRewardSummaryText));
    }

    private static string BuildGrowthRevealSummary(PrototypeRpgMemberProgressionResult[] memberResults)
    {
        PrototypeRpgMemberProgressionResult[] safeResults = memberResults ?? Array.Empty<PrototypeRpgMemberProgressionResult>();
        List<string> lines = new List<string>();
        for (int i = 0; i < safeResults.Length && lines.Count < 2; i++)
        {
            string line = BuildGrowthRevealMemberLine(safeResults[i]);
            if (IsMeaningfulText(line))
            {
                lines.Add(line);
            }
        }

        if (safeResults.Length > lines.Count && lines.Count > 0)
        {
            int remaining = safeResults.Length - lines.Count;
            lines.Add("+" + remaining + " more member change" + (remaining == 1 ? string.Empty : "s"));
        }

        return lines.Count > 0
            ? string.Join("\n", lines.ToArray())
            : "No level-up. XP carried forward.";
    }

    private static string BuildGrowthRevealMemberLine(PrototypeRpgMemberProgressionResult result)
    {
        if (result == null)
        {
            return string.Empty;
        }

        string displayName = IsMeaningfulText(result.DisplayName) ? result.DisplayName : "Party";
        string levelText = result.LevelAfter > result.LevelBefore
            ? "Lv" + result.LevelBefore + " -> Lv" + result.LevelAfter
            : "Lv" + result.LevelAfter + " " + Max(0, result.ExperienceAfter) + "/" + Max(1, result.NextLevelExperience);
        string line = displayName + " gained " + Max(0, result.ExperienceGained) + " XP | " + levelText;
        string statText = BuildGrowthStatDeltaSummary(result);
        return IsMeaningfulText(statText)
            ? line + " | " + statText
            : line;
    }

    private static string BuildGrowthStatDeltaSummary(PrototypeRpgMemberProgressionResult result)
    {
        if (result == null)
        {
            return string.Empty;
        }

        List<string> parts = new List<string>();
        if (result.GrowthBonusMaxHp > 0)
        {
            parts.Add("HP +" + result.GrowthBonusMaxHp);
        }

        if (result.GrowthBonusAttack > 0)
        {
            parts.Add("ATK +" + result.GrowthBonusAttack);
        }

        if (result.GrowthBonusDefense > 0)
        {
            parts.Add("DEF +" + result.GrowthBonusDefense);
        }

        if (result.GrowthBonusSpeed > 0)
        {
            parts.Add("SPD +" + result.GrowthBonusSpeed);
        }

        return parts.Count > 0 ? string.Join(", ", parts.ToArray()) : string.Empty;
    }

    private static string BuildLatestGrowthHighlightSummary(
        PrototypeRpgMemberProgressionResult[] memberResults,
        string gearRewardSummaryText,
        string equipSwapSummaryText,
        string pendingRewardSummaryText)
    {
        PrototypeRpgMemberProgressionResult[] safeResults = memberResults ?? Array.Empty<PrototypeRpgMemberProgressionResult>();
        string highlight = string.Empty;
        for (int i = 0; i < safeResults.Length; i++)
        {
            string memberHighlight = BuildCompactGrowthHighlight(safeResults[i]);
            if (IsMeaningfulText(memberHighlight))
            {
                highlight = memberHighlight;
                break;
            }
        }

        string gearHighlight = ChooseMeaningfulText(equipSwapSummaryText, gearRewardSummaryText);
        if (IsMeaningfulText(gearHighlight))
        {
            highlight = AppendSummarySegment(highlight, CompactSummaryText(gearHighlight, 88));
        }
        else if (IsMeaningfulText(pendingRewardSummaryText))
        {
            highlight = AppendSummarySegment(highlight, "Stash " + CompactSummaryText(pendingRewardSummaryText, 56));
        }

        return IsMeaningfulText(highlight) ? highlight : "None";
    }

    private static string BuildCompactGrowthHighlight(PrototypeRpgMemberProgressionResult result)
    {
        if (result == null)
        {
            return string.Empty;
        }

        string displayName = IsMeaningfulText(result.DisplayName) ? result.DisplayName : "Party";
        string levelText = result.LevelAfter > result.LevelBefore
            ? displayName + " Lv" + result.LevelAfter
            : displayName + " +" + Max(0, result.ExperienceGained) + " XP";
        string statText = BuildGrowthStatDeltaSummary(result);
        return IsMeaningfulText(statText)
            ? levelText + " | " + statText
            : levelText;
    }

    private static string BuildNextRunGrowthPreviewSummary(
        PrototypeRpgMemberProgressionResult[] memberResults,
        string equipSwapSummaryText,
        string gearContinuitySummaryText)
    {
        PrototypeRpgMemberProgressionResult[] safeResults = memberResults ?? Array.Empty<PrototypeRpgMemberProgressionResult>();
        for (int i = 0; i < safeResults.Length; i++)
        {
            string preview = BuildMemberNextRunGrowthPreview(safeResults[i]);
            if (IsMeaningfulText(preview))
            {
                return preview;
            }
        }

        string equipmentPreview = BuildEquipmentNextRunPreview(equipSwapSummaryText);
        if (IsMeaningfulText(equipmentPreview))
        {
            return equipmentPreview;
        }

        return IsMeaningfulText(gearContinuitySummaryText)
            ? "Next run: current gear continuity is locked in."
            : "None";
    }

    private static string BuildMemberNextRunGrowthPreview(PrototypeRpgMemberProgressionResult result)
    {
        if (result == null)
        {
            return string.Empty;
        }

        string displayName = IsMeaningfulText(result.DisplayName) ? result.DisplayName : "This member";
        if (result.GrowthBonusAttack > 0)
        {
            return "Next run: " + displayName + "'s attacks hit harder.";
        }

        if (result.GrowthBonusSpeed > 0)
        {
            return "Next run: " + displayName + " acts earlier more often.";
        }

        if (result.GrowthBonusMaxHp > 0 || result.GrowthBonusDefense > 0)
        {
            return "Next run: " + displayName + " survives more safely.";
        }

        if (result.LeveledUp)
        {
            return "Next run: " + displayName + " brings stronger overall stats.";
        }

        return string.Empty;
    }

    private static string BuildEquipmentNextRunPreview(string equipSwapSummaryText)
    {
        if (!IsMeaningfulText(equipSwapSummaryText))
        {
            return string.Empty;
        }

        if (ContainsText(equipSwapSummaryText, "ATK"))
        {
            return "Next run: party basic attacks hit harder.";
        }

        if (ContainsText(equipSwapSummaryText, "SPD"))
        {
            return "Next run: party acts earlier more often.";
        }

        if (ContainsText(equipSwapSummaryText, "HP") || ContainsText(equipSwapSummaryText, "DEF"))
        {
            return "Next run: party sustain is safer.";
        }

        if (ContainsText(equipSwapSummaryText, "POW"))
        {
            return "Next run: party skills hit harder.";
        }

        return "Next run: the latest equipment upgrade is active.";
    }

    private static bool ContainsText(string value, string token)
    {
        return !string.IsNullOrEmpty(value) &&
            !string.IsNullOrEmpty(token) &&
            value.IndexOf(token, StringComparison.OrdinalIgnoreCase) >= 0;
    }

    private static string BuildEliteSummary(PrototypeRpgEliteOutcomeSnapshot eliteOutcome)
    {
        PrototypeRpgEliteOutcomeSnapshot safeEliteOutcome = eliteOutcome ?? new PrototypeRpgEliteOutcomeSnapshot();
        if (safeEliteOutcome.IsEliteDefeated)
        {
            return HasText(safeEliteOutcome.EliteRewardLabel)
                ? safeEliteOutcome.EliteRewardLabel
                : HasText(safeEliteOutcome.EliteName)
                    ? safeEliteOutcome.EliteName
                    : "Elite defeated";
        }

        return HasText(safeEliteOutcome.EliteTypeLabel) ? safeEliteOutcome.EliteTypeLabel : "None";
    }

    private static string BuildReturnedLootSummary(string rewardResourceId, int returnedLootAmount)
    {
        return HasText(rewardResourceId) && returnedLootAmount > 0
            ? rewardResourceId + " x" + returnedLootAmount
            : "None";
    }

    private static string BuildPartyGrowthAftermathSummary(PrototypeRpgPartyOutcomeSnapshot partyOutcome)
    {
        PrototypeRpgPartyMemberOutcomeSnapshot[] members = partyOutcome != null
            ? partyOutcome.Members
            : null;
        if (members == null || members.Length <= 0)
        {
            return "None";
        }

        string summary = string.Empty;
        int written = 0;
        for (int i = 0; i < members.Length && written < 2; i++)
        {
            PrototypeRpgPartyMemberOutcomeSnapshot member = members[i] ?? new PrototypeRpgPartyMemberOutcomeSnapshot();
            string growthText = ExtractSummaryClauseText(member.AppliedProgressionSummaryText, "Growth");
            string detail = ChooseMeaningfulText(growthText, ChooseMeaningfulText(member.AppliedProgressionSummaryText, member.CurrentRunSummaryText));
            if (!IsMeaningfulText(detail))
            {
                continue;
            }

            string displayName = IsMeaningfulText(member.DisplayName) ? member.DisplayName : "Party";
            string segment = displayName + ": " + CompactSummaryText(detail, 112);
            summary = string.IsNullOrEmpty(summary) ? segment : summary + " | " + segment;
            written++;
        }

        return IsMeaningfulText(summary) ? summary : "None";
    }

    private static string BuildPartyGrowthFollowUpSummary(PrototypeRpgPartyOutcomeSnapshot partyOutcome)
    {
        PrototypeRpgPartyMemberOutcomeSnapshot[] members = partyOutcome != null
            ? partyOutcome.Members
            : null;
        if (members == null || members.Length <= 0)
        {
            return "None";
        }

        string summary = string.Empty;
        int written = 0;
        for (int i = 0; i < members.Length && written < 2; i++)
        {
            PrototypeRpgPartyMemberOutcomeSnapshot member = members[i] ?? new PrototypeRpgPartyMemberOutcomeSnapshot();
            string nextDispatchText = ExtractSummaryClauseText(member.NextRunPreviewSummaryText, "Next Dispatch");
            string detail = ChooseMeaningfulText(nextDispatchText, ChooseMeaningfulText(member.NextRunPreviewSummaryText, member.CurrentRunSummaryText));
            if (!IsMeaningfulText(detail))
            {
                continue;
            }

            string displayName = IsMeaningfulText(member.DisplayName) ? member.DisplayName : "Party";
            string segment = displayName + ": " + CompactSummaryText(detail, 112);
            summary = string.IsNullOrEmpty(summary) ? segment : summary + " | " + segment;
            written++;
        }

        return IsMeaningfulText(summary) ? summary : "None";
    }

    private static string BuildProgressionAftermathSummary(
        PrototypeRpgMemberProgressionResult[] memberResults,
        string pendingRewardSummaryText)
    {
        PrototypeRpgMemberProgressionResult[] safeResults = memberResults ?? Array.Empty<PrototypeRpgMemberProgressionResult>();
        string summary = string.Empty;
        int written = 0;
        for (int i = 0; i < safeResults.Length && written < 2; i++)
        {
            PrototypeRpgMemberProgressionResult result = safeResults[i] ?? new PrototypeRpgMemberProgressionResult();
            if (!IsMeaningfulText(result.GrowthSummaryText))
            {
                continue;
            }

            string displayName = IsMeaningfulText(result.DisplayName) ? result.DisplayName : "Party";
            string segment = displayName + ": " + CompactSummaryText(result.GrowthSummaryText, 112);
            summary = string.IsNullOrEmpty(summary) ? segment : summary + " | " + segment;
            written++;
        }

        if (IsMeaningfulText(pendingRewardSummaryText))
        {
            summary = AppendSummarySegment(summary, "Stash " + pendingRewardSummaryText);
        }

        return IsMeaningfulText(summary) ? summary : "None";
    }

    private static string BuildProgressionFollowUpSummary(
        PrototypeRpgMemberProgressionResult[] memberResults,
        string pendingRewardSummaryText)
    {
        PrototypeRpgMemberProgressionResult[] safeResults = memberResults ?? Array.Empty<PrototypeRpgMemberProgressionResult>();
        string summary = string.Empty;
        int written = 0;
        for (int i = 0; i < safeResults.Length && written < 2; i++)
        {
            PrototypeRpgMemberProgressionResult result = safeResults[i] ?? new PrototypeRpgMemberProgressionResult();
            string nextLevelText = PrototypeRpgMemberProgressionRules.BuildNextLevelHintText(
                result.LevelAfter,
                result.ExperienceAfter,
                result.NextLevelExperience);
            string detail = AppendSummarySegment(nextLevelText, IsMeaningfulText(result.RewardDropSummaryText) ? "Loot " + result.RewardDropSummaryText : string.Empty);
            if (!IsMeaningfulText(detail))
            {
                continue;
            }

            string displayName = IsMeaningfulText(result.DisplayName) ? result.DisplayName : "Party";
            string segment = displayName + ": " + CompactSummaryText(detail, 112);
            summary = string.IsNullOrEmpty(summary) ? segment : summary + " | " + segment;
            written++;
        }

        if (IsMeaningfulText(pendingRewardSummaryText))
        {
            summary = AppendSummarySegment(summary, "Stash " + pendingRewardSummaryText);
        }

        return IsMeaningfulText(summary) ? summary : "None";
    }

    private static string ExtractSummaryClauseText(string summaryText, string label)
    {
        if (!IsMeaningfulText(summaryText) || !IsMeaningfulText(label))
        {
            return string.Empty;
        }

        string[] clauses = summaryText.Split('|');
        string prefix = label.Trim() + " ";
        for (int i = 0; i < clauses.Length; i++)
        {
            string clause = clauses[i] != null ? clauses[i].Trim() : string.Empty;
            if (clause.StartsWith(prefix))
            {
                return clause.Substring(prefix.Length).Trim();
            }
        }

        return string.Empty;
    }

    private static string CompactSummaryText(string text, int maxLength)
    {
        if (!IsMeaningfulText(text))
        {
            return string.Empty;
        }

        string trimmed = text.Trim();
        if (trimmed.Length <= maxLength || maxLength < 4)
        {
            return trimmed;
        }

        return trimmed.Substring(0, maxLength - 3).TrimEnd() + "...";
    }

    private static string AppendSummarySegment(string existing, string segment)
    {
        if (!IsMeaningfulText(segment))
        {
            return existing;
        }

        return IsMeaningfulText(existing) ? existing + " | " + segment : segment;
    }

    private static string Value(string value)
    {
        return string.IsNullOrEmpty(value) ? string.Empty : value;
    }

    private static void PopulateSharedOutcomeMeaning(ExpeditionOutcome result, string routeId)
    {
        if (result == null || !HasText(result.SourceCityId) || !HasText(result.TargetDungeonId))
        {
            return;
        }

        GoldenPathChainDefinition chainDefinition = null;
        GoldenPathOutcomeMeaningDefinition outcomeMeaning = null;
        if (!GoldenPathContentRegistry.TryGetOutcomeMeaningForChain(result.SourceCityId, result.TargetDungeonId, routeId, out chainDefinition, out outcomeMeaning) &&
            !GoldenPathContentRegistry.TryGetOutcomeMeaningForChain(result.SourceCityId, result.TargetDungeonId, out chainDefinition, out outcomeMeaning))
        {
            if (!GoldenPathContentRegistry.TryGetChain(result.SourceCityId, result.TargetDungeonId, out chainDefinition))
            {
                return;
            }
        }

        result.OutcomeMeaningId = ChooseValue(
            chainDefinition != null ? chainDefinition.OutcomeMeaningId : string.Empty,
            outcomeMeaning != null ? outcomeMeaning.OutcomeMeaningId : result.OutcomeMeaningId);
        result.OutcomeRewardMeaningText = ChooseText(
            chainDefinition != null ? chainDefinition.RewardMeaningText : string.Empty,
            outcomeMeaning != null ? outcomeMeaning.RewardMeaningText : result.OutcomeRewardMeaningText);
        result.CityImpactMeaningText = ChooseText(
            chainDefinition != null ? chainDefinition.ResultImpactMeaningText : string.Empty,
            outcomeMeaning != null ? outcomeMeaning.CityImpactMeaningText : result.CityImpactMeaningText);
        result.RecommendationShiftText = ChooseText(
            outcomeMeaning != null ? outcomeMeaning.RecommendationShiftText : string.Empty,
            result.RecommendationShiftText);
    }

    private static int NonNegative(int value)
    {
        return value > 0 ? value : 0;
    }

    private static int Max(int left, int right)
    {
        return left > right ? left : right;
    }

    private static bool HasText(string value)
    {
        return !string.IsNullOrEmpty(value) && value != "None";
    }

    private static bool IsMeaningfulText(string value)
    {
        return !string.IsNullOrEmpty(value) && value != "None" && value != "(missing)";
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
}
