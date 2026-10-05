---
title: Colour and parameters
type: lesson
minutes: 7
summary: Tinting with C, and the three parameters. Flip horizontally, flip vertically, and additive blending for glows.
---

## Colour

Colour (`C`) tints an object. Each value is a red, green and blue amount from 0 to 255:

```
 C,easing,start,end,startR,startG,startB,endR,endG,endB
```

The default is `255,255,255`, which shows the image exactly as it is. Any other colour **multiplies**
the image's own colours. Each pixel keeps only that share of its red, green and blue. That has a few
consequences:

- A **white** image becomes exactly the colour you choose. That's why storyboarders draw so many
  elements in white, and why the playground library is all white.
- A **black** pixel stays black, whatever the colour.
- Tinting can only darken or recolour. It never makes an image brighter than it already is.

This circle fades smoothly through three colours and back:

```osb
Sprite,Foreground,Centre,"sb/circle.png",320,240
 C,0,0,1000,255,90,90,255,220,90
 C,0,1000,2000,255,220,90,90,200,255
 C,0,2000,3000,90,200,255,255,90,90
```

::: note
A colour fade goes in a straight line through RGB values, so fading between opposite colours (like red
to cyan) passes through a dull grey in the middle. Adding a stop on a colour in between avoids it.
:::

## Parameters

Parameter (`P`) commands switch a special effect on. They take a single letter instead of numbers:

| Parameter | Effect |
| :-- | :-- |
| `H` | Flip the image horizontally, like a mirror. That's not the same as rotating it by half a turn. |
| `V` | Flip the image vertically. |
| `A` | Additive blending: add the image's colour to whatever is behind it. |

Parameters behave differently from other commands. They only apply **while the command is running**,
and switch off again when it ends. To keep one on for the object's whole life, give it the same start
and end time, written with the end time left empty:

```
 P,0,1000,,A
```

## Additive blending

Normally an image covers what's behind it. With additive blending, its colours are **added** to the
picture behind it instead, so it can only make things brighter. Black parts of the image disappear
completely. That makes it perfect for light: glows, flares, sparkles and lasers.

These two coloured glows overlap. Where they meet, the light adds up and turns almost white:

```osb
Sprite,Background,Centre,"bg.jpg",320,240
 F,0,0,3000,1
Sprite,Foreground,Centre,"sb/glow.png",270,240
 P,0,0,,A
 S,0,0,3000,2.2
 C,0,0,3000,255,70,90
Sprite,Foreground,Centre,"sb/glow.png",370,240
 P,0,0,,A
 S,0,0,3000,2.2
 C,0,0,3000,70,140,255
```

## Further reading

- [Colour and Parameter commands](https://osu.ppy.sh/wiki/en/Storyboard/Scripting/Commands) on the osu! wiki
