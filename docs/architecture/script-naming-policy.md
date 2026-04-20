# Script Naming Policy

## Goal

Use names that reveal long-term ownership and runtime role without forcing a dangerous mass rename of existing legacy files.

## Rules

### New production-facing scripts

Do **not** use the `Prototype` prefix for newly introduced runtime/service/scene-architecture scripts.

Examples:

- `AppRoot`
- `GameSessionRoot`
- `RuntimeGameState`
- `SceneFlowService`
- `BattleUiSkinDefinition`
- `InventoryUiSkinDefinition`

### Existing legacy files

Do **not** mass rename existing `Prototype*` or `StaticPlaceholder*` files in this batch.

Examples of files that stay as-is for now:

- `PrototypePresentationShell`
- `PrototypeDebugHUD`
- `StaticPlaceholderWorldView`
- `BootEntry`

Those names reflect current legacy reality. A rename only becomes worthwhile when ownership has already been extracted and the old file has become thin enough to rename safely.

### Naming by role

Use names that describe the script’s responsibility:

| Script role | Preferred naming |
| --- | --- |
| Persistent root | `AppRoot`, `GameSessionRoot` |
| Persistent service | `*Service`, `*Coordinator` |
| Persistent state holder | `*State`, `*Context`, `*Snapshot` |
| Scene presenter | `*Presenter`, `*Controller`, `*PanelPresenter` |
| Static authored data | `*Definition`, `*Catalog`, `*Registry` |
| View-model builder / adapter | `*Builder`, `*Adapter` |
| Debug-only surface | `Debug*`, or keep legacy debug owner until isolated |

### Avoid on new files

- `Prototype*` for production-facing new code
- `StaticPlaceholder*` for new owners
- names that imply canonical truth when the type is only a presenter
- names that imply presentation when the type owns runtime truth

## Practical Interpretation For This Project

- canonical contracts may keep their current historical names until a dedicated refactor batch moves them
- new scaffolds introduced to support future scene migration should use long-term names immediately
- presentation names should make it obvious they are disposable scene-local surfaces

## Rename Timing Rule

Rename only when at least one of these is true:

1. ownership has already been extracted and the rename will reduce confusion
2. the file is now thin enough that the rename will not create wide merge risk
3. a dedicated rename/refactor batch has been planned

Until then, prefer additive wrappers and scaffolds over churn-heavy rename waves.
