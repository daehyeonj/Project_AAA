# Batch 57 Current Lane Semantics Audit

Date: `2026-04-17`

## Selected Branch

- `A` - audit disproves dead-end suspicion

## Intended Semantics Of `Current Lane`

- `commit`

`Current Lane` is currently coded to reflect the last committed lane selection, not hover-only preview.

## Why This Branch Was Allowed

- This batch asked for a narrow audit-or-fix pass on the accepted GitHub `main` snapshot.
- The current code can be audited without changing runtime UI.
- Static audit was sufficient to answer the semantics question before making any behavior change.

## Q1-Q4 Answers

### Q1. What is `Current Lane` supposed to reflect?

- `commit`, not hover

Reason:

- Hover writes `_hoverPanelLaneOptionId` and refreshes prompt/presentation.
- Commit writes `_selectedPanelLaneOptionId` and `_currentLaneSelectionText`.

### Q2. Does the current producer path write `CurrentLaneSelectionText` on commit?

- `Yes`

Evidence:

- `Assets/_Game/Scripts/Dungeon/StaticPlaceholderWorldView.DungeonRun.PanelShell.cs`
  - `TryTriggerDungeonPanelLaneOption(...)` enters `TryTriggerCurrentPanelLaneOption(...)`
  - successful commit executes:
    - `_selectedPanelLaneOptionId = option.OptionId`
    - `_hoverPanelLaneOptionId = string.Empty`
    - `_currentLaneSelectionText = laneLabel + " | " + option.OptionLabel`

### Q3. Does the shell surface forward that field into the accepted explore shell footer?

- `Yes`

Evidence:

- `Assets/_Game/Scripts/Dungeon/StaticPlaceholderWorldView.DungeonRun.ShellContracts.cs`
  - `context.CurrentLaneSelectionText = CurrentLaneSelectionText`
- `Assets/_Game/Scripts/UI/Battle/PrototypePresentationShell.DungeonRun.cs`
  - explore footer draws:
    - `"Current Lane: " + SafeShellText(panelContext.CurrentLaneSelectionText)`

### Q4. Is there fresh manual repro that clicking/committing a lane does NOT update `Current Lane`?

- `No fresh repro was supplied in this batch`

Because no fresh manual repro was provided, the audit does not justify changing the current behavior from commit-based to hover-preview-based.

## Targeted Static Audit Summary

The accepted explore shell path is currently:

1. presentation click
2. bootstrap bridge
3. world-view panel-shell commit
4. shell context rebuild
5. footer render

Concrete path:

- `PrototypePresentationShell.DungeonRun.cs`
  - click -> `_bootEntry.TryTriggerDungeonPanelLaneOption(option.OptionId)`
  - hover -> `_bootEntry.SetDungeonPanelLaneHover(hoveredOptionId)`
- `BootEntry.cs`
  - forwards both methods into `_worldView`
- `BootstrapSceneStateBridge.cs`
  - forwards both methods into the active entry
- `StaticPlaceholderWorldView.DungeonRun.PanelShell.cs`
  - hover updates hover state only
  - commit updates `_currentLaneSelectionText`
- `StaticPlaceholderWorldView.DungeonRun.ShellContracts.cs`
  - forwards `CurrentLaneSelectionText` into panel context
- `PrototypePresentationShell.DungeonRun.cs`
  - footer renders the forwarded committed value

## Conclusion

The dead-end suspicion is not confirmed by static audit on the accepted GitHub `main` snapshot.

The more accurate reading is:

- hover previews a lane card and updates hover-driven presentation
- commit updates `Current Lane`

## Hover Preview Note

Making `Current Lane` live-preview hovered lane before commit would be a UI behavior change, not a proven bug fix from this audit.

That kind of change should be requested explicitly as a semantics/UI behavior adjustment, not merged under a dead-end bug claim.

## Files Changed

- `docs/runtime/batch57-current-lane-semantics-audit.md`

## Validation

- Compile: `NOT RUN`
  - No runtime code changed in this batch.
- Validator: `N/A`
  - No content/tooling rail changed.
- Manual verification: `NOT RUN`
  - No fresh manual repro was provided, and no code fix was applied.

## UI Shape Changed?

- `No`

## Why No Code Change Was Correct

- The current producer -> shell surface -> presentation footer path is live in code.
- The audited semantics are commit-based rather than hover-based.
- Without fresh repro of a committed selection failing to update the footer, changing behavior here would be an unproven UI semantics change rather than a narrow bug fix.
