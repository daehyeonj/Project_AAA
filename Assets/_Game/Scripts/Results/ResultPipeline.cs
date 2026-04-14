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
        return result;
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

    private static bool HasText(string value)
    {
        return !string.IsNullOrEmpty(value) && value != "None";
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
