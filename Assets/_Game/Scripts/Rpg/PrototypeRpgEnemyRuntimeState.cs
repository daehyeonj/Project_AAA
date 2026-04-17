using UnityEngine;

public sealed class PrototypeRpgEnemyRuntimeState
{
    public string EnemyId { get; }
    public string EncounterId { get; }
    public string DisplayName { get; }
    public string TypeLabel { get; }
    public int MaxHp { get; }
    public int AttackPower { get; }
    public bool IsElite { get; }

    public int CurrentHp { get; private set; }
    public bool IsDefeated { get; private set; }
    public string LaneKey { get; private set; }
    public string LaneLabel { get; private set; }
    public string PositionRuleText { get; private set; }
    public string StanceKey { get; private set; }
    public string IntentLabel { get; private set; }
    public string IntentKey { get; private set; }
    public string IntentTargetPatternKey { get; private set; }
    public string IntentTargetId { get; private set; }
    public int IntentPredictedValue { get; private set; }
    public string IntentRangeKey { get; private set; }
    public string IntentLaneRuleKey { get; private set; }
    public string IntentThreatLaneKey { get; private set; }
    public string IntentThreatLaneLabel { get; private set; }
    public string IntentRangeText { get; private set; }
    public string IntentPredictedReachabilityText { get; private set; }
    public string IntentTargetRuleText { get; private set; }
    public string BurstWindowStateKey { get; private set; }
    public string BurstWindowLabel { get; private set; }
    public string BurstWindowSummaryText { get; private set; }
    public string BurstWindowSourceRoleLabel { get; private set; }
    public string BurstWindowSourceSkillLabel { get; private set; }
    public int BurstWindowBonusDamage { get; private set; }
    public int BurstWindowRemainingPartyActions { get; private set; }
    public bool HasBurstWindow => BurstWindowRemainingPartyActions > 0 && !IsDefeated;
    private bool BurstWindowSkipDecayOnce { get; set; }

    public PrototypeRpgEnemyRuntimeState(
        string enemyId,
        string encounterId,
        string displayName,
        string typeLabel,
        int maxHp,
        int attackPower,
        bool isElite)
    {
        EnemyId = string.IsNullOrWhiteSpace(enemyId) ? string.Empty : enemyId.Trim();
        EncounterId = string.IsNullOrWhiteSpace(encounterId) ? string.Empty : encounterId.Trim();
        DisplayName = string.IsNullOrWhiteSpace(displayName) ? "Monster" : displayName.Trim();
        TypeLabel = string.IsNullOrWhiteSpace(typeLabel) ? "Monster" : typeLabel.Trim();
        MaxHp = maxHp > 0 ? maxHp : 1;
        AttackPower = attackPower > 0 ? attackPower : 1;
        IsElite = isElite;
        LaneKey = string.Empty;
        LaneLabel = string.Empty;
        PositionRuleText = string.Empty;
        StanceKey = string.Empty;
        ResetForEncounter();
    }

    public void SetBattleLaneContext(string laneKey, string laneLabel, string positionRuleText, string stanceKey = "")
    {
        LaneKey = string.IsNullOrWhiteSpace(laneKey) ? string.Empty : laneKey.Trim();
        LaneLabel = string.IsNullOrWhiteSpace(laneLabel) ? string.Empty : laneLabel.Trim();
        PositionRuleText = string.IsNullOrWhiteSpace(positionRuleText) ? string.Empty : positionRuleText.Trim();
        StanceKey = string.IsNullOrWhiteSpace(stanceKey) ? string.Empty : stanceKey.Trim();
    }

    public void ResetForEncounter()
    {
        CurrentHp = MaxHp;
        IsDefeated = false;
        ClearIntent();
        ClearBurstWindow();
    }

    public void SetCurrentHp(int currentHp)
    {
        CurrentHp = Mathf.Clamp(currentHp, 0, MaxHp);
        IsDefeated = CurrentHp <= 0;
        if (IsDefeated)
        {
            ClearBurstWindow();
        }
    }

    public void SetDefeated(bool isDefeated)
    {
        if (isDefeated)
        {
            CurrentHp = 0;
            IsDefeated = true;
            ClearBurstWindow();
            return;
        }

        IsDefeated = CurrentHp <= 0;
    }

    public int ApplyDamage(int damage)
    {
        int previousHp = CurrentHp;
        SetCurrentHp(CurrentHp - Mathf.Max(0, damage));
        return previousHp - CurrentHp;
    }

    public void ClearIntent()
    {
        IntentLabel = string.Empty;
        IntentKey = string.Empty;
        IntentTargetPatternKey = string.Empty;
        IntentTargetId = string.Empty;
        IntentPredictedValue = 0;
        IntentRangeKey = string.Empty;
        IntentLaneRuleKey = string.Empty;
        IntentThreatLaneKey = string.Empty;
        IntentThreatLaneLabel = string.Empty;
        IntentRangeText = string.Empty;
        IntentPredictedReachabilityText = string.Empty;
        IntentTargetRuleText = string.Empty;
    }

    public void SetIntent(string intentKey, string targetPatternKey, string previewText, int predictedValue, string targetId, string rangeKey = "", string laneRuleKey = "", string threatLaneKey = "", string threatLaneLabel = "", string rangeText = "", string predictedReachabilityText = "", string targetRuleText = "")
    {
        IntentKey = string.IsNullOrWhiteSpace(intentKey) ? string.Empty : intentKey.Trim();
        IntentTargetPatternKey = string.IsNullOrWhiteSpace(targetPatternKey) ? string.Empty : targetPatternKey.Trim();
        IntentLabel = string.IsNullOrWhiteSpace(previewText) ? string.Empty : previewText.Trim();
        IntentPredictedValue = Mathf.Max(0, predictedValue);
        IntentTargetId = string.IsNullOrWhiteSpace(targetId) ? string.Empty : targetId.Trim();
        IntentRangeKey = string.IsNullOrWhiteSpace(rangeKey) ? string.Empty : rangeKey.Trim();
        IntentLaneRuleKey = string.IsNullOrWhiteSpace(laneRuleKey) ? string.Empty : laneRuleKey.Trim();
        IntentThreatLaneKey = string.IsNullOrWhiteSpace(threatLaneKey) ? string.Empty : threatLaneKey.Trim();
        IntentThreatLaneLabel = string.IsNullOrWhiteSpace(threatLaneLabel) ? string.Empty : threatLaneLabel.Trim();
        IntentRangeText = string.IsNullOrWhiteSpace(rangeText) ? string.Empty : rangeText.Trim();
        IntentPredictedReachabilityText = string.IsNullOrWhiteSpace(predictedReachabilityText) ? string.Empty : predictedReachabilityText.Trim();
        IntentTargetRuleText = string.IsNullOrWhiteSpace(targetRuleText) ? string.Empty : targetRuleText.Trim();
    }

    public void SetIntentLabel(string intentLabel)
    {
        IntentLabel = string.IsNullOrWhiteSpace(intentLabel) ? string.Empty : intentLabel.Trim();
    }

    public void OpenBurstWindow(string sourceRoleLabel, string sourceSkillLabel, int bonusDamage, int futurePartyActions, string summaryText)
    {
        BurstWindowStateKey = "burst_ready";
        BurstWindowLabel = "Burst Ready";
        BurstWindowSourceRoleLabel = string.IsNullOrWhiteSpace(sourceRoleLabel) ? string.Empty : sourceRoleLabel.Trim();
        BurstWindowSourceSkillLabel = string.IsNullOrWhiteSpace(sourceSkillLabel) ? string.Empty : sourceSkillLabel.Trim();
        BurstWindowBonusDamage = Mathf.Max(1, bonusDamage);
        BurstWindowRemainingPartyActions = Mathf.Max(1, futurePartyActions);
        BurstWindowSummaryText = string.IsNullOrWhiteSpace(summaryText)
            ? "Payoff +" + BurstWindowBonusDamage + " for " + BurstWindowRemainingPartyActions + " ally action(s)."
            : summaryText.Trim();
        BurstWindowSkipDecayOnce = true;
    }

    public void ExtendBurstWindow(int additionalPartyActions, string summaryText)
    {
        if (!HasBurstWindow)
        {
            return;
        }

        BurstWindowRemainingPartyActions = Mathf.Max(1, BurstWindowRemainingPartyActions + Mathf.Max(1, additionalPartyActions));
        if (!string.IsNullOrWhiteSpace(summaryText))
        {
            BurstWindowSummaryText = summaryText.Trim();
        }

        BurstWindowSkipDecayOnce = true;
    }

    public int ConsumeBurstWindowBonus()
    {
        if (!HasBurstWindow)
        {
            return 0;
        }

        int bonus = Mathf.Max(0, BurstWindowBonusDamage);
        ClearBurstWindow();
        return bonus;
    }

    public void AdvanceBurstWindowActionWindow()
    {
        if (!HasBurstWindow)
        {
            return;
        }

        if (BurstWindowSkipDecayOnce)
        {
            BurstWindowSkipDecayOnce = false;
            return;
        }

        BurstWindowRemainingPartyActions = Mathf.Max(0, BurstWindowRemainingPartyActions - 1);
        if (BurstWindowRemainingPartyActions <= 0)
        {
            ClearBurstWindow();
        }
    }

    public void ClearBurstWindow()
    {
        BurstWindowStateKey = string.Empty;
        BurstWindowLabel = string.Empty;
        BurstWindowSummaryText = string.Empty;
        BurstWindowSourceRoleLabel = string.Empty;
        BurstWindowSourceSkillLabel = string.Empty;
        BurstWindowBonusDamage = 0;
        BurstWindowRemainingPartyActions = 0;
        BurstWindowSkipDecayOnce = false;
    }
}
