---
title: What is easing?
type: lesson
minutes: 6
summary: Same start, same end, same duration, yet completely different motion. In, Out and In/Out.
---

So far every command has changed at a steady speed. A dot moving from left to right covered the same
distance every millisecond. That's **linear** motion, easing `0`, and it looks mechanical, because
almost nothing in real life moves like that. A car speeds up as it pulls away and slows down before
it stops. A thrown ball slows as it rises.

**Easing** changes *how* a value travels from its start to its end. The command still starts and ends
at the same times with the same values. Only the pace in between changes.

Pick an easing below and compare it with linear motion:

<div data-easing-explorer data-easing="1"></div>

## In and Out

Most easings come in two directions:

- **Out** easings start **fast** and **slow down** at the end. The motion "eases out" as it arrives.
  Use them for things that arrive or settle: an object entering the screen, a pop-in, a landing.
- **In** easings start **slowly** and **speed up**. The motion "eases in" as it gets going. Use them for
  things that leave or launch: an object flying off screen, a fade-out that accelerates.

Many families also have **In/Out**, which is slow at both ends and fast in the middle. It's ideal for
moving between two places that are both on screen.

## Writing it

The easing is the number right after the command type. The original pair is `1` (Easing Out) and `2`
(Easing In). These three dots race over the same distance in the same two seconds: white is linear,
mint is easing 1 (Out), gold is easing 2 (In).

```osb
Sprite,Foreground,Centre,"sb/dot.png",100,160
 M,0,0,2000,100,160,540,160
 F,0,0,3000,1
Sprite,Foreground,Centre,"sb/dot.png",100,240
 M,1,0,2000,100,240,540,240
 F,0,0,3000,1
 C,0,0,3000,97,188,166
Sprite,Foreground,Centre,"sb/dot.png",100,320
 M,2,0,2000,100,320,540,320
 F,0,0,3000,1
 C,0,0,3000,240,200,90
```

All three arrive at exactly the same moment. Easing never changes **when** a command ends, only how it
spends its time.

## Not just for moving

Every command with values can be eased: Fade, Move, Scale, Vector scale, Rotate and Colour. An eased
fade looks softer, an eased spin winds down gently, and an eased scale can make a sprite pop. This star
grows quickly at first and then settles:

```osb
Sprite,Foreground,Centre,"sb/star.png",320,240
 S,1,0,1000,0,3
 F,0,0,2500,1
```

## Further reading

- [The easing table](https://osu.ppy.sh/wiki/en/Storyboard/Scripting/Commands) on the osu! wiki
- [easings.net](https://easings.net), a visual cheat sheet for the common easing curves
