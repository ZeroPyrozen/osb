---
title: "Exercise: centre stage"
type: exercise
minutes: 4
summary: Put a circle in the middle of the screen and keep it there for three seconds.
exercise:
  duration: 3000
  guides: true
  starter: |
    // Put sb/circle.png in the middle of the screen,
    // and make it visible from 0 to 3000 ms.
    Sprite,Foreground,Centre,"sb/circle.png",0,0
  solution: |
    Sprite,Foreground,Centre,"sb/circle.png",320,240
     F,0,0,3000,1
  checks:
    - kind: count
      of: sprites
      equals: 1
      label: There's exactly one sprite
    - kind: object
      path: sb/circle.png
      origin: Centre
      x: 320
      y: 240
      label: The circle's centre is at (320, 240)
    - kind: visible
      time: 0
      label: It's visible at 0 ms
    - kind: visible
      time: 3000
      label: It's still visible at 3000 ms
  hints:
    - The centre of the 640 × 480 canvas is (320, 240).
    - A sprite needs at least one command to appear. Try adding " F,0,0,3000,1" on the next line, starting with a space.
---

The starter script declares a circle, but it's in the wrong place and it has no commands, so it never
shows up.

1. Move the circle to the **centre** of the screen.
2. Make it **visible from 0 to 3000 ms** with a Fade command that keeps it at full opacity.

The preview updates as you type, and **Guides** is switched on so you can see the play area. When you
think you've got it, press **Check my work**.
