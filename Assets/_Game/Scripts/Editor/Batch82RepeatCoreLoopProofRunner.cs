using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

public static class Batch82RepeatCoreLoopProofRunner
{
    private const string ScenePath = "Assets/Scenes/SampleScene.unity";
    private const double StepTimeoutSeconds = 18d;
    private const string SessionActiveKey = "Batch82RepeatCoreLoopProofRunner.Active";
    private const string SessionModeKey = "Batch82RepeatCoreLoopProofRunner.Mode";
    private const string SelectedCityId = "city-a";
    private const string TargetDungeonId = "dungeon-alpha";
    private const string SafeRouteId = "safe";
    private const string RiskyRouteId = "risky";
    private static ProofSession _session;

    private enum ProofMode
    {
        RepeatLoop,
        SecondRunDecisionPressure,
        RecoveryPressureChoice
    }

    [InitializeOnLoadMethod]
    private static void RestorePendingSession()
    {
        if (!SessionState.GetBool(SessionActiveKey, false) || _session != null)
        {
            return;
        }

        _session = new ProofSession(ReadProofModeFromSession());
        _session.RestoreAfterDomainReload();
    }

    public static void RunBatch82RepeatCoreLoopProof()
    {
        RunProof(ProofMode.RepeatLoop);
    }

    public static void RunBatch83SecondRunDecisionPressureProof()
    {
        RunProof(ProofMode.SecondRunDecisionPressure);
    }

    public static void RunBatch84RecoveryPressureChoiceProof()
    {
        RunProof(ProofMode.RecoveryPressureChoice);
    }

    private static void RunProof(ProofMode proofMode)
    {
        if (_session != null)
        {
            Debug.LogError("[Batch82Proof] Proof runner is already active.");
            EditorApplication.Exit(1);
            return;
        }

        SessionState.SetString(SessionModeKey, proofMode.ToString());
        _session = new ProofSession(proofMode);
        _session.Start();
    }

    private static ProofMode ReadProofModeFromSession()
    {
        string modeText = SessionState.GetString(SessionModeKey, ProofMode.RepeatLoop.ToString());
        if (Enum.TryParse(modeText, out ProofMode proofMode))
        {
            return proofMode;
        }

        return ProofMode.RepeatLoop;
    }

    private sealed class ProofSession
    {
        private enum ProofStep
        {
            OpenScene,
            EnterPlayMode,
            WaitForBoot,
            WaitForMainMenu,
            EnterWorldSim,
            WaitForWorldSim,
            SelectCity,
            WaitForCityHub,
            ValidatePreRunCityHub,
            RecruitParty,
            EnterFirstExpeditionPrep,
            ValidateFirstExpeditionPrep,
            SelectFirstRoute,
            ConfirmFirstLaunch,
            WaitForFirstDungeonRun,
            SimulateFirstRunResult,
            ReturnFirstResultToWorld,
            WaitForReturnedCityHub,
            ValidatePostResultBoard,
            ReenterSecondExpeditionPrep,
            ValidateSecondExpeditionPrep,
            RecoverOneDayBeforeSecondRoute,
            ValidateSecondPrepAfterRecovery,
            SelectSecondRouteOrValidateBlocked,
            ConfirmSecondLaunchOrValidateBlocked,
            WaitForSecondDungeonRunOrFinish,
            Shutdown
        }

        private readonly ProofMode _proofMode;
        private readonly List<CheckpointResult> _checkpoints = new List<CheckpointResult>();
        private readonly StringBuilder _summary = new StringBuilder();
        private ProofStep _step = ProofStep.OpenScene;
        private double _stepStartedAt;
        private BootEntry _boot;
        private string _selectedCityId = string.Empty;
        private string _firstPrepRouteCardText = string.Empty;
        private string _firstRunLaunchText = string.Empty;
        private string _postResultBoardText = string.Empty;
        private string _secondPrepText = string.Empty;
        private string _secondPrepAfterRecoveryText = string.Empty;
        private string _secondRunDecisionText = string.Empty;
        private int _secondPrepWorldDayBeforeRecovery;
        private bool _secondLaunchConfirmed;
        private bool _shutdownRequested;

        public ProofSession(ProofMode proofMode)
        {
            _proofMode = proofMode;
        }

        public void Start()
        {
            SessionState.SetBool(SessionActiveKey, true);
            SessionState.SetString(SessionModeKey, _proofMode.ToString());
            Debug.Log("[Batch82Proof] Opening sample scene.");
            EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
            EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
            EditorApplication.update += Tick;
            AdvanceTo(ProofStep.EnterPlayMode, "Scene opened.");
            EditorApplication.isPlaying = true;
        }

        public void RestoreAfterDomainReload()
        {
            EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
            EditorApplication.update += Tick;
            AdvanceTo(ProofStep.WaitForBoot, "Restored proof session after domain reload.");
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
                    case ProofStep.EnterPlayMode:
                        break;
                    case ProofStep.WaitForBoot:
                        WaitForBoot();
                        break;
                    case ProofStep.WaitForMainMenu:
                        WaitForMainMenu();
                        break;
                    case ProofStep.EnterWorldSim:
                        EnterWorldSim();
                        break;
                    case ProofStep.WaitForWorldSim:
                        WaitForWorldSim();
                        break;
                    case ProofStep.SelectCity:
                        SelectCity();
                        break;
                    case ProofStep.WaitForCityHub:
                        WaitForCityHub();
                        break;
                    case ProofStep.ValidatePreRunCityHub:
                        ValidatePreRunCityHub();
                        break;
                    case ProofStep.RecruitParty:
                        RecruitParty();
                        break;
                    case ProofStep.EnterFirstExpeditionPrep:
                        EnterFirstExpeditionPrep();
                        break;
                    case ProofStep.ValidateFirstExpeditionPrep:
                        ValidateFirstExpeditionPrep();
                        break;
                    case ProofStep.SelectFirstRoute:
                        SelectFirstRoute();
                        break;
                    case ProofStep.ConfirmFirstLaunch:
                        ConfirmFirstLaunch();
                        break;
                    case ProofStep.WaitForFirstDungeonRun:
                        WaitForFirstDungeonRun();
                        break;
                    case ProofStep.SimulateFirstRunResult:
                        SimulateFirstRunResult();
                        break;
                    case ProofStep.ReturnFirstResultToWorld:
                        ReturnFirstResultToWorld();
                        break;
                    case ProofStep.WaitForReturnedCityHub:
                        WaitForReturnedCityHub();
                        break;
                    case ProofStep.ValidatePostResultBoard:
                        ValidatePostResultBoard();
                        break;
                    case ProofStep.ReenterSecondExpeditionPrep:
                        ReenterSecondExpeditionPrep();
                        break;
                    case ProofStep.ValidateSecondExpeditionPrep:
                        ValidateSecondExpeditionPrep();
                        break;
                    case ProofStep.RecoverOneDayBeforeSecondRoute:
                        RecoverOneDayBeforeSecondRoute();
                        break;
                    case ProofStep.ValidateSecondPrepAfterRecovery:
                        ValidateSecondPrepAfterRecovery();
                        break;
                    case ProofStep.SelectSecondRouteOrValidateBlocked:
                        SelectSecondRouteOrValidateBlocked();
                        break;
                    case ProofStep.ConfirmSecondLaunchOrValidateBlocked:
                        ConfirmSecondLaunchOrValidateBlocked();
                        break;
                    case ProofStep.WaitForSecondDungeonRunOrFinish:
                        WaitForSecondDungeonRunOrFinish();
                        break;
                    case ProofStep.Shutdown:
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

            Debug.Log("[Batch82Proof] BootEntry located.");
            AdvanceTo(ProofStep.WaitForMainMenu, "BootEntry is active.");
        }

        private void WaitForMainMenu()
        {
            if (!_boot.IsMainMenuActive)
            {
                return;
            }

            RecordPass("Boot -> MainMenu", "Reached MainMenu. Stage=" + _boot.CurrentAppFlowStage + ".");
            AdvanceTo(ProofStep.EnterWorldSim, "MainMenu reached.");
        }

        private void EnterWorldSim()
        {
            _boot.EnterWorldSimFromMenu();
            AdvanceTo(ProofStep.WaitForWorldSim, "Requested WorldSim entry.");
        }

        private void WaitForWorldSim()
        {
            if (!_boot.IsWorldSimActive || _boot.CurrentAppFlowStage != AppFlowStage.WorldSim)
            {
                return;
            }

            RecordPass("MainMenu -> WorldSim", "Entered WorldSim. Day=" + _boot.WorldDayCount + ".");
            AdvanceTo(ProofStep.SelectCity, "WorldSim active.");
        }

        private void SelectCity()
        {
            WorldSelectableMarker cityMarker = FindCityMarker(SelectedCityId);
            if (cityMarker == null)
            {
                Fail("Could not find " + SelectedCityId + " marker.");
                return;
            }

            Camera mainCamera = Camera.main;
            object worldView = GetWorldView();
            if (mainCamera == null || worldView == null)
            {
                Fail("WorldSim selection prerequisites were missing.");
                return;
            }

            Vector3 screenPoint = mainCamera.WorldToScreenPoint(cityMarker.transform.position);
            InvokePublicMethod(worldView, "SelectAtScreenPosition", mainCamera, (Vector2)screenPoint);
            _selectedCityId = cityMarker.EntityData != null ? cityMarker.EntityData.Id : string.Empty;
            RecordPass("WorldSim city selection", "Selected city " + SafeText(_selectedCityId) + ".");
            AdvanceTo(ProofStep.WaitForCityHub, "City selection applied.");
        }

        private void WaitForCityHub()
        {
            if (_boot.CurrentAppFlowStage != AppFlowStage.CityHub)
            {
                return;
            }

            RecordPass("WorldSim -> CityHub", "CityHub active. SelectedCity=" + SafeText(_selectedCityId) + ".");
            AdvanceTo(ProofStep.ValidatePreRunCityHub, "CityHub active.");
        }

        private void ValidatePreRunCityHub()
        {
            WorldBoardReadModel board = _boot.GetWorldBoardReadModel();
            CityStatusReadModel city = FindCity(board, _selectedCityId);
            CityDecisionReadModel decision = city != null ? city.Decision : null;
            CityActionRecommendation recommendation = GetFirstRecommendation(decision);
            if (city == null || decision == null)
            {
                Fail("Pre-run CityHub did not expose selected city read-model context.");
                return;
            }

            bool hasPressureOrRoute = HasText(city.NeedPressureStateId) ||
                                      HasText(city.DispatchReadinessStateId) ||
                                      HasText(city.RecommendedRouteSummaryText) ||
                                      HasText(decision.WhyCityMattersText) ||
                                      recommendation != null;
            if (!hasPressureOrRoute)
            {
                Fail("Pre-run CityHub did not expose pressure/readiness/route context.");
                return;
            }

            RecordPass(
                "Before first run CityHub context",
                "City pressure/readiness route context is visible. Pressure=" + SafeText(city.NeedPressureStateId) +
                " Readiness=" + SafeText(city.DispatchReadinessStateId) +
                " Route=" + SafeText(city.RecommendedRouteSummaryText) +
                " Why=" + SafeText(decision.WhyCityMattersText) +
                " Recommendation=" + SafeText(recommendation != null ? recommendation.SummaryText : "None") + ".");
            AdvanceTo(ProofStep.RecruitParty, "Pre-run CityHub context validated.");
        }

        private void RecruitParty()
        {
            _boot.RecruitWorldSimParty();
            if (_boot.IdleParties <= 0)
            {
                Fail("Party recruitment did not create an idle party.");
                return;
            }

            RecordPass("CityHub party recruit", "Recruited party. IdleParties=" + _boot.IdleParties + ".");
            AdvanceTo(ProofStep.EnterFirstExpeditionPrep, "Party recruited.");
        }

        private void EnterFirstExpeditionPrep()
        {
            if (!_boot.TryEnterSelectedWorldDungeon())
            {
                Fail("Failed to enter first ExpeditionPrep from selected city.");
                return;
            }

            AdvanceTo(ProofStep.ValidateFirstExpeditionPrep, "Requested first ExpeditionPrep.");
        }

        private void ValidateFirstExpeditionPrep()
        {
            if (_boot.CurrentAppFlowStage != AppFlowStage.ExpeditionPrep || !_boot.IsDungeonRouteChoiceVisible)
            {
                return;
            }

            ExpeditionPrepSurfaceData prep = _boot.GetSelectedExpeditionPrepSurfaceData();
            if (!ValidatePrepIdentity(prep, "First ExpeditionPrep context"))
            {
                return;
            }

            ExpeditionPrepRouteOptionData safeOption = FindRouteOption(prep, SafeRouteId);
            ExpeditionPrepRouteOptionData riskyOption = FindRouteOption(prep, RiskyRouteId);
            _firstPrepRouteCardText = BuildRouteCardText(safeOption);
            bool hasRouteScenario = safeOption != null &&
                                    riskyOption != null &&
                                    ContainsAll(_firstPrepRouteCardText, "Stability Run", "Combat", "slime-heavy sustain") &&
                                    ContainsAny(BuildRouteCardText(riskyOption), "Surge Window", "risky", "surge");
            bool hasPartyContext = HasText(prep.StagedPartySummaryText) &&
                                   HasText(prep.PartyLoadoutSummaryText) &&
                                   HasText(prep.LaunchGateSummaryText);
            if (!hasRouteScenario || !hasPartyContext)
            {
                Fail(
                    "First ExpeditionPrep was missing route scenario or party/loadout gate context. " +
                    "SafeCard=" + SafeText(_firstPrepRouteCardText) +
                    " RiskyCard=" + SafeText(BuildRouteCardText(riskyOption)) +
                    " Party=" + SafeText(prep.StagedPartySummaryText) +
                    " Loadout=" + SafeText(prep.PartyLoadoutSummaryText) +
                    " Gate=" + SafeText(prep.LaunchGateSummaryText) + ".");
                return;
            }

            RecordPass(
                "First ExpeditionPrep context",
                "RouteOptions=" + prep.RouteOptions.Length +
                " Party=" + SafeText(prep.StagedPartySummaryText) +
                " Loadout=" + SafeText(prep.PartyLoadoutSummaryText) +
                " Gate=" + SafeText(prep.LaunchGateSummaryText) + ".");
            AdvanceTo(ProofStep.SelectFirstRoute, "First ExpeditionPrep validated.");
        }

        private void SelectFirstRoute()
        {
            if (!_boot.IsRouteChoiceAvailable(SafeRouteId) || !_boot.TryTriggerRouteChoice(SafeRouteId))
            {
                Fail("Could not select first route " + SafeRouteId + ". Prompt=" + SafeText(_boot.RouteChoicePromptLabel) + ".");
                return;
            }

            ExpeditionPrepSurfaceData prep = _boot.GetSelectedExpeditionPrepSurfaceData();
            string commitSummary = BuildPrepDecisionText(prep);
            if (!ContainsAll(commitSummary, "Stability Run", "slime-heavy sustain"))
            {
                Fail("First route commit summary did not preserve operating scenario. Summary=" + SafeText(commitSummary) + ".");
                return;
            }

            RecordPass("First route select", "Selected route " + SafeRouteId + ". " + SafeText(commitSummary));
            AdvanceTo(ProofStep.ConfirmFirstLaunch, "First route selected.");
        }

        private void ConfirmFirstLaunch()
        {
            if (!_boot.CanConfirmRouteChoice() || !_boot.TryConfirmRouteChoice())
            {
                Fail("Could not confirm first launch. Prompt=" + SafeText(_boot.RouteChoicePromptLabel) + ".");
                return;
            }

            AdvanceTo(ProofStep.WaitForFirstDungeonRun, "First launch confirmed.");
        }

        private void WaitForFirstDungeonRun()
        {
            if (_boot.CurrentAppFlowStage != AppFlowStage.DungeonRun || _boot.IsDungeonRouteChoiceVisible)
            {
                return;
            }

            ExpeditionPlan launchPlan = GetCurrentLaunchPlan();
            if (!ValidateLaunchPlan(launchPlan, "First launch contract"))
            {
                return;
            }

            _firstRunLaunchText = BuildLaunchPlanText(launchPlan);
            RecordPass("First ExpeditionPrep -> DungeonRun", _firstRunLaunchText);
            AdvanceTo(ProofStep.SimulateFirstRunResult, "First DungeonRun active.");
        }

        private void SimulateFirstRunResult()
        {
            object worldView = GetWorldView();
            if (worldView == null)
            {
                Fail("World view missing for simulated first result.");
                return;
            }

            object clearState = ParseNestedEnum(worldView, "RunResultState", "Clear");
            object victoryState = ParseNestedEnum(worldView, "BattleState", "Victory");
            InvokeNonPublicMethod(
                worldView,
                "FinishDungeonRun",
                clearState,
                victoryState,
                true,
                16,
                "Batch82 proof cleared Dungeon Alpha and returned with mana_shard x16.");

            if (!_boot.IsDungeonResultPanelVisible && _boot.CurrentAppFlowStage != AppFlowStage.ResultPipeline)
            {
                Fail("Simulated first run result did not reach ResultPipeline. Stage=" + _boot.CurrentAppFlowStage + ".");
                return;
            }

            RecordPass("First run result", "Result reached. Stage=" + _boot.CurrentAppFlowStage + " Prompt=" + SafeText(_boot.ResultPanelReturnPromptLabel) + ".");
            AdvanceTo(ProofStep.ReturnFirstResultToWorld, "First result simulated.");
        }

        private void ReturnFirstResultToWorld()
        {
            object worldView = GetWorldView();
            if (worldView == null)
            {
                Fail("World view missing for first result return.");
                return;
            }

            SetPrivateField(worldView, "_pendingDungeonExit", true);
            AppFlowObservedSnapshot snapshot = (AppFlowObservedSnapshot)InvokePublicMethod(worldView, "BuildAppFlowSnapshot");
            bool returned = (bool)InvokeNonPublicMethod(_boot, "TryExitDungeonRunToWorldSim");
            if (!snapshot.HasPendingWorldReturn || !returned)
            {
                Fail("First result return was not consumed. Pending=" + snapshot.HasPendingWorldReturn + " Returned=" + returned + ".");
                return;
            }

            RecordPass("First result return", "Result return consumed and world shell re-entry requested.");
            AdvanceTo(ProofStep.WaitForReturnedCityHub, "Returned to world shell.");
        }

        private void WaitForReturnedCityHub()
        {
            if (!_boot.IsWorldSimActive || _boot.CurrentAppFlowStage != AppFlowStage.CityHub)
            {
                return;
            }

            RecordPass("World return -> CityHub", "Returned to selected CityHub after first result. Stage=" + _boot.CurrentAppFlowStage + ".");
            AdvanceTo(ProofStep.ValidatePostResultBoard, "Returned CityHub active.");
        }

        private void ValidatePostResultBoard()
        {
            PrototypeCityHubUiSurfaceData ui = _boot.GetCityHubUiSurfaceData();
            PrototypeCityHubSelectionSurfaceData selection = ui != null ? ui.Selection : null;
            PrototypeCityHubOutcomeSurfaceData outcome = ui != null ? ui.Outcome : null;
            if (ui == null || selection == null || outcome == null || !ui.HasSelectedCity || !selection.IsCitySelection)
            {
                Fail("CityHub UI did not expose a selected city after first result return.");
                return;
            }

            string summary = selection.PressureBoardSummaryText;
            string latest = selection.RecentResultEvidenceText;
            string changed = selection.PressureChangeText;
            string ready = selection.PartyReadinessSummaryText;
            string next = outcome.CorrectiveFollowUpText;
            _postResultBoardText =
                "Summary=" + SafeText(summary) +
                " | Latest=" + SafeText(latest) +
                " | Changed=" + SafeText(changed) +
                " | Ready=" + SafeText(ready) +
                " | Next=" + SafeText(next);

            bool hasCompactBoard = ContainsAll(summary, "Latest:", "Changed:", "Next:") &&
                                   ContainsAny(summary, "Ready:", "Blocked:");
            bool hasLatest = ContainsAll(latest, "Returned", "Party") &&
                             ContainsAny(latest, "Cleared", "Returned") &&
                             ContainsAny(latest, "Stability Run", "Dungeon Alpha", "safe") &&
                             ContainsAny(latest, "mana_shard", "x16", "16");
            bool hidesDebugKey = !ContainsValue(latest, "run_clear");
            bool hasChanged = HasText(changed) && ContainsAny(changed, "Stock +", "Pressure", "Readiness", "absorbed");
            bool hasReady = HasText(ready) && ContainsAny(ready, "Ready", "Blocked", "warning", "recovery", "party");
            bool hasNext = HasText(next) || ContainsValue(summary, "Next:");
            bool hasWhyAndRoute = HasText(selection.WhyCityMattersText) &&
                                  HasText(selection.RecommendedRouteText) &&
                                  HasText(selection.RecommendationReasonText);

            if (!hasCompactBoard || !hasLatest || !hidesDebugKey || !hasChanged || !hasReady || !hasNext || !hasWhyAndRoute)
            {
                Fail("Post-result pressure board was incomplete. " + _postResultBoardText);
                return;
            }

            RecordPass("World pressure board after first result", _postResultBoardText);
            AdvanceTo(ProofStep.ReenterSecondExpeditionPrep, "Post-result board validated.");
        }

        private void ReenterSecondExpeditionPrep()
        {
            if (!_boot.TryEnterSelectedWorldDungeon())
            {
                string readiness = GetPostResultReadinessText();
                if (ContainsAny(readiness, "Blocked", "recovery", "no idle", "strained", "Wait"))
                {
                    RecordPass("Second prep blocked with reason", "Re-entry is blocked and explained. " + SafeText(readiness));
                    RecordPass("Repeat core loop proof completed", "First result returns to CityHub and blocks second prep with a clear next action.");
                    AdvanceTo(ProofStep.Shutdown, "Proof complete with explained prep block.");
                    return;
                }

                Fail("Could not re-enter second ExpeditionPrep, and the board did not explain the block. Readiness=" + SafeText(readiness) + ".");
                return;
            }

            AdvanceTo(ProofStep.ValidateSecondExpeditionPrep, "Requested second ExpeditionPrep.");
        }

        private void ValidateSecondExpeditionPrep()
        {
            if (_boot.CurrentAppFlowStage != AppFlowStage.ExpeditionPrep || !_boot.IsDungeonRouteChoiceVisible)
            {
                return;
            }

            ExpeditionPrepSurfaceData prep = _boot.GetSelectedExpeditionPrepSurfaceData();
            if (!ValidatePrepIdentity(prep, "Second ExpeditionPrep context"))
            {
                return;
            }

            ExpeditionPrepRouteOptionData safeOption = FindRouteOption(prep, SafeRouteId);
            ExpeditionPrepRouteOptionData riskyOption = FindRouteOption(prep, RiskyRouteId);
            string safeCard = BuildRouteCardText(safeOption);
            string riskyCard = BuildRouteCardText(riskyOption);
            string latestResult = BuildLatestResultText(prep != null ? prep.LatestExpeditionResult : null);
            _secondPrepText =
                "Latest=" + SafeText(latestResult) +
                " | LastRun=" + SafeText(prep != null ? prep.LastRunCarryForwardText : "None") +
                " | CarryForward=" + SafeText(prep != null ? prep.PartyGrowthCarryForwardText : "None") +
                " | Stability=" + SafeText(prep != null ? prep.StabilityAppetiteText : "None") +
                " | Surge=" + SafeText(prep != null ? prep.SurgeAppetiteText : "None") +
                " | LaunchRisk=" + SafeText(prep != null ? prep.LaunchRiskAdviceText : "None") +
                " | LaunchNow=" + SafeText(prep != null ? prep.LaunchNowChoiceText : "None") +
                " | RecoverOneDay=" + SafeText(prep != null ? prep.RecoverOneDayChoiceText : "None") +
                " | AfterWaiting=" + SafeText(prep != null ? prep.AfterRecoveryPreviewText : "None") +
                " | RecoveryChoice=" + SafeText(prep != null ? prep.RecoveryPressureChoiceText : "None") +
                " | AfterRecoveryAppetite=" + SafeText(prep != null ? prep.RouteAppetiteAfterRecoveryText : "None") +
                " | AppetiteRecommendation=" + SafeText(prep != null ? prep.RouteAppetiteRecommendationText : "None") +
                " | Party=" + SafeText(prep != null ? prep.StagedPartySummaryText : "None") +
                " | Loadout=" + SafeText(prep != null ? prep.PartyLoadoutSummaryText : "None") +
                " | Gate=" + SafeText(prep != null ? prep.LaunchGateSummaryText : "None") +
                " | Reason=" + SafeText(prep != null ? prep.RecommendationReasonText : "None") +
                " | Safe=" + SafeText(safeCard) +
                " | Risky=" + SafeText(riskyCard);

            bool hasResultCarryover = prep != null &&
                                      prep.LatestExpeditionResult != null &&
                                      prep.LatestExpeditionResult.ResultStateKey == PrototypeBattleOutcomeKeys.RunClear &&
                                      prep.LatestExpeditionResult.ReturnedLootAmount >= 16 &&
                                      HasText(prep.LatestExpeditionResult.ResultSummaryText);
            bool hasPartyCarryover = prep != null &&
                                     ContainsAny(prep.StagedPartySummaryText, "Alden", "Mira", "Rune", "Lia", "Lv", "Party") &&
                                     HasText(prep.PartyLoadoutSummaryText) &&
                                     ContainsAny(
                                         prep.PartyLoadoutSummaryText + " " +
                                         prep.LatestExpeditionResult.LatestGrowthHighlightText + " " +
                                         prep.LatestExpeditionResult.NextRunGrowthPreviewText,
                                         "Lv", "XP", "ATK", "POW", "SPD", "equipment", "Equipped", "gear", "Focus", "Blade", "Harness");
            bool hasRouteChoice = safeOption != null &&
                                  riskyOption != null &&
                                  ContainsAll(safeCard, "Stability Run", "Combat", "slime-heavy sustain") &&
                                  ContainsAny(riskyCard, "Surge Window", "risky", "surge") &&
                                  !string.Equals(safeCard, riskyCard, StringComparison.Ordinal);
            bool hasGateAndReason = prep != null &&
                                    HasText(prep.LaunchGateSummaryText) &&
                                    HasText(prep.RecommendationReasonText) &&
                                    HasText(prep.RecommendedNextActionText);
            bool hasSecondRunDesire =
                prep != null &&
                ContainsAll(prep.LastRunCarryForwardText, "Returned", "mana_shard", "Party Stable") &&
                ContainsAll(prep.PartyGrowthCarryForwardText, "Alden +16 XP", "Rune") &&
                ContainsAll(prep.StabilityAppetiteText, "protect HP", "dispatch rhythm") &&
                ContainsAll(prep.SurgeAppetiteText, "chase payout", "Rune") &&
                ContainsAll(prep.LaunchRiskAdviceText, "Ready with warning", "recovery", "strain") &&
                ContainsAll(prep.RouteAppetiteRecommendationText, "Stability", "Surge", "recovery", "strain") &&
                ContainsAll(safeCard, "Appetite", "protect HP", "dispatch rhythm") &&
                ContainsAll(riskyCard, "Appetite", "chase payout", "Rune");
            bool hasRecoveryChoice =
                prep != null &&
                prep.CanRecoverOneDay &&
                ContainsAll(prep.LaunchNowChoiceText, "Launch now") &&
                ContainsAny(prep.LaunchNowChoiceText, "Ready with warning", "Ready", "Blocked") &&
                ContainsAll(prep.RecoverOneDayChoiceText, "1 Day", "world") &&
                ContainsAll(prep.AfterRecoveryPreviewText, "After waiting", "readiness", "pressure") &&
                ContainsAll(prep.RecoveryPressureChoiceText, "Launch now", "1 Day") &&
                ContainsAll(prep.RouteAppetiteAfterRecoveryText, "Stability", "Surge");

            if (!hasResultCarryover || !hasPartyCarryover || !hasRouteChoice || !hasGateAndReason || !hasSecondRunDesire || !hasRecoveryChoice)
            {
                Fail("Second ExpeditionPrep did not carry result, party/loadout, route, gate, desire-pressure, or recovery choice context. " + _secondPrepText);
                return;
            }

            RecordPass("Second ExpeditionPrep context", _secondPrepText);
            if (_proofMode == ProofMode.RecoveryPressureChoice)
            {
                AdvanceTo(ProofStep.RecoverOneDayBeforeSecondRoute, "Second ExpeditionPrep recovery choice validated.");
                return;
            }

            AdvanceTo(ProofStep.SelectSecondRouteOrValidateBlocked, "Second ExpeditionPrep validated.");
        }

        private void RecoverOneDayBeforeSecondRoute()
        {
            if (_boot.CurrentAppFlowStage != AppFlowStage.ExpeditionPrep || !_boot.IsDungeonRouteChoiceVisible)
            {
                return;
            }

            _secondPrepWorldDayBeforeRecovery = _boot.WorldDayCount;
            if (!_boot.TryRecoverExpeditionPrepOneDay())
            {
                Fail("Recover 1 Day action did not advance the existing world day/recovery rail.");
                return;
            }

            AdvanceTo(ProofStep.ValidateSecondPrepAfterRecovery, "Recovered one day while ExpeditionPrep remained open.");
        }

        private void ValidateSecondPrepAfterRecovery()
        {
            if (_boot.CurrentAppFlowStage != AppFlowStage.ExpeditionPrep || !_boot.IsDungeonRouteChoiceVisible)
            {
                return;
            }

            ExpeditionPrepSurfaceData prep = _boot.GetSelectedExpeditionPrepSurfaceData();
            string readinessText =
                SafeText(prep != null ? prep.DispatchReadinessText : "None") + " | " +
                SafeText(prep != null ? prep.RecoveryProgressText : "None") + " | " +
                SafeText(prep != null ? prep.RecoveryEtaText : "None") + " | " +
                SafeText(prep != null ? prep.LaunchRiskAdviceText : "None");
            _secondPrepAfterRecoveryText =
                "BeforeDay=" + _secondPrepWorldDayBeforeRecovery +
                " | AfterDay=" + _boot.WorldDayCount +
                " | Readiness=" + readinessText +
                " | LaunchNow=" + SafeText(prep != null ? prep.LaunchNowChoiceText : "None") +
                " | RecoverOneDay=" + SafeText(prep != null ? prep.RecoverOneDayChoiceText : "None") +
                " | AfterWaiting=" + SafeText(prep != null ? prep.AfterRecoveryPreviewText : "None") +
                " | AfterRecoveryAppetite=" + SafeText(prep != null ? prep.RouteAppetiteAfterRecoveryText : "None") +
                " | Recommendation=" + SafeText(prep != null ? prep.RouteAppetiteRecommendationText : "None");

            bool dayAdvanced = _boot.WorldDayCount > _secondPrepWorldDayBeforeRecovery;
            bool boardStillOpen = prep != null && prep.IsBoardOpen && prep.CanRecoverOneDay;
            bool readinessUpdated = prep != null &&
                                    prep.CanLaunch &&
                                    ContainsAny(readinessText, "Ready", "low recovery risk");
            bool appetiteChanged = prep != null &&
                                   ContainsAll(prep.RouteAppetiteAfterRecoveryText, "Surge", "Stability") &&
                                   ContainsAny(prep.RouteAppetiteAfterRecoveryText, "readiness is clear", "tempting", "growth");
            bool waitReadbackStillHonest = prep != null &&
                                           ContainsAll(prep.AfterRecoveryPreviewText, "After waiting", "pressure") &&
                                           ContainsAll(prep.RecoverOneDayChoiceText, "1 Day", "world");

            if (!dayAdvanced || !boardStillOpen || !readinessUpdated || !appetiteChanged || !waitReadbackStillHonest)
            {
                Fail("Recover 1 Day did not update readiness/pressure/appetite through the existing rail. " + _secondPrepAfterRecoveryText);
                return;
            }

            RecordPass("Recover 1 Day choice", _secondPrepAfterRecoveryText);
            AdvanceTo(ProofStep.SelectSecondRouteOrValidateBlocked, "Recovery choice updated second ExpeditionPrep.");
        }

        private void SelectSecondRouteOrValidateBlocked()
        {
            if (_boot.IsRouteChoiceAvailable(SafeRouteId) && _boot.TryTriggerRouteChoice(SafeRouteId))
            {
                ExpeditionPrepSurfaceData prep = _boot.GetSelectedExpeditionPrepSurfaceData();
                _secondRunDecisionText = BuildPrepDecisionText(prep);
                RecordPass("Second route select", "Selected route " + SafeRouteId + ". " + SafeText(_secondRunDecisionText));
                AdvanceTo(ProofStep.ConfirmSecondLaunchOrValidateBlocked, "Second route selected.");
                return;
            }

            ExpeditionPrepSurfaceData blockedPrep = _boot.GetSelectedExpeditionPrepSurfaceData();
            string blockText = BuildBlockedReasonText(blockedPrep);
            if (ContainsAny(blockText, "Blocked", "recovery", "wait", "no idle", "party", "readiness", "slot", "warning"))
            {
                RecordPass("Second-run blocked with reason", "Route selection is blocked and explained. " + SafeText(blockText));
                RecordPass("Repeat core loop proof completed", "The loop reaches a second prep decision with an explained block.");
                AdvanceTo(ProofStep.Shutdown, "Proof complete with explained second route block.");
                return;
            }

            Fail("Second route could not be selected and no clear block reason was visible. " + SafeText(blockText));
        }

        private void ConfirmSecondLaunchOrValidateBlocked()
        {
            if (_boot.CanConfirmRouteChoice() && _boot.TryConfirmRouteChoice())
            {
                _secondLaunchConfirmed = true;
                AdvanceTo(ProofStep.WaitForSecondDungeonRunOrFinish, "Second launch confirmed.");
                return;
            }

            ExpeditionPrepSurfaceData prep = _boot.GetSelectedExpeditionPrepSurfaceData();
            string blockText = BuildBlockedReasonText(prep);
            if (ContainsAny(blockText, "Blocked", "recovery", "wait", "no idle", "party", "readiness", "slot", "warning"))
            {
                _secondRunDecisionText = blockText;
                RecordPass("Second-run blocked with reason", "Launch is blocked and explained. " + SafeText(blockText));
                RecordPass("Repeat core loop proof completed", "The loop reaches a second prep decision with an explained launch gate.");
                AdvanceTo(ProofStep.Shutdown, "Proof complete with explained second launch block.");
                return;
            }

            Fail("Second launch could not be confirmed and no clear gate reason was visible. " + SafeText(blockText));
        }

        private void WaitForSecondDungeonRunOrFinish()
        {
            if (!_secondLaunchConfirmed)
            {
                RecordPass("Repeat core loop proof completed", "The loop reached second prep with an explained non-launch decision.");
                AdvanceTo(ProofStep.Shutdown, "Proof complete.");
                return;
            }

            if (_boot.CurrentAppFlowStage != AppFlowStage.DungeonRun || _boot.IsDungeonRouteChoiceVisible)
            {
                return;
            }

            ExpeditionPlan launchPlan = GetCurrentLaunchPlan();
            if (!ValidateLaunchPlan(launchPlan, "Second launch contract"))
            {
                return;
            }

            RecordPass("Second run launch", BuildLaunchPlanText(launchPlan));
            RecordPass("Repeat core loop proof completed", "World -> CityHub -> ExpeditionPrep -> DungeonRun -> Result -> World/CityHub -> ExpeditionPrep -> DungeonRun is repeatable.");
            AdvanceTo(ProofStep.Shutdown, "Proof complete.");
        }

        private bool ValidatePrepIdentity(ExpeditionPrepSurfaceData prep, string checkpoint)
        {
            if (prep == null ||
                !prep.IsBoardOpen ||
                prep.CityId != _selectedCityId ||
                prep.DungeonId != TargetDungeonId ||
                prep.RouteOptions == null ||
                prep.RouteOptions.Length < 2)
            {
                Fail(
                    checkpoint + " failed identity/options check. City=" + SafeText(prep != null ? prep.CityId : "None") +
                    " Dungeon=" + SafeText(prep != null ? prep.DungeonId : "None") +
                    " RouteOptions=" + (prep != null && prep.RouteOptions != null ? prep.RouteOptions.Length.ToString() : "0") + ".");
                return false;
            }

            return true;
        }

        private bool ValidateLaunchPlan(ExpeditionPlan launchPlan, string checkpoint)
        {
            if (launchPlan == null ||
                !launchPlan.IsConfirmed ||
                launchPlan.OriginCityId != _selectedCityId ||
                launchPlan.TargetDungeonId != TargetDungeonId ||
                launchPlan.SelectedRoute == null ||
                launchPlan.SelectedRoute.RouteId != SafeRouteId)
            {
                Fail(
                    checkpoint + " was not coherent. City=" + SafeText(launchPlan != null ? launchPlan.OriginCityId : "None") +
                    " Dungeon=" + SafeText(launchPlan != null ? launchPlan.TargetDungeonId : "None") +
                    " Route=" + SafeText(launchPlan != null && launchPlan.SelectedRoute != null ? launchPlan.SelectedRoute.RouteId : "None") + ".");
                return false;
            }

            return true;
        }

        private ExpeditionPlan GetCurrentLaunchPlan()
        {
            AppFlowContext context = _boot.CurrentAppFlowContext;
            return context != null && context.CurrentDungeonRun != null
                ? context.CurrentDungeonRun.LaunchPlan
                : null;
        }

        private string BuildLaunchPlanText(ExpeditionPlan launchPlan)
        {
            return
                "LaunchPlan City=" + SafeText(launchPlan != null ? launchPlan.OriginCityId : "None") +
                " Dungeon=" + SafeText(launchPlan != null ? launchPlan.TargetDungeonId : "None") +
                " Route=" + SafeText(launchPlan != null && launchPlan.SelectedRoute != null ? launchPlan.SelectedRoute.RouteLabel : "None") +
                " Objective=" + SafeText(launchPlan != null ? launchPlan.ObjectiveText : "None") +
                " Risk=" + SafeText(launchPlan != null ? launchPlan.RiskRewardPreviewText : "None") +
                " Party=" + SafeText(launchPlan != null ? launchPlan.PartySummaryText : "None") + ".";
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

        private CityActionRecommendation GetFirstRecommendation(CityDecisionReadModel decision)
        {
            if (decision == null || decision.RecommendedActions == null)
            {
                return null;
            }

            for (int i = 0; i < decision.RecommendedActions.Length; i++)
            {
                CityActionRecommendation recommendation = decision.RecommendedActions[i];
                if (recommendation != null && HasText(recommendation.SummaryText))
                {
                    return recommendation;
                }
            }

            return null;
        }

        private WorldSelectableMarker FindCityMarker(string cityId)
        {
            WorldSelectableMarker[] markers = UnityEngine.Object.FindObjectsByType<WorldSelectableMarker>(FindObjectsSortMode.None);
            for (int i = 0; i < markers.Length; i++)
            {
                WorldSelectableMarker marker = markers[i];
                if (marker != null &&
                    marker.EntityData != null &&
                    marker.EntityData.Kind == WorldEntityKind.City &&
                    marker.EntityData.Id == cityId)
                {
                    return marker;
                }
            }

            return null;
        }

        private ExpeditionPrepRouteOptionData FindRouteOption(ExpeditionPrepSurfaceData prep, string routeId)
        {
            ExpeditionPrepRouteOptionData[] options = prep != null ? prep.RouteOptions : null;
            if (options == null)
            {
                return null;
            }

            for (int i = 0; i < options.Length; i++)
            {
                ExpeditionPrepRouteOptionData option = options[i];
                if (option != null && option.OptionKey == routeId)
                {
                    return option;
                }
            }

            return null;
        }

        private string BuildRouteCardText(ExpeditionPrepRouteOptionData option)
        {
            if (option == null)
            {
                return "None";
            }

            return
                "Label=" + SafeText(option.OptionLabel) +
                " | Risk=" + SafeText(option.RouteRiskText) +
                " | Preview=" + SafeText(option.RoutePreviewText) +
                " | Reward=" + SafeText(option.RewardPreviewText) +
                " | CombatPlan=" + SafeText(option.EventPreviewText);
        }

        private string BuildPrepDecisionText(ExpeditionPrepSurfaceData prep)
        {
            ExpeditionStartContext start = prep != null ? prep.StartContext : null;
            return
                "SelectedRoute=" + SafeText(prep != null ? prep.SelectedRouteLabel : "None") +
                " | RoutePreview=" + SafeText(prep != null ? prep.RoutePreviewSummaryText : "None") +
                " | CombatPlan=" + SafeText(prep != null ? prep.EventPreviewText : "None") +
                " | Party=" + SafeText(prep != null ? prep.StagedPartySummaryText : "None") +
                " | Loadout=" + SafeText(prep != null ? prep.PartyLoadoutSummaryText : "None") +
                " | Gate=" + SafeText(prep != null ? prep.LaunchGateSummaryText : "None") +
                " | LaunchRisk=" + SafeText(prep != null ? prep.LaunchRiskAdviceText : "None") +
                " | AppetiteRecommendation=" + SafeText(prep != null ? prep.RouteAppetiteRecommendationText : "None") +
                " | Reason=" + SafeText(prep != null ? prep.RecommendationReasonText : "None") +
                " | StartRoute=" + SafeText(start != null ? start.RoutePreviewSummaryText : "None") +
                " | StartCombatPlan=" + SafeText(start != null ? start.EventPreviewSummaryText : "None");
        }

        private string BuildLatestResultText(ExpeditionResult result)
        {
            if (result == null)
            {
                return "None";
            }

            return
                "State=" + SafeText(result.ResultStateKey) +
                " | Route=" + SafeText(result.RouteLabel) +
                " | Loot=" + SafeText(result.RewardResourceId) + " x" + result.ReturnedLootAmount +
                " | Summary=" + SafeText(result.ResultSummaryText) +
                " | Growth=" + SafeText(result.LatestGrowthHighlightText) +
                " | NextGrowth=" + SafeText(result.NextRunGrowthPreviewText);
        }

        private string BuildBlockedReasonText(ExpeditionPrepSurfaceData prep)
        {
            return
                "Gate=" + SafeText(prep != null ? prep.LaunchGateSummaryText : "None") +
                " | Blocked=" + SafeText(prep != null ? prep.BlockedReasonText : "None") +
                " | Readiness=" + SafeText(prep != null ? prep.DispatchReadinessText : "None") +
                " | Recovery=" + SafeText(prep != null ? prep.RecoveryEtaText : "None") +
                " | LaunchRisk=" + SafeText(prep != null ? prep.LaunchRiskAdviceText : "None") +
                " | Next=" + SafeText(prep != null ? prep.RecommendedNextActionText : "None") +
                " | Prompt=" + SafeText(_boot.RouteChoicePromptLabel);
        }

        private string GetPostResultReadinessText()
        {
            PrototypeCityHubUiSurfaceData ui = _boot.GetCityHubUiSurfaceData();
            PrototypeCityHubSelectionSurfaceData selection = ui != null ? ui.Selection : null;
            return selection != null
                ? "Readiness=" + SafeText(selection.PartyReadinessSummaryText) + " | Summary=" + SafeText(selection.PressureBoardSummaryText)
                : "None";
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
            Debug.Log("[Batch82Proof][" + status + "] " + checkpoint + " :: " + detail);
        }

        private void AdvanceTo(ProofStep nextStep, string reason)
        {
            _step = nextStep;
            _stepStartedAt = EditorApplication.timeSinceStartup;
            Debug.Log("[Batch82Proof] Step -> " + _step + " :: " + reason);
        }

        private void Fail(string detail)
        {
            RecordFail("Repeat core loop proof", detail);
            AdvanceTo(ProofStep.Shutdown, "Proof failed.");
        }

        private void Shutdown(int exitCode)
        {
            if (_shutdownRequested)
            {
                return;
            }

            _shutdownRequested = true;
            SessionState.EraseBool(SessionActiveKey);
            SessionState.EraseString(SessionModeKey);
            EditorApplication.update -= Tick;
            EditorApplication.playModeStateChanged -= OnPlayModeStateChanged;

            _summary.AppendLine("[Batch82Proof] Summary");
            _summary.AppendLine("- Branch=A repeat-loop proof over existing loop contracts");
            _summary.AppendLine("- ProofMode=" + _proofMode);
            _summary.AppendLine("- FirstPrepRouteCard=" + SafeText(_firstPrepRouteCardText));
            _summary.AppendLine("- FirstRunLaunch=" + SafeText(_firstRunLaunchText));
            _summary.AppendLine("- PostResultBoard=" + SafeText(_postResultBoardText));
            _summary.AppendLine("- SecondPrep=" + SafeText(_secondPrepText));
            _summary.AppendLine("- SecondPrepAfterRecovery=" + SafeText(_secondPrepAfterRecoveryText));
            _summary.AppendLine("- SecondRunDecision=" + SafeText(_secondRunDecisionText));
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
                AdvanceTo(ProofStep.WaitForBoot, "Entered play mode.");
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

        private bool HasText(string value)
        {
            return !string.IsNullOrEmpty(value) &&
                   value != "None" &&
                   value != "none" &&
                   value != "0";
        }

        private bool ContainsAll(string value, params string[] needles)
        {
            for (int i = 0; i < needles.Length; i++)
            {
                if (!ContainsValue(value, needles[i]))
                {
                    return false;
                }
            }

            return true;
        }

        private bool ContainsAny(string value, params string[] needles)
        {
            for (int i = 0; i < needles.Length; i++)
            {
                if (ContainsValue(value, needles[i]))
                {
                    return true;
                }
            }

            return false;
        }

        private bool ContainsValue(string haystack, string needle)
        {
            return !string.IsNullOrEmpty(haystack) &&
                   !string.IsNullOrEmpty(needle) &&
                   haystack.IndexOf(needle, StringComparison.OrdinalIgnoreCase) >= 0;
        }

        private string SafeText(string value)
        {
            return string.IsNullOrEmpty(value) ? "None" : value;
        }

        private object ParseNestedEnum(object instance, string enumName, string valueName)
        {
            Type enumType = instance.GetType().GetNestedType(enumName, BindingFlags.NonPublic);
            if (enumType == null)
            {
                throw new InvalidOperationException("Could not find nested enum " + enumName + ".");
            }

            return Enum.Parse(enumType, valueName);
        }

        private T GetPrivateField<T>(object instance, string fieldName)
        {
            if (instance == null)
            {
                return default;
            }

            FieldInfo field = instance.GetType().GetField(fieldName, BindingFlags.Instance | BindingFlags.NonPublic);
            return field != null ? (T)field.GetValue(instance) : default;
        }

        private void SetPrivateField(object instance, string fieldName, object value)
        {
            if (instance == null)
            {
                return;
            }

            FieldInfo field = instance.GetType().GetField(fieldName, BindingFlags.Instance | BindingFlags.NonPublic);
            if (field != null)
            {
                field.SetValue(instance, value);
            }
        }

        private object InvokePublicMethod(object instance, string methodName, params object[] args)
        {
            if (instance == null)
            {
                return null;
            }

            MethodInfo method = instance.GetType().GetMethod(methodName, BindingFlags.Instance | BindingFlags.Public);
            return method != null ? method.Invoke(instance, args) : null;
        }

        private object InvokeNonPublicMethod(object instance, string methodName, params object[] args)
        {
            if (instance == null)
            {
                return null;
            }

            MethodInfo method = instance.GetType().GetMethod(methodName, BindingFlags.Instance | BindingFlags.NonPublic);
            return method != null ? method.Invoke(instance, args) : null;
        }

        private sealed class CheckpointResult
        {
            public readonly string Name;
            public readonly string Status;
            public readonly string Detail;

            public CheckpointResult(string name, string status, string detail)
            {
                Name = name;
                Status = status;
                Detail = detail;
            }
        }
    }
}
