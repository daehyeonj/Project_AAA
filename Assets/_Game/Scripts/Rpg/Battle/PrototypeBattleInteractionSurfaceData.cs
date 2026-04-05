using System;

public sealed class PrototypeBattleInteractionSurfaceData
{
    public bool HasInteractionSurface;
    public string BattlePhaseKey = "none";
    public string CurrentActorId = string.Empty;
    public string CurrentActorLabel = "None";
    public string CurrentActorRoleLabel = string.Empty;
    public string SelectedActionKey = string.Empty;
    public string SelectedActionLabel = "Action";
    public string MenuModeKey = "inactive";
    public PrototypeBattleCommandEntryData[] CommandEntries = Array.Empty<PrototypeBattleCommandEntryData>();
    public PrototypeBattleTargetCandidateData[] TargetCandidates = Array.Empty<PrototypeBattleTargetCandidateData>();
    public PrototypeBattleInputHintData InputHints = new PrototypeBattleInputHintData();
    public PrototypeBattleConfirmContextData ConfirmContext = new PrototypeBattleConfirmContextData();
}

public sealed class PrototypeBattleCommandEntryData
{
    public string ActionKey = string.Empty;
    public string DisplayLabel = "Action";
    public bool IsAvailable;
    public bool IsHovered;
    public bool IsSelected;
    public string AvailabilityReasonKey = string.Empty;
    public string HintText = string.Empty;
    public string KeyboardShortcutHint = string.Empty;
}

public sealed class PrototypeBattleTargetCandidateData
{
    public string TargetId = string.Empty;
    public string DisplayLabel = "None";
    public string RoleLabel = string.Empty;
    public string TypeLabel = string.Empty;
    public int CurrentHp;
    public int MaxHp = 1;
    public bool IsAvailable;
    public bool IsHovered;
    public bool IsSelected;
    public bool IsDefeated;
    public int TargetIndex = -1;
}

public sealed class PrototypeBattleInputHintData
{
    public string ActionHint = string.Empty;
    public string TargetHint = string.Empty;
    public string CancelHint = string.Empty;
}

public sealed class PrototypeBattleConfirmContextData
{
    public bool ConfirmRequired;
    public string PendingActionKey = string.Empty;
    public string PendingActionLabel = string.Empty;
    public string ConfirmLabel = "Confirm";
    public string CancelLabel = "Cancel";
}