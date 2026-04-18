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
    public string PendingRewardSummaryText = string.Empty;
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
    public int Level = 1;
    public int CurrentExperience;
    public int NextLevelExperience = 18;
    public int GrowthBonusMaxHp;
    public int GrowthBonusAttack;
    public int GrowthBonusDefense;
    public int GrowthBonusSpeed;
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

        for (int i = 0; i < members.Length; i++)
        {
            PrototypeRpgPartyMemberDefinition memberDefinition = partyDefinition.Members[i];
            string resolvedLoadoutId = equipmentLoadoutResolver != null && memberDefinition != null
                ? equipmentLoadoutResolver(memberDefinition)
                : null;
            members[i] = BuildMemberSurface(
                memberDefinition,
                resolvedLoadoutId,
                surface.ArchetypeId,
                surface.PromotionStateId);
        }

        surface.Members = members;
        RefreshPartySummaryTexts(surface);
        return surface;
    }

    public static PrototypeRpgMemberRuntimeResolveSurface BuildMemberSurface(
        PrototypeRpgPartyMemberDefinition memberDefinition,
        string resolvedEquipmentLoadoutId = null,
        string partyArchetypeId = "",
        string promotionStateId = "")
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
        RefreshMemberSummaryTexts(surface, partyArchetypeId, promotionStateId);
        return surface;
    }

    public static void RefreshPartySummaryTexts(PrototypeRpgPartyRuntimeResolveSurface surface)
    {
        if (surface == null)
        {
            return;
        }

        string[] appliedParts = new string[surface.Members != null ? surface.Members.Length : 0];
        string[] currentRunParts = new string[appliedParts.Length];
        string[] nextRunParts = new string[appliedParts.Length];
        int highestLevel = 1;
        int totalLevel = 0;
        int memberCount = 0;

        for (int i = 0; i < appliedParts.Length; i++)
        {
            PrototypeRpgMemberRuntimeResolveSurface member = surface.Members[i];
            if (member == null)
            {
                continue;
            }

            appliedParts[i] = member.DisplayName + ": " + member.AppliedProgressionSummaryText;
            currentRunParts[i] = member.DisplayName + ": " + member.CurrentRunSummaryText;
            nextRunParts[i] = member.DisplayName + ": " + member.NextRunPreviewSummaryText;
            highestLevel = Mathf.Max(highestLevel, member.Level > 0 ? member.Level : 1);
            totalLevel += member.Level > 0 ? member.Level : 1;
            memberCount += 1;
        }

        string memberAppliedText = JoinNonEmpty(appliedParts, " | ", string.Empty);
        string memberCurrentRunText = JoinNonEmpty(currentRunParts, " | ", string.Empty);
        string memberNextRunText = JoinNonEmpty(nextRunParts, " | ", string.Empty);
        string levelRailText = BuildPartyLevelRailSummaryText(highestLevel, totalLevel, memberCount);
        surface.AppliedProgressionSummaryText = BuildPartyAppliedProgressionSummary(surface, memberAppliedText, levelRailText);
        surface.CurrentRunSummaryText = BuildPartyCurrentRunSummary(surface, memberCurrentRunText, levelRailText);
        surface.NextRunPreviewSummaryText = BuildPartyNextRunPreviewSummary(surface, memberNextRunText, levelRailText);
    }

    public static void RefreshMemberSummaryTexts(
        PrototypeRpgMemberRuntimeResolveSurface surface,
        string partyArchetypeId,
        string promotionStateId)
    {
        if (surface == null)
        {
            return;
        }

        surface.AppliedProgressionSummaryText = BuildAppliedProgressionSummary(surface, partyArchetypeId, promotionStateId);
        surface.CurrentRunSummaryText = BuildCurrentRunSummary(surface, partyArchetypeId);
        surface.NextRunPreviewSummaryText = BuildNextRunPreviewSummary(surface, partyArchetypeId, promotionStateId);
    }

    private static string BuildPartyAppliedProgressionSummary(PrototypeRpgPartyRuntimeResolveSurface surface, string memberAppliedText, string levelRailText)
    {
        return JoinNonEmpty(
            new[]
            {
                BuildLabelValue("Party", JoinNonEmpty(
                    new[]
                    {
                        surface != null ? surface.ArchetypeLabel : string.Empty,
                        surface != null ? surface.PromotionStateLabel : string.Empty
                    },
                    " / ",
                    string.Empty)),
                BuildLabelValue("Level Rail", levelRailText),
                BuildLabelValue("Doctrine", surface != null ? surface.DoctrineSummaryText : string.Empty),
                BuildLabelValue("Growth", surface != null ? surface.PromotionSummaryText : string.Empty),
                memberAppliedText
            },
            " | ",
            "No applied progression.");
    }

    private static string BuildPartyCurrentRunSummary(PrototypeRpgPartyRuntimeResolveSurface surface, string memberCurrentRunText, string levelRailText)
    {
        return JoinNonEmpty(
            new[]
            {
                surface != null ? surface.StrengthSummaryText : string.Empty,
                BuildLabelValue("Battle Plan", BuildPartyTacticalLoopText(surface != null ? surface.ArchetypeId : string.Empty)),
                BuildLabelValue("Level Rail", levelRailText),
                BuildLabelValue("Power", surface != null ? surface.DerivedPower.ToString() : string.Empty),
                BuildLabelValue("Carry", surface != null ? surface.DerivedCarryCapacity.ToString() : string.Empty),
                memberCurrentRunText
            },
            " | ",
            "No runtime party.");
    }

    private static string BuildPartyNextRunPreviewSummary(PrototypeRpgPartyRuntimeResolveSurface surface, string memberNextRunText, string levelRailText)
    {
        return JoinNonEmpty(
            new[]
            {
                surface != null ? surface.RouteFitSummaryText : string.Empty,
                BuildLabelValue("Next Edge", BuildPartyPromotionPreviewText(
                    surface != null ? surface.ArchetypeId : string.Empty,
                    surface != null ? surface.PromotionStateId : string.Empty)),
                BuildLabelValue("Level Rail", levelRailText),
                BuildLabelValue("Stash", surface != null ? surface.PendingRewardSummaryText : string.Empty),
                memberNextRunText
            },
            " | ",
            "No next-run preview.");
    }

    private static string BuildAppliedProgressionSummary(
        PrototypeRpgMemberRuntimeResolveSurface surface,
        string partyArchetypeId,
        string promotionStateId)
    {
        return JoinNonEmpty(
            new[]
            {
                BuildLabelValue("Level", BuildMemberLevelProgressText(surface)),
                BuildLabelValue("Role", BuildMemberRoleFocusText(surface != null ? surface.RoleTag : string.Empty, partyArchetypeId)),
                BuildLabelValue("Loadout", surface != null ? surface.EquipmentSummaryText : string.Empty),
                BuildLabelValue("Skill", surface != null ? surface.ResolvedSkillName : string.Empty),
                BuildLabelValue("Growth", BuildMemberPromotionEdgeText(
                    surface != null ? surface.RoleTag : string.Empty,
                    partyArchetypeId,
                    promotionStateId,
                    surface))
            },
            " | ",
            "No applied progression.");
    }

    private static string BuildCurrentRunSummary(PrototypeRpgMemberRuntimeResolveSurface surface, string partyArchetypeId)
    {
        return JoinNonEmpty(
            new[]
            {
                BuildLabelValue("Battle Role", BuildMemberBattleRoleText(
                    surface != null ? surface.RoleTag : string.Empty,
                    partyArchetypeId,
                    surface != null ? surface.ResolvedSkillName : string.Empty)),
                BuildLabelValue("Level", BuildMemberLevelProgressText(surface)),
                BuildLabelValue("Stats", BuildMemberStatSummary(surface)),
                BuildLabelValue("Gear Edge", surface != null ? surface.GearContributionSummaryText : string.Empty)
            },
            " | ",
            "No current-run summary.");
    }

    private static string BuildNextRunPreviewSummary(
        PrototypeRpgMemberRuntimeResolveSurface surface,
        string partyArchetypeId,
        string promotionStateId)
    {
        return JoinNonEmpty(
            new[]
            {
                BuildLabelValue("Next Dispatch", BuildMemberNextDispatchText(
                    surface != null ? surface.RoleTag : string.Empty,
                    partyArchetypeId,
                    promotionStateId)),
                BuildLabelValue("Next Level", BuildMemberNextLevelHintText(surface)),
                BuildLabelValue("Carry Forward", JoinNonEmpty(
                    new[]
                    {
                        surface != null ? surface.ResolvedSkillName : string.Empty,
                        surface != null ? surface.EquipmentSummaryText : string.Empty
                    },
                    " / ",
                    string.Empty))
            },
            " | ",
            "No next-run preview.");
    }

    private static string BuildLabelValue(string label, string value)
    {
        return string.IsNullOrWhiteSpace(value) ? string.Empty : label + " " + value.Trim();
    }

    private static string BuildPartyTacticalLoopText(string archetypeId)
    {
        switch (NormalizeKey(archetypeId))
        {
            case "outrider":
                return "Open the burst window fast, cash it with the finisher, then let the mage sweep exposed packs while support keeps tempo alive.";

            case "salvager":
                return "Stabilize first, protect the cleaner haul, and turn steady clears into better carry plus safer follow-up.";

            default:
                return "Hold the frontline, keep attrition stable, and let the backline close once the restart line is safe.";
        }
    }

    private static string BuildPartyPromotionPreviewText(string archetypeId, string promotionStateId)
    {
        string normalizedArchetypeId = NormalizeKey(archetypeId);
        switch (NormalizeKey(promotionStateId))
        {
            case "elite":
                return normalizedArchetypeId == "outrider"
                    ? "Elite burst pacing now supports the highest-pressure shard answer."
                    : normalizedArchetypeId == "salvager"
                        ? "Elite haul discipline can justify longer balanced routes without losing recovery."
                        : "Elite frontline stability can absorb the harder middle answer without giving up the safer reset.";

            case "field":
                return normalizedArchetypeId == "outrider"
                    ? "Field promotion makes the risky route a cleaner answer when pressure turns urgent."
                    : normalizedArchetypeId == "salvager"
                        ? "Field promotion turns balanced routes into better carry conversions for the next city answer."
                        : "Field promotion makes safer or balanced recovery answers easier to commit to on the next dispatch.";

            default:
                return normalizedArchetypeId == "outrider"
                    ? "Recruit pacing still wants city readiness before fully leaning into the risky answer."
                    : normalizedArchetypeId == "salvager"
                        ? "Recruit salvagers already fit balanced routes, but the real carry payoff still needs a clean return."
                        : "Recruit bulwarks already fit safer recovery lines while the first promotion is still pending.";
        }
    }

    private static string BuildMemberRoleFocusText(string roleTag, string archetypeId)
    {
        string normalizedArchetypeId = NormalizeKey(archetypeId);
        switch (NormalizeKey(roleTag))
        {
            case "warrior":
                return normalizedArchetypeId == "outrider"
                    ? "Burst opener that starts the pressure line."
                    : normalizedArchetypeId == "salvager"
                        ? "Carry anchor that keeps the haul line standing."
                        : "Frontline anchor that protects the safer restart line.";

            case "rogue":
                return normalizedArchetypeId == "outrider"
                    ? "Payoff finisher that cashes exposed targets fast."
                    : normalizedArchetypeId == "salvager"
                        ? "Clean finisher that preserves the steadier haul."
                        : "Controlled finisher that closes once the front is stable.";

            case "mage":
                return normalizedArchetypeId == "outrider"
                    ? "Sweep caster that punishes exposed packs."
                    : normalizedArchetypeId == "salvager"
                        ? "Control caster that keeps balanced fights from dragging."
                        : "Backline closer that follows the stable frontline.";

            case "cleric":
                return normalizedArchetypeId == "outrider"
                    ? "Tempo support that keeps the burst line online."
                    : normalizedArchetypeId == "salvager"
                        ? "Recovery support that protects clean returns."
                        : "Stability support that keeps the restart line alive.";

            default:
                return "Flexible role in the current party plan.";
        }
    }

    private static string BuildMemberPromotionEdgeText(
        string roleTag,
        string archetypeId,
        string promotionStateId,
        PrototypeRpgMemberRuntimeResolveSurface surface)
    {
        string gearName = surface != null && !string.IsNullOrWhiteSpace(surface.EquipmentSummaryText)
            ? surface.EquipmentSummaryText
            : "current gear";
        string growthBonusText = PrototypeRpgMemberProgressionRules.BuildGrowthBonusSummaryText(
            surface != null ? surface.GrowthBonusMaxHp : 0,
            surface != null ? surface.GrowthBonusAttack : 0,
            surface != null ? surface.GrowthBonusDefense : 0,
            surface != null ? surface.GrowthBonusSpeed : 0);
        string levelText = BuildMemberLevelProgressText(surface);
        string normalizedPromotionStateId = NormalizeKey(promotionStateId);
        if (normalizedPromotionStateId == "elite")
        {
            return levelText + " | " + growthBonusText + " | " + gearName + " and veteran pacing keep this role online on the hardest answer.";
        }

        if (normalizedPromotionStateId == "field")
        {
            return levelText + " | " + growthBonusText + " | " + gearName + " sharpens this role enough to matter on the next committed route.";
        }

        switch (NormalizeKey(roleTag))
        {
            case "warrior":
                return levelText + " | " + growthBonusText + " | Recruit frame is still building a tougher frontline answer.";
            case "rogue":
                return levelText + " | " + growthBonusText + " | Recruit frame is still sharpening the finisher timing.";
            case "mage":
                return levelText + " | " + growthBonusText + " | Recruit frame is still proving the sweep turn.";
            case "cleric":
                return levelText + " | " + growthBonusText + " | Recruit frame is still stabilizing the sustain answer.";
            default:
                return levelText + " | " + growthBonusText + " | Recruit frame is still looking for its first clean return.";
        }
    }

    private static string BuildMemberBattleRoleText(string roleTag, string archetypeId, string skillName)
    {
        string resolvedSkillName = string.IsNullOrWhiteSpace(skillName) ? "Skill" : skillName.Trim();
        string rolePlan = NormalizeKey(roleTag) switch
        {
            "warrior" => "open or hold the frontline",
            "rogue" => "cash the payoff kill",
            "mage" => "sweep or close the board",
            "cleric" => "stabilize the party plan",
            _ => "support the current turn plan"
        };

        if (NormalizeKey(archetypeId) == "outrider")
        {
            rolePlan = NormalizeKey(roleTag) switch
            {
                "warrior" => "open the burst window",
                "rogue" => "cash the exposed target",
                "mage" => "punish the exposed pack",
                "cleric" => "keep burst tempo alive",
                _ => rolePlan
            };
        }

        return resolvedSkillName + " keeps this role focused on " + rolePlan + ".";
    }

    private static string BuildMemberStatSummary(PrototypeRpgMemberRuntimeResolveSurface surface)
    {
        if (surface == null)
        {
            return string.Empty;
        }

        return "HP " + surface.MaxHp + " | ATK " + surface.Attack + " | DEF " + surface.Defense + " | SPD " + surface.Speed;
    }

    private static string BuildMemberLevelProgressText(PrototypeRpgMemberRuntimeResolveSurface surface)
    {
        return surface == null
            ? string.Empty
            : PrototypeRpgMemberProgressionRules.BuildLevelProgressText(
                surface.Level,
                surface.CurrentExperience,
                surface.NextLevelExperience);
    }

    private static string BuildMemberNextLevelHintText(PrototypeRpgMemberRuntimeResolveSurface surface)
    {
        return surface == null
            ? string.Empty
            : PrototypeRpgMemberProgressionRules.BuildNextLevelHintText(
                surface.Level,
                surface.CurrentExperience,
                surface.NextLevelExperience);
    }

    private static string BuildPartyLevelRailSummaryText(int highestLevel, int totalLevel, int memberCount)
    {
        int safeHighestLevel = highestLevel > 0 ? highestLevel : 1;
        int safeMemberCount = memberCount > 0 ? memberCount : 0;
        string averageText = safeMemberCount > 0
            ? (totalLevel / (float)safeMemberCount).ToString("0.0")
            : "1.0";
        return "Peak Lv " + safeHighestLevel + " | Avg Lv " + averageText;
    }

    private static string BuildMemberNextDispatchText(string roleTag, string archetypeId, string promotionStateId)
    {
        string normalizedArchetypeId = NormalizeKey(archetypeId);
        string normalizedPromotionStateId = NormalizeKey(promotionStateId);
        switch (NormalizeKey(roleTag))
        {
            case "warrior":
                if (normalizedArchetypeId == "outrider")
                {
                    return normalizedPromotionStateId == "recruit"
                        ? "Open the risky pressure line only if the city can absorb a swingy opener."
                        : "Start the risky pressure line early so the payoff party can spike before attrition sticks.";
                }

                if (normalizedArchetypeId == "salvager")
                {
                    return normalizedPromotionStateId == "elite"
                        ? "Anchor a longer balanced haul without giving up carry stability."
                        : "Keep the balanced haul line standing so cleaner returns still convert.";
                }

                return normalizedPromotionStateId == "elite"
                    ? "Hold either safe or balanced recovery answers without risking a collapse."
                    : "Hold the safe restart line while the city recovery answer stays online.";

            case "rogue":
                if (normalizedArchetypeId == "outrider")
                {
                    return normalizedPromotionStateId == "elite"
                        ? "Cash the greedy route payoff on purpose once the target is exposed."
                        : "Turn exposed targets into the faster shard answer on the next risky dispatch.";
                }

                if (normalizedArchetypeId == "salvager")
                {
                    return "Close cleaner fights so balanced routes keep their haul value.";
                }

                return "Finish stable fights without forcing the safer route into a risky overcommit.";

            case "mage":
                if (normalizedArchetypeId == "outrider")
                {
                    return "Sweep exposed packs once the burst window is open.";
                }

                if (normalizedArchetypeId == "salvager")
                {
                    return "Keep balanced routes from dragging long enough to threaten recovery.";
                }

                return "Close the board after the frontline absorbs the safer push.";

            case "cleric":
                if (normalizedArchetypeId == "outrider")
                {
                    return normalizedPromotionStateId == "elite"
                        ? "Keep the burst line alive long enough to justify the hardest pressure answer."
                        : "Extend the burst line so the risky route can finish before attrition snowballs.";
                }

                if (normalizedArchetypeId == "salvager")
                {
                    return "Protect survivors so steadier haul routes remain worth repeating.";
                }

                return "Preserve restart stability so safer recovery routes stay the sensible answer.";

            default:
                return "Carry this role cleanly into the next dispatch.";
        }
    }

    private static string NormalizeKey(string value)
    {
        return string.IsNullOrWhiteSpace(value) ? string.Empty : value.Trim().ToLowerInvariant();
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
