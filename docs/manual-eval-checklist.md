# Project AAA Manual Evaluation Checklist

## Runtime Reality

- Evaluate the current implementation as:
  - `grid dungeon`
  - `standard JRPG battle`
- Do not evaluate against earlier panel/lane combat ideas in this pass.

## Golden Path Checklist

1. Enter `MainMenu -> WorldSim`.
   - Check whether a city problem or pressure is visible before any expedition decision.
2. Move from `WorldSim -> CityHub`.
   - Check whether the city surface explains what matters now, not just raw stock labels.
3. Move from `CityHub -> ExpeditionPrep`.
   - Check whether ExpeditionPrep explains why this dungeon matters now.
   - Check whether recent impact / recommendation / why-now context is visible.
4. Launch into `DungeonRun`.
   - Check whether the grid dungeon still feels tied to a mission objective rather than becoming generic traversal.
5. Enter `BattleScene`.
   - Check whether the standard JRPG battle still communicates why this encounter matters to the expedition.
6. Finish the run and enter `ResultPipeline -> WorldSim`.
   - Check whether the result explains what changed and why it matters to the city.
7. Return to `CityHub`.
   - Check whether the changed bottleneck / opportunity / recommendation is understandable.
8. Re-enter `ExpeditionPrep`.
   - Check whether the previous result changes why this next expedition is being prepared.

## Positive Signals To Look For

- You can describe the city problem before launching.
- You can explain why the selected dungeon is relevant now.
- ExpeditionPrep re-entry shows a concise why-now reason instead of a duplicated or truncated block of result text.
- The prep prompt should now keep the why-now sentence fully readable without collapsing into `...` during the golden-path re-entry check.
- The grid dungeon and battle do not erase the mission context.
- The result clearly changes something in world/city state.
- The next city recommendation or prep prompt reacts to the returned result.

## Known Prototype Limits

- Content scale is intentionally small: 2 cities, 2 dungeons, 1 road.
- Explanations are still string-heavy and partially debug/HUD-driven.
- Some guidance is still explicit because the current goal is legibility, not final presentation polish.
- Hardcoded and placeholder data are acceptable in this evaluation as long as the causal loop remains readable.

## Observation Notes Template

- City problem observed:
- Why the expedition seemed relevant:
- Dungeon objective readability:
- Battle context readability:
- Result-to-world readability:
- CityHub follow-up readability:
- ExpeditionPrep re-entry readability:
- Biggest confusion point:
- Strongest positive signal:
