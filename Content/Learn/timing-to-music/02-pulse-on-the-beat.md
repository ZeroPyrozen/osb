---
title: "Exercise: pulse on the beat"
type: exercise
minutes: 5
summary: Make a ring pulse on the first eight beats of a 120 BPM song, with a metronome to check by ear.
exercise:
  duration: 4500
  bpm: 120
  offset: 0
  starter: |
    // 120 BPM, first beat at 0 ms. Make the ring pulse on each of the
    // first 8 beats: on every beat it snaps to scale 1.5, then shrinks
    // back to 1 by the next beat, slowing down as it shrinks.
    // Press play: the metronome clicks on every beat.
    Sprite,Foreground,Centre,"sb/ring.png",320,240
     S,0,0,500,1.5,1
  solution: |
    Sprite,Foreground,Centre,"sb/ring.png",320,240
     L,0,8
      S,7,0,500,1.5,1
  checks:
    - kind: count
      of: loops
      min: 1
      label: It uses a loop
    - kind: value
      time: 0
      property: scale
      equals: 1.5
      label: On the first beat (0 ms) it's at scale 1.5
    - kind: value
      time: 250
      property: scale
      max: 1.2
      label: It shrinks quickly at first, then slows down (an Out easing)
    - kind: value
      time: 1000
      property: scale
      equals: 1.5
      label: On the third beat (1000 ms) it's back at 1.5
    - kind: value
      time: 3500
      property: scale
      equals: 1.5
      label: On the eighth beat (3500 ms) it's back at 1.5
    - kind: visible
      time: 4100
      expect: false
      label: It stops after the eighth beat
  hints:
    - At 120 BPM a beat lasts 60000 ÷ 120 = 500 ms, so each pass of the loop should last 500 ms.
    - "Slowing down at the end is an Out easing: try 1, 7 or 13 on the Scale command."
---

The song is **120 BPM** and its first beat is at **0 ms**. Make the ring **pulse on each of the first
8 beats**. On every beat it snaps to scale **1.5**, then shrinks back to **1** by the next beat, slowing
down as it shrinks.

Press play to hear the metronome: every click should line up with a pulse.
