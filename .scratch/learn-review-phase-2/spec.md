# Phase 2 of the learn review: the outcomes framework

Status: agreed on 2026-11-08, being built on branch `learn-review-phase-2`

Phase 2 of the action plan in the osb! learn curriculum review of 27 October 2026 (a private review page,
https://claude.ai/artifact/Uxy8DccQLXJoKzwqY9oszm). It covers three findings:

- **F1**: no path, module or unit states a learning outcome or a topic list.
- **F2**: the Advanced path assumes JavaScript and trigonometry it never teaches.
- **F18**: time-sensitive facts carry no date and no review cycle.

Phase 1 (F7–F9, F15–F17 and the F19 reading lists) merged in PR #15. The words used here are defined in
[GLOSSARY.md](../../GLOSSARY.md).

## Goal

Every path and module says what learners will be able to do. Every question and exercise is tied to one of
those outcomes, and CI fails when an outcome is assessed by nothing. The Advanced path teaches the JavaScript
it needs, and units that cite the ranking criteria or osu!(lazer) say when their facts were last checked.

## Out of scope

- The rest of F9, completions that used the solution: parked in
  [solution-assisted-completions](../solution-assisted-completions/spec.md).
- Whether badges, trophies and bonus XP stay earned once earned: parked in
  [keep-awards-once-earned](../keep-awards-once-earned/spec.md), to decide before the public launch.
- Per-learner outcome progress. Outcomes are descriptive: they're shown on pages and used by the coverage
  check, and nothing about them is stored per learner.
- Phases 3 and 4: better questions for the partly assessed outcomes, the lyrics and planning units, placement
  checks and the three project modules.

## Decisions

### Outcomes

- Every module and every path has 3 to 6 outcomes.
- An outcome's text starts with a verb and has no full stop, because the page heading finishes the sentence
  ("By the end of this module, you can …"): "Write an L loop with relative inner times and the right pass
  count".
- Every outcome has one Bloom level: remember, understand, apply, analyse, evaluate or create. Learners
  never see it.
- Outcome ids are short slugs, unique within their module or path (`write-loop`). They aren't stored per
  learner, so they can be renamed at any time.
- Each path outcome names the modules in its own path that build it, and every module builds at least one
  path outcome.
- The text is the review's, as the starting point, minus what needs later phases: path outcomes B5, I5 and
  A5 (built by the phase 4 projects), the two module outcomes that need new units (timing a LoopOnce
  animation to a beat, and timing lyrics, which also leaves "lyrics" out of I3), the "or its placement
  check" wording in the entry requirements, and the topics the review marked as additions. They come back
  with the units that make them true.
- Timing to music's planning outcome is reworded to the level its lessons teach today: "Match effects to
  the song's sections, kiai, build-ups and drops" (analyse, decided 2026-11-09). Planning a whole
  storyboard comes back with phase 3's planning lesson and phase 4's Intermediate project.

### Tags

- Every question names exactly one outcome of its own module.
- Every exercise names at least one outcome of its own module.
- Lessons get no tag: listing an outcome on a module already says its units teach it.
- A unit can't tag another module's outcome. Each module proves its own outcomes, which is what its badge
  certifies.

### Prerequisites and entry requirements

- Both are advice. Nothing ever locks.
- A module lists only the modules it actually builds on, not simply the one before it. A prerequisite can be
  in an earlier path but must come earlier in the course. A module with none shows no "Before you start".
- Each path has an entry requirement, taken from the review. The Advanced one stops asking for JavaScript
  once the path teaches it (step 7).

### Mistakes stop the site, gaps fail CI

The rule of thumb: a mistake makes the content inconsistent, a gap leaves it incomplete. A half-written
outcome shouldn't take the site down, but a typo should never reach learners.

- **Mistakes** stop the site from starting, as broken content does today (a deploy then fails its health
  check and rolls back): an unknown or duplicate outcome id, a Bloom level outside the six, a tag naming an
  outcome that isn't in its own module, a prerequisite that isn't an earlier module, a path outcome naming a
  module outside its path, and a review date that isn't a real date or lies in the future.
- **Gaps** fail CI and show as warnings in the development log, but never stop the site: a module or path
  with fewer than 3 or more than 6 outcomes, a module without topics, a path without an entry requirement, a
  question or exercise without a tag, an outcome nothing assesses, a path outcome that names no modules, and
  a module that builds no path outcome.

The coverage check is strict from day one: step 4 writes the five questions that close today's gaps. The 16
outcomes the review judged only partly assessed need judgement, not a script, so they stay with phase 3.

### Pages

Mocked up on a design canvas (private: https://claude.ai/artifact/Mmj48DtaNjauX8ZsLdzVve) and agreed on
2026-11-09 after an unslop pass. The new parts stay quiet: no accent colour, no pills or chips, spacing on
the 4 px grid, new labels at weight 600, and the mint tick kept for "finished".

- **Module page**: "Before you start" above the Start button, linking to the prerequisites, each with the
  mint tick once the learner has finished it or an empty ring if not. Below the progress bar, "By the end of
  this module, you can" with the outcomes as a plain list with small grey markers. The topics go in the
  sidebar as a plain list between hairlines.
- **Path cards on /learn**: the entry requirement after "Before you start:" (so it starts in lowercase), and
  the path outcomes behind a "By the end of this path, you can" toggle, closed by default, so the page
  doesn't grow by twelve lines.
- **Dated units**: "Facts last checked on <date>" in muted text under the title, without an icon.
- Bloom levels are never shown to learners.
- The "How units work" note about knowledge checks only shows on modules that have one.
- The site-wide signs the unslop pass found (uppercase spaced-out labels, 700-weight headings, gold XP on
  every unit row, shadows on dark cards) are left alone in phase 2.

### Just enough JavaScript

- A new module, `just-enough-javascript`, right before "Storyboarding with code" in the Advanced path, with
  its own badge.
- Two lessons and one script-mode exercise: a JavaScript lesson (variables, arithmetic, `for` loops,
  functions, arrays) and a "Points on a circle" lesson (`Math.cos`, `Math.sin`, radians). Every example runs
  in the playground.
- No knowledge check: the exercise assesses all of the module's outcomes.
- Learners who already code get through by finishing it quickly. There are no optional units.
- "Storyboarding with code" lists it as a prerequisite, and it builds the Advanced path outcome on generator
  scripts.
- The modules after it get new numbers ("Storyboarding with code" becomes module 13). URLs and unit ids
  don't change.

### Review dates

- Units that cite the ranking criteria or osu!(lazer) carry a review date. 21 units do today (step 8 lists
  them).
- Before a unit is dated, every time-sensitive sentence in it is checked against the current osu! wiki and
  lazer's source and anything that changed is fixed. The date is the day of that check.
- Learners see "Facts last checked on <date>".
- `verify-content` warns, without failing, when a date is more than three months old. The README describes
  a quarterly review.

### Names in code

- A path's `level` becomes `difficulty` everywhere (course.yml, the loader, the catalog the browser gets,
  page labels and tests), so "level" only ever means a learner's XP level.
- The Bloom level's key is `bloom`.

### Accepted side effect

Badges, trophies, achievements and bonus XP are recalculated from finished units every time. Adding the new
module therefore reopens the Advanced trophy and "Wizardry" for anyone who had them, and takes away the
150 XP path bonus until they finish it, which can drop a level. Phases 3 and 4 will do the same to the
modules they add units to. Before the launch only learners on the local network have progress, so this is
accepted for now; see [keep-awards-once-earned](../keep-awards-once-earned/spec.md).

## Shapes

```yaml
# course.yml: one path
- slug: advanced
  title: Scripting and wizardry
  difficulty: Advanced
  summary: …
  entry: The Intermediate path, and angles in radians from "Scale, rotate and colour".
  outcomes:
    - id: generator-scripts
      bloom: create
      modules: [just-enough-javascript, storyboarding-with-code]
      text: Write generator scripts with loops, maths and seeded randomness, and carry them over to storybrew

# <module>/module.yml
prerequisites: [first-commands, shorthand]
outcomes:
  - { id: write-loop, bloom: apply, text: Write an L loop with relative inner times and the right pass count }
topics:
  - The L line and its indentation

# a question in a knowledge check
- prompt: In ` L,2000,4`, what does `4` mean?
  outcome: write-loop

# an exercise's front matter, beside its exercise: block
outcomes: [write-loop]

# a unit that cites the ranking criteria or lazer
reviewed: 2026-11-10
```

## Build steps

Each step is one commit. `dotnet test --project Tests`, `npm test` and `npm run verify:content` pass after
every one.

1. [Rename the path level to difficulty](issues/01-rename-path-level.md)
2. [Read the new fields and reject mistakes](issues/02-read-fields-reject-mistakes.md)
3. [Add outcomes, topics, prerequisites and entry requirements](issues/03-outcomes-content.md)
4. [Write five questions for the unassessed outcomes](issues/04-five-new-questions.md)
5. [Require the fields and check coverage in CI](issues/05-require-fields-check-coverage.md)
6. [Show outcomes on the module page and path cards](issues/06-module-page-and-path-cards.md)
7. [Add the Just enough JavaScript module](issues/07-just-enough-javascript.md)
8. [Re-check and date the time-sensitive units](issues/08-review-dates.md)
9. [Document the new fields and rules in the README](issues/09-readme.md)
