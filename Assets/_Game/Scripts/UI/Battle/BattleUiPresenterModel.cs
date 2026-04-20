using System;

[Serializable]
public sealed class BattleUiPresenterModel
{
    [Serializable]
    public sealed class TimelineSlot
    {
        public string Label = "None";
        public string SecondaryLabel = "None";
        public string StatusLabel = "Queued";
        public bool IsCurrent;
        public bool IsEnemy;
    }

    public string DungeonName = "None";
    public string RouteLabel = "None";
    public string ChoiceProgressText = "Choice 0 / 0";
    public string PhaseLabel = "Preview";
    public string TurnHintText = "Preview";

    public string CurrentActorName = "None";
    public string CurrentActorRole = "None";
    public int CurrentActorLevel;
    public int CurrentHp = 1;
    public int CurrentMaxHp = 1;
    public int Attack;
    public int Defense;
    public int Speed;
    public string GearSummary = "No gear";
    public string ActorFooterText = "Preview";

    public string[] ActionLabels = Array.Empty<string>();
    public int SelectedActionIndex;
    public string CommandHintText = "Preview pending.";
    public string ExpectedDamageText = "Expected damage: ?";
    public string EnemyIntentText = "Enemy intent pending.";

    public string TargetName = "None";
    public string TargetRole = "None";
    public int TargetHp = 1;
    public int TargetMaxHp = 1;
    public string TargetSummaryText = "No target.";

    public string ResultPopoverTitle = "Result";
    public string ResultPopoverBody = "Preview pending.";
    public string RunSpoilsLine = "No pending spoils.";
    public TimelineSlot[] Timeline = Array.Empty<TimelineSlot>();
}
