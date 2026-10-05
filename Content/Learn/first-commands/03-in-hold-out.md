---
title: "Exercise: in, hold, out"
type: exercise
minutes: 4
summary: The most common shape in storyboarding. Fade in, stay, fade out, and be gone on time.
exercise:
  duration: 3000
  starter: |
    // Fade the logo in from 0 to 500 ms, keep it fully visible,
    // then fade it out from 2500 to 3000 ms.
    Sprite,Foreground,Centre,"sb/osb.png",320,240
     F,0,0,500,0,1
  solution: |
    Sprite,Foreground,Centre,"sb/osb.png",320,240
     F,0,0,500,0,1
     F,0,2500,3000,1,0
  checks:
    - kind: value
      time: 0
      property: opacity
      equals: 0
      label: It starts invisible at 0 ms
    - kind: value
      time: 500
      property: opacity
      equals: 1
      label: It's fully visible at 500 ms
    - kind: value
      time: 1500
      property: opacity
      equals: 1
      label: It's still fully visible at 1500 ms
    - kind: value
      time: 2500
      property: opacity
      equals: 1
      label: The fade-out hasn't started before 2500 ms
    - kind: value
      time: 3000
      property: opacity
      equals: 0
      label: It's gone at 3000 ms
    - kind: lifetime
      endsBy: 3000
      label: Its last command ends by 3000 ms
  hints:
    - Remember the second rule about time. After a command ends, its end value stays. You don't need a command for the part in the middle.
    - The fade-out is a second Fade from 2500 to 3000 that goes from 1 to 0.
---

The logo fades in, but then vanishes at 500 ms, because that's when its only command ends and the
object stops existing.

Finish it so the logo:

1. fades in from **0 to 500 ms** (already done),
2. stays **fully visible** in the middle,
3. fades out from **2500 to 3000 ms**.

Fade in, hold, fade out. You'll write this pattern for nearly every sprite you ever make.
