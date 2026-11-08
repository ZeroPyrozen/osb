# 07 · Add the Just enough JavaScript module

Status: open
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

## Done when

- `verify-content` passes: the starter fails its checks, the solution passes, and every example parses.
- There are no gaps, and the Advanced path shows the new module before "Storyboarding with code".

## Commit

`feat: Teach just enough JavaScript before storyboarding with code`
