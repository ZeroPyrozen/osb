# 05 · Require the fields and check coverage in CI

Status: done
Blocked by: 04

## What

Turn the completeness rules into gaps (see the spec): they fail CI and show as warnings in the development
log, but never stop the site.

- a module or path with fewer than 3 or more than 6 outcomes
- a module without topics, or a path without an entry requirement
- a question or exercise without a tag
- an outcome that no question or exercise in its module assesses
- a path outcome that names no modules (added while building: nothing else checked the rule that each path
  outcome names the modules that build it)
- a module that builds no path outcome

The loader works out the gaps alongside the course. In Development the site logs them as warnings on start
and on every content reload. A test loads the real course and fails with the list of gaps, so CI catches
them before a merge.

## Done when

- Removing a tag, or an outcome's only question, makes the test fail with a clear message naming the file.
- The real course has no gaps.
- The site still starts with gaps present, and logs them.

## Commit

`feat: Fail CI when an outcome is assessed by nothing`
