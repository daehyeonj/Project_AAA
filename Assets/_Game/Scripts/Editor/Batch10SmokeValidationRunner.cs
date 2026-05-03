using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

public static class Batch10SmokeValidationRunner
{
    private const string ScenePath = "Assets/Scenes/SampleScene.unity";
    private const double StepTimeoutSeconds = 12d;
    private const string SessionActiveKey = "Batch10SmokeValidationRunner.Active";
    private static SmokeSession _session;

    [InitializeOnLoadMethod]
    private static void RestorePendingSession()
    {
        if (!SessionState.GetBool(SessionActiveKey, false) || _session != null)
        {
            return;
        }

        _session = new SmokeSession();
        _session.RestoreAfterDomainReload();
    }

    public static void RunBatch10SmokeValidation()
    {
        if (_session != null)
        {
            Debug.LogError("[Batch10Smoke] Smoke runner is already active.");
            EditorApplication.Exit(1);
            return;
        }

        _session = new SmokeSession();
        _session.Start();
    }

    private sealed class SmokeSession
    {
        private enum SmokeStep
        {
            OpenScene,
            EnterPlayMode,
            WaitForBoot,
            WaitForMainMenu,
            EnterWorldSim,
            WaitForWorldSim,
            SelectCity,
            WaitForCityHub,
            RecruitParty,
            EnterExpeditionPrep,
            WaitForExpeditionPrep,
            SelectRoute,
            ConfirmLaunch,
            WaitForDungeonRun,
            ResolveCoreLoop,
            ReturnToWorld,
            ValidateWorldRefresh,
            ValidateCityHubDecision,
            ValidateCityHubDecisionRelevance,
            ValidateWorldReturnChainSummary,
            ReenterExpeditionPrepAfterWorldReturn,
            ValidatePrepReentryContinuity,
            Shutdown
        }

        private readonly List<CheckpointResult> _checkpoints = new List<CheckpointResult>();
        private readonly StringBuilder _summary = new StringBuilder();
        private SmokeStep _step = SmokeStep.OpenScene;
        private double _stepStartedAt;
        private BootEntry _boot;
        private string _selectedCityId = string.Empty;
        private string _selectedRouteId = "safe";
        private bool _sawBattleStage;
        private bool _loggedBattleEntry;
        private bool _loggedBattleReturn;
        private bool _loggedResultPackaging;
        private bool _loggedRepresentativeContentCatalog;
        private string _battleEntryEncounterId = string.Empty;
        private string _battleEntryRoomType = string.Empty;
        private bool _shutdownRequested;
        private string _lastBattleSnapshot = string.Empty;
        private int _battleActionAttempts;
        private int _battleTargetAttempts;
        private int _battleEnemyAttempts;

        public void Start()
        {
            SessionState.SetBool(SessionActiveKey, true);
            Debug.Log("[Batch10Smoke] Opening sample scene.");
            EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
            EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
            EditorApplication.update += Tick;
            AdvanceTo(SmokeStep.EnterPlayMode, "Scene opened.");
            EditorApplication.isPlaying = true;
        }

        public void RestoreAfterDomainReload()
        {
            EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
            EditorApplication.update += Tick;
            AdvanceTo(SmokeStep.WaitForBoot, "Restored smoke session after domain reload.");
        }

        private void Tick()
        {
            if (_shutdownRequested)
            {
                return;
            }

            if (EditorApplication.timeSinceStartup - _stepStartedAt > StepTimeoutSeconds)
            {
                Fail("Timed out while waiting for step " + _step + ".");
                return;
            }

            try
            {
                switch (_step)
                {
                    case SmokeStep.EnterPlayMode:
                        break;
                    case SmokeStep.WaitForBoot:
                        WaitForBoot();
                        break;
                    case SmokeStep.WaitForMainMenu:
                        WaitForMainMenu();
                        break;
                    case SmokeStep.EnterWorldSim:
                        EnterWorldSim();
                        break;
                    case SmokeStep.WaitForWorldSim:
                        WaitForWorldSim();
                        break;
                    case SmokeStep.SelectCity:
                        SelectCity();
                        break;
                    case SmokeStep.WaitForCityHub:
                        WaitForCityHub();
                        break;
                    case SmokeStep.RecruitParty:
                        RecruitParty();
                        break;
                    case SmokeStep.EnterExpeditionPrep:
                        EnterExpeditionPrep();
                        break;
                    case SmokeStep.WaitForExpeditionPrep:
                        WaitForExpeditionPrep();
                        break;
                    case SmokeStep.SelectRoute:
                        SelectRoute();
                        break;
                    case SmokeStep.ConfirmLaunch:
                        ConfirmLaunch();
                        break;
                    case SmokeStep.WaitForDungeonRun:
                        WaitForDungeonRun();
                        break;
                    case SmokeStep.ResolveCoreLoop:
                        ResolveCoreLoop();
                        break;
                    case SmokeStep.ReturnToWorld:
                        ReturnToWorld();
                        break;
                    case SmokeStep.ValidateWorldRefresh:
                        ValidateWorldRefresh();
                        break;
                    case SmokeStep.ValidateCityHubDecision:
                        ValidateCityHubDecision();
                        break;
                    case SmokeStep.ValidateCityHubDecisionRelevance:
                        ValidateCityHubDecisionRelevance();
                        break;
                    case SmokeStep.ValidateWorldReturnChainSummary:
                        ValidateWorldReturnChainSummary();
                        break;
                    case SmokeStep.ReenterExpeditionPrepAfterWorldReturn:
                        ReenterExpeditionPrepAfterWorldReturn();
                        break;
                    case SmokeStep.ValidatePrepReentryContinuity:
                        ValidatePrepReentryContinuity();
                        break;
                    case SmokeStep.Shutdown:
                        Shutdown(0);
                        break;
                }
            }
            catch (Exception ex)
            {
                Fail("Unhandled exception during step " + _step + ": " + ex);
            }
        }

        private void WaitForBoot()
        {
            _boot = UnityEngine.Object.FindFirstObjectByType<BootEntry>();
            if (_boot == null)
            {
                return;
            }

            Debug.Log("[Batch10Smoke] BootEntry located.");
            AdvanceTo(SmokeStep.WaitForMainMenu, "BootEntry is active.");
        }

        private void WaitForMainMenu()
        {
            if (!_boot.IsMainMenuActive)
            {
                return;
            }

            RecordPass(
                "Boot -> MainMenu",
                "Reached MainMenu. Stage=" + _boot.CurrentAppFlowStage + " StateLabel=" + _boot.CurrentStateLabel + ".");
            AdvanceTo(SmokeStep.EnterWorldSim, "MainMenu reached.");
        }

        private void EnterWorldSim()
        {
            _boot.EnterWorldSimFromMenu();
            AdvanceTo(SmokeStep.WaitForWorldSim, "Requested WorldSim entry.");
        }

        private void WaitForWorldSim()
        {
            if (!_boot.IsWorldSimActive || _boot.CurrentAppFlowStage != AppFlowStage.WorldSim)
            {
                return;
            }

            RecordPass(
                "MainMenu -> WorldSim",
                "Entered WorldSim. Day=" + _boot.WorldDayCount + " IdleParties=" + _boot.IdleParties + ".");
            AdvanceTo(SmokeStep.SelectCity, "WorldSim active.");
        }

        private void SelectCity()
        {
            WorldSelectableMarker cityMarker = FindFirstCityMarker();
            if (cityMarker == null)
            {
                Fail("Could not find a city marker to select.");
                return;
            }

            Camera mainCamera = Camera.main;
            object worldView = GetWorldView();
            if (mainCamera == null || worldView == null)
            {
                Fail("WorldSim selection prerequisites are missing. Camera or world view was null.");
                return;
            }

            Vector3 screenPoint = mainCamera.WorldToScreenPoint(cityMarker.transform.position);
            InvokePublicMethod(worldView, "SelectAtScreenPosition", mainCamera, (Vector2)screenPoint);
            _selectedCityId = cityMarker.EntityData != null ? cityMarker.EntityData.Id : string.Empty;
            Debug.Log("[Batch10Smoke] Selected city " + SafeText(_selectedCityId) + ".");
            AdvanceTo(SmokeStep.WaitForCityHub, "City selection applied.");
        }

        private void WaitForCityHub()
        {
            if (_boot.CurrentAppFlowStage != AppFlowStage.CityHub)
            {
                return;
            }

            RecordPass(
                "WorldSim -> CityHub",
                "City selection promoted to CityHub. City=" + SafeText(_boot.SelectedIdLabel) + " RecommendedRoute=" + SafeText(_boot.SelectedRecommendedRouteSummaryLabel) + ".");
            AdvanceTo(SmokeStep.RecruitParty, "CityHub active.");
        }

        private void RecruitParty()
        {
            _boot.RecruitWorldSimParty();
            if (_boot.IdleParties <= 0)
            {
                Fail("Party recruitment did not create an idle party for city " + SafeText(_selectedCityId) + ".");
                return;
            }

            Debug.Log("[Batch10Smoke] Recruited party. IdleParties=" + _boot.IdleParties + ".");
            AdvanceTo(SmokeStep.EnterExpeditionPrep, "Party recruited.");
        }

        private void EnterExpeditionPrep()
        {
            if (!_boot.TryEnterSelectedWorldDungeon())
            {
                Fail("Failed to enter ExpeditionPrep from selected city " + SafeText(_selectedCityId) + ".");
                return;
            }

            AdvanceTo(SmokeStep.WaitForExpeditionPrep, "Requested ExpeditionPrep.");
        }

        private void WaitForExpeditionPrep()
        {
            if (_boot.CurrentAppFlowStage != AppFlowStage.ExpeditionPrep || !_boot.IsDungeonRouteChoiceVisible)
            {
                return;
            }

            if (!ValidateRepresentativeContentCatalog())
            {
                return;
            }

            RecordPass(
                "CityHub -> ExpeditionPrep",
                "Expedition prep is visible. Objective=" + SafeText(_boot.RouteChoiceTitleLabel) + " Prompt=" + SafeText(_boot.RouteChoicePromptLabel) + ".");
            AdvanceTo(SmokeStep.SelectRoute, "ExpeditionPrep active.");
        }

        private void SelectRoute()
        {
            if (!_boot.IsRouteChoiceAvailable(_selectedRouteId))
            {
                Fail("Route '" + _selectedRouteId + "' was not available. Prompt=" + SafeText(_boot.RouteChoicePromptLabel) + ".");
                return;
            }

            if (!_boot.TryTriggerRouteChoice(_selectedRouteId))
            {
                Fail("Failed to select route '" + _selectedRouteId + "'.");
                return;
            }

            Debug.Log("[Batch10Smoke] Selected route " + _selectedRouteId + ".");
            AdvanceTo(SmokeStep.ConfirmLaunch, "Route selected.");
        }

        private void ConfirmLaunch()
        {
            if (!_boot.CanConfirmRouteChoice())
            {
                Fail("Launch was blocked after route selection. Prompt=" + SafeText(_boot.RouteChoicePromptLabel) + ".");
                return;
            }

            if (!_boot.TryConfirmRouteChoice())
            {
                Fail("TryConfirmRouteChoice returned false for route '" + _selectedRouteId + "'.");
                return;
            }

            AdvanceTo(SmokeStep.WaitForDungeonRun, "Launch confirmed.");
        }

        private void WaitForDungeonRun()
        {
            if (_boot.CurrentAppFlowStage != AppFlowStage.DungeonRun || _boot.IsDungeonRouteChoiceVisible)
            {
                return;
            }

            AppFlowContext context = _boot.CurrentAppFlowContext;
            AppFlowDungeonRunContext runContext = context != null ? context.CurrentDungeonRun : null;
            ExpeditionPlan launchPlan = runContext != null ? runContext.LaunchPlan : null;
            ExpeditionRunState runState = runContext != null ? runContext.RunState : null;
            string launchCity = launchPlan != null ? launchPlan.OriginCityId : string.Empty;
            string launchDungeon = launchPlan != null ? launchPlan.TargetDungeonId : string.Empty;
            string launchRouteId = launchPlan != null && launchPlan.SelectedRoute != null ? launchPlan.SelectedRoute.RouteId : string.Empty;
            string stateRouteId = runState != null ? runState.RouteId : string.Empty;
            string launchObjective = launchPlan != null ? launchPlan.ObjectiveText : string.Empty;
            string stateObjective = runState != null ? runState.ObjectiveText : string.Empty;
            string launchWhyNow = launchPlan != null ? launchPlan.WhyNowText : string.Empty;
            string stateWhyNow = runState != null ? runState.WhyNowText : string.Empty;
            string launchUsefulness = launchPlan != null ? launchPlan.ExpectedUsefulnessText : string.Empty;
            string stateUsefulness = runState != null ? runState.ExpectedUsefulnessText : string.Empty;
            string launchRiskPreview = launchPlan != null ? launchPlan.RiskRewardPreviewText : string.Empty;
            string stateRiskPreview = runState != null ? runState.RiskRewardPreviewText : string.Empty;
            object worldView = GetWorldView();
            string contentSource = worldView != null
                ? SafeText(InvokeNonPublicMethod(worldView, "GetDungeonContentSourceLabel", launchCity, launchDungeon, stateRouteId) as string)
                : "None";
            bool routeMatches = HasMeaningfulValue(_selectedRouteId) && launchRouteId == _selectedRouteId && stateRouteId == _selectedRouteId;
            bool objectiveMatches = HasMatchingContractText(launchObjective, stateObjective);
            bool whyNowMatches = HasMatchingContractText(launchWhyNow, stateWhyNow);
            bool usefulnessMatches = HasMatchingContractText(launchUsefulness, stateUsefulness);
            bool riskPreviewMatches = HasMatchingContractText(launchRiskPreview, stateRiskPreview);
            bool representativeChainUsesData = !IsRepresentativeContentChain(launchCity, launchDungeon, stateRouteId) ||
                                               contentSource.StartsWith("data:", StringComparison.Ordinal);

            if (launchPlan == null ||
                !launchPlan.IsConfirmed ||
                !routeMatches ||
                !objectiveMatches ||
                !whyNowMatches ||
                !usefulnessMatches ||
                !riskPreviewMatches ||
                !representativeChainUsesData)
            {
                RecordFail(
                    "ExpeditionPrep -> DungeonRun launch",
                    "Dungeon run did not preserve the confirmed launch contract. Stage=" + _boot.CurrentAppFlowStage +
                    " SelectedRoute=" + SafeText(_selectedRouteId) +
                    " LaunchConfirmed=" + (launchPlan != null && launchPlan.IsConfirmed) +
                    " LaunchCity=" + SafeText(launchCity) +
                    " LaunchDungeon=" + SafeText(launchDungeon) +
                    " LaunchRoute=" + SafeText(launchRouteId) +
                    " RunRoute=" + SafeText(stateRouteId) +
                    " LaunchObjective=" + SafeText(launchObjective) +
                    " RunObjective=" + SafeText(stateObjective) +
                    " LaunchWhyNow=" + SafeText(launchWhyNow) +
                    " RunWhyNow=" + SafeText(stateWhyNow) +
                    " LaunchUsefulness=" + SafeText(launchUsefulness) +
                    " RunUsefulness=" + SafeText(stateUsefulness) +
                    " LaunchRiskPreview=" + SafeText(launchRiskPreview) +
                    " RunRiskPreview=" + SafeText(stateRiskPreview) +
                    " ContentSource=" + contentSource + ".");
                AdvanceTo(SmokeStep.Shutdown, "Checkpoint summary recorded.");
                return;
            }

            RecordPass(
                "ExpeditionPrep -> DungeonRun launch",
                "Dungeon run preserved the confirmed launch contract. LaunchPlan.City=" + SafeText(launchCity) +
                " LaunchPlan.Dungeon=" + SafeText(launchDungeon) +
                " Route=" + SafeText(stateRouteId) +
                " Objective=" + SafeText(stateObjective) +
                " RiskPreview=" + SafeText(stateRiskPreview) +
                " ContentSource=" + contentSource +
                " RunLabel=" + SafeText(_boot.CurrentDungeonRunLabel) + ".");
            AdvanceTo(SmokeStep.ResolveCoreLoop, "DungeonRun active.");
        }

        private void ResolveCoreLoop()
        {
            TrackBattleCheckpoint();

            if (_boot.IsDungeonResultPanelVisible || _boot.CurrentAppFlowStage == AppFlowStage.ResultPipeline)
            {
                AppFlowContext context = _boot.CurrentAppFlowContext;
                AppFlowResultContext latestResult = context != null ? context.LatestResult : null;
                TrackResultPipelineCheckpoint(latestResult);
                if (_step == SmokeStep.Shutdown)
                {
                    return;
                }

                bool resultApplied = latestResult != null && latestResult.IsAppliedToWorld;
                string marker = latestResult != null ? latestResult.AppliedWorldStateMarker : "None";
                string summary = latestResult != null && latestResult.OutcomeReadback != null
                    ? latestResult.OutcomeReadback.SummaryText
                    : "None";

                if (!resultApplied)
                {
                    Fail("ResultPipeline checkpoint failed before world return. Applied=" + resultApplied + " Marker=" + SafeText(marker) + " Summary=" + SafeText(summary) + ".");
                    return;
                }

                Debug.Log("[Batch10Smoke] Result panel reached. AppliedMarker=" + SafeText(marker) + " Summary=" + SafeText(summary) + ".");
                AdvanceTo(SmokeStep.ReturnToWorld, "Run resolved.");
                return;
            }

            if (_boot.CurrentAppFlowStage == AppFlowStage.BattleScene)
            {
                ResolveBattleStep();
                return;
            }

            PrototypeDungeonRunShellSurfaceData shellSurface = _boot.GetDungeonRunShellSurfaceData();
            bool isEventChoiceVisible = _boot.IsDungeonEventChoiceVisible ||
                                        (shellSurface != null && shellSurface.IsEventChoiceVisible);
            bool isPreEliteChoiceVisible = _boot.IsDungeonPreEliteChoiceVisible ||
                                           (shellSurface != null && shellSurface.IsPreEliteChoiceVisible);

            if (isEventChoiceVisible)
            {
                if (!_boot.TryTriggerEventChoice("recover"))
                {
                    Fail("Failed to resolve event choice with 'recover'.");
                }

                return;
            }

            if (isPreEliteChoiceVisible)
            {
                if (!_boot.TryTriggerPreEliteChoice("recover"))
                {
                    Fail("Failed to resolve pre-elite choice with 'recover'.");
                }

                return;
            }

            ResolveExploreStep();
        }

        private void ReturnToWorld()
        {
            object worldView = GetWorldView();
            if (worldView == null)
            {
                Fail("World return prerequisites were missing.");
                return;
            }

            SetPrivateField(worldView, "_pendingDungeonExit", true);
            AppFlowObservedSnapshot snapshot = (AppFlowObservedSnapshot)InvokePublicMethod(worldView, "BuildAppFlowSnapshot");
            bool returned = (bool)InvokeNonPublicMethod(_boot, "TryExitDungeonRunToWorldSim");
            if (!snapshot.HasPendingWorldReturn || !returned)
            {
                Fail("Result return request was not consumed cleanly. SnapshotPending=" + snapshot.HasPendingWorldReturn + " Returned=" + returned + ".");
                return;
            }

            bool returnedToWorldShell = _boot.IsWorldSimActive &&
                                        (_boot.CurrentAppFlowStage == AppFlowStage.WorldSim ||
                                         _boot.CurrentAppFlowStage == AppFlowStage.CityHub);
            if (!returnedToWorldShell)
            {
                Fail("BootEntry did not return to WorldSim. State=" + _boot.CurrentStateLabel + " Stage=" + _boot.CurrentAppFlowStage + ".");
                return;
            }

            InvokeNonPublicMethod(_boot, "ApplyCurrentAppFlowPresentation", true);
            AdvanceTo(SmokeStep.ValidateWorldRefresh, "Returned to WorldSim.");
        }

        private void ValidateWorldRefresh()
        {
            WorldBoardReadModel board = _boot.GetWorldBoardReadModel();
            CityStatusReadModel city = FindCity(board, _selectedCityId);
            ExpeditionResultReadModel latestResult = city != null ? city.LatestResult : null;
            AppFlowResultContext appFlowResult = _boot.CurrentAppFlowContext != null ? _boot.CurrentAppFlowContext.LatestResult : null;

            bool hasCityReadback = latestResult != null && latestResult.HasResult;
            bool hasImpactText = city != null && HasMeaningfulValue(city.LastDispatchImpactText);
            bool hasStatusChange = latestResult != null && HasMeaningfulValue(latestResult.CityStatusChangeSummaryText);
            string contextObjective = appFlowResult != null && appFlowResult.OutcomeReadback != null && HasMeaningfulValue(appFlowResult.OutcomeReadback.MissionObjectiveText)
                ? appFlowResult.OutcomeReadback.MissionObjectiveText
                : appFlowResult != null && appFlowResult.ExpeditionOutcome != null
                    ? appFlowResult.ExpeditionOutcome.MissionObjectiveText
                    : string.Empty;
            string contextRelevance = appFlowResult != null && appFlowResult.OutcomeReadback != null && HasMeaningfulValue(appFlowResult.OutcomeReadback.MissionRelevanceText)
                ? appFlowResult.OutcomeReadback.MissionRelevanceText
                : appFlowResult != null && appFlowResult.ExpeditionOutcome != null
                    ? appFlowResult.ExpeditionOutcome.MissionRelevanceText
                    : string.Empty;
            string contextOutcomeMeaningId = appFlowResult != null && appFlowResult.OutcomeReadback != null && HasMeaningfulValue(appFlowResult.OutcomeReadback.OutcomeMeaningId)
                ? appFlowResult.OutcomeReadback.OutcomeMeaningId
                : appFlowResult != null && appFlowResult.ExpeditionOutcome != null
                    ? appFlowResult.ExpeditionOutcome.OutcomeMeaningId
                    : string.Empty;
            string contextCityImpactMeaning = appFlowResult != null && appFlowResult.OutcomeReadback != null && HasMeaningfulValue(appFlowResult.OutcomeReadback.CityImpactMeaningText)
                ? appFlowResult.OutcomeReadback.CityImpactMeaningText
                : appFlowResult != null && appFlowResult.ExpeditionOutcome != null
                    ? appFlowResult.ExpeditionOutcome.CityImpactMeaningText
                    : string.Empty;
            string contextRecommendationShift = appFlowResult != null && appFlowResult.OutcomeReadback != null && HasMeaningfulValue(appFlowResult.OutcomeReadback.RecommendationShiftText)
                ? appFlowResult.OutcomeReadback.RecommendationShiftText
                : appFlowResult != null && appFlowResult.ExpeditionOutcome != null
                    ? appFlowResult.ExpeditionOutcome.RecommendationShiftText
                    : string.Empty;
            string outcomeMeaningId = latestResult != null ? latestResult.OutcomeMeaningId : string.Empty;
            string cityImpactMeaning = latestResult != null ? latestResult.CityImpactMeaningText : string.Empty;
            string recommendationShift = latestResult != null ? latestResult.RecommendationShiftText : string.Empty;
            bool hasMissionCarry = latestResult != null &&
                                   HasMatchingContractText(latestResult.MissionObjectiveText, contextObjective) &&
                                   HasMatchingContractText(latestResult.MissionRelevanceText, contextRelevance);
            bool hasOutcomeMeaningCarry = latestResult != null &&
                                          HasMatchingContractText(outcomeMeaningId, contextOutcomeMeaningId) &&
                                          HasMatchingContractText(cityImpactMeaning, contextCityImpactMeaning) &&
                                          HasMatchingContractText(recommendationShift, contextRecommendationShift);
            bool hasWorldReturnSummary = latestResult != null &&
                                         HasMeaningfulValue(latestResult.WorldReturnSummaryText) &&
                                         ContainsValue(latestResult.WorldReturnSummaryText, latestResult.CityStatusChangeSummaryText) &&
                                         (ContainsValue(latestResult.WorldReturnSummaryText, cityImpactMeaning) ||
                                          (!HasMeaningfulValue(cityImpactMeaning) &&
                                           (ContainsValue(latestResult.WorldReturnSummaryText, contextRelevance) ||
                                            (!HasMeaningfulValue(contextRelevance) && ContainsValue(latestResult.WorldReturnSummaryText, contextObjective)))));
            string selectedResult = _boot.SelectedLastExpeditionResultLabel;
            string boardSourceCity = latestResult != null ? latestResult.SourceCityId : string.Empty;
            string appFlowSourceCity = appFlowResult != null && appFlowResult.OutcomeReadback != null
                ? appFlowResult.OutcomeReadback.SourceCityId
                : string.Empty;
            string appliedMarker = appFlowResult != null ? appFlowResult.AppliedWorldStateMarker : string.Empty;

            if (!hasCityReadback || !hasImpactText || !hasStatusChange || !hasMissionCarry || !hasOutcomeMeaningCarry || !hasWorldReturnSummary)
            {
                RecordFail(
                    "ResultPipeline -> WorldSim board refresh",
                    "Result did not fully round-trip. Stage=" + _boot.CurrentAppFlowStage +
                    " SelectedCity=" + SafeText(_selectedCityId) +
                    " BoardSourceCity=" + SafeText(boardSourceCity) +
                    " AppFlowSourceCity=" + SafeText(appFlowSourceCity) +
                    " AppliedMarker=" + SafeText(appliedMarker) +
                    " SelectedResult=" + SafeText(selectedResult) +
                    " CityHasResult=" + hasCityReadback +
                    " ImpactText=" + SafeText(city != null ? city.LastDispatchImpactText : "None") +
                    " StatusChange=" + SafeText(latestResult != null ? latestResult.CityStatusChangeSummaryText : "None") +
                    " Objective=" + SafeText(latestResult != null ? latestResult.MissionObjectiveText : "None") +
                    " ExpectedObjective=" + SafeText(contextObjective) +
                    " Relevance=" + SafeText(latestResult != null ? latestResult.MissionRelevanceText : "None") +
                    " ExpectedRelevance=" + SafeText(contextRelevance) +
                    " OutcomeMeaning=" + SafeText(outcomeMeaningId) +
                    " ExpectedOutcomeMeaning=" + SafeText(contextOutcomeMeaningId) +
                    " CityImpactMeaning=" + SafeText(cityImpactMeaning) +
                    " ExpectedCityImpactMeaning=" + SafeText(contextCityImpactMeaning) +
                    " RecommendationShift=" + SafeText(recommendationShift) +
                    " ExpectedRecommendationShift=" + SafeText(contextRecommendationShift) +
                    " WorldReturnSummary=" + SafeText(latestResult != null ? latestResult.WorldReturnSummaryText : "None") + ".");
                AdvanceTo(SmokeStep.Shutdown, "Checkpoint summary recorded.");
                return;
            }

            RecordPass(
                "ResultPipeline -> WorldSim board refresh",
                "World board reflects result. Stage=" + _boot.CurrentAppFlowStage +
                " SelectedCity=" + SafeText(_selectedCityId) +
                " BoardSourceCity=" + SafeText(boardSourceCity) +
                " AppFlowSourceCity=" + SafeText(appFlowSourceCity) +
                " AppliedMarker=" + SafeText(appliedMarker) +
                " SelectedResult=" + SafeText(selectedResult) +
                " Impact=" + SafeText(city.LastDispatchImpactText) +
                " StatusChange=" + SafeText(latestResult.CityStatusChangeSummaryText) +
                " Objective=" + SafeText(latestResult.MissionObjectiveText) +
                " Relevance=" + SafeText(latestResult.MissionRelevanceText) +
                " OutcomeMeaning=" + SafeText(outcomeMeaningId) +
                " CityImpactMeaning=" + SafeText(cityImpactMeaning) +
                " RecommendationShift=" + SafeText(recommendationShift) +
                " WorldReturnSummary=" + SafeText(latestResult.WorldReturnSummaryText) + ".");
            AdvanceTo(SmokeStep.ValidateCityHubDecision, "World refresh validated.");
        }

        private void ValidateCityHubDecision()
        {
            WorldBoardReadModel board = _boot.GetWorldBoardReadModel();
            CityStatusReadModel city = FindCity(board, _selectedCityId);
            CityDecisionReadModel decision = city != null ? city.Decision : null;
            RecentImpactSummary topImpact = GetFirstRecentImpact(decision);
            CityActionRecommendation topRecommendation = GetFirstRecommendation(decision);
            string expectedSummary = BuildExpectedCityHubRecentImpactSummary(city);
            string actualSummary = topImpact != null ? topImpact.SummaryText : string.Empty;
            string latestStatusChange = city != null && city.LatestResult != null ? city.LatestResult.CityStatusChangeSummaryText : string.Empty;
            string latestResultSummary = city != null && city.LatestResult != null ? city.LatestResult.SummaryText : string.Empty;
            string legacyImpact = city != null ? city.LastDispatchImpactText : string.Empty;

            bool hasTopImpact = topImpact != null && HasMeaningfulValue(actualSummary);
            bool summaryMatches = hasTopImpact && actualSummary == expectedSummary;
            bool hasDecisionHint = topImpact != null && topImpact.ShouldAffectNextDecision && HasMeaningfulValue(topImpact.NextDecisionHintText);
            bool hasRecommendation = topRecommendation != null &&
                                     HasMeaningfulValue(topRecommendation.SummaryText) &&
                                     HasMeaningfulValue(topRecommendation.ReasonText);

            if (!hasTopImpact || !summaryMatches || !hasDecisionHint || !hasRecommendation)
            {
                RecordFail(
                    "WorldSim -> CityHub recent impact coherence",
                    "CityHub decision did not consume the refreshed result cleanly. Stage=" + _boot.CurrentAppFlowStage +
                    " SelectedCity=" + SafeText(_selectedCityId) +
                    " ExpectedSummary=" + SafeText(expectedSummary) +
                    " ActualSummary=" + SafeText(actualSummary) +
                    " LatestStatusChange=" + SafeText(latestStatusChange) +
                    " LatestResultSummary=" + SafeText(latestResultSummary) +
                    " LegacyImpact=" + SafeText(legacyImpact) +
                    " Hint=" + SafeText(topImpact != null ? topImpact.NextDecisionHintText : "None") +
                    " Recommendation=" + SafeText(topRecommendation != null ? topRecommendation.SummaryText : "None") +
                    " RecommendationReason=" + SafeText(topRecommendation != null ? topRecommendation.ReasonText : "None") + ".");
                AdvanceTo(SmokeStep.Shutdown, "Checkpoint summary recorded.");
                return;
            }

            RecordPass(
                "WorldSim -> CityHub recent impact coherence",
                "CityHub consumes the refreshed result. Stage=" + _boot.CurrentAppFlowStage +
                " SelectedCity=" + SafeText(_selectedCityId) +
                " ImpactSummary=" + SafeText(actualSummary) +
                " Hint=" + SafeText(topImpact.NextDecisionHintText) +
                " Recommendation=" + SafeText(topRecommendation.SummaryText) +
                " RecommendationReason=" + SafeText(topRecommendation.ReasonText) + ".");
            AdvanceTo(SmokeStep.ValidateCityHubDecisionRelevance, "CityHub decision validated.");
        }

        private void ValidateCityHubDecisionRelevance()
        {
            WorldBoardReadModel board = _boot.GetWorldBoardReadModel();
            CityStatusReadModel city = FindCity(board, _selectedCityId);
            CityDecisionReadModel decision = city != null ? city.Decision : null;
            ExpeditionResultReadModel latestResult = city != null ? city.LatestResult : null;
            RecentImpactSummary topImpact = GetFirstRecentImpact(decision);
            CityActionRecommendation topRecommendation = GetFirstRecommendation(decision);
            CityBottleneckSignal topBottleneck = GetFirstBottleneck(decision);
            CityOpportunitySignal topOpportunity = GetFirstOpportunity(decision);

            string worldReturnSummary = latestResult != null ? latestResult.WorldReturnSummaryText : string.Empty;
            string missionRelevance = latestResult != null ? latestResult.MissionRelevanceText : string.Empty;
            string impactSummary = topImpact != null ? topImpact.SummaryText : string.Empty;
            string hintText = topImpact != null ? topImpact.NextDecisionHintText : string.Empty;
            string recommendationReason = topRecommendation != null ? topRecommendation.ReasonText : string.Empty;
            string opportunityReason = topOpportunity != null ? topOpportunity.WhyItMattersText : string.Empty;
            string whyText = decision != null ? decision.WhyCityMattersText : string.Empty;
            string bottleneckResource = topBottleneck != null ? topBottleneck.ResourceId : string.Empty;
            string rewardResource = latestResult != null ? latestResult.RewardResourceId : string.Empty;
            string cityImpactMeaning = latestResult != null ? latestResult.CityImpactMeaningText : string.Empty;
            string recommendationShift = latestResult != null ? latestResult.RecommendationShiftText : string.Empty;
            GoldenPathCityDecisionMeaningDefinition cityDecisionMeaning = null;
            bool hasSharedCityDecisionMeaning = GoldenPathContentRegistry.TryGetCityDecisionMeaningForChain(
                _selectedCityId,
                latestResult != null ? latestResult.TargetDungeonId : string.Empty,
                out GoldenPathChainDefinition _,
                out cityDecisionMeaning);
            string cityDecisionMeaningId = cityDecisionMeaning != null ? cityDecisionMeaning.CityDecisionMeaningId : string.Empty;

            bool impactTracksWorldReturn = HasMeaningfulValue(worldReturnSummary) &&
                                           HasMatchingContractText(impactSummary, worldReturnSummary);
            bool hintTracksDecisionShift = HasMeaningfulValue(hintText) &&
                                           (ContainsValue(hintText, recommendationShift) ||
                                            ContainsValue(hintText, cityImpactMeaning) ||
                                            ContainsValue(hintText, missionRelevance) ||
                                            ContainsValue(hintText, rewardResource) ||
                                            ContainsValue(hintText, bottleneckResource) ||
                                            ContainsValue(hintText, "need pressure") ||
                                            ContainsValue(hintText, "dispatch"));
            bool sharedShiftTracksHintOrReason = !HasMeaningfulValue(recommendationShift) ||
                                                 ContainsValue(hintText, recommendationShift) ||
                                                 ContainsValue(recommendationReason, recommendationShift);
            bool recommendationTracksImpact = HasMeaningfulValue(recommendationReason) &&
                                             (ContainsValue(recommendationReason, impactSummary) ||
                                              ContainsValue(recommendationReason, hintText) ||
                                              ContainsValue(recommendationReason, cityImpactMeaning) ||
                                              ContainsValue(recommendationReason, missionRelevance) ||
                                              ContainsValue(recommendationReason, rewardResource) ||
                                              ContainsValue(recommendationReason, bottleneckResource));
            bool sharedCityMeaningTracksDecision = !hasSharedCityDecisionMeaning ||
                                                   ((!HasMeaningfulValue(cityDecisionMeaning.OpportunityReasonText) ||
                                                     ContainsValue(opportunityReason, cityDecisionMeaning.OpportunityReasonText)) &&
                                                    (!HasMeaningfulValue(cityDecisionMeaning.RecommendationRationaleText) ||
                                                     ContainsValue(recommendationReason, cityDecisionMeaning.RecommendationRationaleText)) &&
                                                    (!HasMeaningfulValue(cityDecisionMeaning.WhyCityMattersText) ||
                                                     ContainsValue(whyText, cityDecisionMeaning.WhyCityMattersText) ||
                                                     ContainsValue(hintText, cityDecisionMeaning.WhyCityMattersText)));

            if (!impactTracksWorldReturn || !hintTracksDecisionShift || !sharedShiftTracksHintOrReason || !recommendationTracksImpact || !sharedCityMeaningTracksDecision)
            {
                RecordFail(
                    "WorldSim -> CityHub decision relevance carry-through",
                    "CityHub recent impact did not drive the refreshed decision signals cleanly. Stage=" + _boot.CurrentAppFlowStage +
                    " SelectedCity=" + SafeText(_selectedCityId) +
                    " WorldReturnSummary=" + SafeText(worldReturnSummary) +
                    " ImpactSummary=" + SafeText(impactSummary) +
                    " MissionRelevance=" + SafeText(missionRelevance) +
                    " CityImpactMeaning=" + SafeText(cityImpactMeaning) +
                    " RecommendationShift=" + SafeText(recommendationShift) +
                    " CityDecisionMeaning=" + SafeText(cityDecisionMeaningId) +
                    " Hint=" + SafeText(hintText) +
                    " OpportunityReason=" + SafeText(opportunityReason) +
                    " Recommendation=" + SafeText(topRecommendation != null ? topRecommendation.SummaryText : "None") +
                    " RecommendationReason=" + SafeText(recommendationReason) +
                    " WhyCityMatters=" + SafeText(whyText) +
                    " RewardResource=" + SafeText(rewardResource) +
                    " BottleneckResource=" + SafeText(bottleneckResource) + ".");
                AdvanceTo(SmokeStep.Shutdown, "Checkpoint summary recorded.");
                return;
            }

            RecordPass(
                "WorldSim -> CityHub decision relevance carry-through",
                "CityHub decision signals point at the refreshed result. Stage=" + _boot.CurrentAppFlowStage +
                " SelectedCity=" + SafeText(_selectedCityId) +
                " ImpactSummary=" + SafeText(impactSummary) +
                " Hint=" + SafeText(hintText) +
                " OpportunityReason=" + SafeText(opportunityReason) +
                " Recommendation=" + SafeText(topRecommendation != null ? topRecommendation.SummaryText : "None") +
                " RecommendationReason=" + SafeText(recommendationReason) +
                " RecommendationShift=" + SafeText(recommendationShift) +
                " CityDecisionMeaning=" + SafeText(cityDecisionMeaningId) +
                " WhyCityMatters=" + SafeText(whyText) +
                " BottleneckResource=" + SafeText(bottleneckResource) + ".");
            AdvanceTo(SmokeStep.ValidateWorldReturnChainSummary, "CityHub relevance validated.");
        }

        private void ValidateWorldReturnChainSummary()
        {
            WorldBoardReadModel board = _boot.GetWorldBoardReadModel();
            CityStatusReadModel city = FindCity(board, _selectedCityId);
            CityDecisionReadModel decision = city != null ? city.Decision : null;
            RecentImpactSummary topImpact = GetFirstRecentImpact(decision);
            CityActionRecommendation topRecommendation = GetFirstRecommendation(decision);
            string whyText = decision != null ? decision.WhyCityMattersText : string.Empty;
            string impactResource = topImpact != null ? topImpact.ResourceId : string.Empty;
            string impactDungeon = topImpact != null ? topImpact.RelatedDungeonId : string.Empty;

            bool hasWhyText = HasMeaningfulValue(whyText);
            bool tracksNextDecision = ContainsValue(whyText, "next dispatch") || ContainsValue(whyText, "next decision");
            bool tracksImpactSubject = ContainsValue(whyText, impactResource) ||
                                       ContainsValue(whyText, impactDungeon) ||
                                       ContainsValue(whyText, city != null && city.LatestResult != null ? city.LatestResult.RewardResourceId : string.Empty) ||
                                       ContainsValue(whyText, city != null && city.LatestResult != null ? city.LatestResult.TargetDungeonId : string.Empty);

            if (!hasWhyText || (topImpact != null && topImpact.ShouldAffectNextDecision && (!tracksNextDecision || !tracksImpactSubject)))
            {
                RecordFail(
                    "World return chain causal summary",
                    "CityHub why-now text did not close the world return chain cleanly. Stage=" + _boot.CurrentAppFlowStage +
                    " SelectedCity=" + SafeText(_selectedCityId) +
                    " WhyCityMatters=" + SafeText(whyText) +
                    " ImpactSummary=" + SafeText(topImpact != null ? topImpact.SummaryText : "None") +
                    " ImpactResource=" + SafeText(impactResource) +
                    " ImpactDungeon=" + SafeText(impactDungeon) +
                    " Recommendation=" + SafeText(topRecommendation != null ? topRecommendation.SummaryText : "None") + ".");
                AdvanceTo(SmokeStep.Shutdown, "Checkpoint summary recorded.");
                return;
            }

            RecordPass(
                "World return chain causal summary",
                "CityHub why-now text closes the return chain. Stage=" + _boot.CurrentAppFlowStage +
                " SelectedCity=" + SafeText(_selectedCityId) +
                " WhyCityMatters=" + SafeText(whyText) +
                " ImpactSummary=" + SafeText(topImpact != null ? topImpact.SummaryText : "None") +
                " Recommendation=" + SafeText(topRecommendation != null ? topRecommendation.SummaryText : "None") + ".");
            AdvanceTo(SmokeStep.ReenterExpeditionPrepAfterWorldReturn, "World return chain summary validated.");
        }

        private void ReenterExpeditionPrepAfterWorldReturn()
        {
            if (!_boot.TryEnterSelectedWorldDungeon())
            {
                RecordFail(
                    "CityHub -> ExpeditionPrep re-entry continuity",
                    "Failed to re-enter ExpeditionPrep after world return. Stage=" + _boot.CurrentAppFlowStage +
                    " SelectedCity=" + SafeText(_selectedCityId) +
                    " WhyCityMatters=" + SafeText(_boot.SelectedRecommendationReasonLabel) + ".");
                AdvanceTo(SmokeStep.Shutdown, "Checkpoint summary recorded.");
                return;
            }

            AdvanceTo(SmokeStep.ValidatePrepReentryContinuity, "Requested ExpeditionPrep after world return.");
        }

        private void ValidatePrepReentryContinuity()
        {
            if (_boot.CurrentAppFlowStage != AppFlowStage.ExpeditionPrep || !_boot.IsDungeonRouteChoiceVisible)
            {
                return;
            }

            WorldBoardReadModel board = _boot.GetWorldBoardReadModel();
            CityStatusReadModel city = FindCity(board, _selectedCityId);
            CityDecisionReadModel decision = city != null ? city.Decision : null;
            RecentImpactSummary topImpact = GetFirstRecentImpact(decision);
            CityActionRecommendation topRecommendation = GetFirstRecommendation(decision);
            AppFlowExpeditionPlan prepContext = _boot.CurrentAppFlowContext != null
                ? _boot.CurrentAppFlowContext.ActiveExpeditionPlan
                : null;
            ExpeditionPrepReadModel prepReadModel = prepContext != null ? prepContext.PrepReadModel : null;
            string routeDescription = SafeText(_boot.RouteChoiceDescriptionLabel);
            string routePrompt = SafeText(_boot.RouteChoicePromptLabel);

            bool hasPrepReadModel = prepReadModel != null &&
                                    HasMeaningfulValue(prepReadModel.OriginCityId) &&
                                    HasMeaningfulValue(prepReadModel.TargetDungeonId);
            bool impactMatches = topImpact == null ||
                                 HasMatchingContractText(topImpact.SummaryText, prepReadModel != null ? prepReadModel.RecentImpactSummaryText : string.Empty);
            bool recommendationSummaryMatches = topRecommendation == null ||
                                                HasMatchingContractText(topRecommendation.SummaryText, prepReadModel != null ? prepReadModel.RecommendedActionSummaryText : string.Empty);
            bool recommendationReasonMatches = topRecommendation == null ||
                                               HasMatchingContractText(topRecommendation.ReasonText, prepReadModel != null ? prepReadModel.RecommendedActionReasonText : string.Empty) ||
                                               ContainsValue(prepReadModel != null ? prepReadModel.WhyNowText : string.Empty, topRecommendation.ReasonText);
            bool descriptionTracksReturnedDecision = ContainsValue(routeDescription, prepReadModel != null ? prepReadModel.RecentImpactSummaryText : string.Empty) ||
                                                     ContainsValue(routeDescription, prepReadModel != null ? prepReadModel.RecommendedActionSummaryText : string.Empty) ||
                                                     ContainsValue(routeDescription, prepReadModel != null ? prepReadModel.WhyNowText : string.Empty) ||
                                                     DescriptionCarriesReturnedAftermath(routeDescription, prepReadModel);
            bool promptTracksSignal = ContainsValue(routePrompt, prepReadModel != null ? prepReadModel.RecommendedActionSummaryText : string.Empty) ||
                                      ContainsValue(routePrompt, prepReadModel != null ? prepReadModel.RecentImpactHintText : string.Empty) ||
                                      PromptCarriesLaunchGate(routePrompt, prepReadModel);
            bool whyNowFitsPrompt = prepReadModel != null &&
                                    HasMeaningfulValue(prepReadModel.WhyNowText) &&
                                    prepReadModel.WhyNowText.Length <= 120;
            bool whyNowAvoidsRepeatedClauses = !HasRepeatedReadableClause(prepReadModel != null ? prepReadModel.WhyNowText : string.Empty);
            bool launchTargetMatchesRecommendation = topRecommendation == null ||
                                                     topRecommendation.ActionType != CityRecommendedActionType.LaunchLinkedDungeonExpedition ||
                                                     !HasMeaningfulValue(topRecommendation.TargetDungeonId) ||
                                                     (prepReadModel != null && prepReadModel.TargetDungeonId == topRecommendation.TargetDungeonId);

            if (!hasPrepReadModel ||
                !impactMatches ||
                !recommendationSummaryMatches ||
                !recommendationReasonMatches ||
                !descriptionTracksReturnedDecision ||
                !promptTracksSignal ||
                !whyNowFitsPrompt ||
                !whyNowAvoidsRepeatedClauses ||
                !launchTargetMatchesRecommendation)
            {
                RecordFail(
                    "CityHub -> ExpeditionPrep re-entry continuity",
                    "Returned city decision did not carry cleanly into ExpeditionPrep. Stage=" + _boot.CurrentAppFlowStage +
                    " SelectedCity=" + SafeText(_selectedCityId) +
                    " TargetDungeon=" + SafeText(prepReadModel != null ? prepReadModel.TargetDungeonId : "None") +
                    " ImpactSummary=" + SafeText(prepReadModel != null ? prepReadModel.RecentImpactSummaryText : "None") +
                    " ImpactHint=" + SafeText(prepReadModel != null ? prepReadModel.RecentImpactHintText : "None") +
                    " Recommendation=" + SafeText(prepReadModel != null ? prepReadModel.RecommendedActionSummaryText : "None") +
                    " RecommendationReason=" + SafeText(prepReadModel != null ? prepReadModel.RecommendedActionReasonText : "None") +
                    " WhyNow=" + SafeText(prepReadModel != null ? prepReadModel.WhyNowText : "None") +
                    " WhyNowLength=" + (prepReadModel != null && prepReadModel.WhyNowText != null ? prepReadModel.WhyNowText.Length.ToString() : "0") +
                    " DescriptionTracks=" + descriptionTracksReturnedDecision +
                    " PromptTracks=" + promptTracksSignal +
                    " RouteDescription=" + routeDescription +
                    " RoutePrompt=" + routePrompt +
                    " CityImpact=" + SafeText(topImpact != null ? topImpact.SummaryText : "None") +
                    " CityRecommendation=" + SafeText(topRecommendation != null ? topRecommendation.SummaryText : "None") +
                    " CityRecommendationReason=" + SafeText(topRecommendation != null ? topRecommendation.ReasonText : "None") + ".");
                AdvanceTo(SmokeStep.Shutdown, "Checkpoint summary recorded.");
                return;
            }

            RecordPass(
                "CityHub -> ExpeditionPrep re-entry continuity",
                "Returned city signals seed ExpeditionPrep cleanly. Stage=" + _boot.CurrentAppFlowStage +
                " SelectedCity=" + SafeText(_selectedCityId) +
                " TargetDungeon=" + SafeText(prepReadModel.TargetDungeonId) +
                " ImpactSummary=" + SafeText(prepReadModel.RecentImpactSummaryText) +
                " Signal=" + SafeText(prepReadModel.RecommendedActionSummaryText) +
                " WhyNow=" + SafeText(prepReadModel.WhyNowText) +
                " WhyNowLength=" + (prepReadModel.WhyNowText != null ? prepReadModel.WhyNowText.Length.ToString() : "0") +
                " RoutePrompt=" + routePrompt + ".");
            AdvanceTo(SmokeStep.Shutdown, "Smoke complete.");
        }

        private bool DescriptionCarriesReturnedAftermath(string routeDescription, ExpeditionPrepReadModel prepReadModel)
        {
            if (!HasMeaningfulValue(routeDescription) || prepReadModel == null)
            {
                return false;
            }

            bool hasAftermathAnchor = ContainsValue(routeDescription, "Aftermath") ||
                                      ContainsValue(routeDescription, "Last run") ||
                                      ContainsValue(routeDescription, "Last route");
            bool hasSelectionIdentity = ContainsValue(routeDescription, prepReadModel.OriginCityLabel) ||
                                        ContainsValue(routeDescription, prepReadModel.OriginCityId) ||
                                        ContainsValue(routeDescription, prepReadModel.TargetDungeonLabel) ||
                                        ContainsValue(routeDescription, prepReadModel.TargetDungeonId);
            bool hasReturnedResultEvidence = ContainsValue(routeDescription, "Clear") ||
                                             ContainsValue(routeDescription, "returned") ||
                                             ContainsValue(routeDescription, "mana_shard") ||
                                             ContainsValue(routeDescription, prepReadModel.RecommendedActionReasonText);
            return hasAftermathAnchor && hasSelectionIdentity && hasReturnedResultEvidence;
        }

        private bool PromptCarriesLaunchGate(string routePrompt, ExpeditionPrepReadModel prepReadModel)
        {
            if (!HasMeaningfulValue(routePrompt) || prepReadModel == null || prepReadModel.LaunchReadiness == null)
            {
                return false;
            }

            LaunchReadiness readiness = prepReadModel.LaunchReadiness;
            if (ContainsValue(routePrompt, readiness.SummaryText))
            {
                return true;
            }

            if (readiness.CanLaunch && ContainsValue(routePrompt, "Commit allowed"))
            {
                return true;
            }

            if (readiness.HasWarnings && ContainsValue(routePrompt, "warning"))
            {
                return true;
            }

            PrepBlocker[] blockingIssues = readiness.BlockingIssues;
            if (blockingIssues != null)
            {
                for (int i = 0; i < blockingIssues.Length; i++)
                {
                    PrepBlocker issue = blockingIssues[i];
                    if (issue != null && ContainsValue(routePrompt, issue.SummaryText))
                    {
                        return true;
                    }
                }
            }

            PrepBlocker[] warningIssues = readiness.WarningIssues;
            if (warningIssues != null)
            {
                for (int i = 0; i < warningIssues.Length; i++)
                {
                    PrepBlocker issue = warningIssues[i];
                    if (issue != null && ContainsValue(routePrompt, issue.SummaryText))
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        private void ResolveExploreStep()
        {
            object worldView = GetWorldView();
            if (worldView == null)
            {
                Fail("World view disappeared during dungeon run.");
                return;
            }

            object room = InvokeNonPublicMethod(worldView, "GetCurrentPlannedRoomStep");
            bool exitUnlocked = GetPrivateField<bool>(worldView, "_exitUnlocked");
            bool eliteDefeated = GetPrivateField<bool>(worldView, "_eliteDefeated");
            Vector2Int exitGrid = GetPrivateField<Vector2Int>(worldView, "_exitGridPosition");

            if (exitUnlocked && eliteDefeated)
            {
                SetPrivateField(worldView, "_playerGridPosition", exitGrid);
                InvokeNonPublicMethod(worldView, "ProcessExploreStep");
                return;
            }

            if (room == null)
            {
                return;
            }

            Vector2Int markerPosition = GetFieldValue<Vector2Int>(room, "MarkerPosition");
            SetPrivateField(worldView, "_playerGridPosition", markerPosition);
            InvokeNonPublicMethod(worldView, "ProcessExploreStep");
            InvokePublicMethod(worldView, "TryInteractCurrentDungeonRoomBeat");
        }

        private void ResolveBattleStep()
        {
            object worldView = GetWorldView();
            if (worldView == null)
            {
                Fail("World view disappeared while battle was active.");
                return;
            }

            string battleSnapshot = BuildBattleSnapshot(worldView);
            if (_lastBattleSnapshot != battleSnapshot)
            {
                _lastBattleSnapshot = battleSnapshot;
                _battleActionAttempts = 0;
                _battleTargetAttempts = 0;
                _battleEnemyAttempts = 0;
                Debug.Log("[Batch10Smoke] Battle :: " + battleSnapshot);
            }

            string battleState = SafeText(_boot.BattlePhaseLabel);
            if (battleState == "Player Turn")
            {
                if (!_boot.TryTriggerBattleAction("attack"))
                {
                    _battleActionAttempts += 1;
                    if (_battleActionAttempts >= 20)
                    {
                        Fail("Failed to queue attack during player turn. " + battleSnapshot);
                    }
                }

                return;
            }

            if (battleState == "Target Select")
            {
                if (TryResolveLivingBattleTarget(worldView, out string detail, out bool waitingOnTransientState))
                {
                    return;
                }

                _battleTargetAttempts += 1;
                if (waitingOnTransientState)
                {
                    if (_battleTargetAttempts == 1 || _battleTargetAttempts % 10 == 0)
                    {
                        Debug.Log("[Batch10Smoke] Waiting for target select to stabilize. " + detail);
                    }
                    return;
                }

                if (_battleTargetAttempts >= 20)
                {
                    Fail("Failed to resolve target selection. " + detail + " " + battleSnapshot);
                }

                return;
            }

            if (battleState == "Enemy Turn")
            {
                _battleEnemyAttempts += 1;
                InvokeNonPublicMethod(worldView, "ExecuteQueuedEnemyIntent");
                if (_battleEnemyAttempts == 1 || _battleEnemyAttempts % 15 == 0)
                {
                    Debug.Log("[Batch10Smoke] Executing enemy intent. " + battleSnapshot);
                }
            }
        }

        private void TrackBattleCheckpoint()
        {
            if (_boot.CurrentAppFlowStage == AppFlowStage.BattleScene)
            {
                if (!_loggedBattleEntry)
                {
                    AppFlowContext appFlowContext = _boot.CurrentAppFlowContext;
                    AppFlowBattleContext battleContext = appFlowContext != null ? appFlowContext.PendingBattle : null;
                    BattleHandoffPayload handoff = battleContext != null ? battleContext.HandoffPayload : null;
                    PrototypeBattleRequest request = _boot.GetBattleRequest();
                    PrototypeBattleUiSurfaceData surface = _boot.GetBattleUiSurfaceData();
                    int partyCount = request != null && request.PartyMembers != null ? request.PartyMembers.Length : 0;
                    int enemyCount = request != null && request.EnemyMembers != null ? request.EnemyMembers.Length : 0;
                    string objectiveText = request != null ? request.ObjectiveText : string.Empty;
                    string rewardText = request != null ? request.RewardPreviewText : string.Empty;
                    string riskText = request != null ? request.RiskContextText : string.Empty;
                    bool hasGridRoomContext = HasMeaningfulValue(request != null ? request.RoomId : string.Empty) &&
                                              HasMeaningfulValue(request != null ? request.RoomTypeLabel : string.Empty);
                    bool hasStandardBattleSetup = surface != null &&
                                                  surface.IsBattleActive &&
                                                  partyCount > 0 &&
                                                  enemyCount > 0;
                    bool hasIntentCarry = HasMeaningfulValue(objectiveText) &&
                                          (HasMeaningfulValue(rewardText) || HasMeaningfulValue(riskText));
                    bool requiresSharedEncounterAuthoring = string.Equals(request != null ? request.DungeonId : string.Empty, "dungeon-alpha", StringComparison.Ordinal) &&
                                                            string.Equals(request != null ? request.RouteId : string.Empty, "safe", StringComparison.Ordinal);
                    bool hasSharedEncounterAuthoring = !requiresSharedEncounterAuthoring ||
                                                       (HasMeaningfulValue(request != null ? request.EncounterProfileId : string.Empty) &&
                                                        HasMeaningfulValue(request != null ? request.BattleSetupId : string.Empty) &&
                                                        HasMeaningfulValue(request != null ? request.EncounterProfileSourceText : string.Empty) &&
                                                        HasMeaningfulValue(request != null ? request.BattleSetupSourceText : string.Empty) &&
                                                        request.EncounterProfileSourceText.StartsWith("shared:", StringComparison.Ordinal) &&
                                                        request.BattleSetupSourceText.StartsWith("shared:", StringComparison.Ordinal));
                    bool handoffMatches = handoff != null &&
                                          HasMatchingContractText(handoff.ObjectiveText, objectiveText) &&
                                          HasMatchingContractText(handoff.RiskContextText, riskText);
                    bool surfaceMatches = surface != null &&
                                          HasMatchingContractText(surface.MissionObjectiveText, objectiveText) &&
                                          HasMatchingContractText(surface.MissionRewardPreviewText, rewardText) &&
                                          HasMatchingContractText(surface.MissionRiskContextText, riskText);

                    if (!hasGridRoomContext || !hasStandardBattleSetup || !hasIntentCarry || !hasSharedEncounterAuthoring || !handoffMatches || !surfaceMatches)
                    {
                        RecordFail(
                            "DungeonRun -> BattleScene intent carry-through",
                            "Battle entry did not preserve runtime reality or expedition intent cleanly. RuntimeSurface=GridDungeon+StandardJRPG" +
                            " RoomId=" + SafeText(request != null ? request.RoomId : "None") +
                            " RoomType=" + SafeText(request != null ? request.RoomTypeLabel : "None") +
                            " PartyCount=" + partyCount +
                            " EnemyCount=" + enemyCount +
                            " Objective=" + SafeText(objectiveText) +
                            " Reward=" + SafeText(rewardText) +
                            " Risk=" + SafeText(riskText) +
                            " EncounterProfile=" + SafeText(request != null ? request.EncounterProfileId : "None") +
                            " EncounterProfileSource=" + SafeText(request != null ? request.EncounterProfileSourceText : "None") +
                            " BattleSetup=" + SafeText(request != null ? request.BattleSetupId : "None") +
                            " BattleSetupSource=" + SafeText(request != null ? request.BattleSetupSourceText : "None") +
                            " HandoffObjective=" + SafeText(handoff != null ? handoff.ObjectiveText : "None") +
                            " HandoffRisk=" + SafeText(handoff != null ? handoff.RiskContextText : "None") +
                            " SurfaceObjective=" + SafeText(surface != null ? surface.MissionObjectiveText : "None") +
                            " SurfaceReward=" + SafeText(surface != null ? surface.MissionRewardPreviewText : "None") +
                            " SurfaceRisk=" + SafeText(surface != null ? surface.MissionRiskContextText : "None") + ".");
                        AdvanceTo(SmokeStep.Shutdown, "Checkpoint summary recorded.");
                        return;
                    }

                    _loggedBattleEntry = true;
                    _battleEntryEncounterId = request != null ? request.EncounterId : string.Empty;
                    _battleEntryRoomType = request != null ? request.RoomTypeLabel : string.Empty;
                    RecordPass(
                        "DungeonRun -> BattleScene intent carry-through",
                        "Battle entry preserved runtime reality and expedition intent. RuntimeSurface=GridDungeon+StandardJRPG" +
                        " RoomType=" + SafeText(request.RoomTypeLabel) +
                        " PartyCount=" + partyCount +
                        " EnemyCount=" + enemyCount +
                        " Objective=" + SafeText(objectiveText) +
                        " Reward=" + SafeText(rewardText) +
                        " Risk=" + SafeText(riskText) +
                        " EncounterProfile=" + SafeText(request != null ? request.EncounterProfileSourceText : "None") +
                        " BattleSetup=" + SafeText(request != null ? request.BattleSetupSourceText : "None") + ".");
                }

                _sawBattleStage = true;
                return;
            }

            if (_loggedBattleReturn || !_sawBattleStage || _boot.CurrentAppFlowStage != AppFlowStage.DungeonRun)
            {
                return;
            }

            AppFlowContext context = _boot.CurrentAppFlowContext;
            AppFlowDungeonRunContext runContext = context != null ? context.CurrentDungeonRun : null;
            ExpeditionRunState runState = runContext != null ? runContext.RunState : null;
            DungeonRunReadModel screenModel = runContext != null ? runContext.ScreenModel : null;
            BattleReturnPayload returnPayload = runState != null ? runState.LastBattleReturn : null;
            string outcomeKey = runContext != null ? runContext.LastBattleOutcomeKey : PrototypeBattleOutcomeKeys.None;
            string encounterName = runContext != null ? runContext.LastEncounterName : "None";
            string missionAnchor = HasMeaningfulValue(returnPayload != null ? returnPayload.ExpectedUsefulnessText : string.Empty)
                ? returnPayload.ExpectedUsefulnessText
                : HasMeaningfulValue(returnPayload != null ? returnPayload.WhyNowText : string.Empty)
                    ? returnPayload.WhyNowText
                    : returnPayload != null ? returnPayload.ObjectiveText : string.Empty;
            bool hasReturnCarry = returnPayload != null &&
                                  HasMatchingContractText(returnPayload.EncounterId, _battleEntryEncounterId) &&
                                  HasMatchingContractText(returnPayload.EncounterRoomTypeText, _battleEntryRoomType) &&
                                  HasMeaningfulValue(returnPayload.EncounterRoomTypeText) &&
                                  HasMeaningfulValue(returnPayload.ObjectiveText) &&
                                  HasMeaningfulValue(missionAnchor) &&
                                  HasMeaningfulValue(returnPayload.RoomProgressText) &&
                                  HasMeaningfulValue(returnPayload.NextGoalText) &&
                                  HasMeaningfulValue(returnPayload.PartyConditionText);
            bool readModelCarry = screenModel != null &&
                                  ContainsValue(screenModel.BattleReturnText, returnPayload != null ? returnPayload.ResultSummaryText : string.Empty) &&
                                  ContainsValue(screenModel.BattleReturnText, missionAnchor) &&
                                  ContainsValue(screenModel.BattleReturnText, returnPayload != null ? returnPayload.NextGoalText : string.Empty) &&
                                  ContainsValue(screenModel.RecentOutcomeText, encounterName) &&
                                  ContainsValue(screenModel.RecentOutcomeText, returnPayload != null ? returnPayload.NextGoalText : string.Empty);

            if (!HasMeaningfulValue(outcomeKey) ||
                outcomeKey == PrototypeBattleOutcomeKeys.None ||
                !hasReturnCarry ||
                !readModelCarry)
            {
                if (!HasMeaningfulValue(outcomeKey) || outcomeKey == PrototypeBattleOutcomeKeys.None)
                {
                    return;
                }

                RecordFail(
                    "DungeonRun -> BattleScene -> DungeonRun return",
                    "Battle return lost mission/result continuity after re-entering DungeonRun." +
                    " Outcome=" + SafeText(outcomeKey) +
                    " Encounter=" + SafeText(encounterName) +
                    " EntryEncounterId=" + SafeText(_battleEntryEncounterId) +
                    " ReturnEncounterId=" + SafeText(returnPayload != null ? returnPayload.EncounterId : "None") +
                    " EntryRoomType=" + SafeText(_battleEntryRoomType) +
                    " RoomType=" + SafeText(returnPayload != null ? returnPayload.EncounterRoomTypeText : "None") +
                    " Objective=" + SafeText(returnPayload != null ? returnPayload.ObjectiveText : "None") +
                    " MissionAnchor=" + SafeText(missionAnchor) +
                    " RoomProgress=" + SafeText(returnPayload != null ? returnPayload.RoomProgressText : "None") +
                    " NextGoal=" + SafeText(returnPayload != null ? returnPayload.NextGoalText : "None") +
                    " PartyCondition=" + SafeText(returnPayload != null ? returnPayload.PartyConditionText : "None") +
                    " BattleReturnText=" + SafeText(screenModel != null ? screenModel.BattleReturnText : "None") +
                    " RecentOutcome=" + SafeText(screenModel != null ? screenModel.RecentOutcomeText : "None") + ".");
                AdvanceTo(SmokeStep.Shutdown, "Checkpoint summary recorded.");
                return;
            }

            _loggedBattleReturn = true;
            RecordPass(
                "DungeonRun -> BattleScene -> DungeonRun return",
                "Battle returned with mission/result continuity intact." +
                " Outcome=" + SafeText(outcomeKey) +
                " Encounter=" + SafeText(encounterName) +
                " EntryRoomType=" + SafeText(_battleEntryRoomType) +
                " RoomType=" + SafeText(returnPayload != null ? returnPayload.EncounterRoomTypeText : "None") +
                " MissionAnchor=" + SafeText(missionAnchor) +
                " NextGoal=" + SafeText(returnPayload != null ? returnPayload.NextGoalText : "None") + ".");
        }

        private void TrackResultPipelineCheckpoint(AppFlowResultContext latestResult)
        {
            if (_loggedResultPackaging)
            {
                return;
            }

            ExpeditionRunState runState = latestResult != null ? latestResult.ResolvedRunState : null;
            ExpeditionOutcome outcome = latestResult != null ? latestResult.ExpeditionOutcome : null;
            OutcomeReadback readback = latestResult != null ? latestResult.OutcomeReadback : null;
            string objectiveText = runState != null ? runState.ObjectiveText : string.Empty;
            string relevanceText = HasMeaningfulValue(runState != null ? runState.ExpectedUsefulnessText : string.Empty)
                ? runState.ExpectedUsefulnessText
                : runState != null ? runState.WhyNowText : string.Empty;
            string riskRewardText = runState != null ? runState.RiskRewardPreviewText : string.Empty;
            string pathText = runState != null ? runState.RoomPathSummaryText : string.Empty;
            string resultStateKey = runState != null ? runState.ResultStateKey : string.Empty;
            int runSurvivingMemberCount = runState != null &&
                                          runState.ResultSnapshot != null &&
                                          runState.ResultSnapshot.PartyOutcome != null
                ? runState.ResultSnapshot.PartyOutcome.SurvivingMemberCount
                : -1;
            bool outcomeCarry = outcome != null &&
                                HasMatchingContractText(outcome.MissionObjectiveText, objectiveText) &&
                                HasMatchingContractText(outcome.MissionRelevanceText, relevanceText) &&
                                HasMatchingContractText(outcome.RiskRewardContextText, riskRewardText) &&
                                HasMatchingContractText(outcome.RunPathSummaryText, pathText) &&
                                HasMatchingContractText(outcome.ResultStateKey, resultStateKey);
            bool readbackCarry = readback != null &&
                                 HasMatchingContractText(readback.MissionObjectiveText, outcome != null ? outcome.MissionObjectiveText : string.Empty) &&
                                 HasMatchingContractText(readback.MissionRelevanceText, outcome != null ? outcome.MissionRelevanceText : string.Empty) &&
                                 HasMatchingContractText(readback.RiskRewardContextText, outcome != null ? outcome.RiskRewardContextText : string.Empty) &&
                                 HasMatchingContractText(readback.RunPathSummaryText, outcome != null ? outcome.RunPathSummaryText : string.Empty);
            bool resultMetricsAlive = runState != null &&
                                      outcome != null &&
                                      outcome.ClearedEncounterCount == runState.ClearedEncounterCount &&
                                      outcome.SurvivingMemberCount == runSurvivingMemberCount;

            if (!outcomeCarry || !readbackCarry || !resultMetricsAlive)
            {
                RecordFail(
                    "DungeonRun -> ResultPipeline intent packaging",
                    "ResultPipeline did not preserve run intent/result continuity." +
                    " Objective=" + SafeText(objectiveText) +
                    " OutcomeObjective=" + SafeText(outcome != null ? outcome.MissionObjectiveText : "None") +
                    " Relevance=" + SafeText(relevanceText) +
                    " OutcomeRelevance=" + SafeText(outcome != null ? outcome.MissionRelevanceText : "None") +
                    " RiskReward=" + SafeText(riskRewardText) +
                    " OutcomeRiskReward=" + SafeText(outcome != null ? outcome.RiskRewardContextText : "None") +
                    " RunPath=" + SafeText(pathText) +
                    " OutcomePath=" + SafeText(outcome != null ? outcome.RunPathSummaryText : "None") +
                    " ResultState=" + SafeText(resultStateKey) +
                    " OutcomeState=" + SafeText(outcome != null ? outcome.ResultStateKey : "None") +
                    " Cleared=" + (outcome != null ? outcome.ClearedEncounterCount : -1) + "/" + (runState != null ? runState.ClearedEncounterCount : -1) +
                    " Survivors=" + (outcome != null ? outcome.SurvivingMemberCount : -1) + "/" + runSurvivingMemberCount + ".");
                AdvanceTo(SmokeStep.Shutdown, "Checkpoint summary recorded.");
                return;
            }

            _loggedResultPackaging = true;
            RecordPass(
                "DungeonRun -> ResultPipeline intent packaging",
                "ResultPipeline packaged run intent and result context cleanly." +
                " Objective=" + SafeText(objectiveText) +
                " Relevance=" + SafeText(relevanceText) +
                " RiskReward=" + SafeText(riskRewardText) +
                " RunPath=" + SafeText(pathText) +
                " ResultState=" + SafeText(resultStateKey) + ".");
        }

        private WorldSelectableMarker FindFirstCityMarker()
        {
            WorldSelectableMarker[] markers = UnityEngine.Object.FindObjectsByType<WorldSelectableMarker>(FindObjectsSortMode.None);
            for (int i = 0; i < markers.Length; i++)
            {
                WorldSelectableMarker marker = markers[i];
                if (marker != null &&
                    marker.EntityData != null &&
                    marker.EntityData.Kind == WorldEntityKind.City &&
                    marker.EntityData.Id == "city-a")
                {
                    return marker;
                }
            }

            for (int i = 0; i < markers.Length; i++)
            {
                WorldSelectableMarker marker = markers[i];
                if (marker != null && marker.EntityData != null && marker.EntityData.Kind == WorldEntityKind.City)
                {
                    return marker;
                }
            }

            return null;
        }

        private CityStatusReadModel FindCity(WorldBoardReadModel board, string cityId)
        {
            if (board == null || board.Cities == null)
            {
                return null;
            }

            for (int i = 0; i < board.Cities.Length; i++)
            {
                CityStatusReadModel city = board.Cities[i];
                if (city != null && city.CityId == cityId)
                {
                    return city;
                }
            }

            return null;
        }

        private RecentImpactSummary GetFirstRecentImpact(CityDecisionReadModel decision)
        {
            if (decision == null || decision.RecentImpacts == null)
            {
                return null;
            }

            for (int i = 0; i < decision.RecentImpacts.Length; i++)
            {
                RecentImpactSummary impact = decision.RecentImpacts[i];
                if (impact != null && HasMeaningfulValue(impact.SummaryText))
                {
                    return impact;
                }
            }

            return null;
        }

        private CityBottleneckSignal GetFirstBottleneck(CityDecisionReadModel decision)
        {
            if (decision == null || decision.Bottlenecks == null)
            {
                return null;
            }

            for (int i = 0; i < decision.Bottlenecks.Length; i++)
            {
                CityBottleneckSignal bottleneck = decision.Bottlenecks[i];
                if (bottleneck != null && HasMeaningfulValue(bottleneck.SummaryText))
                {
                    return bottleneck;
                }
            }

            return null;
        }

        private CityOpportunitySignal GetFirstOpportunity(CityDecisionReadModel decision)
        {
            if (decision == null || decision.Opportunities == null)
            {
                return null;
            }

            for (int i = 0; i < decision.Opportunities.Length; i++)
            {
                CityOpportunitySignal opportunity = decision.Opportunities[i];
                if (opportunity != null && HasMeaningfulValue(opportunity.WhyItMattersText))
                {
                    return opportunity;
                }
            }

            return null;
        }

        private CityActionRecommendation GetFirstRecommendation(CityDecisionReadModel decision)
        {
            if (decision == null || decision.RecommendedActions == null)
            {
                return null;
            }

            for (int i = 0; i < decision.RecommendedActions.Length; i++)
            {
                CityActionRecommendation recommendation = decision.RecommendedActions[i];
                if (recommendation != null && HasMeaningfulValue(recommendation.SummaryText))
                {
                    return recommendation;
                }
            }

            return null;
        }

        private string BuildExpectedCityHubRecentImpactSummary(CityStatusReadModel city)
        {
            ExpeditionResultReadModel latestResult = city != null ? city.LatestResult : null;
            if (latestResult != null)
            {
                if (HasMeaningfulValue(latestResult.WorldReturnSummaryText))
                {
                    return latestResult.WorldReturnSummaryText;
                }

                if (HasMeaningfulValue(latestResult.CityStatusChangeSummaryText))
                {
                    return latestResult.CityStatusChangeSummaryText;
                }

                if (HasMeaningfulValue(latestResult.SummaryText))
                {
                    return latestResult.SummaryText;
                }
            }

            if (city != null && HasMeaningfulValue(city.LastDispatchImpactText))
            {
                return city.LastDispatchImpactText;
            }

            return "None";
        }

        private bool ContainsValue(string text, string value)
        {
            return HasMeaningfulValue(text) &&
                   HasMeaningfulValue(value) &&
                   text.IndexOf(value, StringComparison.OrdinalIgnoreCase) >= 0;
        }

        private bool HasMatchingContractText(string producerText, string consumerText)
        {
            return HasMeaningfulValue(producerText) &&
                   HasMeaningfulValue(consumerText) &&
                   string.Equals(producerText, consumerText, StringComparison.Ordinal);
        }

        private bool HasRepeatedReadableClause(string text)
        {
            if (!HasMeaningfulValue(text))
            {
                return false;
            }

            string[] rawClauses = text.Replace("|", ".").Split(new[] { '.' }, StringSplitOptions.RemoveEmptyEntries);
            List<string> normalizedClauses = new List<string>();
            for (int i = 0; i < rawClauses.Length; i++)
            {
                string clause = NormalizeReadableClause(rawClauses[i]);
                if (!HasMeaningfulValue(clause) || clause.Length < 12)
                {
                    continue;
                }

                for (int j = 0; j < normalizedClauses.Count; j++)
                {
                    string existing = normalizedClauses[j];
                    if (existing == clause || existing.Contains(clause) || clause.Contains(existing))
                    {
                        return true;
                    }
                }

                normalizedClauses.Add(clause);
            }

            return false;
        }

        private string NormalizeReadableClause(string text)
        {
            if (!HasMeaningfulValue(text))
            {
                return string.Empty;
            }

            char[] buffer = new char[text.Length];
            int count = 0;
            for (int i = 0; i < text.Length; i++)
            {
                char character = text[i];
                if (char.IsLetterOrDigit(character))
                {
                    buffer[count++] = char.ToLowerInvariant(character);
                    continue;
                }

                if (char.IsWhiteSpace(character) && count > 0 && buffer[count - 1] != ' ')
                {
                    buffer[count++] = ' ';
                }
            }

            return new string(buffer, 0, count).Trim();
        }

        private bool IsRepresentativeContentChain(string cityId, string dungeonId, string routeId)
        {
            return (cityId == "city-a" &&
                    dungeonId == "dungeon-alpha" &&
                    (routeId == "safe" || routeId == "risky")) ||
                   (cityId == "city-b" &&
                    dungeonId == "dungeon-beta" &&
                    routeId == "risky");
        }

        private bool ValidateRepresentativeContentCatalog()
        {
            if (_loggedRepresentativeContentCatalog)
            {
                return true;
            }

            bool chain1Loaded = GoldenPathContentRegistry.TryGetChain("city-a", "dungeon-alpha", out GoldenPathChainDefinition chain1);
            bool chain2Loaded = GoldenPathContentRegistry.TryGetChain("city-b", "dungeon-beta", out GoldenPathChainDefinition chain2);
            bool chain3Loaded = GoldenPathContentRegistry.TryGetChainForRoute("city-a", "dungeon-alpha", "risky", out GoldenPathChainDefinition chain3);
            string chain1Source = GoldenPathContentRegistry.BuildContentSourceLabel("city-a", "dungeon-alpha", "safe");
            string chain2Source = GoldenPathContentRegistry.BuildContentSourceLabel("city-b", "dungeon-beta", "risky");
            string chain3Source = GoldenPathContentRegistry.BuildContentSourceLabel("city-a", "dungeon-alpha", "risky");
            string chain1RouteMeaning = chain1 != null && chain1.CanonicalRoute != null ? chain1.CanonicalRoute.RouteMeaningId : string.Empty;
            string chain2RouteMeaning = chain2 != null && chain2.CanonicalRoute != null ? chain2.CanonicalRoute.RouteMeaningId : string.Empty;
            string chain3RouteMeaning = chain3 != null && chain3.CanonicalRoute != null ? chain3.CanonicalRoute.RouteMeaningId : string.Empty;
            string chain1OutcomeMeaning = chain1 != null ? chain1.OutcomeMeaningId : string.Empty;
            string chain2OutcomeMeaning = chain2 != null ? chain2.OutcomeMeaningId : string.Empty;
            string chain3OutcomeMeaning = chain3 != null ? chain3.OutcomeMeaningId : string.Empty;
            string chain1CityDecisionMeaning = chain1 != null ? chain1.CityDecisionMeaningId : string.Empty;
            string chain2CityDecisionMeaning = chain2 != null ? chain2.CityDecisionMeaningId : string.Empty;
            string chain3CityDecisionMeaning = chain3 != null ? chain3.CityDecisionMeaningId : string.Empty;
            bool chain1RepresentativeRoomLoaded = GoldenPathContentRegistry.TryGetRepresentativeEncounterRoom("city-a", "dungeon-alpha", "safe", out GoldenPathChainDefinition _, out GoldenPathRouteDefinition _, out GoldenPathRoomDefinition chain1RepresentativeRoom);
            bool chain2RepresentativeRoomLoaded = GoldenPathContentRegistry.TryGetRepresentativeEncounterRoom("city-b", "dungeon-beta", "risky", out GoldenPathChainDefinition _, out GoldenPathRouteDefinition _, out GoldenPathRoomDefinition chain2RepresentativeRoom);
            bool chain3RepresentativeRoomLoaded = GoldenPathContentRegistry.TryGetRepresentativeEncounterRoom("city-a", "dungeon-alpha", "risky", out GoldenPathChainDefinition _, out GoldenPathRouteDefinition _, out GoldenPathRoomDefinition chain3RepresentativeRoom);
            string chain1EncounterProfile = chain1RepresentativeRoom != null ? chain1RepresentativeRoom.EncounterProfileId : string.Empty;
            string chain2EncounterProfile = chain2RepresentativeRoom != null ? chain2RepresentativeRoom.EncounterProfileId : string.Empty;
            string chain3EncounterProfile = chain3RepresentativeRoom != null ? chain3RepresentativeRoom.EncounterProfileId : string.Empty;
            string chain1BattleSetup = chain1RepresentativeRoom != null ? chain1RepresentativeRoom.BattleSetupId : string.Empty;
            string chain2BattleSetup = chain2RepresentativeRoom != null ? chain2RepresentativeRoom.BattleSetupId : string.Empty;
            string chain3BattleSetup = chain3RepresentativeRoom != null ? chain3RepresentativeRoom.BattleSetupId : string.Empty;
            bool chain1RouteMeaningLoaded = HasMeaningfulValue(chain1RouteMeaning) &&
                                            GoldenPathContentRegistry.TryGetRouteMeaning(chain1RouteMeaning, out GoldenPathRouteMeaningDefinition _);
            bool chain2RouteMeaningLoaded = HasMeaningfulValue(chain2RouteMeaning) &&
                                            GoldenPathContentRegistry.TryGetRouteMeaning(chain2RouteMeaning, out GoldenPathRouteMeaningDefinition _);
            bool chain3RouteMeaningLoaded = HasMeaningfulValue(chain3RouteMeaning) &&
                                            GoldenPathContentRegistry.TryGetRouteMeaning(chain3RouteMeaning, out GoldenPathRouteMeaningDefinition _);
            GoldenPathOutcomeMeaningDefinition chain1OutcomeMeaningDefinition = null;
            GoldenPathOutcomeMeaningDefinition chain2OutcomeMeaningDefinition = null;
            GoldenPathOutcomeMeaningDefinition chain3OutcomeMeaningDefinition = null;
            GoldenPathCityDecisionMeaningDefinition chain1CityDecisionMeaningDefinition = null;
            GoldenPathCityDecisionMeaningDefinition chain2CityDecisionMeaningDefinition = null;
            GoldenPathCityDecisionMeaningDefinition chain3CityDecisionMeaningDefinition = null;
            bool chain1OutcomeMeaningLoaded = HasMeaningfulValue(chain1OutcomeMeaning) &&
                                              GoldenPathContentRegistry.TryGetOutcomeMeaning(chain1OutcomeMeaning, out chain1OutcomeMeaningDefinition);
            bool chain2OutcomeMeaningLoaded = HasMeaningfulValue(chain2OutcomeMeaning) &&
                                              GoldenPathContentRegistry.TryGetOutcomeMeaning(chain2OutcomeMeaning, out chain2OutcomeMeaningDefinition);
            bool chain3OutcomeMeaningLoaded = HasMeaningfulValue(chain3OutcomeMeaning) &&
                                              GoldenPathContentRegistry.TryGetOutcomeMeaning(chain3OutcomeMeaning, out chain3OutcomeMeaningDefinition);
            bool chain1CityDecisionMeaningLoaded = HasMeaningfulValue(chain1CityDecisionMeaning) &&
                                                   GoldenPathContentRegistry.TryGetCityDecisionMeaning(chain1CityDecisionMeaning, out chain1CityDecisionMeaningDefinition);
            bool chain2CityDecisionMeaningLoaded = HasMeaningfulValue(chain2CityDecisionMeaning) &&
                                                   GoldenPathContentRegistry.TryGetCityDecisionMeaning(chain2CityDecisionMeaning, out chain2CityDecisionMeaningDefinition);
            bool chain3CityDecisionMeaningLoaded = HasMeaningfulValue(chain3CityDecisionMeaning) &&
                                                   GoldenPathContentRegistry.TryGetCityDecisionMeaning(chain3CityDecisionMeaning, out chain3CityDecisionMeaningDefinition);
            bool chain1EncounterProfileLoaded = HasMeaningfulValue(chain1EncounterProfile) &&
                                                GoldenPathContentRegistry.TryGetEncounterProfile(chain1EncounterProfile, out GoldenPathEncounterProfileDefinition _);
            bool chain2EncounterProfileLoaded = HasMeaningfulValue(chain2EncounterProfile) &&
                                                GoldenPathContentRegistry.TryGetEncounterProfile(chain2EncounterProfile, out GoldenPathEncounterProfileDefinition _);
            bool chain3EncounterProfileLoaded = HasMeaningfulValue(chain3EncounterProfile) &&
                                                GoldenPathContentRegistry.TryGetEncounterProfile(chain3EncounterProfile, out GoldenPathEncounterProfileDefinition _);
            bool chain1BattleSetupLoaded = HasMeaningfulValue(chain1BattleSetup) &&
                                           GoldenPathContentRegistry.TryGetBattleSetup(chain1BattleSetup, out GoldenPathBattleSetupDefinition _);
            bool chain2BattleSetupLoaded = HasMeaningfulValue(chain2BattleSetup) &&
                                           GoldenPathContentRegistry.TryGetBattleSetup(chain2BattleSetup, out GoldenPathBattleSetupDefinition _);
            bool chain3BattleSetupLoaded = HasMeaningfulValue(chain3BattleSetup) &&
                                           GoldenPathContentRegistry.TryGetBattleSetup(chain3BattleSetup, out GoldenPathBattleSetupDefinition _);
            string chain1RecommendationShift = chain1OutcomeMeaningLoaded && chain1OutcomeMeaningDefinition != null
                ? chain1OutcomeMeaningDefinition.RecommendationShiftText
                : string.Empty;
            string chain2RecommendationShift = chain2OutcomeMeaningLoaded && chain2OutcomeMeaningDefinition != null
                ? chain2OutcomeMeaningDefinition.RecommendationShiftText
                : string.Empty;
            string chain3RecommendationShift = chain3OutcomeMeaningLoaded && chain3OutcomeMeaningDefinition != null
                ? chain3OutcomeMeaningDefinition.RecommendationShiftText
                : string.Empty;
            bool chain1OutcomeMeaningHasShift = chain1OutcomeMeaningLoaded &&
                                                HasMeaningfulValue(chain1OutcomeMeaningDefinition.CityImpactMeaningText) &&
                                                HasMeaningfulValue(chain1RecommendationShift);
            bool chain2OutcomeMeaningHasShift = chain2OutcomeMeaningLoaded &&
                                                HasMeaningfulValue(chain2OutcomeMeaningDefinition.CityImpactMeaningText) &&
                                                HasMeaningfulValue(chain2RecommendationShift);
            bool chain3OutcomeMeaningHasShift = chain3OutcomeMeaningLoaded &&
                                                HasMeaningfulValue(chain3OutcomeMeaningDefinition.CityImpactMeaningText) &&
                                                HasMeaningfulValue(chain3RecommendationShift);
            bool chain1CityDecisionMeaningValid = chain1CityDecisionMeaningLoaded &&
                                                  HasMeaningfulValue(chain1CityDecisionMeaningDefinition.BottleneckSummaryText) &&
                                                  HasMeaningfulValue(chain1CityDecisionMeaningDefinition.OpportunityReasonText) &&
                                                  HasMeaningfulValue(chain1CityDecisionMeaningDefinition.RecommendationRationaleText) &&
                                                  HasMeaningfulValue(chain1CityDecisionMeaningDefinition.WhyCityMattersText);
            bool chain2CityDecisionMeaningValid = chain2CityDecisionMeaningLoaded &&
                                                  HasMeaningfulValue(chain2CityDecisionMeaningDefinition.BottleneckSummaryText) &&
                                                  HasMeaningfulValue(chain2CityDecisionMeaningDefinition.OpportunityReasonText) &&
                                                  HasMeaningfulValue(chain2CityDecisionMeaningDefinition.RecommendationRationaleText) &&
                                                  HasMeaningfulValue(chain2CityDecisionMeaningDefinition.WhyCityMattersText);
            bool chain3CityDecisionMeaningValid = chain3CityDecisionMeaningLoaded &&
                                                  HasMeaningfulValue(chain3CityDecisionMeaningDefinition.BottleneckSummaryText) &&
                                                  HasMeaningfulValue(chain3CityDecisionMeaningDefinition.OpportunityReasonText) &&
                                                  HasMeaningfulValue(chain3CityDecisionMeaningDefinition.RecommendationRationaleText) &&
                                                  HasMeaningfulValue(chain3CityDecisionMeaningDefinition.WhyCityMattersText);
            bool chain1Valid = chain1Loaded &&
                               chain1 != null &&
                               HasMeaningfulValue(chain1.MissionObjectiveText) &&
                               chain1RouteMeaningLoaded &&
                               chain1OutcomeMeaningHasShift &&
                               chain1CityDecisionMeaningValid &&
                               chain1RepresentativeRoomLoaded &&
                               chain1EncounterProfileLoaded &&
                               chain1BattleSetupLoaded &&
                               chain1Source.StartsWith("data:", StringComparison.Ordinal);
            bool chain2Valid = chain2Loaded &&
                               chain2 != null &&
                               HasMeaningfulValue(chain2.MissionObjectiveText) &&
                               chain2RouteMeaningLoaded &&
                               chain2OutcomeMeaningHasShift &&
                               chain2CityDecisionMeaningValid &&
                               chain2RepresentativeRoomLoaded &&
                               chain2EncounterProfileLoaded &&
                               chain2BattleSetupLoaded &&
                               chain2Source.StartsWith("data:", StringComparison.Ordinal);
            bool chain3Valid = chain3Loaded &&
                               chain3 != null &&
                               HasMeaningfulValue(chain3.MissionObjectiveText) &&
                               chain3RouteMeaningLoaded &&
                               chain3OutcomeMeaningHasShift &&
                               chain3CityDecisionMeaningValid &&
                               chain3RepresentativeRoomLoaded &&
                               chain3EncounterProfileLoaded &&
                               chain3BattleSetupLoaded &&
                               chain3Source.StartsWith("data:", StringComparison.Ordinal);
            WorldBoardReadModel board = _boot.GetWorldBoardReadModel();
            CityStatusReadModel cityB = FindCity(board, "city-b");
            CityStatusReadModel cityA = FindCity(board, "city-a");
            CityDecisionReadModel cityADecision = cityA != null && cityA.Decision != null
                ? cityA.Decision
                : CityDecisionModelBuilder.Build(board ?? WorldBoardReadModel.Empty, cityA);
            CityDecisionReadModel cityBDecision = cityB != null && cityB.Decision != null
                ? cityB.Decision
                : CityDecisionModelBuilder.Build(board ?? WorldBoardReadModel.Empty, cityB);
            CityOpportunitySignal alphaRiskyOpportunity = FindOpportunity(cityADecision, "dungeon-alpha", "risky");
            CityOpportunitySignal betaSafeOpportunity = FindOpportunity(cityBDecision, "dungeon-beta", "safe");
            bool alphaRiskyOpportunitySurfaced = alphaRiskyOpportunity != null &&
                                                HasMeaningfulValue(alphaRiskyOpportunity.SummaryText) &&
                                                HasMeaningfulValue(alphaRiskyOpportunity.WhyItMattersText) &&
                                                HasMeaningfulValue(alphaRiskyOpportunity.RouteLabel) &&
                                                HasMeaningfulValue(alphaRiskyOpportunity.ContentSourceLabel) &&
                                                alphaRiskyOpportunity.ContentSourceLabel.StartsWith("data:", StringComparison.Ordinal);
            bool betaSafeOpportunitySurfaced = betaSafeOpportunity != null &&
                                              HasMeaningfulValue(betaSafeOpportunity.SummaryText) &&
                                              HasMeaningfulValue(betaSafeOpportunity.WhyItMattersText) &&
                                              HasMeaningfulValue(betaSafeOpportunity.RouteLabel) &&
                                              HasMeaningfulValue(betaSafeOpportunity.ContentSourceLabel) &&
                                              betaSafeOpportunity.ContentSourceLabel.StartsWith("data:", StringComparison.Ordinal);

            if (!chain1Valid || !chain2Valid || !chain3Valid || !alphaRiskyOpportunitySurfaced || !betaSafeOpportunitySurfaced)
            {
                RecordFail(
                    "Post-slice representative content catalog",
                    "Representative chain catalog is missing a canonical data path." +
                    " Chain1Loaded=" + chain1Loaded +
                    " Chain1Source=" + SafeText(chain1Source) +
                    " Chain1Objective=" + SafeText(chain1 != null ? chain1.MissionObjectiveText : "None") +
                    " Chain1RouteMeaning=" + SafeText(chain1RouteMeaning) +
                    " Chain1OutcomeMeaning=" + SafeText(chain1OutcomeMeaning) +
                    " Chain1RecommendationShift=" + SafeText(chain1RecommendationShift) +
                    " Chain1CityDecisionMeaning=" + SafeText(chain1CityDecisionMeaning) +
                    " Chain1EncounterProfile=" + SafeText(chain1EncounterProfile) +
                    " Chain1BattleSetup=" + SafeText(chain1BattleSetup) +
                    " Chain2Loaded=" + chain2Loaded +
                    " Chain2Source=" + SafeText(chain2Source) +
                    " Chain2Objective=" + SafeText(chain2 != null ? chain2.MissionObjectiveText : "None") +
                    " Chain2RouteMeaning=" + SafeText(chain2RouteMeaning) +
                    " Chain2OutcomeMeaning=" + SafeText(chain2OutcomeMeaning) +
                    " Chain2RecommendationShift=" + SafeText(chain2RecommendationShift) +
                    " Chain2CityDecisionMeaning=" + SafeText(chain2CityDecisionMeaning) +
                    " Chain2EncounterProfile=" + SafeText(chain2EncounterProfile) +
                    " Chain2BattleSetup=" + SafeText(chain2BattleSetup) +
                    " Chain3Loaded=" + chain3Loaded +
                    " Chain3Source=" + SafeText(chain3Source) +
                    " Chain3Objective=" + SafeText(chain3 != null ? chain3.MissionObjectiveText : "None") +
                    " Chain3RouteMeaning=" + SafeText(chain3RouteMeaning) +
                    " Chain3OutcomeMeaning=" + SafeText(chain3OutcomeMeaning) +
                    " Chain3RecommendationShift=" + SafeText(chain3RecommendationShift) +
                    " Chain3CityDecisionMeaning=" + SafeText(chain3CityDecisionMeaning) +
                    " Chain3EncounterProfile=" + SafeText(chain3EncounterProfile) +
                    " Chain3BattleSetup=" + SafeText(chain3BattleSetup) +
                    " SurfacedRiskyOpportunity=" + (alphaRiskyOpportunitySurfaced ? "true" : "false") +
                    " SurfacedRiskySummary=" + SafeText(alphaRiskyOpportunity != null ? alphaRiskyOpportunity.SummaryText : "None") +
                    " SurfacedRiskyRoute=" + SafeText(alphaRiskyOpportunity != null ? alphaRiskyOpportunity.RouteLabel : "None") +
                    " SurfacedRiskySource=" + SafeText(alphaRiskyOpportunity != null ? alphaRiskyOpportunity.ContentSourceLabel : "None") +
                    " SurfacedSafeOpportunity=" + (betaSafeOpportunitySurfaced ? "true" : "false") +
                    " SurfacedSafeSummary=" + SafeText(betaSafeOpportunity != null ? betaSafeOpportunity.SummaryText : "None") +
                    " SurfacedSafeRoute=" + SafeText(betaSafeOpportunity != null ? betaSafeOpportunity.RouteLabel : "None") +
                    " SurfacedSafeSource=" + SafeText(betaSafeOpportunity != null ? betaSafeOpportunity.ContentSourceLabel : "None") + ".");
                AdvanceTo(SmokeStep.Shutdown, "Checkpoint summary recorded.");
                return false;
            }

            _loggedRepresentativeContentCatalog = true;
            RecordPass(
                "Post-slice representative content catalog",
                "Representative data chains are discoverable through the canonical path." +
                " Chain1=" + SafeText(chain1Source) +
                " RouteMeaning1=shared:" + SafeText(chain1RouteMeaning) +
                " OutcomeMeaning1=shared:" + SafeText(chain1OutcomeMeaning) +
                " RecommendationShift1=shared:" + SafeText(chain1RecommendationShift) +
                " CityDecisionMeaning1=shared:" + SafeText(chain1CityDecisionMeaning) +
                " EncounterProfile1=shared:" + SafeText(chain1EncounterProfile) +
                " BattleSetup1=shared:" + SafeText(chain1BattleSetup) +
                " Chain2=" + SafeText(chain2Source) +
                " RouteMeaning2=shared:" + SafeText(chain2RouteMeaning) +
                " OutcomeMeaning2=shared:" + SafeText(chain2OutcomeMeaning) +
                " RecommendationShift2=shared:" + SafeText(chain2RecommendationShift) +
                " CityDecisionMeaning2=shared:" + SafeText(chain2CityDecisionMeaning) +
                " EncounterProfile2=shared:" + SafeText(chain2EncounterProfile) +
                " BattleSetup2=shared:" + SafeText(chain2BattleSetup) +
                " Chain3=" + SafeText(chain3Source) +
                " RouteMeaning3=shared:" + SafeText(chain3RouteMeaning) +
                " OutcomeMeaning3=shared:" + SafeText(chain3OutcomeMeaning) +
                " RecommendationShift3=shared:" + SafeText(chain3RecommendationShift) +
                " CityDecisionMeaning3=shared:" + SafeText(chain3CityDecisionMeaning) +
                " EncounterProfile3=shared:" + SafeText(chain3EncounterProfile) +
                " BattleSetup3=shared:" + SafeText(chain3BattleSetup) +
                " SurfacedRiskySummary=" + SafeText(alphaRiskyOpportunity.SummaryText) +
                " SurfacedRiskyRoute=" + SafeText(alphaRiskyOpportunity.RouteLabel) +
                " SurfacedRiskySource=" + SafeText(alphaRiskyOpportunity.ContentSourceLabel) +
                " SurfacedSafeSummary=" + SafeText(betaSafeOpportunity.SummaryText) +
                " SurfacedSafeRoute=" + SafeText(betaSafeOpportunity.RouteLabel) +
                " SurfacedSafeSource=" + SafeText(betaSafeOpportunity.ContentSourceLabel) + ".");
            return true;
        }

        private CityOpportunitySignal FindOpportunity(CityDecisionReadModel decision, string dungeonId, string routeId)
        {
            if (decision == null || decision.Opportunities == null)
            {
                return null;
            }

            for (int i = 0; i < decision.Opportunities.Length; i++)
            {
                CityOpportunitySignal opportunity = decision.Opportunities[i];
                if (opportunity == null)
                {
                    continue;
                }

                bool dungeonMatches = !HasMeaningfulValue(dungeonId) || opportunity.DungeonId == dungeonId;
                bool routeMatches = !HasMeaningfulValue(routeId) || opportunity.RouteId == routeId;
                if (dungeonMatches && routeMatches)
                {
                    return opportunity;
                }
            }

            return null;
        }

        private object GetWorldView()
        {
            return GetPrivateField<object>(_boot, "_worldView");
        }

        private void RecordPass(string checkpoint, string detail)
        {
            Record(checkpoint, "PASS", detail);
        }

        private void RecordFail(string checkpoint, string detail)
        {
            Record(checkpoint, "FAIL", detail);
        }

        private void Record(string checkpoint, string status, string detail)
        {
            _checkpoints.Add(new CheckpointResult(checkpoint, status, detail));
            Debug.Log("[Batch10Smoke][" + status + "] " + checkpoint + " :: " + detail);
        }

        private void AdvanceTo(SmokeStep nextStep, string reason)
        {
            _step = nextStep;
            _stepStartedAt = EditorApplication.timeSinceStartup;
            Debug.Log("[Batch10Smoke] Step -> " + _step + " :: " + reason);
        }

        private void Fail(string detail)
        {
            RecordFail("Smoke runner", detail);
            AdvanceTo(SmokeStep.Shutdown, "Smoke failed.");
        }

        private bool TryResolveLivingBattleTarget(object worldView, out string detail, out bool waitingOnTransientState)
        {
            waitingOnTransientState = false;
            bool inputLocked = (bool)InvokeNonPublicMethod(worldView, "IsBattleInputLocked");
            object actor = InvokeNonPublicMethod(worldView, "GetCurrentActorMember");
            object monster = InvokeNonPublicMethod(worldView, "GetFirstLivingBattleMonster");
            if (monster == null)
            {
                detail = "No living battle target was available. " + BuildBattleSnapshot(worldView);
                return false;
            }

            string monsterId = GetPropertyValue<string>(monster, "MonsterId");
            string monsterName = GetPropertyValue<string>(monster, "DisplayName");
            int displayIndex = (int)InvokeNonPublicMethod(worldView, "GetBattleMonsterDisplayIndex", monsterId);
            bool resolved = (bool)InvokeNonPublicMethod(worldView, "TryResolveTargetSelection", monster);
            if (resolved)
            {
                detail = "Targeted " + SafeText(monsterName) + " at display index " + displayIndex + ".";
                return true;
            }

            waitingOnTransientState = inputLocked || actor == null;
            detail = "Target selection returned false. InputLocked=" + inputLocked +
                     " ActorPresent=" + (actor != null) +
                     " Target=" + SafeText(monsterName) +
                     " TargetId=" + SafeText(monsterId) +
                     " DisplayIndex=" + displayIndex +
                     " Monsters=" + BuildBattleMonsterSummary(worldView) + ".";
            return false;
        }

        private string BuildBattleSnapshot(object worldView)
        {
            string phaseLabel = SafeText(_boot.BattlePhaseLabel);
            string internalState = SafeText(GetPrivateField<object>(worldView, "_battleState").ToString());
            string queuedAction = SafeText(GetPrivateField<object>(worldView, "_queuedBattleAction").ToString());
            string activeMonsterId = SafeText(GetPrivateField<string>(worldView, "_activeBattleMonsterId"));
            bool inputLocked = (bool)InvokeNonPublicMethod(worldView, "IsBattleInputLocked");
            bool enemyTelegraph = GetPrivateField<bool>(worldView, "_enemyIntentTelegraphActive");
            int currentActorIndex = GetPrivateField<int>(worldView, "_currentActorIndex");
            return "PhaseLabel=" + phaseLabel +
                   " InternalState=" + internalState +
                   " QueuedAction=" + queuedAction +
                   " InputLocked=" + inputLocked +
                   " CurrentActorIndex=" + currentActorIndex +
                   " ActiveMonsterId=" + activeMonsterId +
                   " EnemyTelegraph=" + enemyTelegraph +
                   " Monsters=" + BuildBattleMonsterSummary(worldView);
        }

        private string BuildBattleMonsterSummary(object worldView)
        {
            List<string> monsters = new List<string>();
            for (int i = 0; i < 4; i++)
            {
                object monster = InvokeNonPublicMethod(worldView, "GetBattleMonsterAtDisplayIndex", i);
                if (monster == null)
                {
                    continue;
                }

                string id = SafeText(GetPropertyValue<string>(monster, "MonsterId"));
                string name = SafeText(GetPropertyValue<string>(monster, "DisplayName"));
                int hp = GetPropertyValue<int>(monster, "CurrentHp");
                bool defeated = GetPropertyValue<bool>(monster, "IsDefeated");
                monsters.Add("#" + i + "=" + name + "/" + id + "/HP:" + hp + "/Defeated:" + defeated);
            }

            return monsters.Count > 0 ? string.Join(", ", monsters) : "None";
        }

        private void Shutdown(int exitCode)
        {
            if (_shutdownRequested)
            {
                return;
            }

            _shutdownRequested = true;
            SessionState.EraseBool(SessionActiveKey);
            EditorApplication.update -= Tick;
            EditorApplication.playModeStateChanged -= OnPlayModeStateChanged;

            _summary.AppendLine("[Batch10Smoke] Summary");
            for (int i = 0; i < _checkpoints.Count; i++)
            {
                CheckpointResult checkpoint = _checkpoints[i];
                _summary.AppendLine("- " + checkpoint.Status + " :: " + checkpoint.Name + " :: " + checkpoint.Detail);
            }

            Debug.Log(_summary.ToString());

            if (EditorApplication.isPlaying)
            {
                EditorApplication.isPlaying = false;
            }

            EditorApplication.Exit(HasFailure() ? 1 : exitCode);
        }

        private void OnPlayModeStateChanged(PlayModeStateChange state)
        {
            if (_shutdownRequested)
            {
                return;
            }

            if (state == PlayModeStateChange.EnteredPlayMode)
            {
                AdvanceTo(SmokeStep.WaitForBoot, "Entered play mode.");
            }
        }

        private bool HasFailure()
        {
            for (int i = 0; i < _checkpoints.Count; i++)
            {
                if (_checkpoints[i].Status == "FAIL")
                {
                    return true;
                }
            }

            return false;
        }

        private static bool HasMeaningfulValue(string value)
        {
            return !string.IsNullOrEmpty(value) && value != "None";
        }

        private static string SafeText(string value)
        {
            return string.IsNullOrEmpty(value) ? "None" : value;
        }
    }

    private readonly struct CheckpointResult
    {
        public CheckpointResult(string name, string status, string detail)
        {
            Name = name;
            Status = status;
            Detail = detail;
        }

        public string Name { get; }
        public string Status { get; }
        public string Detail { get; }
    }

    private static object InvokePublicMethod(object target, string methodName, params object[] args)
    {
        return InvokeMethod(target, methodName, BindingFlags.Instance | BindingFlags.Public, args);
    }

    private static object InvokeNonPublicMethod(object target, string methodName, params object[] args)
    {
        return InvokeMethod(target, methodName, BindingFlags.Instance | BindingFlags.NonPublic, args);
    }

    private static object InvokeMethod(object target, string methodName, BindingFlags flags, params object[] args)
    {
        if (target == null)
        {
            throw new InvalidOperationException("Target was null for method " + methodName + ".");
        }

        MethodInfo method = target.GetType().GetMethod(methodName, flags);
        if (method == null)
        {
            throw new MissingMethodException(target.GetType().FullName, methodName);
        }

        return method.Invoke(target, args);
    }

    private static T GetPrivateField<T>(object target, string fieldName)
    {
        if (target == null)
        {
            return default;
        }

        FieldInfo field = target.GetType().GetField(fieldName, BindingFlags.Instance | BindingFlags.NonPublic);
        if (field == null)
        {
            throw new MissingFieldException(target.GetType().FullName, fieldName);
        }

        object value = field.GetValue(target);
        return value is T typedValue ? typedValue : (T)value;
    }

    private static void SetPrivateField(object target, string fieldName, object value)
    {
        if (target == null)
        {
            throw new InvalidOperationException("Target was null for field " + fieldName + ".");
        }

        FieldInfo field = target.GetType().GetField(fieldName, BindingFlags.Instance | BindingFlags.NonPublic);
        if (field == null)
        {
            throw new MissingFieldException(target.GetType().FullName, fieldName);
        }

        field.SetValue(target, value);
    }

    private static T GetFieldValue<T>(object target, string fieldName)
    {
        if (target == null)
        {
            return default;
        }

        FieldInfo field = target.GetType().GetField(fieldName, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
        if (field == null)
        {
            throw new MissingFieldException(target.GetType().FullName, fieldName);
        }

        object value = field.GetValue(target);
        return value is T typedValue ? typedValue : (T)value;
    }

    private static T GetPropertyValue<T>(object target, string propertyName)
    {
        if (target == null)
        {
            return default;
        }

        PropertyInfo property = target.GetType().GetProperty(propertyName, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
        if (property == null)
        {
            throw new MissingMemberException(target.GetType().FullName, propertyName);
        }

        object value = property.GetValue(target);
        return value is T typedValue ? typedValue : (T)value;
    }
}
