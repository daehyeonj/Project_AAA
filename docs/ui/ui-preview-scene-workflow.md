# UI Preview Scene Workflow

## Purpose

This scaffold adds **editor-friendly preview scenes** for Battle UI and Inventory UI without changing the current `SampleScene` gameplay flow.

Codex created:

- scene-local preview presenters
- empty/default skin assets
- editable layout profile assets
- mock preview data assets
- preview scenes for asset placement and layout tuning

Codex did **not**:

- choose art assets for you
- auto-map sprites by filename
- move or rename anything under `Assets/Sprite`
- change the runtime battle/inventory truth
- switch the real game flow to these preview scenes

## Scenes To Open

- `Assets/_Game/Scenes/Preview/BattleUiPreviewScene.unity`
- `Assets/_Game/Scenes/Preview/InventoryUiPreviewScene.unity`

These are **design workflow scenes**, not runtime flow scenes.

If those scenes are not present yet in your local editor session, run:

- `Tools -> Project AAA -> UI -> Create Or Update Preview Scene Scaffold`

See `docs/ui/ui-preview-scene-setup.md` for the exact fallback steps.

## Assets To Edit

### Battle

- skin: `Assets/_Game/Content/UI/Skins/Battle/BattleUiSkin_Default.asset`
- layout: `Assets/_Game/Content/UI/LayoutProfiles/Battle/BattleUiLayout_Default.asset`
- preview data: `Assets/_Game/Content/UI/Preview/Battle/BattleUiPreview_Default.asset`

### Inventory

- skin: `Assets/_Game/Content/UI/Skins/Inventory/InventoryUiSkin_Default.asset`
- layout: `Assets/_Game/Content/UI/LayoutProfiles/Inventory/InventoryUiLayout_Default.asset`
- preview data: `Assets/_Game/Content/UI/Preview/Inventory/InventoryUiPreview_Default.asset`

## Recommended Workflow

1. Open `BattleUiPreviewScene`.
2. Select `BattleUiSkin_Default.asset`.
3. Manually drag curated sprites from `Assets/_Game/Content/UI/Sprites` into the named slots.
4. If a needed candidate has not been staged yet, inspect the raw source pack under `Assets/Sprite` and curate it first instead of assigning from the raw pack blindly.
5. Follow `docs/ui/manual-skin-assignment-checklist.md` for the minimum required slot checks.
6. Tune sizes and spacing in `BattleUiLayout_Default.asset`.
7. If needed, edit preview copy/text in `BattleUiPreview_Default.asset`.
8. Press Play to verify the layout.
9. Repeat the same flow in `InventoryUiPreviewScene`.

## Slot Meaning

### Battle skin slots

- `PanelBackground`: general battle panel shells
- `PanelHeader`: top strip / summary / header surfaces
- `PanelAccent`: optional accent strip
- `CommandButtonNormal/Hover/Selected/Disabled`: command button states
- `CurrentUnitCard`: current actor panel
- `TargetStatusCard`: target panel
- `TimelineChip`, `TimelineChipCurrent`, `TimelineChipEnemy`: top order strip tokens
- `HpBarBackground`, `HpBarFill`: HP meter
- `PopupBackground`: battle result popover body
- `DropPopupBadge`: optional reward/drop badge
- `BurstWindowBadge`: reserved for future burst/badge work

### Inventory skin slots

- `InventoryBackground`: preview overlay / root shell
- `HeaderPanel`: inventory header
- `FooterPanel`: inventory footer
- `MemberRow` / `MemberRowSelected`: party list rows
- `EquipmentSlotEmpty` / `EquipmentSlotEquipped` / `EquipmentSlotSelected`: equipment slot states
- `ItemRow` / `ItemRowSelected`: inventory rows
- `ItemDetailPanel`: selected item detail surface
- `RunSpoilsBadge`: carried spoils readback
- `PendingExtractionBadge`: pending extraction state

## Layout Tuning

Battle layout profile controls:

- top strip height
- command panel width
- current unit width
- target status width
- padding / gaps
- button height
- timeline chip width / height
- popover width / height

Inventory layout profile controls:

- overlay width / height
- member / equipment / inventory column widths
- slot size / slot gap
- row height
- footer height

## Fallback Behavior

If every slot is empty:

- preview scenes still render using fallback colors and boxes
- text remains visible
- layout remains testable
- no null-reference exception should occur

This is intentional so the scene is useful before art assignment.

## Why Codex Did Not Auto-Assign

The curated sprite copies now import as `Sprite (2D and UI)` after Batch 77.6, but no explicit slot-to-file mapping has been provided by the user.

Because of that:

- Codex did not guess slot mappings
- Codex did not change raw source import settings
- Codex did not auto-pick "best looking" art

Human assignment is the correct workflow here.

## How This Differs From Runtime SampleScene UI

- preview scenes are isolated from `BootEntry`
- preview scenes do not call `StaticPlaceholderWorldView`
- preview scenes do not mutate `ManualTradeRuntimeState`
- preview scenes do not run dungeon/battle/inventory gameplay truth
- preview scenes only render mock data with the same skin/layout concept the future scene-local presenters can use

## Future Migration Boundary

This scaffold is **not** the final migration.

What it gives us now:

- a safe editor workspace for battle/inventory skin placement
- layout tuning without touching the full runtime cluster
- presenter-side model/adaptation seams for future scene-local UI work

What remains later:

- runtime adapter wiring from canonical battle/inventory surfaces into scene-local presenters
- actual battle scene / inventory overlay migration
- final art pass and polish
