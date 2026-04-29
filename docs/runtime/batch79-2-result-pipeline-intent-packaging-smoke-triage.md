# Batch 79.2 ResultPipeline Intent Packaging Smoke Triage

## Selected Branch

- Selected branch: `fix DungeonRun -> ResultPipeline intent packaging before Batch80`
- Scope kept: no world pressure board work, no UI skin/layout replacement, no combat formula change, no route content expansion, and no pending-spoils policy change.

## Failure Being Triaged

- Previous Batch10 failure: `FAIL :: DungeonRun -> ResultPipeline intent packaging`
- The run state carried the correct objective, relevance, and risk/reward context after run clear.
- The public result/readback path replaced those intent fields with key encounter, growth/loot, or world writeback summaries.
- Root cause: `ExpeditionResult` and `PostRunResolutionInput` did not explicitly carry mission intent fields, so public reconstruction had to infer them from nearby result text.

## What Changed

- `PostRunResolutionInput`, `ExpeditionResult`, and `PrototypeDungeonRunResultContext` now carry:
  - `MissionObjectiveText`
  - `MissionRelevanceText`
  - `RiskRewardContextText`
- DungeonRun post-run input now reads those fields from `ExpeditionRunState` and only falls back to prep-surface text when the run state is missing meaningful intent text.
- Route confirmation now keeps the confirmed `ExpeditionPlan` available for the later DungeonRun and ResultPipeline bridge.
- `ResultPipeline.BuildExpeditionOutcome(ExpeditionResult)` centralizes public outcome reconstruction and preserves the intent package.
- `ManualTradeRuntimeState.CreatePublicExpeditionOutcome(...)` now delegates to the ResultPipeline helper instead of duplicating stale mapping logic.
- Batch10 smoke world-return logic now invokes the current `BootEntry` return path instead of reflecting against the retired private appflow coordinator field.

## Targeted Proof

- Command: `Batch79_2ResultPipelineIntentPackagingProofRunner.RunBatch79_2ResultPipelineIntentPackagingProof`
- Log: `unity-batch79-2-packaging-proof.log`
- Result: `PASS`
- Proof marker: `[Batch79_2Proof] PASS :: Package/consumer fields stable. Objective=Stability Run objective Relevance=Best route for stabilizing City A's next dispatch window. RiskReward=Rest Path | Stability Run | Low Risk`

## Batch10 Smoke Result

- Command: `Batch10SmokeValidationRunner.RunBatch10SmokeValidation`
- Log: `unity-batch79-2-smoke.log`
- Result: `PARTIAL`
- Fixed checkpoint: `PASS :: DungeonRun -> ResultPipeline intent packaging`
- Additional post-fix passes:
  - `ResultPipeline -> WorldSim board refresh`
  - `WorldSim -> CityHub recent impact coherence`
  - `WorldSim -> CityHub decision relevance carry-through`
  - `World return chain causal summary`
- Remaining later failure:
  - `FAIL :: CityHub -> ExpeditionPrep re-entry continuity`
  - observed stage was `ExpeditionPrep`, selected city was `city-a`, and route/city-impact text existed on route-facing fields
  - top prep continuity fields were still `None`, so this should be handled as the next CityHub/ExpeditionPrep continuity triage rather than as ResultPipeline packaging debt

## Regression Proof

- Compile: `PASS`, log `unity-batch79-2-compile.log`
- Batch79.1 route scenario proof: `PASS`, log `unity-batch79-2-route-proof.log`
- Batch78.1 combat core proof: `PASS`, log `unity-batch79-2-combat-proof.log`
- `git diff --check`: expected CRLF warnings only after the local Windows edit set; no whitespace errors were introduced.

## Recommendation

- Treat the Batch79.2 ResultPipeline intent-packaging seam as closed.
- Do not claim full Batch10 green.
- The next honest smoke blocker is `CityHub -> ExpeditionPrep re-entry continuity`, where returned city decision text reaches some prep route fields but not the top continuity fields expected by the smoke runner.
