---
title: The easing families
type: lesson
minutes: 7
summary: All 35 easings in one table, what makes each family different, and which to reach for.
---

osu! has 35 easings. That sounds like a lot, but apart from linear and the original pair they're
ten **families**, each with an In, an Out and usually an In/Out version.

| Family | In | Out | In/Out | Character |
| :-- | :-: | :-: | :-: | :-- |
| Linear | | 0 | | Constant speed. |
| Original | 2 | 1 | | The first easings osu! had. They match Quad. |
| Quad | 3 | 4 | 5 | Gentle. |
| Cubic | 6 | 7 | 8 | Noticeable. |
| Quart | 9 | 10 | 11 | Strong. |
| Quint | 12 | 13 | 14 | Very strong. |
| Sine | 15 | 16 | 17 | The softest curve of all. |
| Expo | 18 | 19 | 20 | Extreme, close to a sudden jump. |
| Circ | 21 | 22 | 23 | Sharp at one end. |
| Elastic | 24 | 25, 26, 27 | 28 | Wobbles like a spring. 26 and 27 wobble less (half and quarter). |
| Back | 29 | 30 | 31 | Overshoots, then settles back. |
| Bounce | 32 | 33 | 34 | Bounces like a dropped ball. |

Notice the pattern: within a family, the In number comes first, then Out, then In/Out. (The original
pair is the exception: 1 is Out and 2 is In.)

## Stronger and softer

Sine, Quad, Cubic, Quart, Quint and Expo are all the same idea at increasing strength. The stronger the
easing, the longer it lingers at the slow end and the more abruptly it moves at the fast end. Sine and
Quad suit subtle motion like a floating background. Quint and Expo make punchy, dramatic moves.

## Overshooting families

**Back**, **Elastic** and **Bounce** are the fun ones. Back and Elastic travel *past* the end value and
come back (or, for In versions, pull back before starting), while Bounce hits the end value and rebounds
a few times. They're great for scale and movement:

```osb
Sprite,Foreground,Centre,"sb/heart.png",320,240
 S,30,0,600,0,1.5
 F,0,0,2500,1
 C,0,0,2500,255,110,150
```

::: warning
Be careful with overshooting easings on **Fade**. Opacity can't go above 1, so a fade that overshoots
can flicker or misbehave. Use them on movement, scale and rotation instead.
:::

Try the families yourself:

<div data-easing-explorer data-easing="30"></div>

## Which one should I use?

Some rules of thumb:

- **Arriving or appearing:** an Out easing. Cubic Out (7) or Quint Out (13) are safe choices.
- **Leaving or disappearing:** an In easing.
- **Moving between two places on screen:** an In/Out easing, such as Sine In/Out (17).
- **Popping in:** Back Out (30) on Scale.
- **Dropping and landing:** Bounce Out (33).
- **Springy, cartoonish motion:** Elastic Out (25), or 26 and 27 for less wobble.

## Further reading

- [The easing table](https://osu.ppy.sh/wiki/en/Storyboard/Scripting/Commands) on the osu! wiki
