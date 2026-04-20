using System;
using UnityEngine;

[CreateAssetMenu(menuName = "Project AAA/UI/Battle UI Preview Data", fileName = "BattleUiPreview")]
public sealed class BattleUiPreviewData : ScriptableObject
{
    [Serializable]
    public sealed class TimelineSlot
    {
        public string Label = "Alden";
        public string SecondaryLabel = "Anchor";
        public string StatusLabel = "Queued";
        public bool IsCurrent;
        public bool IsEnemy;
    }

    [Header("Top Strip")]
    public string DungeonName = "Dungeon Alpha";
    public string RouteLabel = "Standard Path";
    public string ChoiceProgressText = "Choice 1 / 6";
    public string PhaseLabel = "Player Turn";
    public string TurnHintText = "Choose command -> choose target";

    [Header("Current Unit")]
    public string CurrentActorName = "Alden";
    public string CurrentActorRole = "Anchor";
    public int CurrentActorLevel = 4;
    public int CurrentHp = 28;
    public int CurrentMaxHp = 32;
    public int Attack = 14;
    public int Defense = 9;
    public int Speed = 6;
    public string GearSummary = "Guild shield / iron blade";
    public string ActorFooterText = "Acting | Front Row";

    [Header("Command Preview")]
    public string[] ActionLabels = { "Attack [1]", "Skill [2]", "Item [3]", "Move [4]", "End Turn [5]" };
    public int SelectedActionIndex;
    public string CommandHintText = "Select action to preview layout slots.";
    public string ExpectedDamageText = "Expected damage: 9-12";
    public string EnemyIntentText = "Enemy intent: Heavy strike toward Front Row.";

    [Header("Target")]
    public string TargetName = "Slime A";
    public string TargetRole = "Bulwark";
    public int TargetHp = 18;
    public int TargetMaxHp = 18;
    public string TargetSummaryText = "Stable target with front-loaded retaliation.";

    [Header("Popover / Spoils")]
    public string ResultPopoverTitle = "Encounter Cleared";
    [TextArea(3, 8)]
    public string ResultPopoverBody = "Result: Clean victory.\nRewards: mana shard x3.\nGrowth: Alden learned to hold lane pressure.\nNext: Continue toward the elite room.";
    public string RunSpoilsLine = "Run Spoils +3 mana shards pending extraction";

    [Header("Timeline")]
    public TimelineSlot[] Timeline =
    {
        new TimelineSlot { Label = "Alden", SecondaryLabel = "Anchor", StatusLabel = "Current", IsCurrent = true },
        new TimelineSlot { Label = "Mira", SecondaryLabel = "Rogue", StatusLabel = "Queued" },
        new TimelineSlot { Label = "Rune", SecondaryLabel = "Mage", StatusLabel = "Queued" },
        new TimelineSlot { Label = "Lia", SecondaryLabel = "Cleric", StatusLabel = "Queued" },
        new TimelineSlot { Label = "Slime A", SecondaryLabel = "Bulwark", StatusLabel = "Queued", IsEnemy = true }
    };
}
