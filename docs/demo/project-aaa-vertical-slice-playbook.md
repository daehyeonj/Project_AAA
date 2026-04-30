# Project AAA Vertical Slice Playbook

## Demo Goal

Show how city pressure drives route choice, route choice shapes dungeon/combat plan, combat payoff creates reward, and the result returns to a world pressure board that tells the player what changed and what to do next.

## Demo Setup

- Scene: open `Assets/Scenes/SampleScene.unity`.
- Path: `city-a -> dungeon-alpha -> safe`.
- Scenario: `Stability Run`.
- First combat: `Slime Front`.
- PrototypeDebugHUD: keep the debug overlay hidden on World/CityHub unless a diagnostic is needed. The accepted battle HUD rail still appears during battle.
- UI skin state: runtime skin bridge/fallback state is acceptable for this demo; do not present it as final art.
- Party state: if no idle party is available, recruit from CityHub before opening ExpeditionPrep.
- Performance notes: avoid opening inventory or debug overlays during the presentation unless demonstrating those systems. The demo path should not require repeated no-op city selection.

## Step-By-Step Flow

1. Start at MainMenu and enter World.
2. Select City A.
3. Point out the pressure board: city problem, linked dungeon, and next action pressure.
4. Open ExpeditionPrep.
5. Select `Rest Path` / `Stability Run`.
6. Explain the route plan: safer shard return, recovery-friendly combat, steadier elite setup.
7. Launch DungeonRun.
8. Enter the first battle, `Slime Front`.
9. Show enemy intent, then use Alden to open `Burst Window` and Mira to cash the payoff.
10. Close the encounter popover after calling out the route-plan readback.
11. Finish/return the result. For a short recording, use the existing proof/smoke path to simulate the run clear after the first proof beat.
12. Show the World/CityHub board: `Latest`, `Why`, `Changed`, `Next`, `Ready`, and `Route`.
13. Re-open ExpeditionPrep to prove the result-pressure loop can re-enter route choice with coherent options.

## Expected Player-Facing Lines

- Latest: `Cleared: Stability Run | Returned mana_shard x16 | Party Stable`
- Why: `Dispatch readiness is Recovering. Wait 1 day(s) for full recovery.`
- Changed: `Stock +16 mana_shard | Pressure Urgent -> Stable | Readiness Ready -> Recovering | Streak 0 -> 1`
- Next: `Stabilize for 1 day before the next push.`
- Ready: `Ready: warning | recovery 1 day | party idle | route available`
- Route: `Rest Path | Stability Run | Low Risk` or `Stabilize for 1 day before the next push.`
- Combat preview: `Combat: Expect slime-heavy sustain fights, stronger shrine value, and a steadier elite setup`
- Combat log: `Alden read intent and opened Burst Window on Slime A (+3 for 2 ally actions).`
- Combat payoff: `Mira cashed Burst Window on Slime A for +3.`
- Encounter popover: `Rest Path | Stability Run | Low Risk | Combat: Expect slime-heavy sustain fights... | Follow-up: Usually leaves the next dispatch cleaner...`
- Scenario payoff: final result/readback should connect `Rest Path | Stability Run | Low Risk` to the mana_shard return and City A pressure response.

## What Not To Click / Known Limitations

- Do not widen the demo to Beta unless Alpha is broken.
- Do not open inventory during the main recording unless a separate inventory/modal proof is being shown.
- Do not toggle the debug overlay during the World/CityHub pressure-board explanation.
- Full manual dungeon clear can take longer than the presenter beat; it is acceptable to show the first battle payoff and then use the proof/smoke path for result return.
- Runtime skin/fallback visuals are acceptable prototype presentation, not final UI art.
- The post-return board may show recovery warning; that is expected and should be framed as "ready with warning" rather than a failed loop.

## Fallback If Something Fails

- If inventory overlay gets in the way, close it before continuing.
- If route entry is blocked, recruit a party first, then retry ExpeditionPrep.
- If recovery blocks immediate re-entry, call out the `Ready:` or blocked line and advance/recover as needed.
- If the first battle payoff is missed, restart the first encounter path and reproduce: Alden `Power Strike` opens `Burst Window`; Mira `Weak Point` cashes it.
- If selected CityHub text looks dense or clipped, capture the screenshot against `docs/ui/cityhub-pressure-board-ux-checklist.md` rather than changing systems mid-demo.
