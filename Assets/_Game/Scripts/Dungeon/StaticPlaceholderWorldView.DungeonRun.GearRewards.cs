using System;
using System.Collections.Generic;
using UnityEngine;

public sealed partial class StaticPlaceholderWorldView
{
    private PrototypeRpgRuntimeGearInventoryState _sessionRpgRuntimeGearInventoryState = new PrototypeRpgRuntimeGearInventoryState();
    private PrototypeRpgGearUnlockState _sessionRpgGearUnlockState = new PrototypeRpgGearUnlockState();

    public string CurrentGearRewardCandidateSummaryText => string.IsNullOrEmpty(_sessionRpgRuntimeGearInventoryState.RewardCandidateSummaryText) ? "None" : _sessionRpgRuntimeGearInventoryState.RewardCandidateSummaryText;
    public string CurrentEquipSwapChoiceSummaryText => string.IsNullOrEmpty(_sessionRpgRuntimeGearInventoryState.EquipSwapSummaryText) ? "None" : _sessionRpgRuntimeGearInventoryState.EquipSwapSummaryText;
    public string CurrentGearCarryContinuitySummaryText => string.IsNullOrEmpty(_sessionRpgRuntimeGearInventoryState.GearCarryContinuitySummaryText) ? "None" : _sessionRpgRuntimeGearInventoryState.GearCarryContinuitySummaryText;
    public string CurrentGearRuleSummaryText => string.IsNullOrEmpty(_sessionRpgRuntimeGearInventoryState.GearRuleSummaryText) ? "None" : _sessionRpgRuntimeGearInventoryState.GearRuleSummaryText;
    public string CurrentGearUnlockSummaryText => string.IsNullOrEmpty(_sessionRpgRuntimeGearInventoryState.GearUnlockSummaryText) ? "None" : _sessionRpgRuntimeGearInventoryState.GearUnlockSummaryText;
    public string CurrentGearComparisonSummaryText => string.IsNullOrEmpty(_sessionRpgRuntimeGearInventoryState.GearComparisonSummaryText) ? "None" : _sessionRpgRuntimeGearInventoryState.GearComparisonSummaryText;
    public string CurrentGearAffixLiteSummaryText => string.IsNullOrEmpty(_sessionRpgRuntimeGearInventoryState.GearAffixLiteSummaryText) ? "None" : _sessionRpgRuntimeGearInventoryState.GearAffixLiteSummaryText;
    public string CurrentUnlockedGearPoolSummaryText => string.IsNullOrEmpty(_sessionRpgRuntimeGearInventoryState.UnlockedGearPoolSummaryText) ? "None" : _sessionRpgRuntimeGearInventoryState.UnlockedGearPoolSummaryText;
    public string CurrentLevelBandGearLinkageSummaryText => string.IsNullOrEmpty(_sessionRpgRuntimeGearInventoryState.LevelBandGearLinkageSummaryText) ? "None" : _sessionRpgRuntimeGearInventoryState.LevelBandGearLinkageSummaryText;
    public string ResultPanelPostRunSummaryText => _runResultState == RunResultState.None ? "None" : BuildRpgResultPanelSummaryOrNone(PostRunSummaryHeadlineText, PostRunSummarySubheadlineText, PendingRewardDeltaSummaryText, RewardCarryoverSummaryText);
    public string ResultPanelOfferAndApplySummaryText => _runResultState == RunResultState.None ? "None" : BuildRpgResultPanelSummaryOrNone(CurrentAppliedIdentitySummaryText, PostRunUpgradeOfferSummaryText, ApplyReadyOfferSummaryText, OfferContinuitySummaryText, NextOfferBasisSummaryText, CurrentNextRunSpecializationPreviewText, CurrentNextRunActiveSkillPreviewText);
    public string ResultPanelStrategySummaryText => _runResultState == RunResultState.None ? "None" : BuildRpgResultPanelSummaryOrNone(CurrentPartyCoverageSummaryText, CurrentRoleOverlapWarningSummaryText, CurrentCrossMemberSynergySummaryText, CurrentPartyArchetypeSummaryText, CurrentFormationCoherenceSummaryText, CurrentStreakSensitivePacingSummaryText, CurrentMomentumTierSummaryText, CurrentRuntimeSynergySummaryText, CurrentPartyActionEconomySummaryText, CurrentEncounterPhaseSummaryText, CurrentEncounterWaveSummaryText, CurrentBossPatternSummaryText, CurrentEnemyIntentExpansionSummaryText, CurrentEnemySkillLoadoutSummaryText, CurrentEnemyActionEconomySummaryText, CurrentEnemyStatusEffectSummaryText, CurrentStatusUsageSummaryText);
    public string ResultPanelProgressionSummaryText => _runResultState == RunResultState.None ? "None" : BuildRpgResultPanelSummaryOrNone(ResultPanelExperienceGainSummaryText, ResultPanelLevelUpPreviewText, ResultPanelLevelUpApplyReadyText, ResultPanelActualLevelApplyText, ResultPanelDerivedStatHydrateText, ResultPanelNextRunStatProjectionText, CurrentPartyJobSpecializationSummaryText, CurrentPartyPassiveSkillSlotSummaryText, CurrentPartyActiveSkillSlotSummaryText, CurrentPartySkillCooldownSummaryText, CurrentPartySkillResourceSummaryText, CurrentPartyStatusEffectSummaryText, ResultPanelSkillLoadoutSummaryText, CurrentPartyEquipmentSummaryText, CurrentPartyGearContributionSummaryText, ResultPanelConsumableCarrySummaryText, ResultPanelConsumableCarryoverPolicyText);
    public string ResultPanelGearRewardSummaryText => _runResultState == RunResultState.None ? "None" : BuildRpgResultPanelSummaryOrNone(CurrentGearRewardCandidateSummaryText, CurrentGearComparisonSummaryText, CurrentGearAffixLiteSummaryText, CurrentGearRuleSummaryText, CurrentGearUnlockSummaryText, CurrentUnlockedGearPoolSummaryText, CurrentLevelBandGearLinkageSummaryText, CurrentEquipSwapChoiceSummaryText, CurrentGearCarryContinuitySummaryText);
    public PrototypeRpgRuntimeGearInventoryState LatestRpgRuntimeGearInventoryState => CopyRpgRuntimeGearInventoryState(_sessionRpgRuntimeGearInventoryState);
    public PrototypeRpgGearUnlockState LatestRpgGearUnlockState => CopyRpgGearUnlockState(_sessionRpgGearUnlockState);

    private void ResetRpgGearRewardRuntimeState()
    {
        _sessionRpgRuntimeGearInventoryState = new PrototypeRpgRuntimeGearInventoryState();
    }

    private void ResetRpgGearRewardSessionState()
    {
        ResetRpgGearRewardRuntimeState();
        _sessionRpgGearUnlockState = new PrototypeRpgGearUnlockState();
    }

    private PrototypeRpgRuntimeGearInventoryState RefreshRpgPostRunGearRewards(PrototypeRpgRunResultSnapshot runResultSnapshot, PrototypeRpgCombatContributionSnapshot contributionSnapshot)
    {
        PrototypeRpgPartyDefinition partyDefinition = _activeDungeonParty != null ? _activeDungeonParty.PartyDefinition : null;
        if (partyDefinition == null)
        {
            ResetRpgGearRewardRuntimeState();
            return LatestRpgRuntimeGearInventoryState;
        }

        PrototypeRpgAppliedPartyProgressState appliedProgressState = GetOrCreateRpgAppliedPartyProgressState(partyDefinition);
        PrototypeRpgPartyLevelRuntimeState partyLevelState = GetOrCreateRpgPartyLevelRuntimeState(partyDefinition, appliedProgressState);
        PrototypeRpgInventoryRuntimeState inventoryState = GetOrCreateRpgInventoryRuntimeState(partyDefinition);

        _sessionRpgRuntimeGearInventoryState.PartyId = string.IsNullOrEmpty(partyDefinition.PartyId) ? string.Empty : partyDefinition.PartyId;
        _sessionRpgRuntimeGearInventoryState.DisplayName = string.IsNullOrEmpty(partyDefinition.DisplayName) ? _sessionRpgRuntimeGearInventoryState.PartyId : partyDefinition.DisplayName;
        _sessionRpgRuntimeGearInventoryState.LastRunIdentity = BuildRpgGearRewardRunIdentity(runResultSnapshot);
        RefreshRpgGearUnlockStateForRun(runResultSnapshot, partyDefinition);
        _sessionRpgRuntimeGearInventoryState.RewardCandidates = BuildPostRunEquipmentRewardCandidates(runResultSnapshot, contributionSnapshot, partyDefinition, appliedProgressState, partyLevelState, _sessionRpgGearUnlockState);
        _sessionRpgRuntimeGearInventoryState.RewardDefinitions = BuildPostRunEquipmentRewardDefinitions(_sessionRpgRuntimeGearInventoryState.RewardCandidates, partyDefinition);
        _sessionRpgRuntimeGearInventoryState.GearDefinitions = BuildRpgGearDefinitions(_sessionRpgRuntimeGearInventoryState.RewardDefinitions);
        FinalizeRpgGearUnlockState(_sessionRpgRuntimeGearInventoryState.GearDefinitions, _sessionRpgRuntimeGearInventoryState.LastRunIdentity, _sessionRpgRuntimeGearInventoryState.PartyId);
        _sessionRpgRuntimeGearInventoryState.UnlockState = CopyRpgGearUnlockState(_sessionRpgGearUnlockState);
        _sessionRpgRuntimeGearInventoryState.AffixDefinitions = BuildRpgGearAffixDefinitions(_sessionRpgRuntimeGearInventoryState.RewardCandidates);
        _sessionRpgRuntimeGearInventoryState.AffixContributions = BuildRpgRewardAffixLiteContributions(_sessionRpgRuntimeGearInventoryState.RewardCandidates);
        _sessionRpgRuntimeGearInventoryState.ComparisonSummaries = BuildRpgGearComparisonSummaries(_sessionRpgRuntimeGearInventoryState.RewardCandidates);
        _sessionRpgRuntimeGearInventoryState.EquipSwapChoices = BuildRpgEquipSwapChoices(_sessionRpgRuntimeGearInventoryState.RewardCandidates);
        _sessionRpgRuntimeGearInventoryState.SlotRules = BuildRpgGearSlotRules(_sessionRpgRuntimeGearInventoryState.RewardCandidates, _sessionRpgRuntimeGearInventoryState.UnlockState);
        CommitRpgEquipSwapChoices(_sessionRpgRuntimeGearInventoryState, appliedProgressState, partyDefinition);

        appliedProgressState.AppliedSummaryText = BuildRpgAppliedPartySummaryText(appliedProgressState, partyDefinition);
        partyLevelState = GetOrCreateRpgPartyLevelRuntimeState(partyDefinition, appliedProgressState);
        inventoryState = GetOrCreateRpgInventoryRuntimeState(partyDefinition);

        _sessionRpgRuntimeGearInventoryState.ActiveEquippedSummaryText = string.IsNullOrEmpty(partyLevelState.EquipmentLoadoutReadbackSummaryText) ? string.Empty : partyLevelState.EquipmentLoadoutReadbackSummaryText;
        _sessionRpgRuntimeGearInventoryState.RewardCandidateSummaryText = BuildRpgGearRewardCandidateSummaryText(_sessionRpgRuntimeGearInventoryState.RewardCandidates);
        _sessionRpgRuntimeGearInventoryState.EquipSwapSummaryText = BuildRpgEquipSwapSummaryText(_sessionRpgRuntimeGearInventoryState.EquipSwapChoices);
        _sessionRpgRuntimeGearInventoryState.GearRuleSummaryText = BuildRpgGearRuleSummaryText(_sessionRpgRuntimeGearInventoryState.SlotRules);
        _sessionRpgRuntimeGearInventoryState.GearUnlockSummaryText = string.IsNullOrEmpty(_sessionRpgGearUnlockState.UnlockSummaryText) ? string.Empty : _sessionRpgGearUnlockState.UnlockSummaryText;
        _sessionRpgRuntimeGearInventoryState.GearComparisonSummaryText = BuildRpgGearComparisonSummaryText(_sessionRpgRuntimeGearInventoryState.ComparisonSummaries);
        _sessionRpgRuntimeGearInventoryState.GearAffixLiteSummaryText = BuildRpgGearAffixLiteSummaryText(_sessionRpgRuntimeGearInventoryState.AffixContributions);
        _sessionRpgRuntimeGearInventoryState.UnlockedGearPoolSummaryText = string.IsNullOrEmpty(_sessionRpgGearUnlockState.UnlockedPoolSummaryText) ? string.Empty : _sessionRpgGearUnlockState.UnlockedPoolSummaryText;
        _sessionRpgRuntimeGearInventoryState.LevelBandGearLinkageSummaryText = BuildRpgLevelBandGearLinkageSummaryText(_sessionRpgRuntimeGearInventoryState.RewardCandidates);
        _sessionRpgRuntimeGearInventoryState.GearCarryContinuitySummaryText = BuildRpgGearCarryContinuitySummaryText(partyDefinition, appliedProgressState, partyLevelState, inventoryState);
        _sessionRpgRuntimeGearInventoryState.HudSummaryText = BuildRpgCompactSummaryText(_sessionRpgRuntimeGearInventoryState.RewardCandidateSummaryText, _sessionRpgRuntimeGearInventoryState.GearComparisonSummaryText, _sessionRpgRuntimeGearInventoryState.GearAffixLiteSummaryText, _sessionRpgRuntimeGearInventoryState.GearUnlockSummaryText, _sessionRpgRuntimeGearInventoryState.UnlockedGearPoolSummaryText, _sessionRpgRuntimeGearInventoryState.LevelBandGearLinkageSummaryText, _sessionRpgRuntimeGearInventoryState.GearCarryContinuitySummaryText);

        ApplyRpgGearRewardReadbackConsistency(runResultSnapshot, partyLevelState, inventoryState, _sessionRpgRuntimeGearInventoryState);
        return LatestRpgRuntimeGearInventoryState;
    }

    private string BuildRpgGearRewardRunIdentity(PrototypeRpgRunResultSnapshot runResultSnapshot)
    {
        PrototypeRpgRunResultSnapshot safeRunResult = runResultSnapshot ?? new PrototypeRpgRunResultSnapshot();
        string dungeonId = string.IsNullOrEmpty(safeRunResult.DungeonId) ? _currentDungeonId : safeRunResult.DungeonId;
        string routeId = string.IsNullOrEmpty(safeRunResult.RouteId) ? _selectedRouteId : safeRunResult.RouteId;
        string resultKey = string.IsNullOrEmpty(safeRunResult.ResultStateKey) ? _runResultState.ToString().ToLowerInvariant() : safeRunResult.ResultStateKey;
        return dungeonId + ":" + routeId + ":" + resultKey + ":" + Mathf.Max(0, safeRunResult.TotalTurnsTaken);
    }

    private PrototypeRpgEquipmentRewardCandidate[] BuildPostRunEquipmentRewardCandidates(PrototypeRpgRunResultSnapshot runResultSnapshot, PrototypeRpgCombatContributionSnapshot contributionSnapshot, PrototypeRpgPartyDefinition partyDefinition, PrototypeRpgAppliedPartyProgressState appliedProgressState, PrototypeRpgPartyLevelRuntimeState partyLevelState, PrototypeRpgGearUnlockState unlockState)
    {
        PrototypeRpgPartyMemberDefinition memberDefinition = ResolveRpgGearRewardTargetMember(runResultSnapshot, contributionSnapshot, partyDefinition);
        if (memberDefinition == null)
        {
            return Array.Empty<PrototypeRpgEquipmentRewardCandidate>();
        }

        PrototypeRpgAppliedPartyMemberProgressState appliedMemberState = GetRpgAppliedPartyMemberProgressState(appliedProgressState, memberDefinition.MemberId);
        PrototypeRpgPartyMemberLevelRuntimeState memberLevelState = GetRpgMemberLevelRuntimeState(partyLevelState, memberDefinition.MemberId);
        string currentLoadoutId = ResolveAppliedOrDefinitionId(appliedMemberState != null ? appliedMemberState.AppliedEquipmentLoadoutId : string.Empty, memberDefinition.EquipmentLoadoutId);
        string sourceKey = ResolveRpgGearRewardSourceKey(runResultSnapshot);
        int currentLevel = memberLevelState != null && memberLevelState.Experience != null ? Mathf.Max(1, memberLevelState.Experience.Level) : 1;
        string levelBandKey = ResolveRpgGearLevelBandKey(currentLevel);
        string targetTierKey = ResolveRpgGearRewardTier(sourceKey, unlockState, levelBandKey);
        string targetLoadoutId = ResolveRpgEquipmentRewardLoadoutId(memberDefinition.RoleTag, currentLoadoutId, sourceKey, targetTierKey, unlockState);
        if (string.IsNullOrEmpty(targetLoadoutId))
        {
            return Array.Empty<PrototypeRpgEquipmentRewardCandidate>();
        }

        PrototypeRpgEquipmentRewardDefinition rewardDefinition = BuildRpgEquipmentRewardDefinition(memberDefinition, targetLoadoutId, sourceKey, unlockState);
        if (rewardDefinition == null)
        {
            return Array.Empty<PrototypeRpgEquipmentRewardCandidate>();
        }

        PrototypeRpgEquipmentLoadoutDefinition currentLoadout = PrototypeRpgEquipmentCatalog.ResolveDefinition(currentLoadoutId, memberDefinition.RoleTag);
        PrototypeRpgEquipmentLoadoutDefinition targetLoadout = PrototypeRpgEquipmentCatalog.ResolveDefinition(targetLoadoutId, memberDefinition.RoleTag);
        PrototypeRpgEquipmentRewardCandidate candidate = new PrototypeRpgEquipmentRewardCandidate();
        candidate.RewardId = rewardDefinition.RewardId;
        candidate.MemberId = string.IsNullOrEmpty(memberDefinition.MemberId) ? string.Empty : memberDefinition.MemberId;
        candidate.MemberDisplayName = string.IsNullOrEmpty(memberDefinition.DisplayName) ? "Adventurer" : memberDefinition.DisplayName;
        candidate.EquipmentLoadoutId = rewardDefinition.EquipmentLoadoutId;
        candidate.CurrentEquipmentLoadoutId = string.IsNullOrEmpty(currentLoadoutId) ? string.Empty : currentLoadoutId;
        candidate.CurrentEquipmentDisplayLabel = currentLoadout != null ? currentLoadout.DisplayName : "Current kit";
        candidate.DisplayLabel = rewardDefinition.DisplayLabel;
        candidate.SlotKey = rewardDefinition.SlotKey;
        candidate.TierKey = rewardDefinition.TierKey;
        candidate.RarityKey = rewardDefinition.RarityKey;
        candidate.LevelBandSummaryText = BuildRpgLevelBandSummaryText(levelBandKey, currentLevel);
        candidate.UnlockStateKey = rewardDefinition.UnlockStateKey;
        candidate.GearGroupKey = rewardDefinition.GearGroupKey;
        candidate.CandidateSourceKey = rewardDefinition.CandidateSourceKey;
        PrototypeRpgGearAffixLiteContribution[] currentAffixes = BuildRpgGearAffixLiteContributions(memberDefinition.RoleTag, currentLoadout, levelBandKey, unlockState, false);
        PrototypeRpgGearAffixLiteContribution[] targetAffixes = BuildRpgGearAffixLiteContributions(memberDefinition.RoleTag, targetLoadout, levelBandKey, unlockState, true);
        candidate.DerivedStatDeltaSummaryText = BuildRpgEquipmentRewardDeltaPreviewText(currentLoadout, targetLoadout, currentAffixes, targetAffixes);
        candidate.SkillHintText = targetLoadout != null && !string.IsNullOrEmpty(targetLoadout.BattleLabelHint) ? targetLoadout.BattleLabelHint : string.Empty;
        candidate.RoleHintText = memberLevelState != null && !string.IsNullOrEmpty(memberLevelState.DerivedStats.BattleLabelHint) ? memberLevelState.DerivedStats.BattleLabelHint : (targetLoadout != null ? targetLoadout.BattleLabelHint : string.Empty);
        candidate.AffixLiteSummaryText = BuildRpgGearAffixLiteCandidateSummaryText(targetAffixes);
        candidate.UnlockedPoolSummaryText = BuildRpgUnlockedGearPoolSummaryText(memberDefinition.RoleTag, levelBandKey, unlockState, sourceKey);
        candidate.IsRecommended = !string.Equals(candidate.CurrentEquipmentLoadoutId, candidate.EquipmentLoadoutId, StringComparison.Ordinal);
        candidate.EffectPreviewText = BuildRpgCompactSummaryText(rewardDefinition.EffectPreviewText, candidate.DerivedStatDeltaSummaryText, candidate.AffixLiteSummaryText, candidate.SkillHintText, candidate.UnlockedPoolSummaryText);
        string sourceLabel = ResolveRpgGearRewardSourceLabel(sourceKey);
        candidate.RuleSummaryText = BuildRpgGearSlotRuleDetailText(candidate.CurrentEquipmentDisplayLabel, candidate.DisplayLabel, candidate.SlotKey, unlockState);
        candidate.UnlockSummaryText = BuildRpgGearUnlockCandidateSummaryText(candidate, unlockState);
        candidate.ComparisonSummaryText = BuildRpgGearComparisonDetailText(candidate.CurrentEquipmentDisplayLabel, candidate.DisplayLabel, candidate.SlotKey, candidate.TierKey, candidate.RarityKey, candidate.DerivedStatDeltaSummaryText, candidate.SkillHintText, candidate.RoleHintText, candidate.AffixLiteSummaryText);
        candidate.SummaryText = BuildRpgCompactSummaryText(
            "Gear reward: " + candidate.MemberDisplayName + " -> " + candidate.DisplayLabel,
            "Source: " + sourceLabel,
            "Slot: " + ResolveRpgGearSlotLabel(candidate.SlotKey),
            "Tier: " + ResolveRpgGearTierLabel(candidate.TierKey),
            "Rarity: " + ResolveRpgGearRarityLabel(candidate.RarityKey),
            candidate.AffixLiteSummaryText,
            candidate.LevelBandSummaryText);
        return new[] { candidate };
    }

    private PrototypeRpgEquipmentRewardDefinition[] BuildPostRunEquipmentRewardDefinitions(PrototypeRpgEquipmentRewardCandidate[] candidates, PrototypeRpgPartyDefinition partyDefinition)
    {
        if (candidates == null || candidates.Length <= 0)
        {
            return Array.Empty<PrototypeRpgEquipmentRewardDefinition>();
        }

        List<PrototypeRpgEquipmentRewardDefinition> definitions = new List<PrototypeRpgEquipmentRewardDefinition>();
        for (int i = 0; i < candidates.Length; i++)
        {
            PrototypeRpgEquipmentRewardCandidate candidate = candidates[i];
            if (candidate == null || string.IsNullOrEmpty(candidate.MemberId))
            {
                continue;
            }

            PrototypeRpgPartyMemberDefinition memberDefinition = FindRpgPartyMemberDefinition(partyDefinition, candidate.MemberId);
            PrototypeRpgEquipmentRewardDefinition definition = BuildRpgEquipmentRewardDefinition(memberDefinition, candidate.EquipmentLoadoutId, candidate.CandidateSourceKey, _sessionRpgGearUnlockState);
            if (definition != null)
            {
                definitions.Add(definition);
            }
        }

        return definitions.Count > 0 ? definitions.ToArray() : Array.Empty<PrototypeRpgEquipmentRewardDefinition>();
    }

    private PrototypeRpgEquipmentRewardDefinition BuildRpgEquipmentRewardDefinition(PrototypeRpgPartyMemberDefinition memberDefinition, string targetLoadoutId, string sourceKey, PrototypeRpgGearUnlockState unlockState)
    {
        string roleTag = memberDefinition != null ? memberDefinition.RoleTag : string.Empty;
        PrototypeRpgEquipmentLoadoutDefinition loadoutDefinition = PrototypeRpgEquipmentCatalog.ResolveDefinition(targetLoadoutId, roleTag);
        if (loadoutDefinition == null)
        {
            return null;
        }

        PrototypeRpgEquipmentDefinition primaryItem = PrototypeRpgEquipmentCatalog.GetPrimaryItemDefinition(loadoutDefinition.LoadoutId, roleTag);
        PrototypeRpgEquipmentRewardDefinition definition = new PrototypeRpgEquipmentRewardDefinition();
        definition.RewardId = "reward_gear_" + (memberDefinition != null ? memberDefinition.MemberId : "party") + "_" + loadoutDefinition.LoadoutId;
        definition.EquipmentLoadoutId = loadoutDefinition.LoadoutId;
        definition.GearDefinitionId = primaryItem != null ? primaryItem.EquipmentId : string.Empty;
        definition.SlotKey = primaryItem != null && !string.IsNullOrEmpty(primaryItem.SlotKey) ? primaryItem.SlotKey : "gear";
        definition.TierKey = string.IsNullOrEmpty(loadoutDefinition.TierKey) ? "core" : loadoutDefinition.TierKey;
        definition.RarityKey = ResolveRpgEquipmentRewardRarityKey(definition.TierKey);
        definition.UnlockStateKey = BuildRpgGearUnlockStateKey(unlockState, definition.TierKey, definition.RarityKey);
        definition.GearGroupKey = BuildRpgGearGroupKey(roleTag, definition.SlotKey, definition.TierKey);
        definition.DisplayLabel = loadoutDefinition.DisplayName;
        definition.EffectPreviewText = BuildRpgCompactSummaryText(loadoutDefinition.SummaryText, loadoutDefinition.PassiveHintText, loadoutDefinition.BattleLabelHint);
        definition.CandidateSourceKey = string.IsNullOrEmpty(sourceKey) ? "run" : sourceKey;
        return definition;
    }

    private PrototypeRpgPartyMemberDefinition ResolveRpgGearRewardTargetMember(PrototypeRpgRunResultSnapshot runResultSnapshot, PrototypeRpgCombatContributionSnapshot contributionSnapshot, PrototypeRpgPartyDefinition partyDefinition)
    {
        PrototypeRpgPartyMemberDefinition[] members = partyDefinition != null ? (partyDefinition.Members ?? Array.Empty<PrototypeRpgPartyMemberDefinition>()) : Array.Empty<PrototypeRpgPartyMemberDefinition>();
        if (members.Length <= 0)
        {
            return null;
        }

        PrototypeRpgMemberContributionSnapshot[] contributionMembers = contributionSnapshot != null ? (contributionSnapshot.Members ?? Array.Empty<PrototypeRpgMemberContributionSnapshot>()) : Array.Empty<PrototypeRpgMemberContributionSnapshot>();
        bool recoveryBias = runResultSnapshot != null && (runResultSnapshot.ResultStateKey == PrototypeBattleOutcomeKeys.RunDefeat || runResultSnapshot.ResultStateKey == PrototypeBattleOutcomeKeys.RunRetreat);
        PrototypeRpgMemberContributionSnapshot bestContribution = null;
        int bestScore = int.MinValue;
        for (int i = 0; i < contributionMembers.Length; i++)
        {
            PrototypeRpgMemberContributionSnapshot contribution = contributionMembers[i];
            if (contribution == null || string.IsNullOrEmpty(contribution.MemberId))
            {
                continue;
            }

            int score = recoveryBias
                ? (contribution.KnockedOut ? 120 : 0) + (contribution.DamageTaken * 5) - (contribution.DamageDealt * 2)
                : (contribution.EliteVictor ? 100 : 0) + (contribution.DamageDealt * 5) + (contribution.KillCount * 12) + contribution.HealingDone;
            if (bestContribution == null || score > bestScore)
            {
                bestContribution = contribution;
                bestScore = score;
            }
        }

        if (bestContribution != null)
        {
            PrototypeRpgPartyMemberDefinition resolved = FindRpgPartyMemberDefinition(partyDefinition, bestContribution.MemberId);
            if (resolved != null)
            {
                return resolved;
            }
        }

        return members[0];
    }

    private string ResolveRpgGearRewardSourceKey(PrototypeRpgRunResultSnapshot runResultSnapshot)
    {
        PrototypeRpgRunResultSnapshot safeRunResult = runResultSnapshot ?? new PrototypeRpgRunResultSnapshot();
        PrototypeRpgLootOutcomeSnapshot lootOutcome = safeRunResult.LootOutcome ?? new PrototypeRpgLootOutcomeSnapshot();
        PrototypeRpgEliteOutcomeSnapshot eliteOutcome = safeRunResult.EliteOutcome ?? new PrototypeRpgEliteOutcomeSnapshot();
        if (eliteOutcome.IsEliteDefeated && lootOutcome.EliteRewardAmount > 0)
        {
            return "elite_reward";
        }

        if (lootOutcome.ChestLootGained > 0)
        {
            return "chest_reward";
        }

        if (safeRunResult.ResultStateKey == PrototypeBattleOutcomeKeys.RunDefeat)
        {
            return "defeat_recovery";
        }

        if (safeRunResult.ResultStateKey == PrototypeBattleOutcomeKeys.RunRetreat)
        {
            return "retreat_recovery";
        }

        if (safeRunResult.RouteId == RiskyRouteId)
        {
            return "risky_route";
        }

        return "run_clear";
    }

    private string ResolveRpgGearRewardSourceLabel(string sourceKey)
    {
        switch (sourceKey)
        {
            case "elite_reward": return "elite cache";
            case "chest_reward": return "chest salvage";
            case "defeat_recovery": return "recovery issue";
            case "retreat_recovery": return "retreat salvage";
            case "risky_route": return "risky route payoff";
            default: return "run reward";
        }
    }

    private string ResolveRpgEquipmentRewardLoadoutId(string roleTag, string currentLoadoutId, string sourceKey, string targetTierKey, PrototypeRpgGearUnlockState unlockState)
    {
        string[] pool = ResolveUnlockedGearPool(roleTag, currentLoadoutId, sourceKey, targetTierKey, unlockState);
        return PickFirstDifferentRpgLoadout(currentLoadoutId, roleTag, pool);
    }

    private string PickFirstDifferentRpgLoadout(string currentLoadoutId, string roleTag, params string[] candidateLoadoutIds)
    {
        for (int i = 0; i < candidateLoadoutIds.Length; i++)
        {
            string candidateLoadoutId = candidateLoadoutIds[i];
            if (string.IsNullOrEmpty(candidateLoadoutId) || string.Equals(candidateLoadoutId, currentLoadoutId, StringComparison.Ordinal))
            {
                continue;
            }

            if (PrototypeRpgEquipmentCatalog.ResolveDefinition(candidateLoadoutId, roleTag) != null)
            {
                return candidateLoadoutId;
            }
        }

        return currentLoadoutId;
    }

    private string ResolveRpgEquipmentRewardRarityKey(string tierKey)
    {
        switch (tierKey)
        {
            case "tier_3": return "epic";
            case "tier_2": return "rare";
            case "tier_1": return "uncommon";
            default: return "common";
        }
    }

    private string BuildRpgEquipmentRewardDeltaPreviewText(PrototypeRpgEquipmentLoadoutDefinition currentLoadout, PrototypeRpgEquipmentLoadoutDefinition targetLoadout, PrototypeRpgGearAffixLiteContribution[] currentAffixes, PrototypeRpgGearAffixLiteContribution[] targetAffixes)
    {
        if (targetLoadout == null)
        {
            return string.Empty;
        }

        int hpDelta = (targetLoadout.TotalMaxHpDelta + GetRpgAffixStatBonus(targetAffixes, "hp")) - ((currentLoadout != null ? currentLoadout.TotalMaxHpDelta : 0) + GetRpgAffixStatBonus(currentAffixes, "hp"));
        int attackDelta = (targetLoadout.TotalAttackDelta + GetRpgAffixStatBonus(targetAffixes, "attack")) - ((currentLoadout != null ? currentLoadout.TotalAttackDelta : 0) + GetRpgAffixStatBonus(currentAffixes, "attack"));
        int defenseDelta = (targetLoadout.TotalDefenseDelta + GetRpgAffixStatBonus(targetAffixes, "defense")) - ((currentLoadout != null ? currentLoadout.TotalDefenseDelta : 0) + GetRpgAffixStatBonus(currentAffixes, "defense"));
        int speedDelta = (targetLoadout.TotalSpeedDelta + GetRpgAffixStatBonus(targetAffixes, "speed")) - ((currentLoadout != null ? currentLoadout.TotalSpeedDelta : 0) + GetRpgAffixStatBonus(currentAffixes, "speed"));
        return "delta HP " + FormatSignedRpgSummaryValue(hpDelta) + " ATK " + FormatSignedRpgSummaryValue(attackDelta) + " DEF " + FormatSignedRpgSummaryValue(defenseDelta) + " SPD " + FormatSignedRpgSummaryValue(speedDelta);
    }

    private string FormatSignedRpgSummaryValue(int value)
    {
        return value > 0 ? "+" + value : value.ToString();
    }

    private int GetRpgAffixStatBonus(PrototypeRpgGearAffixLiteContribution[] affixes, string statKey)
    {
        if (affixes == null || affixes.Length <= 0)
        {
            return 0;
        }

        int total = 0;
        for (int i = 0; i < affixes.Length; i++)
        {
            PrototypeRpgGearAffixLiteContribution affix = affixes[i];
            if (affix == null)
            {
                continue;
            }

            switch (statKey)
            {
                case "hp":
                    total += affix.BonusMaxHp;
                    break;
                case "attack":
                    total += affix.BonusAttack;
                    break;
                case "defense":
                    total += affix.BonusDefense;
                    break;
                case "speed":
                    total += affix.BonusSpeed;
                    break;
            }
        }

        return total;
    }

    private string ResolveRpgGearRewardTier(string sourceKey, PrototypeRpgGearUnlockState unlockState, string levelBandKey)
    {
        string requestedTierKey;
        switch (sourceKey)
        {
            case "elite_reward":
                requestedTierKey = "tier_3";
                break;
            case "chest_reward":
            case "risky_route":
                requestedTierKey = "tier_2";
                break;
            case "defeat_recovery":
            case "retreat_recovery":
            case "run_clear":
            default:
                requestedTierKey = "tier_1";
                break;
        }

        string highestUnlockedTierKey = unlockState != null && !string.IsNullOrEmpty(unlockState.HighestUnlockedTierKey)
            ? unlockState.HighestUnlockedTierKey
            : "tier_1";
        string levelBandCeilingTierKey = ResolveRpgGearLevelBandTierCeilingKey(levelBandKey);
        int allowedTierRank = Mathf.Min(GetRpgTierRank(requestedTierKey), Mathf.Min(GetRpgTierRank(highestUnlockedTierKey), GetRpgTierRank(levelBandCeilingTierKey)));
        return allowedTierRank >= 3 ? "tier_3" : allowedTierRank == 2 ? "tier_2" : "tier_1";
    }

    private string ResolveRpgGearLevelBandKey(int level)
    {
        if (level >= 4)
        {
            return "band_3";
        }

        if (level >= 2)
        {
            return "band_2";
        }

        return "band_1";
    }

    private int GetRpgLevelBandRank(string levelBandKey)
    {
        switch (levelBandKey)
        {
            case "band_3": return 3;
            case "band_2": return 2;
            default: return 1;
        }
    }

    private string ResolveRpgGearLevelBandLabel(string levelBandKey)
    {
        switch (levelBandKey)
        {
            case "band_3": return "Band III";
            case "band_2": return "Band II";
            default: return "Band I";
        }
    }

    private string ResolveRpgGearLevelBandTierCeilingKey(string levelBandKey)
    {
        switch (levelBandKey)
        {
            case "band_3": return "tier_3";
            case "band_2": return "tier_2";
            default: return "tier_1";
        }
    }

    private string BuildRpgLevelBandSummaryText(string levelBandKey, int level)
    {
        return "Level band: Lv " + Mathf.Max(1, level) + " -> " + ResolveRpgGearLevelBandLabel(levelBandKey) + " (" + ResolveRpgGearTierLabel(ResolveRpgGearLevelBandTierCeilingKey(levelBandKey)) + " unlock)";
    }

    private string[] ResolveUnlockedGearPool(string roleTag, string currentLoadoutId, string sourceKey, string targetTierKey, PrototypeRpgGearUnlockState unlockState)
    {
        string[] basePool = BuildRpgGearLoadoutPool(roleTag, sourceKey);
        if (basePool.Length <= 0)
        {
            return Array.Empty<string>();
        }

        int allowedTierRank = GetRpgTierRank(targetTierKey);
        List<string> allowedPool = new List<string>();
        for (int i = 0; i < basePool.Length; i++)
        {
            string candidateLoadoutId = basePool[i];
            PrototypeRpgEquipmentLoadoutDefinition definition = PrototypeRpgEquipmentCatalog.ResolveDefinition(candidateLoadoutId, roleTag);
            if (definition == null)
            {
                continue;
            }

            if (GetRpgTierRank(definition.TierKey) <= allowedTierRank)
            {
                allowedPool.Add(candidateLoadoutId);
            }
        }

        if (allowedPool.Count <= 0)
        {
            return basePool;
        }

        if (!string.IsNullOrEmpty(currentLoadoutId) && allowedPool.Count == 1 && string.Equals(allowedPool[0], currentLoadoutId, StringComparison.Ordinal))
        {
            return basePool;
        }

        return allowedPool.ToArray();
    }

    private string[] BuildRpgGearLoadoutPool(string roleTag, string sourceKey)
    {
        bool recoveryBias = sourceKey == "defeat_recovery" || sourceKey == "retreat_recovery";
        bool highImpactBias = sourceKey == "elite_reward" || sourceKey == "risky_route";
        switch (string.IsNullOrEmpty(roleTag) ? string.Empty : roleTag)
        {
            case "warrior":
                return recoveryBias
                    ? new[] { "equip_warrior_shielded", "equip_warrior_bastion", "equip_warrior_vanguard", "equip_warrior_brutal" }
                    : highImpactBias
                        ? new[] { "equip_warrior_bastion", "equip_warrior_vanguard", "equip_warrior_brutal", "equip_warrior_shielded" }
                        : new[] { "equip_warrior_vanguard", "equip_warrior_brutal", "equip_warrior_shielded", "equip_warrior_bastion" };
            case "rogue":
                return recoveryBias
                    ? new[] { "equip_rogue_evasion", "equip_rogue_saboteur", "equip_rogue_phantom", "equip_rogue_finisher" }
                    : highImpactBias
                        ? new[] { "equip_rogue_phantom", "equip_rogue_saboteur", "equip_rogue_finisher", "equip_rogue_evasion" }
                        : new[] { "equip_rogue_saboteur", "equip_rogue_finisher", "equip_rogue_evasion", "equip_rogue_phantom" };
            case "mage":
                return recoveryBias
                    ? new[] { "equip_mage_warded", "equip_mage_astral", "equip_mage_tempest", "equip_mage_focus" }
                    : highImpactBias
                        ? new[] { "equip_mage_astral", "equip_mage_tempest", "equip_mage_focus", "equip_mage_warded" }
                        : new[] { "equip_mage_tempest", "equip_mage_focus", "equip_mage_warded", "equip_mage_astral" };
            case "cleric":
                return recoveryBias
                    ? new[] { "equip_cleric_guarded", "equip_cleric_saint", "equip_cleric_beacon", "equip_cleric_relic" }
                    : highImpactBias
                        ? new[] { "equip_cleric_saint", "equip_cleric_beacon", "equip_cleric_relic", "equip_cleric_guarded" }
                        : new[] { "equip_cleric_beacon", "equip_cleric_relic", "equip_cleric_guarded", "equip_cleric_saint" };
            default:
                return Array.Empty<string>();
        }
    }

    private int GetRpgTierRank(string tierKey)
    {
        switch (tierKey)
        {
            case "tier_3": return 3;
            case "tier_2": return 2;
            case "tier_1": return 1;
            default: return 0;
        }
    }

    private int GetRpgRarityRank(string rarityKey)
    {
        switch (rarityKey)
        {
            case "epic": return 3;
            case "rare": return 2;
            case "uncommon": return 1;
            default: return 0;
        }
    }

    private string ResolveRpgGearTierLabel(string tierKey)
    {
        switch (tierKey)
        {
            case "tier_3": return "Tier 3";
            case "tier_2": return "Tier 2";
            case "tier_1": return "Tier 1";
            default: return "Core";
        }
    }

    private string ResolveRpgGearRarityLabel(string rarityKey)
    {
        switch (rarityKey)
        {
            case "epic": return "Epic";
            case "rare": return "Rare";
            case "uncommon": return "Uncommon";
            default: return "Common";
        }
    }

    private string ResolveRpgGearSlotLabel(string slotKey)
    {
        switch (slotKey)
        {
            case "weapon": return "Weapon slot";
            case "armor": return "Armor slot";
            case "shield": return "Shield slot";
            case "utility": return "Utility slot";
            case "focus": return "Focus slot";
            case "relic": return "Relic slot";
            default: return "Gear slot";
        }
    }

    private void RefreshRpgGearUnlockStateForRun(PrototypeRpgRunResultSnapshot runResultSnapshot, PrototypeRpgPartyDefinition partyDefinition)
    {
        if (_sessionRpgGearUnlockState == null || !string.Equals(_sessionRpgGearUnlockState.PartyId, partyDefinition != null ? partyDefinition.PartyId : string.Empty, StringComparison.Ordinal))
        {
            _sessionRpgGearUnlockState = new PrototypeRpgGearUnlockState();
        }

        _sessionRpgGearUnlockState.PartyId = partyDefinition != null && !string.IsNullOrEmpty(partyDefinition.PartyId) ? partyDefinition.PartyId : string.Empty;
        string sourceKey = ResolveRpgGearRewardSourceKey(runResultSnapshot);
        PrototypeRpgAppliedPartyProgressState appliedProgressState = GetOrCreateRpgAppliedPartyProgressState(partyDefinition);
        PrototypeRpgPartyLevelRuntimeState partyLevelState = GetOrCreateRpgPartyLevelRuntimeState(partyDefinition, appliedProgressState);
        PrototypeRpgPartyMemberLevelRuntimeState[] members = partyLevelState != null ? (partyLevelState.Members ?? Array.Empty<PrototypeRpgPartyMemberLevelRuntimeState>()) : Array.Empty<PrototypeRpgPartyMemberLevelRuntimeState>();
        int highestLevel = 1;
        for (int i = 0; i < members.Length; i++)
        {
            PrototypeRpgPartyMemberLevelRuntimeState memberState = members[i];
            if (memberState == null || memberState.Experience == null)
            {
                continue;
            }

            highestLevel = Mathf.Max(highestLevel, Mathf.Max(1, memberState.Experience.Level));
        }

        string levelBandKey = ResolveRpgGearLevelBandKey(highestLevel);
        string baseUnlockedTierKey = ResolveRpgBaseUnlockedTierForSource(sourceKey);
        string levelBandCeilingTierKey = ResolveRpgGearLevelBandTierCeilingKey(levelBandKey);
        string unlockedTierKey = GetRpgTierRank(baseUnlockedTierKey) > GetRpgTierRank(levelBandCeilingTierKey) ? levelBandCeilingTierKey : baseUnlockedTierKey;
        string unlockedRarityKey = ResolveRpgEquipmentRewardRarityKey(unlockedTierKey);
        if (GetRpgTierRank(unlockedTierKey) > GetRpgTierRank(_sessionRpgGearUnlockState.HighestUnlockedTierKey))
        {
            _sessionRpgGearUnlockState.HighestUnlockedTierKey = unlockedTierKey;
        }

        if (GetRpgRarityRank(unlockedRarityKey) > GetRpgRarityRank(_sessionRpgGearUnlockState.HighestUnlockedRarityKey))
        {
            _sessionRpgGearUnlockState.HighestUnlockedRarityKey = unlockedRarityKey;
        }

        if (GetRpgLevelBandRank(levelBandKey) > GetRpgLevelBandRank(_sessionRpgGearUnlockState.HighestUnlockedLevelBandKey))
        {
            _sessionRpgGearUnlockState.HighestUnlockedLevelBandKey = levelBandKey;
        }

        EnsureRpgGearUnlockTierRange(_sessionRpgGearUnlockState, _sessionRpgGearUnlockState.HighestUnlockedTierKey);
        _sessionRpgGearUnlockState.UnlockedPoolSummaryText = BuildRpgCompactSummaryText(
            BuildRpgUnlockedGearPoolSummaryText("party", _sessionRpgGearUnlockState.HighestUnlockedLevelBandKey, _sessionRpgGearUnlockState, sourceKey),
            "Level linkage: " + ResolveRpgGearLevelBandLabel(_sessionRpgGearUnlockState.HighestUnlockedLevelBandKey));
        _sessionRpgGearUnlockState.UnlockSummaryText = BuildRpgCompactSummaryText(
            "Unlock ceiling: " + ResolveRpgGearTierLabel(_sessionRpgGearUnlockState.HighestUnlockedTierKey),
            "Rarity ceiling: " + ResolveRpgGearRarityLabel(_sessionRpgGearUnlockState.HighestUnlockedRarityKey),
            "Source: " + ResolveRpgGearRewardSourceLabel(sourceKey),
            "Level band: " + ResolveRpgGearLevelBandLabel(_sessionRpgGearUnlockState.HighestUnlockedLevelBandKey));
    }

    private void FinalizeRpgGearUnlockState(PrototypeRpgGearDefinition[] gearDefinitions, string runIdentity, string partyId)
    {
        if (_sessionRpgGearUnlockState == null)
        {
            _sessionRpgGearUnlockState = new PrototypeRpgGearUnlockState();
        }

        _sessionRpgGearUnlockState.PartyId = string.IsNullOrEmpty(partyId) ? _sessionRpgGearUnlockState.PartyId : partyId;
        _sessionRpgGearUnlockState.LastRunIdentity = string.IsNullOrEmpty(runIdentity) ? _sessionRpgGearUnlockState.LastRunIdentity : runIdentity;
        for (int i = 0; i < gearDefinitions.Length; i++)
        {
            PrototypeRpgGearDefinition definition = gearDefinitions[i];
            if (definition == null)
            {
                continue;
            }

            _sessionRpgGearUnlockState.UnlockedGearDefinitionIds = MergeRpgSummaryKeys(_sessionRpgGearUnlockState.UnlockedGearDefinitionIds, definition.GearDefinitionId);
            _sessionRpgGearUnlockState.RecentlySeenGearGroupKeys = MergeRpgSummaryKeys(_sessionRpgGearUnlockState.RecentlySeenGearGroupKeys, definition.GroupKey);
            _sessionRpgGearUnlockState.UnlockedTierKeys = MergeRpgSummaryKeys(_sessionRpgGearUnlockState.UnlockedTierKeys, definition.TierKey);
        }

        string recentGroupSummaryText = BuildRpgGearRecentGroupSummaryText(_sessionRpgGearUnlockState.RecentlySeenGearGroupKeys);
        _sessionRpgGearUnlockState.UnlockSummaryText = BuildRpgCompactSummaryText(
            "Unlock ceiling: " + ResolveRpgGearTierLabel(_sessionRpgGearUnlockState.HighestUnlockedTierKey),
            "Rarity ceiling: " + ResolveRpgGearRarityLabel(_sessionRpgGearUnlockState.HighestUnlockedRarityKey),
            "Level band: " + ResolveRpgGearLevelBandLabel(_sessionRpgGearUnlockState.HighestUnlockedLevelBandKey),
            recentGroupSummaryText);
        _sessionRpgGearUnlockState.UnlockedPoolSummaryText = BuildRpgCompactSummaryText(_sessionRpgGearUnlockState.UnlockedPoolSummaryText, recentGroupSummaryText);
    }

    private PrototypeRpgGearAffixLiteDefinition[] BuildRpgGearAffixDefinitions(PrototypeRpgEquipmentRewardCandidate[] candidates)
    {
        PrototypeRpgGearAffixLiteContribution[] contributions = BuildRpgRewardAffixLiteContributions(candidates);
        if (contributions == null || contributions.Length <= 0)
        {
            return Array.Empty<PrototypeRpgGearAffixLiteDefinition>();
        }

        List<PrototypeRpgGearAffixLiteDefinition> definitions = new List<PrototypeRpgGearAffixLiteDefinition>();
        for (int i = 0; i < contributions.Length; i++)
        {
            PrototypeRpgGearAffixLiteContribution contribution = contributions[i];
            if (contribution == null || string.IsNullOrEmpty(contribution.AffixId))
            {
                continue;
            }

            PrototypeRpgGearAffixLiteDefinition definition = PrototypeRpgGearAffixLiteCatalog.GetDefinition(contribution.AffixId);
            if (definition == null)
            {
                continue;
            }

            bool exists = false;
            for (int j = 0; j < definitions.Count; j++)
            {
                if (string.Equals(definitions[j].AffixId, definition.AffixId, StringComparison.Ordinal))
                {
                    exists = true;
                    break;
                }
            }

            if (!exists)
            {
                definitions.Add(definition);
            }
        }

        return definitions.Count > 0 ? definitions.ToArray() : Array.Empty<PrototypeRpgGearAffixLiteDefinition>();
    }

    private PrototypeRpgGearAffixLiteContribution[] BuildRpgRewardAffixLiteContributions(PrototypeRpgEquipmentRewardCandidate[] candidates)
    {
        if (candidates == null || candidates.Length <= 0)
        {
            return Array.Empty<PrototypeRpgGearAffixLiteContribution>();
        }

        PrototypeRpgPartyDefinition partyDefinition = _activeDungeonParty != null ? _activeDungeonParty.PartyDefinition : null;
        PrototypeRpgAppliedPartyProgressState appliedProgressState = GetOrCreateRpgAppliedPartyProgressState(partyDefinition);
        PrototypeRpgPartyLevelRuntimeState partyLevelState = GetOrCreateRpgPartyLevelRuntimeState(partyDefinition, appliedProgressState);
        List<PrototypeRpgGearAffixLiteContribution> contributions = new List<PrototypeRpgGearAffixLiteContribution>();
        for (int i = 0; i < candidates.Length; i++)
        {
            PrototypeRpgEquipmentRewardCandidate candidate = candidates[i];
            if (candidate == null)
            {
                continue;
            }

            PrototypeRpgPartyMemberDefinition memberDefinition = FindRpgPartyMemberDefinition(partyDefinition, candidate.MemberId);
            PrototypeRpgPartyMemberLevelRuntimeState memberLevelState = GetRpgMemberLevelRuntimeState(partyLevelState, candidate.MemberId);
            int level = memberLevelState != null && memberLevelState.Experience != null ? Mathf.Max(1, memberLevelState.Experience.Level) : 1;
            string levelBandKey = ResolveRpgGearLevelBandKey(level);
            PrototypeRpgEquipmentLoadoutDefinition loadoutDefinition = PrototypeRpgEquipmentCatalog.ResolveDefinition(candidate.EquipmentLoadoutId, memberDefinition != null ? memberDefinition.RoleTag : string.Empty);
            PrototypeRpgGearAffixLiteContribution[] memberContributions = BuildRpgGearAffixLiteContributions(memberDefinition != null ? memberDefinition.RoleTag : string.Empty, loadoutDefinition, levelBandKey, _sessionRpgGearUnlockState, true);
            for (int j = 0; j < memberContributions.Length; j++)
            {
                PrototypeRpgGearAffixLiteContribution contribution = memberContributions[j];
                if (contribution == null || string.IsNullOrEmpty(contribution.AffixId))
                {
                    continue;
                }

                bool exists = false;
                for (int k = 0; k < contributions.Count; k++)
                {
                    if (string.Equals(contributions[k].AffixId, contribution.AffixId, StringComparison.Ordinal))
                    {
                        exists = true;
                        break;
                    }
                }

                if (!exists)
                {
                    contributions.Add(contribution);
                }
            }
        }

        return contributions.Count > 0 ? contributions.ToArray() : Array.Empty<PrototypeRpgGearAffixLiteContribution>();
    }

    private PrototypeRpgGearAffixLiteContribution[] BuildRpgGearAffixLiteContributions(string roleTag, PrototypeRpgEquipmentLoadoutDefinition loadoutDefinition, string levelBandKey, PrototypeRpgGearUnlockState unlockState, bool candidateBias)
    {
        if (loadoutDefinition == null)
        {
            return Array.Empty<PrototypeRpgGearAffixLiteContribution>();
        }

        List<string> affixIds = new List<string>();
        string slotKey = loadoutDefinition.Items != null && loadoutDefinition.Items.Length > 0 && loadoutDefinition.Items[0] != null
            ? loadoutDefinition.Items[0].SlotKey
            : string.Empty;
        int tierRank = GetRpgTierRank(loadoutDefinition.TierKey);
        switch (string.IsNullOrEmpty(roleTag) ? string.Empty : roleTag)
        {
            case "warrior":
                affixIds.Add(slotKey == "weapon" ? "flat_attack_small" : slotKey == "shield" ? "flat_defense_small" : "flat_max_hp_small");
                if (tierRank >= 2 || (candidateBias && GetRpgLevelBandRank(levelBandKey) >= 3))
                {
                    affixIds.Add(slotKey == "weapon" ? "flat_max_hp_small" : "flat_defense_small");
                }
                break;
            case "rogue":
                affixIds.Add(slotKey == "armor" || slotKey == "utility" ? "flat_speed_small" : "flat_attack_small");
                if (tierRank >= 2 || (candidateBias && GetRpgLevelBandRank(levelBandKey) >= 3))
                {
                    affixIds.Add(slotKey == "armor" ? "flat_attack_small" : "flat_speed_small");
                }
                break;
            case "mage":
                affixIds.Add(slotKey == "focus" ? "skill_power_hint_small" : "flat_defense_small");
                if (tierRank >= 2 || (candidateBias && GetRpgLevelBandRank(levelBandKey) >= 3))
                {
                    affixIds.Add(slotKey == "focus" ? "flat_attack_small" : "flat_max_hp_small");
                }
                break;
            case "cleric":
                affixIds.Add(slotKey == "relic" ? "heal_power_hint_small" : "flat_defense_small");
                if (tierRank >= 2 || (candidateBias && GetRpgLevelBandRank(levelBandKey) >= 3))
                {
                    affixIds.Add(slotKey == "relic" ? "flat_max_hp_small" : "heal_power_hint_small");
                }
                break;
            default:
                affixIds.Add("flat_max_hp_small");
                break;
        }

        List<PrototypeRpgGearAffixLiteContribution> contributions = new List<PrototypeRpgGearAffixLiteContribution>();
        for (int i = 0; i < affixIds.Count; i++)
        {
            string affixId = affixIds[i];
            bool exists = false;
            for (int j = 0; j < contributions.Count; j++)
            {
                if (string.Equals(contributions[j].AffixId, affixId, StringComparison.Ordinal))
                {
                    exists = true;
                    break;
                }
            }

            if (exists)
            {
                continue;
            }

            PrototypeRpgGearAffixLiteDefinition definition = PrototypeRpgGearAffixLiteCatalog.GetDefinition(affixId);
            if (definition == null)
            {
                continue;
            }

            contributions.Add(CreateRpgGearAffixLiteContribution(definition));
        }

        return contributions.Count > 0 ? contributions.ToArray() : Array.Empty<PrototypeRpgGearAffixLiteContribution>();
    }

    private PrototypeRpgGearAffixLiteContribution CreateRpgGearAffixLiteContribution(PrototypeRpgGearAffixLiteDefinition definition)
    {
        PrototypeRpgGearAffixLiteContribution contribution = new PrototypeRpgGearAffixLiteContribution();
        if (definition == null)
        {
            return contribution;
        }

        contribution.AffixId = string.IsNullOrEmpty(definition.AffixId) ? string.Empty : definition.AffixId;
        contribution.DisplayLabel = string.IsNullOrEmpty(definition.DisplayLabel) ? "Affix" : definition.DisplayLabel;
        contribution.BonusMaxHp = definition.BonusMaxHp;
        contribution.BonusAttack = definition.BonusAttack;
        contribution.BonusDefense = definition.BonusDefense;
        contribution.BonusSpeed = definition.BonusSpeed;
        contribution.HintText = BuildRpgCompactSummaryText(definition.SkillPowerHintText, definition.HealPowerHintText);
        contribution.SummaryText = BuildRpgCompactSummaryText(definition.DisplayLabel, definition.SummaryText, contribution.HintText);
        return contribution;
    }

    private PrototypeRpgGearDefinition[] BuildRpgGearDefinitions(PrototypeRpgEquipmentRewardDefinition[] rewardDefinitions)
    {
        if (rewardDefinitions == null || rewardDefinitions.Length <= 0)
        {
            return Array.Empty<PrototypeRpgGearDefinition>();
        }

        PrototypeRpgGearDefinition[] definitions = new PrototypeRpgGearDefinition[rewardDefinitions.Length];
        for (int i = 0; i < rewardDefinitions.Length; i++)
        {
            PrototypeRpgEquipmentRewardDefinition rewardDefinition = rewardDefinitions[i];
            PrototypeRpgGearDefinition gearDefinition = new PrototypeRpgGearDefinition();
            if (rewardDefinition != null)
            {
                gearDefinition.GearDefinitionId = string.IsNullOrEmpty(rewardDefinition.GearDefinitionId) ? string.Empty : rewardDefinition.GearDefinitionId;
                gearDefinition.EquipmentLoadoutId = string.IsNullOrEmpty(rewardDefinition.EquipmentLoadoutId) ? string.Empty : rewardDefinition.EquipmentLoadoutId;
                gearDefinition.SlotKey = string.IsNullOrEmpty(rewardDefinition.SlotKey) ? string.Empty : rewardDefinition.SlotKey;
                gearDefinition.TierKey = string.IsNullOrEmpty(rewardDefinition.TierKey) ? string.Empty : rewardDefinition.TierKey;
                gearDefinition.RarityKey = string.IsNullOrEmpty(rewardDefinition.RarityKey) ? string.Empty : rewardDefinition.RarityKey;
                gearDefinition.UnlockStateKey = string.IsNullOrEmpty(rewardDefinition.UnlockStateKey) ? string.Empty : rewardDefinition.UnlockStateKey;
                gearDefinition.GroupKey = string.IsNullOrEmpty(rewardDefinition.GearGroupKey) ? string.Empty : rewardDefinition.GearGroupKey;
                gearDefinition.DisplayLabel = string.IsNullOrEmpty(rewardDefinition.DisplayLabel) ? string.Empty : rewardDefinition.DisplayLabel;
                gearDefinition.EffectPreviewText = string.IsNullOrEmpty(rewardDefinition.EffectPreviewText) ? string.Empty : rewardDefinition.EffectPreviewText;
                gearDefinition.ComparisonHintText = BuildRpgCompactSummaryText(
                    "Slot: " + ResolveRpgGearSlotLabel(rewardDefinition.SlotKey),
                    "Tier: " + ResolveRpgGearTierLabel(rewardDefinition.TierKey),
                    "Rarity: " + ResolveRpgGearRarityLabel(rewardDefinition.RarityKey));
            }

            definitions[i] = gearDefinition;
        }

        return definitions;
    }

    private PrototypeRpgGearComparisonSummary[] BuildRpgGearComparisonSummaries(PrototypeRpgEquipmentRewardCandidate[] candidates)
    {
        if (candidates == null || candidates.Length <= 0)
        {
            return Array.Empty<PrototypeRpgGearComparisonSummary>();
        }

        PrototypeRpgGearComparisonSummary[] summaries = new PrototypeRpgGearComparisonSummary[candidates.Length];
        for (int i = 0; i < candidates.Length; i++)
        {
            PrototypeRpgEquipmentRewardCandidate candidate = candidates[i];
            PrototypeRpgGearComparisonSummary summary = new PrototypeRpgGearComparisonSummary();
            if (candidate != null)
            {
                summary.MemberId = string.IsNullOrEmpty(candidate.MemberId) ? string.Empty : candidate.MemberId;
                summary.MemberDisplayName = string.IsNullOrEmpty(candidate.MemberDisplayName) ? string.Empty : candidate.MemberDisplayName;
                summary.CurrentEquipmentLoadoutId = string.IsNullOrEmpty(candidate.CurrentEquipmentLoadoutId) ? string.Empty : candidate.CurrentEquipmentLoadoutId;
                summary.CurrentEquipmentDisplayLabel = string.IsNullOrEmpty(candidate.CurrentEquipmentDisplayLabel) ? string.Empty : candidate.CurrentEquipmentDisplayLabel;
                summary.CandidateEquipmentLoadoutId = string.IsNullOrEmpty(candidate.EquipmentLoadoutId) ? string.Empty : candidate.EquipmentLoadoutId;
                summary.CandidateEquipmentDisplayLabel = string.IsNullOrEmpty(candidate.DisplayLabel) ? string.Empty : candidate.DisplayLabel;
                summary.SlotKey = string.IsNullOrEmpty(candidate.SlotKey) ? string.Empty : candidate.SlotKey;
                summary.TierKey = string.IsNullOrEmpty(candidate.TierKey) ? string.Empty : candidate.TierKey;
                summary.RarityKey = string.IsNullOrEmpty(candidate.RarityKey) ? string.Empty : candidate.RarityKey;
                summary.StatDeltaPreviewText = string.IsNullOrEmpty(candidate.DerivedStatDeltaSummaryText) ? string.Empty : candidate.DerivedStatDeltaSummaryText;
                summary.SkillHintDeltaText = BuildRpgCompactSummaryText(candidate.SkillHintText, candidate.RoleHintText);
                summary.SwapReasonText = candidate.IsRecommended ? "Upgrade ready for next run." : "No slot upgrade needed.";
                summary.AffixLiteSummaryText = string.IsNullOrEmpty(candidate.AffixLiteSummaryText) ? string.Empty : candidate.AffixLiteSummaryText;
                summary.ComparisonHintText = string.IsNullOrEmpty(candidate.ComparisonSummaryText)
                    ? BuildRpgGearComparisonDetailText(candidate.CurrentEquipmentDisplayLabel, candidate.DisplayLabel, candidate.SlotKey, candidate.TierKey, candidate.RarityKey, candidate.DerivedStatDeltaSummaryText, candidate.SkillHintText, candidate.RoleHintText, candidate.AffixLiteSummaryText)
                    : candidate.ComparisonSummaryText;
                summary.SummaryText = summary.ComparisonHintText;
            }

            summaries[i] = summary;
        }

        return summaries;
    }

    private PrototypeRpgGearSlotRule[] BuildRpgGearSlotRules(PrototypeRpgEquipmentRewardCandidate[] candidates, PrototypeRpgGearUnlockState unlockState)
    {
        if (candidates == null || candidates.Length <= 0)
        {
            return Array.Empty<PrototypeRpgGearSlotRule>();
        }

        PrototypeRpgGearSlotRule[] rules = new PrototypeRpgGearSlotRule[candidates.Length];
        for (int i = 0; i < candidates.Length; i++)
        {
            PrototypeRpgEquipmentRewardCandidate candidate = candidates[i];
            PrototypeRpgGearSlotRule rule = new PrototypeRpgGearSlotRule();
            if (candidate != null)
            {
                rule.SlotKey = string.IsNullOrEmpty(candidate.SlotKey) ? string.Empty : candidate.SlotKey;
                rule.SlotLabel = ResolveRpgGearSlotLabel(candidate.SlotKey);
                rule.CurrentEquipmentLoadoutId = string.IsNullOrEmpty(candidate.CurrentEquipmentLoadoutId) ? string.Empty : candidate.CurrentEquipmentLoadoutId;
                rule.CurrentEquipmentDisplayLabel = string.IsNullOrEmpty(candidate.CurrentEquipmentDisplayLabel) ? string.Empty : candidate.CurrentEquipmentDisplayLabel;
                rule.CandidateEquipmentLoadoutId = string.IsNullOrEmpty(candidate.EquipmentLoadoutId) ? string.Empty : candidate.EquipmentLoadoutId;
                rule.CandidateEquipmentDisplayLabel = string.IsNullOrEmpty(candidate.DisplayLabel) ? string.Empty : candidate.DisplayLabel;
                rule.AllowedTierKey = unlockState != null && !string.IsNullOrEmpty(unlockState.HighestUnlockedTierKey) ? unlockState.HighestUnlockedTierKey : candidate.TierKey;
                rule.AllowedRarityKey = unlockState != null && !string.IsNullOrEmpty(unlockState.HighestUnlockedRarityKey) ? unlockState.HighestUnlockedRarityKey : candidate.RarityKey;
                rule.ConflictSummaryText = BuildRpgGearSlotRuleDetailText(candidate.CurrentEquipmentDisplayLabel, candidate.DisplayLabel, candidate.SlotKey, unlockState);
                rule.ReplacementSummaryText = "Replace " + candidate.CurrentEquipmentDisplayLabel + " with " + candidate.DisplayLabel + ".";
                rule.SummaryText = BuildRpgCompactSummaryText(
                    rule.ConflictSummaryText,
                    "Allowed: " + ResolveRpgGearTierLabel(rule.AllowedTierKey),
                    "Rarity ceiling: " + ResolveRpgGearRarityLabel(rule.AllowedRarityKey));
            }

            rules[i] = rule;
        }

        return rules;
    }

    private string BuildRpgGearRecentGroupSummaryText(string[] groupKeys)
    {
        if (groupKeys == null || groupKeys.Length <= 0)
        {
            return string.Empty;
        }

        List<string> slotLabels = new List<string>();
        for (int i = 0; i < groupKeys.Length; i++)
        {
            string groupKey = groupKeys[i];
            if (string.IsNullOrEmpty(groupKey))
            {
                continue;
            }

            string[] parts = groupKey.Split(':');
            string slotKey = parts.Length > 1 ? parts[1] : groupKey;
            AddRpgSummaryPart(slotLabels, ResolveRpgGearSlotLabel(slotKey));
        }

        return slotLabels.Count <= 0 ? string.Empty : "Recent families: " + string.Join(", ", slotLabels.ToArray());
    }

    private string BuildRpgGearAffixLiteCandidateSummaryText(PrototypeRpgGearAffixLiteContribution[] affixes)
    {
        string summaryText = BuildRpgGearAffixLiteSummaryText(affixes);
        return string.IsNullOrEmpty(summaryText) ? string.Empty : "Affix-lite: " + summaryText;
    }

    private string BuildRpgGearAffixLiteSummaryText(PrototypeRpgGearAffixLiteContribution[] affixes)
    {
        if (affixes == null || affixes.Length <= 0)
        {
            return string.Empty;
        }

        List<string> parts = new List<string>();
        for (int i = 0; i < affixes.Length; i++)
        {
            PrototypeRpgGearAffixLiteContribution affix = affixes[i];
            if (affix == null)
            {
                continue;
            }

            AddRpgSummaryPart(parts, affix.SummaryText);
        }

        return parts.Count <= 0 ? string.Empty : string.Join(" | ", parts.ToArray());
    }

    private string BuildRpgUnlockedGearPoolSummaryText(string roleTag, string levelBandKey, PrototypeRpgGearUnlockState unlockState, string sourceKey)
    {
        string displayRole = string.Equals(roleTag, "party", StringComparison.Ordinal)
            ? "party pool"
            : string.IsNullOrEmpty(roleTag) ? "mixed pool" : roleTag;
        string tierKey = ResolveRpgGearRewardTier(sourceKey, unlockState, levelBandKey);
        string[] pool = string.Equals(roleTag, "party", StringComparison.Ordinal)
            ? Array.Empty<string>()
            : ResolveUnlockedGearPool(roleTag, string.Empty, sourceKey, tierKey, unlockState);
        List<string> labels = new List<string>();
        for (int i = 0; i < pool.Length; i++)
        {
            PrototypeRpgEquipmentLoadoutDefinition definition = PrototypeRpgEquipmentCatalog.ResolveDefinition(pool[i], roleTag);
            if (definition == null)
            {
                continue;
            }

            AddRpgSummaryPart(labels, definition.DisplayName);
            if (labels.Count >= 3)
            {
                break;
            }
        }

        return BuildRpgCompactSummaryText(
            "Unlocked pool: " + displayRole + " / " + ResolveRpgGearLevelBandLabel(levelBandKey),
            "Pool ceiling: " + ResolveRpgGearTierLabel(tierKey),
            labels.Count > 0 ? "Pool sample: " + string.Join(", ", labels.ToArray()) : string.Empty);
    }

    private string BuildRpgLevelBandGearLinkageSummaryText(PrototypeRpgEquipmentRewardCandidate[] candidates)
    {
        if (candidates == null || candidates.Length <= 0)
        {
            return string.Empty;
        }

        List<string> parts = new List<string>();
        for (int i = 0; i < candidates.Length; i++)
        {
            PrototypeRpgEquipmentRewardCandidate candidate = candidates[i];
            if (candidate == null)
            {
                continue;
            }

            AddRpgSummaryPart(parts, candidate.LevelBandSummaryText);
            string tierLabel = ResolveRpgGearTierLabel(candidate.TierKey);
            string slotLabel = ResolveRpgGearSlotLabel(candidate.SlotKey);
            AddRpgSummaryPart(parts, "Linkage: " + tierLabel + " " + slotLabel + " candidate unlocked");
        }

        return parts.Count <= 0 ? string.Empty : string.Join(" | ", parts.ToArray());
    }

    private string BuildRpgGearUnlockCandidateSummaryText(PrototypeRpgEquipmentRewardCandidate candidate, PrototypeRpgGearUnlockState unlockState)
    {
        if (candidate == null)
        {
            return string.Empty;
        }

        string unlockedTierKey = unlockState != null && !string.IsNullOrEmpty(unlockState.HighestUnlockedTierKey) ? unlockState.HighestUnlockedTierKey : candidate.TierKey;
        string unlockedRarityKey = unlockState != null && !string.IsNullOrEmpty(unlockState.HighestUnlockedRarityKey) ? unlockState.HighestUnlockedRarityKey : candidate.RarityKey;
        return BuildRpgCompactSummaryText(
            "Unlock key: " + candidate.UnlockStateKey,
            "Ceiling: " + ResolveRpgGearTierLabel(unlockedTierKey),
            "Rarity ceiling: " + ResolveRpgGearRarityLabel(unlockedRarityKey),
            candidate.LevelBandSummaryText,
            candidate.UnlockedPoolSummaryText);
    }

    private string BuildRpgGearComparisonDetailText(string currentDisplayLabel, string candidateDisplayLabel, string slotKey, string tierKey, string rarityKey, string statDeltaPreviewText, string skillHintText, string roleHintText, string affixLiteSummaryText)
    {
        return BuildRpgCompactSummaryText(
            "Compare: " + currentDisplayLabel + " -> " + candidateDisplayLabel,
            "Slot: " + ResolveRpgGearSlotLabel(slotKey),
            "Tier: " + ResolveRpgGearTierLabel(tierKey),
            "Rarity: " + ResolveRpgGearRarityLabel(rarityKey),
            statDeltaPreviewText,
            affixLiteSummaryText,
            skillHintText,
            roleHintText);
    }

    private string BuildRpgGearSlotRuleDetailText(string currentDisplayLabel, string candidateDisplayLabel, string slotKey, PrototypeRpgGearUnlockState unlockState)
    {
        string allowedTierKey = unlockState != null && !string.IsNullOrEmpty(unlockState.HighestUnlockedTierKey) ? unlockState.HighestUnlockedTierKey : "tier_1";
        return BuildRpgCompactSummaryText(
            "Slot rule: " + ResolveRpgGearSlotLabel(slotKey) + " keeps one active loadout",
            "Replace: " + currentDisplayLabel + " -> " + candidateDisplayLabel,
            "Unlocked up to " + ResolveRpgGearTierLabel(allowedTierKey));
    }

    private string ResolveRpgBaseUnlockedTierForSource(string sourceKey)
    {
        switch (sourceKey)
        {
            case "elite_reward":
                return "tier_3";
            case "chest_reward":
            case "risky_route":
                return "tier_2";
            case "defeat_recovery":
            case "retreat_recovery":
            case "run_clear":
            default:
                return "tier_1";
        }
    }

    private string BuildRpgGearUnlockStateKey(PrototypeRpgGearUnlockState unlockState, string tierKey, string rarityKey)
    {
        string highestTierKey = unlockState != null && !string.IsNullOrEmpty(unlockState.HighestUnlockedTierKey) ? unlockState.HighestUnlockedTierKey : tierKey;
        string highestRarityKey = unlockState != null && !string.IsNullOrEmpty(unlockState.HighestUnlockedRarityKey) ? unlockState.HighestUnlockedRarityKey : rarityKey;
        return "gear_unlock_" + highestTierKey + "_" + highestRarityKey;
    }

    private string BuildRpgGearGroupKey(string roleTag, string slotKey, string tierKey)
    {
        return (string.IsNullOrEmpty(roleTag) ? "party" : roleTag) + ":" + (string.IsNullOrEmpty(slotKey) ? "gear" : slotKey) + ":" + (string.IsNullOrEmpty(tierKey) ? "core" : tierKey);
    }

    private void EnsureRpgGearUnlockTierRange(PrototypeRpgGearUnlockState unlockState, string highestTierKey)
    {
        if (unlockState == null)
        {
            return;
        }

        unlockState.UnlockedTierKeys = MergeRpgSummaryKeys(unlockState.UnlockedTierKeys, "core");
        if (GetRpgTierRank(highestTierKey) >= 1)
        {
            unlockState.UnlockedTierKeys = MergeRpgSummaryKeys(unlockState.UnlockedTierKeys, "tier_1");
        }

        if (GetRpgTierRank(highestTierKey) >= 2)
        {
            unlockState.UnlockedTierKeys = MergeRpgSummaryKeys(unlockState.UnlockedTierKeys, "tier_2");
        }

        if (GetRpgTierRank(highestTierKey) >= 3)
        {
            unlockState.UnlockedTierKeys = MergeRpgSummaryKeys(unlockState.UnlockedTierKeys, "tier_3");
        }
    }

    private string[] MergeRpgSummaryKeys(string[] keys, string value)
    {
        if (string.IsNullOrEmpty(value))
        {
            return keys ?? Array.Empty<string>();
        }

        List<string> merged = new List<string>(keys ?? Array.Empty<string>());
        for (int i = 0; i < merged.Count; i++)
        {
            if (string.Equals(merged[i], value, StringComparison.Ordinal))
            {
                return merged.ToArray();
            }
        }

        merged.Add(value);
        return merged.ToArray();
    }

    private PrototypeRpgEquipSwapChoice[] BuildRpgEquipSwapChoices(PrototypeRpgEquipmentRewardCandidate[] candidates)
    {
        if (candidates == null || candidates.Length <= 0)
        {
            return Array.Empty<PrototypeRpgEquipSwapChoice>();
        }

        List<PrototypeRpgEquipSwapChoice> choices = new List<PrototypeRpgEquipSwapChoice>();
        for (int i = 0; i < candidates.Length; i++)
        {
            PrototypeRpgEquipmentRewardCandidate candidate = candidates[i];
            if (candidate == null)
            {
                continue;
            }

            PrototypeRpgEquipSwapChoice choice = new PrototypeRpgEquipSwapChoice();
            choice.RewardId = candidate.RewardId;
            choice.MemberId = candidate.MemberId;
            choice.MemberDisplayName = candidate.MemberDisplayName;
            choice.CurrentEquipmentLoadoutId = candidate.CurrentEquipmentLoadoutId;
            choice.CurrentEquipmentDisplayLabel = candidate.CurrentEquipmentDisplayLabel;
            choice.TargetEquipmentLoadoutId = candidate.EquipmentLoadoutId;
            choice.TargetEquipmentDisplayLabel = candidate.DisplayLabel;
            choice.SlotKey = candidate.SlotKey;
            choice.ReplacementSummaryText = "Replace " + choice.CurrentEquipmentDisplayLabel + " with " + choice.TargetEquipmentDisplayLabel + ".";
            choice.ComparisonSummaryText = candidate.ComparisonSummaryText;
            choice.AffixLiteSummaryText = candidate.AffixLiteSummaryText;
            choice.EffectPreviewText = candidate.EffectPreviewText;
            choice.HasChanges = !string.IsNullOrEmpty(candidate.EquipmentLoadoutId) && !string.Equals(candidate.CurrentEquipmentLoadoutId, candidate.EquipmentLoadoutId, StringComparison.Ordinal);
            choice.ApplyReady = candidate.IsRecommended || choice.HasChanges;
            choice.ContinuitySummaryText = choice.HasChanges
                ? BuildRpgCompactSummaryText(choice.MemberDisplayName + " next run gear shifts to " + choice.TargetEquipmentDisplayLabel + ".", "Slot: " + ResolveRpgGearSlotLabel(choice.SlotKey))
                : BuildRpgCompactSummaryText(choice.MemberDisplayName + " keeps " + choice.TargetEquipmentDisplayLabel + " for next run continuity.", "Slot: " + ResolveRpgGearSlotLabel(choice.SlotKey));
            choice.SummaryText = choice.HasChanges
                ? BuildRpgCompactSummaryText("Equip swap: " + choice.MemberDisplayName, choice.ReplacementSummaryText, "Locked for next run.")
                : BuildRpgCompactSummaryText("Equip swap: " + choice.MemberDisplayName + " keeps " + choice.TargetEquipmentDisplayLabel + ".", "Slot: " + ResolveRpgGearSlotLabel(choice.SlotKey));
            choices.Add(choice);
        }

        return choices.Count > 0 ? choices.ToArray() : Array.Empty<PrototypeRpgEquipSwapChoice>();
    }

    private void CommitRpgEquipSwapChoices(PrototypeRpgRuntimeGearInventoryState gearState, PrototypeRpgAppliedPartyProgressState appliedProgressState, PrototypeRpgPartyDefinition partyDefinition)
    {
        if (gearState == null || appliedProgressState == null || gearState.EquipSwapChoices == null)
        {
            return;
        }

        for (int i = 0; i < gearState.EquipSwapChoices.Length; i++)
        {
            PrototypeRpgEquipSwapChoice choice = gearState.EquipSwapChoices[i];
            if (choice == null || string.IsNullOrEmpty(choice.MemberId) || string.IsNullOrEmpty(choice.TargetEquipmentLoadoutId))
            {
                continue;
            }

            PrototypeRpgAppliedPartyMemberProgressState memberState = GetRpgAppliedPartyMemberProgressState(appliedProgressState, choice.MemberId);
            if (memberState == null)
            {
                continue;
            }

            if (choice.ApplyReady)
            {
                memberState.AppliedEquipmentLoadoutId = choice.TargetEquipmentLoadoutId;
                memberState.LastAppliedRunIdentity = string.IsNullOrEmpty(gearState.LastRunIdentity) ? memberState.LastAppliedRunIdentity : gearState.LastRunIdentity;
                memberState.LastAppliedSummaryText = string.IsNullOrEmpty(choice.SummaryText) ? memberState.LastAppliedSummaryText : choice.SummaryText;
                gearState.LastAppliedRewardId = string.IsNullOrEmpty(choice.RewardId) ? gearState.LastAppliedRewardId : choice.RewardId;
            }
        }

        appliedProgressState.AppliedSummaryText = BuildRpgAppliedPartySummaryText(appliedProgressState, partyDefinition);
    }

    private string BuildRpgGearRewardCandidateSummaryText(PrototypeRpgEquipmentRewardCandidate[] candidates)
    {
        if (candidates == null || candidates.Length <= 0)
        {
            return string.Empty;
        }

        List<string> parts = new List<string>();
        for (int i = 0; i < candidates.Length; i++)
        {
            PrototypeRpgEquipmentRewardCandidate candidate = candidates[i];
            if (candidate != null && !string.IsNullOrEmpty(candidate.SummaryText))
            {
                AddRpgSummaryPart(parts, candidate.SummaryText);
            }
        }

        return parts.Count <= 0 ? string.Empty : string.Join(" | ", parts.ToArray());
    }

    private string BuildRpgEquipSwapSummaryText(PrototypeRpgEquipSwapChoice[] choices)
    {
        if (choices == null || choices.Length <= 0)
        {
            return string.Empty;
        }

        List<string> parts = new List<string>();
        for (int i = 0; i < choices.Length; i++)
        {
            PrototypeRpgEquipSwapChoice choice = choices[i];
            if (choice != null && !string.IsNullOrEmpty(choice.SummaryText))
            {
                AddRpgSummaryPart(parts, choice.SummaryText);
            }
        }

        return parts.Count <= 0 ? string.Empty : string.Join(" | ", parts.ToArray());
    }

    private string BuildRpgGearRuleSummaryText(PrototypeRpgGearSlotRule[] rules)
    {
        if (rules == null || rules.Length <= 0)
        {
            return string.Empty;
        }

        List<string> parts = new List<string>();
        for (int i = 0; i < rules.Length; i++)
        {
            PrototypeRpgGearSlotRule rule = rules[i];
            if (rule != null && !string.IsNullOrEmpty(rule.SummaryText))
            {
                AddRpgSummaryPart(parts, rule.SummaryText);
            }
        }

        return parts.Count <= 0 ? string.Empty : string.Join(" | ", parts.ToArray());
    }

    private string BuildRpgGearComparisonSummaryText(PrototypeRpgGearComparisonSummary[] comparisons)
    {
        if (comparisons == null || comparisons.Length <= 0)
        {
            return string.Empty;
        }

        List<string> parts = new List<string>();
        for (int i = 0; i < comparisons.Length; i++)
        {
            PrototypeRpgGearComparisonSummary comparison = comparisons[i];
            if (comparison != null && !string.IsNullOrEmpty(comparison.SummaryText))
            {
                AddRpgSummaryPart(parts, comparison.SummaryText);
            }
        }

        return parts.Count <= 0 ? string.Empty : string.Join(" | ", parts.ToArray());
    }

    private string BuildRpgGearCarryContinuitySummaryText(PrototypeRpgPartyDefinition partyDefinition, PrototypeRpgAppliedPartyProgressState appliedProgressState, PrototypeRpgPartyLevelRuntimeState partyLevelState, PrototypeRpgInventoryRuntimeState inventoryState)
    {
        string appliedSummaryText = BuildRpgAppliedPartySummaryText(appliedProgressState, partyDefinition);
        string equipmentSummaryText = partyLevelState != null ? partyLevelState.EquipmentLoadoutReadbackSummaryText : string.Empty;
        string gearContributionSummaryText = partyLevelState != null ? partyLevelState.GearContributionSummaryText : string.Empty;
        string inventorySummaryText = inventoryState != null ? inventoryState.EquipmentSummaryText : string.Empty;
        return BuildRpgCompactSummaryText(appliedSummaryText, equipmentSummaryText, gearContributionSummaryText, inventorySummaryText);
    }

    private void ApplyRpgGearRewardReadbackConsistency(PrototypeRpgRunResultSnapshot runResultSnapshot, PrototypeRpgPartyLevelRuntimeState partyLevelState, PrototypeRpgInventoryRuntimeState inventoryState, PrototypeRpgRuntimeGearInventoryState gearState)
    {
        if (runResultSnapshot == null || gearState == null)
        {
            return;
        }

        string candidateSummaryText = string.IsNullOrEmpty(gearState.RewardCandidateSummaryText) ? string.Empty : gearState.RewardCandidateSummaryText;
        string equipSwapSummaryText = string.IsNullOrEmpty(gearState.EquipSwapSummaryText) ? string.Empty : gearState.EquipSwapSummaryText;
        string continuitySummaryText = string.IsNullOrEmpty(gearState.GearCarryContinuitySummaryText) ? string.Empty : gearState.GearCarryContinuitySummaryText;
        string ruleSummaryText = string.IsNullOrEmpty(gearState.GearRuleSummaryText) ? string.Empty : gearState.GearRuleSummaryText;
        string unlockSummaryText = string.IsNullOrEmpty(gearState.GearUnlockSummaryText) ? string.Empty : gearState.GearUnlockSummaryText;
        string comparisonSummaryText = string.IsNullOrEmpty(gearState.GearComparisonSummaryText) ? string.Empty : gearState.GearComparisonSummaryText;
        string affixSummaryText = string.IsNullOrEmpty(gearState.GearAffixLiteSummaryText) ? string.Empty : gearState.GearAffixLiteSummaryText;
        string unlockedPoolSummaryText = string.IsNullOrEmpty(gearState.UnlockedGearPoolSummaryText) ? string.Empty : gearState.UnlockedGearPoolSummaryText;
        string levelBandLinkageSummaryText = string.IsNullOrEmpty(gearState.LevelBandGearLinkageSummaryText) ? string.Empty : gearState.LevelBandGearLinkageSummaryText;

        runResultSnapshot.GearRewardCandidateSummaryText = candidateSummaryText;
        runResultSnapshot.EquipSwapChoiceSummaryText = equipSwapSummaryText;
        runResultSnapshot.GearCarryContinuitySummaryText = continuitySummaryText;
        runResultSnapshot.GearRuleSummaryText = ruleSummaryText;
        runResultSnapshot.GearUnlockSummaryText = unlockSummaryText;
        runResultSnapshot.GearComparisonSummaryText = comparisonSummaryText;
        runResultSnapshot.GearAffixLiteSummaryText = affixSummaryText;
        runResultSnapshot.UnlockedGearPoolSummaryText = unlockedPoolSummaryText;
        runResultSnapshot.LevelBandGearLinkageSummaryText = levelBandLinkageSummaryText;
        runResultSnapshot.CurrentAppliedIdentitySummaryText = CurrentAppliedIdentitySummaryText;

        if (partyLevelState != null)
        {
            if (string.IsNullOrEmpty(runResultSnapshot.EquipmentLoadoutReadbackSummaryText))
            {
                runResultSnapshot.EquipmentLoadoutReadbackSummaryText = partyLevelState.EquipmentLoadoutReadbackSummaryText;
            }

            if (string.IsNullOrEmpty(runResultSnapshot.GearContributionSummaryText))
            {
                runResultSnapshot.GearContributionSummaryText = partyLevelState.GearContributionSummaryText;
            }
        }

        if (inventoryState != null && string.IsNullOrEmpty(runResultSnapshot.ConsumableCarrySummaryText))
        {
            runResultSnapshot.ConsumableCarrySummaryText = inventoryState.InventorySummaryText;
        }

        if (!string.IsNullOrEmpty(equipSwapSummaryText))
        {
            runResultSnapshot.ApplyTraceSummaryText = BuildRpgCompactSummaryText(runResultSnapshot.ApplyTraceSummaryText, equipSwapSummaryText);
        }

        string nextBasisSummaryText = BuildRpgCompactSummaryText(runResultSnapshot.NextOfferBasisSummaryText, comparisonSummaryText, affixSummaryText, unlockSummaryText, unlockedPoolSummaryText, levelBandLinkageSummaryText, continuitySummaryText);
        if (!string.IsNullOrEmpty(nextBasisSummaryText))
        {
            runResultSnapshot.NextOfferBasisSummaryText = nextBasisSummaryText;
        }

        if (runResultSnapshot.LootOutcome != null)
        {
            string rewardGrantSummaryText = BuildRpgCompactSummaryText(runResultSnapshot.LootOutcome.RewardGrantSummaryText, candidateSummaryText, comparisonSummaryText, affixSummaryText, equipSwapSummaryText);
            string finalLootSummaryText = BuildRpgCompactSummaryText(runResultSnapshot.LootOutcome.FinalLootSummary, candidateSummaryText, comparisonSummaryText, affixSummaryText, equipSwapSummaryText, ruleSummaryText, unlockSummaryText, unlockedPoolSummaryText, levelBandLinkageSummaryText, continuitySummaryText);
            if (!string.IsNullOrEmpty(rewardGrantSummaryText))
            {
                runResultSnapshot.LootOutcome.RewardGrantSummaryText = rewardGrantSummaryText;
            }

            if (!string.IsNullOrEmpty(finalLootSummaryText))
            {
                runResultSnapshot.LootOutcome.FinalLootSummary = finalLootSummaryText;
            }
        }
    }

    private void ApplyRpgGearRewardSurfaceConsistency(PrototypeRpgRunResultSnapshot runResultSnapshot, PrototypeRpgProgressionPreviewSnapshot previewSnapshot, PrototypeRpgPostRunUpgradeOfferSurface surface)
    {
        string candidateSummaryText = CurrentGearRewardCandidateSummaryText;
        string equipSwapSummaryText = CurrentEquipSwapChoiceSummaryText;
        string continuitySummaryText = CurrentGearCarryContinuitySummaryText;
        string ruleSummaryText = CurrentGearRuleSummaryText;
        string unlockSummaryText = CurrentGearUnlockSummaryText;
        string comparisonSummaryText = CurrentGearComparisonSummaryText;
        string affixSummaryText = CurrentGearAffixLiteSummaryText;
        string unlockedPoolSummaryText = CurrentUnlockedGearPoolSummaryText;
        string levelBandLinkageSummaryText = CurrentLevelBandGearLinkageSummaryText;

        if (string.Equals(candidateSummaryText, "None", StringComparison.Ordinal))
        {
            candidateSummaryText = string.Empty;
        }

        if (string.Equals(equipSwapSummaryText, "None", StringComparison.Ordinal))
        {
            equipSwapSummaryText = string.Empty;
        }

        if (string.Equals(continuitySummaryText, "None", StringComparison.Ordinal))
        {
            continuitySummaryText = string.Empty;
        }

        if (string.Equals(ruleSummaryText, "None", StringComparison.Ordinal))
        {
            ruleSummaryText = string.Empty;
        }

        if (string.Equals(unlockSummaryText, "None", StringComparison.Ordinal))
        {
            unlockSummaryText = string.Empty;
        }

        if (string.Equals(comparisonSummaryText, "None", StringComparison.Ordinal))
        {
            comparisonSummaryText = string.Empty;
        }

        if (string.Equals(affixSummaryText, "None", StringComparison.Ordinal))
        {
            affixSummaryText = string.Empty;
        }

        if (string.Equals(unlockedPoolSummaryText, "None", StringComparison.Ordinal))
        {
            unlockedPoolSummaryText = string.Empty;
        }

        if (string.Equals(levelBandLinkageSummaryText, "None", StringComparison.Ordinal))
        {
            levelBandLinkageSummaryText = string.Empty;
        }

        if (runResultSnapshot != null)
        {
            runResultSnapshot.CurrentAppliedIdentitySummaryText = CurrentAppliedIdentitySummaryText;
            runResultSnapshot.GearRewardCandidateSummaryText = candidateSummaryText;
            runResultSnapshot.EquipSwapChoiceSummaryText = equipSwapSummaryText;
            runResultSnapshot.GearCarryContinuitySummaryText = continuitySummaryText;
            runResultSnapshot.GearRuleSummaryText = ruleSummaryText;
            runResultSnapshot.GearUnlockSummaryText = unlockSummaryText;
            runResultSnapshot.GearComparisonSummaryText = comparisonSummaryText;
            runResultSnapshot.GearAffixLiteSummaryText = affixSummaryText;
            runResultSnapshot.UnlockedGearPoolSummaryText = unlockedPoolSummaryText;
            runResultSnapshot.LevelBandGearLinkageSummaryText = levelBandLinkageSummaryText;
            runResultSnapshot.ApplyTraceSummaryText = BuildRpgCompactSummaryText(runResultSnapshot.ApplyTraceSummaryText, equipSwapSummaryText);
            runResultSnapshot.NextOfferBasisSummaryText = BuildRpgCompactSummaryText(runResultSnapshot.NextOfferBasisSummaryText, comparisonSummaryText, affixSummaryText, unlockSummaryText, unlockedPoolSummaryText, levelBandLinkageSummaryText, continuitySummaryText);
            if (runResultSnapshot.LootOutcome != null)
            {
                runResultSnapshot.LootOutcome.RewardGrantSummaryText = BuildRpgCompactSummaryText(runResultSnapshot.LootOutcome.RewardGrantSummaryText, candidateSummaryText, comparisonSummaryText, affixSummaryText, equipSwapSummaryText);
                runResultSnapshot.LootOutcome.FinalLootSummary = BuildRpgCompactSummaryText(runResultSnapshot.LootOutcome.FinalLootSummary, candidateSummaryText, comparisonSummaryText, affixSummaryText, equipSwapSummaryText, ruleSummaryText, unlockSummaryText, unlockedPoolSummaryText, levelBandLinkageSummaryText, continuitySummaryText);
            }
        }

        if (previewSnapshot != null)
        {
            previewSnapshot.CurrentAppliedIdentitySummaryText = CurrentAppliedIdentitySummaryText;
            previewSnapshot.GearRewardCandidateSummaryText = candidateSummaryText;
            previewSnapshot.EquipSwapChoiceSummaryText = equipSwapSummaryText;
            previewSnapshot.GearCarryContinuitySummaryText = continuitySummaryText;
            previewSnapshot.GearRuleSummaryText = ruleSummaryText;
            previewSnapshot.GearUnlockSummaryText = unlockSummaryText;
            previewSnapshot.GearComparisonSummaryText = comparisonSummaryText;
            previewSnapshot.GearAffixLiteSummaryText = affixSummaryText;
            previewSnapshot.UnlockedGearPoolSummaryText = unlockedPoolSummaryText;
            previewSnapshot.LevelBandGearLinkageSummaryText = levelBandLinkageSummaryText;
            previewSnapshot.ApplyTraceSummaryText = BuildRpgCompactSummaryText(previewSnapshot.ApplyTraceSummaryText, equipSwapSummaryText);
            previewSnapshot.NextOfferBasisSummaryText = BuildRpgCompactSummaryText(previewSnapshot.NextOfferBasisSummaryText, comparisonSummaryText, affixSummaryText, unlockSummaryText, unlockedPoolSummaryText, levelBandLinkageSummaryText, continuitySummaryText);
            previewSnapshot.PreviewSummaryText = BuildRpgCompactSummaryText(previewSnapshot.PreviewSummaryText, comparisonSummaryText, affixSummaryText, unlockSummaryText, unlockedPoolSummaryText, levelBandLinkageSummaryText, continuitySummaryText);
        }

        if (surface != null)
        {
            surface.AppliedPartySummaryText = CurrentAppliedIdentitySummaryText;
            surface.ApplyReadySummaryText = BuildRpgCompactSummaryText(surface.ApplyReadySummaryText, equipSwapSummaryText);
            surface.OfferBasisSummaryText = BuildRpgCompactSummaryText(surface.OfferBasisSummaryText, comparisonSummaryText, affixSummaryText, unlockSummaryText, unlockedPoolSummaryText, levelBandLinkageSummaryText, continuitySummaryText);
            surface.OfferContinuitySummaryText = BuildRpgCompactSummaryText(surface.OfferContinuitySummaryText, ruleSummaryText, affixSummaryText, continuitySummaryText);
        }
    }

    private string BuildRpgCompactSummaryText(params string[] parts)
    {
        if (parts == null || parts.Length <= 0)
        {
            return string.Empty;
        }

        List<string> compactParts = new List<string>();
        for (int i = 0; i < parts.Length; i++)
        {
            AddRpgSummaryPart(compactParts, parts[i]);
        }

        return compactParts.Count <= 0 ? string.Empty : string.Join(" | ", compactParts.ToArray());
    }

    private string BuildRpgResultPanelSummaryOrNone(params string[] parts)
    {
        string summaryText = BuildRpgCompactSummaryText(parts);
        return string.IsNullOrEmpty(summaryText) ? "None" : summaryText;
    }

    private void AddRpgSummaryPart(List<string> parts, string text)
    {
        if (parts == null || string.IsNullOrEmpty(text) || string.Equals(text, "None", StringComparison.Ordinal))
        {
            return;
        }

        for (int i = 0; i < parts.Count; i++)
        {
            if (string.Equals(parts[i], text, StringComparison.Ordinal))
            {
                return;
            }
        }

        parts.Add(text);
    }

    private PrototypeRpgRuntimeGearInventoryState CopyRpgRuntimeGearInventoryState(PrototypeRpgRuntimeGearInventoryState source)
    {
        PrototypeRpgRuntimeGearInventoryState copy = new PrototypeRpgRuntimeGearInventoryState();
        if (source == null)
        {
            return copy;
        }

        copy.PartyId = string.IsNullOrEmpty(source.PartyId) ? string.Empty : source.PartyId;
        copy.DisplayName = string.IsNullOrEmpty(source.DisplayName) ? string.Empty : source.DisplayName;
        copy.LastRunIdentity = string.IsNullOrEmpty(source.LastRunIdentity) ? string.Empty : source.LastRunIdentity;
        copy.LastAppliedRewardId = string.IsNullOrEmpty(source.LastAppliedRewardId) ? string.Empty : source.LastAppliedRewardId;
        copy.RewardCandidateSummaryText = string.IsNullOrEmpty(source.RewardCandidateSummaryText) ? string.Empty : source.RewardCandidateSummaryText;
        copy.EquipSwapSummaryText = string.IsNullOrEmpty(source.EquipSwapSummaryText) ? string.Empty : source.EquipSwapSummaryText;
        copy.GearCarryContinuitySummaryText = string.IsNullOrEmpty(source.GearCarryContinuitySummaryText) ? string.Empty : source.GearCarryContinuitySummaryText;
        copy.GearRuleSummaryText = string.IsNullOrEmpty(source.GearRuleSummaryText) ? string.Empty : source.GearRuleSummaryText;
        copy.GearUnlockSummaryText = string.IsNullOrEmpty(source.GearUnlockSummaryText) ? string.Empty : source.GearUnlockSummaryText;
        copy.GearComparisonSummaryText = string.IsNullOrEmpty(source.GearComparisonSummaryText) ? string.Empty : source.GearComparisonSummaryText;
        copy.GearAffixLiteSummaryText = string.IsNullOrEmpty(source.GearAffixLiteSummaryText) ? string.Empty : source.GearAffixLiteSummaryText;
        copy.UnlockedGearPoolSummaryText = string.IsNullOrEmpty(source.UnlockedGearPoolSummaryText) ? string.Empty : source.UnlockedGearPoolSummaryText;
        copy.LevelBandGearLinkageSummaryText = string.IsNullOrEmpty(source.LevelBandGearLinkageSummaryText) ? string.Empty : source.LevelBandGearLinkageSummaryText;
        copy.ActiveEquippedSummaryText = string.IsNullOrEmpty(source.ActiveEquippedSummaryText) ? string.Empty : source.ActiveEquippedSummaryText;
        copy.HudSummaryText = string.IsNullOrEmpty(source.HudSummaryText) ? string.Empty : source.HudSummaryText;
        copy.GearDefinitions = CopyRpgGearDefinitions(source.GearDefinitions);
        copy.SlotRules = CopyRpgGearSlotRules(source.SlotRules);
        copy.UnlockState = CopyRpgGearUnlockState(source.UnlockState);
        copy.ComparisonSummaries = CopyRpgGearComparisonSummaries(source.ComparisonSummaries);
        copy.AffixDefinitions = CopyRpgGearAffixDefinitions(source.AffixDefinitions);
        copy.AffixContributions = CopyRpgGearAffixLiteContributions(source.AffixContributions);
        copy.RewardDefinitions = CopyRpgEquipmentRewardDefinitions(source.RewardDefinitions);
        copy.RewardCandidates = CopyRpgEquipmentRewardCandidates(source.RewardCandidates);
        copy.EquipSwapChoices = CopyRpgEquipSwapChoices(source.EquipSwapChoices);
        return copy;
    }

    private PrototypeRpgGearDefinition[] CopyRpgGearDefinitions(PrototypeRpgGearDefinition[] source)
    {
        if (source == null || source.Length <= 0)
        {
            return Array.Empty<PrototypeRpgGearDefinition>();
        }

        PrototypeRpgGearDefinition[] copy = new PrototypeRpgGearDefinition[source.Length];
        for (int i = 0; i < source.Length; i++)
        {
            PrototypeRpgGearDefinition entry = source[i];
            PrototypeRpgGearDefinition clone = new PrototypeRpgGearDefinition();
            if (entry != null)
            {
                clone.GearDefinitionId = string.IsNullOrEmpty(entry.GearDefinitionId) ? string.Empty : entry.GearDefinitionId;
                clone.EquipmentLoadoutId = string.IsNullOrEmpty(entry.EquipmentLoadoutId) ? string.Empty : entry.EquipmentLoadoutId;
                clone.SlotKey = string.IsNullOrEmpty(entry.SlotKey) ? string.Empty : entry.SlotKey;
                clone.TierKey = string.IsNullOrEmpty(entry.TierKey) ? string.Empty : entry.TierKey;
                clone.RarityKey = string.IsNullOrEmpty(entry.RarityKey) ? string.Empty : entry.RarityKey;
                clone.UnlockStateKey = string.IsNullOrEmpty(entry.UnlockStateKey) ? string.Empty : entry.UnlockStateKey;
                clone.GroupKey = string.IsNullOrEmpty(entry.GroupKey) ? string.Empty : entry.GroupKey;
                clone.DisplayLabel = string.IsNullOrEmpty(entry.DisplayLabel) ? string.Empty : entry.DisplayLabel;
                clone.EffectPreviewText = string.IsNullOrEmpty(entry.EffectPreviewText) ? string.Empty : entry.EffectPreviewText;
                clone.ComparisonHintText = string.IsNullOrEmpty(entry.ComparisonHintText) ? string.Empty : entry.ComparisonHintText;
            }

            copy[i] = clone;
        }

        return copy;
    }

    private PrototypeRpgGearSlotRule[] CopyRpgGearSlotRules(PrototypeRpgGearSlotRule[] source)
    {
        if (source == null || source.Length <= 0)
        {
            return Array.Empty<PrototypeRpgGearSlotRule>();
        }

        PrototypeRpgGearSlotRule[] copy = new PrototypeRpgGearSlotRule[source.Length];
        for (int i = 0; i < source.Length; i++)
        {
            PrototypeRpgGearSlotRule entry = source[i];
            PrototypeRpgGearSlotRule clone = new PrototypeRpgGearSlotRule();
            if (entry != null)
            {
                clone.SlotKey = string.IsNullOrEmpty(entry.SlotKey) ? string.Empty : entry.SlotKey;
                clone.SlotLabel = string.IsNullOrEmpty(entry.SlotLabel) ? string.Empty : entry.SlotLabel;
                clone.CurrentEquipmentLoadoutId = string.IsNullOrEmpty(entry.CurrentEquipmentLoadoutId) ? string.Empty : entry.CurrentEquipmentLoadoutId;
                clone.CurrentEquipmentDisplayLabel = string.IsNullOrEmpty(entry.CurrentEquipmentDisplayLabel) ? string.Empty : entry.CurrentEquipmentDisplayLabel;
                clone.CandidateEquipmentLoadoutId = string.IsNullOrEmpty(entry.CandidateEquipmentLoadoutId) ? string.Empty : entry.CandidateEquipmentLoadoutId;
                clone.CandidateEquipmentDisplayLabel = string.IsNullOrEmpty(entry.CandidateEquipmentDisplayLabel) ? string.Empty : entry.CandidateEquipmentDisplayLabel;
                clone.AllowedTierKey = string.IsNullOrEmpty(entry.AllowedTierKey) ? string.Empty : entry.AllowedTierKey;
                clone.AllowedRarityKey = string.IsNullOrEmpty(entry.AllowedRarityKey) ? string.Empty : entry.AllowedRarityKey;
                clone.ConflictSummaryText = string.IsNullOrEmpty(entry.ConflictSummaryText) ? string.Empty : entry.ConflictSummaryText;
                clone.ReplacementSummaryText = string.IsNullOrEmpty(entry.ReplacementSummaryText) ? string.Empty : entry.ReplacementSummaryText;
                clone.SummaryText = string.IsNullOrEmpty(entry.SummaryText) ? string.Empty : entry.SummaryText;
            }

            copy[i] = clone;
        }

        return copy;
    }

    private PrototypeRpgGearUnlockState CopyRpgGearUnlockState(PrototypeRpgGearUnlockState source)
    {
        PrototypeRpgGearUnlockState copy = new PrototypeRpgGearUnlockState();
        if (source == null)
        {
            return copy;
        }

        copy.PartyId = string.IsNullOrEmpty(source.PartyId) ? string.Empty : source.PartyId;
        copy.LastRunIdentity = string.IsNullOrEmpty(source.LastRunIdentity) ? string.Empty : source.LastRunIdentity;
        copy.HighestUnlockedTierKey = string.IsNullOrEmpty(source.HighestUnlockedTierKey) ? string.Empty : source.HighestUnlockedTierKey;
        copy.HighestUnlockedRarityKey = string.IsNullOrEmpty(source.HighestUnlockedRarityKey) ? string.Empty : source.HighestUnlockedRarityKey;
        copy.HighestUnlockedLevelBandKey = string.IsNullOrEmpty(source.HighestUnlockedLevelBandKey) ? string.Empty : source.HighestUnlockedLevelBandKey;
        copy.UnlockSummaryText = string.IsNullOrEmpty(source.UnlockSummaryText) ? string.Empty : source.UnlockSummaryText;
        copy.UnlockedPoolSummaryText = string.IsNullOrEmpty(source.UnlockedPoolSummaryText) ? string.Empty : source.UnlockedPoolSummaryText;
        copy.UnlockedTierKeys = source.UnlockedTierKeys != null && source.UnlockedTierKeys.Length > 0 ? (string[])source.UnlockedTierKeys.Clone() : Array.Empty<string>();
        copy.UnlockedGearDefinitionIds = source.UnlockedGearDefinitionIds != null && source.UnlockedGearDefinitionIds.Length > 0 ? (string[])source.UnlockedGearDefinitionIds.Clone() : Array.Empty<string>();
        copy.RecentlySeenGearGroupKeys = source.RecentlySeenGearGroupKeys != null && source.RecentlySeenGearGroupKeys.Length > 0 ? (string[])source.RecentlySeenGearGroupKeys.Clone() : Array.Empty<string>();
        return copy;
    }

    private PrototypeRpgGearComparisonSummary[] CopyRpgGearComparisonSummaries(PrototypeRpgGearComparisonSummary[] source)
    {
        if (source == null || source.Length <= 0)
        {
            return Array.Empty<PrototypeRpgGearComparisonSummary>();
        }

        PrototypeRpgGearComparisonSummary[] copy = new PrototypeRpgGearComparisonSummary[source.Length];
        for (int i = 0; i < source.Length; i++)
        {
            PrototypeRpgGearComparisonSummary entry = source[i];
            PrototypeRpgGearComparisonSummary clone = new PrototypeRpgGearComparisonSummary();
            if (entry != null)
            {
                clone.MemberId = string.IsNullOrEmpty(entry.MemberId) ? string.Empty : entry.MemberId;
                clone.MemberDisplayName = string.IsNullOrEmpty(entry.MemberDisplayName) ? string.Empty : entry.MemberDisplayName;
                clone.CurrentEquipmentLoadoutId = string.IsNullOrEmpty(entry.CurrentEquipmentLoadoutId) ? string.Empty : entry.CurrentEquipmentLoadoutId;
                clone.CurrentEquipmentDisplayLabel = string.IsNullOrEmpty(entry.CurrentEquipmentDisplayLabel) ? string.Empty : entry.CurrentEquipmentDisplayLabel;
                clone.CandidateEquipmentLoadoutId = string.IsNullOrEmpty(entry.CandidateEquipmentLoadoutId) ? string.Empty : entry.CandidateEquipmentLoadoutId;
                clone.CandidateEquipmentDisplayLabel = string.IsNullOrEmpty(entry.CandidateEquipmentDisplayLabel) ? string.Empty : entry.CandidateEquipmentDisplayLabel;
                clone.SlotKey = string.IsNullOrEmpty(entry.SlotKey) ? string.Empty : entry.SlotKey;
                clone.TierKey = string.IsNullOrEmpty(entry.TierKey) ? string.Empty : entry.TierKey;
                clone.RarityKey = string.IsNullOrEmpty(entry.RarityKey) ? string.Empty : entry.RarityKey;
                clone.StatDeltaPreviewText = string.IsNullOrEmpty(entry.StatDeltaPreviewText) ? string.Empty : entry.StatDeltaPreviewText;
                clone.SkillHintDeltaText = string.IsNullOrEmpty(entry.SkillHintDeltaText) ? string.Empty : entry.SkillHintDeltaText;
                clone.SwapReasonText = string.IsNullOrEmpty(entry.SwapReasonText) ? string.Empty : entry.SwapReasonText;
                clone.AffixLiteSummaryText = string.IsNullOrEmpty(entry.AffixLiteSummaryText) ? string.Empty : entry.AffixLiteSummaryText;
                clone.ComparisonHintText = string.IsNullOrEmpty(entry.ComparisonHintText) ? string.Empty : entry.ComparisonHintText;
                clone.SummaryText = string.IsNullOrEmpty(entry.SummaryText) ? string.Empty : entry.SummaryText;
            }

            copy[i] = clone;
        }

        return copy;
    }

    private PrototypeRpgEquipmentRewardDefinition[] CopyRpgEquipmentRewardDefinitions(PrototypeRpgEquipmentRewardDefinition[] source)
    {
        if (source == null || source.Length <= 0)
        {
            return Array.Empty<PrototypeRpgEquipmentRewardDefinition>();
        }

        PrototypeRpgEquipmentRewardDefinition[] copy = new PrototypeRpgEquipmentRewardDefinition[source.Length];
        for (int i = 0; i < source.Length; i++)
        {
            PrototypeRpgEquipmentRewardDefinition entry = source[i];
            PrototypeRpgEquipmentRewardDefinition clone = new PrototypeRpgEquipmentRewardDefinition();
            if (entry != null)
            {
                clone.RewardId = string.IsNullOrEmpty(entry.RewardId) ? string.Empty : entry.RewardId;
                clone.EquipmentLoadoutId = string.IsNullOrEmpty(entry.EquipmentLoadoutId) ? string.Empty : entry.EquipmentLoadoutId;
                clone.GearDefinitionId = string.IsNullOrEmpty(entry.GearDefinitionId) ? string.Empty : entry.GearDefinitionId;
                clone.SlotKey = string.IsNullOrEmpty(entry.SlotKey) ? string.Empty : entry.SlotKey;
                clone.TierKey = string.IsNullOrEmpty(entry.TierKey) ? string.Empty : entry.TierKey;
                clone.RarityKey = string.IsNullOrEmpty(entry.RarityKey) ? string.Empty : entry.RarityKey;
                clone.UnlockStateKey = string.IsNullOrEmpty(entry.UnlockStateKey) ? string.Empty : entry.UnlockStateKey;
                clone.GearGroupKey = string.IsNullOrEmpty(entry.GearGroupKey) ? string.Empty : entry.GearGroupKey;
                clone.DisplayLabel = string.IsNullOrEmpty(entry.DisplayLabel) ? string.Empty : entry.DisplayLabel;
                clone.EffectPreviewText = string.IsNullOrEmpty(entry.EffectPreviewText) ? string.Empty : entry.EffectPreviewText;
                clone.CandidateSourceKey = string.IsNullOrEmpty(entry.CandidateSourceKey) ? string.Empty : entry.CandidateSourceKey;
            }

            copy[i] = clone;
        }

        return copy;
    }

    private PrototypeRpgEquipmentRewardCandidate[] CopyRpgEquipmentRewardCandidates(PrototypeRpgEquipmentRewardCandidate[] source)
    {
        if (source == null || source.Length <= 0)
        {
            return Array.Empty<PrototypeRpgEquipmentRewardCandidate>();
        }

        PrototypeRpgEquipmentRewardCandidate[] copy = new PrototypeRpgEquipmentRewardCandidate[source.Length];
        for (int i = 0; i < source.Length; i++)
        {
            PrototypeRpgEquipmentRewardCandidate entry = source[i];
            PrototypeRpgEquipmentRewardCandidate clone = new PrototypeRpgEquipmentRewardCandidate();
            if (entry != null)
            {
                clone.RewardId = string.IsNullOrEmpty(entry.RewardId) ? string.Empty : entry.RewardId;
                clone.MemberId = string.IsNullOrEmpty(entry.MemberId) ? string.Empty : entry.MemberId;
                clone.MemberDisplayName = string.IsNullOrEmpty(entry.MemberDisplayName) ? string.Empty : entry.MemberDisplayName;
                clone.EquipmentLoadoutId = string.IsNullOrEmpty(entry.EquipmentLoadoutId) ? string.Empty : entry.EquipmentLoadoutId;
                clone.CurrentEquipmentLoadoutId = string.IsNullOrEmpty(entry.CurrentEquipmentLoadoutId) ? string.Empty : entry.CurrentEquipmentLoadoutId;
                clone.CurrentEquipmentDisplayLabel = string.IsNullOrEmpty(entry.CurrentEquipmentDisplayLabel) ? string.Empty : entry.CurrentEquipmentDisplayLabel;
                clone.DisplayLabel = string.IsNullOrEmpty(entry.DisplayLabel) ? string.Empty : entry.DisplayLabel;
                clone.SlotKey = string.IsNullOrEmpty(entry.SlotKey) ? string.Empty : entry.SlotKey;
                clone.TierKey = string.IsNullOrEmpty(entry.TierKey) ? string.Empty : entry.TierKey;
                clone.RarityKey = string.IsNullOrEmpty(entry.RarityKey) ? string.Empty : entry.RarityKey;
                clone.UnlockStateKey = string.IsNullOrEmpty(entry.UnlockStateKey) ? string.Empty : entry.UnlockStateKey;
                clone.GearGroupKey = string.IsNullOrEmpty(entry.GearGroupKey) ? string.Empty : entry.GearGroupKey;
                clone.CandidateSourceKey = string.IsNullOrEmpty(entry.CandidateSourceKey) ? string.Empty : entry.CandidateSourceKey;
                clone.EffectPreviewText = string.IsNullOrEmpty(entry.EffectPreviewText) ? string.Empty : entry.EffectPreviewText;
                clone.DerivedStatDeltaSummaryText = string.IsNullOrEmpty(entry.DerivedStatDeltaSummaryText) ? string.Empty : entry.DerivedStatDeltaSummaryText;
                clone.SkillHintText = string.IsNullOrEmpty(entry.SkillHintText) ? string.Empty : entry.SkillHintText;
                clone.RoleHintText = string.IsNullOrEmpty(entry.RoleHintText) ? string.Empty : entry.RoleHintText;
                clone.AffixLiteSummaryText = string.IsNullOrEmpty(entry.AffixLiteSummaryText) ? string.Empty : entry.AffixLiteSummaryText;
                clone.LevelBandSummaryText = string.IsNullOrEmpty(entry.LevelBandSummaryText) ? string.Empty : entry.LevelBandSummaryText;
                clone.UnlockedPoolSummaryText = string.IsNullOrEmpty(entry.UnlockedPoolSummaryText) ? string.Empty : entry.UnlockedPoolSummaryText;
                clone.ComparisonSummaryText = string.IsNullOrEmpty(entry.ComparisonSummaryText) ? string.Empty : entry.ComparisonSummaryText;
                clone.RuleSummaryText = string.IsNullOrEmpty(entry.RuleSummaryText) ? string.Empty : entry.RuleSummaryText;
                clone.UnlockSummaryText = string.IsNullOrEmpty(entry.UnlockSummaryText) ? string.Empty : entry.UnlockSummaryText;
                clone.SummaryText = string.IsNullOrEmpty(entry.SummaryText) ? string.Empty : entry.SummaryText;
                clone.IsRecommended = entry.IsRecommended;
            }

            copy[i] = clone;
        }

        return copy;
    }

    private PrototypeRpgEquipSwapChoice[] CopyRpgEquipSwapChoices(PrototypeRpgEquipSwapChoice[] source)
    {
        if (source == null || source.Length <= 0)
        {
            return Array.Empty<PrototypeRpgEquipSwapChoice>();
        }

        PrototypeRpgEquipSwapChoice[] copy = new PrototypeRpgEquipSwapChoice[source.Length];
        for (int i = 0; i < source.Length; i++)
        {
            PrototypeRpgEquipSwapChoice entry = source[i];
            PrototypeRpgEquipSwapChoice clone = new PrototypeRpgEquipSwapChoice();
            if (entry != null)
            {
                clone.RewardId = string.IsNullOrEmpty(entry.RewardId) ? string.Empty : entry.RewardId;
                clone.MemberId = string.IsNullOrEmpty(entry.MemberId) ? string.Empty : entry.MemberId;
                clone.MemberDisplayName = string.IsNullOrEmpty(entry.MemberDisplayName) ? string.Empty : entry.MemberDisplayName;
                clone.CurrentEquipmentLoadoutId = string.IsNullOrEmpty(entry.CurrentEquipmentLoadoutId) ? string.Empty : entry.CurrentEquipmentLoadoutId;
                clone.CurrentEquipmentDisplayLabel = string.IsNullOrEmpty(entry.CurrentEquipmentDisplayLabel) ? string.Empty : entry.CurrentEquipmentDisplayLabel;
                clone.TargetEquipmentLoadoutId = string.IsNullOrEmpty(entry.TargetEquipmentLoadoutId) ? string.Empty : entry.TargetEquipmentLoadoutId;
                clone.TargetEquipmentDisplayLabel = string.IsNullOrEmpty(entry.TargetEquipmentDisplayLabel) ? string.Empty : entry.TargetEquipmentDisplayLabel;
                clone.SlotKey = string.IsNullOrEmpty(entry.SlotKey) ? string.Empty : entry.SlotKey;
                clone.ReplacementSummaryText = string.IsNullOrEmpty(entry.ReplacementSummaryText) ? string.Empty : entry.ReplacementSummaryText;
                clone.ComparisonSummaryText = string.IsNullOrEmpty(entry.ComparisonSummaryText) ? string.Empty : entry.ComparisonSummaryText;
                clone.AffixLiteSummaryText = string.IsNullOrEmpty(entry.AffixLiteSummaryText) ? string.Empty : entry.AffixLiteSummaryText;
                clone.EffectPreviewText = string.IsNullOrEmpty(entry.EffectPreviewText) ? string.Empty : entry.EffectPreviewText;
                clone.ContinuitySummaryText = string.IsNullOrEmpty(entry.ContinuitySummaryText) ? string.Empty : entry.ContinuitySummaryText;
                clone.SummaryText = string.IsNullOrEmpty(entry.SummaryText) ? string.Empty : entry.SummaryText;
                clone.ApplyReady = entry.ApplyReady;
                clone.HasChanges = entry.HasChanges;
            }

            copy[i] = clone;
        }

        return copy;
    }

    private PrototypeRpgGearAffixLiteDefinition[] CopyRpgGearAffixDefinitions(PrototypeRpgGearAffixLiteDefinition[] source)
    {
        if (source == null || source.Length <= 0)
        {
            return Array.Empty<PrototypeRpgGearAffixLiteDefinition>();
        }

        PrototypeRpgGearAffixLiteDefinition[] copy = new PrototypeRpgGearAffixLiteDefinition[source.Length];
        for (int i = 0; i < source.Length; i++)
        {
            PrototypeRpgGearAffixLiteDefinition entry = source[i];
            PrototypeRpgGearAffixLiteDefinition clone = new PrototypeRpgGearAffixLiteDefinition();
            if (entry != null)
            {
                clone.AffixId = string.IsNullOrEmpty(entry.AffixId) ? string.Empty : entry.AffixId;
                clone.DisplayLabel = string.IsNullOrEmpty(entry.DisplayLabel) ? string.Empty : entry.DisplayLabel;
                clone.RoleTag = string.IsNullOrEmpty(entry.RoleTag) ? string.Empty : entry.RoleTag;
                clone.SlotKey = string.IsNullOrEmpty(entry.SlotKey) ? string.Empty : entry.SlotKey;
                clone.TierKey = string.IsNullOrEmpty(entry.TierKey) ? string.Empty : entry.TierKey;
                clone.RarityKey = string.IsNullOrEmpty(entry.RarityKey) ? string.Empty : entry.RarityKey;
                clone.BonusMaxHp = entry.BonusMaxHp;
                clone.BonusAttack = entry.BonusAttack;
                clone.BonusDefense = entry.BonusDefense;
                clone.BonusSpeed = entry.BonusSpeed;
                clone.SkillPowerHintText = string.IsNullOrEmpty(entry.SkillPowerHintText) ? string.Empty : entry.SkillPowerHintText;
                clone.HealPowerHintText = string.IsNullOrEmpty(entry.HealPowerHintText) ? string.Empty : entry.HealPowerHintText;
                clone.SummaryText = string.IsNullOrEmpty(entry.SummaryText) ? string.Empty : entry.SummaryText;
            }

            copy[i] = clone;
        }

        return copy;
    }

    private PrototypeRpgGearAffixLiteContribution[] CopyRpgGearAffixLiteContributions(PrototypeRpgGearAffixLiteContribution[] source)
    {
        if (source == null || source.Length <= 0)
        {
            return Array.Empty<PrototypeRpgGearAffixLiteContribution>();
        }

        PrototypeRpgGearAffixLiteContribution[] copy = new PrototypeRpgGearAffixLiteContribution[source.Length];
        for (int i = 0; i < source.Length; i++)
        {
            PrototypeRpgGearAffixLiteContribution entry = source[i];
            PrototypeRpgGearAffixLiteContribution clone = new PrototypeRpgGearAffixLiteContribution();
            if (entry != null)
            {
                clone.AffixId = string.IsNullOrEmpty(entry.AffixId) ? string.Empty : entry.AffixId;
                clone.DisplayLabel = string.IsNullOrEmpty(entry.DisplayLabel) ? string.Empty : entry.DisplayLabel;
                clone.BonusMaxHp = entry.BonusMaxHp;
                clone.BonusAttack = entry.BonusAttack;
                clone.BonusDefense = entry.BonusDefense;
                clone.BonusSpeed = entry.BonusSpeed;
                clone.HintText = string.IsNullOrEmpty(entry.HintText) ? string.Empty : entry.HintText;
                clone.SummaryText = string.IsNullOrEmpty(entry.SummaryText) ? string.Empty : entry.SummaryText;
            }

            copy[i] = clone;
        }

        return copy;
    }
}
