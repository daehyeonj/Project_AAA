# Batch 79 Route Operating Scenario Proof

## Selected Branch

- Selected branch: `A - existing route meaning authoring was present but not visible enough`
- Chosen route pair: `city-a -> dungeon-alpha safe/risky`
- Reason: Dungeon Alpha is still the shortest manual demo path, and its safe/risky route meanings already contain scenario label, choose-when, party-fit, combat-plan, reward, risk, and follow-up text.
- Scope kept: no new city, dungeon, route, combat mechanic, RPG system, economy system, scene migration, or sprite/skin work.

## Scenario Source Text

- Alpha safe route: `route-steady-rest`
- Alpha safe scenario: `Stability Run`
- Alpha safe combat plan: `Expect slime-heavy sustain fights, stronger shrine value, and a steadier elite setup.`
- Alpha risky route: `route-balanced-volatile`
- Alpha risky scenario: `Surge Window`
- Alpha risky combat plan: `Expect mixed slime-goblin pressure, a sharper mid-run spike, and a burst-focused elite finish.`

## Player-Facing Readbacks

- Expedition Prep route option: route card now includes compact `Combat Plan` text from the existing route meaning rail.
- Launch/commit summary: selected route summaries preserve scenario label, choose-when, party fit, reward, risk, follow-up, and battle watch text from the existing prep/start context.
- Dungeon run readback: explore sidebar keeps `Route Plan`, `Route Risk`, `Battle Watch`, and `Next Goal` visible during the run.
- Encounter result popover: battle result popover now has one compact `Route Plan` line.
- Final run result/readback: result panel now has `Scenario Payoff`, deriving clear/retreat/defeat/checked status plus route/follow-up evidence.

## Static Producer-Consumer Proof

- Route option data carries `EventPreviewText` through `ExpeditionPrepRouteOptionData`.
- Prep surface fills it through `BuildRouteCombatPlanEntryText(dungeonId, routeId)`.
- Prep card renders it as `Combat Plan`.
- Battle result popover contract carries `RoutePlanText`.
- Encounter result popover fills `RoutePlanText` with `BuildSelectedRouteScenarioPlanText()`.
- Run result popover fills `RoutePlanText` with `BuildSelectedRouteScenarioPayoffText(outcomeKey)`.
- Dungeon result panel renders `Scenario Payoff` through `BuildDungeonScenarioPayoffLine(resultContext)`.

## Validation

- Compile: `PASS`
- Compile command: Unity batchmode against `C:\Users\umyo\Documents\Project_AAA`
- Compile log: `unity-merge-validate.log`
- Compile markers:
  - `*** Tundra build success`
  - `Batchmode quit successfully invoked - shutting down!`
  - `Exiting batchmode successfully now!`
- Authoring validation: `PASS`
- Authoring log: `unity-batch79-authoring.log`
- Authoring markers:
  - `PassCount=4`
  - `WarnCount=0`
  - `FailCount=0`
  - Alpha safe chain resolves `route-steady-rest`, `outcome-mana-shard-city-a`, `city-decision-city-a-shard-stability`, `encounter-profile-alpha-safe-entry`, and `battle-setup-alpha-safe-room1`.
  - Alpha risky chain resolves `route-balanced-volatile`, `outcome-mana-shard-city-a-surge`, `city-decision-city-a-shard-surge-window`, `encounter-profile-alpha-risky-breach`, and `battle-setup-alpha-risky-room1`.
- Batch10 smoke: `PARTIAL`
- Batch10 smoke log: `unity-batch79-smoke.log`
- Batch10 smoke proof before timeout:
  - Boot -> MainMenu: `PASS`
  - MainMenu -> WorldSim: `PASS`
  - WorldSim -> CityHub: `PASS`
  - CityHub -> ExpeditionPrep: `PASS`
  - ExpeditionPrep -> DungeonRun launch: `PASS`, route `safe`, risk preview `Rest Path | Stability Run | Low Risk`
  - DungeonRun -> BattleScene intent carry-through: `PASS`
  - DungeonRun -> BattleScene -> DungeonRun return: `PASS`, outcome `encounter_victory`, encounter `Slime Front`, next goal `Reach Elite`
- Batch10 smoke blocker:
  - `FAIL :: Smoke runner :: Timed out while waiting for step ResolveCoreLoop`
  - The timeout happened after the first encounter return, so this batch does not claim a full automatic core-loop smoke pass.

## Manual Repro

1. Open `Assets/Scenes/SampleScene.unity`.
2. Enter the world simulation from the main menu.
3. Select `city-a`.
4. Open Expedition Prep for `dungeon-alpha`.
5. Compare safe/risky route cards and confirm each card reads as a scenario, including scenario label, choose-when, party fit, reward/risk/follow-up, and `Combat Plan`.
6. Commit the safe route and launch the run.
7. Confirm the dungeon run readback keeps `Route Plan`, `Route Risk`, `Battle Watch`, and `Next Goal` visible.
8. Clear the first encounter, `Slime Front`.
9. Confirm the battle result popover includes one compact `Route Plan` line.
10. Finish or resolve the run and confirm the result panel includes `Scenario Payoff`.

## Regression Notes

- 78.1 combat payoff logic: not changed.
- RPG/role truth: not changed.
- Inventory modal target-status guard: not changed.
- Runtime skin bridge: not changed.
- Pending Run Spoils / extraction policy: not changed.
- World selection cache/performance rail: not changed.
- UI shape changed?: compact text additions only; no screen redesign.

## Performance Notes

- Route scenario text is built through existing surface and result/popup build points.
- No new `Update` loop, per-frame asset loading, Resources scan, or world selection cache rebuild was added.
- The added UI fields are copied through existing snapshot/contract paths rather than introducing a parallel scenario content system.
