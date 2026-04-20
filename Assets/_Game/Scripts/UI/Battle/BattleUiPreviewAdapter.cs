using System;

public static class BattleUiPreviewAdapter
{
    public static BattleUiPresenterModel BuildPreviewModel(BattleUiPreviewData previewData)
    {
        BattleUiPreviewData safe = previewData;
        BattleUiPresenterModel model = new BattleUiPresenterModel();

        if (safe == null)
        {
            return model;
        }

        model.DungeonName = SafeText(safe.DungeonName, "Dungeon Alpha");
        model.RouteLabel = SafeText(safe.RouteLabel, "Standard Path");
        model.ChoiceProgressText = SafeText(safe.ChoiceProgressText, "Choice 1 / 6");
        model.PhaseLabel = SafeText(safe.PhaseLabel, "Player Turn");
        model.TurnHintText = SafeText(safe.TurnHintText, "Choose command -> choose target");
        model.CurrentActorName = SafeText(safe.CurrentActorName, "Alden");
        model.CurrentActorRole = SafeText(safe.CurrentActorRole, "Anchor");
        model.CurrentActorLevel = safe.CurrentActorLevel;
        model.CurrentHp = Math.Max(1, safe.CurrentHp);
        model.CurrentMaxHp = Math.Max(model.CurrentHp, safe.CurrentMaxHp);
        model.Attack = safe.Attack;
        model.Defense = safe.Defense;
        model.Speed = safe.Speed;
        model.GearSummary = SafeText(safe.GearSummary, "No gear");
        model.ActorFooterText = SafeText(safe.ActorFooterText, "Acting");
        model.ActionLabels = safe.ActionLabels ?? Array.Empty<string>();
        model.SelectedActionIndex = model.ActionLabels.Length <= 0 ? -1 : Math.Max(0, Math.Min(safe.SelectedActionIndex, model.ActionLabels.Length - 1));
        model.CommandHintText = SafeText(safe.CommandHintText, "Select action to preview layout slots.");
        model.ExpectedDamageText = SafeText(safe.ExpectedDamageText, "Expected damage: ?");
        model.EnemyIntentText = SafeText(safe.EnemyIntentText, "Enemy intent pending.");
        model.TargetName = SafeText(safe.TargetName, "Slime A");
        model.TargetRole = SafeText(safe.TargetRole, "Bulwark");
        model.TargetHp = Math.Max(1, safe.TargetHp);
        model.TargetMaxHp = Math.Max(model.TargetHp, safe.TargetMaxHp);
        model.TargetSummaryText = SafeText(safe.TargetSummaryText, "Target preview.");
        model.ResultPopoverTitle = SafeText(safe.ResultPopoverTitle, "Encounter Cleared");
        model.ResultPopoverBody = SafeText(safe.ResultPopoverBody, "Preview pending.");
        model.RunSpoilsLine = SafeText(safe.RunSpoilsLine, "Run Spoils pending extraction");
        model.Timeline = BuildTimeline(safe.Timeline);
        return model;
    }

    private static BattleUiPresenterModel.TimelineSlot[] BuildTimeline(BattleUiPreviewData.TimelineSlot[] source)
    {
        if (source == null || source.Length <= 0)
        {
            return Array.Empty<BattleUiPresenterModel.TimelineSlot>();
        }

        BattleUiPresenterModel.TimelineSlot[] result = new BattleUiPresenterModel.TimelineSlot[source.Length];
        for (int i = 0; i < source.Length; i++)
        {
            BattleUiPreviewData.TimelineSlot slot = source[i] ?? new BattleUiPreviewData.TimelineSlot();
            result[i] = new BattleUiPresenterModel.TimelineSlot
            {
                Label = SafeText(slot.Label, "Queued"),
                SecondaryLabel = SafeText(slot.SecondaryLabel, "Battle"),
                StatusLabel = SafeText(slot.StatusLabel, "Queued"),
                IsCurrent = slot.IsCurrent,
                IsEnemy = slot.IsEnemy
            };
        }

        return result;
    }

    private static string SafeText(string value, string fallback)
    {
        return string.IsNullOrWhiteSpace(value) ? fallback : value.Trim();
    }
}
