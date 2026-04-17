# Golden-Path Content Authoring

## What Batch 69 Adds

- The world/city presentation now leans on the existing world-board + city-decision seams to read like a result-pressure board instead of a neutral bridge.
- Current world-facing summaries now surface, in one decision context:
  - priority city / urgency
  - why the city matters now
  - route answer
  - recent-result evidence
  - party readiness / recovery pressure
  - next action follow-through
- The selected city world source snapshot now carries explicit pressure-board framing instead of only loose status text.
- The world alert ribbon and city/world summary bodies now reuse that framing so recent expedition results look like evidence for the next recommendation, not dead-end logs.

## What Batch 67 Adds

- The battle presentation core is now explicitly:
  - `read intent -> create window -> cash payoff`
- Enemy intent is no longer only an enemy-turn telegraph; living enemies now refresh a preview readback during party turns so current target/action context has something real to read.
- Current role skills now map to one readable battle loop instead of only isolated numbers:
  - `Power Strike`: opens the burst window
  - `Weak Point`: consumes the burst window
  - `Arcane Burst`: punishes exposed targets in the sweep
  - `Radiant Hymn`: extends active windows while healing
- The accepted battle HUD structure stays the same, but setup/payoff state now appears through:
  - enemy state labels
  - enemy trait text
  - command effect/context text
  - target focus rule text

## What Batch 66 Adds

- The surfaced four-route Alpha/Beta portfolio stays unchanged:
  - `city-a -> dungeon-alpha -> safe`
  - `city-a -> dungeon-alpha -> risky`
  - `city-b -> dungeon-beta -> risky`
  - `city-b -> dungeon-beta -> safe`
- Route-facing readback now resolves through a shared runtime party seam instead of a local placeholder rebuild.
- Current party differentiation now has three runtime archetypes:
  - `Bulwark Crew`
  - `Outrider Cell`
  - `Salvager Wing`
- Current party growth now has one lightweight promotion axis:
  - `Recruit Frame -> Field Promotion -> Elite Promotion`
- Dispatch / ExpeditionPrep now surface:
  - current party strength
  - route fit
  - promotion state
  - derived power / carry context
- Battle now consumes the same promotion-aware party resolve path, so the surfaced route choice, prep readback, and battle stats no longer drift apart.

## What Batch 22 Added

- Representative chain asset: `Assets/_Game/Resources/Content/GoldenPathChains/city-a-dungeon-alpha-rest-path.json`
- Runtime loader: `Assets/_Game/Scripts/Dungeon/GoldenPathContentDefinitions.cs`
- Dungeon adapter/fallback seam: `Assets/_Game/Scripts/Dungeon/StaticPlaceholderWorldView.DungeonContentData.cs`

## What Batch 23 Proved

- Second representative chain asset: `Assets/_Game/Resources/Content/GoldenPathChains/city-b-dungeon-beta-greedy-path.json`
- The same registry + adapter path can now carry:
  - `city-a -> dungeon-alpha -> safe`
  - `city-b -> dungeon-beta -> risky`
- This keeps the batch 22 seam from collapsing into a one-off sample for only one city, one dungeon, or one safe-route case.

## What Batch 24 Shared

- Shared route meaning assets: `Assets/_Game/Resources/Content/GoldenPathRouteMeanings/*.json`
- Shared outcome meaning assets: `Assets/_Game/Resources/Content/GoldenPathOutcomeMeanings/*.json`
- The current split is:
  - chain-specific: mission objective, bottleneck summary, dungeon identity, encounter preview, room sequence
  - shared: route risk/reward-bias meaning, reward usefulness meaning, city-impact meaning

## What Batch 25 Shared

- Shared encounter profile assets: `Assets/_Game/Resources/Content/GoldenPathEncounterProfiles/*.json`
- Shared battle setup assets: `Assets/_Game/Resources/Content/GoldenPathBattleSetups/*.json`
- The current split is:
  - chain-specific: city problem, dungeon identity, room display names, canonical route selection
  - shared: encounter-role tags, mission-relevance battle context, enemy-group identity, battle-entry setup meaning

## What Batch 26 Shared

- Shared outcome meaning assets now also carry city-side recommendation-shift meaning for the representative chains.
- The current split is:
  - chain-specific: mission objective, dungeon identity, room sequence, runtime result application math
  - shared: reward usefulness meaning, city-impact interpretation, recommendation-shift reason text

## What Batch 27 Shared

- Shared city-side meaning assets: `Assets/_Game/Resources/Content/GoldenPathCityDecisionMeanings/*.json`
- The current split is:
  - chain-specific: mission objective, dungeon identity, room sequence, runtime shortage math, final recommendation ordering
  - shared: bottleneck framing, dungeon-opportunity rationale, why-this-city-matters text, recommendation rationale text

## What Batch 28 Proved

- Third representative chain asset: `Assets/_Game/Resources/Content/GoldenPathChains/city-a-dungeon-alpha-risky-path.json`
- The same canonical rail can now carry a second representative route for the same `city-a -> dungeon-alpha` pair without replacing the primary city-side chain.
- Keep city-side default wording on the primary chain, then let route-aware consumers resolve route preview, encounter/battle setup, and outcome meaning through `TryGetChainForRoute(...)` / `TryGetCanonicalRoute(...)`.

## What Batch 29 Adds

- Lightweight authoring validator: `Assets/_Game/Scripts/Editor/GoldenPathAuthoringValidationRunner.cs`
- Editor entry point: `Tools/Project AAA/Validate Authoring Content`
- Batch entry point: `GoldenPathAuthoringValidationRunner.RunAuthoringContentValidation`
- The validator does not prove gameplay balance. It checks authoring integrity:
  - representative chain #1 / #2 / #3 existence
  - canonical data-path resolution
  - shared route / outcome / city / encounter / battle reference integrity
  - known fallback visibility
  - chain-local override summary

## What Batch 30 Closes Out

- Supported safe-route asset: `Assets/_Game/Resources/Content/GoldenPathChains/city-b-dungeon-beta-guarded-path.json`
- Shared route meaning asset: `Assets/_Game/Resources/Content/GoldenPathRouteMeanings/route-guarded-balanced.json`
- Shared outcome meaning asset: `Assets/_Game/Resources/Content/GoldenPathOutcomeMeanings/outcome-mana-shard-city-b-guarded-processing.json`
- Shared battle setup assets:
  - `Assets/_Game/Resources/Content/GoldenPathBattleSetups/battle-setup-beta-safe-room1.json`
  - `Assets/_Game/Resources/Content/GoldenPathBattleSetups/battle-setup-beta-safe-room2.json`
  - `Assets/_Game/Resources/Content/GoldenPathBattleSetups/battle-setup-beta-safe-elite.json`
- The validator now treats `city-b -> dungeon-beta -> safe` as a supported canonical route variant instead of a known fallback warning.

## What Batch 31 Surfaces

- `city-b -> dungeon-beta -> safe` is now a surfaced player-facing opportunity variant, not just a validator-only supported route.
- The surfacing seam is data-driven:
  - primary chains surface by default
  - non-primary route variants surface only when their chain asset sets `SurfaceAsOpportunityVariant=true`

## What Batch 32 Surfaces

- `city-a -> dungeon-alpha -> risky` is now also a surfaced player-facing opportunity variant.
- The surfaced matrix now includes both non-primary route variants currently supported by the Alpha/Beta authoring set.
- Current surfaced opportunity matrix:
  - `city-a -> dungeon-alpha -> safe`
  - `city-a -> dungeon-alpha -> risky`
  - `city-b -> dungeon-beta -> risky`
  - `city-b -> dungeon-beta -> safe`
- Current canonical-but-not-surfaced route variant:
  - None in the currently supported Alpha/Beta route-variant set

## What Batch 33 Adds

- Lightweight editor tooling now sits next to the validator:
  - `Tools/Project AAA/Show Surfaced Opportunity Matrix`
  - `Tools/Project AAA/Quick Open Golden Path Chain Assets`
- Batch entry point:
  - `GoldenPathAuthoringValidationRunner.RunSurfacedOpportunityMatrixSummary`
- The surfaced matrix summary is intentionally lightweight:
  - city / dungeon / route
  - canonical vs fallback source label
  - surfaced vs hidden state
  - shared-definition coverage
  - chain-local override summary
- Use it as the fast pre-flight check before adding a new surfaced opportunity or expanding chain #4.

## What Batch 34 Proves

- `city-b -> dungeon-beta -> safe` is now tracked as both canonical and consumer-visible, not only as an authored route variant.
- `GoldenPathAuthoringValidationRunner.RunSurfacedOpportunityMatrixSummary` now cross-checks:
  - authored surfacing intent (`SurfaceAsOpportunityVariant` or primary-chain ownership)
  - actual `CityDecisionReadModel.Opportunities` visibility
  - the `data:*` content source that ExpeditionPrep will inherit through `LinkedOpportunities`
- Treat the following combination as the surfaced proof for a promoted opportunity:
  - `Surface=cityhub+prep(primary|variant)`
  - `Consumer=cityhub+prep(actual)`
  - `ConsumerSource=data:*`

## What Batch 35 Adds

- `GoldenPathAuthoringValidationRunner.RunRepresentativeChainStatusSummary` now gives a no-approval-friendly representative/surfaced status report on top of the existing validator and surfaced matrix summary.
- The surfaced matrix summary now classifies each authored route with explicit status labels:
  - `PASS:fully-canonical-and-surfaced`
  - `WARN:canonical-but-not-surfaced`
  - `WARN:surfaced-with-chain-override`
  - `FAIL:surfaced-using-fallback`
  - `FAIL:surfaced-consumer-mismatch`
- Use those labels to decide whether a route is ready for player-facing promotion, still hidden canonical content, or regressing back toward fallback behavior.

## What Batch 36 Adds

- `GoldenPathAuthoringValidationRunner.RunSurfacedOpportunityPortfolioSummary` now treats the current player-facing surfaced set as an explicit portfolio instead of leaving it implicit inside the larger surfaced matrix dump.
- The surfaced summaries now also show:
  - `Prep=linked(actual)` or `Prep=missing(actual)`
  - `Recommendation=connected(actual)` or `Recommendation=city-only(actual)`
- In the current authored Alpha/Beta set, the surfaced portfolio is:
  - `city-a -> dungeon-alpha -> safe`
  - `city-a -> dungeon-alpha -> risky`
  - `city-b -> dungeon-beta -> risky`
  - `city-b -> dungeon-beta -> safe`
- Treat `CanonicalButNotSurfaced=None` plus `OfficialSurfacedSet=Yes` as the sign that the current surfaced portfolio matches the authored canonical route set.

## What Batch 37 Adds

- `GoldenPathAuthoringValidationRunner.RunSurfacedOpportunityResolutionTrace` now gives a surfaced-only trace that joins:
  - canonical/source state
  - actual CityHub visibility
  - actual ExpeditionPrep linkage
  - recommendation / why-now / usefulness text
  - shared definition ids used by the surfaced route
- The surfaced summaries now also report `MeaningfullyDistinctSurfaced`, so surfaced-maturity audits can tell whether the current player-facing set is genuinely varied instead of only being fully canonical.
- In the current authored Alpha/Beta set, `MeaningfullyDistinctSurfaced=4`, so the surfaced portfolio is stable enough for tooling-deepening work instead of another promotion batch.

## What Batch 38 Adds

- Tooling deepening now adds two more operations-focused helpers on top of the existing validator/surfaced summaries:
  - `Tools/Project AAA/Show Representative Chain Reference Trace`
  - `Tools/Project AAA/Quick Open Surfaced Opportunity Assets`
- The surfaced summaries now also carry `Issues=` so authoring audits can spot:
  - source fallback
  - consumer fallback
  - surfaced-but-unlinked prep state
  - surfaced recommendation drift
  - chain-local override pressure
- Surfaced resolution trace now also prints `SharedAssetPaths=...`, so the operator can see the actual shared definition asset paths, not only ids.
- Treat the new reference trace as the fastest way to answer:
  - which shared assets a representative/surfaced route actually uses
  - whether a route is clean, fallback-driven, or override-heavy
  - which assets should be quick-opened before editing

## What Batch 39 Adds

- Lightweight throughput helper: `Tools/Project AAA/Create Draft From Selected Golden Path Chain`
- Batch helper entry point: `GoldenPathAuthoringDraftHelper.RunCreateRepresentativeDraftTemplate`
- Draft scaffolds now land in:
  - `Assets/_Game/AuthoringDrafts/GoldenPathChains/`
- The helper deliberately copies a selected canonical chain into a non-canonical draft location so content authors can start from:
  - city / dungeon ids
  - route structure
  - room sequence
  - shared route / outcome / city / encounter / battle references
  without accidentally surfacing or canonicalizing the new draft too early.
- The helper auto-resets:
  - `PrimaryCityDungeonDefinition=false`
  - `SurfaceAsOpportunityVariant=false`
- The helper log now also states:
  - which fields were auto-filled
  - which fields still need author review before promotion
  - that validator/tooling summaries ignore the draft until it is moved into `Resources/Content/GoldenPathChains`

## What Batch 40 Adds

- Draft promotion readiness helper: `Tools/Project AAA/Show Draft Promotion Readiness`
- Batch helper entry point: `GoldenPathAuthoringDraftHelper.RunDraftPromotionReadinessSummary`
- The draft helper now also reports:
  - the draft resolver key (`city::dungeon::route`)
  - whether that resolver key is already owned by a canonical chain asset
  - whether the draft could be promoted on the current safe/risky surfaced rail without widening runtime route support
  - the concrete manual next step instead of only a generic "review before promotion" note
- Use this before trying to turn a hidden draft into a surfaced opportunity:
  - `blocked:resolver-key-already-owned` means the draft collides with an existing canonical route and should stay hidden or be reauthored
  - `blocked:requires-runtime-route-surface-expansion` means the current CityHub/ExpeditionPrep surface does not know how to expose that route yet
  - `candidate:manual-promotion-review` means the draft can move forward on the existing rail after author review

## What Batch 41 Adds

- Draft promotion context helper: `Tools/Project AAA/Quick Open Draft Promotion Context`
- Batch helper entry point: `GoldenPathAuthoringDraftHelper.RunBatchTemplateDraftPromotionContextSummary`
- The helper close-out now covers the next authoring step after readiness:
  - select one draft under `Assets/_Game/AuthoringDrafts/GoldenPathChains/`
  - quick-open the draft itself
  - quick-open the current canonical owner chain when the draft collides with an existing resolver key
- quick-open the linked shared route / outcome / city / encounter / battle assets
- This makes the helper usable for real comparison work instead of only telling the author that a draft is blocked.

## What Batch 43 Aligns

- The current surfaced portfolio remains the same four Alpha/Beta routes:
  - `city-a -> dungeon-alpha -> safe`
  - `city-a -> dungeon-alpha -> risky`
  - `city-b -> dungeon-beta -> risky`
  - `city-b -> dungeon-beta -> safe`
- No batch-42 surfaced promotion artifact was found in the repo, and no additional canonical-but-not-surfaced supported route exists on the current safe/risky rail.
- `GoldenPathAuthoringValidationRunner.RunAuthoringContentValidation` now reports authored surface state with the same labels as the surfaced-matrix tooling:
  - `cityhub+prep(primary)`
  - `cityhub+prep(variant)`
  - `hidden-canonical`
- Treat that alignment as the batch-43 close-out:
  - if a primary chain is player-facing, the validator should no longer describe it as hidden
  - if the current Alpha/Beta rail has no hidden supported route left, do not open a fake surfaced-expansion batch just because the older validator wording looked ambiguous

## What Batch 44 Clarifies

- The current surfaced portfolio still remains the same four Alpha/Beta routes:
  - `city-a -> dungeon-alpha -> safe`
  - `city-a -> dungeon-alpha -> risky`
  - `city-b -> dungeon-beta -> risky`
  - `city-b -> dungeon-beta -> safe`
- Batch 44 does not add another surfaced opportunity.
- Instead, it adds a direct surfaced-expansion gate summary:
  - `Tools/Project AAA/Show Surfaced Opportunity Expansion Gate`
  - batch entry point: `GoldenPathAuthoringValidationRunner.RunSurfacedOpportunityExpansionGateSummary`
- Use that summary when you need one answer to three questions at once:
  - is `city-b -> dungeon-beta -> safe` still fully canonical and truly surfaced?
  - is the current surfaced matrix healthy, or is there still a hidden/fallback/mismatch problem to close out?
  - should the next batch stay on portfolio cleanup, or is the rail healthy enough for tooling-first follow-up?
- Current expected gate result on the Alpha/Beta rail is:
  - `ExpansionGate=C:tooling-next`
  - because the current surfaced set is canonical, consumer-visible, prep-linked, recommendation-linked, and `MeaningfullyDistinctSurfaced=4`

## What Batch 45 Clarifies

- The current Alpha/Beta surfaced rail is still saturated on the supported `safe` / `risky` surface:
  - `city-a -> dungeon-alpha -> safe`
  - `city-a -> dungeon-alpha -> risky`
  - `city-b -> dungeon-beta -> risky`
  - `city-b -> dungeon-beta -> safe`
- Batch 45 does not add another surfaced opportunity.
- Instead, it makes `Tools/Project AAA/Show Draft Promotion Readiness` speak the same reality as the surfaced-matrix tooling:
  - blocked-draft counts are now broken out by cause
  - the readiness summary now prints the current surfaced-expansion gate inline
  - the selected-draft context now also states whether the draft fits the current surfaced rail or collides with an already-owned slot
- Use that helper summary when you need one answer to three questions at once:
  - is the current surfaced rail already full on the Alpha/Beta `safe/risky` seam?
  - is the draft blocked because the resolver key is already owned?
  - would promoting the draft require widening runtime route surfacing instead of only moving JSON?
- If you need the shortest status snapshot for GitHub handoff or GPT branching, read `docs/post-slice-batch-status.md` first.

## What Batch 46 Clarifies

- Draft promotion preflight helper: `Tools/Project AAA/Show Draft Promotion Preflight`
- Batch helper entry point: `GoldenPathAuthoringDraftHelper.RunDraftPromotionPreflightSummary`
- The helper now also reports the current supported surfaced rail itself:
  - `SupportedRailSlots=...`
  - `OccupiedSupportedRailSlots=...`
  - `OpenSupportedRailSlots=...`
  - `OpenSupportedResolverKeys=...`
- The current expected preflight state on the Alpha/Beta rail is:
  - `SupportedRailSlots=4`
  - `OccupiedSupportedRailSlots=4`
  - `OpenSupportedRailSlots=None`
  - `OpenSupportedResolverKeys=None`
  - `PreflightRecommendation=supported-safe-risky-rail-is-saturated-retarget-beyond-current-surface-or-widen-rail`
- For the current batch-template draft, the expected state is:
  - `BatchTemplateDraftResolverKey=city-a::dungeon-alpha::safe`
  - `BatchTemplateDraftState=blocked:resolver-key-already-owned`
  - `BatchTemplateDraftRailFit=supported-slot-owned-and-current-rail-saturated`
- Treat batch 46 as a preflight/diagnostic close-out, not as a fake surfaced-expansion batch:
  - if the rail is saturated, do not pretend a retargetable helper or a new non-colliding seed already exists on the current surface
  - use the preflight output to justify either retargeting the draft off the current rail or widening the surfaced seam on purpose

## What Batch 60 Deepens

- Batch 60 does not widen the surfaced portfolio.
- It deepens the existing `city-a -> dungeon-alpha` surfaced pair so the same safe/risky rail reads like authored dungeon content instead of a mostly generic sample.
- Alpha now uses route-specific shared encounter identities where the previous generic opening/midrun profiles made the dungeon feel too anonymous:
  - `encounter-profile-alpha-safe-entry`
  - `encounter-profile-alpha-safe-watch`
  - `encounter-profile-alpha-risky-breach`
  - `encounter-profile-alpha-risky-hall`
- Alpha risky now also uses a route-specific shared city-side rationale asset:
  - `city-decision-city-a-shard-surge-window`
- Batch 60 also tightens the existing Alpha route meanings and battle setup text so the surfaced preview, room sequence, battle framing, and City A recommendation all point at the same authored story:
  - Alpha safe = stability, shrine reset, cache prep, clean monarch close
  - Alpha risky = mixed breach, greed-cache spike, unstable shrine strain, Gel Core payoff
- Treat this batch as content completion on the current Alpha rail, not as a runtime or UI expansion.

## What Batch 61 Deepens

- Batch 61 does not widen the surfaced portfolio.
- It deepens the existing `city-b -> dungeon-beta` surfaced pair so Beta safe/risky no longer read mostly like loot/recover number variants.
- Beta now uses route-specific shared encounter identities where the previous generic opening/midrun wording flattened the dungeon:
  - `encounter-profile-beta-risky-gate`
  - `encounter-profile-beta-risky-vault`
  - `encounter-profile-beta-safe-watch`
  - `encounter-profile-beta-safe-vault`
- Beta safe now also uses a guarded-route city-side rationale asset:
  - `city-decision-city-b-guarded-processing-restart`
- Batch 61 also tightens existing Beta route/outcome/battle text so the surfaced preview, room sequence, battle framing, and City B recommendation all point at the same authored split:
  - Beta risky = raider vault, plunder tempo, war-room strain, chief's vault payout
  - Beta safe = watchfire route, orderly cache/vault hold, captain finish, steadier restart
- Treat this batch as Beta content density on the current surfaced rail, not as a runtime or UI expansion.

## How The Representative Chain Is Authored

The current data-driven samples are:

- `city-a` -> `dungeon-alpha` -> canonical route `safe` (`Rest Path`)
- `city-b` -> `dungeon-beta` -> canonical route `risky` (`Greedy Path`)
- `city-a` -> `dungeon-alpha` -> canonical route `risky` (`Standard Path`)
- supported non-representative route variant: `city-b` -> `dungeon-beta` -> canonical route `safe` (`Guarded Path`)

The asset currently owns:

- city/dungeon linkage metadata
- whether the chain is the primary city+dungeon definition or a route-variant add-on
- whether a non-primary route variant should surface as a city opportunity (`SurfaceAsOpportunityVariant`)
- bottleneck / mission text
- dungeon identity labels
- one canonical route definition
- one room sequence for that route

The shared meaning assets currently own:

- route-level risk / reward-bias / expected-impact meaning
- outcome-level usefulness / city-impact meaning
- post-result recommendation-shift meaning that now flows through `ResultPipeline -> WorldSim -> CityHub`
- city-side bottleneck / opportunity / recommendation rationale meaning that now flows through `CityDecisionModelBuilder` and into `ExpeditionPrep`

## What To Edit For The Next Content Batch

- Add or adjust representative chain data in `Assets/_Game/Resources/Content/GoldenPathChains/*.json`
- Add or adjust shared route meaning data in `Assets/_Game/Resources/Content/GoldenPathRouteMeanings/*.json`
- Add or adjust shared outcome meaning data in `Assets/_Game/Resources/Content/GoldenPathOutcomeMeanings/*.json`
- Add or adjust shared city-side meaning data in `Assets/_Game/Resources/Content/GoldenPathCityDecisionMeanings/*.json`
- Add or adjust shared encounter profile data in `Assets/_Game/Resources/Content/GoldenPathEncounterProfiles/*.json`
- Add or adjust shared battle setup data in `Assets/_Game/Resources/Content/GoldenPathBattleSetups/*.json`
- Keep the same field names as `GoldenPathChainDefinition`, `GoldenPathRouteDefinition`, and `GoldenPathRoomDefinition`
- Put `EncounterProfileId` / `BattleSetupId` on the encounter rooms inside the chain asset, then keep the actual encounter/battle meaning in the shared definition assets
- Prefer adding a new chain asset over widening the existing one into a generic mega-schema too early
- Keep using the same lookup seam:
  - `GoldenPathContentRegistry.TryGetChain(cityId, dungeonId)`
  - `GoldenPathContentRegistry.TryGetChainForRoute(cityId, dungeonId, routeId)`
  - `GoldenPathContentRegistry.TryGetCanonicalRoute(cityId, dungeonId, routeId, ...)`
  - `GoldenPathContentRegistry.TryGetRouteMeaning(routeMeaningId, ...)`
  - `GoldenPathContentRegistry.TryGetOutcomeMeaning(outcomeMeaningId, ...)`
  - `GoldenPathContentRegistry.TryGetCityDecisionMeaning(cityDecisionMeaningId, ...)`
  - `GoldenPathContentRegistry.TryGetRepresentativeEncounterRoom(cityId, dungeonId, routeId, ...)`
  - `GoldenPathContentRegistry.TryGetEncounterProfile(encounterProfileId, ...)`
  - `GoldenPathContentRegistry.TryGetBattleSetup(battleSetupId, ...)`
  - `ExpeditionPrepModelBuilder` for objective/usefulness
  - `CityDecisionModelBuilder` for bottleneck/opportunity/recommendation rationale
  - `StaticPlaceholderWorldView.DungeonContentData.cs` for canonical route identity / room sequence / expected-need-impact
  - `StaticPlaceholderWorldView.BattleContracts.cs` for battle-entry encounter/battle meaning carry-through
  - `CityDecisionModelBuilder` for surfaced route-variant opportunities

## Authoring QA Before Adding Chain #4

- Run `Tools/Project AAA/Validate Authoring Content` before treating a new chain as canonical
- Run `Tools/Project AAA/Show Surfaced Opportunity Matrix` when you need to confirm which authored routes are actually surfaced versus hidden canonical
- Run `Tools/Project AAA/Show Surfaced Opportunity Portfolio` when you want the player-facing surfaced set only, without the hidden canonical rows
- Run `Tools/Project AAA/Show Surfaced Opportunity Expansion Gate` when you want the batch-44 style decision summary that collapses target stability, surfaced-matrix health, and next-step direction into one log
- Run `Tools/Project AAA/Show Representative Chain Status` when you want the compact branch-B style summary for representative chains plus currently surfaced opportunity variants
- Run `Tools/Project AAA/Trace Surfaced Opportunity Resolution` when you need the surfaced-only cityhub/prep trace for each route before widening the portfolio or debugging a rationale mismatch
- Run `Tools/Project AAA/Show Representative Chain Reference Trace` when you need the shared-definition asset-path trace plus per-route `Issues=` drill-down for representative chains and surfaced opportunity variants
- Run `Tools/Project AAA/Quick Open Surfaced Opportunity Assets` when you want the chain asset plus linked shared route/outcome/city/encounter/battle assets selected in one step before editing
- Run `Tools/Project AAA/Create Draft From Selected Golden Path Chain` after selecting one canonical chain asset when you want a safe starting scaffold outside the canonical `Resources` path
- Run `Tools/Project AAA/Show Draft Promotion Readiness` before moving a draft into `Resources/Content/GoldenPathChains`; it tells you whether the draft collides with an existing canonical resolver key or still needs runtime surfacing support
- Run `Tools/Project AAA/Show Draft Promotion Preflight` when you need the shortest honest answer to "is there any open supported resolver key on the current surfaced rail at all?"
- Run `Tools/Project AAA/Quick Open Draft Promotion Context` after the readiness check when you need the blocked/promotable draft, the canonical owner, and the linked shared assets selected together for a real edit pass
- Read `BlockedByCanonicalOwner=` / `BlockedByRouteSurfaceExpansion=` / `PromotionRecommendation=` in the draft readiness summary before opening another surfaced-expansion batch:
  - `BlockedByCanonicalOwner>0` means the current surfaced/canonical rail already owns that resolver key
  - `BlockedByRouteSurfaceExpansion>0` means the draft sits outside the current `safe/risky` surfaced seam
  - `PromotionRecommendation=retarget-the-draft-resolver-key-before-promotion` means there is still at least one open supported slot, but the draft is currently pointing at the wrong owned key
  - `PromotionRecommendation=no-open-supported-resolver-key-on-current-rail` means the current surfaced rail is saturated and the next honest step is retarget-or-widen, not silent promotion
- Read `SupportedRailSlots=` / `OpenSupportedRailSlots=` / `OpenSupportedResolverKeys=` in the preflight or context summary when you need the rail-capacity answer instead of only the selected-draft answer:
  - `OpenSupportedRailSlots=None` plus `OpenSupportedResolverKeys=None` means the current Alpha/Beta `safe/risky` surfaced seam has no honest promotion slot left
  - `SupportedRailFit=supported-slot-owned-and-current-rail-saturated` means the selected draft is not just colliding with an owner, it is colliding on a currently full supported rail
- Read `SurfacedExpansionGate=` in the draft readiness or draft context summary when you want the draft helper and surfaced-portfolio tooling to answer the same question about current promotion headroom
- Read `Consumer=` and `ConsumerSource=` in the surfaced matrix summary before treating a canonicalized route as truly surfaced; asset flags alone are no longer enough for surfaced promotion work
- Read `Status=` / `StatusWhy=` in the surfaced matrix summary before promoting or debugging a route:
  - `PASS:fully-canonical-and-surfaced` means the authored path, CityHub, and ExpeditionPrep all line up on `data:*`
  - `WARN:canonical-but-not-surfaced` means the route is still valid content, but it is not a player-facing option yet
  - `WARN:surfaced-with-chain-override` means the route is surfaced cleanly but still carries chain-local meaning worth reviewing before copying it further
  - `FAIL:surfaced-using-fallback` or `FAIL:surfaced-consumer-mismatch` means do not add more surfaced variety until that path is corrected
- Read `Prep=` / `Recommendation=` before treating a surfaced route as portfolio-ready:
  - `Prep=linked(actual)` means the route is actually present inside `ExpeditionPrepReadModel.LinkedOpportunities`
  - `Recommendation=connected(actual)` means surfaced city rationale plus prep recommendation text are both present for that route's city+dungeon prep state
- Read `MeaningfullyDistinctSurfaced=` before opening another surfacing batch:
  - `MeaningfullyDistinctSurfaced>=3` is the current maturity gate for tooling-deepening work
  - if the trace reports `near-duplicate:*` on a surfaced route, treat that as surfaced-portfolio thinness or rationale drift before adding more player-facing variety
- Read `ExpansionGate=` before deciding whether the next batch should be a surfaced close-out, matrix-alignment pass, or tooling-first pass:
  - `A:target-close-out` means the tracked surfaced target is still not fully canonical + surfaced + prep-linked + recommendation-linked
  - `B:matrix-align` means the target is fine, but the broader surfaced matrix still has hidden/fallback/mismatch drift
  - `C:tooling-next` means the current surfaced portfolio is healthy enough that the next honest bottleneck is tooling/operations, not another forced surfacing pass
- Run `Tools/Project AAA/Quick Open Golden Path Chain Assets` when you need to jump directly to the current chain assets before editing
- Read `Issues=` in the surfaced matrix, surfaced portfolio, representative status, and surfaced trace summaries before copying a route pattern:
  - `clean` means no current surfaced/canonical/prep/recommendation problem was detected
  - `source:fallback-hardcoded` means the authored route itself is not on the canonical rail
  - `consumer:hidden` or `prep:missing-link` means CityHub/ExpeditionPrep surfacing drift still exists
  - `override:*` means chain-local meaning is still active and should be copied deliberately, not blindly
- Read `SharedAssetPaths=` in the surfaced trace or representative reference trace before editing a route when you need the actual JSON asset locations instead of only shared definition ids
- Treat `Assets/_Game/AuthoringDrafts/GoldenPathChains/*.json` as non-canonical working drafts:
  - they are not surfaced
  - they are not part of the validator/tooling matrix
  - they are only promotion candidates after the draft is intentionally moved into `Resources/Content/GoldenPathChains`
- After using the draft helper, review at least:
  - `ChainId`
  - city-side rationale text
  - route preview / risk-reward wording
  - whether the copied shared references are still correct
  - whether the draft should stay hidden or later become a surfaced variant
- After using the draft helper, also read `PromotionState=`:
  - `blocked:resolver-key-already-owned` means the current surfaced/canonical matrix is already occupying that route slot
  - `blocked:requires-runtime-route-surface-expansion` means promotion would require widening the current safe/risky surface instead of just moving JSON
  - `candidate:manual-promotion-review` means the draft can be reviewed for promotion on the current rail
- In batch mode, run `GoldenPathAuthoringValidationRunner.RunAuthoringContentValidation`
- In batch mode, run `GoldenPathAuthoringValidationRunner.RunSurfacedOpportunityMatrixSummary` when you want the surfaced/canonical/fallback snapshot in CI-style logs
- In batch mode, run `GoldenPathAuthoringValidationRunner.RunSurfacedOpportunityPortfolioSummary` when you want the official surfaced-set rollup for CityHub/ExpeditionPrep-focused audits
- In batch mode, run `GoldenPathAuthoringValidationRunner.RunSurfacedOpportunityExpansionGateSummary` when you want the batch-44 style “close out / align / tooling-next” decision in one log
- In batch mode, run `GoldenPathAuthoringValidationRunner.RunRepresentativeChainStatusSummary` when you want the compact representative/surfaced status rollup without opening the full surfaced matrix dump
- In batch mode, run `GoldenPathAuthoringValidationRunner.RunSurfacedOpportunityResolutionTrace` when you need the surfaced-only trace log with CityHub summary, prep recommendation, why-now, usefulness, and shared reference ids in one place
- In batch mode, run `GoldenPathAuthoringValidationRunner.RunRepresentativeChainReferenceTrace` when you need the tracked representative/surfaced routes with asset paths plus per-route issue drill-down in one log
- In batch mode, run `GoldenPathAuthoringDraftHelper.RunCreateRepresentativeDraftTemplate` when you want to verify the draft-scaffold helper against the baseline safe representative chain without touching the canonical surfaced set
- In batch mode, run `GoldenPathAuthoringDraftHelper.RunDraftPromotionReadinessSummary` when you want a draft-folder audit that explains whether each hidden draft is promotable, colliding with an existing canonical route, or blocked by the current surfaced-route rail
- In batch mode, run `GoldenPathAuthoringDraftHelper.RunDraftPromotionPreflightSummary` when you need the supported-rail slot inventory and the batch-template draft's fit against that rail in one log
- In batch mode, run `GoldenPathAuthoringDraftHelper.RunBatchTemplateDraftPromotionContextSummary` when you want the current template draft plus its canonical owner/shared-asset comparison context summarized in one log
- Treat validator `FAIL` as a broken canonical rail:
  - missing chain definition
  - unresolved shared reference
  - representative route not using `data:*`
  - route-variant chain overriding the primary city+dungeon owner unexpectedly
- Treat validator `WARN` as cleanup pressure, not immediate breakage:
  - known fallback coverage still active
  - chain-local override still carries meaning that could be shared later
- Treat validator `FAIL` for `city-b -> dungeon-beta -> safe` as a broken supported route variant:
  - missing route-variant chain asset
  - missing shared route / outcome / battle setup reference
  - resolver miss on `city-b::dungeon-beta::safe`
- Treat validator `FAIL` for `surface-opportunity-variant=false` on a supported surfaced route as a surfaced-content regression:
  - the canonical asset still exists, but CityHub / ExpeditionPrep will no longer expose it as a supported player-facing option
- Treat validator/smoke `FAIL` for `city-a -> dungeon-alpha -> risky` surfacing as the same regression class:
  - route-variant data still exists, but the surfaced matrix no longer matches the canonical authored set
- If both data and fallback exist, the validator should make the fallback visible instead of allowing silent drift

## What Is Still Hardcoded

- non-canonical fallback routes for representative chains that do not yet have route data
- any chain that does not have a JSON asset under `Resources/Content/GoldenPathChains`
- live monster stat tuning and any encounter roster that has not been promoted into a canonical representative battle setup asset
- broader city recommendation heuristics and world-economy mutation rules
- chain-specific overrides for city-impact interpretation if a representative chain cannot reuse an existing outcome-meaning asset cleanly
- any city-side bottleneck/opportunity text for non-representative chains that do not yet point at a shared city-decision meaning asset
- surfaced `city-b -> dungeon-beta -> safe` is no longer part of the known fallback set; if it drops back to fallback, treat that as a canonical authoring regression

## Authoring Rule Of Thumb

- Data first if the new content is meant to become part of the reusable golden-path pattern
- Fallback okay if the content is still prototype-only and not yet chosen as a representative chain
- If data and fallback both exist, the data path is canonical and the fallback path is only for uncovered cases
- A third representative chain should look like the current two:
  - a surfaced route-variant chain may also be a representative chain if it still reuses the same shared city/route/encounter/outcome authoring rail
  - one asset per representative `cityId + dungeonId + canonical route`
  - keep exactly one `PrimaryCityDungeonDefinition=true` asset for each city+dungeon pair that owns the city-side default wording
  - route-variant chains should set `PrimaryCityDungeonDefinition=false` and rely on `TryGetChainForRoute(...)` / `TryGetCanonicalRoute(...)`
  - set `SurfaceAsOpportunityVariant=true` only when a non-primary route variant should appear in CityHub / ExpeditionPrep as a real player-facing option
  - chain-specific mission text that explains why the city should care right now
  - shared route/outcome meaning references whenever the semantics already exist
  - shared city-decision meaning references whenever the city-side bottleneck/opportunity/recommendation rationale can reuse an existing meaning asset
  - shared encounter profile and battle setup references whenever the new chain reuses an existing battle-entry meaning
  - shared outcome meaning references whenever the result should reuse an existing city-impact / recommendation-shift interpretation
