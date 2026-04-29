using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

public static class Batch79_1RouteScenarioRuntimeProofRunner
{
    private const string ScenePath = "Assets/Scenes/SampleScene.unity";
    private const double StepTimeoutSeconds = 18d;
    private const string SessionActiveKey = "Batch79_1RouteScenarioRuntimeProofRunner.Active";
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

    public static void RunBatch79_1RouteScenarioRuntimeProof()
    {
        if (_session != null)
        {
            Debug.LogError("[Batch79_1Proof] Proof runner is already active.");
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
            EnterFirstBattle,
            ResolveFirstBattle,
            CaptureEncounterPopover,
            StaticFinalPayoffConsumerProof,
            Shutdown
        }

        private readonly List<CheckpointResult> _checkpoints = new List<CheckpointResult>();
        private readonly StringBuilder _summary = new StringBuilder();
        private ProofStep _step = ProofStep.OpenScene;
        private double _stepStartedAt;
        private BootEntry _boot;
        private string _selectedCityId = string.Empty;
        private string _routeCardText = string.Empty;
        private string _commitSummaryText = string.Empty;
        private string _dungeonReadbackText = string.Empty;
        private string _encounterRoutePlanText = string.Empty;
        private string _finalPayoffStaticProofText = string.Empty;
        private bool _shutdownRequested;

        public void Start()
        {
            SessionState.SetBool(SessionActiveKey, true);
            Debug.Log("[Batch79_1Proof] Opening sample scene.");
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
                    case ProofStep.EnterFirstBattle:
                        EnterFirstBattle();
                        break;
                    case ProofStep.ResolveFirstBattle:
                        ResolveFirstBattle();
                        break;
                    case ProofStep.CaptureEncounterPopover:
                        CaptureEncounterPopover();
                        break;
                    case ProofStep.StaticFinalPayoffConsumerProof:
                        StaticFinalPayoffConsumerProof();
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

            Debug.Log("[Batch79_1Proof] BootEntry located.");
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

            RecordPass("MainMenu -> WorldSim", "Entered WorldSim. Day=" + _boot.WorldDayCount + " IdleParties=" + _boot.IdleParties + ".");
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
                Fail("WorldSim selection prerequisites are missing. Camera or world view was null.");
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

            RecordPass("WorldSim -> CityHub", "CityHub active. SelectedCity=" + SafeText(_boot.SelectedIdLabel) + ".");
            AdvanceTo(ProofStep.RecruitParty, "CityHub active.");
        }

        private void RecruitParty()
        {
            _boot.RecruitWorldSimParty();
            if (_boot.IdleParties <= 0)
            {
                Fail("Party recruitment did not create an idle party for city " + SafeText(_selectedCityId) + ".");
                return;
            }

            RecordPass("CityHub party recruit", "Recruited party. IdleParties=" + _boot.IdleParties + ".");
            AdvanceTo(ProofStep.EnterExpeditionPrep, "Party recruited.");
        }

        private void EnterExpeditionPrep()
        {
            if (!_boot.TryEnterSelectedWorldDungeon())
            {
                Fail("Failed to enter ExpeditionPrep from selected city " + SafeText(_selectedCityId) + ".");
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
            ExpeditionPrepRouteOptionData safeOption = FindRouteOption(prep, SelectedRouteId);
            if (safeOption == null)
            {
                Fail("Safe route option was missing from ExpeditionPrep.");
                return;
            }

            _routeCardText = BuildRouteCardText(safeOption);
            if (!ContainsAll(_routeCardText, "Stability Run", "Combat", "slime-heavy sustain"))
            {
                Fail("Route card did not expose the Stability Run combat plan. RouteCard=" + SafeText(_routeCardText) + ".");
                return;
            }

            RecordPass("Route card", "Safe route card exposes the operating scenario. " + SafeText(_routeCardText));
            AdvanceTo(ProofStep.SelectRoute, "ExpeditionPrep route card validated.");
        }

        private void SelectRoute()
        {
            if (!_boot.IsRouteChoiceAvailable(SelectedRouteId))
            {
                Fail("Route '" + SelectedRouteId + "' was not available. Prompt=" + SafeText(_boot.RouteChoicePromptLabel) + ".");
                return;
            }

            if (!_boot.TryTriggerRouteChoice(SelectedRouteId))
            {
                Fail("Failed to select route '" + SelectedRouteId + "'.");
                return;
            }

            ExpeditionPrepSurfaceData prep = _boot.GetSelectedExpeditionPrepSurfaceData();
            _commitSummaryText = BuildCommitSummaryText(prep);
            if (!ContainsAll(_commitSummaryText, "Stability Run", "slime-heavy sustain"))
            {
                Fail("Commit summary did not preserve the Stability Run route plan. CommitSummary=" + SafeText(_commitSummaryText) + ".");
                return;
            }

            RecordPass("Commit summary", "Selected route preserves scenario and combat plan before launch. " + SafeText(_commitSummaryText));
            AdvanceTo(ProofStep.ConfirmLaunch, "Route selected.");
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
                Fail("TryConfirmRouteChoice returned false for route '" + SelectedRouteId + "'.");
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

            PrototypeDungeonRunShellSurfaceData shell = _boot.GetDungeonRunShellSurfaceData();
            ExpeditionStartContext start = shell != null ? shell.ExpeditionStartContext : null;
            PrototypeDungeonPanelContext panel = shell != null ? shell.PanelContext : null;
            _dungeonReadbackText =
                "Route Plan=" + SafeText(start != null ? start.RoutePreviewSummaryText : "None") +
                " | Route Risk=" + SafeText(panel != null ? panel.RiskPreviewSummaryText : "None") +
                " | Battle Watch=" + SafeText(start != null ? start.EventPreviewSummaryText : "None") +
                " | Next=" + SafeText(panel != null ? panel.NextMajorGoalText : "None");
            if (!ContainsAll(_dungeonReadbackText, "Stability Run", "slime-heavy sustain"))
            {
                Fail("Dungeon readback did not keep the Stability Run plan visible. DungeonReadback=" + SafeText(_dungeonReadbackText) + ".");
                return;
            }

            RecordPass("Dungeon readback", "Dungeon shell keeps the selected scenario plan visible. " + SafeText(_dungeonReadbackText));
            AdvanceTo(ProofStep.EnterFirstBattle, "DungeonRun readback validated.");
        }

        private void EnterFirstBattle()
        {
            if (_boot.CurrentAppFlowStage == AppFlowStage.BattleScene)
            {
                PrototypeBattleUiSurfaceData surface = _boot.GetBattleUiSurfaceData();
                RecordPass("First encounter entry", "Entered battle. Encounter=" + SafeText(surface != null ? surface.EncounterName : "None") + ".");
                AdvanceTo(ProofStep.ResolveFirstBattle, "BattleScene active.");
                return;
            }

            PrototypeDungeonRunShellSurfaceData shell = _boot.GetDungeonRunShellSurfaceData();
            if (shell != null && shell.IsEventChoiceVisible)
            {
                _boot.TryTriggerEventChoice("recover");
                return;
            }

            if (shell != null && shell.IsPreEliteChoiceVisible)
            {
                _boot.TryTriggerPreEliteChoice("recover");
                return;
            }

            ResolveExploreStep();
        }

        private void ResolveFirstBattle()
        {
            PrototypeDungeonRunShellSurfaceData shell = _boot.GetDungeonRunShellSurfaceData();
            if (shell != null &&
                shell.IsBattleResultPopoverVisible &&
                shell.BattleResultPopover != null &&
                shell.BattleResultPopover.IsVisible)
            {
                AdvanceTo(ProofStep.CaptureEncounterPopover, "Encounter popover visible.");
                return;
            }

            if (_boot.CurrentAppFlowStage != AppFlowStage.BattleScene)
            {
                return;
            }

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
                    Fail("Could not resolve battle target. Target=" + SafeText(monsterId) + ".");
                }

                return;
            }

            if (battleState == "Enemy Turn")
            {
                InvokeNonPublicMethod(GetWorldView(), "ExecuteQueuedEnemyIntent");
            }
        }

        private void CaptureEncounterPopover()
        {
            PrototypeDungeonRunShellSurfaceData shell = _boot.GetDungeonRunShellSurfaceData();
            PrototypeDungeonBattleResultPopoverData popover = shell != null ? shell.BattleResultPopover : null;
            _encounterRoutePlanText = popover != null ? SafeText(popover.RoutePlanText) : "None";
            if (!ContainsAll(_encounterRoutePlanText, "Stability Run", "Combat", "slime-heavy sustain", "Follow-up"))
            {
                Fail("Encounter popover did not expose the selected route plan. RoutePlan=" + SafeText(_encounterRoutePlanText) + ".");
                return;
            }

            RecordPass("Encounter popover", "Battle result popover connects the encounter back to the route plan. RoutePlan=" + SafeText(_encounterRoutePlanText) + ".");
            AdvanceTo(ProofStep.StaticFinalPayoffConsumerProof, "Encounter popover route plan validated.");
        }

        private void StaticFinalPayoffConsumerProof()
        {
            _finalPayoffStaticProofText =
                "OpenRunBattleResultPopover -> BuildSelectedRouteScenarioPayoffText(outcomeKey) -> " +
                "PrototypeDungeonBattleResultPopoverData.RoutePlanText; " +
                "PrototypePresentationShell.DungeonRun -> BuildDungeonScenarioPayoffLine(resultContext) -> Scenario Payoff.";
            RecordPass("Final scenario payoff static consumer", _finalPayoffStaticProofText);
            AdvanceTo(ProofStep.Shutdown, "Proof complete.");
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

        private string BuildCommitSummaryText(ExpeditionPrepSurfaceData prep)
        {
            ExpeditionStartContext start = prep != null ? prep.StartContext : null;
            return
                "SelectedRoute=" + SafeText(prep != null ? prep.SelectedRouteLabel : "None") +
                " | RoutePreview=" + SafeText(prep != null ? prep.RoutePreviewSummaryText : "None") +
                " | EventPreview=" + SafeText(prep != null ? prep.EventPreviewText : "None") +
                " | LaunchGate=" + SafeText(prep != null ? prep.LaunchGateSummaryText : "None") +
                " | PartyFit=" + SafeText(prep != null ? prep.RouteFitSummaryText : "None") +
                " | StartRoute=" + SafeText(start != null ? start.RoutePreviewSummaryText : "None") +
                " | StartBattleWatch=" + SafeText(start != null ? start.EventPreviewSummaryText : "None");
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
            Debug.Log("[Batch79_1Proof][" + status + "] " + checkpoint + " :: " + detail);
        }

        private void AdvanceTo(ProofStep nextStep, string reason)
        {
            _step = nextStep;
            _stepStartedAt = EditorApplication.timeSinceStartup;
            Debug.Log("[Batch79_1Proof] Step -> " + _step + " :: " + reason);
        }

        private void Fail(string detail)
        {
            RecordFail("Route scenario runtime proof", detail);
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

            _summary.AppendLine("[Batch79_1Proof] Summary");
            _summary.AppendLine("- Branch=C narrow smoke wait fix + targeted route scenario runtime proof");
            _summary.AppendLine("- RouteCard=" + SafeText(_routeCardText));
            _summary.AppendLine("- CommitSummary=" + SafeText(_commitSummaryText));
            _summary.AppendLine("- DungeonReadback=" + SafeText(_dungeonReadbackText));
            _summary.AppendLine("- EncounterRoutePlan=" + SafeText(_encounterRoutePlanText));
            _summary.AppendLine("- FinalPayoffStaticProof=" + SafeText(_finalPayoffStaticProofText));
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
            if (state == PlayModeStateChange.EnteredPlayMode)
            {
                AdvanceTo(ProofStep.WaitForBoot, "Entered PlayMode.");
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

        private T GetFieldValue<T>(object instance, string fieldName)
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
