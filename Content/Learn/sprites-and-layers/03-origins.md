---
title: Origins
type: lesson
minutes: 6
summary: The anchor point that sits at (x, y), and why it also matters for scaling and rotating.
---

When you put a sprite at (320, 240), which part of the image goes there? Its centre? Its corner? That's
what the **origin** decides.

| Origin | The point of the image placed at (x, y) |
| :-- | :-- |
| `TopLeft` | top-left corner |
| `TopCentre` | middle of the top edge |
| `TopRight` | top-right corner |
| `CentreLeft` | middle of the left edge |
| `Centre` | the very centre |
| `CentreRight` | middle of the right edge |
| `BottomLeft` | bottom-left corner |
| `BottomCentre` | middle of the bottom edge |
| `BottomRight` | bottom-right corner |

Origins can also be written as numbers (0 is TopLeft, 1 is Centre, and so on), but the names are much
easier to read. There's a tenth origin, `Custom`, which behaves like TopLeft; the wiki recommends not
using it.

::: note
Origin names are case-sensitive and use the British spelling: `Centre`, not `center`.
:::

## Same position, different origins

These three squares all sit at (320, 240). Only their origin differs, so each one ends up in a
different place. They're tinted so you can tell them apart: red is `TopLeft`, green is `Centre`, blue is
`BottomRight`. Turn on **Guides** in the preview to see where (320, 240) is.

```osb
Sprite,Foreground,TopLeft,"sb/square.png",320,240
 F,0,0,4000,0.8
 C,0,0,,255,80,80
Sprite,Foreground,Centre,"sb/square.png",320,240
 F,0,0,4000,0.8
 C,0,0,,80,220,120
Sprite,Foreground,BottomRight,"sb/square.png",320,240
 F,0,0,4000,0.8
 C,0,0,,90,140,255
```

## The origin is also the pivot

Origins matter for more than placement. When a sprite **scales** or **rotates**, it does so around its
origin. A star rotating around its `Centre` spins in place; the same star with `TopLeft` swings around
its corner like a door:

```osb
Sprite,Foreground,Centre,"sb/star.png",200,240
 F,0,0,4000,1
 R,0,0,4000,0,6.283
Sprite,Foreground,TopLeft,"sb/star.png",440,240
 F,0,0,4000,1
 R,0,0,4000,0,6.283
```

## Picking an origin

- **Centre** for most things: it makes positions and spins intuitive.
- **TopLeft** for backgrounds, placed at (0, 0), or at (−107, 0) to cover widescreen.
- **BottomCentre** for things that stand on a floor or grow upwards, like spectrum bars.
- **CentreLeft** for text or bars that should grow to the right from a fixed starting point.

## Further reading

- [Storyboard objects](https://osu.ppy.sh/wiki/en/Storyboard/Scripting/Objects) on the osu! wiki
