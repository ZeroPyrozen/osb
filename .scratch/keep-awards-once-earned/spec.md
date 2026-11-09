# Keep awards once earned

Status: parked (raised while planning phase 2 of the learn review on 2026-11-08); decide before the public
launch

## Problem

Badges, trophies, achievements and their bonus XP aren't stored: they're recalculated from finished units
every time. When a module or path gains a unit, everyone who had finished it loses the badge or trophy, and
its bonus XP (50 for a module, 150 for a path), until they finish the new unit. That can drop them a level,
although course.yml keeps the level thresholds fixed precisely so nobody loses a level when content is
added.

Phase 2 adds a module to the Advanced path, and phases 3 and 4 add units to existing modules. Before the
launch only learners on the local network have progress, so phase 2 accepts it.

## Options

- (a) Keep recalculating, and accept that new content reopens finished modules and paths.
- (b) Store each award with the time it was earned, and keep it and its bonus XP from then on.
- (c) Keep earned awards, and mark modules and paths that gained units since, so learners see what's new.

## Things to weigh

- Signed-in learners sync progress to the server; anonymous learners keep it in the browser. Both need the
  same answer.
- Stored awards need a migration and a backfill from existing completions.
- Whatever is chosen is hard to reverse once the site is public and deserves an ADR.
