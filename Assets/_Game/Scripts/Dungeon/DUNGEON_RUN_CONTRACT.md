# DungeonRun Contract

## Runtime Surface Reality

- Batch 14 lock: the current manual-test dungeon surface is a `grid dungeon` with room-by-room traversal and encounters.
- Treat that as runtime truth for current validation and diagnostics.
- Broader surface-direction decisions stay open until the post-batch-20 manual evaluation.

## Input
- `ExpeditionPrep` confirms an `ExpeditionPlan`.
- `StaticPlaceholderWorldView.AppFlow` exposes that plan through `AppFlowDungeonRunContext.LaunchPlan`.
- `DungeonRun` now derives an explicit `ExpeditionRunState` from the confirmed plan plus live dungeon runtime fields.

## Source Of Truth
- `ExpeditionRunState` is the additive runtime contract for the current run.
- It records:
  - launch plan reference
  - current phase/status
  - room and encounter context
  - route and party summary
  - event / pre-elite choice results
  - pending battle handoff
  - latest battle return
  - result snapshot and recent run log entries
- `DungeonRunReadModel` is the display-facing projection used by HUD/AppFlow readers.

## Battle Handoff
- `BattleHandoffPayload` describes the encounter the run is entering:
  - dungeon / route / room / encounter ids
  - party summary
  - objective text
  - risk context
- `BattleReturnPayload` describes what came back from battle:
  - outcome key
  - encounter id / name
  - encounter room type plus objective / why-now / expected usefulness carry-through
  - room progress plus next-goal continuity for the resumed run
  - party condition and HP summary
  - loot summary
  - encounter cleared / elite defeated flags
  - copied `PrototypeBattleResultSnapshot`

## Result Path
- `FinishDungeonRun(...)` still builds `PrototypeRpgRunResultSnapshot` and `ExpeditionOutcome`.
- `AppFlowResultContext.ResolvedRunState` now carries the resolved `ExpeditionRunState` beside the existing `ExpeditionOutcome` / `OutcomeReadback`.
- Follow-up `ResultPipeline` or `BattleScene` work should prefer:
  1. `AppFlowDungeonRunContext.RunState`
  2. `AppFlowBattleContext.HandoffPayload` / `ReturnPayload`
  3. `AppFlowResultContext.ResolvedRunState`

## Implementation Notes
- The legacy dungeon partial still owns combat/exploration rules.
- `DungeonRunStateFactory` and `DungeonRunCoordinator` only assemble and project run state.
- This keeps the prototype additive while making `ExpeditionPlan -> DungeonRun -> ResultPipeline` handoff explicit.
- Batch 79 keeps route operating-scenario text as display/readback only: route plan lines on dungeon, battle-result, and final-result surfaces derive from the confirmed route plus shared route-meaning authoring, not from a new DungeonRun truth source.
