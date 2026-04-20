# Folder Ownership Map

This map records the current safe folder intent after Batch 77.4. Physical file location still does not overrule the owner rules in `AGENTS.md` and `docs/architecture/*`, but the folder layout should now make the common rails easier to navigate.

| Folder | Owner/system | What belongs there | What must not go there | Current exceptions / legacy files left behind |
| --- | --- | --- | --- | --- |
| `Assets/_Game/Scripts/App` | Appflow / persistent-root scaffold | app root, session root, scene ids, scene flow services, thin runtime-state holders | battle rules, world truth, dungeon logic | none |
| `Assets/_Game/Scripts/Bootstrap` | Appflow bootstrap adapters | `BootEntry`, scene-state bridges, launch coordinators, input gates, bootstrap-facing contracts | broad feature logic, new long-term owners, debug-only tools | `BootEntry.cs` remains a hot spot by necessity |
| `Assets/_Game/Scripts/World` | WorldSim + CityHub read-model seams | world truth, economy, board/city read models, world-view adapters, city decision builders | battle-only UI code, editor tools, preview-only controllers | `StaticPlaceholderWorldView*.cs` partials remain legacy integration hosts |
| `Assets/_Game/Scripts/Dungeon` | DungeonRun + handoff seams | run contracts, room/run factories, dungeon shell contracts, battle/result bridge seams | world truth ownership, battle rule ownership, editor-only validators | `StaticPlaceholderWorldView.DungeonRun*.cs` remains a hot spot until later extraction |
| `Assets/_Game/Scripts/Rpg` | RPG shared domain | party runtime, inventory/equipment shared data, progression snapshots, combat-shared domain data | scene-local HUD presenters, editor tools | inventory surface DTOs still live here because they are shared domain-facing readbacks |
| `Assets/_Game/Scripts/Rpg/Battle` | BattleScene | battle contracts, runtime state, coordinator, intent/lane/result/action ownership helpers | scene shell IMGUI presenters, non-battle inventory UI | none |
| `Assets/_Game/Scripts/UI/Shared` | UI shared | reusable UI helpers, shared skin/render/layout utilities | battle-only or inventory-only presenter logic | currently sparse; future shared shell utilities should land here first |
| `Assets/_Game/Scripts/UI/Battle` | Battle UI | battle skin definitions/providers/renderers, battle preview/runtime presenter adapters, battle shell presenter partials | battle rule truth, editor-only scene builders, world/dungeon ownership logic | `PrototypePresentationShell` root partial stays in `Core`; only battle-facing partials moved here |
| `Assets/_Game/Scripts/UI/Inventory` | Inventory UI | inventory skin definitions/providers/renderers, inventory preview/runtime presenter adapters, inventory shell presenter partials | inventory truth ownership, battle-only presenters | `BootEntry.Inventory.cs` stays in `Bootstrap` because it is an appflow adapter |
| `Assets/_Game/Scripts/UI/Preview` | UI preview | preview-scene controllers and preview-only presenter glue | production runtime presenters, editor-only authoring tools | `BattleUiPreviewPresenter.cs` stays here because it is preview-specific presentation glue |
| `Assets/_Game/Scripts/Debug` | Debug / developer HUD | runtime debug overlays, developer-only diagnostics | canonical gameplay owners, editor-only validators | `PrototypeDebugHUD.cs` moved here in Batch 77.4 |
| `Assets/_Game/Scripts/Editor` | Editor tooling | validators, smoke runners, asset/scene scaffolding helpers, authoring tools | runtime logic, player-facing presenters | must remain under an `Editor` folder |
| `Assets/_Game/Scenes/Runtime` | Runtime scenes | bootstrap/runtime scenes that are safe to load in the actual app rail | preview-only scenes, isolated test scenes | `Boot.Unity` moved here; `SampleScene.unity` remains outside as a legacy exception |
| `Assets/_Game/Scenes/Preview` | UI preview scenes | curated battle/inventory preview scenes for art/layout work | current playable baseline scene, smoke-only scenes | none |
| `Assets/_Game/Scenes/Test` | Test / smoke scenes | disposable test scenes, smoke-specific scenes if added later | current runtime baseline or preview art scenes | currently empty; created as a future-safe bucket |
| `Assets/_Game/Content/UI/Skins/Battle` | Battle UI content | battle skin ScriptableObjects | inventory skins, raw third-party sprite packs | none |
| `Assets/_Game/Content/UI/Skins/Inventory` | Inventory UI content | inventory skin ScriptableObjects | battle skins, raw sprite packs | none |
| `Assets/_Game/Content/UI/LayoutProfiles/Battle` | Battle UI content | battle layout profiles | runtime scene prefabs, raw texture packs | none |
| `Assets/_Game/Content/UI/LayoutProfiles/Inventory` | Inventory UI content | inventory layout profiles | battle layout profiles, raw texture packs | none |
| `Assets/_Game/Content/UI/Preview/Battle` | Battle preview content | preview-only mock data for battle UI scenes | runtime-only authored content, raw imported sprite packs | none |
| `Assets/_Game/Content/UI/Preview/Inventory` | Inventory preview content | preview-only mock data for inventory UI scenes | runtime-only authored content, raw imported sprite packs | none |
| `Assets/_Game/Content/UI/Sprites` | Curated UI sprite ownership target | future curated runtime-safe UI sprites after import/path ownership is stabilized | direct dumps of third-party packs without audit | current art packs stay under `Assets/Sprite` in this batch |
| `Assets/_Game/Resources/Content` | Runtime content resources | JSON/text assets intentionally resolved by `Resources.Load(...)` | arbitrary UI sprites or scenes moved without path updates | left in place because resource-folder names are path-sensitive |
| `Assets/Scenes` | Legacy runtime scene rail | current `SampleScene` baseline until a dedicated scene migration batch updates build settings and tooling | new preview scenes or unrelated runtime scenes | `SampleScene.unity` intentionally remains here in Batch 77.4 |

## Current Navigation Notes

- If you need battle UI presentation code, start in `Assets/_Game/Scripts/UI/Battle`.
- If you need inventory/equipment overlay presentation code, start in `Assets/_Game/Scripts/UI/Inventory`.
- If you need preview scene controllers, start in `Assets/_Game/Scripts/UI/Preview` and `Assets/_Game/Scenes/Preview`.
- If you need the accepted runtime debug HUD, start in `Assets/_Game/Scripts/Debug`.
- If you need canonical battle/runtime/domain truth, stay in `Assets/_Game/Scripts/Rpg/Battle` or the owning thread folders rather than following presenter files.
