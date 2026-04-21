# UI Sprite Candidate Map

## Purpose

This map records the current Batch 77.5 starter curation.

- It is a staging map, not a final art decision.
- Every slot still requires human assignment in Unity Editor.
- Raw source art remains untouched under `Assets/Sprite`.

## Raw Source Audit Summary

### Source packs seen

- `Assets/Sprite/Complete_UI_Book_Styles_Pack_Free_v1.0`
- `Assets/Sprite/Free - Raven Fantasy Icons`
- `Assets/Sprite/Tiny RPG Character Asset Pack v1.03 -Free Soldier&Orc`

### Source packs chosen for curation

- `Complete_UI_Book_Styles_Pack_Free_v1.0/01_TravelBookLite`
  - obvious UI-oriented file names
  - panel/frame/button/bar/slot/icon assets are discoverable by name
  - license/readme files are present in the raw source pack

### Audited but not curated

- `Free - Raven Fantasy Icons`
  - large icon count
  - numeric filenames like `fb1000.png` make safe narrowing poor without a human art pass
  - left in raw source only for now

- `Tiny RPG Character Asset Pack v1.03 -Free Soldier&Orc`
  - character/world art, not a primary UI skin source for this batch

## License / Attribution Snapshot

### TravelBook Lite

- source pack: `Assets/Sprite/Complete_UI_Book_Styles_Pack_Free_v1.0`
- license evidence found: `License.txt`
- visible status: personal/commercial project use is allowed
- visible credit requirement: give appropriate credit or provide the product page link, and indicate changes
- visible restriction: do not resell or publish the same material, or adapted versions of it, as a standalone asset pack
- repo working note: curated copies should keep attribution notes in docs and should not be treated as freely redistributable source art

### Raven Fantasy Icons

- source pack: `Assets/Sprite/Free - Raven Fantasy Icons`
- license evidence found: none in the audited folder
- note found: `Special Note to the Dev.txt`
- LicenseStatus: `UNKNOWN`
- repo working note: keep this pack out of curated assignment until the actual license is confirmed

## Import Audit

### Raw TravelBook candidates

Sampled files:

- `UI_TravelBook_BookCover01a.png`
- `UI_TravelBook_BookPageLeft01a.png`
- `UI_TravelBook_BookPageRight01a.png`
- `UI_TravelBook_Popup01a.png`
- `UI_TravelBook_FrameSelect01a.png`
- `UI_TravelBook_Slot01a.png`
- `UI_TravelBook_Bar01a.png`
- `UI_TravelBook_Fill01a.png`

Observed sampled values:

- `textureType: 0`
- `spriteMode: 0`
- `spriteMeshType: 1`
- `spritePixelsToUnits: 100`
- `alphaIsTransparency: 0`
- `filterMode: 1`
- `textureCompression: 1`

Interpretation:

- these are still imported like generic textures rather than guaranteed UI sprites
- no import setting changes were made in Batch 77.5
- raw source remains untouched

### Curated copies after Batch 77.6

Observed normalized values:

- `textureType: 8`
- `spriteMode: 1`
- `spriteMeshType: 1`
- `spritePixelsToUnits: 100`
- `alphaIsTransparency: 1`
- `filterMode: 1`
- `textureCompression: 0`
- `enableMipMap: 0`

Interpretation:

- curated copies now import as UI-test sprites
- curated copies are assignable to `Sprite` fields
- generated `.meta` files exist only on the project-owned curated copies

## Curated Copy Map

### Battle

- `Battle/Panels/UI_TravelBook_BookCover01a.png`
- `Battle/Panels/UI_TravelBook_Popup01a.png`
- `Battle/Panels/UI_TravelBook_Frame01a.png`
- `Battle/Panels/UI_TravelBook_FrameSelect01a.png`
- `Battle/Buttons/UI_TravelBook_ButtonValue01a.png`
- `Battle/Buttons/UI_TravelBook_ButtonValue01b.png`
- `Battle/Buttons/UI_TravelBook_Select01a.png`
- `Battle/Timeline/UI_TravelBook_Marker01a.png`
- `Battle/Timeline/UI_TravelBook_Point01a.png`
- `Battle/Timeline/UI_TravelBook_Point02a.png`
- `Battle/Bars/UI_TravelBook_Bar01a.png`
- `Battle/Bars/UI_TravelBook_Fill01a.png`
- `Battle/Bars/UI_TravelBook_Fill01b.png`
- `Battle/Popups/UI_TravelBook_Alert01a.png`
- `Battle/Popups/UI_TravelBook_Popup01a.png`
- `Battle/Badges/UI_TravelBook_IconStar01a.png`
- `Battle/Badges/UI_TravelBook_IconCoin01a.png`
- `Battle/Icons/UI_TravelBook_IconArrow01a.png`
- `Battle/Icons/UI_TravelBook_IconHeart01a.png`

### Inventory

- `Inventory/Panels/UI_TravelBook_BookPageLeft01a.png`
- `Inventory/Panels/UI_TravelBook_BookPageRight01a.png`
- `Inventory/Panels/UI_TravelBook_Frame01a.png`
- `Inventory/Slots/UI_TravelBook_Slot01a.png`
- `Inventory/Slots/UI_TravelBook_Slot01b.png`
- `Inventory/Slots/UI_TravelBook_Slot01c.png`
- `Inventory/Rows/UI_TravelBook_ButtonValue01a.png`
- `Inventory/Rows/UI_TravelBook_ButtonValue01b.png`
- `Inventory/Rows/UI_TravelBook_FrameSelect01a.png`
- `Inventory/Badges/UI_TravelBook_IconCoin01a.png`
- `Inventory/Badges/UI_TravelBook_IconStar01a.png`
- `Inventory/Icons/UI_TravelBook_IconGear01a.png`
- `Inventory/Icons/UI_TravelBook_IconHeart01a.png`

### Shared

- `Shared/Frames/UI_TravelBook_FrameSelect01b.png`
- `Shared/Dividers/UI_TravelBook_Line01a.png`
- `Shared/Backgrounds/UI_TravelBook_BookCover01a.png`
- `Shared/Icons/UI_TravelBook_IconGear01a.png`
