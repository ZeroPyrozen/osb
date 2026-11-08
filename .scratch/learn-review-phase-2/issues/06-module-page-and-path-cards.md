# 06 · Show outcomes on the module page and path cards

Status: open
Blocked by: 03, and the mockups approved on the design page

## What

Build what the approved mockups show:

- **Module page** (`Views/Learn/Module.cshtml`): "By the end of this module, you can" with the outcomes,
  then the topics, then "Before you start" linking to the prerequisites, each ticked once the learner has
  finished that module.
- **Path cards** (`Views/Learn/Index.cshtml`): the entry requirement, and the path outcomes behind a "By the
  end of this path, you can" toggle.
- The "How units work" note about knowledge checks only shows on modules that have one.
- Bloom levels appear nowhere.

## Done when

- The pages match the mockups at phone and desktop widths, in both themes.
- The page tests cover the prerequisite ticks.

## Commit

`feat: Show what each module and path teaches, and what to do first`
