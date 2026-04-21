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

## License / Attribution Audit

### TravelBook Lite

- source pack: `Assets/Sprite/Complete_UI_Book_Styles_Pack_Free_v1.0`
- evidence: `License.txt`
- visible license status: usable for personal and commercial projects
- visible credit requirement: appropriate credit or a link to the product page, plus note if changes were made
- visible restriction: do not resell or publish the same material, or adapted/remixed versions of the material, as a standalone asset pack

Working rule for this repo:

- curated copies should keep an attribution note in project docs
- do not claim repo redistribution is release-safe beyond the visible license text
- use in the game project is reasonable, but public asset-pack style redistribution should stay blocked without explicit confirmation

### Raven Fantasy Icons

- source pack: `Assets/Sprite/Free - Raven Fantasy Icons`
- evidence found: `Special Note to the Dev.txt`
- license file found: none in the audited folder
- LicenseStatus: `UNKNOWN`

Working rule for this repo:

- do not claim commercial-safe or redistribution-safe use from the current evidence
- keep this pack held out from curated UI assignment until the actual license is confirmed

## Current Import Audit

### Raw source files

Sampled raw TravelBook UI files still show:

- `textureType: 0`
- `spriteMode: 0`
- `spritePixelsToUnits: 100`
- `alphaIsTransparency: 0`
- `filterMode: 1`
- `textureCompression: 1`

Meaning:

- the source files are still treated like generic textures rather than guaranteed UI sprites
- raw source files remain untouched on purpose
- raw packs are not the place for UI preview import cleanup

### Curated copies after Batch 77.6

Curated `.meta` files under `Assets/_Game/Content/UI/Sprites` now show:

- `textureType: 8`
- `spriteMode: 1`
- `spriteMeshType: 1`
- `spritePixelsToUnits: 100`
- `alphaIsTransparency: 1`
- `filterMode: 1`
- `textureCompression: 0`
- `enableMipMap: 0`

Meaning:

- curated copies are now prepared as `Sprite (2D and UI)` test assets
- curated copies are assignable to `Sprite` fields as well as `Texture` fields
- generated `.meta` files exist for the curated copies, and only those copies were normalized

## Curated Import Decisions In Batch 77.6

Applied on curated copies only:

- Texture Type: `Sprite (2D and UI)`
- Sprite Mode: `Single`
- Alpha Is Transparency: `true`
- Mesh Type: `Full Rect`
- Filter Mode: `Bilinear` for this painted UI pack
- Compression: `None`
- Mip Maps: `Off`

Intentionally not changed:

- raw source import settings under `Assets/Sprite`
- `spritePixelsToUnits: 100`
- folder layout or candidate membership
- any runtime loading path

## Manual Assignment Workflow

1. Open `Assets/_Game/Scenes/Preview/BattleUiPreviewScene.unity` or `Assets/_Game/Scenes/Preview/InventoryUiPreviewScene.unity`.
2. Select the relevant skin asset:
   - `Assets/_Game/Content/UI/Skins/Battle/BattleUiSkin_Default.asset`
   - `Assets/_Game/Content/UI/Skins/Inventory/InventoryUiSkin_Default.asset`
3. Use the curated folders under `Assets/_Game/Content/UI/Sprites` as the first search surface.
4. Read the slot maps:
   - `docs/ui/battle-skin-slot-map.md`
   - `docs/ui/inventory-skin-slot-map.md`
5. Follow `docs/ui/manual-skin-assignment-checklist.md`.
6. If the team wants a written slot plan first, fill `Assets/_Game/Content/UI/Sprites/_SourceMap/skin-assignment-template.md`.
7. Drag the chosen curated sprite into the relevant `Sprite` field manually.
8. Press Play in the preview scene and tune layout after assignment.

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
- `docs/ui/manual-skin-assignment-checklist.md`
- `docs/ui/ui-asset-attribution.md`
- `Assets/_Game/Content/UI/Sprites/_SourceMap/ui-sprite-candidate-map.md`
