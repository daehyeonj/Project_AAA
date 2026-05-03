# Batch 86 Wait Cost Pressure Clock Checklist

Use this after the automated proof passes to confirm the second ExpeditionPrep choice feels honest in manual play.

## Before Waiting

- Confirm second `ExpeditionPrep` shows `Wait Cost: City pressure may rise if you recover`.
- Confirm `Pressure Clock` says the city need stock is consumed by the existing world-day economy tick.
- Confirm `[T] Recover 1 Day` says readiness improves and the world advances 1 day.
- Confirm the launch-now option still reads as `Ready`, `Ready with warning`, or `Blocked`.

## After Waiting

- Press `[T] Recover 1 Day` and confirm the prep board stays open.
- Confirm world day advances by 1 through the normal economy/recovery rail.
- Confirm readiness changes to `Ready` when the recovery ETA clears.
- Confirm `After waiting` reports the real stock/need tick, such as `stock -1`, or an honest shortage/stable reason.
- Confirm the recommendation shifts toward `Next: launch now; waiting again risks shortage`.

## Regression

- Confirm Stability and Surge route cards still show distinct consequence copy.
- Confirm `[Enter] Launch` still freezes the selected route in the confirmed plan.
- Confirm Batch10 smoke remains green before closing the batch.
- Do not accept fake wait penalties, a new fatigue system, ResultPipeline remapping, or per-frame rebuilds as proof.
