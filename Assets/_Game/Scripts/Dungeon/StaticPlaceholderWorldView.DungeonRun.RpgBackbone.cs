using System;
using System.Collections.Generic;
using UnityEngine;

public sealed partial class StaticPlaceholderWorldView
{
    private const int RpgBackboneCarryEntryLimit = 4;

    private PrototypeRpgPartyLevelRuntimeState _sessionRpgPartyLevelRuntimeState = new PrototypeRpgPartyLevelRuntimeState();
    private PrototypeRpgInventoryRuntimeState _sessionRpgInventoryRuntimeState = new PrototypeRpgInventoryRuntimeState();

    public string CurrentPartyLevelSummaryText => string.IsNullOrEmpty(_sessionRpgPartyLevelRuntimeState.SummaryText) ? "None" : _sessionRpgPartyLevelRuntimeState.SummaryText;
    public string CurrentActualLevelApplySummaryText => string.IsNullOrEmpty(_sessionRpgPartyLevelRuntimeState.ActualLevelApplySummaryText) ? "None" : _sessionRpgPartyLevelRuntimeState.ActualLevelApplySummaryText;
    public string CurrentDerivedStatHydrateSummaryText => string.IsNullOrEmpty(_sessionRpgPartyLevelRuntimeState.DerivedStatHydrateSummaryText) ? "None" : _sessionRpgPartyLevelRuntimeState.DerivedStatHydrateSummaryText;
    public string CurrentPartyJobSpecializationSummaryText => string.IsNullOrEmpty(_sessionRpgPartyLevelRuntimeState.JobSpecializationSummaryText) ? "None" : _sessionRpgPartyLevelRuntimeState.JobSpecializationSummaryText;
    public string CurrentPartyPassiveSkillSlotSummaryText => string.IsNullOrEmpty(_sessionRpgPartyLevelRuntimeState.PassiveSkillSlotSummaryText) ? "None" : _sessionRpgPartyLevelRuntimeState.PassiveSkillSlotSummaryText;
    public string CurrentPartyRuntimeSynergySummaryText => string.IsNullOrEmpty(_sessionRpgPartyLevelRuntimeState.RuntimeSynergySummaryText) ? "None" : _sessionRpgPartyLevelRuntimeState.RuntimeSynergySummaryText;
    public string CurrentPartyInventorySummaryText => string.IsNullOrEmpty(_sessionRpgInventoryRuntimeState.InventorySummaryText) ? "None" : _sessionRpgInventoryRuntimeState.InventorySummaryText;
    public string CurrentPartyEquipmentSummaryText => string.IsNullOrEmpty(_sessionRpgInventoryRuntimeState.EquipmentSummaryText) ? "None" : _sessionRpgInventoryRuntimeState.EquipmentSummaryText;
    public string CurrentPartyGearContributionSummaryText => string.IsNullOrEmpty(_sessionRpgPartyLevelRuntimeState.GearContributionSummaryText) ? "None" : _sessionRpgPartyLevelRuntimeState.GearContributionSummaryText;
    public string CurrentConsumableCarryoverPolicySummaryText => string.IsNullOrEmpty(_sessionRpgInventoryRuntimeState.CarryoverPolicySummaryText) ? "None" : _sessionRpgInventoryRuntimeState.CarryoverPolicySummaryText;
    public PrototypeRpgPartyLevelRuntimeState LatestRpgPartyLevelRuntimeState => CopyRpgPartyLevelRuntimeState(_sessionRpgPartyLevelRuntimeState);
    public PrototypeRpgInventoryRuntimeState LatestRpgInventoryRuntimeState => CopyRpgInventoryRuntimeState(_sessionRpgInventoryRuntimeState);

    private void ResetRpgBackboneRuntimeState()
    {
        _sessionRpgPartyLevelRuntimeState = new PrototypeRpgPartyLevelRuntimeState();
        _sessionRpgInventoryRuntimeState = new PrototypeRpgInventoryRuntimeState();
    }

    private static string CopyText(string value)
    {
        return string.IsNullOrEmpty(value) ? string.Empty : value;
    }

    private static int ClampNonNegative(int value)
    {
        return Math.Max(0, value);
    }

    private static TTarget[] CopyArray<TSource, TTarget>(TSource[] source, Func<TSource, TTarget> copyItem)
    {
        if (source == null || source.Length <= 0)
        {
            return Array.Empty<TTarget>();
        }

        TTarget[] copies = new TTarget[source.Length];
        for (int i = 0; i < source.Length; i++)
        {
            copies[i] = copyItem(source[i]);
        }

        return copies;
    }

    private PrototypeRpgPartyLevelRuntimeState GetOrCreateRpgPartyLevelRuntimeState(PrototypeRpgPartyDefinition partyDefinition, PrototypeRpgAppliedPartyProgressState appliedProgressState)
    {
        if (partyDefinition == null)
        {
            return _sessionRpgPartyLevelRuntimeState ?? new PrototypeRpgPartyLevelRuntimeState();
        }

        bool rebuildState = _sessionRpgPartyLevelRuntimeState == null ||
                            _sessionRpgPartyLevelRuntimeState.Members == null ||
                            _sessionRpgPartyLevelRuntimeState.PartyId != partyDefinition.PartyId;
        if (rebuildState)
        {
            PrototypeRpgPartyMemberLevelRuntimeState[] members = BuildDefaultRpgPartyLevelMembers(partyDefinition, appliedProgressState);
            _sessionRpgPartyLevelRuntimeState = new PrototypeRpgPartyLevelRuntimeState();
            _sessionRpgPartyLevelRuntimeState.PartyId = string.IsNullOrEmpty(partyDefinition.PartyId) ? string.Empty : partyDefinition.PartyId;
            _sessionRpgPartyLevelRuntimeState.DisplayName = string.IsNullOrEmpty(partyDefinition.DisplayName) ? _sessionRpgPartyLevelRuntimeState.PartyId : partyDefinition.DisplayName;
            _sessionRpgPartyLevelRuntimeState.Members = members;
        }
        else
        {
            EnsureRpgPartyLevelMembers(partyDefinition, appliedProgressState, _sessionRpgPartyLevelRuntimeState);
        }

        _sessionRpgPartyLevelRuntimeState.EquipmentLoadoutReadbackSummaryText = BuildRpgEquipmentLoadoutReadbackSummaryText(partyDefinition, _sessionRpgPartyLevelRuntimeState);
        _sessionRpgPartyLevelRuntimeState.GearContributionSummaryText = BuildRpgPartyGearContributionSummaryText(_sessionRpgPartyLevelRuntimeState);
        _sessionRpgPartyLevelRuntimeState.GearAffixLiteSummaryText = BuildRpgPartyGearAffixLiteSummaryText(_sessionRpgPartyLevelRuntimeState);
        _sessionRpgPartyLevelRuntimeState.LevelBandGearLinkageSummaryText = BuildRpgPartyLevelBandLinkageSummaryText(_sessionRpgPartyLevelRuntimeState);
        RefreshRpgJobSpecializationRuntimeState(partyDefinition, appliedProgressState, _sessionRpgPartyLevelRuntimeState, null);
        _sessionRpgPartyLevelRuntimeState.JobSpecializationSummaryText = string.IsNullOrEmpty(_sessionRpgJobUnlockRuntimeState.JobSpecializationSummaryText) ? string.Empty : _sessionRpgJobUnlockRuntimeState.JobSpecializationSummaryText;
        _sessionRpgPartyLevelRuntimeState.PassiveSkillSlotSummaryText = string.IsNullOrEmpty(_sessionRpgJobUnlockRuntimeState.PassiveSkillSlotSummaryText) ? string.Empty : _sessionRpgJobUnlockRuntimeState.PassiveSkillSlotSummaryText;
        _sessionRpgPartyLevelRuntimeState.RuntimeSynergySummaryText = string.IsNullOrEmpty(_sessionRpgJobUnlockRuntimeState.RuntimeSynergySummaryText) ? string.Empty : _sessionRpgJobUnlockRuntimeState.RuntimeSynergySummaryText;
        _sessionRpgPartyLevelRuntimeState.NextRunSpecializationPreviewText = string.IsNullOrEmpty(_sessionRpgJobUnlockRuntimeState.NextRunSpecializationPreviewText) ? string.Empty : _sessionRpgJobUnlockRuntimeState.NextRunSpecializationPreviewText;
        _sessionRpgPartyLevelRuntimeState.SummaryText = BuildRpgPartyLevelSummaryText(_sessionRpgPartyLevelRuntimeState);
        return _sessionRpgPartyLevelRuntimeState;
    }

    private PrototypeRpgPartyMemberLevelRuntimeState[] BuildDefaultRpgPartyLevelMembers(PrototypeRpgPartyDefinition partyDefinition, PrototypeRpgAppliedPartyProgressState appliedProgressState)
    {
        PrototypeRpgPartyMemberDefinition[] definitions = partyDefinition != null ? (partyDefinition.Members ?? Array.Empty<PrototypeRpgPartyMemberDefinition>()) : Array.Empty<PrototypeRpgPartyMemberDefinition>();
        if (definitions.Length <= 0)
        {
            return Array.Empty<PrototypeRpgPartyMemberLevelRuntimeState>();
        }

        PrototypeRpgPartyMemberLevelRuntimeState[] members = new PrototypeRpgPartyMemberLevelRuntimeState[definitions.Length];
        for (int i = 0; i < definitions.Length; i++)
        {
            members[i] = BuildDefaultRpgPartyLevelMember(definitions[i], appliedProgressState, i);
        }

        return members;
    }

    private PrototypeRpgPartyMemberLevelRuntimeState BuildDefaultRpgPartyLevelMember(PrototypeRpgPartyMemberDefinition definition, PrototypeRpgAppliedPartyProgressState appliedProgressState, int index)
    {
        PrototypeRpgPartyMemberDefinition safeDefinition = definition ?? new PrototypeRpgPartyMemberDefinition(string.Empty, "Adventurer", "adventurer", "Adventurer", index, new PrototypeRpgStatBlock(1, 1, 0, 0), string.Empty, string.Empty, string.Empty, string.Empty, string.Empty, string.Empty, string.Empty);
        PrototypeRpgAppliedPartyMemberProgressState appliedMemberState = GetRpgAppliedPartyMemberProgressState(appliedProgressState, safeDefinition.MemberId);
        PrototypeRpgPartyMemberLevelRuntimeState memberState = new PrototypeRpgPartyMemberLevelRuntimeState();
        memberState.MemberId = string.IsNullOrEmpty(safeDefinition.MemberId) ? string.Empty : safeDefinition.MemberId;
        memberState.DisplayName = string.IsNullOrEmpty(safeDefinition.DisplayName) ? "Adventurer" : safeDefinition.DisplayName;
        memberState.GrowthTrackId = ResolveAppliedOrDefinitionId(appliedMemberState != null ? appliedMemberState.AppliedGrowthTrackId : string.Empty, safeDefinition.GrowthTrackId);
        memberState.JobId = ResolveAppliedOrDefinitionId(appliedMemberState != null ? appliedMemberState.AppliedJobId : string.Empty, safeDefinition.JobId);
        memberState.EquipmentLoadoutId = ResolveAppliedOrDefinitionId(appliedMemberState != null ? appliedMemberState.AppliedEquipmentLoadoutId : string.Empty, safeDefinition.EquipmentLoadoutId);
        memberState.SkillLoadoutId = ResolveAppliedOrDefinitionId(appliedMemberState != null ? appliedMemberState.AppliedSkillLoadoutId : string.Empty, safeDefinition.SkillLoadoutId);
        memberState.Experience.Level = 1;
        memberState.Experience.CurrentExperience = 0;
        memberState.Experience.ExperienceToNextLevel = GetRpgExperienceToNextLevel(1);
        memberState.Experience.LifetimeExperience = 0;
        memberState.Experience.SummaryText = "Lv 1 0/" + memberState.Experience.ExperienceToNextLevel;
        memberState.Modifiers = BuildRpgStatModifierBundle(safeDefinition.RoleTag, memberState.JobId, memberState.EquipmentLoadoutId, memberState.GrowthTrackId);
        ApplyRpgJobSpecializationAndPassiveBonuses(safeDefinition, appliedMemberState, memberState, memberState.Modifiers);
        memberState.DerivedStats = BuildRpgDerivedStatSummary(safeDefinition.BaseStats, safeDefinition.RoleTag, memberState);
        RefreshRpgMemberEquipmentReadback(memberState);
        memberState.LevelSummaryText = BuildRpgLevelSummaryText(memberState);
        return memberState;
    }

    private void EnsureRpgPartyLevelMembers(PrototypeRpgPartyDefinition partyDefinition, PrototypeRpgAppliedPartyProgressState appliedProgressState, PrototypeRpgPartyLevelRuntimeState partyLevelState)
    {
        if (partyDefinition == null || partyLevelState == null)
        {
            return;
        }

        PrototypeRpgPartyMemberDefinition[] definitions = partyDefinition.Members ?? Array.Empty<PrototypeRpgPartyMemberDefinition>();
        PrototypeRpgPartyMemberLevelRuntimeState[] currentMembers = partyLevelState.Members ?? Array.Empty<PrototypeRpgPartyMemberLevelRuntimeState>();
        if (currentMembers.Length != definitions.Length)
        {
            PrototypeRpgPartyMemberLevelRuntimeState[] rebuilt = new PrototypeRpgPartyMemberLevelRuntimeState[definitions.Length];
            for (int i = 0; i < definitions.Length; i++)
            {
                PrototypeRpgPartyMemberDefinition definition = definitions[i];
                PrototypeRpgPartyMemberLevelRuntimeState existing = GetRpgMemberLevelRuntimeState(partyLevelState, definition != null ? definition.MemberId : string.Empty);
                if (existing == null)
                {
                    existing = BuildDefaultRpgPartyLevelMember(definition, appliedProgressState, i);
                }

                rebuilt[i] = existing;
            }

            partyLevelState.Members = rebuilt;
            currentMembers = rebuilt;
        }
        for (int i = 0; i < definitions.Length; i++)
        {
            PrototypeRpgPartyMemberDefinition definition = definitions[i];
            PrototypeRpgPartyMemberLevelRuntimeState memberState = currentMembers[i];
            if (definition == null || memberState == null)
            {
                continue;
            }

            PrototypeRpgAppliedPartyMemberProgressState appliedMemberState = GetRpgAppliedPartyMemberProgressState(appliedProgressState, definition.MemberId);
            memberState.DisplayName = string.IsNullOrEmpty(definition.DisplayName) ? memberState.DisplayName : definition.DisplayName;
            memberState.GrowthTrackId = ResolveAppliedOrDefinitionId(appliedMemberState != null ? appliedMemberState.AppliedGrowthTrackId : string.Empty, definition.GrowthTrackId);
            memberState.JobId = ResolveAppliedOrDefinitionId(appliedMemberState != null ? appliedMemberState.AppliedJobId : string.Empty, definition.JobId);
            memberState.EquipmentLoadoutId = ResolveAppliedOrDefinitionId(appliedMemberState != null ? appliedMemberState.AppliedEquipmentLoadoutId : string.Empty, definition.EquipmentLoadoutId);
            memberState.SkillLoadoutId = ResolveAppliedOrDefinitionId(appliedMemberState != null ? appliedMemberState.AppliedSkillLoadoutId : string.Empty, definition.SkillLoadoutId);
            memberState.Experience.Level = Mathf.Max(1, memberState.Experience.Level);
            memberState.Experience.ExperienceToNextLevel = GetRpgExperienceToNextLevel(memberState.Experience.Level);
            memberState.Experience.CurrentExperience = Mathf.Clamp(memberState.Experience.CurrentExperience, 0, memberState.Experience.ExperienceToNextLevel - 1);
            memberState.Experience.SummaryText = "Lv " + memberState.Experience.Level + " " + memberState.Experience.CurrentExperience + "/" + memberState.Experience.ExperienceToNextLevel;
            memberState.Modifiers = BuildRpgStatModifierBundle(definition.RoleTag, memberState.JobId, memberState.EquipmentLoadoutId, memberState.GrowthTrackId);
            ApplyRpgJobSpecializationAndPassiveBonuses(definition, appliedMemberState, memberState, memberState.Modifiers);
            memberState.DerivedStats = BuildRpgDerivedStatSummary(definition.BaseStats, definition.RoleTag, memberState);
            RefreshRpgMemberEquipmentReadback(memberState);
            memberState.LevelSummaryText = BuildRpgLevelSummaryText(memberState);
        }
    }

    private PrototypeRpgPartyMemberLevelRuntimeState GetRpgMemberLevelRuntimeState(PrototypeRpgPartyLevelRuntimeState partyLevelState, string memberId)
    {
        if (partyLevelState == null || partyLevelState.Members == null || string.IsNullOrEmpty(memberId))
        {
            return null;
        }

        for (int i = 0; i < partyLevelState.Members.Length; i++)
        {
            PrototypeRpgPartyMemberLevelRuntimeState memberState = partyLevelState.Members[i];
            if (memberState != null && memberState.MemberId == memberId)
            {
                return memberState;
            }
        }

        return null;
    }

    private PrototypeRpgInventoryRuntimeState GetOrCreateRpgInventoryRuntimeState(PrototypeRpgPartyDefinition partyDefinition)
    {
        if (partyDefinition == null)
        {
            return _sessionRpgInventoryRuntimeState ?? new PrototypeRpgInventoryRuntimeState();
        }

        bool rebuildState = _sessionRpgInventoryRuntimeState == null ||
                            _sessionRpgInventoryRuntimeState.Consumables == null ||
                            _sessionRpgInventoryRuntimeState.PartyId != partyDefinition.PartyId;
        if (rebuildState)
        {
            _sessionRpgInventoryRuntimeState = new PrototypeRpgInventoryRuntimeState();
            _sessionRpgInventoryRuntimeState.PartyId = string.IsNullOrEmpty(partyDefinition.PartyId) ? string.Empty : partyDefinition.PartyId;
            _sessionRpgInventoryRuntimeState.DisplayName = string.IsNullOrEmpty(partyDefinition.DisplayName) ? _sessionRpgInventoryRuntimeState.PartyId : partyDefinition.DisplayName;
            _sessionRpgInventoryRuntimeState.Consumables = BuildDefaultRpgInventoryEntries(partyDefinition);
            _sessionRpgInventoryRuntimeState.CarryEntries = Array.Empty<PrototypeRpgRewardCarryEntry>();
        }

        PrototypeRpgAppliedPartyProgressState appliedState = GetOrCreateRpgAppliedPartyProgressState(partyDefinition);
        PrototypeRpgPartyLevelRuntimeState partyLevelState = GetOrCreateRpgPartyLevelRuntimeState(partyDefinition, appliedState);
        RefreshRpgEquippedLoadoutState(_sessionRpgInventoryRuntimeState, partyDefinition, partyLevelState);
        _sessionRpgInventoryRuntimeState.TotalConsumableCount = CountRpgInventoryEntries(_sessionRpgInventoryRuntimeState.Consumables);
        _sessionRpgInventoryRuntimeState.TotalCarryRewardAmount = CountRpgCarryRewardAmount(_sessionRpgInventoryRuntimeState.CarryEntries);
        RefreshRpgInventoryBattleSlotState(_sessionRpgInventoryRuntimeState);
        _sessionRpgInventoryRuntimeState.InventorySummaryText = BuildRpgInventorySummaryText(_sessionRpgInventoryRuntimeState);
        return _sessionRpgInventoryRuntimeState;
    }

    private PrototypeRpgInventoryEntry[] BuildDefaultRpgInventoryEntries(PrototypeRpgPartyDefinition partyDefinition)
    {
        List<PrototypeRpgInventoryEntry> entries = new List<PrototypeRpgInventoryEntry>();
        AddRpgInventoryEntry(entries, "consumable_field_tonic", 2, "baseline");
        AddRpgInventoryEntry(entries, "consumable_smoke_ward", 1, "baseline");
        AddRpgInventoryEntry(entries, "consumable_burst_flask", 1, "baseline");

        PrototypeRpgPartyMemberDefinition[] members = partyDefinition != null ? (partyDefinition.Members ?? Array.Empty<PrototypeRpgPartyMemberDefinition>()) : Array.Empty<PrototypeRpgPartyMemberDefinition>();
        bool hasMage = false;
        bool hasCleric = false;
        for (int i = 0; i < members.Length; i++)
        {
            PrototypeRpgPartyMemberDefinition member = members[i];
            if (member == null)
            {
                continue;
            }

            if (member.RoleTag == "mage")
            {
                hasMage = true;
            }
            else if (member.RoleTag == "cleric")
            {
                hasCleric = true;
            }
        }

        if (hasMage)
        {
            AddRpgInventoryEntry(entries, "consumable_arcane_tonic", 1, "baseline");
        }

        if (hasCleric)
        {
            AddRpgInventoryEntry(entries, "consumable_safeguard_kit", 1, "baseline");
        }

        return entries.ToArray();
    }

    private void AddRpgInventoryEntry(List<PrototypeRpgInventoryEntry> entries, string itemId, int quantity, string sourceKey)
    {
        if (entries == null || quantity <= 0)
        {
            return;
        }

        PrototypeRpgConsumableDefinition definition = PrototypeRpgConsumableCatalog.GetDefinition(itemId);
        PrototypeRpgInventoryEntry entry = new PrototypeRpgInventoryEntry();
        entry.ItemId = definition != null ? definition.ItemId : itemId;
        entry.DisplayName = definition != null ? definition.DisplayName : itemId;
        entry.Quantity = quantity;
        entry.SourceKey = string.IsNullOrEmpty(sourceKey) ? string.Empty : sourceKey;
        entry.SummaryText = entry.DisplayName + " x" + quantity;
        entries.Add(entry);
    }

    private void RefreshRpgEquippedLoadoutState(PrototypeRpgInventoryRuntimeState inventoryState, PrototypeRpgPartyDefinition partyDefinition, PrototypeRpgPartyLevelRuntimeState partyLevelState)
    {
        if (inventoryState == null)
        {
            return;
        }

        inventoryState.EquippedLoadouts = BuildRpgEquippedLoadoutEntries(partyDefinition, partyLevelState);
        inventoryState.TotalEquippedLoadoutCount = inventoryState.EquippedLoadouts != null ? inventoryState.EquippedLoadouts.Length : 0;
        inventoryState.EquipmentSummaryText = BuildRpgEquipmentInventorySummaryText(inventoryState);
    }

    private PrototypeRpgEquippedLoadoutEntry[] BuildRpgEquippedLoadoutEntries(PrototypeRpgPartyDefinition partyDefinition, PrototypeRpgPartyLevelRuntimeState partyLevelState)
    {
        PrototypeRpgPartyMemberDefinition[] definitions = partyDefinition != null ? (partyDefinition.Members ?? Array.Empty<PrototypeRpgPartyMemberDefinition>()) : Array.Empty<PrototypeRpgPartyMemberDefinition>();
        List<PrototypeRpgEquippedLoadoutEntry> entries = new List<PrototypeRpgEquippedLoadoutEntry>();
        for (int i = 0; i < definitions.Length; i++)
        {
            PrototypeRpgEquippedLoadoutEntry entry = BuildRpgEquippedLoadoutEntry(definitions[i], GetRpgMemberLevelRuntimeState(partyLevelState, definitions[i] != null ? definitions[i].MemberId : string.Empty));
            if (entry != null && !string.IsNullOrEmpty(entry.LoadoutId))
            {
                entries.Add(entry);
            }
        }

        return entries.Count > 0 ? entries.ToArray() : Array.Empty<PrototypeRpgEquippedLoadoutEntry>();
    }

    private PrototypeRpgEquippedLoadoutEntry BuildRpgEquippedLoadoutEntry(PrototypeRpgPartyMemberDefinition definition, PrototypeRpgPartyMemberLevelRuntimeState levelState)
    {
        if (definition == null && levelState == null)
        {
            return null;
        }

        string memberId = levelState != null && !string.IsNullOrEmpty(levelState.MemberId) ? levelState.MemberId : (definition != null ? definition.MemberId : string.Empty);
        string memberDisplayName = levelState != null && !string.IsNullOrEmpty(levelState.DisplayName) ? levelState.DisplayName : (definition != null && !string.IsNullOrEmpty(definition.DisplayName) ? definition.DisplayName : "Adventurer");
        string roleTag = definition != null ? definition.RoleTag : string.Empty;
        string loadoutId = levelState != null && !string.IsNullOrEmpty(levelState.EquipmentLoadoutId) ? levelState.EquipmentLoadoutId : (definition != null ? definition.EquipmentLoadoutId : string.Empty);
        PrototypeRpgEquipmentLoadoutDefinition loadoutDefinition = PrototypeRpgEquipmentCatalog.ResolveDefinition(loadoutId, roleTag);
        if (loadoutDefinition == null)
        {
            return null;
        }

        PrototypeRpgEquipmentDefinition primaryItem = PrototypeRpgEquipmentCatalog.GetPrimaryItemDefinition(loadoutId, roleTag);
        PrototypeRpgEquippedLoadoutEntry entry = new PrototypeRpgEquippedLoadoutEntry();
        entry.MemberId = string.IsNullOrEmpty(memberId) ? string.Empty : memberId;
        entry.MemberDisplayName = string.IsNullOrEmpty(memberDisplayName) ? "Adventurer" : memberDisplayName;
        entry.LoadoutId = loadoutDefinition.LoadoutId;
        entry.DisplayName = loadoutDefinition.DisplayName;
        entry.SlotKey = primaryItem != null ? primaryItem.SlotKey : string.Empty;
        entry.MaxHpDelta = loadoutDefinition.TotalMaxHpDelta;
        entry.AttackDelta = loadoutDefinition.TotalAttackDelta;
        entry.DefenseDelta = loadoutDefinition.TotalDefenseDelta;
        entry.SpeedDelta = loadoutDefinition.TotalSpeedDelta;
        entry.PassiveHintText = string.IsNullOrEmpty(loadoutDefinition.PassiveHintText) ? string.Empty : loadoutDefinition.PassiveHintText;
        entry.BattleLabelHint = string.IsNullOrEmpty(loadoutDefinition.BattleLabelHint) ? string.Empty : loadoutDefinition.BattleLabelHint;
        entry.SummaryText = entry.MemberDisplayName + ": " + loadoutDefinition.DisplayName;
        entry.GearContributionSummaryText = BuildRpgEquipmentContributionText(loadoutDefinition);
        return entry;
    }

    private PrototypeRpgEquippedLoadoutEntry GetRpgEquippedLoadoutEntry(PrototypeRpgInventoryRuntimeState inventoryState, string memberId)
    {
        if (inventoryState == null || inventoryState.EquippedLoadouts == null || string.IsNullOrEmpty(memberId))
        {
            return null;
        }

        for (int i = 0; i < inventoryState.EquippedLoadouts.Length; i++)
        {
            PrototypeRpgEquippedLoadoutEntry entry = inventoryState.EquippedLoadouts[i];
            if (entry != null && entry.MemberId == memberId)
            {
                return entry;
            }
        }

        return null;
    }

    private string BuildRpgEquipmentContributionText(PrototypeRpgEquipmentLoadoutDefinition loadoutDefinition)
    {
        if (loadoutDefinition == null)
        {
            return string.Empty;
        }

        string statsText = "HP+" + Mathf.Max(0, loadoutDefinition.TotalMaxHpDelta) + " ATK+" + Mathf.Max(0, loadoutDefinition.TotalAttackDelta) + " DEF+" + Mathf.Max(0, loadoutDefinition.TotalDefenseDelta) + " SPD+" + Mathf.Max(0, loadoutDefinition.TotalSpeedDelta);
        string passiveText = string.IsNullOrEmpty(loadoutDefinition.PassiveHintText) ? string.Empty : " | " + loadoutDefinition.PassiveHintText;
        return loadoutDefinition.DisplayName + " | " + statsText + passiveText;
    }

    private string BuildRpgEquipmentInventorySummaryText(PrototypeRpgInventoryRuntimeState inventoryState)
    {
        PrototypeRpgEquippedLoadoutEntry[] entries = inventoryState != null ? (inventoryState.EquippedLoadouts ?? Array.Empty<PrototypeRpgEquippedLoadoutEntry>()) : Array.Empty<PrototypeRpgEquippedLoadoutEntry>();
        List<string> parts = new List<string>();
        for (int i = 0; i < entries.Length; i++)
        {
            PrototypeRpgEquippedLoadoutEntry entry = entries[i];
            if (entry != null && !string.IsNullOrEmpty(entry.DisplayName))
            {
                parts.Add(entry.MemberDisplayName + ": " + entry.DisplayName);
            }
        }

        return parts.Count <= 0 ? string.Empty : "Gear " + string.Join(" | ", parts.ToArray());
    }

    private string BuildRpgEquipmentLoadoutReadbackSummaryText(PrototypeRpgPartyDefinition partyDefinition, PrototypeRpgPartyLevelRuntimeState partyLevelState)
    {
        PrototypeRpgPartyMemberDefinition[] definitions = partyDefinition != null ? (partyDefinition.Members ?? Array.Empty<PrototypeRpgPartyMemberDefinition>()) : Array.Empty<PrototypeRpgPartyMemberDefinition>();
        List<string> parts = new List<string>();
        for (int i = 0; i < definitions.Length; i++)
        {
            PrototypeRpgPartyMemberDefinition definition = definitions[i];
            if (definition == null)
            {
                continue;
            }

            PrototypeRpgPartyMemberLevelRuntimeState memberState = GetRpgMemberLevelRuntimeState(partyLevelState, definition.MemberId);
            PrototypeRpgEquipmentLoadoutDefinition loadoutDefinition = PrototypeRpgEquipmentCatalog.ResolveDefinition(memberState != null ? memberState.EquipmentLoadoutId : definition.EquipmentLoadoutId, definition.RoleTag);
            if (loadoutDefinition != null)
            {
                parts.Add(definition.DisplayName + " " + loadoutDefinition.DisplayName);
            }
        }

        return parts.Count <= 0 ? string.Empty : "Gear runtime: " + string.Join(" | ", parts.ToArray());
    }

    private string BuildRpgPartyGearContributionSummaryText(PrototypeRpgPartyLevelRuntimeState partyLevelState)
    {
        PrototypeRpgPartyMemberLevelRuntimeState[] members = partyLevelState != null ? (partyLevelState.Members ?? Array.Empty<PrototypeRpgPartyMemberLevelRuntimeState>()) : Array.Empty<PrototypeRpgPartyMemberLevelRuntimeState>();
        List<string> parts = new List<string>();
        for (int i = 0; i < members.Length; i++)
        {
            PrototypeRpgPartyMemberLevelRuntimeState memberState = members[i];
            if (memberState != null && !string.IsNullOrEmpty(memberState.GearContributionSummaryText))
            {
                parts.Add(memberState.DisplayName + " " + memberState.GearContributionSummaryText);
            }
        }

        return parts.Count <= 0 ? string.Empty : "Gear contribution: " + string.Join(" | ", parts.ToArray());
    }

    private string BuildRpgPartyGearAffixLiteSummaryText(PrototypeRpgPartyLevelRuntimeState partyLevelState)
    {
        PrototypeRpgPartyMemberLevelRuntimeState[] members = partyLevelState != null ? (partyLevelState.Members ?? Array.Empty<PrototypeRpgPartyMemberLevelRuntimeState>()) : Array.Empty<PrototypeRpgPartyMemberLevelRuntimeState>();
        List<string> parts = new List<string>();
        for (int i = 0; i < members.Length; i++)
        {
            PrototypeRpgPartyMemberLevelRuntimeState memberState = members[i];
            if (memberState != null && !string.IsNullOrEmpty(memberState.GearAffixLiteSummaryText))
            {
                parts.Add(memberState.DisplayName + " " + memberState.GearAffixLiteSummaryText);
            }
        }

        return parts.Count <= 0 ? string.Empty : "Affix-lite: " + string.Join(" | ", parts.ToArray());
    }

    private string BuildRpgPartyLevelBandLinkageSummaryText(PrototypeRpgPartyLevelRuntimeState partyLevelState)
    {
        PrototypeRpgPartyMemberLevelRuntimeState[] members = partyLevelState != null ? (partyLevelState.Members ?? Array.Empty<PrototypeRpgPartyMemberLevelRuntimeState>()) : Array.Empty<PrototypeRpgPartyMemberLevelRuntimeState>();
        List<string> parts = new List<string>();
        for (int i = 0; i < members.Length; i++)
        {
            PrototypeRpgPartyMemberLevelRuntimeState memberState = members[i];
            if (memberState != null && !string.IsNullOrEmpty(memberState.LevelBandSummaryText))
            {
                parts.Add(memberState.DisplayName + " " + memberState.LevelBandSummaryText);
            }
        }

        return parts.Count <= 0 ? string.Empty : "Band linkage: " + string.Join(" | ", parts.ToArray());
    }

    private string BuildRpgPartyJobSpecializationSummaryText(PrototypeRpgPartyLevelRuntimeState partyLevelState)
    {
        PrototypeRpgPartyMemberLevelRuntimeState[] members = partyLevelState != null ? (partyLevelState.Members ?? Array.Empty<PrototypeRpgPartyMemberLevelRuntimeState>()) : Array.Empty<PrototypeRpgPartyMemberLevelRuntimeState>();
        List<string> parts = new List<string>();
        for (int i = 0; i < members.Length; i++)
        {
            PrototypeRpgPartyMemberLevelRuntimeState memberState = members[i];
            if (memberState != null && !string.IsNullOrEmpty(memberState.JobSpecializationSummaryText))
            {
                parts.Add(memberState.JobSpecializationSummaryText);
            }
        }

        return parts.Count <= 0 ? string.Empty : "Specialization: " + string.Join(" | ", parts.ToArray());
    }

    private string BuildRpgPartyPassiveSkillSlotSummaryText(PrototypeRpgPartyLevelRuntimeState partyLevelState)
    {
        PrototypeRpgPartyMemberLevelRuntimeState[] members = partyLevelState != null ? (partyLevelState.Members ?? Array.Empty<PrototypeRpgPartyMemberLevelRuntimeState>()) : Array.Empty<PrototypeRpgPartyMemberLevelRuntimeState>();
        List<string> parts = new List<string>();
        for (int i = 0; i < members.Length; i++)
        {
            PrototypeRpgPartyMemberLevelRuntimeState memberState = members[i];
            if (memberState != null && !string.IsNullOrEmpty(memberState.PassiveSkillSlotSummaryText))
            {
                parts.Add(memberState.PassiveSkillSlotSummaryText);
            }
        }

        return parts.Count <= 0 ? string.Empty : "Passive slot: " + string.Join(" | ", parts.ToArray());
    }

    private string BuildRpgPartyRuntimeSynergySummaryText(PrototypeRpgPartyLevelRuntimeState partyLevelState)
    {
        PrototypeRpgPartyMemberLevelRuntimeState[] members = partyLevelState != null ? (partyLevelState.Members ?? Array.Empty<PrototypeRpgPartyMemberLevelRuntimeState>()) : Array.Empty<PrototypeRpgPartyMemberLevelRuntimeState>();
        List<string> parts = new List<string>();
        for (int i = 0; i < members.Length; i++)
        {
            PrototypeRpgPartyMemberLevelRuntimeState memberState = members[i];
            if (memberState != null && !string.IsNullOrEmpty(memberState.RuntimeSynergySummaryText))
            {
                parts.Add(memberState.RuntimeSynergySummaryText);
            }
        }

        return parts.Count <= 0 ? string.Empty : "Runtime synergy: " + string.Join(" | ", parts.ToArray());
    }

    private string BuildRpgPartyNextRunSpecializationPreviewText(PrototypeRpgPartyLevelRuntimeState partyLevelState)
    {
        PrototypeRpgPartyMemberLevelRuntimeState[] members = partyLevelState != null ? (partyLevelState.Members ?? Array.Empty<PrototypeRpgPartyMemberLevelRuntimeState>()) : Array.Empty<PrototypeRpgPartyMemberLevelRuntimeState>();
        List<string> parts = new List<string>();
        for (int i = 0; i < members.Length; i++)
        {
            PrototypeRpgPartyMemberLevelRuntimeState memberState = members[i];
            if (memberState != null && !string.IsNullOrEmpty(memberState.NextRunSpecializationPreviewText))
            {
                parts.Add(memberState.NextRunSpecializationPreviewText);
            }
        }

        return parts.Count <= 0 ? string.Empty : "Next specialization: " + string.Join(" | ", parts.ToArray());
    }

    private void RefreshRpgMemberEquipmentReadback(PrototypeRpgPartyMemberLevelRuntimeState memberState)
    {
        if (memberState == null)
        {
            return;
        }

        PrototypeRpgStatModifierBundle modifiers = memberState.Modifiers ?? new PrototypeRpgStatModifierBundle();
        PrototypeRpgDerivedStatSummary derivedStats = memberState.DerivedStats ?? new PrototypeRpgDerivedStatSummary();
        memberState.EquipmentSummaryText = !string.IsNullOrEmpty(derivedStats.EquipmentSummaryText) ? derivedStats.EquipmentSummaryText : modifiers.EquipmentSummaryText;
        memberState.GearContributionSummaryText = !string.IsNullOrEmpty(derivedStats.GearContributionSummaryText) ? derivedStats.GearContributionSummaryText : modifiers.GearContributionSummaryText;
        memberState.GearAffixLiteSummaryText = !string.IsNullOrEmpty(derivedStats.GearAffixLiteSummaryText) ? derivedStats.GearAffixLiteSummaryText : modifiers.GearAffixLiteSummaryText;
        memberState.LevelBandSummaryText = !string.IsNullOrEmpty(derivedStats.LevelBandSummaryText) ? derivedStats.LevelBandSummaryText : modifiers.LevelBandSummaryText;
        memberState.PassiveHintText = !string.IsNullOrEmpty(derivedStats.PassiveHintText) ? derivedStats.PassiveHintText : modifiers.PassiveHintText;
        memberState.BattleLabelHint = !string.IsNullOrEmpty(derivedStats.BattleLabelHint) ? derivedStats.BattleLabelHint : modifiers.BattleLabelHint;
        memberState.JobSpecializationKey = !string.IsNullOrEmpty(derivedStats.JobSpecializationKey) ? derivedStats.JobSpecializationKey : modifiers.JobSpecializationKey;
        memberState.JobSpecializationSummaryText = !string.IsNullOrEmpty(derivedStats.JobSpecializationSummaryText) ? derivedStats.JobSpecializationSummaryText : modifiers.JobSpecializationSummaryText;
        memberState.PassiveSkillId = !string.IsNullOrEmpty(derivedStats.PassiveSkillId) ? derivedStats.PassiveSkillId : modifiers.PassiveSkillId;
        memberState.PassiveSkillSlotSummaryText = !string.IsNullOrEmpty(derivedStats.PassiveSkillSlotSummaryText) ? derivedStats.PassiveSkillSlotSummaryText : modifiers.PassiveSkillSlotSummaryText;
        memberState.RuntimeSynergySummaryText = !string.IsNullOrEmpty(derivedStats.RuntimeSynergySummaryText) ? derivedStats.RuntimeSynergySummaryText : modifiers.RuntimeSynergySummaryText;
        memberState.RuntimeSkillPowerBonus = Mathf.Max(0, modifiers.BonusSkillPower);
    }

    private void ApplyRpgBackboneSessionResult(PrototypeRpgRunResultSnapshot runResultSnapshot)
    {
        PrototypeRpgPartyDefinition partyDefinition = _activeDungeonParty != null ? _activeDungeonParty.PartyDefinition : null;
        if (partyDefinition == null || runResultSnapshot == null)
        {
            return;
        }

        PrototypeRpgAppliedPartyProgressState appliedState = GetOrCreateRpgAppliedPartyProgressState(partyDefinition);
        PrototypeRpgPartyLevelRuntimeState partyLevelState = GetOrCreateRpgPartyLevelRuntimeState(partyDefinition, appliedState);
        PrototypeRpgInventoryRuntimeState inventoryState = GetOrCreateRpgInventoryRuntimeState(partyDefinition);
        PrototypeRpgPartyMemberOutcomeSnapshot[] outcomes = runResultSnapshot.PartyOutcome != null
            ? (runResultSnapshot.PartyOutcome.Members ?? Array.Empty<PrototypeRpgPartyMemberOutcomeSnapshot>())
            : Array.Empty<PrototypeRpgPartyMemberOutcomeSnapshot>();
        int baseExperienceReward = BuildRpgRunExperienceReward(runResultSnapshot);
        string runIdentity = BuildRpgOfferGenerationRunIdentity(runResultSnapshot);
        for (int i = 0; i < outcomes.Length; i++)
        {
            PrototypeRpgPartyMemberOutcomeSnapshot outcome = outcomes[i];
            if (outcome == null)
            {
                continue;
            }

            PrototypeRpgPartyMemberLevelRuntimeState memberState = GetRpgMemberLevelRuntimeState(partyLevelState, outcome.MemberId);
            if (memberState == null)
            {
                continue;
            }

            int memberExperienceReward = baseExperienceReward + (outcome.Survived ? 1 : 0);
            if (outcome.KnockedOut)
            {
                memberExperienceReward = Mathf.Max(1, memberExperienceReward - 1);
            }

            PrototypeRpgPartyMemberDefinition memberDefinition = GetRpgPartyDefinitionMemberById(partyDefinition, outcome.MemberId);
            CommitRpgRunExperience(memberState, memberExperienceReward, memberDefinition, runIdentity);
        }

        int pendingLevelUpCount = 0;
        int appliedMemberCount = 0;
        PrototypeRpgPartyMemberLevelRuntimeState[] levelMembers = partyLevelState.Members ?? Array.Empty<PrototypeRpgPartyMemberLevelRuntimeState>();
        for (int i = 0; i < levelMembers.Length; i++)
        {
            PrototypeRpgPartyMemberLevelRuntimeState member = levelMembers[i];
            if (member == null)
            {
                continue;
            }

            member.LastAppliedRunIdentity = runIdentity;
            pendingLevelUpCount += member.LevelUpPreview != null ? Mathf.Max(0, member.LevelUpPreview.PendingLevelUps) : 0;
            if (!string.IsNullOrEmpty(member.ActualLevelApplySummaryText))
            {
                appliedMemberCount += 1;
            }
        }

        partyLevelState.LastGrantedExperience = baseExperienceReward;
        partyLevelState.LastRunIdentity = runIdentity;
        partyLevelState.AppliedLastRunIdentity = runIdentity;
        partyLevelState.PendingLevelUpCount = pendingLevelUpCount;
        partyLevelState.AppliedMemberCount = appliedMemberCount;
        partyLevelState.ExperienceGainSummaryText = BuildRpgExperienceGainSummaryText(partyLevelState);
        partyLevelState.LevelUpPreviewSummaryText = BuildRpgLevelUpPreviewSummaryText(partyLevelState);
        partyLevelState.LevelUpApplyReadySummaryText = BuildRpgLevelUpApplyReadySummaryText(partyLevelState);
        partyLevelState.ActualLevelApplySummaryText = BuildRpgActualLevelApplySummaryText(partyLevelState);
        partyLevelState.DerivedStatHydrateSummaryText = BuildRpgDerivedStatHydrateSummaryText(partyLevelState);
        partyLevelState.NextRunStatProjectionSummaryText = BuildRpgNextRunStatProjectionSummaryText(partyLevelState);
        RefreshRpgJobSpecializationRuntimeState(partyDefinition, appliedState, partyLevelState, runResultSnapshot);
        partyLevelState.JobSpecializationSummaryText = BuildRpgPartyJobSpecializationSummaryText(partyLevelState);
        partyLevelState.PassiveSkillSlotSummaryText = BuildRpgPartyPassiveSkillSlotSummaryText(partyLevelState);
        partyLevelState.RuntimeSynergySummaryText = BuildRpgPartyRuntimeSynergySummaryText(partyLevelState);
        partyLevelState.NextRunSpecializationPreviewText = BuildRpgPartyNextRunSpecializationPreviewText(partyLevelState);
        partyLevelState.SummaryText = BuildRpgPartyLevelSummaryText(partyLevelState);

        int returnedLootAmount = runResultSnapshot.LootOutcome != null ? Mathf.Max(0, runResultSnapshot.LootOutcome.TotalLootGained) : 0;
        int lostPendingAmount = runResultSnapshot.LootOutcome != null ? Mathf.Max(0, runResultSnapshot.LootOutcome.PendingBonusRewardLostAmount) : 0;
        if (returnedLootAmount > 0 || lostPendingAmount > 0)
        {
            List<PrototypeRpgRewardCarryEntry> carryEntries = new List<PrototypeRpgRewardCarryEntry>(inventoryState.CarryEntries ?? Array.Empty<PrototypeRpgRewardCarryEntry>());
            PrototypeRpgRewardCarryEntry carryEntry = new PrototypeRpgRewardCarryEntry();
            carryEntry.ResourceId = DungeonRewardResourceId;
            carryEntry.ResourceLabel = "Mana Shard Carry";
            carryEntry.Amount = returnedLootAmount;
            carryEntry.SourceKey = string.IsNullOrEmpty(runResultSnapshot.ResultStateKey) ? string.Empty : runResultSnapshot.ResultStateKey;
            carryEntry.RunIdentity = runIdentity;
            carryEntry.SummaryText = returnedLootAmount > 0
                ? "Carry " + BuildLootAmountText(returnedLootAmount) + " from " + carryEntry.SourceKey + "."
                : "No carry returned; pending bonus " + BuildLootAmountText(lostPendingAmount) + " was lost.";
            carryEntries.Insert(0, carryEntry);
            while (carryEntries.Count > RpgBackboneCarryEntryLimit)
            {
                carryEntries.RemoveAt(carryEntries.Count - 1);
            }

            inventoryState.CarryEntries = carryEntries.ToArray();
        }

        inventoryState.LastRunIdentity = runIdentity;
        ApplyRpgConsumableCarryoverPolicy(inventoryState, runResultSnapshot);
        RefreshRpgEquippedLoadoutState(inventoryState, partyDefinition, partyLevelState);
        partyLevelState.EquipmentLoadoutReadbackSummaryText = BuildRpgEquipmentLoadoutReadbackSummaryText(partyDefinition, partyLevelState);
        partyLevelState.GearContributionSummaryText = BuildRpgPartyGearContributionSummaryText(partyLevelState);
        inventoryState.InventorySummaryText = BuildRpgInventorySummaryText(inventoryState);
    }
    private int BuildRpgRunExperienceReward(PrototypeRpgRunResultSnapshot runResultSnapshot)
    {
        if (runResultSnapshot == null)
        {
            return 1;
        }

        int reward = runResultSnapshot.ResultStateKey == PrototypeBattleOutcomeKeys.RunClear
            ? 4
            : runResultSnapshot.ResultStateKey == PrototypeBattleOutcomeKeys.RunRetreat
                ? 2
                : 1;
        reward += Mathf.Min(3, runResultSnapshot.EncounterOutcome != null ? Mathf.Max(0, runResultSnapshot.EncounterOutcome.ClearedEncounterCount) : 0);
        if (runResultSnapshot.EliteOutcome != null && runResultSnapshot.EliteOutcome.IsEliteDefeated)
        {
            reward += 2;
        }

        return Mathf.Max(1, reward);
    }

    private void CommitRpgRunExperience(PrototypeRpgPartyMemberLevelRuntimeState memberState, int experienceReward, PrototypeRpgPartyMemberDefinition memberDefinition, string runIdentity)
    {
        if (memberState == null)
        {
            return;
        }

        PrototypeRpgPartyMemberDefinition safeDefinition = memberDefinition ?? new PrototypeRpgPartyMemberDefinition(string.Empty, memberState.DisplayName, string.Empty, string.Empty, 0, new PrototypeRpgStatBlock(1, 1, 0, 0), string.Empty, string.Empty, string.Empty, string.Empty, string.Empty, string.Empty, string.Empty);
        PrototypeRpgStatBlock memberBaseStats = safeDefinition.BaseStats ?? new PrototypeRpgStatBlock(1, 1, 0, 0);
        string memberRoleTag = safeDefinition.RoleTag;
        PrototypeRpgDerivedStatSummary previousDerivedStats = CopyRpgDerivedStatSummary(memberState.DerivedStats);
        int previousLevel = memberState.Experience != null ? Mathf.Max(1, memberState.Experience.Level) : 1;
        ApplyRpgMemberExperienceGrant(memberState, experienceReward, runIdentity);
        ApplyPendingRpgLevelUps(memberState, memberBaseStats, memberRoleTag, previousLevel, previousDerivedStats, runIdentity);
    }

    private void ApplyPendingRpgLevelUps(PrototypeRpgPartyMemberLevelRuntimeState memberState, PrototypeRpgStatBlock memberBaseStats, string memberRoleTag, int previousLevel, PrototypeRpgDerivedStatSummary previousDerivedStats, string runIdentity)
    {
        if (memberState == null)
        {
            return;
        }

        memberState.Modifiers = BuildRpgStatModifierBundle(memberRoleTag, memberState.JobId, memberState.EquipmentLoadoutId, memberState.GrowthTrackId);
        PrototypeRpgPartyDefinition partyDefinition = _activeDungeonParty != null ? _activeDungeonParty.PartyDefinition : null;
        PrototypeRpgPartyMemberDefinition appliedDefinition = GetRpgPartyDefinitionMemberById(partyDefinition, memberState.MemberId);
        PrototypeRpgAppliedPartyMemberProgressState appliedMemberState = GetRpgAppliedPartyMemberProgressState(GetOrCreateRpgAppliedPartyProgressState(partyDefinition), memberState.MemberId);
        ApplyRpgJobSpecializationAndPassiveBonuses(appliedDefinition, appliedMemberState, memberState, memberState.Modifiers);
        memberState.DerivedStats = BuildRpgDerivedStatSummary(memberBaseStats, memberRoleTag, memberState);
        RefreshRpgMemberEquipmentReadback(memberState);
        memberState.LevelSummaryText = BuildRpgLevelSummaryText(memberState);
        memberState.LastGrantedExperience = memberState.Experience != null ? Mathf.Max(0, memberState.Experience.GainedExperienceThisRun) : 0;
        memberState.AppliedLevelThisSession = memberState.Experience != null ? Mathf.Max(1, memberState.Experience.Level) : Mathf.Max(1, previousLevel);
        memberState.LastAppliedRunIdentity = string.IsNullOrEmpty(runIdentity) ? string.Empty : runIdentity;

        if (memberState.LevelUpPreview == null)
        {
            memberState.LevelUpPreview = new PrototypeRpgLevelUpPreview();
        }

        int appliedLevelDelta = Mathf.Max(0, memberState.AppliedLevelThisSession - previousLevel);
        memberState.LevelUpPreview.MemberId = string.IsNullOrEmpty(memberState.MemberId) ? string.Empty : memberState.MemberId;
        memberState.LevelUpPreview.DisplayName = string.IsNullOrEmpty(memberState.DisplayName) ? "Adventurer" : memberState.DisplayName;
        memberState.LevelUpPreview.PendingLevelUps = appliedLevelDelta;
        memberState.LevelUpPreview.ProjectedMaxHpDelta = Mathf.Max(0, memberState.DerivedStats.MaxHp - (previousDerivedStats != null ? previousDerivedStats.MaxHp : memberState.DerivedStats.MaxHp));
        memberState.LevelUpPreview.ProjectedAttackDelta = Mathf.Max(0, memberState.DerivedStats.Attack - (previousDerivedStats != null ? previousDerivedStats.Attack : memberState.DerivedStats.Attack));
        memberState.LevelUpPreview.ProjectedDefenseDelta = Mathf.Max(0, memberState.DerivedStats.Defense - (previousDerivedStats != null ? previousDerivedStats.Defense : memberState.DerivedStats.Defense));
        memberState.LevelUpPreview.ProjectedSpeedDelta = Mathf.Max(0, memberState.DerivedStats.Speed - (previousDerivedStats != null ? previousDerivedStats.Speed : memberState.DerivedStats.Speed));
        memberState.LevelUpPreview.DerivedStatSummaryText = memberState.DerivedStats != null ? memberState.DerivedStats.SummaryText : string.Empty;
        memberState.LevelUpPreview.NextRunStatProjectionSummaryText = appliedLevelDelta > 0
            ? memberState.DisplayName + " next run hydrates at HP " + memberState.DerivedStats.MaxHp + " ATK " + memberState.DerivedStats.Attack + " DEF " + memberState.DerivedStats.Defense + " SPD " + memberState.DerivedStats.Speed + "."
            : memberState.DisplayName + " next run keeps HP " + memberState.DerivedStats.MaxHp + " ATK " + memberState.DerivedStats.Attack + " DEF " + memberState.DerivedStats.Defense + " SPD " + memberState.DerivedStats.Speed + ".";
        memberState.LevelUpPreview.ApplyReadySummaryText = appliedLevelDelta > 0
            ? memberState.DisplayName + " now runs at Lv " + memberState.AppliedLevelThisSession + "; next run hydrates the upgraded stats."
            : string.Empty;
        memberState.ActualLevelApplySummaryText = appliedLevelDelta > 0
            ? memberState.DisplayName + " applied Lv " + previousLevel + " -> " + memberState.AppliedLevelThisSession + " this session."
            : memberState.DisplayName + " keeps Lv " + memberState.AppliedLevelThisSession + " this session.";
        memberState.DerivedStatHydrateSummaryText = memberState.DisplayName + " hydrates with HP " + memberState.DerivedStats.MaxHp + " ATK " + memberState.DerivedStats.Attack + " DEF " + memberState.DerivedStats.Defense + " SPD " + memberState.DerivedStats.Speed + (string.IsNullOrEmpty(memberState.GearContributionSummaryText) ? "." : " | " + memberState.GearContributionSummaryText + ".");
        memberState.LevelUpPreview.ActualApplySummaryText = memberState.ActualLevelApplySummaryText;
        memberState.LevelUpPreview.SummaryText = appliedLevelDelta > 0
            ? memberState.DisplayName + " Lv " + previousLevel + " -> " + memberState.AppliedLevelThisSession + " | " + memberState.LevelUpPreview.NextRunStatProjectionSummaryText
            : memberState.DisplayName + " +" + memberState.LevelUpPreview.GainedExperienceThisRun + " EXP | " + memberState.DerivedStatHydrateSummaryText;
    }
    private void ApplyRpgMemberExperienceGrant(PrototypeRpgPartyMemberLevelRuntimeState memberState, int experienceReward, string runIdentity)
    {
        if (memberState == null)
        {
            return;
        }

        if (memberState.Experience == null)
        {
            memberState.Experience = new PrototypeRpgExperienceProgress();
        }

        if (memberState.LevelUpPreview == null)
        {
            memberState.LevelUpPreview = new PrototypeRpgLevelUpPreview();
        }

        int safeExperienceReward = Mathf.Max(0, experienceReward);
        int previousLevel = Mathf.Max(1, memberState.Experience.Level);
        memberState.Experience.PreviousLevel = previousLevel;
        memberState.Experience.GainedExperienceThisRun = safeExperienceReward;
        memberState.Experience.LastGrantedExperience = safeExperienceReward;
        memberState.Experience.LeveledUpThisRun = false;
        memberState.Experience.LifetimeExperience += safeExperienceReward;
        memberState.Experience.CurrentExperience += safeExperienceReward;
        while (memberState.Experience.CurrentExperience >= memberState.Experience.ExperienceToNextLevel)
        {
            memberState.Experience.CurrentExperience -= memberState.Experience.ExperienceToNextLevel;
            memberState.Experience.Level += 1;
            memberState.Experience.ExperienceToNextLevel = GetRpgExperienceToNextLevel(memberState.Experience.Level);
        }

        memberState.Experience.Level = Mathf.Max(1, memberState.Experience.Level);
        memberState.Experience.ExperienceToNextLevel = GetRpgExperienceToNextLevel(memberState.Experience.Level);
        memberState.Experience.AppliedLevelThisSession = memberState.Experience.Level;
        memberState.Experience.LastAppliedRunIdentity = string.IsNullOrEmpty(runIdentity) ? string.Empty : runIdentity;
        memberState.Experience.LeveledUpThisRun = memberState.Experience.Level > previousLevel;
        memberState.Experience.SummaryText = "Lv " + memberState.Experience.Level + " " + memberState.Experience.CurrentExperience + "/" + memberState.Experience.ExperienceToNextLevel;
        memberState.LevelSummaryText = BuildRpgLevelSummaryText(memberState);

        memberState.LevelUpPreview.MemberId = string.IsNullOrEmpty(memberState.MemberId) ? string.Empty : memberState.MemberId;
        memberState.LevelUpPreview.DisplayName = string.IsNullOrEmpty(memberState.DisplayName) ? "Adventurer" : memberState.DisplayName;
        memberState.LevelUpPreview.PreviousLevel = previousLevel;
        memberState.LevelUpPreview.NewLevel = memberState.Experience.Level;
        memberState.LevelUpPreview.GainedExperienceThisRun = safeExperienceReward;
        memberState.LevelUpPreview.LeveledUpThisRun = memberState.Experience.LeveledUpThisRun;
        memberState.LevelUpPreview.SummaryText = memberState.Experience.LeveledUpThisRun
            ? memberState.DisplayName + " Lv " + previousLevel + " -> " + memberState.Experience.Level + "."
            : memberState.DisplayName + " +" + safeExperienceReward + " EXP.";
    }
    private int GetRpgExperienceToNextLevel(int level)
    {
        return 6 + (Mathf.Max(1, level) - 1) * 4;
    }

    private PrototypeRpgStatModifierBundle BuildRpgStatModifierBundle(string roleTag, string jobId, string equipmentLoadoutId, string growthTrackId)
    {
        PrototypeRpgStatModifierBundle bundle = new PrototypeRpgStatModifierBundle();
        string normalizedRoleTag = string.IsNullOrWhiteSpace(roleTag) ? string.Empty : roleTag.Trim().ToLowerInvariant();
        string normalizedGrowth = string.IsNullOrWhiteSpace(growthTrackId) ? string.Empty : growthTrackId.Trim().ToLowerInvariant();
        string normalizedJob = string.IsNullOrWhiteSpace(jobId) ? string.Empty : jobId.Trim().ToLowerInvariant();
        PrototypeRpgEquipmentLoadoutDefinition equipmentDefinition = PrototypeRpgEquipmentCatalog.ResolveDefinition(equipmentLoadoutId, roleTag);

        switch (normalizedRoleTag)
        {
            case "warrior": bundle.BonusMaxHp += 2; bundle.BonusDefense += 1; break;
            case "rogue": bundle.BonusAttack += 1; bundle.BonusSpeed += 1; break;
            case "mage": bundle.BonusAttack += 2; break;
            case "cleric": bundle.BonusMaxHp += 1; bundle.BonusDefense += 1; break;
        }

        if (normalizedGrowth.Contains("frontline") || normalizedJob.Contains("warrior")) bundle.BonusMaxHp += 1;
        else if (normalizedGrowth.Contains("precision") || normalizedJob.Contains("rogue")) bundle.BonusSpeed += 1;
        else if (normalizedGrowth.Contains("arcane") || normalizedJob.Contains("mage")) bundle.BonusAttack += 1;
        else if (normalizedGrowth.Contains("support") || normalizedJob.Contains("cleric")) bundle.BonusDefense += 1;

        if (equipmentDefinition != null)
        {
            bundle.EquipmentLoadoutId = equipmentDefinition.LoadoutId;
            bundle.EquipmentDisplayName = equipmentDefinition.DisplayName;
            bundle.EquipmentSlotKey = equipmentDefinition.Items != null && equipmentDefinition.Items.Length > 0 && equipmentDefinition.Items[0] != null ? equipmentDefinition.Items[0].SlotKey : string.Empty;
            bundle.EquipmentMaxHpDelta = equipmentDefinition.TotalMaxHpDelta;
            bundle.EquipmentAttackDelta = equipmentDefinition.TotalAttackDelta;
            bundle.EquipmentDefenseDelta = equipmentDefinition.TotalDefenseDelta;
            bundle.EquipmentSpeedDelta = equipmentDefinition.TotalSpeedDelta;
            bundle.EquipmentSummaryText = equipmentDefinition.DisplayName;
            bundle.GearContributionSummaryText = BuildRpgEquipmentContributionText(equipmentDefinition);
            bundle.PassiveHintText = string.IsNullOrEmpty(equipmentDefinition.PassiveHintText) ? string.Empty : equipmentDefinition.PassiveHintText;
            bundle.BattleLabelHint = string.IsNullOrEmpty(equipmentDefinition.BattleLabelHint) ? string.Empty : equipmentDefinition.BattleLabelHint;
            bundle.BonusMaxHp += equipmentDefinition.TotalMaxHpDelta;
            bundle.BonusAttack += equipmentDefinition.TotalAttackDelta;
            bundle.BonusDefense += equipmentDefinition.TotalDefenseDelta;
            bundle.BonusSpeed += equipmentDefinition.TotalSpeedDelta;
        }

        bundle.SummaryText = "HP+" + bundle.BonusMaxHp + " ATK+" + bundle.BonusAttack + " DEF+" + bundle.BonusDefense + " SPD+" + bundle.BonusSpeed;
        return bundle;
    }

    private PrototypeRpgDerivedStatSummary BuildRpgDerivedStatSummary(PrototypeRpgStatBlock baseStats, string roleTag, PrototypeRpgPartyMemberLevelRuntimeState levelState)
    {
        PrototypeRpgStatBlock safeBaseStats = baseStats ?? new PrototypeRpgStatBlock(1, 1, 0, 0);
        PrototypeRpgPartyMemberLevelRuntimeState safeLevelState = levelState ?? new PrototypeRpgPartyMemberLevelRuntimeState();
        int level = safeLevelState.Experience != null ? Mathf.Max(1, safeLevelState.Experience.Level) : 1;
        PrototypeRpgStatModifierBundle modifiers = safeLevelState.Modifiers ?? new PrototypeRpgStatModifierBundle();
        string safeRoleTag = string.IsNullOrEmpty(roleTag) ? string.Empty : roleTag;
        string levelBandKey = ResolveRpgGearLevelBandKey(level);
        PrototypeRpgEquipmentLoadoutDefinition loadoutDefinition = PrototypeRpgEquipmentCatalog.ResolveDefinition(safeLevelState.EquipmentLoadoutId, safeRoleTag);
        PrototypeRpgGearAffixLiteContribution[] affixContributions = BuildRpgGearAffixLiteContributions(safeRoleTag, loadoutDefinition, levelBandKey, LatestRpgGearUnlockState, false);
        modifiers.GearAffixLiteSummaryText = BuildRpgGearAffixLiteCandidateSummaryText(affixContributions);
        modifiers.LevelBandSummaryText = BuildRpgLevelBandSummaryText(levelBandKey, level);
        return BuildRpgDerivedStatSummaryForLevel(safeBaseStats, modifiers, level, affixContributions, levelBandKey);
    }

    private PrototypeRpgDerivedStatSummary BuildRpgDerivedStatSummaryForLevel(PrototypeRpgStatBlock baseStats, PrototypeRpgStatModifierBundle modifiers, int level, PrototypeRpgGearAffixLiteContribution[] affixContributions, string levelBandKey)
    {
        PrototypeRpgStatBlock safeBaseStats = baseStats ?? new PrototypeRpgStatBlock(1, 1, 0, 0);
        PrototypeRpgStatModifierBundle safeModifiers = modifiers ?? new PrototypeRpgStatModifierBundle();
        int safeLevel = Mathf.Max(1, level);
        int levelBonus = Mathf.Max(0, safeLevel - 1);
        int affixHpBonus = GetRpgAffixStatBonus(affixContributions, "hp");
        int affixAttackBonus = GetRpgAffixStatBonus(affixContributions, "attack");
        int affixDefenseBonus = GetRpgAffixStatBonus(affixContributions, "defense");
        int affixSpeedBonus = GetRpgAffixStatBonus(affixContributions, "speed");
        string affixSummaryText = BuildRpgGearAffixLiteCandidateSummaryText(affixContributions);
        string affixHintText = BuildRpgGearAffixHintSummaryText(affixContributions);
        string levelBandSummaryText = BuildRpgLevelBandSummaryText(levelBandKey, safeLevel);

        PrototypeRpgDerivedStatSummary summary = new PrototypeRpgDerivedStatSummary();
        summary.Level = safeLevel;
        summary.MaxHp = Mathf.Max(1, safeBaseStats.MaxHp + (levelBonus * 2) + safeModifiers.BonusMaxHp + affixHpBonus);
        summary.Attack = Mathf.Max(1, safeBaseStats.Attack + levelBonus + safeModifiers.BonusAttack + affixAttackBonus);
        summary.Defense = Mathf.Max(0, safeBaseStats.Defense + (levelBonus / 2) + safeModifiers.BonusDefense + affixDefenseBonus);
        summary.Speed = Mathf.Max(0, safeBaseStats.Speed + ((levelBonus + 1) / 2) + safeModifiers.BonusSpeed + affixSpeedBonus);
        summary.PowerScore = summary.MaxHp + (summary.Attack * 3) + (summary.Defense * 2) + summary.Speed;
        summary.SkillPowerBonus = Mathf.Max(0, safeModifiers.BonusSkillPower);
        summary.JobSpecializationKey = string.IsNullOrEmpty(safeModifiers.JobSpecializationKey) ? string.Empty : safeModifiers.JobSpecializationKey;
        summary.JobSpecializationSummaryText = string.IsNullOrEmpty(safeModifiers.JobSpecializationSummaryText) ? string.Empty : safeModifiers.JobSpecializationSummaryText;
        summary.PassiveSkillId = string.IsNullOrEmpty(safeModifiers.PassiveSkillId) ? string.Empty : safeModifiers.PassiveSkillId;
        summary.PassiveSkillSlotSummaryText = string.IsNullOrEmpty(safeModifiers.PassiveSkillSlotSummaryText) ? string.Empty : safeModifiers.PassiveSkillSlotSummaryText;
        summary.RuntimeSynergySummaryText = string.IsNullOrEmpty(safeModifiers.RuntimeSynergySummaryText) ? string.Empty : safeModifiers.RuntimeSynergySummaryText;
        summary.EquipmentSummaryText = string.IsNullOrEmpty(safeModifiers.EquipmentSummaryText) ? string.Empty : safeModifiers.EquipmentSummaryText;
        summary.GearContributionSummaryText = BuildRpgCompactSummaryText(safeModifiers.GearContributionSummaryText, affixSummaryText);
        summary.GearAffixLiteSummaryText = affixSummaryText;
        summary.LevelBandSummaryText = levelBandSummaryText;
        summary.PassiveHintText = BuildRpgCompactSummaryText(safeModifiers.PassiveHintText, summary.PassiveSkillSlotSummaryText, affixHintText);
        summary.BattleLabelHint = BuildRpgCompactSummaryText(safeModifiers.BattleLabelHint, summary.JobSpecializationSummaryText, affixHintText);
        string gearText = string.IsNullOrEmpty(summary.EquipmentSummaryText) ? string.Empty : " | Gear " + summary.EquipmentSummaryText;
        string affixText = string.IsNullOrEmpty(summary.GearAffixLiteSummaryText) ? string.Empty : " | " + summary.GearAffixLiteSummaryText;
        string specializationText = string.IsNullOrEmpty(summary.JobSpecializationSummaryText) ? string.Empty : " | " + summary.JobSpecializationSummaryText;
        summary.SummaryText = "Lv " + summary.Level + " | HP " + summary.MaxHp + " ATK " + summary.Attack + " DEF " + summary.Defense + " SPD " + summary.Speed + gearText + affixText + specializationText;
        return summary;
    }

    private string BuildRpgGearAffixHintSummaryText(PrototypeRpgGearAffixLiteContribution[] affixContributions)
    {
        if (affixContributions == null || affixContributions.Length <= 0)
        {
            return string.Empty;
        }

        List<string> parts = new List<string>();
        for (int i = 0; i < affixContributions.Length; i++)
        {
            PrototypeRpgGearAffixLiteContribution affix = affixContributions[i];
            if (affix == null || string.IsNullOrEmpty(affix.HintText))
            {
                continue;
            }

            AddRpgSummaryPart(parts, affix.HintText);
        }

        return parts.Count <= 0 ? string.Empty : string.Join(" | ", parts.ToArray());
    }

    private string BuildRpgLevelSummaryText(PrototypeRpgPartyMemberLevelRuntimeState levelState)
    {
        if (levelState == null || levelState.Experience == null)
        {
            return string.Empty;
        }

        return "Lv " + Mathf.Max(1, levelState.Experience.Level) + " " + Mathf.Max(0, levelState.Experience.CurrentExperience) + "/" + Mathf.Max(1, levelState.Experience.ExperienceToNextLevel);
    }

    private string BuildRpgActualLevelApplySummaryText(PrototypeRpgPartyLevelRuntimeState partyLevelState)
    {
        if (partyLevelState == null || partyLevelState.Members == null || partyLevelState.Members.Length <= 0)
        {
            return string.Empty;
        }

        List<string> parts = new List<string>();
        for (int i = 0; i < partyLevelState.Members.Length; i++)
        {
            PrototypeRpgPartyMemberLevelRuntimeState memberState = partyLevelState.Members[i];
            if (memberState == null || string.IsNullOrEmpty(memberState.ActualLevelApplySummaryText))
            {
                continue;
            }

            parts.Add(memberState.ActualLevelApplySummaryText);
        }

        return parts.Count <= 0 ? string.Empty : "Applied levels: " + string.Join(" | ", parts.ToArray());
    }

    private string BuildRpgDerivedStatHydrateSummaryText(PrototypeRpgPartyLevelRuntimeState partyLevelState)
    {
        if (partyLevelState == null || partyLevelState.Members == null || partyLevelState.Members.Length <= 0)
        {
            return string.Empty;
        }

        List<string> parts = new List<string>();
        for (int i = 0; i < partyLevelState.Members.Length; i++)
        {
            PrototypeRpgPartyMemberLevelRuntimeState memberState = partyLevelState.Members[i];
            if (memberState == null || string.IsNullOrEmpty(memberState.DerivedStatHydrateSummaryText))
            {
                continue;
            }

            parts.Add(memberState.DerivedStatHydrateSummaryText);
        }

        return parts.Count <= 0 ? string.Empty : "Next hydrate: " + string.Join(" | ", parts.ToArray());
    }

    private string BuildRpgPartyLevelSummaryText(PrototypeRpgPartyLevelRuntimeState partyLevelState)
    {
        if (partyLevelState == null || partyLevelState.Members == null || partyLevelState.Members.Length <= 0)
        {
            return string.Empty;
        }

        int totalLevel = 0;
        int totalCurrentExp = 0;
        int totalNextExp = 0;
        int equippedCount = 0;
        for (int i = 0; i < partyLevelState.Members.Length; i++)
        {
            PrototypeRpgPartyMemberLevelRuntimeState memberState = partyLevelState.Members[i];
            if (memberState == null || memberState.Experience == null)
            {
                continue;
            }

            totalLevel += Mathf.Max(1, memberState.Experience.Level);
            totalCurrentExp += Mathf.Max(0, memberState.Experience.CurrentExperience);
            totalNextExp += Mathf.Max(1, memberState.Experience.ExperienceToNextLevel);
            if (!string.IsNullOrEmpty(memberState.EquipmentLoadoutId)) equippedCount += 1;
        }

        int averageLevel = partyLevelState.Members.Length > 0 ? Mathf.Max(1, Mathf.RoundToInt((float)totalLevel / partyLevelState.Members.Length)) : 1;
        return BuildRpgCompactSummaryText(
            "Levels avg Lv " + averageLevel,
            "EXP " + totalCurrentExp + "/" + totalNextExp,
            "Grant " + Mathf.Max(0, partyLevelState.LastGrantedExperience),
            "Level jumps " + Mathf.Max(0, partyLevelState.PendingLevelUpCount),
            "Hydrate-ready " + Mathf.Max(0, partyLevelState.AppliedMemberCount),
            "Gear-ready " + equippedCount,
            partyLevelState.JobSpecializationSummaryText,
            partyLevelState.PassiveSkillSlotSummaryText);
    }
    private void ApplyRpgConsumableCarryoverPolicy(PrototypeRpgInventoryRuntimeState inventoryState, PrototypeRpgRunResultSnapshot runResultSnapshot)
    {
        if (inventoryState == null || runResultSnapshot == null)
        {
            return;
        }

        int depletedCount = TrimRpgDepletedInventoryEntries(inventoryState);
        inventoryState.DepletedConsumableCount = depletedCount;
        AwardRpgResultConsumableCarry(inventoryState, runResultSnapshot);
        inventoryState.TotalConsumableCount = CountRpgInventoryEntries(inventoryState.Consumables);
        inventoryState.TotalCarryRewardAmount = CountRpgCarryRewardAmount(inventoryState.CarryEntries);
        RefreshRpgInventoryBattleSlotState(inventoryState);
        inventoryState.ReplenishSummaryText = BuildRpgConsumableReplenishSummaryText(runResultSnapshot);
        inventoryState.DepletedSummaryText = BuildRpgDepletedConsumableSummaryText(depletedCount);
        inventoryState.CarryoverPolicySummaryText = BuildRpgConsumableCarryoverPolicySummaryText(inventoryState, runResultSnapshot, depletedCount);
        inventoryState.NextRunCarrySummaryText = BuildRpgNextRunCarrySummaryText(inventoryState, runResultSnapshot);
        inventoryState.InventorySummaryText = BuildRpgInventorySummaryText(inventoryState);
    }

    private int TrimRpgDepletedInventoryEntries(PrototypeRpgInventoryRuntimeState inventoryState)
    {
        if (inventoryState == null)
        {
            return 0;
        }

        PrototypeRpgInventoryEntry[] entries = inventoryState.Consumables ?? Array.Empty<PrototypeRpgInventoryEntry>();
        List<PrototypeRpgInventoryEntry> retained = new List<PrototypeRpgInventoryEntry>();
        int depletedCount = 0;
        for (int i = 0; i < entries.Length; i++)
        {
            PrototypeRpgInventoryEntry entry = entries[i];
            if (entry == null)
            {
                continue;
            }

            if (entry.Quantity <= 0)
            {
                depletedCount += 1;
                continue;
            }

            retained.Add(entry);
        }

        inventoryState.Consumables = retained.ToArray();
        return depletedCount;
    }

    private string BuildRpgConsumableReplenishSummaryText(PrototypeRpgRunResultSnapshot runResultSnapshot)
    {
        if (runResultSnapshot == null)
        {
            return string.Empty;
        }

        List<string> parts = new List<string>();
        if (runResultSnapshot.ResultStateKey == PrototypeBattleOutcomeKeys.RunClear)
        {
            parts.Add("clear replenished Field Tonic + Burst Flask");
        }

        if (runResultSnapshot.EliteOutcome != null && runResultSnapshot.EliteOutcome.IsEliteDefeated)
        {
            parts.Add("elite clear replenished Smoke Ward + Safeguard Kit");
        }

        return parts.Count <= 0 ? "Replenish: none." : "Replenish: " + string.Join(" | ", parts.ToArray()) + ".";
    }

    private string BuildRpgDepletedConsumableSummaryText(int depletedCount)
    {
        return depletedCount > 0
            ? "Depleted kits removed from next-run carry: " + depletedCount + "."
            : "No depleted kits were removed.";
    }

    private string BuildRpgConsumableCarryoverPolicySummaryText(PrototypeRpgInventoryRuntimeState inventoryState, PrototypeRpgRunResultSnapshot runResultSnapshot, int depletedCount)
    {
        string resultStateKey = runResultSnapshot != null ? runResultSnapshot.ResultStateKey : string.Empty;
        string basePolicy = resultStateKey == PrototypeBattleOutcomeKeys.RunClear
            ? "Carryover policy: keep remaining kits, trim depleted slots, then grant clear replenish."
            : resultStateKey == PrototypeBattleOutcomeKeys.RunRetreat
                ? "Carryover policy: keep remaining kits, trim depleted slots, no retreat replenish."
                : resultStateKey == PrototypeBattleOutcomeKeys.RunDefeat
                    ? "Carryover policy: keep surviving kits, trim depleted slots, no defeat replenish."
                    : "Carryover policy: keep current kits and trim depleted slots.";
        string depletedText = depletedCount > 0 ? " Removed " + depletedCount + " depleted slot(s)." : " No depleted slots were removed.";
        string remainingText = " Remaining kits " + CountRpgInventoryEntries(inventoryState != null ? inventoryState.Consumables : Array.Empty<PrototypeRpgInventoryEntry>()) + ".";
        return basePolicy + depletedText + remainingText;
    }

    private string BuildRpgNextRunCarrySummaryText(PrototypeRpgInventoryRuntimeState inventoryState, PrototypeRpgRunResultSnapshot runResultSnapshot)
    {
        string resultStateKey = runResultSnapshot != null ? runResultSnapshot.ResultStateKey : string.Empty;
        string routeText = resultStateKey == PrototypeBattleOutcomeKeys.RunClear ? "next run starts with replenished carry." : "next run starts from the surviving carry.";
        string slotSummaryText = inventoryState != null && !string.IsNullOrEmpty(inventoryState.BattleConsumableSlotSummaryText)
            ? inventoryState.BattleConsumableSlotSummaryText
            : BuildCurrentBattleConsumableSlotSummaryText(inventoryState);
        return "Next run carry: " + routeText + " Slot " + slotSummaryText + ".";
    }

    private string BuildRpgInventorySummaryText(PrototypeRpgInventoryRuntimeState inventoryState)
    {
        if (inventoryState == null)
        {
            return string.Empty;
        }

        int consumableCount = CountRpgInventoryEntries(inventoryState.Consumables);
        int carryAmount = CountRpgCarryRewardAmount(inventoryState.CarryEntries);
        string equipmentSummaryText = string.IsNullOrEmpty(inventoryState.EquipmentSummaryText) ? "Gear none" : inventoryState.EquipmentSummaryText;
        string slotSummaryText = string.IsNullOrEmpty(inventoryState.BattleConsumableSlotSummaryText) ? BuildCurrentBattleConsumableSlotSummaryText(inventoryState) : inventoryState.BattleConsumableSlotSummaryText;
        string lastRunText = string.IsNullOrEmpty(inventoryState.LastRunIdentity) ? "none" : inventoryState.LastRunIdentity;
        string useSummaryText = string.IsNullOrEmpty(inventoryState.ConsumedThisRunSummaryText) ? "Consumable runtime use: none this run." : inventoryState.ConsumedThisRunSummaryText;
        string policyText = string.IsNullOrEmpty(inventoryState.CarryoverPolicySummaryText) ? "Carryover policy: baseline." : inventoryState.CarryoverPolicySummaryText;
        return equipmentSummaryText + " | Inventory kits " + consumableCount + " | Slot " + slotSummaryText + " | " + useSummaryText + " | Carry " + BuildLootAmountText(carryAmount) + " | " + policyText + " | Last " + lastRunText;
    }
    private int CountRpgInventoryEntries(PrototypeRpgInventoryEntry[] entries)
    {
        int total = 0;
        PrototypeRpgInventoryEntry[] safeEntries = entries ?? Array.Empty<PrototypeRpgInventoryEntry>();
        for (int i = 0; i < safeEntries.Length; i++)
        {
            PrototypeRpgInventoryEntry entry = safeEntries[i];
            if (entry != null)
            {
                total += Mathf.Max(0, entry.Quantity);
            }
        }

        return total;
    }

    private int CountRpgCarryRewardAmount(PrototypeRpgRewardCarryEntry[] entries)
    {
        int total = 0;
        PrototypeRpgRewardCarryEntry[] safeEntries = entries ?? Array.Empty<PrototypeRpgRewardCarryEntry>();
        for (int i = 0; i < safeEntries.Length; i++)
        {
            PrototypeRpgRewardCarryEntry entry = safeEntries[i];
            if (entry != null)
            {
                total += Mathf.Max(0, entry.Amount);
            }
        }

        return total;
    }
    private string BuildRpgSkillLoadoutSummaryText(PrototypeRpgSkillLoadoutDefinition loadoutDefinition)
    {
        if (loadoutDefinition == null)
        {
            return string.Empty;
        }

        PrototypeRpgSkillDefinition primarySkill = PrototypeRpgSkillCatalog.GetDefinition(loadoutDefinition.PrimarySkillId);
        PrototypeRpgSkillDefinition secondarySkill = PrototypeRpgSkillCatalog.GetDefinition(loadoutDefinition.SecondarySkillId);
        string primaryLabel = primarySkill != null ? primarySkill.DisplayName : "Primary";
        string secondaryLabel = secondarySkill != null ? secondarySkill.DisplayName : "Support";
        return loadoutDefinition.DisplayName + " | " + primaryLabel + " / " + secondaryLabel;
    }

    private string BuildRpgInventoryMemberHintText(PrototypeRpgInventoryRuntimeState inventoryState, string memberId)
    {
        if (inventoryState == null)
        {
            return string.Empty;
        }

        PrototypeRpgEquippedLoadoutEntry equippedEntry = GetRpgEquippedLoadoutEntry(inventoryState, memberId);
        string gearText = equippedEntry != null && !string.IsNullOrEmpty(equippedEntry.DisplayName) ? equippedEntry.DisplayName + " | " + equippedEntry.GearContributionSummaryText : string.Empty;
        string slotSummaryText = string.IsNullOrEmpty(inventoryState.BattleConsumableSlotSummaryText) ? BuildCurrentBattleConsumableSlotSummaryText(inventoryState) : inventoryState.BattleConsumableSlotSummaryText;
        string useSummaryText = string.IsNullOrEmpty(inventoryState.ConsumedThisRunSummaryText) ? string.Empty : " / " + inventoryState.ConsumedThisRunSummaryText;
        string carryText = "Carry " + BuildLootAmountText(CountRpgCarryRewardAmount(inventoryState.CarryEntries));
        return string.IsNullOrEmpty(gearText)
            ? "Slot " + slotSummaryText + useSummaryText + " / " + carryText
            : gearText + " / Slot " + slotSummaryText + useSummaryText + " / " + carryText;
    }

    private string BuildRpgBackboneBasisSuffixText()
    {
        List<string> parts = new List<string>();
        if (!string.IsNullOrEmpty(_sessionRpgPartyLevelRuntimeState.SummaryText)) parts.Add(_sessionRpgPartyLevelRuntimeState.SummaryText);
        if (!string.IsNullOrEmpty(_sessionRpgPartyLevelRuntimeState.ActualLevelApplySummaryText)) parts.Add(_sessionRpgPartyLevelRuntimeState.ActualLevelApplySummaryText);
        if (!string.IsNullOrEmpty(_sessionRpgPartyLevelRuntimeState.DerivedStatHydrateSummaryText)) parts.Add(_sessionRpgPartyLevelRuntimeState.DerivedStatHydrateSummaryText);
        if (!string.IsNullOrEmpty(_sessionRpgPartyLevelRuntimeState.JobSpecializationSummaryText)) parts.Add(_sessionRpgPartyLevelRuntimeState.JobSpecializationSummaryText);
        if (!string.IsNullOrEmpty(_sessionRpgPartyLevelRuntimeState.PassiveSkillSlotSummaryText)) parts.Add(_sessionRpgPartyLevelRuntimeState.PassiveSkillSlotSummaryText);
        if (!string.IsNullOrEmpty(_sessionRpgPartyLevelRuntimeState.RuntimeSynergySummaryText)) parts.Add(_sessionRpgPartyLevelRuntimeState.RuntimeSynergySummaryText);
        if (!string.IsNullOrEmpty(_sessionRpgPartyLevelRuntimeState.EquipmentLoadoutReadbackSummaryText)) parts.Add(_sessionRpgPartyLevelRuntimeState.EquipmentLoadoutReadbackSummaryText);
        if (!string.IsNullOrEmpty(_sessionRpgPartyLevelRuntimeState.GearContributionSummaryText)) parts.Add(_sessionRpgPartyLevelRuntimeState.GearContributionSummaryText);
        if (!string.IsNullOrEmpty(CurrentNextRunActiveSkillPreviewText) && CurrentNextRunActiveSkillPreviewText != "None") parts.Add(CurrentNextRunActiveSkillPreviewText);
        if (!string.IsNullOrEmpty(_sessionRpgInventoryRuntimeState.InventorySummaryText)) parts.Add(_sessionRpgInventoryRuntimeState.InventorySummaryText);
        if (!string.IsNullOrEmpty(_sessionRpgInventoryRuntimeState.CarryoverPolicySummaryText)) parts.Add(_sessionRpgInventoryRuntimeState.CarryoverPolicySummaryText);
        return parts.Count <= 0 ? string.Empty : " Backbone: " + string.Join(" | ", parts.ToArray()) + ".";
    }
    private string BuildRpgMemberBackboneSummaryText(string memberId)
    {
        PrototypeRpgPartyMemberLevelRuntimeState memberState = GetRpgMemberLevelRuntimeState(_sessionRpgPartyLevelRuntimeState, memberId);
        List<string> parts = new List<string>();
        if (memberState != null)
        {
            if (!string.IsNullOrEmpty(memberState.LevelSummaryText)) parts.Add(memberState.LevelSummaryText);
            PrototypeRpgSkillLoadoutDefinition loadoutDefinition = PrototypeRpgSkillLoadoutCatalog.ResolveDefinition(memberState.SkillLoadoutId, string.Empty, string.Empty);
            string loadoutSummaryText = BuildRpgSkillLoadoutSummaryText(loadoutDefinition);
            if (!string.IsNullOrEmpty(loadoutSummaryText)) parts.Add(loadoutSummaryText);
            if (!string.IsNullOrEmpty(memberState.JobSpecializationSummaryText)) parts.Add(memberState.JobSpecializationSummaryText);
            if (!string.IsNullOrEmpty(memberState.PassiveSkillSlotSummaryText)) parts.Add(memberState.PassiveSkillSlotSummaryText);
            if (!string.IsNullOrEmpty(memberState.RuntimeSynergySummaryText)) parts.Add(memberState.RuntimeSynergySummaryText);
            if (!string.IsNullOrEmpty(memberState.EquipmentSummaryText)) parts.Add(memberState.EquipmentSummaryText);
            if (!string.IsNullOrEmpty(memberState.GearContributionSummaryText)) parts.Add(memberState.GearContributionSummaryText);
        }

        DungeonPartyMemberRuntimeData runtimeMember = GetPartyMemberById(memberId);
        if (runtimeMember != null)
        {
            if (!string.IsNullOrEmpty(runtimeMember.ActiveSkillSlotSummaryText)) parts.Add(runtimeMember.ActiveSkillSlotSummaryText);
            if (!string.IsNullOrEmpty(runtimeMember.SkillResourceSummaryText)) parts.Add(runtimeMember.SkillResourceSummaryText);
            if (!string.IsNullOrEmpty(runtimeMember.NextRunActiveSkillPreviewText)) parts.Add(runtimeMember.NextRunActiveSkillPreviewText);
        }

        string inventoryHintText = BuildRpgInventoryMemberHintText(_sessionRpgInventoryRuntimeState, memberId);
        if (!string.IsNullOrEmpty(inventoryHintText)) parts.Add(inventoryHintText);
        return parts.Count <= 0 ? string.Empty : " | " + string.Join(" | ", parts.ToArray());
    }

    private string BuildRpgRunBackboneSummaryText()
    {
        string suffixText = BuildRpgBackboneBasisSuffixText();
        return string.IsNullOrEmpty(suffixText) ? string.Empty : "Progression" + suffixText;
    }

    private string ResolveAppliedOrDefinitionId(string appliedValue, string definitionValue)
    {
        return string.IsNullOrWhiteSpace(appliedValue)
            ? (string.IsNullOrWhiteSpace(definitionValue) ? string.Empty : definitionValue.Trim())
            : appliedValue.Trim();
    }

    private PrototypeRpgPartyLevelRuntimeState CopyRpgPartyLevelRuntimeState(PrototypeRpgPartyLevelRuntimeState source)
    {
        PrototypeRpgPartyLevelRuntimeState copy = new PrototypeRpgPartyLevelRuntimeState();
        if (source == null)
        {
            return copy;
        }

        copy.PartyId = string.IsNullOrEmpty(source.PartyId) ? string.Empty : source.PartyId;
        copy.DisplayName = string.IsNullOrEmpty(source.DisplayName) ? string.Empty : source.DisplayName;
        copy.LastRunIdentity = string.IsNullOrEmpty(source.LastRunIdentity) ? string.Empty : source.LastRunIdentity;
        copy.AppliedLastRunIdentity = string.IsNullOrEmpty(source.AppliedLastRunIdentity) ? string.Empty : source.AppliedLastRunIdentity;
        copy.LastGrantedExperience = Mathf.Max(0, source.LastGrantedExperience);
        copy.PendingLevelUpCount = Mathf.Max(0, source.PendingLevelUpCount);
        copy.AppliedMemberCount = Mathf.Max(0, source.AppliedMemberCount);
        copy.SummaryText = string.IsNullOrEmpty(source.SummaryText) ? string.Empty : source.SummaryText;
        copy.ExperienceGainSummaryText = string.IsNullOrEmpty(source.ExperienceGainSummaryText) ? string.Empty : source.ExperienceGainSummaryText;
        copy.LevelUpPreviewSummaryText = string.IsNullOrEmpty(source.LevelUpPreviewSummaryText) ? string.Empty : source.LevelUpPreviewSummaryText;
        copy.LevelUpApplyReadySummaryText = string.IsNullOrEmpty(source.LevelUpApplyReadySummaryText) ? string.Empty : source.LevelUpApplyReadySummaryText;
        copy.ActualLevelApplySummaryText = string.IsNullOrEmpty(source.ActualLevelApplySummaryText) ? string.Empty : source.ActualLevelApplySummaryText;
        copy.DerivedStatHydrateSummaryText = string.IsNullOrEmpty(source.DerivedStatHydrateSummaryText) ? string.Empty : source.DerivedStatHydrateSummaryText;
        copy.NextRunStatProjectionSummaryText = string.IsNullOrEmpty(source.NextRunStatProjectionSummaryText) ? string.Empty : source.NextRunStatProjectionSummaryText;
        copy.JobSpecializationSummaryText = string.IsNullOrEmpty(source.JobSpecializationSummaryText) ? string.Empty : source.JobSpecializationSummaryText;
        copy.PassiveSkillSlotSummaryText = string.IsNullOrEmpty(source.PassiveSkillSlotSummaryText) ? string.Empty : source.PassiveSkillSlotSummaryText;
        copy.RuntimeSynergySummaryText = string.IsNullOrEmpty(source.RuntimeSynergySummaryText) ? string.Empty : source.RuntimeSynergySummaryText;
        copy.NextRunSpecializationPreviewText = string.IsNullOrEmpty(source.NextRunSpecializationPreviewText) ? string.Empty : source.NextRunSpecializationPreviewText;
        copy.EquipmentLoadoutReadbackSummaryText = string.IsNullOrEmpty(source.EquipmentLoadoutReadbackSummaryText) ? string.Empty : source.EquipmentLoadoutReadbackSummaryText;
        copy.GearContributionSummaryText = string.IsNullOrEmpty(source.GearContributionSummaryText) ? string.Empty : source.GearContributionSummaryText;
        copy.GearAffixLiteSummaryText = string.IsNullOrEmpty(source.GearAffixLiteSummaryText) ? string.Empty : source.GearAffixLiteSummaryText;
        copy.LevelBandGearLinkageSummaryText = string.IsNullOrEmpty(source.LevelBandGearLinkageSummaryText) ? string.Empty : source.LevelBandGearLinkageSummaryText;
        PrototypeRpgPartyMemberLevelRuntimeState[] sourceMembers = source.Members ?? Array.Empty<PrototypeRpgPartyMemberLevelRuntimeState>();
        PrototypeRpgPartyMemberLevelRuntimeState[] copiedMembers = new PrototypeRpgPartyMemberLevelRuntimeState[sourceMembers.Length];
        for (int i = 0; i < sourceMembers.Length; i++) { copiedMembers[i] = CopyRpgPartyMemberLevelRuntimeState(sourceMembers[i]); }
        copy.Members = copiedMembers;
        return copy;
    }
    private PrototypeRpgPartyMemberLevelRuntimeState CopyRpgPartyMemberLevelRuntimeState(PrototypeRpgPartyMemberLevelRuntimeState source)
    {
        PrototypeRpgPartyMemberLevelRuntimeState copy = new PrototypeRpgPartyMemberLevelRuntimeState();
        if (source == null)
        {
            return copy;
        }

        copy.MemberId = string.IsNullOrEmpty(source.MemberId) ? string.Empty : source.MemberId;
        copy.DisplayName = string.IsNullOrEmpty(source.DisplayName) ? string.Empty : source.DisplayName;
        copy.GrowthTrackId = string.IsNullOrEmpty(source.GrowthTrackId) ? string.Empty : source.GrowthTrackId;
        copy.JobId = string.IsNullOrEmpty(source.JobId) ? string.Empty : source.JobId;
        copy.EquipmentLoadoutId = string.IsNullOrEmpty(source.EquipmentLoadoutId) ? string.Empty : source.EquipmentLoadoutId;
        copy.SkillLoadoutId = string.IsNullOrEmpty(source.SkillLoadoutId) ? string.Empty : source.SkillLoadoutId;
        copy.LastGrantedExperience = Mathf.Max(0, source.LastGrantedExperience);
        copy.AppliedLevelThisSession = Mathf.Max(1, source.AppliedLevelThisSession);
        copy.LastAppliedRunIdentity = string.IsNullOrEmpty(source.LastAppliedRunIdentity) ? string.Empty : source.LastAppliedRunIdentity;
        copy.ActualLevelApplySummaryText = string.IsNullOrEmpty(source.ActualLevelApplySummaryText) ? string.Empty : source.ActualLevelApplySummaryText;
        copy.DerivedStatHydrateSummaryText = string.IsNullOrEmpty(source.DerivedStatHydrateSummaryText) ? string.Empty : source.DerivedStatHydrateSummaryText;
        copy.JobSpecializationKey = string.IsNullOrEmpty(source.JobSpecializationKey) ? string.Empty : source.JobSpecializationKey;
        copy.JobSpecializationSummaryText = string.IsNullOrEmpty(source.JobSpecializationSummaryText) ? string.Empty : source.JobSpecializationSummaryText;
        copy.PassiveSkillId = string.IsNullOrEmpty(source.PassiveSkillId) ? string.Empty : source.PassiveSkillId;
        copy.PassiveSkillSlotSummaryText = string.IsNullOrEmpty(source.PassiveSkillSlotSummaryText) ? string.Empty : source.PassiveSkillSlotSummaryText;
        copy.RuntimeSynergySummaryText = string.IsNullOrEmpty(source.RuntimeSynergySummaryText) ? string.Empty : source.RuntimeSynergySummaryText;
        copy.NextRunSpecializationPreviewText = string.IsNullOrEmpty(source.NextRunSpecializationPreviewText) ? string.Empty : source.NextRunSpecializationPreviewText;
        copy.PendingUnlockHintText = string.IsNullOrEmpty(source.PendingUnlockHintText) ? string.Empty : source.PendingUnlockHintText;
        copy.RuntimeSkillPowerBonus = Mathf.Max(0, source.RuntimeSkillPowerBonus);
        copy.EquipmentSummaryText = string.IsNullOrEmpty(source.EquipmentSummaryText) ? string.Empty : source.EquipmentSummaryText;
        copy.GearContributionSummaryText = string.IsNullOrEmpty(source.GearContributionSummaryText) ? string.Empty : source.GearContributionSummaryText;
        copy.GearAffixLiteSummaryText = string.IsNullOrEmpty(source.GearAffixLiteSummaryText) ? string.Empty : source.GearAffixLiteSummaryText;
        copy.LevelBandSummaryText = string.IsNullOrEmpty(source.LevelBandSummaryText) ? string.Empty : source.LevelBandSummaryText;
        copy.PassiveHintText = string.IsNullOrEmpty(source.PassiveHintText) ? string.Empty : source.PassiveHintText;
        copy.BattleLabelHint = string.IsNullOrEmpty(source.BattleLabelHint) ? string.Empty : source.BattleLabelHint;
        copy.Experience = CopyRpgExperienceProgress(source.Experience);
        copy.Modifiers = CopyRpgStatModifierBundle(source.Modifiers);
        copy.DerivedStats = CopyRpgDerivedStatSummary(source.DerivedStats);
        copy.LevelUpPreview = CopyRpgLevelUpPreview(source.LevelUpPreview);
        copy.LevelSummaryText = string.IsNullOrEmpty(source.LevelSummaryText) ? string.Empty : source.LevelSummaryText;
        return copy;
    }
    private PrototypeRpgExperienceProgress CopyRpgExperienceProgress(PrototypeRpgExperienceProgress source)
    {
        PrototypeRpgExperienceProgress copy = new PrototypeRpgExperienceProgress();
        if (source == null)
        {
            return copy;
        }

        copy.Level = Mathf.Max(1, source.Level);
        copy.CurrentExperience = Mathf.Max(0, source.CurrentExperience);
        copy.ExperienceToNextLevel = Mathf.Max(1, source.ExperienceToNextLevel);
        copy.LifetimeExperience = Mathf.Max(0, source.LifetimeExperience);
        copy.GainedExperienceThisRun = Mathf.Max(0, source.GainedExperienceThisRun);
        copy.LastGrantedExperience = Mathf.Max(0, source.LastGrantedExperience);
        copy.PreviousLevel = Mathf.Max(1, source.PreviousLevel);
        copy.AppliedLevelThisSession = Mathf.Max(1, source.AppliedLevelThisSession);
        copy.LeveledUpThisRun = source.LeveledUpThisRun;
        copy.LastAppliedRunIdentity = string.IsNullOrEmpty(source.LastAppliedRunIdentity) ? string.Empty : source.LastAppliedRunIdentity;
        copy.SummaryText = string.IsNullOrEmpty(source.SummaryText) ? string.Empty : source.SummaryText;
        return copy;
    }
    private PrototypeRpgLevelUpPreview CopyRpgLevelUpPreview(PrototypeRpgLevelUpPreview source)
    {
        PrototypeRpgLevelUpPreview copy = new PrototypeRpgLevelUpPreview();
        if (source == null)
        {
            return copy;
        }

        copy.MemberId = string.IsNullOrEmpty(source.MemberId) ? string.Empty : source.MemberId;
        copy.DisplayName = string.IsNullOrEmpty(source.DisplayName) ? string.Empty : source.DisplayName;
        copy.PreviousLevel = Mathf.Max(1, source.PreviousLevel);
        copy.NewLevel = Mathf.Max(1, source.NewLevel);
        copy.PendingLevelUps = Mathf.Max(0, source.PendingLevelUps);
        copy.GainedExperienceThisRun = Mathf.Max(0, source.GainedExperienceThisRun);
        copy.LeveledUpThisRun = source.LeveledUpThisRun;
        copy.ProjectedMaxHpDelta = Mathf.Max(0, source.ProjectedMaxHpDelta);
        copy.ProjectedAttackDelta = Mathf.Max(0, source.ProjectedAttackDelta);
        copy.ProjectedDefenseDelta = Mathf.Max(0, source.ProjectedDefenseDelta);
        copy.ProjectedSpeedDelta = Mathf.Max(0, source.ProjectedSpeedDelta);
        copy.DerivedStatSummaryText = string.IsNullOrEmpty(source.DerivedStatSummaryText) ? string.Empty : source.DerivedStatSummaryText;
        copy.NextRunStatProjectionSummaryText = string.IsNullOrEmpty(source.NextRunStatProjectionSummaryText) ? string.Empty : source.NextRunStatProjectionSummaryText;
        copy.ApplyReadySummaryText = string.IsNullOrEmpty(source.ApplyReadySummaryText) ? string.Empty : source.ApplyReadySummaryText;
        copy.ActualApplySummaryText = string.IsNullOrEmpty(source.ActualApplySummaryText) ? string.Empty : source.ActualApplySummaryText;
        copy.SummaryText = string.IsNullOrEmpty(source.SummaryText) ? string.Empty : source.SummaryText;
        return copy;
    }
    private PrototypeRpgStatModifierBundle CopyRpgStatModifierBundle(PrototypeRpgStatModifierBundle source)
    {
        PrototypeRpgStatModifierBundle copy = new PrototypeRpgStatModifierBundle();
        if (source == null)
        {
            return copy;
        }

        copy.BonusMaxHp = source.BonusMaxHp;
        copy.BonusAttack = source.BonusAttack;
        copy.BonusDefense = source.BonusDefense;
        copy.BonusSpeed = source.BonusSpeed;
        copy.BonusSkillPower = source.BonusSkillPower;
        copy.EquipmentMaxHpDelta = source.EquipmentMaxHpDelta;
        copy.EquipmentAttackDelta = source.EquipmentAttackDelta;
        copy.EquipmentDefenseDelta = source.EquipmentDefenseDelta;
        copy.EquipmentSpeedDelta = source.EquipmentSpeedDelta;
        copy.EquipmentLoadoutId = string.IsNullOrEmpty(source.EquipmentLoadoutId) ? string.Empty : source.EquipmentLoadoutId;
        copy.EquipmentDisplayName = string.IsNullOrEmpty(source.EquipmentDisplayName) ? string.Empty : source.EquipmentDisplayName;
        copy.EquipmentSlotKey = string.IsNullOrEmpty(source.EquipmentSlotKey) ? string.Empty : source.EquipmentSlotKey;
        copy.JobSpecializationKey = string.IsNullOrEmpty(source.JobSpecializationKey) ? string.Empty : source.JobSpecializationKey;
        copy.JobSpecializationSummaryText = string.IsNullOrEmpty(source.JobSpecializationSummaryText) ? string.Empty : source.JobSpecializationSummaryText;
        copy.PassiveSkillId = string.IsNullOrEmpty(source.PassiveSkillId) ? string.Empty : source.PassiveSkillId;
        copy.PassiveSkillSlotSummaryText = string.IsNullOrEmpty(source.PassiveSkillSlotSummaryText) ? string.Empty : source.PassiveSkillSlotSummaryText;
        copy.RuntimeSynergySummaryText = string.IsNullOrEmpty(source.RuntimeSynergySummaryText) ? string.Empty : source.RuntimeSynergySummaryText;
        copy.EquipmentSummaryText = string.IsNullOrEmpty(source.EquipmentSummaryText) ? string.Empty : source.EquipmentSummaryText;
        copy.GearContributionSummaryText = string.IsNullOrEmpty(source.GearContributionSummaryText) ? string.Empty : source.GearContributionSummaryText;
        copy.GearAffixLiteSummaryText = string.IsNullOrEmpty(source.GearAffixLiteSummaryText) ? string.Empty : source.GearAffixLiteSummaryText;
        copy.LevelBandSummaryText = string.IsNullOrEmpty(source.LevelBandSummaryText) ? string.Empty : source.LevelBandSummaryText;
        copy.PassiveHintText = string.IsNullOrEmpty(source.PassiveHintText) ? string.Empty : source.PassiveHintText;
        copy.BattleLabelHint = string.IsNullOrEmpty(source.BattleLabelHint) ? string.Empty : source.BattleLabelHint;
        copy.SummaryText = string.IsNullOrEmpty(source.SummaryText) ? string.Empty : source.SummaryText;
        return copy;
    }

    private PrototypeRpgDerivedStatSummary CopyRpgDerivedStatSummary(PrototypeRpgDerivedStatSummary source)
    {
        PrototypeRpgDerivedStatSummary copy = new PrototypeRpgDerivedStatSummary();
        if (source == null)
        {
            return copy;
        }

        copy.Level = Mathf.Max(1, source.Level);
        copy.MaxHp = Mathf.Max(1, source.MaxHp);
        copy.Attack = Mathf.Max(1, source.Attack);
        copy.Defense = Mathf.Max(0, source.Defense);
        copy.Speed = Mathf.Max(0, source.Speed);
        copy.PowerScore = Mathf.Max(0, source.PowerScore);
        copy.SkillPowerBonus = Mathf.Max(0, source.SkillPowerBonus);
        copy.JobSpecializationKey = string.IsNullOrEmpty(source.JobSpecializationKey) ? string.Empty : source.JobSpecializationKey;
        copy.JobSpecializationSummaryText = string.IsNullOrEmpty(source.JobSpecializationSummaryText) ? string.Empty : source.JobSpecializationSummaryText;
        copy.PassiveSkillId = string.IsNullOrEmpty(source.PassiveSkillId) ? string.Empty : source.PassiveSkillId;
        copy.PassiveSkillSlotSummaryText = string.IsNullOrEmpty(source.PassiveSkillSlotSummaryText) ? string.Empty : source.PassiveSkillSlotSummaryText;
        copy.RuntimeSynergySummaryText = string.IsNullOrEmpty(source.RuntimeSynergySummaryText) ? string.Empty : source.RuntimeSynergySummaryText;
        copy.EquipmentSummaryText = string.IsNullOrEmpty(source.EquipmentSummaryText) ? string.Empty : source.EquipmentSummaryText;
        copy.GearContributionSummaryText = string.IsNullOrEmpty(source.GearContributionSummaryText) ? string.Empty : source.GearContributionSummaryText;
        copy.GearAffixLiteSummaryText = string.IsNullOrEmpty(source.GearAffixLiteSummaryText) ? string.Empty : source.GearAffixLiteSummaryText;
        copy.LevelBandSummaryText = string.IsNullOrEmpty(source.LevelBandSummaryText) ? string.Empty : source.LevelBandSummaryText;
        copy.PassiveHintText = string.IsNullOrEmpty(source.PassiveHintText) ? string.Empty : source.PassiveHintText;
        copy.BattleLabelHint = string.IsNullOrEmpty(source.BattleLabelHint) ? string.Empty : source.BattleLabelHint;
        copy.SummaryText = string.IsNullOrEmpty(source.SummaryText) ? string.Empty : source.SummaryText;
        return copy;
    }

    private PrototypeRpgInventoryRuntimeState CopyRpgInventoryRuntimeState(PrototypeRpgInventoryRuntimeState source)
    {
        PrototypeRpgInventoryRuntimeState copy = new PrototypeRpgInventoryRuntimeState();
        if (source == null)
        {
            return copy;
        }

        copy.PartyId = string.IsNullOrEmpty(source.PartyId) ? string.Empty : source.PartyId;
        copy.DisplayName = string.IsNullOrEmpty(source.DisplayName) ? string.Empty : source.DisplayName;
        copy.LastRunIdentity = string.IsNullOrEmpty(source.LastRunIdentity) ? string.Empty : source.LastRunIdentity;
        copy.TotalConsumableCount = Mathf.Max(0, source.TotalConsumableCount);
        copy.TotalEquippedLoadoutCount = Mathf.Max(0, source.TotalEquippedLoadoutCount);
        copy.TotalCarryRewardAmount = Mathf.Max(0, source.TotalCarryRewardAmount);
        copy.ConsumablesUsedThisRun = Mathf.Max(0, source.ConsumablesUsedThisRun);
        copy.DepletedConsumableCount = Mathf.Max(0, source.DepletedConsumableCount);
        copy.EquipmentSummaryText = string.IsNullOrEmpty(source.EquipmentSummaryText) ? string.Empty : source.EquipmentSummaryText;
        copy.InventorySummaryText = string.IsNullOrEmpty(source.InventorySummaryText) ? string.Empty : source.InventorySummaryText;
        copy.BattleConsumableSlotSummaryText = string.IsNullOrEmpty(source.BattleConsumableSlotSummaryText) ? string.Empty : source.BattleConsumableSlotSummaryText;
        copy.LastConsumableUseSummaryText = string.IsNullOrEmpty(source.LastConsumableUseSummaryText) ? string.Empty : source.LastConsumableUseSummaryText;
        copy.ConsumedThisRunSummaryText = string.IsNullOrEmpty(source.ConsumedThisRunSummaryText) ? string.Empty : source.ConsumedThisRunSummaryText;
        copy.CarryRewardSummaryText = string.IsNullOrEmpty(source.CarryRewardSummaryText) ? string.Empty : source.CarryRewardSummaryText;
        copy.CarryoverPolicySummaryText = string.IsNullOrEmpty(source.CarryoverPolicySummaryText) ? string.Empty : source.CarryoverPolicySummaryText;
        copy.ReplenishSummaryText = string.IsNullOrEmpty(source.ReplenishSummaryText) ? string.Empty : source.ReplenishSummaryText;
        copy.DepletedSummaryText = string.IsNullOrEmpty(source.DepletedSummaryText) ? string.Empty : source.DepletedSummaryText;
        copy.NextRunCarrySummaryText = string.IsNullOrEmpty(source.NextRunCarrySummaryText) ? string.Empty : source.NextRunCarrySummaryText;
        copy.LastResolvedConsumableId = string.IsNullOrEmpty(source.LastResolvedConsumableId) ? string.Empty : source.LastResolvedConsumableId;
        PrototypeRpgEquippedLoadoutEntry[] sourceEquippedLoadouts = source.EquippedLoadouts ?? Array.Empty<PrototypeRpgEquippedLoadoutEntry>();
        PrototypeRpgEquippedLoadoutEntry[] copiedEquippedLoadouts = new PrototypeRpgEquippedLoadoutEntry[sourceEquippedLoadouts.Length];
        for (int i = 0; i < sourceEquippedLoadouts.Length; i++) { copiedEquippedLoadouts[i] = CopyRpgEquippedLoadoutEntry(sourceEquippedLoadouts[i]); }
        PrototypeRpgInventoryEntry[] sourceEntries = source.Consumables ?? Array.Empty<PrototypeRpgInventoryEntry>();
        PrototypeRpgInventoryEntry[] copiedEntries = new PrototypeRpgInventoryEntry[sourceEntries.Length];
        for (int i = 0; i < sourceEntries.Length; i++)
        {
            PrototypeRpgInventoryEntry entry = sourceEntries[i] ?? new PrototypeRpgInventoryEntry();
            copiedEntries[i] = new PrototypeRpgInventoryEntry { ItemId = string.IsNullOrEmpty(entry.ItemId) ? string.Empty : entry.ItemId, DisplayName = string.IsNullOrEmpty(entry.DisplayName) ? string.Empty : entry.DisplayName, Quantity = Mathf.Max(0, entry.Quantity), SourceKey = string.IsNullOrEmpty(entry.SourceKey) ? string.Empty : entry.SourceKey, SummaryText = string.IsNullOrEmpty(entry.SummaryText) ? string.Empty : entry.SummaryText };
        }
        PrototypeRpgRewardCarryEntry[] sourceCarryEntries = source.CarryEntries ?? Array.Empty<PrototypeRpgRewardCarryEntry>();
        PrototypeRpgRewardCarryEntry[] copiedCarryEntries = new PrototypeRpgRewardCarryEntry[sourceCarryEntries.Length];
        for (int i = 0; i < sourceCarryEntries.Length; i++)
        {
            PrototypeRpgRewardCarryEntry entry = sourceCarryEntries[i] ?? new PrototypeRpgRewardCarryEntry();
            copiedCarryEntries[i] = new PrototypeRpgRewardCarryEntry { ResourceId = string.IsNullOrEmpty(entry.ResourceId) ? string.Empty : entry.ResourceId, ResourceLabel = string.IsNullOrEmpty(entry.ResourceLabel) ? string.Empty : entry.ResourceLabel, Amount = Mathf.Max(0, entry.Amount), SourceKey = string.IsNullOrEmpty(entry.SourceKey) ? string.Empty : entry.SourceKey, RunIdentity = string.IsNullOrEmpty(entry.RunIdentity) ? string.Empty : entry.RunIdentity, SummaryText = string.IsNullOrEmpty(entry.SummaryText) ? string.Empty : entry.SummaryText };
        }
        PrototypeRpgBattleConsumableSlot[] sourceSlots = source.BattleConsumableSlots ?? Array.Empty<PrototypeRpgBattleConsumableSlot>();
        PrototypeRpgBattleConsumableSlot[] copiedSlots = new PrototypeRpgBattleConsumableSlot[sourceSlots.Length];
        for (int i = 0; i < sourceSlots.Length; i++) { copiedSlots[i] = CopyRpgBattleConsumableSlot(sourceSlots[i]); }
        copy.EquippedLoadouts = copiedEquippedLoadouts;
        copy.Consumables = copiedEntries;
        copy.CarryEntries = copiedCarryEntries;
        copy.BattleConsumableSlots = copiedSlots;
        return copy;
    }

    private PrototypeRpgEquippedLoadoutEntry CopyRpgEquippedLoadoutEntry(PrototypeRpgEquippedLoadoutEntry source)
    {
        PrototypeRpgEquippedLoadoutEntry copy = new PrototypeRpgEquippedLoadoutEntry();
        if (source == null)
        {
            return copy;
        }

        copy.MemberId = string.IsNullOrEmpty(source.MemberId) ? string.Empty : source.MemberId;
        copy.MemberDisplayName = string.IsNullOrEmpty(source.MemberDisplayName) ? string.Empty : source.MemberDisplayName;
        copy.LoadoutId = string.IsNullOrEmpty(source.LoadoutId) ? string.Empty : source.LoadoutId;
        copy.DisplayName = string.IsNullOrEmpty(source.DisplayName) ? string.Empty : source.DisplayName;
        copy.SlotKey = string.IsNullOrEmpty(source.SlotKey) ? string.Empty : source.SlotKey;
        copy.MaxHpDelta = source.MaxHpDelta;
        copy.AttackDelta = source.AttackDelta;
        copy.DefenseDelta = source.DefenseDelta;
        copy.SpeedDelta = source.SpeedDelta;
        copy.PassiveHintText = string.IsNullOrEmpty(source.PassiveHintText) ? string.Empty : source.PassiveHintText;
        copy.BattleLabelHint = string.IsNullOrEmpty(source.BattleLabelHint) ? string.Empty : source.BattleLabelHint;
        copy.SummaryText = string.IsNullOrEmpty(source.SummaryText) ? string.Empty : source.SummaryText;
        copy.GearContributionSummaryText = string.IsNullOrEmpty(source.GearContributionSummaryText) ? string.Empty : source.GearContributionSummaryText;
        return copy;
    }
    private PrototypeRpgBattleConsumableSlot CopyRpgBattleConsumableSlot(PrototypeRpgBattleConsumableSlot source)
    {
        PrototypeRpgBattleConsumableSlot copy = new PrototypeRpgBattleConsumableSlot();
        if (source == null)
        {
            return copy;
        }

        copy.SlotKey = string.IsNullOrEmpty(source.SlotKey) ? string.Empty : source.SlotKey;
        copy.ItemId = string.IsNullOrEmpty(source.ItemId) ? string.Empty : source.ItemId;
        copy.DisplayName = string.IsNullOrEmpty(source.DisplayName) ? string.Empty : source.DisplayName;
        copy.Quantity = Mathf.Max(0, source.Quantity);
        copy.IsAvailable = source.IsAvailable;
        copy.EffectType = string.IsNullOrEmpty(source.EffectType) ? string.Empty : source.EffectType;
        copy.TargetKind = string.IsNullOrEmpty(source.TargetKind) ? string.Empty : source.TargetKind;
        copy.PowerValue = Mathf.Max(0, source.PowerValue);
        copy.RecommendedLevel = Mathf.Max(1, source.RecommendedLevel);
        copy.SummaryText = string.IsNullOrEmpty(source.SummaryText) ? string.Empty : source.SummaryText;
        return copy;
    }
}







