# Batch 87 Dungeon Route Feel Checklist

Manual QA goal: confirm Dungeon Alpha's Stability and Surge routes feel different inside the dungeon, not only on the final result screen.

## Stability Run

- Open the second ExpeditionPrep and inspect the Rest Path card.
- Confirm the card reads as a safer sustain route with lower strain and lower payout.
- Launch Stability and confirm DungeonRun readback keeps the Stability / sustain / shrine route feel visible.
- Clear the first encounter or inspect the encounter popover and confirm the route check says the plan is keeping HP/readiness protected.
- Confirm final/world board consequence still reads as lower return with safer recovery.

## Surge Window

- Open the second ExpeditionPrep and inspect the Standard Path card.
- Confirm the card reads as a pressure route with higher payout and higher strain.
- Launch Surge and confirm DungeonRun readback keeps the Surge / pressure / Greed Cache / recovery strain feel visible.
- Clear the first encounter or inspect the encounter popover and confirm the route check says payout is on track while recovery strain rises.
- Confirm final/world board consequence still reads as larger return with worse readiness/recovery strain.

## Regression Sanity

- Confirm `[T] Recover 1 Day` still advances the existing world day/recovery rail.
- Confirm Batch10 smoke stays green.
- Confirm no route-feel copy appears to be rebuilt from OnGUI layout, mouse move, or no-op world selection.
- Confirm no UI skin/layout, inventory/equipment, ResultPipeline rewrite, new dungeon, or new route portfolio work slipped into this batch.
