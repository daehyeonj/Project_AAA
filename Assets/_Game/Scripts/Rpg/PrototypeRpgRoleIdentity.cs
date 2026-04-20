using System;
using System.Collections.Generic;

public static class PrototypeRpgRoleIdentity
{
    public static string BuildRoleIdentityLabel(string roleLabel, string roleTag)
    {
        string baseLabel = string.IsNullOrWhiteSpace(roleLabel) ? BuildRoleLabelFallback(roleTag) : roleLabel.Trim();
        string fantasyLabel = BuildFantasyLabel(roleTag);
        return string.IsNullOrEmpty(fantasyLabel) ? baseLabel : baseLabel + " " + fantasyLabel;
    }

    public static string BuildFantasyLabel(string roleTag)
    {
        switch (Normalize(roleTag))
        {
            case "warrior":
                return "Anchor";

            case "rogue":
                return "Finisher";

            case "mage":
                return "Burst";

            case "cleric":
                return "Sustain";

            default:
                return string.Empty;
        }
    }

    public static string BuildRoleIdentityText(string roleTag)
    {
        switch (Normalize(roleTag))
        {
            case "warrior":
                return "Anchor: holds the line and converts ATK/DEF gear into reliable damage.";

            case "rogue":
                return "Finisher: acts early and turns ATK/SPD gear into kill pressure.";

            case "mage":
                return "Burst: converts setup turns into high skill damage.";

            case "cleric":
                return "Sustain: keeps the party alive and makes recovery routes safer.";

            default:
                return "Flexible role in the current party plan.";
        }
    }

    public static string BuildGearPreferenceText(string roleTag)
    {
        switch (Normalize(roleTag))
        {
            case "warrior":
                return "Wants ATK/DEF/HP";

            case "rogue":
                return "Wants ATK/SPD";

            case "mage":
                return "Wants POW/ATK/SPD";

            case "cleric":
                return "Wants HP/DEF";

            default:
                return "Wants balanced gear";
        }
    }

    public static string BuildBattleHintText(string roleTag)
    {
        switch (Normalize(roleTag))
        {
            case "warrior":
                return "Anchor | reliable ATK scaling";

            case "rogue":
                return "Finisher | best on weakened targets";

            case "mage":
                return "Burst | skill preview matters";

            case "cleric":
                return "Sustain | protects party HP";

            default:
                return "Flexible | supports the current turn";
        }
    }

    public static string BuildCommandRoleHintText(string roleTag, string commandKey)
    {
        switch (Normalize(commandKey))
        {
            case "attack":
                switch (Normalize(roleTag))
                {
                    case "warrior":
                        return "Anchor hit: stable damage into the frontline.";
                    case "rogue":
                        return "Finisher window: best when the target is already weak.";
                    case "mage":
                        return "Fallback poke: save the real spike for skill turns.";
                    case "cleric":
                        return "Low-priority chip: stabilize the party before trading hits.";
                }
                break;

            case "skill":
                switch (Normalize(roleTag))
                {
                    case "warrior":
                        return "Anchor payoff: convert steady stats into a safe finish.";
                    case "rogue":
                        return "Finisher burst: cash early tempo into a kill.";
                    case "mage":
                        return "Burst window: preview the spike before committing.";
                    case "cleric":
                        return "Sustain cast: protect recovery before attrition snowballs.";
                }
                break;

            case "move":
                switch (Normalize(roleTag))
                {
                    case "warrior":
                        return "Anchor shift: stay close enough to keep pressure on the front.";
                    case "rogue":
                        return "Finisher shift: move only when it opens the next clean kill.";
                    case "mage":
                        return "Burst shift: protect the backline setup lane.";
                    case "cleric":
                        return "Sustain shift: hold the safer line for the party.";
                }
                break;

            case "end_turn":
                switch (Normalize(roleTag))
                {
                    case "warrior":
                        return "Anchor pass: only when the line is already stable.";
                    case "rogue":
                        return "Finisher pass: usually better after missing the kill window.";
                    case "mage":
                        return "Burst pass: hold if the setup turn is not ready.";
                    case "cleric":
                        return "Sustain pass: avoid it when the party still needs recovery.";
                }
                break;
        }

        return BuildBattleHintText(roleTag);
    }

    public static string BuildPartyRoleSummary<T>(T[] members, Func<T, string> nameSelector, Func<T, string> roleSelector)
    {
        if (members == null || members.Length <= 0 || nameSelector == null || roleSelector == null)
        {
            return "Party roles are pending.";
        }

        List<string> parts = new List<string>(members.Length);
        for (int i = 0; i < members.Length; i++)
        {
            T member = members[i];
            if (member == null)
            {
                continue;
            }

            string displayName = Clean(nameSelector(member), "Party");
            switch (Normalize(roleSelector(member)))
            {
                case "warrior":
                    parts.Add(displayName + " anchors pressure");
                    break;

                case "rogue":
                    parts.Add(displayName + " finishes weak targets");
                    break;

                case "mage":
                    parts.Add(displayName + " bursts groups");
                    break;

                case "cleric":
                    parts.Add(displayName + " stabilizes sustain");
                    break;
            }
        }

        return parts.Count > 0 ? string.Join(" | ", parts.ToArray()) : "Party roles are pending.";
    }

    public static string BuildRouteRoleFitSummary<T>(
        string routeId,
        string riskLabel,
        T[] members,
        Func<T, string> nameSelector,
        Func<T, string> roleSelector)
    {
        string warriorName = ResolveMemberName(members, nameSelector, roleSelector, "warrior");
        string rogueName = ResolveMemberName(members, nameSelector, roleSelector, "rogue");
        string mageName = ResolveMemberName(members, nameSelector, roleSelector, "mage");
        string clericName = ResolveMemberName(members, nameSelector, roleSelector, "cleric");

        switch (ResolveRouteMode(routeId, riskLabel))
        {
            case "safe":
                return "Role fit: " + warriorName + "/" + clericName + " stabilize safer recovery lines.";

            case "risky":
                return "Role fit: " + rogueName + "/" + mageName + " want shorter fights and faster payoffs.";

            default:
                return "Role fit: " + warriorName + " holds while " + rogueName + "/" + mageName + " close, with " + clericName + " covering attrition.";
        }
    }

    public static string BuildEquipmentFitText(string displayName, string roleTag, PrototypeRpgEquipmentDefinition definition)
    {
        if (definition == null)
        {
            return "Fit: No item selected.";
        }

        string subject = Clean(displayName, BuildRoleLabelFallback(roleTag));
        int score = GetEquipmentRoleFitScore(roleTag, definition);
        if (score <= 0)
        {
            return "Fit: Weak for " + subject + "; no " + BuildPreferredStatLine(roleTag).ToLowerInvariant() + " bonus.";
        }

        string strength = score >= 8
            ? "Strong"
            : score >= 4
                ? "Solid"
                : "Workable";
        return "Fit: " + strength + " for " + subject + "; " + BuildEquipmentFitReason(roleTag, definition);
    }

    public static string BuildGrowthMeaningText(string displayName, string roleTag, int maxHpBonus, int attackBonus, int defenseBonus, int speedBonus, bool leveledUp)
    {
        string gainText = BuildGrowthGainText(roleTag, maxHpBonus, attackBonus, defenseBonus, speedBonus, leveledUp);
        string meaningText = BuildGrowthMeaningClause(roleTag, maxHpBonus, attackBonus, defenseBonus, speedBonus, leveledUp);
        string subject = Clean(displayName, BuildRoleLabelFallback(roleTag));
        return subject + " gained " + gainText + ": " + meaningText;
    }

    public static string BuildGrowthMeaningClause(string roleTag, int maxHpBonus, int attackBonus, int defenseBonus, int speedBonus, bool leveledUp)
    {
        switch (Normalize(roleTag))
        {
            case "warrior":
                if (defenseBonus > 0 || maxHpBonus > 0)
                {
                    return "anchor survival improved";
                }

                if (attackBonus > 0)
                {
                    return "reliable frontline damage improved";
                }

                if (speedBonus > 0)
                {
                    return "frontline tempo improved";
                }
                break;

            case "rogue":
                if (speedBonus > 0)
                {
                    return "finisher turns come online earlier";
                }

                if (attackBonus > 0)
                {
                    return "kill pressure improved";
                }

                if (maxHpBonus > 0 || defenseBonus > 0)
                {
                    return "cleanup turns became safer";
                }
                break;

            case "mage":
                if (attackBonus > 0)
                {
                    return "burst preview increased";
                }

                if (speedBonus > 0)
                {
                    return "burst setup lands earlier";
                }

                if (maxHpBonus > 0 || defenseBonus > 0)
                {
                    return "backline burst safety improved";
                }
                break;

            case "cleric":
                if (maxHpBonus > 0)
                {
                    return "sustain safety improved";
                }

                if (defenseBonus > 0)
                {
                    return "recovery lines became sturdier";
                }

                if (speedBonus > 0)
                {
                    return "support turns happen sooner";
                }

                if (attackBonus > 0)
                {
                    return "support pressure improved";
                }
                break;
        }

        return leveledUp ? "overall role output improved" : string.Empty;
    }

    public static string BuildNextRunMeaningText(string displayName, string roleTag, int maxHpBonus, int attackBonus, int defenseBonus, int speedBonus, bool leveledUp)
    {
        string subject = Clean(displayName, BuildRoleLabelFallback(roleTag));
        switch (Normalize(roleTag))
        {
            case "warrior":
                if (defenseBonus > 0 || maxHpBonus > 0)
                {
                    return "Next run: " + subject + " holds the frontline more safely.";
                }

                if (attackBonus > 0)
                {
                    return "Next run: " + subject + " keeps anchor damage online more reliably.";
                }
                break;

            case "rogue":
                if (speedBonus > 0)
                {
                    return "Next run: " + subject + " reaches finisher windows earlier.";
                }

                if (attackBonus > 0)
                {
                    return "Next run: " + subject + " closes weak targets faster.";
                }
                break;

            case "mage":
                if (attackBonus > 0 || speedBonus > 0)
                {
                    return "Next run: " + subject + "'s burst turn spikes harder.";
                }
                break;

            case "cleric":
                if (maxHpBonus > 0 || defenseBonus > 0)
                {
                    return "Next run: " + subject + " keeps recovery lines safer.";
                }

                if (speedBonus > 0)
                {
                    return "Next run: " + subject + " stabilizes the party earlier.";
                }
                break;
        }

        return leveledUp
            ? "Next run: " + subject + " brings stronger role output."
            : string.Empty;
    }

    private static string BuildGrowthGainText(string roleTag, int maxHpBonus, int attackBonus, int defenseBonus, int speedBonus, bool leveledUp)
    {
        List<string> parts = new List<string>(2);
        switch (Normalize(roleTag))
        {
            case "warrior":
                AppendGrowthPart(parts, "DEF", defenseBonus);
                AppendGrowthPart(parts, "HP", maxHpBonus);
                AppendGrowthPart(parts, "ATK", attackBonus);
                AppendGrowthPart(parts, "SPD", speedBonus);
                break;

            case "rogue":
                AppendGrowthPart(parts, "SPD", speedBonus);
                AppendGrowthPart(parts, "ATK", attackBonus);
                AppendGrowthPart(parts, "HP", maxHpBonus);
                AppendGrowthPart(parts, "DEF", defenseBonus);
                break;

            case "mage":
                AppendGrowthPart(parts, "ATK", attackBonus);
                AppendGrowthPart(parts, "SPD", speedBonus);
                AppendGrowthPart(parts, "HP", maxHpBonus);
                AppendGrowthPart(parts, "DEF", defenseBonus);
                break;

            case "cleric":
                AppendGrowthPart(parts, "HP", maxHpBonus);
                AppendGrowthPart(parts, "DEF", defenseBonus);
                AppendGrowthPart(parts, "SPD", speedBonus);
                AppendGrowthPart(parts, "ATK", attackBonus);
                break;

            default:
                AppendGrowthPart(parts, "HP", maxHpBonus);
                AppendGrowthPart(parts, "ATK", attackBonus);
                AppendGrowthPart(parts, "DEF", defenseBonus);
                AppendGrowthPart(parts, "SPD", speedBonus);
                break;
        }

        if (parts.Count > 0)
        {
            return string.Join(" / ", parts.ToArray());
        }

        return leveledUp ? "overall growth" : "steady XP";
    }

    private static void AppendGrowthPart(List<string> parts, string label, int value)
    {
        if (parts == null || value <= 0 || parts.Count >= 2)
        {
            return;
        }

        parts.Add(label + " +" + value);
    }

    private static string BuildEquipmentFitReason(string roleTag, PrototypeRpgEquipmentDefinition definition)
    {
        switch (Normalize(roleTag))
        {
            case "warrior":
                if (definition.AttackBonus > 0 && (definition.DefenseBonus > 0 || definition.MaxHpBonus > 0))
                {
                    return "ATK plus DEF/HP supports anchor damage and survival.";
                }

                if (definition.AttackBonus > 0)
                {
                    return "ATK keeps reliable frontline damage online.";
                }

                return "DEF/HP padding keeps the frontline standing.";

            case "rogue":
                if (definition.AttackBonus > 0 && definition.SpeedBonus > 0)
                {
                    return "ATK plus SPD supports early finisher turns.";
                }

                if (definition.SpeedBonus > 0)
                {
                    return "SPD helps the finisher act before targets stabilize.";
                }

                return "ATK keeps cleanup pressure sharp.";

            case "mage":
                if (definition.SkillPowerBonus > 0)
                {
                    return "POW boosts burst turns and preview spikes.";
                }

                if (definition.AttackBonus > 0 && definition.SpeedBonus > 0)
                {
                    return "ATK plus SPD helps burst setup land earlier.";
                }

                return definition.AttackBonus > 0
                    ? "ATK still raises burst payoff."
                    : "SPD helps burst setup timing.";

            case "cleric":
                if (definition.MaxHpBonus > 0 && definition.DefenseBonus > 0)
                {
                    return "HP plus DEF supports party sustain and recovery.";
                }

                if (definition.MaxHpBonus > 0)
                {
                    return "HP keeps sustain windows safer.";
                }

                return "DEF helps the support line survive attrition.";

            default:
                return "It reinforces the current role's best stats.";
        }
    }

    private static int GetEquipmentRoleFitScore(string roleTag, PrototypeRpgEquipmentDefinition definition)
    {
        if (definition == null)
        {
            return 0;
        }

        switch (Normalize(roleTag))
        {
            case "warrior":
                return (definition.AttackBonus * 3) + (definition.DefenseBonus * 3) + (definition.MaxHpBonus * 2) + definition.SpeedBonus;

            case "rogue":
                return (definition.AttackBonus * 3) + (definition.SpeedBonus * 3) + definition.MaxHpBonus + definition.DefenseBonus;

            case "mage":
                return (definition.SkillPowerBonus * 4) + (definition.AttackBonus * 2) + (definition.SpeedBonus * 2) + definition.MaxHpBonus;

            case "cleric":
                return (definition.MaxHpBonus * 3) + (definition.DefenseBonus * 3) + definition.SpeedBonus + definition.AttackBonus;

            default:
                return definition.GetScore();
        }
    }

    private static string BuildPreferredStatLine(string roleTag)
    {
        switch (Normalize(roleTag))
        {
            case "warrior":
                return "ATK/DEF/HP";

            case "rogue":
                return "ATK/SPD";

            case "mage":
                return "POW/ATK/SPD";

            case "cleric":
                return "HP/DEF";

            default:
                return "primary role";
        }
    }

    private static string ResolveRouteMode(string routeId, string riskLabel)
    {
        string combined = Normalize(routeId) + "|" + Normalize(riskLabel);
        if (ContainsAny(combined, "safe", "steady", "rest", "guarded", "recover", "low"))
        {
            return "safe";
        }

        if (ContainsAny(combined, "risky", "greedy", "volatile", "high", "breach", "crash"))
        {
            return "risky";
        }

        return "balanced";
    }

    private static bool ContainsAny(string value, params string[] tokens)
    {
        if (string.IsNullOrEmpty(value) || tokens == null)
        {
            return false;
        }

        for (int i = 0; i < tokens.Length; i++)
        {
            if (!string.IsNullOrEmpty(tokens[i]) && value.IndexOf(tokens[i], StringComparison.OrdinalIgnoreCase) >= 0)
            {
                return true;
            }
        }

        return false;
    }

    private static string ResolveMemberName<T>(T[] members, Func<T, string> nameSelector, Func<T, string> roleSelector, string targetRoleTag)
    {
        if (members != null && nameSelector != null && roleSelector != null)
        {
            for (int i = 0; i < members.Length; i++)
            {
                T member = members[i];
                if (member == null || Normalize(roleSelector(member)) != targetRoleTag)
                {
                    continue;
                }

                string displayName = Clean(nameSelector(member), string.Empty);
                if (!string.IsNullOrEmpty(displayName))
                {
                    return displayName;
                }
            }
        }

        return BuildRoleLabelFallback(targetRoleTag);
    }

    private static string BuildRoleLabelFallback(string roleTag)
    {
        switch (Normalize(roleTag))
        {
            case "warrior":
                return "Warrior";
            case "rogue":
                return "Rogue";
            case "mage":
                return "Mage";
            case "cleric":
                return "Cleric";
            default:
                return "Adventurer";
        }
    }

    private static string Clean(string value, string fallback)
    {
        return string.IsNullOrWhiteSpace(value) ? fallback : value.Trim();
    }

    private static string Normalize(string value)
    {
        return string.IsNullOrWhiteSpace(value) ? string.Empty : value.Trim().ToLowerInvariant();
    }
}
