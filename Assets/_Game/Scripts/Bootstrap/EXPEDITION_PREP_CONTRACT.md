# ExpeditionPrep Contract

## Input
- `StaticPlaceholderWorldView` builds `ExpeditionPrepReadModel` from `WorldBoardReadModel`, `CityDecisionReadModel`, current party state, current dispatch policy, and current route candidates.
- The prep read model is the screen-facing summary for route choice / dispatch confirmation.
- `CityDecisionReadModel` remains the source for city bottleneck, opportunity, and recommended action linkage.

## Source Of Truth
- `ExpeditionPlan` is the launch payload that freezes:
  - origin city
  - target dungeon
  - party
  - selected route
  - approach / dispatch policy
  - linked bottlenecks and opportunities
  - readiness summary
  - expected reward tags
- `LaunchReadiness` and `PrepBlocker` explain why launch is blocked or allowed with warnings.
- `RouteChoice` and `ApproachChoice` stay lightweight. They explain selection intent without simulating the whole dungeon.

## Handoff
- During route choice, `AppFlowExpeditionPlan` carries the live `PrepReadModel`, `LaunchReadiness`, and `CurrentPlan` as the source-of-truth draft plan.
- `ConfirmedPlan` is only populated after the launch is actually locked.
- `ExpeditionLaunchCoordinator` owns the final validation/build step that turns the prep read model into the confirmed launch payload.
- On confirm, `TryConfirmRouteChoice()` freezes a confirmed `ExpeditionPlan` before `BeginDungeonRun()`.
- `AppFlowDungeonRunContext.LaunchPlan` carries the confirmed payload into DungeonRun/AppFlow.
- DungeonRun logs now reference the confirmed objective so the launch reason stays visible after entering the run.
- Batch 19 re-entry rule: when the player comes back through `ResultPipeline -> WorldSim -> CityHub`, `ExpeditionPrepReadModel` should carry the refreshed city decision context forward too, especially the top recent impact and recommendation summary/reason that made the next launch prompt relevant.
- Batch 79 operating-scenario rule: route option UI may surface route-meaning scenario/party/combat/follow-up copy, but `RouteChoice` stays lightweight and `ExpeditionPlan` remains the confirmed launch payload.
- Batch 79.3 re-entry cache rule: after result return, AppFlow's active prep model must be refreshed on explicit ExpeditionPrep open, route selection, and dispatch policy changes. The route prompt may stay focused on `LaunchReadiness`; returned city decision evidence belongs in `ExpeditionPrepReadModel` and route aftermath echo.
- Batch 83 second-run decision rule: ExpeditionPrep may expose display-only last-run change, party carry-forward, stability/surge appetite, launch-warning, and route-appetite recommendation strings. These are derived from existing result, party/loadout, `LaunchReadiness`, city pressure, and route-option data; they are not new progression, reward, recovery, or result contracts.
- Batch 84 recovery-pressure rule: ExpeditionPrep may expose launch-now vs recover-one-day readbacks and a `[T] Recover 1 Day` action. Waiting must advance the existing world day / dispatch recovery rail and rebuild readbacks from `LaunchReadiness`, city pressure, and route-option surfaces. Do not add a separate fatigue system, fake wait penalty, or ResultPipeline-side remapping for this choice.
