using System;
using System.Collections.Generic;
using UnityEngine;

public sealed partial class StaticPlaceholderWorldView
{
    public string CurrentPartyStatusEffectSummaryText => ResolveRpgStatusSurfaceText(
        BuildCurrentPartyStatusEffectSummaryText(),
        _runResultState != RunResultState.None && _latestRpgRunResultSnapshot.PartyOutcome != null
            ? _latestRpgRunResultSnapshot.PartyOutcome.StatusEffectSummaryText
            : _lastResolvedPartyStatusEffectSummaryText);

    public string CurrentEnemyStatusEffectSummaryText => ResolveRpgStatusSurfaceText(
        BuildCurrentEnemyStatusEffectSummaryText(),
        _lastResolvedEnemyStatusEffectSummaryText);

    public string CurrentStatusUsageSummaryText => ResolveRpgStatusSurfaceText(
        BuildCurrentStatusUsageSummaryText(),
        _runResultState != RunResultState.None
            ? _latestRpgRunResultSnapshot.NotableStatusUsageSummaryText
            : _lastResolvedStatusUsageSummaryText);

    private string ResolveRpgStatusSurfaceText(string liveText, string fallbackText)
    {
        if (!string.IsNullOrEmpty(liveText))
        {
            return liveText;
        }

        return string.IsNullOrEmpty(fallbackText) ? "None" : fallbackText;
    }

    private void ResetRpgEncounterStatusRuntime(bool clearReadback)
    {
        if (_activeDungeonParty != null && _activeDungeonParty.Members != null)
        {
            for (int i = 0; i < _activeDungeonParty.Members.Length; i++)
            {
                ResetRpgMemberStatusRuntime(_activeDungeonParty.Members[i]);
            }
        }

        for (int i = 0; i < _activeMonsters.Count; i++)
        {
            ResetRpgMonsterStatusRuntime(_activeMonsters[i]);
        }

        if (clearReadback)
        {
            _lastResolvedPartyStatusEffectSummaryText = string.Empty;
            _lastResolvedEnemyStatusEffectSummaryText = string.Empty;
            _lastResolvedStatusUsageSummaryText = string.Empty;
        }
    }

    private void ResetRpgMemberStatusRuntime(DungeonPartyMemberRuntimeData member)
    {
        if (member == null)
        {
            return;
        }

        member.StatusEffects = Array.Empty<PrototypeRpgStatusRuntimeState>();
        member.StatusEffectSummaryText = string.Empty;
        member.StatusApplicationsThisEncounter = 0;
        member.StatusTicksResolvedThisEncounter = 0;
    }

    private void ResetRpgMonsterStatusRuntime(DungeonMonsterRuntimeData monster)
    {
        if (monster == null)
        {
            return;
        }

        monster.StatusEffects = Array.Empty<PrototypeRpgStatusRuntimeState>();
        monster.StatusEffectSummaryText = string.Empty;
        monster.StatusApplicationsThisEncounter = 0;
        monster.StatusTicksResolvedThisEncounter = 0;
    }

    private void CaptureRpgEncounterStatusReadback()
    {
        _lastResolvedPartyStatusEffectSummaryText = BuildCurrentPartyStatusEffectSummaryText();
        _lastResolvedEnemyStatusEffectSummaryText = BuildCurrentEnemyStatusEffectSummaryText();
        _lastResolvedStatusUsageSummaryText = BuildCurrentStatusUsageSummaryText();
    }

    private string BuildCurrentPartyStatusEffectSummaryText()
    {
        if (_activeDungeonParty == null || _activeDungeonParty.Members == null)
        {
            return string.Empty;
        }

        List<string> parts = new List<string>();
        for (int i = 0; i < _activeDungeonParty.Members.Length; i++)
        {
            DungeonPartyMemberRuntimeData member = _activeDungeonParty.Members[i];
            if (member == null)
            {
                continue;
            }

            string summaryText = BuildRpgStatusEffectSummaryText(member.StatusEffects);
            if (!string.IsNullOrEmpty(summaryText))
            {
                parts.Add(member.DisplayName + " " + summaryText);
            }
        }

        return parts.Count <= 0 ? string.Empty : "Party status: " + string.Join(" | ", parts.ToArray());
    }

    private string BuildCurrentEnemyStatusEffectSummaryText()
    {
        List<string> parts = new List<string>();
        for (int i = 0; i < _activeMonsters.Count; i++)
        {
            DungeonMonsterRuntimeData monster = _activeMonsters[i];
            if (monster == null || monster.IsDefeated || monster.CurrentHp <= 0)
            {
                continue;
            }

            string summaryText = BuildRpgStatusEffectSummaryText(monster.StatusEffects);
            if (!string.IsNullOrEmpty(summaryText))
            {
                parts.Add(monster.DisplayName + " " + summaryText);
            }
        }

        return parts.Count <= 0 ? string.Empty : "Enemy status: " + string.Join(" | ", parts.ToArray());
    }

    private string BuildCurrentStatusUsageSummaryText()
    {
        if (_activeDungeonParty == null || _activeDungeonParty.Members == null)
        {
            return string.Empty;
        }

        List<string> parts = new List<string>();
        for (int i = 0; i < _activeDungeonParty.Members.Length; i++)
        {
            DungeonPartyMemberRuntimeData member = _activeDungeonParty.Members[i];
            if (member == null)
            {
                continue;
            }

            int appliedCount = GetRunMemberContributionValue(_runMemberStatusApplied, i);
            int tickCount = GetRunMemberContributionValue(_runMemberStatusTicksResolved, i);
            if (appliedCount > 0 || tickCount > 0)
            {
                parts.Add(member.DisplayName + " " + appliedCount + " apply / " + tickCount + " tick");
            }
        }

        return parts.Count <= 0 ? string.Empty : "Status usage: " + string.Join(" | ", parts.ToArray());
    }

    private string BuildRpgStatusEffectSummaryText(PrototypeRpgStatusRuntimeState[] statuses)
    {
        if (statuses == null || statuses.Length <= 0)
        {
            return string.Empty;
        }

        List<string> parts = new List<string>();
        for (int i = 0; i < statuses.Length; i++)
        {
            PrototypeRpgStatusRuntimeState status = statuses[i];
            if (status == null || string.IsNullOrEmpty(status.StatusId) || status.RemainingTurns <= 0)
            {
                continue;
            }

            string label = !string.IsNullOrEmpty(status.ShortLabel) ? status.ShortLabel : status.StatusId;
            string stackText = status.StackCount > 1 ? " x" + status.StackCount : string.Empty;
            string powerText = status.AppliedPowerValue > 1 ? " p" + status.AppliedPowerValue : string.Empty;
            parts.Add(label + stackText + " " + status.RemainingTurns + "t" + powerText);
        }

        return parts.Count <= 0 ? string.Empty : string.Join(", ", parts.ToArray());
    }

    private void RefreshRpgStatusSummaryText(DungeonPartyMemberRuntimeData member)
    {
        if (member == null)
        {
            return;
        }

        member.StatusEffectSummaryText = BuildRpgStatusEffectSummaryText(member.StatusEffects);
    }

    private void RefreshRpgStatusSummaryText(DungeonMonsterRuntimeData monster)
    {
        if (monster == null)
        {
            return;
        }

        monster.StatusEffectSummaryText = BuildRpgStatusEffectSummaryText(monster.StatusEffects);
    }

    private PrototypeRpgStatusRuntimeState GetRpgStatusEffectState(PrototypeRpgStatusRuntimeState[] statuses, string statusId)
    {
        if (statuses == null || statuses.Length <= 0 || string.IsNullOrEmpty(statusId))
        {
            return null;
        }

        string normalizedId = statusId.Trim().ToLowerInvariant();
        for (int i = 0; i < statuses.Length; i++)
        {
            PrototypeRpgStatusRuntimeState status = statuses[i];
            if (status != null && status.StatusId == normalizedId && status.RemainingTurns > 0)
            {
                return status;
            }
        }

        return null;
    }

    private int GetRpgStatusPowerValue(PrototypeRpgStatusRuntimeState[] statuses, string statusId)
    {
        PrototypeRpgStatusRuntimeState status = GetRpgStatusEffectState(statuses, statusId);
        return status != null ? Mathf.Max(1, status.AppliedPowerValue) : 0;
    }

    private int ResolveRpgStatusAwareDamage(DungeonPartyMemberRuntimeData actor, DungeonMonsterRuntimeData target, int damage)
    {
        int resolvedDamage = Mathf.Max(1, damage);
        resolvedDamage -= GetRpgStatusPowerValue(actor != null ? actor.StatusEffects : null, PrototypeRpgStatusEffectKeys.Weaken);
        resolvedDamage -= GetRpgStatusPowerValue(target != null ? target.StatusEffects : null, PrototypeRpgStatusEffectKeys.GuardUp);
        resolvedDamage += GetRpgStatusPowerValue(target != null ? target.StatusEffects : null, PrototypeRpgStatusEffectKeys.Mark);
        return Mathf.Max(1, resolvedDamage);
    }

    private int ResolveRpgStatusAwareDamage(DungeonMonsterRuntimeData actor, DungeonPartyMemberRuntimeData target, int damage)
    {
        int resolvedDamage = Mathf.Max(1, damage);
        resolvedDamage -= GetRpgStatusPowerValue(actor != null ? actor.StatusEffects : null, PrototypeRpgStatusEffectKeys.Weaken);
        resolvedDamage -= GetRpgStatusPowerValue(target != null ? target.StatusEffects : null, PrototypeRpgStatusEffectKeys.GuardUp);
        resolvedDamage += GetRpgStatusPowerValue(target != null ? target.StatusEffects : null, PrototypeRpgStatusEffectKeys.Mark);
        return Mathf.Max(1, resolvedDamage);
    }

    private string BuildRpgStatusSourceContextText(DungeonPartyMemberRuntimeData member)
    {
        if (member == null)
        {
            return string.Empty;
        }

        PrototypeRpgPartyMemberLevelRuntimeState memberLevelState = GetRpgMemberLevelRuntimeState(_sessionRpgPartyLevelRuntimeState, member.MemberId);
        return BuildRpgCompactSummaryText(
            member.RoleTag,
            member.ResolvedSkillSummaryText,
            member.SkillLoadoutSummaryText,
            member.EquipmentSummaryText,
            member.GearContributionSummaryText,
            memberLevelState != null ? memberLevelState.JobSpecializationSummaryText : string.Empty,
            memberLevelState != null ? memberLevelState.PassiveSkillSlotSummaryText : string.Empty,
            memberLevelState != null ? memberLevelState.RuntimeSynergySummaryText : string.Empty).ToLowerInvariant();
    }

    private int ResolveRpgStatusDurationTurns(DungeonPartyMemberRuntimeData source, string statusId, int fallbackDuration)
    {
        int duration = Mathf.Max(1, fallbackDuration);
        if (source == null)
        {
            return duration;
        }

        string contextText = BuildRpgStatusSourceContextText(source);
        if ((statusId == PrototypeRpgStatusEffectKeys.GuardUp || statusId == PrototypeRpgStatusEffectKeys.CleanseOnce) && (source.RoleTag == "warrior" || contextText.Contains("guard") || contextText.Contains("ward")))
        {
            duration += 1;
        }
        else if (statusId == PrototypeRpgStatusEffectKeys.Regen && (source.RoleTag == "cleric" || contextText.Contains("restore") || contextText.Contains("sanctuary")))
        {
            duration += 1;
        }
        else if (statusId == PrototypeRpgStatusEffectKeys.Burn && (source.RoleTag == "mage" || contextText.Contains("arcane") || contextText.Contains("ember")))
        {
            duration += 1;
        }
        else if ((statusId == PrototypeRpgStatusEffectKeys.Mark || statusId == PrototypeRpgStatusEffectKeys.Weaken) && (source.RoleTag == "rogue" || contextText.Contains("precision") || contextText.Contains("mark")))
        {
            duration += 1;
        }

        return duration;
    }

    private int ResolveRpgStatusPowerValue(DungeonPartyMemberRuntimeData source, string statusId, int fallbackPower)
    {
        int power = Mathf.Max(1, fallbackPower);
        if (source == null)
        {
            return power;
        }

        string contextText = BuildRpgStatusSourceContextText(source);
        if (statusId == PrototypeRpgStatusEffectKeys.GuardUp && (source.RoleTag == "warrior" || contextText.Contains("guard") || contextText.Contains("bulwark")))
        {
            power += 1;
        }
        else if (statusId == PrototypeRpgStatusEffectKeys.Regen && (source.RoleTag == "cleric" || contextText.Contains("heal") || contextText.Contains("restoration")))
        {
            power += 1;
        }
        else if (statusId == PrototypeRpgStatusEffectKeys.Burn && (source.RoleTag == "mage" || contextText.Contains("arcane") || contextText.Contains("affix")))
        {
            power += 1;
        }
        else if ((statusId == PrototypeRpgStatusEffectKeys.Mark || statusId == PrototypeRpgStatusEffectKeys.Weaken) && (source.RoleTag == "rogue" || contextText.Contains("precision") || contextText.Contains("pressure")))
        {
            power += 1;
        }

        return power;
    }

    private int ResolveRpgMonsterStatusDurationTurns(DungeonMonsterRuntimeData monster, string statusId, int fallbackDuration)
    {
        int duration = Mathf.Max(1, fallbackDuration);
        if (monster != null && monster.IsElite && (statusId == PrototypeRpgStatusEffectKeys.Burn || statusId == PrototypeRpgStatusEffectKeys.Weaken))
        {
            duration += 1;
        }

        return duration;
    }

    private int ResolveRpgMonsterStatusPowerValue(DungeonMonsterRuntimeData monster, string statusId, int fallbackPower)
    {
        int power = Mathf.Max(1, fallbackPower);
        if (monster != null && monster.IsElite && (statusId == PrototypeRpgStatusEffectKeys.Burn || statusId == PrototypeRpgStatusEffectKeys.Weaken))
        {
            power += 1;
        }

        return power;
    }

    private void TrackRpgStatusApplicationBySource(string sourceEntityId)
    {
        int memberIndex = GetRpgPartyMemberIndexById(sourceEntityId);
        if (memberIndex < 0)
        {
            return;
        }

        AddRunMemberContributionValue(_runMemberStatusApplied, memberIndex, 1);
        DungeonPartyMemberRuntimeData member = GetPartyMemberAtIndex(memberIndex);
        if (member != null)
        {
            member.StatusApplicationsThisEncounter += 1;
        }
    }

    private void TrackRpgStatusTickBySource(string sourceEntityId)
    {
        int memberIndex = GetRpgPartyMemberIndexById(sourceEntityId);
        if (memberIndex < 0)
        {
            return;
        }

        AddRunMemberContributionValue(_runMemberStatusTicksResolved, memberIndex, 1);
        DungeonPartyMemberRuntimeData member = GetPartyMemberAtIndex(memberIndex);
        if (member != null)
        {
            member.StatusTicksResolvedThisEncounter += 1;
        }
    }

    private int GetRpgPartyMemberIndexById(string memberId)
    {
        if (_activeDungeonParty == null || _activeDungeonParty.Members == null || string.IsNullOrEmpty(memberId))
        {
            return -1;
        }

        for (int i = 0; i < _activeDungeonParty.Members.Length; i++)
        {
            DungeonPartyMemberRuntimeData member = _activeDungeonParty.Members[i];
            if (member != null && member.MemberId == memberId)
            {
                return i;
            }
        }

        return -1;
    }

    private bool TryConsumeRpgCleanseOnce(ref PrototypeRpgStatusRuntimeState[] statuses, string sourceEntityId, string targetEntityId, string targetName, string incomingStatusId)
    {
        PrototypeRpgStatusEffectDefinition incomingDefinition = PrototypeRpgStatusEffectCatalog.GetDefinition(incomingStatusId);
        if (incomingDefinition == null || !incomingDefinition.IsDebuff)
        {
            return false;
        }

        List<PrototypeRpgStatusRuntimeState> remaining = new List<PrototypeRpgStatusRuntimeState>();
        bool consumed = false;
        for (int i = 0; i < (statuses ?? Array.Empty<PrototypeRpgStatusRuntimeState>()).Length; i++)
        {
            PrototypeRpgStatusRuntimeState status = statuses[i];
            if (!consumed && status != null && status.StatusId == PrototypeRpgStatusEffectKeys.CleanseOnce && status.RemainingTurns > 0)
            {
                consumed = true;
                continue;
            }

            if (status != null)
            {
                remaining.Add(status);
            }
        }

        if (!consumed)
        {
            return false;
        }

        statuses = remaining.Count > 0 ? remaining.ToArray() : Array.Empty<PrototypeRpgStatusRuntimeState>();
        string statusLabel = string.IsNullOrEmpty(incomingDefinition.DisplayLabel) ? incomingStatusId : incomingDefinition.DisplayLabel;
        string summaryText = targetName + " blocked " + statusLabel + ".";
        RecordBattleEvent(
            PrototypeBattleEventKeys.StatusCleansed,
            sourceEntityId,
            targetEntityId,
            0,
            summaryText,
            actionKey: "status",
            phaseKey: "resolution",
            actorName: targetName,
            targetName: targetName,
            shortText: "Cleanse");
        AppendBattleLog(summaryText);
        return true;
    }

    private PrototypeRpgStatusRuntimeState[] ApplyOrRefreshRpgStatusArray(
        PrototypeRpgStatusRuntimeState[] statuses,
        PrototypeRpgStatusEffectDefinition definition,
        string sourceEntityId,
        string targetEntityId,
        int remainingTurns,
        int appliedPowerValue,
        int stackCount,
        out PrototypeRpgStatusRuntimeState resolvedState,
        out bool refreshedExisting)
    {
        List<PrototypeRpgStatusRuntimeState> next = new List<PrototypeRpgStatusRuntimeState>(statuses ?? Array.Empty<PrototypeRpgStatusRuntimeState>());
        refreshedExisting = false;
        resolvedState = null;
        for (int i = 0; i < next.Count; i++)
        {
            PrototypeRpgStatusRuntimeState current = next[i];
            if (current == null || current.StatusId != definition.StatusId)
            {
                continue;
            }

            current.SourceEntityId = string.IsNullOrEmpty(sourceEntityId) ? current.SourceEntityId : sourceEntityId;
            current.TargetEntityId = string.IsNullOrEmpty(targetEntityId) ? current.TargetEntityId : targetEntityId;
            current.RemainingTurns = Mathf.Max(current.RemainingTurns, remainingTurns);
            current.StackCount = Mathf.Clamp(current.StackCount + Mathf.Max(0, stackCount - 1), 1, Mathf.Max(1, definition.MaxStacks));
            current.AppliedPowerValue = Mathf.Max(current.AppliedPowerValue, appliedPowerValue);
            current.SummaryText = definition.SummaryText;
            resolvedState = current;
            refreshedExisting = true;
            return next.ToArray();
        }

        PrototypeRpgStatusRuntimeState created = new PrototypeRpgStatusRuntimeState();
        created.StatusId = definition.StatusId;
        created.SourceEntityId = string.IsNullOrEmpty(sourceEntityId) ? string.Empty : sourceEntityId;
        created.TargetEntityId = string.IsNullOrEmpty(targetEntityId) ? string.Empty : targetEntityId;
        created.RemainingTurns = Mathf.Max(1, remainingTurns);
        created.StackCount = Mathf.Clamp(Mathf.Max(1, stackCount), 1, Mathf.Max(1, definition.MaxStacks));
        created.AppliedPowerValue = Mathf.Max(1, appliedPowerValue);
        created.DisplayLabel = definition.DisplayLabel;
        created.ShortLabel = definition.ShortLabel;
        created.SummaryText = definition.SummaryText;
        created.IsBuff = definition.IsBuff;
        created.IsDebuff = definition.IsDebuff;
        created.TicksAtTurnStart = definition.TicksAtTurnStart;
        next.Add(created);
        resolvedState = created;
        return next.ToArray();
    }

    private bool ApplyRpgStatusEffectToPartyMember(DungeonPartyMemberRuntimeData source, DungeonPartyMemberRuntimeData target, string statusId, int durationTurns, int appliedPowerValue, int stackCount = 1)
    {
        if (target == null || target.IsDefeated || target.CurrentHp <= 0)
        {
            return false;
        }

        PrototypeRpgStatusEffectDefinition definition = PrototypeRpgStatusEffectCatalog.GetDefinition(statusId);
        if (definition == null)
        {
            return false;
        }

        PrototypeRpgStatusRuntimeState[] statuses = target.StatusEffects ?? Array.Empty<PrototypeRpgStatusRuntimeState>();
        if (TryConsumeRpgCleanseOnce(ref statuses, source != null ? source.MemberId : string.Empty, target.MemberId, target.DisplayName, statusId))
        {
            target.StatusEffects = statuses;
            RefreshRpgStatusSummaryText(target);
            return false;
        }

        int resolvedDuration = ResolveRpgStatusDurationTurns(source, statusId, durationTurns > 0 ? durationTurns : definition.DefaultDurationTurns);
        int resolvedPower = ResolveRpgStatusPowerValue(source, statusId, appliedPowerValue > 0 ? appliedPowerValue : definition.DefaultPowerValue);
        bool refreshedExisting;
        PrototypeRpgStatusRuntimeState appliedState;
        target.StatusEffects = ApplyOrRefreshRpgStatusArray(
            statuses,
            definition,
            source != null ? source.MemberId : string.Empty,
            target.MemberId,
            resolvedDuration,
            resolvedPower,
            stackCount,
            out appliedState,
            out refreshedExisting);
        RefreshRpgStatusSummaryText(target);
        TrackRpgStatusApplicationBySource(source != null ? source.MemberId : string.Empty);
        string summaryText = target.DisplayName + " gained " + definition.DisplayLabel + " " + appliedState.RemainingTurns + "t.";
        RecordBattleEvent(
            PrototypeBattleEventKeys.StatusApplied,
            source != null ? source.MemberId : string.Empty,
            target.MemberId,
            resolvedPower,
            summaryText,
            actionKey: "status",
            phaseKey: "resolution",
            actorName: source != null ? source.DisplayName : target.DisplayName,
            targetName: target.DisplayName,
            shortText: refreshedExisting ? definition.ShortLabel + " refresh" : definition.ShortLabel);
        AppendBattleLog(summaryText);
        return true;
    }

    private bool ApplyRpgStatusEffectToMonster(DungeonPartyMemberRuntimeData source, DungeonMonsterRuntimeData target, string statusId, int durationTurns, int appliedPowerValue, int stackCount = 1)
    {
        if (target == null || target.IsDefeated || target.CurrentHp <= 0)
        {
            return false;
        }

        PrototypeRpgStatusEffectDefinition definition = PrototypeRpgStatusEffectCatalog.GetDefinition(statusId);
        if (definition == null)
        {
            return false;
        }

        PrototypeRpgStatusRuntimeState[] statuses = target.StatusEffects ?? Array.Empty<PrototypeRpgStatusRuntimeState>();
        int resolvedDuration = ResolveRpgStatusDurationTurns(source, statusId, durationTurns > 0 ? durationTurns : definition.DefaultDurationTurns);
        int resolvedPower = ResolveRpgStatusPowerValue(source, statusId, appliedPowerValue > 0 ? appliedPowerValue : definition.DefaultPowerValue);
        bool refreshedExisting;
        PrototypeRpgStatusRuntimeState appliedState;
        target.StatusEffects = ApplyOrRefreshRpgStatusArray(
            statuses,
            definition,
            source != null ? source.MemberId : string.Empty,
            target.MonsterId,
            resolvedDuration,
            resolvedPower,
            stackCount,
            out appliedState,
            out refreshedExisting);
        RefreshRpgStatusSummaryText(target);
        TrackRpgStatusApplicationBySource(source != null ? source.MemberId : string.Empty);
        string summaryText = target.DisplayName + " gained " + definition.DisplayLabel + " " + appliedState.RemainingTurns + "t.";
        RecordBattleEvent(
            PrototypeBattleEventKeys.StatusApplied,
            source != null ? source.MemberId : string.Empty,
            target.MonsterId,
            resolvedPower,
            summaryText,
            actionKey: "status",
            phaseKey: "resolution",
            actorName: source != null ? source.DisplayName : target.DisplayName,
            targetName: target.DisplayName,
            shortText: refreshedExisting ? definition.ShortLabel + " refresh" : definition.ShortLabel);
        AppendBattleLog(summaryText);
        return true;
    }

    private bool ApplyRpgStatusEffectFromMonster(DungeonMonsterRuntimeData source, DungeonPartyMemberRuntimeData target, string statusId, int durationTurns, int appliedPowerValue, int stackCount = 1)
    {
        if (target == null || target.IsDefeated || target.CurrentHp <= 0)
        {
            return false;
        }

        PrototypeRpgStatusEffectDefinition definition = PrototypeRpgStatusEffectCatalog.GetDefinition(statusId);
        if (definition == null)
        {
            return false;
        }

        PrototypeRpgStatusRuntimeState[] statuses = target.StatusEffects ?? Array.Empty<PrototypeRpgStatusRuntimeState>();
        if (TryConsumeRpgCleanseOnce(ref statuses, source != null ? source.MonsterId : string.Empty, target.MemberId, target.DisplayName, statusId))
        {
            target.StatusEffects = statuses;
            RefreshRpgStatusSummaryText(target);
            return false;
        }

        int resolvedDuration = ResolveRpgMonsterStatusDurationTurns(source, statusId, durationTurns > 0 ? durationTurns : definition.DefaultDurationTurns);
        int resolvedPower = ResolveRpgMonsterStatusPowerValue(source, statusId, appliedPowerValue > 0 ? appliedPowerValue : definition.DefaultPowerValue);
        bool refreshedExisting;
        PrototypeRpgStatusRuntimeState appliedState;
        target.StatusEffects = ApplyOrRefreshRpgStatusArray(
            statuses,
            definition,
            source != null ? source.MonsterId : string.Empty,
            target.MemberId,
            resolvedDuration,
            resolvedPower,
            stackCount,
            out appliedState,
            out refreshedExisting);
        RefreshRpgStatusSummaryText(target);
        string summaryText = target.DisplayName + " gained " + definition.DisplayLabel + " " + appliedState.RemainingTurns + "t.";
        RecordBattleEvent(
            PrototypeBattleEventKeys.StatusApplied,
            source != null ? source.MonsterId : string.Empty,
            target.MemberId,
            resolvedPower,
            summaryText,
            actionKey: "status",
            phaseKey: "enemy_turn",
            actorName: source != null ? source.DisplayName : target.DisplayName,
            targetName: target.DisplayName,
            shortText: refreshedExisting ? definition.ShortLabel + " refresh" : definition.ShortLabel);
        AppendBattleLog(summaryText);
        return true;
    }

    private int ApplyRpgStatusTickDamageToMonster(DungeonMonsterRuntimeData monster, PrototypeRpgStatusRuntimeState status)
    {
        if (monster == null || status == null || monster.IsDefeated || monster.CurrentHp <= 0)
        {
            return 0;
        }

        bool wasDefeated = monster.IsDefeated;
        int safeDamage = Mathf.Max(1, status.AppliedPowerValue);
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
        int sourceMemberIndex = GetRpgPartyMemberIndexById(status.SourceEntityId);
        if (sourceMemberIndex >= 0)
        {
            AddRunMemberContributionValue(_runMemberDamageDealt, sourceMemberIndex, applied);
        }

        FlashMonster(monster, new Color(1f, 0.43f, 0.24f, 1f));
        ShowBattlePopupForMonster(monster, "-" + applied, new Color(1f, 0.43f, 0.24f, 1f));
        string sourceName = GetCombatEntityDisplayName(status.SourceEntityId);
        string summaryText = monster.DisplayName + " suffered " + applied + " " + status.ShortLabel.ToLowerInvariant() + " damage.";
        RecordBattleEvent(
            PrototypeBattleEventKeys.StatusTick,
            status.SourceEntityId,
            monster.MonsterId,
            applied,
            summaryText,
            actionKey: "status",
            phaseKey: "status_tick",
            actorName: sourceName,
            targetName: monster.DisplayName,
            shortText: status.ShortLabel + " -" + applied);
        AppendBattleLog(summaryText);
        ResolveMonsterDefeat(monster, wasDefeated);
        if (!wasDefeated && monster.IsDefeated)
        {
            if (sourceMemberIndex >= 0)
            {
                AddRunMemberContributionValue(_runMemberKillCount, sourceMemberIndex, 1);
            }
        }

        TrackRpgStatusTickBySource(status.SourceEntityId);
        return applied;
    }

    private int ApplyRpgStatusTickDamageToPartyMember(DungeonPartyMemberRuntimeData member, PrototypeRpgStatusRuntimeState status)
    {
        if (member == null || status == null || member.IsDefeated || member.CurrentHp <= 0)
        {
            return 0;
        }

        bool wasKnockedOut = member.IsDefeated;
        int safeDamage = Mathf.Max(1, status.AppliedPowerValue);
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
        AddRunMemberContributionValue(_runMemberDamageTaken, member.PartySlotIndex, applied);
        FlashPartyMember(member.PartySlotIndex, new Color(1f, 0.43f, 0.24f, 1f));
        ShowBattlePopupForPartyMember(member.PartySlotIndex, "-" + applied, new Color(1f, 0.43f, 0.24f, 1f));
        string sourceName = GetCombatEntityDisplayName(status.SourceEntityId);
        string summaryText = member.DisplayName + " suffered " + applied + " " + status.ShortLabel.ToLowerInvariant() + " damage.";
        RecordBattleEvent(
            PrototypeBattleEventKeys.StatusTick,
            status.SourceEntityId,
            member.MemberId,
            applied,
            summaryText,
            actionKey: "status",
            phaseKey: "status_tick",
            actorName: sourceName,
            targetName: member.DisplayName,
            shortText: status.ShortLabel + " -" + applied);
        AppendBattleLog(summaryText);
        ResolvePartyMemberKnockOut(member, wasKnockedOut);
        TrackRpgStatusTickBySource(status.SourceEntityId);
        return applied;
    }

    private int ApplyRpgStatusTickHealToPartyMember(DungeonPartyMemberRuntimeData member, PrototypeRpgStatusRuntimeState status)
    {
        if (member == null || status == null || member.IsDefeated || member.CurrentHp <= 0)
        {
            return 0;
        }

        int safeRecover = Mathf.Max(1, status.AppliedPowerValue);
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
        int sourceMemberIndex = GetRpgPartyMemberIndexById(status.SourceEntityId);
        if (sourceMemberIndex >= 0)
        {
            AddRunMemberContributionValue(_runMemberHealingDone, sourceMemberIndex, recovered);
        }

        FlashPartyMember(member.PartySlotIndex, new Color(0.56f, 1f, 0.68f, 1f));
        ShowBattlePopupForPartyMember(member.PartySlotIndex, "+" + recovered, new Color(0.56f, 1f, 0.68f, 1f));
        string sourceName = GetCombatEntityDisplayName(status.SourceEntityId);
        string summaryText = member.DisplayName + " recovered " + recovered + " HP from " + status.ShortLabel.ToLowerInvariant() + ".";
        RecordBattleEvent(
            PrototypeBattleEventKeys.StatusTick,
            status.SourceEntityId,
            member.MemberId,
            recovered,
            summaryText,
            actionKey: "status",
            phaseKey: "status_tick",
            actorName: sourceName,
            targetName: member.DisplayName,
            shortText: status.ShortLabel + " +" + recovered);
        AppendBattleLog(summaryText);
        TrackRpgStatusTickBySource(status.SourceEntityId);
        return recovered;
    }

    private void AdvanceRpgStatusTurnStart(DungeonPartyMemberRuntimeData member)
    {
        if (member == null || member.IsDefeated || member.CurrentHp <= 0)
        {
            return;
        }

        PrototypeRpgStatusRuntimeState[] statuses = member.StatusEffects ?? Array.Empty<PrototypeRpgStatusRuntimeState>();
        if (statuses.Length <= 0)
        {
            member.StatusEffectSummaryText = string.Empty;
            return;
        }

        List<PrototypeRpgStatusRuntimeState> remaining = new List<PrototypeRpgStatusRuntimeState>();
        for (int i = 0; i < statuses.Length; i++)
        {
            PrototypeRpgStatusRuntimeState status = statuses[i];
            if (status == null || string.IsNullOrEmpty(status.StatusId) || status.RemainingTurns <= 0)
            {
                continue;
            }

            if (status.StatusId == PrototypeRpgStatusEffectKeys.Burn)
            {
                ApplyRpgStatusTickDamageToPartyMember(member, status);
            }
            else if (status.StatusId == PrototypeRpgStatusEffectKeys.Regen)
            {
                ApplyRpgStatusTickHealToPartyMember(member, status);
            }

            status.RemainingTurns = Mathf.Max(0, status.RemainingTurns - 1);
            if (status.RemainingTurns > 0)
            {
                remaining.Add(status);
            }
            else
            {
                string summaryText = member.DisplayName + " lost " + status.DisplayLabel + ".";
                RecordBattleEvent(
                    PrototypeBattleEventKeys.StatusExpired,
                    status.SourceEntityId,
                    member.MemberId,
                    0,
                    summaryText,
                    actionKey: "status",
                    phaseKey: "status_tick",
                    actorName: member.DisplayName,
                    targetName: member.DisplayName,
                    shortText: status.ShortLabel + " end");
                AppendBattleLog(summaryText);
            }
        }

        member.StatusEffects = remaining.Count > 0 ? remaining.ToArray() : Array.Empty<PrototypeRpgStatusRuntimeState>();
        RefreshRpgStatusSummaryText(member);
    }

    private void AdvanceRpgStatusTurnStart(DungeonMonsterRuntimeData monster)
    {
        if (monster == null || monster.IsDefeated || monster.CurrentHp <= 0)
        {
            return;
        }

        PrototypeRpgStatusRuntimeState[] statuses = monster.StatusEffects ?? Array.Empty<PrototypeRpgStatusRuntimeState>();
        if (statuses.Length <= 0)
        {
            monster.StatusEffectSummaryText = string.Empty;
            return;
        }

        List<PrototypeRpgStatusRuntimeState> remaining = new List<PrototypeRpgStatusRuntimeState>();
        for (int i = 0; i < statuses.Length; i++)
        {
            PrototypeRpgStatusRuntimeState status = statuses[i];
            if (status == null || string.IsNullOrEmpty(status.StatusId) || status.RemainingTurns <= 0)
            {
                continue;
            }

            if (status.StatusId == PrototypeRpgStatusEffectKeys.Burn)
            {
                ApplyRpgStatusTickDamageToMonster(monster, status);
            }

            status.RemainingTurns = Mathf.Max(0, status.RemainingTurns - 1);
            if (status.RemainingTurns > 0)
            {
                remaining.Add(status);
            }
            else
            {
                string summaryText = monster.DisplayName + " lost " + status.DisplayLabel + ".";
                RecordBattleEvent(
                    PrototypeBattleEventKeys.StatusExpired,
                    status.SourceEntityId,
                    monster.MonsterId,
                    0,
                    summaryText,
                    actionKey: "status",
                    phaseKey: "status_tick",
                    actorName: monster.DisplayName,
                    targetName: monster.DisplayName,
                    shortText: status.ShortLabel + " end");
                AppendBattleLog(summaryText);
            }
        }

        monster.StatusEffects = remaining.Count > 0 ? remaining.ToArray() : Array.Empty<PrototypeRpgStatusRuntimeState>();
        RefreshRpgStatusSummaryText(monster);
    }

    private void ApplyRpgStatusesForResolvedPartySkill(DungeonPartyMemberRuntimeData actor, PrototypeRpgSkillUseContext context, DungeonMonsterRuntimeData targetMonster, List<DungeonMonsterRuntimeData> targetMonsters, List<int> targetPartyIndices)
    {
        if (actor == null || context == null)
        {
            return;
        }

        string skillId = string.IsNullOrEmpty(context.ResolvedSkillId) ? actor.DefaultSkillId : context.ResolvedSkillId;
        if (skillId == "skill_arcane_burst")
        {
            List<DungeonMonsterRuntimeData> safeTargets = targetMonsters ?? new List<DungeonMonsterRuntimeData>();
            for (int i = 0; i < safeTargets.Count; i++)
            {
                ApplyRpgStatusEffectToMonster(actor, safeTargets[i], PrototypeRpgStatusEffectKeys.Burn, 2, 1);
            }

            return;
        }

        if (skillId == "skill_radiant_hymn")
        {
            List<int> safeIndices = targetPartyIndices ?? new List<int>();
            for (int i = 0; i < safeIndices.Count; i++)
            {
                DungeonPartyMemberRuntimeData ally = GetPartyMemberAtIndex(safeIndices[i]);
                ApplyRpgStatusEffectToPartyMember(actor, ally, PrototypeRpgStatusEffectKeys.Regen, 2, 1);
            }

            return;
        }

        if (skillId == "skill_restoration_ward")
        {
            List<int> safeIndices = targetPartyIndices ?? new List<int>();
            for (int i = 0; i < safeIndices.Count; i++)
            {
                DungeonPartyMemberRuntimeData ally = GetPartyMemberAtIndex(safeIndices[i]);
                ApplyRpgStatusEffectToPartyMember(actor, ally, PrototypeRpgStatusEffectKeys.GuardUp, 2, 1);
                ApplyRpgStatusEffectToPartyMember(actor, ally, PrototypeRpgStatusEffectKeys.Regen, 2, 1);
                ApplyRpgStatusEffectToPartyMember(actor, ally, PrototypeRpgStatusEffectKeys.CleanseOnce, 2, 1);
            }

            return;
        }

        if (skillId == "skill_guarding_stance" || context.ApplyModeKey == "self_recover_or_guarded_strike")
        {
            ApplyRpgStatusEffectToPartyMember(actor, actor, PrototypeRpgStatusEffectKeys.GuardUp, 2, 1);
            return;
        }

        if (skillId == "skill_exploit_mark" || context.EffectType == "setup")
        {
            ApplyRpgStatusEffectToMonster(actor, targetMonster, PrototypeRpgStatusEffectKeys.Mark, 2, 1);
            ApplyRpgStatusEffectToMonster(actor, targetMonster, PrototypeRpgStatusEffectKeys.Weaken, 2, 1);
        }
    }

    private void ApplyRpgStatusesForEnemyAction(DungeonMonsterRuntimeData actor, DungeonPartyMemberRuntimeData targetMember, List<int> targetPartyIndices, bool usedSpecial)
    {
        if (actor == null || !usedSpecial)
        {
            return;
        }

        if (targetPartyIndices != null && targetPartyIndices.Count > 1)
        {
            for (int i = 0; i < targetPartyIndices.Count; i++)
            {
                DungeonPartyMemberRuntimeData member = GetPartyMemberAtIndex(targetPartyIndices[i]);
                ApplyRpgStatusEffectFromMonster(actor, member, PrototypeRpgStatusEffectKeys.Burn, 2, 1);
            }

            return;
        }

        ApplyRpgStatusEffectFromMonster(actor, targetMember, PrototypeRpgStatusEffectKeys.Weaken, 2, 1);
    }
}
