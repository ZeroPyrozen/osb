---
title: Points on a circle
type: lesson
minutes: 6
summary: Radians, Math.cos and Math.sin, and the one formula behind rings, spirals and clock faces.
---

Rings of particles, spirals, orbiting stars, clock faces: many storyboard effects put objects on a
circle. One formula does all of them, and this lesson builds it up.

## Angles in radians

You met radians with the Rotate command: a full turn is 2π, about 6.283, and a quarter turn is π ÷ 2.
In JavaScript, π is `Math.PI`, and degrees become radians when you multiply by π and divide by 180:

```js
log(Math.PI);
log(2 * Math.PI);
log(90 * Math.PI / 180);
```

The last one is a quarter turn, about 1.571.

## Cosine and sine

Picture a point going round a circle with a radius of 1. At any angle, `Math.cos(angle)` says how far
the point is to the right of the centre, and `Math.sin(angle)` how far it is below it. Both stay between
−1 and 1, and a negative value means left or up:

| Angle | `Math.cos` | `Math.sin` | The point is |
| :-- | :-- | :-- | :-- |
| 0 | 1 | 0 | on the right |
| π ÷ 2 | 0 | 1 | at the bottom |
| π | −1 | 0 | on the left |
| 3π ÷ 2 | 0 | −1 | at the top |

On a storyboard, y grows downwards, so as the angle grows the point goes round **clockwise**: a quarter
turn from the right lands at the bottom, not the top.

::: note
If you `log` some of these, you get tiny numbers such as `6.123233995736766e-17` instead of 0. That's
rounding inside the computer, and it's close enough: script mode rounds positions to four decimal places
anyway.
:::

## The formula

Make the circle as big as you want by multiplying by a radius, and move it into place by adding a
centre:

```
x = centreX + radius × cos(angle)
y = centreY + radius × sin(angle)
```

As a function that returns the point as `[x, y]`:

```js
function pointOnCircle(centreX, centreY, radius, angle) {
    return [centreX + radius * Math.cos(angle), centreY + radius * Math.sin(angle)];
}

log(pointOnCircle(320, 240, 100, 0));
log(pointOnCircle(320, 240, 100, Math.PI));
```

The first point is 100 pixels right of the centre, at (420, 240), and the second 100 pixels left of it,
at (220, 240).

## A ring

Put the formula in a loop and spread the angles evenly. With `count` objects, object `i` gets the angle
`i` × 2π ÷ `count`:

```js
const layer = sb.layer('Foreground');
const count = 12;
const radius = 150;

for (let i = 0; i < count; i++) {
    const angle = i * 2 * Math.PI / count;
    const dot = layer.sprite('sb/dot.png', 'Centre', 320 + radius * Math.cos(angle), 240 + radius * Math.sin(angle));
    dot.fade(i * 100, i * 100 + 200, 0, 1);
    dot.fade(2500, 3000, 1, 0);
}
```

Watch the order the dots appear in: the first on the right, then clockwise round the ring.

## Beyond rings

Let the radius grow with the angle and the ring becomes a spiral:

```js
const layer = sb.layer('Foreground');

for (let i = 0; i < 40; i++) {
    const angle = i * 0.4;
    const radius = 20 + i * 5;
    const spark = layer.sprite('sb/particle.png', 'Centre', 320 + radius * Math.cos(angle), 240 + radius * Math.sin(angle));
    spark.fade(i * 50, i * 50 + 100, 0, 1);
    spark.fade(2500, 3000, 1, 0);
}
```

Give x and y different radii and the ring becomes an ellipse.

## Further reading

- [Math.cos](https://developer.mozilla.org/en-US/docs/Web/JavaScript/Reference/Global_Objects/Math/cos)
  and [Math.sin](https://developer.mozilla.org/en-US/docs/Web/JavaScript/Reference/Global_Objects/Math/sin)
  on MDN
- [Commands](https://osu.ppy.sh/wiki/en/Storyboard/Scripting/Commands) on the osu! wiki, for the Rotate
  command and its radians
