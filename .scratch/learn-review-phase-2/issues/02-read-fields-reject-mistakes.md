# 02 · Read the new fields and reject mistakes

Status: open
Blocked by: 01

## What

Teach the loader the new fields, all optional for now so the current content keeps loading:

- module: `outcomes` (each `id`, `bloom`, `text`), `topics`, `prerequisites`
- path: `entry`, and `outcomes` (each `id`, `bloom`, `modules`, `text`)
- question: `outcome`
- exercise: `outcomes`, in the front matter beside `exercise:`
- any unit: `reviewed` (a YYYY-MM-DD date)

The course model gains them. Mistakes (see the spec) are reported with the other content problems, so the
site refuses to start:

- an outcome id that repeats within its module or path
- a Bloom level outside remember, understand, apply, analyse, evaluate, create
- a question or exercise tag naming an outcome that isn't in its own module
- a prerequisite that isn't an earlier module in the course (including the module itself)
- a path outcome naming a module that isn't in its path
- a review date that isn't a real date, or lies in the future

## Done when

- `Tests/Learn/CourseLoaderTests.cs` has a test for each mistake, using a temporary course folder.
- The real course loads unchanged and the pages look the same.

## Commit

`feat: Read outcomes, topics, prerequisites and review dates in the learn content`
