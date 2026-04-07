using System;
using System.Collections.Generic;
using UnityEngine;

public sealed partial class StaticPlaceholderWorldView
{
    public string CurrentEnemySkillLoadoutSummaryText => _runResultState != RunResultState.None
        ? ResolveRpgEnemySkillSurfaceText(_lastResolvedEnemySkillUsageSummaryText, BuildCurrentEnemySkillLoadoutSummaryText())
        : ResolveRpgEnemySkillSurfaceText(BuildCurrentEnemySkillLoadoutSummaryText(), _lastResolvedEnemySkillUsageSummaryText);
    public string CurrentEnemyActionEconomySummaryText => _runResultState != RunResultState.None
        ? ResolveRpgEnemySkillSurfaceText(_lastResolvedEnemyActionEconomySummaryText, BuildCurrentEnemyActionEconomySummaryText())
        : ResolveRpgEnemySkillSurfaceText(BuildCurrentEnemyActionEconomySummaryText(), _lastResolvedEnemyActionEconomySummaryText);
    public string CurrentEnemyIntentExpansionSummaryText => _runResultState != RunResultState.None
        ? ResolveRpgEnemySkillSurfaceText(_lastResolvedEnemyIntentSummaryText, BuildCurrentEnemyIntentExpansionSummaryText())
        : ResolveRpgEnemySkillSurfaceText(BuildCurrentEnemyIntentExpansionSummaryText(), _lastResolvedEnemyIntentSummaryText);

    private string ResolveRpgEnemySkillSurfaceText(string liveText, string capturedText)
    {
        string resolved = !string.IsNullOrEmpty(liveText) ? liveText : capturedText;
        return string.IsNullOrEmpty(resolved) ? "None" : resolved;
    }

    private void ResetRpgEncounterEnemySkillRuntime(bool clearReadback)
    {
        for (int i = 0; i < _activeMonsters.Count; i++)
        {
            DungeonMonsterRuntimeData monster = _activeMonsters[i];
            if (monster == null)
            {
                continue;
            }

            monster.ActiveSkillSlots = Array.Empty<PrototypeRpgEnemySkillRuntimeState>();
            monster.SelectedSkillSlotIndex = -1;
            monster.SkillLoadoutSummaryText = string.Empty;
            monster.SkillCooldownSummaryText = string.Empty;
            monster.ActionEconomySummaryText = string.Empty;
            monster.LastResolvedSkillId = string.Empty;
            monster.LastResolvedSkillLabel = string.Empty;
            monster.SkillUsesThisEncounter = 0;
        }

        if (clearReadback)
        {
            _lastResolvedEnemyIntentSummaryText = string.Empty;
            _lastResolvedEnemySkillUsageSummaryText = string.Empty;
            _lastResolvedEnemyActionEconomySummaryText = string.Empty;
        }
    }

    private void CaptureRpgEncounterEnemySkillReadback()
    {
        _lastResolvedEnemyIntentSummaryText = BuildCurrentEnemyIntentExpansionSummaryText();
        _lastResolvedEnemySkillUsageSummaryText = BuildCurrentEnemySkillLoadoutSummaryText();
        _lastResolvedEnemyActionEconomySummaryText = BuildCurrentEnemyActionEconomySummaryText();
    }

    private PrototypeRpgEnemySkillRuntimeState[] BuildResolvedRpgEnemySkillStates(DungeonMonsterRuntimeData monster)
    {
        if (monster == null)
        {
            return Array.Empty<PrototypeRpgEnemySkillRuntimeState>();
        }

        PrototypeRpgEnemySkillLoadoutDefinition loadout = PrototypeRpgEnemySkillCatalog.ResolveLoadout(monster.MonsterType, monster.EncounterRole.ToString(), monster.IsElite);
        if (loadout == null || loadout.SkillIds == null || loadout.SkillIds.Length <= 0)
        {
            return Array.Empty<PrototypeRpgEnemySkillRuntimeState>();
        }

        List<PrototypeRpgEnemySkillRuntimeState> skills = new List<PrototypeRpgEnemySkillRuntimeState>();
        for (int i = 0; i < loadout.SkillIds.Length; i++)
        {
            PrototypeRpgEnemySkillDefinition definition = PrototypeRpgEnemySkillCatalog.GetSkillDefinition(loadout.SkillIds[i]);
            if (definition == null)
            {
                continue;
            }

            PrototypeRpgEnemySkillRuntimeState runtime = new PrototypeRpgEnemySkillRuntimeState();
            runtime.SkillId = definition.SkillId;
            runtime.DisplayName = definition.DisplayName;
            runtime.TargetKind = definition.TargetKind;
            runtime.EffectType = definition.EffectType;
            runtime.PowerValue = ResolveEnemyActionPower(monster, definition);
            runtime.CooldownTurns = Mathf.Max(0, definition.CooldownTurns);
            runtime.CooldownRemaining = 0;
            runtime.MaxEncounterCharges = Mathf.Max(0, definition.EncounterCharges);
            runtime.EncounterChargesRemaining = definition.EncounterCharges > 0 ? definition.EncounterCharges : 0;
            runtime.IntentLabel = definition.IntentLabel;
            runtime.ActionBiasKey = definition.ActionBiasKey;
            runtime.PredictedStatusText = definition.PredictedStatusText;
            runtime.SummaryText = BuildRpgCompactSummaryText(definition.DisplayName, definition.SummaryText);
            runtime.IsAvailable = true;
            runtime.AvailabilitySummaryText = "Ready";
            skills.Add(runtime);
        }

        monster.SkillLoadoutSummaryText = loadout.DisplayName + " | " + loadout.SummaryText;
        return skills.ToArray();
    }

    private int ResolveEnemyActionPower(DungeonMonsterRuntimeData monster, PrototypeRpgEnemySkillDefinition definition)
    {
        if (monster == null || definition == null)
        {
            return 1;
        }

        if (definition.ActionBiasKey == "basic_attack")
        {
            return Mathf.Max(1, monster.Attack);
        }

        if (definition.ActionBiasKey == "finisher")
        {
            return Mathf.Max(monster.SpecialAttack, definition.PowerValue);
        }

        if (definition.ActionBiasKey == "aoe")
        {
            return Mathf.Max(definition.PowerValue, monster.Attack + (monster.IsElite ? 2 : 1));
        }

        if (definition.ActionBiasKey == "debuff")
        {
            return Mathf.Max(definition.PowerValue, monster.Attack + 1);
        }

        if (definition.ActionBiasKey == "self_buff" || definition.ActionBiasKey == "cleanse")
        {
            return Mathf.Max(0, definition.PowerValue);
        }

        return Mathf.Max(1, definition.PowerValue);
    }

    private void InitializeRpgEnemySkillRuntimeState(DungeonMonsterRuntimeData monster, bool forceReset)
    {
        if (monster == null)
        {
            return;
        }

        PrototypeRpgEnemySkillRuntimeState[] resolved = BuildResolvedRpgEnemySkillStates(monster);
        PrototypeRpgEnemySkillRuntimeState[] current = monster.ActiveSkillSlots ?? Array.Empty<PrototypeRpgEnemySkillRuntimeState>();
        bool needsReset = forceReset || current.Length != resolved.Length;
        if (!needsReset)
        {
            for (int i = 0; i < current.Length; i++)
            {
                PrototypeRpgEnemySkillRuntimeState currentSkill = current[i];
                PrototypeRpgEnemySkillRuntimeState resolvedSkill = resolved[i];
                if (currentSkill == null || resolvedSkill == null || currentSkill.SkillId != resolvedSkill.SkillId)
                {
                    needsReset = true;
                    break;
                }
            }
        }

        if (needsReset)
        {
            monster.ActiveSkillSlots = resolved;
            monster.SelectedSkillSlotIndex = resolved.Length > 0 ? 0 : -1;
        }

        RefreshRpgEnemySkillRuntimeState(monster);
    }

    private void RefreshRpgEnemySkillRuntimeState(DungeonMonsterRuntimeData monster)
    {
        if (monster == null)
        {
            return;
        }

        PrototypeRpgEnemySkillRuntimeState[] skills = monster.ActiveSkillSlots ?? Array.Empty<PrototypeRpgEnemySkillRuntimeState>();
        List<string> cooldownParts = new List<string>();
        for (int i = 0; i < skills.Length; i++)
        {
            PrototypeRpgEnemySkillRuntimeState skill = skills[i];
            if (skill == null)
            {
                continue;
            }

            bool hasCharges = skill.MaxEncounterCharges <= 0 || skill.EncounterChargesRemaining > 0;
            bool offCooldown = skill.CooldownRemaining <= 0;
            skill.IsAvailable = hasCharges && offCooldown;
            skill.AvailabilitySummaryText = skill.IsAvailable
                ? skill.MaxEncounterCharges > 0
                    ? "Ready | Uses " + skill.EncounterChargesRemaining + "/" + skill.MaxEncounterCharges
                    : "Ready"
                : skill.CooldownRemaining > 0
                    ? "Cooldown " + skill.CooldownRemaining
                    : "Encounter charge spent";
            cooldownParts.Add(skill.DisplayName + " " + (skill.CooldownRemaining > 0 ? "CD " + skill.CooldownRemaining : "ready"));
        }

        if (skills.Length <= 0)
        {
            monster.SelectedSkillSlotIndex = -1;
        }
        else if (monster.SelectedSkillSlotIndex < 0 || monster.SelectedSkillSlotIndex >= skills.Length)
        {
            monster.SelectedSkillSlotIndex = 0;
        }

        monster.SkillCooldownSummaryText = cooldownParts.Count > 0 ? "Enemy cooldowns: " + string.Join(" | ", cooldownParts.ToArray()) : string.Empty;
        monster.ActionEconomySummaryText = BuildRpgCompactSummaryText("Enemy economy: 1 action per enemy turn", monster.SkillCooldownSummaryText);
    }

    private void AdvanceRpgEnemySkillTurnStart(DungeonMonsterRuntimeData monster)
    {
        InitializeRpgEnemySkillRuntimeState(monster, false);
        PrototypeRpgEnemySkillRuntimeState[] skills = monster != null ? (monster.ActiveSkillSlots ?? Array.Empty<PrototypeRpgEnemySkillRuntimeState>()) : Array.Empty<PrototypeRpgEnemySkillRuntimeState>();
        for (int i = 0; i < skills.Length; i++)
        {
            PrototypeRpgEnemySkillRuntimeState skill = skills[i];
            if (skill != null && skill.CooldownRemaining > 0)
            {
                skill.CooldownRemaining = Mathf.Max(0, skill.CooldownRemaining - 1);
            }
        }

        RefreshRpgEnemySkillRuntimeState(monster);
    }

    private PrototypeRpgEnemySkillRuntimeState ResolveEnemyAction(DungeonMonsterRuntimeData monster, int targetIndex)
    {
        if (monster == null)
        {
            return null;
        }

        InitializeRpgEnemySkillRuntimeState(monster, false);
        return SelectEnemySkillAction(monster, targetIndex);
    }

    private PrototypeRpgEnemySkillRuntimeState SelectEnemySkillAction(DungeonMonsterRuntimeData monster, int targetIndex)
    {
        if (monster == null)
        {
            return null;
        }

        PrototypeRpgEnemySkillRuntimeState[] skills = monster.ActiveSkillSlots ?? Array.Empty<PrototypeRpgEnemySkillRuntimeState>();
        if (skills.Length <= 0)
        {
            return null;
        }

        DungeonPartyMemberRuntimeData target = GetPartyMemberAtIndex(targetIndex);
        bool preferSkillWindow = ShouldUseEliteSpecialAttack(monster);
        int livingAllies = GetLivingAllies().Count;
        int bestIndex = -1;
        int bestScore = int.MinValue;
        for (int i = 0; i < skills.Length; i++)
        {
            PrototypeRpgEnemySkillRuntimeState skill = skills[i];
            if (skill == null || !skill.IsAvailable)
            {
                continue;
            }

            int score = EvaluateEnemyActionPolicy(monster, skill, target, livingAllies, preferSkillWindow);
            if (score > bestScore)
            {
                bestScore = score;
                bestIndex = i;
            }
        }

        if (bestIndex < 0)
        {
            bestIndex = 0;
        }

        monster.SelectedSkillSlotIndex = bestIndex;
        return bestIndex >= 0 && bestIndex < skills.Length ? skills[bestIndex] : null;
    }

    private int EvaluateEnemyActionPolicy(DungeonMonsterRuntimeData monster, PrototypeRpgEnemySkillRuntimeState skill, DungeonPartyMemberRuntimeData target, int livingAllies, bool preferSkillWindow)
    {
        if (skill == null)
        {
            return int.MinValue;
        }

        bool monsterNeedsRecovery = monster != null && monster.CurrentHp <= Mathf.Max(1, monster.MaxHp / 2);
        bool monsterHasDebuff = HasAnyRpgDebuffStatus(monster != null ? monster.StatusEffects : null);
        bool targetIsWeakened = HasRpgStatus(target != null ? target.StatusEffects : null, PrototypeRpgStatusEffectKeys.Weaken);
        bool targetIsMarked = HasRpgStatus(target != null ? target.StatusEffects : null, PrototypeRpgStatusEffectKeys.Mark);
        bool monsterGuarded = HasRpgStatus(monster != null ? monster.StatusEffects : null, PrototypeRpgStatusEffectKeys.GuardUp);
        bool monsterRegenerating = HasRpgStatus(monster != null ? monster.StatusEffects : null, PrototypeRpgStatusEffectKeys.Regen);

        int score = skill.IsSkillAction ? 30 : 10;
        if (preferSkillWindow && skill.IsSkillAction)
        {
            score += 12;
        }

        switch (skill.ActionBiasKey)
        {
            case "debuff":
                score += (!targetIsWeakened ? 24 : 0) + (!targetIsMarked ? 14 : 0);
                break;
            case "aoe":
                score += livingAllies >= 2 ? 26 : -8;
                break;
            case "self_buff":
                score += monsterNeedsRecovery ? 30 : 6;
                score += (!monsterGuarded ? 10 : 0) + (!monsterRegenerating ? 10 : 0);
                break;
            case "cleanse":
                score += monsterHasDebuff ? 34 : 4;
                score += monsterNeedsRecovery ? 12 : 0;
                break;
            case "finisher":
                score += (targetIsWeakened || targetIsMarked) ? 34 : 8;
                score += target != null && target.CurrentHp <= skill.PowerValue + 2 ? 10 : 0;
                break;
            case "basic_attack":
                score += 0;
                break;
        }

        if (monster != null && monster.IsElite && skill.IsSkillAction)
        {
            score += 4;
        }

        score += ScoreEncounterPatternBias(monster, skill);
        return score;
    }

    private bool HasRpgStatus(PrototypeRpgStatusRuntimeState[] statuses, string statusId)
    {
        if (statuses == null || statuses.Length <= 0 || string.IsNullOrEmpty(statusId))
        {
            return false;
        }

        for (int i = 0; i < statuses.Length; i++)
        {
            PrototypeRpgStatusRuntimeState status = statuses[i];
            if (status != null && status.StatusId == statusId && status.RemainingTurns > 0)
            {
                return true;
            }
        }

        return false;
    }

    private bool HasAnyRpgDebuffStatus(PrototypeRpgStatusRuntimeState[] statuses)
    {
        if (statuses == null || statuses.Length <= 0)
        {
            return false;
        }

        for (int i = 0; i < statuses.Length; i++)
        {
            PrototypeRpgStatusRuntimeState status = statuses[i];
            PrototypeRpgStatusEffectDefinition definition = status != null ? PrototypeRpgStatusEffectCatalog.GetDefinition(status.StatusId) : null;
            if (status != null && status.RemainingTurns > 0 && definition != null && definition.IsDebuff)
            {
                return true;
            }
        }

        return false;
    }

    private void CommitRpgEnemySkillUse(DungeonMonsterRuntimeData monster, PrototypeRpgEnemySkillRuntimeState skill)
    {
        if (monster == null || skill == null)
        {
            return;
        }

        if (skill.MaxEncounterCharges > 0)
        {
            skill.EncounterChargesRemaining = Mathf.Max(0, skill.EncounterChargesRemaining - 1);
        }

        skill.CooldownRemaining = Mathf.Max(skill.CooldownRemaining, skill.CooldownTurns);
        monster.LastResolvedSkillId = skill.SkillId;
        monster.LastResolvedSkillLabel = skill.DisplayName;
        monster.SkillUsesThisEncounter += 1;
        RefreshRpgEnemySkillRuntimeState(monster);
    }

    private PrototypeRpgEnemySkillRuntimeState GetQueuedEnemySkillState(DungeonMonsterRuntimeData monster)
    {
        if (monster == null || string.IsNullOrEmpty(_pendingEnemySkillId))
        {
            return null;
        }

        PrototypeRpgEnemySkillRuntimeState[] skills = monster.ActiveSkillSlots ?? Array.Empty<PrototypeRpgEnemySkillRuntimeState>();
        for (int i = 0; i < skills.Length; i++)
        {
            PrototypeRpgEnemySkillRuntimeState skill = skills[i];
            if (skill != null && skill.SkillId == _pendingEnemySkillId)
            {
                return skill;
            }
        }

        return null;
    }

    private bool ResolveQueuedEnemySkillTargetsAllParty(PrototypeRpgEnemySkillRuntimeState skill)
    {
        if (skill == null)
        {
            return false;
        }

        return string.Equals(skill.TargetKind, "all_party_members", StringComparison.Ordinal);
    }

    private bool ResolveQueuedEnemySkillTargetsSelf(PrototypeRpgEnemySkillRuntimeState skill)
    {
        if (skill == null)
        {
            return false;
        }

        return string.Equals(skill.TargetKind, "self", StringComparison.Ordinal);
    }

    private string BuildEnemyIntentEffectSummaryText(PrototypeRpgEnemySkillRuntimeState skill)
    {
        if (skill == null)
        {
            return string.Empty;
        }

        string targetText = ResolveQueuedEnemySkillTargetsAllParty(skill)
            ? "all allies"
            : ResolveQueuedEnemySkillTargetsSelf(skill)
                ? "self"
                : "single target";
        return BuildRpgCompactSummaryText(skill.DisplayName, targetText, !string.IsNullOrEmpty(skill.PredictedStatusText) ? skill.PredictedStatusText : skill.EffectType);
    }

    private string BuildEnemyActionEconomySummaryText(PrototypeRpgEnemySkillRuntimeState skill)
    {
        if (skill == null)
        {
            return string.Empty;
        }

        string chargeText = skill.MaxEncounterCharges > 0
            ? "Uses " + skill.EncounterChargesRemaining + "/" + skill.MaxEncounterCharges
            : "Repeat";
        string cooldownText = skill.CooldownRemaining > 0
            ? "CD " + skill.CooldownRemaining
            : skill.CooldownTurns > 0
                ? "CD ready"
                : "Ready";
        return BuildRpgCompactSummaryText(skill.DisplayName, chargeText, cooldownText);
    }

    private string BuildEnemyIntentExpansionText(DungeonMonsterRuntimeData monster, int targetIndex, PrototypeRpgEnemySkillRuntimeState skill)
    {
        if (monster == null)
        {
            return "Enemy intent.";
        }

        if (skill == null)
        {
            return BuildEnemyIntentText(monster, targetIndex, null);
        }

        string roleLabel = GetMonsterRoleText(monster);
        if (ResolveQueuedEnemySkillTargetsSelf(skill))
        {
            return monster.DisplayName + " (" + roleLabel + ") intends " + skill.DisplayName + " on self.";
        }

        if (ResolveQueuedEnemySkillTargetsAllParty(skill))
        {
            return monster.DisplayName + " (" + roleLabel + ") intends " + skill.DisplayName + " on all living allies.";
        }

        return monster.DisplayName + " (" + roleLabel + ") intends " + skill.DisplayName + " on " + GetPartyMemberDisplayName(targetIndex) + ".";
    }

    private string BuildCurrentEnemySkillLoadoutSummaryText()
    {
        List<string> parts = new List<string>();
        for (int i = 0; i < _activeMonsters.Count; i++)
        {
            DungeonMonsterRuntimeData monster = _activeMonsters[i];
            if (monster == null)
            {
                continue;
            }

            InitializeRpgEnemySkillRuntimeState(monster, false);
            string skillLabel = !string.IsNullOrEmpty(monster.LastResolvedSkillLabel)
                ? monster.LastResolvedSkillLabel + " x" + Mathf.Max(1, monster.SkillUsesThisEncounter)
                : monster.SkillLoadoutSummaryText;
            if (!string.IsNullOrEmpty(skillLabel))
            {
                parts.Add(monster.DisplayName + " " + skillLabel);
            }
        }

        return parts.Count > 0 ? string.Join(" | ", parts.ToArray()) : string.Empty;
    }

    private string BuildCurrentEnemyActionEconomySummaryText()
    {
        List<string> parts = new List<string>();
        for (int i = 0; i < _activeMonsters.Count; i++)
        {
            DungeonMonsterRuntimeData monster = _activeMonsters[i];
            if (monster == null)
            {
                continue;
            }

            InitializeRpgEnemySkillRuntimeState(monster, false);
            if (!string.IsNullOrEmpty(monster.ActionEconomySummaryText))
            {
                parts.Add(monster.DisplayName + " " + monster.ActionEconomySummaryText);
            }
        }

        return parts.Count > 0 ? string.Join(" | ", parts.ToArray()) : string.Empty;
    }

    private string BuildCurrentEnemyIntentExpansionSummaryText()
    {
        if (_currentEnemyIntentSnapshot != null && !string.IsNullOrEmpty(_currentEnemyIntentSnapshot.PreviewText))
        {
            return BuildRpgCompactSummaryText(
                _currentEnemyIntentSnapshot.PreviewText,
                _currentEnemyIntentSnapshot.PredictedEffectText,
                _currentEnemyIntentSnapshot.PredictedStatusText,
                _currentEnemyIntentSnapshot.ActionEconomyText);
        }

        return string.IsNullOrEmpty(_enemyIntentText) || _enemyIntentText == "None" ? string.Empty : _enemyIntentText;
    }

    private string BuildBattleUiEnemyLoadoutHintText(DungeonMonsterRuntimeData monster)
    {
        if (monster == null)
        {
            return string.Empty;
        }

        return BuildRpgCompactSummaryText(monster.SkillLoadoutSummaryText, monster.SkillCooldownSummaryText, monster.ActionEconomySummaryText);
    }

    private int CleanseRpgMonsterDebuffs(DungeonMonsterRuntimeData monster, int maxRemoved)
    {
        if (monster == null || maxRemoved == 0)
        {
            return 0;
        }

        PrototypeRpgStatusRuntimeState[] statuses = monster.StatusEffects ?? Array.Empty<PrototypeRpgStatusRuntimeState>();
        if (statuses.Length <= 0)
        {
            return 0;
        }

        List<PrototypeRpgStatusRuntimeState> remaining = new List<PrototypeRpgStatusRuntimeState>();
        int removed = 0;
        for (int i = 0; i < statuses.Length; i++)
        {
            PrototypeRpgStatusRuntimeState status = statuses[i];
            PrototypeRpgStatusEffectDefinition definition = status != null ? PrototypeRpgStatusEffectCatalog.GetDefinition(status.StatusId) : null;
            if (status != null && definition != null && definition.IsDebuff && (maxRemoved < 0 || removed < maxRemoved))
            {
                removed += 1;
                continue;
            }

            if (status != null)
            {
                remaining.Add(status);
            }
        }

        if (removed <= 0)
        {
            return 0;
        }

        monster.StatusEffects = remaining.Count > 0 ? remaining.ToArray() : Array.Empty<PrototypeRpgStatusRuntimeState>();
        RefreshRpgStatusSummaryText(monster);
        string summaryText = monster.DisplayName + " purged " + removed + " debuff" + (removed == 1 ? string.Empty : "s") + ".";
        RecordBattleEvent(
            PrototypeBattleEventKeys.StatusCleansed,
            monster.MonsterId,
            monster.MonsterId,
            removed,
            summaryText,
            actionKey: "skill",
            phaseKey: "enemy_turn",
            actorName: monster.DisplayName,
            targetName: monster.DisplayName,
            shortText: "Purge");
        AppendBattleLog(summaryText);
        return removed;
    }

    private void ApplyRpgMonsterStatusToSelf(DungeonMonsterRuntimeData monster, string statusId, int durationTurns, int powerValue)
    {
        if (monster == null || string.IsNullOrEmpty(statusId))
        {
            return;
        }

        PrototypeRpgStatusEffectDefinition definition = PrototypeRpgStatusEffectCatalog.GetDefinition(statusId);
        if (definition == null)
        {
            return;
        }

        PrototypeRpgStatusRuntimeState[] statuses = monster.StatusEffects ?? Array.Empty<PrototypeRpgStatusRuntimeState>();
        int resolvedDuration = ResolveRpgMonsterStatusDurationTurns(monster, statusId, durationTurns > 0 ? durationTurns : definition.DefaultDurationTurns);
        int resolvedPower = ResolveRpgMonsterStatusPowerValue(monster, statusId, powerValue > 0 ? powerValue : definition.DefaultPowerValue);
        bool refreshedExisting;
        PrototypeRpgStatusRuntimeState resolvedState;
        monster.StatusEffects = ApplyOrRefreshRpgStatusArray(
            statuses,
            definition,
            monster.MonsterId,
            monster.MonsterId,
            resolvedDuration,
            resolvedPower,
            1,
            out resolvedState,
            out refreshedExisting);
        RefreshRpgStatusSummaryText(monster);
        string summaryText = monster.DisplayName + " gained " + definition.DisplayLabel + ".";
        RecordBattleEvent(
            PrototypeBattleEventKeys.StatusApplied,
            monster.MonsterId,
            monster.MonsterId,
            resolvedPower,
            summaryText,
            actionKey: "skill",
            phaseKey: "enemy_turn",
            actorName: monster.DisplayName,
            targetName: monster.DisplayName,
            shortText: refreshedExisting ? definition.ShortLabel + " refresh" : definition.ShortLabel);
        AppendBattleLog(summaryText);
    }

    private void ApplyResolvedEnemySkillEffects(DungeonMonsterRuntimeData actor, DungeonPartyMemberRuntimeData targetMember, List<int> targetPartyIndices, PrototypeRpgEnemySkillRuntimeState skill)
    {
        if (actor == null)
        {
            return;
        }

        if (skill == null || !skill.IsSkillAction)
        {
            ApplyRpgStatusesForEnemyAction(actor, targetMember, targetPartyIndices, false);
            return;
        }

        switch (skill.SkillId)
        {
            case "enemy_marking_gouge":
                if (targetMember != null)
                {
                    ApplyRpgStatusEffectFromMonster(actor, targetMember, PrototypeRpgStatusEffectKeys.Mark, 2, 1);
                    ApplyRpgStatusEffectFromMonster(actor, targetMember, PrototypeRpgStatusEffectKeys.Weaken, 2, 1);
                }
                break;
            case "enemy_command_rupture":
                if (targetMember != null)
                {
                    ApplyRpgStatusEffectFromMonster(actor, targetMember, PrototypeRpgStatusEffectKeys.Weaken, 2, 2);
                }
                break;
            case "enemy_cinder_wave":
            case "enemy_royal_wave":
                if (targetPartyIndices != null)
                {
                    for (int i = 0; i < targetPartyIndices.Count; i++)
                    {
                        DungeonPartyMemberRuntimeData member = GetPartyMemberAtIndex(targetPartyIndices[i]);
                        if (member != null)
                        {
                            ApplyRpgStatusEffectFromMonster(actor, member, PrototypeRpgStatusEffectKeys.Burn, 2, 1);
                        }
                    }
                }
                break;
            case "enemy_guard_howl":
                ApplyRpgMonsterStatusToSelf(actor, PrototypeRpgStatusEffectKeys.GuardUp, 2, 1);
                ApplyRpgMonsterStatusToSelf(actor, PrototypeRpgStatusEffectKeys.Regen, 2, 1);
                break;
            case "enemy_purging_gel":
                CleanseRpgMonsterDebuffs(actor, -1);
                ApplyRpgMonsterStatusToSelf(actor, PrototypeRpgStatusEffectKeys.Regen, 2, 1);
                ApplyRpgMonsterStatusToSelf(actor, PrototypeRpgStatusEffectKeys.CleanseOnce, 2, 1);
                break;
        }
    }
}
