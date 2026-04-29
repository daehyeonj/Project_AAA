# Validation Matrix

This matrix defines the minimum checks that should be run after architecture or gameplay changes. Start with compile, then run the smallest relevant validator/tooling summary for the touched thread. Smoke stays required for runtime-facing changes, but editor-only tooling/helper batches may defer smoke when compile plus validator/tooling evidence is sufficient.

## Repo-Wide Baseline

1. Unity batch compile
2. Relevant validator / tooling summary for the touched thread or authoring rail
3. Relevant thread smoke check when runtime-facing flow changed or static/tooling evidence is not enough
4. One adjacent handoff smoke check when the touched change crosses a runtime contract
5. Docs/code sync check for `AGENTS.md` and `docs/architecture/*`

## Batch 20 Exit Review Guard

- Treat batch 20 as an exit review batch, not a feature batch.
- Confirm compile plus the full smoke entry point `Batch10SmokeValidationRunner.RunBatch10SmokeValidation`.
- Cross-check the smoke output against `docs/vertical-slice-exit-review.md` and `docs/manual-eval-checklist.md` so the automated story and the manual-evaluation story do not diverge.

## Batch 22 Post-Slice Baseline Guard

- Treat batch 22 as a baseline-freeze plus dataization batch, not a loop-redesign batch.
- Confirm compile plus the full smoke entry point `Batch10SmokeValidationRunner.RunBatch10SmokeValidation`.
- Cross-check the smoke output against `docs/post-slice-baseline.md` and `docs/content/README.md` so the representative chain's data-driven path stays aligned with the Ready golden path.

## Runtime Reality Lock

- Batch 14 source of truth for validation is the current manual-test runtime surface: `grid dungeon` plus `standard JRPG battle`.
- Do not treat older panel/lane combat ideas as the current validation target unless a later manual evaluation explicitly reclassifies the runtime.
- Until the post-batch-20 manual review, prefer checkpoints that verify causal clarity on the current surface over speculative future-surface assertions.

## Thread Matrix

| Thread | Minimum smoke check | Success signal |
| --- | --- | --- |
| Architecture | Open `AGENTS.md`, `docs/architecture/README.md`, `thread-ownership.md`, `flow-contracts.md`, `validation-matrix.md` and confirm the referenced code files still exist | Repo docs match real code paths and owner rules |
| Appflow | `Boot -> MainMenu -> WorldSim`, then select a city and confirm stage changes to `CityHub` or `ExpeditionPrep` when appropriate | `AppFlowCoordinator` transitions line up with observed runtime state |
| WorldSim | Enter `WorldSim`, tick a day, inspect selection, and confirm board counters/logs still update | `ManualTradeRuntimeState` updates and `BuildWorldBoardReadModel()` remains readable |
| CityHub | Select a city and confirm decision signals/recommended action text still appear from `CityStatusReadModel.Decision` | CityHub reads decision signals before falling back to raw labels |
| ExpeditionPrep | From a city selection, enter route choice/prep, verify route choices, readiness, blockers, and launch summary | `ExpeditionPrepReadModel`, `LaunchReadiness`, and `ExpeditionPlan` remain coherent |
| DungeonRun | Confirm a launch, enter the run, progress at least one room/event branch, and verify run-state summaries continue updating | `ExpeditionRunState` and `DungeonRunReadModel` stay in sync |
| BattleScene | Start an encounter, select a command/target, resolve the battle, and return | `PrototypeBattleRequest -> RuntimeState -> Resolution` chain remains intact |
| ResultPipeline | Finish a run, confirm `ExpeditionOutcome`, `WorldDelta`, and `OutcomeReadback` are produced and reflected back in world/city summaries | Result data reaches `ManualTradeRuntimeState` and returns to WorldSim/CityHub |

## Recommended Handoff Checks

### Appflow -> WorldSim

- `BootEntry` enters world mode using `AppFlowObservedSnapshot`
- `CurrentAppFlowStage` and visible shell state agree

### WorldSim -> CityHub

- Selecting a city exposes `CityStatusReadModel` and `CityDecisionReadModel`
- CityHub UI uses decision text rather than only raw world labels
- Batch 11 hardening: after the batch 10 `ResultPipeline -> WorldSim` fix, CityHub recent-impact summaries should prefer canonical `LatestResult.CityStatusChangeSummaryText` / `LatestResult.SummaryText`; treat `LastDispatchImpactText` as fallback or supporting detail only
- Confirm the selected city's top `RecentImpactSummary` and follow-up recommendation still line up with the same refreshed result that WorldSim just applied
- Batch 12 close-out: `WhyCityMattersText` should carry the same returned-result subject into the next-decision headline instead of collapsing to a generic post-run sentence
- Regression guard: the smoke runner now checks `World return chain causal summary` so the result -> world -> city explanation stays traceable end-to-end
- Batch 18 continuity guard: confirm CityHub now prefers `LatestResult.WorldReturnSummaryText` for the top `RecentImpactSummary`, and that the top recommendation reason still points at the same refreshed impact or changed bottleneck before the player moves toward ExpeditionPrep
- Batch 26 authoring guard: if `LatestResult.RecommendationShiftText` is present for a representative chain, confirm the top recent-impact hint or top recommendation reason still carries that same shared shift text instead of dropping back to chain-local fallback wording
- Batch 27 authoring guard: if a representative chain resolves a shared city-decision meaning asset, confirm `CityOpportunitySignal.WhyItMattersText`, the top recommendation reason, and `WhyCityMattersText` still carry that same shared rationale instead of dropping back to CityHub-local fallback text

### CityHub -> ExpeditionPrep

- Prep entry uses selected city/dungeon context plus decision signals
- Route choice/readiness are still explainable from prep contracts
- Batch 19 continuity guard: after the result-return loop, re-enter ExpeditionPrep and confirm the prep read model plus route-choice prompt still carry the selected city's refreshed `RecentImpactSummary`, top recommendation summary/reason, and why-now text before the player commits to the next launch
- Batch 21 clarity guard: confirm the returned `WhyNowText` no longer repeats the same decision clause and is short enough to fit the prep prompt without truncating the main reason for the next launch

### ExpeditionPrep -> DungeonRun

- Confirm launch freezes a confirmed `ExpeditionPlan`
- Run start carries the plan into `ExpeditionRunState`
- Batch 13 entry guard: treat `ExpeditionPlan` as the canonical launch payload for route rationale too, including `RiskRewardPreviewText`, so `DungeonRun` seed does not drift back to the live prep read model after confirmation
- Regression guard: the smoke runner now requires the confirmed route id, objective, why-now, expected usefulness, and risk/reward preview to match between `ExpeditionPlan` and `ExpeditionRunState`
- Batch 22 data guard: for the representative `city-a -> dungeon-alpha -> safe` chain, smoke should also confirm the launch crosses into `DungeonRun` with `ContentSource=data:*` rather than the fallback hardcoded route template
- Batch 23 authoring guard: smoke should also confirm the representative content catalog resolves both `city-a -> dungeon-alpha -> safe` and `city-b -> dungeon-beta -> risky` through `data:*` labels before broader content batches add a third chain
- Batch 24 shared-meaning guard: smoke should also confirm the representative content catalog resolves both chains with `shared:` route-meaning and outcome-meaning references instead of only inline chain text
- Batch 28 representative-chain guard: smoke should also confirm the representative content catalog resolves `city-a -> dungeon-alpha -> risky` through `data:*` plus shared route/outcome/city-decision/encounter/battle references, proving the route-variant chain uses the same canonical rail instead of the hardcoded fallback
- Batch 29 authoring-QA guard: run `GoldenPathAuthoringValidationRunner.RunAuthoringContentValidation` after representative-chain or shared-meaning authoring changes, and treat representative-chain missing references / route-variant ownership drift / hidden `fallback:hardcoded` use as authoring integrity failures rather than runtime-only cleanup
- Batch 30 supported-variant guard: `city-b -> dungeon-beta -> safe` is now a supported canonical route variant. If the validator reports a miss on `city-b::dungeon-beta::safe` or drops back to `fallback:hardcoded`, treat it as an authoring regression rather than acceptable cleanup debt.
- Batch 31 surfaced-opportunity guard: `city-b -> dungeon-beta -> safe` is now a surfaced route variant. Treat missing `SurfaceAsOpportunityVariant`, missing `RouteId=safe` in `CityDecisionReadModel.Opportunities`, or a non-`data:*` content source on that opportunity as a surfaced-content regression.
- Batch 32 surfaced-opportunity guard: `city-a -> dungeon-alpha -> risky` is now also a surfaced route variant. Treat missing `SurfaceAsOpportunityVariant`, missing `RouteId=risky` in `CityDecisionReadModel.Opportunities`, or a non-`data:*` content source on that opportunity as a surfaced-content regression.
- Batch 33 tooling guard: when surfaced-opportunity authoring changes, run both `GoldenPathAuthoringValidationRunner.RunAuthoringContentValidation` and `GoldenPathAuthoringValidationRunner.RunSurfacedOpportunityMatrixSummary`. Treat any authored Alpha/Beta route that reports `fallback:hardcoded` or drops from `cityhub+prep(primary|variant)` to `hidden-canonical` as an authoring-speed regression that should be fixed before adding more variety.
- Batch 34 surfaced-proof guard: for `city-b -> dungeon-beta -> safe` and later promoted variants, do not stop at asset-level surfacing intent. Treat `Surface=cityhub+prep(...)` with `Consumer=not-visible(actual)` or a non-`data:*` `ConsumerSource` in the surfaced-matrix summary as a surfaced-content regression that must be fixed before adding more variety.
- Batch 35 surfaced-status guard: when using the surfaced matrix or representative-chain summary, treat `FAIL:surfaced-using-fallback` and `FAIL:surfaced-consumer-mismatch` as stop-ship authoring regressions for promoted opportunities. Treat `WARN:canonical-but-not-surfaced` as hidden-canonical content that still needs a deliberate promotion decision, and treat `WARN:surfaced-with-chain-override` as cleanup pressure that must be documented before copying that route pattern again.
- Batch 36 surfaced-portfolio guard: when using the surfaced portfolio or surfaced matrix summary, treat `Prep=missing(actual)` on a surfaced route as a CityHub->ExpeditionPrep regression, and treat `Recommendation=city-only(actual)` on a surfaced route as a portfolio-alignment warning that must be understood before adding more surfaced variety. If the current authored Alpha/Beta route set is still the intended surface, `CanonicalButNotSurfaced=None` should remain true.
- Batch 37 surfaced-maturity guard: when using the surfaced portfolio or surfaced trace summary, treat `MeaningfullyDistinctSurfaced < 3` as a surfaced-portfolio thinness signal, and treat any surfaced route whose trace reports `Distinctness=near-duplicate:*` as a rationale-variety warning before opening another tooling-deepening batch. For the current Alpha/Beta surfaced set, the expected state is `MeaningfullyDistinctSurfaced=4`.
- Batch 38 tooling-deepening guard: when using the surfaced matrix, representative status, surfaced trace, or representative reference trace summaries, treat `Issues=source:fallback-hardcoded`, `Issues=consumer:fallback`, `Issues=prep:missing-link`, or `Issues=consumer:hidden` as authoring-operability regressions that should be resolved before widening the surfaced portfolio. Treat `Issues=override:*` as documented cleanup pressure rather than silent shared-authoring drift.
- Batch 39 validation guard: for editor-only draft/template/helper batches, compile plus `GoldenPathAuthoringValidationRunner.RunAuthoringContentValidation` plus the relevant tooling summary is now the default minimum. Only rerun smoke when the helper changes runtime-facing canonical content, opens a new surfaced player path, or leaves compile/validator/tooling state ambiguous.
- Batch 40 draft-promotion guard: when using the draft helper for surfaced-expansion planning, also run `GoldenPathAuthoringDraftHelper.RunDraftPromotionReadinessSummary`. Treat `blocked:resolver-key-already-owned` as proof that the current canonical/surfaced rail already owns that route key, and treat `blocked:requires-runtime-route-surface-expansion` as a sign that promotion would need a new surfaced-route seam instead of only moving JSON.
- Batch 41 helper-usable guard: when a draft is blocked or ambiguous, use `GoldenPathAuthoringDraftHelper.RunBatchTemplateDraftPromotionContextSummary` or the matching quick-open helper before widening the surfaced portfolio. Treat "blocked but no owner/context opened" as an authoring-operations gap, not as sufficient surfaced-expansion proof.
- Batch 43 surfaced-alignment guard: when the surfaced portfolio is already stable, treat validator wording drift as a tooling regression too. Primary chains that are already player-facing should report `Surface=cityhub+prep(primary)` instead of looking hidden, so the validator, surfaced matrix, and portfolio summary all describe the same reality before another surfaced-expansion batch opens.
- Batch 44 surfaced-gate guard: before opening another surfaced-expansion batch on the Alpha/Beta rail, run `GoldenPathAuthoringValidationRunner.RunSurfacedOpportunityExpansionGateSummary`. Treat `ExpansionGate=A:target-close-out` as proof that the tracked surfaced target still is not stable, treat `ExpansionGate=B:matrix-align` as proof that the wider surfaced matrix still has hidden/fallback/mismatch drift, and only treat `ExpansionGate=C:tooling-next` as evidence that the next honest bottleneck is tooling rather than another forced surfacing pass.
- Batch 45 draft-promotion guard: when using `GoldenPathAuthoringDraftHelper.RunDraftPromotionReadinessSummary`, do not treat a hidden draft as an automatic surfaced-expansion candidate. If the summary reports `BlockedByCanonicalOwner>0`, `PromotionRecommendation=retarget-the-draft-resolver-key-before-promotion`, or an inline `SurfacedExpansionGate=... ExpansionGate=C:tooling-next`, treat that as proof that the current Alpha/Beta surfaced rail is already full for that draft key and that the next honest move is retargeting or rail expansion rather than silently colliding with an existing surfaced route.
- Batch 46 draft-preflight guard: when draft promotion is still ambiguous after readiness/context, also run `GoldenPathAuthoringDraftHelper.RunDraftPromotionPreflightSummary`. Treat `OpenSupportedRailSlots=None`, `OpenSupportedResolverKeys=None`, or `SupportedRailFit=supported-slot-owned-and-current-rail-saturated` as explicit proof that the current Alpha/Beta surfaced rail has no honest open promotion slot left. In that state, the next honest move is retargeting beyond the current rail or widening the surfaced seam, not silently promoting the colliding draft.
- Batch 79 operating-scenario guard: for the selected `city-a -> dungeon-alpha -> safe/risky` pair, route scenario text should be visible in prep option cards, launch/dungeon readback, battle-result popover, and final-result readback without adding a new route content owner or a per-frame scenario rebuild.
- Batch 79.1 smoke-triage guard: smoke automation that resolves the dungeon core loop should read event/pre-elite visibility from `PrototypeDungeonRunShellSurfaceData` as the current dungeon shell surface, not only from retired bootstrap event/pre-elite booleans that can remain false for legacy-shell suppression.
- Batch 79.2 confirmed-plan guard: once a route choice is confirmed, keep the confirmed `ExpeditionPlan` available to DungeonRun and ResultPipeline instead of relying only on a live prep-surface fallback.

### DungeonRun -> BattleScene

- Pending encounter produces `BattleHandoffPayload`
- Battle request and UI surface match the run context
- Batch 14 reality lock: validate this boundary as a `grid dungeon room encounter -> standard JRPG battle` handoff, not as a lane/panel prototype
- Confirm battle entry keeps the expedition objective plus reward/risk context visible in `PrototypeBattleRequest`, `BattleHandoffPayload`, and battle-facing diagnostics/HUD
- Batch 25 authoring guard: for representative chains, confirm the content catalog resolves shared `EncounterProfileId` and `BattleSetupId` assets, and confirm battle entry still treats those as authoring-time meaning only rather than a rewrite of the standard JRPG combat rules

### BattleScene -> DungeonRun

- Battle resolution returns through `BattleReturnPayload`
- Run state updates last encounter/outcome text
- Batch 15 continuity guard: confirm `BattleReturnPayload` carries room type, objective, city-usefulness anchor, room progress, and next-goal context back into `DungeonRunReadModel.BattleReturnText` / `RecentOutcomeText`

### DungeonRun -> ResultPipeline

- Run completion produces a meaningful `ExpeditionOutcome`
- Run state snapshot remains readable for post-run analysis
- Batch 16 continuity guard: confirm `ResultPipeline` copies the run state's objective, mission-relevance anchor, risk/reward context, and room-path summary into `ExpeditionOutcome` plus `OutcomeReadback` before world return
- Batch 79.2 packaging guard: `PostRunResolutionInput`, `ExpeditionResult`, and `PrototypeDungeonRunResultContext` must carry `MissionObjectiveText`, `MissionRelevanceText`, and `RiskRewardContextText` from the run/prep intent package before falling back to key-encounter or world-writeback summaries.

### ResultPipeline -> WorldSim

- `WorldDelta` applies without null/missing references
- `OutcomeReadback` shows up in world/city follow-up summaries
- Batch 10 hardening: treat `ResultPipeline` as the owner and keep the `StaticPlaceholderWorldView.AppFlow.cs` adapter keyed to the canonical latest `ExpeditionOutcome` / `OutcomeReadback` source city, not only the current world selection or `_currentHomeCityId`
- Batch 79.2 smoke-return guard: world-return smoke should use the current `BootEntry` return path and accept either `WorldSim` or immediate observed `CityHub` when a selected city consumes the refreshed world result.
- Batch 79.2 public-outcome guard: public reconstruction from `ExpeditionResult` should go through `ResultPipeline.BuildExpeditionOutcome(ExpeditionResult)` so `ManualTradeRuntimeState` does not duplicate stale intent-field mapping.
- Confirm the returned city's `LatestResult.SourceCityId`, `LastDispatchImpactText`, and appflow applied marker all agree after world return
- Batch 17 continuity guard: confirm the refreshed `ExpeditionResultReadModel` keeps `MissionObjectiveText` and `MissionRelevanceText` from `OutcomeReadback`, and that `WorldReturnSummaryText` combines the applied city-status change with the mission relevance anchor before the next CityHub hop
- Batch 26 authoring guard: for representative chains, confirm the refreshed `ExpeditionResultReadModel` now also carries `OutcomeMeaningId`, `CityImpactMeaningText`, and `RecommendationShiftText`, and that `WorldReturnSummaryText` prefers the shared city-impact meaning when it is available

## Static Checks When Manual Play Is Not Available

- Run Unity batch compile
- `rg` the producer and consumer of any touched contract
- Confirm no second canonical type was introduced for the same boundary
- Confirm `BootEntry.cs` and `StaticPlaceholderWorldView*.cs` only changed as adapters/integration hosts when the task was not explicitly about those owners
- Update the relevant architecture docs in the same change

## Current High-Risk Files

- `Assets/_Game/Scripts/Bootstrap/BootEntry.cs`
- `Assets/_Game/Scripts/World/StaticPlaceholderWorldView.cs`
- `Assets/_Game/Scripts/Dungeon/StaticPlaceholderWorldView.DungeonRun.cs`
- `Assets/_Game/Scripts/Debug/PrototypeDebugHUD.cs`

When one of these changes, always run compile plus at least one handoff smoke check.
