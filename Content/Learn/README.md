# osb! learn content

This folder is the whole course behind `/learn`: paths, modules, lessons, quizzes and exercises. It's
plain YAML and Markdown, so you can write and fix content without touching any code.

```
Content/Learn/
├── course.yml                  XP rules, levels, achievements, and the paths with their modules in order
├── what-is-a-storyboard/       one folder per module
│   ├── module.yml              title, summary, icon, badge
│   ├── 01-storyboards-in-osu.md
│   ├── 02-how-storyboards-are-made.md
│   └── …                       units, in file-name order
└── …
```

## Workflow

1. Run the site in Development (`dotnet run`, or `npm run watch` alongside it for scripts and styles).
   Content reloads by itself when you save a file. If something is wrong, the site keeps showing the
   last good version and logs every problem it found, with file names.
2. Open the unit at `/learn/<module>/<unit>` and try it.
3. Run the content checks against the running site. Start the site on plain HTTP for this, because
   the checker can't follow a redirect to the development HTTPS certificate:

   ```
   dotnet run --urls http://localhost:5000
   npm run verify:content                  # checks http://localhost:5000
   npm run verify:content -- http://127.0.0.1:5094
   ```

   It checks that every exercise's **starter fails** its checks and its **solution passes** them, and
   that every `.osb` example parses cleanly and only uses images from the playground library.

The site refuses to start with broken content, so a deploy with broken content fails its health check
and rolls back on its own.

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
- `paths`: each path's slug, title, level, summary, trophy and its **modules in order**. A module folder
  that no path lists is an error.

## module.yml

```yaml
title: Loops
summary: One or two sentences for the module card.
icon: repeat            # a name from Helpers/Icons.cs
badge:
  name: Round and Round
  description: Finished "Loops".
```

Icons: search, arrow-left, arrow-right, heart, logout, user, upload, discord, dots, menu, close, check,
chevron-right, chevron-down, external, download, play, pause, restart, clock, book, quiz, code, lock,
star, trophy, flame, sparkles, map, layers, info, warning, grid, eye, image, curve, scissors, repeat,
film, music, gamepad, bolt, particles, gauge.

## Units

Every unit is a Markdown file named `NN-slug.md` with YAML front matter:

```markdown
---
title: Loops
type: lesson            # lesson, quiz or exercise
minutes: 7              # optional, defaults to 5 for lessons and 3 otherwise
summary: One sentence for lists and search.
---

The body, in Markdown.
```

### Markdown

Standard Markdown plus tables, and these extras:

| Write | Get |
| :-- | :-- |
| ```` ```osb ```` | A storyboard example with **Preview** and **Open in playground** buttons, if it contains a Sprite or Animation line. |
| ```` ```osb-static ```` | Highlighted storyboard code with no preview, for examples the preview can't play (triggers). |
| ```` ```js ```` | Highlighted JavaScript. If it calls `sb.layer(`, it gets a **Run in playground** button (script mode). |
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
    explanation: Shown after answering. Markdown is fine.
---
```

Prompts and choices are Markdown too. Quote any that start with a backtick, or that contain `: ` or
` #`, with single quotes. Quote choices that are just a number (`- "20"`) so they stay text.

### Exercises

```yaml
---
title: "Exercise: fade in"
type: exercise
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

The solution is revealed after the first failed check. For script exercises, the checks run against
the `.osb` the script generates.

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
