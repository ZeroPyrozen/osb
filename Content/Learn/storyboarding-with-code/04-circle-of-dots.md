---
title: "Exercise: burst into a circle"
type: exercise
minutes: 8
summary: Use sine and cosine to send 16 dots from the centre out to a ring.
exercise:
  mode: script
  duration: 2000
  starter: |
    // Arrange 16 dots in a circle of radius 150 around (320, 240).
    // Dot i sits at the angle i * 2π / 16. Math.cos and Math.sin take
    // radians, and Math.PI is π.
    // Each dot starts at i * 50 ms. It moves out from the centre to
    // its place with the "OutBack" easing over 500 ms, and fades in
    // over the first 200 ms. All dots stay visible until 2000 ms.
    const layer = sb.layer('Foreground');
    const count = 16;
    const radius = 150;

    for (let i = 0; i < count; i++) {
        const start = i * 50;
        const dot = layer.sprite('sb/dot.png', 'Centre', 320, 240);
        dot.fade(start, start + 200, 0, 1);
        dot.fade(start + 200, 2000, 1, 1);
        // Work out this dot's place on the circle and move it there.
    }
  solution: |
    const layer = sb.layer('Foreground');
    const count = 16;
    const radius = 150;

    for (let i = 0; i < count; i++) {
        const angle = i * 2 * Math.PI / count;
        const x = 320 + radius * Math.cos(angle);
        const y = 240 + radius * Math.sin(angle);
        const start = i * 50;
        const dot = layer.sprite('sb/dot.png', 'Centre', 320, 240);
        dot.move('OutBack', start, start + 500, [320, 240], [x, y]);
        dot.fade(start, start + 200, 0, 1);
        dot.fade(start + 200, 2000, 1, 1);
    }
  checks:
    - kind: count
      of: sprites
      equals: 16
      label: There are 16 dots
    - kind: value
      object: { index: 0 }
      time: 0
      property: position
      equals: [320, 240]
      tolerance: 1
      label: Each dot starts in the centre
    - kind: value
      object: { index: 0 }
      time: 2000
      property: position
      equals: [470, 240]
      tolerance: 1
      label: Dot 0 ends at angle 0, on the right (470, 240)
    - kind: value
      object: { index: 4 }
      time: 2000
      property: position
      equals: [320, 390]
      tolerance: 1
      label: Dot 4 ends a quarter turn round, at the bottom (320, 390)
    - kind: value
      object: { index: 8 }
      time: 2000
      property: position
      equals: [170, 240]
      tolerance: 1
      label: Dot 8 ends halfway round, on the left (170, 240)
    - kind: command
      object: { index: 15 }
      type: M
      easing: 30
      start: 750
      end: 1250
      label: The last dot moves out with Back Out from 750 to 1250 ms
    - kind: visible
      time: 2000
      every: true
      label: Every dot is visible at 2000 ms
  hints:
    - "A point on a circle is (centreX + radius × cos(angle), centreY + radius × sin(angle))."
    - "The angle for dot i is i * 2 * Math.PI / count. Then move from [320, 240] to [x, y]: dot.move('OutBack', start, start + 500, [320, 240], [x, y])."
---

Time for some maths. Send **16 dots** bursting out from the centre into a **ring**:

- the ring has a radius of **150** around **(320, 240)**,
- dot *i* sits at the angle *i* × 2π ÷ 16, so the dots are evenly spaced,
- dot *i* starts at *i* × 50 ms, **moving out** from the centre to its place with **OutBack** over 500 ms
  and **fading in** over the first 200 ms,
- every dot stays visible until **2000 ms**.

Angles start on the right and, because y grows downwards, go round **clockwise**: a quarter turn puts a
dot at the bottom of the ring.
