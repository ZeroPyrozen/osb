---
title: "Exercise: fix the blink"
type: exercise
minutes: 5
summary: A loop that should blink but doesn't. Find out why and fix it.
exercise:
  duration: 4500
  starter: |
    // The star should blink 6 times starting at 1000 ms: each blink is
    // 250 ms on, then 250 ms off. Something's wrong with this loop.
    // Watch it closely, then fix it.
    Sprite,Foreground,Centre,"sb/star.png",320,240
     L,1000,6
      F,0,0,,1
      F,0,250,,0
  solution: |
    Sprite,Foreground,Centre,"sb/star.png",320,240
     L,1000,6
      F,0,0,250,1
      F,0,250,500,0
  checks:
    - kind: count
      of: loops
      min: 1
      label: It still uses a loop
    - kind: value
      time: 1100
      property: opacity
      equals: 1
      label: The star is on at 1100 ms
    - kind: value
      time: 1350
      property: opacity
      equals: 0
      label: It's off at 1350 ms
    - kind: value
      time: 1600
      property: opacity
      equals: 1
      label: It's on again at 1600 ms
    - kind: value
      time: 3600
      property: opacity
      equals: 1
      label: The sixth blink is on at 3600 ms
    - kind: value
      time: 3850
      property: opacity
      equals: 0
      label: and off at 3850 ms
    - kind: lifetime
      endsBy: 4000
      label: The loop is over by 4000 ms
  hints:
    - How long is one pass of this loop? Remember that it runs from the earliest start to the latest end of the commands inside.
    - Both commands are instant, so the pass lasts only 250 ms and the "off" part never gets any time. Give the commands durations that fill the whole 500 ms.
---

The star is supposed to **blink 6 times** from 1000 ms, each blink **250 ms on and 250 ms off**. Instead
it barely blinks at all, and it stops too early.

Step through the preview with the time slider to see what's happening, then fix the loop. The previous
lesson has everything you need.
