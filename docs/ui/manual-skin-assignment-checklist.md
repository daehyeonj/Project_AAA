# Manual Skin Assignment Checklist

Batch 77.6 prepares the preview workflow, but it does not choose final art for you.

Use this checklist when assigning curated sprites in Unity Editor.

## Battle Skin Assignment

1. Open `Assets/_Game/Scenes/Preview/BattleUiPreviewScene.unity`.
2. Select `Assets/_Game/Content/UI/Skins/Battle/BattleUiSkin_Default.asset`.
3. Assign `PanelBackground` from `Assets/_Game/Content/UI/Sprites/Battle/Panels`.
4. Leave `TopStripBackground` empty unless you have a dedicated strip-style sprite. Do not reuse book-cover or panel art for the full-width top strip.
5. Assign `CommandButtonNormal` from `Assets/_Game/Content/UI/Sprites/Battle/Buttons`.
6. Assign `PopupBackground` from `Assets/_Game/Content/UI/Sprites/Battle/Popups`.
7. If `PopupBackground` is a light parchment-style sprite, verify the popup text uses the dark popup text colors from the skin asset.
8. Use Sprite references as the final skin inputs. The Texture fields remain preview-only fallback and should not be the final authoring path.
6. Press Play or inspect the scene in Edit Mode.
7. Verify unassigned slots still show fallback colors and do not throw null errors.

## Inventory Skin Assignment

1. Open `Assets/_Game/Scenes/Preview/InventoryUiPreviewScene.unity`.
2. Select `Assets/_Game/Content/UI/Skins/Inventory/InventoryUiSkin_Default.asset`.
3. `UI_TravelBook_Slot01a` and `UI_TravelBook_Slot01b` are acceptable preview candidates for equipment slot states.
4. Assign `EquipmentSlotEmpty` from `Assets/_Game/Content/UI/Sprites/Inventory/Slots`.
5. Assign `EquipmentSlotEquipped` from `Assets/_Game/Content/UI/Sprites/Inventory/Slots`.
5. Assign `ItemRowSelected` from `Assets/_Game/Content/UI/Sprites/Inventory/Rows`.
6. Assign `RunSpoilsBadge` and `PendingExtractionBadge` from `Assets/_Game/Content/UI/Sprites/Inventory/Badges`.
7. Verify all 7 equipment slots remain readable in the preview.

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
