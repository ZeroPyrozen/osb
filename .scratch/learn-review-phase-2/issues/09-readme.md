# 09 · Document the new fields and rules in the README

Status: open
Blocked by: 01–08

## What

Update `Content/Learn/README.md`:

- course.yml: `difficulty`, `entry` (it reads on from "Before you start:", so it starts with a lowercase
  letter), and path `outcomes` with their `modules`
- module.yml: `outcomes` (id, bloom, text), `topics` and `prerequisites`
- quizzes and exercises: `outcome:` on every question, `outcomes:` on every exercise
- units: `reviewed:`, which units need it, and the quarterly review
- how to write an outcome: start with a verb, no full stop, one Bloom level, 3 to 6 per module and path
- mistakes stop the site, gaps fail CI, and where to see each
- a pointer to GLOSSARY.md for the words

## Done when

Someone can add a module that passes every rule from the README alone.

## Commit

`docs: Explain outcomes, tags, prerequisites and review dates`
