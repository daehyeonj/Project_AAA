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

        if (effectType == "finisher_damage" || (member != null && member.RoleTag == "rogue" && action == BattleActionType.Skill))
        {
            return PrototypeBattleRangeKeys.SnipeAnyLane;
        }

        if (member != null && member.RoleTag == "warrior" && action == BattleActionType.Skill)
        {
            return PrototypeBattleRangeKeys.MeleeSameOrAdjacent;
        }

        return PrototypeBattleRangeKeys.MeleeSameLane;
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

    private string ResolveEnemyActionRangeKey(DungeonMonsterRuntimeData monster, bool useSpecial)
    {
        if (useSpecial && IsPartyWideEliteSpecial(monster, useSpecial))
        {
            return PrototypeBattleRangeKeys.AllAllyLanes;
        }

        if (useSpecial && monster != null && monster.EncounterRole == MonsterEncounterRole.Striker)
        {
            return PrototypeBattleRangeKeys.PierceLine;
        }

        if (monster != null && monster.EncounterRole == MonsterEncounterRole.Skirmisher)
        {
            return PrototypeBattleRangeKeys.SnipeAnyLane;
        }

        if (monster != null && monster.EncounterRole == MonsterEncounterRole.Bulwark)
        {
            return PrototypeBattleRangeKeys.MeleeSameLane;
        }

        return PrototypeBattleRangeKeys.MeleeSameOrAdjacent;
    }

    private string ResolveEnemyActionLaneRuleKey(DungeonMonsterRuntimeData monster, bool useSpecial, string rangeKey)
    {
        if (useSpecial && IsPartyWideEliteSpecial(monster, useSpecial))
        {
            return PrototypeBattleLaneRuleKeys.AllAllyLanes;
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
            return actorLane == targetLane;
        }

        return Mathf.Abs(actorLane - targetLane) <= 1;
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
                return "All enemy lanes";
            case PrototypeBattleRangeKeys.AllAllyLanes:
                return "All ally lanes";
            case PrototypeBattleRangeKeys.PartyWide:
                return "Party-wide";
            case PrototypeBattleRangeKeys.SnipeAnyLane:
                return "Any lane snipe";
            case PrototypeBattleRangeKeys.PierceLine:
                return "Piercing line";
            case PrototypeBattleRangeKeys.MeleeSameLane:
                return "Same lane only";
            case PrototypeBattleRangeKeys.MeleeSameOrAdjacent:
                return "Same or adjacent lane";
            default:
                return "Lane agnostic";
        }
    }

    private string BuildTargetRuleText(string laneRuleKey)
    {
        switch (laneRuleKey)
        {
            case PrototypeBattleLaneRuleKeys.AllEnemyLanes:
                return "Hits every enemy lane.";
            case PrototypeBattleLaneRuleKeys.AllAllyLanes:
                return "Covers every ally lane.";
            case PrototypeBattleLaneRuleKeys.PartyWide:
                return "Acts on the whole party.";
            case PrototypeBattleLaneRuleKeys.BacklineSnipe:
                return "Can pick any lane and bypasses guards.";
            case PrototypeBattleLaneRuleKeys.PierceLine:
                return "Pierces through lane guards for a single target hit.";
            case PrototypeBattleLaneRuleKeys.SameLaneOnly:
                return "Needs the exact same lane.";
            case PrototypeBattleLaneRuleKeys.SameOrAdjacentEnemyLane:
                return "Needs same or adjacent lane.";
            case PrototypeBattleLaneRuleKeys.AnyEnemyLane:
                return "Can pick any enemy lane.";
            case PrototypeBattleLaneRuleKeys.GuardIntercept:
                return "A lane guard can intercept this hit.";
            default:
                return "No explicit lane rule.";
        }
    }

    private string BuildLaneImpactText(string actorLaneLabel, string targetLaneLabel, string rangeKey, bool isReachable, string targetKind)
    {
        if (targetKind == "all_enemies")
        {
            return "Sweeps all enemy lanes.";
        }

        if (targetKind == "all_allies")
        {
            return "Covers all ally lanes.";
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
            return "Precision reach from " + actorLaneLabel + " to " + targetLaneLabel + ".";
        }

        if (rangeKey == PrototypeBattleRangeKeys.PierceLine)
        {
            return "Piercing pressure from " + actorLaneLabel + " into " + targetLaneLabel + ".";
        }

        if (rangeKey == PrototypeBattleRangeKeys.MeleeSameLane)
        {
            return "Locked pressure in " + targetLaneLabel + ".";
        }

        return actorLaneLabel == targetLaneLabel
            ? "Direct pressure in " + targetLaneLabel + "."
            : "Cross-lane pressure into " + targetLaneLabel + ".";
    }

    private string BuildReachabilitySummaryText(string actorLaneLabel, string targetLaneLabel, string rangeText, bool isReachable, bool requiresTarget)
    {
        if (!requiresTarget)
        {
            return rangeText + " | No single target required.";
        }

        string blockedReason = rangeText == "Same lane only"
            ? "Same lane required."
            : rangeText == "Same or adjacent lane"
                ? "Adjacent lane only."
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
                return "All enemy lanes";
            case "all_allies":
                return "All ally lanes";
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

        return member.PartySlotIndex <= 0
            ? PrototypeBattleLaneKeys.Top
            : member.PartySlotIndex == 1 || member.PartySlotIndex == 3
                ? PrototypeBattleLaneKeys.Mid
                : PrototypeBattleLaneKeys.Bottom;
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

        return monster.GridPosition.y >= 5
            ? PrototypeBattleLaneKeys.Top
            : monster.GridPosition.y >= 4
                ? PrototypeBattleLaneKeys.Mid
                : PrototypeBattleLaneKeys.Bottom;
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
                return "Top lane";
            case PrototypeBattleLaneKeys.Mid:
                return "Mid lane";
            case PrototypeBattleLaneKeys.Bottom:
                return "Bottom lane";
            case PrototypeBattleLaneKeys.Any:
                return "Any lane";
            default:
                return "No lane";
        }
    }

    private string BuildPartyPositionRuleText(DungeonPartyMemberRuntimeData member)
    {
        if (member == null)
        {
            return "Party position pending.";
        }

        if (member.RuntimeState != null && !string.IsNullOrWhiteSpace(member.RuntimeState.PositionRuleText))
        {
            return member.RuntimeState.PositionRuleText;
        }

        return member.PartySlotIndex == 3
            ? "Backline support anchor."
            : "Frontline lane anchor.";
    }

    private string BuildEnemyPositionRuleText(DungeonMonsterRuntimeData monster)
    {
        if (monster == null)
        {
            return "Enemy position pending.";
        }

        if (monster.RuntimeState != null && !string.IsNullOrWhiteSpace(monster.RuntimeState.PositionRuleText))
        {
            return monster.RuntimeState.PositionRuleText;
        }

        switch (monster.EncounterRole)
        {
            case MonsterEncounterRole.Skirmisher:
                return "Flank reach pressure.";
            case MonsterEncounterRole.Striker:
                return "Forward execution pressure.";
            default:
                return "Front guard anchor.";
        }
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

        return member.RoleTag == "rogue" ? "Snipe" : "Melee";
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

        return monster.EncounterRole == MonsterEncounterRole.Skirmisher
            ? "Flank"
            : monster.EncounterRole == MonsterEncounterRole.Striker
                ? "Pressure"
                : "Guard";
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
