# Batch 81 Presentation Vertical Slice Lock

## Selected Branch

`Branch A` - the accepted Alpha Stability Run loop already works. Batch81 locks the presentation path with a playbook and proof note instead of adding a new system.

## Chosen Demo Path

- World pressure: select `City A`.
- Route operating scenario: `city-a -> dungeon-alpha -> safe`.
- Scenario label: `Stability Run`.
- First combat: `Slime Front`.
- Combat payoff: Alden opens `Burst Window`; Mira cashes it.
- Result: `mana_shard x16`, party stable.
- World board: `Latest / Why / Changed / Next / Ready / Route`.
- Re-entry: open ExpeditionPrep again and show route options.

## Proof Composition

No new large proof runner was added. Existing targeted rails already cover the loop:

- Batch10 smoke covers main flow, result writeback, world board refresh, and re-entry.
- Batch79.1 route proof covers route card, launch scenario readback, dungeon readback, and encounter popover route plan.
- Batch78.1 combat proof covers enemy intent, Burst Window setup, Mira payoff, and UI/modal/skin sanity.
- Batch80.1 board proof covers the compact selected CityHub pressure board and re-entry readiness.
- Batch79.2 packaging proof covers ResultPipeline intent/result packaging.
- Batch79.3 re-entry proof covers post-result ExpeditionPrep continuity.

## Current Representative Proof Lines

- Route card: `Rest Path | Stability Run | Choose when: Choose it when City A needs a clean shard refill...`
- Combat plan: `Combat: Expect slime-heavy sustain fights, stronger shrine value, and a steadier elite setup`
- Setup log: `Alden read intent and opened Burst Window on Slime A (+3 for 2 ally actions).`
- Payoff log: `Mira cashed Burst Window on Slime A for +3.`
- Encounter popover: `Rest Path | Stability Run | Low Risk | Combat: Expect slime-heavy sustain fights...`
- World board latest: `Cleared: Stability Run | Returned mana_shard x16 | Party Stable`
- World board changed: `Stock +16 mana_shard | Pressure Urgent -> Stable | Readiness Ready -> Recovering | Streak 0 -> 1`
- World board next: `Stabilize for 1 day before the next push.`
- World board ready: `Ready: warning | recovery 1 day | party idle | route available`
- Re-entry: `RouteOptions=2`

## Validation

- Compile: `PASS`, log `unity-batch81-compile.log`
- Batch10 smoke: `PASS`, log `unity-batch81-smoke.log`
- Route scenario proof: `PASS`, log `unity-batch81-route-proof.log`
- Combat proof: `PASS`, log `unity-batch81-combat-proof.log`
- Board readability proof: `PASS`, log `unity-batch81-board-proof.log`
- Packaging proof: `PASS`, log `unity-batch81-packaging-proof.log`
- Re-entry proof: `PASS`, log `unity-batch81-reentry-proof.log`

## Performance / Scope

- No gameplay systems were added.
- No ResultPipeline or major contract refactor was added.
- No UI skin/layout work was added.
- No scene migration was added.
- No per-frame ResultPipeline rebuild, city-selection heavy rebuild, asset scan, or inventory scan was added.

## Presenter Artifact

- Playbook: `docs/demo/project-aaa-vertical-slice-playbook.md`
- Manual screenshot checklist remains: `docs/ui/cityhub-pressure-board-ux-checklist.md`

## Decision

Presentation recording can begin after one manual screenshot/recording sanity pass confirms no clipping at the target resolution.
