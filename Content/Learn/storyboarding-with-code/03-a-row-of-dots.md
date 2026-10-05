---
title: "Exercise: a row of dots"
type: exercise
minutes: 6
summary: Your first generated storyboard. Ten dots in a row, appearing one after another.
exercise:
  mode: script
  duration: 2000
  starter: |
    // Make 10 dots in a row: x from 140 to 500 (40 px apart), y = 240.
    // Dot i (0 to 9) fades in from i * 100 ms to i * 100 + 300 ms,
    // and every dot stays visible until 2000 ms.
    const layer = sb.layer('Foreground');

    const dot = layer.sprite('sb/dot.png', 'Centre', 140, 240);
    dot.fade(0, 300, 0, 1);
    dot.fade(300, 2000, 1, 1);
  solution: |
    const layer = sb.layer('Foreground');

    for (let i = 0; i < 10; i++) {
        const dot = layer.sprite('sb/dot.png', 'Centre', 140 + i * 40, 240);
        const start = i * 100;
        dot.fade(start, start + 300, 0, 1);
        dot.fade(start + 300, 2000, 1, 1);
    }
  checks:
    - kind: count
      of: sprites
      equals: 10
      label: There are 10 dots
    - kind: object
      index: 0
      x: 140
      y: 240
      label: The first dot is at (140, 240)
    - kind: object
      index: 9
      x: 500
      y: 240
      label: The last dot is at (500, 240)
    - kind: value
      object: { index: 0 }
      time: 300
      property: opacity
      equals: 1
      label: The first dot has faded in by 300 ms
    - kind: value
      object: { index: 9 }
      time: 900
      property: opacity
      equals: 0
      label: The last dot starts fading in at 900 ms
    - kind: value
      object: { index: 9 }
      time: 1200
      property: opacity
      equals: 1
      label: and has faded in by 1200 ms
    - kind: visible
      time: 2000
      every: true
      label: Every dot is still visible at 2000 ms
  hints:
    - "Wrap the sprite and its commands in a loop: for (let i = 0; i < 10; i++) { … }"
    - Inside the loop, work out each dot's x (140 + i * 40) and its start time (i * 100), and use them in the sprite and the fades.
---

Write a script that creates **10 dots in a row**:

- the dots sit at **y = 240**, from **x = 140** to **x = 500**, 40 pixels apart,
- dot number *i* (counting from 0) **fades in** from *i* × 100 ms to *i* × 100 + 300 ms,
- every dot stays **visible until 2000 ms**.

The starter makes the first dot. A loop can make the rest.
