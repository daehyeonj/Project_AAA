using System.Collections.Generic;
using UnityEngine;

public sealed partial class StaticPlaceholderWorldView
{
    private const string BattleLaneRuleSetKey = "three_lane_minimal_v1";
    private const string BattlePositionRuleKey = "top_mid_bottom_v1";
    private const string BattleConsumableRuleKey = "inventory_bridge_not_wired";
    private const string BattleRetreatRuleKey = "retreat_enabled_in_battle";

    private PrototypeBattleContextData BuildCurrentBattleContextView()
    {
        return BuildRpgOwnedBattleContextView();
    }

    private PrototypeBattleRuntimeState BuildCurrentBattleRuntimeStateView(
        PrototypeBattleUiActorData currentActor = null,
        PrototypeBattleUiActionContextData actionContext = null,
        PrototypeBattleUiTargetContextData targetContext = null,
        PrototypeBattleUiTimelineData timeline = null,
        PrototypeEnemyIntentSnapshot enemyIntent = null)
    {
        return CreateRpgOwnedBattleRuntimeStateView(currentActor, actionContext, targetContext, timeline, enemyIntent);
    }

    private PrototypeBattleResultData BuildCurrentBattleCoreResultView()
    {
        return BuildRpgOwnedCurrentBattleCoreResultView();
    }

    private PrototypeBattleResultData BuildBattleCoreResultData(string outcomeKey)
    {
        return BuildRpgOwnedBattleCoreResultData(outcomeKey);
    }

    private string BuildBattleOutcomeSummaryText(string outcomeKey)
    {
        return BuildRpgOwnedBattleOutcomeSummaryText(outcomeKey);
    }

    private string BuildBattleRuleSummaryText()
    {
        return BuildRpgOwnedBattleRuleSummaryText();
    }

    private string BuildBattleRuntimeSummaryText(PrototypeBattleRuntimeState runtimeState)
    {
        return BuildRpgOwnedBattleRuntimeSummaryText(runtimeState);
    }

    private string BuildBattleTimelineRuntimeSummaryText(PrototypeBattleUiTimelineData timeline)
    {
        return BuildRpgOwnedBattleTimelineRuntimeSummaryText(timeline);
    }

    private string BuildBattlePartySeedSummary()
    {
        return BuildRpgOwnedBattlePartySeedSummary();
    }

    private string BuildBattleEnemySeedSummary()
    {
        return BuildRpgOwnedBattleEnemySeedSummary();
    }

    private string BuildBattleWorldModifierSummary()
    {
        return BuildRpgOwnedBattleWorldModifierSummary();
    }

    private string BuildBattleId(string encounterId)
    {
        return BuildRpgOwnedBattleId(encounterId);
    }

    private string BuildNotableBattleEventsSummary(int count)
    {
        return BuildRpgOwnedNotableBattleEventsSummary(count);
    }

    private bool IsLaneNotableBattleEvent(PrototypeBattleEventRecord record)
    {
        if (record == null || string.IsNullOrEmpty(record.EventKey))
        {
            return false;
        }

        return record.EventKey == PrototypeBattleEventKeys.TargetRejected ||
               record.EventKey == PrototypeBattleEventKeys.RangeRuleResolved ||
               record.EventKey == PrototypeBattleEventKeys.LaneRuleResolved ||
               record.EventKey == PrototypeBattleEventKeys.GuardInterceptTriggered ||
               record.EventKey == PrototypeBattleEventKeys.BurstWindowOpened ||
               record.EventKey == PrototypeBattleEventKeys.BurstWindowExtended ||
               record.EventKey == PrototypeBattleEventKeys.BurstWindowConsumed ||
               record.EventKey == PrototypeBattleEventKeys.EnemyIntentShown;
    }

    private PrototypeBattleLaneRuleResolution BuildPartyActionLaneResolution(DungeonPartyMemberRuntimeData member, DungeonMonsterRuntimeData targetMonster, BattleActionType action, PrototypeRpgSkillDefinition skillDefinition)
    {
        PrototypeBattleLaneRuleResolution resolution = BuildRpgOwnedPartyActionLaneResolution(member, targetMonster, action, skillDefinition);
        string targetKind = ResolvePartyActionTargetKind(member, action, skillDefinition);
        if (targetKind != "single_enemy")
        {
            return resolution;
        }

        // Current runtime source-of-truth is standard JRPG targeting, so living enemies
        // stay selectable regardless of leftover lane metadata carried by older battle rails.
        resolution.ResolvedRangeKey = PrototypeBattleRangeKeys.LaneAgnostic;
        resolution.ResolvedLaneRuleKey = PrototypeBattleLaneRuleKeys.LaneAgnostic;
        resolution.RangeText = "Standard target reach";
        resolution.TargetRuleText = "Targets any living enemy.";
        resolution.ReachabilityStateKey = targetMonster != null ? "reachable" : "pending_target";
        resolution.LaneImpactKey = "lane_agnostic";
        resolution.LaneImpactText = "Standard JRPG targeting.";
        resolution.ReachabilitySummaryText = targetMonster != null
            ? "Any living enemy can be targeted."
            : "Choose any living enemy.";
        resolution.PredictedReachabilityText = resolution.ReachabilitySummaryText;
        resolution.ThreatLaneKey = PrototypeBattleLaneKeys.Any;
        resolution.ThreatLaneLabel = "Any enemy";
        resolution.ThreatSummaryText = "Standard target selection.";
        return resolution;
    }

    private PrototypeBattleLaneRuleResolution BuildEnemyActionLaneResolution(DungeonMonsterRuntimeData monster, int targetIndex, bool useSpecial)
    {
        return BuildRpgOwnedEnemyActionLaneResolution(monster, targetIndex, useSpecial);
    }

    private string ResolvePartyActionTargetKind(DungeonPartyMemberRuntimeData member, BattleActionType action, PrototypeRpgSkillDefinition skillDefinition)
    {
        if (action == BattleActionType.Attack)
        {
            return "single_enemy";
        }

        if (action == BattleActionType.Retreat)
        {
            return "party";
        }

        return GetResolvedSkillTargetKind(member, skillDefinition);
    }

    private string ResolvePartyActionEffectType(DungeonPartyMemberRuntimeData member, BattleActionType action, PrototypeRpgSkillDefinition skillDefinition)
    {
        if (action == BattleActionType.Attack)
        {
            return "damage";
        }

        if (action == BattleActionType.Retreat)
        {
            return "retreat";
        }

        return GetResolvedSkillEffectType(member, skillDefinition);
    }

    private string ResolvePartyActionRangeKey(DungeonPartyMemberRuntimeData member, BattleActionType action, string targetKind, string effectType)
    {
        if (action == BattleActionType.Retreat || targetKind == "party")
        {
            return PrototypeBattleRangeKeys.PartyWide;
        }

        if (targetKind == "all_enemies")
        {
            return PrototypeBattleRangeKeys.AllEnemyLanes;
        }

        if (targetKind == "all_allies")
        {
            return PrototypeBattleRangeKeys.AllAllyLanes;
        }

        if (IsRangedPartyMember(member))
        {
            return PrototypeBattleRangeKeys.LaneAgnostic;
        }

        return ResolveMeleeRangeKeyForLane(GetPartyMemberLaneKey(member));
    }

    private string ResolvePartyActionLaneRuleKey(string targetKind, string rangeKey)
    {
        if (targetKind == "party")
        {
            return PrototypeBattleLaneRuleKeys.PartyWide;
        }

        if (targetKind == "all_enemies")
        {
            return PrototypeBattleLaneRuleKeys.AllEnemyLanes;
        }

        if (targetKind == "all_allies")
        {
            return PrototypeBattleLaneRuleKeys.AllAllyLanes;
        }

        if (rangeKey == PrototypeBattleRangeKeys.LaneAgnostic)
        {
            return PrototypeBattleLaneRuleKeys.AnyEnemyLane;
        }

        if (rangeKey == PrototypeBattleRangeKeys.SnipeAnyLane)
        {
            return PrototypeBattleLaneRuleKeys.BacklineSnipe;
        }

        if (rangeKey == PrototypeBattleRangeKeys.None)
        {
            return PrototypeBattleLaneRuleKeys.None;
        }

        if (rangeKey == PrototypeBattleRangeKeys.MeleeSameLane)
        {
            return PrototypeBattleLaneRuleKeys.SameLaneOnly;
        }

        return PrototypeBattleLaneRuleKeys.SameOrAdjacentEnemyLane;
    }

    private string ResolveEnemyActionRangeKey(DungeonMonsterRuntimeData monster, bool useSpecial)
    {
        if (useSpecial && IsPartyWideEliteSpecial(monster, useSpecial))
        {
            return PrototypeBattleRangeKeys.AllAllyLanes;
        }

        return ResolveMeleeRangeKeyForLane(GetMonsterLaneKey(monster));
    }

    private string ResolveEnemyActionLaneRuleKey(DungeonMonsterRuntimeData monster, bool useSpecial, string rangeKey)
    {
        if (useSpecial && IsPartyWideEliteSpecial(monster, useSpecial))
        {
            return PrototypeBattleLaneRuleKeys.AllAllyLanes;
        }

        if (rangeKey == PrototypeBattleRangeKeys.None)
        {
            return PrototypeBattleLaneRuleKeys.None;
        }

        if (rangeKey == PrototypeBattleRangeKeys.PierceLine)
        {
            return PrototypeBattleLaneRuleKeys.PierceLine;
        }

        if (rangeKey == PrototypeBattleRangeKeys.SnipeAnyLane)
        {
            return PrototypeBattleLaneRuleKeys.BacklineSnipe;
        }

        if (rangeKey == PrototypeBattleRangeKeys.MeleeSameLane)
        {
            return PrototypeBattleLaneRuleKeys.SameLaneOnly;
        }

        return PrototypeBattleLaneRuleKeys.SameOrAdjacentEnemyLane;
    }

    private bool DoesRangeReachLane(string actorLaneKey, string targetLaneKey, string rangeKey)
    {
        if (rangeKey == PrototypeBattleRangeKeys.None)
        {
            return false;
        }

        if (rangeKey == PrototypeBattleRangeKeys.AllEnemyLanes ||
            rangeKey == PrototypeBattleRangeKeys.AllAllyLanes ||
            rangeKey == PrototypeBattleRangeKeys.PartyWide ||
            rangeKey == PrototypeBattleRangeKeys.SnipeAnyLane ||
            rangeKey == PrototypeBattleRangeKeys.PierceLine ||
            rangeKey == PrototypeBattleRangeKeys.LaneAgnostic)
        {
            return true;
        }

        int actorLane = GetBattleLaneIndex(actorLaneKey);
        int targetLane = GetBattleLaneIndex(targetLaneKey);
        if (actorLane < 0 || targetLane < 0)
        {
            return false;
        }

        if (rangeKey == PrototypeBattleRangeKeys.MeleeSameLane)
        {
            return targetLane == 0;
        }

        if (rangeKey == PrototypeBattleRangeKeys.MeleeSameOrAdjacent)
        {
            return targetLane <= 1;
        }

        return actorLane == targetLane;
    }

    private string GetLaneImpactKey(string actorLaneKey, string targetLaneKey, bool isReachable)
    {
        if (!isReachable)
        {
            return "out_of_reach";
        }

        int actorLane = GetBattleLaneIndex(actorLaneKey);
        int targetLane = GetBattleLaneIndex(targetLaneKey);
        if (actorLane < 0 || targetLane < 0)
        {
            return "lane_agnostic";
        }

        return actorLane == targetLane ? "same_lane" : "adjacent_lane";
    }

    private string BuildRangeText(string rangeKey)
    {
        switch (rangeKey)
        {
            case PrototypeBattleRangeKeys.AllEnemyLanes:
                return "Entire enemy row";
            case PrototypeBattleRangeKeys.AllAllyLanes:
                return "Entire ally row";
            case PrototypeBattleRangeKeys.PartyWide:
                return "Party-wide";
            case PrototypeBattleRangeKeys.SnipeAnyLane:
                return "Any row precision";
            case PrototypeBattleRangeKeys.PierceLine:
                return "Frontline pierce";
            case PrototypeBattleRangeKeys.MeleeSameLane:
                return "Front Row only";
            case PrototypeBattleRangeKeys.MeleeSameOrAdjacent:
                return "Front Row or Middle Row";
            case PrototypeBattleRangeKeys.LaneAgnostic:
                return "Entire Row";
            case PrototypeBattleRangeKeys.None:
                return "No row reach";
            default:
                return "Row agnostic";
        }
    }

    private string BuildTargetRuleText(string laneRuleKey)
    {
        switch (laneRuleKey)
        {
            case PrototypeBattleLaneRuleKeys.AllEnemyLanes:
                return "Hits every enemy across the entire row.";
            case PrototypeBattleLaneRuleKeys.AllAllyLanes:
                return "Covers every ally across the entire row.";
            case PrototypeBattleLaneRuleKeys.PartyWide:
                return "Acts on the whole party.";
            case PrototypeBattleLaneRuleKeys.BacklineSnipe:
                return "Can pick any row and bypasses guards.";
            case PrototypeBattleLaneRuleKeys.PierceLine:
                return "Pierces through the front row for a single target hit.";
            case PrototypeBattleLaneRuleKeys.SameLaneOnly:
                return "Can only target the enemy Front Row.";
            case PrototypeBattleLaneRuleKeys.SameOrAdjacentEnemyLane:
                return "Can target the enemy Front Row or Middle Row.";
            case PrototypeBattleLaneRuleKeys.AnyEnemyLane:
                return "Can pick any enemy row.";
            case PrototypeBattleLaneRuleKeys.GuardIntercept:
                return "A front-row guard can intercept this hit.";
            case PrototypeBattleLaneRuleKeys.None:
                return "Current row cannot make a direct attack.";
            default:
                return "No explicit row rule.";
        }
    }

    private string BuildLaneImpactText(string actorLaneLabel, string targetLaneLabel, string rangeKey, bool isReachable, string targetKind)
    {
        if (targetKind == "all_enemies")
        {
            return "Sweeps the entire enemy row.";
        }

        if (targetKind == "all_allies")
        {
            return "Covers the entire ally row.";
        }

        if (targetKind == "party")
        {
            return "Acts on the whole party.";
        }

        if (!isReachable)
        {
            return actorLaneLabel + " cannot currently reach " + targetLaneLabel + ".";
        }

        if (rangeKey == PrototypeBattleRangeKeys.SnipeAnyLane)
        {
            return "Precision reach from " + actorLaneLabel + " into " + targetLaneLabel + ".";
        }

        if (rangeKey == PrototypeBattleRangeKeys.PierceLine)
        {
            return "Piercing pressure from " + actorLaneLabel + " into " + targetLaneLabel + ".";
        }

        if (rangeKey == PrototypeBattleRangeKeys.LaneAgnostic)
        {
            return "Ranged pressure can reach " + targetLaneLabel + " from anywhere.";
        }

        if (rangeKey == PrototypeBattleRangeKeys.MeleeSameLane)
        {
            return "Melee reach is limited to the enemy Front Row.";
        }

        if (rangeKey == PrototypeBattleRangeKeys.MeleeSameOrAdjacent)
        {
            return "Melee reach extends through the enemy Front Row and Middle Row.";
        }

        if (rangeKey == PrototypeBattleRangeKeys.None)
        {
            return actorLaneLabel + " has no direct melee reach.";
        }

        return actorLaneLabel == targetLaneLabel
            ? "Direct pressure in " + targetLaneLabel + "."
            : "Cross-row pressure into " + targetLaneLabel + ".";
    }

    private string BuildReachabilitySummaryText(string actorLaneLabel, string targetLaneLabel, string rangeText, bool isReachable, bool requiresTarget)
    {
        if (!requiresTarget)
        {
            return rangeText + " | No single target required.";
        }

        string blockedReason = rangeText == "Front Row only"
            ? "Front Row required."
            : rangeText == "Front Row or Middle Row"
                ? "Front Row or Middle Row required."
                : rangeText == "No row reach"
                    ? "Cannot attack from this row."
                    : "Target blocked.";
        return actorLaneLabel + " -> " + targetLaneLabel + " | " + rangeText + " | " + (isReachable ? "Reachable." : blockedReason);
    }

    private bool ShouldIgnoreGuardIntercept(string laneRuleKey, string rangeKey)
    {
        return rangeKey == PrototypeBattleRangeKeys.SnipeAnyLane ||
               rangeKey == PrototypeBattleRangeKeys.PierceLine ||
               laneRuleKey == PrototypeBattleLaneRuleKeys.BacklineSnipe ||
               laneRuleKey == PrototypeBattleLaneRuleKeys.PierceLine ||
               laneRuleKey == PrototypeBattleLaneRuleKeys.AllEnemyLanes ||
               laneRuleKey == PrototypeBattleLaneRuleKeys.AllAllyLanes ||
               laneRuleKey == PrototypeBattleLaneRuleKeys.PartyWide;
    }

    private int ResolveGuardInterceptPartyTargetIndex(DungeonMonsterRuntimeData monster, int targetIndex, bool useSpecial, PrototypeBattleLaneRuleResolution laneResolution)
    {
        return ResolveRpgOwnedGuardInterceptPartyTargetIndex(monster, targetIndex, useSpecial, laneResolution);
    }

    private bool CanPartyMemberGuardIntercept(DungeonPartyMemberRuntimeData candidate, DungeonPartyMemberRuntimeData originalTarget, string laneKey)
    {
        if (candidate == null || originalTarget == null || candidate.IsDefeated || candidate.CurrentHp <= 0)
        {
            return false;
        }

        if (GetPartyMemberLaneKey(candidate) != laneKey)
        {
            return false;
        }

        string stanceKey = ResolvePartyBattleStanceKey(candidate);
        if (stanceKey == "front_guard" || stanceKey == "support_anchor")
        {
            return true;
        }

        return candidate.Defense >= originalTarget.Defense + 1;
    }

    private string GetThreatLaneLabelForTargetKind(string targetKind)
    {
        switch (targetKind)
        {
            case "all_enemies":
                return "Entire enemy row";
            case "all_allies":
                return "Entire ally row";
            case "party":
                return "Party-wide";
            default:
                return string.Empty;
        }
    }

    private string GetThreatLaneKeyForTargetKind(string targetKind)
    {
        switch (targetKind)
        {
            case "all_enemies":
            case "all_allies":
            case "party":
                return PrototypeBattleLaneKeys.Any;
            default:
                return PrototypeBattleLaneKeys.None;
        }
    }

    private string BuildThreatSummaryText(string threatLaneLabel, string rangeText, string targetRuleText, string laneImpactText, bool isEnemyAction, bool requiresTarget)
    {
        List<string> parts = new List<string>();
        if (!string.IsNullOrEmpty(threatLaneLabel))
        {
            parts.Add((isEnemyAction ? "Threat " : "Pressure ") + threatLaneLabel);
        }

        if (!string.IsNullOrEmpty(rangeText))
        {
            parts.Add(rangeText);
        }

        if (!string.IsNullOrEmpty(targetRuleText))
        {
            parts.Add(targetRuleText);
        }
        else if (!requiresTarget)
        {
            parts.Add("No single-target restriction.");
        }

        if (!string.IsNullOrEmpty(laneImpactText))
        {
            parts.Add(laneImpactText);
        }

        return parts.Count > 0 ? string.Join(" | ", parts.ToArray()) : string.Empty;
    }

    private bool IsRangedPartyMember(DungeonPartyMemberRuntimeData member)
    {
        return member != null && member.RoleTag == "mage";
    }

    private string ResolveMeleeRangeKeyForLane(string laneKey)
    {
        switch (laneKey)
        {
            case PrototypeBattleLaneKeys.Top:
                return PrototypeBattleRangeKeys.MeleeSameOrAdjacent;
            case PrototypeBattleLaneKeys.Mid:
                return PrototypeBattleRangeKeys.MeleeSameLane;
            case PrototypeBattleLaneKeys.Bottom:
                return PrototypeBattleRangeKeys.None;
            default:
                return PrototypeBattleRangeKeys.MeleeSameLane;
        }
    }

    private string ResolveDefaultPartyLaneKey(DungeonPartyMemberRuntimeData member)
    {
        if (member == null)
        {
            return PrototypeBattleLaneKeys.None;
        }

        switch (member.RoleTag)
        {
            case "warrior":
                return PrototypeBattleLaneKeys.Top;
            case "rogue":
            case "cleric":
                return PrototypeBattleLaneKeys.Mid;
            case "mage":
                return PrototypeBattleLaneKeys.Bottom;
            default:
                return PrototypeBattleLaneKeys.Mid;
        }
    }

    private string ResolveDefaultEnemyLaneKey(DungeonMonsterRuntimeData monster)
    {
        return monster == null ? PrototypeBattleLaneKeys.None : PrototypeBattleLaneKeys.Top;
    }

    private string BuildRowReachText(string laneKey, bool isRanged)
    {
        if (isRanged)
        {
            return "Ranged attacks cover the Entire Row.";
        }

        switch (laneKey)
        {
            case PrototypeBattleLaneKeys.Top:
                return "Melee attacks reach Front Row and Middle Row.";
            case PrototypeBattleLaneKeys.Mid:
                return "Melee attacks reach Front Row only.";
            case PrototypeBattleLaneKeys.Bottom:
                return "Melee attacks cannot reach from Back Row.";
            default:
                return "Row reach pending.";
        }
    }

    private string GetPartyMemberLaneKey(DungeonPartyMemberRuntimeData member)
    {
        if (member == null)
        {
            return PrototypeBattleLaneKeys.None;
        }

        if (member.RuntimeState != null && !string.IsNullOrWhiteSpace(member.RuntimeState.LaneKey))
        {
            return member.RuntimeState.LaneKey;
        }

        return ResolveDefaultPartyLaneKey(member);
    }

    private string GetMonsterLaneKey(DungeonMonsterRuntimeData monster)
    {
        if (monster == null)
        {
            return PrototypeBattleLaneKeys.None;
        }

        if (monster.RuntimeState != null && !string.IsNullOrWhiteSpace(monster.RuntimeState.LaneKey))
        {
            return monster.RuntimeState.LaneKey;
        }

        return ResolveDefaultEnemyLaneKey(monster);
    }

    private string GetLaneKeyForTargetKind(string targetKind)
    {
        return targetKind == "all_enemies" || targetKind == "all_allies" || targetKind == "party"
            ? PrototypeBattleLaneKeys.Any
            : PrototypeBattleLaneKeys.None;
    }

    private int GetBattleLaneIndex(string laneKey)
    {
        switch (laneKey)
        {
            case PrototypeBattleLaneKeys.Top:
                return 0;
            case PrototypeBattleLaneKeys.Mid:
                return 1;
            case PrototypeBattleLaneKeys.Bottom:
                return 2;
            default:
                return -1;
        }
    }

    private string GetBattleLaneLabel(string laneKey)
    {
        switch (laneKey)
        {
            case PrototypeBattleLaneKeys.Top:
                return "Front Row";
            case PrototypeBattleLaneKeys.Mid:
                return "Middle Row";
            case PrototypeBattleLaneKeys.Bottom:
                return "Back Row";
            case PrototypeBattleLaneKeys.Any:
                return "Entire Row";
            default:
                return "No row";
        }
    }

    private string BuildPartyPositionRuleText(DungeonPartyMemberRuntimeData member)
    {
        if (member == null)
        {
            return "Party position pending.";
        }

        string laneKey = GetPartyMemberLaneKey(member);
        return BuildPartyPositionRuleTextForLane(member, laneKey);
    }

    private string BuildEnemyPositionRuleText(DungeonMonsterRuntimeData monster)
    {
        if (monster == null)
        {
            return "Enemy position pending.";
        }

        string laneKey = GetMonsterLaneKey(monster);
        return BuildEnemyPositionRuleTextForLane(monster, laneKey);
    }

    private string BuildPartyPositionRuleTextForLane(DungeonPartyMemberRuntimeData member, string laneKey)
    {
        if (member == null)
        {
            return "Party position pending.";
        }

        return GetBattleLaneLabel(laneKey) + " | " + member.RoleLabel + " | " + BuildRowReachText(laneKey, IsRangedPartyMember(member));
    }

    private string BuildEnemyPositionRuleTextForLane(DungeonMonsterRuntimeData monster, string laneKey)
    {
        if (monster == null)
        {
            return "Enemy position pending.";
        }

        return GetBattleLaneLabel(laneKey) + " | Enemy melee unit | " + BuildRowReachText(laneKey, false);
    }

    private string BuildTimelineThreatLabelForPartyMember(DungeonPartyMemberRuntimeData member)
    {
        if (member == null)
        {
            return string.Empty;
        }

        if (member.SkillType == PartySkillType.AllEnemies)
        {
            return "Sweep";
        }

        if (member.SkillType == PartySkillType.PartyHeal)
        {
            return "Support";
        }

        return IsRangedPartyMember(member) ? "Ranged" : "Melee";
    }

    private string BuildTimelineThreatLabelForEnemy(DungeonMonsterRuntimeData monster, bool useSpecial)
    {
        if (monster == null)
        {
            return string.Empty;
        }

        if (useSpecial && IsPartyWideEliteSpecial(monster, useSpecial))
        {
            return "Sweep";
        }

        return "Melee";
    }

    private string ResolvePartyBattleStanceKey(DungeonPartyMemberRuntimeData member)
    {
        if (member == null)
        {
            return string.Empty;
        }

        if (member.RoleTag == "rogue")
        {
            return "precision_flank";
        }

        if (member.SkillType == PartySkillType.PartyHeal)
        {
            return "support_anchor";
        }

        if (member.RoleTag == "mage")
        {
            return "lane_sweep";
        }

        return "front_guard";
    }

    private string ResolveEnemyBattleStanceKey(DungeonMonsterRuntimeData monster)
    {
        if (monster == null)
        {
            return string.Empty;
        }

        switch (monster.EncounterRole)
        {
            case MonsterEncounterRole.Skirmisher:
                return "flank_pressure";
            case MonsterEncounterRole.Striker:
                return "execution_pressure";
            default:
                return "guard_anchor";
        }
    }
}
