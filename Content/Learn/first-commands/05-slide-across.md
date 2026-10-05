---
title: "Exercise: slide across"
type: exercise
minutes: 5
summary: Send a music note across the screen from one edge to the other.
exercise:
  duration: 3000
  widescreen: false
  guides: true
  starter: |
    // Slide the note across the screen from 0 to 3000 ms.
    // It should start completely off the left edge, end completely
    // off the right edge, and travel along the middle (y = 240).
    // sb/note.png is 48 × 64 pixels, and this preview is 4:3.
    Sprite,Foreground,Centre,"sb/note.png",320,240
     F,0,0,3000,1
  solution: |
    Sprite,Foreground,Centre,"sb/note.png",320,240
     M,0,0,3000,-50,240,690,240
  checks:
    - kind: value
      time: 0
      property: x
      max: -24
      label: At 0 ms the note is completely off the left edge
    - kind: value
      time: 1500
      property: x
      min: 0
      max: 640
      label: Halfway through, it's on screen
    - kind: value
      time: 3000
      property: x
      min: 664
      label: At 3000 ms it's completely off the right edge
    - kind: value
      time: 1500
      property: y
      equals: 240
      tolerance: 1
      label: It travels along y = 240
  hints:
    - The note is 48 pixels wide and centred on its position, so its centre must be more than 24 pixels past an edge to be fully off-screen.
    - "One Move command does it: M,0,0,3000, then the start x and y, then the end x and y."
---

Make the note **slide across the screen** between 0 and 3000 ms:

- at **0 ms** it's completely off the **left** edge,
- at **3000 ms** it's completely off the **right** edge,
- and it travels along the middle of the screen, at **y = 240**.

The preview is 4:3 for this exercise, so the edges are x = 0 and x = 640. Hover over the preview to
read coordinates.
