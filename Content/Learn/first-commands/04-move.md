---
title: Move
type: lesson
minutes: 6
summary: M, MX and MY. Paths made of several moves, entering from off-screen, and which command to pick.
---

Move (`M`) changes an object's position. It takes two pairs of values: where the object is at the
start time, and where it is at the end time.

```
 M,easing,start,end,startX,startY,endX,endY
```

This dot travels from the top-left of the screen to the bottom-right in three seconds:

```osb
Sprite,Foreground,Centre,"sb/dot.png",320,240
 M,0,0,3000,100,100,540,380
```

Notice that the Sprite line says (320, 240), but the dot never goes there. Once an object has a Move
command, the move decides the position, even before it starts (that's the third rule about time from
the previous lesson). The position on the Sprite line is only used when nothing moves the object.

## Moving along one axis

`MX` and `MY` move along just one axis. They take a single start value and a single end value:

```
 MX,easing,start,end,startX,endX
 MY,easing,start,end,startY,endY
```

They're handy when the two axes do different things at different times. Here the ring slides right
the whole time, but only drops down during the second half:

```osb
Sprite,Foreground,Centre,"sb/ring.png",120,140
 MX,0,0,3000,120,520
 MY,0,1500,3000,140,340
```

## Paths

To follow a path, chain several moves back to back, each one starting where the previous one ended.
This dot goes around a square:

```osb
Sprite,Foreground,Centre,"sb/dot.png",220,140
 M,0,0,1000,220,140,420,140
 M,0,1000,2000,420,140,420,340
 M,0,2000,3000,420,340,220,340
 M,0,3000,4000,220,340,220,140
```

::: tip
If a move doesn't start where the previous one ended, the object jumps. Sometimes that's what you
want (a cut to a new position), but usually it's a typo.
:::

## Entering from off-screen

Positions outside the 640 × 480 canvas are fine, and that's how objects slide in from the side. Keep
the object's **size and origin** in mind: a 48-pixel-wide image with the Centre origin at x = −20
still pokes 4 pixels onto the screen. Its centre must be at least half its width past the edge.

Also remember widescreen: on a 16:9 screen the visible area runs from x = −107 to x = 747, so
something parked at x = −50 is in plain view for most players.

## M, or MX and MY?

When x and y change over the same time span, use one `M` rather than an `MX` and an `MY`. It's half
the commands, and the ranking criteria list this as an optimisation. Save `MX` and `MY` for when the
axes really do change separately.

## Further reading

- [Storyboard scripting commands](https://osu.ppy.sh/wiki/en/Storyboard/Scripting/Commands) on the osu! wiki
