# DDOL Policy

## Rule

`DontDestroyOnLoad` is allowed only for **small persistent roots and thin services**. Data persists. Screens unload.

## Allowed DDOL Targets

### App/session roots

- `AppRoot`
- `GameSessionRoot`

### Thin persistent services

- `SceneFlowService`
- future app/session coordinators whose job is only to keep scene identity, stage identity, and session references coherent

### Serializable persistent state owned by the root

- `RuntimeGameState`
- future small state holders that summarize current scene/stage/session identity

## Not Allowed As DDOL

- `BootEntry`
- `BootstrapSceneStateBridge`
- `BootstrapInputGate`
- `WorldCameraController`
- `StaticPlaceholderWorldView` scene roots and world-space visuals
- `PrototypePresentationShell`
- `PrototypeDebugHUD`
- inventory overlays, battle popovers, route boards, prep boards, or any other modal/presenter surface
- temporary scene markers, combat tokens, background panels, or effect objects

## Why

Persistent data and services need continuity. Presenters and scene visuals need clean teardown.

If a scene-local object is marked DDOL:

- stale pointers survive after the scene changes
- modal/input blocking state leaks into the next screen
- old world/battle/inventory visuals remain alive and start competing with the new scene
- hot spots like `BootEntry` and `StaticPlaceholderWorldView` become even harder to split later

## Transfer Rule

When one screen hands off to another:

1. canonical runtime state stays in persistent owners
2. the old presenter scene unloads
3. the next presenter scene reads the state it needs from persistent owners or canonical builders
4. any transient selection/highlight/modal state is rebuilt for that scene instead of being dragged across by DDOL

## Current Project Interpretation

- current world economy, party, inventory, equipment, run, battle, and result truths are the only things that deserve persistence
- current IMGUI shells are presentation rails and should remain disposable
- the new scaffold in `Assets/_Game/Scripts/App` exists to make this boundary explicit before scene migration begins

## Migration Gate

No future scene split should mark a presenter object DDOL just to “make it keep working.”

If a scene transition would break because a presenter is not persistent, fix the ownership seam:

- move the needed data into a persistent runtime owner, or
- move thin coordination into a root-level service

Do **not** keep the presenter alive as a shortcut.
