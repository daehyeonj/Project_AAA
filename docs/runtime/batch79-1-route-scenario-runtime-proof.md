# Batch 79.1 Route Scenario Runtime Proof + Smoke Triage

## Selected Branch

- Selected branch: `C - ResolveCoreLoop timeout was caused by a stale smoke wait condition`
- Route proof path: `city-a -> dungeon-alpha -> safe`
- Scenario: `Stability Run`
- Scope kept: no new route content, gameplay system, UI skin work, combat formula change, pending-spoils policy change, or world-pressure-board work.

## What Changed

- Added editor-only proof runner: `Batch79_1RouteScenarioRuntimeProofRunner.RunBatch79_1RouteScenarioRuntimeProof`.
- Narrowed Batch10 smoke triage: `ResolveCoreLoop` now checks `PrototypeDungeonRunShellSurfaceData.IsEventChoiceVisible` and `IsPreEliteChoiceVisible` instead of relying only on retired bootstrap booleans that stay false.
- No runtime gameplay rule changed.

## Runtime Proof Result

- Command: `Batch79_1RouteScenarioRuntimeProofRunner.RunBatch79_1RouteScenarioRuntimeProof`
- Log: `unity-batch79-1-route-proof.log`
- Result: `PASS`

## Captured Player-Facing Evidence

- Route card:
  - `Rest Path | Stability Run`
  - `Choose when: Choose it when City A needs a clean shard refill and you want the next dispatch window to stay steady`
  - `Reward: Reliable shards, stronger recovery, clean monarch finish`
  - `Party fit: Best with Bulwark or Salvager-style crews that convert steady rooms into reliable haul`
  - `Combat: Expect slime-heavy sustain fights, stronger shrine value, and a steadier elite setup`
- Commit summary:
  - `SelectedRoute=Rest Path | Low Risk`
  - `RoutePreview=Rest Path | Stability Run`
  - `LaunchGate=Commit allowed. Launch window is clear.`
  - `StartBattleWatch=Rest Path | Combat: Expect slime-heavy sustain fights, stronger shrine value, and a steadier elite setup`
- Dungeon readback:
  - `Route Plan=Rest Path | Stability Run`
  - `Route Risk=Safer | Slime-heavy / Recover-friendly`
  - `Battle Watch=Rest Path | Combat: Expect slime-heavy sustain fights, stronger shrine value, and a steadier elite setup`
  - `Next=Reach Elite`
- Encounter popover:
  - `RoutePlan=Rest Path | Stability Run | Low Risk | Combat: Expect slime-heavy sustain fights, stronger shrine value, and a steadier elite setup | Follow-up: Usually leaves the next dispatch cleaner instead of forcing another hard reset.`
- Final payoff consumer:
  - static consumer proof passed: `OpenRunBattleResultPopover -> BuildSelectedRouteScenarioPayoffText(outcomeKey) -> PrototypeDungeonBattleResultPopoverData.RoutePlanText`
  - static result-panel proof passed: `PrototypePresentationShell.DungeonRun -> BuildDungeonScenarioPayoffLine(resultContext) -> Scenario Payoff`

## Batch10 Smoke Triage

- Command: `Batch10SmokeValidationRunner.RunBatch10SmokeValidation`
- Log: `unity-batch79-1-smoke.log`
- Result: `FAIL`, but no longer at the Batch79 `ResolveCoreLoop` timeout.
- Timeout triage result: `FIXED`
- Passed after the fix:
  - `Boot -> MainMenu`
  - `MainMenu -> WorldSim`
  - `WorldSim -> CityHub`
  - representative content catalog
  - `CityHub -> ExpeditionPrep`
  - `ExpeditionPrep -> DungeonRun launch`
  - `DungeonRun -> BattleScene intent carry-through`
  - `DungeonRun -> BattleScene -> DungeonRun return`
  - full route progression reached run clear/result packaging instead of timing out after the first battle return
- New later failure:
  - `FAIL :: DungeonRun -> ResultPipeline intent packaging`
  - The smoke now reaches result packaging and reports a mismatch between the run-state objective/relevance/risk-reward fields and the current `ExpeditionOutcome` result/readback summaries.
  - This was not caused by Batch79 route scenario text. Batch79.1 did not alter ResultPipeline, outcome writeback, pending spoils, combat rules, or world mutation logic.

## Regression Notes

- Batch78.1 combat payoff logic: unchanged.
- Inventory modal target-status guard: unchanged.
- Runtime skin bridge: unchanged.
- World selection cache path: unchanged.
- Pending Run Spoils / extraction policy: unchanged.
- UI shape changed?: no runtime UI shape change; editor proof and smoke harness only.

## Performance Notes

- No new per-frame scenario rebuild was added.
- The new proof runner is editor-only.
- The Batch10 fix reads an already-built dungeon shell surface during the existing smoke tick; it does not add runtime player-facing work.

## Recommendation

- Batch79.1 route scenario proof is complete.
- The old Batch10 `ResolveCoreLoop` timeout should no longer block route-scenario work.
- Do not claim full Batch10 green yet because a later ResultPipeline packaging checkpoint is now red.
- Project lead can choose either:
  - proceed to Batch80 if the later ResultPipeline smoke failure is accepted as unrelated pre-existing smoke debt
  - open a narrow ResultPipeline smoke/contract triage before Batch80 if full Batch10 green is required
