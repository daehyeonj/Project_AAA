# Project AAA Post-Slice Batch Status

## Current Mainline Snapshot

- Snapshot date: `2026-04-20`
- Sync intent: `preserve the current local runtime state on GitHub main so repository readers can inspect the live working baseline`
- Active runtime baseline to preserve: `grid dungeon explore shell` + `current battleScene HUD`
- UI safety rule: `reject rollback prompts against the currently accepted runtime UI; additive or preserving UI work is allowed`
- Runtime decision note: see `docs/runtime/mainline-snapshot-2026-04-17.md`

## Current Verdict

- Latest closed batch: `Batch 77.5 UI sprite import audit + Batch 77.4 folder organization + Batch 78 + Batch 77.1 blocker fix + Batch 77.2 battle/inventory UI skinning scaffold + scene architecture scaffold follow-up + Batch 77.3 battle/inventory UI preview scaffold`
- Runtime baseline: `grid dungeon` + `standard JRPG battle`
- Canonical representative rail: stable
- Surfaced portfolio: stable
- Alpha surfaced pair: `content-completed on current rail`
- Beta surfaced pair: `content-thickened on current rail`
- Current signature demo pair: `city-b -> dungeon-beta`
- Current presenter playbook: `docs/runtime/batch71-beta-signature-demo-playbook.md`
- Next honest bottleneck: `manual Unity Editor assignment of the curated UI sprite candidates into battle/inventory skin slots, then either layout tuning or a return to combat-role follow-through on the accepted Batch 78 rail`
- Scene architecture status: `compile-safe persistent-root scaffold added; SampleScene remains the live playable baseline`

## Batch 77.5 Close-Out

- Selected branch: `A`
- Honest scope:
  - audit raw UI sprite packs without moving them
  - create a project-owned curated staging area under `Assets/_Game/Content/UI/Sprites`
  - copy only a small intentional starter set of battle/inventory UI candidates
  - document slot mapping and manual assignment workflow
- Raw source audit:
  - `Assets/Sprite/Complete_UI_Book_Styles_Pack_Free_v1.0` is the clearest UI-source pack for panel/button/bar/slot candidates
  - `Assets/Sprite/Free - Raven Fantasy Icons` contains many numeric icon files and was audited but not curated in this batch because the filenames are too opaque for safe narrowing
  - raw sampled UI metas still show `textureType: 0`, `spriteMode: 0`, `alphaIsTransparency: 0`, `spritePixelsToUnits: 100`, and `filterMode: 1`
- Curated starter copy:
  - battle panel/button/timeline/bar/popup/badge/icon candidates were copied from `TravelBookLite`
  - inventory panel/slot/row/badge/icon candidates were copied from `TravelBookLite`
  - shared frame/divider/background/icon examples were staged for future reuse
  - raw source packs were not moved, renamed, or reimported
- Docs added:
  - `docs/ui/ui-sprite-curation.md`
  - `docs/ui/battle-skin-slot-map.md`
  - `docs/ui/inventory-skin-slot-map.md`
  - `Assets/_Game/Content/UI/Sprites/_SourceMap/README.md`
  - `Assets/_Game/Content/UI/Sprites/_SourceMap/ui-sprite-candidate-map.md`
- Import setting changes:
  - none
  - curated copies intentionally preserved the sampled raw import behavior for now
  - any future `Sprite (2D and UI)` conversion should happen on curated copies only, not on raw source art
- Gameplay changes: `none`
- Validation snapshot:
  - compile: `PASS (unity-merge-validate.log, 2026-04-20)`
  - raw source untouched: `PASS`
  - curated folders created: `PASS`
  - runtime/resource rail unchanged: `PASS`
  - SampleScene path preserved: `PASS`

## Batch 77.4 Close-Out

- Selected branch: `A`
- Honest scope:
  - clarify folder ownership for battle UI, inventory UI, debug HUD, and runtime-vs-preview scene assets without changing gameplay behavior
  - keep path-sensitive runtime content in place where the audit showed higher breakage risk
- Files moved:
  - `Assets/_Game/Scripts/Core/PrototypeDebugHUD.cs` -> `Assets/_Game/Scripts/Debug/PrototypeDebugHUD.cs`
  - `Assets/_Game/Scripts/Core/PrototypePresentationShell.DungeonRun.cs` -> `Assets/_Game/Scripts/UI/Battle/PrototypePresentationShell.DungeonRun.cs`
  - `Assets/_Game/Scripts/Core/PrototypePresentationShell.GameplayBattleHud.cs` -> `Assets/_Game/Scripts/UI/Battle/PrototypePresentationShell.GameplayBattleHud.cs`
  - `Assets/_Game/Scripts/Core/PrototypePresentationShell.Inventory.cs` -> `Assets/_Game/Scripts/UI/Inventory/PrototypePresentationShell.Inventory.cs`
  - `Assets/_Game/Scenes/Boot.Unity` -> `Assets/_Game/Scenes/Runtime/Boot.Unity`
- Folder structure added:
  - `Assets/_Game/Scripts/Debug/`
  - `Assets/_Game/Scenes/Runtime/`
  - `Assets/_Game/Scenes/Test/`
  - `Assets/_Game/Content/UI/Sprites/`
- Intentionally not moved:
  - `Assets/Scenes/SampleScene.unity` because Build Settings, editor defaults, and smoke tooling still point at that legacy baseline
  - `Assets/_Game/Resources/Content/*` because `Resources.Load(...)` paths are active runtime/editor dependencies
  - `Assets/Sprite/*` because runtime import/path ownership is not stabilized yet and this batch is not an asset migration batch
  - `BootEntry.cs` and `StaticPlaceholderWorldView*.cs` because they are still hot spots and not safe folder-shuffle targets in this pass
- Gameplay changes: `none`
- Validation snapshot:
  - compile: `PASS (unity-merge-validate.log, 2026-04-20)`
  - playable-path proof: `SampleScene path intentionally preserved`
  - path-risk guard: `runtime Resources and preview scene paths preserved`

## Batch 77.3 Close-Out

- Selected branch: `B`
- Honest scope:
  - keep the existing runtime/UI rail intact
  - extend the existing battle/inventory skin scaffold into editor-friendly preview scenes, layout profiles, and mock preview data
  - avoid all gameplay/runtime migration while giving the designer a safe art-placement workspace
- Added preview scaffolds:
  - `Assets/_Game/Scenes/Preview/BattleUiPreviewScene.unity`
  - `Assets/_Game/Scenes/Preview/InventoryUiPreviewScene.unity`
  - `Assets/_Game/Content/UI/Skins/Battle/BattleUiSkin_Default.asset`
  - `Assets/_Game/Content/UI/Skins/Inventory/InventoryUiSkin_Default.asset`
  - `Assets/_Game/Content/UI/LayoutProfiles/Battle/BattleUiLayout_Default.asset`
  - `Assets/_Game/Content/UI/LayoutProfiles/Inventory/InventoryUiLayout_Default.asset`
  - `Assets/_Game/Content/UI/Preview/Battle/BattleUiPreview_Default.asset`
  - `Assets/_Game/Content/UI/Preview/Inventory/InventoryUiPreview_Default.asset`
- Added preview code:
  - battle/inventory layout profile ScriptableObjects
  - battle/inventory mock preview data ScriptableObjects
  - scene-local preview presenter/controller scaffold
  - editor generator for preview scenes/assets
- Sprite workflow note:
  - `Assets/Sprite` was audited only
  - no auto-assignment or import-setting mutation was performed
  - preview slots support manual Sprite or Texture assignment, which is important because sampled sprite-pack metas were still imported as `textureType: 0`
- Runtime safety:
  - `SampleScene` play path unchanged
  - no BootEntry dependency added to preview scenes
  - no runtime world/battle/inventory truth mutation added
  - no DDOL UI hierarchy added
- Validation snapshot:
  - compile: `PASS (Unity batch compile succeeded on 2026-04-20 via unity-merge-validate.log)`
  - preview generation: `PASS (Unity batch generator created preview scenes and default preview assets on 2026-04-20 via unity-ui-preview-builder.log)`
  - runtime isolation: `PASS (code-path scope only)`

## Scene Architecture Scaffold Follow-Up

- Selected branch: `scaffold-first, no scene cutover`
- Honest scope:
  - add a persistent-root code scaffold without claiming the project is already multi-scene
  - document what should persist, what must unload, and which current files are legacy adapters
  - keep the accepted `SampleScene` flow, battle HUD rail, inventory rail, and result pipeline intact
- Added runtime scaffold:
  - `Assets/_Game/Scripts/App/GameSceneId.cs`
  - `Assets/_Game/Scripts/App/AppRoot.cs`
  - `Assets/_Game/Scripts/App/GameSessionRoot.cs`
  - `Assets/_Game/Scripts/App/RuntimeGameState.cs`
  - `Assets/_Game/Scripts/App/SceneFlowService.cs`
- Added architecture docs:
  - `docs/architecture/scene-ownership-plan.md`
  - `docs/architecture/ddol-policy.md`
  - `docs/architecture/scene-migration-roadmap.md`
  - `docs/architecture/script-naming-policy.md`
- Ownership call:
  - persistent truth stays with runtime owners like `ManualTradeRuntimeState` and canonical handoff contracts
  - scene-local IMGUI shells (`PrototypePresentationShell`, `PrototypeDebugHUD`) remain disposable presenters
  - `BootEntry` and `StaticPlaceholderWorldView` stay classified as legacy adapters/hot spots, not new long-term owners
- Validation snapshot:
  - compile: `PASS (unity-merge-validate.log, 2026-04-20)`
  - runtime cutover: `NOT STARTED`
  - SampleScene preservation: `PASS (code-path scope only)`

## Batch 77.2 Close-Out

- Selected branch: `A with manual-asset guardrails`
- Honest blocker:
  - the current accepted battle HUD rail is still immediate-mode and color-authored
  - user art exists under `Assets/Sprite`, but automatic assignment is unsafe because the pack is not a dedicated runtime-resolved skin path and at least part of it is not guaranteed sprite-imported
- Fix shape:
  - added `BattleUiSkinDefinition`, `BattleUiSkinProvider`, and `BattleUiSkinRenderer`
  - added `InventoryUiSkinDefinition`, `InventoryUiSkinProvider`, and `InventoryUiSkinRenderer`
  - exposed named skin slots for battle panels, command buttons, current-unit card, target-status card, timeline chips, HP bars, and the battle result popover
  - split inventory skinning into its own definition and wired member rows, equipment slots, item rows, and the run-spoils badge
  - wired the active `PrototypeDebugHUD` battle rail through null-safe skin hooks
  - wired the `PrototypePresentationShell` battle result popover through the same skin path
  - left all slots manual and optional so missing art preserves the current accepted visuals
- UI shape changed?: `No layout rewrite; fallback parity preserved when slots are empty`

## Batch 77.2 Validation Snapshot

- Compile: `PASS` (`unity-merge-validate.log`, 2026-04-20)
- Preflight:
  - active battle HUD owner audit: `PASS`
  - battle result popover owner audit: `PASS`
  - inventory overlay interaction-point audit: `PASS`
  - asset path / non-Resources / importer-risk audit: `PASS`
- Static scaffold proof:
  - no random sprite assignment path added: `PASS`
  - missing-slot fallback preserves current draw path: `PASS`
  - battle result popover now shares the battle skin scaffold: `PASS`
  - inventory row/slot/badge skinning is isolated from battle skin ownership: `PASS`
- Manual runtime proof: `DEFERRED`
- Smoke: `DEFERRED`

## Batch 77.1 Close-Out

- Selected branch: `A + B + D`
- Honest blocker:
  - legitimate world selection was still invalidating the correct selected-city surfaces
  - but the click frame also rebuilt world observation, dispatch, projected launch context, and prep readbacks after a run had added party progression / gear / inventory data
- Measured root-cause seam:
  - `StaticPlaceholderWorldView.SetSelected` rebuilt `BuildWorldObservationSurfaceData()` just to refresh board emphasis
  - the same observation build eagerly recreated dispatch + prep surfaces even while the prep board was closed
  - those closed-board paths still pulled `BuildRuntimePartyResolveSurface(...)` for role/loadout/manifest summaries after post-run data existed
- Fix shape:
  - selection now updates board emphasis from the selected marker plus latest writeback without rebuilding the full observation surface
  - `ManualTradeRuntimeState` now caches compact party identity / role / loadout summaries on progression / equipment / inventory updates
  - world-selection dispatch, party-roster, start-context, and closed-board prep summaries now consume those cached readbacks by default
  - detailed party resolve / manifest construction is still forced for actual prep-launch and dungeon handoff paths
- UI shape changed?: `No`

## Batch 77.1 Validation Snapshot

- Compile: `PASS` (`unity-merge-validate.log`, 2026-04-20)
- Static performance proof:
  - selection-board emphasis no longer rebuilds world observation on `SetSelected`: `PASS`
  - closed-board dispatch/prep/start-context paths no longer require `BuildRuntimePartyResolveSurface(...)`: `PASS`
  - launch/prep/dungeon handoff still request detailed party manifests explicitly: `PASS`
- Manual runtime proof: `DEFERRED`
- Smoke: `DEFERRED`
- Batch 78 resume gate: `code-path blocker removed; fresh editor/runtime FPS recapture still recommended before calling the spike fully closed`

## Batch 78 Close-Out

- Selected branch: `A`
- Batch 78 keeps the accepted battle HUD rail and strengthens the already-existing combat pleasure loop instead of adding a second combat system:
  - `read enemy intent`
  - `open Burst Window`
  - `cash role payoff`
- The implementation stays additive and owner-side:
  - enemy focus text now carries a clearer response hint when intent is readable or Burst Window is active
  - action threat summaries now bundle intent-read, threat severity, and recommended response into the existing command/target readback seams
  - attack / skill preview text now shows visible Burst breakdown when payoff is active
  - target outcome preview now carries `Burst Window`, `Opens Burst Window`, or `Consumes Burst Window` on the same HP-after-hit rail
  - `Weak Point` now depends on the visible Burst Window loop instead of quietly receiving a separate low-HP finisher fallback
  - combat logs now celebrate setup/payoff explicitly instead of hiding the loop only inside generic damage text
- UI shape changed?: `No layout rewrite; only compact combat readbacks/log wording were strengthened on the accepted rail`

## Batch 78 Validation Snapshot

- Compile: `PASS`
- Preflight:
  - Batch 77 role identity rail: `PASS (static audit reused; prep / inventory / battle / growth role seams remain wired)`
  - Batch 76 preview/log rail: `PASS (static audit reused and strengthened in the same owner-side preview/log path)`
  - Batch 75.4 world click / modal / pending-spoils guards: `PASS (untouched code-path audit before edits)`
- Targeted gameplay proof:
  - intent read -> setup hint visibility: `PASS (code-path audit)`
  - Burst Window preview breakdown: `PASS (code-path audit)`
  - target panel window-state readback: `PASS (code-path audit)`
  - payoff log celebration: `PASS (code-path audit)`
- Manual UX proof: `DEFERRED`
- Performance proof: `DEFERRED (readback-only owner-side changes; no fresh runtime capture in this turn)`
- Smoke: `DEFERRED`

## Batch 77 Close-Out

- Selected branch: `A`
- Batch 77 keeps the accepted UI rail intact and closes the clearer product bottleneck: the party did not yet read as four distinct RPG roles even though growth, route, and battle-preview rails already existed.
- The implementation stays additive and owner-respecting:
  - `PrototypeRpgRoleIdentity` centralizes static role fantasy, gear preference, battle-hint, route-fit, and growth-meaning text
  - Expedition Prep now prefers a compact staged-party role summary instead of generic party composition wording
  - route-fit readback now carries role-aware guidance for safer, risky, and mixed-pressure routes without changing route mechanics
  - inventory/equipment now surfaces role identity, preferred stats, and per-item fit text without restoring the removed comparison panel
  - battle current-actor and command-detail readbacks now explain what the active role wants to do on this turn
  - growth/result reveal text now explains why a stat gain matters for that role instead of only listing raw deltas
- UI shape changed?: `No rollback and no layout rewrite; only readback content was strengthened on the current accepted rail`

## Batch 77 Validation Snapshot

- Compile: `PASS`
- Preflight:
  - combat preview visibility rail: `PASS (static audit of actor stats, preview, target after-hit, formula/growth, and applied log path)`
  - 75.4 modal / no-op click / pending-spoils guards: `PASS (code-path audit reused before edits)`
  - 74 / 74.1 inventory overlay guardrails: `PASS (surface/input/render audit reused before edits; comparison panel remained removed)`
- Targeted role-identity proof:
  - Expedition Prep staged party summary: `PASS`
  - Inventory selected member + selected item fit readback: `PASS`
  - Battle current actor + command detail role hints: `PASS`
  - Growth / result role-meaning readback: `PASS`
- Manual UX proof: `DEFERRED`
- Performance proof: `DEFERRED (static/cache-oriented implementation only; no fresh runtime capture in this turn)`
- Smoke: `DEFERRED`

## Batch 76 Close-Out

- Selected branch: `A`
- Batch 76 keeps the accepted battle HUD rail and exposes combat growth through the current runtime truth instead of adding a new combat system.
- Current battle HUD now surfaces:
  - current actor resolved stat line
  - compact stat-source line for the active unit
  - attack / skill preview text in the command flyout
  - formula / growth contribution readback in the same flyout
  - target preview outcome during target selection, including HP after-hit or would-defeat messaging
- Actual combat logs now reuse the same resolved preview source so the visible battle message and the real applied result stay on the same stat rail.
- One narrow runtime seam fix was required:
  - shared-skill resolution now prefers the resolved runtime member skill power before falling back to static skill-definition power, so growth/equipment no longer disappear on the skill path
- UI shape changed?: `Preserved current accepted HUD shape; only stat-feedback/readback content was added`

## Batch 76 Validation Snapshot

- Compile: `PASS`
- Static audit: `PASS`
- Attack preview parity: `PASS`
- Skill preview parity: `PASS`
- Growth/equipment surfacing: `PASS`
- Manual runtime proof: `DEFERRED`
- Smoke: `DEFERRED` because this close-out used compile + code audit, but no fresh manual battle playthrough was executed in this turn

## Batch 75.4 Close-Out

- Selected branch: `A`
- Batch 75.4 closes three blocking UX/runtime seams without changing the accepted dungeon/battle rail:
  - world-map no-op clicks no longer invalidate the city/world readback cache
  - the battle result popover now behaves as a real modal instead of allowing background dungeon progression
  - mid-run reward language now matches the actual storage truth by surfacing `Run Spoils` / `pending extraction` wording instead of fake acquisition claims
- The world-map fix stays narrow:
  - `SelectAtScreenPosition(...)` and `SetSelected(...)` now report whether selection state actually changed
  - `BootEntry.HandleSelectionInput()` only invalidates cached city/world surfaces when selection changed
  - repeated empty clicks after the selection is already cleared now stay on the no-op path
- The reward-truth fix stays honest:
  - immediate battle popups now say `Run Spoils` / `Pending ...`
  - encounter result popovers say `pending extraction` for run spoils and `finalizes at run result` for elite reward reservation
  - the inventory surface reuses current run truth to show pending run spoils during dungeon play without promoting them to permanent inventory early
- The popover modal fix now blocks the old click-through path:
  - non-battle dungeon shells return early while the popover is visible
  - the popover overlay consumes mouse events in OnGUI
  - only the documented close inputs, plus the already-approved `[I]` inspect handoff, remain active while the popover is open
- UI shape changed?: `No layout rollback; current accepted rail preserved with modal behavior tightened and reward wording clarified`

## Batch 75.4 Validation Snapshot

- Compile: `PASS`
- World-map click/perf audit: `PASS`
- No-op cache invalidation guard: `PASS`
- Popover modal click-through guard: `PASS (code-path + OnGUI guard)`
- Pending spoils wording / visibility parity: `PASS`
- Manual runtime performance proof: `DEFERRED`
- Manual popover click-through proof: `DEFERRED`
- Smoke: `DEFERRED`

## Batch 75.3 Close-Out

- Selected branch: `B + C hybrid`
- Batch 75.3 upgrades the centered encounter-result popover into a real readback moment without reopening battle math, drop systems, or HUD layout direction.
- The implementation stays on the current accepted rail:
  - encounter / defeat / retreat popovers still come from one cached shell snapshot
  - new text fields are additive and built once at result production time, not during per-frame GUI work
  - missing encounter-time growth / gear truth is shown honestly as pending instead of faked
- The upgraded popover now carries:
  - concrete header + subtitle context
  - result / rewards / drop readback
  - party HP and combat consequence summary
  - contribution / battle highlight text from current-battle event truth when available
  - growth carryover status and next-step guidance
  - optional `[I]` inspect-equipment handoff directly from the popover when inventory access is available
- UI shape changed?: `No layout rollback; centered popover preserved with richer content and a small subtitle/body sizing adjustment`

## Batch 75.3 Validation Snapshot

- Compile: `PASS`
- Active rail audit:
  - popover producer / cache seam audited before edits: `PASS`
  - current battle / party / next-goal truth reused without new gameplay ownership: `PASS`
  - encounter-time XP / gear finalization stays pending where truth is unavailable: `PASS`
- Targeted code proof:
  - popover body now consumes cached result / reward / party / combat / next-step fields: `PASS`
  - `[I]` inspect handoff is handled inside the popover input gate, so the old global inventory toggle block is not bypassed accidentally: `PASS`
  - centered modal sizing widened only enough to fit the richer compact readback at 1920x1080 intent: `PASS (code-path audit)`
- Manual UX proof: `DEFERRED`
- Performance proof: `DEFERRED (compile + code-path audit only; no fresh runtime FPS capture in this turn)`
- Smoke: `DEFERRED`

## Batch 75.2 Close-Out

- Selected branch: `A`
- Latest git audit confirms that the current `HEAD` already contains the intended Batch 75.1 reward-feedback work:
  - centered battle-end popover state and shell rendering
  - enemy defeat reward floating text on real reward truth
  - inventory/equipment surface without the old visible comparison panel
- No gameplay/system edits were required in 75.2.
- The latest git evidence used for close-out was:
  - `git show --stat --oneline HEAD`
  - `git grep`/code audit for `IsBattleResultPopover`, `Encounter Cleared`, inventory surface ownership, and comparison-panel removal
  - existing docs close-out for Batch 75.1
- Dedicated `docs/runtime/batch75-1*` handoff note is not present, but the mainline proof now lives in the canonical status/content docs instead.
- UI shape changed?: `No additional runtime UI change in 75.2; audit/docs close-out only`

## Batch 75.2 Validation Snapshot

- Latest commit audited: `07b43e1 Add expedition prep and battle handoff flow`
- Git state: `root repo clean for tracked files used by 75.1/75.2 audit; only nested worktree pointers remain modified`
- Compile: `REUSED PASS from latest 75.1 compile proof; no new gameplay code change in 75.2`
- Manual UX proof: `REUSED from current accepted rail evidence; no fresh manual pass in 75.2`
- Performance proof:
  - reward-feedback loop shows no new audit evidence of a 75.1-specific regression
  - the previously known battle-scene-only FPS issue still exists and remains separate from this close-out
- Smoke: `NOT RERUN`

## Batch 75.1 Close-Out

- Selected branch: `B + C hybrid`
- Batch 75.1 is a UX feedback fix on top of the accepted rail, not a new battle, inventory, or reward system batch.
- It closes three narrow visibility gaps without changing reward math:
  - enemy defeat now reuses the existing battle floating popup rail to show real reward truth such as `Loot +3`
  - each encounter victory now opens a centered battle-result popover built once from cached encounter/run summary fields
  - the visible inventory/equipment surface no longer carries the large `Comparison` panel
- The implementation stays honest to existing ownership:
  - kill popups read the actual monster `RewardAmount` / `RewardResourceId`
  - encounter / defeat / retreat summaries read one cached popover snapshot instead of rebuilding per frame
  - inventory still consumes the same runtime-owned comparison/action truth, but compresses it into selected-item detail text instead of a separate panel
- Final-elite handling stays non-duplicative:
  - elite encounter victory still shows the encounter popover
  - run clear continues to the existing run result later
  - run defeat / retreat now surface the compact battle-end popover before the run result shell
- UI shape changed?: `Yes, centered battle-result popover plus comparison-panel deletion on the current inventory surface`

## Batch 75.1 Validation Snapshot

- Compile: `PASS`
- Targeted audit:
  - enemy defeat popup uses actual reward truth and does not double-roll loot: `PASS`
  - encounter victory creates a single modal popover state and clears it on explicit close input: `PASS`
  - run defeat / retreat now pause on the compact popover before the existing result shell: `PASS`
  - inventory still keeps equip / unequip flow while the separate comparison panel is removed: `PASS`
- Manual verification: `DEFERRED`
- Performance proof: `DEFERRED (no fresh scene-by-scene FPS capture was taken in this turn)`
- Smoke: `DEFERRED`

## Batch 75 Close-Out

- Selected branch: `C`
- Batch 75 does not add a new RPG mechanic, inventory framework, or battle formula.
- It closes the missing reveal seam between already-landed progression/equipment truth and the player-facing result/return surfaces.
- The mainline rail now caches one growth/reward readback set per resolved run:
  - member XP gain and level change
  - compact stat delta callouts
  - item drop and equip/store outcome
  - next-run meaning text
  - `[I]` inspect-equipment hint
- The reveal is intentionally owner-driven:
  - `ResultPipeline` now builds cached growth reveal summaries once per result refresh
  - `ManualTradeRuntimeState` refreshes those summaries again after auto-equip/store resolution, so return readback does not lose the item/build-change moment
  - dungeon result, post-run return reveal, and prep return-consume text now read from the same cached fields
- The missing return seam that made this an honest Branch C was:
  - city/world return helpers for gear reward / equip swap / continuity were still returning `None`
  - result/readback did not yet have dedicated growth-reveal fields, so the player had to infer change from scattered text
- UI shape changed?: `Yes, readback-only strengthening on existing result/return/prep surfaces`

## Batch 75 Validation Snapshot

- Compile: `PASS`
- Targeted audit:
  - Batch 72 progression storage and post-run writeback still exist: `PASS`
  - Batch 72.1 battle-return continuity payload remains intact while growth reveal fields are additive: `PASS`
  - Batch 72.2 / 72.3 performance guardrail is respected by cached result summaries instead of per-frame recompute: `PASS`
  - Batch 73 gear reward / auto-equip / stat-delta rail still owns the item/build-change truth: `PASS`
  - Batch 74 / 74.1 inventory surface remains the inspect target instead of being rebuilt inside the result screen: `PASS`
- Manual verification: `DEFERRED`
- Performance proof: `DEFERRED (no fresh scene-by-scene FPS capture was taken in this turn)`
- Smoke: `DEFERRED`

## Batch 74 Close-Out

- Selected branch: `A`
- Batch 74 turns the hidden Batch 73 inventory truth into a real player-facing character equipment surface without replacing the accepted mainline HUD rail.
- The new surface stays on canonical runtime ownership:
  - `ManualTradeRuntimeState` remains the truth owner for party-carried gear, equipped slot state, and comparison inputs
  - `StaticPlaceholderWorldView` now exposes a cached inventory surface adapter instead of minting a second equipment store
  - `BootEntry` and `BootstrapSceneStateBridge` only forward input + display access
- Player-facing surface added:
  - `[I]` open / close inventory
  - `[Esc]` close
  - `[Q/E]` cycle party member
  - `[1-7]` select equipment slot
  - `[Up/Down]` move through compatible inventory items
  - `[Enter]` equip selected item
  - `[U]` or `[Backspace]` unequip selected slot
- Availability by stage:
  - `WorldSim`: full equipment management
  - `ExpeditionPrep`: full equipment management
  - `DungeonRun explore / route / event / result`: full equipment management
  - `BattleScene`: read-only inspection only
- The surface is intentionally additive, not a layout rollback:
  - member column
  - seven-slot equipment grid
  - comparison / action panel
  - party inventory list
  - cached detail/readback blocks
- UI shape changed?: `Yes, additive overlay only`

## Batch 74 Validation Snapshot

- Compile: `PASS`
- Targeted audit:
  - inventory/equipment surface reuses canonical world/runtime truth instead of duplicating it: `PASS`
  - manual equip / unequip now flows through world and dungeon shell input without changing battle legality ownership: `PASS`
  - battle context correctly gates the surface to read-only inspection: `PASS`
  - presentation rebuild is cached off party inventory revision plus UI selection state: `PASS`
- Manual verification: `DEFERRED`
- Smoke: `PARTIAL (Batch10 smoke passed WorldSim -> CityHub -> ExpeditionPrep -> DungeonRun -> BattleScene -> DungeonRun continuity checkpoints, then the existing runner timed out later at ResolveCoreLoop)`

## Batch 73 Close-Out

- Selected branch: `A`
- Batch 73 finishes the next honest RPG seam on top of the existing 72 / 72.1 rail instead of reopening UI layout or worldmap work.
- World/runtime truth now owns:
  - party-carried gear inventory
  - per-member equipment slot state
  - slot legality and auto-equip resolution
  - persistent carryover into the next prep / run
- Character equipment is now explicit across seven slots:
  - `head`
  - `left arm`
  - `right arm`
  - `torso`
  - `belt`
  - `pants`
  - `shoes`
- Result writeback now makes gear progression tangible:
  - successful runs can mint real equipment drops
  - stronger drops auto-equip onto the correct member + slot
  - replaced gear stays in the party inventory if it is no longer equipped
- Readback proof stays on the accepted UI rail:
  - auto-equip now spells out visible stat deltas such as `ATK 5 -> 6`
  - prep / party loadout summary now echoes slot-aware equipment state + carried inventory
  - result reward lines now explain gear reward, equip swap, build change, and carry continuity from the same runtime truth
  - next-run prep / battle readback now shows resolved `Lv / HP / ATK / DEF / SPD` plus compact gear evidence
- UI shape changed?: `No`

## Batch 73 Validation Snapshot

- Compile: `PASS`
- Targeted audit:
  - party inventory truth persists on the canonical world/runtime rail: `PASS`
  - per-member slot equipment affects next-run numbers and derived party power: `PASS`
  - prep / party / result readback now exposes gained gear, equipped upgrades, and stored carryover: `PASS`
  - Batch 72.1 result handoff remains on the stable return path after runtime writeback sync: `PASS`
- Manual verification: `DEFERRED`
- Smoke: `PARTIAL (Batch10 smoke reached the DungeonRun -> BattleScene -> DungeonRun continuity checkpoint and passed it, then the runner timed out later at ResolveCoreLoop)`

## Batch 72 Close-Out

- Selected branch: `A`
- Batch 72 closes party growth / differentiation on the existing runtime seam instead of inventing a second progression framework.
- World / prep readback now answers the party question in one throughline:
  - archetype + promotion identity
  - why that identity fits the route
  - what next edge the current promotion state is unlocking
- Battle command readback now carries party doctrine + member battle-role context on top of the existing burst-window setup/payoff loop.
- Result / return readback now preserves party growth consequence instead of letting city writeback text fully overwrite it:
  - result progression now prefers next-run consequence before raw applied text
  - return aftermath / next-prep follow-up can now echo party growth as well as city impact
- UI shape changed?: `No`

## Batch 72 Validation Snapshot

- Compile: `PASS`
- Targeted audit:
  - dispatch / recommendation / route-fit party throughline: `PASS`
  - ExpeditionPrep staged-party / launch-manifest growth readback: `PASS`
  - battle command setup-payoff text varies by party identity: `PASS`
  - result / next-prep growth consequence survives the return path: `PASS`
- Manual verification: `DEFERRED`
- Smoke: `DEFERRED`

## Batch 72.1 Close-Out

- Selected branch: `B`
- Batch 72.1 adds one thin progression seam on top of the current Batch 72 party rail instead of opening a full inventory or equipment framework.
- Character runtime progression now persists on the canonical world/runtime path:
  - per-member `Level`
  - per-member `Experience`
  - per-member next-level threshold
  - role/archetype-biased growth bonuses for `HP / Attack / Defense / Speed`
- Battle/result return now converts combat contribution into real post-run growth:
  - XP gain
  - level-up resolution
  - stat-growth readback
  - lightweight loot-drop bundles
- Loot is intentionally staged as a hidden pending stash instead of a visible inventory screen:
  - stash bundles accumulate on the party runtime
  - result/readback text can surface the reward without changing the accepted UI layout
  - the reward schema is now ready to hand into a future Batch 73 inventory/equipment surface
- UI shape changed?: `No`

## Batch 72.1 Validation Snapshot

- Compile: `PASS`
- Targeted audit:
  - world/runtime party seam now persists per-member level/xp: `PASS`
  - result pipeline now resolves XP + level-up + stat growth on the canonical return path: `PASS`
  - lightweight loot/drop bundles now flow into a hidden pending stash: `PASS`
  - existing prep/result readback surfaces can now echo next-level and loot continuity without a UI redesign: `PASS`
- Manual verification: `DEFERRED`
- Smoke: `FAIL (existing BattleScene -> DungeonRun return continuity check still breaks before full result/world re-entry proof)`

## Batch 72.1 Continuity Repair Note

- The first reported blocker after Batch 72.1 was not the progression seam itself, but the adjacent `DungeonRun -> BattleScene -> DungeonRun` return seam.
- The concrete drift was:
  - battle return could rebuild from the current room after combat
  - the rebuilt snapshot could fall forward to the next room context
  - `EncounterId / EncounterName / EncounterRoomType / Objective` could therefore collapse away from the original battle-entry context
- The active fix keeps this narrow:
  - battle result snapshot rebuild now prefers the stable battle-entry request before falling back to current-room data
  - battle return payload now prefers the same stable battle-entry request for encounter identity, room type, objective, and summary naming
- UI shape remains unchanged.
- Next required proof: rerun the Batch 10 smoke after closing any already-open Unity editor instance for `Project_AAA`, so the repaired continuity seam can be verified end-to-end.

## Batch 70 Close-Out

- Selected branch: `A`
- Selected signature pair: `city-b -> dungeon-beta`
- Integration polish focused on the existing signature loop instead of adding a new mechanic seam.
- World/result readback polish:
  - priority-board summary now carries urgency, latest result evidence, and current route answer in the same line
  - selected-city pressure-board summary now mirrors that same evidence-first structure
  - world alert/city summaries now read more like a loop close-out than a neutral status snapshot
- Why Beta was chosen:
  - its guarded vs greedy split is still the clearest operating-scenario contrast on the surfaced rail
  - the City B pressure -> route answer -> burst payoff -> restart feedback chain is the easiest 3-5 minute demo story
- UI shape changed?: `No`

## Batch 70 Validation Snapshot

- Compile: `BLOCKED (Unity batch compile aborted because the project was already open in another editor instance)`
- Targeted audit:
  - 66~69 landing on current branch: `PASS`
  - Beta world -> prep -> route -> battle -> result loop continuity: `PASS`
  - result-pressure board summary alignment: `PASS`
- Manual verification: `DEFERRED`
- Smoke: `DEFERRED`

## Batch 70 Why It Matters

- Batch 66~69 already landed on the current branch, so Batch 70 did not reopen that work or fake a new demo seam.
- The strongest current signature rail is `city-b -> dungeon-beta`, because the world pressure, guarded vs greedy route split, burst-window combat payoff, and restart-focused world feedback already exist on the same canonical path.
- The remaining gap was presentation parity: the result-pressure board was carrying the right data, but not summarizing the latest return as directly as the surrounding prep/route/battle readback.
- Batch 70 closes that gap without changing the accepted runtime UI or widening the surfaced portfolio.

## Batch 69 Close-Out

- Selected branch: `A`
- World surfaces changed:
  - top world alert ribbon
  - world selection brief
  - world selection detail
  - world overview brief
  - selected-city world source snapshot
  - world observation priority board
- Pressure signals surfaced:
  - priority city selection from the existing world board read model
  - urgency / top bottleneck framing
  - route answer / recommendation answer
  - recent-result evidence
  - party readiness / recovery pressure
  - next-action follow-through
- Recent-result evidence surfaced through:
  - selected city pressure-board summary
  - top priority-board alert summary
  - world return handoff brief/detail
  - world observation snapshot summary
- Party-readiness surfaces affected:
  - alert ribbon footer
  - selected city brief/detail
  - world overview summary
- UI shape changed?: `No major layout change`

## Batch 69 Validation Snapshot

- Compile: `PASS`
- Targeted audit:
  - CityDecision read model reused on world/city board surfaces: `PASS`
  - recent result -> recommendation / next action alignment: `PASS`
  - party readiness surfaced in the same decision context: `PASS`
- Manual verification: `DEFERRED`
- Smoke: `DEFERRED`

## Batch 69 Why It Matters

- The world map now reads less like a neutral bridge and more like a decision board.
- The player can now read, on the same surface:
  - which city is currently the priority
  - why that city matters now
  - what route / dispatch answer the board is pointing at
  - what the latest expedition changed
  - whether the party is ready to answer that pressure
- This stays on the current world/city decision seam instead of adding a new strategy layer, fake urgency text, or a cosmetic-only panel expansion.

## Batch 68 Close-Out

- Selected branch: `A`
- Surfaced routes touched:
  - `city-a -> dungeon-alpha -> safe`
  - `city-a -> dungeon-alpha -> risky`
  - `city-b -> dungeon-beta -> risky`
  - `city-b -> dungeon-beta -> safe`
- Scenario framing added to the canonical route-meaning rail:
  - short scenario label
  - choose-when text
  - watch-out text
  - follow-up / next-dispatch trace
  - party-fit text
  - combat-plan text
- Party-fit surfaces affected:
  - route option reward preview
  - recommendation reason
  - dispatch route-fit summary
  - expected-need / projected-outcome readback
- City-pressure surfaces affected:
  - recommended route summary
  - recommendation reason
  - projected outcome summary
  - CityHub surfaced route-variant opportunity reason
- UI shape changed?: `No`

## Batch 68 Validation Snapshot

- Compile: `PASS`
- Validator: `PASS`
- Authoring validation:
  - `ProfilesChecked=3`
  - `SupportedVariantsChecked=1`
  - `WarnCount=0`
  - `FailCount=0`
- Manual verification: `DEFERRED`
- Smoke: `DEFERRED`

## Batch 68 Why It Matters

- The current four surfaced routes now read more like operating scenarios than reward table variants.
- Each surfaced route now answers:
  - when to choose it
  - what risk it asks you to absorb
  - what kind of party it prefers
  - what kind of next-dispatch footprint it leaves behind
- The change stays on the existing canonical authoring rail instead of adding another UI-only text layer or widening the surfaced route portfolio.

## Batch 67 Close-Out

- Selected branch: `A`
- Core combat pleasure locked to:
  - `read intent -> create window -> cash payoff`
- Setup mechanics added/used:
  - enemy next-action preview now refreshes during party turns
  - `Power Strike` reads that preview and opens a burst window on the chosen target
  - exposed targets now carry explicit burst-window state/readback instead of hiding setup only in raw numbers
- Payoff mechanics added/used:
  - `Weak Point` now consumes the burst window for a larger single-target payoff
  - `Arcane Burst` now gains bonus splash damage on exposed targets without consuming the window
  - basic `Attack` now gets only a light exposed bonus, so it stays readable but does not replace the setup-payoff loop
- Party roles affected:
  - `Warrior`: setup opener
  - `Rogue`: burst cash-in finisher
  - `Mage`: exposed-target splash follow-through
  - `Cleric`: recovery + burst-window stabilization
- Enemy intent/readback surfaces affected:
  - enemy roster state label
  - target focus rule text
  - command/context threat summary
  - command detail effect text
- UI shape changed?: `No`

## Batch 67 Validation Snapshot

- Compile: `PASS`
- Targeted audit:
  - intent preview during party turn: `PASS`
  - burst-window lifetime and consumption: `PASS`
  - command/target readback for setup vs payoff: `PASS`
- Manual verification: `DEFERRED`
- Smoke: `DEFERRED`

## GPT Quick Judge

- `city-b -> dungeon-beta -> safe`: `PASS:fully-canonical-and-surfaced`
- Surfaced portfolio count: `4`
- Meaningfully distinct surfaced opportunities: `4`
- Canonical but not surfaced on current Alpha/Beta `safe/risky` rail: `None`
- Surfaced using fallback: `None`
- Surfaced consumer mismatch: `None`
- Expansion gate: `C:tooling-next`
- Alpha safe/risky content identity: `route-specific shared encounter/city meaning aligned`
- Beta safe/risky content identity: `route-specific shared encounter/city meaning aligned`

## Current Surfaced Portfolio

- `city-a -> dungeon-alpha -> safe`
- `city-a -> dungeon-alpha -> risky`
- `city-b -> dungeon-beta -> risky`
- `city-b -> dungeon-beta -> safe`

## Draft Helper State

- Current helper status: usable
- Current draft count: `1`
- Promotable drafts on current surfaced rail: `0`
- Blocked drafts: `1`
- Blocked by canonical owner: `1`
- Supported rail slots: `4`
- Occupied supported rail slots: `4`
- Open supported rail slots: `None`
- Open supported resolver keys: `None`
- Promotion recommendation: `no-open-supported-resolver-key-on-current-rail`

## Batch 66 Close-Out

- Selected branch: `B`
- Party seam used or created:
  - `Assets/_Game/Scripts/Rpg/PrototypeRpgPartyData.cs`
  - `Assets/_Game/Scripts/Rpg/PrototypeRpgRuntimeResolveSurface.cs`
  - `Assets/_Game/Scripts/World/StaticPlaceholderWorldView.PartyRuntimeSurface.cs`
- Party archetypes now on the canonical runtime path:
  - `Bulwark Crew`
  - `Outrider Cell`
  - `Salvager Wing`
- Lightweight growth axis now on the canonical runtime path:
  - `Recruit Frame -> Field Promotion -> Elite Promotion`
- World runtime now stores per-party:
  - archetype id
  - promotion state id
  - derived power
  - derived carry
- World / prep / battle readback now all resolve from the same runtime party seam instead of rebuilding a raw placeholder party in each surface.
- Route recommendation and expected-impact text remain on the current surfaced four-route portfolio, but now explicitly explain when the current party fits or pushes against the chosen lane.
- Battle runtime now resolves party members from the promotion-aware RPG surface, so party differentiation shows up as actual stat/loadout/runtime differences instead of flavor-only text.

## Batch 66 Validation Snapshot

- Compile: `PASS`
- Targeted audit: `PASS`
- Party registry/runtime seam: `PASS`
- World + prep route-fit readback: `PASS`
- Battle party runtime promotion readback: `PASS`
- Smoke: `DEFERRED` because this batch changed runtime seam + readback wiring, but no manual playthrough was executed in this close-out

## Batch 66 Follow-Up Hook

- Keep the current surfaced Alpha/Beta four-route portfolio unchanged.
- If Batch 67 expands this area, prefer:
  - stronger route-specific consequences for each party archetype
  - longer-lived specialization persistence/save hook
  - additional surfaced explanation only if it stays on the same canonical party seam

## Batch 61 Close-Out

- Batch 61 did not add a new surfaced opportunity.
- Batch 61 deepens the existing Beta surfaced pair instead of widening the portfolio.
- Beta risky now reads as the raid line:
  - raider gate breach
  - plunder cache tempo
  - crossfire vault
  - chief's vault payout
- Beta safe now reads as the guarded restart line:
  - scout gate watch
  - watchfire and cache stability
  - guarded vault hold
  - captain's hold finish
- The current Beta pair now resolves through route-specific shared encounter/city meaning where the generic shared profiles had made the dungeon feel thin.

## Batch 61 Validation Snapshot

- Compile: `PASS`
- Validator: `PASS`
- Surfaced matrix summary: `PASS`
- Surfaced portfolio summary: `PASS`
- Representative chain status summary: `PASS`
- Representative reference trace: `PASS`
- Smoke: `DEFERRED` because this batch only changed content/data and docs

## Batch 61 Recommendation

- Recommended direction: `continue authored dungeon density on the current Alpha/Beta rail before reopening runtime/UI cleanup`
- Do not open a fake surfaced-expansion batch just because the content rail now reads better; the surfaced portfolio is still intentionally the same four routes.

## Batch 46 Close-Out

- Batch 46 did not add a new surfaced opportunity.
- Batch 46 adds a draft-promotion preflight summary on top of readiness/context.
- Draft tooling now reports:
  - supported rail slot inventory
  - occupied vs open supported slot counts
  - open supported resolver keys
  - whether the selected draft sits on an already-owned supported slot or outside the current rail
  - a saturated-rail recommendation when `Promotable drafts on current rail` honestly remains `0`

## Batch 46 Validation Snapshot

- Compile: `PASS`
- Validator: `PASS`
- Draft readiness summary: `PASS`
- Draft promotion preflight summary: `PASS`
- Draft promotion context summary: `PASS`
- Smoke: `DEFERRED` because this batch only changed editor/debug/helper and docs

## Batch 46 Recommendation

- Recommended direction: `retarget the draft beyond the current surfaced rail` or `widen the surfaced route seam intentionally`
- Do not open another surfaced-expansion batch on the current Alpha/Beta rail until the draft helper reports an open supported resolver key or the surfaced seam itself is deliberately widened.
