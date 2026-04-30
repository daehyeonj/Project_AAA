using UnityEngine;

[DisallowMultipleComponent]
public sealed class BootstrapSceneStateBridge : MonoBehaviour
{
    private BootEntry _bootEntry;

    public PrototypeLanguage CurrentLanguage => Read(entry => entry.CurrentLanguage);
    public bool IsMainMenuActive => Read(entry => entry.IsMainMenuActive);
    public bool IsWorldSimActive => Read(entry => entry.IsWorldSimActive);
    public bool IsDungeonRunHudMode => Read(entry => entry.IsDungeonRunHudMode);
    public bool IsInventorySurfaceOpen => Read(entry => entry.IsInventorySurfaceOpen);
    public bool AutoTickEnabled => Read(entry => entry.AutoTickEnabled);
    public bool AutoTickPaused => Read(entry => entry.AutoTickPaused);
    public bool IsExpeditionPrepBoardOpen => Read(entry => entry.IsExpeditionPrepBoardOpen);
    public bool CanOpenSelectedExpeditionPrepBoard => Read(entry => entry.CanOpenSelectedExpeditionPrepBoard);
    public bool CanOpenSelectedWorldDungeonAction => Read(entry => entry.CanOpenSelectedWorldDungeonAction);
    public bool HasPendingExpeditionPostRunReveal => Read(entry => entry.HasPendingExpeditionPostRunReveal);
    public bool CanConfirmLegacyDungeonRouteChoice() => Dispatch(entry => entry.CanConfirmLegacyDungeonRouteChoice());
    public int VisibleCityCount => Read(entry => entry.VisibleCityCount);
    public int VisibleDungeonCount => Read(entry => entry.VisibleDungeonCount);
    public int VisibleRoadCount => Read(entry => entry.VisibleRoadCount);
    public int WorldDayCount => Read(entry => entry.WorldDayCount);
    public int TradeStepCount => Read(entry => entry.TradeStepCount);
    public int ActiveExpeditions => Read(entry => entry.ActiveExpeditions);
    public int IdleParties => Read(entry => entry.IdleParties);
    public int TotalParties => Read(entry => entry.TotalParties);
    public int UnmetTotal => Read(entry => entry.UnmetTotal);
    public string PrototypeNameLabel => Read(entry => entry.PrototypeNameLabel, "None");
    public string LastTransitionLabel => Read(entry => entry.LastTransitionLabel, "(missing)");
    public string ControlsLabel => Read(entry => entry.ControlsLabel, "None");
    public string EconomyControlsLabel => Read(entry => entry.EconomyControlsLabel, "None");
    public string ExpeditionControlsLabel => Read(entry => entry.ExpeditionControlsLabel, "None");
    public string RouteCapacityUsedLabel => Read(entry => entry.RouteCapacityUsedLabel, "None");
    public string CitiesWithShortagesLabel => Read(entry => entry.CitiesWithShortagesLabel, "None");
    public string SelectedDisplayName => Read(entry => entry.SelectedDisplayName, "None");
    public string SelectedTypeLabel => Read(entry => entry.SelectedTypeLabel, "None");
    public string SelectedLinkedCityLabel => Read(entry => entry.SelectedLinkedCityLabel, "None");
    public string SelectedLinkedDungeonLabel => Read(entry => entry.SelectedLinkedDungeonLabel, "None");
    public string SelectedCityManaShardStockLabel => Read(entry => entry.SelectedCityManaShardStockLabel, "None");
    public string SelectedNeedPressureLabel => Read(entry => entry.SelectedNeedPressureLabel, "None");
    public string SelectedDispatchReadinessLabel => Read(entry => entry.SelectedDispatchReadinessLabel, "None");
    public string SelectedDispatchRecoveryProgressLabel => Read(entry => entry.SelectedDispatchRecoveryProgressLabel, "None");
    public string SelectedDispatchPolicyLabel => Read(entry => entry.SelectedDispatchPolicyLabel, "None");
    public string SelectedRecommendedRouteSummaryLabel => Read(entry => entry.SelectedRecommendedRouteSummaryLabel, "None");
    public string SelectedDispatchPartySummaryLabel => Read(entry => entry.SelectedDispatchPartySummaryLabel, "None");
    public string SelectedDispatchBriefingSummaryLabel => Read(entry => entry.SelectedDispatchBriefingSummaryLabel, "None");
    public string SelectedDispatchRouteFitSummaryLabel => Read(entry => entry.SelectedDispatchRouteFitSummaryLabel, "None");
    public string SelectedDispatchLaunchLockSummaryLabel => Read(entry => entry.SelectedDispatchLaunchLockSummaryLabel, "None");
    public string SelectedDispatchProjectedOutcomeSummaryLabel => Read(entry => entry.SelectedDispatchProjectedOutcomeSummaryLabel, "None");
    public string SelectedActiveExpeditionLaneLabel => Read(entry => entry.SelectedActiveExpeditionLaneLabel, "None");
    public string SelectedDepartureEchoLabel => Read(entry => entry.SelectedDepartureEchoLabel, "None");
    public string SelectedCityVacancyLabel => Read(entry => entry.SelectedCityVacancyLabel, "None");
    public string SelectedDungeonInboundExpeditionLabel => Read(entry => entry.SelectedDungeonInboundExpeditionLabel, "None");
    public string SelectedDungeonDangerLabel => Read(entry => entry.SelectedDungeonDangerLabel, "None");
    public string SelectedDungeonStatusLabel => Read(entry => entry.SelectedDungeonStatusLabel, "None");
    public string SelectedDungeonAvailabilityLabel => Read(entry => entry.SelectedDungeonAvailabilityLabel, "None");
    public string SelectedReturnEtaSummaryLabel => Read(entry => entry.SelectedReturnEtaSummaryLabel, "None");
    public string SelectedReturnWindowLabel => Read(entry => entry.SelectedReturnWindowLabel, "None");
    public string SelectedRecoveryAfterReturnLabel => Read(entry => entry.SelectedRecoveryAfterReturnLabel, "None");
    public string SelectedRewardPreviewLabel => Read(entry => entry.SelectedRewardPreviewLabel, "None");
    public string SelectedEventPreviewLabel => Read(entry => entry.SelectedEventPreviewLabel, "None");
    public string SelectedRoutePreview1Label => Read(entry => entry.SelectedRoutePreview1Label, "None");
    public string SelectedRoutePreview2Label => Read(entry => entry.SelectedRoutePreview2Label, "None");
    public string SelectedRecommendationReasonLabel => Read(entry => entry.SelectedRecommendationReasonLabel, "None");
    public string SelectedLastExpeditionResultLabel => Read(entry => entry.SelectedLastExpeditionResultLabel, "None");
    public string SelectedWorldWritebackLabel => Read(entry => entry.SelectedWorldWritebackLabel, "None");
    public string SelectedRouteOccupancyLabel => Read(entry => entry.SelectedRouteOccupancyLabel, "None");
    public string WorldActiveExpeditionLaneLabel => Read(entry => entry.WorldActiveExpeditionLaneLabel, "None");
    public string WorldDepartureEchoLabel => Read(entry => entry.WorldDepartureEchoLabel, "None");
    public string WorldReturnEtaLabel => Read(entry => entry.WorldReturnEtaLabel, "None");
    public string WorldWritebackSummaryLabel => Read(entry => entry.WorldWritebackSummaryLabel, "None");
    public string RecentWorldWritebackLog1Label => Read(entry => entry.RecentWorldWritebackLog1Label, "None");
    public string RecentWorldWritebackLog2Label => Read(entry => entry.RecentWorldWritebackLog2Label, "None");
    public string RecentWorldWritebackLog3Label => Read(entry => entry.RecentWorldWritebackLog3Label, "None");
    public string RecentExpeditionLog1Label => Read(entry => entry.RecentExpeditionLog1Label, "None");
    public string RecentExpeditionLog2Label => Read(entry => entry.RecentExpeditionLog2Label, "None");
    public string RecentExpeditionLog3Label => Read(entry => entry.RecentExpeditionLog3Label, "None");
    public string RecentDayLog1Label => Read(entry => entry.RecentDayLog1Label, "None");
    public string RecentDayLog2Label => Read(entry => entry.RecentDayLog2Label, "None");
    public string RecentDayLog3Label => Read(entry => entry.RecentDayLog3Label, "None");

    public string GetText(string key)
    {
        return Read(entry => entry.GetText(key), key);
    }

    public string TranslateValue(string value)
    {
        return Read(entry => entry.TranslateValue(value), value);
    }

    public WorldObservationSurfaceData GetWorldObservationSurfaceData()
    {
        return Read(entry => entry.GetWorldObservationSurfaceData(), new WorldObservationSurfaceData());
    }

    public CityHubSurfaceData GetSelectedCityHubSurfaceData()
    {
        return Read(entry => entry.GetSelectedCityHubSurfaceData(), new CityHubSurfaceData());
    }

    public PrototypeCityHubUiSurfaceData GetCityHubUiSurfaceData()
    {
        return Read(entry => entry.GetCityHubUiSurfaceData(), new PrototypeCityHubUiSurfaceData());
    }

    public CityInteractionPresentationSurfaceData GetCityInteractionPresentationSurfaceData()
    {
        return Read(entry => entry.GetCityInteractionPresentationSurfaceData(), new CityInteractionPresentationSurfaceData());
    }

    public ExpeditionPrepSurfaceData GetSelectedExpeditionPrepSurfaceData()
    {
        return Read(entry => entry.GetSelectedExpeditionPrepSurfaceData(), new ExpeditionPrepSurfaceData());
    }

    public ExpeditionLaunchRequest GetSelectedExpeditionLaunchRequest()
    {
        return Read(entry => entry.GetSelectedExpeditionLaunchRequest(), new ExpeditionLaunchRequest());
    }

    public PrototypeBattleUiSurfaceData GetBattleUiSurfaceData()
    {
        return Read(entry => entry.GetBattleUiSurfaceData(), new PrototypeBattleUiSurfaceData());
    }

    public PrototypeRpgInventorySurfaceData GetInventorySurfaceData()
    {
        return Read(entry => entry.GetInventorySurfaceData(), new PrototypeRpgInventorySurfaceData());
    }

    public PrototypeDungeonRunShellSurfaceData GetDungeonRunShellSurfaceData()
    {
        return Read(entry => entry.GetDungeonRunShellSurfaceData(), new PrototypeDungeonRunShellSurfaceData());
    }

    public void SetLanguage(PrototypeLanguage language)
    {
        Dispatch(entry => entry.SetLanguage(language));
    }

    public void EnterWorldSimFromMenu()
    {
        Dispatch(entry => entry.EnterWorldSimFromMenu());
    }

    public void ReturnToMainMenu()
    {
        Dispatch(entry => entry.ReturnToMainMenu());
    }

    public void RunWorldDayStep()
    {
        Dispatch(entry => entry.RunWorldDayStep());
    }

    public void RecruitWorldSimParty()
    {
        Dispatch(entry => entry.RecruitWorldSimParty());
    }

    public PrototypeCityHubActionResult TryExecuteCityHubAction(PrototypeCityHubActionRequest request)
    {
        return Dispatch(entry => entry.TryExecuteCityHubAction(request), new PrototypeCityHubActionResult());
    }

    public bool TryEnterSelectedWorldDungeon()
    {
        return Dispatch(entry => entry.TryEnterSelectedWorldDungeon());
    }

    public void CancelSelectedWorldExpeditionPrepBoard()
    {
        Dispatch(entry => entry.CancelSelectedWorldExpeditionPrepBoard());
    }

    public void AcknowledgePendingExpeditionPostRunReveal()
    {
        Dispatch(entry => entry.AcknowledgePendingExpeditionPostRunReveal());
    }

    public bool TrySelectExpeditionPrepRoute(string optionKey)
    {
        return Dispatch(entry => entry.TrySelectExpeditionPrepRoute(optionKey));
    }

    public bool TryCycleExpeditionPrepDispatchPolicy()
    {
        return Dispatch(entry => entry.TryCycleExpeditionPrepDispatchPolicy());
    }

    public bool TryRecoverExpeditionPrepOneDay()
    {
        return Dispatch(entry => entry.TryRecoverExpeditionPrepOneDay());
    }

    public bool TryConfirmSelectedExpeditionLaunch()
    {
        return Dispatch(entry => entry.TryConfirmSelectedExpeditionLaunch());
    }

    public bool TryToggleInventorySurface()
    {
        return Dispatch(entry => entry.TryToggleInventorySurface());
    }

    public void CloseInventorySurface()
    {
        Dispatch(entry => entry.CloseInventorySurface());
    }

    public bool TryCycleInventoryMember(int direction)
    {
        return Dispatch(entry => entry.TryCycleInventoryMember(direction));
    }

    public bool TrySelectInventoryMember(string memberId)
    {
        return Dispatch(entry => entry.TrySelectInventoryMember(memberId));
    }

    public bool TrySelectInventorySlot(string slotKey)
    {
        return Dispatch(entry => entry.TrySelectInventorySlot(slotKey));
    }

    public bool TrySelectInventoryItem(string itemInstanceId)
    {
        return Dispatch(entry => entry.TrySelectInventoryItem(itemInstanceId));
    }

    public bool TryConfirmInventoryEquip()
    {
        return Dispatch(entry => entry.TryConfirmInventoryEquip());
    }

    public bool TryUnequipSelectedInventorySlot()
    {
        return Dispatch(entry => entry.TryUnequipSelectedInventorySlot());
    }

    public void ToggleWorldAutoTick()
    {
        Dispatch(entry => entry.ToggleWorldAutoTick());
    }

    public void ToggleWorldAutoTickPause()
    {
        Dispatch(entry => entry.ToggleWorldAutoTickPause());
    }

    public void ResetWorldSimulation()
    {
        Dispatch(entry => entry.ResetWorldSimulation());
    }

    public bool TryTriggerLegacyDungeonRouteChoice(string optionKey)
    {
        return Dispatch(entry => entry.TryTriggerLegacyDungeonRouteChoice(optionKey));
    }

    public void SetLegacyDungeonRouteChoiceHover(string optionKey)
    {
        Dispatch(entry => entry.SetLegacyDungeonRouteChoiceHover(optionKey));
    }

    public bool TryCycleCurrentDispatchPolicy()
    {
        return Dispatch(entry => entry.TryCycleCurrentDispatchPolicy());
    }

    public bool TryConfirmLegacyDungeonRouteChoice()
    {
        return Dispatch(entry => entry.TryConfirmLegacyDungeonRouteChoice());
    }

    public void SetDungeonPanelLaneHover(string optionKey)
    {
        Dispatch(entry => entry.SetDungeonPanelLaneHover(optionKey));
    }

    public bool TryTriggerDungeonPanelLaneOption(string optionKey)
    {
        return Dispatch(entry => entry.TryTriggerDungeonPanelLaneOption(optionKey));
    }

    public void SetDungeonRunPreEliteDecisionHover(string optionKey)
    {
        Dispatch(entry => entry.SetDungeonRunPreEliteDecisionHover(optionKey));
    }

    public bool TryTriggerDungeonRunPreEliteDecision(string optionKey)
    {
        return Dispatch(entry => entry.TryTriggerDungeonRunPreEliteDecision(optionKey));
    }

    public void SetDungeonRunEventDecisionHover(string optionKey)
    {
        Dispatch(entry => entry.SetDungeonRunEventDecisionHover(optionKey));
    }

    public bool TryTriggerDungeonRunEventDecision(string optionKey)
    {
        return Dispatch(entry => entry.TryTriggerDungeonRunEventDecision(optionKey));
    }

    public void SetBattleActionHover(string actionKey)
    {
        Dispatch(entry => entry.SetBattleActionHover(actionKey));
    }

    public bool TryTriggerBattleAction(string actionKey)
    {
        return Dispatch(entry => entry.TryTriggerBattleAction(actionKey));
    }

    public void SetBattleTargetHover(string monsterId)
    {
        Dispatch(entry => entry.SetBattleTargetHover(monsterId));
    }

    public bool TryTriggerBattleTarget(string monsterId)
    {
        return Dispatch(entry => entry.TryTriggerBattleTarget(monsterId));
    }

    private void Awake()
    {
        CacheBootEntry();
    }

    private void CacheBootEntry()
    {
        if (_bootEntry == null)
        {
            _bootEntry = GetComponent<BootEntry>();
        }
    }

    private T Read<T>(System.Func<BootEntry, T> selector, T fallback = default)
    {
        CacheBootEntry();
        return _bootEntry != null ? selector(_bootEntry) : fallback;
    }

    private void Dispatch(System.Action<BootEntry> action)
    {
        CacheBootEntry();
        if (_bootEntry != null)
        {
            action(_bootEntry);
        }
    }

    private bool Dispatch(System.Func<BootEntry, bool> action)
    {
        CacheBootEntry();
        return _bootEntry != null && action(_bootEntry);
    }

    private T Dispatch<T>(System.Func<BootEntry, T> action, T fallback = default)
    {
        CacheBootEntry();
        return _bootEntry != null ? action(_bootEntry) : fallback;
    }
}
