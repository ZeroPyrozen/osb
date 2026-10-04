---
title: The ranking criteria
type: lesson
minutes: 8
summary: The rules and guidelines a storyboard must meet before its beatmap can be ranked, and the tools that check them.
---

Before a beatmap can be **ranked**, it goes through a review process, and everything in it has to meet
the **ranking criteria**, storyboard included. Even if you never plan to rank anything, the criteria
are a great checklist for a well-made storyboard.

The criteria separate **rules**, which must never be broken, from **guidelines**, which can be broken
in exceptional cases with a good explanation. They also change over time, so always check the current
version on the osu! wiki before submitting. Here's a summary of the parts that affect storyboards.

## Rules

From the storyboarding section:

- **No image may be larger than 17,000,000 pixels in area.** That's roughly 4100 × 4100. Larger images
  take too long to load on most computers.
- **The difficulty must load without parsing errors.** Every line of the storyboard must be readable.
- **Widescreen support must be the same** in every storyboarded difficulty, unless a difficulty's
  storyboard is designed for a different aspect ratio.

From the general rules, which apply to storyboards too:

- **Epilepsy warning:** repetitive strobes, pulsing images, or rapid changes in contrast, brightness or
  colour require one. Flashing at 3 Hz (three times a second) or slower is unlikely to cause concern.
- **No unused files** in the beatmap folder. Delete images you tried but didn't use.
- **Credit visual media:** the creators of artwork used in the storyboard must be credited in the
  beatmap's description.
- **No obscene imagery,** and imagery must follow the visual content considerations.
- **No AI-generated imagery.** Images and videos that are substantially AI-generated can't be used.
- **Letterbox during breaks** must be consistent between difficulties of the same mode with breaks.

## Guidelines

- **Nothing active after the song ends,** beyond a few extra seconds for an effect to finish.
- **Leave a one-pixel transparent border** around images that rotate, so their edges stay smooth.
- **Avoid noticeable performance problems.** Test-play to make sure frame rates stay steady.
- **Don't use storyboard sounds that could be mistaken for hitsounds** during gameplay.
- **Avoid illogical, conflicting and obsolete commands:** commands that end before they start, triggers
  that can never fire, and commands of the same type with overlapping times.
- **Match the widescreen setting to the storyboard:** on for widescreen designs, off for 4:3.
- **Optimise:** everything from the previous lesson's checklist.

## Tools

**Mapset Verifier** checks a beatmap against much of the ranking criteria automatically, storyboard
checks included, and the criteria themselves recommend it. Tools are a help, not a replacement:
read the criteria and check things yourself too.

## Further reading

- [Ranking criteria](https://osu.ppy.sh/wiki/en/Ranking_criteria) on the osu! wiki
- [Visual content considerations](https://osu.ppy.sh/wiki/en/Rules/Visual_content_considerations) on the osu! wiki
- [Mapset Verifier on GitHub](https://github.com/Naxesss/MapsetVerifier)
