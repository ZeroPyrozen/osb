---
title: Loops
type: lesson
minutes: 7
summary: The L command, times relative to each pass, and how long one pass lasts.
---

Pulsing lights, blinking stars and swaying grass all repeat the same motion again and again. Writing
every repetition by hand would take hundreds of lines. A **loop** writes it once and repeats it.

## Writing a loop

A loop is a command line of its own, followed by the commands to repeat, indented **one level deeper**:

```
 L,startTime,loopCount
  F,0,0,500,0,1
  F,0,500,1000,1,0
```

- `startTime` is when the first pass begins.
- `loopCount` is how many times the commands play in total.
- The commands inside use **relative times**, counted from the start of each pass.

Indent the commands inside the loop with two spaces (or two underscores). A line that goes back to one
space ends the loop.

This star fades in and out 4 times, starting at 1000 ms. Each pass lasts 1000 ms, so the loop runs
until 5000 ms:

```osb
Sprite,Foreground,Centre,"sb/star.png",320,240
 L,1000,4
  F,0,0,500,0,1
  F,0,500,1000,1,0
```

## How long is one pass?

osu! works out the length of a pass from the commands inside the loop: it runs from the **earliest
start** to the **latest end** among them. In the example that's 0 to 1000, so each pass lasts 1000 ms
and the passes follow each other without a gap.

The loop's total length is one pass multiplied by the loop count, and the object exists for all of it.

## Loops and other commands

A sprite can have ordinary commands alongside its loop, and even several loops one after another. Here
the heart pulses with a loop while an ordinary Move carries it across the screen:

```osb
Sprite,Foreground,Centre,"sb/heart.png",320,240
 C,0,0,4000,255,110,150
 M,0,0,4000,120,240,520,240
 L,0,8
  S,1,0,500,1.4,1
```

Each pass here is a single Scale command, so it lasts exactly 500 ms. Eight passes fill the four
seconds that the Move takes.

::: tip
Keep ordinary commands and loop commands on **different properties**, like the Move and the Scale
above. If both change the same property at the same time, the one that started later wins, and that
quickly gets confusing.
:::

## Further reading

- [Compound commands: loops](https://osu.ppy.sh/wiki/en/Storyboard/Scripting/Compound_Commands) on the osu! wiki
