# Batch 83 Second-Run Decision Pressure v1

## Selected Branch

Branch A: the second ExpeditionPrep already receives the first run's changed city/party state, but the player-facing prompt did not yet make the next choice feel desirable.

This batch does not add a new system, route, combat rule, economy rule, layout, skin, or ResultPipeline contract. It turns existing first-run result, party carry-forward, launch warning, and route-option data into compact second-run decision readbacks.

## Player Question

After the first run, the second ExpeditionPrep should make the player think:

- First run changed the city and party.
- I can stabilize one more run and protect the dispatch rhythm.
- Or I can trust the growth/equipment edge and chase a bigger payout.
- Launch is possible, but recovery/fatigue still matters.

## Readback Shape

The prep surface now exposes compact display-only fields:

- `LastRunCarryForwardText`: `Returned mana_shard x16 | Party Stable`
- `PartyGrowthCarryForwardText`: `Alden +16 XP | Rune equipped Stormglass Focus`
- `StabilityAppetiteText`: `protect HP and keep dispatch rhythm steady`
- `SurgeAppetiteText`: `chase payout while Rune's new focus improves burst`
- `LaunchRiskAdviceText`: `Ready with warning / recovery risk / fatigue`
- `RouteAppetiteRecommendationText`: `Stability if recovery matters, Surge if stock pressure still outweighs strain`

The route cards also append a short appetite clause:

- Stability route: protect HP and rhythm.
- Surge route: chase payout with Rune's new burst edge.

## Scope Guard

- Source data remains owned by ResultPipeline, WorldSim/CityHub, party/loadout state, LaunchReadiness, and route meaning surfaces.
- `ExpeditionPrepSurfaceData` only caches player-facing display readbacks.
- `PrototypePresentationShell` only consumes the new strings in existing panels.
- `Batch82RepeatCoreLoopProofRunner.RunBatch83SecondRunDecisionPressureProof` reuses the repeat-loop path and asserts the new readbacks in second ExpeditionPrep.

## Validation

- Compile: `PASS`, `unity-batch83-compile.log`
- Targeted second-run decision pressure proof: `PASS`, `unity-batch83-two-run-feel-proof.log`
- Proof marker: `PASS :: Second ExpeditionPrep context` with `LastRun`, `CarryForward`, `Stability`, `Surge`, `LaunchRisk`, and `AppetiteRecommendation`
- Batch10 smoke: `PASS`, `unity-batch83-smoke.log`
- Static whitespace check: `PASS`, `git diff --check` reported line-ending warnings only

## Manual Feel QA

Use `docs/ui/two-run-feel-qa-checklist.md` for the human screenshot/playback pass. Automated proof can confirm the readbacks exist; manual QA still decides whether the second Prep actually makes the next click tempting.
