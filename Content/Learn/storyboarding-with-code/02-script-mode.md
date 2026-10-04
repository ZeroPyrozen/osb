---
title: Script mode
type: lesson
minutes: 9
summary: The script-mode API. Layers, sprites, commands, loops and helpers, and the .osb they produce.
---

Script mode lets you write JavaScript that builds a storyboard. You create sprites and give them
commands by calling functions, and the playground turns the result into `.osb` and plays it.

## A first script

```js
const layer = sb.layer('Foreground');
const dot = layer.sprite('sb/dot.png', 'Centre', 320, 240);
dot.fade(0, 500, 0, 1);
dot.move('OutQuad', 0, 1000, [100, 240], [540, 240]);
dot.scale(1000, 2);
```

It produces this storyboard. Each call becomes one line:

```osb
Sprite,Foreground,Centre,"sb/dot.png",320,240
 F,0,0,500,0,1
 M,4,0,1000,100,240,540,240
 S,0,1000,,2
```

Press **Run in playground** above the script to try it. The generated `.osb` is shown under the editor
there.

## Layers and objects

| Call | Writes |
| :-- | :-- |
| `sb.layer(name)` | Picks a layer: `'Background'`, `'Fail'`, `'Pass'`, `'Foreground'` or `'Overlay'`. |
| `layer.sprite(path, origin, x, y)` | A Sprite line. The origin defaults to `'Centre'` and the position to (320, 240). |
| `layer.animation(path, frameCount, frameDelay, loopType, origin, x, y)` | An Animation line. |

## Commands

Every command has two forms:

- `(start, end, from, to)` changes a value over time.
- `(time, value)` is an instant command, written with the empty end time shorthand.

To ease a command, put the easing's **name** first: `'Out'`, `'InQuad'`, `'OutBack'`,
`'InOutSine'` and so on, the names from the easing table without spaces.

| Call | Command |
| :-- | :-- |
| `fade(…)` | F, with an opacity |
| `move(…)` | M, with `[x, y]` pairs |
| `moveX(…)`, `moveY(…)` | MX and MY |
| `scale(…)` | S |
| `scaleVec(…)` | V, with `[x, y]` pairs |
| `rotate(…)` | R, in radians |
| `color(…)` | C, with `[r, g, b]` values from 0 to 255 |
| `flipH(start, end)`, `flipV(…)`, `additive(…)` | P with H, V or A. Leave out the end for the whole object's life. |
| `startLoopGroup(start, count)` … `endGroup()` | A loop around the commands in between |
| `startTriggerGroup(name, start, end, group)` … `endGroup()` | A trigger, the same way |

## Loops in code

A JavaScript `for` loop creates many **objects**. A loop group repeats **commands** on one object.
Together they cover most effects. Here a row of stars pops in one by one, each with its own colour:

```js
const layer = sb.layer('Foreground');

for (let i = 0; i < 8; i++) {
    const star = layer.sprite('sb/star.png', 'Centre', 100 + i * 63, 240);
    const start = i * 150;
    star.scale('OutBack', start, start + 400, 0, 0.8);
    star.color(start, [255, 120 + i * 15, 255 - i * 25]);
    star.fade(2500, 3000, 1, 0);
}
```

And here a loop group makes a heart beat eight times:

```js
const layer = sb.layer('Foreground');
const heart = layer.sprite('sb/heart.png');

heart.color(0, [255, 110, 150]);
heart.startLoopGroup(0, 8);
heart.scale('OutQuad', 0, 500, 1.4, 1);
heart.endGroup();
```

## Helpers

| Helper | Gives you |
| :-- | :-- |
| `random()` | a random number from 0 to 1. `random(max)` and `random(min, max)` give other ranges. |
| `randomInt(min, max)` | a random whole number from min to max, inclusive |
| `lerp(a, b, t)` | the value a fraction `t` of the way from `a` to `b` |
| `beatLength(bpm)` | milliseconds per beat |
| `log(…)` | prints values under the editor, for debugging |

The random numbers are **seeded**: a script draws the same "random" storyboard every time it runs, so
your effect doesn't change each time you edit something else.

::: note
Times are rounded to whole milliseconds and values to four decimal places. To stop runaway loops,
scripts can create at most 10,000 sprites and 300,000 commands, and are stopped after 3 seconds.
:::
