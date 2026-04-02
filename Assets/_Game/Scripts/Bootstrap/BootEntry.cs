using UnityEngine;
using UnityEngine.InputSystem;

public sealed class BootEntry : MonoBehaviour
{
    private const float MainMenuDelaySeconds = 0.85f;
    private static readonly Color BootColor = new Color(0.08f, 0.09f, 0.12f, 1f);
    private static readonly Color MainMenuColor = new Color(0.14f, 0.17f, 0.2f, 1f);
    private static readonly Color WorldSimColor = new Color(0.11f, 0.16f, 0.12f, 1f);
    private static readonly Color DungeonRunColor = new Color(0.1f, 0.1f, 0.14f, 1f);

    private GameState _gameState;
    private StaticPlaceholderWorldView _worldView;
    private WorldCameraController _worldCameraController;
    private PrototypeDebugHUD _debugHud;
    private ResourceData[] _resources;
    private bool _hasTransitioned;
    private PrototypeLanguage _currentLanguage = PrototypeLanguage.English;

    public PrototypeLanguage CurrentLanguage => _currentLanguage;
    public string CurrentLanguageLabel => PrototypeLocalization.GetLanguageDisplayName(_currentLanguage);
    public string PrototypeNameLabel => GetText("PrototypeName");
    public string DebugStepLabel => GetText("StepLabel");
    public string CurrentStateLabel => _gameState != null ? _gameState.CurrentState.ToString() : "(missing)";
    public string LastTransitionLabel => _gameState != null ? _gameState.LastTransition : "(missing)";
    public int VisibleCityCount => StaticPlaceholderWorldView.CityCount;
    public int VisibleDungeonCount => StaticPlaceholderWorldView.DungeonCount;
    public int VisibleRoadCount => StaticPlaceholderWorldView.RoadCount;
    public int WorldEntityCount => _worldView != null ? _worldView.EntityCount : 0;
    public int WorldRouteCount => _worldView != null ? _worldView.RouteCount : 0;
    public int ResourceCount => _resources != null ? _resources.Length : 0;
    public int TradeOpportunityCount => _worldView != null ? _worldView.TradeOpportunityCount : 0;
    public int ActiveTradeOpportunityCount => _worldView != null ? _worldView.ActiveTradeOpportunityCount : 0;
    public int UnmetCityNeedsCount => _worldView != null ? _worldView.UnmetCityNeedCount : 0;
    public bool AutoTickEnabled => _worldView != null && _worldView.AutoTickEnabled;
    public bool AutoTickPaused => _worldView != null && _worldView.AutoTickPaused;
    public float TickIntervalSeconds => _worldView != null ? _worldView.TickIntervalSeconds : 0f;
    public int AutoTickCount => _worldView != null ? _worldView.AutoTickCount : 0;
    public int WorldDayCount => _worldView != null ? _worldView.WorldDayCount : 0;
    public int TradeStepCount => _worldView != null ? _worldView.TradeStepCount : 0;
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
    public string CitiesWithShortagesLabel => _worldView != null ? _worldView.CitiesWithShortagesText : "None";
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
    public int TotalParties => _worldView != null ? _worldView.TotalParties : 0;
    public int IdleParties => _worldView != null ? _worldView.IdleParties : 0;
    public int ActiveExpeditions => _worldView != null ? _worldView.ActiveExpeditions : 0;
    public int AvailableContracts => _worldView != null ? _worldView.AvailableContracts : 0;
    public int ExpeditionSuccessCount => _worldView != null ? _worldView.ExpeditionSuccessCount : 0;
    public int ExpeditionFailureCount => _worldView != null ? _worldView.ExpeditionFailureCount : 0;
    public int ExpeditionLootReturnedTotal => _worldView != null ? _worldView.ExpeditionLootReturnedTotal : 0;
    public string RecentDayLog1Label => _worldView != null ? _worldView.RecentDayLog1Text : "None";
    public string RecentDayLog2Label => _worldView != null ? _worldView.RecentDayLog2Text : "None";
    public string RecentDayLog3Label => _worldView != null ? _worldView.RecentDayLog3Text : "None";
    public string RecentExpeditionLog1Label => _worldView != null ? _worldView.RecentExpeditionLog1Text : "None";
    public string RecentExpeditionLog2Label => _worldView != null ? _worldView.RecentExpeditionLog2Text : "None";
    public string RecentExpeditionLog3Label => _worldView != null ? _worldView.RecentExpeditionLog3Text : "None";
    public string SelectedDisplayName => _worldView != null ? _worldView.SelectedDisplayName : "None";
    public string SelectedTypeLabel => _worldView != null ? _worldView.SelectedKind : "None";
    public string SelectedPositionLabel => _worldView != null ? _worldView.SelectedPositionText : "None";
    public string SelectedIdLabel => _worldView != null ? _worldView.SelectedId : "None";
    public string SelectedResourcesLabel => _worldView != null ? _worldView.SelectedResourcesText : "None";
    public string SelectedResourceRolesLabel => _worldView != null ? _worldView.SelectedResourceRolesText : "None";
    public string SelectedSupplyLabel => _worldView != null ? _worldView.SelectedSupplyText : "None";
    public string SelectedNeedsLabel => _worldView != null ? _worldView.SelectedNeedsText : "None";
    public string SelectedHighPriorityNeedsLabel => _worldView != null ? _worldView.SelectedHighPriorityNeedsText : "None";
    public string SelectedNormalPriorityNeedsLabel => _worldView != null ? _worldView.SelectedNormalPriorityNeedsText : "None";
    public string SelectedOutputLabel => _worldView != null ? _worldView.SelectedOutputText : "None";
    public string SelectedProcessingLabel => _worldView != null ? _worldView.SelectedProcessingText : "None";
    public string SelectedLinkedCityLabel => _worldView != null ? _worldView.SelectedLinkedCityText : "None";
    public string SelectedPartiesInCityLabel => _worldView != null ? _worldView.SelectedPartiesInCityText : "None";
    public string SelectedIdlePartiesLabel => _worldView != null ? _worldView.SelectedIdlePartiesText : "None";
    public string SelectedActiveExpeditionsFromCityLabel => _worldView != null ? _worldView.SelectedActiveExpeditionsFromCityText : "None";
    public string SelectedAvailableContractLabel => _worldView != null ? _worldView.SelectedAvailableContractText : "None";
    public string SelectedLinkedDungeonLabel => _worldView != null ? _worldView.SelectedLinkedDungeonText : "None";
    public string SelectedDungeonDangerLabel => _worldView != null ? _worldView.SelectedDungeonDangerText : "None";
    public string SelectedCityManaShardStockLabel => _worldView != null ? _worldView.SelectedCityManaShardStockText : "None";

    public string SelectedNeedPressureLabel => _worldView != null ? _worldView.SelectedNeedPressureText : "None";
    public string SelectedDispatchReadinessLabel => _worldView != null ? _worldView.SelectedDispatchReadinessText : "None";
    public string SelectedDispatchRecoveryProgressLabel => _worldView != null ? _worldView.SelectedDispatchRecoveryProgressText : "None";
    public string SelectedDispatchPolicyLabel => _worldView != null ? _worldView.SelectedDispatchPolicyText : "None";
    public string SelectedRecommendedRouteSummaryLabel => _worldView != null ? _worldView.SelectedRecommendedRouteText : "None";

    public string SelectedRecommendedRouteForLinkedCityLabel => _worldView != null ? _worldView.SelectedRecommendedRouteForLinkedCityText : "None";
    public string SelectedRecommendationReasonLabel => _worldView != null ? _worldView.SelectedRecommendationReasonText : "None";

    public string SelectedLastDispatchImpactLabel => _worldView != null ? _worldView.SelectedLastDispatchImpactText : "None";
    public string SelectedLastDispatchStockDeltaLabel => _worldView != null ? _worldView.SelectedLastDispatchStockDeltaText : "None";
    public string SelectedLastNeedPressureChangeLabel => _worldView != null ? _worldView.SelectedLastNeedPressureChangeText : "None";
    public string SelectedLastDispatchReadinessChangeLabel => _worldView != null ? _worldView.SelectedLastDispatchReadinessChangeText : "None";
    public string SelectedRecoveryEtaLabel => _worldView != null ? _worldView.SelectedRecoveryEtaText : "None";
    public string SelectedRecommendedPowerLabel => _worldView != null ? _worldView.SelectedRecommendedPowerText : "None";
    public string SelectedExpeditionDurationDaysLabel => _worldView != null ? _worldView.SelectedExpeditionDurationDaysText : "None";
    public string SelectedRewardPreviewLabel => _worldView != null ? _worldView.SelectedRewardPreviewText : "None";
    public string SelectedEventPreviewLabel => _worldView != null ? _worldView.SelectedEventPreviewText : "None";
    public string SelectedRoutePreview1Label => _worldView != null ? _worldView.SelectedRoutePreview1Text : "None";
    public string SelectedRoutePreview2Label => _worldView != null ? _worldView.SelectedRoutePreview2Text : "None";
    public string SelectedActiveExpeditionsLabel => _worldView != null ? _worldView.SelectedActiveExpeditionsText : "None";
    public string SelectedLastExpeditionResultLabel => _worldView != null ? _worldView.SelectedLastExpeditionResultText : "None";
    public string SelectedExpeditionLootReturnedLabel => _worldView != null ? _worldView.SelectedExpeditionLootReturnedText : "None";
    public string SelectedLastRunSurvivingMembersLabel => _worldView != null ? _worldView.SelectedLastRunSurvivingMembersText : "None";
    public string SelectedLastRunClearedEncountersLabel => _worldView != null ? _worldView.SelectedLastRunClearedEncountersText : "None";
    public string SelectedLastRunEventChoiceLabel => _worldView != null ? _worldView.SelectedLastRunEventChoiceText : "None";
    public string SelectedLastRunLootBreakdownLabel => _worldView != null ? _worldView.SelectedLastRunLootBreakdownText : "None";
    public string SelectedLastRunDungeonLabel => _worldView != null ? _worldView.SelectedLastRunDungeonText : "None";
    public string SelectedLastRunRouteLabel => _worldView != null ? _worldView.SelectedLastRunRouteText : "None";
    public string SelectedSurplusLabel => _worldView != null ? _worldView.SelectedSurplusText : "None";
    public string SelectedDeficitLabel => _worldView != null ? _worldView.SelectedDeficitText : "None";
    public string SelectedIdentityLabel => _worldView != null ? _worldView.SelectedIdentityText : "None";
    public string SelectedReserveStockRuleLabel => _worldView != null ? _worldView.SelectedReserveStockRuleText : "None";
    public string SelectedProcessingPreferenceLabel => _worldView != null ? _worldView.SelectedProcessingPreferenceText : "None";
    public string SelectedStocksLabel => _worldView != null ? _worldView.SelectedStocksText : "None";
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
    public bool IsDungeonRunHudMode => _gameState != null && _gameState.CurrentState == GameStateId.DungeonRun;
    public bool IsDungeonBattleViewActive => _worldView != null && _worldView.IsBattleViewActive;
    public bool IsDungeonRouteChoiceVisible => _worldView != null && _worldView.IsDungeonRouteChoiceVisible;
    public bool IsDungeonEventChoiceVisible => _worldView != null && _worldView.IsDungeonEventChoiceVisible;
    public bool IsDungeonPreEliteChoiceVisible => _worldView != null && _worldView.IsDungeonPreEliteChoiceVisible;
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
    public string EventChoiceLabel => _worldView != null ? _worldView.EventChoiceText : "None";
    public string PreEliteChoiceRunLabel => _worldView != null ? _worldView.PreEliteChoiceText : "Pending";
    public string LootBreakdownLabel => _worldView != null ? _worldView.LootBreakdownText : "None";
    public string BattleLootBreakdownLabel => _worldView != null ? _worldView.BattleLootText : "None";
    public string ChestLootBreakdownLabel => _worldView != null ? _worldView.ChestLootText : "None";
    public string EventLootBreakdownLabel => _worldView != null ? _worldView.EventLootText : "None";
    public string RouteChoiceTitleLabel => _worldView != null ? _worldView.RouteChoiceTitleText : "None";
    public string RouteChoiceDescriptionLabel => _worldView != null ? _worldView.RouteChoiceDescriptionText : "None";
    public string RouteChoicePromptLabel => _worldView != null ? _worldView.RouteChoicePromptText : "None";
    public string RouteOption1Label => _worldView != null ? _worldView.RouteOption1Text : "None";
    public string RouteOption2Label => _worldView != null ? _worldView.RouteOption2Text : "None";
    public string EventTitleLabel => _worldView != null ? _worldView.EventTitleText : "None";
    public string EventDescriptionLabel => _worldView != null ? _worldView.EventDescriptionText : "None";
    public string EventPromptLabel => _worldView != null ? _worldView.EventPromptText : "None";
    public string EventOptionALabel => _worldView != null ? _worldView.EventOptionAText : "None";
    public string EventOptionBLabel => _worldView != null ? _worldView.EventOptionBText : "None";
    public string PreEliteTitleLabel => _worldView != null ? _worldView.PreEliteTitleText : "None";
    public string PreEliteDescriptionLabel => _worldView != null ? _worldView.PreEliteDescriptionText : "None";
    public string PreElitePromptLabel => _worldView != null ? _worldView.PreElitePromptText : "None";
    public string PreEliteOptionALabel => _worldView != null ? _worldView.PreEliteOptionAText : "None";
    public string PreEliteOptionBLabel => _worldView != null ? _worldView.PreEliteOptionBText : "None";
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
    public string ResultPanelDungeonChosenLabel => _worldView != null ? _worldView.ResultPanelDungeonChosenText : "None";
    public string ResultPanelDungeonDangerLabel => _worldView != null ? _worldView.ResultPanelDungeonDangerText : "None";
    public string ResultPanelRouteChosenLabel => _worldView != null ? _worldView.ResultPanelRouteChosenText : "None";
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
    public string ResultPanelEventChoiceLabel => _worldView != null ? _worldView.ResultPanelEventChoiceText : "None";
    public string ResultPanelSurvivingMembersLabel => _worldView != null ? _worldView.ResultPanelSurvivingMembersText : "None";
    public string ResultPanelClearedEncountersLabel => _worldView != null ? _worldView.ResultPanelClearedEncountersText : "0 / 3";
    public string ResultPanelOpenedChestsLabel => _worldView != null ? _worldView.ResultPanelOpenedChestsText : "0 / 1";
    public string ResultPanelEliteDefeatedLabel => _worldView != null ? _worldView.ResultPanelEliteDefeatedText : "None";
    public string ResultPanelEliteNameLabel => _worldView != null ? _worldView.ResultPanelEliteNameText : "None";
    public string ResultPanelEliteRewardIdentityLabel => _worldView != null ? _worldView.ResultPanelEliteRewardIdentityText : "None";
    public string ResultPanelEliteRewardAmountLabel => _worldView != null ? _worldView.ResultPanelEliteRewardAmountText : "None";
    public string ResultPanelEliteRewardLabel => _worldView != null ? _worldView.ResultPanelEliteRewardText : "None";
    public string ResultPanelPreEliteChoiceLabel => _worldView != null ? _worldView.ResultPanelPreEliteChoiceText : "Pending";
    public string ResultPanelPreEliteHealAmountLabel => _worldView != null ? _worldView.ResultPanelPreEliteHealAmountText : "None";
    public string ResultPanelEliteBonusRewardEarnedLabel => _worldView != null ? _worldView.ResultPanelEliteBonusRewardEarnedText : "None";
    public string ResultPanelEliteBonusRewardAmountLabel => _worldView != null ? _worldView.ResultPanelEliteBonusRewardAmountText : "None";
    public string ResultPanelRoomPathSummaryLabel => _worldView != null ? _worldView.ResultPanelRoomPathSummaryText : "None";
    public string ResultPanelPartyHpSummaryLabel => _worldView != null ? _worldView.ResultPanelPartyHpSummaryText : "None";
    public string ResultPanelPartyConditionLabel => _worldView != null ? _worldView.ResultPanelPartyConditionText : "None";
    public string ResultPanelReturnPromptLabel => _worldView != null ? _worldView.ResultPanelReturnPromptText : "None";
    public string Party1HpLabel => _worldView != null ? _worldView.GetPartyMemberHpText(0) : "None";
    public string Party2HpLabel => _worldView != null ? _worldView.GetPartyMemberHpText(1) : "None";
    public string Party3HpLabel => _worldView != null ? _worldView.GetPartyMemberHpText(2) : "None";
    public string Party4HpLabel => _worldView != null ? _worldView.GetPartyMemberHpText(3) : "None";

    private void Awake()
    {
        _gameState = new GameState(GameStateId.Boot);
        _resources = PlaceholderResourceDataFactory.Create();
        _worldView = new StaticPlaceholderWorldView(_resources);
        EnsureDebugHUD();
        EnsureWorldCameraController();
        Debug.Log("[Boot] Boot scene started successfully.");
        ApplyBackgroundColor(GameStateId.Boot);
        UpdateWorldVisibility(GameStateId.Boot);
        UpdateCameraControl(GameStateId.Boot);
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
        if (_gameState == null)
        {
            return;
        }

        if (!_hasTransitioned && Time.unscaledTime >= MainMenuDelaySeconds)
        {
            ChangeState(GameStateId.MainMenu);
            _hasTransitioned = true;
            return;
        }

        Keyboard keyboard = Keyboard.current;
        bool blockKeyboardShortcuts = IsSearchFieldFocused();
        bool blockDungeonInput = blockKeyboardShortcuts || IsDungeonHudInputBlocked();
        if (keyboard != null)
        {
            if (!blockKeyboardShortcuts && keyboard.lKey.wasPressedThisFrame)
            {
                ToggleLanguage();
            }

            if (!blockKeyboardShortcuts && _gameState.CurrentState == GameStateId.MainMenu &&
                (keyboard.enterKey.wasPressedThisFrame ||
                 keyboard.numpadEnterKey.wasPressedThisFrame ||
                 keyboard.spaceKey.wasPressedThisFrame))
            {
                ChangeState(GameStateId.WorldSim);
                return;
            }

            if (!blockKeyboardShortcuts && _gameState.CurrentState == GameStateId.WorldSim && keyboard.escapeKey.wasPressedThisFrame)
            {
                ChangeState(GameStateId.MainMenu);
                return;
            }

            if (!blockKeyboardShortcuts && _gameState.CurrentState == GameStateId.WorldSim)
            {
                HandleWorldSimEconomyInput(keyboard);
            }
        }

        if (_gameState.CurrentState == GameStateId.WorldSim)
        {
            if (_worldView != null)
            {
                _worldView.UpdateAutoTick(Time.deltaTime);
            }

            HandleSelectionInput();
            return;
        }

        if (_gameState.CurrentState == GameStateId.DungeonRun && _worldView != null)
        {
            Keyboard dungeonKeyboard = blockDungeonInput ? null : keyboard;
            _worldView.UpdateDungeonRun(dungeonKeyboard, Mouse.current, Camera.main);
            if (_worldView.ConsumeDungeonRunExitRequest())
            {
                ChangeState(GameStateId.WorldSim);
            }
        }
    }

    public string GetText(string key)
    {
        return PrototypeLocalization.Get(_currentLanguage, key);
    }

    public string TranslateValue(string text)
    {
        return PrototypeLocalization.TranslateValue(_currentLanguage, text);
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

    public bool IsRouteChoiceAvailable(string optionKey)
    {
        return _worldView != null && _worldView.IsRouteChoiceAvailable(optionKey);
    }

    public bool IsRouteChoiceHovered(string optionKey)
    {
        return _worldView != null && _worldView.IsRouteChoiceHovered(optionKey);
    }

    public bool IsRouteChoiceSelected(string optionKey)
    {
        return _worldView != null && _worldView.IsRouteChoiceSelected(optionKey);
    }

    public void SetRouteChoiceHover(string optionKey)
    {
        if (_worldView != null)
        {
            _worldView.SetRouteChoiceHover(optionKey);
        }
    }

    public bool CanConfirmRouteChoice()
    {
        return _worldView != null && _worldView.CanConfirmRouteChoice();
    }

    public bool TryConfirmRouteChoice()
    {
        return _worldView != null && _worldView.TryConfirmRouteChoice();
    }

    public bool TryTriggerRouteChoice(string optionKey)
    {
        return _worldView != null && _worldView.TryTriggerRouteChoice(optionKey);
    }

    public bool TryCycleCurrentDispatchPolicy()
    {
        return _worldView != null && _worldView.TryCycleCurrentDispatchPolicy();
    }

    public bool IsEventChoiceAvailable(string optionKey)
    {
        return _worldView != null && _worldView.IsEventChoiceAvailable(optionKey);
    }
    public bool IsEventChoiceHovered(string optionKey)
    {
        return _worldView != null && _worldView.IsEventChoiceHovered(optionKey);
    }

    public bool IsEventChoiceSelected(string optionKey)
    {
        return _worldView != null && _worldView.IsEventChoiceSelected(optionKey);
    }

    public void SetEventChoiceHover(string optionKey)
    {
        if (_worldView != null)
        {
            _worldView.SetEventChoiceHover(optionKey);
        }
    }

    public bool TryTriggerEventChoice(string optionKey)
    {
        return _worldView != null && _worldView.TryTriggerEventChoice(optionKey);
    }

    public bool IsPreEliteChoiceAvailable(string optionKey)
    {
        return _worldView != null && _worldView.IsPreEliteChoiceAvailable(optionKey);
    }

    public bool IsPreEliteChoiceHovered(string optionKey)
    {
        return _worldView != null && _worldView.IsPreEliteChoiceHovered(optionKey);
    }

    public bool IsPreEliteChoiceSelected(string optionKey)
    {
        return _worldView != null && _worldView.IsPreEliteChoiceSelected(optionKey);
    }

    public void SetPreEliteChoiceHover(string optionKey)
    {
        if (_worldView != null)
        {
            _worldView.SetPreEliteChoiceHover(optionKey);
        }
    }

    public bool TryTriggerPreEliteChoice(string optionKey)
    {
        return _worldView != null && _worldView.TryTriggerPreEliteChoice(optionKey);
    }
    private void ChangeState(GameStateId nextState)
    {
        if (_gameState == null || !_gameState.ChangeState(nextState))
        {
            return;
        }

        Debug.Log("[GameState] " + _gameState.LastTransition);
        ApplyBackgroundColor(nextState);
        UpdateCameraControl(nextState);
        UpdateWorldVisibility(nextState);
    }

    private void UpdateWorldVisibility(GameStateId state)
    {
        if (_worldView == null)
        {
            return;
        }

        _worldView.SetWorldSimVisible(state == GameStateId.WorldSim);
        _worldView.SetDungeonRunVisible(state == GameStateId.DungeonRun);
    }

    private void UpdateCameraControl(GameStateId state)
    {
        EnsureWorldCameraController();
        if (_worldCameraController != null)
        {
            _worldCameraController.SetWorldSimActive(state == GameStateId.WorldSim);
        }

        if (state == GameStateId.DungeonRun && _worldView != null)
        {
            _worldView.ConfigureDungeonCamera(Camera.main);
        }
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
            _worldView.RecruitSelectedCityParty();
        }


        if (keyboard.xKey.wasPressedThisFrame && _worldView.TryEnterSelectedCityDungeon(Camera.main))
        {
            ChangeState(GameStateId.DungeonRun);
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

        _worldView.SelectAtScreenPosition(mainCamera, mouse.position.ReadValue());
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

    private void EnsureDebugHUD()
    {
        _debugHud = GetComponent<PrototypeDebugHUD>();
        if (_debugHud == null)
        {
            _debugHud = gameObject.AddComponent<PrototypeDebugHUD>();
        }
    }

    private bool IsDungeonHudInputBlocked()
    {
        EnsureDebugHUD();
        return _debugHud != null && _debugHud.ShouldBlockDungeonInput;
    }

    private bool IsSearchFieldFocused()
    {
        EnsureDebugHUD();
        return _debugHud != null && _debugHud.IsSearchFieldFocused;
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

    private void ApplyBackgroundColor(GameStateId state)
    {
        Camera mainCamera = Camera.main;
        if (mainCamera == null)
        {
            return;
        }

        mainCamera.clearFlags = CameraClearFlags.SolidColor;
        mainCamera.backgroundColor = state == GameStateId.WorldSim
            ? WorldSimColor
            : state == GameStateId.DungeonRun
                ? DungeonRunColor
                : state == GameStateId.MainMenu
                    ? MainMenuColor
                    : BootColor;
    }
}













