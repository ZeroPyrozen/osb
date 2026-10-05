---
title: "Exercise: wave of bars"
type: exercise
minutes: 8
summary: Sixteen bars pulsing one after another, built from one loop each.
exercise:
  mode: script
  duration: 4000
  starter: |
    // A wave of 16 bars. Bar i stands at x = 80 + 32 * i, y = 400, on
    // its bottom edge (BottomCentre). Bars are 20 pixels wide.
    // Starting at i * 100 ms, each bar pulses 4 times: it grows from 20
    // to 200 pixels tall in 400 ms, then shrinks back to 20 in 400 ms,
    // both with InOutSine.
    // sb/pixel.png is a single white pixel, so scaleVec sets its size.
    const layer = sb.layer('Foreground');

    const bar = layer.sprite('sb/pixel.png', 'BottomCentre', 80, 400);
    bar.scaleVec(0, 400, [20, 20], [20, 200]);
    bar.scaleVec(400, 800, [20, 200], [20, 20]);
  solution: |
    const layer = sb.layer('Foreground');

    for (let i = 0; i < 16; i++) {
        const bar = layer.sprite('sb/pixel.png', 'BottomCentre', 80 + 32 * i, 400);
        bar.startLoopGroup(i * 100, 4);
        bar.scaleVec('InOutSine', 0, 400, [20, 20], [20, 200]);
        bar.scaleVec('InOutSine', 400, 800, [20, 200], [20, 20]);
        bar.endGroup();
    }
  checks:
    - kind: count
      of: sprites
      equals: 16
      label: There are 16 bars
    - kind: object
      index: 0
      origin: BottomCentre
      x: 80
      y: 400
      label: The first bar stands at (80, 400) on its bottom edge
    - kind: object
      index: 15
      x: 560
      y: 400
      label: The last bar stands at (560, 400)
    - kind: count
      of: loops
      min: 16
      label: Every bar uses a loop
    - kind: value
      object: { index: 0 }
      time: 400
      property: scaleY
      equals: 200
      tolerance: 0.5
      label: Bar 0 reaches full height at 400 ms
    - kind: value
      object: { index: 0 }
      time: 800
      property: scaleY
      equals: 20
      tolerance: 0.5
      label: and is back down at 800 ms
    - kind: value
      object: { index: 15 }
      time: 1900
      property: scaleY
      equals: 200
      tolerance: 0.5
      label: Bar 15 peaks 1500 ms later, at 1900 ms
    - kind: value
      object: { index: 0 }
      time: 2800
      property: scaleY
      equals: 200
      tolerance: 0.5
      label: Bar 0 keeps pulsing. Its fourth peak is at 2800 ms
    - kind: visible
      object: { index: 0 }
      time: 3300
      expect: false
      label: After four pulses it's finished
    - kind: value
      time: start
      property: scaleX
      equals: 20
      every: true
      label: Every bar is 20 pixels wide
  hints:
    - Make the bars in a for loop, with x = 80 + 32 * i.
    - Wrap the two scaleVec commands in bar.startLoopGroup(i * 100, 4) … bar.endGroup(). Inside the loop, the times stay 0 to 400 and 400 to 800.
    - Put the easing name first in each scaleVec, like bar.scaleVec('InOutSine', 0, 400, [20, 20], [20, 200]).
---

Build a **wave of 16 bars**:

- bar *i* stands at **x = 80 + 32 × *i***, **y = 400**, on its bottom edge (`BottomCentre`), and is
  **20 pixels wide**,
- starting at ***i* × 100 ms**, each bar **pulses 4 times**: it grows from 20 to **200 pixels tall** in
  400 ms, then shrinks back to 20 in 400 ms, both with **InOutSine**.

Use a **loop group** for each bar's pulses rather than writing out every pulse. The ranking criteria
recommend loops for anything that repeats, and your file will be a fraction of the size.
