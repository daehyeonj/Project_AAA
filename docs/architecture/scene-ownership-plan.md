# Scene Ownership Plan

## Purpose

This document is the ownership audit for the current `SampleScene` baseline and the source of truth for the first scene-splitting scaffold. It does **not** claim that the project is already multi-scene. It records what should persist, what should unload, and which current classes are legacy adapters that need to shrink over time.

## Runtime Reality

- The current playable baseline is still one `SampleScene`.
- `BootEntry`, `StaticPlaceholderWorldView`, `PrototypePresentationShell`, and `PrototypeDebugHUD` still cooperate inside that one scene.
- This batch adds a compile-safe persistent-root scaffold only. It does **not** migrate gameplay flow, world state, dungeon state, or battle presentation to new scenes yet.

## Classification Rules

| Classification | Meaning | DDOL? |
| --- | --- | --- |
| Persistent Runtime State | Canonical session facts that must survive scene unloads. | Yes, through a small root or serializable state holder. |
| Persistent Service | Stateless or light-state coordinators that manage scene/session flow. | Yes, if they stay thin. |
| Static Content / Definition | Authored data or ScriptableObject definitions. | Not runtime DDOL; load by reference. |
| Scene-local Visual | Cameras, markers, tokens, backdrops, world-space visuals. | No. Rebuild per scene. |
| Scene-local Presenter | Scene UI and input/presentation code tied to one loaded screen. | No. Rebuild per scene. |
| Overlay Presenter | Inventory, readback, modal, or popover surfaces that belong to a loaded scene. | No. Rebuild per scene. |
| Debug/Editor-only | Debug HUDs, validators, editor tools. | No. |
| Legacy Adapter | Integration hot spots that currently bridge multiple threads. | No new ownership here. Shrink over time. |

## Current Ownership Audit

| System | Current file(s) | Current classification | Future ownership target | Notes |
| --- | --- | --- | --- | --- |
| `BootEntry` | `Assets/_Game/Scripts/Bootstrap/BootEntry.cs` | Legacy Adapter | `AppRoot` / scene presenters consume persistent state instead of exposing everything directly | Current integration hub for language, shell state, cached readbacks, and many adapter labels. Do not promote it as the persistent root. |
| `GameFlowCoordinator` | `Assets/_Game/Scripts/Bootstrap/GameFlowCoordinator.cs` | Persistent Service candidate | `SceneFlowService` + app/session flow coordinator | Thin state adapter around `GameStateId`. Safe logic, but it currently exists only as part of the sample-scene shell. |
| `BootstrapSceneStateBridge` | `Assets/_Game/Scripts/Bootstrap/BootstrapSceneStateBridge.cs` | Legacy Adapter | Scene-local bridge or presenter dependency | Pure pass-through into `BootEntry`. Useful while the sample-scene shell exists, but not a long-lived root object. |
| `BootstrapInputGate` | `Assets/_Game/Scripts/Bootstrap/BootstrapInputGate.cs` | Scene-local Presenter / Legacy Adapter | Scene-local input/presenter gate | Couples pointer/keyboard blocking to `PrototypePresentationShell` and `PrototypeDebugHUD`. Must unload with the scene. |
| `WorldCameraController` | `Assets/_Game/Scripts/World/WorldCameraController.cs` | Scene-local Visual | World-sim scene camera controller | Camera position and zoom are scene-specific. Never keep this object alive across scene transitions. |
| `StaticPlaceholderWorldView` | `Assets/_Game/Scripts/World/StaticPlaceholderWorldView.cs` + partials | Legacy Adapter over real owners | Split into owner-side builders/services plus scene presenters | This is the largest mixed-ownership hot spot. It currently hosts world truth access, dungeon flow, battle handoff, inventory surface building, and board visuals. Future migration should extract by owner, not clone the class into every scene. |
| `PrototypePresentationShell` | `Assets/_Game/Scripts/Core/PrototypePresentationShell.cs` + presenter partials under `Assets/_Game/Scripts/UI/*` | Overlay Presenter | World/menu/dungeon scene-local presenters | Draws IMGUI shell surfaces. It is presentation, not canonical runtime state. |
| `PrototypeDebugHUD` | `Assets/_Game/Scripts/Debug/PrototypeDebugHUD.cs` | Debug/Editor-only with current player-facing battle rail | Battle-scene presenter later, debug owner stays optional | Today it carries the accepted battle HUD. Even so, it should not become a persistent root. If battle presentation migrates, move presenter logic out and leave debug-only concerns behind. |
| Inventory / equipment truth | `Assets/_Game/Scripts/World/ManualTradeRuntimeState.cs` | Persistent Runtime State | `GameSessionRoot`-reachable world/runtime owner | Inventory, equipment, pending spoils, and member progression are canonical session data and must survive scene unloads. |
| Inventory / equipment surface builder | `Assets/_Game/Scripts/World/StaticPlaceholderWorldView.InventorySurface.cs` | Legacy Adapter | Inventory surface builder owned closer to runtime/session state | Builds `PrototypeRpgInventorySurfaceData` from persistent truth. Safe to unload; can be rebuilt in another presenter. |
| Inventory overlay presenter | `Assets/_Game/Scripts/UI/Inventory/PrototypePresentationShell.Inventory.cs` | Overlay Presenter | Inventory overlay presenter in the consuming scene | Visual only. Never DDOL. |
| Battle HUD truth | `Assets/_Game/Scripts/Rpg/Battle/PrototypeBattleUiData.cs`, `Assets/_Game/Scripts/Dungeon/StaticPlaceholderWorldView.BattleContracts.cs`, `Assets/_Game/Scripts/Rpg/Battle/StaticPlaceholderWorldView.RpgBattleSurfaceOwnership.cs` | Persistent Runtime State + builder seam | Battle-scene runtime + view-model builder | Canonical battle contracts already live outside the shell. Keep that direction. |
| Battle HUD presenter | `Assets/_Game/Scripts/Debug/PrototypeDebugHUD.cs` | Scene-local Presenter on the accepted rail | Dedicated battle-scene presenter later | Visual shell only. Can be replaced, but the underlying battle contracts should remain canonical. |
| Alternate battle presenter path | `Assets/_Game/Scripts/UI/Battle/PrototypePresentationShell.DungeonRun.cs` | Scene-local Presenter / legacy rail | Remove or align after battle-scene migration | Exists as a secondary presentation path and should not become a second source of truth. |
| Battle result popover truth | `Assets/_Game/Scripts/Dungeon/StaticPlaceholderWorldView.DungeonRun.ShellContracts.cs` | Persistent Runtime State snapshot | Dungeon-run/result presenter consumes cached shell readback | The popover content comes from run/battle/result state and should survive presenter rebuilds. |
| Battle result popover presenter | `Assets/_Game/Scripts/UI/Battle/PrototypePresentationShell.DungeonRun.cs` | Overlay Presenter | Dungeon-run or result-scene presenter | Modal UI only. Never DDOL. |

## Persistent Candidates After This Batch

### Persistent runtime state

- `ManualTradeRuntimeState`
- `AppFlowContext` and its canonical payload chain (`ExpeditionPlan`, `ExpeditionRunState`, `PrototypeBattleRequest`, `PrototypeBattleRuntimeState`, `PrototypeBattleResolution`, `ExpeditionOutcome`, `WorldDelta`, `OutcomeReadback`)
- battle/readback snapshots that must survive scene presenter rebuilds
- player-facing session state such as current language and active logical stage

### Persistent services

- `SceneFlowService`
- a thin app/session coordinator that owns scene-to-stage bookkeeping only

### Scene-local only

- all IMGUI presenters
- `WorldCameraController`
- world markers, dungeon tokens, battle formation visuals
- inventory overlay, battle result popovers, and other modal surfaces

## First Honest Migration Target

The safest first migration is **not** battle. It is:

1. introduce `AppRoot` / `GameSessionRoot` without changing runtime behavior
2. extract `MainMenu` presentation first because it has the least canonical state coupling
3. then extract `WorldSim` presentation while keeping world truth in the persistent runtime owner

Battle and dungeon scenes should move only after the session root is proven stable and the world-side cut has stopped depending on `BootEntry` as the only adapter.
