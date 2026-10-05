---
title: Waves and bars
type: lesson
minutes: 7
summary: Drive motion with formulas. Sine waves, phase offsets, and spectrum-style bars.
---

Particles use randomness. **Procedural** effects use formulas instead: a rule that works out every
sprite's position or size from its number and the time. Change the formula and the whole effect
changes with it.

## The sine wave

`Math.sin` is the storyboarder's favourite function. As its input grows, it swings smoothly between −1
and 1 and back, forever. Multiply it to set how far the motion goes (the **amplitude**), and feed it
different inputs to change how fast the swings come (the **frequency**).

The magic ingredient is the **phase**: give every sprite a slightly different starting point in the
wave, and a row of sprites turns into a travelling wave:

```js
const layer = sb.layer('Foreground');
const count = 20;

for (let i = 0; i < count; i++) {
    const x = 50 + i * 28;
    const dot = layer.sprite('sb/dot.png', 'Centre', x, 240);
    dot.startLoopGroup(i * 60, 4);
    dot.moveY('InOutSine', 0, 400, 200, 280);
    dot.moveY('InOutSine', 400, 800, 280, 200);
    dot.endGroup();
}
```

Each dot bobs up and down with an In/Out Sine easing, which follows the shape of a sine wave. Starting
each dot's loop 60 ms after the previous one does the rest.

## Bars

Bars that grow and shrink are a classic, from spectrum visualisers to equalisers. The setup is always
the same: a stretched pixel with the `BottomCentre` origin, so it grows upwards from a fixed floor, and a
Vector scale command for its size:

```js
const layer = sb.layer('Foreground');

for (let i = 0; i < 24; i++) {
    const bar = layer.sprite('sb/pixel.png', 'BottomCentre', 90 + i * 20, 400);
    const height = 60 + 140 * Math.abs(Math.sin(i * 0.5));
    bar.scaleVec('OutQuad', 0, 600, [12, 0], [12, height]);
    bar.scaleVec('InQuad', 2400, 3000, [12, height], [12, 0]);
    bar.color(0, [97, 188, 166]);
}
```

::: note
A real audio spectrum reads the song itself: how loud each frequency is at each moment. That needs the
audio file, which storybrew provides through its `GetFft` function. With formulas like these you can
fake one convincingly, or animate bars to the beat instead.
:::

## Formulas you'll use again and again

- **Circle:** `x = cx + r × cos(a)`, `y = cy + r × sin(a)`.
- **Wave:** `y = base + amplitude × sin(phase + i × step)`.
- **Spiral:** a circle whose radius grows with each sprite.
- **Grid:** `x = left + column × spacing`, `y = top + row × spacing`.
- **Delay:** `start = i × gap`, so things happen one after another.

Mix them. A grid of dots whose delay depends on the distance from the centre makes a ripple. A circle
whose angle grows over time makes an orbit.
