using System.Collections.Generic;
using UnityEngine;

public sealed partial class StaticPlaceholderWorldView
{
    private const string RpgSkillResourceLabel = "Focus";

    public string CurrentPartySkillLoadoutSummaryText => string.IsNullOrEmpty(BuildRpgPartySkillLoadoutSummaryText()) ? "None" : BuildRpgPartySkillLoadoutSummaryText();
    public string CurrentPartyActiveSkillSlotSummaryText => string.IsNullOrEmpty(BuildRpgPartyActiveSkillSlotSummaryText()) ? "None" : BuildRpgPartyActiveSkillSlotSummaryText();
    public string CurrentPartySkillCooldownSummaryText => string.IsNullOrEmpty(BuildRpgPartySkillCooldownSummaryText()) ? "None" : BuildRpgPartySkillCooldownSummaryText();
    public string CurrentPartySkillResourceSummaryText => string.IsNullOrEmpty(BuildRpgPartySkillResourceSummaryText()) ? "None" : BuildRpgPartySkillResourceSummaryText();
    public string CurrentPartyActionEconomySummaryText => string.IsNullOrEmpty(BuildRpgPartyActionEconomySummaryText()) ? "None" : BuildRpgPartyActionEconomySummaryText();
    public string CurrentNextRunActiveSkillPreviewText => string.IsNullOrEmpty(BuildRpgNextRunActiveSkillPreviewText()) ? "None" : BuildRpgNextRunActiveSkillPreviewText();
    public string CurrentBattleConsumableSlotSummaryText => string.IsNullOrEmpty(_sessionRpgInventoryRuntimeState.BattleConsumableSlotSummaryText) ? BuildBattleConsumableSlotSummaryTextFromResolvedSlots(_sessionRpgInventoryRuntimeState) : _sessionRpgInventoryRuntimeState.BattleConsumableSlotSummaryText;
    public string LastRunExperienceGainSummaryText => string.IsNullOrEmpty(_latestRpgRunResultSnapshot.ExperienceGainSummaryText) ? "None" : _latestRpgRunResultSnapshot.ExperienceGainSummaryText;
    public string LastRunLevelUpPreviewSummaryText => string.IsNullOrEmpty(_latestRpgRunResultSnapshot.LevelUpPreviewSummaryText) ? "None" : _latestRpgRunResultSnapshot.LevelUpPreviewSummaryText;
    public string LastRunLevelUpApplyReadySummaryText => string.IsNullOrEmpty(_latestRpgRunResultSnapshot.LevelUpApplyReadySummaryText) ? "None" : _latestRpgRunResultSnapshot.LevelUpApplyReadySummaryText;
    public string LastRunActualLevelApplySummaryText => string.IsNullOrEmpty(_latestRpgRunResultSnapshot.ActualLevelApplySummaryText) ? "None" : _latestRpgRunResultSnapshot.ActualLevelApplySummaryText;
    public string LastRunDerivedStatHydrateSummaryText => string.IsNullOrEmpty(_latestRpgRunResultSnapshot.DerivedStatHydrateSummaryText) ? "None" : _latestRpgRunResultSnapshot.DerivedStatHydrateSummaryText;
    public string LastRunNextRunStatProjectionSummaryText => string.IsNullOrEmpty(_latestRpgRunResultSnapshot.NextRunStatProjectionSummaryText) ? "None" : _latestRpgRunResultSnapshot.NextRunStatProjectionSummaryText;
    public string LastRunConsumableCarrySummaryText => string.IsNullOrEmpty(_latestRpgRunResultSnapshot.ConsumableCarrySummaryText) ? "None" : _latestRpgRunResultSnapshot.ConsumableCarrySummaryText;
    public string LastRunConsumableCarryoverPolicySummaryText => string.IsNullOrEmpty(_latestRpgRunResultSnapshot.ConsumableCarryoverPolicySummaryText) ? "None" : _latestRpgRunResultSnapshot.ConsumableCarryoverPolicySummaryText;
    private PrototypeRpgPartyMemberDefinition GetRpgPartyDefinitionMemberById(PrototypeRpgPartyDefinition partyDefinition, string memberId)
    {
        if (partyDefinition == null || partyDefinition.Members == null || string.IsNullOrEmpty(memberId))
        {
            return null;
        }

        for (int i = 0; i < partyDefinition.Members.Length; i++)
        {
            PrototypeRpgPartyMemberDefinition member = partyDefinition.Members[i];
            if (member != null && member.MemberId == memberId)
            {
                return member;
            }
        }

        return null;
    }

    private string ResolveRpgSkillEffectBucketKey(PrototypeRpgSkillDefinition skillDefinition)
    {
        if (skillDefinition == null)
        {
            return string.Empty;
        }

        if (!string.IsNullOrEmpty(skillDefinition.EffectType) && skillDefinition.EffectType.Contains("heal"))
        {
            return "support";
        }

        if (!string.IsNullOrEmpty(skillDefinition.EffectType) && skillDefinition.EffectType.Contains("finisher"))
        {
            return "finisher";
        }

        if (skillDefinition.TargetKind == "all_enemies")
        {
            return "aoe";
        }

        if (skillDefinition.TargetKind == "self")
        {
            return "self_setup";
        }

        return "core";
    }

    private string ResolveRpgSkillApplyModeKey(PrototypeRpgSkillDefinition skillDefinition)
    {
        if (skillDefinition == null)
        {
            return string.Empty;
        }

        if (skillDefinition.EffectType == "finisher_damage")
        {
            return "finisher_damage";
        }

        if (skillDefinition.TargetKind == "all_enemies")
        {
            return "splash_damage_or_all_enemies";
        }

        if (skillDefinition.TargetKind == "all_allies")
        {
            return "party_heal";
        }

        if (skillDefinition.TargetKind == "self")
        {
            return "self_recover_or_guarded_strike";
        }

        return "direct_damage";
    }

    private string ResolveRpgSkillUseFlowKey(PrototypeRpgSkillDefinition skillDefinition)
    {
        if (skillDefinition == null)
        {
            return string.Empty;
        }

        if (skillDefinition.TargetKind == "all_enemies")
        {
            return "burst";
        }

        if (skillDefinition.TargetKind == "all_allies")
        {
            return "sustain";
        }

        if (skillDefinition.TargetKind == "self")
        {
            return "recovery";
        }

        return skillDefinition.EffectType == "finisher_damage" ? "execution" : "standard";
    }

    private string ResolveRpgSkillContributionHintKey(PrototypeRpgSkillDefinition skillDefinition)
    {
        if (skillDefinition == null)
        {
            return string.Empty;
        }

        if (skillDefinition.TargetKind == "all_allies")
        {
            return "support";
        }

        if (skillDefinition.TargetKind == "all_enemies")
        {
            return "aoe";
        }

        if (skillDefinition.TargetKind == "self")
        {
            return "survival";
        }

        return "offense";
    }

    private string BuildRpgSkillSlotLabel(PrototypeRpgSkillDefinition skillDefinition, string slotKey)
    {
        string baseLabel = skillDefinition != null && !string.IsNullOrEmpty(skillDefinition.DisplayName)
            ? skillDefinition.DisplayName
            : "Skill";
        switch (slotKey)
        {
            case "secondary":
                if (skillDefinition != null && skillDefinition.EffectType == "setup")
                {
                    return baseLabel + " Setup";
                }

                if (skillDefinition != null && skillDefinition.TargetKind == "self")
                {
                    return baseLabel + " Guard";
                }

                return baseLabel;
            case "finisher":
                return baseLabel + " Finisher";
            default:
                return baseLabel;
        }
    }

    private int ResolveRpgSkillSlotPower(DungeonPartyMemberRuntimeData member, PrototypeRpgSkillDefinition skillDefinition, string slotKey)
    {
        int safePower = skillDefinition != null && skillDefinition.PowerValue > 0
            ? skillDefinition.PowerValue
            : member != null && member.SkillPower > 0
                ? member.SkillPower
                : 1;
        int resolvedPower = member != null ? Mathf.Max(safePower, member.SkillPower) : safePower;
        if (slotKey == "secondary")
        {
            return skillDefinition != null && skillDefinition.TargetKind == "self"
                ? resolvedPower
                : resolvedPower + 1;
        }

        if (slotKey == "finisher")
        {
            return resolvedPower + (skillDefinition != null && (skillDefinition.EffectType == "finisher_damage" || skillDefinition.TargetKind == "all_enemies" || skillDefinition.TargetKind == "all_allies") ? 2 : 1);
        }

        return resolvedPower;
    }

    private int ResolveRpgSkillSlotCooldownTurns(PrototypeRpgSkillDefinition skillDefinition, string slotKey)
    {
        if (slotKey == "primary")
        {
            return 0;
        }

        if (slotKey == "finisher" || (skillDefinition != null && (skillDefinition.TargetKind == "all_enemies" || skillDefinition.TargetKind == "all_allies")))
        {
            return 2;
        }

        return 1;
    }

    private int ResolveRpgSkillSlotMaxCharges(PrototypeRpgSkillDefinition skillDefinition, string slotKey)
    {
        if (slotKey == "primary")
        {
            return 0;
        }

        if (slotKey == "finisher")
        {
            return 1;
        }

        return skillDefinition != null && skillDefinition.TargetKind == "all_enemies" ? 1 : 2;
    }

    private int ResolveRpgSkillSlotResourceCost(PrototypeRpgSkillDefinition skillDefinition, string slotKey)
    {
        if (slotKey == "primary")
        {
            return 0;
        }

        if (slotKey == "finisher")
        {
            return 2;
        }

        return skillDefinition != null && (skillDefinition.TargetKind == "all_enemies" || skillDefinition.TargetKind == "all_allies") ? 2 : 1;
    }

    private int ResolveRpgSkillResourceCap(DungeonPartyMemberRuntimeData member)
    {
        int cap = 3;
        if (member != null && (member.RoleTag == "mage" || member.RoleTag == "cleric"))
        {
            cap += 1;
        }

        if (member != null && member.Level >= 3)
        {
            cap += 1;
        }

        return Mathf.Clamp(cap, 1, 5);
    }

    private string BuildRpgStaticSkillSlotSummaryText(PrototypeRpgRuntimeSkillSlotData slot)
    {
        if (slot == null)
        {
            return string.Empty;
        }

        string chargeText = slot.MaxEncounterCharges > 0
            ? "Use " + slot.MaxEncounterCharges
            : "Repeat";
        string cooldownText = slot.CooldownTurns > 0
            ? "CD " + slot.CooldownTurns
            : "Ready";
        return slot.SkillLabel + " | " + chargeText + " | " + RpgSkillResourceLabel + " " + slot.ResourceCost + " | " + cooldownText;
    }

    private string BuildRpgSkillAvailabilitySummaryText(PrototypeRpgActiveSkillSlotState slot)
    {
        if (slot == null)
        {
            return string.Empty;
        }

        if (slot.IsAvailable)
        {
            return slot.MaxEncounterCharges > 0
                ? "Ready | Uses " + slot.EncounterChargesRemaining + "/" + slot.MaxEncounterCharges
                : "Ready | Repeat";
        }

        if (slot.CooldownRemaining > 0)
        {
            return "Cooldown " + slot.CooldownRemaining + " turn(s)";
        }

        if (slot.MaxEncounterCharges > 0 && slot.EncounterChargesRemaining <= 0)
        {
            return "Encounter charge spent";
        }

        return RpgSkillResourceLabel + " short";
    }

    private string BuildRpgRuntimeSkillSlotStateSummaryText(PrototypeRpgActiveSkillSlotState slot)
    {
        if (slot == null)
        {
            return string.Empty;
        }

        string chargeText = slot.MaxEncounterCharges > 0
            ? "Uses " + slot.EncounterChargesRemaining + "/" + slot.MaxEncounterCharges
            : "Repeat";
        string cooldownText = slot.CooldownRemaining > 0
            ? "CD " + slot.CooldownRemaining
            : slot.CooldownTurns > 0
                ? "CD ready"
                : "Ready";
        return slot.SkillLabel + " | " + chargeText + " | " + RpgSkillResourceLabel + " " + slot.ResourceCost + " | " + cooldownText;
    }

    private void AddRpgRuntimeSkillSlot(List<PrototypeRpgRuntimeSkillSlotData> slots, DungeonPartyMemberRuntimeData member, PrototypeRpgSkillLoadoutDefinition loadoutDefinition, PrototypeRpgSkillDefinition skillDefinition, string slotKey)
    {
        if (slots == null || member == null || skillDefinition == null || string.IsNullOrEmpty(slotKey))
        {
            return;
        }

        int powerValue = ResolveRpgSkillSlotPower(member, skillDefinition, slotKey);
        int cooldownTurns = ResolveRpgSkillSlotCooldownTurns(skillDefinition, slotKey);
        int maxEncounterCharges = ResolveRpgSkillSlotMaxCharges(skillDefinition, slotKey);
        int resourceCost = ResolveRpgSkillSlotResourceCost(skillDefinition, slotKey);
        slots.Add(new PrototypeRpgRuntimeSkillSlotData(
            slotKey,
            skillDefinition.SkillId,
            BuildRpgSkillSlotLabel(skillDefinition, slotKey),
            string.IsNullOrEmpty(skillDefinition.SegmentKey) ? (loadoutDefinition != null ? loadoutDefinition.SegmentKey : string.Empty) : skillDefinition.SegmentKey,
            ResolveRpgSkillEffectBucketKey(skillDefinition),
            skillDefinition.TargetKind,
            skillDefinition.EffectType,
            ResolveRpgSkillApplyModeKey(skillDefinition),
            ResolveRpgSkillUseFlowKey(skillDefinition),
            ResolveRpgSkillContributionHintKey(skillDefinition),
            powerValue,
            cooldownTurns,
            maxEncounterCharges,
            resourceCost,
            1,
            Mathf.Max(1, member.Level),
            skillDefinition.ShortText));
    }

    private PrototypeRpgResolvedSkillLoadout BuildResolvedRpgSkillLoadout(DungeonPartyMemberRuntimeData member)
    {
        PrototypeRpgResolvedSkillLoadout resolved = new PrototypeRpgResolvedSkillLoadout();
        if (member == null)
        {
            return resolved;
        }

        PrototypeRpgSkillLoadoutDefinition loadoutDefinition = PrototypeRpgSkillLoadoutCatalog.ResolveDefinition(member.SkillLoadoutId, member.RoleTag, member.DefaultSkillId);
        PrototypeRpgSkillDefinition primarySkill = PrototypeRpgSkillCatalog.ResolveDefinition(loadoutDefinition != null ? loadoutDefinition.PrimarySkillId : member.DefaultSkillId, member.RoleTag);
        PrototypeRpgSkillDefinition secondarySkill = PrototypeRpgSkillCatalog.ResolveDefinition(loadoutDefinition != null ? loadoutDefinition.SecondarySkillId : string.Empty, member.RoleTag);
        PrototypeRpgSkillDefinition finisherSkill = PrototypeRpgSkillCatalog.ResolveDefinition(loadoutDefinition != null && !string.IsNullOrEmpty(loadoutDefinition.FinisherSkillId) ? loadoutDefinition.FinisherSkillId : primarySkill != null ? primarySkill.SkillId : member.DefaultSkillId, member.RoleTag);
        List<PrototypeRpgRuntimeSkillSlotData> slots = new List<PrototypeRpgRuntimeSkillSlotData>();

        AddRpgRuntimeSkillSlot(slots, member, loadoutDefinition, primarySkill, "primary");
        AddRpgRuntimeSkillSlot(slots, member, loadoutDefinition, secondarySkill, "secondary");
        AddRpgRuntimeSkillSlot(slots, member, loadoutDefinition, finisherSkill, "finisher");

        resolved.MemberId = member.MemberId;
        resolved.RoleTag = member.RoleTag;
        resolved.LoadoutId = loadoutDefinition != null ? loadoutDefinition.LoadoutId : string.Empty;
        resolved.LoadoutLabel = loadoutDefinition != null ? loadoutDefinition.DisplayName : string.Empty;
        resolved.SegmentKey = loadoutDefinition != null ? loadoutDefinition.SegmentKey : string.Empty;
        resolved.PrimarySkillId = primarySkill != null ? primarySkill.SkillId : member.DefaultSkillId;
        resolved.PrimarySkillLabel = primarySkill != null ? primarySkill.DisplayName : member.SkillName;
        PrototypeRpgRuntimeSkillSlotData primarySlot = slots.Count > 0 ? slots[0] : null;
        string effectSummaryText = primarySlot != null
            ? GetBattleUiEffectText(primarySlot.ApplyModeKey, primarySlot.PowerValue)
            : string.Empty;
        resolved.SummaryText = loadoutDefinition != null
            ? loadoutDefinition.DisplayName + " | " + resolved.PrimarySkillLabel + (string.IsNullOrEmpty(effectSummaryText) ? string.Empty : " | " + effectSummaryText)
            : resolved.PrimarySkillLabel + (string.IsNullOrEmpty(effectSummaryText) ? string.Empty : " | " + effectSummaryText);
        resolved.SkillSlots = slots.ToArray();
        resolved.ActiveSkillSlotSummaryText = slots.Count <= 0
            ? string.Empty
            : "Actives: " + string.Join(" | ", BuildRpgStaticSkillSlotSummaryArray(resolved.SkillSlots));
        resolved.ResourceSummaryText = "Skill resource: " + RpgSkillResourceLabel + " " + ResolveRpgSkillResourceCap(member) + " cap | Attack restores 1.";
        resolved.CooldownSummaryText = slots.Count <= 0
            ? string.Empty
            : "Cooldown track: " + string.Join(" | ", BuildRpgStaticCooldownSummaryArray(resolved.SkillSlots));
        resolved.ActionEconomySummaryText = "Turn economy: 1 action per turn | Active skills spend " + RpgSkillResourceLabel + " and trigger cooldowns.";
        return resolved;
    }

    private string[] BuildRpgStaticSkillSlotSummaryArray(PrototypeRpgRuntimeSkillSlotData[] skillSlots)
    {
        if (skillSlots == null || skillSlots.Length <= 0)
        {
            return System.Array.Empty<string>();
        }

        List<string> parts = new List<string>();
        for (int i = 0; i < skillSlots.Length; i++)
        {
            string summaryText = BuildRpgStaticSkillSlotSummaryText(skillSlots[i]);
            if (!string.IsNullOrEmpty(summaryText))
            {
                parts.Add(summaryText);
            }
        }

        return parts.Count > 0 ? parts.ToArray() : System.Array.Empty<string>();
    }

    private string[] BuildRpgStaticCooldownSummaryArray(PrototypeRpgRuntimeSkillSlotData[] skillSlots)
    {
        if (skillSlots == null || skillSlots.Length <= 0)
        {
            return System.Array.Empty<string>();
        }

        List<string> parts = new List<string>();
        for (int i = 0; i < skillSlots.Length; i++)
        {
            PrototypeRpgRuntimeSkillSlotData slot = skillSlots[i];
            if (slot == null)
            {
                continue;
            }

            parts.Add(slot.SkillLabel + " " + (slot.CooldownTurns > 0 ? "CD " + slot.CooldownTurns : "ready"));
        }

        return parts.Count > 0 ? parts.ToArray() : System.Array.Empty<string>();
    }

    private void InitializeRpgBattleSkillRuntimeState(DungeonPartyMemberRuntimeData member, bool forceReset)
    {
        if (member == null || member.RuntimeState == null)
        {
            return;
        }

        PrototypeRpgResolvedSkillLoadout resolvedLoadout = BuildResolvedRpgSkillLoadout(member);
        PrototypeRpgRuntimeSkillSlotData[] skillSlots = resolvedLoadout.SkillSlots ?? System.Array.Empty<PrototypeRpgRuntimeSkillSlotData>();
        PrototypeRpgActiveSkillSlotState[] currentSlots = member.ActiveSkillSlots ?? System.Array.Empty<PrototypeRpgActiveSkillSlotState>();
        bool needsReset = forceReset || currentSlots.Length != skillSlots.Length;
        if (!needsReset)
        {
            for (int i = 0; i < currentSlots.Length; i++)
            {
                PrototypeRpgActiveSkillSlotState slot = currentSlots[i];
                PrototypeRpgRuntimeSkillSlotData definition = skillSlots[i];
                if (slot == null || definition == null || slot.SkillId != definition.SkillId || slot.SlotKey != definition.SlotKey)
                {
                    needsReset = true;
                    break;
                }
            }
        }

        if (needsReset)
        {
            PrototypeRpgActiveSkillSlotState[] activeSlots = new PrototypeRpgActiveSkillSlotState[skillSlots.Length];
            for (int i = 0; i < skillSlots.Length; i++)
            {
                PrototypeRpgRuntimeSkillSlotData definition = skillSlots[i];
                if (definition == null)
                {
                    continue;
                }

                PrototypeRpgActiveSkillSlotState slot = new PrototypeRpgActiveSkillSlotState();
                slot.SlotKey = definition.SlotKey;
                slot.SkillId = definition.SkillId;
                slot.SkillLabel = definition.SkillLabel;
                slot.SegmentKey = definition.SegmentKey;
                slot.EffectBucketKey = definition.EffectBucketKey;
                slot.TargetKind = definition.TargetKind;
                slot.EffectType = definition.EffectType;
                slot.ApplyModeKey = definition.ApplyModeKey;
                slot.UseFlowKey = definition.UseFlowKey;
                slot.ContributionHintKey = definition.ContributionHintKey;
                slot.PowerValue = definition.PowerValue;
                slot.CooldownTurns = definition.CooldownTurns;
                slot.CooldownRemaining = 0;
                slot.MaxEncounterCharges = definition.MaxEncounterCharges;
                slot.EncounterChargesRemaining = definition.MaxEncounterCharges > 0 ? definition.MaxEncounterCharges : 0;
                slot.ResourceCost = definition.ResourceCost;
                slot.ActionPointCost = definition.ActionPointCost;
                slot.RecommendedLevel = definition.RecommendedLevel;
                slot.SummaryText = definition.SummaryText;
                activeSlots[i] = slot;
            }

            int maxResource = ResolveRpgSkillResourceCap(member);
            member.RuntimeState.SetActiveSkillRuntimeState(activeSlots, maxResource, maxResource);
            member.SelectedActiveSkillSlotIndex = skillSlots.Length > 0 ? 0 : -1;
        }

        RefreshRpgBattleSkillRuntimeState(member);
    }

    private void RefreshRpgBattleSkillRuntimeState(DungeonPartyMemberRuntimeData member)
    {
        if (member == null || member.RuntimeState == null)
        {
            return;
        }

        PrototypeRpgActiveSkillSlotState[] slots = member.ActiveSkillSlots ?? System.Array.Empty<PrototypeRpgActiveSkillSlotState>();
        for (int i = 0; i < slots.Length; i++)
        {
            PrototypeRpgActiveSkillSlotState slot = slots[i];
            if (slot == null)
            {
                continue;
            }

            bool hasCharges = slot.MaxEncounterCharges <= 0 || slot.EncounterChargesRemaining > 0;
            bool hasResource = member.CurrentSkillResource >= slot.ResourceCost;
            bool offCooldown = slot.CooldownRemaining <= 0;
            slot.IsAvailable = hasCharges && hasResource && offCooldown;
            slot.AvailabilityStateKey = slot.IsAvailable
                ? "ready"
                : !offCooldown
                    ? "cooldown"
                    : !hasCharges
                        ? "depleted"
                        : "resource_short";
            slot.AvailabilitySummaryText = BuildRpgSkillAvailabilitySummaryText(slot);
            slot.SummaryText = BuildRpgRuntimeSkillSlotStateSummaryText(slot);
        }

        if (slots.Length <= 0)
        {
            member.SelectedActiveSkillSlotIndex = -1;
        }
        else if (member.SelectedActiveSkillSlotIndex < 0 || member.SelectedActiveSkillSlotIndex >= slots.Length)
        {
            member.SelectedActiveSkillSlotIndex = 0;
        }

        PrototypeRpgResolvedSkillLoadout resolvedLoadout = BuildResolvedRpgSkillLoadout(member);
        member.RuntimeState.SetActiveSkillSummaries(
            BuildRpgMemberActiveSkillSlotSummaryText(member),
            BuildRpgMemberSkillCooldownSummaryText(member),
            BuildRpgMemberSkillResourceSummaryText(member),
            BuildRpgMemberActionEconomySummaryText(member),
            BuildRpgMemberNextRunActiveSkillPreviewText(member, resolvedLoadout));
    }

    private void AdvanceRpgBattleSkillTurnStart(DungeonPartyMemberRuntimeData member)
    {
        InitializeRpgBattleSkillRuntimeState(member, false);
        PrototypeRpgActiveSkillSlotState[] slots = member != null ? (member.ActiveSkillSlots ?? System.Array.Empty<PrototypeRpgActiveSkillSlotState>()) : System.Array.Empty<PrototypeRpgActiveSkillSlotState>();
        for (int i = 0; i < slots.Length; i++)
        {
            PrototypeRpgActiveSkillSlotState slot = slots[i];
            if (slot != null && slot.CooldownRemaining > 0)
            {
                slot.CooldownRemaining = Mathf.Max(0, slot.CooldownRemaining - 1);
            }
        }

        RefreshRpgBattleSkillRuntimeState(member);
        SelectBestAvailableRpgBattleSkillSlot(member);
    }

    private void ResetRpgEncounterSkillRuntimeState(TestDungeonPartyData party)
    {
        if (party == null || party.Members == null)
        {
            return;
        }

        for (int i = 0; i < party.Members.Length; i++)
        {
            InitializeRpgBattleSkillRuntimeState(party.Members[i], true);
        }
    }

    private PrototypeRpgActiveSkillSlotState GetSelectedRpgActiveSkillSlot(DungeonPartyMemberRuntimeData member)
    {
        if (member == null)
        {
            return null;
        }

        InitializeRpgBattleSkillRuntimeState(member, false);
        PrototypeRpgActiveSkillSlotState[] slots = member.ActiveSkillSlots ?? System.Array.Empty<PrototypeRpgActiveSkillSlotState>();
        if (slots.Length <= 0 || member.SelectedActiveSkillSlotIndex < 0 || member.SelectedActiveSkillSlotIndex >= slots.Length)
        {
            return null;
        }

        return slots[member.SelectedActiveSkillSlotIndex];
    }

    private void SelectBestAvailableRpgBattleSkillSlot(DungeonPartyMemberRuntimeData member)
    {
        if (member == null)
        {
            return;
        }

        PrototypeRpgActiveSkillSlotState[] slots = member.ActiveSkillSlots ?? System.Array.Empty<PrototypeRpgActiveSkillSlotState>();
        if (slots.Length <= 0)
        {
            member.SelectedActiveSkillSlotIndex = -1;
            return;
        }

        if (member.SelectedActiveSkillSlotIndex >= 0 &&
            member.SelectedActiveSkillSlotIndex < slots.Length &&
            slots[member.SelectedActiveSkillSlotIndex] != null &&
            slots[member.SelectedActiveSkillSlotIndex].IsAvailable)
        {
            return;
        }

        for (int i = 0; i < slots.Length; i++)
        {
            if (slots[i] != null && slots[i].IsAvailable)
            {
                member.SelectedActiveSkillSlotIndex = i;
                return;
            }
        }

        member.SelectedActiveSkillSlotIndex = 0;
    }

    private bool TryCycleCurrentBattleSkillSlotSelection(int direction)
    {
        if (_dungeonRunState != DungeonRunState.Battle || _battleState != BattleState.PartyActionSelect)
        {
            return false;
        }

        DungeonPartyMemberRuntimeData member = GetCurrentActorMember();
        if (member == null)
        {
            return false;
        }

        InitializeRpgBattleSkillRuntimeState(member, false);
        PrototypeRpgActiveSkillSlotState[] slots = member.ActiveSkillSlots ?? System.Array.Empty<PrototypeRpgActiveSkillSlotState>();
        if (slots.Length <= 1)
        {
            return false;
        }

        int currentIndex = member.SelectedActiveSkillSlotIndex < 0 ? 0 : member.SelectedActiveSkillSlotIndex;
        int nextIndex = currentIndex;
        for (int i = 0; i < slots.Length; i++)
        {
            nextIndex = (nextIndex + direction + slots.Length) % slots.Length;
            if (slots[nextIndex] != null)
            {
                break;
            }
        }

        member.SelectedActiveSkillSlotIndex = nextIndex;
        RefreshRpgBattleSkillRuntimeState(member);
        PrototypeRpgActiveSkillSlotState selectedSlot = GetSelectedRpgActiveSkillSlot(member);
        if (selectedSlot != null)
        {
            SetBattleFeedbackText("Skill slot: " + selectedSlot.SkillLabel + " | " + selectedSlot.AvailabilitySummaryText + ".");
            RefreshSelectionPrompt();
            RefreshDungeonPresentation();
            return true;
        }

        return false;
    }

    private void GrantRpgBattleSkillResource(DungeonPartyMemberRuntimeData member, int amount)
    {
        if (member == null || member.RuntimeState == null || amount <= 0)
        {
            return;
        }

        InitializeRpgBattleSkillRuntimeState(member, false);
        member.RuntimeState.SetCurrentSkillResource(member.CurrentSkillResource + amount);
        RefreshRpgBattleSkillRuntimeState(member);
    }

    private void CommitRpgBattleSkillUse(DungeonPartyMemberRuntimeData member, PrototypeRpgSkillUseContext context)
    {
        if (member == null || member.RuntimeState == null || context == null || context.ResolvedSlotIndex < 0)
        {
            return;
        }

        PrototypeRpgActiveSkillSlotState[] slots = member.ActiveSkillSlots ?? System.Array.Empty<PrototypeRpgActiveSkillSlotState>();
        if (context.ResolvedSlotIndex >= slots.Length)
        {
            return;
        }

        PrototypeRpgActiveSkillSlotState slot = slots[context.ResolvedSlotIndex];
        if (slot == null)
        {
            return;
        }

        member.RuntimeState.SetCurrentSkillResource(Mathf.Max(0, member.CurrentSkillResource - Mathf.Max(0, slot.ResourceCost)));
        if (slot.MaxEncounterCharges > 0)
        {
            slot.EncounterChargesRemaining = Mathf.Max(0, slot.EncounterChargesRemaining - 1);
        }

        slot.CooldownRemaining = Mathf.Max(slot.CooldownRemaining, slot.CooldownTurns);
        RefreshRpgBattleSkillRuntimeState(member);
    }

    private string BuildRpgMemberActiveSkillSlotSummaryText(DungeonPartyMemberRuntimeData member)
    {
        if (member == null)
        {
            return string.Empty;
        }

        PrototypeRpgActiveSkillSlotState[] slots = member.ActiveSkillSlots ?? System.Array.Empty<PrototypeRpgActiveSkillSlotState>();
        if (slots.Length <= 0)
        {
            return string.Empty;
        }

        List<string> parts = new List<string>();
        for (int i = 0; i < slots.Length; i++)
        {
            PrototypeRpgActiveSkillSlotState slot = slots[i];
            if (slot != null && !string.IsNullOrEmpty(slot.SummaryText))
            {
                parts.Add(slot.SummaryText);
            }
        }

        return parts.Count <= 0 ? string.Empty : "Actives: " + string.Join(" | ", parts.ToArray());
    }

    private string BuildRpgMemberSkillCooldownSummaryText(DungeonPartyMemberRuntimeData member)
    {
        if (member == null)
        {
            return string.Empty;
        }

        PrototypeRpgActiveSkillSlotState[] slots = member.ActiveSkillSlots ?? System.Array.Empty<PrototypeRpgActiveSkillSlotState>();
        List<string> parts = new List<string>();
        for (int i = 0; i < slots.Length; i++)
        {
            PrototypeRpgActiveSkillSlotState slot = slots[i];
            if (slot != null)
            {
                parts.Add(slot.SkillLabel + " " + (slot.CooldownRemaining > 0 ? "CD " + slot.CooldownRemaining : "ready"));
            }
        }

        return parts.Count <= 0 ? string.Empty : "Cooldowns: " + string.Join(" | ", parts.ToArray());
    }

    private string BuildRpgMemberSkillResourceSummaryText(DungeonPartyMemberRuntimeData member)
    {
        if (member == null)
        {
            return string.Empty;
        }

        return RpgSkillResourceLabel + ": " + member.CurrentSkillResource + "/" + Mathf.Max(1, member.MaxSkillResource) + " | Attack restores 1.";
    }

    private string BuildRpgMemberActionEconomySummaryText(DungeonPartyMemberRuntimeData member)
    {
        if (member == null)
        {
            return string.Empty;
        }

        PrototypeRpgActiveSkillSlotState[] slots = member.ActiveSkillSlots ?? System.Array.Empty<PrototypeRpgActiveSkillSlotState>();
        PrototypeRpgActiveSkillSlotState selectedSlot = null;
        if (member.SelectedActiveSkillSlotIndex >= 0 && member.SelectedActiveSkillSlotIndex < slots.Length)
        {
            selectedSlot = slots[member.SelectedActiveSkillSlotIndex];
        }

        if (selectedSlot == null)
        {
            for (int i = 0; i < slots.Length; i++)
            {
                if (slots[i] != null)
                {
                    selectedSlot = slots[i];
                    break;
                }
            }
        }

        string selectedText = selectedSlot != null
            ? "Selected " + selectedSlot.SkillLabel + " costs " + selectedSlot.ResourceCost + " " + RpgSkillResourceLabel
            : "Selected skill pending";
        return "Action economy: 1 action per turn | " + selectedText + " | Cooldowns tick on this member's next turn.";
    }

    private string BuildRpgMemberNextRunActiveSkillPreviewText(DungeonPartyMemberRuntimeData member, PrototypeRpgResolvedSkillLoadout resolvedLoadout)
    {
        if (member == null)
        {
            return string.Empty;
        }

        PrototypeRpgRuntimeSkillSlotData[] skillSlots = resolvedLoadout != null ? (resolvedLoadout.SkillSlots ?? System.Array.Empty<PrototypeRpgRuntimeSkillSlotData>()) : System.Array.Empty<PrototypeRpgRuntimeSkillSlotData>();
        List<string> labels = new List<string>();
        for (int i = 0; i < skillSlots.Length; i++)
        {
            PrototypeRpgRuntimeSkillSlotData slot = skillSlots[i];
            if (slot != null && !string.IsNullOrEmpty(slot.SkillLabel))
            {
                labels.Add(slot.SkillLabel);
            }
        }

        return labels.Count <= 0 ? string.Empty : member.DisplayName + " next actives: " + string.Join(" / ", labels.ToArray());
    }

    private string BuildRpgPartyActiveSkillSlotSummaryText()
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

            InitializeRpgBattleSkillRuntimeState(member, false);
            if (!string.IsNullOrEmpty(member.ActiveSkillSlotSummaryText))
            {
                parts.Add(member.DisplayName + " " + member.ActiveSkillSlotSummaryText);
            }
        }

        return parts.Count <= 0 ? string.Empty : string.Join(" | ", parts.ToArray());
    }

    private string BuildRpgPartySkillCooldownSummaryText()
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

            InitializeRpgBattleSkillRuntimeState(member, false);
            if (!string.IsNullOrEmpty(member.SkillCooldownSummaryText))
            {
                parts.Add(member.DisplayName + " " + member.SkillCooldownSummaryText);
            }
        }

        return parts.Count <= 0 ? string.Empty : string.Join(" | ", parts.ToArray());
    }

    private string BuildRpgPartySkillResourceSummaryText()
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

            InitializeRpgBattleSkillRuntimeState(member, false);
            if (!string.IsNullOrEmpty(member.SkillResourceSummaryText))
            {
                parts.Add(member.DisplayName + " " + member.SkillResourceSummaryText);
            }
        }

        return parts.Count <= 0 ? string.Empty : string.Join(" | ", parts.ToArray());
    }

    private string BuildRpgPartyActionEconomySummaryText()
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

            InitializeRpgBattleSkillRuntimeState(member, false);
            if (!string.IsNullOrEmpty(member.ActionEconomySummaryText))
            {
                parts.Add(member.DisplayName + " " + member.ActionEconomySummaryText);
            }
        }

        return parts.Count <= 0 ? string.Empty : string.Join(" | ", parts.ToArray());
    }

    private string BuildRpgNextRunActiveSkillPreviewText()
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

            InitializeRpgBattleSkillRuntimeState(member, false);
            if (!string.IsNullOrEmpty(member.NextRunActiveSkillPreviewText))
            {
                parts.Add(member.NextRunActiveSkillPreviewText);
            }
        }

        return parts.Count <= 0 ? string.Empty : string.Join(" | ", parts.ToArray());
    }

    private PrototypeRpgSkillUseContext BuildRpgSkillUseContext(DungeonPartyMemberRuntimeData member, string selectedActionKey)
    {
        PrototypeRpgSkillUseContext context = new PrototypeRpgSkillUseContext();
        if (member == null)
        {
            return context;
        }

        InitializeRpgBattleSkillRuntimeState(member, false);
        PrototypeRpgResolvedSkillLoadout resolvedLoadout = BuildResolvedRpgSkillLoadout(member);
        PrototypeRpgActiveSkillSlotState selectedSlot = GetSelectedRpgActiveSkillSlot(member);
        PrototypeRpgRuntimeSkillSlotData fallbackSlot = resolvedLoadout.SkillSlots != null && resolvedLoadout.SkillSlots.Length > 0 ? resolvedLoadout.SkillSlots[0] : null;
        context.MemberId = member.MemberId;
        context.LoadoutId = resolvedLoadout.LoadoutId;
        context.SelectedActionKey = string.IsNullOrEmpty(selectedActionKey) ? string.Empty : selectedActionKey;
        context.ResolvedSlotIndex = member.SelectedActiveSkillSlotIndex;
        context.ResolvedSlotKey = selectedSlot != null ? selectedSlot.SlotKey : fallbackSlot != null ? fallbackSlot.SlotKey : string.Empty;
        context.ResolvedSkillId = selectedSlot != null ? selectedSlot.SkillId : fallbackSlot != null ? fallbackSlot.SkillId : member.DefaultSkillId;
        context.ResolvedSkillLabel = selectedSlot != null ? selectedSlot.SkillLabel : fallbackSlot != null ? fallbackSlot.SkillLabel : member.SkillName;
        context.SkillSegmentKey = selectedSlot != null ? selectedSlot.SegmentKey : fallbackSlot != null ? fallbackSlot.SegmentKey : string.Empty;
        context.TargetKind = selectedSlot != null ? selectedSlot.TargetKind : fallbackSlot != null ? fallbackSlot.TargetKind : string.Empty;
        context.EffectBucketKey = selectedSlot != null ? selectedSlot.EffectBucketKey : fallbackSlot != null ? fallbackSlot.EffectBucketKey : string.Empty;
        context.EffectType = selectedSlot != null ? selectedSlot.EffectType : fallbackSlot != null ? fallbackSlot.EffectType : string.Empty;
        context.ApplyModeKey = selectedSlot != null ? selectedSlot.ApplyModeKey : fallbackSlot != null ? fallbackSlot.ApplyModeKey : string.Empty;
        context.UseFlowKey = selectedSlot != null ? selectedSlot.UseFlowKey : fallbackSlot != null ? fallbackSlot.UseFlowKey : string.Empty;
        context.ContributionHintKey = selectedSlot != null ? selectedSlot.ContributionHintKey : fallbackSlot != null ? fallbackSlot.ContributionHintKey : string.Empty;
        context.PowerValue = selectedSlot != null ? Mathf.Max(selectedSlot.PowerValue, member.SkillPower) : fallbackSlot != null ? Mathf.Max(fallbackSlot.PowerValue, member.SkillPower) : member.SkillPower;
        context.CooldownTurns = selectedSlot != null ? selectedSlot.CooldownTurns : fallbackSlot != null ? fallbackSlot.CooldownTurns : 0;
        context.CooldownRemaining = selectedSlot != null ? selectedSlot.CooldownRemaining : 0;
        context.MaxEncounterCharges = selectedSlot != null ? selectedSlot.MaxEncounterCharges : fallbackSlot != null ? fallbackSlot.MaxEncounterCharges : 0;
        context.EncounterChargesRemaining = selectedSlot != null ? selectedSlot.EncounterChargesRemaining : 0;
        context.ResourceCost = selectedSlot != null ? selectedSlot.ResourceCost : fallbackSlot != null ? fallbackSlot.ResourceCost : 0;
        context.CurrentResource = member.CurrentSkillResource;
        context.MaxResource = Mathf.Max(1, member.MaxSkillResource);
        context.ActionPointCost = selectedSlot != null ? selectedSlot.ActionPointCost : fallbackSlot != null ? fallbackSlot.ActionPointCost : 1;
        context.IsAvailable = selectedSlot != null ? selectedSlot.IsAvailable : fallbackSlot != null;
        context.AvailabilityStateKey = selectedSlot != null ? selectedSlot.AvailabilityStateKey : string.Empty;
        context.AvailabilitySummaryText = selectedSlot != null ? selectedSlot.AvailabilitySummaryText : context.IsAvailable ? "Ready" : "Unavailable";
        context.SummaryText = resolvedLoadout.SummaryText;
        context.EffectSummaryText = selectedSlot != null ? selectedSlot.SummaryText : fallbackSlot != null ? fallbackSlot.SummaryText : string.Empty;
        context.ActiveSkillSlotSummaryText = member.ActiveSkillSlotSummaryText;
        context.CooldownSummaryText = member.SkillCooldownSummaryText;
        context.ResourceSummaryText = member.SkillResourceSummaryText;
        context.ActionEconomySummaryText = member.ActionEconomySummaryText;
        context.NextRunPreviewText = member.NextRunActiveSkillPreviewText;
        return context;
    }

    private PrototypeRpgBattleConsumableSlot BuildRpgBattleConsumableSlot(PrototypeRpgInventoryEntry entry, int slotIndex)
    {
        PrototypeRpgBattleConsumableSlot slot = new PrototypeRpgBattleConsumableSlot();
        PrototypeRpgConsumableDefinition definition = PrototypeRpgConsumableCatalog.GetDefinition(entry != null ? entry.ItemId : string.Empty);
        if (definition == null || entry == null)
        {
            return slot;
        }

        slot.SlotKey = "slot_" + Mathf.Max(0, slotIndex);
        slot.ItemId = definition.ItemId;
        slot.DisplayName = definition.DisplayName;
        slot.Quantity = Mathf.Max(0, entry.Quantity);
        slot.IsAvailable = definition.CanUseInBattle && slot.Quantity > 0;
        slot.EffectType = definition.BattleEffectType;
        slot.TargetKind = definition.TargetKind;
        slot.PowerValue = definition.PowerValue;
        slot.RecommendedLevel = definition.RecommendedLevel;
        slot.SummaryText = definition.DisplayName + " x" + slot.Quantity + " | " + definition.SummaryText;
        return slot;
    }

    private void RefreshRpgInventoryBattleSlotState(PrototypeRpgInventoryRuntimeState inventoryState)
    {
        if (inventoryState == null)
        {
            return;
        }

        List<PrototypeRpgBattleConsumableSlot> slots = new List<PrototypeRpgBattleConsumableSlot>();
        PrototypeRpgInventoryEntry[] entries = inventoryState.Consumables ?? System.Array.Empty<PrototypeRpgInventoryEntry>();
        for (int i = 0; i < entries.Length; i++)
        {
            PrototypeRpgBattleConsumableSlot slot = BuildRpgBattleConsumableSlot(entries[i], i);
            if (!string.IsNullOrEmpty(slot.ItemId))
            {
                slots.Add(slot);
            }
        }

        inventoryState.BattleConsumableSlots = slots.ToArray();
        inventoryState.BattleConsumableSlotSummaryText = BuildBattleConsumableSlotSummaryTextFromResolvedSlots(inventoryState);
        inventoryState.ConsumedThisRunSummaryText = BuildRpgConsumablesUsedSummaryText(inventoryState);
        inventoryState.CarryRewardSummaryText = BuildRpgConsumableCarrySummaryText(inventoryState);
    }

    private PrototypeRpgBattleConsumableSlot GetCurrentBattleConsumableSlot()
    {
        return GetCurrentBattleConsumableSlot(_sessionRpgInventoryRuntimeState);
    }

    private PrototypeRpgBattleConsumableSlot GetCurrentBattleConsumableSlot(PrototypeRpgInventoryRuntimeState inventoryState)
    {
        if (inventoryState == null)
        {
            return new PrototypeRpgBattleConsumableSlot();
        }

        PrototypeRpgBattleConsumableSlot[] slots = inventoryState.BattleConsumableSlots ?? System.Array.Empty<PrototypeRpgBattleConsumableSlot>();
        if (slots.Length <= 0)
        {
            RefreshRpgInventoryBattleSlotState(inventoryState);
            slots = inventoryState.BattleConsumableSlots ?? System.Array.Empty<PrototypeRpgBattleConsumableSlot>();
        }

        int livingEnemyCount = GetRpgConsumableLivingEnemyCount();
        int injuredPartyCount = GetRpgConsumableInjuredPartyCount();
        PrototypeRpgBattleConsumableSlot bestSlot = null;
        int bestScore = int.MinValue;
        for (int i = 0; i < slots.Length; i++)
        {
            PrototypeRpgBattleConsumableSlot slot = slots[i];
            if (slot == null || !slot.IsAvailable)
            {
                continue;
            }

            int score = ScoreRpgBattleConsumableSlot(slot, injuredPartyCount, livingEnemyCount);
            if (bestSlot == null || score > bestScore)
            {
                bestSlot = slot;
                bestScore = score;
            }
        }

        if (bestSlot != null)
        {
            return bestSlot;
        }

        return slots.Length > 0 && slots[0] != null ? slots[0] : new PrototypeRpgBattleConsumableSlot();
    }

    private int GetRpgConsumableLivingEnemyCount()
    {
        int count = 0;
        for (int i = 0; i < _activeMonsters.Count; i++)
        {
            DungeonMonsterRuntimeData monster = _activeMonsters[i];
            if (monster != null && !monster.IsDefeated && monster.CurrentHp > 0)
            {
                count += 1;
            }
        }

        return count;
    }

    private int GetRpgConsumableInjuredPartyCount()
    {
        if (_activeDungeonParty == null || _activeDungeonParty.Members == null)
        {
            return 0;
        }

        int count = 0;
        for (int i = 0; i < _activeDungeonParty.Members.Length; i++)
        {
            DungeonPartyMemberRuntimeData member = _activeDungeonParty.Members[i];
            if (member != null && !member.IsDefeated && member.CurrentHp > 0 && member.CurrentHp < member.MaxHp)
            {
                count += 1;
            }
        }

        return count;
    }

    private int ScoreRpgBattleConsumableSlot(PrototypeRpgBattleConsumableSlot slot, int injuredPartyCount, int livingEnemyCount)
    {
        if (slot == null || !slot.IsAvailable)
        {
            return int.MinValue;
        }

        int score = slot.PowerValue * 10 + slot.Quantity;
        if (slot.TargetKind == "all_allies")
        {
            score += injuredPartyCount > 1 ? 120 : injuredPartyCount > 0 ? 80 : -10;
            if (slot.EffectType == "protect_heal")
            {
                score += 10;
            }
        }
        else if (slot.TargetKind == "all_enemies")
        {
            score += livingEnemyCount > 1 ? 110 : livingEnemyCount == 1 ? 15 : -20;
        }
        else if (slot.TargetKind == "self")
        {
            score += injuredPartyCount > 0 ? 35 : 5;
        }
        else
        {
            score += 1;
        }

        return score;
    }

    private string BuildCurrentBattleConsumableSlotSummaryText()
    {
        return BuildCurrentBattleConsumableSlotSummaryText(_sessionRpgInventoryRuntimeState);
    }

    private string BuildBattleConsumableSlotSummaryTextFromResolvedSlots(PrototypeRpgInventoryRuntimeState inventoryState)
    {
        if (inventoryState == null)
        {
            return "No battle consumable slot.";
        }

        PrototypeRpgBattleConsumableSlot[] slots = inventoryState.BattleConsumableSlots ?? System.Array.Empty<PrototypeRpgBattleConsumableSlot>();
        if (slots.Length <= 0)
        {
            return "No battle consumable slot.";
        }

        int livingEnemyCount = GetRpgConsumableLivingEnemyCount();
        int injuredPartyCount = GetRpgConsumableInjuredPartyCount();
        PrototypeRpgBattleConsumableSlot bestSlot = null;
        int bestScore = int.MinValue;
        for (int i = 0; i < slots.Length; i++)
        {
            PrototypeRpgBattleConsumableSlot slot = slots[i];
            if (slot == null || !slot.IsAvailable)
            {
                continue;
            }

            int score = ScoreRpgBattleConsumableSlot(slot, injuredPartyCount, livingEnemyCount);
            if (bestSlot == null || score > bestScore)
            {
                bestSlot = slot;
                bestScore = score;
            }
        }

        if (bestSlot == null)
        {
            for (int i = 0; i < slots.Length; i++)
            {
                PrototypeRpgBattleConsumableSlot slot = slots[i];
                if (slot != null && !string.IsNullOrEmpty(slot.ItemId))
                {
                    bestSlot = slot;
                    break;
                }
            }
        }

        if (bestSlot == null || string.IsNullOrEmpty(bestSlot.ItemId))
        {
            return "No battle consumable slot.";
        }

        string targetText = bestSlot.TargetKind == "all_allies" ? "all allies" : bestSlot.TargetKind == "self" ? "self" : bestSlot.TargetKind;
        return bestSlot.DisplayName + " x" + bestSlot.Quantity + " | " + targetText + " | " + bestSlot.EffectType + " " + bestSlot.PowerValue;
    }

    private string BuildCurrentBattleConsumableSlotSummaryText(PrototypeRpgInventoryRuntimeState inventoryState)
    {
        if (inventoryState == null)
        {
            return "No battle consumable slot.";
        }

        if (!string.IsNullOrEmpty(inventoryState.BattleConsumableSlotSummaryText))
        {
            return inventoryState.BattleConsumableSlotSummaryText;
        }

        return BuildBattleConsumableSlotSummaryTextFromResolvedSlots(inventoryState);
    }

    private string BuildRpgPartySkillLoadoutSummaryText()
    {
        if (_activeDungeonParty == null || _activeDungeonParty.Members == null || _activeDungeonParty.Members.Length <= 0)
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

            PrototypeRpgResolvedSkillLoadout loadout = BuildResolvedRpgSkillLoadout(member);
            parts.Add(member.DisplayName + ": " + (string.IsNullOrEmpty(loadout.LoadoutLabel) ? member.SkillName : loadout.LoadoutLabel));
        }

        return parts.Count <= 0 ? string.Empty : "Loadouts " + string.Join(" | ", parts.ToArray());
    }

    private string BuildRpgExperienceGainSummaryText(PrototypeRpgPartyLevelRuntimeState partyLevelState)
    {
        if (partyLevelState == null || partyLevelState.Members == null || partyLevelState.Members.Length <= 0)
        {
            return string.Empty;
        }

        int totalExperience = 0;
        int levelUpCount = 0;
        for (int i = 0; i < partyLevelState.Members.Length; i++)
        {
            PrototypeRpgPartyMemberLevelRuntimeState member = partyLevelState.Members[i];
            if (member == null || member.Experience == null)
            {
                continue;
            }

            totalExperience += Mathf.Max(0, member.Experience.GainedExperienceThisRun);
            if (member.Experience.LeveledUpThisRun)
            {
                levelUpCount += 1;
            }
        }

        return "EXP gained " + totalExperience + " | Level jumps " + levelUpCount + " member(s) | Next-run hydrate ready " + Mathf.Max(0, partyLevelState.AppliedMemberCount) + " member(s).";
    }
    private string BuildRpgLevelUpPreviewSummaryText(PrototypeRpgPartyLevelRuntimeState partyLevelState)
    {
        if (partyLevelState == null || partyLevelState.Members == null || partyLevelState.Members.Length <= 0)
        {
            return string.Empty;
        }

        List<string> leveledMembers = new List<string>();
        for (int i = 0; i < partyLevelState.Members.Length; i++)
        {
            PrototypeRpgPartyMemberLevelRuntimeState member = partyLevelState.Members[i];
            if (member == null || member.LevelUpPreview == null || !member.LevelUpPreview.LeveledUpThisRun)
            {
                continue;
            }

            leveledMembers.Add(member.LevelUpPreview.SummaryText);
        }

        return leveledMembers.Count <= 0 ? "Level-up preview: no level-up this run." : "Level-up preview: " + string.Join(" | ", leveledMembers.ToArray());
    }

    private string BuildRpgLevelUpApplyReadySummaryText(PrototypeRpgPartyLevelRuntimeState partyLevelState)
    {
        if (partyLevelState == null || partyLevelState.Members == null || partyLevelState.Members.Length <= 0)
        {
            return string.Empty;
        }

        List<string> readyMembers = new List<string>();
        for (int i = 0; i < partyLevelState.Members.Length; i++)
        {
            PrototypeRpgPartyMemberLevelRuntimeState member = partyLevelState.Members[i];
            if (member == null || member.LevelUpPreview == null || string.IsNullOrEmpty(member.LevelUpPreview.ApplyReadySummaryText))
            {
                continue;
            }

            readyMembers.Add(member.LevelUpPreview.ApplyReadySummaryText);
        }

        return readyMembers.Count <= 0 ? "Level-up commit: no new level was applied this run." : "Level-up commit: " + string.Join(" | ", readyMembers.ToArray());
    }
    private string BuildRpgNextRunStatProjectionSummaryText(PrototypeRpgPartyLevelRuntimeState partyLevelState)
    {
        if (partyLevelState == null || partyLevelState.Members == null || partyLevelState.Members.Length <= 0)
        {
            return string.Empty;
        }

        List<string> projectedMembers = new List<string>();
        for (int i = 0; i < partyLevelState.Members.Length; i++)
        {
            PrototypeRpgPartyMemberLevelRuntimeState member = partyLevelState.Members[i];
            if (member == null || member.LevelUpPreview == null || string.IsNullOrEmpty(member.LevelUpPreview.NextRunStatProjectionSummaryText))
            {
                continue;
            }

            projectedMembers.Add(member.LevelUpPreview.NextRunStatProjectionSummaryText);
        }

        return projectedMembers.Count <= 0 ? "Next-run stat projection: stable baseline." : "Next-run stat projection: " + string.Join(" | ", projectedMembers.ToArray());
    }

    private string BuildRpgConsumablesUsedSummaryText(PrototypeRpgInventoryRuntimeState inventoryState)
    {
        if (inventoryState == null)
        {
            return string.Empty;
        }

        if (inventoryState.ConsumablesUsedThisRun <= 0)
        {
            return "Consumable runtime use: none this run.";
        }

        string lastUseText = string.IsNullOrEmpty(inventoryState.LastConsumableUseSummaryText) ? "last use summary unavailable." : inventoryState.LastConsumableUseSummaryText;
        return "Consumable runtime use: " + inventoryState.ConsumablesUsedThisRun + " used | " + lastUseText;
    }

    private string BuildRpgSkillLoadoutReadbackSummaryText(PrototypeRpgPartyDefinition partyDefinition, PrototypeRpgPartyLevelRuntimeState partyLevelState)
    {
        if (partyDefinition == null || partyDefinition.Members == null || partyDefinition.Members.Length <= 0)
        {
            return string.Empty;
        }

        List<string> parts = new List<string>();
        for (int i = 0; i < partyDefinition.Members.Length; i++)
        {
            PrototypeRpgPartyMemberDefinition definition = partyDefinition.Members[i];
            if (definition == null)
            {
                continue;
            }

            PrototypeRpgPartyMemberLevelRuntimeState levelState = GetRpgMemberLevelRuntimeState(partyLevelState, definition.MemberId);
            PrototypeRpgSkillLoadoutDefinition loadout = PrototypeRpgSkillLoadoutCatalog.ResolveDefinition(levelState != null ? levelState.SkillLoadoutId : definition.SkillLoadoutId, definition.RoleTag, definition.DefaultSkillId);
            if (loadout != null)
            {
                parts.Add(definition.DisplayName + " " + loadout.DisplayName);
            }
        }

        return parts.Count <= 0 ? string.Empty : "Skill runtime: " + string.Join(" | ", parts.ToArray());
    }

    private string BuildRpgConsumableCarrySummaryText(PrototypeRpgInventoryRuntimeState inventoryState)
    {
        if (inventoryState == null)
        {
            return string.Empty;
        }

        int carryAmount = CountRpgCarryRewardAmount(inventoryState.CarryEntries);
        string slotSummaryText = BuildCurrentBattleConsumableSlotSummaryText(inventoryState);
        string useSummaryText = string.IsNullOrEmpty(inventoryState.ConsumedThisRunSummaryText) ? BuildRpgConsumablesUsedSummaryText(inventoryState) : inventoryState.ConsumedThisRunSummaryText;
        string policyText = string.IsNullOrEmpty(inventoryState.CarryoverPolicySummaryText) ? string.Empty : " | " + inventoryState.CarryoverPolicySummaryText;
        string nextRunCarryText = string.IsNullOrEmpty(inventoryState.NextRunCarrySummaryText) ? string.Empty : " | " + inventoryState.NextRunCarrySummaryText;
        return "Consumables: " + slotSummaryText + " | " + useSummaryText + policyText + nextRunCarryText + " | Carry " + BuildLootAmountText(carryAmount) + ".";
    }
    private void AwardRpgResultConsumableCarry(PrototypeRpgInventoryRuntimeState inventoryState, PrototypeRpgRunResultSnapshot runResultSnapshot)
    {
        if (inventoryState == null || runResultSnapshot == null)
        {
            return;
        }

        if (runResultSnapshot.ResultStateKey == PrototypeBattleOutcomeKeys.RunClear)
        {
            AddOrIncrementRpgInventoryEntry(inventoryState, "consumable_field_tonic", 1, "run_clear");
            AddOrIncrementRpgInventoryEntry(inventoryState, "consumable_burst_flask", 1, "run_clear");
        }

        if (runResultSnapshot.EliteOutcome != null && runResultSnapshot.EliteOutcome.IsEliteDefeated)
        {
            AddOrIncrementRpgInventoryEntry(inventoryState, "consumable_smoke_ward", 1, "elite_clear");
            AddOrIncrementRpgInventoryEntry(inventoryState, "consumable_safeguard_kit", 1, "elite_clear");
        }
    }

    private void AddOrIncrementRpgInventoryEntry(PrototypeRpgInventoryRuntimeState inventoryState, string itemId, int amount, string sourceKey)
    {
        if (inventoryState == null || amount <= 0)
        {
            return;
        }

        PrototypeRpgInventoryEntry[] entries = inventoryState.Consumables ?? System.Array.Empty<PrototypeRpgInventoryEntry>();
        for (int i = 0; i < entries.Length; i++)
        {
            PrototypeRpgInventoryEntry entry = entries[i];
            if (entry != null && entry.ItemId == itemId)
            {
                entry.Quantity += amount;
                entry.SourceKey = string.IsNullOrEmpty(sourceKey) ? entry.SourceKey : sourceKey;
                entry.SummaryText = entry.DisplayName + " x" + entry.Quantity;
                return;
            }
        }

        List<PrototypeRpgInventoryEntry> expanded = new List<PrototypeRpgInventoryEntry>(entries);
        AddRpgInventoryEntry(expanded, itemId, amount, sourceKey);
        inventoryState.Consumables = expanded.ToArray();
    }

    private bool TryUseCurrentBattleConsumable(DungeonPartyMemberRuntimeData actor)
    {
        if (actor == null)
        {
            return false;
        }

        PrototypeRpgInventoryRuntimeState inventoryState = GetOrCreateRpgInventoryRuntimeState(_activeDungeonParty != null ? _activeDungeonParty.PartyDefinition : null);
        PrototypeRpgBattleConsumableSlot slot = GetCurrentBattleConsumableSlot(inventoryState);
        if (slot == null || string.IsNullOrEmpty(slot.ItemId) || !slot.IsAvailable)
        {
            return false;
        }

        int resolvedValue = 0;
        string targetName = actor.DisplayName;
        string logText;
        string feedbackText;
        if (slot.EffectType == "damage" && slot.TargetKind == "all_enemies")
        {
            List<DungeonMonsterRuntimeData> targetMonsters = GetTargetableBattleMonsters();
            int hitCount = 0;
            for (int i = 0; i < targetMonsters.Count; i++)
            {
                int appliedDamage = ApplyBattleDamageToMonster(actor, targetMonsters[i], slot.PowerValue, new Color(1f, 0.66f, 0.22f, 1f));
                if (appliedDamage > 0)
                {
                    resolvedValue += appliedDamage;
                    hitCount += 1;
                }
            }

            targetName = "All enemies";
            logText = actor.DisplayName + " used " + slot.DisplayName + " and hit " + hitCount + " enemies for " + resolvedValue + " damage.";
            feedbackText = slot.DisplayName + " dealt " + resolvedValue + " total damage.";
        }
        else if (slot.TargetKind == "all_allies")
        {
            List<int> livingAllies = GetLivingAllies();
            for (int i = 0; i < livingAllies.Count; i++)
            {
                int memberIndex = livingAllies[i];
                DungeonPartyMemberRuntimeData ally = GetPartyMemberAtIndex(memberIndex);
                int bonusRecovery = slot.EffectType == "protect_heal" && ally != null && ally.CurrentHp <= Mathf.Max(1, ally.MaxHp / 2) ? 1 : 0;
                resolvedValue += ApplyBattleHealToPartyMember(actor, ally, memberIndex, slot.PowerValue + bonusRecovery, new Color(0.82f, 0.94f, 0.60f, 1f));
            }

            targetName = "All allies";
            logText = actor.DisplayName + " used " + slot.DisplayName + " and restored " + resolvedValue + " HP to the party.";
            feedbackText = slot.DisplayName + " restored " + resolvedValue + " HP.";
        }
        else
        {
            resolvedValue = ApplyBattleHealToPartyMember(actor, actor, actor.PartySlotIndex, slot.PowerValue, new Color(0.82f, 0.94f, 0.60f, 1f));
            logText = actor.DisplayName + " used " + slot.DisplayName + " and recovered " + resolvedValue + " HP.";
            feedbackText = slot.DisplayName + " recovered " + resolvedValue + " HP.";
        }

        PrototypeRpgInventoryEntry[] entries = inventoryState.Consumables ?? System.Array.Empty<PrototypeRpgInventoryEntry>();
        for (int i = 0; i < entries.Length; i++)
        {
            PrototypeRpgInventoryEntry entry = entries[i];
            if (entry != null && entry.ItemId == slot.ItemId)
            {
                entry.Quantity = Mathf.Max(0, entry.Quantity - 1);
                entry.SummaryText = entry.DisplayName + " x" + entry.Quantity;
                break;
            }
        }

        inventoryState.LastResolvedConsumableId = slot.ItemId;
        inventoryState.ConsumablesUsedThisRun += 1;
        inventoryState.LastConsumableUseSummaryText = logText;
        inventoryState.TotalConsumableCount = CountRpgInventoryEntries(inventoryState.Consumables);
        inventoryState.TotalCarryRewardAmount = CountRpgCarryRewardAmount(inventoryState.CarryEntries);
        RefreshRpgInventoryBattleSlotState(inventoryState);
        inventoryState.ConsumedThisRunSummaryText = BuildRpgConsumablesUsedSummaryText(inventoryState);
        inventoryState.InventorySummaryText = BuildRpgInventorySummaryText(inventoryState);

        RecordBattleEvent(
            PrototypeBattleEventKeys.ConsumableResolved,
            actor.MemberId,
            string.Empty,
            resolvedValue,
            actor.DisplayName + " used " + slot.DisplayName + ".",
            actionKey: "item",
            phaseKey: "resolution",
            actorName: actor.DisplayName,
            targetName: targetName,
            shortText: slot.DisplayName);
        AppendBattleLog(logText);
        SetBattleFeedbackText(feedbackText);
        AdvanceBattleAfterPartyAction();
        return true;
    }

    private void ApplyRpgBackboneReadbackConsistency(PrototypeRpgRunResultSnapshot runResultSnapshot)
    {
        if (runResultSnapshot == null)
        {
            return;
        }

        PrototypeRpgPartyDefinition partyDefinition = _activeDungeonParty != null ? _activeDungeonParty.PartyDefinition : null;
        PrototypeRpgPartyLevelRuntimeState partyLevelState = _sessionRpgPartyLevelRuntimeState ?? new PrototypeRpgPartyLevelRuntimeState();
        PrototypeRpgInventoryRuntimeState inventoryState = _sessionRpgInventoryRuntimeState ?? new PrototypeRpgInventoryRuntimeState();
        RefreshRpgInventoryBattleSlotState(inventoryState);

        runResultSnapshot.ExperienceGainSummaryText = string.IsNullOrEmpty(partyLevelState.ExperienceGainSummaryText) ? BuildRpgExperienceGainSummaryText(partyLevelState) : partyLevelState.ExperienceGainSummaryText;
        runResultSnapshot.LevelUpPreviewSummaryText = string.IsNullOrEmpty(partyLevelState.LevelUpPreviewSummaryText) ? BuildRpgLevelUpPreviewSummaryText(partyLevelState) : partyLevelState.LevelUpPreviewSummaryText;
        runResultSnapshot.LevelUpApplyReadySummaryText = string.IsNullOrEmpty(partyLevelState.LevelUpApplyReadySummaryText) ? BuildRpgLevelUpApplyReadySummaryText(partyLevelState) : partyLevelState.LevelUpApplyReadySummaryText;
        runResultSnapshot.ActualLevelApplySummaryText = string.IsNullOrEmpty(partyLevelState.ActualLevelApplySummaryText) ? BuildRpgActualLevelApplySummaryText(partyLevelState) : partyLevelState.ActualLevelApplySummaryText;
        runResultSnapshot.DerivedStatHydrateSummaryText = string.IsNullOrEmpty(partyLevelState.DerivedStatHydrateSummaryText) ? BuildRpgDerivedStatHydrateSummaryText(partyLevelState) : partyLevelState.DerivedStatHydrateSummaryText;
        runResultSnapshot.NextRunStatProjectionSummaryText = string.IsNullOrEmpty(partyLevelState.NextRunStatProjectionSummaryText) ? BuildRpgNextRunStatProjectionSummaryText(partyLevelState) : partyLevelState.NextRunStatProjectionSummaryText;
        runResultSnapshot.JobSpecializationSummaryText = string.IsNullOrEmpty(partyLevelState.JobSpecializationSummaryText) ? CurrentPartyJobSpecializationSummaryText : partyLevelState.JobSpecializationSummaryText;
        runResultSnapshot.PassiveSkillSlotSummaryText = string.IsNullOrEmpty(partyLevelState.PassiveSkillSlotSummaryText) ? CurrentPartyPassiveSkillSlotSummaryText : partyLevelState.PassiveSkillSlotSummaryText;
        runResultSnapshot.RuntimeSynergySummaryText = string.IsNullOrEmpty(partyLevelState.RuntimeSynergySummaryText) ? CurrentPartyRuntimeSynergySummaryText : partyLevelState.RuntimeSynergySummaryText;
        runResultSnapshot.NextRunSpecializationPreviewText = string.IsNullOrEmpty(partyLevelState.NextRunSpecializationPreviewText) ? CurrentNextRunSpecializationPreviewText : partyLevelState.NextRunSpecializationPreviewText;
        runResultSnapshot.ActiveSkillSlotSummaryText = CurrentPartyActiveSkillSlotSummaryText;
        runResultSnapshot.SkillCooldownSummaryText = CurrentPartySkillCooldownSummaryText;
        runResultSnapshot.SkillResourceSummaryText = CurrentPartySkillResourceSummaryText;
        runResultSnapshot.ActionEconomySummaryText = CurrentPartyActionEconomySummaryText;
        runResultSnapshot.NextRunActiveSkillPreviewText = CurrentNextRunActiveSkillPreviewText;
        runResultSnapshot.SkillLoadoutReadbackSummaryText = BuildRpgSkillLoadoutReadbackSummaryText(partyDefinition, partyLevelState);
        runResultSnapshot.EquipmentLoadoutReadbackSummaryText = string.IsNullOrEmpty(partyLevelState.EquipmentLoadoutReadbackSummaryText) ? BuildRpgEquipmentLoadoutReadbackSummaryText(partyDefinition, partyLevelState) : partyLevelState.EquipmentLoadoutReadbackSummaryText;
        runResultSnapshot.GearContributionSummaryText = string.IsNullOrEmpty(partyLevelState.GearContributionSummaryText) ? BuildRpgPartyGearContributionSummaryText(partyLevelState) : partyLevelState.GearContributionSummaryText;
        runResultSnapshot.ConsumableCarrySummaryText = BuildRpgConsumableCarrySummaryText(inventoryState);
        runResultSnapshot.ConsumableCarryoverPolicySummaryText = string.IsNullOrEmpty(inventoryState.CarryoverPolicySummaryText) ? BuildRpgConsumableCarryoverPolicySummaryText(inventoryState, runResultSnapshot, Mathf.Max(0, inventoryState.DepletedConsumableCount)) : inventoryState.CarryoverPolicySummaryText;

        PrototypeRpgPartyMemberOutcomeSnapshot[] members = runResultSnapshot.PartyOutcome != null ? (runResultSnapshot.PartyOutcome.Members ?? System.Array.Empty<PrototypeRpgPartyMemberOutcomeSnapshot>()) : System.Array.Empty<PrototypeRpgPartyMemberOutcomeSnapshot>();
        for (int i = 0; i < members.Length; i++)
        {
            PrototypeRpgPartyMemberOutcomeSnapshot memberOutcome = members[i];
            if (memberOutcome == null)
            {
                continue;
            }

            PrototypeRpgPartyMemberLevelRuntimeState levelState = GetRpgMemberLevelRuntimeState(partyLevelState, memberOutcome.MemberId);
            if (levelState == null)
            {
                continue;
            }

            memberOutcome.Level = Mathf.Max(1, levelState.Experience.Level);
            memberOutcome.CurrentExperience = Mathf.Max(0, levelState.Experience.CurrentExperience);
            memberOutcome.ExperienceToNextLevel = Mathf.Max(1, levelState.Experience.ExperienceToNextLevel);
            memberOutcome.GainedExperienceThisRun = Mathf.Max(0, levelState.Experience.GainedExperienceThisRun);
            memberOutcome.LeveledUpThisRun = levelState.Experience.LeveledUpThisRun;
            memberOutcome.LevelUpPreviewText = levelState.LevelUpPreview != null ? levelState.LevelUpPreview.SummaryText : string.Empty;
            memberOutcome.JobSpecializationSummaryText = string.IsNullOrEmpty(levelState.JobSpecializationSummaryText) ? string.Empty : levelState.JobSpecializationSummaryText;
            memberOutcome.PassiveSkillSlotSummaryText = string.IsNullOrEmpty(levelState.PassiveSkillSlotSummaryText) ? string.Empty : levelState.PassiveSkillSlotSummaryText;
            memberOutcome.RuntimeSynergySummaryText = string.IsNullOrEmpty(levelState.RuntimeSynergySummaryText) ? string.Empty : levelState.RuntimeSynergySummaryText;
            DungeonPartyMemberRuntimeData runtimeMember = GetPartyMemberById(memberOutcome.MemberId);
            memberOutcome.ActiveSkillSlotSummaryText = runtimeMember != null ? runtimeMember.ActiveSkillSlotSummaryText : string.Empty;
            memberOutcome.SkillCooldownSummaryText = runtimeMember != null ? runtimeMember.SkillCooldownSummaryText : string.Empty;
            memberOutcome.SkillResourceSummaryText = runtimeMember != null ? runtimeMember.SkillResourceSummaryText : string.Empty;
            memberOutcome.ActionEconomySummaryText = runtimeMember != null ? runtimeMember.ActionEconomySummaryText : string.Empty;
            memberOutcome.DerivedStatSummaryText = levelState.DerivedStats != null ? levelState.DerivedStats.SummaryText : string.Empty;
            memberOutcome.EquipmentSummaryText = string.IsNullOrEmpty(levelState.EquipmentSummaryText) ? string.Empty : levelState.EquipmentSummaryText;
            memberOutcome.GearContributionSummaryText = string.IsNullOrEmpty(levelState.GearContributionSummaryText) ? string.Empty : levelState.GearContributionSummaryText;
            memberOutcome.SkillLoadoutSummaryText = BuildRpgSkillLoadoutSummaryText(PrototypeRpgSkillLoadoutCatalog.ResolveDefinition(levelState.SkillLoadoutId, memberOutcome.RoleTag, memberOutcome.DefaultSkillId));
            memberOutcome.ConsumableSlotSummaryText = BuildCurrentBattleConsumableSlotSummaryText(inventoryState);
        }
    }
}

