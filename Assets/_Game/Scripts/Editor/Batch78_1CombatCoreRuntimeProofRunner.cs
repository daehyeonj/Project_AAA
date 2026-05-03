using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

public static class Batch78_1CombatCoreRuntimeProofRunner
{
    private const string ScenePath = "Assets/Scenes/SampleScene.unity";
    private const double StepTimeoutSeconds = 18d;
    private const string SessionActiveKey = "Batch78_1CombatCoreRuntimeProofRunner.Active";
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

    public static void RunBatch78_1CombatCoreRuntimeProof()
    {
        if (_session != null)
        {
            Debug.LogError("[Batch78_1Proof] Proof runner is already active.");
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
            CaptureIntent,
            QueueSetup,
            CaptureSetupPreview,
            ResolveSetup,
            WaitForMiraTurn,
            QueuePayoff,
            CapturePayoffPreview,
            ResolvePayoff,
            RegressionSanity,
            Shutdown
        }

        private readonly List<CheckpointResult> _checkpoints = new List<CheckpointResult>();
        private readonly StringBuilder _summary = new StringBuilder();
        private ProofStep _step = ProofStep.OpenScene;
        private double _stepStartedAt;
        private BootEntry _boot;
        private string _selectedCityId = string.Empty;
        private string _selectedRouteId = "safe";
        private string _encounterName = string.Empty;
        private string _targetId = string.Empty;
        private string _targetName = string.Empty;
        private string _intentRead = string.Empty;
        private string _setupPreview = string.Empty;
        private string _setupPostEffect = string.Empty;
        private string _setupThreat = string.Empty;
        private string _windowSummary = string.Empty;
        private string _setupActualLog = string.Empty;
        private string _payoffPreview = string.Empty;
        private string _payoffPostEffect = string.Empty;
        private string _payoffRoleHint = string.Empty;
        private string _payoffThreat = string.Empty;
        private string _payoffActualLog = string.Empty;
        private bool _shutdownRequested;

        public void Start()
        {
            SessionState.SetBool(SessionActiveKey, true);
            Debug.Log("[Batch78_1Proof] Opening sample scene.");
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
                    case ProofStep.CaptureIntent:
                        CaptureIntent();
                        break;
                    case ProofStep.QueueSetup:
                        QueueSetup();
                        break;
                    case ProofStep.CaptureSetupPreview:
                        CaptureSetupPreview();
                        break;
                    case ProofStep.ResolveSetup:
                        ResolveSetup();
                        break;
                    case ProofStep.WaitForMiraTurn:
                        WaitForMiraTurn();
                        break;
                    case ProofStep.QueuePayoff:
                        QueuePayoff();
                        break;
                    case ProofStep.CapturePayoffPreview:
                        CapturePayoffPreview();
                        break;
                    case ProofStep.ResolvePayoff:
                        ResolvePayoff();
                        break;
                    case ProofStep.RegressionSanity:
                        RegressionSanity();
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

            Debug.Log("[Batch78_1Proof] BootEntry located.");
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

            RecordPass("CityHub -> ExpeditionPrep", "Route choice visible. Prompt=" + SafeText(_boot.RouteChoicePromptLabel) + ".");
            AdvanceTo(ProofStep.SelectRoute, "ExpeditionPrep active.");
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

            RecordPass("ExpeditionPrep route select", "Selected route " + _selectedRouteId + ".");
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
                Fail("TryConfirmRouteChoice returned false for route '" + _selectedRouteId + "'.");
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

            RecordPass("ExpeditionPrep -> DungeonRun", "DungeonRun active. RunLabel=" + SafeText(_boot.CurrentDungeonRunLabel) + " Route=" + SafeText(_boot.CurrentRouteLabel) + ".");
            AdvanceTo(ProofStep.EnterFirstBattle, "DungeonRun active.");
        }

        private void EnterFirstBattle()
        {
            if (_boot.CurrentAppFlowStage == AppFlowStage.BattleScene)
            {
                PrototypeBattleUiSurfaceData surface = _boot.GetBattleUiSurfaceData();
                _encounterName = surface != null ? SafeText(surface.EncounterName) : "None";
                RecordPass("Demo encounter viability", "Entered " + SafeText(_encounterName) + " on " + SafeText(_boot.CurrentDungeonRunLabel) + " / " + SafeText(_boot.CurrentRouteLabel) + ".");
                AdvanceTo(ProofStep.CaptureIntent, "BattleScene active.");
                return;
            }

            if (_boot.IsDungeonResultPanelVisible || _boot.CurrentAppFlowStage == AppFlowStage.ResultPipeline)
            {
                Fail("Run resolved before the proof encounter could be captured.");
                return;
            }

            if (_boot.IsDungeonEventChoiceVisible)
            {
                if (!_boot.TryTriggerEventChoice("recover"))
                {
                    Fail("Failed to resolve event choice with 'recover'.");
                }

                return;
            }

            if (_boot.IsDungeonPreEliteChoiceVisible)
            {
                if (!_boot.TryTriggerPreEliteChoice("recover"))
                {
                    Fail("Failed to resolve pre-elite choice with 'recover'.");
                }

                return;
            }

            ResolveExploreStep();
        }

        private void CaptureIntent()
        {
            if (!IsPlayerTurn())
            {
                return;
            }

            PrototypeBattleUiSurfaceData surface = _boot.GetBattleUiSurfaceData();
            string actorName = surface != null && surface.CurrentActor != null ? SafeText(surface.CurrentActor.DisplayName) : "None";
            string actorRole = surface != null && surface.CurrentActor != null ? SafeText(surface.CurrentActor.RoleLabel) : "None";
            string actorSkill = surface != null && surface.CurrentActor != null ? SafeText(surface.CurrentActor.SkillLabel) : "None";
            if (!EqualsIgnoreCase(actorName, "Alden"))
            {
                Fail("Expected Alden to open the proof loop, but current actor was " + actorName + ".");
                return;
            }

            object target = GetFirstLivingBattleMonster();
            if (target == null)
            {
                Fail("No living target was available for the proof loop.");
                return;
            }

            _targetId = GetPropertyValue<string>(target, "MonsterId");
            _targetName = GetPropertyValue<string>(target, "DisplayName");
            PrototypeBattleUiEnemyData enemy = FindEnemy(surface, _targetId) ?? (surface != null ? surface.SelectedEnemy : null);
            _intentRead = enemy != null ? SafeText(enemy.IntentLabel) : SafeText(GetPropertyValue<string>(target, "IntentLabel"));
            string runtimeIntentKey = GetMonsterRuntimeString(target, "IntentKey");
            string runtimeIntentLabel = GetMonsterRuntimeString(target, "IntentLabel");
            if (!HasMeaningfulValue(runtimeIntentKey) || !HasMeaningfulValue(runtimeIntentLabel))
            {
                Fail("Selected target did not have a readable runtime intent on the first player turn. Target=" + SafeText(_targetName) + " SurfaceIntent=" + SafeText(_intentRead) + ".");
                return;
            }

            RecordPass(
                "Intent read visible",
                "Encounter=" + SafeText(_encounterName) +
                " Actor=" + actorName +
                " Role=" + actorRole +
                " Skill=" + actorSkill +
                " Target=" + SafeText(_targetName) +
                " Intent=" + SafeText(_intentRead) +
                " RuntimeIntentKey=" + SafeText(runtimeIntentKey) +
                " Threat=" + SafeText(surface != null && surface.ActionContext != null ? surface.ActionContext.ThreatSummaryText : string.Empty) + ".");
            AdvanceTo(ProofStep.QueueSetup, "Readable intent captured.");
        }

        private void QueueSetup()
        {
            if (!IsPlayerTurn())
            {
                return;
            }

            if (!_boot.TryTriggerBattleAction("skill"))
            {
                Fail("Failed to queue Alden Power Strike.");
                return;
            }

            AdvanceTo(ProofStep.CaptureSetupPreview, "Queued Alden skill.");
        }

        private void CaptureSetupPreview()
        {
            if (!IsTargetSelect())
            {
                return;
            }

            PrototypeBattleUiSurfaceData surface = _boot.GetBattleUiSurfaceData();
            PrototypeBattleUiTargetSelectionData target = surface != null ? surface.TargetSelection : null;
            if (target == null || !target.IsActive)
            {
                return;
            }

            _setupPreview = SafeText(target.ExpectedEffectText);
            _setupPostEffect = SafeText(target.PostEffectText);
            _setupThreat = SafeText(target.ThreatSummaryText);
            string combined = _setupPreview + " | " + _setupPostEffect + " | " + _setupThreat + " | " + SafeText(target.SkillHintText);
            if (!ContainsText(_setupPreview, "Expected:") || !ContainsText(_setupPostEffect, "Opens Burst Window"))
            {
                Fail("Alden setup preview did not show expected damage plus Opens Burst Window. Preview=" + _setupPreview + " PostEffect=" + _setupPostEffect + ".");
                return;
            }

            if (!ContainsAny(combined, "Alden", "Power Strike", "Response:"))
            {
                Fail("Alden setup preview did not carry a role/action tie-in. Readback=" + combined + ".");
                return;
            }

            RecordPass(
                "Setup preview visible",
                "Action=Power Strike Target=" + SafeText(target.TargetLabel) +
                " TargetIntent=" + SafeText(target.TargetIntentLabel) +
                " Preview=" + _setupPreview +
                " PostEffect=" + _setupPostEffect +
                " RoleHint=" + SafeText(target.ThreatSummaryText) + ".");
            AdvanceTo(ProofStep.ResolveSetup, "Setup preview captured.");
        }

        private void ResolveSetup()
        {
            object target = GetMonsterById(_targetId);
            if (target == null)
            {
                Fail("Proof target disappeared before Alden setup resolved. TargetId=" + SafeText(_targetId) + ".");
                return;
            }

            bool resolved = (bool)InvokeNonPublicMethod(GetWorldView(), "TryResolveTargetSelection", target);
            if (!resolved)
            {
                Fail("Alden setup target selection returned false. Target=" + SafeText(_targetName) + ".");
                return;
            }

            object updatedTarget = GetMonsterById(_targetId);
            bool windowOpen = GetMonsterRuntimeBool(updatedTarget, "HasBurstWindow");
            _windowSummary = GetMonsterRuntimeString(updatedTarget, "BurstWindowSummaryText");
            PrototypeBattleUiSurfaceData surface = _boot.GetBattleUiSurfaceData();
            _setupActualLog = FindRecentEvidence(surface, "opened Burst Window", "Burst Window opened", PrototypeBattleEventKeys.BurstWindowOpened);
            if (!windowOpen)
            {
                Fail("Alden setup resolved but the target did not keep an active Burst Window. Target=" + SafeText(_targetName) + " Log=" + SafeText(_setupActualLog) + ".");
                return;
            }

            if (!HasMeaningfulValue(_setupActualLog))
            {
                Fail("Alden setup opened a window but no recent log/event confirmed it.");
                return;
            }

            RecordPass(
                "Window trigger and log",
                "Target=" + SafeText(_targetName) +
                " Window=Burst Window" +
                " Summary=" + SafeText(_windowSummary) +
                " ActualLog=" + SafeText(_setupActualLog) + ".");
            AdvanceTo(ProofStep.WaitForMiraTurn, "Alden opened Burst Window.");
        }

        private void WaitForMiraTurn()
        {
            if (!IsPlayerTurn())
            {
                return;
            }

            PrototypeBattleUiSurfaceData surface = _boot.GetBattleUiSurfaceData();
            string actorName = surface != null && surface.CurrentActor != null ? SafeText(surface.CurrentActor.DisplayName) : "None";
            if (!EqualsIgnoreCase(actorName, "Mira"))
            {
                Fail("Expected Mira to cash the Burst Window after Alden, but current actor was " + actorName + ".");
                return;
            }

            object target = GetMonsterById(_targetId);
            if (!GetMonsterRuntimeBool(target, "HasBurstWindow"))
            {
                Fail("Burst Window was not active when Mira's payoff turn started.");
                return;
            }

            RecordPass(
                "Role payoff turn",
                "Actor=Mira Role=" + SafeText(surface != null && surface.CurrentActor != null ? surface.CurrentActor.RoleLabel : string.Empty) +
                " Skill=" + SafeText(surface != null && surface.CurrentActor != null ? surface.CurrentActor.SkillLabel : string.Empty) +
                " Target=" + SafeText(_targetName) +
                " Window=" + SafeText(GetMonsterRuntimeString(target, "BurstWindowLabel")) + ".");
            AdvanceTo(ProofStep.QueuePayoff, "Mira is ready to cash the window.");
        }

        private void QueuePayoff()
        {
            if (!IsPlayerTurn())
            {
                return;
            }

            if (!_boot.TryTriggerBattleAction("skill"))
            {
                Fail("Failed to queue Mira Weak Point.");
                return;
            }

            AdvanceTo(ProofStep.CapturePayoffPreview, "Queued Mira skill.");
        }

        private void CapturePayoffPreview()
        {
            if (!IsTargetSelect())
            {
                return;
            }

            PrototypeBattleUiSurfaceData surface = _boot.GetBattleUiSurfaceData();
            PrototypeBattleUiTargetSelectionData target = surface != null ? surface.TargetSelection : null;
            if (target == null || !target.IsActive)
            {
                return;
            }

            _payoffPreview = SafeText(target.ExpectedEffectText);
            _payoffPostEffect = SafeText(target.PostEffectText);
            _payoffRoleHint = SafeText(target.SkillHintText);
            _payoffThreat = SafeText(target.ThreatSummaryText);
            string combined = _payoffPreview + " | " + _payoffPostEffect + " | " + _payoffRoleHint + " | " + _payoffThreat;
            if (!ContainsText(_payoffPreview, "Expected:") || !ContainsText(_payoffPreview, "Burst"))
            {
                Fail("Mira payoff preview did not show expected Burst damage. Preview=" + _payoffPreview + ".");
                return;
            }

            if (!ContainsText(_payoffPostEffect, "Consumes Burst Window"))
            {
                Fail("Mira payoff preview did not show window consumption. PostEffect=" + _payoffPostEffect + ".");
                return;
            }

            if (!ContainsAny(combined, "Mira", "Weak Point", "Role payoff"))
            {
                Fail("Mira payoff preview did not carry role payoff text. Readback=" + combined + ".");
                return;
            }

            RecordPass(
                "Payoff preview visible",
                "Action=Weak Point Target=" + SafeText(target.TargetLabel) +
                " Preview=" + _payoffPreview +
                " PostEffect=" + _payoffPostEffect +
                " RoleHint=" + _payoffThreat + ".");
            AdvanceTo(ProofStep.ResolvePayoff, "Payoff preview captured.");
        }

        private void ResolvePayoff()
        {
            object target = GetMonsterById(_targetId);
            if (target == null)
            {
                Fail("Proof target disappeared before Mira payoff resolved. TargetId=" + SafeText(_targetId) + ".");
                return;
            }

            bool resolved = (bool)InvokeNonPublicMethod(GetWorldView(), "TryResolveTargetSelection", target);
            if (!resolved)
            {
                Fail("Mira payoff target selection returned false. Target=" + SafeText(_targetName) + ".");
                return;
            }

            object updatedTarget = GetMonsterById(_targetId);
            bool windowStillOpen = GetMonsterRuntimeBool(updatedTarget, "HasBurstWindow");
            bool targetDefeated = GetPropertyValue<bool>(updatedTarget, "IsDefeated") || GetPropertyValue<int>(updatedTarget, "CurrentHp") <= 0;
            PrototypeBattleUiSurfaceData surface = _boot.GetBattleUiSurfaceData();
            _payoffActualLog = FindRecentEvidence(surface, "cashed Burst Window", PrototypeBattleEventKeys.BurstWindowConsumed);
            if (windowStillOpen)
            {
                Fail("Mira payoff resolved but the Burst Window was still active.");
                return;
            }

            if (!HasMeaningfulValue(_payoffActualLog))
            {
                Fail("Mira payoff consumed the window but no recent log/event confirmed it.");
                return;
            }

            RecordPass(
                "Payoff log and consume",
                "Target=" + SafeText(_targetName) +
                " Preview=" + _payoffPreview +
                " ActualLog=" + SafeText(_payoffActualLog) +
                " ClearConsume=" + (targetDefeated ? "Target defeated; window cleared" : "Window consumed after hit") + ".");
            AdvanceTo(ProofStep.RegressionSanity, "Mira consumed Burst Window.");
        }

        private void RegressionSanity()
        {
            RuntimeUiSkinBridge bridge = UnityEngine.Object.FindFirstObjectByType<RuntimeUiSkinBridge>();
            PrototypeDebugHUD debugHud = UnityEngine.Object.FindFirstObjectByType<PrototypeDebugHUD>();
            BattleUiSkinDefinition battleSkin = BattleUiSkinProvider.CurrentBattleSkin;
            bool topStripFallback = battleSkin == null || battleSkin.GetTopStripSlot() == null;
            bool openedInventory = _boot.TryToggleInventorySurface();
            bool inventoryOpen = _boot.IsInventorySurfaceOpen;
            PrototypeRpgInventorySurfaceData inventory = _boot.GetInventorySurfaceData();
            bool inventoryReadOnly = inventory != null && inventory.IsReadOnly;
            bool hudBlocksInput = debugHud != null && debugHud.ShouldBlockDungeonInput;
            _boot.CloseInventorySurface();
            bool inventoryClosed = !_boot.IsInventorySurfaceOpen;

            if (bridge == null || !topStripFallback || !openedInventory || !inventoryOpen || !inventoryReadOnly || !hudBlocksInput || !inventoryClosed)
            {
                Fail(
                    "UI/modal/skin sanity failed." +
                    " RuntimeUiSkinBridge=" + (bridge != null) +
                    " TopStripFallback=" + topStripFallback +
                    " OpenedInventory=" + openedInventory +
                    " InventoryOpen=" + inventoryOpen +
                    " InventoryReadOnly=" + inventoryReadOnly +
                    " HudBlocksInput=" + hudBlocksInput +
                    " InventoryClosed=" + inventoryClosed + ".");
                return;
            }

            RecordPass(
                "UI/modal/skin regression sanity",
                "RuntimeUiSkinBridge=present TopStripFallback=True InventoryOpen=True InventoryReadOnly=True HudBlocksDungeonInput=True InventoryClosed=True.");
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
            InvokePublicMethod(worldView, "TryInteractCurrentDungeonRoomBeat");
        }

        private bool IsPlayerTurn()
        {
            return SafeText(_boot != null ? _boot.BattlePhaseLabel : string.Empty) == "Player Turn";
        }

        private bool IsTargetSelect()
        {
            return SafeText(_boot != null ? _boot.BattlePhaseLabel : string.Empty) == "Target Select";
        }

        private object GetFirstLivingBattleMonster()
        {
            return InvokeNonPublicMethod(GetWorldView(), "GetFirstLivingBattleMonster");
        }

        private object GetMonsterById(string monsterId)
        {
            return string.IsNullOrEmpty(monsterId) ? null : InvokeNonPublicMethod(GetWorldView(), "GetMonsterById", monsterId);
        }

        private string GetMonsterRuntimeString(object monster, string propertyName)
        {
            object runtime = GetPropertyValue<object>(monster, "RuntimeState");
            return runtime != null ? SafeText(GetPropertyValue<string>(runtime, propertyName)) : "None";
        }

        private bool GetMonsterRuntimeBool(object monster, string propertyName)
        {
            object runtime = GetPropertyValue<object>(monster, "RuntimeState");
            return runtime != null && GetPropertyValue<bool>(runtime, propertyName);
        }

        private PrototypeBattleUiEnemyData FindEnemy(PrototypeBattleUiSurfaceData surface, string monsterId)
        {
            if (surface == null || surface.EnemyRoster == null || string.IsNullOrEmpty(monsterId))
            {
                return null;
            }

            for (int i = 0; i < surface.EnemyRoster.Length; i++)
            {
                PrototypeBattleUiEnemyData enemy = surface.EnemyRoster[i];
                if (enemy != null && enemy.MonsterId == monsterId)
                {
                    return enemy;
                }
            }

            return null;
        }

        private string FindRecentEvidence(PrototypeBattleUiSurfaceData surface, params string[] needles)
        {
            if (surface == null)
            {
                return string.Empty;
            }

            if (surface.MessageSurface != null && surface.MessageSurface.RecentLogs != null)
            {
                for (int i = 0; i < surface.MessageSurface.RecentLogs.Length; i++)
                {
                    string log = surface.MessageSurface.RecentLogs[i];
                    if (ContainsAny(log, needles))
                    {
                        return log;
                    }
                }
            }

            if (surface.RecentEvents != null)
            {
                for (int i = surface.RecentEvents.Length - 1; i >= 0; i--)
                {
                    PrototypeBattleEventRecord record = surface.RecentEvents[i];
                    if (record == null)
                    {
                        continue;
                    }

                    string text = SafeText(record.EventKey) + " | " + SafeText(record.Summary) + " | " + SafeText(record.ShortText);
                    if (ContainsAny(text, needles))
                    {
                        return text;
                    }
                }
            }

            return string.Empty;
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
            Debug.Log("[Batch78_1Proof][" + status + "] " + checkpoint + " :: " + detail);
        }

        private void AdvanceTo(ProofStep nextStep, string reason)
        {
            _step = nextStep;
            _stepStartedAt = EditorApplication.timeSinceStartup;
            Debug.Log("[Batch78_1Proof] Step -> " + _step + " :: " + reason);
        }

        private void Fail(string detail)
        {
            RecordFail("Combat core runtime proof", detail);
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

            _summary.AppendLine("[Batch78_1Proof] Summary");
            _summary.AppendLine("- Branch=B minimal readback seam tuning");
            _summary.AppendLine("- Encounter=" + SafeText(_encounterName));
            _summary.AppendLine("- Target=" + SafeText(_targetName));
            _summary.AppendLine("- Intent=" + SafeText(_intentRead));
            _summary.AppendLine("- SetupPreview=" + SafeText(_setupPreview));
            _summary.AppendLine("- SetupPostEffect=" + SafeText(_setupPostEffect));
            _summary.AppendLine("- SetupThreat=" + SafeText(_setupThreat));
            _summary.AppendLine("- WindowSummary=" + SafeText(_windowSummary));
            _summary.AppendLine("- SetupActualLog=" + SafeText(_setupActualLog));
            _summary.AppendLine("- PayoffPreview=" + SafeText(_payoffPreview));
            _summary.AppendLine("- PayoffPostEffect=" + SafeText(_payoffPostEffect));
            _summary.AppendLine("- PayoffRoleHint=" + SafeText(_payoffRoleHint));
            _summary.AppendLine("- PayoffThreat=" + SafeText(_payoffThreat));
            _summary.AppendLine("- PayoffActualLog=" + SafeText(_payoffActualLog));
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

        private static bool HasMeaningfulValue(string value)
        {
            return !string.IsNullOrEmpty(value) && value != "None" && value != "Unknown";
        }

        private static string SafeText(string value)
        {
            return string.IsNullOrEmpty(value) ? "None" : value;
        }

        private static bool EqualsIgnoreCase(string left, string right)
        {
            return string.Equals(left, right, StringComparison.OrdinalIgnoreCase);
        }

        private static bool ContainsText(string value, string needle)
        {
            return !string.IsNullOrEmpty(value) &&
                   !string.IsNullOrEmpty(needle) &&
                   value.IndexOf(needle, StringComparison.OrdinalIgnoreCase) >= 0;
        }

        private static bool ContainsAny(string value, params string[] needles)
        {
            if (string.IsNullOrEmpty(value) || needles == null)
            {
                return false;
            }

            for (int i = 0; i < needles.Length; i++)
            {
                if (ContainsText(value, needles[i]))
                {
                    return true;
                }
            }

            return false;
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
