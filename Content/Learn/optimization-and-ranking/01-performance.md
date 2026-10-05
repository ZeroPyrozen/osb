---
title: Performance
type: lesson
minutes: 8
summary: What makes storyboards slow, and a checklist for keeping them smooth on modest computers.
---

A storyboard that stutters ruins the beatmap it was made for. Players feel every dropped frame,
and not everyone has a fast computer. Performance isn't an afterthought. It's part of the design.

## What costs performance

- **Active sprites.** Every sprite between its first and last command is processed every frame, even
  while it's completely invisible. A sprite at opacity 0 still costs.
- **Pixels.** Every visible pixel of every sprite has to be drawn, including transparent ones. Several
  large images stacked on top of each other (full-screen overlays, big glows) multiply the work.
- **Loading.** Every image is loaded into memory before the beatmap starts. Huge images take a long time
  to load and use a lot of memory.
- **File size.** Hundreds of thousands of commands make a big file that's slow to download and load.

## The checklist

These come straight from the optimisation advice in the ranking criteria, and they're good habits for
any storyboard, ranked or not.

**End sprites when you're done with them.** When a sprite fades out for good, make that its last
command, so it stops being active. If it disappears for a long time and comes back later, use a new
sprite for the second appearance instead of keeping the first one alive.

**Don't draw what nobody can see.** If something is completely covered, such as the background under a
full-screen image, fade it out while it's hidden.

**Crop your images.** Remove empty transparent space around them, and cut off parts that are off-screen
for the whole time they're used.

**Pick the right format.** JPG for large opaque images like backgrounds, PNG when you need transparency.
Never include the same image twice under different names.

**Write fewer commands.**

- Use loops for anything that repeats.
- Use one Move instead of an MX and an MY that change together.
- When a sprite has many commands of the same type, leave at least about 16 ms between their start times.
  Sixty commands a second is plenty for smooth motion.
- Remove commands that don't change anything.

**Clean up after triggers.** Sprites activated by triggers stay active until the end of the difficulty,
so fade them out once they've done their job.

**Test it.** Play the beatmap with the storyboard on, ideally on a slower computer too, and watch for
dropped frames.

::: tip
Generators make it easy to write far more commands than an effect needs, for example one command per
frame for a smooth curve. Many tools can simplify generated keyframes, dropping commands that make no
visible difference. storybrew's Spectrum effect does it, and it can shrink a file dramatically.
:::

## Further reading

- [Ranking criteria: storyboarding](https://osu.ppy.sh/wiki/en/Ranking_criteria#storyboarding) on the osu! wiki
