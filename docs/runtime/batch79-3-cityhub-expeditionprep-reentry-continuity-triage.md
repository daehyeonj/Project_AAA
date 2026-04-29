# Batch 79.3 CityHub -> ExpeditionPrep Re-entry Continuity Triage

## Selected Branch

- Branch A primary: runtime AppFlow re-entry prep context was stale after CityHub returned from a result.
- Branch B narrow follow-up: once the runtime cache gap was fixed, the Batch10 route-prompt expectation needed to accept the current launch-gate contract without weakening returned-result assertions.

## Why This Was The Honest Next Step

- Batch79.2 closed `DungeonRun -> ResultPipeline intent packaging`, then full Batch10 advanced to the next real checkpoint: `CityHub -> ExpeditionPrep re-entry continuity`.
- Batch80 world result-pressure board depends on result return, CityHub state, selected city/dungeon continuity, prep re-entry, route recommendation, and launch context all agreeing.
- This batch stayed a contract/smoke triage pass and did not add world pressure board UI, route content, combat systems, inventory systems, or UI skin/layout work.

## Failure Audit

- Failing smoke step: `Batch10SmokeValidationRunner.ValidatePrepReentryContinuity()` / `CityHub -> ExpeditionPrep re-entry continuity`.
- Expected: after result return and CityHub re-entry, `AppFlowContext.ActiveExpeditionPlan.PrepReadModel` should carry `city-a`, `dungeon-alpha`, top recent impact, recommendation summary/reason, and concise why-now text while the prep surface remains coherent.
- Actual before the fix: the observed stage reached `ExpeditionPrep` and `SelectedCity=city-a`, but AppFlow's prep read model still reported `TargetDungeon=None`, `ImpactSummary=None`, `ImpactHint=None`, `Recommendation=None`, `RecommendationReason=None`, and `WhyNow=None`.
- Root cause: `StaticPlaceholderWorldView.GetCurrentExpeditionPrepReadModel()` only rebuilt the active prep read model while `_dungeonRunState == DungeonRunState.RouteChoice`; the current board path opens ExpeditionPrep through `_isExpeditionPrepBoardOpen` and no longer relies on that dungeon-run state. The visible prep surface was coherent, but the AppFlow context consumed stale/default prep data.

## Re-entry Contract

- Selection identity: result-return re-entry preserves selected city `city-a` and target dungeon `dungeon-alpha` in the prep read model and surface data.
- Result evidence: returned impact, recommendation summary/reason, and why-now evidence live in `ExpeditionPrepReadModel`; route aftermath text may echo the last result/route without becoming a second truth source.
- Dispatch readiness: `LaunchReadiness` remains the canonical route-prompt/commit-gate source, including allowed-with-warning states after a result return.
- Route options: the prep board still exposes the Alpha safe/risky route options and current operating-scenario route text.
- Post-run reveal: no new reveal bypass was added; the proof returns through the existing world/city/prep public path.

## Fix Summary

- Cached the current `ExpeditionPrepReadModel` at explicit prep rebuild points: open ExpeditionPrep board, selected route change, and dispatch policy change.
- Updated Batch10's re-entry assertion narrowly so route description can prove returned aftermath while route prompt can remain launch readiness/gate text.
- Added a targeted Batch79.3 proof runner that simulates a completed run, returns to CityHub, re-enters ExpeditionPrep, and verifies prep read model, surface data, route options, latest result evidence, recommendation, why-now, and launch readiness.

## Files Changed

- `Assets/_Game/Scripts/World/StaticPlaceholderWorldView.ExpeditionPrepSurface.cs`
- `Assets/_Game/Scripts/Dungeon/StaticPlaceholderWorldView.DungeonRun.cs`
- `Assets/_Game/Scripts/Editor/Batch10SmokeValidationRunner.cs`
- `Assets/_Game/Scripts/Editor/Batch79_3CityHubExpeditionPrepReentryProofRunner.cs`
- `docs/runtime/batch79-3-cityhub-expeditionprep-reentry-continuity-triage.md`
- `docs/architecture/flow-contracts.md`
- `docs/architecture/validation-matrix.md`
- `Assets/_Game/Scripts/Bootstrap/EXPEDITION_PREP_CONTRACT.md`
- `docs/post-slice-batch-status.md`

## Validation

- Compile: `PASS`, log `unity-batch79-3-compile.log`.
- Targeted re-entry proof: `PASS`, log `unity-batch79-3-reentry-proof.log`.
- Batch10 smoke: `PASS`, log `unity-batch79-3-smoke.log`.
- ResolveCoreLoop: `PASS`; the previous Batch79.1 timeout remains fixed.
- ResultPipeline packaging: `PASS`; the Batch79.2 intent packaging checkpoint remains stable.
- CityHub -> ExpeditionPrep: `PASS`; previous failure is closed.
- Batch79.1 route scenario proof: `PASS`, log `unity-batch79-3-route-proof.log`.
- Batch78.1 combat proof: `PASS`, log `unity-batch79-3-combat-proof.log`.
- Batch79.2 packaging proof: `PASS`, log `unity-batch79-3-packaging-proof.log`.
- Inventory modal guard: `PASS` through Batch78.1 UI/modal/skin regression sanity.
- Runtime skin bridge: `PASS` through Batch78.1 UI/modal/skin regression sanity.
- World selection performance path: `UNCHANGED`; no new per-frame prep, party, inventory, asset, or result-pipeline rebuild was introduced.

## Performance Impact

- The fix refreshes the AppFlow prep read model only on explicit player/state transition points: prep board open, route choice change, and dispatch policy change.
- It does not rebuild prep state on every city selection, GUI frame, mouse move, or world board read.

## Can Batch80 Begin?

- Yes.
- Batch10 now passes through the prior `CityHub -> ExpeditionPrep re-entry continuity` blocker and completes, while ResultPipeline packaging, route scenario proof, combat proof, inventory modal sanity, runtime skin bridge sanity, and the 77.1 world-selection performance guard remain intact.
