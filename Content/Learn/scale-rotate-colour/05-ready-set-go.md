---
title: "Exercise: ready, set, go"
type: exercise
minutes: 5
summary: One circle, three colours. Make a starting light with the Colour command.
exercise:
  duration: 3000
  starter: |
    // Ready, set, go! Make the light red from 0 to 1000 ms,
    // yellow from 1000 to 2000 ms, and green from 2000 to 3000 ms.
    Sprite,Foreground,Centre,"sb/circle.png",320,240
     F,0,0,3000,1
  solution: |
    Sprite,Foreground,Centre,"sb/circle.png",320,240
     F,0,0,3000,1
     C,0,0,1000,255,0,0
     C,0,1000,2000,255,255,0
     C,0,2000,3000,0,255,0
  checks:
    - kind: count
      of: sprites
      equals: 1
      label: One circle does all three colours
    - kind: value
      time: 500
      property: color
      equals: [255, 0, 0]
      tolerance: 1
      label: Red at 500 ms
    - kind: value
      time: 1500
      property: color
      equals: [255, 255, 0]
      tolerance: 1
      label: Yellow at 1500 ms
    - kind: value
      time: 2500
      property: color
      equals: [0, 255, 0]
      tolerance: 1
      label: Green at 2500 ms
    - kind: visible
      time: 3000
      label: It's still visible at 3000 ms
  hints:
    - Red is 255,0,0 and green is 0,255,0. Yellow is red and green together, 255,255,0.
    - Use three Colour commands back to back, one for each second. A colour that doesn't change only needs its three values once.
---

Turn the circle into a starting light:

- **red** from 0 to 1000 ms,
- **yellow** from 1000 to 2000 ms,
- **green** from 2000 to 3000 ms.

Each colour should switch on at once, with no fade between them. The white `sb/circle.png` takes on the
exact colour you give it.
