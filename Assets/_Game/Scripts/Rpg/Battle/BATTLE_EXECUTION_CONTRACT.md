# Battle Execution Contract

`Project_AAA` battle flow now treats BattleScene as an explicit executor between `DungeonRun` and the downstream result pipeline.

## Runtime Surface Reality

- Batch 14 lock: the current manual-test battle surface is a `standard JRPG battle` entered from grid-dungeon encounters.
- Validate the current battle as command/target selection plus structured battle request/runtime/resolution contracts.
- Any older lane-oriented ideas remain design-direction hypotheses, not current runtime facts, until the post-batch-20 manual evaluation.

## Input

- `PrototypeBattleRequest`
- Captured when `TryStartEncounter()` enters battle.
- Holds encounter, dungeon, route, room, objective, risk context, and party/enemy snapshots.
- Batch 25 post-slice seam: representative encounters may also seed `EncounterProfileId`, `BattleSetupId`, enemy-group text, and battle-context text from shared authoring assets before the live battle starts.
- `BattleHandoffPayload.Request` mirrors the same contract for dungeon-run state readers.

## Runtime State

- `PrototypeBattleRuntimeState`
- Built from the live battle runtime inside `StaticPlaceholderWorldView`.
- Carries phase, actor, current command, party/enemy combatant states, prompt/feedback text, recent log lines, recent events, and the current result snapshot.
- `BuildBattleUiSurfaceData()` now reads from this normalized runtime state through `PrototypeBattleViewModel` instead of rebuilding HUD data straight from scattered raw fields.

## Commands

- `PrototypeBattleCommand`
- `PrototypeBattleCommandResolver` creates the structured command at selection time and binds the target during target selection.
- Current prototype commands are `Attack`, `Skill`, and `Retreat`.
- The active pending command is kept in `_pendingBattleCommand` so target-select and resolution can read the same structured payload.

## Resolution

- `PrototypeBattleResolution`
- Captured on encounter victory and run-end (`clear / defeat / retreat`).
- Holds outcome key/type, encounter metadata, party condition, HP summary, loot summary, turn count, combat totals, event trail, and a `PrototypeBattleResultSnapshot`.
- `BattleReturnPayload.Resolution` mirrors the same contract for dungeon-run state readers.

## Dungeon Run / Result Pipeline Handoff

- `ExpeditionRunState.PendingBattle.Request` exposes the active battle input payload.
- `ExpeditionRunState.ActiveBattleState` exposes the normalized live battle state.
- `ExpeditionRunState.LastBattleReturn.Resolution` exposes the last completed battle resolution.
- `FinishDungeonRun(...)` still produces `PrototypeRpgRunResultSnapshot` and `ExpeditionOutcome`, but now those are created after a structured `PrototypeBattleResolution` has been captured, which makes later `ResultPipeline` enrichment safer.
