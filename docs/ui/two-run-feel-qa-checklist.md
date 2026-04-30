# Two-Run Feel QA Checklist

## Goal

Confirm that the second ExpeditionPrep does not just prove continuity, but makes the player want to choose between stability and surge.

## Setup

- Use the representative path: `city-a -> dungeon-alpha -> safe`.
- Complete or proof-drive the first Stability Run.
- Return to CityHub, then open ExpeditionPrep again.

## Required Readbacks

- Last run changed: the screen should show `Returned mana_shard x16 | Party Stable` or equivalent compact result evidence.
- Party carry-forward: the screen should show `Alden +16 XP` and `Rune equipped Stormglass Focus`.
- Stability appetite: the safe route should read like the HP/recovery/rhythm-preserving option.
- Surge appetite: the risky route should read like the payout option made more tempting by Rune's new focus.
- Launch warning: the gate should say launch is ready with warning, recovery risk, or fatigue pressure.
- Recommendation: the board should clearly say Stability is better if recovery matters, while Surge is justified if stock pressure outweighs strain.

## Pass Criteria

- The player can answer "what changed after the first run?" in under five seconds.
- The player can describe why Stability is safer without opening another panel.
- The player can describe why Surge is tempting without opening another panel.
- The launch warning feels like a meaningful cost, not a hard stop.
- No panel requires reading an internal key such as `run_clear` to understand the choice.

## Fail Signals

- The second Prep feels identical to the first Prep.
- Growth or gear carry-forward is hidden below dense result text.
- Stability and Surge read like generic safe/risky labels instead of different appetites.
- The recommendation tells the player what to do without explaining the tradeoff.
- The screen introduces new mechanics or promises rewards that the runtime does not support.

## Notes

Manual feel QA is not claimed by automated proof. Record screenshots or short clips of CityHub return plus second ExpeditionPrep before calling the player-facing pass complete.
