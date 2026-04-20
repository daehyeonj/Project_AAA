using UnityEngine;

public sealed partial class StaticPlaceholderWorldView
{
    private PrototypeRpgPartyDefinition BuildRuntimePartyDefinition(string partyId)
    {
        if (string.IsNullOrEmpty(partyId))
        {
            return null;
        }

        string displayName = _runtimeEconomyState != null ? _runtimeEconomyState.GetPartyDisplayName(partyId) : string.Empty;
        string archetypeId = _runtimeEconomyState != null ? _runtimeEconomyState.GetPartyArchetypeId(partyId) : string.Empty;
        string promotionStateId = _runtimeEconomyState != null ? _runtimeEconomyState.GetPartyPromotionStateId(partyId) : string.Empty;
        return string.IsNullOrEmpty(archetypeId) && string.IsNullOrEmpty(promotionStateId)
            ? PrototypeRpgPartyCatalog.CreateDefaultPlaceholderParty(partyId)
            : PrototypeRpgPartyCatalog.CreateRuntimeParty(partyId, archetypeId, promotionStateId, displayName);
    }

    private PrototypeRpgPartyRuntimeResolveSurface BuildRuntimePartyResolveSurface(string partyId)
    {
        PrototypeRpgPartyDefinition partyDefinition = BuildRuntimePartyDefinition(partyId);
        PrototypeRpgPartyRuntimeResolveSurface partySurface = partyDefinition == null
            ? null
            : PrototypeRpgRuntimeResolveBuilder.BuildPartySurface(
                partyDefinition,
                memberDefinition => memberDefinition != null ? memberDefinition.EquipmentLoadoutId : string.Empty,
                false);
        ApplyRuntimePartyProgressionSurface(partySurface, partyId);
        return partySurface;
    }

    private void ApplyRuntimePartyProgressionSurface(PrototypeRpgPartyRuntimeResolveSurface partySurface, string partyId)
    {
        if (partySurface == null)
        {
            return;
        }

        if (_runtimeEconomyState != null)
        {
            int resolvedPower = _runtimeEconomyState.GetPartyPower(partyId);
            int resolvedCarryCapacity = _runtimeEconomyState.GetPartyCarryCapacity(partyId);
            if (resolvedPower > 0)
            {
                partySurface.DerivedPower = resolvedPower;
            }

            if (resolvedCarryCapacity > 0)
            {
                partySurface.DerivedCarryCapacity = resolvedCarryCapacity;
            }

            partySurface.PendingRewardSummaryText = _runtimeEconomyState.GetPartyPendingRewardSummary(partyId);
        }

        PrototypeRpgMemberRuntimeResolveSurface[] members = partySurface.Members ?? System.Array.Empty<PrototypeRpgMemberRuntimeResolveSurface>();
        for (int i = 0; i < members.Length; i++)
        {
            PrototypeRpgMemberRuntimeResolveSurface member = members[i];
            if (member == null || _runtimeEconomyState == null)
            {
                continue;
            }

            int level = _runtimeEconomyState.GetPartyMemberLevel(partyId, member.MemberId);
            int currentExperience = _runtimeEconomyState.GetPartyMemberExperience(partyId, member.MemberId);
            PrototypeRpgLevelStatBonus growthBonus = PrototypeRpgMemberProgressionRules.ResolveLevelStatBonus(
                member.RoleTag,
                partySurface.ArchetypeId,
                level);

            member.Level = level > 0 ? level : PrototypeRpgMemberProgressionRules.GetStartingLevel();
            member.CurrentExperience = currentExperience > 0 ? currentExperience : PrototypeRpgMemberProgressionRules.GetStartingExperience();
            member.NextLevelExperience = PrototypeRpgMemberProgressionRules.GetNextLevelExperience(member.Level);
            member.GrowthBonusMaxHp = growthBonus.MaxHpBonus;
            member.GrowthBonusAttack = growthBonus.AttackBonus;
            member.GrowthBonusDefense = growthBonus.DefenseBonus;
            member.GrowthBonusSpeed = growthBonus.SpeedBonus;
            ApplyRuntimeMemberEquipmentSurface(member, partyId);
            member.MaxHp = Mathf.Max(1, member.MaxHp + growthBonus.MaxHpBonus);
            member.Attack = Mathf.Max(1, member.Attack + growthBonus.AttackBonus);
            member.Defense = Mathf.Max(0, member.Defense + growthBonus.DefenseBonus);
            member.Speed = Mathf.Max(0, member.Speed + growthBonus.SpeedBonus);
            member.SkillPower = Mathf.Max(1, member.SkillPower + growthBonus.AttackBonus);
            PrototypeRpgRuntimeResolveBuilder.RefreshMemberSummaryTexts(
                member,
                partySurface.ArchetypeId,
                partySurface.PromotionStateId);
        }

        PrototypeRpgRuntimeResolveBuilder.RefreshPartySummaryTexts(partySurface);
    }

    private void ApplyRuntimeMemberEquipmentSurface(PrototypeRpgMemberRuntimeResolveSurface member, string partyId)
    {
        if (member == null || _runtimeEconomyState == null)
        {
            return;
        }

        member.EquipmentLoadoutId = _runtimeEconomyState.GetPartyMemberEquipmentLoadoutId(partyId, member.MemberId);
        member.EquipmentSummaryText = _runtimeEconomyState.GetPartyMemberEquipmentSummary(partyId, member.MemberId);
        member.GearContributionSummaryText = _runtimeEconomyState.GetPartyMemberGearContributionSummary(partyId, member.MemberId);
        member.MaxHp = Mathf.Max(1, member.MaxHp + _runtimeEconomyState.GetPartyMemberEquipmentMaxHpBonus(partyId, member.MemberId));
        member.Attack = Mathf.Max(1, member.Attack + _runtimeEconomyState.GetPartyMemberEquipmentAttackBonus(partyId, member.MemberId));
        member.Defense = Mathf.Max(0, member.Defense + _runtimeEconomyState.GetPartyMemberEquipmentDefenseBonus(partyId, member.MemberId));
        member.Speed = Mathf.Max(0, member.Speed + _runtimeEconomyState.GetPartyMemberEquipmentSpeedBonus(partyId, member.MemberId));
        member.SkillPower = Mathf.Max(1, member.SkillPower + _runtimeEconomyState.GetPartyMemberEquipmentSkillPowerBonus(partyId, member.MemberId));
    }

    private string ResolveRuntimePartyRoleSummary(string partyId)
    {
        if (_runtimeEconomyState != null)
        {
            string cachedRoleSummary = _runtimeEconomyState.GetPartyRoleSummary(partyId);
            if (HasText(cachedRoleSummary) && cachedRoleSummary != "None")
            {
                return cachedRoleSummary;
            }
        }

        PrototypeRpgPartyRuntimeResolveSurface partySurface = BuildRuntimePartyResolveSurface(partyId);
        if (partySurface == null || partySurface.Members == null || partySurface.Members.Length <= 0)
        {
            return "Alden anchors pressure | Mira finishes weak targets | Rune bursts groups | Lia stabilizes sustain";
        }

        return PrototypeRpgRoleIdentity.BuildPartyRoleSummary(
            partySurface.Members,
            member => member != null ? member.DisplayName : string.Empty,
            member => member != null ? member.RoleTag : string.Empty);
    }

    private string BuildRuntimePartyRouteFitText(string partyId, string dungeonId, string routeId)
    {
        PrototypeRpgPartyRuntimeResolveSurface partySurface = BuildRuntimePartyResolveSurface(partyId);
        if (partySurface == null)
        {
            return BuildLightRuntimePartyRouteFitText(partyId, dungeonId, routeId);
        }

        string normalizedRouteId = NormalizeRouteChoiceId(routeId);
        string balancedRouteId = GetBalancedRouteId(dungeonId);
        bool isRiskyRoute = normalizedRouteId == RiskyRouteId;
        bool isSafeRoute = normalizedRouteId == SafeRouteId;
        bool isBalancedRoute = !string.IsNullOrEmpty(balancedRouteId) && normalizedRouteId == balancedRouteId;
        string routeReasonText;

        switch (partySurface.ArchetypeId)
        {
            case "outrider":
                if (isRiskyRoute)
                {
                    routeReasonText = "Riskier pressure lanes are the cleanest fit because this crew wants exposed targets and faster shard conversion.";
                    break;
                }

                if (isSafeRoute)
                {
                    routeReasonText = "Safer recovery lanes keep the crew alive, but they intentionally trade away some of the burst ceiling.";
                    break;
                }

                routeReasonText = "Balanced lanes are playable, but the best payoff still lives on a route that lets the burst line spike first.";
                break;

            case "salvager":
                if (isRiskyRoute)
                {
                    routeReasonText = "Harder lanes are survivable, but the carry plan usually converts more cleanly on steadier runs.";
                    break;
                }

                if (isBalancedRoute)
                {
                    routeReasonText = "Balanced lanes fit best because payout, carry, and recovery can all stay online together.";
                    break;
                }

                routeReasonText = "Safer recovery lanes still work because this crew stretches the haul without forcing volatility.";
                break;

            default:
                if (isRiskyRoute)
                {
                    routeReasonText = "Riskier pushes are absorbable, but this crew is strongest when restart stability matters more than spike payout.";
                    break;
                }

                if (isBalancedRoute)
                {
                    routeReasonText = "Balanced lanes work because the frontline can stay stable while the party keeps the reset line intact.";
                    break;
                }

                routeReasonText = "Safer lanes are the natural fit because recovery and restart stability stay protected.";
                break;
        }

        string routeRiskLabel = string.Empty;
        DungeonRouteTemplate routeTemplate = GetRouteTemplateById(dungeonId, normalizedRouteId);
        if (routeTemplate != null && !string.IsNullOrEmpty(routeTemplate.RiskLabel))
        {
            routeRiskLabel = routeTemplate.RiskLabel;
        }

        string roleFitText = PrototypeRpgRoleIdentity.BuildRouteRoleFitSummary(
            normalizedRouteId,
            routeRiskLabel,
            partySurface.Members,
            member => member != null ? member.DisplayName : string.Empty,
            member => member != null ? member.RoleTag : string.Empty);

        return BuildScenarioSentenceText(
            BuildLabeledScenarioClause("Party", BuildRuntimePartyIdentitySummaryText(partySurface)),
            ChooseRuntimePartySummaryText(partySurface.RouteFitSummaryText, routeReasonText),
            routeReasonText,
            roleFitText,
            BuildRuntimePartyNextEdgeText(partySurface));
    }

    private string BuildLightRuntimePartyRouteFitText(string partyId, string dungeonId, string routeId)
    {
        string normalizedRouteId = NormalizeRouteChoiceId(routeId);
        string balancedRouteId = GetBalancedRouteId(dungeonId);
        bool isRiskyRoute = normalizedRouteId == RiskyRouteId;
        bool isSafeRoute = normalizedRouteId == SafeRouteId;
        bool isBalancedRoute = !string.IsNullOrEmpty(balancedRouteId) && normalizedRouteId == balancedRouteId;
        string archetypeId = _runtimeEconomyState != null ? _runtimeEconomyState.GetPartyArchetypeId(partyId) : string.Empty;
        string identityText = BuildCachedRuntimePartyIdentitySummaryText(partyId);
        string routeReasonText;

        switch (archetypeId)
        {
            case "outrider":
                if (isRiskyRoute)
                {
                    routeReasonText = "Riskier pressure lanes are the cleanest fit because this crew wants exposed targets and faster shard conversion.";
                    break;
                }

                if (isSafeRoute)
                {
                    routeReasonText = "Safer recovery lanes keep the crew alive, but they intentionally trade away some of the burst ceiling.";
                    break;
                }

                routeReasonText = "Balanced lanes are playable, but the best payoff still lives on a route that lets the burst line spike first.";
                break;

            case "salvager":
                if (isRiskyRoute)
                {
                    routeReasonText = "Harder lanes are survivable, but the carry plan usually converts more cleanly on steadier runs.";
                    break;
                }

                if (isBalancedRoute)
                {
                    routeReasonText = "Balanced lanes fit best because payout, carry, and recovery can all stay online together.";
                    break;
                }

                routeReasonText = "Safer recovery lanes still work because this crew stretches the haul without forcing volatility.";
                break;

            default:
                if (isRiskyRoute)
                {
                    routeReasonText = "Riskier pushes are absorbable, but this crew is strongest when restart stability matters more than spike payout.";
                    break;
                }

                if (isBalancedRoute)
                {
                    routeReasonText = "Balanced lanes work because the frontline can stay stable while the party keeps the reset line intact.";
                    break;
                }

                routeReasonText = "Safer lanes are the natural fit because recovery and restart stability stay protected.";
                break;
        }

        string routeRiskLabel = string.Empty;
        DungeonRouteTemplate routeTemplate = GetRouteTemplateById(dungeonId, normalizedRouteId);
        if (routeTemplate != null && !string.IsNullOrEmpty(routeTemplate.RiskLabel))
        {
            routeRiskLabel = routeTemplate.RiskLabel;
        }

        string roleFitText = _runtimeEconomyState != null
            ? _runtimeEconomyState.GetPartyRouteRoleFitSummary(partyId, normalizedRouteId, routeRiskLabel)
            : string.Empty;
        return BuildScenarioSentenceText(
            BuildLabeledScenarioClause("Party", identityText),
            routeReasonText,
            roleFitText);
    }

    private string BuildCachedRuntimePartyIdentitySummaryText(string partyId)
    {
        if (_runtimeEconomyState == null)
        {
            return string.Empty;
        }

        string identityText = _runtimeEconomyState.GetPartyIdentitySummary(partyId);
        return HasText(identityText) && identityText != "None"
            ? identityText
            : _runtimeEconomyState.GetPartyDisplayName(partyId);
    }

    private string BuildRuntimePartyIdentitySummaryText(PrototypeRpgPartyRuntimeResolveSurface partySurface)
    {
        if (partySurface == null)
        {
            return string.Empty;
        }

        if (HasText(partySurface.ArchetypeLabel) && HasText(partySurface.PromotionStateLabel))
        {
            return partySurface.ArchetypeLabel + " / " + partySurface.PromotionStateLabel;
        }

        if (HasText(partySurface.ArchetypeLabel))
        {
            return partySurface.ArchetypeLabel;
        }

        if (HasText(partySurface.PromotionStateLabel))
        {
            return partySurface.PromotionStateLabel;
        }

        return HasText(partySurface.DisplayName) ? partySurface.DisplayName : string.Empty;
    }

    private string BuildRuntimePartyDoctrineText(PrototypeRpgPartyRuntimeResolveSurface partySurface)
    {
        return partySurface == null
            ? string.Empty
            : ChooseRuntimePartySummaryText(
                partySurface.DoctrineSummaryText,
                ExtractRuntimeSummaryClauseText(partySurface.CurrentRunSummaryText, "Battle Plan"));
    }

    private string BuildRuntimePartyNextEdgeText(PrototypeRpgPartyRuntimeResolveSurface partySurface)
    {
        return partySurface == null
            ? string.Empty
            : ChooseRuntimePartySummaryText(
                ExtractRuntimeSummaryClauseText(partySurface.NextRunPreviewSummaryText, "Next Edge"),
                partySurface.PromotionSummaryText);
    }

    private string BuildRuntimePartyPrepSummaryText(PrototypeRpgPartyRuntimeResolveSurface partySurface, int partyPower, int carryCapacity, string partyResultEcho)
    {
        if (partySurface == null)
        {
            return string.Empty;
        }

        string powerText = partyPower > 0 || carryCapacity > 0
            ? "Power " + Mathf.Max(0, partyPower) + " / Carry " + Mathf.Max(0, carryCapacity)
            : string.Empty;
        string lastReturnText = HasText(partyResultEcho) && partyResultEcho != "None" && !partyResultEcho.StartsWith("Ready in ")
            ? "Last Return: " + partyResultEcho
            : string.Empty;
        return BuildScenarioSentenceText(
            BuildLabeledScenarioClause("Party", BuildRuntimePartyIdentitySummaryText(partySurface)),
            ChooseRuntimePartySummaryText(partySurface.StrengthSummaryText, BuildRuntimePartyDoctrineText(partySurface)),
            BuildRuntimePartyDoctrineText(partySurface),
            BuildRuntimePartyNextEdgeText(partySurface),
            powerText,
            lastReturnText);
    }

    private string BuildRuntimePartyBattleIdentityText(PrototypeRpgPartyRuntimeResolveSurface partySurface)
    {
        if (partySurface == null)
        {
            return string.Empty;
        }

        return BuildScenarioSentenceText(
            BuildLabeledScenarioClause("Party", BuildRuntimePartyIdentitySummaryText(partySurface)),
            BuildRuntimePartyDoctrineText(partySurface),
            BuildRuntimePartyNextEdgeText(partySurface));
    }

    private string ExtractRuntimeSummaryClauseText(string summaryText, string label)
    {
        if (!HasText(summaryText) || !HasText(label))
        {
            return string.Empty;
        }

        string[] clauses = summaryText.Split('|');
        string prefix = label.Trim() + " ";
        for (int i = 0; i < clauses.Length; i++)
        {
            string clause = clauses[i] != null ? clauses[i].Trim() : string.Empty;
            if (!HasText(clause))
            {
                continue;
            }

            if (clause.StartsWith(prefix, System.StringComparison.OrdinalIgnoreCase))
            {
                return clause.Substring(prefix.Length).Trim();
            }
        }

        return string.Empty;
    }

    private string ChooseRuntimePartySummaryText(string primary, string fallback)
    {
        return HasText(primary)
            ? primary.Trim()
            : HasText(fallback)
                ? fallback.Trim()
                : string.Empty;
    }
}
