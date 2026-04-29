using System;
using UnityEditor;
using UnityEngine;

public static class Batch79_2ResultPipelineIntentPackagingProofRunner
{
    public static void RunBatch79_2ResultPipelineIntentPackagingProof()
    {
        try
        {
            RunProof();
            Debug.Log("[Batch79_2Proof] PASS :: ResultPipeline intent packaging proof completed.");
            EditorApplication.Exit(0);
        }
        catch (Exception ex)
        {
            Debug.LogError("[Batch79_2Proof] FAIL :: " + ex);
            EditorApplication.Exit(1);
        }
    }

    private static void RunProof()
    {
        const string cityId = "city-a";
        const string cityLabel = "City A";
        const string dungeonId = "dungeon-alpha";
        const string dungeonLabel = "Dungeon Alpha";
        const string routeId = "safe";
        const string routeLabel = "Rest Path";
        const string objective = "Stability Run objective";
        const string relevance = "Best route for stabilizing City A's next dispatch window.";
        const string riskReward = "Rest Path | Stability Run | Low Risk";
        const string runPath = "Start Room -> Slime Front -> Rest Shrine -> Elite Chamber -> Exit Route";

        PrototypeRpgRunResultSnapshot snapshot = new PrototypeRpgRunResultSnapshot();
        snapshot.ResultStateKey = PrototypeBattleOutcomeKeys.RunClear;
        snapshot.DungeonId = dungeonId;
        snapshot.DungeonLabel = dungeonLabel;
        snapshot.RouteId = routeId;
        snapshot.RouteLabel = routeLabel;
        snapshot.ResultSummary = "Party 1 cleared Dungeon Alpha and returned with Arcane Shard x3.";
        snapshot.SurvivingMembersSummary = "Alden, Mira";
        snapshot.PartyOutcome = new PrototypeRpgPartyOutcomeSnapshot
        {
            PartyConditionText = "Stable",
            PartyHpSummaryText = "Alden 24/24 | Mira 18/18",
            PartyMembersAtEndSummary = "Alden, Mira",
            SurvivingMemberCount = 2,
            KnockedOutMemberCount = 0
        };
        snapshot.LootOutcome = new PrototypeRpgLootOutcomeSnapshot
        {
            TotalLootGained = 3,
            BattleLootGained = 2,
            ChestLootGained = 1,
            FinalLootSummary = "Arcane Shard x3"
        };
        snapshot.EliteOutcome = new PrototypeRpgEliteOutcomeSnapshot
        {
            IsEliteDefeated = true,
            EliteName = "Slime Monarch",
            EliteTypeLabel = "Elite",
            EliteRewardLabel = "Arcane Shard",
            EliteRewardAmount = 3
        };
        snapshot.EncounterOutcome = new PrototypeRpgEncounterOutcomeSnapshot
        {
            ClearedEncounterCount = 3,
            ClearedEncounterSummary = "Slime Front, Watch Hall, Slime Monarch",
            OpenedChestCount = 1,
            RoomPathSummary = runPath,
            SelectedEventChoice = "Rest Shrine"
        };

        PrototypeDungeonRunResultContext context = new PrototypeDungeonRunResultContext();
        context.CanonicalRunResult = snapshot;
        context.DungeonId = dungeonId;
        context.DungeonLabel = dungeonLabel;
        context.RouteId = routeId;
        context.RouteLabel = routeLabel;
        context.RunOutcomeKey = PrototypeBattleOutcomeKeys.RunClear;
        context.ResultSummaryText = snapshot.ResultSummary;
        context.SurvivingMembersSummaryText = snapshot.SurvivingMembersSummary;
        context.ResourceDeltaSummaryText = snapshot.LootOutcome.FinalLootSummary;
        context.MissionObjectiveText = objective;
        context.MissionRelevanceText = relevance;
        context.RiskRewardContextText = riskReward;
        context.SelectedRouteSummaryText = riskReward;
        context.KeyEncounterSummaryText = "Battles 3 | Slime Monarch | 3 / 3";
        context.BattleContextSummaryText = "Slime-heavy sustain fights";
        context.WorldWritebackDungeonSummaryText = dungeonLabel + " | " + routeLabel;
        context.WorldWritebackRouteSummaryText = riskReward;

        BattleResult battleResult = new BattleResult();
        battleResult.ResultStateKey = PrototypeBattleOutcomeKeys.RunClear;
        battleResult.DungeonId = dungeonId;
        battleResult.DungeonLabel = dungeonLabel;
        battleResult.RouteId = routeId;
        battleResult.RouteLabel = routeLabel;
        battleResult.SurvivingMemberCount = 2;
        battleResult.KnockedOutMemberCount = 0;
        battleResult.BattleContextSummaryText = context.BattleContextSummaryText;

        PostRunResolutionInput input = new PostRunResolutionInput();
        input.CompatibilityRunResultContext = context;
        input.BattleResult = battleResult;
        input.SourceCityId = cityId;
        input.SourceCityLabel = cityLabel;
        input.RewardResourceId = "Arcane Shard";
        input.MissionObjectiveText = objective;
        input.MissionRelevanceText = relevance;
        input.RiskRewardContextText = riskReward;
        input.Success = true;
        input.ReturnedLootAmount = 3;
        input.ElapsedDays = 1;

        ExpeditionResult expeditionResult = ResultPipeline.BuildExpeditionResult(input);
        AssertEqual("ExpeditionResult objective", objective, expeditionResult.MissionObjectiveText);
        AssertEqual("ExpeditionResult relevance", relevance, expeditionResult.MissionRelevanceText);
        AssertEqual("ExpeditionResult risk/reward", riskReward, expeditionResult.RiskRewardContextText);
        AssertEqual("ExpeditionResult run path", runPath, expeditionResult.RoomPathSummaryText);

        ExpeditionOutcome outcome = ResultPipeline.BuildExpeditionOutcome(expeditionResult);
        AssertEqual("ExpeditionOutcome objective", objective, outcome.MissionObjectiveText);
        AssertEqual("ExpeditionOutcome relevance", relevance, outcome.MissionRelevanceText);
        AssertEqual("ExpeditionOutcome risk/reward", riskReward, outcome.RiskRewardContextText);
        AssertEqual("ExpeditionOutcome run path", runPath, outcome.RunPathSummaryText);
        AssertEqual("ExpeditionOutcome result state", PrototypeBattleOutcomeKeys.RunClear, outcome.ResultStateKey);

        CityWriteback cityWriteback = ResultPipeline.BuildCityWriteback(
            cityId,
            cityLabel,
            expeditionResult.ResultStateKey,
            expeditionResult.RewardResourceId,
            expeditionResult.ReturnedLootAmount,
            expeditionResult.ReturnedLootSummaryText,
            expeditionResult.PartyConditionText,
            cityLabel + " absorbed Arcane Shard x3.",
            0,
            3,
            3,
            "Urgent",
            "Recovering",
            "Recovering",
            "Ready Soon",
            "1d",
            "Keep the next dispatch stable.",
            "City A stock recovered.");
        WorldWriteback worldWriteback = ResultPipeline.BuildWorldWriteback(
            expeditionResult,
            cityWriteback,
            "City A absorbed Arcane Shard x3.",
            "available",
            "Dungeon Alpha is available.",
            "Next dispatch window is steady.",
            expeditionResult.ResultSummaryText);
        OutcomeReadback readback = ResultPipeline.BuildOutcomeReadback(
            expeditionResult,
            cityWriteback,
            worldWriteback,
            worldWriteback.WritebackSummaryText,
            "Party returned with a stable result.",
            cityId,
            cityLabel,
            expeditionResult.ResultStateKey,
            "Result accepted.",
            expeditionResult.NextSuggestedActionText,
            expeditionResult.NextPrepFollowUpSummaryText,
            expeditionResult.ResultSummaryText,
            expeditionResult.SurvivingMembersSummaryText,
            expeditionResult.ClearedEncounterSummaryText,
            expeditionResult.SelectedEventChoiceText,
            expeditionResult.LootBreakdownSummaryText,
            expeditionResult.DungeonSummaryText,
            expeditionResult.SelectedRouteSummaryText,
            "0",
            "3",
            "3",
            "None",
            "None",
            "None",
            "Run log 1",
            "None",
            "None",
            "Writeback log 1",
            "None",
            "None");

        AssertEqual("OutcomeReadback objective", objective, readback.MissionObjectiveText);
        AssertEqual("OutcomeReadback relevance", relevance, readback.MissionRelevanceText);
        AssertEqual("OutcomeReadback risk/reward", riskReward, readback.RiskRewardContextText);
        AssertEqual("OutcomeReadback run path", runPath, readback.RunPathSummaryText);
        AssertEqual("WorldWriteback route", riskReward, worldWriteback.RouteSummaryText);

        Debug.Log(
            "[Batch79_2Proof] PASS :: Package/consumer fields stable. " +
            "Objective=" + objective +
            " Relevance=" + relevance +
            " RiskReward=" + riskReward +
            " RunPath=" + runPath + ".");
    }

    private static void AssertEqual(string label, string expected, string actual)
    {
        if (expected == actual)
        {
            return;
        }

        throw new InvalidOperationException(label + " mismatch. Expected=[" + expected + "] Actual=[" + actual + "]");
    }
}
