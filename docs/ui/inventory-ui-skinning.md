# Inventory UI Skinning

## Intent

Inventory skinning is split from battle skinning on purpose.

This scaffold keeps the current inventory/equipment overlay behavior intact while exposing a small, safe set of art slots for the accepted overlay rail.

## Active Owner Audit

Current inventory overlay owner:

- `Assets/_Game/Scripts/UI/Inventory/PrototypePresentationShell.Inventory.cs`

This file draws the current inventory/equipment overlay that can appear during world/prep/battle-adjacent contexts.

## New Files

- `Assets/_Game/Scripts/UI/Inventory/InventoryUiSkinDefinition.cs`
- `Assets/_Game/Scripts/UI/Inventory/InventoryUiSkinProvider.cs`
- `Assets/_Game/Scripts/UI/Inventory/InventoryUiSkinRenderer.cs`

## Slot Map

Current inventory slots:

- `MemberRow`
- `MemberRowSelected`
- `ItemRow`
- `ItemRowSelected`
- `EquipmentSlot`
- `EquipmentSlotSelected`
- `RunSpoilsBadge`

## Wired In This Batch

Only the following inventory surfaces are wired in this scaffold:

- party member rows
- equipment slot cards
- inventory item rows
- run spoils badge

Everything else still uses the existing fallback rendering.

## How To Assign A Skin

1. Create a new asset from:
   - `Create > Project AAA > UI > Inventory UI Skin`
2. Assign the asset to:
   - `PrototypePresentationShell`
3. Fill only the slots you actually want to override.

If a slot is empty, the current fallback inventory UI remains visible.

## Sprite Vs Texture Assignment

Each slot accepts either:

- a `Sprite`
- or a `Texture2D`

This is intentional because current user art under `Assets/Sprite` is not guaranteed to be fully reimported as `Sprite (2D and UI)` yet.

## Run Spoils Badge

The run spoils badge only appears when the current inventory surface text already reports pending extraction / run spoils.

This scaffold does not invent new reward behavior.

It only skins the already-existing badge/chip moment.

## Performance Rule

- no `Resources.Load(...)` in the draw path
- no folder scans
- no runtime asset guessing
- assign references once in the inspector and let the provider cache them

## What This Batch Intentionally Did Not Do

- no inventory behavior rewrite
- no equipment logic rewrite
- no pending spoils truth rewrite
- no auto-selected art from `Assets/Sprite`
- no modal/input behavior rollback
