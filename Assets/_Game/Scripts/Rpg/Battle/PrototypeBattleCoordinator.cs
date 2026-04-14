using System;

public static class PrototypeBattleStateFactory
{
    public static PrototypeBattleRequest CreateRequest(PrototypeBattleRequest source)
    {
        PrototypeBattleRequest copy = new PrototypeBattleRequest();
        if (source == null)
        {
            return copy;
        }

        copy.EncounterId = source.EncounterId ?? string.Empty;
        copy.EncounterName = DefaultText(source.EncounterName, "None");
        copy.DungeonId = source.DungeonId ?? string.Empty;
        copy.DungeonLabel = DefaultText(source.DungeonLabel, "None");
        copy.RouteId = source.RouteId ?? string.Empty;
        copy.RouteLabel = DefaultText(source.RouteLabel, "None");
        copy.RoomId = source.RoomId ?? string.Empty;
        copy.RoomLabel = DefaultText(source.RoomLabel, "None");
        copy.RoomTypeLabel = DefaultText(source.RoomTypeLabel, "None");
        copy.PartySummaryText = DefaultText(source.PartySummaryText, "None");
        copy.ObjectiveText = DefaultText(source.ObjectiveText, "None");
        copy.RiskContextText = DefaultText(source.RiskContextText, "None");
        copy.RewardPreviewText = DefaultText(source.RewardPreviewText, "None");
        copy.EncounterProfileId = source.EncounterProfileId ?? string.Empty;
        copy.EncounterProfileSourceText = DefaultText(source.EncounterProfileSourceText, "fallback:hardcoded");
        copy.EncounterContextText = DefaultText(source.EncounterContextText, "None");
        copy.BattleSetupId = source.BattleSetupId ?? string.Empty;
        copy.BattleSetupSourceText = DefaultText(source.BattleSetupSourceText, "fallback:hardcoded");
        copy.EnemyGroupText = DefaultText(source.EnemyGroupText, "None");
        copy.BattleSetupSummaryText = DefaultText(source.BattleSetupSummaryText, "None");
        copy.EliteTypeText = DefaultText(source.EliteTypeText, "None");
        copy.EliteRewardHintText = DefaultText(source.EliteRewardHintText, "None");
        copy.IsEliteEncounter = source.IsEliteEncounter;
        copy.RetreatAllowed = source.RetreatAllowed;
        copy.EnterTurnIndex = Math.Max(0, source.EnterTurnIndex);
        copy.PartyMembers = source.PartyMembers ?? Array.Empty<PrototypeBattleCombatantState>();
        copy.EnemyMembers = source.EnemyMembers ?? Array.Empty<PrototypeBattleCombatantState>();
        return copy;
    }

    public static PrototypeBattleRuntimeState CreateState(PrototypeBattleRuntimeState source)
    {
        PrototypeBattleRuntimeState copy = new PrototypeBattleRuntimeState();
        if (source == null)
        {
            return copy;
        }

        copy.IsBattleActive = source.IsBattleActive;
        copy.BattleStateKey = DefaultText(source.BattleStateKey, PrototypeBattleOutcomeKeys.None);
        copy.Phase = source.Phase;
        copy.PhaseLabel = DefaultText(source.PhaseLabel, "None");
        copy.BattleTurnIndex = Math.Max(0, source.BattleTurnIndex);
        copy.CurrentActorIndex = source.CurrentActorIndex;
        copy.SelectedPartyMemberIndex = source.SelectedPartyMemberIndex;
        copy.CurrentActorId = source.CurrentActorId ?? string.Empty;
        copy.FocusedEnemyId = source.FocusedEnemyId ?? string.Empty;
        copy.HoveredEnemyId = source.HoveredEnemyId ?? string.Empty;
        copy.IsTargetSelectionActive = source.IsTargetSelectionActive;
        copy.IsInputLocked = source.IsInputLocked;
        copy.CanAttack = source.CanAttack;
        copy.CanSkill = source.CanSkill;
        copy.CanRetreat = source.CanRetreat;
        copy.PromptText = DefaultText(source.PromptText, "None");
        copy.FeedbackText = DefaultText(source.FeedbackText, "None");
        copy.PartySummaryText = DefaultText(source.PartySummaryText, "None");
        copy.PartyHpSummaryText = DefaultText(source.PartyHpSummaryText, "None");
        copy.PartyConditionText = DefaultText(source.PartyConditionText, "None");
        copy.CurrentActor = source.CurrentActor ?? new PrototypeBattleCombatantState();
        copy.PartyMembers = source.PartyMembers ?? Array.Empty<PrototypeBattleCombatantState>();
        copy.EnemyMembers = source.EnemyMembers ?? Array.Empty<PrototypeBattleCombatantState>();
        copy.SelectedCommand = source.SelectedCommand ?? new PrototypeBattleCommand();
        copy.PendingEnemyIntent = source.PendingEnemyIntent ?? new PrototypeEnemyIntentSnapshot();
        copy.RecentEvents = source.RecentEvents ?? Array.Empty<PrototypeBattleEventRecord>();
        copy.RecentLogLines = source.RecentLogLines ?? Array.Empty<string>();
        copy.CurrentResultSnapshot = source.CurrentResultSnapshot ?? new PrototypeBattleResultSnapshot();
        return copy;
    }

    public static PrototypeBattleResolution CreateResolution(PrototypeBattleResolution source)
    {
        PrototypeBattleResolution copy = new PrototypeBattleResolution();
        if (source == null)
        {
            return copy;
        }

        copy.ResultType = source.ResultType;
        copy.OutcomeKey = DefaultText(source.OutcomeKey, PrototypeBattleOutcomeKeys.None);
        copy.EncounterId = source.EncounterId ?? string.Empty;
        copy.EncounterName = DefaultText(source.EncounterName, "None");
        copy.DungeonId = source.DungeonId ?? string.Empty;
        copy.DungeonLabel = DefaultText(source.DungeonLabel, "None");
        copy.RouteLabel = DefaultText(source.RouteLabel, "None");
        copy.SummaryText = DefaultText(source.SummaryText, "None");
        copy.PartyConditionText = DefaultText(source.PartyConditionText, "None");
        copy.PartyHpSummaryText = DefaultText(source.PartyHpSummaryText, "None");
        copy.LootSummaryText = DefaultText(source.LootSummaryText, "None");
        copy.EncounterCleared = source.EncounterCleared;
        copy.RunEnded = source.RunEnded;
        copy.EliteDefeated = source.EliteDefeated;
        copy.TurnsTaken = Math.Max(0, source.TurnsTaken);
        copy.SurvivingMemberCount = Math.Max(0, source.SurvivingMemberCount);
        copy.KnockedOutMemberCount = Math.Max(0, source.KnockedOutMemberCount);
        copy.DefeatedEnemyCount = Math.Max(0, source.DefeatedEnemyCount);
        copy.ReturnedLootAmount = Math.Max(0, source.ReturnedLootAmount);
        copy.TotalDamageDealt = Math.Max(0, source.TotalDamageDealt);
        copy.TotalDamageTaken = Math.Max(0, source.TotalDamageTaken);
        copy.TotalHealingDone = Math.Max(0, source.TotalHealingDone);
        copy.ResultTags = source.ResultTags ?? Array.Empty<string>();
        copy.PartyMembersAtEnd = source.PartyMembersAtEnd ?? Array.Empty<PrototypeBattleCombatantState>();
        copy.EnemyMembersAtEnd = source.EnemyMembersAtEnd ?? Array.Empty<PrototypeBattleCombatantState>();
        copy.RecentEvents = source.RecentEvents ?? Array.Empty<PrototypeBattleEventRecord>();
        copy.ResultSnapshot = source.ResultSnapshot ?? new PrototypeBattleResultSnapshot();
        return copy;
    }

    private static string DefaultText(string value, string fallback)
    {
        return string.IsNullOrEmpty(value) ? fallback : value;
    }
}

public static class PrototypeBattleCommandResolver
{
    public static PrototypeBattleCommand CreateCommand(
        PrototypeBattleCommandType commandType,
        string actorId,
        string actorName,
        int actorIndex,
        string skillId,
        string skillLabel,
        string targetKind,
        string effectType,
        int powerValue,
        bool requiresTarget,
        string summaryText)
    {
        PrototypeBattleCommand command = new PrototypeBattleCommand();
        command.CommandType = commandType;
        command.CommandKey = ToCommandKey(commandType);
        command.ActorId = actorId ?? string.Empty;
        command.ActorName = string.IsNullOrEmpty(actorName) ? "None" : actorName;
        command.ActorIndex = actorIndex;
        command.SkillId = skillId ?? string.Empty;
        command.SkillLabel = string.IsNullOrEmpty(skillLabel) ? "None" : skillLabel;
        command.TargetKind = targetKind ?? string.Empty;
        command.EffectType = effectType ?? string.Empty;
        command.PowerValue = Math.Max(0, powerValue);
        command.RequiresTarget = requiresTarget;
        command.SummaryText = string.IsNullOrEmpty(summaryText) ? command.CommandKey : summaryText;
        return command;
    }

    public static PrototypeBattleCommand BindTarget(PrototypeBattleCommand source, string targetId, string targetName, int targetIndex)
    {
        PrototypeBattleCommand copy = source ?? new PrototypeBattleCommand();
        copy.TargetId = targetId ?? string.Empty;
        copy.TargetName = string.IsNullOrEmpty(targetName) ? "None" : targetName;
        copy.TargetIndex = targetIndex;
        copy.IsTargetLocked = !string.IsNullOrEmpty(copy.TargetId);
        return copy;
    }

    private static string ToCommandKey(PrototypeBattleCommandType commandType)
    {
        switch (commandType)
        {
            case PrototypeBattleCommandType.Attack:
                return "attack";
            case PrototypeBattleCommandType.Skill:
                return "skill";
            case PrototypeBattleCommandType.Retreat:
                return "retreat";
            default:
                return string.Empty;
        }
    }
}

public static class PrototypeBattleCoordinator
{
    public static PrototypeBattleViewModel BuildViewModel(
        PrototypeBattleRequest request,
        PrototypeBattleRuntimeState state,
        PrototypeBattleResolution resolution,
        PrototypeBattleUiSurfaceData surface)
    {
        PrototypeBattleRequest safeRequest = PrototypeBattleStateFactory.CreateRequest(request);
        PrototypeBattleRuntimeState safeState = PrototypeBattleStateFactory.CreateState(state);
        PrototypeBattleResolution safeResolution = PrototypeBattleStateFactory.CreateResolution(resolution);

        PrototypeBattleViewModel model = new PrototypeBattleViewModel();
        model.Request = safeRequest;
        model.State = safeState;
        model.Resolution = safeResolution;
        model.EncounterTitle = HasText(safeRequest.EncounterName) ? safeRequest.EncounterName : "None";
        model.PhaseText = HasText(safeState.PhaseLabel) ? safeState.PhaseLabel : "None";
        model.TurnText = safeState.BattleTurnIndex > 0 ? "Turn " + safeState.BattleTurnIndex : "Turn 0";
        model.CurrentActorText = HasText(safeState.CurrentActor != null ? safeState.CurrentActor.DisplayName : string.Empty)
            ? safeState.CurrentActor.DisplayName
            : "None";
        model.ResultText = HasText(safeResolution.SummaryText) ? safeResolution.SummaryText : "None";
        model.RecentLogLines = safeState.RecentLogLines ?? Array.Empty<string>();
        model.HudSurface = surface ?? new PrototypeBattleUiSurfaceData();
        return model;
    }

    private static bool HasText(string value)
    {
        return !string.IsNullOrEmpty(value) && value != "None";
    }
}
