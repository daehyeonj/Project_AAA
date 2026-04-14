# Appflow Contract

`Project AAA` keeps shell visibility in `BootEntry` and `GameStateId`, but uses `AppFlowStage` and `AppFlowContext` as the canonical logical handoff contracts between threads.

## Stages

- `Boot`
- `MainMenu`
- `WorldSim`
- `CityHub`
- `ExpeditionPrep`
- `DungeonRun`
- `BattleScene`
- `ResultPipeline`

## Allowed Transitions

- `Boot -> MainMenu`
- `MainMenu -> WorldSim`
- `WorldSim <-> CityHub`
- `WorldSim / CityHub -> ExpeditionPrep`
- `ExpeditionPrep -> WorldSim`
- `ExpeditionPrep -> DungeonRun`
- `DungeonRun -> BattleScene`
- `BattleScene -> DungeonRun`
- `DungeonRun -> ResultPipeline`
- `ResultPipeline -> WorldSim`
- `WorldSim / CityHub -> MainMenu`

## Handoff Payloads

- `MainMenu -> WorldSim`
  - `AppFlowWorldSelection`
  - world day, selected city/dungeon, idle party id
- `WorldSim / CityHub -> ExpeditionPrep`
  - `AppFlowExpeditionPlan`
  - live prep read model, selected route, recommended route, dispatch policy, launch readiness
- `ExpeditionPrep -> DungeonRun`
  - `AppFlowDungeonRunContext.LaunchPlan`
  - confirmed city, dungeon, party, chosen route, and launch summary
- `DungeonRun -> BattleScene`
  - `AppFlowBattleContext`
  - encounter metadata, battle handoff payload, battle request snapshot
- `BattleScene -> DungeonRun`
  - `AppFlowBattleContext.ReturnPayload`
  - last battle outcome, encounter name, resolution snapshot
- `DungeonRun -> ResultPipeline`
  - `AppFlowResultContext`
  - `ExpeditionOutcome`, `OutcomeReadback`, resolved run state, applied-world marker
- `ResultPipeline -> WorldSim`
  - `AppFlowWorldSelection`
  - `AppFlowResultContext`
  - restores world focus while keeping the latest result readable for follow-up decisions

## Ownership Notes

- `AppFlowCoordinator` owns transition rules and session context only.
- `BootEntry` still owns shell presentation, scene visibility, camera updates, and input entry.
- `StaticPlaceholderWorldView` and `ManualTradeRuntimeState` still own economy, expedition, dungeon, battle, and result calculations.
- If `AppFlowStage` and `GameStateId` disagree, fix the adapter code. Do not add a second logical stage system.
