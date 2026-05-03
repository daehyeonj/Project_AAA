# Batch 90 Encounter Variety Route Pressure Checklist

Goal: confirm Dungeon Alpha battles feel different because the existing route, room, monster, and consequence data are being read back clearly, not because a new combat system was added.

## Stability Path

- Start from `city-a -> dungeon-alpha -> Rest Path`.
- Enter `Slime Front`.
- Confirm battle context mentions `Stability Pressure`, `Slime Front`, live enemy HP/ATK totals, and `Rest Shrine` sustain.
- Confirm enemy intent mentions controlled/predicted slime damage rather than generic enemy intent only.
- Clear the encounter and confirm the popover route plan includes `Route Check`, `Stability Pressure`, `Slime Front`, and `Rest Shrine`.

## Surge Path

- Return to second ExpeditionPrep and choose `Standard Path`.
- Reach `Greed Cache`, interact with `[E]`, and confirm `Cache Pressure` is armed for `Goblin Pair Hall`.
- Enter `Goblin Pair Hall`.
- Confirm battle context mentions `Surge Pressure`, live enemy HP/ATK totals, `Goblin Pair Hall`, `Cache Pressure`, and recovery strain.
- Select an attack/target and confirm target preview mentions `Cache Pressure payoff` plus the role/finish-window reason.
- Clear the encounter and confirm the popover carries `Cache Pressure`, reward secured, and strain warning.

## Regression Guard

- No elemental/crit/status system should appear.
- No new fatigue or recovery system should appear.
- Enemy stat values should not secretly change to justify the copy.
- Result/world board readback should still flow through existing room-interaction and ResultPipeline fields.
- Batch10 smoke must remain green.
