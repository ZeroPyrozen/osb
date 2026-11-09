# osb! learn content

This folder is the whole course behind `/learn`: paths, modules, lessons, knowledge checks and
exercises. It's plain YAML and Markdown, so you can write and fix content without touching any code. The
words used here (path, module, unit, outcome, Bloom level and so on) are defined in
[GLOSSARY.md](../../GLOSSARY.md).

```
Content/Learn/
├── course.yml                  XP rules, levels, achievements, and the paths with their modules in order
├── what-is-a-storyboard/       one folder per module
│   ├── module.yml              title, summary, icon, badge, outcomes, topics, prerequisites
│   ├── 01-storyboards-in-osu.md
│   ├── 02-how-storyboards-are-made.md
│   └── …                       units, in file-name order
└── …
```

## Workflow

1. Run the site in Development (`dotnet run`, or `npm run watch` alongside it for scripts and styles).
   Content reloads by itself when you save a file. If something is wrong, the site keeps showing the
   last good version and logs every problem it found, with file names. It also logs the content's
   **gaps** as a warning (see [Mistakes and gaps](#mistakes-and-gaps)). Pages are cached for a minute
   for logged-out visitors, so an edit can take that long to show unless you're logged in.
2. Open the unit at `/learn/<module>/<unit>` and try it.
3. Run the content checks against the running site. Start the site on plain HTTP for this, because
   the checker can't follow a redirect to the development HTTPS certificate:

   ```
   dotnet run --urls http://localhost:5000
   npm run verify:content                  # checks http://localhost:5000
   npm run verify:content -- http://127.0.0.1:5094
   ```

   It checks that every exercise's **starter fails** its checks and its **solution passes** them, that
   every `.osb` example parses cleanly and only uses images from the playground library, and that every
   script example with a **Run in playground** button runs and does the same. It also warns, without
   failing, about [review dates](#review-dates) more than three months old.
4. Run the tests with `dotnet test --project Tests`. One of them loads the real course and fails with
   its list of gaps. CI runs it on every pull request.

The site refuses to start with broken content, so a deploy with broken content fails its health check
and rolls back on its own.

## Mistakes and gaps

The loader sorts what it finds into two kinds. A **mistake** makes the content inconsistent; a **gap**
only leaves it incomplete.

**Mistakes stop the site from starting**, so they can never reach learners. Besides missing files,
unknown unit types and broken YAML, these are mistakes:

- an outcome without an id, a Bloom level or a text, an id that isn't a lowercase slug, or an id used
  twice in one module or path
- a Bloom level other than remember, understand, apply, analyse, evaluate or create
- a question or exercise naming an outcome that isn't one of its own module's
- a prerequisite that isn't an earlier module in the course
- a path outcome naming a module that isn't in its path
- a review date that isn't a real date, or that lies in the future

**Gaps fail CI but never stop the site.** In Development they show as one warning in the log, when the
site starts and after each reload, and `dotnet test` lists them:

- a module or path with fewer than 3 or more than 6 outcomes
- a module without topics, or a path without an entry requirement
- a question or exercise that doesn't name the outcome it assesses
- an outcome that no question or exercise in its own module assesses
- a path outcome that doesn't name the modules that build it
- a module that no outcome of its path names

## Writing guidelines

- **Write original text.** The [osu! wiki](https://osu.ppy.sh/wiki/en/Storyboard) is the reference for
  facts, but its text is licensed CC BY-NC 4.0, so don't copy it. Explain things in your own words and
  link to the wiki under "Further reading".
- **Check facts against the game.** When the wiki is unclear, the behaviour of osu!(lazer)'s
  `LegacyStoryboardDecoder` (and the notes in `Scripts/storyboard/timeline.js`) is the tiebreaker.
- **Keep it short and hands-on.** A lesson is 5 to 8 minutes. Every new idea gets an example the
  learner can preview.
- **Use osu!'s spelling** in code and when naming things from the game: `Centre`, `Colour`.
- **Don't rename published units.** A unit's id is `<module folder>/<file name without the number>`, and
  learners' progress is stored under it. Renaming a file or folder makes everyone lose that unit.
  Renumbering (`03-` to `04-`) is fine.

## course.yml

- `xp`: XP for each unit type, the perfect-quiz bonus, and the bonuses for finishing a module or path.
- `levels`: XP thresholds. Keep existing thresholds fixed so nobody drops a level when content is added.
- `achievements`: names and descriptions. Their rules live in `Scripts/learn/gamification.js`.
- `paths`: each path, with its **modules in order**. A module folder that no path lists is an error.

```yaml
paths:
  - slug: intermediate
    title: Motion and music
    difficulty: Intermediate        # Beginner, Intermediate or Advanced
    summary: One or two sentences for the path on /learn.
    trophy: { name: In Sync, description: Finish the Intermediate path. }
    entry: the Beginner path. You should be able to write and predict F, M, S, R and C commands.
    outcomes:
      - id: music-timing
        bloom: apply
        modules: [timing-to-music]  # the modules in this path that build it
        text: Time effects to beats, bars and the song's structure
    modules:
      - shorthand
      - loops
      - timing-to-music
```

- `entry` is the path's entry requirement. The page shows it after "Before you start:", so it starts with
  a lowercase letter.
- `outcomes` are the path's 3 to 6 [outcomes](#writing-outcomes). Each one names the modules of this path
  that build it, and every module of the path is named by at least one.

Modules are numbered by their place in course.yml, so adding one renumbers the modules after it. Their
URLs and unit ids don't change.

## module.yml

```yaml
title: Loops
summary: One or two sentences for the module card.
icon: repeat            # a name from Helpers/Icons.cs
prerequisites: [first-commands, shorthand]
outcomes:
  - id: write-loop
    bloom: apply
    text: Write an L loop with relative inner times and the right pass count
  - id: loop-length
    bloom: apply
    text: Calculate a loop's pass length and total duration
  - …                   # 3 to 6 in all
topics:
  - The L line and its indentation
  - Relative times
badge:
  name: Round and Round
  description: Finished "Loops".
```

- `prerequisites` are the modules this one really builds on, not simply the one before it. Each must come
  earlier in the course, in this path or an earlier one. They're advice: the module page lists them under
  "Before you start", ticked once the learner has finished them, and never locks anything. A module with
  none leaves the list out.
- `outcomes` are what a learner can do by the end of the module. See [Writing outcomes](#writing-outcomes).
- `topics` are the subjects the module covers, for the Topics card on its page. Markdown is fine.

Icons: search, arrow-left, arrow-right, heart, logout, user, upload, discord, dots, menu, close, check,
chevron-right, chevron-down, external, download, play, pause, restart, clock, book, quiz, code, braces,
lock, star, trophy, flame, sparkles, map, layers, info, warning, grid, eye, image, curve, scissors,
repeat, film, music, gamepad, bolt, particles, gauge.

## Writing outcomes

An outcome says what a learner can do by the end of a module or path. The page finishes the sentence
"By the end of this module, you can …", so:

- **Start with a verb and leave out the full stop:** "Write an L loop with relative inner times and the
  right pass count".
- **Give each one Bloom level:** remember, understand, apply, analyse, evaluate or create, spelled the
  British way. Learners never see it. Choose the level the module's questions and exercises really reach,
  and only promise what its units teach.
- **Write 3 to 6** for every module and every path.
- **Give each an id**, a short lowercase slug that's unique in its module or path, such as `write-loop`.
  Ids aren't stored per learner, so you can rename one, as long as you rename its tags too.
- **Assess every module outcome** with at least one question or exercise in the same module. A module
  can't tag another module's outcomes: each module proves its own, which is what its badge says.
- **Quote the text** with single quotes if it contains `: ` or starts with a backtick.

## Adding a module

1. Make a folder named after the module's slug, with its `module.yml` and its units (`01-….md` and on).
2. Add the slug to its path's `modules:` in course.yml, at its place in the order.
3. Give it 3 to 6 outcomes, its topics and its prerequisites.
4. Tag every question with `outcome:` and every exercise with `outcomes:`, so each outcome is assessed.
5. Name the module in at least one of its path's outcomes.
6. Check it: the site starts, the development log shows no gaps, `dotnet test` passes and
   `verify:content` checks out.

Badges, trophies, achievements and bonus XP are worked out from finished units every time. A new module
therefore takes its path's trophy away from learners who had finished the path, until they finish the new
module too.

## Units

Every unit is a Markdown file named `NN-slug.md` with YAML front matter:

```markdown
---
title: Loops
type: lesson            # lesson, quiz or exercise
minutes: 7              # optional, defaults to 5 for lessons and 3 otherwise
summary: One sentence for lists and search.
reviewed: 2026-11-09    # only on units that cite the ranking criteria or osu!(lazer), see Review dates
---

The body, in Markdown.
```

Lessons don't name outcomes: the module's outcomes already say what its units teach. Knowledge checks and
exercises name the outcomes they assess, as shown below.

### Markdown

Standard Markdown plus tables, and these extras:

| Write | Get |
| :-- | :-- |
| ```` ```osb ```` | A storyboard example with **Preview** and **Open in playground** buttons, if it contains a Sprite or Animation line. |
| ```` ```osb-static ```` | Highlighted storyboard code with no preview, for examples the preview can't play (triggers). |
| ```` ```js ```` | Highlighted JavaScript. If it builds a storyboard (`sb.layer(`, `GetLayer(`) or prints with `log(`, it gets a **Run in playground** button (script mode), and `verify:content` runs it. |
| ```` ```cs ```` | Highlighted C#, for storybrew examples. |
| ```` ``` ```` | Plain code, for syntax templates and fragments. |
| `::: tip` … `:::` | A callout. Also `::: note`, `::: warning` and `::: try`. |
| `<div data-easing-explorer data-easing="30"></div>` | The interactive easing explorer, starting on easing 30. |

Level-two headings (`##`) appear in the "On this page" list.

### The playground library

Examples and exercises can use these images, all drawn in white so the Colour command tints them
exactly. Anything else shows as a checkerboard.

| Path | Size | What |
| :-- | :-- | :-- |
| `sb/dot.png` | 32 × 32 | small circle |
| `sb/circle.png` | 128 × 128 | circle |
| `sb/ring.png` | 128 × 128 | ring |
| `sb/square.png` | 100 × 100 | square |
| `sb/pixel.png` | 1 × 1 | single pixel, for bars and boxes |
| `sb/bar.png` | 12 × 120 | bar |
| `sb/glow.png` | 128 × 128 | soft glow |
| `sb/particle.png` | 16 × 16 | tiny soft dot |
| `sb/star.png` | 64 × 64 | star |
| `sb/triangle.png` | 100 × 88 | triangle |
| `sb/heart.png` | 64 × 58 | heart |
| `sb/note.png` | 48 × 64 | music note |
| `sb/text/hello.png` | text | "Hello, osu!" |
| `sb/lyrics/0.png` … `3.png` | text | the words "Feel", "the", "beat", "tonight" |
| `sb/spark.png` | 64 × 64 | 6-frame animation (`sb/spark0.png` to `sb/spark5.png`) |
| `bg.jpg` | 854 × 480 | night-sky background covering widescreen |
| `sb/hifumi.png` | 624 × 447 | Hifumi, the osb mascot |
| `sb/osb.png` | 256 × 256 | the osb! logo |

The list lives in `Scripts/storyboard/assets.js`.

### Quizzes

```yaml
---
title: Knowledge check
type: quiz
pass: 4                 # optional, defaults to 60% of the questions
questions:
  - prompt: In ` L,2000,4`, what does `4` mean?
    choices:
      - The commands play 4 times in total
      - The commands play once, then repeat 4 more times
    answer: 0           # counts from 0
    outcome: write-loop # the outcome of this module that the question assesses
    explanation: Shown once the learner passes. Markdown is fine.
---
```

Every question names exactly one outcome of its own module with `outcome:`. A knowledge check has no
`outcomes:` list of its own: it assesses the outcomes its questions name. Write each question at its
outcome's Bloom level where a multiple-choice question can reach it, such as a scenario to judge rather
than a rule to recall, with wrong choices drawn from real misconceptions.

Prompts and choices are Markdown too. Quote any that start with a backtick, or that contain `: ` or
` #`, with single quotes. Quote choices that are just a number (`- "20"`) so they stay text.

Learners see the choices in a new random order on every attempt, so never write a choice that depends
on its position, such as "All of the above" or "Both A and B". Still spread the `answer` positions
evenly in the file, so the source doesn't train anyone to expect the second option.

A failed attempt marks which questions were wrong but doesn't reveal the answers or their
explanations; a pass reveals everything. Only a perfect **first** attempt earns the perfect-quiz
bonus, because every later one comes after the learner has seen some feedback.

### Exercises

```yaml
---
title: "Exercise: fade in"
type: exercise
outcomes: [write-fades]   # the outcomes of this module that the exercise assesses
exercise:
  mode: osb             # osb (the default) or script (JavaScript, see the script-mode lesson)
  duration: 3000        # minimum length of the preview timeline, in ms
  widescreen: false     # preview in 4:3 instead of 16:9
  guides: true          # start with the play-area guides on
  bpm: 120              # optional metronome while playing
  offset: 0
  starter: |
    Sprite,Foreground,Centre,"sb/star.png",320,240
     F,0,1000,3000,1
  solution: |
    Sprite,Foreground,Centre,"sb/star.png",320,240
     F,0,1000,2000,0,1
     F,0,2000,3000,1
  checks:
    - kind: value
      time: 1000
      property: opacity
      equals: 0
      label: At 1000 ms the star is invisible
  hints:
    - Shown one at a time when the learner asks.
---

The instructions, in Markdown.
```

Every exercise lists at least one outcome of its own module with `outcomes:`, beside `exercise:`. A
module without a knowledge check needs its exercises to assess all of its outcomes.

"Show solution" unlocks after three failed checks of the learner's own code (checking the starter, or
the same code twice, doesn't count), after one such check once every hint has been shown, or straight
away if the learner has already completed the exercise. So give every exercise hints that lead up to
the solution. For script exercises, the checks run against the `.osb` the script generates.

### Checks

Every check has a `kind`, a `label` shown to the learner, and an optional `message` shown when it
fails. Most checks pick objects with an optional `object:` matcher: `{ path, layer, origin, is, index }`
where `is` is `sprite` or `animation` and `index` counts objects in file order from 0. Animations can
also be matched on `frameCount`, `frameDelay` and `loopType`. Without a matcher, a check looks at the
first object, or at every object with `every: true`.

| Kind | Passes when | Fields |
| :-- | :-- | :-- |
| `parses` | the script has no errors | |
| `count` | a count is in range | `of`: objects, sprites, animations, samples, loops, triggers, commands, command-lines or easings. `command` limits commands to one type. `equals`, `min`, `max`. |
| `object` | an object matching the fields exists | the matcher fields directly, plus `x`, `y` (and `tolerance`) |
| `command` | a matching command exists | `type`, `easing` (a number or a list), `start`, `end`, `from`, `to`, `param` |
| `value` | a property has the right value at a time | `time`, `property` (opacity, visible, x, y, position, scale, scaleX, scaleY, rotation, color, r, g, b), `equals`, `min`, `max`, `tolerance` |
| `visible` | an object is visible (or not, with `expect: false`) at a time | `time`, `expect` |
| `lifetime` | an object's life starts and ends at the right times | `start`, `end`, `endsBy` |
| `matches-solution` | the learner's storyboard looks the same as the solution, frame by frame | `step` (ms between compared frames, default 50), `tolerance` |

For `value` and `visible`, `time` can also be `start` or `end`: each object's own first or last moment,
handy for checking many generated particles at once.

Write checks about **what the learner sees**, not how they wrote it, so every correct answer passes.
Include at least one check the starter fails.

## Review dates

The ranking criteria change, and so does osu!(lazer). A unit that states a ranking-criteria rule or how
lazer behaves carries `reviewed:` with the day its facts were last checked, and learners see "Facts last
checked on <date>" under its title.

- **Check before you date.** Compare every time-sensitive sentence with the current
  [ranking criteria](https://osu.ppy.sh/wiki/en/Ranking_criteria), the osu! wiki's
  [storyboard pages](https://osu.ppy.sh/wiki/en/Storyboard/Scripting) and lazer's
  [`LegacyStoryboardDecoder`](https://github.com/ppy/osu/blob/master/osu.Game/Beatmaps/Formats/LegacyStoryboardDecoder.cs),
  fix whatever has changed, then set the date to that day. A date in the future is a mistake.
- **Re-check every quarter.** `verify:content` warns, without failing, about any date more than three
  months old.
- **Date new units too.** When you write a unit that cites the criteria or lazer, check and date it in
  the same change. To find the units that need a date, search for "ranking criteria" and "lazer" across
  line breaks, since a phrase can wrap onto the next line.
