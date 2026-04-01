# ChatGPT Handoff

## Current Repo Headline

- Unity project confirmed.
- Unity version: `6000.0.68f1` from `ProjectSettings/ProjectVersion.txt`.
- Current step label in repo: `Step DungeonRun-Batch-15: Elite Variant Pack + Post-Elite Reward Identity`
  - Source: `Assets/_Game/Scripts/Core/PrototypeLocalization.cs:39`
  - Korean table mirror: `Assets/_Game/Scripts/Core/PrototypeLocalization.cs:416`
- Current build scene is `Assets/Scenes/SampleScene.unity`
  - Source: `ProjectSettings/EditorBuildSettings.asset`
- `Assets/_Game/Scenes/Boot.Unity` exists, but it is not the active build scene.
- Repo root `.sln` file: not found in repo root.

## Core Tree

```text
Project_AAA/
|-- Assets/
|   |-- _Game/
|   |   |-- Scenes/
|   |   |   `-- Boot.Unity
|   |   `-- Scripts/
|   |       |-- Bootstrap/
|   |       |   `-- BootEntry.cs
|   |       |-- Core/
|   |       |   |-- GameState.cs
|   |       |   |-- PrototypeDebugHUD.cs
|   |       |   |-- PrototypeLocalization.cs
|   |       |   `-- ResourceData.cs
|   |       |-- Dungeon/
|   |       |   `-- StaticPlaceholderWorldView.DungeonRun.cs
|   |       `-- World/
|   |           |-- DirectTradeScan.cs
|   |           |-- ManualTradeRuntimeState.cs
|   |           |-- StaticPlaceholderWorldView.cs
|   |           |-- WorldCameraController.cs
|   |           `-- WorldData.cs
|   `-- Scenes/
|       `-- SampleScene.unity
|-- Builds/
|   `-- Smoke/
|       `-- Project_AAA-Smoke.exe
|-- Packages/
|-- ProjectSettings/
|-- batch5_callgraph.md
|-- batch5_context_pack.md
|-- handoff_for_chatgpt.md
|-- key_scripts_index.md
`-- tree_core.txt
```

## Current State Flow

- Top-level flow:
  - `Boot -> MainMenu -> WorldSim -> DungeonRun -> WorldSim`
  - Main owner: `Assets/_Game/Scripts/Bootstrap/BootEntry.cs`
  - Current anchors:
    - `Awake()` at `BootEntry.cs:305`
    - `Update()` at `BootEntry.cs:327`
    - `ChangeState(...)` at `BootEntry.cs:540`
    - `HandleWorldSimEconomyInput(...)` at `BootEntry.cs:578`

- WorldSim side:
  - `StaticPlaceholderWorldView` owns selection, world summary text, world visibility, economy bridge, and reset bridge.
  - Current anchors:
    - constructor at `StaticPlaceholderWorldView.cs:16`
    - `SelectAtScreenPosition(...)` at `StaticPlaceholderWorldView.cs:221`
    - `RunEconomyDay()` at `StaticPlaceholderWorldView.cs:234`
    - `UpdateAutoTick(...)` at `StaticPlaceholderWorldView.cs:246`
    - `ResetRuntimeEconomy()` at `StaticPlaceholderWorldView.cs:278`

- DungeonRun internal flow:
  - Current enum is:
    - `None`
    - `RouteChoice`
    - `Explore`
    - `Battle`
    - `EventChoice`
    - `PreEliteChoice`
    - `ResultPanel`
  - Source: `Assets/_Game/Scripts/Dungeon/StaticPlaceholderWorldView.DungeonRun.cs:46-55`
  - Core anchors:
    - `TryEnterSelectedCityDungeon(...)` at `...DungeonRun.cs:1177`
    - `UpdateDungeonRun(...)` at `...DungeonRun.cs:1228`
    - `ConsumeDungeonRunExitRequest()` at `...DungeonRun.cs:1303`
    - `StartDungeonRunForRoute(...)` at `...DungeonRun.cs:5122`
    - `FinishDungeonRun(...)` at `...DungeonRun.cs:5232`
    - `ResetDungeonRunPresentationState()` at `...DungeonRun.cs:5315`

## Current Confirmed Systems

- WorldSim:
  - Placeholder world with cities and dungeons
  - Camera pan/zoom
  - Click selection
  - Debug HUD
  - Search field in HUD
  - Language toggle and EN/KR tables
  - Manual day tick `T`
  - Auto tick update path
  - Reset `R`
  - Party recruit `G`
  - Dungeon entry `X`

- Dispatch planner:
  - Route choice state before entering dungeon
  - Need pressure derived from `mana_shard` city stock
  - Dispatch readiness and recovery tracking per city
  - Consecutive dispatch tracking per city
  - Dispatch policy per city
    - `Safe`
    - `Balanced`
    - `Profit`
  - Recommendation is now:
    - base recommendation from need pressure + readiness
    - then policy bias is applied
  - Policy can be cycled in planner with `Q`
  - Current policy anchors:
    - `DispatchPolicyState` at `...DungeonRun.cs:114-119`
    - `_dispatchPolicyByCityId` at `...DungeonRun.cs:437`
    - `GetDispatchPolicyState(...)` at `...DungeonRun.cs:2005`
    - `BuildDispatchPolicyText(...)` at `...DungeonRun.cs:2025`
    - `ApplyPolicyBiasToRecommendedRoute(...)` at `...DungeonRun.cs:2197`
    - `BuildPolicyRecommendationReasonText(...)` at `...DungeonRun.cs:2272`
    - `TryCycleCurrentDispatchPolicy()` at `...DungeonRun.cs:2464`
    - planner keyboard hook at `...DungeonRun.cs:4708-4725`

- Dungeon run:
  - Explore
  - Turn-based battle
  - Event choice
  - Pre-elite choice
  - Elite encounter
  - Result panel
  - Retreat from explore with `Q`
    - anchor: `...DungeonRun.cs:4807-4812`
  - Result summary now includes:
    - pre-elite choice
    - pre-elite heal amount
    - elite defeated
    - elite reward identity
    - elite reward amount
    - elite bonus reward amount
    - room path summary
    - party HP summary
    - party condition at end

- Economy and result writeback:
  - Actual stock/runtime state is still in `ManualTradeRuntimeState.cs`
  - Dungeon result writeback still happens through:
    - `ManualTradeRuntimeState.ResolveDungeonRun(...)` at `ManualTradeRuntimeState.cs:1614`
  - The actual city stock add is still:
    - `SetStock(cityId, rewardResourceId, GetStock(cityId, rewardResourceId) + safeLootReturned);`

## Important Current Mismatch

- The repo has advanced beyond the old Batch-5 snapshot.
- Old planner docs are still useful, but they are no longer the whole story.
- The biggest new additions since the earlier snapshot are:
  - `DispatchPolicyState`
  - `PreEliteChoice`
  - elite encounter and elite reward identity/result hooks
  - extra HUD/result lines tied to those systems

## Best File Bundle For A New GPT Chat

- Start with:
  - `handoff_for_chatgpt.md`
  - `key_scripts_index.md`
  - `tree_core.txt`

- Then open these source files:
  - `Assets/_Game/Scripts/Bootstrap/BootEntry.cs`
  - `Assets/_Game/Scripts/World/StaticPlaceholderWorldView.cs`
  - `Assets/_Game/Scripts/Dungeon/StaticPlaceholderWorldView.DungeonRun.cs`
  - `Assets/_Game/Scripts/World/ManualTradeRuntimeState.cs`
  - `Assets/_Game/Scripts/Core/PrototypeDebugHUD.cs`
  - `Assets/_Game/Scripts/Core/PrototypeLocalization.cs`
  - `Assets/_Game/Scripts/World/WorldData.cs`

- For planner/readiness-specific work, also read:
  - `batch5_context_pack.md`
  - `batch5_callgraph.md`

## Risks And Notes

- `batch5_context_pack.md` file name is historical. The repo is now at `DungeonRun-Batch-15`.
- Build scene naming is misleading:
  - active build scene: `Assets/Scenes/SampleScene.unity`
  - extra scene file: `Assets/_Game/Scenes/Boot.Unity`
- Recommendation/readiness/policy logic is concentrated in one large file:
  - `Assets/_Game/Scripts/Dungeon/StaticPlaceholderWorldView.DungeonRun.cs`
- World need aggregation and dispatch recommendation are still not the same thing:
  - world economy unmet needs live in `ManualTradeRuntimeState.cs`
  - planner need pressure is still based on `mana_shard` stock thresholds in `...DungeonRun.cs`
