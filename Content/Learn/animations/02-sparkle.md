---
title: "Exercise: sparkle"
type: exercise
minutes: 4
summary: Turn a still sprite into a looping six-frame sparkle.
exercise:
  duration: 2000
  starter: |
    // Turn this sprite into an animation. sb/spark.png has 6 frames
    // (sb/spark0.png to sb/spark5.png). Show each frame for 80 ms,
    // loop forever, and keep it on screen from 0 to 2000 ms.
    Sprite,Foreground,Centre,"sb/spark0.png",320,240
     S,0,0,2000,2
  solution: |
    Animation,Foreground,Centre,"sb/spark.png",320,240,6,80,LoopForever
     S,0,0,2000,2
  checks:
    - kind: count
      of: animations
      equals: 1
      label: It's an Animation, not a Sprite
    - kind: object
      is: animation
      path: sb/spark.png
      frameCount: 6
      label: It plays the 6 frames of sb/spark.png
      message: Write the file name without a frame number, and set the frame count to 6.
    - kind: object
      is: animation
      frameDelay: 80
      label: Each frame shows for 80 ms
    - kind: object
      is: animation
      loopType: LoopForever
      label: It loops forever
    - kind: visible
      time: 0
      label: It's visible at 0 ms
    - kind: visible
      time: 2000
      label: It's still visible at 2000 ms
  hints:
    - The line starts with Animation instead of Sprite, and gets three extra parts at the end. Those are the frame count, the frame delay and the loop type.
    - The file name has no frame number in it. osu! adds 0, 1, 2… itself.
---

The sparkle is just a still image right now: it shows only its first frame. Turn it into an
**Animation** that:

- plays all **6 frames** of `sb/spark.png`,
- shows each frame for **80 ms**,
- **loops forever**,
- and stays on screen from **0 to 2000 ms** (the Scale command already does that).
