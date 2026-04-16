# Project AAA Architecture Entry

Project AAA is a Unity vertical slice prototype that turns city pressure into expedition decisions, turns expedition decisions into a dungeon run, and feeds the run result back into the city/world state.

## Core Loop

`MainMenu -> WorldSim -> CityHub -> ExpeditionPrep -> DungeonRun -> BattleScene -> DungeonRun -> ResultPipeline -> WorldSim`

## Runtime Reality Lock

- Batch 14 source of truth: the current manual-test runtime surface is `grid dungeon` exploration plus `standard JRPG battle`.
- Use that runtime reality for validation, diagnostics, and documentation until the post-batch-20 manual evaluation says otherwise.
- Earlier panel/lane-oriented ideas may still exist as design intent or backlog direction, but they are not the current implementation claim.

## Intent vs Runtime

- Current runtime surface: grid-room dungeon traversal that hands off into a standard command/target JRPG battle scene.
- Vertical slice core purpose: make the city/guild problem, expedition reason, battle/result consequences, and world return legible as one causal loop.
- Post-batch-20 manual evaluation remains pending. Before that review, prefer reality-lock, observability, and intent clarity work over surface pivots.

## Batch 20 Exit Review

- Batch 20 is the vertical slice exit review batch.
- Use `docs/vertical-slice-exit-review.md`, `docs/manual-eval-checklist.md`, and `docs/post-slice-backlog.md` as the primary handoff artifacts for post-batch-20 manual evaluation.
- Current automated evidence does not show a single confirmed golden-path blocker, but final direction fit still depends on human play evaluation.

## Batch 22 Post-Slice Entry

- Batch 22 starts post-slice work by freezing the Ready golden path as a baseline before widening content scope.
- Use `docs/post-slice-baseline.md` for the current baseline summary and representative-chain inventory.
- Use `docs/content/README.md` for the first content authoring seam and fallback expectations.

## Batch 23 Authoring Proof

- Batch 23 proves the batch 22 data seam is reusable by carrying a second representative chain through the same registry/adapter path.
- Keep the Ready golden path behavior anchored on the baseline chain while using `docs/post-slice-baseline.md` and `docs/content/README.md` to track which additional chains are canonical data paths versus fallback-only coverage.

## Batch 24 Shared Authoring

- Batch 24 keeps the two representative chains but splits shared route/reward/impact semantics out of the chain assets.
- Use the chain assets for mission/dungeon/room identity, and use the shared meaning assets for reusable route bias plus outcome/city-impact interpretation.

## Batch 25 Shared Encounter/Battle Authoring

- Batch 25 keeps the same two representative chains but moves encounter-profile and battle-setup meaning out of the chain-local handoff logic.
- Use the chain assets for city/dungeon/room identity, the shared route/outcome assets for expedition meaning, and the shared encounter/battle assets for battle-entry context plus enemy-group interpretation.

## Batch 26 Shared Outcome/Economy Authoring

- Batch 26 keeps the same two representative chains but extends shared outcome meaning into the canonical result/world/city return path.
- Use chain assets for mission/dungeon identity, shared route/outcome assets for expedition meaning, shared encounter/battle assets for battle-entry meaning, and shared outcome meaning for city-impact plus recommendation-shift interpretation after world return.

## Batch 27 Shared City-Side Authoring

- Batch 27 keeps the same two representative chains but moves city-side bottleneck/opportunity/recommendation rationale out of chain-local CityHub string assembly.
- Use chain assets for mission/dungeon identity, shared route/outcome assets for expedition meaning, shared encounter/battle assets for battle-entry meaning, shared outcome meaning for result return, and shared city-decision meaning assets for CityHub/ExpeditionPrep-facing interpretation.

## Batch 28 Representative Chain #3

- Batch 28 keeps the same runtime surface and proves the existing post-slice rail can carry a third representative chain without chain-specific runtime branching.
- Use `TryGetChain(cityId, dungeonId)` for primary city-side meaning, and use route-aware lookup (`TryGetChainForRoute` / `TryGetCanonicalRoute`) for representative route variants that share the same city+dungeon pair.

## Batch 29 Authoring QA

- Batch 29 adds an editor/helper validation rail for representative content authoring.
- Use `GoldenPathAuthoringValidationRunner` to validate representative chain completeness, shared-reference integrity, route-variant ownership expectations, and visible fallback coverage before broadening content scope.

## Batch 30 Supported Safe Variant

- Batch 30 closes the last surfaced authoring warning by promoting `city-b -> dungeon-beta -> safe` onto the canonical route-variant rail.
- Batch 31 turns that supported route variant into a real CityHub/ExpeditionPrep opportunity by letting non-primary chain assets opt into surfacing through `SurfaceAsOpportunityVariant`.
- Batch 32 uses the same surfacing seam to expose `city-a -> dungeon-alpha -> risky`, proving surfaced opportunity variety can expand without adding route-specific runtime branches.
- Batch 33 adds editor-only surfaced-matrix and quick-open tooling so post-slice authoring work can inspect surfaced/canonical/fallback state without touching runtime logic.
- Batch 34 extends that surfaced-matrix tooling to cross-check actual CityHub/ExpeditionPrep consumer visibility, proving the formerly warning-only `city-b -> dungeon-beta -> safe` path is not only canonical but also truly surfaced.
- Batch 35 adds explicit surfaced-status buckets plus a compact representative-chain status summary so authoring audits can distinguish canonical-hidden content, surfaced fallback regressions, and surfaced override pressure without changing runtime selection behavior.
- Batch 36 officializes the current surfaced opportunity portfolio and extends editor tooling to show prep-linkage plus recommendation-linkage for surfaced routes, so post-slice audits can reason about what is actually player-facing instead of inferring it from authored rows alone.
- Batch 37 deepens surfaced-portfolio tooling with a surfaced-only resolution trace plus a `MeaningfullyDistinctSurfaced` maturity count, so post-slice audits can tell whether the current player-facing set is both consumer-visible and varied enough before choosing between more surfacing work and tooling work.
- Batch 38 deepens authoring-operations tooling with explicit per-route issue drill-down plus shared-asset-path tracing, so content authors can inspect canonical/surfaced/fallback/override state without chasing ids by hand through the repo.
- Batch 39 adds a draft-scaffold helper for chain authoring throughput, so new opportunity work can start from a copied canonical chain plus preserved shared refs without promoting that draft into the runtime surface too early.
- Batch 40 adds draft-promotion readiness reporting so authors can tell whether a helper-generated draft is actually promotable on the current canonical/surfaced rail or whether that route key is already occupied.
- Batch 41 adds draft-promotion context quick-open tooling so blocked drafts can open their canonical owner plus linked shared assets without manual repo hunting.
- Batch 43 aligns representative-validator surface labels with the surfaced-matrix reality, so primary player-facing chains no longer look hidden in authoring output when the current surfaced portfolio is already full.
- Batch 44 adds a surfaced-expansion gate summary so post-slice audits can answer “close out target, align matrix, or go tooling-first next?” from one editor/tooling log instead of manually reconciling multiple summaries.
- Use `docs/post-slice-batch-status.md` when you need the shortest GitHub/GPT-friendly snapshot of the current post-slice state without reading the longer baseline/content notes first.
- Batch 46 strengthens draft-promotion preflight with explicit supported-rail slot counts and resolver-key inventory, so operators can tell the current Alpha/Beta surface is saturated without pretending a blocked draft still has a hidden open slot.
- Batch 47 adds draft-retarget helper flow on top of readiness/preflight/context, so a blocked draft can produce or point at a non-colliding off-rail hidden-canonical candidate without widening the current player-facing safe/risky rail.
- Batch 48 adds hidden-canonical promotion flow on top of the retarget helper, so an off-rail draft can be promoted into canonical `Resources` content without widening the current player-facing safe/risky rail.
- Treat that safe route as supported content, not as a tolerated hardcoded exception.

## Read This First

1. `AGENTS.md`
2. `docs/architecture/thread-ownership.md`
3. `docs/architecture/flow-contracts.md`
4. `docs/architecture/validation-matrix.md`
5. The thread-local markdown next to the code you are touching

## Key Source Files

| Thread | Start here | Supporting detail |
| --- | --- | --- |
| Architecture | `docs/architecture/*` | `AGENTS.md` |
| Appflow | `Assets/_Game/Scripts/Bootstrap/AppFlowContracts.cs` | `Assets/_Game/Scripts/Bootstrap/AppFlowCoordinator.cs`, `Assets/_Game/Scripts/Bootstrap/APPFLOW_CONTRACT.md` |
| WorldSim | `Assets/_Game/Scripts/World/ManualTradeRuntimeState.cs` | `Assets/_Game/Scripts/World/WorldBoardReadModels.cs`, `Assets/_Game/Scripts/World/StaticPlaceholderWorldView.WorldBoardReadModel.cs`, `Assets/_Game/Scripts/World/WORLDSIM_READMODEL.md` |
| CityHub | `Assets/_Game/Scripts/World/CityDecisionReadModels.cs` | `Assets/_Game/Scripts/World/CityDecisionModelBuilder.cs`, `Assets/_Game/Scripts/World/CITYHUB_DECISION_LAYER.md` |
| ExpeditionPrep | `Assets/_Game/Scripts/Bootstrap/ExpeditionPrepContracts.cs` | `Assets/_Game/Scripts/Bootstrap/ExpeditionPrepModelBuilder.cs`, `Assets/_Game/Scripts/Bootstrap/ExpeditionLaunchCoordinator.cs`, `Assets/_Game/Scripts/Bootstrap/EXPEDITION_PREP_CONTRACT.md`, `Assets/_Game/Scripts/World/StaticPlaceholderWorldView.ExpeditionPrep.cs` |
| DungeonRun | `Assets/_Game/Scripts/Dungeon/ExpeditionRunContracts.cs` | `Assets/_Game/Scripts/Dungeon/DungeonRunStateFactory.cs`, `Assets/_Game/Scripts/Dungeon/StaticPlaceholderWorldView.RunState.cs`, `Assets/_Game/Scripts/Dungeon/StaticPlaceholderWorldView.DungeonRun.cs`, `Assets/_Game/Scripts/Dungeon/DUNGEON_RUN_CONTRACT.md` |
| BattleScene | `Assets/_Game/Scripts/Rpg/Battle/PrototypeBattleContracts.cs` | `Assets/_Game/Scripts/Rpg/Battle/PrototypeCombatDomainData.cs`, `Assets/_Game/Scripts/Dungeon/StaticPlaceholderWorldView.BattleContracts.cs`, `Assets/_Game/Scripts/Rpg/Battle/BATTLE_EXECUTION_CONTRACT.md` |
| ResultPipeline | `Assets/_Game/Scripts/Results/ResultPipeline.cs` | `Assets/_Game/Scripts/Results/ExpeditionOutcome.cs`, `Assets/_Game/Scripts/Results/WorldDelta.cs`, `Assets/_Game/Scripts/Results/OutcomeReadback.cs`, `Assets/_Game/Scripts/World/ManualTradeRuntimeState.cs` |

## Canonical Handoffs

- `MainMenu -> WorldSim`: `AppFlowObservedSnapshot.WorldSelection`
- `WorldSim -> CityHub`: `WorldBoardReadModel.Selection`, `CityStatusReadModel`, `CityDecisionReadModel`
- `CityHub -> ExpeditionPrep`: `CityDecisionReadModel` plus `ExpeditionPrepReadModel`
- `ExpeditionPrep -> DungeonRun`: confirmed `ExpeditionPlan`
- `DungeonRun -> BattleScene`: `BattleHandoffPayload` carrying `PrototypeBattleRequest`
- `BattleScene -> DungeonRun`: `BattleReturnPayload` carrying `PrototypeBattleResolution`
- `DungeonRun -> ResultPipeline`: `PrototypeRpgRunResultSnapshot` plus `ExpeditionRunState`
- `ResultPipeline -> WorldSim`: `WorldDelta` and `OutcomeReadback`

## Design Rules

- Physical file location does not automatically define thread ownership.
- `Bootstrap` currently hosts both shell logic and some ExpeditionPrep/Appflow contracts. Treat the contract owner named in `docs/architecture/*` as canonical.
- `World` currently hosts both runtime truth and CityHub decision contracts. Treat `ManualTradeRuntimeState` as truth owner and `CityDecisionReadModel` as the city-side interpretation layer.
- `BootEntry.cs` and `StaticPlaceholderWorldView*.cs` are integration hot spots, not license to add new owners there.

## Placeholder vs Promotion Candidate

### Leave as placeholder for now

- Hardcoded sample data in `WorldData.cs` and resource factories
- String-heavy label properties exposed from `BootEntry.cs`
- Debug-only presentation routes in `PrototypeDebugHUD.cs`
- Large integration partials in `StaticPlaceholderWorldView*.cs` where extraction would be high risk

### Promote as canonical architecture candidates

- `AppFlowStage`, `AppFlowContext`, `AppFlowObservedSnapshot`
- `WorldBoardReadModel` and child world/city/dungeon/road read models
- `CityDecisionReadModel` and its signal/recommendation types
- `ExpeditionPrepReadModel`, `LaunchReadiness`, `ExpeditionPlan`
- `ExpeditionRunState`, `BattleHandoffPayload`, `BattleReturnPayload`
- `PrototypeBattleRequest`, `PrototypeBattleRuntimeState`, `PrototypeBattleResolution`
- `ExpeditionOutcome`, `WorldDelta`, `OutcomeReadback`

## Default Verification

1. Unity batch compile
2. Relevant validator / tooling summary for the touched authoring rail
3. One primary-thread smoke path only when runtime-facing flow changed or static/tooling evidence is not enough
4. One adjacent handoff check when the touched change crosses a runtime contract
5. Architecture docs updated with the code

For editor-only tooling, validation, or draft-helper batches, compile plus the relevant validator/tooling summary is the default minimum; smoke can be deferred until a runtime-facing batch needs it.

If you only have time for one code pass, keep the contracts and the docs aligned before touching more gameplay logic.
