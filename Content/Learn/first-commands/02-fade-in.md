---
title: "Exercise: fade in"
type: exercise
minutes: 4
summary: Fade a star in between 1 and 2 seconds, then keep it on screen.
exercise:
  duration: 3000
  starter: |
    // Fade the star in: invisible at 1000 ms,
    // fully visible at 2000 ms, and still there at 3000 ms.
    Sprite,Foreground,Centre,"sb/star.png",320,240
     F,0,1000,3000,1
  solution: |
    Sprite,Foreground,Centre,"sb/star.png",320,240
     F,0,1000,2000,0,1
     F,0,2000,3000,1
  checks:
    - kind: value
      time: 1000
      property: opacity
      equals: 0
      label: At 1000 ms the star is invisible
    - kind: value
      time: 1500
      property: opacity
      min: 0.05
      max: 0.95
      label: At 1500 ms it's partly faded in
    - kind: value
      time: 2000
      property: opacity
      equals: 1
      label: At 2000 ms it's fully visible
    - kind: visible
      time: 3000
      label: It's still visible at 3000 ms
  hints:
    - A fade from invisible to visible goes from 0 to 1, so the values are "0,1".
    - One command can't fade in and then hold. Fade in from 1000 to 2000, then add a second Fade that keeps the opacity at 1 until 3000.
---

Right now the star just appears at 1000 ms. Make it **fade in** instead:

- invisible (opacity 0) at **1000 ms**,
- fully visible (opacity 1) at **2000 ms**,
- and **still visible at 3000 ms**.

::: tip
The time slider under the preview lets you check the opacity at exact moments. Drag it to 1000, 1500
and 2000 ms and compare.
:::
