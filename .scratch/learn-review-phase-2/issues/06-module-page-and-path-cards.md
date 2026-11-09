# 06 · Show outcomes on the module page and path cards

Status: done
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

Found while building:

- The entry requirement reads on from "Before you start:", as in the mockup, so the three in course.yml now
  start with a lowercase letter.
- The mockups' quiet grey for markers and the empty ring (#6b7976) wasn't in the palette, so it's now
  `charcoal-400` in `Styles/app.css`.
- Module pages scrolled sideways on phones, before this step too: the unit rows' one-line summaries widened
  the grid's implicit column. The grid now has an explicit single column below the desktop layout.

## Done when

- The pages match the mockups at phone and desktop widths. The site has only its dark theme, so that's the
  one checked.
- The page tests cover the prerequisite ticks.

## Commit

`feat: Show what each module and path teaches, and what to do first`
