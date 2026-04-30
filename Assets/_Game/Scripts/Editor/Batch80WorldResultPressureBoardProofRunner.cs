using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

public static class Batch80WorldResultPressureBoardProofRunner
{
    private const string ScenePath = "Assets/Scenes/SampleScene.unity";
    private const double StepTimeoutSeconds = 18d;
    private const string SessionActiveKey = "Batch80WorldResultPressureBoardProofRunner.Active";
    private const string SelectedRouteId = "safe";
    private static ProofSession _session;

    [InitializeOnLoadMethod]
    private static void RestorePendingSession()
    {
        if (!SessionState.GetBool(SessionActiveKey, false) || _session != null)
        {
            return;
        }

        _session = new ProofSession();
        _session.RestoreAfterDomainReload();
    }

    public static void RunBatch80WorldResultPressureBoardProof()
    {
        if (_session != null)
        {
            Debug.LogError("[Batch80Proof] Proof runner is already active.");
            EditorApplication.Exit(1);
            return;
        }

        _session = new ProofSession();
        _session.Start();
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
            RecruitParty,
            EnterExpeditionPrep,
            WaitForExpeditionPrep,
            SelectRoute,
            ConfirmLaunch,
            WaitForDungeonRun,
            SimulateRunResult,
            ReturnToWorld,
            WaitForReturnedCityHub,
            ValidatePressureBoard,
            ReenterOrValidateBlocked,
            ValidateReentryContinuity,
            Shutdown
        }

        private readonly List<CheckpointResult> _checkpoints = new List<CheckpointResult>();
        private readonly StringBuilder _summary = new StringBuilder();
        private ProofStep _step = ProofStep.OpenScene;
        private double _stepStartedAt;
        private BootEntry _boot;
        private string _selectedCityId = string.Empty;
        private string _boardSummaryText = string.Empty;
        private string _latestResultText = string.Empty;
        private string _pressureChangeText = string.Empty;
        private string _readinessText = string.Empty;
        private string _nextActionText = string.Empty;
        private bool _shutdownRequested;

        public void Start()
        {
            SessionState.SetBool(SessionActiveKey, true);
            Debug.Log("[Batch80Proof] Opening sample scene.");
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
                    case ProofStep.RecruitParty:
                        RecruitParty();
                        break;
                    case ProofStep.EnterExpeditionPrep:
                        EnterExpeditionPrep();
                        break;
                    case ProofStep.WaitForExpeditionPrep:
                        WaitForExpeditionPrep();
                        break;
                    case ProofStep.SelectRoute:
                        SelectRoute();
                        break;
                    case ProofStep.ConfirmLaunch:
                        ConfirmLaunch();
                        break;
                    case ProofStep.WaitForDungeonRun:
                        WaitForDungeonRun();
                        break;
                    case ProofStep.SimulateRunResult:
                        SimulateRunResult();
                        break;
                    case ProofStep.ReturnToWorld:
                        ReturnToWorld();
                        break;
                    case ProofStep.WaitForReturnedCityHub:
                        WaitForReturnedCityHub();
                        break;
                    case ProofStep.ValidatePressureBoard:
                        ValidatePressureBoard();
                        break;
                    case ProofStep.ReenterOrValidateBlocked:
                        ReenterOrValidateBlocked();
                        break;
                    case ProofStep.ValidateReentryContinuity:
                        ValidateReentryContinuity();
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

            Debug.Log("[Batch80Proof] BootEntry located.");
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
            WorldSelectableMarker cityMarker = FindCityMarker("city-a");
            if (cityMarker == null)
            {
                Fail("Could not find city-a marker.");
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
            AdvanceTo(ProofStep.RecruitParty, "CityHub active.");
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
            AdvanceTo(ProofStep.EnterExpeditionPrep, "Party recruited.");
        }

        private void EnterExpeditionPrep()
        {
            if (!_boot.TryEnterSelectedWorldDungeon())
            {
                Fail("Failed to enter ExpeditionPrep from selected city.");
                return;
            }

            AdvanceTo(ProofStep.WaitForExpeditionPrep, "Requested ExpeditionPrep.");
        }

        private void WaitForExpeditionPrep()
        {
            if (_boot.CurrentAppFlowStage != AppFlowStage.ExpeditionPrep || !_boot.IsDungeonRouteChoiceVisible)
            {
                return;
            }

            ExpeditionPrepSurfaceData prep = _boot.GetSelectedExpeditionPrepSurfaceData();
            if (prep == null || prep.RouteOptions == null || prep.RouteOptions.Length < 2)
            {
                Fail("ExpeditionPrep route options were missing before launch.");
                return;
            }

            RecordPass("CityHub -> ExpeditionPrep", "Prep board visible. RouteOptions=" + prep.RouteOptions.Length + ".");
            AdvanceTo(ProofStep.SelectRoute, "ExpeditionPrep active.");
        }

        private void SelectRoute()
        {
            if (!_boot.IsRouteChoiceAvailable(SelectedRouteId) || !_boot.TryTriggerRouteChoice(SelectedRouteId))
            {
                Fail("Could not select route " + SelectedRouteId + ". Prompt=" + SafeText(_boot.RouteChoicePromptLabel) + ".");
                return;
            }

            RecordPass("ExpeditionPrep route select", "Selected route " + SelectedRouteId + ".");
            AdvanceTo(ProofStep.ConfirmLaunch, "Route selected.");
        }

        private void ConfirmLaunch()
        {
            if (!_boot.CanConfirmRouteChoice() || !_boot.TryConfirmRouteChoice())
            {
                Fail("Could not confirm launch. Prompt=" + SafeText(_boot.RouteChoicePromptLabel) + ".");
                return;
            }

            AdvanceTo(ProofStep.WaitForDungeonRun, "Launch confirmed.");
        }

        private void WaitForDungeonRun()
        {
            if (_boot.CurrentAppFlowStage != AppFlowStage.DungeonRun || _boot.IsDungeonRouteChoiceVisible)
            {
                return;
            }

            AppFlowContext context = _boot.CurrentAppFlowContext;
            ExpeditionPlan launchPlan = context != null && context.CurrentDungeonRun != null
                ? context.CurrentDungeonRun.LaunchPlan
                : null;
            if (launchPlan == null ||
                !launchPlan.IsConfirmed ||
                launchPlan.OriginCityId != _selectedCityId ||
                launchPlan.TargetDungeonId != "dungeon-alpha")
            {
                Fail("Confirmed launch contract was not available before simulated result.");
                return;
            }

            RecordPass("ExpeditionPrep -> DungeonRun", "Launch contract ready. Route=" + SafeText(launchPlan.SelectedRoute != null ? launchPlan.SelectedRoute.RouteId : "None") + ".");
            AdvanceTo(ProofStep.SimulateRunResult, "DungeonRun active.");
        }

        private void SimulateRunResult()
        {
            object worldView = GetWorldView();
            if (worldView == null)
            {
                Fail("World view missing for simulated result.");
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
                "Batch80 proof cleared Dungeon Alpha and returned with mana_shard x16.");

            if (!_boot.IsDungeonResultPanelVisible && _boot.CurrentAppFlowStage != AppFlowStage.ResultPipeline)
            {
                Fail("Simulated run result did not reach ResultPipeline. Stage=" + _boot.CurrentAppFlowStage + ".");
                return;
            }

            RecordPass("Simulated dungeon result", "Result reached. Stage=" + _boot.CurrentAppFlowStage + ".");
            AdvanceTo(ProofStep.ReturnToWorld, "Result simulated.");
        }

        private void ReturnToWorld()
        {
            object worldView = GetWorldView();
            if (worldView == null)
            {
                Fail("World view missing for return.");
                return;
            }

            SetPrivateField(worldView, "_pendingDungeonExit", true);
            AppFlowObservedSnapshot snapshot = (AppFlowObservedSnapshot)InvokePublicMethod(worldView, "BuildAppFlowSnapshot");
            bool returned = (bool)InvokeNonPublicMethod(_boot, "TryExitDungeonRunToWorldSim");
            if (!snapshot.HasPendingWorldReturn || !returned)
            {
                Fail("Result return was not consumed. Pending=" + snapshot.HasPendingWorldReturn + " Returned=" + returned + ".");
                return;
            }

            AdvanceTo(ProofStep.WaitForReturnedCityHub, "Returned to world shell.");
        }

        private void WaitForReturnedCityHub()
        {
            if (!_boot.IsWorldSimActive || _boot.CurrentAppFlowStage != AppFlowStage.CityHub)
            {
                return;
            }

            RecordPass("World return -> CityHub pressure board", "Returned to selected CityHub after result. Stage=" + _boot.CurrentAppFlowStage + ".");
            AdvanceTo(ProofStep.ValidatePressureBoard, "Returned CityHub active.");
        }

        private void ValidatePressureBoard()
        {
            PrototypeCityHubUiSurfaceData ui = _boot.GetCityHubUiSurfaceData();
            PrototypeCityHubSelectionSurfaceData selection = ui != null ? ui.Selection : null;
            PrototypeCityHubOutcomeSurfaceData outcome = ui != null ? ui.Outcome : null;
            if (ui == null || selection == null || outcome == null || !ui.HasSelectedCity || !selection.IsCitySelection)
            {
                Fail("CityHub UI did not expose a selected city after result return.");
                return;
            }

            _boardSummaryText = selection.PressureBoardSummaryText;
            _latestResultText = selection.RecentResultEvidenceText;
            _pressureChangeText = selection.PressureChangeText;
            _readinessText = selection.PartyReadinessSummaryText;
            _nextActionText = outcome.CorrectiveFollowUpText;

            bool hasBoardSummary = ContainsAll(_boardSummaryText, "pressure board", "Latest:", "Changed:", "Next:") &&
                                   ContainsAny(_boardSummaryText, "Ready:", "Blocked:");
            bool hasLatestResult = ContainsAll(_latestResultText, "Returned", "Party") &&
                                   ContainsAny(_latestResultText, "Rest Path", "Stability Run", "Dungeon Alpha", "safe") &&
                                   ContainsAny(_latestResultText, "Cleared", "Failed", "Returned") &&
                                   ContainsAny(_latestResultText, "mana_shard", "x16", "16");
            bool hidesInternalResultKey = !ContainsValue(_latestResultText, "run_clear");
            bool hasWhyItMatters = HasText(selection.WhyCityMattersText);
            bool hasPressureChange = HasText(_pressureChangeText) &&
                                     !ContainsValue(_pressureChangeText, "No pressure change recorded") &&
                                     ContainsAny(_pressureChangeText, "Pressure", "Readiness", "Stock +", "absorbed");
            bool hasReadiness = HasText(_readinessText) && ContainsAny(_readinessText, "Ready", "Blocked", "warning", "recovery");
            bool hasRouteRecommendation = HasText(selection.RecommendedRouteText) &&
                                          ContainsAny(selection.RecommendedRouteText, "Rest Path", "Stability Run", "safe", "Route");
            bool hasRecommendationReason = HasText(selection.RecommendationReasonText);
            bool hasNextAction = HasText(_nextActionText) || ContainsAny(_boardSummaryText, "Next");

            if (!hasBoardSummary ||
                !hasLatestResult ||
                !hidesInternalResultKey ||
                !hasWhyItMatters ||
                !hasPressureChange ||
                !hasReadiness ||
                !hasRouteRecommendation ||
                !hasRecommendationReason ||
                !hasNextAction)
            {
                RecordFail(
                    "Pressure board answers five questions",
                    "Board was incomplete. Summary=" + SafeText(_boardSummaryText) +
                    " Latest=" + SafeText(_latestResultText) +
                    " Why=" + SafeText(selection.WhyCityMattersText) +
                    " Changed=" + SafeText(_pressureChangeText) +
                    " Readiness=" + SafeText(_readinessText) +
                    " Route=" + SafeText(selection.RecommendedRouteText) +
                    " Reason=" + SafeText(selection.RecommendationReasonText) +
                    " Next=" + SafeText(_nextActionText) + ".");
                AdvanceTo(ProofStep.Shutdown, "Proof failed.");
                return;
            }

            RecordPass(
                "Pressure board answers five questions",
                "Summary=" + SafeText(_boardSummaryText) +
                " Latest=" + SafeText(_latestResultText) +
                " Changed=" + SafeText(_pressureChangeText) +
                " Readiness=" + SafeText(_readinessText) +
                " Next=" + SafeText(_nextActionText) + ".");
            AdvanceTo(ProofStep.ReenterOrValidateBlocked, "Pressure board validated.");
        }

        private void ReenterOrValidateBlocked()
        {
            if (_boot.TryEnterSelectedWorldDungeon())
            {
                AdvanceTo(ProofStep.ValidateReentryContinuity, "Requested ExpeditionPrep re-entry.");
                return;
            }

            bool blockedExplained = ContainsAny(_readinessText, "Blocked", "recovery", "no idle", "strained") ||
                                    ContainsAny(_boardSummaryText, "Blocked", "Wait", "recovery", "no idle", "strained");
            if (!blockedExplained)
            {
                Fail("ExpeditionPrep re-entry was blocked without board explanation. Readiness=" + SafeText(_readinessText) + ".");
                return;
            }

            RecordPass("Readiness/re-entry", "Re-entry is blocked, and the board explains why. Readiness=" + SafeText(_readinessText) + ".");
            AdvanceTo(ProofStep.Shutdown, "Proof complete.");
        }

        private void ValidateReentryContinuity()
        {
            if (_boot.CurrentAppFlowStage != AppFlowStage.ExpeditionPrep || !_boot.IsDungeonRouteChoiceVisible)
            {
                return;
            }

            ExpeditionPrepSurfaceData prep = _boot.GetSelectedExpeditionPrepSurfaceData();
            AppFlowContext context = _boot.CurrentAppFlowContext;
            AppFlowExpeditionPlan prepContext = context != null ? context.ActiveExpeditionPlan : null;
            ExpeditionPrepReadModel prepReadModel = prepContext != null ? prepContext.PrepReadModel : null;
            bool hasRouteOptions = prep != null && prep.RouteOptions != null && prep.RouteOptions.Length >= 2;
            bool hasResultCarryover = prep != null &&
                                      prep.LatestExpeditionResult != null &&
                                      HasText(prep.LatestExpeditionResult.ResultStateKey) &&
                                      prepReadModel != null &&
                                      HasText(prepReadModel.RecentImpactSummaryText) &&
                                      HasText(prepReadModel.RecommendedActionSummaryText);
            if (!hasRouteOptions || !hasResultCarryover)
            {
                Fail(
                    "Re-entry prep board did not carry result-pressure context. RouteOptions=" +
                    (prep != null && prep.RouteOptions != null ? prep.RouteOptions.Length.ToString() : "0") +
                    " Impact=" + SafeText(prepReadModel != null ? prepReadModel.RecentImpactSummaryText : "None") +
                    " Recommendation=" + SafeText(prepReadModel != null ? prepReadModel.RecommendedActionSummaryText : "None") + ".");
                return;
            }

            RecordPass(
                "Readiness/re-entry",
                "ExpeditionPrep re-entry remains available. RouteOptions=" + prep.RouteOptions.Length +
                " Impact=" + SafeText(prepReadModel.RecentImpactSummaryText) +
                " Recommendation=" + SafeText(prepReadModel.RecommendedActionSummaryText) + ".");
            RecordPass("Targeted board proof completed", "World/CityHub pressure board and re-entry readiness are coherent.");
            AdvanceTo(ProofStep.Shutdown, "Proof complete.");
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
            Debug.Log("[Batch80Proof][" + status + "] " + checkpoint + " :: " + detail);
        }

        private void AdvanceTo(ProofStep nextStep, string reason)
        {
            _step = nextStep;
            _stepStartedAt = EditorApplication.timeSinceStartup;
            Debug.Log("[Batch80Proof] Step -> " + _step + " :: " + reason);
        }

        private void Fail(string detail)
        {
            RecordFail("World result pressure board proof", detail);
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
            EditorApplication.update -= Tick;
            EditorApplication.playModeStateChanged -= OnPlayModeStateChanged;

            _summary.AppendLine("[Batch80Proof] Summary");
            _summary.AppendLine("- BoardSummary=" + SafeText(_boardSummaryText));
            _summary.AppendLine("- LatestResult=" + SafeText(_latestResultText));
            _summary.AppendLine("- PressureChange=" + SafeText(_pressureChangeText));
            _summary.AppendLine("- Readiness=" + SafeText(_readinessText));
            _summary.AppendLine("- NextAction=" + SafeText(_nextActionText));
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
