---
title: Particles
type: lesson
minutes: 8
summary: What a particle system is, giving every particle its own randomness, and keeping hundreds of sprites cheap.
---

Snow, rain, dust, sparks, bubbles and starfields are all **particle effects**: many small sprites,
each following simple rules with a little randomness. No single particle is interesting on its own.
Together they bring a scene to life.

## The recipe

Every particle effect answers the same few questions for each particle:

1. **When** is it born, and how long does it live?
2. **Where** does it start, and where does it go?
3. **What** does it look like: size, rotation, colour, opacity?

Randomness answers each one slightly differently for every particle. That's the whole trick:

```js
const layer = sb.layer('Foreground');

for (let i = 0; i < 60; i++) {
    const start = random(0, 3000);       // when it's born
    const life = random(1500, 2500);     // how long it lives
    const x = random(-107, 747);         // where it starts
    const peak = random(0.3, 0.9);       // how bright it gets
    const dust = layer.sprite('sb/particle.png');
    dust.move(start, start + life, [x, 500], [x + random(-60, 60), 300]);
    dust.fade(start, start + 300, 0, peak);
    dust.fade(start + life - 300, start + life, peak, 0);
    dust.scale(start, random(0.5, 1.5));
}
```

Run it in the playground and the dust drifts up from the bottom of the screen. Change the numbers and
watch the mood change: more particles, slower motion, bigger sizes.

::: tip
Notice `peak`. Each particle's brightness is picked once and used in both fades. Calling `random()`
separately in each fade would give two different numbers, and the particle would jump from one
brightness to the other halfway through its life. Whenever you need a random value more than once,
store it in a variable first.
:::

## Fade in, fade out

Particles that **pop** into existence and vanish at once look cheap. Fading every particle in at birth
and out before death, even over just 200 or 300 ms, makes the whole effect feel soft and natural.

## Spread them out

If every particle starts at once, the effect arrives in a clump, then thins out. Spread the start times
across the whole effect, as above, so the screen fills gradually and stays evenly busy. For a continuous
effect that lasts a long time, give each particle a **loop**: it flies, then starts again from the
beginning. That's how storybrew's Particles effect works, and it means a few dozen sprites can fill
minutes of song.

## Glow

Additive blending suits most particles: sparks, stars, fireflies, light rain. Overlapping particles
brighten each other instead of stacking up as opaque blobs.

## Keep it cheap

Every particle is a sprite, and every sprite costs something to draw:

- **Use as few as you can** for the look you want. Fifty well-placed particles often look as good as
  five hundred.
- **Use small images.** A 16 × 16 dot scaled up a little is far cheaper than a 512 × 512 one scaled
  down a lot.
- **Don't let invisible particles linger.** A particle that has faded out should also be at the end of
  its life, not drifting around invisibly until the song ends.
