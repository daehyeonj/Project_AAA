# CityHub Pressure Board UX Checklist

Use this checklist for the selected CityHub / selected-city result-pressure board after Batch80.1.

## Capture Setup

- Enter `WorldSim` from the main menu.
- Select `city-a`.
- Keep the selected CityHub / selected city board visible.
- Use the same resolution when comparing before/after captures.

## Required Screenshots

- Before any run: selected CityHub with no latest expedition result.
- After Stability Run return: `city-a -> dungeon-alpha -> safe` result returned to CityHub.
- Blocked/recovery state: after return, while recovery warning or block is visible.
- Ready state: after recovery/day advance or any state where prep is clearly available.
- Route recommendation state: selected CityHub with route recommendation visible.

## Five-Question Readability

- What happened? Confirm `Latest:` is visible and names the run/result, returned loot, and party state.
- Why did it matter? Confirm `Why:` is visible and explains the city pressure/relevance in player-facing language.
- What changed? Confirm `Changed:` is visible and shows stock/pressure/readiness change without fake numbers.
- What next? Confirm `Next:` is visible and points to the next sensible action.
- Can I act now? Confirm `Ready:` is visible and says ready, warning, or blocked.

## Density / Clipping

- Check whether any line overlaps another line.
- Check whether long lines clip before the useful clause.
- Check whether `Latest`, `Changed`, `Next`, and `Ready` can be found within 2 seconds.
- Check whether repeated facts feel noisy, especially returned loot vs stock delta.
- Check whether `None` appears where a better absence state should appear, such as `No recent run` or `No pressure change recorded`.

## Action Clarity

- Confirm the player can find `Open Expedition Prep` or the equivalent prep action.
- Confirm the recommended route is visible without reading debug labels.
- Confirm a blocked state says why it is blocked, such as no idle party, recovery, or no contract slot.
- Confirm recovery warning does not look like a hard block if re-entry remains allowed.

## Pass / Follow-Up Notes

- Pass if the board answers all five questions without duplicate wall-of-text density.
- Follow up if `Why:` or `Next:` is too long to scan.
- Follow up if the selected city board needs layout bounds or font tuning beyond copy ordering.
- Follow up if screenshots show the compact lines are still hidden behind dropdown/bottom sheet clipping.
