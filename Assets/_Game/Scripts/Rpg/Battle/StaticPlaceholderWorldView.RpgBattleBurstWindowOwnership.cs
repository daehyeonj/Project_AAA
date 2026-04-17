using UnityEngine;

public sealed partial class StaticPlaceholderWorldView
{
    private const int RpgOwnedBurstWindowFuturePartyActions = 2;
    private const int RpgOwnedBurstWindowAttackBonus = 1;
    private const int RpgOwnedBurstWindowMageSplashBonus = 2;
    private const int RpgOwnedBurstWindowSupportExtension = 1;

    private void RefreshRpgOwnedEnemyIntentPreviewState()
    {
        if (_dungeonRunState != DungeonRunState.Battle ||
            (_battleState != BattleState.PartyActionSelect && _battleState != BattleState.PartyTargetSelect))
        {
            return;
        }

        for (int i = 0; i < _activeMonsters.Count; i++)
        {
            DungeonMonsterRuntimeData monster = _activeMonsters[i];
            if (monster == null || monster.RuntimeState == null)
            {
                continue;
            }

            if (monster.IsDefeated || monster.CurrentHp <= 0)
            {
                monster.RuntimeState.ClearIntent();
                continue;
            }

            bool useSpecial = ShouldRpgOwnedUseEliteSpecialAttack(monster);
            int targetIndex = GetRpgOwnedMonsterTargetPartyMemberIndex(monster, useSpecial);
            if (targetIndex < 0)
            {
                monster.RuntimeState.ClearIntent();
                continue;
            }

            PrototypeEnemyIntentSnapshot snapshot = BuildRpgOwnedEnemyIntentSnapshot(monster, targetIndex, useSpecial);
            monster.RuntimeState.SetIntent(
                snapshot.IntentKey,
                snapshot.TargetPatternKey,
                snapshot.PreviewText,
                snapshot.PredictedValue,
                snapshot.TargetId,
                snapshot.RangeKey,
                snapshot.LaneRuleKey,
                snapshot.ThreatLaneKey,
                snapshot.ThreatLaneLabel,
                snapshot.RangeText,
                snapshot.PredictedReachabilityText,
                snapshot.TargetRuleText);
        }
    }

    private void AdvanceRpgOwnedBurstWindowsAfterPartyAction()
    {
        for (int i = 0; i < _activeMonsters.Count; i++)
        {
            if (_activeMonsters[i] != null)
            {
                _activeMonsters[i].RuntimeState?.AdvanceBurstWindowActionWindow();
            }
        }
    }

    private bool HasRpgOwnedBurstWindow(DungeonMonsterRuntimeData monster)
    {
        return monster != null && monster.RuntimeState != null && monster.RuntimeState.HasBurstWindow;
    }

    private bool HasAnyRpgOwnedBurstWindow()
    {
        for (int i = 0; i < _activeMonsters.Count; i++)
        {
            if (HasRpgOwnedBurstWindow(_activeMonsters[i]))
            {
                return true;
            }
        }

        return false;
    }

    private bool HasReadableRpgOwnedIntent(DungeonMonsterRuntimeData monster)
    {
        return monster != null &&
               monster.RuntimeState != null &&
               !string.IsNullOrEmpty(monster.RuntimeState.IntentKey) &&
               !string.IsNullOrEmpty(monster.RuntimeState.IntentLabel);
    }

    private int GetRpgOwnedBurstWindowBonusDamage(DungeonMonsterRuntimeData monster)
    {
        if (monster == null || monster.RuntimeState == null)
        {
            return 3;
        }

        int bonus = 3;
        if (monster.RuntimeState.IntentPredictedValue >= 6)
        {
            bonus += 1;
        }

        if (monster.IsElite || monster.RuntimeState.IntentTargetPatternKey == "all_living_allies")
        {
            bonus += 1;
        }

        return Mathf.Clamp(bonus, 3, 5);
    }

    private string BuildRpgOwnedBurstWindowSummaryText(DungeonMonsterRuntimeData monster, int bonusDamage, int remainingPartyActions)
    {
        if (monster == null)
        {
            return "Burst window active.";
        }

        string intentRead = HasReadableRpgOwnedIntent(monster)
            ? "Intent read"
            : "Setup active";
        return intentRead + " | Payoff +" + Mathf.Max(1, bonusDamage) + " | " + Mathf.Max(1, remainingPartyActions) + " ally action(s).";
    }

    private bool TryApplyRpgOwnedBurstWindowSetup(DungeonPartyMemberRuntimeData actor, PrototypeRpgSkillDefinition skillDefinition, DungeonMonsterRuntimeData targetMonster)
    {
        if (actor == null ||
            skillDefinition == null ||
            targetMonster == null ||
            targetMonster.IsDefeated ||
            targetMonster.RuntimeState == null ||
            skillDefinition.SkillId != "skill_power_strike" ||
            !HasReadableRpgOwnedIntent(targetMonster))
        {
            return false;
        }

        int bonusDamage = GetRpgOwnedBurstWindowBonusDamage(targetMonster);
        string summaryText = BuildRpgOwnedBurstWindowSummaryText(targetMonster, bonusDamage, RpgOwnedBurstWindowFuturePartyActions);
        targetMonster.RuntimeState.OpenBurstWindow(
            actor.RoleLabel,
            skillDefinition.DisplayName,
            bonusDamage,
            RpgOwnedBurstWindowFuturePartyActions,
            summaryText);

        RecordRpgOwnedBattleEvent(
            PrototypeBattleEventKeys.BurstWindowOpened,
            actor.MemberId,
            targetMonster.MonsterId,
            bonusDamage,
            actor.DisplayName + " opened a burst window on " + targetMonster.DisplayName + ".",
            actionKey: "skill",
            skillId: skillDefinition.SkillId,
            phaseKey: "resolution",
            actorName: actor.DisplayName,
            targetName: targetMonster.DisplayName,
            shortText: "Burst ready");
        AppendBattleLog(targetMonster.DisplayName + " is exposed. Next payoff gains +" + bonusDamage + ".");
        return true;
    }

    private int TryExtendRpgOwnedBurstWindowsFromSupport(DungeonPartyMemberRuntimeData actor, PrototypeRpgSkillDefinition skillDefinition)
    {
        if (actor == null || skillDefinition == null || skillDefinition.SkillId != "skill_radiant_hymn")
        {
            return 0;
        }

        int extendedCount = 0;
        for (int i = 0; i < _activeMonsters.Count; i++)
        {
            DungeonMonsterRuntimeData monster = _activeMonsters[i];
            if (!HasRpgOwnedBurstWindow(monster))
            {
                continue;
            }

            PrototypeRpgEnemyRuntimeState runtimeState = monster.RuntimeState;
            runtimeState.ExtendBurstWindow(
                RpgOwnedBurstWindowSupportExtension,
                BuildRpgOwnedBurstWindowSummaryText(
                    monster,
                    runtimeState.BurstWindowBonusDamage,
                    runtimeState.BurstWindowRemainingPartyActions + RpgOwnedBurstWindowSupportExtension));
            extendedCount += 1;
        }

        if (extendedCount > 0)
        {
            RecordRpgOwnedBattleEvent(
                PrototypeBattleEventKeys.BurstWindowExtended,
                actor.MemberId,
                string.Empty,
                extendedCount,
                actor.DisplayName + " stabilized " + extendedCount + " burst window(s).",
                actionKey: "skill",
                skillId: skillDefinition.SkillId,
                phaseKey: "resolution",
                actorName: actor.DisplayName,
                targetName: "Burst windows",
                shortText: "Burst extended");
            AppendBattleLog(actor.DisplayName + " stabilized " + extendedCount + " burst window(s).");
        }

        return extendedCount;
    }

    private int GetRpgOwnedAttackBurstBonus(DungeonMonsterRuntimeData targetMonster)
    {
        return HasRpgOwnedBurstWindow(targetMonster) ? RpgOwnedBurstWindowAttackBonus : 0;
    }

    private int GetRpgOwnedArcaneBurstBonus(DungeonMonsterRuntimeData targetMonster)
    {
        return HasRpgOwnedBurstWindow(targetMonster) ? RpgOwnedBurstWindowMageSplashBonus : 0;
    }

    private int ConsumeRpgOwnedBurstWindowPayoff(DungeonPartyMemberRuntimeData actor, PrototypeRpgSkillDefinition skillDefinition, DungeonMonsterRuntimeData targetMonster)
    {
        if (actor == null ||
            skillDefinition == null ||
            targetMonster == null ||
            targetMonster.RuntimeState == null ||
            skillDefinition.SkillId != "skill_weak_point" ||
            !targetMonster.RuntimeState.HasBurstWindow)
        {
            return 0;
        }

        int consumedBonus = targetMonster.RuntimeState.ConsumeBurstWindowBonus();
        if (consumedBonus <= 0)
        {
            return 0;
        }

        RecordRpgOwnedBattleEvent(
            PrototypeBattleEventKeys.BurstWindowConsumed,
            actor.MemberId,
            targetMonster.MonsterId,
            consumedBonus,
            actor.DisplayName + " cashed the burst window on " + targetMonster.DisplayName + ".",
            actionKey: "skill",
            skillId: skillDefinition.SkillId,
            phaseKey: "resolution",
            actorName: actor.DisplayName,
            targetName: targetMonster.DisplayName,
            shortText: "Burst payoff");
        AppendBattleLog(actor.DisplayName + " cashed the burst window on " + targetMonster.DisplayName + " for +" + consumedBonus + ".");
        return consumedBonus;
    }

    private string BuildRpgOwnedBurstWindowStateText(DungeonMonsterRuntimeData monster)
    {
        return HasRpgOwnedBurstWindow(monster) ? monster.RuntimeState.BurstWindowLabel : string.Empty;
    }

    private string BuildRpgOwnedBurstWindowTraitText(DungeonMonsterRuntimeData monster)
    {
        return HasRpgOwnedBurstWindow(monster) ? monster.RuntimeState.BurstWindowSummaryText : string.Empty;
    }

    private string BuildRpgOwnedBurstWindowActionReadback(
        string selectedActionKey,
        DungeonPartyMemberRuntimeData member,
        PrototypeRpgSkillDefinition skillDefinition,
        DungeonMonsterRuntimeData targetMonster)
    {
        if (member == null)
        {
            return string.Empty;
        }

        if (selectedActionKey == "attack")
        {
            return HasRpgOwnedBurstWindow(targetMonster)
                ? "Payoff ready: Attack gains +" + RpgOwnedBurstWindowAttackBonus + " on exposed targets."
                : "Setup step: basic attack is weaker than opening a burst window first.";
        }

        if (selectedActionKey != "skill" || skillDefinition == null)
        {
            return string.Empty;
        }

        switch (skillDefinition.SkillId)
        {
            case "skill_power_strike":
                return HasReadableRpgOwnedIntent(targetMonster)
                    ? "Setup opener: read the target intent, then open a burst window for the next payoff."
                    : "Setup opener: pick a living enemy with a readable intent to open the window.";
            case "skill_weak_point":
                return HasRpgOwnedBurstWindow(targetMonster)
                    ? "Payoff ready: Weak Point cashes the exposed target now."
                    : "Setup first: Weak Point spikes after Power Strike opens the window.";
            case "skill_arcane_burst":
                return HasAnyRpgOwnedBurstWindow()
                    ? "Payoff sweep: exposed enemies take +" + RpgOwnedBurstWindowMageSplashBonus + " and keep the window."
                    : "Setup first: Arcane Burst becomes stronger once exposed targets exist.";
            case "skill_radiant_hymn":
                return HasAnyRpgOwnedBurstWindow()
                    ? "Support bridge: Radiant Hymn heals and extends every active burst window by +1 ally action."
                    : "Support bridge: heal now, then use this to stabilize windows once setup is active.";
            default:
                return string.Empty;
        }
    }

    private string BuildRpgOwnedBattleAttackEffectText(DungeonPartyMemberRuntimeData currentMember, DungeonMonsterRuntimeData previewMonster, int basicAttackPower)
    {
        string baseText = "Expected damage " + Mathf.Max(1, basicAttackPower) + ".";
        string burstText = BuildRpgOwnedBurstWindowActionReadback("attack", currentMember, null, previewMonster);
        return string.IsNullOrEmpty(burstText) ? baseText : baseText + " " + burstText;
    }

    private string BuildRpgOwnedBattleSkillEffectText(
        DungeonPartyMemberRuntimeData currentMember,
        PrototypeRpgSkillDefinition skillDefinition,
        PrototypeBattleUiActionContextData actionContext,
        DungeonMonsterRuntimeData previewMonster)
    {
        string fallbackText = actionContext != null && !string.IsNullOrEmpty(actionContext.ResolvedEffectType)
            ? GetBattleUiEffectText(actionContext.ResolvedEffectType, actionContext.ResolvedPowerValue)
            : "Use the active skill.";
        string burstText = BuildRpgOwnedBurstWindowActionReadback("skill", currentMember, skillDefinition, previewMonster);
        return string.IsNullOrEmpty(burstText) ? fallbackText : fallbackText + " " + burstText;
    }

    private string BuildRpgOwnedBattleThreatSummary(
        string defaultThreatSummary,
        string selectedActionKey,
        DungeonPartyMemberRuntimeData member,
        PrototypeRpgSkillDefinition skillDefinition,
        DungeonMonsterRuntimeData targetMonster)
    {
        string burstText = BuildRpgOwnedBurstWindowActionReadback(selectedActionKey, member, skillDefinition, targetMonster);
        if (string.IsNullOrEmpty(defaultThreatSummary))
        {
            return burstText;
        }

        if (string.IsNullOrEmpty(burstText))
        {
            return defaultThreatSummary;
        }

        return burstText + " | " + defaultThreatSummary;
    }
}
