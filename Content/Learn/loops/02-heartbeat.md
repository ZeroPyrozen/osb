---
title: "Exercise: heartbeat"
type: exercise
minutes: 5
summary: Turn one heartbeat into eight with a loop.
exercise:
  duration: 4500
  starter: |
    // Make the heart beat 8 times, starting at 0 ms. Each beat lasts
    // 500 ms: it grows from scale 1 to 1.25 in the first 100 ms, then
    // shrinks back to 1 over the next 400 ms. One beat is done; use a
    // loop for all eight.
    Sprite,Foreground,Centre,"sb/heart.png",320,240
     C,0,0,4000,255,100,140
     S,0,0,100,1,1.25
     S,0,100,500,1.25,1
  solution: |
    Sprite,Foreground,Centre,"sb/heart.png",320,240
     C,0,0,4000,255,100,140
     L,0,8
      S,0,0,100,1,1.25
      S,0,100,500,1.25,1
  checks:
    - kind: count
      of: loops
      min: 1
      label: It uses a loop
    - kind: value
      time: 100
      property: scale
      equals: 1.25
      label: The first beat peaks at 100 ms
    - kind: value
      time: 500
      property: scale
      equals: 1
      label: It's back to normal size at 500 ms
    - kind: value
      time: 3600
      property: scale
      equals: 1.25
      label: The eighth beat peaks at 3600 ms
    - kind: value
      time: 4000
      property: scale
      equals: 1
      label: It's back to normal size at 4000 ms
    - kind: visible
      time: 4100
      expect: false
      label: It stops after the eighth beat
  hints:
    - Put an L line where the beat starts, then indent the two Scale commands one level deeper (two spaces).
    - Inside the loop the times are relative to each pass, so the commands keep their times of 0, 100 and 500.
---

One heartbeat is already written. Wrap it in a **loop** so the heart beats **8 times** from 0 ms.
Each beat lasts 500 ms, so the eighth one finishes at 4000 ms.
