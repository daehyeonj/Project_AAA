using System;

public static class PrototypeRpgStatusEffectKeys
{
    public const string GuardUp = "guard_up";
    public const string Weaken = "weaken";
    public const string Burn = "burn";
    public const string Regen = "regen";
    public const string Mark = "mark";
    public const string CleanseOnce = "cleanse_once";
}

public sealed class PrototypeRpgStatusEffectDefinition
{
    public string StatusId { get; }
    public string DisplayLabel { get; }
    public string ShortLabel { get; }
    public bool IsBuff { get; }
    public bool IsDebuff { get; }
    public bool TicksAtTurnStart { get; }
    public int DefaultDurationTurns { get; }
    public int DefaultPowerValue { get; }
    public int MaxStacks { get; }
    public string SummaryText { get; }

    public PrototypeRpgStatusEffectDefinition(
        string statusId,
        string displayLabel,
        string shortLabel,
        bool isBuff,
        bool isDebuff,
        bool ticksAtTurnStart,
        int defaultDurationTurns,
        int defaultPowerValue,
        int maxStacks,
        string summaryText)
    {
        StatusId = NormalizeKey(statusId);
        DisplayLabel = string.IsNullOrWhiteSpace(displayLabel) ? "Status" : displayLabel.Trim();
        ShortLabel = string.IsNullOrWhiteSpace(shortLabel) ? DisplayLabel : shortLabel.Trim();
        IsBuff = isBuff;
        IsDebuff = isDebuff;
        TicksAtTurnStart = ticksAtTurnStart;
        DefaultDurationTurns = defaultDurationTurns > 0 ? defaultDurationTurns : 1;
        DefaultPowerValue = defaultPowerValue > 0 ? defaultPowerValue : 1;
        MaxStacks = maxStacks > 0 ? maxStacks : 1;
        SummaryText = string.IsNullOrWhiteSpace(summaryText) ? string.Empty : summaryText.Trim();
    }

    private static string NormalizeKey(string value)
    {
        return string.IsNullOrWhiteSpace(value) ? string.Empty : value.Trim().ToLowerInvariant();
    }
}

public sealed class PrototypeRpgStatusRuntimeState
{
    public string StatusId = string.Empty;
    public string SourceEntityId = string.Empty;
    public string TargetEntityId = string.Empty;
    public int RemainingTurns;
    public int StackCount = 1;
    public int AppliedPowerValue = 1;
    public string DisplayLabel = string.Empty;
    public string ShortLabel = string.Empty;
    public string SummaryText = string.Empty;
    public bool IsBuff;
    public bool IsDebuff;
    public bool TicksAtTurnStart;
}

public static class PrototypeRpgStatusEffectCatalog
{
    private static readonly PrototypeRpgStatusEffectDefinition[] Definitions =
    {
        new PrototypeRpgStatusEffectDefinition(PrototypeRpgStatusEffectKeys.GuardUp, "Guard Up", "Guard", true, false, false, 1, 1, 2, "Incoming damage is softened for the next exchange."),
        new PrototypeRpgStatusEffectDefinition(PrototypeRpgStatusEffectKeys.Weaken, "Weaken", "Weak", false, true, false, 2, 1, 2, "Outgoing damage is dampened."),
        new PrototypeRpgStatusEffectDefinition(PrototypeRpgStatusEffectKeys.Burn, "Burn", "Burn", false, true, true, 2, 1, 3, "Takes damage at the start of its turn."),
        new PrototypeRpgStatusEffectDefinition(PrototypeRpgStatusEffectKeys.Regen, "Regen", "Regen", true, false, true, 2, 1, 2, "Recovers HP at the start of its turn."),
        new PrototypeRpgStatusEffectDefinition(PrototypeRpgStatusEffectKeys.Mark, "Mark", "Mark", false, true, false, 2, 1, 1, "Finisher and setup pressure hit harder."),
        new PrototypeRpgStatusEffectDefinition(PrototypeRpgStatusEffectKeys.CleanseOnce, "Cleanse Once", "Cleanse", true, false, false, 2, 1, 1, "Blocks the next incoming debuff.")
    };

    public static PrototypeRpgStatusEffectDefinition GetDefinition(string statusId)
    {
        string normalizedId = NormalizeKey(statusId);
        if (string.IsNullOrEmpty(normalizedId))
        {
            return null;
        }

        for (int i = 0; i < Definitions.Length; i++)
        {
            PrototypeRpgStatusEffectDefinition definition = Definitions[i];
            if (definition != null && definition.StatusId == normalizedId)
            {
                return definition;
            }
        }

        return null;
    }

    private static string NormalizeKey(string value)
    {
        return string.IsNullOrWhiteSpace(value) ? string.Empty : value.Trim().ToLowerInvariant();
    }
}
