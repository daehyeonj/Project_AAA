using System.Collections.Generic;
using UnityEngine;

public sealed partial class StaticPlaceholderWorldView
{
    private sealed class RpgOwnedBattleActionPreviewData
    {
        public string PreviewText = string.Empty;
        public string PostEffectText = string.Empty;
        public string FormulaText = string.Empty;
        public string GrowthText = string.Empty;
        public int ExpectedAmount;
        public int ExpectedTargetHpAfter;
        public bool WouldDefeatTarget;
        public bool IsBlocked;
    }

    private string GetRpgOwnedActivePartyId()
    {
        return _activeDungeonParty != null && !string.IsNullOrEmpty(_activeDungeonParty.PartyId)
            ? _activeDungeonParty.PartyId
            : string.Empty;
    }

    private int GetRpgOwnedMemberEquipmentAttackBonus(DungeonPartyMemberRuntimeData member)
    {
        string partyId = GetRpgOwnedActivePartyId();
        return _runtimeEconomyState != null && member != null && !string.IsNullOrEmpty(partyId)
            ? _runtimeEconomyState.GetPartyMemberEquipmentAttackBonus(partyId, member.MemberId)
            : 0;
    }

    private int GetRpgOwnedMemberEquipmentSkillPowerBonus(DungeonPartyMemberRuntimeData member)
    {
        string partyId = GetRpgOwnedActivePartyId();
        return _runtimeEconomyState != null && member != null && !string.IsNullOrEmpty(partyId)
            ? _runtimeEconomyState.GetPartyMemberEquipmentSkillPowerBonus(partyId, member.MemberId)
            : 0;
    }

    private string BuildRpgOwnedResolvedFormulaText(
        string statLabel,
        int resolvedValue,
        int baseValue,
        int growthBonus,
        int gearBonus,
        int situationalBonus = 0,
        string situationalLabel = "")
    {
        List<string> parts = new List<string>();
        parts.Add("Base " + Mathf.Max(0, baseValue));
        if (growthBonus > 0)
        {
            parts.Add("Growth " + growthBonus);
        }

        if (gearBonus > 0)
        {
            parts.Add("Gear " + gearBonus);
        }

        if (situationalBonus > 0)
        {
            string label = string.IsNullOrEmpty(situationalLabel) ? "Bonus" : situationalLabel;
            parts.Add(label + " " + situationalBonus);
        }

        return statLabel + " " + Mathf.Max(0, resolvedValue) + " = " + string.Join(" + ", parts.ToArray());
    }

    private string BuildRpgOwnedGrowthContributionText(DungeonPartyMemberRuntimeData member, string statLabel, int growthBonus, int gearBonus)
    {
        if (member == null)
        {
            return string.Empty;
        }

        List<string> parts = new List<string>();
        if (growthBonus > 0)
        {
            int level = member.RuntimeState != null ? Mathf.Max(1, member.RuntimeState.Level) : 1;
            parts.Add("Lv" + level + " +" + growthBonus + " " + statLabel);
        }

        if (gearBonus > 0)
        {
            string gearName = PrototypeRpgEquipmentCatalog.ExtractDisplayName(member.EquipmentSummaryText);
            if (string.IsNullOrEmpty(gearName))
            {
                gearName = "Gear";
            }

            parts.Add(gearName + " +" + gearBonus + " " + statLabel);
        }

        return parts.Count > 0 ? string.Join(" | ", parts.ToArray()) : string.Empty;
    }

    private void PopulateRpgOwnedTargetOutcomePreview(RpgOwnedBattleActionPreviewData preview, DungeonMonsterRuntimeData targetMonster, int expectedAmount)
    {
        if (preview == null || targetMonster == null)
        {
            return;
        }

        int currentHp = Mathf.Max(0, targetMonster.CurrentHp);
        preview.ExpectedTargetHpAfter = Mathf.Max(0, currentHp - Mathf.Max(0, expectedAmount));
        preview.WouldDefeatTarget = preview.ExpectedTargetHpAfter <= 0;
        preview.PostEffectText = preview.WouldDefeatTarget
            ? "Would defeat"
            : "HP " + currentHp + " -> " + preview.ExpectedTargetHpAfter;
    }

    private int PeekRpgOwnedFinisherBonus(DungeonPartyMemberRuntimeData member, PrototypeRpgSkillDefinition skillDefinition, DungeonMonsterRuntimeData targetMonster)
    {
        if (member == null ||
            skillDefinition == null ||
            targetMonster == null ||
            targetMonster.IsDefeated ||
            targetMonster.CurrentHp <= 0)
        {
            return 0;
        }

        if (skillDefinition.SkillId == "skill_weak_point")
        {
            if (HasRpgOwnedBurstWindow(targetMonster) && targetMonster.RuntimeState != null)
            {
                return Mathf.Max(0, targetMonster.RuntimeState.BurstWindowBonusDamage);
            }

            if (targetMonster.CurrentHp <= member.Attack)
            {
                return 1;
            }
        }

        return 0;
    }

    private RpgOwnedBattleActionPreviewData BuildRpgOwnedBattleActionPreview(
        BattleActionType action,
        DungeonPartyMemberRuntimeData member,
        DungeonMonsterRuntimeData targetMonster,
        PrototypeRpgSkillDefinition skillDefinition = null)
    {
        RpgOwnedBattleActionPreviewData preview = new RpgOwnedBattleActionPreviewData();
        if (member == null)
        {
            return preview;
        }

        if (action == BattleActionType.Attack)
        {
            if (targetMonster != null)
            {
                PrototypeBattleLaneRuleResolution laneResolution = BuildPartyActionLaneResolution(member, targetMonster, action, null);
                if (laneResolution.ReachabilityStateKey == "blocked")
                {
                    preview.IsBlocked = true;
                    preview.PreviewText = "Blocked: target out of reach";
                    preview.PostEffectText = laneResolution.ReachabilitySummaryText;
                    return preview;
                }
            }

            int gearBonus = GetRpgOwnedMemberEquipmentAttackBonus(member);
            int growthBonus = member.RuntimeState != null ? Mathf.Max(0, member.RuntimeState.GrowthBonusAttack) : 0;
            int baseValue = Mathf.Max(1, member.Attack - gearBonus - growthBonus);
            int burstBonus = targetMonster != null ? GetRpgOwnedAttackBurstBonus(targetMonster) : 0;
            int resolvedDamage = Mathf.Max(1, member.Attack + burstBonus);
            preview.ExpectedAmount = resolvedDamage;
            preview.PreviewText = targetMonster != null
                ? "Expected: " + resolvedDamage + " dmg to " + targetMonster.DisplayName
                : "Expected: " + resolvedDamage + " dmg";
            preview.FormulaText = BuildRpgOwnedResolvedFormulaText(
                "ATK",
                resolvedDamage,
                baseValue,
                growthBonus,
                gearBonus,
                burstBonus,
                burstBonus > 0 ? "Expose" : string.Empty);
            preview.GrowthText = BuildRpgOwnedGrowthContributionText(member, "ATK", growthBonus, gearBonus);
            PopulateRpgOwnedTargetOutcomePreview(preview, targetMonster, resolvedDamage);
            return preview;
        }

        if (action != BattleActionType.Skill)
        {
            return preview;
        }

        skillDefinition = skillDefinition ?? ResolveMemberSkillDefinition(member);
        string effectType = GetResolvedSkillEffectType(member, skillDefinition);
        string targetKind = GetResolvedSkillTargetKind(member, skillDefinition);
        int resolvedSkillPower = GetResolvedSkillPower(member, skillDefinition);
        int gearBonusValue = GetRpgOwnedMemberEquipmentSkillPowerBonus(member);
        int growthBonusValue = member.RuntimeState != null ? Mathf.Max(0, member.RuntimeState.GrowthBonusAttack) : 0;
        int baseSkillValue = skillDefinition != null && skillDefinition.PowerValue > 0
            ? skillDefinition.PowerValue
            : Mathf.Max(1, resolvedSkillPower - gearBonusValue - growthBonusValue);
        int situationalBonus = 0;
        string situationalLabel = string.Empty;

        if (targetKind == "single_enemy" && targetMonster != null)
        {
            PrototypeBattleLaneRuleResolution laneResolution = BuildPartyActionLaneResolution(member, targetMonster, action, skillDefinition);
            if (laneResolution.ReachabilityStateKey == "blocked")
            {
                preview.IsBlocked = true;
                preview.PreviewText = "Blocked: target out of reach";
                preview.PostEffectText = laneResolution.ReachabilitySummaryText;
                return preview;
            }
        }

        if (effectType == "finisher_damage")
        {
            situationalBonus = PeekRpgOwnedFinisherBonus(member, skillDefinition, targetMonster);
            situationalLabel = "Payoff";
        }
        else if (targetKind == "all_enemies" && effectType == "damage" && targetMonster != null)
        {
            situationalBonus = GetRpgOwnedArcaneBurstBonus(targetMonster);
            situationalLabel = "Expose";
        }

        int resolvedAmount = Mathf.Max(1, resolvedSkillPower + situationalBonus);
        preview.ExpectedAmount = resolvedAmount;
        preview.FormulaText = BuildRpgOwnedResolvedFormulaText(
            "Skill",
            targetKind == "all_enemies" ? resolvedSkillPower : resolvedAmount,
            baseSkillValue,
            growthBonusValue,
            gearBonusValue,
            targetKind == "all_enemies" ? 0 : situationalBonus,
            situationalLabel);
        preview.GrowthText = BuildRpgOwnedGrowthContributionText(member, "Skill", growthBonusValue, gearBonusValue);

        if (effectType == "heal")
        {
            preview.PreviewText = "Expected: party heal " + Mathf.Max(1, resolvedSkillPower) + " HP";
            preview.PostEffectText = "Supports all living allies.";
            return preview;
        }

        if (targetKind == "all_enemies")
        {
            preview.PreviewText = situationalBonus > 0
                ? "Expected: " + resolvedSkillPower + " dmg to all enemies (+" + situationalBonus + " vs exposed)"
                : "Expected: " + resolvedSkillPower + " dmg to all enemies";
            if (targetMonster != null)
            {
                PopulateRpgOwnedTargetOutcomePreview(preview, targetMonster, resolvedAmount);
            }
            else
            {
                preview.PostEffectText = "Uses the same resolved skill power on every enemy.";
            }
            return preview;
        }

        preview.PreviewText = targetMonster != null
            ? "Expected: " + resolvedAmount + " dmg to " + targetMonster.DisplayName
            : "Expected: " + resolvedAmount + " dmg";
        PopulateRpgOwnedTargetOutcomePreview(preview, targetMonster, resolvedAmount);
        return preview;
    }

    private string BuildRpgOwnedResolvedDamageLogText(
        DungeonPartyMemberRuntimeData member,
        string actionName,
        DungeonMonsterRuntimeData targetMonster,
        int appliedDamage,
        RpgOwnedBattleActionPreviewData preview)
    {
        string formulaSuffix = preview != null && !string.IsNullOrEmpty(preview.FormulaText)
            ? " (" + preview.FormulaText + ")."
            : ".";
        string targetName = targetMonster != null ? targetMonster.DisplayName : "target";
        if (string.Equals(actionName, "Attack", System.StringComparison.Ordinal))
        {
            return member.DisplayName + " attacked " + targetName + " for " + appliedDamage + " damage" + formulaSuffix;
        }

        return member.DisplayName + " used " + actionName + " on " + targetName + " for " + appliedDamage + " damage" + formulaSuffix;
    }

    private string BuildRpgOwnedAreaSkillDamageLogText(
        DungeonPartyMemberRuntimeData member,
        string actionName,
        int totalDamage,
        int exposedHitCount,
        RpgOwnedBattleActionPreviewData preview)
    {
        string detailText = preview != null && !string.IsNullOrEmpty(preview.FormulaText)
            ? preview.FormulaText
            : "Skill resolved";
        if (exposedHitCount > 0)
        {
            detailText += "; exposed +" + RpgOwnedBurstWindowMageSplashBonus + " on " + exposedHitCount + " target(s)";
        }

        return member.DisplayName + " used " + actionName + " for " + totalDamage + " total damage (" + detailText + ").";
    }

    private string BuildRpgOwnedPartyHealLogText(
        DungeonPartyMemberRuntimeData member,
        string actionName,
        int totalRecovered,
        RpgOwnedBattleActionPreviewData preview)
    {
        string detailText = preview != null && !string.IsNullOrEmpty(preview.FormulaText)
            ? preview.FormulaText
            : "Skill resolved";
        return member.DisplayName + " used " + actionName + " and restored " + totalRecovered + " HP (" + detailText + ").";
    }

    private bool TryResolveRpgOwnedBattleAction(BattleActionType action)
    {
        if (!IsRpgOwnedBattleActionAvailable(action))
        {
            return false;
        }

        DungeonPartyMemberRuntimeData member = GetCurrentActorMember();
        if (action != BattleActionType.Retreat && member == null)
        {
            return false;
        }

        PrototypeRpgSkillDefinition resolvedSkillDefinition = action == BattleActionType.Skill ? ResolveCurrentActorSkillDefinition() : null;
        string resolvedSkillId = action == BattleActionType.Skill && resolvedSkillDefinition != null ? resolvedSkillDefinition.SkillId : member != null ? member.DefaultSkillId : string.Empty;
        string resolvedSkillName = action == BattleActionType.Skill ? GetResolvedSkillDisplayName(member, resolvedSkillDefinition) : string.Empty;
        int resolvedSkillPower = action == BattleActionType.Skill ? GetResolvedSkillPower(member, resolvedSkillDefinition) : 0;
        string resolvedSkillTargetKind = action == BattleActionType.Skill ? GetResolvedSkillTargetKind(member, resolvedSkillDefinition) : string.Empty;
        string resolvedSkillEffectType = action == BattleActionType.Skill ? GetResolvedSkillEffectType(member, resolvedSkillDefinition) : string.Empty;
        string actionLabel = GetBattleActionDisplayName(action, member);
        string actionKey = GetBattleActionKey(action);

        RecordRpgOwnedBattleEvent(
            PrototypeBattleEventKeys.ActionSelected,
            member != null ? member.MemberId : string.Empty,
            string.Empty,
            0,
            (member != null ? member.DisplayName : ActiveDungeonPartyText) + " selected " + actionLabel + ".",
            actionKey: actionKey,
            skillId: action == BattleActionType.Skill ? resolvedSkillId : string.Empty,
            phaseKey: "party_turn",
            actorName: member != null ? member.DisplayName : ActiveDungeonPartyText,
            shortText: actionLabel);

        if (action == BattleActionType.Retreat)
        {
            RecordRpgOwnedBattleEvent(
                PrototypeBattleEventKeys.RetreatConfirmed,
                member != null ? member.MemberId : string.Empty,
                _currentDungeonId,
                0,
                ActiveDungeonPartyText + " confirmed retreat.",
                actionKey: "retreat",
                phaseKey: "retreat",
                actorName: member != null ? member.DisplayName : ActiveDungeonPartyText,
                targetName: _currentDungeonName,
                shortText: "Retreat confirmed");
            AppendBattleLog("The party retreats from battle and abandons the run.");
            FinishDungeonRun(RunResultState.Retreat, BattleState.Retreat, false, 0, ActiveDungeonPartyText + " retreated from " + _currentDungeonName + " with no loot.");
            _pendingDungeonExit = true;
            return true;
        }

        if (action == BattleActionType.Move)
        {
            ClearBattleHoverState();
            _hoverBattleAction = action;
            _queuedBattleAction = action;
            SetBattleFeedbackText(BuildRpgOwnedBattleMoveSelectionFeedback(member));
            RefreshSelectionPrompt();
            RefreshDungeonPresentation();
            return true;
        }

        if (action == BattleActionType.EndTurn)
        {
            ClearBattleHoverState();
            _hoverBattleAction = action;
            _queuedBattleAction = action;
            LockBattleInput();
            AppendBattleLog(member.DisplayName + " holds position and passes the turn.");
            SetBattleFeedbackText(member.DisplayName + " ended the turn.");
            AdvanceRpgOwnedBattleAfterPartyAction();
            return true;
        }

        ClearBattleHoverState();
        _hoverBattleAction = action;
        _queuedBattleAction = action;
        LockBattleInput();

        if (action == BattleActionType.Attack || (action == BattleActionType.Skill && DoesRpgOwnedSkillRequireTarget(member)))
        {
            _battleState = BattleState.PartyTargetSelect;
            SetActiveBattleMonster(GetFirstLivingBattleMonster());
            ClearBattleInputLock();
            SetBattleFeedbackText(actionLabel + " selected.");
            RefreshSelectionPrompt();
            RefreshDungeonPresentation();
            return true;
        }

        if (resolvedSkillTargetKind == "all_enemies" && resolvedSkillEffectType == "damage")
        {
            int hitCount = 0;
            int totalDamage = 0;
            int exposedHitCount = 0;
            RpgOwnedBattleActionPreviewData preview = BuildRpgOwnedBattleActionPreview(action, member, null, resolvedSkillDefinition);
            var targetMonsters = GetTargetableBattleMonsters();
            for (int i = 0; i < targetMonsters.Count; i++)
            {
                DungeonMonsterRuntimeData targetMonster = targetMonsters[i];
                int burstBonus = GetRpgOwnedArcaneBurstBonus(targetMonster);
                int applied = ApplyRpgOwnedBattleDamageToMonster(member, targetMonster, resolvedSkillPower + burstBonus, new Color(0.46f, 0.75f, 1f, 1f));
                if (applied > 0)
                {
                    totalDamage += applied;
                    hitCount += 1;
                    if (burstBonus > 0)
                    {
                        exposedHitCount += 1;
                    }
                }
            }

            AppendBattleLog(BuildRpgOwnedAreaSkillDamageLogText(member, resolvedSkillName, totalDamage, exposedHitCount, preview));
            SetBattleFeedbackText(
                exposedHitCount > 0
                    ? resolvedSkillName + " hit " + hitCount + " enemies and spiked " + exposedHitCount + " exposed target(s)."
                    : resolvedSkillName + " hit " + hitCount + " enemies.");
            RecordRpgOwnedBattleEvent(
                PrototypeBattleEventKeys.SkillResolved,
                member.MemberId,
                string.Empty,
                totalDamage,
                member.DisplayName + " resolved " + resolvedSkillName + " on all enemies.",
                actionKey: "skill",
                skillId: resolvedSkillId,
                phaseKey: "resolution",
                actorName: member.DisplayName,
                targetName: "All enemies",
                shortText: resolvedSkillName);
        }
        else if (resolvedSkillTargetKind == "all_allies" && resolvedSkillEffectType == "heal")
        {
            int totalRecovered = 0;
            RpgOwnedBattleActionPreviewData preview = BuildRpgOwnedBattleActionPreview(action, member, null, resolvedSkillDefinition);
            var livingAllyIndices = GetLivingAllies();
            for (int i = 0; i < livingAllyIndices.Count; i++)
            {
                int memberIndex = livingAllyIndices[i];
                DungeonPartyMemberRuntimeData ally = GetPartyMemberAtIndex(memberIndex);
                totalRecovered += ApplyRpgOwnedBattleHealToPartyMember(member, ally, memberIndex, resolvedSkillPower, new Color(0.56f, 1f, 0.68f, 1f));
            }

            int extendedWindows = TryExtendRpgOwnedBurstWindowsFromSupport(member, resolvedSkillDefinition);
            AppendBattleLog(BuildRpgOwnedPartyHealLogText(member, resolvedSkillName, totalRecovered, preview));
            SetBattleFeedbackText(
                extendedWindows > 0
                    ? resolvedSkillName + " restored " + totalRecovered + " HP and stabilized " + extendedWindows + " window(s)."
                    : resolvedSkillName + " restored " + totalRecovered + " HP.");
            RecordRpgOwnedBattleEvent(
                PrototypeBattleEventKeys.SkillResolved,
                member.MemberId,
                string.Empty,
                totalRecovered,
                member.DisplayName + " resolved " + resolvedSkillName + " for the party.",
                actionKey: "skill",
                skillId: resolvedSkillId,
                phaseKey: "resolution",
                actorName: member.DisplayName,
                targetName: "All allies",
                shortText: resolvedSkillName);
        }
        else
        {
            AppendBattleLog(member.DisplayName + " used " + resolvedSkillName + ".");
            SetBattleFeedbackText(resolvedSkillName + " resolved.");
            RecordRpgOwnedBattleEvent(
                PrototypeBattleEventKeys.SkillResolved,
                member.MemberId,
                string.Empty,
                0,
                member.DisplayName + " resolved " + resolvedSkillName + ".",
                actionKey: "skill",
                skillId: resolvedSkillId,
                phaseKey: "resolution",
                actorName: member.DisplayName,
                shortText: resolvedSkillName);
        }

        AdvanceRpgOwnedBattleAfterPartyAction();
        return true;
    }

    private bool TryResolveRpgOwnedTargetSelection(DungeonMonsterRuntimeData targetMonster)
    {
        if (_dungeonRunState != DungeonRunState.Battle || _battleState != BattleState.PartyTargetSelect || IsBattleInputLocked() || targetMonster == null || targetMonster.IsDefeated || targetMonster.CurrentHp <= 0)
        {
            return false;
        }

        DungeonPartyMemberRuntimeData member = GetCurrentActorMember();
        if (member == null)
        {
            return false;
        }

        SetActiveBattleMonster(targetMonster);
        PrototypeRpgSkillDefinition resolvedSkillDefinition = _queuedBattleAction == BattleActionType.Skill ? ResolveMemberSkillDefinition(member) : null;
        string actionKey = _queuedBattleAction == BattleActionType.Skill ? "skill" : "attack";
        string resolvedSkillId = _queuedBattleAction == BattleActionType.Skill && resolvedSkillDefinition != null ? resolvedSkillDefinition.SkillId : string.Empty;
        PrototypeBattleLaneRuleResolution laneResolution = BuildPartyActionLaneResolution(member, targetMonster, _queuedBattleAction, resolvedSkillDefinition);
        if (laneResolution.ReachabilityStateKey == "blocked")
        {
            string rejectedSummary = member.DisplayName + " cannot reach " + targetMonster.DisplayName + ". " + laneResolution.ReachabilitySummaryText;
            RecordRpgOwnedBattleEvent(PrototypeBattleEventKeys.TargetRejected, member.MemberId, targetMonster.MonsterId, 0, rejectedSummary, actionKey: actionKey, skillId: resolvedSkillId, phaseKey: "target_select", actorName: member.DisplayName, targetName: targetMonster.DisplayName, shortText: "Target blocked");
            AppendBattleLog(rejectedSummary);
            SetBattleFeedbackText(rejectedSummary);
            return false;
        }

        RecordRpgOwnedBattleEvent(PrototypeBattleEventKeys.RangeRuleResolved, member.MemberId, targetMonster.MonsterId, 0, laneResolution.RangeText, actionKey: actionKey, skillId: resolvedSkillId, phaseKey: "target_select", actorName: member.DisplayName, targetName: targetMonster.DisplayName, shortText: laneResolution.RangeText);
        RecordRpgOwnedBattleEvent(PrototypeBattleEventKeys.LaneRuleResolved, member.MemberId, targetMonster.MonsterId, 0, laneResolution.ReachabilitySummaryText, actionKey: actionKey, skillId: resolvedSkillId, phaseKey: "target_select", actorName: member.DisplayName, targetName: targetMonster.DisplayName, shortText: laneResolution.TargetRuleText);

        RpgOwnedBattleActionPreviewData preview = BuildRpgOwnedBattleActionPreview(_queuedBattleAction, member, targetMonster, resolvedSkillDefinition);
        int damage = preview != null ? Mathf.Max(1, preview.ExpectedAmount) : 1;
        string actionName;
        string burstFeedbackText = string.Empty;
        if (_queuedBattleAction == BattleActionType.Skill)
        {
            string resolvedSkillEffectType = GetResolvedSkillEffectType(member, resolvedSkillDefinition);
            if (resolvedSkillEffectType == "finisher_damage")
            {
                int burstBonus = ConsumeRpgOwnedBurstWindowPayoff(member, resolvedSkillDefinition, targetMonster);
                if (burstBonus <= 0 && targetMonster.CurrentHp <= member.Attack)
                {
                    burstBonus = 1;
                }

                if (burstBonus > 0)
                {
                    burstFeedbackText = "Burst payoff +" + burstBonus + ".";
                }
            }

            actionName = GetResolvedSkillDisplayName(member, resolvedSkillDefinition);
        }
        else
        {
            int burstBonus = GetRpgOwnedAttackBurstBonus(targetMonster);
            if (burstBonus > 0)
            {
                burstFeedbackText = "Exposed target +" + burstBonus + ".";
            }

            actionName = "Attack";
        }

        RecordRpgOwnedBattleEvent(PrototypeBattleEventKeys.TargetSelected, member.MemberId, targetMonster.MonsterId, GetBattleMonsterDisplayIndex(targetMonster.MonsterId), member.DisplayName + " targeted " + targetMonster.DisplayName + ".", actionKey: actionKey, skillId: resolvedSkillId, phaseKey: "target_select", actorName: member.DisplayName, targetName: targetMonster.DisplayName, shortText: "Target locked");
        int appliedDamage = ApplyRpgOwnedBattleDamageToMonster(member, targetMonster, damage, new Color(1f, 0.48f, 0.30f, 1f));
        bool openedBurstWindow = TryApplyRpgOwnedBurstWindowSetup(member, resolvedSkillDefinition, targetMonster);
        AppendBattleLog(BuildRpgOwnedResolvedDamageLogText(member, actionName, targetMonster, appliedDamage, preview));
        if (openedBurstWindow)
        {
            burstFeedbackText = "Burst window opened.";
        }

        SetBattleFeedbackText(
            string.IsNullOrEmpty(burstFeedbackText)
                ? actionName + " dealt " + appliedDamage + " to " + targetMonster.DisplayName + "."
                : actionName + " dealt " + appliedDamage + " to " + targetMonster.DisplayName + ". " + burstFeedbackText);
        RecordRpgOwnedBattleEvent(_queuedBattleAction == BattleActionType.Skill ? PrototypeBattleEventKeys.SkillResolved : PrototypeBattleEventKeys.AttackResolved, member.MemberId, targetMonster.MonsterId, appliedDamage, member.DisplayName + " resolved " + actionName + " on " + targetMonster.DisplayName + ".", actionKey: actionKey, skillId: resolvedSkillId, phaseKey: "resolution", actorName: member.DisplayName, targetName: targetMonster.DisplayName, shortText: actionName);
        AdvanceRpgOwnedBattleAfterPartyAction();
        return true;
    }

    private int ApplyRpgOwnedBattleDamageToMonster(DungeonPartyMemberRuntimeData actor, DungeonMonsterRuntimeData monster, int damage, Color popupColor)
    {
        if (monster == null || monster.IsDefeated || monster.CurrentHp <= 0)
        {
            return 0;
        }

        bool wasDefeated = monster.IsDefeated;
        int safeDamage = Mathf.Max(1, damage);
        int applied = monster.RuntimeState != null ? monster.RuntimeState.ApplyDamage(safeDamage) : 0;
        if (monster.RuntimeState == null)
        {
            int previousHp = monster.CurrentHp;
            monster.CurrentHp = Mathf.Max(0, previousHp - safeDamage);
            applied = previousHp - monster.CurrentHp;
        }

        if (applied <= 0)
        {
            return 0;
        }

        _totalDamageDealt += applied;
        if (actor != null)
        {
            AddRunMemberContributionValue(_runMemberDamageDealt, actor.PartySlotIndex, applied);
        }

        FlashMonster(monster, popupColor);
        ShowBattlePopupForMonster(monster, "-" + applied, popupColor);
        string actionKey = actor != null ? GetBattleActionKey(_queuedBattleAction) : (_pendingEnemyUsedSpecialAttack ? "skill" : "attack");
        string skillId = actor != null && actionKey == "skill" ? actor.DefaultSkillId : string.Empty;
        RecordRpgOwnedBattleEvent(
            PrototypeBattleEventKeys.DamageApplied,
            actor != null ? actor.MemberId : string.Empty,
            monster.MonsterId,
            applied,
            monster.DisplayName + " took " + applied + " damage.",
            actionKey: actionKey,
            skillId: skillId,
            actorName: actor != null ? actor.DisplayName : string.Empty,
            targetName: monster.DisplayName,
            shortText: "-" + applied);
        ResolveMonsterDefeat(monster, wasDefeated);
        if (actor != null && !wasDefeated && monster.IsDefeated)
        {
            AddRunMemberContributionValue(_runMemberKillCount, actor.PartySlotIndex, 1);
        }

        return applied;
    }

    private int ApplyRpgOwnedBattleDamageToPartyMember(DungeonMonsterRuntimeData monster, int targetIndex, int damage, Color popupColor)
    {
        DungeonPartyMemberRuntimeData member = GetPartyMemberAtIndex(targetIndex);
        if (member == null || member.IsDefeated || member.CurrentHp <= 0)
        {
            return 0;
        }

        bool wasKnockedOut = member.IsDefeated;
        int safeDamage = Mathf.Max(1, damage);
        int applied = member.RuntimeState != null ? member.RuntimeState.ApplyDamage(safeDamage) : 0;
        if (member.RuntimeState == null)
        {
            int previousHp = member.CurrentHp;
            member.CurrentHp = Mathf.Max(0, previousHp - safeDamage);
            applied = previousHp - member.CurrentHp;
        }

        if (applied <= 0)
        {
            return 0;
        }

        _totalDamageTaken += applied;
        AddRunMemberContributionValue(_runMemberDamageTaken, targetIndex, applied);

        FlashPartyMember(targetIndex, popupColor);
        ShowBattlePopupForPartyMember(targetIndex, "-" + applied, popupColor);
        RecordRpgOwnedBattleEvent(
            PrototypeBattleEventKeys.DamageApplied,
            monster != null ? monster.MonsterId : string.Empty,
            member.MemberId,
            applied,
            member.DisplayName + " took " + applied + " damage.",
            actionKey: _pendingEnemyUsedSpecialAttack ? "skill" : "attack",
            actorName: monster != null ? monster.DisplayName : string.Empty,
            targetName: member.DisplayName,
            shortText: "-" + applied);
        ResolvePartyMemberKnockOut(member, wasKnockedOut);
        return applied;
    }

    private int ApplyRpgOwnedBattleHealToPartyMember(DungeonPartyMemberRuntimeData actor, DungeonPartyMemberRuntimeData member, int memberIndex, int recoverAmount, Color popupColor)
    {
        if (member == null || member.IsDefeated || member.CurrentHp <= 0)
        {
            return 0;
        }

        int safeRecover = Mathf.Max(1, recoverAmount);
        int recovered = member.RuntimeState != null ? member.RuntimeState.RecoverHp(safeRecover) : 0;
        if (member.RuntimeState == null)
        {
            int previousHp = member.CurrentHp;
            member.CurrentHp = Mathf.Min(member.MaxHp, previousHp + safeRecover);
            recovered = member.CurrentHp - previousHp;
        }

        if (recovered <= 0)
        {
            return 0;
        }

        _totalHealingDone += recovered;
        if (actor != null)
        {
            AddRunMemberContributionValue(_runMemberHealingDone, actor.PartySlotIndex, recovered);
        }

        FlashPartyMember(memberIndex, popupColor);
        ShowBattlePopupForPartyMember(memberIndex, "+" + recovered, popupColor);
        RecordRpgOwnedBattleEvent(
            PrototypeBattleEventKeys.HealApplied,
            actor != null ? actor.MemberId : string.Empty,
            member.MemberId,
            recovered,
            member.DisplayName + " recovered " + recovered + " HP.",
            actionKey: "skill",
            skillId: actor != null ? actor.DefaultSkillId : string.Empty,
            actorName: actor != null ? actor.DisplayName : string.Empty,
            targetName: member.DisplayName,
            shortText: "+" + recovered);
        return recovered;
    }

    private bool IsRpgOwnedBattleActionAvailable(BattleActionType action)
    {
        if (_dungeonRunState != DungeonRunState.Battle || IsBattleInputLocked())
        {
            return false;
        }

        if (action == BattleActionType.Attack || action == BattleActionType.Skill)
        {
            return _battleState == BattleState.PartyActionSelect && GetCurrentActorMember() != null;
        }

        if (action == BattleActionType.Move)
        {
            DungeonPartyMemberRuntimeData member = GetCurrentActorMember();
            return _battleState == BattleState.PartyActionSelect &&
                   member != null &&
                   CanRpgOwnedCurrentActorShiftBattleLane(member);
        }

        if (action == BattleActionType.EndTurn)
        {
            return _battleState == BattleState.PartyActionSelect && GetCurrentActorMember() != null;
        }

        if (action == BattleActionType.Retreat)
        {
            return _battleState == BattleState.PartyActionSelect || _battleState == BattleState.PartyTargetSelect;
        }

        return false;
    }

    private bool DoesRpgOwnedSkillRequireTarget(DungeonPartyMemberRuntimeData member)
    {
        if (member == null)
        {
            return false;
        }

        PrototypeRpgSkillDefinition resolvedSkillDefinition = ResolveMemberSkillDefinition(member);
        return GetResolvedSkillTargetKind(member, resolvedSkillDefinition) == "single_enemy";
    }

    private bool CanRpgOwnedCurrentActorShiftBattleLane(DungeonPartyMemberRuntimeData member)
    {
        return GetRpgOwnedAvailableBattleLaneKeys(member).Length > 0;
    }

    private bool CanRpgOwnedCurrentActorShiftToBattleLane(DungeonPartyMemberRuntimeData member, string targetLaneKey)
    {
        if (string.IsNullOrEmpty(targetLaneKey))
        {
            return false;
        }

        string[] availableLaneKeys = GetRpgOwnedAvailableBattleLaneKeys(member);
        for (int i = 0; i < availableLaneKeys.Length; i++)
        {
            if (availableLaneKeys[i] == targetLaneKey)
            {
                return true;
            }
        }

        return false;
    }

    private string[] GetRpgOwnedAvailableBattleLaneKeys(DungeonPartyMemberRuntimeData member)
    {
        if (member == null || member.RuntimeState == null || member.IsDefeated || member.CurrentHp <= 0)
        {
            return System.Array.Empty<string>();
        }

        return GetRpgOwnedAdjacentBattleLaneKeys(GetPartyMemberLaneKey(member));
    }

    private string[] GetRpgOwnedAdjacentBattleLaneKeys(string currentLaneKey)
    {
        switch (currentLaneKey)
        {
            case PrototypeBattleLaneKeys.Top:
                return new[] { PrototypeBattleLaneKeys.Mid };
            case PrototypeBattleLaneKeys.Mid:
                return new[] { PrototypeBattleLaneKeys.Top, PrototypeBattleLaneKeys.Bottom };
            case PrototypeBattleLaneKeys.Bottom:
                return new[] { PrototypeBattleLaneKeys.Mid };
            default:
                return System.Array.Empty<string>();
        }
    }

    private string GetRpgOwnedNextBattleLaneKey(string currentLaneKey)
    {
        string[] adjacentLaneKeys = GetRpgOwnedAdjacentBattleLaneKeys(currentLaneKey);
        return adjacentLaneKeys.Length > 0 ? adjacentLaneKeys[0] : PrototypeBattleLaneKeys.None;
    }

    private string BuildRpgOwnedNextBattleLaneLabel(DungeonPartyMemberRuntimeData member)
    {
        return GetBattleLaneLabel(GetRpgOwnedNextBattleLaneKey(GetPartyMemberLaneKey(member)));
    }

    private string BuildRpgOwnedBattleMoveSelectionFeedback(DungeonPartyMemberRuntimeData member)
    {
        string[] availableLaneKeys = GetRpgOwnedAvailableBattleLaneKeys(member);
        if (availableLaneKeys.Length <= 0)
        {
            return "No row shift available.";
        }

        string currentLaneLabel = GetBattleLaneLabel(GetPartyMemberLaneKey(member));
        string[] laneLabels = new string[availableLaneKeys.Length];
        for (int i = 0; i < availableLaneKeys.Length; i++)
        {
            laneLabels[i] = GetBattleLaneLabel(availableLaneKeys[i]);
        }

        return currentLaneLabel + " -> " + string.Join(" / ", laneLabels);
    }

    private string GetRpgOwnedBattleMoveActionKey(string laneKey)
    {
        switch (laneKey)
        {
            case PrototypeBattleLaneKeys.Top:
                return "move_front";
            case PrototypeBattleLaneKeys.Mid:
                return "move_middle";
            case PrototypeBattleLaneKeys.Bottom:
                return "move_back";
            default:
                return string.Empty;
        }
    }

    private string GetRpgOwnedBattleMoveLaneKey(string actionKey)
    {
        switch (string.IsNullOrWhiteSpace(actionKey) ? string.Empty : actionKey.Trim().ToLowerInvariant())
        {
            case "move_front":
                return PrototypeBattleLaneKeys.Top;
            case "move_middle":
                return PrototypeBattleLaneKeys.Mid;
            case "move_back":
                return PrototypeBattleLaneKeys.Bottom;
            default:
                return string.Empty;
        }
    }

    private bool IsRpgOwnedBattleMoveActionKey(string actionKey)
    {
        return !string.IsNullOrEmpty(GetRpgOwnedBattleMoveLaneKey(actionKey));
    }

    private bool TryResolveRpgOwnedBattleMoveAction(string actionKey)
    {
        string targetLaneKey = GetRpgOwnedBattleMoveLaneKey(actionKey);
        if (string.IsNullOrEmpty(targetLaneKey))
        {
            return false;
        }

        if (_dungeonRunState != DungeonRunState.Battle ||
            _battleState != BattleState.PartyActionSelect ||
            IsBattleInputLocked())
        {
            return false;
        }

        DungeonPartyMemberRuntimeData member = GetCurrentActorMember();
        if (member == null)
        {
            return false;
        }

        ClearBattleHoverState();
        _hoverBattleAction = BattleActionType.Move;
        _queuedBattleAction = BattleActionType.Move;
        LockBattleInput();

        if (!TryResolveRpgOwnedCurrentActorLaneShift(member, targetLaneKey))
        {
            ClearBattleInputLock();
            RefreshSelectionPrompt();
            RefreshDungeonPresentation();
            return false;
        }

        AdvanceRpgOwnedBattleAfterPartyAction();
        return true;
    }

    private bool TryResolveRpgOwnedCurrentActorLaneShift(DungeonPartyMemberRuntimeData member)
    {
        string[] availableLaneKeys = GetRpgOwnedAvailableBattleLaneKeys(member);
        return availableLaneKeys.Length > 0 && TryResolveRpgOwnedCurrentActorLaneShift(member, availableLaneKeys[0]);
    }

    private bool TryResolveRpgOwnedCurrentActorLaneShift(DungeonPartyMemberRuntimeData member, string targetLaneKey)
    {
        if (!CanRpgOwnedCurrentActorShiftToBattleLane(member, targetLaneKey))
        {
            SetBattleFeedbackText("No row shift available.");
            return false;
        }

        string currentLaneKey = GetPartyMemberLaneKey(member);
        string currentLaneLabel = GetBattleLaneLabel(currentLaneKey);
        string nextLaneLabel = GetBattleLaneLabel(targetLaneKey);
        member.RuntimeState.SetBattleLaneContext(targetLaneKey, nextLaneLabel, BuildPartyPositionRuleTextForLane(member, targetLaneKey), ResolvePartyBattleStanceKey(member));
        ShowBattlePopupForPartyMember(member.PartySlotIndex, "Shift", new Color(0.76f, 0.9f, 1f, 1f));
        string summary = member.DisplayName + " moved from " + currentLaneLabel + " to " + nextLaneLabel + ".";
        AppendBattleLog(summary);
        SetBattleFeedbackText(summary);
        RecordRpgOwnedBattleEvent(
            PrototypeBattleEventKeys.MoveResolved,
            member.MemberId,
            string.Empty,
            0,
            summary,
            actionKey: "move",
            phaseKey: "resolution",
            actorName: member.DisplayName,
            targetName: nextLaneLabel,
            shortText: "Row shift");
        RefreshSelectionPrompt();
        RefreshDungeonPresentation();
        return true;
    }
}
