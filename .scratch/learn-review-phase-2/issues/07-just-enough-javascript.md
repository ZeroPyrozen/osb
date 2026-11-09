# 07 · Add the Just enough JavaScript module

Status: done
Blocked by: 05

## What

A new module, `just-enough-javascript`, right before "Storyboarding with code" in the Advanced path,
written to the final rules from the start:

- a lesson on JavaScript for storyboards: variables, arithmetic, `for` loops, functions and arrays
- a lesson, "Points on a circle": `Math.cos`, `Math.sin` and radians
- one script-mode exercise whose checks run on the generated `.osb`, with hints that lead up to the solution
- no knowledge check: the exercise assesses all of the module's outcomes
- 3 to 6 outcomes, topics, a badge, and every example runnable in the playground

Then:

- "Storyboarding with code" lists the new module as a prerequisite.
- The Advanced path outcome on generator scripts names the new module among those that build it.
- The Advanced entry requirement stops asking for JavaScript.

Found while building:

- Script mode is only introduced in the next module, so this one uses just `sb.layer`, `sprite` and
  `fade`, plus `log`, whose last three values the playground shows under the editor.
- Examples that only print with `log` had no "Run in playground" button, so the button's rule now
  includes them (`isRunnableScript` in `Scripts/storyboard/script.js`). `verify-content` now runs every
  such example and checks the storyboard it writes, so "every example runnable" holds for the whole course.
- The module got its own icon, `braces`, and the Advanced path's summary mentions it.

## Done when

- `verify-content` passes: the starter fails its checks, the solution passes, and every example parses.
- There are no gaps, and the Advanced path shows the new module before "Storyboarding with code".

## Commit

`feat: Teach just enough JavaScript before storyboarding with code`
