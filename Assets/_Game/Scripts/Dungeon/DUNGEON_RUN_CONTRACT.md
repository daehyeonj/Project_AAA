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
- `FinishDungeonRun(...)` still builds `PrototypeRpgRunResultSnapshot`, `PostRunResolutionInput`, `ExpeditionResult`, and `ExpeditionOutcome`.
- `AppFlowResultContext.ResolvedRunState` now carries the resolved `ExpeditionRunState` beside the existing `ExpeditionOutcome` / `OutcomeReadback`.
- Batch 79.2 intent packaging: DungeonRun must pass mission objective, mission relevance, and risk/reward context explicitly into the ResultPipeline bridge instead of letting ResultPipeline infer those fields from key encounter or world writeback summaries.
- Follow-up `ResultPipeline` or `BattleScene` work should prefer:
  1. `AppFlowDungeonRunContext.RunState`
  2. `AppFlowBattleContext.HandoffPayload` / `ReturnPayload`
  3. `AppFlowResultContext.ResolvedRunState`

## Implementation Notes
- The legacy dungeon partial still owns combat/exploration rules.
- `DungeonRunStateFactory` and `DungeonRunCoordinator` only assemble and project run state.
- This keeps the prototype additive while making `ExpeditionPlan -> DungeonRun -> ResultPipeline` handoff explicit.
- Batch 79 keeps route operating-scenario text as display/readback only: route plan lines on dungeon, battle-result, and final-result surfaces derive from the confirmed route plus shared route-meaning authoring, not from a new DungeonRun truth source.
- Batch 87 route-feel rule: Dungeon Alpha route-feel readbacks may expose `Dungeon feel` and `Route Check` lines during DungeonRun and battle-result popovers, but those lines must derive from the confirmed route, authored room order, encounter profile, battle setup, and route meaning data. Do not add a broad dungeon content system, revive the legacy choice-only shell, or rebuild route-feel copy every frame.
- Batch 88 room-interaction rule: route-defining utility rooms stay inside the current grid dungeon. Rest Shrine / Greed Cache use explicit `[E]` interaction while in `DungeonRunState.Explore`, are blocked by modal/battle input gates, and summarize their real recovery/cache effects through the existing run result and world readback chain. Do not add a new room system, fake penalty, or per-frame route rebuild.
- Batch 89 next-beat consequence rule: Rest Shrine / Greed Cache may arm a tiny per-run consequence for the next Alpha encounter, but it must stay in DungeonRun runtime state, derive from real HP/recovery or cache-loot state, surface through existing battle context / route check / result / world readbacks, and clear after the target encounter or run reset. Do not add enemy-stat changes, a fatigue system, or a second result contract to make the consequence feel stronger.
