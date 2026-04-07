using System;
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
    public int MaxHp { get; }
    public int Attack { get; }
    public int Defense { get; }
    public int Speed { get; }
    public int Level { get; }
    public int CurrentExperience { get; }
    public int ExperienceToNextLevel { get; }
    public string DerivedStatSummaryText { get; }
    public string EquipmentSummaryText { get; }
    public string GearContributionSummaryText { get; }
    public string SkillLoadoutSummaryText { get; }
    public string ResolvedSkillSummaryText { get; }
    public string InventorySummaryText { get; }
    public string ConsumableSlotSummaryText { get; }
    public string ActiveSkillSlotSummaryText { get; private set; }
    public string SkillCooldownSummaryText { get; private set; }
    public string SkillResourceSummaryText { get; private set; }
    public string ActionEconomySummaryText { get; private set; }
    public string NextRunActiveSkillPreviewText { get; private set; }
    public int CurrentSkillResource { get; private set; }
    public int MaxSkillResource { get; private set; } = 1;
    public PrototypeRpgActiveSkillSlotState[] ActiveSkillSlots { get; private set; } = Array.Empty<PrototypeRpgActiveSkillSlotState>();

    public int CurrentHp { get; private set; }
    public bool IsKnockedOut { get; private set; }

    public bool IsAvailable => !IsKnockedOut && CurrentHp > 0;

    public PrototypeRpgPartyMemberRuntimeState(string memberId, string displayName, string roleTag, string roleLabel, string defaultSkillId, int maxHp, int attack, int defense, int speed, string growthTrackId, string jobId, string equipmentLoadoutId, string skillLoadoutId, int level, int currentExperience, int experienceToNextLevel, string derivedStatSummaryText, string equipmentSummaryText, string gearContributionSummaryText, string skillLoadoutSummaryText, string resolvedSkillSummaryText, string inventorySummaryText, string consumableSlotSummaryText, string activeSkillSlotSummaryText, string skillCooldownSummaryText, string skillResourceSummaryText, string actionEconomySummaryText, string nextRunActiveSkillPreviewText)
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
        MaxHp = maxHp > 0 ? maxHp : 1;
        Attack = attack > 0 ? attack : 1;
        Defense = defense >= 0 ? defense : 0;
        Speed = speed >= 0 ? speed : 0;
        Level = level > 0 ? level : 1;
        CurrentExperience = currentExperience >= 0 ? currentExperience : 0;
        ExperienceToNextLevel = experienceToNextLevel > 0 ? experienceToNextLevel : 1;
        DerivedStatSummaryText = string.IsNullOrWhiteSpace(derivedStatSummaryText) ? string.Empty : derivedStatSummaryText.Trim();
        EquipmentSummaryText = string.IsNullOrWhiteSpace(equipmentSummaryText) ? string.Empty : equipmentSummaryText.Trim();
        GearContributionSummaryText = string.IsNullOrWhiteSpace(gearContributionSummaryText) ? string.Empty : gearContributionSummaryText.Trim();
        SkillLoadoutSummaryText = string.IsNullOrWhiteSpace(skillLoadoutSummaryText) ? string.Empty : skillLoadoutSummaryText.Trim();
        ResolvedSkillSummaryText = string.IsNullOrWhiteSpace(resolvedSkillSummaryText) ? string.Empty : resolvedSkillSummaryText.Trim();
        InventorySummaryText = string.IsNullOrWhiteSpace(inventorySummaryText) ? string.Empty : inventorySummaryText.Trim();
        ConsumableSlotSummaryText = string.IsNullOrWhiteSpace(consumableSlotSummaryText) ? string.Empty : consumableSlotSummaryText.Trim();
        ActiveSkillSlotSummaryText = string.IsNullOrWhiteSpace(activeSkillSlotSummaryText) ? string.Empty : activeSkillSlotSummaryText.Trim();
        SkillCooldownSummaryText = string.IsNullOrWhiteSpace(skillCooldownSummaryText) ? string.Empty : skillCooldownSummaryText.Trim();
        SkillResourceSummaryText = string.IsNullOrWhiteSpace(skillResourceSummaryText) ? string.Empty : skillResourceSummaryText.Trim();
        ActionEconomySummaryText = string.IsNullOrWhiteSpace(actionEconomySummaryText) ? string.Empty : actionEconomySummaryText.Trim();
        NextRunActiveSkillPreviewText = string.IsNullOrWhiteSpace(nextRunActiveSkillPreviewText) ? string.Empty : nextRunActiveSkillPreviewText.Trim();
        ResetForRun();
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

    public void SetActiveSkillRuntimeState(PrototypeRpgActiveSkillSlotState[] activeSkillSlots, int currentSkillResource, int maxSkillResource)
    {
        ActiveSkillSlots = activeSkillSlots ?? Array.Empty<PrototypeRpgActiveSkillSlotState>();
        MaxSkillResource = maxSkillResource > 0 ? maxSkillResource : 1;
        CurrentSkillResource = Mathf.Clamp(currentSkillResource, 0, MaxSkillResource);
    }

    public void SetCurrentSkillResource(int currentSkillResource)
    {
        CurrentSkillResource = Mathf.Clamp(currentSkillResource, 0, Mathf.Max(1, MaxSkillResource));
    }

    public void SetActiveSkillSummaries(string activeSkillSlotSummaryText, string skillCooldownSummaryText, string skillResourceSummaryText, string actionEconomySummaryText, string nextRunActiveSkillPreviewText)
    {
        ActiveSkillSlotSummaryText = string.IsNullOrWhiteSpace(activeSkillSlotSummaryText) ? string.Empty : activeSkillSlotSummaryText.Trim();
        SkillCooldownSummaryText = string.IsNullOrWhiteSpace(skillCooldownSummaryText) ? string.Empty : skillCooldownSummaryText.Trim();
        SkillResourceSummaryText = string.IsNullOrWhiteSpace(skillResourceSummaryText) ? string.Empty : skillResourceSummaryText.Trim();
        ActionEconomySummaryText = string.IsNullOrWhiteSpace(actionEconomySummaryText) ? string.Empty : actionEconomySummaryText.Trim();
        NextRunActiveSkillPreviewText = string.IsNullOrWhiteSpace(nextRunActiveSkillPreviewText) ? string.Empty : nextRunActiveSkillPreviewText.Trim();
    }
}
