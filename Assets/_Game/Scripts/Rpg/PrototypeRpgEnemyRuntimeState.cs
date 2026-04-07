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
    public string IntentLabel { get; private set; }
    public string IntentKey { get; private set; }
    public string IntentTargetPatternKey { get; private set; }
    public string IntentTargetId { get; private set; }
    public int IntentPredictedValue { get; private set; }
    public string IntentSkillId { get; private set; }
    public string IntentSkillLabel { get; private set; }
    public string IntentEffectSummaryText { get; private set; }
    public string IntentStatusSummaryText { get; private set; }
    public string IntentActionEconomySummaryText { get; private set; }

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
        ResetForEncounter();
    }

    public void ResetForEncounter()
    {
        CurrentHp = MaxHp;
        IsDefeated = false;
        ClearIntent();
    }

    public void SetCurrentHp(int currentHp)
    {
        CurrentHp = Mathf.Clamp(currentHp, 0, MaxHp);
        IsDefeated = CurrentHp <= 0;
    }

    public void SetDefeated(bool isDefeated)
    {
        if (isDefeated)
        {
            CurrentHp = 0;
            IsDefeated = true;
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
        IntentSkillId = string.Empty;
        IntentSkillLabel = string.Empty;
        IntentEffectSummaryText = string.Empty;
        IntentStatusSummaryText = string.Empty;
        IntentActionEconomySummaryText = string.Empty;
    }

    public void SetIntent(string intentKey, string targetPatternKey, string previewText, int predictedValue, string targetId, string skillId = "", string skillLabel = "", string effectSummaryText = "", string statusSummaryText = "", string actionEconomySummaryText = "")
    {
        IntentKey = string.IsNullOrWhiteSpace(intentKey) ? string.Empty : intentKey.Trim();
        IntentTargetPatternKey = string.IsNullOrWhiteSpace(targetPatternKey) ? string.Empty : targetPatternKey.Trim();
        IntentLabel = string.IsNullOrWhiteSpace(previewText) ? string.Empty : previewText.Trim();
        IntentPredictedValue = Mathf.Max(0, predictedValue);
        IntentTargetId = string.IsNullOrWhiteSpace(targetId) ? string.Empty : targetId.Trim();
        IntentSkillId = string.IsNullOrWhiteSpace(skillId) ? string.Empty : skillId.Trim();
        IntentSkillLabel = string.IsNullOrWhiteSpace(skillLabel) ? string.Empty : skillLabel.Trim();
        IntentEffectSummaryText = string.IsNullOrWhiteSpace(effectSummaryText) ? string.Empty : effectSummaryText.Trim();
        IntentStatusSummaryText = string.IsNullOrWhiteSpace(statusSummaryText) ? string.Empty : statusSummaryText.Trim();
        IntentActionEconomySummaryText = string.IsNullOrWhiteSpace(actionEconomySummaryText) ? string.Empty : actionEconomySummaryText.Trim();
    }

    public void SetIntentLabel(string intentLabel)
    {
        IntentLabel = string.IsNullOrWhiteSpace(intentLabel) ? string.Empty : intentLabel.Trim();
    }
}
