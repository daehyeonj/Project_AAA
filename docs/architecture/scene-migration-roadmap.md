# Scene Migration Roadmap

## Goal

Move from the current `SampleScene` mega-shell toward category-based scenes without losing the accepted runtime rail or creating a second source of truth.

## Phase 0: Scaffold Only

Status: `this batch`

- add `AppRoot`
- add `GameSessionRoot`
- add `RuntimeGameState`
- add `SceneFlowService`
- document DDOL rules and scene ownership
- keep `SampleScene` as the active playable baseline

Exit criteria:

- compile passes
- current runtime path is unchanged
- ownership documents make clear what may persist and what must unload

## Phase 1: Main Menu Extraction

Why first:

- smallest dependency surface
- easiest place to prove scene loading/unloading without touching combat or world truth

Scope:

- move main-menu presentation from `PrototypePresentationShell`
- keep session root alive across the scene change
- no gameplay/system rule changes

Exit criteria:

- `MainMenu` can load and unload without losing language/session identity
- no `BootEntry`-owned gameplay truth is needed just to show the menu

## Phase 2: WorldSim Presentation Extraction

Scope:

- move world-map camera, board visuals, and city-selection presentation into a dedicated scene
- keep `ManualTradeRuntimeState` and world read models as the canonical runtime owner
- keep prep-launch confirmation truth outside scene visuals

Risks:

- `StaticPlaceholderWorldView` currently mixes runtime facts, board visuals, prep surfaces, dungeon launch, and inventory hooks
- the cut must extract scene-local visuals without cloning world truth into presenters

Exit criteria:

- world scene can rebuild from persistent runtime/session state
- selection/readback survives scene reload through canonical owners, not through DDOL presenters

## Phase 3: DungeonRun Presentation Extraction

Scope:

- move grid exploration visuals and scene-local input/presentation out of the world scene
- preserve canonical `ExpeditionRunState`
- preserve dungeon-to-battle handoff contracts

Exit criteria:

- entering/leaving dungeon run does not require `SampleScene` to remain loaded
- exploration presenters rebuild entirely from run state and read models

## Phase 4: Battle Scene Extraction

Scope:

- move the accepted battle rail to a dedicated presenter scene
- keep `PrototypeBattleRequest`, `PrototypeBattleRuntimeState`, `PrototypeBattleResolution`, and battle surface builders canonical
- treat the current `PrototypeDebugHUD` battle shell as presentation logic to migrate, not as a persistent authority

Risks:

- accepted player-facing rail currently lives inside a debug-prefixed file
- there is also a legacy alternate battle presenter path in `PrototypePresentationShell`

Exit criteria:

- one battle presenter path remains
- battle request/runtime/resolution truth stays outside the scene-local presenter

## Phase 5: Result / Return Readback Cleanup

Scope:

- decide whether result readback remains a dungeon/world overlay or becomes a dedicated result scene
- keep `ResultPipeline` canonical
- keep pending spoils / extraction truth on the runtime side

Exit criteria:

- reward/result/readback can reload cleanly without scene-local persistent hacks

## Rules For Every Migration Step

- one scene split at a time
- no second source of truth
- no presenter DDOL shortcuts
- add adapters near the canonical owner if cross-scene handoff is needed
- keep `BootEntry` and `StaticPlaceholderWorldView` shrinking, not growing

## Current Recommendation

The next honest implementation step after this scaffold is:

1. prove `AppRoot` in a non-invasive setup
2. extract the main menu
3. only then revisit world/dungeon/battle cuts

That order lowers rollback risk and gives future refactors a persistent root to land on.
