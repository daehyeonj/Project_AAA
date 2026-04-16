public sealed partial class StaticPlaceholderWorldView
{
    public string ResultPanelGearRewardCandidateText => string.IsNullOrEmpty(_latestRpgRunResultSnapshot.GearRewardCandidateSummaryText)
        ? "None"
        : _latestRpgRunResultSnapshot.GearRewardCandidateSummaryText;

    public string ResultPanelEquipSwapChoiceText => string.IsNullOrEmpty(_latestRpgRunResultSnapshot.EquipSwapChoiceSummaryText)
        ? "None"
        : _latestRpgRunResultSnapshot.EquipSwapChoiceSummaryText;

    public string ResultPanelGearCarryContinuityText => string.IsNullOrEmpty(_latestRpgRunResultSnapshot.GearCarryContinuitySummaryText)
        ? "None"
        : _latestRpgRunResultSnapshot.GearCarryContinuitySummaryText;

    public void SetBattleTargetHover(string monsterId)
    {
        DungeonMonsterRuntimeData monster = GetMonsterById(monsterId);
        if (SetHoveredBattleMonster(monster))
        {
            RefreshSelectionPrompt();
            RefreshDungeonPresentation();
        }
    }

    public bool TryTriggerBattleTarget(string monsterId)
    {
        return TryResolveTargetSelection(GetMonsterById(monsterId));
    }

    private string GetNextBattleLaneKey(string currentLaneKey)
    {
        return GetRpgOwnedNextBattleLaneKey(currentLaneKey);
    }

    private string BuildNextBattleLaneLabel(DungeonPartyMemberRuntimeData member)
    {
        return BuildRpgOwnedNextBattleLaneLabel(member);
    }

    private bool CanCurrentActorShiftBattleLane(DungeonPartyMemberRuntimeData member)
    {
        return CanRpgOwnedCurrentActorShiftBattleLane(member);
    }

    private bool TryResolveCurrentActorLaneShift(DungeonPartyMemberRuntimeData member)
    {
        return TryResolveRpgOwnedCurrentActorLaneShift(member);
    }

    private string BuildRouteRiskSummaryText(string routeLabel, string riskLabel)
    {
        if (string.IsNullOrEmpty(routeLabel) && string.IsNullOrEmpty(riskLabel))
        {
            return "None";
        }

        string safeRouteLabel = string.IsNullOrEmpty(routeLabel) ? "Route" : routeLabel;
        string safeRiskLabel = string.IsNullOrEmpty(riskLabel) ? "Unknown" : riskLabel;
        return safeRouteLabel + " | " + safeRiskLabel + " Risk";
    }

    private string BuildRouteRiskSummaryForRoute(string dungeonId, string routeId)
    {
        DungeonRouteTemplate template = GetRouteTemplateById(dungeonId, routeId);
        return template == null
            ? "None"
            : BuildRouteRiskSummaryText(template.RouteLabel, template.RiskLabel);
    }

    private string BuildRouteRewardPreviewEntryText(string dungeonId, string routeId)
    {
        DungeonRouteTemplate template = GetRouteTemplateById(dungeonId, routeId);
        if (template == null)
        {
            return "None";
        }

        int totalReward = template.BattleLootAmount + template.ChestRewardAmount + template.BonusLootAmount;
        return "Battle " + template.BattleLootAmount +
            " | Chest " + template.ChestRewardAmount +
            " | Event " + template.BonusLootAmount +
            " | Recover " + BuildRawHpAmountText(template.RecoverAmount) +
            " | Total " + totalReward;
    }

    private string BuildCurrentDungeonWritebackSummaryText()
    {
        if (_runtimeEconomyState != null)
        {
            string dungeonId = !string.IsNullOrEmpty(_currentDungeonId)
                ? _currentDungeonId
                : _runtimeEconomyState.GetLatestWorldWritebackDungeonId();
            string summary = !string.IsNullOrEmpty(dungeonId)
                ? _runtimeEconomyState.GetLatestWorldWritebackSummaryForDungeon(dungeonId)
                : _runtimeEconomyState.GetLatestWorldWritebackSummary();
            if (!string.IsNullOrEmpty(summary) && summary != "None")
            {
                return summary;
            }
        }

        string dungeonLabel = string.IsNullOrEmpty(_currentDungeonName) ? "None" : _currentDungeonName;
        string routeSummary = BuildSelectedRouteSummary();
        return routeSummary == "None"
            ? dungeonLabel
            : dungeonLabel + " | " + routeSummary;
    }
}
