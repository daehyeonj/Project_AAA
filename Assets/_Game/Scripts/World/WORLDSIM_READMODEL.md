# WorldSim Read Model

## Runtime State
- `ManualTradeRuntimeState` stays as the source of truth for economy, party, expedition, stock, shortage, and route usage changes.
- Result application still lands in runtime state first through the existing expedition/result pipeline.

## Read Model Layer
- `StaticPlaceholderWorldView.BuildWorldBoardReadModel()` builds a read-only board snapshot from runtime state plus placeholder world data.
- `WorldBoardReadModel` is the world-level board contract for WorldSim.
- `CityStatusReadModel` is the city-level contract for CityHub-style views and world selection summaries.
- `DungeonStatusReadModel`, `RoadStatusReadModel`, `ExpeditionStatusReadModel`, and `WorldDecisionSignalReadModel` are the minimum support models for board reads.

## Consumer Rule
- WorldSim UI, HUD, and board summaries should prefer `WorldBoardReadModel` instead of reaching into raw runtime values directly.
- Localized or composite strings can still exist as presentation output, but they should be derived from read models, not treated as source of truth.

## Reuse For CityHub
- Reuse `CityStatusReadModel` for city-level status, shortages, surplus, linked dungeon, active expedition, and latest result summaries.
- Reuse `CityStatusReadModel.Decision` when CityHub needs an explicit "what should the player do now?" layer.
- `CityDecisionModelBuilder` should stay a thin builder over existing read models, not a second economy system.
- If CityHub needs more detail later, extend the builder additively instead of bypassing the read model path.

## Batch 80 Result-Pressure Board
- Selected-city pressure board text is a presentation aggregation over `CityStatusReadModel`, `CityStatusReadModel.Decision`, the latest result read model, outcome/writeback readback, and launch readiness.
- The board may answer what happened, why it mattered, what changed, next action, and readiness/re-entry, but it must not become a new world pressure system or ResultPipeline owner.
- Build or refresh the copy through existing world/result/selection/prep refresh points; do not rebuild result-pressure summaries from `OnGUI`, mouse-move, or per-frame update paths.

## Batch 86 Wait Cost Pressure Clock
- `Recover 1 Day` from ExpeditionPrep must keep using `ManualTradeRuntimeState.RunEconomyDay()` plus dispatch recovery advancement as the runtime source of truth.
- Wait-cost copy may summarize selected-city stock, consumed need count, shortages, need pressure, recovery ETA, and route recommendation, but it must not create a second stock ledger or fake penalty.
- If a day tick consumes city need stock, the prep readback may say `stock -1`; if it does not, the readback should explain the stable/shortage reason from the same read model rail.
