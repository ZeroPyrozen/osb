---
title: Rotate
type: lesson
minutes: 5
summary: Angles in radians, spins in both directions, and swinging things around their origin.
---

Rotate (`R`) turns an object. Its values are angles in **radians**, and positive angles turn
**clockwise**.

```
 R,easing,start,end,startAngle,endAngle
```

## Radians in one minute

A full turn is 2π radians, about **6.283**. To convert degrees, multiply by π ÷ 180 (about 0.01745):

| Turn | Degrees | Radians |
| :-- | :-: | :-: |
| eighth | 45° | 0.785 |
| quarter | 90° | 1.571 |
| half | 180° | 3.142 |
| full | 360° | 6.283 |

Angles can go past a full turn, so `12.566` means two complete spins, and negative angles turn
**anti-clockwise**. These two stars spin in opposite directions, one full turn each:

```osb
Sprite,Foreground,Centre,"sb/star.png",220,240
 R,0,0,3000,0,6.283
Sprite,Foreground,Centre,"sb/star.png",420,240
 R,0,0,3000,0,-6.283
```

## Swinging around the origin

Objects rotate around their **origin**. Pick the origin as the hinge. A bar with `BottomCentre` swings
like a metronome arm, hinged at its base:

```osb
Sprite,Foreground,BottomCentre,"sb/bar.png",320,330
 R,0,0,1000,-0.5,0.5
 R,0,1000,2000,0.5,-0.5
 R,0,2000,3000,-0.5,0.5
```

::: tip
osu! doesn't smooth the edges of rotated images, so their outlines can look jagged. The ranking
criteria suggest leaving a **one-pixel transparent border** around any image you plan to rotate, which
lets the edges blend smoothly.
:::

## Further reading

- [Rotate command](https://osu.ppy.sh/wiki/en/Storyboard/Scripting/Commands) on the osu! wiki
