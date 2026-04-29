# Batch 80 World Result-Pressure Board Proof

## Goal

Make the selected CityHub / selected-city world board explain the latest result-pressure loop without adding a new world pressure system.

The player-facing board now answers:

- What happened: latest route/result, returned loot, and party condition.
- Why it mattered: existing `CityDecisionReadModel.WhyCityMattersText`.
- What changed: need pressure/readiness deltas, streak delta, and returned stock evidence when present.
- What to do next: existing CityHub recommendation / outcome follow-up.
- Can I act now: dispatch readiness and ExpeditionPrep re-entry state.

## Data Policy

- Source contracts remain `ExpeditionResult -> ExpeditionOutcome -> WorldDelta -> OutcomeReadback`, `WorldBoardReadModel`, `CityStatusReadModel`, `CityDecisionReadModel`, and `LaunchReadiness`.
- Batch80 adds display/readback fields only on selected-city and priority-board surfaces.
- No new ResultPipeline fields, world economy mechanics, route content, combat mechanics, UI skin work, scene migration, or per-frame result rebuilds were added.

## Targeted Runtime Proof

- Runner: `Batch80WorldResultPressureBoardProofRunner.RunBatch80WorldResultPressureBoardProof`
- Log: `unity-batch80-board-proof.log`
- Result: `PASS`

Validated flow:

- Boot -> MainMenu -> WorldSim.
- Select `city-a`.
- Recruit party.
- Enter ExpeditionPrep.
- Select `safe` route.
- Confirm launch into DungeonRun.
- Simulate clear result with `mana_shard x16`.
- Return to selected CityHub.
- Validate pressure board copy.
- Re-enter ExpeditionPrep and confirm route options remain available.

Key proof markers:

- `PASS :: World return -> CityHub pressure board`
- `PASS :: Pressure board answers five questions`
- `PASS :: Readiness/re-entry`
- `PASS :: Targeted board proof completed`

Captured board example:

- `City A pressure board`
- `Latest Last run: Rest Path | Stability Run | Low Risk (run_clear) | Returned mana_shard x16 | Party Stable`
- `Changed Need pressure Urgent -> Stable | Dispatch readiness Ready -> Recovering | Streak 0 -> 1 | Stock +16 mana_shard`
- `Ready with warning: party idle, route available, recovery 1 day.`
- `Next Stabilize for 1 day before the next push.`

## Regression Proof

- Compile: `PASS`, log `unity-batch80-compile.log`
- Batch10 smoke: `PASS`, log `unity-batch80-smoke.log`
- Batch79.3 re-entry proof: `PASS`, log `unity-batch80-reentry-proof.log`
- Batch79.2 packaging proof: `PASS`, log `unity-batch80-packaging-proof.log`
- Batch79.1 route proof: `PASS`, log `unity-batch80-route-proof.log`
- Batch78.1 combat proof: `PASS`, log `unity-batch80-combat-proof.log`
- UI/modal/skin sanity: `PASS` through Batch78.1 proof (`RuntimeUiSkinBridge=present`, inventory modal read-only and closed cleanly)
- Whitespace: `git diff --check` reported only CRLF warnings

## Performance / Cache Sanity

- No `OnGUI`, mouse-move, `Update`, asset scan, or ResultPipeline rebuild path was added.
- The board is built through existing world result/read-model/selection/prep refresh paths.
- City selection after result was covered by the targeted proof returning to selected CityHub before reading the board.
