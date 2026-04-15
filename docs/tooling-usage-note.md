# Project AAA Tooling Usage Note

## Batch 48 Tooling Snapshot

- Runtime baseline remains `grid dungeon` + `standard JRPG battle`.
- Current surfaced Alpha/Beta portfolio remains:
  - `city-a -> dungeon-alpha -> safe`
  - `city-a -> dungeon-alpha -> risky`
  - `city-b -> dungeon-beta -> risky`
  - `city-b -> dungeon-beta -> safe`
- Current draft helper state:
  - draft count: `1`
  - promotable drafts on current surfaced rail: `0`
  - blocked by canonical owner: `1`
  - retargetable off-rail candidates: `1`
  - hidden off-rail canonical drafts in draft space: `0`
  - promoted hidden canonical routes: `1`
  - supported rail slots: `4`
  - open supported rail slots: `None`
  - promotion recommendation: `retarget-beyond-current-surface-rail-with-helper`
  - next batch-template retarget draft: `Assets/_Game/AuthoringDrafts/GoldenPathChains/draft-city-a-dungeon-alpha-safe-off-rail-v2.json`
  - promoted hidden canonical asset: `Assets/_Game/Resources/Content/GoldenPathChains/city-a-dungeon-alpha-safe-off-rail-v1.json`

## Run Order

1. `Tools/Project AAA/Validate Authoring Content`
2. `Tools/Project AAA/Show Surfaced Opportunity Expansion Gate`
3. `Tools/Project AAA/Show Draft Promotion Readiness`
4. `Tools/Project AAA/Show Draft Promotion Preflight`
5. `Tools/Project AAA/Quick Open Draft Promotion Context`
6. `Tools/Project AAA/Show Draft Retarget Candidate`
7. `Tools/Project AAA/Create Retargeted Draft From Selected Draft`
8. `Tools/Project AAA/Show Hidden Canonical Promotion Summary`
9. `Tools/Project AAA/Promote Selected Draft As Hidden Canonical`

## How To Read The Draft Helpers

- `Show Draft Promotion Readiness`
  - Use this first when you want per-draft blocked/promotable state.
  - Read:
    - `BlockedByCanonicalOwner=...`
    - `BlockedByRouteSurfaceExpansion=...`
    - `PromotionRecommendation=...`
    - inline `SurfacedExpansionGate=...`
- `Show Draft Promotion Preflight`
  - Use this when you need the supported-rail answer itself, not only the selected draft answer.
  - Read:
    - `SupportedRailSlots=...`
    - `OccupiedSupportedRailSlots=...`
    - `OpenSupportedRailSlots=...`
    - `OpenSupportedResolverKeys=...`
    - `PreflightRecommendation=...`
- `Quick Open Draft Promotion Context`
  - Use this after readiness/preflight when a draft is blocked and you need the actual owner + shared asset context selected together.
  - Read:
    - `CurrentCanonicalOwner=...`
    - `DraftPromotionFit=...`
    - `SupportedRailFit=...`
    - `ManualNext=...`
- `Show Draft Retarget Candidate`
  - Use this after readiness/preflight/context when the current safe/risky rail is saturated and you need the next non-colliding off-rail candidate.
  - Read:
    - `RetargetState=...`
    - `RetargetResolverKey=...`
    - `RetargetDraftAsset=...`
    - `RetargetSurfaceState=hidden-canonical-if-promoted`
- `Create Retargeted Draft From Selected Draft`
  - Use this when the retarget summary says the helper found or can materialize a non-colliding off-rail draft.
  - The helper keeps:
    - shared route / outcome / city / encounter / battle refs
    - hidden draft surface flags
  - The helper changes:
    - `ChainId`
    - `CanonicalRoute.RouteId`
    - target draft asset path under `Assets/_Game/AuthoringDrafts/GoldenPathChains/`
- `Show Hidden Canonical Promotion Summary`
  - Use this when a retargeted draft is already owner-safe and you want the exact canonical promotion target before moving it into `Resources`.
  - Read:
    - `PromotionState=...`
    - `TargetCanonicalChainId=...`
    - `TargetCanonicalAsset=...`
    - `TargetSurfaceState=hidden-canonical`
    - `CurrentRailImpact=...`
- `Promote Selected Draft As Hidden Canonical`
  - Use this when the summary reports `candidate:promote-hidden-canonical-owner-safe`.
  - The helper keeps the route off the current surfaced rail by forcing:
    - `PrimaryCityDungeonDefinition=false`
    - `SurfaceAsOpportunityVariant=false`
  - The helper also canonicalizes the chain id by removing the `draft-` prefix before moving the asset into `Resources/Content/GoldenPathChains/`.

## Current Honest Interpretation

- `PromotionRecommendation=retarget-beyond-current-surface-rail-with-helper`
  means the current Alpha/Beta surfaced `safe/risky` rail is already saturated, but the helper can now point at a non-colliding off-rail draft instead of leaving the next step ambiguous.
- `SupportedRailFit=supported-slot-owned-and-current-rail-saturated`
  means the draft is not merely colliding with a resolver key; it is colliding on a currently full supported surfaced rail.
- The first off-rail hidden-canonical retarget has now been promoted:
  - `city-a::dungeon-alpha::safe-off-rail-v1`
  - `Assets/_Game/Resources/Content/GoldenPathChains/city-a-dungeon-alpha-safe-off-rail-v1.json`
- In surfaced/tooling summaries, that promoted route should now read as:
  - `Status=WARN:canonical-but-not-surfaced`
  - `Surface=hidden-canonical`
  - not as a fifth surfaced route
- The next honest move is:
  - keep accumulating hidden-canonical off-rail inventory, or
  - widen the surfaced route seam deliberately in a later batch if product intent actually needs it.
