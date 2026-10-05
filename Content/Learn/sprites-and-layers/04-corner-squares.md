---
title: "Exercise: into the corners"
type: exercise
minutes: 5
summary: Use origins to tuck two squares neatly into opposite corners.
exercise:
  duration: 3000
  guides: true
  widescreen: false
  starter: |
    // Two 100 × 100 squares: tuck one into the top-left corner
    // and one into the bottom-right corner of the 640 × 480 area.
    Sprite,Foreground,Centre,"sb/square.png",320,240
     F,0,0,3000,1
    Sprite,Foreground,Centre,"sb/square.png",320,240
     F,0,0,3000,1
  solution: |
    Sprite,Foreground,TopLeft,"sb/square.png",0,0
     F,0,0,3000,1
    Sprite,Foreground,BottomRight,"sb/square.png",640,480
     F,0,0,3000,1
  checks:
    - kind: count
      of: sprites
      equals: 2
      label: There are two squares
    - kind: object
      path: sb/square.png
      origin: TopLeft
      x: 0
      y: 0
      label: One square has its top-left corner at (0, 0)
    - kind: object
      path: sb/square.png
      origin: BottomRight
      x: 640
      y: 480
      label: The other has its bottom-right corner at (640, 480)
    - kind: visible
      time: 1500
      every: true
      label: Both are visible at 1500 ms
  hints:
    - With TopLeft, (x, y) is where the image's top-left corner goes, so (0, 0) puts it right in the corner.
    - BottomRight puts the image's bottom-right corner at (x, y). Which position is the bottom-right corner of the screen?
---

Both squares start in the middle of the screen. Change their **origins** and **positions** so that:

- the first square sits snugly in the **top-left** corner, and
- the second sits snugly in the **bottom-right** corner.

"Snugly" means the square's edges line up with the edges of the screen, with no gap and nothing cut off.
The preview is set to 4:3 for this exercise, so the corners are those of the 640 × 480 area.

::: tip
You could do this with the Centre origin and some arithmetic (a 100 × 100 square needs its centre at
(50, 50)), but choosing the right origin means you don't have to know the image's size at all.
:::
