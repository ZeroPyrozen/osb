---
title: From playground to storybrew
type: lesson
minutes: 7
summary: Turn what you've written here into storybrew effects. The same ideas in C#, side by side.
---

When you're ready for real projects, **storybrew** is the natural next step. It's a free, open-source
storyboard editor where you write effects in C#. It plays them with the beatmap's music and exports a
finished `.osb`. Script mode was designed to feel like it, so most of what you know already transfers.

## The same effect, twice

Here's the row of dots from the exercise in script mode:

```js
const layer = sb.layer('Foreground');

for (let i = 0; i < 10; i++) {
    const dot = layer.sprite('sb/dot.png', 'Centre', 140 + i * 40, 240);
    const start = i * 100;
    dot.fade(start, start + 300, 0, 1);
    dot.fade(start + 300, 2000, 1, 1);
}
```

And as a storybrew effect:

```cs
using OpenTK;
using StorybrewCommon.Scripting;
using StorybrewCommon.Storyboarding;

namespace StorybrewScripts
{
    public class RowOfDots : StoryboardObjectGenerator
    {
        [Configurable] public int Count = 10;
        [Configurable] public int StartTime = 0;

        public override void Generate()
        {
            var layer = GetLayer("Dots");
            for (var i = 0; i < Count; i++)
            {
                var start = StartTime + i * 100;
                var dot = layer.CreateSprite("sb/dot.png", OsbOrigin.Centre, new Vector2(140 + i * 40, 240));
                dot.Fade(start, start + 300, 0, 1);
                dot.Fade(start + 300, 2000, 1, 1);
            }
        }
    }
}
```

The shape is the same: get a layer, create sprites in a loop, add commands. C# wraps it in a class with a
`Generate` method, and the `[Configurable]` fields appear as settings in storybrew's interface, so you
can change `Count` without touching the code.

## Translation table

| Script mode | storybrew |
| :-- | :-- |
| `sb.layer('Foreground')` | `GetLayer("name")`. storybrew layers have their own names, and you pick which osu! layer each one exports to in storybrew. |
| `layer.sprite(path, 'Centre', x, y)` | `layer.CreateSprite(path, OsbOrigin.Centre, new Vector2(x, y))` |
| `layer.animation(…)` | `layer.CreateAnimation(path, frameCount, frameDelay, OsbLoopType.LoopForever, origin, position)` |
| `s.fade('OutQuad', 0, 500, 0, 1)` | `s.Fade(OsbEasing.OutQuad, 0, 500, 0, 1)` |
| `s.move(0, 1000, [100, 240], [540, 240])` | `s.Move(0, 1000, new Vector2(100, 240), new Vector2(540, 240))` |
| `s.scaleVec(…)`, `s.rotate(…)` | `s.ScaleVec(…)`, `s.Rotate(…)` |
| `s.color(0, [255, 128, 0])` | `s.Color(0, 1, 0.5, 0)`. storybrew colours go from 0 to 1. |
| `s.additive(start, end)` | `s.Additive(start, end)` |
| `s.startLoopGroup(t, n)` … `s.endGroup()` | `s.StartLoopGroup(t, n)` … `s.EndGroup()` |
| `random(a, b)` | `Random(a, b)`, also seeded |
| `beatLength(bpm)` | `Beatmap.GetTimingPointAt(time).BeatDuration`, read straight from the beatmap |

Easing names are the same in both, except that linear is `OsbEasing.None` in storybrew. Script mode
accepts `'None'` too.

## What storybrew adds

storybrew can do a lot that a browser playground can't:

- **Read the beatmap:** hit objects, timing points and the background, so effects can follow the map.
- **Read the music:** `GetFft` gives the song's spectrum at any moment, which is how audio spectrums are
  made.
- **Know your images:** `GetMapsetBitmap(path)` gives an image's size, for exact scaling and placement.
- **Organise big projects:** many effects, each on its own layers, all previewed together with the
  music.

It also ships with example effects (Background, Particles, Spectrum, Lyrics and more) that are well
worth reading once you're comfortable with the basics.

## Further reading

- [storybrew on GitHub](https://github.com/Damnae/storybrew), with downloads and a wiki
