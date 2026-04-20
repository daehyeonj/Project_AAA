# UI Preview Scene Setup

## Why This Exists

The repository now contains an editor generator:

- `Tools -> Project AAA -> UI -> Create Or Update Preview Scene Scaffold`

That generator creates or updates:

- `Assets/_Game/Scenes/Preview/BattleUiPreviewScene.unity`
- `Assets/_Game/Scenes/Preview/InventoryUiPreviewScene.unity`
- default battle/inventory skin assets
- default battle/inventory layout profile assets
- default battle/inventory preview data assets

If Codex cannot run Unity batchmode because the project is already open in the editor, use the steps below inside the current Unity session.

## Exact Steps In Unity Editor

1. Wait for Unity script compilation to finish.
2. Open the top menu: `Tools`.
3. Open `Project AAA`.
4. Open `UI`.
5. Click `Create Or Update Preview Scene Scaffold`.
6. Let Unity create/update the preview scenes and default assets.
7. Open `Assets/_Game/Scenes/Preview/BattleUiPreviewScene.unity`.
8. Select `BattleUiPreviewRoot`.
9. Inspect `BattleUiPreviewSceneController`.
10. Confirm these references are assigned:
    - `BattleUiSkin_Default`
    - `BattleUiLayout_Default`
    - `BattleUiPreview_Default`
11. Open `Assets/_Game/Scenes/Preview/InventoryUiPreviewScene.unity`.
12. Select `InventoryUiPreviewRoot`.
13. Inspect `InventoryUiPreviewSceneController`.
14. Confirm these references are assigned:
    - `InventoryUiSkin_Default`
    - `InventoryUiLayout_Default`
    - `InventoryUiPreview_Default`
15. Open the default skin assets and manually drag curated Sprite assets into slots.
16. Press Play to inspect the preview layouts.

## If The Menu Is Missing

If `Tools -> Project AAA -> UI -> Create Or Update Preview Scene Scaffold` does not appear:

1. Check Console for compile errors.
2. Fix or report those errors first.
3. Reimport or refresh the `Assets/_Game/Scripts/Editor/UiPreviewSceneScaffoldBuilder.cs` script if needed.
4. Wait for compilation to finish again.

## Manual Asset Creation Fallback

If you need to create assets manually instead of using the generator:

### Battle

- Project window -> `Create -> Project AAA -> UI -> Battle UI Skin`
- Project window -> `Create -> Project AAA -> UI -> Battle UI Layout Profile`
- Project window -> `Create -> Project AAA -> UI -> Battle UI Preview Data`

### Inventory

- Project window -> `Create -> Project AAA -> UI -> Inventory UI Skin`
- Project window -> `Create -> Project AAA -> UI -> Inventory UI Layout Profile`
- Project window -> `Create -> Project AAA -> UI -> Inventory UI Preview Data`

Then assign those assets to the scene controller components manually.

## Manual Scene Fallback

If a preview scene still does not exist:

### Battle

1. Create a new empty scene.
2. Save it as `Assets/_Game/Scenes/Preview/BattleUiPreviewScene.unity`.
3. Add `Main Camera`.
4. Create an empty GameObject named `BattleUiPreviewRoot`.
5. Add `BattleUiPreviewSceneController` to that root.
6. Assign the battle skin/layout/preview-data assets.

### Inventory

1. Create a new empty scene.
2. Save it as `Assets/_Game/Scenes/Preview/InventoryUiPreviewScene.unity`.
3. Add `Main Camera`.
4. Create an empty GameObject named `InventoryUiPreviewRoot`.
5. Add `InventoryUiPreviewSceneController` to that root.
6. Assign the inventory skin/layout/preview-data assets.

## Important Reminder

- Codex created slots and preview scaffolds.
- Human designer assigns art.
- Curated copies under `Assets/_Game/Content/UI/Sprites` were normalized for Sprite assignment in Batch 77.6.
- Codex did not auto-map by filename.
- Codex did not touch `Assets/Sprite` import settings in this batch.
