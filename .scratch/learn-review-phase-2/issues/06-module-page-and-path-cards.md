# 06 · Show outcomes on the module page and path cards

Status: open
Blocked by: 03 (mockups agreed on 2026-11-09, see the spec's Pages section)

## What

Build what the agreed mockups show, keeping the new parts quiet (no accent colour, no pills or chips, 4 px
grid, new labels at weight 600):

- **Module page** (`Views/Learn/Module.cshtml`): "Before you start" above the Start button, linking to the
  prerequisites, each with the mint tick once the learner has finished that module or an empty ring if not.
  Below the progress bar, "By the end of this module, you can" with the outcomes as a plain list with small
  grey markers. The topics as a plain list between hairlines in a sidebar card.
- **Path cards** (`Views/Learn/Index.cshtml`): the entry requirement, and the path outcomes behind a "By the
  end of this path, you can" toggle, closed by default.
- The "How units work" note about knowledge checks only shows on modules that have one.
- Bloom levels appear nowhere.

## Done when

- The pages match the mockups at phone and desktop widths, in both themes.
- The page tests cover the prerequisite ticks.

## Commit

`feat: Show what each module and path teaches, and what to do first`
