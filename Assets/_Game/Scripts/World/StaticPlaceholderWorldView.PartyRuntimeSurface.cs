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
        return partyDefinition == null
            ? null
            : PrototypeRpgRuntimeResolveBuilder.BuildPartySurface(
                partyDefinition,
                memberDefinition => memberDefinition != null ? memberDefinition.EquipmentLoadoutId : string.Empty);
    }

    private string ResolveRuntimePartyRoleSummary(string partyId)
    {
        PrototypeRpgPartyRuntimeResolveSurface partySurface = BuildRuntimePartyResolveSurface(partyId);
        if (partySurface == null || partySurface.Members == null || partySurface.Members.Length <= 0)
        {
            return "Warrior / Mage / Cleric / Rogue";
        }

        string summary = string.Empty;
        for (int i = 0; i < partySurface.Members.Length; i++)
        {
            PrototypeRpgMemberRuntimeResolveSurface member = partySurface.Members[i];
            if (member == null || string.IsNullOrEmpty(member.RoleLabel))
            {
                continue;
            }

            summary = string.IsNullOrEmpty(summary) ? member.RoleLabel : summary + " / " + member.RoleLabel;
        }

        return string.IsNullOrEmpty(summary) ? "Warrior / Mage / Cleric / Rogue" : summary;
    }

    private string BuildRuntimePartyRouteFitText(string partyId, string dungeonId, string routeId)
    {
        PrototypeRpgPartyRuntimeResolveSurface partySurface = BuildRuntimePartyResolveSurface(partyId);
        if (partySurface == null)
        {
            return string.Empty;
        }

        string normalizedRouteId = NormalizeRouteChoiceId(routeId);
        string balancedRouteId = GetBalancedRouteId(dungeonId);
        bool isRiskyRoute = normalizedRouteId == RiskyRouteId;
        bool isSafeRoute = normalizedRouteId == SafeRouteId;
        bool isBalancedRoute = !string.IsNullOrEmpty(balancedRouteId) && normalizedRouteId == balancedRouteId;

        switch (partySurface.ArchetypeId)
        {
            case "outrider":
                if (isRiskyRoute)
                {
                    return "Outrider Cell matches the pressure lane and can turn the push into a shard-spike clear.";
                }

                if (isSafeRoute)
                {
                    return "Outrider Cell stays alive on the safer lane, but it gives up some of its pressure ceiling.";
                }

                return "Outrider Cell handles the middle line, but its best payoff still lives on the pressure route.";

            case "salvager":
                if (isRiskyRoute)
                {
                    return "Salvager Wing can survive the harder lane, but its better carry usually converts more cleanly on steadier runs.";
                }

                if (isBalancedRoute)
                {
                    return "Salvager Wing fits the middle line where safer payout and carry both stay online.";
                }

                return "Salvager Wing matches safer recovery lanes and stretches the haul without spiking volatility.";

            default:
                if (isRiskyRoute)
                {
                    return "Bulwark Crew can absorb the risky push, but this crew is strongest when restart stability matters.";
                }

                if (isBalancedRoute)
                {
                    return "Bulwark Crew can hold the middle line while keeping the frontline stable.";
                }

                return "Bulwark Crew matches the safer lane and protects recovery plus restart stability.";
        }
    }
}
