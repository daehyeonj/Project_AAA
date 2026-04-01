# Batch-5 Callgraph

Current repo note:

- The filename is historical. The repo itself is now at `DungeonRun-Batch-15` per `Assets/_Game/Scripts/Core/PrototypeLocalization.cs:39`.
- The flows below reflect the current code, including `DispatchPolicy` and `PreEliteChoice`.

## WorldSim Entry Flow

- Flow name: `Boot -> MainMenu -> WorldSim`
- Entry point: `Assets/_Game/Scripts/Bootstrap/BootEntry.cs`
- Calls:
  - `Awake()` at `BootEntry.cs:305` creates `new StaticPlaceholderWorldView(_resources)`, then calls `EnsureDebugHUD()` and `EnsureWorldCameraController()`.
  - `Update()` at `BootEntry.cs:327` waits for boot delay, then `ChangeState(GameStateId.MainMenu)`.
  - `Update()` handles main menu confirm, then `ChangeState(GameStateId.WorldSim)`.
  - While in `WorldSim`, `Update()` calls `_worldView.UpdateAutoTick(Time.deltaTime)` and `HandleSelectionInput()`.
- Important fields:
  - `BootEntry._gameState`
  - `BootEntry._worldView`
  - `BootEntry._debugHud`
- Notes:
  - `HandleWorldSimEconomyInput()` at `BootEntry.cs:578` owns `T`, `R`, `Y`, `P`, `G`, `X`.
  - `ChangeState()` at `BootEntry.cs:540` updates background, camera, and world visibility.

## City Select To Planner Flow

- Flow name: `WorldSim city select -> Dispatch Planner`
- Entry point:
  - `BootEntry.HandleSelectionInput()`
  - `BootEntry.HandleWorldSimEconomyInput()` at `BootEntry.cs:578`
- Calls:
  - Mouse click -> `_worldView.SelectAtScreenPosition(Camera.main, mouse.position.ReadValue())`
  - `StaticPlaceholderWorldView.SelectAtScreenPosition(...)` at `StaticPlaceholderWorldView.cs:221` -> `SetSelected(marker)`
  - `X` key -> `_worldView.TryEnterSelectedCityDungeon(Camera.main)`
  - `TryEnterSelectedCityDungeon(...)` at `StaticPlaceholderWorldView.DungeonRun.cs:1177`
  - `ResetDungeonRunPresentationState()` at `StaticPlaceholderWorldView.DungeonRun.cs:5315`
  - `RefreshDispatchRecommendation()` at `StaticPlaceholderWorldView.DungeonRun.cs:2424`
  - If a recommendation exists, `TryTriggerRouteChoice(_recommendedRouteId)` at `StaticPlaceholderWorldView.DungeonRun.cs:1215`
- Important fields:
  - `StaticPlaceholderWorldView._selectedMarker`
  - `_currentHomeCityId`
  - `_currentDungeonId`
  - `_dungeonRunState`
  - `_recommendedRouteId`
- Notes:
  - Planner entry requires a selected city, linked dungeon, and idle party.
  - Planner state is `DungeonRunState.RouteChoice`.

## Planner Recommendation Refresh Flow

- Flow name: `Planner recommendation refresh`
- Entry point:
  - `TryEnterSelectedCityDungeon()` at `StaticPlaceholderWorldView.DungeonRun.cs:1177`
  - `CycleDispatchPolicyForCity()` at `StaticPlaceholderWorldView.DungeonRun.cs:2433`
  - route hover or selection changes that feed `RefreshExpectedNeedImpact()`
- Calls:
  - `RefreshDispatchRecommendation()` at `StaticPlaceholderWorldView.DungeonRun.cs:2424`
  - `GetRecommendedRouteId(...)` at `StaticPlaceholderWorldView.DungeonRun.cs:2226`
  - `GetBaseRecommendedRouteId(...)` at `StaticPlaceholderWorldView.DungeonRun.cs:2175`
  - `ApplyPolicyBiasToRecommendedRoute(...)` at `StaticPlaceholderWorldView.DungeonRun.cs:2197`
  - `BuildRecommendationReasonText(...)` at `StaticPlaceholderWorldView.DungeonRun.cs:2301`
  - `BuildExpectedNeedImpactText(...)` at `StaticPlaceholderWorldView.DungeonRun.cs:2313`
- Important fields:
  - `_dispatchReadinessByCityId`
  - `_dispatchRecoveryDaysRemainingByCityId`
  - `_daysSinceLastDispatchByCityId`
  - `_consecutiveDispatchCountByCityId`
  - `_dispatchPolicyByCityId`
  - `_recommendedRouteId`
  - `_recommendedRouteReason`
  - `_expectedNeedImpactText`
- Notes:
  - Need pressure still comes from `BuildNeedPressureText(cityId)` -> `GetNeedPressureTextForStock(GetCityManaShardStock(cityId))` at `StaticPlaceholderWorldView.DungeonRun.cs:1916`.
  - Recommendation is now `base recommendation + policy bias`, not base recommendation alone.

## Planner Input To Run Start Flow

- Flow name: `Planner input -> dispatch confirm -> Explore`
- Entry point:
  - keyboard: `HandleDungeonRouteChoiceInput()` at `StaticPlaceholderWorldView.DungeonRun.cs:4708`
  - mouse: `PrototypeDebugHUD.DrawRouteChoiceSection()` at `PrototypeDebugHUD.cs:437`
- Calls:
  - `Q` -> `TryCycleCurrentDispatchPolicy()` at `StaticPlaceholderWorldView.DungeonRun.cs:4721`
  - `1/2` -> `TryTriggerRouteChoice(...)` at `StaticPlaceholderWorldView.DungeonRun.cs:4727`
  - `Enter` or HUD button -> `TryConfirmRouteChoice()` at `StaticPlaceholderWorldView.DungeonRun.cs:4741`
  - `TryConfirmRouteChoice()` at `StaticPlaceholderWorldView.DungeonRun.cs:830`
  - `_runtimeEconomyState.BeginDungeonRun(_currentHomeCityId, _currentDungeonId)` at `StaticPlaceholderWorldView.DungeonRun.cs:843`
  - `StartDungeonRunForRoute(template, partyId)` at `StaticPlaceholderWorldView.DungeonRun.cs:849`
- Important fields:
  - `_selectedRouteChoiceId`
  - `_selectedRouteId`
  - `_selectedRouteLabel`
  - `_followedRecommendation`
  - `_preRunManaShardStock`
  - `_preRunNeedPressureText`
  - `_preRunDispatchReadinessText`
- Notes:
  - `StartDungeonRunForRoute()` at `StaticPlaceholderWorldView.DungeonRun.cs:5122` snapshots stock and readiness, then calls `ApplyDispatchReadinessOnDispatch(_currentHomeCityId)` at line `5209`.
  - The run begins in `DungeonRunState.Explore`.

## Run Loop Flow

- Flow name: `Explore -> EventChoice / PreEliteChoice / Battle / ResultPanel`
- Entry point: `StaticPlaceholderWorldView.UpdateDungeonRun()` at `StaticPlaceholderWorldView.DungeonRun.cs:1228`
- Calls:
  - route choice path -> `HandleDungeonRouteChoiceInput(...)`
  - event choice path -> `HandleEventChoiceInput(...)`
  - pre-elite path -> `TryTriggerPreEliteChoiceFromTile()` at `StaticPlaceholderWorldView.DungeonRun.cs:4936`
  - pre-elite open -> `OpenPreEliteChoicePanel()` at `StaticPlaceholderWorldView.DungeonRun.cs:4959`
  - battle and explore handlers continue until `FinishDungeonRun(...)`
- Important fields:
  - `_dungeonRunState`
  - `_battleState`
  - `_runResultState`
  - `_selectedEventChoiceId`
  - `_selectedPreEliteChoiceId`
  - `_eliteBonusRewardPending`
- Notes:
  - `DungeonRunState` now includes `PreEliteChoice`.
  - `PrototypeDebugHUD.OnGUI()` at `PrototypeDebugHUD.cs:90` treats pre-elite as part of overlay mode.

## Finish And World Return Flow

- Flow name: `Clear / Defeat / Retreat -> result snapshot -> WorldSim`
- Entry point:
  - `FinishDungeonRun(...)` at `StaticPlaceholderWorldView.DungeonRun.cs:5232`
  - `ConsumeDungeonRunExitRequest()` at `StaticPlaceholderWorldView.DungeonRun.cs:1303`
- Calls:
  - `FinishDungeonRun(...)` writes result fields and sets `_dungeonRunState = DungeonRunState.ResultPanel`
  - `FinishDungeonRun(...)` -> `_runtimeEconomyState.ResolveDungeonRun(...)` at `StaticPlaceholderWorldView.DungeonRun.cs:5282`
  - `ResolveDungeonRun(...)` writes loot into city stock at `ManualTradeRuntimeState.cs:1641`
  - result panel return input sets `_pendingDungeonExit = true`
  - `BootEntry.Update()` at `BootEntry.cs:382` polls `_worldView.ConsumeDungeonRunExitRequest()`
  - success -> `ChangeState(GameStateId.WorldSim)`
- Important fields:
  - `_resultStockBefore`
  - `_resultStockAfter`
  - `_resultNeedPressureBeforeText`
  - `_resultNeedPressureAfterText`
  - `_resultDispatchReadinessBeforeText`
  - `_resultDispatchReadinessAfterText`
  - `_pendingDungeonExit`
- Notes:
  - Result data now includes route, recommendation-followed flag, pre-elite choice, elite reward identity, and dispatch delta strings.
  - World return is not direct inside `FinishDungeonRun(...)`; it is deferred through `_pendingDungeonExit`.

## Reset Flow

- Flow name: `World reset(R) -> economy reset -> dungeon/planner state reset`
- Entry point:
  - `BootEntry.HandleWorldSimEconomyInput()` at `BootEntry.cs:590`
  - `StaticPlaceholderWorldView.ResetRuntimeEconomy()` at `StaticPlaceholderWorldView.cs:278`
- Calls:
  - `_worldView.ResetRuntimeEconomy()`
  - `_runtimeEconomyState.Reset()` at `ManualTradeRuntimeState.cs:249`
  - `ResetDungeonRunSystems()`
  - readiness dictionaries reset at `StaticPlaceholderWorldView.DungeonRun.cs:1052`
  - `ResetDungeonRunPresentationState()` at `StaticPlaceholderWorldView.DungeonRun.cs:5315`
- Important fields:
  - economy totals and stock dictionaries in `ManualTradeRuntimeState`
  - readiness and policy dictionaries in `StaticPlaceholderWorldView.DungeonRun.cs`
  - planner/result presentation fields in `ResetDungeonRunPresentationState()`
- Notes:
  - Reset clears both runtime economy and dungeon planner state.
  - Default city policy after reset is `DispatchPolicyState.Balanced`.

## Recommendation And Readiness Field Map

- Flow name: `Recommendation / readiness anchor list`
- Entry point: `Assets/_Game/Scripts/Dungeon/StaticPlaceholderWorldView.DungeonRun.cs`
- Calls:
  - `BuildNeedPressureText(...)` at `1916`
  - `EnsureDispatchReadinessEntry(...)` at `1923`
  - `BuildDispatchPolicyText(...)` at `2025`
  - `BuildDispatchRecoveryProgressText(...)` at `2066`
  - `BuildRecoveryAdviceText(...)` at `2088`
  - `GetBaseRecommendedRouteId(...)` at `2175`
  - `ApplyPolicyBiasToRecommendedRoute(...)` at `2197`
  - `BuildRecommendationReasonText(...)` at `2301`
  - `RefreshDispatchRecommendation()` at `2424`
  - `ApplyDispatchReadinessOnDispatch(...)` at `2471`
- Important fields:
  - `_dispatchReadinessByCityId`
  - `_dispatchRecoveryDaysRemainingByCityId`
  - `_daysSinceLastDispatchByCityId`
  - `_consecutiveDispatchCountByCityId`
  - `_lastDispatchReadinessChangeByCityId`
  - `_dispatchPolicyByCityId`
- Notes:
  - Batch-5A work that targets recommendation or policy should start here first, not in combat code.

## First 5 Functions To Read For Batch-5A

- Flow name: `Most useful starting points`
- Entry point: `Assets/_Game/Scripts/Dungeon/StaticPlaceholderWorldView.DungeonRun.cs`
- Calls:
  - `TryEnterSelectedCityDungeon()` at `1177`
  - `RefreshDispatchRecommendation()` at `2424`
  - `GetBaseRecommendedRouteId()` at `2175`
  - `ApplyPolicyBiasToRecommendedRoute()` at `2197`
  - `ApplyDispatchReadinessOnDispatch()` at `2471`
- Important fields:
  - `_currentHomeCityId`
  - `_currentDungeonId`
  - `_recommendedRouteId`
  - `_dispatchPolicyByCityId`
  - `_dispatchReadinessByCityId`
- Notes:
  - If result-side changes are also needed, read `FinishDungeonRun()` at `5232` immediately after these five.
