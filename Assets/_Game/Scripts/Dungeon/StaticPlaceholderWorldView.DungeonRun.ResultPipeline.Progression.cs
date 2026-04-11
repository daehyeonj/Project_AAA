using System.Collections.Generic;
using UnityEngine;

public sealed partial class StaticPlaceholderWorldView
{
    private PrototypeRpgRunResultSnapshot CopyRpgRunResultSnapshot(PrototypeRpgRunResultSnapshot source)
    {
        PrototypeRpgRunResultSnapshot copy = new PrototypeRpgRunResultSnapshot();
        if (source == null)
        {
            return copy;
        }

        copy.ResultStateKey = string.IsNullOrEmpty(source.ResultStateKey) ? string.Empty : source.ResultStateKey;
        copy.DungeonId = string.IsNullOrEmpty(source.DungeonId) ? string.Empty : source.DungeonId;
        copy.DungeonLabel = string.IsNullOrEmpty(source.DungeonLabel) ? string.Empty : source.DungeonLabel;
        copy.RouteId = string.IsNullOrEmpty(source.RouteId) ? string.Empty : source.RouteId;
        copy.RouteLabel = string.IsNullOrEmpty(source.RouteLabel) ? string.Empty : source.RouteLabel;
        copy.TotalTurnsTaken = Mathf.Max(0, source.TotalTurnsTaken);
        copy.ResultSummary = string.IsNullOrEmpty(source.ResultSummary) ? string.Empty : source.ResultSummary;
        copy.SurvivingMembersSummary = string.IsNullOrEmpty(source.SurvivingMembersSummary) ? string.Empty : source.SurvivingMembersSummary;
        copy.PartyOutcome = CopyRpgPartyOutcomeSnapshot(source.PartyOutcome);
        copy.LootOutcome = CopyRpgLootOutcomeSnapshot(source.LootOutcome);
        copy.EliteOutcome = CopyRpgEliteOutcomeSnapshot(source.EliteOutcome);
        copy.EncounterOutcome = CopyRpgEncounterOutcomeSnapshot(source.EncounterOutcome);
        return copy;
    }

    private PrototypeRpgPartyOutcomeSnapshot CopyRpgPartyOutcomeSnapshot(PrototypeRpgPartyOutcomeSnapshot source)
    {
        PrototypeRpgPartyOutcomeSnapshot copy = new PrototypeRpgPartyOutcomeSnapshot();
        if (source == null)
        {
            return copy;
        }

        copy.PartyConditionText = string.IsNullOrEmpty(source.PartyConditionText) ? string.Empty : source.PartyConditionText;
        copy.PartyHpSummaryText = string.IsNullOrEmpty(source.PartyHpSummaryText) ? string.Empty : source.PartyHpSummaryText;
        copy.PartyMembersAtEndSummary = string.IsNullOrEmpty(source.PartyMembersAtEndSummary) ? string.Empty : source.PartyMembersAtEndSummary;
        copy.SurvivingMemberCount = Mathf.Max(0, source.SurvivingMemberCount);
        copy.KnockedOutMemberCount = Mathf.Max(0, source.KnockedOutMemberCount);

        if (source.Members == null || source.Members.Length == 0)
        {
            copy.Members = System.Array.Empty<PrototypeRpgPartyMemberOutcomeSnapshot>();
            return copy;
        }

        PrototypeRpgPartyMemberOutcomeSnapshot[] members = new PrototypeRpgPartyMemberOutcomeSnapshot[source.Members.Length];
        for (int i = 0; i < source.Members.Length; i++)
        {
            members[i] = CopyRpgPartyMemberOutcomeSnapshot(source.Members[i]);
        }

        copy.Members = members;
        return copy;
    }

    private PrototypeRpgPartyMemberOutcomeSnapshot CopyRpgPartyMemberOutcomeSnapshot(PrototypeRpgPartyMemberOutcomeSnapshot source)
    {
        PrototypeRpgPartyMemberOutcomeSnapshot copy = new PrototypeRpgPartyMemberOutcomeSnapshot();
        if (source == null)
        {
            return copy;
        }

        copy.MemberId = string.IsNullOrEmpty(source.MemberId) ? string.Empty : source.MemberId;
        copy.DisplayName = string.IsNullOrEmpty(source.DisplayName) ? string.Empty : source.DisplayName;
        copy.RoleTag = string.IsNullOrEmpty(source.RoleTag) ? string.Empty : source.RoleTag;
        copy.RoleLabel = string.IsNullOrEmpty(source.RoleLabel) ? string.Empty : source.RoleLabel;
        copy.DefaultSkillId = string.IsNullOrEmpty(source.DefaultSkillId) ? string.Empty : source.DefaultSkillId;
        copy.GrowthTrackId = string.IsNullOrEmpty(source.GrowthTrackId) ? string.Empty : source.GrowthTrackId;
        copy.JobId = string.IsNullOrEmpty(source.JobId) ? string.Empty : source.JobId;
        copy.EquipmentLoadoutId = string.IsNullOrEmpty(source.EquipmentLoadoutId) ? string.Empty : source.EquipmentLoadoutId;
        copy.SkillLoadoutId = string.IsNullOrEmpty(source.SkillLoadoutId) ? string.Empty : source.SkillLoadoutId;
        copy.ResolvedSkillName = string.IsNullOrEmpty(source.ResolvedSkillName) ? string.Empty : source.ResolvedSkillName;
        copy.ResolvedSkillShortText = string.IsNullOrEmpty(source.ResolvedSkillShortText) ? string.Empty : source.ResolvedSkillShortText;
        copy.EquipmentSummaryText = string.IsNullOrEmpty(source.EquipmentSummaryText) ? string.Empty : source.EquipmentSummaryText;
        copy.GearContributionSummaryText = string.IsNullOrEmpty(source.GearContributionSummaryText) ? string.Empty : source.GearContributionSummaryText;
        copy.AppliedProgressionSummaryText = string.IsNullOrEmpty(source.AppliedProgressionSummaryText) ? string.Empty : source.AppliedProgressionSummaryText;
        copy.CurrentRunSummaryText = string.IsNullOrEmpty(source.CurrentRunSummaryText) ? string.Empty : source.CurrentRunSummaryText;
        copy.NextRunPreviewSummaryText = string.IsNullOrEmpty(source.NextRunPreviewSummaryText) ? string.Empty : source.NextRunPreviewSummaryText;
        copy.CurrentHp = Mathf.Max(0, source.CurrentHp);
        copy.MaxHp = Mathf.Max(1, source.MaxHp);
        copy.Survived = source.Survived;
        copy.KnockedOut = source.KnockedOut;
        return copy;
    }

    private PrototypeRpgLootOutcomeSnapshot CopyRpgLootOutcomeSnapshot(PrototypeRpgLootOutcomeSnapshot source)
    {
        PrototypeRpgLootOutcomeSnapshot copy = new PrototypeRpgLootOutcomeSnapshot();
        if (source == null)
        {
            return copy;
        }

        copy.TotalLootGained = Mathf.Max(0, source.TotalLootGained);
        copy.BattleLootGained = Mathf.Max(0, source.BattleLootGained);
        copy.ChestLootGained = Mathf.Max(0, source.ChestLootGained);
        copy.EventLootGained = Mathf.Max(0, source.EventLootGained);
        copy.EliteRewardAmount = Mathf.Max(0, source.EliteRewardAmount);
        copy.EliteBonusRewardAmount = Mathf.Max(0, source.EliteBonusRewardAmount);
        copy.FinalLootSummary = string.IsNullOrEmpty(source.FinalLootSummary) ? string.Empty : source.FinalLootSummary;
        return copy;
    }

    private PrototypeRpgEliteOutcomeSnapshot CopyRpgEliteOutcomeSnapshot(PrototypeRpgEliteOutcomeSnapshot source)
    {
        PrototypeRpgEliteOutcomeSnapshot copy = new PrototypeRpgEliteOutcomeSnapshot();
        if (source == null)
        {
            return copy;
        }

        copy.IsEliteDefeated = source.IsEliteDefeated;
        copy.EliteName = string.IsNullOrEmpty(source.EliteName) ? string.Empty : source.EliteName;
        copy.EliteTypeLabel = string.IsNullOrEmpty(source.EliteTypeLabel) ? string.Empty : source.EliteTypeLabel;
        copy.EliteRewardLabel = string.IsNullOrEmpty(source.EliteRewardLabel) ? string.Empty : source.EliteRewardLabel;
        copy.EliteRewardAmount = Mathf.Max(0, source.EliteRewardAmount);
        copy.EliteBonusRewardEarned = source.EliteBonusRewardEarned;
        copy.EliteBonusRewardAmount = Mathf.Max(0, source.EliteBonusRewardAmount);
        return copy;
    }

    private PrototypeRpgEncounterOutcomeSnapshot CopyRpgEncounterOutcomeSnapshot(PrototypeRpgEncounterOutcomeSnapshot source)
    {
        PrototypeRpgEncounterOutcomeSnapshot copy = new PrototypeRpgEncounterOutcomeSnapshot();
        if (source == null)
        {
            return copy;
        }

        copy.ClearedEncounterCount = Mathf.Max(0, source.ClearedEncounterCount);
        copy.ClearedEncounterSummary = string.IsNullOrEmpty(source.ClearedEncounterSummary) ? string.Empty : source.ClearedEncounterSummary;
        copy.OpenedChestCount = Mathf.Max(0, source.OpenedChestCount);
        copy.RoomPathSummary = string.IsNullOrEmpty(source.RoomPathSummary) ? string.Empty : source.RoomPathSummary;
        copy.SelectedEventChoice = string.IsNullOrEmpty(source.SelectedEventChoice) ? string.Empty : source.SelectedEventChoice;
        copy.SelectedPreEliteChoice = string.IsNullOrEmpty(source.SelectedPreEliteChoice) ? string.Empty : source.SelectedPreEliteChoice;
        copy.PreEliteHealAmount = Mathf.Max(0, source.PreEliteHealAmount);
        return copy;
    }

    private string BuildRpgRunResultLootSummary(PrototypeRpgLootOutcomeSnapshot lootOutcome)
    {
        if (lootOutcome == null)
        {
            return "None";
        }

        int battleLoot = Mathf.Max(0, lootOutcome.BattleLootGained);
        int chestLoot = Mathf.Max(0, lootOutcome.ChestLootGained);
        int eventLoot = Mathf.Max(0, lootOutcome.EventLootGained);
        int eliteLoot = Mathf.Max(0, lootOutcome.EliteRewardAmount);
        int eliteBonusLoot = Mathf.Max(0, lootOutcome.EliteBonusRewardAmount);
        int totalLoot = Mathf.Max(Mathf.Max(0, lootOutcome.TotalLootGained), battleLoot + chestLoot + eventLoot + eliteLoot + eliteBonusLoot);
        return "Battle " + battleLoot + " / Chest " + chestLoot + " / Event " + eventLoot + " / Elite " + eliteLoot + " / Elite Bonus " + eliteBonusLoot + " / Total " + totalLoot;
    }

    private PrototypeRpgPartyOutcomeSnapshot BuildRpgPartyOutcomeSnapshot()
    {
        PrototypeRpgPartyOutcomeSnapshot snapshot = new PrototypeRpgPartyOutcomeSnapshot();
        snapshot.PartyConditionText = GetPartyConditionText();
        snapshot.PartyHpSummaryText = BuildTotalPartyHpSummary();
        snapshot.PartyMembersAtEndSummary = BuildPartyMembersAtEndSummary();
        snapshot.SurvivingMemberCount = GetLivingPartyMemberCount();
        snapshot.KnockedOutMemberCount = GetKnockedOutMemberCount();

        if (_activeDungeonParty == null || _activeDungeonParty.Members == null)
        {
            snapshot.Members = System.Array.Empty<PrototypeRpgPartyMemberOutcomeSnapshot>();
            return snapshot;
        }

        PrototypeRpgPartyMemberOutcomeSnapshot[] members = new PrototypeRpgPartyMemberOutcomeSnapshot[_activeDungeonParty.Members.Length];
        for (int i = 0; i < _activeDungeonParty.Members.Length; i++)
        {
            PrototypeRpgPartyMemberOutcomeSnapshot memberSnapshot = new PrototypeRpgPartyMemberOutcomeSnapshot();
            DungeonPartyMemberRuntimeData member = _activeDungeonParty.Members[i];
            if (member != null)
            {
                PrototypeRpgPartyMemberRuntimeState runtimeState = member.RuntimeState;
                memberSnapshot.MemberId = string.IsNullOrEmpty(member.MemberId) ? string.Empty : member.MemberId;
                memberSnapshot.DisplayName = string.IsNullOrEmpty(member.DisplayName) ? string.Empty : member.DisplayName;
                memberSnapshot.RoleTag = string.IsNullOrEmpty(member.RoleTag) ? string.Empty : member.RoleTag;
                memberSnapshot.RoleLabel = string.IsNullOrEmpty(member.RoleLabel) ? string.Empty : member.RoleLabel;
                memberSnapshot.DefaultSkillId = string.IsNullOrEmpty(member.DefaultSkillId) ? string.Empty : member.DefaultSkillId;
                memberSnapshot.GrowthTrackId = runtimeState != null && !string.IsNullOrEmpty(runtimeState.GrowthTrackId) ? runtimeState.GrowthTrackId : string.Empty;
                memberSnapshot.JobId = runtimeState != null && !string.IsNullOrEmpty(runtimeState.JobId) ? runtimeState.JobId : string.Empty;
                memberSnapshot.EquipmentLoadoutId = runtimeState != null && !string.IsNullOrEmpty(runtimeState.EquipmentLoadoutId) ? runtimeState.EquipmentLoadoutId : string.Empty;
                memberSnapshot.SkillLoadoutId = runtimeState != null && !string.IsNullOrEmpty(runtimeState.SkillLoadoutId) ? runtimeState.SkillLoadoutId : string.Empty;
                memberSnapshot.ResolvedSkillName = runtimeState != null && !string.IsNullOrEmpty(runtimeState.ResolvedSkillName) ? runtimeState.ResolvedSkillName : (string.IsNullOrEmpty(member.SkillName) ? string.Empty : member.SkillName);
                memberSnapshot.ResolvedSkillShortText = runtimeState != null && !string.IsNullOrEmpty(runtimeState.ResolvedSkillShortText) ? runtimeState.ResolvedSkillShortText : (string.IsNullOrEmpty(member.SkillShortText) ? string.Empty : member.SkillShortText);
                memberSnapshot.EquipmentSummaryText = runtimeState != null && !string.IsNullOrEmpty(runtimeState.EquipmentSummaryText) ? runtimeState.EquipmentSummaryText : string.Empty;
                memberSnapshot.GearContributionSummaryText = runtimeState != null && !string.IsNullOrEmpty(runtimeState.GearContributionSummaryText) ? runtimeState.GearContributionSummaryText : string.Empty;
                memberSnapshot.AppliedProgressionSummaryText = runtimeState != null && !string.IsNullOrEmpty(runtimeState.AppliedProgressionSummaryText) ? runtimeState.AppliedProgressionSummaryText : string.Empty;
                memberSnapshot.CurrentRunSummaryText = runtimeState != null && !string.IsNullOrEmpty(runtimeState.CurrentRunSummaryText) ? runtimeState.CurrentRunSummaryText : string.Empty;
                memberSnapshot.NextRunPreviewSummaryText = runtimeState != null && !string.IsNullOrEmpty(runtimeState.NextRunPreviewSummaryText) ? runtimeState.NextRunPreviewSummaryText : string.Empty;
                memberSnapshot.CurrentHp = Mathf.Max(0, member.CurrentHp);
                memberSnapshot.MaxHp = Mathf.Max(1, member.MaxHp);
                memberSnapshot.Survived = !member.IsDefeated && member.CurrentHp > 0;
                memberSnapshot.KnockedOut = member.IsDefeated;
            }

            members[i] = memberSnapshot;
        }

        snapshot.Members = members;
        return snapshot;
    }

    private PrototypeRpgRunResultSnapshot BuildRpgRunResultSnapshot(string resultStateKey, int safeReturnedLoot, string safeResultSummary)
    {
        PrototypeRpgRunResultSnapshot snapshot = new PrototypeRpgRunResultSnapshot();
        PrototypeRpgPartyOutcomeSnapshot partyOutcome = BuildRpgPartyOutcomeSnapshot();
        PrototypeRpgLootOutcomeSnapshot lootOutcome = new PrototypeRpgLootOutcomeSnapshot();
        PrototypeRpgEliteOutcomeSnapshot eliteOutcome = new PrototypeRpgEliteOutcomeSnapshot();
        PrototypeRpgEncounterOutcomeSnapshot encounterOutcome = new PrototypeRpgEncounterOutcomeSnapshot();
        int battleLoot = safeReturnedLoot > 0 ? Mathf.Max(0, _battleLootAmount) : 0;
        int chestLoot = safeReturnedLoot > 0 ? Mathf.Max(0, _chestLootAmount) : 0;
        int eventLoot = safeReturnedLoot > 0 ? Mathf.Max(0, _eventLootAmount) : 0;
        int eliteLoot = _eliteRewardGranted ? Mathf.Max(0, _eliteRewardAmount) : 0;
        int eliteBonusLoot = _eliteBonusRewardGranted ? Mathf.Max(0, _eliteBonusRewardGrantedAmount) : 0;

        snapshot.ResultStateKey = string.IsNullOrEmpty(resultStateKey) ? PrototypeBattleOutcomeKeys.None : resultStateKey;
        snapshot.DungeonId = string.IsNullOrEmpty(_currentDungeonId) ? string.Empty : _currentDungeonId;
        snapshot.DungeonLabel = string.IsNullOrEmpty(_currentDungeonName) ? string.Empty : _currentDungeonName;
        snapshot.RouteId = string.IsNullOrEmpty(_selectedRouteId) ? string.Empty : _selectedRouteId;
        snapshot.RouteLabel = string.IsNullOrEmpty(_selectedRouteLabel) ? string.Empty : _selectedRouteLabel;
        snapshot.TotalTurnsTaken = Mathf.Max(0, _runTurnCount);
        snapshot.ResultSummary = string.IsNullOrEmpty(safeResultSummary) ? "The run ended." : safeResultSummary;
        snapshot.SurvivingMembersSummary = BuildSurvivingMembersSummary();

        lootOutcome.BattleLootGained = battleLoot;
        lootOutcome.ChestLootGained = chestLoot;
        lootOutcome.EventLootGained = eventLoot;
        lootOutcome.EliteRewardAmount = eliteLoot;
        lootOutcome.EliteBonusRewardAmount = eliteBonusLoot;
        lootOutcome.TotalLootGained = Mathf.Max(Mathf.Max(0, safeReturnedLoot), battleLoot + chestLoot + eventLoot + eliteLoot + eliteBonusLoot);
        lootOutcome.FinalLootSummary = BuildRpgRunResultLootSummary(lootOutcome);

        eliteOutcome.IsEliteDefeated = _eliteDefeated;
        eliteOutcome.EliteName = string.IsNullOrEmpty(_eliteName) ? string.Empty : _eliteName;
        eliteOutcome.EliteTypeLabel = string.IsNullOrEmpty(_eliteType) ? string.Empty : _eliteType;
        eliteOutcome.EliteRewardLabel = string.IsNullOrEmpty(_eliteRewardLabel) ? string.Empty : _eliteRewardLabel;
        eliteOutcome.EliteRewardAmount = eliteLoot;
        eliteOutcome.EliteBonusRewardEarned = _eliteBonusRewardGranted;
        eliteOutcome.EliteBonusRewardAmount = eliteBonusLoot;

        encounterOutcome.ClearedEncounterCount = Mathf.Max(0, _clearedEncounterCount);
        encounterOutcome.ClearedEncounterSummary = BuildClearedEncounterSummary();
        encounterOutcome.OpenedChestCount = Mathf.Max(0, _chestOpenedCount);
        encounterOutcome.RoomPathSummary = BuildCurrentRoomPathSummary();
        encounterOutcome.SelectedEventChoice = GetSelectedEventChoiceDisplayText();
        encounterOutcome.SelectedPreEliteChoice = GetSelectedPreEliteChoiceDisplayText();
        encounterOutcome.PreEliteHealAmount = Mathf.Max(0, _preEliteHealAmount);

        snapshot.PartyOutcome = partyOutcome;
        snapshot.LootOutcome = lootOutcome;
        snapshot.EliteOutcome = eliteOutcome;
        snapshot.EncounterOutcome = encounterOutcome;
        return snapshot;
    }

    private void ApplyRpgRunResultSnapshotToLegacyFields(PrototypeRpgRunResultSnapshot snapshot)
    {
        PrototypeRpgRunResultSnapshot safeSnapshot = snapshot ?? new PrototypeRpgRunResultSnapshot();
        PrototypeRpgPartyOutcomeSnapshot partyOutcome = safeSnapshot.PartyOutcome ?? new PrototypeRpgPartyOutcomeSnapshot();
        PrototypeRpgLootOutcomeSnapshot lootOutcome = safeSnapshot.LootOutcome ?? new PrototypeRpgLootOutcomeSnapshot();
        PrototypeRpgEliteOutcomeSnapshot eliteOutcome = safeSnapshot.EliteOutcome ?? new PrototypeRpgEliteOutcomeSnapshot();
        PrototypeRpgEncounterOutcomeSnapshot encounterOutcome = safeSnapshot.EncounterOutcome ?? new PrototypeRpgEncounterOutcomeSnapshot();

        _resultTurnsTaken = Mathf.Max(0, safeSnapshot.TotalTurnsTaken);
        _resultLootGained = Mathf.Max(0, lootOutcome.TotalLootGained);
        _resultBattleLootGained = Mathf.Max(0, lootOutcome.BattleLootGained);
        _resultChestLootGained = Mathf.Max(0, lootOutcome.ChestLootGained);
        _resultEventLootGained = Mathf.Max(0, lootOutcome.EventLootGained);
        _resultSurvivingMembersText = string.IsNullOrEmpty(safeSnapshot.SurvivingMembersSummary) ? string.Empty : safeSnapshot.SurvivingMembersSummary;
        _resultPartyHpSummaryText = string.IsNullOrEmpty(partyOutcome.PartyHpSummaryText) ? string.Empty : partyOutcome.PartyHpSummaryText;
        _resultPartyConditionText = string.IsNullOrEmpty(partyOutcome.PartyConditionText) ? string.Empty : partyOutcome.PartyConditionText;
        _resultEventChoiceText = string.IsNullOrEmpty(encounterOutcome.SelectedEventChoice) ? string.Empty : encounterOutcome.SelectedEventChoice;
        _resultPreEliteChoiceText = string.IsNullOrEmpty(encounterOutcome.SelectedPreEliteChoice) ? string.Empty : encounterOutcome.SelectedPreEliteChoice;
        _resultPreEliteHealAmount = Mathf.Max(0, encounterOutcome.PreEliteHealAmount);
        _resultClearedEncounters = Mathf.Max(0, encounterOutcome.ClearedEncounterCount);
        _resultOpenedChests = Mathf.Max(0, encounterOutcome.OpenedChestCount);
        _resultEliteDefeated = eliteOutcome.IsEliteDefeated;
        _resultEliteName = string.IsNullOrEmpty(eliteOutcome.EliteName) ? string.Empty : eliteOutcome.EliteName;
        _resultEliteRewardLabel = string.IsNullOrEmpty(eliteOutcome.EliteRewardLabel) ? string.Empty : eliteOutcome.EliteRewardLabel;
        _resultEliteRewardAmount = Mathf.Max(0, eliteOutcome.EliteRewardAmount);
        _resultEliteBonusRewardAmount = Mathf.Max(0, eliteOutcome.EliteBonusRewardAmount);
        _resultRoomPathSummaryText = string.IsNullOrEmpty(encounterOutcome.RoomPathSummary) ? string.Empty : encounterOutcome.RoomPathSummary;
    }

    private PrototypeRpgProgressionSeedSnapshot CopyRpgProgressionSeedSnapshot(PrototypeRpgProgressionSeedSnapshot source)
    {
        PrototypeRpgProgressionSeedSnapshot copy = new PrototypeRpgProgressionSeedSnapshot();
        if (source == null)
        {
            return copy;
        }

        copy.ResultStateKey = string.IsNullOrEmpty(source.ResultStateKey) ? string.Empty : source.ResultStateKey;
        copy.DungeonId = string.IsNullOrEmpty(source.DungeonId) ? string.Empty : source.DungeonId;
        copy.DungeonLabel = string.IsNullOrEmpty(source.DungeonLabel) ? string.Empty : source.DungeonLabel;
        copy.DungeonDangerLabel = string.IsNullOrEmpty(source.DungeonDangerLabel) ? string.Empty : source.DungeonDangerLabel;
        copy.RouteId = string.IsNullOrEmpty(source.RouteId) ? string.Empty : source.RouteId;
        copy.RouteLabel = string.IsNullOrEmpty(source.RouteLabel) ? string.Empty : source.RouteLabel;
        copy.RouteRiskLabel = string.IsNullOrEmpty(source.RouteRiskLabel) ? string.Empty : source.RouteRiskLabel;
        copy.TotalTurnsTaken = Mathf.Max(0, source.TotalTurnsTaken);
        copy.ClearedEncounterCount = Mathf.Max(0, source.ClearedEncounterCount);
        copy.EliteDefeated = source.EliteDefeated;
        copy.EliteName = string.IsNullOrEmpty(source.EliteName) ? string.Empty : source.EliteName;
        copy.EliteTypeLabel = string.IsNullOrEmpty(source.EliteTypeLabel) ? string.Empty : source.EliteTypeLabel;
        copy.PartyConditionText = string.IsNullOrEmpty(source.PartyConditionText) ? string.Empty : source.PartyConditionText;
        copy.SurvivingMemberCount = Mathf.Max(0, source.SurvivingMemberCount);
        copy.KnockedOutMemberCount = Mathf.Max(0, source.KnockedOutMemberCount);
        copy.EliteClearBonusHint = string.IsNullOrEmpty(source.EliteClearBonusHint) ? string.Empty : source.EliteClearBonusHint;
        copy.RouteRiskHint = string.IsNullOrEmpty(source.RouteRiskHint) ? string.Empty : source.RouteRiskHint;
        copy.DungeonDangerHint = string.IsNullOrEmpty(source.DungeonDangerHint) ? string.Empty : source.DungeonDangerHint;
        copy.Loot = CopyRpgLootSeed(source.Loot);

        if (source.Members == null || source.Members.Length == 0)
        {
            copy.Members = System.Array.Empty<PrototypeRpgMemberProgressionSeed>();
        }
        else
        {
            PrototypeRpgMemberProgressionSeed[] members = new PrototypeRpgMemberProgressionSeed[source.Members.Length];
            for (int i = 0; i < source.Members.Length; i++)
            {
                members[i] = CopyRpgMemberProgressionSeed(source.Members[i]);
            }

            copy.Members = members;
        }

        copy.RewardTags = source.RewardTags != null && source.RewardTags.Length > 0 ? (string[])source.RewardTags.Clone() : System.Array.Empty<string>();
        copy.GrowthTags = source.GrowthTags != null && source.GrowthTags.Length > 0 ? (string[])source.GrowthTags.Clone() : System.Array.Empty<string>();
        return copy;
    }

    private PrototypeRpgMemberProgressionSeed CopyRpgMemberProgressionSeed(PrototypeRpgMemberProgressionSeed source)
    {
        PrototypeRpgMemberProgressionSeed copy = new PrototypeRpgMemberProgressionSeed();
        if (source == null)
        {
            return copy;
        }

        copy.MemberId = string.IsNullOrEmpty(source.MemberId) ? string.Empty : source.MemberId;
        copy.DisplayName = string.IsNullOrEmpty(source.DisplayName) ? string.Empty : source.DisplayName;
        copy.RoleTag = string.IsNullOrEmpty(source.RoleTag) ? string.Empty : source.RoleTag;
        copy.RoleLabel = string.IsNullOrEmpty(source.RoleLabel) ? string.Empty : source.RoleLabel;
        copy.DefaultSkillId = string.IsNullOrEmpty(source.DefaultSkillId) ? string.Empty : source.DefaultSkillId;
        copy.Survived = source.Survived;
        copy.KnockedOut = source.KnockedOut;
        copy.CurrentHp = Mathf.Max(0, source.CurrentHp);
        copy.MaxHp = Mathf.Max(1, source.MaxHp);
        copy.Combat = CopyRpgCombatContributionSeed(source.Combat);
        return copy;
    }

    private PrototypeRpgCombatContributionSeed CopyRpgCombatContributionSeed(PrototypeRpgCombatContributionSeed source)
    {
        PrototypeRpgCombatContributionSeed copy = new PrototypeRpgCombatContributionSeed();
        if (source == null)
        {
            return copy;
        }

        copy.DamageDealt = Mathf.Max(0, source.DamageDealt);
        copy.DamageTaken = Mathf.Max(0, source.DamageTaken);
        copy.HealingDone = Mathf.Max(0, source.HealingDone);
        copy.ActionCount = Mathf.Max(0, source.ActionCount);
        return copy;
    }

    private PrototypeRpgLootSeed CopyRpgLootSeed(PrototypeRpgLootSeed source)
    {
        PrototypeRpgLootSeed copy = new PrototypeRpgLootSeed();
        if (source == null)
        {
            return copy;
        }

        copy.TotalLootGained = Mathf.Max(0, source.TotalLootGained);
        copy.BattleLootGained = Mathf.Max(0, source.BattleLootGained);
        copy.ChestLootGained = Mathf.Max(0, source.ChestLootGained);
        copy.EventLootGained = Mathf.Max(0, source.EventLootGained);
        copy.EliteRewardAmount = Mathf.Max(0, source.EliteRewardAmount);
        copy.EliteBonusRewardAmount = Mathf.Max(0, source.EliteBonusRewardAmount);
        copy.LootBreakdownSummary = string.IsNullOrEmpty(source.LootBreakdownSummary) ? string.Empty : source.LootBreakdownSummary;
        return copy;
    }

    private void ResetRpgProgressionSeedState()
    {
        _latestRpgProgressionSeedSnapshot = new PrototypeRpgProgressionSeedSnapshot();
        _latestRpgCombatContributionSnapshot = new PrototypeRpgCombatContributionSnapshot();
        _latestRpgProgressionPreviewSnapshot = new PrototypeRpgProgressionPreviewSnapshot();
        for (int i = 0; i < _runMemberDamageDealt.Length; i++)
        {
            _runMemberDamageDealt[i] = 0;
            _runMemberDamageTaken[i] = 0;
            _runMemberHealingDone[i] = 0;
            _runMemberActionCount[i] = 0;
            _runMemberKillCount[i] = 0;
        }
    }

    private void AddRunMemberContributionValue(int[] values, int memberIndex, int amount)
    {
        if (values == null || memberIndex < 0 || memberIndex >= values.Length || amount <= 0)
        {
            return;
        }

        values[memberIndex] += amount;
    }

    private int GetRunMemberContributionValue(int[] values, int memberIndex)
    {
        if (values == null || memberIndex < 0 || memberIndex >= values.Length)
        {
            return 0;
        }

        return Mathf.Max(0, values[memberIndex]);
    }

    private void AddProgressionTag(List<string> tags, string tag)
    {
        if (tags == null || string.IsNullOrEmpty(tag) || tags.Contains(tag))
        {
            return;
        }

        tags.Add(tag);
    }

    private string[] BuildRpgProgressionRewardTags(PrototypeRpgRunResultSnapshot snapshot)
    {
        List<string> tags = new List<string>();
        if (snapshot != null && !string.IsNullOrEmpty(snapshot.ResultStateKey) && snapshot.ResultStateKey != PrototypeBattleOutcomeKeys.None)
        {
            AddProgressionTag(tags, snapshot.ResultStateKey);
        }

        if (snapshot != null && snapshot.LootOutcome != null && snapshot.LootOutcome.TotalLootGained > 0)
        {
            AddProgressionTag(tags, "loot_returned");
        }

        if (snapshot != null && snapshot.EliteOutcome != null && snapshot.EliteOutcome.IsEliteDefeated)
        {
            AddProgressionTag(tags, "elite_clear");
        }

        if (snapshot != null && snapshot.EliteOutcome != null && snapshot.EliteOutcome.EliteBonusRewardEarned)
        {
            AddProgressionTag(tags, "elite_bonus");
        }

        if (_selectedRouteId == RiskyRouteId)
        {
            AddProgressionTag(tags, "risky_route");
        }

        return tags.Count > 0 ? tags.ToArray() : System.Array.Empty<string>();
    }

    private string[] BuildRpgProgressionGrowthTags(PrototypeRpgRunResultSnapshot snapshot)
    {
        List<string> tags = new List<string>();
        PrototypeRpgPartyOutcomeSnapshot partyOutcome = snapshot != null ? snapshot.PartyOutcome : null;
        if (partyOutcome != null && partyOutcome.KnockedOutMemberCount > 0)
        {
            AddProgressionTag(tags, "party_recovery");
        }

        if (partyOutcome != null && partyOutcome.SurvivingMemberCount > 0 && partyOutcome.KnockedOutMemberCount <= 0)
        {
            AddProgressionTag(tags, "clean_survival");
        }

        if (snapshot != null && snapshot.TotalTurnsTaken >= 6)
        {
            AddProgressionTag(tags, "endurance_run");
        }

        if (snapshot != null && snapshot.EliteOutcome != null && snapshot.EliteOutcome.IsEliteDefeated)
        {
            AddProgressionTag(tags, "elite_mastery");
        }

        return tags.Count > 0 ? tags.ToArray() : System.Array.Empty<string>();
    }

    private PrototypeRpgLootSeed BuildRpgProgressionLootSeed(PrototypeRpgRunResultSnapshot snapshot)
    {
        PrototypeRpgLootSeed seed = new PrototypeRpgLootSeed();
        PrototypeRpgLootOutcomeSnapshot lootOutcome = snapshot != null ? snapshot.LootOutcome : null;
        if (lootOutcome == null)
        {
            return seed;
        }

        seed.TotalLootGained = Mathf.Max(0, lootOutcome.TotalLootGained);
        seed.BattleLootGained = Mathf.Max(0, lootOutcome.BattleLootGained);
        seed.ChestLootGained = Mathf.Max(0, lootOutcome.ChestLootGained);
        seed.EventLootGained = Mathf.Max(0, lootOutcome.EventLootGained);
        seed.EliteRewardAmount = Mathf.Max(0, lootOutcome.EliteRewardAmount);
        seed.EliteBonusRewardAmount = Mathf.Max(0, lootOutcome.EliteBonusRewardAmount);
        seed.LootBreakdownSummary = string.IsNullOrEmpty(lootOutcome.FinalLootSummary) ? string.Empty : lootOutcome.FinalLootSummary;
        return seed;
    }

    private PrototypeRpgProgressionSeedSnapshot BuildRpgProgressionSeedSnapshot(PrototypeRpgRunResultSnapshot runResultSnapshot)
    {
        PrototypeRpgProgressionSeedSnapshot snapshot = new PrototypeRpgProgressionSeedSnapshot();
        PrototypeRpgRunResultSnapshot safeRunResult = runResultSnapshot ?? new PrototypeRpgRunResultSnapshot();
        PrototypeRpgPartyOutcomeSnapshot partyOutcome = safeRunResult.PartyOutcome ?? new PrototypeRpgPartyOutcomeSnapshot();
        PrototypeRpgEliteOutcomeSnapshot eliteOutcome = safeRunResult.EliteOutcome ?? new PrototypeRpgEliteOutcomeSnapshot();
        PrototypeRpgEncounterOutcomeSnapshot encounterOutcome = safeRunResult.EncounterOutcome ?? new PrototypeRpgEncounterOutcomeSnapshot();
        PrototypeRpgPartyMemberOutcomeSnapshot[] members = partyOutcome.Members ?? System.Array.Empty<PrototypeRpgPartyMemberOutcomeSnapshot>();

        snapshot.ResultStateKey = string.IsNullOrEmpty(safeRunResult.ResultStateKey) ? string.Empty : safeRunResult.ResultStateKey;
        snapshot.DungeonId = string.IsNullOrEmpty(safeRunResult.DungeonId) ? string.Empty : safeRunResult.DungeonId;
        snapshot.DungeonLabel = string.IsNullOrEmpty(safeRunResult.DungeonLabel) ? string.Empty : safeRunResult.DungeonLabel;
        snapshot.DungeonDangerLabel = GetCurrentDungeonDangerText();
        snapshot.RouteId = string.IsNullOrEmpty(safeRunResult.RouteId) ? string.Empty : safeRunResult.RouteId;
        snapshot.RouteLabel = string.IsNullOrEmpty(safeRunResult.RouteLabel) ? string.Empty : safeRunResult.RouteLabel;
        snapshot.RouteRiskLabel = string.IsNullOrEmpty(_selectedRouteRiskLabel) ? string.Empty : _selectedRouteRiskLabel;
        snapshot.TotalTurnsTaken = Mathf.Max(0, safeRunResult.TotalTurnsTaken);
        snapshot.ClearedEncounterCount = Mathf.Max(0, encounterOutcome.ClearedEncounterCount);
        snapshot.EliteDefeated = eliteOutcome.IsEliteDefeated;
        snapshot.EliteName = string.IsNullOrEmpty(eliteOutcome.EliteName) ? string.Empty : eliteOutcome.EliteName;
        snapshot.EliteTypeLabel = string.IsNullOrEmpty(eliteOutcome.EliteTypeLabel) ? string.Empty : eliteOutcome.EliteTypeLabel;
        snapshot.PartyConditionText = string.IsNullOrEmpty(partyOutcome.PartyConditionText) ? string.Empty : partyOutcome.PartyConditionText;
        snapshot.SurvivingMemberCount = Mathf.Max(0, partyOutcome.SurvivingMemberCount);
        snapshot.KnockedOutMemberCount = Mathf.Max(0, partyOutcome.KnockedOutMemberCount);
        snapshot.EliteClearBonusHint = eliteOutcome.EliteBonusRewardEarned ? "Elite bonus reward earned." : eliteOutcome.IsEliteDefeated ? "Elite clear completed." : "Elite clear not achieved.";
        snapshot.RouteRiskHint = string.IsNullOrEmpty(snapshot.RouteRiskLabel) ? "Route risk: none." : "Route risk: " + snapshot.RouteRiskLabel + ".";
        snapshot.DungeonDangerHint = string.IsNullOrEmpty(snapshot.DungeonDangerLabel) ? "Dungeon danger: none." : "Dungeon danger: " + snapshot.DungeonDangerLabel + ".";
        snapshot.Loot = BuildRpgProgressionLootSeed(safeRunResult);
        snapshot.RewardTags = BuildRpgProgressionRewardTags(safeRunResult);
        snapshot.GrowthTags = BuildRpgProgressionGrowthTags(safeRunResult);

        if (members.Length <= 0)
        {
            snapshot.Members = System.Array.Empty<PrototypeRpgMemberProgressionSeed>();
            return snapshot;
        }

        PrototypeRpgMemberProgressionSeed[] memberSeeds = new PrototypeRpgMemberProgressionSeed[members.Length];
        for (int i = 0; i < members.Length; i++)
        {
            PrototypeRpgPartyMemberOutcomeSnapshot memberOutcome = members[i] ?? new PrototypeRpgPartyMemberOutcomeSnapshot();
            PrototypeRpgMemberProgressionSeed memberSeed = new PrototypeRpgMemberProgressionSeed();
            memberSeed.MemberId = string.IsNullOrEmpty(memberOutcome.MemberId) ? string.Empty : memberOutcome.MemberId;
            memberSeed.DisplayName = string.IsNullOrEmpty(memberOutcome.DisplayName) ? string.Empty : memberOutcome.DisplayName;
            memberSeed.RoleTag = string.IsNullOrEmpty(memberOutcome.RoleTag) ? string.Empty : memberOutcome.RoleTag;
            memberSeed.RoleLabel = string.IsNullOrEmpty(memberOutcome.RoleLabel) ? string.Empty : memberOutcome.RoleLabel;
            memberSeed.DefaultSkillId = string.IsNullOrEmpty(memberOutcome.DefaultSkillId) ? string.Empty : memberOutcome.DefaultSkillId;
            memberSeed.Survived = memberOutcome.Survived;
            memberSeed.KnockedOut = memberOutcome.KnockedOut;
            memberSeed.CurrentHp = Mathf.Max(0, memberOutcome.CurrentHp);
            memberSeed.MaxHp = Mathf.Max(1, memberOutcome.MaxHp);
            memberSeed.Combat = new PrototypeRpgCombatContributionSeed
            {
                DamageDealt = GetRunMemberContributionValue(_runMemberDamageDealt, i),
                DamageTaken = GetRunMemberContributionValue(_runMemberDamageTaken, i),
                HealingDone = GetRunMemberContributionValue(_runMemberHealingDone, i),
                ActionCount = GetRunMemberContributionValue(_runMemberActionCount, i)
            };
            memberSeeds[i] = memberSeed;
        }

        snapshot.Members = memberSeeds;
        return snapshot;
    }

    private PrototypeRpgCombatContributionSnapshot CopyRpgCombatContributionSnapshot(PrototypeRpgCombatContributionSnapshot source)
    {
        PrototypeRpgCombatContributionSnapshot copy = new PrototypeRpgCombatContributionSnapshot();
        if (source == null)
        {
            return copy;
        }

        copy.ResultStateKey = string.IsNullOrEmpty(source.ResultStateKey) ? string.Empty : source.ResultStateKey;
        copy.DungeonId = string.IsNullOrEmpty(source.DungeonId) ? string.Empty : source.DungeonId;
        copy.DungeonLabel = string.IsNullOrEmpty(source.DungeonLabel) ? string.Empty : source.DungeonLabel;
        copy.RouteId = string.IsNullOrEmpty(source.RouteId) ? string.Empty : source.RouteId;
        copy.RouteLabel = string.IsNullOrEmpty(source.RouteLabel) ? string.Empty : source.RouteLabel;
        copy.TotalTurnsTaken = Mathf.Max(0, source.TotalTurnsTaken);
        copy.SurvivingMemberCount = Mathf.Max(0, source.SurvivingMemberCount);
        copy.KnockedOutMemberCount = Mathf.Max(0, source.KnockedOutMemberCount);
        copy.TotalDamageDealt = Mathf.Max(0, source.TotalDamageDealt);
        copy.TotalDamageTaken = Mathf.Max(0, source.TotalDamageTaken);
        copy.TotalHealingDone = Mathf.Max(0, source.TotalHealingDone);

        PrototypeRpgMemberContributionSnapshot[] sourceMembers = source.Members ?? System.Array.Empty<PrototypeRpgMemberContributionSnapshot>();
        if (sourceMembers.Length <= 0)
        {
            copy.Members = System.Array.Empty<PrototypeRpgMemberContributionSnapshot>();
        }
        else
        {
            PrototypeRpgMemberContributionSnapshot[] memberCopies = new PrototypeRpgMemberContributionSnapshot[sourceMembers.Length];
            for (int i = 0; i < sourceMembers.Length; i++)
            {
                memberCopies[i] = CopyRpgMemberContributionSnapshot(sourceMembers[i]);
            }

            copy.Members = memberCopies;
        }

        PrototypeBattleEventRecord[] sourceEvents = source.RecentEvents ?? System.Array.Empty<PrototypeBattleEventRecord>();
        if (sourceEvents.Length <= 0)
        {
            copy.RecentEvents = System.Array.Empty<PrototypeBattleEventRecord>();
        }
        else
        {
            PrototypeBattleEventRecord[] eventCopies = new PrototypeBattleEventRecord[sourceEvents.Length];
            for (int i = 0; i < sourceEvents.Length; i++)
            {
                eventCopies[i] = CopyBattleEventRecord(sourceEvents[i]);
            }

            copy.RecentEvents = eventCopies;
        }

        return copy;
    }

    private PrototypeRpgMemberContributionSnapshot CopyRpgMemberContributionSnapshot(PrototypeRpgMemberContributionSnapshot source)
    {
        PrototypeRpgMemberContributionSnapshot copy = new PrototypeRpgMemberContributionSnapshot();
        if (source == null)
        {
            return copy;
        }

        copy.MemberId = string.IsNullOrEmpty(source.MemberId) ? string.Empty : source.MemberId;
        copy.DisplayName = string.IsNullOrEmpty(source.DisplayName) ? string.Empty : source.DisplayName;
        copy.RoleTag = string.IsNullOrEmpty(source.RoleTag) ? string.Empty : source.RoleTag;
        copy.RoleLabel = string.IsNullOrEmpty(source.RoleLabel) ? string.Empty : source.RoleLabel;
        copy.DefaultSkillId = string.IsNullOrEmpty(source.DefaultSkillId) ? string.Empty : source.DefaultSkillId;
        copy.DamageDealt = Mathf.Max(0, source.DamageDealt);
        copy.DamageTaken = Mathf.Max(0, source.DamageTaken);
        copy.HealingDone = Mathf.Max(0, source.HealingDone);
        copy.ActionCount = Mathf.Max(0, source.ActionCount);
        copy.KillCount = Mathf.Max(0, source.KillCount);
        copy.KnockedOut = source.KnockedOut;
        copy.Survived = source.Survived;
        copy.EliteVictor = source.EliteVictor;
        return copy;
    }

    private PrototypeRpgProgressionPreviewSnapshot CopyRpgProgressionPreviewSnapshot(PrototypeRpgProgressionPreviewSnapshot source)
    {
        PrototypeRpgProgressionPreviewSnapshot copy = new PrototypeRpgProgressionPreviewSnapshot();
        if (source == null)
        {
            return copy;
        }

        copy.ResultStateKey = string.IsNullOrEmpty(source.ResultStateKey) ? string.Empty : source.ResultStateKey;
        copy.DungeonId = string.IsNullOrEmpty(source.DungeonId) ? string.Empty : source.DungeonId;
        copy.DungeonLabel = string.IsNullOrEmpty(source.DungeonLabel) ? string.Empty : source.DungeonLabel;
        copy.DungeonDangerLabel = string.IsNullOrEmpty(source.DungeonDangerLabel) ? string.Empty : source.DungeonDangerLabel;
        copy.RouteId = string.IsNullOrEmpty(source.RouteId) ? string.Empty : source.RouteId;
        copy.RouteLabel = string.IsNullOrEmpty(source.RouteLabel) ? string.Empty : source.RouteLabel;
        copy.RouteRiskLabel = string.IsNullOrEmpty(source.RouteRiskLabel) ? string.Empty : source.RouteRiskLabel;
        copy.EliteDefeated = source.EliteDefeated;
        copy.TotalLootGained = Mathf.Max(0, source.TotalLootGained);
        copy.RewardHintTags = source.RewardHintTags != null && source.RewardHintTags.Length > 0 ? (string[])source.RewardHintTags.Clone() : System.Array.Empty<string>();
        copy.GrowthHintTags = source.GrowthHintTags != null && source.GrowthHintTags.Length > 0 ? (string[])source.GrowthHintTags.Clone() : System.Array.Empty<string>();

        PrototypeRpgMemberProgressPreview[] sourceMembers = source.Members ?? System.Array.Empty<PrototypeRpgMemberProgressPreview>();
        if (sourceMembers.Length <= 0)
        {
            copy.Members = System.Array.Empty<PrototypeRpgMemberProgressPreview>();
        }
        else
        {
            PrototypeRpgMemberProgressPreview[] memberCopies = new PrototypeRpgMemberProgressPreview[sourceMembers.Length];
            for (int i = 0; i < sourceMembers.Length; i++)
            {
                memberCopies[i] = CopyRpgMemberProgressPreview(sourceMembers[i]);
            }

            copy.Members = memberCopies;
        }

        return copy;
    }

    private PrototypeRpgMemberProgressPreview CopyRpgMemberProgressPreview(PrototypeRpgMemberProgressPreview source)
    {
        PrototypeRpgMemberProgressPreview copy = new PrototypeRpgMemberProgressPreview();
        if (source == null)
        {
            return copy;
        }

        copy.MemberId = string.IsNullOrEmpty(source.MemberId) ? string.Empty : source.MemberId;
        copy.DisplayName = string.IsNullOrEmpty(source.DisplayName) ? string.Empty : source.DisplayName;
        copy.RoleTag = string.IsNullOrEmpty(source.RoleTag) ? string.Empty : source.RoleTag;
        copy.RoleLabel = string.IsNullOrEmpty(source.RoleLabel) ? string.Empty : source.RoleLabel;
        copy.Survived = source.Survived;
        copy.Contribution = CopyRpgMemberContributionSnapshot(source.Contribution);
        copy.SuggestedGrowthHintTags = source.SuggestedGrowthHintTags != null && source.SuggestedGrowthHintTags.Length > 0 ? (string[])source.SuggestedGrowthHintTags.Clone() : System.Array.Empty<string>();
        copy.SuggestedRewardHintTags = source.SuggestedRewardHintTags != null && source.SuggestedRewardHintTags.Length > 0 ? (string[])source.SuggestedRewardHintTags.Clone() : System.Array.Empty<string>();
        copy.NotableOutcomeKey = string.IsNullOrEmpty(source.NotableOutcomeKey) ? string.Empty : source.NotableOutcomeKey;
        return copy;
    }

    private PrototypeRpgCombatContributionSnapshot BuildRpgCombatContributionSnapshot(PrototypeRpgRunResultSnapshot runResultSnapshot)
    {
        PrototypeRpgCombatContributionSnapshot snapshot = new PrototypeRpgCombatContributionSnapshot();
        PrototypeRpgRunResultSnapshot safeRunResult = runResultSnapshot ?? new PrototypeRpgRunResultSnapshot();
        PrototypeRpgPartyOutcomeSnapshot partyOutcome = safeRunResult.PartyOutcome ?? new PrototypeRpgPartyOutcomeSnapshot();
        PrototypeRpgPartyMemberOutcomeSnapshot[] members = partyOutcome.Members ?? System.Array.Empty<PrototypeRpgPartyMemberOutcomeSnapshot>();

        snapshot.ResultStateKey = string.IsNullOrEmpty(safeRunResult.ResultStateKey) ? string.Empty : safeRunResult.ResultStateKey;
        snapshot.DungeonId = string.IsNullOrEmpty(safeRunResult.DungeonId) ? string.Empty : safeRunResult.DungeonId;
        snapshot.DungeonLabel = string.IsNullOrEmpty(safeRunResult.DungeonLabel) ? string.Empty : safeRunResult.DungeonLabel;
        snapshot.RouteId = string.IsNullOrEmpty(safeRunResult.RouteId) ? string.Empty : safeRunResult.RouteId;
        snapshot.RouteLabel = string.IsNullOrEmpty(safeRunResult.RouteLabel) ? string.Empty : safeRunResult.RouteLabel;
        snapshot.TotalTurnsTaken = Mathf.Max(0, safeRunResult.TotalTurnsTaken);
        snapshot.SurvivingMemberCount = Mathf.Max(0, partyOutcome.SurvivingMemberCount);
        snapshot.KnockedOutMemberCount = Mathf.Max(0, partyOutcome.KnockedOutMemberCount);
        snapshot.TotalDamageDealt = Mathf.Max(0, _totalDamageDealt);
        snapshot.TotalDamageTaken = Mathf.Max(0, _totalDamageTaken);
        snapshot.TotalHealingDone = Mathf.Max(0, _totalHealingDone);
        snapshot.RecentEvents = BuildRecentBattleEventRecords();

        if (members.Length <= 0)
        {
            snapshot.Members = System.Array.Empty<PrototypeRpgMemberContributionSnapshot>();
            return snapshot;
        }

        PrototypeRpgMemberContributionSnapshot[] memberSnapshots = new PrototypeRpgMemberContributionSnapshot[members.Length];
        bool eliteDefeated = safeRunResult.EliteOutcome != null && safeRunResult.EliteOutcome.IsEliteDefeated;
        for (int i = 0; i < members.Length; i++)
        {
            PrototypeRpgPartyMemberOutcomeSnapshot memberOutcome = members[i] ?? new PrototypeRpgPartyMemberOutcomeSnapshot();
            PrototypeRpgMemberContributionSnapshot memberSnapshot = new PrototypeRpgMemberContributionSnapshot();
            memberSnapshot.MemberId = string.IsNullOrEmpty(memberOutcome.MemberId) ? string.Empty : memberOutcome.MemberId;
            memberSnapshot.DisplayName = string.IsNullOrEmpty(memberOutcome.DisplayName) ? string.Empty : memberOutcome.DisplayName;
            memberSnapshot.RoleTag = string.IsNullOrEmpty(memberOutcome.RoleTag) ? string.Empty : memberOutcome.RoleTag;
            memberSnapshot.RoleLabel = string.IsNullOrEmpty(memberOutcome.RoleLabel) ? string.Empty : memberOutcome.RoleLabel;
            memberSnapshot.DefaultSkillId = string.IsNullOrEmpty(memberOutcome.DefaultSkillId) ? string.Empty : memberOutcome.DefaultSkillId;
            memberSnapshot.DamageDealt = GetRunMemberContributionValue(_runMemberDamageDealt, i);
            memberSnapshot.DamageTaken = GetRunMemberContributionValue(_runMemberDamageTaken, i);
            memberSnapshot.HealingDone = GetRunMemberContributionValue(_runMemberHealingDone, i);
            memberSnapshot.ActionCount = GetRunMemberContributionValue(_runMemberActionCount, i);
            memberSnapshot.KillCount = GetRunMemberContributionValue(_runMemberKillCount, i);
            memberSnapshot.KnockedOut = memberOutcome.KnockedOut;
            memberSnapshot.Survived = memberOutcome.Survived;
            memberSnapshot.EliteVictor = eliteDefeated && memberOutcome.Survived;
            memberSnapshots[i] = memberSnapshot;
        }

        snapshot.Members = memberSnapshots;
        return snapshot;
    }

    private string[] BuildRpgProgressionPreviewRewardHintTags(PrototypeRpgRunResultSnapshot runResultSnapshot, PrototypeRpgProgressionSeedSnapshot progressionSeedSnapshot)
    {
        List<string> tags = new List<string>();
        string[] seedTags = progressionSeedSnapshot != null ? (progressionSeedSnapshot.RewardTags ?? System.Array.Empty<string>()) : System.Array.Empty<string>();
        for (int i = 0; i < seedTags.Length; i++)
        {
            AddProgressionTag(tags, seedTags[i]);
        }

        if (runResultSnapshot != null && runResultSnapshot.EliteOutcome != null && runResultSnapshot.EliteOutcome.IsEliteDefeated)
        {
            AddProgressionTag(tags, "elite_victor");
        }

        if (runResultSnapshot != null && runResultSnapshot.ResultStateKey == PrototypeBattleOutcomeKeys.RunRetreat)
        {
            AddProgressionTag(tags, "retreat_penalty_hint");
        }

        bool riskyRoute = (runResultSnapshot != null && runResultSnapshot.RouteId == RiskyRouteId) || (progressionSeedSnapshot != null && progressionSeedSnapshot.RouteId == RiskyRouteId);
        if (riskyRoute)
        {
            AddProgressionTag(tags, "risky_route_hint");
        }

        return tags.Count > 0 ? tags.ToArray() : System.Array.Empty<string>();
    }

    private string[] BuildRpgProgressionPreviewGrowthHintTags(PrototypeRpgProgressionSeedSnapshot progressionSeedSnapshot, PrototypeRpgCombatContributionSnapshot contributionSnapshot)
    {
        List<string> tags = new List<string>();
        string[] seedTags = progressionSeedSnapshot != null ? (progressionSeedSnapshot.GrowthTags ?? System.Array.Empty<string>()) : System.Array.Empty<string>();
        for (int i = 0; i < seedTags.Length; i++)
        {
            AddProgressionTag(tags, seedTags[i]);
        }

        if (contributionSnapshot != null && contributionSnapshot.KnockedOutMemberCount > 0)
        {
            AddProgressionTag(tags, "knocked_out");
        }

        if (contributionSnapshot != null && contributionSnapshot.SurvivingMemberCount > 0)
        {
            AddProgressionTag(tags, "survivor");
        }

        if (contributionSnapshot != null && contributionSnapshot.TotalHealingDone > 0)
        {
            AddProgressionTag(tags, "party_support");
        }

        return tags.Count > 0 ? tags.ToArray() : System.Array.Empty<string>();
    }

    private string[] BuildMemberProgressionGrowthHintTags(PrototypeRpgMemberContributionSnapshot contribution)
    {
        List<string> tags = new List<string>();
        if (contribution == null)
        {
            return System.Array.Empty<string>();
        }

        if (contribution.RoleTag == "warrior" && contribution.DamageTaken > 0 && contribution.DamageTaken >= contribution.DamageDealt)
        {
            AddProgressionTag(tags, "frontline_pressure");
        }

        if (contribution.RoleTag == "mage" && contribution.DamageDealt >= 8)
        {
            AddProgressionTag(tags, "aoe_clear");
        }

        if (contribution.RoleTag == "cleric" && contribution.HealingDone > 0)
        {
            AddProgressionTag(tags, "party_support");
        }

        if (contribution.Survived)
        {
            AddProgressionTag(tags, "survivor");
        }

        if (contribution.KnockedOut)
        {
            AddProgressionTag(tags, "knocked_out");
        }

        return tags.Count > 0 ? tags.ToArray() : System.Array.Empty<string>();
    }

    private string[] BuildMemberProgressionRewardHintTags(PrototypeRpgMemberContributionSnapshot contribution, PrototypeRpgRunResultSnapshot runResultSnapshot)
    {
        List<string> tags = new List<string>();
        if (contribution == null)
        {
            return System.Array.Empty<string>();
        }

        if (contribution.RoleTag == "rogue" && contribution.KillCount > 0)
        {
            AddProgressionTag(tags, "finisher");
        }

        if (runResultSnapshot != null && runResultSnapshot.EliteOutcome != null && runResultSnapshot.EliteOutcome.IsEliteDefeated && contribution.Survived)
        {
            AddProgressionTag(tags, "elite_victor");
        }

        if (runResultSnapshot != null && runResultSnapshot.ResultStateKey == PrototypeBattleOutcomeKeys.RunRetreat)
        {
            AddProgressionTag(tags, "retreat_penalty_hint");
        }

        if (runResultSnapshot != null && runResultSnapshot.RouteId == RiskyRouteId)
        {
            AddProgressionTag(tags, "risky_route_hint");
        }

        return tags.Count > 0 ? tags.ToArray() : System.Array.Empty<string>();
    }

    private string BuildMemberNotableOutcomeKey(PrototypeRpgMemberContributionSnapshot contribution, PrototypeRpgRunResultSnapshot runResultSnapshot)
    {
        if (contribution == null)
        {
            return string.Empty;
        }

        if (contribution.KnockedOut)
        {
            return "knocked_out";
        }

        if (runResultSnapshot != null && runResultSnapshot.EliteOutcome != null && runResultSnapshot.EliteOutcome.IsEliteDefeated && contribution.Survived)
        {
            return "elite_victor";
        }

        if (contribution.RoleTag == "cleric" && contribution.HealingDone > 0)
        {
            return "party_support";
        }

        if (contribution.RoleTag == "mage" && contribution.DamageDealt >= 8)
        {
            return "aoe_clear";
        }

        if (contribution.RoleTag == "rogue" && contribution.KillCount > 0)
        {
            return "finisher";
        }

        if (contribution.RoleTag == "warrior" && contribution.DamageTaken > 0 && contribution.DamageTaken >= contribution.DamageDealt)
        {
            return "frontline_pressure";
        }

        if (runResultSnapshot != null && runResultSnapshot.ResultStateKey == PrototypeBattleOutcomeKeys.RunRetreat)
        {
            return "retreat_penalty_hint";
        }

        if (runResultSnapshot != null && runResultSnapshot.RouteId == RiskyRouteId)
        {
            return "risky_route_hint";
        }

        if (contribution.Survived)
        {
            return "survivor";
        }

        return string.Empty;
    }

    private PrototypeRpgProgressionPreviewSnapshot BuildRpgProgressionPreviewSnapshot(PrototypeRpgRunResultSnapshot runResultSnapshot, PrototypeRpgProgressionSeedSnapshot progressionSeedSnapshot, PrototypeRpgCombatContributionSnapshot contributionSnapshot)
    {
        PrototypeRpgProgressionPreviewSnapshot snapshot = new PrototypeRpgProgressionPreviewSnapshot();
        PrototypeRpgRunResultSnapshot safeRunResult = runResultSnapshot ?? new PrototypeRpgRunResultSnapshot();
        PrototypeRpgProgressionSeedSnapshot safeSeed = progressionSeedSnapshot ?? new PrototypeRpgProgressionSeedSnapshot();
        PrototypeRpgCombatContributionSnapshot safeContribution = contributionSnapshot ?? new PrototypeRpgCombatContributionSnapshot();
        PrototypeRpgMemberContributionSnapshot[] contributionMembers = safeContribution.Members ?? System.Array.Empty<PrototypeRpgMemberContributionSnapshot>();

        snapshot.ResultStateKey = string.IsNullOrEmpty(safeRunResult.ResultStateKey) ? string.Empty : safeRunResult.ResultStateKey;
        snapshot.DungeonId = string.IsNullOrEmpty(safeRunResult.DungeonId) ? string.Empty : safeRunResult.DungeonId;
        snapshot.DungeonLabel = string.IsNullOrEmpty(safeRunResult.DungeonLabel) ? string.Empty : safeRunResult.DungeonLabel;
        snapshot.DungeonDangerLabel = string.IsNullOrEmpty(safeSeed.DungeonDangerLabel) ? string.Empty : safeSeed.DungeonDangerLabel;
        snapshot.RouteId = string.IsNullOrEmpty(safeRunResult.RouteId) ? string.Empty : safeRunResult.RouteId;
        snapshot.RouteLabel = string.IsNullOrEmpty(safeRunResult.RouteLabel) ? string.Empty : safeRunResult.RouteLabel;
        snapshot.RouteRiskLabel = string.IsNullOrEmpty(safeSeed.RouteRiskLabel) ? string.Empty : safeSeed.RouteRiskLabel;
        snapshot.EliteDefeated = safeSeed.EliteDefeated;
        snapshot.TotalLootGained = safeSeed.Loot != null ? Mathf.Max(0, safeSeed.Loot.TotalLootGained) : 0;
        snapshot.RewardHintTags = BuildRpgProgressionPreviewRewardHintTags(safeRunResult, safeSeed);
        snapshot.GrowthHintTags = BuildRpgProgressionPreviewGrowthHintTags(safeSeed, safeContribution);

        if (contributionMembers.Length <= 0)
        {
            snapshot.Members = System.Array.Empty<PrototypeRpgMemberProgressPreview>();
            return snapshot;
        }

        PrototypeRpgMemberProgressPreview[] previews = new PrototypeRpgMemberProgressPreview[contributionMembers.Length];
        for (int i = 0; i < contributionMembers.Length; i++)
        {
            PrototypeRpgMemberContributionSnapshot contribution = CopyRpgMemberContributionSnapshot(contributionMembers[i]);
            PrototypeRpgMemberProgressPreview preview = new PrototypeRpgMemberProgressPreview();
            preview.MemberId = string.IsNullOrEmpty(contribution.MemberId) ? string.Empty : contribution.MemberId;
            preview.DisplayName = string.IsNullOrEmpty(contribution.DisplayName) ? string.Empty : contribution.DisplayName;
            preview.RoleTag = string.IsNullOrEmpty(contribution.RoleTag) ? string.Empty : contribution.RoleTag;
            preview.RoleLabel = string.IsNullOrEmpty(contribution.RoleLabel) ? string.Empty : contribution.RoleLabel;
            preview.Survived = contribution.Survived;
            preview.Contribution = contribution;
            preview.SuggestedGrowthHintTags = BuildMemberProgressionGrowthHintTags(contribution);
            preview.SuggestedRewardHintTags = BuildMemberProgressionRewardHintTags(contribution, safeRunResult);
            preview.NotableOutcomeKey = BuildMemberNotableOutcomeKey(contribution, safeRunResult);
            previews[i] = preview;
        }

        snapshot.Members = previews;
        return snapshot;
    }
}
