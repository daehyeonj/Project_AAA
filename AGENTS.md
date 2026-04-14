# Project AAA Working Agreement

Project AAA is a Unity/C# vertical slice prototype for a city economy -> expedition prep -> dungeon run -> battle -> result -> world feedback loop.

## Read Order

1. `docs/architecture/README.md`
2. `docs/architecture/thread-ownership.md`
3. `docs/architecture/flow-contracts.md`
4. `docs/architecture/validation-matrix.md`
5. Thread-local notes under `Assets/_Game/Scripts/**` only after the repo-level docs above

If ad hoc handoff notes or thread-local markdown files conflict with `docs/architecture/*`, the repo-level architecture docs win. If the docs conflict with code, code wins and the docs must be updated in the same change.

## Thread Summary

- `Architecture`: owns repo-level working rules, ownership maps, contract registry, and validation guidance.
- `Appflow`: owns logical stage transitions and session handoff context.
- `WorldSim`: owns runtime source of truth and board/city/dungeon/road read models.
- `CityHub`: owns city decision-facing read models and recommendation signals.
- `ExpeditionPrep`: owns prep read models, launch readiness, route/approach choice, and confirmed expedition plans.
- `DungeonRun`: owns launched run state, encounter/event progression, and battle/result handoff payloads.
- `BattleScene`: owns battle request, runtime state, command execution, UI surface, and battle resolution.
- `ResultPipeline`: owns expedition outcome, world delta, and outcome readback contracts.

## Change Scope Rules

- Pick one primary thread for each task.
- Cross-thread edits are allowed only for contract wiring, adapters, compile fixes, or doc synchronization.
- Prefer additive helpers, builders, coordinators, and wrappers over large rewrites.
- Do not introduce a second source of truth for data that already belongs to another thread.
- Treat strings and HUD labels as display adapters, not canonical contracts.
- Keep `BootEntry.cs` and `StaticPlaceholderWorldView*.cs` thin when possible. New logic should prefer owner-side helpers before expanding those hot spots.
- When a new cross-thread payload is needed, add it near the canonical owner thread and map to it through adapters.
- Update the relevant `docs/architecture/*` files whenever ownership, handoff, or validation rules change.

## Ownership Guardrails

- `Appflow` may reference payloads from other threads, but it does not own economy math, battle rules, or result application.
- `WorldSim` owns runtime facts; read models may summarize them, but must not replace them.
- `CityHub` can interpret city pressure and opportunities, but must not freeze launch or run contracts.
- `ExpeditionPrep` can validate and confirm launch, but must not own launched runtime or result writeback.
- `DungeonRun` can drive exploration and battle/result handoff, but must not recalculate city policy or world deltas.
- `BattleScene` can resolve combat, but must not own dungeon progression outside battle or mutate world state directly.
- `ResultPipeline` can translate run output into world-facing deltas/readback, but must not own battle legality or world simulation rules.

## Validation Rules

- Run Unity batch compile before calling a task done.
- Run the smallest relevant smoke path for the touched thread and at least one adjacent handoff.
- If a manual play path cannot be checked, say so explicitly and record which static checks were used instead.
- When contracts change, verify both producer and consumer files plus the corresponding architecture docs.

## Canonical Contract Heuristics

- `AppFlowStage` is the canonical logical flow state. `GameStateId` is a shell/presentation adapter.
- `WorldBoardReadModel` and its child read models are the canonical world-side board contracts.
- `CityDecisionReadModel` is the canonical city decision contract.
- `ExpeditionPrepReadModel` is the canonical draft prep contract, and `ExpeditionPlan` is the canonical confirmed launch contract.
- `ExpeditionRunState` is the canonical launched run contract.
- `PrototypeBattleRequest`, `PrototypeBattleRuntimeState`, and `PrototypeBattleResolution` are the canonical battle contracts.
- `ExpeditionOutcome -> WorldDelta -> OutcomeReadback` is the canonical result pipeline chain.

## Current Hot Spots

- `Assets/_Game/Scripts/Bootstrap/BootEntry.cs`
- `Assets/_Game/Scripts/World/StaticPlaceholderWorldView.cs`
- `Assets/_Game/Scripts/Dungeon/StaticPlaceholderWorldView.DungeonRun.cs`
- `Assets/_Game/Scripts/Core/PrototypeDebugHUD.cs`

Touch these files only when the owner-side helper/builder/coordinator path is not enough.
