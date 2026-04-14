# Thread Ownership Map

This map defines who owns what in the current Project AAA codebase. The owner thread decides the canonical contract and validation rule. Other threads may consume or adapt the contract, but should not duplicate it.

## Architecture

- Owner responsibility: repo-level working agreement, thread ownership, shared contract registry, validation rules, and Codex workflow guardrails
- Owned files: `AGENTS.md`, `docs/architecture/*`
- Allowed dependencies: all threads for documentation and reference
- Forbidden coupling: new runtime/service hub, hidden gameplay logic in docs, ownership that only exists in prompts

## Appflow

- Owner responsibility: logical stage transitions and cross-thread session context
- Owned types/files:
  - `Assets/_Game/Scripts/Bootstrap/AppFlowContracts.cs`
  - `Assets/_Game/Scripts/Bootstrap/AppFlowCoordinator.cs`
  - `Assets/_Game/Scripts/Bootstrap/APPFLOW_CONTRACT.md`
- Allowed dependencies:
  - consume `WorldSim`, `ExpeditionPrep`, `DungeonRun`, `BattleScene`, and `ResultPipeline` contracts as payloads
  - map to shell state through `BootEntry.cs`
- Forbidden coupling:
  - economy/resource mutation logic
  - battle legality or turn resolution
  - result application rules

## WorldSim

- Owner responsibility: runtime truth for economy, stock, shortages, route usage, parties, active expeditions, and world-side board snapshots
- Owned types/files:
  - `Assets/_Game/Scripts/World/ManualTradeRuntimeState.cs`
  - `Assets/_Game/Scripts/World/WorldBoardReadModels.cs`
  - `Assets/_Game/Scripts/World/StaticPlaceholderWorldView.WorldBoardReadModel.cs`
  - `Assets/_Game/Scripts/World/WORLDSIM_READMODEL.md`
- Allowed dependencies:
  - consume `ResultPipeline` output for writeback
  - expose derived read models to `CityHub` and `ExpeditionPrep`
- Forbidden coupling:
  - CityHub-specific CTA policy stored as world truth
  - launch confirmation ownership
  - battle execution logic

## CityHub

- Owner responsibility: city-focused decision read models, bottleneck/opportunity signals, and "what should I do now?" interpretation
- Owned types/files:
  - `Assets/_Game/Scripts/World/CityDecisionReadModels.cs`
  - `Assets/_Game/Scripts/World/CityDecisionModelBuilder.cs`
  - `Assets/_Game/Scripts/World/CITYHUB_DECISION_LAYER.md`
- Allowed dependencies:
  - `WorldBoardReadModel`
  - `CityStatusReadModel`
  - latest result/readback summaries from `WorldSim`
- Forbidden coupling:
  - direct economy mutation
  - confirmed launch payload ownership
  - launched runtime or battle state ownership

## ExpeditionPrep

- Owner responsibility: prep read model, route/approach choice, readiness/blockers, and the confirmed expedition launch payload
- Owned types/files:
  - `Assets/_Game/Scripts/Bootstrap/ExpeditionPrepContracts.cs`
  - `Assets/_Game/Scripts/Bootstrap/ExpeditionPrepModelBuilder.cs`
  - `Assets/_Game/Scripts/Bootstrap/ExpeditionLaunchCoordinator.cs`
  - `Assets/_Game/Scripts/Bootstrap/EXPEDITION_PREP_CONTRACT.md`
  - `Assets/_Game/Scripts/World/StaticPlaceholderWorldView.ExpeditionPrep.cs`
- Allowed dependencies:
  - `WorldBoardReadModel`
  - `CityDecisionReadModel`
  - party availability from `ManualTradeRuntimeState`
- Forbidden coupling:
  - launched dungeon runtime ownership
  - battle execution ownership
  - result/writeback ownership

## DungeonRun

- Owner responsibility: launched expedition runtime state, room/encounter progression, event/pre-elite flow, and battle/result handoff payloads
- Owned types/files:
  - `Assets/_Game/Scripts/Dungeon/ExpeditionRunContracts.cs`
  - `Assets/_Game/Scripts/Dungeon/DungeonRunStateFactory.cs`
  - `Assets/_Game/Scripts/Dungeon/StaticPlaceholderWorldView.RunState.cs`
  - `Assets/_Game/Scripts/Dungeon/StaticPlaceholderWorldView.DungeonRun.cs`
  - `Assets/_Game/Scripts/Dungeon/DUNGEON_RUN_CONTRACT.md`
- Allowed dependencies:
  - confirmed `ExpeditionPlan`
  - `BattleScene` contracts for handoff
  - `ResultPipeline` contracts for run completion
- Forbidden coupling:
  - recomputing city recommendation ownership
  - mutating world truth directly outside the result handoff
  - owning battle rules beyond handoff/return integration

## BattleScene

- Owner responsibility: battle request, command selection, target selection, battle runtime state, resolution, and battle UI projection
- Owned types/files:
  - `Assets/_Game/Scripts/Rpg/Battle/PrototypeBattleContracts.cs`
  - `Assets/_Game/Scripts/Rpg/Battle/PrototypeCombatDomainData.cs`
  - `Assets/_Game/Scripts/Rpg/Battle/PrototypeBattleUiData.cs`
  - `Assets/_Game/Scripts/Dungeon/StaticPlaceholderWorldView.BattleContracts.cs`
  - `Assets/_Game/Scripts/Rpg/Battle/BATTLE_EXECUTION_CONTRACT.md`
- Allowed dependencies:
  - `BattleHandoffPayload` from `DungeonRun`
  - `BattleReturnPayload` back to `DungeonRun`
  - runtime state/view-model helpers inside `Rpg/Battle`
- Forbidden coupling:
  - dungeon exploration sequencing
  - world writeback
  - appflow transition ownership

## ResultPipeline

- Owner responsibility: converting resolved run output into expedition outcome, world delta, and outcome readback
- Owned types/files:
  - `Assets/_Game/Scripts/Results/ExpeditionOutcome.cs`
  - `Assets/_Game/Scripts/Results/WorldDelta.cs`
  - `Assets/_Game/Scripts/Results/OutcomeReadback.cs`
  - `Assets/_Game/Scripts/Results/ResultPipeline.cs`
- Allowed dependencies:
  - `PrototypeRpgRunResultSnapshot`
  - `ExpeditionRunState`
  - `PrototypeBattleResolution` or battle result snapshots as upstream detail
- Forbidden coupling:
  - battle command legality
  - world simulation rule ownership
  - city recommendation ownership

## Cross-Thread Hygiene Rules

- Shared contracts live with the thread that conceptually owns them, even if the file currently sits in another folder.
- Thread-local builders/coordinators may assemble data from another thread, but must not silently replace the owner contract.
- `BootEntry.cs` can host shell-facing getters and handoff calls, but should not become the canonical owner of shared gameplay contracts.
