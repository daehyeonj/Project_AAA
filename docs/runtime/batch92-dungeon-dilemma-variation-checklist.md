# Batch 92 Dungeon Dilemma Variation Checklist

## Scope

- Runtime baseline remains `grid dungeon` plus `standard JRPG battle`.
- Focus route: `city-a -> dungeon-alpha`.
- Dilemma rooms:
  - Stability / Rest Path: `Rest Shrine`
  - Surge / Standard Path: `Greed Cache`

## Automated Proof

- Compile: `unity-batch92-compile.log`
- Targeted proof: `unity-batch92-dilemma-proof.log`
- Smoke: `unity-batch92-smoke.log`
- Open-path regression: `unity-batch92-regression-batch89.log`
- Encounter-feel regression: `unity-batch92-regression-batch91.log`

## Manual Feel QA

- On Rest Path, clear `Slime Front`, stand on `Rest Shrine`, and confirm the prompt explains both `[E]` use and moving on to skip.
- Use `Rest Shrine` and confirm the next `Watch Hall` battle/readback says `Shrine Protection`.
- In a separate Rest Path check, move past `Rest Shrine` without using it and confirm the next beat says no `Shrine Protection`.
- On Standard Path, clear `Mixed Front`, stand on `Greed Cache`, and confirm the prompt explains `+3 mana_shard + Cache Pressure` versus skip.
- Move past `Greed Cache` without using it and confirm carried/chest loot does not gain +3, `Goblin Pair Hall` says no `Cache Pressure`, and the result/world board says `Returned mana_shard x17`.
- In a separate Standard Path check, open `Greed Cache` and confirm the existing path still returns `mana_shard x20` with `Cache Pressure`.

## Stop Conditions

- Stop if skip text invents damage, fatigue, hidden stat penalties, or a ResultPipeline-only consequence.
- Stop if Greed Cache skip still grants +3 mana_shard.
- Stop if Greed Cache open no longer grants +3 mana_shard / Cache Pressure.
- Stop if route-defining rooms can be triggered while a battle/result modal is active.

## Manual Status

- Manual screenshot/play QA: `NOT RUN` in Batch92 close-out.
- Automated proof covers runtime branches and result/world readback, not subjective choice feel.
