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
        RecoveryPressureChoice,
        SecondRunRouteConsequence,
        SurgeRuntimeConsequence,
        WaitCostPressureClock,
        DungeonRouteFeel,
        DungeonRoomInteraction,
        RoomInteractionConsequenceChain
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

    public static void RunBatch85SecondRunRouteConsequenceProof()
    {
        RunProof(ProofMode.SecondRunRouteConsequence);
    }

    public static void RunBatch85_1SurgeRuntimeConsequenceProof()
    {
        RunProof(ProofMode.SurgeRuntimeConsequence);
    }

    public static void RunBatch86WaitCostPressureClockProof()
    {
        RunProof(ProofMode.WaitCostPressureClock);
    }

    public static void RunBatch87DungeonRouteFeelProof()
    {
        RunProof(ProofMode.DungeonRouteFeel);
    }

    public static void RunBatch88DungeonRoomInteractionProof()
    {
        RunProof(ProofMode.DungeonRoomInteraction);
    }

    public static void RunBatch89RoomInteractionConsequenceChainProof()
    {
        RunProof(ProofMode.RoomInteractionConsequenceChain);
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
            ValidateSecondDungeonRouteReadback,
            ResolveSecondRouteEncounterPopover,
            SimulateSecondRunResult,
            ReturnSecondResultToWorld,
            WaitForSecondReturnedCityHub,
            ValidateSecondResultBoard,
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
        private string _routeConsequenceSourceText = string.Empty;
        private string _routeFeelSourceText = string.Empty;
        private string _roomInteractionSourceText = string.Empty;
        private string _roomInteractionRuntimeText = string.Empty;
        private string _roomInteractionBattleContextText = string.Empty;
        private string _secondDungeonReadbackText = string.Empty;
        private string _secondEncounterPopoverText = string.Empty;
        private string _secondRunResultText = string.Empty;
        private string _secondResultBoardText = string.Empty;
        private string _waitCostBeforeRecoveryText = string.Empty;
        private string _waitCostAfterRecoveryText = string.Empty;
        private int _secondPrepWorldDayBeforeRecovery;
        private bool _secondLaunchConfirmed;
        private bool _batch88FirstPopoverCleared;
        private bool _batch88GreedCacheInteracted;
        private bool _batch89BattleContextCaptured;
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
                    case ProofStep.ValidateSecondDungeonRouteReadback:
                        ValidateSecondDungeonRouteReadback();
                        break;
                    case ProofStep.ResolveSecondRouteEncounterPopover:
                        ResolveSecondRouteEncounterPopover();
                        break;
                    case ProofStep.SimulateSecondRunResult:
                        SimulateSecondRunResult();
                        break;
                    case ProofStep.ReturnSecondResultToWorld:
                        ReturnSecondResultToWorld();
                        break;
                    case ProofStep.WaitForSecondReturnedCityHub:
                        WaitForSecondReturnedCityHub();
                        break;
                    case ProofStep.ValidateSecondResultBoard:
                        ValidateSecondResultBoard();
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
                " | Stock=" + SafeText(_boot.SelectedCityManaShardStockLabel) +
                " | LastConsumed=" + SafeText(_boot.SelectedLastDayConsumedLabel) +
                " | LastShortages=" + SafeText(_boot.SelectedLastDayShortagesLabel) +
                " | NeedPressure=" + SafeText(_boot.SelectedNeedPressureLabel) +
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
            bool hasWaitCostPressureClock =
                !IsWaitCostPressureClockProofMode() ||
                prep != null &&
                ContainsAll(prep.RecoverOneDayChoiceText, "Wait Cost", "Pressure Clock", "world advances 1 day") &&
                ContainsAll(prep.AfterRecoveryPreviewText, "After waiting", "stock -", "pressure") &&
                ContainsAll(prep.RecoveryPressureChoiceText, "Launch now", "Wait Cost") &&
                ContainsAll(prep.RouteAppetiteRecommendationText, "Stability", "Surge", "recovery", "strain");
            bool hasRouteConsequenceCards =
                ContainsAll(safeCard, "Stability consequence", "base clear mana_shard x16", "ceiling mana_shard x19", "event recovery +8", "lower payout") &&
                ContainsAll(riskyCard, "Surge consequence", "base clear mana_shard x20", "ceiling mana_shard x25", "event recovery +6", "higher payout", "tighter party recovery");
            bool hasRouteConsequenceSources = !IsRouteConsequenceProofMode() ||
                                              ValidateRouteConsequenceDataSources(out _routeConsequenceSourceText);
            bool hasDungeonRouteFeel =
                !IsDungeonRouteFeelProofMode() ||
                ContainsAll(safeCard, "Dungeon feel", "Stability", "sustain", "Rest Shrine", "lower") &&
                ContainsAll(riskyCard, "Dungeon feel", "Surge", "pressure", "Greed Cache", "strain") &&
                ValidateDungeonRouteFeelDataSources(out _routeFeelSourceText);
            bool hasDungeonRoomInteraction =
                !IsDungeonRoomInteractionProofMode() ||
                ContainsAll(safeCard, "Dungeon feel", "Rest Shrine", "sustain") &&
                ContainsAll(riskyCard, "Dungeon feel", "Greed Cache", "strain") &&
                ValidateDungeonRoomInteractionDataSources(out _roomInteractionSourceText);

            if (!hasResultCarryover || !hasPartyCarryover || !hasRouteChoice || !hasGateAndReason || !hasSecondRunDesire || !hasRecoveryChoice || !hasWaitCostPressureClock)
            {
                Fail("Second ExpeditionPrep did not carry result, party/loadout, route, gate, desire-pressure, or recovery choice context. " + _secondPrepText);
                return;
            }

            if (IsRouteConsequenceProofMode() && (!hasRouteConsequenceCards || !hasRouteConsequenceSources))
            {
                Fail(
                    "Second ExpeditionPrep route cards did not expose data-backed Stability/Surge consequences. " +
                    "CardsOk=" + hasRouteConsequenceCards +
                    " SourcesOk=" + hasRouteConsequenceSources +
                    " Sources=" + SafeText(_routeConsequenceSourceText) +
                    " " + _secondPrepText);
                return;
            }

            if (IsDungeonRouteFeelProofMode() && !hasDungeonRouteFeel)
            {
                Fail(
                    "Second ExpeditionPrep route cards did not expose data-backed dungeon-internal Stability/Surge feel. " +
                    "Sources=" + SafeText(_routeFeelSourceText) +
                    " " + _secondPrepText);
                return;
            }

            if (IsDungeonRoomInteractionProofMode() && !hasDungeonRoomInteraction)
            {
                Fail(
                    "Second ExpeditionPrep route cards did not expose data-backed Rest Shrine / Greed Cache interaction beats. " +
                    "Sources=" + SafeText(_roomInteractionSourceText) +
                    " " + _secondPrepText);
                return;
            }

            RecordPass("Second ExpeditionPrep context", _secondPrepText);
            if (IsRouteConsequenceProofMode())
            {
                RecordPass("Second route consequence source", _routeConsequenceSourceText);
            }
            if (IsDungeonRouteFeelProofMode())
            {
                RecordPass("Batch87 route feel source", _routeFeelSourceText);
            }
            if (IsDungeonRoomInteractionProofMode())
            {
                RecordPass(IsRoomInteractionConsequenceProofMode() ? "Batch89 room interaction source" : "Batch88 room interaction source", _roomInteractionSourceText);
            }

            if (_proofMode == ProofMode.RecoveryPressureChoice || IsWaitCostPressureClockProofMode())
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
            _waitCostBeforeRecoveryText = BuildSelectedCityWaitCostRailText("BeforeWaiting");
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
            _waitCostAfterRecoveryText = BuildSelectedCityWaitCostRailText("AfterWaiting");
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
                " | Recommendation=" + SafeText(prep != null ? prep.RouteAppetiteRecommendationText : "None") +
                " | " + SafeText(_waitCostBeforeRecoveryText) +
                " | " + SafeText(_waitCostAfterRecoveryText);

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
            bool waitCostPressureClockUpdated =
                !IsWaitCostPressureClockProofMode() ||
                prep != null &&
                ContainsAll(prep.AfterRecoveryPreviewText, "After waiting", "readiness Ready", "stock -", "pressure") &&
                ContainsAll(prep.RouteAppetiteRecommendationText, "Next", "launch now", "waiting again", "shortage") &&
                ContainsAny(_waitCostAfterRecoveryText, "Consumed=1", "Consumed=2", "Consumed=3", "Shortages=1", "Shortages=2", "Shortages=3");

            if (!dayAdvanced || !boardStillOpen || !readinessUpdated || !appetiteChanged || !waitReadbackStillHonest || !waitCostPressureClockUpdated)
            {
                Fail("Recover 1 Day did not update readiness/pressure/appetite through the existing rail. " + _secondPrepAfterRecoveryText);
                return;
            }

            RecordPass("Recover 1 Day choice", _secondPrepAfterRecoveryText);
            if (IsWaitCostPressureClockProofMode())
            {
                RecordPass("Wait cost pressure clock", _waitCostBeforeRecoveryText + " | " + _waitCostAfterRecoveryText);
            }

            AdvanceTo(ProofStep.SelectSecondRouteOrValidateBlocked, "Recovery choice updated second ExpeditionPrep.");
        }

        private void SelectSecondRouteOrValidateBlocked()
        {
            string targetRouteId = GetSecondProofRouteId();
            if (_boot.IsRouteChoiceAvailable(targetRouteId) && _boot.TryTriggerRouteChoice(targetRouteId))
            {
                ExpeditionPrepSurfaceData prep = _boot.GetSelectedExpeditionPrepSurfaceData();
                _secondRunDecisionText = BuildPrepDecisionText(prep);
                if (IsRouteConsequenceProofMode() && !HasSelectedRouteConsequenceText(_secondRunDecisionText, targetRouteId))
                {
                    Fail("Second route commit summary did not explain the selected " + GetRouteScenarioName(targetRouteId) + " consequence. Summary=" + SafeText(_secondRunDecisionText) + ".");
                    return;
                }

                RecordPass("Second route select", "Selected route " + targetRouteId + ". " + SafeText(_secondRunDecisionText));
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
            string targetRouteId = GetSecondProofRouteId();
            if (!ValidateLaunchPlan(launchPlan, "Second launch contract", targetRouteId))
            {
                return;
            }

            string launchText = BuildLaunchPlanText(launchPlan);
            if (IsDungeonRouteFeelProofMode() || IsDungeonRoomInteractionProofMode())
            {
                if (!HasSelectedRouteConsequenceText(launchText, targetRouteId))
                {
                    Fail("Second launch contract did not preserve selected route consequence. Launch=" + SafeText(launchText) + ".");
                    return;
                }

                RecordPass("Second run launch", launchText);
                AdvanceTo(ProofStep.ValidateSecondDungeonRouteReadback, "Second launch dungeon route contract validated.");
                return;
            }

            if (IsRouteConsequenceProofMode())
            {
                if (!HasSelectedRouteConsequenceText(launchText, targetRouteId))
                {
                    Fail("Second launch contract did not preserve selected route consequence. Launch=" + SafeText(launchText) + ".");
                    return;
                }

                RecordPass("Second run launch", launchText);
                AdvanceTo(ProofStep.SimulateSecondRunResult, "Second launch consequence contract validated.");
                return;
            }

            RecordPass("Second run launch", launchText);
            RecordPass("Repeat core loop proof completed", "World -> CityHub -> ExpeditionPrep -> DungeonRun -> Result -> World/CityHub -> ExpeditionPrep -> DungeonRun is repeatable.");
            AdvanceTo(ProofStep.Shutdown, "Proof complete.");
        }

        private void ValidateSecondDungeonRouteReadback()
        {
            if (_boot.CurrentAppFlowStage != AppFlowStage.DungeonRun || _boot.IsDungeonRouteChoiceVisible)
            {
                return;
            }

            PrototypeDungeonRunShellSurfaceData shell = _boot.GetDungeonRunShellSurfaceData();
            ExpeditionStartContext start = shell != null ? shell.ExpeditionStartContext : null;
            PrototypeDungeonPanelContext panel = shell != null ? shell.PanelContext : null;
            _secondDungeonReadbackText =
                "Route=" + SafeText(start != null ? start.RoutePreviewSummaryText : "None") +
                " | BattleWatch=" + SafeText(start != null ? start.EventPreviewSummaryText : "None") +
                " | PanelRisk=" + SafeText(panel != null ? panel.RiskPreviewSummaryText : "None") +
                " | Room=" + SafeText(shell != null ? shell.CurrentRoomLabel : "None") +
                " | Sustain=" + SafeText(shell != null ? shell.SustainPressureText : "None") +
                " | Next=" + SafeText(shell != null ? shell.NextMajorGoalText : "None");

            bool hasSurgeDungeonReadback =
                ContainsAll(_secondDungeonReadbackText, "Surge", "Dungeon feel", "pressure", "Greed Cache", "strain") &&
                ContainsAny(_secondDungeonReadbackText, "Mixed Front", "Goblin Pair Hall", "Cache");
            if (!hasSurgeDungeonReadback)
            {
                Fail("Second DungeonRun did not expose the Surge internal route feel. " + SafeText(_secondDungeonReadbackText));
                return;
            }

            RecordPass("Second DungeonRun route readback", _secondDungeonReadbackText);
            AdvanceTo(ProofStep.ResolveSecondRouteEncounterPopover, "Second DungeonRun readback validated.");
        }

        private void ResolveSecondRouteEncounterPopover()
        {
            if (IsDungeonRoomInteractionProofMode())
            {
                ResolveDungeonRoomInteractionProof();
                return;
            }

            PrototypeDungeonRunShellSurfaceData shell = _boot.GetDungeonRunShellSurfaceData();
            if (shell != null &&
                shell.IsBattleResultPopoverVisible &&
                shell.BattleResultPopover != null &&
                shell.BattleResultPopover.IsVisible)
            {
                _secondEncounterPopoverText =
                    "Title=" + SafeText(shell.BattleResultPopover.TitleText) +
                    " | Encounter=" + SafeText(shell.BattleResultPopover.EncounterNameText) +
                    " | Summary=" + SafeText(shell.BattleResultPopover.SummaryText) +
                    " | RoutePlan=" + SafeText(shell.BattleResultPopover.RoutePlanText) +
                    " | Loot=" + SafeText(shell.BattleResultPopover.LootSummaryText) +
                    " | Party=" + SafeText(shell.BattleResultPopover.PartySummaryText);
                bool hasRouteCheck =
                    ContainsAll(_secondEncounterPopoverText, "Route Check", "Surge Pressure", "recovery strain") &&
                    ContainsAny(_secondEncounterPopoverText, "Mixed Front", "greed cache", "shard payout", "Standard Path");
                if (!hasRouteCheck)
                {
                    Fail("Second route encounter popover did not explain the Surge route check. " + SafeText(_secondEncounterPopoverText));
                    return;
                }

                RecordPass("Second route encounter popover", _secondEncounterPopoverText);
                AdvanceTo(ProofStep.SimulateSecondRunResult, "Second route popover validated.");
                return;
            }

            if (_boot.CurrentAppFlowStage == AppFlowStage.BattleScene)
            {
                ResolveActiveBattleTurn();
                return;
            }

            if (_boot.CurrentAppFlowStage == AppFlowStage.DungeonRun)
            {
                ResolveRouteFeelExploreStep();
            }
        }

        private void ResolveDungeonRoomInteractionProof()
        {
            PrototypeDungeonRunShellSurfaceData shell = _boot.GetDungeonRunShellSurfaceData();
            if (shell != null &&
                shell.IsBattleResultPopoverVisible &&
                shell.BattleResultPopover != null &&
                shell.BattleResultPopover.IsVisible)
            {
                string popoverText =
                    "Title=" + SafeText(shell.BattleResultPopover.TitleText) +
                    " | Encounter=" + SafeText(shell.BattleResultPopover.EncounterNameText) +
                    " | Summary=" + SafeText(shell.BattleResultPopover.SummaryText) +
                    " | RoutePlan=" + SafeText(shell.BattleResultPopover.RoutePlanText) +
                    " | Loot=" + SafeText(shell.BattleResultPopover.LootSummaryText) +
                    " | Party=" + SafeText(shell.BattleResultPopover.PartySummaryText);

                if (!_batch88FirstPopoverCleared)
                {
                    bool hasFirstRouteCheck =
                        ContainsAll(popoverText, "Route Check", "Surge Pressure") &&
                        ContainsAny(popoverText, "Greed Cache", "payout", "strain");
                    if (!hasFirstRouteCheck)
                    {
                        Fail("Batch88 first encounter popover did not lead toward the Greed Cache interaction. " + SafeText(popoverText));
                        return;
                    }

                    _batch88FirstPopoverCleared = true;
                    InvokeNonPublicMethod(GetWorldView(), "ClearBattleResultPopover");
                    RecordPass("Batch88 first encounter route check", popoverText);
                    return;
                }

                _secondEncounterPopoverText = popoverText;
                bool hasInteractionConsequence = IsRoomInteractionConsequenceProofMode()
                    ? ContainsAll(_secondEncounterPopoverText, "Greed Cache", "Cache Pressure", "Cache Check", "reward secured", "strain warning") &&
                      ContainsAny(_secondEncounterPopoverText, "Goblin Pair Hall", "Route Check", "recovery strain")
                    : ContainsAll(_secondEncounterPopoverText, "Greed Cache", "Cache payoff secured", "Surge strain") &&
                      ContainsAny(_secondEncounterPopoverText, "Goblin Pair Hall", "Route Check", "recovery strain");
                if (!hasInteractionConsequence)
                {
                    Fail((IsRoomInteractionConsequenceProofMode() ? "Batch89" : "Batch88") + " second encounter popover did not reflect the Greed Cache interaction consequence. " + SafeText(_secondEncounterPopoverText));
                    return;
                }

                if (IsRoomInteractionConsequenceProofMode() && !_batch89BattleContextCaptured)
                {
                    Fail("Batch89 reached the next-encounter popover before proving Cache Pressure battle context. " + SafeText(_roomInteractionBattleContextText));
                    return;
                }

                RecordPass(IsRoomInteractionConsequenceProofMode() ? "Batch89 interaction consequence popover" : "Batch88 interaction encounter popover", _secondEncounterPopoverText);
                AdvanceTo(ProofStep.SimulateSecondRunResult, (IsRoomInteractionConsequenceProofMode() ? "Batch89" : "Batch88") + " interaction popover validated.");
                return;
            }

            if (_boot.CurrentAppFlowStage == AppFlowStage.BattleScene)
            {
                if (IsRoomInteractionConsequenceProofMode())
                {
                    CaptureBatch89BattleContextIfReady();
                }

                ResolveActiveBattleTurn();
                return;
            }

            if (_boot.CurrentAppFlowStage != AppFlowStage.DungeonRun)
            {
                return;
            }

            if (!_batch88GreedCacheInteracted && TryResolveBatch88GreedCacheInteraction())
            {
                return;
            }

            ResolveRouteFeelExploreStep();
        }

        private void CaptureBatch89BattleContextIfReady()
        {
            if (_batch89BattleContextCaptured)
            {
                return;
            }

            PrototypeBattleRequest request = _boot.GetBattleRequest();
            string encounterIdentityText =
                SafeText(request != null ? request.EncounterId : "None") +
                " | " + SafeText(request != null ? request.EncounterName : "None") +
                " | " + SafeText(request != null ? request.RoomLabel : "None");
            if (request == null ||
                (!ContainsValue(encounterIdentityText, "Goblin Pair Hall") &&
                 !ContainsValue(encounterIdentityText, "encounter-room-2")))
            {
                return;
            }

            PrototypeBattleUiSurfaceData surface = _boot.GetBattleUiSurfaceData();
            PrototypeBattleContextData context = _boot.GetCurrentBattleContextData();
            _roomInteractionBattleContextText =
                "Request=" + SafeText(request.EncounterContextText) +
                " | Risk=" + SafeText(request.RiskContextText) +
                " | Intent=" + SafeText(surface != null ? surface.MissionIntentSummaryText : "None") +
                " | Context=" + SafeText(context != null ? context.ContextSummaryText : "None");
            if (!ContainsAll(_roomInteractionBattleContextText, "Cache Pressure", "payout secured", "strain warning"))
            {
                Fail("Batch89 battle context did not acknowledge Cache Pressure at the next encounter. " + SafeText(_roomInteractionBattleContextText));
                return;
            }

            _batch89BattleContextCaptured = true;
            RecordPass("Batch89 battle context", _roomInteractionBattleContextText);
        }

        private bool TryResolveBatch88GreedCacheInteraction()
        {
            object worldView = GetWorldView();
            if (worldView == null)
            {
                Fail("World view disappeared before Batch88 Greed Cache interaction.");
                return true;
            }

            object room = InvokeNonPublicMethod(worldView, "GetCurrentPlannedRoomStep");
            if (room == null)
            {
                return false;
            }

            string roomName = GetObjectField<string>(room, "DisplayName");
            if (!ContainsValue(roomName, "Greed Cache"))
            {
                return false;
            }

            Vector2Int markerPosition = GetObjectField<Vector2Int>(room, "MarkerPosition");
            SetPrivateField(worldView, "_playerGridPosition", markerPosition);
            InvokeNonPublicMethod(worldView, "ProcessExploreStep");
            PrototypeDungeonRunShellSurfaceData beforeShell = _boot.GetDungeonRunShellSurfaceData();
            string prompt = SafeText(beforeShell != null ? beforeShell.CurrentSelectionPromptText : _boot.CurrentSelectionPromptLabel);
            int beforeCarried = GetPrivateField<int>(worldView, "_carriedLootAmount");
            int beforeChest = GetPrivateField<int>(worldView, "_chestLootAmount");
            int expectedChestReward = GetExpectedRouteChestLootAmount(RiskyRouteId);
            object interacted = InvokePublicMethod(worldView, "TryInteractCurrentDungeonRoomBeat");
            bool didInteract = interacted is bool value && value;
            PrototypeDungeonRunShellSurfaceData afterShell = _boot.GetDungeonRunShellSurfaceData();
            int afterCarried = GetPrivateField<int>(worldView, "_carriedLootAmount");
            int afterChest = GetPrivateField<int>(worldView, "_chestLootAmount");
            string pendingConsequence = GetPrivateField<string>(worldView, "_pendingRoomInteractionConsequenceText");
            string pendingTarget = GetPrivateField<string>(worldView, "_pendingRoomInteractionTargetEncounterId");
            string feedback = SafeText(_boot.BattleFeedbackLabel);
            string eventText = SafeText(afterShell != null ? afterShell.EventChoiceText : "None");
            _roomInteractionRuntimeText =
                "Prompt=" + SafeText(prompt) +
                " | Interacted=" + didInteract +
                " | Carried=" + beforeCarried + "->" + afterCarried +
                " | Chest=" + beforeChest + "->" + afterChest +
                " | Feedback=" + feedback +
                " | Event=" + eventText +
                " | Pending=" + SafeText(pendingConsequence) +
                " | Target=" + SafeText(pendingTarget) +
                " | NextRoom=" + SafeText(afterShell != null ? afterShell.CurrentRoomLabel : "None");

            bool promptExplainsDecision = ContainsAll(prompt, "[E]", "Greed Cache") &&
                                          ContainsAny(prompt, "Surge", "strain") &&
                                          ContainsAny(prompt, "payout", "mana_shard", "secure");
            bool rewardChanged = didInteract &&
                                 afterCarried - beforeCarried == expectedChestReward &&
                                 afterChest - beforeChest == expectedChestReward;
            bool feedbackExplainsConsequence = ContainsAll(feedback + " " + eventText, "Greed Cache", "Cache payoff secured", "Surge strain");
            bool nextBeatArmed =
                !IsRoomInteractionConsequenceProofMode() ||
                ContainsAll(feedback + " " + eventText + " " + pendingConsequence, "Next: Cache Pressure", "Goblin Pair Hall", "strain warning") &&
                ContainsValue(pendingTarget, "encounter-room-2");
            bool advancedToNextRoom = afterShell != null && ContainsAny(afterShell.CurrentRoomLabel, "Goblin Pair Hall", "Unstable Shrine", "Core Threshold");
            if (!promptExplainsDecision || !rewardChanged || !feedbackExplainsConsequence || !nextBeatArmed || !advancedToNextRoom)
            {
                Fail("Batch88 Greed Cache interaction was not playable or data-backed. " + _roomInteractionRuntimeText);
                return true;
            }

            _batch88GreedCacheInteracted = true;
            RecordPass("Batch88 Greed Cache interaction", _roomInteractionRuntimeText);
            return true;
        }

        private void SimulateSecondRunResult()
        {
            object worldView = GetWorldView();
            if (worldView == null)
            {
                Fail("World view missing for simulated second result.");
                return;
            }

            string targetRouteId = GetSecondProofRouteId();
            int expectedReturnedLoot = GetExpectedRouteBaseClear(targetRouteId);
            object clearState = ParseNestedEnum(worldView, "RunResultState", "Clear");
            object victoryState = ParseNestedEnum(worldView, "BattleState", "Victory");
            InvokeNonPublicMethod(
                worldView,
                "FinishDungeonRun",
                clearState,
                victoryState,
                true,
                expectedReturnedLoot,
                "Batch85 proof cleared the " + GetRouteScenarioName(targetRouteId) + " route and returned with mana_shard x" + expectedReturnedLoot + ".");

            if (!_boot.IsDungeonResultPanelVisible && _boot.CurrentAppFlowStage != AppFlowStage.ResultPipeline)
            {
                Fail("Simulated second run result did not reach ResultPipeline. Stage=" + _boot.CurrentAppFlowStage + ".");
                return;
            }

            AppFlowResultContext resultContext = _boot.CurrentAppFlowContext != null ? _boot.CurrentAppFlowContext.LatestResult : null;
            ExpeditionOutcome outcome = resultContext != null ? resultContext.ExpeditionOutcome : null;
            OutcomeReadback readback = resultContext != null ? resultContext.OutcomeReadback : null;
            _secondRunResultText =
                "Outcome=" + SafeText(outcome != null ? outcome.ResultStateKey : "None") +
                " | Route=" + SafeText(outcome != null ? outcome.RouteSummaryText : "None") +
                " | Loot=mana_shard x" + (outcome != null ? outcome.ReturnedLootAmount : 0) +
                " | Event=" + SafeText(readback != null ? readback.EventChoiceSummaryText : outcome != null ? outcome.EventChoiceSummaryText : "None") +
                " | MeaningId=" + SafeText(outcome != null ? outcome.OutcomeMeaningId : "None") +
                " | Risk=" + SafeText(outcome != null ? outcome.RiskRewardContextText : "None") +
                " | Meaning=" + SafeText(outcome != null ? outcome.CityImpactMeaningText : "None") +
                " | Readback=" + SafeText(readback != null ? readback.CityImpactMeaningText : "None") +
                " | Next=" + SafeText(readback != null ? readback.RecommendationShiftText : "None");
            bool hasActualResult =
                outcome != null &&
                outcome.Success &&
                outcome.ReturnedLootAmount == expectedReturnedLoot &&
                HasSelectedRouteResultText(_secondRunResultText, targetRouteId);
            bool hasRoomInteractionResult =
                !IsDungeonRoomInteractionProofMode() ||
                (IsRoomInteractionConsequenceProofMode()
                    ? ContainsAll(_secondRunResultText, "Greed Cache", "Cache Pressure", "Cache Check", "reward secured", "strain warning")
                    : ContainsAll(_secondRunResultText, "Greed Cache", "Cache payoff secured", "Surge strain"));
            if (!hasActualResult || !hasRoomInteractionResult)
            {
                Fail("Second route actual result did not reflect the " + GetRouteScenarioName(targetRouteId) + " consequence. " + _secondRunResultText);
                return;
            }

            RecordPass("Second route actual result", _secondRunResultText);
            AdvanceTo(ProofStep.ReturnSecondResultToWorld, "Second route result validated.");
        }

        private void ReturnSecondResultToWorld()
        {
            object worldView = GetWorldView();
            if (worldView == null)
            {
                Fail("World view missing for second result return.");
                return;
            }

            SetPrivateField(worldView, "_pendingDungeonExit", true);
            AppFlowObservedSnapshot snapshot = (AppFlowObservedSnapshot)InvokePublicMethod(worldView, "BuildAppFlowSnapshot");
            bool returned = (bool)InvokeNonPublicMethod(_boot, "TryExitDungeonRunToWorldSim");
            if (!snapshot.HasPendingWorldReturn || !returned)
            {
                Fail("Second result return was not consumed. Pending=" + snapshot.HasPendingWorldReturn + " Returned=" + returned + ".");
                return;
            }

            RecordPass("Second result return", "Result return consumed and world shell re-entry requested.");
            AdvanceTo(ProofStep.WaitForSecondReturnedCityHub, "Returned second result to world shell.");
        }

        private void WaitForSecondReturnedCityHub()
        {
            if (!_boot.IsWorldSimActive || _boot.CurrentAppFlowStage != AppFlowStage.CityHub)
            {
                return;
            }

            RecordPass("Second world return -> CityHub", "Returned to selected CityHub after second result. Stage=" + _boot.CurrentAppFlowStage + ".");
            AdvanceTo(ProofStep.ValidateSecondResultBoard, "Second returned CityHub active.");
        }

        private void ValidateSecondResultBoard()
        {
            PrototypeCityHubUiSurfaceData ui = _boot.GetCityHubUiSurfaceData();
            PrototypeCityHubSelectionSurfaceData selection = ui != null ? ui.Selection : null;
            PrototypeCityHubOutcomeSurfaceData outcome = ui != null ? ui.Outcome : null;
            if (ui == null || selection == null || outcome == null || !ui.HasSelectedCity || !selection.IsCitySelection)
            {
                Fail("CityHub UI did not expose a selected city after second result return.");
                return;
            }

            string summary = selection.PressureBoardSummaryText;
            string latest = selection.RecentResultEvidenceText;
            string changed = selection.PressureChangeText;
            string ready = selection.PartyReadinessSummaryText;
            string next = outcome.CorrectiveFollowUpText;
            _secondResultBoardText =
                "Summary=" + SafeText(summary) +
                " | Latest=" + SafeText(latest) +
                " | Changed=" + SafeText(changed) +
                " | Ready=" + SafeText(ready) +
                " | Next=" + SafeText(next);
            string targetRouteId = GetSecondProofRouteId();
            bool hasLatest =
                ContainsAll(latest, "Returned", "Party") &&
                HasSelectedRouteBoardLatestText(latest, targetRouteId);
            bool hasChanged = HasText(changed) && ContainsAny(changed, "Stock +", "Pressure", "Readiness", "absorbed");
            bool hasConsequence = HasSelectedRouteBoardConsequenceText(_secondResultBoardText, targetRouteId);
            bool hasRoomInteractionBoard =
                !IsDungeonRoomInteractionProofMode() ||
                (IsRoomInteractionConsequenceProofMode()
                    ? ContainsAll(_secondResultBoardText, "Greed Cache", "Cache Pressure", "Cache Check", "reward secured", "strain warning")
                    : ContainsAll(_secondResultBoardText, "Greed Cache", "Cache payoff secured", "Surge strain"));
            if (!hasLatest || !hasChanged || !hasConsequence || !hasRoomInteractionBoard)
            {
                Fail("Second result world board did not reflect the chosen " + GetRouteScenarioName(targetRouteId) + " consequence. " + _secondResultBoardText);
                return;
            }

            RecordPass("Second result world board", _secondResultBoardText);
            if (IsDungeonRouteFeelProofMode())
            {
                RecordPass(
                    "Batch87 dungeon route feel comparison",
                    "Stability card/readback source: " + SafeText(_routeFeelSourceText) +
                    " | Surge runtime readback: " + SafeText(_secondDungeonReadbackText) +
                    " | Surge popover: " + SafeText(_secondEncounterPopoverText) +
                    " | Final board: " + SafeText(_secondResultBoardText));
                AdvanceTo(ProofStep.Shutdown, "Batch87 proof complete.");
                return;
            }

            if (IsDungeonRoomInteractionProofMode())
            {
                if (IsRoomInteractionConsequenceProofMode())
                {
                    RecordPass(
                        "Batch89 room interaction consequence chain proof",
                        "Source: " + SafeText(_roomInteractionSourceText) +
                        " | Runtime: " + SafeText(_roomInteractionRuntimeText) +
                        " | BattleContext: " + SafeText(_roomInteractionBattleContextText) +
                        " | Popover: " + SafeText(_secondEncounterPopoverText) +
                        " | Result: " + SafeText(_secondRunResultText) +
                        " | Board: " + SafeText(_secondResultBoardText));
                    AdvanceTo(ProofStep.Shutdown, "Batch89 proof complete.");
                    return;
                }

                RecordPass(
                    "Batch88 dungeon room interaction proof",
                    "Source: " + SafeText(_roomInteractionSourceText) +
                    " | Runtime: " + SafeText(_roomInteractionRuntimeText) +
                    " | Popover: " + SafeText(_secondEncounterPopoverText) +
                    " | Result: " + SafeText(_secondRunResultText) +
                    " | Board: " + SafeText(_secondResultBoardText));
                AdvanceTo(ProofStep.Shutdown, "Batch88 proof complete.");
                return;
            }

            if (IsSurgeRuntimeConsequenceProofMode())
            {
                RecordPass(
                    "Batch85.1 surge runtime comparison",
                    "Stability baseline: base=16 ceiling=19 recover=8; Surge actual: returned mana_shard x" +
                    GetExpectedRouteBaseClear(RiskyRouteId) +
                    " with recovery margin +" + GetExpectedRouteRecoverAmount(RiskyRouteId) +
                    " and board readiness cost visible. " + SafeText(_secondResultBoardText));
                AdvanceTo(ProofStep.Shutdown, "Batch85.1 proof complete.");
                return;
            }

            RecordPass("Batch85 route consequence proof completed", "Safe route reached actual result/world board; risky route consequence was verified from authored data. " + SafeText(_routeConsequenceSourceText));
            AdvanceTo(ProofStep.Shutdown, "Batch85 proof complete.");
        }

        private bool IsRouteConsequenceProofMode()
        {
            return _proofMode == ProofMode.SecondRunRouteConsequence ||
                   _proofMode == ProofMode.SurgeRuntimeConsequence ||
                   _proofMode == ProofMode.DungeonRouteFeel ||
                   _proofMode == ProofMode.DungeonRoomInteraction ||
                   _proofMode == ProofMode.RoomInteractionConsequenceChain;
        }

        private bool IsSurgeRuntimeConsequenceProofMode()
        {
            return _proofMode == ProofMode.SurgeRuntimeConsequence;
        }

        private bool IsWaitCostPressureClockProofMode()
        {
            return _proofMode == ProofMode.WaitCostPressureClock;
        }

        private bool IsDungeonRouteFeelProofMode()
        {
            return _proofMode == ProofMode.DungeonRouteFeel;
        }

        private bool IsDungeonRoomInteractionProofMode()
        {
            return _proofMode == ProofMode.DungeonRoomInteraction ||
                   _proofMode == ProofMode.RoomInteractionConsequenceChain;
        }

        private bool IsRoomInteractionConsequenceProofMode()
        {
            return _proofMode == ProofMode.RoomInteractionConsequenceChain;
        }

        private void ResolveActiveBattleTurn()
        {
            string battleState = SafeText(_boot.BattlePhaseLabel);
            if (battleState == "Player Turn")
            {
                _boot.TryTriggerBattleAction("attack");
                return;
            }

            if (battleState == "Target Select")
            {
                object monster = InvokeNonPublicMethod(GetWorldView(), "GetFirstLivingBattleMonster");
                string monsterId = monster != null ? GetPropertyValue<string>(monster, "MonsterId") : string.Empty;
                if (string.IsNullOrEmpty(monsterId) || !_boot.TryTriggerBattleTarget(monsterId))
                {
                    Fail("Could not resolve battle target for Batch87 route-feel proof. Target=" + SafeText(monsterId) + ".");
                }

                return;
            }

            if (battleState == "Enemy Turn")
            {
                InvokeNonPublicMethod(GetWorldView(), "ExecuteQueuedEnemyIntent");
            }
        }

        private void ResolveRouteFeelExploreStep()
        {
            object worldView = GetWorldView();
            if (worldView == null)
            {
                Fail("World view disappeared during Batch87 route-feel proof.");
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

            Vector2Int markerPosition = GetObjectField<Vector2Int>(room, "MarkerPosition");
            SetPrivateField(worldView, "_playerGridPosition", markerPosition);
            InvokeNonPublicMethod(worldView, "ProcessExploreStep");
            if (IsRoomInteractionConsequenceProofMode())
            {
                CaptureBatch89BattleContextIfReady();
            }

            InvokePublicMethod(worldView, "TryInteractCurrentDungeonRoomBeat");
        }

        private string BuildSelectedCityWaitCostRailText(string label)
        {
            if (_boot == null)
            {
                return label + ": None";
            }

            return label +
                   ": Day=" + _boot.WorldDayCount +
                   " | Stock=" + SafeText(_boot.SelectedCityManaShardStockLabel) +
                   " | Consumed=" + SafeText(_boot.SelectedLastDayConsumedLabel) +
                   " | Shortages=" + SafeText(_boot.SelectedLastDayShortagesLabel) +
                   " | Pressure=" + SafeText(_boot.SelectedNeedPressureLabel) +
                   " | Readiness=" + SafeText(_boot.SelectedDispatchReadinessLabel) +
                   " | RecoveryEta=" + SafeText(_boot.SelectedRecoveryEtaLabel);
        }

        private string GetSecondProofRouteId()
        {
            return IsSurgeRuntimeConsequenceProofMode() || IsDungeonRouteFeelProofMode() || IsDungeonRoomInteractionProofMode() ? RiskyRouteId : SafeRouteId;
        }

        private string GetRouteScenarioName(string routeId)
        {
            return routeId == RiskyRouteId ? "Surge" : "Stability";
        }

        private bool HasSelectedRouteConsequenceText(string text, string routeId)
        {
            return routeId == RiskyRouteId
                ? ContainsAll(text, "Surge consequence", "base clear mana_shard x20", "ceiling mana_shard x25", "higher payout", "event recovery +6")
                : ContainsAll(text, "Stability consequence", "base clear mana_shard x16", "ceiling mana_shard x19", "lower payout", "event recovery +8");
        }

        private bool HasSelectedRouteResultText(string text, string routeId)
        {
            return routeId == RiskyRouteId
                ? ContainsAll(text, "Surge", "mana_shard x20", "outcome-mana-shard-city-a-surge") &&
                  ContainsAny(text, "higher payout", "bigger mana_shard", "less dispatch stability", "rougher", "tighter")
                : ContainsAll(text, "Stability", "mana_shard x16", "outcome-mana-shard-city-a") &&
                  ContainsAny(text, "lower payout", "lower shard", "recovery cushion", "Stabilizes");
        }

        private bool HasSelectedRouteLaunchText(string text, string routeId)
        {
            return routeId == RiskyRouteId
                ? ContainsAll(text, "Standard Path", "Surge consequence", "x20", "higher payout") &&
                  !ContainsValue(text, "Objective=Launch Dungeon Alpha through the Rest Path")
                : ContainsAll(text, "Rest Path", "Stability consequence", "x16", "lower payout");
        }

        private bool HasSelectedRouteBoardLatestText(string latestText, string routeId)
        {
            return routeId == RiskyRouteId
                ? ContainsAny(latestText, "Surge Window", "Standard Path", "risky") &&
                  ContainsAll(latestText, "mana_shard") &&
                  ContainsAny(latestText, "x20", "20")
                : ContainsAny(latestText, "Stability Run", "Rest Path", "safe") &&
                  ContainsAll(latestText, "mana_shard") &&
                  ContainsAny(latestText, "x16", "16");
        }

        private bool HasSelectedRouteBoardConsequenceText(string boardText, string routeId)
        {
            return routeId == RiskyRouteId
                ? ContainsAny(boardText, "Surge", "Standard Path") &&
                  ContainsAny(boardText, "Strained", "recovery 2", "Blocked") &&
                  ContainsAny(boardText, "x20", "Stock +20")
                : ContainsAny(boardText, "Stability", "Rest Path") &&
                  ContainsAny(boardText, "Recovering", "recovery 1") &&
                  ContainsAny(boardText, "x16", "Stock +16");
        }

        private int GetExpectedRouteBaseClear(string routeId)
        {
            return GetExpectedRouteBattleLootAmount(routeId) + GetExpectedRouteChestLootAmount(routeId);
        }

        private int GetExpectedRouteBattleLootAmount(string routeId)
        {
            GoldenPathRouteDefinition route = GetExpectedRouteDefinition(routeId);
            return route != null ? route.BattleLootAmount : routeId == RiskyRouteId ? 17 : 14;
        }

        private int GetExpectedRouteChestLootAmount(string routeId)
        {
            GoldenPathRouteDefinition route = GetExpectedRouteDefinition(routeId);
            return route != null ? route.ChestRewardAmount : routeId == RiskyRouteId ? 3 : 2;
        }

        private int GetExpectedRouteRecoverAmount(string routeId)
        {
            GoldenPathRouteDefinition route = GetExpectedRouteDefinition(routeId);
            return route != null ? route.RecoverAmount : routeId == RiskyRouteId ? 6 : 8;
        }

        private GoldenPathRouteDefinition GetExpectedRouteDefinition(string routeId)
        {
            return GoldenPathContentRegistry.TryGetChainForRoute(
                    SelectedCityId,
                    TargetDungeonId,
                    routeId,
                    out GoldenPathChainDefinition chain) &&
                   chain != null
                ? chain.CanonicalRoute
                : null;
        }

        private bool ValidateRouteConsequenceDataSources(out string detail)
        {
            detail = "None";
            bool safeChainLoaded = GoldenPathContentRegistry.TryGetChainForRoute(SelectedCityId, TargetDungeonId, SafeRouteId, out GoldenPathChainDefinition safeChain);
            bool riskyChainLoaded = GoldenPathContentRegistry.TryGetChainForRoute(SelectedCityId, TargetDungeonId, RiskyRouteId, out GoldenPathChainDefinition riskyChain);
            GoldenPathRouteDefinition safeRoute = safeChain != null ? safeChain.CanonicalRoute : null;
            GoldenPathRouteDefinition riskyRoute = riskyChain != null ? riskyChain.CanonicalRoute : null;
            bool safeRouteMeaningLoaded = safeRoute != null &&
                                          GoldenPathContentRegistry.TryGetRouteMeaning(safeRoute.RouteMeaningId, out GoldenPathRouteMeaningDefinition safeMeaning) &&
                                          safeMeaning != null &&
                                          ContainsAll(safeMeaning.ScenarioLabel + " " + safeMeaning.WatchOutText, "Stability", "lower payout");
            bool riskyRouteMeaningLoaded = riskyRoute != null &&
                                           GoldenPathContentRegistry.TryGetRouteMeaning(riskyRoute.RouteMeaningId, out GoldenPathRouteMeaningDefinition riskyMeaning) &&
                                           riskyMeaning != null &&
                                           ContainsAll(riskyMeaning.ScenarioLabel + " " + riskyMeaning.RewardPreview, "Surge", "Higher");
            bool safeOutcomeMeaningLoaded = safeChain != null &&
                                            GoldenPathContentRegistry.TryGetOutcomeMeaning(safeChain.OutcomeMeaningId, out GoldenPathOutcomeMeaningDefinition safeOutcomeMeaning) &&
                                            safeOutcomeMeaning != null &&
                                            ContainsAny(safeOutcomeMeaning.CityImpactMeaningText, "Stability", "lower", "recovery");
            bool riskyOutcomeMeaningLoaded = riskyChain != null &&
                                             GoldenPathContentRegistry.TryGetOutcomeMeaning(riskyChain.OutcomeMeaningId, out GoldenPathOutcomeMeaningDefinition riskyOutcomeMeaning) &&
                                             riskyOutcomeMeaning != null &&
                                             ContainsAny(riskyOutcomeMeaning.CityImpactMeaningText, "bigger", "less dispatch stability", "rougher");

            int safeBaseClear = safeRoute != null ? safeRoute.BattleLootAmount + safeRoute.ChestRewardAmount : 0;
            int safeCeiling = safeRoute != null ? safeBaseClear + safeRoute.BonusLootAmount : 0;
            int riskyBaseClear = riskyRoute != null ? riskyRoute.BattleLootAmount + riskyRoute.ChestRewardAmount : 0;
            int riskyCeiling = riskyRoute != null ? riskyBaseClear + riskyRoute.BonusLootAmount : 0;
            detail =
                "safe=data:" + SafeText(safeChain != null ? safeChain.ChainId : "None") +
                " base=" + safeBaseClear +
                " ceiling=" + safeCeiling +
                " recover=" + (safeRoute != null ? safeRoute.RecoverAmount : 0) +
                " outcome=" + SafeText(safeChain != null ? safeChain.OutcomeMeaningId : "None") +
                " | risky=data:" + SafeText(riskyChain != null ? riskyChain.ChainId : "None") +
                " base=" + riskyBaseClear +
                " ceiling=" + riskyCeiling +
                " recover=" + (riskyRoute != null ? riskyRoute.RecoverAmount : 0) +
                " outcome=" + SafeText(riskyChain != null ? riskyChain.OutcomeMeaningId : "None");

            bool valueDifferenceLoaded =
                safeChainLoaded &&
                riskyChainLoaded &&
                safeRoute != null &&
                riskyRoute != null &&
                safeRoute.RouteId == SafeRouteId &&
                riskyRoute.RouteId == RiskyRouteId &&
                safeBaseClear == 16 &&
                safeCeiling == 19 &&
                safeRoute.RecoverAmount == 8 &&
                riskyBaseClear == 20 &&
                riskyCeiling == 25 &&
                riskyRoute.RecoverAmount == 6 &&
                riskyBaseClear > safeBaseClear &&
                riskyCeiling > safeCeiling &&
                riskyRoute.RecoverAmount < safeRoute.RecoverAmount;
            return valueDifferenceLoaded &&
                   safeRouteMeaningLoaded &&
                   riskyRouteMeaningLoaded &&
                   safeOutcomeMeaningLoaded &&
                   riskyOutcomeMeaningLoaded;
        }

        private bool ValidateDungeonRouteFeelDataSources(out string detail)
        {
            detail = "None";
            bool safeChainLoaded = GoldenPathContentRegistry.TryGetChainForRoute(SelectedCityId, TargetDungeonId, SafeRouteId, out GoldenPathChainDefinition safeChain);
            bool riskyChainLoaded = GoldenPathContentRegistry.TryGetChainForRoute(SelectedCityId, TargetDungeonId, RiskyRouteId, out GoldenPathChainDefinition riskyChain);
            GoldenPathRouteDefinition safeRoute = safeChain != null ? safeChain.CanonicalRoute : null;
            GoldenPathRouteDefinition riskyRoute = riskyChain != null ? riskyChain.CanonicalRoute : null;
            string safeRooms = BuildRouteRoomListText(safeRoute);
            string riskyRooms = BuildRouteRoomListText(riskyRoute);

            GoldenPathRouteMeaningDefinition safeMeaning = null;
            GoldenPathRouteMeaningDefinition riskyMeaning = null;
            GoldenPathEncounterProfileDefinition safeProfile = null;
            GoldenPathEncounterProfileDefinition riskyProfile = null;
            GoldenPathBattleSetupDefinition safeSetup = null;
            GoldenPathBattleSetupDefinition riskySetup = null;
            bool safeRouteMeaningLoaded = safeRoute != null &&
                                          GoldenPathContentRegistry.TryGetRouteMeaning(safeRoute.RouteMeaningId, out safeMeaning) &&
                                          safeMeaning != null &&
                                          ContainsAll(safeMeaning.ScenarioLabel + " " + safeMeaning.CombatPlanText + " " + safeMeaning.EventFocus, "Stability", "sustain", "Shrine");
            bool riskyRouteMeaningLoaded = riskyRoute != null &&
                                           GoldenPathContentRegistry.TryGetRouteMeaning(riskyRoute.RouteMeaningId, out riskyMeaning) &&
                                           riskyMeaning != null &&
                                           ContainsAll(riskyMeaning.ScenarioLabel + " " + riskyMeaning.CombatPlanText + " " + riskyMeaning.EventFocus, "Surge", "mixed", "strain");
            bool safeEncounterLoaded =
                GoldenPathContentRegistry.TryGetEncounterProfile("encounter-profile-alpha-safe-entry", out safeProfile) &&
                safeProfile != null &&
                ContainsAll(safeProfile.EncounterRoleTagsText + " " + safeProfile.MissionRelevanceText, "slime", "stabilizer");
            bool riskyEncounterLoaded =
                GoldenPathContentRegistry.TryGetEncounterProfile("encounter-profile-alpha-risky-breach", out riskyProfile) &&
                riskyProfile != null &&
                ContainsAll(riskyProfile.EncounterRoleTagsText + " " + riskyProfile.MissionRelevanceText, "mixed", "tempo");
            bool safeBattleSetupLoaded =
                GoldenPathContentRegistry.TryGetBattleSetup("battle-setup-alpha-safe-room1", out safeSetup) &&
                safeSetup != null &&
                ContainsAll(safeSetup.EnemyGroupLabel + " " + safeSetup.WinRelevanceText, "Slime", "Rest Path", "stable");
            bool riskyBattleSetupLoaded =
                GoldenPathContentRegistry.TryGetBattleSetup("battle-setup-alpha-risky-room1", out riskySetup) &&
                riskySetup != null &&
                ContainsAll(riskySetup.EnemyGroupLabel + " " + riskySetup.WinRelevanceText, "Mixed", "Standard Path", "shard payout");
            bool safeRoomsLoaded = ContainsAll(safeRooms, "Slime Front", "Rest Shrine", "Watch Hall", "Supply Cache");
            bool riskyRoomsLoaded = ContainsAll(riskyRooms, "Mixed Front", "Greed Cache", "Goblin Pair Hall", "Unstable Shrine");

            detail =
                "safe=data:" + SafeText(safeChain != null ? safeChain.ChainId : "None") +
                " rooms=" + SafeText(safeRooms) +
                " combat=" + SafeText(safeMeaning != null ? safeMeaning.CombatPlanText : "None") +
                " setup=" + SafeText(safeSetup != null ? safeSetup.WinRelevanceText : "None") +
                " | risky=data:" + SafeText(riskyChain != null ? riskyChain.ChainId : "None") +
                " rooms=" + SafeText(riskyRooms) +
                " combat=" + SafeText(riskyMeaning != null ? riskyMeaning.CombatPlanText : "None") +
                " setup=" + SafeText(riskySetup != null ? riskySetup.WinRelevanceText : "None");

            return safeChainLoaded &&
                   riskyChainLoaded &&
                   safeRoute != null &&
                   riskyRoute != null &&
                   safeRouteMeaningLoaded &&
                   riskyRouteMeaningLoaded &&
                   safeEncounterLoaded &&
                   riskyEncounterLoaded &&
                   safeBattleSetupLoaded &&
                   riskyBattleSetupLoaded &&
                   safeRoomsLoaded &&
                   riskyRoomsLoaded &&
                   !string.Equals(safeRooms, riskyRooms, StringComparison.Ordinal);
        }

        private bool ValidateDungeonRoomInteractionDataSources(out string detail)
        {
            detail = "None";
            bool safeChainLoaded = GoldenPathContentRegistry.TryGetChainForRoute(SelectedCityId, TargetDungeonId, SafeRouteId, out GoldenPathChainDefinition safeChain);
            bool riskyChainLoaded = GoldenPathContentRegistry.TryGetChainForRoute(SelectedCityId, TargetDungeonId, RiskyRouteId, out GoldenPathChainDefinition riskyChain);
            GoldenPathRouteDefinition safeRoute = safeChain != null ? safeChain.CanonicalRoute : null;
            GoldenPathRouteDefinition riskyRoute = riskyChain != null ? riskyChain.CanonicalRoute : null;
            string safeRooms = BuildRouteRoomListText(safeRoute);
            string riskyRooms = BuildRouteRoomListText(riskyRoute);

            bool safeRestShrineLoaded = safeRoute != null &&
                                        safeRoute.RecoverAmount == 8 &&
                                        ContainsAll(safeRooms, "Rest Shrine", "Watch Hall") &&
                                        RouteContainsRoomType(safeRoute, "Rest Shrine", "Shrine");
            bool riskyGreedCacheLoaded = riskyRoute != null &&
                                         riskyRoute.ChestRewardAmount == 3 &&
                                         riskyRoute.BattleLootAmount + riskyRoute.ChestRewardAmount == 20 &&
                                         ContainsAll(riskyRooms, "Greed Cache", "Goblin Pair Hall") &&
                                         RouteContainsRoomType(riskyRoute, "Greed Cache", "Cache");

            detail =
                "safe=data:" + SafeText(safeChain != null ? safeChain.ChainId : "None") +
                " rooms=" + SafeText(safeRooms) +
                " restRecover=" + (safeRoute != null ? safeRoute.RecoverAmount : 0) +
                " | risky=data:" + SafeText(riskyChain != null ? riskyChain.ChainId : "None") +
                " rooms=" + SafeText(riskyRooms) +
                " greedCache=" + (riskyRoute != null ? riskyRoute.ChestRewardAmount : 0) +
                " baseClear=" + (riskyRoute != null ? riskyRoute.BattleLootAmount + riskyRoute.ChestRewardAmount : 0);

            return safeChainLoaded &&
                   riskyChainLoaded &&
                   safeRestShrineLoaded &&
                   riskyGreedCacheLoaded;
        }

        private bool RouteContainsRoomType(GoldenPathRouteDefinition route, string displayName, string roomType)
        {
            if (route == null || route.Rooms == null)
            {
                return false;
            }

            for (int i = 0; i < route.Rooms.Length; i++)
            {
                GoldenPathRoomDefinition room = route.Rooms[i];
                if (room != null &&
                    ContainsValue(room.DisplayName, displayName) &&
                    ContainsValue(room.RoomType, roomType))
                {
                    return true;
                }
            }

            return false;
        }

        private string BuildRouteRoomListText(GoldenPathRouteDefinition route)
        {
            if (route == null || route.Rooms == null || route.Rooms.Length == 0)
            {
                return "None";
            }

            List<string> rooms = new List<string>();
            for (int i = 0; i < route.Rooms.Length; i++)
            {
                GoldenPathRoomDefinition room = route.Rooms[i];
                if (room != null && HasText(room.DisplayName))
                {
                    rooms.Add(room.DisplayName);
                }
            }

            return rooms.Count > 0 ? string.Join(" -> ", rooms.ToArray()) : "None";
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
            return ValidateLaunchPlan(launchPlan, checkpoint, SafeRouteId);
        }

        private bool ValidateLaunchPlan(ExpeditionPlan launchPlan, string checkpoint, string expectedRouteId)
        {
            string launchPlanText = BuildLaunchPlanText(launchPlan);
            if (launchPlan == null ||
                !launchPlan.IsConfirmed ||
                launchPlan.OriginCityId != _selectedCityId ||
                launchPlan.TargetDungeonId != TargetDungeonId ||
                launchPlan.SelectedRoute == null ||
                launchPlan.SelectedRoute.RouteId != expectedRouteId ||
                !HasSelectedRouteLaunchText(launchPlanText, expectedRouteId))
            {
                Fail(
                    checkpoint + " was not coherent. City=" + SafeText(launchPlan != null ? launchPlan.OriginCityId : "None") +
                    " Dungeon=" + SafeText(launchPlan != null ? launchPlan.TargetDungeonId : "None") +
                    " Route=" + SafeText(launchPlan != null && launchPlan.SelectedRoute != null ? launchPlan.SelectedRoute.RouteId : "None") +
                    " ExpectedRoute=" + SafeText(expectedRouteId) +
                    " Launch=" + SafeText(launchPlanText) + ".");
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
                " | Consequence=" + SafeText(prep != null ? prep.ConsequencePreviewText : "None") +
                " | Party=" + SafeText(prep != null ? prep.StagedPartySummaryText : "None") +
                " | Loadout=" + SafeText(prep != null ? prep.PartyLoadoutSummaryText : "None") +
                " | Gate=" + SafeText(prep != null ? prep.LaunchGateSummaryText : "None") +
                " | Commit=" + SafeText(prep != null ? prep.CommitReasonText : "None") +
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
            _summary.AppendLine("- Branch=" + (_proofMode == ProofMode.SurgeRuntimeConsequence
                ? "B surge runtime consequence seam proof over existing recovery rail"
                : "A repeat-loop proof over existing loop contracts"));
            _summary.AppendLine("- ProofMode=" + _proofMode);
            _summary.AppendLine("- FirstPrepRouteCard=" + SafeText(_firstPrepRouteCardText));
            _summary.AppendLine("- FirstRunLaunch=" + SafeText(_firstRunLaunchText));
            _summary.AppendLine("- PostResultBoard=" + SafeText(_postResultBoardText));
            _summary.AppendLine("- SecondPrep=" + SafeText(_secondPrepText));
            _summary.AppendLine("- SecondPrepAfterRecovery=" + SafeText(_secondPrepAfterRecoveryText));
            _summary.AppendLine("- SecondRunDecision=" + SafeText(_secondRunDecisionText));
            _summary.AppendLine("- RouteConsequenceSource=" + SafeText(_routeConsequenceSourceText));
            _summary.AppendLine("- RoomInteractionSource=" + SafeText(_roomInteractionSourceText));
            _summary.AppendLine("- RoomInteractionRuntime=" + SafeText(_roomInteractionRuntimeText));
            _summary.AppendLine("- SecondRunResult=" + SafeText(_secondRunResultText));
            _summary.AppendLine("- SecondResultBoard=" + SafeText(_secondResultBoardText));
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

        private T GetObjectField<T>(object instance, string fieldName)
        {
            if (instance == null)
            {
                return default;
            }

            FieldInfo field = instance.GetType().GetField(fieldName, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            return field != null ? (T)field.GetValue(instance) : default;
        }

        private T GetPropertyValue<T>(object instance, string propertyName)
        {
            if (instance == null)
            {
                return default;
            }

            PropertyInfo property = instance.GetType().GetProperty(propertyName, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            return property != null ? (T)property.GetValue(instance) : default;
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
