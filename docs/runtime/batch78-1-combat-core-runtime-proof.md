# Batch 78.1 Combat Core Runtime Proof

## Branch

- Selected branch: `B - minimal readback seam tuning`
- Reason: the Burst Window combat rail existed, but the first player turn in a fresh battle used the older turn-start event wrapper and did not refresh RPG-owned enemy intent before Alden's first action.
- Code seam changed: `RecordCurrentPartyTurnStartEvent()` now refreshes RPG-owned enemy intent preview state before recording the turn start event.
- Combat rules changed: `none`
- Encounter tuning changed: `none`

## Runtime Proof Command

```powershell
& 'C:\Program Files\Unity\Hub\Editor\6000.0.68f1\Editor\Unity.exe' -batchmode -nographics -projectPath 'C:\Users\umyo\Documents\Project_AAA' -executeMethod Batch78_1CombatCoreRuntimeProofRunner.RunBatch78_1CombatCoreRuntimeProof -logFile 'C:\Users\umyo\Documents\Project_AAA\unity-batch78-1-combat-proof.log'
```

## Selected Demo Path

- Scene: `Assets/Scenes/SampleScene.unity`
- City: `city-a`
- Route: `safe`
- Dungeon: `Dungeon Alpha`
- Encounter: `Slime Front`
- Target: `Slime A`
- Setup actor: `Alden`
- Setup action: `Power Strike`
- Payoff actor: `Mira`
- Payoff action: `Weak Point`

## Captured Runtime Readback

- Intent read: `Slime A (Bulwark) intends Attack on random target: Alden. Front Row -> Front Row | Front Row or Middle Row | Reachable.`
- Setup preview: `Expected: 10 dmg to Slime A`
- Setup after-hit preview: `HP 19 -> 9 | Opens Burst Window`
- Window summary: `Burst Window | Intent read | Payoff +3 | 2 ally action(s).`
- Setup actual log: `Alden read intent and opened Burst Window on Slime A (+3 for 2 ally actions).`
- Payoff preview: `Expected: 10 dmg to Slime A (7 + Burst 3)`
- Payoff after-hit preview: `Would defeat | Burst Window | Consumes Burst Window`
- Role payoff readback: `Response: Mira cashes Burst Window now. | Role payoff: Mira cashes Burst Window now.`
- Payoff actual log: `Mira cashed Burst Window on Slime A for +3.`
- Clear/consume: `Target defeated; window cleared`

## Proof Passes

- `PASS :: Intent read visible`
- `PASS :: Setup preview visible`
- `PASS :: Window trigger and log`
- `PASS :: Role payoff turn`
- `PASS :: Payoff preview visible`
- `PASS :: Payoff log and consume`
- `PASS :: UI/modal/skin regression sanity`

## Manual Player Repro

1. Open `SampleScene`.
2. Enter the world simulation from the main menu.
3. Select `city-a`.
4. Recruit a party if needed.
5. Enter the selected dungeon and choose the `safe` route.
6. Move into the first Alpha encounter, `Slime Front`.
7. On Alden's turn, read `Slime A` intent, choose `Power Strike`, and target `Slime A`.
8. Confirm the preview shows `HP 19 -> 9 | Opens Burst Window`, then commit.
9. On Mira's turn, choose `Weak Point` and target `Slime A`.
10. Confirm the preview shows `Expected: 10 dmg ... (7 + Burst 3)` and `Consumes Burst Window`, then commit.
11. Confirm the battle log says `Mira cashed Burst Window on Slime A for +3.`

## Regression Notes

- Runtime skin bridge: `RuntimeUiSkinBridge=present`
- Battle top strip: `TopStripFallback=True`
- Inventory in battle: `InventoryOpen=True`, `InventoryReadOnly=True`
- Modal input guard: `HudBlocksDungeonInput=True`
- Inventory close restore: `InventoryClosed=True`
- UI visual screenshot proof: not claimed in this batch; this is automated Play Mode runtime proof plus exact manual repro.

## Performance Notes

- The runtime seam refreshes intent once at party turn start, matching the existing RPG-owned subsequent-turn path.
- No per-frame scan, asset lookup, or OnGUI rebuild path was added.
- The new proof runner is editor-only under `Assets/_Game/Scripts/Editor`.
