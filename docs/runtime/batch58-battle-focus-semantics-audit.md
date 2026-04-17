# Batch 58 Battle Focus Semantics Audit

Date: `2026-04-17`

## Selected Branch

- `A` - docs-only close-out

## Intended Semantics Of Battle Focus

- `mixed`

More specifically:

- command focus is `commit-selection` driven
- target focus is only active during `target selection`
- within active target selection, the currently focused target may follow the live hovered enemy and otherwise falls back to the locked/default target

This is not the same as a full hover-preview redesign of battle focus.

## Why This Branch Was Allowed

- This batch asked for a narrow audit-or-fix pass on the accepted GitHub `main` snapshot.
- The current accepted runtime can answer the semantics question through static audit.
- No stale/dead-end bug was confirmed in the live producer -> surface -> presentation path.
- Changing battle focus to a broader hover-preview model would be a behavior change, not a bug fix.

## Q1-Q4 Answers

### Q1. In current accepted mainline, is battle focus intended to be hover-preview or commit/selection-driven?

- `mixed`

Command focus is selection-driven.

Evidence:

- `Assets/_Game/Scripts/Dungeon/StaticPlaceholderWorldView.DungeonRun.cs`
  - `GetBattleUiSelectedActionKey()` maps `_queuedBattleAction` into the selected command key.
- `Assets/_Game/Scripts/Rpg/Battle/StaticPlaceholderWorldView.RpgBattleSurfaceOwnership.cs`
  - `BuildRpgOwnedBattleUiCommandSurfaceData(...)` uses that selected action key to build the command panel and contextual details.
- `Assets/_Game/Scripts/Core/PrototypePresentationShell.DungeonRun.cs`
  - `ResolveDungeonBattleFocusDetail(...)` resolves the focus card from `commandSurface.SelectedActionKey` and selected detail rows, not from the hovered action key.
  - `ShouldShowDungeonBattleCommandFocus(...)` only shows command focus for selected `skill`, `move`, or `retreat`.

Target focus is target-selection-driven.

Evidence:

- `Assets/_Game/Scripts/Core/PrototypePresentationShell.DungeonRun.cs`
  - target hover is only forwarded through `_bootEntry.SetBattleTargetHover(...)` while `surface.TargetSelection.IsActive`
- `Assets/_Game/Scripts/Rpg/Battle/StaticPlaceholderWorldView.RpgBattleSurfaceOwnership.cs`
  - `BuildRpgOwnedBattleUiTargetSelectionData(...)` only activates the target-focus surface while `_battleState == BattleState.PartyTargetSelect`
- `Assets/_Game/Scripts/Dungeon/StaticPlaceholderWorldView.DungeonRun.cs`
  - `GetBattleUiFocusedMonster()` prefers hovered target, then locked target, then first valid fallback target

### Q2. Does the current presentation shell treat hover plumbing and persistent focus rendering as separate things?

- `Yes`

Reason:

- `PrototypePresentationShell.DungeonRun.cs`
  - stage and bottom-HUD drawing collect `hoveredTargetId` / `hoveredActionKey`
  - those values are forwarded back into runtime through `_bootEntry.SetBattleTargetHover(...)` and `_bootEntry.SetBattleActionHover(...)`
- the actual overlay/focus render does not read those raw hover ids directly
  - command focus render reads `surface.CommandSurface.SelectedActionKey`
  - target focus render reads `surface.TargetSelection`

So the shell uses hover as an input signal, while the visible focus card is driven by the rebuilt battle UI surface.

### Q3. Does the battle surface source selected/focused state from real owner/runtime state, or is there a dead-end path where the UI reads stale data?

- `It sources from live owner/runtime state`

Evidence:

- `BootEntry.cs` and `BootstrapSceneStateBridge.cs`
  - only forward hover/trigger calls into the world view
- `StaticPlaceholderWorldView.RpgBattleSurfaceOwnership.cs`
  - rebuilds the battle UI surface from live runtime every refresh
- `StaticPlaceholderWorldView.DungeonRun.cs`
  - `UpdateBattleMouseInteraction(...)` clears hovered monster state whenever battle is not in `PartyTargetSelect`
  - `CancelTargetSelection()` clears hover state, clears `_queuedBattleAction`, returns to `PartyActionSelect`, and refreshes prompt/presentation
- `Assets/_Game/Scripts/Rpg/Battle/StaticPlaceholderWorldView.RpgBattleIntentOwnership.cs`
  - `AdvanceRpgOwnedBattleAfterPartyAction()` clears `_queuedBattleAction`, `_hoverBattleAction`, and battle hover state before the next turn step

The audited path does not show a dead-end surface still reading old focus data after runtime selection state has been cleared.

### Q4. Is there a reproducible bug inside the accepted semantics, or only a mismatch between user expectation and intended behavior?

- `Only expectation/semantics ambiguity was confirmed by static audit`

No fresh manual repro was supplied in this batch for:

- focus text remaining after cancel/back even though selection state cleared
- target focus staying pinned after target selection ended
- selected command summary desyncing from the real committed action

Without that repro, the audit does not justify changing the current focus behavior.

## Targeted Static Audit Summary

The current accepted battle focus path is:

1. presentation hover/click capture
2. bootstrap forwarding
3. world-view hover/selection update
4. battle UI surface rebuild from runtime
5. overlay render from rebuilt surface

Concrete path:

- `PrototypePresentationShell.DungeonRun.cs`
  - collects hovered action/target ids
  - forwards them into `BootEntry`
  - renders command focus from selected command surface data
  - renders target focus only while target selection is active
- `BootEntry.cs`
  - forwards hover and trigger calls into `_worldView`
- `BootstrapSceneStateBridge.cs`
  - forwards those calls into the active boot entry
- `StaticPlaceholderWorldView.DungeonRun.cs`
  - stores transient hover state
  - clears that state when target selection is canceled or no longer active
- `StaticPlaceholderWorldView.RpgBattleSurfaceOwnership.cs`
  - rebuilds command focus from `_queuedBattleAction`
  - rebuilds target focus from focused target state inside current battle state

## Conclusion

The accepted mainline battle focus behavior is not a confirmed stale/dead-end bug.

The more accurate reading is:

- command focus is committed-action driven
- target focus is active-target-selection driven
- hover can temporarily define the currently focused enemy while target selection is active
- outside target selection, that hover state is explicitly cleared

## Behavior-Change Note

If the desired behavior is:

- always show command focus from hover instead of committed action
- keep target focus live-preview driven outside active target selection
- broaden command focus cards to behave like a hover-preview redesign

that should be treated as a new UI behavior request, not as a bug-fix batch.

## Files Changed

- `docs/runtime/batch58-battle-focus-semantics-audit.md`

## Validation

- Compile: `NOT RUN`
  - No runtime code changed in this batch.
- Validator: `N/A`
  - No content/tooling rail changed.
- Manual verification: `NOT RUN`
  - No fresh manual repro was supplied, and no code fix was applied.

## UI Shape Changed?

- `No`

## Why No Code Change Was Correct

- The current battle focus path is still live and internally coherent.
- The audited ambiguity is about semantics, not a proven stale-data seam.
- Changing focus behavior here without a fresh bug repro would be a behavior adjustment to the accepted battle HUD, not a narrow preservation-safe fix.
