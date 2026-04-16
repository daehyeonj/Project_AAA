# Project AAA Tooling Usage Note

## Batch 46 Tooling Snapshot

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
  - supported rail slots: `4`
  - open supported rail slots: `None`

## Run Order

1. `Tools/Project AAA/Validate Authoring Content`
2. `Tools/Project AAA/Show Surfaced Opportunity Expansion Gate`
3. `Tools/Project AAA/Show Draft Promotion Readiness`
4. `Tools/Project AAA/Show Draft Promotion Preflight`
5. `Tools/Project AAA/Quick Open Draft Promotion Context`

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

## Current Honest Interpretation

- `PromotionRecommendation=no-open-supported-resolver-key-on-current-rail`
  means the current Alpha/Beta surfaced `safe/risky` rail is already saturated.
- `SupportedRailFit=supported-slot-owned-and-current-rail-saturated`
  means the draft is not merely colliding with a resolver key; it is colliding on a currently full supported surfaced rail.
- In that state, do not open another fake surfaced-expansion batch on the current rail.
- The next honest move is:
  - retarget the draft beyond the current surfaced rail, or
  - widen the surfaced route seam deliberately.
