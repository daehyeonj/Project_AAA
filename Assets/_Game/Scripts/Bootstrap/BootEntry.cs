using UnityEngine;
using UnityEngine.InputSystem;

public sealed class BootEntry : MonoBehaviour
{
    private static readonly Color BootColor = new Color(0.08f, 0.09f, 0.12f, 1f);
    private static readonly Color MainMenuColor = new Color(0.14f, 0.17f, 0.2f, 1f);
    private static readonly Color WorldSimColor = new Color(0.11f, 0.16f, 0.12f, 1f);
    private static readonly Color DungeonRunColor = new Color(0.1f, 0.1f, 0.14f, 1f);

    private GameFlowCoordinator _gameFlowCoordinator;
    private BootstrapSceneStateBridge _sceneStateBridge;
    private BootstrapInputGate _inputGate;
    private StaticPlaceholderWorldView _worldView;
    private CityInteraction _cityInteraction;
    private WorldCameraController _worldCameraController;
    private ResourceData[] _resources;
    private bool _hasAppliedGameFlowPresentationState;
    private GameStateId _lastAppliedGameFlowStateId = GameStateId.Boot;
    private PrototypeLanguage _currentLanguage = PrototypeLanguage.English;
    private int _cachedWorldObservationSurfaceFrame = -1;
    private WorldObservationSurfaceData _cachedWorldObservationSurfaceData;
    private int _cachedCityHubUiSurfaceFrame = -1;
    private PrototypeCityHubUiSurfaceData _cachedCityHubUiSurfaceData;
    private int _cachedCityInteractionPresentationSurfaceFrame = -1;
    private CityInteractionPresentationSurfaceData _cachedCityInteractionPresentationSurfaceData;
    private bool HasGameFlowCoordinator => _gameFlowCoordinator != null;
    private GameStateId CurrentState => _gameFlowCoordinator != null ? _gameFlowCoordinator.CurrentState : GameStateId.Boot;
    private string LastTransition => _gameFlowCoordinator != null ? _gameFlowCoordinator.LastTransition : "(missing)";

    public PrototypeLanguage CurrentLanguage => _currentLanguage;
    public bool IsBootActive => HasGameFlowCoordinator && CurrentState == GameStateId.Boot;
    public bool IsMainMenuActive => HasGameFlowCoordinator && CurrentState == GameStateId.MainMenu;
    public bool IsWorldSimActive => HasGameFlowCoordinator && CurrentState == GameStateId.WorldSim;
    public string CurrentLanguageLabel => PrototypeLocalization.GetLanguageDisplayName(_currentLanguage);
    public string PrototypeNameLabel => GetText("PrototypeName");
    public string DebugStepLabel => GetText("StepLabel");
    public string CurrentStateLabel => HasGameFlowCoordinator ? CurrentState.ToString() : "(missing)";
    public string LastTransitionLabel => LastTransition;
    public int VisibleCityCount => StaticPlaceholderWorldView.CityCount;
    public int VisibleDungeonCount => StaticPlaceholderWorldView.DungeonCount;
    public int VisibleRoadCount => StaticPlaceholderWorldView.RoadCount;
    public int WorldEntityCount => _worldView != null ? _worldView.EntityCount : 0;
    public int WorldRouteCount => _worldView != null ? _worldView.RouteCount : 0;
    public int ResourceCount => _resources != null ? _resources.Length : 0;
    public int TradeOpportunityCount => _worldView != null ? _worldView.TradeOpportunityCount : 0;
    public int ActiveTradeOpportunityCount => _worldView != null ? _worldView.ActiveTradeOpportunityCount : 0;
    public int UnmetCityNeedsCount => _worldView != null ? _worldView.UnmetCityNeedCount : 0;
    public bool AutoTickEnabled => GetCurrentCityHubHeaderSurfaceData().AutoTickEnabled;
    public bool AutoTickPaused => _worldView != null && _worldView.AutoTickPaused;
    public float TickIntervalSeconds => _worldView != null ? _worldView.TickIntervalSeconds : 0f;
    public int AutoTickCount => _worldView != null ? _worldView.AutoTickCount : 0;
    public int WorldDayCount => GetCurrentCityHubHeaderSurfaceData().WorldDayCount;
    public int TradeStepCount => GetCurrentCityHubOverviewSurfaceData().TradeStepCount;
    public int ProducedTotal => _worldView != null ? _worldView.LastDayProducedTotal : 0;
    public int ClaimedDungeonOutputsTotal => _worldView != null ? _worldView.LastDayClaimedDungeonOutputsTotal : 0;
    public int TradedTotal => _worldView != null ? _worldView.LastDayTradedTotal : 0;
    public int ProcessedTotal => _worldView != null ? _worldView.LastDayProcessedTotal : 0;
    public int ConsumedTotal => _worldView != null ? _worldView.LastDayConsumedTotal : 0;
    public int CriticalFulfilledTotal => _worldView != null ? _worldView.LastDayCriticalFulfilledTotal : 0;
    public int CriticalUnmetTotal => _worldView != null ? _worldView.LastDayCriticalUnmetTotal : 0;
    public int NormalFulfilledTotal => _worldView != null ? _worldView.LastDayNormalFulfilledTotal : 0;
    public int NormalUnmetTotal => _worldView != null ? _worldView.LastDayNormalUnmetTotal : 0;
    public int FulfilledTotal => _worldView != null ? _worldView.LastDayFulfilledTotal : 0;
    public int UnmetTotal => _worldView != null ? _worldView.LastDayUnmetTotal : 0;
    public int ShortagesTotal => _worldView != null ? _worldView.TotalShortages : 0;
    public int ProcessingBlockedTotal => _worldView != null ? _worldView.LastDayProcessingBlockedTotal : 0;
    public int ProcessingReservedTotal => _worldView != null ? _worldView.LastDayProcessingReservedTotal : 0;
    public int UnclaimedDungeonOutputsTotal => _worldView != null ? _worldView.CurrentUnclaimedDungeonOutputsTotal : 0;
    public string RouteCapacityUsedLabel => _worldView != null ? _worldView.RouteCapacityUsedText : "None";
    public string SaturatedRoutesLabel => _worldView != null ? _worldView.SaturatedRoutesText : "None";
    public string CitiesWithShortagesLabel => GetCurrentCityHubOverviewSurfaceData().CitiesWithShortagesText;
    public string CitiesWithSurplusLabel => _worldView != null ? _worldView.CitiesWithSurplusText : "None";
    public string CitiesWithProcessingLabel => _worldView != null ? _worldView.CitiesWithProcessingText : "None";
    public string CitiesWithCriticalUnmetLabel => _worldView != null ? _worldView.CitiesWithCriticalUnmetText : "None";
    public string ResourceIdsLabel => GetResourceIdsLabel();
    public string FirstRouteIdLabel => _worldView != null ? _worldView.FirstRouteId : "None";
    public string FirstRouteTagsLabel => _worldView != null ? _worldView.FirstRouteTagsText : "None";
    public string FirstRouteLinkLabel => _worldView != null ? _worldView.FirstRouteLinkText : "None";
    public string FirstRouteCapacityLabel => _worldView != null ? _worldView.FirstRouteCapacityText : "None";
    public string FirstRouteUsageLabel => _worldView != null ? _worldView.FirstRouteUsageText : "None";
    public string FirstRouteUtilizationLabel => _worldView != null ? _worldView.FirstRouteUtilizationText : "None";
    public string TradeLink1Label => _worldView != null ? _worldView.TradeLink1Text : "None";
    public string TradeLink2Label => _worldView != null ? _worldView.TradeLink2Text : "None";
    public string DungeonOutputHintLabel => _worldView != null ? _worldView.DungeonOutputHintText : "None";
    public string EconomyControlsLabel => GetText("EconomyControlsValue");
    public string ExpeditionControlsLabel => GetText("ExpeditionControlsValue");
    public int TotalParties => GetCurrentCityHubOverviewSurfaceData().TotalParties;
    public int IdleParties => GetCurrentCityHubOverviewSurfaceData().IdleParties;
    public int ActiveExpeditions => GetCurrentCityHubOverviewSurfaceData().ActiveExpeditions;
    public int AvailableContracts => _worldView != null ? _worldView.AvailableContracts : 0;
    public bool IsExpeditionPrepBoardOpen => IsWorldSimActive && GetCurrentCityHubActionSurfaceData().IsExpeditionPrepBoardOpen;
    public bool CanOpenSelectedExpeditionPrepBoard => IsWorldSimActive && GetCurrentCityHubActionSurfaceData().CanOpenExpeditionPrep;
    public bool CanOpenSelectedWorldDungeonAction => IsWorldSimActive && GetCurrentCityHubActionSurfaceData().CanEnterDungeon;
    public bool HasPendingExpeditionPostRunReveal
    {
        get
        {
            WorldObservationSurfaceData observation = GetCurrentWorldObservationSurfaceData();
            return IsWorldSimActive &&
                observation != null &&
                observation.RecentOutcome != null &&
                observation.RecentOutcome.PostRunReveal != null &&
                observation.RecentOutcome.PostRunReveal.HasPendingReveal;
        }
    }
    public int ExpeditionSuccessCount => _worldView != null ? _worldView.ExpeditionSuccessCount : 0;
    public int ExpeditionFailureCount => _worldView != null ? _worldView.ExpeditionFailureCount : 0;
    public int ExpeditionLootReturnedTotal => _worldView != null ? _worldView.ExpeditionLootReturnedTotal : 0;
    public string RecentDayLog1Label => GetIndexedLogText(GetCurrentCityHubLogSurfaceData().RecentDayLogs, 0);
    public string RecentDayLog2Label => GetIndexedLogText(GetCurrentCityHubLogSurfaceData().RecentDayLogs, 1);
    public string RecentDayLog3Label => GetIndexedLogText(GetCurrentCityHubLogSurfaceData().RecentDayLogs, 2);
    public string RecentExpeditionLog1Label => GetIndexedLogText(GetCurrentCityHubLogSurfaceData().RecentExpeditionLogs, 0);
    public string RecentExpeditionLog2Label => GetIndexedLogText(GetCurrentCityHubLogSurfaceData().RecentExpeditionLogs, 1);
    public string RecentExpeditionLog3Label => GetIndexedLogText(GetCurrentCityHubLogSurfaceData().RecentExpeditionLogs, 2);
    public string RecentWorldWritebackLog1Label => GetIndexedLogText(GetCurrentCityHubLogSurfaceData().RecentWorldWritebackLogs, 0);
    public string RecentWorldWritebackLog2Label => GetIndexedLogText(GetCurrentCityHubLogSurfaceData().RecentWorldWritebackLogs, 1);
    public string RecentWorldWritebackLog3Label => GetIndexedLogText(GetCurrentCityHubLogSurfaceData().RecentWorldWritebackLogs, 2);
    public string SelectedDisplayName => FormatObservationText(GetCurrentWorldObservationSurfaceData().SelectedEntity.SelectedEntityLabel);
    public string SelectedTypeLabel => FormatObservationText(GetCurrentWorldObservationSurfaceData().SelectedEntity.SelectedKindLabel);
    public string SelectedPositionLabel => FormatObservationText(GetCurrentWorldObservationSurfaceData().SelectedEntity.SelectedPositionText);
    public string SelectedIdLabel => FormatObservationText(GetCurrentWorldObservationSurfaceData().SelectedEntity.SelectedEntityId);
    public string SelectedResourcesLabel => FormatObservationText(GetCurrentWorldObservationSurfaceData().SelectedEntity.SelectedResourcesText);
    public string SelectedResourceRolesLabel => FormatObservationText(GetCurrentWorldObservationSurfaceData().SelectedEntity.SelectedResourceRolesText);
    public string SelectedSupplyLabel => FormatObservationText(GetCurrentWorldObservationSurfaceData().SelectedEntity.SelectedSupplyText);
    public string SelectedNeedsLabel => FormatObservationText(GetCurrentWorldObservationSurfaceData().SelectedEntity.SelectedNeedsText);
    public string SelectedHighPriorityNeedsLabel => FormatObservationText(GetCurrentWorldObservationSurfaceData().SelectedEntity.SelectedHighPriorityNeedsText);
    public string SelectedNormalPriorityNeedsLabel => FormatObservationText(GetCurrentWorldObservationSurfaceData().SelectedEntity.SelectedNormalPriorityNeedsText);
    public string SelectedOutputLabel => FormatObservationText(GetCurrentWorldObservationSurfaceData().SelectedEntity.SelectedOutputText);
    public string SelectedProcessingLabel => FormatObservationText(GetCurrentWorldObservationSurfaceData().SelectedEntity.SelectedProcessingText);
    public string SelectedLinkedCityLabel => FormatObservationText(GetCurrentWorldObservationSurfaceData().SelectedEntity.SelectedLinkedCityText);
    public string SelectedPartiesInCityLabel => FormatObservationText(GetCurrentWorldObservationSurfaceData().SelectedEntity.SelectedPartiesInCityText);
    public string SelectedIdlePartiesLabel => FormatObservationText(GetCurrentWorldObservationSurfaceData().SelectedEntity.SelectedIdlePartiesText);
    public string SelectedActiveExpeditionsFromCityLabel => FormatObservationText(GetCurrentWorldObservationSurfaceData().SelectedEntity.SelectedActiveExpeditionsFromCityText);
    public string SelectedAvailableContractLabel => FormatObservationText(GetCurrentWorldObservationSurfaceData().SelectedEntity.SelectedAvailableContractText);
    public string SelectedLinkedDungeonLabel => FormatObservationText(GetCurrentWorldObservationSurfaceData().SelectedEntity.SelectedLinkedDungeonText);
    public string SelectedDungeonDangerLabel => FormatObservationText(GetCurrentWorldObservationSurfaceData().SelectedEntity.SelectedDungeonDangerText);
    public string SelectedCityManaShardStockLabel => GetCurrentCityHubSelectionSurfaceData().CityManaShardStockText;

    public string SelectedNeedPressureLabel => GetCurrentCityHubSelectionSurfaceData().NeedPressureText;
    public string SelectedDispatchReadinessLabel => GetCurrentCityHubSelectionSurfaceData().DispatchReadinessText;
    public string SelectedDispatchRecoveryProgressLabel => GetCurrentCityHubSelectionSurfaceData().RecoveryProgressText;
    public string SelectedDispatchPolicyLabel => GetCurrentCityHubSelectionSurfaceData().DispatchPolicyText;
    public string SelectedRecommendedRouteSummaryLabel => GetCurrentCityHubSelectionSurfaceData().RecommendedRouteText;

    public string SelectedRecommendedRouteForLinkedCityLabel => GetCurrentCityHubSelectionSurfaceData().RecommendedRouteText;
    public string SelectedRecommendationReasonLabel => GetCurrentCityHubSelectionSurfaceData().RecommendationReasonText;

    public string SelectedLastDispatchImpactLabel => GetCurrentWorldObservationSurfaceData().RecentOutcome.SelectedLastDispatchImpactText;
    public string SelectedWorldWritebackLabel => GetCurrentCityHubOutcomeSurfaceData().WorldWritebackText;
    public string SelectedLastDispatchStockDeltaLabel => GetCurrentWorldObservationSurfaceData().RecentOutcome.SelectedLastDispatchStockDeltaText;
    public string SelectedLastNeedPressureChangeLabel => GetCurrentWorldObservationSurfaceData().RecentOutcome.SelectedLastNeedPressureChangeText;
    public string SelectedLastDispatchReadinessChangeLabel => GetCurrentWorldObservationSurfaceData().RecentOutcome.SelectedLastDispatchReadinessChangeText;
    public string SelectedRecoveryEtaLabel => GetCurrentExpeditionPrepSurfaceData().RecoveryEtaText;
    public string SelectedRecommendedPowerLabel => FormatObservationInt(GetCurrentWorldObservationSurfaceData().Launch.RecommendedPower);
    public string SelectedExpeditionDurationDaysLabel => FormatObservationInt(GetCurrentWorldObservationSurfaceData().Launch.ExpeditionDurationDays);
    public string SelectedRewardPreviewLabel => GetCurrentCityHubSelectionSurfaceData().RewardPreviewText;
    public string SelectedEventPreviewLabel => GetCurrentCityHubSelectionSurfaceData().EventPreviewText;
    public string SelectedDispatchPartySummaryLabel => GetCurrentCityHubActionSurfaceData().DispatchPartySummaryText;
    public string SelectedDispatchBriefingSummaryLabel => GetCurrentCityHubActionSurfaceData().DispatchBriefingSummaryText;
    public string SelectedDispatchRouteFitSummaryLabel => GetCurrentCityHubActionSurfaceData().RouteFitSummaryText;
    public string SelectedDispatchLaunchLockSummaryLabel => GetCurrentCityHubActionSurfaceData().LaunchLockSummaryText;
    public string SelectedDispatchProjectedOutcomeSummaryLabel => GetCurrentCityHubActionSurfaceData().ProjectedOutcomeSummaryText;
    public string SelectedActiveExpeditionLaneLabel => GetCurrentCityHubActionSurfaceData().ActiveExpeditionLaneText;
    public string SelectedDepartureEchoLabel => GetCurrentWorldObservationSurfaceData().ActiveExpedition.SelectedDepartureEchoText;
    public string SelectedReturnEtaSummaryLabel => GetCurrentCityHubActionSurfaceData().ReturnEtaText;
    public string SelectedCityVacancyLabel => GetCurrentCityHubActionSurfaceData().CityVacancyText;
    public string SelectedDungeonInboundExpeditionLabel => GetCurrentWorldObservationSurfaceData().ActiveExpedition.SelectedDungeonInboundExpeditionText;
    public string SelectedDungeonStatusLabel => GetCurrentCityHubSelectionSurfaceData().DungeonStatusText;
    public string SelectedDungeonAvailabilityLabel => GetCurrentCityHubSelectionSurfaceData().DungeonAvailabilityText;
    public string SelectedRouteOccupancyLabel => GetCurrentWorldObservationSurfaceData().ActiveExpedition.SelectedRouteOccupancyText;
    public string SelectedReturnWindowLabel => GetCurrentExpeditionPrepSurfaceData().ReturnWindowText;
    public string SelectedRecoveryAfterReturnLabel => GetCurrentExpeditionPrepSurfaceData().RecoveryAfterReturnText;
    public string WorldActiveExpeditionLaneLabel => GetCurrentCityHubOverviewSurfaceData().ActiveExpeditionLaneText;
    public string WorldDepartureEchoLabel => GetCurrentWorldObservationSurfaceData().ActiveExpedition.WorldDepartureEchoText;
    public string WorldReturnEtaLabel => GetCurrentCityHubOverviewSurfaceData().ReturnEtaText;
    public string WorldWritebackSummaryLabel => GetCurrentCityHubOverviewSurfaceData().WorldWritebackText;
    public string SelectedRoutePreview1Label => _worldView != null ? _worldView.SelectedRoutePreview1Text : "None";
    public string SelectedRoutePreview2Label => _worldView != null ? _worldView.SelectedRoutePreview2Text : "None";
    public string SelectedActiveExpeditionsLabel => GetCurrentWorldObservationSurfaceData().RecentOutcome.SelectedActiveExpeditionsText;
    public string SelectedLastExpeditionResultLabel => GetCurrentWorldObservationSurfaceData().RecentOutcome.SelectedLastExpeditionResultText;
    public string SelectedExpeditionLootReturnedLabel => GetCurrentWorldObservationSurfaceData().RecentOutcome.SelectedExpeditionLootReturnedText;
    public string SelectedLastRunSurvivingMembersLabel => GetCurrentWorldObservationSurfaceData().RecentOutcome.SelectedLastRunSurvivingMembersText;
    public string SelectedLastRunClearedEncountersLabel => GetCurrentWorldObservationSurfaceData().RecentOutcome.SelectedLastRunClearedEncountersText;
    public string SelectedLastRunEventChoiceLabel => GetCurrentWorldObservationSurfaceData().RecentOutcome.SelectedLastRunEventChoiceText;
    public string SelectedLastRunLootBreakdownLabel => GetCurrentWorldObservationSurfaceData().RecentOutcome.SelectedLastRunLootBreakdownText;
    public string SelectedLastRunDungeonLabel => GetCurrentWorldObservationSurfaceData().RecentOutcome.SelectedLastRunDungeonText;
    public string SelectedLastRunRouteLabel => GetCurrentWorldObservationSurfaceData().RecentOutcome.SelectedLastRunRouteText;
    public string SelectedLastRunGearRewardLabel => _worldView != null ? _worldView.SelectedLastRunGearRewardText : "None";
    public string SelectedLastRunEquipSwapLabel => _worldView != null ? _worldView.SelectedLastRunEquipSwapText : "None";
    public string SelectedLastRunGearContinuityLabel => _worldView != null ? _worldView.SelectedLastRunGearContinuityText : "None";
    public string SelectedSurplusLabel => _worldView != null ? _worldView.SelectedSurplusText : "None";
    public string SelectedDeficitLabel => _worldView != null ? _worldView.SelectedDeficitText : "None";
    public string SelectedIdentityLabel => _worldView != null ? _worldView.SelectedIdentityText : "None";
    public string SelectedReserveStockRuleLabel => _worldView != null ? _worldView.SelectedReserveStockRuleText : "None";
    public string SelectedProcessingPreferenceLabel => _worldView != null ? _worldView.SelectedProcessingPreferenceText : "None";
    public string SelectedStocksLabel => FormatObservationText(GetCurrentWorldObservationSurfaceData().SelectedEntity.SelectedStocksText);
    public string SelectedLastDayProducedLabel => _worldView != null ? _worldView.SelectedLastDayProducedText : "None";
    public string SelectedLastDayDungeonImportsLabel => _worldView != null ? _worldView.SelectedLastDayDungeonImportedText : "None";
    public string SelectedLastDayImportedLabel => _worldView != null ? _worldView.SelectedLastDayImportedText : "None";
    public string SelectedLastDayExportedLabel => _worldView != null ? _worldView.SelectedLastDayExportedText : "None";
    public string SelectedLastDayProcessedInLabel => _worldView != null ? _worldView.SelectedLastDayProcessedInText : "None";
    public string SelectedLastDayProcessedOutLabel => _worldView != null ? _worldView.SelectedLastDayProcessedOutText : "None";
    public string SelectedLastDayProcessedTotalLabel => _worldView != null ? _worldView.SelectedLastDayProcessedTotalText : "None";
    public string SelectedLastDayConsumedLabel => _worldView != null ? _worldView.SelectedLastDayConsumedText : "None";
    public string SelectedLastDayCriticalFulfilledLabel => _worldView != null ? _worldView.SelectedLastDayCriticalFulfilledText : "None";
    public string SelectedLastDayCriticalUnmetLabel => _worldView != null ? _worldView.SelectedLastDayCriticalUnmetText : "None";
    public string SelectedLastDayNormalFulfilledLabel => _worldView != null ? _worldView.SelectedLastDayNormalFulfilledText : "None";
    public string SelectedLastDayNormalUnmetLabel => _worldView != null ? _worldView.SelectedLastDayNormalUnmetText : "None";
    public string SelectedLastDayFulfilledLabel => _worldView != null ? _worldView.SelectedLastDayFulfilledText : "None";
    public string SelectedLastDayUnmetLabel => _worldView != null ? _worldView.SelectedLastDayUnmetText : "None";
    public string SelectedLastDayShortagesLabel => _worldView != null ? _worldView.SelectedLastDayShortagesText : "None";
    public string SelectedTotalFulfilledLabel => _worldView != null ? _worldView.SelectedTotalFulfilledText : "None";
    public string SelectedTotalUnmetLabel => _worldView != null ? _worldView.SelectedTotalUnmetText : "None";
    public string SelectedTotalCriticalUnmetLabel => _worldView != null ? _worldView.SelectedTotalCriticalUnmetText : "None";
    public string SelectedTotalNormalUnmetLabel => _worldView != null ? _worldView.SelectedTotalNormalUnmetText : "None";
    public string SelectedTotalShortagesLabel => _worldView != null ? _worldView.SelectedTotalShortagesText : "None";
    public string SelectedLastDayProcessingBlockedLabel => _worldView != null ? _worldView.SelectedLastDayProcessingBlockedText : "None";
    public string SelectedLastDayProcessingReservedLabel => _worldView != null ? _worldView.SelectedLastDayProcessingReservedText : "None";
    public string SelectedLastDayClaimedOutLabel => _worldView != null ? _worldView.SelectedLastDayClaimedOutText : "None";
    public string SelectedIncomingTradeLabel => _worldView != null ? _worldView.SelectedIncomingTradeText : "None";
    public string SelectedOutgoingTradeLabel => _worldView != null ? _worldView.SelectedOutgoingTradeText : "None";
    public string SelectedUnmetNeedsLabel => _worldView != null ? _worldView.SelectedUnmetNeedsText : "None";
    public string SelectedTagsLabel => _worldView != null ? _worldView.SelectedTagsText : "None";
    public string SelectedStatLabel => _worldView != null ? _worldView.SelectedStatText : "None";
    public string ControlsLabel => GetText("ControlsValue");
    public string SelectedMonsterCountPreviewLabel => _worldView != null ? _worldView.SelectedMonsterCountPreviewText : "None";
    public string DungeonRunWorldControlsLabel => ExpeditionControlsLabel;
    public string DungeonRunExploreControlsLabel => GetText("DungeonExploreControlsValue");
    public string DungeonRunBattleControlsLabel => GetText("DungeonBattleControlsValue");
    public string DungeonRunTargetControlsLabel => GetText("DungeonTargetControlsValue");
    public string DungeonRunRouteControlsLabel => GetText("DungeonRouteControlsValue");
    public bool IsDungeonRunHudMode => HasGameFlowCoordinator && CurrentState == GameStateId.DungeonRun;
    public bool IsDungeonBattleViewActive => _worldView != null && _worldView.IsBattleViewActive;
    // Legacy fallback bridge only. ExpeditionPrep owns the normal launch seam.
    public bool IsLegacyDungeonRouteChoiceVisible => _worldView != null && _worldView.IsDungeonRouteChoiceVisible;
    public bool IsDungeonRunEventDecisionVisible => _worldView != null && _worldView.IsDungeonEventChoiceVisible;
    public bool IsDungeonRunPreEliteDecisionVisible => _worldView != null && _worldView.IsDungeonPreEliteChoiceVisible;
    public bool IsDungeonResultPanelVisible => _worldView != null && _worldView.IsDungeonResultPanelVisible;
    public string DungeonRunStateLabel => _worldView != null ? _worldView.DungeonRunStateText : "None";
    public string CurrentDungeonRunLabel => _worldView != null ? _worldView.CurrentDungeonRunText : "None";
    public string CurrentCityRunLabel => _worldView != null ? _worldView.CurrentCityText : "None";
    public string CurrentDungeonDangerLabel => _worldView != null ? _worldView.CurrentDungeonDangerText : "None";
    public string CurrentCityManaShardStockRunLabel => _worldView != null ? _worldView.CityManaShardStockText : "None";

    public string CurrentNeedPressureRunLabel => _worldView != null ? _worldView.NeedPressureText : "None";
    public string CurrentDispatchReadinessRunLabel => _worldView != null ? _worldView.DispatchReadinessText : "None";
    public string CurrentDispatchRecoveryProgressRunLabel => _worldView != null ? _worldView.DispatchRecoveryProgressText : "None";
    public string CurrentDispatchConsecutiveRunLabel => _worldView != null ? _worldView.DispatchConsecutiveCountText : "None";
    public string CurrentDispatchPolicyRunLabel => _worldView != null ? _worldView.DispatchPolicyText : "None";
    public string RecommendedRouteRunLabel => _worldView != null ? _worldView.RecommendedRouteText : "None";
    public string RecommendationReasonRunLabel => _worldView != null ? _worldView.RecommendationReasonText : "None";
    public string RecoveryAdviceRunLabel => _worldView != null ? _worldView.RecoveryAdviceText : "None";

    public string ExpectedNeedImpactRunLabel => _worldView != null ? _worldView.ExpectedNeedImpactText : "None";
    public string ActiveDungeonPartyLabel => _worldView != null ? _worldView.ActiveDungeonPartyText : "None";
    public string CurrentRoomLabel => _worldView != null ? _worldView.CurrentRoomText : "None";
    public string CurrentRoomTypeLabel => _worldView != null ? _worldView.CurrentRoomTypeText : "None";
    public string RoomProgressLabel => _worldView != null ? _worldView.RoomProgressText : "None";
    public string NextMajorGoalLabel => _worldView != null ? _worldView.NextMajorGoalText : "None";
    public string CurrentRouteLabel => _worldView != null ? _worldView.CurrentRouteText : "None";
    public string RouteRiskLabel => _worldView != null ? _worldView.CurrentRouteRiskText : "None";
    public string TotalPartyHpLabel => _worldView != null ? _worldView.TotalPartyHpText : "None";
    public string PartyConditionLabel => _worldView != null ? _worldView.PartyConditionText : "None";
    public string SustainPressureLabel => _worldView != null ? _worldView.SustainPressureText : "None";
    public string CarriedLootRunLabel => _worldView != null ? _worldView.CarriedLootText : "None";
    public string EncounterProgressLabel => _worldView != null ? _worldView.EncounterProgressText : "None";
    public string ExitUnlockedLabel => _worldView != null ? _worldView.ExitUnlockedText : "None";
    public string EliteStatusLabel => _worldView != null ? _worldView.EliteStatusText : "None";
    public string ChestOpenedLabel => _worldView != null ? _worldView.ChestOpenedText : "None";
    public string EventStatusLabel => _worldView != null ? _worldView.EventStatusText : "None";
    public string DungeonRunEventDecisionLabel => _worldView != null ? _worldView.EventChoiceText : "None";
    public string DungeonRunPreEliteDecisionLabel => _worldView != null ? _worldView.PreEliteChoiceText : "Pending";
    public string LootBreakdownLabel => _worldView != null ? _worldView.LootBreakdownText : "None";
    public string BattleLootBreakdownLabel => _worldView != null ? _worldView.BattleLootText : "None";
    public string ChestLootBreakdownLabel => _worldView != null ? _worldView.ChestLootText : "None";
    public string EventLootBreakdownLabel => _worldView != null ? _worldView.EventLootText : "None";
    public string LegacyDungeonRouteChoiceTitleLabel => _worldView != null ? _worldView.RouteChoiceTitleText : "None";
    public string LegacyDungeonRouteChoiceDescriptionLabel => _worldView != null ? _worldView.RouteChoiceDescriptionText : "None";
    public string LegacyDungeonRouteChoicePromptLabel => _worldView != null ? _worldView.RouteChoicePromptText : "None";
    public string LegacyDungeonRouteOption1Label => _worldView != null ? _worldView.RouteOption1Text : "None";
    public string LegacyDungeonRouteOption2Label => _worldView != null ? _worldView.RouteOption2Text : "None";
    public string DungeonRunEventDecisionTitleLabel => _worldView != null ? _worldView.EventTitleText : "None";
    public string DungeonRunEventDecisionDescriptionLabel => _worldView != null ? _worldView.EventDescriptionText : "None";
    public string DungeonRunEventDecisionPromptLabel => _worldView != null ? _worldView.EventPromptText : "None";
    public string DungeonRunEventDecisionOptionALabel => _worldView != null ? _worldView.EventOptionAText : "None";
    public string DungeonRunEventDecisionOptionBLabel => _worldView != null ? _worldView.EventOptionBText : "None";
    public string DungeonRunPreEliteDecisionTitleLabel => _worldView != null ? _worldView.PreEliteTitleText : "None";
    public string DungeonRunPreEliteDecisionDescriptionLabel => _worldView != null ? _worldView.PreEliteDescriptionText : "None";
    public string DungeonRunPreEliteDecisionPromptLabel => _worldView != null ? _worldView.PreElitePromptText : "None";
    public string DungeonRunPreEliteDecisionOptionALabel => _worldView != null ? _worldView.PreEliteOptionAText : "None";
    public string DungeonRunPreEliteDecisionOptionBLabel => _worldView != null ? _worldView.PreEliteOptionBText : "None";
    public int DungeonRunTurnCount => _worldView != null ? _worldView.DungeonRunTurnCount : 0;
    public string LastBattleLog1Label => _worldView != null ? _worldView.RecentBattleLog1Text : "None";
    public string LastBattleLog2Label => _worldView != null ? _worldView.RecentBattleLog2Text : "None";
    public string LastBattleLog3Label => _worldView != null ? _worldView.RecentBattleLog3Text : "None";
    public string BattleStateLabel => _worldView != null ? _worldView.BattleStateText : "None";
    public string BattleViewStateLabel => _worldView != null ? _worldView.BattleViewStateText : "Inactive";
    public string EncounterNameLabel => _worldView != null ? _worldView.CurrentEncounterNameText : "None";
    public string EncounterRoomTypeLabel => _worldView != null ? _worldView.EncounterRoomTypeText : "None";
    public string CurrentBattleActorLabel => _worldView != null ? _worldView.CurrentBattleActorText : "None";
    public string BattleMonsterNameLabel => _worldView != null ? _worldView.BattleMonsterNameText : "None";
    public string BattleMonsterHpLabel => _worldView != null ? _worldView.BattleMonsterHpText : "None";
    public string BattleMonster1Label => _worldView != null ? _worldView.BattleMonster1Text : "None";
    public string BattleMonster2Label => _worldView != null ? _worldView.BattleMonster2Text : "None";
    public string EliteEncounterNameLabel => _worldView != null ? _worldView.EliteEncounterNameText : "None";
    public string EliteTypeLabel => _worldView != null ? _worldView.EliteTypeText : "None";
    public string EliteHpLabel => _worldView != null ? _worldView.EliteHpText : "None";
    public string EliteDefeatedLabel => _worldView != null ? _worldView.EliteDefeatedText : "None";
    public string EliteRewardStatusLabel => _worldView != null ? _worldView.EliteRewardStatusText : "None";
    public string EliteRewardHintLabel => _worldView != null ? _worldView.EliteRewardHintText : "None";
    public string CurrentSelectionPromptLabel => _worldView != null ? _worldView.CurrentSelectionPromptText : "None";
    public string BattlePhaseLabel => _worldView != null ? _worldView.BattlePhaseText : "None";
    public string BattleCancelHintLabel => _worldView != null ? _worldView.BattleCancelHintText : "None";
    public string BattleFeedbackLabel => _worldView != null ? _worldView.BattleFeedbackText : "None";
    public string EnemyIntentLabel => _worldView != null ? _worldView.EnemyIntentText : "None";
    public string ResultPanelStateLabel => _worldView != null ? _worldView.ResultPanelStateText : "None";
    public string ResultPanelCityDispatchedFromLabel => _worldView != null ? _worldView.ResultPanelCityDispatchedFromText : "None";
    public string ResultPanelDungeonChosenLabel => GetRunResultText(snapshot => snapshot.DungeonLabel, _worldView != null ? _worldView.ResultPanelDungeonChosenText : "None");
    public string ResultPanelDungeonDangerLabel => _worldView != null ? _worldView.ResultPanelDungeonDangerText : "None";
    public string ResultPanelRouteChosenLabel => GetRunResultText(snapshot => snapshot.RouteLabel, _worldView != null ? _worldView.ResultPanelRouteChosenText : "None");
    public string ResultPanelRecommendedRouteLabel => _worldView != null ? _worldView.ResultPanelRecommendedRouteText : "None";
    public string ResultPanelFollowedRecommendationLabel => _worldView != null ? _worldView.ResultPanelFollowedRecommendationText : "None";
    public string ResultPanelManaShardsReturnedLabel => _worldView != null ? _worldView.ResultPanelManaShardsReturnedText : "None";
    public string ResultPanelStockBeforeLabel => _worldView != null ? _worldView.ResultPanelStockBeforeText : "None";
    public string ResultPanelStockAfterLabel => _worldView != null ? _worldView.ResultPanelStockAfterText : "None";
    public string ResultPanelStockDeltaLabel => _worldView != null ? _worldView.ResultPanelStockDeltaText : "None";
    public string ResultPanelNeedPressureBeforeLabel => _worldView != null ? _worldView.ResultPanelNeedPressureBeforeText : "None";
    public string ResultPanelNeedPressureAfterLabel => _worldView != null ? _worldView.ResultPanelNeedPressureAfterText : "None";
    public string ResultPanelDispatchReadinessBeforeLabel => _worldView != null ? _worldView.ResultPanelDispatchReadinessBeforeText : "None";
    public string ResultPanelDispatchReadinessAfterLabel => _worldView != null ? _worldView.ResultPanelDispatchReadinessAfterText : "None";
    public string ResultPanelRecoveryEtaLabel => _worldView != null ? _worldView.ResultPanelRecoveryEtaText : "None";
    public string ResultPanelRouteRiskLabel => _worldView != null ? _worldView.ResultPanelRouteRiskText : "None";
    public string ResultPanelTurnsTakenLabel => _worldView != null ? _worldView.ResultPanelTurnsTakenText : "0";
    public string ResultPanelLootGainedLabel => _worldView != null ? _worldView.ResultPanelLootGainedText : "None";
    public string ResultPanelBattleLootLabel => _worldView != null ? _worldView.ResultPanelBattleLootText : "None";
    public string ResultPanelChestLootLabel => _worldView != null ? _worldView.ResultPanelChestLootText : "None";
    public string ResultPanelEventLootLabel => _worldView != null ? _worldView.ResultPanelEventLootText : "None";
    public string ResultPanelEventChoiceLabel => GetRunResultEncounterText(snapshot => snapshot.SelectedEventChoice, _worldView != null ? _worldView.ResultPanelEventChoiceText : "None");
    public string ResultPanelSurvivingMembersLabel => GetRunResultText(snapshot => snapshot.SurvivingMembersSummary, _worldView != null ? _worldView.ResultPanelSurvivingMembersText : "None");
    public string ResultPanelClearedEncountersLabel => _worldView != null ? _worldView.ResultPanelClearedEncountersText : "0 / 3";
    public string ResultPanelOpenedChestsLabel => _worldView != null ? _worldView.ResultPanelOpenedChestsText : "0 / 1";
    public string ResultPanelEliteDefeatedLabel => _worldView != null ? _worldView.ResultPanelEliteDefeatedText : "None";
    public string ResultPanelEliteNameLabel => GetRunResultEliteText(snapshot => snapshot.EliteName, _worldView != null ? _worldView.ResultPanelEliteNameText : "None");
    public string ResultPanelEliteRewardIdentityLabel => GetRunResultEliteText(snapshot => snapshot.EliteRewardLabel, _worldView != null ? _worldView.ResultPanelEliteRewardIdentityText : "None");
    public string ResultPanelEliteRewardAmountLabel => _worldView != null ? _worldView.ResultPanelEliteRewardAmountText : "None";
    public string ResultPanelEliteRewardLabel => _worldView != null ? _worldView.ResultPanelEliteRewardText : "None";
    public string ResultPanelPreEliteChoiceLabel => GetRunResultEncounterText(snapshot => snapshot.SelectedPreEliteChoice, _worldView != null ? _worldView.ResultPanelPreEliteChoiceText : "Pending");
    public string ResultPanelPreEliteHealAmountLabel => _worldView != null ? _worldView.ResultPanelPreEliteHealAmountText : "None";
    public string ResultPanelEliteBonusRewardEarnedLabel => _worldView != null ? _worldView.ResultPanelEliteBonusRewardEarnedText : "None";
    public string ResultPanelEliteBonusRewardAmountLabel => _worldView != null ? _worldView.ResultPanelEliteBonusRewardAmountText : "None";
    public string ResultPanelRoomPathSummaryLabel => GetRunResultEncounterText(snapshot => snapshot.RoomPathSummary, _worldView != null ? _worldView.ResultPanelRoomPathSummaryText : "None");
    public string ResultPanelPartyHpSummaryLabel => GetRunResultPartyText(snapshot => snapshot.PartyHpSummaryText, _worldView != null ? _worldView.ResultPanelPartyHpSummaryText : "None");
    public string ResultPanelPartyConditionLabel => GetRunResultPartyText(snapshot => snapshot.PartyConditionText, _worldView != null ? _worldView.ResultPanelPartyConditionText : "None");
    public string ResultPanelBattleContextSummaryLabel => GetRunResultText(snapshot => snapshot.BattleContextSummaryText, "None");
    public string ResultPanelBattleRuntimeSummaryLabel => GetRunResultText(snapshot => snapshot.BattleRuntimeSummaryText, "None");
    public string ResultPanelBattleRuleSummaryLabel => GetRunResultText(snapshot => snapshot.BattleRuleSummaryText, "None");
    public string ResultPanelBattleResultCoreSummaryLabel => GetRunResultText(snapshot => snapshot.BattleResultCoreSummaryText, "None");
    public string ResultPanelNotableBattleEventsSummaryLabel => GetRunResultText(snapshot => snapshot.NotableBattleEventsSummaryText, "None");
    public string ResultPanelGearRewardCandidateLabel => GetRunResultText(snapshot => snapshot.GearRewardCandidateSummaryText, _worldView != null ? _worldView.ResultPanelGearRewardCandidateText : "None");
    public string ResultPanelEquipSwapChoiceLabel => GetRunResultText(snapshot => snapshot.EquipSwapChoiceSummaryText, _worldView != null ? _worldView.ResultPanelEquipSwapChoiceText : "None");
    public string ResultPanelGearCarryContinuityLabel => GetRunResultText(snapshot => snapshot.GearCarryContinuitySummaryText, _worldView != null ? _worldView.ResultPanelGearCarryContinuityText : "None");
    public string ResultPanelReturnPromptLabel => _worldView != null ? _worldView.ResultPanelReturnPromptText : "None";
    public string Party1HpLabel => _worldView != null ? _worldView.GetPartyMemberHpText(0) : "None";
    public string Party2HpLabel => _worldView != null ? _worldView.GetPartyMemberHpText(1) : "None";
    public string Party3HpLabel => _worldView != null ? _worldView.GetPartyMemberHpText(2) : "None";
    public string Party4HpLabel => _worldView != null ? _worldView.GetPartyMemberHpText(3) : "None";

    public string GetPartyMemberContributionLabel(int memberIndex)
    {
        PrototypeRpgMemberContributionSnapshot contribution = GetPartyMemberContributionSnapshot(memberIndex);
        bool hasStructuredContribution =
            contribution != null &&
            (!string.IsNullOrEmpty(contribution.MemberId) ||
             contribution.DamageDealt > 0 ||
             contribution.HealingDone > 0 ||
             contribution.ActionCount > 0 ||
             contribution.KillCount > 0);

        if (hasStructuredContribution)
        {
            return "D " + contribution.DamageDealt +
                   "  H " + contribution.HealingDone +
                   "  A " + contribution.ActionCount +
                   "  K " + contribution.KillCount;
        }

        return _worldView != null ? _worldView.GetPartyMemberContributionText(memberIndex) : "D 0  H 0  A 0  K 0";
    }

    public PrototypeBattleUiSurfaceData GetBattleUiSurfaceData()
    {
        return _worldView != null ? _worldView.BuildBattleUiSurfaceData() : new PrototypeBattleUiSurfaceData();
    }

    public PrototypeDungeonRunShellSurfaceData GetDungeonRunShellSurfaceData()
    {
        return _worldView != null ? _worldView.CurrentDungeonRunShellSurfaceData : new PrototypeDungeonRunShellSurfaceData();
    }

    public PrototypeBattleResultSnapshot GetLatestBattleResultSnapshot()
    {
        return _worldView != null ? _worldView.LatestBattleResultSnapshot : new PrototypeBattleResultSnapshot();
    }

    public PrototypeEnemyIntentSnapshot GetCurrentEnemyIntentSnapshot()
    {
        return _worldView != null ? _worldView.CurrentEnemyIntentSnapshot : new PrototypeEnemyIntentSnapshot();
    }

    public PrototypeBattleEventRecord[] GetRecentBattleEventRecords()
    {
        return _worldView != null ? _worldView.RecentBattleEventSnapshotRecords : System.Array.Empty<PrototypeBattleEventRecord>();
    }

    public PrototypeBattleEventRecord GetLatestBattleEventRecord()
    {
        return _worldView != null ? _worldView.LatestBattleEventSnapshot : new PrototypeBattleEventRecord();
    }

    public PrototypeRpgRunResultSnapshot GetLatestRpgRunResultSnapshot()
    {
        return _worldView != null ? _worldView.LatestRpgRunResultSnapshot : new PrototypeRpgRunResultSnapshot();
    }

    public PrototypeRpgCombatContributionSnapshot GetLatestRpgCombatContributionSnapshot()
    {
        return _worldView != null ? _worldView.LatestRpgCombatContributionSnapshot : new PrototypeRpgCombatContributionSnapshot();
    }

    public PrototypeRpgProgressionSeedSnapshot GetLatestRpgProgressionSeedSnapshot()
    {
        return _worldView != null ? _worldView.LatestRpgProgressionSeedSnapshot : new PrototypeRpgProgressionSeedSnapshot();
    }

    public PrototypeRpgProgressionPreviewSnapshot GetLatestRpgProgressionPreviewSnapshot()
    {
        return _worldView != null ? _worldView.LatestRpgProgressionPreviewSnapshot : new PrototypeRpgProgressionPreviewSnapshot();
    }

    public ExpeditionStartContext GetLatestExpeditionStartContext()
    {
        return _worldView != null ? _worldView.LatestExpeditionStartContext : new ExpeditionStartContext();
    }

    public PrototypeDungeonPanelContext GetCurrentDungeonPanelContext()
    {
        return _worldView != null ? _worldView.CurrentDungeonPanelContext : new PrototypeDungeonPanelContext();
    }

    public PrototypeBattleContextData GetCurrentBattleContextData()
    {
        return _worldView != null ? _worldView.CurrentBattleContextSnapshot : new PrototypeBattleContextData();
    }

    public PrototypeDungeonRunResultContext GetLatestDungeonRunResultContext()
    {
        return _worldView != null ? _worldView.LatestDungeonRunResultContext : new PrototypeDungeonRunResultContext();
    }

    public PrototypeDungeonChoiceOutcome GetLatestDungeonChoiceOutcome()
    {
        return _worldView != null ? _worldView.LatestDungeonChoiceOutcome : new PrototypeDungeonChoiceOutcome();
    }

    public PrototypeDungeonEventResolution GetLatestDungeonEventResolution()
    {
        return _worldView != null ? _worldView.LatestDungeonEventResolution : new PrototypeDungeonEventResolution();
    }

    public PrototypeDungeonExtractionResult GetLatestDungeonExtractionResult()
    {
        return _worldView != null ? _worldView.LatestDungeonExtractionResult : new PrototypeDungeonExtractionResult();
    }

    public PrototypeRpgMemberContributionSnapshot GetPartyMemberContributionSnapshot(int memberIndex)
    {
        return _worldView != null ? _worldView.GetLatestRpgMemberContributionSnapshot(memberIndex) : new PrototypeRpgMemberContributionSnapshot();
    }

    public PrototypeRpgMemberProgressionSeed GetPartyMemberProgressionSeed(int memberIndex)
    {
        return _worldView != null ? _worldView.GetLatestRpgMemberProgressionSeed(memberIndex) : new PrototypeRpgMemberProgressionSeed();
    }

    public PrototypeRpgMemberProgressPreview GetPartyMemberProgressPreview(int memberIndex)
    {
        return _worldView != null ? _worldView.GetLatestRpgMemberProgressPreview(memberIndex) : new PrototypeRpgMemberProgressPreview();
    }

    private string GetRunResultText(System.Func<PrototypeRpgRunResultSnapshot, string> selector, string legacyFallback)
    {
        PrototypeRpgRunResultSnapshot snapshot = GetLatestRpgRunResultSnapshot();
        string value = snapshot != null ? selector(snapshot) : string.Empty;
        return string.IsNullOrEmpty(value) ? legacyFallback : value;
    }

    private string GetRunResultPartyText(System.Func<PrototypeRpgPartyOutcomeSnapshot, string> selector, string legacyFallback)
    {
        PrototypeRpgRunResultSnapshot snapshot = GetLatestRpgRunResultSnapshot();
        PrototypeRpgPartyOutcomeSnapshot partyOutcome = snapshot != null ? snapshot.PartyOutcome : null;
        string value = partyOutcome != null ? selector(partyOutcome) : string.Empty;
        return string.IsNullOrEmpty(value) ? legacyFallback : value;
    }

    private string GetRunResultEliteText(System.Func<PrototypeRpgEliteOutcomeSnapshot, string> selector, string legacyFallback)
    {
        PrototypeRpgRunResultSnapshot snapshot = GetLatestRpgRunResultSnapshot();
        PrototypeRpgEliteOutcomeSnapshot eliteOutcome = snapshot != null ? snapshot.EliteOutcome : null;
        string value = eliteOutcome != null ? selector(eliteOutcome) : string.Empty;
        return string.IsNullOrEmpty(value) ? legacyFallback : value;
    }

    private string GetRunResultEncounterText(System.Func<PrototypeRpgEncounterOutcomeSnapshot, string> selector, string legacyFallback)
    {
        PrototypeRpgRunResultSnapshot snapshot = GetLatestRpgRunResultSnapshot();
        PrototypeRpgEncounterOutcomeSnapshot encounterOutcome = snapshot != null ? snapshot.EncounterOutcome : null;
        string value = encounterOutcome != null ? selector(encounterOutcome) : string.Empty;
        return string.IsNullOrEmpty(value) ? legacyFallback : value;
    }
    private void Awake()
    {
        _resources = PlaceholderResourceDataFactory.Create();
        _worldView = new StaticPlaceholderWorldView(_resources);
        EnsureSceneStateBridge();
        EnsureInputGate();
        EnsureWorldCameraController();
        _gameFlowCoordinator = new GameFlowCoordinator(
            new GameState(GameStateId.Boot),
            BootColor,
            MainMenuColor,
            WorldSimColor,
            DungeonRunColor,
            log: message => Debug.Log(message));
        Debug.Log("[Boot] Boot scene started successfully.");
        ApplyGameFlowPresentationState(force: true);
    }

    private void OnDestroy()
    {
        if (_worldView != null)
        {
            _worldView.Dispose();
            _worldView = null;
        }
    }

    private void Update()
    {
        if (!HasGameFlowCoordinator)
        {
            return;
        }

        if (TryAdvanceFromBootToMainMenu())
        {
            ApplyGameFlowPresentationState();
            return;
        }

        Keyboard keyboard = Keyboard.current;
        if (TryHandleGlobalKeyboardInput(keyboard))
        {
            ApplyGameFlowPresentationState();
            return;
        }

        if (CurrentState == GameStateId.WorldSim)
        {
            UpdateWorldSimState();
        }
        else if (CurrentState == GameStateId.DungeonRun)
        {
            UpdateDungeonRunState(keyboard);
        }

        ApplyGameFlowPresentationState();
    }

    public string GetText(string key)
    {
        return PrototypeLocalization.Get(_currentLanguage, key);
    }

    public string TranslateValue(string text)
    {
        return PrototypeLocalization.TranslateValue(_currentLanguage, text);
    }

    public WorldObservationSurfaceData GetWorldObservationSurfaceData()
    {
        return GetCurrentWorldObservationSurfaceData();
    }

    public PrototypeCityHubUiSurfaceData GetCityHubUiSurfaceData()
    {
        return GetCurrentCityHubUiSurfaceData();
    }

    public CityInteractionPresentationSurfaceData GetCityInteractionPresentationSurfaceData()
    {
        return GetCurrentCityInteractionPresentationSurfaceData();
    }

    private CityInteraction GetCityInteraction()
    {
        EnsureCityInteraction();
        return _cityInteraction;
    }

    public CityHubSurfaceData GetSelectedCityHubSurfaceData()
    {
        return GetCurrentCityHubSurfaceData();
    }

    public WorldSimCitySourceData GetSelectedCityHubEntrySnapshot()
    {
        CityHubSurfaceData cityHub = GetCurrentCityHubSurfaceData();
        return cityHub != null && cityHub.EntrySnapshot != null
            ? cityHub.EntrySnapshot
            : new WorldSimCitySourceData();
    }

    public ExpeditionPrepHandoff GetSelectedExpeditionPrepHandoff()
    {
        CityHubSurfaceData cityHub = GetCurrentCityHubSurfaceData();
        return cityHub != null && cityHub.ExpeditionPrep != null
            ? cityHub.ExpeditionPrep
            : new ExpeditionPrepHandoff();
    }

    public ExpeditionPrepSurfaceData GetSelectedExpeditionPrepSurfaceData()
    {
        return GetCurrentExpeditionPrepSurfaceData();
    }

    public ExpeditionLaunchRequest GetSelectedExpeditionLaunchRequest()
    {
        WorldObservationSurfaceData observation = GetCurrentWorldObservationSurfaceData();
        return observation != null &&
            observation.Launch != null &&
            observation.Launch.LaunchRequest != null
            ? observation.Launch.LaunchRequest
            : new ExpeditionLaunchRequest();
    }

    public OutcomeReadback GetSelectedOutcomeReadbackSurface()
    {
        CityHubSurfaceData cityHub = GetCurrentCityHubSurfaceData();
        return cityHub != null && cityHub.ResultPipelineReadback != null
            ? cityHub.ResultPipelineReadback
            : new OutcomeReadback();
    }

    public CityWriteback GetSelectedCityWritebackSurface()
    {
        CityHubSurfaceData cityHub = GetCurrentCityHubSurfaceData();
        return cityHub != null && cityHub.ResultPipelineCityWriteback != null
            ? cityHub.ResultPipelineCityWriteback
            : new CityWriteback();
    }

    public void SetLanguage(PrototypeLanguage language)
    {
        _currentLanguage = language;
    }

    public void ToggleLanguage()
    {
        _currentLanguage = _currentLanguage == PrototypeLanguage.English
            ? PrototypeLanguage.Korean
            : PrototypeLanguage.English;
    }

    public void EnterWorldSimFromMenu()
    {
        if (_gameFlowCoordinator != null && _gameFlowCoordinator.EnterWorldSim())
        {
            ApplyGameFlowPresentationState();
        }
    }

    public void ReturnToMainMenu()
    {
        if (_gameFlowCoordinator != null && _gameFlowCoordinator.ReturnToMainMenu())
        {
            ApplyGameFlowPresentationState();
        }
    }

    public void RunWorldDayStep()
    {
        if (IsWorldSimActive && _worldView != null)
        {
            _worldView.RunEconomyDay();
        }
    }

    public void ResetWorldSimulation()
    {
        if (IsWorldSimActive && _worldView != null)
        {
            _worldView.ResetRuntimeEconomy();
        }
    }

    public void ToggleWorldAutoTick()
    {
        if (IsWorldSimActive && _worldView != null)
        {
            _worldView.ToggleAutoTickEnabled();
        }
    }

    public void ToggleWorldAutoTickPause()
    {
        if (IsWorldSimActive && _worldView != null)
        {
            _worldView.ToggleAutoTickPaused();
        }
    }

    public PrototypeCityHubActionResult TryExecuteCityHubAction(PrototypeCityHubActionRequest request)
    {
        CityInteraction cityInteraction = GetCityInteraction();
        if (cityInteraction == null)
        {
            return new PrototypeCityHubActionResult
            {
                ActionKey = request != null ? request.ActionKey ?? string.Empty : string.Empty,
                ErrorSummaryText = "CityInteraction bridge is unavailable."
            };
        }

        PrototypeCityHubActionResult result = cityInteraction.TryExecuteCityHubAction(request);
        if (result == null)
        {
            return new PrototypeCityHubActionResult
            {
                ActionKey = request != null ? request.ActionKey ?? string.Empty : string.Empty,
                ErrorSummaryText = "CityInteraction returned no action result."
            };
        }

        if (result.Succeeded && result.TransitionToDungeonRunRequested)
        {
            bool didTransition = _gameFlowCoordinator != null && _gameFlowCoordinator.EnterDungeonRun();
            result.Succeeded = didTransition;
            if (!didTransition)
            {
                result.ErrorSummaryText = "CityHub action requested DungeonRun, but the game flow transition failed.";
            }
        }

        if (result.WasHandled && (result.Succeeded || result.ShouldRefreshPresentation))
        {
            InvalidateCachedCitySurfaceData();
            ApplyGameFlowPresentationState();
        }

        return result;
    }

    public void RecruitWorldSimParty()
    {
        TryExecuteCityHubAction(new PrototypeCityHubActionRequest
        {
            ActionKey = PrototypeCityHubActionKeys.RecruitSelectedCityParty,
            SourceSurfaceName = "BootEntry"
        });
    }

    public bool TryEnterSelectedWorldDungeon()
    {
        PrototypeCityHubActionResult result = TryExecuteCityHubAction(new PrototypeCityHubActionRequest
        {
            ActionKey = PrototypeCityHubActionKeys.EnterSelectedWorldDungeon,
            SourceSurfaceName = "BootEntry"
        });
        return result.WasHandled && result.Succeeded;
    }

    public bool TryOpenSelectedWorldExpeditionPrepBoard()
    {
        PrototypeCityHubActionResult result = TryExecuteCityHubAction(new PrototypeCityHubActionRequest
        {
            ActionKey = PrototypeCityHubActionKeys.OpenSelectedWorldExpeditionPrepBoard,
            SourceSurfaceName = "BootEntry"
        });
        return result.WasHandled && result.Succeeded;
    }

    public void CancelSelectedWorldExpeditionPrepBoard()
    {
        TryExecuteCityHubAction(new PrototypeCityHubActionRequest
        {
            ActionKey = PrototypeCityHubActionKeys.CancelSelectedWorldExpeditionPrepBoard,
            SourceSurfaceName = "BootEntry"
        });
    }

    public void AcknowledgePendingExpeditionPostRunReveal()
    {
        if (_worldView == null)
        {
            return;
        }

        _worldView.AcknowledgePendingExpeditionPostRunReveal();
        InvalidateCachedCitySurfaceData();
    }

    public bool TrySelectExpeditionPrepRoute(string optionKey)
    {
        PrototypeCityHubActionResult result = TryExecuteCityHubAction(new PrototypeCityHubActionRequest
        {
            ActionKey = PrototypeCityHubActionKeys.SelectExpeditionPrepRoute,
            OptionKey = optionKey ?? string.Empty,
            SourceSurfaceName = "BootEntry"
        });
        return result.WasHandled && result.Succeeded;
    }

    public bool TryCycleExpeditionPrepDispatchPolicy()
    {
        PrototypeCityHubActionResult result = TryExecuteCityHubAction(new PrototypeCityHubActionRequest
        {
            ActionKey = PrototypeCityHubActionKeys.CycleExpeditionPrepDispatchPolicy,
            SourceSurfaceName = "BootEntry"
        });
        return result.WasHandled && result.Succeeded;
    }

    public bool TryConfirmSelectedExpeditionLaunch()
    {
        PrototypeCityHubActionResult result = TryExecuteCityHubAction(new PrototypeCityHubActionRequest
        {
            ActionKey = PrototypeCityHubActionKeys.ConfirmSelectedExpeditionLaunch,
            SourceSurfaceName = "BootEntry"
        });
        return result.WasHandled && result.Succeeded;
    }

    public bool IsBattleActionAvailable(string actionKey)
    {
        return _worldView != null && _worldView.IsBattleActionAvailable(actionKey);
    }

    public bool IsBattleActionHovered(string actionKey)
    {
        return _worldView != null && _worldView.IsBattleActionHovered(actionKey);
    }

    public bool IsBattleActionSelected(string actionKey)
    {
        return _worldView != null && _worldView.IsBattleActionSelected(actionKey);
    }

    public void SetBattleActionHover(string actionKey)
    {
        if (_worldView != null)
        {
            _worldView.SetBattleActionHover(actionKey);
        }
    }

    public bool TryTriggerBattleAction(string actionKey)
    {
        return _worldView != null && _worldView.TryTriggerBattleAction(actionKey);
    }

    public void SetBattleTargetHover(string monsterId)
    {
        if (_worldView != null)
        {
            _worldView.SetBattleTargetHover(monsterId);
        }
    }

    public bool TryTriggerBattleTarget(string monsterId)
    {
        return _worldView != null && _worldView.TryTriggerBattleTarget(monsterId);
    }

    public bool IsLegacyDungeonRouteChoiceAvailable(string optionKey)
    {
        return _worldView != null && _worldView.IsRouteChoiceAvailable(optionKey);
    }

    public bool IsLegacyDungeonRouteChoiceHovered(string optionKey)
    {
        return _worldView != null && _worldView.IsRouteChoiceHovered(optionKey);
    }

    public bool IsLegacyDungeonRouteChoiceSelected(string optionKey)
    {
        return _worldView != null && _worldView.IsRouteChoiceSelected(optionKey);
    }

    public void SetLegacyDungeonRouteChoiceHover(string optionKey)
    {
        if (_worldView != null)
        {
            _worldView.SetRouteChoiceHover(optionKey);
        }
    }

    public bool CanConfirmLegacyDungeonRouteChoice()
    {
        return _worldView != null && _worldView.CanConfirmRouteChoice();
    }

    public bool TryConfirmLegacyDungeonRouteChoice()
    {
        return _worldView != null && _worldView.TryConfirmRouteChoice();
    }

    public bool TryTriggerLegacyDungeonRouteChoice(string optionKey)
    {
        return _worldView != null && _worldView.TryTriggerRouteChoice(optionKey);
    }

    public void SetDungeonPanelLaneHover(string optionKey)
    {
        if (_worldView != null)
        {
            _worldView.SetDungeonPanelLaneHover(optionKey);
        }
    }

    public bool TryTriggerDungeonPanelLaneOption(string optionKey)
    {
        return _worldView != null && _worldView.TryTriggerDungeonPanelLaneOption(optionKey);
    }

    public bool TryCycleCurrentDispatchPolicy()
    {
        return _worldView != null && _worldView.TryCycleCurrentDispatchPolicy();
    }

    public bool IsDungeonRunEventDecisionAvailable(string optionKey)
    {
        return _worldView != null && _worldView.IsEventChoiceAvailable(optionKey);
    }

    public bool IsDungeonRunEventDecisionHovered(string optionKey)
    {
        return _worldView != null && _worldView.IsEventChoiceHovered(optionKey);
    }

    public bool IsDungeonRunEventDecisionSelected(string optionKey)
    {
        return _worldView != null && _worldView.IsEventChoiceSelected(optionKey);
    }

    public void SetDungeonRunEventDecisionHover(string optionKey)
    {
        if (_worldView != null)
        {
            _worldView.SetEventChoiceHover(optionKey);
        }
    }

    public bool TryTriggerDungeonRunEventDecision(string optionKey)
    {
        return _worldView != null && _worldView.TryTriggerEventChoice(optionKey);
    }

    public bool IsDungeonRunPreEliteDecisionAvailable(string optionKey)
    {
        return _worldView != null && _worldView.IsPreEliteChoiceAvailable(optionKey);
    }

    public bool IsDungeonRunPreEliteDecisionHovered(string optionKey)
    {
        return _worldView != null && _worldView.IsPreEliteChoiceHovered(optionKey);
    }

    public bool IsDungeonRunPreEliteDecisionSelected(string optionKey)
    {
        return _worldView != null && _worldView.IsPreEliteChoiceSelected(optionKey);
    }

    public void SetDungeonRunPreEliteDecisionHover(string optionKey)
    {
        if (_worldView != null)
        {
            _worldView.SetPreEliteChoiceHover(optionKey);
        }
    }

    public bool TryTriggerDungeonRunPreEliteDecision(string optionKey)
    {
        return _worldView != null && _worldView.TryTriggerPreEliteChoice(optionKey);
    }
    private void HandleWorldSimEconomyInput(Keyboard keyboard)
    {
        if (keyboard == null || _worldView == null)
        {
            return;
        }

        if (keyboard.tKey.wasPressedThisFrame)
        {
            _worldView.RunEconomyDay();
        }

        if (keyboard.rKey.wasPressedThisFrame)
        {
            _worldView.ResetRuntimeEconomy();
        }

        if (keyboard.yKey.wasPressedThisFrame)
        {
            _worldView.ToggleAutoTickEnabled();
        }

        if (keyboard.pKey.wasPressedThisFrame)
        {
            _worldView.ToggleAutoTickPaused();
        }

        if (keyboard.gKey.wasPressedThisFrame)
        {
            RecruitWorldSimParty();
        }


        if (keyboard.xKey.wasPressedThisFrame)
        {
            TryEnterSelectedWorldDungeon();
        }
    }

    private void HandleSelectionInput()
    {
        if (_worldView == null)
        {
            return;
        }

        Mouse mouse = Mouse.current;
        Camera mainCamera = Camera.main;
        if (mouse == null || mainCamera == null || !mouse.leftButton.wasPressedThisFrame)
        {
            return;
        }

        Vector2 screenPosition = mouse.position.ReadValue();
        EnsureInputGate();
        if (_inputGate != null && _inputGate.IsWorldSelectionBlocked(screenPosition))
        {
            return;
        }

        _worldView.SelectAtScreenPosition(mainCamera, screenPosition);
    }

    private string GetResourceIdsLabel()
    {
        if (_resources == null || _resources.Length == 0)
        {
            return "None";
        }

        string[] ids = new string[_resources.Length];
        for (int i = 0; i < _resources.Length; i++)
        {
            ResourceData resource = _resources[i];
            ids[i] = resource != null && !string.IsNullOrEmpty(resource.Id)
                ? resource.Id
                : "(missing)";
        }

        return string.Join(", ", ids);
    }

    private WorldObservationSurfaceData GetCurrentWorldObservationSurfaceData()
    {
        if (_worldView == null)
        {
            return new WorldObservationSurfaceData();
        }

        if (_cachedWorldObservationSurfaceData == null || _cachedWorldObservationSurfaceFrame != Time.frameCount)
        {
            _cachedWorldObservationSurfaceData = _worldView.BuildWorldObservationSurfaceData();
            _worldView.ApplyWorldBoardEmphasis(_cachedWorldObservationSurfaceData);
            _cachedWorldObservationSurfaceFrame = Time.frameCount;
        }

        return _cachedWorldObservationSurfaceData;
    }

    private CityHubSurfaceData GetCurrentCityHubSurfaceData()
    {
        WorldObservationSurfaceData observation = GetCurrentWorldObservationSurfaceData();
        return observation != null && observation.CityHub != null
            ? observation.CityHub
            : new CityHubSurfaceData();
    }

    private PrototypeCityHubUiSurfaceData GetCurrentCityHubUiSurfaceData()
    {
        if (_worldView == null)
        {
            return new PrototypeCityHubUiSurfaceData();
        }

        if (_cachedCityHubUiSurfaceData == null || _cachedCityHubUiSurfaceFrame != Time.frameCount)
        {
            CityInteraction cityInteraction = GetCityInteraction();
            WorldObservationSurfaceData observation = GetCurrentWorldObservationSurfaceData();
            _cachedCityHubUiSurfaceData = cityInteraction != null
                ? cityInteraction.BuildUiSurfaceData(observation)
                : new PrototypeCityHubUiSurfaceData();
            _cachedCityHubUiSurfaceFrame = Time.frameCount;
        }

        return _cachedCityHubUiSurfaceData;
    }

    // Keep world-facing compatibility getters routed through grouped bridge surfaces.
    private PrototypeCityHubHeaderSurfaceData GetCurrentCityHubHeaderSurfaceData()
    {
        PrototypeCityHubUiSurfaceData cityHubUi = GetCurrentCityHubUiSurfaceData();
        return cityHubUi != null && cityHubUi.Header != null
            ? cityHubUi.Header
            : new PrototypeCityHubHeaderSurfaceData();
    }

    private PrototypeCityHubSelectionSurfaceData GetCurrentCityHubSelectionSurfaceData()
    {
        PrototypeCityHubUiSurfaceData cityHubUi = GetCurrentCityHubUiSurfaceData();
        return cityHubUi != null && cityHubUi.Selection != null
            ? cityHubUi.Selection
            : new PrototypeCityHubSelectionSurfaceData();
    }

    private PrototypeCityHubActionSurfaceData GetCurrentCityHubActionSurfaceData()
    {
        PrototypeCityHubUiSurfaceData cityHubUi = GetCurrentCityHubUiSurfaceData();
        return cityHubUi != null && cityHubUi.Actions != null
            ? cityHubUi.Actions
            : new PrototypeCityHubActionSurfaceData();
    }

    private PrototypeCityHubOutcomeSurfaceData GetCurrentCityHubOutcomeSurfaceData()
    {
        PrototypeCityHubUiSurfaceData cityHubUi = GetCurrentCityHubUiSurfaceData();
        return cityHubUi != null && cityHubUi.Outcome != null
            ? cityHubUi.Outcome
            : new PrototypeCityHubOutcomeSurfaceData();
    }

    private PrototypeCityHubOverviewSurfaceData GetCurrentCityHubOverviewSurfaceData()
    {
        PrototypeCityHubUiSurfaceData cityHubUi = GetCurrentCityHubUiSurfaceData();
        return cityHubUi != null && cityHubUi.Overview != null
            ? cityHubUi.Overview
            : new PrototypeCityHubOverviewSurfaceData();
    }

    private PrototypeCityHubLogSurfaceData GetCurrentCityHubLogSurfaceData()
    {
        PrototypeCityHubUiSurfaceData cityHubUi = GetCurrentCityHubUiSurfaceData();
        return cityHubUi != null && cityHubUi.Logs != null
            ? cityHubUi.Logs
            : new PrototypeCityHubLogSurfaceData();
    }

    private void InvalidateCachedCitySurfaceData()
    {
        _cachedWorldObservationSurfaceFrame = -1;
        _cachedCityHubUiSurfaceFrame = -1;
        _cachedCityInteractionPresentationSurfaceFrame = -1;
    }

    private static string GetIndexedLogText(string[] values, int index)
    {
        return values != null &&
               index >= 0 &&
               index < values.Length &&
               !string.IsNullOrEmpty(values[index])
            ? values[index]
            : "None";
    }

    private CityInteractionPresentationSurfaceData GetCurrentCityInteractionPresentationSurfaceData()
    {
        if (_worldView == null)
        {
            return new CityInteractionPresentationSurfaceData();
        }

        if (_cachedCityInteractionPresentationSurfaceData == null ||
            _cachedCityInteractionPresentationSurfaceFrame != Time.frameCount)
        {
            CityInteraction cityInteraction = GetCityInteraction();
            _cachedCityInteractionPresentationSurfaceData = cityInteraction != null
                ? cityInteraction.BuildPresentationSurfaceData(GetCurrentCityHubUiSurfaceData())
                : new CityInteractionPresentationSurfaceData();
            _cachedCityInteractionPresentationSurfaceFrame = Time.frameCount;
        }

        return _cachedCityInteractionPresentationSurfaceData;
    }

    private ExpeditionPrepSurfaceData GetCurrentExpeditionPrepSurfaceData()
    {
        WorldObservationSurfaceData observation = GetCurrentWorldObservationSurfaceData();
        return observation != null && observation.ExpeditionPrep != null
            ? observation.ExpeditionPrep
            : new ExpeditionPrepSurfaceData();
    }

    private void EnsureCityInteraction()
    {
        if (_cityInteraction == null && _worldView != null)
        {
            _cityInteraction = new CityInteraction(_worldView, _gameFlowCoordinator, ResolveMainCamera);
        }
    }

    private static string FormatObservationInt(int value)
    {
        return value > 0 ? value.ToString() : "None";
    }

    private static string FormatObservationText(string value)
    {
        return string.IsNullOrEmpty(value) ? "None" : value;
    }

    private bool TryAdvanceFromBootToMainMenu()
    {
        return _gameFlowCoordinator != null && _gameFlowCoordinator.TryAdvanceFromBoot(Time.unscaledTime);
    }

    private bool TryHandleGlobalKeyboardInput(Keyboard keyboard)
    {
        if (keyboard == null)
        {
            return false;
        }

        bool blockKeyboardShortcuts = AreKeyboardShortcutsBlocked();
        if (!blockKeyboardShortcuts && keyboard.lKey.wasPressedThisFrame)
        {
            ToggleLanguage();
        }

        if (TryHandleMainMenuInput(keyboard, blockKeyboardShortcuts))
        {
            return true;
        }

        if (TryHandleWorldSimNavigationInput(keyboard, blockKeyboardShortcuts))
        {
            return true;
        }

        if (!blockKeyboardShortcuts && CurrentState == GameStateId.WorldSim)
        {
            HandleWorldSimEconomyInput(keyboard);
        }

        return false;
    }

    private bool TryHandleMainMenuInput(Keyboard keyboard, bool blockKeyboardShortcuts)
    {
        if (blockKeyboardShortcuts || CurrentState != GameStateId.MainMenu)
        {
            return false;
        }

        if (!keyboard.enterKey.wasPressedThisFrame &&
            !keyboard.numpadEnterKey.wasPressedThisFrame &&
            !keyboard.spaceKey.wasPressedThisFrame)
        {
            return false;
        }

        EnterWorldSimFromMenu();
        return CurrentState == GameStateId.WorldSim;
    }

    private bool TryHandleWorldSimNavigationInput(Keyboard keyboard, bool blockKeyboardShortcuts)
    {
        if (CurrentState != GameStateId.WorldSim || blockKeyboardShortcuts)
        {
            return false;
        }

        if (IsExpeditionPrepBoardOpen)
        {
            if (keyboard.escapeKey.wasPressedThisFrame)
            {
                CancelSelectedWorldExpeditionPrepBoard();
                return true;
            }

            if (keyboard.enterKey.wasPressedThisFrame || keyboard.numpadEnterKey.wasPressedThisFrame)
            {
                return TryConfirmSelectedExpeditionLaunch();
            }

            if (keyboard.qKey.wasPressedThisFrame)
            {
                return TryCycleExpeditionPrepDispatchPolicy();
            }

            if (keyboard.digit1Key.wasPressedThisFrame || keyboard.numpad1Key.wasPressedThisFrame)
            {
                return TrySelectExpeditionPrepRoute("safe");
            }

            if (keyboard.digit2Key.wasPressedThisFrame || keyboard.numpad2Key.wasPressedThisFrame)
            {
                return TrySelectExpeditionPrepRoute("risky");
            }
        }

        if (!keyboard.escapeKey.wasPressedThisFrame)
        {
            return false;
        }

        if (HasPendingExpeditionPostRunReveal)
        {
            AcknowledgePendingExpeditionPostRunReveal();
            return true;
        }

        ReturnToMainMenu();
        return CurrentState == GameStateId.MainMenu;
    }

    private void UpdateWorldSimState()
    {
        if (_worldView != null)
        {
            _worldView.UpdateAutoTick(Time.deltaTime);
        }

        HandleSelectionInput();
    }

    private void UpdateDungeonRunState(Keyboard keyboard)
    {
        if (_worldView == null)
        {
            return;
        }

        _worldView.UpdateDungeonRun(ResolveDungeonKeyboardInput(keyboard), ResolveDungeonPointerInput(), Camera.main);
        TryExitDungeonRunToWorldSim();
    }

    private bool AreKeyboardShortcutsBlocked()
    {
        EnsureInputGate();
        return _inputGate != null && _inputGate.AreKeyboardShortcutsBlocked();
    }

    private Keyboard ResolveDungeonKeyboardInput(Keyboard keyboard)
    {
        EnsureInputGate();
        return _inputGate != null ? _inputGate.ResolveDungeonKeyboardInput(keyboard) : keyboard;
    }

    private Mouse ResolveDungeonPointerInput()
    {
        EnsureInputGate();
        return _inputGate != null ? _inputGate.ResolveDungeonPointerInput() : Mouse.current;
    }

    private void EnsureSceneStateBridge()
    {
        _sceneStateBridge = GetComponent<BootstrapSceneStateBridge>();
        if (_sceneStateBridge == null)
        {
            _sceneStateBridge = gameObject.AddComponent<BootstrapSceneStateBridge>();
        }
    }

    private bool TryEnterDungeonRunFromSelectedWorldDungeon()
    {
        if (!IsWorldSimActive || _worldView == null || _gameFlowCoordinator == null)
        {
            return false;
        }

        Camera mainCamera = ResolveMainCamera();
        bool didEnterDungeonRun = mainCamera != null &&
                                  _worldView.TryEnterSelectedCityDungeon(mainCamera) &&
                                  _gameFlowCoordinator.EnterDungeonRun();
        if (didEnterDungeonRun)
        {
            ApplyGameFlowPresentationState();
        }

        return didEnterDungeonRun;
    }

    private bool TryExitDungeonRunToWorldSim()
    {
        return _worldView != null &&
               _gameFlowCoordinator != null &&
               _worldView.ConsumeDungeonRunExitRequest() &&
               _gameFlowCoordinator.ExitDungeonRunToWorldSim();
    }

    private void ApplyGameFlowPresentationState(bool force = false)
    {
        if (_gameFlowCoordinator == null)
        {
            return;
        }

        GameFlowPresentationState presentationState = _gameFlowCoordinator.GetCurrentPresentationState();
        if (!force &&
            _hasAppliedGameFlowPresentationState &&
            presentationState.CurrentStateId == _lastAppliedGameFlowStateId)
        {
            return;
        }

        ApplyBackgroundColor(presentationState);
        UpdateCameraControl(presentationState);
        UpdateWorldVisibility(presentationState);
        _lastAppliedGameFlowStateId = presentationState.CurrentStateId;
        _hasAppliedGameFlowPresentationState = true;
    }

    private void UpdateWorldVisibility(GameFlowPresentationState presentationState)
    {
        if (_worldView == null || presentationState == null)
        {
            return;
        }

        _worldView.SetWorldSimVisible(presentationState.ShowWorldSim);
        _worldView.SetDungeonRunVisible(presentationState.ShowDungeonRun);
    }

    private void UpdateCameraControl(GameFlowPresentationState presentationState)
    {
        if (presentationState == null)
        {
            return;
        }

        WorldCameraController worldCameraController = ResolveWorldCameraController();
        if (worldCameraController != null)
        {
            worldCameraController.SetWorldSimActive(presentationState.EnableWorldCamera);
        }

        if (presentationState.RequiresDungeonCameraConfigure && _worldView != null)
        {
            Camera mainCamera = ResolveMainCamera();
            if (mainCamera != null)
            {
                _worldView.ConfigureDungeonCamera(mainCamera);
            }
        }
    }

    private void ApplyBackgroundColor(GameFlowPresentationState presentationState)
    {
        Camera mainCamera = ResolveMainCamera();
        if (mainCamera == null || presentationState == null)
        {
            return;
        }

        mainCamera.clearFlags = CameraClearFlags.SolidColor;
        mainCamera.backgroundColor = presentationState.BackgroundColor;
    }

    private void EnsureInputGate()
    {
        _inputGate = GetComponent<BootstrapInputGate>();
        if (_inputGate == null)
        {
            _inputGate = gameObject.AddComponent<BootstrapInputGate>();
        }
    }

    private void EnsureWorldCameraController()
    {
        if (_worldCameraController != null)
        {
            return;
        }

        Camera mainCamera = Camera.main;
        if (mainCamera == null)
        {
            return;
        }

        _worldCameraController = mainCamera.GetComponent<WorldCameraController>();
        if (_worldCameraController == null)
        {
            _worldCameraController = mainCamera.gameObject.AddComponent<WorldCameraController>();
        }
    }

    private Camera ResolveMainCamera()
    {
        return Camera.main;
    }

    private WorldCameraController ResolveWorldCameraController()
    {
        EnsureWorldCameraController();
        return _worldCameraController;
    }
}













