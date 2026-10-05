---
title: Loop pitfalls
type: lesson
minutes: 6
summary: Missing pauses, late first commands, counting passes, and what loops can't contain.
---

Loops are simple, but a few details catch out nearly everyone at least once.

## Pauses don't count

A pass lasts from the earliest start to the latest end of the commands inside the loop, so **a pause at
the end of a pass doesn't exist** unless a command covers it.

Say you want a dot that flashes on for 200 ms, then stays off for 300 ms, every half second. This looks
right, but each pass is only 200 ms long, because nothing runs after 200:

```osb
Sprite,Foreground,Centre,"sb/dot.png",220,240
 S,0,0,3000,3
 L,0,6
  F,0,0,200,1,0
```

The fix is a command that holds the value for the rest of the pass. Now each pass really lasts 500 ms:

```osb
Sprite,Foreground,Centre,"sb/dot.png",420,240
 S,0,0,3000,3
 L,0,6
  F,0,0,200,1,0
  F,0,200,500,0
```

Preview both and compare how often they flash.

## Start the first command at 0

The commands inside a loop should normally start at **0**. If the earliest one starts at 200, two things
happen: the first pass begins 200 ms after the loop's start time, and the pass is 200 ms shorter than
the numbers suggest. Move the start time of the L line instead, and keep the commands inside starting
at 0.

## Counting passes

The loop count is the **total** number of passes, not the number of extra repeats: `L,0,4` plays its
commands 4 times. A count of 0 or 1 plays them once.

## What loops can't do

- **No nesting.** A loop can't contain another loop or a trigger.
- **No objects.** Loops repeat commands on one object. To repeat across many objects, you need one loop
  on each of them, or a script (see the Advanced path).
- **No easing on the loop itself.** Each command inside has its own easing as usual.

## Loops keep files small

Because the commands are written once, loops make files much smaller than writing every repetition
out. The ranking criteria recommend them for anything that repeats many times, unless the repetitions
need to differ.

## Further reading

- [Compound commands: loops](https://osu.ppy.sh/wiki/en/Storyboard/Scripting/Compound_Commands) on the osu! wiki
