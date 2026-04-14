using UnityEngine;

public sealed partial class StaticPlaceholderWorldView
{
    private PrototypeBattleLaneRuleResolution BuildRpgOwnedPartyActionLaneResolution(DungeonPartyMemberRuntimeData member, DungeonMonsterRuntimeData targetMonster, BattleActionType action, PrototypeRpgSkillDefinition skillDefinition)
    {
        PrototypeBattleLaneRuleResolution resolution = new PrototypeBattleLaneRuleResolution();
        string targetKind = ResolvePartyActionTargetKind(member, action, skillDefinition);
        string effectType = ResolvePartyActionEffectType(member, action, skillDefinition);
        resolution.ActorLaneKey = GetPartyMemberLaneKey(member);
        resolution.ActorLaneLabel = GetBattleLaneLabel(resolution.ActorLaneKey);
        resolution.PositionRuleText = BuildPartyPositionRuleText(member);
        resolution.TargetLaneKey = targetMonster != null ? GetMonsterLaneKey(targetMonster) : GetLaneKeyForTargetKind(targetKind);
        resolution.TargetLaneLabel = GetBattleLaneLabel(resolution.TargetLaneKey);
        resolution.ResolvedRangeKey = ResolvePartyActionRangeKey(member, action, targetKind, effectType);
        resolution.ResolvedLaneRuleKey = ResolvePartyActionLaneRuleKey(targetKind, resolution.ResolvedRangeKey);
        resolution.RangeText = BuildRangeText(resolution.ResolvedRangeKey);
        resolution.TargetRuleText = BuildTargetRuleText(resolution.ResolvedLaneRuleKey);
        bool requiresTarget = targetKind == "single_enemy";
        bool isReachable = !requiresTarget || DoesRangeReachLane(resolution.ActorLaneKey, resolution.TargetLaneKey, resolution.ResolvedRangeKey);
        resolution.ReachabilityStateKey = requiresTarget ? (isReachable ? "reachable" : "blocked") : "lane_agnostic";
        resolution.LaneImpactKey = !requiresTarget
            ? "lane_agnostic"
            : GetLaneImpactKey(resolution.ActorLaneKey, resolution.TargetLaneKey, isReachable);
        resolution.LaneImpactText = BuildLaneImpactText(resolution.ActorLaneLabel, resolution.TargetLaneLabel, resolution.ResolvedRangeKey, isReachable, targetKind);
        resolution.ReachabilitySummaryText = BuildReachabilitySummaryText(resolution.ActorLaneLabel, resolution.TargetLaneLabel, resolution.RangeText, isReachable, requiresTarget);
        resolution.PredictedReachabilityText = resolution.ReachabilitySummaryText;
        resolution.ThreatLaneKey = requiresTarget ? resolution.TargetLaneKey : GetThreatLaneKeyForTargetKind(targetKind);
        resolution.ThreatLaneLabel = requiresTarget ? resolution.TargetLaneLabel : GetThreatLaneLabelForTargetKind(targetKind);
        resolution.ThreatSummaryText = BuildThreatSummaryText(resolution.ThreatLaneLabel, resolution.RangeText, resolution.TargetRuleText, resolution.LaneImpactText, false, requiresTarget);
        return resolution;
    }

    private PrototypeBattleLaneRuleResolution BuildRpgOwnedEnemyActionLaneResolution(DungeonMonsterRuntimeData monster, int targetIndex, bool useSpecial)
    {
        PrototypeBattleLaneRuleResolution resolution = new PrototypeBattleLaneRuleResolution();
        DungeonPartyMemberRuntimeData targetMember = GetPartyMemberAtIndex(targetIndex);
        resolution.ActorLaneKey = GetMonsterLaneKey(monster);
        resolution.ActorLaneLabel = GetBattleLaneLabel(resolution.ActorLaneKey);
        resolution.PositionRuleText = BuildEnemyPositionRuleText(monster);
        resolution.TargetLaneKey = useSpecial && IsPartyWideEliteSpecial(monster, useSpecial)
            ? PrototypeBattleLaneKeys.Any
            : GetPartyMemberLaneKey(targetMember);
        resolution.TargetLaneLabel = GetBattleLaneLabel(resolution.TargetLaneKey);
        resolution.ResolvedRangeKey = ResolveEnemyActionRangeKey(monster, useSpecial);
        resolution.ResolvedLaneRuleKey = ResolveEnemyActionLaneRuleKey(monster, useSpecial, resolution.ResolvedRangeKey);
        resolution.RangeText = BuildRangeText(resolution.ResolvedRangeKey);
        resolution.TargetRuleText = BuildTargetRuleText(resolution.ResolvedLaneRuleKey);
        bool requiresTarget = !(useSpecial && IsPartyWideEliteSpecial(monster, useSpecial));
        bool isReachable = !requiresTarget || DoesRangeReachLane(resolution.ActorLaneKey, resolution.TargetLaneKey, resolution.ResolvedRangeKey);
        resolution.ReachabilityStateKey = requiresTarget ? (isReachable ? "reachable" : "blocked") : "lane_agnostic";
        resolution.LaneImpactKey = !requiresTarget
            ? "lane_agnostic"
            : GetLaneImpactKey(resolution.ActorLaneKey, resolution.TargetLaneKey, isReachable);
        resolution.LaneImpactText = BuildLaneImpactText(resolution.ActorLaneLabel, resolution.TargetLaneLabel, resolution.ResolvedRangeKey, isReachable, requiresTarget ? "single_enemy" : "all_allies");
        resolution.ReachabilitySummaryText = BuildReachabilitySummaryText(resolution.ActorLaneLabel, resolution.TargetLaneLabel, resolution.RangeText, isReachable, requiresTarget);
        resolution.PredictedReachabilityText = resolution.ReachabilitySummaryText;
        if (requiresTarget && isReachable)
        {
            int interceptIndex = ResolveRpgOwnedGuardInterceptPartyTargetIndex(monster, targetIndex, useSpecial, resolution);
            if (interceptIndex >= 0 && interceptIndex != targetIndex)
            {
                DungeonPartyMemberRuntimeData interceptMember = GetPartyMemberAtIndex(interceptIndex);
                string interceptName = interceptMember != null && !string.IsNullOrEmpty(interceptMember.DisplayName)
                    ? interceptMember.DisplayName
                    : "Lane guard";
                resolution.TargetRuleText += " Guard intercept possible.";
                resolution.LaneImpactText += " " + interceptName + " can intercept this lane.";
                resolution.ReachabilitySummaryText += " Guard intercept possible via " + interceptName + ".";
                resolution.PredictedReachabilityText = resolution.ReachabilitySummaryText;
            }
        }

        resolution.ThreatLaneKey = requiresTarget ? resolution.TargetLaneKey : PrototypeBattleLaneKeys.Any;
        resolution.ThreatLaneLabel = requiresTarget ? resolution.TargetLaneLabel : "All ally lanes";
        resolution.ThreatSummaryText = BuildThreatSummaryText(resolution.ThreatLaneLabel, resolution.RangeText, resolution.TargetRuleText, resolution.LaneImpactText, true, requiresTarget);
        return resolution;
    }

    private int ResolveRpgOwnedGuardInterceptPartyTargetIndex(DungeonMonsterRuntimeData monster, int targetIndex, bool useSpecial, PrototypeBattleLaneRuleResolution laneResolution)
    {
        if (monster == null || useSpecial && IsPartyWideEliteSpecial(monster, useSpecial) || ShouldIgnoreGuardIntercept(laneResolution != null ? laneResolution.ResolvedLaneRuleKey : string.Empty, laneResolution != null ? laneResolution.ResolvedRangeKey : string.Empty))
        {
            return targetIndex;
        }

        DungeonPartyMemberRuntimeData targetMember = GetPartyMemberAtIndex(targetIndex);
        if (targetMember == null || targetMember.IsDefeated || targetMember.CurrentHp <= 0)
        {
            return targetIndex;
        }

        string laneKey = GetPartyMemberLaneKey(targetMember);
        int interceptIndex = targetIndex;
        int bestDefense = int.MinValue;
        if (_activeDungeonParty == null || _activeDungeonParty.Members == null)
        {
            return targetIndex;
        }

        for (int i = 0; i < _activeDungeonParty.Members.Length; i++)
        {
            if (i == targetIndex)
            {
                continue;
            }

            DungeonPartyMemberRuntimeData candidate = _activeDungeonParty.Members[i];
            if (!CanPartyMemberGuardIntercept(candidate, targetMember, laneKey))
            {
                continue;
            }

            int defenseScore = candidate.Defense;
            if (defenseScore > bestDefense)
            {
                bestDefense = defenseScore;
                interceptIndex = i;
            }
        }

        return interceptIndex;
    }
}
