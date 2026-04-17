using System;
using UnityEngine;

public sealed class PrototypeRpgPartyRuntimeResolveSurface
{
    public string PartyId = string.Empty;
    public string DisplayName = string.Empty;
    public string ArchetypeId = string.Empty;
    public string ArchetypeLabel = string.Empty;
    public string StrengthSummaryText = string.Empty;
    public string RouteFitSummaryText = string.Empty;
    public string DoctrineSummaryText = string.Empty;
    public string PromotionStateId = string.Empty;
    public string PromotionStateLabel = string.Empty;
    public string PromotionSummaryText = string.Empty;
    public int DerivedPower;
    public int DerivedCarryCapacity;
    public PrototypeRpgMemberRuntimeResolveSurface[] Members = Array.Empty<PrototypeRpgMemberRuntimeResolveSurface>();
    public string AppliedProgressionSummaryText = string.Empty;
    public string CurrentRunSummaryText = string.Empty;
    public string NextRunPreviewSummaryText = string.Empty;
}

public sealed class PrototypeRpgMemberRuntimeResolveSurface
{
    public string MemberId = string.Empty;
    public string DisplayName = "Adventurer";
    public string RoleTag = "adventurer";
    public string RoleLabel = "Adventurer";
    public int PartySlotIndex;
    public string GrowthTrackId = string.Empty;
    public string JobId = string.Empty;
    public string EquipmentLoadoutId = string.Empty;
    public string SkillLoadoutId = string.Empty;
    public string DefaultSkillId = string.Empty;
    public string ResolvedSkillName = "Skill";
    public string ResolvedSkillShortText = string.Empty;
    public int MaxHp = 1;
    public int Attack = 1;
    public int Defense;
    public int Speed;
    public int SkillPower = 1;
    public string EquipmentSummaryText = "No gear";
    public string GearContributionSummaryText = "No bonus";
    public string AppliedProgressionSummaryText = string.Empty;
    public string CurrentRunSummaryText = string.Empty;
    public string NextRunPreviewSummaryText = string.Empty;
}

public static class PrototypeRpgRuntimeResolveBuilder
{
    public static PrototypeRpgPartyRuntimeResolveSurface BuildPartySurface(PrototypeRpgPartyDefinition partyDefinition, Func<PrototypeRpgPartyMemberDefinition, string> equipmentLoadoutResolver = null)
    {
        PrototypeRpgPartyRuntimeResolveSurface surface = new PrototypeRpgPartyRuntimeResolveSurface();
        surface.PartyId = partyDefinition != null && !string.IsNullOrWhiteSpace(partyDefinition.PartyId) ? partyDefinition.PartyId.Trim() : string.Empty;
        surface.DisplayName = partyDefinition != null && !string.IsNullOrWhiteSpace(partyDefinition.DisplayName) ? partyDefinition.DisplayName.Trim() : "Party";
        surface.ArchetypeId = partyDefinition != null && !string.IsNullOrWhiteSpace(partyDefinition.ArchetypeId) ? partyDefinition.ArchetypeId.Trim() : string.Empty;
        surface.ArchetypeLabel = partyDefinition != null && !string.IsNullOrWhiteSpace(partyDefinition.ArchetypeLabel) ? partyDefinition.ArchetypeLabel.Trim() : string.Empty;
        surface.StrengthSummaryText = partyDefinition != null && !string.IsNullOrWhiteSpace(partyDefinition.StrengthSummaryText) ? partyDefinition.StrengthSummaryText.Trim() : string.Empty;
        surface.RouteFitSummaryText = partyDefinition != null && !string.IsNullOrWhiteSpace(partyDefinition.RouteFitSummaryText) ? partyDefinition.RouteFitSummaryText.Trim() : string.Empty;
        surface.DoctrineSummaryText = partyDefinition != null && !string.IsNullOrWhiteSpace(partyDefinition.DoctrineSummaryText) ? partyDefinition.DoctrineSummaryText.Trim() : string.Empty;
        surface.PromotionStateId = partyDefinition != null && !string.IsNullOrWhiteSpace(partyDefinition.PromotionStateId) ? partyDefinition.PromotionStateId.Trim() : string.Empty;
        surface.PromotionStateLabel = partyDefinition != null && !string.IsNullOrWhiteSpace(partyDefinition.PromotionStateLabel) ? partyDefinition.PromotionStateLabel.Trim() : string.Empty;
        surface.PromotionSummaryText = partyDefinition != null && !string.IsNullOrWhiteSpace(partyDefinition.PromotionSummaryText) ? partyDefinition.PromotionSummaryText.Trim() : string.Empty;
        surface.DerivedPower = partyDefinition != null ? Mathf.Max(1, partyDefinition.DerivedPower) : 1;
        surface.DerivedCarryCapacity = partyDefinition != null ? Mathf.Max(1, partyDefinition.DerivedCarryCapacity) : 1;

        if (partyDefinition == null || partyDefinition.Members == null || partyDefinition.Members.Length == 0)
        {
            surface.Members = Array.Empty<PrototypeRpgMemberRuntimeResolveSurface>();
            surface.AppliedProgressionSummaryText = "No applied progression.";
            surface.CurrentRunSummaryText = "No runtime party.";
            surface.NextRunPreviewSummaryText = "No next-run preview.";
            return surface;
        }

        PrototypeRpgMemberRuntimeResolveSurface[] members = new PrototypeRpgMemberRuntimeResolveSurface[partyDefinition.Members.Length];
        string[] appliedParts = new string[members.Length];
        string[] currentRunParts = new string[members.Length];
        string[] nextRunParts = new string[members.Length];

        for (int i = 0; i < members.Length; i++)
        {
            PrototypeRpgPartyMemberDefinition memberDefinition = partyDefinition.Members[i];
            string resolvedLoadoutId = equipmentLoadoutResolver != null && memberDefinition != null
                ? equipmentLoadoutResolver(memberDefinition)
                : null;
            members[i] = BuildMemberSurface(memberDefinition, resolvedLoadoutId);
            appliedParts[i] = members[i].DisplayName + ": " + members[i].AppliedProgressionSummaryText;
            currentRunParts[i] = members[i].DisplayName + ": " + members[i].CurrentRunSummaryText;
            nextRunParts[i] = members[i].DisplayName + ": " + members[i].NextRunPreviewSummaryText;
        }

        surface.Members = members;
        string memberAppliedText = JoinNonEmpty(appliedParts, " | ", string.Empty);
        string memberCurrentRunText = JoinNonEmpty(currentRunParts, " | ", string.Empty);
        string memberNextRunText = JoinNonEmpty(nextRunParts, " | ", string.Empty);
        surface.AppliedProgressionSummaryText = JoinNonEmpty(
            new[]
            {
                surface.ArchetypeLabel,
                surface.PromotionStateLabel,
                surface.DoctrineSummaryText,
                memberAppliedText
            },
            " | ",
            "No applied progression.");
        surface.CurrentRunSummaryText = JoinNonEmpty(
            new[]
            {
                surface.StrengthSummaryText,
                "Power " + surface.DerivedPower,
                "Carry " + surface.DerivedCarryCapacity,
                memberCurrentRunText
            },
            " | ",
            "No runtime party.");
        surface.NextRunPreviewSummaryText = JoinNonEmpty(
            new[]
            {
                surface.RouteFitSummaryText,
                surface.PromotionSummaryText,
                memberNextRunText
            },
            " | ",
            "No next-run preview.");
        return surface;
    }

    public static PrototypeRpgMemberRuntimeResolveSurface BuildMemberSurface(PrototypeRpgPartyMemberDefinition memberDefinition, string resolvedEquipmentLoadoutId = null)
    {
        PrototypeRpgMemberRuntimeResolveSurface surface = new PrototypeRpgMemberRuntimeResolveSurface();
        PrototypeRpgStatBlock baseStats = memberDefinition != null && memberDefinition.BaseStats != null
            ? memberDefinition.BaseStats
            : new PrototypeRpgStatBlock(1, 1, 0, 0);

        string roleTag = memberDefinition != null && !string.IsNullOrWhiteSpace(memberDefinition.RoleTag)
            ? memberDefinition.RoleTag.Trim().ToLowerInvariant()
            : "adventurer";
        string equipmentLoadoutId = !string.IsNullOrWhiteSpace(resolvedEquipmentLoadoutId)
            ? resolvedEquipmentLoadoutId.Trim()
            : (memberDefinition != null && !string.IsNullOrWhiteSpace(memberDefinition.EquipmentLoadoutId)
                ? memberDefinition.EquipmentLoadoutId.Trim()
                : string.Empty);
        PrototypeRpgEquipmentDefinition equipmentDefinition = PrototypeRpgEquipmentCatalog.ResolveDefinition(equipmentLoadoutId, roleTag);
        PrototypeRpgSkillDefinition skillDefinition = PrototypeRpgSkillCatalog.ResolveDefinition(memberDefinition != null ? memberDefinition.DefaultSkillId : string.Empty, roleTag)
            ?? PrototypeRpgSkillCatalog.GetFallbackDefinitionForRoleTag(roleTag);

        surface.MemberId = memberDefinition != null && !string.IsNullOrWhiteSpace(memberDefinition.MemberId) ? memberDefinition.MemberId.Trim() : string.Empty;
        surface.DisplayName = memberDefinition != null && !string.IsNullOrWhiteSpace(memberDefinition.DisplayName) ? memberDefinition.DisplayName.Trim() : "Adventurer";
        surface.RoleTag = roleTag;
        surface.RoleLabel = memberDefinition != null && !string.IsNullOrWhiteSpace(memberDefinition.RoleLabel) ? memberDefinition.RoleLabel.Trim() : "Adventurer";
        surface.PartySlotIndex = memberDefinition != null && memberDefinition.PartySlotIndex >= 0 ? memberDefinition.PartySlotIndex : 0;
        surface.GrowthTrackId = memberDefinition != null && !string.IsNullOrWhiteSpace(memberDefinition.GrowthTrackId) ? memberDefinition.GrowthTrackId.Trim() : string.Empty;
        surface.JobId = memberDefinition != null && !string.IsNullOrWhiteSpace(memberDefinition.JobId) ? memberDefinition.JobId.Trim() : string.Empty;
        surface.EquipmentLoadoutId = equipmentDefinition != null ? equipmentDefinition.LoadoutId : string.Empty;
        surface.SkillLoadoutId = memberDefinition != null && !string.IsNullOrWhiteSpace(memberDefinition.SkillLoadoutId) ? memberDefinition.SkillLoadoutId.Trim() : string.Empty;
        surface.DefaultSkillId = skillDefinition != null ? skillDefinition.SkillId : string.Empty;
        surface.ResolvedSkillName = skillDefinition != null ? skillDefinition.DisplayName : "Skill";
        surface.ResolvedSkillShortText = skillDefinition != null && !string.IsNullOrWhiteSpace(skillDefinition.ShortText) ? skillDefinition.ShortText.Trim() : string.Empty;
        surface.MaxHp = Mathf.Max(1, baseStats.MaxHp + (equipmentDefinition != null ? equipmentDefinition.MaxHpBonus : 0));
        surface.Attack = Mathf.Max(1, baseStats.Attack + (equipmentDefinition != null ? equipmentDefinition.AttackBonus : 0));
        surface.Defense = Mathf.Max(0, baseStats.Defense + (equipmentDefinition != null ? equipmentDefinition.DefenseBonus : 0));
        surface.Speed = Mathf.Max(0, baseStats.Speed + (equipmentDefinition != null ? equipmentDefinition.SpeedBonus : 0));
        surface.SkillPower = skillDefinition != null && skillDefinition.PowerValue > 0
            ? skillDefinition.PowerValue
            : Mathf.Max(1, surface.Attack + 1);
        surface.EquipmentSummaryText = PrototypeRpgEquipmentCatalog.BuildEquipmentSummaryText(equipmentDefinition);
        surface.GearContributionSummaryText = PrototypeRpgEquipmentCatalog.BuildStatContributionSummary(equipmentDefinition);
        surface.AppliedProgressionSummaryText = BuildAppliedProgressionSummary(surface);
        surface.CurrentRunSummaryText = BuildCurrentRunSummary(surface);
        surface.NextRunPreviewSummaryText = BuildNextRunPreviewSummary(surface);
        return surface;
    }

    private static string BuildAppliedProgressionSummary(PrototypeRpgMemberRuntimeResolveSurface surface)
    {
        return JoinNonEmpty(
            new[]
            {
                BuildLabelValue("Growth", surface.GrowthTrackId),
                BuildLabelValue("Job", surface.JobId),
                BuildLabelValue("Gear", surface.EquipmentSummaryText),
                BuildLabelValue("Skill", surface.SkillLoadoutId)
            },
            " | ",
            "No applied progression.");
    }

    private static string BuildCurrentRunSummary(PrototypeRpgMemberRuntimeResolveSurface surface)
    {
        return JoinNonEmpty(
            new[]
            {
                surface.ResolvedSkillName,
                "HP " + surface.MaxHp,
                "ATK " + surface.Attack,
                "DEF " + surface.Defense,
                "SPD " + surface.Speed,
                surface.GearContributionSummaryText
            },
            " | ",
            "No current-run summary.");
    }

    private static string BuildNextRunPreviewSummary(PrototypeRpgMemberRuntimeResolveSurface surface)
    {
        return JoinNonEmpty(
            new[]
            {
                "Next " + surface.RoleLabel,
                surface.EquipmentSummaryText,
                surface.ResolvedSkillName,
                "Power " + surface.SkillPower
            },
            " | ",
            "No next-run preview.");
    }

    private static string BuildLabelValue(string label, string value)
    {
        return string.IsNullOrWhiteSpace(value) ? string.Empty : label + " " + value.Trim();
    }

    private static string JoinNonEmpty(string[] values, string separator, string fallback)
    {
        if (values == null || values.Length == 0)
        {
            return fallback;
        }

        int count = 0;
        for (int i = 0; i < values.Length; i++)
        {
            if (!string.IsNullOrWhiteSpace(values[i]))
            {
                count++;
            }
        }

        if (count == 0)
        {
            return fallback;
        }

        string[] compact = new string[count];
        int writeIndex = 0;
        for (int i = 0; i < values.Length; i++)
        {
            if (!string.IsNullOrWhiteSpace(values[i]))
            {
                compact[writeIndex++] = values[i].Trim();
            }
        }

        return string.Join(separator, compact);
    }
}
