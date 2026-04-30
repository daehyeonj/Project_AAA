# Batch 82 Repeatable Core Game Loop v1

## Selected Branch

Branch A: the accepted vertical slice already repeats through the same runtime contracts, but the project needed a targeted proof that the second-run context remains coherent and actionable.

No new gameplay system was added. Batch82 locks the first playable loop by proving the existing CityHub, ExpeditionPrep, DungeonRun, ResultPipeline, world board, party growth, and equipment readbacks survive a second prep/launch decision.

## Repeat Loop Proven

Target loop:

`World -> CityHub -> ExpeditionPrep -> DungeonRun -> ResultPipeline -> World/CityHub -> ExpeditionPrep -> DungeonRun`

Proof route:

- `city-a -> dungeon-alpha -> safe`
- first route scenario: `Stability Run`
- first result: `run_clear`, `mana_shard x16`
- second prep: latest result, city pressure, party growth, equipped gear, route choice, launch gate, and recommendation reason remain visible
- second run: launch can be confirmed again through the same `ExpeditionPlan` handoff

## Player-Facing Evidence

After the first result return, the CityHub pressure board carries the compact board readback:

- `Latest: Cleared: Stability Run | Returned mana_shard x16 | Party Stable`
- `Changed: Stock +16 mana_shard | Pressure Urgent -> Stable | Readiness Ready -> Recovering`
- `Next: Stabilize for 1 day before the next push.`
- `Ready: warning | recovery 1 day | party idle | route available`

Second ExpeditionPrep carries the changed context:

- latest result: `State=run_clear`, `Route=Rest Path`, `Loot=mana_shard x16`
- growth: `Alden +16 XP`
- equipment: `Rune equipped Stormglass Focus (ATK +2 POW +1)`
- loadout: `Rune: Slots RA Stormglass Focus`
- gate: `Commit allowed with warnings: city readiness is not fully recovered`
- route contrast: `Stability Run` remains safe while `Surge Window` remains the higher-pressure alternative

## Validation

- Compile: `PASS`, `unity-batch82-compile.log`
- Targeted repeat-loop proof: `PASS`, `unity-batch82-repeat-loop-proof.log`
- Batch10 smoke: `PASS`, `unity-batch82-smoke.log`
- Batch80.1 pressure board regression: `PASS`, `unity-batch82-board-proof.log`
- Batch79.3 CityHub -> ExpeditionPrep re-entry: `PASS`, `unity-batch82-reentry-proof.log`
- Batch79.2 ResultPipeline intent packaging: `PASS`, `unity-batch82-packaging-proof.log`
- Batch78.1 combat core / modal / skin sanity: `PASS`, `unity-batch82-combat-proof.log`

## Performance And Scope Notes

- No ResultPipeline rebuild was added to UI or per-frame paths.
- No route content, combat mechanic, inventory system, economy redesign, scene migration, UI skin, or layout work was added.
- The new proof reads existing cached/read-model surfaces and uses existing explicit transition points: result writeback, prep open, route selection, and launch confirmation.

## Remaining Weakness

The loop is repeatable at the contract/proof level, but the long-term player motivation after two or more runs still depends on future content pacing, recovery tuning, and manual feel QA. Batch82 proves the loop can continue; it does not yet make the third/fourth run strategically rich.

## Recommended Next Batch

Run a manual two-run feel QA pass, then pick a narrow Batch83 focused on run-to-run motivation: recovery pacing, route risk appetite, or compact second-run player prompts.
