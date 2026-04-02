using UnityEngine;

public sealed class PrototypeRpgPartyMemberRuntimeState
{
    public string MemberId { get; }
    public string DisplayName { get; }
    public string RoleTag { get; }
    public string RoleLabel { get; }
    public string DefaultSkillId { get; }
    public int MaxHp { get; }
    public int Attack { get; }
    public int Defense { get; }
    public int Speed { get; }

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
    {
        MemberId = string.IsNullOrWhiteSpace(memberId) ? string.Empty : memberId.Trim();
        DisplayName = string.IsNullOrWhiteSpace(displayName) ? "Adventurer" : displayName.Trim();
        RoleTag = string.IsNullOrWhiteSpace(roleTag) ? "adventurer" : roleTag.Trim();
        RoleLabel = string.IsNullOrWhiteSpace(roleLabel) ? "Adventurer" : roleLabel.Trim();
        DefaultSkillId = string.IsNullOrWhiteSpace(defaultSkillId) ? string.Empty : defaultSkillId.Trim();
        MaxHp = maxHp > 0 ? maxHp : 1;
        Attack = attack > 0 ? attack : 1;
        Defense = defense >= 0 ? defense : 0;
        Speed = speed >= 0 ? speed : 0;
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
}

