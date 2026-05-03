# Batch 88 Dungeon Room Interaction Checklist

## Scope

- Primary owner: `DungeonRun`.
- Runtime surface: current grid dungeon plus standard JRPG battle.
- Target beats: Alpha Stability `Rest Shrine` and Alpha Surge `Greed Cache`.

## Manual QA Path

1. Launch `city-a -> dungeon-alpha -> safe`.
2. Clear `Slime Front`, move to `Rest Shrine`, and confirm the prompt says `[E] Use Rest Shrine`.
3. Press `[E]` and confirm feedback mentions real recovery/sustain, not a fake penalty.
4. Launch or proof `city-a -> dungeon-alpha -> risky`.
5. Clear `Mixed Front`, move to `Greed Cache`, and confirm the prompt says `[E] Open Greed Cache`.
6. Press `[E]` and confirm carried loot increases by the route cache amount.
7. Clear the next encounter and confirm the result popover includes the cache consequence.
8. Finish/return and confirm CityHub latest-result evidence includes the room consequence.

## Guardrails

- No legacy choice-only dungeon shell revival.
- No new fatigue/economy/result system.
- No fake room penalty.
- No per-frame room/readback rebuild.
- Inventory/modal and battle-result overlays should block interaction through the existing input gate.

## Automated Proof

- Targeted proof: `Batch82RepeatCoreLoopProofRunner.RunBatch88DungeonRoomInteractionProof`.
- Regression smoke: `Batch10SmokeValidationRunner.RunBatch10SmokeValidation`.
