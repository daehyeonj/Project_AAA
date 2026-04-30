# Shared Contracts And Handoff Map

This file is the repo-level registry for Project AAA shared contracts. It answers four questions:

1. Which type is canonical?
2. Which thread owns it?
3. Who produces it?
4. Is it truth, a derived read model, or an adapter/display contract?

## Contract Taxonomy

- `Source of truth`: mutable or canonical runtime data that other layers derive from
- `Derived read model`: stable, read-only projection assembled from truth
- `Shared handoff`: payload passed between threads as a contract boundary
- `Adapter`: wrapper or transport payload around another canonical contract
- `Display-only`: UI/HUD projection that should not become cross-thread truth

## Canonical Contract Registry

| Contract | Owner thread | Producer | Consumer | Truth status | Adapter/display note |
| --- | --- | --- | --- | --- | --- |
| `AppFlowStage` | Appflow | `AppFlowCoordinator` | `BootEntry`, presentation/debug readers | Shared handoff | Canonical logical stage. Do not replace with `GameStateId`. |
| `AppFlowObservedSnapshot` | Appflow | `StaticPlaceholderWorldView.BuildAppFlowSnapshot()` | `AppFlowCoordinator.Synchronize()` | Adapter | Snapshot transport from runtime threads into Appflow. |
| `AppFlowContext` | Appflow | `AppFlowCoordinator` | `BootEntry`, debug/presentation readers | Shared handoff | Session context, not gameplay truth. |
| `WorldBoardReadModel` | WorldSim | `StaticPlaceholderWorldView.BuildWorldBoardReadModel()` | `BootEntry`, `CityDecisionModelBuilder`, ExpeditionPrep builders | Derived read model | Canonical world board snapshot. |
| `CityStatusReadModel` | WorldSim | `BuildCityStatusReadModels()` in world view | CityHub, ExpeditionPrep | Derived read model | Canonical city status input. |
| `CityDecisionReadModel` | CityHub | `CityDecisionModelBuilder.Build(...)` | CityHub consumers, `ExpeditionPrepModelBuilder` | Derived read model | Canonical city decision surface. |
| `ExpeditionPrepReadModel` | ExpeditionPrep | `ExpeditionPrepModelBuilder.BuildReadModel(...)` | HUD/presentation, `AppFlowExpeditionPlan`, `ExpeditionLaunchCoordinator` | Derived read model | Canonical draft launch-prep contract. |
| `LaunchReadiness` | ExpeditionPrep | `ExpeditionPrepModelBuilder` | prep UI, launch coordinator, Appflow | Shared handoff | Canonical launch gate summary. |
| `ExpeditionPlan` | ExpeditionPrep | `ExpeditionLaunchCoordinator.BuildConfirmedPlan(...)` | DungeonRun, Appflow | Shared handoff | Canonical confirmed launch payload. |
| `ExpeditionRunState` | DungeonRun | `DungeonRunStateFactory` / `DungeonRunCoordinator` | Appflow, debug/presentation, result follow-up | Shared handoff | Canonical launched run contract. |
| `DungeonRunReadModel` | DungeonRun | `DungeonRunCoordinator.BuildReadModel(...)` | HUD/Appflow readers | Display-only | Projection of `ExpeditionRunState`. |
| `BattleHandoffPayload` | DungeonRun | run-state assembly in `StaticPlaceholderWorldView` | BattleScene, Appflow | Adapter | Carries `PrototypeBattleRequest` across the boundary. |
| `PrototypeBattleRequest` | BattleScene | `StaticPlaceholderWorldView.BattleContracts.cs` and battle factories | battle runtime/coordinator | Shared handoff | Canonical battle input. |
| `PrototypeBattleRuntimeState` | BattleScene | battle runtime snapshot builders | `PrototypeBattleViewModel`, battle UI | Source of truth | Canonical live battle state contract. |
| `PrototypeBattleResolution` | BattleScene | battle resolution builders | DungeonRun, ResultPipeline follow-up | Shared handoff | Canonical battle output. |
| `BattleReturnPayload` | DungeonRun | run-state assembly after battle | Appflow, DungeonRun readers | Adapter | Summary wrapper around battle output for run consumers. |
| `PostRunResolutionInput` | ResultPipeline | DungeonRun result bridge | `ResultPipeline.BuildExpeditionResult(...)` | Shared handoff | Carries post-run snapshot plus mission objective/relevance/risk intent fields into ResultPipeline. |
| `ExpeditionResult` | ResultPipeline | `ResultPipeline.BuildExpeditionResult(...)` | `ExpeditionOutcome`, `WorldWriteback`, `OutcomeReadback`, `ManualTradeRuntimeState` | Shared handoff | Packaged result payload. Preserve mission objective/relevance/risk before deriving public readbacks. |
| `ExpeditionOutcome` | ResultPipeline | `ResultPipeline.BuildExpeditionOutcome(...)` | `WorldDelta`, Appflow, WorldSim follow-up | Shared handoff | Canonical post-run expedition result. |
| `WorldDelta` | ResultPipeline | `ResultPipeline.BuildWorldDelta(...)` | `ManualTradeRuntimeState.ResolveDungeonRun(...)` | Shared handoff | Canonical world mutation payload. |
| `OutcomeReadback` | ResultPipeline | `ResultPipeline.BuildOutcomeReadback(...)` | WorldSim/CityHub/Appflow readers | Derived read model | Canonical follow-up summary after world application. |
| `ExpeditionResultReadModel` | WorldSim | world board read-model builder | WorldSim/CityHub readers | Derived read model | World-facing projection of result pipeline outputs. Do not treat as a replacement for `ExpeditionOutcome`. |
| `PrototypeBattleUiSurfaceData` | BattleScene | `BuildBattleUiSurfaceData()` | presentation/debug battle HUD | Display-only | UI projection only. |

## Canonicalization And Dedupe Rules

### Appflow vs shell state

- `AppFlowStage` is the canonical cross-thread stage model.
- `GameStateId` remains a shell/presentation state adapter used by `BootEntry` and scene visibility logic.
- Do not add a second logical stage enum without a dedicated migration batch.

### WorldSim vs CityHub

- `WorldBoardReadModel` and `CityStatusReadModel` are the canonical world-side read contracts.
- `CityDecisionReadModel` is the canonical city decision layer on top of those read models.
- `Selected*Label` properties in `BootEntry.cs` and `PrototypeDebugHUD.cs` are display adapters only.
- Batch 80 pressure-board summaries are display/readback aggregations over `WorldBoardReadModel`, `CityStatusReadModel.Decision`, `ExpeditionResultReadModel`, `OutcomeReadback`, and `LaunchReadiness`; they must not introduce a second result, pressure, or recommendation truth source.

### ExpeditionPrep

- `ExpeditionPrepReadModel` is the canonical draft launch-prep surface.
- `LaunchReadiness` and `PrepBlocker` are the canonical readiness/blocker explanation contracts.
- `ExpeditionPlan` is the canonical confirmed launch payload.
- Do not add a second planner screen model if `ExpeditionPrepReadModel` can be extended additively.

### DungeonRun

- `ExpeditionRunState` is the canonical launched run contract.
- `DungeonRunReadModel` is a display projection of that state.
- Keep battle handoff/return payloads as DungeonRun-owned adapters around BattleScene-owned contracts.

### BattleScene

- `PrototypeBattleRequest`, `PrototypeBattleRuntimeState`, and `PrototypeBattleResolution` are the canonical battle contracts.
- `BattleHandoffPayload` and `BattleReturnPayload` should carry or summarize those contracts, not replace them.
- `PrototypeBattleUiSurfaceData` is display-only.

### ResultPipeline

- `ExpeditionOutcome -> WorldDelta -> OutcomeReadback` is the canonical result chain.
- `PostRunResolutionInput -> ExpeditionResult -> ExpeditionOutcome` is the current result-packaging bridge before world writeback.
- `ExpeditionResultReadModel` is the world/read-model projection of that chain.
- Cached strings in runtime state or HUD should be derived from these contracts, not treated as new owners.
- Batch 79.2 continuity: mission objective, mission relevance, and risk/reward context must survive the `PostRunResolutionInput -> ExpeditionResult -> ExpeditionOutcome -> OutcomeReadback` path before any key-encounter or world-writeback fallback text is used.

## Handoff Flow Map

### MainMenu -> WorldSim

- Producer: `BootEntry` entering `WorldSim` with `StaticPlaceholderWorldView.BuildAppFlowSnapshot()`
- Contract: `AppFlowObservedSnapshot.WorldSelection`
- Owner thread: Appflow
- Consumer: `AppFlowCoordinator.TryEnterWorldSim(...)`

### WorldSim -> CityHub

- Producer: `WorldBoardReadModel` and `CityStatusReadModel.Decision`
- Contract: `WorldBoardReadModel.Selection`, `CityStatusReadModel`, `CityDecisionReadModel`
- Owner thread: WorldSim for raw status, CityHub for decision interpretation
- Consumer: CityHub presentation and expedition-intent builders
- Batch 18 continuity: CityHub should prefer `CityStatusReadModel.LatestResult.WorldReturnSummaryText` / mission-relevance carry-through when building `RecentImpactSummary`, `RecommendedActions`, and `WhyCityMattersText`, so the city panel explains what changed and why the next recommendation shifted
- Batch 26 normalization: when a representative chain resolves a shared outcome-meaning asset, `CityDecisionModelBuilder` should prefer its carried `RecommendationShiftText` / `CityImpactMeaningText` before falling back to older mission-relevance heuristics, so city-side hints stay authoring-driven instead of chain-local string glue
- Batch 27 normalization: when a representative chain resolves a shared city-decision meaning asset, `CityDecisionModelBuilder` should prefer its bottleneck/opportunity/recommendation rationale text for `CityBottleneckSignal`, `CityOpportunitySignal`, `RecommendedActions`, and `WhyCityMattersText` before falling back to older CityHub-only string assembly
- Batch 80 pressure-board guard: selected CityHub/selected-city board readback should answer what happened, why it mattered, what changed, next action, and readiness/re-entry using the existing world board, city decision, latest result, outcome readback, and launch readiness data. The board is a presentation aggregation only.

### CityHub -> ExpeditionPrep

- Producer: `CityDecisionModelBuilder` + ExpeditionPrep builders
- Contract: `CityDecisionReadModel`, `ExpeditionPrepReadModel`, `LaunchReadiness`
- Owner thread: CityHub for intent, ExpeditionPrep for prep surface
- Consumer: prep UI, launch coordinator, Appflow expedition plan context
- Batch 19 continuity: `ExpeditionPrepReadModel` should project the returned city's top recent impact plus top recommendation summary/reason into prep-facing why-now/decision text, so re-entry after `WorldSim -> CityHub` explains what changed and why this launch prompt is appearing now instead of collapsing to a generic route picker
- Batch 21 close-out: `ExpeditionPrepReadModel.WhyNowText` should stay concise enough for the prep prompt and avoid repeated decision clauses when it summarizes `CityDecisionReadModel` carry-through after world return
- Batch 22 post-slice data seam: representative-chain authoring data may override prep-facing objective/usefulness text through `GoldenPathContentRegistry`, but `ExpeditionPrepReadModel` remains the canonical runtime prep contract
- Batch 27 city-side seam: representative chains may now carry shared bottleneck/opportunity/recommendation rationale through `CityDecisionReadModel`, and `ExpeditionPrepReadModel` should consume that through existing recent-impact / recommendation / why-now fields rather than new chain-local display glue
- Batch 79 operating-scenario guard: prep route-option display may surface route-meaning scenario label, choose-when, party-fit, combat-plan, risk/reward, and follow-up text, but those strings remain display projections of `GoldenPathRouteMeaningDefinition` plus `ExpeditionPrepReadModel`; they are not a second prep truth source
- Batch 79.3 re-entry continuity: after result return, Appflow's `ActiveExpeditionPlan.PrepReadModel` must be refreshed on explicit prep-open, route-change, and dispatch-policy-change points so it carries the returned city/dungeon identity plus recent impact, recommendation, and why-now evidence. The route prompt may remain `LaunchReadiness` gate/warning text; returned decision evidence should be asserted through `ExpeditionPrepReadModel` and route aftermath echo instead of forcing the prompt to duplicate city-decision copy.
- Batch 83 second-run desire guard: second ExpeditionPrep may cache display-only readbacks for last-run change, party carry-forward, stability appetite, surge appetite, launch warning, and route-appetite recommendation, but those fields must be derived from `ExpeditionResult`, party/loadout state, `LaunchReadiness`, city pressure, and route-option surfaces. They must not become a second result, progression, recovery, or reward truth source.
- Batch 84 recovery-pressure guard: second ExpeditionPrep may expose launch-now vs recover-one-day readbacks and a `[T] Recover 1 Day` action, but the action must run the existing world day / dispatch recovery rail and rebuild prep readbacks from `LaunchReadiness`, city pressure, and route surfaces. Do not add a new fatigue system, fake penalty, ResultPipeline rewrite, or per-frame rebuild path to make waiting feel meaningful.

### ExpeditionPrep -> DungeonRun

- Producer: `ExpeditionLaunchCoordinator.BuildConfirmedPlan(...)`
- Contract: `ExpeditionPlan`
- Owner thread: ExpeditionPrep
- Consumer: `AppFlowDungeonRunContext.LaunchPlan`, `ExpeditionRunState`
- Batch 22 post-slice data seam: representative-chain authoring data may seed the `dungeon-alpha / safe` route identity and room sequence before the confirmed `ExpeditionPlan` enters `DungeonRun`; hardcoded templates remain fallback for uncovered routes and dungeons
- Batch 28 route-variant seam: representative route variants may now share a `cityId + dungeonId` primary chain while resolving their canonical route through `TryGetChainForRoute(...)` / `TryGetCanonicalRoute(...)`; keep city-side default wording on the primary chain and keep route-specific preview/usefulness on the route-aware path
- Batch 79 operating-scenario guard: confirmed launch and DungeonRun readbacks may keep the selected scenario plan visible from the existing route preview / fit / event preview strings; `ExpeditionPlan` and `ExpeditionRunState` remain the canonical handoff contracts

### DungeonRun -> BattleScene

- Producer: run-state/battle-contract assembly in `StaticPlaceholderWorldView`
- Contract: `BattleHandoffPayload`, `PrototypeBattleRequest`
- Owner thread: DungeonRun for the adapter, BattleScene for the request itself
- Consumer: battle runtime and Appflow battle context
- Batch 25 post-slice authoring seam: representative encounter rooms may now resolve `EncounterProfileId` plus `BattleSetupId` through `GoldenPathContentRegistry` before seeding `PrototypeBattleRequest`; hardcoded battle rosters remain fallback for uncovered or non-canonical encounters
- Batch 28 route-variant seam: if a representative chain shares a city+dungeon pair with another chain, battle-entry authoring should still resolve by `cityId + dungeonId + routeId` before falling back, so a second route does not collapse back into the primary chain's battle setup
- Batch 79 operating-scenario guard: battle result popovers may include a compact route-plan display line, but battle legality and resolution still come from BattleScene-owned request/runtime/resolution contracts

### BattleScene -> DungeonRun

- Producer: battle resolution builders
- Contract: `PrototypeBattleResolution`, `BattleReturnPayload`
- Owner thread: BattleScene for the resolution, DungeonRun for the adapter
- Consumer: `ExpeditionRunState.LastBattleReturn`, Appflow synchronization

### DungeonRun -> ResultPipeline

- Producer: dungeon completion flow in `StaticPlaceholderWorldView.DungeonRun.cs`
- Contract: `PrototypeRpgRunResultSnapshot`, `ExpeditionRunState`, `PostRunResolutionInput`, `ExpeditionResult`, then `ExpeditionOutcome`
- Owner thread: DungeonRun upstream, ResultPipeline downstream
- Consumer: `ResultPipeline.BuildExpeditionResult(...)` and `ResultPipeline.BuildExpeditionOutcome(...)`
- Batch 16 continuity: seed `ExpeditionOutcome` from canonical `ExpeditionRunState` mission context too, not only the run-result snapshot, so objective, city-usefulness anchor, risk/reward context, and run-path summary survive into `OutcomeReadback`
- Batch 79 operating-scenario guard: final run/result presentation can state whether the selected route scenario paid off, partially paid off, or missed, but it must derive that from the existing route summary plus result key instead of mutating world state directly
- Batch 79.2 intent-package guard: `MissionObjectiveText`, `MissionRelevanceText`, and `RiskRewardContextText` should be copied explicitly from DungeonRun run/prep intent into `PostRunResolutionInput` and `ExpeditionResult`; do not infer those fields from key encounter, growth/loot, or world writeback summaries.

### ResultPipeline -> WorldSim

- Producer: `ResultPipeline`
- Contract: `WorldDelta`, `OutcomeReadback`
- Owner thread: ResultPipeline
- Consumer: `ManualTradeRuntimeState.ResolveDungeonRun(...)` and WorldSim/CityHub follow-up readers
- Batch 17 continuity: WorldSim's `ExpeditionResultReadModel` should keep the returned `MissionObjectiveText`, `MissionRelevanceText`, and a world-return summary derived from `OutcomeReadback.CityStatusChangeSummaryText` so the board refresh explains both what changed and why it matters before CityHub consumes it
- Batch 26 normalization: representative chains should now carry `OutcomeMeaningId`, `CityImpactMeaningText`, and `RecommendationShiftText` through `ExpeditionOutcome -> WorldDelta -> OutcomeReadback -> ExpeditionResultReadModel`, so `WorldReturnSummaryText` and later CityHub hints can consume shared outcome authoring instead of only chain-local heuristics
- Batch 28 route-variant seam: when `ExpeditionOutcome` still knows the route id through the run snapshot, representative route variants should resolve shared outcome meaning by `cityId + dungeonId + routeId` before falling back to the primary city+dungeon chain
- Batch 79.2 public reconstruction: `ManualTradeRuntimeState` should call `ResultPipeline.BuildExpeditionOutcome(ExpeditionResult)` when it needs a public outcome from a packaged result, so mission intent fields stay aligned with the ResultPipeline owner.
- Batch 80 result-pressure handoff: world/city pressure-board copy may compactly combine `OutcomeReadback`, `WorldWriteback`, `ExpeditionResultReadModel`, city decision, and launch-readiness fields, but it must not expand the ResultPipeline contract or rebuild result summaries during UI layout/mouse/update loops.

## Adapter Boundaries To Keep Thin

- `BootEntry.cs`: shell-facing getters, stage mapping, and input delegation only
- `StaticPlaceholderWorldView.AppFlow.cs`: snapshot adapter from runtime truth into Appflow
- `BattleHandoffPayload` / `BattleReturnPayload`: adapters around battle contracts
- `DungeonRunReadModel` and `PrototypeBattleUiSurfaceData`: display-only projections

If a future change wants to add a new shared contract, extend this file first and only then add the type.
