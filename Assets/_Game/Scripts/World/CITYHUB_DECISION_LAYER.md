# CityHub Decision Layer

## Input
- `ManualTradeRuntimeState` remains the runtime source of truth.
- `StaticPlaceholderWorldView.BuildWorldBoardReadModel()` still builds the world snapshot.
- `CityDecisionModelBuilder.Build(board, city)` is a thin assembly step on top of `CityStatusReadModel`, the linked `DungeonStatusReadModel`, route pressure, and recent expedition readback.

## Output
- `CityDecisionReadModel` is the CityHub-facing read model for "what matters now."
- `CityBottleneckSignal` highlights the top city blockers.
- `CityOpportunitySignal` explains why a dungeon window matters to this city.
- `CityActionRecommendation` keeps the next 1-3 actions deterministic and explainable.
- `RecentImpactSummary` turns the last expedition/result into a next-decision hint instead of a dead-end log.

## Consumer Rule
- `PrototypePresentationShell` should prefer `CityStatusReadModel.Decision` when a city is selected.
- CityHub presentation should read decision signals first, then fall back to raw stock/supply details.
- Do not move economy math into this layer. The builder only assembles meaning from existing read models.

## ExpeditionPrep Handoff
- Reuse the selected city's `Decision.WhyCityMattersText` as the high-level expedition intent.
- Reuse the first `CityOpportunitySignal` for "why this dungeon now" and the reward/bottleneck link.
- Reuse the first `CityActionRecommendation` when ExpeditionPrep needs a default CTA or reminder.
- Reuse `NeedPressureStateId`, `DispatchReadinessStateId`, and `RecommendedRouteSummaryText` before adding new prep-only fields.

## Batch 27 Shared City-Side Authoring
- Representative chains may now resolve a shared city-decision meaning asset before CityHub falls back to local string assembly.
- Keep the canonical flow `world state / latest result -> CityDecisionReadModel -> ExpeditionPrepReadModel`.
- Use shared city-decision meaning for bottleneck framing, opportunity rationale, and recommendation rationale; keep economy math and final action ordering inside CityHub-owned code.
