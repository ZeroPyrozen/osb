---
title: "Exercise: pop and spin"
type: exercise
minutes: 6
summary: Combine Scale, Fade and Rotate into one lively entrance.
exercise:
  duration: 2000
  starter: |
    // 0 to 500 ms: the star grows from nothing to full size
    //              while it fades in.
    // 500 to 1500 ms: it spins once, clockwise.
    // It stays visible until 2000 ms.
    Sprite,Foreground,Centre,"sb/star.png",320,240
     F,0,0,2000,1
  solution: |
    Sprite,Foreground,Centre,"sb/star.png",320,240
     F,0,0,500,0,1
     S,0,0,500,0,1
     R,0,500,1500,0,6.283
     F,0,500,2000,1
  checks:
    - kind: value
      time: 0
      property: scale
      equals: 0
      label: At 0 ms the star has no size
    - kind: value
      time: 0
      property: opacity
      equals: 0
      label: At 0 ms it's invisible
    - kind: value
      time: 500
      property: scale
      equals: 1
      label: At 500 ms it's full size
    - kind: value
      time: 500
      property: opacity
      equals: 1
      label: At 500 ms it's fully visible
    - kind: value
      time: 500
      property: rotation
      equals: 0
      tolerance: 0.02
      label: It hasn't turned yet at 500 ms
    - kind: value
      time: 1500
      property: rotation
      equals: 6.283
      tolerance: 0.01
      label: By 1500 ms it has turned once, clockwise
    - kind: visible
      time: 2000
      label: It's still visible at 2000 ms
  hints:
    - Commands of different types can run at the same time. An S and an F can both run from 0 to 500.
    - One full clockwise turn is 2π radians, about 6.283.
    - The fade-in ends at 500 ms, so you need another Fade to keep the star alive until 2000 ms.
---

Give the star a proper entrance:

1. From **0 to 500 ms** it grows from **nothing to full size** while it **fades in**.
2. From **500 to 1500 ms** it **spins once, clockwise**.
3. It stays visible until **2000 ms**.
