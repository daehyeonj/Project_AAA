# Battle UI Skinning

## Intent

Batch 77.2 adds a battle-scene skinning scaffold for the current accepted IMGUI battle HUD rail.

This is not a final art pass.

The goal is:

- keep the current battle UI fully usable when no art is assigned
- expose named asset slots for the high-value battle surfaces
- let future sprite/texture swaps happen without editing battle logic
- avoid random automatic asset assignment from `Assets/Sprite`

## Active Owner Audit

Current battle-facing owners on the active rail:

- `Assets/_Game/Scripts/Debug/PrototypeDebugHUD.cs`
  - draws the active player-facing battle HUD shell
  - draws `Command Selection`
  - draws `Current Unit`
  - draws `Target Status`
  - draws the top timeline strip and queue chips
- `Assets/_Game/Scripts/UI/Battle/PrototypePresentationShell.DungeonRun.cs`
  - owns the battle result popover overlay
  - still contains a presentation-shell battle renderer, but that is not the currently accepted player-facing battle HUD rail
- `Assets/_Game/Scripts/UI/Inventory/PrototypePresentationShell.Inventory.cs`
  - owns the inventory/equipment overlay interaction during battle-adjacent moments

## Asset Audit

Observed asset situation during Batch 77.2:

- art is present under `Assets/Sprite`
- the folder is under normal `Assets`, not a dedicated `Resources` path
- naming conventions are fairly clear, for example `UI_TravelBook_*`
- at least some imported files are still default textures rather than guaranteed `Sprite (2D and UI)` imports

Because of that, this scaffold does not perform runtime asset discovery or `Resources.Load(...)` guesses.

## New Files

- `Assets/_Game/Scripts/UI/Battle/BattleUiSkinDefinition.cs`
- `Assets/_Game/Scripts/UI/Battle/BattleUiSkinRenderer.cs`
- `Assets/_Game/Scripts/UI/Battle/BattleUiSkinProvider.cs`

## Slot Map

The skin definition exposes these high-value slots:

- `PanelBackground`
- `PanelHeader`
- `PanelAccent`
- `CommandButtonNormal`
- `CommandButtonHover`
- `CommandButtonSelected`
- `CommandButtonDisabled`
- `CurrentUnitCard`
- `TargetStatusCard`
- `TimelineChip`
- `TimelineChipCurrent`
- `TimelineChipEnemy`
- `HpBarBackground`
- `HpBarFill`
- `PopupBackground`

Current wired surfaces:

- battle shell panels/cards on the active HUD rail
- command button backgrounds
- current unit card
- target status card
- timeline queue chips/cards
- HP bars
- battle result popover background/accent

Inventory-specific skinning is intentionally split into:

- `docs/ui/inventory-ui-skinning.md`

## How To Assign A Skin

1. Create a new asset from:
   - `Create > Project AAA > UI > Battle UI Skin`
2. Assign the asset to the active runtime component:
   - `PrototypeDebugHUD` for the accepted battle HUD rail
3. If you also want the presentation-shell battle popover path to use the same explicit asset reference, assign the same asset to:
   - `PrototypePresentationShell`
4. Populate only the slots you want to override.

If a slot is left empty, the code falls back to the currently accepted color-based rendering.

## Sprite Vs Texture Assignment

Each slot accepts either:

- a `Sprite`
- or a `Texture2D`

Why both exist:

- some current user art appears to still be imported as default textures
- the scaffold should remain usable without forcing a bulk asset reimport pass in this batch

Preferred long-term path:

- use `Sprite (2D and UI)` imports for finalized UI assets when possible

Safe short-term path:

- assign a texture directly to the slot if the pack is still using default texture imports

## Performance Rule

- do not load UI art every frame
- do not add runtime folder scans or random asset matching
- assign references in the skin asset once
- leave missing slots empty and let the fallback path render the current UI

## What This Batch Intentionally Did Not Do

- no random sprite selection from `Assets/Sprite`
- no battle layout rewrite
- no combat logic change
- no modal behavior rollback
- no forced atlas slicing
- no automatic importer mutation

## Safe Next Step

The next art-facing pass can stay narrow:

- choose a small set of final backgrounds for `PanelBackground`, `CommandButton*`, `CurrentUnitCard`, `TargetStatusCard`, `TimelineChip*`, and `PopupBackground`
- verify them in-editor
- continue separately with the inventory skin asset once battle art direction is confirmed
