# Project AAA Post-Slice Baseline

## Runtime Reality Lock

- Current runtime reality remains `grid dungeon` exploration plus `standard JRPG battle`.
- Batch 22 is the first post-slice extension batch, but baseline behavior must stay functionally aligned with the Ready golden path.
- Do not treat older panel/lane combat ideas as the current implementation target in this phase.

## Golden Path Baseline

`MainMenu -> WorldSim -> CityHub -> ExpeditionPrep -> DungeonRun -> BattleScene -> DungeonRun -> ResultPipeline -> WorldSim -> CityHub -> ExpeditionPrep`

- The baseline expectation is still a readable city problem -> expedition -> dungeon/battle -> result -> world return -> next expedition loop.
- Post-slice work may add authoring data and expansion seams, but should not silently rewrite this loop.

## Representative Chain Inventory

Batch 22 locked the baseline with representative chain #1, and batch 23 proves the same authoring seam can carry representative chain #2 without rewriting the loop:

### Chain #1 baseline

- City-side problem: `city-a` mana-shard pressure / next-dispatch stability
- Expedition purpose: launch `dungeon-alpha` to bring back `mana_shard` for `City A`
- Linked dungeon identity: `Dungeon Alpha`
- Representative route: `Rest Path` (`safe`)
- Representative encounter/battle setup: `Slime Front -> Rest Shrine -> Watch Hall -> Supply Cache -> Quiet Antechamber -> Elite Chamber`
- Representative reward / outcome meaning: `mana_shard` return that keeps `Dungeon Alpha` readable as the main expedition lever for City A

### Chain #2 authoring proof

- City-side problem: `city-b` refined-mana processing stall caused by missing `mana_shard`
- Expedition purpose: launch `dungeon-beta` to recover `mana_shard` so City B can reopen processing
- Linked dungeon identity: `Dungeon Beta`
- Representative route: `Greedy Path` (`risky`)
- Representative encounter/battle setup: `Raider Gate -> Plunder Cache -> Ambush Hall -> War Banner Shrine -> War Table -> Elite Chamber`
- Representative reward / outcome meaning: `mana_shard` return that reopens blocked processing lines and makes Beta the higher-risk payout answer for City B

### Chain #3 reusable-rail proof

- City-side problem: `city-a` mana-shard pressure is already understood, but the city can convert a brief relief window into a sharper follow-up push
- Expedition purpose: launch `dungeon-alpha` through the `risky` route to spike `mana_shard` stock before the next dispatch window tightens again
- Linked dungeon identity: `Dungeon Alpha`
- Representative route: `Standard Path` (`risky`)
- Representative encounter/battle setup: `Mixed Front -> Greed Cache -> Goblin Pair Hall -> Unstable Shrine -> Core Threshold -> Elite Chamber`
- Representative reward / outcome meaning: a larger `mana_shard` return for City A that buys more buffer than the Rest Path, but leaves a rougher recovery window behind
- Meaningful difference from #1 / #2: same city+dungeon as #1 but a different route/reward emphasis, different encounter/battle chain, and different outcome/economy meaning; different city-side bottleneck, dungeon identity, and recommendation context from #2

## Canonical Data Path vs Fallback

- Canonical data-driven path in batch 22:
  - `Assets/_Game/Resources/Content/GoldenPathChains/city-a-dungeon-alpha-rest-path.json`
  - `Assets/_Game/Resources/Content/GoldenPathChains/city-b-dungeon-beta-greedy-path.json`
  - `Assets/_Game/Resources/Content/GoldenPathChains/city-b-dungeon-beta-guarded-path.json`
  - `Assets/_Game/Resources/Content/GoldenPathChains/city-a-dungeon-alpha-risky-path.json`
  - `Assets/_Game/Resources/Content/GoldenPathRouteMeanings/*.json`
  - `Assets/_Game/Resources/Content/GoldenPathOutcomeMeanings/*.json`
  - `Assets/_Game/Resources/Content/GoldenPathCityDecisionMeanings/*.json`
  - `Assets/_Game/Resources/Content/GoldenPathEncounterProfiles/*.json`
  - `Assets/_Game/Resources/Content/GoldenPathBattleSetups/*.json`
  - consumed through `GoldenPathContentRegistry`
  - used to seed representative chain objective/usefulness for `city-a -> dungeon-alpha` and `city-b -> dungeon-beta`
  - used to seed the canonical route identity + room sequence for `dungeon-alpha / safe`, `dungeon-alpha / risky`, `dungeon-beta / safe`, and `dungeon-beta / risky`
  - batch 24 shared authoring split:
    - route risk / reward-bias / impact meaning now come from shared route-meaning assets
    - reward usefulness / city-impact meaning now come from shared outcome-meaning assets
  - batch 25 shared authoring split:
    - encounter-role / mission-relevance context now comes from shared encounter-profile assets
    - battle-entry enemy-group / setup meaning now comes from shared battle-setup assets
  - batch 30 supported route close-out:
    - `city-b -> dungeon-beta -> safe` now resolves through a non-primary route-variant chain asset plus dedicated shared route/outcome/battle setup assets
- Current fallback path:
  - hardcoded dungeon/route/encounter templates in `StaticPlaceholderWorldView.DungeonRun.cs`
  - still used for prototype-only routes or city/dungeon pairs that do not have a JSON asset under `Resources/Content/GoldenPathChains`
  - no longer used for the surfaced Alpha/Beta safe+risky route variants
  - still used for live monster stat tuning and non-canonical encounter rosters that have not been promoted into the representative content path yet
  - broader economy formulas still remain runtime-owned even though batch 26 promotes shared outcome meaning into the result/world/city interpretation path

## Surfaced Opportunity Matrix

- Currently surfaced city-side opportunities:
  - `city-a -> dungeon-alpha -> safe` as the primary City A expedition lever
  - `city-a -> dungeon-alpha -> risky` as the surfaced high-volatility follow-up option for the same City A shard window
  - `city-b -> dungeon-beta -> risky` as the primary City B expedition lever
  - `city-b -> dungeon-beta -> safe` as the first surfaced non-primary route variant (`Guarded Path`)
- Canonical but not yet surfaced:
  - None in the current authored Alpha/Beta route-variant set
- Batch 31 surfacing rule:
  - primary chains surface by default through `TryGetChain(cityId, dungeonId)`
  - non-primary route variants surface only when their chain asset sets `SurfaceAsOpportunityVariant=true`
  - surfaced route variants must still resolve through `TryGetChainForRoute(...)` / `TryGetCanonicalRoute(...)`, not through chain-specific runtime branches
- Batch 32 surfaced-matrix note:
  - `city-a -> dungeon-alpha -> risky` now joins the player-facing matrix through the same route-variant surfacing flag used by `city-b -> dungeon-beta -> safe`
  - the current surfaced matrix now proves two non-primary route variants can reach CityHub/ExpeditionPrep without special-case runtime branching
- Batch 34 consumer-proof note:
  - `city-b -> dungeon-beta -> safe` is now audited as both canonical and actually consumer-visible
  - the surfaced-matrix tooling now reports asset intent (`Surface=`) separately from actual decision/prep visibility (`Consumer=`), so surfaced promotion work can prove the route reached CityHub/ExpeditionPrep instead of only proving the asset flag exists
- Batch 35 tooling note:
  - surfaced-matrix tooling now also reports explicit status buckets (`PASS:fully-canonical-and-surfaced`, `WARN:canonical-but-not-surfaced`, `WARN:surfaced-with-chain-override`, `FAIL:surfaced-using-fallback`, `FAIL:surfaced-consumer-mismatch`)
  - representative/surfaced routes can now be reviewed through a compact `Representative chain status` summary without changing runtime behavior or requiring anything beyond workspace-local editor tooling
- Batch 36 surfaced-portfolio note:
  - the current authored Alpha/Beta route set is now treated as an explicit surfaced portfolio rather than an implied collection of surfaced rows
  - tooling now reports whether surfaced routes are actually linked into `ExpeditionPrepReadModel.LinkedOpportunities` and whether city-side rationale plus prep recommendation text are connected for the surfaced route's city+dungeon prep state
  - the current surfaced portfolio is `city-a -> dungeon-alpha -> safe`, `city-a -> dungeon-alpha -> risky`, `city-b -> dungeon-beta -> risky`, and `city-b -> dungeon-beta -> safe`
- Batch 37 surfaced-maturity note:
  - the current surfaced portfolio remains the same four Alpha/Beta route variants, but tooling now reports `MeaningfullyDistinctSurfaced=4` so surfaced-maturity audits can distinguish “fully canonical” from “actually varied enough to stop promoting routes for now”
  - surfaced-only trace output now joins CityHub rationale, ExpeditionPrep linkage, recommendation text, and shared-definition ids for each surfaced route without changing runtime selection behavior
- Batch 38 tooling-deepening note:
  - surfaced summaries now also report `Issues=` so operators can spot source fallback, consumer fallback, prep-link drift, recommendation drift, and meaningful chain-local override pressure without manually diffing multiple logs
  - representative/surfaced reference tooling now also reports `SharedAssetPaths=` and can quick-open the currently surfaced chain + shared assets in one selection pass
- Batch 39 throughput note:
  - authoring draft scaffolds now have an editor-only helper that copies a selected canonical chain into `Assets/_Game/AuthoringDrafts/GoldenPathChains/`
  - draft scaffolds intentionally stay outside the canonical `Resources` path, keep surfaced flags off, and preserve copied shared references so authors can start from a safe baseline instead of hand-assembling a new chain JSON from scratch
- Batch 40 helper-backed promotion note:
  - the current surfaced Alpha/Beta matrix still occupies the supported `safe/risky` player-facing slots for the two linked city+dungeon pairs
  - draft helper output is now cross-checked against the canonical resolver key so authors can see whether a draft is truly promotable or just a hidden collision with an already-surfaced route
  - the current draft-template example remains intentionally non-promotable as-is because `city-a -> dungeon-alpha -> safe` is already owned by the baseline canonical chain
- Batch 41 helper usability note:
  - the draft helper can now quick-open the blocked draft, the colliding canonical owner, and the linked shared assets in one step
  - this keeps the surfaced portfolio unchanged while making the draft helper usable for actual compare-and-retarget authoring work instead of only readiness inspection
- Batch 43 surfaced-portfolio alignment note:
  - no concrete batch-42 surfaced promotion artifact was found in the repo, so batch 43 treated the current Alpha/Beta surfaced set as the authoritative player-facing portfolio
  - the portfolio still remains `city-a -> dungeon-alpha -> safe`, `city-a -> dungeon-alpha -> risky`, `city-b -> dungeon-beta -> risky`, and `city-b -> dungeon-beta -> safe`
  - validator wording is now aligned with the surfaced-matrix wording, so primary chains show `cityhub+prep(primary)` instead of looking hidden while the surfaced portfolio is already full
- Batch 44 surfaced-expansion gate note:
  - the batch-43 target `city-b -> dungeon-beta -> safe` remains canonical, surfaced, prep-linked, and recommendation-linked
  - the current surfaced matrix remains healthy: no canonical-but-not-surfaced Alpha/Beta route, no surfaced fallback, and no surfaced consumer mismatch
  - tooling now exposes that audit as an explicit gate summary instead of forcing operators to combine the portfolio, representative-status, and matrix logs by hand
  - current expected gate state on the Alpha/Beta rail is `C:tooling-next`
- Batch 45 draft-promotion gate note:
  - the current surfaced Alpha/Beta rail still has no canonical-but-not-surfaced supported route on the existing `safe/risky` seam
  - draft readiness now echoes the surfaced-expansion gate and blocked-draft cause counts, so authors can tell whether they need a new resolver key, a wider surfaced route seam, or only manual content review
  - treat `PromotionRecommendation=retarget-the-draft-resolver-key-before-promotion` plus `ExpansionGate=C:tooling-next` as proof that the current helper is usable but the current surfaced rail is already saturated for that draft as written

## Baseline Compatibility Note

- Ready golden-path behavior is still anchored on the original `city-a -> dungeon-alpha -> safe` loop.
- Batch 23 does not replace that baseline; it proves the same authoring seam can carry a second, meaningfully different chain without widening the runtime surface or adding chain-specific code branches.
- Batch 24 keeps that behavior intact while moving shared route/reward/impact meaning into reusable assets; mission objective, dungeon identity, and encounter naming remain chain-specific.
- Batch 25 keeps battle rules and the JRPG surface intact while moving representative encounter/battle meaning into reusable assets; room display names and live combat stats may still stay chain-specific or fallback-driven where needed.
- Batch 26 keeps world application logic intact while moving shared outcome meaning further down the canonical return chain: `ExpeditionOutcome -> WorldDelta -> OutcomeReadback -> ExpeditionResultReadModel -> CityDecisionReadModel` now carries shared city-impact plus recommendation-shift text for the representative chains.
- Batch 27 keeps CityHub/ExpeditionPrep behavior intact while moving representative city-side bottleneck/opportunity/recommendation rationale into reusable city-decision meaning assets; runtime shortage math and recommendation ordering still remain CityHub-owned.
- Batch 28 keeps the Ready baseline intact while proving the same authoring rail can carry a second representative route for the same city+dungeon pair; `TryGetChain(cityId, dungeonId)` remains the primary city-side lookup, while route-aware consumers now resolve canonical content by `cityId + dungeonId + routeId` before falling back.
- Batch 29 keeps gameplay behavior intact while adding a lightweight editor validator for representative authoring integrity; use it to spot missing shared references, visible fallback usage, and route-variant ownership drift before adding chain #4.
- Batch 30 keeps gameplay behavior intact while promoting the surfaced `city-b -> dungeon-beta -> safe` variant onto the canonical authoring rail, so the validator no longer treats that route as a tolerated fallback warning.
- Batch 31 keeps gameplay behavior intact while surfacing `city-b -> dungeon-beta -> safe` in the CityHub opportunity matrix through the same canonical authoring rail; route-variant surfacing is now data-driven instead of implicit fallback drift.
- Batch 32 keeps gameplay behavior intact while surfacing `city-a -> dungeon-alpha -> risky` through the same authoring seam, proving surfaced opportunity variety can expand without new runtime branches.
- Batch 33 keeps gameplay behavior intact while adding editor-only authoring helpers for surfaced-matrix inspection and quick asset opening; the new tooling is meant to speed up content work, not change runtime selection rules.
- Batch 34 keeps gameplay behavior intact while extending the surfaced-matrix tooling to prove consumer visibility for canonicalized opportunities; `city-b -> dungeon-beta -> safe` is now called out as canonical plus `cityhub+prep(actual)` instead of only being implied by authoring flags.
- Batch 35 keeps gameplay behavior intact while adding explicit surfaced/canonical status labels plus a compact representative-chain status summary, so post-slice audits can spot fallback, hidden-canonical, and override pressure without widening the runtime surface.
- Batch 36 keeps gameplay behavior intact while officializing the current surfaced opportunity portfolio and extending editor tooling to show prep-linkage plus recommendation-linkage for surfaced routes; this is a portfolio audit improvement, not a runtime content-surface rewrite.
- Batch 37 keeps gameplay behavior intact while deepening surfaced-portfolio tooling: the new trace summary and surfaced-distinctness count help audit whether the current player-facing set is both canonical and meaningfully varied, without widening the runtime surface.
- Batch 38 keeps gameplay behavior intact while deepening authoring operations visibility: the new issue drill-down and reference-path tooling make surfaced/canonical/fallback/override state faster to inspect, but do not change runtime opportunity selection or battle/result behavior.
- Batch 39 keeps gameplay behavior intact while improving authoring throughput: draft scaffolds are generated outside the canonical runtime path, so they speed up content setup without changing surfaced opportunities, dungeon flow, or battle/result behavior.
- Batch 40 keeps gameplay behavior intact while making the draft helper more operationally honest: draft promotion readiness now explains whether a hidden draft can join the current canonical/surfaced rail or whether the authored surface is already saturated for that route key.
- Batch 41 keeps gameplay behavior intact while making the draft helper more practical for day-to-day authoring: blocked drafts now have an explicit comparison context instead of requiring manual asset hunting.
- Batch 43 keeps gameplay behavior intact while aligning validator wording with the surfaced portfolio reality: the current four Alpha/Beta routes are still the official surfaced set, but primary chains no longer read as `hidden` in validation output.
- Batch 44 keeps gameplay behavior intact while adding an explicit surfaced-expansion gate summary: it does not widen the player-facing set, but it makes the next-step decision readable from one tooling pass instead of a manual three-log comparison.
- Batch 45 keeps gameplay behavior intact while aligning draft-promotion tooling with the surfaced-expansion gate: it does not widen the player-facing set, but it makes "no open surfaced slot on the current rail" explicit before someone tries to promote a colliding draft.

## Batch 22 Guardrail

- If the representative chain changes, update the content asset and keep smoke green before broadening content scope.
- If a new dungeon or route is added later, prefer another additive content definition plus fallback adapter instead of copying more hardcoded template blocks first.
