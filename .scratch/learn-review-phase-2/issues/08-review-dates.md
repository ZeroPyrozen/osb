# 08 · Re-check and date the time-sensitive units

Status: done
Blocked by: 02

## What

These 23 units cite the ranking criteria or osu!(lazer). The plan listed 21; the search for it read line by
line and missed `scale-rotate-colour/02-rotate`, whose "ranking criteria" wraps across two lines, and step 4
added a question citing the 17,000,000-pixel limit to `scale-rotate-colour/06-knowledge-check`.

- `what-is-a-storyboard/03-the-stage`
- `first-commands/01-commands-and-time`, `04-move`, `06-knowledge-check`
- `scale-rotate-colour/01-scale`, `02-rotate`, `06-knowledge-check`
- `shorthand/01-three-shorthands`, `02-tidy-up`, `03-knowledge-check`
- `loops/03-loop-pitfalls`, `05-knowledge-check`
- `timing-to-music/03-building-intensity`, `04-knowledge-check`
- `gameplay-and-layers/03-backgrounds-and-overlay`, `04-storyboard-sounds`
- `variables-and-triggers/02-triggers`
- `particles-and-procedural/01-particles`, `04-wave-of-bars`, `05-knowledge-check`
- `optimization-and-ranking/01-performance`, `02-ranking-criteria`, `04-knowledge-check`

The two Easing units that mention osu!stable's opacity wrap-around aren't dated: stable is in maintenance, so
that behaviour won't change.

For each one:

1. Check every time-sensitive sentence against the current osu! wiki (ranking criteria, storyboard pages)
   and lazer's `LegacyStoryboardDecoder`.
2. Fix anything that has changed.
3. Add `reviewed:` with the day of that check.

Then show "Facts last checked on <date>" on dated units, and make `verify-content` warn, without failing,
about any date more than three months old.

## The check, on 2026-11-09

Sources: the osu! wiki's Ranking criteria (last changed 2026-11-04, a skinning clarification) and its
storyboard scripting pages, and lazer's `LegacyStoryboardDecoder.cs` (last changed 2026-08-03, for variables).

- Nothing the units say has changed. The criteria's changes since September (an osu!catch fix, a skinning
  clarification, an allowance for official tournament storyboards) don't touch what the units teach.
- Lazer still reads only the first start and end values of a command, so the sequence warnings stand.
- Three sentences were less exact than the criteria, and now say it: Widescreen support may differ between
  difficulties whose storyboards are made for different aspect ratios (`what-is-a-storyboard/03-the-stage`,
  `gameplay-and-layers/03-backgrounds-and-overlay`), and Letterbox during breaks may differ when their
  storyboards differ (`gameplay-and-layers/03-backgrounds-and-overlay`,
  `optimization-and-ranking/02-ranking-criteria`).

## Done when

- All 23 units are checked and dated, and anything that changed is fixed in the same commit.
- `verify-content` prints the warning for an old date and still exits 0.

## Commit

`feat: Date the units that cite the ranking criteria or lazer`
