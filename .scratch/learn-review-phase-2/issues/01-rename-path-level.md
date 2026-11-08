# 01 · Rename the path level to difficulty

Status: open
Blocked by: none

## What

Beginner, Intermediate and Advanced are a path's **difficulty** (see GLOSSARY.md), but the code calls them
`level`, which also names a learner's XP level. Rename the path's one everywhere:

- the `level:` key of each path in `Content/Learn/course.yml`
- `PathFile.Level` in `Learn/CourseLoader.cs` and `LearnPath.Level` in `Learn/CourseModel.cs`
- the path `level` in the catalog the browser gets (`BuildClientJson`)
- the labels in `Views/Learn/Index.cshtml`, `Module.cshtml` and `Unit.cshtml`
- test fixtures and tests that write a course.yml

The learner's `levels:` and everything about XP levels stay as they are.

## Done when

- Searching for a path `level` finds nothing; only XP levels remain.
- The pages show Beginner, Intermediate and Advanced exactly as before.
- All tests pass.

## Commit

`refactor: Call a path's Beginner, Intermediate or Advanced label its difficulty`
