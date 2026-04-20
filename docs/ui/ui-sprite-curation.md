# UI Sprite Curation

## Purpose

Batch 77.5 adds a curated sprite staging rail for manual battle/inventory skin assignment.

This is not a final art pass.
This is not random design assignment.
This is a project-owned copy area so the human designer can work from a narrowed set instead of hunting through every raw pack file.

## Current Source Audit

### Raw source packs kept intact

- `Assets/Sprite/Complete_UI_Book_Styles_Pack_Free_v1.0`
- `Assets/Sprite/Free - Raven Fantasy Icons`
- `Assets/Sprite/Tiny RPG Character Asset Pack v1.03 -Free Soldier&Orc`

### Curated source chosen in this batch

- `Complete_UI_Book_Styles_Pack_Free_v1.0/01_TravelBookLite`

Why this pack was chosen first:

- the file names clearly describe UI usage
- panels, frames, buttons, slots, bars, and icons are easy to narrow without guessing
- it supports both battle and inventory skin work

### Audited but not curated in this batch

- `Free - Raven Fantasy Icons`
  - too many numeric filenames for safe narrowing without a human art pass
- `Tiny RPG Character Asset Pack v1.03 -Free Soldier&Orc`
  - character/world art, not a primary UI skin source

## Curated Folder Rail

- `Assets/_Game/Content/UI/Sprites/Battle/*`
- `Assets/_Game/Content/UI/Sprites/Inventory/*`
- `Assets/_Game/Content/UI/Sprites/Shared/*`
- `Assets/_Game/Content/UI/Sprites/_SourceMap/*`

## Current Import Audit

Sampled raw TravelBook UI files still show:

- `textureType: 0`
- `spriteMode: 0`
- `spritePixelsToUnits: 100`
- `alphaIsTransparency: 0`
- `filterMode: 1`
- `textureCompression: 1`

Meaning:

- the source files are still treated like generic textures rather than guaranteed UI sprites
- the current skin definitions can still accept them through the `Texture` field
- no import settings were changed in Batch 77.5

## Recommended Import Settings For Curated Copies

Only if the designer wants to use the `Sprite` field rather than the `Texture` field:

- Texture Type: `Sprite (2D and UI)`
- Sprite Mode: `Single`
- Alpha Is Transparency: `true`
- Mesh Type: `Full Rect`
- Filter Mode: `Bilinear` for this painted UI pack
- Compression: `None` or highest-quality testing-safe option

Do this on curated copies only.
Do not batch-edit the raw source pack.

## Manual Assignment Workflow

1. Open `Assets/_Game/Scenes/Preview/BattleUiPreviewScene.unity` or `Assets/_Game/Scenes/Preview/InventoryUiPreviewScene.unity`.
2. Select the relevant skin asset:
   - `Assets/_Game/Content/UI/Skins/Battle/BattleUiSkin_Default.asset`
   - `Assets/_Game/Content/UI/Skins/Inventory/InventoryUiSkin_Default.asset`
3. Use the curated folders under `Assets/_Game/Content/UI/Sprites` as the first search surface.
4. Read the slot maps:
   - `docs/ui/battle-skin-slot-map.md`
   - `docs/ui/inventory-skin-slot-map.md`
5. If a candidate still needs better import settings, change the curated copy only.
6. Drag the chosen asset into the relevant `Sprite` or `Texture` field manually.
7. Press Play in the preview scene and tune layout after assignment.

## What This Batch Intentionally Does Not Do

- no runtime sprite loading
- no random auto-assignment to skin slots
- no changes to `SampleScene`
- no `Resources.Load(...)` UI rail
- no raw source move/rename/delete
- no broad import-setting rewrite

## Related Docs

- `docs/ui/battle-skin-slot-map.md`
- `docs/ui/inventory-skin-slot-map.md`
- `Assets/_Game/Content/UI/Sprites/_SourceMap/ui-sprite-candidate-map.md`
