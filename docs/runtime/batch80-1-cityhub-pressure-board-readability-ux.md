# Batch 80.1 CityHub Pressure Board Readability UX

## Goal

Batch80 proved that the selected CityHub / selected-city board had the right result-pressure data. Batch80.1 makes that board easier to scan without changing the underlying truth.

Selected branch: `Branch A`

- The data was present.
- The issue was density, ordering, internal labels, and repeated lines.
- No new world, combat, result, route, inventory, scene, skin, or layout system was added.

## Readability Changes

- `Latest:` now uses player-facing result language such as `Cleared: Stability Run` instead of internal result keys such as `run_clear`.
- `Changed:` now starts with compact pressure-board evidence: stock delta, pressure delta, and readiness delta.
- `Next:` remains sourced from existing CityHub/outcome follow-up data.
- `Ready:` uses compact readiness copy such as `Ready: warning | recovery 1 day | party idle | route available`.
- Selected CityHub presentation now shows the board in this order: `Latest`, `Why`, `Changed`, `Next`, `Ready`, `Route`.
- Duplicate selected-board lines were removed from the CityHub presentation consumer. The board no longer shows a dense `Pressure Board:` sentence and then repeats `Latest`, `Changed`, `Next`, and readiness below it.

## Manual UX Checklist

Manual screenshot QA checklist:

- `docs/ui/cityhub-pressure-board-ux-checklist.md`

Checklist coverage:

- selected CityHub before a run
- selected CityHub after Stability Run return
- blocked/recovery state
- ready state
- route recommendation state
- five-question visibility
- clipping/overlap
- next action and ExpeditionPrep affordance clarity

## Targeted Proof

- Runner: `Batch80WorldResultPressureBoardProofRunner.RunBatch80WorldResultPressureBoardProof`
- Log: `unity-batch80-1-board-proof.log`
- Result: `PASS`

Captured readable board sample:

- `Latest: Cleared: Stability Run | Returned mana_shard x16 | Party Stable`
- `Changed: Stock +16 mana_shard | Pressure Urgent -> Stable | Readiness Ready -> Recovering | Streak 0 -> 1`
- `Next: Stabilize for 1 day before the next push.`
- `Ready: warning | recovery 1 day | party idle | route available`

## Regression Proof

- Compile: `PASS`, log `unity-batch80-1-compile.log`
- Batch10 smoke: `PASS`, log `unity-batch80-1-smoke.log`
- CityHub -> ExpeditionPrep re-entry continuity remains `PASS` through Batch10.
- ResultPipeline -> WorldSim board refresh remains `PASS` through Batch10.
- World return chain causal summary remains `PASS` through Batch10.

## Performance / Scope

- No ResultPipeline field expansion.
- No new gameplay mechanics or content.
- No UI skin work.
- No large layout redesign.
- No `OnGUI`, mouse-move, `Update`, or no-op selection heavy rebuild was added.
- The change is limited to compact readback string assembly and selected CityHub presentation ordering.
