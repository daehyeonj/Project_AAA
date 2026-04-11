using UnityEngine;

public sealed class PrototypeRpgPartyMemberRuntimeState
{
    public string MemberId { get; }
    public string DisplayName { get; }
    public string RoleTag { get; }
    public string RoleLabel { get; }
    public string DefaultSkillId { get; }
    public string GrowthTrackId { get; }
    public string JobId { get; }
    public string EquipmentLoadoutId { get; }
    public string SkillLoadoutId { get; }
    public string ResolvedSkillName { get; }
    public string ResolvedSkillShortText { get; }
    public int MaxHp { get; }
    public int Attack { get; }
    public int Defense { get; }
    public int Speed { get; }
    public int SkillPower { get; }
    public string EquipmentSummaryText { get; }
    public string GearContributionSummaryText { get; }
    public string AppliedProgressionSummaryText { get; }
    public string CurrentRunSummaryText { get; }
    public string NextRunPreviewSummaryText { get; }
    public string LaneKey { get; private set; }
    public string LaneLabel { get; private set; }
    public string PositionRuleText { get; private set; }
    public string StanceKey { get; private set; }

    public int CurrentHp { get; private set; }
    public bool IsKnockedOut { get; private set; }

    public bool IsAvailable => !IsKnockedOut && CurrentHp > 0;

    public PrototypeRpgPartyMemberRuntimeState(
        string memberId,
        string displayName,
        string roleTag,
        string roleLabel,
        string defaultSkillId,
        int maxHp,
        int attack,
        int defense,
        int speed)
        : this(
            memberId,
            displayName,
            roleTag,
            roleLabel,
            defaultSkillId,
            string.Empty,
            string.Empty,
            string.Empty,
            string.Empty,
            string.Empty,
            string.Empty,
            maxHp,
            attack,
            defense,
            speed,
            Mathf.Max(1, attack + 1),
            "No gear",
            "No bonus",
            string.Empty,
            string.Empty,
            string.Empty)
    {
    }

    public PrototypeRpgPartyMemberRuntimeState(PrototypeRpgMemberRuntimeResolveSurface surface)
        : this(
            surface != null ? surface.MemberId : string.Empty,
            surface != null ? surface.DisplayName : "Adventurer",
            surface != null ? surface.RoleTag : "adventurer",
            surface != null ? surface.RoleLabel : "Adventurer",
            surface != null ? surface.DefaultSkillId : string.Empty,
            surface != null ? surface.GrowthTrackId : string.Empty,
            surface != null ? surface.JobId : string.Empty,
            surface != null ? surface.EquipmentLoadoutId : string.Empty,
            surface != null ? surface.SkillLoadoutId : string.Empty,
            surface != null ? surface.ResolvedSkillName : string.Empty,
            surface != null ? surface.ResolvedSkillShortText : string.Empty,
            surface != null ? surface.MaxHp : 1,
            surface != null ? surface.Attack : 1,
            surface != null ? surface.Defense : 0,
            surface != null ? surface.Speed : 0,
            surface != null ? surface.SkillPower : 1,
            surface != null ? surface.EquipmentSummaryText : "No gear",
            surface != null ? surface.GearContributionSummaryText : "No bonus",
            surface != null ? surface.AppliedProgressionSummaryText : string.Empty,
            surface != null ? surface.CurrentRunSummaryText : string.Empty,
            surface != null ? surface.NextRunPreviewSummaryText : string.Empty)
    {
    }

    public PrototypeRpgPartyMemberRuntimeState(
        string memberId,
        string displayName,
        string roleTag,
        string roleLabel,
        string defaultSkillId,
        string growthTrackId,
        string jobId,
        string equipmentLoadoutId,
        string skillLoadoutId,
        string resolvedSkillName,
        string resolvedSkillShortText,
        int maxHp,
        int attack,
        int defense,
        int speed,
        int skillPower,
        string equipmentSummaryText,
        string gearContributionSummaryText,
        string appliedProgressionSummaryText,
        string currentRunSummaryText,
        string nextRunPreviewSummaryText)
    {
        MemberId = string.IsNullOrWhiteSpace(memberId) ? string.Empty : memberId.Trim();
        DisplayName = string.IsNullOrWhiteSpace(displayName) ? "Adventurer" : displayName.Trim();
        RoleTag = string.IsNullOrWhiteSpace(roleTag) ? "adventurer" : roleTag.Trim();
        RoleLabel = string.IsNullOrWhiteSpace(roleLabel) ? "Adventurer" : roleLabel.Trim();
        DefaultSkillId = string.IsNullOrWhiteSpace(defaultSkillId) ? string.Empty : defaultSkillId.Trim();
        GrowthTrackId = string.IsNullOrWhiteSpace(growthTrackId) ? string.Empty : growthTrackId.Trim();
        JobId = string.IsNullOrWhiteSpace(jobId) ? string.Empty : jobId.Trim();
        EquipmentLoadoutId = string.IsNullOrWhiteSpace(equipmentLoadoutId) ? string.Empty : equipmentLoadoutId.Trim();
        SkillLoadoutId = string.IsNullOrWhiteSpace(skillLoadoutId) ? string.Empty : skillLoadoutId.Trim();
        ResolvedSkillName = string.IsNullOrWhiteSpace(resolvedSkillName) ? "Skill" : resolvedSkillName.Trim();
        ResolvedSkillShortText = string.IsNullOrWhiteSpace(resolvedSkillShortText) ? string.Empty : resolvedSkillShortText.Trim();
        MaxHp = maxHp > 0 ? maxHp : 1;
        Attack = attack > 0 ? attack : 1;
        Defense = defense >= 0 ? defense : 0;
        Speed = speed >= 0 ? speed : 0;
        SkillPower = skillPower > 0 ? skillPower : Mathf.Max(1, Attack + 1);
        EquipmentSummaryText = string.IsNullOrWhiteSpace(equipmentSummaryText) ? "No gear" : equipmentSummaryText.Trim();
        GearContributionSummaryText = string.IsNullOrWhiteSpace(gearContributionSummaryText) ? "No bonus" : gearContributionSummaryText.Trim();
        AppliedProgressionSummaryText = string.IsNullOrWhiteSpace(appliedProgressionSummaryText) ? "No applied progression." : appliedProgressionSummaryText.Trim();
        CurrentRunSummaryText = string.IsNullOrWhiteSpace(currentRunSummaryText) ? "No current-run summary." : currentRunSummaryText.Trim();
        NextRunPreviewSummaryText = string.IsNullOrWhiteSpace(nextRunPreviewSummaryText) ? "No next-run preview." : nextRunPreviewSummaryText.Trim();
        LaneKey = string.Empty;
        LaneLabel = string.Empty;
        PositionRuleText = string.Empty;
        StanceKey = string.Empty;
        ResetForRun();
    }

    public void SetBattleLaneContext(string laneKey, string laneLabel, string positionRuleText, string stanceKey = "")
    {
        LaneKey = string.IsNullOrWhiteSpace(laneKey) ? string.Empty : laneKey.Trim();
        LaneLabel = string.IsNullOrWhiteSpace(laneLabel) ? string.Empty : laneLabel.Trim();
        PositionRuleText = string.IsNullOrWhiteSpace(positionRuleText) ? string.Empty : positionRuleText.Trim();
        StanceKey = string.IsNullOrWhiteSpace(stanceKey) ? string.Empty : stanceKey.Trim();
    }

    public void ResetForRun()
    {
        CurrentHp = MaxHp;
        IsKnockedOut = false;
    }

    public void SetCurrentHp(int currentHp)
    {
        CurrentHp = Mathf.Clamp(currentHp, 0, MaxHp);
        IsKnockedOut = CurrentHp <= 0;
    }

    public void SetKnockedOut(bool isKnockedOut)
    {
        if (isKnockedOut)
        {
            CurrentHp = 0;
            IsKnockedOut = true;
            return;
        }

        IsKnockedOut = CurrentHp <= 0;
    }

    public int ApplyDamage(int damage)
    {
        int previousHp = CurrentHp;
        SetCurrentHp(CurrentHp - Mathf.Max(0, damage));
        return previousHp - CurrentHp;
    }

    public int RecoverHp(int recoverAmount)
    {
        if (IsKnockedOut || CurrentHp <= 0)
        {
            return 0;
        }

        int previousHp = CurrentHp;
        SetCurrentHp(CurrentHp + Mathf.Max(0, recoverAmount));
        return CurrentHp - previousHp;
    }
}

