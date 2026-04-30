# Batch 84 Recovery Pressure Choice v1

## Intent

Batch 84 turns the second `ExpeditionPrep` into an immediate choice:

- launch now while the city/party are `Ready`, `Ready with warning`, or `Blocked`
- wait/recover one day and accept the existing world day / pressure / economy tick
- re-evaluate Stability vs Surge after readiness changes

This is not a new fatigue, penalty, economy, or result system. It is a player-facing bridge over the existing `WorldDayCount`, dispatch recovery, `LaunchReadiness`, city pressure, and route-appetite readbacks.

## Player Readback

Second `ExpeditionPrep` now exposes:

- `LaunchNowChoiceText`: `Launch now: Ready with warning...` or `Launch now: Ready...`
- `RecoverOneDayChoiceText`: `Recover 1 Day...` / `Recover 1 Day / Wait 1 Day...`
- `AfterRecoveryPreviewText`: what should happen to readiness while pressure/stock rails tick
- `RecoveryPressureChoiceText`: compact launch-now vs recover comparison
- `RouteAppetiteAfterRecoveryText`: how recovery changes the Stability/Surge temptation

The board also adds `[T] Recover 1 Day`, which calls the same world-day rail as the existing world action instead of simulating a separate penalty.

## Proof

- Compile: `PASS`, log `unity-batch84-compile.log`
- Targeted proof: `PASS`, log `unity-batch84-recovery-pressure-proof.log`
- Batch83 regression proof: `PASS`, log `unity-batch84-batch83-proof.log`
- Batch10 smoke: `PASS`, log `unity-batch84-smoke.log`

Targeted proof marker:

- `PASS :: Second ExpeditionPrep context`
- `PASS :: Recover 1 Day choice`
- `BeforeDay=1 | AfterDay=2`
- readiness changes from warning recovery to `Ready / low recovery risk`
- route appetite changes to recovery-cleared Stability/Surge readback

## Guardrails

- No new fatigue system.
- No fake wait penalty.
- No ResultPipeline rewrite.
- No per-frame rebuild.
- Existing Batch10 smoke remains the regression floor.
