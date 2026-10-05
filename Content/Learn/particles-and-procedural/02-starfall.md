---
title: "Exercise: starfall"
type: exercise
minutes: 8
summary: A shower of glowing, spinning shooting stars, made with randomness.
exercise:
  mode: script
  duration: 5000
  starter: |
    // Make it rain stars! Create at least 40 shooting stars (sb/star.png).
    // - Each one starts at a random time in the first 2500 ms, at a
    //   random x, just above the screen (y = -40).
    // - It falls diagonally to just below the screen (y = 520), over a
    //   random time between 1500 and 2500 ms.
    // - It spins while it falls, and glows (additive blending).
    // - Every star is gone by 5000 ms.
    const layer = sb.layer('Foreground');

    const star = layer.sprite('sb/star.png');
    star.move(0, 2000, [320, -40], [100, 520]);
    star.scale(0, 0.4);
  solution: |
    const layer = sb.layer('Foreground');

    for (let i = 0; i < 50; i++) {
        const start = random(0, 2500);
        const duration = random(1500, 2500);
        const x = random(-107, 900);
        const star = layer.sprite('sb/star.png');
        star.move('InQuad', start, start + duration, [x, -40], [x - 250, 520]);
        star.rotate(start, start + duration, 0, random(-6, 6));
        star.scale(start, random(0.2, 0.5));
        star.color(start, [255, randomInt(180, 255), 120]);
        star.additive(start, start + duration);
    }
  checks:
    - kind: count
      of: sprites
      min: 40
      label: There are at least 40 stars
    - kind: value
      time: start
      property: y
      max: -20
      every: true
      label: Every star starts above the screen
    - kind: value
      time: end
      property: y
      min: 500
      every: true
      label: Every star ends below the screen
    - kind: count
      of: commands
      command: R
      min: 40
      label: The stars spin
    - kind: count
      of: commands
      command: P
      min: 40
      label: The stars glow (additive blending)
    - kind: lifetime
      endsBy: 5000
      every: true
      label: Every star is gone by 5000 ms
  hints:
    - Put everything in a loop that runs at least 40 times, and pick the start time, duration and x with random(min, max) inside it.
    - Each star needs a move from [x, -40] to somewhere lower left at y = 520, a rotate over the same time, and additive(start, start + duration).
    - The latest a star can finish is 2500 + 2500 = 5000 ms, so keep the start below 2500 and the duration below 2500.
---

Make a **starfall**: a shower of shooting stars streaking down the screen.

- At least **40 stars**, using `sb/star.png`.
- Each one starts at a **random time** in the first 2500 ms and a **random x**, just **above the screen**
  (y = −40).
- It falls **diagonally** to just **below the screen** (y = 520) over a random time from 1500 to
  2500 ms.
- Every star **spins** as it falls, and **glows** with additive blending.
- Every star is **gone by 5000 ms**.

Sizes and colours are up to you. Small, slightly golden stars look lovely.
