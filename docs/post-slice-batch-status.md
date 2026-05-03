# Project AAA Post-Slice Batch Status

## Current Mainline Snapshot

- Snapshot date: `2026-04-20`
- Sync intent: `preserve the current local runtime state on GitHub main so repository readers can inspect the live working baseline`
- Active runtime baseline to preserve: `grid dungeon explore shell` + `current battleScene HUD`
- UI safety rule: `reject rollback prompts against the currently accepted runtime UI; additive or preserving UI work is allowed`
- Runtime decision note: see `docs/runtime/mainline-snapshot-2026-04-17.md`

## Current Verdict

- Latest closed batch: `Batch 92 dungeon dilemma variation v1 + Batch 91 encounter variety runtime proof/balance + Batch 90 encounter variety route pressure v1 + Batch 89 room interaction consequence chain v1 + Batch 88 dungeon room interaction v1 + Batch 87 dungeon route content variety v1 + Batch 86 wait cost pressure clock v1 + Batch 85/85.1 second-run route consequence proof + Batch 84 recovery pressure choice v1 + Batch 83 second-run decision pressure v1 + Batch 82 repeatable core game loop v1 + Batch 81 presentation vertical slice lock + Batch 80.1 selected CityHub pressure board readability UX + Batch 80 world result-pressure board + Batch 79.3 CityHub -> ExpeditionPrep re-entry continuity triage + Batch 79.2 ResultPipeline intent packaging smoke triage + Batch 79.1 route scenario runtime proof + smoke timeout triage + Batch 79 dungeon route operating scenario + Batch 78.1 combat core runtime proof + Batch 78 post-UI combat core revalidation + Batch 77.10.1 inventory modal target-status cleanup + Batch 77.10 runtime skin bridge + Batch 77.9 UI skin preview mapping QA + Batch 77.8.1 skin preview validation gate + Batch 77.8 skin preview layout tuning + Batch 77.7 human-mapped skin assignment gate + Batch 77.6 manual skin assignment QA + Batch 77.5 UI sprite import audit + Batch 77.4 folder organization + Batch 78 + Batch 77.1 blocker fix + Batch 77.2 battle/inventory UI skinning scaffold + scene architecture scaffold follow-up + Batch 77.3 battle/inventory UI preview scaffold`
- Runtime baseline: `grid dungeon` + `standard JRPG battle`
- Canonical representative rail: stable
- Surfaced portfolio: stable
- Alpha surfaced pair: `content-completed on current rail`
- Beta surfaced pair: `content-thickened on current rail`
- Current signature demo pair: `city-a -> dungeon-alpha operating-scenario rail; city-b -> dungeon-beta remains the content-thickened presenter rail`
- Current presenter playbook: `docs/runtime/batch71-beta-signature-demo-playbook.md`
- Next honest bottleneck: `manual feel QA for Batch92 use-vs-skip choice tension, then tune visibility/payoff only if the playable decision still feels too flat`
- Scene architecture status: `compile-safe persistent-root scaffold added; SampleScene remains the live playable baseline`

## Batch 92 Dungeon Dilemma Variation Close-Out

- Selected branch: `Branch A/D - use-vs-skip variation on existing grid room interactions`
- Why this was the honest next game-development step:
  - Batch91 locked the Stability-vs-Surge battle-feel baseline, so Rest Shrine / Greed Cache could become a decision instead of a one-way proof beat
  - existing room, recovery, cache-loot, battle context, result, and world-board rails were enough; no legacy choice shell or new system was needed
  - the player can now see that using a room interaction carries one next-beat consequence, while skipping honestly avoids or loses that consequence
- Preflight:
  - Batch91 landing: `PASS`
  - Room consequence baseline: `PASS`; existing Greed Cache open path still proves +3 mana_shard and Cache Pressure
  - Runtime baseline: `UNCHANGED`; grid dungeon + standard JRPG battle
  - ResultPipeline rewrite: `NONE`
  - Fatigue/economy/combat stat system changes: `NONE`
- Dilemma implementation:
  - Rest Shrine prompt now explains `[E]` use versus moving on to skip
  - Rest Shrine use still arms `Shrine Protection` for `Watch Hall`
  - Rest Shrine skip source text now honestly says no `Shrine Protection`
  - Greed Cache prompt now explains `+3 mana_shard + Cache Pressure` versus moving on to skip
  - Greed Cache skip leaves carried/chest loot unchanged, arms a `cache_skipped` next-beat readback, and advances to `Goblin Pair Hall`
  - Goblin Pair Hall battle context, enemy intent, popover, result, and world board now distinguish `Cache Pressure` open from `Cache skipped`
- Files changed for Batch92:
  - `Assets/_Game/Scripts/Dungeon/StaticPlaceholderWorldView.DungeonRun.cs`
  - `Assets/_Game/Scripts/Editor/Batch82RepeatCoreLoopProofRunner.cs`
  - `Assets/_Game/Scripts/Dungeon/DUNGEON_RUN_CONTRACT.md`
  - `docs/architecture/validation-matrix.md`
  - `docs/runtime/batch92-dungeon-dilemma-variation-checklist.md`
  - `docs/post-slice-batch-status.md`
- Compile:
  - log: `unity-batch92-compile.log`
  - result: `PASS`
- Targeted proof:
  - log: `unity-batch92-dilemma-proof.log`
  - result: `PASS`
  - proof markers: `PASS :: Batch92 Rest Shrine use`, `PASS :: Batch92 Greed Cache skip`, `PASS :: Batch92 Greed Cache skip battle context`, `PASS :: Batch92 dungeon dilemma variation proof`
- Batch10 smoke:
  - log: `unity-batch92-smoke.log`
  - result: `PASS`
- Regression proof:
  - Batch89 open-path regression: `PASS`, log `unity-batch92-regression-batch89.log`
  - Batch91 encounter-feel regression: `PASS`, log `unity-batch92-regression-batch91.log`
- Performance proof:
  - `PASS by static scope`; no per-frame rebuild loop, broad route-pressure cache, or modal rebuild was added
- UI shape changed?: `No layout/skin change; room prompts, battle context, popover/result/world copy, and proof runner only`
- What the player can now choose:
  - `Rest Shrine: heal/protection now, or move on with no Shrine Protection`
  - `Greed Cache: take +3 and Cache Pressure, or skip the payout to avoid Cache Pressure`
- Remaining weakness:
  - Manual feel QA is still needed; automated proof verifies runtime branches and readback, not whether the choice cadence feels tense enough in hand
- Recommended next batch:
  - `Playtest Batch92 and tune choice visibility/payoff if needed; do not add a larger fatigue or economy system until the current use-vs-skip beat is felt manually`

## Batch 91 Encounter Variety Runtime Proof / Balance Close-Out

- Selected branch: `Branch A - Batch90 landed; lock runtime proof/balance over existing battle surfaces`
- Why this was the honest next game-development step:
  - Batch90 made route-pressure readbacks visible, but Batch92 should not add more dungeon dilemmas until the baseline Stability-vs-Surge battle feel is proof-locked
  - existing Alpha safe/risky encounter setup already provides the difference: Slime Front has lower ATK/random pressure, Goblin Pair Hall has higher ATK/lowest-HP focus/Cache Pressure
  - this pass kept mechanics intact and avoided new combat systems, fatigue, skills, ResultPipeline changes, scene migration, legacy shells, and per-frame rebuilds
- Preflight:
  - Batch90 landing: `PASS` via existing route-pressure docs/proof runner/log and Batch91 proof reuse
  - Room consequence: `PASS`; Rest Shrine / Greed Cache still arm `Shrine Protection` / `Cache Pressure`
  - Room interaction: `PASS`; Greed Cache `[E]` interaction remains runtime-proven in Batch89 regression
  - Route consequence: `PASS`; proof still returns Stability x16 and Surge x20/strain readbacks
  - Wait cost: `UNCHANGED`; no wait/recovery rail edits
  - Combat core: `UNCHANGED`; command/target, intent, and role payoff rails remain standard JRPG battle
  - Runtime baseline: `UNCHANGED`; grid dungeon + standard JRPG battle
  - Modal/performance: `UNCHANGED`; no inventory/skin/world-selection surface edits
- Encounter variety implementation:
  - Stability / Slime Front now reads `Threat: Moderate`, `ATK 4`, `predicted 2`, random pressure, and Rest Shrine -> Shrine Protection setup
  - Surge / Goblin Pair Hall now reads `Threat: High`, `ATK 8`, `predicted 4+`, lowest-HP focus, Cache Pressure payoff, and recovery strain
  - Battle context and enemy intent use existing route/setup/live monster state only
  - Preview/log keeps the existing Cache Pressure payoff and Mira/Rune finish-window readback
  - Popover confirms Stability route check and Surge payout/strain
- Balance changes, if any:
  - `None`; wording/proof lock only, no encounter stat tuning
- Files changed for Batch91:
  - `Assets/_Game/Scripts/Dungeon/StaticPlaceholderWorldView.DungeonRun.cs`
  - `Assets/_Game/Scripts/Editor/Batch82RepeatCoreLoopProofRunner.cs`
  - `Assets/_Game/Scripts/Dungeon/DUNGEON_RUN_CONTRACT.md`
  - `Assets/_Game/Scripts/Rpg/Battle/BATTLE_EXECUTION_CONTRACT.md`
  - `docs/architecture/flow-contracts.md`
  - `docs/architecture/validation-matrix.md`
  - `docs/runtime/batch91-encounter-variety-runtime-qa-checklist.md`
  - `docs/post-slice-batch-status.md`
- Compile:
  - log: `unity-batch91-compile.log`
  - result: `PASS`
- Authoring validation:
  - `NOT RUN`; no content assets/JSON changed
- Targeted proof:
  - log: `unity-batch91-encounter-variety-balance-proof.log`
  - result: `PASS`
  - proof markers: `PASS :: Batch91 Stability battle readback`, `PASS :: Batch91 runtime balance lock`, `PASS :: Batch91 Surge battle readback`, `PASS :: Batch91 encounter variety runtime proof balance`
- Batch10 smoke:
  - log: `unity-batch91-smoke.log`
  - result: `PASS`
- Regression proof:
  - Batch89 room interaction consequence chain: `PASS`, log `unity-batch91-regression-batch89.log`
- Performance proof:
  - `PASS by static scope`; no new Update/OnGUI rebuild loop or per-frame route-pressure cache was added
  - `git diff --check`: `PASS`, line-ending warnings only
- UI shape changed?: `No layout/skin change; battle context, intent, target preview, popover copy, and proof runner only`
- What the player can now feel:
  - `Stability let me keep control, while Surge pushed me harder but paid more.`
- Remaining combat/dungeon weakness:
  - Manual feel QA is still needed; automated proof verifies readback and runtime contrast, not subjective fun
- Can Batch92 begin?:
  - `Yes`; the baseline route-specific battle feel is proof-locked enough to expand Rest Shrine / Greed Cache use-vs-skip dilemmas

## Batch 90 Encounter Variety Route Pressure v1 Close-Out

- Selected branch: `Branch C - route-pressure readback over existing safe/risky battle surfaces`
- Why this was the honest next step:
  - Batch89 made Rest Shrine / Greed Cache consequences visible, but the battles themselves still needed clearer route-pressure identity
  - Alpha Safe and Risky already differ through route, room, encounter profile, battle setup, live monster HP/ATK/target-patterns, and Cache Pressure state
  - this pass avoided new combat mechanics, status/element systems, fatigue, inventory changes, ResultPipeline rewrite, legacy shell revival, and per-frame rebuild
- Player-facing readbacks added:
  - Stability `Slime Front` battle context now shows controlled slime pressure, live HP/ATK totals, random pressure, and Rest Shrine sustain
  - Surge `Goblin Pair Hall` battle context now shows Cache Pressure, live HP/ATK totals, lowest-HP focus, and recovery strain
  - enemy intent labels now append route-pressure predicted-damage readback for the targeted Alpha encounters
  - target preview now surfaces `Cache Pressure payoff` / role-finish-window copy for Surge target selection
- Files changed:
  - `Assets/_Game/Scripts/Dungeon/StaticPlaceholderWorldView.DungeonRun.cs`
  - `Assets/_Game/Scripts/Dungeon/StaticPlaceholderWorldView.BattleContracts.cs`
  - `Assets/_Game/Scripts/Rpg/Battle/StaticPlaceholderWorldView.RpgBattleSurfaceOwnership.cs`
  - `Assets/_Game/Scripts/Rpg/Battle/StaticPlaceholderWorldView.RpgBattleIntentOwnership.cs`
  - `Assets/_Game/Scripts/Rpg/Battle/StaticPlaceholderWorldView.RpgBattleActionOwnership.cs`
  - `Assets/_Game/Scripts/Editor/Batch82RepeatCoreLoopProofRunner.cs`
  - `Assets/_Game/Scripts/Dungeon/DUNGEON_RUN_CONTRACT.md`
  - `Assets/_Game/Scripts/Rpg/Battle/BATTLE_EXECUTION_CONTRACT.md`
  - `docs/architecture/flow-contracts.md`
  - `docs/architecture/validation-matrix.md`
  - `docs/runtime/batch90-encounter-variety-route-pressure-checklist.md`
  - `docs/post-slice-batch-status.md`
- Compile:
  - log: `unity-batch90-compile.log`
  - result: `PASS`
- Targeted proof:
  - log: `unity-batch90-encounter-variety-proof.log`
  - result: `PASS`
  - proof markers: `PASS :: Batch90 Stability battle readback`, `PASS :: Batch90 Stability encounter popover`, `PASS :: Batch90 Surge battle readback`, `PASS :: Batch90 encounter variety route pressure proof`
- Regression proof:
  - Batch89 room interaction consequence chain: `PASS`, log `unity-batch90-regression-batch89.log`
- Batch10 smoke:
  - log: `unity-batch90-smoke.log`
  - result: `PASS`
- Static check:
  - `git diff --check`: `PASS`, line-ending warnings only
- UI shape changed?: `No layout/skin overhaul; battle context, intent, target preview, and popover copy only`
- Manual UX proof: `NOT CLAIMED`; checklist added at `docs/runtime/batch90-encounter-variety-route-pressure-checklist.md`

## Batch 89 Room Interaction Consequence Chain v1 Close-Out

- Selected branch: `Branch B - interactions existed and had real data, but needed a tiny next-encounter consequence state`
- Why this was the honest next step:
  - Batch88 proved `[E]` room interactions existed, but Batch89 needed the next battle beat to acknowledge them
  - Greed Cache already grants real cache loot; Rest Shrine already applies real HP recovery or honest no-op preservation
  - this pass avoided new combat mechanics, a fatigue system, a ResultPipeline rewrite, scene migration, and per-frame rebuild
- Player-facing readbacks added:
  - Rest Shrine arms `Shrine Protection` for the next Alpha encounter
  - Greed Cache arms `Cache Pressure` for `Goblin Pair Hall`
  - battle context and encounter popover now confirm the consequence, then final/world readbacks preserve it through existing event summary fields
- Manual UX proof: `NOT CLAIMED`; checklist added at `docs/runtime/batch89-room-interaction-consequence-chain-checklist.md`

## Batch 88 Dungeon Room Interaction v1 Close-Out

- Selected branch: `Branch A - existing Rest Shrine / Greed Cache data already exists; make those beats playable in the current grid dungeon`
- Why this was the honest next step:
  - Batch87 made the route feel readable, but the route-defining utility rooms still needed an explicit player action
  - Rest Shrine and Greed Cache already exist in the Alpha safe/risky room data with real recovery/cache values
  - this pass avoided a new room system, fake consequence, ResultPipeline rewrite, scene migration, and per-frame rebuild
- Player-facing readbacks added:
  - Rest Shrine prompt: `[E] Use Rest Shrine` with Stability sustain/recovery copy
  - Greed Cache prompt: `[E] Open Greed Cache` with real `+3 mana_shard` cache payoff
  - encounter popover route check now reflects `Cache payoff secured; Surge strain rising`
  - result/world board now carries the room consequence through existing event-choice/readback fields
- Files changed:
  - `Assets/_Game/Scripts/Dungeon/StaticPlaceholderWorldView.DungeonRun.cs`
  - `Assets/_Game/Scripts/Dungeon/StaticPlaceholderWorldView.DungeonRun.ShellContracts.cs`
  - `Assets/_Game/Scripts/Dungeon/StaticPlaceholderWorldView.RunState.cs`
  - `Assets/_Game/Scripts/World/StaticPlaceholderWorldView.WorldSnapshotContext.cs`
  - `Assets/_Game/Scripts/Editor/Batch10SmokeValidationRunner.cs`
  - `Assets/_Game/Scripts/Editor/Batch78_1CombatCoreRuntimeProofRunner.cs`
  - `Assets/_Game/Scripts/Editor/Batch79_1RouteScenarioRuntimeProofRunner.cs`
  - `Assets/_Game/Scripts/Editor/Batch82RepeatCoreLoopProofRunner.cs`
  - `Assets/_Game/Scripts/Dungeon/DUNGEON_RUN_CONTRACT.md`
  - `docs/architecture/flow-contracts.md`
  - `docs/architecture/validation-matrix.md`
  - `docs/runtime/batch88-dungeon-room-interaction-checklist.md`
  - `docs/post-slice-batch-status.md`
- Compile:
  - log: `unity-batch88-compile.log`
  - result: `PASS`
- Targeted proof:
  - log: `unity-batch88-room-interaction-proof.log`
  - result: `PASS`
  - proof markers: `PASS :: Batch88 Greed Cache interaction`, `PASS :: Batch88 interaction encounter popover`, `PASS :: Batch88 dungeon room interaction proof`
- Regression proof:
  - Batch87 route feel: `PASS`, log `unity-batch88-regression-batch87.log`
- Batch10 smoke:
  - log: `unity-batch88-smoke.log`
  - result: `PASS`
- Static check:
  - `git diff --check`: `PASS`, line-ending warnings only
- UI shape changed?: `No layout/skin overhaul; interaction prompts and readbacks only`
- Manual UX proof: `NOT CLAIMED`; checklist added at `docs/runtime/batch88-dungeon-room-interaction-checklist.md`

## Batch 87 Dungeon Route Content Variety v1 Close-Out

- Selected branch: `Branch A - existing safe/risky route data already differs; surface that difference inside the dungeon instead of adding a new content system`
- Why this was the honest next step:
  - Batch85/85.1 proved different route consequences, but the dungeon interior still needed to make Stability and Surge feel different before the final board
  - the existing GoldenPath route definitions, route meanings, room order, encounter profiles, battle setups, outcome meanings, and world board already carried enough source data
  - this pass avoided a new route system, new encounter system, fake consequence, ResultPipeline rewrite, and per-frame rebuild
- Player-facing readbacks added:
  - Route cards and launch/commit summaries now include `Dungeon feel`
  - DungeonRun route risk/readback now keeps the selected route feel visible after launch
  - Encounter result popovers now include `Route Check` so the cleared room points back to the chosen route's internal logic
  - Result/world board proof still reflects the selected route's real consequence, including Surge `mana_shard x20`, `Strained`, and recovery cost
- Route feel distinction:
  - Stability / Rest Path: `Slime Front -> Rest Shrine -> Watch Hall -> Supply Cache -> Quiet Antechamber -> Elite Chamber`, slime-heavy sustain, shrine recovery, lower payout, cleaner recovery
  - Surge / Standard Path: `Mixed Front -> Greed Cache -> Goblin Pair Hall -> Unstable Shrine -> Core Threshold -> Elite Chamber`, mixed pressure, cache-first payout, higher shards, tighter recovery
- Files changed:
  - `Assets/_Game/Scripts/Dungeon/StaticPlaceholderWorldView.DungeonRun.cs`
  - `Assets/_Game/Scripts/Dungeon/StaticPlaceholderWorldView.CompatibilityBridge.cs`
  - `Assets/_Game/Scripts/Dungeon/StaticPlaceholderWorldView.RunState.cs`
  - `Assets/_Game/Scripts/Editor/Batch82RepeatCoreLoopProofRunner.cs`
  - `Assets/_Game/Scripts/Dungeon/DUNGEON_RUN_CONTRACT.md`
  - `Assets/_Game/Scripts/Bootstrap/EXPEDITION_PREP_CONTRACT.md`
  - `docs/architecture/flow-contracts.md`
  - `docs/architecture/validation-matrix.md`
  - `docs/runtime/batch87-dungeon-route-feel-checklist.md`
  - `docs/post-slice-batch-status.md`
- Compile:
  - log: `unity-batch87-compile.log`
  - result: `PASS`
- Authoring validation:
  - log: `unity-batch87-authoring-validation.log`
  - result: `PASS`
  - proof marker: `[AuthoringValidation] Summary`, `PassCount=4`, `FailCount=0`
- Targeted proof:
  - log: `unity-batch87-route-feel-proof.log`
  - result: `PASS`
  - proof markers: `PASS :: Second DungeonRun route readback`, `PASS :: Second route encounter popover`, `PASS :: Batch87 dungeon route feel comparison`
- Regression proof:
  - Batch86 wait cost pressure clock: `PASS`, log `unity-batch87-regression-batch86.log`
  - Batch85.1 surge runtime consequence: `PASS`, log `unity-batch87-regression-batch85_1.log`
  - Batch84 recovery pressure choice: `PASS`, log `unity-batch87-regression-batch84.log`
  - Batch83 second-run decision: `PASS`, log `unity-batch87-regression-batch83.log`
  - Batch82 repeat loop: `PASS`, log `unity-batch87-regression-batch82.log`
  - Batch79.1 route scenario runtime proof: `PASS`, log `unity-batch87-regression-batch79_1.log`
  - Batch80 world result pressure board: `PASS`, log `unity-batch87-regression-batch80.log`
  - Batch78.1 combat core runtime proof: `PASS`, log `unity-batch87-regression-batch78_1.log`
- Batch10 smoke:
  - log: `unity-batch87-smoke.log`
  - result: `PASS`
- Static check:
  - `git diff --check`: `PASS`, line-ending warnings only
- UI shape changed?: `No layout/skin overhaul; route-feel text is projected through existing cards, dungeon readback, popover, and board surfaces`
- Manual UX proof: `NOT CLAIMED`; checklist added at `docs/runtime/batch87-dungeon-route-feel-checklist.md`

## Batch 86 Wait Cost Pressure Clock v1 Close-Out

- Selected branch: `Branch A - use the existing world day / economy tick / recovery rail and make the wait cost readable`
- Why this was the honest next step:
  - Batch84 made `Recover 1 Day` actionable, but the player still needed to understand that waiting spends a world day
  - the existing city need, stock, pressure, dispatch readiness, recovery ETA, and route recommendation rails already carried enough real state
  - this pass avoided a new economy system, new fatigue system, fake wait penalty, ResultPipeline rewrite, and per-frame rebuild
- Player-facing readbacks added:
  - Before waiting: `Wait Cost: City pressure may rise if you recover.`
  - Recover action: `[T] Recover 1 Day: readiness improves, world advances 1 day.`
  - Pressure clock: `city need consumes 1 need stock/day; shard cushion mana_shard xN`
  - After waiting: `readiness Ready, need stock -1, pressure Stable`
  - Recommendation shift: `Next: launch now; waiting again risks shortage.`
- Runtime action:
  - `[T] Recover 1 Day` continues to call the existing world day/economy/recovery rail
  - the proof validates day advance, readiness improvement, actual selected-city need consumption, and route recommendation shift
- Files changed:
  - `Assets/_Game/Scripts/World/StaticPlaceholderWorldView.ExpeditionPrepSurface.cs`
  - `Assets/_Game/Scripts/Editor/Batch82RepeatCoreLoopProofRunner.cs`
  - `Assets/_Game/Scripts/Bootstrap/EXPEDITION_PREP_CONTRACT.md`
  - `Assets/_Game/Scripts/World/WORLDSIM_READMODEL.md`
  - `docs/architecture/flow-contracts.md`
  - `docs/architecture/validation-matrix.md`
  - `docs/runtime/batch86-wait-cost-pressure-clock-checklist.md`
  - `docs/post-slice-batch-status.md`
- Compile:
  - log: `unity-batch86-compile.log`
  - result: `PASS`
- Targeted proof:
  - log: `unity-batch86-wait-cost-proof.log`
  - result: `PASS`
  - proof marker: `PASS :: Wait cost pressure clock`
  - observed update: `BeforeWaiting Day=1 / RecoveryEta=1 day -> AfterWaiting Day=2 / Readiness=Ready / Consumed=1`
- Regression proof:
  - Batch84 recovery pressure choice: `PASS`, log `unity-batch86-regression-batch84.log`
  - Batch83 second-run desire proof: `PASS`, log `unity-batch86-regression-batch83.log`
  - Batch85.1 surge runtime consequence proof: `PASS`, log `unity-batch86-regression-batch85_1.log`
- Batch10 smoke:
  - log: `unity-batch86-smoke.log`
  - result: `PASS`
- Static check:
  - `git diff --check`: `PASS`, line-ending warnings only
- UI shape changed?: `Readback text only; no skin/layout overhaul`
- Manual UX proof: `NOT CLAIMED`; checklist added at `docs/runtime/batch86-wait-cost-pressure-clock-checklist.md`

## Batch 84 Recovery Pressure Choice v1 Close-Out

- Selected branch: `Branch A - use the existing world day / recovery rail and make the second ExpeditionPrep choice readable`
- Why this was the honest next step:
  - Batch83 made the second run tempting, but the player still needed an explicit launch-now vs wait/recover decision
  - the existing world day, dispatch recovery, readiness, pressure, and route appetite rails already had enough real state to support the choice
  - this pass avoided a new fatigue system, fake wait penalty, ResultPipeline rewrite, and per-frame rebuild
- Player-facing readbacks added:
  - Launch now: `Ready`, `Ready with warning`, or `Blocked`
  - Recover 1 Day / Wait 1 Day: one-day wait action plus pressure/economy tick warning
  - After waiting: readiness/recovery preview plus pressure/stock rail reminder
  - After recovery appetite: Stability remains safer, Surge becomes more tempting once readiness clears
- Runtime action:
  - `[T] Recover 1 Day` is available from the ExpeditionPrep board
  - the action calls the existing world day/economy/recovery rail and keeps the prep board open
- Files changed:
  - `Assets/_Game/Scripts/Bootstrap/BootEntry.cs`
  - `Assets/_Game/Scripts/Bootstrap/BootstrapSceneStateBridge.cs`
  - `Assets/_Game/Scripts/Bootstrap/EXPEDITION_PREP_CONTRACT.md`
  - `Assets/_Game/Scripts/Core/PrototypePresentationShell.cs`
  - `Assets/_Game/Scripts/Expedition/ExpeditionPrepSurfaceData.cs`
  - `Assets/_Game/Scripts/World/StaticPlaceholderWorldView.ExpeditionPrepSurface.cs`
  - `Assets/_Game/Scripts/Editor/Batch82RepeatCoreLoopProofRunner.cs`
  - `docs/runtime/batch84-recovery-pressure-choice-v1.md`
  - `docs/runtime/batch84-recovery-pressure-choice-checklist.md`
  - `docs/architecture/flow-contracts.md`
  - `docs/architecture/validation-matrix.md`
  - `docs/post-slice-batch-status.md`
- Compile:
  - log: `unity-batch84-compile.log`
  - result: `PASS`
- Targeted proof:
  - log: `unity-batch84-recovery-pressure-proof.log`
  - result: `PASS`
  - proof marker: `PASS :: Recover 1 Day choice`
  - observed update: `BeforeDay=1 | AfterDay=2`, readiness became `Ready / low recovery risk`
- Regression proof:
  - Batch83 second-run desire proof: `PASS`, log `unity-batch84-batch83-proof.log`
- Batch10 smoke:
  - log: `unity-batch84-smoke.log`
  - result: `PASS`
- Static check:
  - `git diff --check`: `PASS`, line-ending warnings only
- UI shape changed?: `Small action-surface addition only; no skin/layout overhaul`
- Manual UX proof: `NOT CLAIMED`; checklist added at `docs/runtime/batch84-recovery-pressure-choice-checklist.md`
- Detailed proof note: `docs/runtime/batch84-recovery-pressure-choice-v1.md`

## Batch 83 Second-Run Decision Pressure v1 Close-Out

- Selected branch: `Branch A - the two-run loop works, but the second ExpeditionPrep needed stronger player-facing choice appetite`
- Why this was the honest next step:
  - Batch82 proved repeatability, but the next product question was whether the player wants to make the second choice
  - existing result, city pressure, party growth, equipment, route, and launch-warning data already supported the prompt
  - this pass could stay display/readback-only without adding mechanics, content, layout, skin, economy, or ResultPipeline work
- Player-facing readbacks added:
  - Last run changed: `Returned mana_shard x16 | Party Stable`
  - Party carry-forward: `Alden +16 XP | Rune equipped Stormglass Focus`
  - Stability appetite: `protect HP and keep dispatch rhythm steady`
  - Surge appetite: `chase payout while Rune's new focus improves burst`
  - Launch warning: `Ready with warning / recovery risk / fatigue`
  - Recommendation: `Stability if recovery matters, Surge if stock pressure still outweighs strain`
- Files changed:
  - `Assets/_Game/Scripts/Expedition/ExpeditionPrepSurfaceData.cs`
  - `Assets/_Game/Scripts/World/StaticPlaceholderWorldView.ExpeditionPrepSurface.cs`
  - `Assets/_Game/Scripts/Core/PrototypePresentationShell.cs`
  - `Assets/_Game/Scripts/Editor/Batch82RepeatCoreLoopProofRunner.cs`
  - `Assets/_Game/Scripts/Bootstrap/EXPEDITION_PREP_CONTRACT.md`
  - `docs/runtime/batch83-second-run-decision-pressure-v1.md`
  - `docs/ui/two-run-feel-qa-checklist.md`
  - `docs/architecture/flow-contracts.md`
  - `docs/architecture/validation-matrix.md`
  - `docs/post-slice-batch-status.md`
- Compile:
  - log: `unity-batch83-compile.log`
  - result: `PASS`
- Targeted proof:
  - log: `unity-batch83-two-run-feel-proof.log`
  - result: `PASS`
  - proof marker: `PASS :: Second ExpeditionPrep context`
- Batch10 smoke:
  - log: `unity-batch83-smoke.log`
  - result: `PASS`
- Static check:
  - `git diff --check`: `PASS`, line-ending warnings only
- UI shape changed?: `No new layout or skin; existing panels now consume compact decision readback lines`
- Manual UX proof: `NOT CLAIMED`; checklist added at `docs/ui/two-run-feel-qa-checklist.md`
- Detailed proof note: `docs/runtime/batch83-second-run-decision-pressure-v1.md`

## Batch 82 Repeatable Core Game Loop v1 Close-Out

- Selected branch: `Branch A - the loop already mostly repeats; lock the two-run contract proof without adding systems`
- Why this was the honest next game-development step:
  - the vertical slice had become presentable, but the next product question was whether it could repeat as a game loop
  - existing result, world board, party growth, equipment, route, and prep contracts already had enough state to inform a second run
  - the safest move was to prove and document repeatability instead of inventing a new city, dungeon, economy, combat, inventory, or UI system
- Preflight:
  - Vertical slice: `PASS through Batch81 playbook/proof baseline and Batch10 rerun`
  - Pressure board: `PASS; Latest/Why/Changed/Next/Ready/Route remain readable after result return`
  - Re-entry: `PASS; CityHub -> ExpeditionPrep re-entry stays coherent after result return`
  - Packaging: `PASS; mission/relevance/risk context stays preserved through ResultPipeline`
  - Combat: `PASS; Batch78.1 burst/payoff proof remains green`
  - Modal/performance: `PASS through Batch78.1 UI/modal/skin sanity and static scope; no new per-frame rebuild path`
- Repeat loop implementation:
  - First prep: `city-a -> dungeon-alpha route cards expose Stability Run and Surge Window plus party/loadout/gate context`
  - First run/result: `safe route launches through confirmed ExpeditionPlan and returns mana_shard x16`
  - World return: `CityHub board shows Cleared Stability Run, Stock +16, pressure/readiness shift, next action, and warning-ready state`
  - Second prep: `latest result, Alden XP, Rune equipped Stormglass Focus, loadout, safe/risky options, warning gate, and recommendation reason carry forward`
  - Second-run readiness: `launch is allowed with warning because city readiness is not fully recovered`
  - Next action: `second DungeonRun can start through the same confirmed plan seam`
- Files changed:
  - `Assets/_Game/Scripts/Editor/Batch82RepeatCoreLoopProofRunner.cs`
  - `Assets/_Game/Scripts/Editor/Batch82RepeatCoreLoopProofRunner.cs.meta`
  - `docs/runtime/batch82-repeatable-core-loop-v1.md`
  - `docs/architecture/validation-matrix.md`
  - `docs/post-slice-batch-status.md`
- Compile:
  - log: `unity-batch82-compile.log`
  - result: `PASS`
- Targeted proof:
  - log: `unity-batch82-repeat-loop-proof.log`
  - result: `PASS`
  - proof marker: `PASS :: Repeat core loop proof completed`
- Batch10 smoke:
  - log: `unity-batch82-smoke.log`
  - result: `PASS`
- Regression proof:
  - Batch80.1 board: `PASS`, log `unity-batch82-board-proof.log`
  - Batch79.3 re-entry: `PASS`, log `unity-batch82-reentry-proof.log`
  - Batch79.2 packaging: `PASS`, log `unity-batch82-packaging-proof.log`
  - Batch78.1 combat/modal/skin: `PASS`, log `unity-batch82-combat-proof.log`
- Performance proof:
  - static scope: `no ResultPipeline UI rebuild, no per-frame asset scan/load, no route/content/inventory/economy/UI layout expansion`
  - runtime: repeat-loop proof reaches selected CityHub, second prep, second route select, and second launch without timeout
- UI shape changed?: `No`
- What the player can now do repeatedly:
  - clear a run, return with loot/growth/equipment and changed city pressure, then open second prep and launch again with the updated context visible
- Remaining game-loop weakness:
  - repeatability is proven for the first two-run loop, but long-run motivation still needs manual feel QA and future pacing/content decisions
- Recommended next batch:
  - `manual two-run feel QA, then Batch83 run-to-run motivation polish`
- Detailed proof note: `docs/runtime/batch82-repeatable-core-loop-v1.md`

## Batch 81 Presentation Vertical Slice Lock Close-Out

- Selected branch: `Branch A - the full Alpha Stability Run demo loop already works; lock it with playbook/proof notes instead of adding systems`
- Why this was the honest next step:
  - the four core pillars now have first-pass proof, so the next risk is presentation coherence rather than missing mechanics
  - the preferred `city-a -> dungeon-alpha -> safe` path already carries city pressure, route scenario, party role combat, result writeback, pressure board, and re-entry
  - no structural ResultPipeline cleanup or new feature should happen before the accepted demo path is recordable
- Preflight:
  - Party growth: `PASS through result/readback lines carrying Alden/Mira progression and differentiated combat roles`
  - Combat core: `PASS through Batch78.1 proof; Burst Window setup/payoff remains intact`
  - Route scenario: `PASS through Batch79.1 proof; Stability Run route card, dungeon readback, and encounter popover remain visible`
  - World pressure board: `PASS through Batch80.1 proof; Latest/Why/Changed/Next/Ready/Route remain readable`
  - Regression: `Batch10, ResultPipeline packaging, CityHub re-entry, UI/modal/skin sanity all PASS`
- Chosen demo path: `city-a -> dungeon-alpha -> safe / Stability Run / Slime Front / Alden Burst Window -> Mira payoff / mana_shard return / CityHub pressure board / ExpeditionPrep re-entry`
- Demo playbook:
  - File: `docs/demo/project-aaa-vertical-slice-playbook.md`
  - Steps: `MainMenu -> World -> City A -> ExpeditionPrep -> Stability Run -> DungeonRun -> Slime Front payoff -> result return -> pressure board -> re-entry`
  - Expected lines: `Cleared: Stability Run`, `Stock +16 mana_shard`, `Stabilize for 1 day before the next push`, `Ready: warning | recovery 1 day | party idle | route available`, `Alden read intent...`, `Mira cashed Burst Window...`
- Fixes, if any: `None; docs/status/proof lock only`
- Files changed:
  - `docs/demo/project-aaa-vertical-slice-playbook.md`
  - `docs/runtime/batch81-presentation-vertical-slice-lock.md`
  - `docs/architecture/validation-matrix.md`
  - `docs/post-slice-batch-status.md`
- Compile:
  - log: `unity-batch81-compile.log`
  - result: `PASS`
- Batch10 smoke:
  - log: `unity-batch81-smoke.log`
  - result: `PASS`
- Targeted vertical slice proof:
  - Route scenario proof: `PASS`, log `unity-batch81-route-proof.log`
  - Combat payoff proof: `PASS`, log `unity-batch81-combat-proof.log`
  - Board readability/re-entry proof: `PASS`, log `unity-batch81-board-proof.log`
  - Packaging proof: `PASS`, log `unity-batch81-packaging-proof.log`
  - Re-entry proof: `PASS`, log `unity-batch81-reentry-proof.log`
- Manual UX proof: `NOT CLAIMED; playbook is ready for manual recording sanity pass`
- Performance proof:
  - static scope: `no new per-frame rebuilds, no asset scans, no UI skin/layout work, no ResultPipeline expansion`
  - runtime: existing Batch10/proofs completed without new timeout or failure
- Known limitations:
  - full manual dungeon clear can be longer than a short presentation; playbook recommends showing first combat payoff then using proof/smoke result-return path for the concise recording
  - runtime skin/fallback visuals remain prototype presentation, not final art
  - human target-resolution clipping check is still needed before recording
- Can presentation recording begin?: `Yes, after one manual screenshot/recording sanity pass`
- Why / why not:
  - `Yes` because the accepted Alpha path is documented and all relevant compile/smoke/targeted proof rails are green
- Recommended next batch:
  - `manual presentation recording sanity pass, then small demo polish fixes or post-demo structural cleanup`
- Detailed proof note: `docs/runtime/batch81-presentation-vertical-slice-lock.md`

## Batch 80.1 Selected CityHub Pressure Board Readability UX Close-Out

- Selected branch: `Branch A - board had the right data, but text density and ordering were weak`
- Why this was the honest next step:
  - Batch80 was accepted as a systems/readback milestone, but the proof output still showed one dense board sentence plus repeated selected-board lines
  - the next player-facing issue was scanability, not missing ResultPipeline/world data
  - this pass could stay in copy/order/readback consumers without changing gameplay, content, UI skin, or result contracts
- Preflight:
  - Batch80 surfaces: `PASS static audit; RecentResultEvidenceText, PressureChangeText, PartyReadinessSummaryText, and selected UI consumers were located`
  - Runtime/display: `PASS through targeted board proof; post-run selected CityHub now shows compact Latest/Changed/Next/Ready lines`
  - Regression: `Batch10 PASS; re-entry/result-world return smoke checkpoints remain PASS`
- Readability changes:
  - Latest: changed from internal-key style `Last run ... (run_clear)` to player-facing `Cleared: Stability Run | Returned mana_shard x16 | Party Stable`
  - Changed: reordered compact evidence to `Stock +N`, pressure delta, readiness delta
  - Next: continues to use existing CityHub/outcome follow-up data
  - Ready: compacted to `Ready: warning | recovery 1 day | party idle | route available` or a clear blocked reason
  - Duplicates removed: selected CityHub presentation no longer shows a dense `Pressure Board:` line and then repeats `Changed`, `Latest`, `Next`, and readiness below it
- Files changed:
  - `Assets/_Game/Scripts/World/StaticPlaceholderWorldView.WorldSnapshotContext.cs`
  - `Assets/_Game/Scripts/Core/PrototypePresentationShell.cs`
  - `Assets/_Game/Scripts/Editor/Batch80WorldResultPressureBoardProofRunner.cs`
  - `docs/ui/cityhub-pressure-board-ux-checklist.md`
  - `docs/runtime/batch80-1-cityhub-pressure-board-readability-ux.md`
  - `docs/architecture/validation-matrix.md`
  - `docs/post-slice-batch-status.md`
- Compile:
  - log: `unity-batch80-1-compile.log`
  - result: `PASS`
- Targeted proof:
  - log: `unity-batch80-1-board-proof.log`
  - result: `PASS`
  - proof marker: `PASS :: Pressure board answers five questions`
- Manual UX proof:
  - `NOT CLAIMED`; checklist added at `docs/ui/cityhub-pressure-board-ux-checklist.md`
- Performance proof:
  - no ResultPipeline expansion, no per-frame heavy rebuild, no mouse-move/no-op selection rebuild, no UI skin or broad layout work
- Batch10/regression proof:
  - log: `unity-batch80-1-smoke.log`
  - result: `PASS`
  - key regression markers: `ResultPipeline -> WorldSim board refresh`, `World return chain causal summary`, `CityHub -> ExpeditionPrep re-entry continuity`
- UI shape changed?: `No broad layout/skin redesign; selected board copy order and density changed`
- What the player can now understand:
  - at a glance: what came back, why City A cares, what changed, what to do next, and whether prep/re-entry is ready or warning-blocked
- Remaining issue:
  - human screenshot QA is still needed to catch actual clipping/overlap at target resolution
- Recommended next batch:
  - `manual screenshot QA follow-up, then Batch81 demo flow polish or the next roadmap priority`
- Detailed proof note: `docs/runtime/batch80-1-cityhub-pressure-board-readability-ux.md`

## Batch 80 World Result-Pressure Board Close-Out

- Selected branch: `Branch A, with narrow Branch C/D readback work; existing outcome/world writeback data was present but not clear enough on the selected city board`
- Why this was the honest next problem:
  - Batch79.3 closed the CityHub -> ExpeditionPrep re-entry blocker, so the next player-facing gap was not result production but city/world explanation
  - ResultPipeline, WorldWriteback, OutcomeReadback, CityDecision, and LaunchReadiness already had enough source data; the selected CityHub board needed clearer aggregation
  - the work could stay display/readback-only without new economy, combat, route, inventory, or ResultPipeline systems
- Preflight:
  - Batch79.3 re-entry: `PASS`, proof re-run in `unity-batch80-reentry-proof.log`
  - Batch79.2 packaging: `PASS`, proof re-run in `unity-batch80-packaging-proof.log`
  - Batch79 route scenario: `PASS`, proof re-run in `unity-batch80-route-proof.log`
  - Batch78.1 combat: `PASS`, proof re-run in `unity-batch80-combat-proof.log`
  - UI/modal/perf: `PASS through Batch78.1 UI/modal/skin sanity; no per-frame result/selection rebuild added`
- Board focus: `selected CityHub / selected city world board`
- Board implementation:
  - What happened: `RecentResultEvidenceText` now prefers latest result route/scenario, result key, returned loot, and party condition
  - Why it mattered: selected board continues to use `CityDecisionReadModel.WhyCityMattersText`
  - What changed: new `PressureChangeText` carries need pressure, dispatch readiness, streak, and stock evidence derived from existing city/result readbacks
  - Next action: priority and selected board summaries use existing CityHub recommendation / outcome follow-up as the next action line
  - Readiness/re-entry: `PartyReadinessSummaryText` now states ready, warning, or blocked status using idle party, contract slot, readiness, and recovery data
- Data/cache policy:
  - board copy is derived from `WorldBoardReadModel`, `CityStatusReadModel.Decision`, `ExpeditionResultReadModel`, `OutcomeReadback`, `WorldWriteback`, and `LaunchReadiness`
  - no new ResultPipeline fields, world pressure system, UI skin/layout work, scene migration, route content, combat mechanic, or per-frame rebuild path was added
- Files changed:
  - `Assets/_Game/Scripts/World/StaticPlaceholderWorldView.WorldSnapshotContext.cs`
  - `Assets/_Game/Scripts/World/StaticPlaceholderWorldView.OutcomeReadbackSurface.cs`
  - `Assets/_Game/Scripts/World/WorldObservationSurfaceData.cs`
  - `Assets/_Game/Scripts/City/CityHubSurfaceData.cs`
  - `Assets/_Game/Scripts/City/PrototypeCityHubUiSurfaceData.cs`
  - `Assets/_Game/Scripts/City/CityInteraction.cs`
  - `Assets/_Game/Scripts/Core/PrototypePresentationShell.cs`
  - `Assets/_Game/Scripts/Editor/Batch80WorldResultPressureBoardProofRunner.cs`
  - `docs/runtime/batch80-world-result-pressure-board-proof.md`
  - `docs/architecture/flow-contracts.md`
  - `docs/architecture/validation-matrix.md`
  - `Assets/_Game/Scripts/World/WORLDSIM_READMODEL.md`
  - `Assets/_Game/Scripts/World/CITYHUB_DECISION_LAYER.md`
  - `docs/post-slice-batch-status.md`
- Compile:
  - log: `unity-batch80-compile.log`
  - result: `PASS`
- Targeted proof:
  - log: `unity-batch80-board-proof.log`
  - result: `PASS`
  - proof markers: `World return -> CityHub pressure board`, `Pressure board answers five questions`, `Readiness/re-entry`, `Targeted board proof completed`
- Manual UX proof: `AUTOMATED Play Mode proof only; human visual pass not claimed`
- Batch10 smoke:
  - log: `unity-batch80-smoke.log`
  - result: `PASS`
- Regression proof:
  - Batch79.3 re-entry: `PASS`, log `unity-batch80-reentry-proof.log`
  - Batch79.2 packaging: `PASS`, log `unity-batch80-packaging-proof.log`
  - Batch79.1 route scenario: `PASS`, log `unity-batch80-route-proof.log`
  - Batch78.1 combat: `PASS`, log `unity-batch80-combat-proof.log`
  - Inventory modal Target Status / runtime skin bridge: `PASS through Batch78.1 UI/modal/skin sanity`
- Performance proof:
  - City selection after result: `PASS through targeted board proof returning to selected CityHub and reading the board`
  - World/CityHub board: `static PASS; no new OnGUI, mouse-move, Update, asset scan, or ResultPipeline rebuild path`
  - `git diff --check`: `PASS with CRLF warnings only`
- UI shape changed?: `No layout, skin, or scene migration work; only existing selected board copy/surface fields changed`
- What the player can now feel:
  - the run came back, City A's pressure changed, the board says why it mattered, and it points to the next sensible dispatch/recovery action
- Recommended next batch:
  - `manual UX review of selected CityHub pressure board density/readability, then pick the next roadmap feature with Batch10 still green`
- Detailed proof note: `docs/runtime/batch80-world-result-pressure-board-proof.md`

## Batch 79.3 CityHub -> ExpeditionPrep Re-entry Continuity Triage Close-Out

- Selected branch: `Branch A primary runtime AppFlow prep-cache fix, plus narrow Branch B smoke-contract update after the runtime gap was closed`
- Why this was the honest next step:
  - Batch79.2 fixed ResultPipeline intent packaging and exposed the next full-smoke blocker at `CityHub -> ExpeditionPrep re-entry continuity`
  - Batch80 depends on result return, CityHub evidence, selected city/dungeon identity, prep re-entry, route options, and launch readiness all agreeing
  - the issue was a contract/cache seam, not a reason to start the world result-pressure board feature early
- Failure audit:
  - failing smoke step: `Batch10SmokeValidationRunner.ValidatePrepReentryContinuity()` / `CityHub -> ExpeditionPrep re-entry continuity`
  - expected: after result return, AppFlow's active prep read model carries selected city `city-a`, target dungeon `dungeon-alpha`, returned recent impact, recommendation summary/reason, and why-now text
  - actual: stage reached `ExpeditionPrep` and `SelectedCity=city-a`, but AppFlow's prep read model still had `TargetDungeon=None`, `ImpactSummary=None`, `ImpactHint=None`, `Recommendation=None`, `RecommendationReason=None`, and `WhyNow=None`
  - root cause: visible prep surface rebuilt through `_isExpeditionPrepBoardOpen`, while AppFlow read-model caching still depended on `_dungeonRunState == DungeonRunState.RouteChoice`
- Re-entry contract:
  - selection identity: `city-a -> dungeon-alpha` survives result return and prep re-entry
  - result evidence: returned impact/recommendation/why-now live in `ExpeditionPrepReadModel`; route aftermath text may echo the last result/route
  - dispatch readiness: `LaunchReadiness` remains the route-prompt/commit-gate source, including warnings
  - route options: Alpha safe/risky route cards remain visible
  - post-run reveal: no bypass added; proof uses the existing world/city/prep path
- Fix summary:
  - cache `ExpeditionPrepReadModel` on explicit prep-open, route-change, and dispatch-policy-change points
  - update Batch10 narrowly so route prompt may be launch gate text while returned-result evidence is asserted through prep model plus route aftermath echo
  - add `Batch79_3CityHubExpeditionPrepReentryProofRunner`
- Files changed:
  - `Assets/_Game/Scripts/World/StaticPlaceholderWorldView.ExpeditionPrepSurface.cs`
  - `Assets/_Game/Scripts/Dungeon/StaticPlaceholderWorldView.DungeonRun.cs`
  - `Assets/_Game/Scripts/Editor/Batch10SmokeValidationRunner.cs`
  - `Assets/_Game/Scripts/Editor/Batch79_3CityHubExpeditionPrepReentryProofRunner.cs`
  - `docs/runtime/batch79-3-cityhub-expeditionprep-reentry-continuity-triage.md`
  - `docs/architecture/flow-contracts.md`
  - `docs/architecture/validation-matrix.md`
  - `Assets/_Game/Scripts/Bootstrap/EXPEDITION_PREP_CONTRACT.md`
  - `docs/post-slice-batch-status.md`
- Compile:
  - log: `unity-batch79-3-compile.log`
  - result: `PASS`
- Targeted re-entry proof:
  - log: `unity-batch79-3-reentry-proof.log`
  - result: `PASS`
  - proof marker: `PASS :: CityHub -> ExpeditionPrep re-entry continuity`
- Batch10 smoke:
  - log: `unity-batch79-3-smoke.log`
  - ResolveCoreLoop: `PASS`
  - ResultPipeline packaging: `PASS`
  - CityHub -> ExpeditionPrep: `PASS`
  - full smoke status: `PASS`
- Regression proof:
  - Batch79.1 route scenario: `PASS`, log `unity-batch79-3-route-proof.log`
  - Batch78.1 combat: `PASS`, log `unity-batch79-3-combat-proof.log`
  - Batch79.2 packaging: `PASS`, log `unity-batch79-3-packaging-proof.log`
  - Inventory modal: `PASS through Batch78.1 UI/modal/skin regression sanity`
  - Runtime skin bridge: `PASS through Batch78.1 UI/modal/skin regression sanity`
  - World selection: `UNCHANGED; no new per-selection or per-frame prep rebuild path`
- Performance impact:
  - no `OnGUI`, mouse-move, world-selection, inventory-scan, asset-load, or result-pipeline per-frame rebuild was added
  - prep cache refresh now occurs only on explicit prep open, route change, and dispatch policy change
- Can Batch80 begin?: `Yes`
- Why / why not:
  - `Yes` because the previous CityHub -> ExpeditionPrep full-smoke blocker is closed and the adjacent ResultPipeline, route, combat, modal, skin, and world-selection guards remain stable
- Detailed proof note: `docs/runtime/batch79-3-cityhub-expeditionprep-reentry-continuity-triage.md`

## Batch 79.2 ResultPipeline Intent Packaging Smoke Triage Close-Out

- Selected branch: `fix the intent packaging seam before Batch80; do not start world pressure board work`
- Why this was the honest next step:
  - Batch79.1 cleared the stale core-loop smoke timeout and exposed the next real failure at `DungeonRun -> ResultPipeline intent packaging`
  - the failing fields were mission intent fields: objective, relevance, and risk/reward text drifted into key-encounter or world-writeback summaries after run clear
  - the fix needed to preserve the existing DungeonRun, ResultPipeline, WorldSim, and CityHub contracts without adding a new world-pressure board surface
- What changed:
  - `PostRunResolutionInput`, `ExpeditionResult`, and `PrototypeDungeonRunResultContext` now carry `MissionObjectiveText`, `MissionRelevanceText`, and `RiskRewardContextText`
  - DungeonRun result input now fills those fields from the canonical run state, with prep-surface fallback only when the run state is missing a meaningful line
  - `ResultPipeline.BuildExpeditionOutcome(ExpeditionResult)` centralizes public outcome reconstruction so `ManualTradeRuntimeState` no longer duplicates stale mapping logic
  - Batch10 world-return smoke no longer reflects against the retired private `BootEntry._appFlowCoordinator` field and accepts the current shell flow returning to `WorldSim` or immediately observed `CityHub`
- Files changed:
  - `Assets/_Game/Scripts/Dungeon/StaticPlaceholderWorldView.DungeonRun.cs`
  - `Assets/_Game/Scripts/Dungeon/StaticPlaceholderWorldView.DungeonRun.ResultPipelineBridge.cs`
  - `Assets/_Game/Scripts/Dungeon/StaticPlaceholderWorldView.DungeonRun.ShellContracts.cs`
  - `Assets/_Game/Scripts/Editor/Batch10SmokeValidationRunner.cs`
  - `Assets/_Game/Scripts/Editor/Batch79_2ResultPipelineIntentPackagingProofRunner.cs`
  - `Assets/_Game/Scripts/Results/ExpeditionResult.cs`
  - `Assets/_Game/Scripts/Results/PostRunResolutionInput.cs`
  - `Assets/_Game/Scripts/Results/ResultPipeline.cs`
  - `Assets/_Game/Scripts/World/ManualTradeRuntimeState.cs`
  - `docs/runtime/batch79-2-result-pipeline-intent-packaging-smoke-triage.md`
  - `docs/architecture/flow-contracts.md`
  - `docs/architecture/validation-matrix.md`
  - `Assets/_Game/Scripts/Dungeon/DUNGEON_RUN_CONTRACT.md`
  - `docs/post-slice-batch-status.md`
- Compile:
  - log: `unity-batch79-2-compile.log`
  - result: `PASS`
- Targeted ResultPipeline proof:
  - log: `unity-batch79-2-packaging-proof.log`
  - result: `PASS`
  - proof marker: `[Batch79_2Proof] PASS :: Package/consumer fields stable`
- Batch10 smoke:
  - log: `unity-batch79-2-smoke.log`
  - result: `PARTIAL`
  - fixed checkpoint: `PASS :: DungeonRun -> ResultPipeline intent packaging`
  - additional passed checkpoints after the fix: `ResultPipeline -> WorldSim board refresh`, `WorldSim -> CityHub recent impact coherence`, `WorldSim -> CityHub decision relevance carry-through`, and `World return chain causal summary`
  - later failure: `FAIL :: CityHub -> ExpeditionPrep re-entry continuity`
  - later failure scope: prep re-entry carries route description/city impact/recommendation on route-facing fields, but the smoke still observes top prep continuity fields as `None`; this is a separate next triage, not the Batch79.2 ResultPipeline packaging seam
- Regression proof:
  - Batch79.1 route scenario proof: `PASS`, log `unity-batch79-2-route-proof.log`
  - Batch78.1 combat core proof: `PASS`, log `unity-batch79-2-combat-proof.log`
  - UI/modal/skin sanity remains covered by Batch78.1 proof: `RuntimeUiSkinBridge=present`, top-strip fallback true, inventory overlay not open during battle proof
- UI shape changed?: `No runtime UI layout or skin replacement work`
- Can Batch80 begin?: `Yes for ResultPipeline packaging risk; no claim of full Batch10 green until the CityHub -> ExpeditionPrep re-entry continuity failure is triaged`
- Detailed proof note: `docs/runtime/batch79-2-result-pipeline-intent-packaging-smoke-triage.md`

## Batch 79.1 Route Scenario Runtime Proof + Smoke Triage Close-Out

- Selected branch: `C - ResolveCoreLoop timeout was caused by a stale smoke wait condition`
- Why this was the honest next step:
  - Batch 79 had the right route-scenario direction, but only static proof plus a partial Batch10 smoke
  - the player-facing question was whether `Stability Run` is actually visible in a short flow
  - the engineering question was whether the Batch10 timeout was a Batch79 regression or an old harness wait issue
- Preflight:
  - Batch79 surfaces: `PASS static producer/consumer audit; route card, commit/start context, dungeon shell, encounter popover, and final payoff consumers are wired`
  - Batch10 timeout: `root cause was stale smoke visibility checks that only read bootstrap booleans while current dungeon shell state exposes event/pre-elite visibility`
  - Regression checks: `combat payoff, inventory modal, runtime skin bridge, world selection cache, pending spoils/extraction policy unchanged`
- Runtime/static proof:
  - Route card: `PASS`, captured `Rest Path | Stability Run` plus `Combat: Expect slime-heavy sustain fights, stronger shrine value, and a steadier elite setup`
  - Commit summary: `PASS`, captured `RoutePreview=Rest Path | Stability Run`, `LaunchGate=Commit allowed. Launch window is clear.`, and `StartBattleWatch=Rest Path | Combat: Expect slime-heavy sustain fights...`
  - Dungeon readback: `PASS`, captured `Route Plan=Rest Path | Stability Run`, `Route Risk=Safer | Slime-heavy / Recover-friendly`, `Battle Watch=Rest Path | Combat...`, `Next=Reach Elite`
  - Encounter popover: `PASS`, captured `RoutePlan=Rest Path | Stability Run | Low Risk | Combat: Expect slime-heavy sustain fights... | Follow-up: Usually leaves the next dispatch cleaner...`
  - Final scenario payoff: `PASS static consumer proof`; final popover and result panel paths still consume `BuildSelectedRouteScenarioPayoffText(outcomeKey)` and `BuildDungeonScenarioPayoffLine(resultContext)`
- Smoke triage:
  - Passed: `Boot -> MainMenu`, `MainMenu -> WorldSim`, `WorldSim -> CityHub`, representative content catalog, `CityHub -> ExpeditionPrep`, `ExpeditionPrep -> DungeonRun launch`, `DungeonRun -> BattleScene intent carry-through`, first battle return, and full route progression into run-clear result packaging
  - Timeout: `FIXED`; no `ResolveCoreLoop` timeout after the shell-surface visibility fix
  - Cause: `Batch10 was looking at retired bootstrap event/pre-elite visibility booleans instead of current dungeon shell surface state`
  - Fixed or deferred: `fixed narrowly for the timeout; later full-smoke failure deferred as ResultPipeline packaging mismatch`
  - Later failure: `FAIL :: DungeonRun -> ResultPipeline intent packaging`, with objective/relevance/risk-reward mismatch after run clear
  - Later failure scope: `not caused by Batch79 route scenario text; no ResultPipeline, world mutation, combat, spoils, or UI skin path was changed`
- Files changed:
  - `Assets/_Game/Scripts/Editor/Batch10SmokeValidationRunner.cs`
  - `Assets/_Game/Scripts/Editor/Batch79_1RouteScenarioRuntimeProofRunner.cs`
  - `docs/runtime/batch79-1-route-scenario-runtime-proof.md`
  - `docs/post-slice-batch-status.md`
  - `docs/architecture/validation-matrix.md`
- Compile:
  - log: `unity-merge-validate.log`
  - result: `PASS`
  - proof markers:
    - `*** Tundra build success`
    - `Batchmode quit successfully invoked - shutting down!`
    - `Exiting batchmode successfully now!`
- Targeted proof:
  - log: `unity-batch79-1-route-proof.log`
  - result: `PASS`
  - proof markers: route card, commit summary, dungeon readback, encounter popover, final static consumer proof all `PASS`
- Manual UX proof: `AUTOMATED Play Mode proof captured; human screenshot proof still not claimed`
- Regression proof: `static unchanged for 78.1 combat, inventory modal Target Status guard, runtime skin bridge, world selection cache, and pending spoils/extraction policy`
- Performance proof: `editor-only proof runner plus existing smoke tick shell-surface read; no new runtime per-frame route scenario rebuild`
- UI shape changed?: `No runtime UI shape change`
- Can Batch80 begin?: `Yes for route-scenario risk, with one caveat`
- Why / why not:
  - `Yes` because Batch79.1 proves the selected Alpha safe route scenario is visible from route card through first encounter popover, and the original ResolveCoreLoop timeout is fixed
  - `Caveat` full Batch10 is still red at a later ResultPipeline packaging checkpoint, so if the lead requires full Batch10 green before priority 4, open a narrow ResultPipeline smoke/contract triage before Batch80
- Detailed proof note: `docs/runtime/batch79-1-route-scenario-runtime-proof.md`

## Batch 79 Dungeon Route Operating Scenario Close-Out

- Selected branch: `A - existing route meaning authoring was present but not visible enough`
- Chosen route pair: `city-a -> dungeon-alpha safe/risky`
- Why this was the honest next problem:
  - Batch 78.1 proved the first combat payoff loop, so the next player-facing gap was why a route choice should feel like a plan before, during, and after the run
  - Alpha is the shortest manual demo path, and its existing safe/risky content already had route meaning text that could be surfaced without widening the route portfolio
  - the weak seam was visibility, not missing combat/RPG/world systems
- Preflight:
  - 78.1 combat: `PASS static preservation; Burst Window formula/payoff path was not changed`
  - RPG/role: `PASS static preservation; party role/growth/equipment truth was not changed`
  - UI/modal/skin: `PASS static preservation; runtime skin bridge, inventory Target Status guard, result popover modality, and top-strip fallback paths were not changed`
  - Performance: `PASS static preservation; no new Update loop, per-frame asset loading, Resources scan, or world selection cache rebuild`
- Scenario implementation:
  - Scenario label: `Stability Run` for Alpha safe and `Surge Window` for Alpha risky now remain visible through route summaries
  - Choose-when: existing route preview text remains the player-facing reason line on prep/launch surfaces
  - Party fit: existing route reward/fit preview remains on route choice details
  - Combat plan: prep route cards now show compact `Combat Plan` text from the route meaning rail
  - Risk/follow-up: existing risk/reward/follow-up text remains surfaced and is reused in route-plan/result readback
  - Commit summary: launch/start context keeps selected scenario and event preview readback on the dungeon handoff
  - Dungeon readback: dungeon explore sidebar now shows `Route Plan`, `Route Risk`, `Battle Watch`, and `Next Goal`
  - Encounter/result follow-through: battle result popover now has one compact `Route Plan` line, and final result now has `Scenario Payoff`
- Files changed for Batch 79:
  - `Assets/_Game/Scripts/Expedition/ExpeditionPrepSurfaceData.cs`
  - `Assets/_Game/Scripts/World/StaticPlaceholderWorldView.ExpeditionPrepSurface.cs`
  - `Assets/_Game/Scripts/Dungeon/StaticPlaceholderWorldView.CompatibilityBridge.cs`
  - `Assets/_Game/Scripts/Core/PrototypePresentationShell.cs`
  - `Assets/_Game/Scripts/Dungeon/StaticPlaceholderWorldView.DungeonRun.ShellContracts.cs`
  - `Assets/_Game/Scripts/Dungeon/StaticPlaceholderWorldView.DungeonRun.cs`
  - `Assets/_Game/Scripts/UI/Battle/PrototypePresentationShell.DungeonRun.cs`
  - `Assets/_Game/Scripts/Bootstrap/EXPEDITION_PREP_CONTRACT.md`
  - `Assets/_Game/Scripts/Dungeon/DUNGEON_RUN_CONTRACT.md`
  - `docs/content/README.md`
  - `docs/architecture/flow-contracts.md`
  - `docs/architecture/validation-matrix.md`
  - `docs/runtime/batch79-route-operating-scenario-proof.md`
- Compile:
  - log: `unity-merge-validate.log`
  - result: `PASS`
  - proof markers:
    - `*** Tundra build success`
    - `Batchmode quit successfully invoked - shutting down!`
    - `Exiting batchmode successfully now!`
- Targeted proof:
  - authoring validation log: `unity-batch79-authoring.log`
  - result: `PASS`
  - proof markers: `PassCount=4`, `WarnCount=0`, `FailCount=0`
  - Alpha safe resolves `route-steady-rest`, `outcome-mana-shard-city-a`, `city-decision-city-a-shard-stability`, `encounter-profile-alpha-safe-entry`, and `battle-setup-alpha-safe-room1`
  - Alpha risky resolves `route-balanced-volatile`, `outcome-mana-shard-city-a-surge`, `city-decision-city-a-shard-surge-window`, `encounter-profile-alpha-risky-breach`, and `battle-setup-alpha-risky-room1`
- Manual UX proof: `DEFERRED; exact repro steps recorded in docs/runtime/batch79-route-operating-scenario-proof.md`
- Automated smoke:
  - log: `unity-batch79-smoke.log`
  - result: `PARTIAL`
  - passed before timeout: Boot -> MainMenu, MainMenu -> WorldSim, WorldSim -> CityHub, CityHub -> ExpeditionPrep, ExpeditionPrep -> DungeonRun launch, DungeonRun -> BattleScene intent carry-through, and first encounter return
  - blocker: `FAIL :: Smoke runner :: Timed out while waiting for step ResolveCoreLoop` after first encounter return
  - conclusion: `do not claim full automatic core-loop smoke pass for Batch 79`
- Performance proof: `static + code-path proof; route scenario text is built through existing route selection/surface/result paths, not per frame`
- Regression proof:
  - route portfolio widening: `No`
  - combat mechanic expansion: `No`
  - RPG/world/economy system expansion: `No`
  - UI skin/modal changes: `No`
  - pending spoils/extraction policy changes: `No`
  - world selection cache changes: `No`
- UI shape changed?: `Compact text additions only; no broad redesign`
- What the player can now feel: `the Alpha safe/risky choice reads as an operating plan with a reason, party/combat expectation, visible in-run reminder, and result payoff check`
- Recommended next batch: `Batch 79.1 runtime/manual route-scenario proof and text tuning; after that choose Batch 80 world pressure board or a short demo-script pass`
- Detailed proof note: `docs/runtime/batch79-route-operating-scenario-proof.md`

## Batch 78.1 Combat Core Runtime Proof Close-Out

- Selected branch: `B - minimal readback seam tuning`
- Why this was the honest next problem:
  - Batch 78 proved the combat rail statically, but product proof needed the first short playable battle to expose the loop without waiting for a later round
  - the existing rules, skills, damage math, and first Alpha safe encounter were already viable
  - the weak seam was battle-start readback: the first player turn used the older turn-start wrapper and did not refresh RPG-owned enemy intent before Alden's first action
- Runtime seam:
  - `RecordCurrentPartyTurnStartEvent()` now calls `RefreshRpgOwnedEnemyIntentPreviewState()` before recording the turn-start event
  - no encounter HP, skill formula, role payoff, UI skin mapping, inventory truth, world route, dungeon route, or result pipeline rule changed
- Runtime proof:
  - runner: `Batch78_1CombatCoreRuntimeProofRunner.RunBatch78_1CombatCoreRuntimeProof`
  - log: `unity-batch78-1-combat-proof.log`
  - result: `PASS`
  - path: `SampleScene -> city-a -> safe route -> Dungeon Alpha -> Slime Front`
  - intent read: `Slime A (Bulwark) intends Attack on random target: Alden. Front Row -> Front Row | Front Row or Middle Row | Reachable.`
  - setup: `Alden / Power Strike / Slime A`
  - setup preview: `Expected: 10 dmg to Slime A`
  - setup after-hit preview: `HP 19 -> 9 | Opens Burst Window`
  - setup log: `Alden read intent and opened Burst Window on Slime A (+3 for 2 ally actions).`
  - window label: `Burst Window | Intent read | Payoff +3 | 2 ally action(s).`
  - payoff: `Mira / Weak Point / Slime A`
  - payoff preview: `Expected: 10 dmg to Slime A (7 + Burst 3)`
  - payoff after-hit preview: `Would defeat | Burst Window | Consumes Burst Window`
  - role payoff text: `Response: Mira cashes Burst Window now. | Role payoff: Mira cashes Burst Window now.`
  - payoff log: `Mira cashed Burst Window on Slime A for +3.`
  - clear/consume: `Target defeated; window cleared`
- Regression proof:
  - runtime skin bridge: `PASS RuntimeUiSkinBridge=present`
  - battle top strip: `PASS TopStripFallback=True`
  - battle inventory modal: `PASS InventoryOpen=True InventoryReadOnly=True HudBlocksDungeonInput=True InventoryClosed=True`
  - popover/spoils: `static unchanged; this batch does not touch result popover or pending Run Spoils paths`
- Compile:
  - log: `unity-merge-validate.log`
  - result: `PASS`
  - proof markers:
    - `*** Tundra build success`
    - `Batchmode quit successfully invoked - shutting down!`
    - `Exiting batchmode successfully now!`
- Manual UX proof: `AUTOMATED Play Mode runtime proof captured; human screenshot/visual QA not claimed`
- Performance proof: `turn-start intent refresh only; no per-frame scan, asset lookup, OnGUI rebuild, or combat cache widening added`
- UI shape changed?: `No`
- Detailed proof note: `docs/runtime/batch78-1-combat-core-runtime-proof.md`

## Batch 78 Post-UI Revalidation Close-Out

- Selected branch: `A`
- Why this was the honest next problem:
  - Batch 77.10 and 77.10.1 closed the runtime skin bridge plus inventory modal leak enough to stop blocking combat work
  - the BattleScene owner-side combat loop already exists on the accepted branch, so this batch revalidated it after UI closeout instead of adding a second mechanic
  - no UI skin mapping, sprite assignment, world cache, pending-spoils, RPG growth, or inventory truth changed in this pass
- Preflight:
  - UI skin/modal: `PASS static audit; RuntimeUiSkinBridge still registers BattleUiSkin_Default and InventoryUiSkin_Default, TopStripBackground remains empty/fallback, and Target Status is under !battleModalOpen`
  - World selection: `PASS static audit; cached city/world surface invalidation remains on explicit selection changes and no combat files touched that rail`
  - Popover/spoils: `PASS static audit; battle result popover remains modal and Run Spoils / pending extraction wording remains intact`
  - Preview/role: `PASS static audit; current actor role, resolved stats, command preview, target after-hit preview, formula/growth text, and actual damage log paths remain wired`
- Combat loop:
  - intent read: enemy intent snapshots surface current target/action/threat data through the battle HUD and target context
  - window trigger: readable enemy intent plus setup action opens `Burst Window`
  - role payoff: Mira/Weak Point cashes the window and Rune/Arcane Burst can exploit active windows without consuming them
  - preview text: command and target previews show expected damage plus Burst breakdown before commit
  - target panel: target outcome text carries `Burst Window`, `Opens Burst Window`, or `Consumes Burst Window`
  - actual log: setup/payoff log lines call out read-intent, opened window, exploited/cashed window, and applied damage
  - clear/consume: window advances after party actions and consuming payoff clears the target bonus
- Compile:
  - log: `unity-merge-validate.log`
  - result: `PASS`
  - proof markers:
    - `*** Tundra build success`
    - `Batchmode quit successfully invoked - shutting down!`
    - `Exiting batchmode successfully now!`
- Manual UX proof: `DEFERRED; user skipped screenshot QA for time`
- Performance proof: `static only; no new combat code, per-frame scan, asset lookup, or UI rebuild path added`
- UI shape changed?: `No`

## Batch 77.10.1 Close-Out

- Selected branch: `A`
- Honest scope:
  - hide the leaked battle `Target Status` panel while the inventory/equipment modal is open
  - reuse the existing inventory-open battle modal guard instead of adding a new state flag
  - preserve the Batch 77.10 runtime skin bridge and all approved skin mappings
- Preflight:
  - runtime skin bridge: `present through RuntimeUiSkinBridge on SampleScene BootEntry`
  - inventory modal state: `BootEntry.IsInventorySurfaceOpen`
  - Target Status owner: `PrototypeDebugHUD.DrawBattleHudShell -> DrawTargetStatusPanel`
  - existing modal guard: `IsBattleInputModalOpen()` already hides Current Unit, Command Selection, command flyouts, and blocks dungeon battle input while inventory is open`
- Fix summary:
  - Target Status visibility: `DrawTargetStatusPanel(rightRect)` now runs only when `!battleModalOpen`
  - input/hover blocking: target-selection overlay and command flyouts are skipped under the same guard while inventory is open
  - inventory behavior: `DrawInventorySurfaceOverlay` remains unchanged
  - battle HUD restore: when inventory closes, `!battleModalOpen` restores Current Unit, Command Selection, Target Status, command flyouts, and target-selection overlay through the existing draw path
- Compile:
  - log: `unity-merge-validate.log`
  - result: `PASS`
  - proof markers:
    - `*** Tundra build success`
    - `Batchmode quit successfully invoked - shutting down!`
    - `Exiting batchmode successfully now!`
- Manual visual proof: `DEFERRED to user SampleScene screenshot QA`
- Gameplay changes: `none`
- Runtime/resource rail unchanged: `PASS`
- Performance impact: `guard-only draw skip; no new asset lookup, Update loop, scan, or allocation-heavy path`

## Batch 77.10 Close-Out

- Selected branch: `A`
- Honest scope:
  - bridge the verified Battle/Inventory skin assets into the actual `SampleScene` manual runtime path
  - avoid gameplay, combat, RPG progression, world, dungeon, result, Resources, and scene migration changes
  - keep the exact approved Batch 77.9 mapping and leave unlisted slots empty
- Preflight:
  - skin assets: `BattleUiSkin_Default.asset` and `InventoryUiSkin_Default.asset present with approved Sprite-only mappings`
  - preview path: `preview controllers consume the default skin assets directly`
  - runtime path: `PrototypeDebugHUD` and `PrototypePresentationShell` already resolve through Battle/Inventory skin providers, but runtime-added components had no serialized scene skin assignment
  - assignment source: `SampleScene serialized bridge reference; no runtime AssetDatabase, folder scan, per-frame lookup, or broad Resources.Load added`
- Runtime bridge:
  - added `RuntimeUiSkinBridge` on the `BootEntry` GameObject in `Assets/Scenes/SampleScene.unity`
  - bridge registers `BattleUiSkin_Default.asset` with `BattleUiSkinProvider`
  - bridge registers `InventoryUiSkin_Default.asset` with `InventoryUiSkinProvider`
  - runtime-added `PrototypeDebugHUD` and `PrototypePresentationShell` now resolve the cached provider skins when local serialized fields are empty
- Renderer consumption:
  - battle top strip remains on `GetTopStripSlot()` and falls back because `TopStripBackground` is empty
  - battle command buttons continue to consume `GetCommandButtonSlot(...)`, which falls back to `CommandButtonNormal`
  - battle result popover consumes `PopupBackground` plus dark popup title/body/hint colors
  - runtime inventory equipment slots now pass `slot.HasEquippedItem` into `GetEquipmentSlot(selected, hasEquippedItem)` so empty/equipped slots can consume `Slot01a`/`Slot01b`
  - run-spoils badge continues to consume `RunSpoilsBadge`
- Compile:
  - log: `unity-merge-validate.log`
  - result: `PASS`
  - proof markers:
    - `*** Tundra build success`
    - `Batchmode quit successfully invoked - shutting down!`
    - `Exiting batchmode successfully now!`
- Manual visual proof: `DEFERRED to user SampleScene screenshot QA`
- Gameplay changes: `none`
- Runtime/resource rail unchanged: `PASS`
- Performance impact: `one scene-load provider registration only; no new Update/OnGUI hot-path asset lookup`

## Batch 77.9 Close-Out

- Selected branch: `B + C`
- Honest scope:
  - verify the user-provided Battle/Inventory preview mapping
  - serialize only the approved preview assignments
  - create the missing curated Sprite copy for `UI_TravelBook_Button01a_1`
  - keep `TopStripBackground` empty so the battle top strip uses fallback rendering
  - document a GPT-shareable mapping handoff before any runtime bridge
- Preflight:
  - BattleUiSkin_Default and InventoryUiSkin_Default: `present`
  - preview scenes: `present`
  - preview scene scaffold skin paths: `BattleUiSkin_Default.asset` and `InventoryUiSkin_Default.asset`
  - preview scaffold dependency check: `no BootEntry / StaticPlaceholderWorldView / SampleScene dependency found`
  - renderer safeguards: `top strip queries TopStripBackground through GetTopStripSlot; popup text colors are wired`
  - license policy: `TravelBook attribution note present; Raven Fantasy Icons still blocked / UNKNOWN`
- Assignments:
  - Battle `PanelBackground`: `UI_TravelBook_BookCover01a`
  - Battle `CommandButtonNormal`: `UI_TravelBook_Button01a_1`
  - Battle `PopupBackground`: `UI_TravelBook_Popup01a`
  - Battle `TopStripBackground`: `empty`
  - Inventory `EquipmentSlotEmpty`: `UI_TravelBook_Slot01a`
  - Inventory `EquipmentSlotEquipped`: `UI_TravelBook_Slot01b`
  - Inventory `RunSpoilsBadge`: `UI_TravelBook_Popup01a`
- Curated Sprite changes:
  - added `Assets/_Game/Content/UI/Sprites/Battle/Buttons/UI_TravelBook_Button01a_1.png`
  - normalized its curated `.meta` to Sprite import policy: `textureType: 8`, `spriteMode: 1`, `alphaIsTransparency: 1`, `textureCompression: 0`, `enableMipMap: 0`
- Visual/static proof:
  - battle top bar: `PASS static (TopStripBackground is empty and PanelBackground is not queried for the full-width top strip)`
  - battle popup readability: `PASS static (dark popup title/body/hint color path remains wired)`
  - battle command button: `PASS static (curated Sprite reference serialized)`
  - inventory slots: `PASS static (Slot01a / Slot01b Sprite references serialized)`
  - inventory badge: `PASS static (Popup01a Sprite reference serialized; manual screenshot pass still needed for final size judgment)`
- Docs:
  - added `docs/ui/ui-skin-preview-mapping-qa.md`
  - updated manual assignment checklist, slot maps, source map, attribution note, and this status file
- Compile:
  - log: `unity-merge-validate.log`
  - result: `PASS`
  - proof markers:
    - `*** Tundra build success`
    - `Batchmode quit successfully invoked - shutting down!`
    - `Exiting batchmode successfully now!`
- Gameplay changes: `none`
- Runtime/resource rail unchanged: `PASS`

## Batch 77.8.1 Close-Out

- Selected branch: `A`
- Honest scope:
  - validate the already-landed 77.8 renderer/policy changes
  - rerun compile after the Unity editor blocker was cleared
  - confirm the current skin assets still reflect user-authored preview testing rather than random Codex assignments
  - close the validation gate with static proof
- Preflight:
  - Unity editor blocker: `cleared before validation`
  - skin assets: `present with partial user-authored preview test assignments`
  - TravelBook attribution note: `present`
  - Raven icons: `still blocked / UNKNOWN`
- Compile:
  - log: `unity-merge-validate.log`
  - result: `PASS`
  - proof markers:
    - `*** Tundra build success`
    - `Batchmode quit successfully invoked - shutting down!`
    - `Exiting batchmode successfully now!`
- Visual/static proof:
  - top bar: `PASS (TopStripBackground is now the only skin slot queried for the full-width battle top strip; empty slot falls back instead of stretching PanelBackground)`
  - popover readability: `PASS (popup title/body/hint color path is wired through the battle skin definition and the current PopupBackground test assignment remains Sprite-based)`
  - inventory: `PASS (no new inventory code landed in 77.8.1; current equipment slot and run-spoils test assignments remain preview-only and policy-stable)`
- Gameplay changes: `none`
- Runtime/resource rail unchanged: `PASS`

## Batch 77.8 Close-Out

- Selected branch: `A with popover readability guard`
- Honest scope:
  - verify whether the reported manual preview assignments were serialized
  - stop wide battle top-strip rects from consuming general panel art blindly
  - keep the final authoring policy Sprite-first while preserving preview-only Texture fallback
  - add a generic popover text-color path for light parchment backgrounds
- Preflight:
  - serialized assignments in the default skin assets: `none`
  - Battle/Inventory skin assets: `present`
  - Battle/Inventory layout profiles: `present`
  - preview scenes: `present`
  - Raven icons: `still blocked`
- Fix summary:
  - top bar: `BattleUiSkinDefinition.TopStripBackground` added; preview and shared battle HUD now fall back instead of stretching PanelBackground across the full-width top strip
  - Sprite/Texture policy: `Sprite` is documented as the final input path; `Texture` is explicitly preview-only fallback
  - popover readability: `PopupTitleTextColor`, `PopupBodyTextColor`, and `PopupHintTextColor` added for light popup-background cases
  - inventory: `no layout or code changes; existing slot-preview guidance kept stable`
- Gameplay changes: `none`
- Validation snapshot:
  - compile: `BLOCKED (Project_AAA was already open in another Unity editor instance on 2026-04-21)`
  - top-strip fallback guard: `PASS`
  - popover text-color path: `PASS`
  - runtime/resource rail unchanged: `PASS`

## Batch 77.7 Close-Out

- Selected branch: `C`
- Honest scope:
  - verify the 77.6-ready preview/skin/layout state
  - confirm there is still no explicit human mapping, screenshot set, or layout-note packet to act on
  - add a dedicated attribution doc
  - keep all preview skins empty and fallback-safe
- Preflight:
  - BattleUiSkin_Default and InventoryUiSkin_Default: `present and still fully unassigned`
  - BattleUiLayout_Default and InventoryUiLayout_Default: `present`
  - BattleUiPreviewScene and InventoryUiPreviewScene: `present`
  - curated sprite folders: `present`
  - Raven icons: `still blocked with UNKNOWN license status`
- Assignments:
  - Battle: `none`
  - Inventory: `none`
- Layout changes:
  - Battle: `none`
  - Inventory: `none`
- Docs added:
  - `docs/ui/ui-asset-attribution.md`
- Gameplay changes: `none`
- Validation snapshot:
  - compile: `PASS (unity-merge-validate.log, 2026-04-20)`
  - preview scenes remain usable with fallback: `PASS`
  - runtime hot spots untouched in this batch: `PASS`
  - waiting-for-human-input close-out: `PASS`

## Batch 77.6 Close-Out

- Selected branch: `A`
- Honest scope:
  - verify the curated sprite staging from Batch 77.5
  - audit visible license text before any assignment claim
  - normalize curated copies for Sprite-field assignment without touching raw packs
  - keep preview scaffolds intact and produce a manual assignment checklist
- 77.5 staging audit:
  - `Assets/_Game/Content/UI/Sprites` exists with `Battle`, `Inventory`, `Shared`, and `_SourceMap`
  - candidate-map docs exist
  - `Assets/Sprite` remained untouched
  - default battle/inventory skin assets still existed and still had no assigned art
- License / attribution audit:
  - `Complete_UI_Book_Styles_Pack_Free_v1.0/License.txt` was found
  - visible text allows personal/commercial project use but requires credit or a product-page link and a note if changes were made
  - visible text also says not to resell or publish the same or adapted material as a standalone asset pack
  - `Free - Raven Fantasy Icons` had no visible license text in the audited folder, so its status remains `UNKNOWN`
  - result: TravelBook Lite is acceptable for project-side preview work with attribution notes; Raven remains held out
- Import audit:
  - raw TravelBook source remained at `textureType: 0`, `spriteMode: 0`, and related generic-texture values
  - curated copies under `Assets/_Game/Content/UI/Sprites` were normalized to `Sprite (2D and UI)` import with `spriteMode: Single`, `alphaIsTransparency: 1`, `textureCompression: 0`, and `enableMipMap: 0`
  - curated `.meta` files now exist and the sprites are prepared for `Sprite` field assignment
- Skin assets:
  - `Assets/_Game/Content/UI/Skins/Battle/BattleUiSkin_Default.asset`: present, still intentionally unassigned
  - `Assets/_Game/Content/UI/Skins/Inventory/InventoryUiSkin_Default.asset`: present, still intentionally unassigned
- Layout profiles:
  - `Assets/_Game/Content/UI/LayoutProfiles/Battle/BattleUiLayout_Default.asset`: present
  - `Assets/_Game/Content/UI/LayoutProfiles/Inventory/InventoryUiLayout_Default.asset`: present
- Preview scenes:
  - `Assets/_Game/Scenes/Preview/BattleUiPreviewScene.unity`: present
  - `Assets/_Game/Scenes/Preview/InventoryUiPreviewScene.unity`: present
  - static proof: preview scene/controller path shows no `BootEntry`, `StaticPlaceholderWorldView`, or `SampleScene` dependency in the preview scaffold audit
- Assignments made:
  - Battle: `none`
  - Inventory: `none`
- Docs added:
  - `docs/ui/manual-skin-assignment-checklist.md`
  - `Assets/_Game/Content/UI/Sprites/_SourceMap/skin-assignment-template.md`
- Gameplay changes: `none`
- Validation snapshot:
  - compile: `PASS (unity-merge-validate.log, 2026-04-20)`
  - license/import audit: `PASS`
  - preview scaffold presence proof: `PASS`
  - manual visual proof: `DEFERRED to Unity Editor screenshot pass`
  - runtime/resource rail unchanged: `PASS`

## Batch 77.5 Close-Out

- Selected branch: `A`
- Honest scope:
  - audit raw UI sprite packs without moving them
  - create a project-owned curated staging area under `Assets/_Game/Content/UI/Sprites`
  - copy only a small intentional starter set of battle/inventory UI candidates
  - document slot mapping and manual assignment workflow
- Raw source audit:
  - `Assets/Sprite/Complete_UI_Book_Styles_Pack_Free_v1.0` is the clearest UI-source pack for panel/button/bar/slot candidates
  - `Assets/Sprite/Free - Raven Fantasy Icons` contains many numeric icon files and was audited but not curated in this batch because the filenames are too opaque for safe narrowing
  - raw sampled UI metas still show `textureType: 0`, `spriteMode: 0`, `alphaIsTransparency: 0`, `spritePixelsToUnits: 100`, and `filterMode: 1`
- Curated starter copy:
  - battle panel/button/timeline/bar/popup/badge/icon candidates were copied from `TravelBookLite`
  - inventory panel/slot/row/badge/icon candidates were copied from `TravelBookLite`
  - shared frame/divider/background/icon examples were staged for future reuse
  - raw source packs were not moved, renamed, or reimported
- Docs added:
  - `docs/ui/ui-sprite-curation.md`
  - `docs/ui/battle-skin-slot-map.md`
  - `docs/ui/inventory-skin-slot-map.md`
  - `Assets/_Game/Content/UI/Sprites/_SourceMap/README.md`
  - `Assets/_Game/Content/UI/Sprites/_SourceMap/ui-sprite-candidate-map.md`
- Import setting changes:
  - none
  - curated copies intentionally preserved the sampled raw import behavior for now
  - any future `Sprite (2D and UI)` conversion should happen on curated copies only, not on raw source art
- Gameplay changes: `none`
- Validation snapshot:
  - compile: `PASS (unity-merge-validate.log, 2026-04-20)`
  - raw source untouched: `PASS`
  - curated folders created: `PASS`
  - runtime/resource rail unchanged: `PASS`
  - SampleScene path preserved: `PASS`

## Batch 77.4 Close-Out

- Selected branch: `A`
- Honest scope:
  - clarify folder ownership for battle UI, inventory UI, debug HUD, and runtime-vs-preview scene assets without changing gameplay behavior
  - keep path-sensitive runtime content in place where the audit showed higher breakage risk
- Files moved:
  - `Assets/_Game/Scripts/Core/PrototypeDebugHUD.cs` -> `Assets/_Game/Scripts/Debug/PrototypeDebugHUD.cs`
  - `Assets/_Game/Scripts/Core/PrototypePresentationShell.DungeonRun.cs` -> `Assets/_Game/Scripts/UI/Battle/PrototypePresentationShell.DungeonRun.cs`
  - `Assets/_Game/Scripts/Core/PrototypePresentationShell.GameplayBattleHud.cs` -> `Assets/_Game/Scripts/UI/Battle/PrototypePresentationShell.GameplayBattleHud.cs`
  - `Assets/_Game/Scripts/Core/PrototypePresentationShell.Inventory.cs` -> `Assets/_Game/Scripts/UI/Inventory/PrototypePresentationShell.Inventory.cs`
  - `Assets/_Game/Scenes/Boot.Unity` -> `Assets/_Game/Scenes/Runtime/Boot.Unity`
- Folder structure added:
  - `Assets/_Game/Scripts/Debug/`
  - `Assets/_Game/Scenes/Runtime/`
  - `Assets/_Game/Scenes/Test/`
  - `Assets/_Game/Content/UI/Sprites/`
- Intentionally not moved:
  - `Assets/Scenes/SampleScene.unity` because Build Settings, editor defaults, and smoke tooling still point at that legacy baseline
  - `Assets/_Game/Resources/Content/*` because `Resources.Load(...)` paths are active runtime/editor dependencies
  - `Assets/Sprite/*` because runtime import/path ownership is not stabilized yet and this batch is not an asset migration batch
  - `BootEntry.cs` and `StaticPlaceholderWorldView*.cs` because they are still hot spots and not safe folder-shuffle targets in this pass
- Gameplay changes: `none`
- Validation snapshot:
  - compile: `PASS (unity-merge-validate.log, 2026-04-20)`
  - playable-path proof: `SampleScene path intentionally preserved`
  - path-risk guard: `runtime Resources and preview scene paths preserved`

## Batch 77.3 Close-Out

- Selected branch: `B`
- Honest scope:
  - keep the existing runtime/UI rail intact
  - extend the existing battle/inventory skin scaffold into editor-friendly preview scenes, layout profiles, and mock preview data
  - avoid all gameplay/runtime migration while giving the designer a safe art-placement workspace
- Added preview scaffolds:
  - `Assets/_Game/Scenes/Preview/BattleUiPreviewScene.unity`
  - `Assets/_Game/Scenes/Preview/InventoryUiPreviewScene.unity`
  - `Assets/_Game/Content/UI/Skins/Battle/BattleUiSkin_Default.asset`
  - `Assets/_Game/Content/UI/Skins/Inventory/InventoryUiSkin_Default.asset`
  - `Assets/_Game/Content/UI/LayoutProfiles/Battle/BattleUiLayout_Default.asset`
  - `Assets/_Game/Content/UI/LayoutProfiles/Inventory/InventoryUiLayout_Default.asset`
  - `Assets/_Game/Content/UI/Preview/Battle/BattleUiPreview_Default.asset`
  - `Assets/_Game/Content/UI/Preview/Inventory/InventoryUiPreview_Default.asset`
- Added preview code:
  - battle/inventory layout profile ScriptableObjects
  - battle/inventory mock preview data ScriptableObjects
  - scene-local preview presenter/controller scaffold
  - editor generator for preview scenes/assets
- Sprite workflow note:
  - `Assets/Sprite` was audited only
  - no auto-assignment or import-setting mutation was performed
  - preview slots support manual Sprite or Texture assignment, which is important because sampled sprite-pack metas were still imported as `textureType: 0`
- Runtime safety:
  - `SampleScene` play path unchanged
  - no BootEntry dependency added to preview scenes
  - no runtime world/battle/inventory truth mutation added
  - no DDOL UI hierarchy added
- Validation snapshot:
  - compile: `PASS (Unity batch compile succeeded on 2026-04-20 via unity-merge-validate.log)`
  - preview generation: `PASS (Unity batch generator created preview scenes and default preview assets on 2026-04-20 via unity-ui-preview-builder.log)`
  - runtime isolation: `PASS (code-path scope only)`

## Scene Architecture Scaffold Follow-Up

- Selected branch: `scaffold-first, no scene cutover`
- Honest scope:
  - add a persistent-root code scaffold without claiming the project is already multi-scene
  - document what should persist, what must unload, and which current files are legacy adapters
  - keep the accepted `SampleScene` flow, battle HUD rail, inventory rail, and result pipeline intact
- Added runtime scaffold:
  - `Assets/_Game/Scripts/App/GameSceneId.cs`
  - `Assets/_Game/Scripts/App/AppRoot.cs`
  - `Assets/_Game/Scripts/App/GameSessionRoot.cs`
  - `Assets/_Game/Scripts/App/RuntimeGameState.cs`
  - `Assets/_Game/Scripts/App/SceneFlowService.cs`
- Added architecture docs:
  - `docs/architecture/scene-ownership-plan.md`
  - `docs/architecture/ddol-policy.md`
  - `docs/architecture/scene-migration-roadmap.md`
  - `docs/architecture/script-naming-policy.md`
- Ownership call:
  - persistent truth stays with runtime owners like `ManualTradeRuntimeState` and canonical handoff contracts
  - scene-local IMGUI shells (`PrototypePresentationShell`, `PrototypeDebugHUD`) remain disposable presenters
  - `BootEntry` and `StaticPlaceholderWorldView` stay classified as legacy adapters/hot spots, not new long-term owners
- Validation snapshot:
  - compile: `PASS (unity-merge-validate.log, 2026-04-20)`
  - runtime cutover: `NOT STARTED`
  - SampleScene preservation: `PASS (code-path scope only)`

## Batch 77.2 Close-Out

- Selected branch: `A with manual-asset guardrails`
- Honest blocker:
  - the current accepted battle HUD rail is still immediate-mode and color-authored
  - user art exists under `Assets/Sprite`, but automatic assignment is unsafe because the pack is not a dedicated runtime-resolved skin path and at least part of it is not guaranteed sprite-imported
- Fix shape:
  - added `BattleUiSkinDefinition`, `BattleUiSkinProvider`, and `BattleUiSkinRenderer`
  - added `InventoryUiSkinDefinition`, `InventoryUiSkinProvider`, and `InventoryUiSkinRenderer`
  - exposed named skin slots for battle panels, command buttons, current-unit card, target-status card, timeline chips, HP bars, and the battle result popover
  - split inventory skinning into its own definition and wired member rows, equipment slots, item rows, and the run-spoils badge
  - wired the active `PrototypeDebugHUD` battle rail through null-safe skin hooks
  - wired the `PrototypePresentationShell` battle result popover through the same skin path
  - left all slots manual and optional so missing art preserves the current accepted visuals
- UI shape changed?: `No layout rewrite; fallback parity preserved when slots are empty`

## Batch 77.2 Validation Snapshot

- Compile: `PASS` (`unity-merge-validate.log`, 2026-04-20)
- Preflight:
  - active battle HUD owner audit: `PASS`
  - battle result popover owner audit: `PASS`
  - inventory overlay interaction-point audit: `PASS`
  - asset path / non-Resources / importer-risk audit: `PASS`
- Static scaffold proof:
  - no random sprite assignment path added: `PASS`
  - missing-slot fallback preserves current draw path: `PASS`
  - battle result popover now shares the battle skin scaffold: `PASS`
  - inventory row/slot/badge skinning is isolated from battle skin ownership: `PASS`
- Manual runtime proof: `DEFERRED`
- Smoke: `DEFERRED`

## Batch 77.1 Close-Out

- Selected branch: `A + B + D`
- Honest blocker:
  - legitimate world selection was still invalidating the correct selected-city surfaces
  - but the click frame also rebuilt world observation, dispatch, projected launch context, and prep readbacks after a run had added party progression / gear / inventory data
- Measured root-cause seam:
  - `StaticPlaceholderWorldView.SetSelected` rebuilt `BuildWorldObservationSurfaceData()` just to refresh board emphasis
  - the same observation build eagerly recreated dispatch + prep surfaces even while the prep board was closed
  - those closed-board paths still pulled `BuildRuntimePartyResolveSurface(...)` for role/loadout/manifest summaries after post-run data existed
- Fix shape:
  - selection now updates board emphasis from the selected marker plus latest writeback without rebuilding the full observation surface
  - `ManualTradeRuntimeState` now caches compact party identity / role / loadout summaries on progression / equipment / inventory updates
  - world-selection dispatch, party-roster, start-context, and closed-board prep summaries now consume those cached readbacks by default
  - detailed party resolve / manifest construction is still forced for actual prep-launch and dungeon handoff paths
- UI shape changed?: `No`

## Batch 77.1 Validation Snapshot

- Compile: `PASS` (`unity-merge-validate.log`, 2026-04-20)
- Static performance proof:
  - selection-board emphasis no longer rebuilds world observation on `SetSelected`: `PASS`
  - closed-board dispatch/prep/start-context paths no longer require `BuildRuntimePartyResolveSurface(...)`: `PASS`
  - launch/prep/dungeon handoff still request detailed party manifests explicitly: `PASS`
- Manual runtime proof: `DEFERRED`
- Smoke: `DEFERRED`
- Batch 78 resume gate: `code-path blocker removed; fresh editor/runtime FPS recapture still recommended before calling the spike fully closed`

## Batch 78 Close-Out

- Selected branch: `A`
- Batch 78 keeps the accepted battle HUD rail and strengthens the already-existing combat pleasure loop instead of adding a second combat system:
  - `read enemy intent`
  - `open Burst Window`
  - `cash role payoff`
- The implementation stays additive and owner-side:
  - enemy focus text now carries a clearer response hint when intent is readable or Burst Window is active
  - action threat summaries now bundle intent-read, threat severity, and recommended response into the existing command/target readback seams
  - attack / skill preview text now shows visible Burst breakdown when payoff is active
  - target outcome preview now carries `Burst Window`, `Opens Burst Window`, or `Consumes Burst Window` on the same HP-after-hit rail
  - `Weak Point` now depends on the visible Burst Window loop instead of quietly receiving a separate low-HP finisher fallback
  - combat logs now celebrate setup/payoff explicitly instead of hiding the loop only inside generic damage text
- UI shape changed?: `No layout rewrite; only compact combat readbacks/log wording were strengthened on the accepted rail`

## Batch 78 Validation Snapshot

- Compile: `PASS`
- Preflight:
  - Batch 77 role identity rail: `PASS (static audit reused; prep / inventory / battle / growth role seams remain wired)`
  - Batch 76 preview/log rail: `PASS (static audit reused and strengthened in the same owner-side preview/log path)`
  - Batch 75.4 world click / modal / pending-spoils guards: `PASS (untouched code-path audit before edits)`
- Targeted gameplay proof:
  - intent read -> setup hint visibility: `PASS (code-path audit)`
  - Burst Window preview breakdown: `PASS (code-path audit)`
  - target panel window-state readback: `PASS (code-path audit)`
  - payoff log celebration: `PASS (code-path audit)`
- Manual UX proof: `DEFERRED`
- Performance proof: `DEFERRED (readback-only owner-side changes; no fresh runtime capture in this turn)`
- Smoke: `DEFERRED`

## Batch 77 Close-Out

- Selected branch: `A`
- Batch 77 keeps the accepted UI rail intact and closes the clearer product bottleneck: the party did not yet read as four distinct RPG roles even though growth, route, and battle-preview rails already existed.
- The implementation stays additive and owner-respecting:
  - `PrototypeRpgRoleIdentity` centralizes static role fantasy, gear preference, battle-hint, route-fit, and growth-meaning text
  - Expedition Prep now prefers a compact staged-party role summary instead of generic party composition wording
  - route-fit readback now carries role-aware guidance for safer, risky, and mixed-pressure routes without changing route mechanics
  - inventory/equipment now surfaces role identity, preferred stats, and per-item fit text without restoring the removed comparison panel
  - battle current-actor and command-detail readbacks now explain what the active role wants to do on this turn
  - growth/result reveal text now explains why a stat gain matters for that role instead of only listing raw deltas
- UI shape changed?: `No rollback and no layout rewrite; only readback content was strengthened on the current accepted rail`

## Batch 77 Validation Snapshot

- Compile: `PASS`
- Preflight:
  - combat preview visibility rail: `PASS (static audit of actor stats, preview, target after-hit, formula/growth, and applied log path)`
  - 75.4 modal / no-op click / pending-spoils guards: `PASS (code-path audit reused before edits)`
  - 74 / 74.1 inventory overlay guardrails: `PASS (surface/input/render audit reused before edits; comparison panel remained removed)`
- Targeted role-identity proof:
  - Expedition Prep staged party summary: `PASS`
  - Inventory selected member + selected item fit readback: `PASS`
  - Battle current actor + command detail role hints: `PASS`
  - Growth / result role-meaning readback: `PASS`
- Manual UX proof: `DEFERRED`
- Performance proof: `DEFERRED (static/cache-oriented implementation only; no fresh runtime capture in this turn)`
- Smoke: `DEFERRED`

## Batch 76 Close-Out

- Selected branch: `A`
- Batch 76 keeps the accepted battle HUD rail and exposes combat growth through the current runtime truth instead of adding a new combat system.
- Current battle HUD now surfaces:
  - current actor resolved stat line
  - compact stat-source line for the active unit
  - attack / skill preview text in the command flyout
  - formula / growth contribution readback in the same flyout
  - target preview outcome during target selection, including HP after-hit or would-defeat messaging
- Actual combat logs now reuse the same resolved preview source so the visible battle message and the real applied result stay on the same stat rail.
- One narrow runtime seam fix was required:
  - shared-skill resolution now prefers the resolved runtime member skill power before falling back to static skill-definition power, so growth/equipment no longer disappear on the skill path
- UI shape changed?: `Preserved current accepted HUD shape; only stat-feedback/readback content was added`

## Batch 76 Validation Snapshot

- Compile: `PASS`
- Static audit: `PASS`
- Attack preview parity: `PASS`
- Skill preview parity: `PASS`
- Growth/equipment surfacing: `PASS`
- Manual runtime proof: `DEFERRED`
- Smoke: `DEFERRED` because this close-out used compile + code audit, but no fresh manual battle playthrough was executed in this turn

## Batch 75.4 Close-Out

- Selected branch: `A`
- Batch 75.4 closes three blocking UX/runtime seams without changing the accepted dungeon/battle rail:
  - world-map no-op clicks no longer invalidate the city/world readback cache
  - the battle result popover now behaves as a real modal instead of allowing background dungeon progression
  - mid-run reward language now matches the actual storage truth by surfacing `Run Spoils` / `pending extraction` wording instead of fake acquisition claims
- The world-map fix stays narrow:
  - `SelectAtScreenPosition(...)` and `SetSelected(...)` now report whether selection state actually changed
  - `BootEntry.HandleSelectionInput()` only invalidates cached city/world surfaces when selection changed
  - repeated empty clicks after the selection is already cleared now stay on the no-op path
- The reward-truth fix stays honest:
  - immediate battle popups now say `Run Spoils` / `Pending ...`
  - encounter result popovers say `pending extraction` for run spoils and `finalizes at run result` for elite reward reservation
  - the inventory surface reuses current run truth to show pending run spoils during dungeon play without promoting them to permanent inventory early
- The popover modal fix now blocks the old click-through path:
  - non-battle dungeon shells return early while the popover is visible
  - the popover overlay consumes mouse events in OnGUI
  - only the documented close inputs, plus the already-approved `[I]` inspect handoff, remain active while the popover is open
- UI shape changed?: `No layout rollback; current accepted rail preserved with modal behavior tightened and reward wording clarified`

## Batch 75.4 Validation Snapshot

- Compile: `PASS`
- World-map click/perf audit: `PASS`
- No-op cache invalidation guard: `PASS`
- Popover modal click-through guard: `PASS (code-path + OnGUI guard)`
- Pending spoils wording / visibility parity: `PASS`
- Manual runtime performance proof: `DEFERRED`
- Manual popover click-through proof: `DEFERRED`
- Smoke: `DEFERRED`

## Batch 75.3 Close-Out

- Selected branch: `B + C hybrid`
- Batch 75.3 upgrades the centered encounter-result popover into a real readback moment without reopening battle math, drop systems, or HUD layout direction.
- The implementation stays on the current accepted rail:
  - encounter / defeat / retreat popovers still come from one cached shell snapshot
  - new text fields are additive and built once at result production time, not during per-frame GUI work
  - missing encounter-time growth / gear truth is shown honestly as pending instead of faked
- The upgraded popover now carries:
  - concrete header + subtitle context
  - result / rewards / drop readback
  - party HP and combat consequence summary
  - contribution / battle highlight text from current-battle event truth when available
  - growth carryover status and next-step guidance
  - optional `[I]` inspect-equipment handoff directly from the popover when inventory access is available
- UI shape changed?: `No layout rollback; centered popover preserved with richer content and a small subtitle/body sizing adjustment`

## Batch 75.3 Validation Snapshot

- Compile: `PASS`
- Active rail audit:
  - popover producer / cache seam audited before edits: `PASS`
  - current battle / party / next-goal truth reused without new gameplay ownership: `PASS`
  - encounter-time XP / gear finalization stays pending where truth is unavailable: `PASS`
- Targeted code proof:
  - popover body now consumes cached result / reward / party / combat / next-step fields: `PASS`
  - `[I]` inspect handoff is handled inside the popover input gate, so the old global inventory toggle block is not bypassed accidentally: `PASS`
  - centered modal sizing widened only enough to fit the richer compact readback at 1920x1080 intent: `PASS (code-path audit)`
- Manual UX proof: `DEFERRED`
- Performance proof: `DEFERRED (compile + code-path audit only; no fresh runtime FPS capture in this turn)`
- Smoke: `DEFERRED`

## Batch 75.2 Close-Out

- Selected branch: `A`
- Latest git audit confirms that the current `HEAD` already contains the intended Batch 75.1 reward-feedback work:
  - centered battle-end popover state and shell rendering
  - enemy defeat reward floating text on real reward truth
  - inventory/equipment surface without the old visible comparison panel
- No gameplay/system edits were required in 75.2.
- The latest git evidence used for close-out was:
  - `git show --stat --oneline HEAD`
  - `git grep`/code audit for `IsBattleResultPopover`, `Encounter Cleared`, inventory surface ownership, and comparison-panel removal
  - existing docs close-out for Batch 75.1
- Dedicated `docs/runtime/batch75-1*` handoff note is not present, but the mainline proof now lives in the canonical status/content docs instead.
- UI shape changed?: `No additional runtime UI change in 75.2; audit/docs close-out only`

## Batch 75.2 Validation Snapshot

- Latest commit audited: `07b43e1 Add expedition prep and battle handoff flow`
- Git state: `root repo clean for tracked files used by 75.1/75.2 audit; only nested worktree pointers remain modified`
- Compile: `REUSED PASS from latest 75.1 compile proof; no new gameplay code change in 75.2`
- Manual UX proof: `REUSED from current accepted rail evidence; no fresh manual pass in 75.2`
- Performance proof:
  - reward-feedback loop shows no new audit evidence of a 75.1-specific regression
  - the previously known battle-scene-only FPS issue still exists and remains separate from this close-out
- Smoke: `NOT RERUN`

## Batch 75.1 Close-Out

- Selected branch: `B + C hybrid`
- Batch 75.1 is a UX feedback fix on top of the accepted rail, not a new battle, inventory, or reward system batch.
- It closes three narrow visibility gaps without changing reward math:
  - enemy defeat now reuses the existing battle floating popup rail to show real reward truth such as `Loot +3`
  - each encounter victory now opens a centered battle-result popover built once from cached encounter/run summary fields
  - the visible inventory/equipment surface no longer carries the large `Comparison` panel
- The implementation stays honest to existing ownership:
  - kill popups read the actual monster `RewardAmount` / `RewardResourceId`
  - encounter / defeat / retreat summaries read one cached popover snapshot instead of rebuilding per frame
  - inventory still consumes the same runtime-owned comparison/action truth, but compresses it into selected-item detail text instead of a separate panel
- Final-elite handling stays non-duplicative:
  - elite encounter victory still shows the encounter popover
  - run clear continues to the existing run result later
  - run defeat / retreat now surface the compact battle-end popover before the run result shell
- UI shape changed?: `Yes, centered battle-result popover plus comparison-panel deletion on the current inventory surface`

## Batch 75.1 Validation Snapshot

- Compile: `PASS`
- Targeted audit:
  - enemy defeat popup uses actual reward truth and does not double-roll loot: `PASS`
  - encounter victory creates a single modal popover state and clears it on explicit close input: `PASS`
  - run defeat / retreat now pause on the compact popover before the existing result shell: `PASS`
  - inventory still keeps equip / unequip flow while the separate comparison panel is removed: `PASS`
- Manual verification: `DEFERRED`
- Performance proof: `DEFERRED (no fresh scene-by-scene FPS capture was taken in this turn)`
- Smoke: `DEFERRED`

## Batch 75 Close-Out

- Selected branch: `C`
- Batch 75 does not add a new RPG mechanic, inventory framework, or battle formula.
- It closes the missing reveal seam between already-landed progression/equipment truth and the player-facing result/return surfaces.
- The mainline rail now caches one growth/reward readback set per resolved run:
  - member XP gain and level change
  - compact stat delta callouts
  - item drop and equip/store outcome
  - next-run meaning text
  - `[I]` inspect-equipment hint
- The reveal is intentionally owner-driven:
  - `ResultPipeline` now builds cached growth reveal summaries once per result refresh
  - `ManualTradeRuntimeState` refreshes those summaries again after auto-equip/store resolution, so return readback does not lose the item/build-change moment
  - dungeon result, post-run return reveal, and prep return-consume text now read from the same cached fields
- The missing return seam that made this an honest Branch C was:
  - city/world return helpers for gear reward / equip swap / continuity were still returning `None`
  - result/readback did not yet have dedicated growth-reveal fields, so the player had to infer change from scattered text
- UI shape changed?: `Yes, readback-only strengthening on existing result/return/prep surfaces`

## Batch 75 Validation Snapshot

- Compile: `PASS`
- Targeted audit:
  - Batch 72 progression storage and post-run writeback still exist: `PASS`
  - Batch 72.1 battle-return continuity payload remains intact while growth reveal fields are additive: `PASS`
  - Batch 72.2 / 72.3 performance guardrail is respected by cached result summaries instead of per-frame recompute: `PASS`
  - Batch 73 gear reward / auto-equip / stat-delta rail still owns the item/build-change truth: `PASS`
  - Batch 74 / 74.1 inventory surface remains the inspect target instead of being rebuilt inside the result screen: `PASS`
- Manual verification: `DEFERRED`
- Performance proof: `DEFERRED (no fresh scene-by-scene FPS capture was taken in this turn)`
- Smoke: `DEFERRED`

## Batch 74 Close-Out

- Selected branch: `A`
- Batch 74 turns the hidden Batch 73 inventory truth into a real player-facing character equipment surface without replacing the accepted mainline HUD rail.
- The new surface stays on canonical runtime ownership:
  - `ManualTradeRuntimeState` remains the truth owner for party-carried gear, equipped slot state, and comparison inputs
  - `StaticPlaceholderWorldView` now exposes a cached inventory surface adapter instead of minting a second equipment store
  - `BootEntry` and `BootstrapSceneStateBridge` only forward input + display access
- Player-facing surface added:
  - `[I]` open / close inventory
  - `[Esc]` close
  - `[Q/E]` cycle party member
  - `[1-7]` select equipment slot
  - `[Up/Down]` move through compatible inventory items
  - `[Enter]` equip selected item
  - `[U]` or `[Backspace]` unequip selected slot
- Availability by stage:
  - `WorldSim`: full equipment management
  - `ExpeditionPrep`: full equipment management
  - `DungeonRun explore / route / event / result`: full equipment management
  - `BattleScene`: read-only inspection only
- The surface is intentionally additive, not a layout rollback:
  - member column
  - seven-slot equipment grid
  - comparison / action panel
  - party inventory list
  - cached detail/readback blocks
- UI shape changed?: `Yes, additive overlay only`

## Batch 74 Validation Snapshot

- Compile: `PASS`
- Targeted audit:
  - inventory/equipment surface reuses canonical world/runtime truth instead of duplicating it: `PASS`
  - manual equip / unequip now flows through world and dungeon shell input without changing battle legality ownership: `PASS`
  - battle context correctly gates the surface to read-only inspection: `PASS`
  - presentation rebuild is cached off party inventory revision plus UI selection state: `PASS`
- Manual verification: `DEFERRED`
- Smoke: `PARTIAL (Batch10 smoke passed WorldSim -> CityHub -> ExpeditionPrep -> DungeonRun -> BattleScene -> DungeonRun continuity checkpoints, then the existing runner timed out later at ResolveCoreLoop)`

## Batch 73 Close-Out

- Selected branch: `A`
- Batch 73 finishes the next honest RPG seam on top of the existing 72 / 72.1 rail instead of reopening UI layout or worldmap work.
- World/runtime truth now owns:
  - party-carried gear inventory
  - per-member equipment slot state
  - slot legality and auto-equip resolution
  - persistent carryover into the next prep / run
- Character equipment is now explicit across seven slots:
  - `head`
  - `left arm`
  - `right arm`
  - `torso`
  - `belt`
  - `pants`
  - `shoes`
- Result writeback now makes gear progression tangible:
  - successful runs can mint real equipment drops
  - stronger drops auto-equip onto the correct member + slot
  - replaced gear stays in the party inventory if it is no longer equipped
- Readback proof stays on the accepted UI rail:
  - auto-equip now spells out visible stat deltas such as `ATK 5 -> 6`
  - prep / party loadout summary now echoes slot-aware equipment state + carried inventory
  - result reward lines now explain gear reward, equip swap, build change, and carry continuity from the same runtime truth
  - next-run prep / battle readback now shows resolved `Lv / HP / ATK / DEF / SPD` plus compact gear evidence
- UI shape changed?: `No`

## Batch 73 Validation Snapshot

- Compile: `PASS`
- Targeted audit:
  - party inventory truth persists on the canonical world/runtime rail: `PASS`
  - per-member slot equipment affects next-run numbers and derived party power: `PASS`
  - prep / party / result readback now exposes gained gear, equipped upgrades, and stored carryover: `PASS`
  - Batch 72.1 result handoff remains on the stable return path after runtime writeback sync: `PASS`
- Manual verification: `DEFERRED`
- Smoke: `PARTIAL (Batch10 smoke reached the DungeonRun -> BattleScene -> DungeonRun continuity checkpoint and passed it, then the runner timed out later at ResolveCoreLoop)`

## Batch 72 Close-Out

- Selected branch: `A`
- Batch 72 closes party growth / differentiation on the existing runtime seam instead of inventing a second progression framework.
- World / prep readback now answers the party question in one throughline:
  - archetype + promotion identity
  - why that identity fits the route
  - what next edge the current promotion state is unlocking
- Battle command readback now carries party doctrine + member battle-role context on top of the existing burst-window setup/payoff loop.
- Result / return readback now preserves party growth consequence instead of letting city writeback text fully overwrite it:
  - result progression now prefers next-run consequence before raw applied text
  - return aftermath / next-prep follow-up can now echo party growth as well as city impact
- UI shape changed?: `No`

## Batch 72 Validation Snapshot

- Compile: `PASS`
- Targeted audit:
  - dispatch / recommendation / route-fit party throughline: `PASS`
  - ExpeditionPrep staged-party / launch-manifest growth readback: `PASS`
  - battle command setup-payoff text varies by party identity: `PASS`
  - result / next-prep growth consequence survives the return path: `PASS`
- Manual verification: `DEFERRED`
- Smoke: `DEFERRED`

## Batch 72.1 Close-Out

- Selected branch: `B`
- Batch 72.1 adds one thin progression seam on top of the current Batch 72 party rail instead of opening a full inventory or equipment framework.
- Character runtime progression now persists on the canonical world/runtime path:
  - per-member `Level`
  - per-member `Experience`
  - per-member next-level threshold
  - role/archetype-biased growth bonuses for `HP / Attack / Defense / Speed`
- Battle/result return now converts combat contribution into real post-run growth:
  - XP gain
  - level-up resolution
  - stat-growth readback
  - lightweight loot-drop bundles
- Loot is intentionally staged as a hidden pending stash instead of a visible inventory screen:
  - stash bundles accumulate on the party runtime
  - result/readback text can surface the reward without changing the accepted UI layout
  - the reward schema is now ready to hand into a future Batch 73 inventory/equipment surface
- UI shape changed?: `No`

## Batch 72.1 Validation Snapshot

- Compile: `PASS`
- Targeted audit:
  - world/runtime party seam now persists per-member level/xp: `PASS`
  - result pipeline now resolves XP + level-up + stat growth on the canonical return path: `PASS`
  - lightweight loot/drop bundles now flow into a hidden pending stash: `PASS`
  - existing prep/result readback surfaces can now echo next-level and loot continuity without a UI redesign: `PASS`
- Manual verification: `DEFERRED`
- Smoke: `FAIL (existing BattleScene -> DungeonRun return continuity check still breaks before full result/world re-entry proof)`

## Batch 72.1 Continuity Repair Note

- The first reported blocker after Batch 72.1 was not the progression seam itself, but the adjacent `DungeonRun -> BattleScene -> DungeonRun` return seam.
- The concrete drift was:
  - battle return could rebuild from the current room after combat
  - the rebuilt snapshot could fall forward to the next room context
  - `EncounterId / EncounterName / EncounterRoomType / Objective` could therefore collapse away from the original battle-entry context
- The active fix keeps this narrow:
  - battle result snapshot rebuild now prefers the stable battle-entry request before falling back to current-room data
  - battle return payload now prefers the same stable battle-entry request for encounter identity, room type, objective, and summary naming
- UI shape remains unchanged.
- Next required proof: rerun the Batch 10 smoke after closing any already-open Unity editor instance for `Project_AAA`, so the repaired continuity seam can be verified end-to-end.

## Batch 70 Close-Out

- Selected branch: `A`
- Selected signature pair: `city-b -> dungeon-beta`
- Integration polish focused on the existing signature loop instead of adding a new mechanic seam.
- World/result readback polish:
  - priority-board summary now carries urgency, latest result evidence, and current route answer in the same line
  - selected-city pressure-board summary now mirrors that same evidence-first structure
  - world alert/city summaries now read more like a loop close-out than a neutral status snapshot
- Why Beta was chosen:
  - its guarded vs greedy split is still the clearest operating-scenario contrast on the surfaced rail
  - the City B pressure -> route answer -> burst payoff -> restart feedback chain is the easiest 3-5 minute demo story
- UI shape changed?: `No`

## Batch 70 Validation Snapshot

- Compile: `BLOCKED (Unity batch compile aborted because the project was already open in another editor instance)`
- Targeted audit:
  - 66~69 landing on current branch: `PASS`
  - Beta world -> prep -> route -> battle -> result loop continuity: `PASS`
  - result-pressure board summary alignment: `PASS`
- Manual verification: `DEFERRED`
- Smoke: `DEFERRED`

## Batch 70 Why It Matters

- Batch 66~69 already landed on the current branch, so Batch 70 did not reopen that work or fake a new demo seam.
- The strongest current signature rail is `city-b -> dungeon-beta`, because the world pressure, guarded vs greedy route split, burst-window combat payoff, and restart-focused world feedback already exist on the same canonical path.
- The remaining gap was presentation parity: the result-pressure board was carrying the right data, but not summarizing the latest return as directly as the surrounding prep/route/battle readback.
- Batch 70 closes that gap without changing the accepted runtime UI or widening the surfaced portfolio.

## Batch 69 Close-Out

- Selected branch: `A`
- World surfaces changed:
  - top world alert ribbon
  - world selection brief
  - world selection detail
  - world overview brief
  - selected-city world source snapshot
  - world observation priority board
- Pressure signals surfaced:
  - priority city selection from the existing world board read model
  - urgency / top bottleneck framing
  - route answer / recommendation answer
  - recent-result evidence
  - party readiness / recovery pressure
  - next-action follow-through
- Recent-result evidence surfaced through:
  - selected city pressure-board summary
  - top priority-board alert summary
  - world return handoff brief/detail
  - world observation snapshot summary
- Party-readiness surfaces affected:
  - alert ribbon footer
  - selected city brief/detail
  - world overview summary
- UI shape changed?: `No major layout change`

## Batch 69 Validation Snapshot

- Compile: `PASS`
- Targeted audit:
  - CityDecision read model reused on world/city board surfaces: `PASS`
  - recent result -> recommendation / next action alignment: `PASS`
  - party readiness surfaced in the same decision context: `PASS`
- Manual verification: `DEFERRED`
- Smoke: `DEFERRED`

## Batch 69 Why It Matters

- The world map now reads less like a neutral bridge and more like a decision board.
- The player can now read, on the same surface:
  - which city is currently the priority
  - why that city matters now
  - what route / dispatch answer the board is pointing at
  - what the latest expedition changed
  - whether the party is ready to answer that pressure
- This stays on the current world/city decision seam instead of adding a new strategy layer, fake urgency text, or a cosmetic-only panel expansion.

## Batch 68 Close-Out

- Selected branch: `A`
- Surfaced routes touched:
  - `city-a -> dungeon-alpha -> safe`
  - `city-a -> dungeon-alpha -> risky`
  - `city-b -> dungeon-beta -> risky`
  - `city-b -> dungeon-beta -> safe`
- Scenario framing added to the canonical route-meaning rail:
  - short scenario label
  - choose-when text
  - watch-out text
  - follow-up / next-dispatch trace
  - party-fit text
  - combat-plan text
- Party-fit surfaces affected:
  - route option reward preview
  - recommendation reason
  - dispatch route-fit summary
  - expected-need / projected-outcome readback
- City-pressure surfaces affected:
  - recommended route summary
  - recommendation reason
  - projected outcome summary
  - CityHub surfaced route-variant opportunity reason
- UI shape changed?: `No`

## Batch 68 Validation Snapshot

- Compile: `PASS`
- Validator: `PASS`
- Authoring validation:
  - `ProfilesChecked=3`
  - `SupportedVariantsChecked=1`
  - `WarnCount=0`
  - `FailCount=0`
- Manual verification: `DEFERRED`
- Smoke: `DEFERRED`

## Batch 68 Why It Matters

- The current four surfaced routes now read more like operating scenarios than reward table variants.
- Each surfaced route now answers:
  - when to choose it
  - what risk it asks you to absorb
  - what kind of party it prefers
  - what kind of next-dispatch footprint it leaves behind
- The change stays on the existing canonical authoring rail instead of adding another UI-only text layer or widening the surfaced route portfolio.

## Batch 67 Close-Out

- Selected branch: `A`
- Core combat pleasure locked to:
  - `read intent -> create window -> cash payoff`
- Setup mechanics added/used:
  - enemy next-action preview now refreshes during party turns
  - `Power Strike` reads that preview and opens a burst window on the chosen target
  - exposed targets now carry explicit burst-window state/readback instead of hiding setup only in raw numbers
- Payoff mechanics added/used:
  - `Weak Point` now consumes the burst window for a larger single-target payoff
  - `Arcane Burst` now gains bonus splash damage on exposed targets without consuming the window
  - basic `Attack` now gets only a light exposed bonus, so it stays readable but does not replace the setup-payoff loop
- Party roles affected:
  - `Warrior`: setup opener
  - `Rogue`: burst cash-in finisher
  - `Mage`: exposed-target splash follow-through
  - `Cleric`: recovery + burst-window stabilization
- Enemy intent/readback surfaces affected:
  - enemy roster state label
  - target focus rule text
  - command/context threat summary
  - command detail effect text
- UI shape changed?: `No`

## Batch 67 Validation Snapshot

- Compile: `PASS`
- Targeted audit:
  - intent preview during party turn: `PASS`
  - burst-window lifetime and consumption: `PASS`
  - command/target readback for setup vs payoff: `PASS`
- Manual verification: `DEFERRED`
- Smoke: `DEFERRED`

## GPT Quick Judge

- `city-b -> dungeon-beta -> safe`: `PASS:fully-canonical-and-surfaced`
- Surfaced portfolio count: `4`
- Meaningfully distinct surfaced opportunities: `4`
- Canonical but not surfaced on current Alpha/Beta `safe/risky` rail: `None`
- Surfaced using fallback: `None`
- Surfaced consumer mismatch: `None`
- Expansion gate: `C:tooling-next`
- Alpha safe/risky content identity: `route-specific shared encounter/city meaning aligned`
- Beta safe/risky content identity: `route-specific shared encounter/city meaning aligned`

## Current Surfaced Portfolio

- `city-a -> dungeon-alpha -> safe`
- `city-a -> dungeon-alpha -> risky`
- `city-b -> dungeon-beta -> risky`
- `city-b -> dungeon-beta -> safe`

## Draft Helper State

- Current helper status: usable
- Current draft count: `1`
- Promotable drafts on current surfaced rail: `0`
- Blocked drafts: `1`
- Blocked by canonical owner: `1`
- Supported rail slots: `4`
- Occupied supported rail slots: `4`
- Open supported rail slots: `None`
- Open supported resolver keys: `None`
- Promotion recommendation: `no-open-supported-resolver-key-on-current-rail`

## Batch 66 Close-Out

- Selected branch: `B`
- Party seam used or created:
  - `Assets/_Game/Scripts/Rpg/PrototypeRpgPartyData.cs`
  - `Assets/_Game/Scripts/Rpg/PrototypeRpgRuntimeResolveSurface.cs`
  - `Assets/_Game/Scripts/World/StaticPlaceholderWorldView.PartyRuntimeSurface.cs`
- Party archetypes now on the canonical runtime path:
  - `Bulwark Crew`
  - `Outrider Cell`
  - `Salvager Wing`
- Lightweight growth axis now on the canonical runtime path:
  - `Recruit Frame -> Field Promotion -> Elite Promotion`
- World runtime now stores per-party:
  - archetype id
  - promotion state id
  - derived power
  - derived carry
- World / prep / battle readback now all resolve from the same runtime party seam instead of rebuilding a raw placeholder party in each surface.
- Route recommendation and expected-impact text remain on the current surfaced four-route portfolio, but now explicitly explain when the current party fits or pushes against the chosen lane.
- Battle runtime now resolves party members from the promotion-aware RPG surface, so party differentiation shows up as actual stat/loadout/runtime differences instead of flavor-only text.

## Batch 66 Validation Snapshot

- Compile: `PASS`
- Targeted audit: `PASS`
- Party registry/runtime seam: `PASS`
- World + prep route-fit readback: `PASS`
- Battle party runtime promotion readback: `PASS`
- Smoke: `DEFERRED` because this batch changed runtime seam + readback wiring, but no manual playthrough was executed in this close-out

## Batch 66 Follow-Up Hook

- Keep the current surfaced Alpha/Beta four-route portfolio unchanged.
- If Batch 67 expands this area, prefer:
  - stronger route-specific consequences for each party archetype
  - longer-lived specialization persistence/save hook
  - additional surfaced explanation only if it stays on the same canonical party seam

## Batch 61 Close-Out

- Batch 61 did not add a new surfaced opportunity.
- Batch 61 deepens the existing Beta surfaced pair instead of widening the portfolio.
- Beta risky now reads as the raid line:
  - raider gate breach
  - plunder cache tempo
  - crossfire vault
  - chief's vault payout
- Beta safe now reads as the guarded restart line:
  - scout gate watch
  - watchfire and cache stability
  - guarded vault hold
  - captain's hold finish
- The current Beta pair now resolves through route-specific shared encounter/city meaning where the generic shared profiles had made the dungeon feel thin.

## Batch 61 Validation Snapshot

- Compile: `PASS`
- Validator: `PASS`
- Surfaced matrix summary: `PASS`
- Surfaced portfolio summary: `PASS`
- Representative chain status summary: `PASS`
- Representative reference trace: `PASS`
- Smoke: `DEFERRED` because this batch only changed content/data and docs

## Batch 61 Recommendation

- Recommended direction: `continue authored dungeon density on the current Alpha/Beta rail before reopening runtime/UI cleanup`
- Do not open a fake surfaced-expansion batch just because the content rail now reads better; the surfaced portfolio is still intentionally the same four routes.

## Batch 46 Close-Out

- Batch 46 did not add a new surfaced opportunity.
- Batch 46 adds a draft-promotion preflight summary on top of readiness/context.
- Draft tooling now reports:
  - supported rail slot inventory
  - occupied vs open supported slot counts
  - open supported resolver keys
  - whether the selected draft sits on an already-owned supported slot or outside the current rail
  - a saturated-rail recommendation when `Promotable drafts on current rail` honestly remains `0`

## Batch 46 Validation Snapshot

- Compile: `PASS`
- Validator: `PASS`
- Draft readiness summary: `PASS`
- Draft promotion preflight summary: `PASS`
- Draft promotion context summary: `PASS`
- Smoke: `DEFERRED` because this batch only changed editor/debug/helper and docs

## Batch 46 Recommendation

- Recommended direction: `retarget the draft beyond the current surfaced rail` or `widen the surfaced route seam intentionally`
- Do not open another surfaced-expansion batch on the current Alpha/Beta rail until the draft helper reports an open supported resolver key or the surfaced seam itself is deliberately widened.
