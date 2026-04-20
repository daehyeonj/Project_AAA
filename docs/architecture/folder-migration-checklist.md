# Folder Migration Checklist

Use this checklist before any future folder cleanup batch.

## Safe Move Rules

1. Audit the current tree before moving anything.
2. Move files only when folder ownership becomes clearer without changing gameplay behavior.
3. Prefer moving small presenter/scaffold groups over hot-spot mega-files.
4. Do not mass-rename classes/files in the same batch as a folder move.
5. If a move has unresolved path-sensitive dependencies, leave it in place and document the exception.

## `.meta` Preservation Rule

1. Unity references assets by GUID in `.meta` files.
2. Move each asset together with its `.meta` file.
3. Prefer `git mv` or Unity/AssetDatabase-safe moves.
4. Do not delete and recreate existing assets just to change folders.
5. After moving, confirm Git reports renames/moves instead of silent asset recreation.

## Resources Warning

1. Anything under a `Resources` folder is path-sensitive.
2. Search for `Resources.Load(...)` and `Resources.LoadAll(...)` before moving content.
3. Do not move `Resources` content unless every load path is updated and revalidated.
4. If a content move would force speculative runtime path edits, defer it.

## Editor Folder Warning

1. Any file using `UnityEditor` must stay under an `Editor` folder.
2. Do not move editor-only tools into runtime folders.
3. If asmdefs already exist, confirm the move does not break the editor/runtime split.

## Scene Move Warning

1. Scene GUIDs remain stable when the `.unity` file and `.meta` move together, but tooling can still depend on the old path.
2. Search Build Settings, smoke runners, editor helpers, docs, and project defaults before moving scenes.
3. Treat `SampleScene` and any scene named in build settings or smoke tooling as path-sensitive until proven otherwise.
4. Prefer moving non-baseline scenes first.

## Documentation Warning

1. Search docs for old file/scene paths after each move.
2. Update architecture docs, UI docs, runtime notes, and batch-status notes in the same change.
3. Record intentional exceptions so the next batch does not rediscover the same blocker.

## Validation After Moving

1. Run Unity batch compile.
2. Search for path-sensitive references again:
   - `Resources.Load`
   - `AssetDatabase.LoadAssetAtPath`
   - `SceneManager.LoadScene`
   - hard-coded scene paths
   - hard-coded asset paths
3. Confirm moved scenes/assets still exist at their documented paths.
4. Confirm the current playable path was either preserved or explicitly updated everywhere it is referenced.
5. Confirm no gameplay/system logic changed as a side effect of the move.

## Batch 77.4 Baseline Exceptions

- `Assets/Scenes/SampleScene.unity` stayed in place because Build Settings, project defaults, and smoke tooling still reference it directly.
- `Assets/_Game/Resources/Content/*` stayed in place because runtime/editor `Resources.Load(...)` paths are active.
- `Assets/Sprite/*` stayed in place because sprite import ownership and runtime content routing are not stabilized yet.
- `BootEntry.cs` and `StaticPlaceholderWorldView*.cs` stayed in their current owner folders because they are still hot spots and not safe broad-move targets.
