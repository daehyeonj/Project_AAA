using System;
using System.Collections.Generic;

public static class DungeonRunStateFactory
{
    public static ExpeditionRunState Create(ExpeditionRunStateSeed seed)
    {
        ExpeditionRunStateSeed safeSeed = seed ?? new ExpeditionRunStateSeed();
        ExpeditionRunState state = new ExpeditionRunState();
        state.RunId = HasText(safeSeed.RunId)
            ? safeSeed.RunId
            : BuildFallbackRunId(safeSeed);
        state.WorldDayCount = safeSeed.WorldDayCount;
        state.LaunchPlan = safeSeed.LaunchPlan ?? new ExpeditionPlan();
        state.Phase = safeSeed.Phase;
        state.Status = safeSeed.Status;
        state.OriginCityId = safeSeed.OriginCityId ?? string.Empty;
        state.OriginCityLabel = DefaultText(safeSeed.OriginCityLabel, "None");
        state.TargetDungeonId = safeSeed.TargetDungeonId ?? string.Empty;
        state.TargetDungeonLabel = DefaultText(safeSeed.TargetDungeonLabel, "None");
        state.RouteId = safeSeed.RouteId ?? string.Empty;
        state.RouteLabel = DefaultText(safeSeed.RouteLabel, "None");
        state.RouteRiskText = DefaultText(safeSeed.RouteRiskText, "None");
        state.RouteContextText = DefaultText(safeSeed.RouteContextText, "None");
        state.ObjectiveText = DefaultText(safeSeed.ObjectiveText, "None");
        state.WhyNowText = DefaultText(safeSeed.WhyNowText, "None");
        state.ExpectedUsefulnessText = DefaultText(safeSeed.ExpectedUsefulnessText, "None");
        state.RiskRewardPreviewText = DefaultText(safeSeed.RiskRewardPreviewText, "None");
        state.PlanSummaryText = DefaultText(safeSeed.PlanSummaryText, "None");
        state.PartyId = safeSeed.PartyId ?? string.Empty;
        state.PartySummaryText = DefaultText(safeSeed.PartySummaryText, "None");
        state.PartyHpSummaryText = DefaultText(safeSeed.PartyHpSummaryText, "None");
        state.PartyConditionText = DefaultText(safeSeed.PartyConditionText, "None");
        state.SustainPressureText = DefaultText(safeSeed.SustainPressureText, "None");
        state.CurrentRoomId = safeSeed.CurrentRoomId ?? string.Empty;
        state.CurrentRoomLabel = DefaultText(safeSeed.CurrentRoomLabel, "None");
        state.CurrentRoomTypeLabel = DefaultText(safeSeed.CurrentRoomTypeLabel, "None");
        state.RoomProgressText = DefaultText(safeSeed.RoomProgressText, "None");
        state.NextGoalText = DefaultText(safeSeed.NextGoalText, "None");
        state.CurrentPromptText = DefaultText(safeSeed.CurrentPromptText, "None");
        state.ResultStateKey = DefaultText(safeSeed.ResultStateKey, PrototypeBattleOutcomeKeys.None);
        state.ResultSummaryText = DefaultText(safeSeed.ResultSummaryText, "None");
        state.RoomPathSummaryText = DefaultText(safeSeed.RoomPathSummaryText, "None");
        state.CurrentTurnCount = Math.Max(0, safeSeed.CurrentTurnCount);
        state.ClearedEncounterCount = Math.Max(0, safeSeed.ClearedEncounterCount);
        state.OpenedChestCount = Math.Max(0, safeSeed.OpenedChestCount);
        state.CarriedLootAmount = Math.Max(0, safeSeed.CarriedLootAmount);
        state.ExitUnlocked = safeSeed.ExitUnlocked;
        state.EliteDefeated = safeSeed.EliteDefeated;
        state.EventResolved = safeSeed.EventResolved;
        state.PreEliteDecisionResolved = safeSeed.PreEliteDecisionResolved;
        state.RunSucceeded = safeSeed.RunSucceeded;
        state.RunFailed = safeSeed.RunFailed;
        state.RunRetreated = safeSeed.RunRetreated;
        state.IsResolved = safeSeed.IsResolved;
        state.ActiveEncounter = safeSeed.ActiveEncounter ?? new EncounterContext();
        state.LastEventChoice = safeSeed.LastEventChoice ?? new EventChoiceResult();
        state.LastPreEliteChoice = safeSeed.LastPreEliteChoice ?? new EventChoiceResult();
        state.PendingBattle = safeSeed.PendingBattle ?? new BattleHandoffPayload();
        state.LastBattleReturn = safeSeed.LastBattleReturn ?? new BattleReturnPayload();
        state.ActiveBattleState = safeSeed.ActiveBattleState ?? new PrototypeBattleRuntimeState();
        state.ActiveBattleViewModel = safeSeed.ActiveBattleViewModel ?? new PrototypeBattleViewModel();
        state.LatestBattleSnapshot = safeSeed.LatestBattleSnapshot ?? new PrototypeBattleResultSnapshot();
        state.ResultSnapshot = safeSeed.ResultSnapshot ?? new PrototypeRpgRunResultSnapshot();
        state.RecentEntries = safeSeed.RecentEntries ?? Array.Empty<RunLogEntry>();
        return state;
    }

    public static DungeonRunReadModel BuildReadModel(ExpeditionRunState state)
    {
        ExpeditionRunState safeState = state ?? new ExpeditionRunState();
        DungeonRunReadModel model = new DungeonRunReadModel();
        model.ObjectiveText = FirstMeaningfulText(
            safeState.ObjectiveText,
            safeState.LaunchPlan != null ? safeState.LaunchPlan.ObjectiveText : string.Empty,
            "None");
        model.PhaseText = ToDisplayText(safeState.Phase);
        model.StatusText = ToDisplayText(safeState.Status);
        model.CurrentLocationText = BuildCurrentLocationText(safeState);
        model.CurrentChoiceText = FirstMeaningfulText(
            safeState.CurrentPromptText,
            safeState.ActiveEncounter != null ? safeState.ActiveEncounter.PromptText : string.Empty,
            "None");
        model.RouteContextText = FirstMeaningfulText(
            safeState.RouteContextText,
            BuildCompositeText(safeState.RouteLabel, safeState.RouteRiskText, safeState.RoomProgressText),
            "None");
        model.PartyStatusText = BuildCompositeText(
            safeState.PartySummaryText,
            safeState.PartyHpSummaryText,
            safeState.PartyConditionText);
        model.RiskRewardText = FirstMeaningfulText(
            safeState.RiskRewardPreviewText,
            safeState.LaunchPlan != null ? safeState.LaunchPlan.RiskRewardPreviewText : string.Empty,
            safeState.LaunchPlan != null && safeState.LaunchPlan.SelectedRoute != null
                ? safeState.LaunchPlan.SelectedRoute.RewardPreviewText
                : string.Empty,
            string.Empty,
            "None");
        model.NextGoalText = DefaultText(safeState.NextGoalText, "None");
        model.RecentOutcomeText = BuildRecentOutcomeText(safeState);
        model.BattleHandoffText = BuildBattleHandoffText(safeState.PendingBattle);
        model.BattleReturnText = BuildBattleReturnText(safeState.LastBattleReturn);
        model.ResultSummaryText = FirstMeaningfulText(
            safeState.ResultSummaryText,
            safeState.ResultSnapshot != null ? safeState.ResultSnapshot.ResultSummary : string.Empty,
            "None");
        model.RecentLogLines = BuildRecentLogLines(safeState.RecentEntries);
        return model;
    }

    private static string BuildFallbackRunId(ExpeditionRunStateSeed seed)
    {
        string cityId = HasText(seed != null ? seed.OriginCityId : string.Empty) ? seed.OriginCityId : "city";
        string dungeonId = HasText(seed != null ? seed.TargetDungeonId : string.Empty) ? seed.TargetDungeonId : "dungeon";
        string routeId = HasText(seed != null ? seed.RouteId : string.Empty) ? seed.RouteId : "route";
        int worldDay = seed != null ? Math.Max(0, seed.WorldDayCount) : 0;
        return cityId + "_" + dungeonId + "_" + routeId + "_day" + worldDay;
    }

    private static string BuildCurrentLocationText(ExpeditionRunState state)
    {
        List<string> parts = new List<string>();
        if (HasText(state != null ? state.CurrentRoomLabel : string.Empty))
        {
            parts.Add(state.CurrentRoomLabel);
        }

        if (HasText(state != null ? state.CurrentRoomTypeLabel : string.Empty))
        {
            parts.Add(state.CurrentRoomTypeLabel);
        }

        if (HasText(state != null ? state.RoomProgressText : string.Empty))
        {
            parts.Add(state.RoomProgressText);
        }

        return parts.Count > 0 ? string.Join(" | ", parts.ToArray()) : "None";
    }

    private static string BuildRecentOutcomeText(ExpeditionRunState state)
    {
        return FirstMeaningfulText(
            BuildBattleReturnText(state != null ? state.LastBattleReturn : null),
            state != null && state.LastPreEliteChoice != null && state.LastPreEliteChoice.Resolved ? state.LastPreEliteChoice.ResultSummaryText : string.Empty,
            state != null && state.LastEventChoice != null && state.LastEventChoice.Resolved ? state.LastEventChoice.ResultSummaryText : string.Empty,
            state != null ? state.ResultSummaryText : string.Empty,
            "None");
    }

    private static string BuildBattleHandoffText(BattleHandoffPayload payload)
    {
        if (payload == null || !HasText(payload.EncounterName))
        {
            return "None";
        }

        return BuildCompositeText(
            payload.EncounterName,
            payload.RouteLabel,
            payload.PartySummaryText);
    }

    private static string BuildBattleReturnText(BattleReturnPayload payload)
    {
        if (payload == null || (!HasText(payload.EncounterName) && !HasText(payload.ResultSummaryText)))
        {
            return "None";
        }

        return FirstMeaningfulText(
            BuildCompositeText(
                payload.ResultSummaryText,
                BuildBattleReturnMissionContextText(payload),
                BuildBattleReturnConsequenceText(payload)),
            BuildCompositeText(payload.EncounterName, payload.PartyConditionText, payload.LootSummaryText),
            "None");
    }

    private static string BuildBattleReturnMissionContextText(BattleReturnPayload payload)
    {
        if (payload == null)
        {
            return string.Empty;
        }

        string missionAnchorText = FirstMeaningfulText(
            payload.ExpectedUsefulnessText,
            payload.WhyNowText,
            payload.ObjectiveText,
            string.Empty,
            string.Empty);
        return BuildCompositeText(missionAnchorText, payload.LootSummaryText, string.Empty);
    }

    private static string BuildBattleReturnConsequenceText(BattleReturnPayload payload)
    {
        if (payload == null)
        {
            return string.Empty;
        }

        string progressText = BuildCompositeText(
            payload.EncounterRoomTypeText,
            payload.RoomProgressText,
            payload.NextGoalText);
        return BuildCompositeText(payload.PartyConditionText, progressText, string.Empty);
    }

    private static string[] BuildRecentLogLines(RunLogEntry[] entries)
    {
        if (entries == null || entries.Length == 0)
        {
            return Array.Empty<string>();
        }

        List<string> lines = new List<string>();
        for (int i = Math.Max(0, entries.Length - 3); i < entries.Length; i++)
        {
            RunLogEntry entry = entries[i];
            if (entry == null || !HasText(entry.SummaryText))
            {
                continue;
            }

            lines.Add(entry.SummaryText);
        }

        return lines.Count > 0 ? lines.ToArray() : Array.Empty<string>();
    }

    private static string BuildCompositeText(string value1, string value2, string value3)
    {
        List<string> parts = new List<string>();
        if (HasText(value1))
        {
            parts.Add(value1);
        }

        if (HasText(value2))
        {
            parts.Add(value2);
        }

        if (HasText(value3))
        {
            parts.Add(value3);
        }

        return parts.Count > 0 ? string.Join(" | ", parts.ToArray()) : "None";
    }

    private static string ToDisplayText(RunPhase phase)
    {
        switch (phase)
        {
            case RunPhase.Start:
                return "Start";
            case RunPhase.Exploring:
                return "Exploring";
            case RunPhase.EventChoice:
                return "Event Choice";
            case RunPhase.PreEliteChoice:
                return "Pre-Elite Choice";
            case RunPhase.PendingBattle:
                return "Pending Battle";
            case RunPhase.InBattle:
                return "In Battle";
            case RunPhase.PostEncounter:
                return "Post Encounter";
            case RunPhase.Result:
                return "Result";
            case RunPhase.Completed:
                return "Completed";
            case RunPhase.Failed:
                return "Failed";
            case RunPhase.Retreated:
                return "Retreated";
            default:
                return "None";
        }
    }

    private static string ToDisplayText(DungeonRunStatus status)
    {
        switch (status)
        {
            case DungeonRunStatus.Active:
                return "Active";
            case DungeonRunStatus.AwaitingChoice:
                return "Awaiting Choice";
            case DungeonRunStatus.InBattle:
                return "In Battle";
            case DungeonRunStatus.Resolved:
                return "Resolved";
            case DungeonRunStatus.Failed:
                return "Failed";
            case DungeonRunStatus.Completed:
                return "Completed";
            default:
                return "Inactive";
        }
    }

    private static string FirstMeaningfulText(string value1, string value2, string fallback)
    {
        return FirstMeaningfulText(value1, value2, string.Empty, string.Empty, fallback);
    }

    private static string FirstMeaningfulText(string value1, string value2, string value3, string value4, string fallback)
    {
        if (HasText(value1))
        {
            return value1;
        }

        if (HasText(value2))
        {
            return value2;
        }

        if (HasText(value3))
        {
            return value3;
        }

        if (HasText(value4))
        {
            return value4;
        }

        return fallback;
    }

    private static string DefaultText(string value, string fallback)
    {
        return HasText(value) ? value : fallback;
    }

    private static bool HasText(string value)
    {
        return !string.IsNullOrEmpty(value) && value != "None";
    }
}

public static class DungeonRunCoordinator
{
    public static ExpeditionRunState CreateState(ExpeditionRunStateSeed seed)
    {
        return DungeonRunStateFactory.Create(seed);
    }

    public static DungeonRunReadModel BuildReadModel(ExpeditionRunState state)
    {
        return DungeonRunStateFactory.BuildReadModel(state);
    }
}
