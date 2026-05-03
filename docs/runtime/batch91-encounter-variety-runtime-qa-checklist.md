# Batch91 Encounter Variety Runtime QA Checklist

Purpose: manually confirm Batch91's runtime proof/balance lock after the automated proof passes.

## Route Feel

- [ ] Stability / Slime Front feels safer, steadier, and more controlled than Surge.
- [ ] Surge / Goblin Pair Hall feels more pressured, more urgent, and more rewarding than Stability.
- [ ] The player can summarize the contrast as: `Stability let me keep control, while Surge pushed me harder but paid more.`

## Battle Readback

- [ ] Stability battle entry/context shows `Threat: Moderate`, controlled slime pressure, lower ATK/predicted damage, and Rest Shrine / Shrine Protection setup.
- [ ] Surge battle entry/context shows `Threat: High`, Cache Pressure, lowest-HP focus, higher ATK/predicted damage, and recovery strain.
- [ ] Enemy intent is readable without opening debug logs.
- [ ] Command/target preview makes Alden setup and Mira/Rune payoff clear, especially under Cache Pressure.

## Consequence Surfaces

- [ ] Stability encounter popover confirms the route check and the Rest Shrine / Shrine Protection plan.
- [ ] Surge encounter popover confirms payout/strain instead of only showing generic clear text.
- [ ] Result/world board still reflects real route consequence and does not invent a fake penalty.

## Regression Sanity

- [ ] Dungeon remains grid-based.
- [ ] Battle remains the standard JRPG command/target surface.
- [ ] Rest Shrine / Greed Cache `[E]` interactions still work.
- [ ] Batch10 smoke remains green.
- [ ] No UI skin/layout migration was introduced.
