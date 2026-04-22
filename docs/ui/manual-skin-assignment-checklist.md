# Manual Skin Assignment Checklist

Batch 77.6 prepared the preview workflow without choosing final art.
Batch 77.9 records the first approved preview mapping gate.

Use this checklist when assigning curated sprites in Unity Editor.

## Batch 77.9 Approved Preview Mapping

Only these slots are approved for the current preview skin assets.
Do not fill unlisted slots unless the user provides a new mapping.

Battle:

- `PanelBackground`: `Assets/_Game/Content/UI/Sprites/Battle/Panels/UI_TravelBook_BookCover01a.png`
- `CommandButtonNormal`: `Assets/_Game/Content/UI/Sprites/Battle/Buttons/UI_TravelBook_Button01a_1.png`
- `PopupBackground`: `Assets/_Game/Content/UI/Sprites/Battle/Popups/UI_TravelBook_Popup01a.png`
- `TopStripBackground`: intentionally empty; fallback rendering is expected

Inventory:

- `EquipmentSlotEmpty`: `Assets/_Game/Content/UI/Sprites/Inventory/Slots/UI_TravelBook_Slot01a.png`
- `EquipmentSlotEquipped`: `Assets/_Game/Content/UI/Sprites/Inventory/Slots/UI_TravelBook_Slot01b.png`
- `RunSpoilsBadge`: `Assets/_Game/Content/UI/Sprites/Battle/Popups/UI_TravelBook_Popup01a.png`

## Battle Skin Assignment

1. Open `Assets/_Game/Scenes/Preview/BattleUiPreviewScene.unity`.
2. Select `Assets/_Game/Content/UI/Skins/Battle/BattleUiSkin_Default.asset`.
3. Verify `PanelBackground` is `UI_TravelBook_BookCover01a`.
4. Verify `CommandButtonNormal` is `UI_TravelBook_Button01a_1`.
5. Verify `PopupBackground` is `UI_TravelBook_Popup01a`.
6. Leave `TopStripBackground` empty unless you have a dedicated strip-style sprite. Do not reuse book-cover or panel art for the full-width top strip.
7. If `PopupBackground` is a light parchment-style sprite, verify the popup text uses the dark popup text colors from the skin asset.
8. Use Sprite references as the final skin inputs. The Texture fields remain preview-only fallback and should not be the final authoring path.
9. Press Play or inspect the scene in Edit Mode.
10. Verify unassigned slots still show fallback colors and do not throw null errors.

## Inventory Skin Assignment

1. Open `Assets/_Game/Scenes/Preview/InventoryUiPreviewScene.unity`.
2. Select `Assets/_Game/Content/UI/Skins/Inventory/InventoryUiSkin_Default.asset`.
3. Verify `EquipmentSlotEmpty` is `UI_TravelBook_Slot01a`.
4. Verify `EquipmentSlotEquipped` is `UI_TravelBook_Slot01b`.
5. Verify `RunSpoilsBadge` is `UI_TravelBook_Popup01a`.
6. Leave `EquipmentSlot`, `EquipmentSlotSelected`, `PendingExtractionBadge`, row, panel, and background slots empty unless the user provides a new mapping.
7. Verify all 7 equipment slots remain readable in the preview.
8. Verify the badge reads as an intentional parchment badge and is not oversized.

## Layout Tuning

1. Open `Assets/_Game/Content/UI/LayoutProfiles/Battle/BattleUiLayout_Default.asset`.
2. Adjust panel padding, card gaps, button height, and related battle spacing.
3. Open `Assets/_Game/Content/UI/LayoutProfiles/Inventory/InventoryUiLayout_Default.asset`.
4. Adjust slot size, slot gap, row height, and column widths.
5. Do not edit code for spacing unless a needed control is missing from the layout profile.

## Screenshot And Report

1. Capture a Battle preview screenshot.
2. Capture an Inventory preview screenshot.
3. Record visible clipping, overlap, stretching, or text crowding.
4. Record FPS only if Play Mode was used.

## Guardrails

- Do not assign directly from `Assets/Sprite`.
- Do not fill empty slots with guessed substitutes just to make the scene look finished.
- If a mapping choice is still undecided, leave the slot empty and note it.
- Do not use Raven Fantasy Icons until the license is confirmed.
- If you want a written mapping before dragging assets, fill `Assets/_Game/Content/UI/Sprites/_SourceMap/skin-assignment-template.md` first.
