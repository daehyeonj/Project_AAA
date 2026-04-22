# UI Skin Preview Mapping QA

Updated: `2026-04-22`

## Purpose

This is the GPT-shareable handoff for the current Battle/Inventory skin mapping and runtime bridge.
The art mapping remains preview/test state, but `SampleScene` now consumes the same skin assets through cached runtime providers.

## Approved Mapping

Battle default preview skin:

- `PanelBackground` -> `Assets/_Game/Content/UI/Sprites/Battle/Panels/UI_TravelBook_BookCover01a.png`
- `CommandButtonNormal` -> `Assets/_Game/Content/UI/Sprites/Battle/Buttons/UI_TravelBook_Button01a_1.png`
- `PopupBackground` -> `Assets/_Game/Content/UI/Sprites/Battle/Popups/UI_TravelBook_Popup01a.png`
- `TopStripBackground` -> empty

Inventory default preview skin:

- `EquipmentSlotEmpty` -> `Assets/_Game/Content/UI/Sprites/Inventory/Slots/UI_TravelBook_Slot01a.png`
- `EquipmentSlotEquipped` -> `Assets/_Game/Content/UI/Sprites/Inventory/Slots/UI_TravelBook_Slot01b.png`
- `RunSpoilsBadge` -> `Assets/_Game/Content/UI/Sprites/Battle/Popups/UI_TravelBook_Popup01a.png`

Unlisted Battle and Inventory slots are intentionally empty.

## Expected Behavior

- Battle top strip uses fallback rendering because `TopStripBackground` is empty.
- `PanelBackground` must not be stretched across the full-width battle top strip.
- `PopupBackground` is a light parchment-style sprite, so popup title/body/hint text should use the dark popup text colors from `BattleUiSkinDefinition`.
- Inventory slot states should stay readable with `Slot01a` for empty and `Slot01b` for equipped.
- `RunSpoilsBadge` uses `Popup01a` as a preview badge test and may still need layout tuning after screenshot QA.

## Policy

- Final skin authoring uses `Sprite` references.
- `Texture` references remain preview-only fallback and should not become the final path.
- Raw source packs under `Assets/Sprite` remain untouched.
- Raven Fantasy Icons remain blocked because their license status is `UNKNOWN`.
- TravelBook Lite usage requires attribution notes and later release/legal review before shipping.

## Static Validation

- `BattleUiSkin_Default.asset` serializes only the approved Battle mapping above.
- `InventoryUiSkin_Default.asset` serializes only the approved Inventory mapping above.
- `UI_TravelBook_Button01a_1.png` was curated into `Assets/_Game/Content/UI/Sprites/Battle/Buttons` and imports as `Sprite (2D and UI)`.
- Preview scene scaffold references the default preview skin assets and does not depend on `BootEntry`, `StaticPlaceholderWorldView`, or `SampleScene`.
- Battle top-strip renderer queries `TopStripBackground` through `GetTopStripSlot()` and falls back when it is empty.
- Popup rendering uses `PopupTitleTextColor`, `PopupBodyTextColor`, and `PopupHintTextColor`.

## Runtime Bridge

- `Assets/Scenes/SampleScene.unity` has a `RuntimeUiSkinBridge` component on the `BootEntry` GameObject.
- The bridge serializes `BattleUiSkin_Default.asset` and `InventoryUiSkin_Default.asset`.
- On scene load, the bridge registers both assets with `BattleUiSkinProvider` and `InventoryUiSkinProvider`.
- Runtime-added `PrototypeDebugHUD` and `PrototypePresentationShell` resolve those cached provider skins when their local serialized fields are empty.
- No runtime `AssetDatabase`, folder scan, per-frame asset lookup, or broad `Resources.Load` path was added.
- Inventory runtime equipment slots now pass `slot.HasEquippedItem` into `GetEquipmentSlot(selected, hasEquippedItem)`, matching the preview empty/equipped slot behavior.

## Runtime Modal Cleanup

Batch 77.10.1 keeps the runtime bridge intact and fixes the battle-inventory modal layering bug:

- `PrototypeDebugHUD.DrawBattleHudShell` uses the existing inventory-open modal guard for `Target Status`.
- While inventory is open, `Current Unit`, `Command Selection`, `Target Status`, command flyouts, and target-selection overlay are skipped.
- The battle top strip remains visible because it is outside the modal-conflict area and still uses `TopStripBackground` fallback.
- When inventory closes, `Target Status` returns through the normal battle HUD draw path.
- No combat, RPG, world, inventory truth, skin mapping, or sprite assignment changed.

## Compile Proof

- Unity log: `unity-merge-validate.log`
- Result: `PASS`
- Markers found:
  - `*** Tundra build success`
  - `Batchmode quit successfully invoked - shutting down!`
  - `Exiting batchmode successfully now!`

## Remaining Gate

Manual screenshot QA is still needed before claiming final visual approval.
Recommended next step is one of:

- user runtime screenshot QA in `SampleScene`
- `Batch 77.11`: runtime layout tuning if screenshots show smear, clipping, oversized badge, or readability issues
- `Batch 78`: combat core work only if UI preview work is intentionally paused
