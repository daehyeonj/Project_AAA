# Key Scripts Index

## Current Repo Note

- Current step label: `Step DungeonRun-Batch-15: Elite Variant Pack + Post-Elite Reward Identity`
- Source: `Assets/_Game/Scripts/Core/PrototypeLocalization.cs:39`
- This index is refreshed against the current repo, not the older Batch-5-only snapshot.

| File path | Main symbols | Current role | Current anchors |
| --- | --- | --- | --- |
| `Assets/_Game/Scripts/Bootstrap/BootEntry.cs` | `BootEntry` | Top-level state owner, world/dungeon visibility, HUD bridge, input entry | `Awake():305`, `Update():327`, `TryCycleCurrentDispatchPolicy():481`, `ChangeState():540`, `HandleWorldSimEconomyInput():578` |
| `Assets/_Game/Scripts/Core/GameState.cs` | `GameStateId`, `GameState` | Global state enum and transition log | `1-31` |
| `Assets/_Game/Scripts/World/StaticPlaceholderWorldView.cs` | `StaticPlaceholderWorldView`, `WorldSelectableMarker` | WorldSim placeholder view, selection bridge, economy bridge, selected summary text | constructor `16`, select `221`, run day `234`, auto tick `246`, reset `278`, selected dungeon id `976`, set selected `1495` |
| `Assets/_Game/Scripts/Dungeon/StaticPlaceholderWorldView.DungeonRun.cs` | `StaticPlaceholderWorldView` partial | Main dungeon state machine, planner, readiness, policy, pre-elite, elite result identity | enums `46-119`, enter dungeon `1177`, update loop `1228`, policy/readiness helpers `1887-2471`, route input `4708`, pre-elite open `4959`, run start `5122`, finish result `5232`, reset presentation `5315` |
| `Assets/_Game/Scripts/World/ManualTradeRuntimeState.cs` | `ManualTradeRuntimeState` | Economy runtime, stock, unmet needs, party expedition state, run resolution | `Reset():249`, `BeginDungeonRun():446`, `GetStockAmount():524`, `RunEconomyDay(bool):950`, `ApplyEndOfDayShortagePressure():1360`, `ResolveDungeonRun():1614` |
| `Assets/_Game/Scripts/World/DirectTradeScan.cs` | `DirectTradeScanResult`, `DirectTradeScanner`, related data classes | Direct trade opportunities, unmet city needs, unclaimed dungeon output scan | key data `44-152`, scanner pass `176-292` |
| `Assets/_Game/Scripts/World/WorldData.cs` | `WorldEntityKind`, `WorldEntityData`, `WorldRouteData`, `WorldData`, `PlaceholderWorldDataFactory` | Placeholder world authoring for cities, dungeons, linked city ids, and map route | data classes `1-104`, authoring `106-184` |
| `Assets/_Game/Scripts/World/WorldCameraController.cs` | `WorldCameraController` | WorldSim pan and zoom controls | `21-95` |
| `Assets/_Game/Scripts/Core/PrototypeDebugHUD.cs` | `PrototypeDebugHUD` | Current debug HUD, planner UI, pre-elite UI, result overlay, search and language UI | `OnGUI():76`, overlay `142`, route section `437`, policy button `467`, pre-elite section `543`, title/language `629`, search `673`, world panels `842-987`, selected economy panel `1034-1055` |
| `Assets/_Game/Scripts/Core/PrototypeLocalization.cs` | `PrototypeLanguage`, `PrototypeLocalization` | EN/KR static label tables, current step label, planner/result/elite label keys | enum `3-7`, English table start `11`, current step label `39`, elite keys `114-129`, planner keys `214-223`, route panel key `280`, lookup helpers `426-439` |
| `Assets/_Game/Scripts/Core/ResourceData.cs` | `ResourceData`, `PlaceholderResourceDataFactory` | Placeholder resource authoring | `15-27` |

## Current Planner Slice

- Recommendation core:
  - `GetBaseRecommendedRouteId(...)` at `StaticPlaceholderWorldView.DungeonRun.cs:2175`
  - `ApplyPolicyBiasToRecommendedRoute(...)` at `StaticPlaceholderWorldView.DungeonRun.cs:2197`
  - `GetRecommendedRouteId(...)` at `StaticPlaceholderWorldView.DungeonRun.cs:2226`
  - `BuildBaseRecommendationReasonText(...)` at `StaticPlaceholderWorldView.DungeonRun.cs:2240`
  - `BuildPolicyRecommendationReasonText(...)` at `StaticPlaceholderWorldView.DungeonRun.cs:2272`
  - `BuildRecommendationReasonText(...)` at `StaticPlaceholderWorldView.DungeonRun.cs:2301`
  - `BuildExpectedNeedImpactText(...)` at `StaticPlaceholderWorldView.DungeonRun.cs:2313`
  - `RefreshDispatchRecommendation()` at `StaticPlaceholderWorldView.DungeonRun.cs:2424`

- Readiness and policy storage:
  - `_dispatchReadinessByCityId`
  - `_dispatchRecoveryDaysRemainingByCityId`
  - `_daysSinceLastDispatchByCityId`
  - `_consecutiveDispatchCountByCityId`
  - `_dispatchPolicyByCityId`
  - `DispatchReadinessState`
  - `DispatchPolicyState`

- Newer systems now touching the same flow:
  - `PreEliteChoice` in `DungeonRunState`
  - pre-elite choice input and panel
  - elite encounter identity and elite reward identity in result panel

## Current Cautions

- `Assets/_Game/Scripts/Dungeon/StaticPlaceholderWorldView.DungeonRun.cs` is the densest file in the repo and now owns:
  - planner
  - readiness
  - policy
  - explore
  - battle
  - event choice
  - pre-elite choice
  - elite result identity
- Active build scene remains `Assets/Scenes/SampleScene.unity`, not `_Game/Scenes/Boot.Unity`.
