# Mainline Snapshot And Legacy Shell Cutoff Rejection

Date: `2026-04-17`

## Purpose

This note records the repository state that was intentionally synced to GitHub main on April 17, 2026 so external GPT-based review can inspect the same live runtime picture the local team is working against.

This sync is a runtime-state preservation sync, not a claim that every newer content or authoring branch outcome should override the currently accepted local runtime.

## Current Runtime Baseline To Preserve

- Dungeon runtime should read as the currently accepted `grid dungeon explore shell`.
- battleScene should keep the currently accepted HUD/layout rather than being silently redesigned during unrelated cleanup.
- Runtime bug fixes that preserve the accepted UI are allowed.
- Prompts that would remove, redesign, or roll back the active UI must be rejected unless the user explicitly approves that UI change.

## Batch Numbering Note

The reviewed prompt file on April 17, 2026 was:

- `C:\Users\umyo\Downloads\batch55_prompt_runtime_recovery_phase1_legacy_dungeon_shell_cutoff_utf8.txt`

The user later referred to the same request as `Batch 54`.

This document refers to that same rejected request as the `legacy dungeon shell cutoff batch`.

## Why The Legacy Shell Cutoff Batch Was Rejected

The batch was rejected because its requested implementation would not be a narrow internal cleanup. It would actively cut live producer, shell-contract, presentation, and bootstrap seams that still feed the currently visible dungeon runtime UI.

The explicit user instruction for this turn was:

- if a prompt would change or roll back the existing UI, refuse it and explain why

The requested batch directly conflicts with that rule because it asks to:

- remove legacy route-choice, event-choice, and pre-elite-choice shell states end-to-end
- remove shell visibility flags from the shell surface contract
- delete or dead-end active presentation consumers
- remove or hard-disable bootstrap/input pass-through seams

That is a live UI behavior change, not a passive audit.

## Concrete Audit Evidence Seen Before Rejection

### Presentation consumers still live

- `Assets/_Game/Scripts/Core/PrototypePresentationShell.DungeonRun.cs`
  - `DrawDungeonRunShell()` still branches on:
    - `shellSurface.IsRouteChoiceVisible`
    - `shellSurface.IsEventChoiceVisible`
    - `shellSurface.IsPreEliteChoiceVisible`
  - `DrawLegacyDungeonRouteFallbackShell(...)` still exists as a live consumer.
  - `DrawDungeonDecisionShell(...)` still exists as a live consumer for event and pre-elite states.

### Shell contract still carries the legacy modes

- `Assets/_Game/Scripts/Dungeon/StaticPlaceholderWorldView.DungeonRun.ShellContracts.cs`
  - `PrototypeDungeonRunShellSurfaceData` still exposes:
    - `IsRouteChoiceVisible`
    - `IsEventChoiceVisible`
    - `IsPreEliteChoiceVisible`
  - `BuildDungeonRunShellSurfaceData(...)` still copies those flags through.
  - Shell mode resolution still treats those states as meaningful runtime paths.

### Producer state is still live

- `Assets/_Game/Scripts/Dungeon/StaticPlaceholderWorldView.DungeonRun.cs`
  - `DungeonRunState` still includes:
    - `RouteChoice`
    - `EventChoice`
    - `PreEliteChoice`
  - Public visibility booleans still exist for those states.
  - Entry helpers still exist, including:
    - `OpenEventChoicePanel()`
    - `OpenPreEliteChoicePanel()`
  - Input handlers still exist for:
    - route choice
    - event choice
    - pre-elite choice

### Bootstrap and bridge seams still expose the path

- `Assets/_Game/Scripts/Bootstrap/BootEntry.cs`
- `Assets/_Game/Scripts/Bootstrap/BootstrapSceneStateBridge.cs`

These files still expose or forward route/event/pre-elite related accessors and trigger seams. Executing the cutoff batch would therefore change the reachable runtime UI path, not merely hide dead code.

## Accepted Decision

The cutoff batch was not executed.

The rejection was intentional and correct because:

- it would alter the current dungeon UI path
- it would conflict with the user’s explicit no-UI-change rule
- it was requested at a time when the team wanted the current UI preserved and mirrored to GitHub for external GPT inspection

## What Was Allowed Instead

The allowed work for this phase was:

- preserve the current local runtime state
- sync that preserved state to GitHub
- document the rejection so GPT/review tooling can understand why the cutoff batch was intentionally not executed
- keep doing UI-preserving bug fixes only

## Guidance For Future GPT Review

If GPT inspects GitHub after this sync, it should interpret the repository as follows:

- the current mainline snapshot is the authoritative runtime picture the team wants reviewed
- the legacy dungeon shell cutoff batch was intentionally rejected on April 17, 2026
- that rejection should not be treated as a missed implementation task
- any future attempt to remove those live dungeon shell paths requires fresh explicit approval because it changes the currently preserved UI
