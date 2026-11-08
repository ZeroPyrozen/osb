# 08 · Re-check and date the time-sensitive units

Status: open
Blocked by: 02

## What

These 21 units cite the ranking criteria or osu!(lazer):

- `what-is-a-storyboard/03-the-stage`
- `first-commands/01-commands-and-time`, `04-move`, `06-knowledge-check`
- `scale-rotate-colour/01-scale`
- `shorthand/01-three-shorthands`, `02-tidy-up`, `03-knowledge-check`
- `loops/03-loop-pitfalls`, `05-knowledge-check`
- `timing-to-music/03-building-intensity`, `04-knowledge-check`
- `gameplay-and-layers/03-backgrounds-and-overlay`, `04-storyboard-sounds`
- `variables-and-triggers/02-triggers`
- `particles-and-procedural/01-particles`, `04-wave-of-bars`, `05-knowledge-check`
- `optimization-and-ranking/01-performance`, `02-ranking-criteria`, `04-knowledge-check`

For each one:

1. Check every time-sensitive sentence against the current osu! wiki (ranking criteria, storyboard pages)
   and lazer's `LegacyStoryboardDecoder`.
2. Fix anything that has changed.
3. Add `reviewed:` with the day of that check.

Then show "Facts last checked on <date>" on dated units, and make `verify-content` warn, without failing,
about any date more than three months old.

## Done when

- All 21 units are checked and dated, and anything that changed is fixed in the same commit.
- `verify-content` prints the warning for an old date and still exits 0.

## Commit

`feat: Date the units that cite the ranking criteria or lazer`
